using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace CONCOST.UI.Controls;

/// <summary>How many nodes a <see cref="SearchableTreePicker"/> lets the user pick.</summary>
public enum PickerSelectionMode
{
    Single,
    Multiple
}

/// <summary>
/// A tree with a search box in front of it: type to narrow the tree down to the matching nodes and the
/// branches that lead to them, then pick one node or several.
/// </summary>
/// <remarks>
/// <para>
/// The search keeps <b>ancestors</b>. Filtering a tree by dropping every non-matching node would leave
/// a flat list of leaves with no context; keeping the path to each match is what makes the result
/// readable — the same thing an IDE does in its own pickers.
/// </para>
/// <para>
/// Matching goes through <see cref="CommandPaletteText.Normalize"/>, so <c>dam</c> finds "Dầm D1" and
/// <c>tang 1</c> finds "Tầng 1". In a Vietnamese-first library that is the difference between a usable
/// picker and one nobody types into twice.
/// </para>
/// </remarks>
[ContentProperty(nameof(ItemsSource))]
public class SearchableTreePicker : Control
{
    private sealed class Node
    {
        public object Item = null!;
        public string Text = string.Empty;
        public List<Node> Children = new();
        public bool Matches;
    }

    private SearchBox? _search;
    private TreeView? _tree;
    private bool _rebuilding;
    private bool _syncingSelection;
    private bool _expandedOnce;

    private readonly HashSet<object> _expanded = new(ReferenceComparer.Instance);
    private readonly List<object> _selected = new();

