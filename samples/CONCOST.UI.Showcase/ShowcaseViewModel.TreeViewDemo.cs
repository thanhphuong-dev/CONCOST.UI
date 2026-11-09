using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using CONCOST.UI.AttachedProperties;
using CONCOST.UI.Controls;
using CONCOST.UI.Icons;
using CONCOST.UI.Services;

namespace CONCOST.UI.Showcase;

public partial class ShowcaseViewModel
{
    private class DemoTreeNode

    {

        public string Key { get; set; } = "";

        public string Name { get; set; } = "";

        public string Details { get; set; } = "";

        public IconKind Icon { get; set; } = IconKind.Folder;

        public Brush IconBrush { get; set; } = Brushes.Gray;

        public string? Tag { get; set; }

        public BadgeAppearance BadgeStyle { get; set; } = BadgeAppearance.Neutral;

        public bool? IsChecked { get; set; } = false;

        public bool IsExpanded { get; set; } = true;

        public List<DemoTreeNode> Children { get; set; } = new();

    }

    private object CreateTreeViewDemo()

    {

        var mainSp = new StackPanel { Margin = new System.Windows.Thickness(0) };

        void AddCardSection(string title, UIElement content, string? xamlSnippet = null, string? csSnippet = null)

        {

            mainSp.Children.Add(CreateStandardDemoCard(title, content, xamlSnippet, csSnippet));

        }

        static TextBlock SectionSubhead(string text)

        {

            var tb = new TextBlock

            {

                Text = text,

                FontSize = 11,

                FontWeight = System.Windows.FontWeights.Bold,

                Margin = new System.Windows.Thickness(0, 0, 0, 6)

            };

            tb.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");

            return tb;

        }

        // ==================== 1. TreeView Demo Data Factory ====================

        DemoTreeNode CreateInitialProjectData() => new DemoTreeNode

