using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CONCOST.UI.Themes;
using CONCOST.UI.Themes.Tools;

namespace CONCOST.UI.Tests;

public class SoftLightThemeTests
{
    private static bool ContainsThemeDictionary(ResourceDictionary dictionary, string themeName) =>
        dictionary.Source?.OriginalString.Contains(themeName, StringComparison.OrdinalIgnoreCase) == true ||
        dictionary.MergedDictionaries.Any(child => ContainsThemeDictionary(child, themeName));

    [Fact]
    public void Soft_light_uses_warm_surfaces_and_can_switch_to_and_from_other_themes()
    {
        WpfApplicationHost.Run(() =>
        {
            var root = new Border();

            ThemeManager.Current.ApplyTheme(root, AppTheme.SoftLight);
            Assert.Equal(Color.FromRgb(0xF1, 0xF3, 0xF6),
                ((SolidColorBrush)root.TryFindResource("CONCOST.Brush.Background")!).Color);
            Assert.Equal(Color.FromRgb(0xF8, 0xF9, 0xFB),
                ((SolidColorBrush)root.TryFindResource("CONCOST.Brush.Surface")!).Color);
            var background = ((SolidColorBrush)root.TryFindResource("CONCOST.Brush.Background")!).Color;
            foreach (var key in new[]
                     {
                         "CONCOST.Brush.TextPrimary",
                         "CONCOST.Brush.TextSecondary",
                         "CONCOST.Brush.TextTertiary"
                     })
            {
                var foreground = ((SolidColorBrush)root.TryFindResource(key)!).Color;
                Assert.True(ContrastChecker.Check(foreground, background).PassesAA, $"{key} must meet WCAG AA contrast.");
            }
            Assert.Contains(root.Resources.MergedDictionaries,
                d => ContainsThemeDictionary(d, "SoftLight.xaml"));

            ThemeManager.Current.ApplyTheme(root, AppTheme.Dark);
            Assert.Equal(Color.FromRgb(0x0B, 0x0F, 0x19),
                ((SolidColorBrush)root.TryFindResource("CONCOST.Brush.Background")!).Color);

            ThemeManager.Current.ApplyTheme(root, AppTheme.SoftLight);
            Assert.Equal(Color.FromRgb(0xF1, 0xF3, 0xF6),
                ((SolidColorBrush)root.TryFindResource("CONCOST.Brush.Background")!).Color);
        });
    }
}
