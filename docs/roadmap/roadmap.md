# CONCOST.UI — Kế hoạch xây dựng WPF UI Library

## 1. Mục tiêu dự án

`CONCOST.UI` được định hướng trở thành một **WPF UI Library / Design System** dùng lại được cho nhiều phần mềm trong hệ sinh thái CONCOST, tương tự HandyControl nhưng tập trung vào các nhu cầu thực tế của phần mềm kỹ thuật, CAD, xây dựng và desktop.

Mục tiêu chính:

- WPF native.
- Không phụ thuộc Microsoft Excel hoặc Office.
- Hỗ trợ MVVM tốt.
- Hạn chế code-behind.
- Hỗ trợ Light / Dark / System Theme.
- Có hệ thống màu, typography, spacing và corner radius thống nhất.
- Hỗ trợ Accent Color.
- Target `.NET 8 WPF`.
- Có thể bổ sung `.NET Framework 4.8` nếu cần tương thích CAD/plugin cũ.
- Có Demo App riêng.
- Có Unit Test.
- Có Documentation.
- Có thể đóng gói NuGet.
- Hạn chế dependency ngoài.
- Thiết kế theo hướng mở rộng lâu dài.

---

# 2. Cấu trúc Solution

Tên solution:

```text
CONCOST.UI
```

Cấu trúc đề xuất:

```text
CONCOST.UI/
│
├── src/
│   ├── CONCOST.UI/
│   ├── CONCOST.UI.Core/
│   └── CONCOST.UI.Themes/
│
├── samples/
│   └── CONCOST.UI.Demo/
│
├── tests/
│   └── CONCOST.UI.Tests/
│
├── docs/
│
├── assets/
│
├── CONCOST.UI.sln
├── README.md
├── LICENSE
└── Directory.Build.props
```

Namespace đề xuất:

```text
CONCOST.UI
```

Ví dụ:

```csharp
using CONCOST.UI.Controls;
using CONCOST.UI.Themes;
```

NuGet package:

```text
CONCOST.UI
```

---

# 3. Kiến trúc tổng thể

```text
                 CONCOST.UI
                    │
          ┌─────────┴──────────┐
          │                    │
       Core                  Themes
          │                    │
     Utilities           Design Tokens
          │                    │
          └─────────┬──────────┘
                    │
                 Controls
                    │
       ┌────────────┼─────────────┐
       │            │             │
    Basic         Input         Data
       │            │             │
 Button/Text     TextBox       DataGrid
 Icon/Card       ComboBox      TreeView
                NumericUpDown
                    │
              Interactivity
                    │
       ┌────────────┼─────────────┐
       │            │             │
    Dialog        Toast        Behavior
```

Nguyên tắc chính:

> Khoảng 80% thành phần nên giải quyết bằng Style / ControlTemplate / AttachedProperty. Chỉ tạo CustomControl khi thật sự cần.

---

# 4. Project `CONCOST.UI.Core`

Không chứa UI nặng.

Cấu trúc:

```text
CONCOST.UI.Core/
├── Commands/
├── Converters/
├── Extensions/
├── Helpers/
├── Interfaces/
├── Models/
├── Utilities/
└── Constants/
```

Các class nên có:

```text
RelayCommand
AsyncRelayCommand
ObservableObject
EnumHelper
ColorHelper
ResourceHelper
DispatcherHelper
VisualTreeHelperEx
```

Dependency:

```text
CONCOST.UI
    ↓
CONCOST.UI.Core
```

---

# 5. Design System

## 5.1 Color Tokens

Tạo:

```text
Themes/Tokens/Colors.xaml
```

Ví dụ:

```xml
<Color x:Key="CONCOST.Color.Primary">#FF7900</Color>
<Color x:Key="CONCOST.Color.Success">#28A745</Color>
<Color x:Key="CONCOST.Color.Warning">#FFB020</Color>
<Color x:Key="CONCOST.Color.Danger">#DC3545</Color>
<Color x:Key="CONCOST.Color.Info">#2196F3</Color>
```

Tạo Brush:

```xml
<SolidColorBrush
    x:Key="CONCOST.Brush.Primary"
    Color="{StaticResource CONCOST.Color.Primary}" />
```

Không hard-code màu trực tiếp trong control:

```xml
Background="#FF7900"
```

Mà dùng:

```xml
Background="{DynamicResource CONCOST.Brush.Primary}"
```

---

## 5.2 Neutral Colors

Chuẩn bị các cấp màu:

```text
Gray50
Gray100
Gray200
Gray300
Gray400
Gray500
Gray600
Gray700
Gray800
Gray900
Gray950
```

Các token semantic:

```text
Background
Surface
SurfaceSecondary
Border
BorderHover
BorderFocused
TextPrimary
TextSecondary
TextDisabled
```

---

# 6. Typography

Tạo:

```text
Themes/Tokens/Typography.xaml
```

Các mức:

```text
FontSizeXS
FontSizeSM
FontSizeMD
FontSizeLG
FontSizeXL
FontSizeXXL
```

Ví dụ:

```xml
<sys:Double x:Key="CONCOST.FontSize.Small">12</sys:Double>
<sys:Double x:Key="CONCOST.FontSize.Default">14</sys:Double>
<sys:Double x:Key="CONCOST.FontSize.Large">16</sys:Double>
<sys:Double x:Key="CONCOST.FontSize.Title">20</sys:Double>
```

Font weight:

```text
FontWeightNormal
FontWeightMedium
FontWeightSemiBold
FontWeightBold
```

---

# 7. Spacing System

Tạo:

```text
Themes/Tokens/Spacing.xaml
```

Thang spacing:

```text
2
4
6
8
12
16
20
24
32
40
48
```

Resource:

```text
CONCOST.Spacing.XXS
CONCOST.Spacing.XS
CONCOST.Spacing.SM
CONCOST.Spacing.MD
CONCOST.Spacing.LG
CONCOST.Spacing.XL
```

Ví dụ:

```xml
<Thickness x:Key="CONCOST.Padding.Control">
    10,6
</Thickness>
```

---

# 8. Corner Radius

Chuẩn hóa:

