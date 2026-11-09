using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using CONCOST.UI.AttachedProperties;
using CONCOST.UI.Controls;
using CONCOST.UI.Core.Helpers;
using CONCOST.UI.Icons;
using CONCOST.UI.Services;
using CONCOST.UI.Themes;

namespace CONCOST.UI.Showcase;

public partial class ShowcaseViewModel
{
    /// <summary>
    /// Comprehensive Theme &amp; Palettes Showcase:
    /// Demonstrates all 5 core theme palettes (Light, Soft Light, Dark, Revit Light, Revit Dark),
    /// plus System and Auto modes, with real-time switching, token swatches, and plugin guidance.
    /// </summary>
    private object CreateThemeSystemDemo()
    {
        var mainSp = new StackPanel { Margin = new Thickness(0) };

        string T(string vi, string en) =>
            LocalizationManager.Current.Culture.TwoLetterISOLanguageName.Equals("vi", StringComparison.OrdinalIgnoreCase) ? vi : en;

        // ═════════════════════════════════════════════════════════════════════
        // CARD 1: 5 Core Themes & Live Switcher Gallery
        // ═════════════════════════════════════════════════════════════════════
        var galleryGrid = new Grid();
        galleryGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        galleryGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        galleryGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        galleryGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        galleryGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        var themeCards = new List<(AppTheme Theme, string Name, string Subtitle, string Description, string BgHex, string SurfaceHex, string BorderHex, string AccentHex, Border CardBorder, Badge ActiveBadge)>();

        Border CreateThemeCard(AppTheme theme, string name, string subtitle, string desc, string bgHex, string surfaceHex, string borderHex, string accentHex, int col, int row)
        {
            var card = new Border
            {
                BorderThickness = new Thickness(1.5),
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(16, 14, 16, 14),
                Margin = new Thickness(col == 0 ? 0 : 8, row == 0 ? 0 : 12, col == 2 ? 0 : 8, 0),
                Cursor = Cursors.Hand
            };
            card.SetResourceReference(Border.BackgroundProperty, "CONCOST.Brush.Surface");
            card.SetResourceReference(Border.BorderBrushProperty, "CONCOST.Brush.Border");

            var sp = new StackPanel();

            // Top Header: Title + Active Badge
            var topGrid = new Grid { Margin = new Thickness(0, 0, 0, 8) };
            topGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            topGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var titleSp = new StackPanel();
            var titleText = new TextBlock
            {
                Text = name,
                FontSize = 14,
                FontWeight = FontWeights.SemiBold
            };
            titleText.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextPrimary");
            titleSp.Children.Add(titleText);

            var subText = new TextBlock
            {
                Text = subtitle,
                FontSize = 10.5,
                FontWeight = FontWeights.Normal,
                Margin = new Thickness(0, 2, 0, 0)
            };
            subText.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextTertiary");
            titleSp.Children.Add(subText);

            Grid.SetColumn(titleSp, 0);
            topGrid.Children.Add(titleSp);

            var activeBadge = new Badge
            {
                Content = T("Đang chọn", "Active"),
                Appearance = BadgeAppearance.Primary,
                Visibility = ThemeManager.Current.Theme == theme ? Visibility.Visible : Visibility.Collapsed,
                VerticalAlignment = VerticalAlignment.Top
            };
            Grid.SetColumn(activeBadge, 1);
            topGrid.Children.Add(activeBadge);

            sp.Children.Add(topGrid);

            // Palette Swatches Preview Dots
            var swatchesPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 0, 0, 10)
            };

            void AddDot(string colorHex, string label)
            {
                var dotBorder = new Border
                {
                    Width = 20,
                    Height = 20,
                    CornerRadius = new CornerRadius(10),
                    Background = new SolidColorBrush(ColorHelper.FromHex(colorHex)),
                    BorderBrush = new SolidColorBrush(Color.FromArgb(40, 0, 0, 0)),
                    BorderThickness = new Thickness(1),
                    Margin = new Thickness(0, 0, 6, 0),
                    ToolTip = label
                };
                swatchesPanel.Children.Add(dotBorder);
            }

            AddDot(bgHex, $"Background: {bgHex}");
            AddDot(surfaceHex, $"Surface: {surfaceHex}");
            AddDot(borderHex, $"Border: {borderHex}");
            AddDot(accentHex, $"Accent: {accentHex}");

            sp.Children.Add(swatchesPanel);

