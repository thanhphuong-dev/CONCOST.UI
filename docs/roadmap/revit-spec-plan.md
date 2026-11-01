# Revit-inspired CAD UI spec — plan to close the gap

Companion to [`revit-spec-gap.md`](revit-spec-gap.md), which counts 62 covered / 19 partial / 54 missing
items out of the 135 in TT-UI-SPEC-001 v1.0. This page is the order in which the missing items get
built, and the decisions that were taken before writing code.

## Scope

In scope: everything the spec calls a control, plus the eight services the spec lists under §4 that a
control cannot do without (selection, transactions, documents, views, camera, renderer bridge,
application state, layout profiles).

Out of scope, on purpose:

- A geometry kernel. `ViewportControl` draws the overlay it is handed (`Overlay`, grips, a
  temporary dimension, a selection rectangle) on top of a real `CameraController`; it does not
  tessellate solids, and host geometry enters through `Overlay` or `RendererBridge`.
- File formats. `DocumentManager` tracks dirty state and paths; saving is the host's job.
- A real 3D renderer. `RendererBridge` is the seam a DirectX/OpenGL host plugs into; the library ships
  a WPF fallback that draws through `OnRender`.

## Decisions taken up front

| Decision | Why |
| --- | --- |
| Surface controls render themselves (`OnRender`) instead of taking a template | Every one of them is geometry — cube faces, crosshairs, grips, tiles. A template would be an empty `Grid` and the painting would still be `OnRender`. Keeps `Generic.xaml` from growing another 2 000 lines. |
| Composed bars derive from `Panel` (`Grid`) and build their children in the constructor | They are layouts of existing controls, not new chrome. They still read their colours through `TryFindResource("CONCOST.Brush.…")`, so a theme switch repaints them. |
| Services live in `Services/`, models with no UI in `Core/` | Matches the existing split (`ToastService`, `UndoRedoManager`). |
| One static `Current` per service, plus a constructor for tests | The rest of the library already uses `Lazy<T> …Current`; a test must still be able to make its own instance. |
| New dependency properties follow the existing naming and register with `AffectsRender` | Consistency with `MiniMap`, `Scheduler`, `KanbanBoard`. |
| Each phase ends with build 0/0, the full test suite green, and a docs page + showcase demo | A phase that is only C# is not done here. |

## Phase A — Camera and navigation (17 items, §3.7 and §3.8)

The whole viewport chain, and the first thing a CAD user touches.

