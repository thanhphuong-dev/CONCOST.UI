using System.Windows.Media;

namespace CONCOST.UI.Themes.Tools;

public class ContrastResult
{
    public double Ratio { get; set; }
    public bool PassesAA { get; set; }
    public bool PassesAAA { get; set; }
}

public static class ContrastChecker
{
    public static ContrastResult Check(Color fg, Color bg)
    {
        double l1 = GetRelativeLuminance(fg);
        double l2 = GetRelativeLuminance(bg);

        double lighter = Math.Max(l1, l2);
        double darker = Math.Min(l1, l2);
        double ratio = (lighter + 0.05) / (darker + 0.05);

        return new ContrastResult
        {
            Ratio = Math.Round(ratio, 2),
            PassesAA = ratio >= 4.5,
            PassesAAA = ratio >= 7.0
        };
    }

    private static double GetRelativeLuminance(Color c)
    {
        double r = TransformComponent(c.R / 255.0);
        double g = TransformComponent(c.G / 255.0);
        double b = TransformComponent(c.B / 255.0);
        return 0.2126 * r + 0.7152 * g + 0.0722 * b;
    }

    private static double TransformComponent(double val)
    {
        return val <= 0.03928 ? val / 12.92 : Math.Pow((val + 0.055) / 1.055, 2.4);
    }
}
