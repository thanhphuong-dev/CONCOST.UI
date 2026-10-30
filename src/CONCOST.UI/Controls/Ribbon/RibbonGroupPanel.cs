using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace CONCOST.UI.Controls.Ribbon;

/// <summary>
/// Lays a <see cref="RibbonGroup"/> out on the classic three-row grid.
/// <para>
/// A <see cref="RibbonItemSize.Large"/> item owns a whole column (3 rows), a Medium item takes two
/// rows and a Small item one. Items are packed into the current column until it is full, then a new
/// column starts — the same rule Office uses, so a group never leaves a hole in the middle.
/// </para>
/// <para>
/// Items that need more room than one column (text boxes, combo boxes, galleries) set
/// <see cref="RibbonItem.ColumnSpan"/>; the panel then looks for a band that is free across every
/// column the item wants before placing it.
/// </para>
/// <para>
/// Both passes call the same pure <see cref="Pack"/> routine, so measuring never depends on the
/// result of a previous measure and the panel cannot get stuck in a layout loop.
/// </para>
/// </summary>
public class RibbonGroupPanel : Panel
{
    /// <summary>Number of rows every group is divided into.</summary>
    private const int RowCount = 3;

    public static readonly DependencyProperty ColumnWidthProperty =
        DependencyProperty.Register(nameof(ColumnWidth), typeof(double), typeof(RibbonGroupPanel),
            new FrameworkPropertyMetadata(72.0, FrameworkPropertyMetadataOptions.AffectsMeasure));

    public static readonly DependencyProperty RowHeightProperty =
        DependencyProperty.Register(nameof(RowHeight), typeof(double), typeof(RibbonGroupPanel),
            new FrameworkPropertyMetadata(30.0, FrameworkPropertyMetadataOptions.AffectsMeasure));

