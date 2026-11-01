using System;
using System.Windows;
using System.Windows.Media.Media3D;
using CONCOST.UI.Controls;

namespace CONCOST.UI.Services;

/// <summary>How a <see cref="CameraController"/> turns the view volume into screen coordinates.</summary>
public enum CameraProjection
{
    /// <summary>Nearer geometry is larger; the default for a 3D view.</summary>
    Perspective,

    /// <summary>No foreshortening: a plan or elevation where a ruler holds everywhere.</summary>
    Orthographic
}

/// <summary>Carries the camera state after a change.</summary>
public class CameraChangedEventArgs : EventArgs
{
    public CameraChangedEventArgs(Vector3D direction, bool isFinal)
    {
        Direction = direction;
        IsFinal = isFinal;
    }

    /// <summary>Current direction from the target towards the eye, normalised.</summary>
    public Vector3D Direction { get; }

    /// <summary><c>false</c> while a gesture is still in flight, so a host can defer the heavy redraw.</summary>
    public bool IsFinal { get; }
}

/// <summary>
/// The orbit camera a CAD viewport sits on: a target point, a distance, two angles and a roll.
/// </summary>
/// <remarks>
/// <para>
/// Yaw and pitch rather than a free quaternion, because every consumer here wants the same thing the
/// user does — "which way am I looking at this" — and a pair of angles can be shown in a status line,
/// saved in a layout profile and compared for equality. Z is up, so a shot from above is pitch 90.
/// </para>
/// <para>
/// The controller owns no visual and takes the viewport size as an argument instead of a property:
/// the same camera can then drive a viewport, a mini map and a print sheet at once, and a test can
/// project into any size without arranging a control.
/// </para>
/// <para>
/// It is also the reference implementation of <see cref="ICameraHost"/>: a view cube can drive it
/// through the four members of that interface, and the roll that a cube's rotation introduces is
/// kept here rather than thrown away.
/// </para>
/// </remarks>
public class CameraController : ICameraHost, ICameraHostEvents
{
    /// <summary>The smallest distance <see cref="Zoom"/> will go to, so the eye never enters the target.</summary>
    public const double MinDistance = 1e-3;

    private Point3D _target;
    private double _distance = 5000;
    private double _yaw = -55;
    private double _pitch = 30;
    private double _roll;
    private double _fieldOfView = 45;
    private double _orthographicHeight = 5000;
    private CameraProjection _projection = CameraProjection.Perspective;
    private ProjectionMode _projectionMode = ProjectionMode.Perspective;

    /// <summary>Raised after any change; see <see cref="CameraChangedEventArgs.IsFinal"/>.</summary>
    public event EventHandler<CameraChangedEventArgs>? Changed;

    /// <summary>Raised alongside <see cref="Changed"/>, for listeners that only care that something moved.</summary>
    public event EventHandler? CameraChanged;

    /// <summary>Raised when the view wants the scene framed again; the geometry is the host's business.</summary>
    public event EventHandler? FitToViewRequested;

    /// <summary>The point the camera orbits and looks at.</summary>
    public Point3D Target
    {
        get => _target;
        set
        {
            if (_target == value) return;

            _target = value;
            OnChanged(true);
        }
    }

    /// <summary>Distance from the eye to <see cref="Target"/>, in world units.</summary>
    public double Distance
    {
        get => _distance;
        set
        {
            double next = Math.Max(MinDistance, value);
            if (Math.Abs(next - _distance) < 1e-9) return;

            _distance = next;
            OnChanged(true);
        }
    }

    /// <summary>Rotation about the vertical axis, in degrees. 0 puts the eye on +X.</summary>
    public double Yaw
    {
        get => _yaw;
        set
        {
            // Normalised into [-180, 180) so a save/restore and an equality check agree on 450 and 90.
            double next = ((value % 360) + 360) % 360;
            if (next >= 180) next -= 360;
            if (Math.Abs(next - _yaw) < 1e-9) return;

            _yaw = next;
            OnChanged(true);
        }
    }

