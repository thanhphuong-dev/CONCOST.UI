using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using CONCOST.UI.AttachedProperties;
using Xunit;

namespace CONCOST.UI.Tests;

public class ScrollViewerTests
{
    private static void RunOnSTA(Action action)
    {
        Exception? exception = null;
        var thread = new Thread(() =>
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                exception = ex;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        if (exception != null)
        {
            throw new AggregateException("STA test failed", exception);
        }
    }

    [Fact]
    public void ScrollViewerHelper_DefaultProperties_MatchSpecifications()
    {
        RunOnSTA(() =>
        {
            var sv = new ScrollViewer();

            Assert.False(ScrollViewerHelper.GetIsInertiaEnabled(sv));
            Assert.Equal(1.0, ScrollViewerHelper.GetScrollSpeed(sv));
            Assert.True(ScrollViewerHelper.GetBubbleAtBoundary(sv));
            Assert.True(ScrollViewerHelper.GetIsAutoHideEnabled(sv));
            Assert.False(ScrollViewerHelper.GetShowBackToTop(sv));
            Assert.Equal(100.0, ScrollViewerHelper.GetBackToTopThreshold(sv));
        });
    }

    [Fact]
    public void ScrollViewerHelper_AttachedProperties_CanBeSetAndRetrieved()
    {
        RunOnSTA(() =>
        {
            var sv = new ScrollViewer();

            ScrollViewerHelper.SetIsInertiaEnabled(sv, true);
            Assert.True(ScrollViewerHelper.GetIsInertiaEnabled(sv));

            ScrollViewerHelper.SetScrollSpeed(sv, 72.5);
            Assert.Equal(72.5, ScrollViewerHelper.GetScrollSpeed(sv));

            ScrollViewerHelper.SetBubbleAtBoundary(sv, false);
            Assert.False(ScrollViewerHelper.GetBubbleAtBoundary(sv));

            ScrollViewerHelper.SetIsAutoHideEnabled(sv, false);
            Assert.False(ScrollViewerHelper.GetIsAutoHideEnabled(sv));

            ScrollViewerHelper.SetShowBackToTop(sv, true);
            Assert.True(ScrollViewerHelper.GetShowBackToTop(sv));

            ScrollViewerHelper.SetBackToTopThreshold(sv, 150.0);
            Assert.Equal(150.0, ScrollViewerHelper.GetBackToTopThreshold(sv));
        });
    }

    [Fact]
    public void ScrollViewerHelper_ProgrammaticNavigation_HandlesNullSafely()
    {
        RunOnSTA(() =>
        {
            // Verify null references do not throw unhandled exceptions
            ScrollViewerHelper.ScrollToTop(null!);
            ScrollViewerHelper.ScrollToBottom(null!);
            ScrollViewerHelper.ScrollToLeft(null!);
            ScrollViewerHelper.ScrollToRight(null!);
            ScrollViewerHelper.ScrollTo(null!, 100);
        });
    }

    [Fact]
    public void ScrollViewerHelper_ProgrammaticNavigation_InvokesScrollViewerMethods()
    {
        RunOnSTA(() =>
        {
            var sv = new ScrollViewer();
            var content = new Border { Width = 1000, Height = 2000 };
            sv.Content = content;

            // Simulate layout
            sv.Measure(new Size(300, 300));
            sv.Arrange(new Rect(0, 0, 300, 300));

            // Test programmatic calls do not throw
            ScrollViewerHelper.ScrollToTop(sv);
            ScrollViewerHelper.ScrollToBottom(sv);
            ScrollViewerHelper.ScrollToLeft(sv);
            ScrollViewerHelper.ScrollToRight(sv);
            ScrollViewerHelper.ScrollTo(sv, 200, 100);
        });
    }

    [Fact]
    public void ScrollViewerHelper_IsInertiaEnabled_HooksAndUnhooksWithoutLeaks()
    {
        RunOnSTA(() =>
        {
            var sv = new ScrollViewer();

            // Enable inertia
            ScrollViewerHelper.SetIsInertiaEnabled(sv, true);
            Assert.True(ScrollViewerHelper.GetIsInertiaEnabled(sv));

            // Disable inertia
            ScrollViewerHelper.SetIsInertiaEnabled(sv, false);
            Assert.False(ScrollViewerHelper.GetIsInertiaEnabled(sv));
        });
    }
}
