using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using CONCOST.UI.Controls;
using CONCOST.UI.Controls.Ribbon;
using CONCOST.UI.Icons;
using CONCOST.UI.Services;
using Xunit;

namespace CONCOST.UI.Tests;

/// <summary>
/// The shell pieces: the project browser, the document workspace, the command line, the status bar,
/// the title bar, the info bar, the backstage and the font picker.
/// </summary>
public class CadShellTests
{
    private static void RunOnSTA(Action action) => WpfApplicationHost.Run(action);

    // ---------------------------------------------------------------- project browser

    private static ProjectBrowser CreateBrowser(out ProjectTreeNode level, out ProjectTreeNode sheet)
    {
        var browser = new ProjectBrowser();
        var project = browser.AddRoot("Project1", ProjectNodeKind.Project);
        var views = project.Add("Views", ProjectNodeKind.Folder);

        level = views.Add("Level 1", ProjectNodeKind.View);
        sheet = project.Add("A101", ProjectNodeKind.Sheet);

        browser.Refresh();

        return browser;
    }

    [Fact]
    public void ProjectBrowser_FilterKeepsThePathToAMatch()
    {
        RunOnSTA(() =>
        {
            var browser = CreateBrowser(out var level, out var sheet);

            browser.SetSearchText("Level 1");

            var visible = browser.VisibleNodes;

            Assert.Contains(visible, node => ReferenceEquals(node, level));
            Assert.DoesNotContain(visible, node => ReferenceEquals(node, sheet));
            Assert.Contains(visible, node => node.Kind == ProjectNodeKind.Project);
        });
    }

    [Fact]
    public void ProjectBrowser_ClearingTheFilterBringsEverythingBack()
    {
        RunOnSTA(() =>
        {
            var browser = CreateBrowser(out var level, out var sheet);

            browser.SetSearchText("zzz");

            Assert.DoesNotContain(browser.VisibleNodes, node => ReferenceEquals(node, level));

            browser.SetSearchText(string.Empty);

            Assert.Contains(browser.VisibleNodes, node => ReferenceEquals(node, level));
            Assert.Contains(browser.VisibleNodes, node => ReferenceEquals(node, sheet));
        });
    }

    [Fact]
    public void ProjectBrowser_GroupingPutsKindHeadersAtTheRoot()
    {
        RunOnSTA(() =>
        {
            var browser = CreateBrowser(out var level, out _);

            browser.GroupMode = ProjectGroupMode.ByKind;

            Assert.Contains(browser.VisibleNodes, node => ReferenceEquals(node, level));
            Assert.Equal(ProjectGroupMode.ByKind, browser.GroupMode);
        });
    }

    [Fact]
    public void ProjectBrowser_SelectionReportsTheNode()
    {
        RunOnSTA(() =>
        {
            var browser = CreateBrowser(out var level, out _);

            ProjectTreeNode? opened = null;

            browser.OpenRequested += (_, e) => opened = e.Node;

            browser.Select(level);

            Assert.Same(level, browser.SelectedNode);

            browser.Open(level);

            Assert.Same(level, opened);
        });
    }

    // ---------------------------------------------------------------- document workspace

    [Fact]
    public void DocumentWorkspace_AddsTabsAndActivatesTheNewest()
    {
        RunOnSTA(() =>
        {
            var workspace = new DocumentWorkspace();

            Assert.Equal(1, workspace.PaneCount);

            var first = workspace.AddDocument("Level 1", new TextBlock { Text = "plan" });
            var second = workspace.AddDocument("A101", new TextBlock { Text = "sheet" });

            Assert.Equal(2, workspace.Documents.Count);
            Assert.Same(second, workspace.ActiveDocument);

            workspace.ActiveDocument = first;

            Assert.Same(first, workspace.ActiveDocument);
        });
    }

    [Fact]
    public void DocumentWorkspace_CloseCanBeVetoedAndRestoresThePlaceholder()
    {
        RunOnSTA(() =>
        {
            var workspace = new DocumentWorkspace();
            var pane = workspace.AddDocument("Level 1", new TextBlock());

            workspace.DocumentClosing += (_, e) => e.Cancel = e.Pane.IsDirty;

            pane.IsDirty = true;

            Assert.False(workspace.CloseDocument(pane));
            Assert.Single(workspace.Documents);

            pane.IsDirty = false;

            Assert.True(workspace.CloseDocument(pane));
            Assert.Empty(workspace.Documents);
            Assert.Null(workspace.ActiveDocument);
            Assert.Equal(1, workspace.PaneCount);
        });
    }

    [Fact]
    public void DocumentWorkspace_Tiling_ShowsEveryPane()
    {
        RunOnSTA(() =>
        {
            var workspace = new DocumentWorkspace();

            workspace.AddDocument("Level 1", new TextBlock());
            workspace.AddDocument("Level 2", new TextBlock());

            workspace.TileMode = DocumentTileMode.Vertical;

            // Two panes and the splitter between them.
            Assert.Equal(3, workspace.PaneCount);

            workspace.TileMode = DocumentTileMode.Grid;

            Assert.Equal(1, workspace.PaneCount);
        });
    }