        {

            Key = "PROJECT",

            Name = "Project: CONCOST Landmark Tower",

            Details = "Tower A — 35 Stories Commercial & Residential",

            Icon = IconKind.Folder,

            IconBrush = new SolidColorBrush(Color.FromRgb(217, 119, 6)), // Gold

            Tag = "Active Project",

            BadgeStyle = BadgeAppearance.Primary,

            IsExpanded = true,

            IsChecked = true,

            Children = new List<DemoTreeNode>

            {

                new DemoTreeNode

                {

                    Key = "LEVEL1",

                    Name = "Level 1 [Elev. +0.000m]",

                    Details = "Ground Floor Commercial Lobby",

                    Icon = IconKind.Layers,

                    IconBrush = new SolidColorBrush(Color.FromRgb(37, 99, 235)), // Blue

                    Tag = "12 Elements",

                    BadgeStyle = BadgeAppearance.Neutral,

                    IsExpanded = true,

                    IsChecked = true,

                    Children = new List<DemoTreeNode>

                    {

                        new DemoTreeNode { Key = "D1", Name = "Main Girder D1 (300x600 - 4D20)", Details = "Span 6.5m, Concrete B25, Rebar 4D20+2D18", Icon = IconKind.Ruler, IconBrush = new SolidColorBrush(Color.FromRgb(5, 150, 105)), Tag = "Approved", BadgeStyle = BadgeAppearance.Success, IsChecked = true },

                        new DemoTreeNode { Key = "D2", Name = "Secondary Beam D2 (200x400 - 3D16)", Details = "Span 4.2m, Concrete B25, Stirrup d8@150", Icon = IconKind.Ruler, IconBrush = new SolidColorBrush(Color.FromRgb(5, 150, 105)), Tag = "In Review", BadgeStyle = BadgeAppearance.Warning, IsChecked = true },

                        new DemoTreeNode { Key = "C1", Name = "Columns C1-C8 (600x600 - 8D25)", Details = "Tied Column, 3.8m clear, Rebar 8D25", Icon = IconKind.Layers, IconBrush = new SolidColorBrush(Color.FromRgb(37, 99, 235)), Tag = "Verified", BadgeStyle = BadgeAppearance.Success, IsChecked = true },

                        new DemoTreeNode { Key = "S1", Name = "Floor Slab S1 (h=150mm)", Details = "Two-way slab, 2-layer mesh d10@150", Icon = IconKind.File, IconBrush = new SolidColorBrush(Color.FromRgb(100, 116, 139)), Tag = "Approved", BadgeStyle = BadgeAppearance.Success, IsChecked = true }

                    }

                },

                new DemoTreeNode

                {

                    Key = "LEVEL2",

                    Name = "Level 2 [Elev. +3.900m]",

                    Details = "First Floor Executive Office Suites",

                    Icon = IconKind.Layers,

                    IconBrush = new SolidColorBrush(Color.FromRgb(37, 99, 235)),

                    Tag = "14 Elements",

                    BadgeStyle = BadgeAppearance.Neutral,

                    IsExpanded = false,

                    IsChecked = false,

                    Children = new List<DemoTreeNode>

                    {

                        new DemoTreeNode { Key = "TG1", Name = "Transfer Girder TG1 (500x1200 - 8D28)", Details = "Heavy load transfer, 9.0m span, Rebar 8D28", Icon = IconKind.Ruler, IconBrush = new SolidColorBrush(Color.FromRgb(220, 38, 38)), Tag = "Critical", BadgeStyle = BadgeAppearance.Danger, IsChecked = false },

                        new DemoTreeNode { Key = "D11", Name = "Perimeter Beam D11 (250x500 - 4D18)", Details = "Facade edge beam, Rebar 4D18", Icon = IconKind.Ruler, IconBrush = new SolidColorBrush(Color.FromRgb(5, 150, 105)), Tag = "Pending", BadgeStyle = BadgeAppearance.Neutral, IsChecked = false },

                        new DemoTreeNode { Key = "C9", Name = "Columns C9-C16 (500x500 - 8D22)", Details = "Floor 2 columns, 3.9m clear", Icon = IconKind.Layers, IconBrush = new SolidColorBrush(Color.FromRgb(37, 99, 235)), Tag = "Approved", BadgeStyle = BadgeAppearance.Success, IsChecked = false }

                    }

                },

                new DemoTreeNode

                {

                    Key = "DOCS",

                    Name = "Engineering Documents & Models",

                    Details = "BIM models, CAD layouts and calculation reports",

                    Icon = IconKind.Folder,

                    IconBrush = new SolidColorBrush(Color.FromRgb(217, 119, 6)),

                    Tag = "4 Files",

                    BadgeStyle = BadgeAppearance.Neutral,

                    IsExpanded = false,

                    IsChecked = false,

                    Children = new List<DemoTreeNode>

                    {

                        new DemoTreeNode { Key = "CAD1", Name = "ST-01_Foundations_Layout.dwg", Details = "AutoCAD 2026 drawing, 14.2 MB", Icon = IconKind.File, IconBrush = new SolidColorBrush(Color.FromRgb(2, 132, 199)), Tag = "CAD", BadgeStyle = BadgeAppearance.Primary, IsChecked = false },

                        new DemoTreeNode { Key = "CAD2", Name = "ST-02_Columns_Reinforcement.dwg", Details = "AutoCAD 2026 drawing, 9.8 MB", Icon = IconKind.File, IconBrush = new SolidColorBrush(Color.FromRgb(2, 132, 199)), Tag = "CAD", BadgeStyle = BadgeAppearance.Primary, IsChecked = false },

                        new DemoTreeNode { Key = "BIM1", Name = "Structural_Tower_LOD350.ifc", Details = "IFC 4x3 coordination model, 88.5 MB", Icon = IconKind.Cube, IconBrush = new SolidColorBrush(Color.FromRgb(147, 51, 234)), Tag = "BIM", BadgeStyle = BadgeAppearance.Primary, IsChecked = false },

                        new DemoTreeNode { Key = "CALC", Name = "Seismic_Calculation_Note.pdf", Details = "Eurocode 8 / TCVN 9386 analysis, 3.4 MB", Icon = IconKind.FileText, IconBrush = new SolidColorBrush(Color.FromRgb(220, 38, 38)), Tag = "PDF", BadgeStyle = BadgeAppearance.Danger, IsChecked = false }

                    }

                }

            }

        };

        var projectData = CreateInitialProjectData();

        // --- Studio State ---

        string currentStyleKey = "CONCOST.TreeView.Base";

        string currentMode = "Badges"; // "Badges", "Checkboxes", "Files", "Simple"

        string filterQuery = "";

        int currentCornerRadius = 8;

        bool showBorder = true;

        string bgMode = "Surface"; // "Surface", "Secondary", "Transparent"

        // ==================== 2. Left Column: Live TreeView Container ====================

        var leftCol = new StackPanel { Width = 400, Margin = new System.Windows.Thickness(0) };

        // Top Header of Preview

        var headerDock = new DockPanel { Margin = new System.Windows.Thickness(0, 0, 0, 8) };

        var lblPreview = new TextBlock

        {

            Text = Loc.T("CÂY PHÂN CẤP DỰ ÁN (LIVE PREVIEW)", "LIVE TREEVIEW PREVIEW"),

            FontSize = 11,

            FontWeight = System.Windows.FontWeights.Bold,

            VerticalAlignment = System.Windows.VerticalAlignment.Center

        };

        lblPreview.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");

        DockPanel.SetDock(lblPreview, Dock.Left);

        headerDock.Children.Add(lblPreview);

        var badgeCurrentStyle = new Badge

        {

            Style = (System.Windows.Style)App.Current.Resources["CONCOST.Badge.Soft"],

            Content = "Base (Standard)",

            Appearance = BadgeAppearance.Primary,

            HorizontalAlignment = System.Windows.HorizontalAlignment.Right,

            FontSize = 10.5

        };

        headerDock.Children.Add(badgeCurrentStyle);

