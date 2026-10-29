using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CONCOST.UI.Icons;

namespace CONCOST.UI.Controls;

public class Card : HeaderedContentControl
{
    static Card()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(Card), new FrameworkPropertyMetadata(typeof(Card)));
    }

    public static readonly DependencyProperty IconProperty =
        DependencyProperty.Register(nameof(Icon), typeof(IconKind), typeof(Card),
            new PropertyMetadata(IconKind.None));

    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(Card),
            new PropertyMetadata(new CornerRadius(8)));

    public static readonly DependencyProperty FooterProperty =
        DependencyProperty.Register(nameof(Footer), typeof(object), typeof(Card),
            new PropertyMetadata(null));

    public static readonly DependencyProperty FooterTemplateProperty =
        DependencyProperty.Register(nameof(FooterTemplate), typeof(DataTemplate), typeof(Card),
            new PropertyMetadata(null));

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

    public object? Footer
    {
        get => GetValue(FooterProperty);
        set => SetValue(FooterProperty, value);
    }

    public DataTemplate? FooterTemplate
    {
        get => (DataTemplate?)GetValue(FooterTemplateProperty);
        set => SetValue(FooterTemplateProperty, value);
    }
}
