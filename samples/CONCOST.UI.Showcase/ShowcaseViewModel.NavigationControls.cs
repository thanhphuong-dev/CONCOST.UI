using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using CONCOST.UI.Controls;
using CONCOST.UI.Controls.Dashboard;
using CONCOST.UI.AttachedProperties;
using CONCOST.UI.Icons;
using CONCOST.UI.Services;

namespace CONCOST.UI.Showcase;

public partial class ShowcaseViewModel
{
    // =========================================================================
    // DEMO: SplitButton
    // =========================================================================
    private object CreateSplitButtonDemo()
    {
        var mainSp = new StackPanel { Margin = new Thickness(0) };

        void AddCardSection(string title, UIElement content, string? codeSnippet = null)
        {
            var card = new Border
            {
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(20),
                Margin = new Thickness(0, 0, 0, 18)
            };
            card.SetResourceReference(Border.BackgroundProperty, "CONCOST.Brush.Surface");
            card.SetResourceReference(Border.BorderBrushProperty, "CONCOST.Brush.Border");

            var sp = new StackPanel();
            var headerGrid = new Grid { Margin = new Thickness(0, 0, 0, 14) };
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var titleBlock = new TextBlock
            {
                Text = title,
                FontWeight = FontWeights.Normal,
                FontSize = 14,
                VerticalAlignment = VerticalAlignment.Center
            };
            titleBlock.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextPrimary");
            Grid.SetColumn(titleBlock, 0);
            headerGrid.Children.Add(titleBlock);

            Border? codeBox = null;
            if (!string.IsNullOrEmpty(codeSnippet))
            {
                var codeBtn = new Button
                {
                    Content = "View XAML / C#",
                    Padding = new Thickness(10, 4, 10, 4),
                    FontSize = 12,
                    Cursor = Cursors.Hand
                };
                codeBtn.SetResourceReference(Button.StyleProperty, "CONCOST.Button.Secondary");
                CONCOST.UI.AttachedProperties.IconElement.SetKind(codeBtn, IconKind.Edit);
                CONCOST.UI.AttachedProperties.BorderElement.SetCornerRadius(codeBtn, new CornerRadius(6));
                Grid.SetColumn(codeBtn, 1);
                headerGrid.Children.Add(codeBtn);

                codeBox = new Border
                {
                    CornerRadius = new CornerRadius(8),
                    BorderThickness = new Thickness(1),
                    Margin = new Thickness(0, 16, 0, 0),
                    Visibility = Visibility.Collapsed
                };
                codeBox.SetResourceReference(Border.BackgroundProperty, "CONCOST.Brush.SurfaceSecondary");
                codeBox.SetResourceReference(Border.BorderBrushProperty, "CONCOST.Brush.Border");

                var codeBoxInner = new StackPanel();
                var codeTopBar = new Grid { Height = 34, Margin = new Thickness(12, 0, 8, 0) };
                codeTopBar.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                codeTopBar.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                var langBadge = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
                var tagIcon = new CONCOST.UI.Icons.Icon { Kind = IconKind.Edit, Width = 14, Height = 14, Margin = new Thickness(0, 0, 6, 0) };
                tagIcon.SetResourceReference(CONCOST.UI.Icons.Icon.ForegroundProperty, "CONCOST.Brush.TextSecondary");
                var langLabel = new TextBlock { Text = "XAML & C# Code-Behind", FontSize = 11.5, FontWeight = FontWeights.Normal };
                langLabel.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");
                langBadge.Children.Add(tagIcon);
                langBadge.Children.Add(langLabel);
                Grid.SetColumn(langBadge, 0);
                codeTopBar.Children.Add(langBadge);

                var copyBtn = new Button { Content = "Copy", Padding = new Thickness(10, 2, 10, 2), FontSize = 11.5, Cursor = Cursors.Hand };
                copyBtn.SetResourceReference(Button.StyleProperty, "CONCOST.Button.Secondary");
                CONCOST.UI.AttachedProperties.IconElement.SetKind(copyBtn, IconKind.Check);
                CONCOST.UI.AttachedProperties.BorderElement.SetCornerRadius(copyBtn, new CornerRadius(4));
                copyBtn.Click += (s, e) =>
                {
                    try
                    {
                        Clipboard.SetText(codeSnippet.Trim());
                        copyBtn.Content = "Copied!";
                        var timer = new System.Windows.Threading.DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
                        timer.Tick += (ts, te) => { copyBtn.Content = "Copy"; timer.Stop(); };
                        timer.Start();
                    }
                    catch { }
                };
                Grid.SetColumn(copyBtn, 1);
                codeTopBar.Children.Add(copyBtn);

                codeBoxInner.Children.Add(codeTopBar);
                var divider = new Border { Height = 1, Margin = new Thickness(0) };
                divider.SetResourceReference(Border.BackgroundProperty, "CONCOST.Brush.Border");
                codeBoxInner.Children.Add(divider);
                var codeText = CreateXamlSyntaxHighlighter(codeSnippet);
                codeBoxInner.Children.Add(codeText);
                codeBox.Child = codeBoxInner;

                codeBtn.Click += (s, e) =>
                {
                    if (codeBox.Visibility == Visibility.Visible)
                    {
                        codeBox.Visibility = Visibility.Collapsed;
                        codeBtn.Content = "View XAML / C#";
                    }
                    else
                    {
                        codeBox.Visibility = Visibility.Visible;
                        codeBtn.Content = "Hide XAML / C#";
                    }
                };
            }

            sp.Children.Add(headerGrid);
            sp.Children.Add(content);
            if (codeBox != null) sp.Children.Add(codeBox);
            card.Child = sp;
            mainSp.Children.Add(card);
        }

        // Helper to build standardized dropdown menu items with icons
        StackPanel BuildMenuContent(params (string Title, IconKind Icon, string Msg)[] items)
        {
            var menuPanel = new StackPanel();
            for (int i = 0; i < items.Length; i++)
            {
                var item = items[i];
                var btn = new Button
                {
                    HorizontalContentAlignment = HorizontalAlignment.Left,
                    Padding = new Thickness(10, 6, 12, 6),
                    Cursor = Cursors.Hand,
                    Margin = i > 0 ? new Thickness(0, 4, 0, 0) : new Thickness(0)
                };
                btn.SetResourceReference(Button.StyleProperty, "CONCOST.Button.Secondary");
                CONCOST.UI.AttachedProperties.BorderElement.SetCornerRadius(btn, new CornerRadius(5));

                var contentSp = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
                if (item.Icon != IconKind.None)
                {
                    var ico = new CONCOST.UI.Icons.Icon { Kind = item.Icon, Width = 13, Height = 13, Margin = new Thickness(0, 0, 8, 0) };
                    ico.SetResourceReference(CONCOST.UI.Icons.Icon.ForegroundProperty, "CONCOST.Brush.Primary");
                    contentSp.Children.Add(ico);
                }
                contentSp.Children.Add(new TextBlock { Text = item.Title, VerticalAlignment = VerticalAlignment.Center, FontSize = 12 });
                btn.Content = contentSp;

                btn.Click += (s, e) => ToastService.Success(item.Msg, item.Title);
                menuPanel.Children.Add(btn);
            }
            return menuPanel;
        }

        // =====================================================================
        // SECTION 1: Semantic Styles & Surface Variants
        // =====================================================================
        var sec1Wp = new WrapPanel { Orientation = Orientation.Horizontal };

        var sbPrimary = new SplitButton
        {
            Content = "Save Model",
            Icon = IconKind.File,
            Margin = new Thickness(0, 0, 12, 12),
            DropDownContent = BuildMenuContent(
                ("Save As DXF", IconKind.File, "Exporting AutoCAD DXF format"),
                ("Save As IFC 4.0", IconKind.Folder, "Exporting BuildingSmart IFC standard"),
                ("Export PDF Report", IconKind.File, "Generating structural summary report")
            )
        };
        sbPrimary.Click += (s, e) => ToastService.Success("Primary Model Saved", "CONCOST Engineering");

        var sbSecondary = new SplitButton
        {
            Content = "Project Options",
            Icon = IconKind.Settings,
            Margin = new Thickness(0, 0, 12, 12),
            DropDownContent = BuildMenuContent(
                ("Design Code Preferences", IconKind.TuneAdjust, "Opening ACI 318 / Eurocode settings"),
                ("Material Catalog", IconKind.Folder, "Opening concrete and rebar database"),
                ("Display Units (SI/US)", IconKind.Settings, "Configuring millimeter/inch units")
            )
        };
        sbSecondary.SetResourceReference(SplitButton.StyleProperty, "CONCOST.SplitButton.Secondary");
        sbSecondary.Click += (s, e) => ToastService.Info("Project Preferences Loaded", "Engineering Config");

