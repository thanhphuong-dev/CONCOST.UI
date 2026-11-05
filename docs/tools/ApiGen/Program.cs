// ApiGen — the API reference generator.
//
// Reflects over the built CONCOST.UI.dll, joins it with the XML documentation file the build
// emits, and writes docs/api-reference.md. Reflection (not regex over the sources) is what keeps
// the numbers honest: a type appears once however many partial files declare it, and the member
// lists are the ones the compiler actually produced.
//
// Usage: dotnet run --project docs/tools/ApiGen [-- --check]

using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Xml.Linq;

var root = FindRepoRoot();
var assemblyPath = Path.Combine(root, "src", "CONCOST.UI", "bin", "Debug", "net8.0-windows", "CONCOST.UI.dll");
var xmlPath = Path.ChangeExtension(assemblyPath, ".xml");
var outPath = Path.Combine(root, "docs", "api-reference.md");

var assembly = Assembly.LoadFrom(assemblyPath);

if (File.Exists(xmlPath))
{
    Program.Docs = XDocument.Load(xmlPath).Root!.Elements("members").First()
        .Elements("member")
        .ToDictionary(m => m.Attribute("name")!.Value, m => (m.Element("summary")?.Value ?? "").Trim());
}
else
{
    Console.Error.WriteLine($"warning: no XML documentation at {xmlPath} — build the library first.");
}

var types = assembly.GetExportedTypes()
    .Where(t => !t.IsCompilerGenerated())
    .OrderBy(t => t.FullName, StringComparer.Ordinal)
    .ToList();

var lines = new List<string>
{
    "# CONCOST.UI — API Reference",
    "",
    "Generated from the built assembly by `docs/tools/ApiGen` — do not edit by hand.",
    "",
    $"**{types.Count} public types** · " +
    $"{types.Count(t => t.IsClass)} classes · {types.Count(t => t.IsEnum)} enums · " +
    $"{types.Count(t => t.IsValueType && !t.IsEnum)} structs · {types.Count(t => t.IsInterface)} interfaces.",
    "",
    "Dependency properties, attached properties and routed events are the XAML surface; the",
    "CLR events and public methods are the code-behind surface. Summaries come from the XML",
    "documentation the build emits next to the assembly.",
    "",
    "## Contents",
    "",
};

var groups = types.GroupBy(t => t.Namespace ?? "CONCOST.UI").OrderBy(g => g.Key, StringComparer.Ordinal).ToList();

foreach (var group in groups)
    lines.Add($"- [{group.Key}](#{Slug(group.Key)}) — {group.Count()} types");

lines.Add("");
lines.Add("---");
lines.Add("");

var missingSummaries = new List<string>();

foreach (var group in groups)
{
    lines.Add($"## {group.Key}");
    lines.Add("");

    foreach (var type in group)
    {
        var kind = TypeKind(type);
        var summary = GetSummary($"T:{type.FullName}");
        if (summary.Length == 0) missingSummaries.Add($"T:{type.FullName}");

        lines.Add($"### {type.Name}{kind}");
        lines.Add("");

        if (summary.Length > 0) lines.Add(summary);

        if (type.BaseType is { } baseType && baseType.FullName is not null &&
            baseType.FullName != "System.Object")
        {
            lines.Add("");
            lines.Add($"Inherits `{baseType.FullName}`.");
        }

        var dependencies = new List<string>();
        var readOnly = new List<string>();
        var routed = new List<string>();

        foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly))
        {
            if (field.FieldType == typeof(DependencyProperty))
            {
                var name = field.Name.EndsWith("Property") ? field.Name[..^"Property".Length] : field.Name;
                dependencies.Add(name);
            }
            else if (field.FieldType == typeof(DependencyPropertyKey))
            {
                var name = field.Name.EndsWith("PropertyKey") ? field.Name[..^"PropertyKey".Length] : field.Name;
                readOnly.Add(name);
            }
            else if (field.FieldType == typeof(RoutedEvent))
            {
                var name = field.Name.EndsWith("Event") ? field.Name[..^"Event".Length] : field.Name;
                routed.Add(name);
            }
        }

        var clrEvents = type.GetEvents(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly);
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(p => p.GetIndexParameters().Length == 0)
            .Where(p => !readOnly.Contains(p.Name))
            .Where(p => dependencies.All(d => d != p.Name))
            .Where(p => p.DeclaringType == type)
            .ToList();
        var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(m => !m.IsSpecialName)
            .Where(m => m.DeclaringType == type)
            .ToList();

        // An attached property has Get/Set accessors taking a DependencyObject last; an instance
        // property of the same name would also be in the property list, so those are not attached.
        var attached = type.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(m => m.Name.StartsWith("Get") || m.Name.StartsWith("Set"))
            .Where(m => m.GetParameters() is { Length: > 0 } ps && typeof(DependencyObject).IsAssignableFrom(ps[^1].ParameterType))
            .Select(m => m.Name[3..])
            .Where(n => properties.All(p => p.Name != n))
            .Distinct()
            .OrderBy(n => n, StringComparer.Ordinal)
            .ToList();

        var bullets = new List<string>();

        if (dependencies.Count > 0)
        {
            var described = dependencies.Select(d => $"`{d}`{DocInline($"P:{type.FullName}.{d}")}");
            bullets.Add($"- **Dependency properties ({dependencies.Count}):** " + string.Join(", ", described));
        }

        if (readOnly.Count > 0)
            bullets.Add($"- **Read-only properties:** " + string.Join(", ", readOnly.Select(r => $"`{r}`")));

        if (attached.Count > 0)
            bullets.Add($"- **Attached properties:** " + string.Join(", ", attached.Select(a => $"`{a}`")));

        if (routed.Count > 0)
        {
            var described = routed.Select(r => $"`{r}`{DocInline($"E:{type.FullName}.{r}")}");
            bullets.Add($"- **Routed events:** " + string.Join(", ", described));
        }

        if (clrEvents.Length > 0)
        {
            var described = clrEvents.Select(e => $"`{e.Name}`{DocInline($"E:{type.FullName}.{e.Name}")}");
            bullets.Add($"- **CLR events:** " + string.Join(", ", described));
        }

        if (properties.Count > 0)
        {
            var described = properties.Select(p =>
            {
                var inline = DocInline($"P:{type.FullName}.{p.Name}");
                return $"`{p.Name}: {Short(p.PropertyType)}`" + inline;
            });
            bullets.Add($"- **Properties ({properties.Count}):** " + string.Join(", ", described));
        }

        if (methods.Count > 0)
        {
            var described = methods.Select(m =>
            {
                var parameters = string.Join(", ", m.GetParameters().Select(pa => $"{Short(pa.ParameterType)} {pa.Name}"));
                var inline = DocInline($"M:{type.FullName}.{m.Name}");
                return $"`{m.Name}({parameters})`" + inline;
            });
            bullets.Add($"- **Methods ({methods.Count}):** " + string.Join(" · ", described));
        }

        lines.AddRange(bullets);

        if (bullets.Count == 0 && summary.Length == 0)
            lines.Add("*No public members.*");

        lines.Add("");
    }
}

