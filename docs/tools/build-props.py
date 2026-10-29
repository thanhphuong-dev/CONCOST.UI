"""
build-props.py — generate docs/data/auto.generated.js for the CONCOST.UI wiki.

Why: hand-written property tables drift away from the library. This script reads the
C# source, extracts every DependencyProperty (name / type / default / read-only /
attached), builds a Vietnamese description, and emits a ready-to-use DOCS_AUTO object
plus a C# snippet per page.

Usage:
    python docs/tools/build-props.py                # regenerate auto.generated.js
    python docs/tools/build-props.py --check-xaml    # validate XAML snippets vs real API
"""
import json
import os
import re
import sys
from collections import OrderedDict

HERE = os.path.dirname(os.path.abspath(__file__))
DOCS = os.path.dirname(HERE)
REPO = os.path.dirname(DOCS)
SRC = os.path.join(REPO, "src", "CONCOST.UI")
OUT = os.path.join(DOCS, "data", "auto.generated.js")

# ---------------------------------------------------------------- extraction

CLASS_RE = re.compile(r"public\s+(?:sealed\s+|abstract\s+|partial\s+|static\s+)*class\s+([A-Za-z0-9_]+)")
DP_RE = re.compile(
    r"DependencyProperty\.Register(ReadOnly)?(Attached)?\s*\(\s*"
    r"(?:nameof\s*\(\s*([A-Za-z0-9_]+)\s*\)|\"([A-Za-z0-9_]+)\")\s*,\s*"
    r"typeof\s*\(\s*([A-Za-z0-9_.<>\[\],\s]+?)\s*\)",
    re.S,
)
EV_RE = re.compile(r"public\s+event\s+[A-Za-z0-9_.<>]+\s+([A-Za-z0-9_]+)")
NS_RE = re.compile(r"^\s*namespace\s+([A-Za-z0-9_.]+)", re.M)
ENUM_RE = re.compile(r"public\s+enum\s+([A-Za-z0-9_]+)\s*\{", re.S)


def strip_comments(text):
    text = re.sub(r"///[^\n]*", "", text)
    text = re.sub(r"/\*.*?\*/", "", text, flags=re.S)
    text = re.sub(r"(?<!:)\bit//[^\n]*", "", text)
    return text


def class_bodies(src):
    """Yield (class_name, body) using brace matching so nested classes are handled."""
    out = []
    for m in CLASS_RE.finditer(src):
        brace = src.find("{", m.end())
        if brace == -1:
            continue
        depth, i = 0, brace
        while i < len(src):
            ch = src[i]
            if ch == "{":
                depth += 1
            elif ch == "}":
                depth -= 1
                if depth == 0:
                    break
            i += 1
        out.append((m.group(1), src[brace:i + 1]))
    return out


def default_value(body, start):
    """Read the first argument of the PropertyMetadata ctor, respecting nested parens."""
    tail = body[start:start + 900]
    md = re.search(r"new\s+(?:FrameworkPropertyMetadata|PropertyMetadata)\s*\(", tail)
    if not md:
        return ""
    i = md.end() - 1
    depth, j = 0, i
    while j < len(tail):
        if tail[j] == "(":
            depth += 1
        elif tail[j] == ")":
            depth -= 1
            if depth == 0:
                break
        j += 1
    inner = tail[i + 1:j]
    # first top-level argument
    depth, cut = 0, len(inner)
    for k, ch in enumerate(inner):
        if ch in "(<[":
            depth += 1
        elif ch in ")>]":
            depth -= 1
        elif ch == "," and depth == 0:
            cut = k
            break
    raw = re.sub(r"\s+", " ", inner[:cut]).strip()
    if raw.startswith("new "):
        raw = raw[4:]
    return raw


def extract():
    classes = OrderedDict()
    enums = OrderedDict()
    for dirpath, dirnames, files in os.walk(SRC):
        dirnames[:] = [d for d in dirnames if d not in ("obj", "bin")]
        for f in sorted(files):
            if not f.endswith(".cs"):
                continue
            path = os.path.join(dirpath, f)
            raw = open(path, encoding="utf-8-sig", errors="ignore").read()
            ns = NS_RE.search(raw)
            ns = ns.group(1) if ns else "CONCOST.UI"
            src = strip_comments(raw)

            for em in ENUM_RE.finditer(src):
                brace = em.end() - 1
                depth, i = 0, brace
                while i < len(src):
                    if src[i] == "{":
                        depth += 1
                    elif src[i] == "}":
                        depth -= 1
                        if depth == 0:
                            break
                    i += 1
                members = [x.strip() for x in src[brace + 1:i].split(",") if x.strip()]
                members = [m.split("=")[0].strip() for m in members]
                members = [m for m in members if re.match(r"^[A-Za-z_][A-Za-z0-9_]*$", m)]
                enums.setdefault(em.group(1), members)

            for name, body in class_bodies(src):
                props, seen = [], set()
                for m in DP_RE.finditer(body):
                    readonly, attached, n1, n2, ptype = m.groups()
                    pname = n1 or n2
                    if pname in seen:
                        continue
                    seen.add(pname)
                    props.append({
                        "name": pname,
                        "type": ptype,
                        "default": default_value(body, m.end()),
                        "readonly": bool(readonly),
                        "attached": bool(attached),
                    })
                events = sorted(set(EV_RE.findall(body)))
                if not props and not events:
                    continue
                entry = classes.setdefault(name, {"namespace": ns, "file": os.path.relpath(path, REPO).replace("\\", "/"),
                                                  "props": [], "events": []})
                known = {p["name"] for p in entry["props"]}
                for p in props:
                    if p["name"] not in known:
                        entry["props"].append(p)
                entry["events"] = sorted(set(entry["events"]) | set(events))
    return classes, enums


# ---------------------------------------------------------------- descriptions

