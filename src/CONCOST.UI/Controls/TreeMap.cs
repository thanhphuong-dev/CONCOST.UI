using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace CONCOST.UI.Controls;

/// <summary>One rectangle of a <see cref="TreeMap"/>: the item it stands for and where it was drawn.</summary>
public class TreeMapEntry
{
    internal TreeMapEntry(object item, string label, double value)
    {
        Item = item;
        Label = label;
        Value = value;
    }

    /// <summary>The source object.</summary>
    public object Item { get; }

    /// <summary>Label taken from <see cref="TreeMap.LabelPath"/>.</summary>
    public string Label { get; }

    /// <summary>Value taken from <see cref="TreeMap.ValuePath"/>.</summary>
    public double Value { get; }

    /// <summary>Share of the total, 0–1.</summary>
    public double Share { get; internal set; }

    /// <summary>Area allotted during layout, in square pixels.</summary>
    internal double Area { get; set; }

    /// <summary>Brush actually used to paint the rectangle.</summary>
    public Brush? Brush { get; internal set; }

    /// <summary>Where the rectangle was drawn, in device-independent pixels. Empty when it did not fit.</summary>
    public Rect Bounds { get; internal set; }
}

/// <summary>Carries the rectangle the user clicked.</summary>
public class TreeMapInvokedEventArgs : EventArgs
{
    public TreeMapInvokedEventArgs(TreeMapEntry entry) => Entry = entry;

    public TreeMapEntry Entry { get; }
}

/// <summary>
/// A squarified treemap: area proportional to value, rectangles packed so they stay close to square.
/// </summary>
/// <remarks>
/// <para>
/// Bind <see cref="ItemsSource"/> plus <see cref="ValuePath"/> / <see cref="LabelPath"/>. Values that
/// are zero or negative are dropped — a treemap cannot show "none of it", and a zero-area rectangle
/// only produces invisible slivers.
/// </para>
/// <para>
/// The layout is recomputed on resize and exposed through <see cref="Entries"/>, so a host can put
/// tooltips or hit-testing on the same rectangles the control drew.
/// </para>
/// </remarks>
public class TreeMap : FrameworkElement
{
    private readonly List<TreeMapEntry> _entries = new();
    private bool _isDirty = true;

    public TreeMap()
    {
        ClipToBounds = true;
        SizeChanged += (_, _) => Invalidate();
    }

    // ---------------------------------------------------------------- source

    public static readonly DependencyProperty ItemsSourceProperty =
        DependencyProperty.Register(nameof(ItemsSource), typeof(IEnumerable), typeof(TreeMap),
            new FrameworkPropertyMetadata(null, OnSourceChanged));