        var sbSuccess = new SplitButton
        {
            Content = "Run Analysis",
            Icon = IconKind.TuneAdjust,
            Margin = new Thickness(0, 0, 12, 12),
            DropDownContent = BuildMenuContent(
                ("Batch Structural Run", IconKind.Check, "Batch solving 12 load combinations"),
                ("Check Deflection Limits", IconKind.Check, "Verifying L/360 deflection standards"),
                ("Generate Rebar Takeoff", IconKind.File, "Calculating total tonnage and cutting list")
            )
        };
        sbSuccess.SetResourceReference(SplitButton.StyleProperty, "CONCOST.SplitButton.Success");
        sbSuccess.Click += (s, e) => ToastService.Success("Analysis Completed Successfully", "FEA Solver");

        var sbWarning = new SplitButton
        {
            Content = "Verify Mesh",
            Icon = IconKind.AlertTriangle,
            Margin = new Thickness(0, 0, 12, 12),
            DropDownContent = BuildMenuContent(
                ("Auto-Refine Elements", IconKind.TuneAdjust, "Refining quadrilateral shell elements"),
                ("Highlight Singularities", IconKind.AlertTriangle, "Marking 3 stress concentration nodes"),
                ("Relax Mesh Tolerances", IconKind.Settings, "Increasing aspect ratio tolerance")
            )
        };
        sbWarning.SetResourceReference(SplitButton.StyleProperty, "CONCOST.SplitButton.Warning");
        sbWarning.Click += (s, e) => ToastService.Warning("3 High-Stress Singularities Detected", "Mesh Quality");

        var sbDanger = new SplitButton
        {
            Content = "Purge Model",
            Icon = IconKind.Close,
            Margin = new Thickness(0, 0, 12, 12),
            DropDownContent = BuildMenuContent(
                ("Clear Current Selection", IconKind.Close, "Cleared 14 selected structural members"),
                ("Delete Orphaned Nodes", IconKind.Close, "Deleted 28 unused analytical nodes"),
                ("Purge Unassigned Materials", IconKind.Close, "Removed 4 unassigned concrete grades")
            )
        };
        sbDanger.SetResourceReference(SplitButton.StyleProperty, "CONCOST.SplitButton.Danger");
        sbDanger.Click += (s, e) => ToastService.Warning("Model Purged of Empty Geometry", "BIM Cleaner");

        var sbInfo = new SplitButton
        {
            Content = "Cloud Sync",
            Icon = IconKind.Folder,
            Margin = new Thickness(0, 0, 12, 12),
            DropDownContent = BuildMenuContent(
                ("Upload Revision 1.4", IconKind.Folder, "Uploading changes to central BIM server"),
                ("Pull Latest Teammate Changes", IconKind.TuneAdjust, "Synchronizing 4 revised load cases"),
                ("View Cloud Audit Log", IconKind.File, "Opening revision history log")
            )
        };
        sbInfo.SetResourceReference(SplitButton.StyleProperty, "CONCOST.SplitButton.Info");
        sbInfo.Click += (s, e) => ToastService.Info("Synchronized with Project Cloud", "Collaboration");

        var sbOutline = new SplitButton
        {
            Content = "Drafting Tools",
            Icon = IconKind.Edit,
            Margin = new Thickness(0, 0, 12, 12),
            DropDownContent = BuildMenuContent(
                ("Snap to Structural Grid", IconKind.Check, "Grid snapping enabled (100mm)"),
                ("Measure Distance", IconKind.TuneAdjust, "Distance measurement mode active"),
                ("Dimension Annotation", IconKind.Edit, "Adding elevation callouts")
            )
        };
        sbOutline.SetResourceReference(SplitButton.StyleProperty, "CONCOST.SplitButton.Outline");
        sbOutline.Click += (s, e) => ToastService.Info("Drafting Mode Activated", "CAD Engine");

        var sbGhost = new SplitButton
        {
            Content = "Quick Filters",
            Icon = IconKind.Filter,
            Margin = new Thickness(0, 0, 12, 12),
            DropDownContent = BuildMenuContent(
                ("Filter by Column Elements", IconKind.Filter, "Showing only column sections"),
                ("Filter by Beam Members", IconKind.Filter, "Showing only primary beams"),
                ("Show All BIM Objects", IconKind.Check, "Resetting all visibility filters")
            )
        };
        sbGhost.SetResourceReference(SplitButton.StyleProperty, "CONCOST.SplitButton.Ghost");
        sbGhost.Click += (s, e) => ToastService.Info("Filter Preset Applied", "Viewport");

        sec1Wp.Children.Add(sbPrimary);
        sec1Wp.Children.Add(sbSecondary);
        sec1Wp.Children.Add(sbSuccess);
        sec1Wp.Children.Add(sbWarning);
        sec1Wp.Children.Add(sbDanger);
        sec1Wp.Children.Add(sbInfo);
        sec1Wp.Children.Add(sbOutline);
        sec1Wp.Children.Add(sbGhost);

        string sec1Code = @"<!-- ==================== XAML MARKUP ==================== -->
<!-- 1. Primary SplitButton (Default) -->
<controls:SplitButton Content=""Save Model"" Icon=""File"" Click=""OnSaveModelClicked"">
    <StackPanel>
        <Button Content=""Save As DXF"" Style=""{DynamicResource CONCOST.Button.Secondary}"" />
        <Button Content=""Save As IFC 4.0"" Style=""{DynamicResource CONCOST.Button.Secondary}"" />
        <Button Content=""Export PDF Report"" Style=""{DynamicResource CONCOST.Button.Secondary}"" />
    </StackPanel>
</controls:SplitButton>

<!-- 2. Secondary Surface Outline SplitButton -->
<controls:SplitButton Content=""Project Options"" Icon=""Settings"" Style=""{StaticResource CONCOST.SplitButton.Secondary}"">
    <StackPanel>
        <Button Content=""Design Code Preferences"" />
        <Button Content=""Material Catalog"" />
    </StackPanel>
</controls:SplitButton>

<!-- 3. Semantic Success SplitButton -->
<controls:SplitButton Content=""Run Analysis"" Icon=""TuneAdjust"" Style=""{StaticResource CONCOST.SplitButton.Success}"">
    <StackPanel>
        <Button Content=""Batch Structural Run"" />
        <Button Content=""Check Deflection Limits"" />
    </StackPanel>
</controls:SplitButton>

<!-- 4. Semantic Danger SplitButton -->
<controls:SplitButton Content=""Purge Model"" Icon=""Close"" Style=""{StaticResource CONCOST.SplitButton.Danger}"" />

<!-- 5. Transparent Outline & Ghost SplitButtons -->
<controls:SplitButton Content=""Drafting Tools"" Icon=""Edit"" Style=""{StaticResource CONCOST.SplitButton.Outline}"" />
<controls:SplitButton Content=""Quick Filters"" Icon=""Filter"" Style=""{StaticResource CONCOST.SplitButton.Ghost}"" />

/* ==================== C# CODE-BEHIND / VIEWMODEL ==================== */
private void OnSaveModelClicked(object sender, RoutedEventArgs e)
{
    ToastService.Success(""Primary Model Saved"", ""CONCOST Engineering"");
}

private void OnRunAnalysisCommand()
{
    // Executes bound ICommand on SplitButton.Command
    ToastService.Success(""Analysis Completed Successfully"", ""FEA Solver"");
}";
        AddCardSection("1. Style ngữ nghĩa và biến thể bề mặt (Primary, Secondary, Success, Warning, Danger, Info, Outline, Ghost)", sec1Wp, sec1Code);

        // =====================================================================
        // SECTION 2: Sizing Scale & Geometry Shapes
        // =====================================================================
        var sec2Sp = new StackPanel();

