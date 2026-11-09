using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using UniformGrid = System.Windows.Controls.Primitives.UniformGrid;
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
        var manager = new DockingManager { Height = 500, Variant = DockingManagerVariant.Seamless };

        var tree = new ToolWindow
        {
            Header = Loc.T("Trình duyệt dự án", "Project Browser"),
            Icon = IconKind.Layers,
            Dock = DockSide.Left,
            DockSize = 238
        };
        var treeView = new TreeView { BorderThickness = new Thickness(0) };
        foreach (var name in new[] { Loc.T("Chế độ xem", "Views"), Loc.T("Mặt bằng", "Floor Plans"), Loc.T("Tầng 1", "Level 1"), Loc.T("Tầng 2", "Level 2"), Loc.T("Mái", "Roof"), Loc.T("Mặt đứng", "Elevations"), Loc.T("Mặt cắt", "Sections"), Loc.T("Bảng thống kê", "Schedules"), Loc.T("Trang tính", "Sheets"), Loc.T("Gia đình", "Families") })
        {
            treeView.Items.Add(new TreeViewItem { Header = name, IsExpanded = true });
        }
        tree.Content = treeView;

        // A second pane on the same edge: two panels docked left are tabs of one group, the way an IDE
        // behaves, instead of each taking its own slice of the edge.
        var layers = new ToolWindow
        {
            Header = Loc.T("Bản vẽ", "Views"),
            Icon = IconKind.LayerColor,
            Dock = DockSide.Left,
            DockSize = 210
        };
        var layerList = new ListBox { BorderThickness = new Thickness(0) };
        foreach (var name in new[] { Loc.T("Mặt bằng tầng 1", "Level 1 Floor Plan"), Loc.T("Mặt đứng phía Bắc", "North Elevation"), Loc.T("Mặt cắt A-A", "Section A-A"), Loc.T("Phối cảnh 3D", "3D View") })
        {
            layerList.Items.Add(name);
        }
        layers.Content = layerList;

        var props = new ToolWindow
        {
            Header = Loc.T("Thuộc tính", "Properties"),
            Icon = IconKind.Settings,
            Dock = DockSide.Right,
            DockSize = 260
        };
        var propList = new StackPanel { Margin = new Thickness(12) };
        foreach (var (key, value) in new[]
                 {
                     (Loc.T("Danh mục", "Category"), Loc.T("Dầm kết cấu", "Structural Framing")),
                     (Loc.T("Kiểu", "Type"), "M_Beam Concrete 300x600"),
                     (Loc.T("Kích thước", "Dimensions"), "300 x 600 mm"),
                     (Loc.T("Vật liệu", "Material"), "Concrete B25"),
                     (Loc.T("Cao độ", "Elevation"), "+3.600 m"),
                     (Loc.T("Chiều dài", "Length"), "6.200 m")
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
            DockSize = 82
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
            Header = Loc.T("Mặt bằng tầng 1", "Level 1 Floor Plan"),
            Icon = IconKind.Drawing,
            Dock = DockSide.Center,
            ShowHeader = true
        };
        var drawing = new Border { CornerRadius = new CornerRadius(6), Margin = new Thickness(10) };
        drawing.SetResourceReference(Border.BackgroundProperty, "CONCOST.Brush.SurfaceSecondary");
        var plan = new Grid { ClipToBounds = true };
        plan.SetResourceReference(Panel.BackgroundProperty, "CONCOST.Brush.SurfaceSecondary");
        for (var i = 0; i < 12; i++)
        {
            var horizontal = new Border { Height = 1, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, (i - 6) * 34, 0, 0), Opacity = .5 };
            horizontal.SetResourceReference(Border.BackgroundProperty, "CONCOST.Brush.Border");
            plan.Children.Add(horizontal);
            var vertical = new Border { Width = 1, HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness((i - 6) * 48, 0, 0, 0), Opacity = .5 };
            vertical.SetResourceReference(Border.BackgroundProperty, "CONCOST.Brush.Border");
            plan.Children.Add(vertical);
        }
        var footprint = new Border { Width = 320, Height = 190, BorderThickness = new Thickness(3), BorderBrush = Brushes.SlateGray, Background = Brushes.Transparent, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
        var rooms = new Grid();
        rooms.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.3, GridUnitType.Star) });
        rooms.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        rooms.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        rooms.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        foreach (var (label, col, row) in new[] { ("LIVING", 0, 0), ("KITCHEN", 1, 0), ("BEDROOM 01", 0, 1), ("BEDROOM 02", 1, 1) })
        {
            var cell = new Border { BorderBrush = Brushes.SlateGray, BorderThickness = new Thickness(1), Child = new TextBlock { Text = label, FontSize = 9, Foreground = Brushes.SlateGray, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center } };
            Grid.SetColumn(cell, col); Grid.SetRow(cell, row); rooms.Children.Add(cell);
        }
        footprint.Child = rooms;
        plan.Children.Add(footprint);
        var levelTag = new TextBlock { Text = "LEVEL 1  ·  1:100", FontSize = 10, Margin = new Thickness(12), VerticalAlignment = VerticalAlignment.Bottom, HorizontalAlignment = HorizontalAlignment.Right };
        levelTag.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");
        plan.Children.Add(levelTag);
        drawing.Child = plan;        canvas.Content = drawing;

        // Nested groups: the left edge is split into two rows, and the top row is itself a tab group.
        // This is the shape the four-slot version could not express.
        //
        // The inner group carries no header of its own: its tab strip already names what it holds,
        // and a band above the tabs would push the panel's content down for no information. The
        // outer group keeps the header, because it is the one that reads as "the left edge".
        var leftTabs = new DockPaneGroup
        {
            Mode = DockGroupMode.Tabs,
            ShowHeader = false,
            HeaderHeight = 0
        };
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
            Mode = DockGroupMode.Split,
            Header = Loc.T("Trình duyệt dự án", "Project Browser"),
            Icon = IconKind.Layers
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

        var autoHide = new Button { Content = Loc.T("Ghim / tự ẩn Properties", "Pin / auto-hide Properties"), Padding = new Thickness(12, 5, 12, 5), Margin = new Thickness(0, 10, 0, 0) };
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
            Loc.T("Không gian làm việc kiểu Revit", "Revit-style project workspace"),
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
        var root = new StackPanel();

        // 100% English command set for QuickAccessToolbar per AGENTS.md
        QuickAccessItem[] CreateCommands() => new[]
        {
            new QuickAccessItem(IconKind.Save, "Save (Ctrl+S)", new RelayCommand(() => ToastService.Success("File saved"))),
            new QuickAccessItem(IconKind.Undo, "Undo (Ctrl+Z)", new RelayCommand(() => ToastService.Info("Undo action"))),
            new QuickAccessItem(IconKind.Redo, "Redo (Ctrl+Y)", new RelayCommand(() => ToastService.Info("Redo action"))),
            new QuickAccessItem { IsSeparator = true },
            new QuickAccessItem(IconKind.Print, "Plot / Print (Ctrl+P)", new RelayCommand(() => ToastService.Info("Plot / Print"))),
            new QuickAccessItem(IconKind.ZoomIn, "Zoom In (+)", new RelayCommand(() => ToastService.Info("Zoom In"))),
            new QuickAccessItem(IconKind.ZoomOut, "Zoom Out (-)", new RelayCommand(() => ToastService.Info("Zoom Out"))),
            new QuickAccessItem { IsSeparator = true },
            new QuickAccessItem(IconKind.Settings, "CAD Options & Settings", new RelayCommand(() => ToastService.Info("Settings")))
        };

        static TextBlock SectionSubhead(string text)
        {
            var tb = new TextBlock
            {
                Text = text,
                FontSize = 11,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 8)
            };
            tb.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");
            return tb;
        }

        // =====================================================================
        // CARD 1: QUICKACCESSTOOLBAR HERO WORKBENCH
        // =====================================================================
        var liveStrip = new QuickAccessToolbar
        {
            ItemsSource = CreateCommands(),
            Variant = QuickAccessToolbarVariant.Plain,
            ButtonSize = 28,
            IconSize = 16,
            Spacing = 3,
            Width = 380,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            OverflowToolTip = "Customize Quick Access Toolbar"
        };

        // Dedicated Hero Stage: The QuickAccessToolbar is front and center as the MAIN control!
        var stage = new Border
        {
            Height = 76,
            CornerRadius = new CornerRadius(8),
            BorderThickness = new Thickness(1),
            Padding = new Thickness(24, 0, 24, 0),
            Margin = new Thickness(0, 0, 0, 16)
        };
        stage.SetResourceReference(Border.BackgroundProperty, "CONCOST.Brush.Surface");
        stage.SetResourceReference(Border.BorderBrushProperty, "CONCOST.Brush.Border");
        stage.Child = liveStrip;

        // Control Center (2-Column OCD-friendly layout)
        var controlGrid = new Grid();
        controlGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        controlGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Pixel) });
        controlGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        var vDivider = new Border { Width = 1, Margin = new Thickness(20, 0, 20, 0) };
        vDivider.SetResourceReference(Border.BackgroundProperty, "CONCOST.Brush.Border");
        Grid.SetColumn(vDivider, 1);
        controlGrid.Children.Add(vDivider);

        // ---- Left Column: Geometry & Variant ----
        var leftCol = new StackPanel();

        // 1. Variant Pills
        leftCol.Children.Add(SectionSubhead("TOOLBAR VARIANT (DEFAULT: PLAIN)"));
        var variantPills = new UniformGrid { Columns = 3, Margin = new Thickness(0, 0, 0, 14) };
        var variantButtons = new Dictionary<QuickAccessToolbarVariant, Button>();

        void UpdateVariantPills(QuickAccessToolbarVariant active)
        {
            foreach (var kvp in variantButtons)
            {
                bool isCur = kvp.Key == active;
                kvp.Value.SetResourceReference(FrameworkElement.StyleProperty, isCur ? "CONCOST.Button.Primary" : "CONCOST.Button.Secondary");
            }
        }

        Button CreateVariantPill(string label, QuickAccessToolbarVariant v)
        {
            var btn = new Button { Content = label, Height = 28, FontSize = 11 };
            btn.Click += (_, _) =>
            {
                liveStrip.Variant = v;
                UpdateVariantPills(v);
            };
            variantButtons[v] = btn;
            return btn;
        }

        variantPills.Children.Add(CreateVariantPill("Plain (Default)", QuickAccessToolbarVariant.Plain));
        variantPills.Children.Add(CreateVariantPill("Outlined", QuickAccessToolbarVariant.Outlined));
        variantPills.Children.Add(CreateVariantPill("Elevated", QuickAccessToolbarVariant.Elevated));
        leftCol.Children.Add(variantPills);
        UpdateVariantPills(liveStrip.Variant);

        // 2. Button Size Pills
        leftCol.Children.Add(SectionSubhead("BUTTON & ICON METRICS"));
        var sizePills = new UniformGrid { Columns = 3, Margin = new Thickness(0, 0, 0, 14) };
        var sizeButtons = new Dictionary<double, Button>();

        void UpdateSizePills(double activeSize)
        {
            foreach (var kvp in sizeButtons)
            {
                bool isCur = Math.Abs(kvp.Key - activeSize) < 0.5;
                kvp.Value.SetResourceReference(FrameworkElement.StyleProperty, isCur ? "CONCOST.Button.Primary" : "CONCOST.Button.Secondary");
            }
        }

        Button CreateSizePill(string label, double btnSize, double icnSize)
        {
            var btn = new Button { Content = label, Height = 28, FontSize = 11 };
            btn.Click += (_, _) =>
            {
                liveStrip.ButtonSize = btnSize;
                liveStrip.IconSize = icnSize;
                UpdateSizePills(btnSize);
            };
            sizeButtons[btnSize] = btn;
            return btn;
        }

        sizePills.Children.Add(CreateSizePill("Compact (22/12px)", 22, 12));
        sizePills.Children.Add(CreateSizePill("Standard (28/16px)", 28, 16));
        sizePills.Children.Add(CreateSizePill("Large (34/20px)", 34, 20));
        leftCol.Children.Add(sizePills);
        UpdateSizePills(liveStrip.ButtonSize);

        // 3. Responsive Width Slider
        leftCol.Children.Add(SectionSubhead("RESPONSIVE WIDTH (OVERFLOW TEST)"));
        leftCol.Children.Add(AppearanceSlider("Toolbar Width", 140, 480, liveStrip.Width, 10, "{0:0} px", v => liveStrip.Width = v));

        Grid.SetColumn(leftCol, 0);
        controlGrid.Children.Add(leftCol);

        // ---- Right Column: Behavior & Live Monitor ----
        var rightCol = new StackPanel();

        rightCol.Children.Add(SectionSubhead("BEHAVIOR & FEATURE FLAGS"));
        var featureGrid = new UniformGrid { Columns = 2, Margin = new Thickness(0, 0, 0, 14) };

        var sepCheck = new CheckBox { Content = "Show Separators", IsChecked = liveStrip.ShowSeparators, Height = 28 };
        sepCheck.Checked += (_, _) => liveStrip.ShowSeparators = true;
        sepCheck.Unchecked += (_, _) => liveStrip.ShowSeparators = false;
        featureGrid.Children.Add(sepCheck);

        var customCheck = new CheckBox { Content = "Customizable Chevron", IsChecked = liveStrip.IsCustomizable, Height = 28 };
        customCheck.Checked += (_, _) => liveStrip.IsCustomizable = true;
        customCheck.Unchecked += (_, _) => liveStrip.IsCustomizable = false;
        featureGrid.Children.Add(customCheck);
        rightCol.Children.Add(featureGrid);

        rightCol.Children.Add(SectionSubhead("TOOLBAR ACTIONS"));
        var actionRow = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 14) };
        var toggleMenuBtn = new Button { Content = "Toggle Overflow Menu", Height = 28, Padding = new Thickness(12, 0, 12, 0), Margin = new Thickness(0, 0, 8, 0) };
        toggleMenuBtn.SetResourceReference(FrameworkElement.StyleProperty, "CONCOST.Button.Secondary");
        toggleMenuBtn.Click += (_, _) => liveStrip.IsOverflowOpen = !liveStrip.IsOverflowOpen;
        actionRow.Children.Add(toggleMenuBtn);

        var resetBtn = new Button { Content = "Reset Defaults", Height = 28, Padding = new Thickness(12, 0, 12, 0) };
        resetBtn.SetResourceReference(FrameworkElement.StyleProperty, "CONCOST.Button.Secondary");
        resetBtn.Click += (_, _) =>
        {
            liveStrip.Width = 380;
            liveStrip.Variant = QuickAccessToolbarVariant.Plain;
            liveStrip.ButtonSize = 28;
            liveStrip.IconSize = 16;
            liveStrip.ShowSeparators = true;
            liveStrip.IsCustomizable = true;
            sepCheck.IsChecked = true;
            customCheck.IsChecked = true;
            UpdateVariantPills(QuickAccessToolbarVariant.Plain);
            UpdateSizePills(28);
        };
        actionRow.Children.Add(resetBtn);
        rightCol.Children.Add(actionRow);

        // Real-time Overflow Status Monitor
        rightCol.Children.Add(SectionSubhead("LIVE OVERFLOW MONITOR"));
        var statusCard = new Border
        {
            CornerRadius = new CornerRadius(6),
            BorderThickness = new Thickness(1),
            Padding = new Thickness(12, 8, 12, 8)
        };
        statusCard.SetResourceReference(Border.BackgroundProperty, "CONCOST.Brush.SurfaceSecondary");
        statusCard.SetResourceReference(Border.BorderBrushProperty, "CONCOST.Brush.Border");

        var statusText = new TextBlock
        {
            FontSize = 11,
            TextWrapping = TextWrapping.Wrap,
            LineHeight = 16
        };
        statusText.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");
        statusCard.Child = statusText;

        liveStrip.LayoutUpdated += (_, _) =>
        {
            var hidden = liveStrip.OverflowItems.Cast<object>()
                .OfType<QuickAccessItem>()
                .Where(item => !item.IsSeparator && !string.IsNullOrEmpty(item.ToolTip))
                .Select(item => item.ToolTip)
                .ToList();

            statusText.Text = hidden.Count > 0
                ? $"Overflow Active ({hidden.Count} commands hidden): {string.Join(" · ", hidden)}"
                : "All 8 commands fit comfortably on the toolbar. Drag width slider left to test responsive overflow.";
        };
        rightCol.Children.Add(statusCard);

        Grid.SetColumn(rightCol, 2);
        controlGrid.Children.Add(rightCol);

        var card1Stack = new StackPanel();
        card1Stack.Children.Add(stage);
        card1Stack.Children.Add(controlGrid);

        root.Children.Add(DemoSection(
            "QuickAccessToolbar",
            card1Stack,
            xaml: "<controls:QuickAccessToolbar ItemsSource=\"{Binding QuickCommands}\"\n" +
                  "    Variant=\"Plain\"\n" +
                  "    ButtonSize=\"28\"\n" +
                  "    IconSize=\"16\"\n" +
                  "    Spacing=\"3\"\n" +
                  "    ShowSeparators=\"True\"\n" +
                  "    IsCustomizable=\"True\"\n" +
                  "    OverflowToolTip=\"Customize Quick Access Toolbar\" />",
            csharp: "// Configure QuickAccessToolbar in code-behind / ViewModel\n" +
                    "var qat = new QuickAccessToolbar\n" +
                    "{\n" +
                    "    ItemsSource = new[]\n" +
                    "    {\n" +
                    "        new QuickAccessItem(IconKind.Save, \"Save (Ctrl+S)\", SaveCmd),\n" +
                    "        new QuickAccessItem(IconKind.Undo, \"Undo (Ctrl+Z)\", UndoCmd),\n" +
                    "        new QuickAccessItem(IconKind.Redo, \"Redo (Ctrl+Y)\", RedoCmd),\n" +
                    "        new QuickAccessItem { IsSeparator = true },\n" +
                    "        new QuickAccessItem(IconKind.Print, \"Plot / Print (Ctrl+P)\", PrintCmd),\n" +
                    "        new QuickAccessItem(IconKind.Settings, \"Settings\", SettingsCmd)\n" +
                    "    },\n" +
                    "    Variant = QuickAccessToolbarVariant.Plain, // Plain is default\n" +
                    "    ButtonSize = 28,\n" +
                    "    IconSize = 16,\n" +
                    "    ShowSeparators = true,\n" +
                    "    IsCustomizable = true\n" +
                    "};"));

        return root;
    }
}
