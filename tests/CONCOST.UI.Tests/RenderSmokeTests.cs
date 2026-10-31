using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CONCOST.UI.Controls;
using CONCOST.UI.Controls.Canvas;
using Xunit;

namespace CONCOST.UI.Tests;

/// <summary>
/// Visual regression guard (§96): render a control offscreen and assert the pixels are not blank.
/// </summary>
/// <remarks>
/// A full image-diff harness needs committed baselines and a tolerance policy, which is a project of
/// its own. What this file does catch is the class of failure that actually happens here: a template
/// that throws while being applied, a control that measures to zero, and a render pass that paints
/// nothing — all of which used to reach the showcase and only show up as an empty card.
/// </remarks>
public class RenderSmokeTests
{
    private static void RunOnSTA(Action action) => WpfApplicationHost.Run(action);

    /// <summary>Renders <paramref name="element"/> into a bitmap and returns the share of non-background pixels.</summary>
    private static double RenderedCoverage(FrameworkElement element, int width, int height)
    {
        var host = new Border
        {
            Background = Brushes.White,
            Child = element,
            Width = width,
            Height = height
        };

        host.Measure(new Size(width, height));
        host.Arrange(new Rect(0, 0, width, height));
        host.UpdateLayout();

        var bitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(host);

        int stride = width * 4;
        var pixels = new byte[stride * height];
        bitmap.CopyPixels(pixels, stride, 0);

        int painted = 0;
        for (int i = 0; i < pixels.Length; i += 4)
        {
            // Anything that is not the white backdrop counts, alpha included.
            if (pixels[i] != 255 || pixels[i + 1] != 255 || pixels[i + 2] != 255) painted++;
        }

        return painted / (double)(width * height);
    }

    /// <summary>
    /// Asserts the element paints at least <paramref name="minimum"/> of the surface. The floor is
    /// per control on purpose: a text box is mostly its own white background, so its border alone is
    /// well under one percent, while an overlay scrim should cover nearly everything.
    /// </summary>
    private static void AssertRendersSomething(FrameworkElement element, double minimum = 0.02,
        int width = 320, int height = 200)
    {
        double coverage = RenderedCoverage(element, width, height);

        Assert.True(coverage > minimum,
            $"{element.GetType().Name} painted {coverage:P2} of the surface, expected at least {minimum:P2} — " +
            "the template probably failed to apply.");
    }

    [Fact]
    public void Button_RendersContent()
    {
        RunOnSTA(() =>
        {
            var button = new Button { Content = "Xuất PDF" };
            button.SetResourceReference(FrameworkElement.StyleProperty, "CONCOST.Button.Primary");
            AssertRendersSomething(button);
        });
    }

    [Fact]
    public void TextBox_RendersItsChrome()
    {
        RunOnSTA(() =>
        {
            var box = new TextBox { Text = "CONCOST" };
            AssertRendersSomething(box, 0.002);
        });
    }

    [Fact]
    public void CheckBox_RendersTheTick()
    {
        RunOnSTA(() =>
        {
            var check = new CheckBox { Content = "Bật hiệu ứng", IsChecked = true };
            AssertRendersSomething(check, 0.02, 200, 60);
        });
    }

    [Fact]
    public void Card_RendersItsSurfaceAndBorder()
    {
        RunOnSTA(() =>
        {
            var card = new Card { Width = 240, Height = 120, Content = new TextBlock { Text = "Thẻ" } };
            AssertRendersSomething(card, 0.005, 300, 180);
        });
    }

    [Fact]
    public void RadialGauge_RendersTheArcAndTheNumber()
    {
        RunOnSTA(() =>
        {
            var gauge = new RadialGauge { Value = 76, Label = "Tiến độ", Suffix = "%" };
            AssertRendersSomething(gauge, 0.02, 200, 200);
        });
    }

    [Fact]
    public void ParticleField_RendersParticles()
    {
        RunOnSTA(() =>
        {
            var field = new ParticleField { ParticleCount = 80, ParticleOpacity = 1 };
            AssertRendersSomething(field, 0.002, 320, 200);
        });
    }

    [Fact]
    public void PageTransitionHost_RendersItsContent()
    {
        RunOnSTA(() =>
        {
            var host = new PageTransitionHost
            {
                Content = new Border
                {
                    Background = Brushes.OrangeRed,
                    Child = new TextBlock { Text = "Trang 1" }
                }
            };
            AssertRendersSomething(host, 0.05, 300, 160);
        });
    }

    [Fact]
    public void CommandPalette_Open_RendersTheOverlay()
    {
        RunOnSTA(() =>
        {
            var palette = new CommandPalette
            {
                ItemsSource = new List<CommandPaletteItem>
                {
                    new("Tổng quan dự án", "Bảng điều khiển KPI", shortcut: "Ctrl+1"),
                    new("Xuất PDF", "Xuất bản vẽ ra PDF", shortcut: "Ctrl+P")
                }
            };

            palette.Open();

            // An open palette paints its scrim, so it should cover nearly the whole surface.
            AssertRendersSomething(palette, 0.5, 700, 460);
        });
    }

    [Fact]
    public void CommandPalette_Closed_PaintsNothing()
    {
        RunOnSTA(() =>
        {
            var palette = new CommandPalette();
            double coverage = RenderedCoverage(palette, 400, 300);

            Assert.True(coverage < 0.01, $"A closed palette painted {coverage:P1} of the surface.");
        });
    }

    [Fact]
    public void SnowCanvas_RendersAfterLayout()
    {
        RunOnSTA(() =>
        {
            // The default flake brush is white, which is invisible on the white test backdrop.
            var canvas = new SnowCanvas { SnowflakeCount = 40, SnowflakeBrush = Brushes.DodgerBlue };

            // Flake positions are random, so the painted fraction wobbles run to run. The assertion
            // is only asking "did the template paint anything", so keep a wide margin rather than
            // pinning a figure that fails on an unlucky seed.
            AssertRendersSomething(canvas, 0.001, 320, 200);
        });
    }

    [Theory]
    [InlineData("CONCOST.Button.Primary")]
    [InlineData("CONCOST.Button.Secondary")]
    [InlineData("CONCOST.Badge.Base")]
    [InlineData("CONCOST.CommandPalette.Base")]
    [InlineData("CONCOST.PageTransitionHost.Base")]
    public void ThemeDictionary_ContainsTheStyleKeys(string key)
    {
        RunOnSTA(() =>
        {
            // The application dictionary only carries the theme once a host applies it, so the
            // dictionary itself is the honest place to assert on.
            var theme = new ResourceDictionary
            {
                Source = new Uri("pack://application:,,,/CONCOST.UI;component/Themes/Generic.xaml", UriKind.Absolute)
            };

            Assert.True(theme.Contains(key), $"The theme dictionary has no '{key}' style.");
        });
    }
}