        var sizeRow = new WrapPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 16) };

        var sbLarge = new SplitButton
        {
            Content = "Large (40px)",
            Icon = IconKind.File,
            Margin = new Thickness(0, 0, 14, 0),
            DropDownContent = BuildMenuContent(("Export High-Res DWG", IconKind.File, "Large Button Action"))
        };
        sbLarge.SetResourceReference(SplitButton.StyleProperty, "CONCOST.SplitButton.Large");
        sbLarge.Click += (s, e) => ToastService.Success("Large SplitButton Triggered", "Sizing Scale");

        var sbMedium = new SplitButton
        {
            Content = "Default (34px)",
            Icon = IconKind.File,
            Margin = new Thickness(0, 0, 14, 0),
            DropDownContent = BuildMenuContent(("Export Standard DXF", IconKind.File, "Medium Button Action"))
        };
        sbMedium.Click += (s, e) => ToastService.Success("Default SplitButton Triggered", "Sizing Scale");

        var sbSmall = new SplitButton
        {
            Content = "Small (28px)",
            Icon = IconKind.File,
            Margin = new Thickness(0, 0, 14, 0),
            DropDownContent = BuildMenuContent(("Export Compact IFC", IconKind.File, "Small Button Action"))
        };
        sbSmall.SetResourceReference(SplitButton.StyleProperty, "CONCOST.SplitButton.Small");
        sbSmall.Click += (s, e) => ToastService.Success("Small SplitButton Triggered", "Sizing Scale");

        sizeRow.Children.Add(sbLarge);
        sizeRow.Children.Add(sbMedium);
        sizeRow.Children.Add(sbSmall);
        sec2Sp.Children.Add(sizeRow);

        var shapeRow = new WrapPanel { Orientation = Orientation.Horizontal };

        var sbPill = new SplitButton
        {
            Content = "Pill Rounded (17px)",
            Icon = IconKind.Check,
            Margin = new Thickness(0, 0, 14, 0),
            DropDownContent = BuildMenuContent(("Apply Pill Preset", IconKind.Check, "Pill Action"))
        };
        sbPill.SetResourceReference(SplitButton.StyleProperty, "CONCOST.SplitButton.Pill");
        sbPill.Click += (s, e) => ToastService.Success("Pill SplitButton Executed", "Geometry");

        var sbRounded = new SplitButton
        {
            Content = "Rounded (8px)",
            Icon = IconKind.Settings,
            CornerRadius = new CornerRadius(8),
            Margin = new Thickness(0, 0, 14, 0),
            DropDownContent = BuildMenuContent(("Apply Rounded Preset", IconKind.Settings, "Rounded Action"))
        };
        sbRounded.SetResourceReference(SplitButton.StyleProperty, "CONCOST.SplitButton.Secondary");
        sbRounded.Click += (s, e) => ToastService.Success("Rounded 8px Executed", "Geometry");

        var sbSquare = new SplitButton
        {
            Content = "Square Sharp (0px)",
            Icon = IconKind.TuneAdjust,
            Margin = new Thickness(0, 0, 14, 0),
            DropDownContent = BuildMenuContent(("Apply Sharp Preset", IconKind.TuneAdjust, "Square Action"))
        };
        sbSquare.SetResourceReference(SplitButton.StyleProperty, "CONCOST.SplitButton.Square");
        sbSquare.Click += (s, e) => ToastService.Success("Square SplitButton Executed", "Geometry");

        shapeRow.Children.Add(sbPill);
        shapeRow.Children.Add(sbRounded);
        shapeRow.Children.Add(sbSquare);
        sec2Sp.Children.Add(shapeRow);

        string sec2Code = @"<!-- ==================== XAML MARKUP ==================== -->
<!-- Sizing Scale -->
<controls:SplitButton Content=""Large (40px)"" Style=""{StaticResource CONCOST.SplitButton.Large}"" />
<controls:SplitButton Content=""Default (34px)"" />
<controls:SplitButton Content=""Small (28px)"" Style=""{StaticResource CONCOST.SplitButton.Small}"" />

<!-- Corner Geometry Shapes -->
<controls:SplitButton Content=""Pill Shape"" Style=""{StaticResource CONCOST.SplitButton.Pill}"" />
<controls:SplitButton Content=""Rounded (8px)"" CornerRadius=""8"" Style=""{StaticResource CONCOST.SplitButton.Secondary}"" />
<controls:SplitButton Content=""Square Sharp (0px)"" Style=""{StaticResource CONCOST.SplitButton.Square}"" />

/* ==================== C# CODE-BEHIND ==================== */
var sb = new SplitButton
{
    Content = ""Large"",
    CornerRadius = new CornerRadius(8)
};
sb.SetResourceReference(SplitButton.StyleProperty, ""CONCOST.SplitButton.Large"");";
        AddCardSection("2. Thang kích thước và hình dạng (Lớn 40px, mặc định 34px, nhỏ 28px | Pill, Rounded, Square)", sec2Sp, sec2Code);

        // =====================================================================
        // SECTION 3: Interaction Modes & Placement Options
        // =====================================================================
        var sec3Wp = new WrapPanel { Orientation = Orientation.Horizontal };

        // 1. DropDown Mode (Clicking anywhere opens dropdown)
        var sbDropDownMode = new SplitButton
        {
            Content = "DropDown Mode (Click Anywhere)",
            Icon = IconKind.Folder,
            HitMode = SplitButtonHitMode.DropDown,
            Margin = new Thickness(0, 0, 14, 12),
            DropDownContent = BuildMenuContent(
                ("Open File Browser", IconKind.Folder, "Opening local explorer"),
                ("Connect Cloud Drive", IconKind.Folder, "Connecting OneDrive/Google Drive"),
                ("Recent Projects", IconKind.File, "Showing 8 recent models")
            )
        };
        sbDropDownMode.SetResourceReference(SplitButton.StyleProperty, "CONCOST.SplitButton.Secondary");

        // 2. Chevron on Left Side
        var sbLeftChevron = new SplitButton
        {
            Content = "Left Chevron Trigger",
            Icon = IconKind.Settings,
            DropDownButtonPosition = Dock.Left,
            Margin = new Thickness(0, 0, 14, 12),
            DropDownContent = BuildMenuContent(
                ("Left Position Option 1", IconKind.Settings, "Left Chevron clicked"),
                ("Left Position Option 2", IconKind.TuneAdjust, "Left Chevron clicked")
            )
        };
        sbLeftChevron.Click += (s, e) => ToastService.Success("Action executed with Left Chevron", "Navigation");

        // 3. Hover to Open DropDown
        var sbHoverOpen = new SplitButton
        {
            Content = "Hover to Open DropDown",
            Icon = IconKind.TuneAdjust,
            IsOpenOnHover = true,
            Margin = new Thickness(0, 0, 14, 12),
            DropDownContent = BuildMenuContent(
                ("Instant Selection A", IconKind.Check, "Hover selection A"),
                ("Instant Selection B", IconKind.Check, "Hover selection B")
            )
        };
        sbHoverOpen.SetResourceReference(SplitButton.StyleProperty, "CONCOST.SplitButton.Success");
        sbHoverOpen.DropDownOpened += (s, e) => ToastService.Info("Dropdown opened via Hover", "Hover Engine");
        sbHoverOpen.Click += (s, e) => ToastService.Success("Primary action clicked", "Hover SplitButton");

        // 4. Icon on Right Side
        var sbIconRight = new SplitButton
        {
            Content = "Icon Placed on Right",
            Icon = IconKind.Check,
            IconPlacement = Dock.Right,
            Margin = new Thickness(0, 0, 14, 12),
            DropDownContent = BuildMenuContent(
                ("Item Alpha", IconKind.Check, "Selected Alpha"),
                ("Item Beta", IconKind.Check, "Selected Beta")
            )
        };
        sbIconRight.SetResourceReference(SplitButton.StyleProperty, "CONCOST.SplitButton.Outline");
        sbIconRight.Click += (s, e) => ToastService.Success("Icon on Right triggered", "Placement");

        // 5. Placement Top (Opens upwards)
        var sbPlaceTop = new SplitButton
        {
            Content = "Placement Top (Upwards)",
            Icon = IconKind.Edit,
            Placement = PlacementMode.Top,
            Margin = new Thickness(0, 0, 14, 12),
            DropDownContent = BuildMenuContent(
                ("Upward Item 1", IconKind.Edit, "Upward menu item 1"),
                ("Upward Item 2", IconKind.Edit, "Upward menu item 2")
            )
        };
        sbPlaceTop.Click += (s, e) => ToastService.Info("Top Placement SplitButton", "Placement");

        sec3Wp.Children.Add(sbDropDownMode);
        sec3Wp.Children.Add(sbLeftChevron);
        sec3Wp.Children.Add(sbHoverOpen);
        sec3Wp.Children.Add(sbIconRight);
        sec3Wp.Children.Add(sbPlaceTop);

        string sec3Code = @"<!-- ==================== XAML MARKUP ==================== -->
