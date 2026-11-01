using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using CONCOST.UI.Controls;
using CONCOST.UI.RevitLike.Model;
using CONCOST.UI.Services;

namespace CONCOST.UI.RevitLike;

/// <summary>
/// The parameter definitions the properties palette reads, and the summary sheets the backstage
/// shows. Both are pure data over the model, which is why they live apart from the shell wiring.
/// </summary>
public partial class MainWindow
{
    private static readonly string[] WallMaterials =
    {
        "Concrete - Cast in place",
        "Concrete - Precast",
        "Masonry - Brick",
        "Masonry - Block",
        "Gypsum Wall Board"
    };

    private static readonly string[] WallFunctions = { "Interior", "Exterior", "Structure", "Foundation", "Retaining" };

    private static readonly string[] Departments = { "General", "Office", "Meeting", "Support", "Lab", "Education", "Storage" };

    private TextBlock? _infoLevels;
    private TextBlock? _infoWalls;
    private TextBlock? _infoRooms;
    private TextBlock? _infoArea;

    private void RegisterModelProperties()
    {
        _properties.Register<WallElement>(
            new PropertyDescriptor(nameof(WallElement.Name), target => ((WallElement)target).Name,
                (target, value) => ((WallElement)target).Name = value?.ToString() ?? string.Empty)
            {
                DisplayName = "Type",
                Category = "Identity Data",
                Description = "Wall type shown in the browser and the status bar."
            },

            new PropertyDescriptor(nameof(WallElement.Length), target => ((WallElement)target).Length)
            {
                DisplayName = "Length",
                Category = "Dimensions",
                Editor = PropertyEditorKind.Numeric,
                Unit = "mm",
                Format = "0"
            },

            new PropertyDescriptor(nameof(WallElement.Thickness), target => ((WallElement)target).Thickness,
                (target, value) => ((WallElement)target).Thickness = ToDouble(value))
            {
                DisplayName = "Thickness",
                Category = "Dimensions",
                Description = "Width of the wall in plan.",
                Editor = PropertyEditorKind.Numeric,
                Unit = "mm",
                DefaultValue = 200.0,
                Format = "0",
                Validator = (_, value) => ToDouble(value) is > 0 and <= 2000
                    ? null
                    : "Thickness must be between 1 and 2000 mm."
            },

            new PropertyDescriptor(nameof(WallElement.Height), target => ((WallElement)target).Height,
                (target, value) => ((WallElement)target).Height = ToDouble(value))
            {
                DisplayName = "Unconnected Height",
                Category = "Constraints",
                Editor = PropertyEditorKind.Numeric,
                Unit = "mm",
                DefaultValue = 3000.0,
                Format = "0",
                Validator = (_, value) => ToDouble(value) is > 0 and <= 20000
                    ? null
                    : "Height must be between 1 and 20000 mm."
            },

            new PropertyDescriptor(nameof(WallElement.BaseOffset), target => ((WallElement)target).BaseOffset,
                (target, value) => ((WallElement)target).BaseOffset = ToDouble(value))
            {
                DisplayName = "Base Offset",
                Category = "Constraints",
                Editor = PropertyEditorKind.Numeric,
                Unit = "mm",
                DefaultValue = 0.0,
                Format = "0"
            },

            new PropertyDescriptor(nameof(WallElement.Material), target => ((WallElement)target).Material,
                (target, value) => ((WallElement)target).Material = value?.ToString() ?? WallMaterials[0])
            {
                DisplayName = "Material",
                Category = "Materials and Finishes",
                Editor = PropertyEditorKind.Choice,
                Choices = WallMaterials.Cast<object>().ToArray(),
                DefaultValue = WallMaterials[0]
            },

            new PropertyDescriptor(nameof(WallElement.Function), target => ((WallElement)target).Function,
                (target, value) => ((WallElement)target).Function = value?.ToString() ?? WallFunctions[0])
            {
                DisplayName = "Function",
                Category = "Construction",
                Editor = PropertyEditorKind.Choice,
                Choices = WallFunctions.Cast<object>().ToArray(),
                DefaultValue = WallFunctions[0]
            },

            new PropertyDescriptor(nameof(WallElement.IsStructural), target => ((WallElement)target).IsStructural,
                (target, value) => ((WallElement)target).IsStructural = value is true)
            {
                DisplayName = "Structural",
                Category = "Structural",
                Editor = PropertyEditorKind.Boolean,
                DefaultValue = false
            },

            new PropertyDescriptor(nameof(WallElement.IsLocked), target => ((WallElement)target).IsLocked,
                (target, value) => ((WallElement)target).IsLocked = value is true)
            {
                DisplayName = "Locked",
                Category = "Constraints",
                Editor = PropertyEditorKind.Boolean,
                DefaultValue = false
            },

            new PropertyDescriptor(nameof(WallElement.Comments), target => ((WallElement)target).Comments,
                (target, value) => ((WallElement)target).Comments = value?.ToString() ?? string.Empty)
            {
                DisplayName = "Comments",
                Category = "Identity Data",
                Description = "Free text carried with the element, exactly as Revit does it."
            });

        _properties.Register<RoomElement>(
            new PropertyDescriptor(nameof(RoomElement.Number), target => ((RoomElement)target).Number)
            {
                DisplayName = "Number",
                Category = "Identity Data"
            },

            new PropertyDescriptor(nameof(RoomElement.Name), target => ((RoomElement)target).Name,
                (target, value) => ((RoomElement)target).Name = value?.ToString() ?? string.Empty)
            {
                DisplayName = "Name",
                Category = "Identity Data"
            },

            new PropertyDescriptor(nameof(RoomElement.Department), target => ((RoomElement)target).Department,
                (target, value) => ((RoomElement)target).Department = value?.ToString() ?? Departments[0])
            {
                DisplayName = "Department",
                Category = "Identity Data",
                Editor = PropertyEditorKind.Choice,
                Choices = Departments.Cast<object>().ToArray(),
                DefaultValue = Departments[0]
            },

            new PropertyDescriptor(nameof(RoomElement.Level), target => ((RoomElement)target).Level)
            {
                DisplayName = "Level",
                Category = "Constraints"
            },

            new PropertyDescriptor(nameof(RoomElement.Width), target => ((RoomElement)target).Width,
                (target, value) => ((RoomElement)target).Width = ToDouble(value))
            {
                DisplayName = "Width",
                Category = "Dimensions",
                Editor = PropertyEditorKind.Numeric,
                Unit = "mm",
                Format = "0",
                Validator = (_, value) => ToDouble(value) > 0 ? null : "Width must be positive."
            },

            new PropertyDescriptor(nameof(RoomElement.Depth), target => ((RoomElement)target).Depth,
                (target, value) => ((RoomElement)target).Depth = ToDouble(value))
            {
                DisplayName = "Depth",
                Category = "Dimensions",
                Editor = PropertyEditorKind.Numeric,
                Unit = "mm",
                Format = "0",
                Validator = (_, value) => ToDouble(value) > 0 ? null : "Depth must be positive."
            },

            new PropertyDescriptor(nameof(RoomElement.Area), target => ((RoomElement)target).Area)
            {
                DisplayName = "Area",
                Category = "Dimensions",
                Editor = PropertyEditorKind.Numeric,
                Unit = "m²",
                Format = "N2"
            });
    }

