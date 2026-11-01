# Revit-inspired CAD UI spec — gap analysis

Source: `Revit_Inspired_CAD_UI_Control_Specification_v1.0.docx` (TT-UI-SPEC-001 v1.0), 135 logical
items in 11 categories, 12 managers/services, 20 acceptance criteria.

This page maps every item in that document onto what CONCOST.UI ships. The first pass found 62 covered
/ 19 partial / 54 missing and drove [`revit-spec-plan.md`](revit-spec-plan.md); the tables below are the
same pass re-read after phases A–F landed.

Legend:

| Mark | Meaning |
| --- | --- |
| ✅ | Covered: a control or service of that name exists and does the job. |
| 🟡 | Partial: a generic control covers the behaviour, but not the CAD-specific shape or the exact item. |
| ❌ | Missing. |

Summary: **132 covered (98%), 3 partial, 0 missing** across the 135 control items, plus **11 of 12
services** and **20 of 20 acceptance criteria**. The four remaining 🟡 entries are listed at the end.

---

## 3.1 Application shell controls (6)

| # | Spec item | Status | In the library |
| --- | --- | --- | --- |
| 1 | MainWindow | ✅ | `CONCOSTWindow` (custom chrome, caption buttons, DPI-safe resize). |
| 2 | ApplicationTitleBar | ✅ | `ApplicationTitleBar` — document title with the unsaved `*`, quick access that overflows into a menu, drag/double-click and the three window buttons. `ComposeTitle` is the pure caption rule. |
| 3 | ApplicationMenu | ✅ | `RibbonBackstageView` — rail of items plus a page area; the pages themselves are host content. |
| 4 | QuickAccessToolbar | ✅ | `QuickAccessToolbar` + `QuickAccessItem` (add/remove/reorder, persisted by the host). |
| 5 | ApplicationInfoBar | ✅ | `ApplicationInfoBar` — four severities, tinted from the accent, one action, closable. |
| 6 | ApplicationStatusBar | ✅ | `StatusBar` + `StatusBarItem` (left/right cells, separators, commands). |

## 3.2 Ribbon controls (18)

| # | Spec item | Status | In the library |
| --- | --- | --- | --- |
| 1 | RibbonControl | ✅ | `Ribbon` — tabs, contextual tabs, four fold states plus overlay, key tips, sizing. |
| 2 | RibbonTabControl | ✅ | `RibbonTabStripPanel` inside `Ribbon`. |
| 3 | RibbonTabItem | ✅ | `RibbonTabItem`. |
| 4 | RibbonPanel | ✅ | `RibbonGroup` / `RibbonPanel` roles. |
| 5 | RibbonPanelHeader | ✅ | Group header with title, dialog launcher and slide-out. |
| 6 | RibbonButton | ✅ | `RibbonButton`. |
| 7 | RibbonLargeButton | ✅ | `RibbonButton` with `Size="Large"`, demoted automatically by `RibbonGroupSizeMode`. |
| 8 | RibbonSplitButton | ✅ | `RibbonSplitButton`, including `RememberLast` / `LastUsedItem`. |
| 9 | RibbonDropDownButton | ✅ | `RibbonDropDownButton`. |
| 10 | RibbonToggleButton | ✅ | `RibbonToggleButton`. |
| 11 | RibbonComboBox | ✅ | `RibbonComboBox`. |
| 12 | RibbonGallery | ✅ | `RibbonGallery` + `RibbonGalleryItem`. |
| 13 | RibbonSeparator | ✅ | `RibbonSeparator`. |
| 14 | RibbonGroup | ✅ | `RibbonGroup`, `RibbonGroupPanel`, `RibbonGroupStrip`. |
| 15 | RibbonBackstageView | ✅ | `RibbonBackstageView` — item rail, back button, page area, `IsOpen` without touching the drawing area. |
| 16 | ContextualRibbonTab | ✅ | `RibbonContextualTabGroup` + `ShowContextualTab` / `HideContextualTab`. |
| 17 | RibbonSearchBox | ✅ | `RibbonSearchBox`. |
| 18 | RibbonMinimizeButton | ✅ | `Ribbon.DisplayMode` + `CycleState()`: four fold states (full, tabs, panel titles, panel buttons), caret button and overlay. |

## 3.3 Docking system controls (12)

