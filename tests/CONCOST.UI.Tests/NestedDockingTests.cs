using System;
using System.Windows;
using System.Windows.Controls;
using CONCOST.UI.Controls;
using Xunit;

namespace CONCOST.UI.Tests;

/// <summary>
/// Nested layouts: <see cref="DockPaneGroup"/> splits an edge, and a group can hold tab groups, so the
/// layout is a tree rather than four slots.
/// </summary>
public class NestedDockingTests
{
    private static void RunOnSTA(Action action) => WpfApplicationHost.Run(action);

    private static Border Host(FrameworkElement element, double width = 600, double height = 400)
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

    private static ToolWindow Pane(string header) => new()
    {
        Header = header,
        Content = new Border()
    };

    [Fact]
    public void Group_Split_DividesItsAreaBetweenTheChildren()
    {
        RunOnSTA(() =>
        {
            var top = Pane("Trên");
            var bottom = Pane("Dưới");

            var group = new DockPaneGroup { Mode = DockGroupMode.Split, Orientation = Orientation.Vertical };
            group.Children.Add(top);
            group.Children.Add(bottom);

            Host(group);

            // Two equal slots and one splitter between them; the band is the group's own property.
            double splitter = group.SplitterThickness;
            Assert.Equal((400 - splitter) / 2, top.ActualHeight, 1);
            Assert.Equal((400 - splitter) / 2, bottom.ActualHeight, 1);
            Assert.Equal(400, top.ActualHeight + bottom.ActualHeight + splitter, 1);
        });
    }

    [Fact]
    public void Group_Split_RespectsProportion()
    {
        RunOnSTA(() =>
        {
            var top = Pane("Trên");
            var bottom = Pane("Dưới");

            var group = new DockPaneGroup { Mode = DockGroupMode.Split, Orientation = Orientation.Vertical };
            group.Children.Add(top);
            group.Children.Add(bottom);
            DockPaneGroup.SetProportion(top, 2);
            DockPaneGroup.SetProportion(bottom, 1);

            Host(group);

            Assert.Equal(2.0, top.ActualHeight / bottom.ActualHeight, 1);
        });
    }

    [Fact]
    public void Group_Tabs_ShowsOnlyTheActiveChild()
    {
        RunOnSTA(() =>
        {
            var first = Pane("Một");
            var second = Pane("Hai");

            var group = new DockPaneGroup { Mode = DockGroupMode.Tabs };
            group.Children.Add(first);
            group.Children.Add(second);

            var host = Host(group);

            Assert.Equal(Visibility.Visible, first.Visibility);
            Assert.Equal(Visibility.Collapsed, second.Visibility);
            Assert.True(first.ActualHeight > 0);

            // Switching tab swaps which one is laid out.
            second.IsActive = true;
            Relayout(host);

            Assert.Equal(Visibility.Collapsed, first.Visibility);
            Assert.Equal(Visibility.Visible, second.Visibility);
        });
    }

    [Fact]
    public void Group_Tabs_TakesItsOwnHeaderOffTheChildren()
    {
        RunOnSTA(() =>
        {
            var first = Pane("Một");
            var group = new DockPaneGroup { Mode = DockGroupMode.Tabs };
            group.Children.Add(first);
            group.Children.Add(Pane("Hai"));

            Host(group);

            // The tab strip names the panes, so a second title bar would be noise.
            Assert.False(first.ShowHeader);
        });
    }

    [Fact]
    public void Manager_AGroupOnAnEdgeOwnsThatEdge()
    {
        RunOnSTA(() =>
        {
            var top = Pane("Trên");
            var bottom = Pane("Dưới");

            var group = new DockPaneGroup
            {
                Dock = DockSide.Left,
                DockSize = 200,
                Mode = DockGroupMode.Split,
                Orientation = Orientation.Vertical
            };
            group.Children.Add(top);
            group.Children.Add(bottom);

            var manager = new DockingManager();
            manager.Children.Add(group);
            manager.Children.Add(new ToolWindow { Header = "Bản vẽ", Dock = DockSide.Center });

            Host(manager);

            Assert.Equal(200, group.ActualWidth, 1);
            Assert.Equal(400, group.ActualHeight, 1);
            Assert.True(top.ActualHeight > 0);
            Assert.True(bottom.ActualHeight > 0);
        });
    }

