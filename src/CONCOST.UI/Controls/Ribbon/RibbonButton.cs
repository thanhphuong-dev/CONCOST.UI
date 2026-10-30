using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CONCOST.UI.Controls.Ribbon;

/// <summary>
/// A ribbon command button. Invoking it (mouse, Space or Enter) executes
/// <see cref="RibbonItem.Command"/>; the visual state follows <see cref="UIElement.IsEnabled"/>.
/// </summary>
public class RibbonButton : RibbonItem
{
    public static readonly DependencyProperty IsPressedProperty =
        DependencyProperty.Register(nameof(IsPressed), typeof(bool), typeof(RibbonButton),
            new FrameworkPropertyMetadata(false));

    static RibbonButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(RibbonButton),
            new FrameworkPropertyMetadata(typeof(RibbonButton)));
    }

    public RibbonButton()
    {
        Cursor = Cursors.Hand;
    }

    /// <summary>True while the mouse is held down on the button.</summary>
    public bool IsPressed
    {
        get => (bool)GetValue(IsPressedProperty);
        private set => SetValue(IsPressedProperty, value);
    }

    /// <summary>Raised after the command ran (or when there is no command).</summary>
    public event RoutedEventHandler? Click;

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);
        if (!IsEnabled) return;

        IsPressed = true;
        CaptureMouse();
        e.Handled = true;
    }

    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonUp(e);

        bool wasPressed = IsPressed;
        IsPressed = false;
        if (IsMouseCaptured) ReleaseMouseCapture();

        if (!IsEnabled || !wasPressed) return;

        Focus();
        OnClick();
        e.Handled = true;
    }

    protected override void OnMouseLeave(MouseEventArgs e)
    {
        base.OnMouseLeave(e);
        IsPressed = false;
        if (IsMouseCaptured) ReleaseMouseCapture();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (!IsEnabled) return;

        if (e.Key is Key.Space or Key.Enter)
        {
            OnClick();
            e.Handled = true;
        }
    }

    protected override System.Windows.Automation.Peers.AutomationPeer OnCreateAutomationPeer() =>
        new RibbonButtonAutomationPeer(this);

    /// <summary>
    /// Runs the button as if it had been clicked — the entry point used by key tips, the
    /// collapsed-group menu and the accessibility peer.
    /// </summary>
    public override void PerformClick()
    {
        if (!IsEnabled) return;

        Focus();
        OnClick();
    }

    /// <summary>Invokes the command and raises <see cref="Click"/>.</summary>
    protected virtual void OnClick()
    {
        InvokeCommand();
        Click?.Invoke(this, new RoutedEventArgs());
    }
}
