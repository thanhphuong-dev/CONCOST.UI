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
            new() { Title = Loc.T("Giao ban công trường", "Site handover meeting"), Start = day.AddHours(7.5), End = day.AddHours(8.5), Resource = Loc.T("Phòng họp", "Meeting room") },
            new() { Title = Loc.T("Duyệt bản vẽ kết cấu", "Review structural drawings"), Start = day.AddHours(9), End = day.AddHours(11), Resource = "Ban A" },
            new() { Title = Loc.T("Kiểm tra hiện trường", "Site inspection"), Start = day.AddHours(10), End = day.AddHours(11.5), Resource = "Khu B" },
            new() { Title = Loc.T("Họp chủ đầu tư", "Client meeting"), Start = day.AddHours(14), End = day.AddHours(15.5) },
            new() { Title = Loc.T("Nghiệm thu cốt thép", "Rebar acceptance"), Start = day.AddHours(16), End = day.AddHours(17) }
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
        status.Text = Loc.T("Chọn một lịch hẹn để xem chi tiết bên dưới.", "Select an appointment to see its details below.");

        scheduler.SelectionChanged += (_, e) =>
        {
            status.Text = e.Item is null
                ? Loc.T("Chưa chọn lịch hẹn nào.", "No appointment selected.")
                : $"{e.Item.Start:HH:mm} - {e.Item.End:HH:mm}  {e.Item.Title}";
        };

        var actions = new WrapPanel();
        actions.Children.Add(DemoButton(Loc.T("Hôm trước", "Previous day"), (_, _) => scheduler.ChangeDay(-1)));
        actions.Children.Add(DemoButton(Loc.T("Hôm nay", "Today"), (_, _) => scheduler.ChangeDay(0)));
        actions.Children.Add(DemoButton(Loc.T("Hôm sau", "Next day"), (_, _) => scheduler.ChangeDay(1)));

        var root = new StackPanel();
        root.Children.Add(actions);
        root.Children.Add(status);
        root.Children.Add(DemoSection(
            Loc.T("Hai cuộc hẹn trùng giờ chia đôi cột thay vì chồng lên nhau", "Two overlapping appointments split the column instead of stacking"),
            scheduler,
            xaml: "<controls:Scheduler Date=\"{Binding SelectedDay}\"\n" +
                  "                    StartHour=\"7\" EndHour=\"18\"\n" +
                  "                    HourHeight=\"44\"\n" +
                  "                    ItemsSource=\"{Binding Bookings}\" />",
            csharp: "scheduler.SelectionChanged += (_, e) =>\n" +
                    Loc.T("    details.Text = e.Item is null ? \"chưa chọn\" : e.Item.Title;\n\n", "    details.Text = e.Item is null ? \"nothing selected\" : e.Item.Title;\n\n") +
                    "scheduler.ChangeDay(-1);\n\n" +
                    Loc.T("Rect bounds = scheduler.GetItemBounds(booking);   // Rect.Empty khi ngoài khung giờ", "Rect bounds = scheduler.GetItemBounds(booking);   // Rect.Empty outside the visible range")));

        return root;
    }

    private object CreateKanbanDemo()
    {
        var todo = new KanbanColumn { Header = Loc.T("Cần làm", "To do"), Maximum = 6 };
        var doing = new KanbanColumn { Header = Loc.T("Đang làm", "In progress"), Accent = new SolidColorBrush(Color.FromRgb(0xF5, 0x9E, 0x0B)) };
        var done = new KanbanColumn { Header = Loc.T("Hoàn tất", "Done"), Accent = new SolidColorBrush(Color.FromRgb(0x22, 0xC5, 0x5E)) };

        todo.Items.Add(new KanbanCard { Title = Loc.T("Mặt bằng tầng 3", "Level 3 plan"), Chip = Loc.T("Kiến trúc", "Architecture") });
        todo.Items.Add(new KanbanCard { Title = Loc.T("Thống kê thép móng", "Foundation rebar schedule"), Chip = Loc.T("Kết cấu", "Structure") });
        doing.Items.Add(new KanbanCard { Title = Loc.T("Chi tiết cầu thang", "Stair details"), Chip = Loc.T("Kết cấu", "Structure") });
        doing.Items.Add(new KanbanCard { Title = Loc.T("Bảng tiến độ tháng 3", "March programme"), Chip = Loc.T("Quản lý", "Management") });
        done.Items.Add(new KanbanCard { Title = Loc.T("Khảo sát địa hình", "Topographic survey"), Chip = Loc.T("Hiện trường", "Site"), IsDone = true });

        var board = new KanbanBoard { Height = 400 };
        board.Columns.Add(todo);
        board.Columns.Add(doing);
        board.Columns.Add(done);

        var status = DemoCaption();
        status.Text = Loc.T("Kéo một thẻ sang cột khác, hoặc dùng nút bên dưới.", "Drag a card to another column, or use the buttons below.");

        board.CardMoved += (_, e) =>
            status.Text = Loc.F("Đã chuyển \"{0}\": {1} -> {2} (vị trí {3})", "Moved \"{0}\": {1} -> {2} (position {3})", e.Card.Title, e.FromColumn?.Header, e.ToColumn.Header, e.ToIndex);

        var actions = new WrapPanel();
        actions.Children.Add(DemoButton(Loc.T("Chuyển thẻ đầu tiên sang Hoàn tất", "Move the first card to Done"), (_, _) =>
        {
            if (todo.Items.Count == 0) return;

            if (todo.Items[0] is KanbanCard card) board.MoveCard(card, done, done.Items.Count);
        }));
        actions.Children.Add(DemoButton(Loc.T("Xáo thứ tự cột Đang làm", "Shuffle the In-progress column"), (_, _) =>
        {
            if (doing.Items.Count < 2) return;

            if (doing.Items[0] is KanbanCard card) board.MoveCard(card, doing, doing.Items.Count);
        }));

        var root = new StackPanel();
        root.Children.Add(actions);
        root.Children.Add(status);
        root.Children.Add(DemoSection(
            Loc.T("Cột, thẻ và giới hạn", "Columns, cards and limits"),
            board,
            xaml: "<controls:KanbanBoard>\n" +
                  Loc.T("    <controls:KanbanColumn Header=\"Cần làm\" Maximum=\"6\">\n", "    <controls:KanbanColumn Header=\"To do\" Maximum=\"6\">\n") +
                  Loc.T("        <controls:KanbanCard Title=\"Mặt bằng tầng 3\" Chip=\"Kiến trúc\" />\n", "        <controls:KanbanCard Title=\"Level 3 plan\" Chip=\"Architecture\" />\n") +
                  "    </controls:KanbanColumn>\n" +
                  Loc.T("    <controls:KanbanColumn Header=\"Đang làm\" Accent=\"#F59E0B\" />\n", "    <controls:KanbanColumn Header=\"In progress\" Accent=\"#F59E0B\" />\n") +
                  "</controls:KanbanBoard>",
            csharp: "board.CardMoved += (_, e) =>\n" +
                    "    taskService.Move(e.Card.Title, e.ToColumn.Header, e.ToIndex);\n\n" +
                    "if (board.MoveCard(card, doneColumn, 0))\n" +
                    Loc.T("    status.Text = \"đã chuyển\";", "    status.Text = \"moved\";")));

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
        status.Text = Loc.T("Lăn chuột để zoom tại con trỏ, giữ chuột trái để kéo.", "Scroll to zoom at the cursor, hold the left button to pan.");

        viewer.MouseWheel += (_, e) => viewer.ZoomAt(e.GetPosition(viewer), e.Delta > 0 ? 1.2 : 1 / 1.2);

        var actions = new WrapPanel();
        actions.Children.Add(DemoButton(Loc.T("Vừa khung", "Fit"), (_, _) => { viewer.Fit(); status.Text = $"Zoom {viewer.Zoom:P0}"; }));
        actions.Children.Add(DemoButton("100%", (_, _) => { viewer.Reset(); status.Text = "Zoom 100%"; }));
        actions.Children.Add(DemoButton("Xoay 90°", (_, _) => { viewer.RotateClockwise(); status.Text = Loc.F("Góc {0}°", "Angle {0}°", viewer.Rotation); }));

        var root = new StackPanel();
        root.Children.Add(actions);
        root.Children.Add(status);
        root.Children.Add(DemoSection(
            Loc.T("Zoom giữ điểm dưới con trỏ đứng yên", "Zoom keeps the point under the cursor fixed"),
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
        string sample =
            Loc.T("// Bảng khối lượng thép cho dầm D1\n", "// Rebar schedule for beam D1") +
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
        status.Text = Loc.F("Dòng: {0}. Dùng nút bên dưới để nhảy tới dòng 6.", "Line: {0}. Use the button below to jump to line 6.", editor.LineCount);

        var actions = new WrapPanel();
        actions.Children.Add(DemoButton(Loc.T("Tới dòng 6", "Go to line 6"), (_, _) => { editor.GoToLine(6); status.Text = Loc.F("Dòng {0}", "Line {0}", editor.CaretLine); }));
        actions.Children.Add(DemoButton("Sang SQL", (_, _) => editor.Syntax = CodeLanguage.Sql));
        actions.Children.Add(DemoButton("Sang XAML", (_, _) => editor.Syntax = CodeLanguage.Xaml));
        actions.Children.Add(DemoButton(Loc.T("Tắt tô màu", "Disable highlighting"), (_, _) => editor.HighlightSyntax = false));
        actions.Children.Add(DemoButton(Loc.T("Bật tô màu", "Enable highlighting"), (_, _) => editor.HighlightSyntax = true));

        var root = new StackPanel();
        root.Children.Add(actions);
        root.Children.Add(status);
        root.Children.Add(DemoSection(
            Loc.T("Tô màu theo ngôn ngữ, số dòng và Tab", "Colouring by language, line numbers and Tab"),
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
        string markdown =
            Loc.T("# Biên bản nghiệm thu\n", "# Acceptance record") +
            "\n" +
            Loc.T("**Công trình:** Chung cư An Phú - Lô B\n", "**Project:** An Phu Apartments - Block B") +
            Loc.T("**Ngày:** 01/11/2026\n", "**Date:** 01/11/2026\n") +
            "\n" +
            Loc.T("## Nội dung kiểm tra\n", "## Inspection items") +
            "\n" +
            Loc.T("1. Cao độ đáy móng so với thiết kế\n", "1. Foundation base level against the design") +
            Loc.T("2. Đường kính và khoảng cách cốt thép\n", "2. Rebar diameter and spacing") +
            Loc.T("3. Lớp bê tông bảo vệ\n", "3. Concrete cover") +
            "\n" +
            Loc.T("> Sai lệch cho phép theo TCVN 4453:2012 là ±10 mm.\n", "> The tolerance allowed by TCVN 4453:2012 is ±10 mm.\n") +
            "\n" +
            "```\n" +
            "D1: 8Ø20 + Ø8a150\n" +
            "D2: 6Ø18 + Ø8a200\n" +
            "```\n" +
            "\n" +
            Loc.T("Chi tiết xem tại [tiêu chuẩn nghiệm thu](https://example.com/tcvn4453).\n", "See the [acceptance standard](https://example.com/tcvn4453) for details.\n") +
            "\n" +
            "---\n" +
            "\n" +
            Loc.T("Kết luận: **đạt** yêu cầu nghiệm thu.", "Conclusion: acceptance requirements **met**.");

        var viewer = new MarkdownViewer { Markdown = markdown, Height = 380 };

        var status = DemoCaption();
        status.Text = Loc.F("{0} liên kết trong tài liệu.", "{0} links in the document.", viewer.LinkCount);

        var root = new StackPanel();
        root.Children.Add(status);
        root.Children.Add(DemoSection(
            Loc.T("Markdown thành FlowDocument", "Markdown to FlowDocument"),
            viewer,
            xaml: "<controls:MarkdownViewer Markdown=\"{Binding ReleaseNotes}\" />",
            csharp: "var document = MarkdownParser.Parse(text);\n" +
                    Loc.T("viewer.Markdown = text;      // tự rebuild\n", "viewer.Markdown = text;      // rebuilds itself") +
                    "int links = viewer.LinkCount;"));

        return root;
    }

    private object CreateCoachMarkDemo()
    {
        var host = new Grid { Height = 320 };

        var target = new Button
        {
            Content = Loc.T("Lưu bản vẽ", "Save drawing"),
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
            Title = Loc.T("Nút lưu nằm ở đây", "The save button lives here"),
            StepIndex = 1,
            StepCount = 3,
            Placement = CoachMarkPlacement.Below
        };

        host.Children.Add(mark);

        var status = DemoCaption();
        status.Text = Loc.T("Thẻ tự lật lên trên khi target sát đáy.", "The card flips above when the target is near the bottom.");

        mark.Next += (_, _) => status.Text = Loc.T("Next: host chuyển sang control kế tiếp.", "Next: the host moves to the next control.");
        mark.Dismissed += (_, _) => status.Text = Loc.T("Đã đóng hướng dẫn.", "Guide dismissed.");

        var actions = new WrapPanel();
        actions.Children.Add(DemoButton(Loc.T("Hiện lại", "Show again"), (_, _) => { mark.Visibility = Visibility.Visible; status.Text = Loc.T("Đang hướng dẫn.", "Guide running."); }));
        actions.Children.Add(DemoButton(Loc.T("Bước tiếp", "Next step"), (_, _) => mark.Advance()));

        var root = new StackPanel();
        root.Children.Add(actions);
        root.Children.Add(status);
        root.Children.Add(DemoSection(
            Loc.T("Thẻ chỉ dẫn bám theo target", "Coach mark anchored to its target"),
            host,
            xaml: "<Grid>\n" +
                  Loc.T("    <Button x:Name=\"saveButton\" Content=\"Lưu bản vẽ\" />\n", "    <Button x:Name=\"saveButton\" Content=\"Save drawing\" />\n") +
                  "    <controls:CoachMark Target=\"{Binding ElementName=saveButton}\"\n" +
                  Loc.T("                        Title=\"Nút lưu nằm ở đây\"\n", "                        Title=\"The save button lives here\"\n") +
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
        status.Text = Loc.T("Click hoặc kéo trong bản đồ để dời khung nhìn.", "Click or drag in the map to move the viewport.");

        map.ViewportChanged += (_, e) => status.Text = Loc.F("Khung nhìn: {0:0} , {1:0} — {2:0}×{3:0}", "Viewport: {0:0} , {1:0} — {2:0}×{3:0}", e.Viewport.X, e.Viewport.Y, e.Viewport.Width, e.Viewport.Height);

        var actions = new WrapPanel();
        actions.Children.Add(DemoButton(Loc.T("Giữa công trình", "Building centre"), (_, _) => map.CentreOn(new Point(6000, 4000))));
        actions.Children.Add(DemoButton(Loc.T("Thu nhỏ khung nhìn", "Shrink viewport"), (_, _) => map.Viewport = new Rect(4000, 3200, 1800, 1200)));

        var root = new StackPanel();
        root.Children.Add(actions);
        root.Children.Add(status);
        root.Children.Add(DemoSection(
            Loc.T("Bản đồ thu nhỏ và khung nhìn", "Mini map and viewport"),
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
        status.Text = Loc.T("Kéo để orbit, lăn chuột để zoom.", "Drag to orbit, scroll to zoom.");

        var actions = new WrapPanel();
        actions.Children.Add(DemoButton(Loc.T("Góc mặc định", "Default angle"), (_, _) => { viewer.ResetView(); status.Text = Loc.T("Đã đặt lại góc nhìn.", "View reset."); }));
        actions.Children.Add(DemoButton(Loc.T("Xoay trái", "Rotate left"), (_, _) => { viewer.Orbit(-15, 0); status.Text = $"Yaw {viewer.Yaw:0}°"; }));
        actions.Children.Add(DemoButton(Loc.T("Xoay phải", "Rotate right"), (_, _) => { viewer.Orbit(15, 0); status.Text = $"Yaw {viewer.Yaw:0}°"; }));

        var root = new StackPanel();
        root.Children.Add(actions);
        root.Children.Add(status);
        root.Children.Add(DemoSection(
            Loc.T("Khung dây 3D chiếu phối cảnh", "3D wireframe with perspective projection"),
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
            Tagline = Loc.T("Bộ control WPF cho ứng dụng xây dựng", "WPF controls for construction apps"),
            Status = Loc.T("Đang nạp tài nguyên...", "Loading resources..."),
            Progress = 20,
            Logo = CreatePlanSheet()
        };

        var status = DemoCaption();
        status.Text = Loc.T("Bấm Tiến trình để thấy thanh chạy.", "Press Progress to see the bar run.");

        splash.Dismissed += (_, _) => status.Text = Loc.T("Đã đóng - gọi Show() để hiện lại.", "Dismissed — call Show() to bring it back.");

        var actions = new WrapPanel();
        actions.Children.Add(DemoButton(Loc.T("Tiến trình +20", "Progress +20"), (_, _) =>
        {
            double next = splash.Progress + 20;
            splash.Report(next, next >= 100 ? Loc.T("Hoàn tất khởi động", "Finish startup") : Loc.T("Đang nạp tài nguyên...", "Loading resources..."));
            status.Text = Loc.F("Tiến trình: {0:0}%", "Progress: {0:0}%", splash.Progress);
        }));
        actions.Children.Add(DemoButton(Loc.T("Chờ không rõ", "Indeterminate"), (_, _) => { splash.IsIndeterminate = true; status.Text = Loc.T("Chế độ chờ.", "Busy mode."); }));
        actions.Children.Add(DemoButton(Loc.T("Đóng", "Close"), (_, _) => splash.Dismiss()));
        actions.Children.Add(DemoButton(Loc.T("Hiện lại", "Show again"), (_, _) => splash.Show()));

        var root = new StackPanel();
        root.Children.Add(actions);
        root.Children.Add(status);
        root.Children.Add(DemoSection(
            Loc.T("Thẻ khởi động có tiến trình", "Splash screen with progress"),
            splash,
            xaml: "<controls:SplashScreen AppName=\"CONCOST.UI\"\n" +
                  "                   Version=\"1.3.0\"\n" +
                  Loc.T("                   Status=\"Đang nạp tài nguyên...\"\n", "                   Status=\"Loading resources...\"\n") +
                  "                   Progress=\"20\" />",
            csharp: Loc.T("splash.Report(65, \"Đang nạp bản vẽ\");\n", "splash.Report(65, \"Loading the drawing\");\n") +
                    "splash.IsIndeterminate = true;\n" +
                    Loc.T("splash.Dismiss();      // hoặc Dismiss(false) khi tắt animation", "splash.Dismiss();      // or Dismiss(false) with animation off")));

        return root;
    }
}
