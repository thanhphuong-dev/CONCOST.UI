# Migration Guide

This guide explains how to migrate WinForms or standard WPF applications to **CONCOST.UI**.

---

## 1. Installation

Reference the single `CONCOST.UI` project from your WPF application:
```xml
<ItemGroup>
  <ProjectReference Include="..\CONCOST.UI\CONCOST.UI.csproj" />
</ItemGroup>
```

Configure the theme directly without a dependency injection package:

```csharp
ThemeManager.Current.Theme = AppTheme.System;
ThemeManager.Current.Preset = ThemePreset.Default;
```
---

## 2. Mapping Existing Controls to CONCOST Controls

| Existing Control (WinForms/WPF) | CONCOST Control | Benefit |
|---|---|---|
| `Button` | `<Button Style="{StaticResource CONCOST.Button.Primary}" />` | Vector icons, five appearance variants, and animated hover/pressed states |
| `TextBox` (numeric input) | `<ui:NumericUpDown Suffix=" mm" />` | Automatic min/max limits, keyboard and mouse increments, and number formatting |
| `TextBox` (engineering units) | `<ui:UnitBox Unit="kN.m" />` or `<eng:MomentBox />` | Engineering unit labels and consistent formatting |
| Rebar input | `<eng:RebarInput Text="4D20" />` | Parses rebar notation and calculates steel area $A_s$ instantly |
| `DataGridView` | `<DataGrid Style="{StaticResource CONCOST.DataGrid.Default}" />` | Zebra rows with automatic light/dark theme colors |
| Drawing canvas | `<canvas:ZoomableCanvas />` | Unlimited zoom and pan with a scalable CAD grid |

---

## 3. Breaking Changes and Deprecation Policy

1. **Deprecation Notice:** Deprecated APIs will include `[Obsolete("Use the replacement API; this will be removed in v(N+2).0.0")]` at least two minor versions before removal.
2. **Semantic Versioning:** Breaking changes only appear in **MAJOR** releases (for example, `2.0.0`) and include migration examples in `CHANGELOG.md`.
