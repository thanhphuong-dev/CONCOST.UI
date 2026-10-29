// English overlay — data & dashboard pages.
Object.assign(DOCS_EN.pages, {

    datapager: {
        title: "DataPager — data paging",
        desc: "Paging for DataGrid/ListView: <code>PageIndex</code>, <code>PageSize</code> and <code>TotalItems</code> are two-way bound and coerced to valid ranges automatically.",
        sections: [
            { title: "Paging a quantity take-off grid", desc: "Set <code>TotalItems</code> to the record count; the pager derives the page count and clamps <code>PageIndex</code> for you." }
        ]
    },

    statisticcard: {
        title: "StatisticCard — KPI cards",
        desc: "At-a-glance metric card with title, value, unit, trend indicator, icon and progress bar — the building block of engineering dashboards.",
        sections: [
            { title: "XAML example", desc: "Bind <code>Value</code>, <code>Trend</code> and <code>Progress</code> to the metric you want to surface." }
        ]
    },

    timeline: {
        title: "Timeline — construction progress",
        desc: "Renders a chronological series of milestones with semantic colours, vector icons and a connector line that stops at the last entry.",
        sections: [
            { title: "Standard XAML syntax", desc: "Each <code>TimelineItem</code> carries <code>Time</code>, <code>Description</code>, <code>Type</code> and <code>Icon</code>." }
        ]
    },

    cardcarousel: {
        title: "CardCarousel — 3D card carousel",
        desc: "Depth-staged card carousel for project thumbnails, model previews or template galleries, with automatic advance and scale/offset per card.",
        sections: [
            { title: "XAML usage", desc: "Bind the item collection; <code>AutoRun</code> and <code>Interval</code> control automatic cycling." }
        ]
    },

    propertygrid: {
        title: "PropertyGrid — object inspector",
        desc: "Editable property inspector for CAD and BIM objects: assigns a plain CLR object and renders the editors (text, number, boolean, enum, alignment, image) automatically.",
        sections: [
            { title: "Assigning an object to the PropertyGrid", desc: "Set <code>SelectedObject</code>; categories are derived from the object's attributes." }
        ]
    }
});
