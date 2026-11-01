using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using CONCOST.UI.Controls;
using CONCOST.UI.RevitLike.Model;

namespace CONCOST.UI.RevitLike.Shell;

/// <summary>
/// Paints the model into a <see cref="ViewportControl"/>'s overlay canvas.
/// </summary>
/// <remarks>
/// The viewport owns the camera and the grid; everything above the grid — rooms, wall lines, tags and
/// the selection highlight — is host geometry, so it goes into <c>Overlay</c>. The overlay is not
/// hit-testable (the viewport handles the mouse itself), which is why the renderer only draws and
/// never listens.
/// </remarks>
public sealed class PlanRenderer
{
    private readonly ViewportControl _viewport;
    private readonly Canvas _overlay;
    private readonly BuildingModel _model;

    public PlanRenderer(ViewportControl viewport, BuildingModel model, LevelModel? level, bool isThreeD)
    {
        _viewport = viewport;
        _model = model;
        Level = level;
        IsThreeD = isThreeD;

        _overlay = viewport.Overlay;
        _overlay.ClipToBounds = true;
    }

    /// <summary>Level to draw, or <c>null</c> for the whole building.</summary>
    public LevelModel? Level { get; }

    /// <summary>Draws the walls as extruded outlines instead of flat plan lines.</summary>
    public bool IsThreeD { get; }

    /// <summary>Element highlighted on top of everything else.</summary>
    public WallElement? SelectedWall { get; set; }

    /// <summary>Drawing style asked for by the View tab or the view control bar.</summary>
    public VisualStyleMode Style { get; set; } = VisualStyleMode.HiddenLine;

    /// <summary>Draws the room number and name.</summary>
    public bool ShowRoomTags { get; set; } = true;

    /// <summary>Draws the overall dimension strings around the plan.</summary>
    public bool ShowDimensions { get; set; } = true;

    /// <summary>Wall being drawn, shown while the second point is still pending.</summary>
    public (Point3D Start, Point3D End)? Preview { get; set; }

    /// <summary>Rebuilds every overlay child. Cheap enough for a demo: a few dozen lines.</summary>
    public void Redraw()
    {
        _overlay.Children.Clear();

        if (_viewport.ActualWidth <= 0 || _viewport.ActualHeight <= 0) return;

        foreach (var level in Levels())
        {
            DrawRooms(level);
            DrawWalls(level);
            DrawDimensions(level);
        }

        DrawPreview();
    }

    private IEnumerable<LevelModel> Levels()
    {
        if (Level is not null)
        {
            yield return Level;
            yield break;
        }

        foreach (var level in _model.Levels) yield return level;
    }

    private void DrawRooms(LevelModel level)
    {
        double z = level.Elevation;

        foreach (var room in level.Rooms)
        {
            var corners = new[]
            {
                new Point3D(room.X, room.Y, z),
                new Point3D(room.X + room.Width, room.Y, z),
                new Point3D(room.X + room.Width, room.Y + room.Depth, z),
                new Point3D(room.X, room.Y + room.Depth, z)
            };

            var polygon = new Polygon
            {
                Points = new PointCollection(Project(corners)),
                StrokeThickness = 1,
                Opacity = 0.85
            };

            // Only the shaded style fills the rooms; the two line styles leave the plan open so the
            // grid and the walls stay readable.
            if (Style == VisualStyleMode.Shaded)
            {
                polygon.SetResourceReference(Shape.FillProperty, "CONCOST.Brush.SurfaceSecondary");
            }

            polygon.SetResourceReference(Shape.StrokeProperty, "CONCOST.Brush.Border");

            _overlay.Children.Add(polygon);

            if (!ShowRoomTags) continue;

            // Room tag: number over name, centred, only when the room is big enough on screen to
            // read - otherwise the plan turns into a pile of overlapping text when zoomed out.
            var centre = Project(new[] { new Point3D(room.X + (room.Width / 2), room.Y + (room.Depth / 2), z) })[0];
            var size = Project(new[]
            {
                new Point3D(room.X, room.Y, z),
                new Point3D(room.X + room.Width, room.Y + room.Depth, z)
            });

            double onScreenWidth = Math.Abs(size[1].X - size[0].X);

            if (onScreenWidth < 90) continue;

            var label = new TextBlock
            {
                Text = $"{room.Number}\n{room.Name}",
                FontSize = 11,
                TextAlignment = TextAlignment.Center,
                IsHitTestVisible = false
            };

            label.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");

            _overlay.Children.Add(label);

            label.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

            Canvas.SetLeft(label, centre.X - (label.DesiredSize.Width / 2));
            Canvas.SetTop(label, centre.Y - (label.DesiredSize.Height / 2));
        }
    }

