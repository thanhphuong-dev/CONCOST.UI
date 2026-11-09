using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CONCOST.UI.Controls;
using Xunit;
using Xunit.Abstractions;

namespace CONCOST.UI.Tests;

/// <summary>
/// The scheduler's now line — the one hard-coded brush left on it — takes the theme's danger token,
/// and a host can recolour it through <see cref="Scheduler.NowLineBrush"/>.
/// </summary>
public class SchedulerNowLineTests
{
    private readonly ITestOutputHelper _output;

    public SchedulerNowLineTests(ITestOutputHelper output) => _output = output;

    private const int Width = 320;
    // 34 (HeaderHeight) + 24 * 8 (HourHeight) = 226px total day span. Use 260 to ensure all 24 hours fit anytime.
    private const int Height = 260;

    private static Scheduler MakeScheduler(Action<Scheduler>? configure = null)
    {
        // A window that spans the whole day guarantees the now line lands inside the range.
        var scheduler = new Scheduler
        {
            Width = Width,
            Height = Height,
            Date = DateTime.Today,
            StartHour = 0,
            EndHour = 24,
            HourHeight = 8,
            ShowNowLine = true
        };

        configure?.Invoke(scheduler);
        return scheduler;
    }

    private static byte[] Render(Action<Scheduler> configure)
    {
        byte[]? pixels = null;

        WpfApplicationHost.Run(() =>
        {
            var scheduler = MakeScheduler(configure);

            scheduler.Measure(new Size(Width, Height));
            scheduler.Arrange(new Rect(0, 0, Width, Height));
            scheduler.UpdateLayout();

            var bitmap = new RenderTargetBitmap(Width, Height, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(scheduler);

            int stride = Width * 4;
            pixels = new byte[stride * Height];
            bitmap.CopyPixels(pixels, stride, 0);
        });

        return pixels!;
    }

    private static int CountDifferences(byte[] a, byte[] b)
    {
        int count = 0;
        for (int i = 0; i < a.Length; i += 4)
        {
            if (a[i] != b[i] || a[i + 1] != b[i + 1] || a[i + 2] != b[i + 2] || a[i + 3] != b[i + 3])
                count++;
        }
        return count;
    }

    [Fact]
    public void Now_line_brush_recolours_the_marker()
    {
        var theme = Render(_ => { });
        var custom = Render(s => s.NowLineBrush = Brushes.Magenta);

        int diff = CountDifferences(theme, custom);
        _output.WriteLine($"NowLineBrush theme vs magenta: {diff} pixel(s) differ");

        Assert.True(diff > 5, "NowLineBrush changed nothing on screen.");
    }

    [Fact]
    public void Hiding_the_now_line_removes_the_marker()
    {
        var shown = Render(_ => { });
        var hidden = Render(s => s.ShowNowLine = false);

        int diff = CountDifferences(shown, hidden);
        _output.WriteLine($"ShowNowLine on vs off: {diff} pixel(s) differ");

        Assert.True(diff > 5, "The now line did not reach the render pass.");
    }
}
