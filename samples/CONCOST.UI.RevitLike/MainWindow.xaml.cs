using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Media3D;
using CONCOST.UI.Controls;
using CONCOST.UI.Controls.Ribbon;
using CONCOST.UI.Core.UndoRedo;
using CONCOST.UI.Icons;
using CONCOST.UI.RevitLike.Model;
using CONCOST.UI.RevitLike.Shell;
using CONCOST.UI.Services;
using CONCOST.UI.Themes;

namespace CONCOST.UI.RevitLike;

/// <summary>
/// A Revit-shaped shell over the CONCOST.UI control set: ribbon, backstage, project browser, tabbed
/// views with a view cube, properties palette, command line and a drafting status bar.
/// </summary>
/// <remarks>
/// The window owns the model and the services; the panels only render what they are given. Every
/// edit goes through <see cref="TransactionManager"/>, so the Undo button, the Ctrl+Z accelerator and
/// the UNDO command all move the same stack — and the properties palette writes through the same
/// manager, which is why changing a thickness in the palette is undoable too.
/// </remarks>
public partial class MainWindow : Window
{
    private readonly BuildingModel _model = new();
    private readonly SelectionManager _selection = new();
    private readonly List<ViewPane> _panes = new();

    private readonly PropertyManager _properties;
    private readonly TransactionManager _transactions;

    private RibbonBuilder _ribbonBuilder = null!;
    private Ribbon _ribbon = null!;
    private RibbonBackstageView _backstage = null!;
    private DocumentWorkspace _workspace = null!;
    private ProjectBrowser _browser = null!;
    private PropertiesPalette _palette = null!;
    private CommandLine _commandLine = null!;
    private CadStatusBar _status = null!;

    private ViewPane? _active;
    private ToolMode _tool = ToolMode.Select;
    private WallPreset _preset = WallPreset.Partition;
    private Point3D? _startPoint;
    private bool _isModified;
    private bool _showRoomTags = true;
    private bool _showDimensions = true;
    private bool _commandLineVisible = true;
    private bool _ortho;
    private bool _polar;
    private bool _syncingToggles;
    private VisualStyleMode _style = VisualStyleMode.HiddenLine;

    public MainWindow()
    {
        InitializeComponent();

        // A window taller than the work area hangs off the bottom of the screen, and the rows that
        // vanish first are the command line and the status bar. Treat the XAML size as a maximum.
        Rect workArea = SystemParameters.WorkArea;

        Width = Math.Min(Width, workArea.Width - 24);
        Height = Math.Min(Height, workArea.Height - 24);

        _transactions = new TransactionManager(UndoRedoManager.Current);
        _properties = new PropertyManager(_transactions);

        BuildCommandSet();
        RegisterModelProperties();

        BuildShell();
        WireShell();

        // After the shell exists: the command table is fed straight into the command line control.
        RegisterCommands();

        OpenView("Level 1");
        WriteWelcome();
    }

    // ------------------------------------------------------------------ shell construction

    private void BuildShell()
    {
        _ribbonBuilder = new RibbonBuilder(this);

        _ribbon = _ribbonBuilder.Build();
        RibbonHost.Content = _ribbon;

        _backstage = _ribbonBuilder.BuildBackstage();
        BackstageHost.Content = _backstage;

        _browser = new ProjectBrowser();
        BuildProjectTree(_browser);
        BrowserHost.Content = _browser;

        _workspace = new DocumentWorkspace
        {
            PlaceholderTitle = "No views open",
            PlaceholderText = "Select a view in the project browser and press Enter."
        };

        WorkspaceHost.Content = _workspace;

        _palette = new PropertiesPalette(_properties, _selection);
        PaletteHost.Content = _palette;

        // No fixed height: the control already sizes to its feed plus its input row, and a taller
        // box only adds an empty band between the last message and the prompt.
        _commandLine = new CommandLine
        {
            HistoryHeight = 72,
            ShowHistory = true,
            Prompt = "Command"
        };

        CommandHost.Content = _commandLine;

        _status = new CadStatusBar
        {
            WorkPlane = UcsMode.Top,
            UnitSuffix = "mm",
            ShowCoordinates = true,
            GridEnabled = true,
            SnapEnabled = true,
            ObjectSnapEnabled = true
        };

        StatusHost.Content = _status;
    }

