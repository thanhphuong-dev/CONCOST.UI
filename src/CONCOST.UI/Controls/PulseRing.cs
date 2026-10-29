using System;
using System.Windows;
using System.Windows.Media;

namespace CONCOST.UI.Controls;

/// <summary>
/// Expanding, fading rings used to draw attention to a live state or a selected point
/// (the “pulse” marker from modern dashboards). Renders from the centre of its box.
/// </summary>
public class PulseRing : FrameworkElement
{
    private double _elapsed;
    private TimeSpan _last;
    private bool _hooked;

    public static readonly DependencyProperty RingCountProperty =
        DependencyProperty.Register(nameof(RingCount), typeof(int), typeof(PulseRing),
            new FrameworkPropertyMetadata(3, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty DurationProperty =
        DependencyProperty.Register(nameof(Duration), typeof(double), typeof(PulseRing),
            new FrameworkPropertyMetadata(2.2, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty MaxRadiusProperty =
        DependencyProperty.Register(nameof(MaxRadius), typeof(double), typeof(PulseRing),
            new FrameworkPropertyMetadata(64.0, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty RingThicknessProperty =
        DependencyProperty.Register(nameof(RingThickness), typeof(double), typeof(PulseRing),
            new FrameworkPropertyMetadata(2.0, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty RingBrushProperty =
        DependencyProperty.Register(nameof(RingBrush), typeof(Brush), typeof(PulseRing),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty PeakOpacityProperty =
        DependencyProperty.Register(nameof(PeakOpacity), typeof(double), typeof(PulseRing),
            new FrameworkPropertyMetadata(0.7, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty IsActiveProperty =
        DependencyProperty.Register(nameof(IsActive), typeof(bool), typeof(PulseRing),
            new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty CenterDotProperty =
        DependencyProperty.Register(nameof(CenterDot), typeof(bool), typeof(PulseRing),
            new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty DotRadiusProperty =
        DependencyProperty.Register(nameof(DotRadius), typeof(double), typeof(PulseRing),
            new FrameworkPropertyMetadata(5.0, FrameworkPropertyMetadataOptions.AffectsRender));

    public int RingCount
    {
        get => (int)GetValue(RingCountProperty);
        set => SetValue(RingCountProperty, value);
    }

    /// <summary>Seconds for one ring to travel from the centre to the outer edge.</summary>
    public double Duration
    {
        get => (double)GetValue(DurationProperty);
        set => SetValue(DurationProperty, value);
    }

    public double MaxRadius
    {
        get => (double)GetValue(MaxRadiusProperty);
        set => SetValue(MaxRadiusProperty, value);
    }

    public double RingThickness
    {
        get => (double)GetValue(RingThicknessProperty);
        set => SetValue(RingThicknessProperty, value);
    }

    public Brush? RingBrush
    {
        get => (Brush?)GetValue(RingBrushProperty);
        set => SetValue(RingBrushProperty, value);
    }

    public double PeakOpacity
    {
        get => (double)GetValue(PeakOpacityProperty);
        set => SetValue(PeakOpacityProperty, value);
    }

    public bool IsActive
    {
        get => (bool)GetValue(IsActiveProperty);
        set => SetValue(IsActiveProperty, value);
    }

    public bool CenterDot
    {
        get => (bool)GetValue(CenterDotProperty);
        set => SetValue(CenterDotProperty, value);
    }

    public double DotRadius
    {
        get => (double)GetValue(DotRadiusProperty);
        set => SetValue(DotRadiusProperty, value);
    }

    public PulseRing()
    {
        IsHitTestVisible = false;
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        double size = MaxRadius * 2;
        double width = double.IsInfinity(availableSize.Width) ? size : Math.Min(availableSize.Width, size);
        double height = double.IsInfinity(availableSize.Height) ? size : Math.Min(availableSize.Height, size);
        return new Size(width, height);
    }

    protected override void OnRender(DrawingContext dc)
    {
        double width = ActualWidth;
        double height = ActualHeight;
        if (width <= 1 || height <= 1) return;

        Color color = RingBrush is SolidColorBrush solid
            ? solid.Color
            : (TryFindResource("CONCOST.Brush.Primary") as SolidColorBrush)?.Color ?? Colors.DodgerBlue;

        var center = new Point(width / 2, height / 2);
        double limit = Math.Min(MaxRadius, Math.Min(width, height) / 2 - RingThickness);
        if (limit <= 0) return;

        int count = Math.Max(1, Math.Min(6, RingCount));
        double duration = Duration <= 0.2 ? 0.2 : Duration;
        double basePhase = (_elapsed % duration) / duration;
        double peak = Math.Max(0, Math.Min(1, PeakOpacity));

        for (int i = 0; i < count; i++)
        {
            double phase = (basePhase + (double)i / count) % 1.0;
            double radius = limit * phase;
            if (radius <= 0.5) continue;

            byte alpha = (byte)Math.Round(255 * peak * Math.Pow(1 - phase, 1.15));
            if (alpha == 0) continue;

            var brush = new SolidColorBrush(Color.FromArgb(alpha, color.R, color.G, color.B));
            brush.Freeze();
            var pen = new Pen(brush, RingThickness);
            pen.Freeze();

            dc.DrawEllipse(null, pen, center, radius, radius);
        }

        if (CenterDot)
        {
            var dot = new SolidColorBrush(Color.FromArgb(235, color.R, color.G, color.B));
            dot.Freeze();
            dc.DrawEllipse(dot, null, center, DotRadius, DotRadius);
        }
    }

    private void OnFrame(object? sender, EventArgs e)
    {
        if (!IsActive || ActualWidth <= 0) { _last = TimeSpan.Zero; return; }

        TimeSpan now = TimeSpan.FromTicks(DateTime.UtcNow.Ticks);
        if (_last == TimeSpan.Zero) { _last = now; return; }

        double delta = (now - _last).TotalSeconds;
        _last = now;
        if (delta <= 0 || delta > 0.5) delta = 0.016;

        _elapsed += delta;
        InvalidateVisual();
    }

    private void Hook()
    {
        if (_hooked) return;
        CompositionTarget.Rendering += OnFrame;
        _hooked = true;
    }

    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
        base.OnRenderSizeChanged(sizeInfo);
        Hook();
    }
}