            // Description
            var descText = new TextBlock
            {
                Text = desc,
                FontSize = 11.5,
                TextWrapping = TextWrapping.Wrap,
                LineHeight = 16,
                Margin = new Thickness(0, 0, 0, 12),
                MinHeight = 48
            };
            descText.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");
            sp.Children.Add(descText);

            // Apply Button
            var applyBtn = new Button
            {
                Content = T("Áp dụng Theme", "Apply Theme"),
                Height = 28,
                FontSize = 11.5,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Cursor = Cursors.Hand
            };
            applyBtn.SetResourceReference(FrameworkElement.StyleProperty, "CONCOST.Button.Secondary");
            BorderElement.SetCornerRadius(applyBtn, new CornerRadius(6));
            applyBtn.Click += (_, _) => ThemeManager.Current.Theme = theme;
            sp.Children.Add(applyBtn);

            card.Child = sp;
            card.MouseLeftButtonUp += (_, _) => ThemeManager.Current.Theme = theme;

            Grid.SetColumn(card, col);
            Grid.SetRow(card, row);
            galleryGrid.Children.Add(card);

            themeCards.Add((theme, name, subtitle, desc, bgHex, surfaceHex, borderHex, accentHex, card, activeBadge));
            return card;
        }

        // 1. Light
        CreateThemeCard(AppTheme.Light, "1. Light", T("Sáng tiêu chuẩn", "Standard Light"),
            T("Giao diện sáng tiêu chuẩn doanh nghiệp với nền trắng sạch sẽ (#FFFFFF), viền xám tinh tế và chữ đen rõ nét.",
              "Clean enterprise light theme with crisp white background (#FFFFFF), subtle borders, and balanced typography."),
            "#FFFFFF", "#F8FAFC", "#E2E8F0", "#FF7900", 0, 0);

        // 2. Soft Light
        CreateThemeCard(AppTheme.SoftLight, "2. Soft Light", T("Màu giấy ấm dịu mắt", "Warm Paper Palette"),
            T("Bảng màu giấy ấm (#FAF8F5 / #F3EFEA) triệt tiêu ánh sáng chói, tối ưu cho kỹ sư làm việc CAD/BIM liên tục.",
              "Warm, low-glare paper palette (#FAF8F5 / #F3EFEA) reducing eye fatigue during marathon CAD/BIM sessions."),
            "#FAF8F5", "#F3EFEA", "#E5DFD7", "#FF7900", 1, 0);

        // 3. Dark
        CreateThemeCard(AppTheme.Dark, "3. Dark", T("Tối than chì hiện đại", "Modern Dark Slate"),
            T("Giao diện tối than chì sâu (#0F172A / #1E293B) với độ tương phản cao, dịu mắt trong không gian làm việc thiếu sáng.",
              "Deep charcoal dark theme (#0F172A / #1E293B) with high-contrast text, ideal for dim studio environments."),
            "#0F172A", "#1E293B", "#334155", "#FF7900", 2, 0);

        // 4. Revit Light
        CreateThemeCard(AppTheme.Revit, "4. Revit Light", T("Bảng màu Autodesk Revit Sáng", "Autodesk Revit Light"),
            T("Bảng màu trích xuất từ Autodesk Revit 2024: dải ribbon xám chuẩn (#F0F0F0) và màu nhấn Revit Cyan (#0696D7).",
              "Native palette extracted from Autodesk Revit 2024: light ribbon bars (#F0F0F0) and official Revit Cyan accent (#0696D7)."),
            "#F0F0F0", "#FFFFFF", "#CCCCCC", "#0696D7", 0, 1);

        // 5. Revit Dark
        CreateThemeCard(AppTheme.RevitDark, "5. Revit Dark", T("Bảng màu Autodesk Revit Tối", "Autodesk Revit Dark"),
            T("Bảng màu chuẩn Autodesk Revit 2024 Dark Mode (#2B2B2B / #383838), hòa hợp hoàn hảo với giao diện Revit mới.",
              "Native palette matching Autodesk Revit 2024 Dark Mode (#2B2B2B / #383838), blending seamlessly into Revit hosts."),
            "#2B2B2B", "#383838", "#4F4F4F", "#0696D7", 1, 1);

        // 6. System
        CreateThemeCard(AppTheme.System, "6. System (OS)", T("Đồng bộ hệ điều hành", "Follow Windows"),
            T("Tự động đọc registry Windows và chuyển đổi theo chế độ Sáng hoặc Tối của hệ thống máy tính người dùng.",
              "Automatically queries Windows registry and dynamically tracks the user's OS Light or Dark mode setting."),
            "#FFFFFF", "#1E293B", "#64748B", "#FF7900", 2, 1);

        // Handler to refresh active status badge on theme change
        void UpdateActiveCard(AppTheme active)
        {
            foreach (var item in themeCards)
            {
                bool isActive = item.Theme == active;
                item.ActiveBadge.Visibility = isActive ? Visibility.Visible : Visibility.Collapsed;
                if (isActive)
                {
                    item.CardBorder.SetResourceReference(Border.BorderBrushProperty, "CONCOST.Brush.Primary");
                }
                else
                {
                    item.CardBorder.SetResourceReference(Border.BorderBrushProperty, "CONCOST.Brush.Border");
                }
            }
        }

        UpdateActiveCard(ThemeManager.Current.Theme);

        void OnThemeManagerChanged(object? s, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ThemeManager.Theme))
            {
                UpdateActiveCard(ThemeManager.Current.Theme);
            }
        }

        ThemeManager.Current.PropertyChanged += OnThemeManagerChanged;
        galleryGrid.Unloaded += (_, _) => ThemeManager.Current.PropertyChanged -= OnThemeManagerChanged;

        string xamlSnippet1 =
