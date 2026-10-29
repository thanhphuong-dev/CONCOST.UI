using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CONCOST.UI.Controls.Charts;

public class PieSegmentModel : DependencyObject
{
    public static readonly DependencyProperty NameProperty =
        DependencyProperty.Register(nameof(Name), typeof(string), typeof(PieSegmentModel), new PropertyMetadata(string.Empty));
    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(nameof(Value), typeof(double), typeof(PieSegmentModel), new PropertyMetadata(0.0));
    public static readonly DependencyProperty ColorProperty =
        DependencyProperty.Register(nameof(Color), typeof(Brush), typeof(PieSegmentModel), new PropertyMetadata(Brushes.DodgerBlue));

    public string Name  { get => (string)GetValue(NameProperty);  set => SetValue(NameProperty, value); }
    public double Value { get => (double)GetValue(ValueProperty); set => SetValue(ValueProperty, value); }
    public Brush  Color { get => (Brush)GetValue(ColorProperty);  set => SetValue(ColorProperty, value); }
}

/// <summary>
/// Base class for all charts with 60 FPS smooth rendering
/// </summary>
public class ChartBase : Control
{
    public static readonly DependencyProperty DatasProperty =
        DependencyProperty.Register(nameof(Datas),
            typeof(IEnumerable<KeyValuePair<string, double>>), typeof(ChartBase),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, OnDatasChanged));

    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(nameof(Title), typeof(string), typeof(ChartBase),
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty UnitProperty =
        DependencyProperty.Register(nameof(Unit), typeof(string), typeof(ChartBase),
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty ShowLegendProperty =
        DependencyProperty.Register(nameof(ShowLegend), typeof(bool), typeof(ChartBase),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));

    protected const double EllipseR = 5;
    protected const double HitPad   = 16;
    protected const double AxisRows = 5;

    protected static readonly Typeface NormalTypeface = new(new FontFamily("Segoe UI, Inter, Arial"), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);
    protected static readonly Typeface SemiBoldTypeface = new(new FontFamily("Segoe UI, Inter, Arial"), FontStyles.Normal, FontWeights.SemiBold, FontStretches.Normal);

    static ChartBase()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(ChartBase),
               new FrameworkPropertyMetadata(typeof(ChartBase)));
        FocusableProperty.OverrideMetadata(typeof(ChartBase), new FrameworkPropertyMetadata(true));
        ClipToBoundsProperty.OverrideMetadata(typeof(ChartBase), new FrameworkPropertyMetadata(false));
    }

    public ChartBase()
    {
        RenderOptions.SetClearTypeHint(this, ClearTypeHint.Enabled);
    }

    public IEnumerable<KeyValuePair<string, double>>? Datas
    {
        get => (IEnumerable<KeyValuePair<string, double>>?)GetValue(DatasProperty);
        set => SetValue(DatasProperty, value);
    }
    public string Title      { get => (string)GetValue(TitleProperty);      set => SetValue(TitleProperty, value); }
    public string Unit       { get => (string)GetValue(UnitProperty);       set => SetValue(UnitProperty, value); }
    public bool   ShowLegend { get => (bool)GetValue(ShowLegendProperty);   set => SetValue(ShowLegendProperty, value); }

    private static void OnDatasChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        => (d as ChartBase)?.InvalidateVisual();

    protected Brush PrimaryBrush  => TryFindResource("CONCOST.Brush.Primary")       as Brush ?? Brushes.DarkOrange;
    protected Brush TextPrimary   => TryFindResource("CONCOST.Brush.TextPrimary")   as Brush ?? Brushes.Black;
    protected Brush TextSecondary => TryFindResource("CONCOST.Brush.TextSecondary") as Brush ?? Brushes.DimGray;
    protected Brush SurfaceBrush  => TryFindResource("CONCOST.Brush.Surface")       as Brush ?? Brushes.White;
    protected Brush SurfaceSec    => TryFindResource("CONCOST.Brush.SurfaceSecondary") as Brush ?? Brushes.Transparent;

    protected static readonly Color[] Palette =
    {
        Color.FromRgb(255, 128,   0),
        Color.FromRgb( 33, 150, 243),
        Color.FromRgb( 76, 175,  80),
        Color.FromRgb(156,  39, 176),
        Color.FromRgb(  0, 188, 212),
        Color.FromRgb(233,  30,  99),
        Color.FromRgb(255, 193,   7),
        Color.FromRgb(121,  85,  72),
    };

    protected FormattedText FT(string text, Brush brush, double size = 11, bool bold = false)
    {
        return new FormattedText(text, CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight, bold ? SemiBoldTypeface : NormalTypeface, Math.Max(7, size), brush, 1.0);
    }

    protected void DrawLegend(DrawingContext dc, IList<(string Name, Brush Color)> items,
                               double x, double y)
    {
        foreach (var (name, color) in items)
        {
            dc.DrawRectangle(color, null, new Rect(x, y + 2, 10, 10));
            var ft = FT(name, TextSecondary, 10);
            dc.DrawText(ft, new Point(x + 14, y));
            x += ft.Width + 30;
        }
    }

    protected void DrawFloatingTooltip(DrawingContext dc, Point anchor, string title, string valStr, Brush accentBrush)
    {
        var titleFt = FT(title, TextSecondary, 10);
        var valFt   = FT(valStr, TextPrimary, 11.5, true);

        double w = Math.Max(titleFt.Width, valFt.Width) + 22;
        double h = titleFt.Height + valFt.Height + 12;

        double x = anchor.X - w / 2;
        double y = anchor.Y - h - 10;

        if (y < 4) y = anchor.Y + 16;
        if (x < 4) x = 4;
        if (x + w > ActualWidth - 4) x = ActualWidth - w - 4;

        var tipRect = new Rect(x, y, w, h);

        var shadowBrush = new SolidColorBrush(Color.FromArgb(20, 0, 0, 0)); 
        shadowBrush.Freeze();
        dc.DrawRoundedRectangle(shadowBrush, null, new Rect(x + 1, y + 2, w, h), 6, 6);

        var bgBrush = new SolidColorBrush(Color.FromRgb(255, 255, 255)); 
        bgBrush.Freeze();
        var borderPen = new Pen(new SolidColorBrush(Color.FromArgb(50, 180, 195, 215)), 1); 
        borderPen.Freeze();
        dc.DrawRoundedRectangle(bgBrush, borderPen, tipRect, 6, 6);

        dc.DrawRoundedRectangle(accentBrush, null, new Rect(x + 4, y + 5, 3, h - 10), 1.5, 1.5);

        dc.DrawText(titleFt, new Point(x + 12, y + 4));
        dc.DrawText(valFt, new Point(x + 12, y + 4 + titleFt.Height + 1));
    }
}

