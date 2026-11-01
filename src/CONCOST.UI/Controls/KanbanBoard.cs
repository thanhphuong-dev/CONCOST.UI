using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CONCOST.UI.Controls;

/// <summary>
/// A card on a <see cref="KanbanBoard"/>. It is a <see cref="ContentControl"/>, so a host can put
/// anything below the title without teaching the board about it.
/// </summary>
public class KanbanCard : ContentControl
{
    /// <summary>Identifies the <see cref="Title"/> property.</summary>
    public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
        nameof(Title), typeof(string), typeof(KanbanCard), new PropertyMetadata(string.Empty));

    /// <summary>Identifies the <see cref="Tag"/> property.</summary>
    public static readonly DependencyProperty ChipProperty = DependencyProperty.Register(
        nameof(Chip), typeof(string), typeof(KanbanCard), new PropertyMetadata(string.Empty));

    /// <summary>Identifies the <see cref="Accent"/> property.</summary>
    public static readonly DependencyProperty AccentProperty = DependencyProperty.Register(
        nameof(Accent), typeof(Brush), typeof(KanbanCard), new PropertyMetadata(null));

    /// <summary>Identifies the <see cref="IsDone"/> property.</summary>
    public static readonly DependencyProperty IsDoneProperty = DependencyProperty.Register(
        nameof(IsDone), typeof(bool), typeof(KanbanCard), new PropertyMetadata(false));

    private Point _pressPoint;
    private bool _pressed;

    static KanbanCard()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(KanbanCard), new FrameworkPropertyMetadata(typeof(KanbanCard)));
    }

    /// <summary>Short caption at the top of the card.</summary>
    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>Chip text — a discipline, a floor, an owner.</summary>
    public string Chip
    {
        get => (string)GetValue(ChipProperty);
        set => SetValue(ChipProperty, value);
    }

    /// <summary>Optional colour for the chip and the stripe down the card.</summary>
    public Brush? Accent
    {
        get => (Brush?)GetValue(AccentProperty);
        set => SetValue(AccentProperty, value);
    }

    /// <summary>Dimmed when set, which a "done" column uses to read at a glance.</summary>
    public bool IsDone
    {
        get => (bool)GetValue(IsDoneProperty);
        set => SetValue(IsDoneProperty, value);
    }

    protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnPreviewMouseLeftButtonDown(e);

        _pressPoint = e.GetPosition(this);
        _pressed = true;
        Focus();
    }

    protected override void OnPreviewMouseMove(MouseEventArgs e)
    {
        base.OnPreviewMouseMove(e);

        if (!_pressed || e.LeftButton != MouseButtonState.Pressed) return;

        var position = e.GetPosition(this);
        if (Math.Abs(position.X - _pressPoint.X) < SystemParameters.MinimumHorizontalDragDistance &&
            Math.Abs(position.Y - _pressPoint.Y) < SystemParameters.MinimumVerticalDragDistance)
        {
            return;
        }

        _pressed = false;

        var data = new DataObject(typeof(KanbanCard), this);
        DragDrop.DoDragDrop(this, data, DragDropEffects.Move);
    }

    protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        base.OnPreviewMouseLeftButtonUp(e);
        _pressed = false;
    }
}

/// <summary>A lane of a <see cref="KanbanBoard"/>: a header, a count and its cards.</summary>
public class KanbanColumn : HeaderedItemsControl
{
    /// <summary>Identifies the <see cref="Accent"/> property.</summary>
    public static readonly DependencyProperty AccentProperty = DependencyProperty.Register(
        nameof(Accent), typeof(Brush), typeof(KanbanColumn), new PropertyMetadata(null));

    /// <summary>Identifies the <see cref="IsDropTarget"/> property.</summary>
    public static readonly DependencyProperty IsDropTargetProperty = DependencyProperty.Register(
        nameof(IsDropTarget), typeof(bool), typeof(KanbanColumn), new PropertyMetadata(false));

    /// <summary>Identifies the <see cref="Maximum"/> property.</summary>
    public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register(
        nameof(Maximum), typeof(int), typeof(KanbanColumn), new PropertyMetadata(0));

    static KanbanColumn()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(KanbanColumn), new FrameworkPropertyMetadata(typeof(KanbanColumn)));
    }

    public KanbanColumn()
    {
        AllowDrop = true;
    }

    /// <summary>Colour of the dot next to the header.</summary>
    public Brush? Accent
    {
        get => (Brush?)GetValue(AccentProperty);
        set => SetValue(AccentProperty, value);
    }

    /// <summary>Set while a card hovers over the lane, so the template can highlight it.</summary>
    public bool IsDropTarget
    {
        get => (bool)GetValue(IsDropTargetProperty);
        set => SetValue(IsDropTargetProperty, value);
    }

    /// <summary>Soft limit shown next to the count; zero means no limit.</summary>
    public int Maximum
    {
        get => (int)GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    /// <summary>Number of cards in the lane.</summary>
    public int Count => Items.Count;

    protected override void OnDragOver(DragEventArgs e)
    {
        base.OnDragOver(e);

        if (!e.Data.GetDataPresent(typeof(KanbanCard)))
        {
            e.Effects = DragDropEffects.None;
            e.Handled = true;
            return;
        }

        e.Effects = Maximum > 0 && Count >= Maximum ? DragDropEffects.None : DragDropEffects.Move;
        IsDropTarget = e.Effects != DragDropEffects.None;
        e.Handled = true;
    }

    protected override void OnDragLeave(DragEventArgs e)
    {
        base.OnDragLeave(e);
        IsDropTarget = false;
    }

    protected override void OnDrop(DragEventArgs e)
    {
        base.OnDrop(e);

        IsDropTarget = false;
        e.Handled = true;

        if (e.Data.GetData(typeof(KanbanCard)) is not KanbanCard card) return;

        var board = FindBoard(this);
        board?.MoveCard(card, this, Count);
    }

    internal static KanbanBoard? FindBoard(DependencyObject? source)
    {
        while (source is not null)
        {
            if (source is KanbanBoard board) return board;
            source = source is Visual or System.Windows.Media.Media3D.Visual3D
                ? VisualTreeHelper.GetParent(source)
                : LogicalTreeHelper.GetParent(source);
        }

        return null;
    }
}