File.WriteAllLines(outPath, lines, new UTF8Encoding(false));

Console.WriteLine($"api-reference.md: {types.Count} types, {types.Sum(CountMembers)} members listed.");

if (args.Contains("--check"))
{
    Console.WriteLine($"public types missing XML summaries: {missingSummaries.Count}");
    foreach (var entry in missingSummaries.Take(40)) Console.WriteLine("  - " + entry);
}

return 0;

// ------------------------------------------------------------------ helpers

static string FindRepoRoot()
{
    var directory = new DirectoryInfo(AppContext.BaseDirectory);
    while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "CONCOST.UI.sln")))
        directory = directory.Parent;

    return directory?.FullName ?? throw new InvalidOperationException("Repository root not found.");
}

static string TypeKind(Type type) =>
    type.IsEnum ? " *(enum)*"
    : type.IsValueType ? " *(struct)*"
    : type.IsInterface ? " *(interface)*"
    : type.IsSealed && type.IsAbstract ? " *(static class)*"
    : type.IsAbstract ? " *(abstract)*"
    : "";

static int CountMembers(Type type) =>
    type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly).Length
    + type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly).Count(m => !m.IsSpecialName)
    + type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly).Length
    + type.GetEvents(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly).Length;

static string Short(Type type)
{
    var name = type.Name;
    var tick = name.IndexOf('`');

    if (type.IsGenericType && tick > 0)
    {
        var arguments = string.Join(", ", type.GetGenericArguments().Select(Short));
        name = $"{name[..tick]}<{arguments}>";
    }
    else if (tick > 0)
    {
        name = name[..tick];
    }

    return name switch
    {
        "String" => "string",
        "Int32" => "int",
        "Int64" => "long",
        "Double" => "double",
        "Boolean" => "bool",
        "Object" => "object",
        "Void" => "void",
        _ => name
    };
}

static string GetSummary(string id) => Program.Docs.TryGetValue(id, out var text) ? Clean(text) : "";

static string Clean(string text)
{
    // XML crefs arrive as "T:CONCOST.UI.Controls.Ribbon"; render them as readable code spans.
    text = System.Text.RegularExpressions.Regex.Replace(text, @"\b[TPEM]:CONCOST\.UI\.([A-Za-z0-9_.]+?)\b", "`$1`");
    text = System.Text.RegularExpressions.Regex.Replace(text, @"\s+", " ").Trim();
    return text;
}

static string DocInline(string id)
{
    var summary = GetSummary(id);
    return summary.Length > 0 ? $" — {summary}" : "";
}

static string Slug(string text) => text.ToLowerInvariant().Replace(".", "");

partial class Program
{
    public static Dictionary<string, string> Docs = new();
}

static class ReflectionExtensions
{
    public static bool IsCompilerGenerated(this Type type) =>
        type.GetCustomAttributesData().Any(a => a.AttributeType.Name.Contains("CompilerGenerated"));
}
