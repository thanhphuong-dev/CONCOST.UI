using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace CONCOST.UI.Controls;

/// <summary>
/// The drawing half of the <see cref="ViewCube"/>: projection, the 26 zones, the compass, the buttons
/// and the hit test that has to agree with all of them.
/// </summary>
public partial class ViewCube
{
    /// <summary>
    /// Radius, in world units, of the circle the widget has to hold: the compass ring and its letters
    /// are the widest thing drawn, and a corner of the cube is <c>sqrt(3)/2</c> away from its centre.
    /// </summary>
    private const double WorstRadius = 1.15;

    /// <summary>Room left round the cube for the buttons, in pixels.</summary>
    private const double Pad = 10;

    /// <summary>
    /// Radius of the compass ring, in world units: wide enough that the letters stand clear of the
    /// cube's silhouette - a ring that hugged the base put N and W behind the cube - and tight enough
    /// that the cube keeps most of the widget.
    /// </summary>
    private const double RingRadius = 0.82;

    /// <summary>The ring lies on the ground, in the plane of the bottom face.</summary>
    private const double RingDepth = -ViewCubeMath.HalfExtent;

    /// <summary>Strength of the mild perspective: enough depth to read as a solid, not enough to breathe.</summary>
    private const double PerspectiveStrength = 0.1;

    private const int RingSegments = 64;
    private const double LetterGapDegrees = 16;
    private const double LetterReach = 0.12;

    /// <summary>Gap, in pixels, kept between a compass letter and the solid it names.</summary>
    private const double LetterMargin = 6;

    /// <summary>
    /// Elevation, in degrees, below which the dial is not drawn. Seen edge on the ring collapses onto
    /// a line through the base of the cube and all four letters land in the same place, so there is
    /// nothing to read there; above this angle the ellipse is tall enough to separate them.
    /// </summary>
    private const double MinCompassElevation = 10;

    private List<ProjectedZone>? _cache;
    private Quaternion _cacheRotation;
    private double _cacheSide = -1;

    /// <summary>The square the widget is laid out in: the smallest of the two actual sides.</summary>
    private double ControlSide
    {
        get
        {
            double width = ActualWidth > 0 ? ActualWidth : DesiredSize.Width;
            double height = ActualHeight > 0 ? ActualHeight : DesiredSize.Height;

            if (width <= 0 || height <= 0)
            {
                return ResolveSide(new System.Windows.Size(double.PositiveInfinity, double.PositiveInfinity));
            }

            return Math.Min(width, height);
        }
    }

    /// <inheritdoc />
    protected override System.Windows.Size MeasureOverride(System.Windows.Size availableSize)
    {
        double side = ResolveSide(availableSize);
        return new System.Windows.Size(side, side);
    }

    /// <summary>
    /// The widget's size: one of the four presets, or a fifth of the viewport's short side kept between
    /// 64 and 160 pixels so it neither disappears in a small panel nor fills a large one.
    /// </summary>
    private double ResolveSide(System.Windows.Size available)
    {
        switch (Size)
        {
            case ViewCubeSize.Tiny:
                return 64;

            case ViewCubeSize.Small:
                return 80;

            case ViewCubeSize.Normal:
                return 100;

            case ViewCubeSize.Large:
                return 128;
        }

        double shortSide = Math.Min(available.Width, available.Height);

        if (double.IsNaN(shortSide) || double.IsInfinity(shortSide) || shortSide <= 0) return 100;

        return Math.Max(64, Math.Min(160, shortSide * 0.2));
    }

    /// <summary>Pixels per world unit: the cube's edge, since the cube is a unit cube.</summary>
    private static double Unit(double side) => Math.Max(1, (side - 2 * Pad) / (2 * WorstRadius));

    private Point Centre(double side) => new(
        ActualWidth > 0 ? ActualWidth / 2 : side / 2,
        ActualHeight > 0 ? ActualHeight / 2 : side / 2);

    private void DropProjection() => _cache = null;

    // ------------------------------------------------------------------ projection

