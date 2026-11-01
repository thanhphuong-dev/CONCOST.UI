namespace CONCOST.UI.Controls.Ribbon;

/// <summary>
/// How much of the ribbon is on screen. The order is the order the minimise button cycles
/// through: every step removes one row of chrome, and the last step folds each panel into the
/// single button that reopens it.
/// </summary>
public enum RibbonDisplayMode
{
    /// <summary>Tab strip and the full panel bodies.</summary>
    Full,

    /// <summary>Only the tab strip; clicking a tab opens the ribbon as an overlay.</summary>
    MinimizeToTabs,

    /// <summary>Tab strip plus the panel titles, with their bodies hidden.</summary>
    MinimizeToPanelTitles,

    /// <summary>Tab strip plus one collapsed button per panel.</summary>
    MinimizeToPanelButtons
}
