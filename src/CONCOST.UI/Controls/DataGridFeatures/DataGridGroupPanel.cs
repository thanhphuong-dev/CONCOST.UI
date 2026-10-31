using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using CONCOST.UI.Core.MVVM;
using CONCOST.UI.Icons;

namespace CONCOST.UI.Controls;

public class DataGridGroupPanel : Control
{
    public static readonly DependencyProperty TargetGridProperty =
        DependencyProperty.Register(nameof(TargetGrid), typeof(StackedHeaderDataGrid), typeof(DataGridGroupPanel),
            new PropertyMetadata(null, OnTargetGridChanged));

    public StackedHeaderDataGrid? TargetGrid
    {
        get => (StackedHeaderDataGrid?)GetValue(TargetGridProperty);
        set => SetValue(TargetGridProperty, value);
    }

    public static readonly DependencyProperty EmptyMessageProperty =
        DependencyProperty.Register(nameof(EmptyMessage), typeof(string), typeof(DataGridGroupPanel),
            new PropertyMetadata("Drag a column header here to group by that column..."));

    public string EmptyMessage
    {
        get => (string)GetValue(EmptyMessageProperty);
        set => SetValue(EmptyMessageProperty, value);
    }

    public ObservableCollection<string> GroupedPropertyNames { get; } = new();

    private static void OnTargetGridChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var panel = (DataGridGroupPanel)d;
        if (e.NewValue is StackedHeaderDataGrid grid)
        {
            panel.SyncWithGrid(grid);
        }
    }

    static DataGridGroupPanel()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(DataGridGroupPanel), new FrameworkPropertyMetadata(typeof(DataGridGroupPanel)));
    }

    public DataGridGroupPanel()
    {
        AllowDrop = true;
        GroupedPropertyNames.CollectionChanged += OnGroupedPropertiesChanged;
        Drop += OnDrop;
        DragOver += OnDragOver;
    }

    private void OnDragOver(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.StringFormat) || e.Data.GetDataPresent(typeof(string)))
        {
            e.Effects = DragDropEffects.Move;
            e.Handled = true;
        }
    }

    private void OnDrop(object sender, DragEventArgs e)
    {
        string? propName = null;
        if (e.Data.GetDataPresent(DataFormats.StringFormat))
            propName = e.Data.GetData(DataFormats.StringFormat) as string;
        else if (e.Data.GetDataPresent(typeof(string)))
            propName = e.Data.GetData(typeof(string)) as string;

        // Pattern instead of string.IsNullOrEmpty: net48 has no [NotNullWhen] on the BCL helper,
        // so the null-state would not flow into AddGroup/RemoveGroup and CS8604 would fire.
        if (propName is { Length: > 0 })
        {
            AddGroup(propName);
            e.Handled = true;
        }
    }

    private void OnGroupedPropertiesChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (TargetGrid?.ItemsSource == null) return;
        var view = CollectionViewSource.GetDefaultView(TargetGrid.ItemsSource);
        if (view == null) return;

        using (view.DeferRefresh())
        {
            view.GroupDescriptions.Clear();
            foreach (var prop in GroupedPropertyNames)
            {
                view.GroupDescriptions.Add(new PropertyGroupDescription(prop));
            }
        }
    }

    private ICommand? _removeGroupCommand;
    public ICommand RemoveGroupCommand => _removeGroupCommand ??= new RelayCommand<string>(prop =>
    {
        if (prop is { Length: > 0 })
            RemoveGroup(prop);
    });

    public void AddGroup(string propertyName)
    {
        if (!string.IsNullOrEmpty(propertyName) && !GroupedPropertyNames.Contains(propertyName))
        {
            GroupedPropertyNames.Add(propertyName);
        }
    }

    public void RemoveGroup(string propertyName)
    {
        if (!string.IsNullOrEmpty(propertyName))
        {
            GroupedPropertyNames.Remove(propertyName);
        }
    }

    public void ClearGroups()
    {
        GroupedPropertyNames.Clear();
    }

    private void SyncWithGrid(StackedHeaderDataGrid grid)
    {
        if (grid.ItemsSource != null)
        {
            var view = CollectionViewSource.GetDefaultView(grid.ItemsSource);
            if (view != null)
            {
                GroupedPropertyNames.Clear();
                foreach (var gd in view.GroupDescriptions.OfType<PropertyGroupDescription>())
                {
                    GroupedPropertyNames.Add(gd.PropertyName);
                }
            }
        }
    }
}

public class CountToVisibilityInvertedConverter : IValueConverter
{
    public static readonly CountToVisibilityInvertedConverter Instance = new();

    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        if (value is int count)
            return count == 0 ? Visibility.Visible : Visibility.Collapsed;
        return Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) => throw new NotImplementedException();
}


