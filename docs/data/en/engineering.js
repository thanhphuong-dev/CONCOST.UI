// English overlay — engineering & CAD pages.
Object.assign(DOCS_EN.pages, {

    spreadsheet: {
        title: "Spreadsheet — engineering worksheets",
        desc: "Excel-like grid with a formula engine, multiple sheets, cell formatting, CSV import/export and a fixed toolbar, formula bar and status bar.",
        sections: [
            { title: "1. Declare it in XAML", desc: "Bind <code>Sheets</code> from the view model and switch the chrome pieces on or off as needed." },
            { title: "2. CSV import and export", desc: "Round-trip quantity take-offs and schedules with any spreadsheet tool the project already uses." },
            { title: "3. Excel-compatible experience and built-in functions", desc: "Cell addressing, range selection, SUM/AVERAGE/COUNT style formulas and number formats behave the way users expect from Excel." }
        ]
    },

    stackedheaderdatagrid: {
        title: "StackedHeaderDataGrid — multi-level headers",
        desc: "A DataGrid built for construction schedules and quantity take-offs: grouped header trees, a drag-and-drop column grouping panel and automatic footer aggregates (Sum, Average, Count, Min, Max) that stay pinned while the body scrolls.",
        sections: [
            { title: "1. Declare the header tree in XAML", desc: "Define grouped columns such as “Section dimensions (b × h)” and “Material quantities”, with an aggregate per leaf column." }
        ]
    },

    cadcanvas: {
        title: "ZoomableCanvas &amp; RulerControl",
        desc: "A 2D CAD viewport with infinite zoom around the cursor, middle/right-button panning, a grid that scales with the zoom level and rulers that stay in sync with the canvas in millimetres.",
        sections: [
            { title: "1. A complete CAD viewport", desc: "Combine a horizontal and a vertical ruler with the zoomable canvas:" }
        ]
    },

    diagramchart: {
        title: "ChartLine — internal force diagrams",
        desc: "Plot bending moment, shear, axial force or deflection from a series of values. Every chart derives from <code>ChartBase</code>, so <code>Datas</code>, <code>Title</code>, <code>Unit</code> and <code>ShowLegend</code> are shared.",
        sections: [
            {
                title: "Moment diagram for a simply supported beam",
                desc: "<code>Datas</code> is an <code>IEnumerable&lt;KeyValuePair&lt;string, double&gt;&gt;</code>: the key is the X axis label (usually the station along the member), the value is the internal force. <code>ShowArea</code> fills the area under the line, <code>Smooth</code> draws a spline instead of straight segments.",
                callout: {
                    title: "There is no InvertAxis property",
                    body: "The control does not flip axes. To follow the TCVN convention (sagging moment drawn downwards) <b>negate the values</b> before binding them to <code>Datas</code> — see the snippet."
                }
            },
            { title: "Comparing load combinations", desc: "Stack several <code>ChartLine</code> controls in one <code>Grid</code> with a shared scale, or use <code>ChartBar</code> for discrete per-combination values." }
        ]
    },

    charts: {
        title: "Charts Suite — vector engineering charts",
        desc: "Bar, line, radar and pie charts rendered as vectors at 60 FPS, sharing one data contract and one theme-aware palette.",
        sections: [
            { title: "ChartBar in XAML", desc: "Bind <code>Datas</code> and let <code>MultiColor</code> colour each bar individually." }
        ]
    },

    rebarinput: {
        title: "RebarInput — rebar notation input",
        desc: "Accepts standard rebar notation (for example <code>4D20</code>) and computes the steel area As in mm² as you type, flagging invalid notation.",
        sections: [
            { title: "1. XAML usage", desc: "Bind <code>Text</code> two-way and read <code>AreaMm2</code> / <code>IsValid</code> to drive validation." }
        ]
    },

    engineeringboxes: {
        title: "Engineering Boxes — six quantity inputs",
        desc: "Six engineering quantity inputs derived from <code>UnitBox</code>: LengthBox (mm, m), AreaBox (m²), VolumeBox (m³), ForceBox (kN), MomentBox (kN.m) and PressureBox (MPa).",
        sections: [
            { title: "1. Using all six inputs", desc: "A structural beam parameter form with steppers, decimals and a fixed unit badge on every field." }
        ]
    },

    compassindicator: {
        title: "CompassIndicator — site orientation",
        desc: "North arrow for site plans and drawings, with the rotation angle bound to the building orientation.",
        sections: [
            { title: "1. XAML example", desc: "Set <code>Angle</code> to the rotation of the structure relative to true north." }
        ]
    }
});
