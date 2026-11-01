using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using CONCOST.UI.Controls;
using Xunit;

namespace CONCOST.UI.Tests;

/// <summary>
/// The docking cluster: <see cref="DockingManager"/>, <see cref="ToolWindow"/>,
/// <see cref="AutoHidePane"/>, <see cref="DockSplitter"/> and <see cref="DockPreview"/>.
/// </summary>
public class DockingTests
{
    private static void RunOnSTA(Action action) => WpfApplicationHost.Run(action);

    private static T Arrange<T>(T element, double width = 600, double height = 400) where T : FrameworkElement
    {
        ArrangeIn(element, width, height);
        return element;
    }

    private static Border ArrangeIn(FrameworkElement element, double width = 600, double height = 400)
    {
        var host = new Border { Child = element };
        Relayout(host, width, height);
        return host;
    }

    private static void Relayout(Border host, double width = 600, double height = 400)
    {
        host.Measure(new Size(width, height));
        host.Arrange(new Rect(0, 0, width, height));
        host.UpdateLayout();
    }

    private static ToolWindow Pane(string header, DockSide side, double size) => new()
    {
        Header = header,
        Dock = side,
        DockSize = size,
        Content = new Border()
    };

    // ------------------------------------------------------------------ ToolWindow

    [Fact]
    public void ToolWindow_Defaults_MatchSpecifications()
    {
        RunOnSTA(() =>
        {
            var window = new ToolWindow();

            Assert.Equal(DockSide.Left, window.Dock);
            Assert.Equal(260, window.DockSize);
            Assert.Equal(120, window.MinDockSize);
            Assert.True(window.ShowHeader);
            Assert.True(window.CanClose);
            Assert.True(window.CanAutoHide);
            Assert.False(window.IsAutoHidden);
            Assert.False(window.IsFlyoutOpen);
        });
    }

    [Fact]
    public void ToolWindow_Close_RaisesClosed_AndCanBeCancelled()
    {
        RunOnSTA(() =>
        {
            var window = new ToolWindow { Header = "Lớp" };
            int closed = 0;
            window.Closed += (_, _) => closed++;

            EventHandler<CancelEventArgs> cancel = (_, e) => e.Cancel = true;
            window.Closing += cancel;

            window.Close();
            Assert.Equal(0, closed);

            window.Closing -= cancel;
            window.Close();
            Assert.Equal(1, closed);
        });
    }

    [Fact]
    public void ToolWindow_ToggleAutoHide_ResetsTheFlyout()
    {
        RunOnSTA(() =>
        {
            var window = new ToolWindow { Header = "Thuộc tính" };

            window.IsAutoHidden = true;
            window.IsFlyoutOpen = true;
            window.ToggleAutoHide();

            Assert.False(window.IsAutoHidden);
            Assert.False(window.IsFlyoutOpen);
        });
    }

    [Fact]
    public void ToolWindow_HeaderBarShowsTheHeader_NotTheBodyContent()
    {
        RunOnSTA(() =>
        {
            var body = new Border { Name = "Body" };
            var pane = new ToolWindow { Header = "Cấu kiện", Content = body };

            Arrange(pane, 240, 300);

            var texts = new System.Collections.Generic.List<string>();
            CollectText(pane, texts);

            Assert.Contains("Cấu kiện", texts);

            // The body must be rendered once, in the body — not mirrored into the header.
            Assert.Single(texts.FindAll(t => t == "Cấu kiện"));
        });
    }

    private static void CollectText(DependencyObject root, System.Collections.Generic.List<string> sink)
    {
        int count = System.Windows.Media.VisualTreeHelper.GetChildrenCount(root);
        for (int i = 0; i < count; i++)
        {
            var child = System.Windows.Media.VisualTreeHelper.GetChild(root, i);
            if (child is TextBlock block && !string.IsNullOrEmpty(block.Text)) sink.Add(block.Text);
            CollectText(child, sink);
        }
    }

    // ------------------------------------------------------------------ DockingManager

