using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using CONCOST.UI.Controls;

namespace CONCOST.UI.Showcase;

public partial class ShowcaseViewModel
{
    // =========================================================================
    // DEMO: the 1.3.0 batch - scheduler, kanban, viewers and editors
    // =========================================================================

    private Button DemoButton(string caption, RoutedEventHandler? onClick = null)
    {
        var button = new Button { Content = caption, Padding = new Thickness(12, 5, 12, 5), Margin = new Thickness(0, 0, 8, 10) };
        button.SetResourceReference(FrameworkElement.StyleProperty, "CONCOST.Button.Secondary");

        if (onClick is not null) button.Click += onClick;

        return button;
    }

    private TextBlock DemoCaption()
    {
        var text = new TextBlock { Margin = new Thickness(0, 0, 0, 6), TextWrapping = TextWrapping.Wrap };
        text.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");
        return text;
    }

    private object CreateSchedulerDemo()
    {
        var day = DateTime.Today;
        var bookings = new List<SchedulerItem>
        {
            new() { Title = "Giao ban công trường", Start = day.AddHours(7.5), End = day.AddHours(8.5), Resource = "Phòng họp" },
            new() { Title = "Duyệt bản vẽ kết cấu", Start = day.AddHours(9), End = day.AddHours(11), Resource = "Ban A" },
            new() { Title = "Kiểm tra hiện trường", Start = day.AddHours(10), End = day.AddHours(11.5), Resource = "Khu B" },
            new() { Title = "Họp chủ đầu tư", Start = day.AddHours(14), End = day.AddHours(15.5) },
            new() { Title = "Nghiệm thu cốt thép", Start = day.AddHours(16), End = day.AddHours(17) }
        };

        var scheduler = new Scheduler
        {
            Date = day,
            StartHour = 7,
            EndHour = 18,
            HourHeight = 44,
            Height = 460,
            ItemsSource = bookings
        };

        var status = DemoCaption();
        status.Text = "Chọn một lịch hẹn để xem chi tiết bên dưới.";

        scheduler.SelectionChanged += (_, e) =>
        {
            status.Text = e.Item is null
                ? "Chưa chọn lịch hẹn nào."
                : $"{e.Item.Start:HH:mm} - {e.Item.End:HH:mm}  {e.Item.Title}";
        };

        var actions = new WrapPanel();
        actions.Children.Add(DemoButton("Hôm trước", (_, _) => scheduler.ChangeDay(-1)));
        actions.Children.Add(DemoButton("Hôm nay", (_, _) => scheduler.ChangeDay(0)));
        actions.Children.Add(DemoButton("Hôm sau", (_, _) => scheduler.ChangeDay(1)));

        var root = new StackPanel();
        root.Children.Add(actions);
        root.Children.Add(status);
        root.Children.Add(DemoSection(
            "Hai cuộc hẹn trùng giờ chia đôi cột thay vì chồng lên nhau",
            scheduler,
            xaml: "<controls:Scheduler Date=\"{Binding SelectedDay}\"\n" +
                  "                    StartHour=\"7\" EndHour=\"18\"\n" +
                  "                    HourHeight=\"44\"\n" +
                  "                    ItemsSource=\"{Binding Bookings}\" />",
            csharp: "scheduler.SelectionChanged += (_, e) =>\n" +
                    "    details.Text = e.Item is null ? \"chưa chọn\" : e.Item.Title;\n\n" +
                    "scheduler.ChangeDay(-1);\n\n" +
                    "Rect bounds = scheduler.GetItemBounds(booking);   // Rect.Empty khi ngoài khung giờ"));

        return root;
    }

    private object CreateKanbanDemo()
    {
        var todo = new KanbanColumn { Header = "Cần làm", Maximum = 6 };
        var doing = new KanbanColumn { Header = "Đang làm", Accent = new SolidColorBrush(Color.FromRgb(0xF5, 0x9E, 0x0B)) };
        var done = new KanbanColumn { Header = "Hoàn tất", Accent = new SolidColorBrush(Color.FromRgb(0x22, 0xC5, 0x5E)) };

        todo.Items.Add(new KanbanCard { Title = "Mặt bằng tầng 3", Chip = "Kiến trúc" });
        todo.Items.Add(new KanbanCard { Title = "Thống kê thép móng", Chip = "Kết cấu" });
        doing.Items.Add(new KanbanCard { Title = "Chi tiết cầu thang", Chip = "Kết cấu" });
        doing.Items.Add(new KanbanCard { Title = "Bảng tiến độ tháng 3", Chip = "Quản lý" });
        done.Items.Add(new KanbanCard { Title = "Khảo sát địa hình", Chip = "Hiện trường", IsDone = true });

