using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace CONCOST.UI.Controls.Ribbon;

/// <summary>
/// Lays the groups of one tab out left to right and decides how much to shrink them.
/// <para>
/// The ladder is the one Office uses: if the natural width does not fit, demote every Large item to
/// Medium, then every item to Small, and only then collapse groups (lowest
/// <see cref="RibbonGroup.CollapsePriority"/> first).
/// </para>
/// <para>
/// The panel escalates <b>one step per layout pass</b>. Re-measuring a child inside the same pass does
/// not work: its constraint is unchanged, so WPF reuses the cached DesiredSize and the ladder would
/// read the widths of the previous density. Asking for another pass costs nothing visible and settles
/// in at most two steps plus one per collapsed group.
/// </para>
/// </summary>
public class RibbonGroupStrip : Panel
{
    private RibbonGroupSizeMode _mode = RibbonGroupSizeMode.Full;
    private readonly HashSet<RibbonGroup> _autoCollapsed = new();
    private double _lastAvailable = -1;
    private bool _measureAfterRestore;
    private Ribbon? _owner;

    protected override Size MeasureOverride(Size availableSize)
    {
        double available = double.IsInfinity(availableSize.Width) ? double.PositiveInfinity : availableSize.Width;

        // The window grew: forget the previous ladder and give every group its space back, except
        // the ones the user collapsed by hand.
        if (available > _lastAvailable + 0.5)
        {
            RestoreAutoCollapsed();
            _measureAfterRestore = true;
        }

        // The host may switch the last rung off at any time; a group collapsed a moment ago has to
        // come back, otherwise the property looks like it did nothing.
        if (!IsGroupCollapseAllowed() && _autoCollapsed.Count > 0)
        {
            RestoreAutoCollapsed();
            _measureAfterRestore = true;
        }

        _lastAvailable = available;

        // During the very first pass the ribbon may still be 0 px wide; collapsing everything then
        // would stick until the user resizes, so wait for a usable width.
        if (available < 120) return new Size(0, 0);

        double total = 0;
        double height = 0;

        foreach (UIElement child in InternalChildren)
        {
            // Measure at the density that is in force right now: the constraint never changes, so a
            // cached DesiredSize would still describe the previous density.
            child.InvalidateMeasure();
            child.Measure(new Size(double.PositiveInfinity, availableSize.Height));
            total += child.DesiredSize.Width;
            height = Math.Max(height, child.DesiredSize.Height);
        }

        MarkLastGroup();

        // The widths above still describe the collapsed layout, so this pass only measures;
        // the decision is taken on the next one with fresh sizes.
        if (_measureAfterRestore)
        {
            _measureAfterRestore = false;
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Loaded, new Action(InvalidateMeasure));
            return new Size(total, height);
        }

        if (total <= available) return new Size(total, height);

        // One escalation step per pass. The returned width stays truthful so the parent keeps
        // asking for the real size instead of clipping a half-collapsed row.
        bool changed = false;

        if (_mode == RibbonGroupSizeMode.Full)
        {
            _mode = RibbonGroupSizeMode.Medium;
            changed = ApplyMode(_mode);
        }
        else if (_mode == RibbonGroupSizeMode.Medium)
        {
            _mode = RibbonGroupSizeMode.Small;
            changed = ApplyMode(_mode);
        }
        else
        {
            // Hosts that need every group readable at any width switch the last rung off; the
            // strip then keeps demoting but never turns a group into a drop-down button.
            changed = IsGroupCollapseAllowed() && CollapseNextGroup();
        }

        // Invalidating during MeasureOverride is swallowed by the layout manager, so the follow-up
        // pass is queued on the dispatcher instead.
        if (changed) Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Loaded, new Action(InvalidateMeasure));

        return new Size(total, height);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        double x = 0;

        foreach (UIElement child in InternalChildren)
        {
            double width = child.DesiredSize.Width;
            child.Arrange(new Rect(x, 0, width, finalSize.Height));
            x += width;
        }

        return finalSize;
    }

    /// <summary>Tells the right-most group to drop its divider, so no line trails into empty space.</summary>
    private void MarkLastGroup()
    {
        List<RibbonGroup> groups = Groups();

        for (int i = 0; i < groups.Count; i++)
        {
            bool isLast = i == groups.Count - 1;

            if (groups[i].IsLastInStrip != isLast) groups[i].SetIsLastInStrip(isLast);
        }
    }

    private bool ApplyMode(RibbonGroupSizeMode mode)
    {
        bool changed = false;

        foreach (RibbonGroup group in Groups())
        {
            if (group.SizeMode == mode) continue;
            group.SizeMode = mode;
            changed = true;
        }

        return changed;
    }

    /// <summary>
    /// Puts back every group this strip folded. The ribbon calls it when the host switches
    /// <see cref="Ribbon.IsGroupCollapseEnabled"/> off, so the property takes effect immediately
    /// instead of only for the next collapse.
    /// </summary>
    internal void RestoreCollapsed()
    {
        if (_autoCollapsed.Count == 0) return;

        RestoreAutoCollapsed();
        InvalidateMeasure();
    }

    /// <summary>
    /// Reads <see cref="Ribbon.IsGroupCollapseEnabled"/> from the ribbon that hosts this strip.
    /// The strip is created by an ItemsPanelTemplate inside a DataTemplate, so it is safer to walk
    /// the visual tree than to rely on the property being in the inheritance chain.
    /// </summary>
    private bool IsGroupCollapseAllowed()
    {
        _owner ??= FindOwner();

        // The visual-tree walk can come up empty on the very first measure pass; the inherited
        // property is the reliable fallback because it is already set on every descendant.
        return _owner?.IsGroupCollapseEnabled
            ?? (bool)GetValue(Ribbon.IsGroupCollapseEnabledProperty);
    }

    private Ribbon? FindOwner()
    {
        DependencyObject? node = this;

        while (node != null)
        {
            if (node is Ribbon ribbon) return ribbon;
            node = System.Windows.Media.VisualTreeHelper.GetParent(node);
        }

        return null;
    }

    private bool CollapseNextGroup()
    {
        RibbonGroup? candidate = null;

        foreach (RibbonGroup group in Groups())
        {
            if (group.IsCollapsed || group.CollapsePriority <= 0) continue;
            if (candidate == null || group.CollapsePriority < candidate.CollapsePriority) candidate = group;
        }

        if (candidate == null) return false;

        candidate.IsCollapsed = true;
        _autoCollapsed.Add(candidate);
        return true;
    }

    /// <summary>
    /// Gives the groups their space back after the ribbon grew: every group the panel collapsed is
    /// expanded again (user-collapsed ones stay collapsed) and the density ladder restarts at Full.
    /// </summary>
    private void RestoreAutoCollapsed()
    {
        foreach (RibbonGroup group in _autoCollapsed)
            if (!group.IsUserCollapsed) group.IsCollapsed = false;

        _autoCollapsed.Clear();
        _mode = RibbonGroupSizeMode.Full;
        ApplyMode(_mode);
    }

    private List<RibbonGroup> Groups()
    {
        var groups = new List<RibbonGroup>();

        foreach (UIElement child in InternalChildren)
            if (child is RibbonGroup group) groups.Add(group);

        return groups;
    }
}
