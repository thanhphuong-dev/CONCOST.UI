using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using CONCOST.UI.Controls;

namespace CONCOST.UI.Showcase;

public partial class ShowcaseViewModel
{
    // =========================================================================
    // DEMO: controls ported from modern web UI patterns
    // =========================================================================

    private static Border DemoSection(string title, UIElement content)
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

        var heading = new TextBlock
        {
            Text = title,
            FontSize = 13,
            FontWeight = FontWeights.SemiBold,
            Margin = new Thickness(0, 0, 0, 14)
        };
        heading.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");

        stack.Children.Add(heading);
        stack.Children.Add(content);
        card.Child = stack;
        return card;
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

        var spark = new Sparkline
        {
            Values = new double[] { 42, 48, 45, 61, 58, 72, 69, 84, 91, 88, 97, 104 },
            Height = 38
        };
        inner.Children.Add(spark);

        card.Content = inner;

        var glass = new SpotlightCard { Width = 300, Padding = new Thickness(24) };
        glass.SetResourceReference(FrameworkElement.StyleProperty, "CONCOST.SpotlightCard.Glass");
        glass.BorderGradient = new LinearGradientBrush(
            (Color)ColorConverter.ConvertFromString("#FF7900"),
            (Color)ColorConverter.ConvertFromString("#0284C7"),
            45);
        glass.Content = DemoCaption("Di chuột qua thẻ để thấy vệt sáng và viền gradient.", 0);

        var row = new WrapPanel();
        row.Children.Add(card);
        row.Children.Add(glass);

        root.Children.Add(DemoSection("SpotlightCard — vệt sáng theo chuột + viền gradient", row));
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

        root.Children.Add(DemoSection("OdometerNumber — số tự chạy khi giá trị đổi", content));
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

            var spark = new Sparkline
            {
                Values = values,
                Height = 54,
                Smooth = true,
                ShowArea = area,
                ShowLastDot = true,
                StrokeThickness = 2
            };
            spark.Stroke = stroke;
            cell.Children.Add(spark);

            grid.Children.Add(cell);
        }

        AddCell("Khối lượng theo tuần", new double[] { 12, 18, 15, 24, 22, 31, 29, 38 }, Brushes.SteelBlue, true);
        AddCell("Độ lún quan trắc", new double[] { 8, 7.6, 7.9, 7.2, 6.8, 6.9, 6.1 }, Brushes.SeaGreen, true);
        AddCell("Chênh lệch tiến độ", new double[] { 4, 6, 3, 7, 5, 9, 12, 10, 14 }, Brushes.IndianRed, false);

        root.Children.Add(DemoSection("Sparkline — biểu đồ mini trong dòng", grid));
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

        root.Children.Add(DemoSection("AvatarStack — chồng avatar + chip +N", content));
        return root;
    }
}
