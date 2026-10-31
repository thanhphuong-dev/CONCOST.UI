using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

// CONCOST.UI.Controls.Canvas is a namespace in this library, so the panel needs a distinct alias.
using KeyTipCanvas = System.Windows.Controls.Canvas;

namespace CONCOST.UI.Controls.Ribbon;

/// <summary>
/// Container for a tabbed ribbon: a strip of <see cref="RibbonTabItem"/> headers on top and the
/// groups of the selected tab underneath. The tab strip is a real <see cref="ListBox"/> in the
/// template, so keyboard navigation, focus and selection visuals come for free.
/// </summary>
public class Ribbon : Control
{
    public static readonly DependencyProperty ItemsSourceProperty =
        DependencyProperty.Register(nameof(ItemsSource), typeof(IEnumerable), typeof(Ribbon),
            new FrameworkPropertyMetadata(null, OnItemsSourceChanged));

    public static readonly DependencyProperty SelectedIndexProperty =
        DependencyProperty.Register(nameof(SelectedIndex), typeof(int), typeof(Ribbon),
            new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedIndexChanged));

    private static readonly DependencyPropertyKey SelectedItemPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(SelectedItem), typeof(RibbonTabItem), typeof(Ribbon),
            new FrameworkPropertyMetadata(null));

    public static readonly DependencyProperty SelectedItemProperty = SelectedItemPropertyKey.DependencyProperty;

    public static readonly DependencyProperty IsMinimizedProperty =
        DependencyProperty.Register(nameof(IsMinimized), typeof(bool), typeof(Ribbon),
            new FrameworkPropertyMetadata(false));

    public static readonly DependencyProperty ApplicationMenuProperty =
        DependencyProperty.Register(nameof(ApplicationMenu), typeof(object), typeof(Ribbon),
            new FrameworkPropertyMetadata(null));

    public static readonly DependencyProperty ShowQuickAccessToolBarProperty =
        DependencyProperty.Register(nameof(ShowQuickAccessToolBar), typeof(bool), typeof(Ribbon),
            new FrameworkPropertyMetadata(true));

    public static readonly DependencyProperty QuickAccessPlacementProperty =
        DependencyProperty.Register(nameof(QuickAccessPlacement), typeof(RibbonQuickAccessPlacement), typeof(Ribbon),
            new FrameworkPropertyMetadata(RibbonQuickAccessPlacement.HeaderRight, OnQuickAccessPlacementChanged));

    public static readonly DependencyProperty MenuBarProperty =
        DependencyProperty.Register(nameof(MenuBar), typeof(object), typeof(Ribbon),
            new FrameworkPropertyMetadata(null, OnMenuBarChanged));

    public static readonly DependencyProperty MenuBarPlacementProperty =
        DependencyProperty.Register(nameof(MenuBarPlacement), typeof(RibbonMenuBarPlacement), typeof(Ribbon),
            new FrameworkPropertyMetadata(RibbonMenuBarPlacement.AboveRibbon, OnMenuBarPlacementChanged));

    public static readonly DependencyProperty ShowMenuBarProperty =
        DependencyProperty.Register(nameof(ShowMenuBar), typeof(bool), typeof(Ribbon),
            new FrameworkPropertyMetadata(true));

    public static readonly DependencyProperty ShowApplicationMenuProperty =
        DependencyProperty.Register(nameof(ShowApplicationMenu), typeof(bool), typeof(Ribbon),
            new FrameworkPropertyMetadata(true));

    public static readonly DependencyProperty ShowTabStripProperty =
        DependencyProperty.Register(nameof(ShowTabStrip), typeof(bool), typeof(Ribbon),
            new FrameworkPropertyMetadata(true));

    public static readonly DependencyProperty TabAlignmentProperty =
        DependencyProperty.Register(nameof(TabAlignment), typeof(RibbonTabAlignment), typeof(Ribbon),
            new FrameworkPropertyMetadata(RibbonTabAlignment.Left));

    public static readonly DependencyProperty SelectedTabIdProperty =
        DependencyProperty.Register(nameof(SelectedTabId), typeof(string), typeof(Ribbon),
            new FrameworkPropertyMetadata(string.Empty,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedTabIdChanged));

    public static readonly DependencyProperty IsGroupCollapseEnabledProperty =
        DependencyProperty.Register(nameof(IsGroupCollapseEnabled), typeof(bool), typeof(Ribbon),
            new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.Inherits,
                OnIsGroupCollapseEnabledChanged));

    public static readonly DependencyProperty IsKeyTipEnabledProperty =
        DependencyProperty.Register(nameof(IsKeyTipEnabled), typeof(bool), typeof(Ribbon),
            new FrameworkPropertyMetadata(true, OnIsKeyTipEnabledChanged));

    public static readonly DependencyProperty IsQuickAccessCustomizableProperty =
        DependencyProperty.Register(nameof(IsQuickAccessCustomizable), typeof(bool), typeof(Ribbon),
            new FrameworkPropertyMetadata(true, OnIsQuickAccessCustomizableChanged));

    public static readonly DependencyProperty ShowBorderProperty =
        DependencyProperty.Register(nameof(ShowBorder), typeof(bool), typeof(Ribbon),
            new FrameworkPropertyMetadata(true));

    public static readonly DependencyProperty EmptyContentProperty =
        DependencyProperty.Register(nameof(EmptyContent), typeof(object), typeof(Ribbon),
            new FrameworkPropertyMetadata(null));

    /// <summary>Opens the list of tabs from the “»” overflow button.</summary>
    public static readonly RoutedUICommand ShowTabListCommand =
        new("All tabs", nameof(ShowTabListCommand), typeof(Ribbon));

    private static readonly DependencyPropertyKey IsTabStripOverflowingPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(IsTabStripOverflowing), typeof(bool), typeof(Ribbon),
            new FrameworkPropertyMetadata(false));

    public static readonly DependencyProperty IsTabStripOverflowingProperty =
        IsTabStripOverflowingPropertyKey.DependencyProperty;

    private static readonly DependencyPropertyKey IsEmptyPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(IsEmpty), typeof(bool), typeof(Ribbon),
            new FrameworkPropertyMetadata(true));

    public static readonly DependencyProperty IsEmptyProperty = IsEmptyPropertyKey.DependencyProperty;

    /// <summary>
    /// Attached property that tells a <see cref="RibbonGroup"/> whether to show its header label.
    /// Set on the <see cref="Ribbon"/> and read by groups through the visual tree.
    /// </summary>
    public static readonly DependencyProperty ShowGroupLabelsProperty =
        DependencyProperty.RegisterAttached("ShowGroupLabels", typeof(bool), typeof(Ribbon),
            new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.Inherits, OnShowGroupLabelsChanged));

    private static void OnShowGroupLabelsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        // Inherited properties raise this on every descendant, so each group hears about it
        // directly instead of having to bind to an ancestor from inside its template.
        if (d is RibbonGroup group) group.ApplyShowGroupLabels((bool)e.NewValue);
    }

    public static bool GetShowGroupLabels(DependencyObject obj) => (bool)obj.GetValue(ShowGroupLabelsProperty);
    public static void SetShowGroupLabels(DependencyObject obj, bool value) => obj.SetValue(ShowGroupLabelsProperty, value);

    private static readonly DependencyPropertyKey VisibleTabsPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(VisibleTabs), typeof(IReadOnlyList<RibbonTabItem>), typeof(Ribbon),
            new FrameworkPropertyMetadata(null));

    public static readonly DependencyProperty VisibleTabsProperty = VisibleTabsPropertyKey.DependencyProperty;

    private readonly HashSet<RibbonTabItem> _watched = new();
    private readonly List<RibbonMenuItem> _quickAccessRows = new();
    private readonly List<RibbonItem> _pinned = new();
    private RibbonQuickAccessToolBar? _quickAccess;
    private ContentControl? _qatAbove;
    private ContentControl? _qatInline;
    private ContentControl? _qatBelow;
    private ContentControl? _menuBarAbove;
    private ContentControl? _menuBarBelow;
    private RibbonKeyTipController? _keyTips;
    private System.Windows.Controls.ScrollViewer? _tabScroll;
    private Window? _hostWindow;
    private bool _restoringPins;
    private bool _syncingTabId;

    static Ribbon()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(Ribbon),
            new FrameworkPropertyMetadata(typeof(Ribbon)));
    }

    public Ribbon()
    {
        Loaded += OnRibbonLoaded;
        Unloaded += OnRibbonUnloaded;

        // Generic.xaml has no code-behind, so the overflow button talks to the ribbon through a
        // routed command instead of a Click handler.
        CommandBindings.Add(new CommandBinding(ShowTabListCommand, OnShowTabList));
    }

    /// <summary>
    /// The <see cref="RibbonTabItem"/> instances shown by this ribbon. Entries may also be
    /// <see cref="RibbonContextualTabGroup"/> containers, which are expanded in place.
    /// </summary>
    public IEnumerable? ItemsSource
    {
        get => (IEnumerable?)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    /// <summary>Index of the active tab, counted over the visible tabs only.</summary>
    public int SelectedIndex
    {
        get => (int)GetValue(SelectedIndexProperty);
        set => SetValue(SelectedIndexProperty, value);
    }

    /// <summary>The active tab, kept in sync with <see cref="SelectedIndex"/>.</summary>
    public RibbonTabItem? SelectedItem
    {
        get => (RibbonTabItem?)GetValue(SelectedItemProperty);
        private set => SetValue(SelectedItemPropertyKey, value);
    }

    /// <summary>
    /// Collapses the content area so only the tab strip remains — the classic “show tabs only”
    /// ribbon state.
    /// </summary>
    public bool IsMinimized
    {
        get => (bool)GetValue(IsMinimizedProperty);
        set => SetValue(IsMinimizedProperty, value);
    }

    /// <summary>
    /// Optional <see cref="RibbonApplicationMenu"/> shown as the coloured File block at the
    /// top-left, before the quick-access strip.
    /// </summary>
    public object? ApplicationMenu
    {
        get => GetValue(ApplicationMenuProperty);
        set => SetValue(ApplicationMenuProperty, value);
    }

    /// <summary>Shows the quick-access strip above the tabs. On by default, as in Office.</summary>
    public bool ShowQuickAccessToolBar
    {
        get => (bool)GetValue(ShowQuickAccessToolBarProperty);
        set => SetValue(ShowQuickAccessToolBarProperty, value);
    }

    /// <summary>
    /// Where the quick-access strip lives: inline at the end of the tab row (default), on its own
    /// strip above the tabs, or on its own strip under the ribbon body. The strip is one control, so
    /// switching places moves it — the pinned commands and the customise menu come along.
    /// </summary>
    public RibbonQuickAccessPlacement QuickAccessPlacement
    {
        get => (RibbonQuickAccessPlacement)GetValue(QuickAccessPlacementProperty);
        set => SetValue(QuickAccessPlacementProperty, value);
    }

    /// <summary>
    /// Optional <see cref="RibbonMenuBar"/> shown inside the ribbon chrome. Placement is controlled
    /// by <see cref="MenuBarPlacement"/>.
    /// </summary>
    public object? MenuBar
    {
        get => GetValue(MenuBarProperty);
        set => SetValue(MenuBarProperty, value);
    }

    /// <summary>
    /// Where the menu bar lives: above the tab strip (default) or below the ribbon body.
    /// When the quick-access toolbar is also above the ribbon, the menu bar sits between the two.
    /// </summary>
    public RibbonMenuBarPlacement MenuBarPlacement
    {
        get => (RibbonMenuBarPlacement)GetValue(MenuBarPlacementProperty);
        set => SetValue(MenuBarPlacementProperty, value);
    }

    /// <summary>Shows the menu bar. On by default.</summary>
    public bool ShowMenuBar
    {
        get => (bool)GetValue(ShowMenuBarProperty);
        set => SetValue(ShowMenuBarProperty, value);
    }

    /// <summary>Shows the File block at the start of the top row. On by default.</summary>
    public bool ShowApplicationMenu
    {
        get => (bool)GetValue(ShowApplicationMenuProperty);
        set => SetValue(ShowApplicationMenuProperty, value);
    }

    /// <summary>Shows the tab strip. On by default — turn it off for a menu-bar-only shell.</summary>
    public bool ShowTabStrip
    {
        get => (bool)GetValue(ShowTabStripProperty);
        set => SetValue(ShowTabStripProperty, value);
    }

    /// <summary>Where the tabs sit inside their row.</summary>
    public RibbonTabAlignment TabAlignment
    {
        get => (RibbonTabAlignment)GetValue(TabAlignmentProperty);
        set => SetValue(TabAlignmentProperty, value);
    }

    /// <summary>
    /// The <see cref="RibbonTabItem.Id"/> of the active tab. Bind this instead of
    /// <see cref="SelectedIndex"/> when tabs can be reordered or added at runtime — an index would
    /// silently point at a different tab.
    /// </summary>
    public string SelectedTabId
    {
        get => (string)GetValue(SelectedTabIdProperty);
        set => SetValue(SelectedTabIdProperty, value);
    }

    /// <summary>
    /// When false the ribbon may still demote Large to Medium to Small, but it never merges a group
    /// into a single drop-down button. Use it when a group must stay readable at any width.
    /// </summary>
    public bool IsGroupCollapseEnabled
    {
        get => (bool)GetValue(IsGroupCollapseEnabledProperty);
        set => SetValue(IsGroupCollapseEnabledProperty, value);
    }

    /// <summary>
    /// The Alt key-tip gesture. Turn it off when the host owns the keyboard (Revit and AutoCAD both
    /// intercept Alt) and drives <see cref="ProcessKeyTipKey"/> itself.
    /// </summary>
    public bool IsKeyTipEnabled
    {
        get => (bool)GetValue(IsKeyTipEnabledProperty);
        set => SetValue(IsKeyTipEnabledProperty, value);
    }

    /// <summary>
    /// Lets the user open the quick-access customise panel. Pinned commands stay usable when false;
    /// only the chevron and the panel go away — the usual locked-down build.
    /// </summary>
    public bool IsQuickAccessCustomizable
    {
        get => (bool)GetValue(IsQuickAccessCustomizableProperty);
        set => SetValue(IsQuickAccessCustomizableProperty, value);
    }

    /// <summary>Draws the one-pixel outline around the ribbon. Off for a ribbon docked into a panel.</summary>
    public bool ShowBorder
    {
        get => (bool)GetValue(ShowBorderProperty);
        set => SetValue(ShowBorderProperty, value);
    }

    /// <summary>Shown in place of the tab content when there is no tab to show.</summary>
    public object? EmptyContent
    {
        get => GetValue(EmptyContentProperty);
        set => SetValue(EmptyContentProperty, value);
    }

    /// <summary>True when there is no tab to show — the <see cref="EmptyContent"/> state.</summary>
    public bool IsEmpty => (bool)GetValue(IsEmptyProperty);

    /// <summary>
    /// True when the tabs no longer fit and the strip can scroll. The template uses it to show the
    /// “»” button that lists every tab.
    /// </summary>
    public bool IsTabStripOverflowing => (bool)GetValue(IsTabStripOverflowingProperty);

    /// <summary>Tabs currently offered to the user, in strip order.</summary>
    public IReadOnlyList<RibbonTabItem> VisibleTabs =>        (IReadOnlyList<RibbonTabItem>?)GetValue(VisibleTabsProperty) ?? Array.Empty<RibbonTabItem>();

    /// <summary>Tabs in declaration order, contextual groups expanded.</summary>
    public IReadOnlyList<RibbonTabItem> Tabs => Expand(ItemsSource);

    /// <summary>Every command in every tab — the pool the quick-access toolbar customises from.</summary>
    public IReadOnlyList<RibbonItem> AllItems
    {
        get
        {
            var items = new List<RibbonItem>();

            foreach (RibbonTabItem tab in Tabs)
            {
                if (tab.Content is not IEnumerable groups) continue;

                foreach (object? entry in groups)
                {
                    if (entry is not RibbonGroup group) continue;
                    items.AddRange(group.Items.OfType<RibbonItem>());
                }
            }

            return items;
        }
    }

    /// <summary>
    /// The toolbar created by the template. Null until the template has been applied.
    /// </summary>
    public RibbonQuickAccessToolBar? QuickAccess => _quickAccess;

    /// <summary>True while the Alt key tips are on screen.</summary>
    public bool IsShowingKeyTips => _keyTips?.IsShowing == true;

    /// <summary>The tab strip built by the template, or null before the template is applied.</summary>
    internal ListBox? TabStrip { get; private set; }

    /// <summary>Shows the key tips as if Alt had been pressed.</summary>
    public void ShowKeyTips() => _keyTips?.Show();

    /// <summary>Dismisses the key tips without running anything.</summary>
    public void HideKeyTips() => _keyTips?.Hide();

    /// <summary>
    /// Feeds one key to an open key-tip session. Returns true when the session consumed it.
    /// <para>
    /// The ribbon already listens to Alt on its host window; this is the escape hatch for hosts
    /// that own their keyboard pipeline (custom input, remote control, a test harness).
    /// </para>
    /// </summary>
    public bool ProcessKeyTipKey(System.Windows.Input.Key key) => _keyTips?.HandleKey(key) == true;

    /// <summary>
    /// Adds <paramref name="item"/> to the quick-access strip. The ribbon remembers the pin even
    /// when the toolbar does not exist yet, so it survives a template being re-applied.
    /// </summary>
    public void PinToQuickAccess(RibbonItem item)
    {
        if (item == null) return;

        if (!_pinned.Contains(item)) _pinned.Add(item);
        _quickAccess?.Pin(item);
    }

    /// <summary>Removes <paramref name="item"/> from the quick-access strip.</summary>
    public void UnpinFromQuickAccess(RibbonItem item)
    {
        if (item == null) return;

        _pinned.Remove(item);
        _quickAccess?.Unpin(item);
    }

    /// <summary>True when <paramref name="item"/> is on the quick-access strip.</summary>
    public bool IsPinnedToQuickAccess(RibbonItem item) => RibbonQuickAccessToolBar.GetIsPinned(item);

    /// <summary>
    /// Snapshot of what a user expects to find again next time: the open tab, the minimised state,
    /// the pinned commands and the groups they folded away by hand.
    /// </summary>
    public RibbonState CaptureState()
    {
        var state = new RibbonState
        {
            IsMinimized = IsMinimized,
            SelectedTabId = SelectedItem == null ? null : TabKey(SelectedItem),
            QuickAccessPlacement = QuickAccessPlacement.ToString()
        };

        foreach (RibbonItem item in _pinned)
        {
            if (!string.IsNullOrEmpty(item.Id)) state.QuickAccessItemIds.Add(item.Id);
        }

        foreach (RibbonTabItem tab in Tabs)
        {
            foreach (RibbonGroup group in GroupsOf(tab))
            {
                if (group.IsUserCollapsed) state.CollapsedGroupKeys.Add(GroupKey(tab, group));
            }
        }

        return state;
    }

    /// <summary>
    /// Puts a captured state back. Unknown ids are ignored, so a saved state survives a ribbon that
    /// lost a tab or a command since it was written.
    /// </summary>
    public void ApplyState(RibbonState? state)
    {
        if (state == null) return;

        IsMinimized = state.IsMinimized;

        // A name from a newer build is ignored rather than throwing: the snapshot is not a contract.
        if (!string.IsNullOrEmpty(state.QuickAccessPlacement) &&
            Enum.TryParse(state.QuickAccessPlacement, out RibbonQuickAccessPlacement placement) &&
            Enum.IsDefined(typeof(RibbonQuickAccessPlacement), placement))
        {
            QuickAccessPlacement = placement;
        }

        if (!string.IsNullOrEmpty(state.SelectedTabId))
        {
            IReadOnlyList<RibbonTabItem> tabs = VisibleTabs;

            for (int i = 0; i < tabs.Count; i++)
            {
                if (!string.Equals(TabKey(tabs[i]), state.SelectedTabId, StringComparison.Ordinal)) continue;
                SelectedIndex = i;
                break;
            }
        }

        var wanted = new HashSet<string>(state.QuickAccessItemIds, StringComparer.Ordinal);

        foreach (RibbonItem item in _pinned.ToList())
        {
            if (string.IsNullOrEmpty(item.Id) || !wanted.Contains(item.Id)) UnpinFromQuickAccess(item);
        }

        foreach (RibbonItem item in AllItems)
        {
            if (string.IsNullOrEmpty(item.Id) || !wanted.Contains(item.Id)) continue;
            if (!IsPinnedToQuickAccess(item)) PinToQuickAccess(item);
        }

        var collapsed = new HashSet<string>(state.CollapsedGroupKeys, StringComparer.Ordinal);

        foreach (RibbonTabItem tab in Tabs)
        {
            foreach (RibbonGroup group in GroupsOf(tab))
            {
                group.IsUserCollapsed = collapsed.Contains(GroupKey(tab, group));
            }
        }
    }

    /// <summary>Identifier of a tab for state persistence: the explicit id, else the header.</summary>
    internal static string TabKey(RibbonTabItem tab) =>
        string.IsNullOrEmpty(tab.Id) ? tab.Header as string ?? string.Empty : tab.Id;

    /// <summary>Key of one group inside one tab, used by <see cref="RibbonState.CollapsedGroupKeys"/>.</summary>
    internal static string GroupKey(RibbonTabItem tab, RibbonGroup group) =>
        TabKey(tab) + ":" + (group.Header as string ?? string.Empty);

    /// <summary>Groups of a tab, in declaration order.</summary>
    internal static IEnumerable<RibbonGroup> GroupsOf(RibbonTabItem tab)
    {
        if (tab.Content is not IEnumerable groups) yield break;

        foreach (object? entry in groups)
        {
            if (entry is RibbonGroup group) yield return group;
        }
    }

    /// <summary>
    /// Recomputes <see cref="VisibleTabs"/> after a contextual group or a tab changed availability.
    /// </summary>
    public void RefreshTabs()
    {
        IReadOnlyList<RibbonTabItem> all = Tabs;
        Watch(all);

        List<RibbonTabItem> visible = all.Where(t => t.IsAvailable).ToList();
        SetValue(VisibleTabsPropertyKey, visible);
        SetValue(IsEmptyPropertyKey, visible.Count == 0);

        if (_quickAccess != null)
        {
            _quickAccess.PinnableItems = AllItems;
            RebuildQuickAccessMenu();
        }

        SyncSelection();
    }

    protected override System.Windows.Automation.Peers.AutomationPeer OnCreateAutomationPeer() =>
        new RibbonAutomationPeer(this);

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        // A re-applied template builds a new overlay: drop any badge left on the old one.
        _keyTips?.Hide();

        _keyTips = GetTemplateChild("PART_KeyTipLayer") is KeyTipCanvas layer
            ? new RibbonKeyTipController(this, layer)
            : null;

        TabStrip = GetTemplateChild("PART_Tabs") as ListBox;

        if (_tabScroll != null) _tabScroll.ScrollChanged -= OnTabScrollChanged;
        _tabScroll = GetTemplateChild("PART_TabScroll") as System.Windows.Controls.ScrollViewer;
        if (_tabScroll != null) _tabScroll.ScrollChanged += OnTabScrollChanged;
        SetValue(IsTabStripOverflowingPropertyKey, false);

        _quickAccess = GetTemplateChild("PART_QAT") as RibbonQuickAccessToolBar;
        if (_quickAccess == null) return;

        // Three slots, one strip: the placement property decides which slot holds it.
        _qatAbove = GetTemplateChild("PART_QatAbove") as ContentControl;
        _qatInline = GetTemplateChild("PART_QatInline") as ContentControl;
        _qatBelow = GetTemplateChild("PART_QatBelow") as ContentControl;
        PlaceQuickAccess();

        // MenuBar slots: above or below the ribbon body.
        _menuBarAbove = GetTemplateChild("PART_MenuBarAbove") as ContentControl;
        _menuBarBelow = GetTemplateChild("PART_MenuBarBelow") as ContentControl;
        PlaceMenuBar();

        _quickAccess.PinnableItems = AllItems;
        _quickAccess.IsCustomizable = IsQuickAccessCustomizable;
        _quickAccess.PinsChanged += OnQuickAccessPinsChanged;

        // A re-applied template builds a brand-new toolbar: put the pins back.
        _restoringPins = true;
        try
        {
            foreach (RibbonItem item in _pinned.ToList()) _quickAccess.Pin(item);
        }
        finally
        {
            _restoringPins = false;
        }

        RebuildQuickAccessMenu();
    }

    private void OnRibbonLoaded(object sender, RoutedEventArgs e)
    {
        _hostWindow = Window.GetWindow(this);
        if (_hostWindow == null) return;

        // Alt is a window-wide gesture in every ribbon app, so the host window is the place to
        // listen. The handlers are removed again on unload to keep the window free of the ribbon.
        _hostWindow.PreviewKeyDown += OnHostPreviewKeyDown;
        _hostWindow.PreviewMouseDown += OnHostPreviewMouseDown;
    }

    private void OnRibbonUnloaded(object sender, RoutedEventArgs e)
    {
        _keyTips?.Hide();

        if (_hostWindow == null) return;

        _hostWindow.PreviewKeyDown -= OnHostPreviewKeyDown;
        _hostWindow.PreviewMouseDown -= OnHostPreviewMouseDown;
        _hostWindow = null;
    }

    private void OnHostPreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (_keyTips == null || !IsKeyTipEnabled || !KeyTipService.GetIsEnabled(this)) return;

        if (_keyTips.IsShowing)
        {
            if (_keyTips.HandleKey(e.Key)) e.Handled = true;
            return;
        }

        // Bare Alt opens the session. Alt+F4, Alt+Tab and Alt+Space still reach the system.
        if (e.Key == System.Windows.Input.Key.System &&
            e.SystemKey is System.Windows.Input.Key.LeftAlt
                or System.Windows.Input.Key.RightAlt
                or System.Windows.Input.Key.None)
        {
            ShowKeyTips();
            e.Handled = true;
        }
    }

    private void OnHostPreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (_keyTips?.IsShowing == true) _keyTips.Hide();
    }

    private void OnTabScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        // Extent minus viewport is the part of the strip that is out of sight; a sub-pixel
        // rounding difference must not pop the overflow button in and out.
        bool overflowing = e.ExtentWidth - e.ViewportWidth > 0.5;
        if (overflowing == IsTabStripOverflowing) return;

        SetValue(IsTabStripOverflowingPropertyKey, overflowing);

        // The strip paints its contextual band itself, so it has to be told to repaint.
        FindTabPanel(TabStrip)?.InvalidateVisual();
    }

    private static RibbonTabStripPanel? FindTabPanel(DependencyObject? root)
    {
        if (root == null) return null;

        int count = System.Windows.Media.VisualTreeHelper.GetChildrenCount(root);

        for (int i = 0; i < count; i++)
        {
            DependencyObject child = System.Windows.Media.VisualTreeHelper.GetChild(root, i);
            if (child is RibbonTabStripPanel panel) return panel;

            RibbonTabStripPanel? nested = FindTabPanel(child);
            if (nested != null) return nested;
        }

        return null;
    }

    /// <summary>Lists every tab under the “»” button so a clipped strip stays reachable.</summary>
    private void OnShowTabList(object sender, ExecutedRoutedEventArgs e)
    {
        if (e.Source is not FrameworkElement anchor) return;

        var menu = new System.Windows.Controls.ContextMenu
        {
            PlacementTarget = anchor,
            Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom
        };

        IReadOnlyList<RibbonTabItem> tabs = VisibleTabs;

        for (int i = 0; i < tabs.Count; i++)
        {
            int index = i;
            var row = new System.Windows.Controls.MenuItem
            {
                Header = tabs[i].Header,
                IsCheckable = true,
                IsChecked = i == SelectedIndex
            };

            row.Click += (_, _) => SelectedIndex = index;
            menu.Items.Add(row);
        }

        menu.IsOpen = true;
    }

    private static void OnSelectedTabIdChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not Ribbon ribbon || ribbon._syncingTabId) return;

        string id = e.NewValue as string ?? string.Empty;
        if (id.Length == 0) return;

        IReadOnlyList<RibbonTabItem> tabs = ribbon.VisibleTabs;
        for (int i = 0; i < tabs.Count; i++)
        {
            if (!string.Equals(tabs[i].Id, id, StringComparison.Ordinal)) continue;

            ribbon.SelectedIndex = i;
            return;
        }
    }

    private static void OnIsGroupCollapseEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        // Switching the last rung off has to give the folded groups back right away, otherwise the
        // property looks like it did nothing until the window is resized.
        if (d is Ribbon ribbon && !(bool)e.NewValue) FindGroupStrip(ribbon)?.RestoreCollapsed();
    }

    private static RibbonGroupStrip? FindGroupStrip(DependencyObject? root)
    {
        if (root == null) return null;

        int count = System.Windows.Media.VisualTreeHelper.GetChildrenCount(root);

        for (int i = 0; i < count; i++)
        {
            DependencyObject child = System.Windows.Media.VisualTreeHelper.GetChild(root, i);
            if (child is RibbonGroupStrip strip) return strip;

            RibbonGroupStrip? nested = FindGroupStrip(child);
            if (nested != null) return nested;
        }

        return null;
    }

    private static void OnIsKeyTipEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not Ribbon ribbon) return;

        KeyTipService.SetIsEnabled(ribbon, (bool)e.NewValue);
        if (!(bool)e.NewValue) ribbon._keyTips?.Hide();
    }

    private static void OnIsQuickAccessCustomizableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Ribbon ribbon && ribbon._quickAccess != null)
            ribbon._quickAccess.IsCustomizable = (bool)e.NewValue;
    }

    private static void OnQuickAccessPlacementChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Ribbon ribbon) ribbon.PlaceQuickAccess();
    }

    /// <summary>
    /// Moves the one toolbar instance into the slot the current placement asks for. Setting
    /// <see cref="ContentControl.Content"/> to null detaches it from the slot that holds it, so the
    /// same instance can be handed to another slot without WPF complaining about two parents.
    /// </summary>
    private void PlaceQuickAccess()
    {
        if (_quickAccess == null) return;

        ContentControl? target = QuickAccessPlacement switch
        {
            RibbonQuickAccessPlacement.AboveRibbon => _qatAbove,
            RibbonQuickAccessPlacement.BelowRibbon => _qatBelow,
            _ => _qatInline
        };

        if (target == null || ReferenceEquals(target.Content, _quickAccess)) return;

        foreach (ContentControl? host in new[] { _qatAbove, _qatInline, _qatBelow })
        {
            if (host != null && ReferenceEquals(host.Content, _quickAccess)) host.Content = null;
        }

        target.Content = _quickAccess;
    }

    private static void OnMenuBarChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Ribbon ribbon) ribbon.PlaceMenuBar();
    }

    private static void OnMenuBarPlacementChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Ribbon ribbon) ribbon.PlaceMenuBar();
    }

    /// <summary>
    /// Moves the menu bar into the slot dictated by <see cref="MenuBarPlacement"/>. The same
    /// instance is reparented, so event handlers and bindings travel with it.
    /// </summary>
    private void PlaceMenuBar()
    {
        object? bar = MenuBar;
        if (bar == null) return;

        ContentControl? target = MenuBarPlacement switch
        {
            RibbonMenuBarPlacement.BelowRibbon => _menuBarBelow,
            _ => _menuBarAbove
        };

        if (target == null || ReferenceEquals(target.Content, bar)) return;

        foreach (ContentControl? host in new[] { _menuBarAbove, _menuBarBelow })
        {
            if (host != null && ReferenceEquals(host.Content, bar)) host.Content = null;
        }

        target.Content = bar;
    }

    private void OnQuickAccessPinsChanged(object? sender, EventArgs e)
    {
        if (!_restoringPins && _quickAccess != null)
        {
            _pinned.Clear();
            _pinned.AddRange(_quickAccess.PinnedItems);
        }

        // The panel may be open: refresh the ticks in place instead of rebuilding it under the user.
        foreach (RibbonMenuItem row in _quickAccessRows)
        {
            if (row.Tag is RibbonItem item) row.IsChecked = RibbonQuickAccessToolBar.GetIsPinned(item);
        }
    }

    /// <summary>
    /// Builds the customise panel: one section per tab, one checklist row per distinct command,
    /// each row showing the tab it belongs to so identical labels stay distinguishable.
    /// </summary>
    private void RebuildQuickAccessMenu()
    {
        if (_quickAccess == null) return;

        var menu = new RibbonMenu { MinWidth = 258 };
        _quickAccessRows.Clear();

        bool firstTab = true;

        foreach (RibbonTabItem tab in Tabs)
        {
            if (tab.Content is not IEnumerable groups) continue;

            var rows = new List<RibbonMenuItem>();
            var labels = new HashSet<string>(StringComparer.Ordinal);

            foreach (object? entry in groups)
            {
                if (entry is not RibbonGroup group) continue;

                foreach (RibbonItem item in group.Items.OfType<RibbonItem>())
                {
                    if (item.Header is not string label || label.Length == 0) continue;

                    // The same label usually means the same command; show it once per tab.
                    if (!labels.Add(label)) continue;

                    rows.Add(_quickAccess.CreateRow(item, null));
                }
            }

            if (rows.Count == 0) continue;

            if (!firstTab) menu.Items.Add(new RibbonMenuSeparator());
            firstTab = false;

            menu.Items.Add(CreateSectionCaption(tab.Header as string ?? string.Empty));
            foreach (RibbonMenuItem row in rows) menu.Items.Add(row);

            _quickAccessRows.AddRange(rows);
        }

        if (menu.Items.Count == 0) menu.Items.Add(new RibbonMenuItem { Header = "No commands", IsEnabled = false });

        _quickAccess.SetCustomizeMenu(menu);
    }

    private static TextBlock CreateSectionCaption(string text)
    {
        var caption = new TextBlock
        {
            Text = text,
            FontSize = 10.5,
            FontWeight = FontWeights.SemiBold,
            Margin = new Thickness(10, 8, 10, 3)
        };

        caption.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextTertiary");
        return caption;
    }

    private static IReadOnlyList<RibbonTabItem> Expand(IEnumerable? source)
    {
        if (source == null) return Array.Empty<RibbonTabItem>();

        var tabs = new List<RibbonTabItem>();

        foreach (object? entry in source)
        {
            switch (entry)
            {
                case RibbonTabItem tab:
                    tabs.Add(tab);
                    break;

                case RibbonContextualTabGroup group:
                    tabs.AddRange(group.Tabs);
                    break;
            }
        }

        return tabs;
    }

    private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not Ribbon ribbon) return;

        if (e.OldValue is IEnumerable oldSource)
        {
            foreach (object? entry in oldSource)
            {
                switch (entry)
                {
                    case RibbonTabItem tab:
                        tab.IsSelected = false;
                        if (ribbon._watched.Remove(tab)) tab.IsAvailableChanged -= ribbon.OnTabAvailabilityChanged;
                        break;

                    case RibbonContextualTabGroup group:
                        group.TabsChanged -= ribbon.OnGroupTabsChanged;
                        break;
                }
            }
        }

        if (e.NewValue is IEnumerable newSource)
        {
            foreach (object? entry in newSource)
            {
                if (entry is RibbonContextualTabGroup group) group.TabsChanged += ribbon.OnGroupTabsChanged;
            }
        }

        ribbon.RefreshTabs();
    }

    private static void OnSelectedIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Ribbon ribbon) ribbon.SyncSelection();
    }

    private void Watch(IReadOnlyList<RibbonTabItem> tabs)
    {
        foreach (RibbonTabItem tab in tabs)
        {
            if (_watched.Add(tab)) tab.IsAvailableChanged += OnTabAvailabilityChanged;
        }
    }

    private void OnTabAvailabilityChanged(object? sender, EventArgs e) => RefreshTabs();

    private void OnGroupTabsChanged(object? sender, EventArgs e) => RefreshTabs();

    private void SyncSelection()
    {
        IReadOnlyList<RibbonTabItem> tabs = VisibleTabs;

        if (tabs.Count == 0)
        {
            SelectedItem = null;
            return;
        }

        int index = Math.Max(0, Math.Min(SelectedIndex, tabs.Count - 1));
        if (index != SelectedIndex) SetCurrentValue(SelectedIndexProperty, index);

        for (int i = 0; i < tabs.Count; i++) tabs[i].IsSelected = i == index;

        SelectedItem = tabs[index];

        // Keep SelectedTabId in step without re-entering the selection logic.
        if (!_syncingTabId && !string.Equals(SelectedTabId, tabs[index].Id, StringComparison.Ordinal))
        {
            _syncingTabId = true;
            try { SetCurrentValue(SelectedTabIdProperty, tabs[index].Id); }
            finally { _syncingTabId = false; }
        }
    }
}
