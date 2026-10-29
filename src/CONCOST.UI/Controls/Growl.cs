using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace CONCOST.UI.Controls;

public enum GrowlType
{
    Info,
    Success,
    Warning,
    Error,
    Fatal,
    Ask
}

[TemplatePart(Name = "PART_CloseButton", Type = typeof(Button))]
[TemplatePart(Name = "PART_ConfirmBtn", Type = typeof(Button))]
[TemplatePart(Name = "PART_CancelBtn", Type = typeof(Button))]
public class GrowlItem : ContentControl
{
    private Button? _closeBtn;
    private Button? _confirmBtn;
    private Button? _cancelBtn;

    static GrowlItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(GrowlItem), new FrameworkPropertyMetadata(typeof(GrowlItem)));
    }

    public static readonly DependencyProperty MessageProperty =
        DependencyProperty.Register(nameof(Message), typeof(string), typeof(GrowlItem), new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty GrowlTypeProperty =
        DependencyProperty.Register(nameof(GrowlType), typeof(GrowlType), typeof(GrowlItem), new PropertyMetadata(GrowlType.Info));

    public static readonly DependencyProperty TimeStringProperty =
        DependencyProperty.Register(nameof(TimeString), typeof(string), typeof(GrowlItem), new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty ShowCloseButtonProperty =
        DependencyProperty.Register(nameof(ShowCloseButton), typeof(bool), typeof(GrowlItem), new PropertyMetadata(true));

    public string Message
    {
        get => (string)GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    public GrowlType GrowlType
    {
        get => (GrowlType)GetValue(GrowlTypeProperty);
        set => SetValue(GrowlTypeProperty, value);
    }

    public string TimeString
    {
        get => (string)GetValue(TimeStringProperty);
        set => SetValue(TimeStringProperty, value);
    }

    public bool ShowCloseButton
    {
        get => (bool)GetValue(ShowCloseButtonProperty);
        set => SetValue(ShowCloseButtonProperty, value);
    }

    public Action? OnConfirm { get; set; }
    public Action? OnCancel { get; set; }

    public override void OnApplyTemplate()
    {
        if (_closeBtn != null) _closeBtn.Click -= OnCloseButtonClick;
        if (_confirmBtn != null) _confirmBtn.Click -= OnConfirmButtonClick;
        if (_cancelBtn != null) _cancelBtn.Click -= OnCancelButtonClick;
        base.OnApplyTemplate();
        _closeBtn = GetTemplateChild("PART_CloseButton") as Button;
        _confirmBtn = GetTemplateChild("PART_ConfirmBtn") as Button;
        _cancelBtn = GetTemplateChild("PART_CancelBtn") as Button;

        if (_closeBtn != null)
        {
            _closeBtn.Click += OnCloseButtonClick;
        }

        if (_confirmBtn != null)
        {
            _confirmBtn.Click += OnConfirmButtonClick;
        }

        if (_cancelBtn != null)
        {
            _cancelBtn.Click += OnCancelButtonClick;
        }
    }

    private void OnCloseButtonClick(object sender, RoutedEventArgs e) => Growl.Close(this);
    private void OnConfirmButtonClick(object sender, RoutedEventArgs e) { OnConfirm?.Invoke(); Growl.Close(this); }
    private void OnCancelButtonClick(object sender, RoutedEventArgs e) { OnCancel?.Invoke(); Growl.Close(this); }
}

public class GrowlWindow : Window
{
    public StackPanel GrowlPanel { get; } = new StackPanel 
    { 
        VerticalAlignment = VerticalAlignment.Bottom, 
        HorizontalAlignment = HorizontalAlignment.Right,
        Margin = new Thickness(0, 0, 16, 20) 
    };

    public GrowlWindow()
    {
        WindowStyle = WindowStyle.None;
        AllowsTransparency = true;
        Background = Brushes.Transparent;
        ShowInTaskbar = false;
        Topmost = true;
        Focusable = false;
        ShowActivated = false;
        
        var workArea = SystemParameters.WorkArea;
        Width = 380;
        Height = workArea.Height;
        Left = workArea.Right - Width;
        Top = workArea.Top;

        Content = new ScrollViewer
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Hidden,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            Content = GrowlPanel,
            VerticalAlignment = VerticalAlignment.Bottom
        };
    }
}

public class Growl : Control
{
    private static StackPanel? _activeWindowPanel;
    private static GrowlWindow? _desktopWindow;

    public static void RegisterPanel(StackPanel panel)
    {
        _activeWindowPanel = panel;
    }

    // ==================== 1. WINDOW SCOPE GROWL (Top-Right of Current Window) ====================
    public static void Info(string message, int waitSeconds = 4) => ShowInternal(GetEffectiveWindowPanel(), message, GrowlType.Info, waitSeconds);
    public static void Success(string message, int waitSeconds = 4) => ShowInternal(GetEffectiveWindowPanel(), message, GrowlType.Success, waitSeconds);
    public static void Warning(string message, int waitSeconds = 4) => ShowInternal(GetEffectiveWindowPanel(), message, GrowlType.Warning, waitSeconds);
    public static void Error(string message, int waitSeconds = 5) => ShowInternal(GetEffectiveWindowPanel(), message, GrowlType.Error, waitSeconds);
    public static void Fatal(string message, int waitSeconds = 6) => ShowInternal(GetEffectiveWindowPanel(), message, GrowlType.Fatal, waitSeconds);
    public static void Ask(string message, Action onConfirm, Action? onCancel = null) => ShowInternal(GetEffectiveWindowPanel(), message, GrowlType.Ask, 0, onConfirm, onCancel);
    public static void Clear() => GetEffectiveWindowPanel()?.Children.Clear();

    // ==================== 2. DESKTOP SCOPE GROWL (Bottom-Right of Windows Desktop) ====================
    public static void InfoGlobal(string message, int waitSeconds = 4) => ShowGlobal(message, GrowlType.Info, waitSeconds);
    public static void SuccessGlobal(string message, int waitSeconds = 4) => ShowGlobal(message, GrowlType.Success, waitSeconds);
    public static void WarningGlobal(string message, int waitSeconds = 4) => ShowGlobal(message, GrowlType.Warning, waitSeconds);
    public static void ErrorGlobal(string message, int waitSeconds = 5) => ShowGlobal(message, GrowlType.Error, waitSeconds);
    public static void FatalGlobal(string message, int waitSeconds = 6) => ShowGlobal(message, GrowlType.Fatal, waitSeconds);
    public static void AskGlobal(string message, Action onConfirm, Action? onCancel = null) => ShowGlobal(message, GrowlType.Ask, 0, onConfirm, onCancel);
    public static void ClearGlobal()
    {
        try
        {
            if (_desktopWindow != null)
            {
                _desktopWindow.GrowlPanel.Children.Clear();
                _desktopWindow.Close();
                _desktopWindow = null;
            }
        }
        catch { }
    }

    private static StackPanel? GetEffectiveWindowPanel()
    {
        if (_activeWindowPanel != null) return _activeWindowPanel;

        if (_desktopWindow == null)
        {
            try
            {
                _desktopWindow = new GrowlWindow();
                _desktopWindow.Show();
            }
            catch { }
        }
        return _desktopWindow?.GrowlPanel;
    }

    private static void ShowGlobal(string message, GrowlType type, int waitSeconds, Action? onConfirm = null, Action? onCancel = null)
    {
        try
        {
            if (_desktopWindow == null)
            {
                _desktopWindow = new GrowlWindow();
                _desktopWindow.Show();
            }

            ShowInternal(_desktopWindow.GrowlPanel, message, type, waitSeconds, onConfirm, onCancel);
        }
        catch { }
    }

    private static void ShowInternal(StackPanel? targetPanel, string message, GrowlType type, int waitSeconds, Action? onConfirm = null, Action? onCancel = null)
    {
        if (targetPanel == null) return;

        try
        {
            var item = new GrowlItem
            {
                Message = message,
                GrowlType = type,
                TimeString = DateTime.Now.ToString("HH:mm:ss"),
                OnConfirm = onConfirm,
                OnCancel = onCancel
            };

            // Insert at top or bottom depending on flow
            targetPanel.Children.Insert(0, item);

            // Animate slide-in from right + fade-in (Smooth motion with CubicEase)
            var tt = new TranslateTransform(320, 0);
            item.RenderTransform = tt;
            item.Opacity = 0;

            var slideAnim = new DoubleAnimation(320, 0, TimeSpan.FromMilliseconds(300))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            var fadeAnim = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(250))
            {
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            tt.BeginAnimation(TranslateTransform.XProperty, slideAnim);
            item.BeginAnimation(UIElement.OpacityProperty, fadeAnim);

            if (waitSeconds > 0)
            {
                var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(waitSeconds) };
                timer.Tick += (s, e) =>
                {
                    timer.Stop();
                    CloseFromPanel(targetPanel, item);
                };
                timer.Start();
            }
        }
        catch { }
    }

    public static void Close(GrowlItem item)
    {
        if (item.Parent is StackPanel sp)
        {
            CloseFromPanel(sp, item);
        }
        else if (item.Parent is FrameworkElement fe && fe.Parent is StackPanel spParent)
        {
            CloseFromPanel(spParent, item);
        }
    }

    private static void CloseFromPanel(StackPanel panel, GrowlItem item)
    {
        try
        {
            var tt = item.RenderTransform as TranslateTransform ?? new TranslateTransform(0, 0);
            item.RenderTransform = tt;

            var slideAnim = new DoubleAnimation(0, 140, TimeSpan.FromMilliseconds(220))
            {
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
            };
            var fadeAnim = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(200))
            {
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
            };

            fadeAnim.Completed += (s, e) =>
            {
                try
                {
                    // Smooth height collapse so subsequent items glide up gracefully
                    var collapseAnim = new DoubleAnimation(item.ActualHeight, 0, TimeSpan.FromMilliseconds(160))
                    {
                        EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
                    };
                    collapseAnim.Completed += (s2, e2) =>
                    {
                        try
                        {
                            panel.Children.Remove(item);
                            if (panel.Parent is ScrollViewer sv && sv.Parent is GrowlWindow gw && panel.Children.Count == 0)
                            {
                                gw.Close();
                                _desktopWindow = null;
                            }
                        }
                        catch { }
                    };
                    item.BeginAnimation(FrameworkElement.HeightProperty, collapseAnim);
                }
                catch
                {
                    try { panel.Children.Remove(item); } catch { }
                }
            };

            tt.BeginAnimation(TranslateTransform.XProperty, slideAnim);
            item.BeginAnimation(UIElement.OpacityProperty, fadeAnim);
        }
        catch
        {
            try { panel.Children.Remove(item); } catch { }
        }
    }
}
