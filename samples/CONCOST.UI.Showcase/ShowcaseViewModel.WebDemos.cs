using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using CONCOST.UI.AttachedProperties;
using CONCOST.UI.Controls;
using CONCOST.UI.Icons;

namespace CONCOST.UI.Showcase;

public partial class ShowcaseViewModel
{
    // =========================================================================
    // DEMO: controls ported from modern web UI patterns
    // =========================================================================

    private Border DemoSection(string title, UIElement content, string? xaml = null, string? csharp = null) => CreateStandardDemoCard(title, content, xaml, csharp);
    private Border DemoSectionLegacy(string title, UIElement content, string? xaml = null, string? csharp = null)
    {
        var card = new Border
        {
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(10),
            Padding = new Thickness(20),
            Margin = new Thickness(0, 0, 0, 18)
        };
        card.SetResourceReference(Border.BackgroundProperty, "CONCOST.Brush.Surface");
        card.SetResourceReference(Border.BorderBrushProperty, "CONCOST.Brush.Border");

        var stack = new StackPanel();

        var header = new Grid { Margin = new Thickness(0, 0, 0, 14) };
        header.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        header.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        var heading = new TextBlock
        {
            Text = title,
            FontSize = 13,
            FontWeight = FontWeights.SemiBold,
            VerticalAlignment = VerticalAlignment.Center
        };
        heading.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");
        Grid.SetColumn(heading, 0);
        header.Children.Add(heading);

        var buttons = new StackPanel { Orientation = Orientation.Horizontal };
        Grid.SetColumn(buttons, 1);
        header.Children.Add(buttons);

        stack.Children.Add(header);
        stack.Children.Add(content);

        if (!string.IsNullOrEmpty(xaml) || !string.IsNullOrEmpty(csharp))
        {
            FrameworkElement? xamlView = string.IsNullOrEmpty(xaml) ? null : CreateXamlSyntaxHighlighter(xaml!);
            FrameworkElement? csView = string.IsNullOrEmpty(csharp) ? null : CreatePlainCodeView(csharp!);

            var codeBox = new Border
            {
                CornerRadius = new CornerRadius(8),
                BorderThickness = new Thickness(1),
                Margin = new Thickness(0, 16, 0, 0),
                Visibility = Visibility.Collapsed
            };
            codeBox.SetResourceReference(Border.BackgroundProperty, "CONCOST.Brush.SurfaceSecondary");
            codeBox.SetResourceReference(Border.BorderBrushProperty, "CONCOST.Brush.Border");

            var codeInner = new StackPanel();
            if (xamlView != null) codeInner.Children.Add(xamlView);
            if (csView != null)
            {
                if (xamlView != null) csView.Visibility = Visibility.Collapsed;
                codeInner.Children.Add(csView);
            }
            codeBox.Child = codeInner;

            void Show(FrameworkElement? view)
            {
                codeBox.Visibility = Visibility.Visible;
                if (xamlView != null) xamlView.Visibility = xamlView == view ? Visibility.Visible : Visibility.Collapsed;
                if (csView != null) csView.Visibility = csView == view ? Visibility.Visible : Visibility.Collapsed;
            }

            if (xamlView != null)
            {
                var btn = CreateCodeButton("View XAML");
                btn.Click += (_, _) => Show(xamlView);
                buttons.Children.Add(btn);
            }

            if (csView != null)
            {
                var btn = CreateCodeButton("View C#");
                btn.Margin = new Thickness(6, 0, 0, 0);
                btn.Click += (_, _) => Show(csView);
                buttons.Children.Add(btn);
            }

            stack.Children.Add(codeBox);
        }

        card.Child = stack;
        return card;
    }

    private static Button CreateCodeButton(string caption)
    {
        var button = new Button
        {
            Content = caption,
            Padding = new Thickness(10, 4, 10, 4),
            FontSize = 12,
            Cursor = Cursors.Hand
        };
        button.SetResourceReference(FrameworkElement.StyleProperty, "CONCOST.Button.Secondary");
        IconElement.SetKind(button, IconKind.Edit);
        BorderElement.SetCornerRadius(button, new CornerRadius(6));
        return button;
    }

    private static FrameworkElement CreatePlainCodeView(string code)
    {
        var text = new TextBlock
        {
            Text = code.Trim(),
            FontFamily = new FontFamily("Consolas, Courier New"),
            FontSize = 12,
            LineHeight = 19,
            Padding = new Thickness(14, 12, 14, 12),
            TextWrapping = TextWrapping.Wrap
        };
        text.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextPrimary");
        return text;
    }