public abstract class ChartRect : ChartBase
{
    protected double OriginX { get; private set; } = 52;
    protected double OriginY { get; private set; }
    protected double ScaleY  { get; private set; }
    protected double MaxVal  { get; private set; }
    protected double DynamicColInterval { get; private set; } = 80;
    protected double PlotWidth { get; private set; }
    protected double PlotHeight { get; private set; }

    private int _hoveredIndex = -1;
    protected int HoveredIndex => _hoveredIndex;

    protected bool SetHoveredIndex(int idx)
    {
        if (_hoveredIndex == idx) return false;
        _hoveredIndex = idx;
        InvalidateVisual();
        return true;
    }

    protected override void OnMouseLeave(MouseEventArgs e)
    {
        base.OnMouseLeave(e);
        SetHoveredIndex(-1);
    }

    protected void RenderAxes(DrawingContext dc)
    {
        if (Datas == null || !Datas.Any()) return;

        var bg = Background ?? SurfaceSec;
        dc.DrawRectangle(bg, null, new Rect(0, 0, ActualWidth, ActualHeight));

        if (!string.IsNullOrEmpty(Title))
        {
            string titleStr = string.IsNullOrEmpty(Unit) ? Title : $"{Title} ({Unit})";
            dc.DrawText(FT(titleStr, TextPrimary, 13, true), new Point(16, 12));
        }

        MaxVal = Datas.Max(kv => kv.Value);
        if (MaxVal <= 0.001) MaxVal = 100;
        int count = Datas.Count();

        OriginX = 52;
        OriginY = Math.Max(80, ActualHeight - 40);
        PlotWidth = Math.Max(100, ActualWidth - OriginX - 30);
        PlotHeight = Math.Max(60, OriginY - 45);

        DynamicColInterval = PlotWidth / Math.Max(1, count);
        ScaleY = PlotHeight / (MaxVal * 1.15);

        var axisPen = new Pen((Brush?)TryFindResource("CONCOST.Brush.Border") ?? Brushes.LightGray, 1); 
        axisPen.Freeze();
        
        var gridPen = new Pen(new SolidColorBrush(Color.FromArgb(45, 180, 195, 215)), 0.8) 
            { DashStyle = DashStyles.Dash }; 
        gridPen.Freeze();

        double step = MaxVal / AxisRows;
        for (int i = 0; i <= AxisRows; i++)
        {
            double val = i * step;
            double py  = OriginY - (val * ScaleY);
            if (i > 0)
                dc.DrawLine(gridPen, new Point(OriginX, py), new Point(OriginX + PlotWidth, py));
            
            var lb = FT(val >= 1000 ? $"{val/1000:0.#}k" : val.ToString("0.#", CultureInfo.InvariantCulture),
                        TextSecondary, 9.5);
            dc.DrawText(lb, new Point(OriginX - lb.Width - 8, py - lb.Height / 2));
        }

        dc.DrawLine(axisPen, new Point(OriginX, OriginY), new Point(OriginX + PlotWidth, OriginY));
        dc.DrawLine(axisPen, new Point(OriginX, OriginY), new Point(OriginX, OriginY - PlotHeight));
    }

    protected double ColCenterX(int idx) => OriginX + (idx + 0.5) * DynamicColInterval;
    protected double ValueToY(double value) => OriginY - value * ScaleY;
}

