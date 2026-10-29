// New control pages + C# overrides for pages that shipped without code-behind samples.
const DOCS_CUSTOM = {

    /* ============================ LAYOUT & SHELL ============================ */
    badge: {

        title: "Badge — huy hiệu số lượng & trạng thái",
        badge: "Layout & Shell",
        desc: "Huy hiệu số lượng / trạng thái, dùng độc lập hoặc gắn lên bất kỳ <code>ButtonBase</code> nào qua <code>attached:ButtonElement.Badge</code>.",
        keywords: ["notification", "huy hiệu", "số lượng", "count", "dot"],
        variants: [
            { key: "CONCOST.Badge.Base", desc: "Nền đặc theo Appearance, chữ trắng." },
            { key: "CONCOST.Badge.Pill", desc: "Bo tròn hoàn toàn cho số nhiều chữ số." },
            { key: "CONCOST.Badge.Dot", desc: "Chấm nhỏ không nội dung (IsDot=True)." },
            { key: "CONCOST.Badge.Outline", desc: "Chỉ viền, nền trong — hợp với toolbar sáng." },
            { key: "CONCOST.Badge.Soft", desc: "Nền nhạt 12%, chữ màu đậm — dịu hơn nền đặc." }
        ],
        sections: [
            {
                title: "Badge độc lập với 6 appearance",
                desc: "<code>Appearance</code> nhận <code>Primary</code>, <code>Success</code>, <code>Warning</code>, <code>Danger</code>, <code>Info</code>, <code>Neutral</code>. Đặt <code>IsDot=\"True\"</code> để hiện chấm thay vì số.",
                xaml: `<StackPanel Orientation="Horizontal" Margin="24">
    <ui:Badge Value="12" Appearance="Danger" Margin="0,0,10,0" />
    <ui:Badge Value="NEW" Appearance="Success" Style="{DynamicResource CONCOST.Badge.Pill}" Margin="0,0,10,0" />
    <ui:Badge Value="99+" Appearance="Primary" Style="{DynamicResource CONCOST.Badge.Outline}" Margin="0,0,10,0" />
    <ui:Badge IsDot="True" Appearance="Warning" Margin="0,0,10,0" />
    <ui:Badge Value="Draft" Appearance="Neutral" Style="{DynamicResource CONCOST.Badge.Soft}" />
</StackPanel>`,
                csharp: `// Tạo badge bằng code và cập nhật theo dữ liệu
var badge = new Badge
{
    Value = "12",
    Appearance = BadgeAppearance.Danger,
    Style = (Style)FindResource("CONCOST.Badge.Pill")
};

// Badge chỉ hiện khi có thông báo
badge.ShowBadge = unreadCount > 0;
badge.Value = unreadCount > 99 ? "99+" : unreadCount.ToString();`
            },
            {
                title: "Gắn badge lên Button / ToggleButton",
                desc: "Attached property <code>ButtonElement.Badge</code> vẽ badge ở góc trên-phải của nút, tự ẩn khi giá trị là <code>null</code>.",
                xaml: `<Button Content="Thông báo"
        Style="{DynamicResource CONCOST.Button.Icon}"
        attached:ButtonElement.Badge="5" />

<Button Content="Sync"
        Style="{DynamicResource CONCOST.Button.Primary}"
        attached:ButtonElement.Badge="NEW"
        attached:ButtonElement.IsLoading="{Binding IsSyncing}" />`,
                csharp: `// Đổi badge runtime
ButtonElement.SetBadge(NotificationButton, unreadCount > 0 ? unreadCount.ToString() : null);

// Bật trạng thái loading kèm text
ButtonElement.SetIsLoading(SyncButton, true);
ButtonElement.SetLoadingText(SyncButton, "Đang đồng bộ...");`
            }
        ],
        related: ["card", "tag", "splitbutton"]
    },

    expander: {

        title: "Expander — khối nội dung thu gọn",
        badge: "Layout & Shell",
        desc: "Khối nội dung thu gọn/mở rộng với 18 style variant (Clean, Card, Filled, Round, Small, Primary/Success/Warning/Danger/Info, hỗ trợ 4 hướng mở).",
        keywords: ["accordion", "collapse", "thu gọn", "mở rộng"],
        variants: [
            { key: "CONCOST.Expander.Base", desc: "Mặc định, viền mảnh, header có icon mũi tên." },
            { key: "CONCOST.Expander.Card", desc: "Nền surface + shadow nhẹ, dùng trong dashboard." },
            { key: "CONCOST.Expander.Filled", desc: "Header nền đặc theo accent." },
            { key: "CONCOST.Expander.Clean", desc: "Không viền, chỉ có đường phân cách mảnh." },
            { key: "CONCOST.Expander.Round", desc: "Bo góc lớn 14px, hợp với khối cài đặt." },
            { key: "CONCOST.Expander.Small", desc: "Header 32px cho panel dày đặc." },
            { key: "CONCOST.Expander.Primary / Success / Warning / Danger / Info", desc: "Header theo màu ngữ nghĩa." },
            { key: "CONCOST.Expander.Right / Down / Up.Template", desc: "Template mở sang phải / xuống / lên tương ứng <code>ExpandDirection</code>." }
        ],
        sections: [
            {
                title: "Expander cho nhóm thuộc tính kết cấu",
                desc: "<code>ExpandDirection</code> nhận <code>Down</code>, <code>Up</code>, <code>Left</code>, <code>Right</code>. Khi mở sang ngang, header tự chuyển sang bố cục dọc.",
                xaml: `<ui:Expander Header="Section properties"
              IsExpanded="True"
              ExpandDirection="Down"
              Style="{DynamicResource CONCOST.Expander.Card}">
    <StackPanel Margin="16">
        <ui:UnitBox Header="Width (b)" Value="300" Unit="mm" DecimalPlaces="0" Margin="0,0,0,10" />
        <ui:UnitBox Header="Depth (h)" Value="600" Unit="mm" DecimalPlaces="0" Margin="0,0,0,10" />
        <ui:UnitBox Header="Cover" Value="25" Unit="mm" DecimalPlaces="0" />
    </StackPanel>
</ui:Expander>`,
                csharp: `// Mở/đóng theo logic nghiệp vụ
SectionExpander.IsExpanded = hasSelection;

// Accordion: chỉ một mục mở tại một thời điểm
void OnExpanderExpanded(object sender, RoutedEventArgs e)
{
    if (sender is not Expander current) return;
    foreach (var other in PropertyPanel.Children.OfType<Expander>())
        if (!ReferenceEquals(other, current))
            other.IsExpanded = false;
}`
            }
        ],
        related: ["card", "groupbox", "propertygrid"]
    },

    splitbutton: {

        title: "SplitButton — nút hành động kèm menu",
        badge: "Layout & Shell",
        desc: "Nút hành động chính kèm menu thả xuống — 19 thuộc tính và 13 style variant, thay thế hoàn toàn combo <code>Button</code> + <code>ContextMenu</code> tự chế.",
        keywords: ["dropdown", "menu", "hành động", "split", "toolbar"],
        variants: [
            { key: "CONCOST.SplitButton.Primary", desc: "Hành động chính của màn hình." },
            { key: "CONCOST.SplitButton.Secondary / Outline / Ghost", desc: "Mức ưu tiên giảm dần." },
            { key: "CONCOST.SplitButton.Success / Warning / Danger / Info", desc: "Theo ngữ nghĩa nghiệp vụ." },
            { key: "CONCOST.SplitButton.Pill / Square", desc: "Bo tròn hoàn toàn hoặc vuông góc." },
            { key: "CONCOST.SplitButton.Large / Small", desc: "Chiều cao 44px / 28px." }
        ],
        sections: [
            {
                title: "Split mode: nút chính + menu riêng",
                desc: "<code>HitMode=\"Split\"</code> (mặc định) tách vùng click: bấm phần thân chạy hành động, bấm mũi tên mở menu. <code>HitMode=\"DropDown\"</code> biến toàn bộ nút thành nút mở menu.",
                xaml: `<ui:SplitButton Content="Save model"
                Icon="Save"
                Style="{DynamicResource CONCOST.SplitButton.Primary}"
                HitMode="Split"
                Click="OnSaveClick">
    <ui:SplitButton.DropDownContent>
        <StackPanel MinWidth="220">
            <Button Content="Save as DXF"  Style="{DynamicResource CONCOST.Button.Icon}" HorizontalContentAlignment="Left" />
            <Button Content="Save as IFC 4.0" Style="{DynamicResource CONCOST.Button.Icon}" HorizontalContentAlignment="Left" />
            <Button Content="Export PDF report" Style="{DynamicResource CONCOST.Button.Icon}" HorizontalContentAlignment="Left" />
        </StackPanel>
    </ui:SplitButton.DropDownContent>
</ui:SplitButton>`,
                csharp: `// Điều khiển dropdown bằng code + command
var split = new SplitButton
{
    Content = "Run analysis",
    Icon = IconKind.CheckCircle,
    Placement = PlacementMode.Bottom,
    MaxDropDownHeight = 320,
    IsOpenOnHover = false,
    AutoCloseOnClick = true
};

split.Click += (s, e) => RunAnalysis();
split.DropDownOpened += (s, e) => Debug.WriteLine("Dropdown opened");
split.DropDownClosed += (s, e) => Debug.WriteLine("Dropdown closed");

// Mở menu theo phím tắt (Alt+Down)
split.IsDropDownOpen = true;`
            },
            {
                title: "Mở menu khi hover",
                desc: "Dùng cho toolbar CAD: rê chuột 200ms là menu bung ra, không cần click.",
                xaml: `<ui:SplitButton Content="Layers"
                Icon="Layers"
                Style="{DynamicResource CONCOST.SplitButton.Outline}"
                IsOpenOnHover="True"
                HoverDelay="0:0:0.2"
                StaysOpen="False"
                DropDownContent="{Binding LayerMenu}" />`,
                callout: { type: "tip", body: "Đặt <code>StaysOpen=\"True\"</code> cho menu nhiều cấp cần thao tác liên tiếp (ví dụ chọn nhiều layer) — menu sẽ không tự đóng khi click bên trong." }
            }
        ],
        related: ["button", "menu", "drawermenu"]
    },

    adaptivepanel: {

        title: "AdaptivePanel — layout theo breakpoint",
        badge: "Layout & Shell",
        desc: "ContentControl tự chọn layout theo bề rộng khả dụng qua danh sách <code>Breakpoint</code> — dùng để chuyển 3 cấp Compact / Normal / Wide mà không cần code-behind.",
        keywords: ["responsive", "breakpoint", "layout", "thích ứng"],
        sections: [
            {
                title: "Breakpoint mặc định",
                desc: "Mặc định: <code>&lt; 900</code> → Compact, <code>900–1400</code> → Normal, <code>≥ 1400</code> → Wide. <code>CurrentLayout</code> bind two-way nên có thể đổi template theo giá trị này.",
                table: [
                    { token: "CurrentLayout", desc: "Layout hiện tại, cập nhật tự động khi <code>SizeChanged</code>. Bind two-way để đổi DataTemplate." },
                    { token: "Breakpoints", desc: "Danh sách <code>Breakpoint(MinWidth, Layout)</code> — có thể thêm/bớt trong code-behind của cửa sổ." }
                ],
                xaml: `<layout:AdaptivePanel CurrentLayout="{Binding LayoutMode, Mode=TwoWay}">
    <layout:AdaptivePanel.ContentTemplateSelector>
        <!-- hoặc dùng trigger trong Style để đổi template -->
    </layout:AdaptivePanel.ContentTemplateSelector>

    <Grid>
        <!-- nội dung dashboard -->
    </Grid>
</layout:AdaptivePanel>`,
                csharp: `// Thêm breakpoint riêng cho màn hình thiết kế
var panel = new AdaptivePanel();
panel.Breakpoints.Clear();
panel.Breakpoints.Add(new Breakpoint(0, LayoutMode.Compact));
panel.Breakpoints.Add(new Breakpoint(1200, LayoutMode.Normal));
panel.Breakpoints.Add(new Breakpoint(1800, LayoutMode.Wide));

// Phản ứng khi layout đổi
panel.PropertyChanged += (s, e) => { };   // dùng DependencyPropertyDescriptor nếu cần
var descriptor = System.ComponentModel.DependencyPropertyDescriptor
    .FromProperty(AdaptivePanel.CurrentLayoutProperty, typeof(AdaptivePanel));
descriptor.AddValueChanged(panel, (s, e) => Debug.WriteLine(panel.CurrentLayout));`
            }
        ],
        related: ["card", "sidemenu", "navigationview"]
    },

    concostwindow: {

        title: "CONCOSTWindow — cửa sổ chrome tuỳ biến",
        badge: "Layout & Shell",
        desc: "Cửa sổ WPF với chrome tuỳ biến (title bar, nút min/max/close trong template) — kéo cửa sổ, double-click để maximize đều xử lý sẵn.",
        keywords: ["window", "chrome", "title bar", "cửa sổ"],
        sections: [
            {
                title: "Khai báo cửa sổ",
                desc: "Kế thừa <code>Window</code> và đặt <code>WindowStyle=\"None\"</code> trong constructor. Template cần 4 phần tử: <code>PART_TitleBar</code>, <code>PART_MinimizeButton</code>, <code>PART_MaximizeButton</code>, <code>PART_CloseButton</code>.",
                xaml: `<!-- MainWindow.xaml -->
<win:CONCOSTWindow x:Class="MyApp.MainWindow"
                   xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                   xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                   xmlns:win="clr-namespace:CONCOST.UI.Controls.Windows;assembly=CONCOST.UI"
                   Title="CONCOST Structural Studio"
                   Width="1280" Height="800"
                   TitleBarBackground="{DynamicResource CONCOST.Brush.Surface}"
                   WindowStartupLocation="CenterScreen">
    <Grid>
        <!-- nội dung ứng dụng -->
    </Grid>
</win:CONCOSTWindow>`,
                csharp: `// MainWindow.xaml.cs
using CONCOST.UI.Controls.Windows;

public partial class MainWindow : CONCOSTWindow
{
    public MainWindow()
    {
        InitializeComponent();
    }
}`,
                callout: { type: "warn", title: "AllowsTransparency", body: "Constructor đặt <code>AllowsTransparency = false</code> để giữ hiệu năng và hỗ trợ DWM shadow. Nếu cần cửa sổ trong suốt hoàn toàn, đặt lại sau <code>InitializeComponent()</code> và chấp nhận mất hardware acceleration." }
            }
        ],
        related: ["dialog", "drawer", "card"]
    },

    /* ============================ FEEDBACK ============================ */
    skeleton: {

        title: "Skeleton — placeholder shimmering",
        badge: "Phản hồi & Trạng thái",
        desc: "Placeholder shimmering thay cho vòng xoay khi tải dữ liệu — giữ nguyên bố cục để tránh layout shift.",
        keywords: ["shimmer", "placeholder", "loading", "skeleton"],
        variants: [
            { key: "CONCOST.Skeleton.Base", desc: "Khối chữ nhật, bo góc theo <code>CornerRadius</code>." },
            { key: "CONCOST.Skeleton.Text", desc: "Dòng text mảnh, đầu bo tròn." },
            { key: "CONCOST.Skeleton.Circle", desc: "Hình tròn cho avatar." },
            { key: "CONCOST.Skeleton.Card", desc: "Khối lớn mô phỏng card KPI." }
        ],
        sections: [
            {
                title: "Skeleton cho danh sách dầm",
                desc: "Bọc nội dung thật và skeleton trong cùng một Grid, đổi <code>IsLoading</code> để chuyển trạng thái. <code>IsWaveEnabled</code> bật hiệu ứng sóng sáng chạy qua.",
                xaml: `<Grid>
    <!-- Trạng thái thật -->
    <ItemsControl ItemsSource="{Binding Beams}" Visibility="{Binding IsLoading, Converter={StaticResource BooleanToVisibilityInverted}}" />

    <!-- Trạng thái chờ -->
    <StackPanel Visibility="{Binding IsLoading, Converter={StaticResource BooleanToVisibility}}">
        <ui:Skeleton Height="18" Width="220" Style="{DynamicResource CONCOST.Skeleton.Text}" Margin="0,0,0,10" />
        <ui:Skeleton Height="72" Style="{DynamicResource CONCOST.Skeleton.Card}" Margin="0,0,0,10" />
        <ui:Skeleton Height="72" Style="{DynamicResource CONCOST.Skeleton.Card}" Margin="0,0,0,10" />
        <ui:Skeleton Height="72" Style="{DynamicResource CONCOST.Skeleton.Card}" />
    </StackPanel>
</Grid>`,
                csharp: `// Skeleton cho avatar + 3 dòng text
var avatar = new Skeleton
{
    Width = 48,
    Height = 48,
    SkeletonShape = SkeletonShape.Circle,
    Duration = TimeSpan.FromSeconds(1.2)
};

var line = new Skeleton
{
    Height = 14,
    SkeletonShape = SkeletonShape.TextLine,
    IsWaveEnabled = true
};

// Tắt shimmer khi dữ liệu đã về
line.IsLoading = false;`
            }
        ],
        related: ["loadings", "loadingoverlay", "emptystate"]
    },

    ripple: {

        title: "Ripple — hiệu ứng gợn sóng Material",
        badge: "Hiệu ứng & Trang trí",
        desc: "Hiệu ứng gợn sóng Material khi click, dùng độc lập hoặc gắn vào bất kỳ phần tử nào qua attached <code>RippleElement</code>.",
        keywords: ["material", "click", "sóng", "hiệu ứng"],
        sections: [
            {
                title: "Ripple dạng attached — không đổi cấu trúc XAML",
                desc: "<code>RippleElement.IsEnabled</code> biến phần tử bất kỳ thành bề mặt có gợn sóng. <code>Mode=\"Target\"</code> phát sóng từ đúng toạ độ click, <code>Center</code> phát từ tâm.",
                xaml: `<Border attached:RippleElement.IsEnabled="True"
        attached:RippleElement.Mode="Target"
        attached:RippleElement.MaxOpacity="0.28"
        Background="{DynamicResource CONCOST.Brush.Surface}"
        CornerRadius="10" Padding="16">
    <TextBlock Text="Click anywhere on this card" />
</Border>`,
                csharp: `// Gắn ripple cho toàn bộ item trong list
foreach (var item in BeamList.Items)
{
    if (item is ListBoxItem row)
        RippleElement.SetIsEnabled(row, true);
}

// Ripple độc lập với màu tuỳ biến
var ripple = new Ripple
{
    RippleBrush = (Brush)FindResource("CONCOST.Brush.Primary"),
    MaxOpacity = 0.35,
    Duration = TimeSpan.FromMilliseconds(450),
    MaxConcurrentRipples = 4
};`
            }
        ],
        related: ["button", "card", "skeleton"]
    },

    loadings: {

        title: "Loading Indicators — bộ spinner & progress",
        badge: "Phản hồi & Trạng thái",
        desc: "Bộ loading indicator: 4 kiểu của <code>LoadingIndicator</code> và 5 control animation độc lập (<code>CycleLoading</code>, <code>RingLoading</code>, <code>RollLoading</code>, <code>WaitLoading</code>, <code>LoadingCircle</code>, <code>LoadingLine</code>).",
        keywords: ["spinner", "loading", "progress", "chờ", "vòng xoay"],
        sections: [
            {
                title: "4 kiểu LoadingIndicator",
                desc: "<code>IndicatorType</code> nhận <code>ThreeDotsWave</code>, <code>CircularDotsRing</code>, <code>LinePulse</code>, <code>PulseCircle</code>. <code>DotSize</code> đổi kích thước hạt.",
                xaml: `<StackPanel Orientation="Horizontal" VerticalAlignment="Center">
    <ui:LoadingIndicator IndicatorType="ThreeDotsWave"    DotSize="10" Margin="0,0,28,0" />
    <ui:LoadingIndicator IndicatorType="CircularDotsRing" DotSize="9"  Margin="0,0,28,0" />
    <ui:LoadingIndicator IndicatorType="LinePulse"        DotSize="12" Margin="0,0,28,0" />
    <ui:LoadingIndicator IndicatorType="PulseCircle"      DotSize="10" />
</StackPanel>`,
                csharp: `// Đổi kiểu theo ngữ cảnh tác vụ
SolverIndicator.IndicatorType = isHeavySolve
    ? LoadingIndicatorType.CircularDotsRing
    : LoadingIndicatorType.ThreeDotsWave;

SolverIndicator.IsLoading = true;`
            },
            {
                title: "Loading có tiến trình xác định",
                desc: "<code>CycleLoading</code> hiển thị vòng tròn phần trăm kèm <code>LoadTitle</code>; <code>Value</code> nhận 0–100.",
                xaml: `<ui:CycleLoading IsLoading="{Binding IsSolving}"
                 Value="{Binding SolveProgress}"
                 LoadTitle="Solving load combinations..."
                 Width="160" Height="160" />`,
                csharp: `// Cập nhật tiến trình từ solver
SolveProgress = completedSteps * 100.0 / totalSteps;
IsSolving = SolveProgress < 100;`
            }
        ],
        related: ["loadingoverlay", "skeleton", "progressbar"]
    },

    loadingoverlay: {

        title: "LoadingOverlay — lớp phủ chặn tương tác",
        badge: "Phản hồi & Trạng thái",
        desc: "Lớp phủ loading chặn tương tác, có tiêu đề, mô tả phụ và thanh tiến trình — dùng cho tác vụ dài như solve, import IFC, sync cloud.",
        keywords: ["overlay", "blocking", "loading", "chặn", "progress"],
        sections: [
            {
                title: "Overlay cho tác vụ solve",
                desc: "<code>IsIndeterminate=\"True\"</code> cho tác vụ không rõ thời lượng; đặt <code>False</code> và truyền <code>Progress</code> để hiện phần trăm.",
                xaml: `<Grid>
    <Grid x:Name="WorkspaceContent">
        <!-- bản vẽ / bảng dữ liệu -->
    </Grid>

    <ui:LoadingOverlay IsLoading="{Binding IsSolving}"
                       Message="Solving 12 load combinations"
                       SubMessage="Moment distribution · P-Delta iteration"
                       Progress="{Binding SolveProgress}"
                       IsIndeterminate="False" />
</Grid>`,
                csharp: `// Bật overlay trong using để luôn tắt khi có exception
IsSolving = true;
try
{
    await solver.RunAsync(progress => SolveProgress = progress);
}
finally
{
    IsSolving = false;   // LoadingOverlay tự ẩn
}`
            }
        ],
        related: ["loadings", "skeleton", "dialog"]
    },

    /* ============================ INPUT ============================ */
    passwordinput: {

        title: "PasswordInput — ô nhập mật khẩu",
        badge: "Nhập liệu & Form",
        desc: "Ô nhập mật khẩu có nút hiện/ẩn, icon khoá và placeholder — thay cho việc tự ghép <code>PasswordBox</code> + <code>ToggleButton</code>.",
        keywords: ["password", "mật khẩu", "login", "ẩn hiện"],
        sections: [
            {
                title: "Đăng nhập",
                desc: "<code>Password</code> là hai-way binding. Khi <code>ShowPassword=\"True\"</code>, nội dung hiện dạng text thường.",
                xaml: `<ui:PasswordInput Placeholder="Mật khẩu truy cập CSDL"
                  IconKind="Lock"
                  ShowPassword="{Binding IsPasswordVisible, Mode=TwoWay}"
                  Password="{Binding DatabasePassword, Mode=TwoWay}"
                  Width="320" />`,
                csharp: `// Kiểm tra rỗng trước khi kết nối
if (string.IsNullOrWhiteSpace(DatabasePassword))
{
    Growl.Toast("Vui lòng nhập mật khẩu", GrowlType.Warning);
    return;
}

// Ẩn lại sau khi đăng nhập thành công
IsPasswordVisible = false;
DatabasePassword = string.Empty;`
            }
        ],
        related: ["textbox", "otpinput", "pinbox"]
    },

    otpinput: {

        title: "OtpInput — mã xác thực nhiều ô",
        badge: "Nhập liệu & Form",
        desc: "Ô nhập mã OTP nhiều ô, tự nhảy ô và gộp giá trị — phù hợp xác thực 2 lớp, mã kích hoạt bản quyền.",
        keywords: ["otp", "mã xác thực", "2fa", "verification"],
        sections: [
            {
                title: "Mã xác thực 6 số",
                desc: "<code>Length</code> quy định số ô; <code>Value</code> là chuỗi đã gộp, bind two-way để submit.",
                xaml: `<ui:OtpInput Length="6"
              Value="{Binding OtpCode, Mode=TwoWay}"
              Width="300" />`,
                csharp: `// Submit khi đủ số
if (OtpCode?.Length == 6)
{
    await authService.VerifyAsync(OtpCode);
}`
            }
        ],
        related: ["pinbox", "passwordinput", "numericupdown"]
    },

    /* ============================ NAVIGATION / DATA ============================ */
    navigationview: {

        title: "NavigationView — điều hướng dạng danh sách",
        badge: "Điều hướng",
        desc: "Thanh điều hướng dạng danh sách item có icon, badge trạng thái và <code>SelectedItem</code> hai-way — bản gọn hơn SideMenu cho panel hẹp.",
        keywords: ["navigation", "menu", "điều hướng", "panel"],
        sections: [
            {
                title: "Navigation dạng tab dọc",
                desc: "<code>NavigationItem</code> là <code>HeaderedContentControl</code>: <code>Header</code> là nhãn, <code>Icon</code> là vector, <code>TagName</code> dùng để định tuyến.",
                xaml: `<nav:NavigationView SelectedItem="{Binding CurrentSection, Mode=TwoWay}"
                       Width="240">
    <nav:NavigationItem Header="Tổng quan"  Icon="Layers"   TagName="overview" IsSelected="True" />
    <nav:NavigationItem Header="Kết cấu"    Icon="SectionI" TagName="structure" />
    <nav:NavigationItem Header="Cốt thép"   Icon="Rebar"    TagName="rebar" />
    <nav:NavigationItem Header="Báo cáo"    Icon="FileText" TagName="report" />
</nav:NavigationView>`,
                csharp: `// Điều hướng theo TagName
CurrentSection.PropertyChanged += (s, e) =>
{
    if (e.PropertyName != nameof(NavigationItem.TagName)) return;
    ContentHost.Content = CurrentSection.TagName switch
    {
        "overview" => new OverviewView(),
        "structure" => new StructureView(),
        "rebar" => new RebarView(),
        _ => new ReportView()
    };
};`
            }
        ],
        related: ["sidemenu", "drawermenu", "tabcontrol"]
    },

    datapager: {

        title: "DataPager — phân trang dữ liệu",
        badge: "Dữ liệu & Dashboard",
        desc: "Phân trang cho DataGrid/ListView: <code>PageIndex</code>, <code>PageSize</code>, <code>TotalItems</code> đều bind two-way và tự ép giá trị hợp lệ.",
        keywords: ["paging", "phân trang", "page", "grid"],
        sections: [
            {
                title: "Phân trang bảng bóc tách khối lượng",
                desc: "Đặt <code>TotalItems</code> bằng tổng bản ghi; pager tự tính số trang và ép <code>PageIndex</code> trong khoảng hợp lệ.",
                xaml: `<DockPanel>
    <ui:DataPager DockPanel.Dock="Bottom"
                  PageIndex="{Binding PageIndex, Mode=TwoWay}"
                  PageSize="{Binding PageSize, Mode=TwoWay}"
                  TotalItems="{Binding TotalRows}"
                  Margin="0,12,0,0" />

    <DataGrid ItemsSource="{Binding PagedRows}" Style="{DynamicResource CONCOST.DataGrid.Base}" />
</DockPanel>`,
                csharp: `// Cắt trang bằng LINQ khi PageIndex/PageSize đổi
public ObservableCollection<BoqRow> PagedRows { get; } = new();

void Refresh()
{
    var skip = (PageIndex - 1) * PageSize;
    PagedRows.Clear();
    foreach (var row in AllRows.Skip(skip).Take(PageSize))
        PagedRows.Add(row);
}`
            }
        ],
        related: ["datagrid", "pagination", "spreadsheet"]
    },

    calendarwithclock: {

        title: "CalendarWithClock — lịch & đồng hồ",
        badge: "Thời gian",
        desc: "Lịch chọn ngày kết hợp đồng hồ chọn giờ/phút/giây — dùng cho mốc nghiệm thu, thời điểm quan trắc, lịch thi công.",
        keywords: ["calendar", "clock", "datetime", "ngày giờ", "lịch"],
        sections: [
            {
                title: "Chọn mốc thời gian nghiệm thu",
                desc: "<code>SelectedDateTime</code> bind two-way; sự kiện <code>Confirmed</code> phát khi người dùng bấm xác nhận.",
                xaml: `<ui:CalendarWithClock SelectedDateTime="{Binding InspectionDateTime, Mode=TwoWay}"
                       ShowSeconds="False"
                       Confirmed="OnInspectionTimeConfirmed" />`,
                csharp: `// Chặn mốc thời gian trong tương lai
private void OnInspectionTimeConfirmed(object sender, RoutedEventArgs e)
{
    if (InspectionDateTime > DateTime.Now)
    {
        Growl.Toast("Mốc nghiệm thu không được ở tương lai", GrowlType.Warning);
        InspectionDateTime = DateTime.Now;
        return;
    }

    LoadMeasurements(InspectionDateTime);
}`
            }
        ],
        related: ["clock", "datepicker", "timeline"]
    },

    /* ============================ C# OVERRIDES ============================ */
    growl: {

        csharp: `// Toast không chặn — tự xếp chồng ở góc màn hình
Growl.Toast("Đã lưu mô hình", GrowlType.Success);
Growl.Toast("3 nút ứng suất cao", GrowlType.Warning);

// Có nút đóng và thời gian hiển thị riêng
var growl = new Growl
{
    Message = "Đồng bộ cloud hoàn tất",
    GrowlType = GrowlType.Info,
    TimeString = "now",
    ShowCloseButton = true
};
GrowlHost.Children.Add(growl);`
    },

    spreadsheet: {

        csharp: `// Tạo workbook 2 sheet và ghi công thức
var sheet = new SpreadsheetSheet { Name = "BOQ" };
sheet.Cells["A1"].Value = "Cấu kiện";
sheet.Cells["B1"].Value = "Khối lượng";
sheet.Cells["B2"].Value = "=SUM(B3:B12)";

var summary = new SpreadsheetSheet { Name = "Summary" };
summary.Cells["A1"].Value = "=BOQ!B2";

Workbook.Sheets.Add(sheet);
Workbook.Sheets.Add(summary);
Workbook.ActiveSheet = sheet;

// Đọc ô đang chọn khi người dùng click
var cell = Workbook.ActiveSheet.Cells[Workbook.ActiveRow, Workbook.ActiveColumn];
Debug.WriteLine($"{cell.Address} = {cell.Value}");`
    },

    dialog: {

        csharp: `// Dialog khai báo trong XAML, mở bằng IsOpen
ConfirmDialog.IsOpen = true;

// Đóng theo token khi kết quả trả về bất đồng bộ
if (await ConfirmDialog.WaitAsync() == DialogResult.OK)
{
    DeleteSelection();
}

// Dialog không cho click mask để đóng (bắt buộc chọn)
ConfirmDialog.MaskCanClose = false;
ConfirmDialog.MessageType = DialogMessageType.Warning;`
    },

    timeline: {

        csharp: `// Dữ liệu mốc tiến độ thi công
var items = new[]
{
    new TimelineItem { Time = "T1/2026", Description = "Khảo sát hiện trạng", Type = TimelineItemType.Success },
    new TimelineItem { Time = "T2/2026", Description = "Thiết kế cơ sở",     Type = TimelineItemType.Primary },
    new TimelineItem { Time = "T4/2026", Description = "Thiết kế kỹ thuật",  Type = TimelineItemType.Warning },
    new TimelineItem { Time = "T7/2026", Description = "Thi công móng",      Type = TimelineItemType.Default, IsLast = true }
};

ProgressTimeline.ItemsSource = items;`
    }
};
