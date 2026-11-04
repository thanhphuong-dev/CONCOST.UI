using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CONCOST.UI.Controls;

namespace CONCOST.UI.Showcase;

public partial class ShowcaseViewModel
{
    // =========================================================================
    // DEMO: Phase 2 — scroll reveal, parallax, typewriter, text reveal
    // =========================================================================

    private object CreateScrollRevealDemo()
    {
        var scroller = new ScrollViewer { Height = 200, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
        var inner = new StackPanel();

        for (int i = 1; i <= 4; i++)
        {
            var card = new Border
            {
                Height = 64,
                CornerRadius = new CornerRadius(8),
                Margin = new Thickness(0, 0, 0, 10),
                Padding = new Thickness(14, 0, 14, 0)
            };
            card.SetResourceReference(Border.BackgroundProperty, "CONCOST.Brush.SurfaceSecondary");
            card.Child = new TextBlock
            {
                Text = Loc.F("Khối {0} — fade + slide khi cuộn tới", "Block {0} — fade + slide on scroll", i),
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 12
            };
            ScrollReveal.SetIsEnabled(card, true);
            ScrollReveal.SetDelay(card, new Duration(TimeSpan.FromMilliseconds(i * 60)));
            inner.Children.Add(card);
        }

        scroller.Content = inner;

        var root = new StackPanel();
        root.Children.Add(DemoSection(
            Loc.T("Mặc định", "Default"),
            scroller,
            xaml: "<ScrollViewer Height=\"200\">\n" +
                  "    <StackPanel>\n" +
                  "        <Border attached:ScrollReveal.IsEnabled=\"True\"\n" +
                  "                attached:ScrollReveal.Offset=\"24\"\n" +
                  "                attached:ScrollReveal.Delay=\"0:0:0.12\">…</Border>\n" +
                  "    </StackPanel>\n" +
                  "</ScrollViewer>",
            csharp: Loc.T("// Bật cho từng phần tử; delay tăng dần để có hiệu ứng stagger\n", "// Enable per element; a growing delay gives the stagger effect\n") +
                    "ScrollReveal.SetIsEnabled(card, true);\n" +
                    "ScrollReveal.SetOffset(card, 24);\n" +
                    "ScrollReveal.SetDelay(card, new Duration(TimeSpan.FromMilliseconds(120)));\n\n" +
                    Loc.T("// Tự tìm ScrollViewer cha; hiện một lần rồi thôi", "// Finds the parent ScrollViewer itself; shows once and stops\n")));

        return root;
    }

    private object CreateParallaxDemo()
    {
        var scroller = new ScrollViewer { Height = 190, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
        var host = new Grid { Height = 320 };

        var back = new ParallaxPanel { Speed = 0.45, VerticalAlignment = VerticalAlignment.Top, Height = 120, Margin = new Thickness(0, 20, 0, 0) };
        var backBorder = new Border { CornerRadius = new CornerRadius(10) };
        backBorder.SetResourceReference(Border.BackgroundProperty, "CONCOST.Brush.SurfaceSecondary");
        back.Content = backBorder;
        host.Children.Add(back);

        var front = new Border { Height = 90, Margin = new Thickness(30, 130, 30, 0), CornerRadius = new CornerRadius(10), VerticalAlignment = VerticalAlignment.Top };
        front.SetResourceReference(Border.BackgroundProperty, "CONCOST.Brush.Primary");
        front.Child = new TextBlock
        {
            Text = Loc.T("Lớp trước (cuộn bình thường)", "Front layer (normal scrolling)"),
            Foreground = Brushes.White,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            FontSize = 12
        };
        host.Children.Add(front);

        scroller.Content = host;

        var root = new StackPanel();
        root.Children.Add(DemoSection(
            Loc.T("Mặc định", "Default"),
            scroller,
            xaml: "<Grid Height=\"320\">\n" +
                  "    <ui:ParallaxPanel Speed=\"0.45\" Height=\"120\" VerticalAlignment=\"Top\">\n" +
                  "        <Border Background=\"{DynamicResource CONCOST.Brush.SurfaceSecondary}\" />\n" +
                  "    </ui:ParallaxPanel>\n" +
                  "    <Border Height=\"90\" Background=\"{DynamicResource CONCOST.Brush.Primary}\" />\n" +
                  "</Grid>",
            csharp: Loc.T("// Speed < 1: lớp nền trôi chậm hơn nội dung (tạo độ sâu)\n", "// Speed < 1: the background layer drifts slower than the content (adds depth)\n") +
                    "var layer = new ParallaxPanel\n" +
                    "{\n" +
                    "    Speed = 0.45,\n" +
                    "    MaxOffset = 120\n" +
                    "};"));

        return root;
    }