<!-- 1. DropDown Mode (Clicking anywhere toggles popup, behaves like DropDownButton) -->
<controls:SplitButton Content=""DropDown Mode""
                      Icon=""Folder""
                      HitMode=""DropDown""
                      Style=""{StaticResource CONCOST.SplitButton.Secondary}"">
    <StackPanel>
        <Button Content=""Open File Browser"" />
        <Button Content=""Connect Cloud Drive"" />
    </StackPanel>
</controls:SplitButton>

<!-- 2. DropDownButton Positioned on Left Side -->
<controls:SplitButton Content=""Left Chevron Trigger""
                      Icon=""Settings""
                      DropDownButtonPosition=""Left""
                      Click=""OnActionClicked"" />

<!-- 3. Hover to Open with Smooth Debounce Timers -->
<controls:SplitButton Content=""Hover to Open""
                      Icon=""TuneAdjust""
                      IsOpenOnHover=""True""
                      DropDownOpened=""OnDropDownOpened""
                      Style=""{StaticResource CONCOST.SplitButton.Success}"" />

<!-- 4. Icon Placed on Right Side & Popup Placement Mode -->
<controls:SplitButton Content=""Icon on Right""
                      Icon=""Check""
                      IconPlacement=""Right""
                      Placement=""Top""
                      Style=""{StaticResource CONCOST.SplitButton.Outline}"" />

/* ==================== C# CODE-BEHIND ==================== */
private void OnDropDownOpened(object sender, RoutedEventArgs e)
{
    ToastService.Info(""Dropdown opened via Hover"", ""Hover Engine"");
}";
        AddCardSection("3. Chế độ tương tác và vị trí (HitMode=DropDown, Chevron Left, HoverOpen, IconPlacement, Placement=Top)", sec3Wp, sec3Code);

        // =====================================================================
        // SECTION 4: Engineering & BIM Workflows (Interactive Checklists)
        // =====================================================================
        var sec4Wp = new WrapPanel { Orientation = Orientation.Horizontal };

        // 1. Interactive Multi-Select Checklist (AutoCloseOnClick="False")
        var checklistPanel = new StackPanel { MinWidth = 200, Margin = new Thickness(4) };
        var listHeader = new TextBlock
        {
            Text = "VISIBLE STRUCTURAL LAYERS",
            FontSize = 11,
            FontWeight = FontWeights.Normal,
            Margin = new Thickness(4, 2, 4, 8)
        };
        listHeader.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");
        checklistPanel.Children.Add(listHeader);

        string[] layers = ["Concrete Columns (C30/37)", "Primary Transfer Beams", "Two-Way Floor Slabs", "Isolated Footings", "Rebar Mesh Top & Bottom"];
        foreach (var lyr in layers)
        {
            var cb = new CheckBox
            {
                Content = lyr,
                IsChecked = true,
                Margin = new Thickness(4, 4, 4, 4),
                FontSize = 12
            };
            cb.Checked += (s, e) => ToastService.Info($"Layer '{lyr}' Enabled", "BIM Filter");
            cb.Unchecked += (s, e) => ToastService.Warning($"Layer '{lyr}' Hidden", "BIM Filter");
            checklistPanel.Children.Add(cb);
        }

        var sbInteractiveFilter = new SplitButton
        {
            Content = "BIM Layer Visibility",
            Icon = IconKind.Filter,
            AutoCloseOnClick = false,
            Margin = new Thickness(0, 0, 16, 12),
            DropDownContent = checklistPanel
        };
        sbInteractiveFilter.SetResourceReference(SplitButton.StyleProperty, "CONCOST.SplitButton.Secondary");
        sbInteractiveFilter.Click += (s, e) => ToastService.Success("Applied All Layer Visibility Presets", "BIM Engine");

        // 2. Rebar Batch Calculation & Report Suite
        var sbRebarSuite = new SplitButton
        {
            Content = "Calculate Rebar Schedule",
            Icon = IconKind.TuneAdjust,
            Margin = new Thickness(0, 0, 16, 12),
            DropDownContent = BuildMenuContent(
                ("Calculate Flexural Reinforcement", IconKind.TuneAdjust, "As_req = 1,420 mm²/m computed"),
                ("Calculate Shear Stirrups & Spacing", IconKind.Check, "Stirrups Ø10@150mm verified"),
                ("Crack Width Verification (wk <= 0.3mm)", IconKind.Check, "Crack width wk = 0.18mm OK"),
                ("Export Full Bar Bending Schedule (BBS)", IconKind.File, "Generating Excel BBS schedule")
            )
        };
        sbRebarSuite.SetResourceReference(SplitButton.StyleProperty, "CONCOST.SplitButton.Success");
        sbRebarSuite.Click += (s, e) => ToastService.Success("Rebar Calculation Run Complete", "Rebar Suite");

        // 3. Multi-Format Model Exporter
        var sbExportSuite = new SplitButton
        {
            Content = "Export Structural Deliverables",
            Icon = IconKind.File,
            Margin = new Thickness(0, 0, 16, 12),
            DropDownContent = BuildMenuContent(
                ("Export AutoCAD DWG 2024", IconKind.File, "Exporting DWG drawings"),
                ("Export Revit IFC 4.0 Reference View", IconKind.Folder, "Exporting IFC building model"),
                ("Export 3D PDF Document", IconKind.File, "Generating 3D interactive PDF"),
                ("Export Tekla CIS/2 Structural Model", IconKind.TuneAdjust, "Exporting steel framing CIS/2")
            )
        };
        sbExportSuite.Click += (s, e) => ToastService.Success("Exported Structural Package", "Delivery");

        sec4Wp.Children.Add(sbInteractiveFilter);
        sec4Wp.Children.Add(sbRebarSuite);
        sec4Wp.Children.Add(sbExportSuite);

        string sec4Code = @"<!-- ==================== XAML MARKUP ==================== -->
<!-- 1. Interactive Multi-Select Filter (AutoCloseOnClick=""False"") -->
<controls:SplitButton Content=""BIM Layer Visibility""
                      Icon=""Filter""
                      AutoCloseOnClick=""False""
                      Style=""{StaticResource CONCOST.SplitButton.Secondary}""
                      Click=""OnApplyAllLayersClicked"">
    <StackPanel MinWidth=""200"" Margin=""4"">
        <TextBlock Text=""VISIBLE STRUCTURAL LAYERS"" FontWeight=""Normal"" FontSize=""11"" Margin=""4,2,4,8"" />
        <CheckBox Content=""Concrete Columns (C30/37)"" IsChecked=""True"" Checked=""OnLayerChanged"" />
        <CheckBox Content=""Primary Transfer Beams"" IsChecked=""True"" Checked=""OnLayerChanged"" />
        <CheckBox Content=""Two-Way Floor Slabs"" IsChecked=""True"" Checked=""OnLayerChanged"" />
        <CheckBox Content=""Isolated Footings"" IsChecked=""True"" Checked=""OnLayerChanged"" />
        <CheckBox Content=""Rebar Mesh Top &amp; Bottom"" IsChecked=""True"" Checked=""OnLayerChanged"" />
    </StackPanel>
</controls:SplitButton>

<!-- 2. Rebar Engineering Calculation Suite -->
<controls:SplitButton Content=""Calculate Rebar Schedule""
                      Icon=""TuneAdjust""
                      Style=""{StaticResource CONCOST.SplitButton.Success}""
                      Click=""OnCalculateRebarClicked"">
    <StackPanel>
        <Button Content=""Calculate Flexural Reinforcement"" />
        <Button Content=""Calculate Shear Stirrups &amp; Spacing"" />
        <Button Content=""Crack Width Verification (wk &lt;= 0.3mm)"" />
        <Button Content=""Export Full Bar Bending Schedule (BBS)"" />
    </StackPanel>
</controls:SplitButton>

/* ==================== C# CODE-BEHIND ==================== */
// Notice AutoCloseOnClick=""False"" prevents the dropdown popup from closing 
// when the user toggles individual checkboxes inside the dropdown content!
private void OnLayerChanged(object sender, RoutedEventArgs e)
{
    if (sender is CheckBox cb)
    {
        ToastService.Info($""Layer '{cb.Content}' visibility toggled"", ""BIM Filter"");
    }
}

