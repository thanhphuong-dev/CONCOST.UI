using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CONCOST.UI.Controls;
using Xunit;

namespace CONCOST.UI.Tests;

/// <summary>
/// The data controls added for the roadmap: <see cref="TreeDataGrid"/>,
/// <see cref="MultiSelectDataGrid"/> and <see cref="TreeMap"/>.
/// </summary>
public class DataControlsTests
{
    private static void RunOnSTA(Action action) => WpfApplicationHost.Run(action);

    // ------------------------------------------------------------------ fixtures

    public sealed class Node
    {
        public Node(string name, params Node[] children)
        {
            Name = name;
            Children = children;
        }

        public string Name { get; }

        public Node[] Children { get; }
    }

    public sealed class Slice
    {
        public Slice(string label, double value)
        {
            Label = label;
            Value = value;
        }

        public string Label { get; }

        public double Value { get; }
    }

    private static Node[] BuildTree() => new[]
    {
        new Node("Kết cấu", new Node("Dầm", new Node("D1"), new Node("D2")), new Node("Cột")),
        new Node("Kiến trúc")
    };

    private static T Arrange<T>(T element, double width = 400, double height = 300) where T : FrameworkElement
    {
        var host = new Border { Child = element };
        host.Measure(new Size(width, height));
        host.Arrange(new Rect(0, 0, width, height));
        host.UpdateLayout();
        return element;
    }

    private static T? FindDescendant<T>(DependencyObject root) where T : DependencyObject
    {
        int count = VisualTreeHelper.GetChildrenCount(root);
        for (int i = 0; i < count; i++)
        {
            var child = VisualTreeHelper.GetChild(root, i);
            if (child is T match) return match;

            var deeper = FindDescendant<T>(child);
            if (deeper is not null) return deeper;
        }

        return null;
    }

    // ------------------------------------------------------------------ TreeDataGrid

    [Fact]
    public void TreeDataGrid_Defaults_MatchSpecifications()
    {
        RunOnSTA(() =>
        {
            var grid = new TreeDataGrid();

            Assert.Equal("Children", grid.ChildrenPath);
            Assert.True(grid.ShowHierarchyColumn);
            Assert.Equal(18, grid.IndentSize);
            Assert.Equal(260, grid.HierarchyColumnWidth);
            Assert.Equal(string.Empty, grid.HierarchyPath);
            Assert.Equal(0, grid.VisibleItemCount);
        });
    }

    [Fact]
    public void TreeDataGrid_HierarchyPath_BindsTheCellToThatProperty()
    {
        RunOnSTA(() =>
        {
            var grid = new TreeDataGrid { RootItems = BuildTree() };

            // No path: the cell shows the node itself, so the column still exists.
            Assert.Single(grid.Columns);

            grid.HierarchyPath = "Name";

            // Setting the path rebuilds the generated column rather than leaving a stale binding.
            Assert.Single(grid.Columns);
            Assert.Equal("Tên", grid.HierarchyHeader);
        });
    }

    [Fact]
    public void TreeDataGrid_StartsCollapsed_ThenExpandAllShowsEveryNode()
    {
        RunOnSTA(() =>
        {
            var grid = new TreeDataGrid { RootItems = BuildTree() };

            // Two roots; the three children stay hidden until a branch is opened.
            Assert.Equal(2, grid.VisibleItemCount);

            grid.ExpandAll();
            Assert.Equal(6, grid.VisibleItemCount);

            grid.CollapseAll();
            Assert.Equal(2, grid.VisibleItemCount);
        });
    }

    [Fact]
    public void TreeDataGrid_ToggleNode_ExpandsOnlyThatBranch()
    {
        RunOnSTA(() =>
        {
            var tree = BuildTree();
            var root = tree[0];
            var grid = new TreeDataGrid { RootItems = tree };

            grid.ToggleNode(root);

            // "Kết cấu" opens to "Dầm" + "Cột"; "Dầm" is still closed, so 2 roots + 2 = 4.
            Assert.Equal(4, grid.VisibleItemCount);
            Assert.True(grid.IsNodeExpanded(root));

            grid.ToggleNode(root);
            Assert.Equal(2, grid.VisibleItemCount);
            Assert.False(grid.IsNodeExpanded(root));
        });
    }

    [Fact]
    public void TreeDataGrid_ExpandTo_OpensTheWholePath()
    {
        RunOnSTA(() =>
        {
            var root = BuildTree()[0];
            var beam = root.Children[0];
            var deepest = beam.Children[0];

            var grid = new TreeDataGrid { RootItems = new[] { root } };

            grid.ExpandTo(deepest);

            Assert.True(grid.IsNodeExpanded(root));
            Assert.True(grid.IsNodeExpanded(beam));
            Assert.Equal(5, grid.VisibleItemCount);
        });
    }

