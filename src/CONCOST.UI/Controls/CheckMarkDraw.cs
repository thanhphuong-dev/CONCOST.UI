using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using CONCOST.UI.Core;

namespace CONCOST.UI.Controls;

/// <summary>
/// Check mark that draws itself stroke by stroke — the confirmation tick used after a save
/// or an approval. Drive it with <see cref="IsChecked"/>; the drawing is animated in 320 ms.
/// </summary>
public class CheckMarkDraw : FrameworkElement
{
    private static readonly DependencyProperty ProgressProperty =
        DependencyProperty.Register(nameof(Progress), typeof(double), typeof(CheckMarkDraw),
            new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty IsCheckedProperty =
        DependencyProperty.Register(nameof(IsChecked), typeof(bool), typeof(CheckMarkDraw),
            new FrameworkPropertyMetadata(true, OnIsCheckedChanged));

    public static readonly DependencyProperty StrokeProperty =
        DependencyProperty.Register(nameof(Stroke), typeof(Brush), typeof(CheckMarkDraw),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty StrokeThicknessProperty =
        DependencyProperty.Register(nameof(StrokeThickness), typeof(double), typeof(CheckMarkDraw),
            new FrameworkPropertyMetadata(3.0, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty DurationProperty =
        DependencyProperty.Register(nameof(Duration), typeof(Duration), typeof(CheckMarkDraw),
            new PropertyMetadata(new Duration(TimeSpan.FromMilliseconds(320))));

    public static readonly DependencyProperty BackgroundBrushProperty =
        DependencyProperty.Register(nameof(BackgroundBrush), typeof(Brush), typeof(CheckMarkDraw),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty ShowCircleProperty =
        DependencyProperty.Register(nameof(ShowCircle), typeof(bool), typeof(CheckMarkDraw),
            new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender));

    private double Progress => (double)GetValue(ProgressProperty);

    /// <summary>True draws the tick, false erases it.</summary>
    public bool IsChecked
    {
        get => (bool)GetValue(IsCheckedProperty);
        set => SetValue(IsCheckedProperty, value);
    }

    public Brush? Stroke
    {
        get => (Brush?)GetValue(StrokeProperty);
        set => SetValue(StrokeProperty, value);
    }

    public double StrokeThickness
    {
        get => (double)GetValue(StrokeThicknessProperty);
        set => SetValue(StrokeThicknessProperty, value);
    }

    public Duration Duration
    {
        get => (Duration)GetValue(DurationProperty);
        set => SetValue(DurationProperty, value);
    }

    public Brush? BackgroundBrush
    {
        get => (Brush?)GetValue(BackgroundBrushProperty);
        set => SetValue(BackgroundBrushProperty, value);
    }

    /// <summary>Draws a soft circle behind the tick.</summary>
    public bool ShowCircle
    {
        get => (bool)GetValue(ShowCircleProperty);
        set => SetValue(ShowCircleProperty, value);
    }

    public CheckMarkDraw()
    {
        IsHitTestVisible = false;
    }

    private static void OnIsCheckedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var mark = (CheckMarkDraw)d;
        double to = (bool)e.NewValue ? 1.0 : 0.0;

        if (!MotionSettings.AnimationsEnabled || mark.Duration.TimeSpan <= TimeSpan.Zero)
        {
            mark.SetValue(ProgressProperty, to);
            return;
        }

        var animation = new DoubleAnimation(to, mark.Duration)
        {
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut },
            FillBehavior = FillBehavior.HoldEnd
        };

        mark.BeginAnimation(ProgressProperty, animation);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        double width = double.IsInfinity(availableSize.Width) ? 40 : availableSize.Width;
        double height = double.IsInfinity(availableSize.Height) ? 40 : availableSize.Height;
        return new Size(width, height);
    }

    protected override void OnRender(DrawingContext dc)
    {
        double width = ActualWidth;
        double height = ActualHeight;
        if (width <= 1 || height <= 1) return;

        Brush stroke = Stroke ?? TryFindResource("CONCOST.Brush.Success") as Brush
                       ?? TryFindResource("CONCOST.Brush.Primary") as Brush
                       ?? Brushes.SeaGreen;

        if (ShowCircle)
        {
            Brush fill = BackgroundBrush ?? CreateSoftFill(stroke);
            dc.DrawEllipse(fill, null, new Point(width / 2, height / 2), width / 2, height / 2);
        }

        double progress = Math.Max(0, Math.Min(1, Progress));
        if (progress <= 0.001) return;

        // Two legs: short then long. 40% of the timeline draws the first leg.
        var start = new Point(width * 0.26, height * 0.53);
        var corner = new Point(width * 0.44, height * 0.71);
        var end = new Point(width * 0.75, height * 0.34);

        var pen = new Pen(stroke, Math.Max(1, StrokeThickness))
        {
            StartLineCap = PenLineCap.Round,
            EndLineCap = PenLineCap.Round,
            LineJoin = PenLineJoin.Round
        };
        pen.Freeze();

        if (progress <= 0.4)
        {
            double t = progress / 0.4;
            dc.DrawLine(pen, start, Lerp(start, corner, t));
            return;
        }

        dc.DrawLine(pen, start, corner);
        double t2 = (progress - 0.4) / 0.6;
        dc.DrawLine(pen, corner, Lerp(corner, end, Math.Max(0, Math.Min(1, t2))));
    }

    private static Point Lerp(Point a, Point b, double t) =>
        new(a.X + (b.X - a.X) * t, a.Y + (b.Y - a.Y) * t);

    private static Brush CreateSoftFill(Brush stroke)
    {
        Color color = stroke is SolidColorBrush solid ? solid.Color : Colors.SeaGreen;
        var brush = new SolidColorBrush(Color.FromArgb(38, color.R, color.G, color.B));
        brush.Freeze();
        return brush;
    }
}