private void OnCalculateRebarClicked(object sender, RoutedEventArgs e)
{
    ToastService.Success(""Rebar Calculation Run Complete"", ""Rebar Suite"");
}";
        AddCardSection("4. Quy trình kỹ thuật và BIM (checklist tương tác với AutoCloseOnClick=False, bộ thép, xuất CAD)", sec4Wp, sec4Code);

        return mainSp;
    }

    // =========================================================================
    // DEMO: SideMenu (Modern Vertical Navigation Suite)
    // =========================================================================
    private object CreateSideMenuDemo()
    {
        var mainSp = new StackPanel { Margin = new Thickness(0) };

        void AddCardSection(string title, UIElement content, string? codeSnippet = null)
        {
            var card = new Border
            {
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(20),
                Margin = new Thickness(0, 0, 0, 18)
            };
            card.SetResourceReference(Border.BackgroundProperty, "CONCOST.Brush.Surface");
            card.SetResourceReference(Border.BorderBrushProperty, "CONCOST.Brush.Border");

            var sp = new StackPanel();
            var headerGrid = new Grid { Margin = new Thickness(0, 0, 0, 14) };
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var titleBlock = new TextBlock
            {
                Text = title,
                FontWeight = FontWeights.Normal,
                FontSize = 14,
                VerticalAlignment = VerticalAlignment.Center
            };
            titleBlock.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextPrimary");
            Grid.SetColumn(titleBlock, 0);
            headerGrid.Children.Add(titleBlock);

            Border? codeBox = null;
            if (!string.IsNullOrEmpty(codeSnippet))
            {
                var codeBtn = new Button
                {
                    Content = "View XAML / C#",
                    Padding = new Thickness(10, 4, 10, 4),
                    FontSize = 12,
                    Cursor = Cursors.Hand
                };
                codeBtn.SetResourceReference(Button.StyleProperty, "CONCOST.Button.Secondary");
                CONCOST.UI.AttachedProperties.IconElement.SetKind(codeBtn, IconKind.Edit);
                CONCOST.UI.AttachedProperties.BorderElement.SetCornerRadius(codeBtn, new CornerRadius(6));
                Grid.SetColumn(codeBtn, 1);
                headerGrid.Children.Add(codeBtn);

                codeBox = new Border
                {
                    CornerRadius = new CornerRadius(8),
                    BorderThickness = new Thickness(1),
                    Margin = new Thickness(0, 16, 0, 0),
                    Visibility = Visibility.Collapsed
                };
                codeBox.SetResourceReference(Border.BackgroundProperty, "CONCOST.Brush.SurfaceSecondary");
                codeBox.SetResourceReference(Border.BorderBrushProperty, "CONCOST.Brush.Border");

                var codeBoxInner = new StackPanel();
                var codeTopBar = new Grid { Height = 34, Margin = new Thickness(12, 0, 8, 0) };
                codeTopBar.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                codeTopBar.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                var langBadge = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
                var tagIcon = new CONCOST.UI.Icons.Icon { Kind = IconKind.Edit, Width = 14, Height = 14, Margin = new Thickness(0, 0, 6, 0) };
                tagIcon.SetResourceReference(CONCOST.UI.Icons.Icon.ForegroundProperty, "CONCOST.Brush.TextSecondary");
                var langLabel = new TextBlock { Text = "XAML & C# Code-Behind", FontSize = 11.5, FontWeight = FontWeights.Normal };
                langLabel.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");
                langBadge.Children.Add(tagIcon);
                langBadge.Children.Add(langLabel);
                Grid.SetColumn(langBadge, 0);
                codeTopBar.Children.Add(langBadge);

                var copyBtn = new Button { Content = "Copy", Padding = new Thickness(10, 2, 10, 2), FontSize = 11.5, Cursor = Cursors.Hand };
                copyBtn.SetResourceReference(Button.StyleProperty, "CONCOST.Button.Secondary");
                CONCOST.UI.AttachedProperties.IconElement.SetKind(copyBtn, IconKind.Check);
                CONCOST.UI.AttachedProperties.BorderElement.SetCornerRadius(copyBtn, new CornerRadius(4));
                copyBtn.Click += (s, e) =>
                {
                    try
                    {
                        Clipboard.SetText(codeSnippet.Trim());
                        copyBtn.Content = "Copied!";
                        var timer = new System.Windows.Threading.DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
                        timer.Tick += (ts, te) => { copyBtn.Content = "Copy"; timer.Stop(); };
                        timer.Start();
                    }
                    catch { }
                };
                Grid.SetColumn(copyBtn, 1);
                codeTopBar.Children.Add(copyBtn);

                codeBoxInner.Children.Add(codeTopBar);
                var divider = new Border { Height = 1, Margin = new Thickness(0) };
                divider.SetResourceReference(Border.BackgroundProperty, "CONCOST.Brush.Border");
                codeBoxInner.Children.Add(divider);
                var codeText = CreateXamlSyntaxHighlighter(codeSnippet);
                codeBoxInner.Children.Add(codeText);
                codeBox.Child = codeBoxInner;

                codeBtn.Click += (s, e) =>
                {
                    if (codeBox.Visibility == Visibility.Visible)
                    {
                        codeBox.Visibility = Visibility.Collapsed;
                        codeBtn.Content = "View XAML / C#";
                    }
                    else
                    {
                        codeBox.Visibility = Visibility.Visible;
                        codeBtn.Content = "Hide XAML / C#";
                    }
                };
            }

            sp.Children.Add(headerGrid);
            sp.Children.Add(content);
            if (codeBox != null) sp.Children.Add(codeBox);
            card.Child = sp;
            mainSp.Children.Add(card);
        }

        // Section 1: Standard Vertical SideMenu & Rail Mode SideMenu
        var sec1Grid = new Grid();
        sec1Grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(240) });
        sec1Grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(20) });
        sec1Grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60) });
        sec1Grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(20) });
        sec1Grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        var sideMenu1 = new SideMenu { Height = 340 };
        var headerTxt = new TextBlock { Text = "PROJECT MODULES", FontSize = 11, FontWeight = FontWeights.Normal, Margin = new Thickness(8, 4, 0, 4) };
        headerTxt.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");
        sideMenu1.Header = headerTxt;
        sideMenu1.Items.Add(new SideMenuItem("Dashboard", IconKind.Layers) { IsSelected = true });
        sideMenu1.Items.Add(new SideMenuItem("Rebar Detailing", IconKind.TuneAdjust, "NEW"));
        sideMenu1.Items.Add(new SideMenuItem("BIM Geometry", IconKind.Folder));
        sideMenu1.Items.Add(new SideMenuItem("Cost Estimation", IconKind.File, "12"));
        sideMenu1.Items.Add(new SideMenuItem("Export Center", IconKind.CheckCircle));
        sideMenu1.Items.Add(new SideMenuItem("Settings", IconKind.Settings));

        var sideMenuRail = new SideMenu { Height = 340 };
        sideMenuRail.SetResourceReference(SideMenu.StyleProperty, "CONCOST.SideMenu.Rail");
        sideMenuRail.Items.Add(new SideMenuItem("", IconKind.Layers) { IsSelected = true });
        sideMenuRail.Items.Add(new SideMenuItem("", IconKind.TuneAdjust));
        sideMenuRail.Items.Add(new SideMenuItem("", IconKind.Folder));
        sideMenuRail.Items.Add(new SideMenuItem("", IconKind.File));
        sideMenuRail.Items.Add(new SideMenuItem("", IconKind.CheckCircle));
        sideMenuRail.Items.Add(new SideMenuItem("", IconKind.Settings));

        var previewPanel = new Border
        {
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(20),
            VerticalAlignment = VerticalAlignment.Stretch
        };
        previewPanel.SetResourceReference(Border.BackgroundProperty, "CONCOST.Brush.SurfaceSecondary");
        previewPanel.SetResourceReference(Border.BorderBrushProperty, "CONCOST.Brush.Border");
        previewPanel.BorderThickness = new Thickness(1);

        var previewText = new TextBlock
        {
            Text = "Active Module: Dashboard\nSelect any menu item to switch context seamlessly.",
            FontSize = 13,
            FontWeight = FontWeights.Normal,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
            TextAlignment = TextAlignment.Center
        };
        previewText.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextPrimary");
        previewPanel.Child = previewText;

        sideMenu1.SelectionChanged += (s, e) =>
        {
            if (e.NewValue != null)
                previewText.Text = $"Active Module: {e.NewValue.Title}\nStatus: Loaded successfully.";
        };

        Grid.SetColumn(sideMenu1, 0);
        Grid.SetColumn(sideMenuRail, 2);
        Grid.SetColumn(previewPanel, 4);

        sec1Grid.Children.Add(sideMenu1);
        sec1Grid.Children.Add(sideMenuRail);
        sec1Grid.Children.Add(previewPanel);

        string sec1Code = @"<!-- ==================== XAML MARKUP ==================== -->
