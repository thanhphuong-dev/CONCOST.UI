using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace CONCOST.UI.Themes.Tools;

/// <summary>
/// Adorner that renders a smooth circular mask / crossfade transition from a snapshot of the previous theme.
/// Inspired by the modern Web View Transitions API and the circular-reveal theme transition.
/// </summary>
public class ThemeTransitionAdorner : Adorner
{
    private readonly ImageBrush _screenshotBrush;
    private double _animationProgress = 0.0;
    private readonly Point _centerPoint;
    private readonly double _maxRadius;

    public static readonly DependencyProperty ProgressProperty =
        DependencyProperty.Register(nameof(Progress), typeof(double), typeof(ThemeTransitionAdorner),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender, OnProgressChanged));

    public double Progress
    {
        get => (double)GetValue(ProgressProperty);
        set => SetValue(ProgressProperty, value);
    }

    private static void OnProgressChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ThemeTransitionAdorner adorner)
        {
            adorner._animationProgress = (double)e.NewValue;
        }
    }

    public ThemeTransitionAdorner(UIElement adornedElement, BitmapSource snapshot, Point originPoint)
        : base(adornedElement)
    {
        IsHitTestVisible = false;
        _screenshotBrush = new ImageBrush(snapshot) { Stretch = Stretch.UniformToFill };

        _centerPoint = originPoint;

        double width = snapshot.PixelWidth;
        double height = snapshot.PixelHeight;
        if (adornedElement is FrameworkElement fe && fe.ActualWidth > 0 && fe.ActualHeight > 0)
        {
            width = fe.ActualWidth;
            height = fe.ActualHeight;
        }

        // Calculate maximum radius to fully cover screen from origin point
        double dx = Math.Max(_centerPoint.X, width - _centerPoint.X);
        double dy = Math.Max(_centerPoint.Y, height - _centerPoint.Y);
        _maxRadius = Math.Sqrt(dx * dx + dy * dy);
    }

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);

        if (AdornedElement is not FrameworkElement fe || fe.ActualWidth <= 0 || fe.ActualHeight <= 0)
            return;

        var fullRect = new Rect(0, 0, fe.ActualWidth, fe.ActualHeight);

        // Gentle, pleasant soft radial reveal without flashing rings
        // 1.0 - Progress produces a peaceful fade out of the old theme
        double opacity = Math.Max(0.0, 1.0 - _animationProgress);
        double currentRadius = _maxRadius * _animationProgress;

        if (opacity > 0.001)
        {
            var fullGeom = new RectangleGeometry(fullRect);
            var circleGeom = new EllipseGeometry(_centerPoint, currentRadius, currentRadius);
            var inverseMask = new CombinedGeometry(GeometryCombineMode.Exclude, fullGeom, circleGeom);

            dc.PushOpacity(opacity);
            dc.PushClip(inverseMask);
            dc.DrawRectangle(_screenshotBrush, null, fullRect);
            dc.Pop();
            dc.Pop();
        }
    }
}

public static class ThemeAnimationHelper
{
    /// <summary>
    /// Captures UIElement visual and performs a zero-stutter smooth theme reveal transition.
    /// </summary>
    public static void AnimateThemeChange(Window window, Point? clickOrigin, Action changeThemeAction)
    {
        if (window == null || window.ActualWidth <= 0 || window.ActualHeight <= 0)
        {
            changeThemeAction();
            return;
        }

        if (window.Content is not FrameworkElement rootContent || rootContent.ActualWidth <= 0 || rootContent.ActualHeight <= 0)
        {
            changeThemeAction();
            return;
        }

        try
        {
            // Capture the content element directly (1:1 with Content bounds, avoids window titlebar/border DPI offsets)
            int width = (int)Math.Max(1, rootContent.ActualWidth);
            int height = (int)Math.Max(1, rootContent.ActualHeight);

            var rtb = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
            rtb.Render(rootContent);
            rtb.Freeze();

            Point origin = clickOrigin ?? new Point(rootContent.ActualWidth - 60, 30);

            // Convert origin to rootContent coordinates
            if (window.Content is Visual contentVis)
            {
                var layer = AdornerLayer.GetAdornerLayer(contentVis);
                if (layer != null)
                {
                    var adorner = new ThemeTransitionAdorner(rootContent, rtb, origin);
                    layer.Add(adorner);

                    // Apply the theme change immediately under the snapshot adorner
                    changeThemeAction();

                    var anim = new DoubleAnimation
                    {
                        From = 0.0,
                        To = 1.0,
                        Duration = TimeSpan.FromMilliseconds(320),
                        EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                    };

                    anim.Completed += (s, e) =>
                    {
                        layer.Remove(adorner);
                    };

                    adorner.BeginAnimation(ThemeTransitionAdorner.ProgressProperty, anim);
                    return;
                }
            }

            changeThemeAction();
        }
        catch
        {
            changeThemeAction();
        }
    }

    /// <summary>
    /// Captures FrameworkElement visual (UserControl / Window in Class Library / Plugin DLL) and performs a smooth theme reveal transition.
    /// </summary>
    public static void AnimateThemeChange(FrameworkElement rootElement, Point? clickOrigin, Action changeThemeAction)
    {
        if (rootElement == null || rootElement.ActualWidth <= 0 || rootElement.ActualHeight <= 0)
        {
            changeThemeAction();
            return;
        }

        try
        {
            int width = (int)Math.Max(1, rootElement.ActualWidth);
            int height = (int)Math.Max(1, rootElement.ActualHeight);

            var rtb = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
            rtb.Render(rootElement);
            rtb.Freeze();

            Point origin = clickOrigin ?? new Point(rootElement.ActualWidth / 2.0, rootElement.ActualHeight / 2.0);

            var layer = AdornerLayer.GetAdornerLayer(rootElement);
            if (layer != null)
            {
                var adorner = new ThemeTransitionAdorner(rootElement, rtb, origin);
                layer.Add(adorner);

                changeThemeAction();

                var anim = new DoubleAnimation
                {
                    From = 0.0,
                    To = 1.0,
                    Duration = TimeSpan.FromMilliseconds(320),
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                };

                anim.Completed += (s, e) =>
                {
                    layer.Remove(adorner);
                };

                adorner.BeginAnimation(ThemeTransitionAdorner.ProgressProperty, anim);
                return;
            }

            changeThemeAction();
        }
        catch
        {
            changeThemeAction();
        }
    }
}