```text
RadiusNone = 0
RadiusXS   = 2
RadiusSM   = 4
RadiusMD   = 6
RadiusLG   = 8
RadiusXL   = 12
RadiusFull = 999
```

Ví dụ:

```xml
<CornerRadius x:Key="CONCOST.CornerRadius.Default">
    6
</CornerRadius>
```

---

# 9. Theme System

Cấu trúc:

```text
Themes/
├── Light.xaml
├── Dark.xaml
├── Generic.xaml
└── ThemeResources.xaml
```

Enum:

```csharp
public enum AppTheme
{
    Light,
    Dark,
    System
}
```

API:

```csharp
ThemeManager.Current.ApplyTheme(AppTheme.Dark);
```

hoặc:

```csharp
ThemeManager.Current.Theme = AppTheme.Dark;
```

Sau này có thể bổ sung:

```text
HighContrast
```

---

# 10. Accent Color

Cho phép thay accent color:

```csharp
ThemeManager.Current.AccentColor = Colors.Orange;
```

Preset:

```text
Orange
Blue
Green
Red
Purple
```

Mặc định ưu tiên:

```text
CONCOST Orange
```

---

# 11. Attached Properties

Tạo thư mục:

```text
AttachedProperties/
```

Các class:

```text
BorderElement
IconElement
InfoElement
TextElement
ButtonElement
ControlElement
DataGridElement
ScrollViewerElement
ValidationElement
TitleElement
```

Ví dụ:

```xml
<Button
    ui:BorderElement.CornerRadius="8"/>
```

```xml
<TextBox
    ui:InfoElement.Placeholder="Nhập tên cấu kiện"
    ui:InfoElement.Required="True"/>
```

---

# 12. Icon System

Cấu trúc:

```text
Icons/
├── IconKind.cs
├── Icon.cs
├── IconProvider.cs
└── Geometries/
```

API:

```xml
<ui:Icon Kind="Save"/>
```

IconKind ban đầu:

```text
Add
Remove
Delete
Edit
Save
Open
Close
Search
Refresh
Settings
Home
User
Folder
File
Copy
Paste
Cut
Undo
Redo
Check
Warning
Error
Info
ChevronUp
ChevronDown
ChevronLeft
ChevronRight
Menu
More
```

Ưu tiên vector Geometry thay vì PNG.

---

# 13. Button

Variants:

```text
Primary
Secondary
Outline
Ghost
Danger
Success
Link
```

API cơ bản:

```xml
<Button
    Style="{StaticResource CONCOST.Button.Primary}"
    Content="Lưu"/>
```

API nâng cao:

```xml
<ui:Button
    Appearance="Primary"
    Icon="Save"
    Content="Lưu"/>
```

Properties:

```text
Appearance
Icon
IconPlacement
IsLoading
LoadingText
CornerRadius
```

States:

```text
Normal
Hover
Pressed
Focused
Disabled
Loading
```

---

# 14. TextBox

Tính năng:

```text
Placeholder
Clear Button
Prefix Icon
Suffix Icon
Validation
ReadOnly
Disabled
Focused State
```

API:

```xml
<TextBox
    ui:InfoElement.Placeholder="Nhập tên..."
    ui:IconElement.Icon="Search"/>
```

---

# 15. PasswordBox

Custom control:

```text
PasswordBoxEx
```

Features:

```text
Show/hide password
Placeholder
Validation
Icon
Password strength
```

---

# 16. ComboBox

Features:

```text
Searchable
Clearable
Placeholder
Icon
Empty state
```

Ưu tiên style WPF ComboBox trước, không viết lại logic ngay.

---

# 17. CheckBox / RadioButton

Variants:

```text
Default
Switch
Card
```

States:

```text
Checked
Unchecked
Indeterminate
Disabled
Hover
```

---

# 18. ToggleSwitch

API:

```xml
<ui:ToggleSwitch
    IsChecked="{Binding DarkMode}"
    OnContent="Bật"
    OffContent="Tắt"/>
```

Properties:

```text
IsChecked
OnContent
OffContent
OnIcon
OffIcon
```

---

# 19. NumericUpDown

Custom control.

API:

```xml
<ui:NumericUpDown
    Value="{Binding Width}"
    Minimum="0"
    Maximum="5000"
    Increment="10"
    DecimalPlaces="0"/>
```

Features:

```text
Min / Max
Increment
Decimal
Mouse wheel
Keyboard arrow
Suffix
Prefix
Unit
Validation
```

Ví dụ:

```xml
<ui:NumericUpDown
    Value="250"
    Suffix="mm"/>
```

---

# 20. UnitBox

Control đặc thù cho CONCOST.

API:

```xml
<ui:UnitBox
    Value="{Binding Length}"
    Unit="mm"/>
```

Unit hỗ trợ:

```text
mm
cm
m
mm²
cm²
m²
kN
kN/m
MPa
kg
t
```

Có thể hỗ trợ conversion:

```text
1000 mm ↔ 1 m
```

---

# 21. Engineering Controls

Các control nên phát triển riêng:

```text
EngineeringNumberBox
LengthBox
AreaBox
VolumeBox
ForceBox
MomentBox
PressureBox
RebarInput
SectionInput
CoordinateInput
AngleInput
```

Ví dụ:

```xml
<ui:LengthBox
    Value="{Binding BeamWidth}"
    Unit="mm"/>
```

Rebar syntax:

```text
4D20
2Ø16
D10@200
2x3D18
```

Sau này có thể parse và validate syntax.

---

# 22. Card

API:

```xml
<ui:Card
    Header="Thông tin tiết diện"
    Icon="Info">
</ui:Card>
```

Properties:

```text
Header
Footer
Icon
Shadow
CornerRadius
IsCollapsible
IsExpanded
```

---

# 23. GroupBox

Custom style hiện đại cho WPF GroupBox.

Features:

```text
Header icon
Header action
Collapsible
Border variants
```

---

# 24. Badge

```xml
<ui:Badge
    Content="NEW"
    Appearance="Success"/>
```

Variants:

```text
Primary
Success
Warning
Danger
Info
Neutral
```

---

# 25. Tag / Chip

