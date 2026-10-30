# Getting Started with CONCOST.UI

**English** · [Tiếng Việt](GettingStarted.vi.md)

CONCOST.UI is a multi-target WPF library (`net48`, `net8.0-windows`, `net10.0-windows`) that ships controls, theme tokens and style variants for engineering, CAD and BIM desktop applications.

The complete, searchable reference for every control (properties, style keys, XAML and C# samples) lives in the interactive wiki: open [`docs/index.html`](index.html) in a browser.

---

## 1. Reference the library

```xml
<ItemGroup>
  <ProjectReference Include="..\CONCOST.UI\CONCOST.UI.csproj" />
</ItemGroup>
```

A `ProjectReference` selects the target framework that matches the host application. When you reference a prebuilt assembly instead, pick the folder that matches the host:

```text
src/CONCOST.UI/bin/Release/net48/CONCOST.UI.dll
src/CONCOST.UI/bin/Release/net8.0-windows/CONCOST.UI.dll
src/CONCOST.UI/bin/Release/net10.0-windows/CONCOST.UI.dll
```

Revit 2021–2024 and AutoCAD 2021–2024 run on .NET Framework 4.8 → use `net48`. Revit 2025+ and AutoCAD 2025+ run on .NET 8 → use `net8.0-windows`. Always confirm the runtime of the exact host release.

## 2. Merge the theme dictionaries

`ThemeResources.xaml` loads the token dictionaries (`Colors`, `Typography`, `Spacing`, `CornerRadius`) and the default Light theme. `Generic.xaml` contains every control style and template. Both are required.

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

> Merging `Themes/Light.xaml` alone is not enough: it only defines theme brushes, so token keys such as `CONCOST.Spacing.MD` or `CONCOST.CornerRadius.Default` stay unresolved.

## 3. Declare the namespaces

```xml
<Window x:Class="YourApp.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:ui="clr-namespace:CONCOST.UI.Controls;assembly=CONCOST.UI"
        xmlns:attached="clr-namespace:CONCOST.UI.AttachedProperties;assembly=CONCOST.UI"
        xmlns:charts="clr-namespace:CONCOST.UI.Controls.Charts;assembly=CONCOST.UI"
        xmlns:canvas="clr-namespace:CONCOST.UI.Controls.Canvas;assembly=CONCOST.UI"
        xmlns:eng="clr-namespace:CONCOST.UI.Controls.Engineering;assembly=CONCOST.UI"
        Background="{DynamicResource CONCOST.Brush.Background}"
        Foreground="{DynamicResource CONCOST.Brush.TextPrimary}">
    <Grid />
</Window>
```

Use `DynamicResource` for every brush and style that must follow a runtime theme change.

## 4. Set the theme

```csharp
using System.Windows.Media;
using CONCOST.UI.Themes;

ThemeManager.Current.Theme = AppTheme.System;        // Light | Dark | System
ThemeManager.Current.Preset = ThemePreset.Default;   // Default | Ocean | Forest | Sunset | Slate | Midnight | HighContrast | Brutal
ThemeManager.Current.AccentColor = Color.FromRgb(0xFF, 0x79, 0x00);
```

## 5. Use the controls

```xml
<StackPanel Margin="24">
    <!-- Engineering input with unit badge -->
    <ui:UnitBox Header="Beam width" Value="300" Unit="mm" DecimalPlaces="0" />

    <!-- Numeric input with steppers -->
    <ui:NumericUpDown Value="25" Minimum="0" Maximum="1000" Increment="5" Suffix=" kN" />

    <!-- Rebar notation: parses 4D20 and computes As = 1256.6 mm² -->
    <eng:RebarInput Text="4D20" />

    <!-- Action button with dropdown -->
    <ui:SplitButton Content="Save model" Icon="Save" Style="{DynamicResource CONCOST.SplitButton.Primary}" />
</StackPanel>
```

### Engineering charts and CAD viewport

```xml
<!-- ChartBar / ChartLine / ChartRadar / ChartPie all derive from ChartBase -->
<charts:ChartLine Title="Moment diagram" Unit="kN.m" ShowArea="True" Datas="{Binding MomentSeries}" />

<Grid>
    <Grid.RowDefinitions>
        <RowDefinition Height="24" />
        <RowDefinition Height="*" />
    </Grid.RowDefinitions>
    <canvas:RulerControl Grid.Row="0" Orientation="Horizontal" MajorStep="50" UnitScale="1" />
    <canvas:ZoomableCanvas Grid.Row="1" ShowGrid="True" GridSize="20" />
</Grid>
```

`ChartBase.Datas` is an `IEnumerable<KeyValuePair<string, double>>`: the key is the X axis label, the value is the plotted number. There is no `InvertAxis` property — to follow the TCVN convention (sagging moment drawn downwards), negate the values before binding.

## 6. Use inside a Revit / AutoCAD plugin

A plugin class library has no `App.xaml`. Scope the resources to the plugin view and let `ThemeManager` apply the theme locally:

```csharp
public partial class SettingsPanel : UserControl
{
    public SettingsPanel()
    {
        InitializeComponent();
        ThemeManager.Current.ApplyTheme(this, AppTheme.Dark);
        ThemeManager.Current.ApplyAccentColor(this, Color.FromRgb(0xFF, 0x79, 0x00));
    }
}
```

Merge the dictionaries in the `UserControl.Resources` of that view instead of `Application.Resources`. See the wiki page **Plugin DLL (Revit/AutoCAD)** for the full pattern and the host-pollution pitfalls.

## 7. Build, run, test

```powershell
dotnet restore CONCOST.UI.sln
dotnet build CONCOST.UI.sln --configuration Release
dotnet run  --project samples/CONCOST.UI.Showcase/CONCOST.UI.Showcase.csproj
dotnet test tests/CONCOST.UI.Tests/CONCOST.UI.Tests.csproj --configuration Release
```

Requirements: Windows 10+, .NET 8 SDK and .NET 10 SDK. The `net48` build restores the reference assemblies automatically.

## 8. Where to look next

| Topic | Where |
| --- | --- |
| Every control, property and style key | [`docs/index.html`](index.html) |
| Theme tokens and presets | wiki → **Theme & Design Tokens** |
| Attached properties (icon, badge, validation, drag-drop) | wiki → **Attached Properties** |
| Common integration errors | wiki → **FAQ & Troubleshooting** |
| WinForms/WPF migration mapping | [`MigrationGuide.md`](MigrationGuide.md) |
| Version history | [`../CHANGELOG.md`](../CHANGELOG.md) |
