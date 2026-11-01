# CONCOST.UI — Documentation

The documentation is a self-contained static site: open [`index.html`](index.html) in any browser (no build step, no server required).

**Live site:** <https://thanhphuong-dev.github.io/CONCOST.UI/> — published from this folder (`main` → `/docs`) through GitHub Pages. Add `?lang=en` or `?lang=vi` to force a language.

Features: command palette search (`Ctrl K` or `/`), per-page table of contents with reading progress, XAML ↔ C# code inspector with copy, light/dark themes, five accent colours, keyboard navigation, deep links (`index.html#splitbutton`) and a fully offline build (no CDN).

**153 pages · 1238 documented properties · 124 controls mapped to source · 2 languages (VI / EN).**

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
| [Preset Brutal](index.html#brutaltheme) | Preset thứ bảy của ThemeManager: nền kem #FFFDF5, viền mực #111111, accent vàng điện #FFE14D — tinh thần neo-brutalism nhưng vẫn đọc được trong app ng |
| [Reduced motion](index.html#reducedmotion) | Mọi hiệu ứng của thư viện đọc MotionSettings.AnimationsEnabled, mặc định lấy từ Windows “Show animations” (SystemParameters.ClientAreaAnimation). Ngườ |
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

### CAD & Drafting

| Page | Nội dung |
| --- | --- |
| [ViewportControl](index.html#viewportcontrol) | Bề mặt vẽ: lưới, tâm ngắm, dấu bám điểm, grip, dimension tạm, khung chọn và một lớp overlay. Camera là CameraController thật nên cùng một camera dùng  |
| [ViewCube](index.html#viewcube) | Khối lập phương vát cạnh ở góc khung vẽ, quay theo camera: 26 vùng bấm — sáu mặt có chữ, mười hai dải cạnh và tám tam giác góc — cùng vòng la bàn trên |
| [NavigationBar](index.html#navigationbar) | Dải nút cho zoom in/out/extents, pan, orbit, walk và nhãn hệ toạ độ làm việc. Công cụ pan/orbit/walk là bật/tắt giữ trạng thái: bấm lại để thoát, giốn |
| [ViewControlBar](index.html#viewcontrolbar) | Tỉ lệ, mức chi tiết, kiểu hiển thị, đổ bóng, crop, hiện phần tử ẩn và nhóm lệnh hide/isolate. Mỗi thay đổi — do chuột hay do code — đều bắn SettingsCh |
| [CommandLine](index.html#commandline) | Lịch sử lệnh, ô nhập có gợi ý, nhớ lệnh cũ bằng phím mũi tên, Tab để hoàn tất tên lệnh, và prompt mode cho giá trị mà lệnh còn thiếu. Prompt được ghi  |
| [DynamicInput](index.html#dynamicinput) | Ô nhập nhỏ đi theo con trỏ trong lúc vẽ: gõ chiều dài, góc hay toạ độ ngay chỗ đang nhìn, không phải nhìn xuống đáy cửa sổ. Một tấm nền, một gạch chân |
| [CadStatusBar](index.html#cadstatusbar) | Một tấm duy nhất, không viền lồng viền: tóm tắt lựa chọn và mặt phẳng làm việc bên trái, toạ độ con trỏ ở giữa, năm công tắc bám điểm có nhãn SNAP, GR |
| [ProjectBrowser](index.html#projectbrowser) | Cây các view, sheet, family, schedule và level. Tìm kiếm giữ nguyên đường dẫn tới node khớp, lọc theo loại, nhóm theo loại, sắp xếp theo tên hoặc loại |
| [DocumentWorkspace](index.html#documentworkspace) | Host các view dưới dạng tab, chia khung dọc/ngang/lưới, tách một view ra cửa sổ riêng, và cho host từ chối việc đóng khi còn thay đổi chưa lưu. |
| [PropertiesPalette](index.html#propertiespalette) | Bảng thuộc tính của đối tượng đang chọn: tiêu đề, ô tìm, lưới theo nhóm, dòng thông báo lỗi và nút reset về mặc định. Mỗi lần ghi đi qua TransactionMa |
| [ApplicationTitleBar](index.html#applicationtitlebar) | Tiêu đề cửa sổ theo kiểu tài liệu: tên bản vẽ kèm dấu * khi có thay đổi chưa lưu, dải quick-access (tràn vào menu khi hết chỗ), kéo để di chuyển, đúp  |
| [ApplicationInfoBar](index.html#applicationinfobar) | Một dòng thông báo nằm dưới ribbon: bốn mức (Info, Success, Warning, Error) quyết định icon và nền pha màu, kèm một hành động và nút đóng. Nó ở trong  |
| [BackstageView](index.html#backstageview) | Trang phủ toàn cửa sổ sau nút File: dải mục bên trái (kèm nút Back) và nội dung của mục đang chọn bên phải. Nó là một panel của cửa sổ chứ không phải  |
| [FontPicker](index.html#fontpicker) | Ô chọn font family và cỡ chữ, hai nút bật đậm/nghiêng, kèm dòng xem trước sống. Đủ cho hộp thoại text style mà không phải tự ghép bốn control. |

### Data & Dashboard

| Page | Nội dung |
| --- | --- |
| [DataPager](index.html#datapager) | Phân trang cho DataGrid/ListView: PageIndex, PageSize, TotalItems đều bind two-way và tự ép giá trị hợp lệ. |
| [StatisticCard (KPI)](index.html#statisticcard) | Thẻ hiển thị số liệu quan trọng trên Dashboard, hỗ trợ chỉ số tăng/giảm phần trăm (Trend) và Icon điểm nhấn. |
| [Timeline](index.html#timeline) | Hiển thị chuỗi sự kiện hoặc tiến độ thi công công trình theo trục thời gian tuyến tính với các nút trạng thái Semantic, Vector Icon, và đường nối tự đ |
| [CardCarousel](index.html#cardcarousel) | Bộ điều khiển vòng xoay thẻ 3D Coverflow mượt mà, hỗ trợ tạo phối cảnh chiều sâu không gian (Depth Perspective), gán Z-Index phân tầng, nút điều hướng |
| [OdometerNumber](index.html#odometernumber) | Số liệu tự “cuộn” từ giá trị cũ sang giá trị mới với easing, dùng cho KPI, dashboard và bảng tổng hợp. Không cần viết Storyboard cho từng lần cập nhật |
| [Sparkline](index.html#sparkline) | Đường xu hướng cực gọn để nhét vào ô bảng, thẻ KPI hay thanh trạng thái: tự co giãn theo khung, có miền tô gradient và chấm đánh dấu điểm cuối. |
| [AvatarStack](index.html#avatarstack) | Hiển thị nhóm người phụ trách dưới dạng avatar chồng lên nhau kèm chip +N — pattern quen thuộc của bảng quản lý công việc. Nhận chuỗi (tự lấy chữ cái  |
| [HeatmapCalendar](index.html#heatmapcalendar) | Lưới ô vuông kiểu GitHub contribution: mỗi ô là một ngày, màu đậm nhạt theo số lượng. Dùng cho nhật ký đổ bê tông, số lượt nghiệm thu, mật độ quan trắ |
| [TreeDataGrid](index.html#treedatagrid) | DataGrid hiển thị cây: bind RootItems và grid tự dẹt cây thành danh sách dòng, mỗi dòng là chính item chứ không phải wrapper — nên mọi cột vẫn bind th |
| [MultiSelectDataGrid](index.html#multiselectdatagrid) | DataGrid có cột checkbox và checkbox ba trạng thái ở header. Bấm vào ô checkbox là đảo một dòng chứ không xoá lựa chọn cũ, nên chọn hai mươi dòng khôn |
| [TreeMap](index.html#treemap) | Diện tích mỗi ô tỉ lệ với giá trị, các ô được xếp squarified để gần vuông thay vì thành dải dài. Dùng cho cơ cấu khối lượng, phân bổ chi phí, mức tiêu |
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
| [DrawerMenu](index.html#drawermenu) | Thanh điều hướng ngăn kéo thông minh, hỗ trợ thu gọn/mở rộng động (Expand/Collapse Animation mượt mà), thanh Header chứa Logo & Nút Toggle Menu, Item  |
| [Breadcrumb](index.html#breadcrumb) | Đường dẫn điều hướng vị trí trong mô hình công trình (vd: Tòa Nhà A > Tầng 15 > Khu Vực Cốt Thép Dầm). |
| [Pagination](index.html#pagination) | Bộ điều khiển phân trang dữ liệu cho các bảng khối lượng lớn, hỗ trợ nhảy trang nhanh và cấu hình PageSize. |
| [StepBar](index.html#stepbar) | Thanh hiển thị tiến trình công việc nhiều bước (Wizard Steps) với các trạng thái Đang Chờ (Waiting), Đang Làm (In-Progress), Hoàn Thành (Finished) và  |
| [Transfer](index.html#transfer) | Bộ chuyển dữ liệu 2 khung đối xứng (Shuttle List) hỗ trợ lọc tìm kiếm tức thì, chọn tất cả và di chuyển hàng loạt cấu kiện qua lại. |
| [CommandPalette (Ctrl+K)](index.html#commandpalette) | Hộp tìm kiếm phủ lên màn hình, lọc danh sách lệnh khi gõ và chạy bằng Enter. Tìm không phân biệt dấu tiếng Việt, điều hướng bằng mũi tên, và tự vẽ lớp |
| [DocumentTabControl](index.html#documenttabcontrol) | TabControl cho tài liệu: mỗi tab có icon, chấm đánh dấu chưa lưu và nút đóng. Đóng tab là một yêu cầu — host có thể chặn để hỏi lưu trước. |
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
| [DockingManager](index.html#dockingmanager) | Panel quản lý các ToolWindow ở bốn cạnh và vùng giữa. Cùng một cạnh thì các panel trở thành tab của một nhóm, kích thước co giãn theo tỉ lệ khi đổi cử |
| [Expander](index.html#expander) | Khối nội dung thu gọn/mở rộng với 18 style variant (Clean, Card, Filled, Round, Small, Primary/Success/Warning/Danger/Info, hỗ trợ 4 hướng mở). |
| [Card & Border](index.html#card) | Hệ thống thẻ chứa nội dung tiêu chuẩn hóa với độ bo góc `CornerRadius=10`, hiệu ứng bóng nổi `Depth2/Depth3` và huy hiệu thông báo. |
| [Badge](index.html#badge) | Huy hiệu số lượng / trạng thái, dùng độc lập hoặc gắn lên bất kỳ ButtonBase nào qua attached:ButtonElement.Badge. |
| [Divider](index.html#divider) | Đường kẻ phân chia khu vực bố cục với tiêu đề nhãn văn bản ở giữa. |
| [AdaptivePanel](index.html#adaptivepanel) | ContentControl tự chọn layout theo bề rộng khả dụng qua danh sách Breakpoint — dùng để chuyển 3 cấp Compact / Normal / Wide mà không cần code-behind. |
| [CONCOSTWindow](index.html#concostwindow) | Cửa sổ WPF với chrome tuỳ biến (title bar, nút min/max/close trong template) — kéo cửa sổ, double-click để maximize đều xử lý sẵn. |
| [Ribbon](index.html#ribbon) | Ribbon kiểu Office / AutoCAD: dải tab ở trên, mỗi tab chứa các group, mỗi group xếp lệnh trên lưới 3 hàng. Khi hẹp, ribbon tự hạ kích thước lệnh, ẩn n |
| [Ribbon commands](index.html#ribboncommands) | Bộ lệnh đặt trong ribbon: nút thường, toggle, radio, split button hai vùng, dropdown và menu lồng nhau. Tất cả đều theo ICommand: IsEnabled tự bám Can |
| [StatusBar](index.html#statusbar) | Dải thông tin dưới đáy cửa sổ. Mỗi ô là một StatusBarItem; ô khai Alignment="Right" bám mép phải, phần giữa co giãn theo cửa sổ. |
| [CommandBar](index.html#commandbar) | Thanh công cụ một dòng: nút có icon + nhãn, và khi hết chỗ thì phần thừa rơi vào menu xổ xuống thay vì bị cắt cụt. |
| [QuickAccessToolbar](index.html#quickaccesstoolbar) | Bản rời của thanh truy cập nhanh trong ribbon: chỉ icon, cao 28px, đặt được ở bất kỳ cửa sổ nào không cần cả ribbon. |
| [MasonryPanel](index.html#masonrypanel) | Panel trả lại chiều cao tự nhiên cho từng con: con nào cao thì chiếm chỗ ấy, và mỗi con rơi vào cột đang ngắn nhất. Không cần đặt chiều cao cho cột, k |
| [Wizard](index.html#wizard) | Các bước là con của wizard nên form đọc như form trong XAML. Mỗi bước tự giữ IsValid; nút Tiếp chỉ bật khi bước hiện tại hợp lệ, và Navigating cho hos |

### Input & Drag

| Page | Nội dung |
| --- | --- |
| [SearchableTreePicker](index.html#searchabletreepicker) | Ô tìm kiếm đặt trước một cây: gõ để thu hẹp cây xuống các mục khớp và những nhánh dẫn tới chúng, rồi chọn một hoặc nhiều mục. |
| [TagInput](index.html#taginput) | Mỗi giá trị là một chip có nút đóng; dán cả chuỗi “bê tông, thép, cốp pha” thì control tự tách theo dấu phẩy. Có gợi ý, có giới hạn số thẻ, và không c |
| [SignaturePad](index.html#signaturepad) | Vùng ký dùng StrokeCollection thật của System.Windows.Ink: mỗi nét là một Stroke, nên huỷ nét chỉ là bỏ phần tử cuối, và ảnh chữ ký được dựng lại sau  |
| [Drop zone & Drag](index.html#dragdrop) | Ba mảnh của một thao tác kéo thả: FileDropZone nhận tệp, DropTargetHighlight vẽ viền gợi ý quanh bất kỳ phần tử nào, và DragGhost mang nhãn theo con t |

### App & Tooling

| Page | Nội dung |
| --- | --- |
| [Scheduler](index.html#scheduler) | Một cột thời gian cho một ngày: khung giờ, vạch giờ và các khối booking tự chia lane khi trùng giờ. Không cần dữ liệu mẫu - đưa SchedulerItem vào Item |
| [KanbanBoard](index.html#kanbanboard) | Bảng nhiều cột, mỗi cột là một KanbanColumn chứa KanbanCard. Thẻ kéo được sang cột khác, và board phát CardMoved để host ghi lại. |
| [CodeEditor](index.html#codeeditor) | Khung sửa mã có số dòng, phím Tab và tô màu cho C#, XAML, SQL và JSON. Bộ tô màu là một hàm thuần nên test được mà không cần mở cửa sổ. |
| [MarkdownViewer](index.html#markdownviewer) | Hiển thị Markdown thành FlowDocument để dùng lại được cho cả màn hình lẫn bản in. Chỉ hỗ trợ tập con cần cho tài liệu trợ giúp, và mọi cú pháp lạ đều  |
| [ImageViewer](index.html#imageviewer) | Xem ảnh bản vẽ với zoom, xoay và kéo, có nền ca-rô để thấy vùng trong suốt. Phép zoom giữ điểm dưới con trỏ đứng yên nên bản vẽ không bị trượt khi lăn |
| [MiniMap](index.html#minimap) | Bản đồ thu nhỏ của toàn bộ mặt bằng kèm khung nhìn hiện tại. Click để nhảy tới vùng tương ứng - dùng chung cho bản vẽ, sơ đồ hay ảnh lớn. |
| [ModelViewer3D](index.html#modelviewer3d) | Chiếu một tập đỉnh/cạnh 3D lên mặt phẳng 2D để xem nhanh mô hình mà không cần renderer. Đủ cho preview, icon mô hình và kiểm tra kết quả xuất. |
| [CoachMark](index.html#coachmark) | Lớp phủ tối và một thẻ chỉ dẫn trỏ vào control đang được giới thiệu. Thẻ tự lật lên trên khi target ở sát đáy cửa sổ. |
| [SplashScreen](index.html#splashscreen) | Thẻ khởi động có logo, tên ứng dụng, phiên bản và dòng trạng thái, kèm thanh tiến trình. Là control trong cây giao diện nên chạy được cả trong cửa sổ  |

### Effects & Decoration

| Page | Nội dung |
| --- | --- |
| [Ripple](index.html#ripple) | Hiệu ứng gợn sóng Material khi click, dùng độc lập hoặc gắn vào bất kỳ phần tử nào qua attached RippleElement. |
| [SpotlightCard](index.html#spotlightcard) | Bề mặt thẻ có vệt sáng bám theo con trỏ và viền gradient hiện dần khi hover — pattern “spotlight card” quen thuộc trên các landing page hiện đại, port |
| [MarqueeTicker](index.html#marqueeticker) | Dải chữ chạy liên tục cho thanh trạng thái, bảng tin công trường hoặc ticker chứng khoán. Render trực tiếp và chạy bằng CompositionTarget.Rendering nê |
| [Mesh gradient & Spotlight](index.html#meshgradient) | Hai hiệu ứng nền: AnimatedMeshGradient cho bốn khối màu “thở” quanh neo cố định, và CursorSpotlight làm tối mọi thứ trừ một vòng quanh con trỏ. |
| [Kính mờ & viền gradient](index.html#glasscard) | Hai bề mặt: FrostedGlass làm mờ đúng vùng nằm sau nó, và AnimatedGradientBorder cho viền gradient xoay quanh thẻ. |
| [Stagger & Shared element](index.html#motion) | Hai kiểu chuyển động: danh sách hiện dần từng mục, và một phần tử “lớn ra” từ chỗ nó từng nằm. |
| [AuroraBackground](index.html#aurorabackground) | Nền aurora gồm vài quầng sáng lớn trôi chậm, vẽ trực tiếp bằng RadialGradientBrush nên không dùng BlurEffect — vẫn mượt trên máy yếu. Dùng cho splash  |
| [BorderBeam](index.html#borderbeam) | Một đốm sáng chạy dọc chu vi khung bo góc, kiểu “border beam” trên các landing page hiện đại. Bọc quanh nội dung bất kỳ, dùng cho item mới, trạng thái |
| [ConfettiBurst](index.html#confettiburst) | Bắn một đợt pháo giấy ngắn khi thao tác thành công (lưu mô hình, duyệt hồ sơ). Tự mô phỏng vật lý và tự tắt khi hạt tan hết, không cần code hẹn giờ. |
| [SheenSweep](index.html#sheensweep) | Một dải sáng chéo chạy qua bề mặt theo chu kỳ, có khoảng nghỉ giữa hai lần quét — hiệu ứng “sheen” quen thuộc trên nút CTA và thẻ sản phẩm. Đặt trong  |
| [PulseRing](index.html#pulsering) | Các vòng tròn lan ra rồi mờ dần quanh một điểm, dùng cho trạng thái đang chạy, đang đồng bộ hoặc đánh dấu vị trí trên bản vẽ. |
| [WaveDivider](index.html#wavedivider) | Hai lớp sóng sin trôi với tốc độ khác nhau, dùng làm đường phân cách section hoặc chân của khối header. Vẽ bằng StreamGeometry nên không cần ảnh. |
| [GradientText](index.html#gradienttext) | Chữ được tô bằng gradient chạy liên tục — dùng cho hero headline, badge phiên bản hoặc số liệu nhấn mạnh. Vẽ trực tiếp bằng FormattedText với brush bi |
| [CursorGlow](index.html#cursorglow) | Quầng sáng mềm trượt theo con trỏ với độ trễ nhẹ, dùng cho hero tối hoặc panel chào mừng. Không chặn chuột (IsHitTestVisible=false) nên đặt trên cùng  |
| [GridPattern](index.html#gridpattern) | Ba kiểu nền lưới vẽ bằng DrawingBrush tile: chấm, đường kẻ, hoặc dấu chữ thập — phù hợp nền bản vẽ, blueprint, dashboard kỹ thuật. Vì là tile brush nê |
| [ShakeElement](index.html#shakeelement) | Attached behaviour rung ngang một phần tử khi validate lỗi — phản hồi quen thuộc, 360ms, biên độ giảm dần. Dùng được trên mọi FrameworkElement và giữ  |
| [TiltCard](index.html#tiltcard) | Thẻ nghiêng về phía con trỏ và nhấc nhẹ khi hover. Độ nghiêng làm bằng skew + scale (2.5D) chứ không dùng Viewport3D, nên chữ vẫn sắc nét và không tốn |
| [MagneticHover](index.html#magnetichover) | Attached behaviour kéo phần tử về phía con trỏ vài pixel rồi trả về vị trí cũ khi rời chuột. Dùng TranslateTransform nên không gây layout mỗi frame. |
| [CheckMarkDraw](index.html#checkmarkdraw) | Dấu ✓ vẽ dần từng nét trong 320ms khi IsChecked chuyển sang true, và xoá ngược khi false. Dùng cho luồng duyệt, lưu, hoàn tất. |
| [ScrollProgressBar](index.html#scrollprogressbar) | Thanh mảnh hiển thị đã cuộn được bao nhiêu phần của một ScrollViewer. Không set Target thì tự tìm ScrollViewer cha gần nhất. |
| [ScrollReveal](index.html#scrollreveal) | Attached behaviour: phần tử bắt đầu ở trạng thái mờ + lệch xuống, và khi cuộn tới ngưỡng thì fade + trượt lên. Tự tìm ScrollViewer cha, hiện **một lần |
| [ParallaxPanel](index.html#parallaxpanel) | Lớp nội dung tự dịch theo vị trí cuộn với hệ số Speed — đặt phía sau nội dung chính để tạo cảm giác chiều sâu. Không chặn chuột. |
| [TypewriterText](index.html#typewritertext) | Chữ hiện dần như đang gõ, có caret nhấp nháy. Tốc độ tính theo ký tự/giây; tắt animation hệ thống thì hiện full text ngay. |
| [TextReveal](index.html#textreveal) | Headline hiện theo từng từ: mỗi từ fade + trượt lên, lệch nhau WordDelay giây. Dùng cho tiêu đề lớn, không dùng cho đoạn văn dài. |
| [SnowCanvas](index.html#snowcanvas) | Khung vẽ hạt tuyết rơi chuyển động GPU siêu mượt, hỗ trợ tùy chỉnh mật độ số lượng hạt, bán kính Min/Max, tốc độ rơi, độ lắc lư dao động (Oscillation  |
| [RadialGauge](index.html#radialgauge) | Cung nền + cung giá trị + vạch chia + số ở giữa, vẽ trong OnRender bằng StreamGeometry với đầu nét tròn nên sắc ở mọi kích thước và mọi DPI. Giá trị c |
| [ParticleField](index.html#particlefield) | Hạt trôi nhẹ làm nền cho hero, splash hoặc empty state. Hạt wrap quanh biên nên nền không bao giờ trống, Seed cho kết quả tái lập, và field tự dựng lạ |
| [PageTransitionHost](index.html#pagetransitionhost) | Đổi Content là trang mới chạy hiệu ứng vào: fade, trượt từ bốn phía, hoặc scale. Animation nằm ở host nên dùng được với mọi loại nội dung, không cần s |
| [Sprite](index.html#sprite) | Nút trợ lý nổi tương tác thông minh (Assistive Touch / Floating Action Button). Sprite luôn nổi phía trên toàn bộ giao diện, cho phép người dùng kéo t |
| [CompareSlider](index.html#compareslider) | Thanh trượt chia đôi màn hình trực quan cho phép so sánh sự thay đổi giữa 2 hình ảnh bản vẽ thiết kế (Rev 01 vs Rev 02), phối cảnh 3D Render trước/sau |
| [Magnifier](index.html#magnifier) | Tròng kính lúp phóng to tọa độ điểm ảnh độ nét cao. Khi rê chuột vào khu vực bản vẽ CAD hoặc mặt cắt dầm cột, kính lúp tròn có hồng tâm chữ thập sẽ tự |
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
| [DataGrid Panels](index.html#datagridpanels) | Bốn control độc lập gắn vào một DataGrid có sẵn để biến bảng chỉ-đọc thành công cụ dữ liệu: dải lọc theo từng cột, ô tìm kiếm toàn bảng, menu chọn cột |
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
| `data/cad.js` | The Revit-inspired CAD shell: viewport, navigation, command line, status bar, browsers, chrome. |
| `data/basic.js` | Styling-only basic controls + attached property tables. |
| `data/enrich.js` | Remaining property tables and C# snippets. |
| `data/auto.generated.js` | **Generated** property tables, C# samples and English property descriptions. Do not edit. |
| `data/nav.js` | Sidebar order, icons, badges (VI + EN labels). |
| `data.js` | Merges every dataset into `DOCS_DATA`. |
| `data/en/ui.js` | UI chrome strings (VI + EN), badge labels and the `DOCS_EN` container. |
| `data/en/*.js` | English overlay per page: title, description, section text, callouts and tables. |
| `data/en/code.js` | Vietnamese → English dictionary applied to code snippets in EN mode. |
| `data/sources.generated.js` | **Generated** page → source file map (powers the “edit this page” link). |
| `vendor/` | Self-hosted fonts, Font Awesome and Prism — no CDN, works offline. |
| `app.js` / `style.css` / `index.html` | Application shell, design system and language switch. |

### Regenerate the generated parts

```bash
python docs/tools/build-props.py     # property tables + C# samples + EN property text from src/CONCOST.UI/**/*.cs
python docs/tools/check-xaml.py      # validate every XAML snippet against the real API
node   docs/tools/build-index.js     # rebuild this page map + the source map
python docs/tools/fetch-fonts.py     # re-download the localised font subsets
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
