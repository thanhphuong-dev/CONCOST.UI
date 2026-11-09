# CONCOST.UI — API Reference

Generated from the built assembly by `docs/tools/ApiGen` — do not edit by hand.

**474 public types** · 378 classes · 83 enums · 6 structs · 7 interfaces.

Dependency properties, attached properties and routed events are the XAML surface; the
CLR events and public methods are the code-behind surface. Summaries come from the XML
documentation the build emits next to the assembly.

## Contents

- [CONCOST.UI.AttachedProperties](#concostuiattachedproperties) — 19 types
- [CONCOST.UI.Controls](#concostuicontrols) — 293 types
- [CONCOST.UI.Controls.Canvas](#concostuicontrolscanvas) — 4 types
- [CONCOST.UI.Controls.Charts](#concostuicontrolscharts) — 8 types
- [CONCOST.UI.Controls.Dashboard](#concostuicontrolsdashboard) — 7 types
- [CONCOST.UI.Controls.Engineering](#concostuicontrolsengineering) — 9 types
- [CONCOST.UI.Controls.Layout](#concostuicontrolslayout) — 3 types
- [CONCOST.UI.Controls.Navigation](#concostuicontrolsnavigation) — 2 types
- [CONCOST.UI.Controls.Ribbon](#concostuicontrolsribbon) — 51 types
- [CONCOST.UI.Controls.Windows](#concostuicontrolswindows) — 1 types
- [CONCOST.UI.Core](#concostuicore) — 1 types
- [CONCOST.UI.Core.Converters](#concostuicoreconverters) — 6 types
- [CONCOST.UI.Core.Helpers](#concostuicorehelpers) — 2 types
- [CONCOST.UI.Core.MVVM](#concostuicoremvvm) — 4 types
- [CONCOST.UI.Core.Settings](#concostuicoresettings) — 2 types
- [CONCOST.UI.Core.UndoRedo](#concostuicoreundoredo) — 3 types
- [CONCOST.UI.Icons](#concostuiicons) — 3 types
- [CONCOST.UI.Services](#concostuiservices) — 43 types
- [CONCOST.UI.Themes](#concostuithemes) — 4 types
- [CONCOST.UI.Themes.Tools](#concostuithemestools) — 8 types
- [XamlGeneratedNamespace](#xamlgeneratednamespace) — 1 types

---

## CONCOST.UI.AttachedProperties

### BorderElement *(static class)*

Controls border styling, corner radius, circular pill, and hover borders.
- **Dependency properties (4):** `CornerRadius`, `Circular`, `HoverBorderBrush`, `HoverBackground`
- **Attached properties:** `Circular`, `CornerRadius`, `HoverBackground`, `HoverBorderBrush`
- **Methods (8):** `GetCornerRadius(DependencyObject obj)` · `SetCornerRadius(DependencyObject obj, CornerRadius value)` · `GetCircular(DependencyObject obj)` · `SetCircular(DependencyObject obj, bool value)` · `GetHoverBorderBrush(DependencyObject obj)` · `SetHoverBorderBrush(DependencyObject obj, Brush value)` · `GetHoverBackground(DependencyObject obj)` · `SetHoverBackground(DependencyObject obj, Brush value)`

### ButtonElement *(static class)*

Advanced button features: Loading spinner, Ripple wave effect, Badge counters.
- **Dependency properties (3):** `IsLoading`, `LoadingText`, `Badge`
- **Attached properties:** `Badge`, `IsLoading`, `LoadingText`
- **Methods (6):** `GetIsLoading(DependencyObject obj)` · `SetIsLoading(DependencyObject obj, bool value)` · `GetLoadingText(DependencyObject obj)` · `SetLoadingText(DependencyObject obj, string value)` · `GetBadge(DependencyObject obj)` · `SetBadge(DependencyObject obj, object value)`

### ComboBoxHelper *(static class)*

Turns a plain into a searchable list without making it editable: a search box appears at the top of the drop-down and narrows the items as the user types, while the selection box itself stays read-only.
- **Dependency properties (3):** `IsSearchEnabled`, `SearchText`, `SearchPlaceholder`
- **Attached properties:** `IsSearchEnabled`, `SearchPlaceholder`, `SearchText`
- **Methods (7):** `GetIsSearchEnabled(DependencyObject obj)` · `SetIsSearchEnabled(DependencyObject obj, bool value)` · `GetSearchText(DependencyObject obj)` · `SetSearchText(DependencyObject obj, string value)` · `GetSearchPlaceholder(DependencyObject obj)` · `SetSearchPlaceholder(DependencyObject obj, string value)` · `ResetSearch(ComboBox combo)`

### DataGridElement *(static class)*

Attached properties for DataGrid: Row spacing, row margin, divider lines, and Header/Cell alignment.
- **Dependency properties (7):** `RowSpacing`, `ShowRowDivider`, `HeaderHorizontalContentAlignment`, `CellHorizontalContentAlignment`, `TextHeader`, `HeaderAlignment`, `CellAlignment`
- **Attached properties:** `CellAlignment`, `CellHorizontalContentAlignment`, `HeaderAlignment`, `HeaderHorizontalContentAlignment`, `RowSpacing`, `ShowRowDivider`, `TextHeader`
- **Methods (14):** `GetRowSpacing(DependencyObject obj)` · `SetRowSpacing(DependencyObject obj, double value)` · `GetShowRowDivider(DependencyObject obj)` · `SetShowRowDivider(DependencyObject obj, bool value)` · `GetHeaderHorizontalContentAlignment(DependencyObject obj)` · `SetHeaderHorizontalContentAlignment(DependencyObject obj, HorizontalAlignment value)` · `GetCellHorizontalContentAlignment(DependencyObject obj)` · `SetCellHorizontalContentAlignment(DependencyObject obj, HorizontalAlignment value)` · `GetTextHeader(DependencyObject obj)` · `SetTextHeader(DependencyObject obj, string value)` · `GetHeaderAlignment(DependencyObject obj)` · `SetHeaderAlignment(DependencyObject obj, Nullable<HorizontalAlignment> value)` · `GetCellAlignment(DependencyObject obj)` · `SetCellAlignment(DependencyObject obj, Nullable<HorizontalAlignment> value)`

### DragDropHelper *(static class)*

- **Dependency properties (3):** `IsDragSource`, `IsDropTarget`, `DragDropGroup`
- **Attached properties:** `DragDropGroup`, `IsDragSource`, `IsDropTarget`
- **Methods (6):** `GetIsDragSource(DependencyObject obj)` · `SetIsDragSource(DependencyObject obj, bool value)` · `GetIsDropTarget(DependencyObject obj)` · `SetIsDropTarget(DependencyObject obj, bool value)` · `GetDragDropGroup(DependencyObject obj)` · `SetDragDropGroup(DependencyObject obj, string value)`

### DragPreviewAdorner

Floating semi-transparent visual preview adorner that follows the mouse cursor while dragging, giving the user tactile feedback of holding and moving the item.

Inherits `System.Windows.Documents.Adorner`.
- **Methods (1):** `UpdatePosition(Point point)`

### IconElement *(static class)*

Attaches vector icons, placement, size, and secondary state icons.
- **Dependency properties (3):** `Kind`, `IconSize`, `Placement`
- **Attached properties:** `IconSize`, `Kind`, `Placement`
- **Methods (6):** `GetKind(DependencyObject obj)` · `SetKind(DependencyObject obj, IconKind value)` · `GetIconSize(DependencyObject obj)` · `SetIconSize(DependencyObject obj, double value)` · `GetPlacement(DependencyObject obj)` · `SetPlacement(DependencyObject obj, Dock value)`

### InfoElement *(static class)*

Provides input metadata, placeholders, prefix/suffix labels, clear button, and character limit.
- **Dependency properties (9):** `Placeholder`, `Title`, `TitlePlacement`, `Required`, `Prefix`, `Suffix`, `ShowClearButton`, `IsClearButtonAlwaysVisible`, `ShowCharacterCount`
- **Attached properties:** `IsClearButtonAlwaysVisible`, `Placeholder`, `Prefix`, `Required`, `ShowCharacterCount`, `ShowClearButton`, `Suffix`, `Title`, `TitlePlacement`
- **Properties (1):** `ClearCommand: RoutedUICommand`
- **Methods (18):** `GetPlaceholder(DependencyObject obj)` · `SetPlaceholder(DependencyObject obj, string value)` · `GetTitle(DependencyObject obj)` · `SetTitle(DependencyObject obj, string value)` · `GetTitlePlacement(DependencyObject obj)` · `SetTitlePlacement(DependencyObject obj, Dock value)` · `GetRequired(DependencyObject obj)` · `SetRequired(DependencyObject obj, bool value)` · `GetPrefix(DependencyObject obj)` · `SetPrefix(DependencyObject obj, object value)` · `GetSuffix(DependencyObject obj)` · `SetSuffix(DependencyObject obj, object value)` · `GetShowClearButton(DependencyObject obj)` · `SetShowClearButton(DependencyObject obj, bool value)` · `GetIsClearButtonAlwaysVisible(DependencyObject obj)` · `SetIsClearButtonAlwaysVisible(DependencyObject obj, bool value)` · `GetShowCharacterCount(DependencyObject obj)` · `SetShowCharacterCount(DependencyObject obj, bool value)`

### InsertionAdorner

High-performance Adorner that renders a visual insertion line between items during drag and drop. Uses pre-frozen hardware brushes and smart state caching to eliminate frame jitter and visual tearing.

Inherits `System.Windows.Documents.Adorner`.
- **Properties (1):** `IsBottom: bool`

### PasswordBoxHelper *(static class)*

Mirrors a 's password into a bindable attached property.
- **Dependency properties (3):** `Password`, `Attach`, `PasswordLength`
- **Attached properties:** `Attach`, `Password`, `PasswordLength`
- **Methods (6):** `GetPassword(DependencyObject dp)` · `SetPassword(DependencyObject dp, string value)` · `GetAttach(DependencyObject dp)` · `SetAttach(DependencyObject dp, bool value)` · `GetPasswordLength(DependencyObject dp)` · `SetPasswordLength(DependencyObject dp, int value)`

### ProgressBarElement *(static class)*

Attached properties for ProgressBar: Show progress text / percentage label, format string pattern.
- **Dependency properties (2):** `ShowText`, `TextPattern`
- **Attached properties:** `ShowText`, `TextPattern`
- **Methods (4):** `GetShowText(DependencyObject obj)` · `SetShowText(DependencyObject obj, bool value)` · `GetTextPattern(DependencyObject obj)` · `SetTextPattern(DependencyObject obj, string value)`

### ScrollViewerHelper *(static class)*

- **Dependency properties (8):** `IsInertiaEnabled`, `ScrollSpeed`, `BubbleAtBoundary`, `IsAutoHideEnabled`, `ShowBackToTop`, `BackToTopThreshold`, `CurrentVerticalOffset`, `CurrentHorizontalOffset`
- **Attached properties:** `BackToTopThreshold`, `BubbleAtBoundary`, `CurrentHorizontalOffset`, `CurrentVerticalOffset`, `IsAutoHideEnabled`, `IsInertiaEnabled`, `ScrollSpeed`, `ShowBackToTop`
- **Methods (21):** `GetIsInertiaEnabled(DependencyObject obj)` · `SetIsInertiaEnabled(DependencyObject obj, bool value)` · `GetScrollSpeed(DependencyObject obj)` · `SetScrollSpeed(DependencyObject obj, double value)` · `GetBubbleAtBoundary(DependencyObject obj)` · `SetBubbleAtBoundary(DependencyObject obj, bool value)` · `GetIsAutoHideEnabled(DependencyObject obj)` · `SetIsAutoHideEnabled(DependencyObject obj, bool value)` · `GetShowBackToTop(DependencyObject obj)` · `SetShowBackToTop(DependencyObject obj, bool value)` · `GetBackToTopThreshold(DependencyObject obj)` · `SetBackToTopThreshold(DependencyObject obj, double value)` · `GetCurrentVerticalOffset(DependencyObject obj)` · `SetCurrentVerticalOffset(DependencyObject obj, double value)` · `GetCurrentHorizontalOffset(DependencyObject obj)` · `SetCurrentHorizontalOffset(DependencyObject obj, double value)` · `ScrollToTop(ScrollViewer sv, bool smooth)` · `ScrollToBottom(ScrollViewer sv, bool smooth)` · `ScrollToLeft(ScrollViewer sv, bool smooth)` · `ScrollToRight(ScrollViewer sv, bool smooth)` · `ScrollTo(ScrollViewer sv, double verticalOffset, double horizontalOffset, bool smooth)`

### SurfaceBorder *(enum)*

Which border colour a surface is outlined with.

Inherits `System.Enum`.

### SurfaceElement *(static class)*

A quick skin for any surface element: the theme's paper, border and corner radius, addressed by role instead of by brush key. The properties resolve through DynamicResource, so a surface painted with them follows a theme or accent change without a line of code.
- **Dependency properties (3):** `Role`, `Border`, `Radius`
- **Attached properties:** `Border`, `Radius`, `Role`
- **Methods (6):** `GetRole(DependencyObject obj)` · `SetRole(DependencyObject obj, SurfaceRole value)` · `GetBorder(DependencyObject obj)` · `SetBorder(DependencyObject obj, SurfaceBorder value)` · `GetRadius(DependencyObject obj)` · `SetRadius(DependencyObject obj, Nullable<CornerRadius> value)`

### SurfaceRole *(enum)*

Which material a surface is cut from, in the words of the design system rather than a hex.

Inherits `System.Enum`.

### TabControlHelper *(static class)*

Provides attached properties and routed commands for TabControl and TabItem, including closable tabs, tab close commands, and header action hosts.
- **Dependency properties (2):** `IsClosable`, `HeaderExtraContent`
- **Attached properties:** `HeaderExtraContent`, `IsClosable`
- **Routed events:** `TabClosing`, `TabClosed`
- **Properties (2):** `CloseCommand: RoutedUICommand`, `AddCommand: RoutedUICommand`
- **Methods (8):** `GetIsClosable(DependencyObject obj)` · `SetIsClosable(DependencyObject obj, bool value)` · `GetHeaderExtraContent(DependencyObject obj)` · `SetHeaderExtraContent(DependencyObject obj, object value)` · `AddTabClosingHandler(DependencyObject d, RoutedEventHandler handler)` · `RemoveTabClosingHandler(DependencyObject d, RoutedEventHandler handler)` · `AddTabClosedHandler(DependencyObject d, RoutedEventHandler handler)` · `RemoveTabClosedHandler(DependencyObject d, RoutedEventHandler handler)`

### TextBoxHelper *(static class)*

Advanced attached properties and behaviors for TextBox: Input filtering/masks, SelectAllOnFocus, ClearOnEscape, CommitOnEnter, Character count tracking.
- **Dependency properties (5):** `CharacterCountText`, `SelectAllOnFocus`, `ClearOnEscape`, `CommitOnEnter`, `InputType`
- **Attached properties:** `CharacterCountText`, `ClearOnEscape`, `CommitOnEnter`, `InputType`, `SelectAllOnFocus`
- **Methods (11):** `GetCharacterCountText(DependencyObject obj)` · `UpdateCharacterCount(TextBox tb)` · `GetSelectAllOnFocus(DependencyObject obj)` · `SetSelectAllOnFocus(DependencyObject obj, bool value)` · `GetClearOnEscape(DependencyObject obj)` · `SetClearOnEscape(DependencyObject obj, bool value)` · `GetCommitOnEnter(DependencyObject obj)` · `SetCommitOnEnter(DependencyObject obj, bool value)` · `GetInputType(DependencyObject obj)` · `SetInputType(DependencyObject obj, TextBoxInputType value)` · `IsTextAllowed(TextBox tb, string newText, TextBoxInputType inputType)`

### TextBoxInputType *(enum)*

Supported input restriction types for TextBox.

Inherits `System.Enum`.

### VisualElement *(static class)*

Appearance and micro-interaction for any : elevation, hover lift, glow, press, focus ring and disabled dimming. The behaviour lives in VisualElement.Appearance.cs.
- **Dependency properties (10):** `Elevation`, `HoverScale`, `HoverElevation`, `Glow`, `PressScale`, `DisabledOpacity`, `FocusRing`, `MotionDuration`, `ShadowColor`, `ScaleOrigin`
- **Attached properties:** `DisabledOpacity`, `Elevation`, `FocusRing`, `Glow`, `HoverElevation`, `HoverScale`, `MotionDuration`, `PressScale`, `ScaleOrigin`, `ShadowColor`
- **Methods (20):** `GetElevation(DependencyObject obj)` · `SetElevation(DependencyObject obj, int value)` · `GetHoverScale(DependencyObject obj)` · `SetHoverScale(DependencyObject obj, double value)` · `GetHoverElevation(DependencyObject obj)` · `SetHoverElevation(DependencyObject obj, int value)` · `GetGlow(DependencyObject obj)` · `SetGlow(DependencyObject obj, bool value)` · `GetPressScale(DependencyObject obj)` · `SetPressScale(DependencyObject obj, double value)` · `GetDisabledOpacity(DependencyObject obj)` · `SetDisabledOpacity(DependencyObject obj, double value)` · `GetFocusRing(DependencyObject obj)` · `SetFocusRing(DependencyObject obj, bool value)` · `GetMotionDuration(DependencyObject obj)` · `SetMotionDuration(DependencyObject obj, double value)` · `GetShadowColor(DependencyObject obj)` · `SetShadowColor(DependencyObject obj, Nullable<Color> value)` · `GetScaleOrigin(DependencyObject obj)` · `SetScaleOrigin(DependencyObject obj, double value)`

## CONCOST.UI.Controls

### AggregateType *(enum)*


Inherits `System.Enum`.
*No public members.*

### AnimatedEffectBase *(abstract)*

Base class for effects that paint themselves every frame. It owns the plumbing, the delta-time normalisation, the visibility handling and the reduced-motion contract, so derived controls only implement and .

Inherits `System.Windows.FrameworkElement`.
- **Dependency properties (1):** `IsAnimated` — Turns this effect on or off (does not unload it).
- **Methods (1):** `Restart()` — Restarts the effect from its first frame. Use this for a “replay” button: toggling is not enough, because keeps its value and a finished effect simply stays finished.

### AnimatedGradientBorder

A card whose outline is a gradient that slowly rotates around it — the "this one is live" ring used on pricing cards, active plan tiles and feature highlights.

Inherits `System.Windows.Controls.ContentControl`.
- **Dependency properties (8):** `ColorA` — First colour of the ring. Default a violet., `ColorB` — Second colour of the ring. Default a teal., `ColorC` — Third colour of the ring. Default an amber., `RingThickness` — Thickness of the ring, in pixels. Default 2., `Corner` — Corner radius of the card. Default 12., `SecondsPerTurn` — How long one full rotation takes. Default 8 seconds., `IsAnimated` — Turn the rotation on or off. Default true., `Fill` — Background inside the ring. Empty takes CONCOST.Brush.Surface.
- **Methods (1):** `OnApplyTemplate()`

### AnimatedMeshGradient

An animated mesh gradient: a handful of large colour blobs drifting on their own orbits and blending into each other, for hero panels and empty states.

Inherits `CONCOST.UI.Controls.AnimatedEffectBase`.
- **Dependency properties (8):** `ColorA` — First colour of the mesh. Default a violet., `ColorB` — Second colour. Default a teal., `ColorC` — Third colour. Default an amber., `ColorD` — Fourth colour. Default a rose., `Speed` — Multiplier on the drift speed. Default 1., `BlobOpacity` — Opacity of each blob where it is strongest. Default 0.55., `Spread` — Blob radius as a fraction of the smaller side. Default 0.75., `Background` — Base fill under the blobs. Empty paints nothing, so the panel behind shows through.

### ApplicationInfoBar

The bar under the ribbon that reports one thing at a time: a warning, an error, an update, or an offer with a single action.

Inherits `System.Windows.Controls.Border`.
- **Dependency properties (6):** `Severity` — How loud the bar is; picks the icon and the tint., `Title` — The bold lead-in., `Message` — The rest of the sentence., `ActionText` — The one action the bar offers; empty hides the button., `IsClosable` — Whether the bar can be dismissed., `ShowIcon` — Draws the severity icon at the left end. The bold title already reads the severity through its tint, so a host that pairs the bar with a spoken message can drop the glyph.
- **CLR events:** `ActionInvoked` — Raised when the action button is pressed., `Closed` — Raised when the bar is dismissed.
- **Methods (2):** `Show(string title, string message, CadNotificationSeverity severity)` · `Hide()` — Hides the bar without raising .

### ApplicationTitleBar

The application title bar: product brand mark, quick access strip, active document title, optional CAD search bar, user account pill and window control buttons (minimise, maximise, close).

Inherits `System.Windows.Controls.Grid`.
- **Dependency properties (21):** `ApplicationName`, `DocumentTitle`, `IsDocumentModified`, `ShowQuickAccess`, `ShowWindowButtons`, `IsMaximized`, `TitleForeground`, `TitleFontSize`, `TitleFontWeight`, `TitleAlignment`, `AppIcon`, `AppIconBrush`, `AppIconSize`, `ShowAppIcon`, `ShowSearchBox`, `SearchPlaceholder`, `UserAccountName`, `ShowUserAccount`, `DimOnInactive` — Dims the whole bar while the host window has lost the foreground — the cue every title bar gives, and the one that keeps a screenshot of a background window from reading as live. The drag area dims with the buttons; the window is not interactive then anyway., `IsWindowActive` — Whether the window the bar rides on currently owns the foreground; the dim reads this., `Subtitle`
- **CLR events:** `MinimizeRequested`, `MaximizeRequested`, `CloseRequested`
- **Properties (1):** `QuickAccess: ObservableCollection<TitleBarAction>`
- **Methods (3):** `SetWindowActive(bool active)` · `RefreshQuickAccess()` · `ComposeTitle(string documentTitle, bool isModified, string applicationName)`

### ArrowDirection *(enum)*

The four arrows drawn around the cube.

Inherits `System.Enum`.

### AuroraBackground

Animated aurora backdrop: a few large, soft radial blobs drifting on a dark or light surface. Rendered directly (no blur effect), so it stays cheap enough for splash screens, hero panels and login windows.

Inherits `System.Windows.FrameworkElement`.
- **Dependency properties (7):** `BlobCount`, `Speed`, `BlobOpacity`, `IsAnimated`, `AccentBrush`, `SecondaryBrush`, `TertiaryBrush`

### AutoCompleteTextBox

AutoCompleteTextBox provides an instant filterable dropdown suggestion list as the user types. Supports custom ItemTemplate, DisplayMemberPath, SelectedItem binding, search icon, clear button, and placeholder text. Optimized for zero lag with cached property reflection and high-performance UI filtering.

Inherits `System.Windows.Controls.TextBox`.
- **Dependency properties (6):** `ItemsSource`, `SelectedItem`, `ItemTemplate`, `DisplayMemberPath`, `MaxDropDownHeight`, `IsDropDownOpen`
- **Routed events:** `ItemSelected`
- **CLR events:** `ItemSelected`
- **Methods (4):** `OnApplyTemplate()` · `ClearValue()` · `GetItemText(object item)` · `FilterSuggestions(string query, bool openIfEmpty)`

### AutoHidePane

The thin strip along an edge that holds the auto-hidden panels as tabs.

Inherits `System.Windows.FrameworkElement`.
- **Dependency properties (3):** `Panes` — Panels shown as tabs. Non- entries are ignored., `Side` — Edge this strip belongs to., `TabSize` — Thickness of the strip, in pixels. Default 28.
- **CLR events:** `PaneInvoked` — Raised when a tab is clicked.
- **Properties (1):** `Tabs: IReadOnlyList<ToolWindow>` — Panels currently shown as tabs.

### AvatarStack

Overlapping avatar row with an optional "+N" overflow chip — the stacked assignee pattern from web dashboards, drawn directly for a stable layout. Items may be a string (initials), an or any object whose ToString() is used for the initials.

Inherits `System.Windows.FrameworkElement`.
- **Dependency properties (10):** `Items`, `MaxVisible`, `Size`, `Overlap`, `Background`, `Foreground`, `RingBrush`, `FontSize`, `ShowOverflow`, `OverflowTextFormat`

### Badge

A badge that overlays a numeric count, a status dot, or arbitrary text on top of child content. Supports appearance variants, max-value overflow display ("99+"), pulsing animation and fine-position offsets.

Inherits `System.Windows.Controls.ContentControl`.
- **Dependency properties (15):** `Appearance`, `CornerRadius`, `HasBadge`, `Value`, `IsDot`, `ShowBadge`, `BadgeMargin`, `Status`, `DotSize`, `MaxValue`, `DisplayText`, `Offset`, `BadgeFontSize`, `BadgePadding`, `Pulse`

### BadgeAppearance *(enum)*


Inherits `System.Enum`.
*No public members.*

### BorderBeam

Draws a light beam running around the border of any content — the “border beam” effect used by modern web cards. Wrap your content and the beam animates on the edge; the surface fill and the beam colour are both configurable.

Inherits `System.Windows.Controls.Decorator`.
- **Dependency properties (8):** `CornerRadius`, `BeamBrush`, `BeamThickness`, `BeamLength` — Length of the lit segment as a fraction of the perimeter (0–1)., `RevolutionSeconds`, `IsAnimated`, `SurfaceBrush`, `Padding`

### Breadcrumb


Inherits `System.Windows.Controls.Control`.
- **Dependency properties (2):** `Items`, `SeparatorIcon`

### BreadcrumbItem


Inherits `System.Windows.DependencyObject`.
- **Dependency properties (5):** `Header`, `Icon`, `IsSelected`, `Command`, `Tag`

### CadInputParser *(static class)*

Parses the values typed into a command line: lengths with units, coordinates and angles.
- **Methods (3):** `TryParseLength(string text, Double& millimetres)` · `TryParseCoordinate(string text, Point3D& point)` · `TryParseAngle(string text, Double& degrees)`

### CadNotification

One entry of the status bar notification list.
- **Properties (5):** `Title: string` — One line summary., `Message: string` — The detail, shown under the title., `Severity: CadNotificationSeverity` — How loud the entry is; drives its icon and colour., `Timestamp: DateTime` — When the entry was raised., `IsRead: bool` — True once the list has been opened and the entry seen.

### CadNotificationSeverity *(enum)*

How loud a status bar notification is.

Inherits `System.Enum`.

### CadStatusBar

The drafting status bar: selection summary and work plane on the left, live coordinates in the middle, and the snap toggles, the background task queue and the notification count on the right.

Inherits `System.Windows.Controls.Grid`.
- **Dependency properties (22):** `SelectionCount` — How many objects are selected., `WorkPlane` — The plane new geometry is drawn on., `Coordinates` — The cursor position to report., `UnitSuffix` — Appended to the coordinate readout, e.g. mm., `ShowCoordinates` — Hides the coordinate readout when there is no viewport under it., `SnapEnabled` — Whether the cursor pulls onto snap points., `GridEnabled` — Whether the grid is drawn., `OrthoEnabled` — Whether drawing is constrained to horizontal and vertical., `PolarEnabled` — Whether drawing is constrained to fixed angle increments., `ObjectSnapEnabled` — Whether snaps to existing geometry are on., `Variant` — How the bar is dressed: at the foot of a window, inside a strip the host already drew, over a drawing that must stay readable., `CornerRadius` — Corner radius of the plate. Half the height makes a pill., `Elevation` — Shadow depth of the plate, 0 to 4. Ignored by the seamless variant., `ControlHeight` — Height of every toggle and button on the bar, in pixels., `IconSize` — Size of the glyph inside a toggle or a counter, in pixels., `ShowDividers` — Shows the rules between the groups. A rule also steps aside with the group beside it., `ShowSelection` — Shows the "nothing selected" summary., `ShowWorkPlane` — Shows the work plane split button., `ShowToggles` — Shows the snap, grid, ortho, polar and object-snap switches as one group., `ShowTaskQueue` — Shows the background task counter. It also stays out of sight while nothing is running., `ShowNotifications` — Shows the notification bell., `ShowToggleLabels` — Prints SNAP, GRID, ORTHO, POLAR and OSNAP beside their glyphs. Turn it off on a narrow bar, where five captions cost more room than the switches are worth.
- **CLR events:** `ToggleChanged` — Raised when one of the drafting toggles changes, including by binding., `WorkPlaneChanged` — Raised when the work plane changes., `TaskSelected` — Raised when a row of the task list is picked., `NotificationSelected` — Raised when a row of the notification list is picked.
- **Properties (2):** `Tasks: ObservableCollection<CadTask>` — Work that is running in the background., `Notifications: ObservableCollection<CadNotification>` — What the status bar has been told, newest last.
- **Methods (10):** `SelectionSummary(int count)` · `FormatCoordinates(Point3D point, string unitSuffix)` · `IsToggleEnabled(CadToggle toggle)` · `SetToggle(CadToggle toggle, bool isEnabled)` · `AddTask(CadTask task)` · `RemoveTask(CadTask task)` · `Notify(CadNotification notification)` · `UpdateCoordinates(Point3D point)` · `RefreshTasks()` — Rebuilds the task list rows after a task's percentage changed. · `RefreshNotifications()` — Rebuilds the notification rows.

### CadStatusBarVariant *(enum)*

How a is dressed.

Inherits `System.Enum`.

### CadTask

One row of the background task queue.
- **Properties (4):** `Title: string` — What is running., `Percent: double` — How far along it is, 0 to 100; ignored when is set., `IsIndeterminate: bool` — Work with no measurable length, e.g. waiting on a service., `Tag: object` — Anything the host wants to hang off the row.
- **Methods (1):** `ToString()`

### CadToggle *(enum)*

The drafting settings a status bar can toggle.

Inherits `System.Enum`.

### CadToggleEventArgs

Carries the toggle that changed and its new state.

Inherits `System.EventArgs`.
- **Properties (2):** `Toggle: CadToggle` — Which setting changed., `IsEnabled: bool` — The new state.

### CalendarWithClock


Inherits `System.Windows.Controls.Control`.
- **Dependency properties (2):** `SelectedDateTime`, `ShowSeconds`
- **Routed events:** `Confirmed`
- **CLR events:** `Confirmed`
- **Methods (1):** `OnApplyTemplate()`

### Card


Inherits `System.Windows.Controls.HeaderedContentControl`.
- **Dependency properties (14):** `Elevation`, `IsInteractive`, `AccentBar`, `AccentBrush`, `AccentThickness`, `HeaderBackground`, `FooterBackground`, `CornerRadius`, `AccentCornerRadius`, `HeaderCornerRadius`, `FooterCornerRadius`, `Icon`, `Footer`, `FooterTemplate`

### CardAccent *(enum)*

Which edge of a carries the accent rule.

Inherits `System.Enum`.

### CellType *(enum)*


Inherits `System.Enum`.
*No public members.*

### CheckComboBox


Inherits `System.Windows.Controls.Control`.
- **Dependency properties (10):** `ItemsSource`, `SelectedValue`, `DisplayText`, `SelectedItemsPreviewMaxWidth`, `Placeholder`, `IsDropDownOpen`, `ShowSelectAll`, `SelectAllText`, `MaxDropDownHeight`, `CornerRadius`
- **Routed events:** `SelectionChanged`
- **CLR events:** `SelectionChanged`
- **Properties (2):** `InternalItems: ObservableCollection<CheckComboBoxItem>`, `SelectedItems: IList<object>`
- **Methods (4):** `OnApplyTemplate()` · `OnItemSelectionToggled()` · `SelectAll()` · `UnselectAll()`

### CheckComboBoxItem

- **CLR events:** `PropertyChanged`
- **Properties (3):** `Content: object`, `IsSelected: bool`, `Parent: CheckComboBox`
- **Methods (1):** `ToString()`

### CheckMarkDraw

Check mark that draws itself stroke by stroke — the confirmation tick used after a save or an approval. Drive it with ; the drawing is animated in 320 ms.

Inherits `System.Windows.FrameworkElement`.
- **Dependency properties (6):** `IsChecked` — True draws the tick, false erases it., `Stroke`, `StrokeThickness`, `Duration`, `BackgroundBrush`, `ShowCircle` — Draws a soft circle behind the tick.

### Clock


Inherits `System.Windows.Controls.Control`.
- **Dependency properties (3):** `SelectedTime`, `DisplayMode`, `ShowSeconds`
- **Routed events:** `SelectedTimeChanged`
- **CLR events:** `SelectedTimeChanged`
- **Properties (1):** `FormattedTime: string`
- **Methods (2):** `OnApplyTemplate()` · `UpdateClockVisuals()`

### ClockDisplayMode *(enum)*


Inherits `System.Enum`.
*No public members.*

### CoachMark

A first-run callout that dims the window and points at one control.

Inherits `System.Windows.Controls.Control`.
- **Dependency properties (9):** `Target` — The element the card points at., `Placement` — Preferred side of the target., `Title` — Bold line at the top of the card., `StepIndex` — One-based step shown as "2 / 4" when is greater than one., `StepCount` — Total number of steps in the tour., `CardWidth` — Fixed card width., `Gap` — Distance between the target and the card., `OverlayBrush` — Colour of the dimming layer; a dark translucent brush by default., `CardRect` — Where the card was placed by the last arrange pass.
- **CLR events:** `Next` — Raised by the card's Next button; the host advances the tour., `Dismissed` — Raised when the card is dismissed.
- **Properties (1):** `IsLastStep: bool` — True when the tour is on its last step.
- **Methods (4):** `Advance()` — Raises ; the host decides whether there is another step. · `Dismiss()` — Hides the callout and raises , once. · `OnApplyTemplate()` · `ComputeCardRect(Size host)`

### CoachMarkPlacement *(enum)*

Where a prefers to sit relative to its target.

Inherits `System.Enum`.

### CodeEditor

A code box with line numbers, tab handling and optional syntax colouring.

Inherits `System.Windows.Controls.Control`.
- **Dependency properties (8):** `Text` — The code, kept in sync with the editor both ways., `Syntax` — Which tokenizer to run., `IsReadOnly` — Blocks editing but keeps selection and copy., `HighlightSyntax` — Colour keywords, strings and comments. Off means plain text and intact undo., `ShowLineNumbers` — Shows the line-number gutter., `TabSize` — Spaces a tab inserts., `LineCount` — Number of logical lines, kept current as the text changes., `CaretLine` — One-based line the caret is on.
- **Methods (4):** `OnApplyTemplate()` · `GoToLine(int line)` · `FindNext(string term, bool matchCase)` · `Refresh()` — Applies the current text to the editor and refreshes the gutter.

### CodeHighlighter *(static class)*

A small tokenizer for the languages a desktop tool actually shows in a help pane or a script editor. It is deliberately a scanner rather than a parser: it never throws, always covers the budget in linear time, and returns the runs a renderer needs.
- **Methods (1):** `Tokenize(string text, CodeLanguage language)`

### CodeLanguage *(enum)*

Language the knows enough about to colour.

Inherits `System.Enum`.

### CodeToken *(struct)*

A run of characters with one meaning. Characters between tokens are plain text.

Inherits `System.ValueType`.
- **Properties (3):** `Start: int` — Index of the first character., `Length: int` — Number of characters., `Kind: CodeTokenKind` — What the run is.

### CodeTokenKind *(enum)*

What a is, so a host can theme it.

Inherits `System.Enum`.

### ColorPicker


Inherits `System.Windows.Controls.Control`.
- **Dependency properties (3):** `SelectedColor`, `HexCode`, `ShowAlpha`
- **Properties (1):** `PresetColors: ObservableCollection<Color>`
- **Methods (1):** `OnApplyTemplate()`

### ColumnChooserItem

A single row inside the list.
- **CLR events:** `PropertyChanged`
- **Properties (3):** `Header: string`, `IsVisible: bool` — Ticked when the column is visible. Setting it shows or hides the column., `Column: DataGridColumn` — The column this row controls.

### ColumnFilterItem

A single filter box inside the .
- **CLR events:** `PropertyChanged`
- **Properties (6):** `Header: string`, `FilterText: string` — What the user typed. Setting it re-filters the grid immediately., `Placeholder: string` — Hint shown while the box is empty — normally the column header., `Width: double` — Width mirrored from the column, so the box sits exactly under it., `Operator: FilterOperator` — The comparison operator for this filter., `Column: DataGridColumn` — The column this box filters.

### ColumnGroup


Inherits `CONCOST.UI.Controls.ColumnNode`.
- **Properties (1):** `Children: ObservableCollection<ColumnNode>`
- **Methods (2):** `GetLeaves()` · `GetDepth()`

### ColumnGroupCandidate

A column that can be chosen for grouping.
- **Properties (3):** `Header: string`, `PropertyPath: string`, `Column: DataGridColumn`

### ColumnLeaf


Inherits `CONCOST.UI.Controls.ColumnNode`.
- **Dependency properties (12):** `BindingPath`, `SortMemberPath`, `CanUserSort`, `CanUserResize`, `Width`, `MinWidth`, `MaxWidth`, `CellType`, `StringFormat`, `AggregateType`, `AggregateFormat`, `CustomAggregateText`
- **Methods (2):** `GetLeaves()` · `GetDepth()`

### ColumnNode *(abstract)*


Inherits `System.Windows.DependencyObject`.
- **Dependency properties (6):** `HeaderText`, `HeaderTemplate`, `Foreground`, `Background`, `ToolTip`, `HorizontalAlignment`
- **Methods (2):** `GetLeaves()` · `GetDepth()`

### ColumnSummaryItem

Aggregate numbers for a single column in the row.
- **CLR events:** `PropertyChanged`
- **Properties (13):** `Header: string`, `DisplayText: string` — What the row prints, e.g. Sum 12 480,0 or Rows 24., `Sum: double`, `Average: double`, `Min: double`, `Max: double`, `Count: int` — How many rows the grid currently shows (after filtering)., `IsNumeric: bool` — True when every value in the column parsed as a number., `Mode: DataGridSummaryAggregate` — The aggregate this cell shows., `Format: string` — Numeric format used by ., `Width: double` — Width mirrored from the column, so the row lines up with the grid., `DisplayIndex: int`, `Column: DataGridColumn` — The column this cell summarises.

### CommandBar

A toolbar of commands that pushes what does not fit into a "more" menu instead of clipping it.

Inherits `CONCOST.UI.Controls.OverflowToolbar`.
- **Dependency properties (1):** `OverflowToolTip` — Tooltip of the overflow button.

### CommandBarItem

One command in a .
- **Properties (8):** `Label: string` — Caption under or beside the icon., `Icon: IconKind` — Glyph., `Command: ICommand` — Command run on click., `CommandParameter: object` — Parameter passed to ., `ToolTip: string` — Tooltip text. Falls back to ., `IsSeparator: bool` — Draw a separator instead of a button., `ShowLabel: bool` — Hide the caption and keep only the icon. Default true., `IsEnabled: bool` — Enabled state of the button.

### CommandDefinition

A command the line can run: a name, optional aliases, and an optional follow-up prompt for the argument the command still needs.
- **Properties (9):** `Name: string` — The primary name, e.g. LINE., `Aliases: List<string>` — Other names that resolve to this command, e.g. L., `Description: string` — One line shown next to the name in the suggestion list., `Prompt: string` — What the command still needs, e.g. "Specify first point". When this is set the line switches to prompt mode after the command runs and reports the answer through instead of parsing another command., `Options: List<string>` — Keywords the prompt accepts, e.g. Undo and Close. They are drawn as [Undo/Close] after the prompt the way a CAD prompt offers its options, and the capital letter of a keyword is enough to pick it., `PromptText: string` — The prompt with its keyword list, e.g. Specify next point or [Undo/Close]., `Tag: object` — Anything the host wants to hang off the definition., `Keys: IEnumerable<string>` — The name and every alias., `Display: string` — What the suggestion list shows: the name, its aliases so a two-letter user is not left guessing, then the description.
- **Methods (6):** `WithOptions(String[] options)` · `MatchOption(string text)` · `WithAliases(String[] aliases)` · `Matches(string text)` · `MatchesKey(string text)` · `ToString()`

### CommandInvokedEventArgs

Carries the command that ran and the arguments typed after it.

Inherits `System.EventArgs`.
- **Properties (3):** `Definition: CommandDefinition` — The command that resolved from what was typed., `RawText: string` — The whole line, as typed., `Arguments: IReadOnlyList<string>` — Everything after the command name, split on whitespace.

### CommandLine

The command line: a scrollable history of what was typed and what came back, an input box with command-name suggestions, history recall, and a prompt mode for the value a command still needs.

Inherits `System.Windows.Controls.Grid`.
- **Dependency properties (11):** `Prompt` — The label in front of the input; a command prompt replaces it while prompt mode is on., `HistoryHeight` — How tall the history scrolls before it scrolls., `ShowHistory` — Hides the history, for a command line that is only ever one line tall., `MaxHistory` — How many lines are kept before the oldest is dropped., `Variant` — How the line is dressed: inside a card, as the strip along the bottom of the window, or as a plate the user has floated off the frame., `CornerRadius` — Corner radius of the floating surface., `Elevation` — Shadow depth of the floating surface, 0 to 4., `EchoPrompt` — Writes the prompt into the transcript and puts the answer on the same line, which is what a CAD command line shows: the dialogue, not a log of results with the question hidden in a label. Turn it off for a read-only log., `SpaceSubmits` — Space submits the line, the way a CAD command line does. Set it to false for a prompt whose answer can contain a space — a room name, a note., `IsHistoryExpanded` — The full history, the window F2 raises., `ExpandedHistoryHeight` — How tall the history grows while is on.
- **CLR events:** `CommandInvoked` — Raised when a typed line resolves to a command., `UnknownCommand` — Raised with the text a command typed that resolved to nothing., `PromptCompleted` — Raised when a command prompt is answered., `PromptCancelled` — Raised when a command prompt is cancelled with Escape.
- **Properties (7):** `Commands: ObservableCollection<CommandDefinition>` — The commands the line can run., `Messages: ObservableCollection<CommandMessage>` — The history, oldest first, limited by ., `Suggestions: IReadOnlyList<CommandDefinition>` — The commands the suggestion list is offering, best first., `PromptText: string` — The prompt as it is drawn, with its keyword list and the colon., `Text: string` — The text in the input box., `IsPromptActive: bool` — True while the line is waiting for the answer to a command prompt., `LastCommand: CommandDefinition` — The command that ran last, which an empty Enter repeats.
- **Methods (14):** `ToggleHistory()` — Raises or folds the full history, which is what F2 does. · `AddCommand(CommandDefinition definition)` · `AddCommands(IEnumerable<CommandDefinition> definitions)` · `Resolve(string rawText)` · `Execute(string command)` · `Submit()` · `BeginPrompt(string prompt)` · `BeginPrompt(string prompt, IEnumerable<string> options)` · `CancelPrompt()` — Leaves prompt mode without an answer. · `Complete()` — Completes the typed command name from the first suggestion. · `WriteLine(string text, CommandMessageKind kind)` · `Clear()` — Clears the visible history. · `FocusInput()` — Gives the input box focus, e.g. when the window opens. · `MatchOption(IEnumerable<string> options, string text)`

### CommandLineVariant *(enum)*

How a is dressed.

Inherits `System.Enum`.

### CommandMessage

One line in the command history.
- **Properties (3):** `Text: string` — The line itself., `Kind: CommandMessageKind` — How the line is coloured., `Timestamp: DateTime` — When the line was written.

### CommandMessageKind *(enum)*

How a command line entry reads.

Inherits `System.Enum`.

### CommandPalette

The Ctrl+K / command-bar overlay: a search box over a filtered list of commands, driven entirely by the keyboard.

Inherits `System.Windows.Controls.Control`.
- **Dependency properties (10):** `IsOpen` — Whether the overlay is showing., `ItemsSource` — Commands to list. Any of ., `FilterText` — Current search term, two-way bound to the search box., `SelectedIndex` — Index into ., `SelectedItem` — The highlighted entry., `HasResults` — False when the current search term matches nothing., `Placeholder` — Hint shown in the empty search box., `EmptyText` — Message shown when nothing matches., `MaxListHeight` — Height cap for the result list before it scrolls. Rounded down to whole rows, so the default (276) shows exactly six., `CloseOnInvoke` — Closes the palette after running an entry. Defaults to true.
- **CLR events:** `ItemInvoked` — Raised after an entry runs., `Opened` — Raised when the palette becomes visible., `Closed` — Raised when the palette hides.
- **Properties (1):** `FilteredItems: IReadOnlyList<CommandPaletteItem>` — The entries currently visible, after filtering.
- **Methods (8):** `Open()` — Shows the overlay and moves focus to the search box. · `Close()` — Hides the overlay. · `Toggle()` — Toggles the overlay. · `Refresh()` — Rebuilds the filtered list from and . · `MoveSelection(int delta)` · `ExecuteSelected()` — Runs the highlighted entry. Returns false when there is nothing to run. · `InvokeItem(CommandPaletteItem item)` · `OnApplyTemplate()`

### CommandPaletteInvokedEventArgs

Raised when a runs an entry.

Inherits `System.EventArgs`.
- **Properties (1):** `Item: CommandPaletteItem` — The entry that was run.

### CommandPaletteItem

One entry in a : what to show, what to search on and what to run.
- **Properties (12):** `Title: string` — Primary line., `Subtitle: string` — Second line, usually what the command does., `Shortcut: string` — Gesture shown on the right, for example Ctrl+S., `Category: string` — Group caption, also searched., `Icon: IconKind` — Icon drawn on the left., `Command: ICommand` — Command run by ., `CommandParameter: object` — Parameter passed to . Falls back to the item itself., `Keywords: string` — Extra search terms that are not displayed., `Tag: object` — Anything the host wants to carry along., `IsEnabled: bool` — Disabled entries stay visible but cannot be run., `CanInvoke: bool` — True when is set and currently executable., `SearchText: string` — Everything a search can match on, lower-cased and stripped of diacritics so "tong quan" finds "Tổng quan".
- **Methods (2):** `Invoke()` — Runs when it can execute. Returns false when nothing ran. · `ToString()`

### CommandPaletteText *(static class)*

Search helpers shared by the palette and its tests.
- **Methods (3):** `Normalize(string value)` · `Tokenize(string query)` · `Matches(CommandPaletteItem item, IReadOnlyList<string> tokens)`

### CommandPromptEventArgs

Carries the answer given to a command prompt.

Inherits `System.EventArgs`.
- **Properties (3):** `Prompt: string` — The prompt that was answered., `Text: string` — What the user typed, trimmed., `Option: string` — The keyword the answer picked, or null when it was a value. Typing U at a [Undo/Close] prompt reports Undo, so the host branches on the keyword rather than on a spelling.

### CompareSlider

CompareSlider splits two visual contents (e.g. Before / After CAD drawings, 3D renderings, revisions) with a draggable split handle and smooth clipping mask.

Inherits `System.Windows.Controls.Control`.
- **Dependency properties (5):** `SourceContent`, `TargetContent`, `Value`, `Orientation`, `HandleSize`
- **Methods (1):** `OnApplyTemplate()`

### ComparisonOperator *(enum)*


Inherits `System.Enum`.
*No public members.*

### CompassIndicator

A north indicator for a plan view: a fixed viewport marker at the top with a dial that carries the needle and the cardinal letters around it. is the rotation of the drawing under the indicator in degrees — the dial turns by the same amount, so the letter sitting under the top marker is the world direction currently pointing up on screen.

Inherits `System.Windows.Controls.Control`.
- **Dependency properties (4):** `Angle` — Rotation of the drawing under the indicator, in degrees clockwise., `ShowCardinals` — Draws the four cardinal letters around the dial. The needle and ticks stay either way., `CardinalBrush` — Brush of the E/S/W letters; the N takes ., `NorthBrush` — Brush of the north letter, the needle and the top marker.

### ConditionalFormattingRule


Inherits `System.Windows.DependencyObject`.
- **Dependency properties (5):** `Operator`, `Value1`, `Value2`, `Background`, `Foreground`
- **Methods (1):** `Evaluate(double val)`

### ConfettiBurst

Short confetti burst for success moments (a saved model, an approved submittal). Set to true to fire; the control animates the particles itself and resets the flag when the burst is over.

Inherits `System.Windows.FrameworkElement`.
- **Dependency properties (7):** `IsBursting`, `ParticleCount`, `Duration`, `Gravity`, `Spread`, `Origin`, `Palette`

### CursorGlow

Soft light that trails the pointer across the surface it covers — the “cursor glow” used on dark hero sections. Place it as the top layer of a Grid; it never blocks input.

Inherits `CONCOST.UI.Controls.AnimatedEffectBase`.
- **Dependency properties (4):** `GlowSize` — Diameter of the glow in pixels., `GlowBrush`, `GlowOpacity`, `FollowSpeed` — 0 = glow snaps to the pointer, 1 = never arrives. 0.15–0.25 feels right.

### CursorSpotlight

Darkens everything except a soft circle around the pointer — the "look here" overlay for onboarding, empty states and product tours.

Inherits `CONCOST.UI.Controls.AnimatedEffectBase`.
- **Dependency properties (7):** `SpotDim` — How dark everything outside the circle gets, 0–1. Default 0.55., `Radius` — Radius of the clear circle, in pixels. Default 160., `Feather` — How much of the radius is the soft edge, 0–1. Default 0.45., `DimColor` — Colour of the dimmed area. Default black., `SpotColor` — Tint inside the circle. Transparent leaves the content untouched. Default transparent., `FollowCursor` — Track the pointer. Set to false to drive yourself., `SpotlightCenter` — Centre of the circle, in this control's coordinates. NaN means "follow the cursor".

### CycleLoading


Inherits `CONCOST.UI.Controls.LoadingBase`.
- **Dependency properties (2):** `Value`, `LoadTitle`

### DataBarWidthConverter

- **Methods (2):** `Convert(Object[] values, Type targetType, object parameter, CultureInfo culture)` · `ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)`

### DataGridCellTemplateFactory *(static class)*

- **Methods (8):** `CreateProgressTemplate(string bindingPath, double max)` · `CreateBadgeTemplate(string bindingPath)` · `CreateRatingTemplate(string bindingPath)` · `CreateDateTemplate(string bindingPath, string format)` · `CreateNumericTemplate(string bindingPath, string format, HorizontalAlignment alignment)` · `CreateHyperlinkTemplate(string bindingPath)` · `CreateRowExpanderTemplate()` · `CreateCheckBoxTemplate(string bindingPath)`

### DataGridClipboardHelper *(static class)*

- **Methods (2):** `CopyToClipboard(StackedHeaderDataGrid grid, bool includeHeaders)` · `ExportToCsv(StackedHeaderDataGrid grid)`

### DataGridColumnChooser

A drop-down that toggles the visibility of individual columns of a . The list is discovered from the grid and follows it when columns are added or removed.

Inherits `System.Windows.Controls.Control`.
- **Dependency properties (5):** `TargetGrid`, `IsOpen` — Whether the column list is showing. Two-way, so the drop-down can close itself., `Columns` — One entry per column of the target grid, in column order., `HeaderText` — Text on the drop-down button and above the list., `CornerRadius`
- **CLR events:** `ColumnsChanged` — Raised after a column's visibility changed.
- **Methods (3):** `RefreshColumns()` — Rebuilds the list from the target grid's columns. · `ShowAll()` — Shows every column. · `HideAll()` — Hides every column. The “Show all” row stays available to bring them back.

### DataGridContextMenuHelper *(static class)*

- **Methods (1):** `CreateDefaultContextMenu(StackedHeaderDataGrid grid)`

### DataGridFilterHelper *(static class)*

- **Methods (1):** `ShowFilterPopup(StackedHeaderDataGrid grid, ColumnLeaf leaf, FrameworkElement targetPlacement)`

### DataGridFilterPanel

A filter row that sits above a : one box per column, aligned with the column it filters. Each box can choose a comparison operator (Contains, Equals, etc.) and typing filters the grid live. The predicate is registered with , so it composes with instead of replacing it.

Inherits `System.Windows.Controls.Control`.
- **Dependency properties (4):** `TargetGrid`, `Filters` — One entry per column of the target grid, in display order., `PlaceholderText`, `ShowOperatorSelector` — Whether each box shows a comparison-operator drop-down above it. Turn it off for a compact one-row filter strip that always uses .
- **CLR events:** `FiltersApplied` — Raised after the grid has been re-filtered.
- **Methods (4):** `OnApplyTemplate()` · `RefreshFilters()` — Rebuilds the box list from the target grid's columns. · `ApplyFilters()` — Pushes the current box contents onto the grid's collection view. · `ClearFilters()` — Empties every box and restores the unfiltered grid.

### DataGridGroupPanel

A panel that sits above a and lets the user group rows by one or more columns. Columns are picked from a drop-down; each selected column becomes a pill that can be removed or reordered. The underlying grouping is applied through .

Inherits `System.Windows.Controls.Control`.
- **Dependency properties (7):** `TargetGrid`, `Groups` — The current group hierarchy, outermost first., `AvailableColumns` — Columns of the target grid that are not currently grouped., `SelectedColumn` — The column currently selected in the drop-down, ready to be added., `HeaderText`, `EmptyText`, `CornerRadius`
- **CLR events:** `GroupsChanged` — Raised after the grouping has changed.
- **Methods (4):** `RefreshAvailableColumns()` — Rebuilds the available-column list from the grid, excluding already-grouped columns. · `AddGroup(string propertyPath, string header)` · `RemoveGroup(GroupDescriptor descriptor)` · `ClearGroups()` — Removes all grouping.

### DataGridPerformance *(static class)*

The switches a grid over ten thousand rows needs, attached so an existing picks them up without a new control type: DeferredScrolling drags the thumb with only a position read-out and scrolls when it is released, CachePages pre-materialises rows ahead of the viewport so fast scrolling does not flash empty cells, and the two scroll helpers reach a row through the virtualized panel.
- **Dependency properties (2):** `DeferredScrolling`, `CachePages`
- **Attached properties:** `CachePages`, `DeferredScrolling`
- **Methods (6):** `GetDeferredScrolling(DataGrid grid)` · `SetDeferredScrolling(DataGrid grid, bool value)` · `GetCachePages(DataGrid grid)` · `SetCachePages(DataGrid grid, double value)` · `ScrollToItem(DataGrid grid, object item)` · `ScrollToIndex(DataGrid grid, int index)`

### DataGridSearchPanel

A single search box that filters every visible column of a at once. The match is a case-insensitive Contains on each cell's text.

Inherits `System.Windows.Controls.Control`.
- **Dependency properties (4):** `TargetGrid`, `SearchText`, `PlaceholderText`, `IsSearching`
- **CLR events:** `SearchChanged` — Raised after the grid has been re-filtered.
- **Methods (2):** `ClearSearch()` — Empties the box and restores the rows the search was hiding. · `ApplySearch()` — Pushes the current search term onto the grid's collection view.

### DataGridSummary

A summary row that sits below a and shows an aggregate per column.

Inherits `System.Windows.Controls.Control`.
- **Dependency properties (6):** `TargetGrid`, `Summaries` — One entry per column of the target grid, in display order., `Aggregate` — The aggregate used for every column without a per-column override., `DisplayFormat` — Numeric format for the aggregate value, e.g. N0 or #,##0.00., `RowHeight`, `AggregateOnColumn`
- **Attached properties:** `AggregateOnColumn`
- **Methods (4):** `SetAggregateOnColumn(DataGridColumn column, DataGridSummaryAggregate value)` · `GetAggregateOnColumn(DataGridColumn column)` · `OnApplyTemplate()` · `RefreshSummaries()` — Recomputes every column's aggregate from the grid's current (filtered) rows.

### DataGridSummaryAggregate *(enum)*

Which aggregate a shows for a column.

Inherits `System.Enum`.

### DataPager


Inherits `System.Windows.Controls.Control`.
- **Dependency properties (3):** `PageIndex`, `PageSize`, `TotalItems`
- **CLR events:** `PageChanged`, `PropertyChanged`
- **Properties (3):** `TotalPages: int`, `CanGoPrev: bool`, `CanGoNext: bool`
- **Methods (4):** `NextPage()` · `PrevPage()` · `FirstPage()` · `LastPage()`

### DetailsVisibilityToBoolConverter

- **Methods (2):** `Convert(object value, Type targetType, object parameter, CultureInfo culture)` · `ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)`

### Dialog


Inherits `System.Windows.Controls.ContentControl`.
- **Dependency properties (9):** `IsOpen`, `Title`, `DialogWidth`, `DialogHeight`, `CornerRadius`, `MaskCanClose`, `ShowCloseButton`, `MessageType`, `Token`
- **Attached properties:** `Token`
- **Properties (1):** `OnClosed: Action`
- **Methods (8):** `SetToken(DependencyObject element, string value)` · `GetToken(DependencyObject element)` · `RegisterGlobalHost(Panel panel)` · `Show(object content, string title, string token)` · `ShowConfirmAsync(string message, string title, string token)` · `ShowInputAsync(string message, string defaultText, string title, string token)` · `Close(string token)` · `OnApplyTemplate()`

### DialogMessageType *(enum)*


Inherits `System.Enum`.
*No public members.*

### Divider

A horizontal or vertical rule with an optional centred label and icon. Supports solid, dashed and dotted lines, custom brush/thickness, and text styling.

Inherits `System.Windows.Controls.Control`.
- **Dependency properties (11):** `Text`, `Orientation`, `Icon`, `LineStyle`, `TextAlignment`, `LineThickness`, `LineBrush`, `TextFontSize`, `TextForeground`, `TextPadding`, `Spacing`

### DividerLineStyle *(enum)*


Inherits `System.Enum`.
*No public members.*

### DockGroupMode *(enum)*

How a arranges its children.

Inherits `System.Enum`.

### DockHeaderButtons *(enum)*

When a pane header shows its pin and close buttons.

Inherits `System.Enum`.

### DockPaneGroup

A node of the docking layout tree: it either splits its area between its children or shows them as tabs. Groups nest, so a layout can be as deep as an IDE's.

Inherits `System.Windows.Controls.Panel`.
- **Dependency properties (10):** `Orientation` — Split direction when is ., `Mode` — Split into slots, or show the children as tabs. Default ., `Dock` — Edge this group occupies when it is a direct child of a ., `DockSize` — Thickness of the edge this group occupies. Ignored when the group is nested., `MinDockSize` — Smallest thickness the edge may shrink to. Default 120., `Proportion`, `SplitterThickness` — Thickness of the gutter between two slots of this group, in pixels. Default 5 — a constant in the first version, so a manager that asked for a thinner gutter still got this one., `TabStripThickness` — Height reserved for the tab strip when is tabs. Default 26., `TabPlacement` — Which edge of the group its tab strip hugs. Default ., `SplitterBrush` — Colour of the hairline in each gutter. Empty takes CONCOST.Brush.Border.
- **Attached properties:** `Proportion`
- **CLR events:** `PaneDragStarted` — Raised when a nested pane's header drag begins., `PaneDragMoved` — Raised while a nested pane's header is dragged, in manager coordinates., `PaneDragCompleted` — Raised when a nested pane's header drag ends, in manager coordinates., `PaneClosed` — Raised when a nested pane closes itself., `PaneAutoHideChanged` — Raised when a nested pane flips its auto-hide state.
- **Properties (2):** `Nodes: IEnumerable<UIElement>` — The children that take part in the layout, in declaration order., `ActivePane: ToolWindow` — The tab that is currently shown, or null when the group is empty.
- **Methods (3):** `GetProportion(DependencyObject element)` · `SetProportion(DependencyObject element, double value)` · `MoveChild(ToolWindow pane, int index)`

### DockPreview

The drop hint shown while a panel is being dragged: a translucent accent rectangle with a dashed border over the area the panel would occupy.

Inherits `System.Windows.FrameworkElement`.
- **Dependency properties (4):** `IsOpen` — Show the hint., `Side` — Edge the panel would dock to. Changes only the drawn hint, not the geometry., `PreviewBrush` — Fill of the hint. Empty uses the theme accent at low opacity., `BorderBrush` — Dashed outline colour. Empty uses the theme accent.

### DockSide *(enum)*

Which edge of a a is docked to.

Inherits `System.Enum`.

### DockSplitter

The draggable divider between two docked areas.

Inherits `System.Windows.FrameworkElement`.
- **Dependency properties (6):** `Orientation` — Direction of the bar itself. Default ., `Thickness` — Hit area of the bar, in pixels. Default 6., `Value` — Current size of the docked area this splitter resizes., `Minimum` — Lower clamp while dragging., `Maximum` — Upper clamp while dragging., `LineBrush` — Colour of the line. Empty takes the theme border brush.
- **CLR events:** `DragCompleted` — Raised after the user finished dragging.

### DockTabPlacement *(enum)*

Which edge of a pane group the tab strip sits on.

Inherits `System.Enum`.

### DockTabStrip

The tab strip of a pane group: one tab per sharing an edge.

Inherits `System.Windows.Controls.Control`.
- **Dependency properties (4):** `TabPlacement` — Which edge of the group the tabs hug. Default ., `TabHeight` — Height of one tab, in pixels. Default 26., `IconSize` — Size of the glyph in front of a tab's caption, in pixels. Default 12., `Panes` — The panes of this group, in declaration order.
- **CLR events:** `TabSelected` — Raised when the user picks a tab.
- **Methods (1):** `OnApplyTemplate()`

### DockingManager

Lays out panels around a centre area: left, top, right and bottom edges, each resizable by a splitter, plus auto-hide strips and a drop preview while a panel is dragged.

Inherits `System.Windows.Controls.Panel`.
- **Dependency properties (13):** `SplitterThickness` — Hit area of the splitter bands, in pixels. Default 5., `AutoHideStripSize` — Thickness of an auto-hide strip, in pixels. Default 28., `ShowDropPreview` — Show the dashed hint while a panel is dragged. Default true., `SplitterBrush` — Colour of the hairline drawn in each splitter band. Empty takes CONCOST.Brush.Border from the theme., `IsProportional` — Keep each pane's share of the window when the window is resized. Set to false to pin to pixels instead. Default true., `Variant` — How the outer edge of the dock is drawn: for a dock that is the window, for one inside a card and for one over a drawing., `CornerRadius` — Corner radius of the dock's own frame, in pixels. Ignored when seamless., `Elevation` — Shadow depth 0–4. A seamless dock ignores it; a floating one always keeps at least 1., `HeaderHeight` — Height of every pane header this manager lays out, in pixels. Default 28., `IconSize` — Size of the glyph in every pane header and tab, in pixels. Default 13., `HeaderButtons` — When pane headers show their pin and close buttons. Default — eight glyphs across four panes is a toolbar, not a title., `ActiveAccent` — Draw the accent rule on the header of the active pane. Default true., `TabPlacement` — Which edge of a pane group its tabs hug. Default ; the document strip in the centre always stays on top.
- **CLR events:** `PaneDocked` — Raised after a panel is docked to a new edge., `PaneClosed` — Raised after a panel is removed from the layout.
- **Properties (3):** `Panes: IEnumerable<ToolWindow>` — The tool windows in the layout, in declaration order., `IsPreviewOpen: bool` — True while a drag is showing the drop hint., `AllPanes: IEnumerable<ToolWindow>` — Every panel in the layout, including the ones nested inside groups.
- **Methods (13):** `Group(DockSide side)` · `Dock(ToolWindow pane, DockSide side)` · `ClosePane(ToolWindow pane)` · `MovePane(ToolWindow pane, int index)` · `ToggleFlyout(ToolWindow pane)` · `CloseFlyouts()` — Collapses every open flyout. · `Float(ToolWindow pane)` · `IsFloating(ToolWindow pane)` · `CaptureLayout()` — Serialises the layout — edge, size, auto-hide and active state per panel — so a host can restore it on the next run. Panels are matched by when it is applied. · `ApplyLayout(string layout)` · `DockAt(ToolWindow pane, Point point)` · `ShowDropHint(ToolWindow pane, Point point)` · `EndDropPreview()` — Hides the drop hint.

### DockingManagerVariant *(enum)*

How a draws its outer edge.

Inherits `System.Enum`.

### DocumentClosingEventArgs

Asks whether a document may close.

Inherits `System.EventArgs`.
- **Properties (2):** `Pane: DocumentPane` — The document that is closing., `Cancel: bool` — Set to true to keep the document open, e.g. to show a save prompt.

### DocumentPane

One open drawing, schedule or sheet in a .
- **Properties (6):** `Title: string` — Caption on the tab., `Content: object` — The view itself — a , a schedule, anything., `Icon: IconKind` — Tab icon., `IsDirty: bool` — Shows the unsaved dot., `Tag: object` — The model view behind the pane, so a host can map tab back to data., `FloatingWindow: Window` — The window the pane is floating in, or null when it is docked.

### DocumentTabControl

A tab control for documents: every tab can be closed, and closing goes through the tab's when it has one.

Inherits `System.Windows.Controls.TabControl`.
- **Dependency properties (1):** `ShowCloseButtons` — Master switch for the close buttons. Default true.
- **Properties (1):** `Documents: IEnumerable<DocumentTabItem>` — The tabs, in order.
- **Methods (3):** `CloseTab(object item)` · `CloseOtherTabs(object keep)` · `CloseAllTabs()` — Closes every tab.

### DocumentTabItem

One document tab: icon, title, a dirty dot and a close button.

Inherits `System.Windows.Controls.TabItem`.
- **Dependency properties (4):** `Icon` — Glyph shown before the title., `IsDirty` — Shows the unsaved dot. The control never clears it — saving is the host's business., `CanClose` — Show the close button. Default true., `CloseCommand` — Optional close command. When set, runs it instead of removing the tab, so a host can cancel or prompt first.
- **Routed events:** `CloseRequested` — Raised when the close button is pressed.
- **CLR events:** `CloseRequested` — Raised when the close button is pressed.
- **Methods (2):** `Close()` — Asks to close this tab. · `OnApplyTemplate()`

### DocumentTileMode *(enum)*

How the open documents share the workspace.

Inherits `System.Enum`.

### DocumentWorkspace

The document area: a tab strip, a layout host that can show one view or tile several, a placeholder for the empty state, and floating windows.

Inherits `System.Windows.Controls.Grid`.
- **CLR events:** `DocumentClosing` — Raised before a document closes; set to keep it., `DocumentAdded` — Raised after a document was added., `DocumentClosed` — Raised after a document was closed., `ActiveDocumentChanged` — Raised when the active document changes., `DocumentFloated` — Raised after a document was floated into its own window.
- **Properties (6):** `Documents: ObservableCollection<DocumentPane>` — The open documents, in tab order., `ActiveDocument: DocumentPane` — The document the workspace is showing; null when there are none., `TileMode: DocumentTileMode` — How the documents share the area., `PlaceholderTitle: string` — Caption shown when nothing is open., `PlaceholderText: string` — Second line of the placeholder., `PaneCount: int` — How many panes the layout is currently showing; a floating document is not one.
- **Methods (6):** `AddDocument(string title, object content, IconKind icon)` · `AddDocument(DocumentPane pane)` · `CloseDocument(DocumentPane pane)` · `CloseOthers(DocumentPane keep)` · `CloseAll()` — Closes every document, honouring the same veto. · `FloatDocument(DocumentPane pane)`

### DragGhost *(static class)*

A small window that follows the pointer while a drag is in progress, so the user can see what they are carrying.
- **Properties (1):** `IsVisible: bool` — True while the ghost is on screen.
- **Methods (3):** `Show(string caption, IconKind icon)` · `Move(Visual reference, Point point)` · `Hide()` — Takes the ghost off screen. Safe to call when it is not showing.

### Drawer


Inherits `System.Windows.Controls.ContentControl`.
- **Dependency properties (5):** `IsOpen`, `Placement`, `DrawerWidth`, `DrawerHeight`, `Title`
- **Methods (1):** `OnApplyTemplate()`

### DrawerMenu

DrawerMenu navigation control for collapsible side navigation. Features smooth animated collapse/expand, mask overlay, header/footer branding, search filter, and menu item selection.

Inherits `System.Windows.Controls.HeaderedItemsControl`.
- **Dependency properties (6):** `IsOpen`, `CompactWidth`, `ExpandedWidth`, `SelectedItem`, `Footer`, `ShowToggleButton`
- **Routed events:** `SelectionChanged`, `IsOpenChanged`
- **CLR events:** `SelectionChanged`, `IsOpenChanged`

### DrawerMenuItem


Inherits `System.Windows.DependencyObject`.
- **Dependency properties (8):** `Icon`, `Text`, `Badge`, `HasBadge`, `IsSelected`, `Tag`, `Command`, `Children`

### DrawerPlacement *(enum)*


Inherits `System.Enum`.
*No public members.*

### DropTargetHighlight *(static class)*

Draws a highlight over any element while a drag is hovering it — the "you can drop here" outline, without the element having to know about drag and drop.
- **Dependency properties (3):** `IsEnabled`, `Brush`, `CornerRadius`
- **Attached properties:** `Brush`, `CornerRadius`, `IsEnabled`
- **Methods (6):** `GetIsEnabled(DependencyObject element)` · `SetIsEnabled(DependencyObject element, bool value)` · `GetBrush(DependencyObject element)` · `SetBrush(DependencyObject element, Brush value)` · `GetCornerRadius(DependencyObject element)` · `SetCornerRadius(DependencyObject element, CornerRadius value)`

### DynamicInput

The input box that follows the cursor while a drawing command is running, so a length or a coordinate can be typed where the user is looking instead of at the bottom of the window.

Inherits `System.Windows.Controls.Grid`.
- **Dependency properties (17):** `Prompt` — The question shown above the box., `Text` — The typed text., `Anchor` — The point the box hangs off., `Offset` — Gap in pixels between and the box., `Kind` — What the box is asking for., `IsValid` — Whether the current text is readable., `ValidationMessage` — The reason the last commit was refused., `IsOpen` — Whether the box is on screen., `Variant` — How the box is dressed., `Placement` — Where the box sits relative to its anchor., `CornerRadius` — Corner radius of the plate., `Elevation` — Depth of the plate's shadow, 0-4., `UnitSuffix` — What the value is measured in; empty means the unit of ., `ShowUnit` — Whether the unit is printed beside the question., `ShowLeader` — Whether a dashed leader runs from the plate back to the anchor., `FieldWidth` — Width of the value field in pixels., `SelectAllOnOpen` — Whether opening the box selects what is already in it.
- **CLR events:** `ValueCommitted` — Raised when the typed value parsed. The box has already closed., `Cancelled` — Raised when the input was abandoned through or Esc.
- **Methods (5):** `Open(Point anchor, string prompt, string prefill)` · `MoveTo(Point anchor)` · `Commit()` — Parses the text and closes the box. Returns false and writes the reason into when the text does not read as the requested kind. · `Cancel()` — Abandons the input: clears the text, closes the box and raises . · `TryParse(DynamicInputKind kind, string text, Double& number, Point3D& point)`

### DynamicInputKind *(enum)*

What a box is asking for.

Inherits `System.Enum`.

### DynamicInputPlacement *(enum)*

Where the box sits relative to the point it hangs off.

Inherits `System.Enum`.

### DynamicInputValueEventArgs

The value a accepted.

Inherits `System.EventArgs`.
- **Properties (4):** `Kind: DynamicInputKind` — What the box was asking for., `Text: string` — The text exactly as typed, so a host can echo it into the command history., `Number: double` — Millimetres for , degrees for ., `Point: Nullable<Point3D>` — The point for ; null for the other kinds.

### DynamicInputVariant *(enum)*

How a box is dressed.

Inherits `System.Enum`.

### EmptyState

A configurable empty-state placeholder with icon (or image), title, description and up to two action buttons. Supports horizontal and vertical layout, custom icon/text sizes, and secondary actions.

Inherits `System.Windows.Controls.Control`.
- **Dependency properties (17):** `Icon`, `Title`, `Description`, `ActionButtonText`, `ActionButtonIcon`, `ActionCommand`, `SecondaryActionText`, `ExtraContent`, `SecondaryActionCommand`, `SecondaryActionIcon`, `IconSize`, `IconBrush`, `TitleFontSize`, `DescriptionFontSize`, `Orientation`, `ImageSource`, `HasActions`

### Expander

An animated panel that shows or hides its content by expanding or collapsing in a chosen direction. Adds header/content styling and routed / events on top of the plain WPF base.

Inherits `System.Windows.Controls.HeaderedContentControl`.
- **Dependency properties (11):** `IsExpanded`, `ExpandDirection`, `CornerRadius`, `AnimationDuration`, `IsAnimated`, `HeaderBackground`, `HeaderForeground`, `HeaderPadding`, `ContentPadding`, `ShowToggleIcon`, `ToggleIconSize`
- **Routed events:** `Expanded`, `Collapsed`
- **CLR events:** `Expanded`, `Collapsed`
- **Methods (1):** `OnApplyTemplate()`

### FileDropEventArgs

Files a accepted.

Inherits `System.EventArgs`.
- **Properties (1):** `Files: IReadOnlyList<string>` — The accepted paths, in drop order.

### FileDropZone

A drop area for files: drag files onto it and it reports them, with a visual state that says whether the drag will be accepted.

Inherits `System.Windows.Controls.Control`.
- **Dependency properties (6):** `Filter` — Semicolon-separated patterns, e.g. *.dwg;*.dxf. * accepts anything., `AllowMultiple` — Accept more than one file per drop. Default true., `Caption` — Main line of text., `Hint` — Second line of text., `IsDragOver` — True while a drag that this zone accepts is over it — the template highlights on it., `IsDragRejected` — True while a drag is over the zone but the files do not pass .
- **CLR events:** `FilesDropped` — Raised when files are dropped or picked. e.Files holds the full paths., `BrowseRequested` — Raised when the user clicks the zone. Open a picker and call .
- **Properties (1):** `Files: ObservableCollection<string>` — The files the zone has taken, most recent last.
- **Methods (3):** `AddFiles(IEnumerable<string> paths)` · `Clear()` — Empties . · `Matches(string path)`

### FilePicker


Inherits `System.Windows.Controls.Control`.
- **Dependency properties (3):** `SelectedPath`, `Filter`, `Title`
- **Methods (1):** `OnApplyTemplate()`

### FilterOperator *(enum)*

Comparison operators available for a .

Inherits `System.Enum`.

### FontPicker

A font chooser: family, size, bold and italic, with a live sample underneath.

Inherits `System.Windows.Controls.Grid`.
- **Dependency properties (6):** `SelectedFontFamily` — The chosen family., `SelectedFontSize` — The chosen size in device-independent points., `IsBold` — Whether the text is bold., `IsItalic` — Whether the text is italic., `ShowPreview` — Shows the sample line under the controls., `PreviewText` — The text the sample shows.
- **CLR events:** `SelectionChanged` — Raised when any of the four values changes.
- **Properties (2):** `EffectiveWeight: FontWeight` — The weight the chosen values amount to., `EffectiveStyle: FontStyle` — The style the chosen values amount to.

### FrostedGlass

A frosted glass panel: it paints a blurred copy of the element behind it, tints it, and draws a hairline edge — the translucent card used over photos and gradient backdrops.

Inherits `System.Windows.Controls.ContentControl`.
- **Dependency properties (5):** `Source` — The element behind the panel. Without it the panel is just a tinted card., `BlurRadius` — Blur strength in pixels. Default 18., `Tint` — Colour laid over the blur — keep it semi-transparent. Empty takes a light veil., `Corner` — Corner radius of the panel. Default 14., `EdgeBrush` — Hairline around the panel. Empty takes CONCOST.Brush.Border.
- **Methods (1):** `OnApplyTemplate()`

### GradientText

Text filled with an animated gradient — the “gradient headline” used on modern product pages. Rendered directly so the gradient can move without touching a template.

Inherits `System.Windows.FrameworkElement`.
- **Dependency properties (8):** `Text`, `FontSize`, `FontWeight`, `StartColor`, `EndColor`, `Angle`, `Speed` — Gradient cycles per second (0.35 = one full pass every ~3 s)., `IsAnimated`

### GridPattern

Tiled dot / line / cross pattern used as an engineering backdrop (drawing paper, blueprint, dashboard grid). Tiled with a so the cost is a single fill regardless of how many cells are visible.

Inherits `System.Windows.FrameworkElement`.
- **Dependency properties (8):** `CellSize`, `Pattern`, `DotRadius`, `StrokeThickness`, `Stroke`, `OffsetX` — Horizontal phase of the pattern — animate it for a parallax backdrop., `OffsetY`, `PatternOpacity`

### GridPatternType *(enum)*


Inherits `System.Enum`.
*No public members.*

### GroupDescriptor

One level of grouping inside the .
- **CLR events:** `PropertyChanged`
- **Properties (3):** `Header: string`, `PropertyPath: string`, `Level: int`

### Growl


Inherits `System.Windows.Controls.Control`.
- **Methods (16):** `RegisterPanel(StackPanel panel)` · `Info(string message, int waitSeconds)` · `Success(string message, int waitSeconds)` · `Warning(string message, int waitSeconds)` · `Error(string message, int waitSeconds)` · `Fatal(string message, int waitSeconds)` · `Ask(string message, Action onConfirm, Action onCancel)` · `Clear()` · `InfoGlobal(string message, int waitSeconds)` · `SuccessGlobal(string message, int waitSeconds)` · `WarningGlobal(string message, int waitSeconds)` · `ErrorGlobal(string message, int waitSeconds)` · `FatalGlobal(string message, int waitSeconds)` · `AskGlobal(string message, Action onConfirm, Action onCancel)` · `ClearGlobal()` · `Close(GrowlItem item)`

### GrowlItem


Inherits `System.Windows.Controls.ContentControl`.
- **Dependency properties (4):** `Message`, `GrowlType`, `TimeString`, `ShowCloseButton`
- **Properties (2):** `OnConfirm: Action`, `OnCancel: Action`
- **Methods (1):** `OnApplyTemplate()`

### GrowlType *(enum)*


Inherits `System.Enum`.
*No public members.*

### GrowlWindow


Inherits `System.Windows.Window`.
- **Properties (1):** `GrowlPanel: StackPanel`

### HeatmapCalendar

GitHub-style contribution heatmap: one rounded cell per day, coloured by intensity, with optional month labels. Useful for daily concrete pours, inspections or site logs.

Inherits `System.Windows.FrameworkElement`.
- **Dependency properties (13):** `Values`, `StartDate`, `CellSize`, `CellGap`, `CornerRadius`, `BaseBrush`, `Level1Brush`, `Level2Brush`, `Level3Brush`, `Level4Brush`, `ShowMonthLabels`, `MonthLabelBrush`, `MonthLabelFontSize`

### HideIsolateAction *(enum)*

A hide or isolate request from the view control bar.

Inherits `System.Enum`.

### HideIsolateEventArgs

Carries a hide or isolate request.

Inherits `System.EventArgs`.
- **Properties (1):** `Action: HideIsolateAction` — What the user asked for.

### ImageViewer

A zoomable, pannable, rotatable picture — the sheet you want to look at when a plan is on screen.

Inherits `System.Windows.FrameworkElement`.
- **Dependency properties (7):** `Source` — The picture to show., `Zoom` — Scale factor; clamped to ..., `Rotation` — Clockwise rotation in degrees., `Offset` — Top-left corner of the picture, in control coordinates., `MinZoom` — Smallest allowed zoom., `MaxZoom` — Largest allowed zoom., `ShowCheckerboard` — Draws a grey checker behind the picture, so transparent drawings read as transparent.
- **Properties (1):** `HasImage: bool` — True when a picture is loaded and it has a usable size.
- **Methods (6):** `GetImageBounds()` — The rectangle the picture covers, including rotation and pan. · `Fit()` — Zooms so the whole picture fits, and centres it. · `Reset()` — Puts the picture back at 100 %, unrotated and centred. · `RotateClockwise()` — Rotates a quarter turn clockwise; normalised to 0..359. · `ZoomAt(Point anchor, double factor)` · `Centre()` — Centres the picture in the control.

### KanbanBoard

A board of lanes and draggable cards — the "to do / doing / done" wall.

Inherits `System.Windows.Controls.Control`.
- **Dependency properties (1):** `Columns` — The lanes of the board, left to right.
- **CLR events:** `CardMoved` — Raised after a card changed lane or position.
- **Methods (2):** `MoveCard(KanbanCard card, KanbanColumn target, int index)` · `ColumnOf(KanbanCard card)`

### KanbanCard

A card on a . It is a , so a host can put anything below the title without teaching the board about it.

Inherits `System.Windows.Controls.ContentControl`.
- **Dependency properties (4):** `Title` — Short caption at the top of the card., `Chip` — Chip text — a discipline, a floor, an owner., `Accent` — Optional colour for the chip and the stripe down the card., `IsDone` — Dimmed when set, which a "done" column uses to read at a glance.

### KanbanCardMovedEventArgs

Describes a card that changed lane or position.

Inherits `System.EventArgs`.
- **Properties (5):** `Card: KanbanCard` — The card that moved., `FromColumn: KanbanColumn` — The lane it came from, or null when it was not on the board yet., `ToColumn: KanbanColumn` — The lane it landed in., `FromIndex: int` — Its old position, or -1., `ToIndex: int` — Its new position.

### KanbanColumn

A lane of a : a header, a count and its cards.

Inherits `System.Windows.Controls.HeaderedItemsControl`.
- **Dependency properties (3):** `Accent` — Colour of the dot next to the header., `IsDropTarget` — Set while a card hovers over the lane, so the template can highlight it., `Maximum` — Soft limit shown next to the count; zero means no limit.
- **Properties (1):** `Count: int` — Number of cards in the lane.

### LoadingBase


Inherits `System.Windows.Controls.Control`.
- **Dependency properties (1):** `IsLoading`

### LoadingCircle


Inherits `CONCOST.UI.Controls.LoadingBase`.
- **Dependency properties (1):** `DotSize`

### LoadingIndicator

A single control with four indeterminate loading looks, chosen by . It paints itself through , so the Windows "Show animations" setting (or ) settles it on a static first frame instead of burning frames nobody asked for, and pauses it without hiding it.

Inherits `CONCOST.UI.Controls.AnimatedEffectBase`.
- **Dependency properties (3):** `IndicatorType`, `DotSize`, `Foreground` — Colour of the dots and bars; the theme default is the accent.
- **Properties (1):** `Phase: double` — How far the current animation loop has run, 0–1 — the phase the glyphs are painted at.

### LoadingIndicatorType *(enum)*


Inherits `System.Enum`.
*No public members.*

### LoadingLine


Inherits `CONCOST.UI.Controls.LoadingBase`.
- **Dependency properties (1):** `DotSize`

### LoadingOverlay


Inherits `System.Windows.Controls.ContentControl`.
- **Dependency properties (10):** `IsLoading`, `Message`, `SubMessage`, `Progress`, `IsIndeterminate`, `IndicatorType`, `Variant`, `GlowColor`, `ShowProgressRing`, `BlurRadius`

### LoadingOverlayVariant *(enum)*


Inherits `System.Enum`.
*No public members.*

### MagneticHover *(static class)*

Attached behaviour that pulls an element slightly toward the pointer — the “magnetic button” effect. Movement is a , so it never triggers a layout pass, and it is skipped entirely when motion is disabled.
- **Dependency properties (3):** `IsEnabled`, `Strength`, `MaxOffset`
- **Attached properties:** `IsEnabled`, `MaxOffset`, `Strength`
- **Methods (6):** `SetIsEnabled(DependencyObject element, bool value)` · `GetIsEnabled(DependencyObject element)` · `SetStrength(DependencyObject element, double value)` · `GetStrength(DependencyObject element)` · `SetMaxOffset(DependencyObject element, double value)` · `GetMaxOffset(DependencyObject element)`

### Magnifier

Magnifier control that provides a floating zoom lens over any visual element. Supports configurable zoom factor, lens radius, border brush, and border thickness.

Inherits `System.Windows.Controls.Control`.
- **Dependency properties (10):** `Target`, `Scale`, `Radius`, `ShowCrosshair`, `CrosshairBrush` — Colour of the crosshair and centre mark; the ring brush when unset., `ShowRim` — Draws a light inner rim between the ring and the image. On a drawing the zoomed content is often the same colour as the ring, and the rim is what keeps the lens readable as a lens., `ShowShadow` — Draws a soft drop under the lens, so it reads as floating above the drawing., `FollowPointer` — Moves the lens with the pointer. Turn it off when the host drives from code — a digitizer, a camera feed, a scripted zoom — so the pointer stops fighting the host., `Center`, `IsActive`

### MarkdownParser *(static class)*

Renders a useful subset of Markdown into a : headings, paragraphs, emphasis, inline code, fenced code, lists, quotes, rules and links.
- **Methods (4):** `Parse(string markdown)` · `Parse(string markdown, MarkdownRenderTheme theme)` · `ParseInlines(string text)` · `ParseInlines(string text, MarkdownRenderTheme palette)`

### MarkdownRenderTheme

The colours a paints its decorations with: rules, quote bars, code frames and inline code. A null brush means "use the theme default", which is what keeps a document pane on the right theme when the shell flips between light and dark.
- **Properties (6):** `RuleBrush: Brush` — The horizontal rule., `QuoteBrush: Brush` — The bar down the left of a block quote., `CodeBorderBrush: Brush` — The frame around a fenced code block., `CodeBackgroundBrush: Brush` — The wash behind a fenced code block; null draws no wash., `InlineCodeBackgroundBrush: Brush` — The wash behind inline code., `HyperlinkBrush: Brush` — The colour of hyperlinks.
- **Methods (1):** `FromResources()` — Resolves every brush from the CONCOST.Brush.* tokens in force.

### MarkdownViewer

Shows Markdown from a help file, a release note or a README without pulling in a markdown engine.

Inherits `System.Windows.Controls.Control`.
- **Dependency properties (4):** `Markdown` — Source text., `RenderTheme` — The colours the decorations — rules, quotes, code frames, inline code, links — are drawn with. Null reads the CONCOST.Brush.* tokens in force, which is what follows the theme swap; a host that tints one panel differently sets its own., `Document` — The parsed document, ready to be shown or printed., `LinkCount` — Number of hyperlinks in the current document.
- **Methods (1):** `Rebuild()` — Re-parses ; call it after mutating the text in place.

### MarqueeTicker

Continuously scrolling text strip (marquee) for status bars, site notices and tickers. Rendered directly and driven by , so it stays smooth without a Storyboard per item.

Inherits `System.Windows.FrameworkElement`.
- **Dependency properties (7):** `Text`, `Speed`, `Gap`, `FontSize`, `Foreground`, `Separator`, `IsRunning`

### MasonryPanel

A waterfall panel: every child keeps its own height and drops into the shortest column, so a gallery of cards of unequal height reads as a wall instead of a grid with ragged gaps.

Inherits `System.Windows.Controls.Panel`.
- **Dependency properties (5):** `Columns` — Number of columns. 0 (the default) fits as many columns of at least as the width allows., `MinColumnWidth` — Narrowest a column may become when is left at 0. Default 220., `ColumnSpacing` — Gap between two columns. Default 12., `RowSpacing` — Gap between two cards stacked in the same column. Default 12., `IsBalanced` — When true (the default) each card goes to the shortest column. When false the cards stay in reading order and fill column 0 up to its share of the total height before column 1 starts, which suits a list that is scanned rather than looked at.
- **Methods (2):** `GetColumnCount(double availableWidth)` · `GetColumnWidth(double availableWidth)`

### MeshEdge *(struct)*

An edge between two vertices of a mesh.

Inherits `System.ValueType`.
- **Properties (2):** `A: int` — Index of the first vertex., `B: int` — Index of the second vertex.

### MiniMap

The whole drawing at a glance, with a rectangle you can drag to move the view — the corner map every CAD viewport has.

Inherits `System.Windows.FrameworkElement`.
- **Dependency properties (8):** `ContentBounds` — Extents of the whole drawing, in the same units as ., `Viewport` — The visible part of the drawing, in world coordinates., `ItemsSource` — The blocks to draw, usually one per wall or sheet region., `ItemBrush` — Default block colour., `ViewportBrush` — Fill of the viewport rectangle., `ViewportFillOpacity` — Strength of the viewport rectangle's tint, 0–1. The rectangle sits over the very items the user is looking for, and a full-strength fill paints over them; the stroke carries the accent at full strength either way., `CornerRadius` — Rounds the map's corners, to match the panel it docks into., `ShowBorder` — Draws the map's outline; off when a host already frames the panel it lives in.
- **CLR events:** `ViewportChanged` — Raised while the viewport rectangle is dragged.
- **Properties (1):** `Scale: double` — Scale that maps a world unit to a pixel, honouring letterboxing.
- **Methods (4):** `WorldToMap(Point world)` · `MapToWorld(Point point)` · `CentreOn(Point world)` · `Fit()` — Fits the viewport to the whole drawing.

### MiniMapItem

One block drawn on a : a wall, a sheet border, a zone.
- **Properties (2):** `Bounds: Rect` — Position and size in the map's world coordinates., `Brush: Brush` — Colour; falls back to .

### MiniMapViewportChangedEventArgs

Reports a viewport the user dragged on the .

Inherits `System.EventArgs`.
- **Properties (2):** `Viewport: Rect` — The new viewport, in world coordinates., `IsFinal: bool` — true on the last event of a drag, so a host can defer the heavy redraw.

### ModelViewer3D

A wireframe viewer for a building mass or a mechanical part: orbit with the left button, pan with the right one, zoom with the wheel.

Inherits `System.Windows.FrameworkElement`.
- **Dependency properties (9):** `Vertices` — The mesh vertices, in model units., `Edges` — The mesh edges, as pairs of vertex indices., `Yaw` — Rotation around the vertical axis, in degrees., `Pitch` — Rotation around the horizontal axis, in degrees; clamped to -89..89., `Zoom` — Scale multiplier, clamped to 0.1..20., `Pan` — Screen-space offset from the centre of the control., `ShowAxes` — Draws the X/Y/Z gizmo in the corner., `EdgeBrush` — Colour of the wireframe., `Background` — Custom brush for the viewport background; null leaves the background transparent.
- **Methods (5):** `SetMesh(IEnumerable<Point3D> vertices, IEnumerable<MeshEdge> edges)` · `LoadBox(Point3D corner, double width, double depth, double height)` · `ResetView()` — Puts the camera back to the default isometric-ish view. · `Orbit(double deltaX, double deltaY)` · `Project(Point3D point, Point& screen)`

### MultiSelectDataGrid

A with a checkbox column for multi-selection, plus a tri-state header checkbox that selects or clears everything.

Inherits `System.Windows.Controls.DataGrid`.
- **Dependency properties (4):** `ShowCheckBoxColumn` — Draw the checkbox column. Default true., `CheckBoxColumnWidth` — Width of the checkbox column, in pixels. Default 44., `IsAllSelected` — Header checkbox state: true when every row is selected, false when none is, null when only some are. Setting it selects or clears everything., `SelectedCount` — How many rows are selected.
- **Properties (1):** `SelectedItemsList: IList` — The selected items, in selection order, with change notifications.
- **Methods (5):** `SelectEverything()` — Selects every row. · `ClearEverything()` — Clears the selection. · `InvertSelection()` — Flips the selected state of every row. · `ToggleItem(object item)` · `IsItemSelected(object item)`

### NavigationBar

The floating navigation bar that sits over a viewport: zoom in, zoom out, zoom to fit, the pan and orbit tools, and the coordinate-system label.

Inherits `System.Windows.Controls.Grid`.
- **Dependency properties (13):** `ActiveTool` — The tool that is currently armed. Pan, orbit and walk are mutually exclusive., `Ucs` — The orientation the view is in, as shown on the coordinate-system button., `ShowWalk` — Shows the walk tool. Hidden by default in hosts that have no walk controller., `Variant` — How the bar is dressed: over a viewport, inside a toolbar, over a busy 3D scene., `Orientation` — Lays the commands out along a row or down a column. The dividers follow., `ButtonSize` — Square size of every command button, in pixels. 26 is the CAD default; 34 is a touch target., `IconSize` — Size of the glyph inside a command button, in pixels., `CornerRadius` — Corner radius of the frame. Half the height makes a pill., `Elevation` — Shadow depth of the frame, 0 to 4. Ignored by the seamless variant., `ShowUcsLabel` — Shows the coordinate-system label and its menu. Off leaves a bar of arrows only., `ShowZoom` — Shows the zoom in / zoom out / zoom to fit cluster., `ShowPerspective` — Shows the perspective / orthographic button at the end of the bar., `ShowDividers` — Shows the rules between groups. A rule also steps aside on its own when one of the two groups beside it is hidden, so a collapsed cluster never leaves a floating line behind.
- **CLR events:** `NavigationRequested` — Raised when a one-shot button is pressed., `ActiveToolChanged` — Raised when the active tool changes, including when it is turned off., `UcsChanged` — Raised when the coordinate system changes.
- **Methods (1):** `SetActiveTool(NavigationTool tool)`

### NavigationBarVariant *(enum)*

How a is dressed.

Inherits `System.Enum`.

### NavigationCommand *(enum)*

A one-shot navigation action a asks the host to perform.

Inherits `System.Enum`.

### NavigationRequestedEventArgs

Carries the command a button stands for.

Inherits `System.EventArgs`.
- **Properties (1):** `Command: NavigationCommand` — The action the user asked for.

### NavigationTool *(enum)*

A navigation tool that stays active until it is switched off.

Inherits `System.Enum`.

### NumericThresholdToBrushConverter

- **Properties (4):** `Threshold: double`, `BelowBrush: Brush`, `AboveBrush: Brush`, `DefaultBrush: Brush`
- **Methods (2):** `Convert(object value, Type targetType, object parameter, CultureInfo culture)` · `ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)`

### NumericUpDown

A culture-aware numeric editor with bounded keyboard and button stepping.

Inherits `System.Windows.Controls.Control`.
- **Dependency properties (18):** `Value`, `Minimum`, `Maximum`, `Increment`, `DecimalPlaces`, `Suffix`, `Prefix`, `FormatString` — Gets or sets a .NET numeric format string. When empty, DecimalPlaces controls the display precision., `CanIncrease`, `CanDecrease`, `LargeIncrement`, `IsReadOnly`, `ShowButtons`, `ButtonPlacement`, `CornerRadius`, `TextAlignment`, `NullValue`, `IsNullable`
- **Routed events:** `ValueChanged`
- **CLR events:** `ValueChanged`
- **Methods (1):** `OnApplyTemplate()`

### OdometerNumber

Numeric readout that rolls from the previous value to the new one — the "number ticker" pattern used by dashboards, without a XAML animation per value.

Inherits `System.Windows.Controls.Control`.
- **Dependency properties (8):** `DisplayText`, `Value`, `Format`, `Prefix`, `Suffix`, `Duration`, `AnimateOnLoad`, `DecimalPlaces`

### OtpInput

A fixed-length verification code entry — the one-time password field of an MFA dialog. Each digit lives in its own cell: typing advances, Backspace retreats, the arrow keys walk the cells and a pasted code fills the cells from wherever the caret stands. fires when the last cell receives its character.

Inherits `System.Windows.Controls.Control`.
- **Dependency properties (9):** `Value` — The code entered so far, as one string., `Length` — Number of cells; the control carries that many characters exactly., `NumericOnly` — Accepts digits only when true (the default for a verification code); any character when false., `MaskCharacter` — Displays this character instead of the typed one; \\0 shows the code as typed., `ItemWidth`, `ItemHeight`, `ItemSpacing`, `ItemCornerRadius`, `IsCompleted` — True when every cell carries a character.
- **Routed events:** `Completed` — Raised when the last cell receives its character.
- **CLR events:** `Completed` — Raised when the last cell receives its character.
- **Methods (2):** `Clear()` — Clears every cell and returns focus to the first one. · `OnApplyTemplate()`

### OverflowPanel

A single-row panel that lays items out left to right and stops when the width runs out.

Inherits `System.Windows.Controls.Panel`.
- **Dependency properties (2):** `OverflowCount` — How many items did not fit in the last layout., `Spacing`
- **Properties (1):** `VisibleCount: int` — Number of leading children that fit.

### OverflowToolbar *(abstract)*

Shared plumbing for a toolbar that overflows into a menu: the items, the ones that did not fit, and the open/closed state of the overflow popup.

Inherits `System.Windows.Controls.Control`.
- **Dependency properties (5):** `ItemsSource` — The commands shown in the bar., `Spacing` — Space between buttons, in pixels. Default 2., `IsOverflowOpen` — The overflow menu is open., `HasOverflow` — At least one item did not fit., `OverflowItems` — The items that did not fit, in order. Raises change notifications as a collection.
- **Methods (3):** `OnApplyTemplate()` · `Invoke(object item)` · `SyncOverflow()` — Recomputes from the current layout.

### PageTransition *(enum)*

How a brings new content in.

Inherits `System.Enum`.

### PageTransitionHost

A content host that animates the swap instead of snapping: drop a page, a wizard step or a tab body in and the new content fades / slides into place.

Inherits `System.Windows.Controls.ContentControl`.
- **Dependency properties (4):** `Transition` — Which entrance to play when the content changes. Defaults to ., `Duration` — How long the entrance takes. Defaults to 260 ms., `IsTransitionEnabled` — Set to false to swap content without any animation., `Offset` — How far the content travels, in device-independent pixels. Defaults to 22.
- **Properties (1):** `IsTransitioning: bool` — True while an entrance animation is running.
- **Methods (1):** `Play()` — Replays the entrance for the current content.

### Pagination


Inherits `System.Windows.Controls.Control`.
- **Dependency properties (4):** `CurrentPage`, `MaxPage`, `TotalItems`, `PageSize`
- **Routed events:** `PageChanged`
- **CLR events:** `PageChanged`
- **Properties (1):** `TotalPages: int`
- **Methods (1):** `OnApplyTemplate()`

### ParallaxPanel

Layer that drifts slower (or faster) than the page as it scrolls — the parallax depth trick. Put it behind the content and give it a speed below 1; it translates itself, so nothing inside has to know about the scroll position.

Inherits `System.Windows.Controls.ContentControl`.
- **Dependency properties (4):** `Target` — Scroll viewer to follow. Leave empty to use the nearest ancestor., `Speed` — Fraction of the scroll distance the layer moves (0.1–0.4 = subtle depth)., `MaxOffset` — Clamp so the layer never drifts more than this many pixels., `Axis`

### ParticleField

Ambient field of drifting particles for hero panels, splash screens and empty states. Particles wrap around the edges, so the field never empties however long it runs.

Inherits `CONCOST.UI.Controls.AnimatedEffectBase`.
- **Dependency properties (10):** `ParticleCount` — How many particles to draw., `MinRadius` — Smallest particle radius., `MaxRadius` — Largest particle radius., `Speed` — Base drift speed in device-independent pixels per second., `Direction` — Drift direction in degrees; 0 is right, 90 is down. Defaults to 270 (upward)., `Spread` — Random jitter added to each particle's direction, as a fraction of the unit circle., `ParticleBrush` — Particle colour. Defaults to CONCOST.Brush.Primary., `ParticleOpacity` — Overall opacity of the field, 0–1., `ConnectDistance` — Draws a line between particles closer than this many pixels. 0 turns the constellation off., `Seed` — Seed for the particle layout, so the same field can be reproduced.
- **Properties (1):** `ActiveParticleCount: int` — Number of particles currently alive. Exposed for tests.

### PasswordInput


Inherits `System.Windows.Controls.Control`.
- **Dependency properties (4):** `Password`, `Placeholder`, `ShowPassword`, `IconKind`
- **Methods (1):** `OnApplyTemplate()`

### PickerSelectionMode *(enum)*

How many nodes a lets the user pick.

Inherits `System.Enum`.

### PinBox


Inherits `System.Windows.Controls.Control`.
- **Dependency properties (8):** `Length`, `Password`, `PasswordChar`, `IsPassword`, `ItemWidth`, `ItemHeight`, `ItemMargin`, `ItemCornerRadius`
- **Methods (1):** `OnApplyTemplate()`

### ProjectBrowser

The project browser: a header of two short rows, a tree whose nodes carry their own context menu, and a footer that counts what the filter left.

Inherits `System.Windows.Controls.Grid`.
- **Dependency properties (16):** `Variant` — What the browser is standing on; Seamless draws no plate at all., `CornerRadius` — Corner of the plate., `Elevation` — Shadow level of the plate, 0-4; Seamless ignores it., `ControlHeight` — Height of the search box, both pickers and every tool button., `IconSize` — Size of the tool glyphs and of the node icons., `ShowSearch` — Shows the search field., `ShowKindFilter` — Shows the kind filter beside the search., `ShowSort` — Shows the sort picker in the tool row., `ShowGroup` — Shows the group-by-kind switch., `ShowToolbar` — Shows the expand / collapse / refresh tools and the group switch., `ShowCount` — Shows the footer count; the footer takes its rule with it., `SearchPlaceholder` — Placeholder of the search field., `AllKindsCaption` — Caption of the first entry of the kind filter., `SortCaptions` — The three sort captions, in enum order; a short or missing list keeps the defaults., `EmptyText` — What the tree says when the filter leaves nothing., `CountCaption` — Footer format, with {0} for the number of visible nodes.
- **CLR events:** `OpenRequested` — Raised when a node is activated — double-clicked or opened from its menu., `RenameRequested` — Raised from the "Rename" menu item., `DeleteRequested` — Raised from the "Delete" menu item., `DuplicateRequested` — Raised from the "Duplicate" menu item., `PropertiesRequested` — Raised from the "Properties" menu item., `SelectionChanged` — Raised when the selected node changes.
- **Properties (7):** `Nodes: ObservableCollection<ProjectTreeNode>` — The root nodes., `FilterText: string` — The text in the search box., `SelectedNode: ProjectTreeNode` — The node that is currently selected., `GroupMode: ProjectGroupMode` — Grouping mode; setting it rebuilds the tree., `SortMode: ProjectSortMode` — Sorting mode; setting it rebuilds the tree., `VisibleNodes: IReadOnlyList<ProjectTreeNode>` — The nodes the tree is showing, after the search, the kind filter and grouping., `KindFilter: Nullable<ProjectNodeKind>` — The kind filter, or null for every kind.
- **Methods (8):** `AddRoot(string name, ProjectNodeKind kind)` · `SetSearchText(string text)` · `Refresh()` — Rebuilds the tree, honouring the search text, the kind filter, grouping and sorting. · `ExpandAll()` — Expands every branch. · `ScrollToTop()` — Scrolls the tree back to its first node. · `CollapseAll()` — Collapses every branch. · `Select(ProjectTreeNode node)` · `Open(ProjectTreeNode node)`

### ProjectBrowserVariant *(enum)*

What the browser is standing on.

Inherits `System.Enum`.

### ProjectGroupMode *(enum)*

How the tree is grouped.

Inherits `System.Enum`.

### ProjectNodeEventArgs

Carries the node an action was requested on.

Inherits `System.EventArgs`.
- **Properties (1):** `Node: ProjectTreeNode` — The node the user acted on.

### ProjectNodeKind *(enum)*

What a project browser node stands for.

Inherits `System.Enum`.

### ProjectSortMode *(enum)*

How siblings are ordered.

Inherits `System.Enum`.

### ProjectTreeNode

One node of the project tree.
- **Properties (6):** `Name: string` — Caption shown in the tree., `Kind: ProjectNodeKind` — What the node is; drives the icon and the context menu., `IsModified: bool` — Shows the unsaved marker next to the name., `Tag: object` — The model object behind the node, when there is one., `Children: ObservableCollection<ProjectTreeNode>` — Child nodes., `Parent: ProjectTreeNode` — Parent node, or null for a root.
- **Methods (4):** `Add(ProjectTreeNode child)` · `Add(string name, ProjectNodeKind kind)` · `Flatten()` — The node and all of its descendants, depth first. · `ToString()`

### ProjectionModeEventArgs

Raised when the projection is chosen from the context menu.

Inherits `System.EventArgs`.
- **Properties (1):** `Mode: ProjectionMode` — The projection the user picked.

### PropertiesPalette

The properties palette: selection header, type selector, search, the category grid, a validation row and a reset button.

Inherits `System.Windows.Controls.Grid`.
- **CLR events:** `EditTypeRequested` — Raised when "Edit type" is pressed., `TypeChangeRequested` — Raised when another type is picked in the type selector., `PropertyCommitted` — Raised after a parameter was written.
- **Properties (5):** `GridControl: PropertyGrid` — The grid that renders the rows., `Properties: PropertyManager` — The property system the palette reads and writes through., `Selection: SelectionManager` — The selection the palette follows., `Target: object` — The object whose parameters are shown, or null., `ValidationMessage: string` — Last validation message, or null when the last write succeeded.
- **Methods (3):** `Build(object target)` · `Refresh()` — Rebuilds the rows for the current target, e.g. after the model changed underneath. · `ResetProperty(string name, String& error)`

### PropertyCategoryGroup

- **CLR events:** `PropertyChanged`
- **Properties (3):** `CategoryName: string`, `IsExpanded: bool`, `Properties: ObservableCollection<PropertyItem>`

### PropertyEditorType *(enum)*


Inherits `System.Enum`.
*No public members.*

### PropertyGrid


Inherits `System.Windows.Controls.Control`.
- **Dependency properties (3):** `SelectedObject`, `Categories`, `ShowToolbar` — Shows the view switch and the filter box above the rows. A host that already offers its own filter turns it off rather than stacking a second search box over the first.
- **Methods (2):** `OnApplyTemplate()` · `BuildProperties()`

### PropertyItem

- **CLR events:** `PropertyChanged`
- **Properties (19):** `Name: string`, `DisplayName: string`, `Category: string`, `Description: string`, `PropertyType: Type`, `EditorType: PropertyEditorType`, `EnumOptions: Array`, `IsReadOnly: bool`, `ValidationError: string`, `Value: object`, `IsBoolTrue: bool`, `IsAlignLeft: bool`, `IsAlignCenter: bool`, `IsAlignRight: bool`, `IsAlignStretch: bool`, `IsValignTop: bool`, `IsValignCenter: bool`, `IsValignBottom: bool`, `IsValignStretch: bool`

### PropertyReferencePicker

A value cell for a property that points at another model object — a host wall, a level, a material — with a drop-down list instead of free text.

Inherits `System.Windows.Controls.Grid`.
- **Dependency properties (4):** `SelectedItem` — The object currently referenced., `Placeholder` — Text shown when nothing is referenced., `IsReadOnly` — Blocks the picker without disabling it, so the value stays selectable for copying., `SearchPlaceholder` — Watermark of the drop-down's search box; a host localises it like the rest of its shell.
- **CLR events:** `ReferencePicked` — Raised when a different object is picked or the reference is cleared.
- **Properties (2):** `ItemsSource: IEnumerable` — The objects that can be picked., `DisplaySelector: Func<object, string>` — How an object is shown; defaults to ToString().
- **Methods (1):** `TogglePopup()` — Opens or closes the drop-down list.

### PulseRing

Expanding, fading rings used to draw attention to a live state or a selected point (the “pulse” marker from modern dashboards). Renders from the centre of its box.

Inherits `System.Windows.FrameworkElement`.
- **Dependency properties (9):** `RingCount`, `Duration` — Seconds for one ring to travel from the centre to the outer edge., `MaxRadius`, `RingThickness`, `RingBrush`, `PeakOpacity`, `IsActive`, `CenterDot`, `DotRadius`

### QuickAccessItem

One command in a — icon only, no caption.
- **Properties (6):** `Icon: IconKind`, `ToolTip: string`, `Command: ICommand`, `CommandParameter: object`, `IsEnabled: bool`, `IsSeparator: bool`

### QuickAccessToolbar

The compact strip of icon-only commands above a ribbon — the standalone version of the ribbon's quick-access toolbar, usable in a plain window.

Inherits `CONCOST.UI.Controls.OverflowToolbar`.
- **Dependency properties (1):** `IsCustomizable` — Show the "more" chevron that opens the overflow menu. Default true.

### RadialGauge

Circular gauge for KPI readouts: a track arc, a value arc that grows clockwise from , optional tick marks and the value printed in the middle.

Inherits `System.Windows.FrameworkElement`.
- **Dependency properties (17):** `Minimum` — Lowest value the arc represents., `Maximum` — Highest value the arc represents., `Value` — Current value, clamped to –., `StartAngle` — Where the arc starts, in degrees clockwise from 3 o'clock. 135 puts the gap at the bottom., `SweepAngle` — How far the arc travels from , in degrees., `TrackThickness` — Thickness of the track and value arcs, in device-independent pixels., `TrackBrush` — Colour of the unfilled track. Defaults to CONCOST.Brush.SurfaceSecondary., `ValueBrush` — Colour of the filled arc. Defaults to CONCOST.Brush.Primary., `ShowTicks` — Draws ticks around the outside of the arc., `TickCount` — Number of tick marks, counted from the start of the arc to the end., `ShowValue` — Prints the value in the middle of the gauge., `ValueFormat` — Numeric format applied to before is appended., `Suffix` — Unit printed straight after the number, for example % or kN., `Label` — Caption printed under the value., `ValueFontSize` — Size of the value text. Leave 0 to scale with the gauge., `IsAnimated` — Eases value changes instead of snapping to them., `AnimationDuration` — How long the ease takes. Defaults to 520 ms.
- **Properties (1):** `NormalizedValue: double` — Fraction of the arc the current value fills, 0–1.

### RangeSlider

RangeSlider allows selecting a numeric sub-range [StartValue, EndValue] with dual interactive thumbs. Essential for engineering filters, structural dimensions, cost intervals, and load limits. Supports snapping to ticks, value labels, custom brushes and vertical orientation.

Inherits `System.Windows.Controls.Control`.
- **Dependency properties (17):** `Minimum`, `Maximum`, `StartValue`, `EndValue`, `SmallChange`, `Unit`, `IsSnapToTick`, `TickFrequency`, `ShowTicks`, `ShowValueLabels`, `LabelFormat`, `TrackHeight`, `ThumbSize`, `ActiveTrackBrush`, `InactiveTrackBrush`, `ThumbBrush`, `Orientation`
- **Routed events:** `ValueChanged`
- **CLR events:** `ValueChanged`
- **Methods (1):** `OnApplyTemplate()`

### Rating

A star-rating control with support for half-star precision, custom geometry icons, hover preview, clear-on-click, and a formatted score label.

Inherits `System.Windows.Controls.Control`.
- **Dependency properties (14):** `Value`, `Max`, `ItemSize`, `ItemMargin`, `ItemGap`, `IsReadOnly`, `AllowHalf`, `AllowClear`, `ShowScore`, `ScoreFormat`, `ActiveBrush`, `InactiveBrush`, `IconGeometry`, `HoverValue`
- **Routed events:** `ValueChanged`
- **CLR events:** `ValueChanged`
- **Methods (1):** `OnApplyTemplate()`

### RatingToStarsConverter

- **Methods (2):** `Convert(object value, Type targetType, object parameter, CultureInfo culture)` · `ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)`

### ReferencePickedEventArgs

Carries the object picked in a .

Inherits `System.EventArgs`.
- **Properties (1):** `Item: object` — The object that was picked, or null when the selection was cleared.

### RegionHoverEventArgs

Raised when the pointer moves on or off one of the 26 zones.

Inherits `System.EventArgs`.
- **Properties (2):** `Hit: ViewCubeHit` — The zone under the pointer, or an empty hit when there is none., `Region: ViewCubeRegion` — The zone under the pointer, or an empty region.

### RingLoading


Inherits `CONCOST.UI.Controls.LoadingBase`.
*No public members.*

### Ripple

A Material-Design inspired container that produces an expanding circular ripple wave upon pointer interaction, with hardware-accelerated transforms and corner clipping.

Inherits `System.Windows.Controls.ContentControl`.
- **Dependency properties (7):** `RippleBrush`, `MaxOpacity`, `Duration`, `MaxConcurrentRipples` — Maximum number of simultaneous waves retained by this control., `CornerRadius`, `IsRippleEnabled`, `Mode`
- **Methods (2):** `OnApplyTemplate()` · `TriggerRipple(Point origin)`

### RippleElement *(static class)*

Attached properties to easily add Ripple wave animations to any existing WPF control (e.g. Button, Border, Card).
- **Dependency properties (7):** `IsEnabled`, `Brush`, `CornerRadius`, `Mode`, `MaxOpacity`, `Duration`, `MaxConcurrentRipples`
- **Attached properties:** `Brush`, `CornerRadius`, `Duration`, `IsEnabled`, `MaxConcurrentRipples`, `MaxOpacity`, `Mode`
- **Methods (14):** `GetIsEnabled(DependencyObject obj)` · `SetIsEnabled(DependencyObject obj, bool value)` · `GetBrush(DependencyObject obj)` · `SetBrush(DependencyObject obj, Brush value)` · `GetCornerRadius(DependencyObject obj)` · `SetCornerRadius(DependencyObject obj, CornerRadius value)` · `GetMode(DependencyObject obj)` · `SetMode(DependencyObject obj, RippleMode value)` · `GetMaxOpacity(DependencyObject obj)` · `SetMaxOpacity(DependencyObject obj, double value)` · `GetDuration(DependencyObject obj)` · `SetDuration(DependencyObject obj, TimeSpan value)` · `GetMaxConcurrentRipples(DependencyObject obj)` · `SetMaxConcurrentRipples(DependencyObject obj, int value)`

### RippleMode *(enum)*


Inherits `System.Enum`.
*No public members.*

### RollLoading


Inherits `CONCOST.UI.Controls.LoadingBase`.
*No public members.*

### Scheduler

A day timeline: hours run down the left edge, bookings are placed by their time of day and overlapping bookings share the column rather than covering each other.

Inherits `System.Windows.FrameworkElement`.
- **Dependency properties (9):** `ItemsSource` — The bookings to draw. Anything that is not a is ignored., `Date` — The day the timeline shows. Only the date part matters., `StartHour` — Hour the timeline starts at., `EndHour` — Hour the timeline ends at., `HourHeight` — Pixels per hour., `SelectedItem` — The booking the user picked, or null., `ItemBrush` — Default block colour., `ShowNowLine` — Draws a red line at the current time when is today., `NowLineBrush` — Colour of the now line; null reads CONCOST.Brush.Danger from the theme in force.
- **CLR events:** `SelectionChanged` — Raised when the selected booking changes; the item is null when cleared., `ItemActivated` — Raised on a double click on a booking, which a host reads as "open it".
- **Methods (3):** `SelectItem(SchedulerItem item)` · `GetItemBounds(SchedulerItem item)` · `ChangeDay(int days)`

### SchedulerItem

One booking on a day.
- **Properties (7):** `Title: string` — Caption shown inside the block., `Start: DateTime` — Start of the booking; only the time of day is used for the block position., `End: DateTime` — End of the booking. An end before the start is drawn as the shortest visible block., `Resource: string` — Optional lane label, shown in the tooltip the control builds for the block., `Accent: Brush` — Block colour. Falls back to the control's ., `Tag: object` — Anything the host wants to hang off the booking — an id, a model object., `Duration: TimeSpan` — Length of the booking, floored at three minutes so it always has a visible block.
- **Methods (1):** `ToString()`

### SchedulerSelectionChangedEventArgs

Reports the booking a selected or cleared.

Inherits `System.EventArgs`.
- **Properties (1):** `Item: SchedulerItem` — The booking now selected, or null when the selection was cleared.

### ScrollProgressBar

Thin reading-progress bar for a . When no is set it walks up the visual tree to the nearest scroll viewer, so dropping it above a page is usually enough.

Inherits `System.Windows.FrameworkElement`.
- **Dependency properties (6):** `Target` — Scroll viewer to track. Leave empty to use the nearest ancestor., `TrackBrush`, `BarBrush`, `Thickness`, `CornerRadius`, `ShowTrack`
- **Properties (1):** `Fraction: double` — 0–1 reading progress of the tracked scroll viewer.

### ScrollReveal *(static class)*

Attached behaviour that fades and slides an element in the first time it scrolls into view — the scroll-reveal used on modern landing pages. Finds the nearest automatically and stops listening after the reveal.
- **Dependency properties (5):** `IsEnabled`, `Offset`, `Duration`, `Delay`, `Threshold`
- **Attached properties:** `Delay`, `Duration`, `IsEnabled`, `Offset`, `Threshold`
- **Methods (10):** `SetIsEnabled(DependencyObject element, bool value)` · `GetIsEnabled(DependencyObject element)` · `SetOffset(DependencyObject element, double value)` · `GetOffset(DependencyObject element)` · `SetDuration(DependencyObject element, Duration value)` · `GetDuration(DependencyObject element)` · `SetDelay(DependencyObject element, Duration value)` · `GetDelay(DependencyObject element)` · `SetThreshold(DependencyObject element, double value)` · `GetThreshold(DependencyObject element)`

### SearchBox

A search input field with an optional leading icon, clear button, debounced instant-search, minimum-length guard, and a that fires on Enter or after the debounce delay.

Inherits `System.Windows.Controls.Control`.
- **Dependency properties (12):** `Text`, `Placeholder`, `IsInstantSearch`, `SearchDelay`, `MinSearchLength`, `CornerRadius`, `SearchIcon`, `ShowSearchIcon`, `ShowClearButton`, `HasText`, `SearchCommand`, `CommandParameter`
- **Routed events:** `SearchStarted`, `Clear`, `TextChanged`
- **CLR events:** `SearchStarted`, `Clear`, `TextChanged`
- **Methods (1):** `OnApplyTemplate()`

### SearchableTreePicker

A tree with a search box in front of it: type to narrow the tree down to the matching nodes and the branches that lead to them, then pick one node or several.

Inherits `System.Windows.Controls.Control`.
- **Dependency properties (12):** `ItemsSource` — The root nodes. Children are read through ., `ChildrenPath` — Property (or dotted path) that yields a node's children. Default Children., `DisplayPath` — Property (or dotted path) shown for a node, and searched. Default Name., `SearchText` — The term the tree is filtered by. Two-way by default., `IsSearchEnabled` — Show the search box. Default true., `Placeholder` — Hint inside the search box., `IsExpandingMatches` — Open every branch that leads to a match while searching. Default true., `EmptyText` — Shown when the filter leaves nothing., `SelectionMode` — One node or several. Multiple shows a checkbox per row. Default ., `SelectedItem` — The selected node in single mode, or the last one picked., `SelectedItems` — The picked nodes, in the order they were picked., `SelectedCount` — How many nodes are picked.
- **CLR events:** `SelectionChanged` — Raised when the set of selected nodes changes.
- **Properties (1):** `VisibleItems: IEnumerable<object>` — The nodes the current filter leaves on screen, in tree order.
- **Methods (3):** `OnApplyTemplate()` · `ClearSelection()` — Clears the selection without touching the tree. · `SelectAllVisible()` — Picks every node that is currently visible — what "select all" means after a search.

### ShakeElement *(static class)*

Attached behaviour that shakes an element horizontally — the classic “invalid input” feedback. Works on any ; the existing RenderTransform is preserved by wrapping it in a group.
- **Dependency properties (3):** `IsShaking`, `Amplitude`, `Duration`
- **Attached properties:** `Amplitude`, `Duration`, `IsShaking`
- **Methods (7):** `SetIsShaking(DependencyObject element, bool value)` · `GetIsShaking(DependencyObject element)` · `SetAmplitude(DependencyObject element, double value)` · `GetAmplitude(DependencyObject element)` · `SetDuration(DependencyObject element, Duration value)` · `GetDuration(DependencyObject element)` · `Shake(FrameworkElement element)`

### SharedElementTransition *(static class)*

Animates an element from a rectangle it used to occupy to the place it occupies now — the "this card grew out of that thumbnail" move between two views.
- **Methods (2):** `Run(FrameworkElement element, Rect fromRect, double milliseconds, IEasingFunction easing)` · `RunFrom(FrameworkElement element, FrameworkElement origin, double milliseconds)`

### SheenSweep

Diagonal light sweep across a surface — the “sheen” that runs over buttons and cards on modern product pages. Place it in a Grid above the content.

Inherits `System.Windows.FrameworkElement`.
- **Dependency properties (7):** `IsActive`, `Speed` — Sweeps per second (0.5 = one sweep every two seconds)., `BandWidth` — Width of the light band as a fraction of the control width., `Angle`, `PeakOpacity`, `SheenBrush`, `PauseSeconds` — Idle time between two sweeps, in seconds.

### SideMenu


Inherits `System.Windows.Controls.Control`.
- **Dependency properties (5):** `Items`, `SelectedItem`, `IsCollapsed`, `Header`, `Footer`
- **Routed events:** `SelectionChanged`
- **CLR events:** `SelectionChanged`

### SideMenuItem


Inherits `System.Windows.DependencyObject`.
- **Dependency properties (7):** `Icon`, `Title`, `Badge`, `HasBadge`, `IsSelected`, `Command`, `Children`

### SignaturePad

A place to sign: the user draws with the mouse or a pen, the strokes are kept as a , and the pad can hand the result back as a bitmap.

Inherits `System.Windows.FrameworkElement`.
- **Dependency properties (10):** `StrokeBrush` — Ink colour. Empty takes CONCOST.Brush.TextPrimary, so a signature is legible in both themes., `StrokeThickness` — Ink width in pixels. Default 2.2 — thin enough to look like a pen, thick enough at 125% DPI., `Fill` — Background of the pad. Empty takes CONCOST.Brush.Surface., `EdgeBrush` — Border colour. Empty takes CONCOST.Brush.Border., `CornerRadius` — Corner radius of the pad. Default 10., `ShowGuideline` — Draws the "sign on the line" rule and its two crosses. Default true., `Placeholder` — Hint shown while the pad is empty, for example "Ký và ghi rõ họ tên"., `PlaceholderBrush` — Colour of . Empty takes CONCOST.Brush.TextTertiary., `HasSignature` — true once there is at least one stroke. Read-only, and the property that an "Approve" button's IsEnabled wants to bind to., `SignatureImage` — The current signature as a bitmap, refreshed after every completed stroke. Bind it Mode=OneWayToSource (or read for a specific pixel size): the pad writes it, the host never sets it.
- **CLR events:** `SignatureChanged` — Raised whenever the ink changes — a stroke added, undone or cleared.
- **Properties (1):** `Strokes: StrokeCollection` — The ink drawn so far. Add or remove a stroke to drive the pad from code.
- **Methods (3):** `Undo()` — Removes the last stroke. Returns false when there was nothing to remove. · `Clear()` — Removes every stroke and clears . · `Export(double pixelWidth, double pixelHeight, Brush background)`

### Skeleton

Modern Shimmer/Skeleton loading placeholder control. Renders an animated gradient sweep across placeholder shapes during asynchronous data fetching, and smoothly transitions to actual content once loading completes.

Inherits `System.Windows.Controls.ContentControl`.
- **Dependency properties (7):** `IsLoading`, `SkeletonShape`, `CornerRadius`, `BaseBrush`, `HighlightBrush`, `Duration`, `IsWaveEnabled`
- **Methods (1):** `OnApplyTemplate()`

### SkeletonShape *(enum)*


Inherits `System.Enum`.
*No public members.*

### SnapKind *(enum)*

The kind of snap a viewport is currently showing.

Inherits `System.Enum`.

### Sparkline

Inline mini chart for trends inside tables, KPI cards and status strips. Renders straight or smoothed lines with an optional gradient area fill.

Inherits `System.Windows.FrameworkElement`.
- **Dependency properties (8):** `Values`, `Stroke`, `Fill`, `StrokeThickness`, `ShowArea`, `ShowLastDot`, `Smooth`, `LastDotBrush`

### SplashScreen

The start-up overlay: product name, version, what it is doing right now, and a progress bar.

Inherits `System.Windows.Controls.Control`.
- **Dependency properties (8):** `AppName` — Product name shown in the largest text., `Version` — Version string under the name., `Status` — Current step — "Loading families…", "Restoring workspace…"., `Progress` — Progress from 0 to 100., `ShowProgress` — Shows or hides the bar; keep the rest of the layout for a short, silent start-up., `IsIndeterminate` — Runs the bar as a marquee when there is no real progress to report., `Logo` — Optional bitmap logo above the name., `Tagline` — Second line of description, shown above the status.
- **CLR events:** `Dismissed` — Raised once, after the splash has faded out.
- **Methods (3):** `Show()` — Brings the splash back, for a reload of the document. · `Dismiss(bool animate)` · `Report(double progress, string status)`

### SplitButton


Inherits `System.Windows.Controls.Control`.
- **Dependency properties (19):** `Content`, `Icon`, `IconSize`, `IconPlacement`, `Command`, `CommandParameter`, `CommandTarget`, `DropDownContent`, `DropDownContentTemplate`, `IsDropDownOpen`, `CornerRadius`, `Placement`, `MaxDropDownHeight`, `HitMode`, `AutoCloseOnClick`, `StaysOpen`, `IsOpenOnHover`, `HoverDelay`, `DropDownButtonPosition`
- **Routed events:** `Click`, `DropDownOpened`, `DropDownClosed`
- **CLR events:** `Click`, `DropDownOpened`, `DropDownClosed`
- **Methods (1):** `OnApplyTemplate()`

### SplitButtonHitMode *(enum)*

Specifies the hit-test interaction mode for a .

Inherits `System.Enum`.

### SpotlightCard

Card surface with a spotlight that follows the cursor plus an optional gradient border revealed on hover — the web "spotlight card" pattern, ported to WPF.

Inherits `System.Windows.Controls.ContentControl`.
- **Dependency properties (7):** `CornerRadius`, `SpotlightBrush`, `SpotlightSize`, `SpotlightOpacity`, `IsSpotlightEnabled`, `BorderGradient`, `ShowHoverBorder`
- **Methods (1):** `OnApplyTemplate()`

### Spreadsheet


Inherits `System.Windows.Controls.Control`.
- **Dependency properties (17):** `ActiveSheet`, `Sheets`, `SelectionRange`, `ActiveRow`, `ActiveColumn`, `FormulaText`, `ShowToolbar`, `ShowFormulaBar`, `ShowSheetTabs`, `ShowStatusBar`, `IsReadOnly`, `RowHeaderWidth`, `ColumnHeaderHeight`, `GridLineBrush`, `HeaderBackground`, `SelectionBrush`, `SelectionBorderBrush`
- **Properties (24):** `UndoCommand: ICommand`, `RedoCommand: ICommand`, `AddSheetCommand: ICommand`, `DeleteSheetCommand: ICommand`, `ToggleBoldCommand: ICommand`, `ToggleItalicCommand: ICommand`, `ToggleUnderlineCommand: ICommand`, `SetAlignmentCommand: ICommand`, `SetFormatCommand: ICommand`, `SetCellBackgroundCommand: ICommand`, `MergeCellsCommand: ICommand`, `UnmergeCellsCommand: ICommand`, `InsertRowCommand: ICommand`, `DeleteRowCommand: ICommand`, `InsertColumnCommand: ICommand`, `DeleteColumnCommand: ICommand`, `ExportCsvCommand: ICommand`, `ImportCsvCommand: ICommand`, `CopyCommand: ICommand`, `PasteCommand: ICommand`, `CutCommand: ICommand`, `ClearSelectionCommand: ICommand`, `AutoSumCommand: ICommand`, `RecalculateCommand: ICommand`
- **Methods (6):** `OnApplyTemplate()` · `GetCellBounds(int row, int col)` · `SetCellValueAndRecalculate(int row, int col, string input)` · `Recalculate()` · `AutoFitColumn(int columnIndex)` · `AutoFitRow(int rowIndex)`

### SpreadsheetAddressHelper *(static class)*

- **Methods (5):** `ColumnIndexToName(int columnIndex)` · `ColumnNameToIndex(string columnName)` · `ToAddress(int row, int col)` · `ParseAddress(string address)` · `TranslateFormulaReferences(string formula, int rowOffset, int columnOffset)`

### SpreadsheetCell

- **CLR events:** `PropertyChanged`
- **Properties (20):** `Row: int`, `Column: int`, `RawValue: string`, `Formula: string`, `ComputedValue: object`, `Format: SpreadsheetCellFormat`, `CustomFormat: string`, `IsBold: bool`, `IsItalic: bool`, `IsUnderline: bool`, `HorizontalAlignment: HorizontalAlignment`, `Foreground: Brush`, `Background: Brush`, `BackgroundResourceKey: string`, `ForegroundResourceKey: string`, `FontSize: double`, `IsReadOnly: bool`, `HasError: bool`, `ErrorMessage: string`, `DisplayText: string`

### SpreadsheetCellFormat *(enum)*


Inherits `System.Enum`.
*No public members.*

### SpreadsheetFormulaEngine

- **Methods (3):** `RecalculateSheet(SpreadsheetSheet sheet)` · `EvaluateCellFormula(SpreadsheetSheet sheet, SpreadsheetCell cell)` · `EvaluateExpression(SpreadsheetSheet sheet, string expr)`

### SpreadsheetGridPresenter


Inherits `System.Windows.FrameworkElement`.
- **Methods (1):** `AttachOwner(Spreadsheet owner)`

### SpreadsheetRange *(struct)*


Inherits `System.ValueType`.
- **Properties (12):** `StartRow: int`, `StartCol: int`, `EndRow: int`, `EndCol: int`, `MinRow: int`, `MaxRow: int`, `MinCol: int`, `MaxCol: int`, `RowCount: int`, `ColCount: int`, `TotalCells: int`, `IsSingleCell: bool`
- **Methods (6):** `Contains(int row, int col)` · `ToAddress()` · `Parse(string rangeStr)` · `Equals(SpreadsheetRange other)` · `Equals(object obj)` · `GetHashCode()`

### SpreadsheetSheet

- **CLR events:** `PropertyChanged`
- **Properties (6):** `Name: string`, `RowCount: int`, `ColumnCount: int`, `DefaultRowHeight: double`, `DefaultColumnWidth: double`, `MergedRanges: IReadOnlyList<SpreadsheetRange>`
- **Methods (16):** `MergeRange(SpreadsheetRange range)` · `UnmergeRange(SpreadsheetRange range)` · `FindMergedRangeContaining(int row, int col)` · `IsMergedNonMaster(int row, int col)` · `GetOrCreateCell(int row, int col)` · `GetCell(int row, int col)` · `SetCell(int row, int col, SpreadsheetCell cell)` · `GetAllCells()` · `GetRowHeight(int row)` · `SetRowHeight(int row, double height)` · `GetColumnWidth(int col)` · `SetColumnWidth(int col, double width)` · `ClearCell(int row, int col)` · `ClearRange(SpreadsheetRange range)` · `ExportToCsv()` · `ImportFromCsv(string csvContent)`

### Sprite

Sprite is a floating assistant widget - the Assistive Touch / Floating Action Button pattern. It floats above application content, is freely draggable across the window, automatically snaps to screen edges, and expands an interactive quick-action menu when clicked or hovered.

Inherits `System.Windows.Controls.ContentControl`.
- **Dependency properties (5):** `IsOpen`, `MenuContent`, `AutoSnap`, `CornerRadius`, `ShowBorder`

### StackedColumn


Inherits `System.Windows.DependencyObject`.
- **Dependency properties (4):** `HeaderText`, `ChildColumns`, `Foreground`, `Background`

### StackedHeaderDataGrid


Inherits `System.Windows.Controls.DataGrid`.
- **Dependency properties (6):** `ColumnHeaderTree`, `StackedHeaderRows`, `StackedRowHeight`, `ShowFooterSummary`, `FooterRowHeight`, `EnableContextMenu`
- **Methods (3):** `OnApplyTemplate()` · `RebuildStackedHeaders()` · `RebuildFooterSummary()`

### StackedHeaderRow


Inherits `System.Windows.DependencyObject`.
- **Dependency properties (1):** `RowHeight`
- **Properties (1):** `StackedColumns: ObservableCollection<StackedColumn>`

### StaggerAnimation *(static class)*

Animates the items of an in one after another instead of all at once, so a list or a grid of cards arrives as a wave rather than a flash.
- **Dependency properties (5):** `IsEnabled`, `Delay`, `Duration`, `OffsetY`, `MaxItems`
- **Attached properties:** `Delay`, `Duration`, `IsEnabled`, `MaxItems`, `OffsetY`
- **Methods (10):** `GetIsEnabled(DependencyObject element)` · `SetIsEnabled(DependencyObject element, bool value)` · `GetDelay(DependencyObject element)` · `SetDelay(DependencyObject element, double value)` · `GetDuration(DependencyObject element)` · `SetDuration(DependencyObject element, double value)` · `GetOffsetY(DependencyObject element)` · `SetOffsetY(DependencyObject element, double value)` · `GetMaxItems(DependencyObject element)` · `SetMaxItems(DependencyObject element, int value)`

### StatusBar

The strip along the bottom of a window: left-aligned cells for state, right-aligned ones for zoom, units and counts.

Inherits `System.Windows.Controls.ItemsControl`.
- **Dependency properties (1):** `Spacing` — Space between two cells, in pixels. Default 10.

### StatusBarAlignment *(enum)*

Side of the bar a sits on.

Inherits `System.Enum`.

### StatusBarItem

One cell of a : an optional icon, a caption, and optionally a command.

Inherits `System.Windows.Controls.ContentControl`.
- **Dependency properties (6):** `Icon` — Glyph before the caption., `Text` — Caption. Ignored when is set., `IsSeparator` — Draws a vertical hairline instead of content., `Alignment` — Which side of the bar the cell hugs. Default ., `Command` — Command run when the cell is clicked., `CommandParameter` — Parameter passed to .

### StatusBarPanel

Arranges status cells from both ends of the bar: left-aligned cells stack from the left, right-aligned ones from the right, and the gap in the middle takes the slack.

Inherits `System.Windows.Controls.Panel`.
- **Dependency properties (1):** `Spacing` — Space between two cells, in pixels. Default 10.

### StatusToBackgroundConverter

- **Methods (2):** `Convert(object value, Type targetType, object parameter, CultureInfo culture)` · `ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)`

### StatusToForegroundConverter

- **Methods (2):** `Convert(object value, Type targetType, object parameter, CultureInfo culture)` · `ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)`

### StepBar

A horizontal or vertical step progress bar. Supports navigation, per-status brushes, animated progress and routed / events.

Inherits `System.Windows.Controls.ItemsControl`.
- **Dependency properties (9):** `StepIndex`, `Dock`, `IsNavigable`, `ShowProgressBar`, `StepConnectorThickness`, `CompletedBrush`, `ActiveBrush`, `WaitingBrush`, `ErrorBrush`
- **Routed events:** `StepChanged`, `StepCompleted`
- **CLR events:** `StepChanged`, `StepCompleted`
- **Methods (5):** `GoToStep(int index)` · `Next()` · `Prev()` · `OnApplyTemplate()` · `UpdateSteps(int stepIndex)`

### StepBarItem

A single step in a : shows an index, icon, header, sub-title and status.

Inherits `System.Windows.Controls.ContentControl`.
- **Dependency properties (6):** `Index`, `Status`, `Description`, `Icon`, `CanNavigate`, `SubTitle`

### StepStatus *(enum)*


Inherits `System.Enum`.
*No public members.*

### Tag


Inherits `System.Windows.Controls.ContentControl`.
- **Dependency properties (3):** `IsClosable`, `Icon`, `CornerRadius`
- **Routed events:** `Closed`
- **CLR events:** `Closed`
- **Methods (1):** `OnApplyTemplate()`

### TagChangedEventArgs

Describes a tag that a just added or removed.

Inherits `System.EventArgs`.
- **Properties (3):** `Item: object` — The value behind the tag., `Text: string` — The chip caption., `IsAdded: bool` — true when the tag was added, false when it was removed.

### TagEntry

One chip in a : the item it came from, plus the text it shows.
- **Properties (2):** `Item: object` — The value behind the chip — a string for free text, the original object for a suggestion., `Text: string` — Chip caption, resolved from (or the item's own ToString).
- **Methods (1):** `ToString()`

### TagInput

A text box that collects tags: type a word, press Enter, and it becomes a chip in the box. With a it behaves like a mention box — typing narrows a list and Enter takes the highlighted entry.

Inherits `System.Windows.Controls.Control`.
- **Dependency properties (10):** `ItemsSource` — The collection the tags come from (and, when it is an , are written back to)., `DisplayPath` — Property (or dotted path) used as a chip caption, for both the tags and the suggestions. Empty uses the item's own ToString(), which is all a string collection needs., `SuggestionSource` — Values offered while the user types. Empty turns the control into a plain tag box., `Text` — What the user has typed and not committed yet. Two-way by default., `Placeholder` — Hint shown while there is no tag and nothing typed., `AllowFreeText` — When true (the default) anything typed can become a tag. When false the user can only pick from , which is what a "choose the materials" box wants., `AllowDuplicates` — Allow the same tag twice. Default false; comparison ignores case and accents., `MaxTags` — Upper bound on the number of tags. 0 (the default) means no limit., `Delimiter` — Characters that commit the text as a tag, in addition to Enter. Default , — which is how a user pastes "bê tông, thép, cốp pha" in one go. Empty disables it., `IsSuggestionOpen` — Whether the suggestion list is showing. Two-way, so a host can close it on demand.
- **CLR events:** `TagsChanged` — Raised when a tag is added or removed, including when the user only asked for it.
- **Properties (3):** `Tags: ObservableCollection<TagEntry>` — The chips currently on screen, in order., `Suggestions: ObservableCollection<TagEntry>` — The entries the suggestion list is showing right now, after filtering., `RemoveTagCommand: ICommand` — Removes the tag a chip's close button carries. The default template binds the button to this command, which is how the button reaches the control from inside an item template.
- **Methods (5):** `CommitText()` — Turns the current into tags, splitting on . Returns the number of tags that were actually added. · `AddTag(object value)` · `RemoveTag(TagEntry entry)` · `ClearTags()` — Removes every tag. Raises once per removed tag. · `OnApplyTemplate()`

### TemporaryDimension

A temporary dimension drawn while something is being edited.
- **Properties (4):** `Start: Point3D` — First witness point, in world coordinates., `End: Point3D` — Second witness point., `Offset: double` — How far the dimension line is pushed off the two points, in pixels., `Text: string` — Caption; the measured distance is used when this is empty.

### TextReveal

Reveals a headline word by word: each word fades in and rises a few pixels, staggered. The web “text reveal” pattern, rendered directly so it costs one text layout per frame.

Inherits `CONCOST.UI.Controls.AnimatedEffectBase`.
- **Dependency properties (8):** `Text`, `FontSize`, `FontWeight`, `Foreground`, `WordDelay` — Seconds between two consecutive words., `WordDuration` — Seconds each word takes to fade and rise., `OffsetY` — Vertical distance (px) each word rises from., `StartDelay`

### TiltCard

Card that leans toward the pointer and lifts slightly — a perspective illusion built from skew + scale, so it stays a plain 2D visual (text stays crisp, no Viewport3D cost). Eases back to flat when the pointer leaves. Compose with if you also want a glow.

Inherits `System.Windows.Controls.ContentControl`.
- **Dependency properties (3):** `MaxTilt` — Maximum tilt in degrees at the edges of the card., `Lift` — Scale applied while hovered (1 = no lift)., `CornerRadius`

### TitleBarAction

One button in the title bar's quick access strip.
- **Properties (4):** `Icon: IconKind` — The glyph., `ToolTip: string` — What the button says it does., `Command: ICommand` — What the button runs; a disabled command disables the button for free., `CommandParameter: object` — Passed to the command.

### ToggleSwitch

A styled toggle switch control that extends with rich visual customisation: track size, thumb size, per-state brushes, corner radius, label placement, and an optional slide animation.

Inherits `System.Windows.Controls.Primitives.ToggleButton`.
- **Dependency properties (12):** `OnContent`, `OffContent`, `SwitchWidth`, `SwitchHeight`, `ThumbSize`, `ThumbBrush`, `OnBrush`, `OffBrush`, `CornerRadius`, `ContentPlacement`, `ShowLabel`, `AnimationDuration`

### ToolWindow

A dockable panel: a header with a title and the pin/close buttons, and the content underneath.

Inherits `System.Windows.Controls.ContentControl`.
- **Dependency properties (15):** `Header` — Title shown in the header. Plain text or any content., `Icon` — Glyph shown before the title., `ShowHeader` — Draw the header. Turn it off for a panel that is only ever docked in the centre., `HeaderHeight` — Height of the header bar, in pixels. Default 28., `IconSize` — Size of the glyph in front of the title, in pixels. Default 13., `HeaderButtons` — When the pin and close buttons appear. Default : on the active pane, plus any pane the pointer is over., `ShowActiveAccent` — Draw the accent rule along the header's top edge while this pane is active. It is the one mark that says which of four adjacent panes the keyboard is talking to., `Dock` — Edge the panel is docked to. The manager owns the layout, this is the request., `DockSize` — Width for a left/right panel, height for a top/bottom one. Ignored in the centre., `MinDockSize` — Lower bound the user cannot drag past. Default 120., `IsAutoHidden` — Collapsed to an edge tab; it slides out over the centre when opened., `IsFlyoutOpen` — An auto-hidden panel that is currently slid out., `CanClose` — Show the close button. Default true., `CanAutoHide` — Show the pin button. Default true., `IsActive` — The panel the user last interacted with — the manager highlights its header.
- **CLR events:** `AutoHideChanged` — Raised when flips. The manager listens so a panel nested inside a group can be pulled out to an edge strip, which is the one place auto-hide is implemented., `Activated` — Raised when this panel becomes the active one. The manager listens so that setting from code switches the tab exactly like a click does., `Closing` — Raised before the panel closes; set to veto., `Closed` — Raised after the panel is closed., `DragStarted` — Header drag began., `DragMoved` — Header drag moved. e is the pointer position in the manager's coordinates., `DragCompleted` — Header drag ended; e is the pointer position in the manager's coordinates.
- **Methods (3):** `Close()` — Closes the panel, honouring . · `ToggleAutoHide()` — Flips the auto-hide state. · `OnApplyTemplate()` — Wires the template parts. The handlers live in code rather than in the template because Generic.xaml is a plain resource dictionary — it has no x:Class, so XAML event handlers are not allowed there.

### Transfer


Inherits `System.Windows.Controls.Control`.
- **Dependency properties (9):** `ItemsSource`, `TransferredItems`, `SourceTitle`, `TargetTitle`, `ItemTemplate`, `ListWidth`, `ListHeight`, `ShowSearch`, `ShowSelectAll`
- **Routed events:** `Transferred`
- **CLR events:** `Transferred`
- **Properties (2):** `SourceItems: ObservableCollection<object>`, `TargetItems: ObservableCollection<object>`
- **Methods (6):** `OnApplyTemplate()` · `UpdateButtonStates()` · `TransferToTarget()` · `TransferToSource()` · `TransferAllToTarget()` · `TransferAllToSource()`

### TreeDataGrid

A that shows a hierarchy: bind and the grid flattens the tree into rows, one row per visible node.

Inherits `System.Windows.Controls.DataGrid`.
- **Dependency properties (13):** `RootItems` — The top-level nodes. Children are read through ., `ChildrenPath` — Property (or dotted path) that yields a node's children. Default Children., `ShowHierarchyColumn` — Draw the generated indent + expander column. Default true., `HierarchyHeader` — Header of the generated column., `HierarchyColumnWidth` — Width of the generated column, in pixels. Default 260., `IndentSize` — Pixels added per nesting level. Default 18., `HierarchyContentTemplate` — Template for the item inside the generated column. Empty renders the item's text., `HierarchyPath` — Property (or dotted path) whose value is shown in the generated column, the way works. Without it the cell shows of the node, which is rarely what a caller wants., `VisibleItemCount` — How many rows are currently visible (expanded branches only)., `Level`, `IndentWidth`, `HasChildren`, `IsExpanded`
- **Attached properties:** `HasChildren`, `IndentWidth`, `IsExpanded`, `Level`
- **Methods (10):** `GetLevel(DependencyObject element)` · `GetIndentWidth(DependencyObject element)` · `GetHasChildren(DependencyObject element)` · `GetIsExpanded(DependencyObject element)` · `SetIsExpanded(DependencyObject element, bool value)` · `ExpandAll()` — Expands every branch that has children. · `CollapseAll()` — Collapses every branch, leaving only the roots visible. · `ExpandTo(object item)` · `IsNodeExpanded(object item)` · `ToggleNode(object item)`

### TreeMap

A squarified treemap: area proportional to value, rectangles packed so they stay close to square.

Inherits `System.Windows.FrameworkElement`.
- **Dependency properties (12):** `ItemsSource`, `ValuePath` — Property that carries the weight of an item. Default Value., `LabelPath` — Property that carries the caption. Default Label; leave empty to use ToString()., `Palette` — Brushes cycled across the rectangles. Empty falls back to ., `Fill` — Single brush used when is empty. Empty takes the theme accent., `Gap` — Space between rectangles, in pixels. Default 3., `CornerRadius` — Corner radius of each rectangle. Default 4., `ShowLabels` — Draw the caption inside each rectangle. Default true., `ShowValues` — Draw the value under the caption. Default false., `ValueFormat` — Numeric format for . Default N0., `LabelForeground` — Caption colour. Empty uses white, which reads on every palette colour., `FontSize` — Caption size. Default 12.
- **CLR events:** `ItemInvoked` — Raised when the user clicks a rectangle.
- **Properties (1):** `Entries: IReadOnlyList<TreeMapEntry>` — The rectangles of the last layout. Reading it before the control has a size (or before the first render) lays them out on demand, so a host can hit-test without waiting for a paint.
- **Methods (2):** `HitTest(Point point)` · `Invalidate()` — Recomputes the layout without waiting for the next render pass.

### TreeMapEntry

One rectangle of a : the item it stands for and where it was drawn.
- **Properties (6):** `Item: object` — The source object., `Label: string` — Label taken from ., `Value: double` — Value taken from ., `Share: double` — Share of the total, 0–1., `Brush: Brush` — Brush actually used to paint the rectangle., `Bounds: Rect` — Where the rectangle was drawn, in device-independent pixels. Empty when it did not fit.

### TreeMapInvokedEventArgs

Carries the rectangle the user clicked.

Inherits `System.EventArgs`.
- **Properties (1):** `Entry: TreeMapEntry`

### TypewriterText

Types the text out character by character with an optional blinking caret — the classic terminal / hero-headline effect. When motion is disabled the full text is shown at once.

Inherits `CONCOST.UI.Controls.AnimatedEffectBase`.
- **Dependency properties (10):** `Text`, `CharsPerSecond` — Typing speed., `StartDelay` — Seconds to wait before the first character., `ShowCaret`, `CaretBrush`, `FontSize`, `FontWeight`, `Foreground`, `IsLooping` — Restarts from empty after the whole string is typed., `RestartDelay`
- **Properties (1):** `VisibleCharacters: int` — Characters currently visible — useful for tests and view models.

### UcsMode *(enum)*

The orientation label a shows, and the host applies to its camera.

Inherits `System.Enum`.

### UnitBox

A numeric field with the engineering unit pinned to its right edge — 3200 next to mm, 25 next to kN/m³. The unit is a label the host owns, not a parser: stays in the unit names, and converting between units is the host's model concern. The optional turns the box into a labelled form row; arrow keys and the inner spin buttons step by .

Inherits `System.Windows.Controls.Control`.
- **Dependency properties (8):** `Value`, `Unit`, `DecimalPlaces`, `Header` — Optional label above the field; the box is a plain field when it is empty., `Increment` — How far one arrow-key press or spin-button click moves the value., `Minimum`, `Maximum`, `IsReadOnly`
- **Routed events:** `ValueChanged` — Raised when the inner numeric editor commits a different value.
- **CLR events:** `ValueChanged` — Raised when the inner numeric editor commits a different value.

### ViewControlBar

The bar that lives at the bottom of a view: scale, detail level, visual style, shadows, crop, hide/isolate and reveal hidden.

Inherits `System.Windows.Controls.Grid`.
- **Dependency properties (19):** `ViewScale` — Scale text shown in the bar, e.g. 1:100. Free text: a host can add its own scales., `DetailLevel` — Coarse, medium or fine., `VisualStyle` — Wireframe, hidden line, shaded or realistic., `ShowShadows` — Whether the view casts shadows., `CropView` — Whether the view is cropped to its crop region., `RevealHidden` — Whether hidden elements are drawn in magenta for editing., `Variant` — How the bar is dressed: over a view, inside a status bar, over a drawing that must stay readable., `CornerRadius` — Corner radius of the frame. Half the height makes a pill., `Elevation` — Shadow depth of the frame, 0 to 4. Ignored by the seamless variant., `ControlHeight` — Height of every picker, toggle and button on the bar, in pixels., `IconSize` — Size of the glyph inside a toggle or a button, in pixels., `ShowDividers` — Shows the rules between the pickers, the toggles and the hide button. A rule also steps aside on its own when one of the two groups beside it is hidden., `ShowScale` — Shows the view scale picker., `ShowDetailLevel` — Shows the detail level picker., `ShowVisualStyle` — Shows the visual style picker., `ShowToggles` — Shows the shadows, crop and reveal-hidden toggles as one group., `ShowHideIsolate` — Shows the hide / isolate split button., `Scales` — The scales offered by the dropdown. null means the five a plan is normally read at (1:50 … 1:1000). An observable list re-fills the dropdown as it changes., `HideIsolateCaption` — Caption of the hide / isolate button. Set it to translate the bar.
- **CLR events:** `SettingsChanged` — Raised after any setting changes, with the property name for the argument., `HideIsolateRequested` — Raised when hide, isolate or reset is picked.
- **Methods (1):** `SyncFromProperties()` — Pushes the property values back into the controls. Called automatically after a property changes from code; exposed because a host may bind the bar lazily.

### ViewControlBarVariant *(enum)*

How a is dressed.

Inherits `System.Enum`.

### ViewCube

The navigation cube: a bevelled box with 26 clickable zones, a compass, a home button, four quarter-turn arrows, two roll buttons and a context menu, kept in step with the camera of the viewport it sits on.

Inherits `System.Windows.FrameworkElement`.
- **Dependency properties (39):** `Host` — The camera the cube mirrors. Assign the viewport's camera and the cube follows it; leave it null and the control shows on its own, which is what a designer surface or a test needs., `Rotation` — The orientation drawn when there is no . With a host the value tracks the camera, so switching the host off keeps the pose instead of jumping back to the default., `HomeRotation` — The view the home button returns to; the Top-Front-Right corner by default., `FrontAngle` — Which way the model's front faces, in degrees about the vertical axis. "Set Current View as Front" turns it, and the FRONT caption and the compass turn with it., `Theme` — Colours; null falls back to the theme's CONCOST.Brush.ViewCube* resources., `Visible` — Whether the cube is on screen at all., `Position` — Which corner of the viewport the cube docks to., `DockMargin` — Distance from the viewport's edges, in pixels. The Margin of the specification., `Size` — Overall size of the widget: the cube is fitted inside it, compass and buttons included., `InactiveOpacity` — Opacity, in percent, while the pointer is elsewhere; 100 while it is over the cube., `ShowCompass` — Draws the compass ring with N, E, S and W under the cube., `SnapToClosestView` — Snaps to the nearest of the 26 views when a drag ends close to one., `SnapAngle` — How close, in degrees, a drag has to end for to pull it in., `FitToViewOnChange` — Asks the host to frame the model after a view change., `AnimatedTransitions` — Runs a transition instead of jumping straight to the target., `AnimationDuration` — Base duration of a transition, in milliseconds; a quarter turn takes this long., `KeepSceneUpright` — Forbids roll while dragging: screen up stays the projection of world up., `ClickThreshold` — How far, in pixels, the pointer may travel before a press becomes a drag., `FaceBrush` — Custom brush for the cube faces., `EdgeBrush` — Custom brush for the bevel edge lines., `BorderBrush` — Custom brush for outer border and compass ring., `TextBrush` — Custom brush for text labels., `HighlightBrush` — Custom brush for hovered part highlight., `ArrowBrush` — Custom brush for step arrows and roll buttons., `CompassBrush` — Custom brush for compass ring., `CompassTextBrush` — Custom brush for compass direction letters., `TopLabel` — Custom label for the Top face (default: TOP)., `BottomLabel` — Custom label for the Bottom face (default: BOTTOM)., `FrontLabel` — Custom label for the Front face (default: FRONT)., `BackLabel` — Custom label for the Back face (default: BACK)., `LeftLabel` — Custom label for the Left face (default: LEFT)., `RightLabel` — Custom label for the Right face (default: RIGHT)., `AlwaysShowTools` — Whether helper tools (Home, Roll, 90-degree step arrows, Context menu) remain visible even without hovering., `DragSensitivity` — Multiplier for mouse drag orbit rotation sensitivity (default: 0.65 for smooth, controlled motion)., `ShowOrientationLabel` — Draws the current orientation's name under the cube, the way an orientation selector does., `OrientationNameOverride` — Text that replaces the computed orientation name; empty uses the computed one., `ShowFocusRing` — Draws a ring while the widget owns the keyboard focus., `AllowOrbit` — When false a press-and-drag stays a click: the cube turns only by picking zones, arrows and the compass., `ShowTooltips` — Shows the name of the zone, letter or button under the pointer as a tooltip.
- **CLR events:** `ViewRequested` — Raised before a transition runs; cancellable, and the target may be replaced., `RegionHovered` — Raised when the pointer moves onto or off a zone., `ViewTransitionCompleted` — Raised when a transition finished, or when a change was applied without animating., `HomeChanged` — Raised after "Set Current View as Home" stored a new home., `FrontChanged` — Raised after the front angle changed, by "Set Current View as Front" or "Reset Front"., `ProjectionModeChanged` — Raised when the projection is chosen from the context menu., `OptionsRequested` — Raised by the "Options…" entry, for a host that owns a settings dialog., `SaveViewRequested` — Raised by "Save Current View…"; the host collects a name and calls ., `SavedViewActivated` — Raised after or a menu pick turned to a saved view.
- **Properties (5):** `SavedViews: ObservableCollection<ViewCubeSavedView>` — The named orientations the host offers in the orientation menu and through ., `OrientationName: string` — The name of the orientation closest to the current view — TOP, NE Isometric — or the when one is set., `Projection: ProjectionMode` — The projection the user picked from the menu; the host applies it., `HoveredRegion: ViewCubeHit` — What the pointer is over, or ., `CurrentRotation: Quaternion` — The zone the cube is drawn from, whichever of the host and the local rotation has it.
- **Methods (17):** `ResetToHome()` — Animates the view back to the designated Home orientation. · `GoToFace(string faceName)` · `Invalidate()` — Redraws from the host's current rotation; call it from a host that cannot raise changes. · `GoTo(ViewCubeRegion region)` · `GoHome()` — Returns to . · `SetCurrentViewAsHome()` — Stores the current orientation as the one the home button returns to. · `SetCurrentViewAsFront()` — Declares the current heading to be the model's front. · `ResetFront()` — Puts the model's front back where the convention says it is. · `RollBy(int quarterTurns)` · `StepToNeighbor(ArrowDirection direction)` · `SetProjection(ProjectionMode mode)` · `SaveCurrentView(string name)` · `RemoveSavedView(string name)` · `GoToSavedView(string name)` · `OpenContextMenu()` — Opens the menu under the widget, or wherever the right button asked for it. · `OpenOrientationMenu()` — Opens the orientation selector: the 26 zones, then the saved views. · `HitTest(Point point)`

### ViewCubeFacelet *(struct)*

One of the 26 drawable zones: the region it stands for and its polygon on the unit cube.

Inherits `System.ValueType`.
- **Properties (3):** `Region: ViewCubeRegion` — The direction this zone stands for., `Corners: Vector3D[]` — Three or four points on the surface of the unit cube, in order round the zone., `Centre: Vector3D` — Middle of the zone, used to sort the zones back to front before drawing.

### ViewCubeHit

What was under the pointer on a .
- **Properties (10):** `Empty: ViewCubeHit` — The hit that means "nothing"., `Part: ViewCubePart` — Which widget was hit., `Label: string` — Caption of the hit: TOP, TOP-FRONT-RIGHT, N, HOME., `Region: ViewCubeRegion` — The zone that was hit; empty for the compass, the home button and the menu., `Arrow: ArrowDirection` — For an arrow hit: which of the four was pressed., `Roll: int` — For a roll hit: +1 clockwise, -1 counter-clockwise., `Compass: Vector3D` — For a compass hit: the horizontal world direction the letter points at., `Direction: Vector3D` — Direction from the model towards the eye that this hit asks for., `IsEmpty: bool` — true when the point missed everything the control draws., `IsRegion: bool` — true when the hit picked one of the 26 zones.
- **Methods (1):** `ToString()`

### ViewCubeMath *(static class)*

The maths a view cube needs, with no drawing in it: the 26 regions, the basis and quaternion conversions, the short-arc interpolation and the search for the region nearest a direction.
- **Properties (3):** `DefaultEye: Vector3D` — The direction of the default home view: the Top-Front-Right corner., `WorldUp: Vector3D` — World up. Z, as Revit, not Y as WPF's own camera., `Regions: IReadOnlyList<ViewCubeRegion>` — The 26 directions, faces first, then the twelve edges, then the eight corners.
- **Methods (21):** `Normalize(Vector3D vector)` · `RightFor(Vector3D forward, Vector3D up)` · `UprightUp(Vector3D forward, Vector3D fallback)` · `Rotate(Vector3D vector, Vector3D axis, double degrees)` · `Angle(Vector3D a, Vector3D b)` · `Elevation(Vector3D eye)` · `FromBasis(Vector3D right, Vector3D up, Vector3D forward)` · `ToBasis(Quaternion rotation, Vector3D& right, Vector3D& up, Vector3D& forward)` · `NormalizeRotation(Quaternion rotation)` · `RotationFor(ViewCubeRegion region, double frontAngle)` · `RotationFromEye(Vector3D eye, double frontAngle)` · `Slerp(Quaternion from, Quaternion to, double t)` · `AngleBetween(Quaternion from, Quaternion to)` · `SmoothStep(double t)` · `Upright(Quaternion rotation)` · `Roll(Quaternion rotation, double degrees)` · `Closest(Vector3D eye)` · `OrientationName(ViewCubeRegion region)` · `Closest(Quaternion rotation)` · `TryClosest(Vector3D eye, double toleranceDegrees, ViewCubeRegion& region)` · `TryFace(Quaternion rotation, double toleranceDegrees, ViewCubeRegion& region)`

### ViewCubeMesh *(static class)*

The bevelled solid the cube is drawn from: a square face in the middle of each side, a narrow strip between every two faces and a triangle at every corner, all on the surface of a unit cube.
- **Properties (1):** `Default: IReadOnlyList<ViewCubeFacelet>` — The solid at the default proportions. Built once; the geometry never varies.
- **Methods (1):** `Build(double faceRatio)`

### ViewCubePart *(enum)*

Which part of a a point landed on.

Inherits `System.Enum`.

### ViewCubePosition *(enum)*

The corner of the viewport the cube is docked to.

Inherits `System.Enum`.

### ViewCubeRegion *(struct)*

One of the 26 directions the cube offers: a face, an edge or a corner, with the forward and up vectors a camera on that side has to take.

Inherits `System.ValueType`.
- **Properties (9):** `Kind: ViewCubeRegionKind` — Face, edge or corner., `Name: string` — Stable identifier, e.g. TOP or TOP-FRONT-RIGHT., `Label: string` — The caption drawn on the region: a face label, or empty for an edge or a corner., `IsEmpty: bool` — true for a value that was never assigned a direction., `IsFace: bool` — true when the region is one of the six captioned squares., `Normal: Vector3D` — Unit outward normal, i.e. the direction from the model towards the eye., `Forward: Vector3D` — Direction the eye looks in when it sits on this side: -Normal., `Up: Vector3D` — Screen-up that keeps the model on its feet while looking from this side., `Right: Vector3D` — Screen-right, derived so that Right x Up = -Forward like every other basis here.
- **Methods (5):** `Rotated(double frontAngle)` · `Equals(ViewCubeRegion other)` · `Equals(object obj)` · `GetHashCode()` · `ToString()`

### ViewCubeRegionKind *(enum)*

Which of the three kinds of zone a region of the cube belongs to.

Inherits `System.Enum`.

### ViewCubeSavedView

A named orientation a host stored on a .
- **Properties (2):** `Name: string` — Label shown in the orientation menu; replacing a view under the same name overwrites it., `Rotation: Quaternion` — The orientation the camera takes when the view is activated.
- **Methods (1):** `ToString()`

### ViewCubeSize *(enum)*

Named sizes, plus which follows the viewport.

Inherits `System.Enum`.

### ViewCubeTheme

The colours of a . Every brush is optional: a null one falls back to the CONCOST.Brush.ViewCube* resource of the theme in force, which is what keeps a light shell light when the theme is swapped at run time.
- **Properties (5):** `Face: Brush` — Fill of the face, edge and corner zones., `Edge: Brush` — The bevel lines between the zones., `Border: Brush` — The outline of the silhouette., `Text: Brush` — Captions and the compass letters., `Highlight: Brush` — Fill and outline of whatever the pointer is over.

### ViewDetailLevel *(enum)*

How much detail a view draws.

Inherits `System.Enum`.

### ViewRequestedEventArgs

Raised before a view change runs; a host may cancel it or redirect it.

Inherits `System.EventArgs`.
- **Properties (3):** `Hit: ViewCubeHit` — What the user picked., `Target: Quaternion` — The orientation the control is about to turn to; a host may replace it., `Cancel: bool` — Set to true to keep the view where it is, e.g. in a locked 2D view.

### ViewportControl

The drawing surface: a grid, a crosshair with an aperture, snap markers, grips, a rubber-band selection and a temporary dimension, all on top of a .

Inherits `System.Windows.Controls.Grid`.
- **Dependency properties (29):** `ShowGrid` — Paints the grid., `GridStep` — Grid spacing in world units. It doubles automatically when it gets too dense to read., `ShowCrosshair` — Paints the CAD crosshair under the pointer., `SnapToGrid` — Pulls the pointer onto the nearest grid intersection before reporting it., `SnapDistance` — How close, in pixels, the pointer has to be to a grid intersection to snap., `SelectionRectangle` — Rubber-band rectangle in surface pixels; set it while dragging a window selection., `Snap` — The active snap marker, in surface pixels., `SnapKind` — Which marker shape is drawn at ., `TemporaryDimension` — Dimension shown while a distance is being edited., `Grips` — Grips drawn over the selected geometry., `ShowAxes` — Draws the two world axes through the model origin. They are a datum, not a warning: the default brush is CONCOST.Brush.CanvasAxis and the default style is a dash., `AxisBrush` — Overrides the axis brush. Null keeps the theme token, which follows the theme at runtime., `AxisThickness` — Stroke width of the axis lines, in pixels., `AxisDashStyle` — Dash pattern of the axis lines. Null draws the default dash; DashStyles.Solid draws a continuous line, which is what a host overlaying a plan on a survey grid usually wants., `ShowOriginMarker` — Draws a small ring at the model origin so the datum is findable when the axes are faded, or switched off. It stands on its own: turning off leaves the ring., `GridBrush` — Overrides the minor grid brush. Null keeps CONCOST.Brush.CanvasGrid., `GridMajorBrush` — Overrides the heavier every-n-th grid brush. Null keeps CONCOST.Brush.CanvasGridMajor., `GridMajorEvery` — One line in every GridMajorEvery is drawn with the heavier brush. Values below two are clamped., `MinGridPixelSpacing` — Smallest gap, in pixels, the grid may have between two lines. Below it the step doubles until the lines are readable again, which is what keeps a zoomed-out plan from becoming a grey wash., `CrosshairBrush` — Overrides the crosshair brush. Null keeps CONCOST.Brush.TextSecondary., `ShowAperture` — Draws the small box at the pointer that marks "here" when no snap has been taken., `SnapMarkerBrush` — Overrides the snap marker brush. Null keeps the accent colour., `SnapMarkerSize` — Half-size of the snap marker glyph, in pixels., `ShowSnapMarker` — Whether the snap glyph is painted at the snapped point. The caption is a separate switch (), so a quiet surface can drop the shape and keep the word., `ShowSnapLabel` — Draws the snap kind (ENDPOINT, MIDPOINT, GRID ...) beside the marker., `SelectionBrush` — Overrides the rubber-band brush. Null keeps the accent colour., `SelectionFillOpacity` — Fill opacity of the rubber band, 0 to 1. Zero draws the outline only., `DimensionBrush` — Overrides the temporary-dimension brush. Null keeps the accent colour., `FitMargin` — How much empty space leaves around the bounds; 1.1 is a tenth. One means flush.
- **CLR events:** `CursorMoved` — Raised as the pointer moves, with the world point under it., `WorldClicked` — Raised when the left button is released after a click that was not a pan.
- **Properties (2):** `Camera: CameraController` — The camera that maps world to surface. Replacing it rewires the change notification; the default is an orthographic plan., `Overlay: Canvas` — The layer above the drawing; hosts put a compass, a mini map or a coach mark here.
- **Methods (5):** `ToScreen(Point3D world)` · `ToWorld(Point screen)` · `ZoomToFit(Rect3D bounds)` · `ToggleGrid()` — Turns the grid on or off; the toggle the status bar drives. · `UnitsToPixels(double units)`

### ViewportCursorEventArgs

Carries the world point under the pointer, plus whatever snap is active.

Inherits `System.EventArgs`.
- **Properties (3):** `World: Point3D` — Point on the view plane, in world coordinates., `Screen: Point` — The same point in surface pixels., `IsSnapped: bool` — true when the point was pulled onto a grid intersection.

### ViewportGrip

A grip drawn on a viewport: the little square that starts an edit.
- **Properties (2):** `Position: Point3D` — Where the grip sits, in world coordinates., `IsHot: bool` — Hot grips are filled with the accent colour.

### VisualStyleMode *(enum)*

The drawing style of a 3D view.

Inherits `System.Enum`.

### WaitLoading


Inherits `CONCOST.UI.Controls.LoadingBase`.
*No public members.*

### Watermark


Inherits `System.Windows.Controls.ContentControl`.
- **Dependency properties (6):** `Text`, `Angle`, `Mode`, `MarkOpacity`, `MarkBrush`, `MarkFontSize`
- **Methods (1):** `OnApplyTemplate()`

### WatermarkMode *(enum)*


Inherits `System.Enum`.
*No public members.*

### WaveDivider

Animated wave band used as a section divider or as the bottom of a header block. Two sine layers drift at different speeds to give a soft parallax feel.

Inherits `System.Windows.FrameworkElement`.
- **Dependency properties (7):** `Amplitude`, `Wavelength`, `Speed` — Horizontal drift in pixels per second., `FillBrush`, `BackBrush`, `ShowBackWave`, `IsAnimated`

### Wizard

A multi-step form: a header that shows where you are, one step's content at a time, and back / next / finish buttons that only advance when the current step says it is valid.

Inherits `System.Windows.Controls.Control`.
- **Dependency properties (10):** `CurrentIndex` — Zero-based index of the step on screen. Two-way, so a view model can drive the wizard., `CurrentStep` — The step on screen, or null when the wizard has no steps., `CanGoBack` — true when there is a step before the current one., `CanGoNext` — true when there is a step after the current one and the current one is valid., `IsLastStep` — true on the final step, where the button says Finish., `ShowHeader` — Draw the step header. Turn it off when the wizard sits inside a dialog that already titles the work., `ShowFooter` — Draw the back / next row. Turn it off to drive the wizard from your own buttons., `BackText` — Caption of the back button. Default "Back"., `NextText` — Caption of the next button. Default "Next"., `FinishText` — Caption of the next button on the last step. Default "Finish".
- **CLR events:** `Navigating` — Raised before a step change; set to stop it., `StepChanged` — Raised after the step changed., `Finished` — Raised when the user clicks Finish on the last step.
- **Properties (1):** `Steps: ObservableCollection<WizardStep>` — The steps, in order. Adding or removing one renumbers the rest.
- **Methods (6):** `Next()` — Moves forward one step. Returns false when that was not allowed. · `Back()` — Moves back one step. Returns false when already on the first step. · `GoTo(int index)` · `Finish()` — Raises — what the Finish button calls. · `RefreshState()` — Recomputes the button states; call it after flipping a step's validity by hand. · `OnApplyTemplate()`

### WizardNavigatingEventArgs

Describes a step change that is about to happen; stops it.

Inherits `System.EventArgs`.
- **Properties (3):** `FromIndex: int` — The step the user is leaving, or -1 before the first one., `ToIndex: int` — The step the user asked for., `Cancel: bool` — Set to true to stay where you are — the hook for "fix the errors first".

### WizardStep

One page of a : a header, a description and the content the user fills in.

Inherits `System.Windows.Controls.HeaderedContentControl`.
- **Dependency properties (6):** `Description` — Second line under the header — what the step is for., `IsValid` — Whether the step's input is acceptable. false disables the Next button, which is what makes "Next" mean "this page is done" instead of "I clicked a button"., `Index` — Zero-based position in the wizard. Set by the wizard, not by hand., `Number` — as the user counts it, starting at 1 — what the header's dot shows., `IsCurrent` — true for the step on screen., `IsCompleted` — true for a step the user has already left — the header's tick.

## CONCOST.UI.Controls.Canvas

### RulerControl


Inherits `System.Windows.Controls.Control`.
- **Dependency properties (4):** `Orientation`, `UnitScale`, `MajorStep`, `OriginOffset`

### SnowCanvas

SnowCanvas creates a high-performance, GPU-accelerated falling snow particle effect. Supports customizable particle count, colors, speed, wind drift, and pause/resume animation.

Inherits `System.Windows.Controls.Canvas`.
- **Dependency properties (8):** `SnowflakeCount`, `SnowflakeBrush`, `MinSnowflakeSize`, `MaxSnowflakeSize`, `MinSpeed`, `MaxSpeed`, `Wind`, `IsActive`

### Snowflake

A single snowflake particle with position, velocity, oscillation phase, and size.
- **Properties (8):** `X: double`, `Y: double`, `Radius: double`, `SpeedY: double`, `SwingSpeed: double`, `SwingAngle: double`, `SwingRadius: double`, `Opacity: double`

### ZoomableCanvas


Inherits `System.Windows.Controls.Canvas`.
- **Dependency properties (7):** `Zoom`, `MinZoom`, `MaxZoom`, `ShowGrid`, `GridSize`, `PanOffsetX`, `PanOffsetY`
- **CLR events:** `ViewportChanged`
- **Methods (2):** `ZoomToFit(Rect bounds, Size viewSize)` · `ResetView()`

## CONCOST.UI.Controls.Charts

### ChartBar


Inherits `CONCOST.UI.Controls.Charts.ChartRect`.
- **Dependency properties (2):** `BarBrush`, `MultiColor`

### ChartBase

Base class for all charts with 60 FPS smooth rendering

Inherits `System.Windows.Controls.Control`.
- **Dependency properties (4):** `Datas`, `Title`, `Unit`, `ShowLegend`

### ChartLine


Inherits `CONCOST.UI.Controls.Charts.ChartRect`.
- **Dependency properties (3):** `LineBrush`, `ShowArea`, `Smooth`

### ChartPie


Inherits `CONCOST.UI.Controls.Charts.ChartBase`.
*No public members.*

### ChartRadar


Inherits `CONCOST.UI.Controls.Charts.ChartBase`.
- **Dependency properties (2):** `RadarBrush`, `GridLevels`

### ChartRect *(abstract)*


Inherits `CONCOST.UI.Controls.Charts.ChartBase`.
*No public members.*

### PieControl


Inherits `CONCOST.UI.Controls.Charts.ChartBase`.
- **Dependency properties (3):** `Segments`, `ArcThickness`, `CenterText`

### PieSegmentModel


Inherits `System.Windows.DependencyObject`.
- **Dependency properties (3):** `Name`, `Value`, `Color`

## CONCOST.UI.Controls.Dashboard

### CardCarousel


Inherits `System.Windows.Controls.ItemsControl`.
- **Dependency properties (7):** `SelectedIndex`, `ItemWidth`, `ItemHeight`, `Spacing`, `MinScale`, `AutoRun`, `Interval`
- **CLR events:** `SelectionChanged`
- **Methods (4):** `OnApplyTemplate()` · `MovePrev()` · `MoveNext()` · `UpdateLayoutPositions(bool animate)`

### CardCarouselItem


Inherits `System.Windows.Controls.ContentControl`.
- **Dependency properties (3):** `Scale`, `OffsetX`, `IsActive`

### StatisticCard


Inherits `System.Windows.Controls.Control`.
- **Dependency properties (8):** `Title`, `Value`, `Unit`, `Trend`, `TrendDirection`, `Icon`, `Progress`, `Subtitle`

### Timeline


Inherits `System.Windows.Controls.ItemsControl`.
*No public members.*

### TimelineItem


Inherits `System.Windows.Controls.HeaderedContentControl`.
- **Dependency properties (6):** `Time`, `Description`, `Icon`, `Type`, `DotBrush`, `IsLast`

### TimelineItemType *(enum)*


Inherits `System.Enum`.
*No public members.*

### TrendDirection *(enum)*


Inherits `System.Enum`.
*No public members.*

## CONCOST.UI.Controls.Engineering

### AreaBox


Inherits `CONCOST.UI.Controls.UnitBox`.
*No public members.*

### ForceBox


Inherits `CONCOST.UI.Controls.UnitBox`.
*No public members.*

### LengthBox


Inherits `CONCOST.UI.Controls.UnitBox`.
*No public members.*

### MomentBox


Inherits `CONCOST.UI.Controls.UnitBox`.
*No public members.*

### PressureBox


Inherits `CONCOST.UI.Controls.UnitBox`.
*No public members.*

### RebarInfo

- **Properties (7):** `Quantity: int`, `Diameter: int`, `Spacing: double`, `IsStirrup: bool`, `RawText: string`, `IsValid: bool`, `TotalAreaMm2: double`

### RebarInput


Inherits `System.Windows.Controls.Control`.
- **Dependency properties (3):** `Text`, `IsValid`, `AreaMm2`
- **Methods (1):** `OnApplyTemplate()`

### RebarParser *(static class)*

- **Methods (1):** `Parse(string input)`

### VolumeBox


Inherits `CONCOST.UI.Controls.UnitBox`.
*No public members.*

## CONCOST.UI.Controls.Layout

### AdaptivePanel


Inherits `System.Windows.Controls.ContentControl`.
- **Dependency properties (1):** `CurrentLayout`
- **Properties (1):** `Breakpoints: List<Breakpoint>`

### Breakpoint

- **Properties (2):** `MinWidth: double`, `Layout: LayoutMode`

### LayoutMode *(enum)*


Inherits `System.Enum`.
*No public members.*

## CONCOST.UI.Controls.Navigation

### NavigationItem


Inherits `System.Windows.Controls.HeaderedContentControl`.
- **Dependency properties (3):** `Icon`, `IsSelected`, `TagName`

### NavigationView


Inherits `System.Windows.Controls.HeaderedItemsControl`.
- **Dependency properties (1):** `SelectedItem`

## CONCOST.UI.Controls.Ribbon

### BackstageItem

One entry in the backstage rail.
- **Properties (5):** `Header: string` — The caption on the rail., `Icon: IconKind` — The glyph in front of the caption., `Content: object` — The page shown when the entry is selected; null shows the header alone., `IsSeparator: bool` — Draws a rule above the entry, which is how Info and Options are set apart., `Tag: object` — Anything the host hangs off the entry.
- **Methods (1):** `ToString()`

### KeyTipService *(static class)*

Attached properties that give the ribbon keyboard access: press Alt, a small letter badge appears over every tab and command, type the letters and the command runs. <ribbon:RibbonButton Header="Bold" KeyTip="B" /> <!-- explicit badge --> <ribbon:RibbonButton Header="Italic" /> <!-- badge derived from "I" --> Leave empty and the ribbon picks a unique letter from the header, so a ribbon written without badges is still fully reachable from the keyboard.
- **Dependency properties (3):** `KeyTip`, `IsEnabled`, `IsShowing`
- **Attached properties:** `IsEnabled`, `IsShowing`, `KeyTip`
- **Methods (7):** `GetKeyTip(DependencyObject element)` · `SetKeyTip(DependencyObject element, string value)` · `GetIsEnabled(DependencyObject element)` · `SetIsEnabled(DependencyObject element, bool value)` · `GetIsShowing(DependencyObject element)` · `Show(Ribbon ribbon)` · `Hide(Ribbon ribbon)`

### Ribbon

Container for a tabbed ribbon: a strip of headers on top and the groups of the selected tab underneath. The tab strip is a real in the template, so keyboard navigation, focus and selection visuals come for free.

Inherits `System.Windows.Controls.Control`.
- **Dependency properties (29):** `ItemsSource` — The instances shown by this ribbon. Entries may also be containers, which are expanded in place., `SelectedIndex` — Index of the active tab, counted over the visible tabs only., `SelectedItem` — The active tab, kept in sync with ., `IsMinimized` — Collapses the content area so only the tab strip remains — the classic “show tabs only” ribbon state. Kept as a two-way alias of so existing markup and view models keep working: true means "at least tabs only" and clears the two deeper states., `ApplicationMenu` — Optional shown as the coloured File block at the top-left, before the quick-access strip., `ShowQuickAccessToolBar` — Shows the quick-access strip above the tabs. On by default, as in Office., `QuickAccessPlacement` — Where the quick-access strip lives: inline at the end of the tab row (default), on its own strip above the tabs, or on its own strip under the ribbon body. The strip is one control, so switching places moves it — the pinned commands and the customise menu come along., `MenuBar` — Optional shown inside the ribbon chrome. Placement is controlled by ., `MenuBarPlacement` — Where the menu bar lives: above the tab strip (default) or below the ribbon body. When the quick-access toolbar is also above the ribbon, the menu bar sits between the two., `ShowMenuBar` — Shows the menu bar. On by default., `ShowApplicationMenu` — Shows the File block at the start of the top row. On by default., `ShowTabStrip` — Shows the tab strip. On by default — turn it off for a menu-bar-only shell., `SelectedTabId` — The of the active tab. Bind this instead of when tabs can be reordered or added at runtime — an index would silently point at a different tab., `IsGroupCollapseEnabled` — When false the ribbon may still demote Large to Medium to Small, but it never merges a group into a single drop-down button. Use it when a group must stay readable at any width., `IsKeyTipEnabled` — The Alt key-tip gesture. Turn it off when the host owns the keyboard (Revit and AutoCAD both intercept Alt) and drives itself., `ShowScreenTips` — Shows the built-in screen tips on every ribbon item. Turning it off is the quiet-shell switch: the item keeps its caption and its key tip, only the hover tip goes. The value is inherited, so one setting on the ribbon reaches every item inside it; a tip a host assigned by hand is theirs and stays., `IsQuickAccessCustomizable` — Lets the user open the quick-access customise panel. Pinned commands stay usable when false; only the chevron and the panel go away — the usual locked-down build., `ShowBorder` — Draws the one-pixel outline around the ribbon. Off for a ribbon docked into a panel., `EmptyContent` — Shown in place of the tab content when there is no tab to show., `DisplayMode` — How much of the ribbon is on screen. The four states are the ones the minimise button cycles through; in the three minimised states a tab click opens the ribbon as an overlay., `IsOverlayOpen` — True while the minimised ribbon is showing its commands in the overlay., `InfoCenter` — Content for the top-right corner: a search box, a sign-in chip, a help button., `ShowInfoCenter` — Shows the info centre. On by default; an empty one occupies no room anyway., `ShowMinimizeButton` — Shows the chevron at the end of the tab strip that folds the ribbon away., `ShowItemLabels`, `IsTabStripOverflowing` — True when the tabs no longer fit and the strip can scroll. The template uses it to show the “»” button that lists every tab., `IsEmpty` — True when there is no tab to show — the state., `ShowGroupLabels`, `VisibleTabs` — Tabs currently offered to the user, in strip order.
- **Attached properties:** `ShowGroupLabels`, `ShowItemLabels`
- **CLR events:** `DisplayModeChanged` — Raised after settled on a new value.
- **Properties (4):** `Tabs: IReadOnlyList<RibbonTabItem>` — Tabs in declaration order, contextual groups expanded, plus any contextual tab the host pushed in at runtime through ., `AllItems: IReadOnlyList<RibbonItem>` — Every command in every tab — the pool the quick-access toolbar customises from., `QuickAccess: RibbonQuickAccessToolBar` — The toolbar created by the template. Null until the template has been applied., `IsShowingKeyTips: bool` — True while the Alt key tips are on screen.
- **Methods (18):** `GetShowItemLabels(DependencyObject obj)` · `SetShowItemLabels(DependencyObject obj, bool value)` · `GetShowGroupLabels(DependencyObject obj)` · `SetShowGroupLabels(DependencyObject obj, bool value)` · `CycleState()` — Steps to the next display mode: full → tabs → titles → buttons → full. · `NextMode(RibbonDisplayMode mode)` · `ShowContextualTab(RibbonTabItem tab, bool activate)` · `HideContextualTab(string id)` · `ShowKeyTips()` — Shows the key tips as if Alt had been pressed. · `HideKeyTips()` — Dismisses the key tips without running anything. · `ProcessKeyTipKey(Key key)` · `PinToQuickAccess(RibbonItem item)` · `UnpinFromQuickAccess(RibbonItem item)` · `IsPinnedToQuickAccess(RibbonItem item)` · `CaptureState()` — Snapshot of what a user expects to find again next time: the open tab, the minimised state, the pinned commands and the groups they folded away by hand. · `ApplyState(RibbonState state)` · `RefreshTabs()` — Recomputes after a contextual group or a tab changed availability. · `OnApplyTemplate()`

### RibbonApplicationMenu

The File button at the top-left of a ribbon: a coloured block that opens a two-column panel — commands on the left, recently used documents on the right.

Inherits `CONCOST.UI.Controls.Ribbon.RibbonDropDownButton`.
- **Dependency properties (3):** `RecentItems` — Right column: usually recent documents. Items can be any content., `RecentHeader` — Caption above the right column., `FooterItems` — Commands pinned to the bottom of the left column, e.g. Exit and Options.

### RibbonAutomationPeer

UI Automation for the ribbon shell. It reports itself as a toolbar so a screen reader announces “toolbar” instead of a bare pane, and it exposes the tab strip as the child list.

Inherits `System.Windows.Automation.Peers.FrameworkElementAutomationPeer`.

### RibbonBackstageView

The full-window backstage behind the File button: a rail of pages on the left, the selected page on the right, and a Back button that returns to the drawing.

Inherits `System.Windows.Controls.Grid`.
- **Dependency properties (5):** `IsOpen` — Whether the backstage covers the window., `ApplicationName` — The product name at the top of the rail., `SelectedItem` — The entry whose page is showing. Two-way bindable, so a shell can restore the page a session closed on., `RailWidth` — Width of the rail; the page takes the rest., `BackText` — Caption of the back button; a host localises it like the rest of its shell.
- **CLR events:** `IsOpenChanged` — Raised when the open state changes., `SelectedItemChanged` — Raised when a different page is selected.
- **Properties (1):** `Items: ObservableCollection<BackstageItem>` — The rail entries.
- **Methods (4):** `Open()` — Opens the backstage on the first page. · `Close()` — Returns to the drawing. · `Select(string header)` · `RebuildRail()` — Rebuilds the rail after items were added or removed.

### RibbonButton

A ribbon command button. Invoking it (mouse, Space or Enter) executes ; the visual state follows .

Inherits `CONCOST.UI.Controls.Ribbon.RibbonItem`.
- **Dependency properties (1):** `IsPressed` — True while the mouse is held down on the button.
- **CLR events:** `Click` — Raised after the command ran (or when there is no command).
- **Methods (1):** `PerformClick()` — Runs the button as if it had been clicked — the entry point used by key tips, the collapsed-group menu and the accessibility peer.

### RibbonButtonAutomationPeer

Automation peer for a ribbon command: exposes Invoke, the label and the enabled state.

Inherits `System.Windows.Automation.Peers.FrameworkElementAutomationPeer`.
- **Methods (2):** `GetPattern(PatternInterface patternInterface)` · `Invoke()` — Runs the command — what a screen reader does when it “clicks” the button.

### RibbonCheckBox

A ribbon-sized check box: a square tick glyph followed by the label, exactly one row tall. It derives from on purpose — the checked state, the command plumbing and the optional grouping are the same, only the look differs. Use it for options that stay on (snap, ortho, grid), and for commands that look like buttons.

Inherits `CONCOST.UI.Controls.Ribbon.RibbonToggleButton`.

### RibbonColorPicker

A ribbon-sized colour button: a swatch plus a label, and a pop-up holding the full . The item only shows the current colour — the hue/saturation pad, the alpha slider and the presets all come from the picker inside the pop-up, so a ribbon colour and a standalone colour picker behave identically.

Inherits `CONCOST.UI.Controls.Ribbon.RibbonItem`.
- **Dependency properties (4):** `SelectedColor` — Colour shown in the swatch and edited in the pop-up., `IsDropDownOpen` — True while the picker pop-up is showing., `ShowAlpha` — Shows the opacity slider in the pop-up., `SwatchBrush` — Frozen brush matching .
- **CLR events:** `SelectedColorChanged` — Raised whenever the user picks a different colour.
- **Methods (1):** `PerformClick()` — Opens the picker.

### RibbonComboBox

A ribbon-sized drop-down list, e.g. Font [ Calibri ▾ ]. A thin shell around a normal : items, templates, keyboard navigation and the drop-down popup all come from the base control. The label comes from ; leave it empty for a bare box.

Inherits `CONCOST.UI.Controls.Ribbon.RibbonItem`.
- **Dependency properties (7):** `ItemsSource` — Choices offered by the drop-down., `SelectedItem` — Currently selected entry, or the typed text when is set., `SelectedIndex` — Index of the selected entry, -1 when nothing is selected., `DisplayMemberPath` — Name of the property shown for each entry, when the entries are objects., `ItemTemplate` — Template used to render each entry in the list and in the closed box., `IsEditable` — Lets the user type a value that is not in the list., `MaxDropDownHeight` — Height cap of the drop-down, before it starts scrolling.
- **Methods (1):** `PerformClick()` — Focuses the box and drops the list open.

### RibbonComboBoxAutomationPeer

Exposes the ribbon drop-down as a combo box that can be expanded and collapsed.

Inherits `System.Windows.Automation.Peers.FrameworkElementAutomationPeer`.
- **Properties (3):** `ExpandCollapseState: ExpandCollapseState`, `IsReadOnly: bool`, `Value: string`
- **Methods (4):** `GetPattern(PatternInterface patternInterface)` · `Expand()` · `Collapse()` · `SetValue(string value)`

### RibbonContextualTabGroup

A set of tabs that only belong to one mode — “Drawing Tools” while a sketch is open, “Table Tools” while the caret is inside a table.

Inherits `System.Windows.Controls.HeaderedItemsControl`.
- **Dependency properties (1):** `Accent` — Band colour. Defaults to the primary accent when left null.
- **CLR events:** `TabsChanged` — Raised when the set of visible tabs may have changed.
- **Properties (2):** `IsGroupVisible: bool` — Set to to hide every tab in the group — the way a contextual group disappears when its mode ends., `Tabs: IReadOnlyList<RibbonTabItem>` — Tabs of this group, in declaration order.

### RibbonDisplayMode *(enum)*

How much of the ribbon is on screen. The order is the order the minimise button cycles through: every step removes one row of chrome, and the last step folds each panel into the single button that reopens it.

Inherits `System.Enum`.

### RibbonDropDownButton

A ribbon button that only opens a menu — it has no primary action of its own. The popup closes on Escape, on a click outside, or as soon as a menu item is invoked.

Inherits `CONCOST.UI.Controls.Ribbon.RibbonItem`.
- **Dependency properties (1):** `IsOpen` — Whether the dropdown is showing.
- **Properties (1):** `Items: ObservableCollection<object>` — Menu content: and .
- **Methods (1):** `PerformClick()` — Opens the menu, which is this control's only action.

### RibbonGallery

A grid of pictures with a selection, the ribbon's answer to a style / colour palette. The tiles visible on the ribbon are the first rows that fit the group cell; the chevron opens the same list in a larger pop-up so nothing is hidden. Both views share one , so picking a tile in either place updates the other. Items are instances, or any object when you supply . The gallery is usually and = 2 so the tiles have room.

Inherits `CONCOST.UI.Controls.Ribbon.RibbonItem`.
- **Dependency properties (10):** `ItemsSource` — Tiles offered by the gallery., `ItemTemplate` — Template for one tile on the ribbon grid. Defaults to the picture alone., `ExpandedItemTemplate` — Template for one tile inside the expanded panel. Falls back to when left null. The theme uses it to put the caption back: the grid shows bare swatches, where a label would be trimmed away, and the panel has room for the name under each tile., `SelectedItem` — Currently selected tile., `IsDropDownOpen` — True while the expanded panel is showing., `ShowExpandButton` — Shows the chevron that opens the full panel. Turn it off for a fixed grid., `ItemWidth` — Width of one tile., `ItemHeight` — Height of one tile., `MaxDropDownHeight` — Height cap of the expanded panel, before it starts scrolling., `MaxDropDownWidth` — Width cap of the expanded panel; tiles wrap inside it.
- **CLR events:** `SelectionChanged` — Raised when changes, by click or by keyboard.
- **Methods (1):** `PerformClick()` — Opens the expanded panel — the gallery has no single command to run.

### RibbonGalleryAutomationPeer

Exposes the gallery's expanded panel so it can be opened from a screen reader.

Inherits `System.Windows.Automation.Peers.FrameworkElementAutomationPeer`.
- **Properties (1):** `ExpandCollapseState: ExpandCollapseState`
- **Methods (3):** `GetPattern(PatternInterface patternInterface)` · `Expand()` · `Collapse()`

### RibbonGalleryItem

One tile of a : a picture plus an optional caption. It is deliberately a plain data object rather than a control — the gallery owns the tile look (size, hover, selection), so the same item can be shown both inline and in the expanded panel.
- **Properties (4):** `Header: string` — Caption under the picture; also used as the tooltip and menu label., `Icon: ImageSource` — Picture shown in the tile., `ToolTip: string` — Text shown on hover. Falls back to ., `Tag: object` — Anything the application wants to carry along, e.g. a style id.
- **Methods (1):** `ToString()`

### RibbonGroup

A labelled cluster of commands inside a . Items are arranged by ; the optional dialog launcher sits in the bottom-right corner, as in Office.

Inherits `System.Windows.Controls.HeaderedItemsControl`.
- **Dependency properties (14):** `DialogLauncherCommand` — Command behind the small dialog-launcher arrow in the corner., `CollapsePriority` — Lower values collapse into a popup earlier when the ribbon runs out of width. Zero means the group never collapses., `IsCollapsed` — True when the group is currently reduced to a single dropdown button., `SizeMode` — Density currently in force. The owning lowers it when the ribbon runs out of width; setting it by hand pins the group to that density., `IsDropDownOpen` — Whether the collapsed group's dropdown is showing., `IsUserCollapsed` — Set when the user collapses the group from the arrow next to its header. The ribbon never re-expands a group collapsed this way; it only restores the ones it collapsed itself., `SlideOut` — Extra content the panel keeps behind the arrow next to its title — the “slideout” of a ribbon panel. Leave it null and the arrow is not drawn at all., `SlideOutTemplate` — How is drawn; optional, the default presenter is fine., `IsSlideOutOpen` — Whether the slideout is showing. It closes when the pointer clicks elsewhere., `IsTitleOnly` — Panel body hidden but title kept — the “minimise to panel titles” ribbon state. Set by the owning ; a group never decides this for itself., `IsButtonOnly` — Group drawn as its single collapsed button — the “minimise to panel buttons” ribbon state. Kept apart from so the collapse ladder and the display mode never undo each other's work., `IsLastInStrip` — Set by on the right-most group. The template drops that group's divider, otherwise the line hangs in the empty space after the last command., `KeyTip` — Letters that open this group's launcher while the Alt key tips are showing. Only used when is set; empty means “derive one from the header”., `HasSlideOut` — True when there is a slideout to open.
- **Properties (1):** `CollapsedMenu: RibbonMenu` — Menu shown when the group is collapsed. It mirrors the group's items, because a collapsed group has to present the same commands as menu rows rather than as ribbon buttons.
- **Methods (1):** `OnApplyTemplate()`

### RibbonGroupAutomationPeer

Gives a group a name and a Group control type, so its commands are announced in context.

Inherits `System.Windows.Automation.Peers.FrameworkElementAutomationPeer`.

### RibbonGroupLauncher

The small arrow in the bottom-right corner of a that opens the dialog behind the group — “Font settings”, “Paragraph settings” and so on. It is a , so and behave exactly as they do everywhere else. The group's template binds it to and hides it when that command is missing, because an arrow that does nothing is worse than no arrow. When the group collapses into a popup the arrow is gone, so the same command is offered as the last row of .

Inherits `System.Windows.Controls.Button`.

### RibbonGroupPanel

Lays a out on the classic three-row grid. A item owns a whole column (3 rows), a Medium item takes two rows and a Small item one. Items are packed into the current column until it is full, then a new column starts — the same rule Office uses, so a group never leaves a hole in the middle. Items that need more room than one column (text boxes, combo boxes, galleries) set ; the panel then looks for a band that is free across every column the item wants before placing it. Both passes call the same pure routine, so measuring never depends on the result of a previous measure and the panel cannot get stuck in a layout loop.

Inherits `System.Windows.Controls.Panel`.
- **Dependency properties (4):** `ColumnWidth` — Width of one column of items., `RowHeight` — Height of a single row (a Small item)., `RowGap` — Vertical gap between two rows., `SeparatorWidth` — Width reserved for a .
- **Properties (2):** `GroupHeight: double` — Total height of the three rows., `MediumHeight: double` — Height of a Medium item (two rows).

### RibbonGroupSizeMode *(enum)*

How tightly a packs its items. The ribbon lowers this automatically when it runs out of width: Full → Medium → Small.

Inherits `System.Enum`.

### RibbonGroupStrip

Lays the groups of one tab out left to right and decides how much to shrink them. The ladder is the one Office uses: if the natural width does not fit, demote every Large item to Medium, then every item to Small, and only then collapse groups (lowest first). The panel escalates one step per layout pass. Re-measuring a child inside the same pass does not work: its constraint is unchanged, so WPF reuses the cached DesiredSize and the ladder would read the widths of the previous density. Asking for another pass costs nothing visible and settles in at most two steps plus one per collapsed group.

Inherits `System.Windows.Controls.Panel`.

### RibbonItem *(abstract)*

Base class for everything that can sit inside a . It owns the command plumbing: follows 's CanExecute instead of being set by hand, so a ribbon never shows a live button for a command that cannot run.

Inherits `System.Windows.Controls.HeaderedContentControl`.
- **Dependency properties (20):** `Command` — Command executed when the item is invoked., `CommandParameter` — Value passed to ., `CommandTarget` — Element the command routes to when it is a ., `Icon` — Icon used for every size unless a specific one is provided., `LargeIcon` — Icon used when is ., `SmallIcon` — Icon used when is Medium or Small., `Size` — How much room the item asks for inside its group., `ColumnSpan` — Number of group columns the item occupies, 1 by default. Text boxes, combo boxes and galleries use it to get a usable width without breaking the group grid., `IsFixedSize` — Keeps even when the ribbon compacts its groups. A gallery sets this: squeezing it into a single row would cut its tiles in half instead of saving any width., `KeyTip` — Letters that activate this command while the Alt key tips are showing, e.g. "B" for Bold or "FR" for a two-step badge. Leave it empty to have one derived from the header., `ScreenTip` — Longer explanation shown in the screen tip under the label., `GestureText` — Shortcut shown right-aligned in the screen tip, e.g. Ctrl+S. Display only — the ribbon never registers global hotkeys, so it cannot fight with the host application's own accelerator table., `Id` — Stable identifier used by quick-access and state persistence., `CollapsePriority` — Lower values are demoted earlier. Zero refuses demotion altogether, which is how a host pins an icon to the large size., `ExtendedScreenTip` — The long text the screen tip grows into after milliseconds. Leave it empty for a tip that stays short., `ToolTipPreview` — Optional illustration shown by the expanded screen tip., `ToolTipFooter` — Bottom line of the expanded screen tip, usually “Press F1 for more help”., `IsLabelVisible` — False once the ribbon has hidden small-button captions to save width., `EffectiveIconSource` — Bindable form of for the control template., `EffectiveSize` — Size actually used on screen. It equals unless the owning group has been compacted by the ribbon, in which case it is capped — templates must switch on this, not on .
- **Properties (1):** `EffectiveIcon: ImageSource` — Icon that matches the current .
- **Methods (2):** `CreateQuickAccessClone()` — Creates the small proxy button the quick-access toolbar shows. A control can only have one visual parent, so the toolbar cannot host the original item — it hosts a clone that shares the same command, icon and tooltip. · `PerformClick()` — Activates the item as a click would. Keyboard key tips and the command search call this, so a control that is not a plain button (a gallery, an input) can do the right thing — open its panel, focus its editor — instead of pretending to be a button.

### RibbonItemSize *(enum)*

How much room an item takes inside a .

Inherits `System.Enum`.

### RibbonKeyTip

The little letter badge that Alt draws over a tab, a group or a command. Badges are created and positioned by the ribbon itself — you never place one by hand. The class is public so the look can be retargeted in a theme, and so the text can be read back in tests.

Inherits `System.Windows.Controls.ContentControl`.
- **Dependency properties (1):** `Target` — The element this badge points at, or null once the badge is detached.

### RibbonLabel

A line of static text inside a group — the caption a ribbon puts next to a cluster of options (“Text height”, “Scale”). It is not interactive: it takes no focus, has no command and lets clicks fall through to whatever sits behind it.

Inherits `CONCOST.UI.Controls.Ribbon.RibbonItem`.

### RibbonMenu

Vertical list of shown inside a dropdown or split button.

Inherits `System.Windows.Controls.ItemsControl`.

### RibbonMenuBar

A horizontal menu bar that sits above or below a ribbon (or stands alone). Each top-level opens a dropdown on click; hovering over another item while a menu is open switches to the new one.

Inherits `System.Windows.Controls.ItemsControl`.
- **Dependency properties (1):** `ShowDivider` — Draws the hairline under the bar. The bar itself always spans the full ribbon width, so this is the line that separates it from the tabs.
- **Properties (1):** `IsAnyOpen: bool` — True when any top-level item currently has its dropdown open.
- **Methods (1):** `CloseAll()` — Closes every open dropdown in this bar.

### RibbonMenuBarItem

One top-level entry of a . It has a header label and a dropdown that opens beneath it, populated with and instances.

Inherits `System.Windows.Controls.HeaderedItemsControl`.
- **Dependency properties (1):** `IsSubmenuOpen` — True while the dropdown beneath this item is showing.
- **Methods (2):** `Open()` — Opens the dropdown and focuses this item. · `Close()` — Closes the dropdown.

### RibbonMenuBarPlacement *(enum)*

Where the menu bar sits relative to the ribbon body.

Inherits `System.Enum`.

### RibbonMenuItem

One row of a . It can run a command, hold a nested submenu, or both. Invoking it raises , which bubbles up to the owning dropdown so the popup can close itself.

Inherits `System.Windows.Controls.HeaderedItemsControl`.
- **Dependency properties (7):** `Command` — Command executed when the item is invoked., `CommandParameter` — Value passed to ., `Icon` — Optional icon shown before the header., `InputGestureText` — Shortcut hint shown on the right (display only)., `IsSubmenuOpen` — True while the nested submenu is shown., `IsCheckable` — Reserves a tick column and flips on click instead of closing. Used by the quick-access customise list and by ribbon check boxes., `IsChecked` — Tick state of a row.
- **Routed events:** `Click` — Raised when the item is invoked.
- **CLR events:** `Click` — Raised when the item is invoked.
- **Methods (1):** `PerformClick()` — Runs the row as a click would. A split button uses it to replay the entry it last ran, so its body can stand for that entry instead of the default command.

### RibbonMenuSeparator

Horizontal rule inside a .

Inherits `System.Windows.Controls.Control`.

### RibbonQuickAccessButton

The icon-only button the quick-access toolbar uses. It exists as its own type so the theme can give it a style without the caller having to look a resource key up by hand.

Inherits `CONCOST.UI.Controls.Ribbon.RibbonButton`.

### RibbonQuickAccessPlacement *(enum)*

Where the quick-access strip sits relative to the ribbon.

Inherits `System.Enum`.

### RibbonQuickAccessToolBar

The small strip of always-available commands above the tabs.

Inherits `System.Windows.Controls.ItemsControl`.
- **Dependency properties (6):** `IsPinned`, `PinnableItems` — Every command the ribbon offers — used to build a fallback customise list., `IsCustomizeOpen` — Whether the customise panel is showing., `IsCustomizable` — Shows the chevron and lets the customise panel open. When false the pinned commands stay usable but the user cannot add or remove any — the locked-down build., `ShowLabels` — Shows the label next to each icon. Off by default, as in Office., `CustomizeMenu` — Content of the customise panel. The owning supplies a menu grouped by tab; when the toolbar is used on its own a flat list is generated from instead.
- **Attached properties:** `IsPinned`
- **CLR events:** `PinsChanged` — Raised whenever a command is pinned or unpinned.
- **Properties (1):** `PinnedItems: IEnumerable<RibbonItem>` — Original items that are currently pinned.
- **Methods (8):** `GetIsPinned(DependencyObject element)` · `SetIsPinned(DependencyObject element, bool value)` · `IsPinned(RibbonItem source)` · `Pin(RibbonItem source)` · `Unpin(RibbonItem source)` · `TogglePin(RibbonItem source)` · `ClearPins()` — Drops every pinned command. · `OnApplyTemplate()`

### RibbonRadioButton

A ribbon button that stays checked once clicked and never unchecks itself — combine with so only one button in the group is active.

Inherits `CONCOST.UI.Controls.Ribbon.RibbonToggleButton`.

### RibbonRadioButtonAutomationPeer

Reports a as a radio button rather than a check box.

Inherits `CONCOST.UI.Controls.Ribbon.RibbonToggleButtonAutomationPeer`.

### RibbonRadioGroup

A column of small buttons where exactly one is on — the “Wall / Floor / Ceiling” choice a ribbon shows as a compact set rather than as a dropdown.

Inherits `CONCOST.UI.Controls.Ribbon.RibbonItem`.
- **Dependency properties (2):** `SelectedIndex` — Index of the checked member, or -1 when the set has not been filled in yet., `SelectedItem` — The checked member itself — handy when the items are not plain strings.
- **Properties (1):** `Items: ObservableCollection<object>` — The options, in display order.
- **Methods (1):** `PerformClick()` — Runs the selected member. A radio group has no command of its own, so a key tip aimed at it should do what a click on the current option does.

### RibbonSearchBox

A ribbon-sized search field, e.g. the “Tell me what you want to do” box in the Office header. A thin shell around : the placeholder, the instant-search behaviour and the clear button all come from the base control. Give it = 2 or 3 so the query has room.

Inherits `CONCOST.UI.Controls.Ribbon.RibbonItem`.
- **Dependency properties (4):** `Text` — Current query., `Placeholder` — Hint shown while the query is empty., `IsInstantSearch` — Runs on every keystroke instead of only on Enter., `SearchCommand` — Command invoked with the query text.
- **Methods (1):** `PerformClick()` — Moves the caret into the field and selects the query.

### RibbonSeparator

Vertical divider between two logical clusters inside a .

Inherits `System.Windows.Controls.Control`.

### RibbonSplitButton

A button with two independent hit-test zones: the body runs , the arrow on the right opens the menu. This is the classic “Save / Save As” control.

Inherits `CONCOST.UI.Controls.Ribbon.RibbonItem`.
- **Dependency properties (6):** `IsOpen` — Whether the dropdown is showing., `ArrowWidth` — Width of the arrow zone, in pixels., `RememberLast` — When true, picking an entry from the menu turns the body into that entry — the “Save or Save As, then Save means Save As” behaviour of the ribbon., `LastUsedItem` — The entry the body currently stands for, or null while it means ., `BodyHeader` — Caption the body shows: the remembered entry's, or the button's own., `BodyIcon` — Icon the body shows: the remembered entry's, or the button's own.
- **CLR events:** `Click` — Raised when the main zone runs its command.
- **Properties (1):** `Items: ObservableCollection<object>` — Menu content shown by the arrow zone.
- **Methods (1):** `PerformClick()` — Runs the primary zone; use to reach the menu.

### RibbonState

The part of a ribbon worth remembering between sessions: which tab was open, whether it was minimised, which commands were pinned to the quick-access strip, and which groups the user folded away by hand. It is a plain data object on purpose — the library never writes a file. Capture it with , hand it to (or your own serialiser), and give it back with .
- **Properties (5):** `SelectedTabId: string` — of the tab that was open, or its header when no id was set., `IsMinimized: bool` — True when only the tab strip was showing., `QuickAccessItemIds: List<string>` — of every command pinned to the quick-access strip., `QuickAccessPlacement: string` — Where the quick-access strip was docked, as the name of a value. Stored as text so an old snapshot stays readable: an unknown name leaves the ribbon's current placement alone., `CollapsedGroupKeys: List<string>` — Groups the user collapsed by hand, as tabId:header. Groups the ribbon collapsed itself to save width are not stored — they come back on their own when there is room.

### RibbonStateStore *(static class)*

Turns a into JSON and back, and optionally reads/writes a file. The library does not decide where the state lives — a Revit add-in usually writes next to the .rvt or under %APPDATA%, a standalone app may use its own settings store. Call with the path you chose, or use and store the string wherever you like.
- **Methods (4):** `ToJson(RibbonState state)` · `FromJson(string json)` · `Save(RibbonState state, string path)` · `Load(string path)`

### RibbonTabItem

One tab of a . Its is the collection of instances the ribbon shows when this tab is selected.

Inherits `System.Windows.Controls.HeaderedContentControl`.
- **Dependency properties (8):** `Icon` — Optional icon shown before the header text., `IsSelected` — True when this tab is the active one., `IsContextual` — Marks a tab that only appears for a mode (Modify, Edit, Sketch…)., `ContextualColor` — Accent colour of a contextual tab header., `ContextualGroup` — Caption of the coloured band drawn above the tabs — “Drawing Tools”, “Table Tools”… Consecutive tabs that share the same value are drawn under one merged band. Leave empty for an ordinary tab., `IsAvailable` — False hides the tab from the strip without removing it from the item source. This is how a contextual group appears and disappears with the current mode., `KeyTip` — Access key shown by the KeyTip layer., `Id` — Stable identifier used when the ribbon state is saved and restored. Leave it empty to fall back on , which is fine as long as headers are unique and never translated at runtime.
- **CLR events:** `IsAvailableChanged` — Raised when changes, so the owning ribbon can re-filter.

### RibbonTabStripPanel

Lays out the ribbon's tab strip in two rows: a thin coloured band on top that names the contextual groups, and the tabs themselves underneath.

Inherits `System.Windows.Controls.Panel`.
- **Dependency properties (3):** `BandHeight` — Height of the contextual band. Zero hides it entirely., `FontFamily` — Font used for the band captions., `FontSize` — Size used for the band captions.

### RibbonTextBox

A compact single-line input that sits on the ribbon grid, optionally preceded by a label (), e.g. Width [ 120 ]. The control itself is a thin shell: the typing, selection and clipboard behaviour all come from a normal in the template, so IME, undo and accessibility keep working. Give it = 2 when the value needs room.

Inherits `CONCOST.UI.Controls.Ribbon.RibbonItem`.
- **Dependency properties (5):** `Text` — Current value., `PlaceholderText` — Hint shown while is empty., `IsReadOnly` — Prevents typing while still allowing selection and copy., `MaxLength` — Maximum number of characters, 0 for no limit., `TextAlignment` — Horizontal alignment of the typed text.
- **Methods (1):** `PerformClick()` — Moves the caret into the field and selects the current value.

### RibbonToggleButton

A ribbon button that keeps a checked state. Use to make a set of buttons behave like radio buttons (checking one clears the rest).

Inherits `CONCOST.UI.Controls.Ribbon.RibbonButton`.
- **Dependency properties (2):** `IsChecked` — Whether the button is checked., `GroupName` — Buttons sharing a non-empty group name behave as radio buttons.
- **CLR events:** `IsCheckedChanged` — Raised when changes. A listens to it to learn which member the user picked, without having to hook every button by hand.

### RibbonToggleButtonAutomationPeer

Adds the Toggle pattern to and .

Inherits `CONCOST.UI.Controls.Ribbon.RibbonButtonAutomationPeer`.
- **Properties (1):** `ToggleState: ToggleState`
- **Methods (2):** `GetPattern(PatternInterface patternInterface)` · `Toggle()`

### RibbonToolTip

The rich screen tip used by ribbon commands: a bold title, a wrapped description, the shortcut on the right and an optional preview bitmap — the layout Office and AutoCAD both use.

Inherits `System.Windows.Controls.ToolTip`.
- **Dependency properties (9):** `Title` — Command name, shown in bold at the top., `Description` — What the command does, in the short form of the tip., `GestureText` — Shortcut shown right-aligned, e.g. Ctrl+S. Display only., `Preview` — Optional picture shown by the expanded tip., `Footer` — Small grey line at the bottom of the expanded tip — “Press F1 for more help”., `ExpandedDescription` — The long form, shown under once the tip expands. Left empty for a simple command, whose tip then never grows., `ExpandDelay` — Milliseconds the pointer has to rest before the tip grows, 1500 by default., `IsExpanded` — True once the long form is showing., `CanExpand` — True when there is something the tip can grow into.

## CONCOST.UI.Controls.Windows

### CONCOSTWindow


Inherits `System.Windows.Window`.
- **Dependency properties (1):** `TitleBarBackground`
- **Methods (1):** `OnApplyTemplate()`

## CONCOST.UI.Core

### MotionSettings *(static class)*

Central switch for motion in the library. Effects check before running their frame loop, so the whole application follows the Windows "Show animations" setting (WCAG 2.2.2) and can be muted from code or XAML.
- **CLR events:** `Changed` — Raised when motion availability changes, so effects can repaint.
- **Properties (3):** `SystemAnimationsEnabled: bool` — Windows "Show animations in Windows" setting (true by default)., `ForceDisabled: bool` — Application-level override. Set to true to mute every effect regardless of the OS setting (for example a "reduce motion" toggle in the app settings)., `AnimationsEnabled: bool` — True when effects should animate. Defaults to the OS preference.

## CONCOST.UI.Core.Converters

### BooleanToVisibilityConverter

- **Properties (2):** `Invert: bool`, `Collapse: bool`
- **Methods (2):** `Convert(object value, Type targetType, object parameter, CultureInfo culture)` · `ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)`

### CountToVisibilityConverter

Converts an integer count to a value. Returns when the count is greater than zero, otherwise .
- **Methods (2):** `Convert(object value, Type targetType, object parameter, CultureInfo culture)` · `ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)`

### ExpanderCornerRadiusConverter

Calculates pixel-perfect inner corner radius for Expander Header and ContentSite, preventing child backgrounds from bleeding past the outer rounded border.
- **Properties (1):** `Offset: double`
- **Methods (4):** `Convert(Object[] values, Type targetType, object parameter, CultureInfo culture)` · `ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)` · `Convert(object value, Type targetType, object parameter, CultureInfo culture)` · `ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)`

### FilterOperatorLabelConverter

Turns a into the sentence-case label shown in the filter drop-down, so the menu reads “Starts with” rather than the camel-cased enum name.
- **Methods (2):** `Convert(object value, Type targetType, object parameter, CultureInfo culture)` · `ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)`

### NullToVisibilityConverter

- **Properties (1):** `Invert: bool`
- **Methods (2):** `Convert(object value, Type targetType, object parameter, CultureInfo culture)` · `ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)`

### PaddingToContentHostMarginConverter

Converts TextBox/PasswordBox Padding to a negative margin on PART_ContentHost so horizontal padding is not doubled when Grid Margin="{TemplateBinding Padding}" is present, aligning caret and placeholder pixel-perfectly.
- **Methods (2):** `Convert(object value, Type targetType, object parameter, CultureInfo culture)` · `ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)`

## CONCOST.UI.Core.Helpers

### ColorHelper *(static class)*

- **Methods (4):** `FromHex(string hex)` · `Lighten(Color color, double factor)` · `Darken(Color color, double factor)` · `CreateBrush(Color color, double opacity)`

### VisualTreeHelperEx *(static class)*

- **Methods (2):** `FindParent(DependencyObject child)` · `FindChild(DependencyObject parent)`

## CONCOST.UI.Core.MVVM

### AsyncRelayCommand

ICommand bất đồng bộ cho phép thực thi tác vụ Async mà không khóa giao diện.
- **CLR events:** `CanExecuteChanged`
- **Properties (1):** `IsExecuting: bool`
- **Methods (3):** `CanExecute(object parameter)` · `Execute(object parameter)` · `RaiseCanExecuteChanged()`

### ObservableObject *(abstract)*

Base class cho ViewModel hỗ trợ thông báo thay đổi thuộc tính INotifyPropertyChanged.
- **CLR events:** `PropertyChanged`

### RelayCommand

ICommand implementation đồng bộ cho kiến trúc MVVM.
- **CLR events:** `CanExecuteChanged`
- **Methods (3):** `CanExecute(object parameter)` · `Execute(object parameter)` · `RaiseCanExecuteChanged()`

### RelayCommand`1

ICommand strongly-typed parameter implementation.
- **CLR events:** `CanExecuteChanged`
- **Methods (3):** `CanExecute(object parameter)` · `Execute(object parameter)` · `RaiseCanExecuteChanged()`

## CONCOST.UI.Core.Settings

### ISettingsService *(interface)*

- **Methods (4):** `Get(string key, T defaultValue)` · `Set(string key, T value)` · `Save()` · `Load()`

### JsonSettingsService

- **Properties (1):** `Current: JsonSettingsService`
- **Methods (4):** `Get(string key, T defaultValue)` · `Set(string key, T value)` · `Save()` · `Load()`

## CONCOST.UI.Core.UndoRedo

### ChangeValueCommand`1

- **Properties (1):** `Name: string`
- **Methods (2):** `Execute()` · `Undo()`

### IUndoableCommand *(interface)*

- **Properties (1):** `Name: string`
- **Methods (2):** `Execute()` · `Undo()`

### UndoRedoManager


Inherits `CONCOST.UI.Core.MVVM.ObservableObject`.
- **Properties (4):** `Current: UndoRedoManager`, `MaxCapacity: int`, `CanUndo: bool`, `CanRedo: bool`
- **Methods (5):** `Execute(IUndoableCommand command)` · `Record(IUndoableCommand command)` · `Undo()` · `Redo()` · `Clear()`

## CONCOST.UI.Icons

### Icon


Inherits `System.Windows.Controls.Control`.
- **Dependency properties (2):** `Kind`, `Data`

### IconKind *(enum)*


Inherits `System.Enum`.
*No public members.*

### IconProvider *(static class)*

- **Methods (1):** `GetGeometry(IconKind kind)`

## CONCOST.UI.Services

### ApplicationStateService

The drafting state one window shares: which snaps are on, which plane new geometry lands on, where the cursor is, and how dirty the document is.
- **CLR events:** `ToggleChanged` — Raised when a drafting toggle changes, whoever changed it., `WorkPlaneChanged` — Raised when the work plane changes., `CursorMoved` — Raised when the reported cursor position changes., `StatusChanged` — Raised when the status line changes., `ModifiedChanged` — Raised when the modified flag changes.
- **Properties (10):** `Current: ApplicationStateService` — The shared instance the application uses., `SnapEnabled: bool` — Whether the cursor pulls onto snap points., `GridEnabled: bool` — Whether the grid is drawn., `OrthoEnabled: bool` — Whether drawing is constrained to horizontal and vertical., `PolarEnabled: bool` — Whether drawing is constrained to fixed angle increments., `ObjectSnapEnabled: bool` — Whether snaps to existing geometry are on., `WorkPlane: UcsMode` — The plane new geometry is drawn on., `CursorPosition: Point3D` — The cursor position in model space, as the status bar reports it., `StatusMessage: string` — The line the status bar shows, e.g. "Specify next point"., `IsModified: bool` — Whether the active document has unsaved changes.
- **Methods (5):** `Toggle(CadToggle toggle)` · `Attach(CadStatusBar bar)` · `Detach(CadStatusBar bar)` · `CaptureToggles()` — Copies the state into a plain object, for layout profiles. · `ApplyToggles(IReadOnlyDictionary<CadToggle, bool> toggles)`

### CameraChangedEventArgs

Carries the camera state after a change.

Inherits `System.EventArgs`.
- **Properties (2):** `Direction: Vector3D` — Current direction from the target towards the eye, normalised., `IsFinal: bool` — false while a gesture is still in flight, so a host can defer the heavy redraw.

### CameraController

The orbit camera a CAD viewport sits on: a target point, a distance, two angles and a roll.
- **CLR events:** `Changed` — Raised after any change; see ., `CameraChanged` — Raised alongside , for listeners that only care that something moved., `FitToViewRequested` — Raised when the view wants the scene framed again; the geometry is the host's business.
- **Properties (15):** `Target: Point3D` — The point the camera orbits and looks at., `Distance: double` — Distance from the eye to , in world units., `Yaw: double` — Rotation about the vertical axis, in degrees. 0 puts the eye on +X., `Pitch: double` — Elevation, in degrees: 90 looks straight down, -90 straight up., `Roll: double` — Rotation about the view axis, in degrees: positive rolls the scene clockwise on screen, which is what a view cube's roll buttons ask for., `FieldOfView: double` — Vertical field of view for , in degrees., `OrthographicHeight: double` — How many world units fit vertically under ., `Projection: CameraProjection` — Perspective or orthographic., `Direction: Vector3D` — Unit vector from the target towards the eye., `Eye: Point3D` — Where the eye sits, in world coordinates., `Forward: Vector3D` — Unit vector from the eye towards the target., `Right: Vector3D` — Screen-right in world space, turned by about the view axis. Falls back to the Y axis when the view is straight down, which is the only case where the normal construction degenerates., `Up: Vector3D` — Screen-up in world space, orthogonal to and ., `Rotation: Quaternion` — The orientation as a single value, the form a view cube and every other consumer of exchange. Reading it costs a few cross products; writing it splits the rotation back into yaw, pitch and roll, which is why the three angles stay the real state., `ViewMatrix: Matrix3D` — World-to-view matrix, for hosts that feed a camera to their own renderer.
- **Methods (12):** `VisibleHeight(double distance)` · `Orbit(double deltaYaw, double deltaPitch, bool isFinal)` · `Pan(double deltaX, double deltaY, Size viewport, bool isFinal)` · `Zoom(double factor, bool isFinal)` · `ZoomAt(Point anchor, Size viewport, double factor)` · `SetOrientation(Vector3D direction, bool isFinal)` · `LookAt(Point3D target)` · `FitTo(Rect3D bounds, double margin)` · `FitToView()` — Frames the model again. The bounds of the scene belong to the host, so this asks rather than guesses: a viewport answers by calling with what it actually contains. · `TryProject(Point3D world, Size viewport, Point& screen)` · `Unproject(Point point, Size viewport)` · `CopyTo(CameraController other)`

### CameraProjection *(enum)*

How a turns the view volume into screen coordinates.

Inherits `System.Enum`.

### CameraSnapshot *(struct)*

The camera as plain numbers, so a view's framing can be saved, restored and compared without keeping a live camera alive.

Inherits `System.ValueType`.
- **Properties (7):** `Target: Point3D`, `Distance: double`, `Yaw: double`, `Pitch: double`, `FieldOfView: double`, `OrthographicHeight: double`, `Projection: CameraProjection`
- **Methods (2):** `Capture(CameraController camera)` · `ApplyTo(CameraController camera)`

### DataGridExportService *(static class)*

Exports the visible content of a to CSV, Excel-compatible XML, or HTML formats without external dependencies.
- **Methods (3):** `ExportToCsv(DataGrid grid, string filePath, Char delimiter)` · `ExportToExcelXml(DataGrid grid, string filePath, string worksheetName)` · `ExportToHtml(DataGrid grid, string filePath, string title)`

### DialogResult *(enum)*


Inherits `System.Enum`.
*No public members.*

### DialogService

- **Properties (1):** `Current: DialogService`
- **Methods (2):** `ShowMessageAsync(string title, string message, DialogType type)` · `ShowConfirmAsync(string title, string message)`

### DialogType *(enum)*


Inherits `System.Enum`.
*No public members.*

### DocumentClosingEventArgs

Asks whether a document may close.

Inherits `System.EventArgs`.
- **Properties (2):** `Document: DocumentModel` — The document that is closing., `Cancel: bool` — Set to true to keep the document open, e.g. to show a save prompt.

### DocumentManager

The set of open documents and which one is active.
- **CLR events:** `DocumentClosing` — Raised before a document closes; cancel it to veto., `DocumentOpened` — Raised after a document was opened., `DocumentClosed` — Raised after a document closed., `ActiveDocumentChanged` — Raised when the active document changes., `DirtyChanged` — Raised when a document's modified flag changes.
- **Properties (3):** `Current: DocumentManager` — The shared instance the application uses., `Documents: ObservableCollection<DocumentModel>` — The open documents, in open order., `ActiveDocument: DocumentModel` — The document commands act on.
- **Methods (5):** `Open(string title, string filePath)` · `Close(DocumentModel document)` · `CloseAll()` — Closes every document, honouring the same veto on each. · `SetDirty(DocumentModel document, bool isDirty)` · `BuildTitle(string applicationName)`

### DocumentModel

One open project document: the file, its title, its modified flag.
- **Properties (6):** `Title: string` — The name shown on the tab and the title bar, without the modified marker., `FilePath: string` — Where the document came from, or null for one that was never saved., `IsDirty: bool` — True when there are unsaved changes., `CreatedUtc: DateTime` — When the document was opened., `Tag: object` — Anything the host hangs off the document., `DisplayTitle: string` — The title with the modified marker a tab shows.
- **Methods (1):** `ToString()`

### ExportService

- **Properties (1):** `Current: ExportService`
- **Methods (2):** `ExportDataGridToCsv(DataGrid grid, string filePath)` · `ExportVisualToPng(FrameworkElement visual, string filePath, double dpi)`

### HotkeyManager *(static class)*

- **Methods (3):** `Register(string gesture, ICommand command)` · `Unregister(string gesture)` · `AttachToWindow(Window window)`

### ICameraHost *(interface)*

The slice of a camera a view cube needs, and the whole of what it is allowed to touch.
- **Properties (3):** `Rotation: Quaternion` — The current orientation, as a rotation from world axes onto the screen axes. Read on every frame; written only while a transition animates or the user drags the cube., `PivotPoint: Point3D` — The point the camera turns around, which a transition leaves where it is., `Projection: ProjectionMode` — The projection in force, or the mode asked for when it is .
- **Methods (1):** `FitToView()` — Frames the model again, for FitToViewOnChange.

### ICameraHostEvents *(interface)*

Optional companion to : a camera that can say when it moved.
- **CLR events:** `CameraChanged` — Raised after the camera moved, turned or changed projection.

### IDialogService *(interface)*

- **Methods (2):** `ShowMessageAsync(string title, string message, DialogType type)` · `ShowConfirmAsync(string title, string message)`

### IRendererAdapter *(interface)*

The seam between the UI and whatever actually draws the model.
- **Methods (3):** `TryProject(Point3D world, Size viewport, Point& screen)` · `TryUnproject(Point screen, Size viewport, Point3D& world)` · `TryPick(Point screen, Size viewport, Object& entity)`

### ITypeSelectable *(interface)*

Implemented by a model object whose type can be swapped for another one — the wall that becomes a curtain wall, the family instance that changes family type.
- **Properties (2):** `TypeName: string` — Type the object currently is., `AvailableTypes: IReadOnlyList<string>` — Types the object could become.

### LocalizationManager


Inherits `CONCOST.UI.Core.MVVM.ObservableObject`.
- **Properties (2):** `Current: LocalizationManager`, `Culture: CultureInfo`
- **Methods (1):** `GetString(string key, string defaultValue)`

### PickEventArgs

Carries the point that was picked and what was under it.

Inherits `System.EventArgs`.
- **Properties (2):** `World: Point3D` — Where the pick landed in model space., `Entity: object` — What the renderer reported under the cursor, when it has an adapter.

### PrintService

- **Properties (1):** `Current: PrintService`
- **Methods (1):** `PrintVisual(Visual visual, string jobDescription)`

### ProjectionMode *(enum)*

How a host turns its scene into screen coordinates.

Inherits `System.Enum`.

### PropertyDescriptor

One editable property of a model object: how to read it, how to write it, and how the palette should present it.
- **Properties (11):** `Name: string` — Property key, unique within a category., `DisplayName: string` — Caption in the palette; defaults to ., `Category: string` — Group the row is sorted into., `Description: string` — Tooltip text., `Editor: PropertyEditorKind` — Which editor to use., `DefaultValue: object` — Value the reset button goes back to., `Choices: IReadOnlyList<object>` — Options for ., `Unit: string` — Unit suffix such as mm, shown after the value., `Format: string` — Format string for numeric values., `Validator: Func<object, object, string>` — Returns an error message for an invalid value, or null when it is acceptable., `IsReadOnly: bool` — true when the property has no setter.
- **Methods (2):** `GetValue(object target)` · `SetValue(object target, object value)`

### PropertyEditorKind *(enum)*

The editor the properties palette shows for a value.

Inherits `System.Enum`.

### PropertyManager

The property system behind : which properties an object has, how to read and write them, and validation.
- **CLR events:** `ValueChanged` — Raised after a value was written.
- **Properties (1):** `Current: PropertyManager` — The instance a host shares between its palette and its commands.
- **Methods (9):** `Register(PropertyDescriptor[] descriptors)` · `Register(Type type, PropertyDescriptor[] descriptors)` · `GetDescriptors(object target)` · `Find(object target, string name)` · `GetValue(object target, string name)` · `Validate(object target, PropertyDescriptor property, object value)` · `TrySetValue(object target, string name, object value, String& error)` · `TrySetValueFromText(object target, string name, string text, String& error)` · `TryConvert(PropertyDescriptor property, string text, Object& value, String& error)`

### PropertyValueChangedEventArgs

Carries one committed property edit.

Inherits `System.EventArgs`.
- **Properties (4):** `Target: object` — The edited object., `Property: PropertyDescriptor` — The descriptor that was written., `OldValue: object` — Value before the edit., `NewValue: object` — Value after the edit.

### RendererBridge

One camera and one optional renderer, shared by the viewport, the coordinate readout and anything that needs to map between model space and the screen.
- **CLR events:** `RenderRequested` — Raised when a redraw is needed, e.g. after the camera moved., `HoverChanged` — Raised when the hovered entity changes.
- **Properties (5):** `Current: RendererBridge` — The shared instance the application uses., `Camera: CameraController` — The camera every projection goes through., `Adapter: IRendererAdapter` — The external renderer, when the host has one., `IsAccelerated: bool` — True when a real renderer answers the pick and projection questions., `CursorWorld: Point3D` — The model-space point under the cursor.
- **Methods (5):** `Invalidate()` — Asks every listener to redraw. · `TryProject(Point3D world, Size viewport, Point& screen)` · `TryUnproject(Point screen, Size viewport, Point3D& world)` · `TryPick(Point screen, Size viewport, Object& entity)` · `UpdateCursor(Point screen, Size viewport)`

### SelectionChangeEventArgs

Carries the selection after a change.

Inherits `System.EventArgs`.
- **Properties (5):** `Kind: SelectionChangeKind` — What kind of change this was., `Added: IReadOnlyList<object>` — Objects that entered the selection., `Removed: IReadOnlyList<object>` — Objects that left it., `Primary: object` — The object the properties palette shows, or null when nothing is selected., `IsEmpty: bool` — true when the selection is now empty.

### SelectionChangeKind *(enum)*

How a selection changed.

Inherits `System.Enum`.

### SelectionManager

What is selected, shared by the viewport, the properties palette, the status bar counter and the contextual ribbon.
- **CLR events:** `SelectionChanged` — Raised after any change, with what was added, what was removed and the new primary.
- **Properties (5):** `Current: SelectionManager` — The instance a host wires its panels to, unless it creates its own for a test., `Items: IReadOnlyList<object>` — What is selected, in selection order., `Count: int` — How many objects are selected — the number the status bar shows., `Primary: object` — The most recently selected object, or null., `IsEmpty: bool` — Whether the selection is empty.
- **Methods (7):** `Select(object item)` · `SelectMany(IEnumerable<object> items)` · `Add(object item)` · `Remove(object item)` · `Toggle(object item)` · `Clear()` — Empties the selection. · `Contains(object item)`

### ToastItem

- **Properties (3):** `Title: string`, `Message: string`, `Type: ToastType`

### ToastService

- **Properties (2):** `Current: ToastService`, `ActiveToasts: ObservableCollection<ToastItem>`
- **Methods (5):** `Show(string message, string title, ToastType type, int durationMs)` · `Success(string message, string title)` · `Warning(string message, string title)` · `Error(string message, string title)` · `Info(string message, string title)`

### ToastType *(enum)*


Inherits `System.Enum`.
*No public members.*

### Transaction

A group of edits that reaches the undo stack as a single step.
- **Properties (3):** `Name: string` — Name that will appear on the undo stack., `ChangeCount: int` — How many edits were recorded., `IsCompleted: bool` — Whether the transaction was committed or rolled back.
- **Methods (5):** `Record(string name, Action apply, Action revert)` · `Record(IUndoableCommand command)` · `Commit()` — Applies nothing further and puts the whole group on the undo stack as one step. · `Rollback()` — Replays the inverses in reverse order and leaves the undo stack alone. · `Dispose()` — Commits unless the caller already did something else with it.

### TransactionEventArgs

Carries the name and size of a transaction that has been committed or rolled back.

Inherits `System.EventArgs`.
- **Properties (2):** `Name: string` — Name shown in the undo menu, e.g. Change parameter., `ChangeCount: int` — How many individual edits the transaction grouped.

### TransactionManager

Opens transactions and hands the finished group to the undo stack.
- **CLR events:** `Committed` — Raised after a transaction commits., `RolledBack` — Raised after a transaction rolls back., `Changed` — Raised after either outcome, so a view can refresh once instead of per edit.
- **Properties (3):** `Current: TransactionManager` — The instance a host shares between its commands., `Active: Transaction` — The transaction that is currently open, if any., `IsInTransaction: bool` — Whether a transaction is open.
- **Methods (2):** `Begin(string name)` · `Run(string name, Action<Transaction> edit)`

### ViewDefinition

A view a document can open: a name, a kind and the framing it was left in.
- **Properties (5):** `Name: string` — The name shown in the project browser and on the tab., `Kind: ViewKind` — What the view shows., `Camera: CameraSnapshot` — The framing the view was last closed with., `IsOpen: bool` — True while the view has a tab., `Tag: object` — The object behind the view, e.g. a database row.
- **Methods (1):** `ToString()`

### ViewEventArgs

Carries the view that was opened or closed.

Inherits `System.EventArgs`.
- **Properties (1):** `View: ViewDefinition` — The view the event is about.

### ViewKind *(enum)*

What a view shows.

Inherits `System.Enum`.

### ViewManager

The views of the active project and which one is in front.
- **CLR events:** `OpenRequested` — Raised when a view should get a tab and a viewport., `ViewClosed` — Raised after a view closed., `ActiveViewChanged` — Raised when the view in front changes.
- **Properties (3):** `Current: ViewManager` — The shared instance the application uses., `Views: ObservableCollection<ViewDefinition>` — The views the project knows about., `ActiveView: ViewDefinition` — The view in front.
- **Methods (7):** `Add(string name, ViewKind kind)` · `Find(string name)` · `Open(ViewDefinition view)` · `Open(string name, ViewKind kind)` · `Close(ViewDefinition view)` · `RememberCamera(ViewDefinition view, CameraController camera)` · `RestoreCamera(ViewDefinition view, CameraController camera)`

### WorkspaceLayoutProfile

A saved arrangement of the workspace: how the views are tiled, which view is in front, and which drafting settings were on.
- **Properties (10):** `Name: string` — The name the user picks in the layout list., `TileMode: DocumentTileMode` — How the views were tiled., `ActiveViewName: string` — The view that was in front, by name., `WorkPlane: UcsMode` — The work plane that was active., `SnapEnabled: bool` — Whether snap was on., `GridEnabled: bool` — Whether the grid was on., `OrthoEnabled: bool` — Whether ortho was on., `PolarEnabled: bool` — Whether polar tracking was on., `ObjectSnapEnabled: bool` — Whether object snap was on., `SavedUtc: DateTime` — When the profile was saved.
- **Methods (3):** `ToToggles()` — Round-trips the profile to a dictionary, for hosts that store their own settings. · `FromToggles(IReadOnlyDictionary<CadToggle, bool> toggles)` · `ToString()`

### WorkspaceLayoutService

Captures, stores and restores workspace layout profiles.
- **CLR events:** `ProfileApplied` — Raised after a profile is applied., `ProfilesChanged` — Raised after the profile list changes.
- **Properties (5):** `Current: WorkspaceLayoutService` — The shared instance the application uses., `Profiles: ObservableCollection<WorkspaceLayoutProfile>` — The saved profiles, in save order., `Workspace: DocumentWorkspace` — The workspace a profile is captured from and applied to., `StatusBar: CadStatusBar` — The status bar a profile is captured from and applied to., `ActiveViewName: string` — The name of the view in front, which the shell sets when the active tab changes.
- **Methods (8):** `Attach(DocumentWorkspace workspace, CadStatusBar statusBar)` · `Capture(string name)` · `Save(string name)` · `Apply(WorkspaceLayoutProfile profile)` · `Apply(string name)` · `Find(string name)` · `Remove(string name)` · `Persist()` — Writes the profile list to settings.

## CONCOST.UI.Themes

### AppTheme *(enum)*


Inherits `System.Enum`.
*No public members.*

### SurfaceStyle *(enum)*


Inherits `System.Enum`.
*No public members.*

### ThemeManager


Inherits `CONCOST.UI.Core.MVVM.ObservableObject`.
- **Properties (5):** `Current: ThemeManager`, `Theme: AppTheme`, `Preset: ThemePreset`, `SurfaceStyle: SurfaceStyle`, `AccentColor: Color`
- **Methods (9):** `ApplyPreset(ThemePreset preset)` · `ApplySurfaceStyle(SurfaceStyle style)` · `ApplyTheme(AppTheme theme)` · `ApplyTheme(FrameworkElement rootElement, AppTheme theme)` · `ApplySurfaceStyle(FrameworkElement rootElement, SurfaceStyle style)` · `ApplyAccentColor(FrameworkElement rootElement, Color color)` · `ApplyAccentColor(Color color)` · `DetectTheme(FrameworkElement probe)` · `RefreshAutoTheme(FrameworkElement rootElement)`

### ThemePreset *(enum)*


Inherits `System.Enum`.
*No public members.*

## CONCOST.UI.Themes.Tools

### AccentPalette

- **Properties (8):** `Primary: Color`, `PrimaryLight: Color`, `PrimaryDark: Color`, `PrimaryHover: Color`, `PrimaryPressed: Color`, `PrimaryDisabled: Color`, `PrimaryText: Color`, `PrimarySoft: Color` — The accent at 12% alpha: a tint that works over any surface without a second blend.

### AccentPaletteGenerator *(static class)*

- **Methods (1):** `Generate(Color baseColor)`

### ContrastChecker *(static class)*

- **Methods (1):** `Check(Color fg, Color bg)`

### ContrastResult

- **Properties (3):** `Ratio: double`, `PassesAA: bool`, `PassesAAA: bool`

### CustomThemeDefinition

- **Properties (7):** `Name: string`, `PrimaryHex: string`, `BackgroundHex: string`, `SurfaceHex: string`, `TextHex: string`, `CornerRadius: double`, `SurfaceStyle: string`

### ThemeAnimationHelper *(static class)*

- **Methods (2):** `AnimateThemeChange(Window window, Nullable<Point> clickOrigin, Action changeThemeAction)` · `AnimateThemeChange(FrameworkElement rootElement, Nullable<Point> clickOrigin, Action changeThemeAction)`

### ThemeBuilder

- **Methods (8):** `Create()` · `WithName(string name)` · `WithPrimary(Color color)` · `WithCornerRadius(double radius)` · `WithSurfaceStyle(string style)` · `Build()` · `ToJson()` · `FromJson(string json)`

### ThemeTransitionAdorner

Adorner that renders a smooth circular mask / crossfade transition from a snapshot of the previous theme. Inspired by the modern Web View Transitions API and the circular-reveal theme transition.

Inherits `System.Windows.Documents.Adorner`.
- **Dependency properties (1):** `Progress`

## XamlGeneratedNamespace

### GeneratedInternalTypeHelper

GeneratedInternalTypeHelper

Inherits `System.Windows.Markup.InternalTypeHelper`.

