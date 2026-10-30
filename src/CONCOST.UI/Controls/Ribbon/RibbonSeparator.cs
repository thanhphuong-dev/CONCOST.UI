using System.Windows;
using System.Windows.Controls;

namespace CONCOST.UI.Controls.Ribbon;

/// <summary>Vertical divider between two logical clusters inside a <see cref="RibbonGroup"/>.</summary>
public class RibbonSeparator : Control
{
    static RibbonSeparator()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(RibbonSeparator),
            new FrameworkPropertyMetadata(typeof(RibbonSeparator)));
    }

    public RibbonSeparator()
    {
        IsTabStop = false;
        Focusable = false;
    }
}
