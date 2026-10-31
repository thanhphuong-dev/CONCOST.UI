using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using CONCOST.UI.Icons;

namespace CONCOST.UI.Controls;

/// <summary>One command in a <see cref="CommandBar"/>.</summary>
public class CommandBarItem
{
    public CommandBarItem()
    {
    }

    public CommandBarItem(string label, ICommand? command, IconKind icon = IconKind.None, string? toolTip = null)
    {
        Label = label;
        Command = command;
        Icon = icon;
        ToolTip = toolTip;
    }

    /// <summary>Caption under or beside the icon.</summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>Glyph.</summary>
    public IconKind Icon { get; set; } = IconKind.None;

    /// <summary>Command run on click.</summary>
    public ICommand? Command { get; set; }

    /// <summary>Parameter passed to <see cref="Command"/>.</summary>
    public object? CommandParameter { get; set; }

    /// <summary>Tooltip text. Falls back to <see cref="Label"/>.</summary>
    public string? ToolTip { get; set; }

    /// <summary>Draw a separator instead of a button.</summary>
    public bool IsSeparator { get; set; }

    /// <summary>Hide the caption and keep only the icon. Default <c>true</c>.</summary>
    public bool ShowLabel { get; set; } = true;

    /// <summary>Enabled state of the button.</summary>
    public bool IsEnabled { get; set; } = true;
}

/// <summary>One command in a <see cref="QuickAccessToolbar"/> — icon only, no caption.</summary>
public class QuickAccessItem
{
    public QuickAccessItem()
    {
    }

    public QuickAccessItem(IconKind icon, string toolTip, ICommand? command)
    {
        Icon = icon;
        ToolTip = toolTip;
        Command = command;
    }

    public IconKind Icon { get; set; } = IconKind.None;

    public string? ToolTip { get; set; }

    public ICommand? Command { get; set; }

    public object? CommandParameter { get; set; }

    public bool IsEnabled { get; set; } = true;

    public bool IsSeparator { get; set; }
}

/// <summary>
/// Shared plumbing for a toolbar that overflows into a menu: the items, the ones that did not fit,
/// and the open/closed state of the overflow popup.
/// </summary>
/// <remarks>
/// The control does not decide what fits — <see cref="OverflowPanel"/> does that during layout, and
/// this class only mirrors the result after the pass. That keeps the "which items are hidden" answer
/// identical to what is actually on screen.
/// </remarks>
public abstract class OverflowToolbar : Control
{
    private readonly ObservableCollection<object> _overflow = new();
    private ItemsControl? _itemsHost;
    private OverflowPanel? _panel;

    protected OverflowToolbar()
    {
        // One observable collection for the lifetime of the control: the popup binds to the DP and
        // the collection notifications do the rest.
        SetValue(OverflowItemsPropertyKey, _overflow);
        LayoutUpdated += (_, _) => SyncOverflow();
    }

    public static readonly DependencyProperty ItemsSourceProperty =
        DependencyProperty.Register(nameof(ItemsSource), typeof(IEnumerable), typeof(OverflowToolbar),
            new PropertyMetadata(null, OnItemsChanged));

    /// <summary>The commands shown in the bar.</summary>
    public IEnumerable? ItemsSource
    {
        get => (IEnumerable?)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public static readonly DependencyProperty SpacingProperty =
        DependencyProperty.Register(nameof(Spacing), typeof(double), typeof(OverflowToolbar),
            new PropertyMetadata(2.0));

    /// <summary>Space between buttons, in pixels. Default 2.</summary>
    public double Spacing
    {
        get => (double)GetValue(SpacingProperty);
        set => SetValue(SpacingProperty, value);
    }

    public static readonly DependencyProperty IsOverflowOpenProperty =
        DependencyProperty.Register(nameof(IsOverflowOpen), typeof(bool), typeof(OverflowToolbar),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    /// <summary>The overflow menu is open.</summary>
    public bool IsOverflowOpen
    {
        get => (bool)GetValue(IsOverflowOpenProperty);
        set => SetValue(IsOverflowOpenProperty, value);
    }

    private static readonly DependencyPropertyKey HasOverflowPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(HasOverflow), typeof(bool), typeof(OverflowToolbar),
            new PropertyMetadata(false));

    public static readonly DependencyProperty HasOverflowProperty = HasOverflowPropertyKey.DependencyProperty;

    /// <summary>At least one item did not fit.</summary>
    public bool HasOverflow => (bool)GetValue(HasOverflowProperty);

    private static readonly DependencyPropertyKey OverflowItemsPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(OverflowItems), typeof(IEnumerable), typeof(OverflowToolbar),
            new PropertyMetadata(null));