        var board = new KanbanBoard { Height = 400 };
        board.Columns.Add(todo);
        board.Columns.Add(doing);
        board.Columns.Add(done);

        var status = DemoCaption();
        status.Text = "Kéo một thẻ sang cột khác, hoặc dùng nút bên dưới.";

        board.CardMoved += (_, e) =>
            status.Text = $"Đã chuyển \"{e.Card.Title}\": {e.FromColumn?.Header} -> {e.ToColumn.Header} (vị trí {e.ToIndex})";

        var actions = new WrapPanel();
        actions.Children.Add(DemoButton("Chuyển thẻ đầu tiên sang Hoàn tất", (_, _) =>
        {
            if (todo.Items.Count == 0) return;

            if (todo.Items[0] is KanbanCard card) board.MoveCard(card, done, done.Items.Count);
        }));
        actions.Children.Add(DemoButton("Xáo thứ tự cột Đang làm", (_, _) =>
        {
            if (doing.Items.Count < 2) return;

            if (doing.Items[0] is KanbanCard card) board.MoveCard(card, doing, doing.Items.Count);
        }));

        var root = new StackPanel();
        root.Children.Add(actions);
        root.Children.Add(status);
        root.Children.Add(DemoSection(
            "Cột, thẻ và giới hạn",
            board,
            xaml: "<controls:KanbanBoard>\n" +
                  "    <controls:KanbanColumn Header=\"Cần làm\" Maximum=\"6\">\n" +
                  "        <controls:KanbanCard Title=\"Mặt bằng tầng 3\" Chip=\"Kiến trúc\" />\n" +
                  "    </controls:KanbanColumn>\n" +
                  "    <controls:KanbanColumn Header=\"Đang làm\" Accent=\"#F59E0B\" />\n" +
                  "</controls:KanbanBoard>",
            csharp: "board.CardMoved += (_, e) =>\n" +
                    "    taskService.Move(e.Card.Title, e.ToColumn.Header, e.ToIndex);\n\n" +
                    "if (board.MoveCard(card, doneColumn, 0))\n" +
                    "    status.Text = \"đã chuyển\";"));

        return root;
    }

    /// <summary>A tiny floor plan drawn offscreen, so the viewer has a real sheet to zoom into.</summary>
    private static ImageSource CreatePlanSheet()
    {
        const int width = 760;
        const int height = 520;

        var visual = new DrawingVisual();

        using (DrawingContext dc = visual.RenderOpen())
        {
            dc.DrawRectangle(new SolidColorBrush(Color.FromRgb(0xFA, 0xFB, 0xFC)), null, new Rect(0, 0, width, height));

            var wall = new Pen(new SolidColorBrush(Color.FromRgb(0x1F, 0x3B, 0x57)), 3);
            var thin = new Pen(new SolidColorBrush(Color.FromRgb(0x9A, 0xA5, 0xB1)), 1);
            var accent = new Pen(new SolidColorBrush(Color.FromRgb(0x2B, 0x6C, 0xB0)), 2);

            // Grid ticks every 40 pixels behind the plan.
            for (int x = 40; x < width; x += 40) dc.DrawLine(thin, new Point(x, 20), new Point(x, height - 20));
            for (int y = 40; y < height; y += 40) dc.DrawLine(thin, new Point(20, y), new Point(width - 20, y));

            dc.DrawRectangle(null, wall, new Rect(60, 60, 640, 400));
            dc.DrawLine(wall, new Point(360, 60), new Point(360, 300));
            dc.DrawLine(wall, new Point(60, 300), new Point(700, 300));
            dc.DrawLine(wall, new Point(520, 300), new Point(520, 460));

            // A column and a slab hatch, just enough to read as a drawing.
            dc.DrawRectangle(null, accent, new Rect(340, 280, 40, 40));
            for (int i = 0; i < 12; i++)
            {
                dc.DrawLine(thin, new Point(70 + i * 24, 310), new Point(90 + i * 24, 450));
            }

            var label = new FormattedText("MẶT BẰNG TẦNG 3", System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight, new Typeface("Segoe UI"), 18,
                new SolidColorBrush(Color.FromRgb(0x1F, 0x3B, 0x57)), 1.0);

            dc.DrawText(label, new Point(80, 74));
        }

        var bitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(visual);
        bitmap.Freeze();

        return bitmap;
    }

