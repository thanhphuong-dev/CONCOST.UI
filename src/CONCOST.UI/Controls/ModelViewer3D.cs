using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace CONCOST.UI.Controls;

/// <summary>An edge between two vertices of a <see cref="ModelViewer3D"/> mesh.</summary>
public readonly struct MeshEdge
{
    public MeshEdge(int a, int b)
    {
        A = a;
        B = b;
    }

    /// <summary>Index of the first vertex.</summary>
    public int A { get; }

    /// <summary>Index of the second vertex.</summary>
    public int B { get; }
}

/// <summary>
/// A wireframe viewer for a building mass or a mechanical part: orbit with the left button, pan with
/// the right one, zoom with the wheel.
/// </summary>
/// <remarks>
/// <para>
/// This is not a renderer. It draws edges with a rotating orthographic camera plus a mild perspective
/// divide, which is enough to sanity-check a mass, pick an orientation and hand it to a real engine —
/// and it needs no DirectX host, no shader and no dedicated window. For a shaded model with thousands
/// of triangles the <c>ViewportHost</c> pattern is still the right answer.
/// </para>
/// <para>
/// <see cref="Project"/> is public so a host can place 2D overlays (dimensions, labels, grips) on the
/// same projection the control uses; anything drawn with it lands exactly on the wireframe.
/// </para>
/// </remarks>
public class ModelViewer3D : FrameworkElement
{
    /// <summary>Identifies the <see cref="Vertices"/> property.</summary>
    public static readonly DependencyProperty VerticesProperty = DependencyProperty.Register(
        nameof(Vertices), typeof(ObservableCollection<Point3D>), typeof(ModelViewer3D),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, OnGeometryChanged));

    /// <summary>Identifies the <see cref="Edges"/> property.</summary>
    public static readonly DependencyProperty EdgesProperty = DependencyProperty.Register(
        nameof(Edges), typeof(ObservableCollection<MeshEdge>), typeof(ModelViewer3D),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, OnGeometryChanged));

    /// <summary>Identifies the <see cref="Yaw"/> property.</summary>
    public static readonly DependencyProperty YawProperty = DependencyProperty.Register(
        nameof(Yaw), typeof(double), typeof(ModelViewer3D),
        new FrameworkPropertyMetadata(-35d, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Identifies the <see cref="Pitch"/> property.</summary>
    public static readonly DependencyProperty PitchProperty = DependencyProperty.Register(
        nameof(Pitch), typeof(double), typeof(ModelViewer3D),
        new FrameworkPropertyMetadata(-20d, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Identifies the <see cref="Zoom"/> property.</summary>
    public static readonly DependencyProperty ZoomProperty = DependencyProperty.Register(
        nameof(Zoom), typeof(double), typeof(ModelViewer3D),
        new FrameworkPropertyMetadata(1d, FrameworkPropertyMetadataOptions.AffectsRender, null, CoerceZoom));

    /// <summary>Identifies the <see cref="Pan"/> property.</summary>
    public static readonly DependencyProperty PanProperty = DependencyProperty.Register(
        nameof(Pan), typeof(Vector), typeof(ModelViewer3D),
        new FrameworkPropertyMetadata(new Vector(0, 0), FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Identifies the <see cref="ShowAxes"/> property.</summary>
    public static readonly DependencyProperty ShowAxesProperty = DependencyProperty.Register(
        nameof(ShowAxes), typeof(bool), typeof(ModelViewer3D),
        new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Identifies the <see cref="EdgeBrush"/> property.</summary>
    public static readonly DependencyProperty EdgeBrushProperty = DependencyProperty.Register(
        nameof(EdgeBrush), typeof(Brush), typeof(ModelViewer3D),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Identifies the <see cref="Background"/> property.</summary>
    public static readonly DependencyProperty BackgroundProperty = DependencyProperty.Register(
        nameof(Background), typeof(Brush), typeof(ModelViewer3D),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    private Point _orbitOrigin;
    private Vector _panStart;
    private double _yawStart;
    private double _pitchStart;
    private bool _orbiting;
    private bool _panning;

    /// <summary>The mesh vertices, in model units.</summary>
    public ObservableCollection<Point3D> Vertices
    {
        get
        {
            if (GetValue(VerticesProperty) is not ObservableCollection<Point3D> vertices)
            {
                vertices = new ObservableCollection<Point3D>();
                SetValue(VerticesProperty, vertices);
            }

            return vertices;
        }

        set => SetValue(VerticesProperty, value);
    }

    /// <summary>The mesh edges, as pairs of vertex indices.</summary>
    public ObservableCollection<MeshEdge> Edges
    {
        get
        {
            if (GetValue(EdgesProperty) is not ObservableCollection<MeshEdge> edges)
            {
                edges = new ObservableCollection<MeshEdge>();
                SetValue(EdgesProperty, edges);
            }

            return edges;
        }

        set => SetValue(EdgesProperty, value);
    }

    /// <summary>Rotation around the vertical axis, in degrees.</summary>
    public double Yaw
    {
        get => (double)GetValue(YawProperty);
        set => SetValue(YawProperty, value);
    }

    /// <summary>Rotation around the horizontal axis, in degrees; clamped to -89..89.</summary>
    public double Pitch
    {
        get => (double)GetValue(PitchProperty);
        set => SetValue(PitchProperty, value);
    }

    /// <summary>Scale multiplier, clamped to 0.1..20.</summary>
    public double Zoom
    {
        get => (double)GetValue(ZoomProperty);
        set => SetValue(ZoomProperty, value);
    }

    /// <summary>Screen-space offset from the centre of the control.</summary>
    public Vector Pan
    {
        get => (Vector)GetValue(PanProperty);
        set => SetValue(PanProperty, value);
    }

    /// <summary>Draws the X/Y/Z gizmo in the corner.</summary>
    public bool ShowAxes
    {
        get => (bool)GetValue(ShowAxesProperty);
        set => SetValue(ShowAxesProperty, value);
    }

    /// <summary>Colour of the wireframe.</summary>
    public Brush? EdgeBrush
    {
        get => (Brush?)GetValue(EdgeBrushProperty);
        set => SetValue(EdgeBrushProperty, value);
    }

    /// <summary>Custom brush for the viewport background; null leaves the background transparent.</summary>
    public Brush? Background
    {
        get => (Brush?)GetValue(BackgroundProperty);
        set => SetValue(BackgroundProperty, value);
    }

    private static object CoerceZoom(DependencyObject d, object baseValue)
    {
        double value = (double)baseValue;
        if (double.IsNaN(value) || double.IsInfinity(value)) return 1;
        return Math.Max(0.1, Math.Min(20, value));
    }

    private static void OnGeometryChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ModelViewer3D viewer) viewer.InvalidateVisual();
    }

    /// <summary>Replaces the mesh in one call; the collections are cleared and refilled once.</summary>
    public void SetMesh(IEnumerable<Point3D> vertices, IEnumerable<MeshEdge> edges)
    {
        Vertices.Clear();
        Edges.Clear();

        foreach (var vertex in vertices) Vertices.Add(vertex);
        foreach (var edge in edges) Edges.Add(edge);
    }

    /// <summary>Builds the edges of a box, the quickest way to have something to look at.</summary>
    public void LoadBox(Point3D corner, double width, double depth, double height)
    {
        var vertices = new List<Point3D>
        {
            new(corner.X, corner.Y, corner.Z),
            new(corner.X + width, corner.Y, corner.Z),
            new(corner.X + width, corner.Y + depth, corner.Z),
            new(corner.X, corner.Y + depth, corner.Z),
            new(corner.X, corner.Y, corner.Z + height),
            new(corner.X + width, corner.Y, corner.Z + height),
            new(corner.X + width, corner.Y + depth, corner.Z + height),
            new(corner.X, corner.Y + depth, corner.Z + height)
        };

        var edges = new List<MeshEdge>
        {
            new(0, 1), new(1, 2), new(2, 3), new(3, 0),
            new(4, 5), new(5, 6), new(6, 7), new(7, 4),
            new(0, 4), new(1, 5), new(2, 6), new(3, 7)
        };

        SetMesh(vertices, edges);
    }

    /// <summary>Puts the camera back to the default isometric-ish view.</summary>
    public void ResetView()
    {
        SetCurrentValue(YawProperty, -35d);
        SetCurrentValue(PitchProperty, -20d);
        SetCurrentValue(ZoomProperty, 1d);
        SetCurrentValue(PanProperty, new Vector(0, 0));
    }

    /// <summary>Orbits by a screen delta; a pixel is roughly half a degree.</summary>
    public void Orbit(double deltaX, double deltaY)
    {
        Yaw = (Yaw + deltaX * 0.5) % 360;

        double pitch = Pitch - deltaY * 0.5;
        Pitch = Math.Max(-89, Math.Min(89, pitch));
    }

    /// <summary>
    /// Projects a model point to control coordinates. Returns <c>false</c> when the mesh has no
    /// extent yet, in which case there is no scale to project with.
    /// </summary>
    public bool Project(Point3D point, out Point screen)
    {
        screen = default;

        var (centre, radius) = Bounds();
        if (radius <= 0 || ActualWidth <= 0 || ActualHeight <= 0) return false;

        var local = point - centre;
        var rotated = Rotate(local);

        double scale = Math.Min(ActualWidth, ActualHeight) * 0.45 * Zoom / radius;
        double camera = radius * 4;
        double depth = camera / Math.Max(0.0001, camera - rotated.Z);

        screen = new Point(
            ActualWidth / 2 + Pan.X + rotated.X * scale * depth,
            ActualHeight / 2 + Pan.Y - rotated.Y * scale * depth);

        return true;
    }

    private Vector3D Rotate(Vector3D vector)
    {
        double yaw = Yaw * Math.PI / 180;
        double pitch = Pitch * Math.PI / 180;

        double cosYaw = Math.Cos(yaw);
        double sinYaw = Math.Sin(yaw);

        double x = vector.X * cosYaw + vector.Y * sinYaw;
        double y = -vector.X * sinYaw + vector.Y * cosYaw;
        double z = vector.Z;

        double cosPitch = Math.Cos(pitch);
        double sinPitch = Math.Sin(pitch);

        return new Vector3D(x, y * cosPitch - z * sinPitch, y * sinPitch + z * cosPitch);
    }

    private (Point3D Centre, double Radius) Bounds()
    {
        if (Vertices.Count == 0) return (new Point3D(), 0);

        double minX = double.MaxValue, minY = double.MaxValue, minZ = double.MaxValue;
        double maxX = double.MinValue, maxY = double.MinValue, maxZ = double.MinValue;

        foreach (var v in Vertices)
        {
            minX = Math.Min(minX, v.X);
            minY = Math.Min(minY, v.Y);
            minZ = Math.Min(minZ, v.Z);
            maxX = Math.Max(maxX, v.X);
            maxY = Math.Max(maxY, v.Y);
            maxZ = Math.Max(maxZ, v.Z);
        }

        var centre = new Point3D((minX + maxX) / 2, (minY + maxY) / 2, (minZ + maxZ) / 2);
        double radius = Math.Max(0.0001, Math.Max(maxX - minX, Math.Max(maxY - minY, maxZ - minZ)) / 2);

        return (centre, radius);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        return new Size(
            double.IsInfinity(availableSize.Width) ? 260 : availableSize.Width,
            double.IsInfinity(availableSize.Height) ? 200 : availableSize.Height);
    }

    protected override void OnRender(DrawingContext dc)
    {
        if (Background is not null)
        {
            dc.DrawRectangle(Background, null, new Rect(RenderSize));
        }

        var edge = EdgeBrush ?? TryFindResource("CONCOST.Brush.TextPrimary") as Brush ?? Brushes.Black;

        if (Vertices.Count == 0)
        {
            DrawEmpty(dc);
            return;
        }

        var pen = new Pen(edge, 1.2);

        foreach (var segment in Edges)
        {
            if (segment.A < 0 || segment.A >= Vertices.Count) continue;
            if (segment.B < 0 || segment.B >= Vertices.Count) continue;

            if (!Project(Vertices[segment.A], out var a)) continue;
            if (!Project(Vertices[segment.B], out var b)) continue;

            dc.DrawLine(pen, a, b);
        }

        if (ShowAxes) DrawAxes(dc);
    }

    private void DrawEmpty(DrawingContext dc)
    {
        var muted = TryFindResource("CONCOST.Brush.TextSecondary") as Brush ?? Brushes.Gray;
        var text = new FormattedText(
            "No mesh",
            System.Globalization.CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            new Typeface("Segoe UI"),
            12,
            muted,
            VisualTreeHelper.GetDpi(this).PixelsPerDip);

        dc.DrawText(text, new Point((ActualWidth - text.Width) / 2, (ActualHeight - text.Height) / 2));
    }

    private void DrawAxes(DrawingContext dc)
    {
        var (centre, radius) = Bounds();

        if (!Project(centre, out var centreScreen)) return;

        // Short gizmo anchored at the model centre, sized to the model so it always reads.
        foreach (var (axis, brush) in new[]
                 {
                     (new Vector3D(radius, 0, 0), Brushes.IndianRed),
                     (new Vector3D(0, radius, 0), Brushes.SeaGreen),
                     (new Vector3D(0, 0, radius), Brushes.RoyalBlue)
                 })
        {
            var end = centre + axis;
            if (!Project(end, out var screen)) continue;

            dc.DrawLine(new Pen(brush, 2), centreScreen, screen);
        }
    }

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);

        Focus();
        _orbiting = true;
        _orbitOrigin = e.GetPosition(this);
        _yawStart = Yaw;
        _pitchStart = Pitch;
        CaptureMouse();
        Cursor = Cursors.SizeAll;
        e.Handled = true;
    }

    protected override void OnMouseRightButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseRightButtonDown(e);

        Focus();
        _panning = true;
        _orbitOrigin = e.GetPosition(this);
        _panStart = Pan;
        CaptureMouse();
        Cursor = Cursors.Hand;
        e.Handled = true;
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);

        var position = e.GetPosition(this);

        if (_orbiting && e.LeftButton == MouseButtonState.Pressed)
        {
            SetCurrentValue(YawProperty, (_yawStart + (position.X - _orbitOrigin.X) * 0.5) % 360);
            SetCurrentValue(PitchProperty, Math.Max(-89, Math.Min(89, _pitchStart - (position.Y - _orbitOrigin.Y) * 0.5)));
            return;
        }

        if (_panning && e.RightButton == MouseButtonState.Pressed)
        {
            Pan = new Vector(
                _panStart.X + position.X - _orbitOrigin.X,
                _panStart.Y + position.Y - _orbitOrigin.Y);
        }
    }

    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonUp(e);

        if (!_orbiting) return;

        _orbiting = false;
        ReleaseMouseCapture();
        Cursor = null;
    }

    protected override void OnMouseRightButtonUp(MouseButtonEventArgs e)
    {
        base.OnMouseRightButtonUp(e);

        if (!_panning) return;

        _panning = false;
        ReleaseMouseCapture();
        Cursor = null;
    }

    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
        base.OnMouseWheel(e);

        Zoom *= e.Delta > 0 ? 1.12 : 1 / 1.12;
        e.Handled = true;
    }
}
