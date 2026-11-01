using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using CONCOST.UI.Services;

namespace CONCOST.UI.Controls;

/// <summary>The kind of snap a viewport is currently showing.</summary>
public enum SnapKind
{
    None,
    Endpoint,
    Midpoint,
    Center,
    Intersection,
    Perpendicular,
    Nearest,
    Grid
}

/// <summary>A grip drawn on a viewport: the little square that starts an edit.</summary>
public sealed class ViewportGrip
{
    public ViewportGrip(Point3D position, bool isHot = false)
    {
        Position = position;
        IsHot = isHot;
    }

    /// <summary>Where the grip sits, in world coordinates.</summary>
    public Point3D Position { get; }

    /// <summary>Hot grips are filled with the accent colour.</summary>
    public bool IsHot { get; }
}

/// <summary>A temporary dimension drawn while something is being edited.</summary>
public sealed class TemporaryDimension
{
    /// <summary>First witness point, in world coordinates.</summary>
    public Point3D Start { get; set; }

    /// <summary>Second witness point.</summary>
    public Point3D End { get; set; }

    /// <summary>How far the dimension line is pushed off the two points, in pixels.</summary>
    public double Offset { get; set; } = 28;

    /// <summary>Caption; the measured distance is used when this is empty.</summary>
    public string? Text { get; set; }
}

/// <summary>Carries the world point under the pointer, plus whatever snap is active.</summary>
public class ViewportCursorEventArgs : EventArgs
{
    public ViewportCursorEventArgs(Point3D world, Point screen, bool snapped)
    {
        World = world;
        Screen = screen;
        IsSnapped = snapped;
    }

    /// <summary>Point on the view plane, in world coordinates.</summary>
    public Point3D World { get; }

    /// <summary>The same point in surface pixels.</summary>
    public Point Screen { get; }

    /// <summary><c>true</c> when the point was pulled onto a grid intersection.</summary>
    public bool IsSnapped { get; }
}

