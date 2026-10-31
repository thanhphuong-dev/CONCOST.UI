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
            Text = "Bốn khối màu trôi quanh neo của nó",
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
            "Mesh gradient động",
            stage,
            xaml: "<controls:AnimatedMeshGradient Height=\"240\"\n" +
                  "    ColorA=\"#7F77DD\" ColorB=\"#1D9E75\"\n" +
                  "    ColorC=\"#EF9F27\" ColorD=\"#D4537E\"\n" +
                  "    Speed=\"1\" BlobOpacity=\"0.55\" />",
            csharp: "// Mỗi khối là một radial gradient trong suốt ở viền,\n" +
                    "// neo quanh bốn góc và chỉ \"thở\" quanh đó —\n" +
                    "// khác AuroraBackground (trôi ngang mặt phẳng).\n" +
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
            Text = "Gói chuyên nghiệp",
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
            Text = "Viền gradient xoay một vòng mỗi 8 giây.",
            FontSize = 11.5,
            TextWrapping = TextWrapping.Wrap
        };
        note.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");
        body.Children.Add(note);

        card.Content = body;

        var toggle = new Button
        {
            Content = "Dừng xoay",
            Padding = new Thickness(12, 5, 12, 5),
            Margin = new Thickness(0, 12, 0, 0),
            HorizontalAlignment = HorizontalAlignment.Left
        };
        toggle.SetResourceReference(FrameworkElement.StyleProperty, "CONCOST.Button.Secondary");
        toggle.Click += (_, _) =>
        {
            card.IsAnimated = !card.IsAnimated;
            toggle.Content = card.IsAnimated ? "Dừng xoay" : "Chạy tiếp";
        };

        var stack = new StackPanel();
        stack.Children.Add(card);
        stack.Children.Add(toggle);

        var root = new StackPanel();
        root.Children.Add(DemoSection(
            "Viền gradient xoay",
            stack,
            xaml: "<controls:AnimatedGradientBorder SecondsPerTurn=\"8\"\n" +
                  "    RingThickness=\"2\" Corner=\"14\">\n" +
                  "    <StackPanel>...</StackPanel>\n" +
                  "</controls:AnimatedGradientBorder>",
            csharp: "// Xoay bằng DoubleAnimation trên RotateTransform của brush —\n" +
                    "// chạy ở render thread, không tốn UI thread mỗi frame.\n" +
                    "card.IsAnimated = false;   // giữ nguyên frame đầu"));

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
            Text = "Kính mờ",
            FontSize = 16,
            FontWeight = FontWeights.SemiBold
        };
        heading.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextPrimary");
        text.Children.Add(heading);
        var hint = new TextBlock
        {
            Text = "Blur lấy đúng vùng nằm sau tấm kính, không kéo giãn cả nền.",
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
            "Kính mờ trên nền động",
            stage,
            xaml: "<Grid>\n" +
                  "    <controls:AnimatedMeshGradient x:Name=\"Backdrop\" />\n" +
                  "    <controls:FrostedGlass Source=\"{Binding ElementName=Backdrop}\"\n" +
                  "        BlurRadius=\"22\" Corner=\"16\" />\n" +
                  "</Grid>",
            csharp: "// WPF không có backdrop filter: trỏ Source vào phần tử phía sau.\n" +
                    "// Viewbox của VisualBrush được đặt theo toạ độ tấm kính trong Source,\n" +
                    "// nên blur khớp đúng vùng nằm sau nó."));

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
            Text = "Di chuột trong khung này",
            FontSize = 14,
            FontWeight = FontWeights.SemiBold,
            HorizontalAlignment = HorizontalAlignment.Center
        });
        content.Children.Add(new TextBlock
        {
            Text = "Overlay không nhận chuột nên không cướp click.",
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
            "Đèn rọi theo con trỏ",
            stage,
            xaml: "<Grid>\n" +
                  "    <StackPanel>...</StackPanel>\n" +
                  "    <controls:CursorSpotlight Radius=\"130\" SpotDim=\"0.62\" />\n" +
                  "</Grid>",
            csharp: "// Không hit-test, nhưng vẫn theo được con trỏ vì\n" +
                    "// Mouse.GetPosition đọc vị trí hiện tại, không cần nhận sự kiện.\n" +
                    "spotlight.FollowCursor = false;\n" +
                    "spotlight.SpotlightCenter = new Point(120, 80);"));

        return root;
    }

    private object CreateStaggerAnimationDemo()
    {
        var list = new ListBox { Height = 210, BorderThickness = new Thickness(0) };

        for (int i = 1; i <= 8; i++)
        {
            list.Items.Add($"Cấu kiện {i:00} — bê tông M{i % 3 + 2}50");
        }

        StaggerAnimation.SetIsEnabled(list, true);
        StaggerAnimation.SetDelay(list, 55);
        StaggerAnimation.SetOffsetY(list, 16);

        var replay = new Button
        {
            Content = "Chạy lại",
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
            "Danh sách hiện dần",
            stack,
            xaml: "<ListBox controls:StaggerAnimation.IsEnabled=\"True\"\n" +
                  "         controls:StaggerAnimation.Delay=\"55\"\n" +
                  "         controls:StaggerAnimation.OffsetY=\"16\" />",
            csharp: "// Trễ theo vị trí, chặn ở MaxItems để danh sách dài\n" +
                    "// không mất mười giây mới hiện xong.\n" +
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
            Text = "Nguồn",
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
            Text = "Đích — lớn ra từ ô bên trái",
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
            Content = "Chạy transition",
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
            xaml: "<!-- Không cần XAML: gọi khi đổi view -->",
            csharp: "var parent = (UIElement)VisualTreeHelper.GetParent(target);\n" +
                    "var origin = source.TranslatePoint(new Point(0, 0), parent);\n\n" +
                    "SharedElementTransition.Run(target, new Rect(origin, source.RenderSize));\n" +
                    "// hoặc gọn hơn:\n" +
                    "SharedElementTransition.RunFrom(target, source);"));

        return root;
    }
}