```xml
<ui:Tag
    Content="Dầm"
    IsClosable="True"/>
```

---

# 26. Divider

```xml
<ui:Divider/>
```

hoặc:

```xml
<ui:Divider Text="Thông tin khác"/>
```

---

# 27. Tooltip

Features:

```text
Delay
Placement
Title
Description
Icon
```

---

# 28. DataGrid

Không viết lại DataGrid engine.

Style các thành phần:

```text
DataGrid
DataGridRow
DataGridCell
DataGridColumnHeader
DataGridRowHeader
```

Variants:

```text
Default
Compact
Small
Bordered
Striped
```

API:

```xml
<DataGrid
    Style="{StaticResource CONCOST.DataGrid.Compact}"/>
```

Attached properties:

```xml
ui:DataGridElement.ShowRowNumber="True"
ui:DataGridElement.EnableStripedRows="True"
ui:DataGridElement.ShowGridLines="True"
```

Enhancements:

```text
Row numbering
Alternating rows
Selected row highlight
Frozen column styling
Inline validation
Column filter
Column search
Column visibility
Empty state
Loading overlay
```

Advanced:

```text
Multi-level headers
Column groups
Summary footer
Excel-like filtering
```

---

# 29. TreeView

Features:

```text
Icons
Expand/collapse animation
Checkbox tree
Selected styling
Context menu
```

---

# 30. ListBox / ListView

Variants:

```text
Default
Card
Compact
Navigation
```

---

# 31. Tabs

Style:

```text
TabControl
TabItem
```

Variants:

```text
Line
Card
Pill
```

Optional:

```text
Closable tabs
Add tab button
```

---

# 32. Navigation

Controls:

```text
NavigationView
NavigationItem
NavigationGroup
```

API:

```xml
<ui:NavigationView>
    <ui:NavigationItem
        Icon="Home"
        Content="Trang chủ"/>
</ui:NavigationView>
```

Features:

```text
Collapsed
Expanded
Nested menu
Selected state
Badge
Separator
```

---

# 33. Breadcrumb

Ví dụ:

```text
Dự án > Kết cấu > Dầm > B1
```

---

# 34. Dialog

Abstraction:

```text
IDialogService
```

API:

```csharp
await DialogService.ShowAsync(...);
```

Dialogs:

```text
Message
Confirm
Warning
Error
Custom content
```

Result:

```text
OK
Cancel
Yes
No
```

---

# 35. MessageBox

API:

```csharp
await CONCOSTMessageBox.ShowAsync(
    "Xác nhận",
    "Bạn có muốn xóa không?");
```

---

# 36. Toast / Notification

Service:

```text
ToastService
```

API:

```csharp
Toast.Success("Lưu thành công");
Toast.Error("Không thể kết nối máy chủ");
Toast.Warning("Thiếu dữ liệu");
Toast.Info("Đang xử lý...");
```

Positions:

```text
TopLeft
TopRight
BottomLeft
BottomRight
TopCenter
BottomCenter
```

---

# 37. Snackbar

Ví dụ:

```text
Đã xóa bản ghi       Hoàn tác
```

---

# 38. Loading

Controls:

```text
Spinner
ProgressRing
ProgressBar
LoadingOverlay
Skeleton
```

Ví dụ:

```xml
<ui:LoadingOverlay
    IsLoading="{Binding IsBusy}">
```

---

# 39. Empty State

```xml
<ui:EmptyState
    Icon="Folder"
    Title="Không có dữ liệu"
    Description="Chưa có cấu kiện nào."/>
```

---

# 40. Validation System

Hỗ trợ:

```text
IDataErrorInfo
INotifyDataErrorInfo
ValidationRules
```

States:

```text
Normal
Valid
Invalid
Warning
```

Attached property:

```xml
ui:ValidationElement.ShowMessage="True"
```

---

# 41. Form Components

Controls:

```text
FormItem
FormSection
FormGroup
```

Ví dụ:

```xml
<ui:FormItem
    Label="Tên cấu kiện"
    Required="True">

    <TextBox/>

</ui:FormItem>
```

---

# 42. SearchBox

```xml
<ui:SearchBox
    Text="{Binding SearchText}"
    Placeholder="Tìm kiếm..."
    IsClearButtonVisible="True"/>
```

---

# 43. Date / Time

Controls:

```text
DatePicker
TimePicker
DateTimePicker
```

Style control mặc định trước, custom khi cần.

---

# 44. Popup / Flyout

Controls:

```text
Flyout
DropDown
PopupMenu
```

---

# 45. ContextMenu

Style:

```text
MenuItem
Separator
Submenu
Icon
Shortcut
```

---

# 46. Window

Custom window:

```text
CONCOSTWindow
```

Features:

```text
Custom title bar
Minimize
Maximize
Close
Resize
Drag
Dark mode
Window shadow
Accent title bar
```

API:

```xml
<ui:CONCOSTWindow
    Title="CONCOST">
```

---

# 47. TitleBar

Tách riêng:

```text
TitleBar
WindowButtons
```

---

# 48. ScrollBar

Style:

```text
Thin
Auto hide
Hover expand
```

---

# 49. Slider

Variants:

```text
Default
RangeSlider
```

---

# 50. Progress

Controls:

```text
ProgressBar
ProgressRing
CircularProgress
```

---

# 51. Avatar

```xml
<ui:Avatar
    Text="NP"/>
```

---

# 52. Pagination

```xml
<ui:Pagination
    Page="1"
    TotalPages="20"/>
```

---

# 53. Toolbar

Controls:

```text
Toolbar
ToolbarButton
ToolbarSeparator
```

---

# 54. Ribbon Helpers

Có thể hỗ trợ:

```text
RibbonIcon
RibbonButtonStyle
RibbonGroupStyle
```

Không viết Ribbon hoàn chỉnh ngay từ đầu.

---

# 55. Docking / Panels

Có thể nghiên cứu sau:

```text
Dockable panel
Side panel
Resizable panel
```

Không ưu tiên trong giai đoạn đầu.

---

# 56. Animations

Animation cơ bản:

```text
FadeIn
FadeOut
SlideIn
SlideOut
ScaleIn
ScaleOut
```

Duration:

```text
Fast = 100ms
Normal = 200ms
Slow = 300ms
```

---

# 57. Behaviors

Folder:

```text
Behaviors/
```

Behaviors:

```text
FocusBehavior
AutoScrollBehavior
SelectAllOnFocusBehavior
NumericInputBehavior
DragDropBehavior
DoubleClickCommandBehavior
EscapeCloseBehavior
```

---

# 58. Command Extensions

Có thể hỗ trợ:

```text
EventToCommand
DoubleClickCommand
LoadedCommand
ClosingCommand
```

---

# 59. Converters

Tạo sẵn:

```text
BooleanToVisibilityConverter
InverseBooleanConverter
NullToVisibilityConverter
EnumToBooleanConverter
StringNullOrEmptyConverter
ColorToBrushConverter
```

---

# 60. Demo Application

Tên:

```text
CONCOST.UI.Demo
```

Navigation:

```text
Home

Basic
├── Button
├── Icon
├── Text
├── Divider
├── Badge

Input
├── TextBox
├── ComboBox
├── NumericUpDown
├── UnitBox
├── CheckBox
├── RadioButton
├── ToggleSwitch

Data
├── DataGrid
├── TreeView
├── ListView
├── Pagination

Navigation
├── Menu
├── Tabs
├── Breadcrumb

Feedback
├── Dialog
├── Toast
├── Loading
├── EmptyState

Layout
├── Card
├── GroupBox
├── Form

Theme
├── Light
├── Dark
├── Accent Color
```

Mỗi Demo Page nên có:

```text
1. Preview
2. Description
3. Variants
4. States
5. XAML
6. C#
7. Properties
```

---

# 61. Documentation

Cấu trúc:

```text
docs/
├── GettingStarted.md
├── Installation.md
├── Themes.md
├── Icons.md
└── Controls/
    ├── Button.md
    ├── TextBox.md
    ├── DataGrid.md
    └── ...
```

---

# 62. Naming Convention

Resource:

```text
CONCOST.Button.Primary
CONCOST.Button.Secondary
CONCOST.TextBox.Default
CONCOST.DataGrid.Default
```

Brush:

```text
CONCOST.Brush.Primary
CONCOST.Brush.Background
```

Color:

```text
CONCOST.Color.Primary
```

Spacing:

```text
CONCOST.Spacing.MD
```

---

# 63. DependencyProperty Convention

Mọi DependencyProperty public phải:

- Có XML documentation.
- Có default value rõ ràng.
- Metadata đúng.
- Dùng `BindsTwoWayByDefault` khi phù hợp.
- Không gây memory leak.
- Không giữ reference dư thừa.

---

# 64. Accessibility

Control phải hỗ trợ:

```text
Keyboard navigation
Tab navigation
Focus indicator
Disabled state
High contrast
Tooltips
AutomationProperties
```

---

# 65. DPI Scaling

Test:

```text
100%
125%
150%
175%
200%
```

---

# 66. Resolution Testing

Test tối thiểu:

```text
1366x768
1920x1080
2560x1440
4K
```

---

# 67. Dark Theme Testing

Kiểm tra:

```text
Text contrast
Border
Hover
Selected
Disabled
Popup
Tooltip
ContextMenu
DataGrid
ScrollBar
```

---

# 68. Performance

Đặc biệt DataGrid.

Test:

```text
1,000 rows
10,000 rows
50,000 rows
```

Yêu cầu:

- Bật virtualization.
- Tránh trigger quá nặng.
- Tránh nhiều Border lồng nhau.
- Tránh animation không cần thiết.

---

# 69. Memory Leak Tests

Đặc biệt:

```text
Event handlers
Static events
DispatcherTimer
Popup
Dialog
Toast
Behavior
```

---

# 70. Unit Tests

Test:

```text
ThemeManager
Color conversion
NumericUpDown
Unit conversion
Validation
Converters
Pagination
```

---

# 71. Packaging

Output mong muốn:

```text
CONCOST.UI.dll
CONCOST.UI.Core.dll
```

Nếu có thể, giữ số lượng DLL thấp.

---

# 72. NuGet

Package:

```text
CONCOST.UI
```

Metadata:

```text
Title
Description
Version
Authors
Repository
License
Tags
```

Tags:

```text
WPF
UI
Controls
Theme
CONCOST
Desktop
```

---

# 73. Versioning

Dùng Semantic Versioning:

```text
MAJOR.MINOR.PATCH
```

Ví dụ:

```text
0.1.0
0.2.0
0.3.0
1.0.0
```

---

# 74. Roadmap

| Milestone | Nội dung | Output |
|---|---|---|
| M0 | Solution + architecture | Project chạy được |
| M1 | Design Tokens | Color / Brush / Spacing / Typography |
| M2 | Theme Engine | Light / Dark / System |
| M3 | Basic Controls | Button / Icon / Card / Divider |
| M4 | Input Controls | TextBox / ComboBox / Checkbox / Toggle |
| M5 | Numeric Controls | NumericUpDown / UnitBox |
| M6 | Data Controls | DataGrid / List / Tree |
| M7 | Feedback | Dialog / Toast / Loading |
| M8 | Navigation | Tabs / Menu / Breadcrumb |
| M9 | Window | CONCOSTWindow / TitleBar |
| M10 | Demo + Docs | Full showcase |
| M11 | Tests + optimization | Stable |
| M12 | NuGet v1.0 | Release |

---

# 75. Version Roadmap

## v0.1.0

```text
ThemeManager
Light Theme
Dark Theme

Colors
Brushes
Typography
Spacing
Radius

Icon
Button
TextBox
Card
Divider
```

## v0.2.0

```text
ComboBox
CheckBox
RadioButton
ToggleSwitch
PasswordBox
```

## v0.3.0

```text
NumericUpDown
UnitBox
SearchBox
FormItem
```

## v0.4.0

```text
DataGrid theme
Row numbers
Compact mode
Striped rows
Validation
Loading
Empty state
```

## v0.5.0

```text
Dialog
MessageBox
Toast
Snackbar
```