    /// <summary>Elevation, in degrees: 90 looks straight down, -90 straight up.</summary>
    public double Pitch
    {
        get => _pitch;
        set
        {
            double next = Math.Max(-90, Math.Min(90, value));
            if (Math.Abs(next - _pitch) < 1e-9) return;

            _pitch = next;
            OnChanged(true);
        }
    }

    /// <summary>
    /// Rotation about the view axis, in degrees: positive rolls the scene clockwise on screen, which is
    /// what a view cube's roll buttons ask for.
    /// </summary>
    public double Roll
    {
        get => _roll;
        set
        {
            double next = NormaliseAngle(value);
            if (Math.Abs(next - _roll) < 1e-9) return;

            _roll = next;
            OnChanged(true);
        }
    }

    /// <summary>Vertical field of view for <see cref="CameraProjection.Perspective"/>, in degrees.</summary>
    public double FieldOfView
    {
        get => _fieldOfView;
        set
        {
            double next = Math.Max(1, Math.Min(170, value));
            if (Math.Abs(next - _fieldOfView) < 1e-9) return;

            _fieldOfView = next;
            OnChanged(true);
        }
    }

    /// <summary>How many world units fit vertically under <see cref="CameraProjection.Orthographic"/>.</summary>
    public double OrthographicHeight
    {
        get => _orthographicHeight;
        set
        {
            double next = Math.Max(MinDistance, value);
            if (Math.Abs(next - _orthographicHeight) < 1e-9) return;

            _orthographicHeight = next;
            OnChanged(true);
        }
    }

    /// <summary>Perspective or orthographic.</summary>
    public CameraProjection Projection
    {
        get => _projection;
        set
        {
            _projectionMode = value == CameraProjection.Orthographic
                ? ProjectionMode.Orthographic
                : ProjectionMode.Perspective;

            ApplyProjection(value);
        }
    }

    /// <summary>
    /// The projection as a view cube sees it. <see cref="ProjectionMode.PerspectiveWithOrthoFaces"/> is
    /// remembered as the mode and leaves the camera in perspective: the caller does the switching.
    /// </summary>
    ProjectionMode ICameraHost.Projection
    {
        get => _projectionMode;
        set
        {
            _projectionMode = value;
            ApplyProjection(value == ProjectionMode.Orthographic
                ? CameraProjection.Orthographic
                : CameraProjection.Perspective);
        }
    }

    private void ApplyProjection(CameraProjection value)
    {
        if (_projection == value) return;

        _projection = value;

        // Keep the apparent zoom when the projection flips, otherwise a switch from perspective
        // to plan jumps by the field-of-view factor.
        if (value == CameraProjection.Orthographic) _orthographicHeight = VisibleHeight(_distance);
        else _distance = _orthographicHeight / (2 * Math.Tan(FieldOfViewRadians / 2));

        OnChanged(true);
    }

    private double FieldOfViewRadians => _fieldOfView * Math.PI / 180;

    /// <summary>Unit vector from the target towards the eye.</summary>
    public Vector3D Direction
    {
        get
        {
            double yaw = _yaw * Math.PI / 180;
            double pitch = _pitch * Math.PI / 180;
            double horizontal = Math.Cos(pitch);

            return new Vector3D(horizontal * Math.Cos(yaw), horizontal * Math.Sin(yaw), Math.Sin(pitch));
        }
    }

    /// <summary>Where the eye sits, in world coordinates.</summary>
    public Point3D Eye => _target + Direction * _distance;

    /// <summary>Unit vector from the eye towards the target.</summary>
    public Vector3D Forward => -Direction;

