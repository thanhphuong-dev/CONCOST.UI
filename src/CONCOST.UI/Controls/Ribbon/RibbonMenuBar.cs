using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CONCOST.UI.Controls.Ribbon;

/// <summary>
/// A horizontal menu bar that sits above or below a ribbon (or stands alone).
/// Each top-level <see cref="RibbonMenuBarItem"/> opens a dropdown on click;
/// hovering over another item while a menu is open switches to the new one.
/// </summary>
public class RibbonMenuBar : ItemsControl
{
    public static readonly DependencyProperty ShowDividerProperty =
        DependencyProperty.Register(nameof(ShowDivider), typeof(bool), typeof(RibbonMenuBar),
            new FrameworkPropertyMetadata(true));

    static RibbonMenuBar()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(RibbonMenuBar),
            new FrameworkPropertyMetadata(typeof(RibbonMenuBar)));
    }

    public RibbonMenuBar()
    {
        Focusable = true;
        IsTabStop = false;

        // Close everything when a menu item is invoked.
        AddHandler(RibbonMenuItem.ClickEvent, new RoutedEventHandler((_, _) => CloseAll()));
    }

    /// <summary>
    /// Draws the hairline under the bar. The bar itself always spans the full ribbon width, so this
    /// is the line that separates it from the tabs.
    /// </summary>
    public bool ShowDivider
    {
        get => (bool)GetValue(ShowDividerProperty);
        set => SetValue(ShowDividerProperty, value);
    }

    /// <summary>True when any top-level item currently has its dropdown open.</summary>
    public bool IsAnyOpen
    {
        get
        {
            foreach (object? entry in Items)
            {
                if (entry is RibbonMenuBarItem item && item.IsSubmenuOpen) return true;
            }
            return false;
        }
    }

    /// <summary>Closes every open dropdown in this bar.</summary>
    public void CloseAll()
    {
        foreach (object? entry in Items)
        {
            if (entry is RibbonMenuBarItem item) item.Close();
        }
    }

    internal void OnItemOpened(RibbonMenuBarItem opened)
    {
        // Keep only the newly-opened item expanded.
        foreach (object? entry in Items)
        {
            if (entry is RibbonMenuBarItem item && !ReferenceEquals(item, opened)) item.Close();
        }
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (e.Key == Key.Escape)
        {
            CloseAll();
            e.Handled = true;
            return;
        }

        int focused = FocusedIndex();
        if (focused < 0) return;

        if (e.Key == Key.Left)
        {
            MoveFocus(focused, -1);
            e.Handled = true;
        }
        else if (e.Key == Key.Right)
        {
            MoveFocus(focused, 1);
            e.Handled = true;
        }
    }

    private int FocusedIndex()
    {
        int i = 0;
        foreach (object? entry in Items)
        {
            if (entry is RibbonMenuBarItem item && item.IsFocused) return i;
            i++;
        }
        return -1;
    }

    private void MoveFocus(int from, int delta)
    {
        int count = Items.Count;
        if (count == 0) return;

        int next = from;
        for (int attempt = 0; attempt < count; attempt++)
        {
            next = (next + delta + count) % count;
            if (Items[next] is RibbonMenuBarItem item)
            {
                item.Focus();
                // If a menu was already open, open the new one too.
                if (IsAnyOpen) item.Open();
                break;
            }
        }
    }

    protected override bool IsItemItsOwnContainerOverride(object item) => item is RibbonMenuBarItem;

    protected override DependencyObject GetContainerForItemOverride() => new RibbonMenuBarItem();
}
