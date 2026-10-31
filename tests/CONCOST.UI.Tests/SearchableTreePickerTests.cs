using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using CONCOST.UI.Controls;
using Xunit;

namespace CONCOST.UI.Tests;

/// <summary>
/// <see cref="SearchableTreePicker"/>: filtering keeps the path to a match, matching ignores
/// diacritics, and selection works in both modes.
/// </summary>
public class SearchableTreePickerTests
{
    private sealed class Node
    {
        public Node(string name) => Name = name;

        public string Name { get; }

        public List<Node> Children { get; } = new();
    }

    private static void RunOnSTA(Action action) => WpfApplicationHost.Run(action);

    private static Node BuildTree()
    {
        var root = new Node("Tầng 1");
        var beams = new Node("Dầm");
        beams.Children.Add(new Node("Dầm D1"));
        beams.Children.Add(new Node("Dầm D2"));

        var columns = new Node("Cột");
        columns.Children.Add(new Node("Cột C1"));

        root.Children.Add(beams);
        root.Children.Add(columns);
        return root;
    }

    private static SearchableTreePicker Picker(params object[] roots) => new()
    {
        ItemsSource = roots,
        ChildrenPath = "Children",
        DisplayPath = "Name"
    };

    [Fact]
    public void Picker_NoFilter_ShowsEveryNode()
    {
        RunOnSTA(() =>
        {
            var picker = Picker(BuildTree());

            // Root, its two branches, and the three leaves.
            Assert.Equal(6, picker.VisibleItems.Count());
        });
    }

    [Fact]
    public void Picker_Search_KeepsThePathToAMatch()
    {
        RunOnSTA(() =>
        {
            var root = BuildTree();
            var picker = Picker(root);

            picker.SearchText = "Dầm D2";

            // The leaf matches, so its parent survives too — a flat list of leaves would lose the path.
            var visible = picker.VisibleItems.ToList();
            Assert.Equal(new object[] { root, root.Children[0], root.Children[0].Children[1] }, visible);
        });
    }

    [Fact]
    public void Picker_Search_IgnoresDiacritics()
    {
        RunOnSTA(() =>
        {
            var root = BuildTree();
            var picker = Picker(root);

            picker.SearchText = "dam d2";

            Assert.Equal(3, picker.VisibleItems.Count());
        });
    }

    [Fact]
    public void Picker_Search_WithNoMatch_LeavesNothing()
    {
        RunOnSTA(() =>
        {
            var picker = Picker(BuildTree());

            picker.SearchText = "không có gì khớp";

            Assert.Empty(picker.VisibleItems);
        });
    }

    [Fact]
    public void Picker_SelectAllVisible_OnlyTakesWhatIsOnScreen()
    {
        RunOnSTA(() =>
        {
            var picker = Picker(BuildTree());
            picker.SelectionMode = PickerSelectionMode.Multiple;

            picker.SelectAllVisible();
            Assert.Equal(6, picker.SelectedCount);

            picker.ClearSelection();
            Assert.Equal(0, picker.SelectedCount);

            picker.SearchText = "Cột";
            picker.SelectAllVisible();

            // The root, "Cột" and "Cột C1" — nothing from the beam branch.
            Assert.Equal(3, picker.SelectedCount);
        });
    }

    [Fact]
    public void Picker_SingleMode_KeepsOneItem()
    {
        RunOnSTA(() =>
        {
            var root = BuildTree();
            var picker = Picker(root);
            picker.SelectionMode = PickerSelectionMode.Multiple;

            picker.SelectAllVisible();
            Assert.Equal(6, picker.SelectedCount);

            // Switching to single trims the selection to the last pick instead of leaving it invalid.
            picker.SelectionMode = PickerSelectionMode.Single;

            Assert.Equal(1, picker.SelectedCount);
            Assert.NotNull(picker.SelectedItem);
        });
    }

    [Fact]
    public void Picker_SelectionChanged_Fires()
    {
        RunOnSTA(() =>
        {
            var picker = Picker(BuildTree());
            picker.SelectionMode = PickerSelectionMode.Multiple;

            int raised = 0;
            picker.SelectionChanged += (_, _) => raised++;

            picker.SelectAllVisible();
            picker.ClearSelection();

            Assert.Equal(2, raised);
        });
    }

    [Fact]
    public void Picker_DottedDisplayPath_IsRead()
    {
        RunOnSTA(() =>
        {
            var root = new Node("A");
            root.Children.Add(new Node("B"));

            var picker = new SearchableTreePicker
            {
                ItemsSource = new[] { new { Name = "Nhóm", Info = new { Label = "Chi tiết" } } },
                ChildrenPath = "Children",
                DisplayPath = "Info.Label"
            };

            // The dotted path resolves through two properties: "Info.Label" is what is shown and searched.
            picker.SearchText = "chi tiết";
            Assert.Single(picker.VisibleItems);

            picker.SearchText = "label";
            Assert.Empty(picker.VisibleItems);
        });
    }
}
