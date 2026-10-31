using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using CONCOST.UI.Controls;
using CONCOST.UI.Core.MVVM;
using CONCOST.UI.Icons;
using CONCOST.UI.Services;

namespace CONCOST.UI.Showcase;

public partial class ShowcaseViewModel
{
    // =========================================================================
    // DEMO: data controls — tree grid, multi-select grid, treemap
    // =========================================================================

    /// <summary>One node of the sample building breakdown.</summary>
    public class BreakdownNode
    {
        public string Name { get; set; } = string.Empty;
        public double Quantity { get; set; }
        public int Count { get; set; }
        public List<BreakdownNode> Children { get; set; } = new();
    }

    /// <summary>One beam row for the multi-select grid.</summary>
    public class BeamRow
    {
        public string Code { get; set; } = string.Empty;
        public string Section { get; set; } = string.Empty;
        public double Length { get; set; }
        public double Weight { get; set; }
    }

    private static List<BreakdownNode> SampleBreakdown() => new()
    {
        new BreakdownNode
        {
            Name = "Tầng 1", Count = 48, Quantity = 412.6,
            Children =
            {
                new BreakdownNode
                {
                    Name = "Cột", Count = 16, Quantity = 96.4,
                    Children =
                    {
                        new BreakdownNode { Name = "C1 (400×400)", Count = 8, Quantity = 48.2 },
                        new BreakdownNode { Name = "C2 (300×300)", Count = 8, Quantity = 48.2 }
                    }
                },
                new BreakdownNode
                {
                    Name = "Dầm", Count = 24, Quantity = 214.8,
                    Children =
                    {
                        new BreakdownNode { Name = "D1 (300×600)", Count = 14, Quantity = 142.6 },
                        new BreakdownNode { Name = "D2 (250×500)", Count = 10, Quantity = 72.2 }
                    }
                },
                new BreakdownNode { Name = "Sàn", Count = 8, Quantity = 101.4 }
            }
        },
        new BreakdownNode
        {
            Name = "Tầng 2", Count = 36, Quantity = 298.2,
            Children =
            {
                new BreakdownNode { Name = "Cột", Count = 12, Quantity = 62.1 },
                new BreakdownNode { Name = "Dầm", Count = 18, Quantity = 168.5 },
                new BreakdownNode { Name = "Sàn", Count = 6, Quantity = 67.6 }
            }
        }
    };

    private object CreateTreeDataGridDemo()
    {
        var grid = new TreeDataGrid
        {
            RootItems = SampleBreakdown(),
            ChildrenPath = "Children",
            ShowHierarchyColumn = true,
            HierarchyHeader = "Cấu kiện",
            HierarchyPath = nameof(BreakdownNode.Name),
            HierarchyColumnWidth = 280,
            IndentSize = 18,
            Height = 320,
            AutoGenerateColumns = false
        };

        grid.Columns.Add(new DataGridTextColumn
        {
            Header = "Số lượng",
            Binding = new Binding(nameof(BreakdownNode.Count)),
            Width = 100
        });
        grid.Columns.Add(new DataGridTextColumn
        {
            Header = "Khối lượng (m³)",
            Binding = new Binding(nameof(BreakdownNode.Quantity)) { StringFormat = "N1" },
            Width = 140
        });

        var counter = new TextBlock
        {
            FontSize = 11.5,
            Margin = new Thickness(0, 8, 0, 0),
            VerticalAlignment = VerticalAlignment.Center
        };
        counter.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");

        void Sync() => counter.Text = $"Đang hiện {grid.VisibleItemCount} / 12 dòng";

        // Open every branch so the indent levels and chevrons are visible straight away.
        grid.ExpandAll();

        var expand = new Button { Content = "Mở hết", Padding = new Thickness(12, 5, 12, 5) };
        expand.SetResourceReference(FrameworkElement.StyleProperty, "CONCOST.Button.Secondary");
        expand.Click += (_, _) => { grid.ExpandAll(); Sync(); };

        var collapse = new Button { Content = "Gấp hết", Padding = new Thickness(12, 5, 12, 5), Margin = new Thickness(8, 0, 0, 0) };
        collapse.SetResourceReference(FrameworkElement.StyleProperty, "CONCOST.Button.Secondary");
        collapse.Click += (_, _) => { grid.CollapseAll(); Sync(); };

