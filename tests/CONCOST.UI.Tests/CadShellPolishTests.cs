using System;
using System.Windows;
using System.Windows.Controls;
using CONCOST.UI.Controls;
using CONCOST.UI.Icons;
using Xunit;

namespace CONCOST.UI.Tests;

/// <summary>
/// The 1.2.0 CAD-shell pass: the title bar dims when its window loses the foreground — the cue
/// every title bar gives — and the info bar can drop its glyph when the title says enough.
/// </summary>
public class CadShellPolishTests
{
    private static void Pump()
    {
        var frame = new System.Windows.Threading.DispatcherFrame();
        System.Windows.Threading.Dispatcher.CurrentDispatcher.BeginInvoke(
            System.Windows.Threading.DispatcherPriority.ContextIdle,
            new Action(() => frame.Continue = false));
        System.Windows.Threading.Dispatcher.PushFrame(frame);
    }

    // ------------------------------------------------------------------ title bar

    [Fact]
    public void TitleBar_dims_while_the_window_is_inactive_and_restores_on_activation()
    {
        WpfApplicationHost.Run(() =>
        {
            var bar = new ApplicationTitleBar { ApplicationName = "CONCOST" };
            var window = new Window
            {
                Content = bar,
                Width = 600,
                Height = 100,
                ShowInTaskbar = false,
                WindowStartupLocation = WindowStartupLocation.Manual,
                Left = -4000,
                Top = -4000
            };
            window.Show();
            window.UpdateLayout();
            Pump();

            Assert.True(bar.IsWindowActive);
            Assert.Equal(1, bar.Opacity, 3);

            bar.SetWindowActive(false);

            Assert.Equal(0.55, bar.Opacity, 3);

            bar.SetWindowActive(true);

            Assert.Equal(1, bar.Opacity, 3);

            window.Content = null;
            window.Close();
        });
    }

    [Fact]
    public void DimOnInactive_false_keeps_the_bar_at_full_strength()
    {
        WpfApplicationHost.Run(() =>
        {
            var bar = new ApplicationTitleBar { ApplicationName = "CONCOST", DimOnInactive = false };
            var window = new Window
            {
                Content = bar,
                Width = 600,
                Height = 100,
                ShowInTaskbar = false,
                WindowStartupLocation = WindowStartupLocation.Manual,
                Left = -4000,
                Top = -4000
            };
            window.Show();
            window.UpdateLayout();
            Pump();

            bar.SetWindowActive(false);

            Assert.Equal(1, bar.Opacity, 3);

            window.Content = null;
            window.Close();
        });
    }

    [Fact]
    public void Unload_restores_full_opacity_so_a_rehost_starts_bright()
    {
        WpfApplicationHost.Run(() =>
        {
            var bar = new ApplicationTitleBar { ApplicationName = "CONCOST" };
            var window = new Window
            {
                Content = bar,
                Width = 600,
                Height = 100,
                ShowInTaskbar = false,
                WindowStartupLocation = WindowStartupLocation.Manual,
                Left = -4000,
                Top = -4000
            };
            window.Show();
            window.UpdateLayout();
            Pump();

            bar.SetWindowActive(false);
            Assert.Equal(0.55, bar.Opacity, 3);

            window.Content = null;
            Pump();

            Assert.Equal(1, bar.Opacity, 3);
            window.Close();
        });
    }

    // ------------------------------------------------------------------ info bar

    [Fact]
    public void InfoBar_ShowIcon_false_drops_the_glyph_and_keeps_the_words()
    {
        WpfApplicationHost.Run(() =>
        {
            var bar = new ApplicationInfoBar
            {
                Title = "Đồng bộ",
                Message = "Đã gửi 12 bản vẽ lên máy chủ."
            };

            var window = new Window
            {
                Content = bar,
                Width = 500,
                Height = 60,
                ShowInTaskbar = false,
                WindowStartupLocation = WindowStartupLocation.Manual,
                Left = -4000,
                Top = -4000
            };
            window.Show();
            window.UpdateLayout();
            Pump();

            Assert.True(bar.ShowIcon);

            // The icon hides without touching the words.
            bar.ShowIcon = false;
            window.UpdateLayout();
            Pump();

            var row = (Grid)bar.Child!;
            var icon = (Icon)row.Children[0];
            Assert.Equal(Visibility.Collapsed, icon.Visibility);

            Assert.Equal("Đồng bộ", bar.Title);
            Assert.Equal("Đã gửi 12 bản vẽ lên máy chủ.", bar.Message);

            window.Content = null;
            window.Close();
        });
    }
}
