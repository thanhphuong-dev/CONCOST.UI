using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using WpfCanvas = System.Windows.Controls.Canvas;

namespace CONCOST.UI.Controls;

public enum RippleMode
{
    /// <summary>
    /// Ripple originates from the exact mouse click coordinate.
    /// </summary>
    Target,

    /// <summary>
    /// Ripple originates from the geometric center of the element.
    /// </summary>
    Center
}

/// <summary>
/// A Material-Design inspired container that produces an expanding circular ripple wave
/// upon pointer interaction, with hardware-accelerated transforms and corner clipping.
/// </summary>
[TemplatePart(Name = "PART_Container", Type = typeof(Border))]
[TemplatePart(Name = "PART_Canvas", Type = typeof(WpfCanvas))]
public class Ripple : ContentControl
{
    private Border? _container;
    private WpfCanvas? _canvas;

    static Ripple()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(Ripple), new FrameworkPropertyMetadata(typeof(Ripple)));
    }

    public static readonly DependencyProperty RippleBrushProperty =
        DependencyProperty.Register(nameof(RippleBrush), typeof(Brush), typeof(Ripple),
            new PropertyMetadata(null));

    public static readonly DependencyProperty MaxOpacityProperty =
        DependencyProperty.Register(nameof(MaxOpacity), typeof(double), typeof(Ripple),
            new PropertyMetadata(0.35), value => value is double opacity &&
                NumericCompatibility.IsFinite(opacity) && opacity >= 0 && opacity <= 1);

    public static readonly DependencyProperty DurationProperty =
        DependencyProperty.Register(nameof(Duration), typeof(TimeSpan), typeof(Ripple),
            new PropertyMetadata(TimeSpan.FromMilliseconds(450)), value => value is TimeSpan duration &&
                duration > TimeSpan.Zero && duration <= TimeSpan.FromSeconds(10));

    public static readonly DependencyProperty MaxConcurrentRipplesProperty =
        DependencyProperty.Register(nameof(MaxConcurrentRipples), typeof(int), typeof(Ripple),
            new PropertyMetadata(4), value => value is int count && count is >= 1 and <= 32);

    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(Ripple),
            new FrameworkPropertyMetadata(new CornerRadius(0), FrameworkPropertyMetadataOptions.AffectsRender, OnCornerRadiusChanged));

    public static readonly DependencyProperty IsRippleEnabledProperty =
        DependencyProperty.Register(nameof(IsRippleEnabled), typeof(bool), typeof(Ripple),
            new PropertyMetadata(true));

    public static readonly DependencyProperty ModeProperty =
        DependencyProperty.Register(nameof(Mode), typeof(RippleMode), typeof(Ripple),
            new PropertyMetadata(RippleMode.Target));

    public Brush? RippleBrush
    {
        get => (Brush?)GetValue(RippleBrushProperty);
        set => SetValue(RippleBrushProperty, value);
    }

    public double MaxOpacity
    {
        get => (double)GetValue(MaxOpacityProperty);
        set => SetValue(MaxOpacityProperty, value);
    }

    public TimeSpan Duration
    {
        get => (TimeSpan)GetValue(DurationProperty);
        set => SetValue(DurationProperty, value);
    }

    /// <summary>Maximum number of simultaneous waves retained by this control.</summary>
    public int MaxConcurrentRipples
    {
        get => (int)GetValue(MaxConcurrentRipplesProperty);
        set => SetValue(MaxConcurrentRipplesProperty, value);
    }

    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    public bool IsRippleEnabled
    {
        get => (bool)GetValue(IsRippleEnabledProperty);
        set => SetValue(IsRippleEnabledProperty, value);
    }

    public RippleMode Mode
    {
        get => (RippleMode)GetValue(ModeProperty);
        set => SetValue(ModeProperty, value);
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        _container = GetTemplateChild("PART_Container") as Border;
        _canvas = GetTemplateChild("PART_Canvas") as WpfCanvas;
        UpdateClip();
    }

    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
        base.OnRenderSizeChanged(sizeInfo);
        UpdateClip();
    }

    private static void OnCornerRadiusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Ripple ripple)
        {
            ripple.UpdateClip();
        }
    }

    private void UpdateClip()
    {
        if (_container == null || ActualWidth <= 0 || ActualHeight <= 0)
            return;

        var cr = CornerRadius;
        if (cr.TopLeft > 0 || cr.TopRight > 0 || cr.BottomRight > 0 || cr.BottomLeft > 0)
        {
            _container.Clip = RippleGeometry.CreateRoundedRectangle(new Size(ActualWidth, ActualHeight), cr);
        }
        else
        {
            _container.Clip = null;
        }
    }

    protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnPreviewMouseLeftButtonDown(e);

        if (!IsRippleEnabled || _canvas == null || ActualWidth <= 0 || ActualHeight <= 0)
            return;

        Point origin = Mode == RippleMode.Center
            ? new Point(ActualWidth / 2.0, ActualHeight / 2.0)
            : e.GetPosition(this);

        TriggerRipple(origin);
    }

    /// <summary>
    /// Triggers a ripple wave programmatically from the specified point.
    /// </summary>
    public void TriggerRipple(Point origin)
    {
        if (!IsRippleEnabled || _canvas == null || ActualWidth <= 0 || ActualHeight <= 0)
            return;

        var canvas = _canvas;
        origin = new Point(NumericCompatibility.Clamp(origin.X, 0, ActualWidth), NumericCompatibility.Clamp(origin.Y, 0, ActualHeight));

        double maxDistX = Math.Max(origin.X, ActualWidth - origin.X);
        double maxDistY = Math.Max(origin.Y, ActualHeight - origin.Y);
        double targetRadius = Math.Sqrt(maxDistX * maxDistX + maxDistY * maxDistY) * 1.15;
        double targetDiameter = targetRadius * 2.0;

        while (canvas.Children.OfType<Ellipse>().Count() >= MaxConcurrentRipples)
        {
            if (canvas.Children.OfType<Ellipse>().FirstOrDefault() is not { } oldest) break;
            oldest.BeginAnimation(UIElement.OpacityProperty, null);
            oldest.RenderTransform.BeginAnimation(ScaleTransform.ScaleXProperty, null);
            oldest.RenderTransform.BeginAnimation(ScaleTransform.ScaleYProperty, null);
            canvas.Children.Remove(oldest);
        }

        Brush fillBrush = RippleBrush ?? ResolveDefaultBrush();

        var ellipse = new Ellipse
        {
            Width = targetDiameter,
            Height = targetDiameter,
            Fill = fillBrush,
            Opacity = MaxOpacity,
            IsHitTestVisible = false
        };

        WpfCanvas.SetLeft(ellipse, origin.X - targetRadius);
        WpfCanvas.SetTop(ellipse, origin.Y - targetRadius);

        var scale = new ScaleTransform(0.0, 0.0);
        ellipse.RenderTransformOrigin = new Point(0.5, 0.5);
        ellipse.RenderTransform = scale;

        _canvas.Children.Add(ellipse);

        var ease = new CubicEase { EasingMode = EasingMode.EaseOut };
        var animDuration = new Duration(Duration);

        var scaleXAnim = new DoubleAnimation(0.0, 1.0, animDuration) { EasingFunction = ease };
        var scaleYAnim = new DoubleAnimation(0.0, 1.0, animDuration) { EasingFunction = ease };
        var opacityAnim = new DoubleAnimation(MaxOpacity, 0.0, animDuration) { EasingFunction = ease };

        opacityAnim.Completed += (s, ev) =>
        {
            canvas.Children.Remove(ellipse);
        };

        scale.BeginAnimation(ScaleTransform.ScaleXProperty, scaleXAnim);
        scale.BeginAnimation(ScaleTransform.ScaleYProperty, scaleYAnim);
        ellipse.BeginAnimation(UIElement.OpacityProperty, opacityAnim);
    }

    private Brush ResolveDefaultBrush()
    {
        if (TryFindResource("CONCOST.Brush.Primary") is Brush primary)
        {
            return primary;
        }
        return new SolidColorBrush(Color.FromArgb(100, 255, 255, 255));
    }
}

