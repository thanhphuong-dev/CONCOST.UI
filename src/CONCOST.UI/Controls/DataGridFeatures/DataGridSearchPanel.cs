using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace CONCOST.UI.Controls;

/// <summary>
/// A single search box that filters every visible column of a <see cref="DataGrid"/> at once.
/// The match is a case-insensitive <c>Contains</c> on each cell's text.
/// </summary>
/// <remarks>
/// The predicate is registered with <see cref="DataGridFilterChain"/>, so a search narrows the
/// rows a <see cref="DataGridFilterPanel"/> has already kept instead of cancelling it.
/// </remarks>
public class DataGridSearchPanel : Control
{
    static DataGridSearchPanel()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(DataGridSearchPanel),
            new FrameworkPropertyMetadata(typeof(DataGridSearchPanel)));
    }

    public DataGridSearchPanel()
    {
        CommandBindings.Add(new CommandBinding(ClearSearchCommand, OnClearSearch));
    }

    #region Dependency Properties

    public static readonly DependencyProperty TargetGridProperty =
        DependencyProperty.Register(nameof(TargetGrid), typeof(DataGrid), typeof(DataGridSearchPanel),
            new FrameworkPropertyMetadata(null, OnTargetGridChanged));

    public DataGrid? TargetGrid
    {
        get => (DataGrid?)GetValue(TargetGridProperty);
        set => SetValue(TargetGridProperty, value);
    }

    public static readonly DependencyProperty SearchTextProperty =
        DependencyProperty.Register(nameof(SearchText), typeof(string), typeof(DataGridSearchPanel),
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSearchTextChanged));

    public string SearchText
    {
        get => (string)GetValue(SearchTextProperty);
        set => SetValue(SearchTextProperty, value);
    }

    public static readonly DependencyProperty PlaceholderTextProperty =
        DependencyProperty.Register(nameof(PlaceholderText), typeof(string), typeof(DataGridSearchPanel),
            new FrameworkPropertyMetadata("Search…"));

    public string PlaceholderText
    {
        get => (string)GetValue(PlaceholderTextProperty);
        set => SetValue(PlaceholderTextProperty, value);
    }

    private static readonly DependencyPropertyKey IsSearchingPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(IsSearching), typeof(bool), typeof(DataGridSearchPanel),
            new FrameworkPropertyMetadata(false));

    /// <summary>True while a search term is active — the template uses it to show the clear button.</summary>
    public static readonly DependencyProperty IsSearchingProperty = IsSearchingPropertyKey.DependencyProperty;

    public bool IsSearching => (bool)GetValue(IsSearchingProperty);

    #endregion

    public static readonly RoutedUICommand ClearSearchCommand =
        new RoutedUICommand("Clear search", nameof(ClearSearchCommand), typeof(DataGridSearchPanel));

    /// <summary>Raised after the grid has been re-filtered.</summary>
    public event EventHandler? SearchChanged;

    private ICollectionView? _view;
    private DependencyPropertyDescriptor? _itemsSourceDescriptor;

    private static void OnTargetGridChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not DataGridSearchPanel panel) return;

        panel.DetachGrid(e.OldValue as DataGrid);
        panel.AttachGrid(e.NewValue as DataGrid);
        panel.ApplySearch();
    }

    private static void OnSearchTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DataGridSearchPanel panel) panel.ApplySearch();
    }

    private void AttachGrid(DataGrid? grid)
    {
        if (grid is null) return;

        _itemsSourceDescriptor = DependencyPropertyDescriptor.FromProperty(
            ItemsControl.ItemsSourceProperty, typeof(DataGrid));
        _itemsSourceDescriptor.AddValueChanged(grid, OnGridItemsSourceChanged);
    }

    private void DetachGrid(DataGrid? grid)
    {
        if (grid is null) return;

        _itemsSourceDescriptor?.RemoveValueChanged(grid, OnGridItemsSourceChanged);
        _itemsSourceDescriptor = null;

        DataGridFilterChain.Clear(_view, this);
        _view = null;
    }

    private void OnGridItemsSourceChanged(object? sender, EventArgs e)
    {
        ApplySearch();
    }

    private void OnClearSearch(object sender, ExecutedRoutedEventArgs e) => ClearSearch();

    /// <summary>Empties the box and restores the rows the search was hiding.</summary>
    public void ClearSearch() => SearchText = string.Empty;

    /// <summary>Pushes the current search term onto the grid's collection view.</summary>
    public void ApplySearch()
    {
        var text = SearchText?.Trim() ?? string.Empty;
        SetValue(IsSearchingPropertyKey, text.Length > 0);

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

        if (text.Length == 0)
        {
            DataGridFilterChain.Set(view, this, null);
        }
        else
        {
            DataGridFilterChain.Set(view, this, row =>
            {
                var grid = TargetGrid;
                if (grid is null) return true;

                foreach (var column in grid.Columns)
                {
                    if (column.Visibility != Visibility.Visible) continue;

                    var value = DataGridFilterChain.ReadCell(row, column);
                    if (value is null) continue;

                    if (value.ToString()?.IndexOf(text, StringComparison.OrdinalIgnoreCase) >= 0) return true;
                }

                return false;
            });
        }

        SearchChanged?.Invoke(this, EventArgs.Empty);
    }
}
