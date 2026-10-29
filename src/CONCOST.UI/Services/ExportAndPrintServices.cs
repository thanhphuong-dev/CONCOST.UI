using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CONCOST.UI.Services;

public class ExportService
{
    private static readonly Lazy<ExportService> _current = new(() => new ExportService());
    public static ExportService Current => _current.Value;

    public void ExportDataGridToCsv(DataGrid grid, string filePath)
    {
        if (grid == null || string.IsNullOrWhiteSpace(filePath)) return;

        var sb = new StringBuilder();
        var cols = grid.Columns.ToList();

        // Header
        sb.AppendLine(string.Join(",", cols.Select(c => $"\"{c.Header}\"")));

        // Rows
        foreach (var item in grid.Items)
        {
            if (item == null) continue;
            var values = new List<string>();
            foreach (var col in cols)
            {
                if (col is DataGridBoundColumn boundCol && boundCol.Binding is System.Windows.Data.Binding binding)
                {
                    var prop = item.GetType().GetProperty(binding.Path.Path);
                    var val = prop?.GetValue(item)?.ToString() ?? "";
                    values.Add($"\"{val.Replace("\"", "\"\"")}\"");
                }
            }
            sb.AppendLine(string.Join(",", values));
        }

        File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
    }

    public void ExportVisualToPng(FrameworkElement visual, string filePath, double dpi = 96)
    {
        if (visual == null || string.IsNullOrWhiteSpace(filePath)) return;

        double width = visual.ActualWidth > 0 ? visual.ActualWidth : visual.Width;
        double height = visual.ActualHeight > 0 ? visual.ActualHeight : visual.Height;

        if (width <= 0 || height <= 0) return;

        var rtb = new RenderTargetBitmap((int)width, (int)height, dpi, dpi, PixelFormats.Pbgra32);
        rtb.Render(visual);

        var enc = new PngBitmapEncoder();
        enc.Frames.Add(BitmapFrame.Create(rtb));

        using var stm = File.Create(filePath);
        enc.Save(stm);
    }
}

public class PrintService
{
    private static readonly Lazy<PrintService> _current = new(() => new PrintService());
    public static PrintService Current => _current.Value;

    public void PrintVisual(Visual visual, string jobDescription = "CONCOST Print Document")
    {
        if (visual == null) return;

        var dlg = new PrintDialog();
        if (dlg.ShowDialog() == true)
        {
            dlg.PrintVisual(visual, jobDescription);
        }
    }
}
