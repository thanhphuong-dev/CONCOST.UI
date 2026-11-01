# BÁO CÁO TỔNG HỢP CONCOST.UI CONTROL LIBRARY

## 1. TỔNG QUAN

- **Tổng số Control (có thể sử dụng trực tiếp):** 139
- **Tổng số Hiệu ứng / Decoration:** 30
- **Tổng số Helper / Internal:** 49

## 2. DANH SÁCH CONTROL THEO NHÓM

### Engineering & CAD

- **SpreadsheetGridPresenter** (FrameworkElement)
  - Thuộc tính mở rộng: ActiveSheet, Sheets, SelectionRange, ActiveRow, ActiveColumn, FormulaText, ShowToolbar, ShowFormulaBar, ShowSheetTabs, ShowStatusBar, IsReadOnly, RowHeaderWidth, ColumnHeaderHeight, GridLineBrush, HeaderBackground, SelectionBrush, SelectionBorderBrush
- **RebarInput** (Control)
  - Thuộc tính mở rộng: Text, IsValid, AreaMm2
- **CompassIndicator** (Control)
  - Thuộc tính mở rộng: Angle

### Data & Dashboard

- **DataPager** (Control)
  - Thuộc tính mở rộng: PageIndex, PageSize, TotalItems
- **StatisticCard** (Control)
  - Thuộc tính mở rộng: Title, Value, Unit, Trend, TrendDirection, Icon, Progress, Subtitle
- **TimelineItem** (HeaderedContentControl)
  - Thuộc tính mở rộng: Time, Description, Icon, Type, DotBrush, IsLast
- **CardCarouselItem** (ContentControl)
  - Thuộc tính mở rộng: Scale, OffsetX, IsActive, SelectedIndex, ItemWidth, ItemHeight, Spacing, MinScale, AutoRun, Interval
- **OdometerNumber** (Control)
  - Thuộc tính mở rộng: DisplayText, DisplayValue, Value, Format, Prefix, Suffix, Duration, AnimateOnLoad, DecimalPlaces
- **Sparkline** (FrameworkElement)
  - Thuộc tính mở rộng: Values, Stroke, Fill, StrokeThickness, ShowArea, ShowLastDot, Smooth, LastDotBrush
- **AvatarStack** (FrameworkElement)
  - Thuộc tính mở rộng: Items, MaxVisible, Size, Overlap, Background, Foreground, RingBrush, FontSize, ShowOverflow, OverflowTextFormat
- **HeatmapCalendar** (FrameworkElement)
  - Thuộc tính mở rộng: Values, StartDate, CellSize, CellGap, CornerRadius, BaseBrush, Level1Brush, Level2Brush, Level3Brush, Level4Brush, ShowMonthLabels, MonthLabelBrush, MonthLabelFontSize

### Input & Form

- **NumericUpDown** (Control)
  - Thuộc tính mở rộng: CanIncrease, CanDecrease, Value, Minimum, Maximum, Increment, DecimalPlaces, Suffix, Prefix, FormatString
- **AutoCompleteTextBox** (TextBox)
  - Thuộc tính mở rộng: ItemsSource, SelectedItem, ItemTemplate, DisplayMemberPath, MaxDropDownHeight, IsDropDownOpen
- **RibbonTextBox** (RibbonItem)
  - Thuộc tính mở rộng: Text, PlaceholderText, IsReadOnly, MaxLength, TextAlignment
- **PasswordInput** (Control)
  - Thuộc tính mở rộng: Password, Placeholder, ShowPassword, IconKind
- **OtpInput** (Control)
  - Thuộc tính mở rộng: Value, Length
- **PinBox** (Control)
  - Thuộc tính mở rộng: Length, Password, PasswordChar, IsPassword, ItemWidth, ItemHeight, ItemMargin, ItemCornerRadius
- **RibbonComboBox** (RibbonItem)
  - Thuộc tính mở rộng: ItemsSource, SelectedItem, SelectedIndex, DisplayMemberPath, ItemTemplate, IsEditable, MaxDropDownHeight
- **SearchBox** (Control)
  - Thuộc tính mở rộng: Text, Placeholder, IsInstantSearch, SearchCommand
- **RibbonSearchBox** (RibbonItem)
  - Thuộc tính mở rộng: Text, Placeholder, IsInstantSearch, SearchCommand
- **FilePicker** (Control)
  - Thuộc tính mở rộng: SelectedPath, Filter, Title
- **ColorPicker** (Control)
  - Thuộc tính mở rộng: SelectedColor, HexCode, ShowAlpha
- **RibbonColorPicker** (RibbonItem)
  - Thuộc tính mở rộng: SelectedColor, IsDropDownOpen, ShowAlpha, SwatchBrush
- **RangeSlider** (Control)
  - Thuộc tính mở rộng: Minimum, Maximum, StartValue, EndValue, SmallChange, Unit
- **Rating** (Control)
  - Thuộc tính mở rộng: Value, Max, ItemSize, ItemMargin, IsReadOnly, ShowScore, ActiveBrush, InactiveBrush
- **Tag** (ContentControl)
  - Thuộc tính mở rộng: IsClosable, Icon, CornerRadius
- **DataGridGroupPanel** (Control)
  - Thuộc tính mở rộng: TargetGrid, EmptyMessage
- **ToggleSwitch** (ToggleButton)
  - Thuộc tính mở rộng: OnContent, OffContent

### Navigation

- **SideMenuItem** (DependencyObject)
  - Thuộc tính mở rộng: Icon, Title, Badge, HasBadge, IsSelected, Command, Children, Items, SelectedItem, IsCollapsed, Header, Footer
- **DrawerMenuItem** (DependencyObject)
  - Thuộc tính mở rộng: Icon, Text, Badge, HasBadge, IsSelected, Tag, Command, Children, IsOpen, CompactWidth, ExpandedWidth, SelectedItem, Footer, ShowToggleButton
- **BreadcrumbItem** (DependencyObject)
  - Thuộc tính mở rộng: Header, Icon, IsSelected, Command, Tag, Items, SeparatorIcon
- **Pagination** (Control)
  - Thuộc tính mở rộng: CurrentPage, MaxPage, TotalItems, PageSize
- **StepBarItem** (ContentControl)
  - Thuộc tính mở rộng: Index, Status, Description, StepIndex, Dock
- **Transfer** (Control)
  - Thuộc tính mở rộng: ItemsSource, TransferredItems, SourceTitle, TargetTitle, ItemTemplate, ListWidth, ListHeight, ShowSearch, ShowSelectAll

