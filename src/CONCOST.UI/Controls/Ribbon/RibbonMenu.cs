using System.Windows;
using System.Windows.Controls;

namespace CONCOST.UI.Controls.Ribbon;

/// <summary>Vertical list of <see cref="RibbonMenuItem"/> shown inside a dropdown or split button.</summary>
public class RibbonMenu : ItemsControl
{
    static RibbonMenu()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(RibbonMenu),
            new FrameworkPropertyMetadata(typeof(RibbonMenu)));
    }
}
