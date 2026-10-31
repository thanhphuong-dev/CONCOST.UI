using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CONCOST.UI.Controls;

/// <summary>
/// A drop-down that toggles the visibility of individual columns of a <see cref="DataGrid"/>.
/// The list is discovered from the grid and follows it when columns are added or removed.
/// </summary>
public class DataGridColumnChooser : Control
{
    static DataGridColumnChooser()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(DataGridColumnChooser),
            new FrameworkPropertyMetadata(typeof(DataGridColumnChooser)));
    }

    public DataGridColumnChooser()
    {
        Columns = new ObservableCollection<ColumnChooserItem>();
        CommandBindings.Add(new CommandBinding(ToggleColumnCommand, OnToggleColumn));
    }

    #region Dependency Properties

    public static readonly DependencyProperty TargetGridProperty =
        DependencyProperty.Register(nameof(TargetGrid), typeof(DataGrid), typeof(DataGridColumnChooser),
            new FrameworkPropertyMetadata(null, OnTargetGridChanged));

    public DataGrid? TargetGrid
    {
        get => (DataGrid?)GetValue(TargetGridProperty);
        set => SetValue(TargetGridProperty, value);
    }

    public static readonly DependencyProperty IsOpenProperty =
        DependencyProperty.Register(nameof(IsOpen), typeof(bool), typeof(DataGridColumnChooser),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnIsOpenChanged));

    /// <summary>Whether the column list is showing. Two-way, so the drop-down can close itself.</summary>
    public bool IsOpen
    {
        get => (bool)GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    public static readonly DependencyProperty ColumnsProperty =
        DependencyProperty.Register(nameof(Columns), typeof(ObservableCollection<ColumnChooserItem>), typeof(DataGridColumnChooser),
            new FrameworkPropertyMetadata(null));

    /// <summary>One entry per column of the target grid, in column order.</summary>
    public ObservableCollection<ColumnChooserItem> Columns
    {
        get => (ObservableCollection<ColumnChooserItem>)GetValue(ColumnsProperty);
        set => SetValue(ColumnsProperty, value);
    }

    public static readonly DependencyProperty HeaderTextProperty =
        DependencyProperty.Register(nameof(HeaderText), typeof(string), typeof(DataGridColumnChooser),
            new FrameworkPropertyMetadata("Columns"));

    /// <summary>Text on the drop-down button and above the list.</summary>
    public string HeaderText
    {
        get => (string)GetValue(HeaderTextProperty);
        set => SetValue(HeaderTextProperty, value);
    }

    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(DataGridColumnChooser),
            new FrameworkPropertyMetadata(new CornerRadius(6)));

    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    #endregion

    public static readonly RoutedUICommand ToggleColumnCommand =
        new RoutedUICommand("Toggle column", nameof(ToggleColumnCommand), typeof(DataGridColumnChooser));

    /// <summary>Raised after a column's visibility changed.</summary>
    public event EventHandler? ColumnsChanged;

    private static void OnTargetGridChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not DataGridColumnChooser chooser) return;

        chooser.DetachGrid(e.OldValue as DataGrid);
        chooser.AttachGrid(e.NewValue as DataGrid);
        chooser.RefreshColumns();
    }

    private static void OnIsOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        // Re-read the grid on open: the host may have hidden a column from its own menu since
        // the list was last built.
        if (d is DataGridColumnChooser chooser && (bool)e.NewValue) chooser.RefreshColumns();
    }

    private void AttachGrid(DataGrid? grid)
    {
        if (grid is null) return;
        grid.Columns.CollectionChanged += OnGridColumnsChanged;
    }

    private void DetachGrid(DataGrid? grid)
    {
        if (grid is null) return;
        grid.Columns.CollectionChanged -= OnGridColumnsChanged;
    }

    private void OnGridColumnsChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        RefreshColumns();
    }

    private void OnToggleColumn(object sender, ExecutedRoutedEventArgs e)
    {
        switch (e.Parameter)
        {
            case string command when command == "SHOW_ALL":
                ShowAll();
                return;
            case string command when command == "HIDE_ALL":
                HideAll();
                return;
            case ColumnChooserItem item:
                ApplyVisibility(item);
                return;
        }
    }

    /// <summary>Rebuilds the list from the target grid's columns.</summary>
    public void RefreshColumns()
    {
        if (Columns is not null)
        {
            foreach (var item in Columns) item.PropertyChanged -= OnItemChanged;
        }

        var list = new ObservableCollection<ColumnChooserItem>();

        if (TargetGrid is not null)
        {
            foreach (var column in TargetGrid.Columns)
            {
                var item = new ColumnChooserItem
                {
                    Header = DataGridFilterChain.HeaderText(column),
                    IsVisible = column.Visibility == Visibility.Visible,
                    Column = column
                };
                item.PropertyChanged += OnItemChanged;
                list.Add(item);
            }
        }

        Columns = list;
    }

    /// <summary>
    /// Applying from the item rather than from a routed command keeps the check boxes working
    /// inside the pop-up, where command routing is easy to break.
    /// </summary>
    private void OnItemChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(ColumnChooserItem.IsVisible)) return;
        if (sender is ColumnChooserItem item) ApplyVisibility(item);
    }

    private void ApplyVisibility(ColumnChooserItem item)
    {
        // The column reference is authoritative: two columns may share a header text.
        var column = item.Column ?? TargetGrid?.Columns.FirstOrDefault(
            c => DataGridFilterChain.HeaderText(c) == item.Header);

        if (column is null) return;

        column.Visibility = item.IsVisible ? Visibility.Visible : Visibility.Collapsed;
        ColumnsChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Shows every column.</summary>
    public void ShowAll()
    {
        if (TargetGrid is null) return;

        foreach (var column in TargetGrid.Columns) column.Visibility = Visibility.Visible;

        RefreshColumns();
        ColumnsChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Hides every column. The “Show all” row stays available to bring them back.</summary>
    public void HideAll()
    {
        if (TargetGrid is null) return;

        foreach (var column in TargetGrid.Columns) column.Visibility = Visibility.Collapsed;

        RefreshColumns();
        ColumnsChanged?.Invoke(this, EventArgs.Empty);
    }
}

/// <summary>A single row inside the <see cref="DataGridColumnChooser"/> list.</summary>
public class ColumnChooserItem : INotifyPropertyChanged
{
    private string _header = string.Empty;
    private bool _isVisible = true;

    public string Header
    {
        get => _header;
        set { _header = value; OnPropertyChanged(nameof(Header)); }
    }

    /// <summary>Ticked when the column is visible. Setting it shows or hides the column.</summary>
    public bool IsVisible
    {
        get => _isVisible;
        set { _isVisible = value; OnPropertyChanged(nameof(IsVisible)); }
    }

    /// <summary>The column this row controls.</summary>
    public DataGridColumn? Column { get; set; }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged(string prop) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
}
