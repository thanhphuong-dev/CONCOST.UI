// English overlay — layout & shell, effects, basic controls and time controls.
Object.assign(DOCS_EN.pages, {

    /* ---------------- Layout & shell ---------------- */
    splitbutton: {
        title: "SplitButton — action button with menu",
        desc: "Primary action button with a dropdown menu: 19 properties and 13 style variants, replacing hand-built Button + ContextMenu combos.",
        variants: [
            { desc: "Primary action of the screen." },
            { desc: "Decreasing priority levels." },
            { desc: "Business semantics." },
            { desc: "Fully rounded or square corners." },
            { desc: "Height 44px / 28px." }
        ],
        sections: [
            { title: "Split mode: action button plus its own menu", desc: "<code>HitMode=\"Split\"</code> (default) separates the click zones: the body runs the action, the chevron opens the menu. <code>HitMode=\"DropDown\"</code> turns the whole button into a menu opener." },
            {
                title: "Open the menu on hover",
                desc: "Useful in CAD toolbars: a 200 ms hover opens the menu without a click.",
                callout: { body: "Set <code>StaysOpen=\"True\"</code> for multi-level menus that need consecutive clicks (for example selecting several layers) — the menu will not close on inner clicks." }
            }
        ]
    },

    expander: {
        title: "Expander — collapsible section",
        desc: "Collapsible content block with 18 style variants (Clean, Card, Filled, Round, Small, semantic colours) and support for all four expand directions.",
        variants: [
            { desc: "Default: hairline border, chevron in the header." },
            { desc: "Surface background with a soft shadow — dashboard friendly." },
            { desc: "Solid accent header." },
            { desc: "No border, only a thin separator." },
            { desc: "Large 14px corner radius for settings blocks." },
            { desc: "32px header for dense panels." },
            { desc: "Header coloured by semantic meaning." },
            { desc: "Templates for expanding right / down / up." }
        ],
        sections: [
            { title: "Expander for a group of section properties", desc: "<code>ExpandDirection</code> accepts <code>Down</code>, <code>Up</code>, <code>Left</code> and <code>Right</code>; horizontal expansion switches the header to a vertical layout automatically." }
        ]
    },

    card: {
        title: "Card &amp; Border &amp; Badge",
        desc: "Standard container with header icon, footer slot, hover states and elevation — the base surface for panels, tiles and metric blocks.",
        variants: [
            { desc: "Hairline border on the surface brush." },
            { desc: "Shadow resolved from the elevation tokens." },
            { desc: "3px accent bar on the left edge." },
            { desc: "Fully rounded corners." }
        ],
        sections: [
            { title: "XAML example", desc: "Compose a panel with an icon header and a footer action." }
        ]
    },

    badge: {
        title: "Badge — count &amp; status badge",
        desc: "Count/status badge, usable standalone or attached to any <code>ButtonBase</code> through <code>attached:ButtonElement.Badge</code>.",
        variants: [
            { desc: "Solid background by Appearance, white text." },
            { desc: "Fully rounded for multi-digit counts." },
            { desc: "Small dot with no content (IsDot=True)." },
            { desc: "Outline only — suits light toolbars." },
            { desc: "12% tint background with strong text — softer than solid." }
        ],
        sections: [
            { title: "Standalone badge with six appearances", desc: "<code>Appearance</code> accepts <code>Primary</code>, <code>Success</code>, <code>Warning</code>, <code>Danger</code>, <code>Info</code> and <code>Neutral</code>. Set <code>IsDot=\"True\"</code> to show a dot instead of a number." },
            { title: "Attach a badge to a Button / ToggleButton", desc: "The <code>ButtonElement.Badge</code> attached property draws the badge in the top-right corner and hides it when the value is <code>null</code>." }
        ]
    },

    divider: {
        title: "Divider — separator",
        desc: "Horizontal or vertical separator with optional label and icon, in solid, dashed or dotted styles.",
        sections: [
            { title: "XAML example", desc: "Use a labelled divider to split form sections without adding another card." }
        ]
    },

    adaptivepanel: {
        title: "AdaptivePanel — breakpoint-driven layout",
        desc: "ContentControl that picks a layout from the available width through a <code>Breakpoint</code> list — three tiers (Compact / Normal / Wide) without code-behind.",
        sections: [
            { title: "Default breakpoints", desc: "Defaults: <code>&lt; 900</code> → Compact, <code>900–1400</code> → Normal, <code>≥ 1400</code> → Wide. <code>CurrentLayout</code> is two-way bound so you can swap templates from it." }
        ]
    },

    concostwindow: {
        title: "CONCOSTWindow — custom window chrome",
        desc: "WPF window with a fully custom chrome (title bar, min/max/close buttons in the template) — dragging and double-click-to-maximise are handled for you.",
        sections: [
            {
                title: "Declaring the window",
                desc: "Derives from <code>Window</code> and sets <code>WindowStyle=\"None\"</code> in the constructor. The template needs four parts: <code>PART_TitleBar</code>, <code>PART_MinimizeButton</code>, <code>PART_MaximizeButton</code> and <code>PART_CloseButton</code>.",
                callout: { title: "AllowsTransparency", body: "The constructor sets <code>AllowsTransparency = false</code> to keep performance and DWM shadows. If you need a fully transparent window, reset it after <code>InitializeComponent()</code> and accept losing hardware acceleration." }
            }
        ]
    },

    /* ---------------- Effects ---------------- */
    snowcanvas: {
        title: "SnowCanvas — snow effect",
        desc: "GPU-friendly falling snow overlay for seasonal themes and splash screens, with density, size, speed and wind controls.",
        sections: [
            { title: "XAML example", desc: "Place it on top of a grid and set <code>IsActive=\"False\"</code> outside the festive period." }
        ]
    },

    sprite: {
        title: "Sprite — floating assistant",
        desc: "Draggable floating helper that snaps to the screen edge, with a context menu — the HandyControl-style assistant pattern.",
        sections: [
            { title: "XAML example", desc: "Provide <code>MenuContent</code> for the quick actions and enable <code>AutoSnap</code>." }
        ]
    },

    compareslider: {
        title: "CompareSlider — before/after comparison",
        desc: "Draggable divider that reveals two layered visuals, for comparing design revisions or as-built versus design.",
        sections: [
            { title: "XAML example", desc: "Put the original in <code>SourceContent</code> and the revision in <code>TargetContent</code>." }
        ]
    },

    magnifier: {
        title: "Magnifier — drawing loupe",
        desc: "Loupe that magnifies a region of the drawing under the cursor, with optional crosshair and adjustable radius.",
        sections: [
            { title: "XAML example", desc: "Point <code>Target</code> at the viewport element and set the <code>Scale</code> and <code>Radius</code>." }
        ]
    },

    icons: {
        title: "Icons Gallery",
        desc: "More than 120 engineering vector icons (sections, rebar, loads, supports, BIM, CAD tools) exposed through the <code>IconKind</code> enum.",
        sections: [
            { title: "Using an icon", desc: "Set <code>IconElement.Kind</code> on any control, or use the <code>Icon</code> control directly." }
        ]
    },

    /* ---------------- Basic controls ---------------- */
    button: {
        title: "Button",
        desc: "Button with vector icons, five appearance families, loading state and notification badge — 23 style variants in total.",
        variants: [
            { desc: "Primary and secondary actions." },
            { desc: "Solid semantic backgrounds." },
            { desc: "12% tint background with strong text — for light toolbars." },
            { desc: "Outline only." },
            { desc: "Icon-only buttons at different background levels." },
            { desc: "Base style to inherit when building your own variant." }
        ],
        sections: [
            { title: "XAML example", desc: "Compose icon, loading state and badge on a single button." }
        ]
    },

    togglebutton: {
        title: "ToggleButton",
        desc: "Two-state button with icon-only variants, used for viewport toggles such as grid, snap and section display.",
        variants: [
            { desc: "Solid, semantic or outline." },
            { desc: "Icon only, square or circular." },
            { desc: "Icon with no background, colour changes when checked." },
            { desc: "Icon with a border." },
            { desc: "Icon that turns success-coloured when checked." }
        ],
        sections: [
            { title: "XAML example", desc: "Toggle the viewport grid and keep the button in sync with the model." }
        ]
    },

    checkbox: {
        title: "CheckBox",
        desc: "CheckBox with three variants (default, button, outlined button), tri-state support and label metadata through attached properties.",
        variants: [
            { desc: "Square box plus label, accent tick." },
            { desc: "Renders as a toggle button with a solid background when checked." },
            { desc: "As above, outline only." }
        ],
        sections: [
            { title: "CheckBox with a title and required marker", desc: "<code>IsThreeState=\"True\"</code> allows the null state (used for “select all”). The <code>InfoElement.Title</code> attached property puts a label above." }
        ]
    },

    radiobutton: {
        title: "RadioButton",
        desc: "RadioButton with four variants, including <code>Segment</code> for segmented controls — a natural fit for view-mode filters.",
        variants: [
            { desc: "Round button plus label." },
            { desc: "Button style with a solid background when selected." },
            { desc: "Button style with a border." },
            { desc: "Segmented control — use it to switch view modes." }
        ],
        sections: [
            { title: "Segmented control for view modes", desc: "Buttons sharing a <code>GroupName</code> are mutually exclusive. Wrap them in a rounded <code>Border</code> for the full segmented look." }
        ]
    },

    slider: {
        title: "Slider",
        desc: "Slider restyled with its own thumb and repeat buttons, keeping the complete WPF <code>Slider</code> API.",
        variants: [
            { desc: "Thin track with a round accent thumb." },
            { desc: "Thumb style for a larger grab handle." },
            { desc: "Click zones on either side of a horizontal track." },
            { desc: "Click zones on either side of a vertical track." }
        ],
        sections: [
            { title: "Discrete slider with ticks", desc: "<code>IsSnapToTickEnabled</code> plus <code>TickFrequency</code> snaps values to fixed steps — handy for concrete grades and bar diameters." }
        ]
    },

    progressbar: {
        title: "ProgressBar",
        desc: "Progress bar with optional percentage text, five thickness/colour variants and theme-aware track colours.",
        variants: [
            { desc: "Thin 6px accent bar." },
            { desc: "12px / 3px." },
            { desc: "Semantic colours." }
        ],
        sections: [
            { title: "XAML example", desc: "Show the percentage through <code>ProgressBarElement.ShowText</code> and switch colour near completion." }
        ]
    },

    scrollviewer: {
        title: "ScrollViewer",
        desc: "ScrollViewer with auto-hiding scrollbars and the <code>ScrollViewerHelper</code> attached set (inertia, scroll speed, back-to-top).",
        variants: [
            { desc: "Thin scrollbar that hides when idle." },
            { desc: "Only a thin line, no track." },
            { desc: "Scrollbar always visible." }
        ],
        sections: [
            { title: "Scrolling panel with inertia", desc: "<code>IsInertiaEnabled</code> gives a touch-like momentum; <code>ShowBackToTop</code> reveals a jump-to-top button past a threshold." }
        ]
    },

    listbox: {
        title: "ListBox",
        desc: "ListBox restyled with theme-aware hover and selection, plus a card variant for block-style lists.",
        variants: [
            { desc: "Flat items with an accent highlight when selected." },
            { desc: "Each item is a bordered card with spacing." }
        ],
        sections: [
            { title: "Element list", desc: "Keep <code>VirtualizingStackPanel</code> for long lists; only change <code>ItemsPanel</code> when you need a special layout." }
        ]
    },

    listview: {
        title: "ListView",
        desc: "ListView for lightweight multi-column lists that do not need DataGrid sorting and filtering.",
        variants: [
            { desc: "Themed column headers and rows, no grid lines." }
        ],
        sections: [
            { title: "Three-column list", desc: "Use a <code>GridView</code> with <code>DisplayMemberBinding</code>. For sorting, filtering or editing, switch to <code>DataGrid</code>." }
        ]
    },

    datagrid: {
        title: "DataGrid",
        desc: "DataGrid restyled for dense engineering tables, with row spacing, dividers, alignment control and conditional formatting rules.",
        variants: [
            { desc: "Standard: bold header, zebra rows, hairline grid." },
            { desc: "26px rows for very dense tables." },
            { desc: "No grid lines, no outer border." },
            { desc: "Smaller font and padding." },
            { desc: "Styles for checkbox cells, the editing text box and the column gripper." }
        ],
        sections: [
            { title: "XAML example", desc: "Disable auto-generated columns and bind the quantity take-off rows." }
        ]
    },

    treeview: {
        title: "TreeView",
        desc: "TreeView with hierarchy connectors, directory-style icons and a modern pill selection variant.",
        variants: [
            { desc: "Connector lines and an expander chevron." },
            { desc: "Card-style items." },
            { desc: "Folder/file icons for directory trees." },
            { desc: "Vertical connector lines only, no background." },
            { desc: "Pill items with a soft accent hover." }
        ],
        sections: [
            { title: "XAML example", desc: "Bind a hierarchical template for the model tree and expand all nodes when the model loads." }
        ]
    },

    groupbox: {
        title: "GroupBox",
        desc: "GroupBox with seven variants for grouping inputs by engineering topic (geometry, material, loads).",
        variants: [
            { desc: "Hairline border with the header on the border." },
            { desc: "Surface background, rounded, header inside." },
            { desc: "Border and header coloured by meaning." },
            { desc: "Header with a solid background." },
            { desc: "Only a rule under the header." }
        ],
        sections: [
            { title: "Material property group", desc: "Set <code>Padding</code> for the content; the header accepts any <code>object</code>, so you can embed an icon." }
        ]
    },

    menu: {
        title: "Menu &amp; ContextMenu",
        desc: "Horizontal menus and context menus restyled with four variants, supporting nested submenus and icons.",
        variants: [
            { desc: "Flat menu with a soft accent hover." },
            { desc: "No border — fits toolbars." },
            { desc: "Floating menu with shadow and rounded corners." },
            { desc: "Pill-shaped items for a modern app." }
        ],
        sections: [
            { title: "Horizontal menu with submenus", desc: "Nested <code>MenuItem</code> elements create submenus; <code>InputGestureText</code> renders the shortcut on the right." }
        ]
    },

    textblock: {
        title: "Typography",
        desc: "The design system type scale: H1–H4, Body, Caption, Code and semantic colours — use these instead of setting FontSize by hand.",
        variants: [
            { desc: "Heading scale at 28 / 22 / 18 / 16px, decreasing weight." },
            { desc: "Body copy at 13.5px with 1.6 line height." },
            { desc: "Secondary label at 11.5px in the muted colour." },
            { desc: "Monospace on a sunken background — for notation and member IDs." },
            { desc: "Semantic status colours." }
        ],
        sections: [
            { title: "Content hierarchy inside a panel", desc: "Use the styles instead of <code>FontSize</code>/<code>FontWeight</code> so the whole application follows theme and scale changes." }
        ]
    },

    /* ---------------- Time ---------------- */
    clock: {
        title: "Clock — time picker",
        desc: "Material-style clock face for picking hour, minute and second. Use <code>CalendarWithClock</code> when you also need a date.",
        sections: [
            { title: "XAML syntax", desc: "Bind <code>SelectedTime</code> and handle <code>SelectedTimeChanged</code>." }
        ]
    },

    calendarwithclock: {
        title: "CalendarWithClock — date &amp; time",
        desc: "Date picker combined with a time picker — for inspection milestones, monitoring timestamps and construction schedules.",
        sections: [
            { title: "Picking an inspection timestamp", desc: "<code>SelectedDateTime</code> is two-way bound; the <code>Confirmed</code> event fires when the user accepts the value." }
        ]
    }
});
