# Changelog

All notable changes to CONCOST.UI are documented in this file.
The format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/) and the project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- Bilingual documentation: VI / EN switch in the top bar (`?lang=en` / `?lang=vi` deep links), 79 English page overlays, 302 generated English property descriptions, and localised navigation, badges, table of contents and command palette.
- Documentation wiki rebuilt as a static site (`docs/`) with 79 pages, command palette search (`Ctrl K`), per-page table of contents, XAML/C# code inspector, light/dark themes and five accent colours.
- New reference pages: `SplitButton`, `Expander`, `Badge`, `Skeleton`, `Ripple`, `OtpInput`, `PasswordInput`, loading indicators (`LoadingIndicator`, `CycleLoading`, `RingLoading`, `RollLoading`, `WaitLoading`, `LoadingCircle`, `LoadingLine`), `LoadingOverlay`, `AdaptivePanel`, `NavigationView`, `CONCOSTWindow`, `DataPager`, `CalendarWithClock`.
- New basic-control pages: `CheckBox`, `RadioButton`, `Slider`, `ScrollViewer`, `ListBox`, `ListView`, `GroupBox`, `Menu`/`ContextMenu`, typography (`TextBlock` styles).
- Guide pages: FAQ & troubleshooting, WinForms/WPF migration playbook.
- Cookbook page (`examples`) with four complete, combined scenarios in XAML + C#: beam parameter form, quantity take-off screen, Revit plugin settings panel, KPI dashboard with skeleton loading.
- `docs/tools/build-index.js` regenerates the `docs/README.md` page map from the live data.
- `docs/tools/build-props.py` generates the property tables and C# samples directly from the C# source (`DependencyProperty` registrations).
- `docs/tools/check-xaml.py` validates every XAML snippet in the documentation against the real control API.
- `CHANGELOG.md`.

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

[Unreleased]: https://github.com/thanhphuong-dev/CONCOST.UI/compare/v3.0.0...HEAD
[3.0.0]: https://github.com/thanhphuong-dev/CONCOST.UI/releases/tag/v3.0.0