/// <summary>
/// Attached properties to easily add Ripple wave animations to any existing WPF control (e.g. Button, Border, Card).
/// </summary>
public static class RippleElement
{
    public static readonly DependencyProperty IsEnabledProperty =
        DependencyProperty.RegisterAttached("IsEnabled", typeof(bool), typeof(RippleElement),
            new FrameworkPropertyMetadata(false, OnIsEnabledChanged));

    public static bool GetIsEnabled(DependencyObject obj) => (bool)obj.GetValue(IsEnabledProperty);
    public static void SetIsEnabled(DependencyObject obj, bool value) => obj.SetValue(IsEnabledProperty, value);

    public static readonly DependencyProperty BrushProperty =
        DependencyProperty.RegisterAttached("Brush", typeof(Brush), typeof(RippleElement),
            new FrameworkPropertyMetadata(null));

    public static Brush? GetBrush(DependencyObject obj) => (Brush?)obj.GetValue(BrushProperty);
    public static void SetBrush(DependencyObject obj, Brush? value) => obj.SetValue(BrushProperty, value);

    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.RegisterAttached("CornerRadius", typeof(CornerRadius), typeof(RippleElement),
            new FrameworkPropertyMetadata(new CornerRadius(0)));

    public static CornerRadius GetCornerRadius(DependencyObject obj) => (CornerRadius)obj.GetValue(CornerRadiusProperty);
    public static void SetCornerRadius(DependencyObject obj, CornerRadius value) => obj.SetValue(CornerRadiusProperty, value);

