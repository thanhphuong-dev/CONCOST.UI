# Getting Started with CONCOST.UI (Multi-target WPF)

## 1. Installation and Integration

Add a reference to the main project in your WPF project file:

```xml
<ItemGroup>
  <ProjectReference Include="..\CONCOST.UI\CONCOST.UI.csproj" />
</ItemGroup>
```

The library produces `net48`, `net8.0-windows`, and `net10.0-windows` builds. A project reference selects the compatible target automatically. If referencing a built DLL directly, choose the folder matching the host application's target framework. For Revit or AutoCAD plugins, confirm the required framework for that exact host release before selecting the assembly.

Merge the resource dictionaries in `App.xaml`:

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

---

## 2. Featured Engineering Controls

### CAD Viewport (ZoomableCanvas and RulerControl)
```xml
<canvas:RulerControl Orientation="Horizontal" MajorStep="50" />
<canvas:ZoomableCanvas ShowGrid="True" GridSize="20">
    <!-- Draw CAD and structural components here. -->
</canvas:ZoomableCanvas>
```

### Engineering Force Diagram (DiagramChart)
```xml
<charts:DiagramChart Points="{Binding MomentPoints}" Title="Moment Diagram" Unit="kN.m" InvertAxis="True" />
```

### Rebar Input (RebarInput)
```xml
<eng:RebarInput Text="4D20" /> <!-- Automatically calculates As = 1256.6 mm² -->
```
