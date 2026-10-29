using System;
using System.Windows;
using System.Windows.Media;

namespace CONCOST.UI.Controls;

public enum GridPatternType
{
    Dots,
    Lines,
    Cross
}

/// <summary>
/// Tiled dot / line / cross pattern used as an engineering backdrop (drawing paper,
/// blueprint, dashboard grid). Tiled with a <see cref="DrawingBrush"/> so the cost is a
/// single fill regardless of how many cells are visible.
/// </summary>
public class GridPattern : FrameworkElement
{
    public static readonly DependencyProperty CellSizeProperty =
        DependencyProperty.Register(nameof(CellSize), typeof(double), typeof(GridPattern),
            new FrameworkPropertyMetadata(24.0, FrameworkPropertyMetadataOptions.AffectsRender, OnPatternChanged));

    public static readonly DependencyProperty PatternProperty =
        DependencyProperty.Register(nameof(Pattern), typeof(GridPatternType), typeof(GridPattern),
            new FrameworkPropertyMetadata(GridPatternType.Dots, FrameworkPropertyMetadataOptions.AffectsRender, OnPatternChanged));

    public static readonly DependencyProperty DotRadiusProperty =
        DependencyProperty.Register(nameof(DotRadius), typeof(double), typeof(GridPattern),
            new FrameworkPropertyMetadata(1.2, FrameworkPropertyMetadataOptions.AffectsRender, OnPatternChanged));

    public static readonly DependencyProperty StrokeThicknessProperty =
        DependencyProperty.Register(nameof(StrokeThickness), typeof(double), typeof(GridPattern),
            new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.AffectsRender, OnPatternChanged));

    public static readonly DependencyProperty StrokeProperty =
        DependencyProperty.Register(nameof(Stroke), typeof(Brush), typeof(GridPattern),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, OnPatternChanged));

    public static readonly DependencyProperty OffsetXProperty =
        DependencyProperty.Register(nameof(OffsetX), typeof(double), typeof(GridPattern),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender, OnPatternChanged));

    public static readonly DependencyProperty OffsetYProperty =
        DependencyProperty.Register(nameof(OffsetY), typeof(double), typeof(GridPattern),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender, OnPatternChanged));

    public static readonly DependencyProperty PatternOpacityProperty =
        DependencyProperty.Register(nameof(PatternOpacity), typeof(double), typeof(GridPattern),
            new FrameworkPropertyMetadata(0.55, FrameworkPropertyMetadataOptions.AffectsRender, OnPatternChanged));

    public double CellSize
    {
        get => (double)GetValue(CellSizeProperty);
        set => SetValue(CellSizeProperty, value);
    }

    public GridPatternType Pattern
    {
        get => (GridPatternType)GetValue(PatternProperty);
        set => SetValue(PatternProperty, value);
    }

    public double DotRadius
    {
        get => (double)GetValue(DotRadiusProperty);
        set => SetValue(DotRadiusProperty, value);
    }

    public double StrokeThickness
    {
        get => (double)GetValue(StrokeThicknessProperty);
        set => SetValue(StrokeThicknessProperty, value);
    }

    public Brush? Stroke
    {
        get => (Brush?)GetValue(StrokeProperty);
        set => SetValue(StrokeProperty, value);
    }

    /// <summary>Horizontal phase of the pattern — animate it for a parallax backdrop.</summary>
    public double OffsetX
    {
        get => (double)GetValue(OffsetXProperty);
        set => SetValue(OffsetXProperty, value);
    }

    public double OffsetY
    {
        get => (double)GetValue(OffsetYProperty);
        set => SetValue(OffsetYProperty, value);
    }

    public double PatternOpacity
    {
        get => (double)GetValue(PatternOpacityProperty);
        set => SetValue(PatternOpacityProperty, value);
    }

    public GridPattern()
    {
        IsHitTestVisible = false;
    }

    private static void OnPatternChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is GridPattern pattern) pattern.InvalidateVisual();
    }

    protected override void OnRender(DrawingContext dc)
    {
        double width = ActualWidth;
        double height = ActualHeight;
        if (width <= 1 || height <= 1) return;

        double cell = Math.Max(4, CellSize);
        Brush stroke = Stroke ?? TryFindResource("CONCOST.Brush.Border") as Brush ?? Brushes.LightGray;

        var drawing = new GeometryDrawing { Brush = null };
        var pen = new Pen(stroke, Math.Max(0.4, StrokeThickness));
        pen.Freeze();
        drawing.Pen = pen;

        var group = new GeometryGroup { FillRule = FillRule.Nonzero };

        switch (Pattern)
        {
            case GridPatternType.Lines:
                group.Children.Add(new LineGeometry(new Point(0, 0), new Point(cell, 0)));
                group.Children.Add(new LineGeometry(new Point(0, 0), new Point(0, cell)));
                break;

            case GridPatternType.Cross:
                group.Children.Add(new LineGeometry(new Point(cell / 2 - 3, cell / 2), new Point(cell / 2 + 3, cell / 2)));
                group.Children.Add(new LineGeometry(new Point(cell / 2, cell / 2 - 3), new Point(cell / 2, cell / 2 + 3)));
                break;

            default:
                double radius = Math.Max(0.4, DotRadius);
                group.Children.Add(new EllipseGeometry(new Point(cell / 2, cell / 2), radius, radius));
                break;
        }

        group.Freeze();
        drawing.Geometry = group;
        drawing.Freeze();

        var brush = new DrawingBrush(drawing)
        {
            TileMode = TileMode.Tile,
            Viewport = new Rect(0, 0, cell, cell),
            ViewportUnits = BrushMappingMode.Absolute,
            Stretch = Stretch.None,
            Viewbox = new Rect(0, 0, cell, cell),
            ViewboxUnits = BrushMappingMode.Absolute,
            Opacity = Math.Max(0, Math.Min(1, PatternOpacity))
        };

        // Phase the tile so the pattern can drift (parallax) without re-drawing geometry.
        var transform = new TransformGroup();
        transform.Children.Add(new TranslateTransform(
            OffsetX % cell - cell,
            OffsetY % cell - cell));
        brush.Transform = transform;
        brush.Freeze();

        dc.DrawRectangle(brush, null, new Rect(0, 0, width, height));
    }
}