/// <summary>
/// A board of lanes and draggable cards — the "to do / doing / done" wall.
/// </summary>
/// <remarks>
/// <para>
/// The board owns the move, not the lane: a drop resolves the card, the lane and the target index and
/// then hands them to <see cref="MoveCard"/>, which detaches the card from wherever it was. Lanes
/// therefore need no knowledge of each other, and a host can drive the same move from a menu item or
/// a keyboard shortcut without faking a drag.
/// </para>
/// <para>
/// <see cref="KanbanColumn.Maximum"/> is a soft limit: the lane refuses drops once it is full, but a
/// programmatic <see cref="MoveCard"/> still goes through, because a validation rule belongs in the
/// view model rather than in the drag handler.
/// </para>
/// </remarks>
public class KanbanBoard : Control
{
    static KanbanBoard()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(KanbanBoard), new FrameworkPropertyMetadata(typeof(KanbanBoard)));
    }

    /// <summary>Identifies the <see cref="Columns"/> property.</summary>
    public static readonly DependencyProperty ColumnsProperty = DependencyProperty.Register(
        nameof(Columns), typeof(ObservableCollection<KanbanColumn>), typeof(KanbanBoard),
        new PropertyMetadata(null));

    /// <summary>Raised after a card changed lane or position.</summary>
    public event EventHandler<KanbanCardMovedEventArgs>? CardMoved;

    /// <summary>The lanes of the board, left to right.</summary>
    public ObservableCollection<KanbanColumn> Columns
    {
        get
        {
            if (GetValue(ColumnsProperty) is not ObservableCollection<KanbanColumn> columns)
            {
                columns = new ObservableCollection<KanbanColumn>();
                SetValue(ColumnsProperty, columns);
            }

            return columns;
        }

        set => SetValue(ColumnsProperty, value);
    }

    /// <summary>
    /// Moves <paramref name="card"/> to <paramref name="target"/> at <paramref name="index"/>,
    /// taking it out of whatever lane held it. Returns <c>true</c> when the card moved.
    /// </summary>
    public bool MoveCard(KanbanCard card, KanbanColumn target, int index)
    {
        if (card is null || target is null) return false;

        KanbanColumn? source = null;
        int from = -1;

        foreach (var column in Columns)
        {
            int at = column.Items.IndexOf(card);
            if (at < 0) continue;

            source = column;
            from = at;
            break;
        }

        int to = Math.Max(0, Math.Min(index, target.Items.Count));

        if (ReferenceEquals(source, target))
        {
            if (from == to) return false;

            target.Items.RemoveAt(from);
            if (to > from) to--;
            target.Items.Insert(to, card);
        }
        else
        {
            source?.Items.Remove(card);
            to = Math.Max(0, Math.Min(to, target.Items.Count));
            target.Items.Insert(to, card);
        }

        CardMoved?.Invoke(this, new KanbanCardMovedEventArgs(card, source, target, from, to));
        return true;
    }

    /// <summary>Finds the lane that currently holds <paramref name="card"/>.</summary>
    public KanbanColumn? ColumnOf(KanbanCard card)
    {
        foreach (var column in Columns)
        {
            if (column.Items.Contains(card)) return column;
        }

        return null;
    }
}

/// <summary>Describes a card that changed lane or position.</summary>
public class KanbanCardMovedEventArgs : EventArgs
{
    public KanbanCardMovedEventArgs(KanbanCard card, KanbanColumn? fromColumn, KanbanColumn toColumn, int fromIndex, int toIndex)
    {
        Card = card;
        FromColumn = fromColumn;
        ToColumn = toColumn;
        FromIndex = fromIndex;
        ToIndex = toIndex;
    }

    /// <summary>The card that moved.</summary>
    public KanbanCard Card { get; }

    /// <summary>The lane it came from, or <c>null</c> when it was not on the board yet.</summary>
    public KanbanColumn? FromColumn { get; }

    /// <summary>The lane it landed in.</summary>
    public KanbanColumn ToColumn { get; }

    /// <summary>Its old position, or <c>-1</c>.</summary>
    public int FromIndex { get; }

    /// <summary>Its new position.</summary>
    public int ToIndex { get; }
}
