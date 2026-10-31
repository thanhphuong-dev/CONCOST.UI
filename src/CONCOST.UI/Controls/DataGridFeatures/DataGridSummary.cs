using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace CONCOST.UI.Controls;

/// <summary>Which aggregate a <see cref="DataGridSummary"/> shows for a column.</summary>
public enum DataGridSummaryAggregate
{
    /// <summary>Sum for a numeric column, row count for a text column.</summary>
    Auto,
    Sum,
    Average,
    Min,
    Max,
    Count
}

/// <summary>
/// A summary row that sits below a <see cref="DataGrid"/> and shows an aggregate per column.
/// </summary>
/// <remarks>
/// The row mirrors the grid's column widths, display order, row-header width and horizontal
/// scroll offset, so every number stays under the column it belongs to. Aggregates are computed
/// over the grid's <em>filtered</em> view: what you see is what is summed.
/// </remarks>
public class DataGridSummary : Control
{
    static DataGridSummary()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(DataGridSummary),
            new FrameworkPropertyMetadata(typeof(DataGridSummary)));
    }

    public DataGridSummary()
    {
        Summaries = new ObservableCollection<ColumnSummaryItem>();
    }

    #region Dependency Properties

    public static readonly DependencyProperty TargetGridProperty =
        DependencyProperty.Register(nameof(TargetGrid), typeof(DataGrid), typeof(DataGridSummary),
            new FrameworkPropertyMetadata(null, OnTargetGridChanged));

    public DataGrid? TargetGrid
    {
        get => (DataGrid?)GetValue(TargetGridProperty);
        set => SetValue(TargetGridProperty, value);
    }

    public static readonly DependencyProperty SummariesProperty =
        DependencyProperty.Register(nameof(Summaries), typeof(ObservableCollection<ColumnSummaryItem>), typeof(DataGridSummary),
            new FrameworkPropertyMetadata(null));

    /// <summary>One entry per column of the target grid, in display order.</summary>
    public ObservableCollection<ColumnSummaryItem> Summaries
    {
        get => (ObservableCollection<ColumnSummaryItem>)GetValue(SummariesProperty);
        set => SetValue(SummariesProperty, value);
    }

    public static readonly DependencyProperty AggregateProperty =
        DependencyProperty.Register(nameof(Aggregate), typeof(DataGridSummaryAggregate), typeof(DataGridSummary),
            new FrameworkPropertyMetadata(DataGridSummaryAggregate.Auto, OnAggregateChanged));

    /// <summary>The aggregate used for every column without a per-column override.</summary>
    public DataGridSummaryAggregate Aggregate
    {
        get => (DataGridSummaryAggregate)GetValue(AggregateProperty);
        set => SetValue(AggregateProperty, value);
    }

    public static readonly DependencyProperty DisplayFormatProperty =
        DependencyProperty.Register(nameof(DisplayFormat), typeof(string), typeof(DataGridSummary),
            new FrameworkPropertyMetadata("N1", OnAggregateChanged));

    /// <summary>Numeric format for the aggregate value, e.g. <c>N0</c> or <c>#,##0.00</c>.</summary>
    public string DisplayFormat
    {
        get => (string)GetValue(DisplayFormatProperty);
        set => SetValue(DisplayFormatProperty, value);
    }

    public static readonly DependencyProperty RowHeightProperty =
        DependencyProperty.Register(nameof(RowHeight), typeof(double), typeof(DataGridSummary),
            new FrameworkPropertyMetadata(28.0));

    public double RowHeight
    {
        get => (double)GetValue(RowHeightProperty);
        set => SetValue(RowHeightProperty, value);
    }

    #endregion

    #region Attached per-column aggregate

    public static readonly DependencyProperty AggregateOnColumnProperty =
        DependencyProperty.RegisterAttached("AggregateOnColumn", typeof(DataGridSummaryAggregate), typeof(DataGridSummary),
            new FrameworkPropertyMetadata(DataGridSummaryAggregate.Auto));

    /// <summary>Overrides <see cref="Aggregate"/> for a single column.</summary>
    public static void SetAggregateOnColumn(DataGridColumn column, DataGridSummaryAggregate value) =>
        column.SetValue(AggregateOnColumnProperty, value);

    public static DataGridSummaryAggregate GetAggregateOnColumn(DataGridColumn column) =>
        (DataGridSummaryAggregate)column.GetValue(AggregateOnColumnProperty);

    #endregion

    private ItemsControl? _items;
    private DataGridColumnTracker? _tracker;
    private ICollectionView? _view;
    private DependencyPropertyDescriptor? _itemsSourceDescriptor;

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _items = GetTemplateChild("PART_Items") as ItemsControl;
        HookGrid();
    }

    protected override void OnInitialized(EventArgs e)
    {
        base.OnInitialized(e);
        Loaded += (_, __) => HookGrid();
        Unloaded += (_, __) => UnhookGrid();
    }

    private static void OnTargetGridChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not DataGridSummary summary) return;

        summary.UnhookGrid();
        summary.HookGrid();
        summary.RefreshSummaries();
    }

    private static void OnAggregateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DataGridSummary summary) summary.RefreshSummaries();
    }

    #region Grid plumbing

    private void HookGrid()
    {
        var grid = TargetGrid;
        if (grid is null || _tracker is not null) return;

        _itemsSourceDescriptor = DependencyPropertyDescriptor.FromProperty(
            ItemsControl.ItemsSourceProperty, typeof(DataGrid));
        _itemsSourceDescriptor.AddValueChanged(grid, OnGridItemsSourceChanged);

        if (_items is not null)
        {
            _tracker = new DataGridColumnTracker(grid, _items);
            _tracker.Changed += OnMetricsChanged;
            _tracker.Attach();
        }

        HookView();
    }

    private void UnhookGrid()
    {
        var grid = TargetGrid;
        if (grid is not null)
        {
            _itemsSourceDescriptor?.RemoveValueChanged(grid, OnGridItemsSourceChanged);
        }

        _itemsSourceDescriptor = null;

        if (_tracker is not null)
        {
            _tracker.Changed -= OnMetricsChanged;
            _tracker.Detach();
            _tracker = null;
        }

        UnhookView();
    }

    /// <summary>The summary follows the filtered view, so filtering re-computes the numbers.</summary>
    private void HookView()
    {
        var view = DataGridFilterChain.ResolveView(TargetGrid);
        if (ReferenceEquals(view, _view)) return;

        UnhookView();
        _view = view;

        if (_view is INotifyCollectionChanged notifier) notifier.CollectionChanged += OnViewChanged;
    }

    private void UnhookView()
    {
        if (_view is INotifyCollectionChanged notifier) notifier.CollectionChanged -= OnViewChanged;
        _view = null;
    }

    private void OnViewChanged(object? sender, NotifyCollectionChangedEventArgs e) => RefreshSummaries();

    private void OnGridItemsSourceChanged(object? sender, EventArgs e)
    {
        HookView();
        RefreshSummaries();
    }

    private void OnMetricsChanged(object? sender, EventArgs e)
    {
        if (Summaries is null || _tracker is null) return;

        foreach (var item in Summaries)
        {
            if (item.Column is null) continue;

            var width = _tracker.WidthOf(item.Column);
            if (Math.Abs(item.Width - width) > 0.5) item.Width = width;
        }
    }

    #endregion

    /// <summary>Recomputes every column's aggregate from the grid's current (filtered) rows.</summary>
    public void RefreshSummaries()
    {
        var grid = TargetGrid;
        if (grid is null)
        {
            Summaries = new ObservableCollection<ColumnSummaryItem>();
            return;
        }

        var rows = new List<object>();
        var view = _view ?? DataGridFilterChain.ResolveView(grid);

        if (view is not null)
        {
            // The view carries a placeholder for the "new row" of an editable grid; it is not data.
            foreach (var row in view)
            {
                if (row is null || ReferenceEquals(row, CollectionView.NewItemPlaceholder)) continue;
                rows.Add(row);
            }
        }
        else if (grid.ItemsSource is not null)
        {
            foreach (var row in grid.ItemsSource)
            {
                if (row is not null) rows.Add(row);
            }
        }

        var columns = _tracker?.OrderedColumns ?? grid.Columns.OrderBy(c => c.DisplayIndex).ToList();
        var list = new ObservableCollection<ColumnSummaryItem>();

        foreach (var column in columns)
        {
            var values = new List<double>(rows.Count);
            var textCells = 0;

            foreach (var row in rows)
            {
                var value = DataGridFilterChain.ReadCell(row, column);
                if (value is null) continue;

                if (TryToDouble(value, out var number)) values.Add(number);
                else textCells++;
            }

            var isNumeric = values.Count > 0 && textCells == 0;

            var mode = GetAggregateOnColumn(column);
            if (mode == DataGridSummaryAggregate.Auto)
            {
                mode = Aggregate == DataGridSummaryAggregate.Auto
                    ? (isNumeric ? DataGridSummaryAggregate.Sum : DataGridSummaryAggregate.Count)
                    : Aggregate;
            }

            var item = new ColumnSummaryItem
            {
                Header = DataGridFilterChain.HeaderText(column),
                Column = column,
                IsNumeric = isNumeric,
                Count = rows.Count,
                Sum = values.Sum(),
                Average = values.Count > 0 ? values.Average() : 0d,
                Min = values.Count > 0 ? values.Min() : 0d,
                Max = values.Count > 0 ? values.Max() : 0d,
                Mode = mode,
                Format = DisplayFormat,
                Width = _tracker?.WidthOf(column) ?? column.ActualWidth,
                DisplayIndex = column.DisplayIndex
            };

            item.DisplayText = Format(item);
            list.Add(item);
        }

        Summaries = list;
    }

    private static string Format(ColumnSummaryItem item)
    {
        if (item.Mode == DataGridSummaryAggregate.Count)
            return string.Format(CultureInfo.CurrentCulture, "Rows {0:N0}", item.Count);

        var value = item.Mode switch
        {
            DataGridSummaryAggregate.Average => item.Average,
            DataGridSummaryAggregate.Min => item.Min,
            DataGridSummaryAggregate.Max => item.Max,
            _ => item.Sum
        };

        var label = item.Mode switch
        {
            DataGridSummaryAggregate.Average => "Avg",
            DataGridSummaryAggregate.Min => "Min",
            DataGridSummaryAggregate.Max => "Max",
            _ => "Sum"
        };

        var format = string.IsNullOrWhiteSpace(item.Format) ? "N1" : item.Format;

        return string.Format(CultureInfo.CurrentCulture, "{0} {1:" + format + "}", label, value);
    }

    /// <summary>
    /// Reads a cell as a number. The text is tried in the current culture first and the invariant
    /// culture second, so a grid bound to <c>"1,5"</c> works on a Vietnamese machine and
    /// <c>"1.5"</c> still works when the data comes from a neutral source.
    /// </summary>
    private static bool TryToDouble(object value, out double result)
    {
        result = 0d;

        switch (value)
        {
            case double d: result = d; return true;
            case float f: result = f; return true;
            case decimal m: result = (double)m; return true;
            case int i: result = i; return true;
            case long l: result = l; return true;
            case short s: result = s; return true;
            case byte b: result = b; return true;
        }

        if (value is IConvertible)
        {
            var text = value.ToString();
            if (double.TryParse(text, NumberStyles.Any, CultureInfo.CurrentCulture, out result)) return true;
            if (double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out result)) return true;
        }

        return false;
    }
}