### Feedback & Status

- **Dialog** (ContentControl)
  - Thuộc tính mở rộng: IsOpen, Title, DialogWidth, DialogHeight, CornerRadius, MaskCanClose, ShowCloseButton, MessageType, Token
- **Drawer** (ContentControl)
  - Thuộc tính mở rộng: IsOpen, Placement, DrawerWidth, DrawerHeight, Title
- **DrawerMenuItem** (DependencyObject)
  - Thuộc tính mở rộng: Icon, Text, Badge, HasBadge, IsSelected, Tag, Command, Children, IsOpen, CompactWidth, ExpandedWidth, SelectedItem, Footer, ShowToggleButton
- **GrowlItem** (ContentControl)
  - Thuộc tính mở rộng: Message, GrowlType, TimeString, ShowCloseButton
- **LoadingOverlay** (ContentControl)
  - Thuộc tính mở rộng: IsLoading, Message, SubMessage, Progress, IsIndeterminate, IndicatorType
- **Skeleton** (ContentControl)
  - Thuộc tính mở rộng: IsLoading, SkeletonShape, CornerRadius, BaseBrush, HighlightBrush, Duration, IsWaveEnabled
- **EmptyState** (Control)
  - Thuộc tính mở rộng: Icon, Title, Description, ActionButtonText, ActionButtonIcon, ActionCommand, SecondaryActionText, ExtraContent
- **Watermark** (ContentControl)
  - Thuộc tính mở rộng: Text, Angle, Mode, MarkOpacity, MarkBrush, MarkFontSize

### Layout & Shell

- **SplitButton** (Control)
  - Thuộc tính mở rộng: Content, Icon, IconSize, IconPlacement, Command, CommandParameter, CommandTarget, DropDownContent, DropDownContentTemplate, IsDropDownOpen, CornerRadius, Placement, MaxDropDownHeight, HitMode, AutoCloseOnClick, StaysOpen, IsOpenOnHover, HoverDelay, DropDownButtonPosition
- **RibbonSplitButton** (RibbonItem)
  - Thuộc tính mở rộng: IsOpen, ArrowWidth
- **Expander** (HeaderedContentControl)
  - Thuộc tính mở rộng: IsExpanded, ExpandDirection, CornerRadius
- **Card** (HeaderedContentControl)
  - Thuộc tính mở rộng: Icon, CornerRadius, Footer, FooterTemplate
- **SpotlightCard** (ContentControl)
  - Thuộc tính mở rộng: CornerRadius, SpotlightBrush, SpotlightSize, SpotlightOpacity, IsSpotlightEnabled, BorderGradient, ShowHoverBorder
- **TiltCard** (ContentControl)
  - Thuộc tính mở rộng: MaxTilt, Lift, CornerRadius
- **CardCarouselItem** (ContentControl)
  - Thuộc tính mở rộng: Scale, OffsetX, IsActive, SelectedIndex, ItemWidth, ItemHeight, Spacing, MinScale, AutoRun, Interval
- **StatisticCard** (Control)
  - Thuộc tính mở rộng: Title, Value, Unit, Trend, TrendDirection, Icon, Progress, Subtitle
- **Badge** (ContentControl)
  - Thuộc tính mở rộng: Appearance, CornerRadius, HasBadge, Value, IsDot, ShowBadge, BadgeMargin, Status
- **Divider** (Control)
  - Thuộc tính mở rộng: Text, Orientation, Icon, LineStyle, TextAlignment
- **WaveDivider** (FrameworkElement)
  - Thuộc tính mở rộng: Amplitude, Wavelength, Speed, FillBrush, BackBrush, ShowBackWave, IsAnimated
- **CONCOSTWindow** (Window)
  - Thuộc tính mở rộng: TitleBarBackground
- **Ribbon** (Control)
  - Thuộc tính mở rộng: ItemsSource, SelectedIndex, SelectedItem, IsMinimized, ApplicationMenu, ShowQuickAccessToolBar, QuickAccessPlacement, MenuBar, MenuBarPlacement, ShowMenuBar, ShowApplicationMenu, ShowTabStrip, TabAlignment, SelectedTabId, IsGroupCollapseEnabled, IsKeyTipEnabled, IsQuickAccessCustomizable, ShowBorder, EmptyContent, IsTabStripOverflowing, IsEmpty, ShowGroupLabels, VisibleTabs
- **RibbonApplicationMenu** (RibbonDropDownButton)
  - Thuộc tính mở rộng: RecentItems, RecentHeader, FooterItems
- **RibbonButton** (RibbonItem)
  - Thuộc tính mở rộng: IsPressed
- **RibbonCheckBox** (RibbonToggleButton) — không có thuộc tính mở rộng riêng
- **RibbonColorPicker** (RibbonItem)
  - Thuộc tính mở rộng: SelectedColor, IsDropDownOpen, ShowAlpha, SwatchBrush
- **RibbonComboBox** (RibbonItem)
  - Thuộc tính mở rộng: ItemsSource, SelectedItem, SelectedIndex, DisplayMemberPath, ItemTemplate, IsEditable, MaxDropDownHeight
- **RibbonContextualTabGroup** (HeaderedItemsControl)
  - Thuộc tính mở rộng: Accent
- **RibbonDropDownButton** (RibbonItem)
  - Thuộc tính mở rộng: IsOpen
- **RibbonGallery** (RibbonItem)
  - Thuộc tính mở rộng: ItemsSource, ItemTemplate, ExpandedItemTemplate, SelectedItem, IsDropDownOpen, ShowExpandButton, ItemWidth, ItemHeight, MaxDropDownHeight, MaxDropDownWidth
- **RibbonGroup** (HeaderedItemsControl)
  - Thuộc tính mở rộng: DialogLauncherCommand, CollapsePriority, IsCollapsed, SizeMode, IsDropDownOpen, IsUserCollapsed, IsLastInStrip
- **RibbonGroupLauncher** (Button) — không có thuộc tính mở rộng riêng
- **RibbonGroupPanel** (Panel)
  - Thuộc tính mở rộng: ColumnWidth, RowHeight, RowGap, SeparatorWidth
- **RibbonGroupStrip** (Panel) — không có thuộc tính mở rộng riêng
- **RibbonItem** (HeaderedContentControl)
  - Thuộc tính mở rộng: Command, CommandParameter, CommandTarget, Icon, LargeIcon, SmallIcon, Size, ColumnSpan, IsFixedSize, ScreenTip, GestureText, Id, EffectiveIconSource, EffectiveSize
