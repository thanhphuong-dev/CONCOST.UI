using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CONCOST.UI.Controls;

/// <summary>How a <see cref="DockPaneGroup"/> arranges its children.</summary>
public enum DockGroupMode
{
    /// <summary>Children are split along <see cref="DockPaneGroup.Orientation"/>, each with its own slot.</summary>
    Split,

    /// <summary>Children are tabs of one slot — only the active one is visible.</summary>
    Tabs
}

/// <summary>
/// A node of the docking layout tree: it either splits its area between its children or shows them as
/// tabs. Groups nest, so a layout can be as deep as an IDE's.
/// </summary>
/// <remarks>
/// <para>
/// This is the piece the first version of the docking manager was missing. With only four edge slots a
/// layout could never express "the left edge, split into two rows" or "a bottom panel split in half" —
/// the shapes an IDE produces all the time. A group fixes that: put one on an edge and it owns that
/// edge, then split it however you like.
/// </para>
/// <para>
/// The group draws and drags its own splitters. That keeps the recursion honest: the manager knows
/// about edges, the group knows about the boundaries inside it, and neither has to walk the other's
/// children.
/// </para>
/// </remarks>
public class DockPaneGroup : Panel
{
    private const double SplitterThickness = 6;
    private const double TabStripThickness = 27;

    private DockTabStrip? _tabs;
    private readonly List<(Rect Rect, int Index, bool Vertical)> _bands = new();
    private Rect[] _bandRects = Array.Empty<Rect>();
    private (int Index, double Start, Point Origin, bool Vertical)? _drag;

    // ---------------------------------------------------------------- properties

    public static readonly DependencyProperty OrientationProperty =
        DependencyProperty.Register(nameof(Orientation), typeof(Orientation), typeof(DockPaneGroup),
            new FrameworkPropertyMetadata(Orientation.Vertical, FrameworkPropertyMetadataOptions.AffectsMeasure));

    /// <summary>Split direction when <see cref="Mode"/> is <see cref="DockGroupMode.Split"/>.</summary>
    public Orientation Orientation
    {
        get => (Orientation)GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    public static readonly DependencyProperty ModeProperty =
        DependencyProperty.Register(nameof(Mode), typeof(DockGroupMode), typeof(DockPaneGroup),
            new FrameworkPropertyMetadata(DockGroupMode.Split, FrameworkPropertyMetadataOptions.AffectsMeasure,
                OnModeChanged));

    /// <summary>Split into slots, or show the children as tabs. Default <see cref="DockGroupMode.Split"/>.</summary>
    public DockGroupMode Mode
    {
        get => (DockGroupMode)GetValue(ModeProperty);
        set => SetValue(ModeProperty, value);
    }

    public static readonly DependencyProperty DockProperty =
        DependencyProperty.Register(nameof(Dock), typeof(DockSide), typeof(DockPaneGroup),
            new FrameworkPropertyMetadata(DockSide.Left, FrameworkPropertyMetadataOptions.AffectsParentMeasure));

    /// <summary>Edge this group occupies when it is a direct child of a <see cref="DockingManager"/>.</summary>
    public DockSide Dock
    {
        get => (DockSide)GetValue(DockProperty);
        set => SetValue(DockProperty, value);
    }

    public static readonly DependencyProperty DockSizeProperty =
        DependencyProperty.Register(nameof(DockSize), typeof(double), typeof(DockPaneGroup),
            new FrameworkPropertyMetadata(260.0, FrameworkPropertyMetadataOptions.AffectsParentMeasure));

    /// <summary>Thickness of the edge this group occupies. Ignored when the group is nested.</summary>
    public double DockSize
    {
        get => (double)GetValue(DockSizeProperty);
        set => SetValue(DockSizeProperty, value);
    }

    public static readonly DependencyProperty MinDockSizeProperty =
        DependencyProperty.Register(nameof(MinDockSize), typeof(double), typeof(DockPaneGroup),
            new PropertyMetadata(120.0));

    /// <summary>Smallest thickness the edge may shrink to. Default 120.</summary>
    public double MinDockSize
    {
        get => (double)GetValue(MinDockSizeProperty);
        set => SetValue(MinDockSizeProperty, value);
    }

    public static readonly DependencyProperty ProportionProperty =
        DependencyProperty.RegisterAttached("Proportion", typeof(double), typeof(DockPaneGroup),
            new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.AffectsParentMeasure));

