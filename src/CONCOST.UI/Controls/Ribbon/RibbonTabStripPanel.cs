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
    private Ribbon? _owner;

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
        double maxHeight = 0;
        double width = 0;

        var constraints = new Size(double.PositiveInfinity, availableSize.Height);

        for (int i = 0; i < InternalChildren.Count; i++)
        {
            UIElement child = InternalChildren[i];
            child.Measure(constraints);
            width += child.DesiredSize.Width;
            maxHeight = Math.Max(maxHeight, child.DesiredSize.Height);
        }

        _measuredWidth = width;
        return new Size(width, maxHeight);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        int count = InternalChildren.Count;
        if (_tabRects.Length < count) _tabRects = new Rect[count];

        double height = Math.Max(0, finalSize.Height);
        double x = 0;

        for (int i = 0; i < count; i++)
        {
            UIElement child = InternalChildren[i];
            var slot = new Rect(x, 0, child.DesiredSize.Width, height);

            child.Arrange(slot);
            _tabRects[i] = slot;
            x += slot.Width;
        }

        return new Size(Math.Max(_measuredWidth, x), finalSize.Height);
    }

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);
    }

    /// <summary>
    /// True while the tabs no longer fit. The band is a merged run drawn across several tabs, so a
    /// clipped run shows up as a stray coloured block; the ribbon knows when that happens and the
    /// band steps aside until the strip fits again.
    /// </summary>
    private bool IsStripClipped()
    {
        _owner ??= FindOwner();
        return _owner?.IsTabStripOverflowing == true;
    }

    private Ribbon? FindOwner()
    {
        DependencyObject? node = this;

        while (node != null)
        {
            if (node is Ribbon ribbon) return ribbon;
            node = System.Windows.Media.VisualTreeHelper.GetParent(node);
        }

        return null;
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
