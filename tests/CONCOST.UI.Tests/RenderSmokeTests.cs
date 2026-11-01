using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using CONCOST.UI.Controls;
using CONCOST.UI.Controls.Canvas;
using CONCOST.UI.Controls.Ribbon;
using CONCOST.UI.Core.UndoRedo;
using CONCOST.UI.Services;
using Xunit;

namespace CONCOST.UI.Tests;

/// <summary>
/// Visual regression guard (§96): render a control offscreen and assert the pixels are not blank.
/// </summary>
/// <remarks>
/// A full image-diff harness needs committed baselines and a tolerance policy, which is a project of
/// its own. What this file does catch is the class of failure that actually happens here: a template
/// that throws while being applied, a control that measures to zero, and a render pass that paints
/// nothing — all of which used to reach the showcase and only show up as an empty card.
/// </remarks>
public class RenderSmokeTests
{
    private static void RunOnSTA(Action action) => WpfApplicationHost.Run(action);

    /// <summary>Renders <paramref name="element"/> into a bitmap and returns the share of non-background pixels.</summary>
    private static double RenderedCoverage(FrameworkElement element, int width, int height, bool themed = false)
    {
        var host = new Border
        {
            Background = Brushes.White,
            Child = element,
            Width = width,
            Height = height
        };

        if (themed) MergeTheme(host);

        host.Measure(new Size(width, height));
        host.Arrange(new Rect(0, 0, width, height));
        host.UpdateLayout();

        var bitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(host);

        int stride = width * 4;
        var pixels = new byte[stride * height];
        bitmap.CopyPixels(pixels, stride, 0);

        int painted = 0;
        for (int i = 0; i < pixels.Length; i += 4)
        {
            // Anything that is not the white backdrop counts, alpha included.
            if (pixels[i] != 255 || pixels[i + 1] != 255 || pixels[i + 2] != 255) painted++;
        }

        return painted / (double)(width * height);
    }

    /// <summary>
    /// Puts the token and theme dictionaries on the host so a template's <c>DynamicResource</c>
    /// chrome brushes resolve. Without it a surface brush is null and the control is transparent, so
    /// only its text paints against the white backdrop.
    /// </summary>
    private static void MergeTheme(FrameworkElement host)
    {
        host.Resources.MergedDictionaries.Add(new ResourceDictionary
        {
            Source = new Uri("pack://application:,,,/CONCOST.UI;component/Themes/ThemeResources.xaml", UriKind.Absolute)
        });
    }

    /// <summary>
    /// Asserts the element paints at least <paramref name="minimum"/> of the surface. The floor is
    /// per control on purpose: a text box is mostly its own white background, so its border alone is
    /// well under one percent, while an overlay scrim should cover nearly everything.
    /// </summary>
    private static void AssertRendersSomething(FrameworkElement element, double minimum = 0.02,
        int width = 320, int height = 200, bool themed = false)
    {
        double coverage = RenderedCoverage(element, width, height, themed);

        Assert.True(coverage > minimum,
            $"{element.GetType().Name} painted {coverage:P2} of the surface, expected at least {minimum:P2} — " +
            "the template probably failed to apply.");
    }

    [Fact]
    public void Button_RendersContent()
    {
        RunOnSTA(() =>
        {
            var button = new Button { Content = "Xuất PDF" };
            button.SetResourceReference(FrameworkElement.StyleProperty, "CONCOST.Button.Primary");
            AssertRendersSomething(button);
        });
    }

    [Fact]
    public void TextBox_RendersItsChrome()
    {
        RunOnSTA(() =>
        {
            var box = new TextBox { Text = "CONCOST" };
            AssertRendersSomething(box, 0.002);
        });
    }

    [Fact]
    public void CheckBox_RendersTheTick()
    {
        RunOnSTA(() =>
        {
            var check = new CheckBox { Content = "Bật hiệu ứng", IsChecked = true };
            AssertRendersSomething(check, 0.02, 200, 60);
        });
    }