public class ChartBar : ChartRect
{
    public static readonly DependencyProperty BarBrushProperty =
        DependencyProperty.Register(nameof(BarBrush), typeof(Brush), typeof(ChartBar),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty MultiColorProperty =
        DependencyProperty.Register(nameof(MultiColor), typeof(bool), typeof(ChartBar),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));

    public Brush? BarBrush   { get => (Brush?)GetValue(BarBrushProperty);  set => SetValue(BarBrushProperty, value); }
    public bool   MultiColor { get => (bool)GetValue(MultiColorProperty); set => SetValue(MultiColorProperty, value); }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        if (Datas == null || !Datas.Any()) return;

        var pos = e.GetPosition(this);
        int newIdx = -1;

        if (pos.X >= OriginX && pos.X <= OriginX + PlotWidth && pos.Y >= OriginY - PlotHeight && pos.Y <= OriginY + 20)
        {
            newIdx = (int)((pos.X - OriginX) / DynamicColInterval);
            if (newIdx < 0 || newIdx >= Datas.Count()) newIdx = -1;
        }

        SetHoveredIndex(newIdx);
    }

    protected override void OnRender(DrawingContext dc)
    {
        if (Datas == null || !Datas.Any()) return;

        RenderAxes(dc);

        var dataList = Datas.ToList();
        double colW = NumericCompatibility.Clamp(DynamicColInterval * 0.40, 24, 42);

        if (ShowLegend && MultiColor)
        {
            var legendItems = dataList.Select((kv, i) =>
                (kv.Key, (Brush)new SolidColorBrush(Palette[i % Palette.Length]))).ToList();
            DrawLegend(dc, legendItems, 16, 32);
        }

        Point hoverAnchor = default;
        string hoverTitle = string.Empty;
        string hoverVal = string.Empty;
        Brush? hoverBrush = null;

        for (int i = 0; i < dataList.Count; i++)
        {
            var kv  = dataList[i];
            bool isCurrentHov = HoveredIndex == i;

            Color baseCol = MultiColor ? Palette[i % Palette.Length]
                          : (BarBrush is SolidColorBrush scb ? scb.Color : Color.FromRgb(255, 128, 0));

            double cx    = ColCenterX(i);
            double barH  = kv.Value * ScaleY;
            double barX  = cx - colW / 2;
            double barY  = OriginY - barH;

            Color topCol = isCurrentHov 
                ? Color.FromRgb((byte)Math.Min(255, baseCol.R + 40), (byte)Math.Min(255, baseCol.G + 40), (byte)Math.Min(255, baseCol.B + 40))
                : Color.FromRgb((byte)Math.Min(255, baseCol.R + 20), (byte)Math.Min(255, baseCol.G + 20), (byte)Math.Min(255, baseCol.B + 20));
            Color botCol = baseCol;

            var colBrush = new LinearGradientBrush(topCol, botCol, 90);
            colBrush.Freeze();

            var barGeo = new StreamGeometry();
            using (var ctx = barGeo.Open())
            {
                double r = Math.Min(6, Math.Min(colW / 2, barH));
                ctx.BeginFigure(new Point(barX, OriginY), true, true);
                ctx.LineTo(new Point(barX, barY + r), true, false);
                ctx.ArcTo(new Point(barX + r, barY), new Size(r, r), 0, false, SweepDirection.Clockwise, true, false);
                ctx.LineTo(new Point(barX + colW - r, barY), true, false);
                ctx.ArcTo(new Point(barX + colW, barY + r), new Size(r, r), 0, false, SweepDirection.Clockwise, true, false);
                ctx.LineTo(new Point(barX + colW, OriginY), true, false);
            }
            barGeo.Freeze();

            dc.DrawGeometry(colBrush, null, barGeo);

            var nameBrush = isCurrentHov ? (Brush?)TryFindResource("CONCOST.Brush.TextPrimary") ?? Brushes.Black : TextSecondary;
            var nameFt = FT(kv.Key, nameBrush, 10, isCurrentHov);
            dc.DrawText(nameFt, new Point(cx - nameFt.Width / 2, OriginY + 8));

            if (isCurrentHov)
            {
                hoverAnchor = new Point(cx, barY);
                hoverTitle  = kv.Key;
                hoverVal    = $"{kv.Value:N1} {Unit}".Trim();
                hoverBrush  = colBrush;
            }
        }

        if (HoveredIndex >= 0 && hoverBrush != null)
        {
            DrawFloatingTooltip(dc, hoverAnchor, hoverTitle, hoverVal, hoverBrush);
        }
    }
}

