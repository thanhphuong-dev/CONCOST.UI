using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CONCOST.UI.Core;

namespace CONCOST.UI.Controls;

/// <summary>
/// Draws a light beam running around the border of any content — the “border beam”
/// effect used by modern web cards. Wrap your content and the beam animates on the
/// edge; the surface fill and the beam colour are both configurable.
/// </summary>
public class BorderBeam : Decorator
{
    private double _angle;
    private TimeSpan _last;
    private bool _hooked;

    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(BorderBeam),
            new FrameworkPropertyMetadata(new CornerRadius(12), FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty BeamBrushProperty =
        DependencyProperty.Register(nameof(BeamBrush), typeof(Brush), typeof(BorderBeam),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty BeamThicknessProperty =
        DependencyProperty.Register(nameof(BeamThickness), typeof(double), typeof(BorderBeam),
            new FrameworkPropertyMetadata(1.6, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty BeamLengthProperty =
        DependencyProperty.Register(nameof(BeamLength), typeof(double), typeof(BorderBeam),
            new FrameworkPropertyMetadata(0.35, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty RevolutionSecondsProperty =
        DependencyProperty.Register(nameof(RevolutionSeconds), typeof(double), typeof(BorderBeam),
            new FrameworkPropertyMetadata(4.0, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty IsAnimatedProperty =
        DependencyProperty.Register(nameof(IsAnimated), typeof(bool), typeof(BorderBeam),
            new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty SurfaceBrushProperty =
        DependencyProperty.Register(nameof(SurfaceBrush), typeof(Brush), typeof(BorderBeam),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty PaddingProperty =
        DependencyProperty.Register(nameof(Padding), typeof(Thickness), typeof(BorderBeam),
            new FrameworkPropertyMetadata(new Thickness(16), FrameworkPropertyMetadataOptions.AffectsMeasure));

    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    public Brush? BeamBrush
    {
        get => (Brush?)GetValue(BeamBrushProperty);
        set => SetValue(BeamBrushProperty, value);
    }

    public double BeamThickness
    {
        get => (double)GetValue(BeamThicknessProperty);
        set => SetValue(BeamThicknessProperty, value);
    }

    /// <summary>Length of the lit segment as a fraction of the perimeter (0–1).</summary>
    public double BeamLength
    {
        get => (double)GetValue(BeamLengthProperty);
        set => SetValue(BeamLengthProperty, value);
    }

    public double RevolutionSeconds
    {
        get => (double)GetValue(RevolutionSecondsProperty);
        set => SetValue(RevolutionSecondsProperty, value);
    }

    public bool IsAnimated
    {
        get => (bool)GetValue(IsAnimatedProperty);
        set => SetValue(IsAnimatedProperty, value);
    }

    public Brush? SurfaceBrush
    {
        get => (Brush?)GetValue(SurfaceBrushProperty);
        set => SetValue(SurfaceBrushProperty, value);
    }

    public Thickness Padding
    {
        get => (Thickness)GetValue(PaddingProperty);
        set => SetValue(PaddingProperty, value);
    }

    protected override Size MeasureOverride(Size constraint)
    {
        Thickness pad = Padding;
        Size inner = new Size(
            Math.Max(0, constraint.Width - pad.Left - pad.Right),
            Math.Max(0, constraint.Height - pad.Top - pad.Bottom));

        if (Child != null)
        {
            Child.Measure(inner);
            Size desired = Child.DesiredSize;
            return new Size(desired.Width + pad.Left + pad.Right, desired.Height + pad.Top + pad.Bottom);
        }

        return new Size(pad.Left + pad.Right, pad.Top + pad.Bottom);
    }

    protected override Size ArrangeOverride(Size arrangeSize)
    {
        Thickness pad = Padding;
        Child?.Arrange(new Rect(pad.Left, pad.Top,
            Math.Max(0, arrangeSize.Width - pad.Left - pad.Right),
            Math.Max(0, arrangeSize.Height - pad.Top - pad.Bottom)));
        return arrangeSize;
    }

    protected override void OnRender(DrawingContext dc)
    {
        double width = ActualWidth;
        double height = ActualHeight;
        if (width <= 1 || height <= 1) return;

        double inset = BeamThickness / 2;
        var rect = new Rect(inset, inset, Math.Max(0, width - BeamThickness), Math.Max(0, height - BeamThickness));

        if (SurfaceBrush != null)
            dc.DrawRoundedRectangle(SurfaceBrush, null, rect, CornerRadius.TopLeft, CornerRadius.TopLeft);

        DrawBeam(dc, rect);
    }

    /// <summary>
    /// Draws a comet-like light that travels along the rounded-rect perimeter.
    /// A rotating linear gradient only lights two arcs, so the beam is built as a
    /// polyline sampled along the perimeter instead.
    /// </summary>
    private void DrawBeam(DrawingContext dc, Rect rect)
    {
        Color color = BeamBrush is SolidColorBrush solid
            ? solid.Color
            : (TryFindResource("CONCOST.Brush.Primary") as SolidColorBrush)?.Color ?? Colors.Orange;

        double radius = Math.Max(0, Math.Min(CornerRadius.TopLeft, Math.Min(rect.Width, rect.Height) / 2));
        double perimeter = Perimeter(rect, radius);
        if (perimeter <= 0) return;

        double length = Math.Max(0.05, Math.Min(0.9, BeamLength)) * perimeter;
        double head = _angle / 360.0 * perimeter;

        // Three passes: a wide faint glow, a mid glow, then the bright core.
        var layers = new[]
        {
            (Thickness: BeamThickness * 4.0, Alpha: (byte)28, LengthFactor: 1.0),
            (Thickness: BeamThickness * 2.2, Alpha: (byte)70, LengthFactor: 0.85),
            (Thickness: BeamThickness, Alpha: (byte)235, LengthFactor: 0.62)
        };

        foreach (var layer in layers)
        {
            StreamGeometry geometry = BuildArc(rect, radius, perimeter, head, length * layer.LengthFactor);
            geometry.Freeze();

            var brush = new SolidColorBrush(Color.FromArgb(layer.Alpha, color.R, color.G, color.B));
            brush.Freeze();

            var pen = new Pen(brush, layer.Thickness)
            {
                StartLineCap = PenLineCap.Round,
                EndLineCap = PenLineCap.Round,
                LineJoin = PenLineJoin.Round
            };
            pen.Freeze();

            dc.DrawGeometry(null, pen, geometry);
        }
    }

    private static StreamGeometry BuildArc(Rect rect, double radius, double perimeter, double start, double length)
    {
        const int Samples = 44;

        var geometry = new StreamGeometry();
        using (StreamGeometryContext ctx = geometry.Open())
        {
            ctx.BeginFigure(PointAt(rect, radius, perimeter, start), false, false);
            for (int i = 1; i <= Samples; i++)
            {
                double t = start + length * i / Samples;
                ctx.LineTo(PointAt(rect, radius, perimeter, t), true, false);
            }
        }

        return geometry;
    }

    private static double Perimeter(Rect rect, double radius)
    {
        double straight = 2 * Math.Max(0, rect.Width - 2 * radius) + 2 * Math.Max(0, rect.Height - 2 * radius);
        return straight + 2 * Math.PI * radius;
    }

    private static Point PointAt(Rect rect, double radius, double perimeter, double distance)
    {
        double t = distance % perimeter;
        if (t < 0) t += perimeter;

        double w = rect.Width, h = rect.Height, r = radius;
        double edgeX = Math.Max(0, w - 2 * r);
        double edgeY = Math.Max(0, h - 2 * r);
        double arc = Math.PI * r / 2;

        // top edge (left -> right)
        if (t <= edgeX) return new Point(rect.Left + r + t, rect.Top);
        t -= edgeX;

        // top-right arc
        if (t <= arc) return ArcPoint(rect.Left + w - r, rect.Top + r, r, -90 + 90 * (t / arc));
        t -= arc;

        // right edge
        if (t <= edgeY) return new Point(rect.Left + w, rect.Top + r + t);
        t -= edgeY;

        // bottom-right arc
        if (t <= arc) return ArcPoint(rect.Left + w - r, rect.Top + h - r, r, 90 * (t / arc));
        t -= arc;

        // bottom edge (right -> left)
        if (t <= edgeX) return new Point(rect.Left + w - r - t, rect.Top + h);
        t -= edgeX;

        // bottom-left arc
        if (t <= arc) return ArcPoint(rect.Left + r, rect.Top + h - r, r, 90 + 90 * (t / arc));
        t -= arc;

        // left edge (bottom -> top)
        if (t <= edgeY) return new Point(rect.Left, rect.Top + h - r - t);
        t -= edgeY;

        // top-left arc
        return ArcPoint(rect.Left + r, rect.Top + r, r, 180 + 90 * (t / arc));
    }

    private static Point ArcPoint(double cx, double cy, double radius, double degrees)
    {
        double rad = degrees * Math.PI / 180;
        return new Point(cx + radius * Math.Cos(rad), cy + radius * Math.Sin(rad));
    }

    private void OnFrame(object? sender, EventArgs e)
    {
        if (!MotionSettings.AnimationsEnabled || !IsAnimated || ActualWidth <= 0) { _last = TimeSpan.Zero; return; }

        TimeSpan now = TimeSpan.FromTicks(DateTime.UtcNow.Ticks);
        if (_last == TimeSpan.Zero) { _last = now; return; }

        double delta = (now - _last).TotalSeconds;
        _last = now;
        if (delta <= 0 || delta > 0.5) delta = 0.016;

        double seconds = RevolutionSeconds <= 0.2 ? 0.2 : RevolutionSeconds;
        _angle = (_angle + 360.0 / seconds * delta) % 360.0;
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
