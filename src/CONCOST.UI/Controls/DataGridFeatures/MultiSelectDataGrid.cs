using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace CONCOST.UI.Controls;

/// <summary>
/// A <see cref="DataGrid"/> with a checkbox column for multi-selection, plus a tri-state header
/// checkbox that selects or clears everything.
/// </summary>
/// <remarks>
/// <para>
/// A plain click in the checkbox column toggles that one row instead of replacing the selection, so
/// picking twenty rows does not need the Ctrl key. Clicking anywhere else in the row keeps the
/// normal DataGrid behaviour (plain click selects, Ctrl and Shift extend).
/// </para>
/// <para>
/// The selection is also published as a plain list (<see cref="SelectedItemsList"/>) that raises
/// change notifications, because <see cref="DataGrid.SelectedItems"/> does not — that list is what a
/// command can bind to.
/// </para>
/// </remarks>
public class MultiSelectDataGrid : DataGrid
{
    static MultiSelectDataGrid()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(MultiSelectDataGrid),
            new FrameworkPropertyMetadata(typeof(MultiSelectDataGrid)));
    }

    private readonly ObservableCollection<object> _selected = new();
    private DataGridTemplateColumn? _checkBoxColumn;
    private bool _syncing;

    public MultiSelectDataGrid()
    {
        SelectionMode = DataGridSelectionMode.Extended;
        SelectionUnit = DataGridSelectionUnit.FullRow;
        SelectionChanged += (_, _) => SyncSelection();
        ApplyCheckBoxColumn();
    }

    // ---------------------------------------------------------------- checkbox column

    public static readonly DependencyProperty ShowCheckBoxColumnProperty =
        DependencyProperty.Register(nameof(ShowCheckBoxColumn), typeof(bool), typeof(MultiSelectDataGrid),
            new PropertyMetadata(true, OnCheckBoxColumnChanged));

    /// <summary>Draw the checkbox column. Default <c>true</c>.</summary>
    public bool ShowCheckBoxColumn
    {
        get => (bool)GetValue(ShowCheckBoxColumnProperty);
        set => SetValue(ShowCheckBoxColumnProperty, value);
    }

    public static readonly DependencyProperty CheckBoxColumnWidthProperty =
        DependencyProperty.Register(nameof(CheckBoxColumnWidth), typeof(double), typeof(MultiSelectDataGrid),
            new PropertyMetadata(44.0, OnCheckBoxColumnChanged));

    /// <summary>Width of the checkbox column, in pixels. Default 44.</summary>
    public double CheckBoxColumnWidth
    {
        get => (double)GetValue(CheckBoxColumnWidthProperty);
        set => SetValue(CheckBoxColumnWidthProperty, value);
    }

    // ---------------------------------------------------------------- selection state

    public static readonly DependencyProperty IsAllSelectedProperty =
        DependencyProperty.Register(nameof(IsAllSelected), typeof(bool?), typeof(MultiSelectDataGrid),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnIsAllSelectedChanged));

    /// <summary>
    /// Header checkbox state: <c>true</c> when every row is selected, <c>false</c> when none is,
    /// <c>null</c> when only some are. Setting it selects or clears everything.
    /// </summary>
    public bool? IsAllSelected
    {
        get => (bool?)GetValue(IsAllSelectedProperty);
        set => SetValue(IsAllSelectedProperty, value);
    }

    private static readonly DependencyPropertyKey SelectedCountPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(SelectedCount), typeof(int), typeof(MultiSelectDataGrid),
            new PropertyMetadata(0));

    public static readonly DependencyProperty SelectedCountProperty = SelectedCountPropertyKey.DependencyProperty;

    /// <summary>How many rows are selected.</summary>
    public int SelectedCount => (int)GetValue(SelectedCountProperty);

    /// <summary>The selected items, in selection order, with change notifications.</summary>
    public IList SelectedItemsList => _selected;

    // ---------------------------------------------------------------- public API

    /// <summary>Selects every row.</summary>
    public void SelectEverything()
    {
        if (Items.Count == 0) return;

        _syncing = true;
        try
        {
            SelectAll();
        }
        finally
        {
            _syncing = false;
        }

        SyncSelection();
    }

    /// <summary>Clears the selection.</summary>
    public void ClearEverything()
    {
        _syncing = true;
        try
        {
            UnselectAll();
        }
        finally
        {
            _syncing = false;
        }

        SyncSelection();
    }

    /// <summary>Flips the selected state of every row.</summary>
    public void InvertSelection()
    {
        var current = new System.Collections.Generic.HashSet<object>();
        foreach (var item in SelectedItems)
        {
            if (item is not null) current.Add(item);
        }

        _syncing = true;
        try
        {
            foreach (var item in Items)
            {
                if (item is null) continue;

                if (current.Contains(item)) SelectedItems.Remove(item);
                else SelectedItems.Add(item);
            }
        }
        finally
        {
            _syncing = false;
        }

        SyncSelection();
    }

    /// <summary>Toggles one row, leaving the rest of the selection alone.</summary>
    public void ToggleItem(object? item)
    {
        if (item is null) return;

        if (SelectedItems.Contains(item)) SelectedItems.Remove(item);
        else SelectedItems.Add(item);
    }

    /// <summary>True when the row is selected.</summary>
    public bool IsItemSelected(object? item) => item is not null && SelectedItems.Contains(item);

    // ---------------------------------------------------------------- internals

    private static void OnIsAllSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not MultiSelectDataGrid grid || grid._syncing) return;

        switch (e.NewValue)
        {
            case true:
                grid.SelectEverything();
                break;
            case false:
                grid.ClearEverything();
                break;
        }
    }

    private static void OnCheckBoxColumnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is MultiSelectDataGrid grid) grid.ApplyCheckBoxColumn();
    }

    private void SyncSelection()
    {
        if (_syncing) return;

        _syncing = true;
        try
        {
            _selected.Clear();
            foreach (var item in SelectedItems)
            {
                if (item is not null) _selected.Add(item);
            }

            SetValue(SelectedCountPropertyKey, _selected.Count);

            bool? all = _selected.Count == 0
                ? false
                : _selected.Count >= Items.Count ? true : null;

            SetCurrentValue(IsAllSelectedProperty, all);
        }
        finally
        {
            _syncing = false;
        }
    }

    protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        if (IsCheckBoxClick(e.OriginalSource as DependencyObject, out var row) && row is not null)
        {
            ToggleItem(row.DataContext);
            e.Handled = true;
            return;
        }

        base.OnPreviewMouseLeftButtonDown(e);
    }

    private bool IsCheckBoxClick(DependencyObject? source, out DataGridRow? row)
    {
        row = null;
        if (_checkBoxColumn is null || source is null) return false;

        var current = source;
        while (current is not null)
        {
            if (current is DataGridCell cell)
            {
                if (!ReferenceEquals(cell.Column, _checkBoxColumn)) return false;
                row = FindParent<DataGridRow>(cell);
                return row is not null;
            }

            if (current is DataGridRow found)
            {
                row = found;
                return true;
            }

            current = VisualTreeHelper.GetParent(current);
        }

        return false;
    }

    private static T? FindParent<T>(DependencyObject start) where T : DependencyObject
    {
        var current = start;
        while (current is not null)
        {
            if (current is T match) return match;
            current = VisualTreeHelper.GetParent(current);
        }
        return null;
    }

    private void ApplyCheckBoxColumn()
    {
        if (ShowCheckBoxColumn)
        {
            if (_checkBoxColumn is null)
            {
                _checkBoxColumn = BuildCheckBoxColumn();
                Columns.Insert(0, _checkBoxColumn);
            }
            else
            {
                _checkBoxColumn.Width = new DataGridLength(CheckBoxColumnWidth);
            }
        }
        else if (_checkBoxColumn is not null)
        {
            Columns.Remove(_checkBoxColumn);
            _checkBoxColumn = null;
        }
    }

    private DataGridTemplateColumn BuildCheckBoxColumn()
    {
        var check = new FrameworkElementFactory(typeof(CheckBox));
        check.SetValue(FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Center);
        check.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);
        // The click must land on the row, not on the box: that is what makes a plain click toggle.
        check.SetValue(UIElement.IsHitTestVisibleProperty, false);
        check.SetBinding(ToggleButton.IsCheckedProperty, new Binding
        {
            Path = new PropertyPath(DataGridRow.IsSelectedProperty),
            Mode = BindingMode.OneWay,
            RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(DataGridRow), 1)
        });

        var header = new FrameworkElementFactory(typeof(CheckBox));
        header.SetValue(FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Center);
        header.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);
        header.SetBinding(ToggleButton.IsCheckedProperty, new Binding
        {
            Path = new PropertyPath(IsAllSelectedProperty),
            Mode = BindingMode.TwoWay,
            RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(MultiSelectDataGrid), 1)
        });

        return new DataGridTemplateColumn
        {
            Width = new DataGridLength(CheckBoxColumnWidth),
            CellTemplate = new DataTemplate { VisualTree = check },
            HeaderTemplate = new DataTemplate { VisualTree = header }
        };
    }
}
