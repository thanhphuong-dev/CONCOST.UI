// English overlay — controls ported from modern web UI patterns.
Object.assign(DOCS_EN.pages, {

    spotlightcard: {
        title: "SpotlightCard — cursor-following glow card",
        desc: "A card surface with a <b>spotlight that follows the cursor</b> plus a gradient border that fades in on hover — the web “spotlight card” pattern, ported to WPF with a <code>RadialGradientBrush</code> and a <code>Canvas</code>, so it costs no extra GPU layer.",
        variants: [
            { desc: "Surface background, 1px border, 14px radius, 260px spotlight." },
            { desc: "Secondary surface, no border, 18px radius, stronger spotlight (0.28)." }
        ],
        sections: [
            {
                title: "A KPI card with a spotlight",
                desc: "Host any content like a normal <code>ContentControl</code>. The glow disappears when the pointer leaves; set <code>IsSpotlightEnabled=\"False\"</code> to disable it entirely.",
                csharp: `// Tune the effect per context
var card = new SpotlightCard
{
    SpotlightSize = 320,
    SpotlightOpacity = 0.24,
    CornerRadius = new CornerRadius(16),
    SpotlightBrush = (Brush)FindResource("CONCOST.Brush.Primary")
};

// Disable it on long lists to keep 60 FPS
foreach (var item in ItemsPanel.Children.OfType<SpotlightCard>())
    item.IsSpotlightEnabled = false;`
            },
            {
                title: "Gradient border on hover",
                desc: "<code>BorderGradient</code> accepts any <code>Brush</code>; use a <code>LinearGradientBrush</code> for a multi-colour edge. <code>ShowHoverBorder=\"False\"</code> keeps the spotlight but drops the border.",
                csharp: `// Recolour the spotlight by business state
card.SpotlightBrush = isValid
    ? (Brush)FindResource("CONCOST.Brush.Success")
    : (Brush)FindResource("CONCOST.Brush.Danger");

// Keep the glow, drop the gradient border
card.ShowHoverBorder = false;`
            }
        ]
    },

    odometernumber: {
        title: "OdometerNumber — animated number",
        desc: "Rolls from the previous value to the new one with easing — for KPI tiles, dashboards and summary rows. No <code>Storyboard</code> per update: just bind <code>Value</code>.",
        variants: [
            { desc: "28px bold — the default for metric tiles." },
            { desc: "42px extra-bold — the hero number of a dashboard." },
            { desc: "15px semibold — for tables and toolbars." }
        ],
        sections: [
            {
                title: "Bind the value and let it roll",
                desc: "<code>Format</code> is a .NET numeric format string (<code>N0</code>, <code>N2</code>, <code>0.###</code>). <code>Prefix</code>/<code>Suffix</code> add units. Set <code>Duration</code> to zero to disable the animation.",
                csharp: `// Update when data arrives — the control rolls from the old value
ConcreteM3 = summary.ConcreteM3;

// Show the value immediately, without the roll
SteelKgNumber.Duration = new Duration(TimeSpan.Zero);

// Skip the animation on the very first load
SteelKgNumber.AnimateOnLoad = false;`
            }
        ]
    },

    sparkline: {
        title: "Sparkline — inline mini chart",
        desc: "A tiny trend line for table cells, KPI cards and status strips: auto-scales to its box, optionally fills a gradient area and marks the latest point.",
        sections: [
            {
                title: "Trend line inside a quantity table",
                desc: "Just pass a sequence of numbers. <code>Smooth</code> draws Bezier curves, <code>ShowArea</code> fills underneath, <code>ShowLastDot</code> highlights the newest value.",
                csharp: `// Feed the series from monitoring data
var values = measurements.Select(m => m.SettlementMm).ToList();

var spark = new Sparkline
{
    Values = values,
    Smooth = true,
    ShowArea = true,
    StrokeThickness = 1.8,
    Height = 32,
    Width = 120
};

// Colour by direction
spark.Stroke = values.Last() >= values.First()
    ? (Brush)FindResource("CONCOST.Brush.Success")
    : (Brush)FindResource("CONCOST.Brush.Danger");`,
                callout: {
                    body: "On very long tables set <code>ShowArea=\"False\"</code> and <code>ShowLastDot=\"False\"</code> to cut the number of primitives drawn while scrolling."
                }
            }
        ]
    },

    avatarstack: {
        title: "AvatarStack — stacked avatars",
        desc: "Shows the people responsible as overlapping avatars with a <b>+N</b> overflow chip — the pattern from task boards. Accepts strings (initials are derived) or <code>ImageSource</code>.",
        sections: [
            {
                title: "Reviewers of a submittal",
                desc: "Pass <code>Items</code> as names or images. <code>MaxVisible</code> caps how many avatars render; the rest collapse into the <code>+N</code> chip (<code>OverflowTextFormat</code> changes the label).",
                csharp: `// Strings → initials are derived ("Nguyen Van A" → "NA")
Reviewers = new[] { "Nguyen Van A", "Tran Thi B", "Le C", "Pham D", "Hoang E" };

// Images work too
stack.Items = new object[]
{
    new BitmapImage(new Uri("pack://application:,,,/Avatars/lead.png")),
    "Tran Thi B"
};

// Tune the layout
stack.Size = 40;
stack.Overlap = 14;
stack.OverflowTextFormat = "{0} more";`
            }
        ]
    },

    heatmapcalendar: {
        title: "HeatmapCalendar — daily intensity grid",
        desc: "GitHub-style contribution grid: one square per day, shaded by intensity. Use it for concrete pour logs, inspection counts or monitoring density — gaps and hot spots become obvious at a glance.",
        sections: [
            {
                title: "One element per day",
                desc: "<code>Values</code> is a sequence of integers; the first element maps to <code>StartDate</code>. Colours use four levels relative to the maximum, while zero days use <code>BaseBrush</code>. Columns are weeks, rows are weekdays (Monday first).",
                csharp: `// Group the pour log by day
var counts = pours
    .GroupBy(p => p.Date.Date)
    .OrderBy(g => g.Key)
    .Select(g => g.Count())
    .ToList();

var heatmap = new HeatmapCalendar
{
    Values = counts,
    StartDate = DateTime.Today.AddDays(-counts.Count + 1),
    CellSize = 13,
    CellGap = 3
};

// Recolour the scale to the brand palette
heatmap.Level1Brush = new SolidColorBrush(Color.FromArgb(90, 0xFF, 0x79, 0x00));
heatmap.Level4Brush = new SolidColorBrush(Color.FromRgb(0xFF, 0x79, 0x00));`
            }
        ]
    },

    marqueeticker: {
        title: "MarqueeTicker — scrolling notice strip",
        desc: "A continuously scrolling text strip for status bars, site notice boards and tickers. It renders directly and is driven by <code>CompositionTarget.Rendering</code>, so it stays smooth without a Storyboard per item.",
        sections: [
            {
                title: "Status bar ticker",
                desc: "<code>Speed</code> is in pixels per second. <code>Separator</code> is inserted between repetitions. Set <code>IsRunning=&quot;False&quot;</code> to pause without losing the position.",
                csharp: `var ticker = new MarqueeTicker
{
    Text = "Site A — pouring slab level 12 · 3 crews active",
    Speed = 55,        // pixels per second
    Separator = "  •  ",
    FontSize = 13
};

// Pause / resume
ticker.IsRunning = false;`,
                callout: {
                    body: "If the ticker lives inside a long scrolling list, set <code>IsRunning=&quot;False&quot;</code> while the item is off-screen to avoid painting every frame."
                }
            }
        ]
    }
});
