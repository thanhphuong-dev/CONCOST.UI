using System;
using System.Windows;
using System.Windows.Media;
using CONCOST.UI.Core;

namespace CONCOST.UI.Controls;

public enum LoadingIndicatorType
{
    ThreeDotsWave,      // 3 dots bouncing in a wave
    CircularDotsRing,   // orbiting spinner ring
    LinePulse,          // smooth horizontal progress line
    PulseCircle         // pulsing radar ripple
}

/// <summary>
/// A single control with four indeterminate loading looks, chosen by
/// <see cref="IndicatorType"/>. It paints itself through
/// <see cref="AnimatedEffectBase"/>, so the Windows "Show animations" setting (or
/// <see cref="MotionSettings.ForceDisabled"/>) settles it on a static first frame
/// instead of burning frames nobody asked for, and <see cref="AnimatedEffectBase.IsAnimated"/>
/// pauses it without hiding it.
/// </summary>
public class LoadingIndicator : AnimatedEffectBase
{
    private double _phase;

    public static readonly DependencyProperty IndicatorTypeProperty =
        DependencyProperty.Register(nameof(IndicatorType), typeof(LoadingIndicatorType), typeof(LoadingIndicator),
            new PropertyMetadata(LoadingIndicatorType.ThreeDotsWave, OnVisualChanged));

    public static readonly DependencyProperty DotSizeProperty =
        DependencyProperty.Register(nameof(DotSize), typeof(double), typeof(LoadingIndicator),
            new PropertyMetadata(10.0, OnVisualChanged));

    public static readonly DependencyProperty ForegroundProperty =
        DependencyProperty.Register(nameof(Foreground), typeof(Brush), typeof(LoadingIndicator),
            new PropertyMetadata(null, OnVisualChanged));

    public LoadingIndicatorType IndicatorType
    {
        get => (LoadingIndicatorType)GetValue(IndicatorTypeProperty);
        set => SetValue(IndicatorTypeProperty, value);
    }

    public double DotSize
    {
        get => (double)GetValue(DotSizeProperty);
        set => SetValue(DotSizeProperty, value);
    }

    /// <summary>Colour of the dots and bars; the theme default is the accent.</summary>
    public Brush? Foreground
    {
        get => (Brush?)GetValue(ForegroundProperty);
        set => SetValue(ForegroundProperty, value);
    }

    /// <summary>How far the current animation loop has run, 0–1 — the phase the glyphs are painted at.</summary>
    public double Phase => _phase;

