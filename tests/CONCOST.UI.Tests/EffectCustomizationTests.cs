using System.Windows.Media;
using CONCOST.UI.Controls;
using Xunit;

namespace CONCOST.UI.Tests;

/// <summary>
/// These three controls used to hard-code the checker square, the confetti size and the magnifier
/// crosshair. Each is now a property a host can set without retemplating the control.
/// </summary>
public class EffectCustomizationTests
{
    [Fact]
    public void ImageViewer_checker_and_zoom_are_configurable()
    {
        WpfApplicationHost.Run(() =>
        {
            var viewer = new ImageViewer();

            Assert.Null(viewer.CheckerboardBrush);
            Assert.Equal(12, viewer.CheckerCellSize);
            Assert.Equal(1.15, viewer.ZoomStep);

            viewer.CheckerCellSize = 1000;
            Assert.Equal(64, viewer.CheckerCellSize);

            viewer.ZoomStep = 9;
            Assert.Equal(2, viewer.ZoomStep);
        });
    }

    [Fact]
    public void Confetti_particle_size_is_configurable()
    {
        WpfApplicationHost.Run(() =>
        {
            var burst = new ConfettiBurst();

            Assert.Equal(4, burst.MinParticleSize);
            Assert.Equal(9, burst.MaxParticleSize);
        });
    }

    [Fact]
    public void Magnifier_crosshair_can_be_overridden()
    {
        WpfApplicationHost.Run(() =>
        {
            var magnifier = new Magnifier();

            Assert.Null(magnifier.CrosshairBrush);

            magnifier.CrosshairBrush = Brushes.Red;
            Assert.Same(Brushes.Red, magnifier.CrosshairBrush);
        });
    }
}
