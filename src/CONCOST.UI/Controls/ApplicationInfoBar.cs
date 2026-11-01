using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CONCOST.UI.AttachedProperties;
using CONCOST.UI.Icons;
using CONCOST.UI.Services;

namespace CONCOST.UI.Controls;

/// <summary>
/// The bar under the ribbon that reports one thing at a time: a warning, an error, an update, or an
/// offer with a single action.
/// </summary>
/// <remarks>
/// It stays in the layout and collapses rather than being created on demand, so a host can bind its
/// visibility and never has to add and remove rows. The action is one button on purpose: a bar with
/// three choices is a dialog pretending to be a strip.
/// </remarks>
public class ApplicationInfoBar : Border
{
    /// <summary>Identifies the <see cref="Severity"/> property.</summary>
    public static readonly DependencyProperty SeverityProperty = DependencyProperty.Register(
        nameof(Severity), typeof(CadNotificationSeverity), typeof(ApplicationInfoBar),
        new FrameworkPropertyMetadata(CadNotificationSeverity.Info, OnSeverityChanged));

    /// <summary>Identifies the <see cref="Title"/> property.</summary>
    public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
        nameof(Title), typeof(string), typeof(ApplicationInfoBar),
        new FrameworkPropertyMetadata(string.Empty, OnTextChanged));

    /// <summary>Identifies the <see cref="Message"/> property.</summary>
    public static readonly DependencyProperty MessageProperty = DependencyProperty.Register(
        nameof(Message), typeof(string), typeof(ApplicationInfoBar),
        new FrameworkPropertyMetadata(string.Empty, OnTextChanged));

    /// <summary>Identifies the <see cref="ActionText"/> property.</summary>
    public static readonly DependencyProperty ActionTextProperty = DependencyProperty.Register(
        nameof(ActionText), typeof(string), typeof(ApplicationInfoBar),
        new FrameworkPropertyMetadata(string.Empty, OnActionTextChanged));

    /// <summary>Identifies the <see cref="IsClosable"/> property.</summary>
    public static readonly DependencyProperty IsClosableProperty = DependencyProperty.Register(
        nameof(IsClosable), typeof(bool), typeof(ApplicationInfoBar),
        new FrameworkPropertyMetadata(true, OnClosableChanged));

    private readonly Grid _row;
    private readonly Icon _icon;
    private readonly TextBlock _title;
    private readonly TextBlock _message;
    private readonly Button _action;
    private readonly Button _close;

    private bool _tintResolved;

    public ApplicationInfoBar()
    {
        _row = new Grid();

        _row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        _row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        _row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        _row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        _row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        Child = _row;

        BorderThickness = new Thickness(0, 0, 0, 1);

        _icon = new Icon { Width = 14, Height = 14, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(10, 0, 8, 0) };

        Grid.SetColumn(_icon, 0);
        _row.Children.Add(_icon);

        _title = new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center,
            FontSize = 12,
            FontWeight = FontWeights.SemiBold,
            Margin = new Thickness(0, 0, 8, 0)
        };

        _title.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextPrimary");

        Grid.SetColumn(_title, 1);
        _row.Children.Add(_title);

        _message = new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center,
            FontSize = 12,
            TextTrimming = TextTrimming.CharacterEllipsis
        };

        _message.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");

        Grid.SetColumn(_message, 2);
        _row.Children.Add(_message);

        _action = new Button { Height = 24, MinWidth = 76, Margin = new Thickness(8, 4, 4, 4), FontSize = 11, Visibility = Visibility.Collapsed };
        _action.SetResourceReference(StyleProperty, "CONCOST.Button.Secondary");
        _action.Click += (_, _) => ActionInvoked?.Invoke(this, EventArgs.Empty);

        Grid.SetColumn(_action, 3);
        _row.Children.Add(_action);

        _close = new Button { Width = 24, Height = 24, Margin = new Thickness(0, 4, 6, 4), Visibility = Visibility.Collapsed };
        _close.SetResourceReference(StyleProperty, "CONCOST.Button.Icon");
        IconElement.SetKind(_close, IconKind.Close);
        IconElement.SetIconSize(_close, 11);

        _close.Click += (_, _) =>
        {
            Visibility = Visibility.Collapsed;

            Closed?.Invoke(this, EventArgs.Empty);
        };

        Grid.SetColumn(_close, 4);
        _row.Children.Add(_close);

        ApplySeverity(Severity);
    }

    /// <summary>How loud the bar is; picks the icon and the tint.</summary>
    public CadNotificationSeverity Severity
    {
        get => (CadNotificationSeverity)GetValue(SeverityProperty);
        set => SetValue(SeverityProperty, value);
    }

    /// <summary>The bold lead-in.</summary>
    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>The rest of the sentence.</summary>
    public string Message
    {
        get => (string)GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    /// <summary>The one action the bar offers; empty hides the button.</summary>
    public string ActionText
    {
        get => (string)GetValue(ActionTextProperty);
        set => SetValue(ActionTextProperty, value);
    }

    /// <summary>Whether the bar can be dismissed.</summary>
    public bool IsClosable
    {
        get => (bool)GetValue(IsClosableProperty);
        set => SetValue(IsClosableProperty, value);
    }

    /// <summary>Raised when the action button is pressed.</summary>
    public event EventHandler? ActionInvoked;

    /// <summary>Raised when the bar is dismissed.</summary>
    public event EventHandler? Closed;

    /// <summary>Shows the bar with a message, which is the call a host makes most often.</summary>
    public void Show(string title, string message, CadNotificationSeverity severity = CadNotificationSeverity.Info)
    {
        Title = title;
        Message = message;
        Severity = severity;
        Visibility = Visibility.Visible;
    }

    /// <summary>Hides the bar without raising <see cref="Closed"/>.</summary>
    public void Hide() => Visibility = Visibility.Collapsed;

    private void ApplySeverity(CadNotificationSeverity severity)
    {
        _icon.Kind = severity switch
        {
            CadNotificationSeverity.Success => IconKind.CheckCircle,
            CadNotificationSeverity.Warning => IconKind.Warning,
            CadNotificationSeverity.Error => IconKind.Error,
            _ => IconKind.Info
        };

        _icon.SetResourceReference(Control.ForegroundProperty, BrushKey(severity));

        Background = Tint(severity);

        if (Load(BrushKey(severity)) is Brush accent) BorderBrush = accent;
    }

    /// <summary>
    /// Resolves the tint on the first layout pass and again after a re-parent.
    /// </summary>
    /// <remarks>
    /// The accent is read from the theme dictionary, which normally lives above this bar — and a bar
    /// built before the host merges that dictionary would keep a transparent background forever.
    /// Parenting is not enough on its own: a host can parent the bar and merge the theme in either
    /// order, so the first measure is the earliest point where both are guaranteed.
    /// </remarks>
    protected override Size MeasureOverride(Size constraint)
    {
        if (!_tintResolved)
        {
            _tintResolved = true;

            ApplySeverity(Severity);
        }

        return base.MeasureOverride(constraint);
    }

    protected override void OnVisualParentChanged(DependencyObject oldParent)
    {
        base.OnVisualParentChanged(oldParent);

        // The new tree may carry a different theme.
        _tintResolved = false;
    }

    private void ApplyText()
    {
        _title.Text = Title;
        _title.Visibility = Title.Length == 0 ? Visibility.Collapsed : Visibility.Visible;
        _message.Text = Message;
    }

    private Brush? Load(string key) => TryFindResource(key) as Brush;

    private Brush Tint(CadNotificationSeverity severity)
    {
        if (Load(BrushKey(severity)) is not SolidColorBrush accent) return Brushes.Transparent;

        var tinted = new SolidColorBrush(Color.FromArgb(0x16, accent.Color.R, accent.Color.G, accent.Color.B));

        tinted.Freeze();

        return tinted;
    }

    private static string BrushKey(CadNotificationSeverity severity) => severity switch
    {
        CadNotificationSeverity.Success => "CONCOST.Brush.Success",
        CadNotificationSeverity.Warning => "CONCOST.Brush.Warning",
        CadNotificationSeverity.Error => "CONCOST.Brush.Danger",
        _ => "CONCOST.Brush.Info"
    };

    private static void OnSeverityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ApplicationInfoBar bar) bar.ApplySeverity((CadNotificationSeverity)e.NewValue);
    }

    private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ApplicationInfoBar bar) bar.ApplyText();
    }

    private static void OnActionTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not ApplicationInfoBar bar) return;

        string text = (string)e.NewValue ?? string.Empty;

        bar._action.Content = text;
        bar._action.Visibility = text.Length == 0 ? Visibility.Collapsed : Visibility.Visible;
    }

    private static void OnClosableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ApplicationInfoBar bar)
        {
            bar._close.Visibility = (bool)e.NewValue ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
