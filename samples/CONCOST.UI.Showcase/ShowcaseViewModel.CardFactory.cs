using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using CONCOST.UI.AttachedProperties;
using CONCOST.UI.Controls;
using CONCOST.UI.Icons;
using CONCOST.UI.Services;

namespace CONCOST.UI.Showcase;

public partial class ShowcaseViewModel
{
    /// <summary>
    /// Creates a standardized, modern showcase card featuring:
    /// 1. Sleek card container with subtle border.
    /// 2. Branded header with primary accent pill, semibold title, and modern code action buttons.
    /// 3. Dedicated Preview Stage (Canvas) with SurfaceSecondary soft background, framing, and clean breathing room.
    /// 4. Expandable syntax-highlighted code drawer with language badge and 1-click clipboard copy.
    /// </summary>
    public Border CreateStandardDemoCard(string title, UIElement content, string? xaml = null, string? csharp = null)
    {
        var card = new Border
        {
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(12),
            Margin = new Thickness(0, 0, 0, 20),
            ClipToBounds = true
        };
        card.SetResourceReference(Border.BackgroundProperty, "CONCOST.Brush.Surface");
        card.SetResourceReference(Border.BorderBrushProperty, "CONCOST.Brush.Border");

        var stack = new StackPanel();

        // 1. Header Bar
        var header = new Grid
        {
            Height = 44,
            Margin = new Thickness(18, 0, 16, 0)
        };
        header.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        header.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        var titlePanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center
        };

        // Primary Accent Pill Indicator
        var accentPill = new Border
        {
            Width = 3.5,
            Height = 16,
            CornerRadius = new CornerRadius(1.75),
            Margin = new Thickness(0, 0, 10, 0),
            VerticalAlignment = VerticalAlignment.Center
        };
        accentPill.SetResourceReference(Border.BackgroundProperty, "CONCOST.Brush.Primary");
        titlePanel.Children.Add(accentPill);

        var heading = new TextBlock
        {
            Text = title,
            FontSize = 13.5,
            FontWeight = FontWeights.SemiBold,
            VerticalAlignment = VerticalAlignment.Center
        };
        heading.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextPrimary");
        titlePanel.Children.Add(heading);

        Grid.SetColumn(titlePanel, 0);
        header.Children.Add(titlePanel);

