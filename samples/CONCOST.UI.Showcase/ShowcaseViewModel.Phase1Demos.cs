using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CONCOST.UI.Controls;

namespace CONCOST.UI.Showcase;

public partial class ShowcaseViewModel
{
    // =========================================================================
    // DEMO: Phase 1 — tilt card, magnetic hover, check mark, scroll progress
    // =========================================================================

    private object CreateTiltCardDemo()
    {
        var card = new TiltCard { Width = 280, HorizontalAlignment = HorizontalAlignment.Left };
        var inner = new StackPanel();
        var title = new TextBlock { Text = Loc.T("Dầm B1 — 300×600", "Beam B1 — 300×600"), FontSize = 14, FontWeight = FontWeights.SemiBold };
        title.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextPrimary");
        inner.Children.Add(title);
        inner.Children.Add(DemoCaption(Loc.T("Di chuột lên thẻ để thấy độ nghiêng + nhấc nhẹ.", "Hover the card to see the tilt and the lift."), 4));
        card.Content = inner;

        var root = new StackPanel();
        root.Children.Add(DemoSection(
            Loc.T("Mặc định", "Default"),
            card,
            xaml: "<ui:TiltCard Width=\"280\" MaxTilt=\"6\" Lift=\"1.02\"\n" +
                  "             Style=\"{DynamicResource CONCOST.TiltCard.Base}\">\n" +
                  "    <StackPanel>\n" +
                  Loc.T("        <TextBlock Text=\"Dầm B1 — 300×600\" FontWeight=\"SemiBold\" />\n", "        <TextBlock Text=\"Beam B1 — 300×600\" FontWeight=\"SemiBold\" />\n") +
                  Loc.T("        <TextBlock Text=\"Di chuột để thấy độ nghiêng\" Opacity=\"0.6\" />\n", "        <TextBlock Text=\"Hover to see the tilt\" Opacity=\"0.6\" />\n") +
                  "    </StackPanel>\n" +
                  "</ui:TiltCard>",
            csharp: Loc.T("// TiltCard dùng skew + scale (2.5D) nên chữ vẫn sắc nét, không tốn Viewport3D\n", "// TiltCard uses skew + scale (2.5D), so text stays crisp and no Viewport3D is needed\n") +
                    "var card = new TiltCard\n" +
                    "{\n" +
                    Loc.T("    MaxTilt = 6,     // độ nghiêng tối đa ở mép thẻ\n", "    MaxTilt = 6,     // maximum tilt at the card edge\n") +
                    Loc.T("    Lift = 1.02      // phóng nhẹ khi hover\n", "    Lift = 1.02      // slight zoom on hover\n") +
                    "};\n\n" +
                    Loc.T("// Muốn có quầng sáng theo chuột: bọc thêm SpotlightCard bên trong", "// Want a pointer glow as well: nest a SpotlightCard inside\n")));

        return root;
    }

    private object CreateMagneticHoverDemo()
    {
        var button = new Button
        {
            Content = Loc.T("Kéo tôi về phía con trỏ", "Pull me toward the pointer"),
            Padding = new Thickness(18, 8, 18, 8),
            HorizontalAlignment = HorizontalAlignment.Left
        };
        button.SetResourceReference(FrameworkElement.StyleProperty, "CONCOST.Button.Primary");
        MagneticHover.SetIsEnabled(button, true);
        MagneticHover.SetStrength(button, 0.3);
        MagneticHover.SetMaxOffset(button, 12);

