using System;
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

    public static readonly DependencyProperty DialBrushProperty =
        DependencyProperty.Register(nameof(DialBrush), typeof(Brush), typeof(CompassIndicator),
            new PropertyMetadata(null));

    public static readonly DependencyProperty DialBorderBrushProperty =
        DependencyProperty.Register(nameof(DialBorderBrush), typeof(Brush), typeof(CompassIndicator),
            new PropertyMetadata(null));

    public static readonly DependencyProperty DialBorderThicknessProperty =
        DependencyProperty.Register(nameof(DialBorderThickness), typeof(double), typeof(CompassIndicator),
            new PropertyMetadata(1.0, null, CoerceNonNegative));

    public static readonly DependencyProperty DialInsetProperty =
        DependencyProperty.Register(nameof(DialInset), typeof(Thickness), typeof(CompassIndicator),
            new PropertyMetadata(new Thickness(7)));

    public static readonly DependencyProperty NeedleThicknessProperty =
        DependencyProperty.Register(nameof(NeedleThickness), typeof(double), typeof(CompassIndicator),
            new PropertyMetadata(1.5, null, CoercePositive));

    public static readonly DependencyProperty CardinalFontSizeProperty =
        DependencyProperty.Register(nameof(CardinalFontSize), typeof(double), typeof(CompassIndicator),
            new PropertyMetadata(9.0, null, CoercePositive));

    public static readonly DependencyProperty CenterMarkerBrushProperty =
        DependencyProperty.Register(nameof(CenterMarkerBrush), typeof(Brush), typeof(CompassIndicator),
            new PropertyMetadata(null));

    public static readonly DependencyProperty CenterMarkerSizeProperty =
        DependencyProperty.Register(nameof(CenterMarkerSize), typeof(double), typeof(CompassIndicator),
            new PropertyMetadata(4.0, null, CoerceNonNegative));

    public static readonly DependencyProperty ShowCenterMarkerProperty =
        DependencyProperty.Register(nameof(ShowCenterMarker), typeof(bool), typeof(CompassIndicator),
            new PropertyMetadata(true));

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

    /// <summary>Background brush of the circular dial. Null uses the theme surface brush.</summary>
    public Brush? DialBrush
    {
        get => (Brush?)GetValue(DialBrushProperty);
        set => SetValue(DialBrushProperty, value);
    }

    /// <summary>Outline brush of the dial. Null uses the theme border brush.</summary>
    public Brush? DialBorderBrush
    {
        get => (Brush?)GetValue(DialBorderBrushProperty);
        set => SetValue(DialBorderBrushProperty, value);
    }

    /// <summary>Outline width of the dial, in device-independent pixels.</summary>
    public double DialBorderThickness
    {
        get => (double)GetValue(DialBorderThicknessProperty);
        set => SetValue(DialBorderThicknessProperty, value);
    }

    /// <summary>Inset between the dial edge and its needle/cardinal content.</summary>
    public Thickness DialInset
    {
        get => (Thickness)GetValue(DialInsetProperty);
        set => SetValue(DialInsetProperty, value);
    }

    /// <summary>Stroke width of the north needle, in device-independent pixels.</summary>
    public double NeedleThickness
    {
        get => (double)GetValue(NeedleThicknessProperty);
        set => SetValue(NeedleThicknessProperty, value);
    }

    /// <summary>Font size of the cardinal letters, in device-independent pixels.</summary>
    public double CardinalFontSize
    {
        get => (double)GetValue(CardinalFontSizeProperty);
        set => SetValue(CardinalFontSizeProperty, value);
    }

    /// <summary>Brush of the centre marker. Null uses the theme secondary text brush.</summary>
    public Brush? CenterMarkerBrush
    {
        get => (Brush?)GetValue(CenterMarkerBrushProperty);
        set => SetValue(CenterMarkerBrushProperty, value);
    }

    /// <summary>Diameter of the centre marker, in device-independent pixels.</summary>
    public double CenterMarkerSize
    {
        get => (double)GetValue(CenterMarkerSizeProperty);
        set => SetValue(CenterMarkerSizeProperty, value);
    }

    /// <summary>Shows or hides the marker at the centre of the dial.</summary>
    public bool ShowCenterMarker
    {
        get => (bool)GetValue(ShowCenterMarkerProperty);
        set => SetValue(ShowCenterMarkerProperty, value);
    }

    private static object CoerceNonNegative(DependencyObject _, object value) =>
        value is double number && NumericCompatibility.IsFinite(number) ? Math.Max(0, number) : 0d;

    private static object CoercePositive(DependencyObject _, object value) =>
        value is double number && NumericCompatibility.IsFinite(number) ? Math.Max(0.5, number) : 1d;

    private static object CoerceAngle(DependencyObject d, object value)
    {
        var angle = (double)value;
        if (!NumericCompatibility.IsFinite(angle)) return 0d;
        angle %= 360d;
        return angle < 0 ? angle + 360d : angle;
    }
}
