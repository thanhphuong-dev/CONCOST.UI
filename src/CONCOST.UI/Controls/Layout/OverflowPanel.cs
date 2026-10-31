using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace CONCOST.UI.Controls;

/// <summary>
/// A single-row panel that lays items out left to right and stops when the width runs out.
/// </summary>
/// <remarks>
/// Items that do not fit are arranged with an empty rect rather than being collapsed: collapsing would
/// change their measured size on the next pass and the count would flicker. <see cref="OverflowCount"/>
/// tells the host how many were pushed out so it can show a "more" button and repeat them in a menu.
/// </remarks>
public class OverflowPanel : Panel
{
    private static readonly DependencyPropertyKey OverflowCountPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(OverflowCount), typeof(int), typeof(OverflowPanel),
            new PropertyMetadata(0));

    public static readonly DependencyProperty OverflowCountProperty = OverflowCountPropertyKey.DependencyProperty;

    /// <summary>How many items did not fit in the last layout.</summary>
    public int OverflowCount => (int)GetValue(OverflowCountProperty);

    /// <summary>Space between items, in pixels.</summary>
    public static readonly DependencyProperty SpacingProperty =
        DependencyProperty.Register(nameof(Spacing), typeof(double), typeof(OverflowPanel),
            new FrameworkPropertyMetadata(2.0, FrameworkPropertyMetadataOptions.AffectsMeasure));

    public double Spacing
    {
        get => (double)GetValue(SpacingProperty);
        set => SetValue(SpacingProperty, value);
    }

    /// <summary>Number of leading children that fit.</summary>
    public int VisibleCount { get; private set; }

    protected override Size MeasureOverride(Size availableSize)
    {
        double spacing = Math.Max(0, Spacing);
        double total = 0;
        double tallest = 0;
        int visible = 0;
        double limit = double.IsInfinity(availableSize.Width) ? double.PositiveInfinity : availableSize.Width;

        for (int i = 0; i < InternalChildren.Count; i++)
        {
            var child = InternalChildren[i];
            child.Measure(new Size(double.PositiveInfinity, availableSize.Height));

            double next = total + (i == 0 ? 0 : spacing) + child.DesiredSize.Width;
            if (next <= limit || visible == 0) visible++;

            total = next;
            tallest = Math.Max(tallest, child.DesiredSize.Height);
        }

        VisibleCount = visible;

        // Width is reported as the full desired width; the parent clips and the overflow button takes
        // its own column, so the panel never asks for more than it was given.
        double width = double.IsInfinity(availableSize.Width) ? total : Math.Min(total, availableSize.Width);
        SetValue(OverflowCountPropertyKey, Math.Max(0, InternalChildren.Count - visible));

        return new Size(width, tallest);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        double spacing = Math.Max(0, Spacing);
        double x = 0;

        for (int i = 0; i < InternalChildren.Count; i++)
        {
            var child = InternalChildren[i];

            if (i >= VisibleCount)
            {
                child.Arrange(new Rect(0, 0, 0, 0));
                continue;
            }

            if (i > 0) x += spacing;

            var size = child.DesiredSize;
            child.Arrange(new Rect(x, 0, size.Width, Math.Max(size.Height, finalSize.Height)));
            x += size.Width;
        }

        return finalSize;
    }
}