    private void DrawWalls(LevelModel level)
    {
        foreach (var wall in level.Walls)
        {
            double baseZ = level.Elevation + wall.BaseOffset;
            double topZ = baseZ + wall.Height;

            var start = new Point3D(wall.Start.X, wall.Start.Y, baseZ);
            var end = new Point3D(wall.End.X, wall.End.Y, baseZ);

            double thickness = Style == VisualStyleMode.Wireframe
                ? 1.2
                : Math.Max(2.5, _viewport.UnitsToPixels(wall.Thickness));

            if (ReferenceEquals(wall, SelectedWall))
            {
                AddLine(start, end, thickness + 6, "CONCOST.Brush.Primary", opacity: 0.35);
            }

            // The wall body: a fat line in plan, two rails plus corner posts when the camera is off
            // the vertical - a flat line would read as a 2D drawing rather than a model.
            if (IsThreeD)
            {
                var topStart = new Point3D(start.X, start.Y, topZ);
                var topEnd = new Point3D(end.X, end.Y, topZ);

                AddLine(start, end, 1.4, "CONCOST.Brush.Border");
                AddLine(topStart, topEnd, 1.4, "CONCOST.Brush.TextPrimary");

                foreach (var corner in new[] { start, end })
                {
                    AddLine(corner, new Point3D(corner.X, corner.Y, topZ), 1.4, "CONCOST.Brush.TextPrimary");
                }

                continue;
            }

            AddLine(start, end, thickness, "CONCOST.Brush.TextPrimary");
        }
    }

    /// <summary>
    /// Two dimension strings — one above the plan, one to its left — taken from the wall grid.
    /// </summary>
    private void DrawDimensions(LevelModel level)
    {
        if (IsThreeD || !ShowDimensions || level.Walls.Count == 0) return;

        double z = level.Elevation;

        List<double> xs = level.Walls
            .SelectMany(wall => new[] { wall.Start.X, wall.End.X })
            .Distinct()
            .OrderBy(value => value)
            .ToList();

        List<double> ys = level.Walls
            .SelectMany(wall => new[] { wall.Start.Y, wall.End.Y })
            .Distinct()
            .OrderBy(value => value)
            .ToList();

        if (xs.Count < 2 || ys.Count < 2) return;

        // 1100 mm clear of the outermost wall: outside the plan, still inside the margin Zoom Extents
        // leaves around the drawing.
        double top = ys[^1] + 1100;
        double left = xs[0] - 1100;

        AddLine(new Point3D(xs[0], top, z), new Point3D(xs[^1], top, z), 1, "CONCOST.Brush.TextSecondary");
        AddLine(new Point3D(left, ys[0], z), new Point3D(left, ys[^1], z), 1, "CONCOST.Brush.TextSecondary");

        // Two wall lines a few centimetres apart give two ticks a few pixels apart, which reads as a
        // smudge in the middle of the string. The nearer one wins.
        double lastTickX = double.NegativeInfinity;

        for (int i = 0; i < xs.Count; i++)
        {
            var anchor = new Point3D(xs[i], top, z);
            double screenX = _viewport.ToScreen(anchor).X;

            if (Math.Abs(screenX - lastTickX) >= 10)
            {
                AddTick(anchor, rotated: false);
                lastTickX = screenX;
            }

            if (i == xs.Count - 1) continue;

            double span = xs[i + 1] - xs[i];

            // A dimension whose text would not fit between its ticks only adds clutter.
            if (_viewport.UnitsToPixels(span) < 34) continue;

            AddLabel(new Point3D((xs[i] + xs[i + 1]) / 2, top, z), $"{span:0}", rotate: false);
        }

        double lastTickY = double.NegativeInfinity;

        for (int i = 0; i < ys.Count; i++)
        {
            var anchor = new Point3D(left, ys[i], z);
            double screenY = _viewport.ToScreen(anchor).Y;

            if (Math.Abs(screenY - lastTickY) >= 10)
            {
                AddTick(anchor, rotated: true);
                lastTickY = screenY;
            }

            if (i == ys.Count - 1) continue;

            double span = ys[i + 1] - ys[i];

            if (_viewport.UnitsToPixels(span) < 34) continue;

            AddLabel(new Point3D(left, (ys[i] + ys[i + 1]) / 2, z), $"{span:0}", rotate: true);
        }
    }