    [Fact]
    public void TreeDataGrid_GeneratesTheHierarchyColumnFirst()
    {
        RunOnSTA(() =>
        {
            var grid = new TreeDataGrid { RootItems = BuildTree() };
            grid.Columns.Add(new DataGridTextColumn { Header = "Tên", Binding = new System.Windows.Data.Binding("Name") });

            Assert.Equal(2, grid.Columns.Count);
            Assert.IsType<DataGridTemplateColumn>(grid.Columns[0]);
            Assert.Equal("Tên", grid.Columns[1].Header);

            grid.ShowHierarchyColumn = false;
            Assert.Single(grid.Columns);

            grid.ShowHierarchyColumn = true;
            Assert.Equal(2, grid.Columns.Count);
            Assert.IsType<DataGridTemplateColumn>(grid.Columns[0]);
        });
    }

    [Fact]
    public void TreeDataGrid_AppliesTheDataGridTemplate()
    {
        RunOnSTA(() =>
        {
            var grid = new TreeDataGrid
            {
                RootItems = BuildTree(),
                Height = 240,
                Width = 360
            };
            grid.Columns.Add(new DataGridTextColumn
            {
                Header = "Tên",
                Binding = new System.Windows.Data.Binding("Name")
            });

            var window = new Window
            {
                Width = 380,
                Height = 260,
                Content = grid,
                ShowInTaskbar = false,
                WindowStyle = WindowStyle.None
            };

            window.Show();
            window.UpdateLayout();
            WpfApplicationHost.Pump();

            try
            {
                // The style is BasedOn CONCOST.DataGrid.Base, so the grid must get a real template.
                Assert.NotNull(grid.Template);
                Assert.NotNull(FindDescendant<ScrollViewer>(grid));
                Assert.Equal(2, grid.VisibleItemCount);
            }
            finally
            {
                window.Content = null;
                window.Close();
            }
        });
    }

    [Fact]
    public void TreeDataGrid_CustomChildrenPath_IsHonoured()
    {
        RunOnSTA(() =>
        {
            var grid = new TreeDataGrid { ChildrenPath = "Kids" };
            grid.RootItems = new[] { new Node("A", new Node("A1")) };

            // "Kids" does not exist on Node, so nothing has children.
            Assert.Equal(1, grid.VisibleItemCount);

            grid.ChildrenPath = "Children";
            Assert.Equal(1, grid.VisibleItemCount);
            grid.ExpandAll();
            Assert.Equal(2, grid.VisibleItemCount);
        });
    }

    // ------------------------------------------------------------------ MultiSelectDataGrid

    [Fact]
    public void MultiSelectDataGrid_CheckBoxColumnAndDefaults()
    {
        RunOnSTA(() =>
        {
            var grid = new MultiSelectDataGrid();

            Assert.True(grid.ShowCheckBoxColumn);
            Assert.Equal(44, grid.CheckBoxColumnWidth);
            Assert.Single(grid.Columns);
            Assert.IsType<DataGridTemplateColumn>(grid.Columns[0]);
            Assert.Equal(0, grid.SelectedCount);
            Assert.False(grid.IsAllSelected);
        });
    }

    [Fact]
    public void MultiSelectDataGrid_SelectEverythingThenClear()
    {
        RunOnSTA(() =>
        {
            var grid = new MultiSelectDataGrid { ItemsSource = new[] { "a", "b", "c", "d" } };
            grid.Columns.Add(new DataGridTextColumn { Binding = new System.Windows.Data.Binding() });

            grid.SelectEverything();

            Assert.Equal(4, grid.SelectedCount);
            Assert.True(grid.IsAllSelected);
            Assert.Equal(4, grid.SelectedItemsList.Count);

            grid.ClearEverything();

            Assert.Equal(0, grid.SelectedCount);
            Assert.False(grid.IsAllSelected);
            Assert.Empty(grid.SelectedItemsList);
        });
    }

    [Fact]
    public void MultiSelectDataGrid_ToggleItem_ProducesPartialState()
    {
        RunOnSTA(() =>
        {
            var grid = new MultiSelectDataGrid { ItemsSource = new[] { "a", "b", "c" } };

            grid.ToggleItem("b");

            Assert.Equal(1, grid.SelectedCount);
            Assert.True(grid.IsItemSelected("b"));
            Assert.Null(grid.IsAllSelected);

            grid.ToggleItem("b");
            Assert.Equal(0, grid.SelectedCount);
            Assert.False(grid.IsAllSelected);
        });
    }

    [Fact]
    public void MultiSelectDataGrid_InvertSelection_FlipsEveryRow()
    {
        RunOnSTA(() =>
        {
            var grid = new MultiSelectDataGrid { ItemsSource = new[] { "a", "b", "c" } };

            grid.ToggleItem("a");
            grid.InvertSelection();

            Assert.Equal(2, grid.SelectedCount);
            Assert.False(grid.IsItemSelected("a"));
            Assert.True(grid.IsItemSelected("b"));
            Assert.True(grid.IsItemSelected("c"));
        });
    }

