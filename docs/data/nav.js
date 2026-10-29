// Sidebar structure. Order here defines prev/next navigation and the command palette.
const DOCS_NAV = [
    {
        title: "Bắt đầu", titleEn: "Getting started",
        items: [
            { id: "intro", name: "Giới thiệu & Cài đặt", nameEn: "Introduction & Setup", icon: "fa-book-open" },
            { id: "class-library", name: "Plugin DLL (Revit/AutoCAD)", nameEn: "Plugin DLL (Revit/AutoCAD)", icon: "fa-puzzle-piece" },
            { id: "theming", name: "Theme & Design Tokens", icon: "fa-palette" },
            { id: "attached-props", name: "Attached Properties", icon: "fa-wand-magic-sparkles" },
            { id: "examples", name: "Ví dụ sử dụng", nameEn: "Usage examples", icon: "fa-code", badge: "Mới", badgeEn: "New" },
            { id: "faq", name: "FAQ & Troubleshooting", icon: "fa-circle-question" },
            { id: "migration", name: "Chuyển từ WinForms/WPF", nameEn: "Migrating from WinForms/WPF", icon: "fa-code-branch" }
        ]
    },
    {
        title: "Engineering & CAD",
        items: [
            { id: "spreadsheet", name: "Spreadsheet", icon: "fa-table-cells" },
            { id: "stackedheaderdatagrid", name: "StackedHeaderDataGrid", icon: "fa-table-columns" },
            { id: "cadcanvas", name: "ZoomableCanvas & Ruler", icon: "fa-ruler-combined" },
            { id: "diagramchart", name: "ChartLine (nội lực)", nameEn: "ChartLine (internal forces)", icon: "fa-chart-area" },
            { id: "charts", name: "Charts Suite", icon: "fa-chart-pie" },
            { id: "rebarinput", name: "RebarInput", icon: "fa-cubes-stacked" },
            { id: "engineeringboxes", name: "Engineering Boxes", icon: "fa-screwdriver-wrench" },
            { id: "compassindicator", name: "CompassIndicator", icon: "fa-compass" }
        ]
    },
    {
        title: "Dữ liệu & Dashboard", titleEn: "Data & Dashboard",
        items: [
            { id: "datapager", name: "DataPager", icon: "fa-table-list" },
            { id: "statisticcard", name: "StatisticCard (KPI)", icon: "fa-chart-line" },
            { id: "timeline", name: "Timeline", icon: "fa-timeline" },
            { id: "cardcarousel", name: "CardCarousel", icon: "fa-cubes" },
            { id: "propertygrid", name: "PropertyGrid", icon: "fa-sliders" }
        ]
    },
    {
        title: "Nhập liệu & Form", titleEn: "Input & Form",
        items: [
            { id: "numericupdown", name: "NumericUpDown", icon: "fa-arrow-up-1-9" },
            { id: "textbox", name: "TextBox", icon: "fa-pen-to-square" },
            { id: "passwordinput", name: "PasswordInput", icon: "fa-lock", badge: "Mới", badgeEn: "New" },
            { id: "otpinput", name: "OtpInput", icon: "fa-shield-halved", badge: "Mới", badgeEn: "New" },
            { id: "pinbox", name: "PinBox", icon: "fa-key" },
            { id: "checkcombobox", name: "CheckComboBox", icon: "fa-square-check" },
            { id: "combobox", name: "ComboBox", icon: "fa-caret-down" },
            { id: "autocompletetextbox", name: "AutoCompleteTextBox", icon: "fa-list-check" },
            { id: "searchbox", name: "SearchBox", icon: "fa-magnifying-glass" },
            { id: "filepicker", name: "FilePicker", icon: "fa-folder-open" },
            { id: "colorpicker", name: "ColorPicker", icon: "fa-palette" },
            { id: "rangeslider", name: "RangeSlider", icon: "fa-sliders" },
            { id: "rating", name: "Rating", icon: "fa-star" },
            { id: "tag", name: "Tag / Chip", icon: "fa-tag" },
            { id: "toggleswitch", name: "ToggleSwitch", icon: "fa-toggle-on" }
        ]
    },
    {
        title: "Điều hướng", titleEn: "Navigation",
        items: [
            { id: "navigationview", name: "NavigationView", icon: "fa-sitemap", badge: "Mới", badgeEn: "New" },
            { id: "sidemenu", name: "SideMenu", icon: "fa-bars" },
            { id: "drawermenu", name: "DrawerMenu", icon: "fa-table-columns" },
            { id: "breadcrumb", name: "Breadcrumb", icon: "fa-chevron-right" },
            { id: "pagination", name: "Pagination", icon: "fa-file-lines" },
            { id: "stepbar", name: "StepBar", icon: "fa-list-ol" },
            { id: "transfer", name: "Transfer", icon: "fa-right-left" },
            { id: "tabcontrol", name: "TabControl", icon: "fa-folder" }
        ]
    },
    {
        title: "Phản hồi & Trạng thái", titleEn: "Feedback & Status",
        items: [
            { id: "dialog", name: "Dialog", icon: "fa-window-maximize" },
            { id: "drawer", name: "Drawer", icon: "fa-bars-staggered" },
            { id: "growl", name: "Growl (Toast)", icon: "fa-bell" },
            { id: "loadings", name: "Loading Indicators", icon: "fa-spinner", badge: "Mới", badgeEn: "New" },
            { id: "loadingoverlay", name: "LoadingOverlay", icon: "fa-circle-notch", badge: "Mới", badgeEn: "New" },
            { id: "skeleton", name: "Skeleton (Shimmer)", icon: "fa-ghost", badge: "Mới", badgeEn: "New" },
            { id: "emptystate", name: "EmptyState", icon: "fa-inbox" },
            { id: "watermark", name: "Watermark", icon: "fa-copyright" }
        ]
    },
    {
        title: "Layout & Shell",
        items: [
            { id: "splitbutton", name: "SplitButton", icon: "fa-angles-right", badge: "Mới", badgeEn: "New" },
            { id: "expander", name: "Expander", icon: "fa-angles-down", badge: "Mới", badgeEn: "New" },
            { id: "card", name: "Card & Border", icon: "fa-id-card" },
            { id: "badge", name: "Badge", icon: "fa-certificate", badge: "Mới", badgeEn: "New" },
            { id: "divider", name: "Divider", icon: "fa-minus" },
            { id: "adaptivepanel", name: "AdaptivePanel", icon: "fa-table-cells-large", badge: "Mới", badgeEn: "New" },
            { id: "concostwindow", name: "CONCOSTWindow", icon: "fa-window-restore", badge: "Mới", badgeEn: "New" }
        ]
    },
    {
        title: "Hiệu ứng & Trang trí", titleEn: "Effects & Decoration",
        items: [
            { id: "ripple", name: "Ripple", icon: "fa-water", badge: "Mới", badgeEn: "New" },
            { id: "snowcanvas", name: "SnowCanvas", icon: "fa-snowflake" },
            { id: "sprite", name: "Sprite", icon: "fa-circle-dot" },
            { id: "compareslider", name: "CompareSlider", icon: "fa-arrows-left-right" },
            { id: "magnifier", name: "Magnifier", icon: "fa-magnifying-glass-plus" },
            { id: "icons", name: "Icons Gallery", icon: "fa-icons" }
        ]
    },
    {
        title: "Controls cơ bản", titleEn: "Basic Controls",
        items: [
            { id: "button", name: "Button", icon: "fa-square-check" },
            { id: "togglebutton", name: "ToggleButton", icon: "fa-toggle-on" },
            { id: "checkbox", name: "CheckBox", icon: "fa-check-square", badge: "Mới", badgeEn: "New" },
            { id: "radiobutton", name: "RadioButton", icon: "fa-circle-dot", badge: "Mới", badgeEn: "New" },
            { id: "slider", name: "Slider", icon: "fa-arrows-up-down", badge: "Mới", badgeEn: "New" },
            { id: "progressbar", name: "ProgressBar", icon: "fa-bars-progress" },
            { id: "scrollviewer", name: "ScrollViewer", icon: "fa-arrow-down-up-across-line", badge: "Mới", badgeEn: "New" },
            { id: "listbox", name: "ListBox", icon: "fa-rectangle-list", badge: "Mới", badgeEn: "New" },
            { id: "listview", name: "ListView", icon: "fa-list", badge: "Mới", badgeEn: "New" },
            { id: "datagrid", name: "DataGrid", icon: "fa-table" },
            { id: "treeview", name: "TreeView", icon: "fa-folder-tree" },
            { id: "groupbox", name: "GroupBox", icon: "fa-square", badge: "Mới", badgeEn: "New" },
            { id: "menu", name: "Menu & ContextMenu", icon: "fa-bars-staggered", badge: "Mới", badgeEn: "New" },
            { id: "textblock", name: "Typography", icon: "fa-font", badge: "Mới", badgeEn: "New" }
        ]
    },
    {
        title: "Thời gian", titleEn: "Time",
        items: [
            { id: "clock", name: "Clock", icon: "fa-clock" },
            { id: "calendarwithclock", name: "CalendarWithClock", icon: "fa-calendar-days", badge: "Mới", badgeEn: "New" }
        ]
    }
];
