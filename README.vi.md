<div align="center">
  <img src="docs/assets/banner.png" width="100%" alt="CONCOST.UI — WPF control cho người làm sản phẩm kỹ thuật">

  <p>
    <a href="https://github.com/thanhphuong-dev/CONCOST.UI/releases/latest"><img src="https://img.shields.io/github/v/release/thanhphuong-dev/CONCOST.UI?label=release&color=ff7900" alt="Bản phát hành mới nhất"></a>
    <img src="https://img.shields.io/badge/.NET-4.8%20%7C%208%20%7C%2010-512BD4" alt=".NET Framework 4.8, .NET 8, .NET 10">
    <img src="https://img.shields.io/badge/platform-Windows%20%C2%B7%20WPF-0078D4" alt="Windows và WPF">
    <img src="https://img.shields.io/badge/license-MIT-2ea44f" alt="Giấy phép MIT">
    <img src="https://img.shields.io/badge/docs-106%20trang-ff7900" alt="106 trang tài liệu">
  </p>

  <p>
    <a href="https://thanhphuong-dev.github.io/CONCOST.UI/"><b>Tài liệu</b></a> &nbsp;·&nbsp;
    <a href="docs/GettingStarted.vi.md">Bắt đầu</a> &nbsp;·&nbsp;
    <a href="CHANGELOG.md">Nhật ký thay đổi</a> &nbsp;·&nbsp;
    <a href="https://github.com/thanhphuong-dev/CONCOST.UI/releases">Tải về</a> &nbsp;·&nbsp;
    <a href="README.md">English</a>
  </p>
</div>

---

**CONCOST.UI là thư viện component WPF cho những phần mềm có vẽ** — trình biên tập CAD, công cụ BIM, bóc tách khối lượng, báo cáo kết cấu. Thư viện có đúng những control mà sản phẩm kỹ thuật cần: bảng tính, canvas zoom được kèm thước, nhập thép, dashboard KPI, hơn 20 hiệu ứng thị giác, và hệ theme chạy được **bên trong plugin Revit / AutoCAD** mà không chiếm tài nguyên của ứng dụng chủ.

## Vì sao nên dùng

- **Ba target framework, một API.** `net48`, `net8.0-windows`, `net10.0-windows` từ cùng một source. Tham chiếu project là MSBuild tự chọn bản khớp, nên cùng một view vừa build được vào plugin AutoCAD .NET Framework 4.8 vừa chạy trong app desktop .NET 8.
- **Theme tôn trọng ứng dụng chủ.** `ThemeManager.ApplyTheme(this, …)` giới hạn light/dark, accent và preset trong **một view** — không cần `Application`, không ghi đè resource toàn cục. Đây chính là điều kiện để dùng được trong add-in Revit.
- **Hiệu ứng nhẹ.** Mọi animation đều vẽ bằng `DrawingContext` hoặc transform — không shader, không `BlurEffect`, không tạo visual tree cho từng item — và tất cả đều tôn trọng thiết lập **"Show animations"** của Windows (WCAG 2.2.2).
- **Tài liệu sinh từ source.** Bảng thuộc tính và snippet C# được sinh từ chính khai báo `DependencyProperty` rồi đối chiếu với API đã biên dịch, nên tài liệu không thể lệch khỏi code.

## Trong thư viện có gì

