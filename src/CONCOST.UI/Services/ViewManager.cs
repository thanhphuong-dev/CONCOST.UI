using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media.Media3D;

namespace CONCOST.UI.Services;

/// <summary>What a view shows.</summary>
public enum ViewKind
{
    /// <summary>A plan view, looking down.</summary>
    Plan,

    /// <summary>An orbit view of the model.</summary>
    ThreeD,

    /// <summary>A section cut.</summary>
    Section,

    /// <summary>An elevation.</summary>
    Elevation,

    /// <summary>A schedule table.</summary>
    Schedule,

    /// <summary>A sheet with views placed on it.</summary>
    Sheet
}

/// <summary>
/// The camera as plain numbers, so a view's framing can be saved, restored and compared without
/// keeping a live camera alive.
/// </summary>
public struct CameraSnapshot
{
    public Point3D Target { get; set; }

    public double Distance { get; set; }

    public double Yaw { get; set; }

    public double Pitch { get; set; }

    public double FieldOfView { get; set; }

    public double OrthographicHeight { get; set; }

    public CameraProjection Projection { get; set; }

    /// <summary>Reads the framing off a camera.</summary>
    public static CameraSnapshot Capture(CameraController camera)
    {
        if (camera is null) return default;

        return new CameraSnapshot
        {
            Target = camera.Target,
            Distance = camera.Distance,
            Yaw = camera.Yaw,
            Pitch = camera.Pitch,
            FieldOfView = camera.FieldOfView,
            OrthographicHeight = camera.OrthographicHeight,
            Projection = camera.Projection
        };
    }

    /// <summary>Writes the framing onto a camera.</summary>
    public void ApplyTo(CameraController camera)
    {
        if (camera is null) return;

        camera.Target = Target;
        camera.Distance = Distance;
        camera.Yaw = Yaw;
        camera.Pitch = Pitch;
        camera.FieldOfView = FieldOfView;
        camera.OrthographicHeight = OrthographicHeight;
        camera.Projection = Projection;
    }
}

/// <summary>A view a document can open: a name, a kind and the framing it was left in.</summary>
public class ViewDefinition
{
    public ViewDefinition(string name, ViewKind kind = ViewKind.Plan)
    {
        Name = name ?? string.Empty;
        Kind = kind;
    }

    /// <summary>The name shown in the project browser and on the tab.</summary>
    public string Name { get; set; }

    /// <summary>What the view shows.</summary>
    public ViewKind Kind { get; set; }

    /// <summary>The framing the view was last closed with.</summary>
    public CameraSnapshot Camera { get; set; }

    /// <summary>True while the view has a tab.</summary>
    public bool IsOpen { get; set; }

    /// <summary>The object behind the view, e.g. a database row.</summary>
    public object? Tag { get; set; }

    /// <inheritdoc />
    public override string ToString() => $"{Kind}: {Name}";
}

/// <summary>Carries the view that was opened or closed.</summary>
public class ViewEventArgs : EventArgs
{
    public ViewEventArgs(ViewDefinition view) => View = view;

    /// <summary>The view the event is about.</summary>
    public ViewDefinition View { get; }
}

/// <summary>
/// The views of the active project and which one is in front.
/// </summary>
/// <remarks>
/// Opening a view is a request, not a drawing operation: <see cref="OpenRequested"/> lets the shell
/// build a tab, and <see cref="RememberCamera"/> is what makes reopening a view land back where the
/// user left it instead of at a default framing.
/// </remarks>
public class ViewManager
{
    private static readonly Lazy<ViewManager> Shared = new(() => new ViewManager());

    private ViewDefinition? _active;

    /// <summary>The shared instance the application uses.</summary>
    public static ViewManager Current => Shared.Value;

    /// <summary>The views the project knows about.</summary>
    public ObservableCollection<ViewDefinition> Views { get; } = new();

    /// <summary>The view in front.</summary>
    public ViewDefinition? ActiveView
    {
        get => _active;
        set
        {
            if (ReferenceEquals(_active, value)) return;

            _active = value;

            ActiveViewChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>Raised when a view should get a tab and a viewport.</summary>
    public event EventHandler<ViewEventArgs>? OpenRequested;

    /// <summary>Raised after a view closed.</summary>
    public event EventHandler<ViewEventArgs>? ViewClosed;

    /// <summary>Raised when the view in front changes.</summary>
    public event EventHandler? ActiveViewChanged;

    /// <summary>Adds a view definition, replacing one with the same name.</summary>
    public ViewDefinition Add(string name, ViewKind kind = ViewKind.Plan)
    {
        var existing = Find(name);

        if (existing is not null)
        {
            existing.Kind = kind;

            return existing;
        }

        var view = new ViewDefinition(name, kind);

        Views.Add(view);

        return view;
    }

    /// <summary>Finds a view by name, ignoring case.</summary>
    public ViewDefinition? Find(string name) =>
        Views.FirstOrDefault(view => string.Equals(view.Name, name, StringComparison.OrdinalIgnoreCase));

    /// <summary>Opens a view: makes it active and asks the shell to show it.</summary>
    public bool Open(ViewDefinition view)
    {
        if (view is null) return false;

        if (!Views.Contains(view)) Views.Add(view);

        view.IsOpen = true;
        _active = view;

        OpenRequested?.Invoke(this, new ViewEventArgs(view));
        ActiveViewChanged?.Invoke(this, EventArgs.Empty);

        return true;
    }

    /// <summary>Opens a view by name, adding it when the project does not have it yet.</summary>
    public bool Open(string name, ViewKind kind = ViewKind.Plan) => Open(Find(name) ?? Add(name, kind));

    /// <summary>Marks a view closed, after the workspace dropped its tab.</summary>
    public bool Close(ViewDefinition view)
    {
        if (view is null || !view.IsOpen) return false;

        view.IsOpen = false;

        if (ReferenceEquals(_active, view)) _active = Views.LastOrDefault(other => other.IsOpen);

        ViewClosed?.Invoke(this, new ViewEventArgs(view));
        ActiveViewChanged?.Invoke(this, EventArgs.Empty);

        return true;
    }

    /// <summary>Saves the framing of a camera into a view, so reopening it returns there.</summary>
    public void RememberCamera(ViewDefinition view, CameraController camera)
    {
        if (view is null) return;

        view.Camera = CameraSnapshot.Capture(camera);
    }

    /// <summary>Restores a view's framing onto a camera.</summary>
    public void RestoreCamera(ViewDefinition view, CameraController camera)
    {
        if (view is null) return;

        view.Camera.ApplyTo(camera);
    }
}