    [Fact]
    public void DockingManager_LaysOutEdgesAndCentre()
    {
        RunOnSTA(() =>
        {
            var manager = new DockingManager();
            var left = Pane("Cây", DockSide.Left, 200);
            var bottom = Pane("Nhật ký", DockSide.Bottom, 150);
            var centre = Pane("Bản vẽ", DockSide.Center, 0);

            manager.Children.Add(left);
            manager.Children.Add(bottom);
            manager.Children.Add(centre);

            Arrange(manager);

            Assert.Equal(200, left.ActualWidth, 1);
            Assert.Equal(400, left.ActualHeight, 1);
            Assert.Equal(150, bottom.ActualHeight, 1);
            Assert.True(centre.ActualWidth > 0);
            Assert.True(centre.ActualHeight > 0);
            Assert.True(centre.ActualWidth < 600);
        });
    }

    [Fact]
    public void DockingManager_Dock_MovesThePanelAndRaisesPaneDocked()
    {
        RunOnSTA(() =>
        {
            var manager = new DockingManager();
            var pane = Pane("Cây", DockSide.Left, 180);
            manager.Children.Add(pane);
            var host = ArrangeIn(manager);

            ToolWindow? docked = null;
            manager.PaneDocked += (_, e) => docked = e;

            manager.Dock(pane, DockSide.Right);

            Assert.Same(pane, docked);
            Assert.Equal(DockSide.Right, pane.Dock);

            Relayout(host);
            Assert.Equal(180, pane.ActualWidth, 1);
        });
    }

    [Fact]
    public void DockingManager_ClosePane_RemovesItAndRaisesPaneClosed()
    {
        RunOnSTA(() =>
        {
            var manager = new DockingManager();
            var pane = Pane("Cây", DockSide.Left, 180);
            manager.Children.Add(pane);

            ToolWindow? closed = null;
            manager.PaneClosed += (_, e) => closed = e;

            manager.ClosePane(pane);

            Assert.Same(pane, closed);
            Assert.Empty(manager.Panes);
        });
    }

    [Fact]
    public void DockingManager_AutoHiddenPanel_BecomesATabAndOpensAsAFlyout()
    {
        RunOnSTA(() =>
        {
            var manager = new DockingManager();
            var pane = Pane("Thuộc tính", DockSide.Left, 220);
            var centre = Pane("Bản vẽ", DockSide.Center, 0);
            manager.Children.Add(pane);
            manager.Children.Add(centre);

            var host = ArrangeIn(manager);
            Assert.Equal(Visibility.Visible, pane.Visibility);

            pane.IsAutoHidden = true;
            Relayout(host);

            // Collapsed out of the layout, and the centre now runs the full width minus the strip.
            Assert.Equal(Visibility.Collapsed, pane.Visibility);
            Assert.True(centre.ActualWidth > 500);

            manager.ToggleFlyout(pane);
            Relayout(host);

            Assert.True(pane.IsFlyoutOpen);
            Assert.Equal(Visibility.Visible, pane.Visibility);
            Assert.Equal(220, pane.ActualWidth, 1);

            manager.CloseFlyouts();
            Relayout(host);
            Assert.False(pane.IsFlyoutOpen);
        });
    }

    [Fact]
    public void DockingManager_ToggleFlyout_ClosesTheOtherOne()
    {
        RunOnSTA(() =>
        {
            var manager = new DockingManager();
            var first = Pane("Một", DockSide.Left, 200);
            var second = Pane("Hai", DockSide.Right, 200);
            manager.Children.Add(first);
            manager.Children.Add(second);

            first.IsAutoHidden = true;
            second.IsAutoHidden = true;
            Arrange(manager);

            manager.ToggleFlyout(first);
            manager.ToggleFlyout(second);

            Assert.False(first.IsFlyoutOpen);
            Assert.True(second.IsFlyoutOpen);
        });
    }

    [Fact]
    public void DockingManager_FlyoutIsDrawnAboveTheCentrePane()
    {
        RunOnSTA(() =>
        {
            var manager = new DockingManager();
            var left = Pane("Thuộc tính", DockSide.Left, 220);
            var centre = Pane("Bản vẽ", DockSide.Center, 0);
            manager.Children.Add(left);
            manager.Children.Add(centre);

            Arrange(manager);

            left.IsAutoHidden = true;
            manager.ToggleFlyout(left);

            var order = manager.Children.Cast<UIElement>().ToList();

            // Child order is z-order in a Panel: a flyout that is not last would be painted under the
            // document area and the user would never see it.
            Assert.True(order.IndexOf(left) > order.IndexOf(centre),
                "the flyout must be drawn after the centre pane");
        });
    }

