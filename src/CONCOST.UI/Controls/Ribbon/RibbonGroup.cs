using System.Windows;
using System.Windows.Controls;

namespace CONCOST.UI.Controls.Ribbon;

/// <summary>
/// A labelled cluster of commands inside a <see cref="RibbonTabItem"/>.
/// Items are arranged by <see cref="RibbonGroupPanel"/>; the optional dialog launcher sits in the
/// bottom-right corner, as in Office.
/// </summary>
public class RibbonGroup : HeaderedItemsControl
{
    public static readonly DependencyProperty DialogLauncherCommandProperty =
        DependencyProperty.Register(nameof(DialogLauncherCommand), typeof(System.Windows.Input.ICommand),
            typeof(RibbonGroup), new FrameworkPropertyMetadata(null));

    public static readonly DependencyProperty CollapsePriorityProperty =
        DependencyProperty.Register(nameof(CollapsePriority), typeof(int), typeof(RibbonGroup),
            new FrameworkPropertyMetadata(100));

    public static readonly DependencyProperty IsCollapsedProperty =
        DependencyProperty.Register(nameof(IsCollapsed), typeof(bool), typeof(RibbonGroup),
            new FrameworkPropertyMetadata(false));

    public static readonly DependencyProperty SizeModeProperty =
        DependencyProperty.Register(nameof(SizeMode), typeof(RibbonGroupSizeMode), typeof(RibbonGroup),
            new FrameworkPropertyMetadata(RibbonGroupSizeMode.Full, FrameworkPropertyMetadataOptions.AffectsMeasure));

    public static readonly DependencyProperty IsDropDownOpenProperty =
        DependencyProperty.Register(nameof(IsDropDownOpen), typeof(bool), typeof(RibbonGroup),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public static readonly DependencyProperty IsUserCollapsedProperty =
        DependencyProperty.Register(nameof(IsUserCollapsed), typeof(bool), typeof(RibbonGroup),
            new FrameworkPropertyMetadata(false, OnUserCollapsedChanged));

    private static readonly DependencyPropertyKey IsLastInStripPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(IsLastInStrip), typeof(bool), typeof(RibbonGroup),
            new FrameworkPropertyMetadata(false));

    /// <summary>True for the right-most group of a tab, which draws no trailing divider.</summary>
    public static readonly DependencyProperty IsLastInStripProperty = IsLastInStripPropertyKey.DependencyProperty;

    private RibbonMenu? _collapsedMenu;

