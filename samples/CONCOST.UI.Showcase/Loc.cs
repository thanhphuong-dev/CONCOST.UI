using System;
using CONCOST.UI.Services;

namespace CONCOST.UI.Showcase;

/// <summary>
/// Two-string switch for the demo pages. The shell is localised through
/// <see cref="LocalizationManager"/> keys; the demos are built imperatively, so a literal pair is
/// the cheapest thing that still follows the language selector.
/// </summary>
internal static class Loc
{
    /// <summary>True when the current culture is Vietnamese.</summary>
    public static bool IsVietnamese =>
        LocalizationManager.Current.Culture.TwoLetterISOLanguageName
            .Equals("vi", StringComparison.OrdinalIgnoreCase);

    /// <summary>Picks the Vietnamese or English literal for the current culture.</summary>
    public static string T(string vi, string en) => IsVietnamese ? vi : en;

    /// <summary>
    /// Formatted variant of <see cref="T(string, string)"/> for interpolated strings. Both branches
    /// take <c>{0}</c>, <c>{1}</c>… placeholders, so the sentence order can differ per language.
    /// </summary>
    public static string F(string vi, string en, params object?[] args) =>
        string.Format(IsVietnamese ? vi : en, args);

    /// <summary>
    /// Localises a code sample shown under <em>View code</em>. The sample stays Vietnamese in
    /// source — only its comments and literals are swapped, line by line.
    /// </summary>
    public static string Code(string code) => IsVietnamese ? code : LocCode.Translate(code);
}
