# Changelog

All notable changes to CONCOST.UI are documented in this file.
The format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/) and the project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

_Nothing yet._

## [1.0.0] - 2026-10-30

First public release. This is the library as it stands, not a delta — everything listed below ships in this version.

### Controls

- **Engineering & CAD** — `Spreadsheet` with a formula engine, `StackedHeaderDataGrid`, `ZoomableCanvas` + `RulerControl`, internal-force charts, `RebarInput`, `UnitBox`, `MomentBox`, `CompassIndicator`.
- **Data & dashboards** — `StatisticCard`, `OdometerNumber`, `Sparkline`, `HeatmapCalendar`, `AvatarStack`, `Timeline`, `CardCarousel`, `DataPager`, `PropertyGrid`.
- **Input & forms** — `NumericUpDown`, `OtpInput`, `PinBox`, `CheckComboBox`, `AutoCompleteTextBox`, `SearchBox`, `FilePicker`, `ColorPicker`, `RangeSlider`, `Rating`, `Tag`, `ToggleSwitch`.
- **Navigation & shell** — `NavigationView`, `SideMenu`, `DrawerMenu`, `Breadcrumb`, `Pagination`, `StepBar`, `Transfer`, `TabControl`, `SplitButton`, `Expander`, `CONCOSTWindow`.
- **Feedback** — `Dialog`, `Drawer`, `Growl`, loading indicators and overlays, `Skeleton`, `EmptyState`, `Watermark`.
- **Visual effects** — `SpotlightCard`, `TiltCard`, `MagneticHover`, `CursorGlow`, `AuroraBackground`, `BorderBeam`, `SheenSweep`, `PulseRing`, `WaveDivider`, `GradientText`, `ConfettiBurst`, `GridPattern`, `MarqueeTicker`, `ScrollReveal`, `ParallaxPanel`, `TypewriterText`, `TextReveal`, `CheckMarkDraw`, `ScrollProgressBar`, `ShakeElement`, `Ripple`, `SnowCanvas`, `Sprite`, `CompareSlider`, `Magnifier`.

### Platform

- Three target frameworks from one source: `net48`, `net8.0-windows`, `net10.0-windows`.
- 8 theme presets (`Default`, `Ocean`, `Forest`, `Sunset`, `Slate`, `Midnight`, `HighContrast`, `Brutal`), light/dark, 5 accent colours, shared design tokens.
- `MotionSettings` honours the Windows "Show animations" setting across every effect (WCAG 2.2.2).

### Documentation

- Static wiki: **106 pages**, bilingual Vietnamese / English, search with `Ctrl K`, XAML ↔ C# inspector, fully offline (self-hosted fonts, icons, syntax highlighting), WCAG AA contrast.
- Property tables and C# samples generated from the source; `check-xaml.py` validates every snippet against the real API.

### Packaging

- NuGet package, framework-dependent demo and self-contained demo attached to the release.

---

## Internal history (pre-public)

Versions 3.0.0 – 3.7.0 below were internal milestones reached before the first public release. They are kept for reference; no public release carries those numbers.

## [3.7.0] - 2026-10-29

### Added
- `ScrollReveal` — attached behaviour that fades and slides an element the first time it scrolls into view (auto-detects the ScrollViewer, reveals once, then unhooks).
- `ParallaxPanel` — content layer that drifts with the scroll position by a speed factor, with offset clamping and horizontal support.
- `TypewriterText` — types text character by character with a blinking caret, optional looping.
- `TextReveal` — headline revealed word by word with a staggered rise and ease-out.

### Changed
- Docs: 106 pages / 627 properties; roadmap Phase 2 marked done.

## [3.6.0] - 2026-10-29

### Added
- `TiltCard` — card that leans toward the pointer and lifts on hover (skew + scale 2.5D, no Viewport3D), with `Base` and `Elevated` variants.
- `MagneticHover` — attached behaviour pulling an element toward the pointer via `TranslateTransform` (no layout pass), with strength/offset limits.
- `CheckMarkDraw` — confirmation tick that draws itself in 320 ms and erases in reverse.
- `ScrollProgressBar` — reading progress bar bound to a `ScrollViewer`, auto-detecting the nearest ancestor when `Target` is omitted.

### Changed
- Docs: 102 pages / 599 properties; the effects roadmap marks Phase 1 done.

## [3.5.0] - 2026-10-29

### Added
- `MotionSettings` + `AnimatedEffectBase` — every effect now follows the Windows "Show animations" setting (`SystemParameters.ClientAreaAnimation`), can be muted app-wide with `MotionSettings.ForceDisabled`, and settles on a static frame instead of freezing when motion is off (WCAG 2.2.2).
- `CursorGlow` — soft light trailing the pointer, with eased follow and fade-out.
- `GridPattern` — tiled dot / line / cross backdrop drawn as a single `DrawingBrush` fill, with phase offsets for parallax.
- `ShakeElement` — attached shake feedback for invalid input (360 ms, decaying amplitude, preserves existing `RenderTransform`).
- `ThemePreset.Brutal` — neo-brutalist palette (cream surfaces, ink borders, electric accent) with automatic snapshot/restore when switching back.

