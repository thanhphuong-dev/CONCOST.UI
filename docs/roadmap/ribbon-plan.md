# Kế hoạch triển khai `Ribbon`

> Trạng thái: **R0–R6 đã xong — toàn bộ 22/22 control đã ship trong 1.1.0**, sớm hơn kế hoạch chia 3 bản (1.1/1.2/1.3) bên dưới. Bản này map 1-1 với 2 tài liệu bạn gửi:
> checklist hành vi A–F và danh sách **22 control** kèm ưu tiên P0/P1/P2.
> Tên class dùng đúng như bảng của bạn (`RibbonTabItem`, `RibbonKeyTip`, `RibbonGroupLauncher`…).

## 0. Nguyên tắc chia bản

Ưu tiên của bạn quyết định thứ tự phát hành, không phải sở thích của em:

| Bản | Nội dung | Điều kiện dùng được |
| --- | --- | --- |
| **1.1.0** | Toàn bộ **P0** (8 control) + layout engine + theme + command | Dựng được ribbon thật cho app CAD/BIM |
| **1.2.0** | Toàn bộ **P1** (11 control) | Ngang tầm ribbon của Office ở mức thường dùng |
| **1.3.0** | Toàn bộ **P2** (5 control) | Hoàn thiện: tìm lệnh, KeyTip, color picker |

> ⚠️ **Thực tế đã lệch kế hoạch này:** R4–R6 làm liền mạch nên cả **22/22 control** (P0+P1+P2) đều có mặt trong **1.1.0**. Không còn bản 1.2.0/1.3.0 riêng cho ribbon — hai số đó giữ lại cho các hạng mục khác của `roadmap-v3.md`.

---

## 0b. Tiến độ

| Phase | Nội dung | Trạng thái |
| --- | --- | --- |
| R0 | Khung: `Ribbon`, `RibbonTabItem`, `RibbonGroup`, `RibbonButton`, `RibbonSeparator`, `RibbonGroupPanel`, token, style | ✅ |
| R1 | Thang thu gọn Full → Medium → Small → gộp group, flyout group, tự mở lại khi rộng ra | ✅ |
| R2 | `RibbonToggleButton`, `RibbonRadioButton`, `RibbonSplitButton`, `RibbonDropDownButton`, `RibbonMenu`, `RibbonMenuItem`, `RibbonMenuSeparator` | ✅ |
| R3 | Dark theme, DPI 150 %, tài liệu 2 trang VI/EN + bảng thuộc tính | ✅ |
| R4 | `RibbonQuickAccessToolBar` (+`RibbonQuickAccessButton`), `RibbonApplicationMenu`, `RibbonContextualTabGroup` (+`RibbonTabStripPanel`), `RibbonToolTip` | ✅ |
| R5 | `RibbonCheckBox`, `RibbonTextBox`, `RibbonComboBox`, `RibbonGallery` (+`RibbonGalleryItem`), `ColumnSpan`, `IsFixedSize` | ✅ |
| R6 | `RibbonKeyTip` (+`KeyTipService`), `RibbonColorPicker`, `RibbonSearchBox`, `RibbonGroupLauncher`, `RibbonState`/`RibbonStateStore`, automation peer | ✅ |
| Release | tag + release + nupkg + zip demo | ✅ **1.1.0** (22/22 control, 291 test xanh) |

