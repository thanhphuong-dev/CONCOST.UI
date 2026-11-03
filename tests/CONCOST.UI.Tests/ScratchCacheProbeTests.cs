using System.Windows;
using System.Windows.Controls;
using Xunit;
using Xunit.Abstractions;

namespace CONCOST.UI.Tests;

public class ScratchCacheProbeTests
{
    private readonly ITestOutputHelper _out;
    public ScratchCacheProbeTests(ITestOutputHelper o) => _out = o;

    [Fact]
    public void Probe()
    {
        WpfApplicationHost.Run(() =>
        {
            var fresh = new DataGrid();
            _out.WriteLine($"fresh: len={VirtualizingPanel.GetCacheLength(fresh).CacheBeforeViewport}/{VirtualizingPanel.GetCacheLength(fresh).CacheAfterViewport} unit={VirtualizingPanel.GetCacheLengthUnit(fresh)}");
            _out.WriteLine($"fresh local: len={fresh.ReadLocalValue(VirtualizingPanel.CacheLengthProperty)} unit={fresh.ReadLocalValue(VirtualizingPanel.CacheLengthUnitProperty)}");

            // The test host merges no dictionaries; check the framework default metadata.
            var window = new Window { Content = fresh, Width = 300, Height = 200, ShowInTaskbar = false, Left = -4000, Top = -4000 };
            window.Show();
            window.UpdateLayout();
            _out.WriteLine($"after style: len={VirtualizingPanel.GetCacheLength(fresh).CacheBeforeViewport} unit={VirtualizingPanel.GetCacheLengthUnit(fresh)}");

            fresh.SetValue(VirtualizingPanel.CacheLengthUnitProperty, VirtualizationCacheLengthUnit.Page);
            fresh.SetValue(VirtualizingPanel.CacheLengthProperty, new VirtualizationCacheLength(2));
            _out.WriteLine($"set: len={VirtualizingPanel.GetCacheLength(fresh).CacheBeforeViewport} unit={VirtualizingPanel.GetCacheLengthUnit(fresh)}");

            fresh.ClearValue(VirtualizingPanel.CacheLengthUnitProperty);
            fresh.ClearValue(VirtualizingPanel.CacheLengthProperty);
            _out.WriteLine($"cleared: len={VirtualizingPanel.GetCacheLength(fresh).CacheBeforeViewport} unit={VirtualizingPanel.GetCacheLengthUnit(fresh)}");

            Assert.True(true);
        });
    }
}