    [Fact]
    public void Card_RendersItsSurfaceAndBorder()
    {
        RunOnSTA(() =>
        {
            var card = new Card { Width = 240, Height = 120, Content = new TextBlock { Text = "Thẻ" } };
            AssertRendersSomething(card, 0.005, 300, 180);
        });
    }

    [Fact]
    public void RadialGauge_RendersTheArcAndTheNumber()
    {
        RunOnSTA(() =>
        {
            var gauge = new RadialGauge { Value = 76, Label = "Tiến độ", Suffix = "%" };
            AssertRendersSomething(gauge, 0.02, 200, 200);
        });
    }

    [Fact]
    public void ParticleField_RendersParticles()
    {
        RunOnSTA(() =>
        {
            var field = new ParticleField { ParticleCount = 80, ParticleOpacity = 1 };
            AssertRendersSomething(field, 0.002, 320, 200);
        });
    }

    [Fact]
    public void PageTransitionHost_RendersItsContent()
    {
        RunOnSTA(() =>
        {
            var host = new PageTransitionHost
            {
                Content = new Border
                {
                    Background = Brushes.OrangeRed,
                    Child = new TextBlock { Text = "Trang 1" }
                }
            };
            AssertRendersSomething(host, 0.05, 300, 160);
        });
    }

    [Fact]
    public void CommandPalette_Open_RendersTheOverlay()
    {
        RunOnSTA(() =>
        {
            var palette = new CommandPalette
            {
                ItemsSource = new List<CommandPaletteItem>
                {
                    new("Tổng quan dự án", "Bảng điều khiển KPI", shortcut: "Ctrl+1"),
                    new("Xuất PDF", "Xuất bản vẽ ra PDF", shortcut: "Ctrl+P")
                }
            };

            palette.Open();

            // An open palette paints its scrim, so it should cover nearly the whole surface.
            AssertRendersSomething(palette, 0.5, 700, 460);
        });
    }

    [Fact]
    public void CommandPalette_Closed_PaintsNothing()
    {
        RunOnSTA(() =>
        {
            var palette = new CommandPalette();
            double coverage = RenderedCoverage(palette, 400, 300);

            Assert.True(coverage < 0.01, $"A closed palette painted {coverage:P1} of the surface.");
        });
    }

    [Fact]
    public void SnowCanvas_RendersAfterLayout()
    {
        RunOnSTA(() =>
        {
            // The default flake brush is white, which is invisible on the white test backdrop.
            var canvas = new SnowCanvas { SnowflakeCount = 40, SnowflakeBrush = Brushes.DodgerBlue };

            // Flake positions are random, so the painted fraction wobbles run to run. The assertion
            // is only asking "did the template paint anything", so keep a wide margin rather than
            // pinning a figure that fails on an unlucky seed.
            AssertRendersSomething(canvas, 0.001, 320, 200);
        });
    }

    [Fact]
    public void TagInput_RendersItsChipsAndPlaceholder()
    {
        RunOnSTA(() =>
        {
            var input = new TagInput { Placeholder = "Thêm vật liệu", Width = 300, Height = 64 };
            input.AddTag("Bê tông");
            input.AddTag("Thép");

            // The test host has no theme merged, so the chrome brushes are transparent; what has to
            // survive is the chip captions and the placeholder.
            AssertRendersSomething(input, 0.002, 320, 200);
        });
    }

    [Fact]
    public void Wizard_RendersTheHeaderAndTheCurrentStep()
    {
        RunOnSTA(() =>
        {
            var wizard = new Wizard { Width = 420, Height = 260 };
            wizard.Steps.Add(new WizardStep
            {
                Header = "Thông tin dự án",
                Description = "Tên và địa điểm",
                Content = new TextBlock { Text = "nội dung" }
            });
            wizard.Steps.Add(new WizardStep { Header = "Nhập tiết diện", Content = new TextBlock { Text = "tiết diện" } });

            AssertRendersSomething(wizard, 0.003, 420, 300);
        });
    }

