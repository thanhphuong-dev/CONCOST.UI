using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using CONCOST.UI.Core;

namespace CONCOST.UI.Controls;

/// <summary>How a <see cref="PageTransitionHost"/> brings new content in.</summary>
public enum PageTransition
{
    /// <summary>Swap instantly, no animation.</summary>
    None,

    /// <summary>Cross-fade only.</summary>
    Fade,

    /// <summary>Content enters from the right and settles into place.</summary>
    SlideLeft,

    /// <summary>Content enters from the left.</summary>
    SlideRight,

    /// <summary>Content rises from below.</summary>
    SlideUp,

    /// <summary>Content drops in from above.</summary>
    SlideDown,

    /// <summary>Content scales up from slightly smaller while fading in.</summary>
    Scale
}

/// <summary>
/// A content host that animates the swap instead of snapping: drop a page, a wizard step or a
/// tab body in and the new content fades / slides into place.
/// </summary>
/// <remarks>
/// The animation runs on the host itself, not on a storyboard in the template, so it works with any
/// content and needs no cooperation from the page. It is skipped when the control is not loaded,
/// when <see cref="IsTransitionEnabled"/> is false, or when motion is turned off globally.
/// </remarks>
public class PageTransitionHost : ContentControl
{
    static PageTransitionHost()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(PageTransitionHost),
            new FrameworkPropertyMetadata(typeof(PageTransitionHost)));
    }

    public static readonly DependencyProperty TransitionProperty =
        DependencyProperty.Register(nameof(Transition), typeof(PageTransition), typeof(PageTransitionHost),
            new PropertyMetadata(PageTransition.Fade));

    public static readonly DependencyProperty DurationProperty =
        DependencyProperty.Register(nameof(Duration), typeof(TimeSpan), typeof(PageTransitionHost),
            new PropertyMetadata(TimeSpan.FromMilliseconds(260)));

    public static readonly DependencyProperty IsTransitionEnabledProperty =
        DependencyProperty.Register(nameof(IsTransitionEnabled), typeof(bool), typeof(PageTransitionHost),
            new PropertyMetadata(true));

    public static readonly DependencyProperty OffsetProperty =
        DependencyProperty.Register(nameof(Offset), typeof(double), typeof(PageTransitionHost),
            new PropertyMetadata(22.0));

    private readonly TranslateTransform _slide = new();
    private readonly ScaleTransform _scale = new(1, 1);
    private readonly TransformGroup _transform = new();

    /// <summary>Which entrance to play when the content changes. Defaults to <see cref="PageTransition.Fade"/>.</summary>
    public PageTransition Transition
    {
        get => (PageTransition)GetValue(TransitionProperty);
        set => SetValue(TransitionProperty, value);
    }

    /// <summary>How long the entrance takes. Defaults to 260 ms.</summary>
    public TimeSpan Duration
    {
        get => (TimeSpan)GetValue(DurationProperty);
        set => SetValue(DurationProperty, value);
    }

    /// <summary>Set to false to swap content without any animation.</summary>
    public bool IsTransitionEnabled
    {
        get => (bool)GetValue(IsTransitionEnabledProperty);
        set => SetValue(IsTransitionEnabledProperty, value);
    }

    /// <summary>How far the content travels, in device-independent pixels. Defaults to 22.</summary>
    public double Offset
    {
        get => (double)GetValue(OffsetProperty);
        set => SetValue(OffsetProperty, value);
    }

    public PageTransitionHost()
    {
        _transform.Children.Add(_scale);
        _transform.Children.Add(_slide);
        RenderTransform = _transform;
        RenderTransformOrigin = new Point(0.5, 0.5);
        ClipToBounds = true;
    }

    /// <summary>True while an entrance animation is running.</summary>
    public bool IsTransitioning { get; private set; }

    protected override void OnContentChanged(object oldContent, object newContent)
    {
        base.OnContentChanged(oldContent, newContent);

        if (newContent is null)
        {
            ResetTransforms();
            return;
        }

        if (!IsLoaded || !IsTransitionEnabled || !MotionSettings.AnimationsEnabled || Transition == PageTransition.None)
        {
            ResetTransforms();
            return;
        }

        Play();
    }

    /// <summary>Replays the entrance for the current content.</summary>
    public void Play()
    {
        double offset = Offset;
        double fromX = 0;
        double fromY = 0;
        double fromScale = 1;
        double fromOpacity = 1;

        switch (Transition)
        {
            case PageTransition.Fade:
                fromOpacity = 0;
                break;
            case PageTransition.SlideLeft:
                fromX = offset;
                fromOpacity = 0;
                break;
            case PageTransition.SlideRight:
                fromX = -offset;
                fromOpacity = 0;
                break;
            case PageTransition.SlideUp:
                fromY = offset;
                fromOpacity = 0;
                break;
            case PageTransition.SlideDown:
                fromY = -offset;
                fromOpacity = 0;
                break;
            case PageTransition.Scale:
                fromScale = 0.94;
                fromOpacity = 0;
                break;
            default:
                ResetTransforms();
                return;
        }

        var duration = new Duration(Duration <= TimeSpan.Zero ? TimeSpan.FromMilliseconds(1) : Duration);
        var ease = new CubicEase { EasingMode = EasingMode.EaseOut };

        Opacity = fromOpacity;
        _slide.X = fromX;
        _slide.Y = fromY;
        _scale.ScaleX = fromScale;
        _scale.ScaleY = fromScale;

        IsTransitioning = true;

        var fade = new DoubleAnimation(1, duration) { EasingFunction = ease, FillBehavior = FillBehavior.Stop };
        fade.Completed += (_, _) =>
        {
            IsTransitioning = false;
            BeginAnimation(OpacityProperty, null);
            Opacity = 1;
        };

        BeginAnimation(OpacityProperty, fade);
        _slide.BeginAnimation(TranslateTransform.XProperty, new DoubleAnimation(0, duration) { EasingFunction = ease, FillBehavior = FillBehavior.Stop });
        _slide.BeginAnimation(TranslateTransform.YProperty, new DoubleAnimation(0, duration) { EasingFunction = ease, FillBehavior = FillBehavior.Stop });
        _scale.BeginAnimation(ScaleTransform.ScaleXProperty, new DoubleAnimation(1, duration) { EasingFunction = ease, FillBehavior = FillBehavior.Stop });
        _scale.BeginAnimation(ScaleTransform.ScaleYProperty, new DoubleAnimation(1, duration) { EasingFunction = ease, FillBehavior = FillBehavior.Stop });
    }

    private void ResetTransforms()
    {
        BeginAnimation(OpacityProperty, null);
        _slide.BeginAnimation(TranslateTransform.XProperty, null);
        _slide.BeginAnimation(TranslateTransform.YProperty, null);
        _scale.BeginAnimation(ScaleTransform.ScaleXProperty, null);
        _scale.BeginAnimation(ScaleTransform.ScaleYProperty, null);

        Opacity = 1;
        _slide.X = 0;
        _slide.Y = 0;
        _scale.ScaleX = 1;
        _scale.ScaleY = 1;
        IsTransitioning = false;
    }
}
