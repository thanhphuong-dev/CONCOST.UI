# CONCOST.UI — Documentation

The documentation is a self-contained static site: open [`index.html`](index.html) in any browser (no build step, no server required).

**Live site:** <https://thanhphuong-dev.github.io/CONCOST.UI/> — published from this folder (`main` → `/docs`) through GitHub Pages. Add `?lang=en` or `?lang=vi` to force a language.

Features: command palette search (`Ctrl K` or `/`), per-page table of contents with reading progress, XAML ↔ C# code inspector with copy, light/dark themes, five accent colours, keyboard navigation and deep links (`index.html#splitbutton`).

**80 pages · 457 documented properties · 56 controls mapped to source · 2 languages (VI / EN).**

### Languages

The wiki ships in Vietnamese and English. The **VI / EN** button in the top bar switches instantly (persisted in `localStorage`), and the language can be forced through the URL:

```text
docs/index.html?lang=en#splitbutton
docs/index.html?lang=vi#splitbutton
```

Vietnamese is the base language (`data/*.js`); English lives in the overlay files under `data/en/`. A page renders in English when its overlay entry exists, otherwise it falls back to Vietnamese, so a half-translated page never breaks the build.

## Page map

### Getting started

| Page | Nội dung |
| --- | --- |
| [Giới thiệu & Cài đặt](index.html#intro) | CONCOST.UI là bộ thư viện giao diện WPF tối ưu cho phần mềm kết cấu, BIM, CAD và ứng dụng desktop kỹ thuật. Thư viện đi kèm hệ thống design tokens tự  |
| [Plugin DLL (Revit/AutoCAD)](index.html#class-library) | Hướng dẫn chuyên sâu kiến trúc phát triển giao diện WPF khi đóng gói dưới dạng Class Library (.DLL) hoặc Plugins cho các phần mềm CAD/BIM chuyên nghiệ |
| [Theme & Design Tokens](index.html#theming) | Toàn bộ bảng màu và phong cách giao diện của CONCOST.UI được quản lý thông qua DynamicResource, cho phép ứng dụng chuyển đổi mượt mà giữa Dark và Ligh |
| [Attached Properties](index.html#attached-props) | Hệ thống thuộc tính đính kèm phong phú của CONCOST.UI cho phép bạn 'thổi hồn' công nghệ hiện đại vào bất kỳ control WPF nguyên bản nào (Button, TextBo |
| [Ví dụ sử dụng](index.html#examples) | Bốn tình huống thực tế ghép nhiều control lại với nhau: form nhập thông số dầm, màn hình bảng khối lượng, panel cấu hình trong plugin Revit và dashboa |
| [FAQ & Troubleshooting](index.html#faq) | Các sự cố thường gặp khi tích hợp CONCOST.UI vào ứng dụng WPF hoặc plugin CAD/BIM, kèm nguyên nhân gốc và cách xử lý. |
| [Chuyển từ WinForms/WPF](index.html#migration) | Bảng ánh xạ từ WinForms / WPF mặc định sang CONCOST.UI, kèm quy trình chuyển đổi 4 bước và chính sách deprecation. |

### Engineering & CAD

| Page | Nội dung |
| --- | --- |
| [Spreadsheet](index.html#spreadsheet) | Bảng tính Excel kỹ thuật hiệu năng cao, tự thiết kế renderer trực tiếp qua DrawingContext (hỗ trợ hàng chục ngàn cells mượt mà 60fps), công thức hàm t |
| [StackedHeaderDataGrid](index.html#stackedheaderdatagrid) | DataGrid tiêu chuẩn xây dựng và dự toán khối lượng chuyên nghiệp, hỗ trợ cấu trúc cây tiêu đề cột gộp đa tầng (Multi-Level Stacked Header Tree), thanh |
| [ZoomableCanvas & Ruler](index.html#cadcanvas) | Không gian vẽ kỹ thuật CAD 2D tương tác mượt mà bằng chuột: Thu phóng vô hạn (Zoom In/Out quanh con trỏ chuột), Kéo lia màn hình (Pan bằng chuột giữa  |
| [ChartLine (nội lực)](index.html#diagramchart) | Vẽ biểu đồ mô-men, lực cắt, lực dọc hoặc độ võng từ một chuỗi giá trị. Toàn bộ biểu đồ kế thừa ChartBase nên dùng chung Datas, Title, Unit, ShowLegend |
| [Charts Suite](index.html#charts) | Bộ biểu đồ vector độ phân giải cao bao gồm ChartBar, ChartLine, ChartPie, ChartRadar và PieControl hiển thị trực quan tỷ trọng khối lượng dự án. |
| [RebarInput](index.html#rebarinput) | Ô nhập liệu cốt thép thông minh dành riêng cho kỹ sư xây dựng. Tự động nhận diện cú pháp thép dầm/cột/móng (vd: 4D20, 2Ø16, 4Φ20, 2x3D18, D10@200), tự |
| [Engineering Boxes](index.html#engineeringboxes) | Bộ 6 hộp nhập đại lượng kỹ thuật chuyên sâu kế thừa từ UnitBox: LengthBox (mm, m), AreaBox (m²), VolumeBox (m³), ForceBox (kN), MomentBox (kN.m), Pres |
| [CompassIndicator](index.html#compassindicator) | La bàn đồ họa chỉ hướng gió và hướng Bắc địa chính (True North / Project North) của dự án công trình, tự động quay kim chỉ hướng theo góc Angle (0 đến |

### Data & Dashboard

| Page | Nội dung |
| --- | --- |
| [DataPager](index.html#datapager) | Phân trang cho DataGrid/ListView: PageIndex, PageSize, TotalItems đều bind two-way và tự ép giá trị hợp lệ. |
| [StatisticCard (KPI)](index.html#statisticcard) | Thẻ hiển thị số liệu quan trọng trên Dashboard, hỗ trợ chỉ số tăng/giảm phần trăm (Trend) và Icon điểm nhấn. |
| [Timeline](index.html#timeline) | Hiển thị chuỗi sự kiện hoặc tiến độ thi công công trình theo trục thời gian tuyến tính với các nút trạng thái Semantic, Vector Icon, và đường nối tự đ |
| [CardCarousel](index.html#cardcarousel) | Bộ điều khiển vòng xoay thẻ 3D Coverflow mượt mà, hỗ trợ tạo phối cảnh chiều sâu không gian (Depth Perspective), gán Z-Index phân tầng, nút điều hướng |
| [PropertyGrid](index.html#propertygrid) | Bảng duyệt và chỉnh sửa thông số đối tượng CAD / BIM chuyên dụng, tự động gom nhóm Category, hỗ trợ editor nội dòng (TextBox, CheckBox, ComboBox, Colo |

### Input & Form

| Page | Nội dung |
| --- | --- |
| [NumericUpDown](index.html#numericupdown) | Bộ nhập giá trị số với 2 nút tăng giảm, hỗ trợ bước nhảy Step, giới hạn Min/Max, định dạng tiền tệ / đơn vị kỹ thuật (mm, m3, kN, MPa). |
| [TextBox](index.html#textbox) | Ô nhập văn bản nâng cao với placeholder mờ, nút xóa nhanh [X], icon tiền tố/hậu tố và ô nhập mật khẩu với nút bật/tắt hiển thị. |
| [PasswordInput](index.html#passwordinput) | Ô nhập mật khẩu có nút hiện/ẩn, icon khoá và placeholder — thay cho việc tự ghép PasswordBox + ToggleButton. |
| [OtpInput](index.html#otpinput) | Ô nhập mã OTP nhiều ô, tự nhảy ô và gộp giá trị — phù hợp xác thực 2 lớp, mã kích hoạt bản quyền. |
| [PinBox](index.html#pinbox) | Bộ nhập mã xác minh bản quyền hoặc OTP từng ô riêng biệt, tự động nhảy con trỏ sang ô tiếp theo và hỗ trợ ẩn mật khẩu. |
| [CheckComboBox](index.html#checkcombobox) | Hộp thoại chọn danh sách nhiều mục với ô CheckBox cho từng dòng, tích hợp tùy chọn 'Chọn tất cả (Select All)' và đếm số lượng đã chọn. |
| [ComboBox](index.html#combobox) | Menu chọn thả xuống hiện đại với mũi tên hoạt ảnh và hỗ trợ bo góc, chế độ tìm kiếm tích hợp. |
| [AutoCompleteTextBox](index.html#autocompletetextbox) | Hộp nhập văn bản thông minh có Popup Dropdown tự động lọc và gợi ý danh sách mã định mức, vật tư, tiêu chuẩn kỹ thuật khi người dùng bắt đầu gõ phím. |
| [SearchBox](index.html#searchbox) | Ô tìm kiếm tích hợp icon kính lúp, nút xóa trắng nhanh (Clear Button) và sự kiện tìm kiếm tức thời SearchTrigger. |
| [FilePicker](index.html#filepicker) | Hộp chọn file tích hợp sẵn nút duyệt Browse với hộp thoại OpenFileDialog / FolderBrowserDialog. |
| [ColorPicker](index.html#colorpicker) | Bộ chọn màu chuyên nghiệp với Canvas 2D Saturation-Value, thanh trượt Hue/Alpha, ô nhập mã màu Hex/RGBA và bảng màu mẫu Presets. |
| [RangeSlider](index.html#rangeslider) | Bộ điều khiển thanh trượt 2 đầu (Dual-Thumb Slider) cho phép kỹ sư chọn nhanh một khoảng giá trị liên tục [StartValue ~ EndValue] để lọc dầm theo tiết |
| [Rating](index.html#rating) | Bộ đánh giá xếp hạng chất lượng thi công hoặc nhà thầu theo số sao tương tác. |
| [Tag / Chip](index.html#tag) | Nhãn trạng thái và phân loại cấu kiện với nút xóa hành động [X], vector icons và các bảng màu trạng thái Semantic. |
| [ToggleSwitch](index.html#toggleswitch) | Công tắc gạt bật/tắt thiết lập với chuyển động hoạt ảnh mượt mà. |

### Navigation

| Page | Nội dung |
| --- | --- |
| [NavigationView](index.html#navigationview) | Thanh điều hướng dạng danh sách item có icon, badge trạng thái và SelectedItem hai-way — bản gọn hơn SideMenu cho panel hẹp. |
| [SideMenu](index.html#sidemenu) | Menu điều hướng dọc hiện đại, hỗ trợ mở rộng/thu gọn icon-only, hiển thị Badge số lượng thông báo và phân nhóm cấp mục. |
| [DrawerMenu](index.html#drawermenu) | Thanh điều hướng ngăn kéo thông minh lấy cảm hứng từ WPFDevelopers, hỗ trợ thu gọn/mở rộng động (Expand/Collapse Animation mượt mà), thanh Header chứa |
| [Breadcrumb](index.html#breadcrumb) | Đường dẫn điều hướng vị trí trong mô hình công trình (vd: Tòa Nhà A > Tầng 15 > Khu Vực Cốt Thép Dầm). |
| [Pagination](index.html#pagination) | Bộ điều khiển phân trang dữ liệu cho các bảng khối lượng lớn, hỗ trợ nhảy trang nhanh và cấu hình PageSize. |
| [StepBar](index.html#stepbar) | Thanh hiển thị tiến trình công việc nhiều bước (Wizard Steps) với các trạng thái Đang Chờ (Waiting), Đang Làm (In-Progress), Hoàn Thành (Finished) và  |
| [Transfer](index.html#transfer) | Bộ chuyển dữ liệu 2 khung đối xứng (Shuttle List) hỗ trợ lọc tìm kiếm tức thì, chọn tất cả và di chuyển hàng loạt cấu kiện qua lại. |
| [TabControl](index.html#tabcontrol) | Bộ chuyển tab điều hướng nội dung với viền chỉ báo vị trí mượt mà (Underline indicator). |

### Feedback & Status

| Page | Nội dung |
| --- | --- |
| [Dialog](index.html#dialog) | Hộp thoại Popup hiện đại với hiệu ứng Zoom & Fade, làm mờ lớp nền (Backdrop Dimming), hỗ trợ đầy đủ Header, Action Footer, và kéo thả di chuyển. |
| [Drawer](index.html#drawer) | Bảng điều khiển trượt từ 4 cạnh màn hình (Left, Right, Top, Bottom) kèm hiệu ứng làm mờ màn chắn (Backdrop Mask) để hiển thị chi tiết đối tượng BIM ho |
| [Growl (Toast)](index.html#growl) | Hệ thống thông báo đẩy dạng Toast xếp chồng thông minh ở góc màn hình, hỗ trợ tự động biến mất (Auto-Dismiss), thanh đếm thời gian và các mức độ Succe |
| [Loading Indicators](index.html#loadings) | Bộ loading indicator: 4 kiểu của LoadingIndicator và 5 control animation độc lập (CycleLoading, RingLoading, RollLoading, WaitLoading, LoadingCircle,  |
| [LoadingOverlay](index.html#loadingoverlay) | Lớp phủ loading chặn tương tác, có tiêu đề, mô tả phụ và thanh tiến trình — dùng cho tác vụ dài như solve, import IFC, sync cloud. |
| [Skeleton (Shimmer)](index.html#skeleton) | Placeholder shimmering thay cho vòng xoay khi tải dữ liệu — giữ nguyên bố cục để tránh layout shift. |
| [EmptyState](index.html#emptystate) | Giao diện minh họa thông minh hiển thị khi danh sách hoặc bảng dữ liệu không có phần tử nào. |
| [Watermark](index.html#watermark) | Lớp phủ hình mờ (DRAFT, CONFIDENTIAL, COPYRIGHT) chống sao chép tài liệu bản vẽ và báo cáo PDF. |

### Layout & Shell

| Page | Nội dung |
| --- | --- |
| [SplitButton](index.html#splitbutton) | Nút hành động chính kèm menu thả xuống — 19 thuộc tính và 13 style variant, thay thế hoàn toàn combo Button + ContextMenu tự chế. |
| [Expander](index.html#expander) | Khối nội dung thu gọn/mở rộng với 18 style variant (Clean, Card, Filled, Round, Small, Primary/Success/Warning/Danger/Info, hỗ trợ 4 hướng mở). |
| [Card & Border](index.html#card) | Hệ thống thẻ chứa nội dung tiêu chuẩn hóa với độ bo góc `CornerRadius=10`, hiệu ứng bóng nổi `Depth2/Depth3` và huy hiệu thông báo. |
| [Badge](index.html#badge) | Huy hiệu số lượng / trạng thái, dùng độc lập hoặc gắn lên bất kỳ ButtonBase nào qua attached:ButtonElement.Badge. |
| [Divider](index.html#divider) | Đường kẻ phân chia khu vực bố cục với tiêu đề nhãn văn bản ở giữa. |
| [AdaptivePanel](index.html#adaptivepanel) | ContentControl tự chọn layout theo bề rộng khả dụng qua danh sách Breakpoint — dùng để chuyển 3 cấp Compact / Normal / Wide mà không cần code-behind. |
| [CONCOSTWindow](index.html#concostwindow) | Cửa sổ WPF với chrome tuỳ biến (title bar, nút min/max/close trong template) — kéo cửa sổ, double-click để maximize đều xử lý sẵn. |

### Effects & Decoration

| Page | Nội dung |
| --- | --- |
| [Ripple](index.html#ripple) | Hiệu ứng gợn sóng Material khi click, dùng độc lập hoặc gắn vào bất kỳ phần tử nào qua attached RippleElement. |
| [SnowCanvas](index.html#snowcanvas) | Khung vẽ hạt tuyết rơi chuyển động GPU siêu mượt (lấy cảm hứng từ WPFDevelopers), hỗ trợ tùy chỉnh mật độ số lượng hạt, bán kính Min/Max, tốc độ rơi,  |
| [Sprite](index.html#sprite) | Nút trợ lý nổi tương tác thông minh (Assistive Touch / Floating Action Button) lấy cảm hứng từ HandyControl. Sprite luôn nổi phía trên toàn bộ giao di |
| [CompareSlider](index.html#compareslider) | Thanh trượt chia đôi màn hình trực quan cho phép so sánh sự thay đổi giữa 2 hình ảnh bản vẽ thiết kế (Rev 01 vs Rev 02), phối cảnh 3D Render trước/sau |
| [Magnifier](index.html#magnifier) | Tròng kính lúp phóng to tọa độ điểm ảnh độ nét cao (lấy cảm hứng từ HandyControl & WPFDevelopers). Khi rê chuột vào khu vực bản vẽ CAD hoặc mặt cắt dầ |
| [Icons Gallery](index.html#icons) | Kho hơn 120+ vector icons độ phân giải vô cực được tối ưu hóa cho ngành kỹ thuật, xây dựng, CAD và BIM. |

### Basic Controls

| Page | Nội dung |
| --- | --- |
| [Button](index.html#button) | Hệ thống nút bấm đa dạng với các kiểu dáng (Primary, Secondary, Success, Outline, Danger), tích hợp vòng xoay Loading Spinner và gắn Vector Icon trực  |
| [ToggleButton](index.html#togglebutton) | Bộ ba điều khiển chọn trạng thái: ToggleButton 2 trạng thái, RadioButton chọn một và CheckBox chọn nhiều. |
| [CheckBox](index.html#checkbox) | CheckBox với 3 biến thể (mặc định, dạng button, dạng button outline), hỗ trợ trạng thái ba trị và label có title/required qua attached properties. |
| [RadioButton](index.html#radiobutton) | RadioButton với 4 biến thể, gồm Segment cho nhóm lựa chọn dạng segmented control (rất hợp cho bộ lọc chế độ xem). |
| [Slider](index.html#slider) | Slider được restyle với thumb và repeat button riêng, giữ nguyên toàn bộ API của WPF Slider. |
| [ProgressBar](index.html#progressbar) | Thanh tiến trình xác định/vô định, thanh trượt điều chỉnh giá trị liên tục và khung cuộn giao diện tự động ẩn thanh cuộn khi không rê chuột. |
| [ScrollViewer](index.html#scrollviewer) | ScrollViewer với scrollbar tự ẩn và bộ attached ScrollViewerHelper (quán tính, tốc độ cuộn, nút back-to-top). |
| [ListBox](index.html#listbox) | ListBox restyle với item hover/selected theo theme, kèm biến thể Card cho danh sách dạng khối. |
| [ListView](index.html#listview) | ListView dùng cho danh sách nhiều cột nhẹ (không cần sort/filter như DataGrid). |
| [DataGrid](index.html#datagrid) | Bảng dữ liệu kỹ thuật kết cấu hỗ trợ sọc vằn (Zebra Striping), cố định dòng, lựa chọn nhiều dòng và hỗ trợ trọn vẹn cả Dark/Light theme. |
| [TreeView](index.html#treeview) | Cây thư mục phân cấp dự án hiển thị các tầng, khu vực và từng cấu kiện nhỏ. |
| [GroupBox](index.html#groupbox) | GroupBox với 7 biến thể, dùng để nhóm các trường nhập theo chủ đề kỹ thuật (hình học, vật liệu, tải trọng). |
| [Menu & ContextMenu](index.html#menu) | Menu ngang và ContextMenu được restyle với 4 biến thể, hỗ trợ submenu nhiều cấp và icon. |
| [Typography](index.html#textblock) | Thang typography của design system: H1–H4, Body, Caption, Code và các màu ngữ nghĩa — dùng thay vì set FontSize thủ công. |

### Time

| Page | Nội dung |
| --- | --- |
| [Clock](index.html#clock) | Đồng hồ tròn phong cách Material để chọn Giờ / Phút / Giây. Cần chọn cả ngày thì dùng CalendarWithClock — trang riêng trong nhóm Thời gian. |
| [CalendarWithClock](index.html#calendarwithclock) | Lịch chọn ngày kết hợp đồng hồ chọn giờ/phút/giây — dùng cho mốc nghiệm thu, thời điểm quan trắc, lịch thi công. |

## How this documentation is maintained

| File | Role |
| --- | --- |
| `data/legacy.js` | Original control pages (prose + XAML). |
| `data/guides.js` | FAQ and migration guides. |
| `data/examples.js` | Cookbook page with complete, combined examples. |
| `data/custom.js` | Newer control pages and C# overrides. |
| `data/basic.js` | Styling-only basic controls + attached property tables. |
| `data/enrich.js` | Remaining property tables and C# snippets. |
| `data/auto.generated.js` | **Generated** property tables, C# samples and English property descriptions. Do not edit. |
| `data/nav.js` | Sidebar order, icons, badges (VI + EN labels). |
| `data.js` | Merges every dataset into `DOCS_DATA`. |
| `data/en/ui.js` | UI chrome strings (VI + EN), badge labels and the `DOCS_EN` container. |
| `data/en/*.js` | English overlay per page: title, description, section text, callouts and tables. |
| `app.js` / `style.css` / `index.html` | Application shell, design system and language switch. |

### Regenerate the generated parts

```bash
python docs/tools/build-props.py     # property tables + C# samples + EN property text from src/CONCOST.UI/**/*.cs
python docs/tools/check-xaml.py      # validate every XAML snippet against the real API
node   docs/tools/build-index.js     # rebuild this page map
```

Run all three after adding or renaming a `DependencyProperty`; the checker must report **0 issues** before committing.

### Adding a page

1. Add the page object to the matching `data/*.js` file (`title`, `badge`, `desc`, `sections[]` with `xaml`/`csharp`).
2. Add the English strings to the matching `data/en/*.js` file (same `sections` index order).
3. Register it in `data/nav.js` — the order there drives the sidebar and prev/next links.
4. Run `build-props.py` if the page documents a control that is new to `PAGE_CLASS`, then `check-xaml.py` and `build-index.js`.

## Related documents

- [Getting Started](GettingStarted.md) — integration, theming, plugin hosting, build.
- [Migration Guide](MigrationGuide.md) — WinForms/WPF mapping and deprecation policy.
- [Roadmap](roadmap/) — active roadmap (revision 3) and archived plans.
- [Changelog](../CHANGELOG.md).