- **RibbonKeyTip** (ContentControl)
  - Thuộc tính mở rộng: Target
- **RibbonMenu** (ItemsControl) — không có thuộc tính mở rộng riêng
- **RibbonMenuBar** (ItemsControl)
  - Thuộc tính mở rộng: ShowDivider
- **RibbonMenuBarItem** (HeaderedItemsControl)
  - Thuộc tính mở rộng: IsSubmenuOpen
- **RibbonMenuItem** (HeaderedItemsControl)
  - Thuộc tính mở rộng: Command, CommandParameter, Icon, InputGestureText, IsSubmenuOpen, IsCheckable, IsChecked
- **RibbonMenuSeparator** (Control) — không có thuộc tính mở rộng riêng
- **RibbonQuickAccessButton** (RibbonButton) — không có thuộc tính mở rộng riêng
- **RibbonQuickAccessToolBar** (ItemsControl)
  - Thuộc tính mở rộng: IsPinned, PinnableItems, IsCustomizeOpen, IsCustomizable, ShowLabels, CustomizeMenu
- **RibbonRadioButton** (RibbonToggleButton) — không có thuộc tính mở rộng riêng
- **RibbonSearchBox** (RibbonItem)
  - Thuộc tính mở rộng: Text, Placeholder, IsInstantSearch, SearchCommand
- **RibbonSeparator** (Control) — không có thuộc tính mở rộng riêng
- **RibbonTabItem** (HeaderedContentControl)
  - Thuộc tính mở rộng: Icon, IsSelected, IsContextual, ContextualColor, ContextualGroup, IsAvailable, Id
- **RibbonTabStripPanel** (Panel)
  - Thuộc tính mở rộng: BandHeight, FontFamily, FontSize
- **RibbonTextBox** (RibbonItem)
  - Thuộc tính mở rộng: Text, PlaceholderText, IsReadOnly, MaxLength, TextAlignment
- **RibbonToggleButton** (RibbonButton)
  - Thuộc tính mở rộng: IsChecked, GroupName


### Effects & Decoration

- **Ripple** (ContentControl)
  - Thuộc tính mở rộng: RippleBrush, MaxOpacity, Duration, MaxConcurrentRipples, CornerRadius, IsRippleEnabled, Mode, IsEnabled, Brush, Progress
- **SpotlightCard** (ContentControl)
  - Thuộc tính mở rộng: CornerRadius, SpotlightBrush, SpotlightSize, SpotlightOpacity, IsSpotlightEnabled, BorderGradient, ShowHoverBorder
- **MarqueeTicker** (FrameworkElement)
  - Thuộc tính mở rộng: Text, Speed, Gap, FontSize, Foreground, Separator, IsRunning
- **AuroraBackground** (FrameworkElement)
  - Thuộc tính mở rộng: BlobCount, Speed, BlobOpacity, IsAnimated, AccentBrush, SecondaryBrush, TertiaryBrush
- **BorderBeam** (Decorator)
  - Thuộc tính mở rộng: CornerRadius, BeamBrush, BeamThickness, BeamLength, RevolutionSeconds, IsAnimated, SurfaceBrush, Padding
- **ConfettiBurst** (FrameworkElement)
  - Thuộc tính mở rộng: IsBursting, ParticleCount, Duration, Gravity, Spread, Origin, Palette
- **SheenSweep** (FrameworkElement)
  - Thuộc tính mở rộng: IsActive, Speed, BandWidth, Angle, PeakOpacity, SheenBrush, PauseSeconds
- **PulseRing** (FrameworkElement)
  - Thuộc tính mở rộng: RingCount, Duration, MaxRadius, RingThickness, RingBrush, PeakOpacity, IsActive, CenterDot, DotRadius
- **WaveDivider** (FrameworkElement)
  - Thuộc tính mở rộng: Amplitude, Wavelength, Speed, FillBrush, BackBrush, ShowBackWave, IsAnimated
- **GradientText** (FrameworkElement)
  - Thuộc tính mở rộng: Text, FontSize, FontWeight, StartColor, EndColor, Angle, Speed, IsAnimated
- **GridPattern** (FrameworkElement)
  - Thuộc tính mở rộng: CellSize, Pattern, DotRadius, StrokeThickness, Stroke, OffsetX, OffsetY, PatternOpacity
- **TiltCard** (ContentControl)
  - Thuộc tính mở rộng: MaxTilt, Lift, CornerRadius
- **CheckMarkDraw** (FrameworkElement)
  - Thuộc tính mở rộng: Progress, IsChecked, Stroke, StrokeThickness, Duration, BackgroundBrush, ShowCircle
- **ScrollProgressBar** (FrameworkElement)
  - Thuộc tính mở rộng: Target, TrackBrush, BarBrush, Thickness, CornerRadius, ShowTrack
- **ParallaxPanel** (ContentControl)
  - Thuộc tính mở rộng: Target, Speed, MaxOffset, Axis
- **Sprite** (ContentControl)
  - Thuộc tính mở rộng: IsOpen, MenuContent, AutoSnap, CornerRadius, ShowBorder
- **CompareSlider** (Control)
  - Thuộc tính mở rộng: SourceContent, TargetContent, Value, Orientation, HandleSize
- **Magnifier** (Control)
  - Thuộc tính mở rộng: Target, Scale, Radius, ShowCrosshair, Center, IsActive

### Basic Controls

- **SplitButton** (Control)
  - Thuộc tính mở rộng: Content, Icon, IconSize, IconPlacement, Command, CommandParameter, CommandTarget, DropDownContent, DropDownContentTemplate, IsDropDownOpen, CornerRadius, Placement, MaxDropDownHeight, HitMode, AutoCloseOnClick, StaysOpen, IsOpenOnHover, HoverDelay, DropDownButtonPosition
- **RibbonButton** (RibbonItem)
  - Thuộc tính mở rộng: IsPressed
- **RibbonDropDownButton** (RibbonItem)
  - Thuộc tính mở rộng: IsOpen
- **RibbonQuickAccessButton** (RibbonButton) — không có thuộc tính mở rộng riêng
- **RibbonRadioButton** (RibbonToggleButton) — không có thuộc tính mở rộng riêng
- **RibbonSplitButton** (RibbonItem)
  - Thuộc tính mở rộng: IsOpen, ArrowWidth
