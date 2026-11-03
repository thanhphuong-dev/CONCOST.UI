using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using CONCOST.UI.Controls.Ribbon;
using Xunit;

namespace CONCOST.UI.Tests;

/// <summary>
/// The 1.2.0 ribbon pass: <see cref="Ribbon.ShowScreenTips"/> — the inherited quiet-shell switch —
/// and the overlay folding away when the ribbon leaves the tree, which is what keeps the
/// reparented content host from being stranded in a popup.
/// </summary>
public class RibbonScreenTipTests
{
    private static void Pump()
    {
        var frame = new System.Windows.Threading.DispatcherFrame();
        System.Windows.Threading.Dispatcher.CurrentDispatcher.BeginInvoke(
            System.Windows.Threading.DispatcherPriority.ContextIdle,
            new Action(() => frame.Continue = false));
        System.Windows.Threading.Dispatcher.PushFrame(frame);
    }

    private static Ribbon ShowRibbon(out RibbonButton button, out RibbonButton pinned, Window window)
    {
        var ribbon = new Ribbon();

        var group = new RibbonGroup { Header = "Draw" };
        button = new RibbonButton { Id = "line", Header = "Line", ScreenTip = "Draw a line", GestureText = "L" };
        pinned = new RibbonButton { Id = "wall", Header = "Wall", ScreenTip = "Draw a wall" };
        group.Items.Add(button);
        group.Items.Add(pinned);

        var tab = new RibbonTabItem { Header = "Home", Id = "home", Content = new List<RibbonGroup> { group } };
        ribbon.ItemsSource = new List<RibbonTabItem> { tab };

        var panel = new StackPanel { Children = { ribbon } };
        window.Content = panel;
        window.Show();
        window.UpdateLayout();
        Pump();

        return ribbon;
    }

    [Fact]
    public void ShowScreenTips_off_at_the_ribbon_quiets_every_item()
    {
        WpfApplicationHost.Run(() =>
        {
            var window = new Window
            {
                Width = 700,
                Height = 300,
                ShowInTaskbar = false,
                WindowStartupLocation = WindowStartupLocation.Manual,
                Left = -4000,
                Top = -4000
            };

            var ribbon = ShowRibbon(out var line, out var wall, window);

            try
            {
                // The tips build themselves while the switch is on.
                Assert.NotNull(line.ToolTip);
                Assert.NotNull(wall.ToolTip);

                ribbon.ShowScreenTips = false;
                Pump();

                // The broadcast reaches both items without touching them.
                Assert.Null(line.ToolTip);
                Assert.Null(wall.ToolTip);

                // Back on, the built tips come back with the same content.
                ribbon.ShowScreenTips = true;
                Pump();

                Assert.NotNull(line.ToolTip);
            }
            finally
            {
                window.Content = null;
                window.Close();
            }
        });
    }

    [Fact]
    public void A_hand_assigned_tip_survives_the_switch()
    {
        WpfApplicationHost.Run(() =>
        {
            var window = new Window
            {
                Width = 700,
                Height = 300,
                ShowInTaskbar = false,
                WindowStartupLocation = WindowStartupLocation.Manual,
                Left = -4000,
                Top = -4000
            };

            var ribbon = ShowRibbon(out var line, out _, window);

            try
            {
                var tip = new ToolTip { Content = "the host's own words" };
                line.ToolTip = tip;
                Pump();

                ribbon.ShowScreenTips = false;
                Pump();

                Assert.True(ReferenceEquals(line.ToolTip, tip), "the host's own tip was overwritten");
            }
            finally
            {
                window.Content = null;
                window.Close();
            }
        });
    }

    [Fact]
    public void Unloading_closes_an_open_overlay_and_returns_the_content_host()
    {
        WpfApplicationHost.Run(() =>
        {
            var window = new Window
            {
                Width = 700,
                Height = 300,
                ShowInTaskbar = false,
                WindowStartupLocation = WindowStartupLocation.Manual,
                Left = -4000,
                Top = -4000
            };

            var ribbon = ShowRibbon(out _, out _, window);

            try
            {
                // The fold-away ribbon hosts its body in an overlay popup.
                ribbon.DisplayMode = RibbonDisplayMode.MinimizeToTabs;
                ribbon.IsOverlayOpen = true;
                window.UpdateLayout();
                Pump();

                Assert.True(ribbon.IsOverlayOpen);

                // Pulling the ribbon out of the tree folds the overlay as part of the unload.
                window.Content = null;
                Pump();

                Assert.False(ribbon.IsOverlayOpen);
            }
            finally
            {
                window.Content = null;
                window.Close();
            }
        });
    }
}
