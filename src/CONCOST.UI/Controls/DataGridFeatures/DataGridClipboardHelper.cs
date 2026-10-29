using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace CONCOST.UI.Controls;

public static class DataGridClipboardHelper
{
    public static void CopyToClipboard(StackedHeaderDataGrid grid, bool includeHeaders = true)
    {
        if (grid == null || grid.ItemsSource == null) return;

        var sb = new StringBuilder();
        var leaves = grid.ColumnHeaderTree?.SelectMany(n => n.GetLeaves()).ToList();

        if (leaves == null || leaves.Count == 0)
        {
            leaves = grid.Columns.Select(c => new ColumnLeaf
            {
                HeaderText = c.Header?.ToString() ?? "",
                BindingPath = (c is DataGridBoundColumn bc && bc.Binding is System.Windows.Data.Binding b) ? b.Path.Path : ""
            }).ToList();
        }

        if (includeHeaders)
        {
            var headerTitles = leaves.Select(l => l.HeaderText).ToArray();
            sb.AppendLine(string.Join("\t", headerTitles));
        }

        var items = new List<object>();
        if (grid.SelectedItems != null && grid.SelectedItems.Count > 0)
        {
            foreach (var item in grid.SelectedItems) items.Add(item);
        }
        else
        {
            foreach (var item in grid.ItemsSource) items.Add(item);
        }

        foreach (var item in items)
        {
            var rowValues = new List<string>();
            foreach (var leaf in leaves)
            {
                if (!string.IsNullOrEmpty(leaf.BindingPath))
                {
                    var val = GetPropertyValue(item, leaf.BindingPath);
                    rowValues.Add(val?.ToString()?.Replace("\t", " ")?.Replace("\r", "")?.Replace("\n", " ") ?? "");
                }
                else
                {
                    rowValues.Add("");
                }
            }
            sb.AppendLine(string.Join("\t", rowValues));
        }

        try
        {
            Clipboard.SetText(sb.ToString());
        }
        catch { }
    }

    public static string ExportToCsv(StackedHeaderDataGrid grid)
    {
        if (grid == null || grid.ItemsSource == null) return string.Empty;

        var sb = new StringBuilder();
        var leaves = grid.ColumnHeaderTree?.SelectMany(n => n.GetLeaves()).ToList() ??
                     grid.Columns.Select(c => new ColumnLeaf { HeaderText = c.Header?.ToString() ?? "" }).ToList();

        sb.AppendLine(string.Join(",", leaves.Select(l => $"\"{l.HeaderText.Replace("\"", "\"\"")}\"")));

        foreach (var item in grid.ItemsSource)
        {
            var rowValues = new List<string>();
            foreach (var leaf in leaves)
            {
                var val = GetPropertyValue(item, leaf.BindingPath);
                var str = val?.ToString() ?? "";
                rowValues.Add($"\"{str.Replace("\"", "\"\"")}\"");
            }
            sb.AppendLine(string.Join(",", rowValues));
        }

        return sb.ToString();
    }

    private static object? GetPropertyValue(object obj, string propertyName)
    {
        if (obj == null || string.IsNullOrEmpty(propertyName)) return null;
        try
        {
            var prop = obj.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            return prop?.GetValue(obj);
        }
        catch { return null; }
    }
}
