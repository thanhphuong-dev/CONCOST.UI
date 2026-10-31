using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using CONCOST.UI.AttachedProperties;

namespace CONCOST.UI.Showcase;

/// <summary>
/// One entry of the "variants" list: a style the library ships for the selected control.
/// </summary>
public sealed class ShowcaseVariantInfo
{
    public string Name { get; init; } = string.Empty;
    public string StyleKey { get; init; } = string.Empty;
    public string Xaml { get; init; } = string.Empty;

    /// <summary>True for the hand-written variants declared in <see cref="ShowcaseItem.Variants"/>.</summary>
    public bool IsCurated { get; init; }
}

/// <summary>
/// One row of the "extended properties" list — either a dependency property declared by the
/// CONCOST control itself, or one of the library's attached properties that applies to it.
/// </summary>
public sealed class ShowcasePropertyInfo
{
    public string Name { get; init; } = string.Empty;
    public string TypeName { get; init; } = string.Empty;
    public string DefaultValue { get; init; } = string.Empty;
    public string Owner { get; init; } = string.Empty;
    public bool IsAttached { get; init; }
    public bool IsReadOnly { get; init; }
}

/// <summary>Everything the right-hand inspector shows for one catalog entry.</summary>
public sealed class ShowcaseInspectorDescription
{
    public Type? ControlType { get; init; }
    public string TypeLabel { get; init; } = string.Empty;
    public IReadOnlyList<ShowcaseVariantInfo> Variants { get; init; } = Array.Empty<ShowcaseVariantInfo>();
    public IReadOnlyList<ShowcasePropertyInfo> Properties { get; init; } = Array.Empty<ShowcasePropertyInfo>();
}

/// <summary>
/// Reads the inspector data straight from the library instead of a hand-maintained table:
/// variants come from the <c>CONCOST.&lt;Control&gt;.&lt;Variant&gt;</c> style keys in the merged theme
/// dictionaries, extended properties from reflection over the control type and the attached
/// property classes. Adding a style key or a dependency property therefore shows up in the
/// showcase with no extra bookkeeping.
/// </summary>
public static class ShowcaseInspector
{
    private static readonly Assembly LibraryAssembly = typeof(CONCOST.UI.Controls.ToggleSwitch).Assembly;
    private static readonly Assembly AttachedAssembly = typeof(BorderElement).Assembly;

    private static readonly object Gate = new();
    private static readonly Dictionary<ShowcaseItem, ShowcaseInspectorDescription> Cache = new();

    private static Dictionary<string, Style>? _styles;
    private static Dictionary<string, Type>? _libraryTypes;
    private static List<Type>? _attachedOwners;

    /// <summary>Attached property classes that make sense on any control, not just one family.</summary>
    private static readonly HashSet<string> UniversalOwners = new(StringComparer.Ordinal)
    {
        "BorderElement",
        "InfoElement",
        "IconElement",
        "VisualElement",
        "DragDropHelper"
    };

    public static ShowcaseInspectorDescription Describe(ShowcaseItem item)
    {
        lock (Gate)
        {
            if (Cache.TryGetValue(item, out var cached))
                return cached;

            var description = Build(item);
            Cache[item] = description;
            return description;
        }
    }

    private static ShowcaseInspectorDescription Build(ShowcaseItem item)
    {
        var type = ResolveControlType(item.Name);

        // Guide pages and effect entries are not bound to a control: nothing to inspect.
        if (type is null)
            return new ShowcaseInspectorDescription { TypeLabel = string.Empty };

        return new ShowcaseInspectorDescription
        {
            ControlType = type,
            TypeLabel = TypeLabel(type),
            Variants = BuildVariants(item, type),
            Properties = BuildProperties(type)
        };
    }

    // ------------------------------------------------------------------ control type

    private static Type? ResolveControlType(string name)
    {
        foreach (var candidate in CandidateNames(name))
        {
            if (LibraryTypes.TryGetValue(candidate, out var libraryType))
                return libraryType;

            var framework = FrameworkType(candidate);
            if (framework is not null)
                return framework;
        }

        return null;
    }

    /// <summary>
    /// "Tag / Chip" → "Tag", "StatisticCard (KPI)" → "StatisticCard", "Charts Suite" → "Charts".
    /// </summary>
    private static IEnumerable<string> CandidateNames(string name)
    {
        yield return name;

        var slash = name.IndexOf('/');
        if (slash > 0)
            yield return name[..slash].Trim();

        var paren = name.IndexOf('(');
        if (paren > 0)
            yield return name[..paren].Trim();

        if (name.EndsWith(" Suite", StringComparison.OrdinalIgnoreCase))
            yield return name[..^" Suite".Length].Trim();

        var compact = name.Replace(" ", string.Empty);
        if (!string.Equals(compact, name, StringComparison.Ordinal))
            yield return compact;
    }

