using System.Windows;

namespace CONCOST.UI.Controls.Ribbon;

/// <summary>
/// A ribbon button that stays checked once clicked and never unchecks itself — combine with
/// <see cref="RibbonToggleButton.GroupName"/> so only one button in the group is active.
/// </summary>
public class RibbonRadioButton : RibbonToggleButton
{
    static RibbonRadioButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(RibbonRadioButton),
            new FrameworkPropertyMetadata(typeof(RibbonRadioButton)));
    }

    protected override System.Windows.Automation.Peers.AutomationPeer OnCreateAutomationPeer() =>
        new RibbonRadioButtonAutomationPeer(this);

    protected override void OnClick()
    {
        // Clicking an already-checked radio keeps it checked.
        if (IsChecked) return;
        base.OnClick();
        IsChecked = true;
    }
}
