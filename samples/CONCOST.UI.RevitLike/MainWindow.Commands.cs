using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using CONCOST.UI.Controls;
using CONCOST.UI.Core.UndoRedo;
using CONCOST.UI.RevitLike.Model;
using CONCOST.UI.RevitLike.Shell;
using CONCOST.UI.Services;
using CONCOST.UI.Themes;

namespace CONCOST.UI.RevitLike;

/// <summary>
/// Every action the shell exposes. The ribbon, the quick access strip, the backstage, the command
/// line and the keyboard all point at these instances, so a command that cannot run greys out
/// everywhere at once instead of only where it was disabled by hand.
/// </summary>
public partial class MainWindow
{
    private ProjectTreeNode? _plansNode;

    // ------------------------------------------------------------------ file

    public ShellCommand NewCommand { get; private set; } = null!;

    public ShellCommand OpenCommand { get; private set; } = null!;

    public ShellCommand SaveCommand { get; private set; } = null!;

    public ShellCommand SaveAsCommand { get; private set; } = null!;

    public ShellCommand ExportCommand { get; private set; } = null!;

    public ShellCommand PrintCommand { get; private set; } = null!;

    public ShellCommand ExitCommand { get; private set; } = null!;

    // ------------------------------------------------------------------ modelling

    public ShellCommand WallCommand { get; private set; } = null!;

    public ShellCommand StructuralWallCommand { get; private set; } = null!;

    public ShellCommand ModifyCommand { get; private set; } = null!;

    public ShellCommand DeleteCommand { get; private set; } = null!;

    public ShellCommand SelectAllCommand { get; private set; } = null!;

    public ShellCommand DeselectAllCommand { get; private set; } = null!;

    public ShellCommand InvertSelectionCommand { get; private set; } = null!;

    public ShellCommand ToggleRoomTagsCommand { get; private set; } = null!;

    public ShellCommand ToggleDimensionsCommand { get; private set; } = null!;

    public ShellCommand RoomReportCommand { get; private set; } = null!;

    public ShellCommand PropertiesCommand { get; private set; } = null!;

    public ShellCommand AddLevelCommand { get; private set; } = null!;

    // ------------------------------------------------------------------ annotation

    public ShellCommand LinearDimensionCommand { get; private set; } = null!;

    public ShellCommand ClearAnnotationsCommand { get; private set; } = null!;

    // ------------------------------------------------------------------ history

    public ShellCommand UndoCommand { get; private set; } = null!;

    public ShellCommand RedoCommand { get; private set; } = null!;

    // ------------------------------------------------------------------ view

    public ShellCommand ShadedCommand { get; private set; } = null!;

    public ShellCommand HiddenLineCommand { get; private set; } = null!;

    public ShellCommand WireframeCommand { get; private set; } = null!;

    public ShellCommand TileVerticalCommand { get; private set; } = null!;

    public ShellCommand TileSingleCommand { get; private set; } = null!;

    public ShellCommand CloseAllViewsCommand { get; private set; } = null!;

    public ShellCommand ZoomExtentsCommand { get; private set; } = null!;

    public ShellCommand ZoomInCommand { get; private set; } = null!;

    public ShellCommand ZoomOutCommand { get; private set; } = null!;

    public ShellCommand OpenLevel1Command { get; private set; } = null!;

    public ShellCommand OpenLevel2Command { get; private set; } = null!;

    public ShellCommand Open3DViewCommand { get; private set; } = null!;

    // ------------------------------------------------------------------ manage

    public ShellCommand ProjectInfoCommand { get; private set; } = null!;

    public ShellCommand AboutCommand { get; private set; } = null!;

    public ShellCommand ToggleThemeCommand { get; private set; } = null!;

    public ShellCommand ToggleCommandLineCommand { get; private set; } = null!;

    private UndoRedoManager Undo => UndoRedoManager.Current;