| Nhóm | Điểm chính |
| --- | --- |
| **Kỹ thuật & CAD** | `Spreadsheet` có engine công thức, `StackedHeaderDataGrid`, `ZoomableCanvas` + `Ruler`, biểu đồ nội lực, `RebarInput`, `CompassIndicator` |
| **Dữ liệu & dashboard** | `StatisticCard`, `OdometerNumber`, `Sparkline`, `HeatmapCalendar`, `AvatarStack`, `Timeline`, `CardCarousel`, `DataPager`, `PropertyGrid` |
| **Nhập liệu & form** | 15 control — `UnitBox` (giá trị + đơn vị + số lẻ), `NumericUpDown`, `OtpInput`, `PinBox`, `CheckComboBox`, `AutoCompleteTextBox`, `ColorPicker`, `RangeSlider`, `Rating`, `ToggleSwitch` |
| **Điều hướng & khung** | `NavigationView`, `SideMenu`, `DrawerMenu`, `Breadcrumb`, `StepBar`, `Pagination`, `Transfer`, `TabControl`, `SplitButton`, `CONCOSTWindow` |
| **Phản hồi & trạng thái** | `Dialog`, `Drawer`, toast `Growl`, loading indicator và overlay, `Skeleton` shimmer, `EmptyState`, `Watermark` |
| **Hiệu ứng thị giác** | `SpotlightCard`, `TiltCard`, `MagneticHover`, `AuroraBackground`, `BorderBeam`, `ConfettiBurst`, `TypewriterText`, `ScrollReveal`, `ParallaxPanel`, `PulseRing`, `GradientText`… |
| **Theme** | Light / dark, 8 preset, 5 màu accent, design token dùng chung, 3 kiểu surface |

## Tương thích framework

| Bản build | Dùng cho |
| --- | --- |
| `net48` | App desktop .NET Framework 4.8 và plugin Revit / AutoCAD chạy trên runtime đó |
| `net8.0-windows` | App desktop .NET 8 và plugin .NET 8 |
| `net10.0-windows` | App desktop .NET 10 và plugin .NET 10 |

Yêu cầu runtime của Revit và AutoCAD khác nhau theo phiên bản. Kiểm tra yêu cầu của đúng phiên bản host bạn dùng, rồi tham chiếu `CONCOST.UI.dll` tương ứng.

## Cài đặt

### A. Gói NuGet từ bản phát hành

