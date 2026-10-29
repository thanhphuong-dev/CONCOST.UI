using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace CONCOST.UI.Controls.Windows;

public class CONCOSTWindow : Window
{
    private FrameworkElement? _titleBar;
    private Button? _minimizeButton;
    private Button? _maximizeButton;
    private Button? _closeButton;
    static CONCOSTWindow()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(CONCOSTWindow), new FrameworkPropertyMetadata(typeof(CONCOSTWindow)));
    }

    public static readonly DependencyProperty TitleBarBackgroundProperty =
        DependencyProperty.Register(nameof(TitleBarBackground), typeof(Brush), typeof(CONCOSTWindow),
            new PropertyMetadata(null));

    public Brush? TitleBarBackground
    {
        get => (Brush?)GetValue(TitleBarBackgroundProperty);
        set => SetValue(TitleBarBackgroundProperty, value);
    }

    public CONCOSTWindow()
    {
        WindowStyle = WindowStyle.None;
        AllowsTransparency = false;
    }

    public override void OnApplyTemplate()
    {
        if (_titleBar != null) _titleBar.MouseLeftButtonDown -= OnTitleBarMouseLeftButtonDown;
        if (_minimizeButton != null) _minimizeButton.Click -= OnMinimizeClick;
        if (_maximizeButton != null) _maximizeButton.Click -= OnMaximizeClick;
        if (_closeButton != null) _closeButton.Click -= OnCloseClick;
        base.OnApplyTemplate();

        _titleBar = GetTemplateChild("PART_TitleBar") as FrameworkElement;
        _minimizeButton = GetTemplateChild("PART_MinimizeButton") as Button;
        _maximizeButton = GetTemplateChild("PART_MaximizeButton") as Button;
        _closeButton = GetTemplateChild("PART_CloseButton") as Button;
        if (_titleBar != null) _titleBar.MouseLeftButtonDown += OnTitleBarMouseLeftButtonDown;
        if (_minimizeButton != null) _minimizeButton.Click += OnMinimizeClick;
        if (_maximizeButton != null) _maximizeButton.Click += OnMaximizeClick;
        if (_closeButton != null) _closeButton.Click += OnCloseClick;
    }

    private void OnTitleBarMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.OriginalSource is DependencyObject source && FindVisualParent<ButtonBase>(source) is not null) return;
        if (e.ClickCount == 2)
            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        else if (e.ButtonState == MouseButtonState.Pressed)
        {
            try { DragMove(); }
            catch (InvalidOperationException) { }
        }
    }

    private static T? FindVisualParent<T>(DependencyObject child) where T : DependencyObject
    {
        for (var parent = VisualTreeHelper.GetParent(child); parent != null; parent = VisualTreeHelper.GetParent(parent))
            if (parent is T target) return target;
        return null;
    }

    private void OnMinimizeClick(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
    private void OnMaximizeClick(object sender, RoutedEventArgs e) => WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
    private void OnCloseClick(object sender, RoutedEventArgs e) => Close();
}
