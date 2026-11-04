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
            Text = Loc.T("Xuất báo cáo", "Export report"),
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
            Loc.T("Mặc định", "Default"),
            host,
            xaml: "<Grid Width=\"320\" Height=\"52\">\n" +
                  "    <Border CornerRadius=\"10\" Background=\"#101828\" />\n" +
                  Loc.T("    <TextBlock Text=\"Xuất báo cáo\" Foreground=\"White\"\n", "    <TextBlock Text=\"Export report\" Foreground=\"White\"\n") +
                  "               HorizontalAlignment=\"Center\" VerticalAlignment=\"Center\" />\n" +
                  "    <ui:SheenSweep Speed=\"0.35\" PauseSeconds=\"0.8\" PeakOpacity=\"0.45\" />\n" +
                  "</Grid>",
            csharp: "var sheen = new SheenSweep\n" +
                    "{\n" +
                    Loc.T("    Speed = 0.35,          // số lần quét / giây\n", "    Speed = 0.35,          // sweeps per second\n") +
                    Loc.T("    PauseSeconds = 0.8,    // nghỉ giữa hai lần quét\n", "    PauseSeconds = 0.8,    // rest between two sweeps\n") +
                    Loc.T("    BandWidth = 0.22,      // bề rộng vệt sáng theo % chiều rộng\n", "    BandWidth = 0.22,      // sheen width as a share of the width\n") +
                    "    Angle = 24,\n" +
                    "    PeakOpacity = 0.45\n" +
                    "};\n\n" +
                    Loc.T("// Tạm dừng khi cần\n", "// Pause when needed") +
                    "sheen.IsActive = false;"));

        return root;
    }

    private object CreatePulseRingDemo()
    {
        var ring = new PulseRing { MaxRadius = 46, Width = 110, Height = 110, DotRadius = 6 };

        var badge = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
        badge.Children.Add(ring);

        var text = new StackPanel { VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(4, 0, 0, 0) };
        var title = new TextBlock { Text = Loc.T("Đang đồng bộ cloud", "Syncing to the cloud"), FontSize = 13, FontWeight = FontWeights.SemiBold };
        title.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextPrimary");
        text.Children.Add(title);
        text.Children.Add(DemoCaption(Loc.T("3 bản vẽ đang được tải lên", "3 drawings uploading"), 2));
        badge.Children.Add(text);

        var root = new StackPanel();
        root.Children.Add(DemoSection(
            Loc.T("Mặc định", "Default"),
            badge,
            xaml: "<ui:PulseRing MaxRadius=\"46\" Width=\"110\" Height=\"110\"\n" +
                  "              RingCount=\"3\" Duration=\"2.2\" DotRadius=\"6\" />",
            csharp: "var pulse = new PulseRing\n" +
                    "{\n" +
                    "    MaxRadius = 46,\n" +
                    "    RingCount = 3,\n" +
                    Loc.T("    Duration = 2.2,        // giây cho một vòng lan\n", "    Duration = 2.2,        // seconds per ripple\n") +
                    "    RingThickness = 2,\n" +
                    "    RingBrush = Brushes.SeaGreen\n" +
                    "};\n\n" +
                    Loc.T("// Tắt khi mất kết nối\n", "// Turn off when the connection drops") +
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
            Text = Loc.T("Phân vùng tải trọng", "Load zones"),
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
            Loc.T("Mặc định", "Default"),
            host,
            xaml: "<Grid Height=\"120\" ClipToBounds=\"True\">\n" +
                  "    <Border Background=\"{DynamicResource CONCOST.Brush.SurfaceSecondary}\" />\n" +
                  "    <ui:WaveDivider Height=\"70\" VerticalAlignment=\"Bottom\" Amplitude=\"10\" Speed=\"18\"\n" +
                  "                    FillBrush=\"{DynamicResource CONCOST.Brush.Primary}\" />\n" +
                  "</Grid>",
            csharp: "var wave = new WaveDivider\n" +
                    "{\n" +
                    "    Height = 70,\n" +
                    Loc.T("    Amplitude = 10,        // biên độ sóng (px)\n", "    Amplitude = 10,        // wave amplitude (px)\n") +
                    Loc.T("    Wavelength = 180,      // độ dài bước sóng\n", "    Wavelength = 180,      // wavelength\n") +
                    Loc.T("    Speed = 18,            // px / giây\n", "    Speed = 18,            // px / second\n") +
                    Loc.T("    ShowBackWave = true    // thêm lớp sóng phía sau\n", "    ShowBackWave = true    // adds a second wave behind\n") +
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

        var sub = DemoCaption(Loc.T("Chữ tô gradient động — dùng cho hero headline, badge phiên bản.", "Animated gradient text — for hero headlines and version badges."), 4);

        var content = new StackPanel();
        content.Children.Add(headline);
        content.Children.Add(sub);

        var root = new StackPanel();
        root.Children.Add(DemoSection(
            Loc.T("Mặc định", "Default"),
            content,
            xaml: "<ui:GradientText Text=\"CONCOST UI\" FontSize=\"34\"\n" +
                  "                 StartColor=\"#FF7900\" EndColor=\"#0284C7\" Speed=\"0.35\" />",
            csharp: "var headline = new GradientText\n" +
                    "{\n" +
                    "    Text = \"CONCOST UI\",\n" +
                    "    FontSize = 34,\n" +
                    "    StartColor = Color.FromRgb(0xFF, 0x79, 0x00),\n" +
                    "    EndColor = Color.FromRgb(0x02, 0x84, 0xC7),\n" +
                    Loc.T("    Speed = 0.35           // chu kỳ gradient / giây\n", "    Speed = 0.35           // gradient cycles per second\n") +
                    "};\n\n" +
                    Loc.T("// Chữ tĩnh, không chạy gradient\n", "// Static text, no gradient") +
                    "headline.IsAnimated = false;"));

        return root;
    }
}
