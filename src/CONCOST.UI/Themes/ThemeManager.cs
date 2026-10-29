using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Runtime.CompilerServices;
using CONCOST.UI.Core.Helpers;
using CONCOST.UI.Core.MVVM;
using CONCOST.UI.Themes.Tools;
using Microsoft.Win32;

namespace CONCOST.UI.Themes;

public enum AppTheme
{
    Light,
    Dark,
    System
}

public enum ThemePreset
{
    Default,
    Ocean,
    Forest,
    Sunset,
    Slate,
    Midnight,
    HighContrast,
    Brutal
}

public enum SurfaceStyle
{
    Flat,
    Elevated,
    Glass,
    Soft,
    Bordered
}

public class ThemeManager : ObservableObject
{
    private static readonly Lazy<ThemeManager> _current = new(() => new ThemeManager());
    private static readonly ConditionalWeakTable<FrameworkElement, ScopedThemeResources> ScopedResources = new();
    public static ThemeManager Current => _current.Value;

    private AppTheme _theme = AppTheme.Light;
    private ThemePreset _preset = ThemePreset.Default;
    private SurfaceStyle _surfaceStyle = SurfaceStyle.Flat;
    private Color _accentColor = ColorHelper.FromHex("#FF7900");

    public AppTheme Theme
    {
        get => _theme;
        set
        {
            if (SetProperty(ref _theme, value))
            {
                ApplyTheme(value);
            }
        }
    }

    public ThemePreset Preset
    {
        get => _preset;
        set
        {
            if (SetProperty(ref _preset, value))
            {
                ApplyPreset(value);
            }
        }
    }

    public SurfaceStyle SurfaceStyle
    {
        get => _surfaceStyle;
        set
        {
            if (SetProperty(ref _surfaceStyle, value))
            {
                ApplySurfaceStyle(value);
            }
        }
    }

    public Color AccentColor
    {
        get => _accentColor;
        set
        {
            if (SetProperty(ref _accentColor, value))
            {
                ApplyAccentColor(value);
            }
        }
    }

    public void ApplyPreset(ThemePreset preset)
    {
        if (_preset != preset)
        {
            _preset = preset;
            OnPropertyChanged(nameof(Preset));
        }

        var (color, dark) = GetPreset(preset);

        if (preset == ThemePreset.Midnight || preset == ThemePreset.HighContrast)
        {
            Theme = AppTheme.Dark;
        }

        AccentColor = color;
        ApplyBrutalOverrides(preset == ThemePreset.Brutal);
    }

    private static readonly (string Key, string Hex)[] BrutalBrushes =
    {
        ("CONCOST.Brush.Background", "#FFFDF5"),
        ("CONCOST.Brush.Surface", "#FFFDF5"),
        ("CONCOST.Brush.SurfaceSecondary", "#F3F0E6"),
        ("CONCOST.Brush.Border", "#111111"),
        ("CONCOST.Brush.BorderSecondary", "#111111"),
        ("CONCOST.Brush.TextPrimary", "#111111"),
        ("CONCOST.Brush.TextSecondary", "#3D3D3D")
    };

    private static readonly Dictionary<string, object?> BrutalSnapshot = new();
    private static bool _brutalApplied;

    /// <summary>
    /// Neo-brutalist preset: flat cream surfaces with ink-black outlines. The theme brushes
    /// are snapshotted the first time so switching back to another preset restores them.
    /// </summary>
    private static void ApplyBrutalOverrides(bool enabled)
    {
        var app = Application.Current;
        if (app == null) return;

        if (enabled)
        {
            if (!_brutalApplied)
            {
                BrutalSnapshot.Clear();
                foreach (var (key, _) in BrutalBrushes)
                    BrutalSnapshot[key] = app.Resources.Contains(key) ? app.Resources[key] : null;
                _brutalApplied = true;
            }

            foreach (var (key, hex) in BrutalBrushes)
                app.Resources[key] = ColorHelper.CreateBrush(ColorHelper.FromHex(hex));

            return;
        }

        if (!_brutalApplied) return;

        foreach (var (key, _) in BrutalBrushes)
        {
            if (BrutalSnapshot.TryGetValue(key, out object? original) && original != null)
                app.Resources[key] = original;
            else
                app.Resources.Remove(key);
        }

        BrutalSnapshot.Clear();
        _brutalApplied = false;
    }

