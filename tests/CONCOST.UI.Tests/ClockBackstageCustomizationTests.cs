using System.Windows.Media;
using CONCOST.UI.Controls;
using CONCOST.UI.Controls.Ribbon;
using Xunit;

namespace CONCOST.UI.Tests;

/// <summary>
/// The clock had no way to change its accent, and the backstage baked "Back" and a 216 px rail.
/// Both are properties now.
/// </summary>
public class ClockBackstageCustomizationTests
{
    [Fact]
    public void Clock_accent_can_be_overridden()
    {
        WpfApplicationHost.Run(() =>
        {
            var clock = new Clock();

            Assert.Null(clock.AccentBrush);

            clock.AccentBrush = Brushes.Red;
            Assert.Same(Brushes.Red, clock.AccentBrush);
        });
    }

    [Fact]
    public void Backstage_back_text_and_rail_width_can_be_overridden()
    {
        WpfApplicationHost.Run(() =>
        {
            var backstage = new RibbonBackstageView();

            Assert.Equal("Back", backstage.BackText);
            Assert.Equal(216, backstage.RailWidth);

            backstage.BackText = "Quay lại";
            backstage.RailWidth = 280;

            Assert.Equal("Quay lại", backstage.BackText);
            Assert.Equal(280, backstage.RailWidth);
            Assert.Equal(280, backstage.ColumnDefinitions[0].Width.Value);
        });
    }
}
