using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using CONCOST.UI.Controls;
using CONCOST.UI.Core;
using CONCOST.UI.Themes;

namespace CONCOST.UI.Showcase;

public partial class ShowcaseViewModel
{
    // =========================================================================
    // DEMO: Phase 0 — cursor glow, grid pattern, shake, brutal preset
    // =========================================================================

    private object CreateCursorGlowDemo()
    {
        var host = new Grid { Height = 170, ClipToBounds = true };
        host.SetResourceReference(Panel.BackgroundProperty, "CONCOST.Brush.TextPrimary");

        var label = new TextBlock
        {
            Text = "Di chuột trong khung này",
            Foreground = Brushes.White,
            FontSize = 13,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        host.Children.Add(label);
        host.Children.Add(new CursorGlow { GlowSize = 240, GlowOpacity = 0.42 });

        var root = new StackPanel();
        root.Children.Add(DemoSection(
            "Mặc định",
            host,
            xaml: "<Grid Height=\"170\" ClipToBounds=\"True\" Background=\"#101828\">\n" +
                  "    <TextBlock Text=\"Di chuột trong khung này\" Foreground=\"White\"\n" +
                  "               HorizontalAlignment=\"Center\" VerticalAlignment=\"Center\" />\n" +
                  "    <ui:CursorGlow GlowSize=\"240\" GlowOpacity=\"0.42\" />\n" +
                  "</Grid>",
            csharp: "var glow = new CursorGlow\n" +
                    "{\n" +
                    "    GlowSize = 240,        // đường kính quầng sáng\n" +
                    "    GlowOpacity = 0.42,\n" +
                    "    FollowSpeed = 0.18     // 0 = bám ngay, 1 = không tới\n" +
                    "};\n\n" +
                    "// Tắt khi cửa sổ không active\n" +
                    "glow.IsAnimated = false;"));

        return root;
    }

    private object CreateGridPatternDemo()
    {
        var grid = new UniformGrid { Columns = 3, Height = 96 };

        foreach (GridPatternType kind in new[] { GridPatternType.Dots, GridPatternType.Lines, GridPatternType.Cross })
        {
            var cell = new Grid { Margin = new Thickness(0, 0, 10, 0) };
            cell.SetResourceReference(Panel.BackgroundProperty, "CONCOST.Brush.Surface");
            cell.Children.Add(new GridPattern { Pattern = kind, CellSize = 18 });
            grid.Children.Add(cell);
        }

        var root = new StackPanel();
        root.Children.Add(DemoSection(
            "Chấm · Kẻ · Chữ thập",
            grid,
            xaml: "<ui:GridPattern Pattern=\"Dots\" CellSize=\"18\" PatternOpacity=\"0.55\" />\n" +
                  "<!-- Pattern: Dots | Lines | Cross -->",
            csharp: "var pattern = new GridPattern\n" +
                    "{\n" +
                    "    Pattern = GridPatternType.Cross,\n" +
                    "    CellSize = 18,\n" +
                    "    StrokeThickness = 1,\n" +
                    "    PatternOpacity = 0.55\n" +
                    "};\n\n" +
                    "// Dịch pha để tạo parallax khi cuộn\n" +
                    "pattern.OffsetX = scrollOffset * 0.15;"));

        return root;
    }

    private object CreateShakeDemo()
    {
        var field = new Border
        {
            Width = 240,
            Height = 40,
            CornerRadius = new CornerRadius(8),
            BorderThickness = new Thickness(1),
            Padding = new Thickness(10, 0, 10, 0),
            HorizontalAlignment = HorizontalAlignment.Left
        };
        field.SetResourceReference(Border.BorderBrushProperty, "CONCOST.Brush.Border");
        field.SetResourceReference(Border.BackgroundProperty, "CONCOST.Brush.Surface");
        field.Child = new TextBlock
        {
            Text = "Nhập chiều dài dầm (không hợp lệ)",
            FontSize = 12,
            VerticalAlignment = VerticalAlignment.Center
        };

        var button = new Button { Content = "Kích hoạt shake", Padding = new Thickness(12, 6, 12, 6), Margin = new Thickness(0, 12, 0, 0), HorizontalAlignment = HorizontalAlignment.Left };
        button.SetResourceReference(FrameworkElement.StyleProperty, "CONCOST.Button.Secondary");
        button.Click += (_, _) => ShakeElement.Shake(field);

