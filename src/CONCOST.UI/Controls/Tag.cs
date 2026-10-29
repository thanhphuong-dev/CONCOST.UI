using System.Windows;
using System.Windows.Controls;
using CONCOST.UI.Icons;

namespace CONCOST.UI.Controls;

public class Tag : ContentControl
{
    private Button? _closeButton;
    static Tag()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(Tag), new FrameworkPropertyMetadata(typeof(Tag)));
    }

    public static readonly DependencyProperty IsClosableProperty =
        DependencyProperty.Register(nameof(IsClosable), typeof(bool), typeof(Tag),
            new PropertyMetadata(false));

    public static readonly DependencyProperty IconProperty =
        DependencyProperty.Register(nameof(Icon), typeof(IconKind), typeof(Tag),
            new PropertyMetadata(IconKind.None));

    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(Tag),
            new PropertyMetadata(new CornerRadius(4)));

    public static readonly RoutedEvent ClosedEvent =
        EventManager.RegisterRoutedEvent(nameof(Closed), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(Tag));

    public event RoutedEventHandler Closed
    {
        add => AddHandler(ClosedEvent, value);
        remove => RemoveHandler(ClosedEvent, value);
    }

    public bool IsClosable
    {
        get => (bool)GetValue(IsClosableProperty);
        set => SetValue(IsClosableProperty, value);
    }

    public IconKind Icon
    {
        get => (IconKind)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    public override void OnApplyTemplate()
    {
        if (_closeButton != null) _closeButton.Click -= OnCloseButtonClick;
        base.OnApplyTemplate();
        _closeButton = GetTemplateChild("PART_CloseButton") as Button;
        if (_closeButton != null) _closeButton.Click += OnCloseButtonClick;
    }

    private void OnCloseButtonClick(object sender, RoutedEventArgs e)
    {
        RaiseEvent(new RoutedEventArgs(ClosedEvent, this));
        Visibility = Visibility.Collapsed;
    }
}
