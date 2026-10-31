using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CONCOST.UI.Controls;

/// <summary>
/// Lays out <see cref="ToolWindow"/> panels around a centre area: left, top, right and bottom edges,
/// each resizable by a splitter, plus auto-hide strips and a drop preview while a panel is dragged.
/// </summary>
/// <remarks>
/// <para>
/// The model follows AvalonDock: a <b>pane group</b> is a slot on one edge, and every panel docked to
/// that edge belongs to the group. Several panels in one group are <b>tabs</b>, not a stack — stacking
/// them would give each its own slice of the edge, which is not how an IDE behaves.
/// </para>
/// <para>
/// Sizes are <b>proportional</b>: a panel keeps its share of the window when the window is resized,
/// the way a <c>GridLength</c> star column does, instead of staying at a fixed pixel width.
/// </para>
/// <para>
/// Dragging a header shows <see cref="DockPreview"/> over the edge the pointer is aiming at and docks
/// the panel there on release. The target is resolved per pane with the five zones an IDE uses
/// (left / top / right / bottom / centre), not by four global bands.
/// </para>
/// <para>
/// The manager owns the geometry, so it also owns the splitter bands: they are hit-tested and dragged
/// here rather than being separate children. <see cref="DockSplitter"/> stays public for custom
/// layouts that are not a <see cref="DockingManager"/>.
/// </para>
/// </remarks>
public class DockingManager : Panel
{
    private sealed class LayoutResult
    {
        public readonly Dictionary<UIElement, Rect> Nodes = new();
        public readonly List<(Rect Rect, ToolWindow Pane, bool Vertical)> Splitters = new();
        public readonly Dictionary<DockSide, Rect> Strips = new();
        public readonly Dictionary<DockSide, Rect> TabStrips = new();
        public ToolWindow? Flyout;
        public Rect FlyoutRect;
        public Rect Center;
    }

    /// <summary>One slot on an edge: either the implicit tab group of direct panes, or a group.</summary>
    private sealed class Slot
    {
        public readonly List<ToolWindow>? Panes;
        public readonly DockPaneGroup? Group;

        public Slot(List<ToolWindow> panes) => Panes = panes;

        public Slot(DockPaneGroup group) => Group = group;

        public double Size => Group?.DockSize ?? (Panes is { Count: > 0 } ? Panes[0].DockSize : 260);

        public double MinSize => Group?.MinDockSize ?? (Panes is { Count: > 0 } ? Panes[0].MinDockSize : 120);

        public double Weight => Group is null ? 1 : Math.Max(0.0001, DockPaneGroup.GetProportion(Group));
    }

    private sealed class SplitterDrag
    {
        public ToolWindow? Pane;
        public DockSide Side;
        public double StartSize;
        public Point Origin;
        public bool Vertical;
    }

    private static readonly DockSide[] Edges =
        { DockSide.Left, DockSide.Top, DockSide.Right, DockSide.Bottom };

    private const double TabStripThickness = 27;

    private readonly AutoHidePane[] _strips = new AutoHidePane[4];
    private readonly DockTabStrip[] _tabStrips = new DockTabStrip[4];
    private readonly DockTabStrip _documentTabs;
    private readonly DockPreview _preview;
    private SplitterDrag? _drag;
    private ToolWindow? _dragPane;
    private Rect _previewRect;
    private bool _isDraggingPane;
    private Rect[] _splitterRects = Array.Empty<Rect>();
    private Size _accountedSize;
    private bool _normalizing;
    private readonly Dictionary<ToolWindow, Window> _floating = new();

    public DockingManager()
    {
        for (int i = 0; i < Edges.Length; i++)
        {
            var strip = new AutoHidePane { Side = Edges[i], Visibility = Visibility.Collapsed };
            strip.PaneInvoked += OnStripPaneInvoked;
            _strips[i] = strip;
            Children.Add(strip);

            var tabs = new DockTabStrip { TabPlacement = DockTabPlacement.Bottom, Visibility = Visibility.Collapsed };
            _tabStrips[i] = tabs;
            Children.Add(tabs);
        }

        _documentTabs = new DockTabStrip { TabPlacement = DockTabPlacement.Top, Visibility = Visibility.Collapsed };
        Children.Add(_documentTabs);

        _preview = new DockPreview();
        Children.Add(_preview);
    }

    // ---------------------------------------------------------------- properties

    public static readonly DependencyProperty SplitterThicknessProperty =
        DependencyProperty.Register(nameof(SplitterThickness), typeof(double), typeof(DockingManager),
            new FrameworkPropertyMetadata(6.0, FrameworkPropertyMetadataOptions.AffectsMeasure));

    /// <summary>Hit area of the splitter bands, in pixels. Default 6.</summary>
    public double SplitterThickness
    {
        get => (double)GetValue(SplitterThicknessProperty);
        set => SetValue(SplitterThicknessProperty, value);
    }

    public static readonly DependencyProperty AutoHideStripSizeProperty =
        DependencyProperty.Register(nameof(AutoHideStripSize), typeof(double), typeof(DockingManager),
            new FrameworkPropertyMetadata(28.0, FrameworkPropertyMetadataOptions.AffectsMeasure));

    /// <summary>Thickness of an auto-hide strip, in pixels. Default 28.</summary>
    public double AutoHideStripSize
    {
        get => (double)GetValue(AutoHideStripSizeProperty);
        set => SetValue(AutoHideStripSizeProperty, value);
    }

    public static readonly DependencyProperty ShowDropPreviewProperty =
        DependencyProperty.Register(nameof(ShowDropPreview), typeof(bool), typeof(DockingManager),
            new PropertyMetadata(true));

    /// <summary>Show the dashed hint while a panel is dragged. Default <c>true</c>.</summary>
    public bool ShowDropPreview
    {
        get => (bool)GetValue(ShowDropPreviewProperty);
        set => SetValue(ShowDropPreviewProperty, value);
    }