        leftCol.Children.Add(headerDock);

        // Search Filter Box

        var txtSearch = new TextBox

        {

            Height = 32,

            Margin = new System.Windows.Thickness(0, 0, 0, 8),

            FontSize = 12

        };

        InfoElement.SetPlaceholder(txtSearch, Loc.T("Tìm kiếm cấu kiện, tầng, hồ sơ...", "Search members, levels, documents..."));

        leftCol.Children.Add(txtSearch);

        // The Single Live TreeView Control

        var liveTree = new TreeView

        {

            Height = 360,

            Width = 400,

            Margin = new System.Windows.Thickness(0, 0, 0, 8),

            HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch,

            VerticalAlignment = System.Windows.VerticalAlignment.Stretch

        };

        leftCol.Children.Add(liveTree);

        // Status Inspector Bar (Selected Item Telemetry)

        var statusBorder = new Border

        {

            Padding = new System.Windows.Thickness(10, 8, 10, 8),

            CornerRadius = new System.Windows.CornerRadius(6),

            BorderThickness = new System.Windows.Thickness(1),

            Margin = new System.Windows.Thickness(0, 0, 0, 8)

        };

        statusBorder.SetResourceReference(Border.BackgroundProperty, "CONCOST.Brush.SurfaceSecondary");

        statusBorder.SetResourceReference(Border.BorderBrushProperty, "CONCOST.Brush.Border");

        var statusSp = new StackPanel();

        var lblSelTitle = new TextBlock

        {

            Text = Loc.T("Đang chọn: (Chưa chọn cấu kiện)", "Selected: (None)"),

            FontSize = 12,

            FontWeight = System.Windows.FontWeights.SemiBold

        };

        lblSelTitle.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.Primary");

        var lblSelDetails = new TextBlock

        {

            Text = Loc.T("Nhấp vào node bất kỳ để kiểm tra thuộc tính chi tiết", "Click any node in the tree to inspect details"),

            FontSize = 11,

            TextWrapping = System.Windows.TextWrapping.Wrap,

            Margin = new System.Windows.Thickness(0, 2, 0, 0)

        };

        lblSelDetails.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");

        statusSp.Children.Add(lblSelTitle);

        statusSp.Children.Add(lblSelDetails);

        statusBorder.Child = statusSp;

        leftCol.Children.Add(statusBorder);

        // Action Buttons Row (Add dynamic member & Reset)

        var actionGrid = new UniformGrid { Columns = 2, Margin = new System.Windows.Thickness(0) };

        var btnAddNode = new Button

        {

            Content = Loc.T("➕ Thêm dầm mẫu D3", "➕ Add Girder D3"),

            Style = (System.Windows.Style)App.Current.Resources["CONCOST.Button.Secondary"],

            Margin = new System.Windows.Thickness(0, 0, 4, 0),

            Height = 30

        };

        var btnReset = new Button

        {

            Content = Loc.T("↺ Khôi phục gốc", "↺ Reset Tree"),

            Style = (System.Windows.Style)App.Current.Resources["CONCOST.Button.Secondary"],

            Margin = new System.Windows.Thickness(4, 0, 0, 0),

            Height = 30

        };

        actionGrid.Children.Add(btnAddNode);

        actionGrid.Children.Add(btnReset);

        leftCol.Children.Add(actionGrid);

        // ==================== 3. Tree Population & Style Application Logic ====================

        void ApplyStylesToTree()

        {

            liveTree.SetResourceReference(System.Windows.FrameworkElement.StyleProperty, currentStyleKey);

            BorderElement.SetCornerRadius(liveTree, new System.Windows.CornerRadius(currentCornerRadius));

            liveTree.BorderThickness = showBorder ? new System.Windows.Thickness(1) : new System.Windows.Thickness(0);

            if (bgMode == "Surface")

                liveTree.SetResourceReference(Control.BackgroundProperty, "CONCOST.Brush.Surface");

            else if (bgMode == "Secondary")

                liveTree.SetResourceReference(Control.BackgroundProperty, "CONCOST.Brush.SurfaceSecondary");

            else

                liveTree.Background = Brushes.Transparent;

            Style? itemStyle = null;

            if (currentStyleKey == "CONCOST.TreeView.Line")

            {

                if (App.Current.Resources["CONCOST.TreeView.Line"] is Style treeStyle)

                {

                    var setter = treeStyle.Setters.OfType<Setter>().FirstOrDefault(s => s.Property == ItemsControl.ItemContainerStyleProperty);

                    itemStyle = setter?.Value as Style;

                }

            }

            else if (currentStyleKey == "CONCOST.ProjectBrowser.TreeClassic")

            {

                itemStyle = App.Current.Resources["CONCOST.ProjectBrowser.ItemClassic"] as Style;

            }

            void ApplyStyleRecursive(ItemCollection items)

            {

                foreach (var obj in items)

                {

                    if (obj is TreeViewItem tvi)

                    {

                        if (itemStyle != null)

                            tvi.Style = itemStyle;

                        else

                            tvi.ClearValue(System.Windows.FrameworkElement.StyleProperty);

                        ApplyStyleRecursive(tvi.Items);

                    }

                }

            }

            ApplyStyleRecursive(liveTree.Items);

        }

