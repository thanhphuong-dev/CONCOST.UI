using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CONCOST.UI.Controls.Ribbon;
using CONCOST.UI.Icons;

namespace CONCOST.UI.RevitLike.Shell;

/// <summary>
/// Composes the Revit-style ribbon: one tab per discipline, plus the contextual tab that appears
/// when something is selected, and the backstage behind the File button.
/// </summary>
/// <remarks>
/// The builder holds a reference to the window instead of a bag of forty commands: every command is
/// already a public member there, so wiring is a straight property read and a rename in the window
/// cannot silently leave a ribbon button dead.
/// </remarks>
internal sealed class RibbonBuilder
{
    private readonly MainWindow _host;

    public RibbonBuilder(MainWindow host) => _host = host;

    /// <summary>The tab that only exists while something is selected.</summary>
    public RibbonTabItem ModifyTab { get; private set; } = null!;

    public Ribbon Build()
    {
        var ribbon = new Ribbon
        {
            ShowBorder = false,
            IsGroupCollapseEnabled = true,
            IsQuickAccessCustomizable = true,
            IsKeyTipEnabled = true,
            ShowQuickAccessToolBar = true,
            EmptyContent = new TextBlock { Text = "No commands", Margin = new Thickness(12, 8, 12, 8) },
            ItemsSource = new List<RibbonTabItem>
            {
                ArchitectureTab(),
                StructureTab(),
                AnnotateTab(),
                ViewTab(),
                ManageTab(),
                BuildModifyTab()
            }
        };

        ribbon.SelectedTabId = "Architecture";

        return ribbon;
    }

    // ------------------------------------------------------------------ tabs

    private RibbonTabItem ArchitectureTab() => Tab("Architecture", IconKind.Home, "A",
        Group("Build")
            .Large("Wall", IconKind.PenTool, _host.WallCommand, "WA", "Place a straight wall: click the start point, then the end point")
            .Large("Modify", IconKind.Edit, _host.ModifyCommand, "MD", "Leave the active tool and edit what is already drawn")
            .Large("Delete", IconKind.TrashCan, _host.DeleteCommand, "DE", "Delete the selected walls").Done(),

        // Small and medium items share one 72 px column, so their captions are kept short and the
        // full sentence lives in the screen tip — the same trade every real ribbon makes.
        Group("Select")
            .Small("All", IconKind.SelectAll, _host.SelectAllCommand, "Ctrl+A", "Select every wall in the view")
            .Small("None", IconKind.DeselectAll, _host.DeselectAllCommand, "Esc", "Clear the selection")
            .Small("Invert", IconKind.InvertSelection, _host.InvertSelectionCommand, null, "Select what is not selected and deselect the rest").Done(),

        Group("Room")
            .Medium("Tags", IconKind.TagItem, _host.ToggleRoomTagsCommand, "RT", "Show or hide the room number and name on the plan")
            .Medium("Report", IconKind.SummaryReport, _host.RoomReportCommand, "RR", "List every room with its area in the command line").Done(),

        Group("Properties")
            .Large("Properties", IconKind.SettingsSliders, _host.PropertiesCommand, "PP", "Show the properties of the current selection").Done());

    private RibbonTabItem StructureTab() => Tab("Structure", IconKind.Beam, "S",
        Group("Structure")
            .Large("Structural Wall", IconKind.Slab, _host.StructuralWallCommand, "SW", "Place a 400 mm structural wall instead of a partition")
            .Large("Add Level", IconKind.LevelTool, _host.AddLevelCommand, "LL", "Add the next storey above the highest one").Done(),

        Group("Editing")
            .Small("Undo", IconKind.Undo, _host.UndoCommand, "Ctrl+Z")
            .Small("Redo", IconKind.Redo, _host.RedoCommand, "Ctrl+Y").Done());

    private RibbonTabItem AnnotateTab() => Tab("Annotate", IconKind.DimensionLinear, "N",
        Group("Dimension")
            .Large("Linear", IconKind.DimensionLinear, _host.LinearDimensionCommand, "DI", "Measure between two points; the result stays on screen until cleared")
            .Large("Clear", IconKind.Eraser, _host.ClearAnnotationsCommand, "CX", "Remove the temporary dimension").Done(),

        Group("Tags")
            .Medium("Tags", IconKind.TagItem, _host.ToggleRoomTagsCommand, "RT", "Show or hide the room number and name on the plan")
            .Medium("Dims", IconKind.DimensionLinear, _host.ToggleDimensionsCommand, "DT", "Show or hide the dimension strings around the plan").Done());

