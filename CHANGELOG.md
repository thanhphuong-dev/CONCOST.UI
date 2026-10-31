# Changelog

All notable changes to CONCOST.UI are documented in this file.
The format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/) and the project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Changed
- **`DockingManager` now follows the AvalonDock model.** The first version treated the four edges as four slots and stacked every panel docked to one edge, each with its own slice. That is not how an IDE behaves, so the layout was rebuilt around **pane groups**:
  - **Tabs.** Panels docked to the same edge form one group and show as tabs along that edge (bottom for side and bottom edges, top for the document area). The selected tab is `ToolWindow.IsActive`, and a pane's own header hides while it has siblings, because the tab already names it. Setting `IsActive` from code switches the tab exactly like a click: the manager listens to `ToolWindow.Activated` and clears the rest of the group.
  - **Proportional sizing.** A pane keeps its share of the window when the window is resized, the way a `GridLength` star column does, instead of staying at a fixed pixel width. `IsProportional = false` pins it back to pixels. The scale factor is applied in measure and the accounted-for size is recorded there, so the second pass does not scale again and the panes cannot run away.
  - **Five drop zones.** A drag now resolves the edge from the **pane under the pointer** (25% bands on each side, the rest is the centre) instead of four global bands on the manager, and the dashed hint draws the region the pane will actually occupy after the drop.
  - **Layout persistence.** `CaptureLayout()` / `ApplyLayout(string)` round-trip each pane's edge, size, auto-hide state and selected tab through a small versioned text format (`CONCOST-DOCK-1`). Panes are matched by `Header`, so an unknown pane in an old snapshot is skipped rather than corrupting the layout.
  - `Group(DockSide)` exposes the panes sharing an edge; `DockTabStrip` is the tab strip control and is public, so a custom layout can use it.