    private static void OnVisualChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is LoadingIndicator indicator)
        {
            indicator._phase = 0;
            indicator.InvalidateVisual();
        }
    }

    protected override void OnTick(double seconds)
    {
        _phase += seconds;
        if (_phase >= 1.0) _phase %= 1.0;
    }

    protected override void OnMotionStopped()
    {
        // A loader parked by reduced motion rests on its first frame.
        _phase = 0;
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        double dot = Math.Max(2, DotSize);

        return IndicatorType switch
        {
            LoadingIndicatorType.ThreeDotsWave => new Size(dot * 4, dot * 2.4),
            LoadingIndicatorType.CircularDotsRing => new Size(dot * 4.4, dot * 4.4),
            LoadingIndicatorType.LinePulse => new Size(96, Math.Max(6, dot * 0.8)),
            LoadingIndicatorType.PulseCircle => new Size(dot * 4.4, dot * 4.4),
            _ => new Size(dot * 4, dot * 2.4)
        };
    }

    protected override void OnRender(DrawingContext dc)
    {
        var brush = Foreground ?? FindAccentBrush();
        if (brush is null) return;

        var size = RenderSize;
        if (size.Width <= 0 || size.Height <= 0) return;

        double dot = Math.Max(2, DotSize);

        switch (IndicatorType)
        {
            case LoadingIndicatorType.ThreeDotsWave:
                DrawWave(dc, brush, size, dot);
                break;
            case LoadingIndicatorType.CircularDotsRing:
                DrawRing(dc, brush, size, dot);
                break;
            case LoadingIndicatorType.LinePulse:
                DrawLinePulse(dc, brush, size, dot);
                break;
            case LoadingIndicatorType.PulseCircle:
                DrawRipple(dc, brush, size, dot);
                break;
        }
    }

    private void DrawWave(DrawingContext dc, Brush brush, Size size, double dot)
    {
        double gap = dot * 0.5;
        double totalWidth = dot * 3 + gap * 2;
        double startX = (size.Width - totalWidth) / 2 + dot / 2;
        double cy = size.Height / 2;
        double amplitude = dot * 0.45;

        for (int i = 0; i < 3; i++)
        {
            double angle = _phase * 2 * Math.PI - i * 0.55;
            double y = cy + Math.Sin(angle) * amplitude;
            double alpha = 0.4 + 0.6 * (0.5 + 0.5 * Math.Cos(angle));

            dc.PushOpacity(alpha);
            dc.DrawEllipse(brush, null, new Point(startX + i * (dot + gap), y), dot / 2, dot / 2);
            dc.Pop();
        }
    }

    private void DrawRing(DrawingContext dc, Brush brush, Size size, double dot)
    {
        double radius = dot * 1.7;
        var center = new Point(size.Width / 2, size.Height / 2);
        double orbit = _phase * 2 * Math.PI;

        for (int i = 0; i < 8; i++)
        {
            double angle = orbit + i * Math.PI / 4;
            var position = new Point(center.X + Math.Cos(angle) * radius, center.Y + Math.Sin(angle) * radius);
            double dotRadius = Math.Max(1.2, dot * 0.28);

            dc.PushOpacity(0.15 + 0.85 * (i + 1) / 8.0);
            dc.DrawEllipse(brush, null, position, dotRadius, dotRadius);
            dc.Pop();
        }
    }

    private void DrawLinePulse(DrawingContext dc, Brush brush, Size size, double dot)
    {
        double barHeight = Math.Max(3, dot * 0.4);
        double pulse = (1 + Math.Sin(_phase * 2 * Math.PI)) / 2;
        double smooth = pulse * pulse * (3 - 2 * pulse);
        double barWidth = size.Width * (0.15 + 0.85 * smooth);
        double radius = barHeight / 2;

        dc.PushOpacity(0.25);
        dc.DrawRoundedRectangle(CreateDimBrush(brush), null, new Rect(0, size.Height / 2 - radius, size.Width, barHeight), radius, radius);
        dc.Pop();

        dc.DrawRoundedRectangle(brush, null, new Rect((size.Width - barWidth) / 2, size.Height / 2 - radius, barWidth, barHeight), radius, radius);
    }

    private void DrawRipple(DrawingContext dc, Brush brush, Size size, double dot)
    {
        var center = new Point(size.Width / 2, size.Height / 2);
        double baseRadius = dot * 0.7;

        for (int k = 0; k < 2; k++)
        {
            double t = (_phase * 0.6 + k * 0.5) % 1;
            double radius = baseRadius + t * baseRadius * 2.4;

            dc.PushOpacity((1 - t) * 0.8);
            dc.DrawEllipse(null, new Pen(brush, Math.Max(1.5, dot * 0.16)), center, radius, radius);
            dc.Pop();
        }

        dc.DrawEllipse(brush, null, center, dot * 0.18, dot * 0.18);
    }

    private Brush CreateDimBrush(Brush brush)
    {
        if (brush is SolidColorBrush solid)
        {
            var color = Color.FromArgb((byte)(solid.Color.A * 0.4), solid.Color.R, solid.Color.G, solid.Color.B);
            return new SolidColorBrush(color);
        }

        return brush;
    }

    private Brush? FindAccentBrush()
    {
        // Prefer the theme accent, fall back to the primary text colour so the
        // glyph is never invisible on an unmerged dictionary.
        if (TryFindResource("CONCOST.Brush.Primary") is Brush accent) return accent;
        return TryFindResource("CONCOST.Brush.TextPrimary") as Brush;
    }
}