D = {
    # generic
    "Value": "Giá trị hiện tại của control (two-way binding mặc định).",
    "Text": "Nội dung văn bản hiển thị hoặc cho phép nhập.",
    "Title": "Tiêu đề hiển thị ở phần đầu control.",
    "Header": "Nội dung phần header.",
    "Icon": "Vector icon hiển thị kèm nội dung.",
    "IconKind": "Loại vector icon (xem <code>IconKind</code>).",
    "IconSize": "Kích thước icon (px).",
    "IconPlacement": "Vị trí icon so với nội dung (<code>Dock.Left</code>, <code>Right</code>…).",
    "CornerRadius": "Bo góc của khung control.",
    "Content": "Nội dung chính của control.",
    "ItemsSource": "Nguồn dữ liệu (thường là <code>IEnumerable</code>) dùng để sinh item.",
    "Items": "Tập item khai báo trực tiếp trong XAML.",
    "SelectedItem": "Item đang được chọn.",
    "SelectedIndex": "Chỉ số item đang được chọn.",
    "SelectedValue": "Giá trị của item đang được chọn.",
    "IsSelected": "Item/control có đang ở trạng thái chọn hay không.",
    "IsOpen": "Control/popup có đang mở hay không.",
    "IsDropDownOpen": "Trạng thái mở của dropdown.",
    "IsLoading": "Bật/tắt trạng thái loading (skeleton, overlay, spinner).",
    "IsActive": "Bật/tắt hiệu ứng hoặc chế độ hoạt động của control.",
    "IsExpanded": "Trạng thái mở rộng của Expander.",
    "IsReadOnly": "Chế độ chỉ đọc — vẫn chọn/copy được nhưng không sửa.",
    "IsIndeterminate": "Chế độ tiến trình không xác định (không có %).",
    "IsClosable": "Cho phép người dùng đóng tag.",
    "IsEnabled": "Bật/tắt hiệu ứng (attached property).",
    "IsDot": "Hiển thị dạng chấm tròn thay vì số (badge notification).",
    "IsPassword": "Ẩn nội dung thành ký tự mật khẩu.",
    "IsValid": "Kết quả kiểm tra hợp lệ của dữ liệu nhập (read-only).",
    "ShowPassword": "Hiện/ẩn mật khẩu đang nhập.",
    "ShowSeconds": "Hiển thị thêm thành phần giây.",
    "ShowCloseButton": "Hiện nút đóng trên control.",
    "ShowSelectAll": "Hiện tuỳ chọn chọn tất cả.",
    "ShowSearch": "Hiện ô tìm kiếm trong danh sách.",
    "ShowBorder": "Hiện viền bao quanh control.",
    "ShowBadge": "Hiện huy hiệu số lượng.",
    "ShowLegend": "Hiện chú giải (legend) của biểu đồ.",
    "ShowArea": "Tô vùng dưới đường line chart.",
    "ShowGrid": "Hiện lưới nền.",
    "ShowAlpha": "Cho phép chọn kênh alpha (độ trong suốt).",
    "ShowScore": "Hiện điểm số bên cạnh số sao.",
    "ShowCrosshair": "Hiện tâm ngắm của kính lúp.",
    "ShowToolbar": "Hiện thanh công cụ của spreadsheet.",
    "ShowFormulaBar": "Hiện thanh nhập công thức.",
    "ShowSheetTabs": "Hiện tab chuyển sheet.",
    "ShowStatusBar": "Hiện thanh trạng thái dưới cùng.",
    "ShowFooterSummary": "Hiện dòng tổng hợp ở footer.",
    "ShowToggleButton": "Hiện nút thu gọn/mở rộng menu.",
    "ShowText": "Hiện phần trăm/giá trị trên thanh tiến trình.",
    "Minimum": "Giá trị nhỏ nhất cho phép.",
    "Maximum": "Giá trị lớn nhất cho phép.",
    "Min": "Giá trị nhỏ nhất của thang điểm.",
    "Max": "Giá trị lớn nhất của thang điểm.",
    "MaxPage": "Số trang tối đa.",
    "MaxZoom": "Mức zoom lớn nhất.",
    "MinZoom": "Mức zoom nhỏ nhất.",
    "Increment": "Bước tăng/giảm khi bấm nút hoặc dùng phím mũi tên.",
    "DecimalPlaces": "Số chữ số thập phân hiển thị.",
    "Suffix": "Đơn vị/hậu tố hiển thị sau giá trị (vd <code> kN</code>).",
    "Prefix": "Tiền tố hiển thị trước giá trị.",
    "FormatString": "Chuỗi định dạng số kiểu .NET (vd <code>N2</code>, <code>0.###</code>).",
    "StringFormat": "Định dạng hiển thị của ô.",
    "Unit": "Đơn vị đo hiển thị kèm giá trị.",
    "UnitScale": "Tỉ lệ quy đổi đơn vị của thước (px → mm).",
    "MajorStep": "Khoảng chia lớn trên thước.",
    "OriginOffset": "Độ lệch gốc toạ độ của thước so với canvas.",
    "Orientation": "Hướng bố trí (<code>Horizontal</code> / <code>Vertical</code>).",
    "Placement": "Vị trí bung ra của popup/drawer.",
    "Placeholder": "Chữ gợi ý khi ô nhập trống.",
    "Description": "Mô tả phụ hiển thị dưới tiêu đề.",
    "Subtitle": "Dòng phụ đề của thẻ.",
    "Message": "Nội dung thông báo.",
    "SubMessage": "Dòng mô tả phụ của thông báo.",
    "Progress": "Giá trị tiến trình 0–100.",
    "Status": "Trạng thái nghiệp vụ hiển thị trên control.",
    "Tag": "Dữ liệu bất kỳ gắn kèm item (thường dùng cho command).",
    "TagName": "Tên định danh của item điều hướng.",
    "Command": "Lệnh thực thi khi người dùng tương tác.",
    "CommandParameter": "Tham số truyền vào <code>Command</code>.",
    "CommandTarget": "Phần tử nhận routed command.",
    "ActionCommand": "Lệnh cho nút hành động chính.",
    "ActionButtonText": "Nhãn nút hành động chính.",
    "ActionButtonIcon": "Icon của nút hành động chính.",
    "SecondaryActionText": "Nhãn nút hành động phụ.",
    "ExtraContent": "Vùng nội dung tuỳ biến thêm.",
    "Footer": "Nội dung chân control.",
    "FooterTemplate": "Template cho vùng footer.",
    "HeaderTemplate": "Template cho header.",
    "ItemTemplate": "Template hiển thị từng item.",
    "DropDownContent": "Nội dung của vùng dropdown.",
    "DropDownContentTemplate": "Template cho nội dung dropdown.",
    "MenuContent": "Nội dung menu ngữ cảnh của sprite.",
    "SourceContent": "Nội dung gốc (bên trái) của thanh so sánh.",
    "TargetContent": "Nội dung sau khi so sánh (bên phải).",
    "MaxDropDownHeight": "Chiều cao tối đa của dropdown.",
    "DropDownButtonPosition": "Vị trí nút mở dropdown trong SplitButton.",
    "HitMode": "Cách phân vùng click: <code>Split</code> (nút + mũi tên riêng) hoặc <code>DropDown</code>.",
    "AutoCloseOnClick": "Tự đóng dropdown sau khi chọn item.",
    "StaysOpen": "Giữ dropdown mở khi click ra ngoài.",
    "IsOpenOnHover": "Mở dropdown khi rê chuột vào.",
    "HoverDelay": "Thời gian chờ trước khi mở khi hover.",
    "Appearance": "Kiểu màu hiển thị (<code>Primary</code>, <code>Success</code>, <code>Warning</code>, <code>Danger</code>, <code>Info</code>, <code>Neutral</code>).",
    "Badge": "Huy hiệu số lượng/thông báo gắn lên item.",
    "HasBadge": "Cho biết item có badge hay không (read-only).",
    "BadgeMargin": "Khoảng cách của badge so với góc control.",
    "Description2": "",
    "DotSize": "Đường kính chấm của indicator (px).",
    "IndicatorType": "Kiểu loading: <code>ThreeDotsWave</code>, <code>CircularDotsRing</code>, <code>LinePulse</code>, <code>PulseCircle</code>.",
    "Duration": "Thời lượng của animation.",
    "SkeletonShape": "Hình dạng placeholder: <code>Rectangle</code>, <code>Circle</code>, <code>TextLine</code>, <code>Card</code>.",
    "BaseBrush": "Brush nền cho ô/khối không có dữ liệu (mặc định).",
    "HighlightBrush": "Màu vệt sáng chạy qua skeleton.",
    "IsWaveEnabled": "Bật hiệu ứng sóng shimmer.",
    "RippleBrush": "Màu của gợn sóng.",
    "MaxOpacity": "Độ đục lớn nhất của gợn sóng.",
    "MaxConcurrentRipples": "Số gợn sóng tối đa cùng lúc.",
    "IsRippleEnabled": "Bật/tắt hiệu ứng ripple.",
    "Mode": "Chế độ hoạt động (ý nghĩa phụ thuộc control).",
    "Length": "Số ô/ký tự cần nhập.",
    "Password": "Giá trị mật khẩu hiện tại.",
    "PasswordChar": "Ký tự che mật khẩu.",
    "ItemWidth": "Chiều rộng mỗi item.",
    "ItemHeight": "Chiều cao mỗi item.",
    "ItemMargin": "Khoảng cách giữa các item.",
    "ItemSize": "Kích thước mỗi item (rating).",
    "ItemCornerRadius": "Bo góc từng ô nhập.",
    "RowHeight": "Chiều cao dòng dữ liệu.",
    "RowHeaderWidth": "Chiều rộng cột tiêu đề dòng.",
    "ColumnHeaderHeight": "Chiều cao header cột.",
    "FooterRowHeight": "Chiều cao dòng footer.",
    "StackedRowHeight": "Chiều cao mỗi tầng header.",
    "PageSize": "Số item mỗi trang.",
    "PageIndex": "Chỉ số trang hiện tại (bắt đầu từ 1).",
    "CurrentPage": "Trang hiện tại của bộ phân trang.",
    "TotalItems": "Tổng số item cần phân trang.",
    "CurrentLayout": "Layout hiện tại theo bề rộng khả dụng (<code>Compact</code>, <code>Normal</code>, <code>Wide</code>).",
    "CompactWidth": "Chiều rộng chế độ thu gọn.",
    "ExpandedWidth": "Chiều rộng chế độ mở rộng.",
    "IsCollapsed": "Menu đang thu gọn về dạng icon-only.",
    "Children": "Danh sách item con (menu phân cấp).",
    "ChildColumns": "Các cột con trong header đa tầng.",
    "ColumnHeaderTree": "Cấu trúc cây header (dựng tự động).",
    "StackedHeaderRows": "Danh sách tầng header.",
    "CellType": "Kiểu hiển thị ô: <code>Text</code>, <code>Numeric</code>, <code>Progress</code>, <code>Badge</code>, <code>Rating</code>, <code>Date</code>, <code>Hyperlink</code>, <code>CheckBox</code>.",
    "BindingPath": "Đường dẫn property dùng để bind dữ liệu.",
    "SortMemberPath": "Property dùng để sắp xếp.",
    "CanUserSort": "Cho phép người dùng sắp xếp theo cột.",
    "CanUserResize": "Cho phép kéo giãn cột.",
    "AggregateType": "Kiểu tổng hợp footer: <code>Sum</code>, <code>Average</code>, <code>Count</code>, <code>Min</code>, <code>Max</code>, <code>Custom</code>.",
    "AggregateFormat": "Định dạng số cho giá trị tổng hợp.",
    "CustomAggregateText": "Chuỗi tổng hợp tự nhập (khi <code>AggregateType=Custom</code>).",
    "EnableContextMenu": "Bật menu ngữ cảnh mặc định của grid.",
    "SelectionRange": "Vùng ô đang chọn trong spreadsheet.",
    "ActiveRow": "Chỉ số dòng đang active.",
    "ActiveColumn": "Chỉ số cột đang active.",
    "ActiveSheet": "Sheet đang mở.",
    "Sheets": "Danh sách sheet của workbook.",
    "FormulaText": "Công thức của ô đang chọn.",
    "GridLineBrush": "Màu đường kẻ lưới.",
    "HeaderBackground": "Màu nền vùng header.",
    "SelectionBrush": "Màu nền vùng đang chọn.",
    "SelectionBorderBrush": "Màu viền vùng đang chọn.",
    "Trend": "Giá trị xu hướng hiển thị trên thẻ KPI (vd <code>+12.4%</code>).",
    "TrendDirection": "Chiều xu hướng: <code>Up</code>, <code>Down</code>, <code>Neutral</code>.",
    "Time": "Mốc thời gian của mốc timeline.",
    "IsLast": "Đánh dấu mốc cuối (ẩn đường nối).",
    "DotBrush": "Màu chấm mốc timeline.",
    "Type": "Loại hiển thị (ý nghĩa phụ thuộc control).",
    "TimeString": "Chuỗi thời gian hiển thị trên toast.",
    "GrowlType": "Loại thông báo: <code>Info</code>, <code>Success</code>, <code>Warning</code>, <code>Error</code>, <code>Fatal</code>, <code>Ask</code>.",
    "DialogWidth": "Chiều rộng hộp thoại.",
    "DialogHeight": "Chiều cao hộp thoại.",
    "MaskCanClose": "Cho phép click lớp phủ để đóng.",
    "MessageType": "Loại nội dung hộp thoại: <code>Information</code>, <code>Success</code>, <code>Warning</code>, <code>Error</code>, <code>Question</code>.",
    "Token": "Token định danh để đóng/mở dialog theo key.",
    "DrawerWidth": "Chiều rộng ngăn kéo.",
    "DrawerHeight": "Chiều cao ngăn kéo.",
    "Categories": "Danh sách nhóm thuộc tính của PropertyGrid.",
    "SelectedObject": "Đối tượng đang được inspect.",
    "SelectedPath": "Đường dẫn file/thư mục đã chọn.",
    "Filter": "Bộ lọc file kiểu Win32 (vd <code>*.dwg;*.dxf</code>).",
    "SeparatorIcon": "Icon phân cách giữa các mục breadcrumb.",
    "LineStyle": "Kiểu đường kẻ: <code>Solid</code>, <code>Dashed</code>, <code>Dotted</code>.",
    "TextAlignment": "Căn nội dung văn bản.",
    "Angle": "Góc xoay của watermark (độ).",
    "MarkOpacity": "Độ đục của watermark.",
    "MarkBrush": "Màu watermark.",
    "MarkFontSize": "Cỡ chữ watermark.",
    "SnowflakeCount": "Số hạt tuyết hiển thị.",
    "SnowflakeBrush": "Màu hạt tuyết.",
    "MinSnowflakeSize": "Kích thước hạt nhỏ nhất.",
    "MaxSnowflakeSize": "Kích thước hạt lớn nhất.",
    "MinSpeed": "Tốc độ rơi nhỏ nhất.",
    "MaxSpeed": "Tốc độ rơi lớn nhất.",
    "Wind": "Lực gió ngang tác động lên hạt.",
    "AutoSnap": "Tự hít vào viền màn hình khi kéo.",
    "AutoRun": "Tự chạy carousel.",
    "Interval": "Chu kỳ chạy tự động (ms).",
    "Spacing": "Khoảng cách giữa các item.",
    "Scale": "Tỉ lệ phóng to của item đang active.",
    "MinScale": "Tỉ lệ nhỏ nhất của item không active.",
    "OffsetX": "Độ lệch ngang của item trong carousel.",
    "Zoom": "Mức zoom hiện tại của canvas.",
    "GridSize": "Kích thước ô lưới (px).",
    "PanOffsetX": "Độ dịch ngang của khung nhìn.",
    "PanOffsetY": "Độ dịch dọc của khung nhìn.",
    "AreaMm2": "Diện tích cốt thép As (mm²) — read-only, tự tính từ ký hiệu.",
    "Target": "Phần tử/bitmap nguồn cần phóng to.",
    "Radius": "Bán kính vùng phóng to.",
    "Center": "Toạ độ tâm vùng phóng to.",
    "HandleSize": "Kích thước tay nắm kéo.",
    "Value1": "Giá trị thấp của khoảng.",
    "Value2": "Giá trị cao của khoảng.",
    "StartValue": "Giá trị bắt đầu của khoảng.",
    "EndValue": "Giá trị kết thúc của khoảng.",
    "SelectedColor": "Màu đang chọn.",
    "HexCode": "Mã hex của màu đang chọn.",
    "SelectedTime": "Thời điểm đang chọn trên đồng hồ.",
    "SelectedDateTime": "Ngày giờ đang chọn.",
    "DisplayMode": "Đơn vị đang chỉnh: <code>Hour</code>, <code>Minute</code>, <code>Second</code>.",
    "SourceTitle": "Tiêu đề danh sách nguồn.",
    "TargetTitle": "Tiêu đề danh sách đích.",
    "TransferredItems": "Các item đã được chuyển sang danh sách đích.",
    "ListWidth": "Chiều rộng mỗi danh sách.",
    "ListHeight": "Chiều cao mỗi danh sách.",
    "SelectedItemsPreviewMaxWidth": "Chiều rộng tối đa vùng xem trước các item đã chọn.",
    "SelectAllText": "Nhãn của mục chọn tất cả.",
    "DisplayText": "Chuỗi mô tả các item đang chọn (read-only).",
    "DisplayMemberPath": "Tên property dùng để hiển thị text của item.",
    "IsInstantSearch": "Tìm kiếm ngay khi gõ (không cần Enter).",
    "SearchCommand": "Lệnh chạy khi tìm kiếm.",
    "Index": "Chỉ số bước hiện tại.",
    "StepIndex": "Chỉ số của bước trong StepBar.",
    "Dock": "Vị trí neo của thanh tiến trình bước.",
    "CurrentLayout2": "",
    "Data": "Dữ liệu điểm của biểu đồ.",
    "Datas": "Tập dữ liệu của biểu đồ (collection điểm).",
    "Segments": "Các lát của biểu đồ tròn.",
    "BarBrush": "Màu cột của bar chart.",
    "LineBrush": "Màu đường của line chart.",
    "RadarBrush": "Màu vùng radar chart.",
    "MultiColor": "Mỗi cột một màu khác nhau.",
    "Smooth": "Vẽ đường cong mượt (spline) thay vì nối thẳng.",
    "CenterText": "Chữ hiển thị giữa biểu đồ tròn.",
    "ArcThickness": "Độ dày cung của donut chart.",
    "GridLevels": "Số vòng lưới của radar chart.",
    "TargetGrid": "Grid đích dùng cho layout.",
    "OffContent": "Nội dung khi ở trạng thái tắt.",
    "OnContent": "Nội dung khi ở trạng thái bật.",
    "SmallChange": "Bước nhảy nhỏ khi dùng phím mũi tên.",
    "Brush": "Brush sử dụng cho hiệu ứng (attached).",
    "Color": "Màu của phần tử.",
    "Name": "Tên định danh.",
    "Operator": "Toán tử so sánh của luật định dạng.",
    "DragDropGroup": "Nhóm kéo-thả — chỉ item cùng nhóm mới thả được vào nhau.",
    "IsDragSource": "Cho phép kéo phần tử này.",
    "IsDropTarget": "Cho phép thả vào phần tử này.",
    "EmptyMessage": "Thông báo khi danh sách rỗng.",
    "ToolTip": "Tooltip hiển thị khi hover.",
    "Width": "Chiều rộng.",
    "MinWidth": "Chiều rộng tối thiểu.",
    "MaxWidth": "Chiều rộng tối đa.",
    "HorizontalAlignment": "Căn theo chiều ngang.",
    "Background": "Màu nền.",
    "Foreground": "Màu chữ/foreground.",
    "SpotlightBrush": "Màu của vệt sáng bám theo con trỏ (mặc định lấy accent của theme).",
    "SpotlightSize": "Đường kính vùng sáng (px).",
    "SpotlightOpacity": "Độ đậm tối đa của vệt sáng (0–1).",
    "IsSpotlightEnabled": "Bật/tắt hiệu ứng vệt sáng (tắt trên danh sách dài để giữ FPS).",
    "BorderGradient": "Brush viền hiện dần khi hover — dùng <code>LinearGradientBrush</code> để có viền chuyển màu.",
    "ShowHoverBorder": "Hiện viền khi hover (tắt để chỉ giữ vệt sáng).",
    "DisplayText": "Chuỗi đã định dạng của giá trị hiện tại (read-only, tự cập nhật khi chạy số).",
    "AnimateOnLoad": "Chạy animation ngay lần đầu nhận giá trị.",
    "Values": "Dãy số liệu để vẽ (tự co giãn theo khung).",
    "StrokeThickness": "Độ dày nét vẽ (px).",
    "ShowArea": "Tô miền dưới đường bằng gradient.",
    "ShowLastDot": "Đánh dấu điểm dữ liệu mới nhất bằng một chấm.",
    "Smooth": "Vẽ đường cong Bezier thay vì nối thẳng.",
    "LastDotBrush": "Màu chấm đánh dấu điểm cuối.",
    "MaxVisible": "Số avatar hiện tối đa; phần còn lại gộp vào chip +N.",
    "Overlap": "Mức chồng lên nhau giữa hai avatar (px).",
    "RingBrush": "Màu vòng viền giữa các avatar.",
    "ShowOverflow": "Hiện chip +N khi số item vượt <code>MaxVisible</code>.",
    "OverflowTextFormat": "Định dạng chuỗi chip tràn (mặc định <code>+{0}</code>).",
    "Items": "Danh sách item: chuỗi (tự lấy chữ cái đầu) hoặc <code>ImageSource</code>.",
    "Size": "Kích thước cạnh của phần tử (px) — với AvatarStack là đường kính avatar.",
    "FontSize": "Cỡ chữ hiển thị bên trong control.",
    "StartDate": "Ngày bắt đầu của chuỗi dữ liệu (phần tử đầu tiên ứng với ngày này).",
    "CellSize": "Cạnh của mỗi ô vuông (px).",
    "CellGap": "Khoảng cách giữa hai ô (px).",
    "Level1Brush": "Màu mức 1 — cường độ thấp nhất (trên 0).",
    "Level2Brush": "Màu mức 2.",
    "Level3Brush": "Màu mức 3.",
    "Level4Brush": "Màu mức 4 — cường độ cao nhất.",
    "ShowMonthLabels": "Hiện nhãn tháng phía trên lưới.",
    "MonthLabelBrush": "Màu chữ của nhãn tháng.",
    "MonthLabelFontSize": "Cỡ chữ nhãn tháng.",
    "Speed": "Tốc độ chạy (pixel/giây).",
    "Gap": "Khoảng cách giữa hai lần lặp nội dung (px).",
    "Separator": "Chuỗi chèn giữa các lần lặp nội dung.",
    "IsRunning": "Đang chạy animation.",
    "TitleBarBackground": "Màu nền thanh tiêu đề của cửa sổ.",
    "Progress2": "",
}

