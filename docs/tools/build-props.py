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
    "BaseBrush": "Màu nền của khối skeleton.",
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

    header = (
        "// AUTO-GENERATED by docs/tools/build-props.py — do not edit by hand.\n"
        "// Source of truth: src/CONCOST.UI/**/*.cs (DependencyProperty registrations).\n"
        f"// Pages: {len(auto)} · Classes scanned: {len(classes)}\n"
    )
    with open(OUT, "w", encoding="utf-8", newline="\n") as fh:
        fh.write(header)
        fh.write("const DOCS_AUTO = ")
        fh.write(js(auto))
        fh.write(";\n")

    total_props = sum(len(v["props"]) for v in auto.values())
    print(f"pages: {len(auto)} | props: {total_props} | classes: {len(classes)}")
    if missing:
        print("missing classes:", ", ".join(missing))


if __name__ == "__main__":
    main()