    private object CreateImageViewerDemo()
    {
        var viewer = new ImageViewer { Source = CreatePlanSheet(), Height = 380, MinZoom = 0.25, MaxZoom = 8 };

        var status = DemoCaption();
        status.Text = "Lăn chuột để zoom tại con trỏ, giữ chuột trái để kéo.";

        viewer.MouseWheel += (_, e) => viewer.ZoomAt(e.GetPosition(viewer), e.Delta > 0 ? 1.2 : 1 / 1.2);

        var actions = new WrapPanel();
        actions.Children.Add(DemoButton("Vừa khung", (_, _) => { viewer.Fit(); status.Text = $"Zoom {viewer.Zoom:P0}"; }));
        actions.Children.Add(DemoButton("100%", (_, _) => { viewer.Reset(); status.Text = "Zoom 100%"; }));
        actions.Children.Add(DemoButton("Xoay 90°", (_, _) => { viewer.RotateClockwise(); status.Text = $"Góc {viewer.Rotation}°"; }));

        var root = new StackPanel();
        root.Children.Add(actions);
        root.Children.Add(status);
        root.Children.Add(DemoSection(
            "Zoom giữ điểm dưới con trỏ đứng yên",
            viewer,
            xaml: "<controls:ImageViewer Source=\"{Binding SheetImage}\"\n" +
                  "                    MinZoom=\"0.25\" MaxZoom=\"8\" />",
            csharp: "viewer.ZoomAt(e.GetPosition(viewer), e.Delta > 0 ? 1.2 : 1 / 1.2);\n\n" +
                    "viewer.Fit();\n" +
                    "viewer.RotateClockwise();"));

        return root;
    }

    private object CreateCodeEditorDemo()
    {
        const string sample =
            "// Bảng khối lượng thép cho dầm D1\n" +
            "public sealed class RebarSchedule\n" +
            "{\n" +
            "    public decimal Weight(string grade, double diameter, double length)\n" +
            "    {\n" +
            "        const double Density = 7850;               // kg/m3\n" +
            "        double area = Math.PI * diameter * diameter / 4e6;\n" +
            "        return (decimal)(area * length * Density) * (grade == \"CB400\" ? 1.02 : 1.0);\n" +
            "    }\n" +
            "}";

        var editor = new CodeEditor
        {
            Text = sample,
            Syntax = CodeLanguage.CSharp,
            TabSize = 4,
            Height = 320
        };

        var status = DemoCaption();
        status.Text = $"Dòng: {editor.LineCount}. Dùng nút bên dưới để nhảy tới dòng 6.";

        var actions = new WrapPanel();
        actions.Children.Add(DemoButton("Tới dòng 6", (_, _) => { editor.GoToLine(6); status.Text = $"Dòng {editor.CaretLine}"; }));
        actions.Children.Add(DemoButton("Sang SQL", (_, _) => editor.Syntax = CodeLanguage.Sql));
        actions.Children.Add(DemoButton("Sang XAML", (_, _) => editor.Syntax = CodeLanguage.Xaml));
        actions.Children.Add(DemoButton("Tắt tô màu", (_, _) => editor.HighlightSyntax = false));
        actions.Children.Add(DemoButton("Bật tô màu", (_, _) => editor.HighlightSyntax = true));

        var root = new StackPanel();
        root.Children.Add(actions);
        root.Children.Add(status);
        root.Children.Add(DemoSection(
            "Tô màu theo ngôn ngữ, số dòng và Tab",
            editor,
            xaml: "<controls:CodeEditor Syntax=\"CSharp\"\n" +
                  "                   Text=\"{Binding ScriptText, UpdateSourceTrigger=PropertyChanged}\"\n" +
                  "                   TabSize=\"4\" ShowLineNumbers=\"True\" />",
            csharp: "foreach (var token in CodeHighlighter.Tokenize(code, CodeLanguage.Sql))\n" +
                    "    Debug.WriteLine(token.Kind);\n\n" +
                    "editor.GoToLine(42);\n" +
                    "editor.FindNext(\"Rebar\");"));

        return root;
    }

