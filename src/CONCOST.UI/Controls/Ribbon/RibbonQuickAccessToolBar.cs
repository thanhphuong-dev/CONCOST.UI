using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace CONCOST.UI.Controls.Ribbon;

/// <summary>
/// The small strip of always-available commands above the tabs.
/// </summary>
/// <remarks>
/// <para>
/// A ribbon item can only have one visual parent, so the toolbar does not host the original
/// <see cref="RibbonItem"/> instances — it hosts light-weight clones produced by
/// <see cref="RibbonItem.CreateQuickAccessClone"/>. Commands, icons and enabled state still flow
/// from the original, so pinning a button never duplicates business logic.
/// </para>
/// <para>
/// The chevron opens the customise panel: a scrollable checklist of every command, grouped by tab,
/// with a tick on the pinned ones. Clicking a row pins or unpins it and leaves the panel open, so
/// several commands can be pinned in one go.
/// </para>
/// </remarks>
public class RibbonQuickAccessToolBar : ItemsControl
{
    /// <summary>Marks a <see cref="RibbonItem"/> as pinned. Set by <see cref="Pin"/>.</summary>
    public static readonly DependencyProperty IsPinnedProperty =
        DependencyProperty.RegisterAttached("IsPinned", typeof(bool), typeof(RibbonQuickAccessToolBar),
            new FrameworkPropertyMetadata(false));

    public static readonly DependencyProperty PinnableItemsProperty =
        DependencyProperty.Register(nameof(PinnableItems), typeof(IEnumerable), typeof(RibbonQuickAccessToolBar),
            new FrameworkPropertyMetadata(null, OnPinnableItemsChanged));

    public static readonly DependencyProperty IsCustomizeOpenProperty =
        DependencyProperty.Register(nameof(IsCustomizeOpen), typeof(bool), typeof(RibbonQuickAccessToolBar),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public static readonly DependencyProperty ShowLabelsProperty =
        DependencyProperty.Register(nameof(ShowLabels), typeof(bool), typeof(RibbonQuickAccessToolBar),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsMeasure));