### Added
- **`AnimatedMeshGradient` — a mesh, not an aurora.** Four colour blobs that oscillate around fixed anchors and blend into each other. The radius is computed from the **longer** side of the control: with the shorter one a wide hero panel shows white gaps between the anchors instead of a mesh. Drawn with radial gradients that fade to transparent — no `BlurEffect`, so it is cheap at full window size.
- **`AnimatedGradientBorder` — a card with a rotating gradient ring.** The rotation is a `DoubleAnimation` on a `RotateTransform` attached to the brush, so WPF runs it on the render thread and the UI thread is untouched. When motion is off the animation is cleared and the ring keeps its first frame.
- **`FrostedGlass` — a real frosted panel.** WPF has no backdrop filter, so the control paints a `VisualBrush` of the element behind (`Source`) through a `BlurEffect`, with the viewbox set to the panel's own rectangle **in the source's coordinates** — the blur lines up with what is behind it instead of stretching the whole backdrop into the panel.
- **`CursorSpotlight` — dim everything but a circle.** Not hit-testable, yet it follows the pointer because `Mouse.GetPosition` reads the current position without needing events; `FollowCursor="False"` plus `SpotlightCenter` hands control to a tour script. Painted as one rectangle with an absolute radial gradient and `SpreadMethod=Pad` — drawing only an ellipse gives a dark ring in the middle, the opposite of a spotlight.
- **`StaggerAnimation` — a list that arrives in a wave.** An attached behaviour: the delay comes from the item's position, so it works with any panel and any template. Containers are tracked by reference so a virtualising panel does not replay the entrance on every scroll, and `MaxItems` caps the delay so a long list does not take seconds to finish.
- **`SharedElementTransition` — grow out of where it was.** Animates only a transform: the element is already laid out in its final place and is scaled/shifted back to the old rectangle for the first frame, then eased to identity. No layout thrash, and the element stays interactive throughout. `RunFrom(target, source)` does the coordinate conversion.
- **`SearchableTreePicker` — a tree with a search box in front.** Typing narrows the tree to the nodes that match **and the branches that lead to them**, so the result still shows a path instead of a flat list of leaves. Matching goes through `CommandPaletteText.Normalize`, so `dam d1` finds "Dầm D1". `SelectionMode="Multiple"` puts a checkbox on every row; `SelectAllVisible()` takes only what the filter left on screen; `SelectedItems`, `SelectedCount` and `SelectionChanged` expose the pick. `DisplayPath` accepts a dotted path, so a node can show `Info.Label` without a wrapper type.
- **`FileDropZone` — a drop area for files.** `Filter` is checked **while the drag is still in the air**, so the zone only lights up for files it will take, and `IsDragRejected` drives the red state. `FilesDropped` reports the accepted paths, `AddFiles` applies the same filter to files the host picked from its own dialog, and `BrowseRequested` keeps the dialog out of the library.
- **`DropTargetHighlight` — a hover outline for any element.** An attached property that draws a dashed outline with an **adorner**, so it sits above the content and never changes the layout. It only reports the hover; accepting the drop stays the element's job through `DragDropEffects`.
- **`DragGhost` — a caption that follows the pointer.** A transparent, topmost, hit-test-invisible window offset 14px from the cursor, with the DPI conversion handled by `Move(visual, point)` so the ghost lands where the pointer is.
- **`DockPaneGroup` — the layout is now a tree, not four slots.** A group splits its area between its children by an attached `Proportion`, and a child may be another group, so a layout can express "the left edge split into two rows, the top row tabbed" — the shapes an IDE produces and the four-slot model could not describe. `Mode="Split"` divides the space (with a draggable splitter between each pair of children), `Mode="Tabs"` shows the children as tabs and keeps exactly one active. Each group draws and hit-tests its own splitters, so nesting can go as deep as the caller likes without the manager knowing about it.
- **Floating windows.** Dragging a header clear of the manager tears the panel out into a `Window`; closing that window docks it back to the same edge at the same size. `Float(pane)` does the same from code and returns the window. A floating panel has `CanAutoHide` switched off — there is no edge to pin it to — and gets it back when it docks.
- `DockingManager.AllPanes` walks the whole tree, so `CaptureLayout` / `ApplyLayout` cover nested panels; a nested panel is matched by `Header` and keeps its place inside its group.
- **Tab dragging.** A tab can be dragged: dropped back on its own strip it reorders, dropped on another edge the panel joins that group, dropped outside the window it tears out into a floating window. `MovePane` / `DockPaneGroup.MoveChild` / `DockAt` / `PreviewDrop` / `EndDropPreview` are public, so a custom drag surface gets the same behaviour without reimplementing the geometry.
- **Auto-hide works for nested panels.** A panel inside a group cannot be hidden where it stands, so the manager pulls it out to the edge strip when `IsAutoHidden` flips; pinning it back docks it to that edge. Groups now forward their children's drag, close, activate and auto-hide events up the tree, so a panel three levels deep behaves like one on an edge.
- **`TreeDataGrid` — a hierarchical `DataGrid`.** Bind `RootItems` and the grid flattens the tree into rows; each row is **the item itself**, not a wrapper, so every column keeps binding straight to the item's properties and a caller never has to project a view model per level. `HierarchyPath` names the property shown in the generated column (the `DisplayMemberPath` idea — without it the cell falls back to `ToString()`, which is rarely what anyone wants), and `HierarchyContentTemplate` takes over when a single line of text is not enough. `ChildrenPath` defaults to `Children`, `IndentSize` sets the per-level indent, `ExpandAll` / `CollapseAll` / `ExpandTo` / `ToggleNode` / `IsNodeExpanded` drive it from code, and `VisibleItemCount` reports what is actually on screen. Expansion state is keyed by **object reference**, so two nodes with equal values stay separate branches and the state survives every rebuild of the flat list.
- **`MultiSelectDataGrid` — checkbox selection over `DataGrid`.** A generated checkbox column plus a tri-state header box: `IsAllSelected` reads and writes, `SelectedCount` and `SelectedItemsList` report the selection, and `SelectEverything` / `ClearEverything` / `InvertSelection` / `ToggleItem` / `IsItemSelected` do the rest. Clicking the cell toggles without a focus dance, and the checkbox itself is not hit-testable, so the click always lands on the row.
- **`TreeMap` — squarified share-of-total.** `ItemsSource` plus `ValuePath` / `LabelPath`, a `Palette` of brushes, `Gap`, `CornerRadius`, `ShowLabels`, `ShowValues` and `ValueFormat`. Tiles are laid out with the RowWorst minimisation (largest value first), `HitTest(Point)` answers what is under the pointer, and `ItemInvoked` carries the entry — label, value, share and bounds. `Entries` lays out lazily on access, so the control can be read in tests and in code without waiting for a render pass.
- **The docking cluster — `DockingManager`, `ToolWindow`, `DockTabStrip`, `AutoHidePane`, `DockSplitter`, `DockPreview`.** Four edge regions plus a centre; panels docked to one edge form a group shown as tabs, the centre takes what is left, and the layout is described under *Changed* above. `IsAutoHidden` pins a pane into an edge strip and `ToggleFlyout` slides it out over the centre, closing the others. `Closing` is cancellable, `Closed`, `PaneDocked` and `PaneClosed` let a host persist or log the layout, and `Dock(pane, side)` moves a pane from code.
- **`DocumentTabControl` / `DocumentTabItem` — document tabs.** Icon, caption, unsaved dot (`IsDirty`, which the control never clears because saving belongs to the host) and a close button. Closing is a **request**: `Close()` raises the bubbling `CloseRequested` event and the control catches it with a class handler, so the button, a context menu and a shortcut all take the same path and the host can veto. `ShowCloseButtons` and per-tab `CanClose` turn the buttons off; `CloseTab`, `CloseOtherTabs` and `CloseAllTabs` drive it from code.
- **`StatusBar` / `StatusBarItem` / `StatusBarPanel`.** Cells that hug either edge of the strip: left cells stack from the left, `Alignment="Right"` cells from the right, and the gap in the middle absorbs the slack — no `Grid` with a `*` column needed. A cell with a `Command` is clickable and switches the cursor; a cell without one keeps the default. `IsSeparator` draws a hairline, and `Content` wins over `Text` when a cell needs a control rather than a caption.
- **`CommandBar` / `CommandBarItem` and `QuickAccessToolbar` / `QuickAccessItem`.** A single-row command strip that pushes what does not fit into a "more" menu instead of clipping it, and the icon-only variant for everyday commands. The control never guesses what fits: `OverflowPanel` measures during layout and reports back, so `HasOverflow` and `OverflowItems` always match what is actually hidden, and `IsOverflowOpen` is two-way. `Invoke(item)` runs a command through the same path as the button.
- **`OverflowPanel`** — the panel behind both toolbars: lays children out in a row and stops when the width runs out, arranging the rest with an empty rect so the visible count never flickers between passes.
- 25 new tests (`DataControlsTests`, `DockingTests`, `DocumentTabTests`, `ShellControlsTests`), plus 45 more for the docking rework, the phase 4 utilities and the phase 5 effects; the suite is now 389.

