using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace CONCOST.UI.Controls;

public enum ComparisonOperator
{
    LessThan,
    LessThanOrEqual,
    GreaterThan,
    GreaterThanOrEqual,
    Equal,
    NotEqual,
    Between
}

public class ConditionalFormattingRule : DependencyObject
{
    public static readonly DependencyProperty OperatorProperty =
        DependencyProperty.Register(nameof(Operator), typeof(ComparisonOperator), typeof(ConditionalFormattingRule),
            new PropertyMetadata(ComparisonOperator.GreaterThan));

    public ComparisonOperator Operator
    {
        get => (ComparisonOperator)GetValue(OperatorProperty);
        set => SetValue(OperatorProperty, value);
    }

    public static readonly DependencyProperty Value1Property =
        DependencyProperty.Register(nameof(Value1), typeof(double), typeof(ConditionalFormattingRule), new PropertyMetadata(0.0));

    public double Value1
    {
        get => (double)GetValue(Value1Property);
        set => SetValue(Value1Property, value);
    }

    public static readonly DependencyProperty Value2Property =
        DependencyProperty.Register(nameof(Value2), typeof(double), typeof(ConditionalFormattingRule), new PropertyMetadata(0.0));

    public double Value2
    {
        get => (double)GetValue(Value2Property);
        set => SetValue(Value2Property, value);
    }

    public static readonly DependencyProperty BackgroundProperty =
        DependencyProperty.Register(nameof(Background), typeof(Brush), typeof(ConditionalFormattingRule), new PropertyMetadata(null));

    public Brush? Background
    {
        get => (Brush?)GetValue(BackgroundProperty);
        set => SetValue(BackgroundProperty, value);
    }

    public static readonly DependencyProperty ForegroundProperty =
        DependencyProperty.Register(nameof(Foreground), typeof(Brush), typeof(ConditionalFormattingRule), new PropertyMetadata(null));

    public Brush? Foreground
    {
        get => (Brush?)GetValue(ForegroundProperty);
        set => SetValue(ForegroundProperty, value);
    }

    public bool Evaluate(double val)
    {
        return Operator switch
        {
            ComparisonOperator.LessThan => val < Value1,
            ComparisonOperator.LessThanOrEqual => val <= Value1,
            ComparisonOperator.GreaterThan => val > Value1,
            ComparisonOperator.GreaterThanOrEqual => val >= Value1,
            ComparisonOperator.Equal => Math.Abs(val - Value1) < 0.000001,
            ComparisonOperator.NotEqual => Math.Abs(val - Value1) >= 0.000001,
            ComparisonOperator.Between => val >= Math.Min(Value1, Value2) && val <= Math.Max(Value1, Value2),
            _ => false
        };
    }
}

public class NumericThresholdToBrushConverter : IValueConverter
{
    public static readonly NumericThresholdToBrushConverter Instance = new();

    public double Threshold { get; set; } = 0.0;
    public Brush? BelowBrush { get; set; } = new SolidColorBrush(Color.FromRgb(239, 68, 68));   // #EF4444 Danger
    public Brush? AboveBrush { get; set; } = new SolidColorBrush(Color.FromRgb(34, 197, 94));   // #22C55E Success
    public Brush? DefaultBrush { get; set; }

    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null) return DefaultBrush;

        if (double.TryParse(value.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out double num) ||
            double.TryParse(value.ToString(), NumberStyles.Any, CultureInfo.CurrentCulture, out num))
        {
            if (num < Threshold) return BelowBrush;
            if (num > Threshold) return AboveBrush;
        }

        return DefaultBrush;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}

public class DataBarWidthConverter : IMultiValueConverter
{
    public static readonly DataBarWidthConverter Instance = new();

    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length >= 2 &&
            values[0] != null &&
            values[1] != null &&
            double.TryParse(values[0].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out double currentVal) &&
            double.TryParse(values[1].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out double maxVal))
        {
            double availableWidth = 100.0;
            if (values.Length >= 3 && values[2] is double w && w > 0)
                availableWidth = Math.Max(10, w - 60);

            if (maxVal <= 0) return 0.0;
            double ratio = Math.Max(0.0, Math.Min(1.0, currentVal / maxVal));
            return Math.Round(ratio * availableWidth, 1);
        }

        return 0.0;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) => throw new NotImplementedException();
}