        void RebuildTree()

        {

            liveTree.Items.Clear();

            bool NodeMatchesFilter(DemoTreeNode node, string query)

            {

                if (string.IsNullOrWhiteSpace(query)) return true;

                if (node.Name.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0) return true;

                if (!string.IsNullOrEmpty(node.Details) && node.Details.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0) return true;

                if (!string.IsNullOrEmpty(node.Tag) && node.Tag.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0) return true;

                return node.Children.Any(c => NodeMatchesFilter(c, query));

            }

            TreeViewItem? CreateTvi(DemoTreeNode node)

            {

                if (!NodeMatchesFilter(node, filterQuery)) return null;

                var tvi = new TreeViewItem

                {

                    Tag = node,

                    IsExpanded = !string.IsNullOrWhiteSpace(filterQuery) || node.IsExpanded

                };

                UIElement headerContent;

                if (currentMode == "Checkboxes")

                {

                    var cb = new CheckBox

                    {

                        Content = node.Name,

                        IsChecked = node.IsChecked,

                        IsThreeState = node.Children.Count > 0,

                        VerticalAlignment = System.Windows.VerticalAlignment.Center,

                        FontSize = 12.5,

                        Margin = new System.Windows.Thickness(2, 0, 0, 0)

                    };

                    cb.Click += (s, e) =>

                    {

                        node.IsChecked = cb.IsChecked;

                        if (node.Children.Count > 0 && cb.IsChecked.HasValue)

                        {

                            void SetChildrenChecked(DemoTreeNode parent, bool isChecked)

                            {

                                foreach (var child in parent.Children)

                                {

                                    child.IsChecked = isChecked;

                                    SetChildrenChecked(child, isChecked);

                                }

                            }

                            SetChildrenChecked(node, cb.IsChecked.Value);

                        }

                        RebuildTree();

                    };

                    headerContent = cb;

                }

                else if (currentMode == "Files")

                {

                    var sp = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = System.Windows.VerticalAlignment.Center };

                    var iconKind = node.Children.Count > 0 ? IconKind.Folder : node.Icon;

                    var ico = new Icon { Kind = iconKind, Width = 15, Height = 15, Margin = new System.Windows.Thickness(0, 0, 8, 0), VerticalAlignment = System.Windows.VerticalAlignment.Center, Foreground = node.IconBrush };

                    sp.Children.Add(ico);

                    var txt = new TextBlock { Text = node.Name, FontSize = 12.5, VerticalAlignment = System.Windows.VerticalAlignment.Center };

                    sp.Children.Add(txt);

                    if (!string.IsNullOrEmpty(node.Tag))

                    {

                        var tagText = new TextBlock { Text = $"({node.Tag})", FontSize = 11, Margin = new System.Windows.Thickness(6, 0, 0, 0), VerticalAlignment = System.Windows.VerticalAlignment.Center };

                        tagText.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");

                        sp.Children.Add(tagText);

                    }

                    headerContent = sp;

                }

                else if (currentMode == "Simple")

                {

                    headerContent = new TextBlock { Text = node.Name, FontSize = 12.5, VerticalAlignment = System.Windows.VerticalAlignment.Center };

                }

                else // "Badges" (Default rich engineering mode)

                {

                    var sp = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = System.Windows.VerticalAlignment.Center };

                    var ico = new Icon { Kind = node.Icon, Width = 15, Height = 15, Margin = new System.Windows.Thickness(0, 0, 8, 0), VerticalAlignment = System.Windows.VerticalAlignment.Center, Foreground = node.IconBrush };

                    sp.Children.Add(ico);

                    var txt = new TextBlock { Text = node.Name, FontSize = 12.5, VerticalAlignment = System.Windows.VerticalAlignment.Center };

                    sp.Children.Add(txt);

                    if (!string.IsNullOrEmpty(node.Tag))

                    {

                        var badge = new Badge

                        {

                            Style = (System.Windows.Style)App.Current.Resources["CONCOST.Badge.Soft"],

                            Content = node.Tag,

                            Appearance = node.BadgeStyle,

                            Margin = new System.Windows.Thickness(10, 0, 0, 0),

                            Padding = new System.Windows.Thickness(6, 1.5, 6, 1.5),

                            FontSize = 10.5

                        };

                        sp.Children.Add(badge);

                    }

                    headerContent = sp;

                }

                tvi.Header = headerContent;

                foreach (var child in node.Children)

                {

                    var childTvi = CreateTvi(child);

                    if (childTvi != null)

                        tvi.Items.Add(childTvi);

                }

                return tvi;

            }

            var rootTvi = CreateTvi(projectData);

