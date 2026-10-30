using System.Windows;
using System.Windows.Controls;

namespace CONCOST.UI.Controls.Ribbon;

/// <summary>Horizontal rule inside a <see cref="RibbonMenu"/>.</summary>
public class RibbonMenuSeparator : Control
{
    static RibbonMenuSeparator()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(RibbonMenuSeparator),
            new FrameworkPropertyMetadata(typeof(RibbonMenuSeparator)));
    }

    public RibbonMenuSeparator()
    {
        IsTabStop = false;
        Focusable = false;
    }
}