    private readonly struct Basis
    {
        public Basis(Quaternion rotation)
        {
            ViewCubeMath.ToBasis(rotation, out Vector3D right, out Vector3D up, out Vector3D forward);

            Right = right;
            Up = up;
            Forward = forward;
            Eye = -forward;
        }

        public Vector3D Right { get; }

        public Vector3D Up { get; }

        public Vector3D Forward { get; }

        public Vector3D Eye { get; }
    }

    private sealed class ProjectedZone
    {
        public ViewCubeRegion Region;
        public Point[] Points = Array.Empty<Point>();
        public Geometry Shape = Geometry.Empty;
        public Point Centre;
        public double Depth;
        public bool Visible;
    }

    private static Point Project(Vector3D point, in Basis basis, Point centre, double unit)
    {
        // Mild perspective rather than orthographic: at 20 degrees of field of view a cube shows its
        // near edge slightly wider and stops looking like a flat icon, without the breathing that a
        // camera fitted to the box would bring as it turns.
        double scale = 1 + Vector3D.DotProduct(point, basis.Eye) * PerspectiveStrength;

        return new Point(
            centre.X + Vector3D.DotProduct(point, basis.Right) * scale * unit,
            centre.Y - Vector3D.DotProduct(point, basis.Up) * scale * unit);
    }

    /// <summary>The 26 zones projected into control space, rebuilt only when the view or the size moved.</summary>
    private List<ProjectedZone> Zones()
    {
        double side = ControlSide;
        Quaternion rotation = CurrentRotation;

        if (_cache is not null && Math.Abs(_cacheSide - side) < 0.01 && _cacheRotation == rotation) return _cache;

        var basis = new Basis(rotation);
        double unit = Unit(side);
        Point centre = Centre(side);

        var zones = new List<ProjectedZone>(26);

        foreach (ViewCubeFacelet facelet in ViewCubeMesh.Default)
        {
            var points = new Point[facelet.Corners.Length];

            for (int i = 0; i < points.Length; i++)
            {
                points[i] = Project(facelet.Corners[i], basis, centre, unit);
            }

            zones.Add(new ProjectedZone
            {
                Region = facelet.Region,
                Points = points,
                Shape = Polygon(points),
                Centre = Project(facelet.Centre, basis, centre, unit),
                Depth = Vector3D.DotProduct(facelet.Centre, basis.Eye),
                Visible = Vector3D.DotProduct(facelet.Region.Normal, basis.Eye) > 0.02
            });
        }

        _cache = zones;
        _cacheRotation = rotation;
        _cacheSide = side;

        return zones;
    }

    // ------------------------------------------------------------------ render

    /// <inheritdoc />
    protected override void OnRender(DrawingContext dc)
    {
        double side = ControlSide;
        if (side < 8) return;

        var basis = new Basis(CurrentRotation);
        double unit = Unit(side);
        Point centre = Centre(side);
        Palette palette = ResolvePalette();

        List<ProjectedZone> zones = Zones();
        CompassGeometry? compass = CompassReadable(basis) ? BuildCompass(basis, centre, unit, zones) : null;

        if (compass is not null) DrawCompassRing(dc, compass, palette);

        var ordered = new List<ProjectedZone>(zones);

        // Back to front. The hidden zones are covered by the ones facing the eye, so they are filled
        // but not stroked: stroking them lays a second line on the silhouette and the edge reads heavy.
        ordered.Sort((left, right) => left.Depth.CompareTo(right.Depth));

        foreach (ProjectedZone zone in ordered)
        {
            bool hovered = _hover.IsRegion && _hover.Region == zone.Region;

            Brush fill = hovered ? palette.Highlight : palette.Face;
            Pen? pen = zone.Visible ? new Pen(hovered ? palette.Highlight : palette.Edge, hovered ? 1.4 : 1) : null;

            dc.DrawGeometry(fill, pen, zone.Shape);

            if (!zone.Visible || !zone.Region.IsFace) continue;

            DrawCaption(dc, zone, basis, unit, hovered ? palette.OnHighlight : palette.Text);
        }

        if (compass is not null) DrawCompassLetters(dc, compass, palette);

        if (!_pointerOver && !AlwaysShowTools && _drag == DragMode.None) return;

        DrawButtons(dc, zones, palette);
    }

