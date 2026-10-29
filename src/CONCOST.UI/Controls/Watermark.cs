using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;

namespace CONCOST.UI.Controls;

public enum WatermarkMode
{
    SingleCenter,
    RepeatedTiled
}

[ContentProperty(nameof(Content))]
public class Watermark : ContentControl
{
    private Border? _borderRoot;

    static Watermark()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(Watermark), new FrameworkPropertyMetadata(typeof(Watermark)));
    }

    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(nameof(Text), typeof(string), typeof(Watermark),
            new FrameworkPropertyMetadata("PRELIMINARY DRAFT", FrameworkPropertyMetadataOptions.AffectsRender, OnRenderPropertyChanged));

    public static readonly DependencyProperty AngleProperty =
        DependencyProperty.Register(nameof(Angle), typeof(double), typeof(Watermark),
            new FrameworkPropertyMetadata(-25.0, FrameworkPropertyMetadataOptions.AffectsRender, OnRenderPropertyChanged));

    public static readonly DependencyProperty ModeProperty =
        DependencyProperty.Register(nameof(Mode), typeof(WatermarkMode), typeof(Watermark),
            new FrameworkPropertyMetadata(WatermarkMode.SingleCenter, FrameworkPropertyMetadataOptions.AffectsRender, OnRenderPropertyChanged));

    public static readonly DependencyProperty MarkOpacityProperty =
        DependencyProperty.Register(nameof(MarkOpacity), typeof(double), typeof(Watermark),
            new FrameworkPropertyMetadata(0.12, FrameworkPropertyMetadataOptions.AffectsRender, OnRenderPropertyChanged));

    public static readonly DependencyProperty MarkBrushProperty =
        DependencyProperty.Register(nameof(MarkBrush), typeof(Brush), typeof(Watermark),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, OnRenderPropertyChanged));

    public static readonly DependencyProperty MarkFontSizeProperty =
        DependencyProperty.Register(nameof(MarkFontSize), typeof(double), typeof(Watermark),
            new FrameworkPropertyMetadata(28.0, FrameworkPropertyMetadataOptions.AffectsRender, OnRenderPropertyChanged));

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public double Angle
    {
        get => (double)GetValue(AngleProperty);
        set => SetValue(AngleProperty, value);
    }

    public WatermarkMode Mode
    {
        get => (WatermarkMode)GetValue(ModeProperty);
        set => SetValue(ModeProperty, value);
    }

    public double MarkOpacity
    {
        get => (double)GetValue(MarkOpacityProperty);
        set => SetValue(MarkOpacityProperty, value);
    }

    public Brush? MarkBrush
    {
        get => (Brush?)GetValue(MarkBrushProperty);
        set => SetValue(MarkBrushProperty, value);
    }

    public double MarkFontSize
    {
        get => (double)GetValue(MarkFontSizeProperty);
        set => SetValue(MarkFontSizeProperty, value);
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        _borderRoot = GetTemplateChild("PART_Root") as Border;
        UpdateWatermarkBrush();
    }

    private static void OnRenderPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Watermark wm)
        {
            wm.UpdateWatermarkBrush();
        }
    }

    private void UpdateWatermarkBrush()
    {
        if (_borderRoot == null) return;

        if (Mode == WatermarkMode.RepeatedTiled)
        {
            var textBlock = new TextBlock
            {
                Text = Text,
                FontSize = MarkFontSize,
                FontWeight = FontWeights.Normal,
                Foreground = MarkBrush ?? (Brush)FindResource("CONCOST.Brush.TextPrimary"),
                Opacity = MarkOpacity,
                RenderTransform = new RotateTransform(Angle)
            };

            var drawing = new GeometryDrawing
            {
                Brush = textBlock.Foreground,
                Geometry = new FormattedText(
                    Text,
                    System.Globalization.CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    new Typeface(FontFamily, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal),
                    MarkFontSize,
                    textBlock.Foreground,
                    1.0).BuildGeometry(new Point(10, 30))
            };

            var drawingGroup = new DrawingGroup { Opacity = MarkOpacity };
            drawingGroup.Children.Add(drawing);
            drawingGroup.Transform = new RotateTransform(Angle, 120, 60);

            var brush = new DrawingBrush(drawingGroup)
            {
                TileMode = TileMode.Tile,
                Viewport = new Rect(0, 0, 240, 140),
                ViewportUnits = BrushMappingMode.Absolute
            };

            _borderRoot.Background = brush;
        }
        else
        {
            _borderRoot.Background = Brushes.Transparent;
        }
    }
}
