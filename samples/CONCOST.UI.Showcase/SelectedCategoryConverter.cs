using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace CONCOST.UI.Showcase;

public sealed class SelectedCategoryConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => string.Equals(value as string, parameter as string, StringComparison.OrdinalIgnoreCase);

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => value is true ? parameter ?? Binding.DoNothing : Binding.DoNothing;
}