    public static readonly DependencyProperty SplitterBrushProperty =
        DependencyProperty.Register(nameof(SplitterBrush), typeof(Brush), typeof(DockingManager),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>
    /// Colour of the hairline drawn in each splitter band. Empty takes
    /// <c>CONCOST.Brush.Border</c> from the theme.
    /// </summary>
    public Brush? SplitterBrush
    {
        get => (Brush?)GetValue(SplitterBrushProperty);
        set => SetValue(SplitterBrushProperty, value);
    }

    public static readonly DependencyProperty IsProportionalProperty =
        DependencyProperty.Register(nameof(IsProportional), typeof(bool), typeof(DockingManager),
            new PropertyMetadata(true));

    /// <summary>
    /// Keep each pane's share of the window when the window is resized. Set to <c>false</c> to pin
    /// <see cref="ToolWindow.DockSize"/> to pixels instead. Default <c>true</c>.
    /// </summary>
    public bool IsProportional
    {
        get => (bool)GetValue(IsProportionalProperty);
        set => SetValue(IsProportionalProperty, value);
    }

    /// <summary>Raised after a panel is docked to a new edge.</summary>
    public event EventHandler<ToolWindow>? PaneDocked;

    /// <summary>Raised after a panel is removed from the layout.</summary>
    public event EventHandler<ToolWindow>? PaneClosed;

    /// <summary>The tool windows in the layout, in declaration order.</summary>
    public IEnumerable<ToolWindow> Panes => Children.OfType<ToolWindow>();

    /// <summary>True while a drag is showing the drop hint.</summary>
    public bool IsPreviewOpen => _preview.IsOpen;

    /// <summary>The panes sharing one edge, which are shown as tabs.</summary>
    public IReadOnlyList<ToolWindow> Group(DockSide side) =>
        Panes.Where(p => p.Dock == side && !p.IsAutoHidden).ToList();

    // ---------------------------------------------------------------- public API

    /// <summary>Docks a panel to an edge and brings it back from auto-hide.</summary>
    public void Dock(ToolWindow pane, DockSide side)
    {
        if (pane is null) return;

        pane.IsAutoHidden = false;
        pane.IsFlyoutOpen = false;
        pane.Dock = side;

        if (side != DockSide.Center && pane.DockSize <= 0) pane.DockSize = 260;

        // A floating panel has to lose its window first, or it would still belong to that host and the
        // insert below would throw "already the logical child of another element".
        if (_floating.TryGetValue(pane, out var window)) window.Close();

        // A pane may currently live inside a group; take it out of whichever parent holds it.
        Detach(pane);

        // A panel docked to an edge goes first on that edge, the way a newly opened tool window does,
        // and becomes the visible tab of the group it joins.
        if (side != DockSide.Center)
        {
            Children.Remove(pane);
            Children.Insert(0, pane);

            foreach (var other in Panes.Where(p => p.Dock == side))
            {
                other.SetCurrentValue(ToolWindow.IsActiveProperty, ReferenceEquals(other, pane));
            }
        }
        else if (!Children.Contains(pane))
        {
            Children.Insert(0, pane);
        }

        InvalidateMeasure();
        PaneDocked?.Invoke(this, pane);
    }

    /// <summary>Removes a panel from the layout, wherever it sits in the tree.</summary>
    public void ClosePane(ToolWindow pane)
    {
        if (pane is null) return;

        if (pane.Parent is DockPaneGroup group)
        {
            group.Children.Remove(pane);
            PaneClosed?.Invoke(this, pane);
            return;
        }

        if (!Children.Contains(pane)) return;

        Children.Remove(pane);
        InvalidateMeasure();
        PaneClosed?.Invoke(this, pane);
    }

    /// <summary>Every panel in the layout, including the ones nested inside groups.</summary>
    public IEnumerable<ToolWindow> AllPanes
    {
        get
        {
            foreach (var pane in Panes) yield return pane;

            foreach (var group in Children.OfType<DockPaneGroup>())
            {
                foreach (var pane in Walk(group)) yield return pane;
            }
        }
    }

    /// <summary>
    /// Moves a panel to another position among the tabs it shares an edge with — what a tab drag does
    /// when it is dropped between two tabs. A pane nested in a group is reordered inside that group.
    /// </summary>
    public void MovePane(ToolWindow pane, int index)
    {
        if (pane is null) return;

        if (pane.Parent is DockPaneGroup group)
        {
            group.MoveChild(pane, index);
            return;
        }

        var siblings = Panes.Where(p => p.Dock == pane.Dock).ToList();
        if (siblings.Count < 2 || !siblings.Contains(pane)) return;

        int target = Math.Max(0, Math.Min(siblings.Count - 1, index));
        var order = siblings.ToList();
        order.Remove(pane);
        order.Insert(target, pane);

        // Lift the whole group out and put it back in the new order at the same place, so the strips
        // and the preview that sit between the children are not disturbed.
        int at = siblings.Min(s => Children.IndexOf(s));
        foreach (var sibling in siblings) Children.Remove(sibling);

        for (int i = 0; i < order.Count; i++) Children.Insert(at + i, order[i]);

        InvalidateMeasure();
    }

    private static IEnumerable<ToolWindow> Walk(DockPaneGroup group)
    {
        foreach (var child in group.Children.OfType<UIElement>())
        {
            if (child is ToolWindow pane) yield return pane;
            else if (child is DockPaneGroup nested)
            {
                foreach (var inner in Walk(nested)) yield return inner;
            }
        }
    }

    /// <summary>Opens or closes the flyout of an auto-hidden panel.</summary>
    public void ToggleFlyout(ToolWindow pane)
    {
        if (pane is null || !pane.IsAutoHidden) return;

        bool open = !pane.IsFlyoutOpen;
        foreach (var other in Panes) other.IsFlyoutOpen = false;
        pane.IsFlyoutOpen = open;

        // A flyout slides over the centre, so it has to be drawn after the centre panes. Child order
        // is the z-order in a Panel, and it can only be changed here — not during arrange.
        if (open) BringToFront(pane);

        InvalidateMeasure();
    }

    /// <summary>Collapses every open flyout.</summary>
    public void CloseFlyouts()
    {
        foreach (var pane in Panes) pane.IsFlyoutOpen = false;
        InvalidateMeasure();
    }

    /// <summary>
    /// Moves a panel into a floating window of its own. Closing that window docks the panel back where
    /// it came from, which is the gesture AvalonDock gives you when a header is dragged off the layout.
    /// </summary>
    /// <remarks>
    /// Calling this for a panel that is already floating brings its window forward instead of opening a
    /// second one. Without that guard the second call reached <c>Border.Child</c> with a panel that
    /// still had the first window's host as its logical parent, and WPF threw
    /// <see cref="InvalidOperationException"/>.
    /// </remarks>
    /// <returns>The window that hosts the panel.</returns>
    public Window Float(ToolWindow pane)
    {
        if (pane is null) throw new ArgumentNullException(nameof(pane));

        if (_floating.TryGetValue(pane, out var existing))
        {
            existing.Activate();
            return existing;
        }

        var side = pane.Dock;
        double size = pane.DockSize;

        Detach(pane);

        bool canAutoHide = pane.CanAutoHide;
        pane.CanAutoHide = false;
        pane.IsAutoHidden = false;
        pane.IsFlyoutOpen = false;
        pane.ShowHeader = true;

        var host = new Border { Child = pane, Padding = new Thickness(1) };
        host.SetResourceReference(Border.BackgroundProperty, "CONCOST.Brush.Surface");
        host.SetResourceReference(Border.BorderBrushProperty, "CONCOST.Brush.Border");

        var window = new Window
        {
            Title = pane.Header?.ToString() ?? "Panel",
            Content = host,
            Width = Math.Max(240, pane.DockSize),
            Height = Math.Max(200, ActualHeight > 0 ? ActualHeight * 0.6 : 320),
            WindowStartupLocation = WindowStartupLocation.CenterScreen,
            ShowInTaskbar = false
        };

        window.SetResourceReference(Window.BackgroundProperty, "CONCOST.Brush.Surface");

        window.Closed += (_, _) =>
        {
            _floating.Remove(pane);
            host.Child = null;
            pane.CanAutoHide = canAutoHide;
            pane.DockSize = size;
            Dock(pane, side);
        };

        _floating[pane] = window;

        InvalidateMeasure();
        window.Show();
        window.Activate();
        return window;
    }

    /// <summary>True while the panel lives in a floating window.</summary>
    public bool IsFloating(ToolWindow pane) => pane is not null && _floating.ContainsKey(pane);

    /// <summary>
    /// Takes a panel out of whatever holds it — a group, this manager, or the host of a floating
    /// window — so it can be re-parented safely.
    /// </summary>
    private static void Detach(ToolWindow pane)
    {
        switch (pane.Parent)
        {
            case Panel panel:
                panel.Children.Remove(pane);
                break;
            case Decorator decorator:
                if (ReferenceEquals(decorator.Child, pane)) decorator.Child = null;
                break;
            case ContentControl content when ReferenceEquals(content.Content, pane):
                content.Content = null;
                break;
        }
    }

    private void BringToFront(ToolWindow pane)
    {
        Children.Remove(pane);
        Children.Add(pane);

        // The drop hint is drawn last so it stays above everything, a flyout included.
        Children.Remove(_preview);
        Children.Add(_preview);
    }

    // ---------------------------------------------------------------- layout persistence

    private const string LayoutHeader = "CONCOST-DOCK-1";

    /// <summary>
    /// Serialises the layout — edge, size, auto-hide and active state per panel — so a host can restore
    /// it on the next run. Panels are matched by <see cref="ToolWindow.Header"/> when it is applied.
    /// </summary>
    public string CaptureLayout()
    {
        var lines = new List<string> { LayoutHeader };

        foreach (var pane in AllPanes)
        {
            lines.Add(string.Join("|",
                pane.Dock,
                pane.DockSize.ToString(System.Globalization.CultureInfo.InvariantCulture),
                pane.IsAutoHidden,
                pane.IsActive,
                Uri.EscapeDataString(pane.Header?.ToString() ?? string.Empty)));
        }

        return string.Join(Environment.NewLine, lines);
    }

    /// <summary>
    /// Applies a layout produced by <see cref="CaptureLayout"/>. Unknown panels in the snapshot are
    /// ignored and panels missing from it keep their current state, so an old snapshot still applies.
    /// </summary>
    /// <returns><c>true</c> when at least one panel matched.</returns>
    public bool ApplyLayout(string? layout)
    {
        if (string.IsNullOrWhiteSpace(layout)) return false;

        var lines = layout!.Replace("\r\n", "\n").Split('\n');
        if (lines.Length == 0 || lines[0].Trim() != LayoutHeader) return false;

        var byHeader = new Dictionary<string, ToolWindow>(StringComparer.Ordinal);
        foreach (var pane in AllPanes)
        {
            var key = pane.Header?.ToString() ?? string.Empty;
            if (key.Length > 0) byHeader[key] = pane;
        }

        int matched = 0;
        var order = new List<ToolWindow>();

        for (int i = 1; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            if (line.Length == 0) continue;

            var parts = line.Split('|');
            if (parts.Length < 5) continue;

            var header = Uri.UnescapeDataString(parts[4]);
            if (!byHeader.TryGetValue(header, out var pane)) continue;

            if (Enum.TryParse<DockSide>(parts[0], out var side)) pane.Dock = side;

            if (double.TryParse(parts[1], System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out var size))
            {
                pane.DockSize = size;
            }

            pane.IsAutoHidden = bool.TryParse(parts[2], out var hidden) && hidden;
            pane.IsActive = bool.TryParse(parts[3], out var active) && active;
            order.Add(pane);
            matched++;
        }

        if (matched == 0) return false;

        // Restore declaration order too, so tabs come back in the order they were saved. Only the
        // top-level children are reordered; a nested pane keeps its place inside its group.
        int at = 0;
        foreach (var pane in order)
        {
            if (pane.Parent is DockPaneGroup) continue;

            Children.Remove(pane);
            Children.Insert(at++, pane);
        }

        InvalidateMeasure();
        return true;
    }

    // ---------------------------------------------------------------- child hooks

    protected override void OnVisualChildrenChanged(DependencyObject visualAdded, DependencyObject visualRemoved)
    {
        base.OnVisualChildrenChanged(visualAdded, visualRemoved);

        if (visualRemoved is ToolWindow removed)
        {
            removed.DragStarted -= OnPaneDragStarted;
            removed.DragMoved -= OnPaneDragMoved;
            removed.DragCompleted -= OnPaneDragCompleted;
            removed.Closed -= OnPaneClosed;
            removed.Activated -= OnPaneActivated;
            removed.AutoHideChanged -= OnDirectAutoHideChanged;
        }

        if (visualRemoved is DockPaneGroup removedGroup) Detach(removedGroup);

        if (visualAdded is ToolWindow added)
        {
            added.DragStarted += OnPaneDragStarted;
            added.DragMoved += OnPaneDragMoved;
            added.DragCompleted += OnPaneDragCompleted;
            added.Closed += OnPaneClosed;
            added.Activated += OnPaneActivated;
            added.AutoHideChanged += OnDirectAutoHideChanged;
        }

        if (visualAdded is DockPaneGroup addedGroup) Attach(addedGroup);

        InvalidateMeasure();
    }

    /// <summary>
    /// Groups re-raise their children's events, so a pane nested three levels deep drags, closes and
    /// activates exactly like one docked straight to an edge.
    /// </summary>
    private void Attach(DockPaneGroup group)
    {
        group.PaneDragStarted += OnNestedDragStarted;
        group.PaneDragMoved += OnNestedDragMoved;
        group.PaneDragCompleted += OnNestedDragCompleted;
        group.PaneClosed += OnNestedClosed;
        group.PaneAutoHideChanged += OnPaneAutoHideChanged;
    }

    private void Detach(DockPaneGroup group)
    {
        group.PaneDragStarted -= OnNestedDragStarted;
        group.PaneDragMoved -= OnNestedDragMoved;
        group.PaneDragCompleted -= OnNestedDragCompleted;
        group.PaneClosed -= OnNestedClosed;
        group.PaneAutoHideChanged -= OnPaneAutoHideChanged;
    }

    private void OnNestedDragStarted(object? sender, ToolWindow pane)
    {
        _dragPane = pane;
        _isDraggingPane = true;
    }

    private void OnNestedDragMoved(object? sender, (ToolWindow Pane, Point Point) e)
    {
        if (!_isDraggingPane || !ShowDropPreview) return;

        var side = ResolveDropSide(e.Point);
        var size = ResolveSize(new Size(ActualWidth, ActualHeight));

        _preview.Side = side;
        _previewRect = DropRect(side, size, e.Pane);
        _preview.IsOpen = true;
        InvalidateArrange();
    }

    private void OnNestedDragCompleted(object? sender, (ToolWindow Pane, Point Point) e)
    {
        _isDraggingPane = false;
        _preview.IsOpen = false;
        _dragPane = null;

        if (IsOutside(e.Point))
        {
            Float(e.Pane);
            return;
        }

        Dock(e.Pane, ResolveDropSide(e.Point));
    }

    private void OnNestedClosed(object? sender, ToolWindow pane) => ClosePane(pane);

    /// <summary>
    /// Auto-hide lives on the edges, so a panel nested inside a group cannot be hidden where it stands.
    /// Pull it up to the manager the moment it asks to be hidden; pinning it back then docks it to the
    /// edge it came from, which is what an IDE does with a panel un-pinned from a split group.
    /// </summary>
    private void OnDirectAutoHideChanged(object? sender, EventArgs e)
    {
        if (sender is ToolWindow pane) PullOutOfGroup(pane);
    }

    private void OnPaneAutoHideChanged(object? sender, ToolWindow pane) => PullOutOfGroup(pane);

    private void PullOutOfGroup(ToolWindow pane)
    {
        if (!pane.IsAutoHidden) return;
        if (pane.Parent is not DockPaneGroup group) return;

        group.Children.Remove(pane);

        if (!Children.Contains(pane)) Children.Insert(0, pane);
        InvalidateMeasure();
    }

    private void OnPaneActivated(object? sender, EventArgs e)
    {
        if (sender is ToolWindow pane) Activate(pane);
    }

    private void OnPaneClosed(object? sender, EventArgs e)
    {
        if (sender is ToolWindow pane) ClosePane(pane);
    }

    private void OnPaneDragStarted(object? sender, EventArgs e)
    {
        _dragPane = sender as ToolWindow;
        _isDraggingPane = true;
    }

    private void OnPaneDragMoved(object? sender, Point point)
    {
        if (!_isDraggingPane || _dragPane is null || !ShowDropPreview) return;

        var side = ResolveDropSide(point);
        var size = ResolveSize(new Size(ActualWidth, ActualHeight));

        _preview.Side = side;
        _previewRect = DropRect(side, size, _dragPane);
        _preview.IsOpen = true;
        InvalidateArrange();
    }

    private void OnPaneDragCompleted(object? sender, Point point)
    {
        _isDraggingPane = false;
        _preview.IsOpen = false;

        var pane = _dragPane ?? sender as ToolWindow;
        _dragPane = null;

        if (pane is null) return;

        // Dragged clear of the manager: tear the panel out into a window instead of docking it.
        if (IsOutside(point))
        {
            Float(pane);
            return;
        }

        Dock(pane, ResolveDropSide(point));
    }

    /// <summary>
    /// Docks a panel at a point in the manager's coordinates, or tears it out into a window when the
    /// point is outside. This is the drop half of the drag gesture, exposed so a custom drag surface
    /// (a tab strip, a toolbox list) can reuse it.
    /// </summary>
    public void DockAt(ToolWindow pane, Point point)
    {
        if (pane is null) return;

        _preview.IsOpen = false;

        if (IsOutside(point))
        {
            Float(pane);
            return;
        }

        Dock(pane, ResolveDropSide(point));
    }

    /// <summary>Shows the drop hint for a drag in progress. <paramref name="point"/> is in manager coordinates.</summary>
    public void ShowDropHint(ToolWindow pane, Point point)
    {
        if (!ShowDropPreview || pane is null) return;

        var size = ResolveSize(new Size(ActualWidth, ActualHeight));

        _preview.Side = ResolveDropSide(point);
        _previewRect = DropRect(_preview.Side, size, pane);
        _preview.IsOpen = !IsOutside(point);
        InvalidateArrange();
    }

    /// <summary>Hides the drop hint.</summary>
    public void EndDropPreview()
    {
        if (!_preview.IsOpen) return;

        _preview.IsOpen = false;
        InvalidateArrange();
    }

    private bool IsOutside(Point point) =>
        point.X < 0 || point.Y < 0 || point.X > ActualWidth || point.Y > ActualHeight;

    /// <summary>
    /// The five-zone answer: which edge of the pane under the pointer the user is aiming at. Falls back
    /// to the manager's own edges when the pointer is over empty space.
    /// </summary>
    private DockSide ResolveDropSide(Point point)
    {
        var size = ResolveSize(new Size(ActualWidth, ActualHeight));
        var layout = ComputeLayout(size);

        foreach (var pair in layout.Nodes)
        {
            if (pair.Key is not ToolWindow) continue;

            var rect = pair.Value;
            if (rect.Width <= 0 || rect.Height <= 0 || !rect.Contains(point)) continue;

            double bandX = rect.Width * 0.25;
            double bandY = rect.Height * 0.25;

            if (point.X < rect.X + bandX) return DockSide.Left;
            if (point.X > rect.Right - bandX) return DockSide.Right;
            if (point.Y < rect.Y + bandY) return DockSide.Top;
            if (point.Y > rect.Bottom - bandY) return DockSide.Bottom;

            return DockSide.Center;
        }

        double w = Math.Max(1, size.Width);
        double h = Math.Max(1, size.Height);
        double edgeX = w * 0.18;
        double edgeY = h * 0.18;

        if (point.X < edgeX) return DockSide.Left;
        if (point.X > w - edgeX) return DockSide.Right;
        if (point.Y < edgeY) return DockSide.Top;
        if (point.Y > h - edgeY) return DockSide.Bottom;

        return DockSide.Center;
    }

    /// <summary>Where the dragged pane will actually land, so the hint never lies.</summary>
    private Rect DropRect(DockSide side, Size size, ToolWindow pane)
    {
        if (side == DockSide.Center) return new Rect(0, 0, size.Width, size.Height);

        double take = Math.Min(Math.Max(pane.MinDockSize, pane.DockSize),
            Math.Max(0, size.Width - SplitterThickness));
        double takeY = Math.Min(Math.Max(pane.MinDockSize, pane.DockSize),
            Math.Max(0, size.Height - SplitterThickness));

        return side switch
        {
            DockSide.Left => new Rect(0, 0, take, size.Height),
            DockSide.Right => new Rect(size.Width - take, 0, take, size.Height),
            DockSide.Top => new Rect(0, 0, size.Width, takeY),
            _ => new Rect(0, size.Height - takeY, size.Width, takeY)
        };
    }

    private void OnStripPaneInvoked(object? sender, ToolWindow pane) => ToggleFlyout(pane);

    // ---------------------------------------------------------------- layout

    private Size ResolveSize(Size available)
    {
        double width = double.IsInfinity(available.Width) ? (ActualWidth > 0 ? ActualWidth : 800) : available.Width;
        double height = double.IsInfinity(available.Height) ? (ActualHeight > 0 ? ActualHeight : 600) : available.Height;
        return new Size(width, height);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var size = ResolveSize(availableSize);

        NormalizeGroups();
        ScaleTo(size);

        var layout = ComputeLayout(size);

        foreach (var pair in layout.Nodes)
        {
            pair.Key.Measure(new Size(Math.Max(0, pair.Value.Width), Math.Max(0, pair.Value.Height)));
        }

        foreach (var strip in _strips)
        {
            if (strip.Visibility == Visibility.Visible) strip.Measure(new Size(size.Width, size.Height));
        }

        foreach (var tabs in _tabStrips) tabs.Measure(new Size(size.Width, TabStripThickness));
        _documentTabs.Measure(new Size(size.Width, TabStripThickness));

        _preview.Measure(size);

        return size;
    }

    /// <summary>
    /// Keeps every pane's share of the window across a resize. Mutating a dependency property during
    /// measure would normally re-enter, so the size we have accounted for is recorded here rather than
    /// in arrange — otherwise the second pass would scale again and the panes would run away.
    /// </summary>
    private void ScaleTo(Size size)
    {
        if (!IsProportional || size.Width <= 0 || size.Height <= 0) return;

        if (_accountedSize.Width > 1 && _accountedSize.Height > 1)
        {
            double fx = size.Width / _accountedSize.Width;
            double fy = size.Height / _accountedSize.Height;

            if (Math.Abs(fx - 1) > 0.001 || Math.Abs(fy - 1) > 0.001)
            {
                foreach (var pane in Panes)
                {
                    if (pane.IsAutoHidden || pane.Dock == DockSide.Center) continue;

                    double factor = pane.Dock is DockSide.Left or DockSide.Right ? fx : fy;
                    if (Math.Abs(factor - 1) <= 0.001) continue;

                    double next = Math.Max(pane.MinDockSize, pane.DockSize * factor);
                    pane.SetCurrentValue(ToolWindow.DockSizeProperty, next);
                }
            }
        }

        _accountedSize = size;
    }

    /// <summary>
    /// Makes each edge group coherent before it is laid out: one active tab, a shared size, and a
    /// header only when the pane is alone (a group of two or more is labelled by its tab strip).
    /// </summary>
    private void NormalizeGroups()
    {
        if (_normalizing) return;

        _normalizing = true;
        try
        {
            foreach (var side in new[] { DockSide.Left, DockSide.Top, DockSide.Right, DockSide.Bottom, DockSide.Center })
            {
                var group = Panes.Where(p => p.Dock == side && !p.IsAutoHidden).ToList();
                if (group.Count == 0) continue;

                if (group.Count > 1)
                {
                    var active = group.FirstOrDefault(p => p.IsActive) ?? group[0];

                    foreach (var pane in group)
                    {
                        bool isActive = ReferenceEquals(pane, active);
                        if (pane.IsActive != isActive)
                            pane.SetCurrentValue(ToolWindow.IsActiveProperty, isActive);

                        if (pane.ShowHeader)
                            pane.SetCurrentValue(ToolWindow.ShowHeaderProperty, false);

                        if (side != DockSide.Center && Math.Abs(pane.DockSize - active.DockSize) > 0.5)
                            pane.SetCurrentValue(ToolWindow.DockSizeProperty, active.DockSize);
                    }
                }
                else
                {
                    var pane = group[0];
                    if (!pane.ShowHeader && !pane.IsAutoHidden)
                        pane.SetCurrentValue(ToolWindow.ShowHeaderProperty, true);
                }
            }
        }
        finally
        {
            _normalizing = false;
        }
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var layout = ComputeLayout(finalSize);

        foreach (var child in Children.OfType<UIElement>())
        {
            if (child is AutoHidePane || child is DockTabStrip || ReferenceEquals(child, _preview)) continue;

            bool visible = layout.Nodes.ContainsKey(child);
            child.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;

            if (visible) child.Arrange(layout.Nodes[child]);
        }

        for (int i = 0; i < _strips.Length; i++)
        {
            var side = Edges[i];
            var strip = _strips[i];

            if (layout.Strips.TryGetValue(side, out var rect))
            {
                strip.Visibility = Visibility.Visible;
                strip.Arrange(rect);
            }
            else
            {
                strip.Visibility = Visibility.Collapsed;
            }

            var tabs = _tabStrips[i];
            if (layout.TabStrips.TryGetValue(side, out var tabRect))
            {
                tabs.Visibility = Visibility.Visible;
                tabs.Arrange(tabRect);
            }
            else
            {
                tabs.Visibility = Visibility.Collapsed;
            }
        }

        if (layout.TabStrips.TryGetValue(DockSide.Center, out var documentTabs))
        {
            _documentTabs.Visibility = Visibility.Visible;
            _documentTabs.Arrange(documentTabs);
        }
        else
        {
            _documentTabs.Visibility = Visibility.Collapsed;
        }

        _preview.Arrange(_preview.IsOpen ? _previewRect : new Rect(0, 0, 0, 0));

        var bands = new Rect[layout.Splitters.Count];
        for (int i = 0; i < bands.Length; i++) bands[i] = layout.Splitters[i].Rect;

        if (!BandsEqual(bands, _splitterRects))
        {
            _splitterRects = bands;
            InvalidateVisual();
        }

        return finalSize;
    }

    private static bool BandsEqual(Rect[] a, Rect[] b)
    {
        if (a.Length != b.Length) return false;

        for (int i = 0; i < a.Length; i++)
        {
            if (a[i] != b[i]) return false;
        }

        return true;
    }

    /// <summary>
    /// Answers hit tests for the splitter bands. A <see cref="Panel"/> paints nothing, so without this
    /// the band is empty space: mouse events pass straight through to whatever is behind and neither
    /// the resize cursor nor the drag ever starts. This is deliberately independent of
    /// <see cref="OnRender"/> so the band works before the first render pass as well.
    /// </summary>
    protected override HitTestResult? HitTestCore(PointHitTestParameters hitTestParameters)
    {
        var point = hitTestParameters.HitPoint;

        foreach (var band in _splitterRects)
        {
            if (band.Contains(point)) return new PointHitTestResult(this, point);
        }

        return base.HitTestCore(hitTestParameters);
    }

    /// <summary>
    /// Paints the splitter bands. The transparent fill makes the band block clicks aimed at anything
    /// behind it, and the hairline in the middle is what tells the user where to grab.
    /// </summary>
    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);