@"<!-- Switch themes programmatically in App.xaml or code-behind -->
<!-- 1. All controls react seamlessly via DynamicResource tokens -->
<Border Background=""{DynamicResource CONCOST.Brush.Surface}""
        BorderBrush=""{DynamicResource CONCOST.Brush.Border}""
        BorderThickness=""1"" CornerRadius=""10"">
    <TextBlock Text=""Dynamic Theme Content""
               Foreground=""{DynamicResource CONCOST.Brush.TextPrimary}"" />
</Border>";

        string csSnippet1 =
@"using CONCOST.UI.Themes;

// 1. Switch to any of the 5 core themes:
ThemeManager.Current.Theme = AppTheme.Light;      // Standard Enterprise Light
ThemeManager.Current.Theme = AppTheme.SoftLight;  // Warm Low-Glare Paper
ThemeManager.Current.Theme = AppTheme.Dark;       // Deep Charcoal Dark
ThemeManager.Current.Theme = AppTheme.Revit;      // Autodesk Revit Light
ThemeManager.Current.Theme = AppTheme.RevitDark;  // Autodesk Revit Dark

// 2. Or synchronize automatically with Windows OS:
ThemeManager.Current.Theme = AppTheme.System;

// 3. Customize accent color independently:
ThemeManager.Current.AccentColor = ColorHelper.FromHex(""#0696D7"");";

        mainSp.Children.Add(CreateStandardDemoCard(
            T("Bảng điều khiển 5 bộ Theme & Chuyển đổi trực tiếp", "5 Core Theme Palettes & Live Switcher"),
            galleryGrid,
            xamlSnippet1,
            csSnippet1));

        // ═════════════════════════════════════════════════════════════════════
        // CARD 2: Live Design Token Swatches
        // ═════════════════════════════════════════════════════════════════════
        var swatchesWrap = new WrapPanel { Margin = new Thickness(0) };

        void AddTokenSwatch(string brushKey, string tokenName, string roleDesc)
        {
            var swatchBox = new Border
            {
                Width = 195,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(10),
                Margin = new Thickness(0, 0, 10, 10)
            };
            swatchBox.SetResourceReference(Border.BackgroundProperty, "CONCOST.Brush.Surface");
            swatchBox.SetResourceReference(Border.BorderBrushProperty, "CONCOST.Brush.Border");

            var sp = new StackPanel();

            var colorPill = new Border
            {
                Height = 36,
                CornerRadius = new CornerRadius(6),
                BorderThickness = new Thickness(1),
                Margin = new Thickness(0, 0, 0, 8)
            };
            colorPill.SetResourceReference(Border.BackgroundProperty, brushKey);
            colorPill.SetResourceReference(Border.BorderBrushProperty, "CONCOST.Brush.Border");
            sp.Children.Add(colorPill);

            var nameText = new TextBlock
            {
                Text = tokenName,
                FontSize = 12,
                FontWeight = FontWeights.SemiBold
            };
            nameText.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextPrimary");
            sp.Children.Add(nameText);

            var keyText = new TextBlock
            {
                Text = brushKey,
                FontSize = 9.5,
                Margin = new Thickness(0, 1, 0, 3)
            };
            keyText.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextTertiary");
            sp.Children.Add(keyText);

            var roleText = new TextBlock
            {
                Text = roleDesc,
                FontSize = 10.5,
                TextWrapping = TextWrapping.Wrap
            };
            roleText.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");
            sp.Children.Add(roleText);

            swatchBox.Child = sp;
            swatchesWrap.Children.Add(swatchBox);
        }

        AddTokenSwatch("CONCOST.Brush.Background", "Background", T("Nền cửa sổ chính & canvas", "Main window background & canvas"));
        AddTokenSwatch("CONCOST.Brush.Surface", "Surface (Card)", T("Bề mặt thẻ, panel, dialog", "Card surface, panels, dialogs"));
        AddTokenSwatch("CONCOST.Brush.SurfaceSecondary", "Surface Secondary", T("Bề mặt phụ, header, thanh công cụ", "Secondary surface, headers, toolbars"));
        AddTokenSwatch("CONCOST.Brush.Border", "Border Standard", T("Đường viền chính chuẩn", "Standard element border"));
        AddTokenSwatch("CONCOST.Brush.TextPrimary", "Text Primary", T("Văn bản chính, tiêu đề, số liệu", "Primary body, headings, values"));
        AddTokenSwatch("CONCOST.Brush.TextSecondary", "Text Secondary", T("Văn bản phụ, nhãn, mô tả", "Secondary captions, labels, notes"));
        AddTokenSwatch("CONCOST.Brush.Primary", "Primary Accent", T("Màu nhấn chủ đạo tương tác", "Brand primary interaction accent"));

        string xamlSnippet2 =
@"<!-- Using theme brush tokens in XAML with DynamicResource -->
<TextBlock Text=""Heading"" Foreground=""{DynamicResource CONCOST.Brush.TextPrimary}"" />
<Border Background=""{DynamicResource CONCOST.Brush.SurfaceSecondary}""
        BorderBrush=""{DynamicResource CONCOST.Brush.Border}""
        BorderThickness=""1"" CornerRadius=""6"" />";

        mainSp.Children.Add(CreateStandardDemoCard(
            T("Bảng tra cứu Tokens của Theme hiện tại (Tự đổi màu khi chuyển theme)", "Live Theme Token Swatches (Updates with Theme Switch)"),
            swatchesWrap,
            xamlSnippet2));

        // ═════════════════════════════════════════════════════════════════════
        // CARD 3: Live Component Demonstration in Current Theme
        // ═════════════════════════════════════════════════════════════════════
        var demoStage = new Grid();
        demoStage.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        demoStage.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        var leftDemo = new StackPanel { Margin = new Thickness(0, 0, 16, 0) };

        var btnRow = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 12) };
        var pBtn = new Button { Content = "Primary Action", Margin = new Thickness(0, 0, 8, 0) };
        pBtn.SetResourceReference(FrameworkElement.StyleProperty, "CONCOST.Button.Primary");
        var sBtn = new Button { Content = "Secondary Action", Margin = new Thickness(0, 0, 8, 0) };
        sBtn.SetResourceReference(FrameworkElement.StyleProperty, "CONCOST.Button.Secondary");
        var oBtn = new Button { Content = "Outlined" };
        oBtn.SetResourceReference(FrameworkElement.StyleProperty, "CONCOST.Button.Outlined");
        btnRow.Children.Add(pBtn);
        btnRow.Children.Add(sBtn);
        btnRow.Children.Add(oBtn);
        leftDemo.Children.Add(btnRow);

        var txtBox = new TextBox
        {
            Text = "CONCOST BIM Architectural Studio 2026",
            Margin = new Thickness(0, 0, 0, 12)
        };
        InfoElement.SetPlaceholder(txtBox, "Search model elements...");
        leftDemo.Children.Add(txtBox);

        var checkRow = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 8) };
        var cb1 = new CheckBox { Content = "Snap to Grid", IsChecked = true, Margin = new Thickness(0, 0, 16, 0) };
        var cb2 = new CheckBox { Content = "Ortho Mode (F8)", IsChecked = false };
        checkRow.Children.Add(cb1);
        checkRow.Children.Add(cb2);
        leftDemo.Children.Add(checkRow);

        Grid.SetColumn(leftDemo, 0);
        demoStage.Children.Add(leftDemo);

        var rightDemo = new StackPanel { Margin = new Thickness(16, 0, 0, 0) };

        var badgeRow = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 12) };
        var bg1 = new Badge { Content = "Revit RVT", Appearance = BadgeAppearance.Primary, Margin = new Thickness(0, 0, 6, 0) };
        var bg2 = new Badge { Content = "Active Drawing", Appearance = BadgeAppearance.Success, Margin = new Thickness(0, 0, 6, 0) };
        var bg3 = new Badge { Content = "Pending Calculation", Appearance = BadgeAppearance.Warning, Margin = new Thickness(0, 0, 6, 0) };
        badgeRow.Children.Add(bg1);
        badgeRow.Children.Add(bg2);
        badgeRow.Children.Add(bg3);
        rightDemo.Children.Add(badgeRow);

        var sliderLbl = new TextBlock
        {
            Text = T("Độ trong suốt mô hình (Transparency): 65%", "Model Transparency: 65%"),
            FontSize = 11.5,
            Margin = new Thickness(0, 0, 0, 4)
        };
        sliderLbl.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");
        rightDemo.Children.Add(sliderLbl);

        var slider = new Slider { Minimum = 0, Maximum = 100, Value = 65, Margin = new Thickness(0, 0, 0, 12) };
        slider.ValueChanged += (_, ev) =>
            sliderLbl.Text = $"{T("Độ trong suốt mô hình (Transparency)", "Model Transparency")}: {(int)ev.NewValue}%";
        rightDemo.Children.Add(slider);

        var pBar = new ProgressBar { Value = 65, Height = 6, Maximum = 100 };
        rightDemo.Children.Add(pBar);

        Grid.SetColumn(rightDemo, 1);
        demoStage.Children.Add(rightDemo);

        mainSp.Children.Add(CreateStandardDemoCard(
            T("Thử nghiệm Controls trong Theme hiện tại", "Live Components Preview in Current Theme"),
            demoStage,
            null,
            null));

        // ═════════════════════════════════════════════════════════════════════
        // CARD 4: Architecture & Revit Plugin Scoping Guide
        // ═════════════════════════════════════════════════════════════════════
        var guideSp = new StackPanel();
        var guideText = new TextBlock
        {
            Text = T(
                "CONCOST.UI hỗ trợ 2 mô hình áp dụng Theme chính xác cho từng loại ứng dụng:\n\n" +
                "1. Ứng dụng Desktop Độc lập (Standalone WPF): Ứng dụng sở hữu Application.Current. Đổi theme toàn cục bằng ThemeManager.Current.Theme = AppTheme.RevitDark.\n" +
                "2. Plugin CAD/BIM (Autodesk Revit, AutoCAD): Plugin chạy bên trong tiến trình của Revit và KHÔNG sở hữu Application.Current. Sử dụng ThemeManager.Current.ApplyTheme(pluginRootElement, AppTheme.Auto) để gắn ResourceDictionary cục bộ vào UserControl của plugin, tự động nhận diện nền cửa sổ Revit và không can thiệp vào tài nguyên của host.",
                "CONCOST.UI supports two distinct theme architectures tailored for enterprise deployment:\n\n" +
                "1. Standalone WPF Applications: The app owns Application.Current. Change global themes via ThemeManager.Current.Theme = AppTheme.RevitDark.\n" +
                "2. CAD/BIM Plugins (Autodesk Revit, AutoCAD): Plugins run inside the Autodesk host process without Application.Current. Call ThemeManager.Current.ApplyTheme(pluginRootElement, AppTheme.Auto) to scope resources strictly to your plugin UserControl, automatically detecting Revit's host window theme without polluting host resources."),
            FontSize = 12.5,
            LineHeight = 20,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 0, 0, 10)
        };
        guideText.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");
        guideSp.Children.Add(guideText);

        string csSnippet4 =
@"using System.Windows;
using System.Windows.Controls;
using CONCOST.UI.Themes;

public partial class RevitDockablePanel : UserControl
{
    public RevitDockablePanel()
    {
        InitializeComponent();

        // Mode 1: Auto-detect Autodesk Revit host window theme (Light or Dark)
        ThemeManager.Current.ApplyTheme(this, AppTheme.Auto);

        // Mode 2: Or explicitly enforce Revit Dark palette for this panel:
        // ThemeManager.Current.ApplyTheme(this, AppTheme.RevitDark);
    }
}";

        mainSp.Children.Add(CreateStandardDemoCard(
            T("Hướng dẫn Kiến trúc Theme cho Revit / AutoCAD Plugin", "Theme Architecture & Plugin Scoping Guide"),
            guideSp,
            null,
            csSnippet4));

        return mainSp;
    }
}