## v0.6.0

```text
TreeView
ListView
Tabs
Breadcrumb
NavigationView
```

## v0.7.0

```text
CONCOSTWindow
TitleBar
ContextMenu
Tooltip
Popup
```

## v0.8.0

```text
Animations
Behaviors
DragDrop
Advanced validation
```

## v0.9.0

Tập trung:

```text
Bug fixes
API cleanup
Performance
Accessibility
Documentation
```

## v1.0.0

Điều kiện:

```text
API ổn định
Không crash
Theme hoàn chỉnh
Demo hoàn chỉnh
Documentation hoàn chỉnh
NuGet package
Các control chính ổn định
```

---

# 76. Nguyên tắc phát triển

1. Không hard-code màu trong control.
2. Không hard-code font nếu không cần.
3. Không viết CustomControl nếu Style đã đủ.
4. Không dùng UserControl cho component cần template hóa mạnh.
5. Tất cả control phải hỗ trợ Dark Theme.
6. Tất cả input phải hỗ trợ Validation.
7. Tất cả control phải hỗ trợ keyboard.
8. Không phá MVVM.
9. Mọi public API phải có documentation.
10. Mỗi control phải có Demo page trước khi coi là hoàn thành.

---

# 77. Definition of Done cho mỗi control

Ví dụ `NumericUpDown`:

```text
☑ Light theme
☑ Dark theme

☑ Normal
☑ Hover
☑ Focus
☑ Disabled
☑ Invalid

☑ Keyboard
☑ Mouse
☑ Binding
☑ TwoWay binding

☑ Min
☑ Max
☑ Increment

☑ Demo
☑ Documentation
☑ Test
```

---

# 78. Git Workflow

Branches:

```text
main
develop
feature/*
fix/*
release/*
```

Ví dụ:

```text
feature/button-control
feature/theme-manager
feature/numeric-updown
```

Commit convention:

```text
feat: add NumericUpDown
fix: correct dark theme textbox border
refactor: simplify ThemeManager
docs: add Button documentation
```

---

# 79. Thứ tự bắt tay vào code

```text
01. Tạo Solution
02. Tạo CONCOST.UI.Core
03. Tạo CONCOST.UI
04. Tạo Demo
05. Directory.Build.props
06. Design Tokens
07. Light Theme
08. Dark Theme
09. ThemeManager
10. Icon system
11. Button
12. TextBox
13. Card
14. Divider
15. ComboBox
16. CheckBox
17. ToggleSwitch
18. NumericUpDown
19. UnitBox
20. DataGrid
```

Sau đó mới mở rộng.

---

# 80. Cấu trúc thư mục cuối cùng

```text
CONCOST.UI
│
├── src
│   │
│   ├── CONCOST.UI.Core
│   │   ├── Commands
│   │   ├── Converters
│   │   ├── Extensions
│   │   ├── Helpers
│   │   └── Utilities
│   │
│   └── CONCOST.UI
│       │
│       ├── AttachedProperties
│       │
│       ├── Behaviors
│       │
│       ├── Controls
│       │   ├── Basic
│       │   ├── Data
│       │   ├── Feedback
│       │   ├── Input
│       │   ├── Layout
│       │   ├── Navigation
│       │   └── Windows
│       │
│       ├── Icons
│       │
│       ├── Services
│       │
│       ├── Styles
│       │   ├── Button.xaml
│       │   ├── TextBox.xaml
│       │   ├── ComboBox.xaml
│       │   ├── DataGrid.xaml
│       │   └── ...
│       │
│       └── Themes
│           ├── Tokens
│           │   ├── Colors.xaml
│           │   ├── Brushes.xaml
│           │   ├── Typography.xaml
│           │   ├── Spacing.xaml
│           │   └── Radius.xaml
│           │
│           ├── Light.xaml
│           ├── Dark.xaml
│           └── Generic.xaml
│
├── samples
│   └── CONCOST.UI.Demo
│
├── tests
│   └── CONCOST.UI.Tests
│
├── docs
│
├── assets
│
└── CONCOST.UI.sln
```

---

# 81. Định hướng dài hạn

`CONCOST.UI` không nên chỉ là một bộ Style WPF.

Mục tiêu dài hạn nên là:

```text
CONCOST.UI
    =
Design System
+ WPF Controls
+ Theme Engine
+ Engineering Controls
+ MVVM Helpers
+ Dialog / Notification System
+ DataGrid Extensions
+ CAD-friendly UI Components
```

Nếu nền tảng Theme + Token + Style + AttachedProperty được làm tốt từ đầu thì thư viện có thể mở rộng lên hàng chục hoặc hàng trăm control mà vẫn dễ maintain.

Phần tạo khác biệt lớn nhất so với HandyControl nên tập trung vào các control dành riêng cho phần mềm kỹ thuật và xây dựng như:

```text
UnitBox
LengthBox
AreaBox
VolumeBox
ForceBox
MomentBox
PressureBox
RebarInput
SectionInput
CoordinateInput
AngleInput
```

Đây sẽ là lợi thế riêng của `CONCOST.UI`.

---

# 82. CI/CD Pipeline

Mục tiêu: mọi commit vào `develop`/`main` đều được build, test, pack tự động.

```text
.github/
└── workflows/
    ├── build.yml       # build + unit test mỗi PR/push
    ├── release.yml     # pack + push NuGet khi tag version
    └── docs.yml        # build & publish documentation site
```

`build.yml` tối thiểu nên có các bước:

```text
1. Checkout
2. Setup .NET SDK (8.x)
3. Restore
4. Build (Debug + Release)
5. Run Unit Tests + publish kết quả
6. (optional) Run code analyzer / lint
```

`release.yml` kích hoạt khi tạo Git tag dạng `v*`:

```text
1. Build Release
2. dotnet pack
3. dotnet nuget push
4. Tạo GitHub Release kèm changelog
```

Yêu cầu:

- Build phải fail nếu Warning-as-Error (đặc biệt với XAML binding warning).
- Badge build status gắn trên README.md.
- Artifact NuGet build ra được lưu lại (GitHub Actions artifact) để kiểm tra trước khi publish chính thức.

