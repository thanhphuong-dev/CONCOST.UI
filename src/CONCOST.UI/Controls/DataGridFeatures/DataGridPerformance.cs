using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CONCOST.UI.Controls;

/// <summary>
/// The switches a grid over ten thousand rows needs, attached so an existing
/// <see cref="DataGrid"/> picks them up without a new control type:
/// <c>DeferredScrolling</c> drags the thumb with only a position read-out and scrolls when it is
/// released, <c>CachePages</c> pre-materialises rows ahead of the viewport so fast scrolling does
/// not flash empty cells, and the two scroll helpers reach a row through the virtualized panel.
/// </summary>
public static class DataGridPerformance
{
    /// <summary>Identifies the <see cref="GetDeferredScrolling"/> attached property.</summary>
    public static readonly DependencyProperty DeferredScrollingProperty =
        DependencyProperty.RegisterAttached("DeferredScrolling", typeof(bool), typeof(DataGridPerformance),
            new FrameworkPropertyMetadata(false, OnDeferredScrollingChanged));

    /// <summary>Identifies the <see cref="GetCachePages"/> attached property.</summary>
    public static readonly DependencyProperty CachePagesProperty =
        DependencyProperty.RegisterAttached("CachePages", typeof(double), typeof(DataGridPerformance),
            new FrameworkPropertyMetadata(0.0, OnCachePagesChanged));

    private static readonly DependencyProperty AppliedViewerProperty =
        DependencyProperty.RegisterAttached("AppliedViewer", typeof(ScrollViewer), typeof(DataGridPerformance),
            new FrameworkPropertyMetadata(null));

    /// <summary>Reads the deferred-scrolling switch of a grid.</summary>
    public static bool GetDeferredScrolling(DataGrid grid) => (bool)grid.GetValue(DeferredScrollingProperty);

    /// <summary>
    /// Drags the scroll thumb showing a position read-out and scrolls the rows on release. A grid at
    /// fifty thousand rows repaints a page of cells per frame otherwise, which no machine keeps up
    /// with when the thumb is dragged end to end.
    /// </summary>
    public static void SetDeferredScrolling(DataGrid grid, bool value) => grid.SetValue(DeferredScrollingProperty, value);

    /// <summary>Reads the page-cache width of a grid; <c>0</c> keeps the theme default.</summary>
    public static double GetCachePages(DataGrid grid) => (double)grid.GetValue(CachePagesProperty);

    /// <summary>
    /// How many pages of rows to materialise on each side of the viewport. One is enough to hide a
    /// fast scroll: the panel reuses the recycled row for the cell that is about to arrive instead of
    /// showing the gap where it will be.
    /// </summary>
    public static void SetCachePages(DataGrid grid, double value) => grid.SetValue(CachePagesProperty, value);

    /// <summary>Brings <paramref name="item"/> into the viewport through the virtualized panel.</summary>
    public static void ScrollToItem(DataGrid grid, object item)
    {
        if (grid is null) throw new ArgumentNullException(nameof(grid));
        grid.ScrollIntoView(item);
    }

    /// <summary>Brings the row at <paramref name="index"/> into the viewport; <c>false</c> when the index is outside the items.</summary>
    public static bool ScrollToIndex(DataGrid grid, int index)
    {
        if (grid is null) throw new ArgumentNullException(nameof(grid));

        if (index < 0 || index >= grid.Items.Count) return false;

        object item = grid.Items[index];
        if (item is null) return false;

        grid.ScrollIntoView(item);
        return true;
    }

    private static void OnCachePagesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not DataGrid grid) return;

        double pages = e.NewValue is double value && !double.IsNaN(value) ? Math.Max(0, value) : 0;

        // The VirtualizingPanel attached properties are read from the items owner, so the value set
        // here is the one the rows panel honours. The two writes are coupled — a length write
        // re-asserts the unit and a ClearValue of the pair leaves the last unit behind — so zero
        // restores the framework default explicitly instead of clearing: one item of cache on each
        // side, unit Item.
        if (pages <= 0)
        {
            grid.SetValue(VirtualizingPanel.CacheLengthUnitProperty, VirtualizationCacheLengthUnit.Item);
            grid.SetValue(VirtualizingPanel.CacheLengthProperty, new VirtualizationCacheLength(1));
            return;
        }

        grid.SetValue(VirtualizingPanel.CacheLengthUnitProperty, VirtualizationCacheLengthUnit.Page);
        grid.SetValue(VirtualizingPanel.CacheLengthProperty, new VirtualizationCacheLength(pages));
    }

    private static void OnDeferredScrollingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not DataGrid grid) return;

        grid.Loaded -= OnGridLoaded;
        grid.Unloaded -= OnGridUnloaded;

        if ((bool)e.NewValue)
        {
            grid.Loaded += OnGridLoaded;
            grid.Unloaded += OnGridUnloaded;

            if (grid.IsLoaded) ApplyDeferredScrolling(grid);
        }
        else
        {
            ApplyDeferredScrolling(grid, enabled: false);
        }
    }

    private static void OnGridLoaded(object sender, RoutedEventArgs e) => ApplyDeferredScrolling((DataGrid)sender);

    private static void OnGridUnloaded(object sender, RoutedEventArgs e) => ApplyDeferredScrolling((DataGrid)sender, enabled: false);

    private static void ApplyDeferredScrolling(DataGrid grid, bool enabled = true)
    {
        // The value lands on the template's viewer, not on the grid: the flag is not inherited, and
        // the grid is not a ScrollViewer. The library's template names its viewer, and a host with a
        // custom template still has one somewhere below.
        ScrollViewer? viewer = grid.Template?.FindName("DG_ScrollViewer", grid) as ScrollViewer
            ?? FindScrollViewer(grid);

        if (viewer is null) return;

        ScrollViewer? previous = (ScrollViewer?)grid.GetValue(AppliedViewerProperty);
        if (!ReferenceEquals(previous, viewer) && previous is not null)
        {
            previous.SetValue(ScrollViewer.IsDeferredScrollingEnabledProperty, false);
        }

        viewer.SetValue(ScrollViewer.IsDeferredScrollingEnabledProperty, enabled);
        grid.SetValue(AppliedViewerProperty, enabled ? viewer : null);
    }

    private static ScrollViewer? FindScrollViewer(DependencyObject root)
    {
        int count = VisualTreeHelper.GetChildrenCount(root);

        for (int i = 0; i < count; i++)
        {
            var child = VisualTreeHelper.GetChild(root, i);

            if (child is ScrollViewer viewer) return viewer;

            var nested = FindScrollViewer(child);
            if (nested is not null) return nested;
        }

        return null;
    }
}