        var content = new StackPanel();
        content.Children.Add(field);
        content.Children.Add(button);

        var root = new StackPanel();
        root.Children.Add(DemoSection(
            "Dữ liệu không hợp lệ",
            content,
            xaml: "<Border x:Name=\"LengthField\" attached:ShakeElement.Amplitude=\"8\">\n" +
                  "    <!-- ô nhập -->\n" +
                  "</Border>\n\n" +
                  "<!-- Kích hoạt từ code -->",
            csharp: "if (!IsValid(value))\n" +
                    "{\n" +
                    "    ShakeElement.Shake(LengthField);\n" +
                    "    Growl.Toast(\"Chiều dài dầm phải > 0\", GrowlType.Warning);\n" +
                    "}\n\n" +
                    "// Tự động bỏ qua nếu người dùng tắt animation hệ thống"));

        return root;
    }

    private object CreateBrutalThemeDemo()
    {
        var note = new TextBlock
        {
            Text = "Nhấn để áp preset Brutal (nền kem, viền mực, accent vàng điện)",
            FontSize = 12,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 0, 0, 12)
        };
        note.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");

        var brutal = new Button { Content = "Brutal", Padding = new Thickness(14, 6, 14, 6), Margin = new Thickness(0, 0, 8, 0) };
        brutal.SetResourceReference(FrameworkElement.StyleProperty, "CONCOST.Button.Primary");
        brutal.Click += (_, _) => ThemeManager.Current.Preset = ThemePreset.Brutal;

        var normal = new Button { Content = "Default", Padding = new Thickness(14, 6, 14, 6) };
        normal.SetResourceReference(FrameworkElement.StyleProperty, "CONCOST.Button.Secondary");
        normal.Click += (_, _) => ThemeManager.Current.Preset = ThemePreset.Default;

        var row = new StackPanel { Orientation = Orientation.Horizontal };
        row.Children.Add(brutal);
        row.Children.Add(normal);

        var content = new StackPanel();
        content.Children.Add(note);
        content.Children.Add(row);

        var root = new StackPanel();
        root.Children.Add(DemoSection(
            "Preset Brutal",
            content,
            xaml: "<!-- Trong App.xaml.cs -->",
            csharp: "ThemeManager.Current.Preset = ThemePreset.Brutal;\n\n" +
                    "// Quay lại mặc định — brush được khôi phục từ snapshot\n" +
                    "ThemeManager.Current.Preset = ThemePreset.Default;"));

        return root;
    }

    private object CreateReducedMotionDemo()
    {
        var note = new TextBlock
        {
            Text = MotionSettings.AnimationsEnabled
                ? "Animation đang BẬT (theo cài đặt Windows)."
                : "Animation đang TẮT — các hiệu ứng vẽ khung tĩnh.",
            FontSize = 12,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 0, 0, 10)
        };
        note.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");

        var toggle = new Button { Content = "Tắt / bật animation trong app", Padding = new Thickness(12, 6, 12, 6), HorizontalAlignment = HorizontalAlignment.Left };
        toggle.SetResourceReference(FrameworkElement.StyleProperty, "CONCOST.Button.Secondary");
        toggle.Click += (_, _) =>
        {
            MotionSettings.ForceDisabled = !MotionSettings.ForceDisabled;
            note.Text = MotionSettings.AnimationsEnabled
                ? "Animation đang BẬT."
                : "Animation đang TẮT — các hiệu ứng vẽ khung tĩnh.";
        };

        var content = new StackPanel();
        content.Children.Add(note);
        content.Children.Add(toggle);

        var root = new StackPanel();
        root.Children.Add(DemoSection(
            "Tắt animation hệ thống",
            content,
            xaml: "<!-- Không cần làm gì: mọi hiệu ứng tự đọc MotionSettings -->",
            csharp: "// Tắt toàn bộ hiệu ứng trong app (ví dụ theo setting của người dùng)\n" +
                    "MotionSettings.ForceDisabled = true;\n\n" +
                    "// Windows \"Show animations\" = off  →  MotionSettings.AnimationsEnabled == false\n" +
                    "// Các hiệu ứng tự vẽ khung tĩnh thay vì đứng hình."));

        return root;
    }
}
