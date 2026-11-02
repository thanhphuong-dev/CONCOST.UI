using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace CONCOST.UI.Controls;

/// <summary>
/// A panel that sits above a <see cref="DataGrid"/> and lets the user group rows by one or more
/// columns. Columns are picked from a drop-down; each selected column becomes a pill that can be
/// removed or reordered. The underlying grouping is applied through
/// <see cref="ICollectionView.GroupDescriptions"/>.
/// </summary>
public class DataGridGroupPanel : Control
{
    static DataGridGroupPanel()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(DataGridGroupPanel),
            new FrameworkPropertyMetadata(typeof(DataGridGroupPanel)));
    }

    public DataGridGroupPanel()
    {
        Groups = new ObservableCollection<GroupDescriptor>();
        AvailableColumns = new ObservableCollection<ColumnGroupCandidate>();
        CommandBindings.Add(new CommandBinding(RemoveGroupCommand, OnRemoveGroup));
        CommandBindings.Add(new CommandBinding(AddGroupCommand, OnAddGroup));
        CommandBindings.Add(new CommandBinding(ClearGroupsCommand, OnClearGroups));
    }

    #region Dependency Properties

    public static readonly DependencyProperty TargetGridProperty =
        DependencyProperty.Register(nameof(TargetGrid), typeof(DataGrid), typeof(DataGridGroupPanel),
            new FrameworkPropertyMetadata(null, OnTargetGridChanged));

    public DataGrid? TargetGrid
    {
        get => (DataGrid?)GetValue(TargetGridProperty);
        set => SetValue(TargetGridProperty, value);
    }

    public static readonly DependencyProperty GroupsProperty =
        DependencyProperty.Register(nameof(Groups), typeof(ObservableCollection<GroupDescriptor>), typeof(DataGridGroupPanel),
            new FrameworkPropertyMetadata(null));

    /// <summary>The current group hierarchy, outermost first.</summary>
    public ObservableCollection<GroupDescriptor> Groups
    {
        get => (ObservableCollection<GroupDescriptor>)GetValue(GroupsProperty);
        set => SetValue(GroupsProperty, value);
    }

    public static readonly DependencyProperty AvailableColumnsProperty =
        DependencyProperty.Register(nameof(AvailableColumns), typeof(ObservableCollection<ColumnGroupCandidate>), typeof(DataGridGroupPanel),
            new FrameworkPropertyMetadata(null));

    /// <summary>Columns of the target grid that are not currently grouped.</summary>
    public ObservableCollection<ColumnGroupCandidate> AvailableColumns
    {
        get => (ObservableCollection<ColumnGroupCandidate>)GetValue(AvailableColumnsProperty);
        set => SetValue(AvailableColumnsProperty, value);
    }

    public static readonly DependencyProperty SelectedColumnProperty =
        DependencyProperty.Register(nameof(SelectedColumn), typeof(ColumnGroupCandidate), typeof(DataGridGroupPanel),
            new FrameworkPropertyMetadata(null));

    /// <summary>The column currently selected in the drop-down, ready to be added.</summary>
    public ColumnGroupCandidate? SelectedColumn
    {
        get => (ColumnGroupCandidate?)GetValue(SelectedColumnProperty);
        set => SetValue(SelectedColumnProperty, value);
    }

    public static readonly DependencyProperty HeaderTextProperty =
        DependencyProperty.Register(nameof(HeaderText), typeof(string), typeof(DataGridGroupPanel),
            new FrameworkPropertyMetadata("Group by"));

    public string HeaderText
    {
        get => (string)GetValue(HeaderTextProperty);
        set => SetValue(HeaderTextProperty, value);
    }

    public static readonly DependencyProperty EmptyTextProperty =
        DependencyProperty.Register(nameof(EmptyText), typeof(string), typeof(DataGridGroupPanel),
            new FrameworkPropertyMetadata("Select a column to group rows"));

    public string EmptyText
    {
        get => (string)GetValue(EmptyTextProperty);
        set => SetValue(EmptyTextProperty, value);
    }

    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(DataGridGroupPanel),
            new FrameworkPropertyMetadata(new CornerRadius(6)));

    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    #endregion

    public static readonly RoutedUICommand RemoveGroupCommand =
        new RoutedUICommand("Remove group", nameof(RemoveGroupCommand), typeof(DataGridGroupPanel));

    public static readonly RoutedUICommand AddGroupCommand =
        new RoutedUICommand("Add group", nameof(AddGroupCommand), typeof(DataGridGroupPanel));

    public static readonly RoutedUICommand ClearGroupsCommand =
        new RoutedUICommand("Clear all groups", nameof(ClearGroupsCommand), typeof(DataGridGroupPanel));

    /// <summary>Raised after the grouping has changed.</summary>
    public event EventHandler? GroupsChanged;

    private static void OnTargetGridChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not DataGridGroupPanel panel) return;

        panel.DetachGrid(e.OldValue as DataGrid);
        panel.AttachGrid(e.NewValue as DataGrid);
        panel.RefreshAvailableColumns();
        panel.ApplyGrouping();
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
        RefreshAvailableColumns();
    }

    /// <summary>Rebuilds the available-column list from the grid, excluding already-grouped columns.</summary>
    public void RefreshAvailableColumns()
    {
        var grid = TargetGrid;
        if (grid is null)
        {
            AvailableColumns.Clear();
            return;
        }

        var groupedPaths = Groups.Select(g => g.PropertyPath).ToHashSet();
        var list = new ObservableCollection<ColumnGroupCandidate>();

        foreach (var column in grid.Columns)
        {
            var path = GetBindingPath(column);
            if (string.IsNullOrEmpty(path)) continue;
            if (groupedPaths.Contains(path!)) continue;

            list.Add(new ColumnGroupCandidate
            {
                Header = DataGridFilterChain.HeaderText(column),
                PropertyPath = path,
                Column = column
            });
        }

        AvailableColumns = list;
        SelectedColumn = list.FirstOrDefault();
    }

    /// <summary>Adds a group by the given column property path.</summary>
    public void AddGroup(string propertyPath, string header)
    {
        if (string.IsNullOrEmpty(propertyPath)) return;
        if (Groups.Any(g => g.PropertyPath == propertyPath)) return;

        Groups.Add(new GroupDescriptor
        {
            Header = header,
            PropertyPath = propertyPath,
            Level = Groups.Count + 1
        });

        RefreshAvailableColumns();
        ApplyGrouping();
        GroupsChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Removes a specific group descriptor.</summary>
    public void RemoveGroup(GroupDescriptor descriptor)
    {
        if (!Groups.Contains(descriptor)) return;

        Groups.Remove(descriptor);
        ReindexLevels();
        RefreshAvailableColumns();
        ApplyGrouping();
        GroupsChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Removes all grouping.</summary>
    public void ClearGroups()
    {
        Groups.Clear();
        RefreshAvailableColumns();
        ApplyGrouping();
        GroupsChanged?.Invoke(this, EventArgs.Empty);
    }

    private void ReindexLevels()
    {
        for (int i = 0; i < Groups.Count; i++)
            Groups[i].Level = i + 1;
    }

    private void ApplyGrouping()
    {
        var view = DataGridFilterChain.ResolveView(TargetGrid);
        if (view is null) return;

        using (view.DeferRefresh())
        {
            view.GroupDescriptions.Clear();
            foreach (var group in Groups)
            {
                if (!string.IsNullOrEmpty(group.PropertyPath))
                    view.GroupDescriptions.Add(new PropertyGroupDescription(group.PropertyPath));
            }
        }
    }

    private void OnRemoveGroup(object sender, ExecutedRoutedEventArgs e)
    {
        if (e.Parameter is GroupDescriptor desc) RemoveGroup(desc);
    }

    private void OnAddGroup(object sender, ExecutedRoutedEventArgs e)
    {
        if (SelectedColumn is null) return;
        AddGroup(SelectedColumn.PropertyPath, SelectedColumn.Header);
    }

    private void OnClearGroups(object sender, ExecutedRoutedEventArgs e) => ClearGroups();

    private static string? GetBindingPath(DataGridColumn column)
    {
        if (column is not DataGridBoundColumn bound) return null;
        if (bound.Binding is not Binding binding) return null;
        return binding.Path?.Path;
    }
}

/// <summary>A column that can be chosen for grouping.</summary>
public class ColumnGroupCandidate
{
    public string Header { get; set; } = string.Empty;
    public string PropertyPath { get; set; } = string.Empty;
    public DataGridColumn? Column { get; set; }
}

/// <summary>One level of grouping inside the <see cref="DataGridGroupPanel"/>.</summary>
public class GroupDescriptor : INotifyPropertyChanged
{
    private string _header = string.Empty;
    private string _propertyPath = string.Empty;
    private int _level;

    public string Header
    {
        get => _header;
        set { _header = value; OnPropertyChanged(nameof(Header)); }
    }

    public string PropertyPath
    {
        get => _propertyPath;
        set { _propertyPath = value; OnPropertyChanged(nameof(PropertyPath)); }
    }

    public int Level
    {
        get => _level;
        set { _level = value; OnPropertyChanged(nameof(Level)); }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged(string prop) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
}
