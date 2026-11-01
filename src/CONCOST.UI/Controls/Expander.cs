using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace CONCOST.UI.Controls;

/// <summary>
/// An animated panel that shows or hides its content by expanding or collapsing in a chosen direction.
/// Adds header/content styling and routed <see cref="Expanded"/>/<see cref="Collapsed"/> events on top of
/// the plain WPF <see cref="HeaderedContentControl"/> base.
/// </summary>
public class Expander : HeaderedContentControl
{
    private Button? _headerButton;

    static Expander()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(Expander), new FrameworkPropertyMetadata(typeof(Expander)));
    }

    // ------------------------------------------------------------------ core state

    public static readonly DependencyProperty IsExpandedProperty =
        DependencyProperty.Register(nameof(IsExpanded), typeof(bool), typeof(Expander),
            new FrameworkPropertyMetadata(true,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnIsExpandedChanged));

    public static readonly DependencyProperty ExpandDirectionProperty =
        DependencyProperty.Register(nameof(ExpandDirection), typeof(ExpandDirection), typeof(Expander),
            new PropertyMetadata(ExpandDirection.Down));

    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(Expander),
            new PropertyMetadata(new CornerRadius(8)));

    public bool IsExpanded
    {
        get => (bool)GetValue(IsExpandedProperty);
        set => SetValue(IsExpandedProperty, value);
    }

    public ExpandDirection ExpandDirection
    {
        get => (ExpandDirection)GetValue(ExpandDirectionProperty);
        set => SetValue(ExpandDirectionProperty, value);
    }

    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    // ------------------------------------------------------------------ animation

    /// <summary>Duration of the expand/collapse animation in milliseconds. Default 250. Set 0 to skip.</summary>
    public static readonly DependencyProperty AnimationDurationProperty =
        DependencyProperty.Register(nameof(AnimationDuration), typeof(double), typeof(Expander),
            new PropertyMetadata(250.0));

    public double AnimationDuration
    {
        get => (double)GetValue(AnimationDurationProperty);
        set => SetValue(AnimationDurationProperty, value);
    }

    /// <summary>Whether the expand/collapse is animated. Default <c>true</c>.</summary>
    public static readonly DependencyProperty IsAnimatedProperty =
        DependencyProperty.Register(nameof(IsAnimated), typeof(bool), typeof(Expander),
            new PropertyMetadata(true));

    public bool IsAnimated
    {
        get => (bool)GetValue(IsAnimatedProperty);
        set => SetValue(IsAnimatedProperty, value);
    }

    // ------------------------------------------------------------------ header visuals

    /// <summary>Background brush of the header bar. Null uses the template default.</summary>
    public static readonly DependencyProperty HeaderBackgroundProperty =
        DependencyProperty.Register(nameof(HeaderBackground), typeof(Brush), typeof(Expander),
            new FrameworkPropertyMetadata(null));

    public Brush? HeaderBackground
    {
        get => (Brush?)GetValue(HeaderBackgroundProperty);
        set => SetValue(HeaderBackgroundProperty, value);
    }

    /// <summary>Foreground brush of the header text/icon. Null uses the template default.</summary>
    public static readonly DependencyProperty HeaderForegroundProperty =
        DependencyProperty.Register(nameof(HeaderForeground), typeof(Brush), typeof(Expander),
            new FrameworkPropertyMetadata(null));

    public Brush? HeaderForeground
    {
        get => (Brush?)GetValue(HeaderForegroundProperty);
        set => SetValue(HeaderForegroundProperty, value);
    }

    /// <summary>Padding of the header strip. Default <c>12,8,12,8</c>.</summary>
    public static readonly DependencyProperty HeaderPaddingProperty =
        DependencyProperty.Register(nameof(HeaderPadding), typeof(Thickness), typeof(Expander),
            new FrameworkPropertyMetadata(new Thickness(12, 8, 12, 8)));

    public Thickness HeaderPadding
    {
        get => (Thickness)GetValue(HeaderPaddingProperty);
        set => SetValue(HeaderPaddingProperty, value);
    }

    // ------------------------------------------------------------------ content visuals

    /// <summary>Padding around the expanded content area. Default <c>12</c> on all sides.</summary>
    public static readonly DependencyProperty ContentPaddingProperty =
        DependencyProperty.Register(nameof(ContentPadding), typeof(Thickness), typeof(Expander),
            new FrameworkPropertyMetadata(new Thickness(12)));

    public Thickness ContentPadding
    {
        get => (Thickness)GetValue(ContentPaddingProperty);
        set => SetValue(ContentPaddingProperty, value);
    }

    // ------------------------------------------------------------------ toggle icon

    /// <summary>Whether the chevron/arrow icon that shows expand state is visible. Default <c>true</c>.</summary>
    public static readonly DependencyProperty ShowToggleIconProperty =
        DependencyProperty.Register(nameof(ShowToggleIcon), typeof(bool), typeof(Expander),
            new PropertyMetadata(true));

    public bool ShowToggleIcon
    {
        get => (bool)GetValue(ShowToggleIconProperty);
        set => SetValue(ShowToggleIconProperty, value);
    }

    /// <summary>Size of the toggle arrow icon in device-independent pixels. Default 16.</summary>
    public static readonly DependencyProperty ToggleIconSizeProperty =
        DependencyProperty.Register(nameof(ToggleIconSize), typeof(double), typeof(Expander),
            new PropertyMetadata(16.0));

    public double ToggleIconSize
    {
        get => (double)GetValue(ToggleIconSizeProperty);
        set => SetValue(ToggleIconSizeProperty, value);
    }

    // ------------------------------------------------------------------ routed events

    /// <summary>Raised after <see cref="IsExpanded"/> becomes <c>true</c>.</summary>
    public static readonly RoutedEvent ExpandedEvent =
        EventManager.RegisterRoutedEvent(nameof(Expanded), RoutingStrategy.Bubble,
            typeof(RoutedEventHandler), typeof(Expander));

    /// <summary>Raised after <see cref="IsExpanded"/> becomes <c>false</c>.</summary>
    public static readonly RoutedEvent CollapsedEvent =
        EventManager.RegisterRoutedEvent(nameof(Collapsed), RoutingStrategy.Bubble,
            typeof(RoutedEventHandler), typeof(Expander));

    public event RoutedEventHandler Expanded
    {
        add => AddHandler(ExpandedEvent, value);
        remove => RemoveHandler(ExpandedEvent, value);
    }

    public event RoutedEventHandler Collapsed
    {
        add => AddHandler(CollapsedEvent, value);
        remove => RemoveHandler(CollapsedEvent, value);
    }

    // ------------------------------------------------------------------ template

    public override void OnApplyTemplate()
    {
        if (_headerButton != null) _headerButton.Click -= OnHeaderButtonClick;
        base.OnApplyTemplate();
        _headerButton = GetTemplateChild("PART_HeaderButton") as Button;
        if (_headerButton != null) _headerButton.Click += OnHeaderButtonClick;
    }

    private void OnHeaderButtonClick(object sender, RoutedEventArgs e) => IsExpanded = !IsExpanded;

    private static void OnIsExpandedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not Expander expander) return;
        bool expanded = (bool)e.NewValue;
        expander.RaiseEvent(new RoutedEventArgs(expanded ? ExpandedEvent : CollapsedEvent, expander));
    }
}
