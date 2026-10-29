using System.Windows;
using System.Windows.Media;

namespace CONCOST.UI.Core.Helpers;

public static class ColorHelper
{
    public static Color FromHex(string hex)
    {
        if (string.IsNullOrWhiteSpace(hex))
            return Colors.Transparent;

        hex = hex.TrimStart('#');

        if (hex.Length == 6)
        {
            byte r = Convert.ToByte(hex.Substring(0, 2), 16);
            byte g = Convert.ToByte(hex.Substring(2, 2), 16);
            byte b = Convert.ToByte(hex.Substring(4, 2), 16);
            return Color.FromRgb(r, g, b);
        }
        if (hex.Length == 8)
        {
            byte a = Convert.ToByte(hex.Substring(0, 2), 16);
            byte r = Convert.ToByte(hex.Substring(2, 2), 16);
            byte g = Convert.ToByte(hex.Substring(4, 2), 16);
            byte b = Convert.ToByte(hex.Substring(6, 2), 16);
            return Color.FromArgb(a, r, g, b);
        }

        throw new FormatException($"Invalid hex color: {hex}");
    }

    public static Color Lighten(Color color, double factor)
    {
        factor = NumericCompatibility.Clamp(factor, 0.0, 1.0);
        return Color.FromArgb(
            color.A,
            (byte)(color.R + (255 - color.R) * factor),
            (byte)(color.G + (255 - color.G) * factor),
            (byte)(color.B + (255 - color.B) * factor)
        );
    }

    public static Color Darken(Color color, double factor)
    {
        factor = NumericCompatibility.Clamp(factor, 0.0, 1.0);
        return Color.FromArgb(
            color.A,
            (byte)(color.R * (1 - factor)),
            (byte)(color.G * (1 - factor)),
            (byte)(color.B * (1 - factor))
        );
    }

    public static SolidColorBrush CreateBrush(Color color, double opacity = 1.0)
    {
        var brush = new SolidColorBrush(color) { Opacity = opacity };
        brush.Freeze();
        return brush;
    }
}