    private object CreateTypewriterDemo()
    {
        var typewriter = new TypewriterText
        {
            Text = Loc.T("Bóc tách khối lượng tự động từ mô hình BIM trong 12 giây.", "Automatic quantity take-off from the BIM model in 12 seconds."),
            FontSize = 15,
            Margin = new Thickness(0, 0, 0, 12)
        };

        var again = new Button { Content = Loc.T("Gõ lại", "Type it again"), Padding = new Thickness(12, 6, 12, 6), HorizontalAlignment = HorizontalAlignment.Left };
        again.SetResourceReference(FrameworkElement.StyleProperty, "CONCOST.Button.Secondary");
        again.Click += (_, _) =>
        {
            typewriter.Restart();
        };

        var content = new StackPanel();
        content.Children.Add(typewriter);
        content.Children.Add(again);

        var root = new StackPanel();
        root.Children.Add(DemoSection(
            Loc.T("Mặc định", "Default"),
            content,
            xaml: Loc.T("<ui:TypewriterText Text=\"Bóc tách khối lượng tự động...\"\n", "<ui:TypewriterText Text=\"Automatic quantity take-off...\"\n") +
                  "                   CharsPerSecond=\"26\" ShowCaret=\"True\" />",
            csharp: "var typer = new TypewriterText\n" +
                    "{\n" +
                    Loc.T("    Text = \"Bóc tách khối lượng...\",\n", "    Text = \"Quantity take-off...\",\n") +
                    "    CharsPerSecond = 26,\n" +
                    "    StartDelay = 0.25,\n" +
                    "    ShowCaret = true,\n" +
                    "    IsLooping = false\n" +
                    "};"));

        return root;
    }

    private object CreateTextRevealDemo()
    {
        var reveal = new TextReveal
        {
            Text = Loc.T("Kết cấu chính xác, thi công an toàn", "Precise structure, safe construction"),
            FontSize = 26,
            WordDelay = 0.08,
            WordDuration = 0.45,
            Margin = new Thickness(0, 0, 0, 12)
        };

        var again = new Button { Content = Loc.T("Chạy lại", "Replay"), Padding = new Thickness(12, 6, 12, 6), HorizontalAlignment = HorizontalAlignment.Left };
        again.SetResourceReference(FrameworkElement.StyleProperty, "CONCOST.Button.Secondary");
        again.Click += (_, _) =>
        {
            reveal.Restart();
        };

        var content = new StackPanel();
        content.Children.Add(reveal);
        content.Children.Add(again);

        var root = new StackPanel();
        root.Children.Add(DemoSection(
            Loc.T("Mặc định", "Default"),
            content,
            xaml: Loc.T("<ui:TextReveal Text=\"Kết cấu chính xác, thi công an toàn\"\n", "<ui:TextReveal Text=\"Precise structure, safe construction\"\n") +
                  "                FontSize=\"26\" WordDelay=\"0.08\" />",
            csharp: "var reveal = new TextReveal\n" +
                    "{\n" +
                    Loc.T("    Text = \"Kết cấu chính xác, thi công an toàn\",\n", "    Text = \"Precise structure, safe construction\",\n") +
                    Loc.T("    WordDelay = 0.08,     // giãn cách giữa các từ\n", "    WordDelay = 0.08,     // gap between words\n") +
                    Loc.T("    WordDuration = 0.45,  // thời gian mỗi từ hiện lên\n", "    WordDuration = 0.45,  // time each word takes to appear\n") +
                    "    OffsetY = 14\n" +
                    "};"));

        return root;
    }
}