    /// <summary>Share of the parent group's space this child takes. Default 1, so children start equal.</summary>
    public static double GetProportion(DependencyObject element) => (double)element.GetValue(ProportionProperty);

    public static void SetProportion(DependencyObject element, double value) =>
        element.SetValue(ProportionProperty, value);

    // ---------------------------------------------------------------- children

    /// <summary>The children that take part in the layout, in declaration order.</summary>
    public IEnumerable<UIElement> Nodes =>
        Children.OfType<UIElement>().Where(c => !ReferenceEquals(c, _tabs));

    private static void OnModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DockPaneGroup group) group.SyncTabStrip();
    }

    protected override void OnVisualChildrenChanged(DependencyObject visualAdded, DependencyObject visualRemoved)
    {
        base.OnVisualChildrenChanged(visualAdded, visualRemoved);

        if (visualRemoved is ToolWindow removed)
        {
            removed.DragStarted -= OnChildDragStarted;
            removed.DragMoved -= OnChildDragMoved;
            removed.DragCompleted -= OnChildDragCompleted;
            removed.Closed -= OnChildClosed;
            removed.Activated -= OnChildActivated;
            removed.AutoHideChanged -= OnChildAutoHideChanged;
        }

        if (visualAdded is ToolWindow added)
        {
            added.DragStarted += OnChildDragStarted;
            added.DragMoved += OnChildDragMoved;
            added.DragCompleted += OnChildDragCompleted;
            added.Closed += OnChildClosed;
            added.Activated += OnChildActivated;
            added.AutoHideChanged += OnChildAutoHideChanged;
        }

        // A nested group's events have to travel up as well, or a pane three levels deep would be
        // invisible to the manager: its drag would not redock it and its pin would not hide it.
        if (visualRemoved is DockPaneGroup removedGroup) DetachNested(removedGroup);
        if (visualAdded is DockPaneGroup addedGroup) AttachNested(addedGroup);

        SyncTabStrip();
        InvalidateMeasure();
    }

    private void AttachNested(DockPaneGroup nested)
    {
        nested.PaneDragStarted += OnNestedDragStarted;
        nested.PaneDragMoved += OnNestedDragMoved;
        nested.PaneDragCompleted += OnNestedDragCompleted;
        nested.PaneClosed += OnNestedClosed;
        nested.PaneAutoHideChanged += OnNestedAutoHideChanged;
    }

    private void DetachNested(DockPaneGroup nested)
    {
        nested.PaneDragStarted -= OnNestedDragStarted;
        nested.PaneDragMoved -= OnNestedDragMoved;
        nested.PaneDragCompleted -= OnNestedDragCompleted;
        nested.PaneClosed -= OnNestedClosed;
        nested.PaneAutoHideChanged -= OnNestedAutoHideChanged;
    }

    private void OnNestedDragStarted(object? sender, ToolWindow pane) => PaneDragStarted?.Invoke(this, pane);

    private void OnNestedDragMoved(object? sender, (ToolWindow Pane, Point Point) e) => PaneDragMoved?.Invoke(this, e);

    private void OnNestedDragCompleted(object? sender, (ToolWindow Pane, Point Point) e) => PaneDragCompleted?.Invoke(this, e);

    private void OnNestedClosed(object? sender, ToolWindow pane) => PaneClosed?.Invoke(this, pane);

    private void OnNestedAutoHideChanged(object? sender, ToolWindow pane) => PaneAutoHideChanged?.Invoke(this, pane);

    // A group re-raises its children's events so the manager can handle a pane nested three levels deep
    // exactly like one docked to an edge.

    /// <summary>Raised when a nested pane's header drag begins.</summary>
    public event EventHandler<ToolWindow>? PaneDragStarted;

    /// <summary>Raised while a nested pane's header is dragged, in manager coordinates.</summary>
    public event EventHandler<(ToolWindow Pane, Point Point)>? PaneDragMoved;

    /// <summary>Raised when a nested pane's header drag ends, in manager coordinates.</summary>
    public event EventHandler<(ToolWindow Pane, Point Point)>? PaneDragCompleted;

    /// <summary>Raised when a nested pane closes itself.</summary>
    public event EventHandler<ToolWindow>? PaneClosed;

    /// <summary>Raised when a nested pane flips its auto-hide state.</summary>
    public event EventHandler<ToolWindow>? PaneAutoHideChanged;

    private void OnChildAutoHideChanged(object? sender, EventArgs e)
    {
        if (sender is ToolWindow pane) PaneAutoHideChanged?.Invoke(this, pane);
    }

    private void OnChildDragStarted(object? sender, EventArgs e)
    {
        if (sender is ToolWindow pane) PaneDragStarted?.Invoke(this, pane);
    }

    private void OnChildDragMoved(object? sender, Point point)
    {
        if (sender is ToolWindow pane) PaneDragMoved?.Invoke(this, (pane, point));
    }

    private void OnChildDragCompleted(object? sender, Point point)
    {
        if (sender is ToolWindow pane) PaneDragCompleted?.Invoke(this, (pane, point));
    }

    private void OnChildClosed(object? sender, EventArgs e)
    {
        if (sender is ToolWindow pane) PaneClosed?.Invoke(this, pane);
    }

    private void OnChildActivated(object? sender, EventArgs e)
    {
        if (sender is ToolWindow pane && Mode == DockGroupMode.Tabs) Activate(pane);
    }

    /// <summary>
    /// A tab group owns a strip; a split group must not. The strip is created on demand and kept as a
    /// child so the panel arranges it like anything else — the manager never has to know about it.
    /// </summary>
    private void SyncTabStrip()
    {
        if (Mode == DockGroupMode.Tabs)
        {
            if (_tabs is null)
            {
                _tabs = new DockTabStrip { TabPlacement = DockTabPlacement.Bottom };
                Children.Add(_tabs);
            }

            _tabs.Panes = Children.OfType<ToolWindow>().ToList();
            _tabs.TabSelected += (_, pane) => Activate(pane);
        }
        else if (_tabs is not null)
        {
            Children.Remove(_tabs);
            _tabs = null;
        }
    }

    private void Activate(ToolWindow pane)
    {
        foreach (var other in Children.OfType<ToolWindow>())
        {
            bool active = ReferenceEquals(other, pane);
            if (other.IsActive != active) other.SetCurrentValue(ToolWindow.IsActiveProperty, active);
        }
    }

    /// <summary>The tab that is currently shown, or <c>null</c> when the group is empty.</summary>
    public ToolWindow? ActivePane =>
        Children.OfType<ToolWindow>().FirstOrDefault(p => p.IsActive)
        ?? Children.OfType<ToolWindow>().FirstOrDefault();

    /// <summary>
    /// Moves a child to another position inside this group — what a tab drag does when it is dropped
    /// between two tabs.
    /// </summary>
    public void MoveChild(ToolWindow pane, int index)
    {
        if (pane is null || !Children.Contains(pane)) return;

        int current = Children.IndexOf(pane);
        if (current < 0) return;

        int target = Math.Max(0, Math.Min(Children.Count - 1, index));
        if (target == current) return;

        Children.Remove(pane);
        Children.Insert(target, pane);
        InvalidateMeasure();
    }

    // ---------------------------------------------------------------- measure / arrange

    protected override Size MeasureOverride(Size availableSize)
    {
        if (Mode == DockGroupMode.Tabs) NormalizeTabs();

        var nodes = Nodes.ToList();
        if (nodes.Count == 0)
        {
            _tabs?.Measure(new Size(availableSize.Width, TabStripThickness));
            return new Size(0, 0);
        }

        var rects = ComputeRects(availableSize);

        for (int i = 0; i < nodes.Count && i < rects.Count; i++)
        {
            nodes[i].Measure(new Size(Math.Max(0, rects[i].Width), Math.Max(0, rects[i].Height)));
        }

        _tabs?.Measure(new Size(availableSize.Width, TabStripThickness));

        return new Size(
            double.IsInfinity(availableSize.Width) ? 0 : availableSize.Width,
            double.IsInfinity(availableSize.Height) ? 0 : availableSize.Height);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var nodes = Nodes.ToList();
        var rects = ComputeRects(finalSize);

        var bands = new List<Rect>();

        for (int i = 0; i < nodes.Count && i < rects.Count; i++)
        {
            // Visibility belongs to ComputeRects alone. Forcing it back to Visible here fights the
            // collapsed tab the measure pass just set, and the two passes ping-pong for ever.
            nodes[i].Arrange(rects[i]);
        }

        _bands.Clear();

        if (Mode == DockGroupMode.Split && nodes.Count > 1)
        {
            bool vertical = Orientation == Orientation.Horizontal;

            for (int i = 0; i < rects.Count - 1; i++)
            {
                var band = vertical
                    ? new Rect(rects[i].Right, 0, SplitterThickness, finalSize.Height)
                    : new Rect(0, rects[i].Bottom, finalSize.Width, SplitterThickness);

                bands.Add(band);
                _bands.Add((band, i, vertical));
            }
        }

        if (Mode == DockGroupMode.Tabs && _tabs is not null)
        {
            _tabs.Arrange(new Rect(0, Math.Max(0, finalSize.Height - TabStripThickness),
                finalSize.Width, Math.Min(TabStripThickness, finalSize.Height)));
        }

        if (!SameBands(bands.ToArray(), _bandRects))
        {
            _bandRects = bands.ToArray();
            InvalidateVisual();
        }

        return finalSize;
    }

    private static bool SameBands(Rect[] a, Rect[] b)
    {
        if (a.Length != b.Length) return false;

        for (int i = 0; i < a.Length; i++)
        {
            if (a[i] != b[i]) return false;
        }

        return true;
    }

    /// <summary>Distributes the available space by <c>Proportion</c>, with a splitter between slots.</summary>
    private List<Rect> ComputeRects(Size size)
    {
        var nodes = Nodes.ToList();
        var rects = new List<Rect>(nodes.Count);
        if (nodes.Count == 0) return rects;

        double width = Math.Max(0, size.Width);
        double height = Math.Max(0, size.Height);

        if (Mode == DockGroupMode.Tabs)
        {
            var active = ActivePane;
            double content = Math.Max(0, height - (_tabs is null ? 0 : TabStripThickness));

            foreach (var node in nodes)
            {
                bool visible = ReferenceEquals(node, active);
                node.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
                rects.Add(visible ? new Rect(0, 0, width, content) : new Rect(0, 0, 0, 0));
            }

            return rects;
        }

        bool vertical = Orientation == Orientation.Horizontal;
        double total = vertical ? width : height;
        double splitters = SplitterThickness * (nodes.Count - 1);
        double usable = Math.Max(0, total - splitters);

        foreach (var node in nodes)
        {
            if (node.Visibility != Visibility.Visible) node.Visibility = Visibility.Visible;
        }

        var weights = nodes.Select(n => Math.Max(0.0001, GetProportion(n))).ToList();
        double sum = weights.Sum();

        double offset = 0;
        for (int i = 0; i < nodes.Count; i++)
        {
            double extent = usable * weights[i] / sum;
            if (i == nodes.Count - 1) extent = Math.Max(0, total - offset);

            rects.Add(vertical
                ? new Rect(offset, 0, extent, height)
                : new Rect(0, offset, width, extent));

            offset += extent + SplitterThickness;
        }

        return rects;
    }

    /// <summary>Keeps exactly one tab active so the group always shows something.</summary>
    private void NormalizeTabs()
    {
        var panes = Children.OfType<ToolWindow>().ToList();
        if (panes.Count == 0) return;

        var active = panes.FirstOrDefault(p => p.IsActive) ?? panes[0];

        foreach (var pane in panes)
        {
            bool isActive = ReferenceEquals(pane, active);
            if (pane.IsActive != isActive)
                pane.SetCurrentValue(ToolWindow.IsActiveProperty, isActive);

            if (pane.ShowHeader)
                pane.SetCurrentValue(ToolWindow.ShowHeaderProperty, false);
        }

        if (_tabs is not null) _tabs.Panes = panes;
    }

    // ---------------------------------------------------------------- splitters

    protected override HitTestResult? HitTestCore(PointHitTestParameters hitTestParameters)
    {
        var point = hitTestParameters.HitPoint;

        foreach (var band in _bandRects)
        {
            if (band.Contains(point)) return new PointHitTestResult(this, point);
        }

        return base.HitTestCore(hitTestParameters);
    }

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);

        if (_bandRects.Length == 0) return;

        var line = TryFindResource("CONCOST.Brush.Border") as Brush ?? Brushes.Gray;

        foreach (var band in _bandRects)
        {
            if (band.Width <= 0 || band.Height <= 0) continue;

            dc.DrawRectangle(Brushes.Transparent, null, band);

            var hairline = band.Width <= band.Height
                ? new Rect(band.X + Math.Floor(band.Width / 2), band.Y, 1, band.Height)
                : new Rect(band.X, band.Y + Math.Floor(band.Height / 2), band.Width, 1);

            dc.DrawRectangle(line, null, hairline);
        }
    }

    protected override void OnMouseDown(MouseButtonEventArgs e)
    {
        base.OnMouseDown(e);

        if (e.ChangedButton != MouseButton.Left) return;

        var point = e.GetPosition(this);

        foreach (var band in _bands)
        {
            if (!band.Rect.Contains(point)) continue;

            var nodes = Nodes.ToList();
            if (band.Index + 1 >= nodes.Count) return;

            double current = Math.Max(0.0001, GetProportion(nodes[band.Index]));
            _drag = (band.Index, current, point, band.Vertical);

            CaptureMouse();
            Cursor = band.Vertical ? Cursors.SizeWE : Cursors.SizeNS;
            e.Handled = true;
            return;
        }
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);

        var point = e.GetPosition(this);

        if (_drag is null)
        {
            var hit = _bands.FirstOrDefault(b => b.Rect.Contains(point));
            Cursor = hit.Vertical ? Cursors.SizeWE : Cursors.SizeNS;
            return;
        }

        if (e.LeftButton != MouseButtonState.Pressed) return;

        var nodes = Nodes.ToList();
        if (_drag.Value.Index + 1 >= nodes.Count) return;

        double delta = _drag.Value.Vertical ? point.X - _drag.Value.Origin.X : point.Y - _drag.Value.Origin.Y;

        // Grow the first slot by the drag and take the same amount off its neighbour, so the rest of
        // the group does not move.
        double total = _drag.Value.Vertical ? ActualWidth : ActualHeight;
        if (total <= 0) return;

        double perPixel = Math.Max(0.0001, _drag.Value.Start) / Math.Max(1, total);
        double next = Math.Max(0.02, _drag.Value.Start + delta * perPixel);

        SetProportion(nodes[_drag.Value.Index], next);
    }

    protected override void OnMouseUp(MouseButtonEventArgs e)
    {
        base.OnMouseUp(e);

        if (_drag is null) return;

        _drag = null;
        Cursor = Cursors.Arrow;
        ReleaseMouseCapture();
    }
}
