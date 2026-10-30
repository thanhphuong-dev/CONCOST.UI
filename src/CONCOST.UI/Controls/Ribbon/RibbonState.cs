using System.Collections.Generic;

namespace CONCOST.UI.Controls.Ribbon;

/// <summary>
/// The part of a ribbon worth remembering between sessions: which tab was open, whether it was
/// minimised, which commands were pinned to the quick-access strip, and which groups the user
/// folded away by hand.
/// <para>
/// It is a plain data object on purpose — the library never writes a file. Capture it with
/// <see cref="Ribbon.CaptureState"/>, hand it to <see cref="RibbonStateStore"/> (or your own
/// serialiser), and give it back with <see cref="Ribbon.ApplyState"/>.
/// </para>
/// </summary>
public sealed class RibbonState
{
    /// <summary><see cref="RibbonTabItem.Id"/> of the tab that was open, or its header when no id was set.</summary>
    public string? SelectedTabId { get; set; }

    /// <summary>True when only the tab strip was showing.</summary>
    public bool IsMinimized { get; set; }

    /// <summary><see cref="RibbonItem.Id"/> of every command pinned to the quick-access strip.</summary>
    public List<string> QuickAccessItemIds { get; set; } = new();

    /// <summary>
    /// Where the quick-access strip was docked, as the name of a
    /// <see cref="RibbonQuickAccessPlacement"/> value. Stored as text so an old snapshot stays
    /// readable: an unknown name leaves the ribbon's current placement alone.
    /// </summary>
    public string? QuickAccessPlacement { get; set; }

    /// <summary>
    /// Groups the user collapsed by hand, as <c>tabId:header</c>. Groups the ribbon collapsed
    /// itself to save width are not stored — they come back on their own when there is room.
    /// </summary>
    public List<string> CollapsedGroupKeys { get; set; } = new();
}