    /// <summary>
    /// A caption drawn on its face: turned with the face and squeezed by the same foreshortening, so
    /// RIGHT on a side face reads at the angle that face is seen at instead of floating upright over it.
    /// </summary>
    private void DrawCaption(DrawingContext dc, ProjectedZone zone, in Basis basis, double unit, Brush brush)
    {
        string label = zone.Region.Label;
        if (string.Equals(label, "TOP", StringComparison.OrdinalIgnoreCase))
            label = TopLabel ?? label;
        else if (string.Equals(label, "BOTTOM", StringComparison.OrdinalIgnoreCase))
            label = BottomLabel ?? label;
        else if (string.Equals(label, "FRONT", StringComparison.OrdinalIgnoreCase))
            label = FrontLabel ?? label;
        else if (string.Equals(label, "BACK", StringComparison.OrdinalIgnoreCase))
            label = BackLabel ?? label;
        else if (string.Equals(label, "LEFT", StringComparison.OrdinalIgnoreCase))
            label = LeftLabel ?? label;
        else if (string.Equals(label, "RIGHT", StringComparison.OrdinalIgnoreCase))
            label = RightLabel ?? label;

        if (string.IsNullOrEmpty(label)) return;

        Vector3D across = zone.Region.Right;
        Vector3D along = zone.Region.Up;

        var x = new Vector(Vector3D.DotProduct(across, basis.Right), -Vector3D.DotProduct(across, basis.Up));
        var y = new Vector(-Vector3D.DotProduct(along, basis.Right), Vector3D.DotProduct(along, basis.Up));

        // A face seen edge on has no room for a caption, and drawing one there is what leaves stray
        // letters outside the cube.
        if (x.Length < 0.2 || y.Length < 0.2) return;

        double size = Math.Max(6, unit * ViewCubeMath.FaceRatio * 0.28);
        FormattedText text = Format(label, size, brush);

        double room = unit * ViewCubeMath.FaceRatio * 0.86;

        if (text.Width > 0 && text.Width * x.Length > room)
        {
            size = Math.Max(5, size * room / (text.Width * x.Length));
            text = Format(label, size, brush);
        }

        dc.PushTransform(new MatrixTransform(x.X, x.Y, y.X, y.Y, zone.Centre.X, zone.Centre.Y));
        dc.DrawText(text, new Point(-text.Width / 2, -text.Height / 2));
        dc.Pop();
    }

    // ------------------------------------------------------------------ compass

    /// <summary>
    /// True when the dial can be read at this orientation: shown when asked for, and only when it is
    /// not edge on. Both the drawing and the hit test ask this, so the two never disagree.
    /// </summary>
    private bool CompassReadable(in Basis basis) =>
        ShowCompass && Math.Abs(ViewCubeMath.Elevation(basis.Eye)) >= MinCompassElevation;

    private sealed class CompassGeometry
    {
        public Geometry Ring = Geometry.Empty;
        public Point[] Samples = Array.Empty<Point>();
        public string[] Labels = Array.Empty<string>();
        public Vector3D[] Directions = Array.Empty<Vector3D>();
        public Point[] Anchors = Array.Empty<Point>();
    }

