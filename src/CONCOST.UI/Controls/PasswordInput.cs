using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CONCOST.UI.Icons;

namespace CONCOST.UI.Controls;

public class PasswordInput : Control
{
    private PasswordBox? _passwordBox;
    private TextBox? _textBox;
    private bool _isSyncing;

    static PasswordInput()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(PasswordInput), new FrameworkPropertyMetadata(typeof(PasswordInput)));
    }

    public static readonly DependencyProperty PasswordProperty =
        DependencyProperty.Register(nameof(Password), typeof(string), typeof(PasswordInput),
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnPasswordChanged));

    public static readonly DependencyProperty PlaceholderProperty =
        DependencyProperty.Register(nameof(Placeholder), typeof(string), typeof(PasswordInput),
            new FrameworkPropertyMetadata(string.Empty));

    public static readonly DependencyProperty ShowPasswordProperty =
        DependencyProperty.Register(nameof(ShowPassword), typeof(bool), typeof(PasswordInput),
            new FrameworkPropertyMetadata(false, OnShowPasswordChanged));

    public static readonly DependencyProperty IconKindProperty =
        DependencyProperty.Register(nameof(IconKind), typeof(IconKind), typeof(PasswordInput),
            new FrameworkPropertyMetadata(IconKind.Lock));

    public string Password
    {
        get => (string)GetValue(PasswordProperty);
        set => SetValue(PasswordProperty, value);
    }

    public string Placeholder
    {
        get => (string)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    public bool ShowPassword
    {
        get => (bool)GetValue(ShowPasswordProperty);
        set => SetValue(ShowPasswordProperty, value);
    }

    public IconKind IconKind
    {
        get => (IconKind)GetValue(IconKindProperty);
        set => SetValue(IconKindProperty, value);
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        if (_passwordBox != null)
            _passwordBox.PasswordChanged -= OnPasswordBoxChanged;
        if (_textBox != null)
            _textBox.TextChanged -= OnTextBoxChanged;

        _passwordBox = GetTemplateChild("PART_PasswordBox") as PasswordBox;
        _textBox = GetTemplateChild("PART_TextBox") as TextBox;

        if (_passwordBox != null)
        {
            _passwordBox.Password = Password;
            _passwordBox.PasswordChanged += OnPasswordBoxChanged;
        }

        if (_textBox != null)
        {
            _textBox.Text = Password;
            _textBox.TextChanged += OnTextBoxChanged;
        }

        // No focus move here: applying a template must not pull the caret out of whatever the
        // host focused (a login form is often opened with the user name field active).
        UpdateVisibility(moveFocus: false);
    }

    private void OnPasswordBoxChanged(object sender, RoutedEventArgs e)
    {
        if (_isSyncing || _passwordBox == null) return;
        _isSyncing = true;
        Password = _passwordBox.Password;
        if (_textBox != null) _textBox.Text = Password;
        _isSyncing = false;
    }

    private void OnTextBoxChanged(object sender, TextChangedEventArgs e)
    {
        if (_isSyncing || _textBox == null) return;
        _isSyncing = true;
        Password = _textBox.Text;
        if (_passwordBox != null) _passwordBox.Password = Password;
        _isSyncing = false;
    }

    private static void OnPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is PasswordInput ctrl && !ctrl._isSyncing)
        {
            ctrl._isSyncing = true;
            var newVal = (string)e.NewValue ?? string.Empty;
            if (ctrl._passwordBox != null) ctrl._passwordBox.Password = newVal;
            if (ctrl._textBox != null) ctrl._textBox.Text = newVal;
            ctrl._isSyncing = false;
        }
    }

    private static void OnShowPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is PasswordInput ctrl)
        {
            // Toggling reveal is a user action, so the caret should follow it.
            ctrl.UpdateVisibility(moveFocus: true);
        }
    }

    private void UpdateVisibility(bool moveFocus)
    {
        if (_passwordBox == null || _textBox == null) return;

        if (ShowPassword)
        {
            _passwordBox.Visibility = Visibility.Collapsed;
            _textBox.Visibility = Visibility.Visible;

            if (!moveFocus) return;

            _textBox.Focus();
            _textBox.CaretIndex = _textBox.Text.Length;
        }
        else
        {
            _textBox.Visibility = Visibility.Collapsed;
            _passwordBox.Visibility = Visibility.Visible;

            if (moveFocus) _passwordBox.Focus();
        }
    }
}