    [Fact]
    public void SignaturePad_RendersItsFrameAndGuideline()
    {
        RunOnSTA(() =>
        {
            var pad = new SignaturePad { Placeholder = "Ký và ghi rõ họ tên" };
            AssertRendersSomething(pad, 0.005, 320, 160);
        });
    }

    [Fact]
    public void Scheduler_RendersTheHourGridAndABooking()
    {
        RunOnSTA(() =>
        {
            var day = DateTime.Today;
            var scheduler = new Scheduler
            {
                Date = day,
                ItemsSource = new[]
                {
                    new SchedulerItem { Title = "Họp kết cấu", Start = day.AddHours(9), End = day.AddHours(10.5) }
                }
            };

            AssertRendersSomething(scheduler, 0.2, 360, 300);
        });
    }

    [Fact]
    public void KanbanBoard_RendersItsColumnsAndCards()
    {
        RunOnSTA(() =>
        {
            var board = new KanbanBoard();
            var column = new KanbanColumn { Header = "Đang làm" };
            column.Items.Add(new KanbanCard { Title = "Mặt bằng tầng 3", Chip = "Kết cấu" });
            board.Columns.Add(column);

            AssertRendersSomething(board, 0.5, 420, 260, themed: true);
        });
    }

    [Fact]
    public void ImageViewer_RendersItsBackdrop()
    {
        RunOnSTA(() => AssertRendersSomething(new ImageViewer(), 0.5, 320, 200));
    }

    [Fact]
    public void CodeEditor_RendersTheGutterAndTheEditor()
    {
        RunOnSTA(() =>
        {
            var editor = new CodeEditor { Text = "public void Run()\n{\n    Build();\n}" };

            // The light theme puts the editor surface on white, so the white test backdrop only
            // leaves the gutter and the glyphs to count.
            AssertRendersSomething(editor, 0.05, 360, 200, themed: true);
        });
    }

    [Fact]
    public void MarkdownViewer_RendersTheParsedDocument()
    {
        RunOnSTA(() =>
        {
            var viewer = new MarkdownViewer { Markdown = "# Tiêu đề\n\nMột đoạn **đậm** và `mã`." };
            viewer.Rebuild();

            AssertRendersSomething(viewer, 0.02, 360, 220, themed: true);
        });
    }

    [Fact]
    public void CoachMark_RendersTheScrimAndTheCard()
    {
        RunOnSTA(() =>
        {
            var host = new Grid { Width = 360, Height = 240 };
            var button = new Button { Content = "Lưu", Width = 80, Height = 30, HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top };
            host.Children.Add(button);

            var mark = new CoachMark { Target = button, Title = "Bắt đầu ở đây" };
            host.Children.Add(mark);

            host.Measure(new Size(360, 240));
            host.Arrange(new Rect(0, 0, 360, 240));
            host.UpdateLayout();

            Assert.False(mark.CardRect.IsEmpty, "The card was never placed next to its target.");
        });
    }

    [Fact]
    public void MiniMap_RendersItsBlocks()
    {
        RunOnSTA(() =>
        {
            var map = new MiniMap
            {
                ItemsSource = new[] { new MiniMapItem { Bounds = new Rect(20, 20, 300, 180) } }
            };

            AssertRendersSomething(map, 0.5, 200, 150);
        });
    }

    [Fact]
    public void ModelViewer3D_RendersABox()
    {
        RunOnSTA(() =>
        {
            var viewer = new ModelViewer3D();
            viewer.LoadBox(new Point3D(0, 0, 0), 10, 6, 4);

            AssertRendersSomething(viewer, 0.5, 300, 220);
        });
    }

    [Fact]
    public void SplashScreen_RendersTheStartUpCard()
    {
        RunOnSTA(() =>
        {
            var splash = new CONCOST.UI.Controls.SplashScreen { AppName = "CONCOST", Version = "1.2.0", Status = "Đang tải…", Progress = 40 };
            AssertRendersSomething(splash, 0.5, 480, 300, themed: true);
        });
    }