/// <summary>Aggregate numbers for a single column in the <see cref="DataGridSummary"/> row.</summary>
public class ColumnSummaryItem : INotifyPropertyChanged
{
    private string _header = string.Empty;
    private string _displayText = string.Empty;
    private double _sum;
    private double _average;
    private double _min;
    private double _max;
    private int _count;
    private double _width;
    private int _displayIndex;

    public string Header
    {
        get => _header;
        set { _header = value; OnPropertyChanged(nameof(Header)); }
    }

    /// <summary>What the row prints, e.g. <c>Sum 12 480,0</c> or <c>Rows 24</c>.</summary>
    public string DisplayText
    {
        get => _displayText;
        set { _displayText = value; OnPropertyChanged(nameof(DisplayText)); }
    }

    public double Sum
    {
        get => _sum;
        set { _sum = value; OnPropertyChanged(nameof(Sum)); }
    }

    public double Average
    {
        get => _average;
        set { _average = value; OnPropertyChanged(nameof(Average)); }
    }

    public double Min
    {
        get => _min;
        set { _min = value; OnPropertyChanged(nameof(Min)); }
    }

    public double Max
    {
        get => _max;
        set { _max = value; OnPropertyChanged(nameof(Max)); }
    }

    /// <summary>How many rows the grid currently shows (after filtering).</summary>
    public int Count
    {
        get => _count;
        set { _count = value; OnPropertyChanged(nameof(Count)); }
    }

    /// <summary>True when every value in the column parsed as a number.</summary>
    public bool IsNumeric { get; set; }

    /// <summary>The aggregate this cell shows.</summary>
    public DataGridSummaryAggregate Mode { get; set; }

    /// <summary>Numeric format used by <see cref="DisplayText"/>.</summary>
    public string Format { get; set; } = "N1";

    /// <summary>Width mirrored from the column, so the row lines up with the grid.</summary>
    public double Width
    {
        get => _width;
        set { _width = value; OnPropertyChanged(nameof(Width)); }
    }

    public int DisplayIndex
    {
        get => _displayIndex;
        set { _displayIndex = value; OnPropertyChanged(nameof(DisplayIndex)); }
    }

    /// <summary>The column this cell summarises.</summary>
    public DataGridColumn? Column { get; set; }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged(string prop) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
}