- **RibbonToggleButton** (RibbonButton)
  - Thuộc tính mở rộng: IsChecked, GroupName
- **RibbonCheckBox** (RibbonToggleButton) — không có thuộc tính mở rộng riêng
- **CompareSlider** (Control)
  - Thuộc tính mở rộng: SourceContent, TargetContent, Value, Orientation, HandleSize
- **RangeSlider** (Control)
  - Thuộc tính mở rộng: Minimum, Maximum, StartValue, EndValue, SmallChange, Unit
- **ScrollProgressBar** (FrameworkElement)
  - Thuộc tính mở rộng: Target, TrackBrush, BarBrush, Thickness, CornerRadius, ShowTrack
- **DataGridGroupPanel** (Control)
  - Thuộc tính mở rộng: TargetGrid, EmptyMessage
- **DrawerMenuItem** (DependencyObject)
  - Thuộc tính mở rộng: Icon, Text, Badge, HasBadge, IsSelected, Tag, Command, Children, IsOpen, CompactWidth, ExpandedWidth, SelectedItem, Footer, ShowToggleButton
- **SideMenuItem** (DependencyObject)
  - Thuộc tính mở rộng: Icon, Title, Badge, HasBadge, IsSelected, Command, Children, Items, SelectedItem, IsCollapsed, Header, Footer
- **RibbonApplicationMenu** (RibbonDropDownButton)
  - Thuộc tính mở rộng: RecentItems, RecentHeader, FooterItems
- **RibbonMenu** (ItemsControl) — không có thuộc tính mở rộng riêng
- **RibbonMenuBar** (ItemsControl)
  - Thuộc tính mở rộng: ShowDivider
- **RibbonMenuBarItem** (HeaderedItemsControl)
  - Thuộc tính mở rộng: IsSubmenuOpen
- **RibbonMenuItem** (HeaderedItemsControl)
  - Thuộc tính mở rộng: Command, CommandParameter, Icon, InputGestureText, IsSubmenuOpen, IsCheckable, IsChecked
- **RibbonMenuSeparator** (Control) — không có thuộc tính mở rộng riêng

### Time

- **CalendarWithClock** (Control)
  - Thuộc tính mở rộng: SelectedDateTime, ShowSeconds
- **Clock** (Control)
  - Thuộc tính mở rộng: SelectedTime, DisplayMode, ShowSeconds

### Khác / Internal

- **InsertionAdorner** (Adorner)
  - Thuộc tính mở rộng: IsDragSource, IsDropTarget, DragDropGroup
- **RulerControl** (Control)
  - Thuộc tính mở rộng: Orientation, UnitScale, MajorStep, OriginOffset
- **PieSegmentModel** (DependencyObject)
  - Thuộc tính mở rộng: Name, Value, Color, Datas, Title, Unit, ShowLegend, BarBrush, MultiColor, LineBrush, ShowArea, Smooth, RadarBrush, GridLevels, Segments, ArcThickness, CenterText
- **ColumnNode** (DependencyObject)
  - Thuộc tính mở rộng: HeaderText, HeaderTemplate, Foreground, Background, ToolTip, HorizontalAlignment, BindingPath, SortMemberPath, CanUserSort, CanUserResize, Width, MinWidth, MaxWidth, CellType, StringFormat, AggregateType, AggregateFormat, CustomAggregateText, ChildColumns, RowHeight, ColumnHeaderTree, StackedHeaderRows, StackedRowHeight, ShowFooterSummary, FooterRowHeight, EnableContextMenu
- **LoadingBase** (Control)
  - Thuộc tính mở rộng: IsLoading, DotSize, Value, LoadTitle
- **LoadingIndicator** (Control)
  - Thuộc tính mở rộng: IndicatorType, DotSize
- **UnitBox** (Control)
  - Thuộc tính mở rộng: Value, Unit, DecimalPlaces, Header
- **ConditionalFormattingRule** (DependencyObject)
  - Thuộc tính mở rộng: Operator, Value1, Value2, Background, Foreground
- **LengthBox** (UnitBox)
- **Icon** (Control)
  - Thuộc tính mở rộng: Kind, Data
- **NavigationItem** (HeaderedContentControl)
  - Thuộc tính mở rộng: Icon, IsSelected, TagName, SelectedItem
- **ThemeTransitionAdorner** (Adorner)
  - Thuộc tính mở rộng: Progress

## 3. THUỘC TÍNH MỞ RỘNG CHUNG (Attached Properties)

Các thuộc tính sau có thể áp dụng cho **hầu hết mọi Control** (thông qua attached properties):

### BorderElement
- `CornerRadius`
- `Circular`
- `HoverBorderBrush`
- `HoverBackground`

### InfoElement
- `Placeholder`
- `Title`
- `TitlePlacement`
- `Required`
- `Prefix`
- `Suffix`
- `ShowClearButton`
- `IsClearButtonAlwaysVisible`
- `ShowCharacterCount`

### IconElement
- `Kind`
- `IconSize`
- `Placement`

### ButtonElement
- `IsLoading`
- `LoadingText`
- `Badge`

### VisualElement
- `Elevation`
- `HoverScale`

### ProgressBarElement
- `ShowText`
- `TextPattern`

### DataGridElement
- `RowSpacing`
- `ShowRowDivider`
- `HeaderHorizontalContentAlignment`
- `CellHorizontalContentAlignment`

### DragDropHelper
- `IsDragSource`
- `IsDropTarget`
- `DragDropGroup`

### Hiệu ứng gắn kèm (Behavior Attached)

- `Ripple.IsEnabled`, `Ripple.Brush`, `Ripple.CornerRadius`, `Ripple.Mode`, `Ripple.MaxOpacity`, `Ripple.Duration`, `Ripple.MaxConcurrentRipples` — hiệu ứng gợn sóng khi click
- `MagneticHover.IsEnabled`, `MagneticHover.Strength`, `MagneticHover.MaxOffset` — hiệu ứng từ tính khi hover
- `ScrollReveal.IsEnabled`, `ScrollReveal.Offset`, `ScrollReveal.Duration`, `ScrollReveal.Delay`, `ScrollReveal.Threshold` — hiệu ứng lộ dần khi scroll
- `ShakeElement.IsShaking`, `ShakeElement.Amplitude`, `ShakeElement.Duration` — hiệu ứng rung