    private void BuildProjectTree(ProjectBrowser browser)
    {
        ProjectTreeNode root = browser.AddRoot(_model.ProjectName, ProjectNodeKind.Project);

        ProjectTreeNode plans = root.Add("Floor Plans", ProjectNodeKind.Folder);
        _plansNode = plans;

        foreach (LevelModel level in _model.Levels) plans.Add(level.Name, ProjectNodeKind.View);

        ProjectTreeNode models = root.Add("3D Views", ProjectNodeKind.Folder);
        models.Add("{3D}", ProjectNodeKind.View);

        ProjectTreeNode sheets = root.Add("Sheets", ProjectNodeKind.Folder);
        sheets.Add("A101 - Ground Floor Plan", ProjectNodeKind.Sheet);
        sheets.Add("A102 - First Floor Plan", ProjectNodeKind.Sheet);

        ProjectTreeNode families = root.Add("Families", ProjectNodeKind.Folder);
        families.Add("Basic Wall", ProjectNodeKind.FamilySymbol);
        families.Add("Rooms", ProjectNodeKind.FamilySymbol);

        // AddRoot builds the tree from the nodes as they were at that moment, so the children added
        // afterwards only appear once the tree is rebuilt.
        browser.Refresh();
        browser.ExpandAll();
    }

    private void WireShell()
    {
        FileButton.SetResourceReference(StyleProperty, "CONCOST.Button.Primary");

        _browser.OpenRequested += OnBrowserOpenRequested;
        _browser.PropertiesRequested += (_, _) => PropertiesCommand.Execute(null);

        _workspace.ActiveDocumentChanged += OnActiveDocumentChanged;

        // The Info page is built once for the lifetime of the window, so its counters are refreshed
        // in place rather than by rebuilding the page.
        _backstage.IsOpenChanged += (_, _) => UpdateProjectInfo();

        _selection.SelectionChanged += (_, _) => SyncSelection();
        _properties.ValueChanged += (_, _) => OnModelChanged();

        _status.ToggleChanged += OnStatusToggleChanged;
        _status.WorkPlaneChanged += OnWorkPlaneChanged;
        _status.NotificationSelected += OnNotificationSelected;

        _commandLine.CommandInvoked += OnCommandInvoked;
        _commandLine.UnknownCommand += (_, text) =>
            _commandLine.WriteLine($"Unknown command \"{text}\". Type HELP for the list.", CommandMessageKind.Error);

        PreviewKeyDown += OnWindowKeyDown;

        BuildQuickAccess();
    }

    private void BuildQuickAccess()
    {
        TitleBar.QuickAccess.Add(new TitleBarAction(IconKind.FileText, "New project", NewCommand));
        TitleBar.QuickAccess.Add(new TitleBarAction(IconKind.Open, "Open project", OpenCommand));
        TitleBar.QuickAccess.Add(new TitleBarAction(IconKind.Save, "Save project", SaveCommand));
        TitleBar.QuickAccess.Add(new TitleBarAction(IconKind.Undo, "Undo (Ctrl+Z)", UndoCommand));
        TitleBar.QuickAccess.Add(new TitleBarAction(IconKind.Redo, "Redo (Ctrl+Y)", RedoCommand));
        TitleBar.QuickAccess.Add(new TitleBarAction(IconKind.PenTool, "Place a wall (WA)", WallCommand));

        TitleBar.RefreshQuickAccess();
    }

    // ------------------------------------------------------------------ views

    private void OnBrowserOpenRequested(object? sender, ProjectNodeEventArgs e)
    {
        if (e.Node.Kind == ProjectNodeKind.View) OpenView(e.Node.Name);
    }

