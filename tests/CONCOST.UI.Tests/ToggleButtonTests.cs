using System.Threading;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using CONCOST.UI.AttachedProperties;
using CONCOST.UI.Core.Helpers;
using CONCOST.UI.Icons;
using CONCOST.UI.Themes.Tools;
using Xunit;

namespace CONCOST.UI.Tests;

public class ToggleButtonTests
{
    [Fact]
    public void ToggleButton_IconKind_CanBeAttached()
    {
        WpfApplicationHost.Run(() =>
        {
            var btn = new ToggleButton();
            IconElement.SetKind(btn, IconKind.Eye);
            Assert.Equal(IconKind.Eye, IconElement.GetKind(btn));

            btn.IsChecked = true;
            Assert.True(btn.IsChecked);

            IconElement.SetKind(btn, IconKind.EyeOff);
            Assert.Equal(IconKind.EyeOff, IconElement.GetKind(btn));
        });
    }

    [Fact]
    public void ToggleButton_PrimaryAndWhite_HasSuperiorContrastOverPrimaryLight()
    {
        var primaryColor = ColorHelper.FromHex("#FF7900");
        var primaryLightColor = ColorHelper.FromHex("#FFA040");
        var whiteColor = Colors.White;

        var contrastOldBug = ContrastChecker.Check(primaryColor, primaryLightColor);
        var contrastFixed = ContrastChecker.Check(whiteColor, primaryColor);

        // The old bug (#FF7900 on #FFA040) had terrible unreadable contrast (< 1.5:1)
        Assert.True(contrastOldBug.Ratio < 1.5, $"Old bug contrast was {contrastOldBug.Ratio}");
        // The fixed White-on-Primary contrast is at least 2.5:1, much higher contrast
        Assert.True(contrastFixed.Ratio >= 2.5, $"Fixed contrast is {contrastFixed.Ratio}");
    }

    [Fact]
    public void ToggleButton_Icons_HaveValidGeometries()
    {
        var icons = new[]
        {
            IconKind.Lock,
            IconKind.Unlock,
            IconKind.Filter,
            IconKind.Layers,
            IconKind.BookmarkItem,
            IconKind.Check,
            IconKind.Eye,
            IconKind.EyeOff
        };

        foreach (var icon in icons)
        {
            var geo = IconProvider.GetGeometry(icon);
            Assert.NotNull(geo);
            Assert.False(geo.IsEmpty());
        }
    }
}
