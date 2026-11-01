using System.Windows;
using System.Windows.Controls;

namespace CONCOST.UI.Controls.Ribbon;

/// <summary>
/// A line of static text inside a group — the caption a ribbon puts next to a cluster of options
/// (“Text height”, “Scale”). It is not interactive: it takes no focus, has no command and lets
/// clicks fall through to whatever sits behind it.
/// </summary>
/// <remarks>
/// A vertical rule is <see cref="RibbonSeparator"/>; this type is only for words.
/// </remarks>
public class RibbonLabel : RibbonItem
{
    static RibbonLabel()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(RibbonLabel),
            new FrameworkPropertyMetadata(typeof(RibbonLabel)));

        // A caption owns a single row, so it never needs a whole column of its own.
        SizeProperty.OverrideMetadata(typeof(RibbonLabel),
            new FrameworkPropertyMetadata(RibbonItemSize.Small));
    }

    public RibbonLabel()
    {
        Focusable = false;
        IsTabStop = false;
        IsHitTestVisible = false;
        HorizontalContentAlignment = HorizontalAlignment.Center;
        VerticalContentAlignment = VerticalAlignment.Center;
    }
}