    [Fact]
    public void DockingManager_ClosingAToolWindowRemovesIt()
    {
        RunOnSTA(() =>
        {
            var manager = new DockingManager();
            var pane = Pane("Cây", DockSide.Left, 180);
            manager.Children.Add(pane);

            pane.Close();

            Assert.Empty(manager.Panes);
        });
    }

    [Fact]
    public void DockingManager_SplitterBandAcceptsTheMouse()
    {
        RunOnSTA(() =>
        {
            var manager = new DockingManager();
            var left = Pane("Cây", DockSide.Left, 200);
            var centre = Pane("Bản vẽ", DockSide.Center, 0);
            manager.Children.Add(left);
            manager.Children.Add(centre);

            // Hit testing needs a real presentation source, so this one runs in a shown window.
            var window = new Window
            {
                Width = 620,
                Height = 440,
                Content = manager,
                ShowInTaskbar = false,
                WindowStyle = WindowStyle.None
            };

            window.Show();
            window.UpdateLayout();
            WpfApplicationHost.Pump();

            try
            {
                var bandPoint = new Point(200 + manager.SplitterThickness / 2, 200);

                // A Panel paints nothing by default, so without an explicit hit surface the whole band
                // is empty space and the resize gesture can never start.
                Assert.Same(manager, manager.InputHitTest(bandPoint));

                // The manager claims the bands and nothing else — it must not swallow the whole surface.
                Assert.NotSame(manager, manager.InputHitTest(new Point(400, 200)));
            }
            finally
            {
                window.Content = null;
                window.Close();
            }
        });
    }

    [Fact]
    public void DockingManager_PanesOnOneEdgeFormATabGroup()
    {
        RunOnSTA(() =>
        {
            var manager = new DockingManager();
            var first = Pane("Cây", DockSide.Left, 200);
            var second = Pane("Lớp", DockSide.Left, 200);
            var right = Pane("Thuộc tính", DockSide.Right, 200);
            manager.Children.Add(first);
            manager.Children.Add(second);
            manager.Children.Add(right);

            Arrange(manager);

            // Two panes on the same edge are one group shown as tabs, so only one of them is laid out.
            Assert.Equal(2, manager.Group(DockSide.Left).Count);
            Assert.Single(manager.Group(DockSide.Right));

            Assert.Equal(Visibility.Visible, first.Visibility);
            Assert.Equal(Visibility.Collapsed, second.Visibility);

            // They share a size, or switching tab would resize the edge.
            Assert.Equal(first.DockSize, second.DockSize, 1);

            // Docking a pane makes it the visible tab of its group.
            manager.Dock(second, DockSide.Left);

            Assert.True(second.IsActive);
            Assert.False(first.IsActive);
        });
    }

    [Fact]
    public void DockingManager_DrawsAHairlineInTheSplitterBand()
    {
        RunOnSTA(() =>
        {
            var manager = new DockingManager();
            manager.Children.Add(Pane("Cây", DockSide.Left, 200));
            manager.Children.Add(Pane("Bản vẽ", DockSide.Center, 0));

            var host = ArrangeIn(manager, 600, 400);

            var bitmap = new System.Windows.Media.Imaging.RenderTargetBitmap(600, 400, 96, 96,
                System.Windows.Media.PixelFormats.Pbgra32);
            bitmap.Render(host);

            var pixels = new byte[4];

            // The hairline sits in the middle of the band, and the band's width is the manager's,
            // so the probe follows the property instead of a number that drifts with the default.
            int x = 200 + (int)Math.Floor(manager.SplitterThickness / 2);
            bitmap.CopyPixels(new Int32Rect(x, 200, 1, 1), pixels, 4, 0);

            // Nothing else paints in the band, so a non-zero alpha proves the hairline is there.
            Assert.True(pixels[3] > 0, "the splitter band painted nothing — the divider is invisible");
        });
    }

    [Fact]
    public void DockingManager_KeepsEachPaneShareWhenTheWindowResizes()
    {
        RunOnSTA(() =>
        {
            var manager = new DockingManager();
            var left = Pane("Cây", DockSide.Left, 300);
            manager.Children.Add(left);
            manager.Children.Add(Pane("Bản vẽ", DockSide.Center, 0));

            var host = ArrangeIn(manager, 600, 400);
            Assert.Equal(300, left.ActualWidth, 1);

            // Half the width: the pane keeps its share instead of staying at 300 pixels.
            Relayout(host, 300, 400);

            Assert.Equal(150, left.ActualWidth, 1);
        });
    }

