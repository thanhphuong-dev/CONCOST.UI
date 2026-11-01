using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using CONCOST.UI.AttachedProperties;
using CONCOST.UI.Icons;

namespace CONCOST.UI.Controls;

/// <summary>What a project browser node stands for.</summary>
public enum ProjectNodeKind
{
    Project,
    Folder,
    View,
    Sheet,
    Family,
    FamilySymbol,
    Schedule,
    Level
}

/// <summary>How the tree is grouped.</summary>
public enum ProjectGroupMode
{
    None,
    ByKind
}

/// <summary>How siblings are ordered.</summary>
public enum ProjectSortMode
{
    NameAscending,
    NameDescending,
    Kind
}

/// <summary>What the browser is standing on.</summary>
public enum ProjectBrowserVariant
{
    /// <summary>No plate at all: the host's palette already draws one.</summary>
    Seamless,

    /// <summary>One outline and a rounded corner, for a browser dropped straight onto a page.</summary>
    Card,

    /// <summary>A card with a shadow, for a browser floating over a drawing.</summary>
    Floating
}

/// <summary>One node of the project tree.</summary>
public class ProjectTreeNode
{
    public ProjectTreeNode(string name, ProjectNodeKind kind)
    {
        Name = name ?? string.Empty;
        Kind = kind;
    }

    /// <summary>Caption shown in the tree.</summary>
    public string Name { get; set; }

    /// <summary>What the node is; drives the icon and the context menu.</summary>
    public ProjectNodeKind Kind { get; set; }

    /// <summary>Shows the unsaved marker next to the name.</summary>
    public bool IsModified { get; set; }

    /// <summary>The model object behind the node, when there is one.</summary>
    public object? Tag { get; set; }

    /// <summary>Child nodes.</summary>
    public ObservableCollection<ProjectTreeNode> Children { get; } = new();

    /// <summary>Parent node, or <c>null</c> for a root.</summary>
    public ProjectTreeNode? Parent { get; internal set; }

    /// <summary>Adds a child and returns it.</summary>
    public ProjectTreeNode Add(ProjectTreeNode child)
    {
        if (child is null) return this;

        child.Parent = this;
        Children.Add(child);

        return child;
    }

    /// <summary>Adds a child by name and kind and returns it.</summary>
    public ProjectTreeNode Add(string name, ProjectNodeKind kind)
    {
        var child = new ProjectTreeNode(name, kind);
        return Add(child);
    }

    /// <summary>The node and all of its descendants, depth first.</summary>
    public IEnumerable<ProjectTreeNode> Flatten()
    {
        yield return this;

        foreach (var child in Children)
        {
            foreach (var descendant in child.Flatten()) yield return descendant;
        }
    }

    /// <inheritdoc />
    public override string ToString() => $"{Kind}: {Name}";
}

/// <summary>Carries the node an action was requested on.</summary>
public class ProjectNodeEventArgs : EventArgs
{
    public ProjectNodeEventArgs(ProjectTreeNode node) => Node = node;

    /// <summary>The node the user acted on.</summary>
    public ProjectTreeNode Node { get; }
}

/// <summary>
/// The project browser: a header of two short rows, a tree whose nodes carry their own context menu,
/// and a footer that counts what the filter left.
/// </summary>
/// <remarks>
/// <para>
/// The tree is built from <see cref="ProjectTreeNode"/> objects rather than data-bound to a view model:
/// each node needs a kind-specific icon and a kind-specific menu, and rebuilding the items on a refresh
/// is cheaper than a template selector plus an item template plus a menu provider.
/// </para>
/// <para>
/// A filter keeps a branch alive when any descendant matches. Dropping the branch instead would hide
/// the only path to a match, which is the one thing a filtered tree must never do.
/// </para>
/// <para>
/// <b>One plate, no outlined chips.</b> The browser draws at most one border — the plate its host did
/// not draw — and everything inside it is flat: the four tools are ghost buttons, the two pickers are
/// filled rather than outlined, and grouping is a switch that says <em>on</em> with the soft accent.
/// A browser is a palette, not a toolbar, so the alternative was a table of 26 px cells with two
/// outlined dropdowns in it, which is what a palette looks like when it is assembled from stock parts.
/// </para>
/// <para>
/// Every caption is a property (<see cref="SearchPlaceholder"/>, <see cref="AllKindsCaption"/>,
/// <see cref="SortCaptions"/>, <see cref="EmptyText"/>, <see cref="CountCaption"/>) because a
/// palette that can only speak English is a palette half the world cannot use.
/// </para>
/// </remarks>
public class ProjectBrowser : Grid
{
    private const string DefaultSearchPlaceholder = "Search the project";
    private const string DefaultAllKindsCaption = "All kinds";
    private const string DefaultEmptyText = "Nothing matches this search.";
    private const string DefaultCountCaption = "{0} items";

    private static readonly string[] DefaultSortCaptions = { "Name A–Z", "Name Z–A", "By kind" };

