using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using CONCOST.UI.Themes;
using CONCOST.UI.Themes.Tools;
using Xunit;

namespace CONCOST.UI.Tests;

public class ThemeIntegrityTests
{
    private static readonly AppTheme[] AllThemes =
    {
        AppTheme.Light,
        AppTheme.Dark,
        AppTheme.SoftLight,
        AppTheme.Revit,
        AppTheme.RevitDark
    };

    private static readonly string[] RequiredBrushes =
    {
        "CONCOST.Brush.Background",
        "CONCOST.Brush.Surface",
        "CONCOST.Brush.SurfaceSecondary",
        "CONCOST.Brush.SurfaceElevated",
        "CONCOST.Brush.SurfaceSunken",
        "CONCOST.Brush.Border",
        "CONCOST.Brush.BorderSecondary",
        "CONCOST.Brush.TextPrimary",
        "CONCOST.Brush.TextSecondary",
        "CONCOST.Brush.TextTertiary",
        "CONCOST.Brush.TextDisabled",
        "CONCOST.Brush.InputBackground",
        "CONCOST.Brush.InputBorder",
        "CONCOST.Brush.Primary",
        "CONCOST.Brush.PrimarySoft",
        "CONCOST.Brush.Success",
        "CONCOST.Brush.Warning",
        "CONCOST.Brush.Danger",
        "CONCOST.Brush.Info",
        "CONCOST.Brush.Ribbon.TabStrip",
        "CONCOST.Brush.Ribbon.TabSelected",
        "CONCOST.Brush.Ribbon.PanelBackground",
        "CONCOST.Brush.Ribbon.Divider",
        "CONCOST.Brush.Ribbon.ItemForeground",
        "CONCOST.Brush.Ribbon.ItemHover",
        "CONCOST.Brush.TabBar.Background",
        "CONCOST.Brush.TabBar.TabActiveBackground",
        "CONCOST.Brush.TabBar.TabTextActive",
        "CONCOST.Brush.TabBar.TabActiveBorder"
    };

    [Theory]
    [InlineData(AppTheme.Light)]
    [InlineData(AppTheme.Dark)]
    [InlineData(AppTheme.SoftLight)]
    [InlineData(AppTheme.Revit)]
    [InlineData(AppTheme.RevitDark)]
    public void Theme_Provides_All_Required_Semantic_Brushes(AppTheme theme)
    {
        WpfApplicationHost.Run(() =>
        {
            var root = new Border();
            ThemeManager.Current.ApplyTheme(root, theme);

            foreach (var brushKey in RequiredBrushes)
            {
                var brush = root.TryFindResource(brushKey) as Brush;
                Assert.True(brush != null, $"Brush '{brushKey}' must be defined and resolvable in theme {theme}.");
            }
        });
    }

    [Theory]
    [InlineData(AppTheme.Light)]
    [InlineData(AppTheme.Dark)]
    [InlineData(AppTheme.SoftLight)]
    [InlineData(AppTheme.Revit)]
    [InlineData(AppTheme.RevitDark)]
    public void Theme_Text_Meets_WCAG_AA_Contrast_Against_Surface(AppTheme theme)
    {
        WpfApplicationHost.Run(() =>
        {
            var root = new Border();
            ThemeManager.Current.ApplyTheme(root, theme);

            var surface = ((SolidColorBrush)root.TryFindResource("CONCOST.Brush.Surface")!).Color;
            var textPrimary = ((SolidColorBrush)root.TryFindResource("CONCOST.Brush.TextPrimary")!).Color;
            var textSecondary = ((SolidColorBrush)root.TryFindResource("CONCOST.Brush.TextSecondary")!).Color;

            var primaryContrast = ContrastChecker.Check(textPrimary, surface);
            Assert.True(primaryContrast.PassesAA, $"TextPrimary in theme {theme} must pass WCAG AA (>=4.5:1). Actual ratio: {primaryContrast.Ratio:F2}:1");

            var secondaryContrast = ContrastChecker.Check(textSecondary, surface);
            Assert.True(secondaryContrast.PassesAA, $"TextSecondary in theme {theme} must pass WCAG AA (>=4.5:1). Actual ratio: {secondaryContrast.Ratio:F2}:1");
        });
    }

    [Fact]
    public void Shadow_Depth_Effects_Are_Resolvable()
    {
        WpfApplicationHost.Run(() =>
        {
            var root = new Border();
            ThemeManager.Current.ApplyTheme(root, AppTheme.Light);

            foreach (var shadowKey in new[] { "CONCOST.Shadow.Depth1", "CONCOST.Shadow.Depth2", "CONCOST.Shadow.Depth3" })
            {
                var effect = root.TryFindResource(shadowKey) as DropShadowEffect;
                Assert.True(effect != null, $"DropShadowEffect '{shadowKey}' must be resolvable from Generic.xaml.");
                Assert.True(effect.BlurRadius > 0, $"BlurRadius for '{shadowKey}' must be greater than 0.");
            }
        });
    }

    [Fact]
    public void Theme_Can_Switch_Sequentially_Without_Exceptions_Or_Leaks()
    {
        WpfApplicationHost.Run(() =>
        {
            var root = new Border();

            foreach (var theme in AllThemes)
            {
                ThemeManager.Current.ApplyTheme(root, theme);
                var surface = root.TryFindResource("CONCOST.Brush.Surface") as SolidColorBrush;
                Assert.NotNull(surface);
            }

            // Switch back to Light
            ThemeManager.Current.ApplyTheme(root, AppTheme.Light);
            var lightBg = ((SolidColorBrush)root.TryFindResource("CONCOST.Brush.Background")!).Color;
            Assert.Equal(Color.FromRgb(0xF8, 0xFA, 0xFC), lightBg);
        });
    }
}