public class ChartLine : ChartRect
{
    public static readonly DependencyProperty LineBrushProperty =
        DependencyProperty.Register(nameof(LineBrush), typeof(Brush), typeof(ChartLine),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty ShowAreaProperty =
        DependencyProperty.Register(nameof(ShowArea), typeof(bool), typeof(ChartLine),
            new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty SmoothProperty =
        DependencyProperty.Register(nameof(Smooth), typeof(bool), typeof(ChartLine),
            new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender));

    public Brush? LineBrush { get => (Brush?)GetValue(LineBrushProperty); set => SetValue(LineBrushProperty, value); }
    public bool   ShowArea  { get => (bool)GetValue(ShowAreaProperty);     set => SetValue(ShowAreaProperty, value); }
    public bool   Smooth    { get => (bool)GetValue(SmoothProperty);       set => SetValue(SmoothProperty, value); }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        if (Datas == null || !Datas.Any()) return;

        var pos = e.GetPosition(this);
        int newIdx = -1;

        if (pos.X >= OriginX && pos.X <= OriginX + PlotWidth)
        {
            newIdx = (int)Math.Round((pos.X - OriginX - (DynamicColInterval / 2)) / DynamicColInterval);
            newIdx = NumericCompatibility.Clamp(newIdx, 0, Datas.Count() - 1);
        }

        SetHoveredIndex(newIdx);
    }

    protected override void OnRender(DrawingContext dc)
    {
        if (Datas == null || !Datas.Any()) return;

        RenderAxes(dc);

        var dataList  = Datas.ToList();
        var brush     = LineBrush ?? PrimaryBrush;
        var points    = new List<Point>();

        for (int i = 0; i < dataList.Count; i++)
        {
            var pt = new Point(ColCenterX(i), ValueToY(dataList[i].Value));
            points.Add(pt);

            var nameFt = FT(dataList[i].Key, TextSecondary, 10);
            dc.DrawText(nameFt, new Point(pt.X - nameFt.Width / 2, OriginY + 8));
        }

        if (HoveredIndex >= 0 && HoveredIndex < points.Count)
        {
            var crossPen = new Pen(new SolidColorBrush(Color.FromArgb(70, 180, 195, 215)), 1)
                { DashStyle = DashStyles.Dash };
            crossPen.Freeze();
            dc.DrawLine(crossPen, new Point(points[HoveredIndex].X, OriginY - PlotHeight), new Point(points[HoveredIndex].X, OriginY));
        }

        if (ShowArea && points.Count > 1)
        {
            var areaGeo = BuildAreaPath(points, OriginY);
            Color col = brush is SolidColorBrush sc ? sc.Color : Color.FromRgb(255,128,0);
            var areaBrush = new LinearGradientBrush(
                Color.FromArgb(85,  col.R, col.G, col.B),
                Color.FromArgb(8,   col.R, col.G, col.B), 90);
            areaBrush.Freeze();
            dc.DrawGeometry(areaBrush, null, areaGeo);
        }

        var linePen = new Pen(brush, 2.5); linePen.Freeze();
        if (Smooth && points.Count > 2)
        {
            var sg = BuildCurvePath(points);
            dc.DrawGeometry(null, linePen, sg);
        }
        else
        {
            for (int i = 0; i < points.Count - 1; i++)
                dc.DrawLine(linePen, points[i], points[i + 1]);
        }

        for (int i = 0; i < dataList.Count; i++)
        {
            bool hov = HoveredIndex == i;
            double r = hov ? EllipseR * 2.0 : EllipseR;
            var pt = points[i];

            var dotFill = hov ? Brushes.White : brush;
            dc.DrawEllipse(dotFill, new Pen(brush, 2), pt, r, r);
        }

        if (HoveredIndex >= 0 && HoveredIndex < dataList.Count)
        {
            var pt = points[HoveredIndex];
            var kv = dataList[HoveredIndex];
            DrawFloatingTooltip(dc, pt, kv.Key, $"{kv.Value:N1} {Unit}".Trim(), brush);
        }
    }

    private static StreamGeometry BuildAreaPath(List<Point> pts, double baseOriginY)
    {
        var sg = new StreamGeometry();
        using (var ctx = sg.Open())
        {
            ctx.BeginFigure(new Point(pts[0].X, baseOriginY), true, true);
            ctx.LineTo(pts[0], true, false);
            foreach (var p in pts.Skip(1)) ctx.LineTo(p, true, false);
            ctx.LineTo(new Point(pts.Last().X, baseOriginY), true, false);
        }
        sg.Freeze();
        return sg;
    }

    private static StreamGeometry BuildCurvePath(List<Point> pts)
    {
        var sg = new StreamGeometry();
        using (var ctx = sg.Open())
        {
            ctx.BeginFigure(pts[0], false, false);
            for (int i = 0; i < pts.Count - 1; i++)
            {
                double tension = 0.3;
                var p0 = i > 0 ? pts[i - 1] : pts[i];
                var p1 = pts[i];
                var p2 = pts[i + 1];
                var p3 = i + 2 < pts.Count ? pts[i + 2] : p2;
                var cp1 = new Point(p1.X + (p2.X - p0.X) * tension,
                                    p1.Y + (p2.Y - p0.Y) * tension);
                var cp2 = new Point(p2.X - (p3.X - p1.X) * tension,
                                    p2.Y - (p3.Y - p1.Y) * tension);
                ctx.BezierTo(cp1, cp2, p2, true, false);
            }
        }
        sg.Freeze();
        return sg;
    }
}