    // ------------------------------------------------------------------ the CAD shell

    [Fact]
    public void ViewCube_RendersTheIsometricBody()
    {
        // Rendered in a box the size of the widget itself: the cube shares that square with the compass
        // ring and the hover buttons, and at the resting opacity a corner of a larger canvas would be
        // measured mostly as empty viewport.
        RunOnSTA(() => AssertRendersSomething(new ViewCube { Size = ViewCubeSize.Large }, 0.1, 128, 128));
    }

    [Fact]
    public void ViewportControl_RendersTheGridAndTheBackdrop()
    {
        RunOnSTA(() =>
        {
            var viewport = new ViewportControl { Width = 360, Height = 240 };
            AssertRendersSomething(viewport, 0.2, 360, 240);
        });
    }

    [Fact]
    public void NavigationBar_RendersItsToolButtons()
    {
        RunOnSTA(() => AssertRendersSomething(new NavigationBar(), 0.02, 260, 40, themed: true));
    }

    [Fact]
    public void ViewControlBar_RendersScaleAndStylePickers()
    {
        RunOnSTA(() =>
        {
            var bar = new ViewControlBar { ShowShadows = true };
            AssertRendersSomething(bar, 0.02, 460, 40, themed: true);
        });
    }

    [Fact]
    public void CommandLine_RendersTheHistoryAndThePrompt()
    {
        RunOnSTA(() =>
        {
            var commandLine = new CommandLine { Width = 520, Height = 180 };

            commandLine.AddCommand(new CommandDefinition("LINE", "Draw a line"));
            commandLine.WriteLine("Command: LINE");

            // The line paints no surface of its own — the host docks it on the window's backdrop —
            // so only the prompt and the history rows count.
            AssertRendersSomething(commandLine, 0.008, 520, 180, themed: true);
        });
    }

    [Fact]
    public void CadStatusBar_RendersTheTogglesAndTheCoordinates()
    {
        RunOnSTA(() =>
        {
            var bar = new CadStatusBar { Coordinates = new Point3D(1250, 800, 0), SelectionCount = 2 };

            // The bar draws its own plate now, so the host has to be tall enough to show it.
            AssertRendersSomething(bar, 0.02, 640, 36, themed: true);
        });
    }

    [Fact]
    public void ProjectBrowser_RendersTheTree()
    {
        RunOnSTA(() =>
        {
            var browser = new ProjectBrowser { Width = 300, Height = 260 };
            var project = browser.AddRoot("Project1", ProjectNodeKind.Project);

            project.Add("Level 1", ProjectNodeKind.View);
            project.Add("A101", ProjectNodeKind.Sheet);

            browser.ExpandAll();

            AssertRendersSomething(browser, 0.02, 300, 260, themed: true);
        });
    }

    [Fact]
    public void DocumentWorkspace_RendersTheTabStripAndThePlaceholder()
    {
        RunOnSTA(() =>
        {
            var workspace = new DocumentWorkspace { Width = 420, Height = 260 };

            workspace.AddDocument("Level 1", new TextBlock { Text = "plan" });

            // The tab strip draws chrome but the body stays transparent for the view underneath, so
            // the tab captions are the only pixels this guard can count.
            AssertRendersSomething(workspace, 0.005, 420, 260, themed: true);
        });
    }

    [Fact]
    public void PropertiesPalette_RendersTheCategoryGrid()
    {
        RunOnSTA(() =>
        {
            var manager = new PropertyManager(new TransactionManager(new UndoRedoManager()));

            manager.Register<RenderedSample>(new PropertyDescriptor("Length", target => ((RenderedSample)target).Length)
            {
                DisplayName = "Length",
                Category = "Dimensions",
                Editor = PropertyEditorKind.Numeric
            });

            var palette = new PropertiesPalette(manager, new SelectionManager()) { Width = 320, Height = 300 };
            palette.Build(new RenderedSample());

            AssertRendersSomething(palette, 0.02, 320, 300, themed: true);
        });
    }

