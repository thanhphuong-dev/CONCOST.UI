using System.Windows.Controls;
using CONCOST.UI.Controls;
using Xunit;

namespace CONCOST.UI.Tests;

/// <summary>
/// The spreadsheet's cell menu used to be an English-only menu built inside the control. A host can
/// now supply its own menu and its own status text.
/// </summary>
public class SpreadsheetCustomizationTests
{
    [Fact]
    public void Context_menu_and_ready_text_can_be_overridden()
    {
        WpfApplicationHost.Run(() =>
        {
            var sheet = new Spreadsheet();

            Assert.Null(sheet.CellContextMenu);
            Assert.Equal("Ready", sheet.ReadyText);

            var menu = new ContextMenu();
            menu.Items.Add(new MenuItem { Header = "Cắt" });
            sheet.CellContextMenu = menu;

            Assert.Same(menu, sheet.CellContextMenu);

            sheet.ReadyText = "Sẵn sàng";
            Assert.Equal("Sẵn sàng", sheet.ReadyText);
        });
    }
}