    private object CreateMarkdownViewerDemo()
    {
        const string markdown =
            "# Biên bản nghiệm thu\n" +
            "\n" +
            "**Công trình:** Chung cư An Phú - Lô B\n" +
            "**Ngày:** 01/11/2026\n" +
            "\n" +
            "## Nội dung kiểm tra\n" +
            "\n" +
            "1. Cao độ đáy móng so với thiết kế\n" +
            "2. Đường kính và khoảng cách cốt thép\n" +
            "3. Lớp bê tông bảo vệ\n" +
            "\n" +
            "> Sai lệch cho phép theo TCVN 4453:2012 là ±10 mm.\n" +
            "\n" +
            "```\n" +
            "D1: 8Ø20 + Ø8a150\n" +
            "D2: 6Ø18 + Ø8a200\n" +
            "```\n" +
            "\n" +
            "Chi tiết xem tại [tiêu chuẩn nghiệm thu](https://example.com/tcvn4453).\n" +
            "\n" +
            "---\n" +
            "\n" +
            "Kết luận: **đạt** yêu cầu nghiệm thu.";

        var viewer = new MarkdownViewer { Markdown = markdown, Height = 380 };

        var status = DemoCaption();
        status.Text = $"{viewer.LinkCount} liên kết trong tài liệu.";

        var root = new StackPanel();
        root.Children.Add(status);
        root.Children.Add(DemoSection(
            "Markdown thành FlowDocument",
            viewer,
            xaml: "<controls:MarkdownViewer Markdown=\"{Binding ReleaseNotes}\" />",
            csharp: "var document = MarkdownParser.Parse(text);\n" +
                    "viewer.Markdown = text;      // tự rebuild\n" +
                    "int links = viewer.LinkCount;"));

        return root;
    }

    private object CreateCoachMarkDemo()
    {
        var host = new Grid { Height = 320 };

        var target = new Button
        {
            Content = "Lưu bản vẽ",
            Width = 120,
            Height = 34,
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(24, 24, 0, 0)
        };

        target.SetResourceReference(FrameworkElement.StyleProperty, "CONCOST.Button.Primary");
        host.Children.Add(target);

        var mark = new CoachMark
        {
            Target = target,
            Title = "Nút lưu nằm ở đây",
            StepIndex = 1,
            StepCount = 3,
            Placement = CoachMarkPlacement.Below
        };

        host.Children.Add(mark);

        var status = DemoCaption();
        status.Text = "Thẻ tự lật lên trên khi target sát đáy.";

        mark.Next += (_, _) => status.Text = "Next: host chuyển sang control kế tiếp.";
        mark.Dismissed += (_, _) => status.Text = "Đã đóng hướng dẫn.";

        var actions = new WrapPanel();
        actions.Children.Add(DemoButton("Hiện lại", (_, _) => { mark.Visibility = Visibility.Visible; status.Text = "Đang hướng dẫn."; }));
        actions.Children.Add(DemoButton("Bước tiếp", (_, _) => mark.Advance()));

        var root = new StackPanel();
        root.Children.Add(actions);
        root.Children.Add(status);
        root.Children.Add(DemoSection(
            "Thẻ chỉ dẫn bám theo target",
            host,
            xaml: "<Grid>\n" +
                  "    <Button x:Name=\"saveButton\" Content=\"Lưu bản vẽ\" />\n" +
                  "    <controls:CoachMark Target=\"{Binding ElementName=saveButton}\"\n" +
                  "                        Title=\"Nút lưu nằm ở đây\"\n" +
                  "                        StepIndex=\"1\" StepCount=\"3\" />\n" +
                  "</Grid>",
            csharp: "mark.Next += (_, _) => ShowStep(++index);\n" +
                    "mark.Dismissed += (_, _) => overlay.Children.Clear();"));

        return root;
    }

    private object CreateMiniMapDemo()
    {
        var items = new List<MiniMapItem>
        {
            new() { Bounds = new Rect(400, 600, 5200, 3200) },
            new() { Bounds = new Rect(1800, 1200, 1600, 1200) },
            new() { Bounds = new Rect(6200, 800, 4200, 2600) },
            new() { Bounds = new Rect(2400, 4400, 5400, 2800) }
        };

        var map = new MiniMap
        {
            Width = 260,
            Height = 180,
            HorizontalAlignment = HorizontalAlignment.Left,
            ContentBounds = new Rect(0, 0, 12000, 8000),
            Viewport = new Rect(3000, 2400, 3600, 2400),
            ItemsSource = items
        };

        var status = DemoCaption();
        status.Text = "Click hoặc kéo trong bản đồ để dời khung nhìn.";

        map.ViewportChanged += (_, e) => status.Text = $"Khung nhìn: {e.Viewport.X:0} , {e.Viewport.Y:0} — {e.Viewport.Width:0}×{e.Viewport.Height:0}";

