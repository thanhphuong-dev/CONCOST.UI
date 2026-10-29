using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using CONCOST.UI.Core;

namespace CONCOST.UI.Controls;

/// <summary>
/// Types the text out character by character with an optional blinking caret — the classic
/// terminal / hero-headline effect. When motion is disabled the full text is shown at once.
/// </summary>
public class TypewriterText : AnimatedEffectBase
{
    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(nameof(Text), typeof(string), typeof(TypewriterText),
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty CharsPerSecondProperty =
        DependencyProperty.Register(nameof(CharsPerSecond), typeof(double), typeof(TypewriterText),
            new FrameworkPropertyMetadata(26.0, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty StartDelayProperty =
        DependencyProperty.Register(nameof(StartDelay), typeof(double), typeof(TypewriterText),
            new FrameworkPropertyMetadata(0.25, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty ShowCaretProperty =
        DependencyProperty.Register(nameof(ShowCaret), typeof(bool), typeof(TypewriterText),
            new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty CaretBrushProperty =
        DependencyProperty.Register(nameof(CaretBrush), typeof(Brush), typeof(TypewriterText),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty FontSizeProperty =
        DependencyProperty.Register(nameof(FontSize), typeof(double), typeof(TypewriterText),
            new FrameworkPropertyMetadata(16.0, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty FontWeightProperty =
        DependencyProperty.Register(nameof(FontWeight), typeof(FontWeight), typeof(TypewriterText),
            new FrameworkPropertyMetadata(FontWeights.Normal, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty ForegroundProperty =
        DependencyProperty.Register(nameof(Foreground), typeof(Brush), typeof(TypewriterText),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty IsLoopingProperty =
        DependencyProperty.Register(nameof(IsLooping), typeof(bool), typeof(TypewriterText),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty RestartDelayProperty =
        DependencyProperty.Register(nameof(RestartDelay), typeof(double), typeof(TypewriterText),
            new FrameworkPropertyMetadata(1.6, FrameworkPropertyMetadataOptions.AffectsRender));

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    /// <summary>Typing speed.</summary>
    public double CharsPerSecond
    {
        get => (double)GetValue(CharsPerSecondProperty);
        set => SetValue(CharsPerSecondProperty, value);
    }

    /// <summary>Seconds to wait before the first character.</summary>
    public double StartDelay
    {
        get => (double)GetValue(StartDelayProperty);
        set => SetValue(StartDelayProperty, value);
    }

    public bool ShowCaret
    {
        get => (bool)GetValue(ShowCaretProperty);
        set => SetValue(ShowCaretProperty, value);
    }

    public Brush? CaretBrush
    {
        get => (Brush?)GetValue(CaretBrushProperty);
        set => SetValue(CaretBrushProperty, value);
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

    /// <summary>Restarts from empty after the whole string is typed.</summary>
    public bool IsLooping
    {
        get => (bool)GetValue(IsLoopingProperty);
        set => SetValue(IsLoopingProperty, value);
    }

    public double RestartDelay
    {
        get => (double)GetValue(RestartDelayProperty);
        set => SetValue(RestartDelayProperty, value);
    }

    public TypewriterText()
    {
        IsHitTestVisible = false;
    }

    /// <summary>Characters currently visible — useful for tests and view models.</summary>
    public int VisibleCharacters
    {
        get
        {
            string text = Text ?? string.Empty;
            if (!MotionSettings.AnimationsEnabled) return text.Length;

            double speed = CharsPerSecond <= 0.5 ? 0.5 : CharsPerSecond;
            double typing = Math.Max(0, Elapsed - Math.Max(0, StartDelay)) * speed;

            if (typing <= text.Length) return (int)Math.Min(text.Length, typing);

            if (!IsLooping) return text.Length;

            double total = text.Length + Math.Max(0.2, RestartDelay);
            double cycle = Math.Max(0.2, Elapsed - Math.Max(0, StartDelay)) % total;
            return (int)Math.Min(text.Length, cycle * speed);
        }
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        FormattedText formatted = BuildText(Text ?? string.Empty, Brushes.Black);
        double caret = ShowCaret ? FontSize * 0.5 : 0;
        return new Size(formatted.Width + caret + 2, formatted.Height + 2);
    }

    private FormattedText BuildText(string text, Brush brush)
    {
        return new FormattedText(
            text,
            CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            new Typeface(new FontFamily("Segoe UI"), FontStyles.Normal, FontWeight, FontStretches.Normal),
            FontSize,
            brush,
            VisualTreeHelper.GetDpi(this).PixelsPerDip);
    }

    protected override void OnRender(DrawingContext dc)
    {
        string text = Text ?? string.Empty;
        if (text.Length == 0 || ActualWidth <= 0) return;

        int visible = Math.Max(0, Math.Min(text.Length, VisibleCharacters));
        Brush brush = Foreground ?? TryFindResource("CONCOST.Brush.TextPrimary") as Brush ?? Brushes.Black;

        string shown = text.Substring(0, visible);
        FormattedText formatted = BuildText(shown, brush);
        dc.DrawText(formatted, new Point(0, 0));

        if (!ShowCaret) return;

        // Caret blinks while typing and stays solid for a moment when finished.
        bool typing = visible < text.Length;
        bool blinkOn = !typing || (Elapsed * 2) % 1 < 0.55;
        if (!blinkOn) return;

        Brush caretBrush = CaretBrush ?? brush;
        double height = formatted.Height * 0.92;
        dc.DrawRectangle(caretBrush, null, new Rect(formatted.Width + 2, (formatted.Height - height) / 2, Math.Max(1.5, FontSize * 0.08), height));
    }
}
