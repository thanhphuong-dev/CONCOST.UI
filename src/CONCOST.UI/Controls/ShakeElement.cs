using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using CONCOST.UI.Core;

namespace CONCOST.UI.Controls;

/// <summary>
/// Attached behaviour that shakes an element horizontally — the classic “invalid input”
/// feedback. Works on any <see cref="FrameworkElement"/>; the existing RenderTransform is
/// preserved by wrapping it in a group.
/// </summary>
public static class ShakeElement
{
    public static readonly DependencyProperty IsShakingProperty =
        DependencyProperty.RegisterAttached("IsShaking", typeof(bool), typeof(ShakeElement),
            new FrameworkPropertyMetadata(false, OnIsShakingChanged));

    public static readonly DependencyProperty AmplitudeProperty =
        DependencyProperty.RegisterAttached("Amplitude", typeof(double), typeof(ShakeElement),
            new FrameworkPropertyMetadata(8.0));

    public static readonly DependencyProperty DurationProperty =
        DependencyProperty.RegisterAttached("Duration", typeof(Duration), typeof(ShakeElement),
            new FrameworkPropertyMetadata(new Duration(TimeSpan.FromMilliseconds(360))));

    public static void SetIsShaking(DependencyObject element, bool value) => element.SetValue(IsShakingProperty, value);
    public static bool GetIsShaking(DependencyObject element) => (bool)element.GetValue(IsShakingProperty);

    public static void SetAmplitude(DependencyObject element, double value) => element.SetValue(AmplitudeProperty, value);
    public static double GetAmplitude(DependencyObject element) => (double)element.GetValue(AmplitudeProperty);

    public static void SetDuration(DependencyObject element, Duration value) => element.SetValue(DurationProperty, value);
    public static Duration GetDuration(DependencyObject element) => (Duration)element.GetValue(DurationProperty);

    /// <summary>Shakes the element once. Convenience for code-behind and view models.</summary>
    public static void Shake(FrameworkElement? element)
    {
        if (element is null) return;
        SetIsShaking(element, false);
        SetIsShaking(element, true);
    }

    private static void OnIsShakingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not FrameworkElement element || !(bool)e.NewValue) return;

        if (!MotionSettings.AnimationsEnabled)
        {
            SetIsShaking(element, false);
            return;
        }

        TranslateTransform translate = EnsureTranslate(element);
        double amplitude = Math.Max(1, GetAmplitude(element));
        Duration duration = GetDuration(element);
        TimeSpan total = duration.HasTimeSpan ? duration.TimeSpan : TimeSpan.FromMilliseconds(360);

        var animation = new DoubleAnimationUsingKeyFrames
        {
            Duration = new Duration(total),
            FillBehavior = FillBehavior.Stop
        };

        int steps = 6;
        for (int i = 1; i <= steps; i++)
        {
            double decay = 1.0 - (double)i / (steps + 1);
            double offset = (i % 2 == 0 ? 1 : -1) * amplitude * decay;
            animation.KeyFrames.Add(new EasingDoubleKeyFrame(offset,
                KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(total.TotalMilliseconds * i / steps))));
        }

        animation.KeyFrames.Add(new LinearDoubleKeyFrame(0, KeyTime.FromTimeSpan(total)));
        animation.Completed += (_, _) => SetIsShaking(element, false);

        translate.BeginAnimation(TranslateTransform.XProperty, animation);
    }

    /// <summary>
    /// Returns the element's <see cref="TranslateTransform"/>, creating one (and preserving
    /// any existing RenderTransform) the first time the behaviour is used.
    /// </summary>
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
