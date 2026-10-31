using System.Globalization;
using System.Text.RegularExpressions;

namespace CONCOST.UI.Controls.Engineering;

public class RebarInfo
{
    public int Quantity { get; set; }
    public int Diameter { get; set; }
    public double Spacing { get; set; }
    public bool IsStirrup { get; set; }
    public string RawText { get; set; } = string.Empty;
    public bool IsValid { get; set; }

    public double TotalAreaMm2
    {
        get
        {
            if (!IsValid || Diameter <= 0) return 0;
            double area1 = Math.PI * Math.Pow(Diameter / 2.0, 2);
            return Quantity > 0 ? area1 * Quantity : area1;
        }
    }
}

public static class RebarParser
{
    // Regex matches: "4D20", "2Ø16", "2d25", "D10@200", "Ø8@150", "2x3D18", "4Φ20", "2φ16", "4 phi 20"
    private static readonly Regex MainRebarRegex = new(@"^(?:(?<layer>\d+)x)?(?<count>\d+)(?:[dDØøΦφФф]|phi)(?<dia>\d+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, TimeSpan.FromMilliseconds(100));
    private static readonly Regex StirrupRegex = new(@"^(?:[dDØøΦφФф]|phi)(?<dia>\d+)@(?<spacing>\d+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, TimeSpan.FromMilliseconds(100));

    public static RebarInfo Parse(string? input)
    {
        var info = new RebarInfo { RawText = input?.Trim() ?? string.Empty };
        // `is not { Length: > 0 }` plus an explicit Trim check keeps the narrowing on net48,
        // where string.IsNullOrWhiteSpace has no [NotNullWhen(false)] annotation.
        if (input is not { Length: > 0 } || input.Trim().Length == 0)
            return info;

        string clean = input.Trim().Replace(" ", "");

        // Try main rebar (e.g. 4D20 or 2x3D18)
        var matchMain = MainRebarRegex.Match(clean);
        if (matchMain.Success)
        {
            if (!int.TryParse(matchMain.Groups["count"].Value, NumberStyles.None, CultureInfo.InvariantCulture, out int count) ||
                !int.TryParse(matchMain.Groups["dia"].Value, NumberStyles.None, CultureInfo.InvariantCulture, out int dia) ||
                count <= 0 || dia <= 0)
                return info;
            if (matchMain.Groups["layer"].Success)
            {
                if (!int.TryParse(matchMain.Groups["layer"].Value, NumberStyles.None, CultureInfo.InvariantCulture, out int layers) ||
                    layers <= 0 || count > int.MaxValue / layers)
                    return info;
                count *= layers;
            }

            info.Quantity = count;
            info.Diameter = dia;
            info.IsStirrup = false;
            info.IsValid = true;
            return info;
        }

        // Try stirrup (e.g. D10@200)
        var matchStirrup = StirrupRegex.Match(clean);
        if (matchStirrup.Success)
        {
            if (!int.TryParse(matchStirrup.Groups["dia"].Value, NumberStyles.None, CultureInfo.InvariantCulture, out int dia) ||
                !double.TryParse(matchStirrup.Groups["spacing"].Value, NumberStyles.None, CultureInfo.InvariantCulture, out double spacing) ||
                dia <= 0 || spacing <= 0)
                return info;

            info.Diameter = dia;
            info.Spacing = spacing;
            info.IsStirrup = true;
            info.IsValid = true;
            return info;
        }

        return info;
    }
}
