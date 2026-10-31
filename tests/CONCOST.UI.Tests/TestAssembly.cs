using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Threading;
using Xunit;

// Several suites host real WPF windows (the ribbon layout, key tips, the extended-properties and
// DataGrid panel tests). Running two of them at once fights over the single Application instance,
// which showed up as random failures in whichever suite lost the race. Serialising the collections
// costs a second or two and makes the suite deterministic; every WPF body also runs on the one
// dispatcher thread below, so no window can outlive the thread that created it.
[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace CONCOST.UI.Tests;

/// <summary>
/// The single STA thread — and the single <see cref="Application"/> — that every WPF test runs on.
/// </summary>
/// <remarks>
/// Two separate problems came from giving each test its own thread:
/// <list type="bullet">
/// <item><description>
/// The first suite to call <c>new Application()</c> owned the only application, and its thread
/// exited when that test finished. <see cref="Application.Current"/> then pointed at a dead
/// dispatcher and whichever suite ran next failed at random — about one run in four.
/// </description></item>
/// <item><description>
/// A window created on a thread that has already exited keeps its HWND, and the next message
/// Windows posts to it crashed the test host with a <see cref="NullReferenceException"/> inside
/// <c>HwndSubclass.SubclassWndProc</c>, aborting the run halfway through.
/// </description></item>
/// </list>
/// Running the bodies on one long-lived dispatcher thread removes both: the application outlives
/// every test, and no window ever belongs to a thread that is gone.
/// </remarks>
internal static class WpfApplicationHost
{
    private static readonly object Gate = new object();
    private static Dispatcher? _dispatcher;

    /// <summary>
    /// Starts the thread on first use. It must <b>not</b> run from a module initializer: the new
    /// thread executes code from this module, so it blocks on the initializer that is waiting for
    /// it, and the wait times out.
    /// </summary>
    internal static void StartThread()
    {
        lock (Gate)
        {
            if (_dispatcher is not null) return;

            using var ready = new ManualResetEventSlim();
            var thread = new Thread(() =>
            {
                // The Application is created on the first test, not here: constructing it can touch
                // this very module, and a module initializer that waits on work which needs the
                // initializer to finish deadlocks for the length of the timeout.
                _dispatcher = Dispatcher.CurrentDispatcher;
                ready.Set();
                Dispatcher.Run();
            })
            {
                IsBackground = true,
                Name = "WPF test application"
            };

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            ready.Wait(TimeSpan.FromSeconds(15));
        }
    }

    /// <summary>Pumps the dispatcher until the queue is idle — use after a property change that triggers binding.</summary>
    public static void Pump()
    {
        var frame = new DispatcherFrame();
        Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle,
            new Action(() => frame.Continue = false));
        Dispatcher.PushFrame(frame);
    }

    /// <summary>Runs a test body on the shared dispatcher thread and rethrows whatever it threw.</summary>
    public static void Run(Action action)
    {
        StartThread();

        var dispatcher = _dispatcher ?? throw new InvalidOperationException("The WPF test thread did not start.");

        Exception? error = null;
        dispatcher.Invoke(() =>
        {
            EnsureApplication();

            try { action(); }
            catch (Exception ex) { error = ex; }
            finally { CloseLeftoverWindows(); }
        });

        if (error is not null) throw new AggregateException("STA test failed", error);
    }

    /// <summary>
    /// Closes whatever a test left open. A window that outlives its test keeps its HWND, and the
    /// message Windows posts while the process tears down lands on a dying thread and crashes the
    /// test host — which is what made a run in ten abort halfway through with a
    /// <see cref="NullReferenceException"/> inside <c>HwndSubclass.SubclassWndProc</c>.
    /// </summary>
    private static void CloseLeftoverWindows()
    {
        var application = Application.Current;
        if (application is null) return;

        foreach (var window in application.Windows.OfType<Window>().ToArray())
        {
            try
            {
                // Pop-ups own their own HWND and are not listed in Application.Windows; leaving one
                // open is enough for Windows to post a message to a thread that is already gone.
                foreach (var popup in FindAll<Popup>(window)) popup.IsOpen = false;

                window.Close();
            }
            catch
            {
                // the window is already on its way out
            }
        }

        // Let WM_CLOSE / WM_DESTROY run before the next test starts.
        var frame = new DispatcherFrame();
        Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle,
            new Action(() => frame.Continue = false));
        Dispatcher.PushFrame(frame);
    }

    private static IEnumerable<T> FindAll<T>(DependencyObject root) where T : DependencyObject
    {
        var count = VisualTreeHelper.GetChildrenCount(root);
        for (var i = 0; i < count; i++)
        {
            var child = VisualTreeHelper.GetChild(root, i);

            if (child is T match) yield return match;

            foreach (var nested in FindAll<T>(child)) yield return nested;
        }
    }

    /// <summary>Creates the one application the whole run shares, on the shared thread.</summary>
    private static void EnsureApplication()
    {
        if (Application.Current is not null) return;

        try
        {
            _ = new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };
        }
        catch (InvalidOperationException)
        {
            // Another suite won the race; Application.Current is good either way.
        }
    }
}
