using System.Windows;
using System.Windows.Controls;

namespace CONCOST.UI.Controls;

public class OtpInput : Control
{
    static OtpInput()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(OtpInput), new FrameworkPropertyMetadata(typeof(OtpInput)));
    }

    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(nameof(Value), typeof(string), typeof(OtpInput),
            new FrameworkPropertyMetadata("1234", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public static readonly DependencyProperty LengthProperty =
        DependencyProperty.Register(nameof(Length), typeof(int), typeof(OtpInput),
            new PropertyMetadata(4));

    public string Value
    {
        get => (string)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public int Length
    {
        get => (int)GetValue(LengthProperty);
        set => SetValue(LengthProperty, value);
    }
}