    private static Dictionary<string, Type> LibraryTypes
    {
        get
        {
            if (_libraryTypes is not null)
                return _libraryTypes;

            var map = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);

            try
            {
                foreach (var type in LibraryAssembly.GetTypes())
                {
                    if (!type.IsPublic || type.IsAbstract || type.IsGenericTypeDefinition)
                        continue;

                    map.TryAdd(type.Name, type);
                }
            }
            catch (ReflectionTypeLoadException)
            {
                // A partially loadable assembly still yields the types we need.
            }

            return _libraryTypes = map;
        }
    }

    private static Type? FrameworkType(string name)
    {
        string[] hosts =
        [
            "System.Windows.Controls.Primitives",
            "System.Windows.Controls",
            "System.Windows.Documents",
            "System.Windows.Shapes",
            "System.Windows"
        ];

        foreach (var host in hosts)
        {
            var type = Type.GetType($"{host}.{name}, PresentationFramework", throwOnError: false);
            if (type is not null)
                return type;
        }

        return null;
    }

    private static string TypeLabel(Type? type)
    {
        if (type is null)
            return string.Empty;

        var ns = type.Namespace ?? string.Empty;
        return ns.StartsWith("CONCOST.", StringComparison.Ordinal)
            ? $"{ns}.{type.Name}"
            : type.FullName ?? type.Name;
    }

    // ------------------------------------------------------------------ variants

    private static IReadOnlyList<ShowcaseVariantInfo> BuildVariants(ShowcaseItem item, Type? type)
    {
        var result = new List<ShowcaseVariantInfo>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // 1. The hand-written variants win: they carry a realistic Content.
        foreach (var variant in item.Variants)
        {
            if (!seen.Add(variant.Name))
                continue;

            result.Add(new ShowcaseVariantInfo
            {
                Name = variant.Name,
                Xaml = variant.XamlCode ?? string.Empty,
                IsCurated = true
            });
        }

        // 2. Every CONCOST.<Control>.<Variant> style key the theme actually ships.
        var token = type?.Name ?? CandidateNames(item.Name).First();
        var prefix = $"CONCOST.{token}.";

        var fromStyles = Styles
            .Where(pair => pair.Key.StartsWith(prefix, StringComparison.Ordinal))
            .Where(pair => MatchesTargetType(pair.Value, type))
            .OrderBy(pair => pair.Key, StringComparer.Ordinal)
            .Select(pair => new ShowcaseVariantInfo
            {
                Name = pair.Key[prefix.Length..],
                StyleKey = pair.Key,
                Xaml = $"<{token} Style=\"{{StaticResource {pair.Key}}}\" />"
            });

        foreach (var variant in fromStyles)
        {
            if (!seen.Add(variant.Name))
                continue;

            result.Add(variant);
        }

        // "Base" is the control's own look — list it first.
        var baseIndex = result.FindIndex(v => v.Name.Equals("Base", StringComparison.OrdinalIgnoreCase));
        if (baseIndex > 0)
        {
            var baseVariant = result[baseIndex];
            result.RemoveAt(baseIndex);
            result.Insert(0, baseVariant);
        }

        return result;
    }

    private static bool MatchesTargetType(Style style, Type? control)
    {
        if (control is null || style.TargetType is null)
            return true;

        return style.TargetType.IsAssignableFrom(control) || control.IsAssignableFrom(style.TargetType);
    }

    /// <summary>Every keyed style reachable from the application resources, merged dictionaries included.</summary>
    private static Dictionary<string, Style> Styles
    {
        get
        {
            if (_styles is not null)
                return _styles;

            var map = new Dictionary<string, Style>(StringComparer.Ordinal);
            Collect(Application.Current?.Resources, map);
            return _styles = map;
        }
    }

    private static void Collect(ResourceDictionary? dictionary, Dictionary<string, Style> map)
    {
        if (dictionary is null)
            return;

        foreach (var merged in dictionary.MergedDictionaries)
            Collect(merged, map);

        foreach (var key in dictionary.Keys)
        {
            if (key is not string name || !name.StartsWith("CONCOST.", StringComparison.Ordinal))
                continue;

            if (map.ContainsKey(name))
                continue;

            try
            {
                if (dictionary[name] is Style style)
                    map[name] = style;
            }
            catch (Exception)
            {
                // A single broken style must not take the whole inspector — and the app — down.
            }
        }
    }

    // ------------------------------------------------------------------ extended properties

    private static IReadOnlyList<ShowcasePropertyInfo> BuildProperties(Type? type)
    {
        var result = new List<ShowcasePropertyInfo>();
        var seen = new HashSet<string>(StringComparer.Ordinal);

        // 1. Dependency properties declared by the CONCOST control (and its CONCOST base types).
        for (var current = type; current is not null && current.Assembly == LibraryAssembly; current = current.BaseType)
        {
            foreach (var field in current.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly))
            {
                if (field.FieldType != typeof(DependencyProperty))
                    continue;

                if (field.GetValue(null) is not DependencyProperty property)
                    continue;

                if (!seen.Add(property.Name))
                    continue;

                result.Add(Describe(property, owner: current.Name, attached: false));
            }
        }

        // 2. Attached properties the library adds on top of the control.
        foreach (var owner in AttachedOwners)
        {
            if (!AppliesTo(owner, type))
                continue;

            foreach (var field in owner.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly))
            {
                if (field.FieldType != typeof(DependencyProperty))
                    continue;

                if (field.GetValue(null) is not DependencyProperty property || property.OwnerType != owner)
                    continue;

                if (!seen.Add(property.Name))
                    continue;

                result.Add(Describe(property, owner.Name, attached: true));
            }
        }

        return result;
    }

    private static ShowcasePropertyInfo Describe(DependencyProperty property, string owner, bool attached) =>
        new()
        {
            Name = property.Name,
            TypeName = Friendly(property.PropertyType),
            DefaultValue = FormatDefault(property.DefaultMetadata?.DefaultValue),
            Owner = owner,
            IsAttached = attached,
            IsReadOnly = property.ReadOnly
        };

    private static bool AppliesTo(Type owner, Type? control)
    {
        if (UniversalOwners.Contains(owner.Name))
            return true;

        if (control is null)
            return false;

        // TextBoxHelper → TextBox, DataGridElement → DataGrid, ProgressBarElement → ProgressBar …
        var stem = owner.Name
            .Replace("Element", string.Empty)
            .Replace("Helper", string.Empty);

        if (stem.Length == 0)
            return false;

        return stem.Equals(control.Name, StringComparison.OrdinalIgnoreCase)
            || control.Name.StartsWith(stem, StringComparison.OrdinalIgnoreCase);
    }

    private static List<Type> AttachedOwners
    {
        get
        {
            if (_attachedOwners is not null)
                return _attachedOwners;

            var owners = new List<Type>();

            try
            {
                owners.AddRange(AttachedAssembly.GetTypes()
                    .Where(t => t.IsAbstract && t.IsSealed && t.IsPublic)
                    .Where(t => t.Namespace is not null && t.Namespace.StartsWith("CONCOST.UI.AttachedProperties", StringComparison.Ordinal))
                    .OrderBy(t => t.Name, StringComparer.Ordinal));
            }
            catch (ReflectionTypeLoadException)
            {
                // Same as above: keep whatever loaded.
            }

            return _attachedOwners = owners;
        }
    }

    // ------------------------------------------------------------------ formatting

    private static string Friendly(Type type)
    {
        if (!type.IsGenericType)
            return Alias(type.Name);

        var name = type.Name[..type.Name.IndexOf('`')];
        var arguments = string.Join(", ", type.GetGenericArguments().Select(Friendly));
        return $"{name}<{arguments}>";
    }

    private static string Alias(string name) => name switch
    {
        "Boolean" => "bool",
        "Int32" => "int",
        "Double" => "double",
        "String" => "string",
        "Object" => "object",
        _ => name
    };

    private static string FormatDefault(object? value)
    {
        var text = value switch
        {
            null => "null",
            string s => s.Length == 0 ? "\"\"" : $"\"{s}\"",
            bool b => b ? "true" : "false",
            Enum e => e.ToString(),
            System.Windows.Media.Brush brush => brush is System.Windows.Media.SolidColorBrush solid
                ? solid.Color.ToString()
                : "brush",
            System.Windows.Thickness t => $"{t.Left},{t.Top},{t.Right},{t.Bottom}",
            System.Windows.CornerRadius c => $"{c.TopLeft},{c.TopRight},{c.BottomRight},{c.BottomLeft}",
            double d when d == double.MaxValue => "double.MaxValue",
            double d when d == double.MinValue => "-double.MaxValue",
            double d => d.ToString("0.############", System.Globalization.CultureInfo.InvariantCulture),
            IFormattable formattable => formattable.ToString(null, System.Globalization.CultureInfo.InvariantCulture),
            _ => value.ToString() ?? "null"
        };

        return text.Length <= 34 ? text : text[..31] + "…";
    }
}