        var actions = new WrapPanel();
        actions.Children.Add(DemoButton("Giữa công trình", (_, _) => map.CentreOn(new Point(6000, 4000))));
        actions.Children.Add(DemoButton("Thu nhỏ khung nhìn", (_, _) => map.Viewport = new Rect(4000, 3200, 1800, 1200)));

        var root = new StackPanel();
        root.Children.Add(actions);
        root.Children.Add(status);
        root.Children.Add(DemoSection(
            "Bản đồ thu nhỏ và khung nhìn",
            map,
            xaml: "<controls:MiniMap ContentBounds=\"0,0,12000,8000\"\n" +
                  "                Viewport=\"{Binding CameraRect}\"\n" +
                  "                ItemsSource=\"{Binding FloorPlates}\" />",
            csharp: "map.Fit();\n" +
                    "Point here = map.WorldToMap(new Point(2400, 1800));\n" +
                    "map.CentreOn(new Point(6000, 4000));"));

        return root;
    }

    private object CreateModelViewer3DDemo()
    {
        var viewer = new ModelViewer3D { Height = 360, ShowAxes = true, Zoom = 1.1 };
        viewer.LoadBox(new Point3D(0, 0, 0), 12, 8, 5);

        var status = DemoCaption();
        status.Text = "Kéo để orbit, lăn chuột để zoom.";

        var actions = new WrapPanel();
        actions.Children.Add(DemoButton("Góc mặc định", (_, _) => { viewer.ResetView(); status.Text = "Đã đặt lại góc nhìn."; }));
        actions.Children.Add(DemoButton("Xoay trái", (_, _) => { viewer.Orbit(-15, 0); status.Text = $"Yaw {viewer.Yaw:0}°"; }));
        actions.Children.Add(DemoButton("Xoay phải", (_, _) => { viewer.Orbit(15, 0); status.Text = $"Yaw {viewer.Yaw:0}°"; }));

        var root = new StackPanel();
        root.Children.Add(actions);
        root.Children.Add(status);
        root.Children.Add(DemoSection(
            "Khung dây 3D chiếu phối cảnh",
            viewer,
            xaml: "<controls:ModelViewer3D Yaw=\"-35\" Pitch=\"-20\"\n" +
                  "                    Zoom=\"1.1\" ShowAxes=\"True\" />",
            csharp: "viewer.LoadBox(new Point3D(0, 0, 0), 12, 8, 5);\n" +
                    "viewer.SetMesh(points, edges);\n" +
                    "viewer.Project(point, out Point screen);"));

        return root;
    }

    private object CreateSplashScreenDemo()
    {
        var splash = new CONCOST.UI.Controls.SplashScreen
        {
            Width = 460,
            Height = 300,
            HorizontalAlignment = HorizontalAlignment.Left,
            AppName = "CONCOST.UI",
            Version = "1.3.0",
            Tagline = "Bộ control WPF cho ứng dụng xây dựng",
            Status = "Đang nạp tài nguyên...",
            Progress = 20,
            Logo = CreatePlanSheet()
        };

        var status = DemoCaption();
        status.Text = "Bấm Tiến trình để thấy thanh chạy.";

        splash.Dismissed += (_, _) => status.Text = "Đã đóng - gọi Show() để hiện lại.";

        var actions = new WrapPanel();
        actions.Children.Add(DemoButton("Tiến trình +20", (_, _) =>
        {
            double next = splash.Progress + 20;
            splash.Report(next, next >= 100 ? "Hoàn tất khởi động" : "Đang nạp tài nguyên...");
            status.Text = $"Tiến trình: {splash.Progress:0}%";
        }));
        actions.Children.Add(DemoButton("Chờ không rõ", (_, _) => { splash.IsIndeterminate = true; status.Text = "Chế độ chờ."; }));
        actions.Children.Add(DemoButton("Đóng", (_, _) => splash.Dismiss()));
        actions.Children.Add(DemoButton("Hiện lại", (_, _) => splash.Show()));

        var root = new StackPanel();
        root.Children.Add(actions);
        root.Children.Add(status);
        root.Children.Add(DemoSection(
            "Thẻ khởi động có tiến trình",
            splash,
            xaml: "<controls:SplashScreen AppName=\"CONCOST.UI\"\n" +
                  "                   Version=\"1.3.0\"\n" +
                  "                   Status=\"Đang nạp tài nguyên...\"\n" +
                  "                   Progress=\"20\" />",
            csharp: "splash.Report(65, \"Đang nạp bản vẽ\");\n" +
                    "splash.IsIndeterminate = true;\n" +
                    "splash.Dismiss();      // hoặc Dismiss(false) khi tắt animation"));

        return root;
    }
}
