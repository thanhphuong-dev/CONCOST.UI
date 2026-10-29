global using CONCOST.UI.Compatibility;

namespace CONCOST.UI.Compatibility;

/// <summary>Small runtime compatibility helpers used when building for .NET Framework 4.8.</summary>
internal static class NumericCompatibility
{
    internal static bool IsFinite(double value) => !double.IsNaN(value) && !double.IsInfinity(value);

    internal static int Clamp(int value, int minimum, int maximum) =>
        value < minimum ? minimum : value > maximum ? maximum : value;

    internal static double Clamp(double value, double minimum, double maximum) =>
        value < minimum ? minimum : value > maximum ? maximum : value;
}

#if NET48
internal static class StringCompatibilityExtensions
{
    internal static bool Contains(this string source, string value, StringComparison comparison) =>
        source.IndexOf(value, comparison) >= 0;

    internal static string Replace(this string source, string oldValue, string? newValue, StringComparison comparison)
    {
        if (string.IsNullOrEmpty(oldValue))
            throw new ArgumentException("The value to replace cannot be empty.", nameof(oldValue));

        var result = new System.Text.StringBuilder();
        var start = 0;
        var match = source.IndexOf(oldValue, start, comparison);
        while (match >= 0)
        {
            result.Append(source, start, match - start);
            result.Append(newValue);
            start = match + oldValue.Length;
            match = source.IndexOf(oldValue, start, comparison);
        }

        result.Append(source, start, source.Length - start);
        return result.ToString();
    }
}
#endif
