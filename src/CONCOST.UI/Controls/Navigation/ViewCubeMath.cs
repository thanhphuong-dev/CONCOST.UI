using System;
using System.Collections.Generic;
using System.Windows.Media.Media3D;

namespace CONCOST.UI.Controls;

/// <summary>Which of the three kinds of zone a region of the cube belongs to.</summary>
public enum ViewCubeRegionKind
{
    /// <summary>One of the six squares in the middle of a face; carries a caption.</summary>
    Face,

    /// <summary>One of the twelve bevelled strips between two faces.</summary>
    Edge,

    /// <summary>One of the eight small triangles at a corner.</summary>
    Corner
}

/// <summary>The four arrows drawn around the cube.</summary>
public enum ArrowDirection
{
    Up,
    Right,
    Down,
    Left
}

/// <summary>The corner of the viewport the cube is docked to.</summary>
public enum ViewCubePosition
{
    TopRight,
    TopLeft,
    BottomRight,
    BottomLeft
}

/// <summary>Named sizes, plus <see cref="Automatic"/> which follows the viewport.</summary>
public enum ViewCubeSize
{
    /// <summary>A fifth of the viewport's short side, clamped to 64-160 px.</summary>
    Automatic,

    /// <summary>64 px.</summary>
    Tiny,

    /// <summary>80 px.</summary>
    Small,

    /// <summary>100 px.</summary>
    Normal,

    /// <summary>128 px.</summary>
    Large
}

/// <summary>
/// One of the 26 directions the cube offers: a face, an edge or a corner, with the forward and up
/// vectors a camera on that side has to take.
/// </summary>
/// <remarks>
/// Built from the normal instead of typed by hand, so an edge such as TOP-FRONT-RIGHT cannot drift
/// away from the two faces it joins. The default value is empty rather than the RIGHT face, so a
/// field that was never assigned can be told apart from a real answer.
/// </remarks>
public readonly struct ViewCubeRegion : IEquatable<ViewCubeRegion>
{
    private readonly Vector3D _normal;
    private readonly Vector3D _up;

    internal ViewCubeRegion(ViewCubeRegionKind kind, Vector3D normal, Vector3D up, string name)
    {
        Kind = kind;
        _normal = normal;
        _up = up;
        Name = name;
        Label = kind == ViewCubeRegionKind.Face ? name : string.Empty;
    }

    /// <summary>Face, edge or corner.</summary>
    public ViewCubeRegionKind Kind { get; }

    /// <summary>Stable identifier, e.g. <c>TOP</c> or <c>TOP-FRONT-RIGHT</c>.</summary>
    public string Name { get; }

    /// <summary>The caption drawn on the region: a face label, or empty for an edge or a corner.</summary>
    public string Label { get; }

    /// <summary><c>true</c> for a value that was never assigned a direction.</summary>
    public bool IsEmpty => _normal.LengthSquared < 1e-9;

    /// <summary><c>true</c> when the region is one of the six captioned squares.</summary>
    public bool IsFace => Kind == ViewCubeRegionKind.Face;

    /// <summary>Unit outward normal, i.e. the direction from the model towards the eye.</summary>
    public Vector3D Normal => _normal;

    /// <summary>Direction the eye looks in when it sits on this side: <c>-Normal</c>.</summary>
    public Vector3D Forward => -_normal;

    /// <summary>Screen-up that keeps the model on its feet while looking from this side.</summary>
    public Vector3D Up => _up;

    /// <summary>Screen-right, derived so that <c>Right x Up = -Forward</c> like every other basis here.</summary>
    public Vector3D Right => ViewCubeMath.RightFor(Forward, _up);

    /// <summary>
    /// The same region with the model's front turned <paramref name="frontAngle"/> degrees about the
    /// vertical axis. Both the normal and the up turn, so a view from above shows the compass and the
    /// FRONT caption in their new places while TOP stays on top.
    /// </summary>
    public ViewCubeRegion Rotated(double frontAngle)
    {
        if (IsEmpty || Math.Abs(frontAngle) < 1e-9) return this;

        return new ViewCubeRegion(
            Kind,
            ViewCubeMath.Rotate(_normal, ViewCubeMath.WorldUp, frontAngle),
            ViewCubeMath.Rotate(_up, ViewCubeMath.WorldUp, frontAngle),
            Name);
    }

    /// <inheritdoc />
    public bool Equals(ViewCubeRegion other) => Kind == other.Kind && string.Equals(Name, other.Name, StringComparison.Ordinal);

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is ViewCubeRegion other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => Name is null ? 0 : Name.GetHashCode();

    /// <summary>Two regions are the same when they name the same zone, whatever front angle produced them.</summary>
    public static bool operator ==(ViewCubeRegion left, ViewCubeRegion right) => left.Equals(right);

    /// <summary>Inverse of <see cref="op_Equality"/>.</summary>
    public static bool operator !=(ViewCubeRegion left, ViewCubeRegion right) => !left.Equals(right);

    /// <inheritdoc />
    public override string ToString() => IsEmpty ? "none" : Name;
}