    public static readonly DependencyProperty ModeProperty =
        DependencyProperty.RegisterAttached("Mode", typeof(RippleMode), typeof(RippleElement),
            new FrameworkPropertyMetadata(RippleMode.Target));
    public static RippleMode GetMode(DependencyObject obj) => (RippleMode)obj.GetValue(ModeProperty);
    public static void SetMode(DependencyObject obj, RippleMode value) => obj.SetValue(ModeProperty, value);

    public static readonly DependencyProperty MaxOpacityProperty =
        DependencyProperty.RegisterAttached("MaxOpacity", typeof(double), typeof(RippleElement),
            new FrameworkPropertyMetadata(0.35), value => value is double opacity &&
                NumericCompatibility.IsFinite(opacity) && opacity >= 0 && opacity <= 1);
    public static double GetMaxOpacity(DependencyObject obj) => (double)obj.GetValue(MaxOpacityProperty);
    public static void SetMaxOpacity(DependencyObject obj, double value) => obj.SetValue(MaxOpacityProperty, value);

    public static readonly DependencyProperty DurationProperty =
        DependencyProperty.RegisterAttached("Duration", typeof(TimeSpan), typeof(RippleElement),
            new FrameworkPropertyMetadata(TimeSpan.FromMilliseconds(420)), value => value is TimeSpan duration &&
                duration > TimeSpan.Zero && duration <= TimeSpan.FromSeconds(10));
    public static TimeSpan GetDuration(DependencyObject obj) => (TimeSpan)obj.GetValue(DurationProperty);
    public static void SetDuration(DependencyObject obj, TimeSpan value) => obj.SetValue(DurationProperty, value);

    public static readonly DependencyProperty MaxConcurrentRipplesProperty =
        DependencyProperty.RegisterAttached("MaxConcurrentRipples", typeof(int), typeof(RippleElement),
            new FrameworkPropertyMetadata(4), value => value is int count && count is >= 1 and <= 32);
    public static int GetMaxConcurrentRipples(DependencyObject obj) => (int)obj.GetValue(MaxConcurrentRipplesProperty);
    public static void SetMaxConcurrentRipples(DependencyObject obj, int value) => obj.SetValue(MaxConcurrentRipplesProperty, value);

    private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not UIElement element)
            return;

        if ((bool)e.NewValue)
        {
            element.PreviewMouseLeftButtonDown += Element_PreviewMouseLeftButtonDown;
        }
        else
        {
            element.PreviewMouseLeftButtonDown -= Element_PreviewMouseLeftButtonDown;
        }
    }

    private static void Element_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is not FrameworkElement fe || fe.ActualWidth <= 0 || fe.ActualHeight <= 0)
            return;

        var layer = AdornerLayer.GetAdornerLayer(fe);
        if (layer == null)
            return;

        var brush = GetBrush(fe);
        var cornerRadius = GetCornerRadius(fe);
        var mode = GetMode(fe);
        var clickPoint = mode == RippleMode.Center
            ? new Point(fe.ActualWidth / 2, fe.ActualHeight / 2)
            : e.GetPosition(fe);
        var maxOpacity = GetMaxOpacity(fe);
        var duration = GetDuration(fe);
        int maxRipples = GetMaxConcurrentRipples(fe);

        var activeRipples = layer.GetAdorners(fe)?.OfType<RippleAdorner>().ToList() ?? new();
        int removeCount = Math.Max(0, activeRipples.Count - maxRipples + 1);
        foreach (var oldRipple in activeRipples.Take(removeCount))
            layer.Remove(oldRipple);

        var adorner = new RippleAdorner(fe, clickPoint, brush, cornerRadius, maxOpacity, duration);
        layer.Add(adorner);
    }
}

/// <summary>
/// Adorner used by RippleElement to render a lightweight expanding ripple over attached elements.
/// </summary>
internal sealed class RippleAdorner : Adorner
{
    private readonly Point _origin;
    private readonly Brush _brush;
    private readonly CornerRadius _cornerRadius;
    private readonly double _maxOpacity;
    private Geometry? _clipGeometry;
    private Size _clipSize;

