// Guide pages added in the v3 documentation pass: FAQ + migration playbook.
const DOCS_GUIDES = {
    faq: {

        title: "FAQ & Troubleshooting",
        badge: "Hỗ trợ",
        desc: "Các sự cố thường gặp khi tích hợp CONCOST.UI vào ứng dụng WPF hoặc plugin CAD/BIM, kèm nguyên nhân gốc và cách xử lý.",
        keywords: ["lỗi", "error", "không hiển thị", "resource", "theme", "net48", "revit"],
        sections: [
            {
                title: "Style không áp dụng / control hiển thị trắng trơn",
                desc: "Hầu như luôn xuất phát từ việc thiếu dictionary hoặc dùng sai kiểu tham chiếu resource.",
                table: [
                    { token: "Thiếu Themes/ThemeResources.xaml", desc: "Chỉ <code>Light.xaml</code> thì các key <code>CONCOST.Spacing.*</code>, <code>CONCOST.FontSize.*</code>, <code>CONCOST.CornerRadius.*</code> không tồn tại → style không resolve." },
                    { token: "Thiếu Themes/Generic.xaml", desc: "Control vẫn render nhưng ở dạng mặc định của WPF: không màu, không bo góc, không animation." },
                    { token: "Dùng StaticResource cho style theme-aware", desc: "Khi đổi theme runtime, brush đã bị “đóng băng” vào control. Dùng <code>DynamicResource</code> cho mọi brush và style có thể đổi theo theme." },
                    { token: "Merge sau khi cửa sổ đã load", desc: "Nếu merge trong <code>Loaded</code>, control đã tạo template cũ. Merge trong <code>App.xaml</code> hoặc trước <code>InitializeComponent()</code>." }
                ],
                callout: { type: "tip", body: "Cách kiểm tra nhanh: mở cửa sổ Output của Visual Studio và tìm <code>System.Windows.ResourceReferenceKeyNotFoundException</code>. Key bị thiếu sẽ hiện ngay trong message." }
            },
            {
                title: "Plugin Revit / AutoCAD: giao diện bị “nhiễm” theme của host",
                desc: "Plugin DLL không có <code>Application.Current</code> riêng. Gán trực tiếp vào <code>Application.Current.Resources</code> sẽ ghi đè style của chính Revit/AutoCAD.",
                xaml: `<!-- ĐÚNG: scope tài nguyên vào root element của plugin -->
<UserControl x:Class="MyPlugin.SettingsPanel"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <UserControl.Resources>
        <ResourceDictionary>
            <ResourceDictionary.MergedDictionaries>
                <ResourceDictionary Source="pack://application:,,,/CONCOST.UI;component/Themes/ThemeResources.xaml" />
                <ResourceDictionary Source="pack://application:,,,/CONCOST.UI;component/Themes/Generic.xaml" />
            </ResourceDictionary.MergedDictionaries>
        </ResourceDictionary>
    </UserControl.Resources>
</UserControl>`,
                csharp: `// SettingsPanel.xaml.cs
using System.Windows.Controls;
using System.Windows.Media;
using CONCOST.UI.Themes;

public partial class SettingsPanel : UserControl
{
    public SettingsPanel()
    {
        InitializeComponent();

        // Áp theme cho riêng view này, không ảnh hưởng host
        ThemeManager.Current.ApplyTheme(this, AppTheme.Dark);
        ThemeManager.Current.ApplyAccentColor(this, Color.FromRgb(0xFF, 0x79, 0x00));
    }
}`,
                callout: { type: "warn", title: "Không dùng App.xaml trong class library", body: "Thư viện plugin không có entry point. Nếu cần một điểm khởi tạo chung, tạo <code>static class ThemeBootstrap.Init(FrameworkElement root)</code> và gọi từ constructor của mỗi view." }
            },
            {
                title: "Lỗi build & đóng gói theo target framework",
                desc: "Ba target của thư viện có dependency khác nhau — nguyên nhân phổ biến của lỗi runtime chỉ xuất hiện trên máy khách.",
                table: [
                    { token: "net48 + System.Text.Json", desc: "Bản <code>net48</code> dùng <code>System.Text.Json</code>; khi copy DLL thủ công phải mang theo các assembly phụ thuộc trong thư mục build output." },
                    { token: "Revit 2021–2024", desc: "Chạy trên .NET Framework 4.8 → tham chiếu bản <code>net48</code>. Revit 2025+ dùng .NET 8 → chọn <code>net8.0-windows</code>." },
                    { token: "AutoCAD 2025+", desc: "Dùng .NET 8. Kiểm tra lại tài liệu của đúng phiên bản host trước khi chọn assembly." },
                    { token: "dotnet build báo thiếu targeting pack", desc: "Cài .NET Framework 4.8 Developer Pack, hoặc build riêng <code>-f net8.0-windows</code> nếu không cần <code>net48</code>." }
                ],
                callout: { type: "info", body: "Dùng <code>dotnet build CONCOST.UI.sln -c Release</code> để build cả ba target; <code>ProjectReference</code> sẽ tự chọn target khớp với host." }
            },
            {
                title: "Hiệu năng khi hiển thị dữ liệu lớn",
                desc: "Các control dữ liệu đều bật virtualization, nhưng một vài cấu hình XAML vô tình tắt nó.",
                table: [
                    { token: "ScrollViewer bao ngoài DataGrid", desc: "ScrollViewer cấp vô hạn chiều cao → virtualization bị tắt, 10.000 dòng sẽ tạo đủ 10.000 container. Bỏ ScrollViewer, để DataGrid tự cuộn." },
                    { token: "StackPanel thay vì ItemsPanel ảo hoá", desc: "Với <code>ListBox</code>/<code>ListView</code>, giữ <code>VirtualizingStackPanel</code> mặc định; chỉ đổi sang WrapPanel khi số item nhỏ." },
                    { token: "Spreadsheet quá nhiều sheet active", desc: "Chỉ giữ một <code>ActiveSheet</code>; mỗi sheet giữ lưới riêng trong bộ nhớ." },
                    { token: "Hiệu ứng nặng trên item template", desc: "<code>Ripple</code>, <code>Skeleton</code> và shadow (<code>VisualElement.Elevation</code>) nhân theo số item — tắt ripple trong item template khi list dài." }
                ]
            }
        ],
        related: ["intro", "class-library", "theming", "migration"]
    },

    migration: {

        title: "Chuyển từ WinForms / WPF sang CONCOST.UI",
        badge: "Hướng dẫn",
        desc: "Bảng ánh xạ từ WinForms / WPF mặc định sang CONCOST.UI, kèm quy trình chuyển đổi 4 bước và chính sách deprecation.",
        keywords: ["migrate", "winforms", "chuyển đổi", "mapping", "breaking change"],
        sections: [
            {
                title: "Ánh xạ control",
                desc: "Cột phải là style key hoặc control thật trong thư viện — không phải tên gợi ý.",
                table: [
                    { token: "<code>Button</code> → <code>CONCOST.Button.Primary</code>", desc: "23 style variants: Primary, Secondary, Success, Warning, Danger, Info, Outline.*, Icon.*, *.Light." },
                    { token: "<code>TextBox</code> số → <code>ui:NumericUpDown</code>", desc: "Có Minimum/Maximum/Increment/DecimalPlaces/Suffix, hỗ trợ phím mũi tên và wheel." },
                    { token: "<code>TextBox</code> đơn vị → <code>ui:UnitBox</code>", desc: "Hiển thị đơn vị và số chữ số thập phân thống nhất toàn ứng dụng." },
                    { token: "<code>TextBox</code> mật khẩu → <code>ui:PasswordInput</code>", desc: "Có nút hiện/ẩn, icon khoá, placeholder." },
                    { token: "<code>DataGridView</code> → <code>DataGrid</code> + <code>CONCOST.DataGrid.Base</code>", desc: "Zebra row, theme-aware, kèm attached <code>DataGridElement.RowSpacing</code>." },
                    { token: "Header đa tầng tự vẽ → <code>ui:StackedHeaderDataGrid</code>", desc: "Khai báo <code>StackedColumn</code> lồng nhau, footer tổng hợp tự động." },
                    { token: "Panel vẽ CAD → <code>canvas:ZoomableCanvas</code> + <code>canvas:RulerControl</code>", desc: "Zoom/pan vô hạn, lưới theo mm, thước đo đồng bộ." },
                    { token: "MessageBox → <code>ui:Dialog</code> / <code>Growl</code>", desc: "Dialog có mask, token đóng theo key; Growl cho toast không chặn." },
                    { token: "ToolTip lỗi → <code>attached:InfoElement.*</code>", desc: "Title, Placeholder, Required, Prefix/Suffix, ShowClearButton." }
                ]
            },
            {
                title: "Quy trình chuyển đổi 4 bước",
                desc: "Thứ tự này giữ cho ứng dụng luôn build được trong suốt quá trình chuyển.",
                xaml: `<!-- Bước 2: thay control, giữ nguyên binding -->
<!-- Trước -->
<TextBox Text="{Binding BeamWidth}" Width="120" />

<!-- Sau -->
<ui:UnitBox Header="Beam width"
            Value="{Binding BeamWidth, Mode=TwoWay}"
            Unit="mm"
            DecimalPlaces="0" />`,
                csharp: `// Bước 3: bật theme một lần cho toàn app
using System.Windows;
using CONCOST.UI.Themes;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        ThemeManager.Current.Theme = AppTheme.System;
        ThemeManager.Current.Preset = ThemePreset.Default;
    }
}

// Bước 4: thay MessageBox bằng Dialog/Growl service
// Growl.Toast("Saved", GrowlType.Success);  // không chặn UI
// Dialog.Show("Xác nhận xoá?", DialogType.Question);`,
                callout: { type: "tip", body: "<b>1.</b> Thêm reference + merge dictionary. <b>2.</b> Thay từng control theo bảng ánh xạ. <b>3.</b> Bật ThemeManager. <b>4.</b> Thay MessageBox/MessageBox.Show bằng Dialog hoặc Growl." }
            },
            {
                title: "Chính sách breaking change",
                desc: "Áp dụng semantic versioning cho toàn bộ assembly.",
                table: [
                    { token: "Deprecation", desc: "API bị thay thế được đánh dấu <code>[Obsolete]</code> ít nhất hai minor version trước khi xoá." },
                    { token: "Major release", desc: "Chỉ major version mới được phép breaking change, kèm ví dụ migrate trong <code>CHANGELOG.md</code>." },
                    { token: "Theme key", desc: "Key resource (<code>CONCOST.*</code>) được xem là public API — đổi tên key cũng là breaking change." }
                ]
            }
        ],
        related: ["intro", "theming", "attached-props", "faq"]
    }
};
