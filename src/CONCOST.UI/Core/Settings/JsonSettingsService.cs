using System.IO;
using System.Text.Json;

namespace CONCOST.UI.Core.Settings;

public interface ISettingsService
{
    T? Get<T>(string key, T? defaultValue = default);
    void Set<T>(string key, T value);
    void Save();
    void Load();
}

public class JsonSettingsService : ISettingsService
{
    private static readonly Lazy<JsonSettingsService> _current = new(() => new JsonSettingsService());
    public static JsonSettingsService Current => _current.Value;

    private readonly string _filePath;
    private Dictionary<string, JsonElement> _data = new();

    public JsonSettingsService(string? appName = "CONCOST")
    {
        string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        string folder = Path.Combine(appData, appName ?? "CONCOST");
        Directory.CreateDirectory(folder);
        _filePath = Path.Combine(folder, "settings.json");
        Load();
    }

    public T? Get<T>(string key, T? defaultValue = default)
    {
        if (_data.TryGetValue(key, out var element))
        {
            try
            {
                return JsonSerializer.Deserialize<T>(element.GetRawText());
            }
            catch
            {
                return defaultValue;
            }
        }
        return defaultValue;
    }

    public void Set<T>(string key, T value)
    {
        var json = JsonSerializer.Serialize(value);
        using var doc = JsonDocument.Parse(json);
        _data[key] = doc.RootElement.Clone();
        Save();
    }

    public void Save()
    {
        try
        {
            var json = JsonSerializer.Serialize(_data, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_filePath, json);
        }
        catch
        {
            // Ignore file write exceptions in restricted environments
        }
    }

    public void Load()
    {
        try
        {
            if (File.Exists(_filePath))
            {
                var json = File.ReadAllText(_filePath);
                _data = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json) ?? new();
            }
        }
        catch
        {
            _data = new();
        }
    }
}
