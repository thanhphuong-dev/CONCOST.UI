using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace CONCOST.UI.Core.Converters;

[ValueConversion(typeof(bool), typeof(Visibility))]
public class BooleanToVisibilityConverter : IValueConverter
{
    public bool Invert { get; set; }
    public bool Collapse { get; set; } = true;

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        bool bValue = value is true;
        if (Invert) bValue = !bValue;

        if (bValue) return Visibility.Visible;
        return Collapse ? Visibility.Collapsed : Visibility.Hidden;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is Visibility vis)
        {
            bool res = (vis == Visibility.Visible);
            return Invert ? !res : res;
        }
        return false;
    }
}

[ValueConversion(typeof(object), typeof(Visibility))]
public class NullToVisibilityConverter : IValueConverter
{
    public bool Invert { get; set; }

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        bool isNotNull = value != null;
        if (value is string s) isNotNull = !string.IsNullOrWhiteSpace(s);

        if (Invert) isNotNull = !isNotNull;

        return isNotNull ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts TextBox/PasswordBox Padding to a negative margin on PART_ContentHost
/// so horizontal padding is not doubled when Grid Margin="{TemplateBinding Padding}" is present,
/// aligning caret and placeholder pixel-perfectly.
/// </summary>
[ValueConversion(typeof(Thickness), typeof(Thickness))]
public class PaddingToContentHostMarginConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is Thickness t)
        {
            return new Thickness(-t.Left, 0, -t.Right, 0);
        }
        return new Thickness(0);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Calculates pixel-perfect inner corner radius for Expander Header and ContentSite,
/// preventing child backgrounds from bleeding past the outer rounded border.
/// </summary>
public class ExpanderCornerRadiusConverter : IMultiValueConverter, IValueConverter
{
    public double Offset { get; set; } = 1.0;

    public object Convert(object[] values, Type targetType, object? parameter, CultureInfo? culture)
    {
        if (values == null || values.Length == 0)
            return new CornerRadius(0);

        CornerRadius cr = default;
        if (values[0] is CornerRadius r) cr = r;
        else if (values[0] is double d) cr = new CornerRadius(d);
        else if (values[0] is int i) cr = new CornerRadius(i);

        bool isExpanded = true;
        if (values.Length > 1 && values[1] is bool b)
        {
            isExpanded = b;
        }

        System.Windows.Controls.ExpandDirection direction = System.Windows.Controls.ExpandDirection.Down;
        if (values.Length > 2 && values[2] is System.Windows.Controls.ExpandDirection dir)
        {
            direction = dir;
        }

        double offset = Offset;
        if (values.Length > 3 && values[3] is Thickness th)
        {
            offset = Math.Max(0, Math.Max(th.Left, th.Top));
        }

        double tl = cr.TopLeft > offset ? cr.TopLeft - offset : (cr.TopLeft > 0 ? cr.TopLeft : 0);
        double tr = cr.TopRight > offset ? cr.TopRight - offset : (cr.TopRight > 0 ? cr.TopRight : 0);
        double br = cr.BottomRight > offset ? cr.BottomRight - offset : (cr.BottomRight > 0 ? cr.BottomRight : 0);
        double bl = cr.BottomLeft > offset ? cr.BottomLeft - offset : (cr.BottomLeft > 0 ? cr.BottomLeft : 0);

        bool isContent = string.Equals(parameter?.ToString(), "Content", StringComparison.OrdinalIgnoreCase);

        if (isContent)
        {
            return direction switch
            {
                System.Windows.Controls.ExpandDirection.Down => new CornerRadius(0, 0, br, bl),
                System.Windows.Controls.ExpandDirection.Up => new CornerRadius(tl, tr, 0, 0),
                System.Windows.Controls.ExpandDirection.Left => new CornerRadius(tl, 0, 0, bl),
                System.Windows.Controls.ExpandDirection.Right => new CornerRadius(0, tr, br, 0),
                _ => new CornerRadius(0, 0, br, bl)
            };
        }

        // HeaderSite
        if (!isExpanded)
        {
            return new CornerRadius(tl, tr, br, bl);
        }

        return direction switch
        {
            System.Windows.Controls.ExpandDirection.Down => new CornerRadius(tl, tr, 0, 0),
            System.Windows.Controls.ExpandDirection.Up => new CornerRadius(0, 0, br, bl),
            System.Windows.Controls.ExpandDirection.Left => new CornerRadius(0, tr, br, 0),
            System.Windows.Controls.ExpandDirection.Right => new CornerRadius(tl, 0, 0, bl),
            _ => new CornerRadius(tl, tr, 0, 0)
        };
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object? parameter, CultureInfo? culture)
    {
        throw new NotImplementedException();
    }

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo? culture)
    {
        CornerRadius cr = default;
        if (value is CornerRadius r) cr = r;
        else if (value is double d) cr = new CornerRadius(d);
        else if (value is int i) cr = new CornerRadius(i);

        double tl = cr.TopLeft > Offset ? cr.TopLeft - Offset : (cr.TopLeft > 0 ? cr.TopLeft : 0);
        double tr = cr.TopRight > Offset ? cr.TopRight - Offset : (cr.TopRight > 0 ? cr.TopRight : 0);
        double br = cr.BottomRight > Offset ? cr.BottomRight - Offset : (cr.BottomRight > 0 ? cr.BottomRight : 0);
        double bl = cr.BottomLeft > Offset ? cr.BottomLeft - Offset : (cr.BottomLeft > 0 ? cr.BottomLeft : 0);

        string mode = parameter?.ToString()?.Trim().ToLowerInvariant() ?? "all";

        return mode switch
        {
            "top" => new CornerRadius(tl, tr, 0, 0),
            "bottom" => new CornerRadius(0, 0, br, bl),
            "left" => new CornerRadius(tl, 0, 0, bl),
            "right" => new CornerRadius(0, tr, br, 0),
            _ => new CornerRadius(tl, tr, br, bl)
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo? culture)
    {
        throw new NotImplementedException();
    }
}

