using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using CONCOST.UI.AttachedProperties;
using CONCOST.UI.Icons;

namespace CONCOST.UI.Controls.Ribbon;

/// <summary>One entry in the backstage rail.</summary>
public class BackstageItem
{
    public BackstageItem(string header, IconKind icon = IconKind.File)
    {
        Header = header ?? string.Empty;
        Icon = icon;
    }

    /// <summary>The caption on the rail.</summary>
    public string Header { get; set; }

    /// <summary>The glyph in front of the caption.</summary>
    public IconKind Icon { get; set; }

    /// <summary>The page shown when the entry is selected; <c>null</c> shows the header alone.</summary>
    public object? Content { get; set; }

    /// <summary>Draws a rule above the entry, which is how Info and Options are set apart.</summary>
    public bool IsSeparator { get; set; }

    /// <summary>Anything the host hangs off the entry.</summary>
    public object? Tag { get; set; }

    /// <inheritdoc />
    public override string ToString() => Header;
}

/// <summary>
/// The full-window backstage behind the File button: a rail of pages on the left, the selected page on
/// the right, and a Back button that returns to the drawing.
/// </summary>
/// <remarks>
/// <para>
/// The backstage is a panel of the window, not a popup, because a page such as Options or Print needs
/// the whole window and swallows the rest of the UI while it is open. It stays in the layout with
/// <see cref="IsOpen"/> driving visibility, so the drawing area underneath keeps its state.
/// </para>
/// <para>
/// Pages are plain content, so a host can put anything there — a file list, a print preview, a
/// settings form — without this control knowing what a page is.
/// </para>
/// </remarks>
public class RibbonBackstageView : Grid
{
    /// <summary>Identifies the <see cref="IsOpen"/> property.</summary>
    public static readonly DependencyProperty IsOpenProperty = DependencyProperty.Register(
        nameof(IsOpen), typeof(bool), typeof(RibbonBackstageView),
        new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnIsOpenChanged));

    /// <summary>Identifies the <see cref="ApplicationName"/> property.</summary>
    public static readonly DependencyProperty ApplicationNameProperty = DependencyProperty.Register(
        nameof(ApplicationName), typeof(string), typeof(RibbonBackstageView),
        new FrameworkPropertyMetadata("CONCOST", OnApplicationNameChanged));

    private readonly TextBlock _applicationName;
    private readonly StackPanel _rail;
    private readonly Border _page;
    private readonly Border _back;

    private BackstageItem? _selected;

    public RibbonBackstageView()
    {
        ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(216) });
        ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        Visibility = Visibility.Collapsed;

        // ---------------------------------------------------------------- rail
        _rail = new StackPanel { Margin = new Thickness(0, 0, 0, 0) };

        _applicationName = new TextBlock
        {
            Text = "CONCOST",
            FontSize = 16,
            FontWeight = FontWeights.SemiBold,
            Margin = new Thickness(18, 16, 12, 14)
        };

        _applicationName.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.Primary");

        _rail.Children.Add(_applicationName);

        _back = new Border { Margin = new Thickness(10, 12, 10, 4), Padding = new Thickness(8, 6, 8, 6), Cursor = Cursors.Hand };
        _back.SetResourceReference(Border.CornerRadiusProperty, "CONCOST.CornerRadius.Small");

        var backContent = new StackPanel { Orientation = Orientation.Horizontal };

        var backIcon = new Icon { Kind = IconKind.ArrowLeft, Width = 12, Height = 12, VerticalAlignment = VerticalAlignment.Center };
        backIcon.SetResourceReference(Control.ForegroundProperty, "CONCOST.Brush.TextSecondary");

        var backText = new TextBlock { Text = "Back", FontSize = 12, Margin = new Thickness(6, 0, 0, 0), VerticalAlignment = VerticalAlignment.Center };
        backText.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");

        backContent.Children.Add(backIcon);
        backContent.Children.Add(backText);

        _back.Child = backContent;
        _back.MouseLeftButtonUp += (_, _) => Close();

        SetColumn(_rail, 0);

        var railSurface = new Border { BorderThickness = new Thickness(0, 0, 1, 0), Child = _rail };
        railSurface.SetResourceReference(Border.BackgroundProperty, "CONCOST.Brush.SurfaceSecondary");
        railSurface.SetResourceReference(Border.BorderBrushProperty, "CONCOST.Brush.Border");

        Children.Add(railSurface);

        // ---------------------------------------------------------------- page
        _page = new Border { Padding = new Thickness(0) };
        _page.SetResourceReference(Border.BackgroundProperty, "CONCOST.Brush.Background");

        SetColumn(_page, 1);
        Children.Add(_page);

        Items.CollectionChanged += (_, _) => RebuildRail();

        PreviewKeyDown += OnPreviewKeyDown;
    }

    /// <summary>The rail entries.</summary>
    public ObservableCollection<BackstageItem> Items { get; } = new();

    /// <summary>Whether the backstage covers the window.</summary>
    public bool IsOpen
    {
        get => (bool)GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    /// <summary>The product name at the top of the rail.</summary>
    public string ApplicationName
    {
        get => (string)GetValue(ApplicationNameProperty);
        set => SetValue(ApplicationNameProperty, value);
    }

    /// <summary>The entry whose page is showing.</summary>
    public BackstageItem? SelectedItem
    {
        get => _selected;
        set
        {
            if (ReferenceEquals(_selected, value)) return;

            _selected = value;

            ShowPage(value);
            HighlightSelection();

            SelectedItemChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>Raised when the open state changes.</summary>
    public event EventHandler? IsOpenChanged;

    /// <summary>Raised when a different page is selected.</summary>
    public event EventHandler? SelectedItemChanged;

    /// <summary>Opens the backstage on the first page.</summary>
    public void Open()
    {
        IsOpen = true;
    }

    /// <summary>Returns to the drawing.</summary>
    public void Close() => IsOpen = false;

    /// <summary>Selects a page by its header.</summary>
    public bool Select(string header)
    {
        var item = Items.FirstOrDefault(candidate => string.Equals(candidate.Header, header, StringComparison.OrdinalIgnoreCase));

        if (item is null) return false;

        SelectedItem = item;

        return true;
    }

    /// <summary>Rebuilds the rail after items were added or removed.</summary>
    public void RebuildRail()
    {
        _rail.Children.Clear();
        _rail.Children.Add(_applicationName);
        _rail.Children.Add(_back);

        foreach (var item in Items)
        {
            if (item.IsSeparator)
            {
                var rule = new Border { Height = 1, Margin = new Thickness(18, 8, 18, 8) };
                rule.SetResourceReference(Border.BackgroundProperty, "CONCOST.Brush.Border");

                _rail.Children.Add(rule);
            }

            _rail.Children.Add(CreateRailEntry(item));
        }

        HighlightSelection();
    }

    private Button CreateRailEntry(BackstageItem item)
    {
        var content = new StackPanel { Orientation = Orientation.Horizontal };

        var icon = new Icon { Kind = item.Icon, Width = 14, Height = 14, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 10, 0) };
        icon.SetResourceReference(Control.ForegroundProperty, "CONCOST.Brush.TextSecondary");

        var text = new TextBlock { Text = item.Header, FontSize = 13, VerticalAlignment = VerticalAlignment.Center };
        text.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextPrimary");

        content.Children.Add(icon);
        content.Children.Add(text);

        var button = new Button
        {
            Content = content,
            HorizontalContentAlignment = HorizontalAlignment.Left,
            Height = 34,
            Margin = new Thickness(4, 1, 4, 1),
            Padding = new Thickness(14, 0, 10, 0),
            Tag = item,
            Background = Brushes.Transparent,
            BorderThickness = new Thickness(0)
        };

        button.SetResourceReference(StyleProperty, "CONCOST.Button.Secondary");
        button.Click += (_, _) => SelectedItem = item;

        return button;
    }

    private void HighlightSelection()
    {
        foreach (var button in _rail.Children.OfType<Button>())
        {
            if (ReferenceEquals(button.Tag, _selected)) button.SetResourceReference(BackgroundProperty, "CONCOST.Brush.SurfacePressed");
            else button.Background = Brushes.Transparent;
        }
    }

    private void ShowPage(BackstageItem? item)
    {
        if (item?.Content is null)
        {
            var headline = new TextBlock
            {
                Text = item?.Header ?? "File",
                FontSize = 20,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(28, 24, 28, 8)
            };

            headline.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextPrimary");

            _page.Child = headline;

            return;
        }

        _page.Child = item.Content as UIElement ?? new TextBlock { Text = item.Content.ToString() };
    }

    private void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (!IsOpen || e.Key != Key.Escape) return;

        Close();

        e.Handled = true;
    }

    private static void OnIsOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not RibbonBackstageView view) return;

        bool open = (bool)e.NewValue;

        view.Visibility = open ? Visibility.Visible : Visibility.Collapsed;

        if (open && view._selected is null) view.SelectedItem = view.Items.FirstOrDefault();

        view.IsOpenChanged?.Invoke(view, EventArgs.Empty);
    }

    private static void OnApplicationNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is RibbonBackstageView view) view._applicationName.Text = (string)e.NewValue ?? string.Empty;
    }
}
