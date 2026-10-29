# CONCOST.UI

**A multi-target WPF control library for .NET Framework 4.8, .NET 8, and .NET 10, built for desktop, Revit, and AutoCAD plugin workflows.**

![Targets](https://img.shields.io/badge/targets-.NET%20Framework%204.8%20%7C%20.NET%208%20%7C%20.NET%2010-512BD4)
![Platform](https://img.shields.io/badge/platform-Windows%20%7C%20WPF-0078D4)
![License](https://img.shields.io/badge/license-MIT-2ea44f)

CONCOST.UI brings custom WPF controls, theme resources, design tokens, and engineering-oriented components together in one library, with a matching `CONCOST.UI.dll` build for each supported target framework.

> **Platform:** Windows desktop applications and WPF plugins. The library targets .NET Framework 4.8, .NET 8, and .NET 10. WPF is Windows-only; the host must provide the matching runtime and WPF framework.

## Features

- A single `CONCOST.UI` assembly containing the controls, icons, theme resources, and shared helpers.
- Light and dark theme resources with configurable accent colors and presets.
- Reusable WPF controls for numeric and unit input, navigation, data entry, dashboards, and engineering workflows.
- Vector-based icons and attached properties for extending standard WPF controls.
- XAML examples in the Demo and Showcase applications.

## Repository layout

```text
.
├── .github/                         # GitHub Actions and issue templates
├── .agents/                         # Repository-specific development guidance
├── docs/                            # Documentation site and archived planning documents
│   └── roadmap/
├── samples/
│   └── CONCOST.UI.Showcase/         # Control gallery and detailed examples
├── src/
│   └── CONCOST.UI/                  # The single library project and assembly
├── tests/
│   └── CONCOST.UI.Tests/            # xUnit test project
├── CONCOST.UI.sln                  # Visual Studio solution
├── Directory.Build.props            # Shared .NET build settings
└── README.md
```

The current solution builds one library project. The `CONCOST.UI.Core` and `CONCOST.UI.Themes` namespaces organize code inside that assembly; they are not separate DLL dependencies.

## Requirements

- Windows 10 or later
- .NET 8 SDK and .NET 10 SDK
- .NET Framework 4.8 reference assemblies (restored automatically for builds)
- Visual Studio 2022 with the **.NET desktop development** workload (optional, for IDE development)

## Build and run

Run these commands from the repository root in PowerShell or a terminal:

```powershell
dotnet restore CONCOST.UI.sln
dotnet build CONCOST.UI.sln --configuration Release
```

Run the Showcase application:

```powershell
dotnet run --project samples/CONCOST.UI.Showcase/CONCOST.UI.Showcase.csproj
```

Run the test suite:

```powershell
dotnet test tests/CONCOST.UI.Tests/CONCOST.UI.Tests.csproj --configuration Release
```

The library builds one assembly per target framework:

```text
src/CONCOST.UI/bin/Release/net48/CONCOST.UI.dll
src/CONCOST.UI/bin/Release/net8.0-windows/CONCOST.UI.dll
src/CONCOST.UI/bin/Release/net10.0-windows/CONCOST.UI.dll
```

Choose the assembly that matches the host application's target framework. Revit and AutoCAD versions can use different .NET runtimes, so check the API/runtime requirements for the exact host version before adding the reference. The `net48` build uses `System.Text.Json`; when distributing that target outside NuGet, include its runtime dependencies from the build output as well.

## Add CONCOST.UI to a WPF application

### Option 1: Add a project reference

For development alongside this repository, add a project reference to the library:

```xml
<ItemGroup>
  <ProjectReference Include="path/to/CONCOST.UI/CONCOST.UI.csproj" />
</ItemGroup>
```

Alternatively, in Visual Studio, right-click the WPF application project, select **Add > Project Reference**, and choose `CONCOST.UI`.

### Option 2: Reference the built assembly

Build the library, then add the matching `CONCOST.UI.dll` reference from the host application. The host must target a compatible Windows framework with WPF enabled. A `net8.0-windows` host should use the `net8.0-windows` assembly; a .NET Framework 4.8 plugin should use `net48`; a `net10.0-windows` host should use `net10.0-windows`. The host application still needs its matching runtime.

## Load the themes

Merge the library's theme resources and control styles into the WPF application's resources. The sample applications use this setup in `App.xaml`:

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

## Use controls in XAML

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

## Change the theme in C#

```csharp
using System.Windows.Media;
using CONCOST.UI.Themes;

ThemeManager.Current.Theme = AppTheme.Dark;
ThemeManager.Current.Preset = ThemePreset.Ocean;
ThemeManager.Current.AccentColor = Color.FromRgb(2, 132, 199);
```

`ThemeManager.Current` also exposes presets such as `Default`, `Forest`, `Sunset`, `Slate`, `Midnight`, and `HighContrast`.

### Use CONCOST.UI from a Revit or AutoCAD WPF plugin

A plugin class library does not need to define its own `App.xaml` or create an `Application`. Apply the resources to the plugin window or `UserControl` root instead. This keeps theme changes scoped to the plugin view instead of changing resources owned by Revit, AutoCAD, or another host:

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

`ApplyTheme(rootElement, theme)` loads the design tokens, selected light/dark palette, and control styles into the supplied root. The same call can be repeated at runtime to switch the plugin view's theme. `ApplyAccentColor(rootElement, color)` and `ApplySurfaceStyle(rootElement, style)` also work without relying on `Application.Current`. Ensure the plugin target framework is supported by the specific Revit or AutoCAD version being hosted.

## Documentation and examples

- [Interactive component documentation](docs/index.html)
- [Control Showcase](samples/CONCOST.UI.Showcase/)
- [Planning documents](docs/roadmap/)
- [Contribution guide](CONTRIBUTING.md)

## Contributing

Bug reports, feature requests, and pull requests are welcome. Please read [CONTRIBUTING.md](CONTRIBUTING.md) before opening a change. New controls should include appropriate theme resources, a Showcase example, and tests where practical.

## License

CONCOST.UI is licensed under the [MIT License](LICENSE).