PREFIX_RULES = [
    (r"^Is([A-Z]\w*)$", lambda m: f"Bật/tắt trạng thái <b>{split_words(m.group(1))}</b>."),
    (r"^Show([A-Z]\w*)$", lambda m: f"Hiển thị thành phần <b>{split_words(m.group(1))}</b>."),
    (r"^Has([A-Z]\w*)$", lambda m: f"Cho biết có <b>{split_words(m.group(1))}</b> hay không (read-only)."),
    (r"^Max([A-Z]\w*)$", lambda m: f"Giá trị lớn nhất của <b>{split_words(m.group(1))}</b>."),
    (r"^Min([A-Z]\w*)$", lambda m: f"Giá trị nhỏ nhất của <b>{split_words(m.group(1))}</b>."),
    (r"^On([A-Z]\w*)$", lambda m: f"Trạng thái bật của <b>{split_words(m.group(1))}</b>."),
    (r"^(\w+)Brush$", lambda m: f"Brush/màu cho <b>{split_words(m.group(1))}</b>."),
    (r"^(\w+)Template$", lambda m: f"DataTemplate cho <b>{split_words(m.group(1))}</b>."),
    (r"^(\w+)Command$", lambda m: f"Lệnh thực thi cho <b>{split_words(m.group(1))}</b>."),
]


