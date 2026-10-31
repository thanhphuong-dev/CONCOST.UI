using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using CONCOST.UI.Controls;
using CONCOST.UI.Core.MVVM;
using CONCOST.UI.Icons;
using CONCOST.UI.Services;

namespace CONCOST.UI.Showcase;

public partial class ShowcaseViewModel
{
    // =========================================================================
    // DEMO: Phase 3 — command palette, particle field, radial gauge, page transition
    // =========================================================================

    private object CreateCommandPaletteDemo()
    {
        var host = new Grid { Height = 540 };

        var backdrop = new Border { CornerRadius = new CornerRadius(12) };
        backdrop.SetResourceReference(Border.BackgroundProperty, "CONCOST.Brush.SurfaceSecondary");
        host.Children.Add(backdrop);

        var palette = new CommandPalette
        {
            Placeholder = "Tìm lệnh, màn hình, thiết lập…",
            EmptyText = "Không có lệnh nào khớp.",
            MaxListHeight = 276
        };

        palette.ItemsSource = new List<CommandPaletteItem>
        {
            new("Tổng quan dự án", "Bảng điều khiển KPI và tiến độ", new RelayCommand(() => ToastService.Success("Mở tổng quan dự án")), IconKind.ChartLine, "Ctrl+1", "Điều hướng"),
            new("Bảng khối lượng", "Bóc tách khối lượng theo cấu kiện", new RelayCommand(() => ToastService.Success("Mở bảng khối lượng")), IconKind.File, "Ctrl+2", "Điều hướng"),
            new("Cấu hình thép", "Đường kính, mác thép, lớp bảo vệ", new RelayCommand(() => ToastService.Success("Mở cấu hình thép")), IconKind.Settings, "Ctrl+3", "Điều hướng"),
            new("Xuất PDF", "Xuất bản vẽ hiện tại ra PDF", new RelayCommand(() => ToastService.Success("Đang xuất PDF…")), IconKind.File, "Ctrl+P", "Xuất bản"),
            new("Xuất Excel", "Xuất bảng khối lượng ra Excel", new RelayCommand(() => ToastService.Success("Đang xuất Excel…")), IconKind.File, "Ctrl+E", "Xuất bản"),
            new("Đổi theme", "Chuyển giữa sáng và tối", new RelayCommand(() => ToastService.Info("Đổi theme")), IconKind.LightMode, "Ctrl+D", "Hiển thị"),
            new("Bật/tắt hiệu ứng", "Tắt chuyển cảnh khi máy yếu", new RelayCommand(() => ToastService.Info("Đã tắt hiệu ứng")), IconKind.LightMode, null, "Hiển thị"),
            new("Cài đặt", "Ngôn ngữ, đơn vị, đường dẫn", new RelayCommand(() => ToastService.Info("Mở cài đặt")), IconKind.Settings, "Ctrl+,", "Hệ thống")
        };

        palette.ItemInvoked += (_, e) => ToastService.Success($"Đã chạy: {e.Item.Title}");

        var content = new StackPanel
        {
            Margin = new Thickness(28),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        var title = new TextBlock
        {
            Text = "Bảng lệnh",
            FontSize = 18,
            FontWeight = FontWeights.SemiBold,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        title.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextPrimary");
        content.Children.Add(title);

        var hint = new TextBlock
        {
            Text = "Gõ không dấu vẫn tìm được: “tong quan” khớp “Tổng quan”.",
            FontSize = 11.5,
            Margin = new Thickness(0, 6, 0, 18),
            HorizontalAlignment = HorizontalAlignment.Center
        };
        hint.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");
        content.Children.Add(hint);

        var openButton = new Button
        {
            Content = "Mở bảng lệnh",
            Padding = new Thickness(18, 8, 18, 8),
            HorizontalAlignment = HorizontalAlignment.Center
        };
        openButton.SetResourceReference(FrameworkElement.StyleProperty, "CONCOST.Button.Primary");
        openButton.Click += (_, _) => palette.Open();
        content.Children.Add(openButton);

        var keys = new TextBlock
        {
            Text = "↑ ↓ chọn   ·   ↵ chạy   ·   esc đóng",
            FontSize = 10.5,
            Margin = new Thickness(0, 14, 0, 0),
            HorizontalAlignment = HorizontalAlignment.Center
        };
        keys.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextTertiary");
        content.Children.Add(keys);

        host.Children.Add(content);
        host.Children.Add(palette);

        var root = new StackPanel();
        root.Children.Add(DemoSection(
            "Ctrl+K — gõ để lọc, ↑↓ để chọn",
            host,
            xaml: "<controls:CommandPalette x:Name=\"Palette\"\n" +
                  "    IsOpen=\"{Binding IsPaletteOpen}\"\n" +
                  "    ItemsSource=\"{Binding Commands}\"\n" +
                  "    Placeholder=\"Tìm lệnh…\"\n" +
                  "    ItemInvoked=\"OnCommandInvoked\" />",
            csharp: "// Mỗi dòng là một CommandPaletteItem: tiêu đề, mô tả, lệnh, icon, phím tắt, nhóm\n" +
                    "palette.ItemsSource = new[]\n" +
                    "{\n" +
                    "    new CommandPaletteItem(\"Xuất PDF\", \"Xuất bản vẽ ra PDF\",\n" +
                    "        ExportPdfCommand, IconKind.File, \"Ctrl+P\", \"Xuất bản\")\n" +
                    "};\n\n" +
                    "palette.Open();   // hoặc gán IsOpen = true\n\n" +
                    "// Tìm kiếm bỏ dấu, nhiều từ khoá độc lập:\n" +
                    "// \"xuat pdf\" khớp \"Xuất PDF\", \"tong quan\" khớp \"Tổng quan\""));

        return root;
    }

    private object CreateParticleFieldDemo()
    {
        var row = new Grid();
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(14) });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        var left = BuildParticleCard("Hạt trôi", 70, 0);
        Grid.SetColumn(left, 0);
        row.Children.Add(left);