    /// <summary>The 45-degree slash a dimension string is marked with.</summary>
    private void AddTick(Point3D point, bool rotated)
    {
        // A tick is five pixels long whatever the zoom: in millimetres it would vanish as soon as the
        // plan is zoomed out.
        double scale = _viewport.UnitsToPixels(1);
        double arm = scale > 1e-9 ? 5 / scale : 0;

        var from = rotated
            ? new Point3D(point.X - arm, point.Y - arm, point.Z)
            : new Point3D(point.X - arm, point.Y + arm, point.Z);

        var to = rotated
            ? new Point3D(point.X + arm, point.Y + arm, point.Z)
            : new Point3D(point.X + arm, point.Y - arm, point.Z);

        AddLine(from, to, 1, "CONCOST.Brush.TextSecondary");
    }

    /// <summary>Places a caption centred on a world point; vertical strings read bottom to top.</summary>
    private void AddLabel(Point3D anchor, string caption, bool rotate)
    {
        var label = new TextBlock
        {
            Text = caption,
            FontSize = 10,
            IsHitTestVisible = false
        };

        label.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");

        label.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

        Point point = Project(new[] { anchor })[0];

        double x = point.X - (label.DesiredSize.Width / 2);
        double y = point.Y - label.DesiredSize.Height - 4;

        if (rotate)
        {
            label.RenderTransform = new RotateTransform(-90);
            label.RenderTransformOrigin = new Point(0.5, 0.5);

            x = point.X - label.DesiredSize.Width / 2 - 6;
            y = point.Y - label.DesiredSize.Height / 2;
        }

        Canvas.SetLeft(label, x);
        Canvas.SetTop(label, y);

        _overlay.Children.Add(label);
    }

    private void DrawPreview()
    {
        if (Preview is not { } preview) return;

        AddLine(preview.Start, preview.End, 2, "CONCOST.Brush.Primary", dash: true);
    }

    private void AddLine(Point3D from, Point3D to, double thickness, string brushKey, double opacity = 1, bool dash = false)
    {
        var a = Project(new[] { from })[0];
        var b = Project(new[] { to })[0];

        var line = new Line
        {
            X1 = a.X,
            Y1 = a.Y,
            X2 = b.X,
            Y2 = b.Y,
            StrokeThickness = thickness,
            Opacity = opacity,
            StrokeStartLineCap = PenLineCap.Flat,
            StrokeEndLineCap = PenLineCap.Flat,
            IsHitTestVisible = false
        };

        if (dash) line.StrokeDashArray = new DoubleCollection { 4, 3 };

        line.SetResourceReference(Shape.StrokeProperty, brushKey);

        _overlay.Children.Add(line);
    }

    private Point[] Project(IReadOnlyList<Point3D> points)
    {
        var result = new Point[points.Count];

        for (int i = 0; i < points.Count; i++)
        {
            var projected = _viewport.ToScreen(points[i]);

            // A degenerate camera (looking straight along the up axis) produces NaN here; clamping
            // keeps a single bad frame from tearing the whole overlay apart.
            result[i] = double.IsFinite(projected.X) && double.IsFinite(projected.Y)
                ? projected
                : new Point(-10000, -10000);
        }

        return result;
    }
}