    private readonly Border _frame;
    private readonly Grid _shell;
    private readonly StackPanel _header;
    private readonly Border _headerRule;
    private readonly StackPanel _tools;
    private readonly List<Button> _toolButtons = new();
    private readonly Border _toolRule;
    private readonly Border _footer;
    private readonly TextBlock _count;
    private readonly TextBlock _empty;
    private readonly TreeView _tree;
    private readonly SearchBox _search;
    private readonly ComboBox _kindFilter;
    private readonly ComboBox _sort;
    private readonly ToggleButton _group;

    private bool _syncing;
    private double _itemIcon = 13;
    private ProjectGroupMode _groupMode = ProjectGroupMode.None;
    private ProjectSortMode _sortMode = ProjectSortMode.NameAscending;

    public ProjectBrowser()
    {
        _frame = new Border();

        _shell = new Grid();
        _shell.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        _shell.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        _shell.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        _shell.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        // The control owns exactly one surface, and Seamless takes even that away: a palette that
        // draws a plate inside its host's plate is the same mistake as a status bar in a frame.
        _frame.Child = _shell;
        Children.Add(_frame);

        // ---------------------------------------------------------------- search and kind filter
        _header = new StackPanel { Margin = new Thickness(6, 6, 6, 4) };

        var filterRow = new Grid();
        filterRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        filterRow.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        _search = new SearchBox { Placeholder = DefaultSearchPlaceholder, FontSize = 12 };
        _search.TextChanged += (_, _) =>
        {
            if (_syncing) return;

            Rebuild();
        };

        SetColumn(_search, 0);
        filterRow.Children.Add(_search);

        // Filled, not outlined: the plate is the only thing in the browser allowed an edge.
        _kindFilter = new ComboBox
        {
            Width = 96,
            Margin = new Thickness(4, 0, 0, 0),
            ToolTip = "Show only one kind of node"
        };
        _kindFilter.SetResourceReference(StyleProperty, "CONCOST.ComboBox.Filled");
        _kindFilter.SelectionChanged += (_, _) =>
        {
            if (_syncing) return;

            Rebuild();
        };

        SetColumn(_kindFilter, 1);
        filterRow.Children.Add(_kindFilter);

        _header.Children.Add(filterRow);

        // ---------------------------------------------------------------- tools
        var toolRow = new Grid { Margin = new Thickness(0, 4, 0, 0) };
        toolRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        toolRow.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        _tools = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };

        // Chevrons, not the circled plus/minus: at 13 px the disc reads as a grey blob beside a
        // stroke-drawn refresh glyph, and a palette cannot afford two weights on one row.
        _tools.Children.Add(ToolButton(IconKind.ChevronDoubleDown, "Expand all", ExpandAll));
        _tools.Children.Add(ToolButton(IconKind.ChevronDoubleUp, "Collapse all", CollapseAll));
        _tools.Children.Add(ToolButton(IconKind.Refresh, "Refresh", Refresh));

        _toolRule = Rule(vertical: true);
        _toolRule.Margin = new Thickness(5, 4, 5, 4);
        _tools.Children.Add(_toolRule);

        // Status without a caption: the glyph carries the switch and the checked state is the soft
        // accent, so a grouped tree does not put a block of orange in a 300 px palette.
        _group = new ToggleButton { MinWidth = 0, MinHeight = 0, ToolTip = "Group by kind" };
        _group.SetResourceReference(StyleProperty, "CONCOST.ToggleButton.Status");
        IconElement.SetKind(_group, IconKind.GridSquare);
        _group.Click += (_, _) => GroupMode = _group.IsChecked == true ? ProjectGroupMode.ByKind : ProjectGroupMode.None;

        _tools.Children.Add(_group);

        SetColumn(_tools, 0);
        toolRow.Children.Add(_tools);

        _sort = new ComboBox { Width = 96, ToolTip = "Sort order" };
        _sort.SetResourceReference(StyleProperty, "CONCOST.ComboBox.Filled");
        _sort.SelectionChanged += (_, _) =>
        {
            if (_syncing) return;

            SortMode = (ProjectSortMode)Math.Max(0, _sort.SelectedIndex);
        };

        SetColumn(_sort, 1);
        toolRow.Children.Add(_sort);

        _header.Children.Add(toolRow);

        SetRow(_header, 0);
        _shell.Children.Add(_header);

        _headerRule = Rule(vertical: false);
        SetRow(_headerRule, 1);
        _shell.Children.Add(_headerRule);

        // ---------------------------------------------------------------- tree
        var treeHost = new Grid();

        _tree = new TreeView { BorderThickness = new Thickness(0), Margin = new Thickness(2, 2, 2, 2) };
        _tree.SelectedItemChanged += OnTreeSelectionChanged;
        _tree.AddHandler(Control.MouseDoubleClickEvent, new System.Windows.Input.MouseButtonEventHandler(OnTreeDoubleClick), true);
        treeHost.Children.Add(_tree);