    private RibbonTabItem ViewTab() => Tab("View", IconKind.Viewport3D, "V",
        Group("Graphics")
            .Large("Shaded", IconKind.RenderShaded, _host.ShadedCommand, "SD", "Fill the rooms and draw solid walls")
            .Large("Hidden Line", IconKind.RenderHiddenLines, _host.HiddenLineCommand, "HL", "Outline the walls without filling the rooms")
            .Large("Wireframe", IconKind.RenderWireframe, _host.WireframeCommand, "WF", "Thin lines only").Done(),

        Group("Windows")
            .Small("Tile", IconKind.ViewCards, _host.TileVerticalCommand, "WT", "Show two views side by side")
            .Small("Single", IconKind.ViewList, _host.TileSingleCommand, "WS", "One view at a time")
            .Small("Close", IconKind.CloseAll, _host.CloseAllViewsCommand, null, "Close every open view").Done(),

        Group("Navigate")
            .Large("Zoom Extents", IconKind.ZoomFit, _host.ZoomExtentsCommand, "ZE")
            .Small("In", IconKind.ZoomIn, _host.ZoomInCommand, null, "Zoom in")
            .Small("Out", IconKind.ZoomOut, _host.ZoomOutCommand, null, "Zoom out").Done(),

        Group("Views")
            .Medium("Level 1", IconKind.Viewport2D, _host.OpenLevel1Command, "L1")
            .Medium("Level 2", IconKind.Viewport2D, _host.OpenLevel2Command, "L2")
            .Medium("3D View", IconKind.Viewport3D, _host.Open3DViewCommand, "F3").Done());

    private RibbonTabItem ManageTab() => Tab("Manage", IconKind.Settings, "M",
        Group("Project")
            .Large("Project Info", IconKind.Info, _host.ProjectInfoCommand, "PI", "Project name, number and element counts")
            .Large("About", IconKind.Help, _host.AboutCommand, "AB", "What this sample demonstrates").Done(),

        Group("Interface")
            .Large("Theme", IconKind.LightMode, _host.ToggleThemeCommand, "TH", "Switch between the light and dark theme")
            .Large("Command Line", IconKind.Menu, _host.ToggleCommandLineCommand, "CL", "Show or hide the command line").Done());

    private RibbonTabItem BuildModifyTab()
    {
        ModifyTab = Tab("Modify | Walls", IconKind.Edit, "MO",
            Group("Selection")
                .Large("Properties", IconKind.SettingsSliders, _host.PropertiesCommand, "PP")
                .Large("Delete", IconKind.TrashCan, _host.DeleteCommand, "DE").Done(),

            Group("History")
                .Large("Undo", IconKind.Undo, _host.UndoCommand, "Ctrl+Z")
                .Large("Redo", IconKind.Redo, _host.RedoCommand, "Ctrl+Y").Done());

        ModifyTab.IsContextual = true;
        ModifyTab.ContextualGroup = "Selection";
        ModifyTab.ContextualColor = ThemeAccentBrush();
        ModifyTab.IsAvailable = false;

        return ModifyTab;
    }

    // ------------------------------------------------------------------ backstage

    /// <summary>Builds the File backstage: a rail of pages, with the project sheet as the first one.</summary>
    public RibbonBackstageView BuildBackstage()
    {
        var backstage = new RibbonBackstageView { ApplicationName = "CONCOST Revit-Like" };

        backstage.Items.Add(new BackstageItem("Info", IconKind.Info) { Content = _host.BuildProjectInfoPanel() });

        backstage.Items.Add(Separator());

        backstage.Items.Add(Page("New", IconKind.FileText, _host.NewCommand));
        backstage.Items.Add(Page("Open", IconKind.Open, _host.OpenCommand));
        backstage.Items.Add(Page("Save", IconKind.Save, _host.SaveCommand));
        backstage.Items.Add(Page("Save As", IconKind.SaveAs, _host.SaveAsCommand));
        backstage.Items.Add(Page("Export", IconKind.ExportFile, _host.ExportCommand));
        backstage.Items.Add(Page("Print", IconKind.PrintDoc, _host.PrintCommand));

        backstage.Items.Add(Separator());

        backstage.Items.Add(Page("Close", IconKind.Close, _host.ExitCommand));

        return backstage;
    }

    private static BackstageItem Separator() => new(string.Empty) { IsSeparator = true };