## 4. HIỆU ỨNG CHUNG CHO CÁC CONTROL

Những hiệu ứng này được thiết kế để áp dụng cho **bất kỳ control nào** (thông qua attached properties hoặc wrap):

| Hiệu ứng | Mô tả | Cách dùng |
|---|---|---|
| **Ripple** | Gợn sóng lan tỏa khi click | `ripple:Ripple.IsEnabled="True"` |
| **MagneticHover** | Control bị hút về phía chuột khi hover | `magnetic:MagneticHover.IsEnabled="True"` |
| **ScrollReveal** | Phần tử lộ dần khi scroll vào viewport | `scroll:ScrollReveal.IsEnabled="True"` |
| **ShakeElement** | Rung ngang khi có lỗi hoặc cảnh báo | `shake:ShakeElement.IsShaking="True"` |
| **CursorGlow** | Hiệu ứng hào quang theo con trỏ chuột | Dùng như overlay panel |
| **SheenSweep** | Quét sáng ngang qua control | Dùng như overlay |

## 5. HIỆU ỨNG TRANG TRÍ (DECORATION EFFECTS)

Các hiệu ứng này thường dùng như **background, overlay hoặc standalone visual**:

- **AuroraBackground**: BlobCount, Speed, BlobOpacity, IsAnimated, AccentBrush, SecondaryBrush, TertiaryBrush
- **BorderBeam**: CornerRadius, BeamBrush, BeamThickness, BeamLength, RevolutionSeconds, IsAnimated, SurfaceBrush, Padding
- **CheckMarkDraw**: Progress, IsChecked, Stroke, StrokeThickness, Duration, BackgroundBrush, ShowCircle
- **CompareSlider**: SourceContent, TargetContent, Value, Orientation, HandleSize
- **ConfettiBurst**: IsBursting, ParticleCount, Duration, Gravity, Spread, Origin, Palette
- **GradientText**: Text, FontSize, FontWeight, StartColor, EndColor, Angle, Speed, IsAnimated
- **GridPattern**: CellSize, Pattern, DotRadius, StrokeThickness, Stroke, OffsetX, OffsetY, PatternOpacity
- **LoadingOverlay**: IsLoading, Message, SubMessage, Progress, IsIndeterminate, IndicatorType
- **Magnifier**: Target, Scale, Radius, ShowCrosshair, Center, IsActive
- **MarqueeTicker**: Text, Speed, Gap, FontSize, Foreground, Separator, IsRunning
- **ParallaxPanel**: Target, Speed, MaxOffset, Axis
- **PulseRing**: RingCount, Duration, MaxRadius, RingThickness, RingBrush, PeakOpacity, IsActive, CenterDot, DotRadius
- **ScrollProgressBar**: Target, TrackBrush, BarBrush, Thickness, CornerRadius, ShowTrack
- **SpotlightCard**: CornerRadius, SpotlightBrush, SpotlightSize, SpotlightOpacity, IsSpotlightEnabled, BorderGradient, ShowHoverBorder
- **Sprite**: IsOpen, MenuContent, AutoSnap, CornerRadius, ShowBorder
- **TiltCard**: MaxTilt, Lift, CornerRadius
- **WaveDivider**: Amplitude, Wavelength, Speed, FillBrush, BackBrush, ShowBackWave, IsAnimated
- **TextReveal**: Text, FontSize, FontWeight, Foreground, WordDelay, WordDuration, OffsetY, StartDelay
- **TypewriterText**: Text, CharsPerSecond, StartDelay, ShowCaret, CaretBrush, FontSize, FontWeight, Foreground, IsLooping, RestartDelay

## 6. SỐ LIỆU TỔNG KẾT

| Hạng mục | Số lượng |
|---|---|
| Control sử dụng trực tiếp | 139 ở bản 1.1.0, + 4 ở mục 8, + 9 ở mục 9, + 17 ở mục 10 → **169** |
| Hiệu ứng / Decoration | 30 |
| Attached Property classes (chung) | 7 |
| Behavior effects (gắn kèm) | 4 |
| Service / Manager riêng | 9 (liệt kê ở mục 10) |

## 7. BỔ SUNG 1.1.0 — 30 CONTROL

Nhóm control thêm trong bản 1.1.0: docking, dữ liệu phân cấp, tiện ích kéo thả và hiệu ứng.
Thuộc tính dưới đây đọc trực tiếp từ source.

### Layout & Shell

- **DockingManager**
  - Thuộc tính: SplitterThickness, AutoHideStripSize, ShowDropPreview, SplitterBrush, IsProportional
- **ToolWindow**
  - Thuộc tính: Header, Icon, ShowHeader, Dock, DockSize, MinDockSize, IsAutoHidden, IsFlyoutOpen, CanClose, CanAutoHide, IsActive
- **DockPaneGroup**
  - Thuộc tính: Orientation, Mode, Dock, DockSize, MinDockSize
  - Attached: Proportion
- **DockTabStrip**
  - Thuộc tính: TabPlacement, Panes
- **AutoHidePane**
  - Thuộc tính: Panes, Side, TabSize
- **DockSplitter**
  - Thuộc tính: Orientation, Thickness, Value, Minimum, Maximum, LineBrush
- **DockPreview**
  - Thuộc tính: IsOpen, Side, PreviewBrush, BorderBrush
- **DocumentTabControl**
  - Thuộc tính: ShowCloseButtons
- **DocumentTabItem**
  - Thuộc tính: Icon, IsDirty, CanClose, CloseCommand
- **StatusBar**
  - Thuộc tính: Spacing
- **StatusBarItem**
  - Thuộc tính: Icon, Text, IsSeparator, Alignment, Command, CommandParameter
- **StatusBarPanel**
  - Thuộc tính: Spacing
- **CommandBar**
  - Thuộc tính: OverflowToolTip
- **QuickAccessToolbar**
  - Thuộc tính: IsCustomizable
- **OverflowPanel**
  - Thuộc tính: OverflowCount, Spacing

### Data & Dashboard

- **TreeDataGrid**
  - Thuộc tính: RootItems, ChildrenPath, ShowHierarchyColumn, HierarchyHeader, HierarchyColumnWidth, IndentSize, HierarchyContentTemplate, HierarchyPath, VisibleItemCount
  - Attached: Level, IndentWidth, HasChildren, IsExpanded
- **MultiSelectDataGrid**
  - Thuộc tính: ShowCheckBoxColumn, CheckBoxColumnWidth, SelectedCount