def split_words(s):
    return re.sub(r"(?<!^)(?=[A-Z])", " ", s).lower()


def describe(name, ptype):
    if name in D and D[name]:
        return D[name]
    for pattern, fn in PREFIX_RULES:
        m = re.match(pattern, name)
        if m:
            return fn(m)
    return f"Thiết lập <code>{name}</code> ({ptype})."


# ---------------------------------------------------------------- english descriptions

EN_D = {
    "Value": "Current value of the control (two-way bound by default).",
    "Text": "Text content shown or edited by the control.",
    "Title": "Title displayed above the content.",
    "Header": "Header content.",
    "Icon": "Vector icon shown next to the content.",
    "IconKind": "Icon glyph (see <code>IconKind</code>).",
    "IconSize": "Icon size in pixels.",
    "IconPlacement": "Icon position relative to the content (<code>Dock.Left</code>, <code>Right</code>…).",
    "CornerRadius": "Corner radius of the control frame.",
    "Content": "Main content of the control.",
    "ItemsSource": "Data source (usually <code>IEnumerable</code>) used to generate items.",
    "Items": "Items declared inline in XAML.",
    "SelectedItem": "Currently selected item.",
    "SelectedIndex": "Index of the selected item.",
    "SelectedValue": "Value of the selected item.",
    "IsSelected": "Whether the item/control is selected.",
    "IsOpen": "Whether the control/popup is open.",
    "IsDropDownOpen": "Whether the dropdown is open.",
    "IsLoading": "Toggles the loading state (skeleton, overlay, spinner).",
    "IsActive": "Turns the effect or active mode on/off.",
    "IsExpanded": "Expansion state of the expander.",
    "IsReadOnly": "Read-only mode — content can be selected and copied but not edited.",
    "IsIndeterminate": "Indeterminate progress mode (no percentage).",
    "IsClosable": "Allows the user to close the tag.",
    "IsEnabled": "Enables or disables the effect (attached property).",
    "IsDot": "Renders a dot instead of a number (notification badge).",
    "IsPassword": "Masks the content with password characters.",
    "IsValid": "Result of the input validation (read-only).",
    "ShowPassword": "Reveals or hides the typed password.",
    "ShowSeconds": "Shows the seconds component.",
    "ShowCloseButton": "Shows the close button.",
    "ShowSelectAll": "Shows the select-all option.",
    "ShowSearch": "Shows the search box inside the list.",
    "ShowBorder": "Draws a border around the control.",
    "ShowBadge": "Shows the count badge.",
    "ShowLegend": "Shows the chart legend.",
    "ShowArea": "Fills the area below the line.",
    "ShowGrid": "Shows the background grid.",
    "ShowAlpha": "Allows picking the alpha channel.",
    "ShowScore": "Shows the numeric score next to the stars.",
    "ShowCrosshair": "Shows the magnifier crosshair.",
    "ShowToolbar": "Shows the spreadsheet toolbar.",
    "ShowFormulaBar": "Shows the formula input bar.",
    "ShowSheetTabs": "Shows the sheet tabs.",
    "ShowStatusBar": "Shows the bottom status bar.",
    "ShowFooterSummary": "Shows the aggregated footer row.",
    "ShowToggleButton": "Shows the collapse/expand button.",
    "ShowText": "Shows the percentage/value on the bar.",
    "Minimum": "Smallest allowed value.",
    "Maximum": "Largest allowed value.",
    "Min": "Lowest value of the scale.",
    "Max": "Highest value of the scale.",
    "MaxPage": "Maximum number of pages.",
    "MaxZoom": "Maximum zoom level.",
    "MinZoom": "Minimum zoom level.",
    "Increment": "Step applied by the buttons and arrow keys.",
    "DecimalPlaces": "Number of decimal digits shown.",
    "Suffix": "Unit/suffix rendered after the value (e.g. <code> kN</code>).",
    "Prefix": "Prefix rendered before the value.",
    "FormatString": "Numeric .NET format string (e.g. <code>N2</code>, <code>0.###</code>).",
    "StringFormat": "Display format of the cell.",
    "Unit": "Measurement unit shown with the value.",
    "UnitScale": "Ruler unit scale (pixels → mm).",
    "MajorStep": "Distance between major ruler ticks.",
    "OriginOffset": "Ruler origin offset relative to the canvas.",
    "Orientation": "Layout direction (<code>Horizontal</code> / <code>Vertical</code>).",
    "Placement": "Where the popup or drawer opens.",
    "Placeholder": "Hint text shown while the input is empty.",
    "Description": "Supporting text below the title.",
    "Subtitle": "Secondary line of the card.",
    "Message": "Message body.",
    "SubMessage": "Secondary line of the message.",
    "Progress": "Progress value from 0 to 100.",
    "Status": "Business status rendered on the control.",
    "Tag": "Arbitrary payload attached to an item (usually for commands).",
    "TagName": "Identifier used for routing a navigation item.",
    "Command": "Command executed on interaction.",
    "CommandParameter": "Parameter passed to <code>Command</code>.",
    "CommandTarget": "Element that receives the routed command.",
    "ActionCommand": "Command for the primary action button.",
    "ActionButtonText": "Label of the primary action button.",
    "ActionButtonIcon": "Icon of the primary action button.",
    "SecondaryActionText": "Label of the secondary action button.",
    "ExtraContent": "Additional custom content area.",
    "Footer": "Footer content.",
    "FooterTemplate": "Template for the footer.",
    "HeaderTemplate": "Template for the header.",
    "ItemTemplate": "Template used for each item.",
    "DropDownContent": "Content of the dropdown area.",
    "DropDownContentTemplate": "Template for the dropdown content.",
    "MenuContent": "Context menu content of the sprite.",
    "SourceContent": "Original content (left side) of the comparison slider.",
    "TargetContent": "Compared content (right side).",
    "MaxDropDownHeight": "Maximum dropdown height.",
    "DropDownButtonPosition": "Position of the dropdown button inside the split button.",
    "HitMode": "Click behaviour: <code>Split</code> (separate action and arrow) or <code>DropDown</code>.",
    "AutoCloseOnClick": "Closes the dropdown after an item is picked.",
    "StaysOpen": "Keeps the dropdown open when clicking outside.",
    "IsOpenOnHover": "Opens the dropdown on hover.",
    "HoverDelay": "Delay before the dropdown opens on hover.",
    "Appearance": "Colour variant (<code>Primary</code>, <code>Success</code>, <code>Warning</code>, <code>Danger</code>, <code>Info</code>, <code>Neutral</code>).",
    "Badge": "Count/notification badge attached to the item.",
    "HasBadge": "Whether the item has a badge (read-only).",
    "BadgeMargin": "Offset of the badge from the control corner.",
    "DotSize": "Diameter of the indicator dot in pixels.",
    "IndicatorType": "Loading style: <code>ThreeDotsWave</code>, <code>CircularDotsRing</code>, <code>LinePulse</code>, <code>PulseCircle</code>.",
    "Duration": "Animation duration.",
    "SkeletonShape": "Placeholder shape: <code>Rectangle</code>, <code>Circle</code>, <code>TextLine</code>, <code>Card</code>.",
    "BaseBrush": "Background brush for empty cells/blocks (default state).",
    "HighlightBrush": "Shimmer highlight brush.",
    "IsWaveEnabled": "Enables the shimmer wave.",
    "RippleBrush": "Ripple colour.",
    "MaxOpacity": "Maximum ripple opacity.",
    "MaxConcurrentRipples": "Maximum number of simultaneous ripples.",
    "IsRippleEnabled": "Enables the ripple effect.",
    "Mode": "Operating mode (meaning depends on the control).",
    "Length": "Number of cells/characters to enter.",
    "Password": "Current password value.",
    "PasswordChar": "Character used to mask the password.",
    "ItemWidth": "Width of each item.",
    "ItemHeight": "Height of each item.",
    "ItemMargin": "Spacing between items.",
    "ItemSize": "Size of each item (rating).",
    "ItemCornerRadius": "Corner radius of each input cell.",
    "RowHeight": "Height of a data row.",
    "RowHeaderWidth": "Width of the row header column.",
    "ColumnHeaderHeight": "Height of the column header.",
    "FooterRowHeight": "Height of the footer row.",
    "StackedRowHeight": "Height of each header level.",
    "PageSize": "Items per page.",
    "PageIndex": "Current page index (1-based).",
    "CurrentPage": "Current page of the pager.",
    "TotalItems": "Total number of items to page through.",
    "CurrentLayout": "Layout chosen for the available width (<code>Compact</code>, <code>Normal</code>, <code>Wide</code>).",
    "CompactWidth": "Width in collapsed mode.",
    "ExpandedWidth": "Width in expanded mode.",
    "IsCollapsed": "Menu is collapsed to icons only.",
    "Children": "Child items (hierarchical menu).",
    "ChildColumns": "Nested columns of a stacked header.",
    "ColumnHeaderTree": "Header tree structure (built automatically).",
    "StackedHeaderRows": "Header level definitions.",
    "CellType": "Cell renderer: <code>Text</code>, <code>Numeric</code>, <code>Progress</code>, <code>Badge</code>, <code>Rating</code>, <code>Date</code>, <code>Hyperlink</code>, <code>CheckBox</code>.",
    "BindingPath": "Property path used to bind the cell.",
    "SortMemberPath": "Property used for sorting.",
    "CanUserSort": "Allows the user to sort by this column.",
    "CanUserResize": "Allows the user to resize this column.",
    "AggregateType": "Footer aggregate: <code>Sum</code>, <code>Average</code>, <code>Count</code>, <code>Min</code>, <code>Max</code>, <code>Custom</code>.",
    "AggregateFormat": "Number format of the aggregate.",
    "CustomAggregateText": "Custom aggregate text (when <code>AggregateType=Custom</code>).",
    "EnableContextMenu": "Enables the default grid context menu.",
    "SelectionRange": "Selected cell range in the spreadsheet.",
    "ActiveRow": "Index of the active row.",
    "ActiveColumn": "Index of the active column.",
    "ActiveSheet": "Currently open sheet.",
    "Sheets": "Sheets of the workbook.",
    "FormulaText": "Formula of the selected cell.",
    "GridLineBrush": "Grid line colour.",
    "HeaderBackground": "Header background brush.",
    "SelectionBrush": "Selection background brush.",
    "SelectionBorderBrush": "Selection border brush.",
    "Trend": "Trend value shown on the KPI card (e.g. <code>+12.4%</code>).",
    "TrendDirection": "Trend direction: <code>Up</code>, <code>Down</code>, <code>Neutral</code>.",
    "Time": "Timestamp of the timeline entry.",
    "IsLast": "Marks the final entry (hides the connector line).",
    "DotBrush": "Timeline dot colour.",
    "Type": "Display type (meaning depends on the control).",
    "TimeString": "Time label shown on the toast.",
    "GrowlType": "Toast type: <code>Info</code>, <code>Success</code>, <code>Warning</code>, <code>Error</code>, <code>Fatal</code>, <code>Ask</code>.",
    "DialogWidth": "Dialog width.",
    "DialogHeight": "Dialog height.",
    "MaskCanClose": "Allows clicking the mask to close.",
    "MessageType": "Dialog content type: <code>Information</code>, <code>Success</code>, <code>Warning</code>, <code>Error</code>, <code>Question</code>.",
    "Token": "Identifier used to close/open the dialog by key.",
    "DrawerWidth": "Drawer width.",
    "DrawerHeight": "Drawer height.",
    "Categories": "Property groups of the property grid.",
    "SelectedObject": "Object being inspected.",
    "SelectedPath": "Selected file or folder path.",
    "Filter": "Win32 file filter (e.g. <code>*.dwg;*.dxf</code>).",
    "SeparatorIcon": "Icon between breadcrumb items.",
    "LineStyle": "Line style: <code>Solid</code>, <code>Dashed</code>, <code>Dotted</code>.",
    "TextAlignment": "Text alignment.",
    "Angle": "Watermark rotation angle in degrees.",
    "MarkOpacity": "Watermark opacity.",
    "MarkBrush": "Watermark brush.",
    "MarkFontSize": "Watermark font size.",
    "SnowflakeCount": "Number of snowflakes.",
    "SnowflakeBrush": "Snowflake colour.",
    "MinSnowflakeSize": "Smallest snowflake size.",
    "MaxSnowflakeSize": "Largest snowflake size.",
    "MinSpeed": "Slowest fall speed.",
    "MaxSpeed": "Fastest fall speed.",
    "Wind": "Horizontal wind force applied to the flakes.",
    "AutoSnap": "Snaps to the screen edge while dragging.",
    "AutoRun": "Runs the carousel automatically.",
    "Interval": "Automatic cycle interval (ms).",
    "Spacing": "Spacing between items.",
    "Scale": "Scale of the active item.",
    "MinScale": "Scale of inactive items.",
    "OffsetX": "Horizontal offset of the item in the carousel.",
    "Zoom": "Current canvas zoom level.",
    "GridSize": "Grid cell size in pixels.",
    "PanOffsetX": "Horizontal pan offset of the viewport.",
    "PanOffsetY": "Vertical pan offset of the viewport.",
    "AreaMm2": "Rebar area As (mm²) — read-only, computed from the notation.",
    "Target": "Element or bitmap to magnify.",
    "Radius": "Magnified area radius.",
    "Center": "Centre of the magnified area.",
    "HandleSize": "Size of the drag handle.",
    "Value1": "Lower value of the range.",
    "Value2": "Upper value of the range.",
    "StartValue": "Start of the range.",
    "EndValue": "End of the range.",
    "SelectedColor": "Currently selected colour.",
    "HexCode": "Hex code of the selected colour.",
    "SelectedTime": "Time selected on the clock.",
    "SelectedDateTime": "Selected date and time.",
    "DisplayMode": "Unit being edited: <code>Hour</code>, <code>Minute</code>, <code>Second</code>.",
    "SourceTitle": "Title of the source list.",
    "TargetTitle": "Title of the target list.",
    "TransferredItems": "Items moved to the target list.",
    "ListWidth": "Width of each list.",
    "ListHeight": "Height of each list.",
    "SelectedItemsPreviewMaxWidth": "Maximum width of the selected-items preview.",
    "SelectAllText": "Label of the select-all entry.",
    "DisplayText": "Text describing the current selection (read-only).",
    "DisplayMemberPath": "Property used as the item text.",
    "IsInstantSearch": "Searches while typing (no Enter required).",
    "SearchCommand": "Command executed on search.",
    "Index": "Index of the current step.",
    "StepIndex": "Index of the step inside the step bar.",
    "Dock": "Docking position of the step bar.",
    "Data": "Chart data points.",
    "Datas": "Chart data (collection of points).",
    "Segments": "Slices of the pie chart.",
    "BarBrush": "Bar colour of the bar chart.",
    "LineBrush": "Line colour of the line chart.",
    "RadarBrush": "Area colour of the radar chart.",
    "MultiColor": "Gives each bar its own colour.",
    "Smooth": "Draws a smooth spline instead of straight segments.",
    "CenterText": "Text rendered in the middle of the pie chart.",
    "ArcThickness": "Thickness of the donut arc.",
    "GridLevels": "Number of radar grid rings.",
    "TargetGrid": "Grid targeted by the grouping panel.",
    "OffContent": "Content shown in the off state.",
    "OnContent": "Content shown in the on state.",
    "SmallChange": "Small step used by the arrow keys.",
    "Brush": "Brush used by the effect (attached).",
    "Color": "Colour of the element.",
    "Name": "Identifier.",
    "Operator": "Comparison operator of the formatting rule.",
    "DragDropGroup": "Drag-and-drop group — only items in the same group accept drops.",
    "IsDragSource": "Allows dragging this element.",
    "IsDropTarget": "Allows dropping onto this element.",
    "EmptyMessage": "Message shown when the list is empty.",
    "ToolTip": "Tooltip shown on hover.",
    "Width": "Width.",
    "MinWidth": "Minimum width.",
    "MaxWidth": "Maximum width.",
    "HorizontalAlignment": "Horizontal alignment.",
    "Background": "Background brush.",
    "Foreground": "Foreground brush.",
    "SpotlightBrush": "Colour of the cursor-following spotlight (defaults to the theme accent).",
    "SpotlightSize": "Diameter of the spotlight area in pixels.",
    "SpotlightOpacity": "Maximum spotlight opacity (0–1).",
    "IsSpotlightEnabled": "Enables the spotlight effect (turn it off on long lists to keep FPS).",
    "BorderGradient": "Border brush revealed on hover — use a <code>LinearGradientBrush</code> for a multi-colour edge.",
    "ShowHoverBorder": "Shows the border on hover (turn off to keep only the spotlight).",
    "DisplayText": "Formatted text of the current value (read-only, updates while the number rolls).",
    "AnimateOnLoad": "Runs the animation the first time a value arrives.",
    "Values": "Series to plot (auto-scales to the available box).",
    "StrokeThickness": "Line thickness in pixels.",
    "ShowArea": "Fills the area under the line with a gradient.",
    "ShowLastDot": "Marks the newest data point with a dot.",
    "Smooth": "Draws Bezier curves instead of straight segments.",
    "LastDotBrush": "Brush of the last-point marker.",
    "MaxVisible": "How many avatars render; the rest collapse into the +N chip.",
    "Overlap": "How far two avatars overlap, in pixels.",
    "RingBrush": "Ring colour between avatars.",
    "ShowOverflow": "Shows the +N chip when the item count exceeds <code>MaxVisible</code>.",
    "OverflowTextFormat": "Format string of the overflow chip (default <code>+{0}</code>).",
    "Items": "Item list: strings (initials are derived) or <code>ImageSource</code>.",
    "Size": "Edge size of the element in pixels — the avatar diameter for AvatarStack.",
    "FontSize": "Font size used inside the control.",
    "StartDate": "First day of the series (the first element maps to this date).",
    "CellSize": "Side length of each cell in pixels.",
    "CellGap": "Spacing between cells in pixels.",
    "Level1Brush": "Level 1 colour — lowest intensity (above zero).",
    "Level2Brush": "Level 2 colour.",
    "Level3Brush": "Level 3 colour.",
    "Level4Brush": "Level 4 colour — highest intensity.",
    "ShowMonthLabels": "Shows month labels above the grid.",
    "MonthLabelBrush": "Brush of the month labels.",
    "MonthLabelFontSize": "Font size of the month labels.",
    "Speed": "Scroll speed in pixels per second.",
    "Gap": "Spacing between two repetitions of the content, in pixels.",
    "Separator": "String inserted between repetitions.",
    "IsRunning": "Whether the animation is running.",
    "TitleBarBackground": "Title bar background of the window.",
}

