using System;
using System.Windows.Media.Media3D;

namespace CONCOST.UI.Services;

/// <summary>How a host turns its scene into screen coordinates.</summary>
public enum ProjectionMode
{
    /// <summary>No foreshortening: a plan or an elevation where a ruler holds everywhere.</summary>
    Orthographic,

    /// <summary>Nearer geometry is larger: the usual 3D view.</summary>
    Perspective,

    /// <summary>Perspective while orbiting, orthographic as soon as a face is looked at square on.</summary>
    PerspectiveWithOrthoFaces
}

/// <summary>
/// The slice of a camera a view cube needs, and the whole of what it is allowed to touch.
/// </summary>
/// <remarks>
/// <para>
/// Deliberately thin: a rotation, the point to turn around, the projection and a fit. Everything else
/// a viewer does - panning, zooming, picking, drawing - stays with the host, so the cube can drive a
/// WPF viewport, an OpenGL viewer or a Revit add-in through the same four members and never owns a
/// camera of its own.
/// </para>
/// <para>
/// Directions are in the model's world coordinates with Z up, as Revit: X east, Y north, Z up. A host
/// that works Y-up - WPF's own <see cref="System.Windows.Media.Media3D.MatrixCamera"/>, three.js -
/// converts at this boundary and nowhere else.
/// </para>
/// </remarks>
public interface ICameraHost
{
    /// <summary>
    /// The current orientation, as a rotation from world axes onto the screen axes. Read on every
    /// frame; written only while a transition animates or the user drags the cube.
    /// </summary>
    Quaternion Rotation { get; set; }

    /// <summary>The point the camera turns around, which a transition leaves where it is.</summary>
    Point3D PivotPoint { get; }

    /// <summary>The projection in force, or the mode asked for when it is <see cref="ProjectionMode.PerspectiveWithOrthoFaces"/>.</summary>
    ProjectionMode Projection { get; set; }

    /// <summary>Frames the model again, for <c>FitToViewOnChange</c>.</summary>
    void FitToView();
}

/// <summary>
/// Optional companion to <see cref="ICameraHost"/>: a camera that can say when it moved.
/// </summary>
/// <remarks>
/// Without it the cube has no way of knowing that the user orbited the viewport, and the host has to
/// call <see cref="Controls.ViewCube.Invalidate"/> from its own render loop. Both are supported on
/// purpose - the second keeps the host interface down to what a foreign viewer can implement in a
/// handful of lines.
/// </remarks>
public interface ICameraHostEvents
{
    /// <summary>Raised after the camera moved, turned or changed projection.</summary>
    event EventHandler? CameraChanged;
}