- **TreeMap**
  - Thuộc tính: ItemsSource, ValuePath, LabelPath, Palette, Fill, Gap, CornerRadius, ShowLabels, ShowValues, ValueFormat, LabelForeground, FontSize
- **DataGridColumnChooser**
  - Thuộc tính: TargetGrid, IsOpen, Columns, HeaderText, CornerRadius
- **DataGridFilterPanel**
  - Thuộc tính: TargetGrid, Filters, PlaceholderText
- **DataGridSearchPanel**
  - Thuộc tính: TargetGrid, SearchText, PlaceholderText, IsSearching
- **DataGridSummary**
  - Thuộc tính: TargetGrid, Summaries, Aggregate, DisplayFormat, RowHeight
  - Attached: AggregateOnColumn

### Input & Form

- **SearchableTreePicker**
  - Thuộc tính: ItemsSource, ChildrenPath, DisplayPath, SearchText, IsSearchEnabled, Placeholder, IsExpandingMatches, EmptyText, SelectionMode, SelectedItem, SelectedItems, SelectedCount
- **FileDropZone**
  - Thuộc tính: Filter, AllowMultiple, Caption, Hint, IsDragOver, IsDragRejected
- **DropTargetHighlight**
  - Attached: IsEnabled, Brush, CornerRadius

### Effects & Decoration

- **AnimatedMeshGradient**
  - Thuộc tính: ColorA, ColorB, ColorC, ColorD, Speed, BlobOpacity, Spread, Background
- **AnimatedGradientBorder**
  - Thuộc tính: ColorA, ColorB, ColorC, RingThickness, Corner, SecondsPerTurn, IsAnimated, Fill
- **FrostedGlass**
  - Thuộc tính: Source, BlurRadius, Tint, Corner, EdgeBrush
- **CursorSpotlight**
  - Thuộc tính: SpotDim, Radius, Feather, DimColor, SpotColor, FollowCursor, SpotlightCenter
- **StaggerAnimation**
  - Attached: IsEnabled, Delay, Duration, OffsetY, MaxItems

### Helper tĩnh (không có DependencyProperty)

- **DragGhost** — cửa sổ ghost theo con trỏ: `Show(caption, icon)`, `Move(visual, point)`, `Hide()`, `IsVisible`
- **SharedElementTransition** — `Run(element, fromRect, ms)`, `RunFrom(element, origin, ms)`

## 8. BỔ SUNG 1.2.0 — 4 CONTROL

Bốn mục còn lại của mục 107 trong roadmap: bố cục waterfall, hộp thẻ, chữ ký tay và wizard nhiều bước.

### Layout & Form

- **MasonryPanel** (Panel)
  - Thuộc tính: Columns, MinColumnWidth, ColumnSpacing, RowSpacing, IsBalanced
  - Phương thức: `GetColumnCount(width)`, `GetColumnWidth(width)`
- **TagInput** (Control)
  - Thuộc tính: ItemsSource, DisplayPath, SuggestionSource, Text, Placeholder, AllowFreeText, AllowDuplicates, MaxTags, Delimiter, IsSuggestionOpen
  - Phương thức: `AddTag(item)`, `RemoveTag(entry)`, `ClearTags()`, `CommitText()`; sự kiện `TagsChanged`
  - Item: **TagEntry** (`Item`, `Text`), **TagChangedEventArgs** (`Item`, `Text`, `IsAdded`)
- **Wizard** (Control) + **WizardStep** (HeaderedContentControl)
  - Wizard: CurrentIndex, CurrentStep, CanGoBack, CanGoNext, IsLastStep, ShowHeader, ShowFooter, BackText, NextText, FinishText
  - WizardStep: Header, Description, IsValid, Index, Number, IsCurrent, IsCompleted
  - Phương thức: `Next()`, `Back()`, `GoTo(index)`, `Finish()`, `RefreshState()`; sự kiện `Navigating`, `StepChanged`, `Finished`

### Engineering

- **SignaturePad** (FrameworkElement)
  - Thuộc tính: StrokeBrush, StrokeThickness, Fill, EdgeBrush, CornerRadius, ShowGuideline, Placeholder, PlaceholderBrush, HasSignature, SignatureImage
  - Phương thức: `Undo()`, `Clear()`, `Export(width, height, background)`; sự kiện `SignatureChanged`
  - Ink: `Strokes` (`StrokeCollection`) — lưu/đọc được bằng Ink Serialized Format

---

## 9. BỔ SUNG 1.2.0 — 9 CONTROL (mục 107)

Chín mục còn lại của mục 107 trong roadmap: lịch công tác, bảng kanban, khung xem ảnh, soạn mã,
markdown, coach mark, minimap, mô hình 3D và splash. Thuộc tính dưới đây đọc trực tiếp từ source.

### Data & Dashboard

- **Scheduler** (FrameworkElement)
  - Thuộc tính: ItemsSource, Date, StartHour, EndHour, HourHeight, SelectedItem, ItemBrush, ShowNowLine
  - Phương thức: `SelectItem(item)`, `GetItemBounds(item)`, `ChangeDay(days)`
  - Sự kiện: `SelectionChanged`, `ItemActivated`
  - Item: **SchedulerItem** — `Title`, `Start`, `End`, `Resource`, `Accent`, `Tag`, `Duration`
- **KanbanBoard** (Control)
  - Thuộc tính: Columns (`ObservableCollection<KanbanColumn>`)
  - Phương thức: `MoveCard(card, target, index)`, `ColumnOf(card)`; sự kiện `CardMoved`
  - **KanbanColumn** (HeaderedItemsControl): Accent, IsDropTarget, Maximum
  - **KanbanCard** (ContentControl): Title, Chip, Accent, IsDone

### Media & Editing

- **ImageViewer** (FrameworkElement)
  - Thuộc tính: Source, Zoom, Rotation, Offset, MinZoom, MaxZoom, ShowCheckerboard
  - Phương thức: `Fit()`, `Reset()`, `Centre()`, `RotateClockwise()`, `ZoomAt(anchor, factor)`, `GetImageBounds()`
- **CodeEditor** (Control)
  - Thuộc tính: Text, Syntax, IsReadOnly, HighlightSyntax, ShowLineNumbers, TabSize, CaretLine
  - Phương thức: `Refresh()`, `GoToLine(line)`, `FindNext(term, matchCase)`; tĩnh `Tokenize(text, language)`
