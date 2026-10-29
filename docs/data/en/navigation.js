// English overlay — navigation pages.
Object.assign(DOCS_EN.pages, {

    navigationview: {
        title: "NavigationView — list-style navigation",
        desc: "Compact navigation list with icons, per-item status badges and a two-way <code>SelectedItem</code> — lighter than SideMenu for narrow panels.",
        sections: [
            { title: "Vertical tab navigation", desc: "<code>NavigationItem</code> is a <code>HeaderedContentControl</code>: <code>Header</code> is the label, <code>Icon</code> the glyph and <code>TagName</code> the routing key." }
        ]
    },

    sidemenu: {
        title: "SideMenu — vertical application menu",
        desc: "Vertical navigation that collapses to icons, supports grouped items and notification badges.",
        sections: [
            { title: "XAML example", desc: "Group modules and show a badge on the entries that need attention." }
        ]
    },

    drawermenu: {
        title: "DrawerMenu — animated side menu",
        desc: "Drawer navigation with an animated expand/collapse, a header with logo and toggle button, vector icons per item and a pinned footer.",
        sections: [
            { title: "1. Declare it in XAML", desc: "Set <code>CompactWidth</code> and <code>ExpandedWidth</code> to match your shell layout." }
        ]
    },

    breadcrumb: {
        title: "Breadcrumb — hierarchy path",
        desc: "Shows the position inside a model hierarchy (project → level → member), with per-item icons and clickable commands.",
        sections: [
            { title: "XAML example", desc: "Add <code>BreadcrumbItem</code> entries; the separator icon is configurable." }
        ]
    },

    pagination: {
        title: "Pagination — data paging control",
        desc: "Page navigation for large result sets with quick page jump and configurable page size.",
        sections: [
            { title: "XAML example", desc: "Set <code>TotalItems</code> and <code>PageSize</code>; the control derives the page count." }
        ]
    },

    stepbar: {
        title: "StepBar — workflow steps",
        desc: "Shows the position in a multi-step workflow with waiting / in-progress / complete / error states.",
        sections: [
            { title: "XAML syntax", desc: "Each <code>StepBarItem</code> carries a <code>Status</code>; the bar docks horizontally or vertically." }
        ]
    },

    transfer: {
        title: "Transfer — two-list shuttle",
        desc: "Moves items between two symmetric lists, with instant filtering, select-all and bulk transfer.",
        sections: [
            { title: "XAML example", desc: "Assign elements to a crew: bind <code>ItemsSource</code> for the source list and <code>TransferredItems</code> for the destination." }
        ]
    },

    tabcontrol: {
        title: "TabControl — module tabs",
        desc: "Tab strip styled for the design system, with closable tabs, extra header content and eleven layout variants.",
        variants: [
            { desc: "Classic tabs, card tabs, or browser-style chrome." },
            { desc: "Pill, segmented or capsule tabs." },
            { desc: "Flat tabs with a sliding indicator." },
            { desc: "Tabs at the bottom, vertically left or vertically right." },
            { desc: "Per-item style inherited by every variant." }
        ],
        sections: [
            { title: "XAML example", desc: "Enable closable tabs and add a trailing “+” button through the attached helper." }
        ]
    }
});
