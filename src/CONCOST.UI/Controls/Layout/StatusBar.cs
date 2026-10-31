using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CONCOST.UI.Icons;

namespace CONCOST.UI.Controls;

/// <summary>Side of the bar a <see cref="StatusBarItem"/> sits on.</summary>
public enum StatusBarAlignment
{
    Left,
    Right
}

/// <summary>
/// One cell of a <see cref="StatusBar"/>: an optional icon, a caption, and optionally a command.
/// </summary>
public class StatusBarItem : ContentControl
{
    static StatusBarItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StatusBarItem),
            new FrameworkPropertyMetadata(typeof(StatusBarItem)));
    }

    public static readonly DependencyProperty IconProperty =
        DependencyProperty.Register(nameof(Icon), typeof(IconKind), typeof(StatusBarItem),
            new PropertyMetadata(IconKind.None));

    /// <summary>Glyph before the caption.</summary>
    public IconKind Icon
    {
        get => (IconKind)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(nameof(Text), typeof(string), typeof(StatusBarItem),
            new PropertyMetadata(string.Empty));

    /// <summary>Caption. Ignored when <see cref="ContentControl.Content"/> is set.</summary>
    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public static readonly DependencyProperty IsSeparatorProperty =
        DependencyProperty.Register(nameof(IsSeparator), typeof(bool), typeof(StatusBarItem),
            new PropertyMetadata(false));

    /// <summary>Draws a vertical hairline instead of content.</summary>
    public bool IsSeparator
    {
        get => (bool)GetValue(IsSeparatorProperty);
        set => SetValue(IsSeparatorProperty, value);
    }

    public static readonly DependencyProperty AlignmentProperty =
        DependencyProperty.Register(nameof(Alignment), typeof(StatusBarAlignment), typeof(StatusBarItem),
            new FrameworkPropertyMetadata(StatusBarAlignment.Left, FrameworkPropertyMetadataOptions.AffectsParentArrange));

    /// <summary>Which side of the bar the cell hugs. Default <see cref="StatusBarAlignment.Left"/>.</summary>
    public StatusBarAlignment Alignment
    {
        get => (StatusBarAlignment)GetValue(AlignmentProperty);
        set => SetValue(AlignmentProperty, value);
    }

    public static readonly DependencyProperty CommandProperty =
        DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(StatusBarItem),
            new PropertyMetadata(null));

    /// <summary>Command run when the cell is clicked.</summary>
    public ICommand? Command
    {
        get => (ICommand?)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public static readonly DependencyProperty CommandParameterProperty =
        DependencyProperty.Register(nameof(CommandParameter), typeof(object), typeof(StatusBarItem),
            new PropertyMetadata(null));

    /// <summary>Parameter passed to <see cref="Command"/>.</summary>
    public object? CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    public StatusBarItem()
    {
        Focusable = false;
    }

    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonUp(e);

        if (Command is null) return;
        if (!Command.CanExecute(CommandParameter)) return;

        Command.Execute(CommandParameter);
        e.Handled = true;
    }
}

/// <summary>
/// Arranges status cells from both ends of the bar: left-aligned cells stack from the left,
/// right-aligned ones from the right, and the gap in the middle takes the slack.
/// </summary>
public class StatusBarPanel : Panel
{
    public static readonly DependencyProperty SpacingProperty =
        DependencyProperty.Register(nameof(Spacing), typeof(double), typeof(StatusBarPanel),
            new FrameworkPropertyMetadata(10.0, FrameworkPropertyMetadataOptions.AffectsMeasure));

    /// <summary>Space between two cells, in pixels. Default 10.</summary>
    public double Spacing
    {
        get => (double)GetValue(SpacingProperty);
        set => SetValue(SpacingProperty, value);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        double width = 0;
        double height = 0;

        foreach (UIElement child in InternalChildren)
        {
            child.Measure(new Size(double.PositiveInfinity, availableSize.Height));
            width += child.DesiredSize.Width;
            height = Math.Max(height, child.DesiredSize.Height);
        }

        double spacing = Math.Max(0, Spacing) * Math.Max(0, InternalChildren.Count - 1);
        double total = width + spacing;

        return new Size(double.IsInfinity(availableSize.Width) ? total : Math.Min(total, availableSize.Width), height);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        double spacing = Math.Max(0, Spacing);
        double left = 0;
        double right = finalSize.Width;
        bool firstLeft = true;
        bool firstRight = true;

        foreach (UIElement child in InternalChildren)
        {
            var size = child.DesiredSize;
            bool isRight = child is StatusBarItem item && item.Alignment == StatusBarAlignment.Right;

            if (isRight)
            {
                if (!firstRight) right -= spacing;
                right -= size.Width;
                child.Arrange(new Rect(right, 0, size.Width, finalSize.Height));
                firstRight = false;
            }
            else
            {
                if (!firstLeft) left += spacing;
                child.Arrange(new Rect(left, 0, size.Width, finalSize.Height));
                left += size.Width;
                firstLeft = false;
            }
        }

        return finalSize;
    }
}

/// <summary>
/// The strip along the bottom of a window: left-aligned cells for state, right-aligned ones for
/// zoom, units and counts.
/// </summary>
public class StatusBar : ItemsControl
{
    static StatusBar()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StatusBar),
            new FrameworkPropertyMetadata(typeof(StatusBar)));
    }

    public static readonly DependencyProperty SpacingProperty =
        DependencyProperty.Register(nameof(Spacing), typeof(double), typeof(StatusBar),
            new FrameworkPropertyMetadata(10.0));

    /// <summary>Space between two cells, in pixels. Default 10.</summary>
    public double Spacing
    {
        get => (double)GetValue(SpacingProperty);
        set => SetValue(SpacingProperty, value);
    }
}
