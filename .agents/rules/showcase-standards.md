# CONCOST.UI Showcase & Component Development Rules

## 1. Control Research & Development Strategy
Whenever a control is requested:
- **Custom Development**: Build each control from first principles, tuned for `CONCOST.UI` (theme integration, modern animations, token system).
- **Variant Completeness**: Implement all meaningful variants and styles so users know how to apply them.
- **Language**: Synchronize all UI labels, code comments, titles, tooltips, and documentation in **English**.

---

## 2. Showcase Standard Rules (Showcase UI Structure)
- **Section Card Design**:
  - Container: White/Surface card with `CornerRadius="10"`, subtle border (`BorderThickness="1"`), clean padding & elevation.
  - Alignment: Absolute OCD-Friendly alignment (fixed/uniform widths for controls in the same group, clean grid/column alignment).
  - States: Focus purely on **Active/Enabled States** (no need for redundant "Disabled States").

---

## 3. Code Inspector & Dual Snippet Support (XAML + C# Code-Behind)
- **Placement**: Top-right corner of each Card Section title (`View Code` / `View XAML` button / toggle).
- **Code Box**:
  - Background: `CONCOST.Brush.SurfaceSecondary`
  - Border: Subtle theme border
  - Toolbar with quick `Copy` button (with feedback notification / `Copied!` tooltip/state).
  - **Syntax Highlighting Palette**:
    - XML / C# Keyword Tag: `#0284C7` (Sky Blue)
    - Attribute / Property: `#B45309` (Amber Brown)
    - String Value: `#047857` (Emerald Green)
    - Comment: `#94A3B8` (Slate Grey)
- **Dual Code Snippet Provision**:
  - Provide both **XAML Markup** and **C# Code-Behind / ViewModel snippets** (e.g. event handlers, commands, service calls, data-binding models) whenever the control involves backend interactions or logic.

---

## 4. Exact 1-to-1 UI-to-Code Sync
- **Strict Sync**: Every control rendered in the UI must have a corresponding concise, clean inline line of XAML inside the inspector.
- **No Placeholders**: Never use truncated placeholders or missing/extra lines. Order, styles, icons, and contents must match 1:1.
