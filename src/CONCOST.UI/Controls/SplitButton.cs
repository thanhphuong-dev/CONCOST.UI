using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Threading;
using CONCOST.UI.Icons;

namespace CONCOST.UI.Controls;

/// <summary>
/// Specifies the hit-test interaction mode for a <see cref="SplitButton"/>.
/// </summary>
public enum SplitButtonHitMode
{
    /// <summary>
    /// Clicking the action button triggers primary action; clicking dropdown button toggles dropdown.
    /// </summary>
    Split,

    /// <summary>
    /// Clicking either the action button or the dropdown button toggles the dropdown popup (DropDownButton mode).
    /// </summary>
    DropDown
}

/// <summary>
/// A modern enterprise SplitButton combining a primary action button and a dropdown menu trigger.
/// Supporting:
/// - Split & DropDown interaction modes (<see cref="HitMode"/>)
/// - Flexible dropdown placements (Bottom, Top, Left, Right)
/// - Left or Right chevron placement (<see cref="DropDownButtonPosition"/>)
/// - Icon left/right placement (<see cref="IconPlacement"/>)
/// - Hover-to-open workflow (<see cref="IsOpenOnHover"/>)
/// - Auto-close behavior on item selection (<see cref="AutoCloseOnClick"/>)
/// - Keyboard accessibility (Alt+Down, F4, Escape)
/// - Independent hover/pressed states and micro-animations
/// </summary>
[ContentProperty(nameof(DropDownContent))]
[TemplatePart(Name = "PART_ActionButton", Type = typeof(Button))]
[TemplatePart(Name = "PART_DropDownButton", Type = typeof(ToggleButton))]
[TemplatePart(Name = "PART_Popup", Type = typeof(Popup))]
public class SplitButton : Control
{
    private Button? _actionButton;
    private ToggleButton? _dropDownButton;
    private Popup? _popup;
    private DispatcherTimer? _hoverOpenTimer;
    private DispatcherTimer? _hoverCloseTimer;