    private void BuildCommandSet()
    {
        NewCommand = new ShellCommand(NewProject);
        OpenCommand = new ShellCommand(() => FilePage("Open", "The sample keeps its project in memory, so there is no file to load."));
        SaveCommand = new ShellCommand(SaveProject);
        SaveAsCommand = new ShellCommand(() => FilePage("Save As", "Saving under a new name needs a file store; the sample only clears the modified marker."));
        ExportCommand = new ShellCommand(() => FilePage("Export", "Export formats are out of scope here — the ribbon item exists to show the backstage layout."));
        PrintCommand = new ShellCommand(() => FilePage("Print", "Printing needs a plot engine; the backstage page is here for the shape of the workflow."));
        ExitCommand = new ShellCommand(Close);

        WallCommand = new ShellCommand(() => StartTool(ToolMode.Wall, WallPreset.Partition));
        StructuralWallCommand = new ShellCommand(() => StartTool(ToolMode.Wall, WallPreset.Structural));
        ModifyCommand = new ShellCommand(CancelTool);
        DeleteCommand = new ShellCommand(DeleteSelection, () => _selection.Count > 0);
        SelectAllCommand = new ShellCommand(SelectAll);
        DeselectAllCommand = new ShellCommand(() => _selection.Clear());
        InvertSelectionCommand = new ShellCommand(InvertSelection);
        ToggleRoomTagsCommand = new ShellCommand(ToggleRoomTags);
        ToggleDimensionsCommand = new ShellCommand(ToggleDimensions);
        RoomReportCommand = new ShellCommand(WriteRoomReport);
        PropertiesCommand = new ShellCommand(ShowProperties);
        AddLevelCommand = new ShellCommand(AddLevel);

        LinearDimensionCommand = new ShellCommand(() => StartTool(ToolMode.Dimension, WallPreset.Partition));
        ClearAnnotationsCommand = new ShellCommand(ClearAnnotations);

        UndoCommand = new ShellCommand(UndoLast, () => Undo.CanUndo);
        RedoCommand = new ShellCommand(RedoLast, () => Undo.CanRedo);

        ShadedCommand = new ShellCommand(() => ApplyStyle(VisualStyleMode.Shaded));
        HiddenLineCommand = new ShellCommand(() => ApplyStyle(VisualStyleMode.HiddenLine));
        WireframeCommand = new ShellCommand(() => ApplyStyle(VisualStyleMode.Wireframe));

        TileVerticalCommand = new ShellCommand(() => SetTileMode(DocumentTileMode.Vertical));
        TileSingleCommand = new ShellCommand(() => SetTileMode(DocumentTileMode.Single));
        CloseAllViewsCommand = new ShellCommand(CloseAllViews);

        ZoomExtentsCommand = new ShellCommand(() => _active?.ZoomExtents());
        ZoomInCommand = new ShellCommand(() => _active?.Viewport.Camera.Zoom(1 / 1.25));
        ZoomOutCommand = new ShellCommand(() => _active?.Viewport.Camera.Zoom(1.25));

        OpenLevel1Command = new ShellCommand(() => OpenView("Level 1"));
        OpenLevel2Command = new ShellCommand(() => OpenView("Level 2"));
        Open3DViewCommand = new ShellCommand(() => OpenView("{3D}"));

        ProjectInfoCommand = new ShellCommand(() =>
        {
            _backstage.Open();
            _backstage.Select("Info");
        });

        AboutCommand = new ShellCommand(WriteAbout);
        ToggleThemeCommand = new ShellCommand(ToggleTheme);
        ToggleCommandLineCommand = new ShellCommand(ToggleCommandLine);
    }

    /// <summary>Pushes the current state into every command's CanExecute, without a routing round trip.</summary>
    private void RefreshCommandStates()
    {
        UndoCommand.Refresh();
        RedoCommand.Refresh();
        DeleteCommand.Refresh();
    }

    // ------------------------------------------------------------------ file actions

    private void NewProject()
    {
        _backstage.Close();
        CloseAllViews();

        Undo.Clear();
        _selection.Clear();
        MarkModified(false);

        OpenView("Level 1");

        Notify("New project", "The session was reset; the demonstration model is fixed.");
        RefreshCommandStates();
    }

    private void SaveProject()
    {
        _backstage.Close();
        MarkModified(false);

        Notify("Save", $"\"{_model.ProjectName}\" saved in memory.", CadNotificationSeverity.Success);
    }

    private void FilePage(string page, string message)
    {
        _backstage.Close();
        _commandLine.WriteLine($"{page}: {message}", CommandMessageKind.Hint);
    }

    // ------------------------------------------------------------------ modelling actions

    private void StartTool(ToolMode tool, WallPreset preset)
    {
        _tool = tool;
        _preset = preset;
        _startPoint = null;

        if (tool == ToolMode.Wall)
        {
            _commandLine.WriteLine($"Wall tool active ({preset.Name}, {preset.Thickness:0} mm). Pick the start point.", CommandMessageKind.Hint);
        }
        else
        {
            _commandLine.WriteLine("Dimension tool active. Pick two points.", CommandMessageKind.Hint);
        }
    }

