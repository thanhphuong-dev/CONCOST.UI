using System.Windows.Automation.Peers;

namespace CONCOST.UI.Controls;

/// <summary>Exposes the self-painted 3D viewport to assistive technology.</summary>
public sealed class ModelViewer3DAutomationPeer : FrameworkElementAutomationPeer
{
    public ModelViewer3DAutomationPeer(ModelViewer3D owner) : base(owner) { }

    protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.Custom;

    protected override string GetClassNameCore() => nameof(ModelViewer3D);

    protected override string GetNameCore()
    {
        string name = base.GetNameCore();
        return string.IsNullOrWhiteSpace(name) ? "3D model viewer" : name;
    }
}

/// <summary>Exposes the image viewer to assistive technology.</summary>
public sealed class ImageViewerAutomationPeer : FrameworkElementAutomationPeer
{
    public ImageViewerAutomationPeer(ImageViewer owner) : base(owner) { }

    protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.Custom;

    protected override string GetClassNameCore() => nameof(ImageViewer);

    protected override string GetNameCore()
    {
        string name = base.GetNameCore();
        return string.IsNullOrWhiteSpace(name) ? "Image viewer" : name;
    }
}

/// <summary>Exposes the magnifier to assistive technology.</summary>
public sealed class MagnifierAutomationPeer : FrameworkElementAutomationPeer
{
    public MagnifierAutomationPeer(Magnifier owner) : base(owner) { }

    protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.Custom;

    protected override string GetClassNameCore() => nameof(Magnifier);

    protected override string GetNameCore()
    {
        string name = base.GetNameCore();
        return string.IsNullOrWhiteSpace(name) ? "Magnifier" : name;
    }
}

/// <summary>Exposes the signature pad to assistive technology.</summary>
public sealed class SignaturePadAutomationPeer : FrameworkElementAutomationPeer
{
    public SignaturePadAutomationPeer(SignaturePad owner) : base(owner) { }

    protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.Custom;

    protected override string GetClassNameCore() => nameof(SignaturePad);

    protected override string GetNameCore()
    {
        string name = base.GetNameCore();
        return string.IsNullOrWhiteSpace(name) ? "Signature pad" : name;
    }
}

/// <summary>Exposes the docking manager to assistive technology.</summary>
public sealed class DockingManagerAutomationPeer : FrameworkElementAutomationPeer
{
    public DockingManagerAutomationPeer(DockingManager owner) : base(owner) { }

    protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.Custom;

    protected override string GetClassNameCore() => nameof(DockingManager);

    protected override string GetNameCore()
    {
        string name = base.GetNameCore();
        return string.IsNullOrWhiteSpace(name) ? "Docking manager" : name;
    }
}

