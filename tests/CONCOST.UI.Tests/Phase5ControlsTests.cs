using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using CONCOST.UI.Controls;
using Xunit;

namespace CONCOST.UI.Tests;

/// <summary>
/// The second §107 batch: day scheduler, kanban board, image viewer, code editor, markdown viewer,
/// coach mark, mini map, wireframe viewer and splash overlay.
/// </summary>
public class Phase5ControlsTests
{
    private static void RunOnSTA(Action action) => WpfApplicationHost.Run(action);

    // ---------------------------------------------------------------- Scheduler

    [Fact]
    public void Scheduler_NineOClockLandsTwoHoursBelowTheTop()
    {
        RunOnSTA(() =>
        {
            var day = new DateTime(2026, 3, 2);
            var item = new SchedulerItem { Title = "Họp", Start = day.AddHours(9), End = day.AddHours(10) };
            var scheduler = new Scheduler { Date = day, StartHour = 7, EndHour = 18, HourHeight = 40, ItemsSource = new[] { item } };

            scheduler.Measure(new Size(400, 600));
            scheduler.Arrange(new Rect(0, 0, 400, 600));

            var bounds = scheduler.GetItemBounds(item);

            // 34 px header + two hours at 40 px, plus the one-pixel inset the block is drawn with.
            Assert.Equal(115, bounds.Y, 1);
        });
    }

    [Fact]
    public void Scheduler_OverlappingBookingsSplitTheColumn()
    {
        RunOnSTA(() =>
        {
            var day = new DateTime(2026, 3, 2);
            var first = new SchedulerItem { Start = day.AddHours(9), End = day.AddHours(11) };
            var second = new SchedulerItem { Start = day.AddHours(10), End = day.AddHours(12) };

            var scheduler = new Scheduler { Date = day, StartHour = 8, EndHour = 18, ItemsSource = new[] { first, second } };
            scheduler.Measure(new Size(400, 600));
            scheduler.Arrange(new Rect(0, 0, 400, 600));

            var a = scheduler.GetItemBounds(first);
            var b = scheduler.GetItemBounds(second);

            Assert.True(a.X < b.X, "The second booking should take the next lane.");
            Assert.Equal(a.Width, b.Width, 1);
            Assert.True(a.Width < 200, "Two lanes should each be narrower than the whole strip.");
        });
    }

    [Fact]
    public void Scheduler_SelectItemRaisesOnceAndClears()
    {
        var seen = new List<SchedulerItem?>();

        RunOnSTA(() =>
        {
            var scheduler = new Scheduler();
            var item = new SchedulerItem { Title = "Kiểm tra", Start = DateTime.Today.AddHours(9), End = DateTime.Today.AddHours(10) };

            scheduler.SelectionChanged += (_, e) => seen.Add(e.Item);

            scheduler.SelectItem(item);
            scheduler.SelectItem(item);
            scheduler.SelectItem(null);
        });

        // Selecting the same booking twice must not raise again; clearing does.
        Assert.Equal(new SchedulerItem?[] { null }, seen.Skip(1).ToArray());
    }

    [Fact]
    public void Scheduler_BookingOutsideTheDayHasNoBounds()
    {
        RunOnSTA(() =>
        {
            var day = new DateTime(2026, 3, 2);
            var late = new SchedulerItem { Start = day.AddHours(23), End = day.AddHours(23.5) };

            var scheduler = new Scheduler { Date = day, StartHour = 7, EndHour = 18, ItemsSource = new[] { late } };
            scheduler.Measure(new Size(400, 600));
            scheduler.Arrange(new Rect(0, 0, 400, 600));

            Assert.True(scheduler.GetItemBounds(late).IsEmpty);
        });
    }

    [Fact]
    public void Scheduler_ChangeDayMovesTheDate()
    {
        RunOnSTA(() =>
        {
            var scheduler = new Scheduler { Date = new DateTime(2026, 3, 2, 14, 30, 0) };
            scheduler.ChangeDay(1);

            Assert.Equal(new DateTime(2026, 3, 3), scheduler.Date);
        });
    }

