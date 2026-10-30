using System.Windows;

namespace CONCOST.UI.Controls.Ribbon;

/// <summary>
/// The icon-only button the quick-access toolbar uses. It exists as its own type so the theme can
/// give it a style without the caller having to look a resource key up by hand.
/// </summary>
public class RibbonQuickAccessButton : RibbonButton
{
    static RibbonQuickAccessButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(RibbonQuickAccessButton),
            new FrameworkPropertyMetadata(typeof(RibbonQuickAccessButton)));
    }

    public RibbonQuickAccessButton()
    {
        Size = RibbonItemSize.Small;
    }
}