    private CompassGeometry BuildCompass(in Basis basis, Point centre, double unit, List<ProjectedZone> zones)
    {
        var samples = new Point[RingSegments];
        var sampleAngles = new double[RingSegments];

        for (int i = 0; i < RingSegments; i++)
        {
            double angle = i * 360.0 / RingSegments;

            sampleAngles[i] = angle;
            samples[i] = Project(RingPoint(angle, RingRadius), basis, centre, unit);
        }

        var labels = new[] { "N", "E", "S", "W" };
        var directions = new[]
        {
            ViewCubeMath.Rotate(new Vector3D(0, 1, 0), ViewCubeMath.WorldUp, FrontAngle),
            ViewCubeMath.Rotate(new Vector3D(1, 0, 0), ViewCubeMath.WorldUp, FrontAngle),
            ViewCubeMath.Rotate(new Vector3D(0, -1, 0), ViewCubeMath.WorldUp, FrontAngle),
            ViewCubeMath.Rotate(new Vector3D(-1, 0, 0), ViewCubeMath.WorldUp, FrontAngle)
        };

        var anchors = new Point[labels.Length];

        for (int i = 0; i < labels.Length; i++)
        {
            double angle = LetterAngle(i);

            anchors[i] = ClearOfCube(
                Project(RingPoint(angle, RingRadius + LetterReach), basis, centre, unit),
                centre,
                zones);
        }

        var ring = new StreamGeometry();

        using (StreamGeometryContext context = ring.Open())
        {
            for (int i = 0; i < RingSegments; i++)
            {
                int next = (i + 1) % RingSegments;

                if (NearLetter((sampleAngles[i] + sampleAngles[next]) / 2)) continue;

                context.BeginFigure(samples[i], false, false);
                context.LineTo(samples[next], true, false);
            }
        }

        ring.Freeze();

        return new CompassGeometry
        {
            Ring = ring,
            Samples = samples,
            Labels = labels,
            Directions = directions,
            Anchors = anchors
        };
    }

    /// <summary>World position of a point on the compass ring, turned with the model's front.</summary>
    private Vector3D RingPoint(double angle, double radius)
    {
        double radians = (angle + FrontAngle) * Math.PI / 180;

        return new Vector3D(
            Math.Cos(radians) * radius,
            Math.Sin(radians) * radius,
            RingDepth);
    }

    /// <summary>Compass angle of a letter: N is north, E is east, and the front angle turns all four.</summary>
    private double LetterAngle(int index) => index switch
    {
        0 => 90,
        1 => 0,
        2 => 270,
        _ => 180
    };

    private bool NearLetter(double angle)
    {
        for (int i = 0; i < 4; i++)
        {
            double delta = Math.Abs(((angle - LetterAngle(i)) % 360 + 540) % 360 - 180);

            if (delta < LetterGapDegrees) return true;
        }

        return false;
    }

    private void DrawCompassRing(DrawingContext dc, CompassGeometry compass, in Palette palette)
    {
        // A wide transparent stroke under the thin one: the ring is a control, and a one pixel line is
        // not something a pointer can be expected to land on.
        Brush ringStroke = CompassBrush ?? palette.Border;
        dc.DrawGeometry(null, new Pen(Brushes.Transparent, 14), compass.Ring);
        dc.DrawGeometry(null, new Pen(ringStroke, 1), compass.Ring);
    }

    /// <summary>
    /// The four letters, drawn after the cube rather than under it. Under it, the letter at the far
    /// side was cut in half by the silhouette, which reads as a fault rather than as depth; nudged
    /// clear of the silhouette first, all four stay whole and still read as a ring of labels.
    /// </summary>
    private void DrawCompassLetters(DrawingContext dc, CompassGeometry compass, in Palette palette)
    {
        for (int i = 0; i < compass.Labels.Length; i++)
        {
            Point anchor = compass.Anchors[i];
            bool hovered = _hover.Part == ViewCubePart.Compass && _hover.Label == compass.Labels[i];

            dc.DrawRectangle(Brushes.Transparent, null, new Rect(anchor.X - 7, anchor.Y - 7, 14, 14));
            Brush letterBrush = CompassTextBrush ?? palette.Text;
            DrawText(dc, compass.Labels[i], anchor, 8.5, hovered ? palette.Highlight : letterBrush);
        }
    }

    /// <summary>
    /// A letter moved out of the cube's silhouette, along the radius of the dial it belongs to, and
    /// then stood clear of the solid: the far half of a dial seen from above lands on top of the cube,
    /// and a label hard against the silhouette reads as part of the caption next to it.
    /// </summary>
    private static Point ClearOfCube(Point anchor, Point centre, List<ProjectedZone> zones)
    {
        Vector outward = anchor - centre;

        if (outward.Length < 1e-6) return anchor;

        outward.Normalize();

        for (int attempt = 0; attempt < 24 && InsideSilhouette(anchor, zones); attempt++)
        {
            anchor += outward * 1.5;
        }

        return anchor + outward * LetterMargin;
    }