    /// <summary>
    /// Screen-right in world space, turned by <see cref="Roll"/> about the view axis. Falls back to the
    /// Y axis when the view is straight down, which is the only case where the normal construction
    /// degenerates.
    /// </summary>
    public Vector3D Right
    {
        get
        {
            var forward = Forward;
            var baseRight = BaseRight(forward);

            if (Math.Abs(_roll) < 1e-9) return baseRight;

            // Rolling is a turn of the two screen axes round the third: right becomes right cos + up sin,
            // which leaves up = right x forward correct without a second formula.
            var baseUp = Vector3D.CrossProduct(baseRight, forward);
            double radians = _roll * Math.PI / 180;

            return baseRight * Math.Cos(radians) + baseUp * Math.Sin(radians);
        }
    }

    /// <summary>Screen-up in world space, orthogonal to <see cref="Right"/> and <see cref="Forward"/>.</summary>
    public Vector3D Up
    {
        get
        {
            var up = Vector3D.CrossProduct(Right, Forward);
            up.Normalize();
            return up;
        }
    }

    /// <summary>
    /// The orientation as a single value, the form a view cube and every other consumer of
    /// <see cref="ICameraHost"/> exchange. Reading it costs a few cross products; writing it splits the
    /// rotation back into yaw, pitch and roll, which is why the three angles stay the real state.
    /// </summary>
    public Quaternion Rotation
    {
        get => ViewCubeMath.FromBasis(Right, Up, Forward);
        set
        {
            ViewCubeMath.ToBasis(value, out Vector3D right, out _, out Vector3D forward);

            var eye = -forward;
            double horizontal = Math.Sqrt(eye.X * eye.X + eye.Y * eye.Y);
            double pitch = Math.Max(-90, Math.Min(90, Math.Atan2(eye.Z, horizontal) * 180 / Math.PI));

            // Straight up or down leaves yaw undefined; the previous one is as good an answer as any and
            // keeps a slerp through the pole from spinning the model for no reason.
            double yaw = horizontal < 1e-9 ? _yaw : Math.Atan2(eye.Y, eye.X) * 180 / Math.PI;

            var baseRight = BaseRight(forward);
            var baseUp = Vector3D.CrossProduct(baseRight, forward);
            double roll = Math.Atan2(
                Vector3D.DotProduct(right, baseUp),
                Vector3D.DotProduct(right, baseRight)) * 180 / Math.PI;

            if (Math.Abs(pitch - _pitch) < 1e-9 && Math.Abs(yaw - _yaw) < 1e-9 && Math.Abs(roll - _roll) < 1e-9) return;

            _pitch = pitch;
            _yaw = NormaliseAngle(yaw);
            _roll = NormaliseAngle(roll);
            OnChanged(true);
        }
    }

    private static Vector3D WorldUp => new(0, 0, 1);

    /// <summary>Screen-right of the same view with no roll, which is what <see cref="Roll"/> is measured against.</summary>
    private static Vector3D BaseRight(Vector3D forward)
    {
        var right = Vector3D.CrossProduct(forward, WorldUp);
        if (right.LengthSquared < 1e-12) right = Vector3D.CrossProduct(forward, new Vector3D(0, 1, 0));
        if (right.LengthSquared < 1e-12) right = new Vector3D(1, 0, 0);

        right.Normalize();
        return right;
    }

    private static double NormaliseAngle(double degrees)
    {
        double next = ((degrees % 360) + 360) % 360;
        return next >= 180 ? next - 360 : next;
    }

    /// <summary>World-to-view matrix, for hosts that feed a camera to their own renderer.</summary>
    public Matrix3D ViewMatrix
    {
        get
        {
            var eye = Eye;
            var right = Right;
            var up = Up;
            var forward = Forward;

            return new Matrix3D(
                right.X, up.X, forward.X, 0,
                right.Y, up.Y, forward.Y, 0,
                right.Z, up.Z, forward.Z, 0,
                -Vector3D.DotProduct(right, (Vector3D)eye),
                -Vector3D.DotProduct(up, (Vector3D)eye),
                -Vector3D.DotProduct(forward, (Vector3D)eye),
                1);
        }
    }

