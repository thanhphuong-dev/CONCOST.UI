using System.Windows.Media;

namespace CONCOST.UI.Controls.Ribbon;

/// <summary>
/// One tile of a <see cref="RibbonGallery"/>: a picture plus an optional caption.
/// <para>
/// It is deliberately a plain data object rather than a control — the gallery owns the tile look
/// (size, hover, selection), so the same item can be shown both inline and in the expanded panel.
/// </para>
/// </summary>
public class RibbonGalleryItem
{
    public RibbonGalleryItem()
    {
    }

    public RibbonGalleryItem(string header, ImageSource? icon)
    {
        Header = header;
        Icon = icon;
    }

    /// <summary>Caption under the picture; also used as the tooltip and menu label.</summary>
    public string Header { get; set; } = string.Empty;

    /// <summary>Picture shown in the tile.</summary>
    public ImageSource? Icon { get; set; }

    /// <summary>Text shown on hover. Falls back to <see cref="Header"/>.</summary>
    public string? ToolTip { get; set; }

    /// <summary>Anything the application wants to carry along, e.g. a style id.</summary>
    public object? Tag { get; set; }

    public override string ToString() => Header;
}
