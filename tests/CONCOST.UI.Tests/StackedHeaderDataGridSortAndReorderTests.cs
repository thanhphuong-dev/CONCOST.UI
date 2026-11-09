using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using CONCOST.UI.Controls;
using Xunit;

namespace CONCOST.UI.Tests;

public class StackedHeaderDataGridSortAndReorderTests
{
    private static void Pump()
    {
        var frame = new DispatcherFrame();
        Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.ContextIdle,
            new Action(() => frame.Continue = false));
        Dispatcher.PushFrame(frame);
    }

    public class TestItem : INotifyPropertyChanged, IEditableObject
    {
        private string _name = string.Empty;
        private int _value;

        public string Name
        {
            get => _name;
            set { _name = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name))); }
        }

        public int Value
        {
            get => _value;
            set { _value = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value))); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        public void BeginEdit() { }
        public void CancelEdit() { }
        public void EndEdit() { }
    }

    [Fact]
    public void StackedHeaderDataGrid_Sort_DoesNotThrow_WhenInEditTransaction()
    {
        WpfApplicationHost.Run(() =>
        {
            var data = new ObservableCollection<TestItem>
            {
                new() { Name = "Alpha", Value = 10 },
                new() { Name = "Beta", Value = 20 }
            };

            var grid = new StackedHeaderDataGrid
            {
                ItemsSource = data,
                AutoGenerateColumns = false
            };

            var leaf1 = new ColumnLeaf { HeaderText = "Name", BindingPath = "Name" };
            var leaf2 = new ColumnLeaf { HeaderText = "Value", BindingPath = "Value" };
            grid.ColumnHeaderTree.Add(leaf1);
            grid.ColumnHeaderTree.Add(leaf2);

            var window = new Window { Content = grid, Width = 500, Height = 300 };
            window.Show();
            Pump();

            var view = (IEditableCollectionView)CollectionViewSource.GetDefaultView(data);
            view.EditItem(data[0]);
            Assert.True(view.IsEditingItem);

            // Trigger sort during active transaction — MUST NOT THROW InvalidOperationException
            var exception = Record.Exception(() =>
            {
                // Simulate clicking header to sort
                var sortMethod = typeof(StackedHeaderDataGrid).GetMethod("PerformSort",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance,
                    null, new Type[] { typeof(ColumnLeaf), typeof(int) }, null);
                Assert.NotNull(sortMethod);
                sortMethod.Invoke(grid, new object[] { leaf1, 0 });
            });

            Assert.Null(exception);
            window.Close();
        });
    }

    [Fact]
    public void DataGridGroupPanel_Grouping_DoesNotThrow_WhenInEditTransaction()
    {
        WpfApplicationHost.Run(() =>
        {
            var data = new ObservableCollection<TestItem>
            {
                new() { Name = "Alpha", Value = 10 },
                new() { Name = "Beta", Value = 20 }
            };

            var grid = new DataGrid
            {
                ItemsSource = data,
                AutoGenerateColumns = true
            };

            var panel = new DataGridGroupPanel { TargetGrid = grid };
            var window = new Window
            {
                Content = new StackPanel { Children = { panel, grid } },
                Width = 500, Height = 300
            };
            window.Show();
            Pump();

            var view = (IEditableCollectionView)CollectionViewSource.GetDefaultView(data);
            view.EditItem(data[0]);
            Assert.True(view.IsEditingItem);

            // Grouping during edit transaction must not throw
            var exception = Record.Exception(() =>
            {
                panel.AddGroup("Name", "Name");
            });

            Assert.Null(exception);
            window.Close();
        });
    }

    [Fact]
    public void StackedHeaderDataGrid_ReorderLeafColumn_ReordersHeadersAndColumns()
    {
        WpfApplicationHost.Run(() =>
        {
            var data = new ObservableCollection<TestItem>
            {
                new() { Name = "Alpha", Value = 10 }
            };

            var grid = new StackedHeaderDataGrid
            {
                ItemsSource = data,
                AutoGenerateColumns = false
            };

            var leaf1 = new ColumnLeaf { HeaderText = "Name", BindingPath = "Name" };
            var leaf2 = new ColumnLeaf { HeaderText = "Value", BindingPath = "Value" };
            grid.ColumnHeaderTree.Add(leaf1);
            grid.ColumnHeaderTree.Add(leaf2);

            var window = new Window { Content = grid, Width = 500, Height = 300 };
            window.Show();
            Pump();

            Assert.Equal(2, grid.Columns.Count);
            Assert.Equal("Name", grid.Columns[0].Header);
            Assert.Equal("Value", grid.Columns[1].Header);

            // Reorder leaf1 onto leaf2
            var reorderMethod = typeof(StackedHeaderDataGrid).GetMethod("ReorderLeafColumn",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.NotNull(reorderMethod);
            reorderMethod.Invoke(grid, new object[] { "Name", leaf2 });

            Pump();

            // After reorder, leaf2 should be before leaf1
            Assert.Equal("Value", grid.Columns[0].Header);
            Assert.Equal("Name", grid.Columns[1].Header);

            window.Close();
        });
    }
}
