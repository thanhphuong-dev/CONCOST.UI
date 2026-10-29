# CONCOST.UI

**A multi-target WPF component library for engineering, CAD, BIM, and desktop applications.**

[English](README.md) · [Tiếng Việt](README.vi.md)

![Frameworks](https://img.shields.io/badge/frameworks-.NET%20Framework%204.8%20%7C%20.NET%208%20%7C%20.NET%2010-512BD4)
![Platform](https://img.shields.io/badge/platform-Windows%20%7C%20WPF-0078D4)
![License](https://img.shields.io/badge/license-MIT-2ea44f)
[![Wiki](https://img.shields.io/badge/wiki-online-ff7900)](https://thanhphuong-dev.github.io/CONCOST.UI/)

CONCOST.UI provides reusable controls, theme resources, icons, and engineering-focused components in the **`CONCOST.UI`** library. It is designed for standalone WPF applications and WPF views hosted by products such as Revit and AutoCAD.

## Highlights

- **Three target frameworks:** .NET Framework 4.8, .NET 8, and .NET 10. Select the build that matches the host application.
- **A broad WPF control set:** data grids, spreadsheet, navigation, input, dashboards, charts, engineering tools, and visual effects.
- **Consistent styling:** light and dark themes, accent colors, theme presets, and shared design tokens.
- **Host-aware theming:** apply resources to a plugin view without requiring a separate WPF `Application` or taking over the host application's global resources.
- **Interactive examples:** use the Showcase application to explore controls, change themes, and inspect usage examples.

## Framework compatibility

| Library build | Intended host |
| --- | --- |
| `net48` | WPF applications and plugins targeting .NET Framework 4.8 |
| `net8.0-windows` | WPF applications and plugins targeting .NET 8 |
| `net10.0-windows` | WPF applications and plugins targeting .NET 10 |

Revit and AutoCAD runtime requirements vary by release. Check the requirements for the exact host version, then reference the matching `CONCOST.UI.dll`. A project reference selects a compatible target automatically when one is available.

## Quick start

### Add a project reference

```xml
<ItemGroup>
  <ProjectReference Include="path/to/CONCOST.UI/src/CONCOST.UI/CONCOST.UI.csproj" />
</ItemGroup>
```

Or reference the prebuilt assembly for the host's target framework:

```text
src/CONCOST.UI/bin/Release/net48/CONCOST.UI.dll
src/CONCOST.UI/bin/Release/net8.0-windows/CONCOST.UI.dll
src/CONCOST.UI/bin/Release/net10.0-windows/CONCOST.UI.dll
```

The library targets Windows because WPF is Windows-only. The host application must provide its matching .NET runtime and WPF framework. The `net48` build uses `System.Text.Json` and its runtime dependencies; when distributing that target, use the NuGet package or include the dependencies from the build output.

### Merge the theme dictionaries

For a standalone WPF application, add the library dictionaries to `App.xaml`:

```xml
<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <ResourceDictionary Source="pack://application:,,,/CONCOST.UI;component/Themes/ThemeResources.xaml" />
            <ResourceDictionary Source="pack://application:,,,/CONCOST.UI;component/Themes/Generic.xaml" />
        </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
</Application.Resources>
```

### Use a control

```xml
<Window
    x:Class="MyApplication.MainWindow"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:ui="clr-namespace:CONCOST.UI.Controls;assembly=CONCOST.UI">

    <StackPanel Margin="24">
        <ui:UnitBox Header="Beam width"
                    Value="300"
                    Unit="mm"
                    DecimalPlaces="0" />

        <ui:NumericUpDown Value="25"
                          Minimum="0"
                          Maximum="1000"
                          Increment="5"
                          Suffix=" kN" />
    </StackPanel>
</Window>
```

### Change the theme at runtime

```csharp
using System.Windows.Media;
using CONCOST.UI.Themes;

ThemeManager.Current.Theme = AppTheme.Dark;
ThemeManager.Current.Preset = ThemePreset.Ocean;
ThemeManager.Current.AccentColor = Color.FromRgb(2, 132, 199);
```

Available presets include `Default`, `Forest`, `Sunset`, `Slate`, `Midnight`, and `HighContrast`.

## Use in a Revit or AutoCAD plugin

A plugin class library does not need its own `App.xaml` or `Application` instance. Apply the theme to the plugin's root `UserControl` or window so its resources remain scoped to that view:

```csharp
using System.Windows.Media;
using CONCOST.UI.Themes;

public partial class SettingsPanel : UserControl
{
    public SettingsPanel()
    {
        InitializeComponent();
        ThemeManager.Current.ApplyTheme(this, AppTheme.Light);
        ThemeManager.Current.ApplyAccentColor(this, Color.FromRgb(2, 132, 199));
    }

    private void UseDarkTheme()
    {
        ThemeManager.Current.ApplyTheme(this, AppTheme.Dark);
    }
}
```

The same view-level API supports runtime theme changes and works without `Application.Current`. See the [Getting Started guide](docs/GettingStarted.md) for more integration details.

## Build, run, and verify

### Requirements

- Windows 10 or later
- .NET 8 SDK and .NET 10 SDK
- Visual Studio 2022 with the **.NET desktop development** workload (optional)
- .NET Framework 4.8 reference assemblies are restored automatically for the `net48` build

### Build the solution

```powershell
dotnet restore CONCOST.UI.sln
dotnet build CONCOST.UI.sln --configuration Release
```

### Run the Showcase

```powershell
dotnet run --project samples/CONCOST.UI.Showcase/CONCOST.UI.Showcase.csproj
```

### Run tests

```powershell
dotnet test tests/CONCOST.UI.Tests/CONCOST.UI.Tests.csproj --configuration Release
```

## Repository structure

```text
.
├── .github/                         # CI workflow and contribution templates
├── docs/                            # Guides, component documentation, and roadmap archive
├── samples/
│   └── CONCOST.UI.Showcase/         # Interactive control gallery
├── src/
│   └── CONCOST.UI/                  # Multi-target WPF library
├── tests/
│   └── CONCOST.UI.Tests/            # Automated test project
├── CONCOST.UI.sln
├── Directory.Build.props
├── README.md
└── README.vi.md
```

The solution contains one library project. Namespaces such as `CONCOST.UI.Core` and `CONCOST.UI.Themes` organize code within the assembly; they are not separate DLL dependencies.

## Documentation

The full reference is an interactive static wiki with 79 pages, search (`Ctrl K`), XAML/C# code inspector and light/dark themes — open [`docs/index.html`](docs/index.html).

- [Documentation index](docs/README.md) — page map and maintenance workflow
- [Getting Started](docs/GettingStarted.md) — integration, theming, plugin hosting, build
- [Migration Guide](docs/MigrationGuide.md) — WinForms/WPF mapping and deprecation policy
- [Interactive component documentation](docs/index.html) — bilingual VI / EN, `?lang=en` to open in English
- **Online wiki:** <https://thanhphuong-dev.github.io/CONCOST.UI/>
- [Changelog](CHANGELOG.md)
- [Control Showcase](samples/CONCOST.UI.Showcase/)
- [Contributing](CONTRIBUTING.md)

Property tables and C# samples are generated from the source: run `python docs/tools/build-props.py` after adding or renaming a `DependencyProperty`, and `python docs/tools/check-xaml.py` to verify that every snippet still matches the real API.

## Contributing

Bug reports, feature requests, and pull requests are welcome. Please read [CONTRIBUTING.md](CONTRIBUTING.md) before contributing. New controls should include appropriate theme resources, a Showcase example, and tests where practical.

## License

CONCOST.UI is distributed under the [MIT License](LICENSE).
