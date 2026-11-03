using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace CONCOST.UI.Services;

/// <summary>
/// Exports the visible content of a <see cref="DataGrid"/> to CSV, Excel-compatible XML,
/// or HTML formats without external dependencies.
/// </summary>
public static class DataGridExportService
{
    #region CSV

    /// <summary>Exports the grid to a comma-separated values file.</summary>
    public static void ExportToCsv(DataGrid grid, string filePath, char delimiter = ',')
    {
        var rows = GetVisibleRows(grid).ToList();
        var columns = grid.Columns.Where(c => c.Visibility == Visibility.Visible && !string.IsNullOrEmpty(HeaderText(c))).ToList();
        var delim = delimiter.ToString();

        using var writer = new StreamWriter(filePath, false, Encoding.UTF8);

        // Header
        writer.WriteLine(string.Join(delim, columns.Select(c => EscapeCsv(HeaderText(c), delimiter))));

        // Rows
        foreach (var row in rows)
        {
            var cells = columns.Select(col => EscapeCsv(FormatCellValue(row, col), delimiter));
            writer.WriteLine(string.Join(delim, cells));
        }
    }

    private static string EscapeCsv(string value, char delimiter)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;
        var needsQuotes = value.Contains(delimiter) || value.Contains('"') || value.Contains('\n') || value.Contains('\r');
        if (!needsQuotes) return value;
        return "\"" + value.Replace("\"", "\"\"") + "\"";
    }

    #endregion

    #region Excel XML Spreadsheet 2003

    /// <summary>
    /// Exports the grid to an Excel-compatible XML Spreadsheet 2003 file.
    /// Opens directly in Excel without requiring any third-party library.
    /// </summary>
    public static void ExportToExcelXml(DataGrid grid, string filePath, string? worksheetName = null)
    {
        var rows = GetVisibleRows(grid).ToList();
        var columns = grid.Columns.Where(c => c.Visibility == Visibility.Visible && !string.IsNullOrEmpty(HeaderText(c))).ToList();
        var sheetName = XmlEncode(worksheetName ?? "Sheet1");

        using var writer = new StreamWriter(filePath, false, Encoding.UTF8);
        writer.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        writer.WriteLine("<?mso-application progid=\"Excel.S\"?>");
        writer.WriteLine("<Workbook xmlns=\"urn:schemas-microsoft-com:office:spreadsheet\"");
        writer.WriteLine("          xmlns:o=\"urn:schemas-microsoft-com:office:office\"");
        writer.WriteLine("          xmlns:x=\"urn:schemas-microsoft-com:office:excel\"");
        writer.WriteLine("          xmlns:ss=\"urn:schemas-microsoft-com:office:spreadsheet\">");
        writer.WriteLine("  <Styles>");
        writer.WriteLine("    <Style ss:ID=\"Header\">");
        writer.WriteLine("      <Font ss:Bold=\"1\"/>");
        writer.WriteLine("      <Interior ss:Color=\"#4472C4\" ss:Pattern=\"Solid\"/>");
        writer.WriteLine("      <Font ss:Color=\"#FFFFFF\" ss:Bold=\"1\"/>");
        writer.WriteLine("    </Style>");
        writer.WriteLine("  </Styles>");
        writer.WriteLine($"  <Worksheet ss:Name=\"{sheetName}\">");
        writer.WriteLine($"    <Table ss:ExpandedColumnCount=\"{columns.Count}\" ss:ExpandedRowCount=\"{rows.Count + 1}\">");

        foreach (var col in columns)
        {
            var width = col.ActualWidth > 0 ? col.ActualWidth : 100;
            writer.WriteLine($"      <Column ss:Width=\"{width:F1}\"/>");
        }

        // Header row
        writer.WriteLine("      <Row>");
        foreach (var col in columns)
        {
            writer.WriteLine($"        <Cell ss:StyleID=\"Header\"><Data ss:Type=\"String\">{XmlEncode(HeaderText(col))}</Data></Cell>");
        }
        writer.WriteLine("      </Row>");

        // Data rows
        foreach (var row in rows)
        {
            writer.WriteLine("      <Row>");
            foreach (var col in columns)
            {
                var value = FormatCellValue(row, col);
                var type = IsNumeric(value) ? "Number" : "String";
                writer.WriteLine($"        <Cell><Data ss:Type=\"{type}\">{XmlEncode(value)}</Data></Cell>");
            }
            writer.WriteLine("      </Row>");
        }

        writer.WriteLine("    </Table>");
        writer.WriteLine("  </Worksheet>");
        writer.WriteLine("</Workbook>");
    }

    #endregion

    #region HTML

    /// <summary>Exports the grid to an HTML table that can be opened in browsers or printed to PDF.</summary>
    public static void ExportToHtml(DataGrid grid, string filePath, string? title = null)
    {
        var rows = GetVisibleRows(grid).ToList();
        var columns = grid.Columns.Where(c => c.Visibility == Visibility.Visible && !string.IsNullOrEmpty(HeaderText(c))).ToList();
        var pageTitle = XmlEncode(title ?? "Exported Data");

        using var writer = new StreamWriter(filePath, false, Encoding.UTF8);
        writer.WriteLine("<!DOCTYPE html>");
        writer.WriteLine("<html>");
        writer.WriteLine("<head>");
        writer.WriteLine($"<meta charset=\"UTF-8\"><title>{pageTitle}</title>");
        writer.WriteLine("<style>");
        writer.WriteLine("  body { font-family: Arial, sans-serif; margin: 24px; }");
        writer.WriteLine("  table { border-collapse: collapse; width: 100%; }");
        writer.WriteLine("  th, td { border: 1px solid #ccc; padding: 8px 12px; text-align: left; }");
        writer.WriteLine("  th { background: #4472C4; color: #fff; }");
        writer.WriteLine("  tr:nth-child(even) { background: #f2f2f2; }");
        writer.WriteLine("</style>");
        writer.WriteLine("</head>");
        writer.WriteLine("<body>");
        writer.WriteLine($"<h2>{pageTitle}</h2>");
        writer.WriteLine("<table>");

        // Header
        writer.WriteLine("  <tr>");
        foreach (var col in columns)
            writer.WriteLine($"    <th>{HtmlEncode(HeaderText(col))}</th>");
        writer.WriteLine("  </tr>");

        // Rows
        foreach (var row in rows)
        {
            writer.WriteLine("  <tr>");
            foreach (var col in columns)
                writer.WriteLine($"    <td>{HtmlEncode(FormatCellValue(row, col))}</td>");
            writer.WriteLine("  </tr>");
        }

        writer.WriteLine("</table>");
        writer.WriteLine("</body>");
        writer.WriteLine("</html>");
    }

    #endregion

    #region Helpers

    private static IEnumerable<object> GetVisibleRows(DataGrid grid)
    {
        var source = grid.ItemsSource;
        if (source is null) yield break;

        foreach (var item in source)
        {
            if (item == CollectionView.NewItemPlaceholder) continue;
            yield return item;
        }
    }

    private static string HeaderText(DataGridColumn column)
    {
        if (column.Header is string s) return s;
        return column.Header?.ToString() ?? string.Empty;
    }

    private static string FormatCellValue(object row, DataGridColumn column)
    {
        if (column is DataGridBoundColumn bound && bound.Binding is Binding binding)
        {
            var path = binding.Path?.Path;
            if (string.IsNullOrEmpty(path)) return string.Empty;

            object? current = row;
            foreach (var part in path!.Split('.'))
            {
                if (current is null) return string.Empty;
                current = ReadProperty(current, part);
            }
            return current?.ToString() ?? string.Empty;
        }

        if (column is DataGridCheckBoxColumn check)
        {
            var path = (check.Binding as Binding)?.Path?.Path;
            if (!string.IsNullOrEmpty(path))
            {
                var value = ReadProperty(row, path!);
                return value is true ? "Yes" : "No";
            }
        }

        return string.Empty;
    }

    private static object? ReadProperty(object target, string propertyName)
    {
        if (target is null) return null;
        var type = target.GetType();
        var prop = type.GetProperty(propertyName);
        if (prop is not null) return prop.GetValue(target);

        // Try field fallback
        var field = type.GetField(propertyName);
        return field?.GetValue(target);
    }

    private static bool IsNumeric(string value)
    {
        return double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out _);
    }

    private static string XmlEncode(string value)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;
        return value
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;")
            .Replace("'", "&apos;");
    }

    private static string HtmlEncode(string value)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;
        return value
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;");
    }

    #endregion
}
