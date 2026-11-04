using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CONCOST.UI.Controls;
using CONCOST.UI.Icons;

namespace CONCOST.UI.Showcase;

/// <summary>Demos for the phase 5 effects: mesh gradient, ring border, glass, spotlight, stagger, shared element.</summary>
public partial class ShowcaseViewModel
{
    private object CreateAnimatedMeshGradientDemo()
    {
        var mesh = new AnimatedMeshGradient { Height = 240 };

        var title = new TextBlock
        {
            Text = Loc.T("Bốn khối màu trôi quanh neo của nó", "Four colour blobs drifting around its anchor"),
            FontSize = 15,
            FontWeight = FontWeights.SemiBold,
            Foreground = Brushes.White
        };

        // A dark pill: white text directly on pastel blobs is unreadable at any size.
        var pill = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(0x99, 0x1A, 0x1A, 0x1A)),
            CornerRadius = new CornerRadius(20),
            Padding = new Thickness(16, 8, 16, 8),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Child = title
        };

        var stage = new Grid { Height = 240 };
        stage.Children.Add(mesh);
        stage.Children.Add(pill);

        var root = new StackPanel();
        root.Children.Add(DemoSection(
            Loc.T("Mesh gradient động", "Animated mesh gradient"),
            stage,
            xaml: "<controls:AnimatedMeshGradient Height=\"240\"\n" +
                  "    ColorA=\"#7F77DD\" ColorB=\"#1D9E75\"\n" +
                  "    ColorC=\"#EF9F27\" ColorD=\"#D4537E\"\n" +
                  "    Speed=\"1\" BlobOpacity=\"0.55\" />",
            csharp: Loc.T("// Mỗi khối là một radial gradient trong suốt ở viền,\n", "// Each blob is a transparent radial gradient at the edge,\n") +
                    Loc.T("// neo quanh bốn góc và chỉ \"thở\" quanh đó —\n", "// anchored to the four corners, and only \"breathes\" there —\n") +
                    Loc.T("// khác AuroraBackground (trôi ngang mặt phẳng).\n", "// unlike AuroraBackground (which drifts across the plane).\n") +
                    "mesh.Restart();"));

        return root;
    }

    private object CreateAnimatedGradientBorderDemo()
    {
        var card = new AnimatedGradientBorder
        {
            Width = 320,
            HorizontalAlignment = HorizontalAlignment.Left,
            Corner = new CornerRadius(14)
        };

        var body = new StackPanel();
        body.Children.Add(new TextBlock
        {
            Text = Loc.T("Gói chuyên nghiệp", "Professional plan"),
            FontSize = 13,
            FontWeight = FontWeights.SemiBold
        });

        var price = new TextBlock
        {
            Text = "2.490.000 ₫",
            FontSize = 24,
            FontWeight = FontWeights.SemiBold,
            Margin = new Thickness(0, 6, 0, 4)
        };
        body.Children.Add(price);

        var note = new TextBlock
        {
            Text = Loc.T("Viền gradient xoay một vòng mỗi 8 giây.", "The gradient border rotates once every 8 seconds."),
            FontSize = 11.5,
            TextWrapping = TextWrapping.Wrap
        };
        note.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");
        body.Children.Add(note);

        card.Content = body;

        var toggle = new Button
        {
            Content = Loc.T("Dừng xoay", "Pause rotation"),
            Padding = new Thickness(12, 5, 12, 5),
            Margin = new Thickness(0, 12, 0, 0),
            HorizontalAlignment = HorizontalAlignment.Left
        };
        toggle.SetResourceReference(FrameworkElement.StyleProperty, "CONCOST.Button.Secondary");
        toggle.Click += (_, _) =>
        {
            card.IsAnimated = !card.IsAnimated;
            toggle.Content = card.IsAnimated ? Loc.T("Dừng xoay", "Pause rotation") : Loc.T("Chạy tiếp", "Resume");
        };

        var stack = new StackPanel();
        stack.Children.Add(card);
        stack.Children.Add(toggle);

        var root = new StackPanel();
        root.Children.Add(DemoSection(
            Loc.T("Viền gradient xoay", "Rotating gradient border"),
            stack,
            xaml: "<controls:AnimatedGradientBorder SecondsPerTurn=\"8\"\n" +
                  "    RingThickness=\"2\" Corner=\"14\">\n" +
                  "    <StackPanel>...</StackPanel>\n" +
                  "</controls:AnimatedGradientBorder>",
            csharp: Loc.T("// Xoay bằng DoubleAnimation trên RotateTransform của brush —\n", "// Rotation is a DoubleAnimation on the brush RotateTransform —\n") +
                    Loc.T("// chạy ở render thread, không tốn UI thread mỗi frame.\n", "// it runs on the render thread, costing the UI thread nothing per frame.\n") +
                    Loc.T("card.IsAnimated = false;   // giữ nguyên frame đầu", "card.IsAnimated = false;   // hold the first frame")));

        return root;
    }

    private object CreateFrostedGlassDemo()
    {
        var backdrop = new AnimatedMeshGradient { Height = 260 };

        var glass = new FrostedGlass
        {
            Source = backdrop,
            Width = 300,
            Height = 150,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Corner = new CornerRadius(16),
            BlurRadius = 22,
            Tint = new SolidColorBrush(Color.FromArgb(0x66, 0xFF, 0xFF, 0xFF))
        };

        var text = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
        var heading = new TextBlock
        {
            Text = Loc.T("Kính mờ", "Frosted glass"),
            FontSize = 16,
            FontWeight = FontWeights.SemiBold
        };
        heading.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextPrimary");
        text.Children.Add(heading);
        var hint = new TextBlock
        {
            Text = Loc.T("Blur lấy đúng vùng nằm sau tấm kính, không kéo giãn cả nền.", "The blur samples exactly the area behind the pane instead of stretching the whole backdrop."),
            FontSize = 11.5,
            Margin = new Thickness(0, 6, 0, 0),
            TextWrapping = TextWrapping.Wrap
        };
        hint.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextPrimary");
        text.Children.Add(hint);

        glass.Content = text;

        var stage = new Grid { Height = 260 };
        stage.Children.Add(backdrop);
        stage.Children.Add(glass);

        var root = new StackPanel();
        root.Children.Add(DemoSection(
            Loc.T("Kính mờ trên nền động", "Frosted glass over a moving background"),
            stage,
            xaml: "<Grid>\n" +
                  "    <controls:AnimatedMeshGradient x:Name=\"Backdrop\" />\n" +
                  "    <controls:FrostedGlass Source=\"{Binding ElementName=Backdrop}\"\n" +
                  "        BlurRadius=\"22\" Corner=\"16\" />\n" +
                  "</Grid>",
            csharp: Loc.T("// WPF không có backdrop filter: trỏ Source vào phần tử phía sau.\n", "// WPF has no backdrop filter: point Source at the element behind.\n") +
                    Loc.T("// Viewbox của VisualBrush được đặt theo toạ độ tấm kính trong Source,\n", "// The VisualBrush Viewbox is set from the pane's coordinates inside Source,\n") +
                    Loc.T("// nên blur khớp đúng vùng nằm sau nó.", "// so the blur lines up with the area behind it.")));

        return root;
    }

    private object CreateCursorSpotlightDemo()
    {
        var spotlight = new CursorSpotlight { Radius = 130, SpotDim = 0.62 };

        var content = new StackPanel
        {
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        content.Children.Add(new TextBlock
        {
            Text = Loc.T("Di chuột trong khung này", "Move the pointer inside this frame"),
            FontSize = 14,
            FontWeight = FontWeights.SemiBold,
            HorizontalAlignment = HorizontalAlignment.Center
        });
        content.Children.Add(new TextBlock
        {
            Text = Loc.T("Overlay không nhận chuột nên không cướp click.", "The overlay is not hit-testable, so it never steals a click."),
            FontSize = 11.5,
            Margin = new Thickness(0, 6, 0, 0),
            HorizontalAlignment = HorizontalAlignment.Center
        });

        var stage = new Grid { Height = 220 };
        stage.SetResourceReference(Panel.BackgroundProperty, "CONCOST.Brush.SurfaceSecondary");
        stage.Children.Add(content);
        stage.Children.Add(spotlight);

        var root = new StackPanel();
        root.Children.Add(DemoSection(
            Loc.T("Đèn rọi theo con trỏ", "Cursor spotlight"),
            stage,
            xaml: "<Grid>\n" +
                  "    <StackPanel>...</StackPanel>\n" +
                  "    <controls:CursorSpotlight Radius=\"130\" SpotDim=\"0.62\" />\n" +
                  "</Grid>",
            csharp: Loc.T("// Không hit-test, nhưng vẫn theo được con trỏ vì\n", "// Not hit-testable, yet it still tracks the pointer because\n") +
                    Loc.T("// Mouse.GetPosition đọc vị trí hiện tại, không cần nhận sự kiện.\n", "// Mouse.GetPosition reads the current position without needing events.\n") +
                    "spotlight.FollowCursor = false;\n" +
                    "spotlight.SpotlightCenter = new Point(120, 80);"));

        return root;
    }

    private object CreateStaggerAnimationDemo()
    {
        var list = new ListBox { Height = 210, BorderThickness = new Thickness(0) };

        for (int i = 1; i <= 8; i++)
        {
            list.Items.Add(Loc.F("Cấu kiện {0:00} — bê tông M{1}50", "Member {0:00} — concrete M{1}50", i, i % 3 + 2));
        }

        StaggerAnimation.SetIsEnabled(list, true);
        StaggerAnimation.SetDelay(list, 55);
        StaggerAnimation.SetOffsetY(list, 16);

        var replay = new Button
        {
            Content = Loc.T("Chạy lại", "Replay"),
            Padding = new Thickness(12, 5, 12, 5),
            Margin = new Thickness(0, 10, 0, 0),
            HorizontalAlignment = HorizontalAlignment.Left
        };
        replay.SetResourceReference(FrameworkElement.StyleProperty, "CONCOST.Button.Secondary");

        replay.Click += (_, _) =>
        {
            // Re-adding the items gives the containers a fresh entrance.
            var items = list.Items.Cast<object>().ToList();
            list.Items.Clear();
            foreach (var item in items) list.Items.Add(item);
        };

        var stack = new StackPanel();
        stack.Children.Add(list);
        stack.Children.Add(replay);

        var root = new StackPanel();
        root.Children.Add(DemoSection(
            Loc.T("Danh sách hiện dần", "List revealing in sequence"),
            stack,
            xaml: "<ListBox controls:StaggerAnimation.IsEnabled=\"True\"\n" +
                  "         controls:StaggerAnimation.Delay=\"55\"\n" +
                  "         controls:StaggerAnimation.OffsetY=\"16\" />",
            csharp: Loc.T("// Trễ theo vị trí, chặn ở MaxItems để danh sách dài\n", "// Delay by index, capped at MaxItems so a long list\n") +
                    Loc.T("// không mất mười giây mới hiện xong.\n", "// does not take ten seconds to finish.\n") +
                    "StaggerAnimation.SetMaxItems(list, 24);"));

        return root;
    }

    private object CreateSharedElementTransitionDemo()
    {
        var source = new Border
        {
            Width = 96,
            Height = 96,
            CornerRadius = new CornerRadius(10),
            VerticalAlignment = VerticalAlignment.Top,
            HorizontalAlignment = HorizontalAlignment.Left
        };
        source.SetResourceReference(Border.BackgroundProperty, "CONCOST.Brush.SurfaceSecondary");

        var sourceText = new TextBlock
        {
            Text = Loc.T("Nguồn", "Source"),
            FontSize = 11.5,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        sourceText.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");
        source.Child = sourceText;

        var target = new Border
        {
            Width = 260,
            Height = 130,
            CornerRadius = new CornerRadius(12),
            Margin = new Thickness(24, 0, 0, 0),
            VerticalAlignment = VerticalAlignment.Top,
            HorizontalAlignment = HorizontalAlignment.Left
        };
        target.SetResourceReference(Border.BackgroundProperty, "CONCOST.Brush.Primary");

        var targetText = new TextBlock
        {
            Text = Loc.T("Đích — lớn ra từ ô bên trái", "Target — grows out of the box on the left"),
            FontSize = 12,
            Foreground = Brushes.White,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        target.Child = targetText;

        var row = new StackPanel { Orientation = Orientation.Horizontal };
        row.Children.Add(source);
        row.Children.Add(target);

        var run = new Button
        {
            Content = Loc.T("Chạy transition", "Run transition"),
            Padding = new Thickness(12, 5, 12, 5),
            Margin = new Thickness(0, 12, 0, 0),
            HorizontalAlignment = HorizontalAlignment.Left
        };
        run.SetResourceReference(FrameworkElement.StyleProperty, "CONCOST.Button.Secondary");

        run.Click += (_, _) =>
        {
            var parent = (UIElement)VisualTreeHelper.GetParent(target);
            var origin = source.TranslatePoint(new Point(0, 0), parent);
            SharedElementTransition.Run(target, new Rect(origin, source.RenderSize));
        };

        var stack = new StackPanel();
        stack.Children.Add(row);
        stack.Children.Add(run);

        var root = new StackPanel();
        root.Children.Add(DemoSection(
            "Shared element",
            stack,
            xaml: Loc.T("<!-- Không cần XAML: gọi khi đổi view -->", "<!-- No XAML needed: call it when the view changes -->"),
            csharp: "var parent = (UIElement)VisualTreeHelper.GetParent(target);\n" +
                    "var origin = source.TranslatePoint(new Point(0, 0), parent);\n\n" +
                    "SharedElementTransition.Run(target, new Rect(origin, source.RenderSize));\n" +
                    Loc.T("// hoặc gọn hơn:\n", "// or, more concisely:\n") +
                    "SharedElementTransition.RunFrom(target, source);"));

        return root;
    }
}
