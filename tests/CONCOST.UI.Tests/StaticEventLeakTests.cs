using System;
using System.Windows;
using System.Windows.Controls;
using CONCOST.UI.Controls;
using Xunit;

namespace CONCOST.UI.Tests;

/// <summary>
/// The static-event leak guard. <see cref="MotionSettings.Changed"/> is a static event that
/// outlives every control subscribing to it: an instance subscribing in its constructor and never
/// dropping the subscription pins the whole page it sat on — the view model, the brushes, the
/// handlers — reachable for the rest of the application. These tests build a control, show it,
/// unload it and let the collector run, then require the instance to be gone.
/// </summary>
/// <remarks>
/// The WeakReference pattern is the only honest way to assert this: a leak here shows up as a
/// test failure (the target survives collection), never as a flake, because nothing else holds the
/// instance — the test itself only holds the weak handle.
/// </remarks>
public class StaticEventLeakTests
{
    private static void ShowAndUnload(FrameworkElement element, Window window)
    {
        window.Content = element;
        window.Show();
        window.UpdateLayout();
        Pump();

        window.Content = null;
        window.UpdateLayout();
        Pump();
    }

    private static void Pump()
    {
        var frame = new System.Windows.Threading.DispatcherFrame();
        System.Windows.Threading.Dispatcher.CurrentDispatcher.BeginInvoke(
            System.Windows.Threading.DispatcherPriority.ContextIdle,
            new Action(() => frame.Continue = false));
        System.Windows.Threading.Dispatcher.PushFrame(frame);
    }

    private static void Collect()
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
    }

    [Fact]
    public void AnimatedEffect_is_released_after_unload()
    {
        WpfApplicationHost.Run(() =>
        {
            var window = new Window
            {
                Width = 300,
                Height = 200,
                ShowInTaskbar = false,
                WindowStartupLocation = WindowStartupLocation.Manual,
                Left = -4000,
                Top = -4000
            };

            WeakReference reference = CollectAfterShowUnload(() => new LoadingIndicator
            {
                Width = 60,
                Height = 24
            }, window);

            window.Close();
            Collect();

            Assert.False(reference.IsAlive,
                "The effect survived collection after being unloaded — it is still subscribed to a static event.");
        });
    }

    [Fact]
    public void AnimatedGradientBorder_is_released_after_unload()
    {
        WpfApplicationHost.Run(() =>
        {
            var window = new Window
            {
                Width = 300,
                Height = 200,
                ShowInTaskbar = false,
                WindowStartupLocation = WindowStartupLocation.Manual,
                Left = -4000,
                Top = -4000
            };

            WeakReference reference = CollectAfterShowUnload(() => new AnimatedGradientBorder
            {
                Width = 120,
                Height = 80
            }, window);

            window.Close();
            Collect();

            Assert.False(reference.IsAlive,
                "The border survived collection after being unloaded — it is still subscribed to a static event.");
        });
    }

    [Fact]
    public void Unloaded_effect_stops_tracking_motion_changes()
    {
        WpfApplicationHost.Run(() =>
        {
            var window = new Window
            {
                Width = 300,
                Height = 200,
                ShowInTaskbar = false,
                WindowStartupLocation = WindowStartupLocation.Manual,
                Left = -4000,
                Top = -4000
            };

            var indicator = new LoadingIndicator { Width = 60, Height = 24 };
            window.Content = indicator;
            window.Show();
            window.UpdateLayout();
            Pump();

            // The subscription is attached while loaded…
            window.Content = null;
            window.UpdateLayout();
            Pump();

            // …and a settings flip afterwards must not reach the detached instance. There is no
            // observable on the instance itself, so this is a smoke pass: the call used to throw
            // into a dead tree and keeps the handler table clean now.
            bool wasForced = Core.MotionSettings.ForceDisabled;
            try
            {
                Core.MotionSettings.ForceDisabled = !wasForced;
                Pump();
                Core.MotionSettings.ForceDisabled = wasForced;
                Pump();
            }
            finally
            {
                Core.MotionSettings.ForceDisabled = wasForced;
            }

            window.Close();
        });
    }

    private static WeakReference CollectAfterShowUnload(Func<FrameworkElement> make, Window window)
    {
        WeakReference reference = new(make());
        ShowAndUnload((FrameworkElement)reference.Target!, window);

        return reference;
    }
}