            if (rootTvi != null)

                liveTree.Items.Add(rootTvi);

            ApplyStylesToTree();

        }

        // Selection Change Handler

        liveTree.SelectedItemChanged += (s, e) =>

        {

            if (liveTree.SelectedItem is TreeViewItem selectedTvi && selectedTvi.Tag is DemoTreeNode selNode)

            {

                lblSelTitle.Text = $"Selected: {selNode.Name}";

                lblSelDetails.Text = $"Details: {selNode.Details} | Tag: {selNode.Tag ?? "None"} | Sub-elements: {selNode.Children.Count}";

            }

            else

            {

                lblSelTitle.Text = Loc.T("Đang chọn: (Chưa chọn cấu kiện)", "Selected: (None)");

                lblSelDetails.Text = Loc.T("Nhấp vào node bất kỳ để kiểm tra thuộc tính chi tiết", "Click any node in the tree to inspect details");

            }

        };

        // Search Filter input

        txtSearch.TextChanged += (s, e) =>

        {

            filterQuery = txtSearch.Text;

            RebuildTree();

        };

        // Add Member Button

        btnAddNode.Click += (s, e) =>

        {

            var level1 = projectData.Children.FirstOrDefault(c => c.Key == "LEVEL1");

            if (level1 != null)

            {

                int nextNum = level1.Children.Count + 1;

                var newBeam = new DemoTreeNode

                {

                    Key = $"D_NEW_{nextNum}",

                    Name = $"Secondary Beam D{nextNum} (250x450 - 4D18)",

                    Details = $"Span 4.8m, Concrete B25, Added dynamically at {DateTime.Now:HH:mm:ss}",

                    Icon = IconKind.Ruler,

                    IconBrush = new SolidColorBrush(Color.FromRgb(5, 150, 105)),

                    Tag = "Draft",

                    BadgeStyle = BadgeAppearance.Warning,

                    IsChecked = true

                };

                level1.Children.Add(newBeam);

                level1.IsExpanded = true;

                projectData.IsExpanded = true;

                RebuildTree();

                ToastService.Success($"Added '{newBeam.Name}' to Level 1", "TreeView Studio");

            }

        };

        // Reset Tree Button

        btnReset.Click += (s, e) =>

        {

            txtSearch.Text = "";

            filterQuery = "";

            currentStyleKey = "CONCOST.TreeView.Base";

            currentMode = "Badges";

            currentCornerRadius = 8;

            showBorder = true;

            bgMode = "Surface";

            projectData = CreateInitialProjectData();

            RebuildTree();

            ToastService.Info("TreeView restored to default state", "TreeView Studio");

        };

        // ==================== 4. Right Column: Comprehensive Control Options Studio ====================

        var rightCol = new StackPanel { Margin = new System.Windows.Thickness(0) };

        // --- Subsection A: Style Variant ---

        rightCol.Children.Add(SectionSubhead(Loc.T("KIỂU DÁNG TREEVIEW (STYLE VARIANT):", "TREEVIEW STYLE VARIANT:")));

        var styleGrid = new UniformGrid { Columns = 3, Margin = new System.Windows.Thickness(0, 0, 0, 12) };

        var styleList = new[]

        {

            ("Base (Chuẩn)", "CONCOST.TreeView.Base"),

            ("Line (Đường nối)", "CONCOST.TreeView.Line"),

            ("Directory (Thư mục)", "CONCOST.TreeView.Directory"),

            ("Card (Thẻ khối)", "CONCOST.TreeView.Card"),

            ("Modern Pill", "CONCOST.TreeView.ModernPill"),

            ("Revit Classic", "CONCOST.ProjectBrowser.TreeClassic")

        };

        var styleButtons = new Dictionary<string, Button>();

        void UpdateStyleButtons()

        {

            foreach (var kvp in styleButtons)

            {

                bool isActive = kvp.Key == currentStyleKey;

                kvp.Value.Style = (System.Windows.Style)App.Current.Resources[isActive ? "CONCOST.Button.Primary" : "CONCOST.Button.Secondary"];

            }

            var activeTuple = styleList.FirstOrDefault(s => s.Item2 == currentStyleKey);

            badgeCurrentStyle.Content = activeTuple.Item1 ?? currentStyleKey;

        }

        foreach (var (lbl, key) in styleList)

        {

            var btn = new Button

            {

                Content = lbl,

                Height = 32,

                Margin = new System.Windows.Thickness(2),

                FontSize = 11.5

            };

            btn.Click += (s, e) =>

            {

                currentStyleKey = key;

                UpdateStyleButtons();

                ApplyStylesToTree();

            };

            styleButtons[key] = btn;

            styleGrid.Children.Add(btn);

        }

        rightCol.Children.Add(styleGrid);

        // --- Subsection B: Node Content Mode ---

        rightCol.Children.Add(SectionSubhead(Loc.T("CHẾ ĐỘ HIỂN THỊ NODE (NODE CONTENT MODE):", "NODE CONTENT MODE:")));

        var modeGrid = new UniformGrid { Columns = 2, Margin = new System.Windows.Thickness(0, 0, 0, 12) };

        var modeList = new[]

        {

            ("🏷️ Engineering Badges", "Badges"),

            ("☑️ CheckBox Multi-Select", "Checkboxes"),

            ("📁 File Explorer", "Files"),

            ("📝 Simple Text", "Simple")

        };

        var modeButtons = new Dictionary<string, Button>();

        void UpdateModeButtons()

        {

            foreach (var kvp in modeButtons)

            {

                bool isActive = kvp.Key == currentMode;

                kvp.Value.Style = (System.Windows.Style)App.Current.Resources[isActive ? "CONCOST.Button.Primary" : "CONCOST.Button.Secondary"];

            }

        }

        foreach (var (lbl, modeKey) in modeList)

        {

            var btn = new Button

            {

                Content = lbl,

                Height = 32,

                Margin = new System.Windows.Thickness(2),

                FontSize = 11.5

            };

            btn.Click += (s, e) =>

            {

                currentMode = modeKey;

                UpdateModeButtons();

                RebuildTree();

            };

            modeButtons[modeKey] = btn;

            modeGrid.Children.Add(btn);

        }

        rightCol.Children.Add(modeGrid);

        // --- Subsection C: Expansion & Drill-down Actions ---

        rightCol.Children.Add(SectionSubhead(Loc.T("THAO TÁC MỞ RỘNG (EXPANSION ACTIONS):", "EXPANSION ACTIONS:")));

        var expandGrid = new UniformGrid { Columns = 2, Margin = new System.Windows.Thickness(0, 0, 0, 12) };

        void SetExpandedRecursive(ItemCollection items, bool isExpanded)

        {

            foreach (var obj in items)

            {

                if (obj is TreeViewItem tvi)

                {

                    tvi.IsExpanded = isExpanded;

                    if (tvi.Tag is DemoTreeNode dn) dn.IsExpanded = isExpanded;

                    SetExpandedRecursive(tvi.Items, isExpanded);

                }

            }

        }

        var btnExpandAll = new Button { Content = Loc.T("📂 Mở rộng tất cả", "📂 Expand All"), Style = (System.Windows.Style)App.Current.Resources["CONCOST.Button.Secondary"], Height = 30, Margin = new System.Windows.Thickness(2), FontSize = 11.5 };

        btnExpandAll.Click += (s, e) => SetExpandedRecursive(liveTree.Items, true);

        var btnCollapseAll = new Button { Content = Loc.T("📁 Thu gọn tất cả", "📁 Collapse All"), Style = (System.Windows.Style)App.Current.Resources["CONCOST.Button.Secondary"], Height = 30, Margin = new System.Windows.Thickness(2), FontSize = 11.5 };

        btnCollapseAll.Click += (s, e) => SetExpandedRecursive(liveTree.Items, false);

        var btnLevel1 = new Button { Content = Loc.T("1️⃣ Mở Cấp 1 (Root Only)", "1️⃣ Level 1 (Root Only)"), Style = (System.Windows.Style)App.Current.Resources["CONCOST.Button.Secondary"], Height = 30, Margin = new System.Windows.Thickness(2), FontSize = 11.5 };

        btnLevel1.Click += (s, e) =>

        {

            SetExpandedRecursive(liveTree.Items, false);

            if (liveTree.Items.Count > 0 && liveTree.Items[0] is TreeViewItem rootTvi)

                rootTvi.IsExpanded = true;

        };

        var btnLevel2 = new Button { Content = Loc.T("2️⃣ Mở Cấp 2 (Floors / Groups)", "2️⃣ Level 2 (Floors / Groups)"), Style = (System.Windows.Style)App.Current.Resources["CONCOST.Button.Secondary"], Height = 30, Margin = new System.Windows.Thickness(2), FontSize = 11.5 };

        btnLevel2.Click += (s, e) =>

        {

            SetExpandedRecursive(liveTree.Items, false);

            if (liveTree.Items.Count > 0 && liveTree.Items[0] is TreeViewItem rootTvi)

            {

                rootTvi.IsExpanded = true;

                foreach (var child in rootTvi.Items)

                {

                    if (child is TreeViewItem childTvi) childTvi.IsExpanded = true;

                }

            }

        };

        expandGrid.Children.Add(btnExpandAll);

        expandGrid.Children.Add(btnCollapseAll);

        expandGrid.Children.Add(btnLevel1);

        expandGrid.Children.Add(btnLevel2);

        rightCol.Children.Add(expandGrid);

        // --- Subsection D: Attached Properties & Tokens ---

        rightCol.Children.Add(SectionSubhead(Loc.T("THUỘC TÍNH MỞ RỘNG & GIAO DIỆN (ATTACHED PROPERTIES):", "ATTACHED PROPERTIES & APPEARANCE:")));

        // 1. CornerRadius attached property

        var radiusDock = new DockPanel { Margin = new System.Windows.Thickness(0, 0, 0, 8) };

        var lblRadius = new TextBlock { Text = Loc.T("Bo góc (CornerRadius):", "Corner Radius:"), Width = 145, VerticalAlignment = System.Windows.VerticalAlignment.Center, FontSize = 12 };

        lblRadius.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");

        DockPanel.SetDock(lblRadius, Dock.Left);

        radiusDock.Children.Add(lblRadius);

        var radiusGrid = new UniformGrid { Columns = 4 };

        var radiusList = new[] { 0, 4, 8, 12 };

        var radiusButtons = new Dictionary<int, Button>();

        void UpdateCornerRadiusButtons()

        {

            foreach (var kvp in radiusButtons)

            {

                bool isActive = kvp.Key == currentCornerRadius;

                kvp.Value.Style = (System.Windows.Style)App.Current.Resources[isActive ? "CONCOST.Button.Primary" : "CONCOST.Button.Secondary"];

            }

        }

        foreach (var r in radiusList)

        {

            var btnR = new Button { Content = $"{r}px", Height = 28, Margin = new System.Windows.Thickness(2), FontSize = 11 };

            btnR.Click += (s, e) =>

            {

                currentCornerRadius = r;

                UpdateCornerRadiusButtons();

                ApplyStylesToTree();

            };

            radiusButtons[r] = btnR;

            radiusGrid.Children.Add(btnR);

        }

        radiusDock.Children.Add(radiusGrid);

        rightCol.Children.Add(radiusDock);

        // 2. Background mode

        var bgDock = new DockPanel { Margin = new System.Windows.Thickness(0, 0, 0, 8) };

        var lblBg = new TextBlock { Text = Loc.T("Màu nền (Background):", "Background:"), Width = 145, VerticalAlignment = System.Windows.VerticalAlignment.Center, FontSize = 12 };

        lblBg.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");

        DockPanel.SetDock(lblBg, Dock.Left);

        bgDock.Children.Add(lblBg);

        var bgGrid = new UniformGrid { Columns = 3 };

        var bgList = new[]

        {

            ("Surface", "Surface"),

            ("Secondary", "Secondary"),

            ("None", "Transparent")

        };

        var bgButtons = new Dictionary<string, Button>();

        void UpdateBgButtons()

        {

            foreach (var kvp in bgButtons)

            {

                bool isActive = kvp.Key == bgMode;

                kvp.Value.Style = (System.Windows.Style)App.Current.Resources[isActive ? "CONCOST.Button.Primary" : "CONCOST.Button.Secondary"];

            }

        }

        foreach (var (lbl, key) in bgList)

        {

            var btnBg = new Button { Content = lbl, Height = 28, Margin = new System.Windows.Thickness(2), FontSize = 11 };

            btnBg.Click += (s, e) =>

            {

                bgMode = key;

                UpdateBgButtons();

                ApplyStylesToTree();

            };

            bgButtons[key] = btnBg;

            bgGrid.Children.Add(btnBg);

        }

        bgDock.Children.Add(bgGrid);

        rightCol.Children.Add(bgDock);

        // 3. Checkboxes for Border & Virtualization

        var flagGrid = new UniformGrid { Columns = 2, Margin = new System.Windows.Thickness(0, 4, 0, 4) };

        var chkBorder = new CheckBox

        {

            Content = Loc.T("Viền khung (Border)", "Show Border"),

            IsChecked = true,

            FontSize = 12,

            VerticalAlignment = System.Windows.VerticalAlignment.Center

        };

        chkBorder.Click += (s, e) =>

        {

            showBorder = chkBorder.IsChecked == true;

            ApplyStylesToTree();

        };

        var chkVirtual = new CheckBox

        {

            Content = Loc.T("Ảo hóa cuộn (Virtualizing)", "Virtualization"),

            IsChecked = true,

            FontSize = 12,

            VerticalAlignment = System.Windows.VerticalAlignment.Center

        };

        chkVirtual.Click += (s, e) =>

        {

            VirtualizingPanel.SetIsVirtualizing(liveTree, chkVirtual.IsChecked == true);

        };

        flagGrid.Children.Add(chkBorder);

        flagGrid.Children.Add(chkVirtual);

        rightCol.Children.Add(flagGrid);

        // ==================== 5. Assemble Grid Studio (2-Column OCD-friendly layout) ====================

        var studioGrid = new Grid { Margin = new System.Windows.Thickness(0) };

        studioGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new System.Windows.GridLength(400, System.Windows.GridUnitType.Pixel) });

        studioGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new System.Windows.GridLength(20, System.Windows.GridUnitType.Pixel) });

        studioGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new System.Windows.GridLength(1, System.Windows.GridUnitType.Star) });

        var vDivider = new Border

        {

            Width = 1,

            HorizontalAlignment = System.Windows.HorizontalAlignment.Center,

            VerticalAlignment = System.Windows.VerticalAlignment.Stretch,

            Margin = new System.Windows.Thickness(0, 4, 0, 4)

        };

        vDivider.SetResourceReference(Border.BackgroundProperty, "CONCOST.Brush.Border");

        Grid.SetColumn(vDivider, 1);

        studioGrid.Children.Add(vDivider);

        Grid.SetColumn(leftCol, 0);

        studioGrid.Children.Add(leftCol);

        Grid.SetColumn(rightCol, 2);

        studioGrid.Children.Add(rightCol);

        // Initial Button Highlights & Tree Build

        UpdateStyleButtons();

        UpdateModeButtons();

        UpdateCornerRadiusButtons();

        UpdateBgButtons();

        RebuildTree();

        // Code Inspector Snippets

        string xamlStudioCode =

