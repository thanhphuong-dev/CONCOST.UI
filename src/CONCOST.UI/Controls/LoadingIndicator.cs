using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace CONCOST.UI.Controls;

public enum LoadingIndicatorType
{
    ThreeDotsWave,       // HandyControl / WPFDevelopers 3 Dots Bouncing Wave
    CircularDotsRing,     // WPFDevelopers Orbiting Spinner Ring
    LinePulse,           // Smooth modern horizontal progress line
    PulseCircle          // Pulsing radar ripple
}

public class LoadingIndicator : Control
{
    static LoadingIndicator()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(LoadingIndicator), new FrameworkPropertyMetadata(typeof(LoadingIndicator)));
    }

    public static readonly DependencyProperty IndicatorTypeProperty =
        DependencyProperty.Register(nameof(IndicatorType), typeof(LoadingIndicatorType), typeof(LoadingIndicator),
            new PropertyMetadata(LoadingIndicatorType.ThreeDotsWave));

    public static readonly DependencyProperty DotSizeProperty =
        DependencyProperty.Register(nameof(DotSize), typeof(double), typeof(LoadingIndicator),
            new PropertyMetadata(10.0));

    public LoadingIndicatorType IndicatorType
    {
        get => (LoadingIndicatorType)GetValue(IndicatorTypeProperty);
        set => SetValue(IndicatorTypeProperty, value);
    }

    public double DotSize
    {
        get => (double)GetValue(DotSizeProperty);
        set => SetValue(DotSizeProperty, value);
    }
}
