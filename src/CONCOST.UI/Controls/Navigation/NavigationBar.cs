using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using CONCOST.UI.AttachedProperties;
using CONCOST.UI.Icons;

namespace CONCOST.UI.Controls;

/// <summary>A one-shot navigation action a <see cref="NavigationBar"/> asks the host to perform.</summary>
public enum NavigationCommand
{
    ZoomIn,
    ZoomOut,
    ZoomExtents,
    ZoomSelection,
    Perspective
}

/// <summary>A navigation tool that stays active until it is switched off.</summary>
public enum NavigationTool
{
    None,
    Pan,
    Orbit,
    Walk
}

/// <summary>The orientation label a <see cref="NavigationBar"/> shows, and the host applies to its camera.</summary>
public enum UcsMode
{
    World,
    Top,
    Front,
    Right
}

/// <summary>How a <see cref="NavigationBar"/> is dressed.</summary>
public enum NavigationBarVariant
{
    /// <summary>A surface plate with one outline and a shadow: the bar that floats over a viewport.</summary>
    Floating,

    /// <summary>No frame at all, only the flat commands: the bar that docks inside a toolbar or a status bar.</summary>
    Seamless,

    /// <summary>A translucent plate for a busy 3D scene, where an opaque one hides the model behind it.</summary>
    Glass
}

/// <summary>Carries the command a <see cref="NavigationBar"/> button stands for.</summary>
public class NavigationRequestedEventArgs : EventArgs
{
    public NavigationRequestedEventArgs(NavigationCommand command) => Command = command;

    /// <summary>The action the user asked for.</summary>
    public NavigationCommand Command { get; }
}

/// <summary>
/// The floating navigation bar that sits over a viewport: zoom in, zoom out, zoom to fit, the pan and
/// orbit tools, and the coordinate-system label.
/// </summary>
/// <remarks>
/// <para>
/// Buttons rather than a command object: every one of them maps to one call on a
/// <c>CameraController</c>, and a host that has no camera can still ignore the event. Keeping the bar
/// passive means it works over a WPF viewport and over a DirectX one alike.
/// </para>
/// <para>
/// Pan, orbit and walk are exclusive. Pressing the active tool again turns it off, which is the only
/// way to get out of the tool without a second bar or an Escape key.
/// </para>
/// <para>
/// Exactly one outline belongs to the bar: the frame. The commands inside it are flat until they are
/// hovered, because a row of outlined chips inside an outlined plate reads as a table of boxes, not
/// as one tool. <see cref="Variant"/> decides whether the frame exists at all — see
/// <see cref="NavigationBarVariant"/>.
/// </para>
/// </remarks>
public class NavigationBar : Grid
{
    /// <summary>Identifies the <see cref="ActiveTool"/> property.</summary>
    public static readonly DependencyProperty ActiveToolProperty = DependencyProperty.Register(
        nameof(ActiveTool), typeof(NavigationTool), typeof(NavigationBar),
        new FrameworkPropertyMetadata(NavigationTool.None, OnActiveToolChanged));

    /// <summary>Identifies the <see cref="Ucs"/> property.</summary>
    public static readonly DependencyProperty UcsProperty = DependencyProperty.Register(
        nameof(Ucs), typeof(UcsMode), typeof(NavigationBar),
        new FrameworkPropertyMetadata(UcsMode.World, OnUcsChanged));

