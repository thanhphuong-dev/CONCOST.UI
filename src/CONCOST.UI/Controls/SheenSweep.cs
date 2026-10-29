using System;
using System.Windows;
using System.Windows.Media;
using CONCOST.UI.Core;

namespace CONCOST.UI.Controls;

/// <summary>
/// Diagonal light sweep across a surface — the “sheen” that runs over buttons and
/// cards on modern product pages. Place it in a Grid above the content.
/// </summary>
public class SheenSweep : FrameworkElement
{
    private double _elapsed;
    private double _phase = -1;   // 0..1 while sweeping, -1 while idle
    private TimeSpan _last;
    private bool _hooked;

    public static readonly DependencyProperty IsActiveProperty =
        DependencyProperty.Register(nameof(IsActive), typeof(bool), typeof(SheenSweep),
            new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty SpeedProperty =
        DependencyProperty.Register(nameof(Speed), typeof(double), typeof(SheenSweep),
            new FrameworkPropertyMetadata(0.55, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty BandWidthProperty =
        DependencyProperty.Register(nameof(BandWidth), typeof(double), typeof(SheenSweep),
            new FrameworkPropertyMetadata(0.22, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty AngleProperty =
        DependencyProperty.Register(nameof(Angle), typeof(double), typeof(SheenSweep),
            new FrameworkPropertyMetadata(24.0, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty PeakOpacityProperty =
        DependencyProperty.Register(nameof(PeakOpacity), typeof(double), typeof(SheenSweep),
            new FrameworkPropertyMetadata(0.35, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty SheenBrushProperty =
        DependencyProperty.Register(nameof(SheenBrush), typeof(Brush), typeof(SheenSweep),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty PauseSecondsProperty =
        DependencyProperty.Register(nameof(PauseSeconds), typeof(double), typeof(SheenSweep),
            new FrameworkPropertyMetadata(1.6, FrameworkPropertyMetadataOptions.AffectsRender));

    public bool IsActive
    {
        get => (bool)GetValue(IsActiveProperty);
        set => SetValue(IsActiveProperty, value);
    }

    /// <summary>Sweeps per second (0.5 = one sweep every two seconds).</summary>
    public double Speed
    {
        get => (double)GetValue(SpeedProperty);
        set => SetValue(SpeedProperty, value);
    }

    /// <summary>Width of the light band as a fraction of the control width.</summary>
    public double BandWidth
    {
        get => (double)GetValue(BandWidthProperty);
        set => SetValue(BandWidthProperty, value);
    }

    public double Angle
    {
        get => (double)GetValue(AngleProperty);
        set => SetValue(AngleProperty, value);
    }

    public double PeakOpacity
    {
        get => (double)GetValue(PeakOpacityProperty);
        set => SetValue(PeakOpacityProperty, value);
    }

    public Brush? SheenBrush
    {
        get => (Brush?)GetValue(SheenBrushProperty);
        set => SetValue(SheenBrushProperty, value);
    }

    /// <summary>Idle time between two sweeps, in seconds.</summary>
    public double PauseSeconds
    {
        get => (double)GetValue(PauseSecondsProperty);
        set => SetValue(PauseSecondsProperty, value);
    }

    public SheenSweep()
    {
        ClipToBounds = true;
        IsHitTestVisible = false;
    }

    protected override void OnRender(DrawingContext dc)
    {
        double width = ActualWidth;
        double height = ActualHeight;
        if (width <= 1 || height <= 1 || _phase < 0) return;

        Color color = SheenBrush is SolidColorBrush solid ? solid.Color : Colors.White;
        byte peak = (byte)Math.Round(255 * Math.Max(0, Math.Min(1, PeakOpacity)));

        double band = Math.Max(0.03, Math.Min(0.9, BandWidth)) * width;
        double travel = width + band * 2;
        double centerX = -band + travel * _phase;

        var brush = new LinearGradientBrush
        {
            StartPoint = new Point(0, 0.5),
            EndPoint = new Point(1, 0.5)
        };
        brush.GradientStops.Add(new GradientStop(Color.FromArgb(0, color.R, color.G, color.B), 0));
        brush.GradientStops.Add(new GradientStop(Color.FromArgb((byte)(peak / 3), color.R, color.G, color.B), 0.34));
        brush.GradientStops.Add(new GradientStop(Color.FromArgb(peak, color.R, color.G, color.B), 0.5));
        brush.GradientStops.Add(new GradientStop(Color.FromArgb((byte)(peak / 3), color.R, color.G, color.B), 0.66));
        brush.GradientStops.Add(new GradientStop(Color.FromArgb(0, color.R, color.G, color.B), 1));
        brush.Freeze();

        dc.PushTransform(new RotateTransform(Angle, centerX, height / 2));
        dc.DrawRectangle(brush, null, new Rect(centerX - band / 2, -height, band, height * 3));
        dc.Pop();
    }

    private void OnFrame(object? sender, EventArgs e)
    {
        if (!IsActive || ActualWidth <= 0) { _last = TimeSpan.Zero; return; }

        TimeSpan now = TimeSpan.FromTicks(DateTime.UtcNow.Ticks);
        if (_last == TimeSpan.Zero) { _last = now; return; }

        double delta = (now - _last).TotalSeconds;
        _last = now;
        if (delta <= 0 || delta > 0.5) delta = 0.016;

        double speed = Speed <= 0.02 ? 0.02 : Speed;
        double sweepSeconds = 1.0 / speed;
        double cycle = sweepSeconds + Math.Max(0, PauseSeconds);

        _elapsed = (_elapsed + delta) % cycle;

        double phase = _elapsed <= sweepSeconds ? _elapsed / sweepSeconds : -1;
        if (Math.Abs(phase - _phase) < 0.0005) return;

        _phase = phase;
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
