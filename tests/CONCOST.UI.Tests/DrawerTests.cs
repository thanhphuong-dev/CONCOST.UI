using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using CONCOST.UI.Controls;
using Xunit;

namespace CONCOST.UI.Tests;

public class DrawerTests
{
    private static void RunOnSTA(Action action) => WpfApplicationHost.Run(action);

    private static ResourceDictionary EnsureResources()
    {
        var app = Application.Current;
        if (app == null)
        {
            try { app = new Application(); }
            catch { app = Application.Current; }
        }

        var uri = new Uri("pack://application:,,,/CONCOST.UI;component/Themes/Generic.xaml", UriKind.Absolute);
        foreach (ResourceDictionary dictionary in app!.Resources.MergedDictionaries)
        {
            if (dictionary.Source == uri) return app.Resources;
        }

        app.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = uri });
        return app.Resources;
    }

    [Fact]
    public void Drawer_RapidCloseAndReopen_DoesNotApplyStaleCloseCompletion()
    {
        RunOnSTA(() =>
        {
            var resources = EnsureResources();
            var drawer = new Drawer
            {
                Style = (Style)resources["CONCOST.Drawer.Base"],
                Content = new TextBlock { Text = "Drawer content" }
            };
            var window = new Window { Width = 420, Height = 320, Content = drawer, ShowInTaskbar = false };

            try
            {
                window.Show();
                window.UpdateLayout();

                drawer.IsOpen = true;
                drawer.IsOpen = false;
                drawer.IsOpen = true;
                PumpDispatcher(TimeSpan.FromMilliseconds(320));

                Assert.True(drawer.IsOpen);
                Assert.Equal(Visibility.Visible, drawer.Visibility);

                drawer.IsOpen = false;
                PumpDispatcher(TimeSpan.FromMilliseconds(260));
                Assert.Equal(Visibility.Collapsed, drawer.Visibility);
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void Drawer_SizeProperties_ClampInvalidValuesAndUpdateTemplatePanel()
    {
        RunOnSTA(() =>
        {
            var resources = EnsureResources();
            var drawer = new Drawer { Style = (Style)resources["CONCOST.Drawer.Base"] };
            var window = new Window { Width = 420, Height = 320, Content = drawer, ShowInTaskbar = false };

            try
            {
                window.Show();
                window.UpdateLayout();
                drawer.ApplyTemplate();
                drawer.DrawerWidth = 260;
                var panel = Assert.IsType<Border>(drawer.Template.FindName("PART_DrawerPanel", drawer));
                Assert.Equal(260, panel.Width);

                drawer.DrawerWidth = double.NaN;
                Assert.Equal(0, drawer.DrawerWidth);
                Assert.Equal(0, panel.Width);

                drawer.DrawerHeight = -25;
                Assert.Equal(0, drawer.DrawerHeight);
            }
            finally
            {
                window.Close();
            }
        });
    }

    private static void PumpDispatcher(TimeSpan duration)
    {
        var frame = new DispatcherFrame();
        var timer = new DispatcherTimer(DispatcherPriority.Background)
        {
            Interval = duration
        };
        timer.Tick += (_, _) =>
        {
            timer.Stop();
            frame.Continue = false;
        };
        timer.Start();
        Dispatcher.PushFrame(frame);
    }
}