        var right = BuildParticleCard("Chòm sao", 40, 96);
        Grid.SetColumn(right, 2);
        row.Children.Add(right);

        var root = new StackPanel();
        root.Children.Add(DemoSection(
            "Hạt nền cho hero panel và empty state",
            row,
            xaml: "<controls:ParticleField ParticleCount=\"70\"\n" +
                  "    Speed=\"18\" Direction=\"270\" Spread=\"0.28\"\n" +
                  "    ParticleOpacity=\"0.75\"\n" +
                  "    ConnectDistance=\"0\" />",
            csharp: "// ConnectDistance > 0 nối các hạt gần nhau thành chòm sao\n" +
                    "var field = new ParticleField\n" +
                    "{\n" +
                    "    ParticleCount = 40,\n" +
                    "    ConnectDistance = 96,\n" +
                    "    Speed = 18\n" +
                    "};\n\n" +
                    "// Tôn trọng thiết lập giảm chuyển động của Windows;\n" +
                    "// Seed cố định để ảnh chụp luôn giống nhau"));

        return root;
    }

    private Border BuildParticleCard(string caption, int count, double connect)
    {
        var field = new ParticleField
        {
            ParticleCount = count,
            ConnectDistance = connect,
            Speed = 16,
            Spread = 0.3,
            ParticleOpacity = 0.9
        };

        var stack = new StackPanel();
        stack.Children.Add(new Border
        {
            Height = 170,
            CornerRadius = new CornerRadius(10),
            ClipToBounds = true,
            Child = field
        });

        var label = new TextBlock
        {
            Text = caption,
            FontSize = 11,
            Margin = new Thickness(2, 10, 0, 0)
        };
        label.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");
        stack.Children.Add(label);

        var card = new Border
        {
            Padding = new Thickness(12),
            CornerRadius = new CornerRadius(12),
            BorderThickness = new Thickness(1),
            Child = stack
        };
        card.SetResourceReference(Border.BackgroundProperty, "CONCOST.Brush.SurfaceSecondary");
        card.SetResourceReference(Border.BorderBrushProperty, "CONCOST.Brush.Border");

        // Give the field something to draw against.
        ((Border)stack.Children[0]).SetResourceReference(Border.BackgroundProperty, "CONCOST.Brush.Surface");

        return card;
    }

    private object CreateRadialGaugeDemo()
    {
        var primary = new RadialGauge
        {
            Value = 76,
            Label = "Tiến độ",
            Suffix = "%",
            TrackThickness = 12
        };

        var withTicks = new RadialGauge
        {
            Value = 42,
            Maximum = 100,
            Label = "Khối lượng",
            Suffix = "%",
            ShowTicks = true,
            TickCount = 20,
            TrackThickness = 10,
            StartAngle = 150,
            SweepAngle = 240
        };
        withTicks.SetResourceReference(RadialGauge.ValueBrushProperty, "CONCOST.Brush.Info");

        var load = new RadialGauge
        {
            Value = 186.4,
            Minimum = 0,
            Maximum = 250,
            Label = "Tải trọng",
            Suffix = " kN",
            ValueFormat = "0.0",
            TrackThickness = 9,
            ShowTicks = true,
            TickCount = 10
        };
        load.SetResourceReference(RadialGauge.ValueBrushProperty, "CONCOST.Brush.Success");

        var gauges = new UniformGrid { Columns = 3, Height = 190 };
        gauges.Children.Add(primary);
        gauges.Children.Add(withTicks);
        gauges.Children.Add(load);

        var slider = new Slider
        {
            Minimum = 0,
            Maximum = 100,
            TickFrequency = 1,
            Margin = new Thickness(0, 10, 0, 0)
        };
        slider.SetBinding(RangeBase.ValueProperty, new Binding(nameof(RadialGauge.Value))
        {
            Source = primary,
            Mode = BindingMode.TwoWay
        });

        var stack = new StackPanel();
        stack.Children.Add(gauges);
        stack.Children.Add(slider);