    [Fact]
    public void DocumentWorkspace_ClosingOthersKeepsTheOneAsked()
    {
        RunOnSTA(() =>
        {
            var workspace = new DocumentWorkspace();

            var keep = workspace.AddDocument("Keep", new TextBlock());

            workspace.AddDocument("Drop 1", new TextBlock());
            workspace.AddDocument("Drop 2", new TextBlock());

            workspace.CloseOthers(keep);

            Assert.Single(workspace.Documents);
            Assert.Same(keep, workspace.ActiveDocument);
        });
    }

    // ---------------------------------------------------------------- command line

    private static CommandLine CreateCommandLine(out CommandDefinition line)
    {
        var commandLine = new CommandLine();

        line = new CommandDefinition("LINE", "Draw a line")
        {
            Prompt = "Specify first point"
        };

        line.WithAliases("L", "LI");

        commandLine.AddCommand(line);
        commandLine.AddCommand(new CommandDefinition("ZOOM", "Zoom the view").WithAliases("Z"));

        return commandLine;
    }

    [Fact]
    public void CommandLine_ResolvesNamesAndAliases()
    {
        RunOnSTA(() =>
        {
            var commandLine = CreateCommandLine(out var line);

            Assert.Same(line, commandLine.Resolve("LINE"));
            Assert.Same(line, commandLine.Resolve("l"));
            Assert.Same(line, commandLine.Resolve("li 10,10"));
            Assert.Null(commandLine.Resolve("nope"));
        });
    }

    [Fact]
    public void CommandLine_SubmittingACommand_RaisesItWithArguments()
    {
        RunOnSTA(() =>
        {
            var commandLine = CreateCommandLine(out var line);

            CommandInvokedEventArgs? invoked = null;

            commandLine.CommandInvoked += (_, e) => invoked = e;

            commandLine.Text = "ZOOM 2";

            commandLine.Submit();

            Assert.NotNull(invoked);
            Assert.Equal("ZOOM", invoked!.Definition.Name);
            Assert.Equal(new[] { "2" }, invoked.Arguments);
            Assert.Equal(string.Empty, commandLine.Text);
            Assert.Contains(commandLine.Messages, message => message.Text == "> ZOOM 2");
            Assert.Equal("ZOOM", commandLine.LastCommand?.Name);
        });
    }

    [Fact]
    public void CommandLine_CommandWithAPrompt_WaitsForTheAnswer()
    {
        RunOnSTA(() =>
        {
            var commandLine = CreateCommandLine(out _);

            CommandPromptEventArgs? answered = null;

            commandLine.PromptCompleted += (_, e) => answered = e;

            commandLine.Text = "L";
            commandLine.Submit();

            Assert.True(commandLine.IsPromptActive);
            Assert.Equal("Specify first point", commandLine.Prompt);

            commandLine.Text = "10,20";
            commandLine.Submit();

            Assert.False(commandLine.IsPromptActive);
            Assert.NotNull(answered);
            Assert.Equal("10,20", answered!.Text);
            Assert.Equal("Specify first point", answered.Prompt);
        });
    }

    [Fact]
    public void CommandLine_EnterOnAnEmptyLine_RepeatsTheLastCommand()
    {
        RunOnSTA(() =>
        {
            var commandLine = CreateCommandLine(out _);
            int invoked = 0;

            commandLine.CommandInvoked += (_, _) => invoked++;

            commandLine.Text = "ZOOM";
            commandLine.Submit();
            commandLine.Submit();

            Assert.Equal(2, invoked);
        });
    }

    [Fact]
    public void CommandLine_UnknownCommand_IsReported()
    {
        RunOnSTA(() =>
        {
            var commandLine = CreateCommandLine(out _);
            string? unknown = null;

            commandLine.UnknownCommand += (_, text) => unknown = text;

            commandLine.Text = "wibble";
            commandLine.Submit();

            Assert.Equal("wibble", unknown);
            Assert.Contains(commandLine.Messages, message => message.Kind == CommandMessageKind.Error);
        });
    }

    [Fact]
    public void CommandLine_CancelPrompt_LeavesPromptMode()
    {
        RunOnSTA(() =>
        {
            var commandLine = CreateCommandLine(out _);
            bool cancelled = false;

            commandLine.PromptCancelled += (_, _) => cancelled = true;

            commandLine.Text = "L";
            commandLine.Submit();

            commandLine.CancelPrompt();

            Assert.False(commandLine.IsPromptActive);
            Assert.True(cancelled);
            Assert.Equal("Command", commandLine.Prompt);
        });
    }

    [Fact]
    public void CadInputParser_ReadsLengthsWithUnits()
    {
        Assert.True(CadInputParser.TryParseLength("1000", out double millimetres));
        Assert.Equal(1000, millimetres, 6);

        Assert.True(CadInputParser.TryParseLength("2m", out double metres));
        Assert.Equal(2000, metres, 6);

        Assert.True(CadInputParser.TryParseLength("3'-6\"", out double feetInches));
        Assert.Equal(3 * 304.8 + 6 * 25.4, feetInches, 6);

        Assert.False(CadInputParser.TryParseLength("long", out _));
    }