EN_PREFIX_RULES = [
    (r"^Is([A-Z]\w*)$", lambda m: f"Toggles the <b>{split_words(m.group(1))}</b> state."),
    (r"^Show([A-Z]\w*)$", lambda m: f"Shows the <b>{split_words(m.group(1))}</b> part."),
    (r"^Has([A-Z]\w*)$", lambda m: f"Whether the control has <b>{split_words(m.group(1))}</b> (read-only)."),
    (r"^Max([A-Z]\w*)$", lambda m: f"Upper bound of <b>{split_words(m.group(1))}</b>."),
    (r"^Min([A-Z]\w*)$", lambda m: f"Lower bound of <b>{split_words(m.group(1))}</b>."),
    (r"^On([A-Z]\w*)$", lambda m: f"On-state value of <b>{split_words(m.group(1))}</b>."),
    (r"^(\w+)Brush$", lambda m: f"Brush used for <b>{split_words(m.group(1))}</b>."),
    (r"^(\w+)Template$", lambda m: f"DataTemplate for <b>{split_words(m.group(1))}</b>."),
    (r"^(\w+)Command$", lambda m: f"Command executed for <b>{split_words(m.group(1))}</b>."),
]


def describe_en(name, ptype):
    if name in EN_D and EN_D[name]:
        return EN_D[name]
    for pattern, fn in EN_PREFIX_RULES:
        m = re.match(pattern, name)
        if m:
            return fn(m)
    return f"Sets <code>{name}</code> ({ptype})."


