using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CONCOST.UI.Controls.Ribbon;

/// <summary>
/// One top-level entry of a <see cref="RibbonMenuBar"/>. It has a header label and a dropdown
/// that opens beneath it, populated with <see cref="RibbonMenuItem"/> and
/// <see cref="RibbonMenuSeparator"/> instances.
/// </summary>
public class RibbonMenuBarItem : HeaderedItemsControl
{
    public static readonly DependencyProperty IsSubmenuOpenProperty =
        DependencyProperty.Register(nameof(IsSubmenuOpen), typeof(bool), typeof(RibbonMenuBarItem),
            new FrameworkPropertyMetadata(false, OnIsSubmenuOpenChanged));

    static RibbonMenuBarItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(RibbonMenuBarItem),
            new FrameworkPropertyMetadata(typeof(RibbonMenuBarItem)));
    }

    public RibbonMenuBarItem()
    {
        Focusable = true;
        IsTabStop = true;
    }

    /// <summary>True while the dropdown beneath this item is showing.</summary>
    public bool IsSubmenuOpen
    {
        get => (bool)GetValue(IsSubmenuOpenProperty);
        set => SetValue(IsSubmenuOpenProperty, value);
    }

    /// <summary>Opens the dropdown and focuses this item.</summary>
    public void Open()
    {
        Focus();
        IsSubmenuOpen = true;
    }

    /// <summary>Closes the dropdown.</summary>
    public void Close() => IsSubmenuOpen = false;

    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonUp(e);
        if (!IsEnabled) return;

        // Toggle: if already open, close; otherwise open (and the bar will close siblings).
        if (IsSubmenuOpen) Close();
        else Open();

        e.Handled = true;
    }

    protected override void OnMouseEnter(MouseEventArgs e)
    {
        base.OnMouseEnter(e);
        if (!IsEnabled) return;

        // If the bar has another item open, switch to this one.
        if (GetBar()?.IsAnyOpen == true) Open();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (!IsEnabled) return;

        if (e.Key is Key.Down or Key.Enter or Key.Space)
        {
            Open();
            e.Handled = true;
        }
        else if (e.Key == Key.Escape && IsSubmenuOpen)
        {
            Close();
            e.Handled = true;
        }
    }

    private static void OnIsSubmenuOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not RibbonMenuBarItem item) return;

        // Notify the parent bar so it can close sibling items.
        if (e.NewValue is true)
            item.GetBar()?.OnItemOpened(item);
    }

    private RibbonMenuBar? GetBar() => ItemsControl.ItemsControlFromItemContainer(this) as RibbonMenuBar;
}