Ghi chú kỹ thuật đã gặp (để lần sau không lặp lại):
- `RibbonGroupPanel` là `ItemsPanel` nên `TemplatedParent` là ItemsControl — phải tìm group bằng cách đi ngược visual tree.
- Đổi `SizeMode` phải `InvalidateMeasure()` **panel**, không chỉ group.
- Template phải trigger theo `EffectiveSize` (không phải `Size`), nếu không nút Large bị hạ size vẫn vẽ kiểu Large.
- Key tip vẽ trong overlay `PART_KeyTipLayer` của chính ribbon, **không** dùng `AdornerLayer` — ribbon lồng trong popup hoặc panel cuộn vẫn đúng chỗ, và không cần `AdornerDecorator` ở host.
- Định vị badge phải dùng `TransformToVisual(layer)`, **không** `TransformToAncestor(layer)`: layer là *anh em* của tab strip và các group, không phải tổ tiên của chúng — dùng sai thì mọi badge dồn về (0,0) và trông như "chỉ có một badge".
- `UIElement.IsVisible` là `false` khi control chưa có presentation source, nên test cho key tip phải host ribbon trong `Window` thật; nếu chỉ đo trong `Border` thì session không tạo badge nào.
- Đổi tab bằng key tip phải build lại badge **hai lần**: ngay lập tức cho dải tab, và một lần nữa ở `DispatcherPriority.Loaded` vì nội dung tab mới chỉ được tạo trong lượt layout kế tiếp.
- Badge cho nút mở hộp thoại của group neo ở góc **dưới-phải** (chỗ mũi tên), nếu không nó đè lên badge của item đầu tiên trong group.
- `KeyTip` là attached property của `KeyTipService`, mỗi control phơi lại bằng `public static readonly DependencyProperty KeyTipProperty = KeyTipService.KeyTipProperty;` — `build-props.py` phải hiểu dạng alias này thì bảng thuộc tính mới có `KeyTip`.
- Không thể đo lại con trong cùng một pass: mỗi pass chỉ leo **một bậc**, và phải đẩy pass kế tiếp qua `Dispatcher.BeginInvoke`.
- Flyout dùng `MinWidth` = bề rộng group để khớp panel.
- **Resource có key trong `Generic.xaml` KHÔNG tra được bằng `SetResourceReference`/`FindResource`** — chỉ style ngầm định (theo `DefaultStyleKey`) mới áp tự động. Muốn có style riêng thì tạo class riêng: `RibbonQuickAccessButton`.
- Trong `Generic.xaml`, `BasedOn="{StaticResource …}"` chỉ thấy key khai báo **phía trên** nó — chèn style mới ở sai chỗ sẽ ném `Cannot find resource named …` lúc chạy, không phải lúc build.
- `ItemsPanel` mặc định là `StackPanel` **dọc** — QAT phải khai báo `ItemsPanel` ngang, nếu không các nút xếp chồng và bị cắt.
- `ItemsPresenter` đặt panel con theo `HorizontalAlignment` của chính panel; để `Stretch` thì ListBox căn giữa dải tab → phải đặt `HorizontalAlignment="Left"`.
- `ToolTip` không nhận cha logic/visual → muốn render nó ra ảnh phải mở bằng `IsOpen = true` rồi mới `RenderTargetBitmap.Render`.
- Dải tab ngữ cảnh vẽ bằng `OnRender` (không phải child) vì `ItemsPanel` chỉ được chứa item container.
- `Generic.xaml` có **style ngầm định cho `TextBlock`** đặt `Foreground = TextPrimary`. Setter của style thắng kế thừa, nên mọi `TextBlock` trong `DataTemplate` **không nhận** màu chữ từ `ListBoxItem`/`Button` cha — phải bind tường minh.
- Style ngầm định **áp cả vào control sinh ra trong `ControlTemplate`**. `ComboBox` dựng `ToggleButton` bên trong nên nó hưởng `CONCOST.ToggleButton.Base` (`MinHeight` 34, `Padding` 14,6): hộp combo cao 20 px bị toggle 34 px đè, mất viền trên/dưới và mũi tên tụt xuống đáy. Mọi toggle trong template phải đặt `Style="{x:Null}"`.
- `WrapPanel.ItemWidth` trong `ItemsPanelTemplate` **không** bind được `AncestorType` tới control cha khi list nằm trong `Popup` (visual tree của popup tách rời). Cách chắc ăn: đặt `Tag` của `ListBox` = gallery rồi bind `Tag.ItemWidth` qua `AncestorType=ListBox`.
- Một `RibbonItem` rộng hơn 1 cột mà đứng sau một cột đang dở sẽ tự tìm dải cột trống kế tiếp; item sau nó vẫn ở lại cột đó nên hai ô nhập xếp chồng, nhóm không phình ngang.

---

## 1. Danh sách 22 control