<!-- 1. Full Navigation SideMenu -->
<controls:SideMenu Width=""240"">
    <controls:SideMenu.Header>
        <TextBlock Text=""PROJECT MODULES"" FontSize=""11"" FontWeight=""Normal"" />
    </controls:SideMenu.Header>
    <controls:SideMenu.Items>
        <controls:SideMenuItem Title=""Dashboard"" Icon=""Layers"" IsSelected=""True"" />
        <controls:SideMenuItem Title=""Rebar Detailing"" Icon=""TuneAdjust"" Badge=""NEW"" />
        <controls:SideMenuItem Title=""BIM Geometry"" Icon=""Folder"" />
        <controls:SideMenuItem Title=""Cost Estimation"" Icon=""File"" Badge=""12"" />
        <controls:SideMenuItem Title=""Settings"" Icon=""Settings"" />
    </controls:SideMenu.Items>
</controls:SideMenu>

<!-- 2. Compact Rail SideMenu -->
<controls:SideMenu Style=""{StaticResource CONCOST.SideMenu.Rail}"">
    <controls:SideMenu.Items>
        <controls:SideMenuItem Icon=""Layers"" IsSelected=""True"" />
        <controls:SideMenuItem Icon=""TuneAdjust"" />
        <controls:SideMenuItem Icon=""Settings"" />
    </controls:SideMenu.Items>
</controls:SideMenu>";
        AddCardSection("1. Bộ điều hướng SideMenu và chế độ rail", sec1Grid, sec1Code);

        return mainSp;
    }

    // =========================================================================
    // DEMO: Timeline (Modern Process Stream Suite)
    // =========================================================================
    private object CreateTimelineDemo()
    {
        var mainSp = new StackPanel { Margin = new Thickness(0) };

        void AddCardSection(string title, UIElement content, string? codeSnippet = null)
        {
            var card = new Border
            {
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(20),
                Margin = new Thickness(0, 0, 0, 18)
            };
            card.SetResourceReference(Border.BackgroundProperty, "CONCOST.Brush.Surface");
            card.SetResourceReference(Border.BorderBrushProperty, "CONCOST.Brush.Border");

            var sp = new StackPanel();

            var headerGrid = new Grid { Margin = new Thickness(0, 0, 0, 14) };
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var titleBlock = new TextBlock
            {
                Text = title,
                FontWeight = FontWeights.Normal,
                FontSize = 14,
                VerticalAlignment = VerticalAlignment.Center
            };
            titleBlock.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextPrimary");
            Grid.SetColumn(titleBlock, 0);
            headerGrid.Children.Add(titleBlock);

            Border? codeBox = null;
            if (!string.IsNullOrEmpty(codeSnippet))
            {
                var codeBtn = new Button
                {
                    Content = "View XAML / C#",
                    Padding = new Thickness(10, 4, 10, 4),
                    FontSize = 12,
                    Cursor = Cursors.Hand
                };
                codeBtn.SetResourceReference(Button.StyleProperty, "CONCOST.Button.Secondary");
                IconElement.SetKind(codeBtn, IconKind.Edit);
                BorderElement.SetCornerRadius(codeBtn, new CornerRadius(6));
                Grid.SetColumn(codeBtn, 1);
                headerGrid.Children.Add(codeBtn);

                codeBox = new Border
                {
                    CornerRadius = new CornerRadius(8),
                    BorderThickness = new Thickness(1),
                    Margin = new Thickness(0, 16, 0, 0),
                    Visibility = Visibility.Collapsed
                };
                codeBox.SetResourceReference(Border.BackgroundProperty, "CONCOST.Brush.SurfaceSecondary");
                codeBox.SetResourceReference(Border.BorderBrushProperty, "CONCOST.Brush.Border");

                var codeBoxInner = new StackPanel();

                var codeTopBar = new Grid
                {
                    Height = 34,
                    Margin = new Thickness(12, 0, 8, 0)
                };
                codeTopBar.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                codeTopBar.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                var langBadge = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    VerticalAlignment = VerticalAlignment.Center
                };
                var tagIcon = new Icon
                {
                    Kind = IconKind.Edit,
                    Width = 14,
                    Height = 14,
                    Margin = new Thickness(0, 0, 6, 0)
                };
                tagIcon.SetResourceReference(Icon.ForegroundProperty, "CONCOST.Brush.TextSecondary");

                var langLabel = new TextBlock
                {
                    Text = "XAML & C# Code Inspector",
                    FontSize = 11.5,
                    FontWeight = FontWeights.Normal,
                    VerticalAlignment = VerticalAlignment.Center
                };
                langLabel.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");

                langBadge.Children.Add(tagIcon);
                langBadge.Children.Add(langLabel);
                Grid.SetColumn(langBadge, 0);
                codeTopBar.Children.Add(langBadge);

                var copyBtn = new Button
                {
                    Content = "Copy",
                    Padding = new Thickness(10, 2, 10, 2),
                    FontSize = 11,
                    Height = 24,
                    VerticalAlignment = VerticalAlignment.Center,
                    Cursor = Cursors.Hand
                };
                copyBtn.SetResourceReference(Button.StyleProperty, "CONCOST.Button.Secondary");
                IconElement.SetKind(copyBtn, IconKind.Check);
                BorderElement.SetCornerRadius(copyBtn, new CornerRadius(4));
                copyBtn.Click += (s, e) =>
                {
                    try
                    {
                        Clipboard.SetText(codeSnippet.Trim());
                        copyBtn.Content = "Copied!";
                        var timer = new System.Windows.Threading.DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
                        timer.Tick += (ts, te) => { copyBtn.Content = "Copy"; timer.Stop(); };
                        timer.Start();
                    }
                    catch { }
                };
                Grid.SetColumn(copyBtn, 1);
                codeTopBar.Children.Add(copyBtn);

                codeBoxInner.Children.Add(codeTopBar);

                var divider = new Border
                {
                    Height = 1,
                    Margin = new Thickness(0)
                };
                divider.SetResourceReference(Border.BackgroundProperty, "CONCOST.Brush.Border");
                codeBoxInner.Children.Add(divider);

                var codeText = CreateXamlSyntaxHighlighter(codeSnippet);
                codeBoxInner.Children.Add(codeText);

                codeBox.Child = codeBoxInner;

                codeBtn.Click += (s, e) =>
                {
                    if (codeBox.Visibility == Visibility.Visible)
                    {
                        codeBox.Visibility = Visibility.Collapsed;
                        codeBtn.Content = "View XAML / C#";
                    }
                    else
                    {
                        codeBox.Visibility = Visibility.Visible;
                        codeBtn.Content = "Hide Code";
                    }
                };
            }

            sp.Children.Add(headerGrid);
            sp.Children.Add(content);
            if (codeBox != null)
            {
                sp.Children.Add(codeBox);
            }
            card.Child = sp;
            mainSp.Children.Add(card);
        }

        // ---------------------------------------------------------------------
        // SECTION 1: Standard Semantic & Status Timeline Stream
        // ---------------------------------------------------------------------
        var tl1 = new Timeline { HorizontalAlignment = HorizontalAlignment.Left, MaxWidth = 680 };

        var item1 = new TimelineItem
        {
            Time = "08:30 AM",
            Header = "Project Model Initialized",
            Description = "IFC 4.0 structural model file parsed and geometric mesh generated for 12 storeys.",
            Type = TimelineItemType.Success,
            Icon = IconKind.CheckCircle
        };

        var item2 = new TimelineItem
        {
            Time = "10:15 AM",
            Header = "FEA Load Analysis Completed",
            Description = "Dead load (G) and live load (Q) load combinations verified under TCVN 5574:2018.",
            Type = TimelineItemType.Primary,
            Icon = IconKind.TuneAdjust
        };

        var item3 = new TimelineItem
        {
            Time = "02:45 PM",
            Header = "Rebar Congestion Warning",
            Description = "Column C3 node exceeds maximum reinforcement ratio limit (rho > 3.5%). Engineer review required.",
            Type = TimelineItemType.Warning,
            Icon = IconKind.Warning
        };

        var item4 = new TimelineItem
        {
            Time = "04:20 PM",
            Header = "Automated Cost Takeoff Scheduled",
            Description = "Ready for BOM export to Excel (.xlsx) format once final section revisions are signed off.",
            Type = TimelineItemType.Info,
            Icon = IconKind.Clock
        };

        tl1.Items.Add(item1);
        tl1.Items.Add(item2);
        tl1.Items.Add(item3);
        tl1.Items.Add(item4);

        string sec1Code = @"<!-- ==================== XAML MARKUP ==================== -->