    private static readonly DependencyPropertyKey CustomizeMenuPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(CustomizeMenu), typeof(object), typeof(RibbonQuickAccessToolBar),
            new FrameworkPropertyMetadata(null));

    public static readonly DependencyProperty CustomizeMenuProperty = CustomizeMenuPropertyKey.DependencyProperty;

    /// <summary>Empties the strip. Bound to the “Remove all” button of the customise panel.</summary>
    public static readonly RoutedUICommand ClearPinsCommand =
        new("Remove all", nameof(ClearPinsCommand), typeof(RibbonQuickAccessToolBar));

    private Popup? _popup;

    static RibbonQuickAccessToolBar()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(RibbonQuickAccessToolBar),
            new FrameworkPropertyMetadata(typeof(RibbonQuickAccessToolBar)));
    }

    public RibbonQuickAccessToolBar()
    {
        Focusable = false;

        CommandBindings.Add(new CommandBinding(ClearPinsCommand, (_, _) => ClearPins()));
    }

    /// <summary>Raised whenever a command is pinned or unpinned.</summary>
    public event EventHandler? PinsChanged;

    /// <summary>Every command the ribbon offers — used to build a fallback customise list.</summary>
    public IEnumerable? PinnableItems
    {
        get => (IEnumerable?)GetValue(PinnableItemsProperty);
        set => SetValue(PinnableItemsProperty, value);
    }

    /// <summary>Whether the customise panel is showing.</summary>
    public bool IsCustomizeOpen
    {
        get => (bool)GetValue(IsCustomizeOpenProperty);
        set => SetValue(IsCustomizeOpenProperty, value);
    }

    /// <summary>Shows the label next to each icon. Off by default, as in Office.</summary>
    public bool ShowLabels
    {
        get => (bool)GetValue(ShowLabelsProperty);
        set => SetValue(ShowLabelsProperty, value);
    }

    /// <summary>
    /// Content of the customise panel. The owning <see cref="Ribbon"/> supplies a menu grouped by
    /// tab; when the toolbar is used on its own a flat list is generated from
    /// <see cref="PinnableItems"/> instead.
    /// </summary>
    public object? CustomizeMenu => GetValue(CustomizeMenuProperty);

    /// <summary>Original items that are currently pinned.</summary>
    public IEnumerable<RibbonItem> PinnedItems =>
        Items.OfType<RibbonQuickAccessButton>().Select(b => b.Tag).OfType<RibbonItem>();

    public static bool GetIsPinned(DependencyObject element) => (bool)element.GetValue(IsPinnedProperty);

    public static void SetIsPinned(DependencyObject element, bool value) => element.SetValue(IsPinnedProperty, value);

    /// <summary>True when <paramref name="source"/> is on the strip.</summary>
    public bool IsPinned(RibbonItem source) => source != null && GetIsPinned(source);

    /// <summary>Pins <paramref name="source"/> at the end of the strip. No-op when already pinned.</summary>
    public void Pin(RibbonItem source)
    {
        if (source == null) return;
        if (Items.OfType<RibbonQuickAccessButton>().Any(b => ReferenceEquals(b.Tag, source))) return;

        RibbonQuickAccessButton proxy = source.CreateQuickAccessClone();
        proxy.Tag = source;

        Items.Add(proxy);
        SetIsPinned(source, true);
        PinsChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Removes <paramref name="source"/> from the strip.</summary>
    public void Unpin(RibbonItem source)
    {
        if (source == null) return;

        for (int i = Items.Count - 1; i >= 0; i--)
        {
            if (Items[i] is RibbonQuickAccessButton button && ReferenceEquals(button.Tag, source)) Items.RemoveAt(i);
        }

        SetIsPinned(source, false);
        PinsChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Pins when missing, unpins when present.</summary>
    public void TogglePin(RibbonItem source)
    {
        if (source == null) return;
        if (GetIsPinned(source)) Unpin(source);
        else Pin(source);
    }

    /// <summary>Drops every pinned command.</summary>
    public void ClearPins()
    {
        foreach (RibbonItem item in PinnedItems.ToList()) Unpin(item);
    }

    internal void SetCustomizeMenu(object? menu) => SetValue(CustomizeMenuPropertyKey, menu);

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _popup = GetTemplateChild("PART_Customize") as Popup;

        // Escape has to close the panel even though the popup itself never holds focus.
        if (_popup?.Child is UIElement content) content.PreviewKeyDown += OnPopupKeyDown;
    }

    private void OnPopupKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Escape || !IsCustomizeOpen) return;

        IsCustomizeOpen = false;
        e.Handled = true;
    }

    private static void OnPinnableItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not RibbonQuickAccessToolBar bar) return;

        // The ribbon supplies its own grouped menu; only fall back when nobody did.
        if (bar.CustomizeMenu == null) bar.SetCustomizeMenu(bar.BuildFlatMenu());
    }

    /// <summary>
    /// Fallback list for a standalone toolbar: one row per command, no tab captions.
    /// </summary>
    private object BuildFlatMenu()
    {
        var menu = new RibbonMenu { MinWidth = 240 };

        foreach (RibbonItem item in PinnableItems?.OfType<RibbonItem>() ?? Enumerable.Empty<RibbonItem>())
        {
            menu.Items.Add(CreateRow(item, null));
        }

        if (menu.Items.Count == 0) menu.Items.Add(new RibbonMenuItem { Header = "No commands", IsEnabled = false });

        return menu;
    }

    /// <summary>One checklist row: icon, label, optional tab caption, tick when pinned.</summary>
    internal RibbonMenuItem CreateRow(RibbonItem item, string? tabCaption)
    {
        var row = new RibbonMenuItem
        {
            Header = item.Header,
            Icon = item.EffectiveIcon,
            InputGestureText = tabCaption ?? string.Empty,
            IsCheckable = true,
            IsChecked = GetIsPinned(item),
            Tag = item
        };

        row.Click += (_, _) =>
        {
            TogglePin(item);
            row.IsChecked = GetIsPinned(item);
        };

        return row;
    }
}
