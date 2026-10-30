using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CONCOST.UI.Controls.Ribbon;

/// <summary>
/// A ribbon button that only opens a menu — it has no primary action of its own.
/// The popup closes on Escape, on a click outside, or as soon as a menu item is invoked.
/// </summary>
public class RibbonDropDownButton : RibbonItem
{
    private readonly ObservableCollection<object> _items = new();

    public static readonly DependencyProperty IsOpenProperty =
        DependencyProperty.Register(nameof(IsOpen), typeof(bool), typeof(RibbonDropDownButton),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    static RibbonDropDownButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(RibbonDropDownButton),
            new FrameworkPropertyMetadata(typeof(RibbonDropDownButton)));
    }

    public RibbonDropDownButton()
    {
        Cursor = Cursors.Hand;
        AddHandler(RibbonMenuItem.ClickEvent, new RoutedEventHandler((_, _) => IsOpen = false));
    }

    /// <summary>Menu content: <see cref="RibbonMenuItem"/> and <see cref="RibbonMenuSeparator"/>.</summary>
    public ObservableCollection<object> Items => _items;

    /// <summary>Whether the dropdown is showing.</summary>
    public bool IsOpen
    {
        get => (bool)GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    /// <summary>Opens the menu, which is this control's only action.</summary>
    public override void PerformClick()
    {
        if (!IsEnabled) return;

        Focus();
        IsOpen = true;
    }

    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonUp(e);
        if (!IsEnabled) return;

        Focus();
        IsOpen = !IsOpen;
        e.Handled = true;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (!IsEnabled) return;

        if (e.Key is Key.Down or Key.Enter or Key.Space)
        {
            IsOpen = true;
            e.Handled = true;
        }
        else if (e.Key == Key.Escape && IsOpen)
        {
            IsOpen = false;
            e.Handled = true;
        }
    }
}
