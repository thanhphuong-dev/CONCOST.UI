using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace CONCOST.UI.Controls.Ribbon;

/// <summary>
/// A grid of pictures with a selection, the ribbon's answer to a style / colour palette.
/// <para>
/// The tiles visible on the ribbon are the first rows that fit the group cell; the chevron opens the
/// same list in a larger pop-up so nothing is hidden. Both views share one
/// <see cref="SelectedItem"/>, so picking a tile in either place updates the other.
/// </para>
/// <para>
/// Items are <see cref="RibbonGalleryItem"/> instances, or any object when you supply
/// <see cref="ItemTemplate"/>. The gallery is usually <see cref="RibbonItemSize.Large"/> and
/// <see cref="RibbonItem.ColumnSpan"/> = 2 so the tiles have room.
/// </para>
/// </summary>
public class RibbonGallery : RibbonItem
{
    public static readonly DependencyProperty ItemsSourceProperty =
        DependencyProperty.Register(nameof(ItemsSource), typeof(IEnumerable), typeof(RibbonGallery),
            new FrameworkPropertyMetadata(null));

    public static readonly DependencyProperty ItemTemplateProperty =
        DependencyProperty.Register(nameof(ItemTemplate), typeof(DataTemplate), typeof(RibbonGallery),
            new FrameworkPropertyMetadata(null));

    public static readonly DependencyProperty ExpandedItemTemplateProperty =
        DependencyProperty.Register(nameof(ExpandedItemTemplate), typeof(DataTemplate), typeof(RibbonGallery),
            new FrameworkPropertyMetadata(null));

    public static readonly DependencyProperty SelectedItemProperty =
        DependencyProperty.Register(nameof(SelectedItem), typeof(object), typeof(RibbonGallery),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnSelectedItemChanged));

    public static readonly DependencyProperty IsDropDownOpenProperty =
        DependencyProperty.Register(nameof(IsDropDownOpen), typeof(bool), typeof(RibbonGallery),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public static readonly DependencyProperty ShowExpandButtonProperty =
        DependencyProperty.Register(nameof(ShowExpandButton), typeof(bool), typeof(RibbonGallery),
            new FrameworkPropertyMetadata(true));

    public static readonly DependencyProperty ItemWidthProperty =
        DependencyProperty.Register(nameof(ItemWidth), typeof(double), typeof(RibbonGallery),
            new FrameworkPropertyMetadata(42.0, FrameworkPropertyMetadataOptions.AffectsMeasure));

    public static readonly DependencyProperty ItemHeightProperty =
        DependencyProperty.Register(nameof(ItemHeight), typeof(double), typeof(RibbonGallery),
            new FrameworkPropertyMetadata(42.0, FrameworkPropertyMetadataOptions.AffectsMeasure));

    public static readonly DependencyProperty MaxDropDownHeightProperty =
        DependencyProperty.Register(nameof(MaxDropDownHeight), typeof(double), typeof(RibbonGallery),
            new FrameworkPropertyMetadata(340.0));

    public static readonly DependencyProperty MaxDropDownWidthProperty =
        DependencyProperty.Register(nameof(MaxDropDownWidth), typeof(double), typeof(RibbonGallery),
            new FrameworkPropertyMetadata(420.0));

    static RibbonGallery()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(RibbonGallery),
            new FrameworkPropertyMetadata(typeof(RibbonGallery)));
    }

    public RibbonGallery()
    {
        // The inner list owns the focus and the arrow keys.
        Focusable = false;
        IsTabStop = false;
    }

    /// <summary>Raised when <see cref="SelectedItem"/> changes, by click or by keyboard.</summary>
    public event EventHandler? SelectionChanged;

    /// <summary>Tiles offered by the gallery.</summary>
    public IEnumerable? ItemsSource
    {
        get => (IEnumerable?)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    /// <summary>Template for one tile on the ribbon grid. Defaults to the picture alone.</summary>
    public DataTemplate? ItemTemplate
    {
        get => (DataTemplate?)GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }

    /// <summary>
    /// Template for one tile inside the expanded panel. Falls back to <see cref="ItemTemplate"/> when
    /// left null. The theme uses it to put the caption back: the grid shows bare swatches, where a
    /// label would be trimmed away, and the panel has room for the name under each tile.
    /// </summary>
    public DataTemplate? ExpandedItemTemplate
    {
        get => (DataTemplate?)GetValue(ExpandedItemTemplateProperty);
        set => SetValue(ExpandedItemTemplateProperty, value);
    }

    /// <summary>Currently selected tile.</summary>
    public object? SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    /// <summary>True while the expanded panel is showing.</summary>
    public bool IsDropDownOpen
    {
        get => (bool)GetValue(IsDropDownOpenProperty);
        set => SetValue(IsDropDownOpenProperty, value);
    }

    /// <summary>Shows the chevron that opens the full panel. Turn it off for a fixed grid.</summary>
    public bool ShowExpandButton
    {
        get => (bool)GetValue(ShowExpandButtonProperty);
        set => SetValue(ShowExpandButtonProperty, value);
    }

    /// <summary>Width of one tile.</summary>
    public double ItemWidth
    {
        get => (double)GetValue(ItemWidthProperty);
        set => SetValue(ItemWidthProperty, value);
    }

    /// <summary>Height of one tile.</summary>
    public double ItemHeight
    {
        get => (double)GetValue(ItemHeightProperty);
        set => SetValue(ItemHeightProperty, value);
    }

    /// <summary>Height cap of the expanded panel, before it starts scrolling.</summary>
    public double MaxDropDownHeight
    {
        get => (double)GetValue(MaxDropDownHeightProperty);
        set => SetValue(MaxDropDownHeightProperty, value);
    }

    /// <summary>Width cap of the expanded panel; tiles wrap inside it.</summary>
    public double MaxDropDownWidth
    {
        get => (double)GetValue(MaxDropDownWidthProperty);
        set => SetValue(MaxDropDownWidthProperty, value);
    }

    protected override System.Windows.Automation.Peers.AutomationPeer OnCreateAutomationPeer() =>
        new RibbonGalleryAutomationPeer(this);

    /// <summary>Opens the expanded panel — the gallery has no single command to run.</summary>
    public override void PerformClick()
    {
        if (!IsEnabled) return;

        IsDropDownOpen = true;
    }

    /// <summary>Caption of a tile, used by the collapsed-group menu.</summary>
    internal static string LabelFor(object? item) => item switch
    {
        RibbonGalleryItem tile => tile.Header,
        null => string.Empty,
        _ => item.ToString() ?? string.Empty
    };

    private static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not RibbonGallery gallery) return;

        // Picking a tile is the end of the interaction: close the panel so the drawing is visible.
        if (gallery.IsDropDownOpen) gallery.SetCurrentValue(IsDropDownOpenProperty, false);

        gallery.SelectionChanged?.Invoke(gallery, EventArgs.Empty);
    }
}