        var root = new StackPanel();
        root.Children.Add(DemoSection(
            Loc.T("Mặc định", "Default"),
            button,
            xaml: Loc.T("<Button Content=\"Kéo tôi về phía con trỏ\"\n", "<Button Content=\"Pull me toward the pointer\"\n") +
                  "        attached:MagneticHover.IsEnabled=\"True\"\n" +
                  "        attached:MagneticHover.Strength=\"0.3\"\n" +
                  "        attached:MagneticHover.MaxOffset=\"12\" />",
            csharp: Loc.T("// Bật cho bất kỳ FrameworkElement nào\n", "// Enable it on any FrameworkElement\n") +
                    "MagneticHover.SetIsEnabled(button, true);\n" +
                    Loc.T("MagneticHover.SetStrength(button, 0.3);   // 0.1–0.4 là hợp lý\n", "MagneticHover.SetStrength(button, 0.3);   // 0.1–0.4 is sensible\n") +
                    Loc.T("MagneticHover.SetMaxOffset(button, 12);   // chặn trôi quá xa", "MagneticHover.SetMaxOffset(button, 12);   // cap how far it drifts")));

        return root;
    }

    private object CreateCheckMarkDemo()
    {
        var mark = new CheckMarkDraw { Width = 52, Height = 52, IsChecked = false };

        var button = new Button { Content = Loc.T("Duyệt / bỏ duyệt", "Approve / unapprove"), Padding = new Thickness(14, 6, 14, 6), Margin = new Thickness(0, 12, 0, 0), HorizontalAlignment = HorizontalAlignment.Left };
        button.SetResourceReference(FrameworkElement.StyleProperty, "CONCOST.Button.Secondary");
        button.Click += (_, _) => mark.IsChecked = !mark.IsChecked;

        var content = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
        content.Children.Add(mark);
        var text = new StackPanel { Margin = new Thickness(12, 0, 0, 0), VerticalAlignment = VerticalAlignment.Center };
        text.Children.Add(DemoCaption(Loc.T("Dấu ✓ tự vẽ nét trong 320ms", "The ✓ draws its own stroke in 320 ms")));
        text.Children.Add(button);
        content.Children.Add(text);

        var root = new StackPanel();
        root.Children.Add(DemoSection(
            Loc.T("Đã duyệt", "Approved"),
            content,
            xaml: "<ui:CheckMarkDraw Width=\"52\" Height=\"52\"\n" +
                  "                  IsChecked=\"{Binding IsApproved}\" />",
            csharp: Loc.T("// Đổi trạng thái là dấu tự vẽ / tự xoá\n", "// Changing the state draws or erases the tick\n") +
                    "ApprovalMark.IsChecked = true;\n\n" +
                    Loc.T("// Tắt animation khi cần\n", "// Turn the animation off when needed\n") +
                    "ApprovalMark.Duration = new Duration(TimeSpan.Zero);"));

        return root;
    }

    private object CreateScrollProgressDemo()
    {
        var scroller = new ScrollViewer
        {
            Height = 110,
            VerticalScrollBarVisibility = ScrollBarVisibility.Hidden,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled
        };
        scroller.Content = new Border { Height = 420 };
        (scroller.Content as Border)!.SetResourceReference(Border.BackgroundProperty, "CONCOST.Brush.SurfaceSecondary");

        var bar = new ScrollProgressBar { Target = scroller, Margin = new Thickness(0, 6, 0, 0) };

        var content = new StackPanel();
        content.Children.Add(bar);
        content.Children.Add(scroller);

        var root = new StackPanel();
        root.Children.Add(DemoSection(
            Loc.T("Mặc định", "Default"),
            content,
            xaml: "<ui:ScrollProgressBar Target=\"{Binding ElementName=PageScroll}\" />\n" +
                  "<ScrollViewer x:Name=\"PageScroll\">…</ScrollViewer>",
            csharp: Loc.T("// Không set Target: control tự tìm ScrollViewer cha gần nhất\n", "// No Target set: the control finds the nearest parent ScrollViewer\n") +
                    Loc.T("// (đặt thanh tiến độ BÊN TRONG ScrollViewer thì mới tự tìm được)\n\n", "// (put the progress bar INSIDE the ScrollViewer so it can find it)\n\n") +
                    Loc.T("bar.Target = PageScroll;      // hoặc gán tường minh\n", "bar.Target = PageScroll;      // or assign it explicitly\n") +
                    "bar.BarBrush = Brushes.Orange;\n" +
                    "bar.Thickness = 3;"));

        return root;
    }
}
