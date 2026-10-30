namespace CONCOST.UI.Controls.Ribbon;

/// <summary>
/// How tightly a <see cref="RibbonGroup"/> packs its items. The ribbon lowers this automatically
/// when it runs out of width: Full → Medium → Small.
/// </summary>
public enum RibbonGroupSizeMode
{
    /// <summary>Items use the size they declare.</summary>
    Full,

    /// <summary>Large items are demoted to Medium.</summary>
    Medium,

    /// <summary>Every item is drawn as Small.</summary>
    Small
}
