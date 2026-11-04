using CONCOST.UI.Controls;
using Xunit;

namespace CONCOST.UI.Tests;

/// <summary>
/// The picker used to fix its two combos and its style toggles at 26 px and its preview at 48 px.
/// Both are properties now.
/// </summary>
public class FontPickerCustomizationTests
{
    [Fact]
    public void Row_and_preview_heights_can_be_overridden()
    {
        WpfApplicationHost.Run(() =>
        {
            var picker = new FontPicker();

            Assert.Equal(26, picker.RowHeight);
            Assert.Equal(48, picker.PreviewMaxHeight);

            picker.RowHeight = 34;
            picker.PreviewMaxHeight = 80;

            Assert.Equal(34, picker.RowHeight);
            Assert.Equal(80, picker.PreviewMaxHeight);
        });
    }
}