/// <summary>
/// The drawing surface: a grid, a crosshair with an aperture, snap markers, grips, a rubber-band
/// selection and a temporary dimension, all on top of a <see cref="CameraController"/>.
/// </summary>
/// <remarks>
/// <para>
/// It is a <see cref="Panel"/> rather than a templated control so a host can drop overlays — a
/// <see cref="ViewCube"/>, a <see cref="NavigationBar"/>, a <see cref="MiniMap"/> — straight into
/// <see cref="UIElementCollection"/> and get them positioned over the drawing without a second layer.
/// The drawing itself is painted in <see cref="OnRender"/>, which keeps the element count at one for
/// a grid of ten thousand lines.
/// </para>
/// <para>
/// The default camera is an orthographic plan looking down: pitch 90, yaw -90, so screen X is world X
/// and screen Y is world -Y. Everything below — grid spacing, snapping, the coordinate readout — is
/// therefore plain arithmetic on the world point the camera unprojects.
/// </para>
/// </remarks>
public class ViewportControl : Grid
{
    /// <summary>Identifies the <see cref="ShowGrid"/> property.</summary>
    public static readonly DependencyProperty ShowGridProperty = DependencyProperty.Register(
        nameof(ShowGrid), typeof(bool), typeof(ViewportControl),
        new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Identifies the <see cref="GridStep"/> property.</summary>
    public static readonly DependencyProperty GridStepProperty = DependencyProperty.Register(
        nameof(GridStep), typeof(double), typeof(ViewportControl),
        new FrameworkPropertyMetadata(1000.0, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Identifies the <see cref="ShowCrosshair"/> property.</summary>
    public static readonly DependencyProperty ShowCrosshairProperty = DependencyProperty.Register(
        nameof(ShowCrosshair), typeof(bool), typeof(ViewportControl),
        new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Identifies the <see cref="SnapToGrid"/> property.</summary>
    public static readonly DependencyProperty SnapToGridProperty = DependencyProperty.Register(
        nameof(SnapToGrid), typeof(bool), typeof(ViewportControl), new FrameworkPropertyMetadata(true));

    /// <summary>Identifies the <see cref="SnapDistance"/> property.</summary>
    public static readonly DependencyProperty SnapDistanceProperty = DependencyProperty.Register(
        nameof(SnapDistance), typeof(double), typeof(ViewportControl), new FrameworkPropertyMetadata(10.0));

    /// <summary>Identifies the <see cref="SelectionRectangle"/> property.</summary>
    public static readonly DependencyProperty SelectionRectangleProperty = DependencyProperty.Register(
        nameof(SelectionRectangle), typeof(Rect), typeof(ViewportControl),
        new FrameworkPropertyMetadata(Rect.Empty, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Identifies the <see cref="Snap"/> property.</summary>
    public static readonly DependencyProperty SnapProperty = DependencyProperty.Register(
        nameof(Snap), typeof(Point?), typeof(ViewportControl),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Identifies the <see cref="SnapKind"/> property.</summary>
    public static readonly DependencyProperty SnapKindProperty = DependencyProperty.Register(
        nameof(SnapKind), typeof(SnapKind), typeof(ViewportControl),
        new FrameworkPropertyMetadata(SnapKind.None, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Identifies the <see cref="TemporaryDimension"/> property.</summary>
    public static readonly DependencyProperty TemporaryDimensionProperty = DependencyProperty.Register(
        nameof(TemporaryDimension), typeof(TemporaryDimension), typeof(ViewportControl),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Identifies the <see cref="Grips"/> property.</summary>
    public static readonly DependencyProperty GripsProperty = DependencyProperty.Register(
        nameof(Grips), typeof(IEnumerable<ViewportGrip>), typeof(ViewportControl),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Identifies the <see cref="ShowAxes"/> property.</summary>
    public static readonly DependencyProperty ShowAxesProperty = DependencyProperty.Register(
        nameof(ShowAxes), typeof(bool), typeof(ViewportControl),
        new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Identifies the <see cref="AxisBrush"/> property.</summary>
    public static readonly DependencyProperty AxisBrushProperty = DependencyProperty.Register(
        nameof(AxisBrush), typeof(Brush), typeof(ViewportControl),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Identifies the <see cref="AxisThickness"/> property.</summary>
    public static readonly DependencyProperty AxisThicknessProperty = DependencyProperty.Register(
        nameof(AxisThickness), typeof(double), typeof(ViewportControl),
        new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Identifies the <see cref="AxisDashStyle"/> property.</summary>
    public static readonly DependencyProperty AxisDashStyleProperty = DependencyProperty.Register(
        nameof(AxisDashStyle), typeof(DashStyle), typeof(ViewportControl),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Identifies the <see cref="ShowOriginMarker"/> property.</summary>
    public static readonly DependencyProperty ShowOriginMarkerProperty = DependencyProperty.Register(
        nameof(ShowOriginMarker), typeof(bool), typeof(ViewportControl),
        new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Identifies the <see cref="GridBrush"/> property.</summary>
    public static readonly DependencyProperty GridBrushProperty = DependencyProperty.Register(
        nameof(GridBrush), typeof(Brush), typeof(ViewportControl),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Identifies the <see cref="GridMajorBrush"/> property.</summary>
    public static readonly DependencyProperty GridMajorBrushProperty = DependencyProperty.Register(
        nameof(GridMajorBrush), typeof(Brush), typeof(ViewportControl),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Identifies the <see cref="GridMajorEvery"/> property.</summary>
    public static readonly DependencyProperty GridMajorEveryProperty = DependencyProperty.Register(
        nameof(GridMajorEvery), typeof(int), typeof(ViewportControl),
        new FrameworkPropertyMetadata(5, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Identifies the <see cref="MinGridPixelSpacing"/> property.</summary>
    public static readonly DependencyProperty MinGridPixelSpacingProperty = DependencyProperty.Register(
        nameof(MinGridPixelSpacing), typeof(double), typeof(ViewportControl),
        new FrameworkPropertyMetadata(8.0, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Identifies the <see cref="CrosshairBrush"/> property.</summary>
    public static readonly DependencyProperty CrosshairBrushProperty = DependencyProperty.Register(
        nameof(CrosshairBrush), typeof(Brush), typeof(ViewportControl),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Identifies the <see cref="ShowAperture"/> property.</summary>
    public static readonly DependencyProperty ShowApertureProperty = DependencyProperty.Register(
        nameof(ShowAperture), typeof(bool), typeof(ViewportControl),
        new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Identifies the <see cref="SnapMarkerBrush"/> property.</summary>
    public static readonly DependencyProperty SnapMarkerBrushProperty = DependencyProperty.Register(
        nameof(SnapMarkerBrush), typeof(Brush), typeof(ViewportControl),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Identifies the <see cref="SnapMarkerSize"/> property.</summary>
    public static readonly DependencyProperty SnapMarkerSizeProperty = DependencyProperty.Register(
        nameof(SnapMarkerSize), typeof(double), typeof(ViewportControl),
        new FrameworkPropertyMetadata(5.0, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Identifies the <see cref="ShowSnapLabel"/> property.</summary>
    public static readonly DependencyProperty ShowSnapLabelProperty = DependencyProperty.Register(
        nameof(ShowSnapLabel), typeof(bool), typeof(ViewportControl),
        new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Identifies the <see cref="SelectionBrush"/> property.</summary>
    public static readonly DependencyProperty SelectionBrushProperty = DependencyProperty.Register(
        nameof(SelectionBrush), typeof(Brush), typeof(ViewportControl),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Identifies the <see cref="SelectionFillOpacity"/> property.</summary>
    public static readonly DependencyProperty SelectionFillOpacityProperty = DependencyProperty.Register(
        nameof(SelectionFillOpacity), typeof(double), typeof(ViewportControl),
        new FrameworkPropertyMetadata(0.12, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Identifies the <see cref="DimensionBrush"/> property.</summary>
    public static readonly DependencyProperty DimensionBrushProperty = DependencyProperty.Register(
        nameof(DimensionBrush), typeof(Brush), typeof(ViewportControl),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Identifies the <see cref="FitMargin"/> property.</summary>
    public static readonly DependencyProperty FitMarginProperty = DependencyProperty.Register(
        nameof(FitMargin), typeof(double), typeof(ViewportControl),
        new FrameworkPropertyMetadata(1.1));

    private readonly System.Windows.Controls.Canvas _overlay;

    private CameraController _camera;
    private Point? _pointer;
    private Point _panOrigin;
    private bool _panning;

    public ViewportControl()
    {
        _camera = CreatePlanCamera();
        _camera.Changed += OnCameraChanged;

        ClipToBounds = true;
        Focusable = true;

        SetResourceReference(Panel.BackgroundProperty, "CONCOST.Brush.Surface");

        _overlay = new System.Windows.Controls.Canvas { IsHitTestVisible = false };
        Children.Add(_overlay);
    }

    /// <summary>Raised as the pointer moves, with the world point under it.</summary>
    public event EventHandler<ViewportCursorEventArgs>? CursorMoved;

    /// <summary>Raised when the left button is released after a click that was not a pan.</summary>
    public event EventHandler<ViewportCursorEventArgs>? WorldClicked;

    /// <summary>
    /// The camera that maps world to surface. Replacing it rewires the change notification; the
    /// default is an orthographic plan.
    /// </summary>
    public CameraController Camera
    {
        get => _camera;
        set
        {
            if (value is null || ReferenceEquals(value, _camera)) return;

            _camera.Changed -= OnCameraChanged;
            _camera = value;
            _camera.Changed += OnCameraChanged;
            InvalidateVisual();
        }
    }

    /// <summary>The layer above the drawing; hosts put a compass, a mini map or a coach mark here.</summary>
    public System.Windows.Controls.Canvas Overlay => _overlay;

    /// <summary>Paints the grid.</summary>
    public bool ShowGrid
    {
        get => (bool)GetValue(ShowGridProperty);
        set => SetValue(ShowGridProperty, value);
    }

    /// <summary>Grid spacing in world units. It doubles automatically when it gets too dense to read.</summary>
    public double GridStep
    {
        get => (double)GetValue(GridStepProperty);
        set => SetValue(GridStepProperty, value);
    }

    /// <summary>Paints the CAD crosshair under the pointer.</summary>
    public bool ShowCrosshair
    {
        get => (bool)GetValue(ShowCrosshairProperty);
        set => SetValue(ShowCrosshairProperty, value);
    }

    /// <summary>Pulls the pointer onto the nearest grid intersection before reporting it.</summary>
    public bool SnapToGrid
    {
        get => (bool)GetValue(SnapToGridProperty);
        set => SetValue(SnapToGridProperty, value);
    }

    /// <summary>How close, in pixels, the pointer has to be to a grid intersection to snap.</summary>
    public double SnapDistance
    {
        get => (double)GetValue(SnapDistanceProperty);
        set => SetValue(SnapDistanceProperty, value);
    }

    /// <summary>Rubber-band rectangle in surface pixels; set it while dragging a window selection.</summary>
    public Rect SelectionRectangle
    {
        get => (Rect)GetValue(SelectionRectangleProperty);
        set => SetValue(SelectionRectangleProperty, value);
    }

    /// <summary>The active snap marker, in surface pixels.</summary>
    public Point? Snap
    {
        get => (Point?)GetValue(SnapProperty);
        set => SetValue(SnapProperty, value);
    }

    /// <summary>Which marker shape is drawn at <see cref="Snap"/>.</summary>
    public SnapKind SnapKind
    {
        get => (SnapKind)GetValue(SnapKindProperty);
        set => SetValue(SnapKindProperty, value);
    }

    /// <summary>Dimension shown while a distance is being edited.</summary>
    public TemporaryDimension? TemporaryDimension
    {
        get => (TemporaryDimension?)GetValue(TemporaryDimensionProperty);
        set => SetValue(TemporaryDimensionProperty, value);
    }

    /// <summary>Grips drawn over the selected geometry.</summary>
    public IEnumerable<ViewportGrip>? Grips
    {
        get => (IEnumerable<ViewportGrip>?)GetValue(GripsProperty);
        set => SetValue(GripsProperty, value);
    }

    /// <summary>
    /// Draws the two world axes through the model origin. They are a datum, not a warning: the
    /// default brush is <c>CONCOST.Brush.CanvasAxis</c> and the default style is a dash.
    /// </summary>
    public bool ShowAxes
    {
        get => (bool)GetValue(ShowAxesProperty);
        set => SetValue(ShowAxesProperty, value);
    }

    /// <summary>Overrides the axis brush. Null keeps the theme token, which follows the theme at runtime.</summary>
    public Brush? AxisBrush
    {
        get => (Brush?)GetValue(AxisBrushProperty);
        set => SetValue(AxisBrushProperty, value);
    }

    /// <summary>Stroke width of the axis lines, in pixels.</summary>
    public double AxisThickness
    {
        get => (double)GetValue(AxisThicknessProperty);
        set => SetValue(AxisThicknessProperty, value);
    }

    /// <summary>
    /// Dash pattern of the axis lines. Null draws the default dash; <c>DashStyles.Solid</c> draws a
    /// continuous line, which is what a host overlaying a plan on a survey grid usually wants.
    /// </summary>
    public DashStyle? AxisDashStyle
    {
        get => (DashStyle?)GetValue(AxisDashStyleProperty);
        set => SetValue(AxisDashStyleProperty, value);
    }

    /// <summary>
    /// Draws a small ring at the model origin so the datum is findable when the axes are faded, or
    /// switched off. It stands on its own: turning <see cref="ShowAxes"/> off leaves the ring.
    /// </summary>
    public bool ShowOriginMarker
    {
        get => (bool)GetValue(ShowOriginMarkerProperty);
        set => SetValue(ShowOriginMarkerProperty, value);
    }

    /// <summary>Overrides the minor grid brush. Null keeps <c>CONCOST.Brush.CanvasGrid</c>.</summary>
    public Brush? GridBrush
    {
        get => (Brush?)GetValue(GridBrushProperty);
        set => SetValue(GridBrushProperty, value);
    }

    /// <summary>Overrides the heavier every-n-th grid brush. Null keeps <c>CONCOST.Brush.CanvasGridMajor</c>.</summary>
    public Brush? GridMajorBrush
    {
        get => (Brush?)GetValue(GridMajorBrushProperty);
        set => SetValue(GridMajorBrushProperty, value);
    }

    /// <summary>One line in every <c>GridMajorEvery</c> is drawn with the heavier brush. Values below two are clamped.</summary>
    public int GridMajorEvery
    {
        get => (int)GetValue(GridMajorEveryProperty);
        set => SetValue(GridMajorEveryProperty, value);
    }

    /// <summary>
    /// Smallest gap, in pixels, the grid may have between two lines. Below it the step doubles until
    /// the lines are readable again, which is what keeps a zoomed-out plan from becoming a grey wash.
    /// </summary>
    public double MinGridPixelSpacing
    {
        get => (double)GetValue(MinGridPixelSpacingProperty);
        set => SetValue(MinGridPixelSpacingProperty, value);
    }

    /// <summary>Overrides the crosshair brush. Null keeps <c>CONCOST.Brush.TextSecondary</c>.</summary>
    public Brush? CrosshairBrush
    {
        get => (Brush?)GetValue(CrosshairBrushProperty);
        set => SetValue(CrosshairBrushProperty, value);
    }

    /// <summary>Draws the small box at the pointer that marks "here" when no snap has been taken.</summary>
    public bool ShowAperture
    {
        get => (bool)GetValue(ShowApertureProperty);
        set => SetValue(ShowApertureProperty, value);
    }

    /// <summary>Overrides the snap marker brush. Null keeps the accent colour.</summary>
    public Brush? SnapMarkerBrush
    {
        get => (Brush?)GetValue(SnapMarkerBrushProperty);
        set => SetValue(SnapMarkerBrushProperty, value);
    }

    /// <summary>Half-size of the snap marker glyph, in pixels.</summary>
    public double SnapMarkerSize
    {
        get => (double)GetValue(SnapMarkerSizeProperty);
        set => SetValue(SnapMarkerSizeProperty, value);
    }

    /// <summary>Draws the snap kind (ENDPOINT, MIDPOINT, GRID ...) beside the marker.</summary>
    public bool ShowSnapLabel
    {
        get => (bool)GetValue(ShowSnapLabelProperty);
        set => SetValue(ShowSnapLabelProperty, value);
    }

    /// <summary>Overrides the rubber-band brush. Null keeps the accent colour.</summary>
    public Brush? SelectionBrush
    {
        get => (Brush?)GetValue(SelectionBrushProperty);
        set => SetValue(SelectionBrushProperty, value);
    }

    /// <summary>Fill opacity of the rubber band, 0 to 1. Zero draws the outline only.</summary>
    public double SelectionFillOpacity
    {
        get => (double)GetValue(SelectionFillOpacityProperty);
        set => SetValue(SelectionFillOpacityProperty, value);
    }

    /// <summary>Overrides the temporary-dimension brush. Null keeps the accent colour.</summary>
    public Brush? DimensionBrush
    {
        get => (Brush?)GetValue(DimensionBrushProperty);
        set => SetValue(DimensionBrushProperty, value);
    }

    /// <summary>
    /// How much empty space <see cref="ZoomToFit(Rect3D)"/> leaves around the bounds; 1.1 is a tenth.
    /// One means flush.
    /// </summary>
    public double FitMargin
    {
        get => (double)GetValue(FitMarginProperty);
        set => SetValue(FitMarginProperty, value);
    }

    /// <summary>
    /// <see cref="FitMargin"/> in the form the camera needs: a multiplier that may not shrink the
    /// view below the bounds. One is flush, so values under one are clamped rather than accepted.
    /// </summary>
    private double FitMarginNormalized
    {
        get
        {
            double margin = FitMargin;
            if (double.IsNaN(margin)) return 1.1;
            return margin < 1 ? 1 : margin;
        }
    }

    /// <summary>Converts a world point to surface pixels.</summary>
    public Point ToScreen(Point3D world)
    {
        _camera.TryProject(world, RenderSize, out var screen);
        return screen;
    }

    /// <summary>The world point under a surface pixel, on the view plane.</summary>
    public Point3D ToWorld(Point screen) => _camera.Unproject(screen, RenderSize);

    /// <summary>Zooms so <paramref name="bounds"/> fills the surface.</summary>
    public void ZoomToFit(Rect3D bounds)
    {
        _camera.FitTo(bounds);

        // CameraController.FitTo frames the sphere around the bounds. That is right for a perspective
        // view, but wasteful for a plan: the drawing lands at half the size the pane could show. An
        // orthographic camera is framed against the bounds projected onto its own screen axes.
        if (_camera.Projection != CameraProjection.Orthographic) return;

        Size size = RenderSize;

        if (bounds.IsEmpty || size.Width <= 0 || size.Height <= 0) return;

        Vector3D right = _camera.Right;
        Vector3D up = _camera.Up;

        double width = Math.Abs(right.X) * bounds.SizeX + Math.Abs(right.Y) * bounds.SizeY + Math.Abs(right.Z) * bounds.SizeZ;
        double height = Math.Abs(up.X) * bounds.SizeX + Math.Abs(up.Y) * bounds.SizeY + Math.Abs(up.Z) * bounds.SizeZ;

        _camera.OrthographicHeight = Math.Max(height, width * size.Height / size.Width) * FitMarginNormalized;
    }

    /// <summary>Turns the grid on or off; the toggle the status bar drives.</summary>
    public void ToggleGrid() => ShowGrid = !ShowGrid;

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);

        if (RenderSize.Width <= 0 || RenderSize.Height <= 0) return;

        // Every layer below takes its brush from a property whose fallback is the theme token, so a
        // host can retune one surface without templating the control, and the theme still drives
        // the default look. The accent is read once because the snap, grip, rubber-band and
        // dimension layers all default to it.
        var accent = FindBrush("CONCOST.Brush.Primary", Brushes.Orange);

        DrawGrid(dc);
        DrawAxes(dc);
        DrawSelection(dc, accent);
        DrawGrips(dc, accent);
        DrawDimension(dc, accent);
        DrawCrosshair(dc);
        DrawSnap(dc, accent);
    }

    private void DrawGrid(DrawingContext dc)
    {
        if (!ShowGrid) return;

        double scale = UnitsToPixels(1);
        if (scale <= 0) return;

        double step = Math.Max(1e-6, GridStep);
        double minSpacing = double.IsNaN(MinGridPixelSpacing) ? 8 : Math.Max(2, MinGridPixelSpacing);

        // Keep at least `minSpacing` pixels between lines; below that the grid is a grey wash.
        while (step * scale < minSpacing) step *= 2;

        var visible = GetVisibleWorld();
        if (visible.IsEmpty) return;

        double startX = Math.Floor(visible.Left / step) * step;
        double startY = Math.Floor(visible.Top / step) * step;

        int count = 0;

        // One pen per weight for the whole frame. A pen per line allocates several thousand objects
        // on every camera move, which is most of the cost of painting the grid.
        var minorPen = new Pen(GridBrush ?? FindBrush("CONCOST.Brush.CanvasGrid", Brushes.Gainsboro), 1);
        var majorPen = new Pen(GridMajorBrush ?? FindBrush("CONCOST.Brush.CanvasGridMajor", Brushes.Silver), 1);

        int majorEvery = Math.Max(2, GridMajorEvery);

        for (double x = startX; x <= visible.Right && count < 4000; x += step, count++)
        {
            var top = ToScreen(new Point3D(x, visible.Top, 0));
            var bottom = ToScreen(new Point3D(x, visible.Bottom, 0));

            dc.DrawLine(WeightPen(x, step, majorEvery, minorPen, majorPen), top, bottom);
        }

        count = 0;

        for (double y = startY; y <= visible.Bottom && count < 4000; y += step, count++)
        {
            var left = ToScreen(new Point3D(visible.Left, y, 0));
            var right = ToScreen(new Point3D(visible.Right, y, 0));

            dc.DrawLine(WeightPen(y, step, majorEvery, minorPen, majorPen), left, right);
        }
    }

    /// <summary>Minor line, every n-th heavier. The datum is a separate pass so hiding the grid keeps it.</summary>
    private static Pen WeightPen(double value, double step, int majorEvery, Pen minor, Pen major)
    {
        // The rounding tolerance keeps a line at exactly five steps from landing on either side of
        // the comparison and changing weight as the view scrolls.
        double units = step * majorEvery;
        return Math.Abs(value / units - Math.Round(value / units)) < 1e-6 ? major : minor;
    }

    private void DrawAxes(DrawingContext dc)
    {
        if (!ShowAxes && !ShowOriginMarker) return;

        var brush = AxisBrush ?? FindBrush("CONCOST.Brush.CanvasAxis", Brushes.Silver);

        var origin = ToScreen(new Point3D(0, 0, 0));
        if (!IsFinite(origin)) return;

        if (ShowAxes)
        {
            double thickness = double.IsNaN(AxisThickness) || AxisThickness <= 0 ? 1 : AxisThickness;
            var pen = new Pen(brush, thickness) { DashStyle = AxisDashStyle ?? DashStyles.Dash };
            double reach = Math.Sqrt(RenderSize.Width * RenderSize.Width + RenderSize.Height * RenderSize.Height);

            // The axis directions are projected rather than assumed to be horizontal and vertical: at
            // a plan they are, after an orbit they are not. An axis the camera looks straight down
            // collapses to a point and is skipped instead of being drawn as a zero-length line.
            DrawAxis(dc, pen, origin, ToScreen(new Point3D(1, 0, 0)), reach);
            DrawAxis(dc, pen, origin, ToScreen(new Point3D(0, 1, 0)), reach);
        }

        if (ShowOriginMarker)
        {
            // A small ring, so the datum is still findable at a zoom where the dashed lines fade out.
            dc.DrawEllipse(null, new Pen(brush, 1.2), origin, 3.5, 3.5);
        }
    }

    private static void DrawAxis(DrawingContext dc, Pen pen, Point origin, Point reference, double reach)
    {
        if (!IsFinite(reference)) return;

        Vector direction = reference - origin;
        if (direction.Length < 1e-6) return;

        direction.Normalize();
        dc.DrawLine(pen, origin - direction * reach, origin + direction * reach);
    }

    private static bool IsFinite(Point point) =>
        !double.IsNaN(point.X) && !double.IsNaN(point.Y) &&
        !double.IsInfinity(point.X) && !double.IsInfinity(point.Y);

    private void DrawSelection(DrawingContext dc, Brush accent)
    {
        var rect = SelectionRectangle;
        if (rect.IsEmpty || rect.Width <= 0 || rect.Height <= 0) return;

        var brush = SelectionBrush ?? accent;

        double opacity = double.IsNaN(SelectionFillOpacity) ? 0 : SelectionFillOpacity;
        if (opacity < 0) opacity = 0;
        else if (opacity > 1) opacity = 1;

        Brush? fill = null;

        // Zero is a request for an outline only, which is what a pick box wants: it must not tint
        // the very geometry the user is trying to read underneath it.
        if (opacity > 0)
        {
            var solid = new SolidColorBrush((brush as SolidColorBrush)?.Color ?? Colors.Orange) { Opacity = opacity };
            solid.Freeze();
            fill = solid;
        }

        var pen = new Pen(brush, 1) { DashStyle = DashStyles.Dash };

        dc.DrawRectangle(fill, pen, rect);
    }

    private void DrawGrips(DrawingContext dc, Brush accent)
    {
        if (Grips is null) return;

        var fill = FindBrush("CONCOST.Brush.Surface", Brushes.White);
        var pen = new Pen(accent, 1);

        foreach (var grip in Grips)
        {
            var point = ToScreen(grip.Position);
            var square = new Rect(point.X - 3, point.Y - 3, 6, 6);

            dc.DrawRectangle(grip.IsHot ? accent : fill, pen, square);
        }
    }

    private void DrawDimension(DrawingContext dc, Brush accent)
    {
        var dimension = TemporaryDimension;
        if (dimension is null) return;

        var start = ToScreen(dimension.Start);
        var end = ToScreen(dimension.End);

        double dx = end.X - start.X;
        double dy = end.Y - start.Y;
        double length = Math.Sqrt(dx * dx + dy * dy);

        if (length < 1e-6) return;

        // Offset perpendicular to the measured direction, in pixels, so it stays legible at any zoom.
        double offset = Math.Abs(dimension.Offset) < 1e-6 ? 28 : dimension.Offset;
        double nx = -dy / length * offset;
        double ny = dx / length * offset;

        var a = new Point(start.X + nx, start.Y + ny);
        var b = new Point(end.X + nx, end.Y + ny);

        var brush = DimensionBrush ?? accent;
        var pen = new Pen(brush, 1);

        dc.DrawLine(new Pen(brush, 0.7), start, a);
        dc.DrawLine(new Pen(brush, 0.7), end, b);
        dc.DrawLine(pen, a, b);

        // Witness ticks at both ends.
        double tx = -ny / offset * 4;
        double ty = nx / offset * 4;

        dc.DrawLine(pen, new Point(a.X - tx, a.Y - ty), new Point(a.X + tx, a.Y + ty));
        dc.DrawLine(pen, new Point(b.X - tx, b.Y - ty), new Point(b.X + tx, b.Y + ty));

        var worldStart = ToWorld(start);
        var worldEnd = ToWorld(end);
        double distance = Math.Sqrt(
            (worldEnd.X - worldStart.X) * (worldEnd.X - worldStart.X) +
            (worldEnd.Y - worldStart.Y) * (worldEnd.Y - worldStart.Y));

        string caption = string.IsNullOrWhiteSpace(dimension.Text)
            ? distance.ToString("0", CultureInfo.CurrentCulture)
            : dimension.Text!;

        var formatted = CreateText(caption, 11, FindBrush("CONCOST.Brush.TextSecondary", Brushes.Gray));
        var middle = new Point((a.X + b.X) / 2, (a.Y + b.Y) / 2);

        // A plate behind the caption keeps it readable over the grid.
        var plate = new Rect(middle.X - formatted.Width / 2 - 3, middle.Y - formatted.Height / 2 - 1,
            formatted.Width + 6, formatted.Height + 2);

        dc.DrawRectangle(FindBrush("CONCOST.Brush.Surface", Brushes.White), null, plate);
        dc.DrawText(formatted, new Point(plate.X + 3, plate.Y + 1));
    }

    private void DrawCrosshair(DrawingContext dc)
    {
        if (!ShowCrosshair || _pointer is null) return;

        var brush = CrosshairBrush ?? FindBrush("CONCOST.Brush.TextSecondary", Brushes.Gray);
        var point = _pointer.Value;
        var pen = new Pen(brush, 0.7) { DashStyle = DashStyles.Dot };

        dc.DrawLine(pen, new Point(0, point.Y), new Point(RenderSize.Width, point.Y));
        dc.DrawLine(pen, new Point(point.X, 0), new Point(point.X, RenderSize.Height));

        if (!ShowAperture) return;

        // The aperture: the small box that tells the user where "here" is when a snap is not taken.
        var box = new Rect(point.X - 5, point.Y - 5, 10, 10);
        dc.DrawRectangle(null, new Pen(brush, 1), box);
    }

    private void DrawSnap(DrawingContext dc, Brush accent)
    {
        if (Snap is not { } snap || SnapKind == SnapKind.None) return;

        var brush = SnapMarkerBrush ?? accent;
        var pen = new Pen(brush, 1.6);
        double size = double.IsNaN(SnapMarkerSize) ? 5 : Math.Max(2, SnapMarkerSize);

        switch (SnapKind)
        {
            case SnapKind.Endpoint:
                dc.DrawRectangle(null, pen, new Rect(snap.X - size, snap.Y - size, size * 2, size * 2));
                break;

            case SnapKind.Midpoint:
                dc.DrawGeometry(null, pen, Triangle(new Point(snap.X, snap.Y - size - 1),
                    new Point(snap.X + size + 1, snap.Y + size), new Point(snap.X - size - 1, snap.Y + size)));
                break;

            case SnapKind.Center:
                dc.DrawEllipse(null, pen, snap, size + 1, size + 1);
                break;

            case SnapKind.Intersection:
                dc.DrawLine(pen, new Point(snap.X - size, snap.Y - size), new Point(snap.X + size, snap.Y + size));
                dc.DrawLine(pen, new Point(snap.X + size, snap.Y - size), new Point(snap.X - size, snap.Y + size));
                break;

            case SnapKind.Perpendicular:
                dc.DrawLine(pen, new Point(snap.X - size, snap.Y - size), new Point(snap.X - size, snap.Y + size));
                dc.DrawLine(pen, new Point(snap.X - size, snap.Y + size), new Point(snap.X + size, snap.Y + size));
                break;

            case SnapKind.Nearest:
                dc.DrawGeometry(brush, pen, Triangle(new Point(snap.X - size, snap.Y),
                    new Point(snap.X + size, snap.Y), new Point(snap.X + size + 3, snap.Y - size * 2)));
                break;

            default:
                dc.DrawRectangle(brush, null, new Rect(snap.X - 2, snap.Y - 2, 4, 4));
                break;
        }

        if (!ShowSnapLabel) return;

        var label = CreateText(SnapKind.ToString().ToUpperInvariant(), 9,
            FindBrush("CONCOST.Brush.TextSecondary", Brushes.Gray));
        dc.DrawText(label, new Point(snap.X + 9, snap.Y + 7));
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);

        var point = e.GetPosition(this);
        _pointer = point;

        if (_panning)
        {
            var delta = new Point(point.X - _panOrigin.X, point.Y - _panOrigin.Y);
            _camera.Pan(delta.X, delta.Y, RenderSize, isFinal: false);
            _panOrigin = point;

            InvalidateVisual();
            return;
        }

        var world = _camera.Unproject(point, RenderSize);
        bool snapped = TrySnapToGrid(point, world, out var snappedWorld);

        CursorMoved?.Invoke(this, new ViewportCursorEventArgs(snappedWorld, point, snapped));
        InvalidateVisual();
    }

    protected override void OnMouseLeave(MouseEventArgs e)
    {
        base.OnMouseLeave(e);

        _pointer = null;
        InvalidateVisual();
    }

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);

        Focus();
        _pointer = e.GetPosition(this);
        InvalidateVisual();
    }

    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonUp(e);

        if (_panning) return;

        var point = e.GetPosition(this);
        var world = _camera.Unproject(point, RenderSize);

        bool snapped = TrySnapToGrid(point, world, out var snappedWorld);
        if (snapped) world = snappedWorld;

        WorldClicked?.Invoke(this, new ViewportCursorEventArgs(world, point, snapped));
    }

    protected override void OnMouseDown(MouseButtonEventArgs e)
    {
        base.OnMouseDown(e);

        if (e.ChangedButton != MouseButton.Middle && !(e.ChangedButton == MouseButton.Left && Keyboard.IsKeyDown(Key.Space)))
        {
            return;
        }

        _panning = true;
        _panOrigin = e.GetPosition(this);
        Cursor = Cursors.SizeAll;
        CaptureMouse();
        e.Handled = true;
    }

    protected override void OnMouseUp(MouseButtonEventArgs e)
    {
        base.OnMouseUp(e);

        if (!_panning) return;

        _panning = false;
        Cursor = null;
        ReleaseMouseCapture();
        _camera.Pan(0, 0, RenderSize);
    }

    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
        base.OnMouseWheel(e);

        if (e.Delta == 0) return;

        // Zoom in means a factor below one, because the factor multiplies the distance.
        _camera.ZoomAt(e.GetPosition(this), RenderSize, e.Delta > 0 ? 1 / 1.15 : 1.15);
        InvalidateVisual();
        e.Handled = true;
    }

    private bool TrySnapToGrid(Point point, Point3D world, out Point3D snapped)
    {
        snapped = world;

        Snap = null;
        SnapKind = SnapKind.None;

        if (!SnapToGrid) return false;

        double step = Math.Max(1e-6, GridStep);
        double scale = UnitsToPixels(1);
        if (scale <= 0) return false;

        while (step * scale < 8) step *= 2;

        var target = new Point3D(Math.Round(world.X / step) * step, Math.Round(world.Y / step) * step, world.Z);
        var targetScreen = ToScreen(target);

        double dx = targetScreen.X - point.X;
        double dy = targetScreen.Y - point.Y;

        if (Math.Sqrt(dx * dx + dy * dy) > Math.Max(2, SnapDistance)) return false;

        snapped = target;
        Snap = targetScreen;
        SnapKind = SnapKind.Grid;
        return true;
    }

    /// <summary>How many pixels one world unit covers, measured along the view's horizontal axis.</summary>
    public double UnitsToPixels(double units)
    {
        if (RenderSize.Height <= 0) return 0;

        if (_camera.Projection == CameraProjection.Orthographic)
        {
            return units * RenderSize.Height / _camera.OrthographicHeight;
        }

        return units * RenderSize.Height / (_camera.Distance * 2 * Math.Tan(_camera.FieldOfView * Math.PI / 360));
    }

    private Rect GetVisibleWorld()
    {
        if (RenderSize.Width <= 0 || RenderSize.Height <= 0) return Rect.Empty;

        var corners = new[]
        {
            _camera.Unproject(new Point(0, 0), RenderSize),
            _camera.Unproject(new Point(RenderSize.Width, 0), RenderSize),
            _camera.Unproject(new Point(0, RenderSize.Height), RenderSize),
            _camera.Unproject(new Point(RenderSize.Width, RenderSize.Height), RenderSize)
        };

        double minX = double.MaxValue, minY = double.MaxValue, maxX = double.MinValue, maxY = double.MinValue;

        foreach (var corner in corners)
        {
            minX = Math.Min(minX, corner.X);
            minY = Math.Min(minY, corner.Y);
            maxX = Math.Max(maxX, corner.X);
            maxY = Math.Max(maxY, corner.Y);
        }

        return new Rect(new Point(minX, minY), new Point(maxX, maxY));
    }

    private void OnCameraChanged(object? sender, CameraChangedEventArgs e) => InvalidateVisual();

    private static CameraController CreatePlanCamera() => new()
    {
        Projection = CameraProjection.Orthographic,
        Yaw = -90,
        Pitch = 90,
        OrthographicHeight = 20000,
        Distance = 10000
    };

    private static Geometry Triangle(Point a, Point b, Point c)
    {
        var geometry = new StreamGeometry();

        using (var context = geometry.Open())
        {
            context.BeginFigure(a, true, true);
            context.LineTo(b, true, true);
            context.LineTo(c, true, true);
        }

        geometry.Freeze();
        return geometry;
    }

    private Brush FindBrush(string key, Brush fallback) => TryFindResource(key) as Brush ?? fallback;

    private FormattedText CreateText(string caption, double size, Brush brush) =>
        new(caption, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Segoe UI"), size, brush,
            VisualTreeHelper.GetDpi(this).PixelsPerDip);
}