# ---------------------------------------------------------------- csharp snippets

SKIP_PROPS = {
    "Background", "Foreground", "ToolTip", "Width", "MinWidth", "MaxWidth", "Height",
    "HorizontalAlignment", "VerticalAlignment", "Margin", "Padding", "Style", "Template",
    "Command", "CommandParameter", "CommandTarget", "HeaderTemplate", "ItemTemplate",
    "FooterTemplate", "DropDownContentTemplate", "BindingPath", "SortMemberPath",
    "CanUserSort", "CanUserResize", "IsEnabled", "Tag", "Name", "IsReadOnly",
    "ItemsSource", "Items", "Data", "Datas", "Children", "Sheets", "Categories",
    "IsValid", "AreaMm2", "CanIncrease", "CanDecrease", "HasBadge", "SelectionRange",
    "SelectedObject", "FormulaText", "ActiveSheet", "ActiveRow", "ActiveColumn",
}

VALUE_FOR_TYPE = {
    "bool": "true",
    "double": "8",
    "int": "4",
    "string": '"Sample"',
    "object": '"Sample"',
    "TimeSpan": "TimeSpan.FromMilliseconds(400)",
    "Thickness": "new Thickness(8)",
    "CornerRadius": "new CornerRadius(8)",
    "DateTime": "DateTime.Now",
    "Color": "Colors.DodgerBlue",
}


