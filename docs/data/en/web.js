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
    },

    aurorabackground: {
        title: "AuroraBackground — animated gradient backdrop",
        desc: "An aurora backdrop made of a few large, slowly drifting glows, drawn directly with <code>RadialGradientBrush</code> — <b>no BlurEffect</b>, so it stays smooth on weak machines. Good for splash screens, hero panels and login windows.",
        sections: [
            {
                title: "Layer it behind content in a Grid",
                desc: "Put <code>AuroraBackground</code> as the bottom layer and the content on top. The control clips itself, so it never bleeds outside its box.",
                csharp: `var aurora = new AuroraBackground
{
    BlobCount = 5,
    BlobOpacity = 0.45,
    Speed = 1.0,               // drift speed multiplier
    AccentBrush = Brushes.OrangeRed
};

// Pause while the window is hidden to avoid needless painting
aurora.IsAnimated = false;`,
                callout: {
                    body: "Aurora works best on dark surfaces. On light backgrounds drop <code>BlobOpacity</code> to 0.25–0.3 so text keeps AA contrast."
                }
            }
        ]
    },

    borderbeam: {
        title: "BorderBeam — light running around the edge",
        desc: "A comet of light travelling along the rounded-rect perimeter — the “border beam” pattern from modern landing pages. Wrap any content; use it for new items, in-progress states or cards that need attention.",
        sections: [
            {
                title: "Wrap a card",
                desc: "<code>BorderBeam</code> is a <code>Decorator</code>: put the content in <code>Child</code>. <code>BeamLength</code> is the lit fraction of the perimeter (0–1) and <code>RevolutionSeconds</code> is the time per lap.",
                csharp: `var beam = new BorderBeam
{
    CornerRadius = new CornerRadius(12),
    BeamThickness = 1.8,
    BeamLength = 0.30,          // 30% of the perimeter
    RevolutionSeconds = 3.5,    // one lap every 3.5 s
    BeamBrush = Brushes.Orange
};

beam.Child = new TextBlock { Text = "Processing..." };`,
                callout: {
                    type: "warn",
                    body: "The beam draws three passes (glow + mid + core) per frame. On long lists enable it only for the selected item, not for every row."
                }
            }
        ]
    },

    confettiburst: {
        title: "ConfettiBurst — celebration effect",
        desc: "Fires a short confetti burst on success (model saved, submittal approved). It simulates the particles itself and <b>resets itself</b> when they fade — no timer code required.",
        sections: [
            {
                title: "Drop it into a Grid above the content",
                desc: "Place <code>ConfettiBurst</code> in the same <code>Grid</code> as the content and set <code>IsBursting = true</code> to fire. <code>Origin</code> defaults to the centre-bottom; leave it empty to burst from the centre.",
                csharp: `private void OnApproveClick(object sender, RoutedEventArgs e)
{
    Approve();
    Celebration.IsBursting = true;   // clears itself when the particles fade
}

// Tuning
Celebration.Gravity = 1100;                                   // px/s²
Celebration.Duration = new Duration(TimeSpan.FromSeconds(2));
Celebration.Palette = new[] { Colors.Orange, Colors.SeaGreen };`,
                callout: {
                    body: "Keep <code>ParticleCount</code> ≤ 150 and set <code>ClipToBounds=&quot;True&quot;</code> on the hosting Grid — particles are plain rectangles, but bound them anyway."
                }
            }
        ]
    },

    sheensweep: {
        title: "SheenSweep — light sweep across a surface",
        desc: "A diagonal band of light that runs across a surface on a loop, with an idle gap between sweeps — the “sheen” used on CTAs and product cards. Place it in the same <code>Grid</code> as the content, on top.",
        sections: [
            {
                title: "Sweeping a button",
                desc: "<code>Speed</code> is sweeps per second and <code>PauseSeconds</code> the idle gap between them. <code>BandWidth</code> is a fraction of the width and <code>Angle</code> tilts the band.",
                csharp: `var sheen = new SheenSweep
{
    Speed = 0.35,          // sweeps per second
    PauseSeconds = 0.8,    // idle gap between sweeps
    BandWidth = 0.22,      // band width as a share of the control width
    Angle = 24,
    PeakOpacity = 0.45
};

// Pause when needed
sheen.IsActive = false;`,
                callout: {
                    type: "warn",
                    body: "Use it on one or two primary CTAs only. On every row of a list it becomes visual noise and burns frames for nothing."
                }
            }
        ]
    },

    pulsering: {
        title: "PulseRing — live-state rings",
        desc: "Rings expand and fade around a point — for running/syncing states or marking a location on a drawing.",
        sections: [
            {
                title: "Live dot next to a status label",
                desc: "<code>RingCount</code> is how many rings are alive at once, <code>Duration</code> is the time one ring takes to reach the edge and <code>MaxRadius</code> caps it. Set <code>CenterDot=&quot;False&quot;</code> for rings only.",
                csharp: `var pulse = new PulseRing
{
    MaxRadius = 46,
    RingCount = 3,
    Duration = 2.2,        // seconds per ring
    RingThickness = 2,
    RingBrush = Brushes.SeaGreen
};

// Stop when the connection drops
pulse.IsActive = false;`,
                callout: {
                    body: "Each ring is a single <code>DrawEllipse</code>, so it is cheap — but on a dense grid (hundreds of points) drop <code>RingCount</code> to 1–2."
                }
            }
        ]
    },

    wavedivider: {
        title: "WaveDivider — animated wave divider",
        desc: "Two sine layers drifting at different speeds, used as a section divider or as the base of a header block. Built from a <code>StreamGeometry</code>, so no images are needed.",
        sections: [
            {
                title: "Wave at the base of a header",
                desc: "<code>Amplitude</code> is in pixels, <code>Wavelength</code> controls the period and <code>Speed</code> is the drift in px/s. Set <code>ShowBackWave=&quot;False&quot;</code> for a single layer.",
                csharp: `var wave = new WaveDivider
{
    Height = 70,
    Amplitude = 10,
    Wavelength = 180,
    Speed = 18,            // px per second
    ShowBackWave = true
};

// Static
wave.IsAnimated = false;`
            }
        ]
    },

    gradienttext: {
        title: "GradientText — animated gradient headline",
        desc: "Text filled with a gradient that keeps moving — for hero headlines, version badges or a highlighted metric. Drawn directly with <code>FormattedText</code> and a brush transformed over time.",
        sections: [
            {
                title: "Gradient headline",
                desc: "<code>StartColor</code>/<code>EndColor</code> are the two ends, <code>Speed</code> is cycles per second and <code>Angle</code> the gradient direction. Set <code>IsAnimated=&quot;False&quot;</code> to keep it still.",
                csharp: `var headline = new GradientText
{
    Text = "CONCOST UI",
    FontSize = 34,
    StartColor = Color.FromRgb(0xFF, 0x79, 0x00),
    EndColor = Color.FromRgb(0x02, 0x84, 0xC7),
    Speed = 0.35
};

// Static text
headline.IsAnimated = false;`,
                callout: {
                    body: "Pick two colours close in hue (orange → red, blue → violet). Opposite hues (orange → blue) go grey in the middle and hurt readability."
                }
            }
        ]
    },

    cursorglow: {
        title: "CursorGlow — light that trails the pointer",
        desc: "A soft glow that follows the pointer with a slight lag, for dark hero sections or welcome panels. It never blocks input (<code>IsHitTestVisible=false</code>), so buttons underneath still work.",
        sections: [
            {
                title: "Layer it over a dark surface",
                desc: "<code>GlowSize</code> is the diameter and <code>FollowSpeed</code> controls the lag (0 = instant, 0.2 = smooth). When the pointer leaves, the glow fades out instead of popping.",
                csharp: `var glow = new CursorGlow
{
    GlowSize = 240,
    GlowOpacity = 0.42,
    FollowSpeed = 0.18
};

// Pause when the window is inactive
glow.IsAnimated = false;`,
                callout: {
                    body: "It only reads on dark surfaces or photos. On light backgrounds use <code>SpotlightCard</code> instead."
                }
            }
        ]
    },

    gridpattern: {
        title: "GridPattern — dot / line / cross backdrop",
        desc: "Three tiled backdrops drawn with a <code>DrawingBrush</code>: dots, lines or cross marks — for blueprint, drawing-paper and dashboard surfaces. Because it is a tile brush the cost is a single fill regardless of how many cells are visible.",
        sections: [
            {
                title: "Three pattern kinds",
                desc: "<code>Pattern</code> accepts <code>Dots</code>, <code>Lines</code> and <code>Cross</code>. <code>CellSize</code> is the grid step and <code>PatternOpacity</code> keeps it in the background. <code>OffsetX/OffsetY</code> phase the tile for parallax.",
                csharp: `var pattern = new GridPattern
{
    Pattern = GridPatternType.Cross,
    CellSize = 18,
    StrokeThickness = 1,
    PatternOpacity = 0.55
};

// Parallax from the scroll position
pattern.OffsetX = scrollOffset * 0.15;
pattern.OffsetY = scrollOffset * 0.08;`,
                callout: {
                    body: "Without an explicit <code>Stroke</code> the grid uses <code>CONCOST.Brush.Border</code>, so it follows the light/dark theme."
                }
            }
        ]
    },

    shakeelement: {
        title: "ShakeElement — invalid-input feedback",
        desc: "Attached behaviour that shakes an element horizontally on validation failure — 360 ms, decaying amplitude. Works on any <code>FrameworkElement</code> and preserves an existing <code>RenderTransform</code>.",
        sections: [
            {
                title: "Trigger it from code",
                desc: "Call <code>ShakeElement.Shake(element)</code>, or set the attached <code>ShakeElement.IsShaking=&quot;True&quot;</code> in XAML. It is skipped automatically when the user disabled system animations.",
                csharp: `if (!IsValid(value))
{
    ShakeElement.Shake(LengthField);
    Growl.Toast("Beam length must be > 0", GrowlType.Warning);
}`,
                callout: {
                    type: "warn",
                    body: "Do not shake the whole form. Shake the field that failed so the user knows where to look."
                }
            }
        ]
    },

    brutaltheme: {
        title: "ThemePreset.Brutal — neo-brutalism",
        desc: "The seventh <code>ThemeManager</code> preset: cream surfaces <code>#FFFDF5</code>, ink borders <code>#111111</code> and an electric accent <code>#FFE14D</code> — neo-brutalist spirit that still reads in a business app.",
        sections: [
            {
                title: "Apply and restore",
                desc: "The preset overrides seven surface/border/text brushes. The first time it runs it <b>snapshots</b> the previous values, so switching back to <code>Default</code> restores them without a restart.",
                csharp: `ThemeManager.Current.Preset = ThemePreset.Brutal;

// Back to the default palette — restored from the snapshot
ThemeManager.Current.Preset = ThemePreset.Default;`,
                callout: {
                    type: "warn",
                    title: "A colour preset, not a style set",
                    body: "It does not change border thickness or corner radius. For true brutalism (2–3px borders, square corners, hard shadows) add a dedicated style variant in <code>Generic.xaml</code>."
                }
            }
        ]
    },

    reducedmotion: {
        title: "Reduced motion — follow the system setting",
        desc: "Every effect reads <code>MotionSettings.AnimationsEnabled</code>, which defaults to the Windows “Show animations” setting (<code>SystemParameters.ClientAreaAnimation</code>). When a user disables animations, effects paint a static frame instead of freezing mid-animation — WCAG 2.2.2.",
        sections: [
            {
                title: "Nothing to configure",
                desc: "It is on by default. An app can mute everything with <code>MotionSettings.ForceDisabled</code>, which is handy when you expose a “reduce motion” toggle in settings.",
                csharp: `// Mute every effect in the app
MotionSettings.ForceDisabled = true;

// Read the current state for a settings screen
bool motion = MotionSettings.AnimationsEnabled;

// React to changes (OS setting or in-app toggle)
MotionSettings.Changed += (s, e) => UpdateMotionLabel();`,
                callout: {
                    body: "When motion is off, <code>AnimatedEffectBase</code> calls <code>OnMotionStopped()</code> so the effect can pick a sensible static frame (CursorGlow parks the glow in the middle) instead of a random one."
                }
            }
        ]
    },

    tiltcard: {
        title: "TiltCard — card that leans toward the pointer",
        desc: "The card leans toward the pointer and lifts slightly on hover. The tilt is built from <b>skew + scale (2.5D)</b> rather than <code>Viewport3D</code>, so text stays crisp and there is no extra 3D layer.",
        variants: [
            { desc: "Surface background, 1px border, 6° max tilt." },
            { desc: "No border, drop shadow, 8° tilt and 1.04 lift." }
        ],
        sections: [
            {
                title: "A member info card",
                desc: "<code>MaxTilt</code> is the angle at the edges and <code>Lift</code> the hover scale. Leaving the card eases it back to flat.",
                csharp: `var card = new TiltCard
{
    MaxTilt = 6,
    Lift = 1.02
};

// Want a cursor glow too? Nest a SpotlightCard inside.`,
                callout: {
                    body: "Keep <code>MaxTilt</code> ≤ 8°. More than that hurts readability and feels toy-like in a business app."
                }
            }
        ]
    },

    magnetichover: {
        title: "MagneticHover — button that follows the pointer",
        desc: "Attached behaviour that pulls an element a few pixels toward the pointer and eases back on leave. It moves a <code>TranslateTransform</code>, so it never triggers a layout pass.",
        sections: [
            {
                title: "Enable it on a button",
                desc: "<code>Strength</code> is how much of the pointer offset the element follows (0.1–0.4) and <code>MaxOffset</code> caps the travel. Skipped automatically when system animations are off.",
                csharp: `MagneticHover.SetIsEnabled(button, true);
MagneticHover.SetStrength(button, 0.3);
MagneticHover.SetMaxOffset(button, 12);`,
                callout: {
                    type: "warn",
                    body: "Do not enable it on every toolbar button — the eye reads it as flicker. One primary CTA per screen is enough."
                }
            }
        ]
    },

    checkmarkdraw: {
        title: "CheckMarkDraw — tick that draws itself",
        desc: "A ✓ that draws stroke by stroke in 320 ms when <code>IsChecked</code> becomes true, and erases in reverse when it goes false. For approve / save / complete flows.",
        sections: [
            {
                title: "Bind it to an approval state",
                desc: "<code>ShowCircle</code> draws a soft disc behind the tick. Set <code>Duration</code> to zero to show it instantly (for example when reloading a list of already-approved rows).",
                csharp: `ApprovalMark.IsChecked = true;

// Show immediately, no drawing
ApprovalMark.Duration = new Duration(TimeSpan.Zero);`,
                callout: {
                    body: "The default colour is <code>CONCOST.Brush.Success</code> when available, otherwise <code>CONCOST.Brush.Primary</code> — set <code>Stroke</code> to override."
                }
            }
        ]
    },

    scrollprogressbar: {
        title: "ScrollProgressBar — reading progress",
        desc: "A thin bar showing how far a <code>ScrollViewer</code> has been scrolled. Without an explicit <code>Target</code> it walks up to the nearest scroll viewer.",
        sections: [
            {
                title: "Pin it to the top of the page",
                desc: "Set <code>Target</code> explicitly when the bar sits outside the ScrollViewer; when it sits inside, no binding is needed. <code>ShowTrack=&quot;False&quot;</code> leaves only the read portion.",
                csharp: `bar.Target = PageScroll;
bar.Thickness = 3;
bar.ShowTrack = false;`,
                callout: {
                    type: "info",
                    body: "When the content is shorter than the viewport (nothing to scroll), the bar shows 100% instead of 0%."
                }
            }
        ]
    }

});
