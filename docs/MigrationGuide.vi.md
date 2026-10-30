# Hướng dẫn chuyển đổi

[English](MigrationGuide.md) · **Tiếng Việt**

Tài liệu này hướng dẫn cách chuyển ứng dụng WinForms hoặc WPF thông thường sang **CONCOST.UI**.

---

## 1. Cài đặt

Tham chiếu project `CONCOST.UI` từ ứng dụng WPF của bạn:
```xml
<ItemGroup>
  <ProjectReference Include="..\CONCOST.UI\CONCOST.UI.csproj" />
</ItemGroup>
```

Gộp **cả hai** dictionary — `ThemeResources.xaml` chứa token và theme mặc định, `Generic.xaml` chứa toàn bộ style của control:

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

Cấu hình theme trực tiếp, không cần gói dependency injection:

```csharp
ThemeManager.Current.Theme = AppTheme.System;
ThemeManager.Current.Preset = ThemePreset.Default;
```
---

## 2. Ánh xạ control cũ sang control của CONCOST

| Control hiện có (WinForms/WPF) | Control của CONCOST | Lợi ích |
|---|---|---|
| `Button` | `<Button Style="{StaticResource CONCOST.Button.Primary}" />` | Icon vector, năm biến thể giao diện, có animation khi hover/nhấn |
| `TextBox` (nhập số) | `<ui:NumericUpDown Suffix=" mm" />` | Tự giới hạn min/max, tăng giảm bằng bàn phím và chuột, định dạng số |
| `TextBox` (đơn vị kỹ thuật) | `<ui:UnitBox Unit="kN.m" />` hoặc `<eng:MomentBox />` | Nhãn đơn vị kỹ thuật và định dạng nhất quán |
| Nhập thép | `<eng:RebarInput Text="4D20" />` | Phân tích ký hiệu thép và tính ngay diện tích $A_s$ |
| `DataGridView` | `<DataGrid Style="{StaticResource CONCOST.DataGrid.Base}" />` | Dòng kẻ sọc, màu tự đổi theo theme sáng/tối |
| Canvas vẽ | `<canvas:ZoomableCanvas />` | Zoom và pan không giới hạn, lưới CAD co giãn theo tỉ lệ |

---

## 3. Thay đổi phá vỡ tương thích và chính sách deprecation

1. **Thông báo deprecation:** API bị deprecate sẽ được đánh dấu `[Obsolete("Use the replacement API; this will be removed in v(N+2).0.0")]` **trước ít nhất hai minor version** so với lúc xoá.
2. **Semantic Versioning:** thay đổi phá vỡ tương thích **chỉ** xuất hiện trong bản **MAJOR** (ví dụ `2.0.0`) và luôn kèm ví dụ migration trong `CHANGELOG.md`.