NAME_VALUES = {
    "Zoom": "1.0", "MinZoom": "0.25", "MaxZoom": "8.0", "GridSize": "20.0",
    "DecimalPlaces": "2", "DotSize": "8", "Duration": "TimeSpan.FromMilliseconds(450)",
    "Interval": "TimeSpan.FromSeconds(3)", "ItemWidth": "160", "ItemHeight": "96",
    "ItemSize": "18", "Spacing": "12", "Length": "6", "PageSize": "20",
    "PageIndex": "1", "CurrentPage": "1", "TotalItems": "128", "MaxPage": "12",
    "Minimum": "0", "Maximum": "1000", "Min": "0", "Max": "5", "Value": "1",
    "Increment": "5", "MajorStep": "50", "UnitScale": "1.0", "Radius": "80",
    "Angle": "0", "MarkOpacity": "0.06", "MarkFontSize": "96", "Scale": "1.08",
    "MinScale": "0.92", "RowHeight": "32", "RowHeaderWidth": "46",
    "ColumnHeaderHeight": "38", "CompactWidth": "64", "ExpandedWidth": "240",
    "DialogWidth": "520", "DialogHeight": "320", "DrawerWidth": "380",
    "DrawerHeight": "100%", "ListWidth": "220", "ListHeight": "260",
    "MaxDropDownHeight": "320", "HandleSize": "28", "CenterText": "\"TOTAL\"",
    "Unit": "\"mm\"", "Header": "\"Beam width\"", "Title": "\"Structural summary\"",
}

PRIORITY = [
    "Value", "Text", "Content", "Title", "Header", "Icon", "IndicatorType", "SkeletonShape",
    "Appearance", "Orientation", "Placement", "Unit", "Placeholder", "Mode", "Length",
    "IsLoading", "IsOpen", "Progress", "Description",
]


def pick_props(props, limit=3):
    usable = [p for p in props if p["name"] not in SKIP_PROPS and not p["readonly"]]
    picked, seen = [], set()
    for want in PRIORITY:
        for p in usable:
            if p["name"] == want and p["name"] not in seen:
                picked.append(p)
                seen.add(p["name"])
    for p in usable:
        if len(picked) >= limit:
            break
        if p["name"] not in seen:
            picked.append(p)
            seen.add(p["name"])
    return picked[:limit]


