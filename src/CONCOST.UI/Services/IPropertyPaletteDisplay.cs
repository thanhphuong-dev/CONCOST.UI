namespace CONCOST.UI.Services;

/// <summary>Optional display labels for a target shown in the Properties palette.</summary>
public interface IPropertyPaletteDisplay
{
    /// <summary>Type or category label shown at the top of the palette.</summary>
    string PropertyTitle { get; }

    /// <summary>Instance or view label shown when no selector row is available.</summary>
    string PropertySubtitle { get; }
}
