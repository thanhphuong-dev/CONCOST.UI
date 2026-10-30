using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CONCOST.UI.Controls.Ribbon;

/// <summary>
/// The small arrow in the bottom-right corner of a <see cref="RibbonGroup"/> that opens the dialog
/// behind the group — “Font settings”, “Paragraph settings” and so on.
/// <para>
/// It is a <see cref="Button"/>, so <see cref="ButtonBase.Command"/> and
/// <see cref="ButtonBase.Click"/> behave exactly as they do everywhere else. The group's template
/// binds it to <see cref="RibbonGroup.DialogLauncherCommand"/> and hides it when that command is
/// missing, because an arrow that does nothing is worse than no arrow.
/// </para>
/// <para>
/// When the group collapses into a popup the arrow is gone, so the same command is offered as the
/// last row of <see cref="RibbonGroup.CollapsedMenu"/>.
/// </para>
/// </summary>
public class RibbonGroupLauncher : Button
{
    static RibbonGroupLauncher()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(RibbonGroupLauncher),
            new FrameworkPropertyMetadata(typeof(RibbonGroupLauncher)));
    }

    public RibbonGroupLauncher()
    {
        // The group is reached from the keyboard as a whole; the corner arrow must not take a
        // tab stop of its own.
        Focusable = false;
        IsTabStop = false;
        Cursor = Cursors.Hand;
    }
}
