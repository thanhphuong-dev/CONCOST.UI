using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CONCOST.UI.Controls.Ribbon;

/// <summary>
/// One row of a <see cref="RibbonMenu"/>. It can run a command, hold a nested submenu, or both.
/// Invoking it raises <see cref="ClickEvent"/>, which bubbles up to the owning dropdown so the
/// popup can close itself.
/// </summary>
public class RibbonMenuItem : HeaderedItemsControl
{
    public static readonly RoutedEvent ClickEvent =
        EventManager.RegisterRoutedEvent(nameof(Click), RoutingStrategy.Bubble,
            typeof(RoutedEventHandler), typeof(RibbonMenuItem));

    public static readonly DependencyProperty CommandProperty =
        DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(RibbonMenuItem),
            new FrameworkPropertyMetadata(null, OnCommandChanged));

    public static readonly DependencyProperty CommandParameterProperty =
        DependencyProperty.Register(nameof(CommandParameter), typeof(object), typeof(RibbonMenuItem),
            new FrameworkPropertyMetadata(null));

    public static readonly DependencyProperty IconProperty =
        DependencyProperty.Register(nameof(Icon), typeof(ImageSource), typeof(RibbonMenuItem),
            new FrameworkPropertyMetadata(null));

    public static readonly DependencyProperty InputGestureTextProperty =
        DependencyProperty.Register(nameof(InputGestureText), typeof(string), typeof(RibbonMenuItem),
            new FrameworkPropertyMetadata(string.Empty));

    public static readonly DependencyProperty IsSubmenuOpenProperty =
        DependencyProperty.Register(nameof(IsSubmenuOpen), typeof(bool), typeof(RibbonMenuItem),
            new FrameworkPropertyMetadata(false));

    public static readonly DependencyProperty IsCheckableProperty =
        DependencyProperty.Register(nameof(IsCheckable), typeof(bool), typeof(RibbonMenuItem),
            new FrameworkPropertyMetadata(false));

    public static readonly DependencyProperty IsCheckedProperty =
        DependencyProperty.Register(nameof(IsChecked), typeof(bool), typeof(RibbonMenuItem),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    static RibbonMenuItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(RibbonMenuItem),
            new FrameworkPropertyMetadata(typeof(RibbonMenuItem)));
    }

    public RibbonMenuItem()
    {
        Focusable = true;
        IsTabStop = true;
    }

    /// <summary>Raised when the item is invoked.</summary>
    public event RoutedEventHandler Click
    {
        add => AddHandler(ClickEvent, value);
        remove => RemoveHandler(ClickEvent, value);
    }

    /// <summary>Command executed when the item is invoked.</summary>
    public ICommand? Command
    {
        get => (ICommand?)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    /// <summary>Value passed to <see cref="Command"/>.</summary>
    public object? CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    /// <summary>Optional icon shown before the header.</summary>
    public ImageSource? Icon
    {
        get => (ImageSource?)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>Shortcut hint shown on the right (display only).</summary>
    public string InputGestureText
    {
        get => (string)GetValue(InputGestureTextProperty);
        set => SetValue(InputGestureTextProperty, value);
    }

    /// <summary>True while the nested submenu is shown.</summary>
    public bool IsSubmenuOpen
    {
        get => (bool)GetValue(IsSubmenuOpenProperty);
        set => SetValue(IsSubmenuOpenProperty, value);
    }

    /// <summary>
    /// Reserves a tick column and flips <see cref="IsChecked"/> on click instead of closing.
    /// Used by the quick-access customise list and by ribbon check boxes.
    /// </summary>
    public bool IsCheckable
    {
        get => (bool)GetValue(IsCheckableProperty);
        set => SetValue(IsCheckableProperty, value);
    }

    /// <summary>Tick state of a <see cref="IsCheckable"/> row.</summary>
    public bool IsChecked
    {
        get => (bool)GetValue(IsCheckedProperty);
        set => SetValue(IsCheckedProperty, value);
    }

    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonUp(e);
        if (!IsEnabled) return;

        // Take focus so Escape reaches the hosting panel and keyboard users keep their place.
        Focus();

        if (HasItems)
        {
            IsSubmenuOpen = !IsSubmenuOpen;
            e.Handled = true;
            return;
        }

        Invoke();
        e.Handled = true;
    }

    protected override void OnMouseEnter(MouseEventArgs e)
    {
        base.OnMouseEnter(e);
        if (HasItems) IsSubmenuOpen = true;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.Key is Key.Enter or Key.Space)
        {
            if (HasItems) IsSubmenuOpen = !IsSubmenuOpen;
            else Invoke();
            e.Handled = true;
        }
        else if (e.Key == Key.Right && HasItems)
        {
            IsSubmenuOpen = true;
            e.Handled = true;
        }
        else if (e.Key == Key.Escape && IsSubmenuOpen)
        {
            IsSubmenuOpen = false;
            e.Handled = true;
        }
    }

    /// <summary>
    /// Runs the row as a click would. A split button uses it to replay the entry it last ran, so
    /// its body can stand for that entry instead of the default command.
    /// </summary>
    public void PerformClick()
    {
        if (!IsEnabled || HasItems) return;
        Invoke();
    }

    private void Invoke()
    {
        if (IsCheckable) IsChecked = !IsChecked;

        if (Command != null && Command.CanExecute(CommandParameter)) Command.Execute(CommandParameter);
        RaiseEvent(new RoutedEventArgs(ClickEvent, this));
    }

    private static void OnCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not RibbonMenuItem item) return;

        if (e.OldValue is ICommand old) old.CanExecuteChanged -= item.OnCanExecuteChanged;
        if (e.NewValue is ICommand current) current.CanExecuteChanged += item.OnCanExecuteChanged;

        item.CoerceValue(IsEnabledProperty);
    }

    private void OnCanExecuteChanged(object? sender, System.EventArgs e) => CoerceValue(IsEnabledProperty);
}