    /// <summary>Opens a view tab, or brings the existing one forward.</summary>
    private void OpenView(string title)
    {
        DocumentPane? existing = _workspace.Documents.FirstOrDefault(document => document.Title == title);

        if (existing is not null)
        {
            _workspace.ActiveDocument = existing;
            return;
        }

        bool isThreeD = title.Contains("3D", StringComparison.OrdinalIgnoreCase);
        LevelModel? level = isThreeD ? null : _model.Levels.FirstOrDefault(candidate => candidate.Name == title);

        var pane = new ViewPane(_model, level, isThreeD)
        {
            // Room tags are a plan annotation; in the 3D view they would stack on top of each other
            // and cover the model.
            Renderer = { Style = _style, ShowRoomTags = _showRoomTags && !isThreeD, ShowDimensions = _showDimensions && !isThreeD }
        };

        pane.Viewport.WorldClicked += (_, e) => OnWorldClicked(pane, e);
        pane.Viewport.CursorMoved += (_, e) => OnCursorMoved(pane, e);
        pane.Controls.SettingsChanged += (_, name) => OnViewSettingsChanged(pane, name);

        _panes.Add(pane);

        _workspace.AddDocument(title, pane, isThreeD ? IconKind.Viewport3D : IconKind.Viewport2D);
    }

    private void OnActiveDocumentChanged(object? sender, EventArgs e)
    {
        _active = _workspace.ActiveDocument?.Content as ViewPane;

        if (_active is null) return;

        _status.WorkPlane = _active.IsThreeD ? UcsMode.World : UcsMode.Top;
        TitleBar.DocumentTitle = _workspace.ActiveDocument?.Title ?? _model.ProjectName;

        SyncSelection();
    }

    private void OnViewSettingsChanged(ViewPane pane, string propertyName)
    {
        // The view control bar is the single source of truth for the drawing style: writing it back
        // into the renderer keeps the View tab and the bar from disagreeing.
        if (propertyName == nameof(ViewControlBar.VisualStyle))
        {
            _style = pane.Controls.VisualStyle;
            pane.Renderer.Style = _style;
            pane.Redraw();
        }

        if (propertyName == nameof(ViewControlBar.DetailLevel))
        {
            _commandLine.WriteLine($"Detail level: {pane.Controls.DetailLevel}", CommandMessageKind.Hint);
        }
    }

    // ------------------------------------------------------------------ pointer tools

    private void OnWorldClicked(ViewPane pane, ViewportCursorEventArgs e)
    {
        switch (_tool)
        {
            case ToolMode.Wall:
                HandleWallPoint(pane, e.World);
                return;

            case ToolMode.Dimension:
                HandleDimensionPoint(pane, e.World);
                return;

            default:
                WallElement? hit = PickWall(pane, e.Screen);

                // Clicking empty space clears the selection; saying so every time buries the messages
                // that matter, so it is only worth a line when something actually was selected.
                if (hit is null && _selection.Count == 0) return;

                _selection.Select(hit);

                if (hit is null) _commandLine.WriteLine("Selection cleared.", CommandMessageKind.Hint);

                return;
        }
    }

    private void HandleWallPoint(ViewPane pane, Point3D world)
    {
        if (_startPoint is null)
        {
            _startPoint = world;
            _commandLine.WriteLine($"Wall start {FormatPoint(world)} — pick the end point.", CommandMessageKind.Hint);
            return;
        }

        Point3D start = _startPoint.Value;
        Point3D end = ConstrainPoint(start, world);

        _startPoint = null;
        pane.Renderer.Preview = null;

        if ((end - start).Length < 50)
        {
            _commandLine.WriteLine("The wall is too short to place.", CommandMessageKind.Error);
            pane.Redraw();

            return;
        }

        AddWall(pane, start, end);
    }

    private void HandleDimensionPoint(ViewPane pane, Point3D world)
    {
        if (_startPoint is null)
        {
            _startPoint = world;
            _commandLine.WriteLine($"Measure from {FormatPoint(world)} — pick the second point.", CommandMessageKind.Hint);
            return;
        }

        Point3D start = _startPoint.Value;
        _startPoint = null;

        double length = (world - start).Length;

        pane.Viewport.TemporaryDimension = new TemporaryDimension
        {
            Start = start,
            End = world,
            Offset = 36,
            Text = $"{length:0} mm"
        };

        _commandLine.WriteLine($"Distance {length:0} mm, angle {AngleOf(start, world):0.0}°", CommandMessageKind.Output);
    }

