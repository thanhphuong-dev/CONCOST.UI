// The Revit-inspired CAD shell: viewport, navigation, command line, status bar, browsers and chrome.
const DOCS_CAD = {
    viewcube: {
        title: "ViewCube - khối điều hướng hướng nhìn",
        badge: "CAD & Drafting",
        desc: "Khối lập phương vát cạnh ở góc khung vẽ, quay theo camera: <b>26 vùng bấm</b> — sáu mặt có chữ, mười hai dải cạnh và tám tam giác góc — cùng vòng la bàn trên mặt phẳng XY, nút home, bốn mũi tên 90°, hai nút xoay và menu ngữ cảnh. Control chỉ là lớp hiển thị: nó không sở hữu camera, chỉ đọc và ghi một <code>Quaternion</code> qua <code>ICameraHost</code>, còn toàn bộ hình học nằm trong <code>ViewCubeMath</code> nên dùng lại được cho WinForms, OpenGL hay web.",
        keywords: ["viewcube", "camera", "quaternion", "orientation", "compass", "projection"],
        sections: [
            {
                title: "Gắn vào camera của host",
                desc: "<code>Host</code> nhận bất kỳ <code>ICameraHost</code> nào, và giao diện chỉ có bốn thành viên: <code>Rotation</code>, <code>PivotPoint</code>, <code>Projection</code>, <code>FitToView</code>. <code>CameraController</code> đã cài sẵn nên khung vẽ dùng được ngay. Camera nào cài thêm <code>ICameraHostEvents</code> thì khối tự cập nhật khi khung vẽ quay; camera không báo được thì host gọi <code>Invalidate()</code> từ vòng vẽ của mình.",
                xaml: `<Grid>
    <ui:ViewportControl x:Name="Viewport" />
    <ui:ViewCube x:Name="Cube"
                 Host="{Binding ElementName=Viewport, Path=Camera}"
                 Position="TopRight"
                 Size="Normal" />
</Grid>`,
                csharp: `// Một camera ngoài lề: bốn thành viên là đủ
public sealed class MyCamera : ICameraHost, ICameraHostEvents
{
    public Quaternion Rotation { get; set; } = Quaternion.Identity;
    public Point3D PivotPoint => _pivot;
    public ProjectionMode Projection { get; set; }
    public void FitToView() => _scene.FrameEverything();

    public event EventHandler? CameraChanged;      // ICameraHostEvents
}`,
                callout: {
                    type: "note",
                    body: "Không gán <code>Host</code> thì khối giữ tư thế của chính nó trong <code>Rotation</code> — tiện cho demo và cho test. Gán host thì <code>Rotation</code> chỉ còn là bản sao của camera, nên khung vẽ và khối không lệch nhau."
                }
            },
            {
                title: "26 vùng bấm, một nguồn hình học",
                desc: "Mỗi mặt là ô vuông tỉ lệ <code>FaceRatio</code> ở giữa, mỗi cạnh là dải vát giữa hai mặt, mỗi góc là tam giác nhỏ — đúng 6 + 12 + 8 = 26 vùng. Pháp tuyến của cạnh và góc là tổng đã chuẩn hoá của các mặt kề; hướng tới và hướng lên theo bảng của đặc tả, riêng cạnh và góc dùng <code>u = (ẑ − (ẑ·f)f)</code>. Chữ trên mặt viết hoa, canh giữa và xoay theo mặt. <code>ViewCubeMath.Regions</code> trả về cả 26 vùng kèm <code>RotationFor</code>, nên dựng đúng tư thế của một vùng chỉ là một dòng.",
                csharp: `// Duyệt 26 vùng mà không cần chuột
foreach (ViewCubeRegion region in ViewCubeMath.Regions)
    Console.WriteLine($"{region.Kind} {region.Name} f={region.Forward} u={region.Up}");

// Dựng một vùng lên đúng mặt của nó
Cube.GoTo(ViewCubeMath.Regions.First(r => r.Name == "Face:FRONT"));

// Vùng bấm của một điểm, mở cho test
ViewCubeHit hit = Cube.HitTest(new Point(70, 70));`,
                callout: {
                    type: "tip",
                    body: "Theo quy ước Revit, <code>FRONT</code> là mặt nhìn từ phía −Y: X sang phải, Z lên, nên chữ Z trên khối hướng lên đúng như bản vẽ. Camera của host cũng phải dùng hệ Z-up thì mới khớp."
                }
            },
            {
                title: "La bàn trên mặt phẳng XY",
                desc: "Vòng la bàn nằm trên mặt đất, bốn chữ N/E/S/W ở ngoài vành: kéo một chữ là quay mô hình quanh trục Z (yaw), bấm vào vành là nhảy tới hướng đó. Nhìn nghiêng quá thấp thì vành dẹp thành một đường và bốn chữ chồng lên nhau, nên dưới 10° khối tự ẩn la bàn; <code>ShowCompass=False</code> ẩn hẳn. <code>FrontAngle</code> là góc giữa hướng front của mô hình và trục Y, và cả bốn chữ xoay theo góc đó.",
                xaml: `<ui:ViewCube ShowCompass="True"
             FrontAngle="0" />`,
                csharp: `// Bản vẽ quay 30° so với trục bắc
Cube.SetCurrentViewAsFront();      // FrontAngle := heading đang nhìn
Cube.SetCurrentViewAsHome();       // HomeRotation := tư thế hiện tại

Cube.ResetFront();                 // trả front về 0

Cube.FrontChanged += (_, _) => settings.Save("front", Cube.FrontAngle);`
            },
            {
                title: "Menu ngữ cảnh, chuyển cảnh và bảng Options",
                desc: "Nút menu ở góc dưới phải mở: Go Home, Orthographic, Perspective, Perspective with Ortho Faces, Set Current View as Home, Set Current View as Front, Reset Front, Show Compass và Options… — mỗi mục đi qua đúng API ở trên, nên host gọi lại được từ ribbon. Trước mỗi lần chuyển cảnh khối bắn <code>ViewRequested</code> để host huỷ hoặc đổi <code>Target</code>; đường đi là <code>slerp</code> cung ngắn với easing <code>t²(3−2t)</code>, 200–600 ms theo góc quay, và bấm giữa chừng thì chạy tiếp từ tư thế đang vẽ chứ không nhảy.",
                csharp: `Cube.ViewRequested += (_, e) =>
{
    if (!CanOrbit)
    {
        e.Cancel = true;               // khung 2D không cho quay
        return;
    }

    e.Target = LockTarget(e.Target);   // hoặc đổi hẳn tư thế đích
};

Cube.ProjectionModeChanged += (_, e) => Camera.Projection = e.Mode;
Cube.OptionsRequested += (_, _) => Host.ShowOptionsDialog();`,
                callout: {
                    type: "note",
                    body: "Bảng Options của đặc tả đều là dependency property: <code>Visible</code>, <code>Position</code>, <code>DockMargin</code> (10), <code>Size</code> (Automatic = 1/5 cạnh ngắn, kẹp 64–160 px; hoặc Tiny 64, Small 80, Normal 100, Large 128), <code>InactiveOpacity</code> 50, <code>ShowCompass</code>, <code>SnapToClosestView</code>, <code>SnapAngle</code> 10, <code>FitToViewOnChange</code>, <code>AnimatedTransitions</code>, <code>AnimationDuration</code> 500, <code>KeepSceneUpright</code> và <code>ClickThreshold</code> 4 — chạm ngắn hơn ngưỡng là bấm, dài hơn là kéo quay quỹ đạo."
                }
            }
        ],
        related: ["navigationbar", "viewportcontrol", "viewcontrolbar"]
    },

    navigationbar: {
        title: "NavigationBar - thanh điều hướng",
        badge: "CAD & Drafting",
        desc: "Dải nút cho zoom in/out/extents, pan, orbit, walk và nhãn hệ toạ độ làm việc. Công cụ pan/orbit/walk là <b>bật/tắt giữ trạng thái</b>: bấm lại để thoát, giống thanh điều hướng của CAD.",
        keywords: ["navigation", "zoom", "pan", "orbit", "walk", "ucs"],
        sections: [
            {
                title: "Lệnh và công cụ là hai kênh riêng",
                desc: "<code>NavigationRequested</code> bắn một lần cho mỗi cú bấm (zoom, extents…), còn <code>ActiveTool</code> giữ trạng thái cho tới khi đổi — nên kéo chuột chỉ là pan khi công cụ đang bật.",
                xaml: `<ui:NavigationBar ShowWalk="True"
                  Ucs="Top"
                  NavigationRequested="OnNavigate"
                  ActiveToolChanged="OnToolChanged"
                  UcsChanged="OnUcsChanged" />`,
                csharp: `bar.NavigationRequested += (_, e) =>
{
    switch (e.Command)
    {
        case NavigationCommand.ZoomIn:  camera.Zoom(0.8);  break;
        case NavigationCommand.ZoomOut: camera.Zoom(1.25); break;
        case NavigationCommand.ZoomExtents: camera.FitTo(bounds); break;
        case NavigationCommand.Perspective:
            camera.Projection = CameraProjection.Perspective;
            break;
    }
};`
            },
            {
                title: "Bật/tắt công cụ từ code",
                desc: "<code>SetActiveTool</code> bấm hộ một lần: gọi lại cùng công cụ sẽ tắt nó. Hữu ích khi gán phím tắt (Space để pan tạm).",
                xaml: `<ui:NavigationBar x:Name="Nav" ShowWalk="False" />`,
                csharp: `// Space giữ để pan tạm thời
private void OnPreviewKeyDown(object sender, KeyEventArgs e)
{
    if (e.Key != Key.Space) return;

    Nav.SetActiveTool(NavigationTool.Pan);
    e.Handled = true;
}`
            },
            {
                title: "Một khung duy nhất, các lệnh phẳng bên trong",
                desc: "Thanh cũ vẽ <b>ba</b> lớp viền: khung của chính nó, viền 1 px của từng nút icon (<code>CONCOST.Button.Icon</code> xin 34 px và một đường bao), và khung của trang demo bọc ngoài. Nay chỉ khung của thanh là có viền — mỗi lệnh là nút phẳng, chỉ hiện nền khi rê chuột, nhãn UCS là nút chữ có mũi nhọn. Đường phân cách lấy token theme nên không còn thành vạch sáng khi đổi theme tối.",
                callout: {
                    type: "note",
                    body: "Đừng bọc <code>NavigationBar</code> trong một <code>Border</code> khác — khung đã có sẵn. Nếu thanh nằm trong toolbar thì dùng <code>Variant=\"Seamless\"</code> để nó bỏ khung của mình đi."
                },
                xaml: `<ui:NavigationBar Variant="Floating"
                  CornerRadius="6"
                  Elevation="1"
                  ButtonSize="26"
                  IconSize="14" />`,
                csharp: `// Một dòng đổi cả cách mặc: Floating, Seamless, Glass
void Dress(NavigationBar bar, string placement) =>
    bar.Variant = placement switch
    {
        "toolbar" => NavigationBarVariant.Seamless,
        "scene"   => NavigationBarVariant.Glass,
        _         => NavigationBarVariant.Floating
    };`
            },
            {
                title: "Ba kiểu, ba chỗ đặt, cùng một bộ lệnh",
                desc: "<code>Floating</code> nổi trên khung vẽ: nền Surface, 1 px viền, bóng theo <code>Elevation</code>. <code>Seamless</code> bỏ hẳn khung để nằm gọn trong toolbar hay status bar. <code>Glass</code> là tấm mờ 90% có bóng, để cảnh 3D phía sau còn đọc được. <code>Orientation</code> đổi thanh thành cột và đường phân cách tự xoay theo; nhóm nào bị ẩn thì vạch cạnh nó cũng rút.",
                xaml: `<ui:NavigationBar Variant="Seamless"
                  Orientation="Vertical"
                  ShowUcsLabel="False"
                  ShowZoom="False"
                  ShowDividers="True"
                  ShowWalk="False" />`,
                csharp: `// Cỡ nút cho cảm ứng: ghost style xin 34 px, thanh ghi đè bằng ButtonSize
bar.ButtonSize = 34;
bar.IconSize   = 16;

// Ẩn nhóm: vạch phân cách cạnh nó tự rút, không để lại vạch lơ lửng
bar.ShowUcsLabel = false;   // mất nhãn + vạch đầu
bar.ShowZoom     = false;   // mất cụm zoom + vạch thứ hai
bar.ShowDividers = false;   // bỏ luôn cả ba vạch`
            }
        ],
        related: ["viewcube", "viewcontrolbar", "viewportcontrol"]
    },

    viewcontrolbar: {
        title: "ViewControlBar - thanh thuộc tính khung nhìn",
        badge: "CAD & Drafting",
        desc: "Tỉ lệ, mức chi tiết, kiểu hiển thị, đổ bóng, crop, hiện phần tử ẩn và nhóm lệnh hide/isolate. Mỗi thay đổi — <b>do chuột hay do code</b> — đều bắn <code>SettingsChanged</code> kèm tên thuộc tính. Ba kiểu dáng (<code>Floating</code> / <code>Seamless</code> / <code>Glass</code>), một chiều cao chung cho cả thanh, và danh sách tỉ lệ do host quyết định.",
        keywords: ["view control", "detail level", "visual style", "shadows", "crop", "hide isolate"],
        sections: [
            {
                title: "Một chỗ khai báo thuộc tính khung nhìn",
                desc: "Host không phải giữ bản sao: control sở hữu giá trị và báo lại, nên thanh ribbon, bảng thuộc tính và khung vẽ cùng đọc một nguồn.",
                xaml: `<ui:ViewControlBar ViewScale="1:100"
                   DetailLevel="Medium"
                   VisualStyle="HiddenLine"
                   ShowShadows="False"
                   SettingsChanged="OnViewSettingChanged"
                   HideIsolateRequested="OnHideIsolate" />`,
                csharp: `bar.SettingsChanged += (_, name) =>
{
    // name là tên thuộc tính: "VisualStyle", "ShowShadows", ...
    switch (name)
    {
        case nameof(ViewControlBar.VisualStyle):
            viewport.VisualStyle = bar.VisualStyle;
            break;

        case nameof(ViewControlBar.DetailLevel):
            viewport.DetailLevel = bar.DetailLevel;
            break;
    }
};`
            },
            {
                title: "Khôi phục một view đã lưu",
                desc: "Gán thuộc tính từ code cũng bắn sự kiện, nên mở lại một view đã lưu chỉ là gán giá trị — không cần thêm đường riêng.",
                xaml: `<ui:ViewControlBar x:Name="Bar" />`,
                csharp: `// Nạp từ file view đã lưu
Bar.ViewScale    = saved.Scale;
Bar.VisualStyle  = saved.Style;
Bar.ShowShadows  = saved.Shadows;
// SettingsChanged bắn ba lần, khung vẽ vẽ lại theo từng bước

// Hide / isolate: host quyết định nghĩa
Bar.HideIsolateRequested += (_, e) =>
    selection.Where(e.Action == HideIsolateAction.IsolateElements);`
            },
            {
                title: "Một khung duy nhất, các điều khiển nằm phẳng bên trong",
                desc: "Thanh cũ vẽ <b>bốn</b> lớp viền: khung của chính nó, một viền 1 px cho mỗi công tắc (<code>CONCOST.ToggleButton.Icon</code> xin 34 px và một đường viền), viền của nút hide/isolate, và thêm một lớp nữa từ khung demo. Giờ chỉ khung là có viền — công tắc dùng kiểu ghost, nút hide dùng kiểu ghost, nên chúng phẳng cho tới khi hover hoặc được bật. Cái làm thanh trông thô nhất lại là một con số vô hình: style combo đặt <code>MinHeight</code> bằng 34 px, nên một thanh khai 26 px vẫn cao 34 px; <code>ControlHeight</code> giờ xoá mức tối thiểu trước khi đặt chiều cao, và vạch phân cách lấy màu từ token theme thay vì xám cố định.",
                callout: {
                    type: "note",
                    body: "Đừng bọc <code>ViewControlBar</code> trong một <code>Border</code> nữa — nó đã có khung. Nếu thanh nằm trong status bar, đặt <code>Variant=\"Seamless\"</code> và nó tự bỏ khung."
                },
                xaml: `<ui:ViewControlBar Variant="Floating"
                   ControlHeight="26" IconSize="14"
                   CornerRadius="6" Elevation="1" />`,
                csharp: `// Một chiều cao cho cả thanh: combo, công tắc và nút hide
bar.ControlHeight = 26;   // combo style xin 34 px, thanh ghi đè
bar.IconSize      = 14;
bar.CornerRadius  = new CornerRadius(6);
bar.Elevation     = 1;    // 0 là phẳng, 4 là nổi nhất`
            },
            {
                title: "Ba kiểu, ba chỗ đặt, cùng một bộ lệnh",
                desc: "<code>Floating</code> nổi trên khung vẽ: nền Surface, một đường viền, bóng theo <code>Elevation</code>. <code>Seamless</code> bỏ hẳn tấm nền để thanh nằm gọn trong status bar mà không thành hai lớp viền. <code>Glass</code> là tấm 90% có bóng, để bản vẽ phía sau vẫn đọc được. Từng nhóm một rút được — <code>ShowScale</code>, <code>ShowDetailLevel</code>, <code>ShowVisualStyle</code>, <code>ShowToggles</code>, <code>ShowHideIsolate</code> — và vạch phân cách cạnh nó đi theo, nên không bao giờ còn một vạch trơ ở cuối thanh.",
                xaml: `<!-- Nằm trong status bar: không viền, không bóng -->
<ui:ViewControlBar Variant="Seamless"
                   ShowToggles="False"
                   VerticalAlignment="Center" />`,
                csharp: `// Danh sách tỉ lệ do host quyết định, và có thể lớn thêm
var scales = new ObservableCollection<string> { "1:50", "1:100", "1:250" };
bar.Scales = scales;
scales.Add("1:500");     // dropdown tự nạp lại

// View lưu ở tỉ lệ chưa có trong danh sách vẫn hiện đúng
bar.ViewScale = "1:750";

// Nhãn dịch được
bar.HideIsolateCaption = "Ẩn";`
            }
        ],
        related: ["viewportcontrol", "navigationbar", "propertiespalette"]
    },

    viewportcontrol: {
        title: "ViewportControl - khung vẽ",
        badge: "CAD & Drafting",
        desc: "Bề mặt vẽ: lưới, tâm ngắm, dấu bám điểm, grip, dimension tạm, khung chọn và một lớp overlay. Camera là <code>CameraController</code> thật nên cùng một camera dùng được cho mini map, khung vẽ và bản in.",
        keywords: ["viewport", "drawing surface", "grid", "snap", "crosshair", "overlay", "camera"],
        sections: [
            {
                title: "Toạ độ hai chiều",
                desc: "<code>ToWorld</code> đổi điểm màn hình sang điểm mô hình trên mặt phẳng qua <code>Camera.Target</code>; <code>ToScreen</code> đi ngược lại. <code>CursorMoved</code> báo cả toạ độ lẫn việc có bám điểm hay không.",
                xaml: `<ui:ViewportControl x:Name="Viewport"
                    Height="420"
                    GridStep="500"
                    ShowGrid="True"
                    ShowCrosshair="True"
                    SnapToGrid="True"
                    CursorMoved="OnCursorMoved" />`,
                csharp: `private void OnCursorMoved(object sender, ViewportCursorEventArgs e)
{
    StatusBar.UpdateCoordinates(e.World);

    if (e.IsSnapped) SnapMarker.Place(e.Screen);
}

// Đọc toạ độ ở một điểm bất kỳ
Point3D world = Viewport.ToWorld(new Point(240, 160));`
            },
            {
                title: "Grip, dimension và lớp phủ",
                desc: "Ba thuộc tính nhận dữ liệu đã chiếu sẵn: <code>Grips</code>, <code>TemporaryDimension</code> và <code>SelectionRectangle</code>. Cần vẽ thứ khác thì thêm vào <code>Overlay</code> — một Canvas nằm trên cùng, cùng hệ toạ độ màn hình.",
                xaml: `<ui:ViewportControl x:Name="Viewport">
    <ui:ViewportControl.Snap>
        <Point X="240" Y="160" />
    </ui:ViewportControl.Snap>
</ui:ViewportControl>`,
                csharp: `// Grip của cấu kiện đang chọn
Viewport.Grips = new[]
{
    new ViewportGrip(new Point3D(0, 0, 0), isHot: true),
    new ViewportGrip(new Point3D(6000, 0, 0))
};

// Dimension tạm theo con trỏ
Viewport.TemporaryDimension = new TemporaryDimension
{
    Start = new Point3D(0, 0, 0),
    End = new Point3D(6000, 0, 0),
    Text = "6000"
};`
            },
            {
                title: "Đường giữa khung là trục toạ độ, không phải cảnh báo",
                desc: "Một đường ngang và một đường dọc cắt nhau ở tâm khung là <b>trục X và Y của thế giới mô hình</b>, vẽ xuyên qua gốc toạ độ. Chúng là mốc để đọc bản vẽ chứ không phải tín hiệu lỗi, nên mặc định là nét đứt màu <code>CONCOST.Brush.CanvasAxis</code>: màu nhấn để dành cho thứ đang được chọn. <code>ShowAxes</code> tắt cả hai đường, <code>ShowOriginMarker</code> giữ lại vòng tròn nhỏ ở gốc kể cả khi đường đã tắt.",
                xaml: `<ui:ViewportControl ShowAxes="True"
                    ShowOriginMarker="True"
                    AxisThickness="1"
                    AxisDashStyle="{x:Static DashStyles.Dash}" />`,
                csharp: `// null = giữ token của theme; đặt brush là host tự quyết màu
Viewport.AxisThickness = 1;
Viewport.AxisDashStyle = DashStyles.Solid;   // lưới khảo sát thường dùng nét liền
Viewport.AxisBrush = Brushes.SlateGray;

// Hoặc bám theo theme dù brush do host đặt
Viewport.SetResourceReference(ViewportControl.AxisBrushProperty,
                              "CONCOST.Brush.TextSecondary");`,
                callout: {
                    type: "note",
                    body: "Hai trục được chiếu từ hướng camera chứ không giả định nằm ngang và dọc: sau khi orbit chúng nghiêng theo, và trục mà camera nhìn thẳng xuống sẽ tự bỏ qua thay vì vẽ một đoạn dài 0."
                }
            },
            {
                title: "Mỗi lớp một brush, mỗi lớp một công tắc",
                desc: "Lưới, con trỏ, dấu bám điểm, khung chọn và dimension đều có brush riêng; <code>null</code> nghĩa là lấy token của theme, nên đổi theme là đổi cả khung vẽ. <code>GridMajorEvery</code> quyết định mỗi bao nhiêu đường thì đậm hơn, còn <code>MinGridPixelSpacing</code> tự nhân đôi bước lưới cho tới khi đủ thoáng — nhờ vậy lưới không bao giờ thành một mảng xám.",
                csharp: `Viewport.GridBrush = new SolidColorBrush(Color.FromRgb(0xEF, 0xF3, 0xF8));
Viewport.GridMajorBrush = new SolidColorBrush(Color.FromRgb(0xDC, 0xE3, 0xEC));
Viewport.GridMajorEvery = 5;        // mỗi đường thứ 5 đậm hơn
Viewport.MinGridPixelSpacing = 8;   // dưới 8 px thì bước tự nhân đôi

Viewport.CrosshairBrush = Brushes.Gray;
Viewport.ShowAperture = false;      // tắt ô ngắm, vẫn còn chữ thập

Viewport.SnapMarkerBrush = Brushes.DeepSkyBlue;
Viewport.SnapMarkerSize = 6;        // px; mỗi loại bám điểm một hình riêng
Viewport.ShowSnapLabel = false;     // tắt chữ GRID / ENDPOINT cạnh dấu

Viewport.SelectionBrush = Brushes.DodgerBlue;
Viewport.SelectionFillOpacity = 0;  // 0 = chỉ viền, 0.12 = mặc định, 1 = tô đặc
Viewport.DimensionBrush = Brushes.Navy;
Viewport.FitMargin = 1.15;          // ZoomToFit chừa 15% không khí quanh biên`,
                callout: {
                    type: "tip",
                    body: "Nhãn cạnh dấu bám điểm chính là tên loại bám điểm viết hoa — <code>GRID</code> khi bám lưới, <code>ENDPOINT</code> khi bám đầu mút — nên tắt nó bằng <code>ShowSnapLabel</code> chứ không phải bằng cách đổi loại bám."
                }
            }
        ],
        related: ["viewcube", "navigationbar", "minimap"]
    },

    commandline: {
        title: "CommandLine - dòng lệnh",
        badge: "CAD & Drafting",
        desc: "Lịch sử lệnh, ô nhập có <b>gợi ý</b>, nhớ lệnh cũ bằng phím mũi tên, Tab để hoàn tất tên lệnh, và <b>prompt mode</b> cho giá trị mà lệnh còn thiếu. Prompt được ghi vào lịch sử như một cuộc đối thoại, từ khoá trong ngoặc, ba cách đặt, phím cách cũng là Enter.",
        keywords: ["command line", "prompt", "autocomplete", "history", "cad input", "parser"],
        sections: [
            {
                title: "Dòng trên cùng là bản ghi cuộc đối thoại, không phải chú thích",
                desc: "Dòng lệnh CAD không phải một ô nhập kèm nhật ký: nó là <b>bản ghi cuộc đối thoại</b>. Vì vậy <code>BeginPrompt</code> ghi luôn câu hỏi vào lịch sử và câu trả lời được nối vào đúng dòng đó — <code>Specify first point: 0,0</code> — chứ không nằm trơ một mình. Từ khoá của prompt nằm trong ngoặc như CAD: <code>WithOptions(\"Undo\", \"Close\")</code> cho ra <code>Specify next point or [Undo/Close]:</code>, gõ <code>U</code> hay <code>C</code> cũng đủ, và <code>e.Option</code> trả về từ khoá đầy đủ — <code>null</code> khi đó là một giá trị.",
                callout: {
                    type: "note",
                    body: "Đừng ghi dòng hướng dẫn bằng <code>WriteLine</code>: lịch sử là chỗ của lệnh, không phải chỗ của chú thích. Hướng dẫn để ngoài control."
                },
                xaml: `<ui:CommandLine Variant="Docked"
                 HistoryHeight="96"
                 ExpandedHistoryHeight="240" />`,
                csharp: `new CommandDefinition("LINE", "Vẽ thanh liên tục")
    { Prompt = "Specify first point" };

commandLine.BeginPrompt("Specify next point", new[] { "Undo", "Close" });
// Specify next point or [Undo/Close]:

commandLine.PromptCompleted += (_, e) =>
{
    if (e.Option == "Undo") StepBack();          // gõ \"U\" cũng ra Undo
    else if (e.Option == "Close") ClosePolyline();
    else if (CadInputParser.TryParseCoordinate(e.Text, out var point)) Draw(point);
};`
            },
            {
                title: "Ba cách đặt, và phím cách cũng là Enter",
                desc: "<code>Variant</code> quyết định mặt nền: <code>Docked</code> vẽ đúng một đường kẻ trên đỉnh — bốn cạnh còn lại là khung cửa sổ; <code>Floating</code> là tấm bo góc có bóng; <code>Inline</code> không vẽ gì để control nằm gọn trong card. <code>SpaceSubmits</code> mặc định bật nên phím cách chạy lệnh như Enter — tắt đi khi câu trả lời có thể chứa dấu cách. <code>F2</code> mở rộng lịch sử lên <code>ExpandedHistoryHeight</code>, và <code>ShowHistory = false</code> rút thanh còn một dòng như thanh bị thu gọn của AutoCAD.",
                xaml: `<ui:CommandLine Variant="Floating" CornerRadius="6" Elevation="3" />`,
                csharp: `bar.Variant = CommandLineVariant.Docked;   // dải dưới đáy cửa sổ
bar.Variant = CommandLineVariant.Floating; // tấm nổi
bar.Variant = CommandLineVariant.Inline;   // không mặt nền

bar.ShowHistory       = false;  // còn một dòng
bar.IsHistoryExpanded = true;   // như bấm F2
bar.SpaceSubmits      = true;   // phím cách = Enter`
            },
            {
                title: "Đăng ký lệnh và trả lời prompt",
                desc: "<code>CommandDefinition.Prompt</code> biến lần nhập kế tiếp thành câu trả lời thay vì một lệnh mới — đúng cách CAD hỏi thêm tham số. Alias cho phép gõ tắt (<code>L</code> cho <code>LINE</code>).",
                xaml: `<ui:CommandLine Height="220"
                HistoryHeight="140"
                CommandInvoked="OnCommandInvoked"
                PromptCompleted="OnPromptCompleted"
                UnknownCommand="OnUnknownCommand" />`,
                csharp: `commandLine.AddCommands(new[]
{
    new CommandDefinition("LINE", "Vẽ đoạn thẳng") { Prompt = "Điểm đầu (x,y)" },
    new CommandDefinition("CIRCLE", "Vẽ đường tròn").WithAliases("C"),
    new CommandDefinition("ZOOM", "Phóng to / thu nhỏ").WithAliases("Z")
});

commandLine.PromptCompleted += (_, e) =>
{
    if (CadInputParser.TryParseCoordinate(e.Text, out Point3D point))
        DrawLine(point);
    else
        commandLine.WriteLine("Không đọc được toạ độ.", CommandMessageKind.Error);
};`
            },
            {
                title: "Đọc số đo kiểu bản vẽ",
                desc: "<code>CadInputParser</code> là hàm thuần: chiều dài nhận <code>mm</code>, <code>cm</code>, <code>m</code>, <code>in</code>, <code>ft</code> và dạng feet-inch <code>3'-6\"</code>; số trần hiểu là milimét. Nhờ vậy cùng một ô nhập dùng được cho cả hệ mét và hệ inch.",
                xaml: `<ui:CommandLine x:Name="Command" />`,
                csharp: `CadInputParser.TryParseLength("3200", out double a);      // 3200 mm
CadInputParser.TryParseLength("3.2m", out double b);      // 3200 mm
CadInputParser.TryParseLength("3'-6\\"", out double c);    // 1066.8 mm

CadInputParser.TryParseCoordinate("1000, 500", out Point3D p);
CadInputParser.TryParseAngle("45°", out double degrees);`
            }
        ],
        related: ["cadstatusbar", "autocompletetextbox", "commandpalette"]
    },

    dynamicinput: {
        title: "DynamicInput - nhập ngay tại con trỏ",
        badge: "CAD & Drafting",
        desc: "Ô nhập nhỏ đi theo con trỏ trong lúc vẽ: gõ chiều dài, góc hay toạ độ ngay chỗ đang nhìn, không phải nhìn xuống đáy cửa sổ. <b>Một tấm nền, một gạch chân</b> — không có ô nhập lồng trong khung thứ hai — câu hỏi ở dòng trên, đơn vị là khối riêng, con số hiện bằng font đều nên các hộp xếp thẳng cột. Gõ sai thì hộp <b>vẫn mở</b> và nói lý do, nên không mất giá trị vừa nhập.",
        keywords: ["dynamic input", "cursor input", "coordinate entry", "length", "angle"],
        props: [
            { name: "Prompt", type: "string", default: "string.Empty", desc: "Câu hỏi ở dòng đầu hộp; đơn vị không dán vào câu hỏi mà là khối riêng bên phải." },
            { name: "Text", type: "string", default: "string.Empty", desc: "Giá trị đang gõ (bind two-way)." },
            { name: "Kind", type: "DynamicInputKind", default: "Distance", desc: "<code>Distance</code>, <code>Angle</code>, <code>Coordinate</code>, <code>Text</code>; cũng là nguồn của đơn vị tự chọn." },
            { name: "Anchor", type: "Point", default: "0,0", desc: "Điểm hộp bám vào, tính theo toạ độ của panel chứa nó; đường gióng chỉ về đây." },
            { name: "Offset", type: "double", default: "18", desc: "Khoảng lệch (px) từ <code>Anchor</code> tới hộp." },
            { name: "Variant", type: "DynamicInputVariant", default: "Floating", desc: "<code>Floating</code> tấm nổi có viền và bóng, <code>Compact</code> dải một dòng phẳng, <code>Glass</code> tấm mờ đọc được trên ảnh nền." },
            { name: "Placement", type: "DynamicInputPlacement", default: "Auto", desc: "<code>Auto</code> tự lật vào trong khi hộp sắp tràn; hoặc ép <code>Right</code>, <code>Left</code>, <code>Above</code>." },
            { name: "CornerRadius", type: "CornerRadius", default: "6", desc: "Bo góc tấm nền; <code>Compact</code> tự trừ 2 vì nó là dải." },
            { name: "Elevation", type: "int", default: "1", desc: "Độ sâu bóng 0–4; <code>Glass</code> luôn ít nhất 2." },
            { name: "UnitSuffix", type: "string", default: "string.Empty", desc: "Đơn vị ghi cạnh câu hỏi; trống thì lấy theo <code>Kind</code> — <code>(mm)</code>, <code>(°)</code>, còn toạ độ thì không có đơn vị." },
            { name: "ShowUnit", type: "bool", default: "true", desc: "Hiện khối đơn vị." },
            { name: "ShowLeader", type: "bool", default: "true", desc: "Vẽ đường gióng nét đứt về <code>Anchor</code>; <code>Compact</code> luôn tắt vì không có chỗ." },
            { name: "FieldWidth", type: "double", default: "120", desc: "Bề rộng tối thiểu của ô nhập (px); toạ độ cần rộng hơn chiều dài." },
            { name: "SelectAllOnOpen", type: "bool", default: "true", desc: "Chọn sẵn giá trị khi mở, để gõ là thay chứ không nối vào số cũ." },
            { name: "IsOpen", type: "bool", default: "false", desc: "Hộp đang hiện và nhận phím." },
            { name: "IsValid", type: "bool", default: "true", desc: "Chuỗi hiện tại đọc được (chỉ đọc)." },
            { name: "ValidationMessage", type: "string", default: "string.Empty", desc: "Lý do lần commit trước bị từ chối (chỉ đọc)." }
        ],
        sections: [
            {
                title: "Gắn vào lớp phủ của khung vẽ",
                desc: "Đặt trong <code>Overlay</code> của <code>ViewportControl</code>; control tự đặt mình cách <code>Anchor</code> một khoảng <code>Offset</code>, nên host chỉ cần gọi <code>Open</code> mỗi lần con trỏ nhích.",
                xaml: `<ui:ViewportControl x:Name="Viewport">
    <ui:ViewportControl.Overlay>
        <ui:DynamicInput x:Name="Live"
                         Kind="Distance"
                         ValueCommitted="OnLiveValue" />
    </ui:ViewportControl.Overlay>
</ui:ViewportControl>`,
                csharp: `Viewport.CursorMoved += (_, e) =>
{
    if (Live.IsOpen) Live.MoveTo(e.Screen);
};

// Bắt đầu một lệnh: mở hộp ngay tại con trỏ
Live.Open(mousePoint, "Chiều dài");

// Enter đóng và trả giá trị; Esc trả về không
Live.Cancelled += (_, _) => commandLine.PromptCancel();`
            },
            {
                title: "Bốn kiểu đọc, một hàm thuần",
                desc: "<code>TryParse</code> là static và không cần cửa sổ, nên luật đọc kiểm được bằng test. Chiều dài và góc dùng chung <code>CadInputParser</code> với dòng lệnh, nên <code>3.2m</code> là 3200 mm ở cả hai nơi.",
                xaml: `<ui:DynamicInput Kind="Coordinate" Prompt="Điểm" />`,
                csharp: `DynamicInput.TryParse(DynamicInputKind.Distance, "3'-6\"", out double mm);
DynamicInput.TryParse(DynamicInputKind.Angle, "45°", out double deg);

// Kiểm tra trước khi mở hộp
if (!DynamicInput.TryParse(DynamicInputKind.Coordinate, text, out _, out Point3D p))
    commandLine.WriteLine("Toạ độ không hợp lệ.", CommandMessageKind.Error);`
            },
            {
                title: "Một viền, một gạch chân, một khối đơn vị",
                desc: "Bản trước vẽ khung bo góc rồi lồng một <code>TextBox</code> mặc định vào trong — hai đường viền cho một con số, nên nó đọc như form trong form và trông lạc hẳn khỏi phần còn lại của thư viện. Bản này chỉ còn <b>một</b> đường viền, ở tấm nền; ô nhập dùng style <code>CONCOST.TextBox.Underline</code>, tức là một gạch chân 1.5 px tự đổi màu khi focus, nên con số nằm <b>trên</b> tấm chứ không nằm trong hộp thứ hai. Đơn vị tách thành khối riêng bên phải dòng câu hỏi thay vì dán vào chuỗi prompt, nhờ vậy câu hỏi dịch được mà không thành một dòng dài. Giá trị không đọc được thì chính đường viền ấy chuyển sang <code>CONCOST.Brush.Danger</code> và dòng lý do hiện bên dưới.",
                callout: {
                    type: "note",
                    body: "Mọi màu và font đi qua <code>SetResourceReference</code>, nên đổi theme là hộp đổi theo ngay — trong control không có mã màu nào viết cứng."
                },
                xaml: `<ui:DynamicInput Kind="Distance" Prompt="Chiều dài" FieldWidth="140" />`,
                csharp: `input.Prompt     = "Chiều dài";  // dòng đầu
input.UnitSuffix = "(mm)";        // khối riêng bên phải, không dán vào prompt
input.FieldWidth = 140;           // ô toạ độ cần rộng hơn ô chiều dài
input.ShowUnit   = false;         // bản vẽ đã có đơn vị ở thanh trạng thái

input.Text = "abc";
input.Commit();                   // false: viền sang màu cảnh báo, hộp vẫn mở`
            },
            {
                title: "Ba kiểu dáng, ba bối cảnh",
                desc: "Cùng một hộp, ba cách ở. <code>Floating</code> có viền 1 px và bóng, dùng khi nó phải nổi trên bản vẽ. <code>Compact</code> bỏ viền, hạ bóng và xếp câu hỏi cùng dòng với ô nhập để nằm gọn trong dải công cụ thấp 40 px. <code>Glass</code> để nền trong suốt 6% và luôn có bóng ít nhất 2 để đọc được trên ảnh render tối. Không phần tử nào bị dựng lại khi đổi kiểu — chỉ <code>Grid.Row</code>, <code>Grid.Column</code> và phần khung đổi — nên chữ đang gõ và focus giữ nguyên.",
                xaml: `<ui:DynamicInput Variant="Glass" Elevation="3" Kind="Angle" Text="45" />`,
                csharp: `input.Variant = DynamicInputVariant.Floating; // tấm nổi trên bản vẽ
input.Variant = DynamicInputVariant.Compact;   // một dòng trong dải công cụ
input.Variant = DynamicInputVariant.Glass;     // nền mờ trên ảnh render`
            },
            {
                title: "Tự lật quanh mép và đường gióng về điểm neo",
                desc: "<code>Placement = Auto</code> so hộp với control cha theo từng trục: gần mép phải thì hộp sang trái, gần mép dưới thì lên trên, và đo lại mỗi lần hộp đổi kích thước nên nới <code>FieldWidth</code> giữa chừng cũng không đẩy hộp ra ngoài. Đường gióng nét đứt nối góc gần nhất của tấm về đúng <code>Anchor</code> mà con trỏ đang ở, để mắt không phải đoán hộp thuộc về điểm nào. Hộp kiểu <code>Compact</code> không có chỗ nên tự tắt đường gióng.",
                xaml: `<ui:DynamicInput Placement="Auto" Offset="18" ShowLeader="True" />`,
                csharp: `input.Placement  = DynamicInputPlacement.Auto;  // lật khi sắp tràn
input.Placement  = DynamicInputPlacement.Left;  // hoặc ép Right / Left / Above
input.Offset     = 18;                          // khoảng hở tới điểm neo
input.ShowLeader = false;                       // tắt đường gióng`
            }
        ],
        related: ["commandline", "viewportcontrol", "searchbox"]
    },

    cadstatusbar: {
        title: "CadStatusBar - thanh trạng thái CAD",
        badge: "CAD & Drafting",
        desc: "Một tấm duy nhất, không viền lồng viền: tóm tắt lựa chọn và mặt phẳng làm việc bên trái, toạ độ con trỏ ở giữa, năm công tắc bám điểm có nhãn SNAP, GRID, ORTHO, POLAR, OSNAP cùng hàng đợi tác vụ nền và chuông thông báo bên phải. Công tắc nằm phẳng trong tấm và chỉ sáng bằng màu nhấn nhạt khi bật — không phải một khối cam cho mỗi bit đang mở. Thanh chỉ giữ các bit ấy đồng bộ, ý nghĩa là của host.",
        keywords: ["status bar", "snap", "ortho", "polar", "coordinates", "notification", "task", "toggle"],
        props: [
            { name: "Variant", type: "CadStatusBarVariant", default: "Floating", desc: "<code>Floating</code> là tấm có một viền và một bóng, đặt ở đáy cửa sổ. <code>Seamless</code> không vẽ tấm nào, để đặt vào dải mà host đã dựng. <code>Glass</code> là tấm mờ 90% cho bản vẽ phía sau." },
            { name: "ControlHeight", type: "double", default: "24", desc: "Chiều cao của mọi công tắc, bộ đếm và nút tách trên thanh. Style công tắc mang sẵn mức tối thiểu 34 px, thanh xoá nó trước nên con số này mới có tác dụng." },
            { name: "IconSize", type: "double", default: "13", desc: "Cỡ icon trong công tắc và bộ đếm." },
            { name: "CornerRadius", type: "CornerRadius", default: "6", desc: "Bo góc của tấm; nửa chiều cao thì thành viên thuốc." },
            { name: "Elevation", type: "int", default: "1", desc: "Độ nổi bóng 0–4; <code>Glass</code> luôn giữ ít nhất 2." },
            { name: "ShowToggleLabels", type: "bool", default: "true", desc: "In SNAP, GRID, ORTHO, POLAR, OSNAP cạnh icon. Tắt trên thanh hẹp — năm nhãn tốn chỗ hơn năm công tắc đáng giá." },
            { name: "ShowSelection", type: "bool", default: "true", desc: "Hiện dòng tóm tắt lựa chọn." },
            { name: "ShowWorkPlane", type: "bool", default: "true", desc: "Hiện nút tách chọn mặt phẳng làm việc." },
            { name: "ShowToggles", type: "bool", default: "true", desc: "Hiện cả cụm năm công tắc như một nhóm." },
            { name: "ShowTaskQueue", type: "bool", default: "true", desc: "Cho phép hiện bộ đếm tác vụ nền; không có việc nào chạy thì nó vẫn ẩn." },
            { name: "ShowNotifications", type: "bool", default: "true", desc: "Hiện chuông thông báo; số chỉ hiện khi còn mục chưa đọc." },
            { name: "ShowDividers", type: "bool", default: "true", desc: "Hiện hai vạch phân cách; vạch cạnh một nhóm bị ẩn cũng tự rút." },
            { name: "ShowCoordinates", type: "bool", default: "true", desc: "Hiện ô toạ độ ở cột giữa." },
            { name: "SelectionCount", type: "int", default: "0", desc: "Số đối tượng đang chọn; 0 đọc là “Nothing selected”." },
            { name: "WorkPlane", type: "UcsMode", default: "World", desc: "Mặt phẳng hình học mới được vẽ lên: <code>World</code>, <code>Top</code>, <code>Front</code>, <code>Right</code>." },
            { name: "Coordinates", type: "Point3D", default: "0, 0, 0", desc: "Toạ độ con trỏ; đổi qua <code>UpdateCoordinates</code> theo từng lần di chuột." },
            { name: "UnitSuffix", type: "string", default: "mm", desc: "Đơn vị ghi sau toạ độ." }
        ],
        sections: [
            {
                title: "Năm công tắc, một nguồn",
                desc: "Khai báo state một lần trong <code>ApplicationStateService</code>; ribbon, phím tắt và thanh trạng thái cùng đọc ghi trên đó, nên không có hai nơi giữ cùng một cờ.",
                xaml: `<ui:CadStatusBar Variant="Floating" SelectionCount="{Binding Selection.Count}"
               Coordinates="{Binding Cursor}"
               WorkPlane="Top"
               ToggleChanged="OnToggleChanged"
               WorkPlaneChanged="OnWorkPlaneChanged" />`,
                csharp: `state.ToggleChanged += (_, e) =>
{
    switch (e.Toggle)
    {
        case CadToggle.Ortho: viewport.OrthoMode = e.IsEnabled; break;
        case CadToggle.Snap:  viewport.SnapToGrid = e.IsEnabled; break;
    }
};

// Thanh trạng thái tự cập nhật khi state đổi
state.Attach(statusBar);`
            },
            {
                title: "Một tấm, không viền lồng viền",
                desc: "Thanh tự vẽ đúng một tấm và đó là đường viền duy nhất. Năm công tắc in thẳng lên tấm, phẳng cho tới khi rê chuột; bật thì nền là <code>PrimarySoft</code> 12% chứ không phải màu nhấn đặc, vì năm khối cam thường trực đọc thành năm báo động. Bộ đếm tác vụ và chuông mặc cùng một dáng, nên không còn quả viên xám nào tranh chỗ với thanh. Đừng bọc thanh trong một <code>Border</code> nữa — hai viền lồng nhau là lỗi quen thuộc khi ghép thanh trạng thái.",
                xaml: `<ui:CadStatusBar ControlHeight="24" IconSize="13"
               CornerRadius="6" Elevation="1"
               ShowToggleLabels="True" />`,
                csharp: `bar.ShowToggleLabels = false;   // thanh hẹp thì bỏ SNAP/OSNAP
bar.ControlHeight    = 30;      // cỡ chạm ngón tay
bar.CornerRadius     = new CornerRadius(15);   // viên thuốc

// Mỗi nhóm bật/tắt riêng, vạch phân cách rút theo
bar.ShowSelection = false;
bar.ShowToggles   = false;`
            },
            {
                title: "Ba kiểu dáng, ba bối cảnh",
                desc: "Cùng một thanh, khác cái nó đứng trên. <code>Floating</code> có viền 1 px và bóng — thanh neo ở đáy cửa sổ. <code>Seamless</code> bỏ cả tấm, không viền, không bóng, nên đặt vào dải có sẵn của host mà không thành hai lớp. <code>Glass</code> để tấm mờ 90% và luôn có bóng ít nhất 2 để chữ vẫn đọc được trên cảnh 3D.",
                xaml: `<ui:CadStatusBar Variant="Glass" ShowSelection="False" />`,
                csharp: `bar.Variant = CadStatusBarVariant.Seamless;  // nằm trong dải của host
bar.Variant = CadStatusBarVariant.Glass;     // nổi trên cảnh 3D
bar.Variant = CadStatusBarVariant.Floating;  // tấm có viền + bóng`
            },
            {
                title: "Bớt nhóm thì vạch phân cách rút theo",
                desc: "Mỗi nhóm có công tắc hiện/ẩn riêng: tóm tắt, mặt phẳng, cụm công tắc, tác vụ nền, thông báo, toạ độ. Vạch phân cách không phải trang trí cố định — nó biết nhóm bên cạnh nó đã đi chưa, nên thanh đã bỏ một cụm không để lại vạch lơ lửng ở cuối. Bộ đếm tác vụ còn tự ẩn khi hàng đợi rỗng, và <code>ShowTaskQueue = false</code> thì nó không bao giờ hiện.",
                xaml: `<ui:CadStatusBar ShowTaskQueue="False"
               ShowNotifications="True"
               ShowDividers="False" />`,
                csharp: `bar.ShowWorkPlane = false;   // mặt phẳng do ribbon giữ
bar.ShowDividers  = false;   // bỏ cả hai vạch một lúc`
            },
            {
                title: "Toạ độ, tác vụ nền và thông báo",
                desc: "<code>UpdateCoordinates</code> được gọi theo con trỏ, không theo từng pixel vẽ. Tác vụ nền là danh sách có phần trăm; thông báo là danh sách có mức độ, chưa đọc thì hiện số.",
                xaml: `<ui:CadStatusBar x:Name="Status" UnitSuffix="mm" />`,
                csharp: `Status.UpdateCoordinates(new Point3D(1250, 800, 0));

var task = new CadTask("Đang bóc khối lượng", 0);
Status.AddTask(task);
task.Percent = 45;
Status.RefreshTasks();

Status.Notify(new CadNotification(
    "Đã lưu bản vẽ", "level1.rvt", CadNotificationSeverity.Success));`
            }
        ],
        related: ["commandline", "viewportcontrol", "statusbar"]
    },

    projectbrowser: {
        title: "ProjectBrowser - cây dự án",
        badge: "CAD & Drafting",
        desc: "Cây các view, sheet, family, schedule và level. Tìm kiếm giữ nguyên đường dẫn tới node khớp, lọc theo loại, nhóm theo loại, sắp xếp theo tên hoặc loại, menu chuột phải khác nhau theo loại node. Ô tìm, hai combobox và công tắc nhóm đều nằm sẵn trong control — thêm ô thứ hai bên ngoài là cách nhanh nhất để nhân đôi nó.",
        keywords: ["project browser", "tree", "views", "sheets", "palette", "chrome"],
        props: [
            { name: "Nodes", type: "ObservableCollection<ProjectTreeNode>", default: "rỗng", desc: "Các node gốc; thêm bằng <code>AddRoot</code> rồi <code>Refresh</code>." },
            { name: "SelectedNode", type: "ProjectTreeNode?", default: "null", desc: "Node đang chọn; gán qua <code>Select</code> để giữ nguyên logic chọn." },
            { name: "GroupMode", type: "ProjectGroupMode", default: "None", desc: "<code>None</code> giữ nguyên cây; <code>ByKind</code> nhóm theo loại node." },
            { name: "SortMode", type: "ProjectSortMode", default: "NameAscending", desc: "<code>NameAscending</code>, <code>NameDescending</code>, <code>Kind</code>." },
            { name: "KindFilter", type: "ProjectNodeKind?", default: "null", desc: "Chỉ hiện một loại node; <code>null</code> là hiện tất cả." },
            { name: "FilterText", type: "string", default: "string.Empty", desc: "Chuỗi tìm hiện tại (chỉ đọc) — đổi qua <code>SetSearchText</code>." },
            { name: "VisibleNodes", type: "IReadOnlyList<ProjectTreeNode>", default: "rỗng", desc: "Đúng những node đang hiển thị sau khi lọc, nhóm và sắp xếp." },
            { name: "Variant", type: "ProjectBrowserVariant", default: "Seamless", desc: "Ai vẽ viền: <code>Seamless</code> cho palette của host, <code>Card</code> khi đứng riêng, <code>Floating</code> khi nổi trên bản vẽ." },
            { name: "ControlHeight", type: "double", default: "24", desc: "Cao của ô tìm, hai combobox và nút công cụ. Control tự đặt <code>MinHeight = 0</code> trước, vì style combo có sàn 34 px." },
            { name: "IconSize", type: "double", default: "13", desc: "Cỡ glyph trong nút công cụ và icon của từng node." },
            { name: "CornerRadius", type: "CornerRadius", default: "8", desc: "Bo góc của tấm nền." },
            { name: "Elevation", type: "int", default: "0", desc: "Độ nổi của tấm; <code>Seamless</code> bỏ qua giá trị này." },
            { name: "ShowToolbar", type: "bool", default: "true", desc: "Hàng công cụ phía trên cây." },
            { name: "ShowSearch", type: "bool", default: "true", desc: "Ô tìm kiếm." },
            { name: "ShowKindFilter", type: "bool", default: "true", desc: "Combobox lọc theo loại." },
            { name: "ShowSort", type: "bool", default: "true", desc: "Combobox sắp xếp." },
            { name: "ShowGroup", type: "bool", default: "true", desc: "Công tắc nhóm theo loại." },
            { name: "ShowCount", type: "bool", default: "true", desc: "Dòng đếm ở chân palette." },
            { name: "SearchPlaceholder", type: "string", default: "\"Search the project\"", desc: "Chữ mờ trong ô tìm." },
            { name: "AllKindsCaption", type: "string", default: "\"All kinds\"", desc: "Mục đầu của bộ lọc loại." },
            { name: "SortCaptions", type: "IEnumerable<string>", default: "Name A–Z, Name Z–A, By kind", desc: "Ba nhãn sắp xếp; để thiếu thì nhãn đó giữ mặc định." },
            { name: "EmptyText", type: "string", default: "\"Nothing matches this search.\"", desc: "Câu hiện thay chỗ cây khi bộ lọc không khớp." },
            { name: "CountCaption", type: "string", default: "\"{0} items\"", desc: "Dòng đếm ở chân, <code>{0}</code> là số node đang hiện." }
        ],
        sections: [
            {
                title: "Dựng cây từ mô hình",
                desc: "<code>ProjectTreeNode</code> chỉ có tên, loại và con — không phải control. Sau khi thêm node, gọi <code>Refresh</code> để dựng lại cây hiển thị.",
                xaml: `<ui:ProjectBrowser x:Name="Browser"
                    Height="320"
                    OpenRequested="OnOpenView"
                    DeleteRequested="OnDeleteNode" />`,
                csharp: `var project = Browser.AddRoot("Building B", ProjectNodeKind.Project);

var views = project.Add("Views", ProjectNodeKind.Folder);
views.Add("Level 1", ProjectNodeKind.View);
views.Add("Level 2", ProjectNodeKind.View);

project.Add("A101", ProjectNodeKind.Sheet);

Browser.Refresh();
Browser.ExpandAll();

// Bấm đúp hoặc Enter trên một node: mở nó
Browser.Open(views.Children[0]);`
            },
            {
                title: "Một tấm duy nhất, không có chip nào",
                desc: "<code>Variant</code> quyết định ai vẽ viền. Đặt <code>Seamless</code> khi palette của host đã có viền — hai viền lồng nhau là lỗi thường gặp nhất khi ghép. Nút công cụ là glyph ghost nên tấm nền là đường viền duy nhất trong palette.",
                xaml: `<ui:ProjectBrowser Variant="Seamless"
                    ControlHeight="24"
                    IconSize="13" />`,
                csharp: `browser.Variant = ProjectBrowserVariant.Seamless;   // palette đã có viền
browser.Variant = ProjectBrowserVariant.Floating;
browser.Elevation = 3;                              // chỉ Floating mới dùng

// Style combo có sàn 34 px — control tự đưa sàn về 0 rồi mới gán cao
browser.ControlHeight = 24;
browser.IconSize = 13;`
            },
            {
                title: "Tìm, nhóm và sắp xếp",
                desc: "Ô tìm, bộ lọc loại, bộ sắp xếp và công tắc nhóm đều đã nằm trong control. Tìm không phân biệt hoa thường và giữ tổ tiên của node khớp, nên kết quả vẫn đọc được như một nhánh. <code>VisibleNodes</code> trả đúng những gì đang hiện — tiện cho test và cho dòng đếm.",
                xaml: `<ui:ProjectBrowser x:Name="Browser"
                    ShowToolbar="True"
                    ShowSearch="True"
                    ShowKindFilter="True" />`,
                csharp: `Browser.SetSearchText("lev");   // hiện Level 1, Level 2 và tổ tiên

Browser.GroupMode = ProjectGroupMode.ByKind;
Browser.SortMode  = ProjectSortMode.NameAscending;

Browser.SetSearchText(null);   // xoá lọc

// Tắt cả nhóm: header tự biến mất, cây bắt đầu từ mép trên
Browser.ShowToolbar = false;`
            },
            {
                title: "Chữ do host quyết định",
                desc: "Không có nhãn nào viết cứng trong code. Mọi câu chữ đều là property, nên một shell tiếng Việt không phải sống với “All kinds”.",
                xaml: `<ui:ProjectBrowser SearchPlaceholder="Tìm view, sheet, family…"
                    AllKindsCaption="Mọi loại"
                    CountCaption="{0} mục" />`,
                csharp: `browser.SortCaptions  = new[] { "Tên A→Z", "Tên Z→A", "Theo loại" };
browser.EmptyText    = "Không có mục nào khớp bộ lọc.";
browser.CountCaption = "{0} mục";`
            }
        ],
        related: ["documentworkspace", "treemap", "treedatagrid"]
    },

    documentworkspace: {
        title: "DocumentWorkspace - vùng cửa sổ bản vẽ",
        badge: "CAD & Drafting",
        desc: "Host các view dưới dạng tab, chia khung dọc/ngang/lưới, tách một view ra cửa sổ riêng, và cho host <b>từ chối</b> việc đóng khi còn thay đổi chưa lưu.",
        keywords: ["document", "tab host", "tile", "split", "float", "workspace"],
        props: [
            { name: "Documents", type: "ObservableCollection<DocumentPane>", default: "rỗng", desc: "Các pane đang mở; thêm bằng <code>AddDocument</code>." },
            { name: "ActiveDocument", type: "DocumentPane?", default: "null", desc: "Pane đang hiển thị (chỉ đọc) — đổi bằng cách chọn tab hoặc <code>Select</code>." },
            { name: "TileMode", type: "DocumentTileMode", default: "Single", desc: "<code>Single</code>, <code>Vertical</code>, <code>Horizontal</code>, <code>Grid</code>." },
            { name: "PaneCount", type: "int", default: "0", desc: "Số pane đang có trong vùng hiển thị (chỉ đọc)." },
            { name: "PlaceholderTitle", type: "string", default: "\"No views open\"", desc: "Tiêu đề khi chưa có pane nào." },
            { name: "PlaceholderText", type: "string", default: "\"Open a view…\"", desc: "Dòng gợi ý dưới tiêu đề đó." },
            { name: "DocumentPane.IsDirty", type: "bool", default: "false", desc: "Đánh dấu pane có thay đổi chưa lưu; tab hiện dấu <code>*</code>." }
        ],
        sections: [
            {
                title: "Thêm, chia khung, tách cửa sổ",
                desc: "<code>TileMode</code> chia đều vùng hiển thị; <code>FloatDocument</code> đưa một pane ra <code>Window</code> riêng và tự đóng cửa sổ đó khi pane bị đóng.",
                xaml: `<ui:DocumentWorkspace x:Name="Workspace"
                      TileMode="Vertical"
                      DocumentClosing="OnDocumentClosing" />`,
                csharp: `var pane = Workspace.AddDocument("Level 1", viewport, IconKind.Viewport3D);

Workspace.TileMode = DocumentTileMode.Vertical;   // Single | Vertical | Horizontal | Grid
Window? floating = Workspace.FloatDocument(pane);
Workspace.CloseOthers(pane);`
            },
            {
                title: "Đóng có thể bị chặn",
                desc: "<code>DocumentClosing</code> bắn trước khi pane rời cây; đặt <code>Cancel</code> để giữ lại — đó là chỗ đặt câu hỏi “lưu thay đổi?”.",
                xaml: `<ui:DocumentWorkspace x:Name="Workspace" />`,
                csharp: `Workspace.DocumentClosing += (_, e) =>
{
    if (!e.Pane.IsDirty) return;

    var answer = MessageBox.Show($"Lưu {e.Pane.Title}?", "CONCOST",
        MessageBoxButton.YesNoCancel);

    if (answer == MessageBoxResult.Cancel) e.Cancel = true;
};

// "No views open" khi chưa có pane nào
Workspace.PlaceholderTitle = "Chưa mở view nào";`
            }
        ],
        related: ["documenttabcontrol", "dockingmanager", "projectbrowser"]
    },

    propertiespalette: {
        title: "PropertiesPalette - bảng thuộc tính",
        badge: "CAD & Drafting",
        desc: "Bảng thuộc tính của đối tượng đang chọn: tiêu đề, ô tìm, lưới theo nhóm, dòng thông báo lỗi và nút reset về mặc định. Mỗi lần ghi đi qua <code>TransactionManager</code> nên <b>undo một bước</b>.",
        keywords: ["properties", "palette", "inspector", "validation", "transaction", "reset"],
        props: [
            { name: "GridControl", type: "PropertyGrid", default: "—", desc: "Lưới bên trong, đã có sẵn tiêu đề và ô tìm của palette." },
            { name: "Properties", type: "PropertyManager", default: "PropertyManager.Current", desc: "Sổ đăng ký descriptor dùng để dựng bảng." },
            { name: "Selection", type: "SelectionManager", default: "SelectionManager.Current", desc: "Nguồn chọn; palette tự dựng lại khi lựa chọn đổi." },
            { name: "Target", type: "object?", default: "null", desc: "Đối tượng đang hiển thị (chỉ đọc) — đổi qua <code>Build</code>." },
            { name: "ValidationMessage", type: "string?", default: "null", desc: "Lỗi đang hiện ở dải dưới lưới; <code>null</code> khi ẩn." }
        ],
        sections: [
            {
                title: "Khai báo descriptor, không dùng reflection",
                desc: "Reflection sẽ lộ mọi property của WPF và không diễn đạt được đơn vị, danh sách chọn hay validator. Descriptor viết tay nói đúng thứ bảng cần và không phụ thuộc tên property.",
                xaml: `<ui:PropertiesPalette x:Name="Palette"
                       Height="360"
                       PropertyCommitted="OnPropertyCommitted" />`,
                csharp: `properties.Register<Beam>(
    new PropertyDescriptor("Length",
        b => ((Beam)b).Length,
        (b, v) => ((Beam)b).Length = Convert.ToDouble(v))
    {
        DisplayName = "Chiều dài",
        Category = "Kích thước",
        Editor = PropertyEditorKind.Numeric,
        Unit = "mm",
        DefaultValue = 6000.0,
        Validator = (_, v) => v is double d && d > 0 ? null : "Chiều dài phải dương."
    });

// Chọn đối tượng rồi dựng bảng
selection.Select(beam);
palette.Build(beam);`
            },
            {
                title: "Reset về mặc định",
                desc: "<code>ResetProperty</code> trả về <code>DefaultValue</code> nếu descriptor khai báo; nếu không, nó trả <code>false</code> và lý do trong <code>error</code> thay vì im lặng.",
                xaml: `<ui:PropertiesPalette x:Name="Palette" />`,
                csharp: `if (!palette.ResetProperty("Length", out string? error))
    MessageBox.Show(error);

// Sửa từ code cũng đi qua transaction
properties.TrySetValueFromText(beam, "Length", "7500", out string? why);`
            }
        ],
        related: ["propertygrid", "viewcontrolbar", "documentworkspace"]
    },

    applicationtitlebar: {
        title: "ApplicationTitleBar - thanh tiêu đề",
        badge: "CAD & Drafting",
        desc: "Tiêu đề cửa sổ theo kiểu tài liệu: tên bản vẽ kèm dấu <code>*</code> khi có thay đổi chưa lưu, dải quick-access (tràn vào menu khi hết chỗ), kéo để di chuyển, đúp chuột để phóng to, và ba nút cửa sổ phát sự kiện.",
        keywords: ["title bar", "caption", "quick access", "window chrome", "modified"],
        sections: [
            {
                title: "Tiêu đề có dấu chưa lưu",
                desc: "<code>ComposeTitle</code> là hàm thuần, nên cùng một chuỗi tiêu đề dùng được cho cả caption lẫn tooltip và test so khớp được.",
                xaml: `<ui:ApplicationTitleBar ApplicationName="CONCOST"
                       DocumentTitle="{Binding Title}"
                       IsDocumentModified="{Binding IsDirty}"
                       CloseRequested="OnCloseRequested" />`,
                csharp: `titleBar.IsDocumentModified = true;

string caption = ApplicationTitleBar.ComposeTitle("Level 1", true, "CONCOST");
// → "Level 1* — CONCOST"`

            },
            {
                title: "Quick access tràn vào menu",
                desc: "Dải nút nhận <code>TitleBarAction</code> (icon, tooltip, command). Khi cửa sổ hẹp, phần không vừa được gom vào một nút menu thay vì bị cắt.",
                xaml: `<ui:ApplicationTitleBar ShowQuickAccess="True" />`,
                csharp: `titleBar.QuickAccess.Add(new TitleBarAction(IconKind.Undo, "Hoàn tác", UndoCommand));
titleBar.QuickAccess.Add(new TitleBarAction(IconKind.Save, "Lưu", SaveCommand));

titleBar.RefreshQuickAccess();`
            }
        ],
        related: ["applicationinfobar", "statusbar", "commandbar"]
    },

    applicationinfobar: {
        title: "ApplicationInfoBar - thanh thông tin",
        badge: "CAD & Drafting",
        desc: "Một dòng thông báo nằm dưới ribbon: bốn mức (Info, Success, Warning, Error) quyết định icon và nền pha màu, kèm một hành động và nút đóng. Nó ở trong layout và tự thu gọn nên host không phải thêm bớt hàng.",
        keywords: ["info bar", "message bar", "warning", "severity", "notification"],
        sections: [
            {
                title: "Bốn mức, một hành động",
                desc: "Một hành động là cố ý: thanh có ba lựa chọn là một hộp thoại đóng giả dải thông báo. Cần nhiều lựa chọn thì dùng <code>Dialog</code>.",
                xaml: `<ui:ApplicationInfoBar x:Name="InfoBar"
                       Severity="Warning"
                       Title="Cảnh báo"
                       Message="3 cấu kiện vượt nhịp."
                       ActionText="Xem chi tiết"
                       ActionInvoked="OnInfoAction"
                       Closed="OnInfoClosed" />`,
                csharp: `InfoBar.Show("Cảnh báo", "3 cấu kiện vượt nhịp.", CadNotificationSeverity.Warning);

InfoBar.ActionInvoked += (_, _) => ShowWarningList();
InfoBar.Closed         += (_, _) => Settings.DismissedWarningTip = true;`
            },
            {
                title: "Nền pha theo màu nhấn",
                desc: "Nền là màu nhấn của mức với alpha <code>0x16</code> và viền là chính màu đó, nên đổi theme là đổi theo. Control tự đọc lại từ điển theme khi layout lần đầu, không phụ thuộc thứ tự merge.",
                xaml: `<ui:ApplicationInfoBar x:Name="InfoBar" IsClosable="False" />`,
                csharp: `// Thu gọn mà không bắn Closed
InfoBar.Hide();

// Nội dung dài: thanh vẫn một dòng, message cắt bằng ellipsis
InfoBar.Show("Đồng bộ", "Đang lấy dữ liệu từ mô hình trung tâm…", CadNotificationSeverity.Info);`
            }
        ],
        related: ["applicationtitlebar", "cadstatusbar", "growl"]
    },

    backstageview: {
        title: "BackstageView - trang File toàn cửa sổ",
        badge: "CAD & Drafting",
        desc: "Trang phủ toàn cửa sổ sau nút File: dải mục bên trái (kèm nút Back) và nội dung của mục đang chọn bên phải. Nó là một panel của cửa sổ chứ không phải popup, nên trang Options hay Print dùng được cả cửa sổ.",
        keywords: ["backstage", "file menu", "ribbon", "info", "print", "options"],
        sections: [
            {
                title: "Nội dung là của host",
                desc: "Mục backstage chỉ có tiêu đề, icon và một <code>Content</code> bất kỳ, nên file list, preview in hay form cài đặt đều đặt được mà control không cần biết đó là gì.",
                xaml: `<ui:RibbonBackstageView x:Name="Backstage"
                      ApplicationName="CONCOST"
                      IsOpen="False"
                      SelectedItemChanged="OnBackstagePage" />`,
                csharp: `Backstage.Items.Add(new BackstageItem("Thông tin", IconKind.Info)
{
    Content = infoPanel
});

Backstage.Items.Add(new BackstageItem("In", IconKind.File)
{
    IsSeparator = true,          // kẻ một dòng phía trên
    Content = printPreview
});

Backstage.Open();
Backstage.Select("Thông tin");`
            },
            {
                title: "Đóng bằng Esc hoặc nút Back",
                desc: "<code>IsOpen</code> chỉ đổi <code>Visibility</code>, nên vùng vẽ bên dưới giữ nguyên trạng thái. <code>IsOpenChanged</code> để host dừng hoạt ảnh hoặc trả focus.",
                xaml: `<ui:RibbonBackstageView IsOpen="{Binding IsFileOpen, Mode=TwoWay}" />`,
                csharp: `Backstage.IsOpenChanged += (_, _) =>
{
    if (!Backstage.IsOpen) viewport.Focus();
};`
            }
        ],
        related: ["ribbon", "drawermenu", "applicationtitlebar"]
    },

    fontpicker: {
        title: "FontPicker - chọn font",
        badge: "CAD & Drafting",
        desc: "Ô chọn font family và cỡ chữ, hai nút bật đậm/nghiêng, kèm dòng xem trước sống. Đủ cho hộp thoại text style mà không phải tự ghép bốn control.",
        keywords: ["font", "typeface", "font size", "bold", "italic", "text style"],
        sections: [
            {
                title: "Xem trước ngay khi đổi",
                desc: "Dòng preview dùng đúng family, cỡ, đậm và nghiêng đang chọn — <code>EffectiveWeight</code> và <code>EffectiveStyle</code> trả ra giá trị đã tổng hợp để host áp cho đối tượng thật.",
                xaml: `<ui:FontPicker SelectedFontFamily="Segoe UI"
               SelectedFontSize="12"
               ShowPreview="True"
               PreviewText="Bê tông B25 — 400 × 600" />`,
                csharp: `picker.SelectionChanged += (_, _) =>
{
    label.FontFamily = picker.SelectedFontFamily;
    label.FontSize   = picker.SelectedFontSize;
    label.FontWeight = picker.EffectiveWeight;
    label.FontStyle  = picker.EffectiveStyle;
};`
            },
            {
                title: "Không cần preview",
                desc: "<code>ShowPreview=False</code> cho thanh công cụ; danh sách family vẫn lấy từ <code>Fonts.SystemFontFamilies</code> đã sắp xếp.",
                xaml: `<ui:FontPicker x:Name="Font" ShowPreview="False" />`,
                csharp: `Font.SelectedFontFamily = new FontFamily("Consolas");
Font.SelectedFontSize = 10;
Font.IsBold = true;`
            }
        ],
        related: ["colorpicker", "propertygrid", "textbox"]
    }
};
