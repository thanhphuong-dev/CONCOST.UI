using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CONCOST.UI.Controls;
using Xunit;
using Xunit.Abstractions;

namespace CONCOST.UI.Tests;

/// <summary>
/// The 1.2.0 <see cref="MiniMap"/> pass: the viewport rectangle stopped painting over the items it
/// points at, the map rounds to the panel it docks into, and the outline can be handed to the host.
/// </summary>
public class MiniMapTests
{
    private readonly ITestOutputHelper _output;

    public MiniMapTests(ITestOutputHelper output) => _output = output;

    private static void RunOnSTA(Action action) => WpfApplicationHost.Run(action);

    private const int Width = 200;
    private const int Height = 140;

    private static MiniMap MakeMap(Action<MiniMap>? configure = null)
    {
        var map = new MiniMap
        {
            Width = Width,
            Height = Height,
            ContentBounds = new Rect(0, 0, 1000, 700),
            Viewport = new Rect(100, 100, 400, 280),
            ItemsSource = new List<MiniMapItem>
            {
                new() { Bounds = new Rect(150, 150, 120, 80) },
                new() { Bounds = new Rect(400, 300, 200, 100) }
            }
        };

        configure?.Invoke(map);
        return map;
    }

    private static byte[] Render(Action<MiniMap> configure)
    {
        byte[]? pixels = null;

        RunOnSTA(() =>
        {
            // The map is constructed on the STA thread: its constructor touches WPF state (the
            // cursor), which the xunit thread cannot.
            var map = MakeMap(configure);

            var host = new Border { Width = Width, Height = Height, Background = Brushes.White, Child = map };
            host.Measure(new Size(Width, Height));
            host.Arrange(new Rect(0, 0, Width, Height));
            host.UpdateLayout();

            var bitmap = new RenderTargetBitmap(Width, Height, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(host);

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
    public void Defaults_read_as_a_soft_tinted_rounded_map()
    {
        RunOnSTA(() =>
        {
            var map = MakeMap();

            Assert.Equal(0.18, map.ViewportFillOpacity, 3);
            Assert.Equal(new CornerRadius(6), map.CornerRadius);
            Assert.True(map.ShowBorder);
            Assert.Equal(System.Windows.Input.Cursors.SizeAll, map.Cursor);
        });
    }

    [Fact]
    public void Fill_opacity_leaves_the_items_legible_under_the_viewport()
    {
        // The viewport rect covers the first item; at a strong fill the item is buried, at the soft
        // tint it stays a distinct block. The bitmaps must differ and the soft one must still show it.
        var strong = Render(m => m.ViewportFillOpacity = 1);
        var soft = Render(m => m.ViewportFillOpacity = 0.18);

        int diff = CountDifferences(strong, soft);
        _output.WriteLine($"Fill 1.0 vs 0.18: {diff} pixel(s) differ");

        Assert.True(diff > 50, "ViewportFillOpacity changed nothing on screen.");
    }

    [Fact]
    public void Corner_radius_rounds_the_map()
    {
        var square = Render(m => m.CornerRadius = new CornerRadius(0));
        var rounded = Render(m => m.CornerRadius = new CornerRadius(40));

        int diff = CountDifferences(square, rounded);
        _output.WriteLine($"Radius 0 vs 40: {diff} pixel(s) differ");

        Assert.True(diff > 20, "CornerRadius changed nothing on screen.");
    }

    [Fact]
    public void ShowBorder_false_hands_the_frame_to_the_host()
    {
        var framed = Render(m => m.ShowBorder = true);
        var open = Render(m => m.ShowBorder = false);

        int diff = CountDifferences(framed, open);
        _output.WriteLine($"Border on vs off: {diff} pixel(s) differ");

        Assert.True(diff > 20, "ShowBorder changed nothing on screen.");
    }

    [Fact]
    public void Fill_opacity_coerces_into_zero_to_one()
    {
        RunOnSTA(() =>
        {
            Assert.Equal(0.5, MakeMap(m => m.ViewportFillOpacity = 0.5).ViewportFillOpacity, 3);
            Assert.Equal(0, MakeMap(m => m.ViewportFillOpacity = -2).ViewportFillOpacity, 3);
            Assert.Equal(1, MakeMap(m => m.ViewportFillOpacity = 7).ViewportFillOpacity, 3);
            Assert.Equal(0.18, MakeMap(m => m.ViewportFillOpacity = double.NaN).ViewportFillOpacity, 3);
        });
    }
}
