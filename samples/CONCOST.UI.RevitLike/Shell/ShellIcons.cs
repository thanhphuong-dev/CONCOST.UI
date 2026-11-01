using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using CONCOST.UI.Icons;
using CONCOST.UI.Themes;

namespace CONCOST.UI.RevitLike.Shell;

/// <summary>
/// Turns an <see cref="IconKind"/> into an <see cref="ImageSource"/> for ribbon items.
/// </summary>
/// <remarks>
/// Every icon shares one unfrozen <see cref="SolidColorBrush"/>. Sharing is what makes the glyphs
/// follow a runtime theme switch: recolouring that single brush repaints all of them at once, where a
/// frozen drawing would keep the ink it was created with and go invisible in dark mode.
/// </remarks>
public static class ShellIcons
{
    private static readonly Color FallbackInk = Color.FromRgb(0x47, 0x54, 0x67);

    private static SolidColorBrush? _ink;

    private static SolidColorBrush Ink
    {
        get
        {
            if (_ink is not null) return _ink;

            _ink = new SolidColorBrush(CurrentInk());

            ThemeManager.Current.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName is not (nameof(ThemeManager.Theme) or nameof(ThemeManager.Preset) or nameof(ThemeManager.AccentColor)))
                    return;

                // ThemeManager raises the change before it swaps the dictionary, so read the new
                // brush on the next pass - otherwise the icons take the colour that just went away.
                Application.Current?.Dispatcher.BeginInvoke(
                    DispatcherPriority.Loaded,
                    new Action(() => _ink.Color = CurrentInk()));
            };

            return _ink;
        }
    }

    /// <summary>Geometry from the library's own icon set, drawn in the current text colour.</summary>
    public static ImageSource Get(IconKind kind)
    {
        Geometry? geometry = IconProvider.GetGeometry(kind);

        if (geometry is null) return new DrawingImage(new GeometryDrawing(Brushes.Transparent, null, Geometry.Empty));

        // Deliberately unfrozen: a frozen drawing cannot react to the shared brush changing.
        return new DrawingImage(new GeometryDrawing(Ink, null, geometry));
    }

    /// <summary>Re-reads the theme colour, for hosts that swap dictionaries by hand.</summary>
    public static void RefreshInk()
    {
        if (_ink is not null) _ink.Color = CurrentInk();
    }

    private static Color CurrentInk()
    {
        if (Application.Current?.TryFindResource("CONCOST.Brush.TextPrimary") is SolidColorBrush themed)
            return themed.Color;

        return FallbackInk;
    }
}