/// <summary>
/// The maths a view cube needs, with no drawing in it: the 26 regions, the basis and quaternion
/// conversions, the short-arc interpolation and the search for the region nearest a direction.
/// </summary>
/// <remarks>
/// <para>
/// Kept out of the control on purpose. Everything here works on <see cref="Vector3D"/> and
/// <see cref="Quaternion"/> from WindowsBase, which a WinForms, OpenGL or direct3D host can reach
/// just as easily, and none of it knows that a <see cref="System.Windows.Media.DrawingContext"/>
/// exists. A test can therefore check the geometry and the camera maths without arranging a window.
/// </para>
/// <para>
/// World axes follow Revit: X east, Y north, Z up. A view direction is the vector from the model
/// towards the eye, so FRONT is (0,-1,0) - the eye stands to the south and looks north.
/// </para>
/// </remarks>
public static class ViewCubeMath
{
    /// <summary>Edge of the square in the middle of a face, as a fraction of the cube's edge.</summary>
    public const double FaceRatio = 0.6;

    /// <summary>Width of the bevel between two faces: the rest of the edge, split in two.</summary>
    public const double EdgeRatio = (1 - FaceRatio) / 2;

    /// <summary>Half the edge of the unit cube the regions are built on.</summary>
    public const double HalfExtent = 0.5;

    /// <summary>The direction of the default home view: the Top-Front-Right corner.</summary>
    public static Vector3D DefaultEye { get; } = NormalizeStatic(new Vector3D(1, -1, 1));

    /// <summary>World up. Z, as Revit, not Y as WPF's own camera.</summary>
    public static Vector3D WorldUp { get; } = new(0, 0, 1);

    // Declared before Regions: static initialisers run in the order they are written, and the region
    // table is built from these.
    private static readonly Vector3D[] Axes =
    {
        new(1, 0, 0),
        new(0, 1, 0),
        new(0, 0, 1)
    };

    private static readonly int[] Signs = { 1, -1 };

    /// <summary>The 26 directions, faces first, then the twelve edges, then the eight corners.</summary>
    public static IReadOnlyList<ViewCubeRegion> Regions { get; } = BuildRegions();

    // ------------------------------------------------------------------ regions

