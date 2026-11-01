using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using CONCOST.UI.AttachedProperties;
using CONCOST.UI.Icons;

namespace CONCOST.UI.Controls;

/// <summary>One button in the title bar's quick access strip.</summary>
public class TitleBarAction
{
    public TitleBarAction(IconKind icon, string toolTip, ICommand? command = null, object? commandParameter = null)
    {
        Icon = icon;
        ToolTip = toolTip;
        Command = command;
        CommandParameter = commandParameter;
    }

    /// <summary>The glyph.</summary>
    public IconKind Icon { get; set; }

    /// <summary>What the button says it does.</summary>
    public string ToolTip { get; set; }

    /// <summary>What the button runs; a disabled command disables the button for free.</summary>
    public ICommand? Command { get; set; }

    /// <summary>Passed to the command.</summary>
    public object? CommandParameter { get; set; }
}

/// <summary>
/// The application title bar: product brand mark, quick access strip, active document title,
/// optional CAD search bar, user account pill and window control buttons (minimise, maximise, close).
/// </summary>
public class ApplicationTitleBar : Grid
{
    public static readonly DependencyProperty ApplicationNameProperty = DependencyProperty.Register(
        nameof(ApplicationName), typeof(string), typeof(ApplicationTitleBar),
        new FrameworkPropertyMetadata("CONCOST", OnTitleChanged));

    public static readonly DependencyProperty DocumentTitleProperty = DependencyProperty.Register(
        nameof(DocumentTitle), typeof(string), typeof(ApplicationTitleBar),
        new FrameworkPropertyMetadata(string.Empty, OnTitleChanged));

    public static readonly DependencyProperty IsDocumentModifiedProperty = DependencyProperty.Register(
        nameof(IsDocumentModified), typeof(bool), typeof(ApplicationTitleBar),
        new FrameworkPropertyMetadata(false, OnTitleChanged));

    public static readonly DependencyProperty ShowQuickAccessProperty = DependencyProperty.Register(
        nameof(ShowQuickAccess), typeof(bool), typeof(ApplicationTitleBar),
        new FrameworkPropertyMetadata(true, OnQuickAccessVisibilityChanged));

    public static readonly DependencyProperty ShowWindowButtonsProperty = DependencyProperty.Register(
        nameof(ShowWindowButtons), typeof(bool), typeof(ApplicationTitleBar),
        new FrameworkPropertyMetadata(true, OnWindowButtonsVisibilityChanged));

    public static readonly DependencyProperty IsMaximizedProperty = DependencyProperty.Register(
        nameof(IsMaximized), typeof(bool), typeof(ApplicationTitleBar),
        new FrameworkPropertyMetadata(false, OnIsMaximizedChanged));

    public static readonly DependencyProperty TitleForegroundProperty = DependencyProperty.Register(
        nameof(TitleForeground), typeof(Brush), typeof(ApplicationTitleBar),
        new FrameworkPropertyMetadata(null, OnVisualPropertyChanged));

    public static readonly DependencyProperty TitleFontSizeProperty = DependencyProperty.Register(
        nameof(TitleFontSize), typeof(double), typeof(ApplicationTitleBar),
        new FrameworkPropertyMetadata(12.0, OnVisualPropertyChanged));

    public static readonly DependencyProperty TitleFontWeightProperty = DependencyProperty.Register(
        nameof(TitleFontWeight), typeof(FontWeight), typeof(ApplicationTitleBar),
        new FrameworkPropertyMetadata(FontWeights.Normal, OnVisualPropertyChanged));

    public static readonly DependencyProperty TitleAlignmentProperty = DependencyProperty.Register(
        nameof(TitleAlignment), typeof(HorizontalAlignment), typeof(ApplicationTitleBar),
        new FrameworkPropertyMetadata(HorizontalAlignment.Center, OnVisualPropertyChanged));

    public static readonly DependencyProperty AppIconProperty = DependencyProperty.Register(
        nameof(AppIcon), typeof(IconKind), typeof(ApplicationTitleBar),
        new FrameworkPropertyMetadata(IconKind.Cube, OnVisualPropertyChanged));