    [Fact]
    public void Manager_NestedPaneCanBeClosedAndDockedOut()
    {
        RunOnSTA(() =>
        {
            var inner = Pane("Cấu kiện");
            var tabs = new DockPaneGroup { Mode = DockGroupMode.Tabs };
            tabs.Children.Add(inner);
            tabs.Children.Add(Pane("Lớp"));

            var group = new DockPaneGroup { Dock = DockSide.Left, Mode = DockGroupMode.Split };
            group.Children.Add(tabs);

            var manager = new DockingManager();
            manager.Children.Add(group);
            manager.Children.Add(new ToolWindow { Header = "Bản vẽ", Dock = DockSide.Center });

            Host(manager);

            Assert.Equal(3, manager.AllPanes.Count());

            manager.Dock(inner, DockSide.Right);

            Assert.Single(tabs.Children.OfType<ToolWindow>());
            Assert.Contains(inner, manager.Panes);
            Assert.Equal(DockSide.Right, inner.Dock);
        });
    }

    [Fact]
    public void Manager_LayoutSnapshotCoversNestedPanes()
    {
        RunOnSTA(() =>
        {
            var inner = Pane("Cấu kiện");
            var tabs = new DockPaneGroup { Mode = DockGroupMode.Tabs };
            tabs.Children.Add(inner);

            var group = new DockPaneGroup { Dock = DockSide.Left, Mode = DockGroupMode.Split };
            group.Children.Add(tabs);

            var manager = new DockingManager();
            manager.Children.Add(group);
            manager.Children.Add(new ToolWindow { Header = "Bản vẽ", Dock = DockSide.Center });

            Host(manager);

            var snapshot = manager.CaptureLayout();

            // Headers are URL-escaped so a name containing the field separator cannot corrupt the file.
            Assert.Contains(Uri.EscapeDataString("Cấu kiện"), snapshot);
            Assert.Contains(Uri.EscapeDataString("Bản vẽ"), snapshot);
        });
    }

    [Fact]
    public void Manager_FloatMovesThePaneIntoAWindowAndDocksItBack()
    {
        RunOnSTA(() =>
        {
            var manager = new DockingManager();
            var pane = Pane("Nhật ký");
            pane.Dock = DockSide.Bottom;
            pane.DockSize = 150;

            manager.Children.Add(pane);
            manager.Children.Add(new ToolWindow { Header = "Bản vẽ", Dock = DockSide.Center });

            Host(manager);

            var window = manager.Float(pane);

            try
            {
                Assert.DoesNotContain(pane, manager.Panes);
                Assert.False(pane.CanAutoHide);

                window.Close();
                WpfApplicationHost.Pump();

                // Closing the floating window puts the panel back where it came from.
                Assert.Contains(pane, manager.Panes);
                Assert.Equal(DockSide.Bottom, pane.Dock);
                Assert.Equal(150, pane.DockSize, 1);
            }
            finally
            {
                if (window.IsVisible) window.Close();
            }
        });
    }