        var buttons = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center
        };
        Grid.SetColumn(buttons, 1);
        header.Children.Add(buttons);

        stack.Children.Add(header);

        // Header Divider
        var headerDivider = new Border { Height = 1 };
        headerDivider.SetResourceReference(Border.BackgroundProperty, "CONCOST.Brush.BorderSubtle");
        stack.Children.Add(headerDivider);

        // 2. Dedicated Preview Stage (Canvas Stage)
        bool isFullBleed = content is FrameworkElement fe && (
            fe.Tag as string == "full-bleed" ||
            fe is Spreadsheet ||
            fe is StackedHeaderDataGrid ||
            fe is DataGrid);

        var stage = new Border
        {
            CornerRadius = new CornerRadius(8),
            BorderThickness = new Thickness(1),
            Padding = isFullBleed ? new Thickness(0) : new Thickness(22, 18, 22, 18),
            Margin = new Thickness(16, 14, 16, 16),
            ClipToBounds = true
        };
        stage.SetResourceReference(Border.BackgroundProperty, "CONCOST.Brush.CanvasBackground");
        stage.SetResourceReference(Border.BorderBrushProperty, "CONCOST.Brush.BorderSubtle");
        stage.Child = content;

        stack.Children.Add(stage);

        // 3. Code Drawer Panel
        if (!string.IsNullOrEmpty(xaml) || !string.IsNullOrEmpty(csharp))
        {
            // Detect if xaml snippet is actually C# code
            bool isVi = LocalizationManager.Current.Culture.TwoLetterISOLanguageName.Equals("vi", StringComparison.OrdinalIgnoreCase);
            bool xamlLooksLikeXaml = !string.IsNullOrWhiteSpace(xaml) && xaml.TrimStart().StartsWith("<");
            string xamlBtnText = isVi ? (xamlLooksLikeXaml ? "Xem XAML" : "Xem mã") : (xamlLooksLikeXaml ? "View XAML" : "View Code");
            string csBtnText = isVi ? "Xem C#" : "View C#";
            string xamlBadgeText = isVi ? (xamlLooksLikeXaml ? "Mã XAML" : "Đoạn mã") : (xamlLooksLikeXaml ? "XAML Code" : "Code Snippet");
            string csBadgeText = isVi ? "Mã C#" : "C# Code";
            string copyText = isVi ? "Sao chép" : "Copy";
            string copiedText = isVi ? "Đã chép!" : "Copied!";

            FrameworkElement? xamlView = string.IsNullOrEmpty(xaml) ? null : CreateXamlSyntaxHighlighter(xaml!);
            FrameworkElement? csView = string.IsNullOrEmpty(csharp) ? null : CreatePlainCodeView(csharp!);

            var codeBox = new Border
            {
                CornerRadius = new CornerRadius(8),
                BorderThickness = new Thickness(1),
                Margin = new Thickness(16, 0, 16, 16),
                Visibility = Visibility.Collapsed
            };
            codeBox.SetResourceReference(Border.BackgroundProperty, "CONCOST.Brush.SurfaceSecondary");
            codeBox.SetResourceReference(Border.BorderBrushProperty, "CONCOST.Brush.Border");

            var codeInner = new StackPanel();

            // Code Top Bar with Language Tag & Copy Action
            var codeTopBar = new Grid
            {
                Height = 36,
                Margin = new Thickness(12, 0, 8, 0)
            };
            codeTopBar.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            codeTopBar.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var langBadge = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center
            };
            var tagIcon = new CONCOST.UI.Icons.Icon
            {
                Kind = IconKind.FileCode,
                Width = 14,
                Height = 14,
                Margin = new Thickness(0, 0, 6, 0)
            };
            tagIcon.SetResourceReference(CONCOST.UI.Icons.Icon.ForegroundProperty, "CONCOST.Brush.TextSecondary");

            var langLabel = new TextBlock
            {
                Text = xamlBadgeText,
                FontSize = 11.5,
                FontWeight = FontWeights.Medium,
                VerticalAlignment = VerticalAlignment.Center
            };
            langLabel.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");

            langBadge.Children.Add(tagIcon);
            langBadge.Children.Add(langLabel);
            Grid.SetColumn(langBadge, 0);
            codeTopBar.Children.Add(langBadge);

            string currentActiveCode = xaml ?? csharp ?? "";

            var copyBtn = new Button
            {
                Content = copyText,
                Padding = new Thickness(10, 2, 10, 2),
                FontSize = 11.5,
                Height = 24,
                VerticalAlignment = VerticalAlignment.Center,
                Cursor = Cursors.Hand
            };
            copyBtn.SetResourceReference(Button.StyleProperty, "CONCOST.Button.Secondary");
            IconElement.SetKind(copyBtn, IconKind.Check);
            BorderElement.SetCornerRadius(copyBtn, new CornerRadius(4));

            copyBtn.Click += (_, _) =>
            {
                try
                {
                    Clipboard.SetText(currentActiveCode.Trim());
                    copyBtn.Content = copiedText;
                    var timer = new System.Windows.Threading.DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
                    timer.Tick += (ts, te) =>
                    {
                        copyBtn.Content = copyText;
                        timer.Stop();
                    };
                    timer.Start();
                }
                catch { }
            };
            Grid.SetColumn(copyBtn, 1);
            codeTopBar.Children.Add(copyBtn);

            codeInner.Children.Add(codeTopBar);

            var codeDivider = new Border { Height = 1 };
            codeDivider.SetResourceReference(Border.BackgroundProperty, "CONCOST.Brush.Border");
            codeInner.Children.Add(codeDivider);

            if (xamlView != null) codeInner.Children.Add(xamlView);
            if (csView != null)
            {
                if (xamlView != null) csView.Visibility = Visibility.Collapsed;
                codeInner.Children.Add(csView);
            }
            codeBox.Child = codeInner;

            void ToggleCode(FrameworkElement? targetView, string langName, string snippet)
            {
                if (codeBox.Visibility == Visibility.Visible &&
                    ((xamlView != null && xamlView.Visibility == Visibility.Visible && targetView == xamlView) ||
                     (csView != null && csView.Visibility == Visibility.Visible && targetView == csView)))
                {
                    codeBox.Visibility = Visibility.Collapsed;
                    return;
                }

                codeBox.Visibility = Visibility.Visible;
                langLabel.Text = langName;
                currentActiveCode = snippet;
                if (xamlView != null) xamlView.Visibility = (xamlView == targetView) ? Visibility.Visible : Visibility.Collapsed;
                if (csView != null) csView.Visibility = (csView == targetView) ? Visibility.Visible : Visibility.Collapsed;
            }

            if (xamlView != null)
            {
                var btn = CreateCodeActionPill(xamlBtnText);
                btn.Click += (_, _) => ToggleCode(xamlView, xamlBadgeText, xaml!);
                buttons.Children.Add(btn);
            }

            if (csView != null)
            {
                var btn = CreateCodeActionPill(csBtnText);
                btn.Margin = new Thickness(6, 0, 0, 0);
                btn.Click += (_, _) => ToggleCode(csView, csBadgeText, csharp!);
                buttons.Children.Add(btn);
            }

            stack.Children.Add(codeBox);
        }

        card.Child = stack;
        return card;
    }

    private static Button CreateCodeActionPill(string caption)
    {
        var button = new Button
        {
            Content = caption,
            Padding = new Thickness(10, 3, 10, 3),
            FontSize = 11.5,
            Height = 26,
            Cursor = Cursors.Hand
        };
        button.SetResourceReference(FrameworkElement.StyleProperty, "CONCOST.Button.Secondary");
        IconElement.SetKind(button, IconKind.Edit);
        BorderElement.SetCornerRadius(button, new CornerRadius(5));
        return button;
    }

    /// <summary>
    /// Creates a modern callout notice card for engineering hints and notes.
    /// </summary>
    public static Border CreateCalloutCard(string text, IconKind icon = IconKind.Info)
    {
        var callout = new Border
        {
            CornerRadius = new CornerRadius(6),
            BorderThickness = new Thickness(3, 1, 1, 1),
            Padding = new Thickness(14, 10, 14, 10),
            Margin = new Thickness(0, 10, 0, 0)
        };
        callout.SetResourceReference(Border.BackgroundProperty, "CONCOST.Brush.SurfaceSecondary");
        callout.SetResourceReference(Border.BorderBrushProperty, "CONCOST.Brush.Primary");

        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        var ic = new CONCOST.UI.Icons.Icon
        {
            Kind = icon,
            Width = 16,
            Height = 16,
            Margin = new Thickness(0, 1, 10, 0),
            VerticalAlignment = VerticalAlignment.Top
        };
        ic.SetResourceReference(CONCOST.UI.Icons.Icon.ForegroundProperty, "CONCOST.Brush.Primary");
        Grid.SetColumn(ic, 0);
        grid.Children.Add(ic);

        var tb = new TextBlock
        {
            Text = text,
            FontSize = 12,
            LineHeight = 18,
            TextWrapping = TextWrapping.Wrap,
            VerticalAlignment = VerticalAlignment.Center
        };
        tb.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");
        Grid.SetColumn(tb, 1);
        grid.Children.Add(tb);

        callout.Child = grid;
        return callout;
    }
}