    public static readonly DependencyProperty AppIconBrushProperty = DependencyProperty.Register(
        nameof(AppIconBrush), typeof(Brush), typeof(ApplicationTitleBar),
        new FrameworkPropertyMetadata(null, OnVisualPropertyChanged));

    public static readonly DependencyProperty AppIconSizeProperty = DependencyProperty.Register(
        nameof(AppIconSize), typeof(double), typeof(ApplicationTitleBar),
        new FrameworkPropertyMetadata(16.0, OnVisualPropertyChanged));

    public static readonly DependencyProperty ShowAppIconProperty = DependencyProperty.Register(
        nameof(ShowAppIcon), typeof(bool), typeof(ApplicationTitleBar),
        new FrameworkPropertyMetadata(true, OnVisualPropertyChanged));

    public static readonly DependencyProperty ShowSearchBoxProperty = DependencyProperty.Register(
        nameof(ShowSearchBox), typeof(bool), typeof(ApplicationTitleBar),
        new FrameworkPropertyMetadata(false, OnSearchBoxVisibilityChanged));

    public static readonly DependencyProperty SearchPlaceholderProperty = DependencyProperty.Register(
        nameof(SearchPlaceholder), typeof(string), typeof(ApplicationTitleBar),
        new FrameworkPropertyMetadata("Tìm kiếm lệnh, công cụ CAD (Ctrl+K)...", OnVisualPropertyChanged));

    public static readonly DependencyProperty UserAccountNameProperty = DependencyProperty.Register(
        nameof(UserAccountName), typeof(string), typeof(ApplicationTitleBar),
        new FrameworkPropertyMetadata(null, OnVisualPropertyChanged));

    public static readonly DependencyProperty ShowUserAccountProperty = DependencyProperty.Register(
        nameof(ShowUserAccount), typeof(bool), typeof(ApplicationTitleBar),
        new FrameworkPropertyMetadata(false, OnUserAccountVisibilityChanged));

    public static readonly DependencyProperty SubtitleProperty = DependencyProperty.Register(
        nameof(Subtitle), typeof(string), typeof(ApplicationTitleBar),
        new FrameworkPropertyMetadata(null, OnTitleChanged));

    private readonly StackPanel _brand;
    private readonly Icon _mark;
    private readonly StackPanel _quickAccess;
    private readonly Grid _centerArea;
    private readonly StackPanel _titleStack;
    private readonly TextBlock _title;
    private readonly TextBlock _subtitle;
    private readonly Border _searchBoxHost;
    private readonly Icon _searchIcon;
    private readonly TextBlock _searchHint;
    private readonly Border _userAccountHost;
    private readonly Icon _userIcon;
    private readonly TextBlock _userAccountText;
    private readonly StackPanel _windowButtons;
    private readonly TextBlock _maximizeGlyph;
    private readonly System.Collections.Generic.List<TextBlock> _windowButtonGlyphs = new();

    public ApplicationTitleBar()
    {
        ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // 0: Brand
        ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // 1: Quick Access
        ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // 2: Title & Search
        ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // 3: User Account
        ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // 4: Window Buttons

        Height = 36;
        Background = Brushes.Transparent;

        // 0. Brand
        _brand = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(10, 0, 8, 0)
        };

        _mark = new Icon
        {
            Kind = IconKind.Cube,
            Width = 16,
            Height = 16,
            VerticalAlignment = VerticalAlignment.Center
        };
        _mark.SetResourceReference(Control.ForegroundProperty, "CONCOST.Brush.Primary");
        _brand.Children.Add(_mark);

        SetColumn(_brand, 0);
        Children.Add(_brand);

