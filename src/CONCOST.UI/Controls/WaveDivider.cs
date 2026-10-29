using System;
using System.Windows;
using System.Windows.Media;

namespace CONCOST.UI.Controls;

/// <summary>
/// Animated wave band used as a section divider or as the bottom of a header block.
/// Two sine layers drift at different speeds to give a soft parallax feel.
/// </summary>
public class WaveDivider : FrameworkElement
{
    private double _elapsed;
    private TimeSpan _last;
    private bool _hooked;

    public static readonly DependencyProperty AmplitudeProperty =
        DependencyProperty.Register(nameof(Amplitude), typeof(double), typeof(WaveDivider),
            new FrameworkPropertyMetadata(12.0, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty WavelengthProperty =
        DependencyProperty.Register(nameof(Wavelength), typeof(double), typeof(WaveDivider),
            new FrameworkPropertyMetadata(180.0, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty SpeedProperty =
        DependencyProperty.Register(nameof(Speed), typeof(double), typeof(WaveDivider),
            new FrameworkPropertyMetadata(24.0, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty FillBrushProperty =
        DependencyProperty.Register(nameof(FillBrush), typeof(Brush), typeof(WaveDivider),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty BackBrushProperty =
        DependencyProperty.Register(nameof(BackBrush), typeof(Brush), typeof(WaveDivider),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty ShowBackWaveProperty =
        DependencyProperty.Register(nameof(ShowBackWave), typeof(bool), typeof(WaveDivider),
            new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty IsAnimatedProperty =
        DependencyProperty.Register(nameof(IsAnimated), typeof(bool), typeof(WaveDivider),
            new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender));

    public double Amplitude
    {
        get => (double)GetValue(AmplitudeProperty);
        set => SetValue(AmplitudeProperty, value);
    }

    public double Wavelength
    {
        get => (double)GetValue(WavelengthProperty);
        set => SetValue(WavelengthProperty, value);
    }

    /// <summary>Horizontal drift in pixels per second.</summary>
    public double Speed
    {
        get => (double)GetValue(SpeedProperty);
        set => SetValue(SpeedProperty, value);
    }

    public Brush? FillBrush
    {
        get => (Brush?)GetValue(FillBrushProperty);
        set => SetValue(FillBrushProperty, value);
    }

    public Brush? BackBrush
    {
        get => (Brush?)GetValue(BackBrushProperty);
        set => SetValue(BackBrushProperty, value);
    }

    public bool ShowBackWave
    {
        get => (bool)GetValue(ShowBackWaveProperty);
        set => SetValue(ShowBackWaveProperty, value);
    }

    public bool IsAnimated
    {
        get => (bool)GetValue(IsAnimatedProperty);
        set => SetValue(IsAnimatedProperty, value);
    }

    public WaveDivider()
    {
        ClipToBounds = true;
        IsHitTestVisible = false;
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        double width = double.IsInfinity(availableSize.Width) ? 320 : availableSize.Width;
        double height = double.IsInfinity(availableSize.Height) ? 60 : availableSize.Height;
        return new Size(width, height);
    }

    protected override void OnRender(DrawingContext dc)
    {
        double width = ActualWidth;
        double height = ActualHeight;
        if (width <= 1 || height <= 1) return;

        Brush front = FillBrush ?? TryFindResource("CONCOST.Brush.Primary") as Brush ?? Brushes.SteelBlue;
        Brush back = BackBrush ?? TryFindResource("CONCOST.Brush.SurfaceSecondary") as Brush ?? Brushes.LightSteelBlue;

        if (ShowBackWave)
            dc.DrawGeometry(back, null, BuildWave(width, height, _elapsed * 0.6, 0.55, 0.35));

        dc.DrawGeometry(front, null, BuildWave(width, height, _elapsed, 1.0, 0.0));
    }

    private Geometry BuildWave(double width, double height, double phasePixels, double amplitudeFactor, double offsetFactor)
    {
        double amplitude = Math.Max(0, Amplitude) * amplitudeFactor;
        double wavelength = Math.Max(20, Wavelength);
        double baseline = height * (0.45 + offsetFactor);
        double step = 4;

        var geometry = new StreamGeometry();
        using (StreamGeometryContext ctx = geometry.Open())
        {
            ctx.BeginFigure(new Point(0, height), true, true);

            for (double x = 0; x <= width; x += step)
            {
                double angle = (x + phasePixels) / wavelength * Math.PI * 2;
                ctx.LineTo(new Point(x, baseline + Math.Sin(angle) * amplitude), true, false);
            }

            ctx.LineTo(new Point(width, baseline + Math.Sin((width + phasePixels) / wavelength * Math.PI * 2) * amplitude), true, false);
            ctx.LineTo(new Point(width, height), false, false);
        }

        geometry.Freeze();
        return geometry;
    }

    private void OnFrame(object? sender, EventArgs e)
    {
        if (!IsAnimated || ActualWidth <= 0) { _last = TimeSpan.Zero; return; }

        TimeSpan now = TimeSpan.FromTicks(DateTime.UtcNow.Ticks);
        if (_last == TimeSpan.Zero) { _last = now; return; }

        double delta = (now - _last).TotalSeconds;
        _last = now;
        if (delta <= 0 || delta > 0.5) delta = 0.016;

        _elapsed += Speed * delta;
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
