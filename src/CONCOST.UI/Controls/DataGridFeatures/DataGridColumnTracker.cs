using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CONCOST.UI.Controls;

/// <summary>
/// Mirrors a <see cref="DataGrid"/>'s horizontal geometry onto an element that sits above or
/// below it: the row-header offset, the horizontal scroll offset and each column's live width.
/// </summary>
/// <remarks>
/// A filter row or a summary row is a separate control, so it cannot inherit the grid's column
/// widths the way a template part would. This tracker keeps the host aligned — including while
/// the user scrolls sideways or resizes a column — so a value always sits under its column.
/// </remarks>
internal sealed class DataGridColumnTracker
{
    private const double FallbackColumnWidth = 120d;

    private readonly DataGrid _grid;
    private readonly FrameworkElement _host;
    private readonly TranslateTransform _translate = new TranslateTransform();

    private ScrollViewer? _scrollViewer;
    private double[] _widths = Array.Empty<double>();

    public DataGridColumnTracker(DataGrid grid, FrameworkElement host)
    {
        _grid = grid;
        _host = host;
        _host.RenderTransform = _translate;
    }

    /// <summary>Raised when the header offset, the scroll offset or a column width changed.</summary>
    public event EventHandler? Changed;

    /// <summary>Columns in the order the grid paints them.</summary>
    public IReadOnlyList<DataGridColumn> OrderedColumns =>
        _grid.Columns.OrderBy(c => c.DisplayIndex).ToList();

    public void Attach()
    {
        _grid.Columns.CollectionChanged += OnColumnsChanged;
        _grid.LayoutUpdated += OnLayoutUpdated;
        _grid.SizeChanged += OnSizeChanged;

        _grid.ApplyTemplate();
        _scrollViewer = _grid.Template?.FindName("DG_ScrollViewer", _grid) as ScrollViewer
                        ?? FindScrollViewer(_grid);

        if (_scrollViewer is not null) _scrollViewer.ScrollChanged += OnScrollChanged;

        Apply();
    }

    public void Detach()
    {
        _grid.Columns.CollectionChanged -= OnColumnsChanged;
        _grid.LayoutUpdated -= OnLayoutUpdated;
        _grid.SizeChanged -= OnSizeChanged;

        if (_scrollViewer is not null) _scrollViewer.ScrollChanged -= OnScrollChanged;
        _scrollViewer = null;
    }

    /// <summary>
    /// The width to give a column's cell. Falls back to the declared width before the grid has
    /// ever been measured, so the first paint is not a row of 120 px boxes.
    /// </summary>
    public double WidthOf(DataGridColumn column)
    {
        if (column.ActualWidth > 0) return column.ActualWidth;

        var declared = column.Width;
        if (!declared.IsAuto && !declared.IsStar && declared.DisplayValue > 0) return declared.DisplayValue;

        return FallbackColumnWidth;
    }

    private void OnColumnsChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) => Apply();

    private void OnLayoutUpdated(object? sender, EventArgs e) => Apply();

    private void OnSizeChanged(object sender, SizeChangedEventArgs e) => Apply();

    private void OnScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        if (Math.Abs(e.HorizontalChange) < 0.5) return;
        Apply();
    }

    private void Apply()
    {
        var changed = false;

        var headerOffset = _grid.HeadersVisibility == DataGridHeadersVisibility.Column
            ? 0d
            : _grid.RowHeaderActualWidth;

        if (Math.Abs(_host.Margin.Left - headerOffset) > 0.5)
        {
            _host.Margin = new Thickness(headerOffset, 0, 0, 0);
            changed = true;
        }

        var offset = _scrollViewer?.HorizontalOffset ?? 0d;
        if (Math.Abs(_translate.X + offset) > 0.5)
        {
            _translate.X = -offset;
            changed = true;
        }

        // Column widths change on every layout pass while the grid settles (and whenever the user
        // drags a gripper), so they are part of the signal, not just the two offsets above.
        var columns = OrderedColumns;
        if (_widths.Length != columns.Count)
        {
            _widths = new double[columns.Count];
            changed = true;
        }

        for (var i = 0; i < columns.Count; i++)
        {
            var width = WidthOf(columns[i]);
            if (Math.Abs(_widths[i] - width) > 0.5)
            {
                _widths[i] = width;
                changed = true;
            }
        }

        if (changed) Changed?.Invoke(this, EventArgs.Empty);
    }

    private static ScrollViewer? FindScrollViewer(DependencyObject parent)
    {
        var count = VisualTreeHelper.GetChildrenCount(parent);
        for (var i = 0; i < count; i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is ScrollViewer viewer) return viewer;

            var nested = FindScrollViewer(child);
            if (nested is not null) return nested;
        }

        return null;
    }
}
