# Bắt đầu với CONCOST.UI

[English](GettingStarted.md) · **Tiếng Việt**

CONCOST.UI là thư viện WPF đa target (`net48`, `net8.0-windows`, `net10.0-windows`), cung cấp control, design token và các style variant cho ứng dụng desktop kỹ thuật, CAD và BIM.

Tài liệu đầy đủ và tra cứu được cho từng control (thuộc tính, style key, mẫu XAML và C#) nằm trong wiki tương tác: mở [`docs/index.html`](index.html) bằng trình duyệt.

---

## 1. Tham chiếu thư viện

```xml
<ItemGroup>
  <ProjectReference Include="..\CONCOST.UI\CONCOST.UI.csproj" />
</ItemGroup>
```

`ProjectReference` sẽ tự chọn target framework khớp với ứng dụng chủ. Nếu dùng DLL build sẵn thì chọn đúng thư mục:

```text
src/CONCOST.UI/bin/Release/net48/CONCOST.UI.dll
src/CONCOST.UI/bin/Release/net8.0-windows/CONCOST.UI.dll
src/CONCOST.UI/bin/Release/net10.0-windows/CONCOST.UI.dll
```

Revit 2021–2024 và AutoCAD 2021–2024 chạy trên .NET Framework 4.8 → dùng `net48`. Revit 2025+ và AutoCAD 2025+ chạy trên .NET 8 → dùng `net8.0-windows`. Luôn kiểm tra runtime của đúng phiên bản host bạn đang dùng.

## 2. Gộp resource dictionary của theme

`ThemeResources.xaml` nạp các dictionary token (`Colors`, `Typography`, `Spacing`, `CornerRadius`) và theme Light mặc định. `Generic.xaml` chứa toàn bộ style và template của control. **Bắt buộc phải có cả hai.**

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

> Chỉ gộp `Themes/Light.xaml` là **không đủ**: file đó chỉ định nghĩa brush của theme, nên các key token như `CONCOST.Spacing.MD` hay `CONCOST.CornerRadius.Default` sẽ không resolve được.

## 3. Khai báo namespace

```xml
<Window x:Class="YourApp.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:ui="clr-namespace:CONCOST.UI.Controls;assembly=CONCOST.UI"
        xmlns:attached="clr-namespace:CONCOST.UI.AttachedProperties;assembly=CONCOST.UI"
        xmlns:charts="clr-namespace:CONCOST.UI.Controls.Charts;assembly=CONCOST.UI"
        xmlns:canvas="clr-namespace:CONCOST.UI.Controls.Canvas;assembly=CONCOST.UI"
        xmlns:eng="clr-namespace:CONCOST.UI.Controls.Engineering;assembly=CONCOST.UI"
        Background="{DynamicResource CONCOST.Brush.Background}"
        Foreground="{DynamicResource CONCOST.Brush.TextPrimary}">
    <Grid />
</Window>
```

Dùng `DynamicResource` cho **mọi** brush và style cần thay đổi theo theme lúc runtime.

## 4. Cấu hình theme

```csharp
using System.Windows.Media;
using CONCOST.UI.Themes;

ThemeManager.Current.Theme = AppTheme.System;        // Light | Dark | System
ThemeManager.Current.Preset = ThemePreset.Default;   // Default | Ocean | Forest | Sunset | Slate | Midnight | HighContrast | Brutal
ThemeManager.Current.AccentColor = Color.FromRgb(0xFF, 0x79, 0x00);
```

## 5. Dùng control

```xml
<StackPanel Margin="24">
    <!-- Ô nhập kỹ thuật có nhãn đơn vị -->
    <ui:UnitBox Header="Bề rộng dầm" Value="300" Unit="mm" DecimalPlaces="0" />

    <!-- Ô nhập số có nút tăng/giảm -->
    <ui:NumericUpDown Value="25" Minimum="0" Maximum="1000" Increment="5" Suffix=" kN" />

    <!-- Ký hiệu thép: tự phân tích 4D20 và tính As = 1256.6 mm² -->
    <eng:RebarInput Text="4D20" />

    <!-- Nút hành động có menu sổ xuống -->
    <ui:SplitButton Content="Lưu mô hình" Icon="Save" Style="{DynamicResource CONCOST.SplitButton.Primary}" />
</StackPanel>
```

### Biểu đồ kỹ thuật và khung nhìn CAD

```xml
<!-- ChartBar / ChartLine / ChartRadar / ChartPie đều kế thừa từ ChartBase -->
<charts:ChartLine Title="Biểu đồ mô men" Unit="kN.m" ShowArea="True" Datas="{Binding MomentSeries}" />

<Grid>
    <Grid.RowDefinitions>
        <RowDefinition Height="24" />
        <RowDefinition Height="*" />
    </Grid.RowDefinitions>
    <canvas:RulerControl Grid.Row="0" Orientation="Horizontal" MajorStep="50" UnitScale="1" />
    <canvas:ZoomableCanvas Grid.Row="1" ShowGrid="True" GridSize="20" />
</Grid>
```

`ChartBase.Datas` là `IEnumerable<KeyValuePair<string, double>>`: key là nhãn trục X, value là giá trị được vẽ. **Không có** thuộc tính `InvertAxis` — muốn theo quy ước TCVN (mô men dương vẽ xuống dưới) thì đảo dấu giá trị trước khi binding.

## 6. Dùng trong plugin Revit / AutoCAD

Class library của plugin không có `App.xaml`. Hãy giới hạn resource trong view của plugin và để `ThemeManager` áp theme cục bộ:

```csharp
public partial class SettingsPanel : UserControl
{
    public SettingsPanel()
    {
        InitializeComponent();
        ThemeManager.Current.ApplyTheme(this, AppTheme.Dark);
        ThemeManager.Current.ApplyAccentColor(this, Color.FromRgb(0xFF, 0x79, 0x00));
    }
}
```

Gộp dictionary trong `UserControl.Resources` của view đó, **không** dùng `Application.Resources`. Xem trang wiki **Plugin DLL (Revit/AutoCAD)** để biết đầy đủ cách làm và những lỗi thường gặp khi làm nhiễm resource của host.

## 7. Build, chạy, test

```powershell
dotnet restore CONCOST.UI.sln
dotnet build CONCOST.UI.sln --configuration Release
dotnet run  --project samples/CONCOST.UI.Showcase/CONCOST.UI.Showcase.csproj
dotnet test tests/CONCOST.UI.Tests/CONCOST.UI.Tests.csproj --configuration Release
```

Yêu cầu: Windows 10+, .NET 8 SDK và .NET 10 SDK. Bản `net48` tự restore reference assembly.

## 8. Đọc tiếp ở đâu

| Chủ đề | Ở đâu |
| --- | --- |
| Mọi control, thuộc tính và style key | [`docs/index.html`](index.html) |
| Design token và preset | wiki → **Theme & Design Tokens** |
| Attached property (icon, badge, validation, drag-drop) | wiki → **Attached Properties** |
| Lỗi tích hợp thường gặp | wiki → **FAQ & Troubleshooting** |
| Ánh xạ khi chuyển từ WinForms/WPF | [`MigrationGuide.vi.md`](MigrationGuide.vi.md) |
| Lịch sử phiên bản | [`../CHANGELOG.md`](../CHANGELOG.md) |