| # | Spec item | Status | In the library |
| --- | --- | --- | --- |
| 1 | DockingManager | ✅ | `DockingManager` — dock, float, tab, flyout, capture/restore. |
| 2 | DockingGroup | ✅ | `DockPaneGroup`. |
| 3 | DockingPane | ✅ | `ToolWindow`. |
| 4 | DockingTabControl | ✅ | `DockTabStrip`. |
| 5 | DockingTabItem | ✅ | `DockTabStrip` tab buttons. |
| 6 | DockingHeader | ✅ | `ToolWindow` header (drag, pin, close, menu). |
| 7 | DockingSplitter | ✅ | `DockSplitter` (band hit-testing, pair resizing, `MinSize`). |
| 8 | FloatingWindow | ✅ | `DockingManager.Float` returns a floating `Window`. |
| 9 | AutoHidePane | ✅ | `AutoHidePane`. |
| 10 | AutoHideTab | ✅ | Flyout strip tab. |
| 11 | DockingGuide | ✅ | `DockPreview` (dashed drop preview). |
| 12 | DockingLayoutSerializer | ✅ | `DockingManager.CaptureLayout()` / `ApplyLayout(string)`; the host owns file I/O. |

## 3.4 Project browser (10)

| # | Spec item | Status | In the library |
| --- | --- | --- | --- |
| 1 | ProjectBrowser | ✅ | `ProjectBrowser` — tree, search row, group/sort row and a per-kind context menu in one panel. |
| 2 | ProjectTreeView | ✅ | The tree inside `ProjectBrowser`; search keeps the path to every match. |
| 3 | ProjectTreeNode | ✅ | `ProjectTreeNode` with `ProjectNodeKind` (Project/Folder/View/Sheet/Family/Schedule/Level), `Parent`, `IsModified`, `Children`. |
| 4 | ProjectTreeItem | ✅ | Rows are built per node with its kind icon and a modified marker. |
| 5 | ProjectBrowserSearchBox | ✅ | Search row, diacritic-insensitive through `CommandPaletteText.Normalize`. |
| 6 | ProjectBrowserToolbar | ✅ | The group/sort row plus the search row; `SetSearchText` drives it from code. |
| 7 | ProjectBrowserContextMenu | ✅ | Per-kind menu: open, rename, duplicate, delete, properties — each a request event. |
| 8 | ProjectBrowserFilter | ✅ | `KindFilter` narrows to one node kind; the text filter is independent. |
| 9 | ProjectBrowserGrouping | ✅ | `GroupMode` (`None` / `ByKind`) and `SortMode` (`NameAscending` / `NameDescending` / `Kind`). |
| 10 | ProjectBrowserNavigation | ✅ | `Open`, `Select`, `ExpandAll` / `CollapseAll`, and `OpenRequested` / `RenameRequested` / `DeleteRequested` / `DuplicateRequested` / `PropertiesRequested`. |

## 3.5 Properties (18)

| # | Spec item | Status | In the library |
| --- | --- | --- | --- |
| 1 | PropertiesPalette | ✅ | `PropertiesPalette` ties the header, type selector, search, categories, validation and reset together. |
| 2 | PropertiesHeader | ✅ | Selection title + subtitle, rebuilt on every selection change. |
| 3 | TypeSelector | ✅ | Type combo in the palette's type row. |
| 4 | EditTypeButton | ✅ | `EditTypeRequested` / `TypeChangeRequested`. |
| 5 | PropertiesCategory | ✅ | `PropertyCategoryGroup`. |
| 6 | PropertyGrid | ✅ | `PropertyGrid` + `PropertyItem`. |
| 7 | PropertyRow | ✅ | `PropertyItem` row template. |
| 8 | PropertyNameCell | ✅ | Row name column. |
| 9 | PropertyValueCell | ✅ | Row value column. |
| 10 | PropertyTextEditor | ✅ | Default string editor. |
| 11 | PropertyNumericEditor | ✅ | Numeric editor (`NumericUpDown`). |
| 12 | PropertyComboBox | ✅ | Enum/list editor. |
| 13 | PropertyCheckBox | ✅ | Boolean editor. |
| 14 | PropertyColorPicker | ✅ | `ColorPicker` editor. |
| 15 | PropertyReferencePicker | ✅ | `PropertyReferencePicker` — pick another model object. |
| 16 | PropertySearchBox | ✅ | `SearchBox` at the top of the palette filters the grid. |
| 17 | PropertyValidationMessage | ✅ | `ValidationMessage` row under the grid; a rejected value never reaches the object. |
| 18 | PropertyResetButton | ✅ | `ResetProperty` returns the descriptor's `DefaultValue` and reports why when it cannot. |

## 3.6 Document workspace (9)