    static RibbonGroup()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(RibbonGroup),
            new FrameworkPropertyMetadata(typeof(RibbonGroup)));
    }

    /// <summary>Same property as <see cref="KeyTipService.KeyTipProperty"/>, so both spellings work.</summary>
    public static readonly DependencyProperty KeyTipProperty = KeyTipService.KeyTipProperty;

    /// <summary>
    /// Letters that open this group's launcher while the Alt key tips are showing. Only used when
    /// <see cref="DialogLauncherCommand"/> is set; empty means “derive one from the header”.
    /// </summary>
    public string KeyTip
    {
        get => KeyTipService.GetKeyTip(this);
        set => KeyTipService.SetKeyTip(this, value);
    }

    /// <summary>Command behind the small dialog-launcher arrow in the corner.</summary>
    public System.Windows.Input.ICommand? DialogLauncherCommand
    {
        get => (System.Windows.Input.ICommand?)GetValue(DialogLauncherCommandProperty);
        set => SetValue(DialogLauncherCommandProperty, value);
    }

    /// <summary>
    /// Lower values collapse into a popup earlier when the ribbon runs out of width.
    /// Zero means the group never collapses.
    /// </summary>
    public int CollapsePriority
    {
        get => (int)GetValue(CollapsePriorityProperty);
        set => SetValue(CollapsePriorityProperty, value);
    }

    /// <summary>
    /// Set by <see cref="RibbonGroupStrip"/> on the right-most group. The template drops that group's
    /// divider, otherwise the line hangs in the empty space after the last command.
    /// </summary>
    public bool IsLastInStrip => (bool)GetValue(IsLastInStripProperty);

    internal void SetIsLastInStrip(bool value) => SetValue(IsLastInStripPropertyKey, value);

    /// <summary>True when the group is currently reduced to a single dropdown button.</summary>
    public bool IsCollapsed
    {
        get => (bool)GetValue(IsCollapsedProperty);
        set => SetValue(IsCollapsedProperty, value);
    }

    /// <summary>
    /// Density currently in force. The owning <see cref="RibbonGroupStrip"/> lowers it when the
    /// ribbon runs out of width; setting it by hand pins the group to that density.
    /// </summary>
    public RibbonGroupSizeMode SizeMode
    {
        get => (RibbonGroupSizeMode)GetValue(SizeModeProperty);
        set => SetValue(SizeModeProperty, value);
    }

    /// <summary>Whether the collapsed group's dropdown is showing.</summary>
    public bool IsDropDownOpen
    {
        get => (bool)GetValue(IsDropDownOpenProperty);
        set => SetValue(IsDropDownOpenProperty, value);
    }

    /// <summary>
    /// Set when the user collapses the group from the arrow next to its header. The ribbon never
    /// re-expands a group collapsed this way; it only restores the ones it collapsed itself.
    /// </summary>
    public bool IsUserCollapsed
    {
        get => (bool)GetValue(IsUserCollapsedProperty);
        set => SetValue(IsUserCollapsedProperty, value);
    }

    protected override System.Windows.Automation.Peers.AutomationPeer OnCreateAutomationPeer() =>
        new RibbonGroupAutomationPeer(this);

    private static void OnUserCollapsedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is RibbonGroup group) group.IsCollapsed = (bool)e.NewValue;
    }

    /// <summary>
    /// Menu shown when the group is collapsed. It mirrors the group's items, because a collapsed
    /// group has to present the same commands as menu rows rather than as ribbon buttons.
    /// </summary>
    public RibbonMenu CollapsedMenu => _collapsedMenu ??= BuildCollapsedMenu();

    private RibbonMenu BuildCollapsedMenu()
    {
        var menu = new RibbonMenu();

        foreach (object? item in Items)
        {
            if (item is RibbonSeparator)
            {
                menu.Items.Add(new RibbonMenuSeparator());
                continue;
            }

            // Selection lists cannot be shown inline in a menu, so they become a submenu that
            // still drives the original control.
            if (item is RibbonGallery gallery)
            {
                menu.Items.Add(BuildChoiceEntry(gallery.Header, gallery.EffectiveIcon,
                    gallery.ItemsSource, value => gallery.SelectedItem = value));
                continue;
            }

            if (item is RibbonComboBox combo)
            {
                menu.Items.Add(BuildChoiceEntry(combo.Header, combo.EffectiveIcon,
                    combo.ItemsSource, value => combo.SelectedItem = value));
                continue;
            }

            if (item is not RibbonItem source) continue;

            var entry = new RibbonMenuItem
            {
                Header = source.Header,
                Icon = source.EffectiveIcon,
                Command = source.Command,
                CommandParameter = source.CommandParameter,
                IsEnabled = source.IsEnabled
            };

            entry.Click += (_, _) => IsDropDownOpen = false;
            menu.Items.Add(entry);
        }

        // The corner arrow is gone once the group is collapsed, so the same command gets a row.
        if (DialogLauncherCommand != null)
        {
            if (menu.Items.Count > 0) menu.Items.Add(new RibbonMenuSeparator());

            var launcher = new RibbonMenuItem
            {
                Header = "Group settings",
                Command = DialogLauncherCommand
            };

            launcher.Click += (_, _) => IsDropDownOpen = false;
            menu.Items.Add(launcher);
        }

        return menu;
    }

    /// <summary>
    /// Builds a menu row whose submenu lists the entries of a gallery or combo box, so a collapsed
    /// group still lets the user pick a value.
    /// </summary>
    private RibbonMenuItem BuildChoiceEntry(object? header, System.Windows.Media.ImageSource? icon,
        System.Collections.IEnumerable? source, Action<object?> select)
    {
        var root = new RibbonMenuItem { Header = header, Icon = icon };

        if (source == null) return root;

        foreach (object? value in source)
        {
            string label = value is RibbonGalleryItem tile ? tile.Header : value?.ToString() ?? string.Empty;
            var choice = new RibbonMenuItem { Header = label };

            if (value is RibbonGalleryItem { Icon: not null } tileWithIcon) choice.Icon = tileWithIcon.Icon;

            choice.Click += (_, _) =>
            {
                select(value);
                IsDropDownOpen = false;
            };

            root.Items.Add(choice);
        }

        return root;
    }

    protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        // Items may have been added or replaced while collapsed.
        if (e.Property == IsCollapsedProperty && (bool)e.NewValue) _collapsedMenu = null;

        // Changing the density must re-pack the items. Invalidating the group alone is not enough:
        // the panel is measured again with the same constraint, so WPF would skip its MeasureOverride.
        if (e.Property == SizeModeProperty) InvalidateItemsPanel();
    }

    private void InvalidateItemsPanel()
    {
        RibbonGroupPanel? panel = FindPanel(this);
        panel?.InvalidateMeasure();
    }

    private static RibbonGroupPanel? FindPanel(DependencyObject root)
    {
        int count = System.Windows.Media.VisualTreeHelper.GetChildrenCount(root);

        for (int i = 0; i < count; i++)
        {
            DependencyObject child = System.Windows.Media.VisualTreeHelper.GetChild(root, i);
            if (child is RibbonGroupPanel panel) return panel;

            RibbonGroupPanel? nested = FindPanel(child);
            if (nested != null) return nested;
        }

        return null;
    }
}