        var root = new StackPanel();
        root.Children.Add(DemoSection(
            "Vòng cung, tick mark và số ở giữa",
            stack,
            xaml: "<controls:RadialGauge Value=\"76\" Label=\"Tiến độ\" Suffix=\"%\"\n" +
                  "    StartAngle=\"135\" SweepAngle=\"270\" TrackThickness=\"12\" />\n\n" +
                  "<controls:RadialGauge Value=\"186.4\" Maximum=\"250\"\n" +
                  "    ValueFormat=\"0.0\" Suffix=\" kN\"\n" +
                  "    ShowTicks=\"True\" TickCount=\"10\" />",
            csharp: "// Value tự kẹp vào [Minimum, Maximum] và chạy easing khi đổi\n" +
                    "gauge.Value = 76;\n\n" +
                    "// Bật tick mark ngoài vòng cung\n" +
                    "gauge.ShowTicks = true;\n" +
                    "gauge.TickCount = 10;\n\n" +
                    "// Tắt animation: gauge.IsAnimated = false"));

        return root;
    }

    private object CreatePageTransitionDemo()
    {
        var pages = new List<(string Title, string Body, string Hex)>
        {
            ("Trang 1 — Tổng quan", "KPI, tiến độ và cảnh báo của dự án.", "#FF7900"),
            ("Trang 2 — Khối lượng", "Bảng bóc tách theo cấu kiện và tầng.", "#2563EB"),
            ("Trang 3 — Cài đặt", "Ngôn ngữ, đơn vị đo và đường dẫn mẫu.", "#0E9F6E")
        };

        int index = 0;

        var host = new PageTransitionHost { Height = 190, Transition = PageTransition.SlideLeft };

        void Render()
        {
            var page = pages[index];
            var card = new Border
            {
                CornerRadius = new CornerRadius(12),
                Padding = new Thickness(24)
            };
            card.SetResourceReference(Border.BackgroundProperty, "CONCOST.Brush.SurfaceSecondary");

            var inner = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
            var stripe = new Border
            {
                Width = 46,
                Height = 4,
                CornerRadius = new CornerRadius(2),
                Background = (Brush)new BrushConverter().ConvertFromString(page.Hex)!,
                Margin = new Thickness(0, 0, 0, 12),
                HorizontalAlignment = HorizontalAlignment.Left
            };
            inner.Children.Add(stripe);

            var title = new TextBlock { Text = page.Title, FontSize = 16, FontWeight = FontWeights.SemiBold };
            title.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextPrimary");
            inner.Children.Add(title);

            var body = new TextBlock { Text = page.Body, FontSize = 12, Margin = new Thickness(0, 6, 0, 0) };
            body.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");
            inner.Children.Add(body);

            card.Child = inner;
            host.Content = card;
        }

        var previous = new Button { Content = "‹ Trước", Padding = new Thickness(14, 6, 14, 6) };
        previous.SetResourceReference(FrameworkElement.StyleProperty, "CONCOST.Button.Secondary");
        previous.Click += (_, _) =>
        {
            index = (index + pages.Count - 1) % pages.Count;
            host.Transition = PageTransition.SlideRight;
            Render();
        };

        var next = new Button { Content = "Sau ›", Padding = new Thickness(14, 6, 14, 6), Margin = new Thickness(8, 0, 0, 0) };
        next.SetResourceReference(FrameworkElement.StyleProperty, "CONCOST.Button.Primary");
        next.Click += (_, _) =>
        {
            index = (index + 1) % pages.Count;
            host.Transition = PageTransition.SlideLeft;
            Render();
        };

        var modes = new ComboBox { Width = 170, Margin = new Thickness(16, 0, 0, 0) };
        modes.SetResourceReference(FrameworkElement.StyleProperty, "CONCOST.ComboBox.Base");
        modes.Items.Add("Fade");
        modes.Items.Add("Slide ngang");
        modes.Items.Add("Slide dọc");
        modes.Items.Add("Scale");
        modes.SelectedIndex = 0;
        modes.SelectionChanged += (_, _) =>
        {
            host.Transition = modes.SelectedIndex switch
            {
                0 => PageTransition.Fade,
                1 => PageTransition.SlideLeft,
                2 => PageTransition.SlideUp,
                _ => PageTransition.Scale
            };
            host.Play();
        };

        var bar = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 14, 0, 0) };
        bar.Children.Add(previous);
        bar.Children.Add(next);
        bar.Children.Add(modes);

        var stack = new StackPanel();
        stack.Children.Add(host);
        stack.Children.Add(bar);

        Render();

        var root = new StackPanel();
        root.Children.Add(DemoSection(
            "Đổi nội dung là tự chạy hiệu ứng",
            stack,
            xaml: "<controls:PageTransitionHost Transition=\"SlideLeft\"\n" +
                  "    Duration=\"0:0:0.26\">\n" +
                  "    <ContentControl Content=\"{Binding CurrentPage}\" />\n" +
                  "</controls:PageTransitionHost>",
            csharp: "// Gán Content — hiệu ứng tự chạy\n" +
                    "host.Transition = PageTransition.SlideLeft;\n" +
                    "host.Content = nextPage;\n\n" +
                    "// Hoặc phát lại cho nội dung hiện tại\n" +
                    "host.Play();\n\n" +
                    "// Tắt: IsTransitionEnabled = false"));

        return root;
    }
}
