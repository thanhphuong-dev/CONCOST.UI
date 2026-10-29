using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CONCOST.UI.Controls.Canvas;

public class RulerControl : Control
{
    static RulerControl()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(RulerControl), new FrameworkPropertyMetadata(typeof(RulerControl)));
    }

    public static readonly DependencyProperty OrientationProperty =
        DependencyProperty.Register(nameof(Orientation), typeof(Orientation), typeof(RulerControl),
            new PropertyMetadata(Orientation.Horizontal, OnRulerInvalidated));

    public static readonly DependencyProperty UnitScaleProperty =
        DependencyProperty.Register(nameof(UnitScale), typeof(double), typeof(RulerControl),
            new PropertyMetadata(1.0, OnRulerInvalidated));

    public static readonly DependencyProperty MajorStepProperty =
        DependencyProperty.Register(nameof(MajorStep), typeof(double), typeof(RulerControl),
            new PropertyMetadata(50.0, OnRulerInvalidated));

    public static readonly DependencyProperty OriginOffsetProperty =
        DependencyProperty.Register(nameof(OriginOffset), typeof(double), typeof(RulerControl),
            new PropertyMetadata(0.0, OnRulerInvalidated));

    public double OriginOffset
    {
        get => (double)GetValue(OriginOffsetProperty);
        set => SetValue(OriginOffsetProperty, value);
    }

    public Orientation Orientation
    {
        get => (Orientation)GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    public double UnitScale
    {
        get => (double)GetValue(UnitScaleProperty);
        set => SetValue(UnitScaleProperty, value);
    }

    public double MajorStep
    {
        get => (double)GetValue(MajorStepProperty);
        set => SetValue(MajorStepProperty, value);
    }

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);

        var bg = Background ?? Brushes.Transparent;
        dc.DrawRectangle(bg, null, new Rect(0, 0, ActualWidth, ActualHeight));

        var pen = new Pen(Foreground ?? Brushes.Gray, 1);
        pen.Freeze();

        var typeface = new Typeface(FontFamily, FontStyle, FontWeight, FontStretch);
        double step = MajorStep * UnitScale;
        if (step < 10) step = 10;

        if (Orientation == Orientation.Horizontal)
        {
            dc.DrawLine(pen, new Point(0, ActualHeight), new Point(ActualWidth, ActualHeight));

            double startX = OriginOffset % step;
            if (startX > 0) startX -= step;

            for (double x = startX; x < ActualWidth; x += step)
            {
                if (x >= -step)
                {
                    dc.DrawLine(pen, new Point(x, ActualHeight - 8), new Point(x, ActualHeight));
                    double logicalVal = (x - OriginOffset) / UnitScale;
                    var ft = new FormattedText(
                        ((int)Math.Round(logicalVal)).ToString(CultureInfo.InvariantCulture),
                        CultureInfo.CurrentCulture,
                        FlowDirection.LeftToRight,
                        typeface,
                        9,
                        Foreground ?? Brushes.Gray,
                        1.0);
                    dc.DrawText(ft, new Point(x + 2, 2));

                    // Minor ticks
                    double minorStep = step / 5.0;
                    for (int i = 1; i < 5; i++)
                    {
                        double mx = x + i * minorStep;
                        if (mx >= 0 && mx < ActualWidth)
                            dc.DrawLine(pen, new Point(mx, ActualHeight - 4), new Point(mx, ActualHeight));
                    }
                }
            }
        }
        else
        {
            dc.DrawLine(pen, new Point(ActualWidth, 0), new Point(ActualWidth, ActualHeight));

            double startY = OriginOffset % step;
            if (startY > 0) startY -= step;

            for (double y = startY; y < ActualHeight; y += step)
            {
                if (y >= -step)
                {
                    dc.DrawLine(pen, new Point(ActualWidth - 8, y), new Point(ActualWidth, y));
                    double logicalVal = (y - OriginOffset) / UnitScale;
                    var ft = new FormattedText(
                        ((int)Math.Round(logicalVal)).ToString(CultureInfo.InvariantCulture),
                        CultureInfo.CurrentCulture,
                        FlowDirection.LeftToRight,
                        typeface,
                        9,
                        Foreground ?? Brushes.Gray,
                        1.0);
                    dc.DrawText(ft, new Point(2, y + 2));

                    // Minor ticks
                    double minorStep = step / 5.0;
                    for (int i = 1; i < 5; i++)
                    {
                        double my = y + i * minorStep;
                        if (my >= 0 && my < ActualHeight)
                            dc.DrawLine(pen, new Point(ActualWidth - 4, my), new Point(ActualWidth, my));
                    }
                }
            }
        }
    }

    private static void OnRulerInvalidated(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is RulerControl ruler)
        {
            ruler.InvalidateVisual();
        }
    }
}