    private void DeleteSelection()
    {
        var doomed = _selection.Items.OfType<WallElement>().ToList();

        if (doomed.Count == 0) return;

        _transactions.Run("Delete", transaction =>
        {
            foreach (WallElement wall in doomed)
            {
                LevelModel? level = LevelOf(wall);

                if (level is null) continue;

                // Captured per iteration: the inverse has to put the wall back where it was, not at
                // the end of whatever the list looks like at undo time.
                int index = level.Walls.IndexOf(wall);
                LevelModel owner = level;

                transaction.Record(
                    $"Delete {wall.Name}",
                    () => owner.Walls.Remove(wall),
                    () => owner.Walls.Insert(Math.Min(index, owner.Walls.Count), wall));
            }
        });

        _selection.Clear();
        OnModelChanged();

        _commandLine.WriteLine($"{doomed.Count} element(s) deleted.", CommandMessageKind.Output);
    }

    private LevelModel? LevelOf(WallElement wall)
    {
        return _model.Levels.FirstOrDefault(level => level.Walls.Contains(wall));
    }

    private IEnumerable<object> ElementsInView(ViewPane? pane)
    {
        if (pane?.Level is { } level) return level.Walls.Cast<object>();

        return _model.Levels.SelectMany(level => level.Walls.Cast<object>());
    }

    private void SelectAll()
    {
        _selection.SelectMany(ElementsInView(_active));

        _commandLine.WriteLine($"{_selection.Count} element(s) selected.", CommandMessageKind.Output);
    }

    private void InvertSelection()
    {
        var current = new HashSet<object>(_selection.Items);
        var inverted = ElementsInView(_active).Where(element => !current.Contains(element)).ToList();

        _selection.SelectMany(inverted);

        _commandLine.WriteLine($"Selection inverted: {_selection.Count} element(s).", CommandMessageKind.Output);
    }

    private void ToggleDimensions()
    {
        _showDimensions = !_showDimensions;

        foreach (ViewPane pane in _panes)
        {
            pane.Renderer.ShowDimensions = _showDimensions && !pane.IsThreeD;
            pane.Redraw();
        }

        _commandLine.WriteLine(_showDimensions ? "Dimensions on." : "Dimensions off.", CommandMessageKind.Output);
    }

    private void ToggleRoomTags()
    {
        _showRoomTags = !_showRoomTags;

        foreach (ViewPane pane in _panes)
        {
            pane.Renderer.ShowRoomTags = _showRoomTags && !pane.IsThreeD;
            pane.Redraw();
        }

        _commandLine.WriteLine(_showRoomTags ? "Room tags on." : "Room tags off.", CommandMessageKind.Output);
    }

    private void ShowProperties()
    {
        if (_selection.Count == 0)
        {
            Notify("Properties", "Select an element first; the palette follows the selection.");
            return;
        }

        _palette.Build(_selection.Primary);
    }

    private void AddLevel()
    {
        double top = _model.Levels.Max(level => level.Elevation);
        int number = _model.Levels.Count + 1;

        var level = new LevelModel($"Level {number}", top + 3600);

        // A new storey starts as a shell: the same footprint, so the model keeps its outline.
        LevelModel template = _model.Levels[0];

        foreach (WallElement wall in template.Walls.Where(candidate => candidate.Thickness > 250))
        {
            level.AddWall(wall.Name, wall.Start.X, wall.Start.Y, wall.End.X, wall.End.Y, wall.Thickness, wall.Material);
        }

        _transactions.Run("Add level", transaction =>
            transaction.Record(
                $"Add {level.Name}",
                () => _model.Levels.Add(level),
                () => _model.Levels.Remove(level)));

        _plansNode?.Add(level.Name, ProjectNodeKind.View);
        _browser.Refresh();

        OnModelChanged();

        Notify($"{level.Name} added", $"Elevation {level.Elevation:0} mm, {level.Walls.Count} wall(s).");
    }

    // ------------------------------------------------------------------ annotation

    private void ClearAnnotations()
    {
        foreach (ViewPane pane in _panes)
        {
            pane.Viewport.TemporaryDimension = null;
            pane.Redraw();
        }

        _commandLine.WriteLine("Annotations cleared.", CommandMessageKind.Output);
    }