### Changed
- The eight effects that predate the base class (`AuroraBackground`, `BorderBeam`, `ConfettiBurst`, `MarqueeTicker`, `SheenSweep`, `PulseRing`, `WaveDivider`, `GradientText`) now check `MotionSettings` before running their frame loop.

## [3.4.0] - 2026-10-29

### Added
- `SheenSweep` — diagonal light sweep with an idle gap, for CTAs and highlight cards.
- `PulseRing` — expanding, fading rings that mark a live or in-progress state.
- `WaveDivider` — two drifting sine layers as a section divider or header base.
- `GradientText` — headline text filled with an animated gradient.
- Showcase demos with code panels for all four, and bilingual docs (93 pages / 564 properties).

## [3.3.0] - 2026-10-29

### Added
- `AuroraBackground` — animated aurora backdrop (large drifting radial glows, no `BlurEffect`), clipped to its box, for splash screens and hero panels.
- `BorderBeam` — a comet of light that travels along the rounded-rect perimeter (three-pass glow), wrapping any content via `Child`.
- `ConfettiBurst` — self-resetting confetti burst for approve/saved moments, with gravity, duration, palette and origin.
- Showcase demos with View XAML / View C# panels for all three, and bilingual documentation pages (89 pages / 533 properties).

### Changed
- `BorderBeam` no longer uses a rotating linear gradient: the beam is sampled along the perimeter, so a single light runs around the border instead of two faint arcs.

## [3.2.0] - 2026-10-29

### Added
- `HeatmapCalendar` — GitHub-style daily intensity grid with four colour levels, month labels and configurable cell size/gap, for pour logs, inspections and monitoring density.
- `MarqueeTicker` — continuously scrolling status strip driven by `CompositionTarget.Rendering`, with speed, separator and pause support.
- Showcase demos for both, plus **View XAML / View C#** panels on every web-inspired demo (the four 3.1.0 demos previously shipped without code).

### Fixed
- Showcase: the new-control demos were missing the code panels that every other demo has, so users could not see the XAML or C# behind them.

## [3.1.0] - 2026-10-29

### Added
- Four controls ported from modern web UI patterns:
  - `SpotlightCard` — cursor-following radial spotlight plus a gradient border revealed on hover (`CONCOST.SpotlightCard.Base` / `.Glass`).
  - `OdometerNumber` — rolls from the previous value to the new one with easing (`Base` / `Hero` / `Compact`), no per-update storyboard.
  - `Sparkline` — inline mini chart with optional smooth curves, gradient area fill and last-point marker.
  - `AvatarStack` — overlapping avatars with a `+N` overflow chip; accepts names (initials derived) or `ImageSource`.
- Showcase demos for all four (`ShowcaseViewModel.WebDemos.cs`), documentation pages in both languages, and 20 new property descriptions.
- Offline build: fonts (latin/latin-ext/vietnamese subsets), Font Awesome and Prism are vendored under `docs/vendor/`, so the wiki renders with no CDN and no network.
- `docs/tools/fetch-fonts.py` re-downloads the localised font subsets; `docs/tools/build-index.js` now also emits the page → source file map.
- "Edit this page" link in the reading rail, pointing at the exact data file that owns the page.
- `docs/data/en/code.js`: 290-entry Vietnamese → English dictionary applied to code snippets, so EN pages no longer show Vietnamese comments or sample strings (289 code lines covered, 0 left).
- Bilingual documentation: VI / EN switch in the top bar (`?lang=en` / `?lang=vi` deep links), 84 English page overlays, 324 generated English property descriptions, and localised navigation, badges, table of contents and command palette.
- Documentation wiki rebuilt as a static site (`docs/`) with 84 pages, command palette search (`Ctrl K`), per-page table of contents, XAML/C# code inspector, light/dark themes and five accent colours.
- New reference pages: `SplitButton`, `Expander`, `Badge`, `Skeleton`, `Ripple`, `OtpInput`, `PasswordInput`, loading indicators (`LoadingIndicator`, `CycleLoading`, `RingLoading`, `RollLoading`, `WaitLoading`, `LoadingCircle`, `LoadingLine`), `LoadingOverlay`, `AdaptivePanel`, `NavigationView`, `CONCOSTWindow`, `DataPager`, `CalendarWithClock`.
- New basic-control pages: `CheckBox`, `RadioButton`, `Slider`, `ScrollViewer`, `ListBox`, `ListView`, `GroupBox`, `Menu`/`ContextMenu`, typography (`TextBlock` styles).
- Guide pages: FAQ & troubleshooting, WinForms/WPF migration playbook.
- Cookbook page (`examples`) with four complete, combined scenarios in XAML + C#: beam parameter form, quantity take-off screen, Revit plugin settings panel, KPI dashboard with skeleton loading.
- `docs/tools/build-index.js` regenerates the `docs/README.md` page map from the live data.
- `docs/tools/build-props.py` generates the property tables and C# samples directly from the C# source (`DependencyProperty` registrations).
- `docs/tools/check-xaml.py` validates every XAML snippet in the documentation against the real control API.
- `CHANGELOG.md`.

