using System.Collections.Generic;

namespace CONCOST.UI.Showcase;

/// <summary>
/// Vietnamese to English dictionary for the code samples the demos show under
/// <em>View code</em>. The samples keep their Vietnamese comments in source; the English
/// reader gets the translated line at render time, the same way the docs site does it.
/// </summary>
internal static class LocCode
{
    private static readonly Dictionary<string, string> EnglishLines = new()
    {
        { "                       ActionText=\"Định vị vị trí va chạm\"", "                       ActionText=\"Locate the clash\"" },
        { "                       DocumentTitle=\"Landmark Tower — Mặt Bằng Kết Cấu\"", "                       DocumentTitle=\"Landmark Tower — Structural Plan\"" },
        { "                       UserAccountName=\"KTS. Lê Hoàng Minh\"", "                       UserAccountName=\"Arch. Le Hoang Minh\"" },
        { "                 TopLabel=\"TRÊN\" FrontLabel=\"TRƯỚC\" />", "                 TopLabel=\"TOP\" FrontLabel=\"FRONT\" />" },
        { "          attached:ComboBoxHelper.SearchPlaceholder=\"Tìm mác bê tông…\" />", "          attached:ComboBoxHelper.SearchPlaceholder=\"Find a concrete grade…\" />" },
        { "        case \"SAVE\": commandLine.WriteLine(\"Đã lưu bản vẽ thành công.\"); break;", "        case \"SAVE\": commandLine.WriteLine(\"Drawing saved successfully.\"); break;" },
        { "    // Nhận độ dài số thực và vẽ đoạn thẳng trực tiếp", "    // Read the real length and draw the segment straight away" },
        { "    Point3D worldPos = e.World;      // Tọa độ thực trong không gian CAD (mm)", "    Point3D worldPos = e.World;      // real-world CAD coordinates (mm)" },
        { "    bool isSnapped = e.IsSnapped;    // Đang bám điểm hay tự do", "    bool isSnapped = e.IsSnapped;    // snapped or free" },
        { "    new CommandDefinition(\"CIRCLE\", \"Vẽ đường tròn\").WithAliases(\"C\"),", "    new CommandDefinition(\"CIRCLE\", \"Draw a circle\").WithAliases(\"C\")," },
        { "    new CommandDefinition(\"LINE\", \"Vẽ đoạn thẳng liên tục\") { Prompt = \"Specify first point\" },", "    new CommandDefinition(\"LINE\", \"Draw a continuous line\") { Prompt = \"Specify first point\" }," },
        { "    new CommandDefinition(\"SAVE\", \"Lưu bản vẽ\").WithAliases(\"QSAVE\")", "    new CommandDefinition(\"SAVE\", \"Save the drawing\").WithAliases(\"QSAVE\")" },
        { "    new CommandDefinition(\"ZOOM\", \"Phóng to / thu nhỏ\").WithAliases(\"Z\"),", "    new CommandDefinition(\"ZOOM\", \"Zoom in / out\").WithAliases(\"Z\")," },
        { "    { DisplayName = \"Chiều dài\", Category = \"Kích thước\", Unit = \"mm\" });", "    { DisplayName = \"Length\", Category = \"Dimensions\", Unit = \"mm\" });" },
        { "// 1. Cấu hình kiểu dáng (Variant) và kích thước", "// 1. Configure the variant and the sizes" },
        { "// 1. Cấu hình lưới tọa độ & điểm bám CAD", "// 1. Configure the grid and the CAD snap points" },
        { "// 1. Đăng ký tập lệnh CAD và phím tắt Alias", "// 1. Register the CAD command set and its aliases" },
        { "// 2. Màu và nét của trục toạ độ", "// 2. Colour and stroke of the datum axes" },
        { "// 2. Thực thi lệnh và nhận sự kiện", "// 2. Execute commands and receive events" },
        { "// 2. Ẩn / hiện các cụm nút tính năng", "// 2. Show or hide the button clusters" },
        { "// 3. Màu lưới: nét mảnh và nét đậm riêng, alpha quyết định độ đậm", "// 3. Grid colour: thin and bold strokes are separate; alpha sets the weight" },
        { "// 3. Xử lý các lệnh điều hướng một lần (NavigationCommand)", "// 3. Handle one-shot navigation commands (NavigationCommand)" },
        { "// 4. Thu phóng vừa khung hình bản vẽ", "// 4. Zoom the drawing to fit" },
        { "// 4. Xử lý công cụ tương tác giữ trạng thái (ActiveTool: Pan, Orbit, Walk)", "// 4. Handle stateful interactive tools (ActiveTool: Pan, Orbit, Walk)" },
        { "// 5. Tương tác con trỏ và bám điểm thời gian thực", "// 5. Pointer interaction and live snapping" },
        { "// Bắt các sự kiện điều khiển cửa sổ:", "// Hook the window control events:" },
        { "// Bốn cấp độ thông báo BIM: Info, Success, Warning, Error", "// Four BIM notification levels: Info, Success, Warning, Error" },
        { "// Hỗ trợ đổi nhãn mặt sang Tiếng Việt:", "// Face labels can be switched to Vietnamese:" },
        { "// Kết quả hiển thị: \"Landmark Tower — Mặt Bằng Kết Cấu* — CONCOST BIM Suite 2026\"", "// Rendered title: \"Landmark Tower — Structural Plan* — CONCOST BIM Suite 2026\"" },
        { "// Lắng nghe sự kiện thay đổi font để cập nhật kiểu chữ bản vẽ", "// Listen for font changes to restyle the drawing text" },
        { "// Mở hộp nhập liệu bám tại toạ độ con trỏ chuột", "// Open the input box anchored at the pointer" },
        { "// Quản lý tiêu đề tự động kèm dấu * khi có thay đổi chưa lưu:", "// The title is composed for you, with a * while there are unsaved changes:" },
        { "// Thay đổi bố cục hiển thị cửa sổ (Tile Mode)", "// Change the window layout (tile mode)" },
        { "// Thay đổi thuộc tính tự động đồng bộ đồ họa mô hình và hoàn tác Undo", "// Editing a property syncs the model graphics and records undo" },
        { "// Thêm các trang quản trị: Thông tin, Bản vẽ gần đây, Xuất file", "// Add backstage pages: information, recent drawings, export" },
        { "// Thêm các tài liệu đa dạng: Mặt bằng 2D, Khung 3D, Dự toán", "// Add a mix of documents: 2D plans, 3D views, cost estimates" },
        { "// ViewCube được gắn trực tiếp vào ICameraHost của Viewport 3D:", "// The ViewCube attaches straight to the 3D viewport's ICameraHost:" },
        { "// Đăng ký thuộc tính kỹ thuật phân loại theo Category", "// Register technical properties grouped by Category" },
        { "// Đổi góc nhìn trực tiếp bằng code:", "// Change the view straight from code:" },
        { "<!-- Màu lưới: nét mảnh và nét đậm (mỗi GridMajorEvery ô) đặt riêng -->", "<!-- Grid colour: thin and bold strokes (every GridMajorEvery cells) are set separately -->" },
        { "<!-- Màu và nét của trục toạ độ: ba thuộc tính mở rộng, không cần template riêng -->", "<!-- Datum axis colour and stroke: three attached properties, no custom template needed -->" },
        { "LiveInput.Open(cursorPoint, \"Chiều dài\", \"3200\");", "LiveInput.Open(cursorPoint, \"Length\", \"3200\");" },
        { "backstage.Items.Add(new BackstageItem(\"Mở bản vẽ\", IconKind.Folder) { Content = recentPanel });", "backstage.Items.Add(new BackstageItem(\"Open drawing\", IconKind.Folder) { Content = recentPanel });" },
        { "backstage.Items.Add(new BackstageItem(\"Thông tin dự án\", IconKind.Info) { Content = infoPanel });", "backstage.Items.Add(new BackstageItem(\"Project information\", IconKind.Info) { Content = infoPanel });" },
        { "bar.ButtonSize = 28;                         // Kích thước nút bấm (px)", "bar.ButtonSize = 28;                         // button size (px)" },
        { "bar.CornerRadius = new CornerRadius(6);      // Bo góc thanh điều hướng", "bar.CornerRadius = new CornerRadius(6);      // bar corner radius" },
        { "bar.Elevation = 1;                           // Độ nổi đổ bóng (0-4)", "bar.Elevation = 1;                           // shadow elevation (0-4)" },
        { "bar.IconSize = 14;                           // Kích thước icon", "bar.IconSize = 14;                           // icon size" },
        { "bar.Orientation = Orientation.Horizontal;    // Horizontal (ngang) hoặc Vertical (dọc)", "bar.Orientation = Orientation.Horizontal;    // Horizontal or Vertical" },
        { "bar.ShowDividers = true;                     // Vạch ngăn cách giữa các nhóm", "bar.ShowDividers = true;                     // separators between groups" },
        { "bar.ShowPerspective = true;                  // Nút chuyển đổi phối cảnh (Perspective)", "bar.ShowPerspective = true;                  // perspective toggle" },
        { "bar.ShowUcsLabel = true;                     // Nhãn hệ tọa độ (World, Top, Front, Right)", "bar.ShowUcsLabel = true;                     // coordinate-system label (World, Top, Front, Right)" },
        { "bar.ShowWalk = true;                         // Công cụ đi bộ (Walk)", "bar.ShowWalk = true;                         // walk tool" },
        { "bar.ShowZoom = true;                         // Cụm zoom (In, Out, Extents, Selection)", "bar.ShowZoom = true;                         // zoom cluster (In, Out, Extents, Selection)" },
        { "bar.Variant = NavigationBarVariant.Floating; // Floating (nổi có bóng), Seamless (phẳng trong toolbar), Glass (kính mờ 3D)", "bar.Variant = NavigationBarVariant.Floating; // Floating (raised, with a shadow), Seamless (flat in a toolbar), Glass (frosted 3D)" },
        { "cube.FrontLabel = \"TRƯỚC\";", "cube.FrontLabel = \"FRONT\";" },
        { "cube.TopLabel = \"TRÊN\";", "cube.TopLabel = \"TOP\";" },
        { "infoBar.ActionText = \"Định vị va chạm\";", "infoBar.ActionText = \"Locate the clash\";" },
        { "infoBar.Show(\"Cảnh báo va chạm\", \"Dầm D1 giao cắt ống MEP\", CadNotificationSeverity.Warning);", "infoBar.Show(\"Clash warning\", \"Beam D1 crosses an MEP pipe\", CadNotificationSeverity.Warning);" },
        { "viewport.GridMajorEvery = 5;         // Cứ mỗi 5 ô lưới có một đường nét đậm", "viewport.GridMajorEvery = 5;         // every 5 grid cells gets a bold line" },
        { "viewport.GridStep = 500;             // Bước lưới chính (500 mm)", "viewport.GridStep = 500;             // main grid step (500 mm)" },
        { "viewport.ShowAperture = true;        // Ô ngắm vuông xung quanh tâm", "viewport.ShowAperture = true;        // square aperture around the crosshair" },
        { "viewport.ShowCrosshair = true;       // Tâm ngắm kỹ thuật", "viewport.ShowCrosshair = true;       // engineering crosshair" },
        { "viewport.SnapToGrid = true;          // Bắt điểm vào lưới", "viewport.SnapToGrid = true;          // snap to the grid" },
        { "workspace.AddDocument(\"Dự Toán A101\", scheduleGrid, IconKind.FileText);", "workspace.AddDocument(\"Cost Estimate A101\", scheduleGrid, IconKind.FileText);" },
        { "workspace.AddDocument(\"Mặt Bằng Tầng 18\", planCanvas, IconKind.FileCAD);", "workspace.AddDocument(\"Level 18 Plan\", planCanvas, IconKind.FileCAD);" },
    };

    /// <summary>Translates a code sample line by line; unknown lines pass through unchanged.</summary>
    public static string Translate(string code)
    {
        if (string.IsNullOrEmpty(code))
        {
            return code;
        }

        // Verbatim samples inherit the file's line endings, so normalise before matching
        // and put the original ending back afterwards.
        string newline = code.Contains("\r\n") ? "\r\n" : "\n";
        var lines = code.Replace("\r\n", "\n").Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            if (EnglishLines.TryGetValue(lines[i], out var translated))
            {
                lines[i] = translated;
            }
        }

        return string.Join(newline, lines);
    }
}
