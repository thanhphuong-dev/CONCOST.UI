using System;
using System.Windows;
using System.Windows.Controls;

namespace CONCOST.UI.Controls;

/// <summary>
/// A waterfall panel: every child keeps its own height and drops into the shortest column, so a
/// gallery of cards of unequal height reads as a wall instead of a grid with ragged gaps.
/// </summary>
/// <remarks>
/// <para>
/// Use it as the <c>ItemsPanel</c> of an <see cref="ItemsControl"/>, or put children in it directly.
/// A virtualising panel cannot balance columns — it does not know a container's height before it is
/// realised — so this panel measures every child. That is the right trade for a photo wall or a
/// dashboard of a few dozen cards, and the wrong one for an unbounded list; use a
/// <see cref="VirtualizingStackPanel"/> there.
/// </para>
/// <para>
/// The target column is the one with the smallest bottom edge *at the moment the child is placed*,
/// not the one with the fewest children. A tall card in column 0 therefore pushes the next two cards
/// into column 1, which is what keeps the bottom edge of the wall roughly level.
/// </para>
/// </remarks>
public class MasonryPanel : Panel
{
    /// <summary>Child heights from the last measure pass, in pixels.</summary>
    private double[] _heights = Array.Empty<double>();

    public static readonly DependencyProperty ColumnsProperty =
        DependencyProperty.Register(nameof(Columns), typeof(int), typeof(MasonryPanel),
            new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsMeasure));

    /// <summary>
    /// Number of columns. <c>0</c> (the default) fits as many columns of at least
    /// <see cref="MinColumnWidth"/> as the width allows.
    /// </summary>
    public int Columns
    {
        get => (int)GetValue(ColumnsProperty);
        set => SetValue(ColumnsProperty, value);
    }

    public static readonly DependencyProperty MinColumnWidthProperty =
        DependencyProperty.Register(nameof(MinColumnWidth), typeof(double), typeof(MasonryPanel),
            new FrameworkPropertyMetadata(220.0, FrameworkPropertyMetadataOptions.AffectsMeasure));

    /// <summary>Narrowest a column may become when <see cref="Columns"/> is left at 0. Default 220.</summary>
    public double MinColumnWidth
    {
        get => (double)GetValue(MinColumnWidthProperty);
        set => SetValue(MinColumnWidthProperty, value);
    }

    public static readonly DependencyProperty ColumnSpacingProperty =
        DependencyProperty.Register(nameof(ColumnSpacing), typeof(double), typeof(MasonryPanel),
            new FrameworkPropertyMetadata(12.0, FrameworkPropertyMetadataOptions.AffectsMeasure));

    /// <summary>Gap between two columns. Default 12.</summary>
    public double ColumnSpacing
    {
        get => (double)GetValue(ColumnSpacingProperty);
        set => SetValue(ColumnSpacingProperty, value);
    }

    public static readonly DependencyProperty RowSpacingProperty =
        DependencyProperty.Register(nameof(RowSpacing), typeof(double), typeof(MasonryPanel),
            new FrameworkPropertyMetadata(12.0, FrameworkPropertyMetadataOptions.AffectsMeasure));

    /// <summary>Gap between two cards stacked in the same column. Default 12.</summary>
    public double RowSpacing
    {
        get => (double)GetValue(RowSpacingProperty);
        set => SetValue(RowSpacingProperty, value);
    }

    public static readonly DependencyProperty IsBalancedProperty =
        DependencyProperty.Register(nameof(IsBalanced), typeof(bool), typeof(MasonryPanel),
            new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsArrange));

    /// <summary>
    /// When <c>true</c> (the default) each card goes to the shortest column. When <c>false</c> the
    /// cards stay in reading order and fill column 0 up to its share of the total height before
    /// column 1 starts, which suits a list that is scanned rather than looked at.
    /// </summary>
    public bool IsBalanced
    {
        get => (bool)GetValue(IsBalancedProperty);
        set => SetValue(IsBalancedProperty, value);
    }

    /// <summary>
    /// The number of columns <paramref name="availableWidth"/> produces. Exposed because a host that
    /// virtualises its own items needs the same answer the layout uses.
    /// </summary>
    public int GetColumnCount(double availableWidth)
    {
        if (Columns > 0) return Math.Max(1, Columns);

        double width = double.IsInfinity(availableWidth) || availableWidth <= 0 ? MinColumnWidth : availableWidth;
        double spacing = Math.Max(0, ColumnSpacing);
        double step = Math.Max(1, MinColumnWidth) + spacing;

        // n columns occupy n*w + (n-1)*spacing, which is why the gap that precedes the last column is
        // added back before dividing.
        return Math.Max(1, (int)Math.Floor((width + spacing) / step));
    }

    /// <summary>Width of one column when the panel is <paramref name="availableWidth"/> wide.</summary>
    public double GetColumnWidth(double availableWidth)
    {
        int columns = GetColumnCount(availableWidth);
        double spacing = Math.Max(0, ColumnSpacing);
        double width = double.IsInfinity(availableWidth) || availableWidth <= 0
            ? columns * Math.Max(1, MinColumnWidth) + (columns - 1) * spacing
            : availableWidth;

        return Math.Max(0, (width - (columns - 1) * spacing) / columns);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        int columns = GetColumnCount(availableSize.Width);
        double columnWidth = GetColumnWidth(availableSize.Width);

        _heights = new double[InternalChildren.Count];

        for (int i = 0; i < InternalChildren.Count; i++)
        {
            // A card's height is its own business: measuring with an infinite height is what lets it
            // keep a portrait shape instead of being squashed to a row.
            InternalChildren[i].Measure(new Size(columnWidth, double.PositiveInfinity));
            _heights[i] = InternalChildren[i].DesiredSize.Height;
        }

        double height = Place(columns, out _, out _);

        double widest = double.IsInfinity(availableSize.Width)
            ? columns * columnWidth + (columns - 1) * Math.Max(0, ColumnSpacing)
            : availableSize.Width;

        return new Size(widest, height);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        int columns = GetColumnCount(finalSize.Width);
        double columnWidth = GetColumnWidth(finalSize.Width);
        double spacing = Math.Max(0, ColumnSpacing);

        _ = Place(columns, out int[] columnOf, out double[] top);

        for (int i = 0; i < InternalChildren.Count; i++)
        {
            UIElement child = InternalChildren[i];

            child.Arrange(new Rect(columnOf[i] * (columnWidth + spacing), top[i], columnWidth,
                child.DesiredSize.Height));
        }

        return finalSize;
    }

    /// <summary>
    /// Assigns every child to a column and returns the total height of the wall. Pure apart from the
    /// cached heights, so measure and arrange agree on the same answer.
    /// </summary>
    private double Place(int columns, out int[] columnOf, out double[] top)
    {
        int count = _heights.Length;
        columnOf = new int[count];
        top = new double[count];

        if (count == 0) return 0;

        double spacing = Math.Max(0, RowSpacing);
        var bottoms = new double[columns];
        double height = 0;

        if (IsBalanced)
        {
            for (int i = 0; i < count; i++)
            {
                int column = ShortestColumn(bottoms);
                double y = bottoms[column] > 0 ? bottoms[column] + spacing : 0;

                columnOf[i] = column;
                top[i] = y;
                bottoms[column] = y + _heights[i];
                height = Math.Max(height, bottoms[column]);
            }

            return height;
        }

        // Reading order: each column gets roughly 1/columns of the total, then the next one starts.
        // Comparing against that target instead of a fixed count keeps a page of mixed heights level.
        double total = 0;
        foreach (double h in _heights) total += h;
        total += spacing * (count - 1);

        double target = total / columns;
        int current = 0;
        double cursor = 0;

        for (int i = 0; i < count; i++)
        {
            if (current < columns - 1 && cursor > 0 && cursor + _heights[i] > target)
            {
                current++;
                cursor = 0;
            }

            columnOf[i] = current;
            top[i] = cursor;
            cursor += _heights[i] + spacing;
            height = Math.Max(height, cursor - spacing);
        }

        return height;
    }

    /// <summary>
    /// Picks the shortest column. Strictly smaller, so a tie always resolves to the left column and the
    /// result stays predictable for a given set of heights.
    /// </summary>
    private static int ShortestColumn(double[] bottoms)
    {
        int best = 0;

        for (int i = 1; i < bottoms.Length; i++)
        {
            if (bottoms[i] < bottoms[best] - 0.001) best = i;
        }

        return best;
    }
}
