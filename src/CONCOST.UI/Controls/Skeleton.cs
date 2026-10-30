using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace CONCOST.UI.Controls;

public enum SkeletonShape
{
    /// <summary>
    /// Standard rectangular block with customizable CornerRadius.
    /// </summary>
    Rectangle,

    /// <summary>
    /// Perfectly rounded circular placeholder for user avatars and circular icons.
    /// </summary>
    Circle,

    /// <summary>
    /// Sleek typography text line placeholder with rounded ends.
    /// </summary>
    TextLine,

    /// <summary>
    /// Card-sized placeholder container for dashboard metrics and preview cards.
    /// </summary>
    Card
}

/// <summary>
/// Modern Shimmer/Skeleton loading placeholder control.
/// Renders an animated gradient sweep across placeholder shapes during asynchronous data fetching,
/// and smoothly transitions to actual content once loading completes.
/// </summary>
[TemplatePart(Name = "PART_SkeletonHost", Type = typeof(Border))]
[TemplatePart(Name = "PART_ContentPresenter", Type = typeof(ContentPresenter))]
public class Skeleton : ContentControl
{
    private Border? _skeletonHost;
    private ContentPresenter? _contentPresenter;
    private TranslateTransform? _translateTransform;

    static Skeleton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(Skeleton), new FrameworkPropertyMetadata(typeof(Skeleton)));
    }

    public static readonly DependencyProperty IsLoadingProperty =
        DependencyProperty.Register(nameof(IsLoading), typeof(bool), typeof(Skeleton),
            new FrameworkPropertyMetadata(true, OnIsLoadingChanged));

    public static readonly DependencyProperty SkeletonShapeProperty =
        DependencyProperty.Register(nameof(SkeletonShape), typeof(SkeletonShape), typeof(Skeleton),
            new FrameworkPropertyMetadata(SkeletonShape.Rectangle, OnShapeOrSizeChanged));

    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(Skeleton),
            new FrameworkPropertyMetadata(new CornerRadius(6), FrameworkPropertyMetadataOptions.AffectsRender, OnShapeOrSizeChanged));

    public static readonly DependencyProperty BaseBrushProperty =
        DependencyProperty.Register(nameof(BaseBrush), typeof(Brush), typeof(Skeleton),
            new FrameworkPropertyMetadata(null, OnBrushSettingsChanged));

    public static readonly DependencyProperty HighlightBrushProperty =
        DependencyProperty.Register(nameof(HighlightBrush), typeof(Brush), typeof(Skeleton),
            new FrameworkPropertyMetadata(null, OnBrushSettingsChanged));

    public static readonly DependencyProperty DurationProperty =
        DependencyProperty.Register(nameof(Duration), typeof(TimeSpan), typeof(Skeleton),
            new FrameworkPropertyMetadata(TimeSpan.FromSeconds(1.5), OnBrushSettingsChanged));

    public static readonly DependencyProperty IsWaveEnabledProperty =
        DependencyProperty.Register(nameof(IsWaveEnabled), typeof(bool), typeof(Skeleton),
            new FrameworkPropertyMetadata(true, OnBrushSettingsChanged));

    public bool IsLoading
    {
        get => (bool)GetValue(IsLoadingProperty);
        set => SetValue(IsLoadingProperty, value);
    }

    public SkeletonShape SkeletonShape
    {
        get => (SkeletonShape)GetValue(SkeletonShapeProperty);
        set => SetValue(SkeletonShapeProperty, value);
    }

    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    public Brush? BaseBrush
    {
        get => (Brush?)GetValue(BaseBrushProperty);
        set => SetValue(BaseBrushProperty, value);
    }

    public Brush? HighlightBrush
    {
        get => (Brush?)GetValue(HighlightBrushProperty);
        set => SetValue(HighlightBrushProperty, value);
    }

    public TimeSpan Duration
    {
        get => (TimeSpan)GetValue(DurationProperty);
        set => SetValue(DurationProperty, value);
    }

    public bool IsWaveEnabled
    {
        get => (bool)GetValue(IsWaveEnabledProperty);
        set => SetValue(IsWaveEnabledProperty, value);
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        _skeletonHost = GetTemplateChild("PART_SkeletonHost") as Border;
        _contentPresenter = GetTemplateChild("PART_ContentPresenter") as ContentPresenter;

        ApplyShimmerBrush();
        UpdateShapeAndLayout();
        UpdateState(false);
    }

    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
        base.OnRenderSizeChanged(sizeInfo);
        UpdateShapeAndLayout();
    }

    private static void OnIsLoadingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Skeleton skeleton)
        {
            skeleton.UpdateState(true);
        }
    }

    private static void OnShapeOrSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Skeleton skeleton)
        {
            skeleton.UpdateShapeAndLayout();
        }
    }

    private static void OnBrushSettingsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Skeleton skeleton)
        {
            skeleton.ApplyShimmerBrush();
        }
    }

    private void UpdateState(bool animate)
    {
        if (_skeletonHost == null || _contentPresenter == null)
            return;

        if (IsLoading)
        {
            if (animate)
            {
                var fadeOutContent = new DoubleAnimation(1.0, 0.0, new Duration(TimeSpan.FromMilliseconds(200)));
                var fadeInSkeleton = new DoubleAnimation(0.0, 1.0, new Duration(TimeSpan.FromMilliseconds(200)));
                _contentPresenter.BeginAnimation(UIElement.OpacityProperty, fadeOutContent);
                _skeletonHost.BeginAnimation(UIElement.OpacityProperty, fadeInSkeleton);
            }
            else
            {
                _contentPresenter.Opacity = 0.0;
                _skeletonHost.Opacity = 1.0;
            }
            _skeletonHost.Visibility = Visibility.Visible;
            _contentPresenter.Visibility = Content != null ? Visibility.Visible : Visibility.Collapsed;
            StartShimmerAnimation();
        }
        else
        {
            if (animate)
            {
                var fadeOutSkeleton = new DoubleAnimation(1.0, 0.0, new Duration(TimeSpan.FromMilliseconds(250)));
                var fadeInContent = new DoubleAnimation(0.0, 1.0, new Duration(TimeSpan.FromMilliseconds(250)));
                fadeOutSkeleton.Completed += (s, e) =>
                {
                    if (!IsLoading && _skeletonHost != null)
                    {
                        _skeletonHost.Visibility = Visibility.Collapsed;
                        StopShimmerAnimation();
                    }
                };
                _skeletonHost.BeginAnimation(UIElement.OpacityProperty, fadeOutSkeleton);
                _contentPresenter.BeginAnimation(UIElement.OpacityProperty, fadeInContent);
            }
            else
            {
                _skeletonHost.Opacity = 0.0;
                _skeletonHost.Visibility = Visibility.Collapsed;
                _contentPresenter.Opacity = 1.0;
                StopShimmerAnimation();
            }
            _contentPresenter.Visibility = Visibility.Visible;
        }
    }

    private void UpdateShapeAndLayout()
    {
        if (_skeletonHost == null)
            return;

        switch (SkeletonShape)
        {
            case SkeletonShape.Circle:
                double side = Math.Min(ActualWidth > 0 ? ActualWidth : Width, ActualHeight > 0 ? ActualHeight : Height);
                if (double.IsNaN(side) || side <= 0) side = 40;
                _skeletonHost.CornerRadius = new CornerRadius(side / 2.0);
                break;

            case SkeletonShape.TextLine:
                _skeletonHost.CornerRadius = new CornerRadius(4);
                break;

            case SkeletonShape.Card:
                _skeletonHost.CornerRadius = new CornerRadius(10);
                break;

            case SkeletonShape.Rectangle:
            default:
                _skeletonHost.CornerRadius = CornerRadius;
                break;
        }
    }

    private void ApplyShimmerBrush()
    {
        if (_skeletonHost == null)
            return;

        Color baseColor = ResolveColor(BaseBrush, "CONCOST.Brush.SurfaceSecondary", Color.FromRgb(30, 41, 59));
        Color highlightColor = ResolveColor(HighlightBrush, "CONCOST.Brush.SurfaceHover", Color.FromRgb(51, 65, 85));

        var gradient = new LinearGradientBrush
        {
            StartPoint = new Point(0, 0),
            EndPoint = new Point(1, 0.25)
        };

        gradient.GradientStops.Add(new GradientStop(baseColor, 0.0));
        gradient.GradientStops.Add(new GradientStop(highlightColor, 0.5));
        gradient.GradientStops.Add(new GradientStop(baseColor, 1.0));

        _translateTransform = new TranslateTransform(-1.0, 0.0);
        gradient.RelativeTransform = _translateTransform;

        _skeletonHost.Background = gradient;

        if (IsLoading)
        {
            StartShimmerAnimation();
        }
    }

    private void StartShimmerAnimation()
    {
        if (_translateTransform == null || !IsWaveEnabled)
            return;

        var anim = new DoubleAnimation(-1.0, 1.0, new Duration(Duration))
        {
            RepeatBehavior = RepeatBehavior.Forever
        };

        _translateTransform.BeginAnimation(TranslateTransform.XProperty, anim);
    }

    private void StopShimmerAnimation()
    {
        _translateTransform?.BeginAnimation(TranslateTransform.XProperty, null);
    }

    private Color ResolveColor(Brush? brush, string resourceKey, Color fallback)
    {
        if (brush is SolidColorBrush scb)
            return scb.Color;

        if (TryFindResource(resourceKey) is SolidColorBrush resScb)
            return resScb.Color;

        return fallback;
    }
}
