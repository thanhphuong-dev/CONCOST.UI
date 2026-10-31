using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Input;
using CONCOST.UI.Icons;

namespace CONCOST.UI.Controls;

/// <summary>
/// One entry in a <see cref="CommandPalette"/>: what to show, what to search on and what to run.
/// </summary>
/// <remarks>
/// Kept deliberately plain — no <c>INotifyPropertyChanged</c>, no MVVM base class — so a host can
/// build the list from a projection of its own commands without writing a view model per entry.
/// Filtering uses <see cref="SearchText"/>, which is the title, subtitle, category, shortcut and
/// <see cref="Keywords"/> glued together and stripped of diacritics.
/// </remarks>
public class CommandPaletteItem
{
    public CommandPaletteItem()
    {
    }

    public CommandPaletteItem(string title, string? subtitle = null, ICommand? command = null,
        IconKind icon = IconKind.None, string? shortcut = null, string? category = null)
    {
        Title = title;
        Subtitle = subtitle;
        Command = command;
        Icon = icon;
        Shortcut = shortcut;
        Category = category;
    }

    /// <summary>Primary line.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Second line, usually what the command does.</summary>
    public string? Subtitle { get; set; }

    /// <summary>Gesture shown on the right, for example <c>Ctrl+S</c>.</summary>
    public string? Shortcut { get; set; }

    /// <summary>Group caption, also searched.</summary>
    public string? Category { get; set; }

    /// <summary>Icon drawn on the left.</summary>
    public IconKind Icon { get; set; } = IconKind.None;

    /// <summary>Command run by <see cref="Invoke"/>.</summary>
    public ICommand? Command { get; set; }

    /// <summary>Parameter passed to <see cref="Command"/>. Falls back to the item itself.</summary>
    public object? CommandParameter { get; set; }

    /// <summary>Extra search terms that are not displayed.</summary>
    public string? Keywords { get; set; }

    /// <summary>Anything the host wants to carry along.</summary>
    public object? Tag { get; set; }

    /// <summary>Disabled entries stay visible but cannot be run.</summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>True when <see cref="Command"/> is set and currently executable.</summary>
    public bool CanInvoke
    {
        get
        {
            if (!IsEnabled) return false;

            ICommand? command = Command;
            return command is not null && command.CanExecute(CommandParameter ?? this);
        }
    }

    /// <summary>
    /// Everything a search can match on, lower-cased and stripped of diacritics so
    /// "tong quan" finds "Tổng quan".
    /// </summary>
    public string SearchText => CommandPaletteText.Normalize(
        string.Join(" ", new[] { Title, Subtitle, Category, Shortcut, Keywords }
            .Where(part => !string.IsNullOrWhiteSpace(part))));

    /// <summary>Runs <see cref="Command"/> when it can execute. Returns false when nothing ran.</summary>
    public bool Invoke()
    {
        if (!IsEnabled) return false;

        ICommand? command = Command;
        if (command is null) return false;

        object parameter = CommandParameter ?? this;
        if (!command.CanExecute(parameter)) return false;

        command.Execute(parameter);
        return true;
    }

    public override string ToString() => Title;
}

/// <summary>Search helpers shared by the palette and its tests.</summary>
public static class CommandPaletteText
{
    /// <summary>
    /// Lower-cases and removes diacritics, including the Vietnamese <c>đ</c>, which
    /// <see cref="NormalizationForm.FormD"/> leaves alone.
    /// </summary>
    public static string Normalize(string? value)
    {
        if (value is null || value.Length == 0) return string.Empty;

        string decomposed = value.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(decomposed.Length);

        foreach (char c in decomposed)
        {
            if (char.IsWhiteSpace(c))
            {
                builder.Append(' ');
                continue;
            }

            if (c == 'đ' || c == 'Đ')
            {
                builder.Append('d');
                continue;
            }

            if (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark) continue;

            builder.Append(char.ToLowerInvariant(c));
        }

        return builder.ToString().Normalize(NormalizationForm.FormC);
    }

    /// <summary>Splits a query into search terms, dropping empty ones.</summary>
    public static IReadOnlyList<string> Tokenize(string? query)
    {
        if (string.IsNullOrWhiteSpace(query)) return Array.Empty<string>();

        string[] parts = Normalize(query).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        return parts.Length == 0 ? Array.Empty<string>() : parts;
    }

    /// <summary>True when every token appears somewhere in the item's search text.</summary>
    public static bool Matches(CommandPaletteItem item, IReadOnlyList<string> tokens)
    {
        if (tokens.Count == 0) return true;

        string haystack = item.SearchText;
        for (int i = 0; i < tokens.Count; i++)
        {
            if (haystack.IndexOf(tokens[i], StringComparison.Ordinal) < 0) return false;
        }

        return true;
    }
}