public class ChartRadar : ChartBase
{
    public static readonly DependencyProperty RadarBrushProperty =
        DependencyProperty.Register(nameof(RadarBrush), typeof(Brush), typeof(ChartRadar),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty GridLevelsProperty =
        DependencyProperty.Register(nameof(GridLevels), typeof(int), typeof(ChartRadar),
            new FrameworkPropertyMetadata(5, FrameworkPropertyMetadataOptions.AffectsRender));

    public Brush? RadarBrush { get => (Brush?)GetValue(RadarBrushProperty); set => SetValue(RadarBrushProperty, value); }
    public int    GridLevels { get => (int)GetValue(GridLevelsProperty);    set => SetValue(GridLevelsProperty, value); }

    private readonly List<Point> _dataPts = new();
    private int _hovIdx = -1;

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        var pos = e.GetPosition(this);
        int newIdx = -1;
        double minD = HitPad * 1.8;
        for (int i = 0; i < _dataPts.Count; i++)
        {
            double d = Math.Sqrt(Math.Pow(pos.X - _dataPts[i].X, 2) + Math.Pow(pos.Y - _dataPts[i].Y, 2));
            if (d < minD) { minD = d; newIdx = i; }
        }
        if (newIdx != _hovIdx)
        {
            _hovIdx = newIdx;
            InvalidateVisual();
        }
    }

    protected override void OnMouseLeave(MouseEventArgs e)
    {
        base.OnMouseLeave(e);
        if (_hovIdx != -1) { _hovIdx = -1; InvalidateVisual(); }
    }

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);
        if (Datas == null || !Datas.Any()) return;

        dc.DrawRectangle(Background ?? SurfaceSec, null, new Rect(0, 0, ActualWidth, ActualHeight));

        if (!string.IsNullOrEmpty(Title))
            dc.DrawText(FT(Title, TextPrimary, 13, true), new Point(16, 12));

        _dataPts.Clear();

        var dataList  = Datas.ToList();
        int   n       = dataList.Count;
        double cx     = ActualWidth  / 2.0;
        double cy     = ActualHeight / 2.0;
        double margin = 50;
        double maxR   = Math.Min(ActualWidth, ActualHeight) / 2.0 - margin;
        if (maxR < 10) return;

        double maxVal = dataList.Max(kv => kv.Value);
        if (maxVal <= 0.001) maxVal = 100;

        var gridPen = new Pen((Brush?)TryFindResource("CONCOST.Brush.Border") ?? Brushes.LightGray, 0.8); 
        gridPen.Freeze();
        var axPen   = new Pen((Brush?)TryFindResource("CONCOST.Brush.Border") ?? Brushes.LightGray, 1.0); 
        axPen.Freeze();

        for (int lv = 1; lv <= GridLevels; lv++)
        {
            double r = maxR * lv / GridLevels;
            var ring = GetPolygonPoints(cx, cy, r, n);
            DrawPolygon(dc, ring, null, gridPen);

            double scaleVal = maxVal * lv / GridLevels;
            var lft = FT(scaleVal.ToString("0.#", CultureInfo.InvariantCulture), TextSecondary, 8.5);
            dc.DrawText(lft, new Point(cx - lft.Width / 2, cy - r - lft.Height - 1));
        }

        for (int i = 0; i < n; i++)
        {
            var outerPt = GetRadialPoint(cx, cy, maxR, i, n);
            dc.DrawLine(axPen, new Point(cx, cy), outerPt);

            var lft = FT(dataList[i].Key, TextSecondary, 10.5);
            double tx = outerPt.X, ty = outerPt.Y;
            if (Math.Abs(outerPt.X - cx) < 5) tx -= lft.Width / 2;
            else if (outerPt.X < cx) tx -= lft.Width + 6;
            else tx += 6;
            if (outerPt.Y < cy) ty -= lft.Height + 4;
            else if (Math.Abs(outerPt.Y - cy) < 5) ty -= lft.Height / 2;
            else ty += 4;
            dc.DrawText(lft, new Point(tx, ty));
        }

        var activeBrush = RadarBrush ?? PrimaryBrush;
        Color activeCol = activeBrush is SolidColorBrush sc2 ? sc2.Color : Color.FromRgb(33,150,243);

        for (int i = 0; i < n; i++)
        {
            double ratio = dataList[i].Value / maxVal;
            var outerPt  = GetRadialPoint(cx, cy, maxR, i, n);
            var dataPt   = new Point(cx + (outerPt.X - cx) * ratio,
                                      cy + (outerPt.Y - cy) * ratio);
            _dataPts.Add(dataPt);
        }

        var fillBrush = new SolidColorBrush(Color.FromArgb(70, activeCol.R, activeCol.G, activeCol.B));
        fillBrush.Freeze();
        var strokePen = new Pen(activeBrush, 2.5); strokePen.Freeze();
        DrawPolygon(dc, _dataPts, fillBrush, strokePen);

        for (int i = 0; i < _dataPts.Count; i++)
        {
            bool hov = i == _hovIdx;
            double r = hov ? EllipseR * 2.2 : EllipseR;
            var dotFill = hov ? Brushes.White : activeBrush;
            dc.DrawEllipse(dotFill, new Pen(activeBrush, 1.5), _dataPts[i], r, r);
        }

        if (_hovIdx >= 0 && _hovIdx < _dataPts.Count)
        {
            var pt = _dataPts[_hovIdx];
            var kv = dataList[_hovIdx];
            DrawFloatingTooltip(dc, pt, kv.Key, $"{kv.Value:N1} {Unit}".Trim(), activeBrush);
        }
    }

    private static Point GetRadialPoint(double cx, double cy, double r, int i, int n)
    {
        double angle = (i * 360.0 / n - 90) * Math.PI / 180.0;
        return new Point(cx + r * Math.Cos(angle), cy + r * Math.Sin(angle));
    }

    private static List<Point> GetPolygonPoints(double cx, double cy, double r, int n)
    {
        var pts = new List<Point>();
        for (int i = 0; i < n; i++)
            pts.Add(GetRadialPoint(cx, cy, r, i, n));
        return pts;
    }

    private static void DrawPolygon(DrawingContext dc, List<Point> pts,
                                     Brush? fill, Pen? stroke)
    {
        if (pts.Count < 2) return;
        var sg = new StreamGeometry();
        using (var ctx = sg.Open())
        {
            ctx.BeginFigure(pts.Last(), fill != null, true);
            ctx.PolyLineTo(pts, stroke != null, false);
        }
        sg.Freeze();
        dc.DrawGeometry(fill, stroke, sg);
    }
}

