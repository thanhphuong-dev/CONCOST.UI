using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace CONCOST.UI.Controls;

/// <summary>
/// Overlapping avatar row with an optional "+N" overflow chip — the stacked
/// assignee pattern from web dashboards, drawn directly for a stable layout.
/// Items may be a string (initials), an <see cref="ImageSource"/> or any object
/// whose ToString() is used for the initials.
/// </summary>
public class AvatarStack : FrameworkElement
{
    public static readonly DependencyProperty ItemsProperty =
        DependencyProperty.Register(nameof(Items), typeof(IEnumerable), typeof(AvatarStack),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty MaxVisibleProperty =
        DependencyProperty.Register(nameof(MaxVisible), typeof(int), typeof(AvatarStack),
            new FrameworkPropertyMetadata(4, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty SizeProperty =
        DependencyProperty.Register(nameof(Size), typeof(double), typeof(AvatarStack),
            new FrameworkPropertyMetadata(32.0, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty OverlapProperty =
        DependencyProperty.Register(nameof(Overlap), typeof(double), typeof(AvatarStack),
            new FrameworkPropertyMetadata(10.0, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty BackgroundProperty =
        DependencyProperty.Register(nameof(Background), typeof(Brush), typeof(AvatarStack),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty ForegroundProperty =
        DependencyProperty.Register(nameof(Foreground), typeof(Brush), typeof(AvatarStack),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty RingBrushProperty =
        DependencyProperty.Register(nameof(RingBrush), typeof(Brush), typeof(AvatarStack),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty FontSizeProperty =
        DependencyProperty.Register(nameof(FontSize), typeof(double), typeof(AvatarStack),
            new FrameworkPropertyMetadata(11.5, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty ShowOverflowProperty =
        DependencyProperty.Register(nameof(ShowOverflow), typeof(bool), typeof(AvatarStack),
            new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty OverflowTextFormatProperty =
        DependencyProperty.Register(nameof(OverflowTextFormat), typeof(string), typeof(AvatarStack),
            new FrameworkPropertyMetadata("+{0}", FrameworkPropertyMetadataOptions.AffectsRender));

    public IEnumerable? Items
    {
        get => (IEnumerable?)GetValue(ItemsProperty);
        set => SetValue(ItemsProperty, value);
    }

    public int MaxVisible
    {
        get => (int)GetValue(MaxVisibleProperty);
        set => SetValue(MaxVisibleProperty, value);
    }

    public double Size
    {
        get => (double)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    public double Overlap
    {
        get => (double)GetValue(OverlapProperty);
        set => SetValue(OverlapProperty, value);
    }

    public Brush? Background
    {
        get => (Brush?)GetValue(BackgroundProperty);
        set => SetValue(BackgroundProperty, value);
    }

    public Brush? Foreground
    {
        get => (Brush?)GetValue(ForegroundProperty);
        set => SetValue(ForegroundProperty, value);
    }

    public Brush? RingBrush
    {
        get => (Brush?)GetValue(RingBrushProperty);
        set => SetValue(RingBrushProperty, value);
    }

    public double FontSize
    {
        get => (double)GetValue(FontSizeProperty);
        set => SetValue(FontSizeProperty, value);
    }

    public bool ShowOverflow
    {
        get => (bool)GetValue(ShowOverflowProperty);
        set => SetValue(ShowOverflowProperty, value);
    }

    public string OverflowTextFormat
    {
        get => (string)GetValue(OverflowTextFormatProperty);
        set => SetValue(OverflowTextFormatProperty, value);
    }

    private double Step => Math.Max(4, Size - Overlap);

    private List<object?> ResolveItems()
    {
        if (Items is null) return new List<object?>();

        var list = new List<object?>();
        foreach (object? item in Items) list.Add(item);
        return list;
    }

    private int VisibleCount(int total) => Math.Max(0, Math.Min(MaxVisible, total));

    private int OverflowCount(int total)
    {
        int visible = VisibleCount(total);
        return ShowOverflow && total > visible ? total - visible : 0;
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        List<object?> items = ResolveItems();
        int total = items.Count;
        if (total == 0) return new Size(0, Size);

        int visible = VisibleCount(total);
        int overflow = OverflowCount(total);
        int chips = visible + (overflow > 0 ? 1 : 0);
        if (chips == 0) return new Size(0, Size);

        double width = Size + (chips - 1) * Step;
        return new Size(width, Size);
    }

    protected override void OnRender(DrawingContext dc)
    {
        List<object?> items = ResolveItems();
        if (items.Count == 0) return;

        Brush ring = RingBrush ?? TryFindResource("CONCOST.Brush.Surface") as Brush ?? Brushes.White;
        Brush fallbackBackground = Background ?? TryFindResource("CONCOST.Brush.Primary") as Brush ?? Brushes.SteelBlue;
        Brush textBrush = Foreground ?? Brushes.White;
        double dpi = VisualTreeHelper.GetDpi(this).PixelsPerDip;

        var ringPen = new Pen(ring, 2);
        ringPen.Freeze();

        int visible = VisibleCount(items.Count);
        int overflow = OverflowCount(items.Count);

        for (int i = visible - 1; i >= 0; i--)
        {
            double x = i * Step;
            var center = new Point(CenterX(i), Size / 2);
            double radius = Size / 2;

            if (items[i] is ImageSource image)
            {
                dc.PushClip(new EllipseGeometry(center, radius, radius));
                dc.DrawImage(image, new Rect(x, 0, Size, Size));
                dc.Pop();
            }
            else
            {
                dc.DrawEllipse(fallbackBackground, null, center, radius, radius);
                DrawText(dc, InitialsOf(items[i]), center, textBrush, dpi);
            }

            dc.DrawEllipse(null, ringPen, center, radius, radius);
        }

        if (overflow > 0)
        {
            double x = visible * Step;
            var center = new Point(CenterX(visible), Size / 2);
            double radius = Size / 2;

            Brush overflowBrush = TryFindResource("CONCOST.Brush.SurfaceSecondary") as Brush
                                  ?? new SolidColorBrush(Color.FromRgb(0xEE, 0xF1, 0xF5));
            Brush overflowText = TryFindResource("CONCOST.Brush.TextSecondary") as Brush
                                 ?? new SolidColorBrush(Color.FromRgb(0x47, 0x54, 0x67));

            dc.DrawEllipse(overflowBrush, null, center, radius, radius);
            DrawText(dc, string.Format(CultureInfo.InvariantCulture, OverflowTextFormat, overflow), center, overflowText, dpi);
            dc.DrawEllipse(null, ringPen, center, radius, radius);
        }
    }

    /// <summary>
    /// Centre of the slice that stays visible: the left-most chip is fully visible,
    /// every later chip is partially covered by the one on its left.
    /// </summary>
    private double CenterX(int index)
    {
        if (index <= 0) return Size / 2;
        return index * Step + Size - Step / 2;
    }

    private void DrawText(DrawingContext dc, string text, Point center, Brush brush, double pixelsPerDip)
    {
        if (string.IsNullOrEmpty(text)) return;

        var formatted = new FormattedText(
            text,
            CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            new Typeface("Segoe UI"),
            FontSize,
            brush,
            pixelsPerDip);

        dc.DrawText(formatted, new Point(center.X - formatted.Width / 2, center.Y - formatted.Height / 2));
    }

    private static string InitialsOf(object? item)
    {
        string raw = item?.ToString() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(raw)) return "?";

        string[] parts = raw.Split(new[] { ' ', '-', '_' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 1)
            return parts[0].Length >= 2 ? parts[0].Substring(0, 2).ToUpperInvariant() : parts[0].ToUpperInvariant();

        return (parts[0].Substring(0, 1) + parts[parts.Length - 1].Substring(0, 1)).ToUpperInvariant();
    }
}
