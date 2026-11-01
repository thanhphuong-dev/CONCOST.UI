using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace CONCOST.UI.Controls;

/// <summary>One booking on a <see cref="Scheduler"/> day.</summary>
public class SchedulerItem
{
    /// <summary>Caption shown inside the block.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Start of the booking; only the time of day is used for the block position.</summary>
    public DateTime Start { get; set; }

    /// <summary>End of the booking. An end before the start is drawn as the shortest visible block.</summary>
    public DateTime End { get; set; }

    /// <summary>Optional lane label, shown in the tooltip the control builds for the block.</summary>
    public string? Resource { get; set; }

    /// <summary>Block colour. Falls back to the control's <see cref="Scheduler.ItemBrush"/>.</summary>
    public Brush? Accent { get; set; }

    /// <summary>Anything the host wants to hang off the booking — an id, a model object.</summary>
    public object? Tag { get; set; }

    /// <summary>Length of the booking, floored at three minutes so it always has a visible block.</summary>
    public TimeSpan Duration => End > Start ? End - Start : TimeSpan.FromMinutes(3);

    public override string ToString() => string.IsNullOrEmpty(Title) ? Start.ToString("t", CultureInfo.CurrentCulture) : Title;
}

/// <summary>Reports the booking a <see cref="Scheduler"/> selected or cleared.</summary>
public class SchedulerSelectionChangedEventArgs : EventArgs
{
    public SchedulerSelectionChangedEventArgs(SchedulerItem? item) => Item = item;

    /// <summary>The booking now selected, or <c>null</c> when the selection was cleared.</summary>
    public SchedulerItem? Item { get; }
}

/// <summary>
/// A day timeline: hours run down the left edge, bookings are placed by their time of day and
/// overlapping bookings share the column rather than covering each other.
/// </summary>
/// <remarks>
/// <para>
/// The layout is computed twice from the same method — once for painting, once for hit testing —
/// so a click can never disagree with the block the pointer is over. Both passes clamp to
/// <see cref="StartHour"/>..<see cref="EndHour"/> so a booking that runs past midnight still lands
/// on the strip instead of being drawn off the bottom.
/// </para>
/// <para>
/// Overlap uses a greedy lane packer: each booking takes the first lane that is free at its start,
/// and every booking in the same overlap cluster gets <c>1 / laneCount</c> of the width. That is the
/// behaviour of an agenda view, and it keeps the maths to one sort plus one pass.
/// </para>
/// </remarks>
public class Scheduler : FrameworkElement
{
    private const double HeaderHeight = 34;
    private const double GutterWidth = 56;