@"<!-- CONCOST.UI TreeView Studio with Attached Properties & Complete Style Variants -->

<TreeView Style=""{DynamicResource CONCOST.TreeView.Base}""

          attached:BorderElement.CornerRadius=""8""

          BorderThickness=""1""

          VirtualizingPanel.IsVirtualizing=""True""

          VirtualizingPanel.ScrollUnit=""Pixel""

          Width=""400"" Height=""360"">

    <!-- 1. Root Level Project Node -->

    <TreeViewItem Header=""Project: CONCOST Landmark Tower"" IsExpanded=""True"">

        <!-- 2. Floor Hierarchy with Vector Icon and Status Badge -->

        <TreeViewItem IsExpanded=""True"">

            <TreeViewItem.Header>

                <StackPanel Orientation=""Horizontal"">

                    <icons:Icon Kind=""Layers"" Width=""15"" Height=""15"" Foreground=""#2563EB"" Margin=""0,0,8,0""/>

                    <TextBlock Text=""Level 1 [Elev. +0.000m]"" VerticalAlignment=""Center""/>

                    <controls:Badge Style=""{DynamicResource CONCOST.Badge.Soft}""

                                    Content=""12 Elements"" Appearance=""Neutral"" Margin=""10,0,0,0""/>

                </StackPanel>

            </TreeViewItem.Header>

            <!-- 3. Structural Member Node with Status -->

            <TreeViewItem>

                <TreeViewItem.Header>

                    <StackPanel Orientation=""Horizontal"">

                        <icons:Icon Kind=""Ruler"" Width=""15"" Height=""15"" Foreground=""#059669"" Margin=""0,0,8,0""/>

                        <TextBlock Text=""Main Girder D1 (300x600 - 4D20)"" VerticalAlignment=""Center""/>

                        <controls:Badge Style=""{DynamicResource CONCOST.Badge.Soft}""

                                        Content=""Approved"" Appearance=""Success"" Margin=""10,0,0,0""/>

                    </StackPanel>

                </TreeViewItem.Header>

            </TreeViewItem>

        </TreeViewItem>

    </TreeViewItem>

