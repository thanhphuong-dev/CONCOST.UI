using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CONCOST.UI.Controls.Ribbon;

/// <summary>
/// Turns a <see cref="RibbonState"/> into JSON and back, and optionally reads/writes a file.
/// <para>
/// The library does not decide where the state lives — a Revit add-in usually writes next to the
/// .rvt or under <c>%APPDATA%</c>, a standalone app may use its own settings store. Call
/// <see cref="Save"/> with the path you chose, or use <see cref="ToJson"/> and store the string
/// wherever you like.
/// </para>
/// </summary>
public static class RibbonStateStore
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>Serialises the state to indented JSON.</summary>
    public static string ToJson(RibbonState state)
    {
        if (state == null) throw new ArgumentNullException(nameof(state));
        return JsonSerializer.Serialize(state, Options);
    }

    /// <summary>Reads a state back. Returns null for empty or malformed JSON instead of throwing.</summary>
    public static RibbonState? FromJson(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;

        try
        {
            return JsonSerializer.Deserialize<RibbonState>(json!, Options);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>Writes the state to <paramref name="path"/>, creating the folder when needed.</summary>
    public static void Save(RibbonState state, string path)
    {
        if (state == null) throw new ArgumentNullException(nameof(state));
        if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("A file path is required.", nameof(path));

        string? folder = Path.GetDirectoryName(Path.GetFullPath(path));
        if (!string.IsNullOrEmpty(folder)) Directory.CreateDirectory(folder);

        File.WriteAllText(path, ToJson(state));
    }

    /// <summary>Reads the state from <paramref name="path"/>. Returns null when the file is missing or broken.</summary>
    public static RibbonState? Load(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path)) return null;

        try
        {
            return FromJson(File.ReadAllText(path));
        }
        catch (IOException)
        {
            return null;
        }
        catch (UnauthorizedAccessException)
        {
            return null;
        }
    }
}