    // ---------------------------------------------------------------- Kanban

    [Fact]
    public void Kanban_MoveCardMovesBetweenColumnsAndReportsIt()
    {
        RunOnSTA(() =>
        {
            var board = new KanbanBoard();
            var todo = new KanbanColumn { Header = "Cần làm" };
            var doing = new KanbanColumn { Header = "Đang làm" };
            var card = new KanbanCard { Title = "Mặt bằng" };

            todo.Items.Add(card);
            board.Columns.Add(todo);
            board.Columns.Add(doing);

            KanbanCardMovedEventArgs? args = null;
            board.CardMoved += (_, e) => args = e;

            Assert.True(board.MoveCard(card, doing, 0));
            Assert.Empty(todo.Items);
            Assert.Same(card, doing.Items[0]);
            Assert.Same(todo, args?.FromColumn);
            Assert.Same(doing, args?.ToColumn);
        });
    }

    [Fact]
    public void Kanban_MoveCardWithinAColumnReorders()
    {
        RunOnSTA(() =>
        {
            var board = new KanbanBoard();
            var column = new KanbanColumn();
            var first = new KanbanCard { Title = "1" };
            var second = new KanbanCard { Title = "2" };

            column.Items.Add(first);
            column.Items.Add(second);
            board.Columns.Add(column);

            Assert.True(board.MoveCard(first, column, 2));
            Assert.Same(second, column.Items[0]);
            Assert.Same(first, column.Items[1]);
        });
    }

    [Fact]
    public void Kanban_MoveCardToTheSameSpotReturnsFalse()
    {
        RunOnSTA(() =>
        {
            var board = new KanbanBoard();
            var column = new KanbanColumn();
            var card = new KanbanCard();

            column.Items.Add(card);
            board.Columns.Add(column);

            Assert.False(board.MoveCard(card, column, 0));
        });
    }

    [Fact]
    public void Kanban_ColumnOfFindsTheLane()
    {
        RunOnSTA(() =>
        {
            var board = new KanbanBoard();
            var column = new KanbanColumn();
            var card = new KanbanCard();
            column.Items.Add(card);
            board.Columns.Add(column);

            Assert.Same(column, board.ColumnOf(card));
            Assert.Null(board.ColumnOf(new KanbanCard()));
        });
    }

    // ---------------------------------------------------------------- ImageViewer

    [Fact]
    public void ImageViewer_ZoomAtKeepsTheAnchorOverTheSamePoint()
    {
        RunOnSTA(() =>
        {
            var viewer = new ImageViewer { Zoom = 1, Offset = new Point(10, 10) };
            var anchor = new Point(60, 60);

            double before = (anchor.X - viewer.Offset.X) / viewer.Zoom;
            viewer.ZoomAt(anchor, 2);
            double after = (anchor.X - viewer.Offset.X) / viewer.Zoom;

            Assert.Equal(2, viewer.Zoom, 3);
            Assert.Equal(before, after, 3);
        });
    }

    [Fact]
    public void ImageViewer_ZoomIsClampedToItsRange()
    {
        RunOnSTA(() =>
        {
            var viewer = new ImageViewer { MinZoom = 0.5, MaxZoom = 4 };

            viewer.Zoom = 99;
            Assert.Equal(4, viewer.Zoom, 3);

            viewer.Zoom = 0.01;
            Assert.Equal(0.5, viewer.Zoom, 3);
        });
    }

    [Fact]
    public void ImageViewer_RotateClockwiseWrapsAtThreeSixty()
    {
        RunOnSTA(() =>
        {
            var viewer = new ImageViewer { Rotation = 270 };
            viewer.RotateClockwise();

            Assert.Equal(0, viewer.Rotation, 3);
        });
    }

    // ---------------------------------------------------------------- CodeHighlighter

    private static string Slice(string text, CodeToken token) => text.Substring(token.Start, token.Length);

