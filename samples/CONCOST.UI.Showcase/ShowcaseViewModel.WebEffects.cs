using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CONCOST.UI.Controls;

namespace CONCOST.UI.Showcase;

public partial class ShowcaseViewModel
{
    // =========================================================================
    // DEMO: modern visual effects (aurora backdrop, border beam, confetti)
    // =========================================================================

    private object CreateAuroraBackgroundDemo()
    {
        var aurora = new AuroraBackground { BlobCount = 5, BlobOpacity = 0.45 };
        var title = new TextBlock
        {
            Text = "CONCOST STRUCTURAL STUDIO",
            FontSize = 20,
            FontWeight = FontWeights.Bold,
            Foreground = Brushes.White,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        var subtitle = new TextBlock
        {
            Text = "AuroraBackground — nền động cho splash screen / hero",
            FontSize = 12,
            Foreground = new SolidColorBrush(Color.FromArgb(200, 255, 255, 255)),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 6, 0, 0)
        };

        var stack = new StackPanel { HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
        stack.Children.Add(title);
        stack.Children.Add(subtitle);

        var host = new Grid { Height = 170, ClipToBounds = true, Background = new SolidColorBrush(Color.FromRgb(0x0B, 0x0F, 0x17)) };
        host.Children.Add(aurora);
        host.Children.Add(stack);

        var root = new StackPanel();
        root.Children.Add(DemoSection(
            "AuroraBackground — nền gradient động",
            host,
            xaml: "<Grid Height=\"170\" ClipToBounds=\"True\" Background=\"#0B0F17\">\n" +
                  "    <ui:AuroraBackground BlobCount=\"5\" BlobOpacity=\"0.45\" />\n" +
                  "    <TextBlock Text=\"CONCOST STUDIO\" Foreground=\"White\"\n" +
                  "               HorizontalAlignment=\"Center\" VerticalAlignment=\"Center\" />\n" +
                  "</Grid>",
            csharp: "var aurora = new AuroraBackground\n" +
                    "{\n" +
                    "    BlobCount = 5,\n" +
                    "    BlobOpacity = 0.45,\n" +
                    "    Speed = 1.0,              // hệ số tốc độ\n" +
                    "    AccentBrush = Brushes.OrangeRed\n" +
                    "};\n\n" +
                    "// Tắt animation khi cửa sổ không hiển thị\n" +
                    "aurora.IsAnimated = false;"));

        return root;
    }

    private object CreateBorderBeamDemo()
    {
        var inner = new StackPanel();
        var heading = new TextBlock { Text = "Nghiệm thu đợt 3", FontSize = 14, FontWeight = FontWeights.SemiBold };
        heading.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextPrimary");
        inner.Children.Add(heading);
        inner.Children.Add(DemoCaption("Viền sáng chạy quanh thẻ — dùng cho item mới, trạng thái đang xử lý.", 4));

        var beam = new BorderBeam
        {
            CornerRadius = new CornerRadius(12),
            Padding = new Thickness(18),
            BeamThickness = 1.8,
            BeamLength = 0.30,
            RevolutionSeconds = 3.5,
            HorizontalAlignment = HorizontalAlignment.Left
        };
        beam.SetResourceReference(BorderBeam.SurfaceBrushProperty, "CONCOST.Brush.Surface");
        beam.Child = inner;

        var root = new StackPanel();
        root.Children.Add(DemoSection(
            "BorderBeam — vệt sáng chạy quanh viền",
            beam,
            xaml: "<ui:BorderBeam CornerRadius=\"12\" Padding=\"18\"\n" +
                  "               BeamThickness=\"1.8\" BeamLength=\"0.30\" RevolutionSeconds=\"3.5\">\n" +
                  "    <StackPanel>\n" +
                  "        <TextBlock Text=\"Nghiệm thu đợt 3\" FontWeight=\"SemiBold\" />\n" +
                  "        <TextBlock Text=\"Đang xử lý...\" Foreground=\"Gray\" />\n" +
                  "    </StackPanel>\n" +
                  "</ui:BorderBeam>",
            csharp: "var beam = new BorderBeam\n" +
                    "{\n" +
                    "    CornerRadius = new CornerRadius(12),\n" +
                    "    BeamThickness = 1.8,\n" +
                    "    BeamLength = 0.30,          // 30% chu vi\n" +
                    "    RevolutionSeconds = 3.5,    // một vòng / 3.5 giây\n" +
                    "    BeamBrush = Brushes.Orange\n" +
                    "};\n" +
                    "beam.Child = new TextBlock { Text = \"Đang xử lý...\" };"));

        return root;
    }

    private object CreateConfettiBurstDemo()
    {
        var confetti = new ConfettiBurst { ParticleCount = 110 };
        var message = new TextBlock
        {
            Text = "Bấm để ăn mừng",
            FontSize = 13,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        message.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");

        var button = new Button
        {
            Content = "Duyệt hồ sơ",
            Padding = new Thickness(14, 6, 14, 6),
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 12, 0, 0)
        };
        button.SetResourceReference(FrameworkElement.StyleProperty, "CONCOST.Button.Primary");
        button.Click += (_, _) => confetti.IsBursting = true;

        var stack = new StackPanel { HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
        stack.Children.Add(message);
        stack.Children.Add(button);

        var host = new Grid { Height = 170, ClipToBounds = true };
        host.SetResourceReference(Panel.BackgroundProperty, "CONCOST.Brush.SurfaceSecondary");
        host.Children.Add(stack);
        host.Children.Add(confetti);

        var root = new StackPanel();
        root.Children.Add(DemoSection(
            "ConfettiBurst — hiệu ứng ăn mừng",
            host,
            xaml: "<Grid Height=\"170\" ClipToBounds=\"True\">\n" +
                  "    <Button Content=\"Duyệt hồ sơ\" Click=\"OnApproveClick\" />\n" +
                  "    <ui:ConfettiBurst x:Name=\"Celebration\" ParticleCount=\"110\" />\n" +
                  "</Grid>",
            csharp: "private void OnApproveClick(object sender, RoutedEventArgs e)\n" +
                    "{\n" +
                    "    Approve();\n" +
                    "    Celebration.IsBursting = true;   // tự tắt khi hạt tan hết\n" +
                    "}\n\n" +
                    "// Tuỳ chỉnh\n" +
                    "Celebration.Gravity = 1100;\n" +
                    "Celebration.Duration = new Duration(TimeSpan.FromSeconds(2));\n" +
                    "Celebration.Palette = new[] { Colors.Orange, Colors.SeaGreen };"));

        return root;
    }
}
