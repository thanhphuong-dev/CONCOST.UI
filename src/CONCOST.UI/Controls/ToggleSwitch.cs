using System.Windows;
using System.Windows.Controls.Primitives;

namespace CONCOST.UI.Controls;

public class ToggleSwitch : ToggleButton
{
    static ToggleSwitch()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(ToggleSwitch), new FrameworkPropertyMetadata(typeof(ToggleSwitch)));
    }

    public static readonly DependencyProperty OnContentProperty =
        DependencyProperty.Register(nameof(OnContent), typeof(object), typeof(ToggleSwitch),
            new PropertyMetadata("On"));

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
}