        var bar = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 10, 0, 0) };
        bar.Children.Add(expand);
        bar.Children.Add(collapse);
        bar.Children.Add(counter);

        var stack = new StackPanel();
        stack.Children.Add(grid);
        stack.Children.Add(bar);

        Sync();

        var root = new StackPanel();
        root.Children.Add(DemoSection(
            "Cây phân tách khối lượng — dòng chính là item, không phải bản sao",
            stack,
            xaml: "<controls:TreeDataGrid RootItems=\"{Binding Nodes}\"\n" +
                  "    ChildrenPath=\"Children\"\n" +
                  "    HierarchyHeader=\"Cấu kiện\"\n" +
                  "    HierarchyPath=\"Name\"\n" +
                  "    IndentSize=\"18\" />",
            csharp: "// Mở/gấp theo code\n" +
                    "grid.ExpandAll();\n" +
                    "grid.CollapseAll();\n" +
                    "grid.ExpandTo(node);      // mở đường tới node\n" +
                    "grid.ToggleNode(node);\n\n" +
                    "// Trạng thái mở khoá theo tham chiếu,\n" +
                    "// nên dựng lại ItemsSource vẫn giữ nguyên."));

        return root;
    }

    private object CreateMultiSelectDataGridDemo()
    {
        var rows = new List<BeamRow>
        {
            new() { Code = "D1-01", Section = "300×600", Length = 6.2, Weight = 268.4 },
            new() { Code = "D1-02", Section = "300×600", Length = 6.2, Weight = 268.4 },
            new() { Code = "D1-03", Section = "300×600", Length = 5.8, Weight = 251.1 },
            new() { Code = "D2-01", Section = "250×500", Length = 4.6, Weight = 172.3 },
            new() { Code = "D2-02", Section = "250×500", Length = 4.6, Weight = 172.3 },
            new() { Code = "C1-01", Section = "400×400", Length = 3.6, Weight = 339.8 },
            new() { Code = "C1-02", Section = "400×400", Length = 3.6, Weight = 339.8 }
        };

        var grid = new MultiSelectDataGrid
        {
            ItemsSource = rows,
            ShowCheckBoxColumn = true,
            CheckBoxColumnWidth = 44,
            Height = 320,
            AutoGenerateColumns = false,
            SelectionMode = DataGridSelectionMode.Extended
        };

        grid.Columns.Add(new DataGridTextColumn { Header = "Mã", Binding = new Binding(nameof(BeamRow.Code)), Width = 110 });
        grid.Columns.Add(new DataGridTextColumn { Header = "Tiết diện", Binding = new Binding(nameof(BeamRow.Section)), Width = 110 });
        grid.Columns.Add(new DataGridTextColumn
        {
            Header = "Dài (m)",
            Binding = new Binding(nameof(BeamRow.Length)) { StringFormat = "N2" },
            Width = 100
        });
        grid.Columns.Add(new DataGridTextColumn
        {
            Header = "Nặng (kg)",
            Binding = new Binding(nameof(BeamRow.Weight)) { StringFormat = "N1" },
            Width = 110
        });

        var status = new TextBlock { FontSize = 11.5, VerticalAlignment = VerticalAlignment.Center };
        status.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");

        void Sync() =>
            status.Text = grid.SelectedCount == 0
                ? "Chưa chọn dòng nào"
                : $"{grid.SelectedCount} dòng · {grid.SelectedItemsList.OfType<BeamRow>().Sum(r => r.Weight):N1} kg";

        grid.SelectionChanged += (_, _) => Sync();

        var all = new Button { Content = "Chọn hết", Padding = new Thickness(12, 5, 12, 5) };
        all.SetResourceReference(FrameworkElement.StyleProperty, "CONCOST.Button.Secondary");
        all.Click += (_, _) => { grid.SelectEverything(); Sync(); };

        var none = new Button { Content = "Bỏ chọn", Padding = new Thickness(12, 5, 12, 5), Margin = new Thickness(8, 0, 0, 0) };
        none.SetResourceReference(FrameworkElement.StyleProperty, "CONCOST.Button.Secondary");
        none.Click += (_, _) => { grid.ClearEverything(); Sync(); };

        var invert = new Button { Content = "Đảo chọn", Padding = new Thickness(12, 5, 12, 5), Margin = new Thickness(8, 0, 0, 0) };
        invert.SetResourceReference(FrameworkElement.StyleProperty, "CONCOST.Button.Secondary");
        invert.Click += (_, _) => { grid.InvertSelection(); Sync(); };

        var bar = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 10, 0, 0) };
        bar.Children.Add(all);
        bar.Children.Add(none);
        bar.Children.Add(invert);
        bar.Children.Add(new Border { Width = 16 });
        bar.Children.Add(status);

        var stack = new StackPanel();
        stack.Children.Add(grid);
        stack.Children.Add(bar);

        Sync();

        var root = new StackPanel();
        root.Children.Add(DemoSection(
            "Chọn nhiều dòng có ô tick ba trạng thái",
            stack,
            xaml: "<controls:MultiSelectDataGrid ItemsSource=\"{Binding Beams}\"\n" +
                  "    ShowCheckBoxColumn=\"True\"\n" +
                  "    CheckBoxColumnWidth=\"44\" />",
            csharp: "// Ô tick ở header là ba trạng thái:\n" +
                    "// IsAllSelected đọc/ghi được\n" +
                    "grid.IsAllSelected = true;\n\n" +
                    "grid.SelectEverything();\n" +
                    "grid.ClearEverything();\n" +
                    "grid.InvertSelection();\n\n" +
                    "// Đếm và danh sách chọn\n" +
                    "int n = grid.SelectedCount;\n" +
                    "var items = grid.SelectedItemsList;   // observable"));

        return root;
    }

    /// <summary>One treemap tile.</summary>
    public class FolderSize
    {
        public string Label { get; set; } = string.Empty;
        public double Value { get; set; }
    }

    private object CreateTreeMapDemo()
    {
        var map = new TreeMap
        {
            ItemsSource = new List<FolderSize>
            {
                new() { Label = "Kết cấu", Value = 1840 },
                new() { Label = "Kiến trúc", Value = 1120 },
                new() { Label = "MEP", Value = 860 },
                new() { Label = "Hạ tầng", Value = 420 },
                new() { Label = "Cảnh quan", Value = 260 },
                new() { Label = "Nội thất", Value = 180 },
                new() { Label = "Khác", Value = 90 }
            },
            ValuePath = nameof(FolderSize.Value),
            LabelPath = nameof(FolderSize.Label),
            Palette = new[]
            {
                (Brush)new BrushConverter().ConvertFromString("#3B82F6")!,
                (Brush)new BrushConverter().ConvertFromString("#6366F1")!,
                (Brush)new BrushConverter().ConvertFromString("#8B5CF6")!,
                (Brush)new BrushConverter().ConvertFromString("#0EA5E9")!,
                (Brush)new BrushConverter().ConvertFromString("#14B8A6")!,
                (Brush)new BrushConverter().ConvertFromString("#F59E0B")!,
                (Brush)new BrushConverter().ConvertFromString("#64748B")!
            },
            Height = 300,
            Gap = 3,
            CornerRadius = 6,
            ValueFormat = "N0"
        };

        var readout = new TextBlock { FontSize = 11.5, Margin = new Thickness(0, 10, 0, 0) };
        readout.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");
        readout.Text = "Bấm vào một ô để xem tỉ trọng.";

        map.ItemInvoked += (_, e) =>
            readout.Text = $"{e.Entry.Label} — {e.Entry.Value:N0} MB · {e.Entry.Share:P1} tổng dung lượng";

        var stack = new StackPanel();
        stack.Children.Add(map);
        stack.Children.Add(readout);

        var root = new StackPanel();
        root.Children.Add(DemoSection(
            "Bản đồ tỉ trọng — thuật toán squarified",
            stack,
            xaml: "<controls:TreeMap ItemsSource=\"{Binding Sizes}\"\n" +
                  "    ValuePath=\"Value\" LabelPath=\"Label\"\n" +
                  "    Gap=\"3\" CornerRadius=\"6\" />",
            csharp: "map.ItemInvoked += (_, e) =>\n" +
                    "    Show(e.Entry.Label, e.Entry.Share);\n\n" +
                    "// Truy vấn ngược từ toạ độ chuột\n" +
                    "var hit = map.HitTest(point);\n\n" +
                    "// Dữ liệu đổi thì gọi Invalidate()"));

        return root;
    }

    private object CreateSearchableTreePickerDemo()
    {
        var picker = new SearchableTreePicker
        {
            ItemsSource = SampleBreakdown(),
            ChildrenPath = nameof(BreakdownNode.Children),
            DisplayPath = nameof(BreakdownNode.Name),
            SelectionMode = PickerSelectionMode.Multiple,
            Height = 420,
            Width = 330,
            HorizontalAlignment = HorizontalAlignment.Left,
            Placeholder = "Tìm cấu kiện (không cần dấu)..."
        };

        var readout = new TextBlock
        {
            Text = "Chưa chọn cấu kiện nào.",
            FontSize = 12,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 0, 0, 8)
        };
        readout.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");

        var selected = new TextBlock
        {
            FontFamily = new FontFamily("Consolas"),
            FontSize = 11.5,
            TextWrapping = TextWrapping.Wrap,
            LineHeight = 19
        };
        selected.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextPrimary");

        picker.SelectionChanged += (_, _) =>
        {
            readout.Text = picker.SelectedCount == 0
                ? "Chưa chọn cấu kiện nào."
                : $"Đã chọn {picker.SelectedCount} mục:";

            selected.Text = string.Join("\n", picker.SelectedItems
                .OfType<BreakdownNode>()
                .Select(n => "• " + n.Name));
        };

        var info = new StackPanel { Margin = new Thickness(18, 0, 0, 0), VerticalAlignment = VerticalAlignment.Top };
        info.Children.Add(readout);
        info.Children.Add(selected);

        var columns = new Grid();
        columns.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        columns.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        Grid.SetColumn(picker, 0);
        Grid.SetColumn(info, 1);
        columns.Children.Add(picker);
        columns.Children.Add(info);

        var note = new TextBlock
        {
            Text = "Gõ “dam d1” hay “tang 1” đều tìm được — bộ lọc bỏ dấu và giữ lại nhánh cha để thấy đường dẫn.",
            FontSize = 11.5,
            Margin = new Thickness(0, 10, 0, 0),
            TextWrapping = TextWrapping.Wrap
        };
        note.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");

        var stack = new StackPanel();
        stack.Children.Add(columns);
        stack.Children.Add(note);

        var root = new StackPanel();
        root.Children.Add(DemoSection(
            "Cây có tìm kiếm, chọn nhiều",
            stack,
            xaml: "<controls:SearchableTreePicker\n" +
                  "    ItemsSource=\"{Binding Nodes}\"\n" +
                  "    ChildrenPath=\"Children\"\n" +
                  "    DisplayPath=\"Name\"\n" +
                  "    SelectionMode=\"Multiple\"\n" +
                  "    Placeholder=\"Tìm cấu kiện...\" />",
            csharp: "picker.SelectionChanged += (_, _) =>\n" +
                    "    Show(picker.SelectedItems);\n\n" +
                    "// Bộ lọc giữ nhánh cha, nên kết quả vẫn thấy đường dẫn\n" +
                    "picker.SearchText = \"dam d1\";\n\n" +
                    "// Chọn tất cả những gì đang hiện (sau khi lọc)\n" +
                    "picker.SelectAllVisible();"));

        return root;
    }

    // =========================================================================
    // DEMO: docking shell
    // =========================================================================

    private object CreateDockingManagerDemo()
    {
        var manager = new DockingManager { Height = 460 };

        var tree = new ToolWindow
        {
            Header = "Cấu kiện",
            Icon = IconKind.Layers,
            Dock = DockSide.Left,
            DockSize = 210
        };
        var treeView = new TreeView { BorderThickness = new Thickness(0) };
        foreach (var name in new[] { "Tầng 1", "Tầng 2", "Mái", "Móng" })
        {
            treeView.Items.Add(new TreeViewItem { Header = name, IsExpanded = true });
        }
        tree.Content = treeView;

        // A second pane on the same edge: two panels docked left are tabs of one group, the way an IDE
        // behaves, instead of each taking its own slice of the edge.
        var layers = new ToolWindow
        {
            Header = "Lớp",
            Icon = IconKind.LayerColor,
            Dock = DockSide.Left,
            DockSize = 210
        };
        var layerList = new ListBox { BorderThickness = new Thickness(0) };
        foreach (var name in new[] { "Kết cấu", "Kiến trúc", "MEP", "Cảnh quan" })
        {
            layerList.Items.Add(name);
        }
        layers.Content = layerList;

        var props = new ToolWindow
        {
            Header = "Thuộc tính",
            Icon = IconKind.Settings,
            Dock = DockSide.Right,
            DockSize = 230
        };
        var propList = new StackPanel { Margin = new Thickness(12) };
        foreach (var (key, value) in new[]
                 {
                     ("Tiết diện", "300×600"),
                     ("Chiều dài", "6.20 m"),
                     ("Bê tông", "B25"),
                     ("Thép", "CB400V"),
                     ("Lớp bảo vệ", "25 mm")
                 })
        {
            var row = new Grid { Margin = new Thickness(0, 0, 0, 8) };
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var k = new TextBlock { Text = key, FontSize = 11.5 };
            k.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");
            row.Children.Add(k);

            var v = new TextBlock { Text = value, FontSize = 11.5, FontWeight = FontWeights.SemiBold };
            v.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextPrimary");
            Grid.SetColumn(v, 1);
            row.Children.Add(v);

            propList.Children.Add(row);
        }
        props.Content = propList;

        var log = new ToolWindow
        {
            Header = "Nhật ký",
            Icon = IconKind.FileText,
            Dock = DockSide.Bottom,
            DockSize = 120
        };
        var logBox = new TextBox
        {
            IsReadOnly = true,
            BorderThickness = new Thickness(0),
            FontFamily = new FontFamily("Consolas"),
            FontSize = 11.5,
            Text = "[09:12] Nạp mô hình: 1 284 cấu kiện\n[09:12] Bóc tách khối lượng xong\n[09:13] Đã lưu bảng khối lượng"
        };
        log.Content = logBox;

        var canvas = new ToolWindow
        {
            Header = "Bản vẽ",
            Icon = IconKind.Drawing,
            Dock = DockSide.Center,
            ShowHeader = true
        };
        var drawing = new Border { CornerRadius = new CornerRadius(6), Margin = new Thickness(10) };
        drawing.SetResourceReference(Border.BackgroundProperty, "CONCOST.Brush.SurfaceSecondary");
        var drawingText = new TextBlock
        {
            Text = "Vùng giữa — kéo header của panel bên cạnh để dock sang đây",
            FontSize = 12,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            TextWrapping = TextWrapping.Wrap,
            MaxWidth = 280,
            TextAlignment = TextAlignment.Center
        };
        drawingText.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");
        drawing.Child = drawingText;
        canvas.Content = drawing;

        // Nested groups: the left edge is split into two rows, and the top row is itself a tab group.
        // This is the shape the four-slot version could not express.
        var leftTabs = new DockPaneGroup { Mode = DockGroupMode.Tabs };
        DockPaneGroup.SetProportion(leftTabs, 2);
        leftTabs.Children.Add(tree);
        leftTabs.Children.Add(layers);

        var search = new ToolWindow
        {
            Header = "Tìm kiếm",
            Icon = IconKind.Search,
            Dock = DockSide.Left
        };
        var searchBox = new StackPanel { Margin = new Thickness(10) };
        var find = new TextBox { Text = "dầm D1" };
        find.SetResourceReference(FrameworkElement.StyleProperty, "CONCOST.TextBox.Base");
        searchBox.Children.Add(find);
        var findHint = new TextBlock
        {
            Text = "3 kết quả trong 1 284 cấu kiện",
            FontSize = 11,
            Margin = new Thickness(0, 8, 0, 0)
        };
        findHint.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");
        searchBox.Children.Add(findHint);
        search.Content = searchBox;
        DockPaneGroup.SetProportion(search, 1);

        var leftGroup = new DockPaneGroup
        {
            Dock = DockSide.Left,
            DockSize = 250,
            Orientation = Orientation.Vertical,
            Mode = DockGroupMode.Split
        };
        leftGroup.Children.Add(leftTabs);
        leftGroup.Children.Add(search);

        manager.Children.Add(leftGroup);
        manager.Children.Add(props);
        manager.Children.Add(log);
        manager.Children.Add(canvas);

        var readout = new TextBlock { FontSize = 11.5, Margin = new Thickness(0, 10, 0, 0) };
        readout.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");
        readout.Text = "Trái: nhóm chia hai hàng, hàng trên có hai tab. Phải, dưới và vùng giữa là panel đơn.";

        manager.PaneDocked += (_, pane) => readout.Text = $"Đã dock “{pane.Header}” sang {pane.Dock}.";
        manager.PaneClosed += (_, pane) => readout.Text = $"Đã đóng “{pane.Header}”.";

        var autoHide = new Button { Content = "Ghim / tự ẩn panel phải", Padding = new Thickness(12, 5, 12, 5), Margin = new Thickness(0, 10, 0, 0) };
        autoHide.SetResourceReference(FrameworkElement.StyleProperty, "CONCOST.Button.Secondary");
        autoHide.HorizontalAlignment = HorizontalAlignment.Left;
        autoHide.Click += (_, _) =>
        {
            props.ToggleAutoHide();
            readout.Text = props.IsAutoHidden
                ? "Panel phải đã thu về dải tab ở mép — bấm tab để mở flyout."
                : "Panel phải đã ghim lại vào layout.";
        };

        var saveLayout = new Button { Content = "Lưu layout", Padding = new Thickness(12, 5, 12, 5), Margin = new Thickness(8, 10, 0, 0) };
        saveLayout.SetResourceReference(FrameworkElement.StyleProperty, "CONCOST.Button.Secondary");

        var restoreLayout = new Button { Content = "Khôi phục", Padding = new Thickness(12, 5, 12, 5), Margin = new Thickness(8, 10, 0, 0) };
        restoreLayout.SetResourceReference(FrameworkElement.StyleProperty, "CONCOST.Button.Secondary");

        string? saved = null;

        saveLayout.Click += (_, _) =>
        {
            saved = manager.CaptureLayout();
            readout.Text = $"Đã lưu layout ({saved.Split('\n').Length - 1} panel) — thử kéo cạnh rồi khôi phục.";
            ToastService.Success("Đã lưu layout");
        };

        restoreLayout.Click += (_, _) =>
        {
            if (saved is null)
            {
                readout.Text = "Chưa có layout nào được lưu.";
                return;
            }

            manager.ApplyLayout(saved);
            readout.Text = "Đã khôi phục layout đã lưu.";
        };

        var floatButton = new Button { Content = "Tách cửa sổ nổi", Padding = new Thickness(12, 5, 12, 5), Margin = new Thickness(8, 10, 0, 0) };
        floatButton.SetResourceReference(FrameworkElement.StyleProperty, "CONCOST.Button.Secondary");
        floatButton.Click += (_, _) =>
        {
            // Float is idempotent: a second click brings the existing window forward instead of
            // opening another one, so there is no handler to accumulate either.
            manager.Float(props);
            readout.Text = "Đã tách “Thuộc tính” ra cửa sổ nổi — đóng cửa sổ đó để dock lại.";
        };

        var buttons = new StackPanel { Orientation = Orientation.Horizontal };
        buttons.Children.Add(autoHide);
        buttons.Children.Add(floatButton);
        buttons.Children.Add(saveLayout);
        buttons.Children.Add(restoreLayout);

        var stack = new StackPanel();
        stack.Children.Add(manager);
        stack.Children.Add(readout);
        stack.Children.Add(buttons);

        var root = new StackPanel();
        root.Children.Add(DemoSection(
            "Dock bốn cạnh, kéo để đổi chỗ, ghim để tự ẩn",
            stack,
            xaml: "<controls:DockingManager>\n" +
                  "    <controls:ToolWindow Header=\"Cấu kiện\" Dock=\"Left\" DockSize=\"210\" />\n" +
                  "    <controls:ToolWindow Header=\"Thuộc tính\" Dock=\"Right\" DockSize=\"230\" />\n" +
                  "    <controls:ToolWindow Header=\"Nhật ký\" Dock=\"Bottom\" DockSize=\"120\" />\n" +
                  "    <controls:ToolWindow Header=\"Bản vẽ\" Dock=\"Center\" />\n" +
                  "</controls:DockingManager>",
            csharp: "// Dock theo code (menu View)\n" +
                    "manager.Dock(pane, DockSide.Right);\n\n" +
                    "// Tự ẩn / flyout\n" +
                    "pane.ToggleAutoHide();\n" +
                    "manager.ToggleFlyout(pane);\n" +
                    "manager.CloseFlyouts();\n\n" +
                    "// Đóng: Closing cho phép huỷ\n" +
                    "pane.Closing += (_, e) => e.Cancel = HasUnsaved();\n" +
                    "manager.ClosePane(pane);"));

        return root;
    }

    private object CreateDocumentTabControlDemo()
    {
        var tabs = new DocumentTabControl
        {
            Height = 230,
            ShowCloseButtons = true
        };

        var docs = new (string Title, IconKind Icon, bool Dirty)[]
        {
            ("Bản vẽ kết cấu.dwg", IconKind.Drawing, false),
            ("Bảng khối lượng.xlsx", IconKind.FileExcel, true),
            ("Mặt bằng tầng 1.dwg", IconKind.Drawing, false),
            ("Ghi chú.txt", IconKind.FileText, false)
        };

        foreach (var (title, icon, dirty) in docs)
        {
            var body = new StackPanel { Margin = new Thickness(16) };

            var heading = new TextBlock { Text = title, FontSize = 13, FontWeight = FontWeights.SemiBold };
            heading.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextPrimary");
            body.Children.Add(heading);

            var hint = new TextBlock
            {
                Text = "Nội dung tài liệu hiển thị ở đây.",
                FontSize = 11.5,
                Margin = new Thickness(0, 6, 0, 0)
            };
            hint.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");
            body.Children.Add(hint);

            tabs.Items.Add(new DocumentTabItem
            {
                Header = title,
                Icon = icon,
                IsDirty = dirty,
                Content = body
            });
        }

        var readout = new TextBlock { FontSize = 11.5, Margin = new Thickness(0, 10, 0, 0) };
        readout.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");
        readout.Text = $"{tabs.Documents.Count()} tài liệu đang mở.";

        tabs.SelectionChanged += (_, _) =>
        {
            var current = tabs.SelectedItem as DocumentTabItem;
            readout.Text = current is null ? "Đang xem: —" : $"Đang xem: {current.Header}";
        };

        var closeCurrent = new Button { Content = "Đóng tab đang xem", Padding = new Thickness(12, 5, 12, 5), Margin = new Thickness(0, 10, 0, 0) };
        closeCurrent.SetResourceReference(FrameworkElement.StyleProperty, "CONCOST.Button.Secondary");
        closeCurrent.HorizontalAlignment = HorizontalAlignment.Left;
        closeCurrent.Click += (_, _) =>
        {
            tabs.CloseTab(tabs.SelectedItem);
            readout.Text = $"{tabs.Documents.Count()} tài liệu đang mở.";
        };

        var stack = new StackPanel();
        stack.Children.Add(tabs);
        stack.Children.Add(readout);
        stack.Children.Add(closeCurrent);

        var root = new StackPanel();
        root.Children.Add(DemoSection(
            "Tab tài liệu: icon, chấm chưa lưu, nút đóng",
            stack,
            xaml: "<controls:DocumentTabControl ShowCloseButtons=\"True\">\n" +
                  "    <controls:DocumentTabItem Header=\"Bản vẽ.dwg\"\n" +
                  "        Icon=\"Drawing\" IsDirty=\"False\" />\n" +
                  "    <controls:DocumentTabItem Header=\"Khối lượng.xlsx\"\n" +
                  "        Icon=\"FileExcel\" IsDirty=\"True\" />\n" +
                  "</controls:DocumentTabControl>",
            csharp: "// Đóng là một YÊU CẦU, không phải lệnh:\n" +
                    "// CloseCommand do host quyết định (hỏi lưu trước).\n" +
                    "tabs.CloseTab(document);\n" +
                    "tabs.CloseOtherTabs(document);\n" +
                    "tabs.CloseAllTabs();\n\n" +
                    "// Chấm chưa lưu do host đặt\n" +
                    "tab.IsDirty = true;"));

        return root;
    }

    // =========================================================================
    // DEMO: shell chrome — status bar, command bar, quick access toolbar
    // =========================================================================

    private object CreateStatusBarDemo()
    {
        var shell = new Border
        {
            Height = 190,
            CornerRadius = new CornerRadius(8),
            BorderThickness = new Thickness(1)
        };
        shell.SetResourceReference(Border.BorderBrushProperty, "CONCOST.Brush.Border");

        var dock = new DockPanel { LastChildFill = true };

        var bar = new StatusBar();
        DockPanel.SetDock(bar, Dock.Bottom);
        bar.Items.Add(new StatusBarItem { Text = "Sẵn sàng", Icon = IconKind.CheckCircle });
        bar.Items.Add(new StatusBarItem { Text = "1 284 cấu kiện", Icon = IconKind.Layers });
        bar.Items.Add(new StatusBarItem
        {
            Text = "3 cảnh báo",
            Icon = IconKind.Warning,
            Command = new RelayCommand(() => ToastService.Warning("Mở danh sách cảnh báo"))
        });
        bar.Items.Add(new StatusBarItem { Text = "mm", Alignment = StatusBarAlignment.Right, IsSeparator = true });
        bar.Items.Add(new StatusBarItem { Text = "100%", Alignment = StatusBarAlignment.Right });

        var body = new TextBlock
        {
            Text = "Vùng nội dung cửa sổ",
            FontSize = 12,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        body.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");

        dock.Children.Add(bar);
        dock.Children.Add(body);
        shell.Child = dock;

        var note = new TextBlock
        {
            Text = "Ô “3 cảnh báo” có Command nên bấm được; ô chữ không có command thì con trỏ để mặc định.",
            FontSize = 11.5,
            Margin = new Thickness(0, 10, 0, 0),
            TextWrapping = TextWrapping.Wrap
        };
        note.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");

        var stack = new StackPanel();
        stack.Children.Add(shell);
        stack.Children.Add(note);

        var root = new StackPanel();
        root.Children.Add(DemoSection(
            "Hai phía, một khoảng trống ở giữa",
            stack,
            xaml: "<controls:StatusBar>\n" +
                  "    <controls:StatusBarItem Text=\"Sẵn sàng\" Icon=\"CheckCircle\" />\n" +
                  "    <controls:StatusBarItem Text=\"100%\" Alignment=\"Right\" />\n" +
                  "</controls:StatusBar>",
            csharp: "bar.Items.Add(new StatusBarItem\n" +
                    "{\n" +
                    "    Text = \"3 cảnh báo\",\n" +
                    "    Icon = IconKind.Warning,\n" +
                    "    Command = ShowWarningsCommand\n" +
                    "});\n\n" +
                    "// Không cần Grid cột * để đẩy hai nhóm ra hai đầu:\n" +
                    "// StatusBarPanel xếp trái từ mép trái, phải từ mép phải."));

        return root;
    }

    private object CreateCommandBarDemo()
    {
        var commands = new[]
        {
            new CommandBarItem("Xuất PDF", new RelayCommand(() => ToastService.Success("Đang xuất PDF…")), IconKind.FilePdf),
            new CommandBarItem("Xuất Excel", new RelayCommand(() => ToastService.Success("Đang xuất Excel…")), IconKind.FileExcel),
            new CommandBarItem("In", new RelayCommand(() => ToastService.Info("Đang gửi lệnh in")), IconKind.Print),
            new CommandBarItem { IsSeparator = true },
            new CommandBarItem("Cấu hình", new RelayCommand(() => ToastService.Info("Mở cấu hình")), IconKind.Settings),
            new CommandBarItem("Trợ giúp", new RelayCommand(() => ToastService.Info("Mở tài liệu")), IconKind.Help)
        };

        var wide = new CommandBar { ItemsSource = commands };
        var narrow = new CommandBar { ItemsSource = commands, Width = 260, HorizontalAlignment = HorizontalAlignment.Left };

        var wideNote = new TextBlock { FontSize = 11, Margin = new Thickness(0, 4, 0, 0) };
        wideNote.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");

        var narrowNote = new TextBlock { FontSize = 11, Margin = new Thickness(0, 4, 0, 0) };
        narrowNote.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");

        wide.LayoutUpdated += (_, _) =>
            wideNote.Text = wide.HasOverflow
                ? $"Tràn {wide.OverflowItems.Cast<object>().Count()} lệnh"
                : "Đủ chỗ — không có lệnh nào bị ẩn";

        narrow.LayoutUpdated += (_, _) =>
            narrowNote.Text = narrow.HasOverflow
                ? $"Tràn {narrow.OverflowItems.Cast<object>().Count()} lệnh — bấm mũi tên để mở"
                : "Đủ chỗ";

        var stack = new StackPanel();
        stack.Children.Add(new TextBlock { Text = "Toàn chiều rộng", FontSize = 11.5, Margin = new Thickness(0, 0, 0, 4) });
        stack.Children.Add(wide);
        stack.Children.Add(wideNote);
        stack.Children.Add(new TextBlock { Text = "Bị bó còn 260px", FontSize = 11.5, Margin = new Thickness(0, 16, 0, 4) });
        stack.Children.Add(narrow);
        stack.Children.Add(narrowNote);

        var root = new StackPanel();
        root.Children.Add(DemoSection(
            "Hết chỗ thì rơi vào menu, không bị cắt cụt",
            stack,
            xaml: "<controls:CommandBar ItemsSource=\"{Binding Commands}\"\n" +
                  "    OverflowToolTip=\"Thêm lệnh\" />",
            csharp: "bar.ItemsSource = new[]\n" +
                    "{\n" +
                    "    new CommandBarItem(\"Xuất PDF\", ExportPdf, IconKind.FilePdf),\n" +
                    "    new CommandBarItem { IsSeparator = true },\n" +
                    "    new CommandBarItem(\"In\", Print, IconKind.Print)\n" +
                    "};\n\n" +
                    "// Ai bị ẩn — do OverflowPanel đo, control chỉ đọc lại\n" +
                    "if (bar.HasOverflow) ...bar.OverflowItems..."));

        return root;
    }

    private object CreateQuickAccessToolbarDemo()
    {
        var items = new[]
        {
            new QuickAccessItem(IconKind.Save, "Lưu (Ctrl+S)", new RelayCommand(() => ToastService.Success("Đã lưu"))),
            new QuickAccessItem(IconKind.Undo, "Hoàn tác (Ctrl+Z)", new RelayCommand(() => ToastService.Info("Hoàn tác"))),
            new QuickAccessItem(IconKind.Redo, "Làm lại (Ctrl+Y)", new RelayCommand(() => ToastService.Info("Làm lại"))),
            new QuickAccessItem { IsSeparator = true },
            new QuickAccessItem(IconKind.Print, "In (Ctrl+P)", new RelayCommand(() => ToastService.Info("In"))),
            new QuickAccessItem(IconKind.ZoomIn, "Phóng to", new RelayCommand(() => ToastService.Info("Phóng to"))),
            new QuickAccessItem(IconKind.ZoomOut, "Thu nhỏ", new RelayCommand(() => ToastService.Info("Thu nhỏ"))),
            new QuickAccessItem(IconKind.Settings, "Cài đặt", new RelayCommand(() => ToastService.Info("Cài đặt")))
        };

        var strip = new Border { CornerRadius = new CornerRadius(6), BorderThickness = new Thickness(1), Padding = new Thickness(4) };
        strip.SetResourceReference(Border.BorderBrushProperty, "CONCOST.Brush.Border");
        strip.SetResourceReference(Border.BackgroundProperty, "CONCOST.Brush.Surface");

        var full = new QuickAccessToolbar { ItemsSource = items };
        strip.Child = full;

        var compact = new QuickAccessToolbar
        {
            ItemsSource = items,
            Width = 130,
            IsCustomizable = false,
            HorizontalAlignment = HorizontalAlignment.Left
        };

        var readout = new TextBlock { FontSize = 11, Margin = new Thickness(0, 8, 0, 0) };
        readout.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");
        full.LayoutUpdated += (_, _) => readout.Text = full.HasOverflow
            ? $"Bản đầy đủ tràn {full.OverflowItems.Cast<object>().Count()} lệnh"
            : "Bản đầy đủ: đủ chỗ cho cả 7 lệnh";

        var stack = new StackPanel();
        stack.Children.Add(strip);
        stack.Children.Add(readout);
        stack.Children.Add(new TextBlock { Text = "Bị bó còn 130px", FontSize = 11.5, Margin = new Thickness(0, 16, 0, 6) });
        stack.Children.Add(compact);

        var root = new StackPanel();
        root.Children.Add(DemoSection(
            "Dải icon gọn — dùng được ngoài ribbon",
            stack,
            xaml: "<controls:QuickAccessToolbar ItemsSource=\"{Binding QuickCommands}\"\n" +
                  "    IsCustomizable=\"False\" />",
            csharp: "qat.ItemsSource = new[]\n" +
                    "{\n" +
                    "    new QuickAccessItem(IconKind.Save, \"Lưu\", SaveCommand),\n" +
                    "    new QuickAccessItem { IsSeparator = true },\n" +
                    "    new QuickAccessItem(IconKind.Print, \"In\", PrintCommand)\n" +
                    "};"));

        return root;
    }
}