    [Fact]
    public void DockingManager_IsProportionalCanBeTurnedOff()
    {
        RunOnSTA(() =>
        {
            var manager = new DockingManager { IsProportional = false };
            var left = Pane("Cây", DockSide.Left, 150);
            manager.Children.Add(left);
            manager.Children.Add(Pane("Bản vẽ", DockSide.Center, 0));

            var host = ArrangeIn(manager, 600, 400);
            Relayout(host, 300, 400);

            // Pinned to pixels: the pane keeps its width and the centre gives up the space.
            Assert.Equal(150, left.ActualWidth, 1);
        });
    }

    [Fact]
    public void DockingManager_CaptureAndApplyLayout_RoundTrips()
    {
        RunOnSTA(() =>
        {
            var manager = new DockingManager();
            var tree = Pane("Cây", DockSide.Left, 180);
            var props = Pane("Thuộc tính", DockSide.Right, 240);
            manager.Children.Add(tree);
            manager.Children.Add(props);

            Arrange(manager);

            tree.IsActive = true;
            var saved = manager.CaptureLayout();

            manager.Dock(tree, DockSide.Bottom);
            tree.DockSize = 90;
            tree.IsAutoHidden = true;

            Assert.True(manager.ApplyLayout(saved));

            Assert.Equal(DockSide.Left, tree.Dock);
            Assert.Equal(180, tree.DockSize, 1);
            Assert.False(tree.IsAutoHidden);
            Assert.Equal(DockSide.Right, props.Dock);
        });
    }

    [Fact]
    public void DockingManager_ApplyLayout_IgnoresAnUnknownSnapshot()
    {
        RunOnSTA(() =>
        {
            var manager = new DockingManager();
            manager.Children.Add(Pane("Cây", DockSide.Left, 180));

            Assert.False(manager.ApplyLayout(null));
            Assert.False(manager.ApplyLayout("not a layout"));
            Assert.False(manager.ApplyLayout("CONCOST-DOCK-1\nLeft|200|False|True|Không tồn tại"));
        });
    }

    // ------------------------------------------------------------------ DockSplitter

    [Fact]
    public void DockSplitter_Defaults_AndClamping()
    {
        RunOnSTA(() =>
        {
            var splitter = new DockSplitter();

            Assert.Equal(Orientation.Vertical, splitter.Orientation);
            Assert.Equal(6, splitter.Thickness);
            Assert.Equal(0, splitter.Minimum);
            Assert.True(double.IsPositiveInfinity(splitter.Maximum));

            splitter.Minimum = 100;
            splitter.Maximum = 300;
            splitter.Value = 50;

            // Value is a plain DP: clamping happens while dragging, not on assignment.
            Assert.Equal(50, splitter.Value);
        });
    }

    [Fact]
    public void DockSplitter_MeasuresThinAlongItsAxis()
    {
        RunOnSTA(() =>
        {
            var splitter = new DockSplitter { Orientation = Orientation.Horizontal, Thickness = 8 };
            splitter.Measure(new Size(200, 200));

            Assert.Equal(200, splitter.DesiredSize.Width, 1);
            Assert.Equal(8, splitter.DesiredSize.Height, 1);
        });
    }

    // ------------------------------------------------------------------ DockPreview / AutoHidePane

    [Fact]
    public void DockPreview_IsOpen_DrivesVisibility()
    {
        RunOnSTA(() =>
        {
            var preview = new DockPreview();

            Assert.False(preview.IsOpen);
            Assert.Equal(Visibility.Collapsed, preview.Visibility);

            preview.IsOpen = true;
            Assert.Equal(Visibility.Visible, preview.Visibility);
        });
    }

    [Fact]
    public void AutoHidePane_CollectsOnlyToolWindows()
    {
        RunOnSTA(() =>
        {
            var strip = new AutoHidePane();
            var pane = Pane("Một", DockSide.Left, 200);

            strip.Panes = new object[] { pane, "không phải tool window" };

            Assert.Single(strip.Tabs);
            Assert.Same(pane, strip.Tabs[0]);
        });
    }
}
