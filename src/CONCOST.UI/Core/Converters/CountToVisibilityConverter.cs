using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace CONCOST.UI.Core.Converters;

/// <summary>
/// Converts an integer count to a <see cref="Visibility"/> value.
/// Returns <see cref="Visibility.Visible"/> when the count is greater than zero,
/// otherwise <see cref="Visibility.Collapsed"/>.
/// </summary>
[ValueConversion(typeof(int), typeof(Visibility))]
public class CountToVisibilityConverter : IValueConverter
{
    public static readonly CountToVisibilityConverter Instance = new();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is int count && count > 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