<dash:Timeline HorizontalAlignment=""Left"" MaxWidth=""680"">
    <!-- 1. Success Milestone Node -->
    <dash:TimelineItem Time=""08:30 AM""
                       Header=""Project Model Initialized""
                       Description=""IFC 4.0 structural model file parsed and geometric mesh generated for 12 storeys.""
                       Type=""Success""
                       Icon=""CheckCircle"" />

    <!-- 2. Primary Analysis Node -->
    <dash:TimelineItem Time=""10:15 AM""
                       Header=""FEA Load Analysis Completed""
                       Description=""Dead load (G) and live load (Q) load combinations verified under TCVN 5574:2018.""
                       Type=""Primary""
                       Icon=""TuneAdjust"" />

    <!-- 3. Warning Limit State Node -->
    <dash:TimelineItem Time=""02:45 PM""
                       Header=""Rebar Congestion Warning""
                       Description=""Column C3 node exceeds maximum reinforcement ratio limit (rho > 3.5%). Engineer review required.""
                       Type=""Warning""
                       Icon=""Warning"" />

    <!-- 4. Scheduled Action Node -->
    <dash:TimelineItem Time=""04:20 PM""
                       Header=""Automated Cost Takeoff Scheduled""
                       Description=""Ready for BOM export to Excel (.xlsx) format once final section revisions are signed off.""
                       Type=""Info""
                       Icon=""Clock"" />
</dash:Timeline>

<!-- ==================== C# CODE-BEHIND ==================== -->
// Populate or update Timeline items dynamically via code or data binding
var timeline = new Timeline();
timeline.Items.Add(new TimelineItem
{
    Time = DateTime.Now.ToString(""hh:mm tt""),
    Header = ""Calculation Finished"",
    Description = ""All beam sections verified successfully."",
    Type = TimelineItemType.Success,
    Icon = IconKind.CheckCircle
});";
        AddCardSection("1. Timeline quy trình ngữ nghĩa chuẩn", tl1, sec1Code);

        // ---------------------------------------------------------------------
        // SECTION 2: Card-Enclosed Timeline with Rich Interactive Content
        // ---------------------------------------------------------------------
        var tlCard = new Timeline
        {
            HorizontalAlignment = HorizontalAlignment.Left,
            MaxWidth = 680
        };
        tlCard.SetResourceReference(Timeline.StyleProperty, "CONCOST.Timeline.Card");

        var cItem1 = new TimelineItem
        {
            Time = "Phase 1 • 2026",
            Header = "Foundation & Pile Cap Inspection",
            Type = TimelineItemType.Success,
            Icon = IconKind.CheckCircle
        };
        var tagPanel1 = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 4, 0, 0) };
        var tag1 = new Border { CornerRadius = new CornerRadius(4), Padding = new Thickness(8, 2, 8, 2), Margin = new Thickness(0, 0, 8, 0) };
        tag1.SetResourceReference(Border.BackgroundProperty, "CONCOST.Brush.SurfaceSecondary");
        tag1.Child = new TextBlock { Text = "Passed 100%", FontSize = 11.5, FontWeight = FontWeights.Normal, Foreground = new SolidColorBrush(Color.FromRgb(16, 185, 129)) };
        tagPanel1.Children.Add(tag1);
        cItem1.Content = tagPanel1;

        var cItem2 = new TimelineItem
        {
            Time = "Phase 2 • 2026",
            Header = "Superstructure Framing & Slabs",
            Description = "Pouring concrete for 5th floor slab (Grade B30 with silica fume additives).",
            Type = TimelineItemType.Primary,
            Icon = IconKind.Layers
        };

        var cItem3 = new TimelineItem
        {
            Time = "Phase 3 • 2026",
            Header = "Facade Installation & Handover",
            Description = "Curtain wall structural testing scheduled upon completion of frame deflection survey.",
            Type = TimelineItemType.Default,
            Icon = IconKind.ShieldCheck
        };

        tlCard.Items.Add(cItem1);
        tlCard.Items.Add(cItem2);
        tlCard.Items.Add(cItem3);

        string sec2Code = @"<!-- ==================== XAML MARKUP ==================== -->
<dash:Timeline Style=""{DynamicResource CONCOST.Timeline.Card}"" MaxWidth=""680"">
    <dash:TimelineItem Time=""Phase 1 • 2026""
                       Header=""Foundation &amp; Pile Cap Inspection""
                       Type=""Success""
                       Icon=""CheckCircle"">
        <Border CornerRadius=""4"" Padding=""8,2"" Background=""{DynamicResource CONCOST.Brush.SurfaceSecondary}"">
            <TextBlock Text=""Passed 100%"" FontSize=""11.5"" FontWeight=""Normal"" Foreground=""#10B981"" />
        </Border>
    </dash:TimelineItem>

    <dash:TimelineItem Time=""Phase 2 • 2026""
                       Header=""Superstructure Framing &amp; Slabs""
                       Description=""Pouring concrete for 5th floor slab (Grade B30 with silica fume additives).""
                       Type=""Primary""
                       Icon=""Layers"" />

    <dash:TimelineItem Time=""Phase 3 • 2026""
                       Header=""Facade Installation &amp; Handover""
                       Description=""Curtain wall structural testing scheduled upon completion of frame deflection survey.""
                       Type=""Default""
                       Icon=""ShieldCheck"" />
