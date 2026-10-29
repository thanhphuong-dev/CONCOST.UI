# CONCOST.UI

**Thư viện điều khiển WPF đa target dành cho ứng dụng kỹ thuật, CAD, BIM và desktop.**

[English](README.md) · [Tiếng Việt](README.vi.md)

![Frameworks](https://img.shields.io/badge/frameworks-.NET%20Framework%204.8%20%7C%20.NET%208%20%7C%20.NET%2010-512BD4)
![Platform](https://img.shields.io/badge/platform-Windows%20%7C%20WPF-0078D4)
![License](https://img.shields.io/badge/license-MIT-2ea44f)
[![Wiki](https://img.shields.io/badge/wiki-online-ff7900)](https://thanhphuong-dev.github.io/CONCOST.UI/)
[![Release](https://img.shields.io/github/v/release/thanhphuong-dev/CONCOST.UI?label=release&color=ff7900)](https://github.com/thanhphuong-dev/CONCOST.UI/releases/latest)

CONCOST.UI cung cấp các control WPF có thể tái sử dụng, tài nguyên giao diện, biểu tượng và thành phần chuyên dụng cho quy trình kỹ thuật. Thư viện phục vụ cả ứng dụng WPF độc lập lẫn giao diện WPF chạy bên trong các phần mềm như Revit và AutoCAD.

**Tài liệu:** [thanhphuong-dev.github.io/CONCOST.UI](https://thanhphuong-dev.github.io/CONCOST.UI/) — 79 trang, song ngữ Việt / Anh, có tìm kiếm.

## Điểm nổi bật

- **Ba target framework:** .NET Framework 4.8, .NET 8 và .NET 10. Chọn bản build phù hợp với ứng dụng chủ.
- **Bộ control WPF phong phú:** bảng dữ liệu, Spreadsheet, điều hướng, nhập liệu, dashboard, biểu đồ, công cụ kỹ thuật và hiệu ứng giao diện.
- **Giao diện nhất quán:** theme sáng/tối, màu nhấn, bộ preset và design token dùng chung.
- **Hỗ trợ giao diện plugin:** áp dụng tài nguyên theme trong phạm vi view của plugin, không cần tạo `Application` WPF riêng hoặc ghi đè tài nguyên toàn cục của ứng dụng chủ.
- **Ví dụ tương tác:** mở Showcase để trải nghiệm control, đổi theme và xem ví dụ tích hợp.

## Tương thích framework

| Bản build thư viện | Ứng dụng chủ phù hợp |
| --- | --- |
| `net48` | Ứng dụng và plugin WPF target .NET Framework 4.8 |
| `net8.0-windows` | Ứng dụng và plugin WPF target .NET 8 |
| `net10.0-windows` | Ứng dụng và plugin WPF target .NET 10 |

Yêu cầu runtime của Revit và AutoCAD thay đổi theo từng phiên bản. Hãy kiểm tra framework mà phiên bản ứng dụng chủ yêu cầu, sau đó tham chiếu đúng `CONCOST.UI.dll`. Khi framework tương thích, Project Reference sẽ tự chọn target phù hợp.

## Bắt đầu nhanh

### Thêm Project Reference

```xml
<ItemGroup>
  <ProjectReference Include="path/to/CONCOST.UI/src/CONCOST.UI/CONCOST.UI.csproj" />
</ItemGroup>
```

Hoặc tham chiếu assembly đã build tương ứng với framework của ứng dụng chủ:

```text
src/CONCOST.UI/bin/Release/net48/CONCOST.UI.dll
src/CONCOST.UI/bin/Release/net8.0-windows/CONCOST.UI.dll
src/CONCOST.UI/bin/Release/net10.0-windows/CONCOST.UI.dll
```

Thư viện target Windows vì WPF chỉ hỗ trợ Windows. Ứng dụng chủ cần có runtime và framework WPF tương ứng. Bản `net48` sử dụng `System.Text.Json` cùng các thư viện phụ thuộc của nó; khi phân phối target này, hãy dùng gói NuGet hoặc cung cấp kèm các dependency trong thư mục build output.

### Nạp resource theme

Với ứng dụng WPF độc lập, thêm các ResourceDictionary của thư viện vào `App.xaml`:

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

### Sử dụng control

```xml
<Window
    x:Class="MyApplication.MainWindow"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:ui="clr-namespace:CONCOST.UI.Controls;assembly=CONCOST.UI">

    <StackPanel Margin="24">
        <ui:UnitBox Header="Beam width"
                    Value="300"
                    Unit="mm"
                    DecimalPlaces="0" />

        <ui:NumericUpDown Value="25"
                          Minimum="0"
                          Maximum="1000"
                          Increment="5"
                          Suffix=" kN" />
    </StackPanel>
</Window>
```

### Đổi theme khi ứng dụng đang chạy

```csharp
using System.Windows.Media;
using CONCOST.UI.Themes;

ThemeManager.Current.Theme = AppTheme.Dark;
ThemeManager.Current.Preset = ThemePreset.Ocean;
ThemeManager.Current.AccentColor = Color.FromRgb(2, 132, 199);
```

Các preset hiện có gồm `Default`, `Forest`, `Sunset`, `Slate`, `Midnight` và `HighContrast`.

## Tích hợp với plugin Revit hoặc AutoCAD

Class Library của plugin không cần `App.xaml` hoặc một đối tượng `Application` riêng. Hãy áp dụng theme lên `UserControl` hoặc Window gốc của plugin để tài nguyên được giới hạn trong view đó:

```csharp
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

    private void UseDarkTheme()
    {
        ThemeManager.Current.ApplyTheme(this, AppTheme.Dark);
    }
}
```

API áp dụng theme theo view cũng hỗ trợ đổi theme lúc chạy và không phụ thuộc `Application.Current`. Xem [hướng dẫn Getting Started](docs/GettingStarted.md) để biết thêm chi tiết tích hợp.

## Build, chạy và kiểm tra

### Yêu cầu

- Windows 10 trở lên
- .NET 8 SDK và .NET 10 SDK
- Visual Studio 2022 với workload **.NET desktop development** (không bắt buộc)
- Reference assemblies của .NET Framework 4.8 được tự động restore khi build target `net48`

### Build solution

```powershell
dotnet restore CONCOST.UI.sln
dotnet build CONCOST.UI.sln --configuration Release
```

### Chạy Showcase

```powershell
dotnet run --project samples/CONCOST.UI.Showcase/CONCOST.UI.Showcase.csproj
```

### Chạy bộ kiểm thử

```powershell
dotnet test tests/CONCOST.UI.Tests/CONCOST.UI.Tests.csproj --configuration Release
```

## Cấu trúc repository

```text
.
├── .github/                          # Quy trình CI và mẫu đóng góp
├── docs/                             # Wiki tài liệu — nguồn của GitHub Pages
│   ├── index.html · app.js · style.css   # Khung site, renderer, design system
│   ├── data/                         # Nội dung trang (VI) + bảng API sinh tự động
│   │   └── en/                       # Lớp tiếng Anh cho cả 79 trang
│   ├── tools/                        # build-props.py · check-xaml.py
│   ├── GettingStarted.md
│   ├── MigrationGuide.md
│   └── roadmap/                      # Roadmap hiện hành (v3) + kế hoạch lưu trữ
├── samples/
│   └── CONCOST.UI.Showcase/          # Thư viện trình diễn control tương tác
├── src/
│   └── CONCOST.UI/                   # Thư viện WPF đa target
├── tests/
│   └── CONCOST.UI.Tests/             # Project kiểm thử tự động
├── CHANGELOG.md
├── CONCOST.UI.sln
├── Directory.Build.props
├── README.md
└── README.vi.md
```

Solution có một project thư viện. Các namespace như `CONCOST.UI.Core` và `CONCOST.UI.Themes` chỉ dùng để tổ chức mã nguồn bên trong assembly, không phải dependency DLL riêng.

## Tài liệu

Toàn bộ tài liệu tham chiếu là một wiki tĩnh tương tác gồm 79 trang, có tìm kiếm (`Ctrl K`), code inspector XAML/C# và theme sáng/tối — mở [`docs/index.html`](docs/index.html).

- [Mục lục tài liệu](docs/README.md) — bản đồ trang và quy trình cập nhật
- [Hướng dẫn bắt đầu](docs/GettingStarted.md) — tích hợp, theme, plugin host, build
- [Hướng dẫn chuyển đổi phiên bản](docs/MigrationGuide.md)
- [Tài liệu control tương tác](docs/index.html) — song ngữ Việt / Anh, thêm `?lang=en` để mở bằng tiếng Anh
- **Wiki online:** <https://thanhphuong-dev.github.io/CONCOST.UI/>
- [Lịch sử phiên bản](CHANGELOG.md)
- [Control Showcase](samples/CONCOST.UI.Showcase/)
- [Hướng dẫn đóng góp](CONTRIBUTING.md)

Bảng thuộc tính và snippet C# được sinh trực tiếp từ source: chạy `python docs/tools/build-props.py` sau khi thêm/đổi tên `DependencyProperty`, và `python docs/tools/check-xaml.py` để kiểm tra mọi snippet còn khớp API thật.

## Đóng góp

Chúng tôi hoan nghênh báo lỗi, đề xuất tính năng và pull request. Vui lòng đọc [CONTRIBUTING.md](CONTRIBUTING.md) trước khi đóng góp. Control mới nên có theme resource phù hợp, ví dụ trong Showcase và kiểm thử khi khả thi.

## Giấy phép

CONCOST.UI được phát hành theo [giấy phép MIT](LICENSE).
