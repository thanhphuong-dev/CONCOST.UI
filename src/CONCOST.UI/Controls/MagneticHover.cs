using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using CONCOST.UI.Core;

namespace CONCOST.UI.Controls;

/// <summary>
/// Attached behaviour that pulls an element slightly toward the pointer — the “magnetic
/// button” effect. Movement is a <see cref="TranslateTransform"/>, so it never triggers a
/// layout pass, and it is skipped entirely when motion is disabled.
/// </summary>
public static class MagneticHover
{
    private sealed class State
    {
        public TranslateTransform? Transform;
        public FrameworkElement? Element;
    }

    private static readonly DependencyProperty StateProperty =
        DependencyProperty.RegisterAttached("State", typeof(State), typeof(MagneticHover), new PropertyMetadata(null));

    public static readonly DependencyProperty IsEnabledProperty =
        DependencyProperty.RegisterAttached("IsEnabled", typeof(bool), typeof(MagneticHover),
            new PropertyMetadata(false, OnIsEnabledChanged));

    public static readonly DependencyProperty StrengthProperty =
        DependencyProperty.RegisterAttached("Strength", typeof(double), typeof(MagneticHover),
            new PropertyMetadata(0.28));

    public static readonly DependencyProperty MaxOffsetProperty =
        DependencyProperty.RegisterAttached("MaxOffset", typeof(double), typeof(MagneticHover),
            new PropertyMetadata(10.0));

    public static void SetIsEnabled(DependencyObject element, bool value) => element.SetValue(IsEnabledProperty, value);
    public static bool GetIsEnabled(DependencyObject element) => (bool)element.GetValue(IsEnabledProperty);

    /// <summary>How much of the pointer offset the element follows (0.1–0.4 feels natural).</summary>
    public static void SetStrength(DependencyObject element, double value) => element.SetValue(StrengthProperty, value);
    public static double GetStrength(DependencyObject element) => (double)element.GetValue(StrengthProperty);

    public static void SetMaxOffset(DependencyObject element, double value) => element.SetValue(MaxOffsetProperty, value);
    public static double GetMaxOffset(DependencyObject element) => (double)element.GetValue(MaxOffsetProperty);

    private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not FrameworkElement element) return;

        if ((bool)e.NewValue)
        {
            var state = new State { Element = element, Transform = EnsureTranslate(element) };
            element.SetValue(StateProperty, state);
            element.MouseMove += OnMouseMove;
            element.MouseLeave += OnMouseLeave;
        }
        else
        {
            element.MouseMove -= OnMouseMove;
            element.MouseLeave -= OnMouseLeave;
            Reset(element, true);
        }
    }

    private static void OnMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
    {
        if (sender is not FrameworkElement element) return;
        if (!MotionSettings.AnimationsEnabled) return;

        State? state = element.GetValue(StateProperty) as State;
        if (state?.Transform is null) return;

        Point position = e.GetPosition(element);
        double strength = Math.Max(0, Math.Min(1, GetStrength(element)));
        double limit = Math.Max(0, GetMaxOffset(element));

        double dx = (position.X - element.ActualWidth / 2) * strength;
        double dy = (position.Y - element.ActualHeight / 2) * strength;

        state.Transform.BeginAnimation(TranslateTransform.XProperty, null);
        state.Transform.BeginAnimation(TranslateTransform.YProperty, null);
        state.Transform.X = Math.Max(-limit, Math.Min(limit, dx));
        state.Transform.Y = Math.Max(-limit, Math.Min(limit, dy));
    }

    private static void OnMouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
    {
        if (sender is FrameworkElement element) Reset(element, true);
    }

    private static void Reset(FrameworkElement element, bool animate)
    {
        State? state = element.GetValue(StateProperty) as State;
        if (state?.Transform is null) return;

        if (!animate || !MotionSettings.AnimationsEnabled)
        {
            state.Transform.BeginAnimation(TranslateTransform.XProperty, null);
            state.Transform.BeginAnimation(TranslateTransform.YProperty, null);
            state.Transform.X = 0;
            state.Transform.Y = 0;
            return;
        }

        var duration = new Duration(TimeSpan.FromMilliseconds(260));
        var ease = new CubicEase { EasingMode = EasingMode.EaseOut };
        state.Transform.BeginAnimation(TranslateTransform.XProperty, new DoubleAnimation(0, duration) { EasingFunction = ease });
        state.Transform.BeginAnimation(TranslateTransform.YProperty, new DoubleAnimation(0, duration) { EasingFunction = ease });
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