    private static bool InsideSilhouette(Point point, List<ProjectedZone> zones)
    {
        foreach (ProjectedZone zone in zones)
        {
            if (zone.Visible && zone.Shape.FillContains(point)) return true;
        }

        return false;
    }

    // ------------------------------------------------------------------ buttons

    private void DrawButtons(DrawingContext dc, List<ProjectedZone> zones, in Palette palette)
    {
        double side = ControlSide;

        DrawHomeButton(dc, GetHomeRect(side), palette);
        DrawMenuButton(dc, GetMenuRect(side), palette);

        // The quarter turns and the rolls only make sense square on a face: anywhere else they would
        // have to guess which way is "up", and Revit hides them for the same reason.
        if (!ViewCubeMath.TryFace(CurrentRotation, 0.5, out _)) return;

        (Rect clockwise, Rect counter) = RollRects(side);

        DrawRollButton(dc, clockwise, true, palette);
        DrawRollButton(dc, counter, false, palette);

        foreach ((ArrowDirection direction, Rect rect) in ArrowRects(zones, side))
        {
            bool hovered = _hover.Part == ViewCubePart.Arrow && _hover.Arrow == direction;

            dc.DrawRectangle(Brushes.Transparent, null, rect);
            Brush glyphBrush = ArrowBrush ?? palette.Text;
            DrawArrowGlyph(dc, rect, direction, hovered ? palette.Highlight : glyphBrush);
        }
    }

    private Rect GetHomeRect(double side) => new(2, 2, ButtonSize, ButtonSize);

    private Rect GetMenuRect(double side) => new(side - ButtonSize - 2, side - ButtonSize - 2, ButtonSize, ButtonSize);

    private static (Rect Clockwise, Rect Counter) RollRects(double side)
    {
        const double gap = 3;

        double right = side - gap - RollButtonSize;

        return (
            new Rect(right, gap, RollButtonSize, RollButtonSize),
            new Rect(right - RollButtonSize - gap, gap, RollButtonSize, RollButtonSize));
    }

    /// <summary>Boxes for the four arrows, just outside the cube's silhouette.</summary>
    private IEnumerable<(ArrowDirection Direction, Rect Rect)> ArrowRects(List<ProjectedZone> zones, double side)
    {
        Rect bounds = Silhouette(zones, side);
        double middleX = bounds.X + bounds.Width / 2;
        double middleY = bounds.Y + bounds.Height / 2;

        const double width = 11;
        const double height = 9;
        const double gap = ArrowGap;

        var up = new Rect(middleX - width / 2, bounds.Top - gap - height, width, height);
        var down = new Rect(middleX - width / 2, bounds.Bottom + gap, width, height);
        var left = new Rect(bounds.Left - gap - height, middleY - width / 2, height, width);
        var right = new Rect(bounds.Right + gap, middleY - width / 2, height, width);

        yield return (ArrowDirection.Up, Clamp(up, side));
        yield return (ArrowDirection.Down, Clamp(down, side));
        yield return (ArrowDirection.Left, Clamp(left, side));
        yield return (ArrowDirection.Right, Clamp(right, side));
    }

    private static Rect Clamp(Rect rect, double side)
    {
        double width = Math.Min(rect.Width, side);
        double height = Math.Min(rect.Height, side);

        double x = Math.Max(0, Math.Min(side - width, rect.X));
        double y = Math.Max(0, Math.Min(side - height, rect.Y));

        return new Rect(x, y, width, height);
    }

    /// <summary>The box the visible zones cover, which is where the arrows are hung.</summary>
    private static Rect Silhouette(List<ProjectedZone> zones, double side)
    {
        double minX = double.MaxValue, minY = double.MaxValue;
        double maxX = double.MinValue, maxY = double.MinValue;

        foreach (ProjectedZone zone in zones)
        {
            if (!zone.Visible) continue;

            foreach (Point point in zone.Points)
            {
                minX = Math.Min(minX, point.X);
                minY = Math.Min(minY, point.Y);
                maxX = Math.Max(maxX, point.X);
                maxY = Math.Max(maxY, point.Y);
            }
        }

        return minX > maxX ? new Rect(0, 0, side, side) : new Rect(minX, minY, maxX - minX, maxY - minY);
    }

