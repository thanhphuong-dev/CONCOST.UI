# Changelog

All notable changes to CONCOST.UI are documented in this file.
The format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/) and the project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- **Ribbon — inputs and gallery (R5)**. `RibbonCheckBox`, `RibbonTextBox`, `RibbonComboBox` and `RibbonGallery` (with the plain `RibbonGalleryItem` data object) now live on the three-row grid. They are thin shells over the existing check box, text box, combo box and a pair of list boxes, so IME, undo, keyboard navigation and accessibility come for free.
- `RibbonItem.ColumnSpan` — how many group columns an item takes (1 by default). Text boxes, combo boxes and galleries use it to get a usable width without breaking the grid.
- `RibbonItem.IsFixedSize` — keeps an item at the size it asked for when the ribbon compacts. The gallery sets it, because squeezing a gallery into one row would slice its tiles.
- A collapsed `RibbonGroup` now turns galleries and combo boxes into submenus, so a compacted group can still pick a value.
- `RibbonGallery.ExpandedItemTemplate` — the tile template used inside the “show all” pop-up, falling back to `ItemTemplate` when unset. The ribbon grid now draws swatches alone and the pop-up draws swatch + name, because a 32 px cell has no room for a caption.
- `RibbonLayoutTests` — span packing, separator columns, density capping, fixed-size items, combo selection sync and gallery selection.
- **Ribbon — keyboard, colour and persistence (R6)**. `RibbonKeyTip` and `KeyTipService` give the ribbon the Office Alt gesture: press Alt and every tab, command and group launcher grows a letter badge; type the letters and the command runs. Badges are drawn in the ribbon's own overlay (`PART_KeyTipLayer`), not an `AdornerLayer`, so they stay aligned inside popups and scrolling hosts.
- `RibbonItem.KeyTip`, `RibbonGroup.KeyTip` and `RibbonTabItem.KeyTip` — explicit badge text. Leave it empty and a unique letter is derived from `Header`, so a ribbon written without badges is still fully keyboard reachable. `Ribbon.ShowKeyTips()` / `HideKeyTips()` / `IsShowingKeyTips` / `ProcessKeyTipKey(Key)` expose the session to hosts that own their keyboard pipeline.
- `RibbonColorPicker` — the large colour tile: a swatch on the grid, the full `ColorPicker` in a popup, two-way `SelectedColor`.
- `RibbonSearchBox` — `SearchBox` on the ribbon grid, with `Placeholder`, `IsInstantSearch` and `SearchCommand`.
- `RibbonGroupLauncher` — a real control for the group's corner arrow (was an anonymous `Button`), hidden when the group has no `DialogLauncherCommand`. A collapsed group now offers the same command as the last row of its menu.
- `RibbonState` / `RibbonStateStore` — `CaptureState()` and `ApplyState()` round-trip the open tab, the minimised state, the pinned commands and the groups folded away by hand; `ToJson` / `FromJson` / `Save` / `Load` keep the JSON handling out of the app. Unknown ids in an old snapshot are ignored.
- Automation peers for the ribbon family: `RibbonAutomationPeer` (toolbar), `RibbonButtonAutomationPeer` (Invoke), `RibbonToggleButtonAutomationPeer` / `RibbonRadioButtonAutomationPeer` (Toggle), `RibbonGalleryAutomationPeer` and `RibbonComboBoxAutomationPeer` (ExpandCollapse, plus Value on the combo box), `RibbonGroupAutomationPeer`.
- `RibbonItem.PerformClick()` — one virtual entry point for "run this item", overridden by buttons, toggles, dropdowns, split buttons, galleries, the colour picker and the two input shells. Key tips, the accessibility peer and the collapsed-group menu all go through it.
- `RibbonKeyTipTests` — 12 tests covering badge generation, letter matching, tab switching, Esc, duplicate tips, the launcher row, JSON round-trip, forward-compatible state and the automation peers.
- `Ribbon.QuickAccessPlacement` — moves the quick-access strip to `HeaderRight` (default, inline with tabs), `AboveRibbon` (its own thin strip above the tabs) or `BelowRibbon` (under the ribbon body). The same toolbar instance is reparented, so pinned commands and the customise menu travel with it. The placement is saved and restored by `CaptureState()` / `ApplyState()`.
- `Ribbon.ShowQuickAccessToolBar` — collapses the strip entirely when set to `false`.
- `RibbonMenuBar` and `RibbonMenuBarItem` — a standalone horizontal menu bar that reuses `RibbonMenuItem`, `RibbonMenu` and `RibbonMenuSeparator` for its dropdowns. Click to open, hover to switch while a menu is open, Left/Right arrow keys to navigate, Escape to close all.