    private static ViewCubeRegion[] BuildRegions()
    {
        var regions = new List<ViewCubeRegion>(26);

        foreach (Vector3D axis in Axes)
        {
            foreach (int sign in Signs)
            {
                Add(ViewCubeRegionKind.Face, axis * sign);
            }
        }

        // An edge touches two faces, so its normal is the sum of theirs. Every pair of axes gives four
        // edges - one per combination of sides - and the three pairs give the twelve.
        for (int first = 0; first < Axes.Length; first++)
        {
            for (int second = first + 1; second < Axes.Length; second++)
            {
                foreach (int firstSign in Signs)
                {
                    foreach (int secondSign in Signs)
                    {
                        Add(ViewCubeRegionKind.Edge, Axes[first] * firstSign + Axes[second] * secondSign);
                    }
                }
            }
        }

        foreach (int x in Signs)
        {
            foreach (int y in Signs)
            {
                foreach (int z in Signs)
                {
                    Add(ViewCubeRegionKind.Corner, new Vector3D(x, y, z));
                }
            }
        }

        return regions.ToArray();

        void Add(ViewCubeRegionKind kind, Vector3D normal)
        {
            normal = Normalize(normal);

            // Faces take their up from the table in the specification; an edge or a corner never looks
            // straight up or down, so the projection of world up onto the screen plane is well defined
            // there and gives the upright answer without another table.
            Vector3D up = kind == ViewCubeRegionKind.Face ? FaceUp(normal) : UprightUp(-normal, WorldUp);

            regions.Add(new ViewCubeRegion(kind, normal, up, NameOf(normal)));
        }
    }

    /// <summary>
    /// Screen-up for a view square on a face, from the table in the specification. TOP and BOTTOM are
    /// the two cases the general formula cannot answer, because forward and world up are parallel.
    /// </summary>
    private static Vector3D FaceUp(Vector3D normal) =>
        Math.Abs(normal.Z) > 0.5 ? new Vector3D(0, normal.Z > 0 ? 1 : -1, 0) : WorldUp;

    // ------------------------------------------------------------------ naming

    private static string NameOf(Vector3D normal)
    {
        var parts = new List<string>(3);

        if (normal.Z > 0.1) parts.Add("TOP");
        else if (normal.Z < -0.1) parts.Add("BOTTOM");

        if (normal.Y > 0.1) parts.Add("BACK");
        else if (normal.Y < -0.1) parts.Add("FRONT");

        if (normal.X > 0.1) parts.Add("RIGHT");
        else if (normal.X < -0.1) parts.Add("LEFT");

        return string.Join("-", parts);
    }

    // ------------------------------------------------------------------ directions

    /// <summary>Unit length, with a defined answer for a vector that has none.</summary>
    public static Vector3D Normalize(Vector3D vector)
    {
        if (double.IsNaN(vector.X) || vector.LengthSquared < 1e-12) return new Vector3D(0, -1, 0);

        vector.Normalize();
        return vector;
    }

    private static Vector3D NormalizeStatic(Vector3D vector)
    {
        vector.Normalize();
        return vector;
    }

    /// <summary>Screen-right for a view, built the way the camera builds it: <c>forward x up</c>.</summary>
    public static Vector3D RightFor(Vector3D forward, Vector3D up)
    {
        var right = Vector3D.CrossProduct(forward, up);

        // Degenerate only when up is along forward, which the callers avoid; the fallback keeps a
        // hand-built basis from producing a zero axis that later normalises into a NaN.
        if (right.LengthSquared < 1e-12) right = Vector3D.CrossProduct(forward, WorldUp);
        if (right.LengthSquared < 1e-12) right = new Vector3D(1, 0, 0);

        right.Normalize();
        return right;
    }

    /// <summary>
    /// The upright up for a forward vector: world up with its forward component removed, which is the
    /// <c>u = (z - (z.f) f) / |...|</c> of the specification. Falls back to <paramref name="fallback"/>
    /// when forward is itself vertical, where no upright answer exists.
    /// </summary>
    public static Vector3D UprightUp(Vector3D forward, Vector3D fallback)
    {
        var up = WorldUp - forward * Vector3D.DotProduct(WorldUp, forward);

        if (up.LengthSquared < 1e-9) up = fallback - forward * Vector3D.DotProduct(fallback, forward);
        if (up.LengthSquared < 1e-9)
        {
            up = Math.Abs(forward.X) < 0.9 ? new Vector3D(1, 0, 0) : new Vector3D(0, 1, 0);
            up -= forward * Vector3D.DotProduct(up, forward);
        }

        if (up.LengthSquared < 1e-12) return new Vector3D(0, 1, 0);

        up.Normalize();
        return up;
    }

