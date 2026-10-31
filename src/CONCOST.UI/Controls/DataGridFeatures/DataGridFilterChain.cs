using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Data;

namespace CONCOST.UI.Controls;

/// <summary>
/// Keeps the filtering of one <see cref="DataGrid"/> composable.
/// </summary>
/// <remarks>
/// A grid filters through its <see cref="ICollectionView"/>, and a view holds exactly one
/// <see cref="ICollectionView.Filter"/> delegate. The filter row and the search box both want that
/// slot, so each one registers a predicate here and the chain hands the view a single composite
/// delegate. A row is visible only when every registered predicate accepts it, which is what a
/// user expects from “type in the search box, then narrow a column further”.
/// </remarks>
internal static class DataGridFilterChain
{
    private static readonly Dictionary<ICollectionView, Dictionary<object, Func<object, bool>>> Chains =
        new Dictionary<ICollectionView, Dictionary<object, Func<object, bool>>>();

    /// <summary>
    /// The view the grid actually filters through. A grid bound to a plain list still filters via
    /// its default view, so casting <c>ItemsSource</c> to <see cref="ICollectionView"/> is not enough.
    /// </summary>
    public static ICollectionView? ResolveView(DataGrid? grid)
    {
        if (grid?.ItemsSource is null) return null;

        return grid.ItemsSource as ICollectionView ?? CollectionViewSource.GetDefaultView(grid.ItemsSource);
    }

    /// <summary>Adds, replaces, or — when <paramref name="predicate"/> is <c>null</c> — removes one owner's predicate.</summary>
    public static void Set(ICollectionView? view, object owner, Func<object, bool>? predicate)
    {
        if (view is null) return;

        if (!Chains.TryGetValue(view, out var map))
        {
            if (predicate is null) return;

            map = new Dictionary<object, Func<object, bool>>();
            Chains[view] = map;
        }

        if (predicate is null) map.Remove(owner);
        else map[owner] = predicate;

        if (map.Count == 0)
        {
            Chains.Remove(view);
            view.Filter = null;
        }
        else
        {
            // The composite reads the live map, so a single delegate keeps working as the
            // registered predicates change underneath it.
            view.Filter = item =>
            {
                if (!Chains.TryGetValue(view, out var live)) return true;

                foreach (var test in live.Values)
                {
                    if (!test(item)) return false;
                }

                return true;
            };
        }

        view.Refresh();
    }

    /// <summary>Drops whatever an owner installed, for teardown or when the target grid goes away.</summary>
    public static void Clear(ICollectionView? view, object owner)
    {
        if (view is null) return;
        if (Chains.TryGetValue(view, out var map) && map.ContainsKey(owner))
        {
            Set(view, owner, null);
        }
    }

    /// <summary>
    /// Reads the value a column shows for a row, following the column's binding path — including
    /// dotted paths such as <c>Material.Grade</c>, which a single <c>GetProperty</c> would miss.
    /// </summary>
    public static object? ReadCell(object? row, DataGridColumn? column)
    {
        if (row is null) return null;
        if (column is not DataGridBoundColumn bound) return null;
        if (bound.Binding is not Binding binding) return null;

        var path = binding.Path?.Path;
        if (string.IsNullOrEmpty(path)) return null;

        object? current = row;
        foreach (var part in path!.Split('.'))
        {
            if (current is null) return null;

            // Indexers and attached-property syntax are out of scope for a quick filter.
            if (part.Length == 0 || part.IndexOf('[') >= 0) return null;

            var property = current.GetType().GetProperty(part);
            if (property is null) return null;

            current = property.GetValue(current);
        }

        return current;
    }

    /// <summary>The header text a column wants to show, for the filter row and the chooser.</summary>
    public static string HeaderText(DataGridColumn column)
    {
        if (column is null) return string.Empty;
        if (column.Header is string text) return text;

        return column.Header?.ToString() ?? string.Empty;
    }
}