| Deliverable | Closes |
| --- | --- |
| `Services/CameraController.cs` — target, distance, yaw/pitch, FOV, ortho/perspective, `Orbit`, `Pan`, `Zoom`, `SetOrientation`, `FitTo`, `Project` | CameraController (4/11), AC-011 |
| `Controls/Navigation/ViewCube.cs` — isometric cube, face/edge/corner hit zones, compass ring, home button, `ViewRequested` | ViewCube, Face, Corner, Compass (3.7 #4–7), AC-012 |
| `Controls/Navigation/NavigationBar.cs` — zoom in/out/extents, pan, orbit, walk, UCS label | NavigationBar, ZoomControl, PanControl, OrbitControl, UCSControl (3.7 #8–12) |
| `Controls/Navigation/ViewControlBar.cs` — scale, detail level, visual style, shadows, crop, hide/isolate, reveal hidden | 3.8 #1–8, AC-013 |
| `Controls/Navigation/ViewportControl.cs` — render surface with grid, crosshair, snap marker, grips, temporary dimension, selection rectangle, overlay layer | ViewportControl, ViewportHost, ViewportOverlay, CrosshairControl, GridControl, SnapIndicator, SelectionOverlay, GripControl, TemporaryDimension (3.7 #1–3, #13–18) |

## Phase B — Selection and properties (8 items, §3.5 partial + services)

| Deliverable | Closes |
| --- | --- |
| `Services/SelectionManager.cs` — `Select/Add/Remove/Toggle/Clear`, `Primary`, `SelectionChanged` | SelectionManager, SelectionCounter feed |
| `Services/TransactionManager.cs` — begin/commit/rollback on top of `UndoRedoManager`, `ChangesCommitted` | TransactionManager, AC-016 |
| `Services/PropertyManager.cs` — descriptors per object type, get/set, validation, `PropertyChanged` fan-out | PropertyManager, AC-008 |
| `Controls/PropertiesPalette.cs` — header, type selector, edit-type button, search row, category grid, validation row, reset button | PropertiesPalette, Header, TypeSelector, EditTypeButton, SearchBox, ResetButton, ValidationMessage (3.5 #1–4, #16–18), AC-007/009 |
| `Controls/PropertyReferencePicker.cs` — pick another model object | PropertyReferencePicker (3.5 #15) |

## Phase C — Project browser and document workspace (10 items)

| Deliverable | Closes |
| --- | --- |
| `Controls/ProjectBrowser.cs` + `ProjectTreeNode` — node kinds, search, kind/state filter, grouping, per-kind context menu, double-click to open | 3.4 #1–10, AC-005, AC-006 |
| `Controls/Docking/DocumentWorkspace.cs` — tab host, placeholder, split panes, tile/cascade, float a view out | 3.6 #1–9, AC-019 |

## Phase D — Command line and CAD status bar (12 items)

| Deliverable | Closes |
| --- | --- |
| `Controls/CommandLine.cs` — prompt, history, autocomplete over command names and options, coordinate entry | 3.9 #1–7, AC-014 |
| `Controls/DynamicInput.cs` — the box that follows the cursor for a length, an angle or a coordinate, with the same parser as the command line | 3.9 #6, AC-014 |
| `Controls/CadStatusBar.cs` — coordinate readout, snap/grid/ortho/polar/osnap toggles, work plane selector, selection counter, background task and notification indicators | 3.10 #2–11, AC-015 |

## Phase E — Services and shell leftovers (8 items)

| Deliverable | Closes |
| --- | --- |
| `Services/DocumentManager.cs`, `Services/ViewManager.cs`, `Services/ApplicationStateService.cs` | §4 #1–3 |
| `Services/WorkspaceLayoutService.cs` — named profiles over `DockingManager.CaptureLayout` | §4 #8, AC-019 |
| `Services/RendererBridge.cs` — invalidate/redraw seam for a hardware renderer | §4 #12, AC-020 |
| `Controls/ApplicationTitleBar.cs`, `Controls/ApplicationInfoBar.cs` | 3.1 #2, #5 |
| `Controls/Ribbon/RibbonBackstageView.cs` | 3.2 #15 |
| `Controls/FontPicker.cs` | 3.11 #6 |

## Phase F — Flows and acceptance

Wire the phases together in one sample — select → properties → edit → transaction → viewport refresh,
project browser → view → workspace → viewport, ViewCube → camera — and assert every acceptance
criterion that is still ❌ or 🟡: AC-005, 006, 008, 009, 011, 012, 013, 014, 015, 016, 019, 020.

## Verification per phase

1. `dotnet build CONCOST.UI.sln -c Release` → 0 warnings, 0 errors.
2. `dotnet test tests/CONCOST.UI.Tests` → all green; new behaviour gets a unit test, and every new
   surface element gets a render-smoke entry so a template or `OnRender` that paints nothing fails.
3. `python docs/tools/check-xaml.py` and `node docs/tools/build-index.js` after the docs pages land.

## Status

Phases A–F are built and the tree is green. What is deliberately still open, and why:

| Item | State | Note |
| --- | --- | --- |
| 3.6 #5 DocumentTabContextMenu | 🟡 | `DocumentWorkspace` raises the closing event and the tab strip has a close button, but no menu is shipped — a host that wants Close/Close Others/Close All builds it from `CloseOthers` / `CloseAll`. |
| 3.7 #2 ViewportHost | 🟡 | `RendererBridge.Adapter` (`IRendererAdapter`) is the invalidate/pick seam for a DirectX/OpenGL renderer. There is no shipped `HwndHost` element, because the library cannot own a device. |
| 3.11 #10 ContextMenuControl | 🟡 | Menu styles and per-kind menus exist (the project browser builds one); there is no generic context-menu builder. |
| §4 #5 CommandManager | 🟡 | `UndoRedoManager`, `TransactionManager` and `RelayCommand` cover the behaviour under three names instead of one facade. |

Counted against the 135 items of TT-UI-SPEC-001 v1.0 the result is **132 covered (98%), 3 partial, 0 missing**,
plus 11 of the 12 services in full — see [`revit-spec-gap.md`](revit-spec-gap.md) for the item-by-item table.
