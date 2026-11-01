using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using CONCOST.UI.AttachedProperties;
using CONCOST.UI.Icons;

namespace CONCOST.UI.Controls;

/// <summary>How much detail a view draws.</summary>
public enum ViewDetailLevel
{
    Coarse,
    Medium,
    Fine
}

/// <summary>The drawing style of a 3D view.</summary>
public enum VisualStyleMode
{
    Wireframe,
    HiddenLine,
    Shaded,
    Realistic
}

/// <summary>A hide or isolate request from the view control bar.</summary>
public enum HideIsolateAction
{
    HideElements,
    HideCategory,
    IsolateElements,
    IsolateCategory,
    Reset
}

/// <summary>How a <see cref="ViewControlBar"/> is dressed.</summary>
public enum ViewControlBarVariant
{
    /// <summary>A surface plate with one outline and a shadow: the bar that floats over a view.</summary>
    Floating,

    /// <summary>No frame at all, only the flat controls: the bar that docks in a status bar.</summary>
    Seamless,

    /// <summary>A translucent plate for a view drawn behind it, where an opaque one hides the model.</summary>
    Glass
}

/// <summary>Carries a hide or isolate request.</summary>
public class HideIsolateEventArgs : EventArgs
{
    public HideIsolateEventArgs(HideIsolateAction action) => Action = action;

    /// <summary>What the user asked for.</summary>
    public HideIsolateAction Action { get; }
}

/// <summary>
/// The bar that lives at the bottom of a view: scale, detail level, visual style, shadows, crop,
/// hide/isolate and reveal hidden.
/// </summary>
/// <remarks>
/// <para>
/// It holds the state the spec calls view properties, not a copy of the view's camera: every change
/// raises <see cref="SettingsChanged"/> so the host can push it into its view and into the properties
/// palette, which is what AC-013 asks for. The bar never decides anything on its own beyond keeping
/// its own controls in step.
/// </para>
/// <para>
/// Exactly one outline belongs to the bar: the frame. The pickers keep their own edge because a
/// combo box without one is not recognisable as a field, but the icon toggles and the hide button are
/// flat until they are hovered or ticked — a row of outlined chips inside an outlined plate reads as
/// a table of boxes, not as one tool. <see cref="Variant"/> decides whether the frame exists at all;
/// see <see cref="ViewControlBarVariant"/>.
/// </para>
/// </remarks>
public class ViewControlBar : Grid
{
    /// <summary>Identifies the <see cref="ViewScale"/> property.</summary>
    public static readonly DependencyProperty ViewScaleProperty = DependencyProperty.Register(
        nameof(ViewScale), typeof(string), typeof(ViewControlBar), new FrameworkPropertyMetadata("1:100"));

    /// <summary>Identifies the <see cref="DetailLevel"/> property.</summary>
    public static readonly DependencyProperty DetailLevelProperty = DependencyProperty.Register(
        nameof(DetailLevel), typeof(ViewDetailLevel), typeof(ViewControlBar),
        new FrameworkPropertyMetadata(ViewDetailLevel.Medium));

    /// <summary>Identifies the <see cref="VisualStyle"/> property.</summary>
    public static readonly DependencyProperty VisualStyleProperty = DependencyProperty.Register(
        nameof(VisualStyle), typeof(VisualStyleMode), typeof(ViewControlBar),
        new FrameworkPropertyMetadata(VisualStyleMode.HiddenLine));

    /// <summary>Identifies the <see cref="ShowShadows"/> property.</summary>
    public static readonly DependencyProperty ShowShadowsProperty = DependencyProperty.Register(
        nameof(ShowShadows), typeof(bool), typeof(ViewControlBar), new FrameworkPropertyMetadata(false));

    /// <summary>Identifies the <see cref="CropView"/> property.</summary>
    public static readonly DependencyProperty CropViewProperty = DependencyProperty.Register(
        nameof(CropView), typeof(bool), typeof(ViewControlBar), new FrameworkPropertyMetadata(false));

    /// <summary>Identifies the <see cref="RevealHidden"/> property.</summary>
    public static readonly DependencyProperty RevealHiddenProperty = DependencyProperty.Register(
        nameof(RevealHidden), typeof(bool), typeof(ViewControlBar), new FrameworkPropertyMetadata(false));

