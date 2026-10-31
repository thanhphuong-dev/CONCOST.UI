using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace CONCOST.UI.Controls;

/// <summary>
/// Animates an element from a rectangle it used to occupy to the place it occupies now — the "this card
/// grew out of that thumbnail" move between two views.
/// </summary>
/// <remarks>
/// <para>
/// The trick is that nothing is animated but a transform: the element is already laid out in its final
/// position, and it is scaled and shifted back to the old rectangle for the first frame, then eased to
/// identity. That means no layout thrash, and the element stays interactive and correctly measured the
/// whole time.
/// </para>
/// <para>
/// Both rectangles have to be in the same coordinate space — normally the parent the element lives in.
/// </para>
/// </remarks>
public static class SharedElementTransition
{
    /// <summary>
    /// Runs the transition. <paramref name="fromRect"/> is where the element should appear to come from,
    /// in the coordinates of <paramref name="element"/>'s parent.
    /// </summary>
    public static void Run(FrameworkElement element, Rect fromRect, double milliseconds = 340,
        IEasingFunction? easing = null)
    {
        if (element is null || fromRect.IsEmpty) return;
        if (!Core.MotionSettings.AnimationsEnabled) return;
        if (element.Parent is not UIElement parent) return;

        var origin = element.TranslatePoint(new Point(0, 0), parent);
        var size = element.RenderSize;

        if (size.Width <= 0 || size.Height <= 0) return;

        double scaleX = fromRect.Width / size.Width;
        double scaleY = fromRect.Height / size.Height;

        var translate = new TranslateTransform(fromRect.X - origin.X, fromRect.Y - origin.Y);
        var scale = new ScaleTransform(scaleX, scaleY);

        var group = new TransformGroup();
        group.Children.Add(scale);
        group.Children.Add(translate);

        element.RenderTransformOrigin = new Point(0, 0);
        element.RenderTransform = group;

        var length = TimeSpan.FromMilliseconds(Math.Max(1, milliseconds));
        var ease = easing ?? new CubicEase { EasingMode = EasingMode.EaseOut };

        var toScaleX = new DoubleAnimation(1, length) { EasingFunction = ease };
        var toScaleY = new DoubleAnimation(1, length) { EasingFunction = ease };
        var toX = new DoubleAnimation(0, length) { EasingFunction = ease };
        var toY = new DoubleAnimation(0, length) { EasingFunction = ease };

        toY.Completed += (_, _) =>
        {
            // Hand the element back untouched: a leftover transform breaks hit-testing and clips.
            element.RenderTransform = null;
            element.Opacity = 1;
        };

        element.Opacity = 0.35;

        scale.BeginAnimation(ScaleTransform.ScaleXProperty, toScaleX);
        scale.BeginAnimation(ScaleTransform.ScaleYProperty, toScaleY);
        translate.BeginAnimation(TranslateTransform.XProperty, toX);
        translate.BeginAnimation(TranslateTransform.YProperty, toY);
        element.BeginAnimation(UIElement.OpacityProperty, new DoubleAnimation(1, length));
    }

    /// <summary>
    /// Runs the transition from another element's current bounds — the usual "expand from that card"
    /// case, with the coordinate conversion done here.
    /// </summary>
    public static void RunFrom(FrameworkElement element, FrameworkElement origin, double milliseconds = 340)
    {
        if (element is null || origin is null) return;
        if (element.Parent is not UIElement parent) return;

        var topLeft = origin.TranslatePoint(new Point(0, 0), parent);
        Run(element, new Rect(topLeft, origin.RenderSize), milliseconds);
    }
}