| # | Spec item | Status | In the library |
| --- | --- | --- | --- |
| 1 | DocumentWorkspace | ✅ | `DocumentWorkspace` — documents, tabs, tile modes, float, placeholder. |
| 2 | DocumentTabControl | ✅ | `DocumentTabControl`. |
| 3 | DocumentTabItem | ✅ | `DocumentTabItem`. |
| 4 | DocumentTabHeader | ✅ | Icon, unsaved dot, close button in the item template. |
| 5 | DocumentTabContextMenu | 🟡 | `CloseDocument` / `CloseOthers` / `CloseAll` are public, but no menu is shipped. |
| 6 | DocumentSplitter | ✅ | `GridSplitter` between panes in `Vertical` and `Horizontal` tile modes. |
| 7 | DocumentTileManager | ✅ | `DocumentTileMode` (`Single` / `Vertical` / `Horizontal` / `Grid`), `PaneCount`. |
| 8 | DocumentFloatingWindow | ✅ | `FloatDocument(pane)` returns the `Window` and closes it when the pane closes. |
| 9 | DocumentPlaceholder | ✅ | `PlaceholderTitle` / `PlaceholderText` when no pane is open. |

## 3.7 Viewport and navigation (18)

| # | Spec item | Status | In the library |
| --- | --- | --- | --- |
| 1 | ViewportControl | ✅ | `ViewportControl` — surface with a real `CameraController`, grid, crosshair, snap, grips, dimension, selection rectangle, overlay. |
| 2 | ViewportHost | 🟡 | `RendererBridge.Adapter` (`IRendererAdapter`) is the invalidate/pick seam; there is no shipped `HwndHost`, because the library cannot own a device. |
| 3 | ViewportOverlay | ✅ | `ViewportControl.Overlay` — a Canvas above the surface in screen coordinates. |
| 4 | ViewCube | ✅ | `ViewCube` with face/edge/corner zones and `ViewRequested`. |
| 5 | ViewCubeFace | ✅ | `ViewCubeFace` + `HitTest` / `IsFaceVisible`. |
| 6 | ViewCubeCorner | ✅ | Corner and edge parts in `ViewCubeHit`, with quarter turns. |
| 7 | ViewCubeCompass | ✅ | Compass ring inside `ViewCube` (`ShowCompass`), driven by the camera yaw. |
| 8 | NavigationBar | ✅ | `NavigationBar` — zoom in/out/extents, pan, orbit, walk, UCS label. |
| 9 | ZoomControl | ✅ | `NavigationBar` zoom commands + `CameraController.Zoom` / `FitTo`. |
| 10 | PanControl | ✅ | Latched pan tool (`ActiveTool`) + `CameraController.Pan`. |
| 11 | OrbitControl | ✅ | Latched orbit tool + `CameraController.Orbit`. |
| 12 | UCSControl | ✅ | `Ucs` / `UcsChanged` on `NavigationBar`. |
| 13 | CrosshairControl | ✅ | `ShowCrosshair` with aperture over the surface. |
| 14 | GridControl | ✅ | Snapping grid: `ShowGrid`, `GridStep`, `SnapToGrid`, `SnapDistance`. |
| 15 | SnapIndicator | ✅ | `Snap` / `SnapKind` draw the marker and set `IsSnapped` on `CursorMoved`. |
| 16 | SelectionOverlay | ✅ | `SelectionRectangle` painted on top of the surface. |
| 17 | GripControl | ✅ | `Grips` / `ViewportGrip` with hot-grip rendering. |
| 18 | TemporaryDimension | ✅ | `TemporaryDimension` (start, end, offset, text). |

## 3.8 View control bar (8)

| # | Spec item | Status | In the library |
| --- | --- | --- | --- |
| 1 | ViewControlBar | ✅ | `ViewControlBar` — one strip owning every view setting. |
| 2 | ViewScaleSelector | ✅ | `ViewScale` (1:50 … 1:500) with `SettingsChanged`. |
| 3 | DetailLevelSelector | ✅ | `DetailLevel` (`Coarse` / `Medium` / `Fine`). |
| 4 | VisualStyleSelector | ✅ | `VisualStyle` (wireframe, hidden line, shaded, realistic). |
| 5 | ShadowToggle | ✅ | `ShowShadows`. |
| 6 | CropViewToggle | ✅ | `CropView`. |
| 7 | HideIsolateMenu | ✅ | `HideIsolateRequested` with hide/isolate/show-all actions. |
| 8 | RevealHiddenToggle | ✅ | `RevealHidden`. |

## 3.9 Command and input (8)

