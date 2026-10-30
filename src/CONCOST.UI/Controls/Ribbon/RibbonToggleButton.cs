using System.Windows;

namespace CONCOST.UI.Controls.Ribbon;

/// <summary>
/// A ribbon button that keeps a checked state. Use <see cref="GroupName"/> to make a set of
/// buttons behave like radio buttons (checking one clears the rest).
/// </summary>
public class RibbonToggleButton : RibbonButton
{
    public static readonly DependencyProperty IsCheckedProperty =
        DependencyProperty.Register(nameof(IsChecked), typeof(bool), typeof(RibbonToggleButton),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public static readonly DependencyProperty GroupNameProperty =
        DependencyProperty.Register(nameof(GroupName), typeof(string), typeof(RibbonToggleButton),
            new FrameworkPropertyMetadata(string.Empty));

    static RibbonToggleButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(RibbonToggleButton),
            new FrameworkPropertyMetadata(typeof(RibbonToggleButton)));
    }

    /// <summary>Whether the button is checked.</summary>
    public bool IsChecked
    {
        get => (bool)GetValue(IsCheckedProperty);
        set => SetValue(IsCheckedProperty, value);
    }

    /// <summary>Buttons sharing a non-empty group name behave as radio buttons.</summary>
    public string GroupName
    {
        get => (string)GetValue(GroupNameProperty);
        set => SetValue(GroupNameProperty, value);
    }

    protected override System.Windows.Automation.Peers.AutomationPeer OnCreateAutomationPeer() =>
        new RibbonToggleButtonAutomationPeer(this);

    protected override void OnClick()
    {
        base.OnClick();
        IsChecked = !IsChecked;
    }

    protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.Property != IsCheckedProperty || !IsChecked || string.IsNullOrEmpty(GroupName)) return;

        // Radio behaviour: clear the siblings that share the group name.
        if (Parent is not RibbonGroup group) return;

        foreach (object? child in group.Items)
        {
            if (ReferenceEquals(child, this)) continue;
            if (child is RibbonToggleButton other &&
                string.Equals(other.GroupName, GroupName, System.StringComparison.Ordinal))
            {
                other.IsChecked = false;
            }
        }
    }
}