        // A filtered tree that shows nothing looks broken; it has to say so.
        _empty = new TextBlock
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            TextAlignment = TextAlignment.Center,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(18),
            Visibility = Visibility.Collapsed
        };
        _empty.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextTertiary");
        treeHost.Children.Add(_empty);

        SetRow(treeHost, 2);
        _shell.Children.Add(treeHost);

        // ---------------------------------------------------------------- footer
        _footer = new Border { BorderThickness = new Thickness(0, 1, 0, 0), Padding = new Thickness(8, 3, 8, 3) };
        _footer.SetResourceReference(Border.BorderBrushProperty, "CONCOST.Brush.Border");

        _count = new TextBlock { FontSize = 11 };
        _count.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");
        _footer.Child = _count;

        SetRow(_footer, 3);
        _shell.Children.Add(_footer);

        ApplyChrome();
        ApplyControlHeight();
        ApplyGroupVisibility();
        ApplyCaptions();
    }

    // ---------------------------------------------------------------- extended properties

    /// <summary>What the browser is standing on; <c>Seamless</c> draws no plate at all.</summary>
    public static readonly DependencyProperty VariantProperty = DependencyProperty.Register(
        nameof(Variant), typeof(ProjectBrowserVariant), typeof(ProjectBrowser),
        new FrameworkPropertyMetadata(ProjectBrowserVariant.Seamless, OnChromeChanged));

    /// <summary>Corner of the plate.</summary>
    public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register(
        nameof(CornerRadius), typeof(CornerRadius), typeof(ProjectBrowser),
        new FrameworkPropertyMetadata(new CornerRadius(8), OnChromeChanged));

    /// <summary>Shadow level of the plate, 0-4; <c>Seamless</c> ignores it.</summary>
    public static readonly DependencyProperty ElevationProperty = DependencyProperty.Register(
        nameof(Elevation), typeof(int), typeof(ProjectBrowser),
        new FrameworkPropertyMetadata(0, OnChromeChanged));

    /// <summary>Height of the search box, both pickers and every tool button.</summary>
    public static readonly DependencyProperty ControlHeightProperty = DependencyProperty.Register(
        nameof(ControlHeight), typeof(double), typeof(ProjectBrowser),
        new FrameworkPropertyMetadata(24.0, FrameworkPropertyMetadataOptions.AffectsMeasure, OnControlHeightChanged));

    /// <summary>Size of the tool glyphs and of the node icons.</summary>
    public static readonly DependencyProperty IconSizeProperty = DependencyProperty.Register(
        nameof(IconSize), typeof(double), typeof(ProjectBrowser),
        new FrameworkPropertyMetadata(13.0, FrameworkPropertyMetadataOptions.AffectsMeasure, OnControlHeightChanged));

    /// <summary>Shows the search field.</summary>
    public static readonly DependencyProperty ShowSearchProperty = DependencyProperty.Register(
        nameof(ShowSearch), typeof(bool), typeof(ProjectBrowser),
        new FrameworkPropertyMetadata(true, OnGroupVisibilityChanged));

    /// <summary>Shows the kind filter beside the search.</summary>
    public static readonly DependencyProperty ShowKindFilterProperty = DependencyProperty.Register(
        nameof(ShowKindFilter), typeof(bool), typeof(ProjectBrowser),
        new FrameworkPropertyMetadata(true, OnGroupVisibilityChanged));

    /// <summary>Shows the sort picker in the tool row.</summary>
    public static readonly DependencyProperty ShowSortProperty = DependencyProperty.Register(
        nameof(ShowSort), typeof(bool), typeof(ProjectBrowser),
        new FrameworkPropertyMetadata(true, OnGroupVisibilityChanged));

    /// <summary>Shows the group-by-kind switch.</summary>
    public static readonly DependencyProperty ShowGroupProperty = DependencyProperty.Register(
        nameof(ShowGroup), typeof(bool), typeof(ProjectBrowser),
        new FrameworkPropertyMetadata(true, OnGroupVisibilityChanged));

    /// <summary>Shows the expand / collapse / refresh tools and the group switch.</summary>
    public static readonly DependencyProperty ShowToolbarProperty = DependencyProperty.Register(
        nameof(ShowToolbar), typeof(bool), typeof(ProjectBrowser),
        new FrameworkPropertyMetadata(true, OnGroupVisibilityChanged));

    /// <summary>Shows the footer count; the footer takes its rule with it.</summary>
    public static readonly DependencyProperty ShowCountProperty = DependencyProperty.Register(
        nameof(ShowCount), typeof(bool), typeof(ProjectBrowser),
        new FrameworkPropertyMetadata(true, OnGroupVisibilityChanged));

    /// <summary>Placeholder of the search field.</summary>
    public static readonly DependencyProperty SearchPlaceholderProperty = DependencyProperty.Register(
        nameof(SearchPlaceholder), typeof(string), typeof(ProjectBrowser),
        new FrameworkPropertyMetadata(DefaultSearchPlaceholder, OnCaptionsChanged));

    /// <summary>Caption of the first entry of the kind filter.</summary>
    public static readonly DependencyProperty AllKindsCaptionProperty = DependencyProperty.Register(
        nameof(AllKindsCaption), typeof(string), typeof(ProjectBrowser),
        new FrameworkPropertyMetadata(DefaultAllKindsCaption, OnCaptionsChanged));

    /// <summary>The three sort captions, in enum order; a short or missing list keeps the defaults.</summary>
    public static readonly DependencyProperty SortCaptionsProperty = DependencyProperty.Register(
        nameof(SortCaptions), typeof(IEnumerable<string>), typeof(ProjectBrowser),
        new FrameworkPropertyMetadata(null, OnCaptionsChanged));

    /// <summary>What the tree says when the filter leaves nothing.</summary>
    public static readonly DependencyProperty EmptyTextProperty = DependencyProperty.Register(
        nameof(EmptyText), typeof(string), typeof(ProjectBrowser),
        new FrameworkPropertyMetadata(DefaultEmptyText, OnCaptionsChanged));

    /// <summary>Footer format, with <c>{0}</c> for the number of visible nodes.</summary>
    public static readonly DependencyProperty CountCaptionProperty = DependencyProperty.Register(
        nameof(CountCaption), typeof(string), typeof(ProjectBrowser),
        new FrameworkPropertyMetadata(DefaultCountCaption, OnCaptionsChanged));


    /// <summary>What the browser is standing on; <c>Seamless</c> draws no plate at all.</summary>
    public ProjectBrowserVariant Variant
    {
        get => (ProjectBrowserVariant)GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }

    /// <summary>Corner of the plate.</summary>
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    /// <summary>Shadow level of the plate, 0-4; <c>Seamless</c> ignores it.</summary>
    public int Elevation
    {
        get => (int)GetValue(ElevationProperty);
        set => SetValue(ElevationProperty, value);
    }

    /// <summary>Height of the search box, both pickers and every tool button.</summary>
    public double ControlHeight
    {
        get => (double)GetValue(ControlHeightProperty);
        set => SetValue(ControlHeightProperty, value);
    }

    /// <summary>Size of the tool glyphs and of the node icons.</summary>
    public double IconSize
    {
        get => (double)GetValue(IconSizeProperty);
        set => SetValue(IconSizeProperty, value);
    }

    /// <summary>Shows the search field.</summary>
    public bool ShowSearch
    {
        get => (bool)GetValue(ShowSearchProperty);
        set => SetValue(ShowSearchProperty, value);
    }

    /// <summary>Shows the kind filter beside the search.</summary>
    public bool ShowKindFilter
    {
        get => (bool)GetValue(ShowKindFilterProperty);
        set => SetValue(ShowKindFilterProperty, value);
    }

    /// <summary>Shows the sort picker in the tool row.</summary>
    public bool ShowSort
    {
        get => (bool)GetValue(ShowSortProperty);
        set => SetValue(ShowSortProperty, value);
    }

    /// <summary>Shows the group-by-kind switch.</summary>
    public bool ShowGroup
    {
        get => (bool)GetValue(ShowGroupProperty);
        set => SetValue(ShowGroupProperty, value);
    }

    /// <summary>Shows the expand / collapse / refresh tools and the group switch.</summary>
    public bool ShowToolbar
    {
        get => (bool)GetValue(ShowToolbarProperty);
        set => SetValue(ShowToolbarProperty, value);
    }

    /// <summary>Shows the footer count; the footer takes its rule with it.</summary>
    public bool ShowCount
    {
        get => (bool)GetValue(ShowCountProperty);
        set => SetValue(ShowCountProperty, value);
    }

    /// <summary>Placeholder of the search field.</summary>
    public string SearchPlaceholder
    {
        get => (string)GetValue(SearchPlaceholderProperty);
        set => SetValue(SearchPlaceholderProperty, value);
    }

    /// <summary>Caption of the first entry of the kind filter.</summary>
    public string AllKindsCaption
    {
        get => (string)GetValue(AllKindsCaptionProperty);
        set => SetValue(AllKindsCaptionProperty, value);
    }

    /// <summary>The three sort captions, in enum order; a short or missing list keeps the defaults.</summary>
    public IEnumerable<string>? SortCaptions
    {
        get => (IEnumerable<string>?)GetValue(SortCaptionsProperty);
        set => SetValue(SortCaptionsProperty, value);
    }

    /// <summary>What the tree says when the filter leaves nothing.</summary>
    public string EmptyText
    {
        get => (string)GetValue(EmptyTextProperty);
        set => SetValue(EmptyTextProperty, value);
    }

    /// <summary>Footer format, with <c>{0}</c> for the number of visible nodes.</summary>
    public string CountCaption
    {
        get => (string)GetValue(CountCaptionProperty);
        set => SetValue(CountCaptionProperty, value);
    }

    /// <summary>The root nodes.</summary>
    public ObservableCollection<ProjectTreeNode> Nodes { get; } = new();

    /// <summary>The text in the search box.</summary>
    public string FilterText => _search.Text ?? string.Empty;

    /// <summary>The node that is currently selected.</summary>
    public ProjectTreeNode? SelectedNode { get; private set; }

    /// <summary>Grouping mode; setting it rebuilds the tree.</summary>
    public ProjectGroupMode GroupMode
    {
        get => _groupMode;
        set
        {
            if (_groupMode == value) return;

            _groupMode = value;

            if (_group is not null) _group.IsChecked = value == ProjectGroupMode.ByKind;

            Rebuild();
        }
    }

    /// <summary>Sorting mode; setting it rebuilds the tree.</summary>
    public ProjectSortMode SortMode
    {
        get => _sortMode;
        set
        {
            if (_sortMode == value) return;

            _sortMode = value;

            if (_sort is not null) _sort.SelectedIndex = (int)value;

            Rebuild();
        }
    }

    /// <summary>The nodes the tree is showing, after the search, the kind filter and grouping.</summary>
    public IReadOnlyList<ProjectTreeNode> VisibleNodes =>
        AllItems().Select(item => item.Tag).OfType<ProjectTreeNode>().ToList();

    /// <summary>The kind filter, or <c>null</c> for every kind.</summary>
    public ProjectNodeKind? KindFilter =>
        _kindFilter.SelectedIndex <= 0 ? null : (ProjectNodeKind)(_kindFilter.SelectedIndex - 1);

    /// <summary>Raised when a node is activated — double-clicked or opened from its menu.</summary>
    public event EventHandler<ProjectNodeEventArgs>? OpenRequested;

    /// <summary>Raised from the "Rename" menu item.</summary>
    public event EventHandler<ProjectNodeEventArgs>? RenameRequested;

    /// <summary>Raised from the "Delete" menu item.</summary>
    public event EventHandler<ProjectNodeEventArgs>? DeleteRequested;

    /// <summary>Raised from the "Duplicate" menu item.</summary>
    public event EventHandler<ProjectNodeEventArgs>? DuplicateRequested;

    /// <summary>Raised from the "Properties" menu item.</summary>
    public event EventHandler<ProjectNodeEventArgs>? PropertiesRequested;

    /// <summary>Raised when the selected node changes.</summary>
    public event EventHandler? SelectionChanged;

    /// <summary>Adds a root node and refreshes the tree.</summary>
    public ProjectTreeNode AddRoot(string name, ProjectNodeKind kind)
    {
        var node = new ProjectTreeNode(name, kind);

        Nodes.Add(node);
        Rebuild();

        return node;
    }

    /// <summary>Sets the search text, as if the user typed it.</summary>
    public void SetSearchText(string? text)
    {
        _search.Text = text ?? string.Empty;
        Rebuild();
    }

    /// <summary>Rebuilds the tree, honouring the search text, the kind filter, grouping and sorting.</summary>
    public void Refresh() => Rebuild();

    /// <summary>Expands every branch.</summary>
    public void ExpandAll()
    {
        foreach (var item in AllItems()) item.IsExpanded = true;

        ScrollToTop();
    }

    /// <summary>Scrolls the tree back to its first node.</summary>
    public void ScrollToTop()
    {
        // Expanding generates item containers as they come into view, and the tree ends up scrolled
        // to whichever container was generated last. A full expand is only of any use from the top.
        UpdateLayout();

        FindScrollViewer(_tree)?.ScrollToTop();
    }

    private static ScrollViewer? FindScrollViewer(DependencyObject root)
    {
        if (root is ScrollViewer viewer) return viewer;

        int count = VisualTreeHelper.GetChildrenCount(root);

        for (int index = 0; index < count; index++)
        {
            var found = FindScrollViewer(VisualTreeHelper.GetChild(root, index));

            if (found is not null) return found;
        }

        return null;
    }

    /// <summary>Collapses every branch.</summary>
    public void CollapseAll()
    {
        foreach (var item in AllItems()) item.IsExpanded = false;
    }

    /// <summary>Selects a node, e.g. after the host opened a view from elsewhere.</summary>
    public void Select(ProjectTreeNode? node)
    {
        SelectedNode = node;

        foreach (var item in AllItems())
        {
            if (ReferenceEquals(item.Tag, node)) item.IsSelected = true;
        }
    }

    /// <summary>Raises <see cref="OpenRequested"/> for a node, e.g. when Enter is pressed on it.</summary>
    public void Open(ProjectTreeNode node)
    {
        if (node is null) return;

        SelectedNode = node;

        OpenRequested?.Invoke(this, new ProjectNodeEventArgs(node));
    }

    private static void OnChromeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ProjectBrowser browser) browser.ApplyChrome();
    }

    private static void OnControlHeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ProjectBrowser browser) browser.ApplyControlHeight();
    }

    private static void OnGroupVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ProjectBrowser browser) browser.ApplyGroupVisibility();
    }

    private static void OnCaptionsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ProjectBrowser browser) browser.ApplyCaptions();
    }

    /// <summary>Dresses the plate. Only this border is allowed to draw an edge.</summary>
    private void ApplyChrome()
    {
        _frame.CornerRadius = CornerRadius;

        if (Variant == ProjectBrowserVariant.Seamless)
        {
            // No plate at all: the palette the browser is docked in has already drawn one.
            _frame.SetValue(Border.BackgroundProperty, Brushes.Transparent);
            _frame.BorderThickness = new Thickness(0);
            VisualElement.SetElevation(_frame, 0);

            return;
        }

        _frame.SetResourceReference(Border.BackgroundProperty, "CONCOST.Brush.Surface");
        _frame.SetResourceReference(Border.BorderBrushProperty, "CONCOST.Brush.Border");
        _frame.BorderThickness = new Thickness(1);
        VisualElement.SetElevation(_frame, Variant == ProjectBrowserVariant.Floating ? Math.Max(1, Elevation) : Elevation);
    }

    private void ApplyControlHeight()
    {
        double height = double.IsNaN(ControlHeight) || ControlHeight < 18 ? 18 : ControlHeight;
        double icon = double.IsNaN(IconSize) || IconSize < 8 ? 8 : IconSize;
        double font = Math.Max(9.5, Math.Round(height * 0.42));

        // The search box asks for 28 px and both pickers for 34; a minimum wins over an explicit
        // Height, so each one is zeroed before the browser decides for itself.
        _search.MinHeight = 0;
        _search.Height = height;
        _search.FontSize = font;

        foreach (var combo in new[] { _kindFilter, _sort })
        {
            combo.MinHeight = 0;
            combo.Height = height;
            combo.FontSize = font;
        }

        foreach (var button in _toolButtons)
        {
            button.MinWidth = 0;
            button.MinHeight = 0;
            button.Width = height;
            button.Height = height;

            IconElement.SetIconSize(button, icon);
        }

        _group.MinWidth = 0;
        _group.MinHeight = 0;
        _group.Width = height;
        _group.Height = height;
        IconElement.SetIconSize(_group, icon);

        _itemIcon = icon;
        _tree.FontSize = font + 1;

        // The node headers bake the icon size and inherit the tree's font, so the tree is rebuilt
        // once with the new metrics instead of keeping a caption set in the old size.
        Rebuild();
    }

    private void ApplyGroupVisibility()
    {
        _search.Visibility = ShowSearch ? Visibility.Visible : Visibility.Collapsed;
        _kindFilter.Visibility = ShowKindFilter ? Visibility.Visible : Visibility.Collapsed;
        _sort.Visibility = ShowSort ? Visibility.Visible : Visibility.Collapsed;
        _group.Visibility = ShowGroup ? Visibility.Visible : Visibility.Collapsed;

        foreach (var button in _toolButtons)
        {
            button.Visibility = ShowToolbar ? Visibility.Visible : Visibility.Collapsed;
        }

        _toolRule.Visibility = ShowToolbar && ShowGroup ? Visibility.Visible : Visibility.Collapsed;
        _tools.Visibility = ShowToolbar || ShowGroup ? Visibility.Visible : Visibility.Collapsed;

        // The header is two rows and a rule; with nothing left in it they all leave together, so the
        // tree starts at the top edge instead of under a band of empty space.
        bool header = ShowSearch || ShowKindFilter || ShowToolbar || ShowSort || ShowGroup;

        _header.Visibility = header ? Visibility.Visible : Visibility.Collapsed;
        _headerRule.Visibility = header ? Visibility.Visible : Visibility.Collapsed;
        _footer.Visibility = ShowCount ? Visibility.Visible : Visibility.Collapsed;
    }

    private void ApplyCaptions()
    {
        _search.Placeholder = string.IsNullOrWhiteSpace(SearchPlaceholder) ? DefaultSearchPlaceholder : SearchPlaceholder;

        var kinds = new List<string>
        {
            string.IsNullOrWhiteSpace(AllKindsCaption) ? DefaultAllKindsCaption : AllKindsCaption
        };

        kinds.AddRange(Enum.GetNames(typeof(ProjectNodeKind)));

        var sorts = SortCaptions?.Where(caption => !string.IsNullOrWhiteSpace(caption)).ToList() ?? new List<string>();

        while (sorts.Count < DefaultSortCaptions.Length) sorts.Add(DefaultSortCaptions[sorts.Count]);

        _syncing = true;

        try
        {
            int kind = Math.Max(0, _kindFilter.SelectedIndex);
            _kindFilter.ItemsSource = kinds;
            _kindFilter.SelectedIndex = Math.Min(kind, kinds.Count - 1);

            int sort = Math.Max(0, _sort.SelectedIndex);
            _sort.ItemsSource = sorts.Take(DefaultSortCaptions.Length).ToList();
            _sort.SelectedIndex = Math.Min(sort, DefaultSortCaptions.Length - 1);
        }
        finally
        {
            _syncing = false;
        }

        RefreshFooter();
    }

    private void RefreshFooter()
    {
        int count = AllItems().Count(item => item.Tag is ProjectTreeNode);
        string caption = string.IsNullOrWhiteSpace(CountCaption) ? DefaultCountCaption : CountCaption;

        _count.Text = caption.Contains("{0}")
            ? string.Format(CultureInfo.CurrentCulture, caption, count)
            : caption;

        _empty.Text = EmptyText ?? string.Empty;
        _empty.Visibility = _tree.Items.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    private void Rebuild()
    {
        _syncing = true;

        try
        {
            var expanded = AllItems().Where(item => item.IsExpanded).Select(item => item.Tag).OfType<ProjectTreeNode>().ToHashSet();
            bool searching = FilterText.Length > 0;

            _tree.Items.Clear();

            if (GroupMode == ProjectGroupMode.ByKind)
            {
                // Grouping is a flat presentation: the groups sit at the root and the nodes keep their
                // own children, so a view inside a family folder still expands as it did.
                foreach (var group in Nodes.GroupBy(node => node.Kind).OrderBy(group => group.Key))
                {
                    var groupItem = new TreeViewItem
                    {
                        Header = CreateHeader(Enum.GetName(typeof(ProjectNodeKind), group.Key) + "s", group.Key, false),
                        IsExpanded = true,
                        Tag = null
                    };

                    foreach (var node in Sorted(group))
                    {
                        var item = BuildNode(node, expanded, searching);
                        if (item is not null) groupItem.Items.Add(item);
                    }

                    if (groupItem.Items.Count > 0) _tree.Items.Add(groupItem);
                }
            }
            else
            {
                foreach (var node in Sorted(Nodes))
                {
                    var item = BuildNode(node, expanded, searching);
                    if (item is not null) _tree.Items.Add(item);
                }
            }
        }
        finally
        {
            _syncing = false;
        }

        RefreshFooter();
    }

    /// <summary>
    /// Builds the item for one node, or <c>null</c> when the node is filtered out. A node survives when
    /// it matches or when any descendant does, so the path to a match is never dropped.
    /// </summary>
    private TreeViewItem? BuildNode(ProjectTreeNode node, HashSet<ProjectTreeNode> expanded, bool searching)
    {
        var children = new List<TreeViewItem>();

        foreach (var child in Sorted(node.Children))
        {
            var childItem = BuildNode(child, expanded, searching);
            if (childItem is not null) children.Add(childItem);
        }

        if (!Matches(node) && children.Count == 0) return null;

        var item = new TreeViewItem
        {
            Header = CreateHeader(node.Name, node.Kind, node.IsModified),
            Tag = node,
            ContextMenu = CreateMenu(node),
            IsExpanded = expanded.Contains(node) || (searching && children.Count > 0)
        };

        foreach (var child in children) item.Items.Add(child);

        return item;
    }

    private IEnumerable<ProjectTreeNode> Sorted(IEnumerable<ProjectTreeNode> nodes) => SortMode switch
    {
        ProjectSortMode.NameDescending => nodes.OrderByDescending(node => node.Name, StringComparer.CurrentCultureIgnoreCase),
        ProjectSortMode.Kind => nodes.OrderBy(node => node.Kind).ThenBy(node => node.Name, StringComparer.CurrentCultureIgnoreCase),
        _ => nodes.OrderBy(node => node.Name, StringComparer.CurrentCultureIgnoreCase)
    };

    private bool Matches(ProjectTreeNode node)
    {
        if (KindFilter is { } kind && node.Kind != kind) return false;

        string filter = FilterText;

        return filter.Length == 0 || node.Name.IndexOf(filter, StringComparison.CurrentCultureIgnoreCase) >= 0;
    }

    private FrameworkElement CreateHeader(string caption, ProjectNodeKind kind, bool isModified)
    {
        var panel = new StackPanel { Orientation = Orientation.Horizontal };

        var icon = new Icon
        {
            Kind = IconFor(kind),
            Width = _itemIcon,
            Height = _itemIcon,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 5, 0)
        };

        icon.SetResourceReference(Control.ForegroundProperty, kind == ProjectNodeKind.Project ? "CONCOST.Brush.Primary" : "CONCOST.Brush.TextSecondary");
        panel.Children.Add(icon);

        var text = new TextBlock { Text = caption, VerticalAlignment = VerticalAlignment.Center };
        text.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextPrimary");
        panel.Children.Add(text);

        if (isModified)
        {
            var dot = new System.Windows.Shapes.Ellipse { Width = 5, Height = 5, Margin = new Thickness(5, 0, 0, 0), VerticalAlignment = VerticalAlignment.Center };
            dot.SetResourceReference(System.Windows.Shapes.Shape.FillProperty, "CONCOST.Brush.Primary");
            panel.Children.Add(dot);
        }

        return panel;
    }

    private static IconKind IconFor(ProjectNodeKind kind) => kind switch
    {
        ProjectNodeKind.Project => IconKind.FolderOpen,

        // The open flap, not the closed block: at 13 px the closed folder is a solid rounded square
        // and it outweighed every outline glyph around it.
        ProjectNodeKind.Folder => IconKind.FolderOpen,

        // Outline rather than the filled isometric solid: at 13 px a solid glyph in a tree reads as
        // a dark square, and four views in a row become four squares.
        ProjectNodeKind.View => IconKind.ViewTree,
        ProjectNodeKind.Sheet => IconKind.FileText,
        ProjectNodeKind.Family => IconKind.Cube,
        ProjectNodeKind.FamilySymbol => IconKind.GridSquare,
        ProjectNodeKind.Schedule => IconKind.Table,
        ProjectNodeKind.Level => IconKind.LevelTool,
        _ => IconKind.FileText
    };

    private ContextMenu CreateMenu(ProjectTreeNode node)
    {
        var menu = new ContextMenu();

        void Item(string caption, Action action)
        {
            var item = new MenuItem { Header = caption };
            item.Click += (_, _) => action();
            menu.Items.Add(item);
        }

        if (node.Kind is ProjectNodeKind.View or ProjectNodeKind.Sheet or ProjectNodeKind.Schedule or ProjectNodeKind.FamilySymbol)
        {
            Item("Open", () => OpenRequested?.Invoke(this, new ProjectNodeEventArgs(node)));
            menu.Items.Add(new Separator());
        }

        Item("Rename", () => RenameRequested?.Invoke(this, new ProjectNodeEventArgs(node)));
        Item("Duplicate", () => DuplicateRequested?.Invoke(this, new ProjectNodeEventArgs(node)));
        Item("Delete", () => DeleteRequested?.Invoke(this, new ProjectNodeEventArgs(node)));
        menu.Items.Add(new Separator());
        Item("Properties", () => PropertiesRequested?.Invoke(this, new ProjectNodeEventArgs(node)));

        return menu;
    }

    private Button ToolButton(IconKind kind, string tooltip, Action action)
    {
        // Ghost, not Icon: an outlined chip inside the plate is a second edge on a palette that has
        // no room for two, and four of them read as a table of cells.
        var button = new Button { MinWidth = 0, MinHeight = 0, Margin = new Thickness(1, 0, 1, 0), ToolTip = tooltip };

        button.SetResourceReference(StyleProperty, "CONCOST.Button.Icon.Ghost");
        IconElement.SetKind(button, kind);

        button.Click += (_, _) => action();

        _toolButtons.Add(button);

        return button;
    }

    /// <summary>A one-pixel rule that takes its colour from the theme rather than from a fixed grey.</summary>
    private static Border Rule(bool vertical)
    {
        var rule = new Border { SnapsToDevicePixels = true };

        if (vertical)
        {
            rule.Width = 1;
            rule.VerticalAlignment = VerticalAlignment.Stretch;
        }
        else
        {
            rule.Height = 1;
            rule.HorizontalAlignment = HorizontalAlignment.Stretch;
        }

        rule.SetResourceReference(Border.BackgroundProperty, "CONCOST.Brush.Border");

        return rule;
    }

    private IEnumerable<TreeViewItem> AllItems()
    {
        var stack = new Stack<TreeViewItem>(_tree.Items.OfType<TreeViewItem>());

        while (stack.Count > 0)
        {
            var item = stack.Pop();

            yield return item;

            foreach (var child in item.Items.OfType<TreeViewItem>()) stack.Push(child);
        }
    }

    private void OnTreeSelectionChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        SelectedNode = (e.NewValue as TreeViewItem)?.Tag as ProjectTreeNode;
        SelectionChanged?.Invoke(this, EventArgs.Empty);
    }

    private void OnTreeDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        var source = e.OriginalSource as DependencyObject;
        var item = source is null ? null : FindItem(source);

        if (item?.Tag is not ProjectTreeNode node) return;

        SelectedNode = node;
        OpenRequested?.Invoke(this, new ProjectNodeEventArgs(node));
        e.Handled = true;
    }

    private static TreeViewItem? FindItem(DependencyObject source)
    {
        for (var current = source; current is not null; current = ParentOf(current))
        {
            if (current is TreeViewItem item) return item;
        }

        return null;
    }

    private static DependencyObject? ParentOf(DependencyObject element) =>
        element is Visual or System.Windows.Media.Media3D.Visual3D
            ? VisualTreeHelper.GetParent(element)
            : LogicalTreeHelper.GetParent(element);
}
