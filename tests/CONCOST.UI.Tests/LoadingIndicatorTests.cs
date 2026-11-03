using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CONCOST.UI.Controls;
using CONCOST.UI.Core;
using Xunit;
using Xunit.Abstractions;

namespace CONCOST.UI.Tests;

/// <summary>
/// <see cref="LoadingIndicator"/> shipped as an empty <c>Control</c>: no template, no style, four
/// enum values nobody could see. The rebuild paints the four types on <see cref="AnimatedEffectBase"/>,
/// so these tests pin the natural sizes, that the types actually draw differently, and the
/// reduced-motion contract — motion off settles the glyph on its first frame.
/// </summary>
public class LoadingIndicatorTests
{
    private readonly ITestOutputHelper _output;

    public LoadingIndicatorTests(ITestOutputHelper output) => _output = output;

    [Theory]
    [InlineData(LoadingIndicatorType.ThreeDotsWave, 40, 24)]
    [InlineData(LoadingIndicatorType.CircularDotsRing, 44, 44)]
    [InlineData(LoadingIndicatorType.LinePulse, 96, 8)]
    [InlineData(LoadingIndicatorType.PulseCircle, 44, 44)]
    public void Each_type_measures_a_natural_size(LoadingIndicatorType type, double width, double height)
    {
        WpfApplicationHost.Run(() =>
        {
            var indicator = new LoadingIndicator { IndicatorType = type, DotSize = 10 };

            indicator.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

            Assert.Equal(width, indicator.DesiredSize.Width, 1);
            Assert.Equal(height, indicator.DesiredSize.Height, 1);
        });
    }

    [Fact]
    public void The_four_types_draw_different_glyphs()
    {
        var wave = Render(LoadingIndicatorType.ThreeDotsWave, phase: 0);
        var ring = Render(LoadingIndicatorType.CircularDotsRing, phase: 0);
        var line = Render(LoadingIndicatorType.LinePulse, phase: 0);
        var ripple = Render(LoadingIndicatorType.PulseCircle, phase: 0);

        Assert.True(CountDifferences(wave, ring) > 20, "Wave and ring drew the same picture.");
        Assert.True(CountDifferences(wave, line) > 20, "Wave and line drew the same picture.");
        Assert.True(CountDifferences(ring, ripple) > 20, "Ring and ripple drew the same picture.");
    }

    [Fact]
    public void The_glyph_advances_over_time_while_animating()
    {
        WpfApplicationHost.Run(() =>
        {
            // Pin the effect still so the only thing moving the phase is the tick
            // under test — the compositor hook is timing-dependent by nature.
            bool wasForced = MotionSettings.ForceDisabled;
            MotionSettings.ForceDisabled = true;
            try
            {
                var indicator = new LoadingIndicator { Width = 96, Height = 32 };
                var window = new Window
                {
                    Content = indicator,
                    Width = 140,
                    Height = 80,
                    ShowInTaskbar = false,
                    WindowStartupLocation = WindowStartupLocation.Manual,
                    Left = -4000,
                    Top = -4000
                };
                window.Show();
                WpfApplicationHost.Pump();

                Assert.True(indicator.IsVisible);

                Tick(indicator, 0.25);

                Assert.Equal(0.25, indicator.Phase, 3);
            }
            finally
            {
                MotionSettings.ForceDisabled = wasForced;
            }
        });
    }

    [Fact]
    public void Reduced_motion_parks_the_glyph_on_its_first_frame()
    {
        WpfApplicationHost.Run(() =>
        {
            bool wasForced = MotionSettings.ForceDisabled;
            MotionSettings.ForceDisabled = true;
            try
            {
                var indicator = new LoadingIndicator { Width = 96, Height = 32 };
                var window = new Window
                {
                    Content = indicator,
                    Width = 140,
                    Height = 80,
                    ShowInTaskbar = false,
                    WindowStartupLocation = WindowStartupLocation.Manual,
                    Left = -4000,
                    Top = -4000
                };
                window.Show();
                WpfApplicationHost.Pump();

                // The static frame is phase 0 — the same picture Restart() would give back.
                Assert.Equal(0, indicator.Phase, 3);
            }
            finally
            {
                MotionSettings.ForceDisabled = wasForced;
            }
        });
    }

    [Fact]
    public void Foreground_colours_the_glyph()
    {
        var primary = Render(LoadingIndicatorType.ThreeDotsWave, phase: 0, foreground: null);
        var red = Render(LoadingIndicatorType.ThreeDotsWave, phase: 0, foreground: Brushes.Red);

        Assert.True(CountDifferences(primary, red) > 10, "Foreground changed nothing on screen.");
    }

    private static byte[] Render(LoadingIndicatorType type, double phase, Brush? foreground = null)
    {
        byte[]? pixels = null;

        WpfApplicationHost.Run(() =>
        {
            var indicator = new LoadingIndicator
            {
                IndicatorType = type,
                Width = 96,
                Height = 44,
                Foreground = foreground ?? Brushes.DodgerBlue
            };

            // Paint the exact frame the test asks for instead of racing the compositor.
            Tick(indicator, phase);

            var host = new Border
            {
                Width = 96,
                Height = 44,
                Background = Brushes.White,
                Child = indicator
            };

            host.Measure(new Size(96, 44));
            host.Arrange(new Rect(0, 0, 96, 44));
            host.UpdateLayout();

            var bitmap = new RenderTargetBitmap(96, 44, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(host);

            int stride = 96 * 4;
            pixels = new byte[stride * 44];
            bitmap.CopyPixels(pixels, stride, 0);
        });

        return pixels!;
    }

    private static void Tick(LoadingIndicator indicator, double seconds)
    {
        typeof(AnimatedEffectBase)
            .GetMethod("OnTick", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
            .Invoke(indicator, new object[] { seconds });
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
}
