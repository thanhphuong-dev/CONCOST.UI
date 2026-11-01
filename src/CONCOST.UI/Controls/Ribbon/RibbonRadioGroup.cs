using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;

namespace CONCOST.UI.Controls.Ribbon;

/// <summary>
/// A column of small buttons where exactly one is on — the “Wall / Floor / Ceiling” choice a
/// ribbon shows as a compact set rather than as a dropdown.
/// </summary>
/// <remarks>
/// <para>
/// The members are ordinary <see cref="RibbonToggleButton"/> instances, so they keep their own
/// command, key tip and screen tip; the container only owns the mutual exclusion and the
/// <see cref="SelectedIndex"/> / <see cref="SelectedItem"/> pair the host binds to.
/// </para>
/// <para>
/// A radio set always has one member on: clicking the current option again is a no-op rather than
/// a way to reach an empty state the data model cannot describe.
/// </para>
/// </remarks>
public class RibbonRadioGroup : RibbonItem
{
    public static readonly DependencyProperty SelectedIndexProperty =
        DependencyProperty.Register(nameof(SelectedIndex), typeof(int), typeof(RibbonRadioGroup),
            new FrameworkPropertyMetadata(-1, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnSelectedIndexChanged, CoerceSelectedIndex));

    public static readonly DependencyProperty SelectedItemProperty =
        DependencyProperty.Register(nameof(SelectedItem), typeof(object), typeof(RibbonRadioGroup),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnSelectedItemChanged));

    private bool _syncing;

    static RibbonRadioGroup()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(RibbonRadioGroup),
            new FrameworkPropertyMetadata(typeof(RibbonRadioGroup)));
    }

    public RibbonRadioGroup()
    {
        Items.CollectionChanged += OnItemsChanged;
    }

    /// <summary>The options, in display order.</summary>
    public ObservableCollection<object> Items { get; } = new();

    /// <summary>Index of the checked member, or -1 when the set has not been filled in yet.</summary>
    public int SelectedIndex
    {
        get => (int)GetValue(SelectedIndexProperty);
        set => SetValue(SelectedIndexProperty, value);
    }

    /// <summary>The checked member itself — handy when the items are not plain strings.</summary>
    public object? SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    /// <summary>
    /// Runs the selected member. A radio group has no command of its own, so a key tip aimed at it
    /// should do what a click on the current option does.
    /// </summary>
    public override void PerformClick()
    {
        base.PerformClick();

        if (SelectedIndex >= 0 && SelectedIndex < Items.Count && Items[SelectedIndex] is RibbonToggleButton button)
            button.PerformClick();
    }

    private void OnItemsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems != null)
        {
            foreach (object? item in e.OldItems)
                if (item is RibbonToggleButton removed) removed.IsCheckedChanged -= OnMemberCheckedChanged;
        }

        if (e.NewItems != null)
        {
            foreach (object? item in e.NewItems)
            {
                if (item is not RibbonToggleButton added) continue;

                // A member of a radio set is always a one-row command, whatever the caller wrote.
                added.Size = RibbonItemSize.Small;
                added.GroupName = string.Empty;
                added.IsCheckedChanged += OnMemberCheckedChanged;
            }
        }

        // The set may have shrunk below the remembered index.
        CoerceValue(SelectedIndexProperty);

        if (e.Action == NotifyCollectionChangedAction.Reset) SyncFromSelection();
        else if (SelectedIndex >= 0) SyncFromSelection();
    }

    private void OnMemberCheckedChanged(object? sender, EventArgs e)
    {
        if (_syncing || sender is not RibbonToggleButton button) return;

        int index = Items.IndexOf(button);
        if (index < 0) return;

        if (button.IsChecked)
        {
            Select(index);
            return;
        }

        // Clicking the option that is already on must not leave the set with nothing selected.
        if (index == SelectedIndex)
        {
            _syncing = true;
            try { button.IsChecked = true; }
            finally { _syncing = false; }
        }
    }

    /// <summary>Checks the member at <paramref name="index"/> and clears the others.</summary>
    private void Select(int index)
    {
        _syncing = true;

        try
        {
            SetCurrentValue(SelectedIndexProperty, index);
            SetCurrentValue(SelectedItemProperty, index >= 0 && index < Items.Count ? Items[index] : null);
            ApplyChecked();
        }
        finally
        {
            _syncing = false;
        }
    }

    private void ApplyChecked()
    {
        for (int i = 0; i < Items.Count; i++)
        {
            if (Items[i] is RibbonToggleButton button) button.IsChecked = i == SelectedIndex;
        }
    }

    /// <summary>Pushes the current index onto the buttons — used after a programmatic change.</summary>
    private void SyncFromSelection()
    {
        if (_syncing) return;

        _syncing = true;
        try { ApplyChecked(); }
        finally { _syncing = false; }
    }

    private static object CoerceSelectedIndex(DependencyObject d, object baseValue)
    {
        if (d is not RibbonRadioGroup group) return baseValue;

        int index = (int)baseValue;
        if (index < 0) return baseValue;
        return index >= group.Items.Count ? Math.Max(0, group.Items.Count - 1) : index;
    }

    private static void OnSelectedIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not RibbonRadioGroup group || group._syncing) return;

        int index = (int)e.NewValue;

        group._syncing = true;
        try
        {
            group.ApplyChecked();
            group.SetCurrentValue(SelectedItemProperty,
                index >= 0 && index < group.Items.Count ? group.Items[index] : null);
        }
        finally
        {
            group._syncing = false;
        }
    }

    private static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not RibbonRadioGroup group || group._syncing) return;

        int index = e.NewValue == null ? -1 : group.Items.IndexOf(e.NewValue);
        if (index < 0) return;

        group.Select(index);
    }
}
