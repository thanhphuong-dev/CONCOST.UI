using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CONCOST.UI.Controls.Ribbon;

/// <summary>
/// Base class for everything that can sit inside a <see cref="RibbonGroup"/>.
/// It owns the command plumbing: <see cref="IsEnabled"/> follows <see cref="Command"/>'s
/// <c>CanExecute</c> instead of being set by hand, so a ribbon never shows a live button
/// for a command that cannot run.
/// </summary>
public abstract class RibbonItem : HeaderedContentControl
{
    public static readonly DependencyProperty CommandProperty =
        DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(RibbonItem),
            new FrameworkPropertyMetadata(null, OnCommandChanged));

    public static readonly DependencyProperty CommandParameterProperty =
        DependencyProperty.Register(nameof(CommandParameter), typeof(object), typeof(RibbonItem),
            new FrameworkPropertyMetadata(null, OnCommandParameterChanged));

    public static readonly DependencyProperty CommandTargetProperty =
        DependencyProperty.Register(nameof(CommandTarget), typeof(IInputElement), typeof(RibbonItem),
            new FrameworkPropertyMetadata(null));

    public static readonly DependencyProperty IconProperty =
        DependencyProperty.Register(nameof(Icon), typeof(ImageSource), typeof(RibbonItem),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty LargeIconProperty =
        DependencyProperty.Register(nameof(LargeIcon), typeof(ImageSource), typeof(RibbonItem),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty SmallIconProperty =
        DependencyProperty.Register(nameof(SmallIcon), typeof(ImageSource), typeof(RibbonItem),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty SizeProperty =
        DependencyProperty.Register(nameof(Size), typeof(RibbonItemSize), typeof(RibbonItem),
            new FrameworkPropertyMetadata(RibbonItemSize.Large, FrameworkPropertyMetadataOptions.AffectsParentMeasure));

    public static readonly DependencyProperty ColumnSpanProperty =
        DependencyProperty.Register(nameof(ColumnSpan), typeof(int), typeof(RibbonItem),
            new FrameworkPropertyMetadata(1, FrameworkPropertyMetadataOptions.AffectsParentMeasure,
                null, CoerceColumnSpan));

    public static readonly DependencyProperty IsFixedSizeProperty =
        DependencyProperty.Register(nameof(IsFixedSize), typeof(bool), typeof(RibbonItem),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsParentMeasure));

    /// <summary>Same property as <see cref="KeyTipService.KeyTipProperty"/>, so both spellings work.</summary>
    public static readonly DependencyProperty KeyTipProperty = KeyTipService.KeyTipProperty;

    public static readonly DependencyProperty ScreenTipProperty =
        DependencyProperty.Register(nameof(ScreenTip), typeof(string), typeof(RibbonItem),
            new FrameworkPropertyMetadata(string.Empty));

    public static readonly DependencyProperty GestureTextProperty =
        DependencyProperty.Register(nameof(GestureText), typeof(string), typeof(RibbonItem),
            new FrameworkPropertyMetadata(string.Empty));

    public static readonly DependencyProperty IdProperty =
        DependencyProperty.Register(nameof(Id), typeof(string), typeof(RibbonItem),
            new FrameworkPropertyMetadata(string.Empty));

    private RibbonToolTip? _autoTip;
    private bool _autoTipSuspended;

    private static readonly DependencyPropertyKey EffectiveIconSourcePropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(EffectiveIconSource), typeof(ImageSource), typeof(RibbonItem),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty EffectiveIconSourceProperty =
        EffectiveIconSourcePropertyKey.DependencyProperty;

    private static readonly DependencyPropertyKey EffectiveSizePropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(EffectiveSize), typeof(RibbonItemSize), typeof(RibbonItem),
            new FrameworkPropertyMetadata(RibbonItemSize.Large, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty EffectiveSizeProperty = EffectiveSizePropertyKey.DependencyProperty;

    static RibbonItem()
    {
        // A ribbon item is enabled by its command, not by whoever sets IsEnabled last.
        IsEnabledProperty.OverrideMetadata(typeof(RibbonItem),
            new FrameworkPropertyMetadata(true, null, CoerceIsEnabled));
    }

    protected RibbonItem()
    {
        Focusable = true;
        IsTabStop = true;
        HorizontalContentAlignment = HorizontalAlignment.Center;
        VerticalContentAlignment = VerticalAlignment.Center;

        // A greyed-out command still has to explain itself on hover.
        ToolTipService.SetShowOnDisabled(this, true);
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

    /// <summary>Element the command routes to when it is a <see cref="RoutedCommand"/>.</summary>
    public IInputElement? CommandTarget
    {
        get => (IInputElement?)GetValue(CommandTargetProperty);
        set => SetValue(CommandTargetProperty, value);
    }

    /// <summary>Icon used for every size unless a specific one is provided.</summary>
    public ImageSource? Icon
    {
        get => (ImageSource?)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>Icon used when <see cref="Size"/> is <see cref="RibbonItemSize.Large"/>.</summary>
    public ImageSource? LargeIcon
    {
        get => (ImageSource?)GetValue(LargeIconProperty);
        set => SetValue(LargeIconProperty, value);
    }

    /// <summary>Icon used when <see cref="Size"/> is Medium or Small.</summary>
    public ImageSource? SmallIcon
    {
        get => (ImageSource?)GetValue(SmallIconProperty);
        set => SetValue(SmallIconProperty, value);
    }

    /// <summary>How much room the item asks for inside its group.</summary>
    public RibbonItemSize Size
    {
        get => (RibbonItemSize)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    /// <summary>
    /// Number of group columns the item occupies, 1 by default. Text boxes, combo boxes and
    /// galleries use it to get a usable width without breaking the group grid.
    /// </summary>
    public int ColumnSpan
    {
        get => (int)GetValue(ColumnSpanProperty);
        set => SetValue(ColumnSpanProperty, value);
    }

    /// <summary>
    /// Keeps <see cref="Size"/> even when the ribbon compacts its groups. A gallery sets this:
    /// squeezing it into a single row would cut its tiles in half instead of saving any width.
    /// </summary>
    public bool IsFixedSize
    {
        get => (bool)GetValue(IsFixedSizeProperty);
        set => SetValue(IsFixedSizeProperty, value);
    }

    /// <summary>
    /// Letters that activate this command while the Alt key tips are showing, e.g. <c>"B"</c> for
    /// Bold or <c>"FR"</c> for a two-step badge. Leave it empty to have one derived from the header.
    /// </summary>
    public string KeyTip
    {
        get => KeyTipService.GetKeyTip(this);
        set => KeyTipService.SetKeyTip(this, value);
    }

    /// <summary>Longer explanation shown in the screen tip under the label.</summary>
    public string ScreenTip
    {
        get => (string)GetValue(ScreenTipProperty);
        set => SetValue(ScreenTipProperty, value);
    }

    /// <summary>
    /// Shortcut shown right-aligned in the screen tip, e.g. <c>Ctrl+S</c>.
    /// Display only — the ribbon never registers global hotkeys, so it cannot fight with the host
    /// application's own accelerator table.
    /// </summary>
    public string GestureText
    {
        get => (string)GetValue(GestureTextProperty);
        set => SetValue(GestureTextProperty, value);
    }

    /// <summary>Stable identifier used by quick-access and state persistence.</summary>
    public string Id
    {
        get => (string)GetValue(IdProperty);
        set => SetValue(IdProperty, value);
    }

    /// <summary>Icon that matches the current <see cref="Size"/>.</summary>
    public ImageSource? EffectiveIcon =>
        Size == RibbonItemSize.Large ? LargeIcon ?? Icon : SmallIcon ?? Icon;

    /// <summary>Bindable form of <see cref="EffectiveIcon"/> for the control template.</summary>
    public ImageSource? EffectiveIconSource => (ImageSource?)GetValue(EffectiveIconSourceProperty);

    /// <summary>
    /// Size actually used on screen. It equals <see cref="Size"/> unless the owning group has been
    /// compacted by the ribbon, in which case it is capped — templates must switch on this, not on
    /// <see cref="Size"/>.
    /// </summary>
    public RibbonItemSize EffectiveSize => (RibbonItemSize)GetValue(EffectiveSizeProperty);

    internal void SetEffectiveSize(RibbonItemSize size)
    {
        if (EffectiveSize != size) SetValue(EffectiveSizePropertyKey, size);
    }

    /// <summary>
    /// Creates the small proxy button the quick-access toolbar shows. A control can only have one
    /// visual parent, so the toolbar cannot host the original item — it hosts a clone that shares
    /// the same command, icon and tooltip.
    /// </summary>
    public RibbonQuickAccessButton CreateQuickAccessClone()
    {
        var clone = new RibbonQuickAccessButton
        {
            Header = Header,
            Command = Command,
            CommandParameter = CommandParameter,
            CommandTarget = CommandTarget,
            Icon = Icon,
            LargeIcon = LargeIcon,
            SmallIcon = SmallIcon,
            Size = RibbonItemSize.Small,
            KeyTip = KeyTip,
            ScreenTip = ScreenTip,
            GestureText = GestureText,
            Id = Id
        };

        return clone;
    }

    protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.Property == IconProperty || e.Property == LargeIconProperty ||
            e.Property == SmallIconProperty || e.Property == SizeProperty)
        {
            SetValue(EffectiveIconSourcePropertyKey, EffectiveIcon);
        }

        if (e.Property == HeaderProperty || e.Property == ScreenTipProperty ||
            e.Property == GestureTextProperty || e.Property == IconProperty ||
            e.Property == LargeIconProperty)
        {
            UpdateAutoToolTip();
        }
    }

    /// <summary>
    /// Keeps the built-in screen tip in step with the item. It gives up as soon as the caller
    /// assigns a <see cref="FrameworkElement.ToolTip"/> of their own, so a hand-written tip is
    /// never overwritten.
    /// </summary>
    private void UpdateAutoToolTip()
    {
        if (_autoTipSuspended) return;

        if (ToolTip != null && !ReferenceEquals(ToolTip, _autoTip))
        {
            _autoTipSuspended = true;
            return;
        }

        string title = Header as string ?? string.Empty;
        if (title.Length == 0 && ScreenTip.Length == 0)
        {
            ToolTip = null;
            return;
        }

        _autoTip ??= new RibbonToolTip();
        _autoTip.Title = title;
        _autoTip.Description = ScreenTip;
        _autoTip.GestureText = GestureText;

        if (!ReferenceEquals(ToolTip, _autoTip)) ToolTip = _autoTip;
    }

    /// <summary>Runs the command if it can execute. Returns true when it did.</summary>
    protected bool InvokeCommand()
    {
        ICommand? command = Command;
        if (command == null) return false;
        if (!command.CanExecute(CommandParameter)) return false;

        command.Execute(CommandParameter);
        return true;
    }

    /// <summary>
    /// Activates the item as a click would. Keyboard key tips and the command search call this, so
    /// a control that is not a plain button (a gallery, an input) can do the right thing — open its
    /// panel, focus its editor — instead of pretending to be a button.
    /// </summary>
    public virtual void PerformClick()
    {
        if (!IsEnabled) return;
        Focus();
    }

    private static object CoerceIsEnabled(DependencyObject d, object baseValue)
    {
        if (d is not RibbonItem item || item.Command == null) return baseValue;
        return item.Command.CanExecute(item.CommandParameter);
    }

    private static object CoerceColumnSpan(DependencyObject d, object baseValue)
    {
        int span = (int)baseValue;
        return span < 1 ? 1 : span;
    }

    private static void OnCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not RibbonItem item) return;

        if (e.OldValue is ICommand old) old.CanExecuteChanged -= item.OnCanExecuteChanged;
        if (e.NewValue is ICommand current) current.CanExecuteChanged += item.OnCanExecuteChanged;

        item.CoerceValue(IsEnabledProperty);
    }

    private static void OnCommandParameterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is RibbonItem item && item.Command != null) item.CoerceValue(IsEnabledProperty);
    }

    private void OnCanExecuteChanged(object? sender, EventArgs e) => CoerceValue(IsEnabledProperty);
}
