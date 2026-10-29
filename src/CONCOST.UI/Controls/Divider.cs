using System.Windows;
using System.Windows.Controls;
using CONCOST.UI.Icons;

namespace CONCOST.UI.Controls;

public enum DividerLineStyle
{
    Solid,
    Dashed,
    Dotted
}

public class Divider : Control
{
    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(nameof(Text), typeof(string), typeof(Divider),
            new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty OrientationProperty =
        DependencyProperty.Register(nameof(Orientation), typeof(Orientation), typeof(Divider),
            new PropertyMetadata(Orientation.Horizontal));

    public static readonly DependencyProperty IconProperty =
        DependencyProperty.Register(nameof(Icon), typeof(IconKind), typeof(Divider),
            new PropertyMetadata(IconKind.None));

    public static readonly DependencyProperty LineStyleProperty =
        DependencyProperty.Register(nameof(LineStyle), typeof(DividerLineStyle), typeof(Divider),
            new PropertyMetadata(DividerLineStyle.Solid));

    public static readonly DependencyProperty TextAlignmentProperty =
        DependencyProperty.Register(nameof(TextAlignment), typeof(HorizontalAlignment), typeof(Divider),
            new PropertyMetadata(HorizontalAlignment.Center));

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public Orientation Orientation
    {
        get => (Orientation)GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    public IconKind Icon
    {
        get => (IconKind)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public DividerLineStyle LineStyle
    {
        get => (DividerLineStyle)GetValue(LineStyleProperty);
        set => SetValue(LineStyleProperty, value);
    }

    public HorizontalAlignment TextAlignment
    {
        get => (HorizontalAlignment)GetValue(TextAlignmentProperty);
        set => SetValue(TextAlignmentProperty, value);
    }

    static Divider()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(Divider), new FrameworkPropertyMetadata(typeof(Divider)));
    }
}