    [Fact]
    public void CodeHighlighter_MarksKeywordsStringsAndComments()
    {
        const string code = "public void Run() { // go\n  var s = \"hi\"; }";
        var tokens = CodeHighlighter.Tokenize(code, CodeLanguage.CSharp);

        Assert.Contains(tokens, t => t.Kind == CodeTokenKind.Keyword && Slice(code, t) == "public");
        Assert.Contains(tokens, t => t.Kind == CodeTokenKind.String && Slice(code, t) == "\"hi\"");
        Assert.Contains(tokens, t => t.Kind == CodeTokenKind.Comment && Slice(code, t) == "// go");
    }

    [Fact]
    public void CodeHighlighter_SqlKeywordsAreCaseInsensitive()
    {
        const string sql = "Select * From dbo.Wall";
        var tokens = CodeHighlighter.Tokenize(sql, CodeLanguage.Sql);
        var keywords = tokens.Where(t => t.Kind == CodeTokenKind.Keyword).Select(t => Slice(sql, t)).ToList();

        // The SQL keyword set is case-insensitive, so mixed-case statements still colour.
        Assert.Equal(2, keywords.Count);
        Assert.Contains("Select", keywords);
        Assert.Contains("From", keywords);
    }

    [Fact]
    public void CodeHighlighter_XamlMarksElementsAndAttributes()
    {
        const string xaml = "<Border Background=\"Red\" />";
        var tokens = CodeHighlighter.Tokenize(xaml, CodeLanguage.Xaml);

        Assert.Contains(tokens, t => t.Kind == CodeTokenKind.Tag && Slice(xaml, t) == "Border");
        Assert.Contains(tokens, t => t.Kind == CodeTokenKind.Attribute && Slice(xaml, t) == "Background");
        Assert.Contains(tokens, t => t.Kind == CodeTokenKind.String && Slice(xaml, t) == "\"Red\"");
    }

    [Fact]
    public void CodeHighlighter_UnterminatedStringStopsAtTheLineEnd()
    {
        const string code = "var a = \"oops\nvar b = 2;";
        var tokens = CodeHighlighter.Tokenize(code, CodeLanguage.CSharp);

        var literal = tokens.First(t => t.Kind == CodeTokenKind.String);

        Assert.Equal("\"oops", Slice(code, literal));
        Assert.Contains(tokens, t => t.Kind == CodeTokenKind.Number && Slice(code, t) == "2");
    }

    [Fact]
    public void CodeHighlighter_PlainLanguageReturnsNothing()
    {
        Assert.Empty(CodeHighlighter.Tokenize("public void Run()", CodeLanguage.Plain));
    }

    [Fact]
    public void CodeEditor_LineCountFollowsTheText()
    {
        RunOnSTA(() =>
        {
            var editor = new CodeEditor { Text = "a\nb\nc" };

            Assert.Equal(3, editor.LineCount);

            editor.Text = "one line";
            Assert.Equal(1, editor.LineCount);
        });
    }

    [Fact]
    public void CodeEditor_TabSizeIsClamped()
    {
        RunOnSTA(() =>
        {
            var editor = new CodeEditor { TabSize = 0 };
            Assert.Equal(1, editor.TabSize);

            editor.TabSize = 40;
            Assert.Equal(12, editor.TabSize);
        });
    }

    // ---------------------------------------------------------------- Markdown

    [Fact]
    public void Markdown_HeadingsGetBiggerType()
    {
        var document = MarkdownParser.Parse("# Tiêu đề\n\nNội dung");

        var heading = (Paragraph)document.Blocks.FirstBlock!;
        var body = (Paragraph)document.Blocks.LastBlock!;

        var headingRun = (Run)heading.Inlines.FirstInline!;
        var bodyRun = (Run)body.Inlines.FirstInline!;

        Assert.True(headingRun.FontSize > bodyRun.FontSize);
    }

