using System.Windows;
using System.Windows.Controls;

namespace CONCOST.UI.Controls;

public class CompassIndicator : Control
{
    static CompassIndicator()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(CompassIndicator), new FrameworkPropertyMetadata(typeof(CompassIndicator)));
    }

    public static readonly DependencyProperty AngleProperty =
        DependencyProperty.Register(nameof(Angle), typeof(double), typeof(CompassIndicator),
            new PropertyMetadata(0.0, null, CoerceAngle));

    public double Angle
    {
        get => (double)GetValue(AngleProperty);
        set => SetValue(AngleProperty, value);
    }

    private static object CoerceAngle(DependencyObject d, object value)
    {
        var angle = (double)value;
        if (!NumericCompatibility.IsFinite(angle)) return 0d;
        angle %= 360d;
        return angle < 0 ? angle + 360d : angle;
    }
}
