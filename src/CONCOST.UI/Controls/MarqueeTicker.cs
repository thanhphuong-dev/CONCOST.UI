using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace CONCOST.UI.Controls;

/// <summary>
/// Continuously scrolling text strip (marquee) for status bars, site notices and
/// tickers. Rendered directly and driven by <see cref="CompositionTarget.Rendering"/>,
/// so it stays smooth without a Storyboard per item.
/// </summary>
public class MarqueeTicker : FrameworkElement
{
    private double _offset;
    private DateTime _lastTick;
    private bool _hooked;

    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(nameof(Text), typeof(string), typeof(MarqueeTicker),
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty SpeedProperty =
        DependencyProperty.Register(nameof(Speed), typeof(double), typeof(MarqueeTicker),
            new FrameworkPropertyMetadata(48.0, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty GapProperty =
        DependencyProperty.Register(nameof(Gap), typeof(double), typeof(MarqueeTicker),
            new FrameworkPropertyMetadata(64.0, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty FontSizeProperty =
        DependencyProperty.Register(nameof(FontSize), typeof(double), typeof(MarqueeTicker),
            new FrameworkPropertyMetadata(13.0, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty ForegroundProperty =
        DependencyProperty.Register(nameof(Foreground), typeof(Brush), typeof(MarqueeTicker),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty SeparatorProperty =
        DependencyProperty.Register(nameof(Separator), typeof(string), typeof(MarqueeTicker),
            new FrameworkPropertyMetadata("  •  ", FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty IsRunningProperty =
        DependencyProperty.Register(nameof(IsRunning), typeof(bool), typeof(MarqueeTicker),
            new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender));

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public double Speed
    {
        get => (double)GetValue(SpeedProperty);
        set => SetValue(SpeedProperty, value);
    }

    public double Gap
    {
        get => (double)GetValue(GapProperty);
        set => SetValue(GapProperty, value);
    }

    public double FontSize
    {
        get => (double)GetValue(FontSizeProperty);
        set => SetValue(FontSizeProperty, value);
    }

    public Brush? Foreground
    {
        get => (Brush?)GetValue(ForegroundProperty);
        set => SetValue(ForegroundProperty, value);
    }

    public string Separator
    {
        get => (string)GetValue(SeparatorProperty);
        set => SetValue(SeparatorProperty, value);
    }

    public bool IsRunning
    {
        get => (bool)GetValue(IsRunningProperty);
        set => SetValue(IsRunningProperty, value);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        double width = double.IsInfinity(availableSize.Width) ? 240 : availableSize.Width;
        double height = FontSize * 1.9;
        return new Size(width, height);
    }

    protected override void OnRender(DrawingContext dc)
    {
        string text = BuildLine();
        if (string.IsNullOrEmpty(text) || ActualWidth <= 0) return;

        // Clip so the marquee never paints outside its box.
        dc.PushClip(new RectangleGeometry(new Rect(0, 0, ActualWidth, ActualHeight)));

        Brush brush = Foreground ?? TryFindResource("CONCOST.Brush.TextSecondary") as Brush ?? Brushes.Gray;
        double dpi = VisualTreeHelper.GetDpi(this).PixelsPerDip;

        var formatted = new FormattedText(
            text,
            CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            new Typeface("Segoe UI"),
            FontSize,
            brush,
            dpi);

        double lineWidth = formatted.Width + Gap;
        double y = (ActualHeight - formatted.Height) / 2;
        double x = -_offset;

        if (lineWidth <= 0) { dc.Pop(); return; }

        while (x < ActualWidth)
        {
            dc.DrawText(formatted, new Point(x, y));
            x += lineWidth;
        }

        dc.Pop();
    }

    private string BuildLine()
    {
        string text = Text ?? string.Empty;
        if (text.Length == 0) return string.Empty;
        return text + Separator;
    }

    private void OnFrame(object? sender, EventArgs e)
    {
        if (!IsRunning || ActualWidth <= 0) { _lastTick = DateTime.UtcNow; return; }

        DateTime now = DateTime.UtcNow;
        if (_lastTick == default) _lastTick = now;

        double delta = (now - _lastTick).TotalSeconds;
        _lastTick = now;
        if (delta <= 0 || delta > 0.5) delta = 0.016;

        _offset += Speed * delta;

        double lineWidth = MeasureLineWidth();
        if (lineWidth > 0 && _offset >= lineWidth) _offset -= lineWidth;

        InvalidateVisual();
    }

    private double MeasureLineWidth()
    {
        string text = BuildLine();
        if (string.IsNullOrEmpty(text)) return 0;

        var formatted = new FormattedText(
            text,
            CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            new Typeface("Segoe UI"),
            FontSize,
            Brushes.Black,
            VisualTreeHelper.GetDpi(this).PixelsPerDip);

        return formatted.Width + Gap;
    }

    protected override void OnVisualParentChanged(DependencyObject oldParent)
    {
        base.OnVisualParentChanged(oldParent);
        Hook();
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