### Accessibility
- Colour tokens now meet WCAG AA: muted text 2.87:1 → 5.43:1, accent-as-text 2.63:1 → 5.09:1 (a darker `--accent-ink` is used for text while `--accent` stays for fills), dark-mode muted text 4.59:1 → 6.46:1.
- Sidebar, pager, quick links and related links are real anchors, so the whole wiki is reachable by keyboard; `aria-current="page"` marks the active page.
- Visible `:focus-visible` ring on every interactive element, skip-to-content link, `tabindex="-1"` main landmark, `aria-expanded` on the menu and search triggers, focus trap plus focus restore in the command palette, `aria-live` result count.
- Section cards are `<h2>` elements, giving every page a single `h1` and a proper heading outline.
- Larger tap targets on phones (40px buttons, 10px nav padding).

### Added
- `docs/404.html` with a styled not-found page, inline SVG favicon, Open Graph/Twitter meta tags and a `<noscript>` fallback.

### Changed
- Documentation installation snippet now merges `Themes/ThemeResources.xaml` (tokens + default theme) and `Themes/Generic.xaml`; the previous snippet merged `Themes/Light.xaml`, which left typography, spacing and corner-radius tokens unresolved.
- Hand-written property tables replaced by generated ones, so documented properties always match the source.
- Roadmap revisions 1–2 and the first implementation plan moved to `docs/roadmap/archive/`; revision 3 remains the active roadmap.

### Fixed
- Documentation referenced a `DiagramChart` control and `Points` / `InvertAxis` properties that do not exist. The page now documents `ChartLine` with the real `Datas` (`IEnumerable<KeyValuePair<string, double>>`) contract and explains how to honour the TCVN moment convention.
- Documentation referenced a non-existent `attached:InputBase.Placeholder`; replaced with `attached:InfoElement.Placeholder`.
- Incorrect members corrected across pages: `RulerControl.ZoomLevel`/`ScrollOffset` → `UnitScale`/`OriginOffset`; `Drawer.Dock`/`MaskClosable` → `Placement`/`DrawerWidth`; `NumericUpDown.Unit` → `Suffix`; `Pagination.TotalCount` → `TotalItems`; `Rating.Maximum`/`AllowHalf` → `Max`/`ItemSize`; `Tag.Type`/`ShowCloseButton` → `Icon`/`IsClosable`; `TimelineItem.Timestamp`/`Header` → `Time`/`Description`; `Transfer.SelectedItemsSource` → `TransferredItems`; `DataGridGroupPanel.DataGrid` → `TargetGrid`.
- `docs/MigrationGuide.md` referenced a `CHANGELOG.md` file that did not exist.

## [3.0.0]

### Added
- Multi-target WPF library: `net48`, `net8.0-windows`, `net10.0-windows`.
- Theme system with `ThemeManager`, six presets (`Default`, `Ocean`, `Forest`, `Sunset`, `Slate`, `Midnight`, `HighContrast`) and view-scoped theming for plugin hosts.
- Design tokens: colours, typography, spacing and corner radius dictionaries.
- Engineering controls: `Spreadsheet` with formula engine, `StackedHeaderDataGrid`, `ZoomableCanvas`, `RulerControl`, chart suite (`ChartBar`, `ChartLine`, `ChartRadar`, `ChartPie`), `RebarInput`, `UnitBox` family, `CompassIndicator`.
- Input controls: `NumericUpDown`, `UnitBox`, `CheckComboBox`, `PinBox`, `ColorPicker`, `RangeSlider`, `AutoCompleteTextBox`, `SearchBox`, `FilePicker`, `Rating`, `Tag`, `ToggleSwitch`.
- Navigation controls: `SideMenu`, `DrawerMenu`, `Breadcrumb`, `Pagination`, `StepBar`, `Transfer`, `NavigationView`.
- Feedback controls: `Dialog`, `Drawer`, `Growl`, `LoadingIndicator`, `LoadingOverlay`, `Skeleton`, `EmptyState`, `Watermark`.
- Layout and effects: `SplitButton`, `Expander`, `Card`, `Badge`, `Divider`, `AdaptivePanel`, `CONCOSTWindow`, `Ripple`, `SnowCanvas`, `Sprite`, `CompareSlider`, `Magnifier`.
- Attached property sets: `InfoElement`, `IconElement`, `ButtonElement`, `BorderElement`, `VisualElement`, `ProgressBarElement`, `DataGridElement`, `TextBoxHelper`, `PasswordBoxHelper`, `TabControlHelper`, `ScrollViewerHelper`, `DragDropHelper`.
- Interactive Showcase application and automated test project.

[Unreleased]: https://github.com/thanhphuong-dev/CONCOST.UI/compare/v1.0.0...HEAD
[1.0.0]: https://github.com/thanhphuong-dev/CONCOST.UI/releases/tag/v1.0.0
