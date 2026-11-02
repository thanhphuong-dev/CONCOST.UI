using System;
using System.Globalization;
using System.Windows.Data;
using CONCOST.UI.Controls;

namespace CONCOST.UI.Core.Converters;

/// <summary>
/// Turns a <see cref="FilterOperator"/> into the sentence-case label shown in the filter
/// drop-down, so the menu reads “Starts with” rather than the camel-cased enum name.
/// </summary>
[ValueConversion(typeof(FilterOperator), typeof(string))]
public class FilterOperatorLabelConverter : IValueConverter
{
    public static readonly FilterOperatorLabelConverter Instance = new();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is FilterOperator op
            ? op switch
            {
                FilterOperator.Contains => "Contains",
                FilterOperator.Equals => "Equals",
                FilterOperator.StartsWith => "Starts with",
                FilterOperator.EndsWith => "Ends with",
                FilterOperator.GreaterThan => "Greater than",
                FilterOperator.LessThan => "Less than",
                _ => op.ToString()
            }
            : string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
