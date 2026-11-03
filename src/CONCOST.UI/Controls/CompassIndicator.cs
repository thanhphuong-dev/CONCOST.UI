using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CONCOST.UI.Controls;

/// <summary>
/// A north indicator for a plan view: a fixed viewport marker at the top with a
/// dial that carries the needle and the cardinal letters around it.
/// <see cref="Angle"/> is the rotation of the drawing under the indicator in
/// degrees — the dial turns by the same amount, so the letter sitting under the
/// top marker is the world direction currently pointing up on screen.
/// </summary>
public class CompassIndicator : Control
{
    static CompassIndicator()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(CompassIndicator), new FrameworkPropertyMetadata(typeof(CompassIndicator)));
    }

    public static readonly DependencyProperty AngleProperty =
        DependencyProperty.Register(nameof(Angle), typeof(double), typeof(CompassIndicator),
            new PropertyMetadata(0.0, null, CoerceAngle));

    public static readonly DependencyProperty ShowCardinalsProperty =
        DependencyProperty.Register(nameof(ShowCardinals), typeof(bool), typeof(CompassIndicator),
            new PropertyMetadata(true));

    public static readonly DependencyProperty CardinalBrushProperty =
        DependencyProperty.Register(nameof(CardinalBrush), typeof(Brush), typeof(CompassIndicator),
            new PropertyMetadata(null));

    public static readonly DependencyProperty NorthBrushProperty =
        DependencyProperty.Register(nameof(NorthBrush), typeof(Brush), typeof(CompassIndicator),
            new PropertyMetadata(null));

    /// <summary>Rotation of the drawing under the indicator, in degrees clockwise.</summary>
    public double Angle
    {
        get => (double)GetValue(AngleProperty);
        set => SetValue(AngleProperty, value);
    }

    /// <summary>Draws the four cardinal letters around the dial. The needle and ticks stay either way.</summary>
    public bool ShowCardinals
    {
        get => (bool)GetValue(ShowCardinalsProperty);
        set => SetValue(ShowCardinalsProperty, value);
    }

    /// <summary>Brush of the E/S/W letters; the N takes <see cref="NorthBrush"/>.</summary>
    public Brush? CardinalBrush
    {
        get => (Brush?)GetValue(CardinalBrushProperty);
        set => SetValue(CardinalBrushProperty, value);
    }

    /// <summary>Brush of the north letter, the needle and the top marker.</summary>
    public Brush? NorthBrush
    {
        get => (Brush?)GetValue(NorthBrushProperty);
        set => SetValue(NorthBrushProperty, value);
    }

    private static object CoerceAngle(DependencyObject d, object value)
    {
        var angle = (double)value;
        if (!NumericCompatibility.IsFinite(angle)) return 0d;
        angle %= 360d;
        return angle < 0 ? angle + 360d : angle;
    }
}