    [Fact]
    public void ApplicationTitleBar_RendersTheCaptionAndTheQuickAccessStrip()
    {
        RunOnSTA(() =>
        {
            var bar = new ApplicationTitleBar
            {
                ApplicationName = "CONCOST",
                DocumentTitle = "Level 1",
                IsDocumentModified = true
            };

            AssertRendersSomething(bar, 0.02, 520, 32, themed: true);
        });
    }

    [Fact]
    public void ApplicationInfoBar_RendersTheMessage()
    {
        RunOnSTA(() =>
        {
            var bar = new ApplicationInfoBar();

            bar.Show("Cập nhật", "Đã có bản 1.3.");

            AssertRendersSomething(bar, 0.5, 480, 40, themed: true);
        });
    }

    [Fact]
    public void RibbonBackstageView_WhenOpenCoversTheWindow()
    {
        RunOnSTA(() =>
        {
            var backstage = new RibbonBackstageView { Width = 620, Height = 380 };

            backstage.Items.Add(new BackstageItem("Thông tin"));
            backstage.Items.Add(new BackstageItem("In"));

            backstage.Open();

            AssertRendersSomething(backstage, 0.5, 620, 380, themed: true);
        });
    }

    [Fact]
    public void FontPicker_RendersTheFamiliesAndThePreview()
    {
        RunOnSTA(() => AssertRendersSomething(new FontPicker(), 0.02, 360, 64, themed: true));
    }

    [Fact]
    public void DynamicInput_RendersTheOpenBoxBetweenThePromptAndItsField()
    {
        RunOnSTA(() =>
        {
            // A collapsed box paints nothing, so the guard has to open it the way a command does.
            // The floor is low because the light theme's surface token is pure #FFFFFF: the bitmap
            // comparison only sees the border, the callout edge and the two lines of text.
            var input = new DynamicInput { Prompt = "Length", Kind = DynamicInputKind.Distance };
            input.Open(new Point(0, 0));
            input.Text = "3200";

            AssertRendersSomething(input, 0.02, 240, 110, themed: true);
        });
    }

    private sealed class RenderedSample
    {
        public double Length { get; set; } = 4000;
    }

    [Theory]
    [InlineData("CONCOST.Button.Primary")]
    [InlineData("CONCOST.Button.Secondary")]
    [InlineData("CONCOST.Badge.Base")]
    [InlineData("CONCOST.CommandPalette.Base")]
    [InlineData("CONCOST.PageTransitionHost.Base")]
    [InlineData("CONCOST.TagInput.Base")]
    [InlineData("CONCOST.Wizard.Base")]
    [InlineData("CONCOST.KanbanBoard.Base")]
    [InlineData("CONCOST.KanbanColumn.Base")]
    [InlineData("CONCOST.KanbanCard.Base")]
    [InlineData("CONCOST.CodeEditor.Base")]
    [InlineData("CONCOST.MarkdownViewer.Base")]
    [InlineData("CONCOST.CoachMark.Base")]
    [InlineData("CONCOST.SplashScreen.Base")]
    // The three the navigation bar asks for by name: a rename would silently hand its commands
    // back to the stock outlined button.
    [InlineData("CONCOST.Button.Icon.Ghost")]
    [InlineData("CONCOST.ToggleButton.Icon.Ghost")]
    [InlineData("CONCOST.Button.NavigationLabel")]
    // The view control bar asks for these two: the same reason, one page further down.
    [InlineData("CONCOST.SplitButton.Ghost")]
    public void ThemeDictionary_ContainsTheStyleKeys(string key)
    {
        RunOnSTA(() =>
        {
            // The application dictionary only carries the theme once a host applies it, so the
            // dictionary itself is the honest place to assert on.
            var theme = new ResourceDictionary
            {
                Source = new Uri("pack://application:,,,/CONCOST.UI;component/Themes/Generic.xaml", UriKind.Absolute)
            };

            Assert.True(theme.Contains(key), $"The theme dictionary has no '{key}' style.");
        });
    }
}
