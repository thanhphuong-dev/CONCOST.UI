using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace CONCOST.UI.AttachedProperties;

/// <summary>
/// Turns a plain <see cref="ComboBox"/> into a searchable list without making it editable: a search
/// box appears at the top of the drop-down and narrows the items as the user types, while the
/// selection box itself stays read-only.
/// </summary>
/// <remarks>
/// The filter is applied to the combo box's own <see cref="ItemCollection"/>, so the bound
/// <c>ItemsSource</c> is never touched and two combo boxes can share one source. The term is cleared
/// when the drop-down closes, so reopening always starts from the full list.
/// </remarks>
public static class ComboBoxHelper
{
    #region IsSearchEnabled

    /// <summary>Shows a search box at the top of the drop-down. The combo box stays non-editable.</summary>
    public static readonly DependencyProperty IsSearchEnabledProperty =
        DependencyProperty.RegisterAttached(
            "IsSearchEnabled",
            typeof(bool),
            typeof(ComboBoxHelper),
            new FrameworkPropertyMetadata(false, OnIsSearchEnabledChanged));

    public static bool GetIsSearchEnabled(DependencyObject obj) => (bool)obj.GetValue(IsSearchEnabledProperty);

    public static void SetIsSearchEnabled(DependencyObject obj, bool value) => obj.SetValue(IsSearchEnabledProperty, value);

    private static void OnIsSearchEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not ComboBox combo) return;

        combo.DropDownOpened -= OnDropDownOpened;
        combo.DropDownClosed -= OnDropDownClosed;

        if (e.NewValue is true)
        {
            combo.DropDownOpened += OnDropDownOpened;
            combo.DropDownClosed += OnDropDownClosed;
        }
        else
        {
            ResetSearch(combo);
        }
    }

    #endregion

    #region SearchText

    /// <summary>The term currently filtering the list. Two-way, so the template's box can drive it.</summary>
    public static readonly DependencyProperty SearchTextProperty =
        DependencyProperty.RegisterAttached(
            "SearchText",
            typeof(string),
            typeof(ComboBoxHelper),
            new FrameworkPropertyMetadata(
                string.Empty,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnSearchTextChanged));

    public static string GetSearchText(DependencyObject obj) => (string)obj.GetValue(SearchTextProperty);

    public static void SetSearchText(DependencyObject obj, string value) => obj.SetValue(SearchTextProperty, value);

    private static void OnSearchTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ComboBox combo && GetIsSearchEnabled(combo)) ApplyFilter(combo, e.NewValue as string);
    }

    #endregion

    #region SearchPlaceholder

    /// <summary>Placeholder inside the search box.</summary>
    public static readonly DependencyProperty SearchPlaceholderProperty =
        DependencyProperty.RegisterAttached(
            "SearchPlaceholder",
            typeof(string),
            typeof(ComboBoxHelper),
            new FrameworkPropertyMetadata("Search…"));

    public static string GetSearchPlaceholder(DependencyObject obj) => (string)obj.GetValue(SearchPlaceholderProperty);

    public static void SetSearchPlaceholder(DependencyObject obj, string value) => obj.SetValue(SearchPlaceholderProperty, value);

    #endregion

    private static void OnDropDownOpened(object? sender, EventArgs e)
    {
        if (sender is not ComboBox combo) return;

        ApplyFilter(combo, GetSearchText(combo));

        // The search box is the whole point of opening the list, so it takes the caret straight away.
        if (combo.Template?.FindName("PART_SearchBox", combo) is TextBox box)
        {
            box.Focus();
            box.SelectAll();
        }
    }

    private static void OnDropDownClosed(object? sender, EventArgs e)
    {
        if (sender is ComboBox combo) ResetSearch(combo);
    }

    /// <summary>Clears the term and shows the full list again — what closing the drop-down does.</summary>
    public static void ResetSearch(ComboBox combo)
    {
        if (combo is null) return;

        if (!string.IsNullOrEmpty(GetSearchText(combo))) SetSearchText(combo, string.Empty);

        if (combo.Items.Filter is not null) combo.Items.Filter = null;
    }

    /// <summary>Narrows <see cref="ItemsControl.Items"/> — never the bound source.</summary>
    private static void ApplyFilter(ComboBox combo, string? text)
    {
        var term = text?.Trim() ?? string.Empty;

        if (term.Length == 0)
        {
            combo.Items.Filter = null;
            return;
        }

        combo.Items.Filter = item => item is not null &&
                                     TextOf(item, combo).IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0;
    }

    /// <summary>
    /// The text a row shows. <c>DisplayMemberPath</c> and <c>TextSearch.TextPath</c> both describe
    /// it; without either, the item's own <c>ToString()</c> is what the list renders.
    /// </summary>
    private static string TextOf(object item, ComboBox combo)
    {
        var path = combo.DisplayMemberPath;

        if (string.IsNullOrEmpty(path))
        {
            path = TextSearch.GetTextPath(combo);
        }

        if (!string.IsNullOrEmpty(path))
        {
            var property = item.GetType().GetProperty(path!);
            if (property is not null) return property.GetValue(item)?.ToString() ?? string.Empty;
        }

        return item.ToString() ?? string.Empty;
    }
}
