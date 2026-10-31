using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using CONCOST.UI.Themes;
using Xunit;

namespace CONCOST.UI.Tests;

/// <summary>
/// The presets write brushes straight into the application's resource dictionary, which beats every
/// merged theme dictionary. Going back to another preset therefore has to give those keys back, or
/// the surfaces stay stuck on the last preset and Light/Dark stops working.
/// </summary>
public class ThemeManagerPresetTests
{
    private const string BackgroundKey = "CONCOST.Brush.Background";

    private static void RunOnSTA(Action action) => WpfApplicationHost.Run(action);

    [Fact]
    public void Brutal_does_not_pin_the_surface_brushes_at_application_level()
    {
        RunOnSTA(() =>
        {
            var app = Application.Current!;
            var theme = new ResourceDictionary
            {
                Source = new Uri("pack://application:,,,/CONCOST.UI;component/Themes/Light.xaml", UriKind.Absolute)
            };

            app.Resources.MergedDictionaries.Add(theme);

            try
            {
                var light = ((System.Windows.Media.SolidColorBrush)theme[BackgroundKey]).Color;

                ThemeManager.Current.Preset = ThemePreset.Brutal;
                var brutal = ((System.Windows.Media.SolidColorBrush)app.Resources[BackgroundKey]).Color;
                Assert.NotEqual(light, brutal);

                ThemeManager.Current.Preset = ThemePreset.Default;

                // The key must be gone from the application's own dictionary, so the merged theme
                // dictionary shows through again instead of a pinned snapshot.
                Assert.DoesNotContain(BackgroundKey, app.Resources.Keys.Cast<object>());
                Assert.Equal(light, ((System.Windows.Media.SolidColorBrush)app.Resources[BackgroundKey]).Color);
            }
            finally
            {
                ThemeManager.Current.Preset = ThemePreset.Default;
                app.Resources.MergedDictionaries.Remove(theme);
            }
        });
    }

    [Fact]
    public void Changing_the_preset_notifies_listeners()
    {
        RunOnSTA(() =>
        {
            var seen = new List<string?>();
            PropertyChangedEventHandler handler = (_, e) => seen.Add(e.PropertyName);
            ThemeManager.Current.PropertyChanged += handler;

            try
            {
                ThemeManager.Current.Preset = ThemePreset.Ocean;

                // The header selector follows the engine through this notification.
                Assert.Contains(nameof(ThemeManager.Preset), seen);
            }
            finally
            {
                ThemeManager.Current.PropertyChanged -= handler;
                ThemeManager.Current.Preset = ThemePreset.Default;
            }
        });
    }
}
