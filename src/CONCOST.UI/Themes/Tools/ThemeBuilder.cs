using System.Text.Json;
using System.Windows.Media;
using CONCOST.UI.Core.Helpers;

namespace CONCOST.UI.Themes.Tools;

public class CustomThemeDefinition
{
    public string Name { get; set; } = "CustomTheme";
    public string PrimaryHex { get; set; } = "#FF7900";
    public string BackgroundHex { get; set; } = "#FFFFFF";
    public string SurfaceHex { get; set; } = "#F8FAFC";
    public string TextHex { get; set; } = "#0F172A";
    public double CornerRadius { get; set; } = 6.0;
    public string SurfaceStyle { get; set; } = "Flat";
}

public class ThemeBuilder
{
    private readonly CustomThemeDefinition _def = new();

    public static ThemeBuilder Create() => new();

    public ThemeBuilder WithName(string name)
    {
        _def.Name = name;
        return this;
    }

    public ThemeBuilder WithPrimary(Color color)
    {
        _def.PrimaryHex = $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";
        return this;
    }

    public ThemeBuilder WithCornerRadius(double radius)
    {
        _def.CornerRadius = radius;
        return this;
    }

    public ThemeBuilder WithSurfaceStyle(string style)
    {
        _def.SurfaceStyle = style;
        return this;
    }

    public CustomThemeDefinition Build() => _def;

    public string ToJson() => JsonSerializer.Serialize(_def, new JsonSerializerOptions { WriteIndented = true });

    public static CustomThemeDefinition? FromJson(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<CustomThemeDefinition>(json);
        }
        catch
        {
            return null;
        }
    }
}
