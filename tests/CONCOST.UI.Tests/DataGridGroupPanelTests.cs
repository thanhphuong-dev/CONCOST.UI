using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using CONCOST.UI.Controls;
using Xunit;

namespace CONCOST.UI.Tests;

public class DataGridGroupPanelTests
{
    private static void Pump()
    {
        var frame = new DispatcherFrame();
        Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.ContextIdle,
            new Action(() => frame.Continue = false));
        Dispatcher.PushFrame(frame);
    }

    [Fact]
    public void Template_AppliesAndHasParts()
    {
        WpfApplicationHost.Run(() =>
        {
            var panel = new DataGridGroupPanel();
            var window = new Window { Content = panel, Width = 400, Height = 100 };
            window.Show();
            Pump();

            Assert.NotNull(panel.Template);
            Assert.Equal("Group by", panel.HeaderText);
        });
    }

    [Fact]
    public void AddGroup_UpdatesGroupsAndAvailableColumns()
    {
        WpfApplicationHost.Run(() =>
        {
            var grid = new DataGrid
            {
                ItemsSource = new ObservableCollection<BeamSample>
                {
                    new() { Name = "B1", Width = 200, Height = 400, Rebar = "4D20" },
                    new() { Name = "B2", Width = 250, Height = 450, Rebar = "6D20" }
                }
            };
            grid.Columns.Add(new DataGridTextColumn { Header = "Name", Binding = new System.Windows.Data.Binding("Name") });
            grid.Columns.Add(new DataGridTextColumn { Header = "Width", Binding = new System.Windows.Data.Binding("Width") });

            var panel = new DataGridGroupPanel { TargetGrid = grid };
            var window = new Window { Content = panel, Width = 400, Height = 100 };
            window.Show();
            Pump();

            Assert.True(panel.AvailableColumns.Count >= 2);

            panel.AddGroup("Width", "Width");

            Assert.Single(panel.Groups);
            Assert.Equal("Width", panel.Groups[0].PropertyPath);
            Assert.DoesNotContain(panel.AvailableColumns, c => c.PropertyPath == "Width");
        });
    }

    [Fact]
    public void RemoveGroup_RestoresAvailableColumn()
    {
        WpfApplicationHost.Run(() =>
        {
            var grid = new DataGrid
            {
                ItemsSource = new ObservableCollection<BeamSample>
                {
                    new() { Name = "B1", Width = 200, Height = 400, Rebar = "4D20" }
                }
            };
            grid.Columns.Add(new DataGridTextColumn { Header = "Name", Binding = new System.Windows.Data.Binding("Name") });
            grid.Columns.Add(new DataGridTextColumn { Header = "Width", Binding = new System.Windows.Data.Binding("Width") });

            var panel = new DataGridGroupPanel { TargetGrid = grid };
            var window = new Window { Content = panel, Width = 400, Height = 100 };
            window.Show();
            Pump();

            panel.AddGroup("Width", "Width");
            Assert.Single(panel.Groups);

            panel.RemoveGroup(panel.Groups[0]);
            Assert.Empty(panel.Groups);
            Assert.Contains(panel.AvailableColumns, c => c.PropertyPath == "Width");
        });
    }

    [Fact]
    public void ClearGroups_EmptiesGroups()
    {
        WpfApplicationHost.Run(() =>
        {
            var grid = new DataGrid
            {
                ItemsSource = new ObservableCollection<BeamSample>
                {
                    new() { Name = "B1", Width = 200, Height = 400, Rebar = "4D20" }
                }
            };
            grid.Columns.Add(new DataGridTextColumn { Header = "Name", Binding = new System.Windows.Data.Binding("Name") });
            grid.Columns.Add(new DataGridTextColumn { Header = "Width", Binding = new System.Windows.Data.Binding("Width") });

            var panel = new DataGridGroupPanel { TargetGrid = grid };
            var window = new Window { Content = panel, Width = 400, Height = 100 };
            window.Show();
            Pump();

            panel.AddGroup("Name", "Name");
            panel.AddGroup("Width", "Width");
            Assert.Equal(2, panel.Groups.Count);

            panel.ClearGroups();
            Assert.Empty(panel.Groups);
        });
    }

    private class BeamSample
    {
        public string Name { get; set; } = string.Empty;
        public double Width { get; set; }
        public double Height { get; set; }
        public string Rebar { get; set; } = string.Empty;
    }
}
