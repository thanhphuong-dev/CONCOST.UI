using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using CONCOST.UI.Core;

namespace CONCOST.UI.Controls;

/// <summary>
/// Text filled with an animated gradient — the “gradient headline” used on modern
/// product pages. Rendered directly so the gradient can move without touching a template.
/// </summary>
public class GradientText : FrameworkElement
{
    private double _elapsed;
    private TimeSpan _last;
    private bool _hooked;

    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(nameof(Text), typeof(string), typeof(GradientText),
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty FontSizeProperty =
        DependencyProperty.Register(nameof(FontSize), typeof(double), typeof(GradientText),
            new FrameworkPropertyMetadata(28.0, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty FontWeightProperty =
        DependencyProperty.Register(nameof(FontWeight), typeof(FontWeight), typeof(GradientText),
            new FrameworkPropertyMetadata(FontWeights.Bold, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty StartColorProperty =
        DependencyProperty.Register(nameof(StartColor), typeof(Color), typeof(GradientText),
            new FrameworkPropertyMetadata(Color.FromRgb(0xFF, 0x79, 0x00), FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty EndColorProperty =
        DependencyProperty.Register(nameof(EndColor), typeof(Color), typeof(GradientText),
            new FrameworkPropertyMetadata(Color.FromRgb(0x02, 0x84, 0xC7), FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty AngleProperty =
        DependencyProperty.Register(nameof(Angle), typeof(double), typeof(GradientText),
            new FrameworkPropertyMetadata(15.0, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty SpeedProperty =
        DependencyProperty.Register(nameof(Speed), typeof(double), typeof(GradientText),
            new FrameworkPropertyMetadata(0.35, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty IsAnimatedProperty =
        DependencyProperty.Register(nameof(IsAnimated), typeof(bool), typeof(GradientText),
            new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender));

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public double FontSize
    {
        get => (double)GetValue(FontSizeProperty);
        set => SetValue(FontSizeProperty, value);
    }

    public FontWeight FontWeight
    {
        get => (FontWeight)GetValue(FontWeightProperty);
        set => SetValue(FontWeightProperty, value);
    }

    public Color StartColor
    {
        get => (Color)GetValue(StartColorProperty);
        set => SetValue(StartColorProperty, value);
    }

    public Color EndColor
    {
        get => (Color)GetValue(EndColorProperty);
        set => SetValue(EndColorProperty, value);
    }

    public double Angle
    {
        get => (double)GetValue(AngleProperty);
        set => SetValue(AngleProperty, value);
    }

    /// <summary>Gradient cycles per second (0.35 = one full pass every ~3 s).</summary>
    public double Speed
    {
        get => (double)GetValue(SpeedProperty);
        set => SetValue(SpeedProperty, value);
    }

    public bool IsAnimated
    {
        get => (bool)GetValue(IsAnimatedProperty);
        set => SetValue(IsAnimatedProperty, value);
    }

    public GradientText()
    {
        IsHitTestVisible = false;
    }

    private FormattedText BuildText(Brush brush)
    {
        return new FormattedText(
            Text ?? string.Empty,
            CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            new Typeface(new FontFamily("Segoe UI"), FontStyles.Normal, FontWeight, FontStretches.Normal),
            FontSize,
            brush,
            VisualTreeHelper.GetDpi(this).PixelsPerDip);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        FormattedText text = BuildText(Brushes.Black);
        return new Size(text.Width, text.Height);
    }

    protected override void OnRender(DrawingContext dc)
    {
        if (string.IsNullOrEmpty(Text) || ActualWidth <= 0) return;

        double offset = (_elapsed % 1.0);
        if (!IsAnimated) offset = 0;

        var brush = new LinearGradientBrush
        {
            StartPoint = new Point(0, 0.5),
            EndPoint = new Point(1, 0.5),
            SpreadMethod = GradientSpreadMethod.Reflect
        };
        brush.GradientStops.Add(new GradientStop(StartColor, 0));
        brush.GradientStops.Add(new GradientStop(EndColor, 0.5));
        brush.GradientStops.Add(new GradientStop(StartColor, 1));

        // Slide the gradient: rotate around the text centre and shift by the phase.
        var group = new TransformGroup();
        group.Children.Add(new RotateTransform(Angle, 0.5, 0.5));
        group.Children.Add(new TranslateTransform(offset * 2 - 1, 0));
        brush.RelativeTransform = group;
        brush.Freeze();

        FormattedText text = BuildText(brush);
        dc.DrawText(text, new Point(0, 0));
    }

    private void OnFrame(object? sender, EventArgs e)
    {
        if (!MotionSettings.AnimationsEnabled || !IsAnimated || ActualWidth <= 0) { _last = TimeSpan.Zero; return; }

        TimeSpan now = TimeSpan.FromTicks(DateTime.UtcNow.Ticks);
        if (_last == TimeSpan.Zero) { _last = now; return; }

        double delta = (now - _last).TotalSeconds;
        _last = now;
        if (delta <= 0 || delta > 0.5) delta = 0.016;

        _elapsed += Speed * delta;
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