    public void ApplySurfaceStyle(SurfaceStyle style)
    {
        var app = Application.Current;
        if (app == null) return;

        double shadowDepth = style switch
        {
            SurfaceStyle.Elevated => 4.0,
            SurfaceStyle.Glass => 2.0,
            SurfaceStyle.Soft => 6.0,
            _ => 1.0
        };

        app.Resources["CONCOST.Shadow.Depth"] = shadowDepth;
    }

    public void ApplyTheme(AppTheme theme)
    {
        var app = Application.Current;
        if (app == null) return;

        var effectiveTheme = theme;
        if (effectiveTheme == AppTheme.System)
        {
            effectiveTheme = GetSystemTheme();
        }

        string themeUri = effectiveTheme == AppTheme.Dark
            ? "pack://application:,,,/CONCOST.UI;component/Themes/Dark.xaml"
            : "pack://application:,,,/CONCOST.UI;component/Themes/Light.xaml";

        var newDict = new ResourceDictionary { Source = new Uri(themeUri, UriKind.Absolute) };

        var merged = app.Resources.MergedDictionaries;
        var existing = merged.FirstOrDefault(d => d.Source != null && (d.Source.OriginalString.Contains("/Themes/Dark.xaml") || d.Source.OriginalString.Contains("/Themes/Light.xaml")));

        if (existing != null)
        {
            int index = merged.IndexOf(existing);
            merged[index] = newDict;
        }
        else
        {
            merged.Add(newDict);
        }

        ApplyAccentColor(_accentColor);
    }

    /// <summary>
    /// Apply theme and resources directly to a root FrameworkElement when the host owns Application.Current.
    /// </summary>
    public void ApplyTheme(FrameworkElement rootElement, AppTheme theme)
    {
        if (rootElement == null) return;
        if (!rootElement.Dispatcher.CheckAccess())
            throw new InvalidOperationException("Apply the CONCOST.UI theme on the root element's dispatcher thread.");

        var effectiveTheme = theme == AppTheme.System ? GetSystemTheme() : theme;
        var scope = ScopedResources.GetValue(rootElement, CreateScopedResources);
        EnsureScopedResources(rootElement, scope);
        ReplaceThemeDictionary(scope.ThemeResources, effectiveTheme);
        RefreshScopedResources(rootElement, scope);
        ApplyAccentColor(rootElement, _accentColor);
        ApplySurfaceStyle(rootElement, _surfaceStyle);
    }

    /// <summary>Applies surface styling within the supplied plugin or view root.</summary>
    public void ApplySurfaceStyle(FrameworkElement rootElement, SurfaceStyle style)
    {
        if (rootElement == null) return;
        if (!rootElement.Dispatcher.CheckAccess())
            throw new InvalidOperationException("Apply CONCOST.UI resources on the root element's dispatcher thread.");

        double shadowDepth = style switch
        {
            SurfaceStyle.Elevated => 4.0,
            SurfaceStyle.Glass => 2.0,
            SurfaceStyle.Soft => 6.0,
            _ => 1.0
        };
        rootElement.Resources["CONCOST.Shadow.Depth"] = shadowDepth;
    }

    private sealed class ScopedThemeResources
    {
        public ResourceDictionary Container { get; } = new();
        public ResourceDictionary ThemeResources { get; } = new()
        {
            Source = new Uri("pack://application:,,,/CONCOST.UI;component/Themes/ThemeResources.xaml", UriKind.Absolute)
        };
    }

    private static ScopedThemeResources CreateScopedResources(FrameworkElement _) => new();

    private static void EnsureScopedResources(FrameworkElement rootElement, ScopedThemeResources scope)
    {
        if (scope.Container.MergedDictionaries.Count == 0)
        {
            scope.Container.MergedDictionaries.Add(scope.ThemeResources);
            scope.Container.MergedDictionaries.Add(new ResourceDictionary
            {
                Source = new Uri("pack://application:,,,/CONCOST.UI;component/Themes/Generic.xaml", UriKind.Absolute)
            });
        }

        if (!rootElement.Resources.MergedDictionaries.Contains(scope.Container))
            rootElement.Resources.MergedDictionaries.Add(scope.Container);
    }

    private static void RefreshScopedResources(FrameworkElement rootElement, ScopedThemeResources scope)
    {
        var dictionaries = rootElement.Resources.MergedDictionaries;
        int index = dictionaries.IndexOf(scope.Container);
        if (index < 0)
        {
            dictionaries.Add(scope.Container);
            return;
        }

        // Replacing a nested theme dictionary does not reliably notify existing DynamicResource
        // expressions in every host. Reinsert the same scoped container in-place so WPF reevaluates
        // resources while preserving the host's merged-dictionary ordering.
        dictionaries.RemoveAt(index);
        dictionaries.Insert(index, scope.Container);
    }

