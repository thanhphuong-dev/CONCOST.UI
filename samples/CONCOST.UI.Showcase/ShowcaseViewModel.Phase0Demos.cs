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
            Text = Loc.T("Di chuột trong khung này", "Move the pointer inside this frame"),
            Foreground = Brushes.White,
            FontSize = 13,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        host.Children.Add(label);
        host.Children.Add(new CursorGlow { GlowSize = 240, GlowOpacity = 0.42 });

        var root = new StackPanel();
        root.Children.Add(DemoSection(
            Loc.T("Mặc định", "Default"),
            host,
            xaml: "<Grid Height=\"170\" ClipToBounds=\"True\" Background=\"#101828\">\n" +
                  Loc.T("    <TextBlock Text=\"Di chuột trong khung này\" Foreground=\"White\"\n", "    <TextBlock Text=\"Move the pointer inside this frame\" Foreground=\"White\"\n") +
                  "               HorizontalAlignment=\"Center\" VerticalAlignment=\"Center\" />\n" +
                  "    <ui:CursorGlow GlowSize=\"240\" GlowOpacity=\"0.42\" />\n" +
                  "</Grid>",
            csharp: "var glow = new CursorGlow\n" +
                    "{\n" +
                    Loc.T("    GlowSize = 240,        // đường kính quầng sáng\n", "    GlowSize = 240,        // glow diameter\n") +
                    "    GlowOpacity = 0.42,\n" +
                    Loc.T("    FollowSpeed = 0.18     // 0 = bám ngay, 1 = không tới\n", "    FollowSpeed = 0.18     // 0 = snap, 1 = never arrives\n") +
                    "};\n\n" +
                    Loc.T("// Tắt khi cửa sổ không active\n", "// Turn off while the window is inactive\n") +
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
            Loc.T("Chấm · Kẻ · Chữ thập", "Dots · Lines · Crosses"),
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
                    Loc.T("// Dịch pha để tạo parallax khi cuộn\n", "// Shift the phase to get parallax while scrolling\n") +
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
            Text = Loc.T("Nhập chiều dài dầm (không hợp lệ)", "Enter a beam length (invalid)"),
            FontSize = 12,
            VerticalAlignment = VerticalAlignment.Center
        };

        var button = new Button { Content = Loc.T("Kích hoạt shake", "Trigger shake"), Padding = new Thickness(12, 6, 12, 6), Margin = new Thickness(0, 12, 0, 0), HorizontalAlignment = HorizontalAlignment.Left };
        button.SetResourceReference(FrameworkElement.StyleProperty, "CONCOST.Button.Secondary");
        button.Click += (_, _) => ShakeElement.Shake(field);

        var content = new StackPanel();
        content.Children.Add(field);
        content.Children.Add(button);

        var root = new StackPanel();
        root.Children.Add(DemoSection(
            Loc.T("Dữ liệu không hợp lệ", "Invalid data"),
            content,
            xaml: "<Border x:Name=\"LengthField\" attached:ShakeElement.Amplitude=\"8\">\n" +
                  Loc.T("    <!-- ô nhập -->\n", "    <!-- input box -->\n") +
                  "</Border>\n\n" +
                  Loc.T("<!-- Kích hoạt từ code -->", "<!-- Trigger from code -->\n"),
            csharp: "if (!IsValid(value))\n" +
                    "{\n" +
                    "    ShakeElement.Shake(LengthField);\n" +
                    Loc.T("    Growl.Toast(\"Chiều dài dầm phải > 0\", GrowlType.Warning);\n", "    Growl.Toast(\"Beam length must be > 0\", GrowlType.Warning);\n") +
                    "}\n\n" +
                    Loc.T("// Tự động bỏ qua nếu người dùng tắt animation hệ thống", "// Automatically skipped when the user disables system animation\n")));

        return root;
    }

    /// <summary>
    /// AppTheme.Auto: the plugin has no Application of its own, so the theme is read from the
    /// window it lives in. Two simulated host windows — one light, one dark — prove the point.
    /// </summary>
    private object CreateAutoThemeDemo()
    {
        var note = new TextBlock
        {
            Text = Loc.T("Hai khung dưới là host window giả lập (một sáng, một tối). Nội dung bên trong tự chọn theme theo khung chứa nó — đúng cách plugin Revit/AutoCAD chạy: không có Application riêng, chỉ có window của host.", "The two frames below are simulated host windows (one light, one dark). The content inside picks its theme from the frame that hosts it — exactly how a Revit/AutoCAD plug-in runs: no Application of its own, only the host window."),
            FontSize = 12,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 0, 0, 14)
        };
        note.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");

        Border HostPanel(Color background, Color border)
        {
            var panel = new Border
            {
                Background = new SolidColorBrush(background),
                BorderBrush = new SolidColorBrush(border),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(16),
                Margin = new Thickness(0, 0, 12, 0)
            };
            return panel;
        }

        FrameworkElement PluginContent(string caption)
        {
            var stack = new StackPanel();

            var title = new TextBlock { Text = caption, FontSize = 12.5, FontWeight = FontWeights.SemiBold };
            title.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextPrimary");
            stack.Children.Add(title);

            var body = new TextBlock
            {
                Text = Loc.T("Nội dung plugin — chữ, viền và nút lấy theo theme của host.", "Plug-in content — text, borders and buttons follow the host theme."),
                FontSize = 11.5,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 5, 0, 0)
            };
            body.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");
            stack.Children.Add(body);

            var action = new Button
            {
                Content = "Rebar layout",
                Margin = new Thickness(0, 12, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Left
            };
            action.SetResourceReference(FrameworkElement.StyleProperty, "CONCOST.Button.Primary");
            stack.Children.Add(action);

            // The plugin's own root: transparent, so detection walks up to the host panel.
            var scope = new Border { Child = stack };
            scope.Loaded += (_, __) => ThemeManager.Current.ApplyTheme(scope, AppTheme.Auto);
            return scope;
        }

        var lightHost = HostPanel(Color.FromRgb(0xFF, 0xFF, 0xFF), Color.FromRgb(0xE2, 0xE8, 0xF0));
        var darkHost = HostPanel(Color.FromRgb(0x11, 0x18, 0x27), Color.FromRgb(0x33, 0x41, 0x55));

        var lightScope = PluginContent(Loc.T("Host sáng (light window)", "Light host window"));
        var darkScope = PluginContent(Loc.T("Host tối (dark window)", "Dark host window"));
        lightHost.Child = lightScope;
        darkHost.Child = darkScope;

        var row = new Grid();
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        Grid.SetColumn(lightHost, 0);
        Grid.SetColumn(darkHost, 1);
        darkHost.Margin = new Thickness(0);
        row.Children.Add(lightHost);
        row.Children.Add(darkHost);

        var swap = new Button
        {
            Content = Loc.T("Đảo màu 2 host", "Swap the two hosts"),
            Margin = new Thickness(0, 14, 0, 0),
            HorizontalAlignment = HorizontalAlignment.Left,
            Padding = new Thickness(14, 6, 14, 6)
        };
        swap.SetResourceReference(FrameworkElement.StyleProperty, "CONCOST.Button.Secondary");
        swap.Click += (_, __) =>
        {
            (lightHost.Background, darkHost.Background) = (darkHost.Background, lightHost.Background);
            (lightHost.BorderBrush, darkHost.BorderBrush) = (darkHost.BorderBrush, lightHost.BorderBrush);

            // The host changed its own theme: tell the plugin roots to look again.
            ThemeManager.Current.RefreshAutoTheme(lightScope);
            ThemeManager.Current.RefreshAutoTheme(darkScope);
        };

        var content = new StackPanel();
        content.Children.Add(note);
        content.Children.Add(row);
        content.Children.Add(swap);

        var root = new StackPanel();
        root.Children.Add(DemoSection(
            Loc.T("Auto — theme bám theo window chủ", "Auto — theme follows the host window"),
            content,
            xaml: Loc.T("<!-- Plugin root: một UserControl hoặc Grid của add-in -->\n", "<!-- Plug-in root: a UserControl or Grid from the add-in -->\n") +
                  "<UserControl x:Name=\"PluginRoot\" Background=\"Transparent\">\n" +
                  Loc.T("    <!-- nội dung plugin -->\n", "    <!-- plug-in content -->\n") +
                  "</UserControl>",
            csharp: Loc.T("// Không cần Application.Current — chỉ cần root của plugin\n", "// No Application.Current needed — just the plug-in root\n") +
                    "ThemeManager.Current.ApplyTheme(PluginRoot, AppTheme.Auto);\n\n" +
                    Loc.T("// Host vừa đổi theme (Revit/AutoCAD chuyển sáng-tối)\n", "// The host just switched theme (Revit/AutoCAD light-dark)\n") +
                    "ThemeManager.Current.RefreshAutoTheme(PluginRoot);\n\n" +
                    Loc.T("// Hỏi trước khi cần biết host đang sáng hay tối\n", "// Ask before you need to know whether the host is light or dark\n") +
                    "var hostTheme = ThemeManager.DetectTheme(PluginRoot);"));

        return root;
    }

    private object CreateBrutalThemeDemo()
    {
        var note = new TextBlock
        {
            Text = Loc.T("Nhấn để áp preset Brutal (nền kem, viền mực, accent vàng điện)", "Click to apply the Brutal preset (cream ground, ink border, electric-yellow accent)"),
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
                    Loc.T("// Quay lại mặc định — brush được khôi phục từ snapshot\n", "// Back to the default — brushes are restored from the snapshot\n") +
                    "ThemeManager.Current.Preset = ThemePreset.Default;"));

        return root;
    }

    private object CreateReducedMotionDemo()
    {
        var note = new TextBlock
        {
            Text = MotionSettings.AnimationsEnabled
                ? Loc.T("Animation đang BẬT (theo cài đặt Windows).", "Animation is ON (per the Windows setting).")
                : Loc.T("Animation đang TẮT — các hiệu ứng vẽ khung tĩnh.", "Animation is OFF — effects draw a static frame."),
            FontSize = 12,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 0, 0, 10)
        };
        note.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");

        var toggle = new Button { Content = Loc.T("Tắt / bật animation trong app", "Turn animation off / on in the app"), Padding = new Thickness(12, 6, 12, 6), HorizontalAlignment = HorizontalAlignment.Left };
        toggle.SetResourceReference(FrameworkElement.StyleProperty, "CONCOST.Button.Secondary");
        toggle.Click += (_, _) =>
        {
            MotionSettings.ForceDisabled = !MotionSettings.ForceDisabled;
            note.Text = MotionSettings.AnimationsEnabled
                ? Loc.T("Animation đang BẬT.", "Animation is ON.")
                : Loc.T("Animation đang TẮT — các hiệu ứng vẽ khung tĩnh.", "Animation is OFF — effects draw a static frame.");
        };

        var content = new StackPanel();
        content.Children.Add(note);
        content.Children.Add(toggle);

        var root = new StackPanel();
        root.Children.Add(DemoSection(
            Loc.T("Tắt animation hệ thống", "Disable system animation"),
            content,
            xaml: Loc.T("<!-- Không cần làm gì: mọi hiệu ứng tự đọc MotionSettings -->", "<!-- Nothing to do: every effect reads MotionSettings on its own -->\n"),
            csharp: Loc.T("// Tắt toàn bộ hiệu ứng trong app (ví dụ theo setting của người dùng)\n", "// Turn every effect off in the app (for example from a user setting)\n") +
                    "MotionSettings.ForceDisabled = true;\n\n" +
                    "// Windows \"Show animations\" = off  →  MotionSettings.AnimationsEnabled == false\n" +
                    Loc.T("// Các hiệu ứng tự vẽ khung tĩnh thay vì đứng hình.", "// Effects draw a static frame instead of freezing.\n")));

        return root;
    }
}
