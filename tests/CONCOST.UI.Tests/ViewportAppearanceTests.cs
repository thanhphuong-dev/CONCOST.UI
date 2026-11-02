using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CONCOST.UI.Controls;
using Xunit;
using Xunit.Abstractions;

namespace CONCOST.UI.Tests;

/// <summary>
/// Paints a <see cref="ViewportControl"/> twice — once with the default look, once with a single
/// appearance property changed — and asserts the pixels actually differ. A property that is
/// registered without <c>AffectsRender</c>, or whose value never reaches the pen, looks correct in
/// the property grid and does nothing on screen.
/// </summary>
public class ViewportAppearanceTests
{
    private readonly ITestOutputHelper _output;

    public ViewportAppearanceTests(ITestOutputHelper output) => _output = output;

    private const int Width = 320;
    private const int Height = 240;

    private static ViewportControl MakeViewport(Action<ViewportControl>? configure = null)
    {
        var viewport = new ViewportControl
        {
            Width = Width,
            Height = Height,
            GridStep = 100,
            ShowGrid = true,
            ShowAxes = true,
            ShowOriginMarker = true,
            ShowCrosshair = false,
            ShowAperture = false,
            SnapToGrid = false
        };

        configure?.Invoke(viewport);
        return viewport;
    }

    private static byte[] Render(ViewportControl viewport)
    {
        viewport.Measure(new Size(Width, Height));
        viewport.Arrange(new Rect(0, 0, Width, Height));
        viewport.UpdateLayout();

        var bitmap = new RenderTargetBitmap(Width, Height, 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(viewport);

        int stride = Width * 4;
        var pixels = new byte[stride * Height];
        bitmap.CopyPixels(pixels, stride, 0);
        return pixels;
    }

    private static int Differences(byte[] a, byte[] b)
    {
        int count = 0;
        for (int i = 0; i < a.Length; i += 4)
        {
            if (a[i] != b[i] || a[i + 1] != b[i + 1] || a[i + 2] != b[i + 2] || a[i + 3] != b[i + 3])
                count++;
        }
        return count;
    }

    private void AssertChangesTheView(string name, Action<ViewportControl> configure)
    {
        WpfApplicationHost.Run(() =>
        {
            var baseline = Render(MakeViewport());
            var changed = Render(MakeViewport(configure));

            int diff = Differences(baseline, changed);
            _output.WriteLine($"{name}: {diff} pixel(s) differ");

            Assert.True(diff > 0, $"{name} changed nothing on screen — the value never reaches the render pass.");
        });
    }

    [Fact]
    public void Axis_brush_changes_the_view() =>
        AssertChangesTheView("AxisBrush", v => v.AxisBrush = Brushes.Red);

    [Fact]
    public void Axis_thickness_changes_the_view() =>
        AssertChangesTheView("AxisThickness", v => v.AxisThickness = 4);

    [Fact]
    public void Axis_dash_style_changes_the_view() =>
        AssertChangesTheView("AxisDashStyle", v => v.AxisDashStyle = DashStyles.Solid);

    [Fact]
    public void Grid_brush_changes_the_view() =>
        AssertChangesTheView("GridBrush", v => v.GridBrush = Brushes.Red);

    [Fact]
    public void Grid_major_brush_changes_the_view() =>
        AssertChangesTheView("GridMajorBrush", v => v.GridMajorBrush = Brushes.Red);

    [Fact]
    public void Grid_step_changes_the_view() =>
        AssertChangesTheView("GridStep", v => v.GridStep = 250);

    [Fact]
    public void Grid_major_every_changes_the_view() =>
        AssertChangesTheView("GridMajorEvery", v => v.GridMajorEvery = 2);

    [Fact]
    public void Origin_marker_visibility_changes_the_view() =>
        AssertChangesTheView("ShowOriginMarker", v => v.ShowOriginMarker = false);

    [Fact]
    public void Selection_fill_opacity_changes_the_view()
    {
        WpfApplicationHost.Run(() =>
        {
            var rect = new Rect(40, 40, 120, 80);

            var faint = Render(MakeViewport(v => { v.SelectionRectangle = rect; v.SelectionFillOpacity = 0.05; }));
            var strong = Render(MakeViewport(v => { v.SelectionRectangle = rect; v.SelectionFillOpacity = 0.5; }));

            int diff = Differences(faint, strong);
            _output.WriteLine($"SelectionFillOpacity: {diff} pixel(s) differ");
            Assert.True(diff > 0, "SelectionFillOpacity changed nothing on screen.");
        });
    }

    [Fact]
    public void Snap_marker_colour_changes_the_view()
    {
        WpfApplicationHost.Run(() =>
        {
            var snap = new Point(160, 120);

            var orange = Render(MakeViewport(v =>
            {
                v.Snap = snap;
                v.SnapKind = SnapKind.Endpoint;
                v.SnapMarkerBrush = Brushes.Orange;
            }));

            var blue = Render(MakeViewport(v =>
            {
                v.Snap = snap;
                v.SnapKind = SnapKind.Endpoint;
                v.SnapMarkerBrush = Brushes.Blue;
            }));

            int diff = Differences(orange, blue);
            _output.WriteLine($"SnapMarkerBrush: {diff} pixel(s) differ");
            Assert.True(diff > 0, "SnapMarkerBrush changed nothing on screen.");
        });
    }

    [Fact]
    public void Snap_marker_size_changes_the_view()
    {
        WpfApplicationHost.Run(() =>
        {
            var snap = new Point(160, 120);

            var small = Render(MakeViewport(v => { v.Snap = snap; v.SnapKind = SnapKind.Endpoint; v.SnapMarkerSize = 4; }));
            var large = Render(MakeViewport(v => { v.Snap = snap; v.SnapKind = SnapKind.Endpoint; v.SnapMarkerSize = 12; }));

            int diff = Differences(small, large);
            _output.WriteLine($"SnapMarkerSize: {diff} pixel(s) differ");
            Assert.True(diff > 0, "SnapMarkerSize changed nothing on screen.");
        });
    }

    [Fact]
    public void Show_snap_marker_can_hide_the_glyph()
    {
        WpfApplicationHost.Run(() =>
        {
            var snap = new Point(160, 120);

            var shown = Render(MakeViewport(v =>
            {
                v.Snap = snap;
                v.SnapKind = SnapKind.Endpoint;
                v.SnapMarkerSize = 12;
                v.ShowSnapLabel = false;
            }));

            var hidden = Render(MakeViewport(v =>
            {
                v.Snap = snap;
                v.SnapKind = SnapKind.Endpoint;
                v.SnapMarkerSize = 12;
                v.ShowSnapLabel = false;
                v.ShowSnapMarker = false;
            }));

            int diff = Differences(shown, hidden);
            _output.WriteLine($"ShowSnapMarker: {diff} pixel(s) differ");
            Assert.True(diff > 0, "ShowSnapMarker did not hide the glyph.");
        });
    }
}
