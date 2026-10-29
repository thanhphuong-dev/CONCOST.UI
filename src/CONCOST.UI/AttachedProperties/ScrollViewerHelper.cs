using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace CONCOST.UI.AttachedProperties
{
    /// <summary>
    /// Advanced ScrollViewer attached behavior suite inspired by HandyControl and WPFDevelopers.
    /// Features:
    /// - Smooth inertia wheel scrolling with CubicEase & HandoffBehavior.Compose
    /// - Shift + MouseWheel horizontal scrolling
    /// - Boundary bubble pass-through (avoids scroll-traps)
    /// - Configurable scroll speed multipliers
    /// - Auto-hiding or persistent scrollbar states
    /// - Automatic floating Back-to-Top button integration
    /// - Programmatic smooth scrolling APIs (ScrollToTop, ScrollToBottom, ScrollToLeft, ScrollToRight)
    /// </summary>
    public static class ScrollViewerHelper
    {
        // ── Per-ScrollViewer state (accumulator + running flag) ─────────────
        private static readonly Dictionary<ScrollViewer, ScrollState> _states = new();

        private class ScrollState
        {
            public double TotalVerticalOffset;
            public double TotalHorizontalOffset;
            public bool IsRunningVertical;
            public bool IsRunningHorizontal;
            public Button? BackToTopButton;
        }

        private static ScrollState GetState(ScrollViewer sv)
        {
            if (!_states.TryGetValue(sv, out var state))
            {
                state = new ScrollState();
                _states[sv] = state;
            }
            return state;
        }

        #region IsInertiaEnabled

        public static readonly DependencyProperty IsInertiaEnabledProperty =
            DependencyProperty.RegisterAttached(
                "IsInertiaEnabled",
                typeof(bool),
                typeof(ScrollViewerHelper),
                new PropertyMetadata(false, OnIsInertiaEnabledChanged));

        public static bool GetIsInertiaEnabled(DependencyObject obj) =>
            (bool)obj.GetValue(IsInertiaEnabledProperty);

        public static void SetIsInertiaEnabled(DependencyObject obj, bool value) =>
            obj.SetValue(IsInertiaEnabledProperty, value);

        private static void OnIsInertiaEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not ScrollViewer sv) return;

            if ((bool)e.NewValue)
                sv.PreviewMouseWheel += OnPreviewMouseWheel;
            else
                sv.PreviewMouseWheel -= OnPreviewMouseWheel;
        }

        #endregion

        #region ScrollSpeed

        public static readonly DependencyProperty ScrollSpeedProperty =
            DependencyProperty.RegisterAttached(
                "ScrollSpeed",
                typeof(double),
                typeof(ScrollViewerHelper),
                new PropertyMetadata(1.0));

        public static double GetScrollSpeed(DependencyObject obj) =>
            (double)obj.GetValue(ScrollSpeedProperty);

        public static void SetScrollSpeed(DependencyObject obj, double value) =>
            obj.SetValue(ScrollSpeedProperty, value);

        #endregion

        #region BubbleAtBoundary

        public static readonly DependencyProperty BubbleAtBoundaryProperty =
            DependencyProperty.RegisterAttached(
                "BubbleAtBoundary",
                typeof(bool),
                typeof(ScrollViewerHelper),
                new PropertyMetadata(true));

        public static bool GetBubbleAtBoundary(DependencyObject obj) =>
            (bool)obj.GetValue(BubbleAtBoundaryProperty);

        public static void SetBubbleAtBoundary(DependencyObject obj, bool value) =>
            obj.SetValue(BubbleAtBoundaryProperty, value);

        #endregion

        #region IsAutoHideEnabled

        public static readonly DependencyProperty IsAutoHideEnabledProperty =
            DependencyProperty.RegisterAttached(
                "IsAutoHideEnabled",
                typeof(bool),
                typeof(ScrollViewerHelper),
                new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.Inherits));

        public static bool GetIsAutoHideEnabled(DependencyObject obj) =>
            (bool)obj.GetValue(IsAutoHideEnabledProperty);

        public static void SetIsAutoHideEnabled(DependencyObject obj, bool value) =>
            obj.SetValue(IsAutoHideEnabledProperty, value);

        #endregion

        #region ShowBackToTop & BackToTopThreshold

        public static readonly DependencyProperty ShowBackToTopProperty =
            DependencyProperty.RegisterAttached(
                "ShowBackToTop",
                typeof(bool),
                typeof(ScrollViewerHelper),
                new PropertyMetadata(false, OnShowBackToTopChanged));

        public static bool GetShowBackToTop(DependencyObject obj) =>
            (bool)obj.GetValue(ShowBackToTopProperty);

        public static void SetShowBackToTop(DependencyObject obj, bool value) =>
            obj.SetValue(ShowBackToTopProperty, value);

        public static readonly DependencyProperty BackToTopThresholdProperty =
            DependencyProperty.RegisterAttached(
                "BackToTopThreshold",
                typeof(double),
                typeof(ScrollViewerHelper),
                new PropertyMetadata(100.0));

        public static double GetBackToTopThreshold(DependencyObject obj) =>
            (double)obj.GetValue(BackToTopThresholdProperty);

        public static void SetBackToTopThreshold(DependencyObject obj, double value) =>
            obj.SetValue(BackToTopThresholdProperty, value);

        private static void OnShowBackToTopChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not ScrollViewer sv) return;

            if ((bool)e.NewValue)
            {
                sv.Loaded += OnScrollViewerLoadedForBackToTop;
                sv.ScrollChanged += OnScrollViewerScrollChangedForBackToTop;
                if (sv.IsLoaded)
                {
                    SetupBackToTop(sv);
                }
            }
            else
            {
                sv.Loaded -= OnScrollViewerLoadedForBackToTop;
                sv.ScrollChanged -= OnScrollViewerScrollChangedForBackToTop;
                var state = GetState(sv);
                if (state.BackToTopButton != null)
                {
                    state.BackToTopButton.Visibility = Visibility.Collapsed;
                }
            }
        }

        private static void OnScrollViewerLoadedForBackToTop(object sender, RoutedEventArgs e)
        {
            if (sender is ScrollViewer sv)
            {
                SetupBackToTop(sv);
            }
        }

        private static void SetupBackToTop(ScrollViewer sv)
        {
            var state = GetState(sv);
            if (state.BackToTopButton == null && sv.Template != null)
            {
                if (sv.Template.FindName("PART_BackToTop", sv) is Button btn)
                {
                    state.BackToTopButton = btn;
                    btn.Click += (_, _) => ScrollToTop(sv);
                }
            }
            UpdateBackToTopVisibility(sv);
        }

        private static void OnScrollViewerScrollChangedForBackToTop(object sender, ScrollChangedEventArgs e)
        {
            if (sender is ScrollViewer sv)
            {
                UpdateBackToTopVisibility(sv);
            }
        }

        private static void UpdateBackToTopVisibility(ScrollViewer sv)
        {
            if (!GetShowBackToTop(sv)) return;
            var state = GetState(sv);
            if (state.BackToTopButton == null)
            {
                SetupBackToTop(sv);
                if (state.BackToTopButton == null) return;
            }

            var btn = state.BackToTopButton;
            double threshold = GetBackToTopThreshold(sv);
            bool shouldShow = sv.VerticalOffset > threshold;

            if (shouldShow && btn.Visibility != Visibility.Visible)
            {
                btn.Visibility = Visibility.Visible;
                var fadeIn = new DoubleAnimation(0.0, 1.0, TimeSpan.FromMilliseconds(200));
                btn.BeginAnimation(UIElement.OpacityProperty, fadeIn);
            }
            else if (!shouldShow && btn.Visibility == Visibility.Visible)
            {
                var fadeOut = new DoubleAnimation(btn.Opacity, 0.0, TimeSpan.FromMilliseconds(200));
                fadeOut.Completed += (_, _) =>
                {
                    if (sv.VerticalOffset <= threshold)
                    {
                        btn.Visibility = Visibility.Collapsed;
                    }
                };
                btn.BeginAnimation(UIElement.OpacityProperty, fadeOut);
            }
        }

        #endregion

        #region Animatable DPs

        public static readonly DependencyProperty CurrentVerticalOffsetProperty =
            DependencyProperty.RegisterAttached(
                "CurrentVerticalOffset",
                typeof(double),
                typeof(ScrollViewerHelper),
                new PropertyMetadata(0.0, OnCurrentVerticalOffsetChanged));

        public static double GetCurrentVerticalOffset(DependencyObject obj) =>
            (double)obj.GetValue(CurrentVerticalOffsetProperty);

        public static void SetCurrentVerticalOffset(DependencyObject obj, double value) =>
            obj.SetValue(CurrentVerticalOffsetProperty, value);

        private static void OnCurrentVerticalOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ScrollViewer sv)
                sv.ScrollToVerticalOffset((double)e.NewValue);
        }

        public static readonly DependencyProperty CurrentHorizontalOffsetProperty =
            DependencyProperty.RegisterAttached(
                "CurrentHorizontalOffset",
                typeof(double),
                typeof(ScrollViewerHelper),
                new PropertyMetadata(0.0, OnCurrentHorizontalOffsetChanged));

        public static double GetCurrentHorizontalOffset(DependencyObject obj) =>
            (double)obj.GetValue(CurrentHorizontalOffsetProperty);

        public static void SetCurrentHorizontalOffset(DependencyObject obj, double value) =>
            obj.SetValue(CurrentHorizontalOffsetProperty, value);

        private static void OnCurrentHorizontalOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ScrollViewer sv)
                sv.ScrollToHorizontalOffset((double)e.NewValue);
        }

        #endregion

        #region Wheel & Smooth Inertia Logic

        private static void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (sender is not ScrollViewer sv) return;

            var state = GetState(sv);
            bool isShift = (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift;
            double speed = GetScrollSpeed(sv);
            if (speed <= 0) speed = 1.0;
            double delta = e.Delta * speed;
            bool bubbleAtBoundary = GetBubbleAtBoundary(sv);

            // Shift + Wheel or purely horizontal scroll container
            if (isShift && sv.ScrollableWidth > 0)
            {
                if (CanScroll(sv.HorizontalOffset, sv.ScrollableWidth, delta))
                {
                    e.Handled = true;
                    ScrollHorizontalWithDelta(sv, state, delta);
                }
                else if (!bubbleAtBoundary)
                {
                    e.Handled = true;
                }
            }
            else if (!isShift && sv.ScrollableHeight > 0)
            {
                if (CanScroll(sv.VerticalOffset, sv.ScrollableHeight, delta))
                {
                    e.Handled = true;
                    ScrollVerticalWithDelta(sv, state, delta);
                }
                else if (!bubbleAtBoundary)
                {
                    e.Handled = true;
                }
            }
            else if (sv.ScrollableWidth > 0)
            {
                // Fallback to horizontal scroll if vertical range is 0
                if (CanScroll(sv.HorizontalOffset, sv.ScrollableWidth, delta))
                {
                    e.Handled = true;
                    ScrollHorizontalWithDelta(sv, state, delta);
                }
                else if (!bubbleAtBoundary)
                {
                    e.Handled = true;
                }
            }
        }

        private static bool CanScroll(double currentOffset, double maxOffset, double delta)
        {
            // delta > 0 is scrolling UP or LEFT (reducing offset)
            if (delta > 0) return currentOffset > 0.01;
            // delta < 0 is scrolling DOWN or RIGHT (increasing offset)
            return currentOffset < maxOffset - 0.01;
        }

        private static void ScrollVerticalWithDelta(ScrollViewer sv, ScrollState state, double delta)
        {
            if (!state.IsRunningVertical)
            {
                state.TotalVerticalOffset = sv.VerticalOffset;
                SetCurrentVerticalOffset(sv, sv.VerticalOffset);
            }

            state.TotalVerticalOffset = NumericCompatibility.Clamp(
                state.TotalVerticalOffset - delta,
                0,
                sv.ScrollableHeight);

            AnimateVertical(sv, state, state.TotalVerticalOffset);
        }

        private static void ScrollHorizontalWithDelta(ScrollViewer sv, ScrollState state, double delta)
        {
            if (!state.IsRunningHorizontal)
            {
                state.TotalHorizontalOffset = sv.HorizontalOffset;
                SetCurrentHorizontalOffset(sv, sv.HorizontalOffset);
            }

            state.TotalHorizontalOffset = NumericCompatibility.Clamp(
                state.TotalHorizontalOffset - delta,
                0,
                sv.ScrollableWidth);

            AnimateHorizontal(sv, state, state.TotalHorizontalOffset);
        }

        private static void AnimateVertical(ScrollViewer sv, ScrollState state, double targetOffset)
        {
            var anim = new DoubleAnimation
            {
                To = targetOffset,
                Duration = TimeSpan.FromMilliseconds(450),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut },
                FillBehavior = FillBehavior.Stop
            };

            anim.Completed += (_, _) =>
            {
                SetCurrentVerticalOffset(sv, targetOffset);
                state.IsRunningVertical = false;
            };

            state.IsRunningVertical = true;
            sv.BeginAnimation(CurrentVerticalOffsetProperty, anim, HandoffBehavior.Compose);
        }

        private static void AnimateHorizontal(ScrollViewer sv, ScrollState state, double targetOffset)
        {
            var anim = new DoubleAnimation
            {
                To = targetOffset,
                Duration = TimeSpan.FromMilliseconds(450),
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut },
                FillBehavior = FillBehavior.Stop
            };

            anim.Completed += (_, _) =>
            {
                SetCurrentHorizontalOffset(sv, targetOffset);
                state.IsRunningHorizontal = false;
            };

            state.IsRunningHorizontal = true;
            sv.BeginAnimation(CurrentHorizontalOffsetProperty, anim, HandoffBehavior.Compose);
        }

        #endregion

        #region Public Programmatic Navigation APIs

        /// <summary>
        /// Smoothly scroll to the top of the ScrollViewer (offset 0).
        /// </summary>
        public static void ScrollToTop(ScrollViewer sv, bool smooth = true)
        {
            if (sv == null) return;
            if (!smooth)
            {
                sv.ScrollToTop();
                return;
            }
            var state = GetState(sv);
            state.TotalVerticalOffset = 0;
            AnimateVertical(sv, state, 0);
        }

        /// <summary>
        /// Smoothly scroll to the bottom of the ScrollViewer.
        /// </summary>
        public static void ScrollToBottom(ScrollViewer sv, bool smooth = true)
        {
            if (sv == null) return;
            if (!smooth)
            {
                sv.ScrollToBottom();
                return;
            }
            var state = GetState(sv);
            state.TotalVerticalOffset = sv.ScrollableHeight;
            AnimateVertical(sv, state, sv.ScrollableHeight);
        }

        /// <summary>
        /// Smoothly scroll to the leftmost position of the ScrollViewer.
        /// </summary>
        public static void ScrollToLeft(ScrollViewer sv, bool smooth = true)
        {
            if (sv == null) return;
            if (!smooth)
            {
                sv.ScrollToLeftEnd();
                return;
            }
            var state = GetState(sv);
            state.TotalHorizontalOffset = 0;
            AnimateHorizontal(sv, state, 0);
        }

        /// <summary>
        /// Smoothly scroll to the rightmost position of the ScrollViewer.
        /// </summary>
        public static void ScrollToRight(ScrollViewer sv, bool smooth = true)
        {
            if (sv == null) return;
            if (!smooth)
            {
                sv.ScrollToRightEnd();
                return;
            }
            var state = GetState(sv);
            state.TotalHorizontalOffset = sv.ScrollableWidth;
            AnimateHorizontal(sv, state, sv.ScrollableWidth);
        }

        /// <summary>
        /// Smoothly scroll to a specific vertical and/or horizontal offset.
        /// </summary>
        public static void ScrollTo(ScrollViewer sv, double verticalOffset, double horizontalOffset = -1, bool smooth = true)
        {
            if (sv == null) return;
            var state = GetState(sv);

            if (verticalOffset >= 0)
            {
                double targetV = NumericCompatibility.Clamp(verticalOffset, 0, sv.ScrollableHeight);
                if (smooth)
                {
                    state.TotalVerticalOffset = targetV;
                    AnimateVertical(sv, state, targetV);
                }
                else
                {
                    sv.ScrollToVerticalOffset(targetV);
                }
            }

            if (horizontalOffset >= 0)
            {
                double targetH = NumericCompatibility.Clamp(horizontalOffset, 0, sv.ScrollableWidth);
                if (smooth)
                {
                    state.TotalHorizontalOffset = targetH;
                    AnimateHorizontal(sv, state, targetH);
                }
                else
                {
                    sv.ScrollToHorizontalOffset(targetH);
                }
            }
        }

        #endregion
    }
}
