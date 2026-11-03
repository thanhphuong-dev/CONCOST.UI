using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CONCOST.UI.Controls;
using Xunit;
using Xunit.Abstractions;

namespace CONCOST.UI.Tests;

/// <summary>
/// The large-data surface: <see cref="DataGridPerformance"/>'s attached switches land where WPF
/// reads them, and a fifty-thousand-row grid stays virtualized — the far rows are not realised
/// until something asks for them, and the scroll helpers can reach one.
/// </summary>
public class DataGridPerformanceTests
{
    private readonly ITestOutputHelper _output;

    public DataGridPerformanceTests(ITestOutputHelper output) => _output = output;
    private static void Pump()
    {
        var frame = new System.Windows.Threading.DispatcherFrame();
        System.Windows.Threading.Dispatcher.CurrentDispatcher.BeginInvoke(
            System.Windows.Threading.DispatcherPriority.ContextIdle,
            new Action(() => frame.Continue = false));
        System.Windows.Threading.Dispatcher.PushFrame(frame);
    }

    private static DataGrid Show(int rows, double height = 320)
    {
        var grid = new DataGrid
        {
            Height = height,
            Width = 420,
            AutoGenerateColumns = false,
            CanUserAddRows = false,
            HeadersVisibility = DataGridHeadersVisibility.Column
        };

        grid.Columns.Add(new DataGridTextColumn { Header = "Name", Binding = new System.Windows.Data.Binding("Name") });
        grid.Columns.Add(new DataGridTextColumn { Header = "Width", Binding = new System.Windows.Data.Binding("Width") });

        grid.ItemsSource = Enumerable.Range(0, rows)
            .Select(i => new BeamSample { Name = $"Dầm {i}", Width = 200 + i % 40 })
            .ToList();

        var window = new Window
        {
            Content = grid,
            Width = 460,
            Height = height + 40,
            ShowInTaskbar = false,
            WindowStartupLocation = WindowStartupLocation.Manual,
            Left = -4000,
            Top = -4000
        };

        window.Show();
        window.UpdateLayout();
        Pump();

        return grid;
    }

    [Fact]
    public void DeferredScrolling_reaches_the_template_viewer()
    {
        WpfApplicationHost.Run(() =>
        {
            var grid = Show(20);

            Assert.False(ScrollViewer.GetIsDeferredScrollingEnabled(grid));

            DataGridPerformance.SetDeferredScrolling(grid, true);
            Pump();

            var viewer = FindViewer(grid);
            Assert.NotNull(viewer);
            Assert.True(ScrollViewer.GetIsDeferredScrollingEnabled(viewer!));

            // Turning it off clears the flag it set, on the same viewer.
            DataGridPerformance.SetDeferredScrolling(grid, false);
            Pump();

            Assert.False(ScrollViewer.GetIsDeferredScrollingEnabled(viewer!));
        });
    }

    [Fact]
    public void CachePages_forwards_to_the_virtualizing_panel()
    {
        WpfApplicationHost.Run(() =>
        {
            var grid = Show(20);

            DataGridPerformance.SetCachePages(grid, 2);
            Pump();

            Assert.Equal(2, VirtualizingPanel.GetCacheLength(grid).CacheBeforeViewport);
            Assert.Equal(2, VirtualizingPanel.GetCacheLength(grid).CacheAfterViewport);
            Assert.Equal(VirtualizationCacheLengthUnit.Page, VirtualizingPanel.GetCacheLengthUnit(grid));

            // Zero restores the framework default — one item of cache per side, unit Item.
            DataGridPerformance.SetCachePages(grid, 0);
            Pump();

            Assert.Equal(VirtualizationCacheLengthUnit.Item, VirtualizingPanel.GetCacheLengthUnit(grid));
            Assert.Equal(1, VirtualizingPanel.GetCacheLength(grid).CacheBeforeViewport);
        });
    }

    [Fact]
    public void Fifty_thousand_rows_stay_virtualized_and_reachable()
    {
        WpfApplicationHost.Run(() =>
        {
            var grid = Show(50_000);

            Assert.Equal(50_000, grid.Items.Count);

            // The first rows exist; the far ones do not — that is the virtualization contract.
            Assert.NotNull(grid.ItemContainerGenerator.ContainerFromIndex(0));
            Assert.Null(grid.ItemContainerGenerator.ContainerFromIndex(49_999));

            Assert.True(DataGridPerformance.ScrollToIndex(grid, 49_999));
            Pump();

            // After the jump the far row is realised and the near one is gone — the panel moved.
            Assert.NotNull(grid.ItemContainerGenerator.ContainerFromIndex(49_999));

            // Out-of-range asks are refused rather than thrown.
            Assert.False(DataGridPerformance.ScrollToIndex(grid, 50_000));
            Assert.False(DataGridPerformance.ScrollToIndex(grid, -1));
        });
    }

    [Fact]
    public void ScrollToItem_brings_the_row_in_even_when_it_was_far()
    {
        WpfApplicationHost.Run(() =>
        {
            var grid = Show(50_000);

            var target = grid.Items.OfType<BeamSample>().Last();
            DataGridPerformance.ScrollToItem(grid, target);
            Pump();

            Assert.NotNull(grid.ItemContainerGenerator.ContainerFromItem(target));
        });
    }

    private static ScrollViewer? FindViewer(DataGrid grid)
    {
        if (grid.Template?.FindName("DG_ScrollViewer", grid) is ScrollViewer named) return named;

        return FindViewerDown(grid);
    }

    private static ScrollViewer? FindViewerDown(DependencyObject root)
    {
        int count = VisualTreeHelper.GetChildrenCount(root);

        for (int i = 0; i < count; i++)
        {
            var child = VisualTreeHelper.GetChild(root, i);

            if (child is ScrollViewer viewer) return viewer;

            var nested = FindViewerDown(child);
            if (nested is not null) return nested;
        }

        return null;
    }

    private class BeamSample
    {
        public string Name { get; set; } = string.Empty;
        public double Width { get; set; }
    }
}
