using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;

namespace CONCOST.UI.Controls.Ribbon;

/// <summary>
/// UI Automation for the ribbon shell. It reports itself as a toolbar so a screen reader announces
/// “toolbar” instead of a bare pane, and it exposes the tab strip as the child list.
/// </summary>
public class RibbonAutomationPeer : FrameworkElementAutomationPeer
{
    public RibbonAutomationPeer(Ribbon owner) : base(owner)
    {
    }

    protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.ToolBar;

    protected override string GetClassNameCore() => "Ribbon";

    protected override string GetNameCore()
    {
        string name = base.GetNameCore();
        return string.IsNullOrEmpty(name) ? "Ribbon" : name;
    }

    protected override string GetHelpTextCore() =>
        "Press Alt to show the key tips, then type the letters of a command.";
}

/// <summary>Automation peer for a ribbon command: exposes Invoke, the label and the enabled state.</summary>
public class RibbonButtonAutomationPeer : FrameworkElementAutomationPeer, IInvokeProvider
{
    public RibbonButtonAutomationPeer(RibbonButton owner) : base(owner)
    {
    }

    /// <summary>The button this peer belongs to.</summary>
    protected RibbonButton Button => (RibbonButton)Owner;

    public override object? GetPattern(PatternInterface patternInterface) =>
        patternInterface == PatternInterface.Invoke ? this : base.GetPattern(patternInterface);

    /// <summary>Runs the command — what a screen reader does when it “clicks” the button.</summary>
    public void Invoke()
    {
        if (!IsEnabled()) return;

        Button.PerformClick();
    }

    protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.Button;

    protected override string GetClassNameCore() => "RibbonButton";

    protected override string GetNameCore()
    {
        string name = base.GetNameCore();
        if (!string.IsNullOrEmpty(name)) return name;

        return Button.Header as string ?? string.Empty;
    }

    protected override bool IsEnabledCore() => Button.IsEnabled;
}

/// <summary>Adds the Toggle pattern to <see cref="RibbonToggleButton"/> and <see cref="RibbonCheckBox"/>.</summary>
public class RibbonToggleButtonAutomationPeer : RibbonButtonAutomationPeer, IToggleProvider
{
    public RibbonToggleButtonAutomationPeer(RibbonToggleButton owner) : base(owner)
    {
    }

    /// <summary>The toggle this peer belongs to.</summary>
    protected RibbonToggleButton ToggleButton => (RibbonToggleButton)Owner;

    public override object? GetPattern(PatternInterface patternInterface) =>
        patternInterface == PatternInterface.Toggle ? this : base.GetPattern(patternInterface);

    public ToggleState ToggleState =>
        ToggleButton.IsChecked ? ToggleState.On : ToggleState.Off;

    public void Toggle()
    {
        if (!IsEnabled()) return;

        ToggleButton.PerformClick();
    }

    protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.CheckBox;

    protected override string GetClassNameCore() => "RibbonToggleButton";
}

/// <summary>Reports a <see cref="RibbonRadioButton"/> as a radio button rather than a check box.</summary>
public class RibbonRadioButtonAutomationPeer : RibbonToggleButtonAutomationPeer
{
    public RibbonRadioButtonAutomationPeer(RibbonRadioButton owner) : base(owner)
    {
    }

    protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.RadioButton;

    protected override string GetClassNameCore() => "RibbonRadioButton";
}

/// <summary>Exposes the gallery's expanded panel so it can be opened from a screen reader.</summary>
public class RibbonGalleryAutomationPeer : FrameworkElementAutomationPeer, IExpandCollapseProvider
{
    public RibbonGalleryAutomationPeer(RibbonGallery owner) : base(owner)
    {
    }

    /// <summary>The gallery this peer belongs to.</summary>
    protected RibbonGallery Gallery => (RibbonGallery)Owner;

    public override object? GetPattern(PatternInterface patternInterface) =>
        patternInterface == PatternInterface.ExpandCollapse ? this : base.GetPattern(patternInterface);

    public ExpandCollapseState ExpandCollapseState =>
        Gallery.IsDropDownOpen ? ExpandCollapseState.Expanded : ExpandCollapseState.Collapsed;

    public void Expand() => Gallery.IsDropDownOpen = true;

    public void Collapse() => Gallery.IsDropDownOpen = false;

    protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.List;

    protected override string GetClassNameCore() => "RibbonGallery";

    protected override string GetNameCore()
    {
        string name = base.GetNameCore();
        if (!string.IsNullOrEmpty(name)) return name;

        string header = Gallery.Header as string ?? string.Empty;
        string selected = RibbonGallery.LabelFor(Gallery.SelectedItem);

        return selected.Length == 0 ? header : header + ", " + selected;
    }

    protected override bool IsEnabledCore() => Gallery.IsEnabled;
}

/// <summary>Exposes the ribbon drop-down as a combo box that can be expanded and collapsed.</summary>
public class RibbonComboBoxAutomationPeer : FrameworkElementAutomationPeer, IExpandCollapseProvider, IValueProvider
{
    public RibbonComboBoxAutomationPeer(RibbonComboBox owner) : base(owner)
    {
    }

    /// <summary>The combo box this peer belongs to.</summary>
    protected RibbonComboBox ComboBox => (RibbonComboBox)Owner;

    public override object? GetPattern(PatternInterface patternInterface) => patternInterface switch
    {
        PatternInterface.ExpandCollapse => this,
        PatternInterface.Value => this,
        _ => base.GetPattern(patternInterface)
    };

    public ExpandCollapseState ExpandCollapseState =>
        ComboBox.Editor?.IsDropDownOpen == true ? ExpandCollapseState.Expanded : ExpandCollapseState.Collapsed;

    public void Expand()
    {
        if (ComboBox.Editor is { } editor) editor.IsDropDownOpen = true;
    }

    public void Collapse()
    {
        if (ComboBox.Editor is { } editor) editor.IsDropDownOpen = false;
    }

    public bool IsReadOnly => !ComboBox.IsEditable;

    public string Value => ComboBox.SelectedItem?.ToString() ?? string.Empty;

    public void SetValue(string value) => ComboBox.SelectedItem = value;

    protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.ComboBox;

    protected override string GetClassNameCore() => "RibbonComboBox";

    protected override string GetNameCore()
    {
        string name = base.GetNameCore();
        if (!string.IsNullOrEmpty(name)) return name;

        return ComboBox.Header as string ?? string.Empty;
    }

    protected override bool IsEnabledCore() => ComboBox.IsEnabled;
}

/// <summary>Gives a group a name and a Group control type, so its commands are announced in context.</summary>
public class RibbonGroupAutomationPeer : FrameworkElementAutomationPeer
{
    public RibbonGroupAutomationPeer(RibbonGroup owner) : base(owner)
    {
    }

    protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.Group;

    protected override string GetClassNameCore() => "RibbonGroup";

    protected override string GetNameCore()
    {
        string name = base.GetNameCore();
        if (!string.IsNullOrEmpty(name)) return name;

        return ((RibbonGroup)Owner).Header as string ?? string.Empty;
    }
}
