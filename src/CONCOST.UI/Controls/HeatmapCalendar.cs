using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace CONCOST.UI.Controls;

/// <summary>
/// GitHub-style contribution heatmap: one rounded cell per day, coloured by intensity,
/// with optional month labels. Useful for daily concrete pours, inspections or site logs.
/// </summary>
public class HeatmapCalendar : FrameworkElement
{
    private const double MonthLabelHeight = 16;

    public static readonly DependencyProperty ValuesProperty =
        DependencyProperty.Register(nameof(Values), typeof(IEnumerable<int>), typeof(HeatmapCalendar),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty StartDateProperty =
        DependencyProperty.Register(nameof(StartDate), typeof(DateTime), typeof(HeatmapCalendar),
            new FrameworkPropertyMetadata(DateTime.Today.AddDays(-181), FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty CellSizeProperty =
        DependencyProperty.Register(nameof(CellSize), typeof(double), typeof(HeatmapCalendar),
            new FrameworkPropertyMetadata(12.0, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty CellGapProperty =
        DependencyProperty.Register(nameof(CellGap), typeof(double), typeof(HeatmapCalendar),
            new FrameworkPropertyMetadata(3.0, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.Register(nameof(CornerRadius), typeof(double), typeof(HeatmapCalendar),
            new FrameworkPropertyMetadata(2.5, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty BaseBrushProperty =
        DependencyProperty.Register(nameof(BaseBrush), typeof(Brush), typeof(HeatmapCalendar),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty Level1BrushProperty =
        DependencyProperty.Register(nameof(Level1Brush), typeof(Brush), typeof(HeatmapCalendar),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty Level2BrushProperty =
        DependencyProperty.Register(nameof(Level2Brush), typeof(Brush), typeof(HeatmapCalendar),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty Level3BrushProperty =
        DependencyProperty.Register(nameof(Level3Brush), typeof(Brush), typeof(HeatmapCalendar),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty Level4BrushProperty =
        DependencyProperty.Register(nameof(Level4Brush), typeof(Brush), typeof(HeatmapCalendar),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty ShowMonthLabelsProperty =
        DependencyProperty.Register(nameof(ShowMonthLabels), typeof(bool), typeof(HeatmapCalendar),
            new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty MonthLabelBrushProperty =
        DependencyProperty.Register(nameof(MonthLabelBrush), typeof(Brush), typeof(HeatmapCalendar),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty MonthLabelFontSizeProperty =
        DependencyProperty.Register(nameof(MonthLabelFontSize), typeof(double), typeof(HeatmapCalendar),
            new FrameworkPropertyMetadata(9.5, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

    public IEnumerable<int>? Values
    {
        get => (IEnumerable<int>?)GetValue(ValuesProperty);
        set => SetValue(ValuesProperty, value);
    }

    public DateTime StartDate
    {
        get => (DateTime)GetValue(StartDateProperty);
        set => SetValue(StartDateProperty, value);
    }

    public double CellSize
    {
        get => (double)GetValue(CellSizeProperty);
        set => SetValue(CellSizeProperty, value);
    }

    public double CellGap
    {
        get => (double)GetValue(CellGapProperty);
        set => SetValue(CellGapProperty, value);
    }

    public double CornerRadius
    {
        get => (double)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    public Brush? BaseBrush
    {
        get => (Brush?)GetValue(BaseBrushProperty);
        set => SetValue(BaseBrushProperty, value);
    }

    public Brush? Level1Brush
    {
        get => (Brush?)GetValue(Level1BrushProperty);
        set => SetValue(Level1BrushProperty, value);
    }

    public Brush? Level2Brush
    {
        get => (Brush?)GetValue(Level2BrushProperty);
        set => SetValue(Level2BrushProperty, value);
    }

    public Brush? Level3Brush
    {
        get => (Brush?)GetValue(Level3BrushProperty);
        set => SetValue(Level3BrushProperty, value);
    }

    public Brush? Level4Brush
    {
        get => (Brush?)GetValue(Level4BrushProperty);
        set => SetValue(Level4BrushProperty, value);
    }

    public bool ShowMonthLabels
    {
        get => (bool)GetValue(ShowMonthLabelsProperty);
        set => SetValue(ShowMonthLabelsProperty, value);
    }

    public Brush? MonthLabelBrush
    {
        get => (Brush?)GetValue(MonthLabelBrushProperty);
        set => SetValue(MonthLabelBrushProperty, value);
    }

    public double MonthLabelFontSize
    {
        get => (double)GetValue(MonthLabelFontSizeProperty);
        set => SetValue(MonthLabelFontSizeProperty, value);
    }

    private double Step => CellSize + CellGap;

    private List<int> ResolveValues() => Values?.ToList() ?? new List<int>();

    private int WeekCount(int count) => count == 0 ? 0 : (count + 6) / 7;

    protected override Size MeasureOverride(Size availableSize)
    {
        List<int> values = ResolveValues();
        if (values.Count == 0) return new Size(0, 0);

        double width = WeekCount(values.Count) * Step - CellGap;
        double height = 7 * Step - CellGap + (ShowMonthLabels ? MonthLabelHeight : 0);

        if (!double.IsInfinity(availableSize.Width) && availableSize.Width > 0 && availableSize.Width < width)
            width = availableSize.Width;

        return new Size(width, height);
    }

    protected override void OnRender(DrawingContext dc)
    {
        List<int> values = ResolveValues();
        if (values.Count == 0) return;

        int max = values.Max();
        if (max <= 0) max = 1;

        Brush baseBrush = BaseBrush ?? TryFindResource("CONCOST.Brush.SurfaceSecondary") as Brush
                          ?? new SolidColorBrush(Color.FromRgb(0xEE, 0xF1, 0xF5));
        Brush[] levels =
        {
            Level1Brush ?? CreateAccent(0.25),
            Level2Brush ?? CreateAccent(0.45),
            Level3Brush ?? CreateAccent(0.70),
            Level4Brush ?? CreateAccent(1.00)
        };

        Brush labelBrush = MonthLabelBrush ?? TryFindResource("CONCOST.Brush.TextSecondary") as Brush ?? Brushes.Gray;
        double dpi = VisualTreeHelper.GetDpi(this).PixelsPerDip;
        double top = ShowMonthLabels ? MonthLabelHeight : 0;

        int lastMonth = -1;
        double lastLabelRight = double.NegativeInfinity;

        for (int i = 0; i < values.Count; i++)
        {
            int week = i / 7;
            DateTime date = StartDate.AddDays(i);
            int row = ((int)date.DayOfWeek + 6) % 7; // Monday-first rows

            double x = week * Step;
            double y = top + row * Step;

            Brush brush = values[i] <= 0 ? baseBrush : levels[LevelFor(values[i], max)];
            var rect = new Rect(x, y, CellSize, CellSize);
            dc.DrawRoundedRectangle(brush, null, rect, CornerRadius, CornerRadius);

            if (ShowMonthLabels && row == 0)
            {
                if (lastMonth != -1 && date.Month != lastMonth && x > lastLabelRight + 18)
                {
                    var text = new FormattedText(
                        CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(date.Month),
                        CultureInfo.CurrentCulture,
                        FlowDirection.LeftToRight,
                        new Typeface("Segoe UI"),
                        MonthLabelFontSize,
                        labelBrush,
                        dpi);

                    dc.DrawText(text, new Point(x, 1));
                    lastLabelRight = x + text.Width;
                }

                lastMonth = date.Month;
            }
        }
    }

    private static int LevelFor(int value, int max)
    {
        double ratio = (double)value / max;
        if (ratio <= 0.25) return 0;
        if (ratio <= 0.50) return 1;
        if (ratio <= 0.75) return 2;
        return 3;
    }

    private Brush CreateAccent(double strength)
    {
        Brush accent = TryFindResource("CONCOST.Brush.Primary") as Brush ?? Brushes.SteelBlue;
        Color color = accent is SolidColorBrush solid ? solid.Color : Colors.SteelBlue;
        byte alpha = (byte)Math.Round(40 + 215 * strength);
        var brush = new SolidColorBrush(Color.FromArgb(alpha, color.R, color.G, color.B));
        brush.Freeze();
        return brush;
    }
}