    /// <summary>
    /// A chevron pointing out of the cube, drawn rather than filled: a solid triangle this small read
    /// as a fleck of debris next to the cube instead of as a button.
    /// </summary>
    private void DrawArrowGlyph(DrawingContext dc, Rect rect, ArrowDirection direction, Brush brush)
    {
        Point tip, left, right;

        switch (direction)
        {
            case ArrowDirection.Up:
                tip = new Point(rect.X + rect.Width / 2, rect.Y);
                left = new Point(rect.X, rect.Bottom);
                right = new Point(rect.Right, rect.Bottom);
                break;

            case ArrowDirection.Down:
                tip = new Point(rect.X + rect.Width / 2, rect.Bottom);
                left = new Point(rect.X, rect.Y);
                right = new Point(rect.Right, rect.Y);
                break;

            case ArrowDirection.Left:
                tip = new Point(rect.X, rect.Y + rect.Height / 2);
                left = new Point(rect.Right, rect.Y);
                right = new Point(rect.Right, rect.Bottom);
                break;

            default:
                tip = new Point(rect.Right, rect.Y + rect.Height / 2);
                left = new Point(rect.X, rect.Y);
                right = new Point(rect.X, rect.Bottom);
                break;
        }

        var chevron = new StreamGeometry();

        using (StreamGeometryContext context = chevron.Open())
        {
            context.BeginFigure(left, false, false);
            context.LineTo(tip, true, false);
            context.LineTo(right, true, false);
        }

        chevron.Freeze();

        dc.DrawGeometry(null, new Pen(brush, 1.6)
        {
            StartLineCap = PenLineCap.Round,
            EndLineCap = PenLineCap.Round,
            LineJoin = PenLineJoin.Round
        }, chevron);
    }

    private void DrawHomeButton(DrawingContext dc, Rect rect, in Palette palette)
    {
        bool hovered = _hover.Part == ViewCubePart.Home;
        Brush brush = hovered ? palette.Highlight : palette.Text;

        dc.DrawRectangle(Brushes.Transparent, null, rect);
        dc.DrawRoundedRectangle(null, new Pen(hovered ? palette.Highlight : palette.Border, 1), rect, 3, 3);

        // A house, drawn as a path rather than a glyph so it needs no icon font.
        double left = rect.X + 3;
        double right = rect.Right - 3;
        double bottom = rect.Bottom - 3;
        double middle = rect.Y + rect.Height / 2;

        var roof = new StreamGeometry();

        using (StreamGeometryContext context = roof.Open())
        {
            context.BeginFigure(new Point(left - 0.5, middle), false, false);
            context.LineTo(new Point(rect.X + rect.Width / 2, rect.Y + 2.5), true, false);
            context.LineTo(new Point(right + 0.5, middle), true, false);
            context.LineTo(new Point(right, bottom), true, false);
            context.LineTo(new Point(left, bottom), true, false);
        }

        roof.Freeze();

        dc.DrawGeometry(null, new Pen(brush, 1), roof);
    }

    private void DrawMenuButton(DrawingContext dc, Rect rect, in Palette palette)
    {
        bool hovered = _hover.Part == ViewCubePart.Menu;
        Brush brush = hovered ? palette.Highlight : palette.Text;

        dc.DrawRectangle(Brushes.Transparent, null, rect);
        dc.DrawRoundedRectangle(null, new Pen(hovered ? palette.Highlight : palette.Border, 1), rect, 3, 3);

        double middleX = rect.X + rect.Width / 2;
        double top = rect.Y + 6;

        var chevron = new StreamGeometry();

        using (StreamGeometryContext context = chevron.Open())
        {
            context.BeginFigure(new Point(middleX - 3, top), false, false);
            context.LineTo(new Point(middleX, top + 3), true, false);
            context.LineTo(new Point(middleX + 3, top), true, false);
        }

        chevron.Freeze();

        dc.DrawGeometry(null, new Pen(brush, 1.2)
        {
            StartLineCap = PenLineCap.Round,
            EndLineCap = PenLineCap.Round
        }, chevron);
    }