    /// <summary>Rotates <paramref name="vector"/> about an arbitrary axis, in degrees (Rodrigues).</summary>
    public static Vector3D Rotate(Vector3D vector, Vector3D axis, double degrees)
    {
        axis = Normalize(axis);

        if (Math.Abs(degrees) < 1e-9) return vector;

        double radians = degrees * Math.PI / 180;
        double cos = Math.Cos(radians);
        double sin = Math.Sin(radians);

        var cross = Vector3D.CrossProduct(axis, vector);
        double dot = Vector3D.DotProduct(axis, vector);

        return vector * cos + cross * sin + axis * (dot * (1 - cos));
    }

    /// <summary>Angle between two directions, in degrees.</summary>
    public static double Angle(Vector3D a, Vector3D b)
    {
        double dot = Vector3D.DotProduct(Normalize(a), Normalize(b));
        return Math.Acos(Math.Min(1, Math.Max(-1, dot))) * 180 / Math.PI;
    }

    /// <summary>Elevation of a view direction above the horizontal plane, in degrees.</summary>
    public static double Elevation(Vector3D eye) =>
        Math.Asin(Math.Min(1, Math.Max(-1, Normalize(eye).Z))) * 180 / Math.PI;

    // ------------------------------------------------------------------ quaternions

    /// <summary>
    /// The rotation that carries the world onto a camera basis. The matrix of that rotation has the
    /// screen axes as its columns - right, up and backwards - and is turned into a quaternion through
    /// Shepperd's method, which stays accurate for the half turns a quarter-turn-only formula loses.
    /// </summary>
    public static Quaternion FromBasis(Vector3D right, Vector3D up, Vector3D forward)
    {
        Vector3D back = -forward;

        double m00 = right.X, m01 = up.X, m02 = back.X;
        double m10 = right.Y, m11 = up.Y, m12 = back.Y;
        double m20 = right.Z, m21 = up.Z, m22 = back.Z;

        double trace = m00 + m11 + m22;
        double x, y, z, w;

        if (trace > 0)
        {
            double s = Math.Sqrt(trace + 1.0) * 2;
            w = 0.25 * s;
            x = (m21 - m12) / s;
            y = (m02 - m20) / s;
            z = (m10 - m01) / s;
        }
        else if (m00 > m11 && m00 > m22)
        {
            double s = Math.Sqrt(1.0 + m00 - m11 - m22) * 2;
            w = (m21 - m12) / s;
            x = 0.25 * s;
            y = (m01 + m10) / s;
            z = (m02 + m20) / s;
        }
        else if (m11 > m22)
        {
            double s = Math.Sqrt(1.0 + m11 - m00 - m22) * 2;
            w = (m02 - m20) / s;
            x = (m01 + m10) / s;
            y = 0.25 * s;
            z = (m12 + m21) / s;
        }
        else
        {
            double s = Math.Sqrt(1.0 + m22 - m00 - m11) * 2;
            w = (m10 - m01) / s;
            x = (m02 + m20) / s;
            y = (m12 + m21) / s;
            z = 0.25 * s;
        }

        return NormalizeRotation(new Quaternion(x, y, z, w));
    }

    /// <summary>The three screen axes of a rotation, back from the quaternion.</summary>
    public static void ToBasis(Quaternion rotation, out Vector3D right, out Vector3D up, out Vector3D forward)
    {
        rotation = NormalizeRotation(rotation);

        double x = rotation.X, y = rotation.Y, z = rotation.Z, w = rotation.W;
        double xx = x * x, yy = y * y, zz = z * z;
        double xy = x * y, xz = x * z, yz = y * z;
        double wx = w * x, wy = w * y, wz = w * z;

        right = new Vector3D(1 - 2 * (yy + zz), 2 * (xy + wz), 2 * (xz - wy));
        up = new Vector3D(2 * (xy - wz), 1 - 2 * (xx + zz), 2 * (yz + wx));
        forward = new Vector3D(-2 * (xz + wy), -2 * (yz - wx), -(1 - 2 * (xx + yy)));
    }