    public RippleAdorner(UIElement adornedElement, Point origin, Brush? brush, CornerRadius cornerRadius,
        double maxOpacity, TimeSpan duration)
        : base(adornedElement)
    {
        _origin = origin;
        _cornerRadius = cornerRadius;
        _maxOpacity = maxOpacity;
        IsHitTestVisible = false;

        _brush = brush ?? ((adornedElement as FrameworkElement)?.TryFindResource("CONCOST.Brush.Primary") as Brush)
                       ?? (Application.Current?.TryFindResource("CONCOST.Brush.Primary") as Brush)
                       ?? new SolidColorBrush(Color.FromArgb(100, 255, 121, 0));

        var progressAnim = new DoubleAnimation(0.0, 1.0, new Duration(duration))
        {
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };
        progressAnim.Completed += (s, e) =>
        {
            var layer = AdornerLayer.GetAdornerLayer(AdornedElement);
            layer?.Remove(this);
        };

        BeginAnimation(ProgressProperty, progressAnim, HandoffBehavior.SnapshotAndReplace);
    }

    public static readonly DependencyProperty ProgressProperty =
        DependencyProperty.Register(nameof(Progress), typeof(double), typeof(RippleAdorner),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender));

    private double Progress => (double)GetValue(ProgressProperty);

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);

        if (AdornedElement is not FrameworkElement fe || fe.ActualWidth <= 0 || fe.ActualHeight <= 0)
            return;

        double w = fe.ActualWidth;
        double h = fe.ActualHeight;

        // Clip to rounded border if corner radius is present
        bool hasClip = _cornerRadius.TopLeft > 0 || _cornerRadius.TopRight > 0 ||
            _cornerRadius.BottomRight > 0 || _cornerRadius.BottomLeft > 0;
        if (hasClip)
        {
            var clipSize = new Size(w, h);
            if (_clipGeometry == null || _clipSize != clipSize)
            {
                _clipSize = clipSize;
                _clipGeometry = RippleGeometry.CreateRoundedRectangle(clipSize, _cornerRadius);
            }
            dc.PushClip(_clipGeometry);
        }

        double maxDistX = Math.Max(_origin.X, w - _origin.X);
        double maxDistY = Math.Max(_origin.Y, h - _origin.Y);
        double maxRadius = Math.Sqrt(maxDistX * maxDistX + maxDistY * maxDistY) * 1.15;

        double currentRadius = maxRadius * Progress;

        dc.PushOpacity(_maxOpacity * (1 - Progress));
        dc.DrawEllipse(_brush, null, _origin, currentRadius, currentRadius);
        dc.Pop();

        if (hasClip) dc.Pop();
    }
}

internal static class RippleGeometry
{
    public static Geometry CreateRoundedRectangle(Size size, CornerRadius cornerRadius)
    {
        double width = Math.Max(0, size.Width);
        double height = Math.Max(0, size.Height);
        double tl = Math.Max(0, cornerRadius.TopLeft);
        double tr = Math.Max(0, cornerRadius.TopRight);
        double br = Math.Max(0, cornerRadius.BottomRight);
        double bl = Math.Max(0, cornerRadius.BottomLeft);

        double scale = 1;
        scale = Math.Min(scale, Limit(width, tl + tr));
        scale = Math.Min(scale, Limit(width, bl + br));
        scale = Math.Min(scale, Limit(height, tl + bl));
        scale = Math.Min(scale, Limit(height, tr + br));
        tl *= scale; tr *= scale; br *= scale; bl *= scale;

        var geometry = new StreamGeometry();
        using (var context = geometry.Open())
        {
            context.BeginFigure(new Point(tl, 0), true, true);
            context.LineTo(new Point(width - tr, 0), true, false);
            AddCorner(context, new Point(width, tr), tr);
            context.LineTo(new Point(width, height - br), true, false);
            AddCorner(context, new Point(width - br, height), br);
            context.LineTo(new Point(bl, height), true, false);
            AddCorner(context, new Point(0, height - bl), bl);
            context.LineTo(new Point(0, tl), true, false);
            AddCorner(context, new Point(tl, 0), tl);
        }
        if (geometry.CanFreeze) geometry.Freeze();
        return geometry;
    }

    private static double Limit(double extent, double sum) => sum <= 0 ? 1 : Math.Min(1, extent / sum);

    private static void AddCorner(StreamGeometryContext context, Point end, double radius)
    {
        if (radius <= 0)
            context.LineTo(end, true, false);
        else
            context.ArcTo(end, new Size(radius, radius), 0, false, SweepDirection.Clockwise, true, false);
    }
}
