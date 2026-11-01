using System;
using System.Collections.Generic;
using System.Windows.Media.Media3D;

namespace CONCOST.UI.RevitLike.Model;

/// <summary>
/// Anything the model can hold and the properties palette can edit.
/// </summary>
public abstract class BuildingElement
{
    protected BuildingElement(string name, string category)
    {
        Name = name;
        Category = category;
    }

    /// <summary>Display name, also what the project browser shows.</summary>
    public string Name { get; set; }

    /// <summary>Revit-style category label ("Walls", "Rooms", …).</summary>
    public string Category { get; }

    /// <summary>Element id shown in the properties header.</summary>
    public string Id { get; } = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();

    public override string ToString() => Name;
}

/// <summary>A straight wall drawn in plan, in millimetres.</summary>
public sealed class WallElement : BuildingElement
{
    public WallElement(string name, Point3D start, Point3D end)
        : base(name, "Walls")
    {
        Start = start;
        End = end;
    }

    public Point3D Start { get; set; }

    public Point3D End { get; set; }

    /// <summary>Wall thickness in millimetres.</summary>
    public double Thickness { get; set; } = 200;

    /// <summary>Unconnected height in millimetres.</summary>
    public double Height { get; set; } = 3000;

    /// <summary>Offset of the wall base from its level.</summary>
    public double BaseOffset { get; set; }

    public string Material { get; set; } = "Concrete - Cast in place";

    public string Function { get; set; } = "Interior";

    public bool IsStructural { get; set; }

    public bool IsLocked { get; set; }

    public string Comments { get; set; } = string.Empty;

    /// <summary>Plan length in millimetres.</summary>
    public double Length => (End - Start).Length;

    public override string ToString() => $"{Name} ({Length:0} mm)";
}

/// <summary>A rectangular room bounded by walls.</summary>
public sealed class RoomElement : BuildingElement
{
    public RoomElement(string number, string name, double x, double y, double width, double depth)
        : base(name, "Rooms")
    {
        Number = number;
        X = x;
        Y = y;
        Width = width;
        Depth = depth;
    }

    public string Number { get; }

    public double X { get; set; }

    public double Y { get; set; }

    public double Width { get; set; }

    public double Depth { get; set; }

    public string Department { get; set; } = "General";

    public string Level { get; set; } = "Level 1";

    /// <summary>Floor area in square metres.</summary>
    public double Area => Width * Depth / 1_000_000.0;

    public override string ToString() => $"{Number} - {Name}";
}

/// <summary>One storey: its own walls, rooms and elevation.</summary>
public sealed class LevelModel
{
    public LevelModel(string name, double elevation)
    {
        Name = name;
        Elevation = elevation;
    }

    public string Name { get; }

    /// <summary>Elevation above the project base, in millimetres.</summary>
    public double Elevation { get; }

    public List<WallElement> Walls { get; } = new();

    public List<RoomElement> Rooms { get; } = new();

    /// <summary>Adds a wall built from millimetre coordinates.</summary>
    public WallElement AddWall(string name, double x1, double y1, double x2, double y2,
        double thickness = 200, string material = "Concrete - Cast in place")
    {
        var wall = new WallElement(name, new Point3D(x1, y1, 0), new Point3D(x2, y2, 0))
        {
            Thickness = thickness,
            Material = material
        };

        Walls.Add(wall);

        return wall;
    }
}

/// <summary>
/// The whole project: a handful of levels, each with a plan. Small on purpose - the sample is about
/// the shell, so the model only has to be rich enough for selection, the property palette and undo
/// to have something real to work on.
/// </summary>
public sealed class BuildingModel
{
    public BuildingModel()
    {
        Levels.Add(BuildLevel1());
        Levels.Add(BuildLevel2());
    }

    public string ProjectName { get; } = "Riverside Tower";

    public string ProjectNumber { get; } = "P-2401";

    public List<LevelModel> Levels { get; } = new();

    /// <summary>Bounds of every level in plan, used by Zoom Extents.</summary>
    public Rect3D Bounds => PlanBounds(null, includeHeight: false);

    /// <summary>Extents of the whole building, tall enough for the three-dimensional view.</summary>
    public Rect3D ModelBounds => PlanBounds(null, includeHeight: true);

    /// <summary>
    /// Extents of one storey, placed at that storey's elevation so a straight-down camera looks at
    /// the right floor rather than at the base of the building.
    /// </summary>
    public Rect3D PlanBounds(LevelModel level) => PlanBounds(level, includeHeight: false);

