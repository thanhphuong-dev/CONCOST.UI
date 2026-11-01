using System;
using System.Windows;
using System.Windows.Media.Media3D;

namespace CONCOST.UI.Services;

/// <summary>
/// The seam between the UI and whatever actually draws the model.
/// </summary>
/// <remarks>
/// The library ships a camera and a plan viewport, not a geometry kernel. A host that has one — a
/// Direct3D renderer, a third-party viewer — plugs it in here, and everything that asks "where is this
/// point on screen" or "what is under the cursor" gets the host's answer instead of the camera's
/// projection. With no adapter the bridge still answers projection questions, which is enough for the
/// grid, the temporary dimensions and the coordinate readout.
/// </remarks>
public interface IRendererAdapter
{
    /// <summary>Projects a point; return <c>false</c> when it is behind the eye.</summary>
    bool TryProject(Point3D world, Size viewport, out Point screen);

    /// <summary>Unprojects a screen point onto the plane the renderer is drawing on.</summary>
    bool TryUnproject(Point screen, Size viewport, out Point3D world);

    /// <summary>Finds the entity under a screen point; return <c>false</c> when nothing is there.</summary>
    bool TryPick(Point screen, Size viewport, out object? entity);
}

/// <summary>Carries the point that was picked and what was under it.</summary>
public class PickEventArgs : EventArgs
{
    public PickEventArgs(Point3D world, object? entity)
    {
        World = world;
        Entity = entity;
    }

    /// <summary>Where the pick landed in model space.</summary>
    public Point3D World { get; }

    /// <summary>What the renderer reported under the cursor, when it has an adapter.</summary>
    public object? Entity { get; }
}

/// <summary>
/// One camera and one optional renderer, shared by the viewport, the coordinate readout and anything
/// that needs to map between model space and the screen.
/// </summary>
public class RendererBridge
{
    private static readonly Lazy<RendererBridge> Shared = new(() => new RendererBridge());

    private object? _hovered;

    /// <summary>The shared instance the application uses.</summary>
    public static RendererBridge Current => Shared.Value;

    /// <summary>The camera every projection goes through.</summary>
    public CameraController Camera { get; } = new();

    /// <summary>The external renderer, when the host has one.</summary>
    public IRendererAdapter? Adapter { get; set; }

    /// <summary>True when a real renderer answers the pick and projection questions.</summary>
    public bool IsAccelerated => Adapter is not null;

    /// <summary>The model-space point under the cursor.</summary>
    public Point3D CursorWorld { get; private set; }

    /// <summary>Raised when a redraw is needed, e.g. after the camera moved.</summary>
    public event EventHandler? RenderRequested;

    /// <summary>Raised when the hovered entity changes.</summary>
    public event EventHandler<PickEventArgs>? HoverChanged;

    /// <summary>Asks every listener to redraw.</summary>
    public void Invalidate() => RenderRequested?.Invoke(this, EventArgs.Empty);

    /// <summary>Projects a model point to the screen, through the adapter when there is one.</summary>
    public bool TryProject(Point3D world, Size viewport, out Point screen)
    {
        if (Adapter is not null) return Adapter.TryProject(world, viewport, out screen);

        return Camera.TryProject(world, viewport, out screen);
    }

    /// <summary>Unprojects a screen point, through the adapter when there is one.</summary>
    public bool TryUnproject(Point screen, Size viewport, out Point3D world)
    {
        if (Adapter is not null) return Adapter.TryUnproject(screen, viewport, out world);

        world = Camera.Unproject(screen, viewport);

        return true;
    }

    /// <summary>Finds what is under the cursor; without an adapter nothing is.</summary>
    public bool TryPick(Point screen, Size viewport, out object? entity)
    {
        if (Adapter is not null) return Adapter.TryPick(screen, viewport, out entity);

        entity = null;

        return false;
    }

    /// <summary>
    /// Reports where the cursor is: updates the shared coordinate state and raises
    /// <see cref="HoverChanged"/> when the entity under it changed.
    /// </summary>
    public void UpdateCursor(Point screen, Size viewport)
    {
        if (!TryUnproject(screen, viewport, out Point3D world)) return;

        CursorWorld = world;

        ApplicationStateService.Current.CursorPosition = world;

        TryPick(screen, viewport, out object? entity);

        if (ReferenceEquals(entity, _hovered)) return;

        _hovered = entity;

        HoverChanged?.Invoke(this, new PickEventArgs(world, entity));
    }
}
