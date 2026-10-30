using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace CONCOST.UI.Controls.Ribbon;

/// <summary>
/// The File button at the top-left of a ribbon: a coloured block that opens a two-column panel —
/// commands on the left, recently used documents on the right.
/// </summary>
/// <remarks>
/// This is the Office 2010 “menu panel” shape, not the full-window backstage of Office 2013+.
/// It is deliberately built on <see cref="RibbonDropDownButton"/> so command plumbing, Escape
/// handling and “close when an item is clicked” all come for free.
/// </remarks>
public class RibbonApplicationMenu : RibbonDropDownButton
{
    public static readonly DependencyProperty RecentItemsProperty =
        DependencyProperty.Register(nameof(RecentItems), typeof(IEnumerable), typeof(RibbonApplicationMenu),
            new FrameworkPropertyMetadata(null));

    public static readonly DependencyProperty RecentHeaderProperty =
        DependencyProperty.Register(nameof(RecentHeader), typeof(string), typeof(RibbonApplicationMenu),
            new FrameworkPropertyMetadata("Recent"));

    public static readonly DependencyProperty FooterItemsProperty =
        DependencyProperty.Register(nameof(FooterItems), typeof(IEnumerable), typeof(RibbonApplicationMenu),
            new FrameworkPropertyMetadata(null));

    static RibbonApplicationMenu()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(RibbonApplicationMenu),
            new FrameworkPropertyMetadata(typeof(RibbonApplicationMenu)));
    }

    public RibbonApplicationMenu()
    {
        Size = RibbonItemSize.Small;
        HorizontalContentAlignment = HorizontalAlignment.Left;
    }

    /// <summary>Right column: usually recent documents. Items can be any content.</summary>
    public IEnumerable? RecentItems
    {
        get => (IEnumerable?)GetValue(RecentItemsProperty);
        set => SetValue(RecentItemsProperty, value);
    }

    /// <summary>Caption above the right column.</summary>
    public string RecentHeader
    {
        get => (string)GetValue(RecentHeaderProperty);
        set => SetValue(RecentHeaderProperty, value);
    }

    /// <summary>Commands pinned to the bottom of the left column, e.g. Exit and Options.</summary>
    public IEnumerable? FooterItems
    {
        get => (IEnumerable?)GetValue(FooterItemsProperty);
        set => SetValue(FooterItemsProperty, value);
    }

    protected override void OnInitialized(EventArgs e)
    {
        base.OnInitialized(e);

        if (string.IsNullOrEmpty(Header as string)) Header = "File";
    }
}