</TreeView>

<!-- Style Variants Available in Theme:

     • Style=""{DynamicResource CONCOST.TreeView.Base}""           (Clean modern surface, chevron expander)

     • Style=""{DynamicResource CONCOST.TreeView.Line}""           (Dashed hierarchy connector lines)

     • Style=""{DynamicResource CONCOST.TreeView.Directory}""      (Compact file explorer style)

     • Style=""{DynamicResource CONCOST.TreeView.Card}""           (10px corner card presentation)

     • Style=""{DynamicResource CONCOST.TreeView.ModernPill}""     (Clean borderless with pill selection)

     • Style=""{DynamicResource CONCOST.ProjectBrowser.TreeClassic}"" (Revit / Windows Explorer [+] / [-])

-->";

        string csStudioCode =

@"// Programmatic Style & Attached Property Configuration

myTreeView.SetResourceReference(FrameworkElement.StyleProperty, ""CONCOST.TreeView.Line"");

BorderElement.SetCornerRadius(myTreeView, new CornerRadius(8));

VirtualizingPanel.SetIsVirtualizing(myTreeView, true);

VirtualizingPanel.SetScrollUnit(myTreeView, ScrollUnit.Pixel);

// Recursive Expansion Helpers

void ExpandAll(ItemsControl container)

{

    foreach (var item in container.Items)

    {

        if (container.ItemContainerGenerator.ContainerFromItem(item) is TreeViewItem tvi)

        {

            tvi.IsExpanded = true;

            ExpandAll(tvi);

        }

    }

}

// Selection Tracking

myTreeView.SelectedItemChanged += (s, e) =>

{

    if (myTreeView.SelectedItem is TreeViewItem selectedNode)

    {

        Console.WriteLine($""Selected element: {selectedNode.Header}"");

    }

};";

        AddCardSection(Loc.T("Bảng điều khiển & Tùy biến TreeView (Interactive TreeView Studio)", "Interactive TreeView Studio & Property Inspector"), studioGrid, xamlStudioCode, csStudioCode);

        return mainSp;

    }
}
