// Property tables + C# snippets for the remaining pages.
// Kept separate so the page content files stay readable.
const DOCS_ENRICH = {

    /* ---------------- props for the new basic pages ---------------- */
    checkbox: {

        className: "CheckBox",
        props: [
            { name: "IsChecked", type: "bool?", default: "false", desc: "Trạng thái chọn; <code>null</code> khi bật ba trị và chưa xác định." },
            { name: "IsThreeState", type: "bool", default: "false", desc: "Cho phép chu kỳ true → false → null." },
            { name: "Content", type: "object", default: "null", desc: "Nhãn hiển thị cạnh ô chọn." },
            { name: "InfoElement.Title", type: "string", default: "null", attached: true, desc: "Nhãn phụ phía trên." },
            { name: "InfoElement.Required", type: "bool", default: "false", attached: true, desc: "Đánh dấu bắt buộc." },
            { name: "IconElement.Kind", type: "IconKind", default: "IconKind.None", attached: true, desc: "Icon hiển thị trước nhãn." }
        ]
    },

    radiobutton: {

        className: "RadioButton",
        props: [
            { name: "IsChecked", type: "bool?", default: "false", desc: "Nút có đang được chọn." },
            { name: "GroupName", type: "string", default: "string.Empty", desc: "Tên nhóm — cùng tên thì loại trừ lẫn nhau." },
            { name: "Content", type: "object", default: "null", desc: "Nhãn của nút." },
            { name: "IconElement.Kind", type: "IconKind", default: "IconKind.None", attached: true, desc: "Icon trước nhãn (hợp với style <code>Segment</code>)." }
        ]
    },

    slider: {

        className: "Slider",
        props: [
            { name: "Minimum", type: "double", default: "0", desc: "Giá trị nhỏ nhất." },
            { name: "Maximum", type: "double", default: "10", desc: "Giá trị lớn nhất." },
            { name: "Value", type: "double", default: "0", desc: "Giá trị hiện tại (bind two-way)." },
            { name: "TickFrequency", type: "double", default: "1", desc: "Khoảng giữa các mốc tick." },
            { name: "IsSnapToTickEnabled", type: "bool", default: "false", desc: "Ép giá trị bám theo mốc tick." },
            { name: "Orientation", type: "Orientation", default: "Horizontal", desc: "Hướng thanh trượt." },
            { name: "IsDirectionReversed", type: "bool", default: "false", desc: "Đảo chiều tăng giá trị." },
            { name: "IsMoveToPointEnabled", type: "bool", default: "false", desc: "Click vào track để nhảy thumb tới đó." }
        ]
    },

    scrollviewer: {

        className: "ScrollViewer",
        props: [
            { name: "ScrollViewerHelper.IsInertiaEnabled", type: "bool", default: "false", attached: true, desc: "Cuộn có quán tính (mượt như touch)." },
            { name: "ScrollViewerHelper.ScrollSpeed", type: "double", default: "1.0", attached: true, desc: "Hệ số tốc độ cuộn chuột." },
            { name: "ScrollViewerHelper.BubbleAtBoundary", type: "bool", default: "true", attached: true, desc: "Cho phép cuộn lan ra control cha khi hết vùng." },
            { name: "ScrollViewerHelper.IsAutoHideEnabled", type: "bool", default: "true", attached: true, desc: "Tự ẩn scrollbar khi không dùng." },
            { name: "ScrollViewerHelper.ShowBackToTop", type: "bool", default: "false", attached: true, desc: "Hiện nút quay về đầu." },
            { name: "ScrollViewerHelper.BackToTopThreshold", type: "double", default: "100.0", attached: true, desc: "Ngưỡng cuộn (px) để hiện nút back-to-top." },
            { name: "ScrollViewerHelper.CurrentVerticalOffset", type: "double", default: "0.0", attached: true, desc: "Vị trí cuộn dọc hiện tại." },
            { name: "ScrollViewerHelper.CurrentHorizontalOffset", type: "double", default: "0.0", attached: true, desc: "Vị trí cuộn ngang hiện tại." },
            { name: "VerticalScrollBarVisibility", type: "ScrollBarVisibility", default: "Visible", desc: "Chế độ hiện scrollbar dọc." },
            { name: "HorizontalScrollBarVisibility", type: "ScrollBarVisibility", default: "Disabled", desc: "Chế độ hiện scrollbar ngang." }
        ]
    },

    listbox: {

        className: "ListBox",
        props: [
            { name: "ItemsSource", type: "IEnumerable", default: "null", desc: "Nguồn dữ liệu." },
            { name: "SelectedItem", type: "object", default: "null", desc: "Item đang chọn (bind two-way)." },
            { name: "SelectedIndex", type: "int", default: "-1", desc: "Chỉ số item đang chọn." },
            { name: "SelectionMode", type: "SelectionMode", default: "Single", desc: "<code>Single</code>, <code>Multiple</code>, <code>Extended</code>." },
            { name: "ItemTemplate", type: "DataTemplate", default: "null", desc: "Template hiển thị item." }
        ]
    },

    listview: {

        className: "ListView",
        props: [
            { name: "ItemsSource", type: "IEnumerable", default: "null", desc: "Nguồn dữ liệu." },
            { name: "SelectedItem", type: "object", default: "null", desc: "Item đang chọn." },
            { name: "View", type: "ViewBase", default: "null", desc: "<code>GridView</code> khai báo các cột." },
            { name: "SelectionMode", type: "SelectionMode", default: "Single", desc: "Chế độ chọn." }
        ]
    },

    groupbox: {

        className: "GroupBox",
        props: [
            { name: "Header", type: "object", default: "null", desc: "Tiêu đề nhóm." },
            { name: "Padding", type: "Thickness", default: "0", desc: "Khoảng đệm nội dung." },
            { name: "BorderThickness", type: "Thickness", default: "1", desc: "Độ dày viền." },
            { name: "IconElement.Kind", type: "IconKind", default: "IconKind.None", attached: true, desc: "Icon trong header." }
        ]
    },

    menu: {

        className: "Menu",
        props: [
            { name: "Header", type: "object", default: "null", desc: "Nhãn của <code>MenuItem</code>." },
            { name: "InputGestureText", type: "string", default: "string.Empty", desc: "Chuỗi phím tắt hiển thị bên phải (không tự đăng ký phím)." },
            { name: "IsCheckable", type: "bool", default: "false", desc: "MenuItem có dấu tick." },
            { name: "IsChecked", type: "bool", default: "false", desc: "Trạng thái tick của MenuItem." },
            { name: "IconElement.Kind", type: "IconKind", default: "IconKind.None", attached: true, desc: "Icon của MenuItem." },
            { name: "IconElement.Placement", type: "Dock", default: "Dock.Left", attached: true, desc: "Vị trí icon trong item." }
        ]
    },

    textblock: {

        className: "TextBlock",
        props: [
            { name: "Text", type: "string", default: "string.Empty", desc: "Nội dung văn bản." },
            { name: "TextWrapping", type: "TextWrapping", default: "NoWrap", desc: "<code>Wrap</code> khi văn bản dài trong panel hẹp." },
            { name: "TextTrimming", type: "TextTrimming", default: "None", desc: "<code>CharacterEllipsis</code> để cắt bằng dấu ba chấm." },
            { name: "LineHeight", type: "double", default: "NaN", desc: "Chiều cao dòng (mặc định theo style)." },
            { name: "MaxLines", type: "int", default: "0", desc: "Giới hạn số dòng (0 = không giới hạn)." }
        ]
    },

    /* ---------------- C# snippets for legacy basic pages ---------------- */
    button: {

        csharp: `// Nút có icon + trạng thái loading
<ui:Button x:Name="SaveButton" Content="Save model" Style="{DynamicResource CONCOST.Button.Primary}" />

SaveButton.SetValue(IconElement.KindProperty, IconKind.Save);
ButtonElement.SetIsLoading(SaveButton, true);
ButtonElement.SetLoadingText(SaveButton, "Saving...");

// Nút icon tròn không nền
CloseButton.SetValue(IconElement.KindProperty, IconKind.Close);
CloseButton.SetValue(BorderElement.CircularProperty, true);
CloseButton.SetValue(VisualElement.ElevationProperty, 1);`
    },

    togglebutton: {

        csharp: `// Nút bật/tắt lưới trục
<ui:ToggleButton x:Name="GridToggle" Style="{DynamicResource CONCOST.ToggleButton.Icon}"
                 attached:IconElement.Kind="GridSquare" />

GridToggle.Checked += (s, e) => Viewport.ShowGrid = true;
GridToggle.Unchecked += (s, e) => Viewport.ShowGrid = false;

// Đồng bộ ngược từ model
GridToggle.IsChecked = Viewport.ShowGrid;`
    },

    textbox: {

        csharp: `// Ô nhập số có đơn vị và kiểm tra bắt buộc
<ui:TextBox x:Name="BeamName"
            Style="{DynamicResource CONCOST.TextBox.Base}"
            attached:InfoElement.Title="Beam name"
            attached:InfoElement.Placeholder="B1, B2, G3..."
            attached:InfoElement.Required="True"
            attached:TextBoxHelper.SelectAllOnFocus="True" />

// Lọc ký tự nhập: chỉ cho phép số thực dương
TextBoxHelper.SetInputType(QuantityBox, TextBoxInputType.PositiveDecimal);

// Kiểm tra giá trị trước khi lưu
if (string.IsNullOrWhiteSpace(BeamName.Text))
{
    Growl.Toast("Beam name is required", GrowlType.Warning);
    return;
}`
    },

    combobox: {

        csharp: `// ComboBox chọn cấp bê tông
<ComboBox x:Name="GradeCombo"
          ItemsSource="{Binding ConcreteGrades}"
          DisplayMemberPath="Name"
          SelectedValuePath="Id"
          Style="{DynamicResource CONCOST.ComboBox.Base}"
          attached:InfoElement.Title="Concrete grade"
          attached:InfoElement.Placeholder="Select grade..." />

// Chọn theo giá trị model
GradeCombo.SelectedValue = model.ConcreteGradeId;

// Đọc lựa chọn
if (GradeCombo.SelectedItem is ConcreteGrade grade)
    Debug.WriteLine($"{grade.Name} · fc' = {grade.Strength} MPa");`
    },

    datagrid: {

        csharp: `// Bảng bóc tách khối lượng
<DataGrid x:Name="BoqGrid"
          ItemsSource="{Binding BoqRows}"
          AutoGenerateColumns="False"
          Style="{DynamicResource CONCOST.DataGrid.Base}"
          attached:DataGridElement.RowSpacing="2"
          attached:DataGridElement.ShowRowDivider="True" />

// Cấu hình cột trong code
BoqGrid.Columns.Add(new DataGridTextColumn
{
    Header = "Element",
    Binding = new Binding(nameof(BoqRow.Element)),
    Width = new DataGridLength(1, DataGridLengthUnitType.Star)
});

// Tô màu theo giá trị (conditional formatting)
BoqGrid.SetValue(DataGridElement.ShowRowDividerProperty, true);`
    },

    treeview: {

        csharp: `// Cây phân cấp dự án
<TreeView x:Name="ModelTree"
          ItemsSource="{Binding ModelNodes}"
          Style="{DynamicResource CONCOST.TreeView.Line}">
    <TreeView.ItemTemplate>
        <HierarchicalDataTemplate ItemsSource="{Binding Children}">
            <TextBlock Text="{Binding Name}" Style="{DynamicResource CONCOST.TextBlock.Body}" />
        </HierarchicalDataTemplate>
    </TreeView.ItemTemplate>
</TreeView>

// Mở rộng toàn bộ cây
foreach (var node in ModelTree.Items.OfType<TreeViewItem>())
    node.IsExpanded = true;`
    },

    tabcontrol: {

        csharp: `// Tab có nút đóng và nút "+" cuối dải
<TabControl x:Name="DocumentTabs"
            Style="{DynamicResource CONCOST.TabControl.InLine}"
            attached:TabControlHelper.IsClosable="True">
    <TabItem Header="Level 1" />
    <TabItem Header="Level 2" />
</TabControl>

// Nút thêm tab ở cuối dải header
var addButton = new Button { Content = "+", Width = 26 };
TabControlHelper.SetHeaderExtraContent(DocumentTabs, addButton);

// Đóng tab đang chọn
if (DocumentTabs.SelectedItem is TabItem tab)
    DocumentTabs.Items.Remove(tab);`
    },

    progressbar: {

        csharp: `// Thanh tiến trình có phần trăm
<ProgressBar x:Name="SolveProgress"
             Minimum="0" Maximum="100" Value="{Binding Percent}"
             Style="{DynamicResource CONCOST.ProgressBar.Thick}"
             attached:ProgressBarElement.ShowText="True"
             attached:ProgressBarElement.TextPattern="{}{0:0}% hoàn thành" />

// Chuyển sang màu cảnh báo khi gần đầy
SolveProgress.Style = (Style)FindResource(
    Percent > 90 ? "CONCOST.ProgressBar.Warning" : "CONCOST.ProgressBar.Success");`
    }
};