    private void DrawRollButton(DrawingContext dc, Rect rect, bool clockwise, in Palette palette)
    {
        bool hovered = _hover.Part == ViewCubePart.Roll && _hover.Roll == (clockwise ? 1 : -1);
        Brush box = hovered ? palette.Highlight : palette.Border;
        Brush glyph = hovered ? palette.Highlight : palette.Text;

        dc.DrawRectangle(Brushes.Transparent, null, rect);
        dc.DrawRoundedRectangle(null, new Pen(box, 1), rect, 3, 3);

        double radius = 3.6;
        double middleX = rect.X + rect.Width / 2;
        double middleY = rect.Y + rect.Height / 2;

        // Clean 270-degree arc with a crisp arrow head
        var arc = new StreamGeometry();
        using (StreamGeometryContext context = arc.Open())
        {
            context.BeginFigure(new Point(middleX + radius, middleY), false, false);
            context.ArcTo(new Point(middleX, middleY + radius), new Size(radius, radius), 0, true, SweepDirection.Counterclockwise, true, false);
        }
        arc.Freeze();

        dc.PushTransform(new TranslateTransform(middleX, middleY));
        if (clockwise) dc.PushTransform(new ScaleTransform(-1, 1));
        dc.PushTransform(new TranslateTransform(-middleX, -middleY));

        dc.DrawGeometry(null, new Pen(glyph, 1.2)
        {
            StartLineCap = PenLineCap.Round,
            EndLineCap = PenLineCap.Round
        }, arc);

        var head = Polygon(new[]
        {
            new Point(middleX + 2.4, middleY + radius),
            new Point(middleX - 0.6, middleY + radius - 2.2),
            new Point(middleX - 0.6, middleY + radius + 2.2)
        });

        dc.DrawGeometry(glyph, null, head);

        dc.Pop();
        if (clockwise) dc.Pop();
        dc.Pop();
    }

    // ------------------------------------------------------------------ hit test

    /// <summary>Resolves a point on the control to the widget under it.</summary>
    public ViewCubeHit HitTest(Point point)
    {
        double side = ControlSide;
        if (side < 8) return ViewCubeHit.Empty;

        double width = ActualWidth > 0 ? ActualWidth : side;
        double height = ActualHeight > 0 ? ActualHeight : side;

        if (point.X < 0 || point.Y < 0 || point.X > width || point.Y > height) return ViewCubeHit.Empty;

        if (_pointerOver || AlwaysShowTools || _drag != DragMode.None)
        {
            if (GetHomeRect(side).Contains(point)) return ViewCubeHit.ForHome();
            if (GetMenuRect(side).Contains(point)) return ViewCubeHit.ForMenu();

            if (ViewCubeMath.TryFace(CurrentRotation, 0.5, out _))
            {
                (Rect clockwise, Rect counter) = RollRects(side);

                if (clockwise.Contains(point)) return ViewCubeHit.ForRoll(1);
                if (counter.Contains(point)) return ViewCubeHit.ForRoll(-1);

                foreach ((ArrowDirection direction, Rect rect) in ArrowRects(Zones(), side))
                {
                    if (rect.Contains(point)) return ViewCubeHit.ForArrow(direction);
                }
            }
        }

        List<ProjectedZone> zones = Zones();
        var ordered = new List<ProjectedZone>(zones);

        ordered.Sort((left, right) => right.Depth.CompareTo(left.Depth));

        foreach (ProjectedZone zone in ordered)
        {
            if (!zone.Visible) continue;
            if (!zone.Shape.FillContains(point)) continue;

            return ViewCubeHit.ForRegion(zone.Region);
        }

        if (!CompassReadable(new Basis(CurrentRotation))) return ViewCubeHit.Empty;

        var basis = new Basis(CurrentRotation);
        CompassGeometry compass = BuildCompass(basis, Centre(side), Unit(side), zones);

        for (int i = 0; i < compass.Labels.Length; i++)
        {
            if (Distance(point, compass.Anchors[i]) > 9) continue;

            return ViewCubeHit.ForCompass(compass.Labels[i], compass.Directions[i]);
        }

        if (DistanceToRing(point, compass.Samples) > 7) return ViewCubeHit.Empty;

        // The ring itself is a dial: a press on it turns the model, but a click still reports the
        // letter the pointer is closest to so a host can tell what was aimed at.
        int nearest = 0;

        for (int i = 1; i < compass.Anchors.Length; i++)
        {
            if (Distance(point, compass.Anchors[i]) >= Distance(point, compass.Anchors[nearest])) continue;

            nearest = i;
        }

        return ViewCubeHit.ForCompass(compass.Labels[nearest], compass.Directions[nearest]);
    }

