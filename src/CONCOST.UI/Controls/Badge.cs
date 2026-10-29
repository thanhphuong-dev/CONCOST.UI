using System.Windows;
using System.Windows.Controls;

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

public class Badge : ContentControl
{
    static Badge()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(Badge), new FrameworkPropertyMetadata(typeof(Badge)));
    }

    public static readonly DependencyProperty AppearanceProperty =
        DependencyProperty.Register(nameof(Appearance), typeof(BadgeAppearance), typeof(Badge),
            new PropertyMetadata(BadgeAppearance.Primary));

    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(Badge),
            new PropertyMetadata(new CornerRadius(10)));

    private static readonly DependencyPropertyKey HasBadgePropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(HasBadge), typeof(bool), typeof(Badge),
            new PropertyMetadata(false));

    public static readonly DependencyProperty HasBadgeProperty = HasBadgePropertyKey.DependencyProperty;

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

    public bool HasBadge => (bool)GetValue(HasBadgeProperty);

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

    private static void OnBadgeStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Badge badge)
        {
            var hasVal = badge.Value != null && !string.Empty.Equals(badge.Value);
            badge.SetValue(HasBadgePropertyKey, badge.ShowBadge && (hasVal || badge.IsDot));
        }
    }
}
