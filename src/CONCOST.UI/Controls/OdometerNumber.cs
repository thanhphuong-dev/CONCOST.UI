using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace CONCOST.UI.Controls;

/// <summary>
/// Numeric readout that rolls from the previous value to the new one — the
/// "number ticker" pattern used by dashboards, without a XAML animation per value.
/// </summary>
public class OdometerNumber : Control
{
    private static readonly DependencyPropertyKey DisplayTextPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(DisplayText), typeof(string), typeof(OdometerNumber),
            new PropertyMetadata("0"));

    public static readonly DependencyProperty DisplayTextProperty = DisplayTextPropertyKey.DependencyProperty;

    private static readonly DependencyProperty DisplayValueProperty =
        DependencyProperty.Register(nameof(DisplayValue), typeof(double), typeof(OdometerNumber),
            new PropertyMetadata(0.0, OnDisplayValueChanged));

    static OdometerNumber()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(OdometerNumber), new FrameworkPropertyMetadata(typeof(OdometerNumber)));
    }

    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(nameof(Value), typeof(double), typeof(OdometerNumber),
            new PropertyMetadata(0.0, OnValueChanged, CoerceValue));

    public static readonly DependencyProperty FormatProperty =
        DependencyProperty.Register(nameof(Format), typeof(string), typeof(OdometerNumber),
            new PropertyMetadata("N0", OnFormatChanged));

    public static readonly DependencyProperty PrefixProperty =
        DependencyProperty.Register(nameof(Prefix), typeof(string), typeof(OdometerNumber),
            new PropertyMetadata(string.Empty, OnFormatChanged));

    public static readonly DependencyProperty SuffixProperty =
        DependencyProperty.Register(nameof(Suffix), typeof(string), typeof(OdometerNumber),
            new PropertyMetadata(string.Empty, OnFormatChanged));

    public static readonly DependencyProperty DurationProperty =
        DependencyProperty.Register(nameof(Duration), typeof(Duration), typeof(OdometerNumber),
            new PropertyMetadata(new Duration(TimeSpan.FromMilliseconds(700))));

    public static readonly DependencyProperty AnimateOnLoadProperty =
        DependencyProperty.Register(nameof(AnimateOnLoad), typeof(bool), typeof(OdometerNumber),
            new PropertyMetadata(true));

    public static readonly DependencyProperty DecimalPlacesProperty =
        DependencyProperty.Register(nameof(DecimalPlaces), typeof(int), typeof(OdometerNumber),
            new PropertyMetadata(0));

    public double Value
    {
        get => (double)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public string Format
    {
        get => (string)GetValue(FormatProperty);
        set => SetValue(FormatProperty, value);
    }

    public string Prefix
    {
        get => (string)GetValue(PrefixProperty);
        set => SetValue(PrefixProperty, value);
    }

    public string Suffix
    {
        get => (string)GetValue(SuffixProperty);
        set => SetValue(SuffixProperty, value);
    }

    public Duration Duration
    {
        get => (Duration)GetValue(DurationProperty);
        set => SetValue(DurationProperty, value);
    }

    public bool AnimateOnLoad
    {
        get => (bool)GetValue(AnimateOnLoadProperty);
        set => SetValue(AnimateOnLoadProperty, value);
    }

    public int DecimalPlaces
    {
        get => (int)GetValue(DecimalPlacesProperty);
        set => SetValue(DecimalPlacesProperty, value);
    }

    public string DisplayText => (string)GetValue(DisplayTextProperty);

    private double DisplayValue => (double)GetValue(DisplayValueProperty);

    private static object CoerceValue(DependencyObject d, object baseValue)
    {
        double value = (double)baseValue;
        return double.IsNaN(value) || double.IsInfinity(value) ? 0.0 : value;
    }

    private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (OdometerNumber)d;
        double target = (double)e.NewValue;
        double from = control.DisplayValue;

        if (control.Duration.TimeSpan <= TimeSpan.Zero || (!control.AnimateOnLoad && from == 0.0))
        {
            control.SetValue(DisplayValueProperty, target);
            return;
        }

        var animation = new DoubleAnimation(from, target, control.Duration)
        {
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut },
            FillBehavior = FillBehavior.HoldEnd
        };

        control.BeginAnimation(DisplayValueProperty, animation);
    }

    private static void OnDisplayValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (OdometerNumber)d;
        control.SetValue(DisplayTextPropertyKey, control.FormatValue((double)e.NewValue));
    }

    private static void OnFormatChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (OdometerNumber)d;
        control.SetValue(DisplayTextPropertyKey, control.FormatValue(control.DisplayValue));
    }

    private string FormatValue(double value)
    {
        string format = string.IsNullOrWhiteSpace(Format) ? "N0" : Format;

        // DecimalPlaces wins when the caller did not express precision in Format.
        if (DecimalPlaces > 0 && format.IndexOf('.') < 0 && format.IndexOf(':') < 0)
            format = "N" + DecimalPlaces.ToString(CultureInfo.InvariantCulture);

        string number;
        try
        {
            number = value.ToString(format, CultureInfo.CurrentCulture);
        }
        catch (FormatException)
        {
            number = value.ToString("N0", CultureInfo.CurrentCulture);
        }

        return string.Concat(Prefix, number, Suffix);
    }
}