    public IEnumerable? ItemsSource
    {
        get => (IEnumerable?)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public static readonly DependencyProperty ValuePathProperty =
        DependencyProperty.Register(nameof(ValuePath), typeof(string), typeof(TreeMap),
            new FrameworkPropertyMetadata("Value", OnSourceChanged));

    /// <summary>Property that carries the weight of an item. Default <c>Value</c>.</summary>
    public string ValuePath
    {
        get => (string)GetValue(ValuePathProperty);
        set => SetValue(ValuePathProperty, value);
    }

    public static readonly DependencyProperty LabelPathProperty =
        DependencyProperty.Register(nameof(LabelPath), typeof(string), typeof(TreeMap),
            new FrameworkPropertyMetadata("Label", OnSourceChanged));

    /// <summary>Property that carries the caption. Default <c>Label</c>; leave empty to use <c>ToString()</c>.</summary>
    public string LabelPath
    {
        get => (string)GetValue(LabelPathProperty);
        set => SetValue(LabelPathProperty, value);
    }

    // ---------------------------------------------------------------- paint

    public static readonly DependencyProperty PaletteProperty =
        DependencyProperty.Register(nameof(Palette), typeof(IEnumerable), typeof(TreeMap),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Brushes cycled across the rectangles. Empty falls back to <see cref="Fill"/>.</summary>
    public IEnumerable? Palette
    {
        get => (IEnumerable?)GetValue(PaletteProperty);
        set => SetValue(PaletteProperty, value);
    }

    public static readonly DependencyProperty FillProperty =
        DependencyProperty.Register(nameof(Fill), typeof(Brush), typeof(TreeMap),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Single brush used when <see cref="Palette"/> is empty. Empty takes the theme accent.</summary>
    public Brush? Fill
    {
        get => (Brush?)GetValue(FillProperty);
        set => SetValue(FillProperty, value);
    }

    public static readonly DependencyProperty GapProperty =
        DependencyProperty.Register(nameof(Gap), typeof(double), typeof(TreeMap),
            new FrameworkPropertyMetadata(3.0, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Space between rectangles, in pixels. Default 3.</summary>
    public double Gap
    {
        get => (double)GetValue(GapProperty);
        set => SetValue(GapProperty, value);
    }

    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.Register(nameof(CornerRadius), typeof(double), typeof(TreeMap),
            new FrameworkPropertyMetadata(4.0, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Corner radius of each rectangle. Default 4.</summary>
    public double CornerRadius
    {
        get => (double)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    // ---------------------------------------------------------------- labels

    public static readonly DependencyProperty ShowLabelsProperty =
        DependencyProperty.Register(nameof(ShowLabels), typeof(bool), typeof(TreeMap),
            new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Draw the caption inside each rectangle. Default <c>true</c>.</summary>
    public bool ShowLabels
    {
        get => (bool)GetValue(ShowLabelsProperty);
        set => SetValue(ShowLabelsProperty, value);
    }

    public static readonly DependencyProperty ShowValuesProperty =
        DependencyProperty.Register(nameof(ShowValues), typeof(bool), typeof(TreeMap),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Draw the value under the caption. Default <c>false</c>.</summary>
    public bool ShowValues
    {
        get => (bool)GetValue(ShowValuesProperty);
        set => SetValue(ShowValuesProperty, value);
    }

    public static readonly DependencyProperty ValueFormatProperty =
        DependencyProperty.Register(nameof(ValueFormat), typeof(string), typeof(TreeMap),
            new FrameworkPropertyMetadata("N0", FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Numeric format for <see cref="ShowValues"/>. Default <c>N0</c>.</summary>
    public string ValueFormat
    {
        get => (string)GetValue(ValueFormatProperty);
        set => SetValue(ValueFormatProperty, value);
    }

    public static readonly DependencyProperty LabelForegroundProperty =
        DependencyProperty.Register(nameof(LabelForeground), typeof(Brush), typeof(TreeMap),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Caption colour. Empty uses white, which reads on every palette colour.</summary>
    public Brush? LabelForeground
    {
        get => (Brush?)GetValue(LabelForegroundProperty);
        set => SetValue(LabelForegroundProperty, value);
    }

    public static readonly DependencyProperty FontSizeProperty =
        DependencyProperty.Register(nameof(FontSize), typeof(double), typeof(TreeMap),
            new FrameworkPropertyMetadata(12.0, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Caption size. Default 12.</summary>
    public double FontSize
    {
        get => (double)GetValue(FontSizeProperty);
        set => SetValue(FontSizeProperty, value);
    }

    /// <summary>
    /// The rectangles of the last layout. Reading it before the control has a size (or before the
    /// first render) lays them out on demand, so a host can hit-test without waiting for a paint.
    /// </summary>
    public IReadOnlyList<TreeMapEntry> Entries
    {
        get
        {
            if (_isDirty && ActualWidth > 0 && ActualHeight > 0)
            {
                BuildEntries(new Rect(0, 0, ActualWidth, ActualHeight));
            }

            return _entries;
        }
    }

    /// <summary>Raised when the user clicks a rectangle.</summary>
    public event EventHandler<TreeMapInvokedEventArgs>? ItemInvoked;

    /// <summary>Returns the rectangle under a point, or <c>null</c>.</summary>
    public TreeMapEntry? HitTest(Point point)
    {
        for (int i = 0; i < _entries.Count; i++)
        {
            if (_entries[i].Bounds.Contains(point)) return _entries[i];
        }
        return null;
    }

    /// <summary>Recomputes the layout without waiting for the next render pass.</summary>
    public void Invalidate()
    {
        _isDirty = true;
        InvalidateVisual();
    }

    private static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TreeMap map) map.Invalidate();
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        double width = double.IsInfinity(availableSize.Width) ? 0 : availableSize.Width;
        double height = double.IsInfinity(availableSize.Height) ? 0 : availableSize.Height;
        return new Size(width, height);
    }

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);

        var bounds = new Rect(0, 0, ActualWidth, ActualHeight);
        if (bounds.Width < 4 || bounds.Height < 4) return;

        if (_isDirty) BuildEntries(bounds);
        if (_entries.Count == 0) return;

        var labelBrush = LabelForeground
                         ?? TryFindResource("CONCOST.Brush.White") as Brush
                         ?? Brushes.White;

        double dpi = VisualTreeHelper.GetDpi(this).PixelsPerDip;
        double radius = Math.Max(0, CornerRadius);
        double halfGap = Math.Max(0, Gap) / 2;

        for (int i = 0; i < _entries.Count; i++)
        {
            var entry = _entries[i];
            var rect = entry.Bounds;
            if (rect.Width <= 0 || rect.Height <= 0) continue;

            var cell = halfGap > 0 ? Rect.Inflate(rect, -halfGap, -halfGap) : rect;
            if (cell.Width <= 0 || cell.Height <= 0) continue;

            var brush = entry.Brush ?? Fill ?? TryFindResource("CONCOST.Brush.Primary") as Brush ?? Brushes.SteelBlue;
            dc.DrawRoundedRectangle(brush, null, cell, radius, radius);

            if (!ShowLabels && !ShowValues) continue;
            if (cell.Width < 34 || cell.Height < 20) continue;

            DrawLabel(dc, entry, cell, labelBrush, dpi);
        }
    }

    private void DrawLabel(DrawingContext dc, TreeMapEntry entry, Rect cell, Brush brush, double dpi)
    {
        double padding = 6;
        double maxWidth = cell.Width - padding * 2;
        if (maxWidth <= 8) return;

        double y = cell.Y + padding;

        if (ShowLabels && !string.IsNullOrEmpty(entry.Label))
        {
            var text = new FormattedText(entry.Label, CultureInfo.CurrentUICulture,
                FlowDirection.LeftToRight, new Typeface("Segoe UI"), FontSize, brush, dpi)
            {
                MaxTextWidth = maxWidth,
                MaxLineCount = 1,
                Trimming = TextTrimming.CharacterEllipsis
            };

            if (y + text.Height <= cell.Bottom - 2)
            {
                dc.DrawText(text, new Point(cell.X + padding, y));
                y += text.Height + 2;
            }
        }

        if (!ShowValues) return;

        string formatted;
        try
        {
            formatted = entry.Value.ToString(ValueFormat, CultureInfo.CurrentCulture);
        }
        catch (FormatException)
        {
            formatted = entry.Value.ToString(CultureInfo.CurrentCulture);
        }

        var value = new FormattedText(formatted, CultureInfo.CurrentUICulture,
            FlowDirection.LeftToRight, new Typeface("Segoe UI"), Math.Max(9, FontSize - 2), brush, dpi)
        {
            MaxTextWidth = maxWidth,
            MaxLineCount = 1,
            Trimming = TextTrimming.CharacterEllipsis
        };

        if (y + value.Height <= cell.Bottom - 2) dc.DrawText(value, new Point(cell.X + padding, y));
    }

    private void BuildEntries(Rect bounds)
    {
        _isDirty = false;
        _entries.Clear();

        if (ItemsSource is null) return;

        var raw = new List<TreeMapEntry>();
        double total = 0;

        foreach (var item in ItemsSource)
        {
            if (item is null) continue;

            double value = ReadDouble(item, ValuePath);
            if (value <= 0) continue;

            raw.Add(new TreeMapEntry(item, ReadLabel(item), value));
            total += value;
        }

        if (raw.Count == 0 || total <= 0) return;

        // Biggest first: squarified packing degrades badly when a large cell is placed last.
        raw.Sort((a, b) => b.Value.CompareTo(a.Value));

        var palette = ReadPalette();
        double area = bounds.Width * bounds.Height;

        var work = new List<TreeMapEntry>(raw.Count);
        for (int i = 0; i < raw.Count; i++)
        {
            var entry = raw[i];
            entry.Share = entry.Value / total;
            entry.Area = entry.Share * area;
            entry.Brush = palette.Count > 0 ? palette[i % palette.Count] : null;
            work.Add(entry);
        }

        Squarify(work, bounds);
        _entries.AddRange(raw);
    }

    private static void Squarify(List<TreeMapEntry> items, Rect rect)
    {
        int start = 0;

        while (start < items.Count)
        {
            if (rect.Width < 0.5 || rect.Height < 0.5) break;

            bool horizontal = rect.Width >= rect.Height;
            double side = horizontal ? rect.Width : rect.Height;

            double rowArea = items[start].Area;
            double worst = RowWorst(items, start, start + 1, side);
            int end = start + 1;

            while (end < items.Count)
            {
                double candidateArea = rowArea + items[end].Area;
                double candidateWorst = RowWorst(items, start, end + 1, side);

                if (candidateWorst > worst) break;

                rowArea = candidateArea;
                worst = candidateWorst;
                end++;
            }

            double thickness = rowArea / side;
            double offset = 0;

            for (int i = start; i < end; i++)
            {
                double share = rowArea > 0 ? items[i].Area / rowArea : 0;

                if (horizontal)
                {
                    double width = share * rect.Width;
                    items[i].Bounds = new Rect(rect.X + offset, rect.Y, width, thickness);
                    offset += width;
                }
                else
                {
                    double height = share * rect.Height;
                    items[i].Bounds = new Rect(rect.X, rect.Y + offset, thickness, height);
                    offset += height;
                }
            }

            rect = horizontal
                ? new Rect(rect.X, rect.Y + thickness, rect.Width, Math.Max(0, rect.Height - thickness))
                : new Rect(rect.X + thickness, rect.Y, Math.Max(0, rect.Width - thickness), rect.Height);

            start = end;
        }
    }

    /// <summary>Worst width/height ratio in a candidate row — the number squarify tries to minimise.</summary>
    private static double RowWorst(List<TreeMapEntry> items, int start, int end, double side)
    {
        double rowArea = 0;
        for (int i = start; i < end; i++) rowArea += items[i].Area;

        if (rowArea <= 0 || side <= 0) return double.MaxValue;

        double thickness = rowArea / side;
        if (thickness <= 0) return double.MaxValue;

        double worst = 0;
        for (int i = start; i < end; i++)
        {
            double length = items[i].Area / thickness;
            if (length <= 0) continue;

            double ratio = Math.Max(length / thickness, thickness / length);
            if (ratio > worst) worst = ratio;
        }

        return worst;
    }

    private List<Brush> ReadPalette()
    {
        var list = new List<Brush>();
        if (Palette is null) return list;

        foreach (var item in Palette)
        {
            if (item is Brush brush) list.Add(brush);
        }

        return list;
    }

    private double ReadDouble(object item, string path)
    {
        if (string.IsNullOrEmpty(path)) return 0;

        var descriptor = TypeDescriptor.GetProperties(item)[path];
        if (descriptor?.GetValue(item) is null) return 0;

        try
        {
            return Convert.ToDouble(descriptor.GetValue(item), CultureInfo.InvariantCulture);
        }
        catch (Exception ex) when (ex is InvalidCastException or FormatException or OverflowException)
        {
            return 0;
        }
    }

    private string ReadLabel(object item)
    {
        if (string.IsNullOrEmpty(LabelPath)) return item.ToString() ?? string.Empty;

        var descriptor = TypeDescriptor.GetProperties(item)[LabelPath];
        var value = descriptor?.GetValue(item);
        return value?.ToString() ?? item.ToString() ?? string.Empty;
    }

    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonUp(e);

        var hit = HitTest(e.GetPosition(this));
        if (hit is null) return;

        ItemInvoked?.Invoke(this, new TreeMapInvokedEventArgs(hit));
    }
}