    private void AddWall(ViewPane pane, Point3D start, Point3D end)
    {
        LevelModel? level = pane.Level;

        if (level is null)
        {
            Notify("3D view", "Walls are drawn in a plan view. Open Level 1 or Level 2.", CadNotificationSeverity.Warning);
            return;
        }

        var wall = new WallElement(_preset.Name, new Point3D(start.X, start.Y, 0), new Point3D(end.X, end.Y, 0))
        {
            Thickness = _preset.Thickness,
            Height = _preset.Height,
            Material = _preset.Material,
            Function = _preset.Function,
            IsStructural = _preset.IsStructural
        };

        // Record runs the lambda, so the list is updated for us and Undo replays the inverse.
        _transactions.Run("Place wall", transaction =>
            transaction.Record("Add wall", () => level.Walls.Add(wall), () => level.Walls.Remove(wall)));

        OnModelChanged();

        _selection.Select(wall);
        _commandLine.WriteLine($"{wall.Name} placed, {wall.Length:0} mm long.", CommandMessageKind.Output);
    }

    /// <summary>Nearest wall to the pointer, in screen pixels, or <c>null</c> when nothing is close.</summary>
    private WallElement? PickWall(ViewPane pane, Point screen)
    {
        WallElement? best = null;
        double bestDistance = 12;

        foreach (LevelModel level in LevelsOf(pane))
        {
            double z = level.Elevation;

            foreach (WallElement wall in level.Walls)
            {
                Point a = pane.Viewport.ToScreen(new Point3D(wall.Start.X, wall.Start.Y, z + wall.BaseOffset));
                Point b = pane.Viewport.ToScreen(new Point3D(wall.End.X, wall.End.Y, z + wall.BaseOffset));

                double distance = DistanceToSegment(screen, a, b);

                if (distance >= bestDistance) continue;

                bestDistance = distance;
                best = wall;
            }
        }

        return best;
    }

    private IEnumerable<LevelModel> LevelsOf(ViewPane pane)
    {
        if (pane.Level is not null)
        {
            yield return pane.Level;
            yield break;
        }

        foreach (LevelModel level in _model.Levels) yield return level;
    }

    /// <summary>Ortho projects onto the dominant axis; polar turns the point to the nearest 45°.</summary>
    private Point3D ConstrainPoint(Point3D start, Point3D raw)
    {
        double dx = raw.X - start.X;
        double dy = raw.Y - start.Y;

        if (Math.Abs(dx) < 1e-9 && Math.Abs(dy) < 1e-9) return raw;

        if (_ortho)
        {
            return Math.Abs(dx) >= Math.Abs(dy)
                ? new Point3D(raw.X, start.Y, raw.Z)
                : new Point3D(start.X, raw.Y, raw.Z);
        }

        if (!_polar) return raw;

        double length = Math.Sqrt((dx * dx) + (dy * dy));
        double angle = Math.Round(Math.Atan2(dy, dx) * 180 / Math.PI / 45) * 45 * Math.PI / 180;

        return new Point3D(start.X + (Math.Cos(angle) * length), start.Y + (Math.Sin(angle) * length), raw.Z);
    }

    /// <summary>The coordinate readout the status bar uses, so the hints and the bar agree.</summary>
    private static string FormatPoint(Point3D point) => CadStatusBar.FormatCoordinates(point, "mm");

    private static double AngleOf(Point3D start, Point3D end)
    {
        double angle = Math.Atan2(end.Y - start.Y, end.X - start.X) * 180 / Math.PI;

        return angle < 0 ? angle + 360 : angle;
    }

    private static double DistanceToSegment(Point point, Point a, Point b)
    {
        double dx = b.X - a.X;
        double dy = b.Y - a.Y;
        double lengthSquared = (dx * dx) + (dy * dy);

        if (lengthSquared < 1e-9) return (point - a).Length;

        double t = Math.Max(0, Math.Min(1, (((point.X - a.X) * dx) + ((point.Y - a.Y) * dy)) / lengthSquared));
        var projection = new Point(a.X + (t * dx), a.Y + (t * dy));

        return (point - projection).Length;
    }