| STT | Control | Base / cách làm | Ưu tiên | Bản |
| --- | --- | --- | --- | --- |
| 1 | `Ribbon` | `Control` — chọn tab, minimized, phân bổ không gian, host QAT + KeyTip layer | P0 | 1.1.0 |
| 2 | `RibbonTabItem` | `HeaderedContentControl` — `Header`, `HeaderTemplate`, `Icon`, `IsSelected`, `IsContextual` | P0 | 1.1.0 |
| 3 | `RibbonGroup` | `HeaderedItemsControl` — `Header`, `CollapsePriority`, `IsCollapsed`, chứa `RibbonGroupPanel` | P0 | 1.1.0 |
| 4 | `RibbonButton` | `RibbonItem` — Large/Medium/Small, icon + label | P0 | 1.1.0 |
| 5 | `RibbonToggleButton` | `RibbonItem` — `IsChecked` | P0 | 1.1.0 |
| 6 | `RibbonDropDownButton` | `RibbonItem` — mở menu, không có hành động chính | P0 | 1.1.0 |
| 7 | `RibbonSplitButton` | `RibbonItem` — **2 vùng hit-test độc lập** (nút chính / mũi tên) | P0 | 1.1.0 |
| 8 | `RibbonSeparator` | `Control` — vạch ngăn dọc trong group | P0 | 1.1.0 |
| 9 | `RibbonQuickAccessToolBar` | `ItemsControl` — ghim/bỏ ghim, `ContextMenu`, lưu state | P1 | 1.2.0 |
| 10 | `RibbonApplicationMenu` | `RibbonDropDownButton` mở rộng — menu File dạng panel, **không** phải backstage toàn màn hình | P1 | 1.2.0 |
| 11 | `RibbonCheckBox` | Wrapper `CheckBox` sẵn có + template ribbon (icon 16, label bên phải) | P1 | 1.2.0 |
| 12 | `RibbonRadioButton` | `RibbonToggleButton` + `GroupName` (tự bỏ chọn cùng nhóm) | P1 | 1.2.0 |
| 13 | `RibbonComboBox` | Wrapper `ComboBox` + `CheckComboBox` sẵn có | P1 | 1.2.0 |
| 14 | `RibbonTextBox` | Wrapper `TextBox` sẵn có + template ribbon | P1 | 1.2.0 |
| 15 | `RibbonGallery` | `ItemsControl` + `ScrollViewer` — lưới ảnh, nhóm, nút mở rộng | P1 | 1.2.0 |
| 16 | `RibbonContextualTabGroup` | `HeaderedItemsControl` — tab màu ngữ cảnh (Modify/Edit/Sketch), ẩn/hiện theo điều kiện | P1 | 1.2.0 |
| 17 | `RibbonToolTip` | `ToolTip` mở rộng — tên lệnh + mô tả + phím tắt + ảnh | P1 | 1.2.0 |
| 18 | `RibbonMenuItem` | `MenuItem` sẵn có + template ribbon — menu lồng, separator, menu động | P1 | 1.2.0 |
| 19 | `RibbonColorPicker` | Wrapper `ColorPicker` sẵn có — bảng màu trong dropdown | P2 | 1.3.0 |
| 20 | `RibbonSearchBox` | Wrapper `SearchBox` sẵn có — tìm lệnh theo label + KeyTip | P2 | 1.3.0 |
| 21 | `RibbonKeyTip` | `Adorner` + service — overlay khi nhấn `Alt` | P2 | 1.3.0 |
| 22 | `RibbonGroupLauncher` | Nút nhỏ góc dưới-phải group → mở dialog cấu hình nhóm | P2 | 1.3.0 |

**Điểm quan trọng về chi phí:** 6 control P1/P2 (`CheckBox`, `TextBox`, `ComboBox`, `ColorPicker`,
`SearchBox`, `MenuItem`) là **wrapper mỏng** quanh control đã có trong thư viện — chỉ viết template
ribbon-sized, không viết lại logic. Nhờ vậy 22 control không phải 22 lần công việc.

---

## 2. Cây phân cấp