</dash:Timeline>";
        AddCardSection("2. Timeline trong thẻ với nội dung lồng tuỳ biến", tlCard, sec2Code);

        return mainSp;
    }

    // =========================================================================
    // DEMO: CardCarousel (3D Card Flow)
    // =========================================================================
    private object CreateCardCarouselDemo()
    {
        var mainSp = new StackPanel { Margin = new Thickness(0) };

        void AddCardSection(string title, UIElement content, string? codeSnippet = null)
        {
            var card = new Border
            {
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(20),
                Margin = new Thickness(0, 0, 0, 18)
            };
            card.SetResourceReference(Border.BackgroundProperty, "CONCOST.Brush.Surface");
            card.SetResourceReference(Border.BorderBrushProperty, "CONCOST.Brush.Border");

            var sp = new StackPanel();

            var headerGrid = new Grid { Margin = new Thickness(0, 0, 0, 14) };
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var titleBlock = new TextBlock
            {
                Text = title,
                FontWeight = FontWeights.Normal,
                FontSize = 14,
                VerticalAlignment = VerticalAlignment.Center
            };
            titleBlock.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextPrimary");
            Grid.SetColumn(titleBlock, 0);
            headerGrid.Children.Add(titleBlock);

            Border? codeBox = null;
            if (!string.IsNullOrEmpty(codeSnippet))
            {
                var codeBtn = new Button
                {
                    Content = "View XAML / C#",
                    Padding = new Thickness(10, 4, 10, 4),
                    FontSize = 12,
                    Cursor = Cursors.Hand
                };
                codeBtn.SetResourceReference(Button.StyleProperty, "CONCOST.Button.Secondary");
                IconElement.SetKind(codeBtn, IconKind.Edit);
                BorderElement.SetCornerRadius(codeBtn, new CornerRadius(6));
                Grid.SetColumn(codeBtn, 1);
                headerGrid.Children.Add(codeBtn);

                codeBox = new Border
                {
                    CornerRadius = new CornerRadius(8),
                    BorderThickness = new Thickness(1),
                    Margin = new Thickness(0, 16, 0, 0),
                    Visibility = Visibility.Collapsed
                };
                codeBox.SetResourceReference(Border.BackgroundProperty, "CONCOST.Brush.SurfaceSecondary");
                codeBox.SetResourceReference(Border.BorderBrushProperty, "CONCOST.Brush.Border");

                var codeBoxInner = new StackPanel();

                var codeTopBar = new Grid
                {
                    Height = 34,
                    Margin = new Thickness(12, 0, 8, 0)
                };
                codeTopBar.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                codeTopBar.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                var langBadge = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    VerticalAlignment = VerticalAlignment.Center
                };
                var tagIcon = new Icon
                {
                    Kind = IconKind.Edit,
                    Width = 14,
                    Height = 14,
                    Margin = new Thickness(0, 0, 6, 0)
                };
                tagIcon.SetResourceReference(Icon.ForegroundProperty, "CONCOST.Brush.TextSecondary");

                var langLabel = new TextBlock
                {
                    Text = "XAML & C# Code Inspector",
                    FontSize = 11.5,
                    FontWeight = FontWeights.Normal,
                    VerticalAlignment = VerticalAlignment.Center
                };
                langLabel.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");

                langBadge.Children.Add(tagIcon);
                langBadge.Children.Add(langLabel);
                Grid.SetColumn(langBadge, 0);
                codeTopBar.Children.Add(langBadge);

                var copyBtn = new Button
                {
                    Content = "Copy",
                    Padding = new Thickness(10, 2, 10, 2),
                    FontSize = 11,
                    Height = 24,
                    VerticalAlignment = VerticalAlignment.Center,
                    Cursor = Cursors.Hand
                };
                copyBtn.SetResourceReference(Button.StyleProperty, "CONCOST.Button.Secondary");
                IconElement.SetKind(copyBtn, IconKind.Check);
                BorderElement.SetCornerRadius(copyBtn, new CornerRadius(4));
                copyBtn.Click += (s, e) =>
                {
                    try
                    {
                        Clipboard.SetText(codeSnippet.Trim());
                        copyBtn.Content = "Copied!";
                        var timer = new System.Windows.Threading.DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
                        timer.Tick += (ts, te) => { copyBtn.Content = "Copy"; timer.Stop(); };
                        timer.Start();
                    }
                    catch { }
                };
                Grid.SetColumn(copyBtn, 1);
                codeTopBar.Children.Add(copyBtn);

                codeBoxInner.Children.Add(codeTopBar);

                var divider = new Border
                {
                    Height = 1,
                    Margin = new Thickness(0)
                };
                divider.SetResourceReference(Border.BackgroundProperty, "CONCOST.Brush.Border");
                codeBoxInner.Children.Add(divider);

                var codeText = CreateXamlSyntaxHighlighter(codeSnippet);
                codeBoxInner.Children.Add(codeText);

                codeBox.Child = codeBoxInner;

                codeBtn.Click += (s, e) =>
                {
                    if (codeBox.Visibility == Visibility.Visible)
                    {
                        codeBox.Visibility = Visibility.Collapsed;
                        codeBtn.Content = "View XAML / C#";
                    }
                    else
                    {
                        codeBox.Visibility = Visibility.Visible;
                        codeBtn.Content = "Hide Code";
                    }
                };
            }

            sp.Children.Add(headerGrid);
            sp.Children.Add(content);
            if (codeBox != null)
            {
                sp.Children.Add(codeBox);
            }
            card.Child = sp;
            mainSp.Children.Add(card);
        }

        // Helper to construct sample BIM / Structural engineering cards
        UIElement CreateProjectCard(string title, string subtitle, string tag, IconKind icon, Color accentColor, string metric)
        {
            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // Top Header: Icon + Tag
            var topBar = new Grid();
            topBar.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            topBar.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var iconBox = new Border
            {
                Width = 32, Height = 32,
                CornerRadius = new CornerRadius(8),
                Background = new SolidColorBrush(Color.FromArgb(30, accentColor.R, accentColor.G, accentColor.B)),
                HorizontalAlignment = HorizontalAlignment.Left
            };
            var ico = new Icon { Kind = icon, Width = 16, Height = 16, Foreground = new SolidColorBrush(accentColor), HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
            iconBox.Child = ico;
            Grid.SetColumn(iconBox, 0);
            topBar.Children.Add(iconBox);

            var tagBadge = new Border
            {
                CornerRadius = new CornerRadius(4),
                Padding = new Thickness(6, 2, 6, 2),
                Background = new SolidColorBrush(Color.FromArgb(24, accentColor.R, accentColor.G, accentColor.B))
            };
            tagBadge.Child = new TextBlock { Text = tag, FontSize = 10.5, FontWeight = FontWeights.Normal, Foreground = new SolidColorBrush(accentColor) };
            Grid.SetColumn(tagBadge, 1);
            topBar.Children.Add(tagBadge);

            Grid.SetRow(topBar, 0);
            grid.Children.Add(topBar);

            // Middle: Title & Description
            var midPanel = new StackPanel { VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 8, 0, 8) };
            var txtTitle = new TextBlock { Text = title, FontWeight = FontWeights.Normal, FontSize = 13.5 };
            txtTitle.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextPrimary");
            var txtSub = new TextBlock { Text = subtitle, FontSize = 11.5, Margin = new Thickness(0, 2, 0, 0), TextWrapping = TextWrapping.Wrap };
            txtSub.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");
            midPanel.Children.Add(txtTitle);
            midPanel.Children.Add(txtSub);

            Grid.SetRow(midPanel, 1);
            grid.Children.Add(midPanel);

            // Bottom: Key Metric Indicator
            var btmBorder = new Border { BorderThickness = new Thickness(0, 1, 0, 0), Padding = new Thickness(0, 6, 0, 0) };
            btmBorder.SetResourceReference(Border.BorderBrushProperty, "CONCOST.Brush.Border");
            var txtMetric = new TextBlock { Text = metric, FontSize = 11.5, FontWeight = FontWeights.Normal };
            txtMetric.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextPrimary");
            btmBorder.Child = txtMetric;

            Grid.SetRow(btmBorder, 2);
            grid.Children.Add(btmBorder);

            return grid;
        }

        // ---------------------------------------------------------------------
        // SECTION 1: Interactive BIM Engineering Card Carousel
        // ---------------------------------------------------------------------
        var carousel1 = new CardCarousel
        {
            Height = 270,
            ItemWidth = 280,
            ItemHeight = 175,
            Spacing = 190,
            AutoRun = false,
            SelectedIndex = 1,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };

        var card1 = new CardCarouselItem { Content = CreateProjectCard("Landmark 81 Tower", "Structural FEA & Wind Analysis", "ACTIVE", IconKind.Layers, Color.FromRgb(37, 99, 235), "81 Storeys • B40 Concrete") };
        var card2 = new CardCarouselItem { Content = CreateProjectCard("Thu Thiem Bridge 2", "Cable-stayed Bridge Takeoff", "IN PROGRESS", IconKind.Ruler, Color.FromRgb(245, 158, 11), "Span 885m • 14,500T Steel") };
        var card3 = new CardCarouselItem { Content = CreateProjectCard("Long Thanh Airport T1", "Roof Truss & Precast Columns", "APPROVED", IconKind.CheckCircle, Color.FromRgb(16, 185, 129), "400,000 m² Floor Area") };
        var card4 = new CardCarouselItem { Content = CreateProjectCard("Metro Line No.1 Ben Thanh", "Underground Station Box", "COMPLETED", IconKind.ShieldCheck, Color.FromRgb(139, 92, 246), "3 Sub-levels • Diaphragm Wall") };
        var card5 = new CardCarouselItem { Content = CreateProjectCard("Vinhomes Central Park", "High-rise Pile Cap Optimization", "PLANNED", IconKind.Clock, Color.FromRgb(14, 165, 233), "D2500 Bored Piles") };

        carousel1.Items.Add(card1);
        carousel1.Items.Add(card2);
        carousel1.Items.Add(card3);
        carousel1.Items.Add(card4);
        carousel1.Items.Add(card5);

        string sec1Code = @"<!-- ==================== XAML MARKUP ==================== -->
<dash:CardCarousel Height=""240""
                   ItemWidth=""260""
                   ItemHeight=""165""
                   Spacing=""150""
                   SelectedIndex=""1""
                   AutoRun=""False"">
    <dash:CardCarouselItem>
        <Grid>
            <!-- Custom Card Template UI -->
            <TextBlock Text=""Landmark 81 Tower"" FontWeight=""Normal"" />
            <TextBlock Text=""81 Storeys • B40 Concrete"" FontSize=""11.5"" />
        </Grid>
    </dash:CardCarouselItem>

    <dash:CardCarouselItem>
        <Grid>
            <TextBlock Text=""Thu Thiem Bridge 2"" FontWeight=""Normal"" />
            <TextBlock Text=""Span 885m • 14,500T Steel"" FontSize=""11.5"" />
        </Grid>
    </dash:CardCarouselItem>

    <dash:CardCarouselItem>
        <Grid>
            <TextBlock Text=""Long Thanh Airport T1"" FontWeight=""Normal"" />
            <TextBlock Text=""400,000 m² Floor Area"" FontSize=""11.5"" />
        </Grid>
    </dash:CardCarouselItem>
</dash:CardCarousel>

<!-- ==================== C# CODE-BEHIND ==================== -->
// Programmatic slide movement and event handling
carousel.MoveNext();
carousel.MovePrev();
carousel.SelectionChanged += (s, e) =>
{
    Console.WriteLine($""Active Card Index: {e.NewValue}"");
};";
        AddCardSection("1. CardCarousel chiều sâu 3D cho mô hình dự án và workspace", carousel1, sec1Code);

        return mainSp;
    }
}


