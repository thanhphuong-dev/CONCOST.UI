using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using CONCOST.UI.Core;

namespace CONCOST.UI.Controls;

/// <summary>
/// Attached behaviour that fades and slides an element in the first time it scrolls into
/// view — the scroll-reveal used on modern landing pages. Finds the nearest
/// <see cref="ScrollViewer"/> automatically and stops listening after the reveal.
/// </summary>
public static class ScrollReveal
{
    private sealed class State
    {
        public ScrollViewer? Viewer;
        public TranslateTransform? Transform;
        public bool Revealed;
    }

    private static readonly DependencyProperty StateProperty =
        DependencyProperty.RegisterAttached("State", typeof(State), typeof(ScrollReveal), new PropertyMetadata(null));

    public static readonly DependencyProperty IsEnabledProperty =
        DependencyProperty.RegisterAttached("IsEnabled", typeof(bool), typeof(ScrollReveal),
            new PropertyMetadata(false, OnIsEnabledChanged));

    public static readonly DependencyProperty OffsetProperty =
        DependencyProperty.RegisterAttached("Offset", typeof(double), typeof(ScrollReveal),
            new PropertyMetadata(24.0));

    public static readonly DependencyProperty DurationProperty =
        DependencyProperty.RegisterAttached("Duration", typeof(Duration), typeof(ScrollReveal),
            new PropertyMetadata(new Duration(TimeSpan.FromMilliseconds(460))));

    public static readonly DependencyProperty DelayProperty =
        DependencyProperty.RegisterAttached("Delay", typeof(Duration), typeof(ScrollReveal),
            new PropertyMetadata(new Duration(TimeSpan.Zero)));

    public static readonly DependencyProperty ThresholdProperty =
        DependencyProperty.RegisterAttached("Threshold", typeof(double), typeof(ScrollReveal),
            new PropertyMetadata(0.92));

    public static void SetIsEnabled(DependencyObject element, bool value) => element.SetValue(IsEnabledProperty, value);
    public static bool GetIsEnabled(DependencyObject element) => (bool)element.GetValue(IsEnabledProperty);

    /// <summary>Vertical distance (px) the element slides up from.</summary>
    public static void SetOffset(DependencyObject element, double value) => element.SetValue(OffsetProperty, value);
    public static double GetOffset(DependencyObject element) => (double)element.GetValue(OffsetProperty);

    public static void SetDuration(DependencyObject element, Duration value) => element.SetValue(DurationProperty, value);
    public static Duration GetDuration(DependencyObject element) => (Duration)element.GetValue(DurationProperty);

    public static void SetDelay(DependencyObject element, Duration value) => element.SetValue(DelayProperty, value);
    public static Duration GetDelay(DependencyObject element) => (Duration)element.GetValue(DelayProperty);

    /// <summary>Fraction of the viewport height at which the reveal fires (0.9 = near the bottom).</summary>
    public static void SetThreshold(DependencyObject element, double value) => element.SetValue(ThresholdProperty, value);
    public static double GetThreshold(DependencyObject element) => (double)element.GetValue(ThresholdProperty);