---

# 83. Code Analysis & Coding Standards

Bổ sung để nguyên tắc ở mục 76 thực sự được enforce, không chỉ nằm trên giấy.

```text
.editorconfig
Directory.Build.props (TreatWarningsAsErrors cho rule quan trọng)
StyleCop.Analyzers hoặc .NET analyzers built-in
Nullable reference types: enable
```

Rule nên bật:

```text
CA2000 - Dispose objects before losing scope
CA1001 - Types that own disposable fields should be disposable
CA2213 - Disposable fields should be disposed
IDE0005 - Unused usings
```

XAML riêng:

```text
Không dùng magic number cho màu/spacing (đã nêu ở mục 76, nay kiểm tra bằng CI thay vì code review thủ công)
```

Có thể viết một script/analyzer nhỏ quét toàn bộ `.xaml` để phát hiện `Background="#..."` hard-code, chạy trong CI, fail build nếu phát hiện.

---

# 84. Chiến lược Multi-Targeting (.NET 8 + .NET Framework 4.8)

Vì mục tiêu ban đầu ghi "có thể bổ sung `.NET Framework 4.8` nếu cần tương thích CAD/plugin cũ", cần làm rõ chiến lược kỹ thuật ngay từ đầu để tránh phải viết lại sau này.

```xml
<TargetFrameworks>net8.0-windows;net48</TargetFrameworks>
```

Vấn đề cần lường trước:

```text
- API khác nhau giữa net8.0 và net48 (ví dụ: nullable reference types, Span<T>, record)
- WPF trên net48 thiếu một số cải tiến performance/accessibility có ở net8.0
- NuGet package cần build multi-target, test riêng từng target
- CI matrix build cả hai target
```

Khuyến nghị:

```text
- Giữ CONCOST.UI.Core không phụ thuộc API mới của .NET 8 nếu có thể, dùng #if NET8_0_OR_GREATER khi cần thiết
- Test Demo App trên cả hai target trước khi release
- Ghi rõ trong docs: tính năng nào chỉ hỗ trợ net8.0-windows
```

---

# 85. Contribution & Quy trình cộng tác

```text
CONTRIBUTING.md
CODE_OF_CONDUCT.md (nếu mở rộng team hoặc open-source nội bộ)
.github/ISSUE_TEMPLATE/
.github/PULL_REQUEST_TEMPLATE.md
```

Nội dung `CONTRIBUTING.md` tối thiểu:

```text
- Cách setup môi trường dev
- Coding convention (link tới mục 76 + 83)
- Quy trình branch/commit (link mục 78)
- Checklist trước khi mở PR (build pass, test pass, demo page đã thêm)
- Definition of Done cho control mới (link mục 77)
```

---

# 86. Localization / Đa ngôn ngữ (i18n)

Toàn bộ nội dung hiện tại của control (placeholder, message, tooltip) đang mặc định tiếng Việt. Cần tách resource string ngay từ đầu để không phải retrofit sau này.

```text
Resources/
├── Strings.resx           # default (en hoặc vi)
├── Strings.vi.resx
└── Strings.en.resx
```

API:

```csharp
LocalizationManager.Current.Culture = new CultureInfo("vi-VN");
```

```xml
<TextBlock Text="{ui:Loc Key=Common_Save}"/>
```

Yêu cầu:

```text
- Toàn bộ text hiển thị trong control built-in (MessageBox, Dialog mặc định, Validation message) phải qua resource, không hard-code string tiếng Việt trong code-behind.
- Fallback về tiếng Anh nếu thiếu bản dịch.
- Font UI mặc định phải test kỹ hiển thị dấu tiếng Việt (nhiều font hiện đại như Segoe UI Variable bị lỗi dấu ở một số ký tự tổ hợp).
```

---

# 87. Định dạng số theo Culture (Culture-aware Numeric Formatting)

Liên quan trực tiếp tới `UnitBox`, `NumericUpDown`, `LengthBox`... vì các control kỹ thuật rất nhạy với dấu phân cách thập phân.

```text
- Hỗ trợ cả dấu chấm (.) và dấu phẩy (,) làm decimal separator tùy CultureInfo
- Parse linh hoạt: cho phép người dùng gõ cả hai kiểu, tự chuẩn hóa theo culture hiện tại
- Format hiển thị số lớn có group separator (1.000.000 hoặc 1,000,000 tùy culture)
```

API:

```csharp
NumericFormatOptions.Culture = CultureInfo.CurrentCulture;
NumericFormatOptions.AllowBothSeparators = true;
```

---

# 88. Canvas / Viewport Control (Zoom & Pan)

Đây là control nền tảng quan trọng nhất còn thiếu để `CONCOST.UI` thực sự khác biệt cho phần mềm CAD/kỹ thuật, thay vì chỉ là bộ Style thông thường.

```text
Controls/Canvas/
├── ZoomableCanvas.cs
├── PanZoomBehavior.cs
├── GridBackground.cs
└── RulerControl.cs
```

API:

```xml
<ui:ZoomableCanvas
    Zoom="{Binding ZoomLevel}"
    MinZoom="0.1"
    MaxZoom="10"
    ShowGrid="True"
    ShowRuler="True">

    <!-- nội dung CAD / sơ đồ kết cấu -->

</ui:ZoomableCanvas>
```

Tính năng:

```text
Zoom bằng mouse wheel (Ctrl + Scroll)
Pan bằng chuột giữa hoặc Space + Drag
Zoom to fit
Zoom to selection
Grid nền có thể bật/tắt, đổi khoảng cách lưới
Ruler (thước đo) trên/trái canvas
Snap to grid (cơ bản)
Minimap (tùy chọn, giai đoạn sau)
```

Đây nên là control "flagship" của thư viện, tương tự vai trò của `DataGrid` nhưng cho nội dung đồ họa/kỹ thuật.

---

# 89. Chart / Diagram Controls

Phần mềm kết cấu/xây dựng cần trực quan hóa dữ liệu dạng biểu đồ kỹ thuật, không chỉ chart thống kê thông thường.