### Changed
- Ribbon button labels trim with an ellipsis instead of being hard-clipped by the cell, and the icon slot collapses when an item has no icon, so text-only commands are no longer indented by a blank gap.
- The group label no longer carries a permanent overflow chevron — it fades in when the pointer enters the group (and stays while its menu is open), so a group reads as a caption rather than a drop-down button.
- The group dialog launcher is a corner arrow instead of a solid triangle, and it too waits for the pointer: a lone arrow parked at the far right of a wide group read as a stray mark.
- A checked `RibbonToggleButton` / `RibbonRadioButton` is a soft fill with a hairline border, not an accent-coloured ring. Three toggles side by side (B / I / U) each drawing a primary border turned the group into what looked like one segmented control.
- `RibbonColorPicker` draws the swatch as a single 24 px chip instead of a chip inside a frame, so it carries the same visual weight as the icons next to it.
- The `RibbonApplicationMenu` block is neutral by default (`SurfaceSecondary` + `TextPrimary`) instead of a solid accent slab. A primary-coloured rectangle in the corner was the loudest thing in the ribbon and competed with the accent, which belongs to the selected tab. Set `Background` / `Foreground` on the control to get the Office 2010 look back.
- `RibbonGroup.IsLastInStrip` is set by `RibbonGroupStrip` and drops the right-most group's divider, so no line trails into the empty part of the strip.
- The `ComboBox`'s internal toggle button no longer inherits the app-wide `ToggleButton` style (`MinHeight` 34). Small combo boxes now draw a complete border with a centred arrow instead of two side rails.
- `RibbonTabItem.KeyTip` is now the same dependency property as `KeyTipService.KeyTip`, so `KeyTip="H"` and `KeyTipService.KeyTip="H"` mean the same thing.
- `build-props.py` resolves dependency-property aliases (`XProperty = Other.YProperty`), so an attached property re-exposed on a control shows up in the generated tables.

### Fixed
- The quick-access toolbar drew its customise chevron even with nothing pinned, leaving a lone triangle floating in the tab row. It is now collapsed while the toolbar is empty.
- Gallery tiles clipped their captions and left uneven rows: the cell was sized for the swatch while the template drew swatch + label. Tiles are now uniform at any ribbon width.

## [1.0.1] - 2026-10-30

### Fixed
- The Showcase app icon rendered as a blank square in Explorer and was missing in the header. The .ico stored only PNG payloads; Windows needs DIB (BMP) frames for 16-128 px and reserves PNG for 256. The packer now emits real DIB frames, and the in-app logo uses a PNG so it never depends on frame selection.
- Replay buttons did nothing: `AnimatedEffectBase` kept accumulating elapsed time, so toggling `IsAnimated` re-hooked the loop but the effect stayed finished. Added `Restart()` (plus `OnRestarted()` for derived phase state); the TypewriterText and TextReveal demos use it.

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

[Unreleased]: https://github.com/thanhphuong-dev/CONCOST.UI/compare/v1.0.1...HEAD
[1.0.1]: https://github.com/thanhphuong-dev/CONCOST.UI/releases/tag/v1.0.1
[1.0.0]: https://github.com/thanhphuong-dev/CONCOST.UI/releases/tag/v1.0.0