    [Fact]
    public void Markdown_BoldAndInlineCodeBecomeTypedInlines()
    {
        var inlines = MarkdownParser.ParseInlines("một **đậm** và `mã`").ToList();

        Assert.Contains(inlines, i => i is Bold);
        Assert.Contains(inlines, i => i is Run run && run.Background is not null);
    }

    [Fact]
    public void Markdown_FenceBecomesOneCodeParagraph()
    {
        var document = MarkdownParser.Parse("```\nvar a = 1;\nvar b = 2;\n```");
        var paragraph = Assert.IsType<Paragraph>(document.Blocks.FirstBlock);
        var run = Assert.IsType<Run>(Assert.Single(paragraph.Inlines));

        // The fenced block is kept verbatim as one run, not split per source line.
        Assert.Contains("var a = 1;", run.Text);
        Assert.Contains("var b = 2;", run.Text);
    }

    [Fact]
    public void Markdown_BulletsBecomeAList()
    {
        var document = MarkdownParser.Parse("- một\n- hai\n- ba");
        var list = Assert.IsType<List>(document.Blocks.FirstBlock);

        Assert.Equal(3, list.ListItems.Count);
    }

    [Fact]
    public void Markdown_LinksAreCountedByTheViewer()
    {
        RunOnSTA(() =>
        {
            var viewer = new MarkdownViewer { Markdown = "Xem [tài liệu](https://example.com) và [khác](https://example.org)." };
            viewer.Rebuild();

            Assert.Equal(2, viewer.LinkCount);
        });
    }

    [Fact]
    public void Markdown_GarbageNeverThrows()
    {
        foreach (string text in new[] { "", "###", "**", "`", "[", "> ", "```", "***", "- ", "1. " })
        {
            var document = MarkdownParser.Parse(text);
            Assert.NotNull(document);
        }
    }

    // ---------------------------------------------------------------- CoachMark

    [Fact]
    public void CoachMark_PlacesTheCardBelowTheTargetWhenThereIsRoom()
    {
        RunOnSTA(() =>
        {
            var host = new Grid { Width = 400, Height = 300 };
            var target = new Border { Width = 80, Height = 30, HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top };
            host.Children.Add(target);

            var mark = new CoachMark { Target = target };
            host.Children.Add(mark);

            host.Measure(new Size(400, 300));
            host.Arrange(new Rect(0, 0, 400, 300));

            Assert.False(mark.CardRect.IsEmpty);
            Assert.True(mark.CardRect.Y > target.RenderSize.Height, "The card should sit under the target.");
        });
    }

    [Fact]
    public void CoachMark_FlipsAboveWhenTheTargetIsAtTheBottom()
    {
        RunOnSTA(() =>
        {
            var host = new Grid { Width = 400, Height = 200 };
            var target = new Border
            {
                Width = 80,
                Height = 30,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Bottom
            };

            host.Children.Add(target);

            var mark = new CoachMark { Target = target };
            host.Children.Add(mark);

            host.Measure(new Size(400, 200));
            host.Arrange(new Rect(0, 0, 400, 200));

            Assert.True(mark.CardRect.Bottom < 200);
            Assert.True(mark.CardRect.Y < 170, "The card should be flipped above a target pinned to the bottom.");
        });
    }

    [Fact]
    public void CoachMark_DismissCollapsesAndRaisesOnce()
    {
        RunOnSTA(() =>
        {
            var mark = new CoachMark();
            int raised = 0;
            mark.Dismissed += (_, _) => raised++;

            mark.Dismiss();
            mark.Dismiss();

            Assert.Equal(Visibility.Collapsed, mark.Visibility);
            Assert.Equal(1, raised);
        });
    }

    // ---------------------------------------------------------------- MiniMap

    [Fact]
    public void MiniMap_WorldAndMapCoordinatesRoundTrip()
    {
        RunOnSTA(() =>
        {
            var map = new MiniMap { ContentBounds = new Rect(0, 0, 1000, 500) };
            map.Measure(new Size(200, 100));
            map.Arrange(new Rect(0, 0, 200, 100));

            var world = new Point(250, 125);
            var back = map.MapToWorld(map.WorldToMap(world));

            Assert.Equal(world.X, back.X, 2);
            Assert.Equal(world.Y, back.Y, 2);
        });
    }