    /// <summary>Unit quaternion; the identity when the input has no length.</summary>
    public static Quaternion NormalizeRotation(Quaternion rotation)
    {
        double length = Math.Sqrt(
            rotation.X * rotation.X + rotation.Y * rotation.Y + rotation.Z * rotation.Z + rotation.W * rotation.W);

        if (length < 1e-12) return Quaternion.Identity;

        return new Quaternion(rotation.X / length, rotation.Y / length, rotation.Z / length, rotation.W / length);
    }

    /// <summary>The rotation a camera on <paramref name="region"/> takes, with the model's front turned.</summary>
    public static Quaternion RotationFor(ViewCubeRegion region, double frontAngle = 0)
    {
        if (region.IsEmpty) return RotationFromEye(DefaultEye, frontAngle);

        ViewCubeRegion turned = region.Rotated(frontAngle);
        return FromBasis(turned.Right, turned.Up, turned.Forward);
    }

    /// <summary>
    /// The rotation for an arbitrary view direction, with the model kept on its feet. A host that only
    /// knows "which way am I looking" - a slider, a saved name, a URL parameter - uses this.
    /// </summary>
    public static Quaternion RotationFromEye(Vector3D eye, double frontAngle = 0)
    {
        var forward = -Normalize(eye);
        var up = UprightUp(forward, WorldUp);

        if (Math.Abs(frontAngle) > 1e-9)
        {
            forward = Rotate(forward, WorldUp, frontAngle);
            up = Rotate(up, WorldUp, frontAngle);
        }

        return FromBasis(RightFor(forward, up), up, forward);
    }

    /// <summary>Spherical interpolation along the shorter arc, with the sign flip that keeps it short.</summary>
    public static Quaternion Slerp(Quaternion from, Quaternion to, double t)
    {
        from = NormalizeRotation(from);
        to = NormalizeRotation(to);

        double dot = from.X * to.X + from.Y * to.Y + from.Z * to.Z + from.W * to.W;

        if (dot < 0)
        {
            // -q is the same orientation as q, and picking the negative one is what makes a 190 degree
            // turn go the 170 degree way instead of spinning the long way round.
            to = new Quaternion(-to.X, -to.Y, -to.Z, -to.W);
            dot = -dot;
        }

        if (dot > 0.9995)
        {
            // Nearly parallel: the sine below would divide by almost nothing.
            return NormalizeRotation(new Quaternion(
                from.X + (to.X - from.X) * t,
                from.Y + (to.Y - from.Y) * t,
                from.Z + (to.Z - from.Z) * t,
                from.W + (to.W - from.W) * t));
        }

        double theta = Math.Acos(Math.Min(1, dot));
        double sin = Math.Sin(theta);
        double weightFrom = Math.Sin((1 - t) * theta) / sin;
        double weightTo = Math.Sin(t * theta) / sin;

        return NormalizeRotation(new Quaternion(
            from.X * weightFrom + to.X * weightTo,
            from.Y * weightFrom + to.Y * weightTo,
            from.Z * weightFrom + to.Z * weightTo,
            from.W * weightFrom + to.W * weightTo));
    }

    /// <summary>The angle between two orientations, in degrees, along the shorter arc.</summary>
    public static double AngleBetween(Quaternion from, Quaternion to)
    {
        from = NormalizeRotation(from);
        to = NormalizeRotation(to);

        double dot = Math.Abs(from.X * to.X + from.Y * to.Y + from.Z * to.Z + from.W * to.W);

        return 2 * Math.Acos(Math.Min(1, dot)) * 180 / Math.PI;
    }

    /// <summary>Ease-in-out of the specification: <c>t^2 (3 - 2t)</c>.</summary>
    public static double SmoothStep(double t)
    {
        t = Math.Min(1, Math.Max(0, t));
        return t * t * (3 - 2 * t);
    }