    /// <summary>Identifies the <see cref="ShowWalk"/> property.</summary>
    public static readonly DependencyProperty ShowWalkProperty = DependencyProperty.Register(
        nameof(ShowWalk), typeof(bool), typeof(NavigationBar),
        new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsMeasure, OnGroupVisibilityChanged));

    /// <summary>Identifies the <see cref="Variant"/> property.</summary>
    public static readonly DependencyProperty VariantProperty = DependencyProperty.Register(
        nameof(Variant), typeof(NavigationBarVariant), typeof(NavigationBar),
        new FrameworkPropertyMetadata(NavigationBarVariant.Floating, OnChromeChanged));

    /// <summary>Identifies the <see cref="Orientation"/> property.</summary>
    public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register(
        nameof(Orientation), typeof(Orientation), typeof(NavigationBar),
        new FrameworkPropertyMetadata(Orientation.Horizontal,
            FrameworkPropertyMetadataOptions.AffectsMeasure, OnOrientationChanged));

    /// <summary>Identifies the <see cref="ButtonSize"/> property.</summary>
    public static readonly DependencyProperty ButtonSizeProperty = DependencyProperty.Register(
        nameof(ButtonSize), typeof(double), typeof(NavigationBar),
        new FrameworkPropertyMetadata(26.0, FrameworkPropertyMetadataOptions.AffectsMeasure, OnButtonSizeChanged));

    /// <summary>Identifies the <see cref="IconSize"/> property.</summary>
    public static readonly DependencyProperty IconSizeProperty = DependencyProperty.Register(
        nameof(IconSize), typeof(double), typeof(NavigationBar),
        new FrameworkPropertyMetadata(14.0, FrameworkPropertyMetadataOptions.AffectsMeasure, OnButtonSizeChanged));

    /// <summary>Identifies the <see cref="CornerRadius"/> property.</summary>
    public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register(
        nameof(CornerRadius), typeof(CornerRadius), typeof(NavigationBar),
        new FrameworkPropertyMetadata(new CornerRadius(6), OnChromeChanged));

    /// <summary>Identifies the <see cref="Elevation"/> property.</summary>
    public static readonly DependencyProperty ElevationProperty = DependencyProperty.Register(
        nameof(Elevation), typeof(int), typeof(NavigationBar),
        new FrameworkPropertyMetadata(1, OnChromeChanged));

    /// <summary>Identifies the <see cref="ShowUcsLabel"/> property.</summary>
    public static readonly DependencyProperty ShowUcsLabelProperty = DependencyProperty.Register(
        nameof(ShowUcsLabel), typeof(bool), typeof(NavigationBar),
        new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsMeasure, OnGroupVisibilityChanged));

    /// <summary>Identifies the <see cref="ShowZoom"/> property.</summary>
    public static readonly DependencyProperty ShowZoomProperty = DependencyProperty.Register(
        nameof(ShowZoom), typeof(bool), typeof(NavigationBar),
        new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsMeasure, OnGroupVisibilityChanged));

    /// <summary>Identifies the <see cref="ShowPerspective"/> property.</summary>
    public static readonly DependencyProperty ShowPerspectiveProperty = DependencyProperty.Register(
        nameof(ShowPerspective), typeof(bool), typeof(NavigationBar),
        new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsMeasure, OnGroupVisibilityChanged));

    /// <summary>Identifies the <see cref="ShowDividers"/> property.</summary>
    public static readonly DependencyProperty ShowDividersProperty = DependencyProperty.Register(
        nameof(ShowDividers), typeof(bool), typeof(NavigationBar),
        new FrameworkPropertyMetadata(true, OnGroupVisibilityChanged));

    private readonly Border _frame;
    private readonly StackPanel _strip;
    private readonly StackPanel _zoom;
    private readonly StackPanel _tools;
    private readonly Button _ucs;
    private readonly Button _perspective;
    private readonly Button[] _commands;
    private readonly ToggleButton[] _toolButtons;
    private readonly Border[] _dividers;

    private readonly ToggleButton _pan;
    private readonly ToggleButton _orbit;
    private readonly ToggleButton _walk;

    public NavigationBar()
    {
        Background = Brushes.Transparent;
        HorizontalAlignment = HorizontalAlignment.Left;
        VerticalAlignment = VerticalAlignment.Top;
        Margin = new Thickness(8);

        _strip = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(2) };

        _ucs = new Button
        {
            Content = "World",
            Margin = new Thickness(0, 0, 2, 0),
            Padding = new Thickness(7, 0, 7, 0),
            ToolTip = "Coordinate system"
        };

        _ucs.SetResourceReference(StyleProperty, "CONCOST.Button.NavigationLabel");
        _ucs.Click += (_, _) => OpenCoordinateMenu();

        var zoomIn = CommandButton(IconKind.ZoomIn, "Zoom in", NavigationCommand.ZoomIn);
        var zoomOut = CommandButton(IconKind.ZoomOut, "Zoom out", NavigationCommand.ZoomOut);
        var zoomFit = CommandButton(IconKind.ZoomFit, "Zoom to fit", NavigationCommand.ZoomExtents);

        _zoom = new StackPanel { Orientation = Orientation.Horizontal };
        _zoom.Children.Add(zoomIn);
        _zoom.Children.Add(zoomOut);
        _zoom.Children.Add(zoomFit);

        _pan = ToolToggle(IconKind.Pan3D, "Pan", NavigationTool.Pan);
        _orbit = ToolToggle(IconKind.Orbit3D, "Orbit", NavigationTool.Orbit);
        _walk = ToolToggle(IconKind.Walk3D, "Walk", NavigationTool.Walk);

        _tools = new StackPanel { Orientation = Orientation.Horizontal };
        _tools.Children.Add(_pan);
        _tools.Children.Add(_orbit);
        _tools.Children.Add(_walk);

        _perspective = CommandButton(IconKind.Viewport3D, "Perspective / orthographic", NavigationCommand.Perspective);

        _commands = new[] { zoomIn, zoomOut, zoomFit, _perspective };
        _toolButtons = new[] { _pan, _orbit, _walk };
        _dividers = new[] { Divider(), Divider(), Divider() };

        _strip.Children.Add(_ucs);
        _strip.Children.Add(_dividers[0]);
        _strip.Children.Add(_zoom);
        _strip.Children.Add(_dividers[1]);
        _strip.Children.Add(_tools);
        _strip.Children.Add(_dividers[2]);
        _strip.Children.Add(_perspective);

        _frame = new Border { Background = Brushes.Transparent, Child = _strip };

        Children.Add(_frame);

        // The label lives in a fixed slot so the strip does not jump when the mode changes.
        ApplyUcsLabel();
        ApplyChrome();
        ApplyOrientation();
        ApplyButtonSize();
        ApplyGroupVisibility();
    }

    /// <summary>Raised when a one-shot button is pressed.</summary>
    public event EventHandler<NavigationRequestedEventArgs>? NavigationRequested;

    /// <summary>Raised when the active tool changes, including when it is turned off.</summary>
    public event EventHandler? ActiveToolChanged;

    /// <summary>Raised when the coordinate system changes.</summary>
    public event EventHandler? UcsChanged;

    /// <summary>The tool that is currently armed. Pan, orbit and walk are mutually exclusive.</summary>
    public NavigationTool ActiveTool
    {
        get => (NavigationTool)GetValue(ActiveToolProperty);
        set => SetValue(ActiveToolProperty, value);
    }

    /// <summary>The orientation the view is in, as shown on the coordinate-system button.</summary>
    public UcsMode Ucs
    {
        get => (UcsMode)GetValue(UcsProperty);
        set => SetValue(UcsProperty, value);
    }

    /// <summary>Shows the walk tool. Hidden by default in hosts that have no walk controller.</summary>
    public bool ShowWalk
    {
        get => (bool)GetValue(ShowWalkProperty);
        set => SetValue(ShowWalkProperty, value);
    }

    /// <summary>
    /// How the bar is dressed: <see cref="NavigationBarVariant.Floating"/> over a viewport,
    /// <see cref="NavigationBarVariant.Seamless"/> inside a toolbar, <see cref="NavigationBarVariant.Glass"/>
    /// over a busy 3D scene.
    /// </summary>
    public NavigationBarVariant Variant
    {
        get => (NavigationBarVariant)GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }

    /// <summary>Lays the commands out along a row or down a column. The dividers follow.</summary>
    public Orientation Orientation
    {
        get => (Orientation)GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    /// <summary>Square size of every command button, in pixels. 26 is the CAD default; 34 is a touch target.</summary>
    public double ButtonSize
    {
        get => (double)GetValue(ButtonSizeProperty);
        set => SetValue(ButtonSizeProperty, value);
    }

    /// <summary>Size of the glyph inside a command button, in pixels.</summary>
    public double IconSize
    {
        get => (double)GetValue(IconSizeProperty);
        set => SetValue(IconSizeProperty, value);
    }

    /// <summary>Corner radius of the frame. Half the height makes a pill.</summary>
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    /// <summary>Shadow depth of the frame, 0 to 4. Ignored by the seamless variant.</summary>
    public int Elevation
    {
        get => (int)GetValue(ElevationProperty);
        set => SetValue(ElevationProperty, value);
    }

    /// <summary>Shows the coordinate-system label and its menu. Off leaves a bar of arrows only.</summary>
    public bool ShowUcsLabel
    {
        get => (bool)GetValue(ShowUcsLabelProperty);
        set => SetValue(ShowUcsLabelProperty, value);
    }

    /// <summary>Shows the zoom in / zoom out / zoom to fit cluster.</summary>
    public bool ShowZoom
    {
        get => (bool)GetValue(ShowZoomProperty);
        set => SetValue(ShowZoomProperty, value);
    }

    /// <summary>Shows the perspective / orthographic button at the end of the bar.</summary>
    public bool ShowPerspective
    {
        get => (bool)GetValue(ShowPerspectiveProperty);
        set => SetValue(ShowPerspectiveProperty, value);
    }

    /// <summary>
    /// Shows the rules between groups. A rule also steps aside on its own when one of the two groups
    /// beside it is hidden, so a collapsed cluster never leaves a floating line behind.
    /// </summary>
    public bool ShowDividers
    {
        get => (bool)GetValue(ShowDividersProperty);
        set => SetValue(ShowDividersProperty, value);
    }

    /// <summary>
    /// Arms <paramref name="tool"/>, or turns everything off when it is already active. Public so a
    /// host can mirror its own Escape handling onto the bar.
    /// </summary>
    public void SetActiveTool(NavigationTool tool) => ActiveTool = ActiveTool == tool ? NavigationTool.None : tool;

    private static void OnActiveToolChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not NavigationBar bar) return;

        bar._pan.IsChecked = (NavigationTool)e.NewValue == NavigationTool.Pan;
        bar._orbit.IsChecked = (NavigationTool)e.NewValue == NavigationTool.Orbit;
        bar._walk.IsChecked = (NavigationTool)e.NewValue == NavigationTool.Walk;

        bar.ActiveToolChanged?.Invoke(bar, EventArgs.Empty);
    }

    private static void OnUcsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not NavigationBar bar) return;

        bar.ApplyUcsLabel();
        bar.UcsChanged?.Invoke(bar, EventArgs.Empty);
    }

    private static void OnChromeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NavigationBar bar) bar.ApplyChrome();
    }

    private static void OnOrientationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NavigationBar bar) bar.ApplyOrientation();
    }

    private static void OnButtonSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NavigationBar bar) bar.ApplyButtonSize();
    }

    private static void OnGroupVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NavigationBar bar) bar.ApplyGroupVisibility();
    }

    /// <summary>Dresses the frame. Only this frame draws an outline; the commands inside stay flat.</summary>
    private void ApplyChrome()
    {
        _frame.CornerRadius = CornerRadius;
        _frame.Opacity = 1;

        switch (Variant)
        {
            case NavigationBarVariant.Seamless:
                // No plate at all: the bar borrows the surface it is docked on.
                _frame.SetValue(Border.BackgroundProperty, Brushes.Transparent);
                _frame.BorderThickness = new Thickness(0);
                VisualElement.SetElevation(_frame, 0);
                break;

            case NavigationBarVariant.Glass:
                // A translucent plate, so the model underneath still reads through it.
                _frame.SetResourceReference(Border.BackgroundProperty, "CONCOST.Brush.Surface");
                _frame.BorderThickness = new Thickness(0);
                _frame.Opacity = 0.9;
                VisualElement.SetElevation(_frame, Math.Max(2, Elevation));
                break;

            default:
                _frame.SetResourceReference(Border.BackgroundProperty, "CONCOST.Brush.Surface");
                _frame.SetResourceReference(Border.BorderBrushProperty, "CONCOST.Brush.Border");
                _frame.BorderThickness = new Thickness(1);
                VisualElement.SetElevation(_frame, Elevation);
                break;
        }
    }

    private void ApplyOrientation()
    {
        bool vertical = Orientation == Orientation.Vertical;

        _strip.Orientation = Orientation;
        _zoom.Orientation = Orientation;
        _tools.Orientation = Orientation;

        foreach (var divider in _dividers)
        {
            // The rule stretches across the bars and is one pixel along it, whichever way it runs.
            divider.Width = vertical ? double.NaN : 1;
            divider.Height = vertical ? 1 : double.NaN;
            divider.Margin = vertical ? new Thickness(5, 3, 5, 3) : new Thickness(3, 5, 3, 5);
        }
    }

    private void ApplyButtonSize()
    {
        double size = double.IsNaN(ButtonSize) || ButtonSize < 16 ? 16 : ButtonSize;
        double icon = double.IsNaN(IconSize) || IconSize < 8 ? 8 : IconSize;

        foreach (var button in _commands)
        {
            ResizeButton(button, size);
            IconElement.SetIconSize(button, icon);
        }

        foreach (var toggle in _toolButtons)
        {
            ResizeButton(toggle, size);
            IconElement.SetIconSize(toggle, icon);
        }

        // The label is a text button: it keeps its height and takes the row's width.
        _ucs.Height = size;
        _ucs.MinHeight = 0;
        _ucs.FontSize = Math.Max(10, Math.Round(size * 0.42));
    }

    private static void ResizeButton(FrameworkElement button, double size)
    {
        // The ghost styles ask for a 34 px touch target; the bar decides for itself.
        button.MinWidth = 0;
        button.MinHeight = 0;
        button.Width = size;
        button.Height = size;
    }

    private void ApplyGroupVisibility()
    {
        _ucs.Visibility = ShowUcsLabel ? Visibility.Visible : Visibility.Collapsed;
        _zoom.Visibility = ShowZoom ? Visibility.Visible : Visibility.Collapsed;
        _walk.Visibility = ShowWalk ? Visibility.Visible : Visibility.Collapsed;
        _perspective.Visibility = ShowPerspective ? Visibility.Visible : Visibility.Collapsed;

        RefreshDividers();
    }

    private void RefreshDividers()
    {
        bool on = ShowDividers;

        _dividers[0].Visibility = on && Shows(_ucs) && Shows(_zoom) ? Visibility.Visible : Visibility.Collapsed;
        _dividers[1].Visibility = on && Shows(_zoom) && Shows(_tools) ? Visibility.Visible : Visibility.Collapsed;
        _dividers[2].Visibility = on && Shows(_tools) && Shows(_perspective) ? Visibility.Visible : Visibility.Collapsed;
    }

    private static bool Shows(UIElement element) => element.Visibility == Visibility.Visible;

    private void ApplyUcsLabel() => _ucs.Content = Ucs switch
    {
        UcsMode.Top => "Top",
        UcsMode.Front => "Front",
        UcsMode.Right => "Right",
        _ => "World"
    };

    private void OpenCoordinateMenu()
    {
        var menu = new ContextMenu { PlacementTarget = _ucs, Placement = PlacementMode.Top };

        foreach (UcsMode mode in Enum.GetValues(typeof(UcsMode)))
        {
            var item = new MenuItem { Header = mode.ToString(), IsCheckable = true, IsChecked = Ucs == mode };
            var captured = mode;

            item.Click += (_, _) => Ucs = captured;
            menu.Items.Add(item);
        }

        menu.IsOpen = true;
    }

    private Button CommandButton(IconKind kind, string tooltip, NavigationCommand command)
    {
        var button = new Button
        {
            Margin = new Thickness(1, 0, 1, 0),
            ToolTip = tooltip
        };

        button.SetResourceReference(StyleProperty, "CONCOST.Button.Icon.Ghost");
        IconElement.SetKind(button, kind);

        button.Click += (_, _) => NavigationRequested?.Invoke(this, new NavigationRequestedEventArgs(command));

        return button;
    }

    private ToggleButton ToolToggle(IconKind kind, string tooltip, NavigationTool tool)
    {
        var toggle = new ToggleButton
        {
            Margin = new Thickness(1, 0, 1, 0),
            ToolTip = tooltip
        };

        toggle.SetResourceReference(StyleProperty, "CONCOST.ToggleButton.Icon.Ghost");
        IconElement.SetKind(toggle, kind);

        toggle.Click += (_, _) => SetActiveTool(tool);

        return toggle;
    }

    private static Border Divider()
    {
        var divider = new Border
        {
            Width = 1,
            Margin = new Thickness(3, 5, 3, 5),
            SnapsToDevicePixels = true
        };

        // A fixed grey rule turned into a bright line the moment the theme went dark.
        divider.SetResourceReference(Border.BackgroundProperty, "CONCOST.Brush.Border");

        return divider;
    }
}