### Fixed
- **`DockingManager.Float` crashed the app when called twice on the same panel.** The first call reparented the panel into a `Border` inside its window; the second call only removed it from `Children` — where it no longer was — and then set `Border.Child`, so WPF threw *"Specified element is already the logical child of another element"* and the process died. `Float` is now idempotent: it keeps a panel→window map and brings the existing window forward. `Detach` takes a panel out of whatever holds it (a group, the manager, a floating host), and `Dock` closes the floating window first for the same reason.
- A `DockPaneGroup` in tabs mode ping-ponged between two layout passes for ever: `ComputeRects` collapsed the inactive tab while `ArrangeOverride` forced every child back to `Visible`, so each pass undid the other and the process died with a stack overflow. Visibility is now owned by the measure pass alone.
- **`DockingManager` splitter bands were not hit-testable.** A `Panel` paints nothing, so the 6px band between two panes was empty space: mouse events passed through to whatever was behind, the resize cursor never appeared and dragging a splitter did nothing. `HitTestCore` now claims the bands (and only the bands), and `OnRender` draws a hairline in each one — which also makes the divider visible for the first time. `SplitterBrush` overrides the colour; empty takes `CONCOST.Brush.Border`.
- **Drag-to-redock never started.** `ToolWindow` captured the mouse on itself but subscribed `MouseMove` / `MouseLeftButtonUp` on the header bar. Once an element captures, the mouse route begins at the capture element and bubbles up, so a handler on a *child* of the capture element stops running — the drag never passed its 4px threshold and `DragStarted` was never raised. The move and release handlers now sit on the `ToolWindow`, and a `LostMouseCapture` handler clears the flag so a cancelled drag cannot stick.
- **The auto-hide flyout was painted under the centre pane.** Child order is z-order in a `Panel`, and the flyout kept its declaration position, so the document area drew over it and opening a flyout looked like nothing happened. `ToggleFlyout` now moves the pane to the top of the child list, then puts the drop preview back above it.
- `ToolWindow.IsActive` was never set, so the active-panel header highlight could not appear; the manager now marks the panel under the click and clears the others.
- `ToolWindow` drew the pane's **body** in its header bar instead of `Header`, so every tool window showed an icon and a clipped copy of its content.
- The pin and close buttons in the `ToolWindow` header, the close button on a `DocumentTabItem` and the buttons in `QuickAccessToolbar` rendered nothing: they used an icon-only button style whose template reads the attached `IconElement.Kind`, while the icon was passed as content. They now set `IconElement.Kind`, which is what that style actually draws.
- The `CommandBar` and `QuickAccessToolbar` overflow chevrons were `ToggleButton`s wearing a `Button` style — a target-type mismatch that threw while the template was being applied. They now use the `ToggleButton` ghost style.
- The `TreeDataGrid` expander rendered as a solid accent block with an invisible chevron; it now draws a 10px chevron in the secondary text colour, down when a branch is open and right when it is closed.
- `SnowCanvas_RendersAfterLayout` asserted a painted fraction tight enough to fail on an unlucky seed (0.18% against a 0.20% floor). Flake positions are random, so the floor is now 0.10% — the test is asking "did the template paint anything", not "how much".

