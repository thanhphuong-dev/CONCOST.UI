using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CONCOST.UI.Controls;

namespace CONCOST.UI.Showcase;

public partial class ShowcaseViewModel
{
    // =========================================================================
    // DEMO: sheen sweep, pulse ring, wave divider, gradient text
    // =========================================================================

    private object CreateSheenSweepDemo()
    {
        var host = new Grid { Width = 320, Height = 52, HorizontalAlignment = HorizontalAlignment.Left };
        var surface = new Border { CornerRadius = new CornerRadius(10) };
        surface.SetResourceReference(Border.BackgroundProperty, "CONCOST.Brush.TextPrimary");
        host.Children.Add(surface);

        var label = new TextBlock
        {
            Text = "Xuất báo cáo",
            Foreground = Brushes.White,
            FontSize = 14,
            FontWeight = FontWeights.SemiBold,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        host.Children.Add(label);
        host.Children.Add(new SheenSweep { Speed = 0.35, PauseSeconds = 0.8, PeakOpacity = 0.45 });

        var root = new StackPanel();
        root.Children.Add(DemoSection(
            "Mặc định",
            host,
            xaml: "<Grid Width=\"320\" Height=\"52\">\n" +
                  "    <Border CornerRadius=\"10\" Background=\"#101828\" />\n" +
                  "    <TextBlock Text=\"Xuất báo cáo\" Foreground=\"White\"\n" +
                  "               HorizontalAlignment=\"Center\" VerticalAlignment=\"Center\" />\n" +
                  "    <ui:SheenSweep Speed=\"0.35\" PauseSeconds=\"0.8\" PeakOpacity=\"0.45\" />\n" +
                  "</Grid>",
            csharp: "var sheen = new SheenSweep\n" +
                    "{\n" +
                    "    Speed = 0.35,          // số lần quét / giây\n" +
                    "    PauseSeconds = 0.8,    // nghỉ giữa hai lần quét\n" +
                    "    BandWidth = 0.22,      // bề rộng vệt sáng theo % chiều rộng\n" +
                    "    Angle = 24,\n" +
                    "    PeakOpacity = 0.45\n" +
                    "};\n\n" +
                    "// Tạm dừng khi cần\n" +
                    "sheen.IsActive = false;"));

        return root;
    }

    private object CreatePulseRingDemo()
    {
        var ring = new PulseRing { MaxRadius = 46, Width = 110, Height = 110, DotRadius = 6 };

        var badge = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
        badge.Children.Add(ring);

        var text = new StackPanel { VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(4, 0, 0, 0) };
        var title = new TextBlock { Text = "Đang đồng bộ cloud", FontSize = 13, FontWeight = FontWeights.SemiBold };
        title.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextPrimary");
        text.Children.Add(title);
        text.Children.Add(DemoCaption("3 bản vẽ đang được tải lên", 2));
        badge.Children.Add(text);

        var root = new StackPanel();
        root.Children.Add(DemoSection(
            "Mặc định",
            badge,
            xaml: "<ui:PulseRing MaxRadius=\"46\" Width=\"110\" Height=\"110\"\n" +
                  "              RingCount=\"3\" Duration=\"2.2\" DotRadius=\"6\" />",
            csharp: "var pulse = new PulseRing\n" +
                    "{\n" +
                    "    MaxRadius = 46,\n" +
                    "    RingCount = 3,\n" +
                    "    Duration = 2.2,        // giây cho một vòng lan\n" +
                    "    RingThickness = 2,\n" +
                    "    RingBrush = Brushes.SeaGreen\n" +
                    "};\n\n" +
                    "// Tắt khi mất kết nối\n" +
                    "pulse.IsActive = false;"));

        return root;
    }

    private object CreateWaveDividerDemo()
    {
        var host = new Grid { Height = 120, ClipToBounds = true };
        var surface = new Border();
        surface.SetResourceReference(Border.BackgroundProperty, "CONCOST.Brush.SurfaceSecondary");
        host.Children.Add(surface);

        var caption = new TextBlock
        {
            Text = "Phân vùng tải trọng",
            FontSize = 15,
            FontWeight = FontWeights.SemiBold,
            Margin = new Thickness(18, 14, 0, 0),
            VerticalAlignment = VerticalAlignment.Top
        };
        caption.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextPrimary");
        host.Children.Add(caption);

        var wave = new WaveDivider { Height = 70, VerticalAlignment = VerticalAlignment.Bottom, Amplitude = 10, Speed = 18 };
        wave.SetResourceReference(WaveDivider.FillBrushProperty, "CONCOST.Brush.Primary");
        host.Children.Add(wave);

        var root = new StackPanel();
        root.Children.Add(DemoSection(
            "Mặc định",
            host,
            xaml: "<Grid Height=\"120\" ClipToBounds=\"True\">\n" +
                  "    <Border Background=\"{DynamicResource CONCOST.Brush.SurfaceSecondary}\" />\n" +
                  "    <ui:WaveDivider Height=\"70\" VerticalAlignment=\"Bottom\" Amplitude=\"10\" Speed=\"18\"\n" +
                  "                    FillBrush=\"{DynamicResource CONCOST.Brush.Primary}\" />\n" +
                  "</Grid>",
            csharp: "var wave = new WaveDivider\n" +
                    "{\n" +
                    "    Height = 70,\n" +
                    "    Amplitude = 10,        // biên độ sóng (px)\n" +
                    "    Wavelength = 180,      // độ dài bước sóng\n" +
                    "    Speed = 18,            // px / giây\n" +
                    "    ShowBackWave = true    // thêm lớp sóng phía sau\n" +
                    "};"));

        return root;
    }

    private object CreateGradientTextDemo()
    {
        var headline = new GradientText
        {
            Text = "CONCOST UI",
            FontSize = 34,
            HorizontalAlignment = HorizontalAlignment.Left
        };

        var sub = DemoCaption("Chữ tô gradient động — dùng cho hero headline, badge phiên bản.", 4);

        var content = new StackPanel();
        content.Children.Add(headline);
        content.Children.Add(sub);

        var root = new StackPanel();
        root.Children.Add(DemoSection(
            "Mặc định",
            content,
            xaml: "<ui:GradientText Text=\"CONCOST UI\" FontSize=\"34\"\n" +
                  "                 StartColor=\"#FF7900\" EndColor=\"#0284C7\" Speed=\"0.35\" />",
            csharp: "var headline = new GradientText\n" +
                    "{\n" +
                    "    Text = \"CONCOST UI\",\n" +
                    "    FontSize = 34,\n" +
                    "    StartColor = Color.FromRgb(0xFF, 0x79, 0x00),\n" +
                    "    EndColor = Color.FromRgb(0x02, 0x84, 0xC7),\n" +
                    "    Speed = 0.35           // chu kỳ gradient / giây\n" +
                    "};\n\n" +
                    "// Chữ tĩnh, không chạy gradient\n" +
                    "headline.IsAnimated = false;"));

        return root;
    }
}