| # | Spec item | Status | In the library |
| --- | --- | --- | --- |
| 1 | CommandLine | ✅ | `CommandLine` — history, input, prompt mode, repetition on Enter. |
| 2 | CommandInputBox | ✅ | The input box with Tab completion over names and aliases. |
| 3 | CommandHistory | ✅ | `Messages` + arrow-key recall + `MaxHistory`. |
| 4 | CommandAutoComplete | ✅ | Prefix and alias matching as you type. |
| 5 | CommandPrompt | ✅ | `CommandDefinition.Prompt` → `PromptCompleted` / `PromptCancelled`. |
| 6 | DynamicInput | ✅ | `DynamicInput` — the box that follows the cursor for a length, an angle or a coordinate. |
| 7 | CoordinateInput | ✅ | `CadInputParser.TryParseCoordinate`, shared by the command line and `DynamicInput`. |
| 8 | MeasurementInput | ✅ | `CadInputParser.TryParseLength` / `TryParseAngle` (mm, cm, m, in, ft, `3'-6"`). |

## 3.10 Status bar (11)

| # | Spec item | Status | In the library |
| --- | --- | --- | --- |
| 1 | StatusBar | ✅ | `StatusBar`. |
| 2 | CoordinateDisplay | ✅ | `Coordinates` + `FormatCoordinates` + `UnitSuffix`. |
| 3 | SnapToggle | ✅ | `CadToggle.Snap`. |
| 4 | GridToggle | ✅ | `CadToggle.Grid`. |
| 5 | OrthoToggle | ✅ | `CadToggle.Ortho`. |
| 6 | PolarToggle | ✅ | `CadToggle.Polar`. |
| 7 | ObjectSnapToggle | ✅ | `CadToggle.ObjectSnap`. |
| 8 | WorkPlaneSelector | ✅ | `WorkPlane` / `WorkPlaneChanged`. |
| 9 | SelectionCounter | ✅ | `SelectionCount`. |
| 10 | BackgroundTaskIndicator | ✅ | `CadTask` queue with `AddTask` / `RemoveTask` / `RefreshTasks`. |
| 11 | NotificationIndicator | ✅ | `CadNotification` list with severity, count and `NotificationSelected`. |

## 3.11 Shared UI (17)

| # | Spec item | Status | In the library |
| --- | --- | --- | --- |
| 1 | IconButton | ✅ | `Icon` + icon button styles. |
| 2 | SplitButton | ✅ | `SplitButton`. |
| 3 | DropDownButton | ✅ | `RibbonDropDownButton` and the shared drop-down style. |
| 4 | SearchBox | ✅ | `SearchBox`. |
| 5 | ColorPicker | ✅ | `ColorPicker`. |
| 6 | FontPicker | ✅ | `FontPicker` — family, size, bold/italic, live preview. |
| 7 | NumericUpDown | ✅ | `NumericUpDown`. |
| 8 | UnitInput | ✅ | `UnitBox` (engineering units). |
| 9 | ToolTipControl | ✅ | `ToolTip` styles + `RibbonToolTip`. |
| 10 | ContextMenuControl | 🟡 | Menu styles ship and the project browser builds a per-kind menu; there is no generic context-menu builder. |
| 11 | NotificationToast | ✅ | `Growl` + `ToastService`. |
| 12 | ProgressIndicator | ✅ | `LoadingIndicator`, `ScrollProgressBar`, `PieControl`. |
| 13 | LoadingOverlay | ✅ | `LoadingOverlay`. |
| 14 | EmptyState | ✅ | `EmptyState`. |
| 15 | ErrorDialog | ✅ | `Dialog` (error layout). |
| 16 | ConfirmationDialog | ✅ | `Dialog` (confirm layout). |
| 17 | ColorThemeManager | ✅ | `ThemeManager` (theme, accent, tokens, DPI). |

---

## 4. Managers and services (12)

| # | Spec item | Status | In the library |
| --- | --- | --- | --- |
| 1 | ApplicationStateService | ✅ | `ApplicationStateService` — the five drafting flags, work plane and selection count, mirrored onto every attached status bar. |
| 2 | DocumentManager | ✅ | `DocumentManager` — open documents, active document, dirty state and paths. |
| 3 | ViewManager | ✅ | `ViewManager` — views per document with their own camera and view settings. |
| 4 | SelectionManager | ✅ | `SelectionManager` — `Select` / `Add` / `Remove` / `Toggle` / `Clear`, `Primary`, `SelectionChanged`. |
| 5 | CommandManager | 🟡 | `UndoRedoManager`, `TransactionManager` and `RelayCommand` cover it under three names instead of one facade. |
| 6 | PropertyManager | ✅ | `PropertyManager` — descriptors per type, validated set, change fan-out. |
| 7 | TransactionManager | ✅ | `TransactionManager` — begin/commit/rollback over `UndoRedoManager`, one step per edit, `ChangesCommitted`. |
| 8 | WorkspaceLayoutService | ✅ | `WorkspaceLayoutService` — named profiles over `DockingManager.CaptureLayout` / `ApplyLayout`. |
| 9 | ThemeService | ✅ | `ThemeManager`. |
| 10 | NotificationService | ✅ | `ToastService` (+ `DialogService` for blocking messages). |
| 11 | CameraController | ✅ | `CameraController` — target, distance, yaw/pitch, FOV, ortho/perspective, `Orbit`, `Pan`, `Zoom`, `FitTo`, `TryProject` / `Unproject`. |
| 12 | RendererBridge | ✅ | `RendererBridge` — `Adapter`, `Invalidate`, `TryPick`, `HoverChanged`, plus the camera the renderer draws with. |

