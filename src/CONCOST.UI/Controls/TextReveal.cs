using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using CONCOST.UI.Core;

namespace CONCOST.UI.Controls;

/// <summary>
/// Reveals a headline word by word: each word fades in and rises a few pixels, staggered.
/// The web “text reveal” pattern, rendered directly so it costs one text layout per frame.
/// </summary>
public class TextReveal : AnimatedEffectBase
{
    private string _cachedText = string.Empty;
    private string[] _words = Array.Empty<string>();
    private double[] _widths = Array.Empty<double>();
    private double _spaceWidth;

    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(nameof(Text), typeof(string), typeof(TextReveal),
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, OnTextChanged));

    public static readonly DependencyProperty FontSizeProperty =
        DependencyProperty.Register(nameof(FontSize), typeof(double), typeof(TextReveal),
            new FrameworkPropertyMetadata(24.0, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, OnTextChanged));

    public static readonly DependencyProperty FontWeightProperty =
        DependencyProperty.Register(nameof(FontWeight), typeof(FontWeight), typeof(TextReveal),
            new FrameworkPropertyMetadata(FontWeights.Bold, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender, OnTextChanged));

    public static readonly DependencyProperty ForegroundProperty =
        DependencyProperty.Register(nameof(Foreground), typeof(Brush), typeof(TextReveal),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty WordDelayProperty =
        DependencyProperty.Register(nameof(WordDelay), typeof(double), typeof(TextReveal),
            new FrameworkPropertyMetadata(0.085, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty WordDurationProperty =
        DependencyProperty.Register(nameof(WordDuration), typeof(double), typeof(TextReveal),
            new FrameworkPropertyMetadata(0.45, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty OffsetYProperty =
        DependencyProperty.Register(nameof(OffsetY), typeof(double), typeof(TextReveal),
            new FrameworkPropertyMetadata(14.0, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty StartDelayProperty =
        DependencyProperty.Register(nameof(StartDelay), typeof(double), typeof(TextReveal),
            new FrameworkPropertyMetadata(0.1, FrameworkPropertyMetadataOptions.AffectsRender));

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

    public Brush? Foreground
    {
        get => (Brush?)GetValue(ForegroundProperty);
        set => SetValue(ForegroundProperty, value);
    }

    /// <summary>Seconds between two consecutive words.</summary>
    public double WordDelay
    {
        get => (double)GetValue(WordDelayProperty);
        set => SetValue(WordDelayProperty, value);
    }

    /// <summary>Seconds each word takes to fade and rise.</summary>
    public double WordDuration
    {
        get => (double)GetValue(WordDurationProperty);
        set => SetValue(WordDurationProperty, value);
    }

    /// <summary>Vertical distance (px) each word rises from.</summary>
    public double OffsetY
    {
        get => (double)GetValue(OffsetYProperty);
        set => SetValue(OffsetYProperty, value);
    }

    public double StartDelay
    {
        get => (double)GetValue(StartDelayProperty);
        set => SetValue(StartDelayProperty, value);
    }

    public TextReveal()
    {
        IsHitTestVisible = false;
    }

    private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TextReveal reveal) reveal._cachedText = string.Empty;
    }

    private void EnsureWords()
    {
        string text = Text ?? string.Empty;
        if (_cachedText == text && _words.Length > 0) return;

        _cachedText = text;
        _words = text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        _widths = new double[_words.Length];
        for (int i = 0; i < _words.Length; i++)
            _widths[i] = Measure(_words[i]);

        _spaceWidth = Measure(" ");
    }

    private FormattedText Build(string text, Brush brush) =>
        new FormattedText(
            text,
            CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            new Typeface(new FontFamily("Segoe UI"), FontStyles.Normal, FontWeight, FontStretches.Normal),
            FontSize,
            brush,
            VisualTreeHelper.GetDpi(this).PixelsPerDip);

    private double Measure(string text) => Build(text, Brushes.Black).WidthIncludingTrailingWhitespace;

    protected override Size MeasureOverride(Size availableSize)
    {
        EnsureWords();

        double width = 0;
        for (int i = 0; i < _widths.Length; i++)
            width += _widths[i] + (i > 0 ? _spaceWidth : 0);

        double height = Build("Ag", Brushes.Black).Height;
        return new Size(width + 2, height + Math.Max(0, OffsetY) + 2);
    }

    protected override void OnRender(DrawingContext dc)
    {
        EnsureWords();
        if (_words.Length == 0 || ActualWidth <= 0) return;

        Brush brush = Foreground ?? TryFindResource("CONCOST.Brush.TextPrimary") as Brush ?? Brushes.Black;
        bool animate = MotionSettings.AnimationsEnabled;

        double baselineTop = Math.Max(0, OffsetY);
        double delay = Math.Max(0, StartDelay);
        double wordDelay = Math.Max(0.01, WordDelay);
        double duration = Math.Max(0.05, WordDuration);
        double x = 0;

        for (int i = 0; i < _words.Length; i++)
        {
            double progress = 1;
            if (animate)
            {
                double start = delay + i * wordDelay;
                progress = Math.Max(0, Math.Min(1, (Elapsed - start) / duration));
                // Ease-out cubic so words decelerate into place.
                progress = 1 - Math.Pow(1 - progress, 3);
            }

            if (progress > 0.01)
            {
                byte alpha = (byte)Math.Round(255 * progress);
                var wordBrush = brush is SolidColorBrush solid
                    ? new SolidColorBrush(Color.FromArgb(alpha, solid.Color.R, solid.Color.G, solid.Color.B))
                    : brush;
                if (wordBrush is SolidColorBrush created) created.Freeze();

                double offset = animate ? (1 - progress) * Math.Max(0, OffsetY) : 0;
                FormattedText formatted = Build(_words[i], wordBrush);
                dc.DrawText(formatted, new Point(x, baselineTop - offset));
            }

            x += _widths[i] + _spaceWidth;
        }
    }
}
