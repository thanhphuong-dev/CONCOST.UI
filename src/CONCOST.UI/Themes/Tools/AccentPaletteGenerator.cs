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
}

public static class AccentPaletteGenerator
{
    public static AccentPalette Generate(Color baseColor)
    {
        var light = ColorHelper.Lighten(baseColor, 0.25);
        var dark = ColorHelper.Darken(baseColor, 0.25);
        var hover = ColorHelper.Lighten(baseColor, 0.1);
        var pressed = ColorHelper.Darken(baseColor, 0.15);
        var disabled = Color.FromArgb(100, baseColor.R, baseColor.G, baseColor.B);
        var soft = Color.FromArgb(31, baseColor.R, baseColor.G, baseColor.B);

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
            PrimarySoft = soft
        };
    }
}