    /// <summary>The same orientation with any roll removed: screen up becomes the projection of world up.</summary>
    public static Quaternion Upright(Quaternion rotation)
    {
        ToBasis(rotation, out _, out Vector3D up, out Vector3D forward);

        var upright = UprightUp(forward, up);

        if ((upright - up).LengthSquared < 1e-12) return rotation;

        return FromBasis(RightFor(forward, upright), upright, forward);
    }

    /// <summary>Turns the camera about its own view axis; positive degrees roll the scene clockwise.</summary>
    public static Quaternion Roll(Quaternion rotation, double degrees)
    {
        if (Math.Abs(degrees) < 1e-9) return rotation;

        ToBasis(rotation, out Vector3D right, out Vector3D up, out Vector3D forward);

        // Rolling the scene clockwise on screen is turning the camera counter-clockwise about its own
        // axis, which is the sense <c>CameraController.Roll</c> uses as well.
        var rolledRight = Rotate(right, forward, -degrees);
        var rolledUp = Rotate(up, forward, -degrees);

        return FromBasis(rolledRight, rolledUp, forward);
    }

    // ------------------------------------------------------------------ nearest region

    /// <summary>The region whose direction is closest to <paramref name="eye"/>.</summary>
    public static ViewCubeRegion Closest(Vector3D eye)
    {
        eye = Normalize(eye);

        var best = default(ViewCubeRegion);
        double bestDot = double.NegativeInfinity;

        foreach (ViewCubeRegion region in Regions)
        {
            double dot = Vector3D.DotProduct(region.Normal, eye);
            if (dot <= bestDot) continue;

            bestDot = dot;
            best = region;
        }

        return best;
    }

    /// <summary>The region closest to an orientation's view direction.</summary>
    public static ViewCubeRegion Closest(Quaternion rotation)
    {
        ToBasis(rotation, out _, out _, out Vector3D forward);
        return Closest(-forward);
    }

    /// <summary>
    /// True when a direction is within <paramref name="toleranceDegrees"/> of a region, which is how the
    /// 90 degree arrows and the roll buttons decide that the view is square on a face.
    /// </summary>
    public static bool TryClosest(Vector3D eye, double toleranceDegrees, out ViewCubeRegion region)
    {
        eye = Normalize(eye);
        region = Closest(eye);

        return !region.IsEmpty && Angle(region.Normal, eye) <= toleranceDegrees;
    }

    /// <summary>True when an orientation looks square on one of the six faces.</summary>
    public static bool TryFace(Quaternion rotation, double toleranceDegrees, out ViewCubeRegion region)
    {
        ToBasis(rotation, out _, out _, out Vector3D forward);

        return TryClosest(-forward, toleranceDegrees, out region) && region.IsFace;
    }
}

/// <summary>One of the 26 drawable zones: the region it stands for and its polygon on the unit cube.</summary>
public readonly struct ViewCubeFacelet
{
    internal ViewCubeFacelet(ViewCubeRegion region, Vector3D[] corners)
    {
        Region = region;
        Corners = corners;
    }

    /// <summary>The direction this zone stands for.</summary>
    public ViewCubeRegion Region { get; }

    /// <summary>Three or four points on the surface of the unit cube, in order round the zone.</summary>
    public Vector3D[] Corners { get; }

    /// <summary>Middle of the zone, used to sort the zones back to front before drawing.</summary>
    public Vector3D Centre
    {
        get
        {
            var sum = new Vector3D();

            foreach (Vector3D corner in Corners) sum += corner;

            return sum / Corners.Length;
        }
    }
}