```
Ribbon (Control)
├── RibbonQuickAccessToolBar (P1)
├── RibbonApplicationMenu (P1)
├── dải tab
│   ├── RibbonTabItem (P0)
│   └── RibbonContextualTabGroup (P1)  ← chứa RibbonTabItem, có màu ngữ cảnh
│       └── RibbonGroup (P0)
│           ├── RibbonGroupPanel (Panel — layout engine)
│           ├── RibbonButton / ToggleButton / DropDownButton / SplitButton (P0)
│           ├── RibbonCheckBox / RadioButton / ComboBox / TextBox / Gallery (P1)
│           ├── RibbonColorPicker / SearchBox (P2)
│           ├── RibbonSeparator (P0)
│           └── RibbonGroupLauncher (P2)
└── RibbonKeyTip layer (Adorner, P2)
```

`RibbonItem` là base abstract chung: `Command`, `CommandParameter`, `CommandTarget`, `Icon`,
`LargeIcon`, `SmallIcon`, `Label`, `KeyTip`, `ToolTip`, `Size`, `Id`.

---

## 3. Layout engine — phần khó nhất

### 3.1 Lưới 3 hàng của một group

Cao cố định theo token `CONCOST.Ribbon.GroupHeight` (mặc định 92 px) + 20 px header:

| Size | Số hàng | Ô | Nội dung |
| --- | --- | --- | --- |
| `Large` | 3 (cả cột) | 64 × 92 | icon 32 trên, label dưới |
| `Medium` | 2 | 64 × 61 | icon 20 + label bên phải |
| `Small` | 1 | 64 × 30 | icon 16 + label bên phải |

Xếp cột: `Large` chiếm trọn 1 cột; cột mới chứa 1 `Medium` (2 hàng) + 1 `Small` (1 hàng), hoặc 3 `Small`.
Cột thiếu hàng thì để trống — không kéo giãn (giống Office).

### 3.2 Thang thu gọn

```
0  Large đầy đủ
1  hạ toàn bộ Large → Medium
2  hạ Medium → Small
3  gộp từng group thành 1 nút popup, theo CollapsePriority tăng dần
4  tab overflow (nút ») khi không đủ chỗ cho tab
5  Minimized: chỉ còn dải tab; click tab → popup
```

`CollapsePriority` nhỏ = bị gộp sớm. `CollapsePriority="0"` = **không bao giờ** gộp.
Group có `RibbonGroupLauncher` thì nút launcher vẫn truy cập được cả khi group đã gộp (nằm trong popup).

### 3.3 Chống layout loop

- `MeasureOverride` là **hàm thuần** của `(items, availableSize, SizeMode)` — không gọi `InvalidateMeasure`.
- Cache `DesiredWidth` theo `(SizeMode, DPI, font)`; xoá cache khi DPI/font/item đổi.
- Tràn do rounding → `ClipToBounds`, log `Debug`, **không** đo lại.
- Test: 200 chiều rộng 400–2000 px, assert `Measure` mỗi group ≤ 2 lần.

---

## 4. Command model

- `Command` + `CommandParameter` + `CommandTarget`; `CanExecuteChanged` → `CoerceValue(IsEnabled)`.
- `Command == null` → `IsEnabled` do người dùng set.
- `RibbonSplitButton`: `ClickCommand` cho vùng trái, `Items` cho vùng phải — hit-test tách rời.
- `RibbonToggleButton` / `RibbonRadioButton`: `IsChecked`, `GroupName` (radio tự loại trừ trong cùng group).
- `InputGestureText` chỉ **hiển thị**; không tự đăng ký phím tắt (tránh tranh chấp với Revit/AutoCAD).

---

## 5. Theme & token

Dictionary mới `Themes/Tokens/Ribbon.xaml`, merge trong `ThemeResources.xaml`:

```
CONCOST.Ribbon.GroupHeight        92
CONCOST.Ribbon.TabHeight          34
CONCOST.Ribbon.LargeIconSize      32
CONCOST.Ribbon.MediumIconSize     20
CONCOST.Ribbon.SmallIconSize      16
CONCOST.Ribbon.ItemCornerRadius   6
CONCOST.Ribbon.GroupGap           6
```

Không thêm màu mới — dùng lại `CONCOST.Brush.Surface / SurfaceHover / SurfacePressed / Border /
TextPrimary / TextSecondary / Primary`, nên Ribbon tự đúng ở cả 8 preset kể cả `Brutal`.

