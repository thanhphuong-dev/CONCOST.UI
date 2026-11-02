using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using CONCOST.UI.Services;
using Xunit;

namespace CONCOST.UI.Tests;

public class DataGridExportServiceTests
{
    private static DataGrid CreateSampleGrid()
    {
        var grid = new DataGrid
        {
            ItemsSource = new ObservableCollection<BeamSample>
            {
                new() { Name = "B1", Width = 200, Height = 400 },
                new() { Name = "B2", Width = 250, Height = 450 }
            }
        };
        grid.Columns.Add(new DataGridTextColumn { Header = "Name", Binding = new System.Windows.Data.Binding("Name") });
        grid.Columns.Add(new DataGridTextColumn { Header = "Width", Binding = new System.Windows.Data.Binding("Width") });
        grid.Columns.Add(new DataGridTextColumn { Header = "Height", Binding = new System.Windows.Data.Binding("Height") });
        return grid;
    }

    [Fact]
    public void ExportToCsv_CreatesFileWithHeaderAndRows()
    {
        WpfApplicationHost.Run(() =>
        {
            var grid = CreateSampleGrid();
            var path = Path.GetTempFileName() + ".csv";
            try
            {
                DataGridExportService.ExportToCsv(grid, path);
                var lines = File.ReadAllLines(path);
                Assert.True(lines.Length >= 3); // header + 2 rows
                Assert.Contains("Name", lines[0]);
                Assert.Contains("B1", lines[1]);
                Assert.Contains("B2", lines[2]);
            }
            finally
            {
                File.Delete(path);
            }
        });
    }

    [Fact]
    public void ExportToExcelXml_CreatesValidXml()
    {
        WpfApplicationHost.Run(() =>
        {
            var grid = CreateSampleGrid();
            var path = Path.GetTempFileName() + ".xml";
            try
            {
                DataGridExportService.ExportToExcelXml(grid, path, "TestSheet");
                var content = File.ReadAllText(path);
                Assert.Contains("<?xml version", content);
                Assert.Contains("Workbook", content);
                Assert.Contains("TestSheet", content);
                Assert.Contains("B1", content);
                Assert.Contains("B2", content);
            }
            finally
            {
                File.Delete(path);
            }
        });
    }

    [Fact]
    public void ExportToHtml_CreatesValidHtml()
    {
        WpfApplicationHost.Run(() =>
        {
            var grid = CreateSampleGrid();
            var path = Path.GetTempFileName() + ".html";
            try
            {
                DataGridExportService.ExportToHtml(grid, path, "Test Export");
                var content = File.ReadAllText(path);
                Assert.Contains("<!DOCTYPE html>", content);
                Assert.Contains("Test Export", content);
                Assert.Contains("<table>", content);
                Assert.Contains("B1", content);
                Assert.Contains("B2", content);
            }
            finally
            {
                File.Delete(path);
            }
        });
    }

    private class BeamSample
    {
        public string Name { get; set; } = string.Empty;
        public double Width { get; set; }
        public double Height { get; set; }
    }
}
