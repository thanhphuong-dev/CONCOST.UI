namespace CONCOST.UI.Controls.Ribbon;

/// <summary>
/// Where the quick-access strip sits relative to the ribbon.
/// </summary>
public enum RibbonQuickAccessPlacement
{
    /// <summary>
    /// Inline at the end of the tab row, on the same baseline as the tabs. This is the compact
    /// default: the strip costs no extra height, which matters for a ribbon inside a document window.
    /// </summary>
    HeaderRight,

    /// <summary>
    /// Its own thin strip above the tab row, left-aligned — the Office 2010 “show above the ribbon”
    /// position. Costs one extra row of height.
    /// </summary>
    AboveRibbon,

    /// <summary>
    /// Its own thin strip under the ribbon body, left-aligned — the Office “show below the ribbon”
    /// position. Handy when the strip carries labels and would crowd the tab row.
    /// </summary>
    BelowRibbon
}