    /// <summary>Height of the world that fits vertically at <paramref name="distance"/>.</summary>
    public double VisibleHeight(double distance) => 2 * distance * Math.Tan(FieldOfViewRadians / 2);

    /// <summary>Turns the camera around the target. Pitch stops at the poles so the view never flips.</summary>
    public void Orbit(double deltaYaw, double deltaPitch, bool isFinal = true)
    {
        double pitch = Math.Max(-89, Math.Min(89, _pitch + deltaPitch));
        double yaw = _yaw + deltaYaw;

        if (Math.Abs(pitch - _pitch) < 1e-9 && Math.Abs(yaw - _yaw) < 1e-9) return;

        _pitch = pitch;
        _yaw = ((yaw % 360) + 360) % 360;
        if (_yaw >= 180) _yaw -= 360;

        OnChanged(isFinal);
    }

    /// <summary>
    /// Slides the target so the drawing follows the pointer: dragging right moves the model right,
    /// which means the target moves left.
    /// </summary>
    /// <param name="deltaX">Horizontal pointer travel in pixels.</param>
    /// <param name="deltaY">Vertical pointer travel in pixels, positive down.</param>
    /// <param name="viewport">Size of the surface the pixels were measured on.</param>
    public void Pan(double deltaX, double deltaY, Size viewport, bool isFinal = true)
    {
        if (viewport.Height <= 0) return;

        double worldPerPixel = (Projection == CameraProjection.Perspective
            ? VisibleHeight(_distance)
            : _orthographicHeight) / viewport.Height;

        _target += Right * (-deltaX * worldPerPixel) + Up * (deltaY * worldPerPixel);
        OnChanged(isFinal);
    }

    /// <summary>Multiplies the distance: a factor above 1 pulls back, below 1 moves in.</summary>
    public void Zoom(double factor, bool isFinal = true)
    {
        if (factor <= 0) return;

        double next = Math.Max(MinDistance, _distance * factor);

        if (Projection == CameraProjection.Orthographic)
        {
            _orthographicHeight *= next / _distance;
            _distance = next;
        }
        else
        {
            _distance = next;
        }

        OnChanged(isFinal);
    }

    /// <summary>
    /// Zooms while keeping the world point under <paramref name="anchor"/> where it is — the behaviour
    /// a wheel over a viewport has to have, and the reason panning after zooming feels wrong if it is
    /// missing.
    /// </summary>
    public void ZoomAt(Point anchor, Size viewport, double factor)
    {
        if (factor <= 0 || viewport.Height <= 0) return;

        var before = Unproject(anchor, viewport);
        Zoom(factor, isFinal: false);
        var after = Unproject(anchor, viewport);

        _target += before - after;
        OnChanged(true);
    }

    /// <summary>Straightens the camera towards <paramref name="direction"/> (target → eye).</summary>
    public void SetOrientation(Vector3D direction, bool isFinal = true)
    {
        if (direction.LengthSquared < 1e-12) return;

        direction.Normalize();

        double horizontal = Math.Sqrt(direction.X * direction.X + direction.Y * direction.Y);

        _pitch = Math.Atan2(direction.Z, horizontal) * 180 / Math.PI;
        _yaw = horizontal < 1e-9 ? _yaw : Math.Atan2(direction.Y, direction.X) * 180 / Math.PI;

        if (_yaw >= 180) _yaw -= 360;

        OnChanged(isFinal);
    }

    /// <summary>Centres the view on <paramref name="target"/> without changing the angles.</summary>
    public void LookAt(Point3D target) => Target = target;

