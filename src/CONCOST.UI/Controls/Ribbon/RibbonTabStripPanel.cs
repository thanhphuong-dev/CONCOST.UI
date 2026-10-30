using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CONCOST.UI.Controls.Ribbon;

/// <summary>
/// Lays out the ribbon's tab strip in two rows: a thin coloured band on top that names the
/// contextual groups, and the tabs themselves underneath.
/// </summary>
/// <remarks>
/// Consecutive tabs that share a <see cref="RibbonTabItem.ContextualGroup"/> are merged into one
/// band, exactly the way Revit and AutoCAD label “Modify | Walls” above the tabs they introduce.
/// The band is painted in <see cref="OnRender"/> rather than with extra visual children, because an
/// <c>ItemsPanel</c> may only contain item containers.
/// </remarks>
public class RibbonTabStripPanel : Panel
{
    public static readonly DependencyProperty BandHeightProperty =
        DependencyProperty.Register(nameof(BandHeight), typeof(double), typeof(RibbonTabStripPanel),
            new FrameworkPropertyMetadata(15.0, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty FontFamilyProperty =
        DependencyProperty.Register(nameof(FontFamily), typeof(FontFamily), typeof(RibbonTabStripPanel),
            new FrameworkPropertyMetadata(new FontFamily("Segoe UI"), FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty FontSizeProperty =
        DependencyProperty.Register(nameof(FontSize), typeof(double), typeof(RibbonTabStripPanel),
            new FrameworkPropertyMetadata(9.5, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

    private Rect[] _tabRects = new Rect[0];
    private double _measuredWidth;

    /// <summary>Height of the contextual band. Zero hides it entirely.</summary>
    public double BandHeight
    {
        get => (double)GetValue(BandHeightProperty);
        set => SetValue(BandHeightProperty, value);
    }

    /// <summary>Font used for the band captions.</summary>
    public FontFamily FontFamily
    {
        get => (FontFamily)GetValue(FontFamilyProperty);
        set => SetValue(FontFamilyProperty, value);
    }

    /// <summary>Size used for the band captions.</summary>
    public double FontSize
    {
        get => (double)GetValue(FontSizeProperty);
        set => SetValue(FontSizeProperty, value);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        double band = HasContextualBand() ? BandHeight : 0;
        double maxHeight = 0;
        double width = 0;

        var constraints = new Size(double.PositiveInfinity,
            double.IsInfinity(availableSize.Height) ? double.PositiveInfinity : Math.Max(0, availableSize.Height - band));

        foreach (UIElement child in InternalChildren)
        {
            child.Measure(constraints);
            width += child.DesiredSize.Width;
            maxHeight = Math.Max(maxHeight, child.DesiredSize.Height);
        }

        _measuredWidth = width;
        return new Size(width, band + maxHeight);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        int count = InternalChildren.Count;
        if (_tabRects.Length < count) _tabRects = new Rect[count];

        double band = HasContextualBand() ? BandHeight : 0;
        double height = Math.Max(0, finalSize.Height - band);
        double x = 0;

        for (int i = 0; i < count; i++)
        {
            UIElement child = InternalChildren[i];
            var slot = new Rect(x, band, child.DesiredSize.Width, height);

            child.Arrange(slot);
            _tabRects[i] = slot;
            x += slot.Width;
        }

        return new Size(Math.Max(_measuredWidth, x), finalSize.Height);
    }

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);

        int count = InternalChildren.Count;
        if (count == 0 || _tabRects.Length < count) return;

        Brush? fallback = TryFindResource("CONCOST.Brush.Primary") as Brush ?? Brushes.SteelBlue;

        int index = 0;
        while (index < count)
        {
            RibbonTabItem? tab = TabAt(index);
            string group = tab?.ContextualGroup ?? string.Empty;

            if (group.Length == 0)
            {
                index++;
                continue;
            }

            Brush accent = tab!.ContextualColor ?? fallback;
            double left = _tabRects[index].X;
            double right = _tabRects[index].Right;

            index++;

            while (index < count && string.Equals(TabAt(index)?.ContextualGroup, group, StringComparison.Ordinal))
            {
                right = _tabRects[index].Right;
                index++;
            }

            DrawBand(dc, left, right, group, accent);
        }
    }

    private void DrawBand(DrawingContext dc, double left, double right, string caption, Brush accent)
    {
        double height = BandHeight;
        if (height <= 0 || right - left < 4) return;

        const double radius = 3;
        var geometry = new StreamGeometry();

        using (StreamGeometryContext ctx = geometry.Open())
        {
            ctx.BeginFigure(new Point(left, height), isFilled: true, isClosed: true);
            ctx.LineTo(new Point(left, radius), true, false);
            ctx.ArcTo(new Point(left + radius, 0), new Size(radius, radius), 0, false, SweepDirection.Clockwise, true, false);
            ctx.LineTo(new Point(right - radius, 0), true, false);
            ctx.ArcTo(new Point(right, radius), new Size(radius, radius), 0, false, SweepDirection.Clockwise, true, false);
            ctx.LineTo(new Point(right, height), true, false);
        }

        geometry.Freeze();
        dc.DrawGeometry(accent, null, geometry);

        var text = new FormattedText(
            caption,
            CultureInfo.CurrentUICulture,
            FlowDirection.LeftToRight,
            new Typeface(FontFamily, FontStyles.Normal, FontWeights.SemiBold, FontStretches.Normal),
            FontSize,
            Brushes.White,
            VisualTreeHelper.GetDpi(this).PixelsPerDip)
        {
            MaxTextWidth = Math.Max(0, right - left - 8),
            MaxLineCount = 1,
            Trimming = TextTrimming.CharacterEllipsis
        };

        double x = left + Math.Max(4, (right - left - text.WidthIncludingTrailingWhitespace) / 2);
        double y = Math.Max(0, (height - text.Height) / 2);

        dc.DrawText(text, new Point(x, y));
    }

    private bool HasContextualBand()
    {
        if (BandHeight <= 0) return false;

        for (int i = 0; i < InternalChildren.Count; i++)
        {
            if (!string.IsNullOrEmpty(TabAt(i)?.ContextualGroup)) return true;
        }

        return false;
    }

    private RibbonTabItem? TabAt(int index)
    {
        if (index < 0 || index >= InternalChildren.Count) return null;

        return InternalChildren[index] switch
        {
            RibbonTabItem tab => tab,
            ContentControl { Content: RibbonTabItem inner } => inner,
            _ => null
        };
    }}
