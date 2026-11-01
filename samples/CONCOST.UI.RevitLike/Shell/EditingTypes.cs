namespace CONCOST.UI.RevitLike.Shell;

/// <summary>What the left button does in the drawing area right now.</summary>
public enum ToolMode
{
    /// <summary>Pick and edit existing elements.</summary>
    Select,

    /// <summary>Place a wall between two points.</summary>
    Wall,

    /// <summary>Measure between two points and leave the dimension on screen.</summary>
    Dimension
}

/// <summary>
/// The wall type a placement run will use. The structure tab and the command line both pick one of
/// these, which is why the thickness, material and function travel together.
/// </summary>
public sealed record WallPreset(
    string Name,
    double Thickness,
    double Height,
    string Material,
    string Function,
    bool IsStructural)
{
    /// <summary>The 200 mm partition the Architecture tab places.</summary>
    public static readonly WallPreset Partition =
        new("Basic Wall: Interior", 200, 3000, "Concrete - Cast in place", "Interior", false);

    /// <summary>The 400 mm structural wall the Structure tab places.</summary>
    public static readonly WallPreset Structural =
        new("Basic Wall: Structural", 400, 3000, "Concrete - Cast in place", "Structure", true);
}
