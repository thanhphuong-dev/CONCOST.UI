using System.Windows;
using System.Windows.Controls;

namespace CONCOST.UI.Controls;

public class UnitBox : Control
{
    static UnitBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(UnitBox), new FrameworkPropertyMetadata(typeof(UnitBox)));
    }

    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(nameof(Value), typeof(double), typeof(UnitBox),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public static readonly DependencyProperty UnitProperty =
        DependencyProperty.Register(nameof(Unit), typeof(string), typeof(UnitBox),
            new PropertyMetadata("mm"));

    public static readonly DependencyProperty DecimalPlacesProperty =
        DependencyProperty.Register(nameof(DecimalPlaces), typeof(int), typeof(UnitBox),
            new PropertyMetadata(1));

    public static readonly DependencyProperty HeaderProperty =
        DependencyProperty.Register(nameof(Header), typeof(string), typeof(UnitBox),
            new PropertyMetadata(string.Empty));

    public double Value
    {
        get => (double)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public string Unit
    {
        get => (string)GetValue(UnitProperty);
        set => SetValue(UnitProperty, value);
    }

    public int DecimalPlaces
    {
        get => (int)GetValue(DecimalPlacesProperty);
        set => SetValue(DecimalPlacesProperty, value);
    }

    public string Header
    {
        get => (string)GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }
}