    /// <summary>Pulls back until <paramref name="bounds"/> fits, with a small margin.</summary>
    public void FitTo(Rect3D bounds, double margin = 1.15)
    {
        if (bounds.IsEmpty) return;

        _target = new Point3D(
            bounds.X + bounds.SizeX / 2,
            bounds.Y + bounds.SizeY / 2,
            bounds.Z + bounds.SizeZ / 2);

        double radius = new Vector3D(bounds.SizeX, bounds.SizeY, bounds.SizeZ).Length / 2;
        if (radius < 1e-6) radius = 1;

        _distance = Math.Max(MinDistance, radius / Math.Tan(FieldOfViewRadians / 2) * margin);
        _orthographicHeight = radius * 2 * margin;

        OnChanged(true);
    }

    /// <summary>
    /// Frames the model again. The bounds of the scene belong to the host, so this asks rather than
    /// guesses: a viewport answers by calling <see cref="FitTo"/> with what it actually contains.
    /// </summary>
    public void FitToView() => FitToViewRequested?.Invoke(this, EventArgs.Empty);

    void ICameraHost.FitToView() => FitToView();

    /// <summary>The point a transition keeps fixed: the target this camera already orbits.</summary>
    Point3D ICameraHost.PivotPoint => _target;

    /// <summary>Projects a world point to the surface. <c>false</c> when it sits behind the eye.</summary>
    public bool TryProject(Point3D world, Size viewport, out Point screen)
    {
        screen = default;

        var offset = (Vector3D)(world - Eye);

        double depth = Vector3D.DotProduct(offset, Forward);
        if (depth <= 1e-9) return false;

        double x = Vector3D.DotProduct(offset, Right);
        double y = Vector3D.DotProduct(offset, Up);

        if (Projection == CameraProjection.Perspective)
        {
            double focal = viewport.Height / 2 / Math.Tan(FieldOfViewRadians / 2);
            x = x * focal / depth;
            y = y * focal / depth;
        }
        else
        {
            double scale = viewport.Height / _orthographicHeight;
            x *= scale;
            y *= scale;
        }

        screen = new Point(viewport.Width / 2 + x, viewport.Height / 2 - y);
        return true;
    }

    /// <summary>
    /// The world point under a surface point, taken on the plane through <see cref="Target"/> that
    /// faces the camera. That plane is what a pointer drag or a coordinate readout actually refers to.
    /// </summary>
    public Point3D Unproject(Point point, Size viewport)
    {
        if (viewport.Height <= 0) return _target;

        var forward = Forward;
        var right = Right;
        var up = Up;

        double dx = point.X - viewport.Width / 2;
        double dy = viewport.Height / 2 - point.Y;
        var eye = Eye;

        if (Projection == CameraProjection.Perspective)
        {
            double focal = viewport.Height / 2 / Math.Tan(FieldOfViewRadians / 2);
            var ray = right * dx + up * dy + forward * focal;

            double denominator = Vector3D.DotProduct(ray, forward);
            if (Math.Abs(denominator) < 1e-12) return _target;

            double travel = Vector3D.DotProduct(_target - eye, forward) / denominator;
            return eye + ray * travel;
        }

        double scale = viewport.Height / _orthographicHeight;
        var origin = eye + right * (dx / scale) + up * (dy / scale);

        return origin + forward * Vector3D.DotProduct(_target - eye, forward);
    }

    /// <summary>Copies this camera's state onto <paramref name="other"/>, e.g. after a view switch.</summary>
    public void CopyTo(CameraController other)
    {
        if (other is null) return;

        other._target = _target;
        other._distance = _distance;
        other._yaw = _yaw;
        other._pitch = _pitch;
        other._roll = _roll;
        other._fieldOfView = _fieldOfView;
        other._orthographicHeight = _orthographicHeight;
        other._projection = _projection;
        other._projectionMode = _projectionMode;
        other.OnChanged(true);
    }

    private void OnChanged(bool isFinal)
    {
        Changed?.Invoke(this, new CameraChangedEventArgs(Direction, isFinal));
        CameraChanged?.Invoke(this, EventArgs.Empty);
    }
}
