using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CONCOST.UI.Icons;

namespace CONCOST.UI.Controls;

/// <summary>Which edge of a <see cref="DockingManager"/> a <see cref="ToolWindow"/> is docked to.</summary>
public enum DockSide
{
    Left,
    Top,
    Right,
    Bottom,
    Center
}

/// <summary>When a pane header shows its pin and close buttons.</summary>
public enum DockHeaderButtons
{
    /// <summary>Both buttons are always visible, the way a dialog frame shows them.</summary>
    Always,

    /// <summary>
    /// Visible on the active pane and while the pointer is over any pane. The default: a header that
    /// carries two glyphs it is not currently using is two glyphs of noise, and four panes make eight.
    /// </summary>
    Active,

    /// <summary>Visible only while the pointer is over the pane — the quietest state that still works.</summary>
    Hover,

    /// <summary>Never drawn; the host closes the pane from its own menu instead.</summary>
    None
}

/// <summary>
/// A dockable panel: a header with a title and the pin/close buttons, and the content underneath.
/// </summary>
/// <remarks>
/// <para>
/// The window does not place itself — <see cref="DockingManager"/> reads <see cref="Dock"/> and
/// <see cref="DockSize"/> and does the layout, so the same panel can be moved to another edge, hidden
/// behind an edge tab (<see cref="IsAutoHidden"/>) or dropped into the centre area without the panel
/// knowing anything about the layout.
/// </para>
/// <para>
/// Dragging the header raises <see cref="DragStarted"/>, <see cref="DragMoved"/> and
/// <see cref="DragCompleted"/>; the manager turns those into a drop preview and a new dock side.
/// </para>
/// </remarks>
public class ToolWindow : ContentControl
{
    static ToolWindow()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(ToolWindow),
            new FrameworkPropertyMetadata(typeof(ToolWindow)));
    }

    public ToolWindow()
    {
        Focusable = false;
    }

    // ---------------------------------------------------------------- header

    public static readonly DependencyProperty HeaderProperty =
        DependencyProperty.Register(nameof(Header), typeof(object), typeof(ToolWindow),
            new PropertyMetadata("Công cụ"));

    /// <summary>Title shown in the header. Plain text or any content.</summary>
    public object? Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    public static readonly DependencyProperty IconProperty =
        DependencyProperty.Register(nameof(Icon), typeof(IconKind), typeof(ToolWindow),
            new PropertyMetadata(IconKind.None));

    /// <summary>Glyph shown before the title.</summary>
    public IconKind Icon
    {
        get => (IconKind)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public static readonly DependencyProperty ShowHeaderProperty =
        DependencyProperty.Register(nameof(ShowHeader), typeof(bool), typeof(ToolWindow),
            new PropertyMetadata(true));

    /// <summary>Draw the header. Turn it off for a panel that is only ever docked in the centre.</summary>
    public bool ShowHeader
    {
        get => (bool)GetValue(ShowHeaderProperty);
        set => SetValue(ShowHeaderProperty, value);
    }

    public static readonly DependencyProperty HeaderHeightProperty =
        DependencyProperty.Register(nameof(HeaderHeight), typeof(double), typeof(ToolWindow),
            new FrameworkPropertyMetadata(28.0, FrameworkPropertyMetadataOptions.AffectsMeasure));

    /// <summary>Height of the header bar, in pixels. Default 28.</summary>
    public double HeaderHeight
    {
        get => (double)GetValue(HeaderHeightProperty);
        set => SetValue(HeaderHeightProperty, value);
    }

    public static readonly DependencyProperty IconSizeProperty =
        DependencyProperty.Register(nameof(IconSize), typeof(double), typeof(ToolWindow),
            new FrameworkPropertyMetadata(13.0, FrameworkPropertyMetadataOptions.AffectsMeasure));

    /// <summary>Size of the glyph in front of the title, in pixels. Default 13.</summary>
    public double IconSize
    {
        get => (double)GetValue(IconSizeProperty);
        set => SetValue(IconSizeProperty, value);
    }

    public static readonly DependencyProperty HeaderButtonsProperty =
        DependencyProperty.Register(nameof(HeaderButtons), typeof(DockHeaderButtons), typeof(ToolWindow),
            new PropertyMetadata(DockHeaderButtons.Active));

    /// <summary>
    /// When the pin and close buttons appear. Default <see cref="DockHeaderButtons.Active"/>: on the
    /// active pane, plus any pane the pointer is over.
    /// </summary>
    public DockHeaderButtons HeaderButtons
    {
        get => (DockHeaderButtons)GetValue(HeaderButtonsProperty);
        set => SetValue(HeaderButtonsProperty, value);
    }

    public static readonly DependencyProperty ShowActiveAccentProperty =
        DependencyProperty.Register(nameof(ShowActiveAccent), typeof(bool), typeof(ToolWindow),
            new PropertyMetadata(true));

    /// <summary>
    /// Draw the accent rule along the header's top edge while this pane is active. It is the one mark
    /// that says which of four adjacent panes the keyboard is talking to.
    /// </summary>
    public bool ShowActiveAccent
    {
        get => (bool)GetValue(ShowActiveAccentProperty);
        set => SetValue(ShowActiveAccentProperty, value);
    }

    // ---------------------------------------------------------------- docking state

    public static readonly DependencyProperty DockProperty =
        DependencyProperty.Register(nameof(Dock), typeof(DockSide), typeof(ToolWindow),
            new FrameworkPropertyMetadata(DockSide.Left, FrameworkPropertyMetadataOptions.AffectsParentMeasure));

    /// <summary>Edge the panel is docked to. The manager owns the layout, this is the request.</summary>
    public DockSide Dock
    {
        get => (DockSide)GetValue(DockProperty);
        set => SetValue(DockProperty, value);
    }

    public static readonly DependencyProperty DockSizeProperty =
        DependencyProperty.Register(nameof(DockSize), typeof(double), typeof(ToolWindow),
            new FrameworkPropertyMetadata(260.0, FrameworkPropertyMetadataOptions.AffectsParentMeasure));

    /// <summary>Width for a left/right panel, height for a top/bottom one. Ignored in the centre.</summary>
    public double DockSize
    {
        get => (double)GetValue(DockSizeProperty);
        set => SetValue(DockSizeProperty, value);
    }

    public static readonly DependencyProperty MinDockSizeProperty =
        DependencyProperty.Register(nameof(MinDockSize), typeof(double), typeof(ToolWindow),
            new PropertyMetadata(120.0));

    /// <summary>Lower bound the user cannot drag past. Default 120.</summary>
    public double MinDockSize
    {
        get => (double)GetValue(MinDockSizeProperty);
        set => SetValue(MinDockSizeProperty, value);
    }

    public static readonly DependencyProperty IsAutoHiddenProperty =
        DependencyProperty.Register(nameof(IsAutoHidden), typeof(bool), typeof(ToolWindow),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsParentMeasure,
                OnAutoHiddenChanged));

    /// <summary>Collapsed to an edge tab; it slides out over the centre when opened.</summary>
    public bool IsAutoHidden
    {
        get => (bool)GetValue(IsAutoHiddenProperty);
        set => SetValue(IsAutoHiddenProperty, value);
    }

    private static void OnAutoHiddenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ToolWindow pane) pane.AutoHideChanged?.Invoke(pane, EventArgs.Empty);
    }

    /// <summary>
    /// Raised when <see cref="IsAutoHidden"/> flips. The manager listens so a panel nested inside a
    /// group can be pulled out to an edge strip, which is the one place auto-hide is implemented.
    /// </summary>
    public event EventHandler? AutoHideChanged;

    public static readonly DependencyProperty IsFlyoutOpenProperty =
        DependencyProperty.Register(nameof(IsFlyoutOpen), typeof(bool), typeof(ToolWindow),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsParentMeasure));

    /// <summary>An auto-hidden panel that is currently slid out.</summary>
    public bool IsFlyoutOpen
    {
        get => (bool)GetValue(IsFlyoutOpenProperty);
        set => SetValue(IsFlyoutOpenProperty, value);
    }

    public static readonly DependencyProperty CanCloseProperty =
        DependencyProperty.Register(nameof(CanClose), typeof(bool), typeof(ToolWindow),
            new PropertyMetadata(true));

    /// <summary>Show the close button. Default <c>true</c>.</summary>
    public bool CanClose
    {
        get => (bool)GetValue(CanCloseProperty);
        set => SetValue(CanCloseProperty, value);
    }

    public static readonly DependencyProperty CanAutoHideProperty =
        DependencyProperty.Register(nameof(CanAutoHide), typeof(bool), typeof(ToolWindow),
            new PropertyMetadata(true));

    /// <summary>Show the pin button. Default <c>true</c>.</summary>
    public bool CanAutoHide
    {
        get => (bool)GetValue(CanAutoHideProperty);
        set => SetValue(CanAutoHideProperty, value);
    }

    public static readonly DependencyProperty IsActiveProperty =
        DependencyProperty.Register(nameof(IsActive), typeof(bool), typeof(ToolWindow),
            new FrameworkPropertyMetadata(false,
                FrameworkPropertyMetadataOptions.AffectsParentMeasure, OnIsActiveChanged));

    /// <summary>The panel the user last interacted with — the manager highlights its header.</summary>
    public bool IsActive
    {
        get => (bool)GetValue(IsActiveProperty);
        set => SetValue(IsActiveProperty, value);
    }

    private static void OnIsActiveChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ToolWindow pane && e.NewValue is true) pane.Activated?.Invoke(pane, EventArgs.Empty);
    }

    // ---------------------------------------------------------------- events

    /// <summary>
    /// Raised when this panel becomes the active one. The manager listens so that setting
    /// <see cref="IsActive"/> from code switches the tab exactly like a click does.
    /// </summary>
    public event EventHandler? Activated;

    /// <summary>Raised before the panel closes; set <see cref="CancelEventArgs.Cancel"/> to veto.</summary>
    public event EventHandler<CancelEventArgs>? Closing;

    /// <summary>Raised after the panel is closed.</summary>
    public event EventHandler? Closed;

    /// <summary>Header drag began.</summary>
    public event EventHandler? DragStarted;

    /// <summary>Header drag moved. <c>e</c> is the pointer position in the manager's coordinates.</summary>
    public event EventHandler<Point>? DragMoved;

    /// <summary>Header drag ended; <c>e</c> is the pointer position in the manager's coordinates.</summary>
    public event EventHandler<Point>? DragCompleted;

    // ---------------------------------------------------------------- commands

    /// <summary>Closes the panel, honouring <see cref="Closing"/>.</summary>
    public void Close()
    {
        var args = new CancelEventArgs();
        Closing?.Invoke(this, args);
        if (args.Cancel) return;

        Closed?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Flips the auto-hide state.</summary>
    public void ToggleAutoHide()
    {
        IsAutoHidden = !IsAutoHidden;
        if (!IsAutoHidden) IsFlyoutOpen = false;
    }

    // ---------------------------------------------------------------- header interaction

    private Border? _headerBar;
    private Button? _closeButton;
    private Button? _pinButton;
    private Point _dragOrigin;
    private bool _isDragging;

    /// <summary>
    /// Wires the template parts. The handlers live in code rather than in the template because
    /// <c>Generic.xaml</c> is a plain resource dictionary — it has no <c>x:Class</c>, so XAML event
    /// handlers are not allowed there.
    /// </summary>
    /// <remarks>
    /// Only the press is handled on the header. The move and release handlers sit on the
    /// <see cref="ToolWindow"/> itself because the header captures the mouse: once an element captures,
    /// the mouse route starts at the capture element and bubbles up, so a handler on a <em>child</em>
    /// of that element never runs again. Capturing on the header instead would keep the child in the
    /// route, but then a template swap would silently drop the capture.
    /// </remarks>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        if (_headerBar is not null) _headerBar.MouseLeftButtonDown -= OnHeaderMouseLeftButtonDown;
        MouseMove -= OnHeaderMouseMove;
        MouseLeftButtonUp -= OnHeaderMouseLeftButtonUp;
        LostMouseCapture -= OnHeaderLostCapture;

        if (_closeButton is not null) _closeButton.Click -= OnCloseButtonClick;
        if (_pinButton is not null) _pinButton.Click -= OnPinButtonClick;

        _headerBar = GetTemplateChild("HeaderBar") as Border;
        _closeButton = GetTemplateChild("CloseButton") as Button;
        _pinButton = GetTemplateChild("PinButton") as Button;

        if (_headerBar is not null) _headerBar.MouseLeftButtonDown += OnHeaderMouseLeftButtonDown;
        MouseMove += OnHeaderMouseMove;
        MouseLeftButtonUp += OnHeaderMouseLeftButtonUp;
        LostMouseCapture += OnHeaderLostCapture;

        if (_closeButton is not null) _closeButton.Click += OnCloseButtonClick;
        if (_pinButton is not null) _pinButton.Click += OnPinButtonClick;
    }

    /// <summary>Called when the header is pressed.</summary>
    protected virtual void OnHeaderMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount > 1)
        {
            ToggleAutoHide();
            e.Handled = true;
            return;
        }

        _dragOrigin = e.GetPosition(this);
        _isDragging = false;
        CaptureMouse();
    }

    /// <summary>Called by the template while the header is dragged.</summary>
    protected virtual void OnHeaderMouseMove(object sender, MouseEventArgs e)
    {
        if (!IsMouseCaptured || e.LeftButton != MouseButtonState.Pressed) return;

        var position = e.GetPosition(this);
        if (!_isDragging)
        {
            if (Math.Abs(position.X - _dragOrigin.X) < 4 && Math.Abs(position.Y - _dragOrigin.Y) < 4) return;

            _isDragging = true;
            DragStarted?.Invoke(this, EventArgs.Empty);
        }

        DragMoved?.Invoke(this, ToManagerPoint(e));
    }

    /// <summary>Called by the template when the header is released.</summary>
    protected virtual void OnHeaderMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        // ReleaseMouseCapture raises LostMouseCapture synchronously, and that handler clears the flag.
        // Read it first, otherwise the drop never reaches the manager and the preview stays on screen.
        bool dragging = _isDragging;
        _isDragging = false;

        ReleaseMouseCapture();

        if (!dragging) return;

        DragCompleted?.Invoke(this, ToManagerPoint(e));
    }

    /// <summary>Clears the drag flag when capture is taken away, so a cancelled drag cannot stick.</summary>
    protected virtual void OnHeaderLostCapture(object sender, MouseEventArgs e)
    {
        _isDragging = false;
    }

    private Point ToManagerPoint(MouseEventArgs e)
    {
        if (ParentElement() is UIElement manager) return e.GetPosition(manager);
        return e.GetPosition(this);
    }

    private DependencyObject? ParentElement()
    {
        return System.Windows.Media.VisualTreeHelper.GetParent(this);
    }

    /// <summary>Called by the template's close button.</summary>
    protected virtual void OnCloseButtonClick(object sender, RoutedEventArgs e) => Close();

    /// <summary>Called by the template's pin button.</summary>
    protected virtual void OnPinButtonClick(object sender, RoutedEventArgs e) => ToggleAutoHide();
}
