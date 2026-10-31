using System.Threading;
using System.Windows;
using CONCOST.UI.Controls;
using CONCOST.UI.Controls.Canvas;
using CONCOST.UI.Controls.Engineering;
using Xunit;

namespace CONCOST.UI.Tests;

public class ControlStabilityTests
{
    private static void RunOnSta(Action action)
    {
        Exception? exception = null;
        WpfApplicationHost.Run(() =>
        {
            try { action(); }
            catch (Exception ex) { exception = ex; }
        });
        if (exception is not null) throw new AggregateException("STA test failed", exception);
    }

    [Fact]
    public void RangeSlider_ValuesStayOrderedAndInsideBounds()
    {
        RunOnSta(() =>
        {
            var slider = new RangeSlider { Minimum = 10, Maximum = 90 };
            slider.StartValue = 70;
            slider.EndValue = 85;
            Assert.Equal(70, slider.StartValue);
            Assert.Equal(85, slider.EndValue);
            slider.Minimum = 120;
            Assert.Equal(120, slider.Minimum);
            Assert.Equal(120, slider.Maximum);
            Assert.Equal(120, slider.StartValue);
            Assert.Equal(120, slider.EndValue);
        });
    }

    [Fact]
    public void Pagination_RecomputesMaximumAndClampsPage()
    {
        RunOnSta(() =>
        {
            var pagination = new Pagination { TotalItems = 95, PageSize = 10, CurrentPage = 9 };
            Assert.Equal(10, pagination.MaxPage);
            pagination.TotalItems = 21;
            Assert.Equal(3, pagination.MaxPage);
            Assert.Equal(3, pagination.CurrentPage);
        });
    }

    [Fact]
    public void RebarParser_RejectsOverflowWithoutThrowing()
    {
        var info = RebarParser.Parse("999999999999999999999x3D20");
        Assert.False(info.IsValid);
        Assert.False(RebarParser.Parse("D10@0").IsValid);
    }

    [Fact]
    public void ZoomableCanvas_ClampsInvalidZoomInputs()
    {
        RunOnSta(() =>
        {
            var canvas = new ZoomableCanvas();
            canvas.Zoom = double.NaN;
            Assert.Equal(1, canvas.Zoom);
            canvas.MinZoom = 2;
            Assert.Equal(2, canvas.Zoom);
            canvas.MaxZoom = 1;
            Assert.Equal(2, canvas.MaxZoom);
            canvas.GridSize = double.PositiveInfinity;
            Assert.Equal(20, canvas.GridSize);
        });
    }

    [Fact]
    public void CompareSlider_ClampsOutOfRangeValues()
    {
        RunOnSta(() =>
        {
            var slider = new CompareSlider();
            slider.Value = 5;
            Assert.Equal(1, slider.Value);
            slider.Value = double.NaN;
            Assert.Equal(0.5, slider.Value);
        });
    }
}