    private Rect3D PlanBounds(LevelModel? level, bool includeHeight)
    {
        double minX = double.MaxValue, minY = double.MaxValue;
        double maxX = double.MinValue, maxY = double.MinValue;
        double minZ = double.MaxValue, maxZ = double.MinValue;

        foreach (var candidate in level is null ? Levels : new List<LevelModel> { level })
        {
            double baseZ = candidate.Elevation;
            double topZ = baseZ;

            foreach (var wall in candidate.Walls)
            {
                minX = Math.Min(minX, Math.Min(wall.Start.X, wall.End.X));
                minY = Math.Min(minY, Math.Min(wall.Start.Y, wall.End.Y));
                maxX = Math.Max(maxX, Math.Max(wall.Start.X, wall.End.X));
                maxY = Math.Max(maxY, Math.Max(wall.Start.Y, wall.End.Y));

                minZ = Math.Min(minZ, baseZ + wall.BaseOffset);
                topZ = Math.Max(topZ, baseZ + wall.BaseOffset + wall.Height);
            }

            maxZ = Math.Max(maxZ, topZ);
        }

        if (minX > maxX) return new Rect3D(-12000, -8000, 0, 24000, 16000, includeHeight ? 3000 : 0);

        // A little breathing room so the outer walls are not flush with the edge.
        double z = level?.Elevation ?? (includeHeight ? minZ : 0);
        double depth = includeHeight ? Math.Max(1, maxZ - minZ) : 0;

        return new Rect3D(minX - 1500, minY - 1500, z, maxX - minX + 3000, maxY - minY + 3000, depth);
    }

    private static LevelModel BuildLevel1()
    {
        var level = new LevelModel("Level 1", 0);

        // Shell: 24 m x 16 m, 300 mm exterior walls.
        level.AddWall("Basic Wall: Exterior", 0, 0, 24000, 0, 300, "Concrete - Cast in place");
        level.AddWall("Basic Wall: Exterior", 24000, 0, 24000, 16000, 300);
        level.AddWall("Basic Wall: Exterior", 24000, 16000, 0, 16000, 300);
        level.AddWall("Basic Wall: Exterior", 0, 16000, 0, 0, 300);

        // Core and partitions, 200 mm interior walls.
        level.AddWall("Basic Wall: Interior", 9000, 300, 9000, 7600, 200, "Concrete - Cast in place");
        level.AddWall("Basic Wall: Interior", 9000, 8400, 9000, 15700, 200);
        level.AddWall("Basic Wall: Interior", 300, 7600, 9000, 7600, 200);
        level.AddWall("Basic Wall: Interior", 9000, 8400, 23700, 8400, 200);
        level.AddWall("Basic Wall: Interior", 15600, 8400, 15600, 15700, 200);
        level.AddWall("Basic Wall: Interior", 9000, 12000, 15600, 12000, 200);

        level.Rooms.Add(new RoomElement("101", "Reception", 300, 300, 8550, 7100));
        level.Rooms.Add(new RoomElement("102", "Open Office", 9300, 300, 14350, 7900));
        level.Rooms.Add(new RoomElement("103", "Meeting", 9300, 8700, 6060, 3100));
        level.Rooms.Add(new RoomElement("104", "Break Room", 15900, 8700, 7750, 3100));
        level.Rooms.Add(new RoomElement("105", "Storage", 9300, 12300, 6060, 3300));
        level.Rooms.Add(new RoomElement("106", "Archive", 15900, 12300, 7750, 3300));

        return level;
    }

    private static LevelModel BuildLevel2()
    {
        var level = new LevelModel("Level 2", 3600);

        level.AddWall("Basic Wall: Exterior", 0, 0, 24000, 0, 300);
        level.AddWall("Basic Wall: Exterior", 24000, 0, 24000, 16000, 300);
        level.AddWall("Basic Wall: Exterior", 24000, 16000, 0, 16000, 300);
        level.AddWall("Basic Wall: Exterior", 0, 16000, 0, 0, 300);

        level.AddWall("Basic Wall: Interior", 300, 6000, 23700, 6000, 200);
        level.AddWall("Basic Wall: Interior", 12000, 300, 12000, 5800, 200);
        level.AddWall("Basic Wall: Interior", 8000, 6200, 8000, 15700, 200);

        level.Rooms.Add(new RoomElement("201", "Studio", 300, 300, 11500, 5500));
        level.Rooms.Add(new RoomElement("202", "Lab", 12300, 300, 11400, 5500));
        level.Rooms.Add(new RoomElement("203", "Library", 300, 6300, 7500, 9400));
        level.Rooms.Add(new RoomElement("204", "Training", 8300, 6300, 15150, 9400));

        foreach (var room in level.Rooms) room.Level = "Level 2";

        return level;
    }
}