    private static TextBlock DemoCaption(string text, double top = 0)
    {
        var block = new TextBlock
        {
            Text = text,
            FontSize = 12,
            Margin = new Thickness(0, top, 0, 6),
            TextWrapping = TextWrapping.Wrap
        };
        block.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");
        return block;
    }

    private object CreateSpotlightCardDemo()
    {
        var root = new StackPanel();

        var card = new SpotlightCard { Width = 300, Margin = new Thickness(0, 0, 0, 14) };
        card.SetResourceReference(FrameworkElement.StyleProperty, "CONCOST.SpotlightCard.Base");

        var inner = new StackPanel();
        inner.Children.Add(DemoCaption("Tổng khối lượng bê tông"));

        var number = new OdometerNumber { Value = 1284.6, Format = "N1", Suffix = " m³", Margin = new Thickness(0, 2, 0, 8) };
        number.SetResourceReference(FrameworkElement.StyleProperty, "CONCOST.OdometerNumber.Hero");
        inner.Children.Add(number);

        inner.Children.Add(new Sparkline
        {
            Values = new double[] { 42, 48, 45, 61, 58, 72, 69, 84, 91, 88, 97, 104 },
            Height = 38
        });

        card.Content = inner;

        var glass = new SpotlightCard { Width = 300, Padding = new Thickness(24) };
        glass.SetResourceReference(FrameworkElement.StyleProperty, "CONCOST.SpotlightCard.Glass");
        glass.BorderGradient = new LinearGradientBrush(
            (Color)ColorConverter.ConvertFromString("#FF7900"),
            (Color)ColorConverter.ConvertFromString("#0284C7"),
            45);
        glass.Content = DemoCaption("Di chuột qua thẻ để thấy vệt sáng và viền gradient.");

        var row = new WrapPanel();
        row.Children.Add(card);
        row.Children.Add(glass);

        root.Children.Add(DemoSection(
            "Mặc định",
            row,
            xaml: "<ui:SpotlightCard Width=\"300\" Style=\"{DynamicResource CONCOST.SpotlightCard.Base}\">\n" +
                  "    <StackPanel>\n" +
                  "        <TextBlock Text=\"Tổng khối lượng bê tông\" Style=\"{DynamicResource CONCOST.TextBlock.Caption}\" />\n" +
                  "        <ui:OdometerNumber Value=\"1284.6\" Format=\"N1\" Suffix=\" m³\"\n" +
                  "                           Style=\"{DynamicResource CONCOST.OdometerNumber.Hero}\" />\n" +
                  "        <ui:Sparkline Values=\"{Binding ConcreteTrend}\" Height=\"38\" />\n" +
                  "    </StackPanel>\n" +
                  "</ui:SpotlightCard>",
            csharp: "// Viền gradient khi hover\n" +
                    "card.BorderGradient = new LinearGradientBrush(\n" +
                    "    Color.FromRgb(0xFF, 0x79, 0x00),\n" +
                    "    Color.FromRgb(0x02, 0x84, 0xC7), 45);\n\n" +
                    "// Tắt hiệu ứng trên danh sách dài để giữ 60 FPS\n" +
                    "foreach (var item in ItemsPanel.Children.OfType<SpotlightCard>())\n" +
                    "    item.IsSpotlightEnabled = false;"));

        return root;
    }