    // ------------------------------------------------------------------ history

    private void UndoLast()
    {
        if (!Undo.CanUndo) return;

        Undo.Undo();
        AfterHistoryChange("Undo");
    }

    private void RedoLast()
    {
        if (!Undo.CanRedo) return;

        Undo.Redo();
        AfterHistoryChange("Redo");
    }

    private void AfterHistoryChange(string label)
    {
        // The palette keeps a snapshot of the values it rendered, so it has to be told the object
        // moved underneath it.
        _palette.Refresh();

        // An undone placement can take the selected wall away with it.
        if (_selection.Primary is WallElement wall && !_model.Levels.Any(level => level.Walls.Contains(wall)))
        {
            _selection.Clear();
        }

        foreach (ViewPane pane in _panes) pane.Redraw();

        RefreshCommandStates();
        MarkModified();

        _commandLine.WriteLine(label, CommandMessageKind.Hint);
    }

    // ------------------------------------------------------------------ view actions

    private void ApplyStyle(VisualStyleMode style)
    {
        _style = style;

        foreach (ViewPane pane in _panes)
        {
            pane.Renderer.Style = style;
            pane.Controls.VisualStyle = style;
            pane.Redraw();
        }

        _commandLine.WriteLine($"Visual style: {style}", CommandMessageKind.Output);
    }

    private void SetTileMode(DocumentTileMode mode)
    {
        _workspace.TileMode = mode;

        if (mode == DocumentTileMode.Vertical && _workspace.Documents.Count < 2) OpenView("Level 2");
    }

    private void CloseAllViews()
    {
        _workspace.CloseAll();

        _panes.Clear();
        _active = null;

        _status.SelectionCount = 0;
    }

    private void ToggleTheme()
    {
        AppTheme next = ThemeManager.Current.Theme == AppTheme.Dark ? AppTheme.Light : AppTheme.Dark;

        ThemeManager.Current.ApplyTheme(next);

        _commandLine.WriteLine($"Theme: {next}", CommandMessageKind.Output);
    }

    private void ToggleCommandLine()
    {
        _commandLineVisible = !_commandLineVisible;

        CommandHost.Visibility = _commandLineVisible ? Visibility.Visible : Visibility.Collapsed;

        if (_commandLineVisible) _commandLine.FocusInput();
    }

    // ------------------------------------------------------------------ command line

    private void RegisterCommands()
    {
        Add("WALL", "Place a straight wall", WallCommand, "WA", "LINE");
        Add("SE", "Structural wall (400 mm)", StructuralWallCommand, "SW");
        Add("SELECT", "Return to selection mode", ModifyCommand, "S", "MD");
        Add("DELETE", "Delete the selected elements", DeleteCommand, "DE", "ERASE", "E");
        Add("UNDO", "Undo the last change", UndoCommand, "U");
        Add("REDO", "Redo the last undone change", RedoCommand);
        Add("ZOOM", "Zoom to extents (ZOOM I / ZOOM O to scale)", ZoomExtentsCommand, "Z", "ZE");
        Add("DIM", "Measure between two points", LinearDimensionCommand, "DI");
        Add("ANNOTATIONCLEAR", "Remove the temporary dimension", ClearAnnotationsCommand, "CX", "CL");
        Add("SELECTALL", "Select every element in the view", SelectAllCommand, "SA");
        Add("INVERT", "Invert the current selection", InvertSelectionCommand, "IS");
        Add("ROOMTAGS", "Show or hide the room tags", ToggleRoomTagsCommand, "RT");
        Add("DIMTAGS", "Show or hide the dimension strings", ToggleDimensionsCommand, "DT");
        Add("ROOMS", "List the rooms and their areas", RoomReportCommand, "RR", "REPORT");
        Add("LEVEL", "Open a storey: LEVEL 2", new ShellCommand(() =>
            _commandLine.WriteLine("Usage: LEVEL <storey>, for example LEVEL 2.", CommandMessageKind.Hint)), "LV");
        Add("3D", "Open the three-dimensional view", Open3DViewCommand, "F3");
        Add("NEWLEVEL", "Add the next storey above the highest one", AddLevelCommand, "LL");
        Add("SHADED", "Shaded visual style", ShadedCommand, "SD");
        Add("HIDDEN", "Hidden line visual style", HiddenLineCommand, "HL");
        Add("WIREFRAME", "Wireframe visual style", WireframeCommand, "WF");
        Add("TILE", "Tile the open views vertically", TileVerticalCommand, "WT");
        Add("PROPERTIES", "Focus the properties palette", PropertiesCommand, "PP", "PR");
        Add("THEME", "Switch between the light and dark theme", ToggleThemeCommand, "TH");
        Add("SAVE", "Save the project", SaveCommand, "QSAVE");
        Add("HELP", "List the available commands", new ShellCommand(WriteHelp), "?");
        Add("LIST", "List the walls on the current storey", new ShellCommand(WriteWallList), "LS");
    }

