using System.Windows;

namespace CONCOST.UI.Controls.Ribbon;

/// <summary>
/// A ribbon-sized check box: a square tick glyph followed by the label, exactly one row tall.
/// <para>
/// It derives from <see cref="RibbonToggleButton"/> on purpose — the checked state, the command
/// plumbing and the optional <see cref="RibbonToggleButton.GroupName"/> grouping are the same, only
/// the look differs. Use it for options that stay on (snap, ortho, grid), and
/// <see cref="RibbonToggleButton"/> for commands that look like buttons.
/// </para>
/// </summary>
public class RibbonCheckBox : RibbonToggleButton
{
    static RibbonCheckBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(RibbonCheckBox),
            new FrameworkPropertyMetadata(typeof(RibbonCheckBox)));
    }
}