    private object CreateOdometerNumberDemo()
    {
        var root = new StackPanel();
        var row = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 14) };

        var hero = new OdometerNumber { Value = 1284.6, Format = "N1", Suffix = " m³", Margin = new Thickness(0, 0, 32, 0) };
        hero.SetResourceReference(FrameworkElement.StyleProperty, "CONCOST.OdometerNumber.Hero");

        var mid = new OdometerNumber { Value = 48620, Format = "N0", Suffix = " kg", Margin = new Thickness(0, 0, 32, 0) };
        mid.SetResourceReference(FrameworkElement.StyleProperty, "CONCOST.OdometerNumber.Base");

        var compact = new OdometerNumber { Value = 92.4, Format = "N1", Suffix = " %" };
        compact.SetResourceReference(FrameworkElement.StyleProperty, "CONCOST.OdometerNumber.Compact");

        row.Children.Add(hero);
        row.Children.Add(mid);
        row.Children.Add(compact);

        var run = new Button { Content = "Chạy lại số liệu", Padding = new Thickness(12, 6, 12, 6), HorizontalAlignment = HorizontalAlignment.Left };
        run.SetResourceReference(FrameworkElement.StyleProperty, "CONCOST.Button.Secondary");
        run.Click += (_, _) =>
        {
            var random = new Random();
            hero.Value = Math.Round(900 + random.NextDouble() * 900, 1);
            mid.Value = random.Next(20000, 90000);
            compact.Value = Math.Round(60 + random.NextDouble() * 38, 1);
        };

        var content = new StackPanel();
        content.Children.Add(row);
        content.Children.Add(run);

        root.Children.Add(DemoSection(
            "Ba kích thước",
            content,
            xaml: "<ui:OdometerNumber Value=\"{Binding ConcreteM3}\" Format=\"N1\" Suffix=\" m³\"\n" +
                  "                   Style=\"{DynamicResource CONCOST.OdometerNumber.Hero}\" />\n" +
                  "<ui:OdometerNumber Value=\"{Binding SteelKg}\" Format=\"N0\" Suffix=\" kg\"\n" +
                  "                   Prefix=\"+\" Duration=\"0:0:1.2\" />",
            csharp: "// Cập nhật giá trị — control tự chạy từ số cũ sang số mới\n" +
                    "ConcreteM3 = summary.ConcreteM3;\n\n" +
                    "// Hiện ngay, không chạy animation\n" +
                    "SteelKgNumber.Duration = new Duration(TimeSpan.Zero);\n\n" +
                    "// Bỏ animation cho lần load đầu\n" +
                    "SteelKgNumber.AnimateOnLoad = false;"));

        return root;
    }

    private object CreateSparklineDemo()
    {
        var root = new StackPanel();
        var grid = new UniformGrid { Columns = 3, Margin = new Thickness(0, 0, 0, 4) };

        void AddCell(string caption, double[] values, Brush stroke, bool area)
        {
            var cell = new StackPanel { Margin = new Thickness(0, 0, 18, 0) };
            cell.Children.Add(DemoCaption(caption));

            cell.Children.Add(new Sparkline
            {
                Values = values,
                Height = 54,
                Smooth = true,
                ShowArea = area,
                ShowLastDot = true,
                StrokeThickness = 2,
                Stroke = stroke
            });

            grid.Children.Add(cell);
        }

        AddCell("Khối lượng theo tuần", new double[] { 12, 18, 15, 24, 22, 31, 29, 38 }, Brushes.SteelBlue, true);
        AddCell("Độ lún quan trắc", new double[] { 8, 7.6, 7.9, 7.2, 6.8, 6.9, 6.1 }, Brushes.SeaGreen, true);
        AddCell("Chênh lệch tiến độ", new double[] { 4, 6, 3, 7, 5, 9, 12, 10, 14 }, Brushes.IndianRed, false);

        root.Children.Add(DemoSection(
            "Ba kiểu",
            grid,
            xaml: "<ui:Sparkline Values=\"{Binding Last7Days}\" Height=\"28\"\n" +
                  "              Smooth=\"True\" ShowArea=\"True\" ShowLastDot=\"True\"\n" +
                  "              Stroke=\"{DynamicResource CONCOST.Brush.Primary}\" />",
            csharp: "var spark = new Sparkline\n" +
                    "{\n" +
                    "    Values = measurements.Select(m => m.SettlementMm).ToList(),\n" +
                    "    Smooth = true,\n" +
                    "    ShowArea = true,\n" +
                    "    StrokeThickness = 1.8,\n" +
                    "    Stroke = Brushes.SeaGreen\n" +
                    "};"));

        return root;
    }

    private object CreateAvatarStackDemo()
    {
        var root = new StackPanel();

        var reviewers = new AvatarStack
        {
            Items = new[] { "Nguyen Van A", "Tran Thi B", "Le C", "Pham D", "Hoang E", "Vu G" },
            MaxVisible = 4,
            Size = 36,
            Overlap = 12,
            Margin = new Thickness(0, 0, 0, 16)
        };

        var compact = new AvatarStack
        {
            Items = new[] { "Nguyen Van A", "Tran Thi B", "Le C" },
            MaxVisible = 3,
            Size = 24,
            Overlap = 8
        };

        var content = new StackPanel();
        content.Children.Add(DemoCaption("Nhóm kiểm tra — 6 người, hiện 4 + chip +2"));
        content.Children.Add(reviewers);
        content.Children.Add(DemoCaption("Kích thước nhỏ cho bảng dữ liệu", 8));
        content.Children.Add(compact);

        root.Children.Add(DemoSection(
            "Mặc định",
            content,
            xaml: "<ui:AvatarStack Items=\"{Binding Reviewers}\" MaxVisible=\"4\" Size=\"36\" Overlap=\"12\" />",
            csharp: "// Chuỗi → tự lấy chữ cái đầu (\"Nguyen Van A\" → \"NA\")\n" +
                    "Reviewers = new[] { \"Nguyen Van A\", \"Tran Thi B\", \"Le C\", \"Pham D\", \"Hoang E\" };\n\n" +
                    "// Ảnh cũng dùng được\n" +
                    "stack.Items = new object[] { new BitmapImage(new Uri(\"pack://application:,,,/Avatars/lead.png\")), \"Tran Thi B\" };"));

        return root;
    }

    private object CreateHeatmapCalendarDemo()
    {
        var random = new Random(7);
        var values = new List<int>();
        for (int i = 0; i < 182; i++) values.Add(random.Next(0, 10) < 3 ? 0 : random.Next(1, 12));

        var heatmap = new HeatmapCalendar
        {
            Values = values,
            StartDate = DateTime.Today.AddDays(-181),
            CellSize = 13,
            CellGap = 3,
            Margin = new Thickness(0, 0, 0, 8)
        };

        var legend = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
        legend.Children.Add(DemoCaption("Ít", 0));
        foreach (double strength in new[] { 0.0, 0.25, 0.45, 0.70, 1.0 })
        {
            var swatch = new Border
            {
                Width = 12,
                Height = 12,
                CornerRadius = new CornerRadius(2.5),
                Margin = new Thickness(4, 0, 4, 0)
            };
            byte alpha = (byte)Math.Round(strength == 0 ? 0 : 40 + 215 * strength);
            swatch.Background = strength == 0
                ? new SolidColorBrush(Color.FromRgb(0xEE, 0xF1, 0xF5))
                : new SolidColorBrush(Color.FromArgb(alpha, 0xFF, 0x79, 0x00));
            legend.Children.Add(swatch);
        }
        legend.Children.Add(DemoCaption("Nhiều", 0));

        var content = new StackPanel();
        content.Children.Add(DemoCaption("Số lượt đổ bê tông theo ngày (182 ngày gần nhất)"));
        content.Children.Add(heatmap);
        content.Children.Add(legend);

        var root = new StackPanel();
        root.Children.Add(DemoSection(
            "Mặc định",
            content,
            xaml: "<ui:HeatmapCalendar Values=\"{Binding PourCounts}\"\n" +
                  "                    StartDate=\"{Binding StartDate}\"\n" +
                  "                    CellSize=\"13\" CellGap=\"3\" />",
            csharp: "// Mỗi phần tử là số lượng của một ngày, bắt đầu từ StartDate\n" +
                    "var counts = pours\n" +
                    "    .GroupBy(p => p.Date.Date)\n" +
                    "    .Select(g => g.Count())\n" +
                    "    .ToList();\n\n" +
                    "var heatmap = new HeatmapCalendar\n" +
                    "{\n" +
                    "    Values = counts,\n" +
                    "    StartDate = DateTime.Today.AddDays(-counts.Count + 1),\n" +
                    "    CellSize = 13,\n" +
                    "    CellGap = 3\n" +
                    "};"));

        return root;
    }

    private object CreateMarqueeTickerDemo()
    {
        var ticker = new MarqueeTicker
        {
            Text = "Công trường A — đổ bê tông sàn tầng 12 · 3 mũi thi công đang hoạt động · cảnh báo gió mạnh sau 15:00 · kiểm tra nghiệm thu lúc 16:30",
            Speed = 55,
            Height = 30
        };

        var host = new Border
        {
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(12, 0, 12, 0),
            BorderThickness = new Thickness(1)
        };
        host.SetResourceReference(Border.BackgroundProperty, "CONCOST.Brush.SurfaceSecondary");
        host.SetResourceReference(Border.BorderBrushProperty, "CONCOST.Brush.Border");
        host.Child = ticker;

        var root = new StackPanel();
        root.Children.Add(DemoSection(
            "Mặc định",
            host,
            xaml: "<ui:MarqueeTicker Text=\"{Binding SiteNotice}\" Speed=\"55\" Height=\"30\" />",
            csharp: "var ticker = new MarqueeTicker\n" +
                    "{\n" +
                    "    Text = \"Công trường A — đổ bê tông sàn tầng 12 · 3 mũi thi công đang hoạt động\",\n" +
                    "    Speed = 55,      // px / giây\n" +
                    "    Separator = \"  •  \"\n" +
                    "};\n\n" +
                    "// Tạm dừng khi cần\n" +
                    "ticker.IsRunning = false;"));

        return root;
    }
}
