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
            Name = Loc.T("Tầng 1", "Level 1"), Count = 48, Quantity = 412.6,
            Children =
            {
                new BreakdownNode
                {
                    Name = Loc.T("Cột", "Column"), Count = 16, Quantity = 96.4,
                    Children =
                    {
                        new BreakdownNode { Name = "C1 (400×400)", Count = 8, Quantity = 48.2 },
                        new BreakdownNode { Name = "C2 (300×300)", Count = 8, Quantity = 48.2 }
                    }
                },
                new BreakdownNode
                {
                    Name = Loc.T("Dầm", "Beam"), Count = 24, Quantity = 214.8,
                    Children =
                    {
                        new BreakdownNode { Name = "D1 (300×600)", Count = 14, Quantity = 142.6 },
                        new BreakdownNode { Name = "D2 (250×500)", Count = 10, Quantity = 72.2 }
                    }
                },
                new BreakdownNode { Name = Loc.T("Sàn", "Slab"), Count = 8, Quantity = 101.4 }
            }
        },
        new BreakdownNode
        {
            Name = Loc.T("Tầng 2", "Level 2"), Count = 36, Quantity = 298.2,
            Children =
            {
                new BreakdownNode { Name = Loc.T("Cột", "Column"), Count = 12, Quantity = 62.1 },
                new BreakdownNode { Name = Loc.T("Dầm", "Beam"), Count = 18, Quantity = 168.5 },
                new BreakdownNode { Name = Loc.T("Sàn", "Slab"), Count = 6, Quantity = 67.6 }
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
            HierarchyHeader = Loc.T("Cấu kiện", "Member"),
            HierarchyPath = nameof(BreakdownNode.Name),
            HierarchyColumnWidth = 280,
            IndentSize = 18,
            Height = 320,
            AutoGenerateColumns = false
        };

        grid.Columns.Add(new DataGridTextColumn
        {
            Header = Loc.T("Số lượng", "Count"),
            Binding = new Binding(nameof(BreakdownNode.Count)),
            Width = 100
        });
        grid.Columns.Add(new DataGridTextColumn
        {
            Header = Loc.T("Khối lượng (m³)", "Volume (m³)"),
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

        void Sync() => counter.Text = Loc.F("Đang hiện {0} / 12 dòng", "Showing {0} / 12 rows", grid.VisibleItemCount);

        // Open every branch so the indent levels and chevrons are visible straight away.
        grid.ExpandAll();

        var expand = new Button { Content = Loc.T("Mở hết", "Expand all"), Padding = new Thickness(12, 5, 12, 5) };
        expand.SetResourceReference(FrameworkElement.StyleProperty, "CONCOST.Button.Secondary");
        expand.Click += (_, _) => { grid.ExpandAll(); Sync(); };

        var collapse = new Button { Content = Loc.T("Gấp hết", "Collapse all"), Padding = new Thickness(12, 5, 12, 5), Margin = new Thickness(8, 0, 0, 0) };
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
            Loc.T("Cây phân tách khối lượng — dòng chính là item, không phải bản sao", "Quantity breakdown tree — rows are items, not copies"),
            stack,
            xaml: "<controls:TreeDataGrid RootItems=\"{Binding Nodes}\"\n" +
                  "    ChildrenPath=\"Children\"\n" +
                  Loc.T("    HierarchyHeader=\"Cấu kiện\"\n", "    HierarchyHeader=\"Member\"\n") +
                  "    HierarchyPath=\"Name\"\n" +
                  "    IndentSize=\"18\" />",
            csharp: Loc.T("// Mở/gấp theo code\n", "// Expand/collapse from code") +
                    "grid.ExpandAll();\n" +
                    "grid.CollapseAll();\n" +
                    Loc.T("grid.ExpandTo(node);      // mở đường tới node\n", "grid.ExpandTo(node);      // expands the path to the node") +
                    "grid.ToggleNode(node);\n\n" +
                    Loc.T("// Trạng thái mở khoá theo tham chiếu,\n", "// Expansion state is keyed by reference,") +
                    Loc.T("// nên dựng lại ItemsSource vẫn giữ nguyên.", "// so rebuilding ItemsSource keeps it.")));

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

        grid.Columns.Add(new DataGridTextColumn { Header = Loc.T("Mã", "Mark"), Binding = new Binding(nameof(BeamRow.Code)), Width = 110 });
        grid.Columns.Add(new DataGridTextColumn { Header = Loc.T("Tiết diện", "Section"), Binding = new Binding(nameof(BeamRow.Section)), Width = 110 });
        grid.Columns.Add(new DataGridTextColumn
        {
            Header = Loc.T("Dài (m)", "Length (m)"),
            Binding = new Binding(nameof(BeamRow.Length)) { StringFormat = "N2" },
            Width = 100
        });
        grid.Columns.Add(new DataGridTextColumn
        {
            Header = Loc.T("Nặng (kg)", "Weight (kg)"),
            Binding = new Binding(nameof(BeamRow.Weight)) { StringFormat = "N1" },
            Width = 110
        });

        var status = new TextBlock { FontSize = 11.5, VerticalAlignment = VerticalAlignment.Center };
        status.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");

        void Sync() =>
            status.Text = grid.SelectedCount == 0
                ? Loc.T("Chưa chọn dòng nào", "No row selected")
                : Loc.F("{0} dòng · {1:N1} kg", "{0} rows · {1:N1} kg", grid.SelectedCount, grid.SelectedItemsList.OfType<BeamRow>().Sum(r => r.Weight));

        grid.SelectionChanged += (_, _) => Sync();

        var all = new Button { Content = Loc.T("Chọn hết", "Select all"), Padding = new Thickness(12, 5, 12, 5) };
        all.SetResourceReference(FrameworkElement.StyleProperty, "CONCOST.Button.Secondary");
        all.Click += (_, _) => { grid.SelectEverything(); Sync(); };

        var none = new Button { Content = Loc.T("Bỏ chọn", "Clear selection"), Padding = new Thickness(12, 5, 12, 5), Margin = new Thickness(8, 0, 0, 0) };
        none.SetResourceReference(FrameworkElement.StyleProperty, "CONCOST.Button.Secondary");
        none.Click += (_, _) => { grid.ClearEverything(); Sync(); };

        var invert = new Button { Content = Loc.T("Đảo chọn", "Invert selection"), Padding = new Thickness(12, 5, 12, 5), Margin = new Thickness(8, 0, 0, 0) };
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
            Loc.T("Chọn nhiều dòng có ô tick ba trạng thái", "Multi-row selection with tri-state check boxes"),
            stack,
            xaml: "<controls:MultiSelectDataGrid ItemsSource=\"{Binding Beams}\"\n" +
                  "    ShowCheckBoxColumn=\"True\"\n" +
                  "    CheckBoxColumnWidth=\"44\" />",
            csharp: Loc.T("// Ô tick ở header là ba trạng thái:\n", "// The header checkbox is tri-state:") +
                    Loc.T("// IsAllSelected đọc/ghi được\n", "// IsAllSelected reads and writes") +
                    "grid.IsAllSelected = true;\n\n" +
                    "grid.SelectEverything();\n" +
                    "grid.ClearEverything();\n" +
                    "grid.InvertSelection();\n\n" +
                    Loc.T("// Đếm và danh sách chọn\n", "// Count and the selected list") +
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
                new() { Label = Loc.T("Kết cấu", "Structure"), Value = 1840 },
                new() { Label = Loc.T("Kiến trúc", "Architecture"), Value = 1120 },
                new() { Label = "MEP", Value = 860 },
                new() { Label = Loc.T("Hạ tầng", "Infrastructure"), Value = 420 },
                new() { Label = Loc.T("Cảnh quan", "Landscape"), Value = 260 },
                new() { Label = Loc.T("Nội thất", "Interior"), Value = 180 },
                new() { Label = Loc.T("Khác", "Other"), Value = 90 }
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
        readout.Text = Loc.T("Bấm vào một ô để xem tỉ trọng.", "Click a cell to see its share.");

        map.ItemInvoked += (_, e) =>
            readout.Text = Loc.F("{0} — {1:N0} MB · {2:P1} tổng dung lượng", "{0} — {1:N0} MB · {2:P1} of total", e.Entry.Label, e.Entry.Value, e.Entry.Share);

        var stack = new StackPanel();
        stack.Children.Add(map);
        stack.Children.Add(readout);

        var root = new StackPanel();
        root.Children.Add(DemoSection(
            Loc.T("Bản đồ tỉ trọng — thuật toán squarified", "Treemap — squarified layout"),
            stack,
            xaml: "<controls:TreeMap ItemsSource=\"{Binding Sizes}\"\n" +
                  "    ValuePath=\"Value\" LabelPath=\"Label\"\n" +
                  "    Gap=\"3\" CornerRadius=\"6\" />",
            csharp: "map.ItemInvoked += (_, e) =>\n" +
                    "    Show(e.Entry.Label, e.Entry.Share);\n\n" +
                    Loc.T("// Truy vấn ngược từ toạ độ chuột\n", "// Hit-test back from the pointer coordinates") +
                    "var hit = map.HitTest(point);\n\n" +
                    Loc.T("// Dữ liệu đổi thì gọi Invalidate()", "// Call Invalidate() when the data changes")));

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
            Placeholder = Loc.T("Tìm cấu kiện (không cần dấu)...", "Find a member (no accents needed)...")
        };

        var readout = new TextBlock
        {
            Text = Loc.T("Chưa chọn cấu kiện nào.", "No member selected yet."),
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
                ? Loc.T("Chưa chọn cấu kiện nào.", "No member selected yet.")
                : Loc.F("Đã chọn {0} mục:", "Selected {0} items:", picker.SelectedCount);

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
            Text = Loc.T("Gõ “dam d1” hay “tang 1” đều tìm được — bộ lọc bỏ dấu và giữ lại nhánh cha để thấy đường dẫn.", "Type “dam d1” or “tang 1” and it still matches — the filter strips accents and keeps the parent branches so the path stays visible."),
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
            Loc.T("Cây có tìm kiếm, chọn nhiều", "Searchable tree with multi-select"),
            stack,
            xaml: "<controls:SearchableTreePicker\n" +
                  "    ItemsSource=\"{Binding Nodes}\"\n" +
                  "    ChildrenPath=\"Children\"\n" +
                  "    DisplayPath=\"Name\"\n" +
                  "    SelectionMode=\"Multiple\"\n" +
                  Loc.T("    Placeholder=\"Tìm cấu kiện...\" />", "    Placeholder=\"Find a member...\" />"),
            csharp: "picker.SelectionChanged += (_, _) =>\n" +
                    "    Show(picker.SelectedItems);\n\n" +
                    Loc.T("// Bộ lọc giữ nhánh cha, nên kết quả vẫn thấy đường dẫn\n", "// The filter keeps parent branches, so results still show their path") +
                    "picker.SearchText = \"dam d1\";\n\n" +
                    Loc.T("// Chọn tất cả những gì đang hiện (sau khi lọc)\n", "// Select everything currently visible (after filtering)") +
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
            Header = Loc.T("Cấu kiện", "Member"),
            Icon = IconKind.Layers,
            Dock = DockSide.Left,
            DockSize = 210
        };
        var treeView = new TreeView { BorderThickness = new Thickness(0) };
        foreach (var name in new[] { Loc.T("Tầng 1", "Level 1"), Loc.T("Tầng 2", "Level 2"), Loc.T("Mái", "Roof"), Loc.T("Móng", "Foundation") })
        {
            treeView.Items.Add(new TreeViewItem { Header = name, IsExpanded = true });
        }
        tree.Content = treeView;

        // A second pane on the same edge: two panels docked left are tabs of one group, the way an IDE
        // behaves, instead of each taking its own slice of the edge.
        var layers = new ToolWindow
        {
            Header = Loc.T("Lớp", "Layer"),
            Icon = IconKind.LayerColor,
            Dock = DockSide.Left,
            DockSize = 210
        };
        var layerList = new ListBox { BorderThickness = new Thickness(0) };
        foreach (var name in new[] { Loc.T("Kết cấu", "Structure"), Loc.T("Kiến trúc", "Architecture"), "MEP", Loc.T("Cảnh quan", "Landscape") })
        {
            layerList.Items.Add(name);
        }
        layers.Content = layerList;

        var props = new ToolWindow
        {
            Header = Loc.T("Thuộc tính", "Properties"),
            Icon = IconKind.Settings,
            Dock = DockSide.Right,
            DockSize = 230
        };
        var propList = new StackPanel { Margin = new Thickness(12) };
        foreach (var (key, value) in new[]
                 {
                     (Loc.T("Tiết diện", "Section"), "300×600"),
                     (Loc.T("Chiều dài", "Length"), "6.20 m"),
                     (Loc.T("Bê tông", "Concrete"), "B25"),
                     (Loc.T("Thép", "Steel"), "CB400V"),
                     (Loc.T("Lớp bảo vệ", "Cover"), "25 mm")
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
            Header = Loc.T("Nhật ký", "Log"),
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
            Text = Loc.T("[09:12] Nạp mô hình: 1 284 cấu kiện\n[09:12] Bóc tách khối lượng xong\n[09:13] Đã lưu bảng khối lượng", "[09:12] Model loaded: 1,284 members\n[09:12] Quantity take-off finished\n[09:13] Quantity schedule saved")
        };
        log.Content = logBox;

        var canvas = new ToolWindow
        {
            Header = Loc.T("Bản vẽ", "Drawing"),
            Icon = IconKind.Drawing,
            Dock = DockSide.Center,
            ShowHeader = true
        };
        var drawing = new Border { CornerRadius = new CornerRadius(6), Margin = new Thickness(10) };
        drawing.SetResourceReference(Border.BackgroundProperty, "CONCOST.Brush.SurfaceSecondary");
        var drawingText = new TextBlock
        {
            Text = Loc.T("Vùng giữa — kéo header của panel bên cạnh để dock sang đây", "Centre area — drag the header of a neighbouring panel here to dock it"),
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
            Header = Loc.T("Tìm kiếm", "Search"),
            Icon = IconKind.Search,
            Dock = DockSide.Left
        };
        var searchBox = new StackPanel { Margin = new Thickness(10) };
        var find = new TextBox { Text = Loc.T("dầm D1", "beam D1") };
        find.SetResourceReference(FrameworkElement.StyleProperty, "CONCOST.TextBox.Base");
        searchBox.Children.Add(find);
        var findHint = new TextBlock
        {
            Text = Loc.T("3 kết quả trong 1 284 cấu kiện", "3 results in 1,284 members"),
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
        readout.Text = Loc.T("Trái: nhóm chia hai hàng, hàng trên có hai tab. Phải, dưới và vùng giữa là panel đơn.", "Left: a group split into two rows, the top row with two tabs. Right, bottom and centre are single panels.");

        manager.PaneDocked += (_, pane) => readout.Text = Loc.F("Đã dock “{0}” sang {1}.", "Docked “{0}” to {1}.", pane.Header, pane.Dock);
        manager.PaneClosed += (_, pane) => readout.Text = Loc.F("Đã đóng “{0}”.", "Closed “{0}”.", pane.Header);

        var autoHide = new Button { Content = Loc.T("Ghim / tự ẩn panel phải", "Pin / auto-hide the right panel"), Padding = new Thickness(12, 5, 12, 5), Margin = new Thickness(0, 10, 0, 0) };
        autoHide.SetResourceReference(FrameworkElement.StyleProperty, "CONCOST.Button.Secondary");
        autoHide.HorizontalAlignment = HorizontalAlignment.Left;
        autoHide.Click += (_, _) =>
        {
            props.ToggleAutoHide();
            readout.Text = props.IsAutoHidden
                ? Loc.T("Panel phải đã thu về dải tab ở mép — bấm tab để mở flyout.", "The right panel folded into an edge tab strip — click a tab to open the flyout.")
                : Loc.T("Panel phải đã ghim lại vào layout.", "The right panel is pinned back into the layout.");
        };

        var saveLayout = new Button { Content = Loc.T("Lưu layout", "Save layout"), Padding = new Thickness(12, 5, 12, 5), Margin = new Thickness(8, 10, 0, 0) };
        saveLayout.SetResourceReference(FrameworkElement.StyleProperty, "CONCOST.Button.Secondary");

        var restoreLayout = new Button { Content = Loc.T("Khôi phục", "Restore"), Padding = new Thickness(12, 5, 12, 5), Margin = new Thickness(8, 10, 0, 0) };
        restoreLayout.SetResourceReference(FrameworkElement.StyleProperty, "CONCOST.Button.Secondary");

        string? saved = null;

        saveLayout.Click += (_, _) =>
        {
            saved = manager.CaptureLayout();
            readout.Text = Loc.F("Đã lưu layout ({0} panel) — thử kéo cạnh rồi khôi phục.", "Layout saved ({0} panels) — try dragging an edge, then restore.", saved.Split('\n').Length - 1);
            ToastService.Success(Loc.T("Đã lưu layout", "Layout saved"));
        };

        restoreLayout.Click += (_, _) =>
        {
            if (saved is null)
            {
                readout.Text = Loc.T("Chưa có layout nào được lưu.", "No layout has been saved yet.");
                return;
            }

            manager.ApplyLayout(saved);
            readout.Text = Loc.T("Đã khôi phục layout đã lưu.", "Restored the saved layout.");
        };

        var floatButton = new Button { Content = Loc.T("Tách cửa sổ nổi", "Float into a window"), Padding = new Thickness(12, 5, 12, 5), Margin = new Thickness(8, 10, 0, 0) };
        floatButton.SetResourceReference(FrameworkElement.StyleProperty, "CONCOST.Button.Secondary");
        floatButton.Click += (_, _) =>
        {
            // Float is idempotent: a second click brings the existing window forward instead of
            // opening another one, so there is no handler to accumulate either.
            manager.Float(props);
            readout.Text = Loc.T("Đã tách “Thuộc tính” ra cửa sổ nổi — đóng cửa sổ đó để dock lại.", "Floated “Properties” into its own window — close it to dock it back.");
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
            Loc.T("Dock bốn cạnh, kéo để đổi chỗ, ghim để tự ẩn", "Dock on four sides, drag to rearrange, pin to auto-hide"),
            stack,
            xaml: "<controls:DockingManager>\n" +
                  Loc.T("    <controls:ToolWindow Header=\"Cấu kiện\" Dock=\"Left\" DockSize=\"210\" />\n", "    <controls:ToolWindow Header=\"Members\" Dock=\"Left\" DockSize=\"210\" />\n") +
                  Loc.T("    <controls:ToolWindow Header=\"Thuộc tính\" Dock=\"Right\" DockSize=\"230\" />\n", "    <controls:ToolWindow Header=\"Properties\" Dock=\"Right\" DockSize=\"230\" />\n") +
                  Loc.T("    <controls:ToolWindow Header=\"Nhật ký\" Dock=\"Bottom\" DockSize=\"120\" />\n", "    <controls:ToolWindow Header=\"Log\" Dock=\"Bottom\" DockSize=\"120\" />\n") +
                  Loc.T("    <controls:ToolWindow Header=\"Bản vẽ\" Dock=\"Center\" />\n", "    <controls:ToolWindow Header=\"Drawing\" Dock=\"Center\" />\n") +
                  "</controls:DockingManager>",
            csharp: "// Dock theo code (menu View)\n" +
                    "manager.Dock(pane, DockSide.Right);\n\n" +
                    Loc.T("// Tự ẩn / flyout\n", "// Auto-hide / flyout") +
                    "pane.ToggleAutoHide();\n" +
                    "manager.ToggleFlyout(pane);\n" +
                    "manager.CloseFlyouts();\n\n" +
                    Loc.T("// Đóng: Closing cho phép huỷ\n", "// Closing can be cancelled") +
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
            (Loc.T("Bản vẽ kết cấu.dwg", "Structural drawing.dwg"), IconKind.Drawing, false),
            (Loc.T("Bảng khối lượng.xlsx", "Quantity schedule.xlsx"), IconKind.FileExcel, true),
            (Loc.T("Mặt bằng tầng 1.dwg", "Level 1 plan.dwg"), IconKind.Drawing, false),
            (Loc.T("Ghi chú.txt", "Notes.txt"), IconKind.FileText, false)
        };

        foreach (var (title, icon, dirty) in docs)
        {
            var body = new StackPanel { Margin = new Thickness(16) };

            var heading = new TextBlock { Text = title, FontSize = 13, FontWeight = FontWeights.SemiBold };
            heading.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextPrimary");
            body.Children.Add(heading);

            var hint = new TextBlock
            {
                Text = Loc.T("Nội dung tài liệu hiển thị ở đây.", "The document content shows here."),
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
        readout.Text = Loc.F("{0} tài liệu đang mở.", "{0} documents open.", tabs.Documents.Count());

        tabs.SelectionChanged += (_, _) =>
        {
            var current = tabs.SelectedItem as DocumentTabItem;
            readout.Text = current is null ? Loc.T("Đang xem: —", "Viewing: —") : Loc.F("Đang xem: {0}", "Viewing: {0}", current.Header);
        };

        var closeCurrent = new Button { Content = Loc.T("Đóng tab đang xem", "Close the current tab"), Padding = new Thickness(12, 5, 12, 5), Margin = new Thickness(0, 10, 0, 0) };
        closeCurrent.SetResourceReference(FrameworkElement.StyleProperty, "CONCOST.Button.Secondary");
        closeCurrent.HorizontalAlignment = HorizontalAlignment.Left;
        closeCurrent.Click += (_, _) =>
        {
            tabs.CloseTab(tabs.SelectedItem);
            readout.Text = Loc.F("{0} tài liệu đang mở.", "{0} documents open.", tabs.Documents.Count());
        };

        var stack = new StackPanel();
        stack.Children.Add(tabs);
        stack.Children.Add(readout);
        stack.Children.Add(closeCurrent);

        var root = new StackPanel();
        root.Children.Add(DemoSection(
            Loc.T("Tab tài liệu: icon, chấm chưa lưu, nút đóng", "Document tabs: icon, dirty dot, close button"),
            stack,
            xaml: "<controls:DocumentTabControl ShowCloseButtons=\"True\">\n" +
                  Loc.T("    <controls:DocumentTabItem Header=\"Bản vẽ.dwg\"\n", "    <controls:DocumentTabItem Header=\"Drawing.dwg\"\n") +
                  "        Icon=\"Drawing\" IsDirty=\"False\" />\n" +
                  Loc.T("    <controls:DocumentTabItem Header=\"Khối lượng.xlsx\"\n", "    <controls:DocumentTabItem Header=\"Quantities.xlsx\"\n") +
                  "        Icon=\"FileExcel\" IsDirty=\"True\" />\n" +
                  "</controls:DocumentTabControl>",
            csharp: Loc.T("// Đóng là một YÊU CẦU, không phải lệnh:\n", "// Closing is a REQUEST, not a command:") +
                    Loc.T("// CloseCommand do host quyết định (hỏi lưu trước).\n", "// CloseCommand is the host's call (ask to save first).") +
                    "tabs.CloseTab(document);\n" +
                    "tabs.CloseOtherTabs(document);\n" +
                    "tabs.CloseAllTabs();\n\n" +
                    Loc.T("// Chấm chưa lưu do host đặt\n", "// The unsaved dot is set by the host") +
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
        bar.Items.Add(new StatusBarItem { Text = Loc.T("Sẵn sàng", "Ready"), Icon = IconKind.CheckCircle });
        bar.Items.Add(new StatusBarItem { Text = Loc.T("1 284 cấu kiện", "1,284 members"), Icon = IconKind.Layers });
        bar.Items.Add(new StatusBarItem
        {
            Text = Loc.T("3 cảnh báo", "3 warnings"),
            Icon = IconKind.Warning,
            Command = new RelayCommand(() => ToastService.Warning(Loc.T("Mở danh sách cảnh báo", "Open the warning list")))
        });
        bar.Items.Add(new StatusBarItem { Text = "mm", Alignment = StatusBarAlignment.Right, IsSeparator = true });
        bar.Items.Add(new StatusBarItem { Text = "100%", Alignment = StatusBarAlignment.Right });

        var body = new TextBlock
        {
            Text = Loc.T("Vùng nội dung cửa sổ", "Window content area"),
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
            Text = Loc.T("Ô “3 cảnh báo” có Command nên bấm được; ô chữ không có command thì con trỏ để mặc định.", "The “3 warnings” cell has a Command so it is clickable; a text cell without one keeps the default cursor."),
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
            Loc.T("Hai phía, một khoảng trống ở giữa", "Two sides with a gap in the middle"),
            stack,
            xaml: "<controls:StatusBar>\n" +
                  Loc.T("    <controls:StatusBarItem Text=\"Sẵn sàng\" Icon=\"CheckCircle\" />\n", "    <controls:StatusBarItem Text=\"Ready\" Icon=\"CheckCircle\" />\n") +
                  "    <controls:StatusBarItem Text=\"100%\" Alignment=\"Right\" />\n" +
                  "</controls:StatusBar>",
            csharp: "bar.Items.Add(new StatusBarItem\n" +
                    "{\n" +
                    Loc.T("    Text = \"3 cảnh báo\",\n", "    Text = \"3 warnings\",\n") +
                    "    Icon = IconKind.Warning,\n" +
                    "    Command = ShowWarningsCommand\n" +
                    "});\n\n" +
                    Loc.T("// Không cần Grid cột * để đẩy hai nhóm ra hai đầu:\n", "// No * column is needed to push the two groups to the ends:") +
                    Loc.T("// StatusBarPanel xếp trái từ mép trái, phải từ mép phải.", "// StatusBarPanel lays the left group from the left edge and the right group from the right.")));

        return root;
    }

    private object CreateCommandBarDemo()
    {
        var commands = new[]
        {
            new CommandBarItem(Loc.T("Xuất PDF", "Export PDF"), new RelayCommand(() => ToastService.Success(Loc.T("Đang xuất PDF…", "Exporting PDF…"))), IconKind.FilePdf),
            new CommandBarItem(Loc.T("Xuất Excel", "Export Excel"), new RelayCommand(() => ToastService.Success(Loc.T("Đang xuất Excel…", "Exporting Excel…"))), IconKind.FileExcel),
            new CommandBarItem("In", new RelayCommand(() => ToastService.Info(Loc.T("Đang gửi lệnh in", "Sending the print job"))), IconKind.Print),
            new CommandBarItem { IsSeparator = true },
            new CommandBarItem(Loc.T("Cấu hình", "Configure"), new RelayCommand(() => ToastService.Info(Loc.T("Mở cấu hình", "Open settings"))), IconKind.Settings),
            new CommandBarItem(Loc.T("Trợ giúp", "Help"), new RelayCommand(() => ToastService.Info(Loc.T("Mở tài liệu", "Open the documentation"))), IconKind.Help)
        };

        var wide = new CommandBar { ItemsSource = commands };
        var narrow = new CommandBar { ItemsSource = commands, Width = 260, HorizontalAlignment = HorizontalAlignment.Left };

        var wideNote = new TextBlock { FontSize = 11, Margin = new Thickness(0, 4, 0, 0) };
        wideNote.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");

        var narrowNote = new TextBlock { FontSize = 11, Margin = new Thickness(0, 4, 0, 0) };
        narrowNote.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");

        wide.LayoutUpdated += (_, _) =>
            wideNote.Text = wide.HasOverflow
                ? Loc.F("Tràn {0} lệnh", "{0} commands overflowing", wide.OverflowItems.Cast<object>().Count())
                : Loc.T("Đủ chỗ — không có lệnh nào bị ẩn", "Enough room — no command is hidden");

        narrow.LayoutUpdated += (_, _) =>
            narrowNote.Text = narrow.HasOverflow
                ? Loc.F("Tràn {0} lệnh — bấm mũi tên để mở", "{0} commands overflowing — click the arrow to open", narrow.OverflowItems.Cast<object>().Count())
                : Loc.T("Đủ chỗ", "Enough room");

        var stack = new StackPanel();
        stack.Children.Add(new TextBlock { Text = Loc.T("Toàn chiều rộng", "Full width"), FontSize = 11.5, Margin = new Thickness(0, 0, 0, 4) });
        stack.Children.Add(wide);
        stack.Children.Add(wideNote);
        stack.Children.Add(new TextBlock { Text = Loc.T("Bị bó còn 260px", "Squeezed to 260px"), FontSize = 11.5, Margin = new Thickness(0, 16, 0, 4) });
        stack.Children.Add(narrow);
        stack.Children.Add(narrowNote);

        var root = new StackPanel();
        root.Children.Add(DemoSection(
            Loc.T("Hết chỗ thì rơi vào menu, không bị cắt cụt", "Overflow goes to a menu instead of clipping"),
            stack,
            xaml: "<controls:CommandBar ItemsSource=\"{Binding Commands}\"\n" +
                  Loc.T("    OverflowToolTip=\"Thêm lệnh\" />", "    OverflowToolTip=\"More commands\" />"),
            csharp: "bar.ItemsSource = new[]\n" +
                    "{\n" +
                    Loc.T("    new CommandBarItem(\"Xuất PDF\", ExportPdf, IconKind.FilePdf),\n", "    new CommandBarItem(\"Export PDF\", ExportPdf, IconKind.FilePdf),\n") +
                    "    new CommandBarItem { IsSeparator = true },\n" +
                    "    new CommandBarItem(\"In\", Print, IconKind.Print)\n" +
                    "};\n\n" +
                    Loc.T("// Ai bị ẩn — do OverflowPanel đo, control chỉ đọc lại\n", "// Which items are hidden — OverflowPanel measures it and the control just reads it back") +
                    "if (bar.HasOverflow) ...bar.OverflowItems..."));

        return root;
    }

    private object CreateQuickAccessToolbarDemo()
    {
        // One strip, with every switch that changes it beside it. Three copies of the same strip
        // differing only by a property the picker can already set is a gallery, not a workbench.
        var items = new[]
        {
            new QuickAccessItem(IconKind.Save, Loc.T("Lưu (Ctrl+S)", "Save (Ctrl+S)"), new RelayCommand(() => ToastService.Success(Loc.T("Đã lưu", "Saved")))),
            new QuickAccessItem(IconKind.Undo, Loc.T("Hoàn tác (Ctrl+Z)", "Undo (Ctrl+Z)"), new RelayCommand(() => ToastService.Info(Loc.T("Hoàn tác", "Undo")))),
            new QuickAccessItem(IconKind.Redo, Loc.T("Làm lại (Ctrl+Y)", "Redo (Ctrl+Y)"), new RelayCommand(() => ToastService.Info(Loc.T("Làm lại", "Redo")))),
            new QuickAccessItem { IsSeparator = true },
            new QuickAccessItem(IconKind.Print, Loc.T("In (Ctrl+P)", "Print (Ctrl+P)"), new RelayCommand(() => ToastService.Info(Loc.T("In", "Print")))),
            new QuickAccessItem(IconKind.ZoomIn, Loc.T("Phóng to", "Zoom in"), new RelayCommand(() => ToastService.Info(Loc.T("Phóng to", "Zoom in")))),
            new QuickAccessItem(IconKind.ZoomOut, Loc.T("Thu nhỏ", "Zoom out"), new RelayCommand(() => ToastService.Info(Loc.T("Thu nhỏ", "Zoom out")))),
            new QuickAccessItem { IsSeparator = true },
            new QuickAccessItem(IconKind.Settings, Loc.T("Cài đặt", "Settings"), new RelayCommand(() => ToastService.Info(Loc.T("Cài đặt", "Settings"))))
        };

        static TextBlock Head(string text)
        {
            var head = new TextBlock
            {
                Text = text,
                FontSize = 11.5,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 12)
            };
            head.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextPrimary");
            return head;
        }

        var strip = new QuickAccessToolbar
        {
            ItemsSource = items,
            Variant = QuickAccessToolbarVariant.Outlined,
            Width = 360,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            OverflowToolTip = Loc.T("Lệnh không vừa", "Commands that did not fit")
        };

        var readout = new TextBlock
        {
            FontSize = 11,
            Margin = new Thickness(0, 10, 0, 0),
            TextWrapping = TextWrapping.Wrap,
            LineHeight = 16
        };
        readout.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");

        var openButton = SecondaryButton(Loc.T("Mở menu tràn", "Open the overflow menu"));
        openButton.Margin = new Thickness(0, 10, 0, 0);
        openButton.HorizontalAlignment = HorizontalAlignment.Left;
        openButton.Click += (_, _) => strip.IsOverflowOpen = !strip.IsOverflowOpen;

        strip.LayoutUpdated += (_, _) =>
        {
            var hidden = strip.OverflowItems.Cast<object>()
                .OfType<QuickAccessItem>()
                .Where(item => !item.IsSeparator && !string.IsNullOrEmpty(item.ToolTip))
                .Select(item => item.ToolTip)
                .ToList();

            readout.Text = hidden.Count > 0
                ? Loc.F("Bị đẩy vào menu ({0} lệnh): {1}", "Pushed into the menu ({0} commands): {1}",
                    hidden.Count, string.Join(" · ", hidden))
                : Loc.T("Đủ chỗ cho cả 8 lệnh — kéo thanh Rộng xuống để xem menu tràn.",
                        "Room for all 8 commands — pull the width slider down to see the overflow menu.");
        };

        var stage = new Border
        {
            Height = 72,
            CornerRadius = new CornerRadius(10),
            BorderThickness = new Thickness(1),
            Padding = new Thickness(16),
            Child = strip
        };
        stage.SetResourceReference(Border.BackgroundProperty, "CONCOST.Brush.SurfaceSecondary");
        stage.SetResourceReference(Border.BorderBrushProperty, "CONCOST.Brush.Border");

        // ---- the panel ----------------------------------------------------
        var col1 = new StackPanel();
        col1.Children.Add(Head(Loc.T("KÍCH THƯỚC VÀ NHỊP", "SIZE & RHYTHM")));
        col1.Children.Add(AppearanceSlider(Loc.T("Cạnh nút", "Button size"), 20, 36, strip.ButtonSize, 1, "{0:0} px", v => strip.ButtonSize = v));
        col1.Children.Add(AppearanceSlider(Loc.T("Cỡ icon", "Icon size"), 10, 22, strip.IconSize, 1, "{0:0} px", v => strip.IconSize = v));
        col1.Children.Add(AppearanceSlider(Loc.T("Khoảng cách", "Spacing"), 0, 12, strip.Spacing, 1, "{0:0} px", v => strip.Spacing = v));
        col1.Children.Add(AppearanceSlider(Loc.T("Bề rộng", "Width"), 140, 460, strip.Width, 10, "{0:0} px", v => strip.Width = v));

        var col2 = new StackPanel();
        col2.Children.Add(Head(Loc.T("HIỂN THỊ", "PRESENTATION")));

        var variantRow = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 12) };
        var variantLabel = new TextBlock
        {
            Text = Loc.T("Kiểu nền:", "Variant:"),
            FontSize = 12,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 10, 0),
            Width = 92
        };
        variantLabel.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");

        var variantBox = new ComboBox { Width = 158, HorizontalAlignment = HorizontalAlignment.Left };
        variantBox.Items.Add("Plain");
        variantBox.Items.Add("Outlined");
        variantBox.Items.Add("Elevated");
        variantBox.SelectedIndex = 1;
        variantBox.SelectionChanged += (_, _) =>
            strip.Variant = (QuickAccessToolbarVariant)Math.Max(0, variantBox.SelectedIndex);

        variantRow.Children.Add(variantLabel);
        variantRow.Children.Add(variantBox);
        col2.Children.Add(variantRow);

        var toggles = new WrapPanel { Width = 401, HorizontalAlignment = HorizontalAlignment.Left, Margin = new Thickness(0, 0, 0, 6) };
        toggles.Children.Add(AppearanceToggle(Loc.T("Vẽ dấu phân cách", "Draw separators"), strip.ShowSeparators, v => strip.ShowSeparators = v));
        toggles.Children.Add(AppearanceToggle(Loc.T("Luôn hiện chevron", "Always show the chevron"), strip.IsCustomizable, v => strip.IsCustomizable = v));
        col2.Children.Add(toggles);

        var note = new TextBlock
        {
            Text = Loc.T(
                "Dấu phân cách chỉ vẽ khi ShowSeparators bật. Chevron hiện khi IsCustomizable bật hoặc khi có lệnh bị tràn — tắt cả hai thì dải chỉ còn đúng số icon vừa chỗ.",
                "A separator is only drawn while ShowSeparators is on. The chevron shows while IsCustomizable is on, or as soon as a command overflows — turn both off and the strip is nothing but the icons that fit."),
            FontSize = 11,
            TextWrapping = TextWrapping.Wrap,
            MaxWidth = 400,
            HorizontalAlignment = HorizontalAlignment.Left,
            LineHeight = 16
        };
        note.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");
        col2.Children.Add(note);

        var workbench = new Grid { Margin = new Thickness(0, 18, 0, 0) };
        workbench.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(410) });
        workbench.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(24) });
        workbench.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        Grid.SetColumn(col1, 0);
        Grid.SetColumn(col2, 2);
        workbench.Children.Add(col1);
        workbench.Children.Add(col2);

        var deck = new StackPanel();
        deck.Children.Add(stage);
        deck.Children.Add(workbench);
        deck.Children.Add(readout);
        deck.Children.Add(openButton);

        var root = new StackPanel();
        root.Children.Add(DemoSection(
            Loc.T("Dải lệnh nhanh", "The quick-access strip"),
            deck,
            xaml: "<controls:QuickAccessToolbar ItemsSource=\"{Binding QuickCommands}\"\n" +
                  "    Variant=\"Outlined\"\n" +
                  "    ButtonSize=\"24\"\n" +
                  "    IconSize=\"14\"\n" +
                  "    Spacing=\"2\"\n" +
                  "    ShowSeparators=\"True\"\n" +
                  "    IsCustomizable=\"True\" />",
            csharp: Loc.T("// Mỗi thuộc tính đều là DependencyProperty nên bind được\n",
                          "// Every one of these is a DependencyProperty, so all of them bind\n") +
                    "qat.Variant = QuickAccessToolbarVariant.Outlined;\n" +
                    "qat.ButtonSize = 30;\n" +
                    "qat.IconSize = 16;\n" +
                    "qat.Spacing = 4;\n" +
                    "qat.Width = 150;            " + Loc.T("// hẹp lại để thấy menu tràn\n", "// narrow it to see the overflow menu\n") +
                    "qat.ShowSeparators = true;\n" +
                    "qat.IsCustomizable = true;\n\n" +
                    Loc.T("// Chevron tự hiện khi panel báo có lệnh không vừa\n",
                          "// The chevron appears by itself once the panel reports an overflow\n") +
                    "foreach (QuickAccessItem hidden in qat.OverflowItems)\n" +
                    "    Debug.WriteLine(hidden.ToolTip);"));

        return root;
    }
}