6 trạng thái mỗi item: `Normal · Hover · Pressed · Checked · Disabled · Focused`, viết hoàn toàn
trong `ControlTemplate.Triggers` (không code-behind đổi màu). Template `TemplateBinding` cho
`Background`, `BorderBrush`, `BorderThickness`, `Padding`, `CornerRadius`, `IconSize`.

---

## 6. Keyboard, KeyTip, Accessibility

- `Alt` → `RibbonKeyTip` overlay; gõ ký tự lọc theo tiền tố; khớp duy nhất → kích hoạt.
- `KeyTip` để trống thì tự sinh từ `Label` (ký tự chưa dùng).
- Ribbon minimized: KeyTip chọn tab → mở popup → mới chọn item.
- `←/→` giữa tab, `Tab` giữa item, `↓` mở menu, `Esc` đóng popup và **trả focus** về phần tử đã mở.
- AutomationPeer cho `Ribbon` (TabControl), `RibbonTabItem` (SelectionItem), `RibbonButton` (Invoke),
  `RibbonToggleButton` (Toggle); `Name` = Label, `HelpText` = mô tả tooltip, `AcceleratorKey` = phím tắt.

---

## 7. Lưu cấu hình

```csharp
public sealed class RibbonState
{
    public string? SelectedTabId { get; set; }
    public bool IsMinimized { get; set; }
    public List<string> QuickAccessItemIds { get; set; } = new();
    public Dictionary<string, int> GroupSizes { get; set; } = new();
}
```

`Ribbon.CaptureState()` / `ApplyState(state)`; helper `RibbonStateStore.Save/Load` (JSON,
`System.Text.Json` sẵn có). **Thư viện không tự ghi file** — app quyết định đường dẫn.

---

## 8. Kế hoạch phase

| Phase | Nội dung | Checklist | Deliverable | Ước lượng |
| --- | --- | --- | --- | --- |
| **R0 — Khung** | `Ribbon`, `RibbonTabItem`, `RibbonGroup`, `RibbonButton`, `RibbonSeparator`; token; style tối thiểu; harness render | A1–A3, A6, A8, C1, D1 | 5 class + demo chạy được + ảnh render | 1,5 ngày |
| **R1 — Layout** | `RibbonGroupPanel`, 3 hàng, thang thu gọn 5 bước, tab overflow, popup khi minimized | **B1–B8**, A4, A5 | Panel + 8 unit test + ảnh render 5 chiều rộng | 3 ngày |
| **R2 — Command** | Toggle, Radio, DropDown, Split (2 vùng), menu lồng; CanExecute → IsEnabled | C1–C8 | 4 control + test command sync | 2,5 ngày |
| **R3 — Theme + release 1.1.0** | 6 trạng thái, DPI 100–200%, đổi theme runtime, 8 preset; docs 6 trang; release | **D1–D8**, A7, F2 | ảnh 2 theme × 3 DPI + docs + **1.1.0** | 2 ngày |
| **R4 — P1 nhóm 1** | QAT (ghim/bỏ ghim, context menu), `RibbonApplicationMenu`, `RibbonToolTip`, `RibbonContextualTabGroup` | E3–E5, A6, E2 | 4 control + demo | 2,5 ngày |
| **R5 — P1 nhóm 2** ✅ | `RibbonCheckBox`, `RibbonRadioButton`, `RibbonComboBox`, `RibbonTextBox`, `RibbonGallery`, `RibbonMenuItem` | C6, C7 | 6 control (5 là wrapper) | 2,5 ngày |
| **R6 — P2 + bền hoá** ✅ | `RibbonKeyTip`, `RibbonColorPicker`, `RibbonSearchBox`, `RibbonGroupLauncher`, `RibbonState`; memory/perf/a11y | **E1, E6–E8**, **F1–F8** | 4 control + test suite + docs + **1.3.0** | 3 ngày |

**Tổng ≈ 17 ngày công** (~3,5 tuần làm thật). Mốc dùng được: **sau R3 → 1.1.0**.

---

## 9. Kiểm thử