    static SplitButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(SplitButton), new FrameworkPropertyMetadata(typeof(SplitButton)));
    }

    #region Dependency Properties

    public static readonly DependencyProperty ContentProperty =
        DependencyProperty.Register(nameof(Content), typeof(object), typeof(SplitButton), new PropertyMetadata(null));

    public static readonly DependencyProperty IconProperty =
        DependencyProperty.Register(nameof(Icon), typeof(IconKind), typeof(SplitButton), new PropertyMetadata(IconKind.None));

    public static readonly DependencyProperty IconSizeProperty =
        DependencyProperty.Register(nameof(IconSize), typeof(double), typeof(SplitButton), new PropertyMetadata(14.0));

    public static readonly DependencyProperty IconPlacementProperty =
        DependencyProperty.Register(nameof(IconPlacement), typeof(Dock), typeof(SplitButton), new PropertyMetadata(Dock.Left));

    public static readonly DependencyProperty CommandProperty =
        DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(SplitButton),
            new PropertyMetadata(null, OnCommandChanged));

    public static readonly DependencyProperty CommandParameterProperty =
        DependencyProperty.Register(nameof(CommandParameter), typeof(object), typeof(SplitButton), new PropertyMetadata(null));

    public static readonly DependencyProperty CommandTargetProperty =
        DependencyProperty.Register(nameof(CommandTarget), typeof(IInputElement), typeof(SplitButton), new PropertyMetadata(null));

    public static readonly DependencyProperty DropDownContentProperty =
        DependencyProperty.Register(nameof(DropDownContent), typeof(object), typeof(SplitButton), new PropertyMetadata(null));

    public static readonly DependencyProperty DropDownContentTemplateProperty =
        DependencyProperty.Register(nameof(DropDownContentTemplate), typeof(DataTemplate), typeof(SplitButton), new PropertyMetadata(null));

    public static readonly DependencyProperty IsDropDownOpenProperty =
        DependencyProperty.Register(nameof(IsDropDownOpen), typeof(bool), typeof(SplitButton),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnIsDropDownOpenChanged));

    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(SplitButton), new PropertyMetadata(new CornerRadius(6)));

    public static readonly DependencyProperty PlacementProperty =
        DependencyProperty.Register(nameof(Placement), typeof(PlacementMode), typeof(SplitButton), new PropertyMetadata(PlacementMode.Bottom));

    public static readonly DependencyProperty MaxDropDownHeightProperty =
        DependencyProperty.Register(nameof(MaxDropDownHeight), typeof(double), typeof(SplitButton), new PropertyMetadata(320.0));

    public static readonly DependencyProperty HitModeProperty =
        DependencyProperty.Register(nameof(HitMode), typeof(SplitButtonHitMode), typeof(SplitButton), new PropertyMetadata(SplitButtonHitMode.Split));

    public static readonly DependencyProperty AutoCloseOnClickProperty =
        DependencyProperty.Register(nameof(AutoCloseOnClick), typeof(bool), typeof(SplitButton), new PropertyMetadata(true));

    public static readonly DependencyProperty StaysOpenProperty =
        DependencyProperty.Register(nameof(StaysOpen), typeof(bool), typeof(SplitButton), new PropertyMetadata(false));

    public static readonly DependencyProperty IsOpenOnHoverProperty =
        DependencyProperty.Register(nameof(IsOpenOnHover), typeof(bool), typeof(SplitButton), new PropertyMetadata(false));

    public static readonly DependencyProperty HoverDelayProperty =
        DependencyProperty.Register(nameof(HoverDelay), typeof(TimeSpan), typeof(SplitButton), new PropertyMetadata(TimeSpan.FromMilliseconds(200)));

    public static readonly DependencyProperty DropDownButtonPositionProperty =
        DependencyProperty.Register(nameof(DropDownButtonPosition), typeof(Dock), typeof(SplitButton), new PropertyMetadata(Dock.Right));

    #endregion

    #region Routed Events

    public static readonly RoutedEvent ClickEvent =
        EventManager.RegisterRoutedEvent(nameof(Click), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(SplitButton));

    public event RoutedEventHandler Click
    {
        add => AddHandler(ClickEvent, value);
        remove => RemoveHandler(ClickEvent, value);
    }

    public static readonly RoutedEvent DropDownOpenedEvent =
        EventManager.RegisterRoutedEvent(nameof(DropDownOpened), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(SplitButton));

    public event RoutedEventHandler DropDownOpened
    {
        add => AddHandler(DropDownOpenedEvent, value);
        remove => RemoveHandler(DropDownOpenedEvent, value);
    }

    public static readonly RoutedEvent DropDownClosedEvent =
        EventManager.RegisterRoutedEvent(nameof(DropDownClosed), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(SplitButton));

    public event RoutedEventHandler DropDownClosed
    {
        add => AddHandler(DropDownClosedEvent, value);
        remove => RemoveHandler(DropDownClosedEvent, value);
    }

    #endregion

    #region Properties

    public object? Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    public IconKind Icon
    {
        get => (IconKind)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public double IconSize
    {
        get => (double)GetValue(IconSizeProperty);
        set => SetValue(IconSizeProperty, value);
    }

    public Dock IconPlacement
    {
        get => (Dock)GetValue(IconPlacementProperty);
        set => SetValue(IconPlacementProperty, value);
    }

    public ICommand? Command
    {
        get => (ICommand?)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public object? CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    public IInputElement? CommandTarget
    {
        get => (IInputElement?)GetValue(CommandTargetProperty);
        set => SetValue(CommandTargetProperty, value);
    }

    public object? DropDownContent
    {
        get => GetValue(DropDownContentProperty);
        set => SetValue(DropDownContentProperty, value);
    }

    public DataTemplate? DropDownContentTemplate
    {
        get => (DataTemplate?)GetValue(DropDownContentTemplateProperty);
        set => SetValue(DropDownContentTemplateProperty, value);
    }

    public bool IsDropDownOpen
    {
        get => (bool)GetValue(IsDropDownOpenProperty);
        set => SetValue(IsDropDownOpenProperty, value);
    }

    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    public PlacementMode Placement
    {
        get => (PlacementMode)GetValue(PlacementProperty);
        set => SetValue(PlacementProperty, value);
    }

    public double MaxDropDownHeight
    {
        get => (double)GetValue(MaxDropDownHeightProperty);
        set => SetValue(MaxDropDownHeightProperty, value);
    }

    public SplitButtonHitMode HitMode
    {
        get => (SplitButtonHitMode)GetValue(HitModeProperty);
        set => SetValue(HitModeProperty, value);
    }

    public bool AutoCloseOnClick
    {
        get => (bool)GetValue(AutoCloseOnClickProperty);
        set => SetValue(AutoCloseOnClickProperty, value);
    }

    public bool StaysOpen
    {
        get => (bool)GetValue(StaysOpenProperty);
        set => SetValue(StaysOpenProperty, value);
    }

    public bool IsOpenOnHover
    {
        get => (bool)GetValue(IsOpenOnHoverProperty);
        set => SetValue(IsOpenOnHoverProperty, value);
    }

    public TimeSpan HoverDelay
    {
        get => (TimeSpan)GetValue(HoverDelayProperty);
        set => SetValue(HoverDelayProperty, value);
    }

    public Dock DropDownButtonPosition
    {
        get => (Dock)GetValue(DropDownButtonPositionProperty);
        set => SetValue(DropDownButtonPositionProperty, value);
    }

    #endregion

    #region Template & Event Handlers

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        if (_actionButton != null)
        {
            _actionButton.Click -= OnActionButtonClick;
        }

        if (_popup != null)
        {
            _popup.Opened -= OnPopupOpened;
            _popup.RemoveHandler(ButtonBase.ClickEvent, new RoutedEventHandler(OnPopupItemClicked));
            _popup.RemoveHandler(MenuItem.ClickEvent, new RoutedEventHandler(OnPopupItemClicked));
        }

        _actionButton = GetTemplateChild("PART_ActionButton") as Button;
        _dropDownButton = GetTemplateChild("PART_DropDownButton") as ToggleButton;
        _popup = GetTemplateChild("PART_Popup") as Popup;

        if (_actionButton != null)
        {
            _actionButton.Click += OnActionButtonClick;
        }

        if (_popup != null)
        {
            _popup.Opened += OnPopupOpened;
            _popup.AddHandler(ButtonBase.ClickEvent, new RoutedEventHandler(OnPopupItemClicked));
            _popup.AddHandler(MenuItem.ClickEvent, new RoutedEventHandler(OnPopupItemClicked));
        }

        UpdateCanExecute();
    }

    private void OnActionButtonClick(object sender, RoutedEventArgs e)
    {
        if (HitMode == SplitButtonHitMode.DropDown)
        {
            IsDropDownOpen = !IsDropDownOpen;
            return;
        }

        RaiseEvent(new RoutedEventArgs(ClickEvent, this));
        ExecuteCommand();
    }

    private void OnPopupItemClicked(object sender, RoutedEventArgs e)
    {
        if (!AutoCloseOnClick) return;
        IsDropDownOpen = false;
    }

    private void OnPopupOpened(object? sender, EventArgs e)
    {
        if (_popup?.Child is FrameworkElement popupElem)
        {
            popupElem.MouseEnter -= OnPopupContentMouseEnter;
            popupElem.MouseEnter += OnPopupContentMouseEnter;
            popupElem.MouseLeave -= OnPopupContentMouseLeave;
            popupElem.MouseLeave += OnPopupContentMouseLeave;
        }
    }

    private void OnPopupContentMouseEnter(object sender, MouseEventArgs e)
    {
        if (IsOpenOnHover)
        {
            _hoverCloseTimer?.Stop();
        }
    }

    private void OnPopupContentMouseLeave(object sender, MouseEventArgs e)
    {
        if (IsOpenOnHover && IsDropDownOpen && !IsMouseOver)
        {
            _hoverCloseTimer?.Start();
        }
    }

    protected override void OnMouseEnter(MouseEventArgs e)
    {
        base.OnMouseEnter(e);
        if (!IsOpenOnHover || !IsEnabled) return;

        _hoverCloseTimer?.Stop();

        if (!IsDropDownOpen)
        {
            _hoverOpenTimer ??= new DispatcherTimer();
            _hoverOpenTimer.Interval = HoverDelay;
            _hoverOpenTimer.Tick -= OnHoverOpenTick;
            _hoverOpenTimer.Tick += OnHoverOpenTick;
            _hoverOpenTimer.Start();
        }
    }

    protected override void OnMouseLeave(MouseEventArgs e)
    {
        base.OnMouseLeave(e);
        if (!IsOpenOnHover) return;

        _hoverOpenTimer?.Stop();

        if (IsDropDownOpen)
        {
            _hoverCloseTimer ??= new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(250) };
            _hoverCloseTimer.Tick -= OnHoverCloseTick;
            _hoverCloseTimer.Tick += OnHoverCloseTick;
            _hoverCloseTimer.Start();
        }
    }

    private void OnHoverOpenTick(object? sender, EventArgs e)
    {
        _hoverOpenTimer?.Stop();
        if (IsMouseOver && IsEnabled)
        {
            IsDropDownOpen = true;
        }
    }

    private void OnHoverCloseTick(object? sender, EventArgs e)
    {
        _hoverCloseTimer?.Stop();
        if (!IsMouseOver && !IsPopupMouseOver())
        {
            IsDropDownOpen = false;
        }
    }

    private bool IsPopupMouseOver()
    {
        if (_popup?.Child is FrameworkElement elem)
        {
            return elem.IsMouseOver;
        }
        return false;
    }

    private static void OnIsDropDownOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SplitButton sb)
        {
            if ((bool)e.NewValue)
                sb.RaiseEvent(new RoutedEventArgs(DropDownOpenedEvent, sb));
            else
                sb.RaiseEvent(new RoutedEventArgs(DropDownClosedEvent, sb));
        }
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.Handled) return;

        if ((e.Key == Key.Down && (Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt) || e.Key == Key.F4)
        {
            IsDropDownOpen = !IsDropDownOpen;
            e.Handled = true;
        }
        else if (e.Key == Key.Escape && IsDropDownOpen)
        {
            IsDropDownOpen = false;
            e.Handled = true;
        }
    }

    private static void OnCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SplitButton sb)
        {
            if (e.OldValue is ICommand oldCmd)
                oldCmd.CanExecuteChanged -= sb.OnCanExecuteChanged;

            if (e.NewValue is ICommand newCmd)
                newCmd.CanExecuteChanged += sb.OnCanExecuteChanged;

            sb.UpdateCanExecute();
        }
    }

    private void OnCanExecuteChanged(object? sender, EventArgs e) => UpdateCanExecute();

    private void UpdateCanExecute()
    {
        if (Command != null)
        {
            if (Command is RoutedCommand routed)
            {
                IsEnabled = routed.CanExecute(CommandParameter, CommandTarget ?? this);
            }
            else
            {
                IsEnabled = Command.CanExecute(CommandParameter);
            }
        }
    }

    private void ExecuteCommand()
    {
        if (Command != null)
        {
            if (Command is RoutedCommand routed)
            {
                if (routed.CanExecute(CommandParameter, CommandTarget ?? this))
                {
                    routed.Execute(CommandParameter, CommandTarget ?? this);
                }
            }
            else if (Command.CanExecute(CommandParameter))
            {
                Command.Execute(CommandParameter);
            }
        }
    }

    #endregion
}