    private static double DistanceToRing(Point point, Point[] samples)
    {
        double best = double.MaxValue;

        for (int i = 0; i < samples.Length; i++)
        {
            Point start = samples[i];
            Point end = samples[(i + 1) % samples.Length];

            best = Math.Min(best, DistanceToSegment(point, start, end));
        }

        return best;
    }

    private static double DistanceToSegment(Point point, Point start, Point end)
    {
        double dx = end.X - start.X;
        double dy = end.Y - start.Y;
        double lengthSquared = dx * dx + dy * dy;

        if (lengthSquared < 1e-9) return Distance(point, start);

        double t = ((point.X - start.X) * dx + (point.Y - start.Y) * dy) / lengthSquared;

        t = Math.Max(0, Math.Min(1, t));

        return Distance(point, new Point(start.X + t * dx, start.Y + t * dy));
    }

    private static double Distance(Point a, Point b) =>
        Math.Sqrt((a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y));

    // ------------------------------------------------------------------ helpers

    private readonly struct Palette
    {
        public Palette(Brush face, Brush edge, Brush border, Brush text, Brush highlight, Brush onHighlight)
        {
            Face = face;
            Edge = edge;
            Border = border;
            Text = text;
            Highlight = highlight;
            OnHighlight = onHighlight;
        }

        public Brush Face { get; }

        public Brush Edge { get; }

        public Brush Border { get; }

        public Brush Text { get; }

        public Brush Highlight { get; }

        public Brush OnHighlight { get; }
    }

    private Palette ResolvePalette()
    {
        ViewCubeTheme? theme = Theme;

        Brush face = FaceBrush ?? theme?.Face ?? FindBrush("CONCOST.Brush.ViewCubeFace", Brushes.Gainsboro);
        Brush edge = EdgeBrush ?? theme?.Edge ?? FindBrush("CONCOST.Brush.ViewCubeEdge", Brushes.Silver);
        Brush border = BorderBrush ?? theme?.Border ?? FindBrush("CONCOST.Brush.ViewCubeBorder", Brushes.Gray);
        Brush text = TextBrush ?? theme?.Text ?? FindBrush("CONCOST.Brush.ViewCubeText", Brushes.DimGray);
        Brush highlight = HighlightBrush ?? theme?.Highlight ?? FindBrush("CONCOST.Brush.ViewCubeHighlight", Brushes.DodgerBlue);
        Brush onHighlight = (HighlightBrush ?? theme?.Highlight) is null
            ? FindBrush("CONCOST.Brush.TextInverse", Brushes.White)
            : Brushes.White;

        return new Palette(face, edge, border, text, highlight, onHighlight);
    }

    private Brush FindBrush(string key, Brush fallback) => TryFindResource(key) as Brush ?? fallback;

    private static Geometry Polygon(Point[] points)
    {
        var geometry = new StreamGeometry();

        using (StreamGeometryContext context = geometry.Open())
        {
            context.BeginFigure(points[0], true, true);
            context.PolyLineTo(points.Skip(1).ToArray(), true, true);
        }

        geometry.Freeze();
        return geometry;
    }

    private FormattedText Format(string caption, double size, Brush brush) =>
        new(caption, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Segoe UI"), size, brush,
            VisualTreeHelper.GetDpi(this).PixelsPerDip);

    private void DrawText(DrawingContext dc, string caption, Point origin, double size, Brush brush)
    {
        FormattedText formatted = Format(caption, size, brush);

        dc.DrawText(formatted, new Point(origin.X - formatted.Width / 2, origin.Y - formatted.Height / 2));
    }
}
