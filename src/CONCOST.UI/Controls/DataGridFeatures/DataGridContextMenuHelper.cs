using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace CONCOST.UI.Controls;

public static class DataGridContextMenuHelper
{
    public static ContextMenu CreateDefaultContextMenu(StackedHeaderDataGrid grid)
    {
        var menu = new ContextMenu();

        var copyItem = new MenuItem { Header = "Copy Selection (Ctrl+C)" };
        copyItem.Click += (s, e) => DataGridClipboardHelper.CopyToClipboard(grid, false);
        menu.Items.Add(copyItem);

        var copyWithHeadersItem = new MenuItem { Header = "Copy with Headers (Excel TSV)" };
        copyWithHeadersItem.Click += (s, e) => DataGridClipboardHelper.CopyToClipboard(grid, true);
        menu.Items.Add(copyWithHeadersItem);

        menu.Items.Add(new Separator());

        var selectAllItem = new MenuItem { Header = "Select All (Ctrl+A)" };
        selectAllItem.Click += (s, e) => grid.SelectAll();
        menu.Items.Add(selectAllItem);

        menu.Items.Add(new Separator());

        // Quick Filter Sub-menu
        var filterMenu = new MenuItem { Header = "Filter Column..." };
        var leaves = grid.ColumnHeaderTree?.SelectMany(n => n.GetLeaves()).Where(l => !string.IsNullOrEmpty(l.BindingPath)).ToList();
        if (leaves != null && leaves.Count > 0)
        {
            foreach (var leaf in leaves)
            {
                var leafItem = new MenuItem { Header = $"Filter by '{leaf.HeaderText}'" };
                leafItem.Click += (s, e) => DataGridFilterHelper.ShowFilterPopup(grid, leaf, grid);
                filterMenu.Items.Add(leafItem);
            }
            menu.Items.Add(filterMenu);
        }

        menu.Items.Add(new Separator());

        var exportCsvItem = new MenuItem { Header = "Export Table to CSV (Clipboard)" };
        exportCsvItem.Click += (s, e) =>
        {
            var csv = DataGridClipboardHelper.ExportToCsv(grid);
            try
            {
                Clipboard.SetText(csv);
                MessageBox.Show("Table exported to clipboard as CSV successfully! You can paste directly into Excel.", "CONCOST DataGrid", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch { }
        };
        menu.Items.Add(exportCsvItem);

        return menu;
    }
}