    private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not FrameworkElement element) return;

        if (!(bool)e.NewValue)
        {
            element.Loaded -= OnLoaded;
            Detach(element);
            return;
        }

        if (!MotionSettings.AnimationsEnabled)
        {
            // Reduced motion: no slide, no fade — just show it.
            element.Opacity = 1;
            return;
        }

        var state = new State { Transform = EnsureTranslate(element) };
        element.SetValue(StateProperty, state);
        element.Opacity = 0;
        state.Transform.Y = GetOffset(element);

        element.Loaded -= OnLoaded;
        element.Loaded += OnLoaded;
        if (element.IsLoaded) OnLoaded(element, new RoutedEventArgs());
    }

    private static void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement element) return;
        if (element.GetValue(StateProperty) is not State state) return;

        ScrollViewer? viewer = FindScrollViewer(element);
        if (viewer is null)
        {
            Reveal(element);
            return;
        }

        state.Viewer = viewer;
        viewer.ScrollChanged -= OnScrollChanged;
        viewer.ScrollChanged += OnScrollChanged;
        Check(element);
    }

    private static void OnScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        if (sender is not ScrollViewer viewer) return;

        foreach (FrameworkElement element in FindRevealable(viewer))
            Check(element);
    }

    private static System.Collections.Generic.IEnumerable<FrameworkElement> FindRevealable(DependencyObject root)
    {
        int count = VisualTreeHelper.GetChildrenCount(root);
        for (int i = 0; i < count; i++)
        {
            DependencyObject child = VisualTreeHelper.GetChild(root, i);
            if (child is FrameworkElement element && element.GetValue(StateProperty) is State)
                yield return element;

            foreach (FrameworkElement nested in FindRevealable(child))
                yield return nested;
        }
    }

    private static void Check(FrameworkElement element)
    {
        if (element.GetValue(StateProperty) is not State state || state.Revealed) return;
        if (state.Viewer is null) return;

        double threshold = Math.Max(0.1, Math.Min(1.0, GetThreshold(element)));

        try
        {
            Point position = element.TransformToAncestor(state.Viewer).Transform(new Point(0, 0));
            double viewport = state.Viewer.ViewportHeight;
            if (viewport <= 0) viewport = state.Viewer.ActualHeight;

            bool visible = position.Y < viewport * threshold && position.Y + element.ActualHeight > 0;
            if (visible) Reveal(element);
        }
        catch (InvalidOperationException)
        {
            // Element left the visual tree mid-scroll; ignore.
        }
    }

    private static void Reveal(FrameworkElement element)
    {
        if (element.GetValue(StateProperty) is not State state || state.Revealed) return;
        state.Revealed = true;

        Detach(element);

        if (!MotionSettings.AnimationsEnabled)
        {
            element.Opacity = 1;
            if (state.Transform is not null) state.Transform.Y = 0;
            return;
        }

        Duration duration = GetDuration(element);
        Duration delay = GetDelay(element);
        var ease = new CubicEase { EasingMode = EasingMode.EaseOut };

        var fade = new DoubleAnimation(1, duration) { EasingFunction = ease };
        if (delay.TimeSpan > TimeSpan.Zero) fade.BeginTime = delay.TimeSpan;
        element.BeginAnimation(UIElement.OpacityProperty, fade);

        if (state.Transform is not null)
        {
            var slide = new DoubleAnimation(0, duration) { EasingFunction = ease };
            if (delay.TimeSpan > TimeSpan.Zero) slide.BeginTime = delay.TimeSpan;
            state.Transform.BeginAnimation(TranslateTransform.YProperty, slide);
        }
    }

    private static void Detach(FrameworkElement element)
    {
        if (element.GetValue(StateProperty) is not State state) return;
        if (state.Viewer is null) return;
        state.Viewer.ScrollChanged -= OnScrollChanged;
        state.Viewer = null;
    }

    private static ScrollViewer? FindScrollViewer(DependencyObject start)
    {
        DependencyObject? current = VisualTreeHelper.GetParent(start);
        while (current is not null)
        {
            if (current is ScrollViewer viewer) return viewer;
            current = VisualTreeHelper.GetParent(current);
        }

        return null;
    }

    private static TranslateTransform EnsureTranslate(FrameworkElement element)
    {
        if (element.RenderTransform is TranslateTransform existing && !existing.IsFrozen)
            return existing;

        var translate = new TranslateTransform();
        Transform? current = element.RenderTransform;

        if (current is null || current.Value.IsIdentity)
        {
            element.RenderTransform = translate;
        }
        else
        {
            var group = new TransformGroup();
            group.Children.Add(current);
            group.Children.Add(translate);
            element.RenderTransform = group;
        }

        return translate;
    }
}