    private static void ReplaceThemeDictionary(ResourceDictionary themeResources, AppTheme theme)
    {
        string themeName = theme == AppTheme.Dark ? "Dark.xaml" : "Light.xaml";
        string themeUri = $"pack://application:,,,/CONCOST.UI;component/Themes/{themeName}";
        var dictionaries = themeResources.MergedDictionaries;
        var current = dictionaries.FirstOrDefault(dictionary =>
            dictionary.Source?.OriginalString.Contains("/Themes/Dark.xaml", StringComparison.OrdinalIgnoreCase) == true ||
            dictionary.Source?.OriginalString.Contains("/Themes/Light.xaml", StringComparison.OrdinalIgnoreCase) == true);
        var replacement = new ResourceDictionary { Source = new Uri(themeUri, UriKind.Absolute) };

        if (current == null)
            dictionaries.Add(replacement);
        else
            dictionaries[dictionaries.IndexOf(current)] = replacement;
    }

    private static (Color Color, bool Dark) GetPreset(ThemePreset preset) => preset switch
    {
        ThemePreset.Ocean => (ColorHelper.FromHex("#0284C7"), false),
        ThemePreset.Forest => (ColorHelper.FromHex("#16A34A"), false),
        ThemePreset.Sunset => (ColorHelper.FromHex("#EA580C"), false),
        ThemePreset.Slate => (ColorHelper.FromHex("#475569"), false),
        ThemePreset.Midnight => (ColorHelper.FromHex("#8B5CF6"), true),
        ThemePreset.HighContrast => (ColorHelper.FromHex("#FFFF00"), true),
        ThemePreset.Brutal => (ColorHelper.FromHex("#FFE14D"), false),
        _ => (ColorHelper.FromHex("#FF7900"), false)
    };

    public void ApplyAccentColor(FrameworkElement rootElement, Color color)
    {
        if (rootElement == null) return;
        if (!rootElement.Dispatcher.CheckAccess())
            throw new InvalidOperationException("Apply CONCOST.UI resources on the root element's dispatcher thread.");

        var palette = AccentPaletteGenerator.Generate(color);
        rootElement.Resources["CONCOST.Color.Primary"] = palette.Primary;
        rootElement.Resources["CONCOST.Brush.Primary"] = ColorHelper.CreateBrush(palette.Primary);
        rootElement.Resources["CONCOST.Brush.PrimaryHover"] = ColorHelper.CreateBrush(palette.PrimaryHover);
        rootElement.Resources["CONCOST.Brush.PrimaryPressed"] = ColorHelper.CreateBrush(palette.PrimaryPressed);
        rootElement.Resources["CONCOST.Brush.PrimaryLight"] = ColorHelper.CreateBrush(palette.PrimaryLight);
        rootElement.Resources["CONCOST.Brush.PrimaryDark"] = ColorHelper.CreateBrush(palette.PrimaryDark);
        rootElement.Resources["CONCOST.Brush.BorderFocused"] = ColorHelper.CreateBrush(palette.Primary);
    }

    public void ApplyAccentColor(Color color)
    {
        var app = Application.Current;
        if (app == null) return;

        var palette = AccentPaletteGenerator.Generate(color);

        app.Resources["CONCOST.Color.Primary"] = palette.Primary;
        app.Resources["CONCOST.Brush.Primary"] = ColorHelper.CreateBrush(palette.Primary);
        app.Resources["CONCOST.Brush.PrimaryHover"] = ColorHelper.CreateBrush(palette.PrimaryHover);
        app.Resources["CONCOST.Brush.PrimaryPressed"] = ColorHelper.CreateBrush(palette.PrimaryPressed);
        app.Resources["CONCOST.Brush.PrimaryLight"] = ColorHelper.CreateBrush(palette.PrimaryLight);
        app.Resources["CONCOST.Brush.PrimaryDark"] = ColorHelper.CreateBrush(palette.PrimaryDark);
        app.Resources["CONCOST.Brush.BorderFocused"] = ColorHelper.CreateBrush(palette.Primary);
    }

    private static AppTheme GetSystemTheme()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
            var value = key?.GetValue("AppsUseLightTheme");
            if (value is int intValue && intValue == 0)
            {
                return AppTheme.Dark;
            }
        }
        catch
        {
        }
        return AppTheme.Light;
    }
}
