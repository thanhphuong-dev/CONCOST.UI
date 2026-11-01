using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using CONCOST.UI.Icons;

namespace CONCOST.UI.Controls;

/// <summary>Which edge of a pane group the tab strip sits on.</summary>
public enum DockTabPlacement
{
    Top,
    Bottom
}

/// <summary>
/// The tab strip of a pane group: one tab per <see cref="ToolWindow"/> sharing an edge.
/// </summary>
/// <remarks>
/// <para>
/// This is the AvalonDock idea that a pane group holds <em>contents</em>, and several contents in one
/// group are tabs rather than a stack. Stacking them, as the first version of this control did, gives
/// every panel its own slice of the edge — which is not what an IDE does and not what a user expects
/// when two panels are docked to the same side.
/// </para>
/// <para>
/// The tabs are built in code rather than through <see cref="ItemsControl.ItemsSource"/>. A pane is a
/// <see cref="UIElement"/>, and an items control that is handed a UIElement as an item <b>adopts</b>
/// it: the pane is reparented into the generated container and disappears from the manager's children,
/// which silently collapses the layout. Building the buttons by hand keeps every pane where it belongs
/// and lets a tab carry the pane's own icon and caption without a wrapper object.
/// </para>
/// </remarks>
public class DockTabStrip : Control
{
    static DockTabStrip()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(DockTabStrip),
            new FrameworkPropertyMetadata(typeof(DockTabStrip)));
    }

    private readonly List<ToggleButton> _buttons = new();
    private readonly List<ToolWindow> _shown = new();
    private StackPanel? _host;

    /// <summary>Raised when the user picks a tab.</summary>
    public event EventHandler<ToolWindow>? TabSelected;

    public static readonly DependencyProperty TabPlacementProperty =
        DependencyProperty.Register(nameof(TabPlacement), typeof(DockTabPlacement), typeof(DockTabStrip),
            new PropertyMetadata(DockTabPlacement.Bottom, OnTabPlacementChanged));

    /// <summary>Which edge of the group the tabs hug. Default <see cref="DockTabPlacement.Bottom"/>.</summary>
    public DockTabPlacement TabPlacement
    {
        get => (DockTabPlacement)GetValue(TabPlacementProperty);
        set => SetValue(TabPlacementProperty, value);
    }

    /// <summary>
    /// Puts the accent rule on the side the content is: under a tab that hugs the bottom of a group,
    /// above one that sits on top of it. Without this the underline stayed at the bottom and a top
    /// strip read as if it were still below the pane.
    /// </summary>
    private static void OnTabPlacementChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DockTabStrip strip) strip.ApplyTabMetrics();
    }

    public static readonly DependencyProperty TabHeightProperty =
        DependencyProperty.Register(nameof(TabHeight), typeof(double), typeof(DockTabStrip),
            new FrameworkPropertyMetadata(26.0, FrameworkPropertyMetadataOptions.AffectsMeasure, OnMetricsChanged));

    /// <summary>Height of one tab, in pixels. Default 26.</summary>
    public double TabHeight
    {
        get => (double)GetValue(TabHeightProperty);
        set => SetValue(TabHeightProperty, value);
    }

    public static readonly DependencyProperty IconSizeProperty =
        DependencyProperty.Register(nameof(IconSize), typeof(double), typeof(DockTabStrip),
            new FrameworkPropertyMetadata(12.0, FrameworkPropertyMetadataOptions.AffectsMeasure, OnMetricsChanged));

    /// <summary>Size of the glyph in front of a tab's caption, in pixels. Default 12.</summary>
    public double IconSize
    {
        get => (double)GetValue(IconSizeProperty);
        set => SetValue(IconSizeProperty, value);
    }

    private static void OnMetricsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DockTabStrip strip) strip.ApplyTabMetrics();
    }

    public static readonly DependencyProperty PanesProperty =
        DependencyProperty.Register(nameof(Panes), typeof(IEnumerable), typeof(DockTabStrip),
            new PropertyMetadata(null, OnPanesChanged));

    /// <summary>The panes of this group, in declaration order.</summary>
    public IEnumerable? Panes
    {
        get => (IEnumerable?)GetValue(PanesProperty);
        set => SetValue(PanesProperty, value);
    }

    private static void OnPanesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DockTabStrip strip) strip.Rebuild();
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        _host = GetTemplateChild("PART_Tabs") as StackPanel;
        Rebuild();
    }

    private void Rebuild()
    {
        if (_host is null) return;

        var panes = new List<ToolWindow>();
        if (Panes is not null)
        {
            foreach (var item in Panes)
            {
                if (item is ToolWindow pane) panes.Add(pane);
            }
        }

        // The manager re-assigns the group on every layout pass; rebuilding the buttons each time would
        // throw away focus and hover state, so only rebuild when the set really changed.
        if (SamePanes(panes, _shown))
        {
            SyncChecked();
            return;
        }

        _host.Children.Clear();
        _buttons.Clear();
        _shown.Clear();
        _shown.AddRange(panes);

        var style = TryFindResource("CONCOST.DockTab.Base") as Style;

        foreach (var pane in panes)
        {
            var button = new ToggleButton
            {
                Tag = pane,
                IsChecked = pane.IsActive,
                ToolTip = pane.Header,
                Focusable = false
            };

            if (style is not null) button.Style = style;
            button.Content = BuildContent(pane);
            button.Click += OnTabClick;

            // Dragging a tab either reorders it inside the strip or moves the pane to another edge.
            button.PreviewMouseLeftButtonDown += OnTabPressed;
            button.MouseMove += OnTabDragging;
            button.MouseLeftButtonUp += OnTabReleased;

            _host.Children.Add(button);
            _buttons.Add(button);
        }

        ApplyTabMetrics();
    }

    /// <summary>Puts the height, the glyph size and the accent side on every tab.</summary>
    private void ApplyTabMetrics()
    {
        var accent = TabPlacement == DockTabPlacement.Top
            ? new Thickness(0, 2, 0, 0)
            : new Thickness(0, 0, 0, 2);

        foreach (var button in _buttons)
        {
            button.Height = TabHeight;
            button.BorderThickness = accent;

            if (button.Content is not StackPanel row) continue;

            foreach (var child in row.Children)
            {
                if (child is Icon icon)
                {
                    icon.Width = IconSize;
                    icon.Height = IconSize;
                }
            }
        }
    }

    private static bool SamePanes(List<ToolWindow> a, List<ToolWindow> b)
    {
        if (a.Count != b.Count) return false;

        for (int i = 0; i < a.Count; i++)
        {
            if (!ReferenceEquals(a[i], b[i])) return false;
        }

        return true;
    }

    private void SyncChecked()
    {
        for (int i = 0; i < _buttons.Count && i < _shown.Count; i++)
        {
            bool active = _shown[i].IsActive;
            if (_buttons[i].IsChecked != active) _buttons[i].IsChecked = active;
        }
    }

    private static UIElement BuildContent(ToolWindow pane)
    {
        var row = new StackPanel { Orientation = Orientation.Horizontal };

        if (pane.Icon != IconKind.None)
        {
            row.Children.Add(new Icon
            {
                Kind = pane.Icon,
                Width = 12,
                Height = 12,
                Margin = new Thickness(0, 0, 6, 0),
                VerticalAlignment = VerticalAlignment.Center
            });
        }

        row.Children.Add(new TextBlock
        {
            Text = pane.Header?.ToString() ?? string.Empty,
            VerticalAlignment = VerticalAlignment.Center
        });

        return row;
    }

    private Point _pressPoint;
    private ToolWindow? _dragPane;
    private bool _dragging;

    private void OnTabPressed(object sender, MouseButtonEventArgs e)
    {
        if (sender is not ToggleButton button || button.Tag is not ToolWindow pane) return;

        _pressPoint = e.GetPosition(this);
        _dragPane = pane;
        _dragging = false;
    }

    private void OnTabDragging(object sender, MouseEventArgs e)
    {
        if (_dragPane is null || e.LeftButton != MouseButtonState.Pressed) return;

        var point = e.GetPosition(this);

        if (!_dragging)
        {
            if (Math.Abs(point.X - _pressPoint.X) < 5 && Math.Abs(point.Y - _pressPoint.Y) < 5) return;

            _dragging = true;
            (sender as UIElement)?.CaptureMouse();
        }

        var manager = FindManager();
        if (manager is null) return;

        manager.ShowDropHint(_dragPane, manager.PointFromScreen(PointToScreen(point)));
    }

    private void OnTabReleased(object sender, MouseButtonEventArgs e)
    {
        (sender as UIElement)?.ReleaseMouseCapture();

        var pane = _dragPane;
        bool dragged = _dragging;
        _dragPane = null;
        _dragging = false;

        if (!dragged || pane is null) return;

        var manager = FindManager();
        if (manager is null) return;

        manager.EndDropPreview();

        var local = e.GetPosition(this);
        var screen = PointToScreen(local);

        // Dropped back on its own strip: reorder rather than redock.
        if (IsOverTabs(local))
        {
            manager.MovePane(pane, IndexAt(local));
            return;
        }

        manager.DockAt(pane, manager.PointFromScreen(screen));
    }

    private bool IsOverTabs(Point point) =>
        point.X >= 0 && point.Y >= 0 && point.X <= ActualWidth && point.Y <= ActualHeight;

    private int IndexAt(Point point)
    {
        for (int i = 0; i < _buttons.Count; i++)
        {
            var origin = _buttons[i].TranslatePoint(new Point(0, 0), this);
            double width = _buttons[i].ActualWidth;

            if (point.X < origin.X + width / 2) return i;
        }

        return Math.Max(0, _buttons.Count - 1);
    }

    private DockingManager? FindManager()
    {
        DependencyObject? node = this;

        while (node is not null)
        {
            if (node is DockingManager manager) return manager;
            node = VisualTreeHelper.GetParent(node);
        }

        return null;
    }

    private void OnTabClick(object sender, RoutedEventArgs e)
    {
        if (sender is not ToggleButton button || button.Tag is not ToolWindow pane) return;

        // A toggle would uncheck itself on a second click; a tab that is already selected stays selected.
        button.IsChecked = true;

        foreach (var other in _buttons)
        {
            if (!ReferenceEquals(other, button)) other.IsChecked = false;
        }

        // The manager clears the other panes of the group when a pane becomes active, so the same code
        // path serves a click, a keyboard shortcut and a programmatic activation.
        pane.SetCurrentValue(ToolWindow.IsActiveProperty, true);
        TabSelected?.Invoke(this, pane);
    }
}
