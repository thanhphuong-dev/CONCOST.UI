using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using CONCOST.UI.Core;

namespace CONCOST.UI.Controls;

/// <summary>
/// Circular gauge for KPI readouts: a track arc, a value arc that grows clockwise from
/// <see cref="StartAngle"/>, optional tick marks and the value printed in the middle.
/// </summary>
/// <remarks>
/// Drawn in <see cref="OnRender"/> rather than with a template, so the geometry stays crisp at
/// any size and the arc can be animated by a single <see cref="double"/> instead of a storyboard
/// per segment. Value changes ease over <see cref="AnimationDuration"/>; set
/// <see cref="IsAnimated"/> to false (or turn motion off globally) to jump straight to the value.
/// </remarks>
public class RadialGauge : FrameworkElement
{
    public static readonly DependencyProperty MinimumProperty =
        DependencyProperty.Register(nameof(Minimum), typeof(double), typeof(RadialGauge),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender, OnRangeChanged));

    public static readonly DependencyProperty MaximumProperty =
        DependencyProperty.Register(nameof(Maximum), typeof(double), typeof(RadialGauge),
            new FrameworkPropertyMetadata(100.0, FrameworkPropertyMetadataOptions.AffectsRender, OnRangeChanged));

    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(nameof(Value), typeof(double), typeof(RadialGauge),
            new FrameworkPropertyMetadata(0.0, OnValueChanged));

    public static readonly DependencyProperty StartAngleProperty =
        DependencyProperty.Register(nameof(StartAngle), typeof(double), typeof(RadialGauge),
            new FrameworkPropertyMetadata(135.0, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty SweepAngleProperty =
        DependencyProperty.Register(nameof(SweepAngle), typeof(double), typeof(RadialGauge),
            new FrameworkPropertyMetadata(270.0, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty TrackThicknessProperty =
        DependencyProperty.Register(nameof(TrackThickness), typeof(double), typeof(RadialGauge),
            new FrameworkPropertyMetadata(12.0, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty TrackBrushProperty =
        DependencyProperty.Register(nameof(TrackBrush), typeof(Brush), typeof(RadialGauge),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty ValueBrushProperty =
        DependencyProperty.Register(nameof(ValueBrush), typeof(Brush), typeof(RadialGauge),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty ShowTicksProperty =
        DependencyProperty.Register(nameof(ShowTicks), typeof(bool), typeof(RadialGauge),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty TickCountProperty =
        DependencyProperty.Register(nameof(TickCount), typeof(int), typeof(RadialGauge),
            new FrameworkPropertyMetadata(10, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty ShowValueProperty =
        DependencyProperty.Register(nameof(ShowValue), typeof(bool), typeof(RadialGauge),
            new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty ValueFormatProperty =
        DependencyProperty.Register(nameof(ValueFormat), typeof(string), typeof(RadialGauge),
            new FrameworkPropertyMetadata("0", FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty SuffixProperty =
        DependencyProperty.Register(nameof(Suffix), typeof(string), typeof(RadialGauge),
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty LabelProperty =
        DependencyProperty.Register(nameof(Label), typeof(string), typeof(RadialGauge),
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty ValueFontSizeProperty =
        DependencyProperty.Register(nameof(ValueFontSize), typeof(double), typeof(RadialGauge),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty IsAnimatedProperty =
        DependencyProperty.Register(nameof(IsAnimated), typeof(bool), typeof(RadialGauge),
            new FrameworkPropertyMetadata(true));

    public static readonly DependencyProperty AnimationDurationProperty =
        DependencyProperty.Register(nameof(AnimationDuration), typeof(TimeSpan), typeof(RadialGauge),
            new FrameworkPropertyMetadata(TimeSpan.FromMilliseconds(520)));

    private static readonly DependencyProperty AnimatedValueProperty =
        DependencyProperty.Register("AnimatedValue", typeof(double), typeof(RadialGauge),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender));

    private static readonly Typeface ValueTypeface = new(new FontFamily("Segoe UI"), FontStyles.Normal, FontWeights.SemiBold, FontStretches.Normal);
    private static readonly Typeface LabelTypeface = new(new FontFamily("Segoe UI"), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);

    /// <summary>Lowest value the arc represents.</summary>
    public double Minimum
    {
        get => (double)GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    /// <summary>Highest value the arc represents.</summary>
    public double Maximum
    {
        get => (double)GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    /// <summary>Current value, clamped to <see cref="Minimum"/>–<see cref="Maximum"/>.</summary>
    public double Value
    {
        get => (double)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    /// <summary>Where the arc starts, in degrees clockwise from 3 o'clock. 135 puts the gap at the bottom.</summary>
    public double StartAngle
    {
        get => (double)GetValue(StartAngleProperty);
        set => SetValue(StartAngleProperty, value);
    }

    /// <summary>How far the arc travels from <see cref="StartAngle"/>, in degrees.</summary>
    public double SweepAngle
    {
        get => (double)GetValue(SweepAngleProperty);
        set => SetValue(SweepAngleProperty, value);
    }

    /// <summary>Thickness of the track and value arcs, in device-independent pixels.</summary>
    public double TrackThickness
    {
        get => (double)GetValue(TrackThicknessProperty);
        set => SetValue(TrackThicknessProperty, value);
    }

    /// <summary>Colour of the unfilled track. Defaults to <c>CONCOST.Brush.SurfaceSecondary</c>.</summary>
    public Brush? TrackBrush
    {
        get => (Brush?)GetValue(TrackBrushProperty);
        set => SetValue(TrackBrushProperty, value);
    }

    /// <summary>Colour of the filled arc. Defaults to <c>CONCOST.Brush.Primary</c>.</summary>
    public Brush? ValueBrush
    {
        get => (Brush?)GetValue(ValueBrushProperty);
        set => SetValue(ValueBrushProperty, value);
    }

    /// <summary>Draws <see cref="TickCount"/> ticks around the outside of the arc.</summary>
    public bool ShowTicks
    {
        get => (bool)GetValue(ShowTicksProperty);
        set => SetValue(ShowTicksProperty, value);
    }

    /// <summary>Number of tick marks, counted from the start of the arc to the end.</summary>
    public int TickCount
    {
        get => (int)GetValue(TickCountProperty);
        set => SetValue(TickCountProperty, value);
    }

    /// <summary>Prints the value in the middle of the gauge.</summary>
    public bool ShowValue
    {
        get => (bool)GetValue(ShowValueProperty);
        set => SetValue(ShowValueProperty, value);
    }

    /// <summary>Numeric format applied to <see cref="Value"/> before <see cref="Suffix"/> is appended.</summary>
    public string ValueFormat
    {
        get => (string)GetValue(ValueFormatProperty);
        set => SetValue(ValueFormatProperty, value);
    }

    /// <summary>Unit printed straight after the number, for example <c>%</c> or <c>kN</c>.</summary>
    public string Suffix
    {
        get => (string)GetValue(SuffixProperty);
        set => SetValue(SuffixProperty, value);
    }

    /// <summary>Caption printed under the value.</summary>
    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    /// <summary>Size of the value text. Leave 0 to scale with the gauge.</summary>
    public double ValueFontSize
    {
        get => (double)GetValue(ValueFontSizeProperty);
        set => SetValue(ValueFontSizeProperty, value);
    }

    /// <summary>Eases value changes instead of snapping to them.</summary>
    public bool IsAnimated
    {
        get => (bool)GetValue(IsAnimatedProperty);
        set => SetValue(IsAnimatedProperty, value);
    }

    /// <summary>How long the ease takes. Defaults to 520 ms.</summary>
    public TimeSpan AnimationDuration
    {
        get => (TimeSpan)GetValue(AnimationDurationProperty);
        set => SetValue(AnimationDurationProperty, value);
    }

    private double AnimatedValue
    {
        get => (double)GetValue(AnimatedValueProperty);
        set => SetValue(AnimatedValueProperty, value);
    }

    /// <summary>Fraction of the arc the current value fills, 0–1.</summary>
    public double NormalizedValue
    {
        get
        {
            double range = Maximum - Minimum;
            if (range <= 0 || double.IsNaN(range) || double.IsInfinity(range)) return 0;
            double fraction = (Clamp(Value) - Minimum) / range;
            return fraction < 0 ? 0 : fraction > 1 ? 1 : fraction;
        }
    }

    public RadialGauge()
    {
        Loaded += (_, _) => SetCurrentValue(AnimatedValueProperty, Clamp(Value));
    }

    private double Clamp(double value)
    {
        if (double.IsNaN(value)) return Minimum;
        double low = Math.Min(Minimum, Maximum);
        double high = Math.Max(Minimum, Maximum);
        return value < low ? low : value > high ? high : value;
    }

    private static void OnRangeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is RadialGauge gauge) gauge.SetCurrentValue(AnimatedValueProperty, gauge.Clamp(gauge.Value));
    }

    private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not RadialGauge gauge) return;

        double target = gauge.Clamp((double)e.NewValue);

        if (!gauge.IsAnimated || !MotionSettings.AnimationsEnabled || !gauge.IsLoaded)
        {
            gauge.BeginAnimation(AnimatedValueProperty, null);
            gauge.SetCurrentValue(AnimatedValueProperty, target);
            return;
        }

        var animation = new DoubleAnimation(target, gauge.AnimationDuration)
        {
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut },
            FillBehavior = FillBehavior.HoldEnd
        };

        gauge.BeginAnimation(AnimatedValueProperty, animation);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        double width = double.IsInfinity(availableSize.Width) ? 140 : availableSize.Width;
        double height = double.IsInfinity(availableSize.Height) ? 140 : availableSize.Height;
        return new Size(width, height);
    }

    protected override void OnRender(DrawingContext dc)
    {
        double width = ActualWidth;
        double height = ActualHeight;
        if (width <= 4 || height <= 4) return;

        double thickness = Math.Max(1, TrackThickness);
        double tickGutter = ShowTicks ? 9.0 : 0.0;
        double radius = Math.Min(width, height) / 2.0 - thickness / 2.0 - tickGutter - 2.0;
        if (radius <= 1) return;

        var center = new Point(width / 2.0, height / 2.0);
        Brush trackBrush = TrackBrush ?? TryFindResource("CONCOST.Brush.SurfaceSecondary") as Brush ?? Brushes.Gainsboro;
        Brush valueBrush = ValueBrush ?? TryFindResource("CONCOST.Brush.Primary") as Brush ?? Brushes.DodgerBlue;

        var trackPen = new Pen(trackBrush, thickness)
        {
            StartLineCap = PenLineCap.Round,
            EndLineCap = PenLineCap.Round
        };
        trackPen.Freeze();

        double start = StartAngle;
        double sweep = SweepAngle;

        if (Math.Abs(sweep) >= 359.99)
        {
            dc.DrawEllipse(null, trackPen, center, radius, radius);
        }
        else
        {
            dc.DrawGeometry(null, trackPen, BuildArc(center, radius, start, sweep));
        }

        double range = Maximum - Minimum;
        double fraction = range <= 0 ? 0 : (Clamp(AnimatedValue) - Minimum) / range;
        fraction = fraction < 0 ? 0 : fraction > 1 ? 1 : fraction;

        if (fraction > 0.0005)
        {
            var valuePen = new Pen(valueBrush, thickness)
            {
                StartLineCap = PenLineCap.Round,
                EndLineCap = PenLineCap.Round
            };
            valuePen.Freeze();

            double filled = sweep * fraction;

            if (Math.Abs(filled) >= 359.99)
            {
                dc.DrawEllipse(null, valuePen, center, radius, radius);
            }
            else
            {
                dc.DrawGeometry(null, valuePen, BuildArc(center, radius, start, filled));
            }
        }

        if (ShowTicks && TickCount > 0)
        {
            Brush tickBrush = TryFindResource("CONCOST.Brush.TextTertiary") as Brush ?? Brushes.Gray;
            DrawTicks(dc, center, radius, thickness, start, sweep, tickBrush);
        }

        if (ShowValue)
        {
            DrawReadout(dc, center, radius);
        }
    }

    private void DrawTicks(DrawingContext dc, Point center, double radius, double thickness, double start, double sweep, Brush brush)
    {
        var tickPen = new Pen(brush, 1.4) { StartLineCap = PenLineCap.Round, EndLineCap = PenLineCap.Round };
        tickPen.Freeze();

        double inner = radius + thickness / 2.0 + 3.0;
        double outer = inner + 4.0;

        for (int i = 0; i <= TickCount; i++)
        {
            double angle = start + sweep * i / TickCount;
            dc.DrawLine(tickPen, PointOnCircle(center, inner, angle), PointOnCircle(center, outer, angle));
        }
    }

    private void DrawReadout(DrawingContext dc, Point center, double radius)
    {
        string text = string.Concat(
            Clamp(AnimatedValue).ToString(ValueFormat, CultureInfo.CurrentCulture),
            Suffix);

        double fontSize = ValueFontSize > 0 ? ValueFontSize : Math.Max(11, radius * 0.46);
        double pixelsPerDip = VisualTreeHelper.GetDpi(this).PixelsPerDip;

        Brush ink = TryFindResource("CONCOST.Brush.TextPrimary") as Brush ?? Brushes.Black;
        Brush muted = TryFindResource("CONCOST.Brush.TextSecondary") as Brush ?? Brushes.DimGray;

        var valueText = new FormattedText(text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
            ValueTypeface, fontSize, ink, pixelsPerDip);

        string label = Label ?? string.Empty;
        FormattedText? labelText = null;
        if (label.Length > 0)
        {
            labelText = new FormattedText(label, CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                LabelTypeface, Math.Max(9, fontSize * 0.34), muted, pixelsPerDip);
        }

        double blockHeight = valueText.Height + (labelText is null ? 0 : labelText.Height + 2);
        double top = center.Y - blockHeight / 2.0;

        dc.DrawText(valueText, new Point(center.X - valueText.Width / 2.0, top));

        if (labelText is not null)
        {
            dc.DrawText(labelText, new Point(center.X - labelText.Width / 2.0, top + valueText.Height + 2));
        }
    }

    private static Point PointOnCircle(Point center, double radius, double angleDegrees)
    {
        double radians = angleDegrees * Math.PI / 180.0;
        return new Point(center.X + radius * Math.Cos(radians), center.Y + radius * Math.Sin(radians));
    }

    private static StreamGeometry BuildArc(Point center, double radius, double startAngle, double sweepAngle)
    {
        var geometry = new StreamGeometry();
        using (StreamGeometryContext ctx = geometry.Open())
        {
            Point from = PointOnCircle(center, radius, startAngle);
            Point to = PointOnCircle(center, radius, startAngle + sweepAngle);

            ctx.BeginFigure(from, false, false);
            ctx.ArcTo(to, new Size(radius, radius), 0,
                Math.Abs(sweepAngle) > 180,
                sweepAngle >= 0 ? SweepDirection.Clockwise : SweepDirection.Counterclockwise,
                true, false);
        }

        geometry.Freeze();
        return geometry;
    }
}
