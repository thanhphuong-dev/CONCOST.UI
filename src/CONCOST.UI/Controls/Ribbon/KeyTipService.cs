using System.Windows;

namespace CONCOST.UI.Controls.Ribbon;

/// <summary>
/// Attached properties that give the ribbon keyboard access: press Alt, a small letter badge
/// appears over every tab and command, type the letters and the command runs.
/// <para>
/// <code>
/// &lt;ribbon:RibbonButton Header="Bold" KeyTip="B" /&gt;     &lt;!-- explicit badge --&gt;
/// &lt;ribbon:RibbonButton Header="Italic" /&gt;              &lt;!-- badge derived from "I" --&gt;
/// </code>
/// </para>
/// <para>
/// Leave <see cref="KeyTipProperty"/> empty and the ribbon picks a unique letter from the header,
/// so a ribbon written without badges is still fully reachable from the keyboard.
/// </para>
/// </summary>
public static class KeyTipService
{
    /// <summary>Letter sequence that activates the element. Empty means “derive one from the header”.</summary>
    public static readonly DependencyProperty KeyTipProperty =
        DependencyProperty.RegisterAttached("KeyTip", typeof(string), typeof(KeyTipService),
            new FrameworkPropertyMetadata(string.Empty));

    /// <summary>Set on a <see cref="Ribbon"/> to opt in to the Alt gesture. On by default.</summary>
    public static readonly DependencyProperty IsEnabledProperty =
        DependencyProperty.RegisterAttached("IsEnabled", typeof(bool), typeof(KeyTipService),
            new FrameworkPropertyMetadata(true));

    private static readonly DependencyPropertyKey IsShowingPropertyKey =
        DependencyProperty.RegisterAttachedReadOnly("IsShowing", typeof(bool), typeof(KeyTipService),
            new FrameworkPropertyMetadata(false));

    /// <summary>Read-only mirror of the session state, usable from a trigger.</summary>
    public static readonly DependencyProperty IsShowingProperty = IsShowingPropertyKey.DependencyProperty;

    /// <summary>Reads the badge text of <paramref name="element"/>.</summary>
    public static string GetKeyTip(DependencyObject element) => (string)element.GetValue(KeyTipProperty);

    /// <summary>Sets the badge text of <paramref name="element"/>.</summary>
    public static void SetKeyTip(DependencyObject element, string value) => element.SetValue(KeyTipProperty, value);

    /// <summary>True when the ribbon reacts to Alt.</summary>
    public static bool GetIsEnabled(DependencyObject element) => (bool)element.GetValue(IsEnabledProperty);

    /// <summary>Turns the Alt gesture on or off for a ribbon.</summary>
    public static void SetIsEnabled(DependencyObject element, bool value) => element.SetValue(IsEnabledProperty, value);

    /// <summary>True while badges are on screen.</summary>
    public static bool GetIsShowing(DependencyObject element) => (bool)element.GetValue(IsShowingProperty);

    internal static void SetIsShowing(DependencyObject element, bool value) =>
        element.SetValue(IsShowingPropertyKey, value);

    /// <summary>Shows the badges as if Alt had been pressed.</summary>
    public static void Show(Ribbon ribbon) => ribbon.ShowKeyTips();

    /// <summary>Dismisses the badges.</summary>
    public static void Hide(Ribbon ribbon) => ribbon.HideKeyTips();
}