    /// <summary>Identifies the <see cref="ItemsSource"/> property.</summary>
    public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
        nameof(ItemsSource), typeof(IEnumerable), typeof(Scheduler),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Identifies the <see cref="Date"/> property.</summary>
    public static readonly DependencyProperty DateProperty = DependencyProperty.Register(
        nameof(Date), typeof(DateTime), typeof(Scheduler),
        new FrameworkPropertyMetadata(DateTime.Today, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Identifies the <see cref="StartHour"/> property.</summary>
    public static readonly DependencyProperty StartHourProperty = DependencyProperty.Register(
        nameof(StartHour), typeof(double), typeof(Scheduler),
        new FrameworkPropertyMetadata(7d, FrameworkPropertyMetadataOptions.AffectsRender, OnRangeChanged));

    /// <summary>Identifies the <see cref="EndHour"/> property.</summary>
    public static readonly DependencyProperty EndHourProperty = DependencyProperty.Register(
        nameof(EndHour), typeof(double), typeof(Scheduler),
        new FrameworkPropertyMetadata(20d, FrameworkPropertyMetadataOptions.AffectsRender, OnRangeChanged));

    /// <summary>Identifies the <see cref="HourHeight"/> property.</summary>
    public static readonly DependencyProperty HourHeightProperty = DependencyProperty.Register(
        nameof(HourHeight), typeof(double), typeof(Scheduler),
        new FrameworkPropertyMetadata(46d, FrameworkPropertyMetadataOptions.AffectsRender, OnRangeChanged));

    /// <summary>Identifies the <see cref="SelectedItem"/> property.</summary>
    public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(
        nameof(SelectedItem), typeof(SchedulerItem), typeof(Scheduler),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Identifies the <see cref="ItemBrush"/> property.</summary>
    public static readonly DependencyProperty ItemBrushProperty = DependencyProperty.Register(
        nameof(ItemBrush), typeof(Brush), typeof(Scheduler),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Identifies the <see cref="ShowNowLine"/> property.</summary>
    public static readonly DependencyProperty ShowNowLineProperty = DependencyProperty.Register(
        nameof(ShowNowLine), typeof(bool), typeof(Scheduler),
        new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Raised when the selected booking changes; the item is <c>null</c> when cleared.</summary>
    public event EventHandler<SchedulerSelectionChangedEventArgs>? SelectionChanged;

    /// <summary>Raised on a double click on a booking, which a host reads as "open it".</summary>
    public event EventHandler<SchedulerItem>? ItemActivated;

    /// <summary>The bookings to draw. Anything that is not a <see cref="SchedulerItem"/> is ignored.</summary>
    public IEnumerable? ItemsSource
    {
        get => (IEnumerable?)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    /// <summary>The day the timeline shows. Only the date part matters.</summary>
    public DateTime Date
    {
        get => (DateTime)GetValue(DateProperty);
        set => SetValue(DateProperty, value);
    }

    /// <summary>Hour the timeline starts at.</summary>
    public double StartHour
    {
        get => (double)GetValue(StartHourProperty);
        set => SetValue(StartHourProperty, value);
    }

    /// <summary>Hour the timeline ends at.</summary>
    public double EndHour
    {
        get => (double)GetValue(EndHourProperty);
        set => SetValue(EndHourProperty, value);
    }

    /// <summary>Pixels per hour.</summary>
    public double HourHeight
    {
        get => (double)GetValue(HourHeightProperty);
        set => SetValue(HourHeightProperty, value);
    }

    /// <summary>The booking the user picked, or <c>null</c>.</summary>
    public SchedulerItem? SelectedItem
    {
        get => (SchedulerItem?)GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    /// <summary>Default block colour.</summary>
    public Brush? ItemBrush
    {
        get => (Brush?)GetValue(ItemBrushProperty);
        set => SetValue(ItemBrushProperty, value);
    }

    /// <summary>Draws a red line at the current time when <see cref="Date"/> is today.</summary>
    public bool ShowNowLine
    {
        get => (bool)GetValue(ShowNowLineProperty);
        set => SetValue(ShowNowLineProperty, value);
    }

    /// <summary>
    /// Selects a booking as a click would, raising <see cref="SelectionChanged"/>. Passing
    /// <c>null</c> clears the selection.
    /// </summary>
    public void SelectItem(SchedulerItem? item)
    {
        if (ReferenceEquals(item, SelectedItem)) return;

        SetCurrentValue(SelectedItemProperty, item);
        SelectionChanged?.Invoke(this, new SchedulerSelectionChangedEventArgs(item));
    }

    /// <summary>The rectangle a booking occupies, or <see cref="Rect.Empty"/> when it is off the day.</summary>
    public Rect GetItemBounds(SchedulerItem item) => FindBounds(BuildLayout(), item);

    /// <summary>Moves the timeline by <paramref name="days"/> days; the date part is normalised.</summary>
    public void ChangeDay(int days) => Date = Date.Date.AddDays(days);

    protected override Size MeasureOverride(Size availableSize)
    {
        double height = HeaderHeight + SpanHours * Math.Max(1, HourHeight);
        return new Size(double.IsInfinity(availableSize.Width) ? 0 : availableSize.Width, height);
    }

    private double SpanHours
    {
        get
        {
            double span = EndHour - StartHour;
            return span <= 0 ? 1 : span;
        }
    }

    private static void OnRangeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Scheduler scheduler) scheduler.InvalidateMeasure();
    }

    private List<(SchedulerItem Item, Rect Bounds)> BuildLayout()
    {
        var result = new List<(SchedulerItem, Rect)>();

        if (ActualWidth <= 0 || ActualHeight <= 0) return result;

        double span = SpanHours;
        double usableWidth = Math.Max(0, ActualWidth - GutterWidth);
        if (usableWidth <= 0) return result;

        var dayStart = Date.Date.AddHours(StartHour);
        var dayEnd = Date.Date.AddHours(StartHour + span);

        var visible = Items.Where(i => i.End > dayStart && i.Start < dayEnd)
            .OrderBy(i => i.Start)
            .ToList();

        // Greedy lane packing: `laneEnds[l]` is when lane l becomes free again.
        var laneEnds = new List<DateTime>();
        var lanes = new Dictionary<SchedulerItem, int>();

        foreach (var item in visible)
        {
            var start = Clamp(item.Start, dayStart, dayEnd);
            int lane = laneEnds.FindIndex(end => end <= start);

            if (lane < 0)
            {
                lane = laneEnds.Count;
                laneEnds.Add(default);
            }

            laneEnds[lane] = Clamp(item.End, dayStart, dayEnd);
            lanes[item] = lane;
        }

        // A cluster is a run of bookings that share lanes; each cluster divides the width by its
        // own lane count so a lone booking still gets the full strip.
        foreach (var item in visible)
        {
            var start = Clamp(item.Start, dayStart, dayEnd);
            var end = Clamp(item.End, dayStart, dayEnd);
            if (end <= start) end = start.AddMinutes(3);

            int laneCount = ClusterLaneCount(visible, lanes, item, dayStart, dayEnd);
            int lane = lanes[item];

            double top = HeaderHeight + (start - dayStart).TotalHours * HourHeight;
            double height = Math.Max(4, (end - start).TotalHours * HourHeight);
            double width = usableWidth / laneCount;

            var bounds = new Rect(
                GutterWidth + lane * width + 2,
                top + 1,
                Math.Max(8, width - 4),
                Math.Max(4, height - 2));

            result.Add((item, bounds));
        }

        return result;
    }

    private static int ClusterLaneCount(
        List<SchedulerItem> visible,
        Dictionary<SchedulerItem, int> lanes,
        SchedulerItem item,
        DateTime dayStart,
        DateTime dayEnd)
    {
        var start = Clamp(item.Start, dayStart, dayEnd);
        var end = Clamp(item.End, dayStart, dayEnd);

        int count = lanes[item] + 1;

        foreach (var other in visible)
        {
            if (ReferenceEquals(other, item)) continue;

            var otherStart = Clamp(other.Start, dayStart, dayEnd);
            var otherEnd = Clamp(other.End, dayStart, dayEnd);

            bool overlaps = otherStart < end && other.End > start;
            if (overlaps) count = Math.Max(count, lanes[other] + 1);
        }

        return Math.Max(1, count);
    }

    private static DateTime Clamp(DateTime value, DateTime min, DateTime max) =>
        value < min ? min : value > max ? max : value;

    private IEnumerable<SchedulerItem> Items =>
        (ItemsSource as IEnumerable)?.OfType<SchedulerItem>() ?? Enumerable.Empty<SchedulerItem>();

    private static Rect FindBounds(List<(SchedulerItem Item, Rect Bounds)> layout, SchedulerItem item)
    {
        foreach (var entry in layout)
        {
            if (ReferenceEquals(entry.Item, item)) return entry.Bounds;
        }

        return Rect.Empty;
    }

    protected override void OnRender(DrawingContext dc)
    {
        var surface = TryFindResource("CONCOST.Brush.Surface") as Brush ?? Brushes.White;
        var line = TryFindResource("CONCOST.Brush.Border") as Brush ?? Brushes.Gainsboro;
        var muted = TryFindResource("CONCOST.Brush.TextSecondary") as Brush ?? Brushes.Gray;
        var text = TryFindResource("CONCOST.Brush.TextPrimary") as Brush ?? Brushes.Black;
        var accent = ItemBrush ?? TryFindResource("CONCOST.Brush.Primary") as Brush ?? Brushes.SteelBlue;

        dc.DrawRectangle(surface, null, new Rect(RenderSize));

        // Header: the day being shown, so a screenshot of the control is self-explanatory.
        var header = new FormattedText(
            Date.ToString("dddd, dd/MM/yyyy", CultureInfo.CurrentCulture),
            CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            new Typeface("Segoe UI Semibold"),
            12,
            text,
            VisualTreeHelper.GetDpi(this).PixelsPerDip);

        dc.DrawText(header, new Point(10, (HeaderHeight - header.Height) / 2));
        dc.DrawLine(new Pen(line, 1), new Point(0, HeaderHeight), new Point(RenderSize.Width, HeaderHeight));

        var hours = new Pen(line, 1);
        var minor = new Pen(line, 0.5) { DashStyle = new DashStyle(new double[] { 2, 3 }, 0) };

        for (double hour = StartHour; hour <= EndHour + 0.001; hour += 1)
        {
            double y = HeaderHeight + (hour - StartHour) * HourHeight;
            if (y > RenderSize.Height) break;

            dc.DrawLine(hours, new Point(GutterWidth, y), new Point(RenderSize.Width, y));

            var label = new FormattedText(
                TimeSpan.FromHours(hour).ToString(@"hh\:mm"),
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface("Segoe UI"),
                10,
                muted,
                VisualTreeHelper.GetDpi(this).PixelsPerDip);

            dc.DrawText(label, new Point(10, y + 2));

            if (hour + 0.5 <= EndHour)
            {
                double half = HeaderHeight + (hour + 0.5 - StartHour) * HourHeight;
                dc.DrawLine(minor, new Point(GutterWidth, half), new Point(RenderSize.Width, half));
            }
        }

        var dpi = VisualTreeHelper.GetDpi(this).PixelsPerDip;

        foreach (var (item, bounds) in BuildLayout())
        {
            if (bounds.IsEmpty) continue;

            bool selected = ReferenceEquals(item, SelectedItem);
            Brush fill = item.Accent ?? accent;

            dc.DrawRoundedRectangle(fill, null, bounds, 4, 4);

            var title = new FormattedText(
                item.Title,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface("Segoe UI Semibold"),
                11,
                Brushes.White,
                dpi)
            {
                MaxTextWidth = Math.Max(0, bounds.Width - 12),
                MaxTextHeight = Math.Max(0, bounds.Height - 6),
                Trimming = TextTrimming.CharacterEllipsis
            };

            dc.DrawText(title, new Point(bounds.X + 6, bounds.Y + 3));

            if (bounds.Height > 34)
            {
                var range = new FormattedText(
                    string.Format(CultureInfo.CurrentCulture, "{0:HH:mm}–{1:HH:mm}", item.Start, item.End),
                    CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    new Typeface("Segoe UI"),
                    10,
                    Brushes.White,
                    dpi);

                dc.DrawText(range, new Point(bounds.X + 6, bounds.Y + 19));
            }

            if (selected)
            {
                var ring = new Pen(text, 2);
                dc.DrawRoundedRectangle(null, ring, bounds, 4, 4);
            }
        }

        if (ShowNowLine && Date.Date == DateTime.Today)
        {
            double now = DateTime.Now.TimeOfDay.TotalHours;
            if (now >= StartHour && now <= EndHour)
            {
                double y = HeaderHeight + (now - StartHour) * HourHeight;
                dc.DrawLine(new Pen(Brushes.IndianRed, 1.5), new Point(GutterWidth, y), new Point(RenderSize.Width, y));
                dc.DrawEllipse(Brushes.IndianRed, null, new Point(GutterWidth, y), 3, 3);
            }
        }
    }

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);

        var point = e.GetPosition(this);

        foreach (var (item, bounds) in BuildLayout())
        {
            if (!bounds.Contains(point)) continue;

            SelectItem(item);

            if (e.ClickCount > 1) ItemActivated?.Invoke(this, item);

            e.Handled = true;
            return;
        }

        // A click on empty timeline clears the selection, which is what a host highlights off.
        SelectItem(null);
    }

    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
        base.OnMouseWheel(e);

        if ((Keyboard.Modifiers & ModifierKeys.Control) == 0) return;

        HourHeight = Math.Max(24, Math.Min(160, HourHeight + Math.Sign(e.Delta) * 6));
        e.Handled = true;
    }
}