        if (_splitterRects.Length == 0) return;

        var line = SplitterBrush ?? TryFindResource("CONCOST.Brush.Border") as Brush ?? Brushes.Gray;

        foreach (var band in _splitterRects)
        {
            if (band.Width <= 0 || band.Height <= 0) continue;

            dc.DrawRectangle(Brushes.Transparent, null, band);

            var hairline = band.Width <= band.Height
                ? new Rect(band.X + Math.Floor(band.Width / 2), band.Y, 1, band.Height)
                : new Rect(band.X, band.Y + Math.Floor(band.Height / 2), band.Width, 1);

            dc.DrawRectangle(line, null, hairline);
        }
    }

    private LayoutResult ComputeLayout(Size size)
    {
        var result = new LayoutResult();

        double width = Math.Max(0, size.Width);
        double height = Math.Max(0, size.Height);
        if (width <= 0 || height <= 0) return result;

        var panes = Panes.ToList();
        double stripSize = Math.Max(0, AutoHideStripSize);
        double splitter = Math.Max(0, SplitterThickness);

        var rect = new Rect(0, 0, width, height);

        foreach (var side in Edges)
        {
            var hidden = panes.Where(p => p.Dock == side && p.IsAutoHidden).ToList();
            if (hidden.Count == 0) continue;

            double take = Math.Min(stripSize, side is DockSide.Left or DockSide.Right ? rect.Width : rect.Height);
            result.Strips[side] = Take(ref rect, side, take);

            var strip = StripFor(side);
            if (strip is not null) strip.Panes = hidden;
        }

        foreach (var side in Edges)
        {
            if (result.Strips.ContainsKey(side)) continue;

            var strip = StripFor(side);
            if (strip is not null) strip.Panes = null;
        }

        foreach (var side in Edges)
        {
            var slots = BuildSlots(side, panes);
            if (slots.Count == 0) continue;

            bool vertical = side is DockSide.Left or DockSide.Right;
            double max = vertical ? rect.Width : rect.Height;

            double take = Math.Min(Math.Max(slots[0].MinSize, slots[0].Size), Math.Max(0, max - splitter));
            if (take <= 0) continue;

            var edgeRect = Take(ref rect, side, take);
            LayoutSlots(edgeRect, side, slots, result);

            var anchor = slots[0].Panes?[0];
            if (anchor is not null) result.Splitters.Add((Take(ref rect, side, splitter), anchor, vertical));
            else Take(ref rect, side, splitter);
        }

        result.Center = rect;

        var centreSlots = BuildSlots(DockSide.Center, panes);
        if (centreSlots.Count > 0) LayoutSlots(rect, DockSide.Center, centreSlots, result);

        // The flyout slides over the centre from its own edge.
        var flyout = panes.FirstOrDefault(p => p.IsAutoHidden && p.IsFlyoutOpen);
        if (flyout is not null)
        {
            double flyoutSize = Math.Max(flyout.MinDockSize, flyout.DockSize);
            var centreRect = result.Center;

            var flyoutRect = flyout.Dock switch
            {
                DockSide.Left => new Rect(centreRect.X, centreRect.Y, Math.Min(flyoutSize, centreRect.Width), centreRect.Height),
                DockSide.Right => new Rect(centreRect.Right - Math.Min(flyoutSize, centreRect.Width), centreRect.Y, Math.Min(flyoutSize, centreRect.Width), centreRect.Height),
                DockSide.Top => new Rect(centreRect.X, centreRect.Y, centreRect.Width, Math.Min(flyoutSize, centreRect.Height)),
                DockSide.Bottom => new Rect(centreRect.X, centreRect.Bottom - Math.Min(flyoutSize, centreRect.Height), centreRect.Width, Math.Min(flyoutSize, centreRect.Height)),
                _ => centreRect
            };

            result.Nodes[flyout] = flyoutRect;
            result.Flyout = flyout;
            result.FlyoutRect = flyoutRect;
        }

        return result;
    }

    /// <summary>
    /// The slots an edge holds, in declaration order: the panes docked straight to the edge form one
    /// implicit tab group, and every <see cref="DockPaneGroup"/> on that edge is a slot of its own.
    /// </summary>
    private List<Slot> BuildSlots(DockSide side, List<ToolWindow> panes)
    {
        var slots = new List<Slot>();
        List<ToolWindow>? direct = null;

        foreach (var child in Children.OfType<UIElement>())
        {
            if (child is ToolWindow pane)
            {
                if (pane.Dock != side || pane.IsAutoHidden) continue;

                direct ??= new List<ToolWindow>();
                direct.Add(pane);
            }
            else if (child is DockPaneGroup group && group.Dock == side)
            {
                if (direct is not null)
                {
                    slots.Add(new Slot(direct));
                    direct = null;
                }

                slots.Add(new Slot(group));
            }
        }

        if (direct is not null) slots.Add(new Slot(direct));

        return slots;
    }

    private void LayoutSlots(Rect rect, DockSide side, List<Slot> slots, LayoutResult result)
    {
        if (slots.Count == 1)
        {
            LayoutSlot(rect, side, slots[0], result);
            return;
        }

        // More than one slot on an edge: they share the edge's length by proportion.
        bool vertical = side is DockSide.Left or DockSide.Right;
        double total = vertical ? rect.Height : rect.Width;
        double sum = slots.Sum(s => s.Weight);
        double offset = vertical ? rect.Y : rect.X;

        foreach (var slot in slots)
        {
            double size = total * slot.Weight / sum;
            var slotRect = vertical
                ? new Rect(rect.X, offset, rect.Width, size)
                : new Rect(offset, rect.Y, size, rect.Height);

            LayoutSlot(slotRect, side, slot, result);
            offset += size;
        }
    }

    private void LayoutSlot(Rect rect, DockSide side, Slot slot, LayoutResult result)
    {
        if (slot.Group is not null)
        {
            result.Nodes[slot.Group] = rect;
            return;
        }

        var panes = slot.Panes;
        if (panes is null || panes.Count == 0) return;

        if (panes.Count == 1)
        {
            result.Nodes[panes[0]] = rect;
            return;
        }

        var active = panes.FirstOrDefault(p => p.IsActive) ?? panes[0];
        result.Nodes[active] = CutTabs(rect, side, panes, result);
    }

    /// <summary>
    /// Reserves the tab strip band at one edge of a pane group and returns the content rect. A group
    /// with a single pane needs no strip: that pane's own header already names it.
    /// </summary>
    private Rect CutTabs(Rect groupRect, DockSide side, List<ToolWindow> group, LayoutResult result)
    {
        if (group.Count < 2 || groupRect.Height <= TabStripThickness * 2) return groupRect;

        bool top = side == DockSide.Center;
        var tabRect = top
            ? new Rect(groupRect.X, groupRect.Y, groupRect.Width, TabStripThickness)
            : new Rect(groupRect.X, groupRect.Bottom - TabStripThickness, groupRect.Width, TabStripThickness);

        result.TabStrips[side] = tabRect;

        var tabs = side == DockSide.Center ? _documentTabs : TabStripFor(side);
        if (tabs is not null)
        {
            tabs.TabPlacement = top ? DockTabPlacement.Top : DockTabPlacement.Bottom;
            tabs.Panes = group;
        }

        return top
            ? new Rect(groupRect.X, groupRect.Y + TabStripThickness, groupRect.Width, groupRect.Height - TabStripThickness)
            : new Rect(groupRect.X, groupRect.Y, groupRect.Width, groupRect.Height - TabStripThickness);
    }

    private static Rect Take(ref Rect rect, DockSide side, double amount)
    {
        switch (side)
        {
            case DockSide.Left:
            {
                var taken = new Rect(rect.X, rect.Y, amount, rect.Height);
                rect = new Rect(rect.X + amount, rect.Y, Math.Max(0, rect.Width - amount), rect.Height);
                return taken;
            }
            case DockSide.Right:
            {
                var taken = new Rect(rect.Right - amount, rect.Y, amount, rect.Height);
                rect = new Rect(rect.X, rect.Y, Math.Max(0, rect.Width - amount), rect.Height);
                return taken;
            }
            case DockSide.Top:
            {
                var taken = new Rect(rect.X, rect.Y, rect.Width, amount);
                rect = new Rect(rect.X, rect.Y + amount, rect.Width, Math.Max(0, rect.Height - amount));
                return taken;
            }
            case DockSide.Bottom:
            {
                var taken = new Rect(rect.X, rect.Bottom - amount, rect.Width, amount);
                rect = new Rect(rect.X, rect.Y, rect.Width, Math.Max(0, rect.Height - amount));
                return taken;
            }
            default:
                return rect;
        }
    }

    private AutoHidePane? StripFor(DockSide side)
    {
        for (int i = 0; i < _strips.Length; i++)
        {
            if (_strips[i].Side == side) return _strips[i];
        }

        return null;
    }

    private DockTabStrip? TabStripFor(DockSide side)
    {
        for (int i = 0; i < Edges.Length; i++)
        {
            if (Edges[i] == side) return _tabStrips[i];
        }

        return null;
    }

    // ---------------------------------------------------------------- splitter drag

    /// <remarks>
    /// This hooks <see cref="UIElement.MouseDownEvent"/>, which bubbles, rather than
    /// <c>MouseLeftButtonDown</c>: the latter is a direct event raised on the element under the pointer,
    /// so a click on a pane — or on a pane's content — would never reach the manager. The splitter band
    /// belongs to the manager and has no children, which is why the first version appeared to work in
    /// isolation and did nothing in a real layout.
    /// </remarks>
    protected override void OnMouseDown(MouseButtonEventArgs e)
    {
        base.OnMouseDown(e);

        if (e.ChangedButton != MouseButton.Left) return;

        Activate(PaneAt(e.OriginalSource as DependencyObject));

        var point = e.GetPosition(this);
        var layout = ComputeLayout(ResolveSize(new Size(ActualWidth, ActualHeight)));

        foreach (var splitter in layout.Splitters)
        {
            if (!splitter.Rect.Contains(point)) continue;

            _drag = new SplitterDrag
            {
                Pane = splitter.Pane,
                Side = splitter.Pane.Dock,
                StartSize = splitter.Pane.DockSize,
                Origin = point,
                Vertical = splitter.Vertical
            };

            CaptureMouse();
            Cursor = splitter.Vertical ? Cursors.SizeWE : Cursors.SizeNS;
            e.Handled = true;
            return;
        }
    }

    private static ToolWindow? PaneAt(DependencyObject? source)
    {
        while (source is not null)
        {
            if (source is ToolWindow pane) return pane;
            source = VisualTreeHelper.GetParent(source);
        }

        return null;
    }

    /// <summary>Marks one panel as the active tab of its group.</summary>
    private void Activate(ToolWindow? pane)
    {
        if (pane is null) return;

        foreach (var other in Panes.Where(p => p.Dock == pane.Dock))
        {
            bool active = ReferenceEquals(other, pane);
            if (other.IsActive != active) other.SetCurrentValue(ToolWindow.IsActiveProperty, active);
        }
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);

        var point = e.GetPosition(this);

        if (_drag is null)
        {
            var layout = ComputeLayout(ResolveSize(new Size(ActualWidth, ActualHeight)));
            var hit = layout.Splitters.FirstOrDefault(s => s.Rect.Contains(point));
            Cursor = hit.Pane is null ? Cursors.Arrow : hit.Vertical ? Cursors.SizeWE : Cursors.SizeNS;
            return;
        }

        if (e.LeftButton != MouseButtonState.Pressed) return;
        if (_drag.Pane is null) return;

        double delta = _drag.Vertical ? point.X - _drag.Origin.X : point.Y - _drag.Origin.Y;

        // A right or bottom panel grows as the splitter moves towards the centre, so the delta flips.
        if (_drag.Side is DockSide.Right or DockSide.Bottom) delta = -delta;

        double next = Math.Max(_drag.Pane.MinDockSize, _drag.StartSize + delta);

        // Panes sharing an edge are tabs of one group, so they all take the new size.
        foreach (var pane in Panes.Where(p => p.Dock == _drag.Side && !p.IsAutoHidden))
        {
            pane.SetCurrentValue(ToolWindow.DockSizeProperty, next);
        }
    }

    /// <remarks>Bubbling, for the same reason as <see cref="OnMouseDown"/>.</remarks>
    protected override void OnMouseUp(MouseButtonEventArgs e)
    {
        base.OnMouseUp(e);

        if (e.ChangedButton != MouseButton.Left || _drag is null) return;

        _drag = null;
        Cursor = Cursors.Arrow;
        ReleaseMouseCapture();
    }
}
