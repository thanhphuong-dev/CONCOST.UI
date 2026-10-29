using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using CONCOST.UI.Icons;

namespace CONCOST.UI.Controls;

public enum DialogMessageType
{
    None,
    Information,
    Success,
    Warning,
    Error,
    Question
}

[TemplatePart(Name = "PART_Mask", Type = typeof(Border))]
[TemplatePart(Name = "PART_DialogCard", Type = typeof(Border))]
[TemplatePart(Name = "PART_CloseButton", Type = typeof(Button))]
[TemplatePart(Name = "PART_HeaderBorder", Type = typeof(Border))]
public class Dialog : ContentControl
{
    private Border? _mask;
    private Border? _dialogCard;
    private Button? _closeBtn;
    private Border? _headerBorder;

    private static readonly Dictionary<string, Dialog> _tokenDialogs = new();
    private static Panel? _globalHostPanel;

    static Dialog()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(Dialog), new FrameworkPropertyMetadata(typeof(Dialog)));
    }

    #region Dependency Properties

    public static readonly DependencyProperty IsOpenProperty =
        DependencyProperty.Register(nameof(IsOpen), typeof(bool), typeof(Dialog),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnIsOpenChanged));

    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(nameof(Title), typeof(string), typeof(Dialog),
            new PropertyMetadata("Dialog"));

    public static readonly DependencyProperty DialogWidthProperty =
        DependencyProperty.Register(nameof(DialogWidth), typeof(double), typeof(Dialog),
            new PropertyMetadata(440.0));

    public static readonly DependencyProperty DialogHeightProperty =
        DependencyProperty.Register(nameof(DialogHeight), typeof(double), typeof(Dialog),
            new PropertyMetadata(double.NaN));

    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(Dialog),
            new PropertyMetadata(new CornerRadius(12)));

    public static readonly DependencyProperty MaskCanCloseProperty =
        DependencyProperty.Register(nameof(MaskCanClose), typeof(bool), typeof(Dialog),
            new PropertyMetadata(true));

    public static readonly DependencyProperty ShowCloseButtonProperty =
        DependencyProperty.Register(nameof(ShowCloseButton), typeof(bool), typeof(Dialog),
            new PropertyMetadata(true));

    public static readonly DependencyProperty MessageTypeProperty =
        DependencyProperty.Register(nameof(MessageType), typeof(DialogMessageType), typeof(Dialog),
            new PropertyMetadata(DialogMessageType.None));

    public static readonly DependencyProperty TokenProperty =
        DependencyProperty.RegisterAttached("Token", typeof(string), typeof(Dialog),
            new PropertyMetadata(null, OnTokenChanged));

    public static void SetToken(DependencyObject element, string value) => element.SetValue(TokenProperty, value);
    public static string GetToken(DependencyObject element) => (string)element.GetValue(TokenProperty);

    private static void OnTokenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Dialog dialog && e.NewValue is string token)
        {
            _tokenDialogs[token] = dialog;
        }
    }

    public bool IsOpen
    {
        get => (bool)GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public double DialogWidth
    {
        get => (double)GetValue(DialogWidthProperty);
        set => SetValue(DialogWidthProperty, value);
    }

    public double DialogHeight
    {
        get => (double)GetValue(DialogHeightProperty);
        set => SetValue(DialogHeightProperty, value);
    }

    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    public bool MaskCanClose
    {
        get => (bool)GetValue(MaskCanCloseProperty);
        set => SetValue(MaskCanCloseProperty, value);
    }

    public bool ShowCloseButton
    {
        get => (bool)GetValue(ShowCloseButtonProperty);
        set => SetValue(ShowCloseButtonProperty, value);
    }

    public DialogMessageType MessageType
    {
        get => (DialogMessageType)GetValue(MessageTypeProperty);
        set => SetValue(MessageTypeProperty, value);
    }

    public Action? OnClosed { get; set; }

    #endregion

    public static void RegisterGlobalHost(Panel panel)
    {
        _globalHostPanel = panel;
    }

    #region HandyControl & WPFDevelopers 1:1 Static Show APIs

    public static Dialog Show(object content, string title = "Dialog", string token = "")
    {
        if (!string.IsNullOrEmpty(token) && _tokenDialogs.TryGetValue(token, out var existingDialog))
        {
            existingDialog.Title = title;
            existingDialog.Content = content;
            existingDialog.MessageType = DialogMessageType.None;
            existingDialog.IsOpen = true;
            return existingDialog;
        }

        var dlg = new Dialog
        {
            Title = title,
            Content = content,
            MessageType = DialogMessageType.None,
            IsOpen = true
        };

        if (_globalHostPanel != null)
        {
            if (!_globalHostPanel.Children.Contains(dlg))
                _globalHostPanel.Children.Add(dlg);
        }
        else if (Application.Current?.MainWindow?.Content is Panel winPanel)
        {
            if (!winPanel.Children.Contains(dlg))
                winPanel.Children.Add(dlg);
        }
        else if (Application.Current?.MainWindow?.Content is Decorator decorator && decorator.Child is Panel decPanel)
        {
            if (!decPanel.Children.Contains(dlg))
                decPanel.Children.Add(dlg);
        }

        return dlg;
    }

    public static Task<bool> ShowConfirmAsync(string message, string title = "Confirm", string token = "")
    {
        var tcs = new TaskCompletionSource<bool>();

        var sp = new StackPanel { Margin = new Thickness(0) };
        var msgBlock = new TextBlock
        {
            Text = message,
            FontSize = 13.5,
            LineHeight = 20,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 0, 0, 20)
        };
        msgBlock.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextPrimary");

        var btnPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right
        };

        var cancelBtn = new Button
        {
            Content = "Cancel",
            Width = 90,
            Height = 32,
            Margin = new Thickness(0, 0, 10, 0)
        };
        cancelBtn.SetResourceReference(Button.StyleProperty, "CONCOST.Button.Secondary");

        var okBtn = new Button
        {
            Content = "Confirm",
            Width = 90,
            Height = 32
        };
        okBtn.SetResourceReference(Button.StyleProperty, "CONCOST.Button.Primary");

        btnPanel.Children.Add(cancelBtn);
        btnPanel.Children.Add(okBtn);

        sp.Children.Add(msgBlock);
        sp.Children.Add(btnPanel);

        var dlg = Show(sp, title, token);
        dlg.MessageType = DialogMessageType.Question;

        cancelBtn.Click += (s, e) =>
        {
            dlg.IsOpen = false;
            tcs.TrySetResult(false);
        };

        okBtn.Click += (s, e) =>
        {
            dlg.IsOpen = false;
            tcs.TrySetResult(true);
        };

        dlg.OnClosed = () => tcs.TrySetResult(false);

        return tcs.Task;
    }

    public static Task<string?> ShowInputAsync(string message, string defaultText = "", string title = "Input", string token = "")
    {
        var tcs = new TaskCompletionSource<string?>();

        var sp = new StackPanel { Margin = new Thickness(0) };
        var msgBlock = new TextBlock
        {
            Text = message,
            FontSize = 13,
            Margin = new Thickness(0, 0, 0, 10)
        };
        msgBlock.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextPrimary");

        var input = new TextBox
        {
            Text = defaultText,
            Height = 34,
            Margin = new Thickness(0, 0, 0, 18)
        };
        input.SetResourceReference(TextBox.StyleProperty, "CONCOST.TextBox.Base");

        var btnPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right
        };

        var cancelBtn = new Button
        {
            Content = "Cancel",
            Width = 90,
            Height = 32,
            Margin = new Thickness(0, 0, 10, 0)
        };
        cancelBtn.SetResourceReference(Button.StyleProperty, "CONCOST.Button.Secondary");

        var okBtn = new Button
        {
            Content = "OK",
            Width = 90,
            Height = 32
        };
        okBtn.SetResourceReference(Button.StyleProperty, "CONCOST.Button.Primary");

        btnPanel.Children.Add(cancelBtn);
        btnPanel.Children.Add(okBtn);

        sp.Children.Add(msgBlock);
        sp.Children.Add(input);
        sp.Children.Add(btnPanel);

        var dlg = Show(sp, title, token);

        cancelBtn.Click += (s, e) =>
        {
            dlg.IsOpen = false;
            tcs.TrySetResult(null);
        };

        okBtn.Click += (s, e) =>
        {
            dlg.IsOpen = false;
            tcs.TrySetResult(input.Text);
        };

        return tcs.Task;
    }

    public static void Close(string token)
    {
        if (_tokenDialogs.TryGetValue(token, out var dlg))
        {
            dlg.IsOpen = false;
        }
    }

    #endregion

    public override void OnApplyTemplate()
    {
        if (_closeBtn != null) _closeBtn.Click -= OnCloseClick;
        if (_mask != null) _mask.MouseLeftButtonDown -= OnMaskMouseLeftButtonDown;
        base.OnApplyTemplate();
        _mask = GetTemplateChild("PART_Mask") as Border;
        _dialogCard = GetTemplateChild("PART_DialogCard") as Border;
        _closeBtn = GetTemplateChild("PART_CloseButton") as Button;
        _headerBorder = GetTemplateChild("PART_HeaderBorder") as Border;

        if (_closeBtn != null)
        {
            _closeBtn.Click += OnCloseClick;
        }

        if (_mask != null)
        {
            _mask.MouseLeftButtonDown += OnMaskMouseLeftButtonDown;
        }

        UpdateState(IsOpen);
    }

    private void OnCloseClick(object sender, RoutedEventArgs e) => IsOpen = false;
    private void OnMaskMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (MaskCanClose) IsOpen = false;
    }

    private static void OnIsOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Dialog dlg)
        {
            dlg.UpdateState((bool)e.NewValue);
        }
    }

    private void UpdateState(bool open)
    {
        if (_mask == null || _dialogCard == null)
        {
            Visibility = open ? Visibility.Visible : Visibility.Collapsed;
            return;
        }

        if (open)
        {
            Visibility = Visibility.Visible;
            _mask.Visibility = Visibility.Visible;
            _dialogCard.Visibility = Visibility.Visible;

            _mask.Opacity = 1.0;
            var st = new ScaleTransform(0.85, 0.85);
            _dialogCard.RenderTransformOrigin = new Point(0.5, 0.5);
            _dialogCard.RenderTransform = st;

            var popAnim = new DoubleAnimation(0.85, 1.0, TimeSpan.FromMilliseconds(240))
            {
                EasingFunction = new BackEase { EasingMode = EasingMode.EaseOut, Amplitude = 0.35 }
            };
            st.BeginAnimation(ScaleTransform.ScaleXProperty, popAnim);
            st.BeginAnimation(ScaleTransform.ScaleYProperty, popAnim);
            _dialogCard.BeginAnimation(UIElement.OpacityProperty, new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(200)));
        }
        else
        {
            if (_dialogCard.RenderTransform is ScaleTransform st)
            {
                var shrinkAnim = new DoubleAnimation(1.0, 0.85, TimeSpan.FromMilliseconds(180))
                {
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
                };
                shrinkAnim.Completed += (s, e) =>
                {
                    _mask.Visibility = Visibility.Collapsed;
                    _dialogCard.Visibility = Visibility.Collapsed;
                    Visibility = Visibility.Collapsed;
                    OnClosed?.Invoke();
                };
                st.BeginAnimation(ScaleTransform.ScaleXProperty, shrinkAnim);
                st.BeginAnimation(ScaleTransform.ScaleYProperty, shrinkAnim);
                _dialogCard.BeginAnimation(UIElement.OpacityProperty, new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(180)));
            }
            else
            {
                _mask.Visibility = Visibility.Collapsed;
                _dialogCard.Visibility = Visibility.Collapsed;
                Visibility = Visibility.Collapsed;
                OnClosed?.Invoke();
            }
        }
    }
}