| Loại | Cách | Áp cho |
| --- | --- | --- |
| Unit test (xunit) | Layout là hàm thuần: `(items, width)` → số cột, group bị gộp, SizeMode cuối | B1–B5, F2 |
| Command sync | Fake `ICommand` bật/tắt `CanExecute` → assert `IsEnabled` | C2 |
| Render harness | PNG ở 5 chiều rộng × 3 DPI × 2 theme | A, B, D |
| Memory | Thêm/xoá 500 tab, `GC.Collect`, `WeakReference` phải chết | F1 |
| Keyboard matrix | Alt → KeyTip → tab → menu → Esc → focus đúng chỗ | E1, E6, E8 |
| Accessibility | Accessibility Insights: mọi item có Name + ControlType | F7 |
| Docs | `check-xaml.py` 0 lỗi; `build-props.py` sinh bảng ~60 DP | — |

---

## 10. Rủi ro

| Rủi ro | Mức | Giảm thiểu |
| --- | --- | --- |
| Layout loop / nhấp nháy khi resize | **Cao** | Hàm thuần + cache + cấm `InvalidateMeasure`; test 200 chiều rộng |
| KeyTip đụng Alt của IME tiếng Việt | TB | Chỉ bắt khi `Keyboard.Modifiers == Alt` và không có composition; test UniKey |
| Popup đóng ngoài ý muốn khi mở ComboBox/Gallery/menu con (E7) | TB | `StaysOpen=true` + đóng theo `LostKeyboardFocus` có kiểm tra cây |
| `RibbonGallery` phình scope (P1 nhưng nặng nhất) | TB | Làm bản tối giản ở R5: lưới + scroll + nhóm, chưa cần filter/search |
| Per-monitor DPI (F5) cần manifest của **app** | TB | Ghi rõ trong docs: app cần `PerMonitorV2`; thư viện chỉ phản ứng đúng |
| 60+ DP mới làm phình docs | Thấp | Sinh tự động, không viết tay |

---

## 11. File sẽ tạo / sửa

**Tạo** — `src/CONCOST.UI/Controls/Ribbon/`: 22 file control + `RibbonItem.cs` (base) +
`RibbonGroupPanel.cs` + `RibbonState.cs` + `RibbonStateStore.cs`;
`src/CONCOST.UI/Themes/Tokens/Ribbon.xaml`;
`samples/CONCOST.UI.Showcase/ShowcaseViewModel.RibbonDemos.cs`;
`docs/data/ribbon.js` + `docs/data/en/ribbon.js`;
`tests/CONCOST.UI.Tests/RibbonLayoutTests.cs`, `RibbonCommandTests.cs`, `RibbonStateTests.cs`.

**Sửa** — `Themes/Generic.xaml` (+~1200 dòng style), `Themes/ThemeResources.xaml` (merge token),
`ShowcaseViewModel.cs` (đăng ký demo), `docs/data/nav.js` (nhóm Ribbon + 8 trang),
`docs/tools/build-props.py`, `docs/tools/check-xaml.py`, `CHANGELOG.md`, `Directory.Build.props`.

---

## 12. Cần bạn chốt

| # | Câu hỏi | Đề xuất |
| --- | --- | --- |
| 1 | `RibbonApplicationMenu` là **menu panel** (như Office 2010 File menu) hay **backstage toàn màn hình** (Office 2013+)? | Menu panel — rẻ hơn nhiều, đủ dùng |
| 2 | `RibbonGallery` bản 1.2.0 chỉ cần lưới + scroll + nhóm, hay phải có filter/search ngay? | Chỉ lưới + scroll + nhóm |
| 3 | Có cần Ribbon tự đăng ký phím tắt toàn cục không? | Không, chỉ hiển thị `InputGestureText` |
| 4 | QAT lưu vào đâu? | Thư viện chỉ cấp `CaptureState/ApplyState` + helper JSON, app tự gọi |
| 5 | Release đúng như bảng mục 0 (1.1.0 = P0, 1.2.0 = P1, 1.3.0 = P2)? | Đúng vậy |

Chốt 5 câu trên (hoặc "theo em") là em bắt đầu **R0** ngay.
