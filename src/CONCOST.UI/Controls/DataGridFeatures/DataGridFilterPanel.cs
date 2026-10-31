using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace CONCOST.UI.Controls;

/// <summary>
/// A filter row that sits above a <see cref="DataGrid"/>: one box per column, aligned with the
/// column it filters. Typing filters the grid live with a case-insensitive <c>Contains</c> match.
/// </summary>
/// <remarks>
/// The predicate is registered with <see cref="DataGridFilterChain"/>, so it composes with
/// <see cref="DataGridSearchPanel"/> instead of replacing it — a row has to satisfy both.
/// </remarks>
public class DataGridFilterPanel : Control
{
    static DataGridFilterPanel()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(DataGridFilterPanel),
            new FrameworkPropertyMetadata(typeof(DataGridFilterPanel)));
    }

    public DataGridFilterPanel()
    {
        Filters = new ObservableCollection<ColumnFilterItem>();
        CommandBindings.Add(new CommandBinding(ClearFiltersCommand, OnClearFilters));
    }

    #region Dependency Properties

    public static readonly DependencyProperty TargetGridProperty =
        DependencyProperty.Register(nameof(TargetGrid), typeof(DataGrid), typeof(DataGridFilterPanel),
            new FrameworkPropertyMetadata(null, OnTargetGridChanged));

    public DataGrid? TargetGrid
    {
        get => (DataGrid?)GetValue(TargetGridProperty);
        set => SetValue(TargetGridProperty, value);
    }

    public static readonly DependencyProperty FiltersProperty =
        DependencyProperty.Register(nameof(Filters), typeof(ObservableCollection<ColumnFilterItem>), typeof(DataGridFilterPanel),
            new FrameworkPropertyMetadata(null));

    /// <summary>One entry per column of the target grid, in display order.</summary>
    public ObservableCollection<ColumnFilterItem> Filters
    {
        get => (ObservableCollection<ColumnFilterItem>)GetValue(FiltersProperty);
        set => SetValue(FiltersProperty, value);
    }

    public static readonly DependencyProperty PlaceholderTextProperty =
        DependencyProperty.Register(nameof(PlaceholderText), typeof(string), typeof(DataGridFilterPanel),
            new FrameworkPropertyMetadata("Filter…"));

    /// <summary>Placeholder shown in a box whose column has no header text.</summary>
    public string PlaceholderText
    {
        get => (string)GetValue(PlaceholderTextProperty);
        set => SetValue(PlaceholderTextProperty, value);
    }

    #endregion

    public static readonly RoutedUICommand ClearFiltersCommand =
        new RoutedUICommand("Clear filters", nameof(ClearFiltersCommand), typeof(DataGridFilterPanel));

    /// <summary>Raised after the grid has been re-filtered.</summary>
    public event EventHandler? FiltersApplied;

    private ItemsControl? _items;
    private DataGridColumnTracker? _tracker;
    private ICollectionView? _view;
    private DependencyPropertyDescriptor? _itemsSourceDescriptor;

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _items = GetTemplateChild("PART_Items") as ItemsControl;
        HookGrid();
        RefreshFilters();
    }

    protected override void OnInitialized(EventArgs e)
    {
        base.OnInitialized(e);
        Loaded += (_, __) => HookGrid();
        Unloaded += (_, __) => UnhookGrid();
    }

    private static void OnTargetGridChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not DataGridFilterPanel panel) return;

        panel.UnhookGrid();
        panel.HookGrid();
        panel.RefreshFilters();
        panel.ApplyFilters();
    }

    #region Grid plumbing

    private void HookGrid()
    {
        var grid = TargetGrid;
        if (grid is null || _tracker is not null) return;

        grid.Columns.CollectionChanged += OnGridColumnsChanged;

        // A grid is usually bound after the panel, so the view can only be resolved once the
        // ItemsSource actually arrives.
        _itemsSourceDescriptor = DependencyPropertyDescriptor.FromProperty(
            ItemsControl.ItemsSourceProperty, typeof(DataGrid));
        _itemsSourceDescriptor.AddValueChanged(grid, OnGridItemsSourceChanged);

        if (_items is not null)
        {
            _tracker = new DataGridColumnTracker(grid, _items);
            _tracker.Changed += OnMetricsChanged;
            _tracker.Attach();
        }
    }

    private void UnhookGrid()
    {
        var grid = TargetGrid;
        if (grid is not null)
        {
            grid.Columns.CollectionChanged -= OnGridColumnsChanged;
            _itemsSourceDescriptor?.RemoveValueChanged(grid, OnGridItemsSourceChanged);
        }

        _itemsSourceDescriptor = null;

        if (_tracker is not null)
        {
            _tracker.Changed -= OnMetricsChanged;
            _tracker.Detach();
            _tracker = null;
        }

        DataGridFilterChain.Clear(_view, this);
        _view = null;
    }

    private void OnGridColumnsChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        RefreshFilters();
        ApplyFilters();
    }

    private void OnGridItemsSourceChanged(object? sender, EventArgs e)
    {
        ApplyFilters();
    }

    private void OnMetricsChanged(object? sender, EventArgs e)
    {
        if (Filters is null || _tracker is null) return;

        foreach (var item in Filters)
        {
            if (item.Column is null) continue;

            var width = _tracker.WidthOf(item.Column);
            if (Math.Abs(item.Width - width) > 0.5) item.Width = width;
        }
    }

    #endregion

    /// <summary>Rebuilds the box list from the target grid's columns.</summary>
    public void RefreshFilters()
    {
        if (Filters is not null)
        {
            foreach (var item in Filters) item.PropertyChanged -= OnFilterItemChanged;
        }

        var list = new ObservableCollection<ColumnFilterItem>();
        var columns = _tracker?.OrderedColumns ?? TargetGrid?.Columns;

        if (columns is not null)
        {
            foreach (var column in columns)
            {
                var item = new ColumnFilterItem
                {
                    Header = DataGridFilterChain.HeaderText(column),
                    Column = column,
                    Placeholder = string.IsNullOrEmpty(DataGridFilterChain.HeaderText(column))
                        ? PlaceholderText
                        : DataGridFilterChain.HeaderText(column),
                    Width = _tracker?.WidthOf(column) ?? 140d
                };
                item.PropertyChanged += OnFilterItemChanged;
                list.Add(item);
            }
        }

        Filters = list;
    }

    private void OnFilterItemChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ColumnFilterItem.FilterText)) ApplyFilters();
    }

    /// <summary>Pushes the current box contents onto the grid's collection view.</summary>
    public void ApplyFilters()
    {
        var view = DataGridFilterChain.ResolveView(TargetGrid);
        if (view is null)
        {
            DataGridFilterChain.Clear(_view, this);
            _view = null;
            return;
        }

        if (!ReferenceEquals(view, _view))
        {
            DataGridFilterChain.Clear(_view, this);
            _view = view;
        }

        var active = false;
        if (Filters is not null)
        {
            foreach (var item in Filters)
            {
                if (!string.IsNullOrWhiteSpace(item.FilterText))
                {
                    active = true;
                    break;
                }
            }
        }

        if (!active)
        {
            DataGridFilterChain.Set(view, this, null);
        }
        else
        {
            var filters = Filters ?? new ObservableCollection<ColumnFilterItem>();

            DataGridFilterChain.Set(view, this, row =>
            {
                foreach (var item in filters)
                {
                    var text = item.FilterText;
                    if (string.IsNullOrWhiteSpace(text)) continue;

                    var value = DataGridFilterChain.ReadCell(row, item.Column);
                    var cell = value?.ToString() ?? string.Empty;
                    if (cell.IndexOf(text.Trim(), StringComparison.OrdinalIgnoreCase) < 0) return false;
                }

                return true;
            });
        }

        FiltersApplied?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Empties every box and restores the unfiltered grid.</summary>
    public void ClearFilters()
    {
        if (Filters is not null)
        {
            foreach (var item in Filters) item.FilterText = string.Empty;
        }

        ApplyFilters();
    }

    private void OnClearFilters(object sender, ExecutedRoutedEventArgs e) => ClearFilters();
}

/// <summary>A single filter box inside the <see cref="DataGridFilterPanel"/>.</summary>
public class ColumnFilterItem : INotifyPropertyChanged
{
    private string _header = string.Empty;
    private string _filterText = string.Empty;
    private string _placeholder = string.Empty;
    private double _width;

    public string Header
    {
        get => _header;
        set { _header = value; OnPropertyChanged(nameof(Header)); }
    }

    /// <summary>What the user typed. Setting it re-filters the grid immediately.</summary>
    public string FilterText
    {
        get => _filterText;
        set { _filterText = value; OnPropertyChanged(nameof(FilterText)); }
    }

    /// <summary>Hint shown while the box is empty — normally the column header.</summary>
    public string Placeholder
    {
        get => _placeholder;
        set { _placeholder = value; OnPropertyChanged(nameof(Placeholder)); }
    }

    /// <summary>Width mirrored from the column, so the box sits exactly under it.</summary>
    public double Width
    {
        get => _width;
        set { _width = value; OnPropertyChanged(nameof(Width)); }
    }

    /// <summary>The column this box filters.</summary>
    public DataGridColumn? Column { get; set; }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged(string prop) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
}
