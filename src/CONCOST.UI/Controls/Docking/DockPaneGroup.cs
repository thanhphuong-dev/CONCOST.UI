using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using CONCOST.UI.Icons;

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
    private DockTabStrip? _tabs;
    private readonly List<(Rect Rect, int Index, bool Vertical)> _bands = new();
    private Rect[] _bandRects = Array.Empty<Rect>();
    private (int Index, double First, double Pair, double PairPixels, Point Origin, bool Vertical)? _drag;

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

    // ---------------------------------------------------------------- chrome

    public static readonly DependencyProperty SplitterThicknessProperty =
        DependencyProperty.Register(nameof(SplitterThickness), typeof(double), typeof(DockPaneGroup),
            new FrameworkPropertyMetadata(5.0, FrameworkPropertyMetadataOptions.AffectsMeasure));

    /// <summary>
    /// Thickness of the gutter between two slots of this group, in pixels. Default 5 — a constant in
    /// the first version, so a manager that asked for a thinner gutter still got this one.
    /// </summary>
    public double SplitterThickness
    {
        get => (double)GetValue(SplitterThicknessProperty);
        set => SetValue(SplitterThicknessProperty, value);
    }

    public static readonly DependencyProperty TabStripThicknessProperty =
        DependencyProperty.Register(nameof(TabStripThickness), typeof(double), typeof(DockPaneGroup),
            new FrameworkPropertyMetadata(26.0, FrameworkPropertyMetadataOptions.AffectsMeasure, OnTabChromeChanged));

    /// <summary>Height reserved for the tab strip when <see cref="Mode"/> is tabs. Default 26.</summary>
    public double TabStripThickness
    {
        get => (double)GetValue(TabStripThicknessProperty);
        set => SetValue(TabStripThicknessProperty, value);
    }

    public static readonly DependencyProperty TabPlacementProperty =
        DependencyProperty.Register(nameof(TabPlacement), typeof(DockTabPlacement), typeof(DockPaneGroup),
            new FrameworkPropertyMetadata(DockTabPlacement.Top, FrameworkPropertyMetadataOptions.AffectsMeasure,
                OnTabChromeChanged));

    /// <summary>Which edge of the group its tab strip hugs. Default <see cref="DockTabPlacement.Top"/>.</summary>
    public DockTabPlacement TabPlacement
    {
        get => (DockTabPlacement)GetValue(TabPlacementProperty);
        set => SetValue(TabPlacementProperty, value);
    }

    public static readonly DependencyProperty SplitterBrushProperty =
        DependencyProperty.Register(nameof(SplitterBrush), typeof(Brush), typeof(DockPaneGroup),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Colour of the hairline in each gutter. Empty takes <c>CONCOST.Brush.Border</c>.</summary>
    public Brush? SplitterBrush
    {
        get => (Brush?)GetValue(SplitterBrushProperty);
        set => SetValue(SplitterBrushProperty, value);
    }

    // ---------------------------------------------------------------- header

    public static readonly DependencyProperty HeaderProperty =
        DependencyProperty.Register(nameof(Header), typeof(object), typeof(DockPaneGroup),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>
    /// Caption on the group's own header band.
    /// </summary>
    /// <remarks>
    /// A group hides the headers of the panes it holds — two panels in one group are tabs of a
    /// single slot, and giving each its own header would put two titles above one panel. That left
    /// the group with no title at all, so the pane's content started at the very top of the area
    /// with a floating accent rule above it. This band puts the title back.
    /// </remarks>
    public object? Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    public static readonly DependencyProperty IconProperty =
        DependencyProperty.Register(nameof(Icon), typeof(IconKind), typeof(DockPaneGroup),
            new FrameworkPropertyMetadata(IconKind.None, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Glyph shown before the group's caption.</summary>
    public IconKind Icon
    {
        get => (IconKind)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public static readonly DependencyProperty HeaderHeightProperty =
        DependencyProperty.Register(nameof(HeaderHeight), typeof(double), typeof(DockPaneGroup),
            new FrameworkPropertyMetadata(28.0, FrameworkPropertyMetadataOptions.AffectsMeasure));

    /// <summary>
    /// Height of the group's own header band, in pixels. Set it to 0 to drop the band entirely and
    /// let the group's content reach the top of its slot.
    /// </summary>
    public double HeaderHeight
    {
        get => (double)GetValue(HeaderHeightProperty);
        set => SetValue(HeaderHeightProperty, value);
    }

    public static readonly DependencyProperty ShowHeaderProperty =
        DependencyProperty.Register(nameof(ShowHeader), typeof(bool), typeof(DockPaneGroup),
            new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsMeasure));

    /// <summary>Draw the group's own header band. Default <c>true</c>.</summary>
    public bool ShowHeader
    {
        get => (bool)GetValue(ShowHeaderProperty);
        set => SetValue(ShowHeaderProperty, value);
    }

    /// <summary>Thickness of the group's header band, or 0 when there is no band.</summary>
    private double EffectiveHeaderHeight =>
        ShowHeader && Header != null && !string.IsNullOrWhiteSpace(Header.ToString()) && HeaderHeight > 0 && HeaderHeight < 200 ? HeaderHeight : 0;

    private static void OnTabChromeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not DockPaneGroup group || group._tabs is null) return;

        group._tabs.TabPlacement = group.TabPlacement;
        group._tabs.TabHeight = group.TabStripThickness;
    }

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
                _tabs = new DockTabStrip
                {
                    TabPlacement = TabPlacement,
                    TabHeight = TabStripThickness,
                    IconSize = Math.Max(8, TabStripThickness * 0.46)
                };
                _tabs.TabSelected += OnTabStripSelected;
                Children.Add(_tabs);
            }

            _tabs.Panes = Children.OfType<ToolWindow>().ToList();
        }
        else if (_tabs is not null)
        {
            Children.Remove(_tabs);
            _tabs = null;
        }
    }

    private void OnTabStripSelected(object? sender, ToolWindow pane) => Activate(pane);

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

        double header = EffectiveHeaderHeight;

        if (Mode == DockGroupMode.Split && nodes.Count > 1)
        {
            bool vertical = Orientation == Orientation.Horizontal;

            for (int i = 0; i < rects.Count - 1; i++)
            {
                var band = vertical
                    ? new Rect(rects[i].Right, header, SplitterThickness, Math.Max(0, finalSize.Height - header))
                    : new Rect(0, rects[i].Bottom, finalSize.Width, SplitterThickness);

                bands.Add(band);
                _bands.Add((band, i, vertical));
            }
        }

        if (Mode == DockGroupMode.Tabs && _tabs is not null)
        {
            // The strip sits under the group's own header, so the title and the tabs read as one
            // piece of chrome instead of a caption floating above an unrelated shelf.
            double stripTop = TabPlacement == DockTabPlacement.Top
                ? header
                : Math.Max(header, finalSize.Height - TabStripThickness);

            double stripHeight = Math.Min(TabStripThickness, Math.Max(0, finalSize.Height - header));

            _tabs.Arrange(new Rect(0, stripTop, finalSize.Width, stripHeight));
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

        // The group's own header sits above everything it holds, so every slot starts below it.
        double header = EffectiveHeaderHeight;

        if (Mode == DockGroupMode.Tabs)
        {
            var active = ActivePane;

            // The strip takes a row off the top when it sits there, and one off the bottom when it
            // hangs below. Either way the content gets what is left.
            double strip = _tabs is null ? 0 : TabStripThickness;
            double content = TabPlacement == DockTabPlacement.Top
                ? Math.Max(0, height - header - strip)
                : Math.Max(0, height - header - strip);

            double top = TabPlacement == DockTabPlacement.Top ? header + strip : header;

            foreach (var node in nodes)
            {
                bool visible = ReferenceEquals(node, active);
                node.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
                rects.Add(visible
                    ? new Rect(0, top, width, content)
                    : new Rect(0, top, 0, 0));
            }

            return rects;
        }

        bool vertical = Orientation == Orientation.Horizontal;
        double total = vertical ? width : height - header;
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
                ? new Rect(offset, header, extent, height - header)
                : new Rect(0, header + offset, width, extent));

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

        DrawHeader(dc);

        if (_bandRects.Length == 0) return;

        var line = SplitterBrush
            ?? TryFindResource("CONCOST.Brush.BorderSecondary") as Brush
            ?? TryFindResource("CONCOST.Brush.Border") as Brush
            ?? Brushes.Gray;

        var hot = TryFindResource("CONCOST.Brush.Primary") as Brush ?? Brushes.Orange;

        foreach (var band in _bandRects)
        {
            if (band.Width <= 0 || band.Height <= 0) continue;

            dc.DrawRectangle(Brushes.Transparent, null, band);

            var hairline = band.Width <= band.Height
                ? new Rect(band.X + Math.Floor(band.Width / 2), band.Y, 1, band.Height)
                : new Rect(band.X, band.Y + Math.Floor(band.Height / 2), band.Width, 1);

            dc.DrawRectangle(line, null, hairline);

            // The gutter under the pointer lights up in the accent colour, the same way the
            // manager's own splitters do. Without it the divider reads as a printed rule.
            if (!_hotBand.HasValue || !_hotBand.Value.Equals(band)) continue;

            var glow = band.Width <= band.Height
                ? new Rect(band.X + Math.Floor(band.Width / 2) - 1, band.Y, 3, band.Height)
                : new Rect(band.X, band.Y + Math.Floor(band.Height / 2) - 1, band.Width, 3);

            dc.DrawRectangle(hot, null, glow);
        }
    }

    /// <summary>
    /// Draws the group's own header band: a tinted strip with a caption, matching the header of a
    /// <see cref="ToolWindow"/> so a nested group looks like the same kind of panel as a pane.
    /// </summary>
    private void DrawHeader(DrawingContext dc)
    {
        double header = EffectiveHeaderHeight;
        if (header <= 0 || ActualWidth <= 0) return;

        var surface = TryFindResource("CONCOST.Brush.SurfaceSecondary") as Brush ?? Brushes.Gainsboro;
        var border = TryFindResource("CONCOST.Brush.Border") as Brush ?? Brushes.Gray;
        var text = TryFindResource("CONCOST.Brush.TextPrimary") as Brush ?? Brushes.Black;
        var accent = TryFindResource("CONCOST.Brush.Primary") as Brush ?? Brushes.Orange;

        dc.DrawRectangle(surface, null, new Rect(0, 0, ActualWidth, header));

        var line = new Pen(border, 1);
        dc.DrawLine(line, new Point(0, header - 0.5), new Point(ActualWidth, header - 0.5));

        string caption = Header?.ToString() ?? string.Empty;
        if (string.IsNullOrEmpty(caption)) return;

        // A short accent rule in front of the caption marks the band as a title rather than as a
        // strip of tint, and it is the one saturated mark the group owns.
        dc.DrawRectangle(accent, null, new Rect(0, 6, 2.5, header - 12));

        double dpi = VisualTreeHelper.GetDpi(this).PixelsPerDip;

        var formatted = new FormattedText(caption, CultureInfo.CurrentUICulture,
            FlowDirection.LeftToRight, new Typeface("Segoe UI"), 12, text, dpi)
        {
            MaxTextWidth = Math.Max(8, ActualWidth - 24),
            MaxLineCount = 1,
            Trimming = TextTrimming.CharacterEllipsis
        };

        dc.DrawText(formatted, new Point(12, (header - formatted.Height) / 2));
    }
    private Rect? _hotBand;

    private void UpdateHotBand(Point point)
    {
        Rect? found = null;

        foreach (var band in _bandRects)
        {
            if (band.Contains(point)) { found = band; break; }
        }

        if (found.HasValue && _hotBand.HasValue && found.Value == _hotBand.Value) return;

        _hotBand = found;
        InvalidateVisual();
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

            CaptureMouse();
            Cursor = band.Vertical ? Cursors.SizeWE : Cursors.SizeNS;
            e.Handled = true;

            // The two slots either side of the band keep their combined pixel extent for the whole
            // drag, so only the divider moves and the remaining slots do not creep.
            double total = band.Vertical ? ActualWidth : ActualHeight;
            double usable = Math.Max(0, total - SplitterThickness * (nodes.Count - 1));

            var weights = nodes.Select(n => Math.Max(0.0001, GetProportion(n))).ToList();
            double sum = Math.Max(0.0001, weights.Sum());
            double first = weights[band.Index];
            double pair = first + weights[band.Index + 1];
            double pairPixels = usable * pair / sum;

            _drag = (band.Index, first, pair, pairPixels, point, band.Vertical);
            return;
        }
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);

        var point = e.GetPosition(this);

        if (_drag is null)
        {
            foreach (var band in _bands)
            {
                if (!band.Rect.Contains(point)) continue;

                Cursor = band.Vertical ? Cursors.SizeWE : Cursors.SizeNS;
                UpdateHotBand(point);
                return;
            }

            // Clearing the local value restores inheritance, so pane content does not inherit a
            // resize cursor just because the pointer crossed the group.
            Cursor = null;
            UpdateHotBand(point);
            return;
        }

        if (e.LeftButton != MouseButtonState.Pressed) return;

        var nodes = Nodes.ToList();
        if (_drag.Value.Index + 1 >= nodes.Count) return;

        double delta = _drag.Value.Vertical ? point.X - _drag.Value.Origin.X : point.Y - _drag.Value.Origin.Y;
        double scale = _drag.Value.Pair / Math.Max(1, _drag.Value.PairPixels);

        double min = _drag.Value.Pair * 0.05;
        double next = Math.Min(_drag.Value.Pair - min, Math.Max(min, _drag.Value.First + delta * scale));

        SetProportion(nodes[_drag.Value.Index], next);
        SetProportion(nodes[_drag.Value.Index + 1], _drag.Value.Pair - next);
    }

    protected override void OnMouseUp(MouseButtonEventArgs e)
    {
        base.OnMouseUp(e);

        if (_drag is null) return;

        _drag = null;
        Cursor = null;
        ReleaseMouseCapture();
    }
}
