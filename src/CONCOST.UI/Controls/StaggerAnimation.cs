using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace CONCOST.UI.Controls;

/// <summary>
/// Animates the items of an <see cref="ItemsControl"/> in one after another instead of all at once, so a
/// list or a grid of cards arrives as a wave rather than a flash.
/// </summary>
/// <remarks>
/// <para>
/// This is an attached behaviour, not a control: the delay is derived from the item's position, so it
/// works with any panel and any item template without the items having to know about it.
/// </para>
/// <para>
/// Containers are tracked by reference, which matters because a virtualising panel reuses them: an item
/// scrolled back into view must not replay its entrance on every scroll.
/// </para>
/// </remarks>
public static class StaggerAnimation
{
    public static readonly DependencyProperty IsEnabledProperty =
        DependencyProperty.RegisterAttached("IsEnabled", typeof(bool), typeof(StaggerAnimation),
            new PropertyMetadata(false, OnIsEnabledChanged));

    /// <summary>Turns the entrance animation on for this items control.</summary>
    public static bool GetIsEnabled(DependencyObject element) => (bool)element.GetValue(IsEnabledProperty);

    public static void SetIsEnabled(DependencyObject element, bool value) =>
        element.SetValue(IsEnabledProperty, value);

    public static readonly DependencyProperty DelayProperty =
        DependencyProperty.RegisterAttached("Delay", typeof(double), typeof(StaggerAnimation),
            new PropertyMetadata(45.0));

    /// <summary>Milliseconds between one item and the next. Default 45.</summary>
    public static double GetDelay(DependencyObject element) => (double)element.GetValue(DelayProperty);

    public static void SetDelay(DependencyObject element, double value) =>
        element.SetValue(DelayProperty, value);

    public static readonly DependencyProperty DurationProperty =
        DependencyProperty.RegisterAttached("Duration", typeof(double), typeof(StaggerAnimation),
            new PropertyMetadata(320.0));

    /// <summary>Milliseconds each item takes to arrive. Default 320.</summary>
    public static double GetDuration(DependencyObject element) => (double)element.GetValue(DurationProperty);

    public static void SetDuration(DependencyObject element, double value) =>
        element.SetValue(DurationProperty, value);

    public static readonly DependencyProperty OffsetYProperty =
        DependencyProperty.RegisterAttached("OffsetY", typeof(double), typeof(StaggerAnimation),
            new PropertyMetadata(14.0));

    /// <summary>How far below its final place an item starts, in pixels. Default 14.</summary>
    public static double GetOffsetY(DependencyObject element) => (double)element.GetValue(OffsetYProperty);

    public static void SetOffsetY(DependencyObject element, double value) =>
        element.SetValue(OffsetYProperty, value);

    public static readonly DependencyProperty MaxItemsProperty =
        DependencyProperty.RegisterAttached("MaxItems", typeof(int), typeof(StaggerAnimation),
            new PropertyMetadata(24));

    /// <summary>
    /// Stop increasing the delay after this many items, so a long list does not take ten seconds to
    /// finish arriving. Default 24.
    /// </summary>
    public static int GetMaxItems(DependencyObject element) => (int)element.GetValue(MaxItemsProperty);

    public static void SetMaxItems(DependencyObject element, int value) =>
        element.SetValue(MaxItemsProperty, value);

    private sealed class State
    {
        public readonly HashSet<object> Animated = new();
        public bool Hooking;
    }

    private static readonly Dictionary<ItemsControl, State> States = new();

    private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not ItemsControl items) return;

        if ((bool)e.NewValue)
        {
            if (States.ContainsKey(items)) return;

            var state = new State();
            States[items] = state;

            items.ItemContainerGenerator.StatusChanged += OnGeneratorStatusChanged;
            AnimateReadyContainers(items, state);
        }
        else if (States.ContainsKey(items))
        {
            States.Remove(items);
            items.ItemContainerGenerator.StatusChanged -= OnGeneratorStatusChanged;
        }
    }

    private static void OnGeneratorStatusChanged(object? sender, EventArgs e)
    {
        if (sender is not System.Windows.Controls.ItemContainerGenerator generator) return;
        if (generator.Status != System.Windows.Controls.Primitives.GeneratorStatus.ContainersGenerated) return;

        foreach (var pair in States)
        {
            if (ReferenceEquals(pair.Key.ItemContainerGenerator, generator))
            {
                AnimateReadyContainers(pair.Key, pair.Value);
                return;
            }
        }
    }

    private static void AnimateReadyContainers(ItemsControl items, State state)
    {
        if (state.Hooking) return;

        state.Hooking = true;
        try
        {
            int delay = (int)Math.Max(0, GetDelay(items));
            int duration = (int)Math.Max(1, GetDuration(items));
            double offset = GetOffsetY(items);
            int max = Math.Max(1, GetMaxItems(items));

            for (int i = 0; i < items.Items.Count; i++)
            {
                if (items.ItemContainerGenerator.ContainerFromIndex(i) is not FrameworkElement container) continue;
                if (!state.Animated.Add(container)) continue;

                Animate(container, i, delay, duration, offset, max);
            }
        }
        finally
        {
            state.Hooking = false;
        }
    }

    private static void Animate(FrameworkElement container, int index, int delay, int duration,
        double offset, int max)
    {
        if (!Core.MotionSettings.AnimationsEnabled) return;

        var translate = new TranslateTransform(0, offset);
        var original = container.RenderTransform;

        // Keep whatever transform the container already had; the entrance is added on top of it.
        if (original is null || original == Transform.Identity)
        {
            container.RenderTransform = translate;
        }
        else
        {
            var group = new TransformGroup();
            group.Children.Add(original);
            group.Children.Add(translate);
            container.RenderTransform = group;
        }

        var begin = TimeSpan.FromMilliseconds((double)Math.Min(index, max) * delay);
        var length = TimeSpan.FromMilliseconds(duration);
        var ease = new CubicEase { EasingMode = EasingMode.EaseOut };

        var move = new DoubleAnimation(offset, 0, length)
        {
            BeginTime = begin,
            EasingFunction = ease
        };

        var fade = new DoubleAnimation(0, 1, length)
        {
            BeginTime = begin,
            EasingFunction = ease
        };

        move.Completed += (_, _) =>
        {
            container.RenderTransform = original;
            container.Opacity = 1;
        };

        container.Opacity = 0;
        translate.BeginAnimation(TranslateTransform.YProperty, move);
        container.BeginAnimation(UIElement.OpacityProperty, fade);
    }
}