/// <summary>
/// The bevelled solid the cube is drawn from: a square face in the middle of each side, a narrow strip
/// between every two faces and a triangle at every corner, all on the surface of a unit cube.
/// </summary>
/// <remarks>
/// Built from <see cref="ViewCubeMath.Regions"/> rather than from a second table of coordinates, so
/// the zone a pointer hits is by construction the zone that was drawn. The bevel is what gives the
/// cube its 26 distinguishable targets: without it the six faces alone would leave the twelve edges
/// and eight corners overlapping whatever face they sit on.
/// </remarks>
public static class ViewCubeMesh
{
    /// <summary>The solid at the default proportions. Built once; the geometry never varies.</summary>
    public static IReadOnlyList<ViewCubeFacelet> Default { get; } = Build();

    /// <summary>The 26 zones, in <see cref="ViewCubeMath.Regions"/> order.</summary>
    public static IReadOnlyList<ViewCubeFacelet> Build(double faceRatio = ViewCubeMath.FaceRatio)
    {
        double half = ViewCubeMath.HalfExtent;
        double a = faceRatio / 2;

        var facelets = new List<ViewCubeFacelet>(26);

        foreach (ViewCubeRegion region in ViewCubeMath.Regions)
        {
            Vector3D normal = region.Normal;

            switch (region.Kind)
            {
                case ViewCubeRegionKind.Face:
                    {
                        Perpendicular(normal, out Vector3D first, out Vector3D second);

                        var centre = normal * half;

                        facelets.Add(new ViewCubeFacelet(region, new[]
                        {
                            centre + first * a + second * a,
                            centre + first * a - second * a,
                            centre - first * a - second * a,
                            centre - first * a + second * a
                        }));
                        break;
                    }

                case ViewCubeRegionKind.Edge:
                    {
                        Faces(normal, out Vector3D first, out Vector3D second, out _);

                        // The strip runs along the cube's edge and leans from one face to the other:
                        // its two long sides are on the face planes, half an edge apart from the corner.
                        Vector3D along = ViewCubeMath.Normalize(Vector3D.CrossProduct(first, second));
                        Vector3D near = first * half + second * a;
                        Vector3D far = first * a + second * half;

                        facelets.Add(new ViewCubeFacelet(region, new[]
                        {
                            near + along * a,
                            far + along * a,
                            far - along * a,
                            near - along * a
                        }));
                        break;
                    }

                default:
                    {
                        Faces(normal, out Vector3D first, out Vector3D second, out Vector3D third);

                        // Each vertex of the corner triangle sits on one face and half an edge away
                        // from the other two, which closes the solid against the three strips.
                        facelets.Add(new ViewCubeFacelet(region, new[]
                        {
                            first * half + second * a + third * a,
                            first * a + second * half + third * a,
                            first * a + second * a + third * half
                        }));
                        break;
                    }
            }
        }

        return facelets;
    }

    /// <summary>The two axes a face does not run along.</summary>
    private static void Perpendicular(Vector3D normal, out Vector3D first, out Vector3D second)
    {
        var axes = new List<Vector3D>(2);

        foreach (Vector3D axis in new[] { new Vector3D(1, 0, 0), new Vector3D(0, 1, 0), new Vector3D(0, 0, 1) })
        {
            if (Math.Abs(Vector3D.DotProduct(axis, normal)) > 0.5) continue;

            axes.Add(axis);
        }

        first = axes[0];
        second = axes[1];
    }

    /// <summary>The face normals a region touches, i.e. its normal with each axis rounded to a side.</summary>
    private static void Faces(Vector3D normal, out Vector3D first, out Vector3D second, out Vector3D third)
    {
        var faces = new List<Vector3D>(3);

        if (Math.Abs(normal.X) > 1e-6) faces.Add(new Vector3D(Math.Sign(normal.X), 0, 0));
        if (Math.Abs(normal.Y) > 1e-6) faces.Add(new Vector3D(0, Math.Sign(normal.Y), 0));
        if (Math.Abs(normal.Z) > 1e-6) faces.Add(new Vector3D(0, 0, Math.Sign(normal.Z)));

        first = faces[0];
        second = faces[1];
        third = faces.Count > 2 ? faces[2] : new Vector3D();
    }
}
