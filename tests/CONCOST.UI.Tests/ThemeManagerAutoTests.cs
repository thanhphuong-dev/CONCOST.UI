using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CONCOST.UI.Themes;
using Xunit;

namespace CONCOST.UI.Tests;

/// <summary>
/// <see cref="AppTheme.Auto"/>: the theme comes from the window the controls sit in, not from our
/// own <c>Application</c> — the mode a Revit/AutoCAD plugin needs, since the host owns the window.
/// </summary>
public class ThemeManagerAutoTests
{
    private static void RunOnSTA(Action action) => WpfApplicationHost.Run(action);

    private static Border Host(Window window, Color background, out Border content)
    {
        var panel = new Border { Background = new SolidColorBrush(background), Width = 200, Height = 100 };
        content = new Border();
        panel.Child = content;
        window.Content = panel;
        return panel;
    }

    private static Color BackgroundOf(FrameworkElement root) =>
        ((SolidColorBrush)root.TryFindResource("CONCOST.Brush.Background")!).Color;

    [Fact]
    public void DetectTheme_reads_a_light_host_window()
    {
        RunOnSTA(() =>
        {
            var window = new Window { Width = 300, Height = 200, WindowStyle = WindowStyle.None, ShowInTaskbar = false };
            var panel = Host(window, Colors.White, out var content);
            window.Show();
            window.UpdateLayout();

            Assert.Equal(AppTheme.Light, ThemeManager.DetectTheme(content));

            window.Close();
            GC.KeepAlive(panel);
        });
    }

    [Fact]
    public void DetectTheme_reads_a_dark_host_window()
    {
        RunOnSTA(() =>
        {
            var window = new Window { Width = 300, Height = 200, WindowStyle = WindowStyle.None, ShowInTaskbar = false };
            var panel = Host(window, Color.FromRgb(0x11, 0x18, 0x27), out var content);
            window.Show();
            window.UpdateLayout();

            Assert.Equal(AppTheme.Dark, ThemeManager.DetectTheme(content));

            window.Close();
            GC.KeepAlive(panel);
        });
    }

    [Fact]
    public void Auto_applies_the_light_theme_inside_a_light_host()
    {
        RunOnSTA(() =>
        {
            var window = new Window { Width = 300, Height = 200, WindowStyle = WindowStyle.None, ShowInTaskbar = false };
            Host(window, Colors.White, out var content);
            window.Show();
            window.UpdateLayout();

            ThemeManager.Current.ApplyTheme(content, AppTheme.Auto);

            Assert.Equal(Color.FromRgb(0xF8, 0xFA, 0xFC), BackgroundOf(content));

            window.Close();
        });
    }

    [Fact]
    public void Auto_applies_the_dark_theme_inside_a_dark_host()
    {
        RunOnSTA(() =>
        {
            var window = new Window { Width = 300, Height = 200, WindowStyle = WindowStyle.None, ShowInTaskbar = false };
            Host(window, Color.FromRgb(0x11, 0x18, 0x27), out var content);
            window.Show();
            window.UpdateLayout();

            ThemeManager.Current.ApplyTheme(content, AppTheme.Auto);

            Assert.Equal(Color.FromRgb(0x0B, 0x0F, 0x19), BackgroundOf(content));

            window.Close();
        });
    }

    [Fact]
    public void Auto_leaves_the_application_theme_alone()
    {
        RunOnSTA(() =>
        {
            var window = new Window { Width = 300, Height = 200, WindowStyle = WindowStyle.None, ShowInTaskbar = false };
            Host(window, Color.FromRgb(0x11, 0x18, 0x27), out var content);
            window.Show();
            window.UpdateLayout();

            var before = ThemeManager.Current.Theme;
            ThemeManager.Current.ApplyTheme(content, AppTheme.Auto);

            // A plugin must not repaint the host's other windows.
            Assert.Equal(before, ThemeManager.Current.Theme);

            window.Close();
        });
    }

    [Fact]
    public void RefreshAutoTheme_follows_the_host_when_it_changes()
    {
        RunOnSTA(() =>
        {
            var window = new Window { Width = 300, Height = 200, WindowStyle = WindowStyle.None, ShowInTaskbar = false };
            var panel = Host(window, Colors.White, out var content);
            window.Show();
            window.UpdateLayout();

            ThemeManager.Current.ApplyTheme(content, AppTheme.Auto);
            Assert.Equal(Color.FromRgb(0xF8, 0xFA, 0xFC), BackgroundOf(content));

            panel.Background = new SolidColorBrush(Color.FromRgb(0x11, 0x18, 0x27));
            window.UpdateLayout();
            ThemeManager.Current.RefreshAutoTheme(content);

            Assert.Equal(Color.FromRgb(0x0B, 0x0F, 0x19), BackgroundOf(content));

            window.Close();
        });
    }

    [Fact]
    public void RefreshAutoTheme_does_nothing_for_an_explicit_theme()
    {
        RunOnSTA(() =>
        {
            var window = new Window { Width = 300, Height = 200, WindowStyle = WindowStyle.None, ShowInTaskbar = false };
            var panel = Host(window, Colors.White, out var content);
            window.Show();
            window.UpdateLayout();

            ThemeManager.Current.ApplyTheme(content, AppTheme.Dark);

            panel.Background = new SolidColorBrush(Colors.White);
            ThemeManager.Current.RefreshAutoTheme(content);

            // The host asked for Dark on purpose; a light host must not override it.
            Assert.Equal(Color.FromRgb(0x0B, 0x0F, 0x19), BackgroundOf(content));

            window.Close();
        });
    }
}