```text
Controls/Charts/
├── LineChart.cs
├── BarChart.cs
├── DiagramChart.cs      # biểu đồ momen / lực cắt / chuyển vị
└── GaugeChart.cs
```

API ví dụ:

```xml
<ui:DiagramChart
    Values="{Binding MomentDiagramPoints}"
    Title="Biểu đồ Moment"
    Unit="kN.m"/>
```

Ghi chú:

```text
Giai đoạn đầu: wrap nhẹ quanh thư viện chart có sẵn (ví dụ OxyPlot/LiveCharts) thay vì tự viết engine vẽ.
Giai đoạn sau: viết riêng DiagramChart tối ưu cho biểu đồ kỹ thuật (trục đảo, vùng tô âm/dương).
```

---

# 90. GridSplitter

```xml
<ui:GridSplitter
    Orientation="Vertical"
    ShowGripDots="True"/>
```

Features:

```text
Style mặc định đẹp hơn GridSplitter gốc WPF
Hover/drag state rõ ràng
Hỗ trợ cả Horizontal và Vertical
Double-click để reset về kích thước mặc định (tùy chọn)
```

---

# 91. Expander

Tách riêng khỏi `Card.IsCollapsible` để dùng độc lập trong các layout đơn giản (sidebar, filter panel).

```xml
<ui:Expander
    Header="Thông số nâng cao"
    IsExpanded="False"/>
```

Properties:

```text
Header
IsExpanded
ExpandDirection
Icon
Animation on expand/collapse
```

---

# 92. FilePicker / FolderPicker

```xml
<ui:FilePicker
    SelectedPath="{Binding ExportPath}"
    Filter="Excel Files|*.xlsx"/>
```

```xml
<ui:FolderPicker
    SelectedPath="{Binding ProjectFolder}"/>
```

Tính năng:

```text
Nút "..." mở dialog chọn file/folder chuẩn hệ điều hành
Hiển thị đường dẫn rút gọn khi quá dài
Validate đường dẫn tồn tại
Drag & drop file trực tiếp vào ô nhập (tùy chọn)
```

---

# 93. Hotkey / Keyboard Shortcut Manager

Hữu ích cho ứng dụng kỹ thuật có nhiều thao tác lặp lại (Save, New Element, Zoom Fit...).

```csharp
HotkeyManager.Register("Ctrl+S", SaveCommand);
HotkeyManager.Register("Ctrl+Shift+N", NewElementCommand);
```

Tính năng:

```text
Đăng ký/hủy đăng ký hotkey theo scope (Window/UserControl)
Tránh xung đột hotkey trùng nhau (cảnh báo khi đăng ký trùng)
Hiển thị hotkey trong Tooltip/MenuItem tự động (InputGestureText)
```

---

# 94. Undo/Redo Manager (Core)

Khác với animation ở mục 56, đây là undo/redo cho **dữ liệu** — nền tảng bắt buộc cho phần mềm kỹ thuật có thao tác nhập liệu phức tạp.

```text
CONCOST.UI.Core/
└── UndoRedo/
    ├── IUndoableCommand.cs
    ├── UndoRedoManager.cs
    └── CompositeCommand.cs
```

API:

```csharp
UndoRedoManager.Current.Execute(new ChangeValueCommand(beam, nameof(beam.Width), oldValue, newValue));
UndoRedoManager.Current.Undo();
UndoRedoManager.Current.Redo();
```

Tính năng:

```text
Stack Undo/Redo có giới hạn dung lượng (tránh memory leak)
Gộp nhiều thay đổi liên tiếp thành 1 command (ví dụ kéo Slider) — CompositeCommand
Tích hợp sẵn với NumericUpDown, UnitBox, TextBox (optional, qua behavior)
```

---

# 95. Settings & Preferences Persistence

`ThemeManager` hiện chỉ set theme lúc runtime — cần một service lưu lựa chọn người dùng (Theme, AccentColor, ngôn ngữ, layout) giữa các lần mở ứng dụng.

```text
CONCOST.UI.Core/
└── Settings/
    ├── ISettingsService.cs
    └── JsonSettingsService.cs
```

API:

```csharp
SettingsService.Current.Set("Theme", AppTheme.Dark);
var theme = SettingsService.Current.Get<AppTheme>("Theme");
```

Lưu mặc định dạng JSON tại:

```text
%AppData%/CONCOST/<AppName>/settings.json
```

Tự động áp dụng lại Theme/Accent khi app khởi động lại.

---

# 96. Visual Regression Testing

Bổ sung cho mục 70 (Unit Tests), vì design system cần đảm bảo giao diện không bị "vỡ" âm thầm giữa các version.

```text
tests/CONCOST.UI.VisualTests/
```

Quy trình:

```text
1. Render từng control ở cả Light/Dark theme thành ảnh (offscreen rendering WPF)
2. So sánh pixel-diff với ảnh baseline đã duyệt
3. Fail CI nếu sai khác vượt ngưỡng cho phép
4. Khi thay đổi có chủ đích -> cập nhật baseline kèm review
```

---

# 97. Automated UI Testing

```text
tests/CONCOST.UI.UITests/
```

Công cụ đề xuất: **WinAppDriver** hoặc **FlaUI**.

Kịch bản test tối thiểu:

```text
- Mở Demo App, điều hướng qua từng trang không crash
- Click Button, kiểm tra state đổi đúng
- Nhập giá trị vào NumericUpDown, kiểm tra Min/Max clamp
- Chuyển Light/Dark, kiểm tra không exception
```

---

# 98. Tương thích XAML Designer

Nhiều ControlTemplate chạy tốt lúc runtime nhưng vỡ trong VS Designer/Blend — cần kiểm tra riêng, không chỉ dựa vào Demo App.

Checklist mỗi control trước khi merge:

```text
☐ Hiển thị đúng trong VS XAML Designer (không lỗi "cannot create instance")
☐ DesignTime data/DesignTime.IsDesignTimeCreatable hoạt động nếu control có logic phức tạp
☐ Không throw exception khi binding DataContext = null lúc design-time
```

---

# 99. In ấn & Xuất dữ liệu (Print & Export)