- **MarkdownViewer** (Control)
  - Thuộc tính: Markdown; `Document` (read-only, `FlowDocument` đã parse, bind được)
  - Phương thức: `Rebuild()`; helper tĩnh **MarkdownParser** — `Parse(markdown)`
- **ModelViewer3D** (FrameworkElement)
  - Thuộc tính: Vertices, Edges, Yaw, Pitch, Zoom, Pan, ShowAxes, EdgeBrush
  - Phương thức: `SetMesh(vertices, edges)`, `LoadBox(corner, w, d, h)`, `ResetView()`, `Orbit(dx, dy)`, `Project(point, out screen)`
  - Struct: **MeshEdge** (`A`, `B`)

### Feedback & Overlay

- **CoachMark** (Control)
  - Thuộc tính: Target, Placement, Title, StepIndex, StepCount, CardWidth, Gap, OverlayBrush
  - Phương thức: `Advance()`, `Dismiss()`, `ComputeCardRect(host)`
  - Sự kiện: `Next`, `Dismissed`; enum `CoachMarkPlacement`
- **MiniMap** (FrameworkElement)
  - Thuộc tính: ContentBounds, Viewport, ItemsSource, ItemBrush, ViewportBrush
  - Phương thức: `WorldToMap(point)`, `MapToWorld(point)`, `CentreOn(world)`, `Fit()`; sự kiện `ViewportChanged`
  - Item: **MiniMapItem** (`Bounds`, `Brush`); args: `MiniMapViewportChangedEventArgs` (`Viewport`, `IsFinal`)
- **SplashScreen** (Control)
  - Thuộc tính: AppName, Version, Status, Progress, ShowProgress, IsIndeterminate, Logo, Tagline
  - Phương thức: `Show()`, `Report(progress, status)`, `Dismiss(animate)`; sự kiện `Dismissed`

## 10. BỔ SUNG 1.2.0 — CAD SHELL (TT-UI-SPEC-001)

Phần còn lại của spec 135 mục: khung nhìn, điều hướng, properties, project browser, document
workspace, dòng lệnh, status bar, các service và chrome. Bảng đối chiếu từng mục nằm ở
`docs/roadmap/revit-spec-gap.md`; dưới đây là danh sách control và API.

### Viewport & điều hướng

- **ViewportControl** (Panel)
  - Thuộc tính: ShowGrid, GridStep, ShowCrosshair, SnapToGrid, SnapDistance, SelectionRectangle, Snap, SnapKind, TemporaryDimension, Grips
  - `Overlay` — `Canvas` phủ theo toạ độ màn hình, đã tắt hit-test
  - Phương thức: `ToWorld(point)`, `ToScreen(point)`, `UnitsToPixels(...)`, `ZoomToFit()`, `ToggleGrid()`
  - Sự kiện: `CursorMoved`, `WorldClicked`
- **ViewCube** (FrameworkElement) — khối vát cạnh 26 vùng bấm, vẽ bằng `OnRender`
  - Thuộc tính: Host, Rotation, HomeRotation, FrontAngle, Theme, Visible, Position, DockMargin, Size, InactiveOpacity, ShowCompass, SnapToClosestView, SnapAngle, FitToViewOnChange, AnimatedTransitions, AnimationDuration, KeepSceneUpright, ClickThreshold
  - Phương thức: `GoTo(region)`, `GoHome()`, `SetCurrentViewAsHome()`, `SetCurrentViewAsFront()`, `ResetFront()`, `RollBy(quarterTurns)`, `StepToNeighbor(direction)`, `SetProjection(mode)`, `Invalidate()`, `HitTest(point)`
  - Sự kiện: `ViewRequested` (huỷ được và đổi được `Target`), `RegionHovered`, `ViewTransitionCompleted`, `HomeChanged`, `FrontChanged`, `ProjectionModeChanged`, `OptionsRequested`
  - Kiểu phụ: `ViewCubePart`, `ViewCubeRegion` / `ViewCubeRegionKind`, `ViewCubeHit` (Part, Region, Arrow, Roll), `ICameraHost` / `ICameraHostEvents`, `ProjectionMode`, `ViewCubeMath` (bảng 26 vùng, `RotationFor`, `Slerp`, `Roll`)
- **NavigationBar** (Control)
  - Thuộc tính: ActiveTool, Ucs, ShowWalk
  - Phương thức: `SetActiveTool(tool)`
  - Sự kiện: `NavigationRequested`, `ActiveToolChanged`, `UcsChanged`
  - Enum: `NavigationCommand`, `NavigationTool` (None/Pan/Orbit/Walk), `UcsMode` (World/Top/Front/Right)
- **ViewControlBar** (Control)
  - Thuộc tính: ViewScale, DetailLevel, VisualStyle, ShowShadows, CropView, RevealHidden
  - Sự kiện: `SettingsChanged` (kèm tên thuộc tính vừa đổi), `HideIsolateRequested`
  - Enum: `ViewDetailLevel`, `VisualStyleMode`, `HideIsolateAction`

### Properties

- **PropertiesPalette** (Control) — panel ghép, không có DependencyProperty riêng
  - Thuộc tính CLR: `GridControl`, `Properties`, `Selection`, `Target`, `ValidationMessage`
  - Phương thức: `Refresh()`, `ResetProperty(...)`, `Build()`
  - Sự kiện: `EditTypeRequested`, `TypeChangeRequested`, `PropertyCommitted`
- **PropertyReferencePicker** (Control)
  - Thuộc tính: SelectedItem, Placeholder, IsReadOnly
  - Phương thức: `TogglePopup()`; sự kiện `ReferencePicked`

### Project browser & document workspace

- **ProjectBrowser** (Control) — panel ghép
  - Thuộc tính CLR: `Nodes`, `SelectedNode`, `GroupMode`, `SortMode`, `KindFilter`, `FilterText`, `VisibleNodes`
  - Phương thức: `AddRoot(node)`, `SetSearchText(text)`, `Refresh()`, `ExpandAll()`, `CollapseAll()`, `Select(node)`, `Open(node)`
  - Sự kiện: `OpenRequested`, `RenameRequested`, `DeleteRequested`, `DuplicateRequested`, `PropertiesRequested`, `SelectionChanged`
  - **ProjectTreeNode**: `Kind` (`ProjectNodeKind`), `Parent`, `Children`, `IsModified`
  - Enum: `ProjectNodeKind`, `ProjectGroupMode`, `ProjectSortMode`
