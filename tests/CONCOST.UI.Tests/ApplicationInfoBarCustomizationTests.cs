using CONCOST.UI.Controls;
using Xunit;

namespace CONCOST.UI.Tests;

/// <summary>
/// The info bar used to fix its icon at 14 px, its action at 76 px and its text at 12 pt. Each is
/// now a property, and the icon can be dropped entirely.
/// </summary>
public class ApplicationInfoBarCustomizationTests
{
    [Fact]
    public void Icon_action_and_text_metrics_can_be_overridden()
    {
        WpfApplicationHost.Run(() =>
        {
            var bar = new ApplicationInfoBar();

            Assert.Equal(14, bar.IconSize);
            Assert.True(bar.ShowIcon);
            Assert.Equal(76, bar.ActionMinWidth);
            Assert.Equal(12, bar.TextFontSize);

            bar.IconSize = 20;
            bar.ShowIcon = false;
            bar.ActionMinWidth = 120;
            bar.TextFontSize = 13;

            Assert.Equal(20, bar.IconSize);
            Assert.False(bar.ShowIcon);
            Assert.Equal(120, bar.ActionMinWidth);
            Assert.Equal(13, bar.TextFontSize);
        });
    }
}