    /// <summary>Identifies the <see cref="Variant"/> property.</summary>
    public static readonly DependencyProperty VariantProperty = DependencyProperty.Register(
        nameof(Variant), typeof(ViewControlBarVariant), typeof(ViewControlBar),
        new FrameworkPropertyMetadata(ViewControlBarVariant.Floating, OnChromeChanged));

    /// <summary>Identifies the <see cref="CornerRadius"/> property.</summary>
    public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register(
        nameof(CornerRadius), typeof(CornerRadius), typeof(ViewControlBar),
        new FrameworkPropertyMetadata(new CornerRadius(6), OnChromeChanged));

    /// <summary>Identifies the <see cref="Elevation"/> property.</summary>
    public static readonly DependencyProperty ElevationProperty = DependencyProperty.Register(
        nameof(Elevation), typeof(int), typeof(ViewControlBar),
        new FrameworkPropertyMetadata(1, OnChromeChanged));

    /// <summary>Identifies the <see cref="ControlHeight"/> property.</summary>
    public static readonly DependencyProperty ControlHeightProperty = DependencyProperty.Register(
        nameof(ControlHeight), typeof(double), typeof(ViewControlBar),
        new FrameworkPropertyMetadata(26.0, FrameworkPropertyMetadataOptions.AffectsMeasure, OnControlHeightChanged));

    /// <summary>Identifies the <see cref="IconSize"/> property.</summary>
    public static readonly DependencyProperty IconSizeProperty = DependencyProperty.Register(
        nameof(IconSize), typeof(double), typeof(ViewControlBar),
        new FrameworkPropertyMetadata(14.0, FrameworkPropertyMetadataOptions.AffectsMeasure, OnControlHeightChanged));

    /// <summary>Identifies the <see cref="ShowDividers"/> property.</summary>
    public static readonly DependencyProperty ShowDividersProperty = DependencyProperty.Register(
        nameof(ShowDividers), typeof(bool), typeof(ViewControlBar),
        new FrameworkPropertyMetadata(true, OnGroupVisibilityChanged));