    /// <summary>The sheet the backstage shows on its Info page.</summary>
    public UIElement BuildProjectInfoPanel()
    {
        var panel = new StackPanel { Margin = new Thickness(32, 28, 32, 28) };

        var title = new TextBlock { Text = _model.ProjectName, FontSize = 24, FontWeight = FontWeights.SemiBold };
        title.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextPrimary");

        var subtitle = new TextBlock
        {
            Text = $"Project number {_model.ProjectNumber} — CONCOST Revit-Like sample",
            FontSize = 12,
            Margin = new Thickness(0, 4, 0, 22)
        };

        subtitle.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");

        panel.Children.Add(title);
        panel.Children.Add(subtitle);

        _infoLevels = InfoRow(panel, "Levels");
        _infoWalls = InfoRow(panel, "Walls");
        _infoRooms = InfoRow(panel, "Rooms");
        _infoArea = InfoRow(panel, "Total room area");

        var note = new TextBlock
        {
            Text = "The model is built in memory when the window opens. Every edit is undoable, including the " +
                   "ones made in the properties palette.",
            FontSize = 11,
            TextWrapping = TextWrapping.Wrap,
            MaxWidth = 460,
            Margin = new Thickness(0, 20, 0, 0)
        };

        note.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextTertiary");

        panel.Children.Add(note);

        UpdateProjectInfo();

        return panel;
    }

    private static TextBlock InfoRow(Panel host, string label)
    {
        var row = new Grid { Margin = new Thickness(0, 0, 0, 8) };
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(180) });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        var caption = new TextBlock { Text = label, FontSize = 12 };
        caption.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");

        var value = new TextBlock { Text = "—", FontSize = 12, FontWeight = FontWeights.SemiBold };
        value.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextPrimary");

        row.Children.Add(caption);
        Grid.SetColumn(value, 1);
        row.Children.Add(value);

        host.Children.Add(row);

        return value;
    }

    private void UpdateProjectInfo()
    {
        if (_infoLevels is not { } levels || _infoWalls is not { } walls ||
            _infoRooms is not { } rooms || _infoArea is not { } area)
        {
            return;
        }

        levels.Text = _model.Levels.Count.ToString(CultureInfo.CurrentCulture);
        walls.Text = _model.Levels.Sum(level => level.Walls.Count).ToString(CultureInfo.CurrentCulture);
        rooms.Text = _model.Levels.Sum(level => level.Rooms.Count).ToString(CultureInfo.CurrentCulture);
        area.Text = $"{_model.Levels.Sum(level => level.Rooms.Sum(room => room.Area)):N0} m²";
    }

    private void WriteRoomReport()
    {
        IEnumerable<LevelModel> levels = _active?.Level is { } level ? new[] { level } : _model.Levels;
        double total = 0;

        foreach (LevelModel step in levels)
        {
            if (step.Rooms.Count == 0) continue;

            _commandLine.WriteLine($"{step.Name} — {step.Rooms.Count} room(s)", CommandMessageKind.Hint);

            foreach (RoomElement room in step.Rooms)
            {
                total += room.Area;

                _commandLine.WriteLine($"  {room.Number}  {room.Name,-16}{room.Department,-12}{room.Area,8:N1} m²");
            }
        }

        _commandLine.WriteLine($"Total room area: {total:N1} m²", CommandMessageKind.Output);
    }

    private static double ToDouble(object? value)
    {
        return value switch
        {
            double number => number,
            float number => number,
            decimal number => (double)number,
            int number => number,
            long number => number,
            string text when double.TryParse(text, NumberStyles.Float, CultureInfo.CurrentCulture, out double parsed) => parsed,
            string text when double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out double parsed) => parsed,
            _ => 0
        };
    }
}