public class ChartPie : ChartBase
{
    private struct Sector
    {
        public double StartAngle, EndAngle, Radius;
        public double CX, CY;
        public Point MidPoint;
    }
    private readonly List<Sector> _sectors = new();
    private int _hovIdx = -1;

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        var pos = e.GetPosition(this);
        int newIdx = HitSector(pos);
        if (newIdx != _hovIdx)
        {
            _hovIdx = newIdx;
            InvalidateVisual();
        }
    }

    protected override void OnMouseLeave(MouseEventArgs e)
    {
        base.OnMouseLeave(e);
        if (_hovIdx != -1) { _hovIdx = -1; InvalidateVisual(); }
    }

    private int HitSector(Point pos)
    {
        for (int i = 0; i < _sectors.Count; i++)
        {
            var s  = _sectors[i];
            double dx = pos.X - s.CX, dy = pos.Y - s.CY;
            double d  = Math.Sqrt(dx*dx + dy*dy);
            if (d > s.Radius + 12) continue;

            double ang = Math.Atan2(dy, dx) * 180.0 / Math.PI;
            double st  = s.StartAngle, en = s.EndAngle;
            while (ang < st - 0.5) ang += 360;
            while (ang > en + 360)  ang -= 360;
            if (ang >= st && ang <= en) return i;
        }
        return -1;
    }

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);
        if (Datas == null || !Datas.Any()) return;

        dc.DrawRectangle(Background ?? SurfaceSec, null, new Rect(0, 0, ActualWidth, ActualHeight));

        if (!string.IsNullOrEmpty(Title))
            dc.DrawText(FT(Title, TextPrimary, 13, true), new Point(16, 12));

        _sectors.Clear();

        var dataList  = Datas.ToList();
        double sum    = dataList.Sum(kv => kv.Value);
        if (sum <= 0.001) return;

        double legendH = ShowLegend ? 24 : 0;
        double cx = ActualWidth  / 2.0;
        double cy = (ActualHeight - legendH) / 2.0 + (string.IsNullOrEmpty(Title) ? 0 : 10);
        double R  = Math.Min(ActualWidth, ActualHeight - legendH) / 2.0 - 24;
        if (R < 10) return;

        double explodeOffset = 8;
        double curAngle = -90.0;
        var sepPen = new Pen(Background ?? SurfaceSec, 2); sepPen.Freeze();

        for (int i = 0; i < dataList.Count; i++)
        {
            double sweep = dataList[i].Value / sum * 360.0;
            double startA = curAngle, endA = curAngle + sweep;
            bool   hov    = i == _hovIdx;

            double midRad = (startA + sweep / 2.0) * Math.PI / 180.0;
            double ox = hov ? Math.Cos(midRad) * explodeOffset : 0;
            double oy = hov ? Math.Sin(midRad) * explodeOffset : 0;
            double cxs = cx + ox, cys = cy + oy;

            double sRad = startA * Math.PI / 180.0;
            double eRad = endA   * Math.PI / 180.0;
            var sp = new Point(cxs + R * Math.Cos(sRad), cys + R * Math.Sin(sRad));
            var ep = new Point(cxs + R * Math.Cos(eRad), cys + R * Math.Sin(eRad));

            Color col = Palette[i % Palette.Length];
            double opacity = (_hovIdx >= 0) ? (hov ? 1.0 : 0.55) : 0.95;
            Color fillCol = Color.FromArgb((byte)(255 * opacity), col.R, col.G, col.B);

            var pGeo = new PathGeometry();
            var fig  = new PathFigure { StartPoint = new Point(cxs, cys), IsClosed=true, IsFilled=true };
            fig.Segments.Add(new LineSegment(sp, true));
            fig.Segments.Add(new ArcSegment(ep, new Size(R,R), 0, sweep>180, SweepDirection.Clockwise, true));
            pGeo.Figures.Add(fig);
            pGeo.Freeze();

            var segBrush = new SolidColorBrush(fillCol); segBrush.Freeze();
            var strokePen = hov ? new Pen(Brushes.White, 2) : sepPen;
            dc.DrawGeometry(segBrush, strokePen, pGeo);

            if (sweep > 18)
            {
                double pct = dataList[i].Value / sum * 100;
                double labelR = R * (hov ? 0.62 : 0.58);
                var labelFt = FT($"{pct:0.0}%", Brushes.White, 10.5, true);
                double lx = cxs + labelR * Math.Cos(midRad) - labelFt.Width  / 2;
                double ly = cys + labelR * Math.Sin(midRad) - labelFt.Height / 2;
                dc.DrawText(labelFt, new Point(lx, ly));
            }

            _sectors.Add(new Sector
            {
                StartAngle = startA, EndAngle = endA,
                Radius = R + explodeOffset, CX = cx, CY = cy,
                MidPoint = new Point(cxs + R * 0.7 * Math.Cos(midRad), cys + R * 0.7 * Math.Sin(midRad))
            });

            curAngle = endA;
        }

        if (_hovIdx >= 0 && _hovIdx < _sectors.Count)
        {
            var kv = dataList[_hovIdx];
            var col = new SolidColorBrush(Palette[_hovIdx % Palette.Length]); col.Freeze();
            DrawFloatingTooltip(dc, _sectors[_hovIdx].MidPoint, kv.Key, $"{kv.Value:N1} ({kv.Value/sum*100:0.0}%)", col);
        }

        if (ShowLegend)
        {
            double lx = 16, ly = ActualHeight - legendH + 4;
            for (int i = 0; i < dataList.Count; i++)
            {
                var col = new SolidColorBrush(Palette[i % Palette.Length]); col.Freeze();
                dc.DrawRectangle(col, null, new Rect(lx, ly + 2, 10, 10));
                var ft = FT(dataList[i].Key, TextSecondary, 9.5);
                dc.DrawText(ft, new Point(lx + 13, ly));
                lx += ft.Width + 26;
            }
        }
    }
}