    private static BackstageItem Page(string header, IconKind icon, ICommand command)
    {
        // A backstage page that only runs a command has nothing to lay out, so it shows the command
        // it stands for and runs it from the button underneath.
        var body = new StackPanel { Margin = new Thickness(32, 28, 32, 28) };

        var title = new TextBlock { Text = header, FontSize = 24, FontWeight = FontWeights.SemiBold };
        title.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextPrimary");

        var caption = new TextBlock
        {
            Text = "This sample keeps the project in memory; the command reports what it would do.",
            FontSize = 12,
            Margin = new Thickness(0, 6, 0, 20),
            TextWrapping = TextWrapping.Wrap,
            MaxWidth = 460
        };

        caption.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");

        var run = new Button
        {
            Content = header,
            Height = 34,
            Padding = new Thickness(18, 0, 18, 0),
            HorizontalAlignment = HorizontalAlignment.Left,
            Command = command
        };

        run.SetResourceReference(FrameworkElement.StyleProperty, "CONCOST.Button.Primary");

        body.Children.Add(title);
        body.Children.Add(caption);
        body.Children.Add(run);

        return new BackstageItem(header, icon) { Content = body };
    }

    // ------------------------------------------------------------------ building blocks

    private static RibbonTabItem Tab(string header, IconKind icon, string keyTip, params RibbonGroup[] groups)
    {
        var tab = new RibbonTabItem
        {
            Header = header,
            Id = header,
            Icon = ShellIcons.Get(icon),
            KeyTip = keyTip,
            Content = new List<RibbonGroup>(groups)
        };

        return tab;
    }

    private static GroupBuilder Group(string header) => new(header);

    internal static RibbonButton Item(string header, IconKind icon, ICommand? command,
        RibbonItemSize size, string? gesture = null, string? tip = null)
    {
        var button = new RibbonButton
        {
            Header = header,
            Id = header,
            Icon = ShellIcons.Get(icon),
            Command = command,
            Size = size,
            KeyTip = DeriveKeyTip(header),
            GestureText = gesture ?? string.Empty,
            ScreenTip = tip ?? header
        };

        return button;
    }

    /// <summary>First letters of the words, which is how Office and AutoCAD pick key tips.</summary>
    internal static string DeriveKeyTip(string header)
    {
        var letters = new System.Text.StringBuilder(2);

        foreach (string word in header.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            letters.Append(char.ToUpperInvariant(word[0]));

            if (letters.Length == 2) break;
        }

        return letters.ToString();
    }

    private static System.Windows.Media.Brush ThemeAccentBrush()
    {
        if (Application.Current?.TryFindResource("CONCOST.Brush.Primary") is System.Windows.Media.Brush brush) return brush;

        return System.Windows.Media.Brushes.SteelBlue;
    }

    /// <summary>Collects items for one <see cref="RibbonGroup"/> so a tab reads as a list of commands.</summary>
    private sealed class GroupBuilder
    {
        private readonly RibbonGroup _group;

        public GroupBuilder(string header) => _group = new RibbonGroup { Header = header };

        public GroupBuilder Large(string header, IconKind icon, ICommand? command, string? gesture = null, string? tip = null)
            => Add(header, icon, command, RibbonItemSize.Large, gesture, tip);

        public GroupBuilder Medium(string header, IconKind icon, ICommand? command, string? gesture = null, string? tip = null)
            => Add(header, icon, command, RibbonItemSize.Medium, gesture, tip);

        public GroupBuilder Small(string header, IconKind icon, ICommand? command, string? gesture = null, string? tip = null)
            => Add(header, icon, command, RibbonItemSize.Small, gesture, tip);

        /// <summary>A split button: the body runs one command, the arrow lists the alternatives.</summary>
        public GroupBuilder Split(string header, IconKind icon, ICommand? primary, params (string Header, IconKind Icon, ICommand Command)[] entries)
        {
            var split = new RibbonSplitButton
            {
                Header = header,
                Id = header,
                Icon = ShellIcons.Get(icon),
                Command = primary,
                Size = RibbonItemSize.Large,
                ScreenTip = header,
                KeyTip = DeriveKeyTip(header)
            };

            foreach (var entry in entries)
            {
                split.Items.Add(new RibbonMenuItem
                {
                    Header = entry.Header,
                    Icon = ShellIcons.Get(entry.Icon),
                    Command = entry.Command
                });
            }

            _group.Items.Add(split);

            return this;
        }

        public RibbonGroup Done() => _group;

        private GroupBuilder Add(string header, IconKind icon, ICommand? command, RibbonItemSize size,
            string? gesture, string? tip)
        {
            _group.Items.Add(Item(header, icon, command, size, gesture, tip));

            return this;
        }
    }
}