    static SearchableTreePicker()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(SearchableTreePicker),
            new FrameworkPropertyMetadata(typeof(SearchableTreePicker)));
    }

    /// <summary>Raised when the set of selected nodes changes.</summary>
    public event EventHandler? SelectionChanged;

    // ---------------------------------------------------------------- source

    public static readonly DependencyProperty ItemsSourceProperty =
        DependencyProperty.Register(nameof(ItemsSource), typeof(IEnumerable), typeof(SearchableTreePicker),
            new PropertyMetadata(null, OnSourceChanged));

    /// <summary>The root nodes. Children are read through <see cref="ChildrenPath"/>.</summary>
    public IEnumerable? ItemsSource
    {
        get => (IEnumerable?)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public static readonly DependencyProperty ChildrenPathProperty =
        DependencyProperty.Register(nameof(ChildrenPath), typeof(string), typeof(SearchableTreePicker),
            new PropertyMetadata("Children", OnSourceChanged));

    /// <summary>Property (or dotted path) that yields a node's children. Default <c>Children</c>.</summary>
    public string ChildrenPath
    {
        get => (string)GetValue(ChildrenPathProperty);
        set => SetValue(ChildrenPathProperty, value);
    }

    public static readonly DependencyProperty DisplayPathProperty =
        DependencyProperty.Register(nameof(DisplayPath), typeof(string), typeof(SearchableTreePicker),
            new PropertyMetadata("Name", OnSourceChanged));

    /// <summary>Property (or dotted path) shown for a node, and searched. Default <c>Name</c>.</summary>
    public string DisplayPath
    {
        get => (string)GetValue(DisplayPathProperty);
        set => SetValue(DisplayPathProperty, value);
    }

    // ---------------------------------------------------------------- search

    public static readonly DependencyProperty SearchTextProperty =
        DependencyProperty.Register(nameof(SearchText), typeof(string), typeof(SearchableTreePicker),
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnSearchChanged));

    /// <summary>The term the tree is filtered by. Two-way by default.</summary>
    public string SearchText
    {
        get => (string)GetValue(SearchTextProperty);
        set => SetValue(SearchTextProperty, value);
    }

    public static readonly DependencyProperty IsSearchEnabledProperty =
        DependencyProperty.Register(nameof(IsSearchEnabled), typeof(bool), typeof(SearchableTreePicker),
            new PropertyMetadata(true));

    /// <summary>Show the search box. Default <c>true</c>.</summary>
    public bool IsSearchEnabled
    {
        get => (bool)GetValue(IsSearchEnabledProperty);
        set => SetValue(IsSearchEnabledProperty, value);
    }

    public static readonly DependencyProperty PlaceholderProperty =
        DependencyProperty.Register(nameof(Placeholder), typeof(string), typeof(SearchableTreePicker),
            new PropertyMetadata("Tìm trong cây..."));

    /// <summary>Hint inside the search box.</summary>
    public string Placeholder
    {
        get => (string)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    public static readonly DependencyProperty IsExpandingMatchesProperty =
        DependencyProperty.Register(nameof(IsExpandingMatches), typeof(bool), typeof(SearchableTreePicker),
            new PropertyMetadata(true));

    /// <summary>Open every branch that leads to a match while searching. Default <c>true</c>.</summary>
    public bool IsExpandingMatches
    {
        get => (bool)GetValue(IsExpandingMatchesProperty);
        set => SetValue(IsExpandingMatchesProperty, value);
    }

    public static readonly DependencyProperty EmptyTextProperty =
        DependencyProperty.Register(nameof(EmptyText), typeof(string), typeof(SearchableTreePicker),
            new PropertyMetadata("Không tìm thấy mục nào"));

    /// <summary>Shown when the filter leaves nothing.</summary>
    public string EmptyText
    {
        get => (string)GetValue(EmptyTextProperty);
        set => SetValue(EmptyTextProperty, value);
    }

    // ---------------------------------------------------------------- selection

    public static readonly DependencyProperty SelectionModeProperty =
        DependencyProperty.Register(nameof(SelectionMode), typeof(PickerSelectionMode), typeof(SearchableTreePicker),
            new PropertyMetadata(PickerSelectionMode.Single, OnSelectionModeChanged));

    /// <summary>One node or several. Multiple shows a checkbox per row. Default <see cref="PickerSelectionMode.Single"/>.</summary>
    public PickerSelectionMode SelectionMode
    {
        get => (PickerSelectionMode)GetValue(SelectionModeProperty);
        set => SetValue(SelectionModeProperty, value);
    }

    public static readonly DependencyProperty SelectedItemProperty =
        DependencyProperty.Register(nameof(SelectedItem), typeof(object), typeof(SearchableTreePicker),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    /// <summary>The selected node in single mode, or the last one picked.</summary>
    public object? SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    private static readonly DependencyPropertyKey SelectedItemsKey =
        DependencyProperty.RegisterReadOnly(nameof(SelectedItems), typeof(IReadOnlyList<object>),
            typeof(SearchableTreePicker), new PropertyMetadata(Array.Empty<object>()));

    public static readonly DependencyProperty SelectedItemsProperty = SelectedItemsKey.DependencyProperty;

    /// <summary>The picked nodes, in the order they were picked.</summary>
    public IReadOnlyList<object> SelectedItems => (IReadOnlyList<object>)GetValue(SelectedItemsProperty);

    private static readonly DependencyPropertyKey SelectedCountKey =
        DependencyProperty.RegisterReadOnly(nameof(SelectedCount), typeof(int),
            typeof(SearchableTreePicker), new PropertyMetadata(0));

    public static readonly DependencyProperty SelectedCountProperty = SelectedCountKey.DependencyProperty;

    /// <summary>How many nodes are picked.</summary>
    public int SelectedCount => (int)GetValue(SelectedCountProperty);

    // ---------------------------------------------------------------- template

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        if (_search is not null) _search.TextChanged -= OnSearchBoxChanged;
        if (_tree is not null)
        {
            _tree.SelectedItemChanged -= OnTreeSelectionChanged;
            _tree.AddHandler(TreeViewItem.ExpandedEvent, new RoutedEventHandler(OnNodeExpanded));
            _tree.AddHandler(TreeViewItem.CollapsedEvent, new RoutedEventHandler(OnNodeCollapsed));
        }

        _search = GetTemplateChild("PART_Search") as SearchBox;
        _tree = GetTemplateChild("PART_Tree") as TreeView;

        if (_search is not null)
        {
            _search.Placeholder = Placeholder;
            _search.TextChanged += OnSearchBoxChanged;
        }

        if (_tree is not null)
        {
            _tree.SelectedItemChanged += OnTreeSelectionChanged;
            _tree.AddHandler(TreeViewItem.ExpandedEvent, new RoutedEventHandler(OnNodeExpanded));
            _tree.AddHandler(TreeViewItem.CollapsedEvent, new RoutedEventHandler(OnNodeCollapsed));
        }

        Rebuild();
    }

    // ---------------------------------------------------------------- public API

    /// <summary>Clears the selection without touching the tree.</summary>
    public void ClearSelection()
    {
        _selected.Clear();
        PublishSelection();
        SyncCheckBoxes();
    }

    /// <summary>Picks every node that is currently visible — what "select all" means after a search.</summary>
    public void SelectAllVisible()
    {
        if (SelectionMode != PickerSelectionMode.Multiple) return;

        foreach (var node in Flatten(BuildNodes()))
        {
            if (!_selected.Contains(node.Item)) _selected.Add(node.Item);
        }

        PublishSelection();
        SyncCheckBoxes();
    }

    /// <summary>The nodes the current filter leaves on screen, in tree order.</summary>
    public IEnumerable<object> VisibleItems => Flatten(BuildNodes()).Select(n => n.Item);

    /// <summary>
    /// Every node of a pruned tree, parents before children. The pruning already dropped the branches
    /// that do not lead to a match, so everything left is visible.
    /// </summary>
    private static IEnumerable<Node> Flatten(IEnumerable<Node> nodes)
    {
        foreach (var node in nodes)
        {
            yield return node;

            foreach (var child in Flatten(node.Children)) yield return child;
        }
    }

    // ---------------------------------------------------------------- build

    private static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SearchableTreePicker picker) picker.Rebuild();
    }

    private static void OnSearchChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not SearchableTreePicker picker) return;

        if (picker._search is not null && picker._search.Text != (string)e.NewValue)
            picker._search.Text = (string)e.NewValue;

        picker.Rebuild();
    }

    private static void OnSelectionModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not SearchableTreePicker picker) return;

        if (picker.SelectionMode == PickerSelectionMode.Single && picker._selected.Count > 1)
        {
            var keep = picker._selected[picker._selected.Count - 1];
            picker._selected.Clear();
            picker._selected.Add(keep);
            picker.PublishSelection();
        }

        picker.Rebuild();
    }

    private void OnSearchBoxChanged(object? sender, RoutedEventArgs e)
    {
        if (_search is null) return;

        SetCurrentValue(SearchTextProperty, _search.Text);
    }

    /// <summary>
    /// Reads the source into a tree of nodes, marks what the filter keeps, and prunes the rest. A node
    /// survives when it matches or when something below it does, which is what keeps the path visible.
    /// </summary>
    private List<Node> BuildNodes()
    {
        string query = CommandPaletteText.Normalize(SearchText);

        List<Node> Build(IEnumerable? items)
        {
            var nodes = new List<Node>();
            if (items is null) return nodes;

            foreach (var item in items)
            {
                if (item is null) continue;

                var node = new Node
                {
                    Item = item,
                    Text = ReadText(item),
                    Children = Build(ReadChildren(item))
                };

                node.Matches = query.Length == 0 || CommandPaletteText.Normalize(node.Text).Contains(query);
                nodes.Add(node);
            }

            return nodes;
        }

        var roots = Build(ItemsSource);

        bool Keep(Node node)
        {
            var kept = new List<Node>();

            foreach (var child in node.Children)
            {
                if (Keep(child)) kept.Add(child);
            }

            node.Children = kept;
            node.Matches = node.Matches || kept.Count > 0;
            return node.Matches;
        }

        roots.RemoveAll(n => !Keep(n));
        return roots;
    }

    private IEnumerable? ReadChildren(object item) => ReadPath(item, ChildrenPath) as IEnumerable;

    private string ReadText(object item) => ReadPath(item, DisplayPath)?.ToString() ?? item.ToString() ?? string.Empty;

    private static object? ReadPath(object item, string path)
    {
        if (string.IsNullOrEmpty(path)) return item;

        object? current = item;

        foreach (var part in path.Split('.'))
        {
            if (current is null) return null;

            var property = current.GetType().GetProperty(part,
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

            if (property is null) return null;

            current = property.GetValue(current);
        }

        return current;
    }

    private void Rebuild()
    {
        if (_tree is null || _rebuilding) return;

        _rebuilding = true;
        try
        {
            var roots = BuildNodes();

            _tree.Items.Clear();

            foreach (var node in roots) _tree.Items.Add(BuildItem(node));

            // Open the branches the first time the tree is built: a picker that starts fully collapsed
            // looks like a two-row list and gives no hint that there is anything to search inside.
            if (!_expandedOnce)
            {
                _expandedOnce = true;

                foreach (var item in Descendants(_tree.Items.OfType<TreeViewItem>()))
                {
                    item.IsExpanded = true;
                    if (item.Tag is not null) _expanded.Add(item.Tag);
                }
            }

            if (SearchText.Length > 0 && IsExpandingMatches)
            {
                foreach (var item in _tree.Items.OfType<TreeViewItem>()) item.IsExpanded = true;
            }
        }
        finally
        {
            _rebuilding = false;
        }

        SyncCheckBoxes();
    }

    private TreeViewItem BuildItem(Node node)
    {
        var item = new TreeViewItem
        {
            Header = BuildHeader(node),
            Tag = node.Item,
            IsExpanded = _expanded.Contains(node.Item) || (SearchText.Length > 0 && IsExpandingMatches)
        };

        foreach (var child in node.Children) item.Items.Add(BuildItem(child));

        return item;
    }

    private UIElement BuildHeader(Node node)
    {
        var row = new StackPanel { Orientation = Orientation.Horizontal };

        if (SelectionMode == PickerSelectionMode.Multiple)
        {
            var check = new CheckBox
            {
                IsChecked = _selected.Contains(node.Item),
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 7, 0),
                Tag = node.Item
            };

            check.Checked += OnCheckChanged;
            check.Unchecked += OnCheckChanged;
            row.Children.Add(check);
        }

        row.Children.Add(new TextBlock
        {
            Text = node.Text,
            VerticalAlignment = VerticalAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis
        });

        return row;
    }

    private void SyncCheckBoxes()
    {
        if (_tree is null) return;

        foreach (var item in Descendants(_tree.Items.OfType<TreeViewItem>()))
        {
            if (item.Header is not StackPanel row) continue;
            if (row.Children.Count == 0 || row.Children[0] is not CheckBox check) continue;
            if (check.Tag is null) continue;

            bool should = _selected.Contains(check.Tag);
            if (check.IsChecked != should) check.IsChecked = should;
        }
    }

    private static IEnumerable<TreeViewItem> Descendants(IEnumerable<TreeViewItem> items)
    {
        foreach (var item in items)
        {
            yield return item;

            foreach (var nested in Descendants(item.Items.OfType<TreeViewItem>())) yield return nested;
        }
    }

    // ---------------------------------------------------------------- interaction

    private void OnCheckChanged(object sender, RoutedEventArgs e)
    {
        if (_syncingSelection) return;
        if (sender is not CheckBox check || check.Tag is null) return;

        if (check.IsChecked == true)
        {
            if (!_selected.Contains(check.Tag)) _selected.Add(check.Tag);
        }
        else
        {
            _selected.Remove(check.Tag);
        }

        PublishSelection();
    }

    private void OnTreeSelectionChanged(object? sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (_rebuilding || SelectionMode != PickerSelectionMode.Single) return;

        if (e.NewValue is TreeViewItem item && item.Tag is not null)
        {
            _selected.Clear();
            _selected.Add(item.Tag);
            PublishSelection();
        }
    }

    private void OnNodeExpanded(object sender, RoutedEventArgs e)
    {
        if (_rebuilding) return;
        if (e.OriginalSource is TreeViewItem item && item.Tag is not null) _expanded.Add(item.Tag);
    }

    private void OnNodeCollapsed(object sender, RoutedEventArgs e)
    {
        if (_rebuilding) return;
        if (e.OriginalSource is TreeViewItem item && item.Tag is not null) _expanded.Remove(item.Tag);
    }

    private void PublishSelection()
    {
        _syncingSelection = true;
        try
        {
            SetValue(SelectedItemsKey, (IReadOnlyList<object>)_selected.ToArray());
            SetValue(SelectedCountKey, _selected.Count);

            if (SelectionMode == PickerSelectionMode.Single)
                SetCurrentValue(SelectedItemProperty, _selected.Count > 0 ? _selected[_selected.Count - 1] : null);
            else
                SetCurrentValue(SelectedItemProperty, _selected.Count > 0 ? _selected[_selected.Count - 1] : null);
        }
        finally
        {
            _syncingSelection = false;
        }

        SelectionChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Identity comparison: two nodes with equal values are still two different nodes.</summary>
    private sealed class ReferenceComparer : IEqualityComparer<object>
    {
        public static readonly ReferenceComparer Instance = new();

        public new bool Equals(object? x, object? y) => ReferenceEquals(x, y);

        public int GetHashCode(object obj) => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj);
    }
}