    [Fact]
    public void Manager_NestedPaneCanBeAutoHiddenToItsEdge()
    {
        RunOnSTA(() =>
        {
            var inner = Pane("Cấu kiện");
            inner.Dock = DockSide.Left;

            var tabs = new DockPaneGroup { Mode = DockGroupMode.Tabs };
            tabs.Children.Add(inner);
            tabs.Children.Add(Pane("Lớp"));

            var group = new DockPaneGroup { Dock = DockSide.Left, Mode = DockGroupMode.Split };
            group.Children.Add(tabs);

            var manager = new DockingManager();
            manager.Children.Add(group);
            manager.Children.Add(new ToolWindow { Header = "Bản vẽ", Dock = DockSide.Center });

            var host = Host(manager);

            Assert.True(ReferenceEquals(inner.Parent, tabs),
                $"parent is {inner.Parent?.GetType().Name ?? "null"}");

            inner.IsAutoHidden = true;
            Relayout(host);

            // Auto-hide lives on the edges, so the pane is pulled out of the group and onto the strip.
            Assert.DoesNotContain(inner, tabs.Children.OfType<ToolWindow>());
            Assert.Contains(inner, manager.Panes);
            Assert.Equal(Visibility.Collapsed, inner.Visibility);

            inner.IsAutoHidden = false;
            Relayout(host);

            Assert.Equal(Visibility.Visible, inner.Visibility);
        });
    }

    [Fact]
    public void Manager_MovePane_ReordersTheTabsOfAnEdge()
    {
        RunOnSTA(() =>
        {
            var first = new ToolWindow { Header = "Một", Dock = DockSide.Left };
            var second = new ToolWindow { Header = "Hai", Dock = DockSide.Left };
            var third = new ToolWindow { Header = "Ba", Dock = DockSide.Left };

            var manager = new DockingManager();
            manager.Children.Add(first);
            manager.Children.Add(second);
            manager.Children.Add(third);
            manager.Children.Add(new ToolWindow { Header = "Bản vẽ", Dock = DockSide.Center });

            Host(manager);

            manager.MovePane(third, 0);

            var order = manager.Group(DockSide.Left).Select(p => p.Header).ToList();
            Assert.Equal(new object[] { "Ba", "Một", "Hai" }, order);
        });
    }

    [Fact]
    public void Group_MoveChild_ReordersInsideTheGroup()
    {
        RunOnSTA(() =>
        {
            var first = Pane("Một");
            var second = Pane("Hai");

            var group = new DockPaneGroup { Mode = DockGroupMode.Tabs };
            group.Children.Add(first);
            group.Children.Add(second);

            Host(group);

            group.MoveChild(second, 0);

            Assert.Same(second, group.Children[0]);
        });
    }

    [Fact]
    public void Manager_DockAt_ADropOutsideTearsThePaneOut()
    {
        RunOnSTA(() =>
        {
            var pane = new ToolWindow { Header = "Nhật ký", Dock = DockSide.Bottom };
            var manager = new DockingManager();
            manager.Children.Add(pane);
            manager.Children.Add(new ToolWindow { Header = "Bản vẽ", Dock = DockSide.Center });

            Host(manager);

            // Far outside the manager's bounds — the drop becomes a float.
            manager.DockAt(pane, new Point(-40, -40));

            Assert.DoesNotContain(pane, manager.Panes);

            // Close whatever window was opened so the shared test host stays clean.
            foreach (var window in Application.Current.Windows.OfType<Window>().ToArray())
            {
                if (ReferenceEquals(window.Content, null)) continue;
                window.Close();
            }

            Assert.Contains(pane, manager.Panes);
        });
    }

    [Fact]
    public void Manager_FloatTwice_ReusesTheWindowInsteadOfThrowing()
    {
        RunOnSTA(() =>
        {
            var manager = new DockingManager();
            var pane = new ToolWindow { Header = "Thuộc tính", Dock = DockSide.Right };
            manager.Children.Add(pane);
            manager.Children.Add(new ToolWindow { Header = "Bản vẽ", Dock = DockSide.Center });

            Host(manager);

            var first = manager.Float(pane);
            Assert.True(manager.IsFloating(pane));

            // The second call used to reach Border.Child with a pane that still belonged to the first
            // window's host, and WPF threw "already the logical child of another element".
            var second = manager.Float(pane);

            Assert.Same(first, second);

            first.Close();
            WpfApplicationHost.Pump();

            Assert.False(manager.IsFloating(pane));
            Assert.Contains(pane, manager.Panes);
        });
    }
}
