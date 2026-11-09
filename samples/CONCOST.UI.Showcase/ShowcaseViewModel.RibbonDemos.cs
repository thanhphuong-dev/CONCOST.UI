using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using CONCOST.UI.Controls;
using CONCOST.UI.Controls.Ribbon;
using CONCOST.UI.Core.MVVM;
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

                Application.Current?.Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Loaded,
                    new Action(() => _ribbonIconBrush.Color = CurrentIconInk()));
            };

            return _ribbonIconBrush;
        }
    }

    /// <summary>
    /// Builds an ImageSource from the library's own icon set, so the demo never uses placeholders.
    /// </summary>
    private static ImageSource RibbonIcon(IconKind kind)
    {
        Geometry? geometry = IconProvider.GetGeometry(kind);
        if (geometry == null) return new DrawingImage(new GeometryDrawing(Brushes.Transparent, null, Geometry.Empty));

        return new DrawingImage(new GeometryDrawing(RibbonIconBrush, null, geometry));
    }

    /// <summary>
    /// One letter drawn as vector geometry for typography toggles.
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

        Rect bounds = geometry.Bounds;
        geometry.Transform = new TranslateTransform(
            8 - bounds.Left - (bounds.Width / 2),
            8 - bounds.Top - (bounds.Height / 2));

        var frame = new RectangleGeometry(new Rect(0, 0, 16, 16));
        frame.Freeze();
        geometry.Freeze();

        var group = new DrawingGroup();
        group.Children.Add(new GeometryDrawing(Brushes.Transparent, null, frame));
        group.Children.Add(new GeometryDrawing(RibbonIconBrush, null, geometry));

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

    private static Border CreateRecentDocCard(string filename, string path, string meta, IconKind icon, Color? accentColor = null, Action? onClick = null)
    {
        var accent = accentColor ?? (filename.EndsWith(".rvt", StringComparison.OrdinalIgnoreCase)
            ? Color.FromRgb(0x02, 0x84, 0xC7)  // Revit Blue
            : filename.EndsWith(".dwg", StringComparison.OrdinalIgnoreCase)
                ? Color.FromRgb(0xDC, 0x26, 0x26)  // AutoCAD Red
                : Color.FromRgb(0x47, 0x55, 0x69)); // Neutral CAD

        var card = new Border
        {
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(8, 6, 8, 6),
            Margin = new Thickness(0, 0, 0, 3),
            Background = Brushes.Transparent,
            BorderThickness = new Thickness(1),
            BorderBrush = Brushes.Transparent,
            Cursor = Cursors.Hand
        };

        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        // CAD/BIM File type badge with refined soft tint
        var iconHost = new Border
        {
            Width = 30,
            Height = 30,
            CornerRadius = new CornerRadius(5),
            Margin = new Thickness(0, 0, 10, 0),
            VerticalAlignment = VerticalAlignment.Center,
            Background = new SolidColorBrush(Color.FromArgb(0x18, accent.R, accent.G, accent.B)),
            BorderThickness = new Thickness(1),
            BorderBrush = new SolidColorBrush(Color.FromArgb(0x35, accent.R, accent.G, accent.B))
        };
        var glyph = new Icon
        {
            Kind = icon,
            Width = 14,
            Height = 14,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
            Foreground = new SolidColorBrush(accent)
        };
        iconHost.Child = glyph;
        Grid.SetColumn(iconHost, 0);
        grid.Children.Add(iconHost);

        var textPanel = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
        var nameText = new TextBlock
        {
            Text = filename,
            FontSize = 11.5,
            FontWeight = FontWeights.SemiBold
        };
        nameText.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextPrimary");
        var pathText = new TextBlock
        {
            Text = $"{path} • {meta}",
            FontSize = 10,
            Margin = new Thickness(0, 1.5, 0, 0)
        };
        pathText.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");

        textPanel.Children.Add(nameText);
        textPanel.Children.Add(pathText);
        Grid.SetColumn(textPanel, 1);
        grid.Children.Add(textPanel);

        // Revit-standard pushpin for pinning frequent CAD/BIM files
        var pinIcon = new Icon
        {
            Kind = IconKind.PinItem,
            Width = 12,
            Height = 12,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Right,
            Opacity = 0.2,
            Margin = new Thickness(10, 0, 2, 0)
        };
        pinIcon.SetResourceReference(Icon.ForegroundProperty, "CONCOST.Brush.TextSecondary");
        Grid.SetColumn(pinIcon, 2);
        grid.Children.Add(pinIcon);

        card.MouseEnter += (_, _) =>
        {
            card.SetResourceReference(Border.BackgroundProperty, "CONCOST.Brush.SurfaceHover");
            card.SetResourceReference(Border.BorderBrushProperty, "CONCOST.Brush.Border");
            nameText.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.Primary");
            pinIcon.Opacity = 0.8;
        };
        card.MouseLeave += (_, _) =>
        {
            card.Background = Brushes.Transparent;
            card.BorderBrush = Brushes.Transparent;
            nameText.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextPrimary");
            pinIcon.Opacity = 0.2;
        };
        if (onClick != null) card.MouseDown += (_, _) => onClick();

        card.Child = grid;
        return card;
    }

    /// <summary>
    /// Master Ribbon Showcase: Professional CAD/BIM Application Studio presentation accompanied
    /// by an intuitive, modern UI/UX property and style control center.
    /// </summary>
    private object CreateRibbonDemo()
    {
        // -------------------------------------------------------------
        // 1. LIVE WORKSPACE HUD STATUS FEEDBACK
        // -------------------------------------------------------------
        var statusAction = new TextBlock
        {
            Text = "Command: Ready",
            FontSize = 11.5,
            FontWeight = FontWeights.SemiBold,
            VerticalAlignment = VerticalAlignment.Center
        };
        statusAction.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextPrimary");

        var statusTab = new TextBlock
        {
            Text = "Tab: Modify | Create Sketch",
            FontSize = 11,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 16, 0)
        };
        statusTab.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");

        var statusTool = new TextBlock
        {
            Text = "Tool: Draw Line (L)",
            FontSize = 11,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 16, 0)
        };
        statusTool.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");

        var modeBadge = new Border
        {
            CornerRadius = new CornerRadius(3),
            Padding = new Thickness(6, 2, 6, 2),
            VerticalAlignment = VerticalAlignment.Center
        };
        modeBadge.SetResourceReference(Border.BackgroundProperty, "CONCOST.Brush.PrimarySoftLight");
        modeBadge.SetResourceReference(Border.BorderBrushProperty, "CONCOST.Brush.PrimarySoftBorder");
        modeBadge.BorderThickness = new Thickness(1);

        var modeBadgeText = new TextBlock
        {
            Text = "2D SKETCH MODE: ACTIVE",
            FontSize = 10,
            FontWeight = FontWeights.Bold
        };
        modeBadgeText.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.PrimarySoftForeground");
        modeBadge.Child = modeBadgeText;

        void Notify(string actionName)
        {
            statusAction.Text = $"Command: {actionName}";
        }

        RibbonButton ActionCmd(string label, RibbonItemSize size, IconKind kind, string tip = "", string gesture = "")
        {
            var btn = Cmd(label, size, kind, tip, gesture);
            btn.Click += (_, _) =>
            {
                Notify(label + (string.IsNullOrEmpty(gesture) ? "" : $" [{gesture}]"));
                statusTool.Text = $"Tool: {label}";
            };
            return btn;
        }

        // -------------------------------------------------------------
        // 2. BUILD THE COMPREHENSIVE MASTER RIBBON
        // -------------------------------------------------------------

        // ---- Application Menu (Revit / Office File Block) ----
        var fileMenu = new RibbonApplicationMenu
        {
            Header = "File",
            RecentHeader = Loc.T("Mô hình & Dự án gần đây", "Recent Models & Projects"),
            Items =
            {
                RibbonMenuEntry(Loc.T("Dự án mới…", "New Project…"), IconKind.File, "Ctrl+N"),
                RibbonMenuEntry(Loc.T("Mở mô hình…", "Open Model…"), IconKind.Folder, "Ctrl+O"),
                RibbonMenuEntry(Loc.T("Lưu bản vẽ", "Save Drawing"), IconKind.Save, "Ctrl+S"),
                RibbonMenuEntry(Loc.T("Lưu thành…", "Save As…"), IconKind.Save),
                new RibbonMenuSeparator(),
                RibbonMenuEntry(Loc.T("In ấn / Xuất bản…", "Plot / Print…"), IconKind.Print, "Ctrl+P"),
                RibbonMenuEntry(Loc.T("Xuất CAD/BIM…", "Export CAD/BIM…"), IconKind.Save)
            },
            RecentItems = new List<object>
            {
                CreateRecentDocCard("Tower-A_Structural_Revit.rvt", "Projects/HighRise_2026", "4.8 MB · 12m ago", IconKind.Cube, Color.FromRgb(0x02, 0x84, 0xC7), () => Notify("Open Tower-A_Structural_Revit.rvt")),
                CreateRecentDocCard("Architectural_FloorPlan_L02.dwg", "Drawings/Phase2/Architectural", "12.4 MB · 2h ago", IconKind.File, Color.FromRgb(0xDC, 0x26, 0x26), () => Notify("Open Architectural_FloorPlan_L02.dwg")),
                CreateRecentDocCard("Foundation_Detail_Section_04.dwg", "Drawings/Substructure", "6.1 MB · Yesterday", IconKind.Layers, Color.FromRgb(0xD9, 0x77, 0x06), () => Notify("Open Foundation_Detail_Section_04.dwg")),
                CreateRecentDocCard("Site_Landscape_MasterPlan.dwg", "MasterPlans/Civil", "18.9 MB · 3 days ago", IconKind.GridSquare, Color.FromRgb(0x05, 0x96, 0x69), () => Notify("Open Site_Landscape_MasterPlan.dwg"))
            },
            FooterItems = new List<object>
            {
                RibbonMenuEntry(Loc.T("Tùy chọn & Cài đặt", "Options & Settings"), IconKind.Settings),
                RibbonMenuEntry(Loc.T("Thoát", "Exit"), IconKind.Close)
            }
        };

        // ---- MenuBar (File, Edit, View, Tools, Help) ----
        var mbFile = new RibbonMenuBarItem { Header = "File" };
        mbFile.Items.Add(RibbonMenuEntry("New Project", IconKind.File, "Ctrl+N"));
        mbFile.Items.Add(RibbonMenuEntry("Open Model…", IconKind.Folder, "Ctrl+O"));
        mbFile.Items.Add(RibbonMenuEntry("Save", IconKind.Save, "Ctrl+S"));
        mbFile.Items.Add(new RibbonMenuSeparator());
        mbFile.Items.Add(RibbonMenuEntry("Exit", IconKind.Close));

        var mbEdit = new RibbonMenuBarItem { Header = "Edit" };
        mbEdit.Items.Add(RibbonMenuEntry("Undo", IconKind.Undo, "Ctrl+Z"));
        mbEdit.Items.Add(RibbonMenuEntry("Redo", IconKind.Redo, "Ctrl+Y"));
        mbEdit.Items.Add(new RibbonMenuSeparator());
        mbEdit.Items.Add(RibbonMenuEntry("Cut", IconKind.Close, "Ctrl+X"));
        mbEdit.Items.Add(RibbonMenuEntry("Copy", IconKind.Copy, "Ctrl+C"));
        mbEdit.Items.Add(RibbonMenuEntry("Paste", IconKind.Copy, "Ctrl+V"));

        var mbView = new RibbonMenuBarItem { Header = "View" };
        mbView.Items.Add(RibbonMenuEntry("Grid", IconKind.GridSquare, "F7"));
        mbView.Items.Add(RibbonMenuEntry("Ruler", IconKind.Ruler, "F8"));
        mbView.Items.Add(RibbonMenuEntry("Layers Manager", IconKind.Layers));

        var mbTools = new RibbonMenuBarItem { Header = "Tools" };
        mbTools.Items.Add(RibbonMenuEntry("Drafting Settings", IconKind.Settings));
        mbTools.Items.Add(RibbonMenuEntry("Material Palette", IconKind.TableData));

        var mbHelp = new RibbonMenuBarItem { Header = "Help" };
        mbHelp.Items.Add(RibbonMenuEntry("Documentation", IconKind.File));
        mbHelp.Items.Add(RibbonMenuEntry("About CONCOST.UI", IconKind.Check));

        var menuBar = new RibbonMenuBar();
        menuBar.Items.Add(mbFile);
        menuBar.Items.Add(mbEdit);
        menuBar.Items.Add(mbView);
        menuBar.Items.Add(mbTools);
        menuBar.Items.Add(mbHelp);

        // ---- Quick Access Toolbar Commands ----
        var qatSave = ActionCmd("Save", RibbonItemSize.Large, IconKind.Save, "Save current drawing", "Ctrl+S");
        qatSave.Id = "qat-save";
        var qatUndo = ActionCmd("Undo", RibbonItemSize.Large, IconKind.Undo, "Undo last action", "Ctrl+Z");
        qatUndo.Id = "qat-undo";
        var qatRedo = ActionCmd("Redo", RibbonItemSize.Large, IconKind.Redo, "Redo last action", "Ctrl+Y");
        qatRedo.Id = "qat-redo";
        var qatPrint = ActionCmd("Print", RibbonItemSize.Large, IconKind.Print, "Print drawing", "Ctrl+P");
        qatPrint.Id = "qat-print";

        // ---- Tab 1: Home ----
        var clipboardGroup = new RibbonGroup { Header = "Clipboard", Icon = RibbonIcon(IconKind.Copy) };
        var pasteSplit = new RibbonSplitButton
        {
            Header = "Paste",
            Size = RibbonItemSize.Large,
            Icon = RibbonIcon(IconKind.Copy),
            ScreenTip = "Paste clipboard contents"
        };
        pasteSplit.Items.Add(RibbonMenuEntry("Paste", IconKind.Copy, "Ctrl+V"));
        pasteSplit.Items.Add(RibbonMenuEntry("Paste as Block", IconKind.Layers));
        pasteSplit.Items.Add(new RibbonMenuSeparator());
        pasteSplit.Items.Add(RibbonMenuEntry("Paste Special…", IconKind.File));
        clipboardGroup.Items.Add(pasteSplit);
        clipboardGroup.Items.Add(ActionCmd("Cut", RibbonItemSize.Small, IconKind.Close, "Cut selection", "Ctrl+X"));
        clipboardGroup.Items.Add(ActionCmd("Copy", RibbonItemSize.Small, IconKind.Copy, "Copy selection", "Ctrl+C"));
        clipboardGroup.Items.Add(ActionCmd("Format", RibbonItemSize.Small, IconKind.Edit, "Format painter"));

        var drawGroup = new RibbonGroup { Header = "Draw", Icon = RibbonIcon(IconKind.Edit) };
        drawGroup.Items.Add(ActionCmd("Line", RibbonItemSize.Large, IconKind.Edit, "Draw 2D line", "L"));
        var circleDropDown = new RibbonDropDownButton
        {
            Header = "Circle",
            Size = RibbonItemSize.Large,
            Icon = RibbonIcon(IconKind.GridSquare)
        };
        circleDropDown.Items.Add(RibbonMenuEntry("Center, Radius", IconKind.GridSquare, "C"));
        circleDropDown.Items.Add(RibbonMenuEntry("Center, Diameter", IconKind.GridSquare));
        circleDropDown.Items.Add(RibbonMenuEntry("3 Points", IconKind.GridSquare));
        drawGroup.Items.Add(circleDropDown);
        drawGroup.Items.Add(ActionCmd("Arc", RibbonItemSize.Large, IconKind.ChartLine, "Draw arc", "A"));
        drawGroup.Items.Add(new RibbonSeparator());
        drawGroup.Items.Add(ActionCmd("Rect", RibbonItemSize.Small, IconKind.GridSquare, "Draw rectangle", "REC"));
        drawGroup.Items.Add(ActionCmd("Polygon", RibbonItemSize.Small, IconKind.Layers, "Draw polygon", "POL"));
        drawGroup.Items.Add(ActionCmd("Hatch", RibbonItemSize.Small, IconKind.HatchSolid, "Fill hatch", "H"));

        var modifyGroup = new RibbonGroup { Header = "Modify", Icon = RibbonIcon(IconKind.Ruler) };
        modifyGroup.Items.Add(ActionCmd("Move", RibbonItemSize.Small, IconKind.Ruler, "Move objects", "M"));
        modifyGroup.Items.Add(ActionCmd("Rotate", RibbonItemSize.Small, IconKind.Refresh, "Rotate objects", "RO"));
        modifyGroup.Items.Add(ActionCmd("Trim", RibbonItemSize.Small, IconKind.Close, "Trim objects", "TR"));
        modifyGroup.Items.Add(ActionCmd("Offset", RibbonItemSize.Small, IconKind.Copy, "Parallel offset", "O"));
        modifyGroup.Items.Add(ActionCmd("Mirror", RibbonItemSize.Small, IconKind.AlignLeft, "Mirror objects", "MI"));
        modifyGroup.Items.Add(ActionCmd("Scale", RibbonItemSize.Small, IconKind.ZoomIn, "Scale objects", "SC"));

        var historyGroup = new RibbonGroup { Header = "History", Icon = RibbonIcon(IconKind.Undo) };
        historyGroup.Items.Add(qatUndo);
        historyGroup.Items.Add(qatRedo);
        historyGroup.Items.Add(qatSave);

        var homeTab = new RibbonTabItem
        {
            Header = "Home",
            Id = "home",
            Icon = RibbonIcon(IconKind.Folder),
            Content = new List<RibbonGroup> { clipboardGroup, drawGroup, modifyGroup, historyGroup }
        };

        // ---- Tab 2: Insert ----
        var dataGroup = new RibbonGroup { Header = "Data & Media", Icon = RibbonIcon(IconKind.TableData) };
        dataGroup.Items.Add(ActionCmd("Table", RibbonItemSize.Large, IconKind.TableData, "Insert schedule table"));
        dataGroup.Items.Add(ActionCmd("Chart", RibbonItemSize.Large, IconKind.ChartBar, "Insert progress chart"));
        var exportSplit = new RibbonSplitButton
        {
            Header = "Export",
            Size = RibbonItemSize.Large,
            Icon = RibbonIcon(IconKind.Save),
            ScreenTip = "Export current sheet"
        };
        exportSplit.Items.Add(RibbonMenuEntry("PDF Document", IconKind.File, "Ctrl+P"));
        exportSplit.Items.Add(RibbonMenuEntry("Excel Spreadsheet", IconKind.TableData));
        exportSplit.Items.Add(RibbonMenuEntry("CSV Data", IconKind.File));
        dataGroup.Items.Add(exportSplit);
        dataGroup.Items.Add(new RibbonSeparator());
        dataGroup.Items.Add(ActionCmd("Link", RibbonItemSize.Small, IconKind.Link, "Link external model", "Ctrl+K"));
        dataGroup.Items.Add(ActionCmd("Import", RibbonItemSize.Small, IconKind.Folder, "Import CAD"));

        var dimGroup = new RibbonGroup { Header = "Dimension", Icon = RibbonIcon(IconKind.ChartLine) };
        dimGroup.Items.Add(ActionCmd("Aligned", RibbonItemSize.Large, IconKind.Ruler, "Aligned dimension", "DI"));
        dimGroup.Items.Add(ActionCmd("Angular", RibbonItemSize.Small, IconKind.ChartLine, "Angular dimension"));
        dimGroup.Items.Add(ActionCmd("Radial", RibbonItemSize.Small, IconKind.GridSquare, "Radial dimension"));
        dimGroup.Items.Add(ActionCmd("Diameter", RibbonItemSize.Small, IconKind.Layers, "Diameter dimension"));

        var annotateGroup = new RibbonGroup { Header = "Annotate", Icon = RibbonIcon(IconKind.Edit) };
        annotateGroup.Items.Add(ActionCmd("Text", RibbonItemSize.Large, IconKind.Edit, "Multi-line text", "T"));
        annotateGroup.Items.Add(ActionCmd("Leader", RibbonItemSize.Small, IconKind.AlignLeft, "Multi-leader arrow"));
        annotateGroup.Items.Add(ActionCmd("Cloud", RibbonItemSize.Small, IconKind.Layers, "Revision cloud"));

        var insertTab = new RibbonTabItem
        {
            Header = "Insert",
            Id = "insert",
            Icon = RibbonIcon(IconKind.TableData),
            Content = new List<RibbonGroup> { dataGroup, dimGroup, annotateGroup }
        };

        // ---- Tab 3: Format & Tools ----
        var textGroup = new RibbonGroup { Header = "Text & Font", Icon = RibbonIcon(IconKind.Edit) };
        var fontCombo = new RibbonComboBox
        {
            Header = "Font",
            Size = RibbonItemSize.Small,
            ColumnSpan = 2,
            ItemsSource = new List<string> { "Segoe UI", "Calibri", "Arial", "Consolas" },
            SelectedItem = "Segoe UI"
        };
        DependencyPropertyDescriptor.FromProperty(RibbonComboBox.SelectedItemProperty, typeof(RibbonComboBox))
            .AddValueChanged(fontCombo, (_, _) => Notify($"Font: {fontCombo.SelectedItem}"));

        var sizeCombo = new RibbonComboBox
        {
            Header = "Size",
            Size = RibbonItemSize.Small,
            ColumnSpan = 2,
            ItemsSource = new List<string> { "9", "10", "11", "12", "14", "16", "18", "24" },
            SelectedItem = "11"
        };
        DependencyPropertyDescriptor.FromProperty(RibbonComboBox.SelectedItemProperty, typeof(RibbonComboBox))
            .AddValueChanged(sizeCombo, (_, _) => Notify($"Font Size: {sizeCombo.SelectedItem}pt"));

        textGroup.Items.Add(fontCombo);
        textGroup.Items.Add(sizeCombo);

        var styleGroup = new RibbonGroup { Header = "Style", Icon = RibbonIcon(IconKind.Check) };
        var boldToggle = new RibbonToggleButton
        {
            Header = "Bold",
            Icon = TextGlyphIcon("B", FontWeights.Bold, FontStyles.Normal),
            IsChecked = true,
            ScreenTip = "Toggle bold"
        };
        boldToggle.Click += (_, _) => Notify($"Bold: {(boldToggle.IsChecked ? "ON" : "OFF")}");
        var italicToggle = new RibbonToggleButton
        {
            Header = "Italic",
            Icon = TextGlyphIcon("I", FontWeights.Normal, FontStyles.Italic),
            ScreenTip = "Toggle italic"
        };
        italicToggle.Click += (_, _) => Notify($"Italic: {(italicToggle.IsChecked ? "ON" : "OFF")}");
        var underlineToggle = new RibbonToggleButton
        {
            Header = "Underline",
            ColumnSpan = 2,
            Icon = TextGlyphIcon("U", FontWeights.Normal, FontStyles.Normal, TextDecorations.Underline),
            ScreenTip = "Toggle underline"
        };
        underlineToggle.Click += (_, _) => Notify($"Underline: {(underlineToggle.IsChecked ? "ON" : "OFF")}");
        styleGroup.Items.Add(boldToggle);
        styleGroup.Items.Add(italicToggle);
        styleGroup.Items.Add(underlineToggle);

        var snapGroup = new RibbonGroup { Header = "Drafting Snap", Icon = RibbonIcon(IconKind.GridSquare) };
        var snapCheck = new RibbonCheckBox { Header = "Snap", IsChecked = true, ScreenTip = "Object snap" };
        snapCheck.Click += (_, _) => Notify($"Snap: {(snapCheck.IsChecked == true ? "ON" : "OFF")}");
        var orthoCheck = new RibbonCheckBox { Header = "Ortho", ScreenTip = "Ortho angle constraint" };
        orthoCheck.Click += (_, _) => Notify($"Ortho: {(orthoCheck.IsChecked == true ? "ON" : "OFF")}");
        var polarCheck = new RibbonCheckBox { Header = "Polar", IsChecked = true, ScreenTip = "Polar tracking" };
        polarCheck.Click += (_, _) => Notify($"Polar: {(polarCheck.IsChecked == true ? "ON" : "OFF")}");
        snapGroup.Items.Add(snapCheck);
        snapGroup.Items.Add(orthoCheck);
        snapGroup.Items.Add(polarCheck);
        snapGroup.Items.Add(new RibbonSeparator());
        snapGroup.Items.Add(new RibbonTextBox { Header = "Step", Text = "10", ColumnSpan = 2, PlaceholderText = "10" });
        snapGroup.Items.Add(new RibbonTextBox { Header = "Angle", Text = "15", ColumnSpan = 2, PlaceholderText = "15" });

        var galleryGroup = new RibbonGroup { Header = "Materials", Icon = RibbonIcon(IconKind.Layers) };
        var gallery = new RibbonGallery
        {
            Size = RibbonItemSize.Large,
            ColumnSpan = 3,
            ItemWidth = 34,
            ItemHeight = 32,
            ScreenTip = "Select material swatch"
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
        gallery.SelectionChanged += (_, _) =>
        {
            if (gallery.SelectedItem is RibbonGalleryItem item)
            {
                Notify($"Material: {item.Header}");
                statusTool.Text = $"Material: {item.Header}";
            }
        };
        galleryGroup.Items.Add(gallery);

        var colorSearchGroup = new RibbonGroup { Header = "Color & Find", Icon = RibbonIcon(IconKind.Search) };
        var colorPicker = new RibbonColorPicker
        {
            Header = "Color",
            KeyTip = "C",
            SelectedColor = (Color)ColorConverter.ConvertFromString("#DC2626")
        };
        colorPicker.SelectedColorChanged += (_, _) => Notify($"Color: {colorPicker.SelectedColor}");
        var searchBox = new RibbonSearchBox
        {
            Header = "Find",
            Size = RibbonItemSize.Large,
            Placeholder = "Search commands, swatches…",
            ColumnSpan = 3
        };
        colorSearchGroup.Items.Add(colorPicker);
        colorSearchGroup.Items.Add(searchBox);

        var formatTab = new RibbonTabItem
        {
            Header = "Format",
            Id = "format",
            Icon = RibbonIcon(IconKind.Edit),
            Content = new List<RibbonGroup> { textGroup, styleGroup, snapGroup, galleryGroup, colorSearchGroup }
        };

        // ---- Tab 4: View ----
        var viewGroup = new RibbonGroup { Header = "Visibility", Icon = RibbonIcon(IconKind.GridSquare) };
        var gridToggle = new RibbonToggleButton
        {
            Header = "Grid",
            Size = RibbonItemSize.Small,
            IsChecked = true,
            Icon = RibbonIcon(IconKind.GridSquare),
            GestureText = "F7"
        };
        gridToggle.Click += (_, _) => Notify($"Grid: {(gridToggle.IsChecked ? "ON" : "OFF")}");
        var rulerToggle = new RibbonToggleButton
        {
            Header = "Ruler",
            Size = RibbonItemSize.Small,
            Icon = RibbonIcon(IconKind.Ruler),
            GestureText = "F8"
        };
        rulerToggle.Click += (_, _) => Notify($"Ruler: {(rulerToggle.IsChecked ? "ON" : "OFF")}");
        viewGroup.Items.Add(gridToggle);
        viewGroup.Items.Add(rulerToggle);
        viewGroup.Items.Add(ActionCmd("Layers", RibbonItemSize.Small, IconKind.Layers, "Manage drawing layers"));

        var navGroup = new RibbonGroup { Header = "Navigation", Icon = RibbonIcon(IconKind.ZoomIn) };
        navGroup.Items.Add(ActionCmd("Zoom In", RibbonItemSize.Small, IconKind.ZoomIn, "Zoom in", "I"));
        navGroup.Items.Add(ActionCmd("Zoom Out", RibbonItemSize.Small, IconKind.ZoomOut, "Zoom out", "O"));
        navGroup.Items.Add(ActionCmd("Extents", RibbonItemSize.Large, IconKind.ZoomIn, "Zoom to extents", "E"));
        navGroup.Items.Add(ActionCmd("Pan", RibbonItemSize.Small, IconKind.Edit, "Pan drawing", "P"));

        var scaleGroup = new RibbonGroup { Header = "Scale", Icon = RibbonIcon(IconKind.Ruler) };
        var radioGroup = new RibbonRadioGroup();
        var r50 = new RibbonRadioButton { Header = "1:50" };
        r50.IsCheckedChanged += (_, _) => { if (r50.IsChecked) Notify("Scale: 1:50"); };
        var r100 = new RibbonRadioButton { Header = "1:100", IsChecked = true };
        r100.IsCheckedChanged += (_, _) => { if (r100.IsChecked) Notify("Scale: 1:100"); };
        var r200 = new RibbonRadioButton { Header = "1:200" };
        r200.IsCheckedChanged += (_, _) => { if (r200.IsChecked) Notify("Scale: 1:200"); };
        radioGroup.Items.Add(r50);
        radioGroup.Items.Add(r100);
        radioGroup.Items.Add(r200);
        scaleGroup.Items.Add(radioGroup);

        var viewTab = new RibbonTabItem
        {
            Header = "View",
            Id = "view",
            Icon = RibbonIcon(IconKind.GridSquare),
            Content = new List<RibbonGroup> { viewGroup, navGroup, scaleGroup }
        };

        // ---- Contextual Tab Group: Revit Style ("Modify | Create Sketch" at far right) ----
        var sketchModeGroup = new RibbonGroup { Header = "Mode", Icon = RibbonIcon(IconKind.Check) };
        var cancelSketch = ActionCmd("Cancel", RibbonItemSize.Large, IconKind.Close, "Cancel sketch mode", "Esc");
        var finishSketch = ActionCmd("Finish", RibbonItemSize.Large, IconKind.Check, "Finish sketch mode", "Enter");
        sketchModeGroup.Items.Add(cancelSketch);
        sketchModeGroup.Items.Add(finishSketch);

        var sketchDrawGroup = new RibbonGroup { Header = "Sketch Tools", Icon = RibbonIcon(IconKind.Edit) };
        sketchDrawGroup.Items.Add(ActionCmd("Line", RibbonItemSize.Large, IconKind.Edit, "Draw sketch line", "L"));
        sketchDrawGroup.Items.Add(ActionCmd("Circle", RibbonItemSize.Large, IconKind.GridSquare, "Draw sketch circle", "C"));
        sketchDrawGroup.Items.Add(new RibbonSeparator());
        sketchDrawGroup.Items.Add(ActionCmd("Trim", RibbonItemSize.Small, IconKind.Close, "Trim curve", "TR"));
        sketchDrawGroup.Items.Add(ActionCmd("Offset", RibbonItemSize.Small, IconKind.Copy, "Offset curve", "O"));
        sketchDrawGroup.Items.Add(ActionCmd("Split", RibbonItemSize.Small, IconKind.Layers, "Split element", "SL"));

        var sketchDimGroup = new RibbonGroup { Header = "Constraints", Icon = RibbonIcon(IconKind.Link) };
        sketchDimGroup.Items.Add(ActionCmd("Aligned", RibbonItemSize.Large, IconKind.Ruler, "Aligned dimension", "DI"));
        sketchDimGroup.Items.Add(ActionCmd("Lock", RibbonItemSize.Small, IconKind.Link, "Lock constraint"));
        sketchDimGroup.Items.Add(ActionCmd("Equal", RibbonItemSize.Small, IconKind.GridSquare, "Equal spacing", "EQ"));

        var sketchTab = new RibbonTabItem
        {
            Header = "Create Sketch",
            Id = "create-sketch",
            Icon = RibbonIcon(IconKind.Edit),
            Content = new List<RibbonGroup> { sketchModeGroup, sketchDrawGroup, sketchDimGroup }
        };

        var sketchContextualGroup = new RibbonContextualTabGroup
        {
            Header = "Modify",
            AutoActivate = true,
            Visibility = Visibility.Visible
        };
        sketchContextualGroup.Items.Add(sketchTab);

        // ---- InfoCenter on top right of Ribbon ----
        var infoStatus = new TextBlock
        {
            Text = "3 objects selected",
            FontSize = 11,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 8, 0)
        };
        infoStatus.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");

        // Assemble the single master Ribbon!
        var mainRibbon = new Ribbon
        {
            MenuBar = menuBar,
            MenuBarPlacement = RibbonMenuBarPlacement.AboveRibbon,
            ShowMenuBar = false,
            QuickAccessPlacement = RibbonQuickAccessPlacement.AboveRibbon,
            ShowQuickAccessToolBar = false, // In AboveRibbon placement, QAT is housed directly in the ApplicationTitleBar (Revit standard)
            ApplicationMenu = fileMenu,
            ShowApplicationMenu = true,
            ShowBorder = false,
            ShowTabStrip = true,
            InfoCenter = infoStatus,
            ItemsSource = new object[] { homeTab, insertTab, formatTab, viewTab, sketchContextualGroup }
        };

        mainRibbon.Loaded += (_, _) =>
        {
            mainRibbon.PinToQuickAccess(qatSave);
            mainRibbon.PinToQuickAccess(qatUndo);
            mainRibbon.PinToQuickAccess(qatRedo);
            mainRibbon.PinToQuickAccess(qatPrint);
            mainRibbon.SelectedTabId = "create-sketch";
        };

        // -------------------------------------------------------------
        // 3. STUDIO APPLICATION SHELL (HERO WORKSPACE EXPERIENCE)
        // -------------------------------------------------------------
        var appTitleBar = new ApplicationTitleBar
        {
            ApplicationName = "CONCOST CAD Studio 2026",
            DocumentTitle = "Tower_A_Structural_Plan.rvt",
            Subtitle = "RVT 2026",
            ShowAppIcon = true,
            ShowWindowButtons = true,
            ShowSearchBox = true,
            SearchPlaceholder = Loc.T("Tìm lệnh & công cụ… (Ctrl+Q)", "Search commands & tools… (Ctrl+Q)"),
            ShowUserAccount = true,
            UserAccountName = Loc.T("Kiến trúc sư", "Architect"),
            ShowQuickAccess = true,
            Height = 32
        };

        appTitleBar.QuickAccess.Add(new TitleBarAction(IconKind.Save, Loc.T("Lưu", "Save"), new RelayCommand(() => Notify("Save (Ctrl+S)"))));
        appTitleBar.QuickAccess.Add(new TitleBarAction(IconKind.Undo, Loc.T("Hoàn tác", "Undo"), new RelayCommand(() => Notify("Undo (Ctrl+Z)"))));
        appTitleBar.QuickAccess.Add(new TitleBarAction(IconKind.Redo, Loc.T("Làm lại", "Redo"), new RelayCommand(() => Notify("Redo (Ctrl+Y)"))));
        appTitleBar.QuickAccess.Add(new TitleBarAction(IconKind.Print, Loc.T("In ấn", "Print"), new RelayCommand(() => Notify("Print (Ctrl+P)"))));
        appTitleBar.RefreshQuickAccess();

        var appWindowFrame = new Border
        {
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(10),
            Margin = new Thickness(0, 0, 0, 4),
            ClipToBounds = true
        };
        appWindowFrame.SetResourceReference(Border.BackgroundProperty, "CONCOST.Brush.Surface");
        appWindowFrame.SetResourceReference(Border.BorderBrushProperty, "CONCOST.Brush.Border");

        var appWindowGrid = new Grid();
        appWindowGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // 0: Application Title Bar
        appWindowGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // 1: Ribbon
        appWindowGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // 2: Document tabs
        appWindowGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(190) }); // 3: Viewport

        Grid.SetRow(appTitleBar, 0);
        appWindowGrid.Children.Add(appTitleBar);

        Grid.SetRow(mainRibbon, 1);
        appWindowGrid.Children.Add(mainRibbon);

        // Document Tabs Bar in Row 2
        var docTabsBar = new Border
        {
            Height = 28,
            BorderThickness = new Thickness(0, 1, 0, 1),
            Padding = new Thickness(8, 0, 8, 0)
        };
        docTabsBar.SetResourceReference(Border.BackgroundProperty, "CONCOST.Brush.SurfaceSecondary");
        docTabsBar.SetResourceReference(Border.BorderBrushProperty, "CONCOST.Brush.BorderSubtle");

        var docTabsPanel = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };

        Border CreateDocTab(string name, bool active)
        {
            var tab = new Border
            {
                Height = 24,
                CornerRadius = new CornerRadius(4),
                Padding = new Thickness(10, 0, 10, 0),
                Margin = new Thickness(0, 0, 4, 0),
                Cursor = Cursors.Hand
            };
            if (active)
            {
                tab.SetResourceReference(Border.BackgroundProperty, "CONCOST.Brush.Surface");
                tab.SetResourceReference(Border.BorderBrushProperty, "CONCOST.Brush.Border");
                tab.BorderThickness = new Thickness(1);
            }
            else
            {
                tab.Background = Brushes.Transparent;
            }

            var txt = new TextBlock
            {
                Text = name,
                FontSize = 11,
                FontWeight = active ? FontWeights.SemiBold : FontWeights.Normal,
                VerticalAlignment = VerticalAlignment.Center
            };
            txt.SetResourceReference(TextBlock.ForegroundProperty, active ? "CONCOST.Brush.TextPrimary" : "CONCOST.Brush.TextSecondary");
            tab.Child = txt;
            return tab;
        }

        docTabsPanel.Children.Add(CreateDocTab(Loc.T("📑 Tầng 01 - Mặt Bằng Kết Cấu", "📑 Level 01 - Structural Plan"), true));
        docTabsPanel.Children.Add(CreateDocTab(Loc.T("📑 Khung nhìn 3D - Kết Cấu Công Trình", "📑 3D View - Structural Frame"), false));
        docTabsPanel.Children.Add(CreateDocTab(Loc.T("📑 Chi Tiết Mặt Cắt A-A", "📑 Section Detail A-A"), false));
        docTabsBar.Child = docTabsPanel;
        Grid.SetRow(docTabsBar, 2);
        appWindowGrid.Children.Add(docTabsBar);

        // Viewport Canvas & Live HUD in Row 3
        // Blueprint navy background — #0D1B2A with white grid lines mimicking a CAD viewport
        var blueprintBg = new SolidColorBrush(Color.FromRgb(0x0D, 0x1B, 0x2A));
        blueprintBg.Freeze();

        var viewportStage = new Border { ClipToBounds = true, Background = blueprintBg };

        var viewportGrid = new Grid();
        viewportGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        viewportGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(28) });

        // Blueprint Drafting Canvas Simulation
        var canvasContainer = new Grid();

        // --- Blueprint Grid Overlay (subtle white dot-grid) ---
        var gridCanvas = new Canvas { IsHitTestVisible = false, Opacity = 0.12 };
        var gridPen = new SolidColorBrush(Colors.White);
        gridPen.Freeze();
        // Horizontal grid lines
        for (int gy = 0; gy <= 7; gy++)
        {
            var hLine = new Line { X1 = 0, Y1 = gy * 20, X2 = 960, Y2 = gy * 20, StrokeThickness = 0.5, Stroke = gridPen };
            gridCanvas.Children.Add(hLine);
        }
        // Vertical grid lines
        for (int gx = 0; gx <= 48; gx++)
        {
            var vLine = new Line { X1 = gx * 20, Y1 = 0, X2 = gx * 20, Y2 = 160, StrokeThickness = 0.5, Stroke = gridPen };
            gridCanvas.Children.Add(vLine);
        }
        canvasContainer.Children.Add(gridCanvas);

        // --- CAD Drawing: Structural Floor Plan Outline (white lines) ---
        var cadWhite = new SolidColorBrush(Color.FromRgb(0xE0, 0xEA, 0xFF));
        cadWhite.Freeze();
        var cadAccent = new SolidColorBrush(Color.FromRgb(0xF0, 0xAB, 0x40)); // amber dimension lines
        cadAccent.Freeze();
        var cadGreen = new SolidColorBrush(Color.FromRgb(0x34, 0xD3, 0x99)); // sketch mode lines
        cadGreen.Freeze();
        var cadDimText = new SolidColorBrush(Color.FromRgb(0xFB, 0xBF, 0x24));
        cadDimText.Freeze();

        var blueprintVisual = new Canvas { IsHitTestVisible = false };

        // Main floor plan rectangle — outer wall
        var wallPen = new Pen(cadWhite, 1.8);
        wallPen.Freeze();
        // Use polylines for walls
        var outerWall = new Polyline
        {
            Points = new PointCollection { new(80, 20), new(560, 20), new(560, 118), new(80, 118), new(80, 20) },
            Stroke = cadWhite, StrokeThickness = 1.8
        };
        blueprintVisual.Children.Add(outerWall);

        // Interior partition walls
        var partLine1 = new Line { X1 = 220, Y1 = 20, X2 = 220, Y2 = 118, Stroke = cadWhite, StrokeThickness = 1 };
        var partLine2 = new Line { X1 = 390, Y1 = 20, X2 = 390, Y2 = 118, Stroke = cadWhite, StrokeThickness = 1 };
        var partLine3 = new Line { X1 = 220, Y1 = 68, X2 = 390, Y2 = 68, Stroke = cadWhite, StrokeThickness = 1 };
        blueprintVisual.Children.Add(partLine1);
        blueprintVisual.Children.Add(partLine2);
        blueprintVisual.Children.Add(partLine3);

        // Door arc (room 1)
        var doorArc = new Path
        {
            Stroke = cadWhite, StrokeThickness = 0.8, StrokeDashArray = new DoubleCollection { 4, 2 },
            Data = Geometry.Parse("M 110,118 A 28,28 0 0 0 138,90")
        };
        blueprintVisual.Children.Add(doorArc);
        var doorLine = new Line { X1 = 110, Y1 = 90, X2 = 138, Y2 = 90, Stroke = cadWhite, StrokeThickness = 0.8 };
        blueprintVisual.Children.Add(doorLine);

        // Dimension line (top) — amber
        var dimLineTop = new Line { X1 = 80, Y1 = 10, X2 = 560, Y2 = 10, Stroke = cadAccent, StrokeThickness = 0.8 };
        var dimTick1 = new Line { X1 = 80, Y1 = 7, X2 = 80, Y2 = 13, Stroke = cadAccent, StrokeThickness = 0.8 };
        var dimTick2 = new Line { X1 = 560, Y1 = 7, X2 = 560, Y2 = 13, Stroke = cadAccent, StrokeThickness = 0.8 };
        blueprintVisual.Children.Add(dimLineTop);
        blueprintVisual.Children.Add(dimTick1);
        blueprintVisual.Children.Add(dimTick2);

        // Dimension text "12 400 mm"
        var dimLabel = new TextBlock
        {
            Text = "12 400 mm", FontSize = 9, Foreground = cadDimText,
            FontFamily = new FontFamily("Consolas")
        };
        Canvas.SetLeft(dimLabel, 290);
        Canvas.SetTop(dimLabel, 2);
        blueprintVisual.Children.Add(dimLabel);

        // Sketch mode active line (green dashes — simulates "Create Sketch" active state)
        var sketchActive = new Polyline
        {
            Points = new PointCollection { new(220, 68), new(390, 68) },
            Stroke = cadGreen, StrokeThickness = 2,
            StrokeDashArray = new DoubleCollection { 6, 3 }
        };
        blueprintVisual.Children.Add(sketchActive);

        // Column cross-marks at corners
        foreach (var pt in new[] { new Point(80, 20), new Point(560, 20), new Point(80, 118), new Point(560, 118) })
        {
            var cx1 = new Line { X1 = pt.X - 5, Y1 = pt.Y, X2 = pt.X + 5, Y2 = pt.Y, Stroke = cadAccent, StrokeThickness = 1.2 };
            var cx2 = new Line { X1 = pt.X, Y1 = pt.Y - 5, X2 = pt.X, Y2 = pt.Y + 5, Stroke = cadAccent, StrokeThickness = 1.2 };
            blueprintVisual.Children.Add(cx1);
            blueprintVisual.Children.Add(cx2);
        }

        // Room label text
        var roomLabel1 = new TextBlock { Text = "ROOM A", FontSize = 8.5, Foreground = cadWhite, Opacity = 0.6, FontFamily = new FontFamily("Consolas") };
        Canvas.SetLeft(roomLabel1, 118);
        Canvas.SetTop(roomLabel1, 60);
        var roomLabel2 = new TextBlock { Text = "ROOM B", FontSize = 8.5, Foreground = cadWhite, Opacity = 0.6, FontFamily = new FontFamily("Consolas") };
        Canvas.SetLeft(roomLabel2, 280);
        Canvas.SetTop(roomLabel2, 36);
        var roomLabel3 = new TextBlock { Text = "ROOM C", FontSize = 8.5, Foreground = cadWhite, Opacity = 0.6, FontFamily = new FontFamily("Consolas") };
        Canvas.SetLeft(roomLabel3, 420);
        Canvas.SetTop(roomLabel3, 60);
        blueprintVisual.Children.Add(roomLabel1);
        blueprintVisual.Children.Add(roomLabel2);
        blueprintVisual.Children.Add(roomLabel3);

        canvasContainer.Children.Add(blueprintVisual);

        // Center HUD Action Text (over blueprint)
        var hudTextColor = new SolidColorBrush(Colors.White);
        hudTextColor.Freeze();
        statusAction.Foreground = hudTextColor;
        statusAction.FontSize = 12;

        var hudOverlay = new StackPanel
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        hudOverlay.Children.Add(statusAction);
        canvasContainer.Children.Add(hudOverlay);

        Grid.SetRow(canvasContainer, 0);
        viewportGrid.Children.Add(canvasContainer);

        // Bottom CAD Status Bar — dark navy consistent with the blueprint viewport
        var bottomBarBg = new SolidColorBrush(Color.FromRgb(0x07, 0x11, 0x1D));
        bottomBarBg.Freeze();
        var bottomBarBorder = new SolidColorBrush(Color.FromRgb(0x1E, 0x34, 0x50));
        bottomBarBorder.Freeze();
        var statusTextBrush = new SolidColorBrush(Color.FromRgb(0x8B, 0xA3, 0xC2));
        statusTextBrush.Freeze();

        // Override status bar text colors to match dark blueprint theme
        statusTab.Foreground = statusTextBrush;
        statusTool.Foreground = statusTextBrush;

        var bottomBar = new Border
        {
            BorderThickness = new Thickness(0, 1, 0, 0),
            Padding = new Thickness(12, 0, 12, 0),
            Background = bottomBarBg,
            BorderBrush = bottomBarBorder
        };

        var bottomBarGrid = new Grid();
        bottomBarGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        bottomBarGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        bottomBarGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        var coordsText = new TextBlock
        {
            Text = "X: 2,450.00 mm   Y: 1,820.00 mm   Z: 0.00 mm",
            FontSize = 10.5,
            VerticalAlignment = VerticalAlignment.Center,
            FontFamily = new FontFamily("Consolas"),
            Foreground = statusTextBrush
        };
        Grid.SetColumn(coordsText, 0);
        bottomBarGrid.Children.Add(coordsText);

        var midStatus = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        midStatus.Children.Add(statusTab);
        midStatus.Children.Add(statusTool);
        midStatus.Children.Add(modeBadge);
        Grid.SetColumn(midStatus, 1);
        bottomBarGrid.Children.Add(midStatus);

        var flagsText = new TextBlock
        {
            Text = "SCALE: 1:100  |  SNAP: ON  |  ORTHO: OFF  |  POLAR: ON",
            FontSize = 10.5,
            VerticalAlignment = VerticalAlignment.Center,
            FontFamily = new FontFamily("Consolas"),
            Foreground = statusTextBrush
        };
        Grid.SetColumn(flagsText, 2);
        bottomBarGrid.Children.Add(flagsText);

        bottomBar.Child = bottomBarGrid;
        Grid.SetRow(bottomBar, 1);
        viewportGrid.Children.Add(bottomBar);

        viewportStage.Child = viewportGrid;
        Grid.SetRow(viewportStage, 3);
        appWindowGrid.Children.Add(viewportStage);

        appWindowFrame.Child = appWindowGrid;

        var ribbonHeroContainer = new StackPanel { Tag = "full-bleed" };
        ribbonHeroContainer.Children.Add(appWindowFrame);

        // -------------------------------------------------------------
        // 4. INTERACTIVE CONFIGURATION & STYLE INSPECTOR (CARD 2)
        // -------------------------------------------------------------
        var controlGrid = new Grid { Margin = new Thickness(4, 2, 4, 2) };
        controlGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        controlGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) }); // vertical divider
        controlGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        StackPanel CreateSectionHeader(string title, IconKind icon)
        {
            var header = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 10) };
            var hIcon = new Icon { Kind = icon, Width = 15, Height = 15, Margin = new Thickness(0, 0, 8, 0), VerticalAlignment = VerticalAlignment.Center };
            hIcon.SetResourceReference(Icon.ForegroundProperty, "CONCOST.Brush.Primary");
            var hTitle = new TextBlock { Text = title, FontSize = 12.5, FontWeight = FontWeights.SemiBold, VerticalAlignment = VerticalAlignment.Center };
            hTitle.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextPrimary");
            header.Children.Add(hIcon);
            header.Children.Add(hTitle);
            return header;
        }

        Border CreateSectionDivider()
        {
            var div = new Border { Height = 1, Margin = new Thickness(0, 14, 0, 14) };
            div.SetResourceReference(Border.BackgroundProperty, "CONCOST.Brush.BorderSubtle");
            return div;
        }

        // ==========================================
        // LEFT COLUMN: Display Modes & Toolbars
        // ==========================================
        var leftCol = new StackPanel { Margin = new Thickness(0, 0, 16, 0) };

        // --- 1. Display Mode (Revit 4 States) ---
        leftCol.Children.Add(CreateSectionHeader(
            Loc.T("Chế độ hiển thị (4 trạng thái chuẩn Autodesk Revit)", "Display Mode — 4 States (Autodesk Revit standard)"),
            IconKind.ZoomIn));

        var modeButtons = new Dictionary<RibbonDisplayMode, Button>();
        void UpdateModePills(RibbonDisplayMode activeMode)
        {
            foreach (var kvp in modeButtons)
            {
                bool isCur = kvp.Key == activeMode;
                kvp.Value.SetResourceReference(FrameworkElement.StyleProperty, isCur ? "CONCOST.Button.Primary" : "CONCOST.Button.Secondary");
            }
        }

        Button CreateModePill(string title, RibbonDisplayMode mode)
        {
            var btn = new Button
            {
                Content = title,
                Height = 30,
                Margin = new Thickness(1),
                FontSize = 11,
                Cursor = Cursors.Hand
            };
            btn.SetResourceReference(FrameworkElement.StyleProperty, "CONCOST.Button.Secondary");
            btn.Click += (_, _) =>
            {
                mainRibbon.DisplayMode = mode;
                UpdateModePills(mode);
            };
            modeButtons[mode] = btn;
            return btn;
        }

        var modePillsGrid = new UniformGrid { Columns = 4, Margin = new Thickness(0, 0, 0, 8) };
        modePillsGrid.Children.Add(CreateModePill(Loc.T("1. Đầy đủ", "1. Full"), RibbonDisplayMode.Full));
        modePillsGrid.Children.Add(CreateModePill(Loc.T("2. Chỉ Tabs", "2. Tabs"), RibbonDisplayMode.MinimizeToTabs));
        modePillsGrid.Children.Add(CreateModePill(Loc.T("3. Tiêu đề", "3. Titles"), RibbonDisplayMode.MinimizeToPanelTitles));
        modePillsGrid.Children.Add(CreateModePill(Loc.T("4. Nút Panel", "4. Buttons"), RibbonDisplayMode.MinimizeToPanelButtons));
        leftCol.Children.Add(modePillsGrid);
        UpdateModePills(mainRibbon.DisplayMode);
        mainRibbon.DisplayModeChanged += (_, _) => UpdateModePills(mainRibbon.DisplayMode);

        var modeActions = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 0) };
        var cycleBtn = new Button
        {
            Content = Loc.T("Xoay vòng (Cycle)", "Cycle Display Mode"),
            Padding = new Thickness(12, 4, 12, 4),
            Height = 28,
            Margin = new Thickness(0, 0, 8, 0),
            FontSize = 11.5
        };
        cycleBtn.SetResourceReference(FrameworkElement.StyleProperty, "CONCOST.Button.Secondary");
        cycleBtn.Click += (_, _) => mainRibbon.CycleState();
        modeActions.Children.Add(cycleBtn);

        var toggleMinBtn = new Button
        {
            Content = Loc.T("Thu gọn (Ctrl+F1)", "Toggle (Ctrl+F1)"),
            Padding = new Thickness(12, 4, 12, 4),
            Height = 28,
            Margin = new Thickness(0, 0, 14, 0),
            FontSize = 11.5
        };
        toggleMinBtn.SetResourceReference(FrameworkElement.StyleProperty, "CONCOST.Button.Secondary");
        toggleMinBtn.Click += (_, _) => mainRibbon.ToggleMinimized();
        modeActions.Children.Add(toggleMinBtn);

        var showMinimizeChevron = new CheckBox
        {
            Content = Loc.T("Nút mũi tên thu nhỏ góc phải", "Show minimize chevron"),
            IsChecked = true,
            VerticalAlignment = VerticalAlignment.Center,
            FontSize = 11.5
        };
        showMinimizeChevron.Checked += (_, _) => mainRibbon.ShowMinimizeButton = true;
        showMinimizeChevron.Unchecked += (_, _) => mainRibbon.ShowMinimizeButton = false;
        modeActions.Children.Add(showMinimizeChevron);
        leftCol.Children.Add(modeActions);

        leftCol.Children.Add(CreateSectionDivider());

        // --- 2. Toolbars Placement (QAT & MenuBar) ---
        leftCol.Children.Add(CreateSectionHeader(
            Loc.T("Thanh lệnh nhanh (QAT) & MenuBar", "Quick Access Toolbar (QAT) & MenuBar"),
            IconKind.Settings));

        var qatPillsGrid = new UniformGrid { Columns = 3, Margin = new Thickness(0, 0, 0, 8) };
        var qatButtons = new Dictionary<RibbonQuickAccessPlacement, Button>();

        void UpdateQatPills(RibbonQuickAccessPlacement activePlacement)
        {
            foreach (var kvp in qatButtons)
            {
                bool isCur = kvp.Key == activePlacement;
                kvp.Value.SetResourceReference(FrameworkElement.StyleProperty, isCur ? "CONCOST.Button.Primary" : "CONCOST.Button.Secondary");
            }
        }

        var showQatCheck = new CheckBox
        {
            Content = Loc.T("Hiện thanh QAT", "Show QAT"),
            IsChecked = true,
            Margin = new Thickness(0, 0, 14, 0),
            VerticalAlignment = VerticalAlignment.Center,
            FontSize = 11.5
        };

        void ApplyQatPlacement(RibbonQuickAccessPlacement placement)
        {
            mainRibbon.QuickAccessPlacement = placement;
            UpdateQatPills(placement);

            if (placement == RibbonQuickAccessPlacement.AboveRibbon)
            {
                appTitleBar.ShowQuickAccess = showQatCheck.IsChecked == true;
                mainRibbon.ShowQuickAccessToolBar = false;
                Notify(Loc.T("Vị trí QAT: Trên thanh tiêu đề (Chuẩn Autodesk Revit)", "QAT Placement: Above Ribbon in TitleBar (Autodesk Revit standard)"));
            }
            else
            {
                appTitleBar.ShowQuickAccess = false;
                mainRibbon.ShowQuickAccessToolBar = showQatCheck.IsChecked == true;
                if (placement == RibbonQuickAccessPlacement.HeaderRight)
                {
                    Notify(Loc.T("Vị trí QAT: Cùng hàng với Tabs (HeaderRight)", "QAT Placement: HeaderRight (Inline with tabs)"));
                }
                else
                {
                    Notify(Loc.T("Vị trí QAT: Dưới thân Ribbon (BelowRibbon)", "QAT Placement: Below Ribbon Panels"));
                }
            }
        }

        Button CreateQatPill(string title, RibbonQuickAccessPlacement placement)
        {
            var btn = new Button
            {
                Content = title,
                Height = 30,
                Margin = new Thickness(1),
                FontSize = 11,
                Cursor = Cursors.Hand
            };
            btn.SetResourceReference(FrameworkElement.StyleProperty, "CONCOST.Button.Secondary");
            btn.Click += (_, _) => ApplyQatPlacement(placement);
            qatButtons[placement] = btn;
            return btn;
        }

        qatPillsGrid.Children.Add(CreateQatPill(Loc.T("AboveRibbon (Chuẩn)", "AboveRibbon (Default)"), RibbonQuickAccessPlacement.AboveRibbon));
        qatPillsGrid.Children.Add(CreateQatPill("HeaderRight", RibbonQuickAccessPlacement.HeaderRight));
        qatPillsGrid.Children.Add(CreateQatPill("BelowRibbon", RibbonQuickAccessPlacement.BelowRibbon));
        leftCol.Children.Add(qatPillsGrid);
        UpdateQatPills(mainRibbon.QuickAccessPlacement);

        var qatToggles = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 0) };
        showQatCheck.Checked += (_, _) =>
        {
            if (mainRibbon.QuickAccessPlacement == RibbonQuickAccessPlacement.AboveRibbon)
            {
                appTitleBar.ShowQuickAccess = true;
                mainRibbon.ShowQuickAccessToolBar = false;
            }
            else
            {
                appTitleBar.ShowQuickAccess = false;
                mainRibbon.ShowQuickAccessToolBar = true;
            }
            Notify(Loc.T("Hiện thanh lệnh nhanh QAT: BẬT", "Show QAT: ON"));
        };
        showQatCheck.Unchecked += (_, _) =>
        {
            appTitleBar.ShowQuickAccess = false;
            mainRibbon.ShowQuickAccessToolBar = false;
            Notify(Loc.T("Hiện thanh lệnh nhanh QAT: TẮT", "Show QAT: OFF"));
        };
        qatToggles.Children.Add(showQatCheck);

        var qatCustomCheck = new CheckBox
        {
            Content = Loc.T("Tùy biến lệnh QAT", "Customizable QAT"),
            IsChecked = true,
            Margin = new Thickness(0, 0, 14, 0),
            VerticalAlignment = VerticalAlignment.Center,
            FontSize = 11.5
        };
        qatCustomCheck.Checked += (_, _) => mainRibbon.IsQuickAccessCustomizable = true;
        qatCustomCheck.Unchecked += (_, _) => mainRibbon.IsQuickAccessCustomizable = false;
        qatToggles.Children.Add(qatCustomCheck);

        var showMbCheck = new CheckBox
        {
            Content = Loc.T("Hiện MenuBar", "Show MenuBar"),
            IsChecked = false,
            VerticalAlignment = VerticalAlignment.Center,
            FontSize = 11.5
        };
        showMbCheck.Checked += (_, _) =>
        {
            mainRibbon.ShowMenuBar = true;
            Notify(Loc.T("Thanh MenuBar: BẬT", "MenuBar: ON"));
        };
        showMbCheck.Unchecked += (_, _) =>
        {
            mainRibbon.ShowMenuBar = false;
            Notify(Loc.T("Thanh MenuBar: TẮT", "MenuBar: OFF"));
        };
        qatToggles.Children.Add(showMbCheck);
        leftCol.Children.Add(qatToggles);

        Grid.SetColumn(leftCol, 0);
        controlGrid.Children.Add(leftCol);

        // --- Vertical Divider Line ---
        var vDivider = new Border
        {
            Width = 1,
            Margin = new Thickness(0, 4, 0, 4),
            VerticalAlignment = VerticalAlignment.Stretch
        };
        vDivider.SetResourceReference(Border.BackgroundProperty, "CONCOST.Brush.BorderSubtle");
        Grid.SetColumn(vDivider, 1);
        controlGrid.Children.Add(vDivider);

        // ==========================================
        // RIGHT COLUMN: Contextual Tab & Feature Flags
        // ==========================================
        var rightCol = new StackPanel { Margin = new Thickness(16, 0, 0, 0) };

        // --- 1. Revit Contextual Tab ---
        rightCol.Children.Add(CreateSectionHeader(
            Loc.T("Tab ngữ cảnh Revit (Modify | Sketch)", "Revit Contextual Tab (Modify | Sketch)"),
            IconKind.Edit));

        var ctxActions = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 8) };
        var toggleSketchBtn = new Button
        {
            Content = Loc.T("Thoát chế độ vẽ phác", "Exit Sketch Mode"),
            Padding = new Thickness(14, 4, 14, 4),
            Height = 30,
            Margin = new Thickness(0, 0, 12, 0),
            FontSize = 11.5
        };
        toggleSketchBtn.SetResourceReference(FrameworkElement.StyleProperty, "CONCOST.Button.Secondary");

        bool isSketchActive = true;
        void SetSketchMode(bool active)
        {
            isSketchActive = active;
            sketchContextualGroup.Visibility = active ? Visibility.Visible : Visibility.Collapsed;
            toggleSketchBtn.Content = active ? Loc.T("Thoát chế độ vẽ phác", "Exit Sketch Mode") : Loc.T("Vào chế độ vẽ phác", "Enter Sketch Mode");
            modeBadgeText.Text = active ? Loc.T("CHẾ ĐỘ VẼ PHÁC 2D: ĐANG BẬT", "2D SKETCH MODE: ACTIVE") : Loc.T("KHÔNG GIAN LÀM VIỆC TIÊU CHUẨN", "STANDARD WORKSPACE");
            Notify(active ? "Sketch Mode Activated" : "Sketch Mode Exited");
        }

        toggleSketchBtn.Click += (_, _) => SetSketchMode(!isSketchActive);
        cancelSketch.Click += (_, _) => SetSketchMode(false);
        finishSketch.Click += (_, _) => SetSketchMode(false);
        ctxActions.Children.Add(toggleSketchBtn);

        var autoActivateCheck = new CheckBox
        {
            Content = Loc.T("Tự động kích hoạt khi chọn (AutoActivate)", "AutoActivate on selection"),
            IsChecked = true,
            VerticalAlignment = VerticalAlignment.Center,
            FontSize = 11.5
        };
        autoActivateCheck.Checked += (_, _) => sketchContextualGroup.AutoActivate = true;
        autoActivateCheck.Unchecked += (_, _) => sketchContextualGroup.AutoActivate = false;
        ctxActions.Children.Add(autoActivateCheck);
        rightCol.Children.Add(ctxActions);

        var swatchRow = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 0) };
        var swatchLabel = new TextBlock
        {
            Text = Loc.T("Màu nhấn tab:", "Accent:"),
            FontSize = 11.5,
            FontWeight = FontWeights.Medium,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 10, 0)
        };
        swatchLabel.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");
        swatchRow.Children.Add(swatchLabel);

        Border CreateAccentCircle(string name, Brush? brush, Action onClick)
        {
            var circle = new Border
            {
                Width = 22,
                Height = 22,
                CornerRadius = new CornerRadius(11),
                Margin = new Thickness(0, 0, 8, 0),
                Cursor = Cursors.Hand,
                ToolTip = name,
                BorderThickness = new Thickness(1.5)
            };
            circle.SetResourceReference(Border.BorderBrushProperty, "CONCOST.Brush.Border");
            if (brush != null) circle.Background = brush;
            else circle.SetResourceReference(Border.BackgroundProperty, "CONCOST.Brush.Primary");

            circle.MouseDown += (_, _) => onClick();
            return circle;
        }

        static SolidColorBrush FreezeBrush(byte r, byte g, byte b)
        {
            var brush = new SolidColorBrush(Color.FromRgb(r, g, b));
            brush.Freeze();
            return brush;
        }

        swatchRow.Children.Add(CreateAccentCircle(Loc.T("Mặc định theo Theme", "Theme Default"), null, () =>
        {
            sketchContextualGroup.Accent = null;
            Notify("Contextual Accent: Theme Default");
        }));
        swatchRow.Children.Add(CreateAccentCircle(Loc.T("Xanh Revit (#107C41)", "Revit Green (#107C41)"), FreezeBrush(0x10, 0x7C, 0x41), () =>
        {
            sketchContextualGroup.Accent = FreezeBrush(0x10, 0x7C, 0x41);
            Notify("Contextual Accent: Revit Green (#107C41)");
        }));
        swatchRow.Children.Add(CreateAccentCircle("Teal (#0D9488)", FreezeBrush(0x0D, 0x94, 0x88), () =>
        {
            sketchContextualGroup.Accent = FreezeBrush(0x0D, 0x94, 0x88);
            Notify("Contextual Accent: Teal (#0D9488)");
        }));
        swatchRow.Children.Add(CreateAccentCircle("Amber (#D97706)", FreezeBrush(0xD9, 0x77, 0x06), () =>
        {
            sketchContextualGroup.Accent = FreezeBrush(0xD9, 0x77, 0x06);
            Notify("Contextual Accent: Amber (#D97706)");
        }));
        swatchRow.Children.Add(CreateAccentCircle("Crimson (#D93B3B)", FreezeBrush(0xD9, 0x3B, 0x3B), () =>
        {
            sketchContextualGroup.Accent = FreezeBrush(0xD9, 0x3B, 0x3B);
            Notify("Contextual Accent: Crimson (#D93B3B)");
        }));
        swatchRow.Children.Add(CreateAccentCircle("Royal Blue (#2563EB)", FreezeBrush(0x25, 0x63, 0xEB), () =>
        {
            sketchContextualGroup.Accent = FreezeBrush(0x25, 0x63, 0xEB);
            Notify("Contextual Accent: Royal Blue (#2563EB)");
        }));
        rightCol.Children.Add(swatchRow);

        rightCol.Children.Add(CreateSectionDivider());

        // --- 2. Ribbon Feature Flags (Clean 2-Column Aligned Grid) ---
        rightCol.Children.Add(CreateSectionHeader(
            Loc.T("Tùy chọn thuộc tính Ribbon", "Ribbon Feature Flags"),
            IconKind.Layers));

        var shellGrid = new UniformGrid { Columns = 2, Margin = new Thickness(0, 0, 0, 0) };

        CheckBox CreateFeatureCheck(string label, bool initial, Action<bool> apply)
        {
            var cb = new CheckBox
            {
                Content = label,
                IsChecked = initial,
                Margin = new Thickness(0, 2, 8, 4),
                FontSize = 11.5
            };
            cb.Checked += (_, _) => apply(true);
            cb.Unchecked += (_, _) => apply(false);
            return cb;
        }

        shellGrid.Children.Add(CreateFeatureCheck(Loc.T("Hiện viền khung (ShowBorder)", "Show Border (ShowBorder)"), true, on => mainRibbon.ShowBorder = on));
        shellGrid.Children.Add(CreateFeatureCheck(Loc.T("Hiện khối File (ShowApplicationMenu)", "Show File Tab (ShowApplicationMenu)"), true, on => mainRibbon.ShowApplicationMenu = on));
        shellGrid.Children.Add(CreateFeatureCheck(Loc.T("Hiện dải Tab (ShowTabStrip)", "Show Tab Strip (ShowTabStrip)"), true, on => mainRibbon.ShowTabStrip = on));
        shellGrid.Children.Add(CreateFeatureCheck(Loc.T("Hiện nhãn nhóm (ShowGroupLabels)", "Show Group Labels (ShowGroupLabels)"), true, on => Ribbon.SetShowGroupLabels(mainRibbon, on)));
        shellGrid.Children.Add(CreateFeatureCheck(Loc.T("Mẹo lệnh ScreenTip (ShowScreenTips)", "ScreenTips (ShowScreenTips)"), true, on => mainRibbon.ShowScreenTips = on));
        shellGrid.Children.Add(CreateFeatureCheck(Loc.T("Phím tắt Alt (IsKeyTipEnabled)", "KeyTips (IsKeyTipEnabled)"), true, on => mainRibbon.IsKeyTipEnabled = on));
        shellGrid.Children.Add(CreateFeatureCheck(Loc.T("Tự thu gọn khi hẹp (IsGroupCollapseEnabled)", "Auto Collapse (IsGroupCollapseEnabled)"), true, on => mainRibbon.IsGroupCollapseEnabled = on));

        rightCol.Children.Add(shellGrid);

        Grid.SetColumn(rightCol, 2);
        controlGrid.Children.Add(rightCol);

        // -------------------------------------------------------------
        // 5. ASSEMBLE CLEAN MASTER SHOWCASE (2 POLISHED CARDS)
        // -------------------------------------------------------------
        var root = new StackPanel();

        // Card 1: Studio Application Shell & Ribbon Workspace
        root.Children.Add(DemoSection(
            Loc.T("Trải nghiệm Ribbon hoàn chỉnh", "Complete Ribbon Experience"),
            ribbonHeroContainer,
            xaml: "<ribbon:Ribbon x:Name=\"AppRibbon\"\n" +
                  "               DisplayMode=\"Full\"\n" +
                  "               ShowBorder=\"True\"\n" +
                  "               QuickAccessPlacement=\"AboveRibbon\"\n" +
                  "               MenuBarPlacement=\"AboveRibbon\"\n" +
                  "               SelectedTabId=\"create-sketch\">\n\n" +
                  "    <!-- Application Menu (File block) -->\n" +
                  "    <ribbon:Ribbon.ApplicationMenu>\n" +
                  "        <ribbon:RibbonApplicationMenu Header=\"File\">\n" +
                  "            <ribbon:RibbonMenuItem Header=\"New Project…\" Icon=\"File\" InputGestureText=\"Ctrl+N\" />\n" +
                  "            <ribbon:RibbonMenuItem Header=\"Open Model…\" Icon=\"Folder\" InputGestureText=\"Ctrl+O\" />\n" +
                  "            <ribbon:RibbonMenuItem Header=\"Save\" Icon=\"Save\" InputGestureText=\"Ctrl+S\" />\n" +
                  "        </ribbon:RibbonApplicationMenu>\n" +
                  "    </ribbon:Ribbon.ApplicationMenu>\n\n" +
                  "    <!-- MenuBar above the tab strip -->\n" +
                  "    <ribbon:Ribbon.MenuBar>\n" +
                  "        <ribbon:RibbonMenuBar>\n" +
                  "            <ribbon:RibbonMenuBarItem Header=\"File\" />\n" +
                  "            <ribbon:RibbonMenuBarItem Header=\"Edit\" />\n" +
                  "            <ribbon:RibbonMenuBarItem Header=\"View\" />\n" +
                  "            <ribbon:RibbonMenuBarItem Header=\"Tools\" />\n" +
                  "        </ribbon:RibbonMenuBar>\n" +
                  "    </ribbon:Ribbon.MenuBar>\n\n" +
                  "    <!-- Standard Tabs -->\n" +
                  "    <ribbon:RibbonTabItem Header=\"Home\" Id=\"home\">\n" +
                  "        <ribbon:RibbonGroup Header=\"Clipboard\">\n" +
                  "            <ribbon:RibbonButton Header=\"Paste\" Size=\"Large\" />\n" +
                  "            <ribbon:RibbonButton Header=\"Copy\" Size=\"Small\" />\n" +
                  "        </ribbon:RibbonGroup>\n" +
                  "        <ribbon:RibbonGroup Header=\"Draw\">\n" +
                  "            <ribbon:RibbonButton Header=\"Line\" Size=\"Large\" />\n" +
                  "            <ribbon:RibbonButton Header=\"Circle\" Size=\"Large\" />\n" +
                  "        </ribbon:RibbonGroup>\n" +
                  "    </ribbon:RibbonTabItem>\n\n" +
                  "    <!-- Revit Contextual Tab Group (Modify | Create Sketch) -->\n" +
                  "    <ribbon:RibbonContextualTabGroup Header=\"Modify\" AutoActivate=\"True\">\n" +
                  "        <ribbon:RibbonTabItem Header=\"Create Sketch\" Id=\"create-sketch\">\n" +
                  "            <ribbon:RibbonGroup Header=\"Mode\">\n" +
                  "                <ribbon:RibbonButton Header=\"Cancel\" Size=\"Large\" />\n" +
                  "                <ribbon:RibbonButton Header=\"Finish\" Size=\"Large\" />\n" +
                  "            </ribbon:RibbonGroup>\n" +
                  "        </ribbon:RibbonTabItem>\n" +
                  "    </ribbon:RibbonContextualTabGroup>\n" +
                  "</ribbon:Ribbon>",
            csharp: "// Pin commands into Quick Access ToolBar\n" +
                    "ribbon.PinToQuickAccess(saveCommand);\n" +
                    "ribbon.PinToQuickAccess(undoCommand);\n" +
                    "ribbon.PinToQuickAccess(redoCommand);\n\n" +
                    "// Show contextual tab programmatically (Revit style)\n" +
                    "var sketchTab = new RibbonTabItem\n" +
                    "{\n" +
                    "    Header = \"Create Sketch\",\n" +
                    "    ContextualGroup = \"Modify\",\n" +
                    "    AutoActivate = true\n" +
                    "};\n" +
                    "ribbon.ShowContextualTab(sketchTab);\n\n" +
                    "// Cycle 4 display modes: Full -> Tabs -> Titles -> Buttons\n" +
                    "ribbon.CycleState();"));

        // Card 2: Interactive Configuration & Extended Properties
        root.Children.Add(DemoSection(
            Loc.T("Bảng điều khiển thuộc tính & Styles", "Ribbon Properties & Style Controls"),
            controlGrid,
            xaml: "<!-- Extended Properties and Style Customization -->\n" +
                  "<ribbon:Ribbon DisplayMode=\"Full\"\n" +
                  "               QuickAccessPlacement=\"AboveRibbon\"\n" +
                  "               MenuBarPlacement=\"AboveRibbon\"\n" +
                  "               ShowMenuBar=\"True\"\n" +
                  "               ShowQuickAccessToolBar=\"True\"\n" +
                  "               ShowBorder=\"True\"\n" +
                  "               ShowApplicationMenu=\"True\"\n" +
                  "               ShowTabStrip=\"True\"\n" +
                  "               ShowGroupLabels=\"True\"\n" +
                  "               ShowScreenTips=\"True\"\n" +
                  "               IsKeyTipEnabled=\"True\"\n" +
                  "               IsGroupCollapseEnabled=\"True\" />",
            csharp: "// Dynamic runtime property tuning\n" +
                    "ribbon.DisplayMode = RibbonDisplayMode.MinimizeToTabs;\n" +
                    "ribbon.QuickAccessPlacement = RibbonQuickAccessPlacement.AboveRibbon;\n" +
                    "ribbon.MenuBarPlacement = RibbonMenuBarPlacement.AboveRibbon;\n" +
                    "ribbon.ShowBorder = true;\n" +
                    "Ribbon.SetShowGroupLabels(ribbon, true);\n\n" +
                    "// Select active tab by identifier\n" +
                    "ribbon.SelectedTabId = \"create-sketch\";\n\n" +
                    "// Accent color for contextual tab group (or null to follow active theme)\n" +
                    "sketchGroup.Accent = new SolidColorBrush(Color.FromRgb(0x10, 0x7C, 0x41));"));

        return root;
    }
}