Tải `CONCOST.UI.1.0.1.nupkg` ở [bản mới nhất](https://github.com/thanhphuong-dev/CONCOST.UI/releases/latest), sau đó:

```powershell
dotnet nuget add source "C:\duong\dan\tai\ve" --name concost-local
dotnet add package CONCOST.UI --version 1.0.1
```

### B. Tham chiếu project

```xml
<ItemGroup>
  <ProjectReference Include="path/to/CONCOST.UI/src/CONCOST.UI/CONCOST.UI.csproj" />
</ItemGroup>
```

### C. Dùng DLL build sẵn

```text
src/CONCOST.UI/bin/Release/net48/CONCOST.UI.dll
src/CONCOST.UI/bin/Release/net8.0-windows/CONCOST.UI.dll
src/CONCOST.UI/bin/Release/net10.0-windows/CONCOST.UI.dll
```

> Bản `net48` phụ thuộc `System.Text.Json` và các dependency đi kèm. Nên dùng gói NuGet, hoặc copy đủ file từ thư mục build output ra cạnh DLL.

## Bắt đầu nhanh

### 1. Gộp resource dictionary của theme

```xml
<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <ResourceDictionary Source="pack://application:,,,/CONCOST.UI;component/Themes/ThemeResources.xaml" />
            <ResourceDictionary Source="pack://application:,,,/CONCOST.UI;component/Themes/Generic.xaml" />
        </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
</Application.Resources>
```

### 2. Đặt control vào view

```xml
<Window x:Class="MyApplication.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:ui="clr-namespace:CONCOST.UI.Controls;assembly=CONCOST.UI">

    <StackPanel Margin="24">
        <ui:UnitBox Header="Bề rộng dầm" Value="300" Unit="mm" DecimalPlaces="0" />

        <ui:NumericUpDown Value="25" Minimum="0" Maximum="1000" Increment="5" Suffix=" kN" />

        <ui:SpotlightCard Margin="0,16,0,0">
            <StackPanel>
                <TextBlock Text="Khối lượng bê tông" FontSize="12" Opacity=".65" />
                <ui:OdometerNumber Value="1284.6" Format="N1" Suffix=" m³" />
                <ui:Sparkline Values="{Binding WeeklyPour}" Height="40" ShowArea="True" />
            </StackPanel>
        </ui:SpotlightCard>
    </StackPanel>
</Window>
```

### 3. Đổi theme lúc chạy

```csharp
using System.Windows.Media;
using CONCOST.UI.Themes;

ThemeManager.Current.Theme = AppTheme.Dark;
ThemeManager.Current.Preset = ThemePreset.Ocean;
ThemeManager.Current.AccentColor = Color.FromRgb(2, 132, 199);
```

## Dùng trong plugin Revit / AutoCAD

Class library của plugin không có `App.xaml` và không được đụng vào resource toàn cục của host. Hãy áp theme lên **view gốc** của plugin — resource chỉ tồn tại trong `UserControl` hoặc `Window` đó:

```csharp
using System.Windows.Controls;
using System.Windows.Media;
using CONCOST.UI.Themes;

public partial class SettingsPanel : UserControl
{
    public SettingsPanel()
    {
        InitializeComponent();
        ThemeManager.Current.ApplyTheme(this, AppTheme.Light);
        ThemeManager.Current.ApplyAccentColor(this, Color.FromRgb(2, 132, 199));
    }
}
```

Cách này chạy được khi **không có** `Application.Current`, và vẫn đổi theme được lúc runtime. Xem [hướng dẫn Getting Started](docs/GettingStarted.vi.md) để biết chi tiết tích hợp.

## Theme

| Preset | Đặc điểm |
| --- | --- |
| `Default` | Surface trung tính, accent cam — giao diện mặc định |
| `Ocean` | Tông xanh lạnh, điềm và "kỹ thuật" |
| `Forest` | Xanh lá dịu, đỡ mỏi mắt khi làm lâu |
| `Sunset` | Hổ phách ấm, hợp màn hình thuyết trình |
| `Slate` | Xám ít bão hoà, gần với CAD |
| `Midnight` | Xanh đêm sâu, dark có tương phản cao |
| `HighContrast` | Ưu tiên khả năng đọc, dễ tiếp cận |
| `Brutal` | Neo-brutalism: nền kem, viền mực, accent vàng điện |

Đổi preset rồi quay lại preset cũ thì màu được **khôi phục nguyên trạng** — ThemeManager lưu lại những gì nó ghi đè.

## Hiệu ứng thị giác

Tất cả đều vẽ trực tiếp nên gần như không tốn tài nguyên và vẫn sắc nét ở mọi mức DPI.

| Nhóm | Control |
| --- | --- |
| **Theo con trỏ** | `SpotlightCard`, `CursorGlow`, `TiltCard`, `MagneticHover`, `Ripple`, `Magnifier` |
| **Chuyển động & hiện chữ** | `ScrollReveal`, `ParallaxPanel`, `TypewriterText`, `TextReveal`, `CheckMarkDraw`, `ScrollProgressBar`, `MarqueeTicker` |
| **Nền & không gian** | `AuroraBackground`, `GridPattern`, `WaveDivider`, `SnowCanvas`, `Sprite` |
| **Nhấn & chúc mừng** | `BorderBeam`, `SheenSweep`, `PulseRing`, `GradientText`, `ConfettiBurst` |

`MotionSettings` là công tắc chung của tất cả. Mặc định nó đọc thiết lập *Show animations* của Windows, tắt được toàn app bằng `MotionSettings.ForceDisabled`, và mỗi hiệu ứng sẽ **chọn một khung tĩnh** thay vì đứng hình giữa animation.

## Tài liệu

[![Xem trước tài liệu](docs/assets/docs-preview.png)](https://thanhphuong-dev.github.io/CONCOST.UI/)

Tài liệu là một wiki tĩnh — **106 trang**, song ngữ **Việt / Anh**, tìm kiếm bằng `Ctrl K`, mỗi trang có code inspector XAML ↔ C#. Chạy **offline hoàn toàn**: font, icon và syntax highlight đều tự host, không CDN.

- **Bản online:** <https://thanhphuong-dev.github.io/CONCOST.UI/>
- [Mục lục tài liệu](docs/README.md) — bản đồ trang và quy trình bảo trì
- [Getting Started](docs/GettingStarted.vi.md) — tích hợp, theme, plugin hosting, build
- [Migration Guide](docs/MigrationGuide.vi.md) — ánh xạ WinForms / WPF và chính sách deprecation
- [Roadmap hiệu ứng](docs/roadmap/effects-roadmap.md) — kế hoạch tiếp theo
- [Nhật ký thay đổi](CHANGELOG.md)

## Build từ source

**Yêu cầu:** Windows 10+, .NET 8 SDK, .NET 10 SDK. Reference assembly của .NET Framework 4.8 được restore tự động cho target `net48`. Visual Studio 2022 với workload *.NET desktop development* là tuỳ chọn.

```powershell
dotnet restore CONCOST.UI.sln
dotnet build   CONCOST.UI.sln --configuration Release
dotnet test    tests/CONCOST.UI.Tests/CONCOST.UI.Tests.csproj --configuration Release
dotnet run     --project samples/CONCOST.UI.Showcase/CONCOST.UI.Showcase.csproj
```

App Showcase là cách nhanh nhất để xem toàn bộ: duyệt theo nhóm, đổi theme và màu accent, bấm **View XAML** / **View C#** ở bất kỳ demo nào để copy đúng đoạn code đang chạy.

### Giữ tài liệu khớp với code

```powershell
python docs/tools/build-props.py     # sinh lại bảng thuộc tính từ khai báo DependencyProperty
python docs/tools/check-xaml.py      # kiểm tra mọi snippet XAML còn đúng API thật
node   docs/tools/build-index.js     # dựng lại bản đồ trang và map trang → file nguồn
```

## Cấu trúc repo

```text
.
├── docs/                        Wiki tài liệu — nguồn của GitHub Pages
│   ├── index.html · app.js · style.css     khung site, renderer, design system
│   ├── data/                    nội dung trang (VI) + bảng API sinh tự động
│   │   └── en/                  lớp tiếng Anh cho mọi trang
│   ├── assets/                  banner và ảnh chụp dùng trong README này
│   ├── tools/                   build-props.py · check-xaml.py · build-index.js · fetch-fonts.py · banner.html
│   ├── vendor/                  font, Font Awesome, Prism tự host (build offline)
│   └── roadmap/                 roadmap hiện hành và kế hoạch lưu trữ
├── samples/CONCOST.UI.Showcase/ Bộ sưu tập control tương tác
├── src/CONCOST.UI/              Thư viện WPF đa target
├── tests/CONCOST.UI.Tests/      Project test tự động
├── artifacts/                   gói phát hành và bản build demo (đã git-ignore)
├── CHANGELOG.md · CONTRIBUTING.md · Directory.Build.props · CONCOST.UI.sln
└── README.md · README.vi.md
```

Solution chỉ có **một** project thư viện. Các namespace như `CONCOST.UI.Core`, `CONCOST.UI.Themes`, `CONCOST.UI.Controls` chỉ tổ chức code trong assembly đó — không phải DLL riêng.

## Đóng góp

Báo lỗi, đề xuất tính năng và pull request đều được hoan nghênh — vui lòng đọc [CONTRIBUTING.md](CONTRIBUTING.md) trước. Một control mới cần có: resource theme, demo trong Showcase (kèm panel XAML và C#), tài liệu cả hai ngôn ngữ, và test nếu hợp lý.

## Giấy phép

[MIT](LICENSE) — dùng được cho cả sản phẩm thương mại và mã nguồn mở.