    private void Add(string name, string description, ICommand command, params string[] aliases)
    {
        var definition = new CommandDefinition(name, description) { Tag = command };

        if (aliases.Length > 0) definition.Aliases.AddRange(aliases);

        _commandLine.AddCommand(definition);
    }

    private void OnCommandInvoked(object? sender, CommandInvokedEventArgs e)
    {
        // LEVEL is the only command that needs its argument, so it resolves here rather than in the
        // table above.
        if (e.Definition.Name == "LEVEL" && e.Arguments.Count > 0)
        {
            string requested = string.Join(" ", e.Arguments);
            string title = int.TryParse(requested, out int number) ? $"Level {number}" : requested;

            OpenView(title);
            return;
        }

        if (e.Definition.Tag is ICommand command && command.CanExecute(null)) command.Execute(null);
    }

    private void WriteHelp()
    {
        _commandLine.WriteLine("CONCOST Revit-Like — command reference", CommandMessageKind.Hint);

        foreach (CommandDefinition definition in _commandLine.Commands.OrderBy(item => item.Name))
        {
            string aliases = definition.Aliases.Count == 0 ? string.Empty : $"  ({string.Join(", ", definition.Aliases)})";

            _commandLine.WriteLine($"  {definition.Name,-16}{definition.Description}{aliases}");
        }
    }

    private void WriteWallList()
    {
        IEnumerable<LevelModel> levels = _active?.Level is { } level ? new[] { level } : _model.Levels;

        foreach (LevelModel step in levels)
        {
            _commandLine.WriteLine($"{step.Name} — {step.Walls.Count} wall(s)", CommandMessageKind.Hint);

            foreach (WallElement wall in step.Walls)
            {
                _commandLine.WriteLine($"  {wall.Id}  {wall.Name,-24}{wall.Length,8:0} mm   {wall.Thickness:0} mm   {wall.Material}");
            }
        }
    }

    private void WriteAbout()
    {
        Notify(
            "CONCOST Revit-Like",
            "A reference shell built from the CONCOST.UI CAD controls: ribbon, backstage, project browser, tabbed viewports, properties palette, command line and status bar.");
    }

    private void WriteWelcome()
    {
        _commandLine.WriteLine("CONCOST Revit-Like — Riverside Tower", CommandMessageKind.Output);
        _commandLine.WriteLine("Type WALL to draw, or click a wall to edit its parameters. HELP lists every command.", CommandMessageKind.Hint);

        _status.Notify(new CadNotification("Ready", "Level 1 is open.", CadNotificationSeverity.Success));
    }

    // ------------------------------------------------------------------ keyboard

    private void OnWindowKeyDown(object sender, KeyEventArgs e)
    {
        // Typing in the command line or a palette editor must reach the editor, not the shortcuts.
        if (Keyboard.FocusedElement is TextBoxBase && e.Key != Key.Escape) return;

        bool control = (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control;

        if (control)
        {
            switch (e.Key)
            {
                case Key.Z when (Keyboard.Modifiers & ModifierKeys.Shift) == 0:
                    UndoLast();
                    e.Handled = true;
                    return;

                case Key.Y:
                case Key.Z:
                    RedoLast();
                    e.Handled = true;
                    return;

                case Key.A:
                    SelectAll();
                    e.Handled = true;
                    return;

                case Key.S:
                    SaveProject();
                    e.Handled = true;
                    return;
            }
        }

        switch (e.Key)
        {
            case Key.Escape:
                CancelTool();
                e.Handled = true;
                break;

            case Key.Delete:
                DeleteSelection();
                e.Handled = true;
                break;

            case Key.F3:
                OpenView("{3D}");
                e.Handled = true;
                break;
        }
    }
}