    public static readonly DependencyProperty RowGapProperty =
        DependencyProperty.Register(nameof(RowGap), typeof(double), typeof(RibbonGroupPanel),
            new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.AffectsMeasure));

    public static readonly DependencyProperty SeparatorWidthProperty =
        DependencyProperty.Register(nameof(SeparatorWidth), typeof(double), typeof(RibbonGroupPanel),
            new FrameworkPropertyMetadata(9.0, FrameworkPropertyMetadataOptions.AffectsMeasure));

    /// <summary>Width of one column of items.</summary>
    public double ColumnWidth
    {
        get => (double)GetValue(ColumnWidthProperty);
        set => SetValue(ColumnWidthProperty, value);
    }

    /// <summary>Height of a single row (a Small item).</summary>
    public double RowHeight
    {
        get => (double)GetValue(RowHeightProperty);
        set => SetValue(RowHeightProperty, value);
    }

    /// <summary>Vertical gap between two rows.</summary>
    public double RowGap
    {
        get => (double)GetValue(RowGapProperty);
        set => SetValue(RowGapProperty, value);
    }

    /// <summary>Width reserved for a <see cref="RibbonSeparator"/>.</summary>
    public double SeparatorWidth
    {
        get => (double)GetValue(SeparatorWidthProperty);
        set => SetValue(SeparatorWidthProperty, value);
    }

    /// <summary>Total height of the three rows.</summary>
    public double GroupHeight => RowCount * RowHeight + (RowCount - 1) * RowGap;

    /// <summary>Height of a Medium item (two rows).</summary>
    public double MediumHeight => 2 * RowHeight + RowGap;

    protected override Size MeasureOverride(Size availableSize)
    {
        Pack((child, rect) => child.Measure(new Size(rect.Width, rect.Height)), out double width);

        return new Size(width, GroupHeight);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        Pack((child, rect) => child.Arrange(rect), out _);
        return finalSize;
    }

    /// <summary>
    /// The <see cref="RibbonGroup"/> this panel belongs to. The panel is created by the group's
    /// items panel template, so its <c>TemplatedParent</c> is the ItemsControl — the group has to
    /// be found by walking up the visual tree.
    /// </summary>
    private RibbonGroup? OwnerGroup()
    {
        DependencyObject? current = this;

        while (current != null)
        {
            if (current is RibbonGroup group) return group;
            current = System.Windows.Media.VisualTreeHelper.GetParent(current);
        }

        return null;
    }

    /// <summary>One column of the grid: an item column or a separator column.</summary>
    private sealed class Slot
    {
        public double Width;
        public bool IsSeparator;
        public readonly bool[] Used = new bool[RowCount];
    }

    private void Pack(Action<UIElement, Rect> place, out double width)
    {
        double columnWidth = Math.Max(1, ColumnWidth);
        double rowHeight = Math.Max(1, RowHeight);
        double gap = Math.Max(0, RowGap);
        double separator = Math.Max(1, SeparatorWidth);

        var slots = new List<Slot>();
        int cursor = 0;

        RibbonGroupSizeMode cap = OwnerGroup()?.SizeMode ?? RibbonGroupSizeMode.Full;

        foreach (UIElement child in InternalChildren)
        {
            if (child is RibbonSeparator)
            {
                // A separator always ends the column before it and starts a new one.
                slots.Add(new Slot { Width = separator, IsSeparator = true });
                place(child, new Rect(Left(slots, slots.Count - 1), 0, separator, GroupHeight));
                cursor = slots.Count;
                continue;
            }

            RibbonItemSize size = (child as RibbonItem)?.Size ?? RibbonItemSize.Medium;

            // The group's density caps what an item may ask for, unless the item opted out.
            bool fixedSize = (child as RibbonItem)?.IsFixedSize ?? false;

            if (!fixedSize)
            {
                if (cap == RibbonGroupSizeMode.Small)
                    size = RibbonItemSize.Small;
                else if (cap == RibbonGroupSizeMode.Medium && size == RibbonItemSize.Large)
                    size = RibbonItemSize.Medium;
            }

            int rowsNeeded = size switch
            {
                RibbonItemSize.Large => 3,
                RibbonItemSize.Medium => 2,
                _ => 1
            };

            int span = Math.Max(1, (child as RibbonItem)?.ColumnSpan ?? 1);

            // Templates switch on EffectiveSize, so a demoted Large item draws as a one-row button.
            if (child is RibbonItem item) item.SetEffectiveSize(size);

            int column = cursor;
            int row;

            while (true)
            {
                EnsureSlots(slots, column + span, columnWidth);

                int blockingSeparator = FirstSeparator(slots, column, span);
                if (blockingSeparator >= 0)
                {
                    column = blockingSeparator + 1;
                    continue;
                }

                row = FirstFreeBand(slots, column, span, rowsNeeded);
                if (row >= 0) break;

                // Nothing free here: try the next column. Slots are added on demand above, so this
                // loop always terminates — each iteration either places the item or moves right.
                column++;
            }

            double itemWidth = 0;
            for (int i = 0; i < span; i++) itemWidth += slots[column + i].Width;

            double top = row * (rowHeight + gap);
            double itemHeight = rowsNeeded * rowHeight + (rowsNeeded - 1) * gap;

            place(child, new Rect(Left(slots, column), top, itemWidth, itemHeight));

            for (int i = 0; i < span; i++)
            {
                for (int r = row; r < row + rowsNeeded; r++) slots[column + i].Used[r] = true;
            }

            // Stay in this column so the next item can use the rows left below it: two wide inputs
            // end up stacked in the same band, which keeps the group narrow.
            cursor = column;
        }

        width = Left(slots, slots.Count);
    }

    private static void EnsureSlots(List<Slot> slots, int count, double columnWidth)
    {
        while (slots.Count < count) slots.Add(new Slot { Width = columnWidth });
    }

    private static int FirstSeparator(List<Slot> slots, int column, int span)
    {
        int end = Math.Min(slots.Count, column + span);

        for (int i = column; i < end; i++)
        {
            if (slots[i].IsSeparator) return i;
        }

        return -1;
    }

    private static int FirstFreeBand(List<Slot> slots, int column, int span, int rows)
    {
        for (int row = 0; row + rows <= RowCount; row++)
        {
            bool free = true;

            for (int i = 0; i < span && free; i++)
            {
                bool[] used = slots[column + i].Used;

                for (int r = row; r < row + rows; r++)
                {
                    if (!used[r]) continue;
                    free = false;
                    break;
                }
            }

            if (free) return row;
        }

        return -1;
    }

    private static double Left(List<Slot> slots, int index)
    {
        double left = 0;
        for (int i = 0; i < index && i < slots.Count; i++) left += slots[i].Width;
        return left;
    }
}