    [Fact]
    public void CadInputParser_ReadsCoordinatesAndAngles()
    {
        Assert.True(CadInputParser.TryParseCoordinate("1000, 2000", out var flat));
        Assert.Equal(1000, flat.X, 6);
        Assert.Equal(2000, flat.Y, 6);
        Assert.Equal(0, flat.Z, 6);

        Assert.True(CadInputParser.TryParseCoordinate("1,2,3", out var solid));
        Assert.Equal(3, solid.Z, 6);

        Assert.False(CadInputParser.TryParseCoordinate("1000", out _));

        Assert.True(CadInputParser.TryParseAngle("45deg", out double degrees));
        Assert.Equal(45, degrees, 6);
    }

    // ---------------------------------------------------------------- status bar

    [Fact]
    public void CadStatusBar_TogglesRoundTripThroughTheProperties()
    {
        RunOnSTA(() =>
        {
            var bar = new CadStatusBar();
            var changed = new System.Collections.Generic.List<CadToggle>();

            bar.ToggleChanged += (_, e) => changed.Add(e.Toggle);

            bar.SetToggle(CadToggle.Ortho, true);

            Assert.True(bar.OrthoEnabled);
            Assert.Contains(CadToggle.Ortho, changed);
            Assert.True(bar.IsToggleEnabled(CadToggle.Ortho));
        });
    }

    [Fact]
    public void CadStatusBar_FormatsTheReadout()
    {
        Assert.Equal("Nothing selected", CadStatusBar.SelectionSummary(0));
        Assert.Equal("1 object selected", CadStatusBar.SelectionSummary(1));
        Assert.Contains("3", CadStatusBar.SelectionSummary(3));

        string readout = CadStatusBar.FormatCoordinates(new System.Windows.Media.Media3D.Point3D(1000, 2000, 0), "mm");

        Assert.Contains("X", readout);
        Assert.Contains("mm", readout);
    }

    [Fact]
    public void CadStatusBar_QueuesTasksAndNotifications()
    {
        RunOnSTA(() =>
        {
            var bar = new CadStatusBar();

            bar.AddTask(new CadTask("Regenerating", 25));

            Assert.Single(bar.Tasks);

            bar.Notify(new CadNotification("Saved", "Project1.rvt", CadNotificationSeverity.Success));

            Assert.Single(bar.Notifications);
            Assert.False(bar.Notifications[0].IsRead);
        });
    }

    // ---------------------------------------------------------------- title bar, info bar, backstage, fonts

    [Fact]
    public void ApplicationTitleBar_ComposesTheTitle()
    {
        Assert.Equal("Level 1* \u2014 CONCOST", ApplicationTitleBar.ComposeTitle("Level 1", true, "CONCOST"));
        Assert.Equal("Level 1", ApplicationTitleBar.ComposeTitle("Level 1", false, null));
        Assert.Equal("CONCOST", ApplicationTitleBar.ComposeTitle(string.Empty, false, "CONCOST"));
    }

    [Fact]
    public void ApplicationInfoBar_ShowsAndHides()
    {
        RunOnSTA(() =>
        {
            var bar = new ApplicationInfoBar { Visibility = Visibility.Collapsed };

            bar.Show("Saved", "The project was saved.", CadNotificationSeverity.Success);

            Assert.Equal(Visibility.Visible, bar.Visibility);
            Assert.Equal("Saved", bar.Title);
            Assert.Equal(CadNotificationSeverity.Success, bar.Severity);

            bar.Hide();

            Assert.Equal(Visibility.Collapsed, bar.Visibility);
        });
    }

    [Fact]
    public void RibbonBackstage_OpensOnTheFirstPageAndSelectsByName()
    {
        RunOnSTA(() =>
        {
            var backstage = new RibbonBackstageView();

            backstage.Items.Add(new BackstageItem("Info", IconKind.Info) { Content = new TextBlock { Text = "Info page" } });
            backstage.Items.Add(new BackstageItem("Save", IconKind.Save) { Content = new TextBlock { Text = "Save page" } });

            backstage.Open();

            Assert.True(backstage.IsOpen);
            Assert.Equal(Visibility.Visible, backstage.Visibility);
            Assert.Equal("Info", backstage.SelectedItem?.Header);

            Assert.True(backstage.Select("Save"));
            Assert.Equal("Save", backstage.SelectedItem!.Header);

            backstage.Close();

            Assert.False(backstage.IsOpen);
            Assert.Equal(Visibility.Collapsed, backstage.Visibility);
        });
    }

    [Fact]
    public void FontPicker_AppliesItsValues()
    {
        RunOnSTA(() =>
        {
            var picker = new FontPicker
            {
                SelectedFontSize = 18,
                IsBold = true
            };

            Assert.Equal(18, picker.SelectedFontSize);
            Assert.Equal(FontWeights.Bold, picker.EffectiveWeight);
            Assert.Equal(FontStyles.Normal, picker.EffectiveStyle);

            picker.IsItalic = true;

            Assert.Equal(FontStyles.Italic, picker.EffectiveStyle);
        });
    }
}
