// English overlay — feedback & status pages.
Object.assign(DOCS_EN.pages, {

    dialog: {
        title: "Dialog — modal popup",
        desc: "Modal dialog with a dimmed mask, close-by-token and typed content (information, success, warning, error, question).",
        sections: [
            { title: "Showing a dialog from XAML", desc: "Declare the dialog once, then open it with <code>IsOpen</code> and read the result by token." }
        ]
    },

    drawer: {
        title: "Drawer — sliding side panel",
        desc: "Panel that slides in from any of the four edges with a backdrop mask, used for BIM object details and configuration forms.",
        sections: [
            { title: "XAML syntax", desc: "Set <code>Placement</code> and <code>DrawerWidth</code>; <code>IsOpen</code> drives the animation." }
        ]
    },

    growl: {
        title: "Growl — toast notifications",
        desc: "Non-blocking toast notifications stacked in a screen corner, with typed variants and an optional close button.",
        sections: [
            { title: "Raising a notification from C#", desc: "Call <code>Growl.Toast</code> with the message type, or add a <code>Growl</code> instance to a host panel for custom placement." }
        ]
    },

    loadings: {
        title: "Loading indicators — spinners &amp; progress",
        desc: "Four <code>LoadingIndicator</code> styles plus five standalone animation controls (<code>CycleLoading</code>, <code>RingLoading</code>, <code>RollLoading</code>, <code>WaitLoading</code>, <code>LoadingCircle</code>, <code>LoadingLine</code>).",
        sections: [
            { title: "The four LoadingIndicator styles", desc: "<code>IndicatorType</code> accepts <code>ThreeDotsWave</code>, <code>CircularDotsRing</code>, <code>LinePulse</code> and <code>PulseCircle</code>; <code>DotSize</code> scales the dots." },
            { title: "Determinate progress", desc: "<code>CycleLoading</code> renders a percentage ring with <code>LoadTitle</code>; <code>Value</code> runs from 0 to 100." }
        ]
    },

    loadingoverlay: {
        title: "LoadingOverlay — blocking overlay",
        desc: "Overlay that blocks interaction while a long task runs (solve, IFC import, cloud sync), with a title, sub-message and progress bar.",
        sections: [
            { title: "Overlay for a solver run", desc: "Use <code>IsIndeterminate=\"True\"</code> when the duration is unknown, or bind <code>Progress</code> for determinate work." }
        ]
    },

    skeleton: {
        title: "Skeleton — shimmering placeholders",
        desc: "Shimmering placeholders that hold the layout while data loads, avoiding the layout shift of a centred spinner.",
        variants: [
            { desc: "Rectangular block with the configured corner radius." },
            { desc: "Thin text line with rounded ends." },
            { desc: "Circle for avatars." },
            { desc: "Large block mimicking a KPI card." }
        ],
        sections: [
            { title: "Skeleton for a beam list", desc: "Stack the real content and the skeleton in the same Grid and toggle <code>IsLoading</code>; <code>IsWaveEnabled</code> controls the shimmer." }
        ]
    },

    emptystate: {
        title: "EmptyState — empty results",
        desc: "Illustrated empty state with title, description and primary/secondary actions — used when a filter returns nothing.",
        sections: [
            { title: "XAML example", desc: "Bind <code>ActionCommand</code> to the recovery action (clear filters, create the first item)." }
        ]
    },

    watermark: {
        title: "Watermark — copyright overlay",
        desc: "Tiled or single centred watermark for drawings and print previews, with configurable angle, opacity and brush.",
        sections: [
            { title: "XAML example", desc: "Set <code>Mode</code> to <code>RepeatedTiled</code> for drawings, or <code>SingleCenter</code> for previews." }
        ]
    },

    ripple: {
        title: "Ripple — Material click effect",
        desc: "Material-style ripple on click, usable standalone or attached to any element through <code>RippleElement</code>.",
        sections: [
            { title: "Ripple as an attached effect", desc: "<code>RippleElement.IsEnabled</code> turns any element into a ripple surface. <code>Mode=\"Target\"</code> starts the wave at the click point, <code>Center</code> starts it at the centre." }
        ]
    }
});
