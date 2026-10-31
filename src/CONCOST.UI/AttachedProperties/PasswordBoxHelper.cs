using System.Windows;
using System.Windows.Controls;

namespace CONCOST.UI.AttachedProperties;

/// <summary>
/// Mirrors a <see cref="PasswordBox"/>'s password into a bindable attached property.
/// </summary>
/// <remarks>
/// <see cref="PasswordBox.Password"/> is deliberately not a dependency property, so it cannot be
/// bound. The helper listens to <see cref="PasswordBox.PasswordChangedEvent"/> for every password
/// box through a class handler, which means <c>Password="{Binding …}"</c> works on its own — no
/// extra attribute needed. Set <see cref="AttachProperty"/> to <c>false</c> to opt a single box out.
/// </remarks>
public static class PasswordBoxHelper
{
    static PasswordBoxHelper()
    {
        EventManager.RegisterClassHandler(
            typeof(PasswordBox),
            PasswordBox.PasswordChangedEvent,
            new RoutedEventHandler(OnAnyPasswordBoxChanged),
            true);
    }

    public static readonly DependencyProperty PasswordProperty =
        DependencyProperty.RegisterAttached("Password", typeof(string), typeof(PasswordBoxHelper),
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnPasswordPropertyChanged));

    /// <summary>
    /// Kept for compatibility and as an opt-out: the sync is on by default, set this to
    /// <c>false</c> on a box whose password must never leave the control.
    /// </summary>
    public static readonly DependencyProperty AttachProperty =
        DependencyProperty.RegisterAttached("Attach", typeof(bool), typeof(PasswordBoxHelper),
            new PropertyMetadata(true));

    private static readonly DependencyProperty IsUpdatingProperty =
        DependencyProperty.RegisterAttached("IsUpdating", typeof(bool), typeof(PasswordBoxHelper));

    public static readonly DependencyProperty PasswordLengthProperty =
        DependencyProperty.RegisterAttached("PasswordLength", typeof(int), typeof(PasswordBoxHelper),
            new PropertyMetadata(0));

    public static string GetPassword(DependencyObject dp) => (string)dp.GetValue(PasswordProperty);
    public static void SetPassword(DependencyObject dp, string value) => dp.SetValue(PasswordProperty, value);

    public static bool GetAttach(DependencyObject dp) => (bool)dp.GetValue(AttachProperty);
    public static void SetAttach(DependencyObject dp, bool value) => dp.SetValue(AttachProperty, value);

    public static int GetPasswordLength(DependencyObject dp) => (int)dp.GetValue(PasswordLengthProperty);
    public static void SetPasswordLength(DependencyObject dp, int value) => dp.SetValue(PasswordLengthProperty, value);

    private static bool GetIsUpdating(DependencyObject dp) => (bool)dp.GetValue(IsUpdatingProperty);
    private static void SetIsUpdating(DependencyObject dp, bool value) => dp.SetValue(IsUpdatingProperty, value);

    private static void OnPasswordPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is not PasswordBox box || GetIsUpdating(box)) return;

        box.Password = (string)e.NewValue ?? string.Empty;
        SetPasswordLength(box, box.Password.Length);
    }

    /// <summary>Runs for every password box: typing in the box writes the attached property.</summary>
    private static void OnAnyPasswordBoxChanged(object sender, RoutedEventArgs e)
    {
        if (sender is not PasswordBox box || !GetAttach(box)) return;

        SetIsUpdating(box, true);
        SetPassword(box, box.Password);
        SetPasswordLength(box, box.Password.Length);
        SetIsUpdating(box, false);
    }
}