    public static readonly DependencyProperty OverflowItemsProperty = OverflowItemsPropertyKey.DependencyProperty;

    /// <summary>The items that did not fit, in order. Raises change notifications as a collection.</summary>
    public IEnumerable OverflowItems => (IEnumerable)GetValue(OverflowItemsProperty);

    private static void OnItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is OverflowToolbar toolbar) toolbar.SyncOverflow();
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        _itemsHost = GetTemplateChild("PART_Items") as ItemsControl;
        _panel = null;
    }

    /// <summary>Runs an item's command, used by both the bar and the overflow menu.</summary>
    public void Invoke(object? item)
    {
        switch (item)
        {
            case CommandBarItem command when command.IsEnabled && command.Command is not null:
                if (command.Command.CanExecute(command.CommandParameter)) command.Command.Execute(command.CommandParameter);
                break;
            case QuickAccessItem quick when quick.IsEnabled && quick.Command is not null:
                if (quick.Command.CanExecute(quick.CommandParameter)) quick.Command.Execute(quick.CommandParameter);
                break;
        }
    }

    /// <summary>Recomputes <see cref="OverflowItems"/> from the current layout.</summary>
    public void SyncOverflow()
    {
        if (_itemsHost is null) return;

        _panel ??= FindPanel(_itemsHost);
        if (_panel is null) return;

        int visible = _panel.VisibleCount;
        var items = EnumerateItems().ToList();

        _overflow.Clear();
        for (int i = visible; i < items.Count; i++) _overflow.Add(items[i]);

        SetValue(HasOverflowPropertyKey, _overflow.Count > 0);
        if (_overflow.Count == 0) IsOverflowOpen = false;
    }

    private IEnumerable<object> EnumerateItems()
    {
        if (ItemsSource is null) yield break;

        foreach (var item in ItemsSource)
        {
            if (item is not null) yield return item;
        }
    }

    private static OverflowPanel? FindPanel(DependencyObject root)
    {
        int count = VisualTreeHelper.GetChildrenCount(root);
        for (int i = 0; i < count; i++)
        {
            var child = VisualTreeHelper.GetChild(root, i);
            if (child is OverflowPanel panel) return panel;

            var deeper = FindPanel(child);
            if (deeper is not null) return deeper;
        }

        return null;
    }
}

/// <summary>
/// A toolbar of commands that pushes what does not fit into a "more" menu instead of clipping it.
/// </summary>
public class CommandBar : OverflowToolbar
{
    static CommandBar()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(CommandBar),
            new FrameworkPropertyMetadata(typeof(CommandBar)));
    }

    public static readonly DependencyProperty OverflowToolTipProperty =
        DependencyProperty.Register(nameof(OverflowToolTip), typeof(string), typeof(CommandBar),
            new PropertyMetadata("Thêm"));

    /// <summary>Tooltip of the overflow button.</summary>
    public string OverflowToolTip
    {
        get => (string)GetValue(OverflowToolTipProperty);
        set => SetValue(OverflowToolTipProperty, value);
    }
}

/// <summary>
/// The compact strip of icon-only commands above a ribbon — the standalone version of the ribbon's
/// quick-access toolbar, usable in a plain window.
/// </summary>
public class QuickAccessToolbar : OverflowToolbar
{
    static QuickAccessToolbar()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(QuickAccessToolbar),
            new FrameworkPropertyMetadata(typeof(QuickAccessToolbar)));
    }

    public static readonly DependencyProperty IsCustomizableProperty =
        DependencyProperty.Register(nameof(IsCustomizable), typeof(bool), typeof(QuickAccessToolbar),
            new PropertyMetadata(true));

    /// <summary>Show the "more" chevron that opens the overflow menu. Default <c>true</c>.</summary>
    public bool IsCustomizable
    {
        get => (bool)GetValue(IsCustomizableProperty);
        set => SetValue(IsCustomizableProperty, value);
    }
}