    private void OnCursorMoved(ViewPane pane, ViewportCursorEventArgs e)
    {
        _status.UpdateCoordinates(e.World);

        if (_tool != ToolMode.Wall || _startPoint is not { } start) return;

        pane.Renderer.Preview = (start, ConstrainPoint(start, e.World));
        pane.Redraw();
    }

    // ------------------------------------------------------------------ state sync

    private void SyncSelection()
    {
        foreach (ViewPane pane in _panes)
        {
            pane.Renderer.SelectedWall = _selection.Primary as WallElement;
            pane.Redraw();
        }

        _status.SelectionCount = _selection.Count;

        // The contextual tab is the sample's demonstration of IsAvailable: the tab keeps its place in
        // the item source and simply stops being offered.
        _ribbonBuilder.ModifyTab.IsAvailable = _selection.Count > 0;

        if (_selection.Count > 0) _ribbon.SelectedTabId = _ribbonBuilder.ModifyTab.Id;
        else if (_ribbon.SelectedTabId == _ribbonBuilder.ModifyTab.Id) _ribbon.SelectedTabId = "Architecture";
    }

    private void OnModelChanged()
    {
        MarkModified();

        foreach (ViewPane pane in _panes) pane.Redraw();

        _palette.Refresh();
        UpdateProjectInfo();
        RefreshCommandStates();
    }

    private void MarkModified(bool modified = true)
    {
        _isModified = modified;
        TitleBar.IsDocumentModified = modified;
    }

    private void OnStatusToggleChanged(object? sender, CadToggleEventArgs e)
    {
        if (_syncingToggles) return;

        switch (e.Toggle)
        {
            case CadToggle.Grid:
                foreach (ViewPane pane in _panes) pane.Viewport.ShowGrid = e.IsEnabled;
                break;

            case CadToggle.Snap:
                foreach (ViewPane pane in _panes) pane.Viewport.SnapToGrid = e.IsEnabled;
                break;

            case CadToggle.Ortho:
                _ortho = e.IsEnabled;
                if (e.IsEnabled) MirrorToggle(CadToggle.Polar, ref _polar, false);
                break;

            case CadToggle.Polar:
                _polar = e.IsEnabled;
                if (e.IsEnabled) MirrorToggle(CadToggle.Ortho, ref _ortho, false);
                break;
        }
    }

    /// <summary>Writes the counterpart toggle back without letting the echo re-enter the handler.</summary>
    private void MirrorToggle(CadToggle toggle, ref bool field, bool value)
    {
        field = value;
        _syncingToggles = true;
        _status.SetToggle(toggle, value);
        _syncingToggles = false;
    }

    private void OnWorkPlaneChanged(object? sender, EventArgs e)
    {
        foreach (ViewPane pane in _panes)
        {
            if (pane.IsThreeD) continue;

            pane.Navigation.Ucs = _status.WorkPlane;
        }

        _commandLine.WriteLine($"Work plane: {_status.WorkPlane}", CommandMessageKind.Hint);
    }

    private void OnNotificationSelected(object? sender, CadNotification notification)
    {
        InfoBar.Show(notification.Title, notification.Message, notification.Severity);
    }

    // ------------------------------------------------------------------ helpers

    /// <summary>Shows a message in the info bar, the status bar list and the command line at once.</summary>
    private void Notify(string title, string message, CadNotificationSeverity severity = CadNotificationSeverity.Info)
    {
        InfoBar.Show(title, message, severity);
        _status.Notify(new CadNotification(title, message, severity));

        _commandLine.WriteLine(
            $"{title}: {message}",
            severity == CadNotificationSeverity.Error ? CommandMessageKind.Error : CommandMessageKind.Output);
    }

    private void CancelTool()
    {
        if (_tool == ToolMode.Select && _startPoint is null) return;

        _tool = ToolMode.Select;
        _startPoint = null;

        foreach (ViewPane pane in _panes)
        {
            pane.Renderer.Preview = null;
            pane.Redraw();
        }

        _commandLine.WriteLine("*Cancel*", CommandMessageKind.Hint);
    }

    private void OnFileButtonClick(object sender, RoutedEventArgs e) => _backstage.Open();
}
