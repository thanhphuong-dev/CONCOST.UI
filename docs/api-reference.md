# CONCOST.UI — API Reference

Generated from the built assembly by `docs/tools/ApiGen` — do not edit by hand.

**484 public types** · 385 classes · 85 enums · 6 structs · 8 interfaces.

Dependency properties, attached properties and routed events are the XAML surface; the
CLR events and public methods are the code-behind surface. Summaries come from the XML
documentation the build emits next to the assembly.

## Contents

- [CONCOST.UI.AttachedProperties](#concostuiattachedproperties) — 20 types
- [CONCOST.UI.Controls](#concostuicontrols) — 300 types
- [CONCOST.UI.Controls.Canvas](#concostuicontrolscanvas) — 5 types
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
- [CONCOST.UI.Services](#concostuiservices) — 44 types
- [CONCOST.UI.Themes](#concostuithemes) — 4 types
- [CONCOST.UI.Themes.Tools](#concostuithemestools) — 8 types
- [XamlGeneratedNamespace](#xamlgeneratednamespace) — 1 types

---

## CONCOST.UI.AttachedProperties

### AccessibleName *(static class)*

- **Dependency properties (1):** `FromToolTip`
- **Attached properties:** `FromToolTip`
- **Methods (2):** `SetFromToolTip(DependencyObject element, bool value)` · `GetFromToolTip(DependencyObject element)`

### BorderElement *(static class)*

- **Dependency properties (4):** `CornerRadius`, `Circular`, `HoverBorderBrush`, `HoverBackground`
- **Attached properties:** `Circular`, `CornerRadius`, `HoverBackground`, `HoverBorderBrush`
- **Methods (8):** `GetCornerRadius(DependencyObject obj)` · `SetCornerRadius(DependencyObject obj, CornerRadius value)` · `GetCircular(DependencyObject obj)` · `SetCircular(DependencyObject obj, bool value)` · `GetHoverBorderBrush(DependencyObject obj)` · `SetHoverBorderBrush(DependencyObject obj, Brush value)` · `GetHoverBackground(DependencyObject obj)` · `SetHoverBackground(DependencyObject obj, Brush value)`

### ButtonElement *(static class)*

- **Dependency properties (3):** `IsLoading`, `LoadingText`, `Badge`
- **Attached properties:** `Badge`, `IsLoading`, `LoadingText`
- **Methods (6):** `GetIsLoading(DependencyObject obj)` · `SetIsLoading(DependencyObject obj, bool value)` · `GetLoadingText(DependencyObject obj)` · `SetLoadingText(DependencyObject obj, string value)` · `GetBadge(DependencyObject obj)` · `SetBadge(DependencyObject obj, object value)`

### ComboBoxHelper *(static class)*

- **Dependency properties (3):** `IsSearchEnabled`, `SearchText`, `SearchPlaceholder`
- **Attached properties:** `IsSearchEnabled`, `SearchPlaceholder`, `SearchText`
- **Methods (7):** `GetIsSearchEnabled(DependencyObject obj)` · `SetIsSearchEnabled(DependencyObject obj, bool value)` · `GetSearchText(DependencyObject obj)` · `SetSearchText(DependencyObject obj, string value)` · `GetSearchPlaceholder(DependencyObject obj)` · `SetSearchPlaceholder(DependencyObject obj, string value)` · `ResetSearch(ComboBox combo)`

### DataGridElement *(static class)*

- **Dependency properties (7):** `RowSpacing`, `ShowRowDivider`, `HeaderHorizontalContentAlignment`, `CellHorizontalContentAlignment`, `TextHeader`, `HeaderAlignment`, `CellAlignment`
- **Attached properties:** `CellAlignment`, `CellHorizontalContentAlignment`, `HeaderAlignment`, `HeaderHorizontalContentAlignment`, `RowSpacing`, `ShowRowDivider`, `TextHeader`
- **Methods (14):** `GetRowSpacing(DependencyObject obj)` · `SetRowSpacing(DependencyObject obj, double value)` · `GetShowRowDivider(DependencyObject obj)` · `SetShowRowDivider(DependencyObject obj, bool value)` · `GetHeaderHorizontalContentAlignment(DependencyObject obj)` · `SetHeaderHorizontalContentAlignment(DependencyObject obj, HorizontalAlignment value)` · `GetCellHorizontalContentAlignment(DependencyObject obj)` · `SetCellHorizontalContentAlignment(DependencyObject obj, HorizontalAlignment value)` · `GetTextHeader(DependencyObject obj)` · `SetTextHeader(DependencyObject obj, string value)` · `GetHeaderAlignment(DependencyObject obj)` · `SetHeaderAlignment(DependencyObject obj, Nullable<HorizontalAlignment> value)` · `GetCellAlignment(DependencyObject obj)` · `SetCellAlignment(DependencyObject obj, Nullable<HorizontalAlignment> value)`

### DragDropHelper *(static class)*

- **Dependency properties (3):** `IsDragSource`, `IsDropTarget`, `DragDropGroup`
- **Attached properties:** `DragDropGroup`, `IsDragSource`, `IsDropTarget`
- **Methods (6):** `GetIsDragSource(DependencyObject obj)` · `SetIsDragSource(DependencyObject obj, bool value)` · `GetIsDropTarget(DependencyObject obj)` · `SetIsDropTarget(DependencyObject obj, bool value)` · `GetDragDropGroup(DependencyObject obj)` · `SetDragDropGroup(DependencyObject obj, string value)`

### DragPreviewAdorner


Inherits `System.Windows.Documents.Adorner`.
- **Methods (1):** `UpdatePosition(Point point)`

### IconElement *(static class)*

- **Dependency properties (3):** `Kind`, `IconSize`, `Placement`
- **Attached properties:** `IconSize`, `Kind`, `Placement`
- **Methods (6):** `GetKind(DependencyObject obj)` · `SetKind(DependencyObject obj, IconKind value)` · `GetIconSize(DependencyObject obj)` · `SetIconSize(DependencyObject obj, double value)` · `GetPlacement(DependencyObject obj)` · `SetPlacement(DependencyObject obj, Dock value)`

### InfoElement *(static class)*

- **Dependency properties (9):** `Placeholder`, `Title`, `TitlePlacement`, `Required`, `Prefix`, `Suffix`, `ShowClearButton`, `IsClearButtonAlwaysVisible`, `ShowCharacterCount`
- **Attached properties:** `IsClearButtonAlwaysVisible`, `Placeholder`, `Prefix`, `Required`, `ShowCharacterCount`, `ShowClearButton`, `Suffix`, `Title`, `TitlePlacement`
- **Properties (1):** `ClearCommand: RoutedUICommand`
- **Methods (18):** `GetPlaceholder(DependencyObject obj)` · `SetPlaceholder(DependencyObject obj, string value)` · `GetTitle(DependencyObject obj)` · `SetTitle(DependencyObject obj, string value)` · `GetTitlePlacement(DependencyObject obj)` · `SetTitlePlacement(DependencyObject obj, Dock value)` · `GetRequired(DependencyObject obj)` · `SetRequired(DependencyObject obj, bool value)` · `GetPrefix(DependencyObject obj)` · `SetPrefix(DependencyObject obj, object value)` · `GetSuffix(DependencyObject obj)` · `SetSuffix(DependencyObject obj, object value)` · `GetShowClearButton(DependencyObject obj)` · `SetShowClearButton(DependencyObject obj, bool value)` · `GetIsClearButtonAlwaysVisible(DependencyObject obj)` · `SetIsClearButtonAlwaysVisible(DependencyObject obj, bool value)` · `GetShowCharacterCount(DependencyObject obj)` · `SetShowCharacterCount(DependencyObject obj, bool value)`

### InsertionAdorner


Inherits `System.Windows.Documents.Adorner`.
- **Properties (1):** `IsBottom: bool`

### PasswordBoxHelper *(static class)*

- **Dependency properties (3):** `Password`, `Attach`, `PasswordLength`
- **Attached properties:** `Attach`, `Password`, `PasswordLength`
- **Methods (6):** `GetPassword(DependencyObject dp)` · `SetPassword(DependencyObject dp, string value)` · `GetAttach(DependencyObject dp)` · `SetAttach(DependencyObject dp, bool value)` · `GetPasswordLength(DependencyObject dp)` · `SetPasswordLength(DependencyObject dp, int value)`

### ProgressBarElement *(static class)*

- **Dependency properties (2):** `ShowText`, `TextPattern`
- **Attached properties:** `ShowText`, `TextPattern`
- **Methods (4):** `GetShowText(DependencyObject obj)` · `SetShowText(DependencyObject obj, bool value)` · `GetTextPattern(DependencyObject obj)` · `SetTextPattern(DependencyObject obj, string value)`

### ScrollViewerHelper *(static class)*

- **Dependency properties (8):** `IsInertiaEnabled`, `ScrollSpeed`, `BubbleAtBoundary`, `IsAutoHideEnabled`, `ShowBackToTop`, `BackToTopThreshold`, `CurrentVerticalOffset`, `CurrentHorizontalOffset`
- **Attached properties:** `BackToTopThreshold`, `BubbleAtBoundary`, `CurrentHorizontalOffset`, `CurrentVerticalOffset`, `IsAutoHideEnabled`, `IsInertiaEnabled`, `ScrollSpeed`, `ShowBackToTop`
- **Methods (21):** `GetIsInertiaEnabled(DependencyObject obj)` · `SetIsInertiaEnabled(DependencyObject obj, bool value)` · `GetScrollSpeed(DependencyObject obj)` · `SetScrollSpeed(DependencyObject obj, double value)` · `GetBubbleAtBoundary(DependencyObject obj)` · `SetBubbleAtBoundary(DependencyObject obj, bool value)` · `GetIsAutoHideEnabled(DependencyObject obj)` · `SetIsAutoHideEnabled(DependencyObject obj, bool value)` · `GetShowBackToTop(DependencyObject obj)` · `SetShowBackToTop(DependencyObject obj, bool value)` · `GetBackToTopThreshold(DependencyObject obj)` · `SetBackToTopThreshold(DependencyObject obj, double value)` · `GetCurrentVerticalOffset(DependencyObject obj)` · `SetCurrentVerticalOffset(DependencyObject obj, double value)` · `GetCurrentHorizontalOffset(DependencyObject obj)` · `SetCurrentHorizontalOffset(DependencyObject obj, double value)` · `ScrollToTop(ScrollViewer sv, bool smooth)` · `ScrollToBottom(ScrollViewer sv, bool smooth)` · `ScrollToLeft(ScrollViewer sv, bool smooth)` · `ScrollToRight(ScrollViewer sv, bool smooth)` · `ScrollTo(ScrollViewer sv, double verticalOffset, double horizontalOffset, bool smooth)`

### SurfaceBorder *(enum)*


Inherits `System.Enum`.
*No public members.*

### SurfaceElement *(static class)*

- **Dependency properties (3):** `Role`, `Border`, `Radius`
- **Attached properties:** `Border`, `Radius`, `Role`
- **Methods (6):** `GetRole(DependencyObject obj)` · `SetRole(DependencyObject obj, SurfaceRole value)` · `GetBorder(DependencyObject obj)` · `SetBorder(DependencyObject obj, SurfaceBorder value)` · `GetRadius(DependencyObject obj)` · `SetRadius(DependencyObject obj, Nullable<CornerRadius> value)`

### SurfaceRole *(enum)*


Inherits `System.Enum`.
*No public members.*

### TabControlHelper *(static class)*

- **Dependency properties (2):** `IsClosable`, `HeaderExtraContent`
- **Attached properties:** `HeaderExtraContent`, `IsClosable`
- **Routed events:** `TabClosing`, `TabClosed`
- **Properties (2):** `CloseCommand: RoutedUICommand`, `AddCommand: RoutedUICommand`
- **Methods (8):** `GetIsClosable(DependencyObject obj)` · `SetIsClosable(DependencyObject obj, bool value)` · `GetHeaderExtraContent(DependencyObject obj)` · `SetHeaderExtraContent(DependencyObject obj, object value)` · `AddTabClosingHandler(DependencyObject d, RoutedEventHandler handler)` · `RemoveTabClosingHandler(DependencyObject d, RoutedEventHandler handler)` · `AddTabClosedHandler(DependencyObject d, RoutedEventHandler handler)` · `RemoveTabClosedHandler(DependencyObject d, RoutedEventHandler handler)`

### TextBoxHelper *(static class)*

- **Dependency properties (5):** `CharacterCountText`, `SelectAllOnFocus`, `ClearOnEscape`, `CommitOnEnter`, `InputType`
- **Attached properties:** `CharacterCountText`, `ClearOnEscape`, `CommitOnEnter`, `InputType`, `SelectAllOnFocus`
- **Methods (11):** `GetCharacterCountText(DependencyObject obj)` · `UpdateCharacterCount(TextBox tb)` · `GetSelectAllOnFocus(DependencyObject obj)` · `SetSelectAllOnFocus(DependencyObject obj, bool value)` · `GetClearOnEscape(DependencyObject obj)` · `SetClearOnEscape(DependencyObject obj, bool value)` · `GetCommitOnEnter(DependencyObject obj)` · `SetCommitOnEnter(DependencyObject obj, bool value)` · `GetInputType(DependencyObject obj)` · `SetInputType(DependencyObject obj, TextBoxInputType value)` · `IsTextAllowed(TextBox tb, string newText, TextBoxInputType inputType)`

### TextBoxInputType *(enum)*


Inherits `System.Enum`.
*No public members.*

### VisualElement *(static class)*

- **Dependency properties (10):** `Elevation`, `HoverScale`, `HoverElevation`, `Glow`, `PressScale`, `DisabledOpacity`, `FocusRing`, `MotionDuration`, `ShadowColor`, `ScaleOrigin`
- **Attached properties:** `DisabledOpacity`, `Elevation`, `FocusRing`, `Glow`, `HoverElevation`, `HoverScale`, `MotionDuration`, `PressScale`, `ScaleOrigin`, `ShadowColor`
- **Methods (20):** `GetElevation(DependencyObject obj)` · `SetElevation(DependencyObject obj, int value)` · `GetHoverScale(DependencyObject obj)` · `SetHoverScale(DependencyObject obj, double value)` · `GetHoverElevation(DependencyObject obj)` · `SetHoverElevation(DependencyObject obj, int value)` · `GetGlow(DependencyObject obj)` · `SetGlow(DependencyObject obj, bool value)` · `GetPressScale(DependencyObject obj)` · `SetPressScale(DependencyObject obj, double value)` · `GetDisabledOpacity(DependencyObject obj)` · `SetDisabledOpacity(DependencyObject obj, double value)` · `GetFocusRing(DependencyObject obj)` · `SetFocusRing(DependencyObject obj, bool value)` · `GetMotionDuration(DependencyObject obj)` · `SetMotionDuration(DependencyObject obj, double value)` · `GetShadowColor(DependencyObject obj)` · `SetShadowColor(DependencyObject obj, Nullable<Color> value)` · `GetScaleOrigin(DependencyObject obj)` · `SetScaleOrigin(DependencyObject obj, double value)`

## CONCOST.UI.Controls

### AggregateType *(enum)*


Inherits `System.Enum`.
*No public members.*

### AnimatedEffectBase *(abstract)*


Inherits `System.Windows.FrameworkElement`.
- **Dependency properties (1):** `IsAnimated`
- **Methods (1):** `Restart()`

### AnimatedGradientBorder


Inherits `System.Windows.Controls.ContentControl`.
- **Dependency properties (8):** `ColorA`, `ColorB`, `ColorC`, `RingThickness`, `Corner`, `SecondsPerTurn`, `IsAnimated`, `Fill`
- **Methods (1):** `OnApplyTemplate()`

### AnimatedMeshGradient


Inherits `CONCOST.UI.Controls.AnimatedEffectBase`.
- **Dependency properties (8):** `ColorA`, `ColorB`, `ColorC`, `ColorD`, `Speed`, `BlobOpacity`, `Spread`, `Background`

### ApplicationInfoBar


Inherits `System.Windows.Controls.Border`.
- **Dependency properties (9):** `Severity`, `Title`, `Message`, `ActionText`, `IsClosable`, `IconSize`, `ShowIcon`, `ActionMinWidth`, `TextFontSize`
- **CLR events:** `ActionInvoked`, `Closed`
- **Methods (2):** `Show(string title, string message, CadNotificationSeverity severity)` · `Hide()`

### ApplicationTitleBar


Inherits `System.Windows.Controls.Grid`.
- **Dependency properties (21):** `ApplicationName`, `DocumentTitle`, `IsDocumentModified`, `ShowQuickAccess`, `ShowWindowButtons`, `IsMaximized`, `TitleForeground`, `TitleFontSize`, `TitleFontWeight`, `TitleAlignment`, `AppIcon`, `AppIconBrush`, `AppIconSize`, `ShowAppIcon`, `ShowSearchBox`, `SearchPlaceholder`, `UserAccountName`, `ShowUserAccount`, `DimOnInactive`, `IsWindowActive`, `Subtitle`
- **CLR events:** `MinimizeRequested`, `MaximizeRequested`, `CloseRequested`
- **Properties (1):** `QuickAccess: ObservableCollection<TitleBarAction>`
- **Methods (3):** `SetWindowActive(bool active)` · `RefreshQuickAccess()` · `ComposeTitle(string documentTitle, bool isModified, string applicationName)`

### ArrowDirection *(enum)*


Inherits `System.Enum`.
*No public members.*

### AuroraBackground


Inherits `System.Windows.FrameworkElement`.
- **Dependency properties (7):** `BlobCount`, `Speed`, `BlobOpacity`, `IsAnimated`, `AccentBrush`, `SecondaryBrush`, `TertiaryBrush`

### AutoCompleteTextBox


Inherits `System.Windows.Controls.TextBox`.
- **Dependency properties (6):** `ItemsSource`, `SelectedItem`, `ItemTemplate`, `DisplayMemberPath`, `MaxDropDownHeight`, `IsDropDownOpen`
- **Routed events:** `ItemSelected`
- **CLR events:** `ItemSelected`
- **Methods (4):** `OnApplyTemplate()` · `ClearValue()` · `GetItemText(object item)` · `FilterSuggestions(string query, bool openIfEmpty)`

### AutoHidePane


Inherits `System.Windows.FrameworkElement`.
- **Dependency properties (3):** `Panes`, `Side`, `TabSize`
- **CLR events:** `PaneInvoked`
- **Properties (1):** `Tabs: IReadOnlyList<ToolWindow>`

### AvatarStack


Inherits `System.Windows.FrameworkElement`.
- **Dependency properties (10):** `Items`, `MaxVisible`, `Size`, `Overlap`, `Background`, `Foreground`, `RingBrush`, `FontSize`, `ShowOverflow`, `OverflowTextFormat`

### Badge


Inherits `System.Windows.Controls.ContentControl`.
- **Dependency properties (15):** `Appearance`, `CornerRadius`, `HasBadge`, `Value`, `IsDot`, `ShowBadge`, `BadgeMargin`, `Status`, `DotSize`, `MaxValue`, `DisplayText`, `Offset`, `BadgeFontSize`, `BadgePadding`, `Pulse`

### BadgeAppearance *(enum)*


Inherits `System.Enum`.
*No public members.*

### BorderBeam


Inherits `System.Windows.Controls.Decorator`.
- **Dependency properties (8):** `CornerRadius`, `BeamBrush`, `BeamThickness`, `BeamLength`, `RevolutionSeconds`, `IsAnimated`, `SurfaceBrush`, `Padding`

### Breadcrumb


Inherits `System.Windows.Controls.Control`.
- **Dependency properties (2):** `Items`, `SeparatorIcon`

### BreadcrumbItem


Inherits `System.Windows.DependencyObject`.
- **Dependency properties (5):** `Header`, `Icon`, `IsSelected`, `Command`, `Tag`

### CadInputParser *(static class)*

- **Methods (3):** `TryParseLength(string text, Double& millimetres)` · `TryParseCoordinate(string text, Point3D& point)` · `TryParseAngle(string text, Double& degrees)`

### CadNotification

- **Properties (5):** `Title: string`, `Message: string`, `Severity: CadNotificationSeverity`, `Timestamp: DateTime`, `IsRead: bool`

### CadNotificationSeverity *(enum)*


Inherits `System.Enum`.
*No public members.*

### CadStatusBar


Inherits `System.Windows.Controls.Grid`.
- **Dependency properties (22):** `SelectionCount`, `WorkPlane`, `Coordinates`, `UnitSuffix`, `ShowCoordinates`, `SnapEnabled`, `GridEnabled`, `OrthoEnabled`, `PolarEnabled`, `ObjectSnapEnabled`, `Variant`, `CornerRadius`, `Elevation`, `ControlHeight`, `IconSize`, `ShowDividers`, `ShowSelection`, `ShowWorkPlane`, `ShowToggles`, `ShowTaskQueue`, `ShowNotifications`, `ShowToggleLabels`
- **CLR events:** `ToggleChanged`, `WorkPlaneChanged`, `TaskSelected`, `NotificationSelected`
- **Properties (2):** `Tasks: ObservableCollection<CadTask>`, `Notifications: ObservableCollection<CadNotification>`
- **Methods (10):** `SelectionSummary(int count)` · `FormatCoordinates(Point3D point, string unitSuffix)` · `IsToggleEnabled(CadToggle toggle)` · `SetToggle(CadToggle toggle, bool isEnabled)` · `AddTask(CadTask task)` · `RemoveTask(CadTask task)` · `Notify(CadNotification notification)` · `UpdateCoordinates(Point3D point)` · `RefreshTasks()` · `RefreshNotifications()`

### CadStatusBarVariant *(enum)*


Inherits `System.Enum`.
*No public members.*

### CadTask

- **Properties (4):** `Title: string`, `Percent: double`, `IsIndeterminate: bool`, `Tag: object`
- **Methods (1):** `ToString()`

### CadToggle *(enum)*


Inherits `System.Enum`.
*No public members.*

### CadToggleEventArgs


Inherits `System.EventArgs`.
- **Properties (2):** `Toggle: CadToggle`, `IsEnabled: bool`

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


Inherits `System.Enum`.
*No public members.*

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


Inherits `System.Windows.FrameworkElement`.
- **Dependency properties (6):** `IsChecked`, `Stroke`, `StrokeThickness`, `Duration`, `BackgroundBrush`, `ShowCircle`

### Clock


Inherits `System.Windows.Controls.Control`.
- **Dependency properties (4):** `SelectedTime`, `DisplayMode`, `ShowSeconds`, `AccentBrush`
- **Routed events:** `SelectedTimeChanged`
- **CLR events:** `SelectedTimeChanged`
- **Properties (1):** `FormattedTime: string`
- **Methods (2):** `OnApplyTemplate()` · `UpdateClockVisuals()`

### ClockDisplayMode *(enum)*


Inherits `System.Enum`.
*No public members.*

### CoachMark


Inherits `System.Windows.Controls.Control`.
- **Dependency properties (9):** `Target`, `Placement`, `Title`, `StepIndex`, `StepCount`, `CardWidth`, `Gap`, `OverlayBrush`, `CardRect`
- **CLR events:** `Next`, `Dismissed`
- **Properties (1):** `IsLastStep: bool`
- **Methods (4):** `Advance()` · `Dismiss()` · `OnApplyTemplate()` · `ComputeCardRect(Size host)`

### CoachMarkPlacement *(enum)*


Inherits `System.Enum`.
*No public members.*

### CodeEditor


Inherits `System.Windows.Controls.Control`.
- **Dependency properties (14):** `Text`, `Syntax`, `IsReadOnly`, `HighlightSyntax`, `ShowLineNumbers`, `KeywordBrush`, `StringBrush`, `CommentBrush`, `NumberBrush`, `TagBrush`, `AttributeBrush`, `TabSize`, `LineCount`, `CaretLine`
- **Methods (4):** `OnApplyTemplate()` · `GoToLine(int line)` · `FindNext(string term, bool matchCase)` · `Refresh()`

### CodeHighlighter *(static class)*

- **Methods (1):** `Tokenize(string text, CodeLanguage language)`

### CodeLanguage *(enum)*


Inherits `System.Enum`.
*No public members.*

### CodeToken *(struct)*


Inherits `System.ValueType`.
- **Properties (3):** `Start: int`, `Length: int`, `Kind: CodeTokenKind`

### CodeTokenKind *(enum)*


Inherits `System.Enum`.
*No public members.*

### ColorPicker


Inherits `System.Windows.Controls.Control`.
- **Dependency properties (9):** `SelectedColor`, `HexCode`, `ShowAlpha`, `CornerRadius`, `ShowHexInput`, `ShowPresets`, `PresetColumns`, `SwatchSize`, `FieldHeight`
- **Properties (1):** `PresetColors: ObservableCollection<Color>`
- **Methods (1):** `OnApplyTemplate()`

### ColumnChooserItem

- **CLR events:** `PropertyChanged`
- **Properties (3):** `Header: string`, `IsVisible: bool`, `Column: DataGridColumn`

### ColumnFilterItem

- **CLR events:** `PropertyChanged`
- **Properties (6):** `Header: string`, `FilterText: string`, `Placeholder: string`, `Width: double`, `Operator: FilterOperator`, `Column: DataGridColumn`

### ColumnGroup


Inherits `CONCOST.UI.Controls.ColumnNode`.
- **Properties (1):** `Children: ObservableCollection<ColumnNode>`
- **Methods (2):** `GetLeaves()` · `GetDepth()`

### ColumnGroupCandidate

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

- **CLR events:** `PropertyChanged`
- **Properties (13):** `Header: string`, `DisplayText: string`, `Sum: double`, `Average: double`, `Min: double`, `Max: double`, `Count: int`, `IsNumeric: bool`, `Mode: DataGridSummaryAggregate`, `Format: string`, `Width: double`, `DisplayIndex: int`, `Column: DataGridColumn`

### CommandBar


Inherits `CONCOST.UI.Controls.OverflowToolbar`.
- **Dependency properties (1):** `OverflowToolTip`

### CommandBarItem

- **Properties (8):** `Label: string`, `Icon: IconKind`, `Command: ICommand`, `CommandParameter: object`, `ToolTip: string`, `IsSeparator: bool`, `ShowLabel: bool`, `IsEnabled: bool`

### CommandDefinition

- **Properties (9):** `Name: string`, `Aliases: List<string>`, `Description: string`, `Prompt: string`, `Options: List<string>`, `PromptText: string`, `Tag: object`, `Keys: IEnumerable<string>`, `Display: string`
- **Methods (6):** `WithOptions(String[] options)` · `MatchOption(string text)` · `WithAliases(String[] aliases)` · `Matches(string text)` · `MatchesKey(string text)` · `ToString()`

### CommandInvokedEventArgs


Inherits `System.EventArgs`.
- **Properties (3):** `Definition: CommandDefinition`, `RawText: string`, `Arguments: IReadOnlyList<string>`

### CommandLine


Inherits `System.Windows.Controls.Grid`.
- **Dependency properties (11):** `Prompt`, `HistoryHeight`, `ShowHistory`, `MaxHistory`, `Variant`, `CornerRadius`, `Elevation`, `EchoPrompt`, `SpaceSubmits`, `IsHistoryExpanded`, `ExpandedHistoryHeight`
- **CLR events:** `CommandInvoked`, `UnknownCommand`, `PromptCompleted`, `PromptCancelled`
- **Properties (7):** `Commands: ObservableCollection<CommandDefinition>`, `Messages: ObservableCollection<CommandMessage>`, `Suggestions: IReadOnlyList<CommandDefinition>`, `PromptText: string`, `Text: string`, `IsPromptActive: bool`, `LastCommand: CommandDefinition`
- **Methods (14):** `ToggleHistory()` · `AddCommand(CommandDefinition definition)` · `AddCommands(IEnumerable<CommandDefinition> definitions)` · `Resolve(string rawText)` · `Execute(string command)` · `Submit()` · `BeginPrompt(string prompt)` · `BeginPrompt(string prompt, IEnumerable<string> options)` · `CancelPrompt()` · `Complete()` · `WriteLine(string text, CommandMessageKind kind)` · `Clear()` · `FocusInput()` · `MatchOption(IEnumerable<string> options, string text)`

### CommandLineVariant *(enum)*


Inherits `System.Enum`.
*No public members.*

### CommandMessage

- **Properties (3):** `Text: string`, `Kind: CommandMessageKind`, `Timestamp: DateTime`

### CommandMessageKind *(enum)*


Inherits `System.Enum`.
*No public members.*

### CommandPalette


Inherits `System.Windows.Controls.Control`.
- **Dependency properties (10):** `IsOpen`, `ItemsSource`, `FilterText`, `SelectedIndex`, `SelectedItem`, `HasResults`, `Placeholder`, `EmptyText`, `MaxListHeight`, `CloseOnInvoke`
- **CLR events:** `ItemInvoked`, `Opened`, `Closed`
- **Properties (1):** `FilteredItems: IReadOnlyList<CommandPaletteItem>`
- **Methods (8):** `Open()` · `Close()` · `Toggle()` · `Refresh()` · `MoveSelection(int delta)` · `ExecuteSelected()` · `InvokeItem(CommandPaletteItem item)` · `OnApplyTemplate()`

### CommandPaletteInvokedEventArgs


Inherits `System.EventArgs`.
- **Properties (1):** `Item: CommandPaletteItem`

### CommandPaletteItem

- **Properties (12):** `Title: string`, `Subtitle: string`, `Shortcut: string`, `Category: string`, `Icon: IconKind`, `Command: ICommand`, `CommandParameter: object`, `Keywords: string`, `Tag: object`, `IsEnabled: bool`, `CanInvoke: bool`, `SearchText: string`
- **Methods (2):** `Invoke()` · `ToString()`

### CommandPaletteText *(static class)*

- **Methods (3):** `Normalize(string value)` · `Tokenize(string query)` · `Matches(CommandPaletteItem item, IReadOnlyList<string> tokens)`

### CommandPromptEventArgs


Inherits `System.EventArgs`.
- **Properties (3):** `Prompt: string`, `Text: string`, `Option: string`

### CompareSlider


Inherits `System.Windows.Controls.Control`.
- **Dependency properties (5):** `SourceContent`, `TargetContent`, `Value`, `Orientation`, `HandleSize`
- **Methods (1):** `OnApplyTemplate()`

### ComparisonOperator *(enum)*


Inherits `System.Enum`.
*No public members.*

### CompassIndicator


Inherits `System.Windows.Controls.Control`.
- **Dependency properties (13):** `Angle`, `ShowCardinals`, `CardinalBrush`, `NorthBrush`, `DialBrush`, `DialBorderBrush`, `DialBorderThickness`, `DialInset`, `NeedleThickness`, `CardinalFontSize`, `CenterMarkerBrush`, `CenterMarkerSize`, `ShowCenterMarker`

### ConditionalFormattingRule


Inherits `System.Windows.DependencyObject`.
- **Dependency properties (5):** `Operator`, `Value1`, `Value2`, `Background`, `Foreground`
- **Methods (1):** `Evaluate(double val)`

### ConfettiBurst


Inherits `System.Windows.FrameworkElement`.
- **Dependency properties (9):** `IsBursting`, `ParticleCount`, `Duration`, `Gravity`, `Spread`, `Origin`, `Palette`, `MinParticleSize`, `MaxParticleSize`

### CursorGlow


Inherits `CONCOST.UI.Controls.AnimatedEffectBase`.
- **Dependency properties (4):** `GlowSize`, `GlowBrush`, `GlowOpacity`, `FollowSpeed`

### CursorSpotlight


Inherits `CONCOST.UI.Controls.AnimatedEffectBase`.
- **Dependency properties (7):** `SpotDim`, `Radius`, `Feather`, `DimColor`, `SpotColor`, `FollowCursor`, `SpotlightCenter`

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


Inherits `System.Windows.Controls.Control`.
- **Dependency properties (5):** `TargetGrid`, `IsOpen`, `Columns`, `HeaderText`, `CornerRadius`
- **CLR events:** `ColumnsChanged`
- **Methods (3):** `RefreshColumns()` · `ShowAll()` · `HideAll()`

### DataGridContextMenuHelper *(static class)*

- **Methods (1):** `CreateDefaultContextMenu(StackedHeaderDataGrid grid)`

### DataGridFilterHelper *(static class)*

- **Methods (1):** `ShowFilterPopup(StackedHeaderDataGrid grid, ColumnLeaf leaf, FrameworkElement targetPlacement)`

### DataGridFilterPanel


Inherits `System.Windows.Controls.Control`.
- **Dependency properties (4):** `TargetGrid`, `Filters`, `PlaceholderText`, `ShowOperatorSelector`
- **CLR events:** `FiltersApplied`
- **Methods (4):** `OnApplyTemplate()` · `RefreshFilters()` · `ApplyFilters()` · `ClearFilters()`

### DataGridGroupPanel


Inherits `System.Windows.Controls.Control`.
- **Dependency properties (7):** `TargetGrid`, `Groups`, `AvailableColumns`, `SelectedColumn`, `HeaderText`, `EmptyText`, `CornerRadius`
- **CLR events:** `GroupsChanged`
- **Methods (4):** `RefreshAvailableColumns()` · `AddGroup(string propertyPath, string header)` · `RemoveGroup(GroupDescriptor descriptor)` · `ClearGroups()`

### DataGridPerformance *(static class)*

- **Dependency properties (2):** `DeferredScrolling`, `CachePages`
- **Attached properties:** `CachePages`, `DeferredScrolling`
- **Methods (6):** `GetDeferredScrolling(DataGrid grid)` · `SetDeferredScrolling(DataGrid grid, bool value)` · `GetCachePages(DataGrid grid)` · `SetCachePages(DataGrid grid, double value)` · `ScrollToItem(DataGrid grid, object item)` · `ScrollToIndex(DataGrid grid, int index)`

### DataGridSearchPanel


Inherits `System.Windows.Controls.Control`.
- **Dependency properties (4):** `TargetGrid`, `SearchText`, `PlaceholderText`, `IsSearching`
- **CLR events:** `SearchChanged`
- **Methods (2):** `ClearSearch()` · `ApplySearch()`

### DataGridSummary


Inherits `System.Windows.Controls.Control`.
- **Dependency properties (6):** `TargetGrid`, `Summaries`, `Aggregate`, `DisplayFormat`, `RowHeight`, `AggregateOnColumn`
- **Attached properties:** `AggregateOnColumn`
- **Methods (4):** `SetAggregateOnColumn(DataGridColumn column, DataGridSummaryAggregate value)` · `GetAggregateOnColumn(DataGridColumn column)` · `OnApplyTemplate()` · `RefreshSummaries()`

### DataGridSummaryAggregate *(enum)*


Inherits `System.Enum`.
*No public members.*

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
- **Methods (8):** `SetToken(DependencyObject element, string value)` · `GetToken(DependencyObject element)` · `RegisterGlobalHost(Panel panel)` · `Show(object content, string title, string token)` · `ShowConfirmAsync(string message, string title, string token, string confirmText, string cancelText)` · `ShowInputAsync(string message, string defaultText, string title, string token, string okText, string cancelText)` · `Close(string token)` · `OnApplyTemplate()`

### DialogMessageType *(enum)*


Inherits `System.Enum`.
*No public members.*

### Divider


Inherits `System.Windows.Controls.Control`.
- **Dependency properties (11):** `Text`, `Orientation`, `Icon`, `LineStyle`, `TextAlignment`, `LineThickness`, `LineBrush`, `TextFontSize`, `TextForeground`, `TextPadding`, `Spacing`

### DividerLineStyle *(enum)*


Inherits `System.Enum`.
*No public members.*

### DockGroupMode *(enum)*


Inherits `System.Enum`.
*No public members.*

### DockHeaderButtons *(enum)*


Inherits `System.Enum`.
*No public members.*

### DockPaneGroup


Inherits `System.Windows.Controls.Panel`.
- **Dependency properties (14):** `Orientation`, `Mode`, `Dock`, `DockSize`, `MinDockSize`, `Proportion`, `SplitterThickness`, `TabStripThickness`, `TabPlacement`, `SplitterBrush`, `Header`, `Icon`, `HeaderHeight`, `ShowHeader`
- **Attached properties:** `Proportion`
- **CLR events:** `PaneDragStarted`, `PaneDragMoved`, `PaneDragCompleted`, `PaneClosed`, `PaneAutoHideChanged`
- **Properties (2):** `Nodes: IEnumerable<UIElement>`, `ActivePane: ToolWindow`
- **Methods (3):** `GetProportion(DependencyObject element)` · `SetProportion(DependencyObject element, double value)` · `MoveChild(ToolWindow pane, int index)`

### DockPreview


Inherits `System.Windows.FrameworkElement`.
- **Dependency properties (8):** `IsOpen`, `Side`, `PreviewBrush`, `BorderBrush`, `HighlightedSide`, `CompassCenter`, `TargetRect`, `IsOuterTarget`

### DockSide *(enum)*


Inherits `System.Enum`.
*No public members.*

### DockSplitter


Inherits `System.Windows.FrameworkElement`.
- **Dependency properties (6):** `Orientation`, `Thickness`, `Value`, `Minimum`, `Maximum`, `LineBrush`
- **CLR events:** `DragCompleted`

### DockTabPlacement *(enum)*


Inherits `System.Enum`.
*No public members.*

### DockTabStrip


Inherits `System.Windows.Controls.Control`.
- **Dependency properties (4):** `TabPlacement`, `TabHeight`, `IconSize`, `Panes`
- **CLR events:** `TabSelected`
- **Methods (1):** `OnApplyTemplate()`

### DockingManager


Inherits `System.Windows.Controls.Panel`.
- **Dependency properties (13):** `SplitterThickness`, `AutoHideStripSize`, `ShowDropPreview`, `SplitterBrush`, `IsProportional`, `Variant`, `CornerRadius`, `Elevation`, `HeaderHeight`, `IconSize`, `HeaderButtons`, `ActiveAccent`, `TabPlacement`
- **CLR events:** `PaneDocked`, `PaneClosed`
- **Properties (3):** `Panes: IEnumerable<ToolWindow>`, `IsPreviewOpen: bool`, `AllPanes: IEnumerable<ToolWindow>`
- **Methods (13):** `Group(DockSide side)` · `Dock(ToolWindow pane, DockSide side)` · `ClosePane(ToolWindow pane)` · `MovePane(ToolWindow pane, int index)` · `ToggleFlyout(ToolWindow pane)` · `CloseFlyouts()` · `Float(ToolWindow pane)` · `IsFloating(ToolWindow pane)` · `CaptureLayout()` · `ApplyLayout(string layout)` · `DockAt(ToolWindow pane, Point point)` · `ShowDropHint(ToolWindow pane, Point point)` · `EndDropPreview()`

### DockingManagerAutomationPeer


Inherits `System.Windows.Automation.Peers.FrameworkElementAutomationPeer`.
*No public members.*

### DockingManagerVariant *(enum)*


Inherits `System.Enum`.
*No public members.*

### DocumentClosingEventArgs


Inherits `System.EventArgs`.
- **Properties (2):** `Pane: DocumentPane`, `Cancel: bool`

### DocumentPane

- **Properties (6):** `Title: string`, `Content: object`, `Icon: IconKind`, `IsDirty: bool`, `Tag: object`, `FloatingWindow: Window`

### DocumentTabControl


Inherits `System.Windows.Controls.TabControl`.
- **Dependency properties (1):** `ShowCloseButtons`
- **Properties (1):** `Documents: IEnumerable<DocumentTabItem>`
- **Methods (3):** `CloseTab(object item)` · `CloseOtherTabs(object keep)` · `CloseAllTabs()`

### DocumentTabItem


Inherits `System.Windows.Controls.TabItem`.
- **Dependency properties (4):** `Icon`, `IsDirty`, `CanClose`, `CloseCommand`
- **Routed events:** `CloseRequested`
- **CLR events:** `CloseRequested`
- **Methods (2):** `Close()` · `OnApplyTemplate()`

### DocumentTileMode *(enum)*


Inherits `System.Enum`.
*No public members.*

### DocumentWorkspace


Inherits `System.Windows.Controls.Grid`.
- **CLR events:** `DocumentClosing`, `DocumentAdded`, `DocumentClosed`, `ActiveDocumentChanged`, `DocumentFloated`
- **Properties (6):** `Documents: ObservableCollection<DocumentPane>`, `ActiveDocument: DocumentPane`, `TileMode: DocumentTileMode`, `PlaceholderTitle: string`, `PlaceholderText: string`, `PaneCount: int`
- **Methods (6):** `AddDocument(string title, object content, IconKind icon)` · `AddDocument(DocumentPane pane)` · `CloseDocument(DocumentPane pane)` · `CloseOthers(DocumentPane keep)` · `CloseAll()` · `FloatDocument(DocumentPane pane)`

### DragGhost *(static class)*

- **Properties (1):** `IsVisible: bool`
- **Methods (3):** `Show(string caption, IconKind icon)` · `Move(Visual reference, Point point)` · `Hide()`

### Drawer


Inherits `System.Windows.Controls.ContentControl`.
- **Dependency properties (5):** `IsOpen`, `Placement`, `DrawerWidth`, `DrawerHeight`, `Title`
- **Methods (1):** `OnApplyTemplate()`

### DrawerMenu


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

- **Dependency properties (3):** `IsEnabled`, `Brush`, `CornerRadius`
- **Attached properties:** `Brush`, `CornerRadius`, `IsEnabled`
- **Methods (6):** `GetIsEnabled(DependencyObject element)` · `SetIsEnabled(DependencyObject element, bool value)` · `GetBrush(DependencyObject element)` · `SetBrush(DependencyObject element, Brush value)` · `GetCornerRadius(DependencyObject element)` · `SetCornerRadius(DependencyObject element, CornerRadius value)`

### DynamicInput


Inherits `System.Windows.Controls.Grid`.
- **Dependency properties (17):** `Prompt`, `Text`, `Anchor`, `Offset`, `Kind`, `IsValid`, `ValidationMessage`, `IsOpen`, `Variant`, `Placement`, `CornerRadius`, `Elevation`, `UnitSuffix`, `ShowUnit`, `ShowLeader`, `FieldWidth`, `SelectAllOnOpen`
- **CLR events:** `ValueCommitted`, `Cancelled`
- **Methods (5):** `Open(Point anchor, string prompt, string prefill)` · `MoveTo(Point anchor)` · `Commit()` · `Cancel()` · `TryParse(DynamicInputKind kind, string text, Double& number, Point3D& point)`

### DynamicInputKind *(enum)*


Inherits `System.Enum`.
*No public members.*

### DynamicInputPlacement *(enum)*


Inherits `System.Enum`.
*No public members.*

### DynamicInputValueEventArgs


Inherits `System.EventArgs`.
- **Properties (4):** `Kind: DynamicInputKind`, `Text: string`, `Number: double`, `Point: Nullable<Point3D>`

### DynamicInputVariant *(enum)*


Inherits `System.Enum`.
*No public members.*

### EmptyState


Inherits `System.Windows.Controls.Control`.
- **Dependency properties (17):** `Icon`, `Title`, `Description`, `ActionButtonText`, `ActionButtonIcon`, `ActionCommand`, `SecondaryActionText`, `ExtraContent`, `SecondaryActionCommand`, `SecondaryActionIcon`, `IconSize`, `IconBrush`, `TitleFontSize`, `DescriptionFontSize`, `Orientation`, `ImageSource`, `HasActions`

### Expander


Inherits `System.Windows.Controls.HeaderedContentControl`.
- **Dependency properties (11):** `IsExpanded`, `ExpandDirection`, `CornerRadius`, `AnimationDuration`, `IsAnimated`, `HeaderBackground`, `HeaderForeground`, `HeaderPadding`, `ContentPadding`, `ShowToggleIcon`, `ToggleIconSize`
- **Routed events:** `Expanded`, `Collapsed`
- **CLR events:** `Expanded`, `Collapsed`
- **Methods (1):** `OnApplyTemplate()`

### FileDropEventArgs


Inherits `System.EventArgs`.
- **Properties (1):** `Files: IReadOnlyList<string>`

### FileDropZone


Inherits `System.Windows.Controls.Control`.
- **Dependency properties (6):** `Filter`, `AllowMultiple`, `Caption`, `Hint`, `IsDragOver`, `IsDragRejected`
- **CLR events:** `FilesDropped`, `BrowseRequested`
- **Properties (1):** `Files: ObservableCollection<string>`
- **Methods (3):** `AddFiles(IEnumerable<string> paths)` · `Clear()` · `Matches(string path)`

### FilePicker


Inherits `System.Windows.Controls.Control`.
- **Dependency properties (3):** `SelectedPath`, `Filter`, `Title`
- **Methods (1):** `OnApplyTemplate()`

### FilterOperator *(enum)*


Inherits `System.Enum`.
*No public members.*

### FontPicker


Inherits `System.Windows.Controls.Grid`.
- **Dependency properties (8):** `SelectedFontFamily`, `SelectedFontSize`, `IsBold`, `IsItalic`, `ShowPreview`, `PreviewText`, `RowHeight`, `PreviewMaxHeight`
- **CLR events:** `SelectionChanged`
- **Properties (2):** `EffectiveWeight: FontWeight`, `EffectiveStyle: FontStyle`

### FrostedGlass


Inherits `System.Windows.Controls.ContentControl`.
- **Dependency properties (5):** `Source`, `BlurRadius`, `Tint`, `Corner`, `EdgeBrush`
- **Methods (1):** `OnApplyTemplate()`

### GradientText


Inherits `System.Windows.FrameworkElement`.
- **Dependency properties (8):** `Text`, `FontSize`, `FontWeight`, `StartColor`, `EndColor`, `Angle`, `Speed`, `IsAnimated`

### GridPattern


Inherits `System.Windows.FrameworkElement`.
- **Dependency properties (8):** `CellSize`, `Pattern`, `DotRadius`, `StrokeThickness`, `Stroke`, `OffsetX`, `OffsetY`, `PatternOpacity`

### GridPatternType *(enum)*


Inherits `System.Enum`.
*No public members.*

### GroupDescriptor

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


Inherits `System.Windows.FrameworkElement`.
- **Dependency properties (13):** `Values`, `StartDate`, `CellSize`, `CellGap`, `CornerRadius`, `BaseBrush`, `Level1Brush`, `Level2Brush`, `Level3Brush`, `Level4Brush`, `ShowMonthLabels`, `MonthLabelBrush`, `MonthLabelFontSize`

### HideIsolateAction *(enum)*


Inherits `System.Enum`.
*No public members.*

### HideIsolateEventArgs


Inherits `System.EventArgs`.
- **Properties (1):** `Action: HideIsolateAction`

### ImageViewer


Inherits `System.Windows.FrameworkElement`.
- **Dependency properties (10):** `Source`, `Zoom`, `Rotation`, `Offset`, `MinZoom`, `MaxZoom`, `ShowCheckerboard`, `CheckerboardBrush`, `CheckerCellSize`, `ZoomStep`
- **Properties (1):** `HasImage: bool`
- **Methods (6):** `GetImageBounds()` · `Fit()` · `Reset()` · `RotateClockwise()` · `ZoomAt(Point anchor, double factor)` · `Centre()`

### ImageViewerAutomationPeer


Inherits `System.Windows.Automation.Peers.FrameworkElementAutomationPeer`.
*No public members.*

### KanbanBoard


Inherits `System.Windows.Controls.Control`.
- **Dependency properties (1):** `Columns`
- **CLR events:** `CardMoved`
- **Methods (2):** `MoveCard(KanbanCard card, KanbanColumn target, int index)` · `ColumnOf(KanbanCard card)`

### KanbanCard


Inherits `System.Windows.Controls.ContentControl`.
- **Dependency properties (4):** `Title`, `Chip`, `Accent`, `IsDone`

### KanbanCardMovedEventArgs


Inherits `System.EventArgs`.
- **Properties (5):** `Card: KanbanCard`, `FromColumn: KanbanColumn`, `ToColumn: KanbanColumn`, `FromIndex: int`, `ToIndex: int`

### KanbanColumn


Inherits `System.Windows.Controls.HeaderedItemsControl`.
- **Dependency properties (3):** `Accent`, `IsDropTarget`, `Maximum`
- **Properties (1):** `Count: int`

### LoadingBase


Inherits `System.Windows.Controls.Control`.
- **Dependency properties (1):** `IsLoading`

### LoadingCircle


Inherits `CONCOST.UI.Controls.LoadingBase`.
- **Dependency properties (1):** `DotSize`

### LoadingIndicator


Inherits `CONCOST.UI.Controls.AnimatedEffectBase`.
- **Dependency properties (3):** `IndicatorType`, `DotSize`, `Foreground`
- **Properties (1):** `Phase: double`

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

- **Dependency properties (3):** `IsEnabled`, `Strength`, `MaxOffset`
- **Attached properties:** `IsEnabled`, `MaxOffset`, `Strength`
- **Methods (6):** `SetIsEnabled(DependencyObject element, bool value)` · `GetIsEnabled(DependencyObject element)` · `SetStrength(DependencyObject element, double value)` · `GetStrength(DependencyObject element)` · `SetMaxOffset(DependencyObject element, double value)` · `GetMaxOffset(DependencyObject element)`

### Magnifier


Inherits `System.Windows.Controls.Control`.
- **Dependency properties (10):** `Target`, `Scale`, `Radius`, `ShowCrosshair`, `ShowRim`, `ShowShadow`, `FollowPointer`, `Center`, `IsActive`, `CrosshairBrush`

### MagnifierAutomationPeer


Inherits `System.Windows.Automation.Peers.FrameworkElementAutomationPeer`.
*No public members.*

### MarkdownParser *(static class)*

- **Methods (4):** `Parse(string markdown)` · `Parse(string markdown, MarkdownRenderTheme theme)` · `ParseInlines(string text)` · `ParseInlines(string text, MarkdownRenderTheme palette)`

### MarkdownRenderTheme

- **Properties (6):** `RuleBrush: Brush`, `QuoteBrush: Brush`, `CodeBorderBrush: Brush`, `CodeBackgroundBrush: Brush`, `InlineCodeBackgroundBrush: Brush`, `HyperlinkBrush: Brush`
- **Methods (1):** `FromResources()`

### MarkdownViewer


Inherits `System.Windows.Controls.Control`.
- **Dependency properties (4):** `Markdown`, `RenderTheme`, `Document`, `LinkCount`
- **Methods (1):** `Rebuild()`

### MarqueeTicker


Inherits `System.Windows.FrameworkElement`.
- **Dependency properties (7):** `Text`, `Speed`, `Gap`, `FontSize`, `Foreground`, `Separator`, `IsRunning`

### MasonryPanel


Inherits `System.Windows.Controls.Panel`.
- **Dependency properties (5):** `Columns`, `MinColumnWidth`, `ColumnSpacing`, `RowSpacing`, `IsBalanced`
- **Methods (2):** `GetColumnCount(double availableWidth)` · `GetColumnWidth(double availableWidth)`

### MeshEdge *(struct)*


Inherits `System.ValueType`.
- **Properties (2):** `A: int`, `B: int`

### MiniMap


Inherits `System.Windows.FrameworkElement`.
- **Dependency properties (8):** `ContentBounds`, `Viewport`, `ItemsSource`, `ItemBrush`, `ViewportBrush`, `ViewportFillOpacity`, `CornerRadius`, `ShowBorder`
- **CLR events:** `ViewportChanged`
- **Properties (1):** `Scale: double`
- **Methods (4):** `WorldToMap(Point world)` · `MapToWorld(Point point)` · `CentreOn(Point world)` · `Fit()`

### MiniMapItem

- **Properties (2):** `Bounds: Rect`, `Brush: Brush`

### MiniMapViewportChangedEventArgs


Inherits `System.EventArgs`.
- **Properties (2):** `Viewport: Rect`, `IsFinal: bool`

### ModelViewer3D


Inherits `System.Windows.FrameworkElement`.
- **Dependency properties (9):** `Vertices`, `Edges`, `Yaw`, `Pitch`, `Zoom`, `Pan`, `ShowAxes`, `EdgeBrush`, `Background`
- **Methods (5):** `SetMesh(IEnumerable<Point3D> vertices, IEnumerable<MeshEdge> edges)` · `LoadBox(Point3D corner, double width, double depth, double height)` · `ResetView()` · `Orbit(double deltaX, double deltaY)` · `Project(Point3D point, Point& screen)`

### ModelViewer3DAutomationPeer


Inherits `System.Windows.Automation.Peers.FrameworkElementAutomationPeer`.
*No public members.*

### MultiSelectDataGrid


Inherits `System.Windows.Controls.DataGrid`.
- **Dependency properties (4):** `ShowCheckBoxColumn`, `CheckBoxColumnWidth`, `IsAllSelected`, `SelectedCount`
- **Properties (1):** `SelectedItemsList: IList`
- **Methods (5):** `SelectEverything()` · `ClearEverything()` · `InvertSelection()` · `ToggleItem(object item)` · `IsItemSelected(object item)`

### NavigationBar


Inherits `System.Windows.Controls.Grid`.
- **Dependency properties (13):** `ActiveTool`, `Ucs`, `ShowWalk`, `Variant`, `Orientation`, `ButtonSize`, `IconSize`, `CornerRadius`, `Elevation`, `ShowUcsLabel`, `ShowZoom`, `ShowPerspective`, `ShowDividers`
- **CLR events:** `NavigationRequested`, `ActiveToolChanged`, `UcsChanged`
- **Methods (1):** `SetActiveTool(NavigationTool tool)`

### NavigationBarVariant *(enum)*


Inherits `System.Enum`.
*No public members.*

### NavigationCommand *(enum)*


Inherits `System.Enum`.
*No public members.*

### NavigationRequestedEventArgs


Inherits `System.EventArgs`.
- **Properties (1):** `Command: NavigationCommand`

### NavigationTool *(enum)*


Inherits `System.Enum`.
*No public members.*

### NumericThresholdToBrushConverter

- **Properties (4):** `Threshold: double`, `BelowBrush: Brush`, `AboveBrush: Brush`, `DefaultBrush: Brush`
- **Methods (2):** `Convert(object value, Type targetType, object parameter, CultureInfo culture)` · `ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)`

### NumericUpDown


Inherits `System.Windows.Controls.Control`.
- **Dependency properties (18):** `Value`, `Minimum`, `Maximum`, `Increment`, `DecimalPlaces`, `Suffix`, `Prefix`, `FormatString`, `CanIncrease`, `CanDecrease`, `LargeIncrement`, `IsReadOnly`, `ShowButtons`, `ButtonPlacement`, `CornerRadius`, `TextAlignment`, `NullValue`, `IsNullable`
- **Routed events:** `ValueChanged`
- **CLR events:** `ValueChanged`
- **Methods (1):** `OnApplyTemplate()`

### OdometerNumber


Inherits `System.Windows.Controls.Control`.
- **Dependency properties (8):** `DisplayText`, `Value`, `Format`, `Prefix`, `Suffix`, `Duration`, `AnimateOnLoad`, `DecimalPlaces`

### OtpInput


Inherits `System.Windows.Controls.Control`.
- **Dependency properties (9):** `Value`, `Length`, `NumericOnly`, `MaskCharacter`, `ItemWidth`, `ItemHeight`, `ItemSpacing`, `ItemCornerRadius`, `IsCompleted`
- **Routed events:** `Completed`
- **CLR events:** `Completed`
- **Methods (2):** `Clear()` · `OnApplyTemplate()`

### OverflowPanel


Inherits `System.Windows.Controls.Panel`.
- **Dependency properties (2):** `OverflowCount`, `Spacing`
- **Properties (1):** `VisibleCount: int`

### OverflowToolbar *(abstract)*


Inherits `System.Windows.Controls.Control`.
- **Dependency properties (5):** `ItemsSource`, `Spacing`, `IsOverflowOpen`, `HasOverflow`, `OverflowItems`
- **Methods (3):** `OnApplyTemplate()` · `Invoke(object item)` · `SyncOverflow()`

### PageTransition *(enum)*


Inherits `System.Enum`.
*No public members.*

### PageTransitionHost


Inherits `System.Windows.Controls.ContentControl`.
- **Dependency properties (4):** `Transition`, `Duration`, `IsTransitionEnabled`, `Offset`
- **Properties (1):** `IsTransitioning: bool`
- **Methods (1):** `Play()`

### Pagination


Inherits `System.Windows.Controls.Control`.
- **Dependency properties (4):** `CurrentPage`, `MaxPage`, `TotalItems`, `PageSize`
- **Routed events:** `PageChanged`
- **CLR events:** `PageChanged`
- **Properties (1):** `TotalPages: int`
- **Methods (1):** `OnApplyTemplate()`

### ParallaxPanel


Inherits `System.Windows.Controls.ContentControl`.
- **Dependency properties (4):** `Target`, `Speed`, `MaxOffset`, `Axis`

### ParticleField


Inherits `CONCOST.UI.Controls.AnimatedEffectBase`.
- **Dependency properties (10):** `ParticleCount`, `MinRadius`, `MaxRadius`, `Speed`, `Direction`, `Spread`, `ParticleBrush`, `ParticleOpacity`, `ConnectDistance`, `Seed`
- **Properties (1):** `ActiveParticleCount: int`

### PasswordInput


Inherits `System.Windows.Controls.Control`.
- **Dependency properties (4):** `Password`, `Placeholder`, `ShowPassword`, `IconKind`
- **Methods (1):** `OnApplyTemplate()`

### PickerSelectionMode *(enum)*


Inherits `System.Enum`.
*No public members.*

### PinBox


Inherits `System.Windows.Controls.Control`.
- **Dependency properties (8):** `Length`, `Password`, `PasswordChar`, `IsPassword`, `ItemWidth`, `ItemHeight`, `ItemMargin`, `ItemCornerRadius`
- **Methods (1):** `OnApplyTemplate()`

### ProjectBrowser


Inherits `System.Windows.Controls.Grid`.
- **Dependency properties (18):** `Variant`, `Depth`, `TreeStyle`, `CornerRadius`, `Elevation`, `ControlHeight`, `IconSize`, `ShowSearch`, `ShowKindFilter`, `ShowSort`, `ShowGroup`, `ShowToolbar`, `ShowCount`, `SearchPlaceholder`, `AllKindsCaption`, `SortCaptions`, `EmptyText`, `CountCaption`
- **Attached properties:** `Depth`
- **CLR events:** `OpenRequested`, `RenameRequested`, `DeleteRequested`, `DuplicateRequested`, `PropertiesRequested`, `SelectionChanged`
- **Properties (7):** `Nodes: ObservableCollection<ProjectTreeNode>`, `FilterText: string`, `SelectedNode: ProjectTreeNode`, `GroupMode: ProjectGroupMode`, `SortMode: ProjectSortMode`, `VisibleNodes: IReadOnlyList<ProjectTreeNode>`, `KindFilter: Nullable<ProjectNodeKind>`
- **Methods (10):** `GetDepth(DependencyObject element)` · `SetDepth(DependencyObject element, int value)` · `AddRoot(string name, ProjectNodeKind kind)` · `SetSearchText(string text)` · `Refresh()` · `ExpandAll()` · `ScrollToTop()` · `CollapseAll()` · `Select(ProjectTreeNode node)` · `Open(ProjectTreeNode node)`

### ProjectBrowserTreeStyle *(enum)*


Inherits `System.Enum`.
*No public members.*

### ProjectBrowserVariant *(enum)*


Inherits `System.Enum`.
*No public members.*

### ProjectGroupMode *(enum)*


Inherits `System.Enum`.
*No public members.*

### ProjectNodeEventArgs


Inherits `System.EventArgs`.
- **Properties (1):** `Node: ProjectTreeNode`

### ProjectNodeKind *(enum)*


Inherits `System.Enum`.
*No public members.*

### ProjectSortMode *(enum)*


Inherits `System.Enum`.
*No public members.*

### ProjectTreeNode

- **Properties (8):** `Name: string`, `Kind: ProjectNodeKind`, `IconSource: ImageSource`, `IsModified: bool`, `IsActive: bool`, `Tag: object`, `Children: ObservableCollection<ProjectTreeNode>`, `Parent: ProjectTreeNode`
- **Methods (4):** `Add(ProjectTreeNode child)` · `Add(string name, ProjectNodeKind kind)` · `Flatten()` · `ToString()`

### ProjectionModeEventArgs


Inherits `System.EventArgs`.
- **Properties (1):** `Mode: ProjectionMode`

### PropertiesPalette


Inherits `System.Windows.Controls.Grid`.
- **Dependency properties (9):** `EmptyTitle`, `EmptySubtitle`, `EmptyHint`, `SearchPlaceholder`, `EditTypeText`, `TypeToolTip`, `ResetToolTip`, `ShowSearch`, `RevitLayout`
- **CLR events:** `EditTypeRequested`, `TypeChangeRequested`, `PropertyCommitted`
- **Properties (5):** `GridControl: PropertyGrid`, `Properties: PropertyManager`, `Selection: SelectionManager`, `Target: object`, `ValidationMessage: string`
- **Methods (3):** `Build(object target)` · `Refresh()` · `ResetProperty(string name, String& error)`

### PropertyCategoryGroup

- **CLR events:** `PropertyChanged`
- **Properties (3):** `CategoryName: string`, `IsExpanded: bool`, `Properties: ObservableCollection<PropertyItem>`

### PropertyEditorType *(enum)*


Inherits `System.Enum`.
*No public members.*

### PropertyGrid


Inherits `System.Windows.Controls.Control`.
- **Dependency properties (3):** `SelectedObject`, `Categories`, `ShowToolbar`
- **Methods (2):** `OnApplyTemplate()` · `BuildProperties()`

### PropertyItem

- **CLR events:** `PropertyChanged`
- **Properties (19):** `Name: string`, `DisplayName: string`, `Category: string`, `Description: string`, `PropertyType: Type`, `EditorType: PropertyEditorType`, `EnumOptions: Array`, `IsReadOnly: bool`, `ValidationError: string`, `Value: object`, `IsBoolTrue: bool`, `IsAlignLeft: bool`, `IsAlignCenter: bool`, `IsAlignRight: bool`, `IsAlignStretch: bool`, `IsValignTop: bool`, `IsValignCenter: bool`, `IsValignBottom: bool`, `IsValignStretch: bool`

### PropertyReferencePicker


Inherits `System.Windows.Controls.Grid`.
- **Dependency properties (4):** `SelectedItem`, `Placeholder`, `IsReadOnly`, `SearchPlaceholder`
- **CLR events:** `ReferencePicked`
- **Properties (2):** `ItemsSource: IEnumerable`, `DisplaySelector: Func<object, string>`
- **Methods (1):** `TogglePopup()`

### PulseRing


Inherits `System.Windows.FrameworkElement`.
- **Dependency properties (9):** `RingCount`, `Duration`, `MaxRadius`, `RingThickness`, `RingBrush`, `PeakOpacity`, `IsActive`, `CenterDot`, `DotRadius`

### QuickAccessItem

- **Properties (6):** `Icon: IconKind`, `ToolTip: string`, `Command: ICommand`, `CommandParameter: object`, `IsEnabled: bool`, `IsSeparator: bool`

### QuickAccessToolbar


Inherits `CONCOST.UI.Controls.OverflowToolbar`.
- **Dependency properties (6):** `IsCustomizable`, `ButtonSize`, `IconSize`, `ShowSeparators`, `Variant`, `OverflowToolTip`

### QuickAccessToolbarVariant *(enum)*


Inherits `System.Enum`.
*No public members.*

### RadialGauge


Inherits `System.Windows.FrameworkElement`.
- **Dependency properties (17):** `Minimum`, `Maximum`, `Value`, `StartAngle`, `SweepAngle`, `TrackThickness`, `TrackBrush`, `ValueBrush`, `ShowTicks`, `TickCount`, `ShowValue`, `ValueFormat`, `Suffix`, `Label`, `ValueFontSize`, `IsAnimated`, `AnimationDuration`
- **Properties (1):** `NormalizedValue: double`

### RangeSlider


Inherits `System.Windows.Controls.Control`.
- **Dependency properties (17):** `Minimum`, `Maximum`, `StartValue`, `EndValue`, `SmallChange`, `Unit`, `IsSnapToTick`, `TickFrequency`, `ShowTicks`, `ShowValueLabels`, `LabelFormat`, `TrackHeight`, `ThumbSize`, `ActiveTrackBrush`, `InactiveTrackBrush`, `ThumbBrush`, `Orientation`
- **Routed events:** `ValueChanged`
- **CLR events:** `ValueChanged`
- **Methods (1):** `OnApplyTemplate()`

### Rating


Inherits `System.Windows.Controls.Control`.
- **Dependency properties (14):** `Value`, `Max`, `ItemSize`, `ItemMargin`, `ItemGap`, `IsReadOnly`, `AllowHalf`, `AllowClear`, `ShowScore`, `ScoreFormat`, `ActiveBrush`, `InactiveBrush`, `IconGeometry`, `HoverValue`
- **Routed events:** `ValueChanged`
- **CLR events:** `ValueChanged`
- **Methods (1):** `OnApplyTemplate()`

### RatingToStarsConverter

- **Methods (2):** `Convert(object value, Type targetType, object parameter, CultureInfo culture)` · `ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)`

### ReferencePickedEventArgs


Inherits `System.EventArgs`.
- **Properties (1):** `Item: object`

### RegionHoverEventArgs


Inherits `System.EventArgs`.
- **Properties (2):** `Hit: ViewCubeHit`, `Region: ViewCubeRegion`

### RingLoading


Inherits `CONCOST.UI.Controls.LoadingBase`.
*No public members.*

### Ripple


Inherits `System.Windows.Controls.ContentControl`.
- **Dependency properties (7):** `RippleBrush`, `MaxOpacity`, `Duration`, `MaxConcurrentRipples`, `CornerRadius`, `IsRippleEnabled`, `Mode`
- **Methods (2):** `OnApplyTemplate()` · `TriggerRipple(Point origin)`

### RippleElement *(static class)*

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


Inherits `System.Windows.FrameworkElement`.
- **Dependency properties (9):** `ItemsSource`, `Date`, `StartHour`, `EndHour`, `HourHeight`, `SelectedItem`, `ItemBrush`, `ShowNowLine`, `NowLineBrush`
- **CLR events:** `SelectionChanged`, `ItemActivated`
- **Methods (3):** `SelectItem(SchedulerItem item)` · `GetItemBounds(SchedulerItem item)` · `ChangeDay(int days)`

### SchedulerItem

- **Properties (7):** `Title: string`, `Start: DateTime`, `End: DateTime`, `Resource: string`, `Accent: Brush`, `Tag: object`, `Duration: TimeSpan`
- **Methods (1):** `ToString()`

### SchedulerSelectionChangedEventArgs


Inherits `System.EventArgs`.
- **Properties (1):** `Item: SchedulerItem`

### ScrollProgressBar


Inherits `System.Windows.FrameworkElement`.
- **Dependency properties (6):** `Target`, `TrackBrush`, `BarBrush`, `Thickness`, `CornerRadius`, `ShowTrack`
- **Properties (1):** `Fraction: double`

### ScrollReveal *(static class)*

- **Dependency properties (5):** `IsEnabled`, `Offset`, `Duration`, `Delay`, `Threshold`
- **Attached properties:** `Delay`, `Duration`, `IsEnabled`, `Offset`, `Threshold`
- **Methods (10):** `SetIsEnabled(DependencyObject element, bool value)` · `GetIsEnabled(DependencyObject element)` · `SetOffset(DependencyObject element, double value)` · `GetOffset(DependencyObject element)` · `SetDuration(DependencyObject element, Duration value)` · `GetDuration(DependencyObject element)` · `SetDelay(DependencyObject element, Duration value)` · `GetDelay(DependencyObject element)` · `SetThreshold(DependencyObject element, double value)` · `GetThreshold(DependencyObject element)`

### SearchBox


Inherits `System.Windows.Controls.Control`.
- **Dependency properties (12):** `Text`, `Placeholder`, `IsInstantSearch`, `SearchDelay`, `MinSearchLength`, `CornerRadius`, `SearchIcon`, `ShowSearchIcon`, `ShowClearButton`, `HasText`, `SearchCommand`, `CommandParameter`
- **Routed events:** `SearchStarted`, `Clear`, `TextChanged`
- **CLR events:** `SearchStarted`, `Clear`, `TextChanged`
- **Methods (1):** `OnApplyTemplate()`

### SearchableTreePicker


Inherits `System.Windows.Controls.Control`.
- **Dependency properties (12):** `ItemsSource`, `ChildrenPath`, `DisplayPath`, `SearchText`, `IsSearchEnabled`, `Placeholder`, `IsExpandingMatches`, `EmptyText`, `SelectionMode`, `SelectedItem`, `SelectedItems`, `SelectedCount`
- **CLR events:** `SelectionChanged`
- **Properties (1):** `VisibleItems: IEnumerable<object>`
- **Methods (3):** `OnApplyTemplate()` · `ClearSelection()` · `SelectAllVisible()`

### ShakeElement *(static class)*

- **Dependency properties (3):** `IsShaking`, `Amplitude`, `Duration`
- **Attached properties:** `Amplitude`, `Duration`, `IsShaking`
- **Methods (7):** `SetIsShaking(DependencyObject element, bool value)` · `GetIsShaking(DependencyObject element)` · `SetAmplitude(DependencyObject element, double value)` · `GetAmplitude(DependencyObject element)` · `SetDuration(DependencyObject element, Duration value)` · `GetDuration(DependencyObject element)` · `Shake(FrameworkElement element)`

### SharedElementTransition *(static class)*

- **Methods (2):** `Run(FrameworkElement element, Rect fromRect, double milliseconds, IEasingFunction easing)` · `RunFrom(FrameworkElement element, FrameworkElement origin, double milliseconds)`

### SheenSweep


Inherits `System.Windows.FrameworkElement`.
- **Dependency properties (7):** `IsActive`, `Speed`, `BandWidth`, `Angle`, `PeakOpacity`, `SheenBrush`, `PauseSeconds`

### SideMenu


Inherits `System.Windows.Controls.Control`.
- **Dependency properties (5):** `Items`, `SelectedItem`, `IsCollapsed`, `Header`, `Footer`
- **Routed events:** `SelectionChanged`
- **CLR events:** `SelectionChanged`

### SideMenuItem


Inherits `System.Windows.DependencyObject`.
- **Dependency properties (7):** `Icon`, `Title`, `Badge`, `HasBadge`, `IsSelected`, `Command`, `Children`

### SignaturePad


Inherits `System.Windows.FrameworkElement`.
- **Dependency properties (10):** `StrokeBrush`, `StrokeThickness`, `Fill`, `EdgeBrush`, `CornerRadius`, `ShowGuideline`, `Placeholder`, `PlaceholderBrush`, `HasSignature`, `SignatureImage`
- **CLR events:** `SignatureChanged`
- **Properties (1):** `Strokes: StrokeCollection`
- **Methods (3):** `Undo()` · `Clear()` · `Export(double pixelWidth, double pixelHeight, Brush background)`

### SignaturePadAutomationPeer


Inherits `System.Windows.Automation.Peers.FrameworkElementAutomationPeer`.
*No public members.*

### Skeleton


Inherits `System.Windows.Controls.ContentControl`.
- **Dependency properties (7):** `IsLoading`, `SkeletonShape`, `CornerRadius`, `BaseBrush`, `HighlightBrush`, `Duration`, `IsWaveEnabled`
- **Methods (1):** `OnApplyTemplate()`

### SkeletonShape *(enum)*


Inherits `System.Enum`.
*No public members.*

### SnapKind *(enum)*


Inherits `System.Enum`.
*No public members.*

### Sparkline


Inherits `System.Windows.FrameworkElement`.
- **Dependency properties (8):** `Values`, `Stroke`, `Fill`, `StrokeThickness`, `ShowArea`, `ShowLastDot`, `Smooth`, `LastDotBrush`

### SplashScreen


Inherits `System.Windows.Controls.Control`.
- **Dependency properties (8):** `AppName`, `Version`, `Status`, `Progress`, `ShowProgress`, `IsIndeterminate`, `Logo`, `Tagline`
- **CLR events:** `Dismissed`
- **Methods (3):** `Show()` · `Dismiss(bool animate)` · `Report(double progress, string status)`

### SplitButton


Inherits `System.Windows.Controls.Control`.
- **Dependency properties (19):** `Content`, `Icon`, `IconSize`, `IconPlacement`, `Command`, `CommandParameter`, `CommandTarget`, `DropDownContent`, `DropDownContentTemplate`, `IsDropDownOpen`, `CornerRadius`, `Placement`, `MaxDropDownHeight`, `HitMode`, `AutoCloseOnClick`, `StaysOpen`, `IsOpenOnHover`, `HoverDelay`, `DropDownButtonPosition`
- **Routed events:** `Click`, `DropDownOpened`, `DropDownClosed`
- **CLR events:** `Click`, `DropDownOpened`, `DropDownClosed`
- **Methods (1):** `OnApplyTemplate()`

### SplitButtonHitMode *(enum)*


Inherits `System.Enum`.
*No public members.*

### SpotlightCard


Inherits `System.Windows.Controls.ContentControl`.
- **Dependency properties (7):** `CornerRadius`, `SpotlightBrush`, `SpotlightSize`, `SpotlightOpacity`, `IsSpotlightEnabled`, `BorderGradient`, `ShowHoverBorder`
- **Methods (1):** `OnApplyTemplate()`

### Spreadsheet


Inherits `System.Windows.Controls.Control`.
- **Dependency properties (19):** `ActiveSheet`, `Sheets`, `SelectionRange`, `ActiveRow`, `ActiveColumn`, `FormulaText`, `ShowToolbar`, `ShowFormulaBar`, `ShowSheetTabs`, `ShowStatusBar`, `IsReadOnly`, `RowHeaderWidth`, `ColumnHeaderHeight`, `GridLineBrush`, `HeaderBackground`, `SelectionBrush`, `SelectionBorderBrush`, `CellContextMenu`, `ReadyText`
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

- **Dependency properties (5):** `IsEnabled`, `Delay`, `Duration`, `OffsetY`, `MaxItems`
- **Attached properties:** `Delay`, `Duration`, `IsEnabled`, `MaxItems`, `OffsetY`
- **Methods (10):** `GetIsEnabled(DependencyObject element)` · `SetIsEnabled(DependencyObject element, bool value)` · `GetDelay(DependencyObject element)` · `SetDelay(DependencyObject element, double value)` · `GetDuration(DependencyObject element)` · `SetDuration(DependencyObject element, double value)` · `GetOffsetY(DependencyObject element)` · `SetOffsetY(DependencyObject element, double value)` · `GetMaxItems(DependencyObject element)` · `SetMaxItems(DependencyObject element, int value)`

### StatusBar


Inherits `System.Windows.Controls.ItemsControl`.
- **Dependency properties (1):** `Spacing`

### StatusBarAlignment *(enum)*


Inherits `System.Enum`.
*No public members.*

### StatusBarItem


Inherits `System.Windows.Controls.ContentControl`.
- **Dependency properties (6):** `Icon`, `Text`, `IsSeparator`, `Alignment`, `Command`, `CommandParameter`

### StatusBarPanel


Inherits `System.Windows.Controls.Panel`.
- **Dependency properties (1):** `Spacing`

### StatusToBackgroundConverter

- **Methods (2):** `Convert(object value, Type targetType, object parameter, CultureInfo culture)` · `ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)`

### StatusToForegroundConverter

- **Methods (2):** `Convert(object value, Type targetType, object parameter, CultureInfo culture)` · `ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)`

### StepBar


Inherits `System.Windows.Controls.ItemsControl`.
- **Dependency properties (9):** `StepIndex`, `Dock`, `IsNavigable`, `ShowProgressBar`, `StepConnectorThickness`, `CompletedBrush`, `ActiveBrush`, `WaitingBrush`, `ErrorBrush`
- **Routed events:** `StepChanged`, `StepCompleted`
- **CLR events:** `StepChanged`, `StepCompleted`
- **Methods (5):** `GoToStep(int index)` · `Next()` · `Prev()` · `OnApplyTemplate()` · `UpdateSteps(int stepIndex)`

### StepBarItem


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


Inherits `System.EventArgs`.
- **Properties (3):** `Item: object`, `Text: string`, `IsAdded: bool`

### TagEntry

- **Properties (2):** `Item: object`, `Text: string`
- **Methods (1):** `ToString()`

### TagInput


Inherits `System.Windows.Controls.Control`.
- **Dependency properties (10):** `ItemsSource`, `DisplayPath`, `SuggestionSource`, `Text`, `Placeholder`, `AllowFreeText`, `AllowDuplicates`, `MaxTags`, `Delimiter`, `IsSuggestionOpen`
- **CLR events:** `TagsChanged`
- **Properties (3):** `Tags: ObservableCollection<TagEntry>`, `Suggestions: ObservableCollection<TagEntry>`, `RemoveTagCommand: ICommand`
- **Methods (5):** `CommitText()` · `AddTag(object value)` · `RemoveTag(TagEntry entry)` · `ClearTags()` · `OnApplyTemplate()`

### TemporaryDimension

- **Properties (4):** `Start: Point3D`, `End: Point3D`, `Offset: double`, `Text: string`

### TextReveal


Inherits `CONCOST.UI.Controls.AnimatedEffectBase`.
- **Dependency properties (8):** `Text`, `FontSize`, `FontWeight`, `Foreground`, `WordDelay`, `WordDuration`, `OffsetY`, `StartDelay`

### TiltCard


Inherits `System.Windows.Controls.ContentControl`.
- **Dependency properties (3):** `MaxTilt`, `Lift`, `CornerRadius`

### TitleBarAction

- **Properties (5):** `Icon: IconKind`, `IconSourceFactory: Func<ImageSource>`, `ToolTip: string`, `Command: ICommand`, `CommandParameter: object`

### ToggleSwitch


Inherits `System.Windows.Controls.Primitives.ToggleButton`.
- **Dependency properties (12):** `OnContent`, `OffContent`, `SwitchWidth`, `SwitchHeight`, `ThumbSize`, `ThumbBrush`, `OnBrush`, `OffBrush`, `CornerRadius`, `ContentPlacement`, `ShowLabel`, `AnimationDuration`

### ToolWindow


Inherits `System.Windows.Controls.ContentControl`.
- **Dependency properties (15):** `Header`, `Icon`, `ShowHeader`, `HeaderHeight`, `IconSize`, `HeaderButtons`, `ShowActiveAccent`, `Dock`, `DockSize`, `MinDockSize`, `IsAutoHidden`, `IsFlyoutOpen`, `CanClose`, `CanAutoHide`, `IsActive`
- **CLR events:** `AutoHideChanged`, `Activated`, `Closing`, `Closed`, `DragStarted`, `DragMoved`, `DragCompleted`
- **Methods (3):** `Close()` · `ToggleAutoHide()` · `OnApplyTemplate()`

### Transfer


Inherits `System.Windows.Controls.Control`.
- **Dependency properties (9):** `ItemsSource`, `TransferredItems`, `SourceTitle`, `TargetTitle`, `ItemTemplate`, `ListWidth`, `ListHeight`, `ShowSearch`, `ShowSelectAll`
- **Routed events:** `Transferred`
- **CLR events:** `Transferred`
- **Properties (2):** `SourceItems: ObservableCollection<object>`, `TargetItems: ObservableCollection<object>`
- **Methods (6):** `OnApplyTemplate()` · `UpdateButtonStates()` · `TransferToTarget()` · `TransferToSource()` · `TransferAllToTarget()` · `TransferAllToSource()`

### TreeDataGrid


Inherits `System.Windows.Controls.DataGrid`.
- **Dependency properties (13):** `RootItems`, `ChildrenPath`, `ShowHierarchyColumn`, `HierarchyHeader`, `HierarchyColumnWidth`, `IndentSize`, `HierarchyContentTemplate`, `HierarchyPath`, `VisibleItemCount`, `Level`, `IndentWidth`, `HasChildren`, `IsExpanded`
- **Attached properties:** `HasChildren`, `IndentWidth`, `IsExpanded`, `Level`
- **Methods (10):** `GetLevel(DependencyObject element)` · `GetIndentWidth(DependencyObject element)` · `GetHasChildren(DependencyObject element)` · `GetIsExpanded(DependencyObject element)` · `SetIsExpanded(DependencyObject element, bool value)` · `ExpandAll()` · `CollapseAll()` · `ExpandTo(object item)` · `IsNodeExpanded(object item)` · `ToggleNode(object item)`

### TreeMap


Inherits `System.Windows.FrameworkElement`.
- **Dependency properties (12):** `ItemsSource`, `ValuePath`, `LabelPath`, `Palette`, `Fill`, `Gap`, `CornerRadius`, `ShowLabels`, `ShowValues`, `ValueFormat`, `LabelForeground`, `FontSize`
- **CLR events:** `ItemInvoked`
- **Properties (1):** `Entries: IReadOnlyList<TreeMapEntry>`
- **Methods (2):** `HitTest(Point point)` · `Invalidate()`

### TreeMapEntry

- **Properties (6):** `Item: object`, `Label: string`, `Value: double`, `Share: double`, `Brush: Brush`, `Bounds: Rect`

### TreeMapInvokedEventArgs


Inherits `System.EventArgs`.
- **Properties (1):** `Entry: TreeMapEntry`

### TypewriterText


Inherits `CONCOST.UI.Controls.AnimatedEffectBase`.
- **Dependency properties (10):** `Text`, `CharsPerSecond`, `StartDelay`, `ShowCaret`, `CaretBrush`, `FontSize`, `FontWeight`, `Foreground`, `IsLooping`, `RestartDelay`
- **Properties (1):** `VisibleCharacters: int`

### UcsMode *(enum)*


Inherits `System.Enum`.
*No public members.*

### UnitBox


Inherits `System.Windows.Controls.Control`.
- **Dependency properties (8):** `Value`, `Unit`, `DecimalPlaces`, `Header`, `Increment`, `Minimum`, `Maximum`, `IsReadOnly`
- **Routed events:** `ValueChanged`
- **CLR events:** `ValueChanged`

### ViewControlBar


Inherits `System.Windows.Controls.Grid`.
- **Dependency properties (23):** `ViewScale`, `DetailLevel`, `VisualStyle`, `ShowShadows`, `CropView`, `AnnotationCropView`, `ShowCropRegion`, `ShowAnnotationCropRegion`, `RevealHidden`, `Variant`, `CornerRadius`, `Elevation`, `ControlHeight`, `IconSize`, `ShowDividers`, `ShowScale`, `ShowDetailLevel`, `ShowVisualStyle`, `ShowToggles`, `ShowCropControls`, `ShowHideIsolate`, `Scales`, `HideIsolateCaption`
- **CLR events:** `SettingsChanged`, `HideIsolateRequested`
- **Methods (1):** `SyncFromProperties()`

### ViewControlBarVariant *(enum)*


Inherits `System.Enum`.
*No public members.*

### ViewCube


Inherits `System.Windows.FrameworkElement`.
- **Dependency properties (39):** `Host`, `Rotation`, `HomeRotation`, `FrontAngle`, `Theme`, `Visible`, `Position`, `DockMargin`, `Size`, `InactiveOpacity`, `ShowCompass`, `SnapToClosestView`, `SnapAngle`, `FitToViewOnChange`, `AnimatedTransitions`, `AnimationDuration`, `KeepSceneUpright`, `ClickThreshold`, `FaceBrush`, `EdgeBrush`, `BorderBrush`, `TextBrush`, `HighlightBrush`, `ArrowBrush`, `CompassBrush`, `CompassTextBrush`, `TopLabel`, `BottomLabel`, `FrontLabel`, `BackLabel`, `LeftLabel`, `RightLabel`, `AlwaysShowTools`, `DragSensitivity`, `ShowOrientationLabel`, `OrientationNameOverride`, `ShowFocusRing`, `AllowOrbit`, `ShowTooltips`
- **CLR events:** `ViewRequested`, `RegionHovered`, `ViewTransitionCompleted`, `HomeChanged`, `FrontChanged`, `ProjectionModeChanged`, `OptionsRequested`, `SaveViewRequested`, `SavedViewActivated`
- **Properties (5):** `SavedViews: ObservableCollection<ViewCubeSavedView>`, `OrientationName: string`, `Projection: ProjectionMode`, `HoveredRegion: ViewCubeHit`, `CurrentRotation: Quaternion`
- **Methods (17):** `ResetToHome()` · `GoToFace(string faceName)` · `Invalidate()` · `GoTo(ViewCubeRegion region)` · `GoHome()` · `SetCurrentViewAsHome()` · `SetCurrentViewAsFront()` · `ResetFront()` · `RollBy(int quarterTurns)` · `StepToNeighbor(ArrowDirection direction)` · `SetProjection(ProjectionMode mode)` · `SaveCurrentView(string name)` · `RemoveSavedView(string name)` · `GoToSavedView(string name)` · `OpenContextMenu()` · `OpenOrientationMenu()` · `HitTest(Point point)`

### ViewCubeFacelet *(struct)*


Inherits `System.ValueType`.
- **Properties (3):** `Region: ViewCubeRegion`, `Corners: Vector3D[]`, `Centre: Vector3D`

### ViewCubeHit

- **Properties (10):** `Empty: ViewCubeHit`, `Part: ViewCubePart`, `Label: string`, `Region: ViewCubeRegion`, `Arrow: ArrowDirection`, `Roll: int`, `Compass: Vector3D`, `Direction: Vector3D`, `IsEmpty: bool`, `IsRegion: bool`
- **Methods (1):** `ToString()`

### ViewCubeMath *(static class)*

- **Properties (3):** `DefaultEye: Vector3D`, `WorldUp: Vector3D`, `Regions: IReadOnlyList<ViewCubeRegion>`
- **Methods (21):** `Normalize(Vector3D vector)` · `RightFor(Vector3D forward, Vector3D up)` · `UprightUp(Vector3D forward, Vector3D fallback)` · `Rotate(Vector3D vector, Vector3D axis, double degrees)` · `Angle(Vector3D a, Vector3D b)` · `Elevation(Vector3D eye)` · `FromBasis(Vector3D right, Vector3D up, Vector3D forward)` · `ToBasis(Quaternion rotation, Vector3D& right, Vector3D& up, Vector3D& forward)` · `NormalizeRotation(Quaternion rotation)` · `RotationFor(ViewCubeRegion region, double frontAngle)` · `RotationFromEye(Vector3D eye, double frontAngle)` · `Slerp(Quaternion from, Quaternion to, double t)` · `AngleBetween(Quaternion from, Quaternion to)` · `SmoothStep(double t)` · `Upright(Quaternion rotation)` · `Roll(Quaternion rotation, double degrees)` · `Closest(Vector3D eye)` · `OrientationName(ViewCubeRegion region)` · `Closest(Quaternion rotation)` · `TryClosest(Vector3D eye, double toleranceDegrees, ViewCubeRegion& region)` · `TryFace(Quaternion rotation, double toleranceDegrees, ViewCubeRegion& region)`

### ViewCubeMesh *(static class)*

- **Properties (1):** `Default: IReadOnlyList<ViewCubeFacelet>`
- **Methods (1):** `Build(double faceRatio)`

### ViewCubePart *(enum)*


Inherits `System.Enum`.
*No public members.*

### ViewCubePosition *(enum)*


Inherits `System.Enum`.
*No public members.*

### ViewCubeRegion *(struct)*


Inherits `System.ValueType`.
- **Properties (9):** `Kind: ViewCubeRegionKind`, `Name: string`, `Label: string`, `IsEmpty: bool`, `IsFace: bool`, `Normal: Vector3D`, `Forward: Vector3D`, `Up: Vector3D`, `Right: Vector3D`
- **Methods (5):** `Rotated(double frontAngle)` · `Equals(ViewCubeRegion other)` · `Equals(object obj)` · `GetHashCode()` · `ToString()`

### ViewCubeRegionKind *(enum)*


Inherits `System.Enum`.
*No public members.*

### ViewCubeSavedView

- **Properties (2):** `Name: string`, `Rotation: Quaternion`
- **Methods (1):** `ToString()`

### ViewCubeSize *(enum)*


Inherits `System.Enum`.
*No public members.*

### ViewCubeTheme

- **Properties (5):** `Face: Brush`, `Edge: Brush`, `Border: Brush`, `Text: Brush`, `Highlight: Brush`

### ViewDetailLevel *(enum)*


Inherits `System.Enum`.
*No public members.*

### ViewRequestedEventArgs


Inherits `System.EventArgs`.
- **Properties (3):** `Hit: ViewCubeHit`, `Target: Quaternion`, `Cancel: bool`

### ViewportControl


Inherits `System.Windows.Controls.Grid`.
- **Dependency properties (32):** `ShowGrid`, `GridStep`, `ShowCrosshair`, `SnapToGrid`, `SnapDistance`, `SelectionRectangle`, `Snap`, `SnapKind`, `TemporaryDimension`, `Grips`, `ShowAxes`, `AxisBrush`, `AxisThickness`, `AxisDashStyle`, `ShowOriginMarker`, `GridBrush`, `GridMajorBrush`, `GridMajorEvery`, `MinGridPixelSpacing`, `CrosshairBrush`, `CrosshairThickness`, `CrosshairDashStyle`, `CrosshairExtent`, `ShowAperture`, `SnapMarkerBrush`, `SnapMarkerSize`, `ShowSnapMarker`, `ShowSnapLabel`, `SelectionBrush`, `SelectionFillOpacity`, `DimensionBrush`, `FitMargin`
- **CLR events:** `CursorMoved`, `WorldClicked`
- **Properties (2):** `Camera: CameraController`, `Overlay: Canvas`
- **Methods (5):** `ToScreen(Point3D world)` · `ToWorld(Point screen)` · `ZoomToFit(Rect3D bounds)` · `ToggleGrid()` · `UnitsToPixels(double units)`

### ViewportCursorEventArgs


Inherits `System.EventArgs`.
- **Properties (3):** `World: Point3D`, `Screen: Point`, `IsSnapped: bool`

### ViewportGrip

- **Properties (2):** `Position: Point3D`, `IsHot: bool`

### VisualStyleMode *(enum)*


Inherits `System.Enum`.
*No public members.*

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


Inherits `System.Windows.FrameworkElement`.
- **Dependency properties (7):** `Amplitude`, `Wavelength`, `Speed`, `FillBrush`, `BackBrush`, `ShowBackWave`, `IsAnimated`

### Wizard


Inherits `System.Windows.Controls.Control`.
- **Dependency properties (10):** `CurrentIndex`, `CurrentStep`, `CanGoBack`, `CanGoNext`, `IsLastStep`, `ShowHeader`, `ShowFooter`, `BackText`, `NextText`, `FinishText`
- **CLR events:** `Navigating`, `StepChanged`, `Finished`
- **Properties (1):** `Steps: ObservableCollection<WizardStep>`
- **Methods (6):** `Next()` · `Back()` · `GoTo(int index)` · `Finish()` · `RefreshState()` · `OnApplyTemplate()`

### WizardNavigatingEventArgs


Inherits `System.EventArgs`.
- **Properties (3):** `FromIndex: int`, `ToIndex: int`, `Cancel: bool`

### WizardStep


Inherits `System.Windows.Controls.HeaderedContentControl`.
- **Dependency properties (6):** `Description`, `IsValid`, `Index`, `Number`, `IsCurrent`, `IsCompleted`

## CONCOST.UI.Controls.Canvas

### RulerControl


Inherits `System.Windows.Controls.Control`.
- **Dependency properties (4):** `Orientation`, `UnitScale`, `MajorStep`, `OriginOffset`

### RulerControlAutomationPeer


Inherits `System.Windows.Automation.Peers.FrameworkElementAutomationPeer`.
*No public members.*

### SnowCanvas


Inherits `System.Windows.Controls.Canvas`.
- **Dependency properties (8):** `SnowflakeCount`, `SnowflakeBrush`, `MinSnowflakeSize`, `MaxSnowflakeSize`, `MinSpeed`, `MaxSpeed`, `Wind`, `IsActive`

### Snowflake

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

- **Properties (5):** `Header: string`, `Icon: IconKind`, `Content: object`, `IsSeparator: bool`, `Tag: object`
- **Methods (1):** `ToString()`

### KeyTipService *(static class)*

- **Dependency properties (3):** `KeyTip`, `IsEnabled`, `IsShowing`
- **Attached properties:** `IsEnabled`, `IsShowing`, `KeyTip`
- **Methods (7):** `GetKeyTip(DependencyObject element)` · `SetKeyTip(DependencyObject element, string value)` · `GetIsEnabled(DependencyObject element)` · `SetIsEnabled(DependencyObject element, bool value)` · `GetIsShowing(DependencyObject element)` · `Show(Ribbon ribbon)` · `Hide(Ribbon ribbon)`

### Ribbon


Inherits `System.Windows.Controls.Control`.
- **Dependency properties (29):** `ItemsSource`, `SelectedIndex`, `SelectedItem`, `IsMinimized`, `ApplicationMenu`, `ShowQuickAccessToolBar`, `QuickAccessPlacement`, `MenuBar`, `MenuBarPlacement`, `ShowMenuBar`, `ShowApplicationMenu`, `ShowTabStrip`, `SelectedTabId`, `IsGroupCollapseEnabled`, `IsKeyTipEnabled`, `ShowScreenTips`, `IsQuickAccessCustomizable`, `ShowBorder`, `EmptyContent`, `DisplayMode`, `IsOverlayOpen`, `InfoCenter`, `ShowInfoCenter`, `ShowMinimizeButton`, `ShowItemLabels`, `IsTabStripOverflowing`, `IsEmpty`, `ShowGroupLabels`, `VisibleTabs`
- **Attached properties:** `ShowGroupLabels`, `ShowItemLabels`
- **CLR events:** `DisplayModeChanged`
- **Properties (5):** `Items: ObservableCollection<object>`, `Tabs: IReadOnlyList<RibbonTabItem>`, `AllItems: IReadOnlyList<RibbonItem>`, `QuickAccess: RibbonQuickAccessToolBar`, `IsShowingKeyTips: bool`
- **Methods (19):** `GetShowItemLabels(DependencyObject obj)` · `SetShowItemLabels(DependencyObject obj, bool value)` · `GetShowGroupLabels(DependencyObject obj)` · `SetShowGroupLabels(DependencyObject obj, bool value)` · `CycleState()` · `ToggleMinimized()` · `NextMode(RibbonDisplayMode mode)` · `ShowContextualTab(RibbonTabItem tab, bool activate)` · `HideContextualTab(string id)` · `ShowKeyTips()` · `HideKeyTips()` · `ProcessKeyTipKey(Key key)` · `PinToQuickAccess(RibbonItem item)` · `UnpinFromQuickAccess(RibbonItem item)` · `IsPinnedToQuickAccess(RibbonItem item)` · `CaptureState()` · `ApplyState(RibbonState state)` · `RefreshTabs()` · `OnApplyTemplate()`

### RibbonApplicationMenu


Inherits `CONCOST.UI.Controls.Ribbon.RibbonDropDownButton`.
- **Dependency properties (3):** `RecentItems`, `RecentHeader`, `FooterItems`

### RibbonAutomationPeer


Inherits `System.Windows.Automation.Peers.FrameworkElementAutomationPeer`.
*No public members.*

### RibbonBackstageView


Inherits `System.Windows.Controls.Grid`.
- **Dependency properties (5):** `IsOpen`, `ApplicationName`, `SelectedItem`, `RailWidth`, `BackText`
- **CLR events:** `IsOpenChanged`, `SelectedItemChanged`
- **Properties (1):** `Items: ObservableCollection<BackstageItem>`
- **Methods (4):** `Open()` · `Close()` · `Select(string header)` · `RebuildRail()`

### RibbonButton


Inherits `CONCOST.UI.Controls.Ribbon.RibbonItem`.
- **Dependency properties (1):** `IsPressed`
- **CLR events:** `Click`
- **Methods (1):** `PerformClick()`

### RibbonButtonAutomationPeer


Inherits `System.Windows.Automation.Peers.FrameworkElementAutomationPeer`.
- **Methods (2):** `GetPattern(PatternInterface patternInterface)` · `Invoke()`

### RibbonCheckBox


Inherits `CONCOST.UI.Controls.Ribbon.RibbonToggleButton`.
*No public members.*

### RibbonColorPicker


Inherits `CONCOST.UI.Controls.Ribbon.RibbonItem`.
- **Dependency properties (4):** `SelectedColor`, `IsDropDownOpen`, `ShowAlpha`, `SwatchBrush`
- **CLR events:** `SelectedColorChanged`
- **Methods (1):** `PerformClick()`

### RibbonComboBox


Inherits `CONCOST.UI.Controls.Ribbon.RibbonItem`.
- **Dependency properties (7):** `ItemsSource`, `SelectedItem`, `SelectedIndex`, `DisplayMemberPath`, `ItemTemplate`, `IsEditable`, `MaxDropDownHeight`
- **Methods (1):** `PerformClick()`

### RibbonComboBoxAutomationPeer


Inherits `System.Windows.Automation.Peers.FrameworkElementAutomationPeer`.
- **Properties (3):** `ExpandCollapseState: ExpandCollapseState`, `IsReadOnly: bool`, `Value: string`
- **Methods (4):** `GetPattern(PatternInterface patternInterface)` · `Expand()` · `Collapse()` · `SetValue(string value)`

### RibbonContextualTabGroup


Inherits `System.Windows.Controls.HeaderedItemsControl`.
- **Dependency properties (2):** `Accent`, `AutoActivate`
- **CLR events:** `TabsChanged`
- **Properties (2):** `IsGroupVisible: bool`, `Tabs: IReadOnlyList<RibbonTabItem>`

### RibbonDisplayMode *(enum)*


Inherits `System.Enum`.
*No public members.*

### RibbonDropDownButton


Inherits `CONCOST.UI.Controls.Ribbon.RibbonItem`.
- **Dependency properties (1):** `IsOpen`
- **Properties (1):** `Items: ObservableCollection<object>`
- **Methods (1):** `PerformClick()`

### RibbonGallery


Inherits `CONCOST.UI.Controls.Ribbon.RibbonItem`.
- **Dependency properties (10):** `ItemsSource`, `ItemTemplate`, `ExpandedItemTemplate`, `SelectedItem`, `IsDropDownOpen`, `ShowExpandButton`, `ItemWidth`, `ItemHeight`, `MaxDropDownHeight`, `MaxDropDownWidth`
- **CLR events:** `SelectionChanged`
- **Methods (1):** `PerformClick()`

### RibbonGalleryAutomationPeer


Inherits `System.Windows.Automation.Peers.FrameworkElementAutomationPeer`.
- **Properties (1):** `ExpandCollapseState: ExpandCollapseState`
- **Methods (3):** `GetPattern(PatternInterface patternInterface)` · `Expand()` · `Collapse()`

### RibbonGalleryItem

- **Properties (4):** `Header: string`, `Icon: ImageSource`, `ToolTip: string`, `Tag: object`
- **Methods (1):** `ToString()`

### RibbonGroup


Inherits `System.Windows.Controls.HeaderedItemsControl`.
- **Dependency properties (14):** `DialogLauncherCommand`, `CollapsePriority`, `IsCollapsed`, `SizeMode`, `IsDropDownOpen`, `IsUserCollapsed`, `SlideOut`, `SlideOutTemplate`, `IsSlideOutOpen`, `IsTitleOnly`, `IsButtonOnly`, `IsLastInStrip`, `KeyTip`, `HasSlideOut`
- **Properties (1):** `CollapsedMenu: RibbonMenu`
- **Methods (1):** `OnApplyTemplate()`

### RibbonGroupAutomationPeer


Inherits `System.Windows.Automation.Peers.FrameworkElementAutomationPeer`.
*No public members.*

### RibbonGroupLauncher


Inherits `System.Windows.Controls.Button`.
*No public members.*

### RibbonGroupPanel


Inherits `System.Windows.Controls.Panel`.
- **Dependency properties (4):** `ColumnWidth`, `RowHeight`, `RowGap`, `SeparatorWidth`
- **Properties (2):** `GroupHeight: double`, `MediumHeight: double`

### RibbonGroupSizeMode *(enum)*


Inherits `System.Enum`.
*No public members.*

### RibbonGroupStrip


Inherits `System.Windows.Controls.Panel`.
*No public members.*

### RibbonItem *(abstract)*


Inherits `System.Windows.Controls.HeaderedContentControl`.
- **Dependency properties (20):** `Command`, `CommandParameter`, `CommandTarget`, `Icon`, `LargeIcon`, `SmallIcon`, `Size`, `ColumnSpan`, `IsFixedSize`, `KeyTip`, `ScreenTip`, `GestureText`, `Id`, `CollapsePriority`, `ExtendedScreenTip`, `ToolTipPreview`, `ToolTipFooter`, `IsLabelVisible`, `EffectiveIconSource`, `EffectiveSize`
- **Properties (1):** `EffectiveIcon: ImageSource`
- **Methods (2):** `CreateQuickAccessClone()` · `PerformClick()`

### RibbonItemSize *(enum)*


Inherits `System.Enum`.
*No public members.*

### RibbonKeyTip


Inherits `System.Windows.Controls.ContentControl`.
- **Dependency properties (1):** `Target`

### RibbonLabel


Inherits `CONCOST.UI.Controls.Ribbon.RibbonItem`.
*No public members.*

### RibbonMenu


Inherits `System.Windows.Controls.ItemsControl`.
*No public members.*

### RibbonMenuBar


Inherits `System.Windows.Controls.ItemsControl`.
- **Dependency properties (1):** `ShowDivider`
- **Properties (1):** `IsAnyOpen: bool`
- **Methods (1):** `CloseAll()`

### RibbonMenuBarItem


Inherits `System.Windows.Controls.HeaderedItemsControl`.
- **Dependency properties (1):** `IsSubmenuOpen`
- **Methods (2):** `Open()` · `Close()`

### RibbonMenuBarPlacement *(enum)*


Inherits `System.Enum`.
*No public members.*

### RibbonMenuItem


Inherits `System.Windows.Controls.HeaderedItemsControl`.
- **Dependency properties (7):** `Command`, `CommandParameter`, `Icon`, `InputGestureText`, `IsSubmenuOpen`, `IsCheckable`, `IsChecked`
- **Routed events:** `Click`
- **CLR events:** `Click`
- **Methods (1):** `PerformClick()`

### RibbonMenuSeparator


Inherits `System.Windows.Controls.Control`.
*No public members.*

### RibbonQuickAccessButton


Inherits `CONCOST.UI.Controls.Ribbon.RibbonButton`.
*No public members.*

### RibbonQuickAccessPlacement *(enum)*


Inherits `System.Enum`.
*No public members.*

### RibbonQuickAccessToolBar


Inherits `System.Windows.Controls.ItemsControl`.
- **Dependency properties (6):** `IsPinned`, `PinnableItems`, `IsCustomizeOpen`, `IsCustomizable`, `ShowLabels`, `CustomizeMenu`
- **Attached properties:** `IsPinned`
- **CLR events:** `PinsChanged`
- **Properties (1):** `PinnedItems: IEnumerable<RibbonItem>`
- **Methods (8):** `GetIsPinned(DependencyObject element)` · `SetIsPinned(DependencyObject element, bool value)` · `IsPinned(RibbonItem source)` · `Pin(RibbonItem source)` · `Unpin(RibbonItem source)` · `TogglePin(RibbonItem source)` · `ClearPins()` · `OnApplyTemplate()`

### RibbonRadioButton


Inherits `CONCOST.UI.Controls.Ribbon.RibbonToggleButton`.
*No public members.*

### RibbonRadioButtonAutomationPeer


Inherits `CONCOST.UI.Controls.Ribbon.RibbonToggleButtonAutomationPeer`.
*No public members.*

### RibbonRadioGroup


Inherits `CONCOST.UI.Controls.Ribbon.RibbonItem`.
- **Dependency properties (2):** `SelectedIndex`, `SelectedItem`
- **Properties (1):** `Items: ObservableCollection<object>`
- **Methods (1):** `PerformClick()`

### RibbonSearchBox


Inherits `CONCOST.UI.Controls.Ribbon.RibbonItem`.
- **Dependency properties (4):** `Text`, `Placeholder`, `IsInstantSearch`, `SearchCommand`
- **Methods (1):** `PerformClick()`

### RibbonSeparator


Inherits `System.Windows.Controls.Control`.
*No public members.*

### RibbonSplitButton


Inherits `CONCOST.UI.Controls.Ribbon.RibbonItem`.
- **Dependency properties (6):** `IsOpen`, `ArrowWidth`, `RememberLast`, `LastUsedItem`, `BodyHeader`, `BodyIcon`
- **CLR events:** `Click`
- **Properties (1):** `Items: ObservableCollection<object>`
- **Methods (1):** `PerformClick()`

### RibbonState

- **Properties (5):** `SelectedTabId: string`, `IsMinimized: bool`, `QuickAccessItemIds: List<string>`, `QuickAccessPlacement: string`, `CollapsedGroupKeys: List<string>`

### RibbonStateStore *(static class)*

- **Methods (4):** `ToJson(RibbonState state)` · `FromJson(string json)` · `Save(RibbonState state, string path)` · `Load(string path)`

### RibbonTabItem


Inherits `System.Windows.Controls.HeaderedContentControl`.
- **Dependency properties (10):** `Icon`, `IsSelected`, `IsContextual`, `ContextualColor`, `HasCustomContextualColor`, `ContextualGroup`, `DisplayHeader`, `IsAvailable`, `KeyTip`, `Id`
- **CLR events:** `IsAvailableChanged`

### RibbonTabStripPanel


Inherits `System.Windows.Controls.Panel`.
- **Dependency properties (3):** `BandHeight`, `FontFamily`, `FontSize`

### RibbonTextBox


Inherits `CONCOST.UI.Controls.Ribbon.RibbonItem`.
- **Dependency properties (5):** `Text`, `PlaceholderText`, `IsReadOnly`, `MaxLength`, `TextAlignment`
- **Methods (1):** `PerformClick()`

### RibbonToggleButton


Inherits `CONCOST.UI.Controls.Ribbon.RibbonButton`.
- **Dependency properties (2):** `IsChecked`, `GroupName`
- **CLR events:** `IsCheckedChanged`

### RibbonToggleButtonAutomationPeer


Inherits `CONCOST.UI.Controls.Ribbon.RibbonButtonAutomationPeer`.
- **Properties (1):** `ToggleState: ToggleState`
- **Methods (2):** `GetPattern(PatternInterface patternInterface)` · `Toggle()`

### RibbonToolTip


Inherits `System.Windows.Controls.ToolTip`.
- **Dependency properties (9):** `Title`, `Description`, `GestureText`, `Preview`, `Footer`, `ExpandedDescription`, `ExpandDelay`, `IsExpanded`, `CanExpand`

## CONCOST.UI.Controls.Windows

### CONCOSTWindow


Inherits `System.Windows.Window`.
- **Dependency properties (1):** `TitleBarBackground`
- **Methods (1):** `OnApplyTemplate()`

## CONCOST.UI.Core

### MotionSettings *(static class)*

- **CLR events:** `Changed`
- **Properties (3):** `SystemAnimationsEnabled: bool`, `ForceDisabled: bool`, `AnimationsEnabled: bool`

## CONCOST.UI.Core.Converters

### BooleanToVisibilityConverter

- **Properties (2):** `Invert: bool`, `Collapse: bool`
- **Methods (2):** `Convert(object value, Type targetType, object parameter, CultureInfo culture)` · `ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)`

### CountToVisibilityConverter

- **Methods (2):** `Convert(object value, Type targetType, object parameter, CultureInfo culture)` · `ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)`

### ExpanderCornerRadiusConverter

- **Properties (1):** `Offset: double`
- **Methods (4):** `Convert(Object[] values, Type targetType, object parameter, CultureInfo culture)` · `ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)` · `Convert(object value, Type targetType, object parameter, CultureInfo culture)` · `ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)`

### FilterOperatorLabelConverter

- **Methods (2):** `Convert(object value, Type targetType, object parameter, CultureInfo culture)` · `ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)`

### NullToVisibilityConverter

- **Properties (1):** `Invert: bool`
- **Methods (2):** `Convert(object value, Type targetType, object parameter, CultureInfo culture)` · `ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)`

### PaddingToContentHostMarginConverter

- **Methods (2):** `Convert(object value, Type targetType, object parameter, CultureInfo culture)` · `ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)`

## CONCOST.UI.Core.Helpers

### ColorHelper *(static class)*

- **Methods (4):** `FromHex(string hex)` · `Lighten(Color color, double factor)` · `Darken(Color color, double factor)` · `CreateBrush(Color color, double opacity)`

### VisualTreeHelperEx *(static class)*

- **Methods (2):** `FindParent(DependencyObject child)` · `FindChild(DependencyObject parent)`

## CONCOST.UI.Core.MVVM

### AsyncRelayCommand

- **CLR events:** `CanExecuteChanged`
- **Properties (1):** `IsExecuting: bool`
- **Methods (3):** `CanExecute(object parameter)` · `Execute(object parameter)` · `RaiseCanExecuteChanged()`

### ObservableObject *(abstract)*

- **CLR events:** `PropertyChanged`

### RelayCommand

- **CLR events:** `CanExecuteChanged`
- **Methods (3):** `CanExecute(object parameter)` · `Execute(object parameter)` · `RaiseCanExecuteChanged()`

### RelayCommand`1

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

- **CLR events:** `ToggleChanged`, `WorkPlaneChanged`, `CursorMoved`, `StatusChanged`, `ModifiedChanged`
- **Properties (10):** `Current: ApplicationStateService`, `SnapEnabled: bool`, `GridEnabled: bool`, `OrthoEnabled: bool`, `PolarEnabled: bool`, `ObjectSnapEnabled: bool`, `WorkPlane: UcsMode`, `CursorPosition: Point3D`, `StatusMessage: string`, `IsModified: bool`
- **Methods (5):** `Toggle(CadToggle toggle)` · `Attach(CadStatusBar bar)` · `Detach(CadStatusBar bar)` · `CaptureToggles()` · `ApplyToggles(IReadOnlyDictionary<CadToggle, bool> toggles)`

### CameraChangedEventArgs


Inherits `System.EventArgs`.
- **Properties (2):** `Direction: Vector3D`, `IsFinal: bool`

### CameraController

- **CLR events:** `Changed`, `CameraChanged`, `FitToViewRequested`
- **Properties (15):** `Target: Point3D`, `Distance: double`, `Yaw: double`, `Pitch: double`, `Roll: double`, `FieldOfView: double`, `OrthographicHeight: double`, `Projection: CameraProjection`, `Direction: Vector3D`, `Eye: Point3D`, `Forward: Vector3D`, `Right: Vector3D`, `Up: Vector3D`, `Rotation: Quaternion`, `ViewMatrix: Matrix3D`
- **Methods (12):** `VisibleHeight(double distance)` · `Orbit(double deltaYaw, double deltaPitch, bool isFinal)` · `Pan(double deltaX, double deltaY, Size viewport, bool isFinal)` · `Zoom(double factor, bool isFinal)` · `ZoomAt(Point anchor, Size viewport, double factor)` · `SetOrientation(Vector3D direction, bool isFinal)` · `LookAt(Point3D target)` · `FitTo(Rect3D bounds, double margin)` · `FitToView()` · `TryProject(Point3D world, Size viewport, Point& screen)` · `Unproject(Point point, Size viewport)` · `CopyTo(CameraController other)`

### CameraProjection *(enum)*


Inherits `System.Enum`.
*No public members.*

### CameraSnapshot *(struct)*


Inherits `System.ValueType`.
- **Properties (7):** `Target: Point3D`, `Distance: double`, `Yaw: double`, `Pitch: double`, `FieldOfView: double`, `OrthographicHeight: double`, `Projection: CameraProjection`
- **Methods (2):** `Capture(CameraController camera)` · `ApplyTo(CameraController camera)`

### DataGridExportService *(static class)*

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


Inherits `System.EventArgs`.
- **Properties (2):** `Document: DocumentModel`, `Cancel: bool`

### DocumentManager

- **CLR events:** `DocumentClosing`, `DocumentOpened`, `DocumentClosed`, `ActiveDocumentChanged`, `DirtyChanged`
- **Properties (3):** `Current: DocumentManager`, `Documents: ObservableCollection<DocumentModel>`, `ActiveDocument: DocumentModel`
- **Methods (5):** `Open(string title, string filePath)` · `Close(DocumentModel document)` · `CloseAll()` · `SetDirty(DocumentModel document, bool isDirty)` · `BuildTitle(string applicationName)`

### DocumentModel

- **Properties (6):** `Title: string`, `FilePath: string`, `IsDirty: bool`, `CreatedUtc: DateTime`, `Tag: object`, `DisplayTitle: string`
- **Methods (1):** `ToString()`

### ExportService

- **Properties (1):** `Current: ExportService`
- **Methods (2):** `ExportDataGridToCsv(DataGrid grid, string filePath)` · `ExportVisualToPng(FrameworkElement visual, string filePath, double dpi)`

### HotkeyManager *(static class)*

- **Methods (3):** `Register(string gesture, ICommand command)` · `Unregister(string gesture)` · `AttachToWindow(Window window)`

### ICameraHost *(interface)*

- **Properties (3):** `Rotation: Quaternion`, `PivotPoint: Point3D`, `Projection: ProjectionMode`
- **Methods (1):** `FitToView()`

### ICameraHostEvents *(interface)*

- **CLR events:** `CameraChanged`

### IDialogService *(interface)*

- **Methods (2):** `ShowMessageAsync(string title, string message, DialogType type)` · `ShowConfirmAsync(string title, string message)`

### IPropertyPaletteDisplay *(interface)*

- **Properties (2):** `PropertyTitle: string`, `PropertySubtitle: string`

### IRendererAdapter *(interface)*

- **Methods (3):** `TryProject(Point3D world, Size viewport, Point& screen)` · `TryUnproject(Point screen, Size viewport, Point3D& world)` · `TryPick(Point screen, Size viewport, Object& entity)`

### ITypeSelectable *(interface)*

- **Properties (2):** `TypeName: string`, `AvailableTypes: IReadOnlyList<string>`

### LocalizationManager


Inherits `CONCOST.UI.Core.MVVM.ObservableObject`.
- **Properties (2):** `Current: LocalizationManager`, `Culture: CultureInfo`
- **Methods (1):** `GetString(string key, string defaultValue)`

### PickEventArgs


Inherits `System.EventArgs`.
- **Properties (2):** `World: Point3D`, `Entity: object`

### PrintService

- **Properties (1):** `Current: PrintService`
- **Methods (1):** `PrintVisual(Visual visual, string jobDescription)`

### ProjectionMode *(enum)*


Inherits `System.Enum`.
*No public members.*

### PropertyDescriptor

- **Properties (11):** `Name: string`, `DisplayName: string`, `Category: string`, `Description: string`, `Editor: PropertyEditorKind`, `DefaultValue: object`, `Choices: IReadOnlyList<object>`, `Unit: string`, `Format: string`, `Validator: Func<object, object, string>`, `IsReadOnly: bool`
- **Methods (2):** `GetValue(object target)` · `SetValue(object target, object value)`

### PropertyEditorKind *(enum)*


Inherits `System.Enum`.
*No public members.*

### PropertyManager

- **CLR events:** `ValueChanged`
- **Properties (1):** `Current: PropertyManager`
- **Methods (9):** `Register(PropertyDescriptor[] descriptors)` · `Register(Type type, PropertyDescriptor[] descriptors)` · `GetDescriptors(object target)` · `Find(object target, string name)` · `GetValue(object target, string name)` · `Validate(object target, PropertyDescriptor property, object value)` · `TrySetValue(object target, string name, object value, String& error)` · `TrySetValueFromText(object target, string name, string text, String& error)` · `TryConvert(PropertyDescriptor property, string text, Object& value, String& error)`

### PropertyValueChangedEventArgs


Inherits `System.EventArgs`.
- **Properties (4):** `Target: object`, `Property: PropertyDescriptor`, `OldValue: object`, `NewValue: object`

### RendererBridge

- **CLR events:** `RenderRequested`, `HoverChanged`
- **Properties (5):** `Current: RendererBridge`, `Camera: CameraController`, `Adapter: IRendererAdapter`, `IsAccelerated: bool`, `CursorWorld: Point3D`
- **Methods (5):** `Invalidate()` · `TryProject(Point3D world, Size viewport, Point& screen)` · `TryUnproject(Point screen, Size viewport, Point3D& world)` · `TryPick(Point screen, Size viewport, Object& entity)` · `UpdateCursor(Point screen, Size viewport)`

### SelectionChangeEventArgs


Inherits `System.EventArgs`.
- **Properties (5):** `Kind: SelectionChangeKind`, `Added: IReadOnlyList<object>`, `Removed: IReadOnlyList<object>`, `Primary: object`, `IsEmpty: bool`

### SelectionChangeKind *(enum)*


Inherits `System.Enum`.
*No public members.*

### SelectionManager

- **CLR events:** `SelectionChanged`
- **Properties (5):** `Current: SelectionManager`, `Items: IReadOnlyList<object>`, `Count: int`, `Primary: object`, `IsEmpty: bool`
- **Methods (7):** `Select(object item)` · `SelectMany(IEnumerable<object> items)` · `Add(object item)` · `Remove(object item)` · `Toggle(object item)` · `Clear()` · `Contains(object item)`

### ToastItem

- **Properties (3):** `Title: string`, `Message: string`, `Type: ToastType`

### ToastService

- **Properties (2):** `Current: ToastService`, `ActiveToasts: ObservableCollection<ToastItem>`
- **Methods (5):** `Show(string message, string title, ToastType type, int durationMs)` · `Success(string message, string title)` · `Warning(string message, string title)` · `Error(string message, string title)` · `Info(string message, string title)`

### ToastType *(enum)*


Inherits `System.Enum`.
*No public members.*

### Transaction

- **Properties (3):** `Name: string`, `ChangeCount: int`, `IsCompleted: bool`
- **Methods (5):** `Record(string name, Action apply, Action revert)` · `Record(IUndoableCommand command)` · `Commit()` · `Rollback()` · `Dispose()`

### TransactionEventArgs


Inherits `System.EventArgs`.
- **Properties (2):** `Name: string`, `ChangeCount: int`

### TransactionManager

- **CLR events:** `Committed`, `RolledBack`, `Changed`
- **Properties (3):** `Current: TransactionManager`, `Active: Transaction`, `IsInTransaction: bool`
- **Methods (2):** `Begin(string name)` · `Run(string name, Action<Transaction> edit)`

### ViewDefinition

- **Properties (5):** `Name: string`, `Kind: ViewKind`, `Camera: CameraSnapshot`, `IsOpen: bool`, `Tag: object`
- **Methods (1):** `ToString()`

### ViewEventArgs


Inherits `System.EventArgs`.
- **Properties (1):** `View: ViewDefinition`

### ViewKind *(enum)*


Inherits `System.Enum`.
*No public members.*

### ViewManager

- **CLR events:** `OpenRequested`, `ViewClosed`, `ActiveViewChanged`
- **Properties (3):** `Current: ViewManager`, `Views: ObservableCollection<ViewDefinition>`, `ActiveView: ViewDefinition`
- **Methods (7):** `Add(string name, ViewKind kind)` · `Find(string name)` · `Open(ViewDefinition view)` · `Open(string name, ViewKind kind)` · `Close(ViewDefinition view)` · `RememberCamera(ViewDefinition view, CameraController camera)` · `RestoreCamera(ViewDefinition view, CameraController camera)`

### WorkspaceLayoutProfile

- **Properties (10):** `Name: string`, `TileMode: DocumentTileMode`, `ActiveViewName: string`, `WorkPlane: UcsMode`, `SnapEnabled: bool`, `GridEnabled: bool`, `OrthoEnabled: bool`, `PolarEnabled: bool`, `ObjectSnapEnabled: bool`, `SavedUtc: DateTime`
- **Methods (3):** `ToToggles()` · `FromToggles(IReadOnlyDictionary<CadToggle, bool> toggles)` · `ToString()`

### WorkspaceLayoutService

- **CLR events:** `ProfileApplied`, `ProfilesChanged`
- **Properties (5):** `Current: WorkspaceLayoutService`, `Profiles: ObservableCollection<WorkspaceLayoutProfile>`, `Workspace: DocumentWorkspace`, `StatusBar: CadStatusBar`, `ActiveViewName: string`
- **Methods (8):** `Attach(DocumentWorkspace workspace, CadStatusBar statusBar)` · `Capture(string name)` · `Save(string name)` · `Apply(WorkspaceLayoutProfile profile)` · `Apply(string name)` · `Find(string name)` · `Remove(string name)` · `Persist()`

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

- **Properties (11):** `Primary: Color`, `PrimaryLight: Color`, `PrimaryDark: Color`, `PrimaryHover: Color`, `PrimaryPressed: Color`, `PrimaryDisabled: Color`, `PrimaryText: Color`, `PrimarySoft: Color`, `PrimarySoftLight: Color`, `PrimarySoftBorder: Color`, `PrimarySoftForeground: Color`

### AccentPaletteGenerator *(static class)*

- **Methods (1):** `Generate(Color baseColor, bool isDark)`

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


Inherits `System.Windows.Documents.Adorner`.
- **Dependency properties (1):** `Progress`

## XamlGeneratedNamespace

### GeneratedInternalTypeHelper


Inherits `System.Windows.Markup.InternalTypeHelper`.
*No public members.*