    [Fact]
    public void MultiSelectDataGrid_HeaderFlagDrivesTheSelection()
    {
        RunOnSTA(() =>
        {
            var grid = new MultiSelectDataGrid { ItemsSource = new[] { "a", "b" } };

            grid.IsAllSelected = true;
            Assert.Equal(2, grid.SelectedCount);

            grid.IsAllSelected = false;
            Assert.Equal(0, grid.SelectedCount);
        });
    }

    [Fact]
    public void MultiSelectDataGrid_HidingTheColumnKeepsTheSelection()
    {
        RunOnSTA(() =>
        {
            var grid = new MultiSelectDataGrid { ItemsSource = new[] { "a", "b" } };
            grid.ToggleItem("a");

            grid.ShowCheckBoxColumn = false;

            Assert.Empty(grid.Columns);
            Assert.Equal(1, grid.SelectedCount);
        });
    }

    // ------------------------------------------------------------------ TreeMap

    [Fact]
    public void TreeMap_LaysOutEveryPositiveValue()
    {
        RunOnSTA(() =>
        {
            var map = Arrange(new TreeMap
            {
                ItemsSource = new[]
                {
                    new Slice("Bê tông", 60),
                    new Slice("Cốp pha", 30),
                    new Slice("Thép", 10)
                },
                Width = 400,
                Height = 300
            });

            var entries = map.Entries;

            Assert.Equal(3, entries.Count);
            Assert.Equal("Bê tông", entries[0].Label);
            Assert.Equal(0.6, entries[0].Share, 3);

            double share = entries.Sum(e => e.Share);
            Assert.Equal(1.0, share, 6);

            // The area is fully covered: sum of the rectangle areas equals the control area.
            double area = entries.Sum(e => e.Bounds.Width * e.Bounds.Height);
            Assert.InRange(area, 400 * 300 - 1, 400 * 300 + 1);
        });
    }

    [Fact]
    public void TreeMap_DropsZeroAndNegativeValues()
    {
        RunOnSTA(() =>
        {
            var map = Arrange(new TreeMap
            {
                ItemsSource = new[]
                {
                    new Slice("Có", 10),
                    new Slice("Không", 0),
                    new Slice("Âm", -5)
                }
            });

            Assert.Single(map.Entries);
            Assert.Equal("Có", map.Entries[0].Label);
        });
    }

    [Fact]
    public void TreeMap_HitTest_ReturnsTheRectangleUnderThePoint()
    {
        RunOnSTA(() =>
        {
            var map = Arrange(new TreeMap
            {
                ItemsSource = new[] { new Slice("Lớn", 90), new Slice("Nhỏ", 10) }
            });

            var first = map.Entries[0];
            var centre = new Point(first.Bounds.X + first.Bounds.Width / 2, first.Bounds.Y + first.Bounds.Height / 2);

            var hit = map.HitTest(centre);
            Assert.NotNull(hit);
            Assert.Equal("Lớn", hit!.Label);

            // A point outside the control hits nothing.
            Assert.Null(map.HitTest(new Point(-10, -10)));
        });
    }

    [Fact]
    public void TreeMap_CyclesThePalette()
    {
        RunOnSTA(() =>
        {
            var map = Arrange(new TreeMap
            {
                ItemsSource = new[] { new Slice("a", 3), new Slice("b", 2), new Slice("c", 1) },
                Palette = new[] { Brushes.Red, Brushes.Blue }
            });

            var entries = map.Entries;

            Assert.Same(Brushes.Red, entries[0].Brush);
            Assert.Same(Brushes.Blue, entries[1].Brush);
            Assert.Same(Brushes.Red, entries[2].Brush);
        });
    }

    [Fact]
    public void TreeMap_ClickRaisesItemInvoked()
    {
        RunOnSTA(() =>
        {
            var map = Arrange(new TreeMap
            {
                ItemsSource = new[] { new Slice("Duy nhất", 1) }
            });

            TreeMapEntry? invoked = null;
            map.ItemInvoked += (_, e) => invoked = e.Entry;

            // A synthetic mouse event carries no position, and the element sits at the origin, so
            // GetPosition resolves to (0,0) — inside the only rectangle.
            map.RaiseEvent(new System.Windows.Input.MouseButtonEventArgs(
                System.Windows.Input.Mouse.PrimaryDevice,
                Environment.TickCount,
                System.Windows.Input.MouseButton.Left)
            {
                RoutedEvent = System.Windows.Input.Mouse.MouseUpEvent,
                Source = map
            });

            Assert.NotNull(invoked);
            Assert.Equal("Duy nhất", invoked!.Label);
        });
    }
}
