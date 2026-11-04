using CONCOST.UI.Controls;
using Xunit;

namespace CONCOST.UI.Tests;

/// <summary>
/// The palette used to bake five English strings into its constructor. A host can now localise or
/// rename every one of them, and drop the search box.
/// </summary>
public class PropertiesPaletteCustomizationTests
{
    [Fact]
    public void Chrome_strings_can_be_overridden()
    {
        WpfApplicationHost.Run(() =>
        {
            var palette = new PropertiesPalette();

            Assert.Equal("Nothing selected", palette.EmptyTitle);
            Assert.Equal("Select an element to edit its parameters.", palette.EmptySubtitle);
            Assert.Equal("Search parameters", palette.SearchPlaceholder);
            Assert.Equal("Edit type", palette.EditTypeText);
            Assert.True(palette.ShowSearch);

            palette.EmptyTitle = "Chưa chọn";
            palette.EmptySubtitle = "Chọn một cấu kiện để sửa tham số.";
            palette.EmptyHint = "Bấm vào tường để xem loại và kích thước.";
            palette.SearchPlaceholder = "Tìm tham số";
            palette.EditTypeText = "Sửa loại";
            palette.TypeToolTip = "Loại cấu kiện";
            palette.ResetToolTip = "Đặt lại tham số vừa sửa";
            palette.ShowSearch = false;

            Assert.Equal("Chưa chọn", palette.EmptyTitle);
            Assert.Equal("Sửa loại", palette.EditTypeText);
            Assert.False(palette.ShowSearch);
        });
    }
}