def value_for(p, enums):
    if p["name"] in NAME_VALUES:
        return NAME_VALUES[p["name"]]
    t = p["type"]
    if t in VALUE_FOR_TYPE:
        return VALUE_FOR_TYPE[t]
    if t in enums:
        members = [m for m in enums[t] if m not in ("None", "Default")]
        return f"{t}.{members[0]}" if members else f"{t}.{enums[t][0]}"
    if t.endswith("Brush") or t == "Brush":
        return '(Brush)Application.Current.FindResource("CONCOST.Brush.Primary")'
    return None


def build_csharp(cls, entry, enums):
    chosen = pick_props(entry["props"])
    sample = "Sample" + cls
    lines = [f"// XAML: <ui:{cls} x:Name=\"{sample}\" />"]
    for p in chosen:
        v = value_for(p, enums)
        if v:
            lines.append(f"{sample}.{p['name']} = {v};")
    if len(lines) == 1:
        lines.append(f"{sample}.ToolTip = \"CONCOST.UI\";")
    ev = entry["events"][0] if entry["events"] else None
    if ev:
        lines += ["", "// Sự kiện", f"{sample}.{ev} += (s, e) =>", "{",
                  f"    Debug.WriteLine(\"{cls}.{ev} raised\");", "};"]
    return "\n".join(lines)


# ---------------------------------------------------------------- page map

PAGE_CLASS = {
    # engineering
    "spreadsheet": ["Spreadsheet"],
    "stackedheaderdatagrid": ["StackedHeaderDataGrid", "StackedColumn", "ColumnLeaf", "StackedHeaderRow"],
    "cadcanvas": ["ZoomableCanvas", "RulerControl"],
    "diagramchart": ["ChartLine", "ChartBase"],
    "charts": ["ChartBase", "ChartBar", "ChartLine", "ChartRadar", "ChartPie"],
    "rebarinput": ["RebarInput"],
    "engineeringboxes": ["UnitBox", "LengthBox", "ForceBox", "MomentBox", "PressureBox", "VolumeBox"],
    "compassindicator": ["CompassIndicator"],
    # dashboard / data
    "cardcarousel": ["CardCarousel", "CardCarouselItem"],
    "timeline": ["Timeline", "TimelineItem"],
    "statisticcard": ["StatisticCard"],
    "datapager": ["DataPager"],
    # feedback
    "dialog": ["Dialog"],
    "drawer": ["Drawer"],
    "growl": ["Growl", "GrowlItem"],
    "loadings": ["LoadingIndicator", "LoadingBase", "CycleLoading", "LoadingCircle", "LoadingLine"],
    "loadingoverlay": ["LoadingOverlay"],
    "skeleton": ["Skeleton"],
    "ripple": ["Ripple", "RippleElement"],
    # input
    "numericupdown": ["NumericUpDown"],
    "propertygrid": ["PropertyGrid"],
    "checkcombobox": ["CheckComboBox"],
    "pinbox": ["PinBox"],
    "otpinput": ["OtpInput"],
    "passwordinput": ["PasswordInput"],
    "colorpicker": ["ColorPicker"],
    "rangeslider": ["RangeSlider"],
    "autocompletetextbox": ["AutoCompleteTextBox"],
    "searchbox": ["SearchBox"],
    "filepicker": ["FilePicker"],
    "rating": ["Rating"],
    "tag": ["Tag"],
    "toggleswitch": ["ToggleSwitch"],
    "calendarwithclock": ["CalendarWithClock"],
    "clock": ["Clock"],
    # navigation
    "stepbar": ["StepBar", "StepBarItem"],
    "transfer": ["Transfer"],
    "sidemenu": ["SideMenu", "SideMenuItem"],
    "drawermenu": ["DrawerMenu", "DrawerMenuItem"],
    "breadcrumb": ["Breadcrumb", "BreadcrumbItem"],
    "pagination": ["Pagination"],
    "navigationview": ["NavigationView", "NavigationItem"],
    # layout / shell
    "adaptivepanel": ["AdaptivePanel"],
    "concostwindow": ["CONCOSTWindow"],
    "expander": ["Expander"],
    "splitbutton": ["SplitButton"],
    "badge": ["Badge"],
    "card": ["Card"],
    "divider": ["Divider"],
    "emptystate": ["EmptyState"],
    "watermark": ["Watermark"],
    # web-inspired
    "spotlightcard": ["SpotlightCard"],
    "odometernumber": ["OdometerNumber"],
    "sparkline": ["Sparkline"],
    "avatarstack": ["AvatarStack"],
    "heatmapcalendar": ["HeatmapCalendar"],
    "marqueeticker": ["MarqueeTicker"],
    "aurorabackground": ["AuroraBackground"],
    "borderbeam": ["BorderBeam"],
    "confettiburst": ["ConfettiBurst"],
    # effects
    "snowcanvas": ["SnowCanvas"],
    "sprite": ["Sprite"],
    "compareslider": ["CompareSlider"],
    "magnifier": ["Magnifier"],
    # misc
    "icons": ["Icon"],
}


PRIMARY = {
    "charts": "ChartBar",
    "diagramchart": "ChartLine",
    "cadcanvas": "ZoomableCanvas",
    "engineeringboxes": "UnitBox",
    "loadings": "LoadingIndicator",
}


def js(obj):
    return json.dumps(obj, ensure_ascii=False, indent=2)


def main():
    classes, enums = extract()
    auto = OrderedDict()
    missing = []

    for page, class_names in PAGE_CLASS.items():
        merged, events, ns = [], [], ""
        primary = PRIMARY.get(page, class_names[0])
        for cname in class_names:
            entry = classes.get(cname)
            if not entry:
                missing.append(cname)
                continue
            if not ns:
                ns = entry["namespace"]
            events += [e for e in entry["events"] if e not in events]
            known = {p["name"] for p in merged}
            for p in entry["props"]:
                if p["name"] in known:
                    continue
                p = dict(p)
                p["desc"] = describe(p["name"], p["type"])
                merged.append(p)

        if not merged and not events:
            continue

        csharp = ""
        if classes.get(primary):
            csharp = build_csharp(primary, classes[primary], enums)

        auto[page] = {
            "class": primary,
            "namespace": ns,
            "props": merged,
            "events": events,
            "csharp": csharp,
        }

    # English descriptions for every property name in the library, used by the
    # language switch regardless of whether the table is generated or hand-written.
    en_props = OrderedDict()
    for cname, info in classes.items():
        for p in info["props"]:
            if p["name"] not in en_props:
                en_props[p["name"]] = describe_en(p["name"], p["type"])
    en_props = OrderedDict(sorted(en_props.items(), key=lambda kv: kv[0].lower()))

    header = (
        "// AUTO-GENERATED by docs/tools/build-props.py — do not edit by hand.\n"
        "// Source of truth: src/CONCOST.UI/**/*.cs (DependencyProperty registrations).\n"
        f"// Pages: {len(auto)} · Classes scanned: {len(classes)} · EN property descriptions: {len(en_props)}\n"
    )
    with open(OUT, "w", encoding="utf-8", newline="\n") as fh:
        fh.write(header)
        fh.write("const DOCS_AUTO = ")
        fh.write(js(auto))
        fh.write(";\n\n")
        fh.write("const DOCS_EN_PROPS = ")
        fh.write(js(en_props))
        fh.write(";\n")

    total_props = sum(len(v["props"]) for v in auto.values())
    print(f"pages: {len(auto)} | props: {total_props} | classes: {len(classes)} | en props: {len(en_props)}")
    if missing:
        print("missing classes:", ", ".join(missing))


if __name__ == "__main__":
    main()
