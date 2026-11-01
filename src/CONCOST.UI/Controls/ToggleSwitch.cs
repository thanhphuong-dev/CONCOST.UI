using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace CONCOST.UI.Controls;

/// <summary>
/// A styled toggle switch control that extends <see cref="ToggleButton"/> with
/// rich visual customisation: track size, thumb size, per-state brushes, corner radius,
/// label placement, and an optional slide animation.
/// </summary>
public class ToggleSwitch : ToggleButton
{
    static ToggleSwitch()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(ToggleSwitch), new FrameworkPropertyMetadata(typeof(ToggleSwitch)));
    }

    // ------------------------------------------------------------------ On/Off label content

    /// <summary>Content shown beside the switch when it is checked.</summary>
    public static readonly DependencyProperty OnContentProperty =
        DependencyProperty.Register(nameof(OnContent), typeof(object), typeof(ToggleSwitch),
            new PropertyMetadata("On"));

    /// <summary>Content shown beside the switch when it is unchecked.</summary>
    public static readonly DependencyProperty OffContentProperty =
        DependencyProperty.Register(nameof(OffContent), typeof(object), typeof(ToggleSwitch),
            new PropertyMetadata("Off"));

    public object OnContent
    {
        get => GetValue(OnContentProperty);
        set => SetValue(OnContentProperty, value);
    }

    public object OffContent
    {
        get => GetValue(OffContentProperty);
        set => SetValue(OffContentProperty, value);
    }

    // ------------------------------------------------------------------ track dimensions

    /// <summary>Width of the track in device-independent pixels. Default 44.</summary>
    public static readonly DependencyProperty SwitchWidthProperty =
        DependencyProperty.Register(nameof(SwitchWidth), typeof(double), typeof(ToggleSwitch),
            new FrameworkPropertyMetadata(44.0, FrameworkPropertyMetadataOptions.AffectsMeasure));

    public double SwitchWidth
    {
        get => (double)GetValue(SwitchWidthProperty);
        set => SetValue(SwitchWidthProperty, value);
    }

    /// <summary>Height of the track in device-independent pixels. Default 22.</summary>
    public static readonly DependencyProperty SwitchHeightProperty =
        DependencyProperty.Register(nameof(SwitchHeight), typeof(double), typeof(ToggleSwitch),
            new FrameworkPropertyMetadata(22.0, FrameworkPropertyMetadataOptions.AffectsMeasure));

    public double SwitchHeight
    {
        get => (double)GetValue(SwitchHeightProperty);
        set => SetValue(SwitchHeightProperty, value);
    }

    // ------------------------------------------------------------------ thumb

    /// <summary>Diameter of the circular thumb. Default 16.</summary>
    public static readonly DependencyProperty ThumbSizeProperty =
        DependencyProperty.Register(nameof(ThumbSize), typeof(double), typeof(ToggleSwitch),
            new FrameworkPropertyMetadata(16.0));

    public double ThumbSize
    {
        get => (double)GetValue(ThumbSizeProperty);
        set => SetValue(ThumbSizeProperty, value);
    }

    /// <summary>Fill brush of the thumb. When null the template's default brush is used.</summary>
    public static readonly DependencyProperty ThumbBrushProperty =
        DependencyProperty.Register(nameof(ThumbBrush), typeof(Brush), typeof(ToggleSwitch),
            new FrameworkPropertyMetadata(null));

    public Brush? ThumbBrush
    {
        get => (Brush?)GetValue(ThumbBrushProperty);
        set => SetValue(ThumbBrushProperty, value);
    }

    // ------------------------------------------------------------------ track brushes

    /// <summary>Track background when the switch is checked (On state).</summary>
    public static readonly DependencyProperty OnBrushProperty =
        DependencyProperty.Register(nameof(OnBrush), typeof(Brush), typeof(ToggleSwitch),
            new FrameworkPropertyMetadata(null));

    public Brush? OnBrush
    {
        get => (Brush?)GetValue(OnBrushProperty);
        set => SetValue(OnBrushProperty, value);
    }

    /// <summary>Track background when the switch is unchecked (Off state).</summary>
    public static readonly DependencyProperty OffBrushProperty =
        DependencyProperty.Register(nameof(OffBrush), typeof(Brush), typeof(ToggleSwitch),
            new FrameworkPropertyMetadata(null));

    public Brush? OffBrush
    {
        get => (Brush?)GetValue(OffBrushProperty);
        set => SetValue(OffBrushProperty, value);
    }

    // ------------------------------------------------------------------ shape

    /// <summary>Corner radius of the track. Default <c>CornerRadius(11)</c> gives a pill shape.</summary>
    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(ToggleSwitch),
            new FrameworkPropertyMetadata(new CornerRadius(11)));

    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    // ------------------------------------------------------------------ label

    /// <summary>
    /// Which side of the track the On/Off label sits. Default <see cref="Dock.Right"/>.
    /// </summary>
    public static readonly DependencyProperty ContentPlacementProperty =
        DependencyProperty.Register(nameof(ContentPlacement), typeof(Dock), typeof(ToggleSwitch),
            new FrameworkPropertyMetadata(Dock.Right));

    public Dock ContentPlacement
    {
        get => (Dock)GetValue(ContentPlacementProperty);
        set => SetValue(ContentPlacementProperty, value);
    }

    /// <summary>Whether the On/Off content label is visible. Default <c>true</c>.</summary>
    public static readonly DependencyProperty ShowLabelProperty =
        DependencyProperty.Register(nameof(ShowLabel), typeof(bool), typeof(ToggleSwitch),
            new FrameworkPropertyMetadata(true));

    public bool ShowLabel
    {
        get => (bool)GetValue(ShowLabelProperty);
        set => SetValue(ShowLabelProperty, value);
    }

    // ------------------------------------------------------------------ animation

    /// <summary>Duration of the thumb slide animation in milliseconds. Default 200. Set to 0 to disable.</summary>
    public static readonly DependencyProperty AnimationDurationProperty =
        DependencyProperty.Register(nameof(AnimationDuration), typeof(double), typeof(ToggleSwitch),
            new FrameworkPropertyMetadata(200.0));

    public double AnimationDuration
    {
        get => (double)GetValue(AnimationDurationProperty);
        set => SetValue(AnimationDurationProperty, value);
    }
}
