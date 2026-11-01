using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CONCOST.UI.Controls;

public enum BadgeAppearance
{
    Primary,
    Success,
    Warning,
    Danger,
    Info,
    Neutral
}

/// <summary>
/// A badge that overlays a numeric count, a status dot, or arbitrary text on top of child content.
/// Supports appearance variants, max-value overflow display ("99+"), pulsing animation and fine-position offsets.
/// </summary>
public class Badge : ContentControl
{
    static Badge()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(Badge), new FrameworkPropertyMetadata(typeof(Badge)));
    }

    // ------------------------------------------------------------------ appearance

    public static readonly DependencyProperty AppearanceProperty =
        DependencyProperty.Register(nameof(Appearance), typeof(BadgeAppearance), typeof(Badge),
            new PropertyMetadata(BadgeAppearance.Primary));

    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(Badge),
            new PropertyMetadata(new CornerRadius(10)));

    public BadgeAppearance Appearance
    {
        get => (BadgeAppearance)GetValue(AppearanceProperty);
        set => SetValue(AppearanceProperty, value);
    }

    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    // ------------------------------------------------------------------ HasBadge (read-only)

    private static readonly DependencyPropertyKey HasBadgePropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(HasBadge), typeof(bool), typeof(Badge),
            new PropertyMetadata(false));

    public static readonly DependencyProperty HasBadgeProperty = HasBadgePropertyKey.DependencyProperty;

    public bool HasBadge => (bool)GetValue(HasBadgeProperty);

    // ------------------------------------------------------------------ Value / IsDot / ShowBadge

    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(nameof(Value), typeof(object), typeof(Badge),
            new PropertyMetadata(null, OnBadgeStateChanged));

    public static readonly DependencyProperty IsDotProperty =
        DependencyProperty.Register(nameof(IsDot), typeof(bool), typeof(Badge),
            new PropertyMetadata(false, OnBadgeStateChanged));

    public static readonly DependencyProperty ShowBadgeProperty =
        DependencyProperty.Register(nameof(ShowBadge), typeof(bool), typeof(Badge),
            new PropertyMetadata(true, OnBadgeStateChanged));

    public static readonly DependencyProperty BadgeMarginProperty =
        DependencyProperty.Register(nameof(BadgeMargin), typeof(Thickness), typeof(Badge),
            new PropertyMetadata(new Thickness(0, -6, -8, 0)));

    public static readonly DependencyProperty StatusProperty =
        DependencyProperty.Register(nameof(Status), typeof(string), typeof(Badge),
            new PropertyMetadata(null));

    public object? Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public bool IsDot
    {
        get => (bool)GetValue(IsDotProperty);
        set => SetValue(IsDotProperty, value);
    }

    public bool ShowBadge
    {
        get => (bool)GetValue(ShowBadgeProperty);
        set => SetValue(ShowBadgeProperty, value);
    }

    public Thickness BadgeMargin
    {
        get => (Thickness)GetValue(BadgeMarginProperty);
        set => SetValue(BadgeMarginProperty, value);
    }

    public string? Status
    {
        get => (string?)GetValue(StatusProperty);
        set => SetValue(StatusProperty, value);
    }

    // ------------------------------------------------------------------ dot

    /// <summary>Diameter of the dot when <see cref="IsDot"/> is <c>true</c>. Default 8.</summary>
    public static readonly DependencyProperty DotSizeProperty =
        DependencyProperty.Register(nameof(DotSize), typeof(double), typeof(Badge),
            new FrameworkPropertyMetadata(8.0));

    public double DotSize
    {
        get => (double)GetValue(DotSizeProperty);
        set => SetValue(DotSizeProperty, value);
    }

    // ------------------------------------------------------------------ MaxValue / overflow

    /// <summary>
    /// When <see cref="Value"/> is a number larger than this, the badge shows "&lt;MaxValue&gt;+" instead.
    /// Default 99.
    /// </summary>
    public static readonly DependencyProperty MaxValueProperty =
        DependencyProperty.Register(nameof(MaxValue), typeof(int), typeof(Badge),
            new PropertyMetadata(99, OnBadgeStateChanged));

    public int MaxValue
    {
        get => (int)GetValue(MaxValueProperty);
        set => SetValue(MaxValueProperty, value);
    }

    // ------------------------------------------------------------------ DisplayText (read-only)

    private static readonly DependencyPropertyKey DisplayTextPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(DisplayText), typeof(string), typeof(Badge),
            new PropertyMetadata(string.Empty));

    /// <summary>
    /// The text rendered inside the badge pill. Computed from <see cref="Value"/> and <see cref="MaxValue"/>:
    /// numbers greater than MaxValue display as "99+".
    /// </summary>
    public static readonly DependencyProperty DisplayTextProperty = DisplayTextPropertyKey.DependencyProperty;

    public string DisplayText => (string)GetValue(DisplayTextProperty);

    // ------------------------------------------------------------------ position fine-tuning

    /// <summary>Additional pixel offset applied on top of <see cref="BadgeMargin"/>.</summary>
    public static readonly DependencyProperty OffsetProperty =
        DependencyProperty.Register(nameof(Offset), typeof(Point), typeof(Badge),
            new PropertyMetadata(new Point(0, 0)));

    public Point Offset
    {
        get => (Point)GetValue(OffsetProperty);
        set => SetValue(OffsetProperty, value);
    }

    // ------------------------------------------------------------------ typography

    /// <summary>Font size inside the badge pill. Default 10.</summary>
    public static readonly DependencyProperty BadgeFontSizeProperty =
        DependencyProperty.Register(nameof(BadgeFontSize), typeof(double), typeof(Badge),
            new FrameworkPropertyMetadata(10.0));

    public double BadgeFontSize
    {
        get => (double)GetValue(BadgeFontSizeProperty);
        set => SetValue(BadgeFontSizeProperty, value);
    }

    /// <summary>Padding inside the badge pill. Default <c>4,1,4,1</c>.</summary>
    public static readonly DependencyProperty BadgePaddingProperty =
        DependencyProperty.Register(nameof(BadgePadding), typeof(Thickness), typeof(Badge),
            new FrameworkPropertyMetadata(new Thickness(4, 1, 4, 1)));

    public Thickness BadgePadding
    {
        get => (Thickness)GetValue(BadgePaddingProperty);
        set => SetValue(BadgePaddingProperty, value);
    }

    // ------------------------------------------------------------------ pulse

    /// <summary>
    /// When <c>true</c> the badge pulses gently to draw attention. Driven by a CSS-style scale
    /// animation defined in the control template.
    /// </summary>
    public static readonly DependencyProperty PulseProperty =
        DependencyProperty.Register(nameof(Pulse), typeof(bool), typeof(Badge),
            new FrameworkPropertyMetadata(false));

    public bool Pulse
    {
        get => (bool)GetValue(PulseProperty);
        set => SetValue(PulseProperty, value);
    }

    // ------------------------------------------------------------------ state callbacks

    private static void OnBadgeStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not Badge badge) return;

        var hasVal = badge.Value != null && !string.Empty.Equals(badge.Value);
        badge.SetValue(HasBadgePropertyKey, badge.ShowBadge && (hasVal || badge.IsDot));
        badge.SetValue(DisplayTextPropertyKey, ComputeDisplayText(badge));
    }

    private static string ComputeDisplayText(Badge badge)
    {
        if (badge.Value is null) return string.Empty;

        // Numeric overflow: show "99+" (or "<MaxValue>+")
        if (badge.Value is int intVal)
        {
            return intVal > badge.MaxValue ? $"{badge.MaxValue}+" : intVal.ToString();
        }

        if (badge.Value is long longVal)
        {
            return longVal > badge.MaxValue ? $"{badge.MaxValue}+" : longVal.ToString();
        }

        if (double.TryParse(badge.Value.ToString(), out double dbl) && dbl > badge.MaxValue)
        {
            return $"{badge.MaxValue}+";
        }

        return badge.Value.ToString() ?? string.Empty;
    }
}