Nhu cầu thực tế của phần mềm kỹ thuật/xây dựng: in bản vẽ, xuất báo cáo, xuất bảng dữ liệu.

```text
Services/
├── PrintService.cs
└── ExportService.cs
```

API:

```csharp
await PrintService.Current.PrintAsync(visualToPrint);
await ExportService.Current.ExportToPdfAsync(dataGrid, "report.pdf");
await ExportService.Current.ExportToExcelAsync(dataGrid, "data.xlsx");
```

Tính năng:

```text
Print Preview control
Export DataGrid -> Excel (không phụ thuộc Microsoft Excel, dùng thư viện thuần .NET)
Export Canvas/ZoomableCanvas -> PDF/PNG (in bản vẽ)
Header/Footer tùy chỉnh khi in (logo công ty, số trang)
```

Lưu ý: giữ đúng mục tiêu ban đầu "Không phụ thuộc Microsoft Excel hoặc Office" — dùng thư viện export thuần .NET (ví dụ ClosedXML, QuestPDF) chứ không interop Office.

---

# 100. Đóng gói, Ký số & Giấy phép

Bổ sung cho mục 71/72 khi phân phối nội bộ hoặc cho đối tác.

```text
- Strong-name signing cho assembly (nếu cần load vào plugin CAD yêu cầu signed assembly)
- Authenticode signing cho file NuGet/installer (tránh cảnh báo SmartScreen khi phân phối ngoài)
- Rà soát license của icon set / font / thư viện chart bên thứ ba trước khi đóng gói (MIT/Apache OK, tránh GPL nếu NuGet phân phối rộng)
- LICENSE riêng rõ ràng: nội bộ CONCOST hay public (MIT/Apache/Proprietary)
```

---

# 101. Cập nhật Bảng Roadmap Tổng Thể

Bổ sung các milestone hạ tầng và control mới vào bảng ở mục 74:

| Milestone | Nội dung | Output |
|---|---|---|
| M0 | Solution + architecture + CI/CD | Project chạy được, pipeline build/test tự động |
| M1 | Design Tokens | Color / Brush / Spacing / Typography |
| M2 | Theme Engine + Settings Persistence | Light / Dark / System, lưu lựa chọn người dùng |
| M3 | Basic Controls | Button / Icon / Card / Divider / Expander |
| M4 | Input Controls | TextBox / ComboBox / Checkbox / Toggle / FilePicker |
| M5 | Numeric & Engineering Controls | NumericUpDown / UnitBox / LengthBox... |
| M6 | Data Controls | DataGrid / List / Tree / GridSplitter |
| M7 | Feedback | Dialog / Toast / Loading |
| M8 | Navigation | Tabs / Menu / Breadcrumb |
| M9 | Window | CONCOSTWindow / TitleBar / Hotkey Manager |
| M10 | Canvas & Chart | ZoomableCanvas / DiagramChart / Print-Export |
| M11 | Localization | i18n, culture-aware numeric formatting |
| M12 | Demo + Docs | Full showcase |
| M13 | Tests + Visual Regression + optimization | Stable, có test tự động UI |
| M14 | NuGet v1.0 (signed) | Release chính thức |

---

# 102. Cập nhật Version Roadmap — Hạng mục bổ sung

Các hạng mục mới nên được lồng vào version roadmap hiện có (mục 75) như sau, không tạo thêm version rời rạc:

```text
v0.1.0  + CI/CD pipeline cơ bản, .editorconfig, analyzer
v0.2.0  + Settings persistence (lưu Theme/Accent)
v0.3.0  + FilePicker, GridSplitter, Expander
v0.4.0  + Undo/Redo Manager (Core), Hotkey Manager
v0.5.0  (giữ nguyên: Dialog/MessageBox/Toast/Snackbar)
v0.6.0  (giữ nguyên: TreeView/ListView/Tabs/Breadcrumb/NavigationView)
v0.7.0  (giữ nguyên: CONCOSTWindow/TitleBar/ContextMenu/Tooltip/Popup)
v0.75.0 + ZoomableCanvas, GridBackground, RulerControl   [MỚI]
v0.8.0  (giữ nguyên: Animations/Behaviors/DragDrop/Advanced validation)
v0.85.0 + Chart/DiagramChart (wrap OxyPlot/LiveCharts)   [MỚI]
         + PrintService, ExportService (PDF/Excel)       [MỚI]
v0.9.0  + Localization (i18n) đầy đủ                     [MỚI]
         + Culture-aware numeric formatting               [MỚI]
         (giữ nguyên: Bug fixes/API cleanup/Performance/Accessibility/Docs)
v0.95.0 + Visual Regression Testing                       [MỚI]
         + Automated UI Testing                           [MỚI]
v1.0.0  Điều kiện release giữ nguyên như mục 75, bổ sung:
         + Assembly signing hoàn tất
         + Localization tối thiểu vi-VN + en-US
         + CI/CD publish NuGet tự động
```

---

# 103. Tầm nhìn hoàn thiện

Với các bổ sung trên, `CONCOST.UI` không chỉ là "Design System + WPF Controls" như định hướng ở mục 81, mà trở thành một nền tảng đầy đủ:

```text
CONCOST.UI (hoàn chỉnh)
    =
Design System (Token/Theme/Style)
+ WPF Controls (Basic/Input/Data/Feedback/Navigation)
+ Engineering Controls (UnitBox/LengthBox/RebarInput...)
+ Canvas & Chart Engine (ZoomableCanvas/DiagramChart)   [MỚI — flagship]
+ MVVM Helpers (RelayCommand/Undo-Redo)                  [MỚI]
+ Dialog / Notification System
+ Print & Export System                                  [MỚI]
+ Localization Engine                                     [MỚI]
+ CI/CD & QA Pipeline (Visual Regression/UI Test)         [MỚI]
+ Settings Persistence                                    [MỚI]
```

Đây là bộ khung đủ để `CONCOST.UI` cạnh tranh trực tiếp với các thư viện thương mại (Syncfusion, DevExpress, Telerik) trong phân khúc hẹp hơn nhưng sâu hơn: **phần mềm kỹ thuật, xây dựng và CAD trên nền WPF**.
