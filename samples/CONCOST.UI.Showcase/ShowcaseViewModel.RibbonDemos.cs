using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CONCOST.UI.Controls.Ribbon;
using CONCOST.UI.Icons;
using CONCOST.UI.Themes;

namespace CONCOST.UI.Showcase;

public partial class ShowcaseViewModel
{
    private static readonly Color RibbonInk = Color.FromRgb(0x47, 0x54, 0x67);

    private static SolidColorBrush? _ribbonIconBrush;

    /// <summary>
    /// One shared, mutable brush for every ribbon icon. Sharing it is what makes the icons follow
    /// a runtime theme switch: the images are not frozen, so changing the brush colour repaints
    /// them all at once instead of leaving dark glyphs on a dark ribbon.
    /// </summary>
    private static SolidColorBrush RibbonIconBrush
    {
        get
        {
            if (_ribbonIconBrush != null) return _ribbonIconBrush;

            _ribbonIconBrush = new SolidColorBrush(CurrentIconInk());

            ThemeManager.Current.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName is not (nameof(ThemeManager.Theme) or nameof(ThemeManager.Preset) or nameof(ThemeManager.AccentColor)))
                    return;

                // ThemeManager raises PropertyChanged before it swaps the dictionary, so read the
                // new brush on the next pass or the icons would take the old colour again.
                Application.Current?.Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Loaded,
                    new Action(() => _ribbonIconBrush.Color = CurrentIconInk()));
            };

            return _ribbonIconBrush;
        }
    }

    /// <summary>
    /// Builds an ImageSource from the library's own icon set, so the demo never uses placeholders.
    /// The ink follows the active theme, otherwise the glyphs vanish in dark mode.
    /// </summary>
    private static ImageSource RibbonIcon(IconKind kind)
    {
        Geometry? geometry = IconProvider.GetGeometry(kind);
        if (geometry == null) return new DrawingImage(new GeometryDrawing(Brushes.Transparent, null, Geometry.Empty));

        // Deliberately not frozen: a frozen drawing cannot react to the brush changing.
        return new DrawingImage(new GeometryDrawing(RibbonIconBrush, null, geometry));
    }

    /// <summary>
    /// One letter drawn as vector geometry. The library's icon set has no bold / italic /
    /// underline glyphs, and a ribbon button with neither icon nor chrome reads as a stray word
    /// rather than a command.
    /// </summary>
    private static ImageSource TextGlyphIcon(string text, FontWeight weight, FontStyle style,
        TextDecorationCollection? decorations = null)
    {
        var typeface = new Typeface(new FontFamily("Segoe UI"), style, weight, FontStretches.Normal);

        var formatted = new FormattedText(
            text, System.Globalization.CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
            typeface, 13, RibbonIconBrush, 1.0);

        if (decorations != null) formatted.SetTextDecorations(decorations);

        Geometry geometry = formatted.BuildGeometry(new Point(0, 0));
        if (geometry.Bounds.IsEmpty)
            return new DrawingImage(new GeometryDrawing(Brushes.Transparent, null, Geometry.Empty));

        // Centre the glyph inside a 16 px box so every icon in a row lines up on the same baseline.
        Rect bounds = geometry.Bounds;
        geometry.Transform = new TranslateTransform(
            8 - bounds.Left - (bounds.Width / 2),
            8 - bounds.Top - (bounds.Height / 2));

        // The transparent frame fixes the natural size at 16 × 16. Without it the drawing is exactly
        // as tall as the letter, and the Image stretches the letter itself to 16 px — which makes a
        // bold "B" tower over the 16 px icons beside it.
        var frame = new RectangleGeometry(new Rect(0, 0, 16, 16));
        frame.Freeze();
        geometry.Freeze();

        var group = new DrawingGroup();
        group.Children.Add(new GeometryDrawing(Brushes.Transparent, null, frame));
        group.Children.Add(new GeometryDrawing(RibbonIconBrush, null, geometry));

        // The group is deliberately left unfrozen: freezing it also freezes every Freezable inside
        // it, including the shared RibbonIconBrush, and the next theme change then dies with
        // "Cannot set a property ... because it is in a read-only state" when it recolours the
        // brush. Only the geometries — which never change — are frozen.
        return new DrawingImage(group);
    }

    private static Color CurrentIconInk()
    {
        if (Application.Current?.TryFindResource("CONCOST.Brush.TextPrimary") is SolidColorBrush themed)
            return themed.Color;

        return RibbonInk;
    }

    private static RibbonButton Cmd(string label, RibbonItemSize size, IconKind kind, string tip = "", string gesture = "")
        => new()
        {
            Header = label,
            Size = size,
            ScreenTip = tip,
            GestureText = gesture,
            Icon = RibbonIcon(kind)
        };

    private static RibbonMenuItem RibbonMenuEntry(string label, IconKind kind, string gesture = "")
        => new()
        {
            Header = label,
            Icon = RibbonIcon(kind),
            InputGestureText = gesture
        };

    private object CreateRibbonDemo()
    {
        // ================= card 1: base ribbon =================
        var ribbon = new Ribbon { Margin = new Thickness(0, 0, 0, 12) };

        var clipboard = new RibbonGroup { Header = "Clipboard" };
        clipboard.Items.Add(Cmd("Save", RibbonItemSize.Large, IconKind.Save, "Save the current drawing"));
        clipboard.Items.Add(Cmd("Copy", RibbonItemSize.Large, IconKind.Copy));
        clipboard.Items.Add(Cmd("Print", RibbonItemSize.Large, IconKind.Print));
        clipboard.Items.Add(new RibbonSeparator());
        clipboard.Items.Add(Cmd("Edit", RibbonItemSize.Small, IconKind.Edit));
        clipboard.Items.Add(Cmd("Link", RibbonItemSize.Small, IconKind.Link));
        clipboard.Items.Add(Cmd("File", RibbonItemSize.Small, IconKind.File));

        var paragraph = new RibbonGroup { Header = "Paragraph" };
        // Medium (two rows) plus one small fills the first column; three smalls fill the second.
        paragraph.Items.Add(Cmd("Align", RibbonItemSize.Medium, IconKind.AlignLeft));
        paragraph.Items.Add(Cmd("Grid", RibbonItemSize.Small, IconKind.GridSquare));
        paragraph.Items.Add(Cmd("Ruler", RibbonItemSize.Small, IconKind.Ruler));
        paragraph.Items.Add(Cmd("Layers", RibbonItemSize.Small, IconKind.Layers));
        paragraph.Items.Add(Cmd("Settings", RibbonItemSize.Small, IconKind.Settings));

        var insert = new RibbonGroup { Header = "Insert" };
        insert.Items.Add(Cmd("Table", RibbonItemSize.Large, IconKind.TableData, "Insert a table"));
        insert.Items.Add(Cmd("Chart", RibbonItemSize.Large, IconKind.ChartBar, "Insert a chart"));
        insert.Items.Add(Cmd("Layers", RibbonItemSize.Large, IconKind.Layers));
        insert.Items.Add(new RibbonSeparator());
        insert.Items.Add(Cmd("Link", RibbonItemSize.Small, IconKind.Link));
        insert.Items.Add(Cmd("File", RibbonItemSize.Small, IconKind.File));
        insert.Items.Add(Cmd("Search", RibbonItemSize.Small, IconKind.Search));

        var home = new RibbonTabItem { Header = "Home", Icon = RibbonIcon(IconKind.Folder) };
        home.Content = new List<RibbonGroup> { clipboard, paragraph, insert };

        var media = new RibbonGroup { Header = "Media" };
        media.Items.Add(Cmd("Chart", RibbonItemSize.Large, IconKind.ChartBar));
        media.Items.Add(Cmd("Layers", RibbonItemSize.Medium, IconKind.Layers));
        media.Items.Add(Cmd("Link", RibbonItemSize.Small, IconKind.Link));

        var data = new RibbonGroup { Header = "Data" };
        data.Items.Add(Cmd("Table", RibbonItemSize.Large, IconKind.TableData));
        data.Items.Add(Cmd("File", RibbonItemSize.Small, IconKind.File));
        data.Items.Add(Cmd("Copy", RibbonItemSize.Small, IconKind.Copy));
        data.Items.Add(Cmd("Search", RibbonItemSize.Small, IconKind.Search));

        var insertTab = new RibbonTabItem { Header = "Insert", Icon = RibbonIcon(IconKind.TableData) };
        insertTab.Content = new List<RibbonGroup> { media, data };

        var find = new RibbonGroup { Header = "Find" };
        find.Items.Add(Cmd("Search", RibbonItemSize.Large, IconKind.Search, "Find in drawing"));

        var show = new RibbonGroup { Header = "Show" };
        show.Items.Add(new RibbonToggleButton
        {
            Header = "Grid",
            Size = RibbonItemSize.Small,
            IsChecked = true,
            Icon = RibbonIcon(IconKind.GridSquare)
        });
        show.Items.Add(new RibbonToggleButton
        {
            Header = "Ruler",
            Size = RibbonItemSize.Small,
            Icon = RibbonIcon(IconKind.Ruler)
        });
        show.Items.Add(Cmd("Layers", RibbonItemSize.Small, IconKind.Layers));

        var viewTab = new RibbonTabItem { Header = "View", Icon = RibbonIcon(IconKind.GridSquare) };
        viewTab.Content = new List<RibbonGroup> { find, show };

        ribbon.ItemsSource = new List<RibbonTabItem> { home, insertTab, viewTab };

        var minimize = new Button
        {
            Content = "Thu gọn / mở rộng",
            Padding = new Thickness(12, 6, 12, 6),
            HorizontalAlignment = HorizontalAlignment.Left
        };
        minimize.SetResourceReference(FrameworkElement.StyleProperty, "CONCOST.Button.Secondary");
        minimize.Click += (_, _) => ribbon.IsMinimized = !ribbon.IsMinimized;

        var baseStack = new StackPanel();
        baseStack.Children.Add(ribbon);
        baseStack.Children.Add(minimize);

        var root = new StackPanel();
        root.Children.Add(DemoSection("Mặc định", baseStack,
            xaml: "<ribbon:Ribbon>\n" +
                  "    <ribbon:RibbonTabItem Header=\"Home\">\n" +
                  "        <ribbon:RibbonGroup Header=\"Clipboard\">\n" +
                  "            <ribbon:RibbonButton Header=\"Save\" Size=\"Large\" />\n" +
                  "            <ribbon:RibbonSeparator />\n" +
                  "            <ribbon:RibbonButton Header=\"Copy\" Size=\"Small\" />\n" +
                  "        </ui:RibbonGroup>\n" +
                  "    </ui:RibbonTabItem>\n" +
                  "</ui:Ribbon>",
            csharp: "var ribbon = new Ribbon();\n" +
                    "var home = new RibbonTabItem { Header = \"Home\" };\n" +
                    "var group = new RibbonGroup { Header = \"Clipboard\" };\n" +
                    "group.Items.Add(new RibbonButton { Header = \"Save\", Size = RibbonItemSize.Large });\n" +
                    "home.Content = new[] { group };\n" +
                    "ribbon.ItemsSource = new[] { home };"));

        // ================= card 2: dropdown, split, radio, nested menu =================
        var exportSplit = new RibbonSplitButton
        {
            Header = "Export",
            Size = RibbonItemSize.Large,
            Icon = RibbonIcon(IconKind.Save),
            ScreenTip = "Export the current drawing"
        };
        exportSplit.Items.Add(RibbonMenuEntry("PDF", IconKind.File, "Ctrl+P"));
        exportSplit.Items.Add(RibbonMenuEntry("Excel", IconKind.TableData));
        exportSplit.Items.Add(new RibbonMenuSeparator());
        exportSplit.Items.Add(RibbonMenuEntry("CSV", IconKind.File));

        var exportGroup = new RibbonGroup { Header = "Export" };
        exportGroup.Items.Add(exportSplit);
        exportGroup.Items.Add(new RibbonSeparator());
        exportGroup.Items.Add(Cmd("PDF", RibbonItemSize.Small, IconKind.File));
        exportGroup.Items.Add(Cmd("Excel", RibbonItemSize.Small, IconKind.TableData));
        exportGroup.Items.Add(Cmd("CSV", RibbonItemSize.Small, IconKind.File));

        var chartSub = new RibbonMenuItem { Header = "Chart", Icon = RibbonIcon(IconKind.ChartBar) };
        chartSub.Items.Add(RibbonMenuEntry("Bar", IconKind.ChartBar));
        chartSub.Items.Add(RibbonMenuEntry("Line", IconKind.ChartLine));

        var insertDropDown = new RibbonDropDownButton
        {
            Header = "Insert",
            Size = RibbonItemSize.Large,
            Icon = RibbonIcon(IconKind.TableData)
        };
        insertDropDown.Items.Add(RibbonMenuEntry("Table", IconKind.TableData));
        insertDropDown.Items.Add(chartSub);
        insertDropDown.Items.Add(new RibbonMenuSeparator());
        insertDropDown.Items.Add(RibbonMenuEntry("Link", IconKind.Link, "Ctrl+K"));

        var insertGroup = new RibbonGroup { Header = "Insert" };
        insertGroup.Items.Add(insertDropDown);
        insertGroup.Items.Add(new RibbonSeparator());
        insertGroup.Items.Add(Cmd("Table", RibbonItemSize.Small, IconKind.TableData));
        insertGroup.Items.Add(Cmd("Chart", RibbonItemSize.Small, IconKind.ChartBar));
        insertGroup.Items.Add(Cmd("Link", RibbonItemSize.Small, IconKind.Link));

        var units = new RibbonGroup { Header = "Units" };
        foreach (string unit in new[] { "mm", "cm", "m" })
        {
            units.Items.Add(new RibbonRadioButton
            {
                Header = unit,
                Size = RibbonItemSize.Small,
                GroupName = "unit",
                IsChecked = unit == "mm"
            });
        }
        units.Items.Add(new RibbonSeparator());
        units.Items.Add(Cmd("Settings", RibbonItemSize.Small, IconKind.Settings));
        units.Items.Add(Cmd("Search", RibbonItemSize.Small, IconKind.Search));

        var formatTab = new RibbonTabItem { Header = "Format", Icon = RibbonIcon(IconKind.Edit) };
        formatTab.Content = new List<RibbonGroup> { exportGroup, insertGroup, units };

        var advanced = new Ribbon();
        advanced.ItemsSource = new List<RibbonTabItem> { formatTab };

        var advancedStack = new StackPanel();
        advancedStack.Children.Add(advanced);

        root.Children.Add(DemoSection("Lệnh nâng cao", advancedStack,
            xaml: "<ribbon:RibbonSplitButton Header=\"Export\" Size=\"Large\">\n" +
                  "    <ribbon:RibbonMenuItem Header=\"PDF\" />\n" +
                  "    <ribbon:RibbonMenuSeparator />\n" +
                  "    <ribbon:RibbonMenuItem Header=\"CSV\" />\n" +
                  "</ui:RibbonSplitButton>\n\n" +
                  "<ribbon:RibbonDropDownButton Header=\"Insert\" Size=\"Large\">\n" +
                  "    <ribbon:RibbonMenuItem Header=\"Table\" />\n" +
                  "    <ribbon:RibbonMenuItem Header=\"Chart\">\n" +
                  "        <ribbon:RibbonMenuItem Header=\"Bar\" />\n" +
                  "    </ui:RibbonMenuItem>\n" +
                  "</ui:RibbonDropDownButton>",
            csharp: "var split = new RibbonSplitButton { Header = \"Export\" };\n" +
                    "split.Command = ExportCommand;                            // vùng trái\n" +
                    "split.Items.Add(new RibbonMenuItem { Header = \"PDF\" });  // vùng mũi tên\n\n" +
                    "var radio = new RibbonRadioButton\n" +
                    "{\n" +
                    "    Header = \"mm\",\n" +
                    "    GroupName = \"unit\",   // cùng GroupName = loại trừ lẫn nhau\n" +
                    "    IsChecked = true\n" +
                    "};"));

        root.Children.Add(BuildRibbonShellCard());
        root.Children.Add(BuildRibbonInputCard());
        root.Children.Add(BuildRibbonKeyboardCard());
        root.Children.Add(BuildRibbonExtendedCard());

        return root;
    }

    /// <summary>Card 7 — the properties a host uses to bend the ribbon to its own shell.</summary>
    private UIElement BuildRibbonExtendedCard()
    {
        var fileGroup = new RibbonGroup { Header = "File" };
        fileGroup.Items.Add(Cmd("New", RibbonItemSize.Large, IconKind.File, "Tạo bản vẽ mới", "Ctrl+N"));
        fileGroup.Items.Add(Cmd("Open", RibbonItemSize.Large, IconKind.Folder, "Mở bản vẽ", "Ctrl+O"));
        fileGroup.Items.Add(Cmd("Save", RibbonItemSize.Large, IconKind.Save, "Ghi bản vẽ", "Ctrl+S"));

        var editGroup = new RibbonGroup { Header = "Edit" };
        editGroup.Items.Add(Cmd("Undo", RibbonItemSize.Large, IconKind.Undo, "Hoàn tác", "Ctrl+Z"));
        editGroup.Items.Add(Cmd("Redo", RibbonItemSize.Large, IconKind.Redo, "Làm lại", "Ctrl+Y"));
        editGroup.Items.Add(new RibbonSeparator());
        editGroup.Items.Add(Cmd("Cut", RibbonItemSize.Small, IconKind.Close, "Cắt", "Ctrl+X"));
        editGroup.Items.Add(Cmd("Copy", RibbonItemSize.Small, IconKind.Copy, "Sao chép", "Ctrl+C"));

        var viewGroup = new RibbonGroup { Header = "Show" };
        viewGroup.Items.Add(Cmd("Grid", RibbonItemSize.Small, IconKind.GridSquare, "Hiện lưới", "F7"));
        viewGroup.Items.Add(Cmd("Ruler", RibbonItemSize.Small, IconKind.Ruler, "Hiện thước", "F8"));

        var media = new RibbonGroup { Header = "Media" };
        media.Items.Add(Cmd("Table", RibbonItemSize.Large, IconKind.TableData, "Chèn bảng"));
        media.Items.Add(Cmd("Chart", RibbonItemSize.Large, IconKind.ChartBar, "Chèn biểu đồ"));

        var home = new RibbonTabItem { Header = "Home", Id = "home", Icon = RibbonIcon(IconKind.Folder) };
        home.Content = new List<RibbonGroup> { fileGroup, editGroup };

        var insert = new RibbonTabItem { Header = "Insert", Id = "insert", Icon = RibbonIcon(IconKind.TableData) };
        insert.Content = new List<RibbonGroup> { media };

        var view = new RibbonTabItem { Header = "View", Id = "view", Icon = RibbonIcon(IconKind.GridSquare) };
        view.Content = new List<RibbonGroup> { viewGroup };

        var extended = new Ribbon
        {
            EmptyContent = "Chưa mở không gian làm việc nào.",
            ItemsSource = new object[] { home, insert, view }
        };

        var options = new WrapPanel();

        options.Children.Add(Option("Viền khung", true, on => extended.ShowBorder = on));
        options.Children.Add(Option("Nhãn group", true, on => Ribbon.SetShowGroupLabels(extended, on)));
        options.Children.Add(Option("Gộp group khi hẹp", true, on => extended.IsGroupCollapseEnabled = on));
        options.Children.Add(Option("Cho tùy biến QAT", true, on => extended.IsQuickAccessCustomizable = on));
        options.Children.Add(Option("Bật key tip (Alt)", true, on => extended.IsKeyTipEnabled = on));

        var empty = new Button
        {
            Content = "Bỏ hết tab",
            Padding = new Thickness(12, 5, 12, 5),
            Margin = new Thickness(0, 0, 16, 8)
        };
        empty.SetResourceReference(FrameworkElement.StyleProperty, "CONCOST.Button.Secondary");
        empty.Click += (_, _) =>
        {
            bool wasEmpty = extended.IsEmpty;
            extended.ItemsSource = wasEmpty ? new object[] { home, insert, view } : Array.Empty<RibbonTabItem>();
            empty.Content = extended.IsEmpty ? "Khôi phục tab" : "Bỏ hết tab";
        };
        options.Children.Add(empty);

        var tabs = new ComboBox { Width = 150, Margin = new Thickness(0, 0, 0, 8) };
        tabs.Items.Add("home");
        tabs.Items.Add("insert");
        tabs.Items.Add("view");
        tabs.SelectedIndex = 0;
        tabs.SelectionChanged += (_, _) =>
        {
            if (tabs.SelectedItem is string id) extended.SelectedTabId = id;
        };
        options.Children.Add(tabs);

        var stack = new StackPanel();
        stack.Children.Add(extended);
        stack.Children.Add(options);

        return DemoSection("Thuộc tính mở rộng", stack,
            xaml: "<ribbon:Ribbon ShowBorder=\"False\"\n" +
                  "               IsGroupCollapseEnabled=\"False\"\n" +
                  "               IsQuickAccessCustomizable=\"False\"\n" +
                  "               IsKeyTipEnabled=\"False\"\n" +
                  "               SelectedTabId=\"{Binding ActiveTabId}\"\n" +
                  "               EmptyContent=\"No workspace open.\" />\n\n" +
                  "<!-- Caption row off for every group in the ribbon -->\n" +
                  "<ribbon:Ribbon ShowGroupLabels=\"False\" />",
            csharp: "// Select by id instead of by index\n" +
                    "ribbon.SelectedTabId = \"insert\";\n\n" +
                    "// Drop the last rung: a group never becomes a single button\n" +
                    "ribbon.IsGroupCollapseEnabled = false;\n\n" +
                    "// Lock the quick access strip: pinned commands stay, the customise button goes\n" +
                    "ribbon.IsQuickAccessCustomizable = false;\n\n" +
                    "// Hand Alt back to the host (Revit / AutoCAD)\n" +
                    "ribbon.IsKeyTipEnabled = false;\n\n" +
                    "// Docked into a panel: no outline, no caption row\n" +
                    "ribbon.ShowBorder = false;\n" +
                    "Ribbon.SetShowGroupLabels(ribbon, false);\n\n" +
                    "// Empty state\n" +
                    "ribbon.EmptyContent = \"No workspace open.\";   // IsEmpty is true with no tabs");
    }

    private static CheckBox Option(string label, bool initial, Action<bool> apply)
    {
        var box = new CheckBox
        {
            Content = label,
            IsChecked = initial,
            Margin = new Thickness(0, 0, 16, 8),
            VerticalAlignment = VerticalAlignment.Center
        };

        box.Checked += (_, _) => apply(true);
        box.Unchecked += (_, _) => apply(false);

        return box;
    }

    /// <summary>Card 3 — application menu, quick-access toolbar, contextual tabs, rich tooltips.</summary>
    private UIElement BuildRibbonShellCard()
    {
        // ---- File menu (left column + recent documents on the right) ----
        var fileMenu = new RibbonApplicationMenu
        {
            Header = "File",
            RecentHeader = "Mở gần đây",
            Items =
            {
                RibbonMenuEntry("New drawing", IconKind.File, "Ctrl+N"),
                RibbonMenuEntry("Open…", IconKind.Folder, "Ctrl+O"),
                RibbonMenuEntry("Save", IconKind.Save, "Ctrl+S"),
                new RibbonMenuSeparator(),
                RibbonMenuEntry("Print", IconKind.Print, "Ctrl+P")
            },
            FooterItems = new List<object>
            {
                RibbonMenuEntry("Options", IconKind.Settings),
                RibbonMenuEntry("Exit", IconKind.Close)
            }
        };

        var recent = new List<object>();
        foreach (string name in new[] { "Tower-A.dwg", "Site-plan.dwg", "Detail-04.dwg" })
        {
            var row = new TextBlock
            {
                Text = name,
                Margin = new Thickness(0, 3, 0, 3),
                FontSize = 12
            };
            row.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextPrimary");
            recent.Add(row);
        }
        fileMenu.RecentItems = recent;

        // ---- Quick access: three commands pinned before the window is shown ----
        var save = Cmd("Save", RibbonItemSize.Large, IconKind.Save, "Ghi bản vẽ hiện tại", "Ctrl+S");
        save.Id = "save";
        var undo = Cmd("Undo", RibbonItemSize.Large, IconKind.Undo, "Hoàn tác thao tác cuối", "Ctrl+Z");
        undo.Id = "undo";
        var redo = Cmd("Redo", RibbonItemSize.Large, IconKind.Redo, "Làm lại thao tác vừa hoàn tác", "Ctrl+Y");
        redo.Id = "redo";

        // ---- Home tab: a full ribbon, not three lonely buttons ----
        var fileGroup = new RibbonGroup { Header = "File" };
        fileGroup.Items.Add(Cmd("New", RibbonItemSize.Large, IconKind.File, "Tạo bản vẽ mới", "Ctrl+N"));
        fileGroup.Items.Add(Cmd("Open", RibbonItemSize.Large, IconKind.Folder, "Mở bản vẽ có sẵn", "Ctrl+O"));
        fileGroup.Items.Add(save);

        var editGroup = new RibbonGroup { Header = "Edit" };
        editGroup.Items.Add(undo);
        editGroup.Items.Add(redo);
        editGroup.Items.Add(new RibbonSeparator());
        editGroup.Items.Add(Cmd("Cut", RibbonItemSize.Small, IconKind.Close, "Cắt", "Ctrl+X"));
        editGroup.Items.Add(Cmd("Copy", RibbonItemSize.Small, IconKind.Copy, "Sao chép", "Ctrl+C"));
        editGroup.Items.Add(Cmd("Paste", RibbonItemSize.Small, IconKind.Copy, "Dán", "Ctrl+V"));

        var insertGroup = new RibbonGroup { Header = "Insert" };
        insertGroup.Items.Add(Cmd("Table", RibbonItemSize.Large, IconKind.TableData, "Chèn bảng", "TB"));
        insertGroup.Items.Add(Cmd("Chart", RibbonItemSize.Large, IconKind.ChartBar, "Chèn biểu đồ"));
        insertGroup.Items.Add(new RibbonSeparator());
        insertGroup.Items.Add(Cmd("Link", RibbonItemSize.Small, IconKind.Link, "Chèn liên kết", "Ctrl+K"));
        insertGroup.Items.Add(Cmd("Layers", RibbonItemSize.Small, IconKind.Layers, "Quản lý lớp"));
        insertGroup.Items.Add(Cmd("Settings", RibbonItemSize.Small, IconKind.Settings, "Cấu hình"));

        var homeTab = new RibbonTabItem { Header = "Home", Icon = RibbonIcon(IconKind.Folder) };
        homeTab.Content = new List<RibbonGroup> { fileGroup, editGroup, insertGroup };

        var mediaGroup = new RibbonGroup { Header = "Media" };
        mediaGroup.Items.Add(Cmd("Chart", RibbonItemSize.Large, IconKind.ChartLine, "Chèn biểu đồ"));
        mediaGroup.Items.Add(Cmd("Layers", RibbonItemSize.Medium, IconKind.Layers, "Quản lý lớp"));
        mediaGroup.Items.Add(Cmd("Link", RibbonItemSize.Small, IconKind.Link, "Chèn liên kết"));

        var dataGroup = new RibbonGroup { Header = "Data" };
        dataGroup.Items.Add(Cmd("Table", RibbonItemSize.Large, IconKind.TableData, "Chèn bảng"));
        dataGroup.Items.Add(Cmd("File", RibbonItemSize.Small, IconKind.File, "Nhập dữ liệu"));
        dataGroup.Items.Add(Cmd("Copy", RibbonItemSize.Small, IconKind.Copy, "Sao chép dữ liệu"));
        dataGroup.Items.Add(Cmd("Search", RibbonItemSize.Small, IconKind.Search, "Tìm dữ liệu"));

        var insertTab = new RibbonTabItem { Header = "Insert", Icon = RibbonIcon(IconKind.TableData) };
        insertTab.Content = new List<RibbonGroup> { mediaGroup, dataGroup };

        var showGroup = new RibbonGroup { Header = "Show" };
        showGroup.Items.Add(new RibbonToggleButton
        {
            Header = "Grid",
            Size = RibbonItemSize.Small,
            IsChecked = true,
            Icon = RibbonIcon(IconKind.GridSquare),
            ScreenTip = "Hiện lưới",
            GestureText = "F7"
        });
        showGroup.Items.Add(new RibbonToggleButton
        {
            Header = "Ruler",
            Size = RibbonItemSize.Small,
            Icon = RibbonIcon(IconKind.Ruler),
            ScreenTip = "Hiện thước",
            GestureText = "F8"
        });
        showGroup.Items.Add(Cmd("Layers", RibbonItemSize.Small, IconKind.Layers, "Quản lý lớp"));

        var viewTab = new RibbonTabItem { Header = "View", Icon = RibbonIcon(IconKind.GridSquare) };
        viewTab.Content = new List<RibbonGroup> { showGroup };

        // ---- Contextual tab: only visible while a sketch is being edited ----
        var drawGroup = new RibbonGroup { Header = "Draw" };
        drawGroup.Items.Add(Cmd("Line", RibbonItemSize.Large, IconKind.Edit, "Vẽ đoạn thẳng", "L"));
        drawGroup.Items.Add(Cmd("Circle", RibbonItemSize.Large, IconKind.GridSquare, "Vẽ đường tròn", "C"));
        drawGroup.Items.Add(new RibbonSeparator());
        drawGroup.Items.Add(Cmd("Trim", RibbonItemSize.Small, IconKind.Close, "Cắt phần thừa", "TR"));
        drawGroup.Items.Add(Cmd("Offset", RibbonItemSize.Small, IconKind.Copy, "Tạo bản song song", "O"));

        var sketchTab = new RibbonTabItem { Header = "Sketch", Icon = RibbonIcon(IconKind.Edit) };
        sketchTab.Content = new List<RibbonGroup> { drawGroup };

        var accent = new SolidColorBrush(Color.FromRgb(0xD9, 0x3B, 0x3B));
        accent.Freeze();

        var sketch = new RibbonContextualTabGroup
        {
            Header = "Công cụ vẽ",
            Accent = accent,
            Visibility = Visibility.Collapsed
        };
        sketch.Items.Add(sketchTab);

        var shellRibbon = new Ribbon();
        shellRibbon.ItemsSource = new object[] { homeTab, insertTab, sketch, viewTab };

        shellRibbon.Loaded += (_, _) =>
        {
            shellRibbon.PinToQuickAccess(save);
            shellRibbon.PinToQuickAccess(undo);
            shellRibbon.PinToQuickAccess(redo);
        };

        // ---- QAT placement selector: demonstrates QuickAccessPlacement DP ----
        var placementLabel = new TextBlock
        {
            Text = "Vị trí Quick Access:",
            FontSize = 11.5,
            Margin = new Thickness(0, 10, 0, 4)
        };
        placementLabel.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");

        var placementBox = new ComboBox
        {
            Width = 180,
            HorizontalAlignment = HorizontalAlignment.Left
        };
        placementBox.Items.Add("Inline (HeaderRight)");
        placementBox.Items.Add("AboveRibbon");
        placementBox.Items.Add("BelowRibbon");
        placementBox.SelectedIndex = 0;
        placementBox.SelectionChanged += (_, _) =>
        {
            shellRibbon.QuickAccessPlacement = placementBox.SelectedIndex switch
            {
                1 => RibbonQuickAccessPlacement.AboveRibbon,
                2 => RibbonQuickAccessPlacement.BelowRibbon,
                _ => RibbonQuickAccessPlacement.HeaderRight
            };
        };

        // ---- Contextual tab toggle: clearly labelled so the user knows what it does ----
        var ctxLabel = new TextBlock
        {
            Text = "Tab ngữ cảnh (chỉ hiện khi đang vẽ sketch):",
            FontSize = 11.5,
            Margin = new Thickness(0, 14, 0, 4)
        };
        ctxLabel.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");

        var toggleSketch = new Button
        {
            Content = "Hiện tab Sketch",
            Padding = new Thickness(12, 6, 12, 6),
            Width = 180,
            HorizontalAlignment = HorizontalAlignment.Left
        };
        toggleSketch.SetResourceReference(FrameworkElement.StyleProperty, "CONCOST.Button.Secondary");
        bool sketchVisible = false;
        toggleSketch.Click += (_, _) =>
        {
            sketchVisible = !sketchVisible;
            sketch.Visibility = sketchVisible ? Visibility.Visible : Visibility.Collapsed;
            toggleSketch.Content = sketchVisible ? "Ẩn tab Sketch" : "Hiện tab Sketch";
        };

        // ---- MenuBar inside the ribbon chrome: placement can be Above or Below ----
        var mbFile = new RibbonMenuBarItem { Header = "File" };
        mbFile.Items.Add(RibbonMenuEntry("New", IconKind.File, "Ctrl+N"));
        mbFile.Items.Add(RibbonMenuEntry("Open…", IconKind.Folder, "Ctrl+O"));
        mbFile.Items.Add(RibbonMenuEntry("Save", IconKind.Save, "Ctrl+S"));
        mbFile.Items.Add(new RibbonMenuSeparator());
        mbFile.Items.Add(RibbonMenuEntry("Exit", IconKind.Close));

        var mbEdit = new RibbonMenuBarItem { Header = "Edit" };
        mbEdit.Items.Add(RibbonMenuEntry("Undo", IconKind.Undo, "Ctrl+Z"));
        mbEdit.Items.Add(RibbonMenuEntry("Redo", IconKind.Redo, "Ctrl+Y"));

        var mbView = new RibbonMenuBarItem { Header = "View" };
        mbView.Items.Add(RibbonMenuEntry("Grid", IconKind.GridSquare, "F7"));
        mbView.Items.Add(RibbonMenuEntry("Ruler", IconKind.Ruler, "F8"));

        var menuBar = new RibbonMenuBar();
        menuBar.Items.Add(mbFile);
        menuBar.Items.Add(mbEdit);
        menuBar.Items.Add(mbView);
        shellRibbon.MenuBar = menuBar;

        // ---- MenuBar placement selector ----
        var mbPlacementLabel = new TextBlock
        {
            Text = "Vị trí MenuBar:",
            FontSize = 11.5,
            Margin = new Thickness(0, 10, 0, 4)
        };
        mbPlacementLabel.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");

        var mbPlacementBox = new ComboBox
        {
            Width = 180,
            HorizontalAlignment = HorizontalAlignment.Left
        };
        mbPlacementBox.Items.Add("Trên ribbon (AboveRibbon)");
        mbPlacementBox.Items.Add("Dưới ribbon (BelowRibbon)");
        mbPlacementBox.SelectedIndex = 0;
        mbPlacementBox.SelectionChanged += (_, _) =>
        {
            shellRibbon.MenuBarPlacement = mbPlacementBox.SelectedIndex == 1
                ? RibbonMenuBarPlacement.BelowRibbon
                : RibbonMenuBarPlacement.AboveRibbon;
        };

        // ---- Show/hide MenuBar ----
        var mbShowCheck = new System.Windows.Controls.CheckBox
        {
            Content = "Hiện MenuBar",
            IsChecked = true,
            Margin = new Thickness(0, 6, 0, 0)
        };
        mbShowCheck.Checked += (_, _) => shellRibbon.ShowMenuBar = true;
        mbShowCheck.Unchecked += (_, _) => shellRibbon.ShowMenuBar = false;

        // ---- The hairline under the bar ----
        var mbDividerCheck = new System.Windows.Controls.CheckBox
        {
            Content = "Đường kẻ dưới MenuBar",
            IsChecked = true,
            Margin = new Thickness(0, 6, 0, 0)
        };
        mbDividerCheck.Checked += (_, _) => menuBar.ShowDivider = true;
        mbDividerCheck.Unchecked += (_, _) => menuBar.ShowDivider = false;

        var stack = new StackPanel();
        stack.Children.Add(shellRibbon);
        stack.Children.Add(mbPlacementLabel);
        stack.Children.Add(mbPlacementBox);
        stack.Children.Add(mbShowCheck);
        stack.Children.Add(mbDividerCheck);
        stack.Children.Add(placementLabel);
        stack.Children.Add(placementBox);
        stack.Children.Add(ctxLabel);
        stack.Children.Add(toggleSketch);

        return DemoSection("MenuBar, QAT, tab và tab ngữ cảnh", stack,
            xaml: "<ribbon:Ribbon ApplicationMenu=\"{StaticResource FileMenu}\">\n" +
                  "    <ribbon:Ribbon.MenuBar>\n" +
                  "        <ribbon:RibbonMenuBar>\n" +
                  "            <ribbon:RibbonMenuBarItem Header=\"File\">\n" +
                  "                <ribbon:RibbonMenuItem Header=\"New\" InputGestureText=\"Ctrl+N\" />\n" +
                  "                <ribbon:RibbonMenuItem Header=\"Open…\" InputGestureText=\"Ctrl+O\" />\n" +
                  "                <ribbon:RibbonMenuSeparator />\n" +
                  "                <ribbon:RibbonMenuItem Header=\"Exit\" />\n" +
                  "            </ribbon:RibbonMenuBarItem>\n" +
                  "            <ribbon:RibbonMenuBarItem Header=\"Edit\">\n" +
                  "                <ribbon:RibbonMenuItem Header=\"Undo\" InputGestureText=\"Ctrl+Z\" />\n" +
                  "            </ribbon:RibbonMenuBarItem>\n" +
                  "        </ribbon:RibbonMenuBar>\n" +
                  "    </ribbon:Ribbon.MenuBar>\n\n" +
                  "    <ribbon:RibbonTabItem Header=\"Home\">\n" +
                  "        <ribbon:RibbonGroup Header=\"File\">\n" +
                  "            <ribbon:RibbonButton Header=\"Save\" Size=\"Large\"\n" +
                  "                                 ScreenTip=\"Ghi bản vẽ hiện tại\" GestureText=\"Ctrl+S\" />\n" +
                  "        </ribbon:RibbonGroup>\n" +
                  "    </ribbon:RibbonTabItem>\n\n" +
                  "    <ribbon:RibbonContextualTabGroup Header=\"Công cụ vẽ\" Accent=\"#D93B3B\"\n" +
                  "                                     Visibility=\"Collapsed\">\n" +
                  "        <ribbon:RibbonTabItem Header=\"Sketch\" />\n" +
                  "    </ribbon:RibbonContextualTabGroup>\n" +
                  "</ribbon:Ribbon>",
            csharp: "// MenuBar nằm trên cùng, trên cả QAT và tabs\n" +
                    "var bar = new RibbonMenuBar();\n" +
                    "bar.Items.Add(new RibbonMenuBarItem { Header = \"File\" });\n" +
                    "ribbon.MenuBar = bar;\n" +
                    "ribbon.MenuBarPlacement = RibbonMenuBarPlacement.AboveRibbon;\n" +
                    "ribbon.ShowMenuBar = true;\n\n" +
                    "// QAT: ghim lệnh, chevron mở menu tùy biến\n" +
                    "ribbon.PinToQuickAccess(saveButton);\n" +
                    "ribbon.UnpinFromQuickAccess(saveButton);\n" +
                    "ribbon.QuickAccess.ShowLabels = true;\n\n" +
                    "// Tab ngữ cảnh: ẩn/hiện theo chế độ đang làm việc\n" +
                    "sketch.Visibility = Visibility.Visible;   // hiện cả nhóm\n" +
                    "sketch.Accent = Brushes.Crimson;          // đổi màu dải\n\n" +
                    "// Screen tip tự sinh từ Header + ScreenTip + GestureText\n" +
                    "var tip = new RibbonToolTip\n" +
                    "{\n" +
                    "    Title = \"Save\",\n" +
                    "    Description = \"Ghi bản vẽ hiện tại\",\n" +
                    "    GestureText = \"Ctrl+S\",\n" +
                    "    Footer = \"Nhấn F1 để xem trợ giúp\"\n" +
                    "};");
    }

    /// <summary>Card 4 — inputs that live on the grid: check boxes, text boxes, combo boxes, gallery.</summary>
    private UIElement BuildRibbonInputCard()
    {
        // ---- Text group: two wide combos, then the usual inline toggles ----
        var textGroup = new RibbonGroup { Header = "Text" };

        var font = new RibbonComboBox
        {
            Header = "Font",
            Size = RibbonItemSize.Small,
            ColumnSpan = 2,
            ItemsSource = new List<string> { "Calibri", "Arial", "Consolas", "Segoe UI" },
            ScreenTip = "Typeface used by the selected text"
        };
        font.SelectedItem = "Calibri";

        var size = new RibbonComboBox
        {
            Header = "Size",
            Size = RibbonItemSize.Small,
            ColumnSpan = 2,
            ItemsSource = new List<string> { "9", "10", "11", "12", "14", "18" },
            ScreenTip = "Text size in points"
        };
        size.SelectedItem = "11";

        textGroup.Items.Add(font);
        textGroup.Items.Add(size);

        // ---- Style group: B / I / U as glyph tiles. Bold and Italic share the first row and
        //      Underline takes the second one at double width, because "Underline" does not fit a
        //      single 56 px column and would trim to "Under…". ----
        var styleGroup = new RibbonGroup { Header = "Style" };

        styleGroup.Items.Add(new RibbonToggleButton
        {
            Header = "Bold",
            Icon = TextGlyphIcon("B", FontWeights.Bold, FontStyles.Normal),
            IsChecked = true,
            ScreenTip = "Bold"
        });

        styleGroup.Items.Add(new RibbonToggleButton
        {
            Header = "Italic",
            Icon = TextGlyphIcon("I", FontWeights.Normal, FontStyles.Italic),
            ScreenTip = "Italic"
        });

        styleGroup.Items.Add(new RibbonToggleButton
        {
            Header = "Underline",
            ColumnSpan = 2,
            Icon = TextGlyphIcon("U", FontWeights.Normal, FontStyles.Normal, TextDecorations.Underline),
            ScreenTip = "Underline"
        });

        // ---- Snap group: three check boxes in one column, then two numeric fields ----
        var snapGroup = new RibbonGroup { Header = "Snap" };
        snapGroup.Items.Add(new RibbonCheckBox { Header = "Snap", IsChecked = true, ScreenTip = "Snap to the nearest point" });
        snapGroup.Items.Add(new RibbonCheckBox { Header = "Ortho", ScreenTip = "Constrain to right angles" });
        snapGroup.Items.Add(new RibbonCheckBox { Header = "Polar", IsChecked = true, ScreenTip = "Constrain to polar angles" });
        snapGroup.Items.Add(new RibbonSeparator());
        snapGroup.Items.Add(new RibbonTextBox { Header = "Step", Text = "10", ColumnSpan = 2, PlaceholderText = "0" });
        snapGroup.Items.Add(new RibbonTextBox { Header = "Angle", Text = "15", ColumnSpan = 2, PlaceholderText = "0" });

        // ---- Styles group: the gallery, three columns wide ----
        // Tiles are swatches only (the caption lives in the expanded panel), so two rows fit the
        // group height instead of spilling over the bottom row.
        var gallery = new RibbonGallery
        {
            Size = RibbonItemSize.Large,
            ColumnSpan = 3,
            ItemWidth = 34,
            ItemHeight = 32,
            ScreenTip = "Pick a material for the selected walls"
        };

        var swatches = new List<RibbonGalleryItem>
        {
            Swatch("Steel", Color.FromRgb(0x5B, 0x7C, 0x99)),
            Swatch("Slate", Color.FromRgb(0x6B, 0x72, 0x80)),
            Swatch("Sand", Color.FromRgb(0xD6, 0xC3, 0xA1)),
            Swatch("Moss", Color.FromRgb(0x6B, 0x8E, 0x5A)),
            Swatch("Clay", Color.FromRgb(0xC0, 0x7A, 0x5B)),
            Swatch("Plum", Color.FromRgb(0x8A, 0x5B, 0x8A)),
            Swatch("Ink", Color.FromRgb(0x2B, 0x34, 0x40)),
            Swatch("Sky", Color.FromRgb(0x5B, 0xA3, 0xC7)),
            Swatch("Amber", Color.FromRgb(0xD9, 0xA0, 0x3B)),
            Swatch("Rose", Color.FromRgb(0xC7, 0x5B, 0x72)),
            Swatch("Jade", Color.FromRgb(0x4F, 0xA9, 0x8A)),
            Swatch("Graphite", Color.FromRgb(0x4A, 0x4F, 0x57))
        };

        gallery.ItemsSource = swatches;
        gallery.SelectedItem = swatches[0];

        var stylesGroup = new RibbonGroup { Header = "Styles" };
        stylesGroup.Items.Add(gallery);

        // No zoom group here: this card is about the fields that live on the grid, and a fifth group
        // pushed the strip 6 px past the card width, which makes the ladder collapse a whole group.
        var inputTab = new RibbonTabItem { Header = "Format", Icon = RibbonIcon(IconKind.Edit) };
        inputTab.Content = new List<RibbonGroup> { textGroup, styleGroup, snapGroup, stylesGroup };

        var ribbon = new Ribbon();
        ribbon.ItemsSource = new List<RibbonTabItem> { inputTab };

        var status = new TextBlock
        {
            Text = "Đang chọn: Steel",
            FontSize = 11.5,
            Margin = new Thickness(0, 10, 0, 0)
        };
        status.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");

        gallery.SelectionChanged += (_, _) =>
        {
            string name = gallery.SelectedItem is RibbonGalleryItem tile
                ? tile.Header
                : gallery.SelectedItem?.ToString() ?? string.Empty;

            status.Text = $"Đang chọn: {name}";
        };

        var stack = new StackPanel();
        stack.Children.Add(ribbon);
        stack.Children.Add(status);

        return DemoSection("Ô nhập liệu và gallery", stack,
            xaml: "<ribbon:RibbonGroup Header=\"Text\">\n" +
                  "    <ribbon:RibbonComboBox Header=\"Font\" ColumnSpan=\"2\"\n" +
                  "                           ItemsSource=\"{Binding Fonts}\" SelectedItem=\"Calibri\" />\n" +
                  "    <ribbon:RibbonComboBox Header=\"Size\" ItemsSource=\"{Binding Sizes}\" />\n" +
                  "</ribbon:RibbonGroup>\n\n" +
                  "<ribbon:RibbonGroup Header=\"Snap\">\n" +
                  "    <ribbon:RibbonCheckBox Header=\"Snap\" IsChecked=\"True\" />\n" +
                  "    <ribbon:RibbonCheckBox Header=\"Ortho\" />\n" +
                  "    <ribbon:RibbonTextBox Header=\"Step\" Text=\"10\" ColumnSpan=\"2\" />\n" +
                  "</ribbon:RibbonGroup>\n\n" +
                  "<ribbon:RibbonGroup Header=\"Styles\">\n" +
                  "    <ribbon:RibbonGallery ColumnSpan=\"3\" SelectedItem=\"{Binding Material}\">\n" +
                  "        <ribbon:RibbonGalleryItem Header=\"Steel\" Icon=\"{StaticResource SteelSwatch}\" />\n" +
                  "        <ribbon:RibbonGalleryItem Header=\"Sand\" Icon=\"{StaticResource SandSwatch}\" />\n" +
                  "    </ribbon:RibbonGallery>\n" +
                  "</ribbon:RibbonGroup>",
            csharp: "// ColumnSpan: số cột group mà control chiếm (mặc định 1)\n" +
                    "var font = new RibbonComboBox\n" +
                    "{\n" +
                    "    Header = \"Font\",\n" +
                    "    ColumnSpan = 2,\n" +
                    "    ItemsSource = new[] { \"Calibri\", \"Arial\" }\n" +
                    "};\n" +
                    "font.SelectedItem = \"Calibri\";\n\n" +
                    "var step = new RibbonTextBox { Header = \"Step\", Text = \"10\", ColumnSpan = 2 };\n" +
                    "var ortho = new RibbonCheckBox { Header = \"Ortho\" };   // IsChecked hai chiều\n\n" +
                    "var gallery = new RibbonGallery { ColumnSpan = 3, ItemWidth = 42, ItemHeight = 42 };\n" +
                    "gallery.ItemsSource = new[] { new RibbonGalleryItem(\"Steel\", icon) };\n" +
                    "gallery.SelectionChanged += (_, _) => Apply(gallery.SelectedItem);");
    }

    /// <summary>Builds a colour chip for the gallery demo, frozen so it can be shared safely.</summary>
    private static RibbonGalleryItem Swatch(string name, Color color)
    {
        var brush = new SolidColorBrush(color);
        brush.Freeze();

        var geometry = new RectangleGeometry(new Rect(0, 0, 22, 22), 4, 4);
        geometry.Freeze();

        var drawing = new GeometryDrawing(brush, null, geometry);
        drawing.Freeze();

        var image = new DrawingImage(drawing);
        image.Freeze();

        return new RibbonGalleryItem(name, image) { ToolTip = name, Tag = name };
    }

    /// <summary>Card 5 — Alt key tips, the colour button, the search field and state persistence.</summary>
    private UIElement BuildRibbonKeyboardCard()
    {
        var status = new TextBlock { Text = "Màu: #FFDC2626", FontSize = 11.5, Margin = new Thickness(0, 4, 0, 0) };
        status.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");

        var colour = new RibbonColorPicker
        {
            Header = "Colour",
            KeyTip = "C",
            SelectedColor = (Color)ColorConverter.ConvertFromString("#DC2626")
        };

        colour.SelectedColorChanged += (_, _) =>
            status.Text = $"Màu: {colour.SelectedColor.ToString()}";

        var clipboard = new RibbonGroup { Header = "Clipboard" };

        var paste = new RibbonButton
        {
            Header = "Paste",
            Icon = RibbonIcon(IconKind.Paste),
            KeyTip = "V",
            ScreenTip = "Paste from the clipboard"
        };
        paste.Click += (_, _) => status.Text = "Đã dán.";

        clipboard.Items.Add(paste);
        clipboard.Items.Add(new RibbonButton { Header = "Cut", Icon = RibbonIcon(IconKind.Cut), KeyTip = "X" });
        clipboard.Items.Add(new RibbonButton { Header = "Copy", Icon = RibbonIcon(IconKind.Copy), KeyTip = "C" });

        // ---- Paint group: a colour swatch, then fill and outline. The icons read as a pair: one
        //      solid square, one hollow, which is exactly the difference between the two commands. ----
        var paint = new RibbonGroup { Header = "Paint", KeyTip = "P" };
        paint.DialogLauncherCommand = new ActionCommand(() => status.Text = "Đã mở hộp thoại Paint.");
        paint.Items.Add(colour);
        paint.Items.Add(new RibbonButton { Header = "Fill", Icon = RibbonIcon(IconKind.HatchSolid), KeyTip = "F" });
        paint.Items.Add(new RibbonButton { Header = "Border", Icon = RibbonIcon(IconKind.GridSquare), KeyTip = "B" });

        var edit = new RibbonGroup { Header = "Edit" };
        edit.Items.Add(new RibbonButton { Header = "Undo", Icon = RibbonIcon(IconKind.Undo), KeyTip = "U" });
        edit.Items.Add(new RibbonButton { Header = "Redo", Icon = RibbonIcon(IconKind.Redo), KeyTip = "R" });

        // One field only: this group exists to show RibbonSearchBox, and anything added under it
        // pushes the strip past the card width, which drops the whole tab to the Small density.
        var find = new RibbonGroup { Header = "Find" };
        find.Items.Add(new RibbonSearchBox
        {
            Header = "Find",
            Size = RibbonItemSize.Large,
            Placeholder = "Tìm lệnh, màu…",
            ColumnSpan = 3
        });

        var home = new RibbonTabItem { Header = "Home", KeyTip = "H" };
        home.Content = new List<RibbonGroup> { clipboard, paint, edit, find };

        var view = new RibbonGroup { Header = "View" };
        view.Items.Add(new RibbonButton { Header = "Zoom in", Icon = RibbonIcon(IconKind.ZoomIn), KeyTip = "I" });
        view.Items.Add(new RibbonButton { Header = "Zoom out", Icon = RibbonIcon(IconKind.ZoomOut), KeyTip = "O" });

        var insert = new RibbonTabItem { Header = "Insert", KeyTip = "N", Content = new List<RibbonGroup> { view } };

        var ribbon = new Ribbon();
        ribbon.ItemsSource = new List<RibbonTabItem> { home, insert };

        var hint = new TextBlock
        {
            Text = "Nhấn Alt để hiện key tip, gõ chữ cái để chạy lệnh, Esc để thoát.",
            FontSize = 11.5,
            Margin = new Thickness(0, 10, 0, 0)
        };
        hint.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");

        var show = new Button
        {
            Content = "Hiện key tip",
            Margin = new Thickness(0, 10, 0, 0),
            Padding = new Thickness(14, 6, 14, 6),
            HorizontalAlignment = HorizontalAlignment.Left
        };
        show.SetResourceReference(FrameworkElement.StyleProperty, "CONCOST.Button.Secondary");
        show.Click += (_, _) => ribbon.ShowKeyTips();

        var stack = new StackPanel();
        stack.Children.Add(ribbon);
        stack.Children.Add(show);
        stack.Children.Add(hint);
        stack.Children.Add(status);

        return DemoSection("Bàn phím, màu và tìm kiếm", stack,
            xaml: "<ribbon:RibbonTabItem Header=\"Home\" KeyTip=\"H\">\n" +
                  "    <ribbon:RibbonGroup Header=\"Paint\" KeyTip=\"P\"\n" +
                  "                        DialogLauncherCommand=\"{Binding OpenPaintCommand}\">\n" +
                  "        <ribbon:RibbonColorPicker Header=\"Colour\" KeyTip=\"C\"\n" +
                  "                                  SelectedColor=\"#DC2626\" />\n" +
                  "        <ribbon:RibbonButton Header=\"Fill\" KeyTip=\"F\" />\n" +
                  "        <ribbon:RibbonButton Header=\"Border\" KeyTip=\"B\" />\n" +
                  "    </ribbon:RibbonGroup>\n\n" +
                  "    <ribbon:RibbonGroup Header=\"Find\">\n" +
                  "        <ribbon:RibbonSearchBox Header=\"Find\" ColumnSpan=\"3\"\n" +
                  "                                Placeholder=\"Tìm lệnh, màu…\" />\n" +
                  "    </ribbon:RibbonGroup>\n" +
                  "</ribbon:RibbonTabItem>",
            csharp: "// Key tip: bỏ trống thì ribbon tự lấy chữ cái đầu của Header\n" +
                    "var fill = new RibbonButton { Header = \"Fill\", KeyTip = \"F\" };\n" +
                    "var border = new RibbonButton { Header = \"Border\" };   // tự thành \"B\"\n\n" +
                    "var colour = new RibbonColorPicker { Header = \"Colour\", ShowAlpha = true };\n" +
                    "colour.SelectedColorChanged += (_, _) => Apply(colour.SelectedColor);\n\n" +
                    "var search = new RibbonSearchBox { ColumnSpan = 3, Placeholder = \"Tìm lệnh\" };\n" +
                    "search.SearchCommand = SearchCommand;\n\n" +
                    "// Lưu và khôi phục trạng thái người dùng\n" +
                    "RibbonStateStore.Save(ribbon.CaptureState(), \"ribbon.json\");\n" +
                    "ribbon.ApplyState(RibbonStateStore.Load(\"ribbon.json\"));");
    }

    /// <summary>A one-line command for the demo, so the launcher arrow has something to do.</summary>
    private sealed class ActionCommand : System.Windows.Input.ICommand
    {
        private readonly Action _run;

        internal ActionCommand(Action run) => _run = run;

        public event EventHandler? CanExecuteChanged { add { } remove { } }

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter) => _run();
    }
}
