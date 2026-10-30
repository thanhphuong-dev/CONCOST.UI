namespace CONCOST.UI.Controls.Ribbon;

/// <summary>
/// Where the menu bar sits relative to the ribbon body.
/// </summary>
public enum RibbonMenuBarPlacement
{
    /// <summary>
    /// Above the tab strip. When the quick-access toolbar is also set to <see cref="RibbonQuickAccessPlacement.AboveRibbon"/>,
    /// the menu bar lands between the QAT and the tabs — the classic CAD/BIM layout.
    /// </summary>
    AboveRibbon,

    /// <summary>
    /// Under the ribbon content area, above the QAT if the QAT is set to <see cref="RibbonQuickAccessPlacement.BelowRibbon"/>.
    /// </summary>
    BelowRibbon
}