public class PieControl : ChartBase
{
    public static readonly DependencyProperty SegmentsProperty =
        DependencyProperty.Register(nameof(Segments),
            typeof(ObservableCollection<PieSegmentModel>), typeof(PieControl),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender,
                (d,e) => { if (d is PieControl p) p.InvalidateVisual(); }));

    public static readonly DependencyProperty ArcThicknessProperty =
        DependencyProperty.Register(nameof(ArcThickness), typeof(double), typeof(PieControl),
            new FrameworkPropertyMetadata(36.0, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty CenterTextProperty =
        DependencyProperty.Register(nameof(CenterText), typeof(string), typeof(PieControl),
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.AffectsRender));

    public ObservableCollection<PieSegmentModel>? Segments
    {
        get => (ObservableCollection<PieSegmentModel>?)GetValue(SegmentsProperty);
        set => SetValue(SegmentsProperty, value);
    }
    public double ArcThickness { get => (double)GetValue(ArcThicknessProperty); set => SetValue(ArcThicknessProperty, value); }
    public string CenterText   { get => (string)GetValue(CenterTextProperty);   set => SetValue(CenterTextProperty, value); }

    static PieControl()
        => DefaultStyleKeyProperty.OverrideMetadata(typeof(PieControl),
               new FrameworkPropertyMetadata(typeof(PieControl)));

    private struct ArcSector
    {
        public double StartAngle, EndAngle, OuterR, InnerR, CX, CY;
        public Point MidPoint;
    }
    private readonly List<ArcSector> _arcs = new();
    private int _hovIdx = -1;

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        var pos = e.GetPosition(this);
        int newIdx = HitArc(pos);
        if (newIdx != _hovIdx)
        {
            _hovIdx = newIdx;
            InvalidateVisual();
        }
    }

    protected override void OnMouseLeave(MouseEventArgs e)
    {
        base.OnMouseLeave(e);
        if (_hovIdx != -1) { _hovIdx = -1; InvalidateVisual(); }
    }

    private int HitArc(Point pos)
    {
        for (int i = 0; i < _arcs.Count; i++)
        {
            var a  = _arcs[i];
            double dx = pos.X - a.CX, dy = pos.Y - a.CY;
            double d  = Math.Sqrt(dx*dx + dy*dy);
            if (d < a.InnerR - 2 || d > a.OuterR + 2) continue;

            double ang = Math.Atan2(dy, dx) * 180.0 / Math.PI;
            double st  = a.StartAngle, en = a.EndAngle;
            while (ang < st - 0.5) ang += 360;
            while (ang > en + 360)  ang -= 360;
            if (ang >= st && ang <= en) return i;
        }
        return -1;
    }

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);
        if (Segments == null || !Segments.Any()) return;

        dc.DrawRectangle(Background ?? Brushes.Transparent, null,
            new Rect(0, 0, ActualWidth, ActualHeight));

        if (!string.IsNullOrEmpty(Title))
            dc.DrawText(FT(Title, TextPrimary, 13, true), new Point(16, 12));

        _arcs.Clear();

        double total  = Segments.Sum(s => s.Value);
        if (total <= 0.001) return;

        double legendH = ShowLegend ? 24 : 0;
        double cx = ActualWidth  / 2.0;
        double cy = (ActualHeight - legendH) / 2.0 + (string.IsNullOrEmpty(Title) ? 0 : 8);
        double OR = Math.Min(ActualWidth, ActualHeight - legendH) / 2.0 - 12;
        double IR = Math.Max(0, OR - ArcThickness);
        if (OR < 10) return;

        double explodeOff = 8;
        double curAngle   = -90.0;
        var sepPen = new Pen(Background ?? Brushes.Transparent, 2.5); sepPen.Freeze();

        for (int i = 0; i < Segments.Count; i++)
        {
            var seg   = Segments[i];
            double sweep = seg.Value / total * 360.0;
            double startA = curAngle, endA = curAngle + sweep;
            bool   hov    = i == _hovIdx;

            double midRad = (startA + sweep / 2.0) * Math.PI / 180.0;
            double ox = hov ? Math.Cos(midRad) * explodeOff : 0;
            double oy = hov ? Math.Sin(midRad) * explodeOff : 0;
            double cxs = cx+ox, cys = cy+oy;

            double sRad = startA * Math.PI / 180.0;
            double eRad = endA   * Math.PI / 180.0;

            var p1 = new Point(cxs + OR*Math.Cos(sRad), cys + OR*Math.Sin(sRad));
            var p2 = new Point(cxs + OR*Math.Cos(eRad), cys + OR*Math.Sin(eRad));
            var p3 = new Point(cxs + IR*Math.Cos(eRad), cys + IR*Math.Sin(eRad));
            var p4 = new Point(cxs + IR*Math.Cos(sRad), cys + IR*Math.Sin(sRad));

            var geo = new PathGeometry();
            var fig = new PathFigure { StartPoint=p1, IsClosed=true, IsFilled=true };
            fig.Segments.Add(new ArcSegment(p2, new Size(OR,OR), 0, sweep>180, SweepDirection.Clockwise, true));
            fig.Segments.Add(new LineSegment(p3, true));
            fig.Segments.Add(new ArcSegment(p4, new Size(IR,IR), 0, sweep>180, SweepDirection.Counterclockwise, true));
            geo.Figures.Add(fig);
            geo.Freeze();

            Brush segBrush = seg.Color ?? Brushes.DodgerBlue;
            if (seg.Color is SolidColorBrush scb)
            {
                double op = (_hovIdx >= 0) ? (hov ? 1.0 : 0.55) : 0.95;
                segBrush = new SolidColorBrush(Color.FromArgb((byte)(255 * op), scb.Color.R, scb.Color.G, scb.Color.B));
                segBrush.Freeze();
            }

            var strokePen = hov ? new Pen(Brushes.White, 2) : sepPen;
            dc.DrawGeometry(segBrush, strokePen, geo);

            _arcs.Add(new ArcSector
            {
                StartAngle=startA, EndAngle=endA, OuterR=OR+explodeOff, InnerR=IR,
                CX=cx, CY=cy,
                MidPoint = new Point(cxs + (OR+IR)/2.0 * Math.Cos(midRad), cys + (OR+IR)/2.0 * Math.Sin(midRad))
            });

            curAngle = endA;
        }

        if (!string.IsNullOrEmpty(CenterText))
        {
            var cft = FT(CenterText, TextPrimary, 13, true);
            dc.DrawText(cft, new Point(cx - cft.Width/2, cy - cft.Height/2));
        }
        else
        {
            string totStr = total >= 1000 ? $"{total/1000:0.#}k" : total.ToString("N0");
            var tft = FT(totStr, TextSecondary, 12, true);
            dc.DrawText(tft, new Point(cx - tft.Width/2, cy - tft.Height/2));
        }

        if (_hovIdx >= 0 && _hovIdx < _arcs.Count)
        {
            var seg = Segments[_hovIdx];
            DrawFloatingTooltip(dc, _arcs[_hovIdx].MidPoint, seg.Name, $"{seg.Value:N1} ({seg.Value/total*100:0.0}%)", seg.Color ?? Brushes.DodgerBlue);
        }

        if (ShowLegend && Segments.Count > 0)
        {
            double lx = 16, ly = ActualHeight - legendH + 4;
            for (int i = 0; i < Segments.Count; i++)
            {
                dc.DrawRectangle(Segments[i].Color ?? Brushes.Gray, null, new Rect(lx, ly+2, 10, 10));
                var sft = FT(Segments[i].Name, TextSecondary, 9.5);
                dc.DrawText(sft, new Point(lx+13, ly));
                lx += sft.Width + 26;
            }
        }
    }
}