- **DocumentWorkspace** (Control) — panel ghép
  - Thuộc tính CLR: `Documents`, `ActiveDocument`, `TileMode`, `PaneCount`, `PlaceholderTitle`, `PlaceholderText`
  - Phương thức: `AddDocument(...)`, `CloseDocument(...)`, `CloseOthers(...)`, `CloseAll()`, `FloatDocument(pane)`
  - Sự kiện: `DocumentClosing`, `DocumentAdded`, `DocumentClosed`, `ActiveDocumentChanged`, `DocumentFloated`
  - **DocumentPane**: `IsDirty`; tile mode dùng `GridSplitter` giữa các pane, `DocumentTileMode` = Single/Vertical/Horizontal/Grid

### Dòng lệnh & trạng thái

- **CommandLine** (Control)
  - Thuộc tính: Prompt, HistoryHeight, ShowHistory, MaxHistory
  - Phương thức: `AddCommand(def)`, `AddCommands(defs)`, `Submit()`, `BeginPrompt(text)`, `CancelPrompt()`, `Complete()`, `WriteLine(text, kind)`, `Clear()`, `FocusInput()`, `Resolve(text)`
  - Sự kiện: `CommandInvoked`, `UnknownCommand`, `PromptCompleted`, `PromptCancelled`
  - **CommandDefinition**: `Name`, `Description`, `Prompt`, `Keys`; `WithAliases(...)`
  - Helper tĩnh **CadInputParser**: `TryParseLength(text, out mm)`, `TryParseAngle(text, out deg)`, `TryParseCoordinate(text, out point)`
- **CadStatusBar** (Control)
  - Thuộc tính: SelectionCount, WorkPlane, Coordinates, UnitSuffix, ShowCoordinates, SnapEnabled, GridEnabled, OrthoEnabled, PolarEnabled, ObjectSnapEnabled
  - Phương thức: `SetToggle(toggle, on)`, `IsToggleEnabled(toggle)`, tĩnh `FormatCoordinates(point, unit)`, `SelectionSummary(count)`, `AddTask(...)`, `RemoveTask(...)`, `Notify(...)`, `UpdateCoordinates(...)`, `RefreshTasks()`, `RefreshNotifications()`
  - Sự kiện: `ToggleChanged`, `WorkPlaneChanged`, `TaskSelected`, `NotificationSelected`
  - Enum: `CadToggle` (Snap/Grid/Ortho/Polar/ObjectSnap), `CadNotificationSeverity`
- **DynamicInput** (Grid)
  - Thuộc tính: Prompt, Text, Anchor, Offset, Kind, IsOpen; `IsValid`, `ValidationMessage` (read-only)
  - Phương thức: `Open(anchor, prompt)`, `MoveTo(anchor)`, `Commit()`, `Cancel()`, tĩnh `TryParse(kind, text, out number, out point)`
  - Sự kiện: `ValueCommitted`, `Cancelled`; enum `DynamicInputKind` (Distance/Angle/Coordinate/Text)

### Chrome

- **ApplicationTitleBar** (Control): ApplicationName, DocumentTitle, IsDocumentModified, ShowQuickAccess, ShowWindowButtons, IsMaximized; `ComposeTitle(...)`, `RefreshQuickAccess()`; sự kiện `MinimizeRequested`, `MaximizeRequested`, `CloseRequested`
- **ApplicationInfoBar** (Control): Severity, Title, Message, ActionText, IsClosable; `Show(...)`, `Hide()`; sự kiện `ActionInvoked`, `Closed`
- **RibbonBackstageView** (Control): IsOpen, ApplicationName; `Open()`, `Close()`, `Select(item)`, `RebuildRail()`; sự kiện `IsOpenChanged`, `SelectedItemChanged`
- **FontPicker** (Control): SelectedFontFamily, SelectedFontSize, IsBold, IsItalic, ShowPreview, PreviewText; sự kiện `SelectionChanged`

### Service / Manager

| Service | API chính |
|---|---|
| **CameraController** | `Orbit`, `Pan`, `Zoom`, `ZoomAt`, `SetOrientation`, `LookAt`, `FitTo`, `TryProject`, `Unproject`, `VisibleHeight(distance)`, `CopyTo`; sự kiện `Changed` |
| **SelectionManager** | `Select`, `SelectMany`, `Add`, `Remove`, `Toggle`, `Clear`, `Contains`; `SelectionChanged` |
| **TransactionManager** | `Begin`, `Commit`, `Rollback`, `Record`, `Execute`, `Run`, `Undo`; `Committed`, `RolledBack`, `Changed` |
| **PropertyManager** | `Register`, `GetDescriptors`, `Find`, `GetValue`, `TrySetValue`, `TrySetValueFromText`, `Validate`, `TryConvert`; `ValueChanged` |
| **ApplicationStateService** | `Toggle`, `Attach`, `Detach`, `CaptureToggles`, `ApplyToggles`; `ToggleChanged`, `WorkPlaneChanged`, `CursorMoved`, `StatusChanged`, `ModifiedChanged` |
| **DocumentManager** | `Open`, `Close`, `CloseAll`, `SetDirty`, `BuildTitle`; `DocumentOpened`, `DocumentClosed`, `DocumentClosing`, `ActiveDocumentChanged`, `DirtyChanged` |
| **ViewManager** | `Add`, `Find`, `Open`, `Close`, `Capture`, `ApplyTo`, `RememberCamera`, `RestoreCamera`; `OpenRequested`, `ViewClosed`, `ActiveViewChanged` |
| **WorkspaceLayoutService** | `Attach`, `Capture`, `Save`, `Apply`, `Find`, `Remove`, `Persist`, `ToToggles`, `FromToggles`; `ProfileApplied`, `ProfilesChanged` |
| **RendererBridge** | `Adapter`, `Invalidate`, `TryProject`, `TryUnproject`, `TryPick`, `UpdateCursor`; `RenderRequested`, `HoverChanged` |

Trạng thái đối chiếu spec: **132/135 mục (98%) đạt, 3 mục một phần, 0 mục thiếu**; 11/12 service và
20/20 tiêu chí chấp nhận. Bốn mục một phần — DocumentTabContextMenu, ViewportHost, ContextMenuControl
và CommandManager — được giải thích trong `docs/roadmap/revit-spec-gap.md`.

---
*Báo cáo được duy trì bằng tay; mục 7–10 sinh một phần từ source bằng `docs/tools/build-props.py`.*