    [Fact]
    public void MiniMap_CentreOnIsClampedToTheSheet()
    {
        RunOnSTA(() =>
        {
            var map = new MiniMap
            {
                ContentBounds = new Rect(0, 0, 1000, 500),
                Viewport = new Rect(0, 0, 200, 100)
            };

            map.CentreOn(new Point(-500, -500));
            Assert.Equal(0, map.Viewport.X, 2);
            Assert.Equal(0, map.Viewport.Y, 2);

            map.CentreOn(new Point(2000, 2000));
            Assert.Equal(800, map.Viewport.X, 2);
            Assert.Equal(400, map.Viewport.Y, 2);
        });
    }

    [Fact]
    public void MiniMap_FitShowsEverything()
    {
        RunOnSTA(() =>
        {
            var content = new Rect(10, 20, 400, 300);
            var map = new MiniMap { ContentBounds = content, Viewport = new Rect(0, 0, 50, 50) };

            map.Fit();

            Assert.Equal(content, map.Viewport);
        });
    }

    // ---------------------------------------------------------------- ModelViewer3D

    [Fact]
    public void ModelViewer3D_LoadBoxCreatesEightVerticesAndTwelveEdges()
    {
        RunOnSTA(() =>
        {
            var viewer = new ModelViewer3D();
            viewer.LoadBox(new Point3D(0, 0, 0), 10, 6, 4);

            Assert.Equal(8, viewer.Vertices.Count);
            Assert.Equal(12, viewer.Edges.Count);
        });
    }

    [Fact]
    public void ModelViewer3D_OrbitClampsPitch()
    {
        RunOnSTA(() =>
        {
            var viewer = new ModelViewer3D { Pitch = 0 };
            viewer.Orbit(0, -1000);

            Assert.Equal(89, viewer.Pitch, 3);
        });
    }

    [Fact]
    public void ModelViewer3D_ProjectPutsTheCentreInTheMiddle()
    {
        RunOnSTA(() =>
        {
            var viewer = new ModelViewer3D();
            viewer.LoadBox(new Point3D(0, 0, 0), 10, 6, 4);
            viewer.Measure(new Size(200, 200));
            viewer.Arrange(new Rect(0, 0, 200, 200));

            Assert.True(viewer.Project(new Point3D(5, 3, 2), out var screen));
            Assert.Equal(100, screen.X, 1);
            Assert.Equal(100, screen.Y, 1);
        });
    }

    [Fact]
    public void ModelViewer3D_ZoomIsClamped()
    {
        RunOnSTA(() =>
        {
            var viewer = new ModelViewer3D { Zoom = 100 };
            Assert.Equal(20, viewer.Zoom, 3);

            viewer.Zoom = 0;
            Assert.Equal(0.1, viewer.Zoom, 3);
        });
    }

    // ---------------------------------------------------------------- SplashScreen

    [Fact]
    public void SplashScreen_DismissWithoutAnimationCollapsesAndRaisesOnce()
    {
        RunOnSTA(() =>
        {
            var splash = new CONCOST.UI.Controls.SplashScreen();
            int raised = 0;
            splash.Dismissed += (_, _) => raised++;

            splash.Dismiss(false);
            splash.Dismiss(false);

            Assert.Equal(Visibility.Collapsed, splash.Visibility);
            Assert.Equal(1, raised);
        });
    }

    [Fact]
    public void SplashScreen_ReportSetsProgressAndStatus()
    {
        RunOnSTA(() =>
        {
            var splash = new CONCOST.UI.Controls.SplashScreen();
            splash.Report(140, "Đang mở mô hình");

            Assert.Equal(100, splash.Progress, 3);
            Assert.Equal("Đang mở mô hình", splash.Status);
        });
    }
}