## [1.1.0] - 2026-10-31

### Added
- **`CommandPalette` — the Ctrl+K command bar.** A search box over a filtered command list, driven by the keyboard: type to filter, `↑`/`↓` to move, `Enter` to run, `Esc` to close. It draws its own scrim, so the host drops it into a `Grid` and toggles `IsOpen`; there is no popup to manage and no focus dance to write. `CommandPaletteItem` carries the title, subtitle, shortcut, icon, category and the `ICommand` to run — plain properties, so a host can project its own commands without a view model per entry. Search matches every token independently across title, subtitle, category and shortcut, and **ignores diacritics**: `tong quan` finds "Tổng quan", `duong kinh` finds "Đường kính", which is the difference between usable and unusable in a Vietnamese-first library. The highlight follows the filtered list, so it can never point at a row that was just filtered away, and the list height snaps to whole rows (`MaxListHeight`, default 276 = six rows) so the overlay never shows half a command. `ItemInvoked`, `Opened`, `Closed`, `CloseOnInvoke`, `InvokeItem` and `MoveSelection` are all public.
- **`RadialGauge` — circular KPI readout.** Track arc, value arc, optional tick marks and a formatted number in the middle (`ValueFormat`, `Suffix`, `Label`). `StartAngle` / `SweepAngle` open the ring at the bottom by default (135°/270°); `Value` clamps to `Minimum`–`Maximum` and eases into place with a cubic ease, or jumps when `IsAnimated` is false or motion is off. Drawn in `OnRender` with round caps, so it stays crisp at any size and at any DPI.
- **`ParticleField` — ambient particle backdrop.** Drifting particles for hero panels, splash screens and empty states, with `Direction`, `Speed`, `Spread`, `ParticleOpacity` and an optional `ConnectDistance` that draws the constellation lines. Particles wrap around the edges so the field never empties, `Seed` makes the layout reproducible, and the field rebuilds itself when its host is resized. Built on `AnimatedEffectBase`, so it stops with the rest of the library when Windows animations are off.
- **`PageTransitionHost` — animated content swap.** Set `Content` and the new page fades (`Fade`), slides in from either side or from above/below, or scales up (`Scale`); `Duration` and `Offset` tune it and `Play()` replays the entrance. The animation runs on the host, so it works with any content and needs no storyboard per page. Skipped entirely when the control is not loaded, `IsTransitionEnabled` is false, or motion is off.
- **Strong-name signing (§100).** `build/CONCOST.UI.snk` plus `SignAssembly` in `Directory.Build.props`: the library, the tests and the showcase now ship with a public key token (`5aec1392da75741c`).
- **CI publishes the package.** The workflow writes the `.nupkg` to `artifacts/packages`, uploads it as a build artifact, and pushes to nuget.org on a `v*` tag when a `NUGET_API_KEY` secret exists — a pull request from a fork can never reach the feed.
- **Visual regression and automated UI tests (§96, §97).** `RenderSmokeTests` renders each control offscreen through `RenderTargetBitmap` and asserts the pixels are not blank, which catches the failure this repository actually hit — a template that throws while being applied and leaves an empty card. `KeyboardInputTests` routes real `KeyEventArgs` through a shown window, so the palette's arrows, `Enter`, `Esc` and the focus hand-off to the search box are tested through the same path a user takes.
- **`AppTheme.Auto` — a theme that follows the host window.** `ThemeManager.ApplyTheme(root, AppTheme.Auto)` reads the background of the window the root sits in and picks Light or Dark from its brightness, so an add-in can match Revit or AutoCAD without owning an `Application`. Detection walks up from the root to the first element that actually paints a background (a plugin panel is usually transparent, the host's panel is not) and falls back to the Windows setting when it finds none. `ThemeManager.DetectTheme(root)` answers the same question without applying anything, and `ThemeManager.RefreshAutoTheme(root)` re-probes after the host switches its own theme — roots given an explicit theme are left alone.
- **`ComboBoxHelper` — a searchable, still read-only ComboBox.** `IsSearchEnabled` puts a search box at the top of the drop-down, `SearchText` holds the term (two-way) and `SearchPlaceholder` is its hint. The filter is applied to the ComboBox's own `Items`, so the bound `ItemsSource` is never modified and two combo boxes can share one source; the selection box stays non-editable, so the user can only pick a value that exists. `ResetSearch` clears the term and is called automatically when the drop-down closes.
- **DataGrid panels — filter, search, columns, summary.** Four controls that attach to an existing `DataGrid` instead of replacing it, so a table picks up data-tool behaviour by pointing `TargetGrid` at it.
- `DataGridFilterPanel` — one box per column, live case-insensitive `Contains` filtering. Each box is as wide as the column it filters and follows the grid's column order, resizing and horizontal scroll, so the row never drifts out of alignment. Dotted binding paths (`Material.Grade`) are read correctly. `Filters`, `ApplyFilters()`, `ClearFilters()` and `FiltersApplied` expose it to the host.
- `DataGridSearchPanel` — a single box that scans every visible column; hidden columns are skipped, so a table that hides internal columns does not leak them through search. `SearchText` is two-way and `IsSearching` drives a host-side hint.
- `DataGridColumnChooser` — a drop-down that lists the grid's columns and toggles their visibility. `IsOpen`, `Columns`, `ShowAll()` and `HideAll()` are public, columns are matched by reference (not by caption, so duplicate headers stay correct), and the list rebuilds when columns change and whenever the drop-down opens.
- `DataGridSummary` — a summary row under the grid: `Sum` for numeric columns, row count for text ones. It mirrors the grid's column widths, display order, row-header width and horizontal scroll offset, adds `Aggregate` (`Auto` / `Sum` / `Average` / `Min` / `Max` / `Count`), `DisplayFormat`, `RowHeight` and the attached `DataGridSummary.SetAggregateOnColumn` override, and aggregates the **filtered** view so the numbers match what is on screen.
- The filter row and the search box now **compose**: both register a predicate on the grid's collection view, so a row has to satisfy both instead of the last panel winning.
- `DataGridFeaturePanelsTests` — 21 tests: filtering through the default view (not just an `ICollectionView` `ItemsSource`), dotted paths, clear/restore, hidden-column search, filter + search composition, chooser hide/show/refresh/popup, summary aggregates, per-column override, culture-aware decimals and width mirroring.

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
- **Ribbon — host tuning properties.** `SelectedTabId` selects a tab by `Id` and stays in sync with `SelectedIndex` both ways, so reordering tabs cannot point at the wrong one. `IsGroupCollapseEnabled` drops the last rung of the collapse ladder: groups still shrink, but never become a single drop-down button, and switching it off re-opens the groups that were just folded. `IsQuickAccessCustomizable` locks the strip down — pinned commands keep running, the chevron and the customise panel go away. `IsKeyTipEnabled` hands the Alt gesture back to the host, which Revit and AutoCAD both need. `ShowBorder` removes the shell outline for a ribbon docked into a panel, `EmptyContent` fills the content area when there is no tab, and the previously unused `Ribbon.ShowGroupLabels` now really drops the group caption row. `RibbonQuickAccessToolBar.IsCustomizable` is the toolbar-side switch behind the last of those.
- **Ribbon — narrow windows.** The tab strip now lives in its own clipping viewport, so it can no longer paint over the quick-access commands, and a `»` button appears as soon as the tabs stop fitting; it lists every tab so none becomes unreachable. `Ribbon.IsTabStripOverflowing` exposes that state, and a contextual band steps aside while the strip is clipped instead of showing up as a stray coloured block.
- `RibbonMenuBar.ShowDivider` — the hairline under the bar. The bar itself now stretches to the full ribbon width instead of hugging its items, so the line no longer stops halfway across the header.
- `RibbonExtendedPropertiesTests` — 14 tests covering selection by id, the collapse switch in both directions, the locked-down strip, the outline, the caption row, the empty state and the automation-free parts of each.
- `RibbonMenuBar` and `RibbonMenuBarItem` — a standalone horizontal menu bar that reuses `RibbonMenuItem`, `RibbonMenu` and `RibbonMenuSeparator` for its dropdowns. Click to open, hover to switch while a menu is open, Left/Right arrow keys to navigate, Escape to close all.

### Documentation
- Four new wiki pages, Vietnamese and English: `CommandPalette` (scrim, Ctrl+K wiring, diacritic-insensitive search, row snapping), `RadialGauge` (percentage and reversed-scale gauges, tick colour), `ParticleField` (backdrop, constellation mode, particle budget) and `PageTransitionHost` (transitions, navigation direction, `IsTransitioning`). The wiki is now 113 pages with 796 generated property rows; `check-xaml.py` reports 0 issues over 119 tags.
- `docs/roadmap/` now matches what shipped: the effects roadmap marks phase 3 done, the ribbon plan records 22/22 controls in 1.1.0 instead of the planned 1.1/1.2/1.3 split, and `roadmap-v3.md` annotates §96, §97, §100, §101, §107.13 and the version roadmap with the real state — including what is *not* done (pixel-diff baselines, Authenticode).

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
- Nine nullable warnings that only appeared on `net48` (`CS8602` / `CS8604` in `AutoCompleteTextBox`, `CheckComboBox`, `DataGridGroupPanel`, `RebarParser` and `Spreadsheet`). The cause is not the code: the .NET Framework reference assemblies carry no `[NotNullWhen(false)]` on `string.IsNullOrEmpty` / `IsNullOrWhiteSpace`, so flow analysis cannot narrow there even though it does on .NET 8. The guards are now pattern checks (`x is { Length: > 0 }`), which narrow on every target. The build is warning-free on all three TFMs.
- `RollLoading` and `CycleLoading` could not be instantiated at all: their storyboards were declared in the template's inner `Grid.Resources` while the trigger referenced them with `{StaticResource …}`, which resolves against the template's own scope, so applying the template threw `XamlParseException: Cannot find resource named 'RollStoryboard'`. Both storyboards now live in `ControlTemplate.Resources`. The fault stayed hidden because no demo opens those two spinners; it surfaced the moment the showcase inspector materialised the whole theme dictionary to list style keys, which took the application down before its window appeared. `LoadingTemplateTests` (2 tests) applies both templates and resolves the storyboards.
- The Showcase crashed on a theme switch after opening the Ribbon page: `TextGlyphIcon` froze the `DrawingGroup` that draws the bold/italic/underline glyphs, and freezing a group freezes every `Freezable` inside it — including the one `SolidColorBrush` shared by every ribbon icon. The next theme change then died with “Cannot set a property on object '#FF0F172A' because it is in a read-only state”. Only the glyph geometries are frozen now, so the shared ink brush stays live and the icons follow the theme.
- Switching away from the `Brutal` preset pinned `CONCOST.Brush.Background`, `Surface`, `Border` and the text brushes in the application's own resource dictionary. `ResourceDictionary.Contains` also looks inside merged dictionaries, so the "original" value saved before Brutal was the light theme's brush, and restoring it wrote that value back at application level — after which Light/Dark could no longer change those brushes. The snapshot now only records keys the application really owns, and anything else is removed so the theme dictionary shows through again.
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

[Unreleased]: https://github.com/thanhphuong-dev/CONCOST.UI/compare/v1.1.0...HEAD
[1.1.0]: https://github.com/thanhphuong-dev/CONCOST.UI/releases/tag/v1.1.0
[1.0.1]: https://github.com/thanhphuong-dev/CONCOST.UI/releases/tag/v1.0.1
[1.0.0]: https://github.com/thanhphuong-dev/CONCOST.UI/releases/tag/v1.0.0