        // 1. Quick Access
        _quickAccess = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 8, 0)
        };
        SetColumn(_quickAccess, 1);
        Children.Add(_quickAccess);

        // 2. Center Area (Title & SearchBox)
        _centerArea = new Grid { VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Stretch };
        
        _titleStack = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center
        };

        _title = new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis,
            FontSize = 12
        };
        _title.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextPrimary");
        _titleStack.Children.Add(_title);

        _subtitle = new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center,
            FontSize = 10,
            Margin = new Thickness(8, 0, 0, 0),
            Visibility = Visibility.Collapsed
        };
        _subtitle.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");
        _titleStack.Children.Add(_subtitle);

        _centerArea.Children.Add(_titleStack);

        // Optional CAD InfoCenter Search Bar
        _searchBoxHost = new Border
        {
            Width = 240,
            Height = 24,
            CornerRadius = new CornerRadius(12),
            BorderThickness = new Thickness(1),
            Margin = new Thickness(12, 0, 12, 0),
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center,
            Visibility = Visibility.Collapsed
        };
        _searchBoxHost.SetResourceReference(Border.BackgroundProperty, "CONCOST.Brush.SurfaceSecondary");
        _searchBoxHost.SetResourceReference(Border.BorderBrushProperty, "CONCOST.Brush.Border");

        var searchGrid = new Grid();
        searchGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        searchGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        _searchIcon = new Icon { Kind = IconKind.Search, Width = 12, Height = 12, Margin = new Thickness(8, 0, 4, 0), VerticalAlignment = VerticalAlignment.Center };
        _searchIcon.SetResourceReference(Control.ForegroundProperty, "CONCOST.Brush.TextTertiary");
        Grid.SetColumn(_searchIcon, 0);
        searchGrid.Children.Add(_searchIcon);

        _searchHint = new TextBlock { Text = "Tìm lệnh CAD (Ctrl+K)", FontSize = 11, VerticalAlignment = VerticalAlignment.Center, TextTrimming = TextTrimming.CharacterEllipsis };
        _searchHint.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextTertiary");
        Grid.SetColumn(_searchHint, 1);
        searchGrid.Children.Add(_searchHint);

        _searchBoxHost.Child = searchGrid;
        _centerArea.Children.Add(_searchBoxHost);

        SetColumn(_centerArea, 2);
        Children.Add(_centerArea);

        // 3. User Account Pill
        _userAccountHost = new Border
        {
            CornerRadius = new CornerRadius(14),
            Padding = new Thickness(8, 3, 10, 3),
            Margin = new Thickness(0, 0, 10, 0),
            VerticalAlignment = VerticalAlignment.Center,
            Visibility = Visibility.Collapsed
        };
        _userAccountHost.SetResourceReference(Border.BackgroundProperty, "CONCOST.Brush.SurfaceSecondary");

        var userStack = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
        _userIcon = new Icon { Kind = IconKind.User, Width = 13, Height = 13, Margin = new Thickness(0, 0, 6, 0), VerticalAlignment = VerticalAlignment.Center };
        _userIcon.SetResourceReference(Control.ForegroundProperty, "CONCOST.Brush.Primary");
        userStack.Children.Add(_userIcon);

        _userAccountText = new TextBlock { FontSize = 11, FontWeight = FontWeights.Medium, VerticalAlignment = VerticalAlignment.Center };
        _userAccountText.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextPrimary");
        userStack.Children.Add(_userAccountText);

        _userAccountHost.Child = userStack;
        SetColumn(_userAccountHost, 3);
        Children.Add(_userAccountHost);

        // 4. Window Buttons (Minimize, Maximize, Close) with Sleek Hover States
        _windowButtons = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Stretch };
        
        var minBtn = CreateWindowButton("—", "Minimize", () => MinimizeRequested?.Invoke(this, EventArgs.Empty), isClose: false);
        _maximizeGlyph = new TextBlock { Text = "□", FontSize = 11, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
        _maximizeGlyph.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");

        var maxBtn = CreateWindowButton(_maximizeGlyph, "Maximize", () => MaximizeRequested?.Invoke(this, EventArgs.Empty), isClose: false);
        var closeBtn = CreateWindowButton("✕", "Close", () => CloseRequested?.Invoke(this, EventArgs.Empty), isClose: true);

        _windowButtons.Children.Add(minBtn);
        _windowButtons.Children.Add(maxBtn);
        _windowButtons.Children.Add(closeBtn);

        SetColumn(_windowButtons, 4);
        Children.Add(_windowButtons);

        MouseLeftButtonDown += OnBarMouseDown;
        UpdateTitle();
        ApplyVisualProperties();
    }

    public ObservableCollection<TitleBarAction> QuickAccess { get; } = new();

    public string ApplicationName
    {
        get => (string)GetValue(ApplicationNameProperty);
        set => SetValue(ApplicationNameProperty, value);
    }

    public string DocumentTitle
    {
        get => (string)GetValue(DocumentTitleProperty);
        set => SetValue(DocumentTitleProperty, value);
    }

    public bool IsDocumentModified
    {
        get => (bool)GetValue(IsDocumentModifiedProperty);
        set => SetValue(IsDocumentModifiedProperty, value);
    }

    public bool ShowQuickAccess
    {
        get => (bool)GetValue(ShowQuickAccessProperty);
        set => SetValue(ShowQuickAccessProperty, value);
    }

    public bool ShowWindowButtons
    {
        get => (bool)GetValue(ShowWindowButtonsProperty);
        set => SetValue(ShowWindowButtonsProperty, value);
    }

    public bool IsMaximized
    {
        get => (bool)GetValue(IsMaximizedProperty);
        set => SetValue(IsMaximizedProperty, value);
    }

    public Brush? TitleForeground
    {
        get => (Brush?)GetValue(TitleForegroundProperty);
        set => SetValue(TitleForegroundProperty, value);
    }

    public double TitleFontSize
    {
        get => (double)GetValue(TitleFontSizeProperty);
        set => SetValue(TitleFontSizeProperty, value);
    }

    public FontWeight TitleFontWeight
    {
        get => (FontWeight)GetValue(TitleFontWeightProperty);
        set => SetValue(TitleFontWeightProperty, value);
    }

    public HorizontalAlignment TitleAlignment
    {
        get => (HorizontalAlignment)GetValue(TitleAlignmentProperty);
        set => SetValue(TitleAlignmentProperty, value);
    }

    public IconKind AppIcon
    {
        get => (IconKind)GetValue(AppIconProperty);
        set => SetValue(AppIconProperty, value);
    }

    public Brush? AppIconBrush
    {
        get => (Brush?)GetValue(AppIconBrushProperty);
        set => SetValue(AppIconBrushProperty, value);
    }

    public double AppIconSize
    {
        get => (double)GetValue(AppIconSizeProperty);
        set => SetValue(AppIconSizeProperty, value);
    }

    public bool ShowAppIcon
    {
        get => (bool)GetValue(ShowAppIconProperty);
        set => SetValue(ShowAppIconProperty, value);
    }

    public bool ShowSearchBox
    {
        get => (bool)GetValue(ShowSearchBoxProperty);
        set => SetValue(ShowSearchBoxProperty, value);
    }

    public string SearchPlaceholder
    {
        get => (string)GetValue(SearchPlaceholderProperty);
        set => SetValue(SearchPlaceholderProperty, value);
    }

    public string? UserAccountName
    {
        get => (string?)GetValue(UserAccountNameProperty);
        set => SetValue(UserAccountNameProperty, value);
    }

    public bool ShowUserAccount
    {
        get => (bool)GetValue(ShowUserAccountProperty);
        set => SetValue(ShowUserAccountProperty, value);
    }

    public string? Subtitle
    {
        get => (string?)GetValue(SubtitleProperty);
        set => SetValue(SubtitleProperty, value);
    }

    public event EventHandler? MinimizeRequested;
    public event EventHandler? MaximizeRequested;
    public event EventHandler? CloseRequested;

    public void RefreshQuickAccess()
    {
        _quickAccess.Children.Clear();

        foreach (var action in QuickAccess)
        {
            var button = new Button
            {
                Width = 26,
                Height = 26,
                Margin = new Thickness(1, 0, 1, 0),
                ToolTip = action.ToolTip,
                Command = action.Command,
                CommandParameter = action.CommandParameter,
                Cursor = Cursors.Hand
            };

            button.SetResourceReference(StyleProperty, "CONCOST.Button.Icon.Ghost");
            if (TitleForeground is not null)
            {
                button.Foreground = TitleForeground;
            }
            IconElement.SetKind(button, action.Icon);
            IconElement.SetIconSize(button, 14);

            _quickAccess.Children.Add(button);
        }
    }

    public static string ComposeTitle(string? documentTitle, bool isModified, string? applicationName)
    {
        string document = (documentTitle ?? string.Empty).Trim();
        if (document.Length > 0 && isModified) document += "*";
        if (string.IsNullOrEmpty(applicationName)) return document;
        return document.Length == 0 ? applicationName! : $"{document} — {applicationName}";
    }

    private void OnBarMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton != MouseButton.Left) return;
        if (e.OriginalSource is DependencyObject source && IsInteractive(source)) return;

        var window = Window.GetWindow(this);

        if (e.ClickCount == 2)
        {
            MaximizeRequested?.Invoke(this, EventArgs.Empty);
            if (window is not null && !ShowWindowButtons)
                window.WindowState = window.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
            e.Handled = true;
            return;
        }

        try
        {
            window?.DragMove();
        }
        catch (InvalidOperationException)
        {
        }
    }

    private static bool IsInteractive(DependencyObject source)
    {
        for (DependencyObject? current = source; current is not null;)
        {
            if (current is ButtonBase or TextBox) return true;
            if (current is ApplicationTitleBar) return false;

            current = current is Visual or System.Windows.Media.Media3D.Visual3D
                ? VisualTreeHelper.GetParent(current)
                : LogicalTreeHelper.GetParent(current);
        }
        return false;
    }

    private Border CreateWindowButton(object content, string tooltip, Action action, bool isClose)
    {
        var border = new Border
        {
            Width = 44,
            Background = Brushes.Transparent,
            Cursor = Cursors.Hand,
            ToolTip = tooltip,
            VerticalAlignment = VerticalAlignment.Stretch
        };

        UIElement childElement;
        if (content is string glyphStr)
        {
            var text = new TextBlock
            {
                Text = glyphStr,
                FontSize = isClose ? 11 : 12,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            text.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");
            _windowButtonGlyphs.Add(text);
            childElement = text;
        }
        else
        {
            if (content is TextBlock tb) _windowButtonGlyphs.Add(tb);
            childElement = (UIElement)content;
        }

        border.Child = childElement;

        border.MouseEnter += (_, _) =>
        {
            if (isClose)
            {
                border.Background = new SolidColorBrush(Color.FromRgb(0xE8, 0x11, 0x23));
                if (border.Child is TextBlock tb) tb.Foreground = Brushes.White;
            }
            else
            {
                border.Background = new SolidColorBrush(Color.FromArgb(0x22, 0x88, 0x88, 0x88));
            }
        };

        border.MouseLeave += (_, _) =>
        {
            border.Background = Brushes.Transparent;
            if (isClose && border.Child is TextBlock tb)
            {
                if (TitleForeground is not null)
                    tb.Foreground = TitleForeground;
                else
                    tb.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");
            }
        };

        border.MouseLeftButtonUp += (_, _) => action();

        return border;
    }

    private void UpdateTitle()
    {
        _title.Text = ComposeTitle(DocumentTitle, IsDocumentModified, ApplicationName);

        if (!string.IsNullOrWhiteSpace(Subtitle))
        {
            _subtitle.Text = $"({Subtitle})";
            _subtitle.Visibility = Visibility.Visible;
        }
        else
        {
            _subtitle.Visibility = Visibility.Collapsed;
        }

        var window = Window.GetWindow(this);
        if (window is not null) window.Title = _title.Text;
    }

    private void ApplyVisualProperties()
    {
        if (TitleForeground is not null)
        {
            _title.Foreground = TitleForeground;
            if (TitleForeground is SolidColorBrush scb)
            {
                _subtitle.Foreground = new SolidColorBrush(Color.FromArgb(170, scb.Color.R, scb.Color.G, scb.Color.B));
            }
            else
            {
                _subtitle.Foreground = TitleForeground;
            }

            foreach (var glyph in _windowButtonGlyphs)
                glyph.Foreground = TitleForeground;

            _searchBoxHost.Background = new SolidColorBrush(Color.FromArgb(0x1E, 0xFF, 0xFF, 0xFF));
            _searchBoxHost.BorderBrush = new SolidColorBrush(Color.FromArgb(0x38, 0xFF, 0xFF, 0xFF));
            _searchIcon.Foreground = new SolidColorBrush(Color.FromArgb(0xA0, 0xFF, 0xFF, 0xFF));
            _searchHint.Foreground = new SolidColorBrush(Color.FromArgb(0xA0, 0xFF, 0xFF, 0xFF));

            _userAccountHost.Background = new SolidColorBrush(Color.FromArgb(0x1E, 0xFF, 0xFF, 0xFF));
            _userAccountHost.BorderBrush = new SolidColorBrush(Color.FromArgb(0x38, 0xFF, 0xFF, 0xFF));
            _userAccountText.Foreground = TitleForeground;
        }
        else
        {
            _title.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextPrimary");
            _subtitle.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");
            foreach (var glyph in _windowButtonGlyphs)
                glyph.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");

            _searchBoxHost.SetResourceReference(Border.BackgroundProperty, "CONCOST.Brush.SurfaceSecondary");
            _searchBoxHost.SetResourceReference(Border.BorderBrushProperty, "CONCOST.Brush.Border");
            _searchIcon.SetResourceReference(Control.ForegroundProperty, "CONCOST.Brush.TextTertiary");
            _searchHint.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextTertiary");

            _userAccountHost.SetResourceReference(Border.BackgroundProperty, "CONCOST.Brush.SurfaceSecondary");
            _userAccountHost.BorderBrush = null;
            _userAccountText.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextPrimary");
        }

        _title.FontSize = TitleFontSize;
        _title.FontWeight = TitleFontWeight;
        _titleStack.HorizontalAlignment = TitleAlignment;

        _mark.Kind = AppIcon;
        _mark.Width = AppIconSize;
        _mark.Height = AppIconSize;

        if (AppIconBrush is not null)
            _mark.Foreground = AppIconBrush;
        else
            _mark.SetResourceReference(Control.ForegroundProperty, "CONCOST.Brush.Primary");

        _brand.Visibility = ShowAppIcon ? Visibility.Visible : Visibility.Collapsed;
        _searchBoxHost.Visibility = ShowSearchBox ? Visibility.Visible : Visibility.Collapsed;

        if (ShowUserAccount && !string.IsNullOrWhiteSpace(UserAccountName))
        {
            _userAccountText.Text = UserAccountName;
            _userAccountHost.Visibility = Visibility.Visible;
        }
        else
        {
            _userAccountHost.Visibility = Visibility.Collapsed;
        }

        RefreshQuickAccess();
    }

    private static void OnTitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ApplicationTitleBar bar) bar.UpdateTitle();
    }

    private static void OnVisualPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ApplicationTitleBar bar) bar.ApplyVisualProperties();
    }

    private static void OnQuickAccessVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not ApplicationTitleBar bar) return;
        bar._quickAccess.Visibility = (bool)e.NewValue ? Visibility.Visible : Visibility.Collapsed;
        if ((bool)e.NewValue) bar.RefreshQuickAccess();
    }

    private static void OnWindowButtonsVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ApplicationTitleBar bar)
            bar._windowButtons.Visibility = (bool)e.NewValue ? Visibility.Visible : Visibility.Collapsed;
    }

    private static void OnSearchBoxVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ApplicationTitleBar bar)
            bar._searchBoxHost.Visibility = (bool)e.NewValue ? Visibility.Visible : Visibility.Collapsed;
    }

    private static void OnUserAccountVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ApplicationTitleBar bar) bar.ApplyVisualProperties();
    }

    private static void OnIsMaximizedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not ApplicationTitleBar bar) return;
        bar._maximizeGlyph.Text = (bool)e.NewValue ? "❐" : "□";
        if (bar._maximizeGlyph.Parent is Border b)
            b.ToolTip = (bool)e.NewValue ? "Restore" : "Maximize";
    }
}
