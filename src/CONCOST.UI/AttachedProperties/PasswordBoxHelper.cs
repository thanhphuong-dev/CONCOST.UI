using System.Windows;
using System.Windows.Controls;

namespace CONCOST.UI.AttachedProperties;

public static class PasswordBoxHelper
{
    public static readonly DependencyProperty PasswordProperty =
        DependencyProperty.RegisterAttached("Password", typeof(string), typeof(PasswordBoxHelper),
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnPasswordPropertyChanged));

    public static readonly DependencyProperty AttachProperty =
        DependencyProperty.RegisterAttached("Attach", typeof(bool), typeof(PasswordBoxHelper),
            new PropertyMetadata(false, Attach));

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
        if (sender is PasswordBox box)
        {
            box.PasswordChanged -= HandlePasswordChanged;
            if (!GetIsUpdating(box))
            {
                box.Password = (string)e.NewValue ?? string.Empty;
                SetPasswordLength(box, box.Password.Length);
            }
            box.PasswordChanged += HandlePasswordChanged;
        }
    }

    private static void Attach(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is PasswordBox box)
        {
            if ((bool)e.OldValue)
            {
                box.PasswordChanged -= HandlePasswordChanged;
            }
            if ((bool)e.NewValue)
            {
                box.PasswordChanged += HandlePasswordChanged;
            }
        }
    }

    private static void HandlePasswordChanged(object sender, RoutedEventArgs e)
    {
        if (sender is PasswordBox box)
        {
            SetIsUpdating(box, true);
            SetPassword(box, box.Password);
            SetPasswordLength(box, box.Password.Length);
            SetIsUpdating(box, false);
        }
    }
}