    /// <summary>Identifies the <see cref="ShowScale"/> property.</summary>
    public static readonly DependencyProperty ShowScaleProperty = DependencyProperty.Register(
        nameof(ShowScale), typeof(bool), typeof(ViewControlBar),
        new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsMeasure, OnGroupVisibilityChanged));

    /// <summary>Identifies the <see cref="ShowDetailLevel"/> property.</summary>
    public static readonly DependencyProperty ShowDetailLevelProperty = DependencyProperty.Register(
        nameof(ShowDetailLevel), typeof(bool), typeof(ViewControlBar),
        new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsMeasure, OnGroupVisibilityChanged));

    /// <summary>Identifies the <see cref="ShowVisualStyle"/> property.</summary>
    public static readonly DependencyProperty ShowVisualStyleProperty = DependencyProperty.Register(
        nameof(ShowVisualStyle), typeof(bool), typeof(ViewControlBar),
        new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsMeasure, OnGroupVisibilityChanged));

    /// <summary>Identifies the <see cref="ShowToggles"/> property.</summary>
    public static readonly DependencyProperty ShowTogglesProperty = DependencyProperty.Register(
        nameof(ShowToggles), typeof(bool), typeof(ViewControlBar),
        new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsMeasure, OnGroupVisibilityChanged));

    /// <summary>Identifies the <see cref="ShowHideIsolate"/> property.</summary>
    public static readonly DependencyProperty ShowHideIsolateProperty = DependencyProperty.Register(
        nameof(ShowHideIsolate), typeof(bool), typeof(ViewControlBar),
        new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsMeasure, OnGroupVisibilityChanged));

    /// <summary>Identifies the <see cref="Scales"/> property.</summary>
    public static readonly DependencyProperty ScalesProperty = DependencyProperty.Register(
        nameof(Scales), typeof(IEnumerable<string>), typeof(ViewControlBar),
        new FrameworkPropertyMetadata(null, OnScalesChanged));

    /// <summary>Identifies the <see cref="HideIsolateCaption"/> property.</summary>
    public static readonly DependencyProperty HideIsolateCaptionProperty = DependencyProperty.Register(
        nameof(HideIsolateCaption), typeof(string), typeof(ViewControlBar),
        new FrameworkPropertyMetadata("Hide", OnCaptionChanged));

    /// <summary>The five scales a plan view is normally read at.</summary>
    private static readonly string[] DefaultScales = { "1:50", "1:100", "1:200", "1:500", "1:1000" };

    private readonly Border _frame;
    private readonly StackPanel _strip;
    private readonly ComboBox _scale;
    private readonly ComboBox _detail;
    private readonly ComboBox _style;
    private readonly StackPanel _toggles;
    private readonly ToggleButton _shadows;
    private readonly ToggleButton _crop;
    private readonly ToggleButton _reveal;
    private readonly SplitButton _hide;
    private readonly ToggleButton[] _toolButtons;
    private readonly Border[] _dividers;

    private IEnumerable<string>? _scales;
    private bool _syncing;

    public ViewControlBar()
    {
        Background = Brushes.Transparent;
        HorizontalAlignment = HorizontalAlignment.Left;
        VerticalAlignment = VerticalAlignment.Bottom;
        Margin = new Thickness(8);

        _scale = Combo(76, "View scale");
        _detail = Combo(92, "Detail level");
        _style = Combo(112, "Visual style");

        _shadows = IconToggle(IconKind.Shadows3D, "Cast shadows");
        _crop = IconToggle(IconKind.GridSquare, "Crop view");
        _reveal = IconToggle(IconKind.EyeOff, "Reveal hidden elements");

        _toolButtons = new[] { _shadows, _crop, _reveal };

        _toggles = new StackPanel();
        _toggles.Children.Add(_shadows);
        _toggles.Children.Add(_crop);
        _toggles.Children.Add(_reveal);

        _hide = HideIsolateButton();
        _dividers = new[] { Divider(), Divider() };

        _scale.SelectionChanged += (_, _) =>
        {
            if (_syncing || _scale.SelectedItem is not string text) return;

            SetCurrentValue(ViewScaleProperty, text);
        };

        _detail.SelectionChanged += (_, _) =>
        {
            if (_syncing || _detail.SelectedIndex < 0) return;

            SetCurrentValue(DetailLevelProperty, (ViewDetailLevel)_detail.SelectedIndex);
        };

        _style.SelectionChanged += (_, _) =>
        {
            if (_syncing || _style.SelectedIndex < 0) return;

            SetCurrentValue(VisualStyleProperty, (VisualStyleMode)_style.SelectedIndex);
        };

        _shadows.Click += (_, _) => SetCurrentValue(ShowShadowsProperty, _shadows.IsChecked == true);
        _crop.Click += (_, _) => SetCurrentValue(CropViewProperty, _crop.IsChecked == true);
        _reveal.Click += (_, _) => SetCurrentValue(RevealHiddenProperty, _reveal.IsChecked == true);

        _strip = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(3) };

        _strip.Children.Add(_scale);
        _strip.Children.Add(_detail);
        _strip.Children.Add(_style);
        _strip.Children.Add(_dividers[0]);
        _strip.Children.Add(_toggles);
        _strip.Children.Add(_dividers[1]);
        _strip.Children.Add(_hide);

        _frame = new Border { Child = _strip };

        Children.Add(_frame);

        ApplyScales();
        ApplyCaption();
        ApplyChrome();
        ApplyControlHeight();
        ApplyGroupVisibility();
    }

    /// <summary>Raised after any setting changes, with the property name for the argument.</summary>
    public event EventHandler<string>? SettingsChanged;

    /// <summary>Raised when hide, isolate or reset is picked.</summary>
    public event EventHandler<HideIsolateEventArgs>? HideIsolateRequested;

    /// <summary>Scale text shown in the bar, e.g. <c>1:100</c>. Free text: a host can add its own scales.</summary>
    public string ViewScale
    {
        get => (string)GetValue(ViewScaleProperty);
        set => SetValue(ViewScaleProperty, value);
    }

    /// <summary>Coarse, medium or fine.</summary>
    public ViewDetailLevel DetailLevel
    {
        get => (ViewDetailLevel)GetValue(DetailLevelProperty);
        set => SetValue(DetailLevelProperty, value);
    }

    /// <summary>Wireframe, hidden line, shaded or realistic.</summary>
    public VisualStyleMode VisualStyle
    {
        get => (VisualStyleMode)GetValue(VisualStyleProperty);
        set => SetValue(VisualStyleProperty, value);
    }

    /// <summary>Whether the view casts shadows.</summary>
    public bool ShowShadows
    {
        get => (bool)GetValue(ShowShadowsProperty);
        set => SetValue(ShowShadowsProperty, value);
    }

    /// <summary>Whether the view is cropped to its crop region.</summary>
    public bool CropView
    {
        get => (bool)GetValue(CropViewProperty);
        set => SetValue(CropViewProperty, value);
    }

    /// <summary>Whether hidden elements are drawn in magenta for editing.</summary>
    public bool RevealHidden
    {
        get => (bool)GetValue(RevealHiddenProperty);
        set => SetValue(RevealHiddenProperty, value);
    }

    /// <summary>
    /// How the bar is dressed: <see cref="ViewControlBarVariant.Floating"/> over a view,
    /// <see cref="ViewControlBarVariant.Seamless"/> inside a status bar,
    /// <see cref="ViewControlBarVariant.Glass"/> over a drawing that must stay readable.
    /// </summary>
    public ViewControlBarVariant Variant
    {
        get => (ViewControlBarVariant)GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
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

    /// <summary>Height of every picker, toggle and button on the bar, in pixels.</summary>
    public double ControlHeight
    {
        get => (double)GetValue(ControlHeightProperty);
        set => SetValue(ControlHeightProperty, value);
    }

    /// <summary>Size of the glyph inside a toggle or a button, in pixels.</summary>
    public double IconSize
    {
        get => (double)GetValue(IconSizeProperty);
        set => SetValue(IconSizeProperty, value);
    }

    /// <summary>
    /// Shows the rules between the pickers, the toggles and the hide button. A rule also steps aside
    /// on its own when one of the two groups beside it is hidden.
    /// </summary>
    public bool ShowDividers
    {
        get => (bool)GetValue(ShowDividersProperty);
        set => SetValue(ShowDividersProperty, value);
    }

    /// <summary>Shows the view scale picker.</summary>
    public bool ShowScale
    {
        get => (bool)GetValue(ShowScaleProperty);
        set => SetValue(ShowScaleProperty, value);
    }

    /// <summary>Shows the detail level picker.</summary>
    public bool ShowDetailLevel
    {
        get => (bool)GetValue(ShowDetailLevelProperty);
        set => SetValue(ShowDetailLevelProperty, value);
    }

    /// <summary>Shows the visual style picker.</summary>
    public bool ShowVisualStyle
    {
        get => (bool)GetValue(ShowVisualStyleProperty);
        set => SetValue(ShowVisualStyleProperty, value);
    }

    /// <summary>Shows the shadows, crop and reveal-hidden toggles as one group.</summary>
    public bool ShowToggles
    {
        get => (bool)GetValue(ShowTogglesProperty);
        set => SetValue(ShowTogglesProperty, value);
    }

    /// <summary>Shows the hide / isolate split button.</summary>
    public bool ShowHideIsolate
    {
        get => (bool)GetValue(ShowHideIsolateProperty);
        set => SetValue(ShowHideIsolateProperty, value);
    }

    /// <summary>
    /// The scales offered by the dropdown. <c>null</c> means the five a plan is normally read at
    /// (1:50 … 1:1000). An observable list re-fills the dropdown as it changes.
    /// </summary>
    public IEnumerable<string>? Scales
    {
        get => (IEnumerable<string>?)GetValue(ScalesProperty);
        set => SetValue(ScalesProperty, value);
    }

    /// <summary>Caption of the hide / isolate button. Set it to translate the bar.</summary>
    public string HideIsolateCaption
    {
        get => (string)GetValue(HideIsolateCaptionProperty);
        set => SetValue(HideIsolateCaptionProperty, value);
    }

    /// <summary>
    /// Pushes the property values back into the controls. Called automatically after a property
    /// changes from code; exposed because a host may bind the bar lazily.
    /// </summary>
    public void SyncFromProperties()
    {
        EnsureScaleIsListed();

        _syncing = true;

        try
        {
            if (_scale.Items.Contains(ViewScale)) _scale.SelectedItem = ViewScale;
            else _scale.Text = ViewScale;

            _detail.SelectedIndex = (int)DetailLevel;
            _style.SelectedIndex = (int)VisualStyle;

            _shadows.IsChecked = ShowShadows;
            _crop.IsChecked = CropView;
            _reveal.IsChecked = RevealHidden;
        }
        finally
        {
            _syncing = false;
        }
    }

    private static void OnChromeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ViewControlBar bar) bar.ApplyChrome();
    }

    private static void OnControlHeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ViewControlBar bar) bar.ApplyControlHeight();
    }

    private static void OnGroupVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ViewControlBar bar) bar.ApplyGroupVisibility();
    }

    private static void OnScalesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ViewControlBar bar) bar.ApplyScales();
    }

    private static void OnCaptionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ViewControlBar bar) bar.ApplyCaption();
    }

    /// <summary>Dresses the frame. Only this frame draws an outline; the toggles inside stay flat.</summary>
    private void ApplyChrome()
    {
        _frame.CornerRadius = CornerRadius;
        _frame.Opacity = 1;

        switch (Variant)
        {
            case ViewControlBarVariant.Seamless:
                // No plate at all: the bar borrows the status bar it is docked in.
                _frame.SetValue(Border.BackgroundProperty, Brushes.Transparent);
                _frame.BorderThickness = new Thickness(0);
                VisualElement.SetElevation(_frame, 0);
                break;

            case ViewControlBarVariant.Glass:
                // A translucent plate, so the drawing underneath still reads through it.
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

    private void ApplyControlHeight()
    {
        double height = double.IsNaN(ControlHeight) || ControlHeight < 20 ? 20 : ControlHeight;
        double icon = double.IsNaN(IconSize) || IconSize < 8 ? 8 : IconSize;
        double font = Math.Max(10, Math.Round(height * 0.42));

        foreach (var combo in new[] { _scale, _detail, _style })
        {
            // The combo style asks for a 34 px row; the bar decides for itself, height first
            // because a minimum wins over it.
            combo.MinHeight = 0;
            combo.Height = height;
            combo.FontSize = font;
        }

        foreach (var toggle in _toolButtons)
        {
            toggle.MinWidth = 0;
            toggle.MinHeight = 0;
            toggle.Width = height;
            toggle.Height = height;
            IconElement.SetIconSize(toggle, icon);
        }

        _hide.MinHeight = 0;
        _hide.Height = height;
        _hide.FontSize = font;
        _hide.IconSize = icon;

        foreach (var divider in _dividers)
        {
            double air = Math.Round(height * 0.15);

            divider.Margin = new Thickness(3, air, 3, air);
        }
    }

    private void ApplyGroupVisibility()
    {
        _scale.Visibility = ShowScale ? Visibility.Visible : Visibility.Collapsed;
        _detail.Visibility = ShowDetailLevel ? Visibility.Visible : Visibility.Collapsed;
        _style.Visibility = ShowVisualStyle ? Visibility.Visible : Visibility.Collapsed;
        _toggles.Visibility = ShowToggles ? Visibility.Visible : Visibility.Collapsed;
        _hide.Visibility = ShowHideIsolate ? Visibility.Visible : Visibility.Collapsed;

        RefreshDividers();
    }

    private void RefreshDividers()
    {
        bool on = ShowDividers;

        // A rule also leaves with the group beside it, so a collapsed cluster never leaves a
        // floating line at the end of the bar.
        _dividers[0].Visibility = on && Shows(_style) && Shows(_toggles) ? Visibility.Visible : Visibility.Collapsed;
        _dividers[1].Visibility = on && Shows(_toggles) && Shows(_hide) ? Visibility.Visible : Visibility.Collapsed;
    }

    private static bool Shows(UIElement element) => element.Visibility == Visibility.Visible;

    private void ApplyScales()
    {
        if (_scales is INotifyCollectionChanged previous) previous.CollectionChanged -= OnScalesReplaced;

        _scales = Scales;

        if (_scales is INotifyCollectionChanged live) live.CollectionChanged += OnScalesReplaced;

        RebuildScaleItems();
    }

    private void OnScalesReplaced(object? sender, NotifyCollectionChangedEventArgs e) => RebuildScaleItems();

    private void RebuildScaleItems()
    {
        var items = new List<string>();

        foreach (var scale in Scales ?? DefaultScales)
        {
            if (!string.IsNullOrWhiteSpace(scale) && !items.Contains(scale)) items.Add(scale);
        }

        _scale.ItemsSource = items;

        SyncFromProperties();
    }

    /// <summary>
    /// Grows the dropdown when a host restores a view saved on a scale the list does not offer.
    /// A saved plan read at 1:250 must not leave the box blank.
    /// </summary>
    private void EnsureScaleIsListed()
    {
        if (_scale.ItemsSource is not List<string> items || items.Contains(ViewScale)) return;

        items.Add(ViewScale);

        // WPF ignores a re-assignment of the same source, so the list is copied to force a reload.
        _scale.ItemsSource = new List<string>(items);
    }

    private void ApplyCaption() =>
        _hide.Content = string.IsNullOrWhiteSpace(HideIsolateCaption) ? "Hide" : HideIsolateCaption;

    private ComboBox Combo(double width, string tooltip)
    {
        var combo = new ComboBox
        {
            Width = width,
            Height = 26,
            Margin = new Thickness(0, 0, 3, 0),
            FontSize = 11,
            ToolTip = tooltip,
            ItemsSource = new List<string>()
        };

        return combo;
    }

    private static ToggleButton IconToggle(IconKind kind, string tooltip)
    {
        var toggle = new ToggleButton
        {
            Margin = new Thickness(1, 0, 1, 0),
            ToolTip = tooltip
        };

        toggle.SetResourceReference(StyleProperty, "CONCOST.ToggleButton.Icon.Ghost");
        IconElement.SetKind(toggle, kind);

        return toggle;
    }

    private SplitButton HideIsolateButton()
    {
        var button = new SplitButton
        {
            Content = "Hide",
            Icon = IconKind.HideElement,
            MinWidth = 72,
            ToolTip = "Hide or isolate elements and categories"
        };

        // Ghost, not Secondary: an outlined chip inside an outlined plate is the double border the
        // bar used to show. The accent stays reserved for the current selection.
        button.SetResourceReference(StyleProperty, "CONCOST.SplitButton.Ghost");

        var menu = new StackPanel { Width = 190 };

        void Add(HideIsolateAction action, string caption)
        {
            var item = new Button
            {
                Content = caption,
                HorizontalContentAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(0, 0, 0, 2),
                Padding = new Thickness(8, 4, 8, 4)
            };

            item.SetResourceReference(StyleProperty, "CONCOST.Button.Secondary");
            item.Click += (_, _) =>
            {
                button.IsDropDownOpen = false;
                HideIsolateRequested?.Invoke(this, new HideIsolateEventArgs(action));
            };

            menu.Children.Add(item);
        }

        Add(HideIsolateAction.HideElements, "Hide in view");
        Add(HideIsolateAction.HideCategory, "Hide category");
        Add(HideIsolateAction.IsolateElements, "Isolate elements");
        Add(HideIsolateAction.IsolateCategory, "Isolate category");
        Add(HideIsolateAction.Reset, "Reset temporary hide/isolate");

        button.DropDownContent = new Border { Padding = new Thickness(4), Child = menu };

        // The main part repeats the most common action instead of doing nothing.
        button.Click += (_, _) => HideIsolateRequested?.Invoke(this, new HideIsolateEventArgs(HideIsolateAction.HideElements));

        return button;
    }

    private void RaiseSettings(string property) => SettingsChanged?.Invoke(this, property);

    private static string? NameOf(DependencyProperty property)
    {
        if (property == ViewScaleProperty) return nameof(ViewScale);
        if (property == DetailLevelProperty) return nameof(DetailLevel);
        if (property == VisualStyleProperty) return nameof(VisualStyle);
        if (property == ShowShadowsProperty) return nameof(ShowShadows);
        if (property == CropViewProperty) return nameof(CropView);
        if (property == RevealHiddenProperty) return nameof(RevealHidden);

        return null;
    }

    private static Border Divider()
    {
        var divider = new Border
        {
            Width = 1,
            Margin = new Thickness(3, 4, 3, 4),
            SnapsToDevicePixels = true
        };

        // A fixed grey rule turned into a bright line the moment the theme went dark.
        divider.SetResourceReference(Border.BackgroundProperty, "CONCOST.Brush.Border");

        return divider;
    }

    protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (_scale is null || _syncing) return;

        if (NameOf(e.Property) is not string changed) return;

        SyncFromProperties();

        // Raised for code and for the pointer alike: a host restoring a saved view must be told
        // just as one that clicked the toggle, which is the only way AC-013 can hold.
        RaiseSettings(changed);
    }
}
