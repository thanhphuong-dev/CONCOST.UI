// Basic control pages (styling-only controls) + attached-property enrichment
// for the pages that originally shipped without a property table.
const DOCS_BASIC = {

    /* ============================ NEW BASIC PAGES ============================ */
    checkbox: {

        title: "CheckBox — ô chọn & segmented",
        badge: "Controls cơ bản",
        desc: "CheckBox với 3 biến thể (mặc định, dạng button, dạng button outline), hỗ trợ trạng thái ba trị và label có title/required qua attached properties.",
        keywords: ["checkbox", "chọn", "tri-state", "check"],
        variants: [
            { key: "CONCOST.CheckBox.Base", desc: "Ô vuông + label, tick màu accent." },
            { key: "CONCOST.CheckBox.Button", desc: "Hiển thị như ToggleButton nền đặc khi chọn." },
            { key: "CONCOST.CheckBox.Button.Outline", desc: "Như trên nhưng chỉ viền." }
        ],
        sections: [
            {
                title: "CheckBox có tiêu đề và bắt buộc",
                desc: "<code>IsThreeState=\"True\"</code> cho phép trạng thái null (dùng cho “chọn tất cả”). Attached <code>InfoElement.Title</code> đặt nhãn phía trên.",
                xaml: `<StackPanel Margin="24">
    <CheckBox Content="Hiển thị lưới trục"
              IsChecked="True"
              Style="{DynamicResource CONCOST.CheckBox.Base}"
              Margin="0,0,0,10" />

    <CheckBox Content="Bao gồm tải trọng gió"
              Style="{DynamicResource CONCOST.CheckBox.Button}"
              Margin="0,0,0,10" />

    <CheckBox Content="Chọn tất cả cấu kiện"
              IsThreeState="True"
              IsChecked="{x:Null}"
              Style="{DynamicResource CONCOST.CheckBox.Button.Outline}" />
</StackPanel>`,
                csharp: `// Chọn tất cả: lan trạng thái xuống item
void OnSelectAll(object sender, RoutedEventArgs e)
{
    bool? state = SelectAllCheckBox.IsChecked;
    foreach (var row in Rows) row.IsSelected = state == true;
}

// Đồng bộ ngược: nếu mọi dòng được chọn thì checkbox = true
SelectAllCheckBox.IsChecked = Rows.All(r => r.IsSelected) ? true
    : Rows.Any(r => r.IsSelected) ? (bool?)null : false;`
            }
        ],
        related: ["togglebutton", "toggleswitch", "radiobutton"]
    },

    radiobutton: {

        title: "RadioButton — nhóm lựa chọn",
        badge: "Controls cơ bản",
        desc: "RadioButton với 4 biến thể, gồm <code>Segment</code> cho nhóm lựa chọn dạng segmented control (rất hợp cho bộ lọc chế độ xem).",
        keywords: ["radio", "lựa chọn", "segment", "nhóm"],
        variants: [
            { key: "CONCOST.RadioButton.Base", desc: "Nút tròn + label." },
            { key: "CONCOST.RadioButton.Button", desc: "Dạng button nền đặc khi chọn." },
            { key: "CONCOST.RadioButton.Button.Outline", desc: "Dạng button viền." },
            { key: "CONCOST.RadioButton.Segment", desc: "Segmented control liền khối — dùng cho chuyển chế độ xem." }
        ],
        sections: [
            {
                title: "Segmented control cho chế độ hiển thị",
                desc: "Cùng <code>GroupName</code> thì chỉ một nút được chọn. Đặt trong <code>Border</code> bo góc để tạo khối segmented hoàn chỉnh.",
                xaml: `<Border CornerRadius="8" Padding="3"
        Background="{DynamicResource CONCOST.Brush.SurfaceSecondary}">
    <StackPanel Orientation="Horizontal">
        <RadioButton Content="2D" GroupName="ViewMode" IsChecked="True"
                     Style="{DynamicResource CONCOST.RadioButton.Segment}" />
        <RadioButton Content="3D" GroupName="ViewMode"
                     Style="{DynamicResource CONCOST.RadioButton.Segment}" />
        <RadioButton Content="Section" GroupName="ViewMode"
                     Style="{DynamicResource CONCOST.RadioButton.Segment}" />
    </StackPanel>
</Border>`,
                csharp: `// Đọc lựa chọn qua Tag thay vì so sánh Content
foreach (RadioButton rb in ViewModePanel.Children.OfType<RadioButton>())
{
    rb.Checked += (s, e) =>
    {
        var mode = (s as RadioButton)?.Content?.ToString();
        Viewport.Mode = mode switch
        {
            "2D" => ViewMode.Plan,
            "3D" => ViewMode.Model,
            _ => ViewMode.Section
        };
    };
}`
            }
        ],
        related: ["checkbox", "toggleswitch", "tabcontrol"]
    },

    slider: {

        title: "Slider — thanh trượt giá trị",
        badge: "Controls cơ bản",
        desc: "Slider được restyle với thumb và repeat button riêng, giữ nguyên toàn bộ API của WPF <code>Slider</code>.",
        keywords: ["slider", "kéo", "range", "giá trị"],
        variants: [
            { key: "CONCOST.Slider.Base", desc: "Track mảnh, thumb tròn accent." },
            { key: "CONCOST.Slider.Thumb", desc: "Style cho thumb (dùng khi cần thumb lớn hơn)." },
            { key: "CONCOST.Slider.RepeatButton.Horizontal", desc: "Vùng bấm hai bên track (ngang)." },
            { key: "CONCOST.Slider.RepeatButton.Vertical", desc: "Vùng bấm hai bên track (dọc)." }
        ],
        sections: [
            {
                title: "Slider rời rạc có tick",
                desc: "<code>IsSnapToTickEnabled</code> + <code>TickFrequency</code> ép giá trị về bước cố định — phù hợp chọn cấp bê tông, đường kính thép.",
                xaml: `<StackPanel Margin="24" Width="320">
    <TextBlock Text="{Binding ElementName=OpacitySlider, Path=Value, StringFormat=Opacity: {0:0}%}"
               Style="{DynamicResource CONCOST.TextBlock.Caption}" Margin="0,0,0,6" />

    <Slider x:Name="OpacitySlider"
            Minimum="0" Maximum="100" Value="65"
            TickFrequency="5" IsSnapToTickEnabled="True"
            Style="{DynamicResource CONCOST.Slider.Base}" />

    <Slider Minimum="0" Maximum="10" Value="4"
            Orientation="Vertical" Height="160" Margin="0,16,0,0"
            Style="{DynamicResource CONCOST.Slider.Base}" />
</StackPanel>`,
                csharp: `// Ghi giá trị vào model khi người dùng thả tay kéo
OpacitySlider.AddHandler(Thumb.DragCompletedEvent,
    new DragCompletedEventHandler((s, e) => ApplyOpacity(OpacitySlider.Value)));

// Hoặc bind hai chiều trực tiếp
// <Slider Value="{Binding LayerOpacity, Mode=TwoWay}" />`
            }
        ],
        related: ["rangeslider", "progressbar", "numericupdown"]
    },

    scrollviewer: {

        title: "ScrollViewer — vùng cuộn & quán tính",
        badge: "Controls cơ bản",
        desc: "ScrollViewer với scrollbar tự ẩn và bộ attached <code>ScrollViewerHelper</code> (quán tính, tốc độ cuộn, nút back-to-top).",
        keywords: ["scroll", "cuộn", "scrollbar", "back to top"],
        variants: [
            { key: "CONCOST.ScrollViewer.Base", desc: "Scrollbar mảnh, tự ẩn khi không dùng." },
            { key: "CONCOST.ScrollViewer.Minimal", desc: "Chỉ còn một vạch mảnh, không có track." },
            { key: "CONCOST.ScrollViewer.AlwaysVisible", desc: "Luôn hiện scrollbar (dùng cho panel cố định)." }
        ],
        sections: [
            {
                title: "Panel cuộn có quán tính",
                desc: "<code>IsInertiaEnabled</code> tạo cảm giác cuộn mượt như touch; <code>ShowBackToTop</code> hiện nút quay về đầu khi cuộn quá ngưỡng.",
                xaml: `<ScrollViewer Style="{DynamicResource CONCOST.ScrollViewer.Base}"
              VerticalScrollBarVisibility="Auto"
              attached:ScrollViewerHelper.IsInertiaEnabled="True"
              attached:ScrollViewerHelper.ScrollSpeed="1.4"
              attached:ScrollViewerHelper.ShowBackToTop="True"
              attached:ScrollViewerHelper.BackToTopThreshold="240">
    <StackPanel>
        <!-- nội dung dài -->
    </StackPanel>
</ScrollViewer>`,
                csharp: `// Cuộn tới một phần tử sau khi load
Dispatcher.BeginInvoke(new Action(() =>
{
    TargetSection.BringIntoView();
}), DispatcherPriority.Loaded);

// Đọc vị trí cuộn hiện tại (attached, read-only-ish)
double offset = ScrollViewerHelper.GetCurrentVerticalOffset(ContentScroll);`
            }
        ],
        related: ["listbox", "datagrid", "card"]
    },

    listbox: {

        title: "ListBox — danh sách item",
        badge: "Controls cơ bản",
        desc: "ListBox restyle với item hover/selected theo theme, kèm biến thể Card cho danh sách dạng khối.",
        keywords: ["list", "danh sách", "listbox", "item"],
        variants: [
            { key: "CONCOST.ListBox.Base", desc: "Item phẳng, highlight accent khi chọn." },
            { key: "CONCOST.ListBox.Card", desc: "Mỗi item là một card có viền và khoảng cách." }
        ],
        sections: [
            {
                title: "Danh sách cấu kiện",
                desc: "Giữ <code>VirtualizingStackPanel</code> khi danh sách dài; chỉ đổi <code>ItemsPanel</code> khi cần layout đặc biệt.",
                xaml: `<ListBox ItemsSource="{Binding Beams}"
         SelectedItem="{Binding SelectedBeam, Mode=TwoWay}"
         Style="{DynamicResource CONCOST.ListBox.Card}"
         ScrollViewer.HorizontalScrollBarVisibility="Disabled">
    <ListBox.ItemTemplate>
        <DataTemplate>
            <StackPanel Margin="4">
                <TextBlock Text="{Binding Name}" Style="{DynamicResource CONCOST.TextBlock.Body}" />
                <TextBlock Text="{Binding Section}" Style="{DynamicResource CONCOST.TextBlock.Caption}" />
            </StackPanel>
        </DataTemplate>
    </ListBox.ItemTemplate>
</ListBox>`,
                csharp: `// Chọn item theo model (không cần thao tác chuột)
BeamList.SelectedItem = Beams.FirstOrDefault(b => b.Id == targetId);
BeamList.ScrollIntoView(BeamList.SelectedItem);`
            }
        ],
        related: ["listview", "datagrid", "scrollviewer"]
    },

    listview: {

        title: "ListView — danh sách nhiều cột",
        badge: "Controls cơ bản",
        desc: "ListView dùng cho danh sách nhiều cột nhẹ (không cần sort/filter như DataGrid).",
        keywords: ["listview", "gridview", "cột", "danh sách"],
        variants: [
            { key: "CONCOST.ListView.Base", desc: "Header cột và row theo theme, không có gridline." }
        ],
        sections: [
            {
                title: "Danh sách 3 cột",
                desc: "Dùng <code>GridView</code> với <code>DisplayMemberBinding</code>. Với dữ liệu cần sort/filter/sửa, chuyển sang <code>DataGrid</code>.",
                xaml: `<ListView ItemsSource="{Binding Materials}"
          Style="{DynamicResource CONCOST.ListView.Base}">
    <ListView.View>
        <GridView>
            <GridViewColumn Header="Grade"  DisplayMemberBinding="{Binding Grade}"  Width="120" />
            <GridViewColumn Header="fc' (MPa)" DisplayMemberBinding="{Binding Strength, StringFormat=N1}" Width="110" />
            <GridViewColumn Header="Density" DisplayMemberBinding="{Binding Density}" Width="110" />
        </GridView>
    </ListView.View>
</ListView>`,
                csharp: `// Sắp xếp theo cột khi bấm header (ListView không tự hỗ trợ)
GridViewColumnHeader.AddHandler(ButtonBase.ClickEvent, new RoutedEventHandler((s, e) =>
{
    if (e.OriginalSource is not GridViewColumnHeader header) return;
    var view = CollectionViewSource.GetDefaultView(Materials);
    view.SortDescriptions.Clear();
    view.SortDescriptions.Add(new SortDescription(header.Tag?.ToString(), ListSortDirection.Ascending));
}));`
            }
        ],
        related: ["listbox", "datagrid", "stackedheaderdatagrid"]
    },

    groupbox: {

        title: "GroupBox — nhóm trường nhập",
        badge: "Controls cơ bản",
        desc: "GroupBox với 7 biến thể, dùng để nhóm các trường nhập theo chủ đề kỹ thuật (hình học, vật liệu, tải trọng).",
        keywords: ["groupbox", "nhóm", "khung", "fieldset"],
        variants: [
            { key: "CONCOST.GroupBox.Base", desc: "Viền mảnh, header nằm trên đường viền." },
            { key: "CONCOST.GroupBox.Card", desc: "Nền surface + bo góc, header bên trong." },
            { key: "CONCOST.GroupBox.Primary / Success / Danger", desc: "Viền và header theo màu ngữ nghĩa." },
            { key: "CONCOST.GroupBox.HeaderFilled", desc: "Header có nền đặc." },
            { key: "CONCOST.GroupBox.Underline", desc: "Chỉ có đường kẻ dưới header." }
        ],
        sections: [
            {
                title: "Nhóm thuộc tính vật liệu",
                desc: "Đặt <code>Padding</code> cho nội dung; header nhận mọi <code>object</code> nên có thể chèn icon.",
                xaml: `<ui:GroupBox Header="Material — C30/37"
              Style="{DynamicResource CONCOST.GroupBox.Card}"
              Padding="16">
    <StackPanel>
        <ui:UnitBox Header="fc'" Value="30" Unit="MPa" DecimalPlaces="0" Margin="0,0,0,10" />
        <ui:UnitBox Header="γ"   Value="25" Unit="kN/m³" DecimalPlaces="1" Margin="0,0,0,10" />
        <ui:UnitBox Header="E"   Value="33000" Unit="MPa" DecimalPlaces="0" />
    </StackPanel>
</ui:GroupBox>`,
                csharp: `// Đổi màu nhóm theo trạng thái kiểm tra
MaterialGroup.Style = (Style)FindResource(
    isValid ? "CONCOST.GroupBox.Success" : "CONCOST.GroupBox.Danger");`
            }
        ],
        related: ["card", "expander", "divider"]
    },

    menu: {

        title: "Menu & ContextMenu — menu ngữ cảnh",
        badge: "Controls cơ bản",
        desc: "Menu ngang và ContextMenu được restyle với 4 biến thể, hỗ trợ submenu nhiều cấp và icon.",
        keywords: ["menu", "contextmenu", "submenu", "chuột phải"],
        variants: [
            { key: "CONCOST.Menu.Base", desc: "Menu phẳng, hover accent nhạt." },
            { key: "CONCOST.Menu.Flat", desc: "Không viền, dùng trong toolbar." },
            { key: "CONCOST.Menu.Card", desc: "Menu nổi có shadow và bo góc." },
            { key: "CONCOST.Menu.ModernPill", desc: "Item dạng pill, hợp với app hiện đại." }
        ],
        sections: [
            {
                title: "Menu ngang + submenu",
                desc: "<code>MenuItem</code> lồng nhau tạo submenu. Đặt <code>InputGestureText</code> để hiện phím tắt bên phải.",
                xaml: `<Menu Style="{DynamicResource CONCOST.Menu.ModernPill}">
    <MenuItem Header="File">
        <MenuItem Header="New model"   InputGestureText="Ctrl+N" />
        <MenuItem Header="Open..."     InputGestureText="Ctrl+O" />
        <Separator />
        <MenuItem Header="Export">
            <MenuItem Header="DXF" />
            <MenuItem Header="IFC 4.0" />
            <MenuItem Header="PDF report" />
        </MenuItem>
    </MenuItem>
    <MenuItem Header="Analysis">
        <MenuItem Header="Run solver" InputGestureText="F5" Click="OnRunSolver" />
    </MenuItem>
</Menu>`,
                csharp: `// ContextMenu dựng bằng code cho DataGrid row
var menu = new ContextMenu { Style = (Style)FindResource("CONCOST.Menu.Card") };
menu.Items.Add(new MenuItem { Header = "Copy section", Command = CopyCommand });
menu.Items.Add(new MenuItem { Header = "Assign material", Command = AssignCommand });
menu.Items.Add(new Separator());
menu.Items.Add(new MenuItem { Header = "Delete", Command = DeleteCommand });

BeamGrid.ContextMenu = menu;`
            }
        ],
        related: ["splitbutton", "drawermenu", "navigationview"]
    },

    textblock: {

        title: "Typography — thang chữ & màu ngữ nghĩa",
        badge: "Controls cơ bản",
        desc: "Thang typography của design system: H1–H4, Body, Caption, Code và các màu ngữ nghĩa — dùng thay vì set FontSize thủ công.",
        keywords: ["typography", "font", "chữ", "heading", "text"],
        variants: [
            { key: "CONCOST.TextBlock.H1 / H2 / H3 / H4", desc: "Thang tiêu đề 28 / 22 / 18 / 16px, weight giảm dần." },
            { key: "CONCOST.TextBlock.Body", desc: "Văn bản chính 13.5px, line-height 1.6." },
            { key: "CONCOST.TextBlock.Caption", desc: "Nhãn phụ 11.5px, màu muted." },
            { key: "CONCOST.TextBlock.Code", desc: "Font mono, nền sunken — dùng cho ký hiệu, mã cấu kiện." },
            { key: "CONCOST.TextBlock.Primary / Success / Warning / Danger", desc: "Màu theo ngữ nghĩa trạng thái." }
        ],
        sections: [
            {
                title: "Phân cấp nội dung trong panel",
                desc: "Dùng đúng style thay vì <code>FontSize</code>/<code>FontWeight</code> để toàn bộ ứng dụng đổi theme và scale nhất quán.",
                xaml: `<StackPanel Margin="24">
    <TextBlock Text="Kết quả phân tích" Style="{DynamicResource CONCOST.TextBlock.H3}" />
    <TextBlock Text="Tổ hợp tải trọng bao phủ toàn bộ 12 trường hợp."
               Style="{DynamicResource CONCOST.TextBlock.Body}" Margin="0,4,0,12" />
    <TextBlock Text="MOMENT DIAGRAM · kN.m" Style="{DynamicResource CONCOST.TextBlock.Caption}" />
    <TextBlock Text="Mmax = 245.8 kN.m" Style="{DynamicResource CONCOST.TextBlock.Code}" Margin="0,6,0,0" />
    <TextBlock Text="Kiểm tra độ võng: ĐẠT" Style="{DynamicResource CONCOST.TextBlock.Success}" Margin="0,6,0,0" />
</StackPanel>`,
                csharp: `// Đổi style theo trạng thái kiểm tra
ResultText.Style = (Style)FindResource(
    isPassing ? "CONCOST.TextBlock.Success" : "CONCOST.TextBlock.Danger");

// Tạo nhanh một heading trong code
var heading = new TextBlock
{
    Text = "Structural summary",
    Style = (Style)FindResource("CONCOST.TextBlock.H3")
};`
            }
        ],
        related: ["card", "divider", "badge"]
    },

    /* ============================ ENRICHMENT (props only) ============================ */
    button: {

        className: "Button",
        props: [
            { name: "ButtonElement.IsLoading", type: "bool", default: "false", attached: true, desc: "Hiện spinner trong nút và chặn click lặp." },
            { name: "ButtonElement.LoadingText", type: "string", default: "\"Loading...\"", attached: true, desc: "Text thay thế <code>Content</code> khi đang loading." },
            { name: "ButtonElement.Badge", type: "object", default: "null", attached: true, desc: "Badge số lượng/thông báo ở góc trên-phải." },
            { name: "IconElement.Kind", type: "IconKind", default: "IconKind.None", attached: true, desc: "Vector icon hiển thị trong nút." },
            { name: "IconElement.Placement", type: "Dock", default: "Dock.Left", attached: true, desc: "Icon nằm trước hay sau nội dung." },
            { name: "IconElement.IconSize", type: "double", default: "16.0", attached: true, desc: "Kích thước icon (px)." },
            { name: "BorderElement.Circular", type: "bool", default: "false", attached: true, desc: "Bo tròn hoàn toàn (nút icon tròn)." },
            { name: "VisualElement.Elevation", type: "int", default: "0", attached: true, desc: "Độ nổi (0–4) tạo shadow theo token." }
        ],
        variants: [
            { key: "CONCOST.Button.Primary / Secondary", desc: "Hành động chính / phụ." },
            { key: "CONCOST.Button.Success / Warning / Danger / Info", desc: "Nền đặc theo ngữ nghĩa." },
            { key: "CONCOST.Button.*.Light", desc: "Nền nhạt 12%, chữ màu đậm — dùng trong toolbar sáng." },
            { key: "CONCOST.Button.Outline / Outline.Primary / Outline.Danger / Outline.Success / Outline.Warning / Outline.Info", desc: "Chỉ viền." },
            { key: "CONCOST.Button.Icon / Icon.Primary / Icon.Circle / Icon.Primary.Circle / Icon.Ghost", desc: "Nút chỉ có icon, các mức nền khác nhau." },
            { key: "CONCOST.Button.Base", desc: "Style nền tảng — kế thừa khi cần tự tạo variant." }
        ]
    },

    togglebutton: {

        className: "ToggleButton",
        props: [
            { name: "IconElement.Kind", type: "IconKind", default: "IconKind.None", attached: true, desc: "Icon hiển thị; với style <code>Icon</code> chỉ icon là nội dung." },
            { name: "IconElement.IconSize", type: "double", default: "16.0", attached: true, desc: "Kích thước icon." },
            { name: "BorderElement.CornerRadius", type: "CornerRadius", default: "CornerRadius(4)", attached: true, desc: "Bo góc khi ở trạng thái chọn." },
            { name: "ButtonElement.Badge", type: "object", default: "null", attached: true, desc: "Badge gắn góc nút." }
        ],
        variants: [
            { key: "CONCOST.ToggleButton.Base / Success / Outline", desc: "Nền đặc / ngữ nghĩa / chỉ viền." },
            { key: "CONCOST.ToggleButton.Icon / Icon.Circle", desc: "Chỉ icon, dạng vuông hoặc tròn." },
            { key: "CONCOST.ToggleButton.Icon.Ghost / Icon.Ghost.Circle", desc: "Icon không nền, chỉ đổi màu khi chọn." },
            { key: "CONCOST.ToggleButton.Icon.Outline / Icon.Outline.Circle", desc: "Icon có viền." },
            { key: "CONCOST.ToggleButton.Icon.Success", desc: "Icon chuyển màu success khi bật." }
        ]
    },

    textbox: {

        className: "TextBox",
        props: [
            { name: "InfoElement.Placeholder", type: "string", default: "string.Empty", attached: true, desc: "Chữ gợi ý khi ô trống." },
            { name: "InfoElement.Title", type: "string", default: "null", attached: true, desc: "Nhãn phía trên ô nhập." },
            { name: "InfoElement.TitlePlacement", type: "Dock", default: "Dock.Top", attached: true, desc: "Vị trí nhãn (Top/Left)." },
            { name: "InfoElement.Required", type: "bool", default: "false", attached: true, desc: "Đánh dấu bắt buộc (dấu * đỏ)." },
            { name: "InfoElement.Prefix / Suffix", type: "object", default: "null", attached: true, desc: "Nội dung chèn trước/sau ô nhập (đơn vị, icon)." },
            { name: "InfoElement.ShowClearButton", type: "bool", default: "false", attached: true, desc: "Hiện nút xoá nhanh [X]." },
            { name: "InfoElement.ShowCharacterCount", type: "bool", default: "false", attached: true, desc: "Hiện bộ đếm ký tự." },
            { name: "TextBoxHelper.InputType", type: "TextBoxInputType", default: "TextBoxInputType.None", attached: true, desc: "Lọc nhập: <code>DigitsOnly</code>, <code>Decimal</code>, <code>PositiveInteger</code>, <code>Alphanumeric</code>, <code>Formula</code>." },
            { name: "TextBoxHelper.SelectAllOnFocus", type: "bool", default: "false", attached: true, desc: "Chọn toàn bộ text khi focus." },
            { name: "TextBoxHelper.ClearOnEscape", type: "bool", default: "false", attached: true, desc: "Xoá nội dung khi nhấn Esc." },
            { name: "TextBoxHelper.CommitOnEnter", type: "bool", default: "false", attached: true, desc: "Cập nhật binding ngay khi nhấn Enter." },
            { name: "PasswordBoxHelper.Attach", type: "bool", default: "false", attached: true, desc: "Bật hỗ trợ binding cho <code>PasswordBox</code>." },
            { name: "PasswordBoxHelper.Password", type: "string", default: "string.Empty", attached: true, desc: "Cho phép bind mật khẩu (kèm <code>Attach=\"True\"</code>)." }
        ],
        variants: [
            { key: "CONCOST.TextBox.Base / Default", desc: "Ô nhập chuẩn có viền." },
            { key: "CONCOST.TextBox.Filled", desc: "Nền sunken, không viền." },
            { key: "CONCOST.TextBox.Ghost", desc: "Chỉ đường kẻ dưới, dùng trong toolbar." },
            { key: "CONCOST.TextBox.Underline", desc: "Viền dưới dày 2px khi focus." },
            { key: "CONCOST.TextBox.Round", desc: "Bo tròn hoàn toàn." },
            { key: "CONCOST.TextBox.Small / Large", desc: "Chiều cao 28px / 40px." },
            { key: "CONCOST.PasswordBox.Base / Filled / Round / Small / Large", desc: "Bộ variant tương ứng cho PasswordBox." }
        ]
    },

    combobox: {

        className: "ComboBox",
        props: [
            { name: "InfoElement.Placeholder", type: "string", default: "string.Empty", attached: true, desc: "Chữ gợi ý khi chưa chọn." },
            { name: "InfoElement.Title", type: "string", default: "null", attached: true, desc: "Nhãn phía trên." },
            { name: "InfoElement.Required", type: "bool", default: "false", attached: true, desc: "Đánh dấu trường bắt buộc." },
            { name: "IconElement.Kind", type: "IconKind", default: "IconKind.None", attached: true, desc: "Icon mũi tên hoặc icon ngữ cảnh." },
            { name: "ComboBoxHelper.IsSearchEnabled", type: "bool", default: "false", attached: true, desc: "Hiện ô tìm kiếm trên đầu danh sách; ô chọn vẫn <b>không sửa được giá trị</b>." },
            { name: "ComboBoxHelper.SearchText", type: "string", default: "string.Empty", attached: true, desc: "Từ khoá đang lọc (hai chiều)." },
            { name: "ComboBoxHelper.SearchPlaceholder", type: "string", default: "\"Search…\"", attached: true, desc: "Chữ gợi ý trong ô tìm kiếm." }
        ],
        variants: [
            { key: "CONCOST.ComboBox.Base", desc: "Chuẩn, có viền và mũi tên." },
            { key: "CONCOST.ComboBox.Filled", desc: "Nền sunken." },
            { key: "CONCOST.ComboBox.Round", desc: "Bo tròn hoàn toàn." },
            { key: "CONCOST.ComboBox.Small / Large", desc: "Chiều cao 28px / 40px." }
        ]
    },

    datagrid: {

        className: "DataGrid",
        props: [
            { name: "DataGridElement.RowSpacing", type: "double", default: "4.0", attached: true, desc: "Khoảng cách giữa các dòng." },
            { name: "DataGridElement.ShowRowDivider", type: "bool", default: "false", attached: true, desc: "Hiện đường phân cách dòng." },
            { name: "DataGridElement.HeaderHorizontalContentAlignment", type: "HorizontalAlignment", default: "HorizontalAlignment.Left", attached: true, desc: "Căn nội dung header." },
            { name: "DataGridElement.CellHorizontalContentAlignment", type: "HorizontalAlignment", default: "HorizontalAlignment.Left", attached: true, desc: "Căn nội dung ô." },
            { name: "ConditionalFormattingRule", type: "collection", default: "—", desc: "Bộ luật tô màu theo giá trị (Operator + ngưỡng + brush)." }
        ],
        variants: [
            { key: "CONCOST.DataGrid.Base", desc: "Chuẩn: header đậm, zebra row, gridline mảnh." },
            { key: "CONCOST.DataGrid.Compact", desc: "Row 26px cho bảng dày đặc." },
            { key: "CONCOST.DataGrid.Seamless", desc: "Không gridline, không viền ngoài." },
            { key: "CONCOST.DataGrid.Small", desc: "Font và padding nhỏ hơn." },
            { key: "CONCOST.DataGrid.CheckBox / EditingTextBox / ColumnHeaderGripper", desc: "Style cho ô checkbox, ô đang sửa và tay kéo giãn cột." }
        ]
    },

    treeview: {

        className: "TreeView",
        props: [
            { name: "IconElement.Kind", type: "IconKind", default: "IconKind.None", attached: true, desc: "Icon cho <code>TreeViewItem</code>." },
            { name: "VisualElement.HoverScale", type: "double", default: "1.0", attached: true, desc: "Tỉ lệ phóng khi hover item." }
        ],
        variants: [
            { key: "CONCOST.TreeView.Base", desc: "Có đường nối, mũi tên expander." },
            { key: "CONCOST.TreeView.Card", desc: "Item dạng card." },
            { key: "CONCOST.TreeView.Directory", desc: "Kiểu cây thư mục (icon folder/file)." },
            { key: "CONCOST.TreeView.Line", desc: "Chỉ đường nối dọc, không nền." },
            { key: "CONCOST.TreeView.ModernPill", desc: "Item pill, hover accent nhạt." }
        ]
    },

    tabcontrol: {

        className: "TabControl",
        props: [
            { name: "TabControlHelper.IsClosable", type: "bool", default: "false", attached: true, desc: "Hiện nút đóng trên từng <code>TabItem</code>." },
            { name: "TabControlHelper.HeaderExtraContent", type: "object", default: "null", attached: true, desc: "Nội dung chèn cuối dải tab (nút +, filter…)." }
        ],
        variants: [
            { key: "CONCOST.TabControl.Base / Card / Chrome", desc: "Kiểu tab cổ điển, card, hoặc chrome trình duyệt." },
            { key: "CONCOST.TabControl.Pills / Segmented / Capsule", desc: "Tab dạng pill, segmented, hoặc viên thuốc." },
            { key: "CONCOST.TabControl.InLine / Sliding", desc: "Tab phẳng có indicator trượt." },
            { key: "CONCOST.TabControl.Bottom / Vertical / VerticalRight", desc: "Tab ở dưới / dọc trái / dọc phải." },
            { key: "CONCOST.TabItem.Base", desc: "Style cho từng TabItem (kế thừa bởi mọi variant)." }
        ]
    },

    card: {

        className: "Card",
        props: [
            { name: "Icon", type: "IconKind", default: "IconKind.None", desc: "Icon ở header card." },
            { name: "CornerRadius", type: "CornerRadius", default: "CornerRadius(10)", desc: "Bo góc card (header/footer và vạch accent tự tính theo)." },
            { name: "Footer", type: "object", default: "null", desc: "Nội dung chân card (chỉ hiện khi có giá trị)." },
            { name: "FooterTemplate", type: "DataTemplate", default: "null", desc: "Template cho footer." },
            { name: "Elevation", type: "int", default: "1", desc: "Độ nổi 0–4, shadow dựng từ token theme." },
            { name: "IsInteractive", type: "bool", default: "false", desc: "Tự nâng nổi + phóng nhẹ + con trỏ bàn tay khi hover." },
            { name: "AccentBar", type: "CardAccent", default: "CardAccent.None", desc: "Cạnh mang vạch accent: <code>Top</code>, <code>Left</code>, <code>Right</code>, <code>Bottom</code>." },
            { name: "AccentBrush", type: "Brush", default: "null", desc: "Màu vạch accent (để trống = màu nhấn của theme)." },
            { name: "AccentThickness", type: "double", default: "3.0", desc: "Độ dày vạch accent (px)." },
            { name: "HeaderBackground", type: "Brush", default: "null", desc: "Nền vùng header (để trống = trong suốt)." },
            { name: "FooterBackground", type: "Brush", default: "CONCOST.Brush.SurfaceSecondary", desc: "Nền vùng footer." },
            { name: "AccentCornerRadius", type: "CornerRadius", default: "—", desc: "Bo góc vạch accent (read-only, tự tính)." },
            { name: "HeaderCornerRadius", type: "CornerRadius", default: "—", desc: "Bo góc vùng header (read-only)." },
            { name: "FooterCornerRadius", type: "CornerRadius", default: "—", desc: "Bo góc vùng footer (read-only)." },
            { name: "BorderElement.HoverBorderBrush", type: "Brush", default: "null", attached: true, desc: "Màu viền khi hover." },
            { name: "BorderElement.HoverBackground", type: "Brush", default: "null", attached: true, desc: "Màu nền khi hover." },
            { name: "VisualElement.HoverElevation", type: "int", default: "0", attached: true, desc: "Độ nổi khi hover (Card.IsInteractive tự đặt Elevation + 1)." }
        ],
        sections: [
            {
                title: "Vạch accent & card tương tác",
                desc: "Chọn cạnh mang vạch accent bằng một thuộc tính và bật <code>IsInteractive</code> để thẻ tự nổi lên khi rê chuột — toàn bộ màu sắc đi theo theme.",
                xaml: `<StackPanel Orientation="Horizontal">
    <!-- Vạch accent cạnh trái, tự lấy màu nhấn của theme -->
    <ui:Card Header="Cột thép D600"
             AccentBar="Left"
             IsInteractive="True"
             Width="240">
        <TextBlock Text="Tổ hợp bao phủ 12 trường hợp tải trọng." TextWrapping="Wrap" />
    </ui:Card>

    <!-- Vạch accent trên, màu success tuỳ biến -->
    <ui:Card Header="Kiểm tra võng"
             AccentBar="Top"
             AccentBrush="{DynamicResource CONCOST.Brush.Success}"
             AccentThickness="4"
             CornerRadius="14"
             Margin="12,0,0,0"
             Width="240">
        <TextBlock Text="Đạt giới hạn L/250." />
    </ui:Card>
</StackPanel>`
            }
        ],
        variants: [
            { key: "CONCOST.Border.Base", desc: "Viền mảnh + nền surface." },
            { key: "CONCOST.Border.Elevation", desc: "Có shadow theo token elevation." },
            { key: "CONCOST.Border.AccentLeft", desc: "Vạch accent 3px bên trái." },
            { key: "CONCOST.Border.Pill", desc: "Bo tròn hoàn toàn." }
        ]
    },

    progressbar: {

        className: "ProgressBar",
        props: [
            { name: "ProgressBarElement.ShowText", type: "bool", default: "false", attached: true, desc: "Hiện phần trăm trên thanh." },
            { name: "ProgressBarElement.TextPattern", type: "string", default: "\"{0:0}%\"", attached: true, desc: "Định dạng text hiển thị (vd <code>{0:0}% hoàn thành</code>)." },
            { name: "VisualElement.Elevation", type: "int", default: "0", attached: true, desc: "Độ nổi của thanh." }
        ],
        variants: [
            { key: "CONCOST.ProgressBar.Base", desc: "Thanh mảnh 6px, màu accent." },
            { key: "CONCOST.ProgressBar.Thick / Thin", desc: "12px / 3px." },
            { key: "CONCOST.ProgressBar.Success / Warning / Danger", desc: "Màu theo ngữ nghĩa." }
        ]
    },

    toggleswitch: {

        className: "ToggleSwitch",
        props: [
            { name: "OnContent", type: "object", default: "null", desc: "Nội dung hiển thị khi bật." },
            { name: "OffContent", type: "object", default: "null", desc: "Nội dung hiển thị khi tắt." }
        ]
    }
};
