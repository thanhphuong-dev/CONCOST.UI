// CONCOST.UI Complete Comprehensive Dataset (Full 45+ UI Controls)
const DOCS_DATA = {
    // -------------------------------------------------------------
    // INTRO & GETTING STARTED
    // -------------------------------------------------------------
    intro: {
        title: "Giới Thiệu & Cài Đặt CONCOST.UI",
        badge: "Bắt Đầu Nhanh",
        desc: "CONCOST.UI là bộ thư viện giao diện WPF chuyên nghiệp thế hệ mới, tối ưu hóa cho các kỹ sư xây dựng, phần mềm BIM, CAD, kết cấu và ứng dụng Desktop cao cấp. Thư viện tích hợp hệ thống Design Tokens tự động thích ứng Dark/Light theme cùng kho hơn 45+ UI controls hiện đại chuẩn WPFDevelopers & HandyControl.",
        sections: [
            {
                title: "1. Cài đặt và Tích hợp qua XAML (Ứng dụng WPF Exe độc lập)",
                desc: "Để sử dụng thư viện trong ứng dụng WPF Exe, đăng ký ResourceDictionary của CONCOST.UI vào App.xaml:",
                xaml: `<Application x:Class="YourApp.App"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <Application.Resources>
        <ResourceDictionary>
            <ResourceDictionary.MergedDictionaries>
                <!-- 1. Theme Nền Tảng (Light hoặc Dark) -->
                <ResourceDictionary Source="pack://application:,,,/CONCOST.UI;component/Themes/Light.xaml" />
                <!-- 2. Thư Viện Controls và Styles CONCOST -->
                <ResourceDictionary Source="pack://application:,,,/CONCOST.UI;component/Themes/Generic.xaml" />
            </ResourceDictionary.MergedDictionaries>
        </ResourceDictionary>
    </Application.Resources>
</Application>`,
                csharp: `// Cấu hình khởi tạo và đổi màu chủ đạo (Accent Color) trong C# Code-Behind:
using CONCOST.UI.Themes;
using CONCOST.UI.Core.Helpers;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        
        // Đặt màu chủ đạo mặc định (Vd: Màu Cam Kỹ Thuật #FF7900 hoặc Xanh Dương #0284C7)
        ThemeManager.Current.AccentColor = ColorHelper.FromHex("#FF7900");
    }
}`
            },
            {
                title: "2. Khai báo Namespaces trong View XAML",
                desc: "Khai báo các XML namespace để truy cập Controls, Icons và Attached Properties:",
                xaml: `<Window x:Class="YourApp.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:ui="clr-namespace:CONCOST.UI.Controls;assembly=CONCOST.UI"
        xmlns:icons="clr-namespace:CONCOST.UI.Icons;assembly=CONCOST.UI"
        xmlns:dash="clr-namespace:CONCOST.UI.Controls.Dashboard;assembly=CONCOST.UI"
        xmlns:attached="clr-namespace:CONCOST.UI.AttachedProperties;assembly=CONCOST.UI"
        Background="{DynamicResource CONCOST.Brush.Background}"
        Foreground="{DynamicResource CONCOST.Brush.TextPrimary}">
    
    <Grid>
        <!-- Bắt đầu sử dụng các component của CONCOST -->
    </Grid>
</Window>`
            }
        ]
    },

    // -------------------------------------------------------------
    // CLASS LIBRARY & CAD / BIM PLUGINS (DLL MODE)
    // -------------------------------------------------------------
    "class-library": {
        title: "Sử Dụng Toàn Diện Trong Class Library (DLL) & Plugins (Revit, AutoCAD, Tekla, Navisworks)",
        badge: "Plugin Architecture & DLL Mode",
        desc: "Hướng dẫn chuyên sâu kiến trúc phát triển giao diện WPF khi đóng gói dưới dạng Class Library (.DLL) hoặc Plugins cho các phần mềm CAD/BIM chuyên nghiệp (Autodesk Revit, AutoCAD, Tekla Structures, Navisworks, Civil 3D, Rhino/Grasshopper). Giải quyết triệt để vấn đề không có App.xaml, quản lý tài nguyên cục bộ không gây ô nhiễm phần mềm chủ (Host Pollution), nạp Styles an toàn và chuyển đổi Theme Dark/Light mượt mà tại thời gian thực (Runtime).",
        sections: [
            {
                title: "1. Bản Chất Kiến Trúc: Sự Khác Biệt Giữa Standalone Exe & Class Library DLL",
                desc: "Khi chạy ứng dụng WPF độc lập (.exe), `App.xaml` tự động khởi tạo đối tượng toàn cục `Application.Current`. Nhưng khi chạy dưới dạng **Plugin DLL** được nhúng vào phần mềm chủ (`revit.exe`, `acad.exe`, `tekla.exe`):",
                table: [
                    { token: "Vấn Đề: Application.Current", desc: "Thuộc quyền kiểm soát của phần mềm mẹ. Can thiệp hoặc gán đè `Application.Current.Resources` có thể làm hỏng giao diện của Revit/AutoCAD hoặc gây văng phần mềm (Crash)." },
                    { token: "Vấn Đề: Quản Lý Assembly", desc: "Các file ResourceDictionary (`Generic.xaml`, `Light.xaml`, `Dark.xaml`) cần được nạp tường minh qua Pack URI có tên Assembly cụ thể." },
                    { token: "Giải Pháp: Cục Bộ Hóa Tài Nguyên (Local Resource Scoping)", desc: "CONCOST.UI cung cấp phương thức `ThemeManager.Current.ApplyTheme(rootElement, theme)` giúp nạp toàn bộ Styles, Brushes và Tokens vào thẳng thẻ gốc (`Window` hoặc `UserControl`) của plugin, cách ly 100% với phần mềm chủ." }
                ]
            },
            {
                title: "2. Phương Pháp 1: Nạp Tự Động Trong Constructor C# (Khuyên Dùng Nhất)",
                desc: "Chỉ với 2 dòng lệnh trong Constructor của `Window` hoặc `UserControl`, toàn bộ hệ thống giao diện CONCOST.UI sẵn sàng hoạt động:",
                csharp: `using System.Windows;
using CONCOST.UI.Themes;
using CONCOST.UI.Core.Helpers;

namespace YourBimPlugin.Views;

public partial class StructuralModelInspectorWindow : Window
{
    public StructuralModelInspectorWindow()
    {
        InitializeComponent();

        // 1. Tự động nạp Generic.xaml, toàn bộ 45+ Controls Style và Theme Light/Dark vào phạm vi Window này:
        ThemeManager.Current.ApplyTheme(this, AppTheme.Light);
        
        // 2. Tùy chỉnh màu chủ đạo nhận diện thương hiệu cho Plugin (vd: Xanh Dương Kỹ Thuật #0284C7):
        ThemeManager.Current.ApplyAccentColor(this, ColorHelper.FromHex("#0284C7"));
    }
}`
            },
            {
                title: "3. Phương Pháp 2: Khai Báo Tĩnh Trực Tiếp Trong File XAML",
                desc: "Nếu bạn muốn thiết kế giao diện có hiển thị trực quan (Live Preview) trong trình thiết kế Visual Studio Designer / Blend, bạn có thể nhúng ResourceDictionary trực tiếp vào `<Window.Resources>` hoặc `<UserControl.Resources>`:",
                xaml: `<Window x:Class="YourBimPlugin.Views.RevitQuantityTakeoffWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:ui="clr-namespace:CONCOST.UI.Controls;assembly=CONCOST.UI"
        xmlns:icons="clr-namespace:CONCOST.UI.Icons;assembly=CONCOST.UI"
        xmlns:dash="clr-namespace:CONCOST.UI.Controls.Dashboard;assembly=CONCOST.UI"
        xmlns:attached="clr-namespace:CONCOST.UI.AttachedProperties;assembly=CONCOST.UI"
        Title="CONCOST - Quản Lý Bóc Tách Khối Lượng BIM" Height="680" Width="1080"
        Background="{DynamicResource CONCOST.Brush.Background}"
        Foreground="{DynamicResource CONCOST.Brush.TextPrimary}">

    <!-- Khai báo tài nguyên cục bộ chỉ áp dụng bên trong Window này -->
    <Window.Resources>
        <ResourceDictionary>
            <ResourceDictionary.MergedDictionaries>
                <!-- Nạp Design Tokens và Theme mặc định -->
                <ResourceDictionary Source="pack://application:,,,/CONCOST.UI;component/Themes/ThemeResources.xaml" />
                <!-- Nạp toàn bộ định dạng và Controls của CONCOST -->
                <ResourceDictionary Source="pack://application:,,,/CONCOST.UI;component/Themes/Generic.xaml" />
            </ResourceDictionary.MergedDictionaries>
        </ResourceDictionary>
    </Window.Resources>

    <Grid Margin="20">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
        </Grid.RowDefinitions>

        <!-- Thanh tìm kiếm & Nút bấm hiện đại -->
        <StackPanel Orientation="Horizontal" Grid.Row="0" Margin="0,0,0,16">
            <ui:SearchBox Width="320" attached:InputBase.Placeholder="Tìm kiếm dầm, cột, móng, cọc..." />
            <Button Style="{DynamicResource CONCOST.Button.Primary}"
                    Content="Xuất File Excel Quyết Toán"
                    attached:IconElement.Kind="File"
                    attached:BorderElement.CornerRadius="8"
                    Margin="12,0,0,0" Padding="16,8" />
        </StackPanel>

        <!-- Vòng xoay thẻ 3D dự án -->
        <dash:CardCarousel Grid.Row="1" Height="270" ItemWidth="280" ItemHeight="175" Spacing="190" />
    </Grid>
</Window>`
            },
            {
                title: "4. Chuyển Đổi Theme Dark/Light & Màu Sắc Lúc Đang Chạy (Runtime Switching)",
                desc: "Khi người dùng thao tác gạt công tắc đổi chế độ tối (Dark Mode) hoặc click chọn bảng màu Palette ngay trên giao diện Plugin, hệ thống sẽ hoán đổi ResourceDictionary cục bộ trong cây Visual Tree mà không cần khởi động lại Plugin:",
                csharp: `// Gắn sự kiện chuyển Theme ngay trong View Code-Behind:
private void OnThemeToggleClick(object sender, RoutedEventArgs e)
{
    bool isDarkMode = ToggleThemeSwitch.IsChecked ?? false;
    
    // Áp dụng Theme mới cho chính Window / UserControl này:
    ThemeManager.Current.ApplyTheme(this, isDarkMode ? AppTheme.Dark : AppTheme.Light);
}

// Đổi Màu Accent (Màu Cam, Xanh Lá, Xanh Dương, Tím, Đỏ,...) tức thời:
private void OnColorPresetSelected(string hexColor)
{
    ThemeManager.Current.ApplyAccentColor(this, ColorHelper.FromHex(hexColor));
}

// Chuyển Theme với Hoạt Ảnh Quét Sóng Dịu Mắt (Soft Radial Reveal) chuyên nghiệp:
private void OnSmoothThemeToggleClick(object sender, RoutedEventArgs e)
{
    Point clickOrigin = ToggleThemeSwitch.TransformToAncestor(this).Transform(new Point(10, 10));
    
    ThemeAnimationHelper.AnimateThemeChange(this, clickOrigin, () =>
    {
        bool useDarkTheme = ToggleThemeSwitch.IsChecked ?? false;
        ThemeManager.Current.ApplyTheme(this, useDarkTheme ? AppTheme.Dark : AppTheme.Light);
    });
}`
            },
            {
                title: "5. Sử Dụng Trong Dockable Panel (Revit IDockablePaneProvider / AutoCAD PaletteSet)",
                desc: "Nếu bạn tích hợp CONCOST.UI vào dạng Bảng Điều Khiển Gắn Liền (Dockable Panel / Dockable Pane), hãy nạp Theme trực tiếp vào thẻ `<UserControl>` gốc:",
                csharp: `using System.Windows.Controls;
using Autodesk.Revit.UI;
using CONCOST.UI.Themes;

namespace YourBimPlugin.Dockable;

public partial class BimInspectorDockablePane : UserControl, IDockablePaneProvider
{
    public BimInspectorDockablePane()
    {
        InitializeComponent();

        // Nạp Theme và Resource cho Dockable Panel (UserControl):
        ThemeManager.Current.ApplyTheme(this, AppTheme.Light);
    }

    public void SetupDockablePane(DockablePaneSetupData data)
    {
        data.FrameworkControl = this;
        data.InitialState = new DockablePaneState
        {
            DockPosition = DockPosition.Right,
            TabDockPosition = TabDockPosition.Bottom
        };
    }
}`
            },
            {
                title: "6. Mẫu Tích Hợp Hoàn Chỉnh: Revit API IExternalCommand & Win32 Owner Handle",
                desc: "Đoạn mã tiêu chuẩn công nghiệp khi hiển thị cửa sổ WPF Modal / Modeless từ lệnh Revit để đảm bảo quản lý phím tắt, cửa sổ thu nhỏ/phóng to đồng bộ với Revit:",
                csharp: `using System;
using System.Windows.Interop;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using YourBimPlugin.Views;

namespace YourBimPlugin.Commands;

[Transaction(TransactionMode.Manual)]
[Regeneration(RegenerationOption.Manual)]
public class OpenCostManagerCommand : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        try
        {
            // 1. Khởi tạo cửa sổ giao diện CONCOST:
            var window = new StructuralModelInspectorWindow();

            // 2. Gán Owner Handle của Revit MainWindow để cửa sổ luôn nằm trên và không bị chìm xuống dưới:
            IntPtr revitHandle = System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle;
            var helper = new WindowInteropHelper(window)
            {
                Owner = revitHandle
            };

            // 3. Hiển thị cửa sổ (ShowDialog cho Modal hoặc Show cho Modeless):
            window.ShowDialog();

            return Result.Succeeded;
        }
        catch (Exception ex)
        {
            message = ex.Message;
            return Result.Failed;
        }
    }
}`
            }
        ]
    },

    // -------------------------------------------------------------
    // THEMING SYSTEM
    // -------------------------------------------------------------
    theming: {
        title: "Hệ Thống Theme & Token Màu Sắc",
        badge: "Design Tokens",
        desc: "Toàn bộ bảng màu và phong cách giao diện của CONCOST.UI được quản lý thông qua DynamicResource, cho phép ứng dụng chuyển đổi mượt mà giữa Dark và Light mode theo thời gian thực mà không cần khởi động lại.",
        sections: [
            {
                title: "1. Danh mục Token Màu & Brush Chuẩn",
                desc: "Các Resource Brush quan trọng được sử dụng đồng bộ xuyên suốt hệ thống:",
                table: [
                    { token: "CONCOST.Brush.Primary", desc: "Màu chủ đạo (Accent Brand Color), tự động sinh biến thể Hover, Pressed, Light, Dark" },
                    { token: "CONCOST.Brush.Background", desc: "Màu nền trang chính của toàn bộ cửa sổ ứng dụng" },
                    { token: "CONCOST.Brush.Surface", desc: "Màu nền cho các Container, Card, Dialog, Sidebar" },
                    { token: "CONCOST.Brush.SurfaceSecondary", desc: "Màu nền phụ cho Toolbar, Input nền mờ, Badge" },
                    { token: "CONCOST.Brush.Border", desc: "Màu viền phân cách tiêu chuẩn" },
                    { token: "CONCOST.Brush.TextPrimary", desc: "Màu chữ văn bản chính (Độ tương phản cao)" },
                    { token: "CONCOST.Brush.TextSecondary", desc: "Màu chữ phụ, nhãn thông số, subtitle" }
                ],
                csharp: `// Chuyển đổi Dark/Light mode và Màu Accent trong C#:
ThemeManager.Current.Theme = AppTheme.Dark; // hoặc AppTheme.Light / AppTheme.System
ThemeManager.Current.AccentColor = ColorHelper.FromHex("#0284C7"); // Ocean Blue

// Chuyển Theme với Hoạt ảnh Quét Sóng Dịu Mắt (Soft Reveal Animation):
ThemeAnimationHelper.AnimateThemeChange(this, clickPoint, () => {
    ThemeManager.Current.Theme = AppTheme.Dark;
});`
            }
        ]
    },

    // -------------------------------------------------------------
    // ATTACHED PROPERTIES (ĐÍNH KÈM TOÀN NĂNG)
    // -------------------------------------------------------------
    "attached-props": {
        title: "Attached Properties Toàn Diện (Mở Rộng Sức Mạnh Mọi Control)",
        badge: "Core Extensions & Helpers",
        desc: "Hệ thống thuộc tính đính kèm phong phú của CONCOST.UI cho phép bạn 'thổi hồn' công nghệ hiện đại vào bất kỳ control WPF nguyên bản nào (Button, TextBox, PasswordBox, ScrollViewer, ProgressBar, DataGrid, Border...) chỉ với 1 dòng khai báo XAML mà không cần viết lại ControlTemplate phức tạp.",
        sections: [
            {
                title: "1. Nhóm BorderElement (Bo Góc, Nút Tròn & Màu Viền Hover)",
                desc: "Điều khiển bo tròn và hiệu ứng tương tác chuột cho Button, Border, Panel:",
                table: [
                    { token: "attached:BorderElement.CornerRadius", desc: "Độ bo góc tùy ý (vd: CornerRadius=\"8\" hoặc CornerRadius=\"10,10,0,0\")" },
                    { token: "attached:BorderElement.Circular", desc: "Tự động biến control thành hình tròn hoàn hảo (Circular Pill: True/False)" },
                    { token: "attached:BorderElement.HoverBackground", desc: "Màu nền khi rê chuột qua (Brush)" },
                    { token: "attached:BorderElement.HoverBorderBrush", desc: "Màu viền khi rê chuột qua (Brush)" }
                ],
                xaml: `<!-- Nút tròn 40x40 kèm bo tròn tự động -->
<Button Style="{DynamicResource CONCOST.Button.Primary}"
        Width="40" Height="40"
        attached:BorderElement.Circular="True"
        attached:IconElement.Kind="Check" />`
            },
            {
                title: "2. Nhóm InfoElement & InputBase (Metadata Ô Nhập Liệu, Tiêu Đề, Clear Button)",
                desc: "Nâng cấp TextBox, ComboBox, PasswordBox chuyên nghiệp:",
                table: [
                    { token: "attached:InfoElement.Placeholder", desc: "Chữ gợi ý chìm khi ô nhập trống (vd: Placeholder=\"Nhập tên cấu kiện...\")" },
                    { token: "attached:InfoElement.Title", desc: "Nhãn tiêu đề trường dữ liệu gắn liền với ô nhập" },
                    { token: "attached:InfoElement.TitlePlacement", desc: "Vị trí đặt nhãn tiêu đề: Top hoặc Left" },
                    { token: "attached:InfoElement.Required", desc: "Đánh dấu trường bắt buộc (hiển thị dấu sao đỏ *)" },
                    { token: "attached:InfoElement.Prefix", desc: "Icon hoặc ký tự tiền tố ở đầu ô nhập (vd: $, VND, Icon)" },
                    { token: "attached:InfoElement.Suffix", desc: "Đơn vị hoặc ký tự hậu tố ở cuối ô nhập (vd: mm, m², MPa)" },
                    { token: "attached:InfoElement.ShowClearButton", desc: "Hiển thị nút [X] xóa trắng nhanh nội dung: True/False" }
                ],
                xaml: `<TextBox Style="{DynamicResource CONCOST.TextBox.Base}"
         attached:InfoElement.Title="Bề Dày Sàn Bê Tông"
         attached:InfoElement.Required="True"
         attached:InfoElement.Placeholder="Nhập số mm..."
         attached:InfoElement.Suffix="mm"
         attached:InfoElement.ShowClearButton="True"
         Width="280" />`
            },
            {
                title: "3. Nhóm IconElement (Vector Icons Tiền Tố, Hậu Tố & Kích Thước)",
                desc: "Gán bất kỳ vector icon nào trong kho 120+ icon vào Button, Menu, TabItem, Header:",
                table: [
                    { token: "attached:IconElement.Kind", desc: "Tên loại icon vector (vd: Kind=\"Ruler\", Kind=\"Layers\", Kind=\"Search\", Kind=\"Trash\")" },
                    { token: "attached:IconElement.IconSize", desc: "Kích thước icon (mặc định 16px)" },
                    { token: "attached:IconElement.Placement", desc: "Vị trí đặt icon: Dock.Left hoặc Dock.Right so với Text Content" }
                ],
                xaml: `<Button Style="{DynamicResource CONCOST.Button.Primary}"
        Content="Xuất Báo Cáo Quyết Toán"
        attached:IconElement.Kind="File"
        attached:IconElement.IconSize="18"
        attached:IconElement.Placement="Left"
        attached:BorderElement.CornerRadius="8"
        Padding="16,8" />`
            },
            {
                title: "4. Nhóm ButtonElement (Loading Spinner & Badge Thông Báo)",
                desc: "Tính năng cao cấp cho Button:",
                table: [
                    { token: "attached:ButtonElement.IsLoading", desc: "Bật vòng xoay Spinner đang xử lý, tự động ẩn nội dung chữ và chặn click trùng lặp" },
                    { token: "attached:ButtonElement.LoadingText", desc: "Chữ hiển thị thay thế khi đang loading (mặc định \"Loading...\")" },
                    { token: "attached:ButtonElement.Badge", desc: "Gắn huy hiệu số lượng / thông báo lên góc nút (vd: Badge=\"5\" hoặc Badge=\"NEW\")" }
                ],
                xaml: `<!-- Nút ở trạng thái đang tính toán kết cấu -->
<Button Style="{DynamicResource CONCOST.Button.Primary}"
        Content="Tính Toán Ma Trận Độ Cứng"
        attached:ButtonElement.IsLoading="True"
        attached:ButtonElement.LoadingText="Đang Giải Hệ Phương Trình..."
        attached:BorderElement.CornerRadius="8"
        Padding="18,8" />`
            },
            {
                title: "5. Nhóm VisualElement (Đổ Bóng Nổi & Phóng To Hover Scale)",
                desc: "Tạo hiệu ứng vi tương tác (Micro-Interactions) nổi bật cho Card, Button, Image:",
                table: [
                    { token: "attached:VisualElement.Elevation", desc: "Mức độ đổ bóng 3D: 0, 1, 2, 3, 4" },
                    { token: "attached:VisualElement.HoverScale", desc: "Tỉ lệ phóng to đàn hồi êm ái khi rê chuột (vd: HoverScale=\"1.05\")" }
                ],
                xaml: `<Border Background="{DynamicResource CONCOST.Brush.Surface}"
        attached:BorderElement.CornerRadius="12"
        attached:VisualElement.HoverScale="1.04"
        Padding="20">
    <TextBlock Text="Thẻ Dự Án Tự Phóng To Khi Rê Chuột" FontWeight="Bold"/>
</Border>`
            },
            {
                title: "6. Nhóm ScrollViewerHelper (Cuộn Quán Tính Inertia & Tự Ẩn Thanh Cuộn)",
                desc: "Biến ScrollViewer thông thường thành bộ cuộn mượt mà như trình duyệt web:",
                table: [
                    { token: "attached:ScrollViewerHelper.IsInertiaEnabled", desc: "Bật thuật toán cuộn quán tính mượt CubicEase: True/False" }
                ],
                xaml: `<ScrollViewer attached:ScrollViewerHelper.IsInertiaEnabled="True" VerticalScrollBarVisibility="Auto">
    <!-- Nội dung bảng tính dài tự động cuộn cực kỳ êm ái -->
</ScrollViewer>`
            },
            {
                title: "7. Nhóm PasswordBoxHelper (Binding MVVM 2 Chiều Cho Mật Khẩu)",
                desc: "Khắc phục nhược điểm không hỗ trợ TwoWay Binding của WPF PasswordBox nguyên bản:",
                table: [
                    { token: "attached:PasswordBoxHelper.Attach", desc: "Kích hoạt cơ chế lắng nghe thay đổi mật khẩu: True" },
                    { token: "attached:PasswordBoxHelper.Password", desc: "Liên kết TwoWay Binding trực tiếp với ViewModel" },
                    { token: "attached:PasswordBoxHelper.PasswordLength", desc: "Theo dõi độ dài mật khẩu để hiển thị thanh đo độ mạnh" }
                ],
                xaml: `<PasswordBox Style="{DynamicResource CONCOST.PasswordBox.Base}"
             attached:PasswordBoxHelper.Attach="True"
             attached:PasswordBoxHelper.Password="{Binding UserPassword, Mode=TwoWay}"
             attached:InputBase.Placeholder="Nhập mật khẩu truy cập CSDL..." />`
            },
            {
                title: "8. Nhóm ProgressBarElement & DataGridElement",
                desc: "Tùy biến nhãn tiến trình phần trăm và khoảng cách dòng bảng dữ liệu:",
                table: [
                    { token: "attached:ProgressBarElement.ShowText", desc: "Hiển thị nhãn số phần trăm % ngay trên thanh tiến trình" },
                    { token: "attached:ProgressBarElement.TextPattern", desc: "Định dạng chuỗi phần trăm (vd: \"{0:0}%\" hoặc \"Đã làm {0:0}%\")" },
                    { token: "attached:DataGridElement.RowSpacing", desc: "Khoảng cách giữa các dòng trong bảng dữ liệu (Double)" },
                    { token: "attached:DataGridElement.ShowRowDivider", desc: "Hiển thị đường phân cách mảnh giữa các dòng" }
                ],
                xaml: `<ProgressBar Value="78" Maximum="100" Height="14"
             Style="{DynamicResource CONCOST.ProgressBar.Base}"
             attached:ProgressBarElement.ShowText="True"
             attached:ProgressBarElement.TextPattern="Tiến độ: {0:0}%" />`
            },
            {
                title: "9. Nhóm DragDropHelper (Kéo Thả Sắp Xếp & Chuyển Danh Sách Chuẩn gong-wpf-dragdrop)",
                desc: "Cho phép người dùng kéo thả sắp xếp lại thứ tự (Reorder) hoặc chuyển item giữa 2 ListBox / DataGrid / TreeView hoàn toàn bằng XAML thuần túy mà không cần viết code-behind phức tạp:",
                table: [
                    { token: "attached:DragDropHelper.IsDragSource", desc: "Kích hoạt control là nguồn phát sự kiện kéo item (True/False)" },
                    { token: "attached:DragDropHelper.IsDropTarget", desc: "Kích hoạt control là đích nhận item thả vào (True/False)" },
                    { token: "attached:DragDropHelper.DragDropGroup", desc: "Tên nhóm tương thích để chỉ cho phép kéo thả giữa các danh sách cùng nhóm (String)" }
                ],
                xaml: `<!-- Kéo thả sắp xếp danh sách tầng hoặc chuyển giao việc cho kỹ sư -->
<ListBox ItemsSource="{Binding PendingTasksList}"
         attached:DragDropHelper.IsDragSource="True"
         attached:DragDropHelper.IsDropTarget="True"
         attached:DragDropHelper.DragDropGroup="TaskKanban"
         MinHeight="200" />`
            }
        ]
    },

    // =============================================================
    // CUSTOM CONTROLS (CONCOST Engineering Suite)
    // =============================================================
    cardcarousel: {
        title: "CardCarousel (Vòng Xoay Thẻ 3D)",
        badge: "Dashboard Suite",
        desc: "Bộ điều khiển vòng xoay thẻ 3D Coverflow mượt mà, hỗ trợ tạo phối cảnh chiều sâu không gian (Depth Perspective), gán Z-Index phân tầng, nút điều hướng nổi tròn chuẩn glassmorphism và tự động chuyển slide.",
        props: [
            { name: "SelectedIndex", type: "int", default: "0", desc: "Chỉ số thẻ bài đang được chọn ở vị trí trung tâm" },
            { name: "ItemWidth", type: "double", default: "280", desc: "Chiều rộng của từng thẻ bài" },
            { name: "ItemHeight", type: "double", default: "175", desc: "Chiều cao của từng thẻ bài" },
            { name: "Spacing", type: "double", default: "190", desc: "Khoảng cách trục ngang giữa các thẻ bài liền kề" },
            { name: "MinScale", type: "double", default: "0.75", desc: "Tỉ lệ thu nhỏ tối thiểu của các thẻ nằm xa tâm" },
            { name: "AutoRun", type: "bool", default: "false", desc: "Tự động xoay chuyển slide tuần tự (Tự tạm dừng khi rê chuột)" },
            { name: "Interval", type: "double", default: "4000", desc: "Thời gian chu kỳ xoay tự động (tính bằng milliseconds)" }
        ],
        sections: [
            {
                title: "Ví Dụ Sử Dụng XAML",
                desc: "Khai báo `CardCarousel` chứa danh sách các `CardCarouselItem` dự án kỹ thuật:",
                xaml: `<dash:CardCarousel Height="270"
                   ItemWidth="280"
                   ItemHeight="175"
                   Spacing="190"
                   AutoRun="False"
                   SelectedIndex="1">
    <dash:CardCarouselItem>
        <Border Background="{DynamicResource CONCOST.Brush.Surface}"
                BorderBrush="{DynamicResource CONCOST.Brush.Border}"
                BorderThickness="1" CornerRadius="10" Padding="14">
            <StackPanel>
                <TextBlock Text="Landmark 81 Tower" FontWeight="Bold" Foreground="{DynamicResource CONCOST.Brush.TextPrimary}"/>
                <TextBlock Text="Phân tích kết cấu chịu tải gió" FontSize="11.5" Foreground="{DynamicResource CONCOST.Brush.TextSecondary}"/>
            </StackPanel>
        </Border>
    </dash:CardCarouselItem>
</dash:CardCarousel>`,
                csharp: `// Tương tác điều khiển CardCarousel qua Code-Behind:
var carousel = new CardCarousel();
carousel.MoveNext(); // Chuyển sang thẻ kế tiếp
carousel.MovePrev(); // Quay về thẻ phía trước`
            }
        ]
    },

    timeline: {
        title: "Timeline (Dòng Thời Gian Tiến Độ)",
        badge: "Dashboard Suite",
        desc: "Hiển thị chuỗi sự kiện hoặc tiến độ thi công công trình theo trục thời gian tuyến tính với các nút trạng thái Semantic, Vector Icon, và đường nối tự động ẩn ở nút cuối cùng.",
        props: [
            { name: "Timestamp", type: "string", default: "null", desc: "Chuỗi hiển thị ngày giờ / mốc thời gian sự kiện" },
            { name: "Type", type: "TimelineItemType", default: "Primary", desc: "Kiểu màu nút: Primary, Success, Warning, Danger, Info" },
            { name: "Icon", type: "IconKind", default: "None", desc: "Icon vector hiển thị bên trong vòng tròn timeline" }
        ],
        sections: [
            {
                title: "Cú Pháp XAML Chuẩn",
                desc: "Tạo danh sách tiến độ dự án với Timeline:",
                xaml: `<dash:Timeline>
    <dash:TimelineItem Header="Khởi Công Thi Công Cọc Khoan Nhồi"
                       Timestamp="2026-03-01 08:00"
                       Type="Success"
                       Icon="CheckCircle">
        <TextBlock Text="Hoàn thành ép 120/120 cọc D1500 đạt tải trọng thiết kế." 
                   Foreground="{DynamicResource CONCOST.Brush.TextSecondary}"/>
    </dash:TimelineItem>
</dash:Timeline>`
            }
        ]
    },

    dialog: {
        title: "Dialog (Hộp Thoại Modal Pop-up)",
        badge: "Custom Controls",
        desc: "Hộp thoại Popup hiện đại với hiệu ứng Zoom & Fade, làm mờ lớp nền (Backdrop Dimming), hỗ trợ đầy đủ Header, Action Footer, và kéo thả di chuyển.",
        props: [
            { name: "Title", type: "string", default: "null", desc: "Tiêu đề của hộp thoại modal" },
            { name: "IsOpen", type: "bool", default: "false", desc: "Trạng thái hiển thị mở/đóng của Dialog" },
            { name: "ShowCloseButton", type: "bool", default: "true", desc: "Bật nút đóng nhanh [X] ở góc phải" },
            { name: "CornerRadius", type: "CornerRadius", default: "12", desc: "Độ bo góc của hộp thoại" }
        ],
        sections: [
            {
                title: "Hiển Thị Dialog trong XAML",
                desc: "Sử dụng `Dialog` control:",
                xaml: `<ui:Dialog x:Name="MyDialog"
           Title="Nghiệm Thu Khối Lượng"
           IsOpen="True"
           CornerRadius="12">
    <StackPanel Margin="10">
        <TextBlock Text="Bạn có chắc chắn muốn phê duyệt đợt thanh toán?" 
                   Foreground="{DynamicResource CONCOST.Brush.TextPrimary}"/>
        <Button Style="{DynamicResource CONCOST.Button.Primary}" Content="Xác Nhận" Margin="0,16,0,0"/>
    </StackPanel>
</ui:Dialog>`
            }
        ]
    },

    drawer: {
        title: "Drawer (Ngăn Kéo Trượt Side-Panel)",
        badge: "Custom Controls",
        desc: "Bảng điều khiển trượt từ 4 cạnh màn hình (Left, Right, Top, Bottom) kèm hiệu ứng làm mờ màn chắn (Backdrop Mask) để hiển thị chi tiết đối tượng BIM hoặc form cấu hình.",
        props: [
            { name: "IsOpen", type: "bool", default: "false", desc: "Bật/Tắt mở ngăn kéo trượt" },
            { name: "Dock", type: "Dock", default: "Right", desc: "Hướng trượt ra: Left, Right, Top, Bottom" },
            { name: "MaskClosable", type: "bool", default: "true", desc: "Cho phép click ra ngoài màn chắn mờ để tự động đóng Drawer" }
        ],
        sections: [
            {
                title: "Cú Pháp XAML",
                desc: "Khai báo ngăn kéo cấu hình thuộc tính:",
                xaml: `<ui:Drawer IsOpen="True" Dock="Right" Width="380" MaskClosable="True">
    <StackPanel Margin="20">
        <TextBlock Text="Thông Số Cột Bê Tông C1" FontWeight="Bold" FontSize="16"/>
        <TextBox Style="{DynamicResource CONCOST.TextBox.Base}" Margin="0,10" attached:InputBase.Placeholder="Tên cấu kiện"/>
    </StackPanel>
</ui:Drawer>`
            }
        ]
    },

    growl: {
        title: "Growl (Thông Báo Nổi Toast)",
        badge: "Custom Controls",
        desc: "Hệ thống thông báo đẩy dạng Toast xếp chồng thông minh ở góc màn hình, hỗ trợ tự động biến mất (Auto-Dismiss), thanh đếm thời gian và các mức độ Success, Warning, Error, Info.",
        sections: [
            {
                title: "Gọi Thông Báo Trong C#",
                desc: "Sử dụng lớp tiện ích `Growl` hoặc `ToastService`:",
                csharp: `// Thông báo Thành công
Growl.Success("Đã lưu mô hình BIM thành công!", "CONCOST System");

// Thông báo Cảnh báo
Growl.Warning("Cảnh báo xung đột cao độ dầm và ống HVAC!", "Clash Detection");

// Thông báo Lỗi
Growl.Error("Không thể kết nối đến máy chủ CSDL SQL!", "Lỗi Kết Nối");`
            }
        ]
    },

    propertygrid: {
        title: "PropertyGrid (Thanh Thuộc Tính Đối Tượng)",
        badge: "Custom Controls",
        desc: "Bảng duyệt và chỉnh sửa thông số đối tượng CAD / BIM chuyên dụng, tự động gom nhóm Category, hỗ trợ editor nội dòng (TextBox, CheckBox, ComboBox, ColorPicker, NumericUpDown).",
        sections: [
            {
                title: "Gán Object Dữ Liệu Vào PropertyGrid",
                desc: "Sử dụng thuộc tính `SelectedObject`:",
                xaml: `<ui:PropertyGrid SelectedObject="{Binding SelectedStructuralElement}" />`
            }
        ]
    },

    numericupdown: {
        title: "NumericUpDown (Nhập Số Độ Chính Xác Cao)",
        badge: "Custom Controls",
        desc: "Bộ nhập giá trị số với 2 nút tăng giảm, hỗ trợ bước nhảy Step, giới hạn Min/Max, định dạng tiền tệ / đơn vị kỹ thuật (mm, m3, kN, MPa).",
        props: [
            { name: "Value", type: "double", default: "0", desc: "Giá trị số hiện tại" },
            { name: "Minimum", type: "double", default: "0", desc: "Giá trị nhỏ nhất cho phép" },
            { name: "Maximum", type: "double", default: "100", desc: "Giá trị lớn nhất cho phép" },
            { name: "Increment", type: "double", default: "1", desc: "Bước nhảy mỗi lần bấm tăng/giảm" },
            { name: "Unit", type: "string", default: "null", desc: "Hậu tố đơn vị hiển thị (vd: mm, m², kN)" }
        ],
        sections: [
            {
                title: "Ví Dụ XAML",
                desc: "Ô nhập bề dày sàn bê tông:",
                xaml: `<ui:NumericUpDown Value="250" Minimum="50" Maximum="1000" Increment="10" Unit="mm" />`
            }
        ]
    },

    charts: {
        title: "Charts Suite (Biểu Đồ Vector Kỹ Thuật)",
        badge: "Custom Controls",
        desc: "Bộ biểu đồ vector độ phân giải cao bao gồm ChartBar, ChartLine, ChartPie, ChartRadar và PieControl hiển thị trực quan tỷ trọng khối lượng dự án.",
        sections: [
            {
                title: "Khai Báo ChartBar XAML",
                desc: "Biểu đồ cột tiến độ thi công:",
                xaml: `<ui:ChartBar ItemsSource="{Binding MonthlyProgressData}" Height="220" />`
            }
        ]
    },

    stepbar: {
        title: "StepBar (Quy Trình Các Bước)",
        badge: "Custom Controls",
        desc: "Thanh hiển thị tiến trình công việc nhiều bước (Wizard Steps) với các trạng thái Đang Chờ (Waiting), Đang Làm (In-Progress), Hoàn Thành (Finished) và Lỗi (Error).",
        props: [
            { name: "StepIndex", type: "int", default: "0", desc: "Vị trí bước hiện tại đang thực hiện" }
        ],
        sections: [
            {
                title: "Cú Pháp XAML",
                desc: "Quy trình lập dự toán công trình:",
                xaml: `<ui:StepBar StepIndex="1">
    <ui:StepBarItem Header="Nhập Mô Hình IFC" Status="Complete"/>
    <ui:StepBarItem Header="Bóc Tách Khối Lượng" Status="InProgress"/>
    <ui:StepBarItem Header="Áp Định Mức Đơn Giá" Status="Waiting"/>
    <ui:StepBarItem Header="Xuất Báo Cáo Quyết Toán" Status="Waiting"/>
</ui:StepBar>`
            }
        ]
    },

    transfer: {
        title: "Transfer (Hộp Chuyển Danh Sách 2 Chiều)",
        badge: "Custom Controls",
        desc: "Bộ chuyển dữ liệu 2 khung đối xứng (Shuttle List) hỗ trợ lọc tìm kiếm tức thì, chọn tất cả và di chuyển hàng loạt cấu kiện qua lại.",
        sections: [
            {
                title: "Ví Dụ XAML",
                desc: "Phân bổ cấu kiện thép sang tổ đội thi công:",
                xaml: `<ui:Transfer ItemsSource="{Binding AllBeamsList}" SelectedItemsSource="{Binding AssignedBeamsList}" />`
            }
        ]
    },

    checkcombobox: {
        title: "CheckComboBox (Dropdown Chọn Nhiều)",
        badge: "Custom Controls",
        desc: "Hộp thoại chọn danh sách nhiều mục với ô CheckBox cho từng dòng, tích hợp tùy chọn 'Chọn tất cả (Select All)' và đếm số lượng đã chọn.",
        sections: [
            {
                title: "Cú Pháp XAML",
                desc: "Bộ lọc danh sách tầng công trình:",
                xaml: `<ui:CheckComboBox ItemsSource="{Binding FloorList}" SelectAllText="Tất cả các tầng" />`
            }
        ]
    },

    pinbox: {
        title: "PinBox (Ô Nhập Mã PIN / OTP)",
        badge: "Custom Controls",
        desc: "Bộ nhập mã xác minh bản quyền hoặc OTP từng ô riêng biệt, tự động nhảy con trỏ sang ô tiếp theo và hỗ trợ ẩn mật khẩu.",
        sections: [
            {
                title: "Ví Dụ XAML",
                desc: "Ô nhập mã kích hoạt License phần mềm:",
                xaml: `<ui:PinBox Length="6" PasswordChar="•" />`
            }
        ]
    },

    colorpicker: {
        title: "ColorPicker (Bảng Chọn Màu Toàn Diện)",
        badge: "Custom Controls",
        desc: "Bộ chọn màu chuyên nghiệp với Canvas 2D Saturation-Value, thanh trượt Hue/Alpha, ô nhập mã màu Hex/RGBA và bảng màu mẫu Presets.",
        sections: [
            {
                title: "Ví Dụ XAML",
                desc: "Chọn màu hiển thị cho lớp bê tông:",
                xaml: `<ui:ColorPicker SelectedColor="{Binding StructuralLayerColor, Mode=TwoWay}" />`
            }
        ]
    },

    clock: {
        title: "Clock & CalendarWithClock",
        badge: "Custom Controls",
        desc: "Đồng hồ tương tác tròn phong cách Material Design chọn Giờ/Phút/Giây kết hợp Lịch chọn ngày tháng.",
        sections: [
            {
                title: "Cú Pháp XAML",
                desc: "Chọn mốc thời gian nghiệm thu:",
                xaml: `<ui:CalendarWithClock SelectedDateTime="{Binding InspectionDateTime, Mode=TwoWay}" />`
            }
        ]
    },

    sidemenu: {
        title: "SideMenu (Thanh Menu Dọc Ứng Dụng)",
        badge: "Custom Controls",
        desc: "Menu điều hướng dọc hiện đại, hỗ trợ mở rộng/thu gọn icon-only, hiển thị Badge số lượng thông báo và phân nhóm cấp mục.",
        sections: [
            {
                title: "Ví Dụ XAML",
                desc: "Thanh điều hướng phân hệ chính:",
                xaml: `<ui:SideMenu IsCollapsed="False">
    <ui:SideMenuItem Header="Tổng Quan Dự Án" Icon="Layers" IsSelected="True"/>
    <ui:SideMenuItem Header="Mô Hình Kết Cấu" Icon="Folder"/>
    <ui:SideMenuItem Header="Dự Toán & Chi Phí" Icon="File" Badge="3"/>
</ui:SideMenu>`
            }
        ]
    },

    drawermenu: {
        title: "DrawerMenu (Menu Ngăn Kéo Thu Phóng Chuẩn WPFDevelopers)",
        badge: "Custom Controls & Navigation",
        desc: "Thanh điều hướng ngăn kéo thông minh lấy cảm hứng từ WPFDevelopers, hỗ trợ thu gọn/mở rộng động (Expand/Collapse Animation mượt mà), thanh Header chứa Logo & Nút Toggle Menu, Item Template kèm Vector Icon, Title, Badge thông báo và khu vực Footer cố định.",
        props: [
            { name: "IsOpen", type: "bool", default: "true", desc: "Trạng thái mở rộng (True) hoặc thu gọn thành dạng Rail Icon-Only (False)" },
            { name: "CompactWidth", type: "double", default: "64", desc: "Chiều rộng khi ở chế độ thu gọn (chỉ hiển thị Icon)" },
            { name: "ExpandedWidth", type: "double", default: "240", desc: "Chiều rộng khi mở rộng đầy đủ (Icon + Text + Badge)" },
            { name: "ShowToggleButton", type: "bool", default: "true", desc: "Bật/Tắt nút bấm 3 gạch ngang toggle ở góc trên" },
            { name: "SelectedItem", type: "DrawerMenuItem", default: "null", desc: "Mục menu đang được chọn" }
        ],
        sections: [
            {
                title: "1. Khai Báo Trong XAML",
                desc: "Sử dụng `DrawerMenu` và danh sách `DrawerMenuItem`:",
                xaml: `<ui:DrawerMenu IsOpen="True"
               CompactWidth="64"
               ExpandedWidth="240"
               ItemsSource="{Binding MenuItemsList}">
    <!-- Header: Logo và Tên Ứng Dụng -->
    <ui:DrawerMenu.Header>
        <StackPanel Orientation="Horizontal" VerticalAlignment="Center">
            <icons:Icon Kind="Ruler" Width="20" Height="20" Foreground="{DynamicResource CONCOST.Brush.Primary}" Margin="0,0,10,0"/>
            <TextBlock Text="CONCOST BIM" FontWeight="Bold" FontSize="14" Foreground="{DynamicResource CONCOST.Brush.TextPrimary}"/>
        </StackPanel>
    </ui:DrawerMenu.Header>

    <!-- Footer: Bản quyền / Phiên bản -->
    <ui:DrawerMenu.Footer>
        <TextBlock Text="v3.0.0 Enterprise" FontSize="11" Foreground="{DynamicResource CONCOST.Brush.TextSecondary}" HorizontalAlignment="Center"/>
    </ui:DrawerMenu.Footer>
</ui:DrawerMenu>`,
                csharp: `// Khởi tạo danh sách Menu Items trong ViewModel / Code-Behind:
public ObservableCollection<DrawerMenuItem> MenuItemsList { get; } = new()
{
    new DrawerMenuItem("Tổng Quan Dự Án", IconKind.Layers, "PRO") { IsSelected = true },
    new DrawerMenuItem("Mô Hình Kết Cấu 3D", IconKind.Folder),
    new DrawerMenuItem("Bóc Tách Khối Lượng", IconKind.File, "12"),
    new DrawerMenuItem("Dự Toán & Đơn Giá", IconKind.CheckCircle),
    new DrawerMenuItem("Cài Đặt Hệ Thống", IconKind.Settings)
};`
            }
        ]
    },

    snowcanvas: {
        title: "SnowCanvas (Hiệu Ứng Tuyết Rơi Hạt GPU)",
        badge: "Canvas Effects",
        desc: "Khung vẽ hạt tuyết rơi chuyển động GPU siêu mượt (lấy cảm hứng từ WPFDevelopers), hỗ trợ tùy chỉnh mật độ số lượng hạt, bán kính Min/Max, tốc độ rơi, độ lắc lư dao động (Oscillation Swing), lực gió thổi ngang (Wind Drift), màu hạt và tạm dừng/tiếp tục chuyển động.",
        props: [
            { name: "SnowflakeCount", type: "int", default: "60", desc: "Số lượng hạt tuyết sinh ra đồng thời trên màn hình" },
            { name: "SnowflakeBrush", type: "Brush", default: "Brushes.White", desc: "Màu sắc của hạt tuyết (có thể đổi màu theo theme)" },
            { name: "MinSnowflakeSize", type: "double", default: "1.5", desc: "Kích thước bán kính hạt tuyết nhỏ nhất (px)" },
            { name: "MaxSnowflakeSize", type: "double", default: "4.5", desc: "Kích thước bán kính hạt tuyết lớn nhất (px)" },
            { name: "MinSpeed", type: "double", default: "0.8", desc: "Vận tốc rơi tối thiểu theo trục dọc" },
            { name: "MaxSpeed", type: "double", default: "2.5", desc: "Vận tốc rơi tối đa theo trục dọc" },
            { name: "Wind", type: "double", default: "0.2", desc: "Lực gió làm trôi dạt hạt tuyết theo phương ngang" },
            { name: "IsActive", type: "bool", default: "true", desc: "Bật/Tắt hiệu ứng chuyển động của hạt tuyết" }
        ],
        sections: [
            {
                title: "1. Sử Dụng Trong XAML",
                desc: "Nhúng `SnowCanvas` làm nền mờ cho Login Page, Dashboard Banner hoặc Giáng Sinh / Năm Mới:",
                xaml: `<Grid Height="320" ClipToBounds="True">
    <!-- Lớp nền Gradient tối -->
    <Border Background="#0F172A" CornerRadius="10" />

    <!-- Canvas hạt tuyết rơi -->
    <canvas:SnowCanvas SnowflakeCount="80"
                       MinSnowflakeSize="1.5"
                       MaxSnowflakeSize="5.0"
                       MinSpeed="1.0"
                       MaxSpeed="2.5"
                       Wind="0.3"
                       SnowflakeBrush="White" />

    <!-- Nội dung hiển thị bên trên lớp tuyết rơi -->
    <StackPanel VerticalAlignment="Center" HorizontalAlignment="Center">
        <TextBlock Text="CHÚC MỪNG NĂM MỚI 2026" FontSize="22" FontWeight="Bold" Foreground="White" HorizontalAlignment="Center"/>
        <TextBlock Text="Hệ Thống Quản Lý Kết Cấu CONCOST BIM" FontSize="13" Foreground="#94A3B8" Margin="0,8,0,0" HorizontalAlignment="Center"/>
    </StackPanel>
</Grid>`
            }
        ]
    },

    sprite: {
        title: "Sprite (Trợ Lý Nổi Kéo Thả Hít Viền Chuẩn HandyControl)",
        badge: "Floating Assistant & Widget",
        desc: "Nút trợ lý nổi tương tác thông minh (Assistive Touch / Floating Action Button) lấy cảm hứng từ HandyControl. Sprite luôn nổi phía trên toàn bộ giao diện, cho phép người dùng kéo thả tự do bằng chuột khắp màn hình, tự động 'hít' (Auto-Snap) vào mép viền cửa sổ gần nhất khi thả tay và bung mở Popup Menu phím tắt nhanh chỉ với 1 click.",
        props: [
            { name: "IsOpen", type: "bool", default: "false", desc: "Trạng thái mở/đóng của Popup Menu phím tắt nhanh" },
            { name: "MenuContent", type: "object", default: "null", desc: "Nội dung giao diện hiển thị bên trong Popup Menu mở rộng" },
            { name: "AutoSnap", type: "bool", default: "true", desc: "Tự động hít vào mép viền trái/phải gần nhất của cửa sổ khi ngừng kéo" },
            { name: "CornerRadius", type: "CornerRadius", default: "24", desc: "Độ bo góc của nút nổi Sprite (mặc định tròn 48x48)" }
        ],
        sections: [
            {
                title: "1. Khai Báo Trong XAML",
                desc: "Thêm `Sprite` vào Grid gốc của Window hoặc UserControl:",
                xaml: `<Grid>
    <!-- Giao diện chính của ứng dụng -->
    <Frame x:Name="MainContentFrame" />

    <!-- Trợ lý nổi Sprite kéo thả tự do -->
    <ui:Sprite Width="52" Height="52"
               Margin="24,24,0,0"
               HorizontalAlignment="Left"
               VerticalAlignment="Top"
               AutoSnap="True">
        <!-- Icon hiển thị trong nút tròn -->
        <icons:Icon Kind="Layers" Width="22" Height="22" Foreground="#FFFFFF"/>

        <!-- Menu phím tắt mở rộng khi click -->
        <ui:Sprite.MenuContent>
            <StackPanel Width="180" Margin="4">
                <TextBlock Text="Phím Tắt Kỹ Thuật" FontWeight="Bold" FontSize="12" Margin="0,0,0,8"/>
                <Button Style="{DynamicResource CONCOST.Button.Secondary}" Content="Bóc Tách Thép Dầm" Margin="0,2"/>
                <Button Style="{DynamicResource CONCOST.Button.Secondary}" Content="Kiểm Tra Xung Đột" Margin="0,2"/>
                <Button Style="{DynamicResource CONCOST.Button.Primary}" Content="Xuất Quyết Toán" Margin="0,2"/>
            </StackPanel>
        </ui:Sprite.MenuContent>
    </ui:Sprite>
</Grid>`
            }
        ]
    },

    compareslider: {
        title: "CompareSlider (Thanh Trượt So Sánh 2 Ảnh Before / After)",
        badge: "Interactive & BIM",
        desc: "Thanh trượt chia đôi màn hình trực quan cho phép so sánh sự thay đổi giữa 2 hình ảnh bản vẽ thiết kế (Rev 01 vs Rev 02), phối cảnh 3D Render trước/sau tối ưu kết cấu hoặc ảnh chụp thực tế công trường.",
        props: [
            { name: "SourceContent", type: "object", default: "null", desc: "Nội dung gốc nằm bên dưới (Bản vẽ Trước / Before)" },
            { name: "TargetContent", type: "object", default: "null", desc: "Nội dung so sánh nằm đè bên trên có mặt nạ cắt Clip Mask (Bản vẽ Sau / After)" },
            { name: "Value", type: "double", default: "0.5", desc: "Tỉ lệ vị trí đường phân cách cắt ngang (từ 0.0 đến 1.0)" },
            { name: "Orientation", type: "Orientation", default: "Horizontal", desc: "Hướng trượt ngang (Horizontal) hoặc trượt dọc (Vertical)" },
            { name: "HandleSize", type: "double", default: "36.0", desc: "Kích thước đường kính của nút tròn cầm kéo thanh trượt" }
        ],
        sections: [
            {
                title: "1. Sử Dụng Trong XAML",
                desc: "So sánh phương án dầm bê tông trước và sau khi tối ưu:",
                xaml: `<ui:CompareSlider Height="300" Value="0.5">
    <ui:CompareSlider.SourceContent>
        <Image Source="/Assets/Drawing_RevA.png" Stretch="UniformToFill"/>
    </ui:CompareSlider.SourceContent>
    <ui:CompareSlider.TargetContent>
        <Image Source="/Assets/Drawing_RevB.png" Stretch="UniformToFill"/>
    </ui:CompareSlider.TargetContent>
</ui:CompareSlider>`
            }
        ]
    },

    magnifier: {
        title: "Magnifier (Kính Lúp Phóng To Soi Điểm Ảnh CAD/BIM)",
        badge: "Interactive & Engineering",
        desc: "Tròng kính lúp phóng to tọa độ điểm ảnh độ nét cao (lấy cảm hứng từ HandyControl & WPFDevelopers). Khi rê chuột vào khu vực bản vẽ CAD hoặc mặt cắt dầm cột, kính lúp tròn có hồng tâm chữ thập sẽ tự động bám theo và phóng to chi tiết 2x - 5x.",
        props: [
            { name: "Target", type: "FrameworkElement", default: "null", desc: "Phần tử UI hoặc hình ảnh cần được soi phóng to" },
            { name: "Scale", type: "double", default: "2.5", desc: "Hệ số phóng đại kính lúp (vd: 2.0x, 3.5x, 5.0x)" },
            { name: "Radius", type: "double", default: "60.0", desc: "Bán kính tròng kính lúp tròn (px)" },
            { name: "BorderBrush", type: "Brush", default: "Primary", desc: "Màu viền ngoài của tròng kính lúp" }
        ],
        sections: [
            {
                title: "1. Sử Dụng Trong XAML",
                desc: "Gắn kính lúp phóng đại vào một khung vẽ bản vẽ kỹ thuật:",
                xaml: `<Grid>
    <!-- Vùng bản vẽ kỹ thuật -->
    <Border x:Name="CadDrawingView" Height="320" Background="#1E293B">
        <!-- Nội dung bản vẽ CAD chi tiết -->
    </Border>

    <!-- Kính lúp phóng to bám theo chuột -->
    <ui:Magnifier Target="{Binding ElementName=CadDrawingView}" Scale="3.0" Radius="65"/>
</Grid>`
            }
        ]
    },

    rangeslider: {
        title: "RangeSlider (Thanh Trượt Chọn Khoảng Giá Trị Min-Max)",
        badge: "Engineering & Filter",
        desc: "Bộ điều khiển thanh trượt 2 đầu (Dual-Thumb Slider) cho phép kỹ sư chọn nhanh một khoảng giá trị liên tục [StartValue ~ EndValue] để lọc dầm theo tiết diện, lọc cọc theo chiều sâu, lọc dự toán theo tầm giá.",
        props: [
            { name: "Minimum", type: "double", default: "0.0", desc: "Giá trị chặn dưới nhỏ nhất của toàn dải trượt" },
            { name: "Maximum", type: "double", default: "100.0", desc: "Giá trị chặn trên lớn nhất của toàn dải trượt" },
            { name: "StartValue", type: "double", default: "20.0", desc: "Giá trị đầu trượt bên trái (Min Filter)" },
            { name: "EndValue", type: "double", default: "80.0", desc: "Giá trị đầu trượt bên phải (Max Filter)" }
        ],
        sections: [
            {
                title: "1. Sử Dụng Trong XAML",
                desc: "Lọc các cấu kiện dầm có bề rộng từ 200mm đến 600mm:",
                xaml: `<ui:RangeSlider Minimum="100" Maximum="1000"
                StartValue="{Binding MinBeamWidth, Mode=TwoWay}"
                EndValue="{Binding MaxBeamWidth, Mode=TwoWay}"
                Height="32" />`
            }
        ]
    },

    autocompletetextbox: {
        title: "AutoCompleteTextBox (Ô Tìm Kiếm Gợi Ý Tự Động)",
        badge: "Input & Suggestion",
        desc: "Hộp nhập văn bản thông minh có Popup Dropdown tự động lọc và gợi ý danh sách mã định mức, vật tư, tiêu chuẩn kỹ thuật khi người dùng bắt đầu gõ phím.",
        props: [
            { name: "ItemsSource", type: "IEnumerable", default: "null", desc: "Danh sách nguồn dữ liệu gợi ý (List<string> hoặc đối tượng)" },
            { name: "MaxDropDownHeight", type: "double", default: "220.0", desc: "Chiều cao tối đa của Popup danh sách gợi ý" },
            { name: "IsDropDownOpen", type: "bool", default: "false", desc: "Trạng thái hiển thị mở/đóng của danh sách gợi ý" }
        ],
        sections: [
            {
                title: "1. Sử Dụng Trong XAML",
                desc: "Tìm nhanh vật tư xây dựng:",
                xaml: `<ui:AutoCompleteTextBox ItemsSource="{Binding MaterialStandardsList}"
                        Text="{Binding SelectedMaterialCode, Mode=TwoWay}"
                        Width="320" Height="34" />`
            }
        ]
    },

    searchbox: {
        title: "SearchBox (Ô Tìm Kiếm Thông Minh)",
        badge: "Custom Controls",
        desc: "Ô tìm kiếm tích hợp icon kính lúp, nút xóa trắng nhanh (Clear Button) và sự kiện tìm kiếm tức thời SearchTrigger.",
        sections: [
            {
                title: "Ví Dụ XAML",
                desc: "Tìm kiếm mã hiệu vật tư:",
                xaml: `<ui:SearchBox Style="{DynamicResource CONCOST.SearchBox.Base}" attached:InputBase.Placeholder="Nhập tên dầm, cột, sàn..." />`
            }
        ]
    },

    tag: {
        title: "Tag / Chip (Nhãn Cấu Kiện)",
        badge: "Custom Controls",
        desc: "Nhãn trạng thái và phân loại cấu kiện với nút xóa hành động [X], vector icons và các bảng màu trạng thái Semantic.",
        sections: [
            {
                title: "Ví Dụ XAML",
                desc: "Tag trạng thái kiểm tra:",
                xaml: `<ui:Tag Content="Đã Nghiệm Thu" Type="Success" ShowCloseButton="True"/>`
            }
        ]
    },

    pagination: {
        title: "Pagination (Phân Trang Dữ Liệu)",
        badge: "Custom Controls",
        desc: "Bộ điều khiển phân trang dữ liệu cho các bảng khối lượng lớn, hỗ trợ nhảy trang nhanh và cấu hình PageSize.",
        sections: [
            {
                title: "Ví Dụ XAML",
                desc: "Phân trang bảng vật tư:",
                xaml: `<ui:Pagination TotalCount="1250" PageSize="25" CurrentPage="1" />`
            }
        ]
    },

    filepicker: {
        title: "FilePicker (Chọn Tệp & Thư Mục)",
        badge: "Custom Controls",
        desc: "Hộp chọn file tích hợp sẵn nút duyệt Browse với hộp thoại OpenFileDialog / FolderBrowserDialog.",
        sections: [
            {
                title: "Ví Dụ XAML",
                desc: "Chọn file bản vẽ CAD/IFC:",
                xaml: `<ui:FilePicker Filter="IFC Files (*.ifc)|*.ifc|All Files (*.*)|*.*" />`
            }
        ]
    },

    breadcrumb: {
        title: "Breadcrumb (Đường Dẫn Phân Cấp)",
        badge: "Custom Controls",
        desc: "Đường dẫn điều hướng vị trí trong mô hình công trình (vd: Tòa Nhà A > Tầng 15 > Khu Vực Cốt Thép Dầm).",
        sections: [
            {
                title: "Ví Dụ XAML",
                desc: "Breadcrumb phân cấp mô hình:",
                xaml: `<ui:Breadcrumb>
    <ui:BreadcrumbItem Content="Landmark 81"/>
    <ui:BreadcrumbItem Content="Tầng Hầm B2"/>
    <ui:BreadcrumbItem Content="Dầm Chuyển D1"/>
</ui:Breadcrumb>`
            }
        ]
    },

    statisticcard: {
        title: "StatisticCard (Thẻ Chỉ Số KPI)",
        badge: "Custom Controls",
        desc: "Thẻ hiển thị số liệu quan trọng trên Dashboard, hỗ trợ chỉ số tăng/giảm phần trăm (Trend) và Icon điểm nhấn.",
        sections: [
            {
                title: "Ví Dụ XAML",
                desc: "Thẻ chỉ số tổng khối lượng bê tông:",
                xaml: `<ui:StatisticCard Title="Tổng Bê Tông Đã Đổ" Value="14,250 m³" Trend="+12.5%" TrendDirection="Up" Icon="Layers"/>`
            }
        ]
    },

    rating: {
        title: "Rating (Đánh Giá Sao)",
        badge: "Custom Controls",
        desc: "Bộ đánh giá xếp hạng chất lượng thi công hoặc nhà thầu theo số sao tương tác.",
        sections: [
            {
                title: "Ví Dụ XAML",
                desc: "Đánh giá chất lượng thép:",
                xaml: `<ui:Rating Value="4.5" Maximum="5" AllowHalf="True"/>`
            }
        ]
    },

    watermark: {
        title: "Watermark (Hình Mờ Bản Quyền)",
        badge: "Custom Controls",
        desc: "Lớp phủ hình mờ (DRAFT, CONFIDENTIAL, COPYRIGHT) chống sao chép tài liệu bản vẽ và báo cáo PDF.",
        sections: [
            {
                title: "Ví Dụ XAML",
                desc: "Bản vẽ sơ bộ chưa phê duyệt:",
                xaml: `<ui:Watermark Text="BẢN VẼ TẠM THỜI - CHƯA PHÊ DUYỆT" Opacity="0.15" />`
            }
        ]
    },

    emptystate: {
        title: "EmptyState (Trạng Thái Trống)",
        badge: "Custom Controls",
        desc: "Giao diện minh họa thông minh hiển thị khi danh sách hoặc bảng dữ liệu không có phần tử nào.",
        sections: [
            {
                title: "Ví Dụ XAML",
                desc: "Không tìm thấy cấu kiện:",
                xaml: `<ui:EmptyState Title="Chưa có dữ liệu" Description="Vui lòng import mô hình IFC hoặc tạo mới cấu kiện." />`
            }
        ]
    },

    divider: {
        title: "Divider (Đường Phân Cách)",
        badge: "Custom Controls",
        desc: "Đường kẻ phân chia khu vực bố cục với tiêu đề nhãn văn bản ở giữa.",
        sections: [
            {
                title: "Ví Dụ XAML",
                desc: "Phân cách nhóm thông số:",
                xaml: `<ui:Divider Text="Thông Số Vật Liệu Thép" HorizontalAlignment="Stretch"/>`
            }
        ]
    },

    icons: {
        title: "Icons Gallery (Thư Viện Vector Icons)",
        badge: "Custom Controls",
        desc: "Kho hơn 120+ vector icons độ phân giải vô cực được tối ưu hóa cho ngành kỹ thuật, xây dựng, CAD và BIM.",
        sections: [
            {
                title: "Sử Dụng Icon Trong XAML",
                desc: "Gọi Icon trực tiếp qua thẻ `icons:Icon`:",
                xaml: `<icons:Icon Kind="Ruler" Width="20" Height="20" Foreground="{DynamicResource CONCOST.Brush.Primary}" />
<icons:Icon Kind="Layers" Width="20" Height="20" Foreground="{DynamicResource CONCOST.Brush.TextPrimary}" />
<icons:Icon Kind="ShieldCheck" Width="20" Height="20" Foreground="#16A34A" />`
            }
        ]
    },

    // =============================================================
    // BASIC & ENHANCED CONTROLS
    // =============================================================
    button: {
        title: "Button (Nút Bấm)",
        badge: "Basic Controls",
        desc: "Hệ thống nút bấm đa dạng với các kiểu dáng (Primary, Secondary, Success, Outline, Danger), tích hợp vòng xoay Loading Spinner và gắn Vector Icon trực tiếp qua Attached Properties.",
        sections: [
            {
                title: "Các Biến Thể Style Chuẩn",
                desc: "Sử dụng Style `{DynamicResource CONCOST.Button.*}` kèm `attached:IconElement`:",
                xaml: `<!-- Primary Button kèm Icon -->
<Button Style="{DynamicResource CONCOST.Button.Primary}"
        Content="Lưu Dự Án"
        attached:IconElement.Kind="Check"
        attached:BorderElement.CornerRadius="8"
        Padding="16,8" />

<!-- Danger Button -->
<Button Style="{DynamicResource CONCOST.Button.Danger}"
        Content="Xóa Mô Hình"
        attached:IconElement.Kind="Trash"
        attached:BorderElement.CornerRadius="8"
        Padding="16,8" />`
            }
        ]
    },

    togglebutton: {
        title: "ToggleButton & RadioButton & CheckBox",
        badge: "Basic Controls",
        desc: "Bộ ba điều khiển chọn trạng thái: ToggleButton 2 trạng thái, RadioButton chọn một và CheckBox chọn nhiều.",
        sections: [
            {
                title: "Ví Dụ XAML",
                desc: "Khai báo các lựa chọn:",
                xaml: `<CheckBox Content="Bật tính toán biến dạng dầm" IsChecked="True" />
<RadioButton Content="Phương pháp PTHH tuyến tính" IsChecked="True" GroupName="Method" />
<RadioButton Content="Phương pháp phi tuyến" GroupName="Method" />`
            }
        ]
    },

    toggleswitch: {
        title: "ToggleSwitch (Công Tắc Gạt On/Off)",
        badge: "Basic Controls",
        desc: "Công tắc gạt bật/tắt thiết lập với chuyển động hoạt ảnh mượt mà.",
        sections: [
            {
                title: "Ví Dụ XAML",
                desc: "Công tắc gạt chế độ tối:",
                xaml: `<ui:ToggleSwitch IsChecked="True" OnContent="Bật" OffContent="Tắt"/>`
            }
        ]
    },

    textbox: {
        title: "TextBox & PasswordBox (Ô Nhập Liệu)",
        badge: "Basic Controls",
        desc: "Ô nhập văn bản nâng cao với placeholder mờ, nút xóa nhanh [X], icon tiền tố/hậu tố và ô nhập mật khẩu với nút bật/tắt hiển thị.",
        sections: [
            {
                title: "Ví Dụ XAML",
                desc: "Nhập liệu thông tin:",
                xaml: `<TextBox Style="{DynamicResource CONCOST.TextBox.Base}"
         attached:InputBase.Placeholder="Nhập tên kỹ sư thiết kế..."
         attached:InputBase.ShowClearButton="True"
         attached:IconElement.Kind="Edit" />`
            }
        ]
    },

    combobox: {
        title: "ComboBox (Danh Sách Thả Dropdown)",
        badge: "Basic Controls",
        desc: "Menu chọn thả xuống hiện đại với mũi tên hoạt ảnh và hỗ trợ bo góc, chế độ tìm kiếm tích hợp.",
        sections: [
            {
                title: "Ví Dụ XAML",
                desc: "Chọn cấp độ bền bê tông:",
                xaml: `<ComboBox Style="{DynamicResource CONCOST.ComboBox.Base}">
    <ComboBoxItem Content="Bê tông B25 (M350)"/>
    <ComboBoxItem Content="Bê tông B30 (M400)" IsSelected="True"/>
    <ComboBoxItem Content="Bê tông B40 (M500)"/>
</ComboBox>`
            }
        ]
    },

    datagrid: {
        title: "DataGrid & ListView (Bảng Dữ Liệu Bóc Tách)",
        badge: "Basic Controls",
        desc: "Bảng dữ liệu kỹ thuật kết cấu hỗ trợ sọc vằn (Zebra Striping), cố định dòng, lựa chọn nhiều dòng và hỗ trợ trọn vẹn cả Dark/Light theme.",
        sections: [
            {
                title: "Ví Dụ XAML",
                desc: "Bảng thống kê khối lượng thép dầm:",
                xaml: `<DataGrid Style="{DynamicResource CONCOST.DataGrid.Base}"
          ItemsSource="{Binding RebarQuantityList}"
          AutoGenerateColumns="True" />`
            }
        ]
    },

    treeview: {
        title: "TreeView (Cây Phân Cấp Đối Tượng)",
        badge: "Basic Controls",
        desc: "Cây thư mục phân cấp dự án hiển thị các tầng, khu vực và từng cấu kiện nhỏ.",
        sections: [
            {
                title: "Ví Dụ XAML",
                desc: "Cây phân cấp tòa nhà:",
                xaml: `<TreeView Style="{DynamicResource CONCOST.TreeView.Base}">
    <TreeViewItem Header="Tòa Tháp A" IsExpanded="True">
        <TreeViewItem Header="Tầng 01 - Khối Đế"/>
        <TreeViewItem Header="Tầng 02 - Văn Phòng"/>
    </TreeViewItem>
</TreeView>`
            }
        ]
    },

    tabcontrol: {
        title: "TabControl (Thanh Tab Chuyển Phân Hệ)",
        badge: "Basic Controls",
        desc: "Bộ chuyển tab điều hướng nội dung với viền chỉ báo vị trí mượt mà (Underline indicator).",
        sections: [
            {
                title: "Ví Dụ XAML",
                desc: "Chuyển tab phân tích:",
                xaml: `<TabControl Style="{DynamicResource CONCOST.TabControl.Base}">
    <TabItem Header="Mô Hình 3D">
        <TextBlock Text="Không gian hiển thị 3D OpenCascade"/>
    </TabItem>
    <TabItem Header="Bảng Khối Lượng">
        <TextBlock Text="Dữ liệu bóc tách chi tiết"/>
    </TabItem>
</TabControl>`
            }
        ]
    },

    card: {
        title: "Card & Border & Badge (Khung Chứa & Huy Hiệu)",
        badge: "Basic Controls",
        desc: "Hệ thống thẻ chứa nội dung tiêu chuẩn hóa với độ bo góc `CornerRadius=10`, hiệu ứng bóng nổi `Depth2/Depth3` và huy hiệu thông báo.",
        sections: [
            {
                title: "Ví Dụ XAML",
                desc: "Thẻ tổng quan cấu kiện:",
                xaml: `<Border Background="{DynamicResource CONCOST.Brush.Surface}"
        BorderBrush="{DynamicResource CONCOST.Brush.Border}"
        BorderThickness="1" CornerRadius="10" Padding="18">
    <StackPanel>
        <TextBlock Text="Cột Vuông 800x800" FontWeight="Bold" FontSize="14"/>
        <TextBlock Text="Cường độ chịu nén: 35 MPa" Foreground="{DynamicResource CONCOST.Brush.TextSecondary}"/>
    </StackPanel>
</Border>`
            }
        ]
    },

    progressbar: {
        title: "ProgressBar & Slider & ScrollViewer",
        badge: "Basic Controls",
        desc: "Thanh tiến trình xác định/vô định, thanh trượt điều chỉnh giá trị liên tục và khung cuộn giao diện tự động ẩn thanh cuộn khi không rê chuột.",
        sections: [
            {
                title: "Ví Dụ XAML",
                desc: "Thanh tiến trình xuất báo cáo:",
                xaml: `<ProgressBar Value="65" Height="8" Maximum="100" Style="{DynamicResource CONCOST.ProgressBar.Base}" />`
            }
        ]
    },

    // -------------------------------------------------------------
    // ENGINEERING & CAD SUITE (FLAGSHIP)
    // -------------------------------------------------------------
    spreadsheet: {
        title: "Spreadsheet (Bảng Tính Excel Kỹ Thuật)",
        badge: "Engineering Flagship",
        desc: "Bảng tính Excel kỹ thuật hiệu năng cao, tự thiết kế renderer trực tiếp qua DrawingContext (hỗ trợ hàng chục ngàn cells mượt mà 60fps), công thức hàm tính toán toán học (=SUM, =AVERAGE, =COUNT), trộn ô (Merge Cells), định dạng tiền tệ/số thập phân, in-place cell editing và xuất nhập file CSV.",
        props: [
            { name: "ActiveSheet", type: "SpreadsheetSheet", default: "Sheet1", desc: "Trang tính hiện thời đang được chọn và hiển thị." },
            { name: "Sheets", type: "ObservableCollection<SpreadsheetSheet>", default: "Count=1", desc: "Danh sách tất cả các sheet trong bảng tính." },
            { name: "SelectionRange", type: "SpreadsheetRange", default: "(0,0)-(0,0)", desc: "Vùng các ô đang được bôi đen / chọn." },
            { name: "ActiveRow / ActiveColumn", type: "int", default: "0, 0", desc: "Tọa độ hàng và cột của ô tiêu điểm hiện thời." },
            { name: "IsReadOnly", type: "bool", default: "false", desc: "Khóa chế độ chỉ đọc, không cho phép chỉnh sửa cell." },
            { name: "ShowFormulaBar", type: "bool", default: "true", desc: "Bật / tắt thanh nhập công thức Excel fx trên đầu." },
            { name: "ShowToolbar", type: "bool", default: "true", desc: "Bật / tắt thanh công cụ định dạng (Bold, Italic, Căn lề, Màu nền, Merge)." },
            { name: "ShowSheetTabs", type: "bool", default: "true", desc: "Bật / tắt thanh tab danh sách sheet dưới đáy." },
            { name: "ShowStatusBar", type: "bool", default: "true", desc: "Bật / tắt thanh trạng thái tính toán thống kê (Count, Sum, Avg ô đang chọn)." }
        ],
        sections: [
            {
                title: "1. Khai Báo Trong XAML",
                desc: "Khai báo control Spreadsheet hoàn chỉnh giao diện Excel:",
                xaml: `<ui:Spreadsheet x:Name="MySpreadsheet"
                Height="500"
                ShowToolbar="True"
                ShowFormulaBar="True"
                ShowSheetTabs="True"
                ShowStatusBar="True" />`,
                csharp: `// Nạp dữ liệu và tính toán công thức bằng C#:
var sheet = MySpreadsheet.ActiveSheet;
sheet.RowCount = 50;
sheet.ColumnCount = 20;

// Gán tiêu đề cột
sheet.GetOrCreateCell(0, 0).RawValue = "Mã Hiệu";
sheet.GetOrCreateCell(0, 1).RawValue = "Tên Công Tác";
sheet.GetOrCreateCell(0, 2).RawValue = "Khối Lượng";
sheet.GetOrCreateCell(0, 3).RawValue = "Đơn Giá";
sheet.GetOrCreateCell(0, 4).RawValue = "Thành Tiền";

// Gán dữ liệu dòng 1
sheet.GetOrCreateCell(1, 0).RawValue = "AF.11111";
sheet.GetOrCreateCell(1, 1).RawValue = "Bê tông móng M250";
sheet.GetOrCreateCell(1, 2).RawValue = "150";
sheet.GetOrCreateCell(1, 3).RawValue = "1250000";
sheet.GetOrCreateCell(1, 4).Formula = "=C2*D2"; // Tự động nhân 150 * 1,250,000

// Tính tổng thành tiền
sheet.GetOrCreateCell(2, 3).RawValue = "Tổng Cộng:";
sheet.GetOrCreateCell(2, 3).IsBold = true;
sheet.GetOrCreateCell(2, 4).Formula = "=SUM(E2:E2)";
sheet.GetOrCreateCell(2, 4).IsBold = true;

MySpreadsheet.Recalculate();`
            },
            {
                title: "2. Xuất & Nhập Dữ Liệu CSV",
                desc: "Gọi các lệnh tích hợp sẵn để tương tác file dữ liệu ngoài:",
                xaml: `<StackPanel Orientation="Horizontal" Margin="0,0,0,8">
    <Button Content="Xuất CSV" Command="{Binding ExportCsvCommand, ElementName=MySpreadsheet}" 
            Style="{DynamicResource CONCOST.Button.Primary}" Margin="0,0,8,0"/>
    <Button Content="Nhập CSV" Command="{Binding ImportCsvCommand, ElementName=MySpreadsheet}" 
            Style="{DynamicResource CONCOST.Button.Secondary}"/>
</StackPanel>`
            },
            {
                title: "3. Trải Nghiệm Chuẩn Microsoft Excel 1:1 & Bộ Hàm Tiện Lợi",
                desc: "Các tính năng nhập liệu, thuật toán công thức & hiệu năng mượt mà chuẩn Enterprise:",
                xaml: `<!-- Trải nghiệm công thức chuẩn Microsoft Excel:
- Point-and-Click: Gõ dấu '=' rồi nhấp chuột vào ô khác để tự chèn tọa độ ô (e.g. '=A1+B1').
- Phím tắt F4: Chu trình cố định tọa độ tuyệt đối ($A$1 -> A$1 -> $A1 -> A1).
- Formula Intellisense AutoComplete: Gõ '=' và ký tự (e.g. '=S') tự động bật Popup gợi ý danh sách hàm (SUM, SUMIF...).
  Dùng phím mũi tên Lên/Xuống và Tab/Enter để tự động hoàn thành hàm.
- Multi-Color Formula Highlighting: Các ô và vùng tham chiếu trong công thức được viền đa màu (Xanh, Đỏ, Tím, Lục, Hổ Phách) như Excel.
- Auto-Fit Column/Row: Nhấp đúp chuột vào vạch phân cách giữa 2 tiêu đề cột/dòng để tự động co giãn vừa khít nội dung.
- Viewport Virtualization 60 FPS: Tối ưu hóa cắt tỉa vùng nhìn (Culling), cuộn hàng chục nghìn dòng siêu mượt mà.
- Hỗ trợ toán tử nối chuỗi '&' (e.g. ="Tổng: " & C2) và toán tử phần trăm '%' (e.g. =150*10%).
- Hỗ trợ tham chiếu tuyệt đối '$' khi kéo AutoFill (giữ nguyên hàng/cột bị khóa $).
- Thư viện 40+ hàm: SUM, AVERAGE, MIN, MAX, COUNT, COUNTA, PRODUCT, SUMIF, COUNTIF,
  VLOOKUP, HLOOKUP, MATCH, INDEX, IF, IFERROR, AND, OR, NOT, ROUND, ABS, SQRT, POWER, MOD,
  LEFT, RIGHT, MID, TRIM, UPPER, LOWER, LEN, CONCAT, TODAY, NOW, DATE, YEAR, MONTH, DAY,
  ISNUMBER, ISTEXT, ISBLANK. -->
<ui:Spreadsheet x:Name="MySpreadsheet" Height="550" />`
            }
        ]
    },

    stackedheaderdatagrid: {
        title: "StackedHeaderDataGrid (Bảng Dữ Liệu Header Đa Tầng & Chân Trang Tổng Hợp)",
        badge: "Engineering Flagship",
        desc: "DataGrid tiêu chuẩn xây dựng và dự toán khối lượng chuyên nghiệp, hỗ trợ cấu trúc cây tiêu đề cột gộp đa tầng (Multi-Level Stacked Header Tree), thanh gom nhóm kéo thả kéo cột (Group DropZone Panel) và tự động tính tổng (Sum, Avg, Count, Min, Max) tại chân trang cố định không bị trôi khi cuộn.",
        props: [
            { name: "ColumnHeaderTree", type: "ObservableCollection<ColumnNode>", default: "null", desc: "Cây định nghĩa phân cấp gom nhóm các tầng tiêu đề cột." },
            { name: "StackedHeaderRows", type: "ObservableCollection<StackedHeaderRow>", default: "null", desc: "Cấu hình gom cột theo phong cách danh sách hàng (Legacy Span)." },
            { name: "StackedRowHeight", type: "double", default: "28.0", desc: "Chiều cao của mỗi tầng tiêu đề cột phía trên." },
            { name: "CanUserSortColumns", type: "bool", default: "true", desc: "Cho phép nhấp vào cột lá để sắp xếp Tăng/Giảm dần." },
            { name: "DataGridGroupPanel", type: "DataGridGroupPanel", default: "null", desc: "Thanh gom nhóm dữ liệu kéo thả liên kết với DataGrid." }
        ],
        sections: [
            {
                title: "1. Khai Báo Cây Header Đa Tầng trong XAML",
                desc: "Định nghĩa phân nhóm cột 'Kích Thước Dầm (b x h)' và 'Vật Liệu (Bê tông, Cốt thép)':",
                xaml: `<StackPanel>
    <!-- Thanh gom nhóm cột thả tự do (DevExpress/Syncfusion style) -->
    <ui:DataGridGroupPanel DataGrid="{Binding ElementName=MyGrid}" />

    <!-- DataGrid tiêu chuẩn kỹ thuật -->
    <ui:StackedHeaderDataGrid x:Name="MyGrid"
                              AutoGenerateColumns="False"
                              CanUserSortColumns="True"
                              CanUserResizeColumns="True"
                              ItemsSource="{Binding BeamList}">
        <ui:StackedHeaderDataGrid.ColumnHeaderTree>
            <!-- Cột đơn lẻ -->
            <ui:ColumnLeaf HeaderText="Tên Cấu Kiện" BindingPath="BeamName" Width="140" CanUserSort="True"/>

            <!-- Cụm cột gộp tầng 1: Tiết Diện -->
            <ui:ColumnGroup HeaderText="Kích Thước Tiết Diện (mm)">
                <ui:ColumnLeaf HeaderText="Rộng b" BindingPath="Width" Width="90" AggregateType="Average" AggregateFormat="TB: {0:N0}"/>
                <ui:ColumnLeaf HeaderText="Cao h" BindingPath="Height" Width="90" AggregateType="Average" AggregateFormat="TB: {0:N0}"/>
            </ui:ColumnGroup>

            <!-- Cụm cột gộp tầng 2: Khối Lượng Dự Toán -->
            <ui:ColumnGroup HeaderText="Dự Toán Khối Lượng">
                <ui:ColumnLeaf HeaderText="V Bê Tông (m³)" BindingPath="ConcreteVolume" Width="110" AggregateType="Sum" AggregateFormat="Tổng: {0:N2}"/>
                <ui:ColumnLeaf HeaderText="As Thép (kg)" BindingPath="SteelWeight" Width="110" AggregateType="Sum" AggregateFormat="Tổng: {0:N1}"/>
            </ui:ColumnGroup>
        </ui:StackedHeaderDataGrid.ColumnHeaderTree>
    </ui:StackedHeaderDataGrid>
</StackPanel>`,
                csharp: `// Model cấu kiện trong ViewModel:
public class BeamItem
{
    public string BeamName { get; set; } = "D1";
    public double Width { get; set; } = 300;
    public double Height { get; set; } = 600;
    public double ConcreteVolume { get; set; } = 3.6;
    public double SteelWeight { get; set; } = 425.5;
}`
            }
        ]
    },

    cadcanvas: {
        title: "ZoomableCanvas & RulerControl (Không Gian Đồ Họa CAD 2D)",
        badge: "Engineering Flagship",
        desc: "Không gian vẽ kỹ thuật CAD 2D tương tác mượt mà bằng chuột: Thu phóng vô hạn (Zoom In/Out quanh con trỏ chuột), Kéo lia màn hình (Pan bằng chuột giữa hoặc chuột phải), Bật lưới tọa độ Grid tự co giãn theo tỷ lệ và đồng bộ thước đo mm/m chính xác với RulerControl.",
        props: [
            { name: "Scale", type: "double", default: "1.0", desc: "Hệ số phóng to thu nhỏ hiện thời của khung vẽ (0.01 đến 100)." },
            { name: "Offset", type: "Point", default: "(0,0)", desc: "Độ dời tọa độ gốc bản vẽ (Pan offset)." },
            { name: "ShowGrid", type: "bool", default: "true", desc: "Bật / tắt lưới ô vuông tọa độ CAD." },
            { name: "GridSize", type: "double", default: "20.0", desc: "Khoảng cách giữa các đường lưới chính." },
            { name: "RulerControl.MajorStep", type: "double", default: "100.0", desc: "Bước vạch chia số chính trên thước đo." }
        ],
        sections: [
            {
                title: "1. Cấu Trúc Viewport CAD Hoàn Chỉnh",
                desc: "Kết hợp RulerControl ngang dọc và ZoomableCanvas:",
                xaml: `<Grid Background="#1E1E1E">
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="24" />
        <ColumnDefinition Width="*" />
    </Grid.ColumnDefinitions>
    <Grid.RowDefinitions>
        <RowDefinition Height="24" />
        <RowDefinition Height="*" />
    </Grid.RowDefinitions>

    <!-- Thước đo tọa độ ngang -->
    <ui:RulerControl Grid.Row="0" Grid.Column="1" Orientation="Horizontal" MajorStep="50"
                     ZoomLevel="{Binding ElementName=CadCanvas, Path=Scale}"
                     ScrollOffset="{Binding ElementName=CadCanvas, Path=Offset.X}"/>

    <!-- Thước đo tọa độ dọc -->
    <ui:RulerControl Grid.Row="1" Grid.Column="0" Orientation="Vertical" MajorStep="50"
                     ZoomLevel="{Binding ElementName=CadCanvas, Path=Scale}"
                     ScrollOffset="{Binding ElementName=CadCanvas, Path=Offset.Y}"/>

    <!-- Vùng vẽ CAD tương tác -->
    <ui:ZoomableCanvas x:Name="CadCanvas" Grid.Row="1" Grid.Column="1"
                       ShowGrid="True" GridSize="20">
        <!-- Đối tượng hình học vẽ bằng Path / Shapes -->
        <Rectangle Canvas.Left="50" Canvas.Top="50" Width="200" Height="100" 
                   Stroke="#0284C7" StrokeThickness="2" Fill="#200284C7"/>
        <TextBlock Canvas.Left="60" Canvas.Top="60" Text="Cột C1 (200x100)" Foreground="White" />
    </ui:ZoomableCanvas>
</Grid>`
            }
        ]
    },

    diagramchart: {
        title: "DiagramChart (Biểu Đồ Nội Lực Dầm/Cột Kỹ Thuật)",
        badge: "Engineering Flagship",
        desc: "Control chuyên dụng vẽ biểu đồ phân bố nội lực kết cấu (Mô-men uốn M, Lực cắt Q, Lực dọc N, Độ võng dầm), tự động tính toán giá trị cực trị (Max/Min), tô màu miền diện tích âm/dương và hỗ trợ đảo chiều trục biểu đồ theo quy ước Việt Nam / TCVN (Mô-men căng thớ dưới vẽ xuống dưới).",
        props: [
            { name: "Points", type: "PointCollection", default: "null", desc: "Tập hợp các điểm tọa độ (X: chiều dài cấu kiện, Y: giá trị nội lực)." },
            { name: "Title", type: "string", default: "Diagram", desc: "Tên nhãn biểu đồ (vd: Biểu đồ Mô-men M)." },
            { name: "Unit", type: "string", default: "kN.m", desc: "Đơn vị đo nội lực (kN.m, kN, mm, MPa)." },
            { name: "InvertAxis", type: "bool", default: "false", desc: "Đảo chiều trục biểu đồ (thường bật True cho biểu đồ Moment)." }
        ],
        sections: [
            {
                title: "1. Khai Báo Biểu Đồ Nội Lực XAML",
                desc: "Vẽ biểu đồ nội lực Moment uốn có đảo trục:",
                xaml: `<ui:DiagramChart Title="Biểu đồ Mô-men Dầm B1"
                 Unit="kN.m"
                 InvertAxis="True"
                 Height="220"
                 Points="{Binding MomentPoints}" />`,
                csharp: `// Tạo dữ liệu biểu đồ parabol trong ViewModel:
public PointCollection MomentPoints { get; set; } = new PointCollection
{
    new Point(0.0, 0.0),
    new Point(1.0, 35.2),
    new Point(2.0, 58.4),
    new Point(3.0, 64.0), // Cực đại giữa nhịp
    new Point(4.0, 58.4),
    new Point(5.0, 35.2),
    new Point(6.0, 0.0)
};`
            }
        ]
    },

    rebarinput: {
        title: "RebarInput (Hộp Nhập Cốt Thép & Tự Tính Diện Tích As)",
        badge: "Engineering Flagship",
        desc: "Ô nhập liệu cốt thép thông minh dành riêng cho kỹ sư xây dựng. Tự động nhận diện cú pháp thép dầm/cột/móng (vd: 4D20, 2Ø16, 4Φ20, 2x3D18, D10@200), tự động tính toán tổng diện tích tiết diện cốt thép As (mm²) tức thời, và tự động cảnh báo viền đỏ khi nhập sai cú pháp.",
        props: [
            { name: "Text", type: "string", default: "4D20", desc: "Chuỗi cú pháp cốt thép (hỗ trợ các ký hiệu D, d, Ø, ø, Φ, φ, phi)." },
            { name: "AreaMm2", type: "double", default: "1256.6", desc: "Tổng diện tích cốt thép tính được theo đơn vị mm² (Read-Only TwoWay binding)." },
            { name: "IsValid", type: "bool", default: "true", desc: "Trạng thái hợp lệ của cú pháp vừa nhập." }
        ],
        sections: [
            {
                title: "1. Ví Dụ Sử Dụng XAML",
                desc: "Hộp nhập thép tiêu chuẩn kèm huy hiệu diện tích As tự động:",
                xaml: `<StackPanel>
    <TextBlock Text="Nhập cốt thép chủ dầm (vd: 4D20, 2Ø16, 2x3D18):" Margin="0,0,0,6"/>
    <eng:RebarInput Text="{Binding MainRebar, Mode=TwoWay}" Margin="0,0,0,16"/>

    <TextBlock Text="Nhập cốt đai (vd: D8@150, Ø10@200):" Margin="0,0,0,6"/>
    <eng:RebarInput Text="{Binding StirrupRebar, Mode=TwoWay}" Style="{DynamicResource CONCOST.RebarInput.Pill}"/>
</StackPanel>`,
                csharp: `// ViewModel lắng nghe diện tích:
private string _mainRebar = "4D20";
public string MainRebar
{
    get => _mainRebar;
    set { _mainRebar = value; OnPropertyChanged(); }
}`
            }
        ]
    },

    engineeringboxes: {
        title: "Engineering Boxes (Bộ Hộp Nhập Đại Lượng Kỹ Thuật)",
        badge: "Engineering Flagship",
        desc: "Bộ 6 hộp nhập đại lượng kỹ thuật chuyên sâu kế thừa từ UnitBox: LengthBox (mm, m), AreaBox (m²), VolumeBox (m³), ForceBox (kN), MomentBox (kN.m), PressureBox (MPa). Tích hợp nút xoay tăng giảm số, format số thập phân chuẩn xác và huy hiệu đơn vị gắn liền.",
        props: [
            { name: "Value", type: "double", default: "0.0", desc: "Giá trị số học đang nhập (TwoWay binding)." },
            { name: "Unit", type: "string", default: "mm / kN", desc: "Ký hiệu đơn vị hiển thị ở huy hiệu bên phải." },
            { name: "DecimalPlaces", type: "int", default: "1", desc: "Số chữ số phần thập phân hiển thị." },
            { name: "Header", type: "string", default: "string.Empty", desc: "Nhãn tiêu đề phía trên ô nhập." }
        ],
        sections: [
            {
                title: "1. Ví Dụ Sử Dụng Toàn Bộ 6 Hộp Nhập Kỹ Thuật",
                desc: "Form nhập thông số dầm kết cấu:",
                xaml: `<Grid>
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="*" />
        <ColumnDefinition Width="12" />
        <ColumnDefinition Width="*" />
    </Grid.ColumnDefinitions>
    <Grid.RowDefinitions>
        <RowDefinition Height="Auto" />
        <RowDefinition Height="12" />
        <RowDefinition Height="Auto" />
    </Grid.RowDefinitions>

    <!-- Hàng 1: Kích thước hình học -->
    <eng:LengthBox Grid.Row="0" Grid.Column="0" Value="{Binding BeamWidth}" Header="Chiều Rộng Dầm" />
    <eng:LengthBox Grid.Row="0" Grid.Column="2" Value="{Binding BeamHeight}" Header="Chiều Cao Dầm" />

    <!-- Hàng 2: Nội lực tính toán -->
    <eng:MomentBox Grid.Row="2" Grid.Column="0" Value="{Binding DesignMoment}" Header="Mô-men Uốn M" />
    <eng:ForceBox Grid.Row="2" Grid.Column="2" Value="{Binding DesignShear}" Header="Lực Cắt Q" />
</Grid>`
            }
        ]
    },

    compassindicator: {
        title: "CompassIndicator (La Bàn Chỉ Hướng Công Trình)",
        badge: "Engineering Flagship",
        desc: "La bàn đồ họa chỉ hướng gió và hướng Bắc địa chính (True North / Project North) của dự án công trình, tự động quay kim chỉ hướng theo góc Angle (0 đến 360 độ).",
        props: [
            { name: "Angle", type: "double", default: "0.0", desc: "Góc xoay hướng công trình tính bằng độ (Degrees)." }
        ],
        sections: [
            {
                title: "1. Ví Dụ XAML",
                desc: "Hiển thị hướng nhà xoay 45 độ Đông-Bắc:",
                xaml: `<ui:CompassIndicator Angle="45" Width="100" Height="100" />`
            }
        ]
    }
};
