using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace CONCOST.UI.Controls;

/// <summary>
/// Inline mini chart for trends inside tables, KPI cards and status strips.
/// Renders straight or smoothed lines with an optional gradient area fill.
/// </summary>
public class Sparkline : FrameworkElement
{
    public static readonly DependencyProperty ValuesProperty =
        DependencyProperty.Register(nameof(Values), typeof(IEnumerable<double>), typeof(Sparkline),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty StrokeProperty =
        DependencyProperty.Register(nameof(Stroke), typeof(Brush), typeof(Sparkline),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty FillProperty =
        DependencyProperty.Register(nameof(Fill), typeof(Brush), typeof(Sparkline),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty StrokeThicknessProperty =
        DependencyProperty.Register(nameof(StrokeThickness), typeof(double), typeof(Sparkline),
            new FrameworkPropertyMetadata(1.6, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty ShowAreaProperty =
        DependencyProperty.Register(nameof(ShowArea), typeof(bool), typeof(Sparkline),
            new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty ShowLastDotProperty =
        DependencyProperty.Register(nameof(ShowLastDot), typeof(bool), typeof(Sparkline),
            new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty SmoothProperty =
        DependencyProperty.Register(nameof(Smooth), typeof(bool), typeof(Sparkline),
            new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty LastDotBrushProperty =
        DependencyProperty.Register(nameof(LastDotBrush), typeof(Brush), typeof(Sparkline),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    public IEnumerable<double>? Values
    {
        get => (IEnumerable<double>?)GetValue(ValuesProperty);
        set => SetValue(ValuesProperty, value);
    }

    public Brush? Stroke
    {
        get => (Brush?)GetValue(StrokeProperty);
        set => SetValue(StrokeProperty, value);
    }

    public Brush? Fill
    {
        get => (Brush?)GetValue(FillProperty);
        set => SetValue(FillProperty, value);
    }

    public double StrokeThickness
    {
        get => (double)GetValue(StrokeThicknessProperty);
        set => SetValue(StrokeThicknessProperty, value);
    }

    public bool ShowArea
    {
        get => (bool)GetValue(ShowAreaProperty);
        set => SetValue(ShowAreaProperty, value);
    }

    public bool ShowLastDot
    {
        get => (bool)GetValue(ShowLastDotProperty);
        set => SetValue(ShowLastDotProperty, value);
    }

    public bool Smooth
    {
        get => (bool)GetValue(SmoothProperty);
        set => SetValue(SmoothProperty, value);
    }

    public Brush? LastDotBrush
    {
        get => (Brush?)GetValue(LastDotBrushProperty);
        set => SetValue(LastDotBrushProperty, value);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        double width = double.IsInfinity(availableSize.Width) ? 120 : availableSize.Width;
        double height = double.IsInfinity(availableSize.Height) ? 32 : availableSize.Height;
        return new Size(width, height);
    }

    protected override void OnRender(DrawingContext dc)
    {
        List<double>? values = Values?.Where(v => !double.IsNaN(v) && !double.IsInfinity(v)).ToList();
        if (values is null || values.Count < 2) return;

        double width = ActualWidth;
        double height = ActualHeight;
        if (width <= 1 || height <= 1) return;

        double inset = StrokeThickness / 2.0 + 1.0;
        double min = values.Min();
        double max = values.Max();
        double range = max - min;
        if (range < 0.000001) range = 1;

        int count = values.Count;
        double stepX = (width - inset * 2) / (count - 1);

        var points = new Point[count];
        for (int i = 0; i < count; i++)
        {
            double y = height - inset - ((values[i] - min) / range) * (height - inset * 2);
            points[i] = new Point(inset + i * stepX, y);
        }

        Brush stroke = Stroke ?? TryFindResource("CONCOST.Brush.Primary") as Brush ?? Brushes.DodgerBlue;
        var pen = new Pen(stroke, StrokeThickness)
        {
            LineJoin = PenLineJoin.Round,
            StartLineCap = PenLineCap.Round,
            EndLineCap = PenLineCap.Round
        };
        pen.Freeze();

        StreamGeometry line = BuildGeometry(points, false, Smooth, height - inset);
        line.Freeze();

        if (ShowArea)
        {
            StreamGeometry area = BuildGeometry(points, true, Smooth, height - inset);
            area.Freeze();

            Brush areaBrush = Fill ?? CreateAreaBrush(stroke);
            dc.DrawGeometry(areaBrush, null, area);
        }

        dc.DrawGeometry(null, pen, line);

        if (ShowLastDot)
        {
            Brush dotBrush = LastDotBrush ?? stroke;
            double radius = Math.Max(2.0, StrokeThickness * 1.4);
            dc.DrawEllipse(dotBrush, null, points[count - 1], radius, radius);
        }
    }

    private static StreamGeometry BuildGeometry(Point[] points, bool closeToBottom, bool smooth, double baselineY)
    {
        var geometry = new StreamGeometry();
        using (StreamGeometryContext ctx = geometry.Open())
        {
            ctx.BeginFigure(points[0], closeToBottom, closeToBottom);

            if (smooth)
            {
                for (int i = 1; i < points.Length; i++)
                {
                    Point previous = points[i - 1];
                    Point current = points[i];
                    double controlX = (current.X - previous.X) * 0.42;

                    ctx.BezierTo(
                        new Point(previous.X + controlX, previous.Y),
                        new Point(current.X - controlX, current.Y),
                        current,
                        true,
                        false);
                }
            }
            else
            {
                for (int i = 1; i < points.Length; i++)
                {
                    ctx.LineTo(points[i], true, false);
                }
            }

            if (closeToBottom)
            {
                ctx.LineTo(new Point(points[points.Length - 1].X, baselineY), false, false);
                ctx.LineTo(new Point(points[0].X, baselineY), false, false);
            }
        }

        return geometry;
    }

    private static Brush CreateAreaBrush(Brush stroke)
    {
        Color color = stroke is SolidColorBrush solid ? solid.Color : Colors.DodgerBlue;
        var brush = new LinearGradientBrush
        {
            StartPoint = new Point(0.5, 0),
            EndPoint = new Point(0.5, 1)
        };
        brush.GradientStops.Add(new GradientStop(Color.FromArgb(90, color.R, color.G, color.B), 0));
        brush.GradientStops.Add(new GradientStop(Color.FromArgb(6, color.R, color.G, color.B), 1));
        brush.Freeze();
        return brush;
    }
}
