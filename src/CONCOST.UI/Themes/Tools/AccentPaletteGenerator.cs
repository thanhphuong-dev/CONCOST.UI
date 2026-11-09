using System.Windows.Media;
using CONCOST.UI.Core.Helpers;

namespace CONCOST.UI.Themes.Tools;

public class AccentPalette
{
    public Color Primary { get; set; }
    public Color PrimaryLight { get; set; }
    public Color PrimaryDark { get; set; }
    public Color PrimaryHover { get; set; }
    public Color PrimaryPressed { get; set; }
    public Color PrimaryDisabled { get; set; }
    public Color PrimaryText { get; set; }

    /// <summary>The accent at 12% alpha: a tint that works over any surface without a second blend.</summary>
    public Color PrimarySoft { get; set; }

    /// <summary>Luminous, clean soft background tint for row and item selection.</summary>
    public Color PrimarySoftLight { get; set; }

    /// <summary>Subtle matching border color for selected rows and items.</summary>
    public Color PrimarySoftBorder { get; set; }

    /// <summary>High-contrast matching foreground color on top of soft background.</summary>
    public Color PrimarySoftForeground { get; set; }
}

public static class AccentPaletteGenerator
{
    public static AccentPalette Generate(Color baseColor, bool isDark = false)
    {
        var light = ColorHelper.Lighten(baseColor, 0.25);
        var dark = ColorHelper.Darken(baseColor, 0.25);
        var hover = ColorHelper.Lighten(baseColor, 0.1);
        var pressed = ColorHelper.Darken(baseColor, 0.15);
        var disabled = Color.FromArgb(100, baseColor.R, baseColor.G, baseColor.B);
        var soft = Color.FromArgb(31, baseColor.R, baseColor.G, baseColor.B);

        Color softLight;
        Color softBorder;
        Color softFg;

        if (isDark)
        {
            // Dark theme: deep, rich warm-tinted dark background with subtle luminous border
            byte rLight = (byte)(baseColor.R * 0.16 + 15 * 0.84);
            byte gLight = (byte)(baseColor.G * 0.16 + 23 * 0.84);
            byte bLight = (byte)(baseColor.B * 0.16 + 42 * 0.84);
            softLight = Color.FromRgb(rLight, gLight, bLight);

            byte rBorder = (byte)(baseColor.R * 0.40 + 15 * 0.60);
            byte gBorder = (byte)(baseColor.G * 0.40 + 23 * 0.60);
            byte bBorder = (byte)(baseColor.B * 0.40 + 42 * 0.60);
            softBorder = Color.FromRgb(rBorder, gBorder, bBorder);

            softFg = ColorHelper.Lighten(baseColor, 0.25);
        }
        else
        {
            // Light theme: clean, luminous 8% tint with white, subtle 28% border
            byte rLight = (byte)(255 - (255 - baseColor.R) * 0.08);
            byte gLight = (byte)(255 - (255 - baseColor.G) * 0.08);
            byte bLight = (byte)(255 - (255 - baseColor.B) * 0.08);
            softLight = Color.FromRgb(rLight, gLight, bLight);

            byte rBorder = (byte)(255 - (255 - baseColor.R) * 0.28);
            byte gBorder = (byte)(255 - (255 - baseColor.G) * 0.28);
            byte bBorder = (byte)(255 - (255 - baseColor.B) * 0.28);
            softBorder = Color.FromRgb(rBorder, gBorder, bBorder);

            softFg = ColorHelper.Darken(baseColor, 0.20);
        }

        // Calculate contrasting text color (white or dark)
        var contrastAgainstWhite = ContrastChecker.Check(Colors.White, baseColor);
        var textColor = contrastAgainstWhite.Ratio >= 3.0 ? Colors.White : Colors.Black;

        return new AccentPalette
        {
            Primary = baseColor,
            PrimaryLight = light,
            PrimaryDark = dark,
            PrimaryHover = hover,
            PrimaryPressed = pressed,
            PrimaryDisabled = disabled,
            PrimaryText = textColor,
            PrimarySoft = soft,
            PrimarySoftLight = softLight,
            PrimarySoftBorder = softBorder,
            PrimarySoftForeground = softFg
        };
    }
}
