using System.Windows.Automation.Peers;

namespace CONCOST.UI.Controls.Canvas;

/// <summary>Exposes the ruler to assistive technology.</summary>
public sealed class RulerControlAutomationPeer : FrameworkElementAutomationPeer
{
    public RulerControlAutomationPeer(RulerControl owner) : base(owner) { }

    protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.Custom;

    protected override string GetClassNameCore() => nameof(RulerControl);

    protected override string GetNameCore()
    {
        string name = base.GetNameCore();
        return string.IsNullOrWhiteSpace(name) ? "Ruler" : name;
    }
}