## 5. Mandatory interaction flows

| Flow | Status | Note |
| --- | --- | --- |
| Select object → Properties + contextual ribbon + status bar | ✅ | `SelectionManager` fan-out: palette rebuilds, bar count updates, ribbon follows context. |
| Edit property → validate → transaction → document → renderer | ✅ | `PropertyManager` → `TransactionManager` → `DocumentManager.IsDirty` → `RendererBridge.Invalidate`. |
| Open view: project browser → view manager → workspace → viewport | ✅ | `OpenRequested` → `ViewManager` → `DocumentWorkspace.AddDocument`. |
| ViewCube → camera controller → renderer | ✅ | `ViewRequested` → `CameraController.SetOrientation` / `Orbit` → `RendererBridge.RenderRequested`. |
| Change type → related instances | ✅ | `TypeChangeRequested` goes through `PropertyManager`, so every instance of the type follows. |
| Undo/redo refreshes model and UI | ✅ | `TransactionManager.ChangesCommitted` drives the palette and the renderer. |
| Change workspace → docking restore | ✅ | `WorkspaceLayoutService` over `DockingManager` profiles. |

## 8. Acceptance criteria

| ID | Criterion | Status |
| --- | --- | --- |
| AC-001 | Ribbon switches tabs, shows panels, runs commands | ✅ |
| AC-002 | Contextual ribbon follows selection/context | ✅ |
| AC-003 | Quick access toolbar customisable and persisted | ✅ |
| AC-004 | Dock, float, tab, auto-hide, resize, restore | ✅ |
| AC-005 | Project browser: tree, search, filter, context menu | ✅ |
| AC-006 | Double-click a view node opens that view | ✅ |
| AC-007 | Properties show view, instance and common properties | ✅ |
| AC-008 | Property edit is validated, transacted, viewport refreshed | ✅ |
| AC-009 | Type selector changes the type and its instances | ✅ |
| AC-010 | Workspace supports several tabs, open/close | ✅ |
| AC-011 | Each view keeps its own camera, zoom, pan, visual state | ✅ |
| AC-012 | ViewCube drives the camera with face/edge/corner hit-testing | ✅ |
| AC-013 | View control bar updates the view and syncs with properties | ✅ |
| AC-014 | Command line with history and autocomplete | ✅ |
| AC-015 | Status bar reflects coordinates, snap, selection, progress | ✅ |
| AC-016 | Undo/redo updates the model and all dependent UI | ✅ |
| AC-017 | Theme and accent change consistently | ✅ |
| AC-018 | DPI scaling on high-density screens | ✅ |
| AC-019 | Workspace layout survives a restart | ✅ |
| AC-020 | No panel keeps business data detached from the document model | ✅ |

## Still open

Four items stay partial on purpose — each is one decision about where the boundary sits, not missing
work:

| Item | Why it stops where it does |
| --- | --- |
| 3.6 #5 DocumentTabContextMenu | The workspace exposes `CloseDocument` / `CloseOthers` / `CloseAll` and a cancellable closing event; which of those belongs on a menu is a host decision. |
| 3.7 #2 ViewportHost | `RendererBridge` is the seam a DirectX/OpenGL host plugs into. A `HwndHost` here would mean the library owning a device and a swap chain, which is exactly the dependency the package avoids. |
| 3.11 #10 ContextMenuControl | Menu styles and per-kind menus exist; a generic builder would have to model the host's commands, which the ribbon already does. |
| §4 #5 CommandManager | The behaviour ships as `UndoRedoManager` + `TransactionManager` + `RelayCommand`. A single facade is naming, not capability. |

The acceptance criteria all have a test: phases A–F end with `CadViewTests`, `CadServicesTests`,
`CadShellTests`, `DynamicInputTests` and the render-smoke guards for every new surface.
