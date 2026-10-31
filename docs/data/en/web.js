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
    },

    scrollreveal: {
        title: "ScrollReveal — fade in on scroll",
        desc: "Attached behaviour: the element starts faded and pushed down, then fades and slides up once it scrolls into view. It finds the parent <code>ScrollViewer</code>, reveals **once**, and stops listening.",
        sections: [
            {
                title: "Enable per block, stagger the delay",
                desc: "<code>Offset</code> is the slide distance in pixels, <code>Delay</code> creates the stagger and <code>Threshold</code> is the fraction of the viewport that triggers it (0.92 = near the bottom).",
                csharp: `ScrollReveal.SetIsEnabled(card, true);
ScrollReveal.SetOffset(card, 24);
ScrollReveal.SetDelay(card, new Duration(TimeSpan.FromMilliseconds(120)));`,
                callout: {
                    type: "warn",
                    body: "Do not enable it on hundreds of virtualised list items — every scroll walks the visual tree. Use it for a few static blocks."
                }
            }
        ]
    },

    parallaxpanel: {
        title: "ParallaxPanel — layer that drifts slower",
        desc: "A content layer that shifts with the scroll position by a <code>Speed</code> factor — put it behind the main content for a sense of depth. It never blocks input.",
        sections: [
            {
                title: "Slow background, normal content",
                desc: "<code>Speed &lt; 1</code> drifts slower, <code>&gt; 1</code> faster. <code>MaxOffset</code> caps the travel. Set <code>Axis=&quot;Horizontal&quot;</code> for horizontal scrolling.",
                csharp: `var layer = new ParallaxPanel
{
    Speed = 0.45,
    MaxOffset = 120
};

// Or bind explicitly when the panel sits outside the ScrollViewer
layer.Target = PageScroll;`
            }
        ]
    },

    typewritertext: {
        title: "TypewriterText — types character by character",
        desc: "Text appears as if being typed, with a blinking caret. Speed is in characters per second; when system animations are off the full text shows immediately.",
        sections: [
            {
                title: "Type a description line",
                desc: "<code>CharsPerSecond</code> is the speed, <code>StartDelay</code> the wait before typing and <code>IsLooping</code> restarts after <code>RestartDelay</code> seconds.",
                csharp: `var typer = new TypewriterText
{
    Text = "Quantity take-off straight from the BIM model.",
    CharsPerSecond = 26,
    StartDelay = 0.25,
    ShowCaret = true,
    IsLooping = false
};`,
                callout: {
                    body: "Keep it at 20–30 characters per second. Faster is unreadable, slower feels sluggish. To type the text again call <code>Restart()</code>."
                }
            }
        ]
    },

    textreveal: {
        title: "TextReveal — word-by-word headline",
        desc: "The headline appears word by word: each word fades and rises, offset by <code>WordDelay</code> seconds. For large headings, not for paragraphs.",
        sections: [
            {
                title: "A headline that assembles itself",
                desc: "<code>WordDelay</code> spaces the words, <code>WordDuration</code> is each word's animation and <code>OffsetY</code> is the rise distance.",
                csharp: `var reveal = new TextReveal
{
    Text = "Precise structure, safe construction",
    WordDelay = 0.08,
    WordDuration = 0.45,
    OffsetY = 14
};`,
                callout: {
                    type: "warn",
                    body: "Each word is one <code>FormattedText</code>. Beyond ~15 words use a normal <code>TextBlock</code> to avoid burning CPU every frame. To replay from the start call <code>Restart()</code> — toggling <code>IsAnimated</code> is not enough because elapsed time is kept."
                }
            }
        ]
    },

    ribbon: {
        title: "Ribbon — a tabbed command bar",
        badge: "Layout & Shell",
        desc: "An Office / AutoCAD style ribbon: a tab strip on top, each tab holds groups, each group lays its commands out on a three-row grid. When space runs out it demotes command sizes and then collapses groups into a single button — a command never disappears.",
        keywords: ["ribbon", "tab", "group", "toolbar", "office", "autocad"],
        sections: [
            {
                title: "Declaring a ribbon",
                desc: "The controls live in the <code>CONCOST.UI.Controls.Ribbon</code> namespace. <code>ItemsSource</code> takes <code>RibbonTabItem</code>s; each tab's <code>Content</code> is a list of <code>RibbonGroup</code>s.",
                xaml: `<Window xmlns:ribbon="clr-namespace:CONCOST.UI.Controls.Ribbon;assembly=CONCOST.UI">

    <ribbon:Ribbon>
        <ribbon:RibbonTabItem Header="Home">
            <ribbon:RibbonGroup Header="Clipboard">
                <ribbon:RibbonButton Header="Save" Size="Large" />
                <ribbon:RibbonSeparator />
                <ribbon:RibbonButton Header="Copy" Size="Small" />
            </ribbon:RibbonGroup>
        </ribbon:RibbonTabItem>
    </ribbon:Ribbon>
</Window>`,
                csharp: `var ribbon = new Ribbon();

var clipboard = new RibbonGroup { Header = "Clipboard" };
clipboard.Items.Add(new RibbonButton { Header = "Save", Size = RibbonItemSize.Large });
clipboard.Items.Add(new RibbonSeparator());
clipboard.Items.Add(new RibbonButton { Header = "Copy", Size = RibbonItemSize.Small });

var home = new RibbonTabItem { Header = "Home" };
home.Content = new List<RibbonGroup> { clipboard };
ribbon.ItemsSource = new List<RibbonTabItem> { home };`,
                callout: {
                    type: "tip",
                    body: "An <code>ImageSource</code> does <b>not</b> recolour itself when the theme changes. If your app switches themes at runtime, share one <b>unfrozen</b> <code>SolidColorBrush</code> between every icon and change its <code>Color</code> on the theme change — that is what the Showcase does. Otherwise the glyphs stay dark on a dark ribbon."
                }
            },
            {
                title: "Three command sizes",
                desc: "<code>Size</code> decides how many rows a command takes: <code>Large</code> = all three (28 px icon above the label), <code>Medium</code> = two, <code>Small</code> = one (16 px icon beside the label). A column holds one Medium plus one Small, or three Smalls.",
                xaml: `<ribbon:RibbonGroup Header="Mixed">
    <ribbon:RibbonButton Header="Large" Size="Large" />
    <ribbon:RibbonButton Header="Medium" Size="Medium" />
    <ribbon:RibbonButton Header="Small" Size="Small" />
</ribbon:RibbonGroup>`,
                callout: {
                    type: "tip",
                    body: "To keep every column full, order items as <code>Large</code> · <code>Medium + Small</code> · <code>3 × Small</code>. Two Mediums side by side push the second one into a new column and leave the last row empty."
                }
            },
            {
                title: "Automatic collapsing",
                desc: "The ribbon measures the active tab and steps down: demote every Large to Medium, then everything to Small, then collapse groups into a single button one at a time. A collapsed group keeps all its commands in its flyout. When the window gets narrow enough that even the tabs stop fitting, the strip clips and a <b>»</b> button appears listing every tab — nothing becomes unreachable.",
                csharp: `// Critical group: never collapses
var review = new RibbonGroup { Header = "Review", CollapsePriority = 0 };

// Secondary group: collapses first (lower number = earlier)
var utilities = new RibbonGroup { Header = "Utilities", CollapsePriority = 10 };

// Collapse the whole ribbon down to its tab strip
ribbon.IsMinimized = true;`,
                callout: {
                    type: "warn",
                    body: "<code>CollapsePriority = 0</code> pins a group: it never collapses, however narrow the ribbon gets. Use it for commands that must stay reachable."
                }
            },
            {
                title: "Group flyout",
                desc: "The arrow next to a group name opens a flyout with every command of that group — the same idea as AutoCAD's panel flyout. The flyout is at least as wide as the group and closes on Escape, on a click outside, or after a command runs.",
                csharp: `// Open/close the flyout from code
group.IsDropDownOpen = true;

// The flyout content is a menu built from the group's items
RibbonMenu flyout = group.CollapsedMenu;`,
                callout: {
                    type: "warn",
                    body: "The flyout is currently a <b>command menu</b>, not a re-laid-out panel like AutoCAD — a WPF control can only have one visual parent, so the live items cannot be reparented into the popup."
                }
            },
            {
                title: "Quick Access — always-on commands",
                desc: "The always-on strip: by default it sits <b>on the same row as the tabs</b>, hard against the right edge (`QuickAccessPlacement.HeaderRight`); switch to `AboveRibbon` or `BelowRibbon` to give it its own strip. Anything pinned there stays clickable no matter which tab is open. The chevron on the right opens the customise panel: every command in the ribbon, grouped by tab, with icons and a tick on the pinned ones. The panel is height-capped and scrolls, and stays open so several commands can be pinned in a row.",
                csharp: `// Pin / unpin
ribbon.PinToQuickAccess(saveButton);
ribbon.UnpinFromQuickAccess(saveButton);

// Show the labels, not just the icons
ribbon.QuickAccess.ShowLabels = true;

// Open the customise menu from code
ribbon.QuickAccess.IsCustomizeOpen = true;

// Move the strip: inline (default), above the tabs, or below the ribbon body
ribbon.QuickAccessPlacement = RibbonQuickAccessPlacement.AboveRibbon;

// Or drive a toolbar of your own
var bar = new RibbonQuickAccessToolBar { PinnableItems = ribbon.AllItems };
bar.Pin(ribbon.AllItems[0]);`,
                callout: {
                    type: "tip",
                    body: "A WPF control can only have one visual parent, so the toolbar does not host the original <code>RibbonItem</code> — it hosts a light clone (<code>RibbonQuickAccessButton</code>) that shares the same <code>Command</code>, icon and tooltip. Pinning a command never duplicates its logic."
                }
            },
            {
                title: "File menu — ApplicationMenu",
                desc: "<code>RibbonApplicationMenu</code> is the raised block in the top-left corner, set apart from the tab strip. Clicking it opens a two-column panel: commands on the left, recent documents on the right, plus a footer group (Options, Exit). The block is <b>neutral</b> by default (grey surface, strong text) so it does not compete with the accent colour, which belongs to the selected tab; set <code>Background</code> / <code>Foreground</code> on the control for the Office 2010 coloured block.",
                xaml: `<ribbon:Ribbon.ApplicationMenu>
    <ribbon:RibbonApplicationMenu Header="File" RecentHeader="Recent">
        <ribbon:RibbonMenuItem Header="New" InputGestureText="Ctrl+N" />
        <ribbon:RibbonMenuItem Header="Open" InputGestureText="Ctrl+O" />
        <ribbon:RibbonMenuSeparator />
        <ribbon:RibbonMenuItem Header="Print" InputGestureText="Ctrl+P" />
    </ribbon:RibbonApplicationMenu>
</ribbon:Ribbon.ApplicationMenu>`,
                csharp: `var file = new RibbonApplicationMenu { Header = "File", RecentHeader = "Recent" };
file.Items.Add(new RibbonMenuItem { Header = "New", InputGestureText = "Ctrl+N" });
file.FooterItems = new object[] { new RibbonMenuItem { Header = "Exit" } };
file.RecentItems = new object[] { "Tower-A.dwg", "Site-plan.dwg" };

ribbon.ApplicationMenu = file;`,
                callout: {
                    type: "warn",
                    body: "This is the Office 2010 <b>menu panel</b>, not the full-window backstage of 2013+. For a backstage, build your own <code>Grid</code> and swap the window content."
                }
            },
            {
                title: "Contextual tabs",
                desc: "<code>RibbonContextualTabGroup</code> wraps tabs that only exist in one mode (Sketch, Table, Modify). Drop it straight into <code>ItemsSource</code>; the ribbon expands it into its child tabs and paints a coloured band above them, merging adjacent tabs of the same group into one band.",
                xaml: `<ribbon:Ribbon>
    <ribbon:RibbonTabItem Header="Home" />

    <ribbon:RibbonContextualTabGroup Header="Drawing Tools" Accent="#D93B3B">
        <ribbon:RibbonTabItem Header="Sketch" />
    </ribbon:RibbonContextualTabGroup>
</ribbon:Ribbon>`,
                csharp: `var sketch = new RibbonContextualTabGroup
{
    Header = "Drawing Tools",
    Accent = Brushes.Crimson
};
sketch.Items.Add(new RibbonTabItem { Header = "Sketch" });

ribbon.ItemsSource = new object[] { homeTab, sketch };

// Show or hide the whole group with the current mode
sketch.Visibility = Visibility.Visible;   // or Collapsed`,
                callout: {
                    type: "tip",
                    body: "Use <code>Visibility</code> instead of adding and removing tabs: a hidden tab keeps its state and <code>SelectedIndex</code> falls back to the nearest valid tab on its own. Set <code>IsAvailable</code> on a single tab to hide just that one."
                }
            },
            {
                title: "Screen tips",
                desc: "Every command already has a screen tip built from <code>Header</code>, <code>ScreenTip</code> and <code>GestureText</code> — nothing else to write. Assign a <code>RibbonToolTip</code> when you want a picture or a footer line.",
                xaml: `<ribbon:RibbonButton Header="Save"
                     ScreenTip="Write the current drawing to disk"
                     GestureText="Ctrl+S" />

<ribbon:RibbonButton Header="Print">
    <ribbon:RibbonButton.ToolTip>
        <ribbon:RibbonToolTip Title="Print"
                              Description="Send the drawing to a printer or export a PDF."
                              GestureText="Ctrl+P"
                              Footer="Press F1 for more help" />
    </ribbon:RibbonButton.ToolTip>
</ribbon:RibbonButton>`,
                csharp: `// Generated from Header + ScreenTip + GestureText
save.GestureText = "Ctrl+S";

// Or built by hand
var tip = new RibbonToolTip
{
    Title = "Print",
    Description = "Send the drawing to a printer or export a PDF.",
    GestureText = "Ctrl+P",
    Footer = "Press F1 for more help",
    Preview = screenshot
};
print.ToolTip = tip;`,
                callout: {
                    type: "tip",
                    body: "The tip still shows on a disabled command (<code>ToolTipService.ShowOnDisabled</code>) — why a command is greyed out is usually exactly what the user needs to know. Assign your own <code>ToolTip</code> and the ribbon stops overwriting it."
                }
            },
            {
                title: "Inputs on the grid",
                desc: "<code>RibbonComboBox</code>, <code>RibbonTextBox</code> and <code>RibbonCheckBox</code> sit on the same three-row grid. All three derive from <code>RibbonItem</code>, so they keep <code>Command</code>, <code>ScreenTip</code>, <code>Size</code> and <b><code>ColumnSpan</code></b> — how many group columns the control takes, 1 by default.",
                xaml: `<ribbon:RibbonGroup Header="Text">
    <ribbon:RibbonComboBox Header="Font" ColumnSpan="2"
                           ItemsSource="{Binding Fonts}" SelectedItem="{Binding Font}" />
    <ribbon:RibbonComboBox Header="Size" ColumnSpan="2"
                           ItemsSource="{Binding Sizes}" SelectedItem="{Binding Size}" />

    <ribbon:RibbonSeparator />

    <ribbon:RibbonCheckBox Header="Snap" IsChecked="{Binding Snap}" />
    <ribbon:RibbonTextBox Header="Step" ColumnSpan="2" Text="{Binding Step}" />
</ribbon:RibbonGroup>`,
                csharp: `var font = new RibbonComboBox
{
    Header = "Font",          // label on the left; leave it empty for a bare box
    ColumnSpan = 2,           // two columns = 140 px
    ItemsSource = fonts,
    DisplayMemberPath = "Name"
};

font.SelectedItem = "Calibri";    // or font.SelectedIndex = 0

var step = new RibbonTextBox { Header = "Step", Text = "10", ColumnSpan = 2 };
var snap = new RibbonCheckBox { Header = "Snap", IsChecked = true };`,
                callout: {
                    type: "tip",
                    body: "One column is only 70 px, so <code>RibbonComboBox</code> and <code>RibbonTextBox</code> almost always want <code>ColumnSpan=\"2\"</code> — otherwise the label eats the box. Two spanning controls in a row stack inside the same column band, so the group stays narrow."
                }
            },
            {
                title: "Gallery",
                desc: "<code>RibbonGallery</code> is a selectable grid of pictures, made for colour swatches and style libraries. On the ribbon the grid draws <b>swatches only</b>; the chevron opens the full list in a pop-up where every tile carries its name. Both views share one <code>SelectedItem</code>, so a pick anywhere is the same pick.",
                xaml: `<ribbon:RibbonGroup Header="Styles">
    <ribbon:RibbonGallery ColumnSpan="3"
                          ItemWidth="34" ItemHeight="32"
                          ItemsSource="{Binding Swatches}"
                          SelectedItem="{Binding Material}" />
</ribbon:RibbonGroup>`,
                csharp: `var gallery = new RibbonGallery
{
    ColumnSpan = 3,
    ItemWidth = 34,           // ribbon cell: room for the swatch alone
    ItemHeight = 32,
    ItemsSource = new[]
    {
        new RibbonGalleryItem("Steel", steelIcon),
        new RibbonGalleryItem("Sand", sandIcon)
    }
};

gallery.SelectedItem = gallery.ItemsSource.First();
gallery.SelectionChanged += (_, _) => Apply(gallery.SelectedItem);

// The pop-up is roomier, so it can afford a caption under each tile
gallery.ExpandedItemTemplate = (DataTemplate)FindResource("MyNamedTile");

// Drop the chevron when the grid already fits
gallery.ShowExpandButton = false;`,
                callout: {
                    type: "warn",
                    body: "<code>RibbonGalleryItem</code> is a plain object (Header / Icon / ToolTip / Tag), not a control — that is what lets one item render both on the ribbon and in the pop-up. By default the ribbon grid draws the picture alone (<code>ItemTemplate</code>) and the pop-up draws picture + name (<code>ExpandedItemTemplate</code>): a 32 px cell has no room for a label, so it would either trim to <code>Gr…</code> or spill into the next row. The gallery sets <code>IsFixedSize</code>, so it is <b>not</b> squeezed into one row when the ribbon compacts."
                }
            },
            {
                title: "Key tips (Alt)",
                desc: "Press <b>Alt</b> while the ribbon is on screen: every tab, command and group launcher grows a small letter badge. Type the letters to run the command, type another tab's letter to switch tabs and re-badge, press <b>Esc</b> to leave. Nothing to declare — leave <code>KeyTip</code> empty and the ribbon derives a unique letter from <code>Header</code>.",
                xaml: `<ribbon:RibbonTabItem Header="Home" KeyTip="H">
    <ribbon:RibbonGroup Header="Font" KeyTip="F"
                        DialogLauncherCommand="{Binding OpenFontDialog}">
        <ribbon:RibbonToggleButton Header="Bold" KeyTip="B" />
        <ribbon:RibbonToggleButton Header="Italic" />      <!-- becomes "I" -->
    </ribbon:RibbonGroup>
</ribbon:RibbonTabItem>`,
                csharp: `// The ribbon already listens for Alt on its host window
ribbon.ShowKeyTips();      // or open it by hand, e.g. from a toolbar button
ribbon.HideKeyTips();

bool showing = ribbon.IsShowingKeyTips;

// Hand Alt back to the application if it means something else there
KeyTipService.SetIsEnabled(ribbon, false);

// Hosts with their own keyboard pipeline can forward keys directly
bool handled = ribbon.ProcessKeyTipKey(Key.B);`,
                callout: {
                    type: "tip",
                    body: "Badges are drawn inside the ribbon itself (<code>PART_KeyTipLayer</code>), not in an <code>AdornerLayer</code>, so they stay put inside popups and scrolling panels. Commands that are not on screen — a collapsed group, a hidden tab — get no badge and cannot be reached by key tip. The first pass also badges the open tab's commands, so what you can see is one letter away."
                }
            },
            {
                title: "Colour, search and saved state",
                desc: "<code>RibbonColorPicker</code> is the large colour tile: a swatch on the grid and the full <code>ColorPicker</code> in a popup. <code>RibbonSearchBox</code> wraps <code>SearchBox</code> into a grid cell. <code>RibbonStateStore</code> handles what users expect most — the tab they left on, the minimised state, their pinned commands and the groups they folded away.",
                xaml: `<ribbon:RibbonGroup Header="Paint">
    <ribbon:RibbonColorPicker Header="Colour" KeyTip="C"
                              SelectedColor="#DC2626" ShowAlpha="True" />
</ribbon:RibbonGroup>

<ribbon:RibbonGroup Header="Find">
    <ribbon:RibbonSearchBox Header="Find" ColumnSpan="3"
                            Placeholder="Search commands, colours…"
                            SearchCommand="{Binding SearchCommand}" />
</ribbon:RibbonGroup>`,
                csharp: `var colour = new RibbonColorPicker { Header = "Colour", ShowAlpha = true };
colour.SelectedColorChanged += (_, _) => Apply(colour.SelectedColor);

var search = new RibbonSearchBox
{
    ColumnSpan = 3,
    Placeholder = "Search commands, colours…",
    IsInstantSearch = true,       // runs as you type, no Enter needed
    SearchCommand = SearchCommand
};

// Save and restore the user's layout
RibbonStateStore.Save(ribbon.CaptureState(), "ribbon.json");
ribbon.ApplyState(RibbonStateStore.Load("ribbon.json"));

// Or keep the JSON yourself
string json = RibbonStateStore.ToJson(ribbon.CaptureState());
RibbonState? state = RibbonStateStore.FromJson(json);`,
                callout: {
                    type: "warn",
                    body: "State is matched by <code>Id</code>: a tab uses <code>Id</code> then <code>Header</code>, a command uses <code>Id</code>, a group uses <code>tab:Header</code>. Without an <code>Id</code> a command is <b>not</b> restored to the quick-access bar. Unknown ids in an old file are ignored, so an old snapshot never breaks a newer ribbon."
                }
            },
            {
                title: "MenuBar above the ribbon",
                desc: "<code>RibbonMenuBar</code> is a horizontal strip placed above a ribbon (or used alone in a simpler app). Each <code>RibbonMenuBarItem</code> opens a dropdown of <code>RibbonMenuItem</code> and <code>RibbonMenuSeparator</code> instances, reusing the existing menu system outright. Click to open, hover to switch while a menu is open, Left/Right arrows to navigate, Escape to close. The bar always spans the full ribbon width; set <code>ShowDivider=\"False\"</code> when you want it to blend into the surface.",
                csharp: `var file = new RibbonMenuBarItem { Header = "File" };
file.Items.Add(new RibbonMenuItem { Header = "New" });

var bar = new RibbonMenuBar();
bar.Items.Add(file);
bar.Items.Add(new RibbonMenuBarItem { Header = "Edit" });`
            },
            {
                title: "Host tuning properties",
                desc: "Six switches for bolting the ribbon into a specific shell: pick a tab by <code>Id</code>, drop the last rung of the collapse ladder, lock the quick-access strip, hand <code>Alt</code> back to the host, remove the outline and the group captions, and supply an empty state.",
                xaml: `<ribbon:Ribbon ShowBorder="False"
               IsGroupCollapseEnabled="False"
               IsQuickAccessCustomizable="False"
               IsKeyTipEnabled="False"
               SelectedTabId="{Binding ActiveTabId}"
               EmptyContent="No workspace open." />

<!-- Group captions are an attached property: set it on the ribbon, every group follows -->
<ribbon:Ribbon ShowGroupLabels="False" />`,
                csharp: `// Select by id: reordering tabs cannot point at the wrong one
ribbon.SelectedTabId = "insert";

// No last rung: groups shrink, but never become a single drop-down button
ribbon.IsGroupCollapseEnabled = false;

// Lock the strip: pinned commands still run, the customise button is gone
ribbon.IsQuickAccessCustomizable = false;

// Hand Alt back to Revit / AutoCAD
ribbon.IsKeyTipEnabled = false;

// Docked into a panel of its own
ribbon.ShowBorder = false;
Ribbon.SetShowGroupLabels(ribbon, false);

// Empty state
ribbon.EmptyContent = "No workspace open.";   // IsEmpty is true with no tabs`,
                callout: {
                    type: "tip",
                    body: "<code>SelectedTabId</code> and <code>SelectedIndex</code> stay in sync both ways: selecting a tab publishes its <code>Id</code>, setting an <code>Id</code> selects the tab. Turning <code>IsGroupCollapseEnabled</code> off also re-opens the groups that were just folded, so flipping it at runtime behaves."
                }
            }
        ],
        related: ["ribboncommands", "splitbutton", "concostwindow"]
    },

    ribboncommands: {
        title: "Ribbon commands — buttons, split, menus",
        badge: "Layout & Shell",
        desc: "The command set that lives inside a ribbon: buttons, toggles, radios, two-zone split buttons, dropdowns and nested menus. All of them follow <code>ICommand</code>: <code>IsEnabled</code> mirrors <code>CanExecute</code>.",
        keywords: ["ribbon button", "split button", "dropdown", "menu", "command", "radio"],
        sections: [
            {
                title: "Buttons, toggles and radios",
                desc: "<code>RibbonToggleButton</code> keeps a checked state. Give several the same <code>GroupName</code> and they behave like radio buttons; use <code>RibbonRadioButton</code> so clicking again cannot uncheck the active one.",
                xaml: `<ribbon:RibbonGroup Header="Units">
    <ribbon:RibbonRadioButton Header="mm" Size="Small" GroupName="unit" IsChecked="True" />
    <ribbon:RibbonRadioButton Header="cm" Size="Small" GroupName="unit" />
    <ribbon:RibbonRadioButton Header="m"  Size="Small" GroupName="unit" />
</ribbon:RibbonGroup>`,
                csharp: `var bold = new RibbonToggleButton { Header = "Bold", Size = RibbonItemSize.Small };
bold.Command = EditingCommands.ToggleBold;   // IsEnabled follows CanExecute

// Radio: clicking the active one keeps it checked
var mm = new RibbonRadioButton { Header = "mm", GroupName = "unit", IsChecked = true };`
            },
            {
                title: "Split button — two hit zones",
                desc: "The body runs <code>Command</code>, the arrow opens the menu. The two zones are hit-tested separately: on a large button the arrow zone is the bottom band (where the triangle sits), on a small one it is the right edge.",
                xaml: `<ribbon:RibbonSplitButton Header="Export" Size="Large">
    <ribbon:RibbonMenuItem Header="PDF" />
    <ribbon:RibbonMenuSeparator />
    <ribbon:RibbonMenuItem Header="CSV" />
</ribbon:RibbonSplitButton>`,
                csharp: `var export = new RibbonSplitButton { Header = "Export", Size = RibbonItemSize.Large };
export.Command = ExportCommand;                            // body
export.Items.Add(new RibbonMenuItem { Header = "PDF" });   // arrow
export.ArrowWidth = 20;                                    // arrow zone size`
            },
            {
                title: "Nested menus",
                desc: "<code>RibbonMenu</code> is a vertical list; <code>RibbonMenuItem</code> runs a command, or opens a submenu when it has children. <code>InputGestureText</code> only displays a shortcut — it does not register one.",
                xaml: `<ribbon:RibbonDropDownButton Header="Insert" Size="Large">
    <ribbon:RibbonMenuItem Header="Table" />
    <ribbon:RibbonMenuItem Header="Chart">
        <ribbon:RibbonMenuItem Header="Bar" />
        <ribbon:RibbonMenuItem Header="Line" />
    </ribbon:RibbonMenuItem>
    <ribbon:RibbonMenuSeparator />
    <ribbon:RibbonMenuItem Header="Link" InputGestureText="Ctrl+K" />
</ribbon:RibbonDropDownButton>`,
                callout: {
                    type: "tip",
                    body: "Submenus open on hover or with <code>→</code>, and close with <code>Esc</code>. Invoking a command closes the whole popup chain."
                }
            }
        ],
        related: ["ribbon", "splitbutton", "badge"]
    },
    datagridpanels: {
        title: "DataGrid panels — filter, search, column chooser, summary",
        badge: "Basic Controls",
        desc: "Four standalone controls that attach to an existing <code>DataGrid</code> and turn a read-only table into a data tool: a per-column filter row, a table-wide search box, a drop-down for column visibility and a summary row under the grid. None of them replaces the grid — point <code>TargetGrid</code> at it and they follow.",
        keywords: ["datagrid", "filter", "search", "column chooser", "summary", "aggregate"],
        sections: [
            {
                title: "Per-column filter row",
                desc: "One box per column; typing filters live with a case-insensitive <code>Contains</code> on the text that column shows. Each box is <b>as wide as its column</b>, so it stays under the column it filters even after a resize or a sideways scroll. Multi-level binding paths such as <code>Material.Grade</code> are read correctly.",
                xaml: `<StackPanel>
    <controls:DataGridFilterPanel TargetGrid="{Binding ElementName=beamGrid}" />

    <DataGrid x:Name="beamGrid"
              ItemsSource="{Binding Beams}"
              AutoGenerateColumns="False"
              CanUserAddRows="False">
        <DataGrid.Columns>
            <DataGridTextColumn Header="Element" Binding="{Binding Name}" Width="2*" />
            <DataGridTextColumn Header="Grade" Binding="{Binding Material.Grade}" Width="*" />
        </DataGrid.Columns>
    </DataGrid>
</StackPanel>`,
                csharp: `var panel = new DataGridFilterPanel { TargetGrid = beamGrid };

// Pre-filter a column before the user types anything
panel.Filters.Single(f => f.Header == "Grade").FilterText = "C30";

// Clear everything (or bind a button to ClearFiltersCommand)
panel.ClearFilters();

// Know when the grid has been re-filtered
panel.FiltersApplied += (_, __) => status.Text = $"{beamGrid.Items.Count} rows";`,
                callout: {
                    type: "tip",
                    body: "The filter row and the search box <b>compose</b> instead of replacing each other: a row has to satisfy both. Clearing the search leaves the column filters in place."
                }
            },
            {
                title: "Table-wide search",
                desc: "A single box that scans every visible column. Columns hidden through <code>Visibility</code> are skipped, so a table that hides internal columns does not leak them through the search box.",
                xaml: `<StackPanel>
    <controls:DataGridSearchPanel TargetGrid="{Binding ElementName=beamGrid}"
                                  PlaceholderText="Search element, concrete grade…" />
    <DataGrid x:Name="beamGrid" ItemsSource="{Binding Beams}" AutoGenerateColumns="False" />
</StackPanel>`,
                csharp: `var search = new DataGridSearchPanel { TargetGrid = beamGrid };

search.SearchText = "C30";      // filters immediately, two-way
search.ClearSearch();           // brings every row back

// IsSearching drives a "searching" hint in the toolbar
if (search.IsSearching) hint.Visibility = Visibility.Visible;`,
                callout: {
                    type: "tip",
                    body: "The box uses the <code>TextBox</code> clear button (<code>InfoElement.ShowClearButton</code>), so there is no separate Clear button to lay out."
                }
            },
            {
                title: "Column chooser",
                desc: "A drop-down listing every column of the grid; clearing a tick hides that column at once. The list rebuilds when columns are added or removed and every time the drop-down opens, so a column hidden from the host's own menu still shows the right state.",
                xaml: `<StackPanel>
    <controls:DataGridColumnChooser TargetGrid="{Binding ElementName=beamGrid}"
                                    HeaderText="Visible columns"
                                    HorizontalAlignment="Left"
                                    Margin="0,0,0,8" />
    <DataGrid x:Name="beamGrid" ItemsSource="{Binding Beams}" AutoGenerateColumns="False" />
</StackPanel>`,
                csharp: `var chooser = new DataGridColumnChooser
{
    TargetGrid = beamGrid,
    HeaderText = "Visible columns"
};

// Hide the second column, then bring everything back
chooser.Columns[1].IsVisible = false;
chooser.ShowAll();

// Open or close it from code (it closes itself on an outside click)
chooser.IsOpen = true;`,
                callout: {
                    type: "tip",
                    body: "Columns are matched by <b>reference</b>, not by header text, so two columns that share a caption still hide the right one. Hide the button itself when a print view must not allow it."
                }
            },
            {
                title: "Summary row",
                desc: "One cell per column under the grid: numbers show <code>Sum</code>, text columns show the row count. The row <b>mirrors the grid's geometry</b> — column widths, display order, the row-header width and the horizontal scroll offset — so a value always sits under its column.",
                xaml: `<StackPanel>
    <DataGrid x:Name="beamGrid" ItemsSource="{Binding Beams}" AutoGenerateColumns="False" />
    <controls:DataGridSummary TargetGrid="{Binding ElementName=beamGrid}" DisplayFormat="N0" />
</StackPanel>`,
                csharp: `var summary = new DataGridSummary
{
    TargetGrid = beamGrid,
    Aggregate = DataGridSummaryAggregate.Auto,   // numbers → Sum, text → Count
    DisplayFormat = "N0"
};

// Override the aggregate for one column
DataGridSummary.SetAggregateOnColumn(lengthColumn, DataGridSummaryAggregate.Average);

// Recompute when the data changes outside the binding
summary.RefreshSummaries();`,
                callout: {
                    type: "tip",
                    body: "Aggregates run on the <b>filtered</b> view: if the filter leaves five rows, the sum is those five rows. Numbers are read in the current culture first and the invariant culture second, so Vietnamese <code>\"1,5\"</code> data still adds up."
                }
            }
        ],
        related: ["datagrid", "datapager", "stackedheaderdatagrid"]
    },

    commandpalette: {
        title: "CommandPalette — the Ctrl+K command bar",
        desc: "A search box over the whole screen that filters your commands as you type and runs them on <code>Enter</code>. Search <b>ignores Vietnamese diacritics</b>, the arrow keys move the highlight, and the control paints its own scrim — no popup to manage, no focus dance.",
        variants: [
            { desc: "Dark scrim <code>#8C0F172A</code>, card up to 600px wide, 44px rows, footer with the key hints." }
        ],
        sections: [
            {
                title: "Opening the palette with Ctrl+K",
                desc: "Put the palette in the same <code>Grid</code> as the page content and bind <code>IsOpen</code>. The shortcut belongs to the app — the control never grabs a global key while it is not in the visual tree.",
                callout: {
                    type: "tip",
                    body: "To keep the palette open for a run of commands set <code>CloseOnInvoke=\"False\"</code>; by default it closes itself after invoking."
                }
            },
            {
                title: "Diacritic-insensitive search, one token at a time",
                desc: "The query is normalised to <code>FormD</code> and stripped of marks, with <code>đ</code> mapped to <code>d</code> because <code>FormD</code> does not decompose it. Each token then matches independently across title, subtitle, category, shortcut and <code>Keywords</code>.",
                callout: {
                    type: "note",
                    body: "The list height snaps to <b>whole rows</b> (44px each, <code>MaxListHeight</code> defaults to 276 = six rows) so a half command never appears at the bottom edge. The highlight always follows the filtered list, so it cannot point at a row that was just filtered away."
                }
            }
        ]
    },

    radialgauge: {
        title: "RadialGauge — circular KPI readout",
        desc: "A track arc, a value arc, optional tick marks and a formatted number in the middle, drawn in <code>OnRender</code> with <code>StreamGeometry</code> and round caps, so it stays crisp at any size and any DPI. <code>Value</code> clamps to <code>Minimum</code>–<code>Maximum</code> and eases into place.",
        sections: [
            {
                title: "A percentage gauge",
                desc: "The ring opens at the bottom by default (starts at 135°, sweeps 270°). <code>ValueFormat</code> takes a standard numeric format; <code>Suffix</code> and <code>Label</code> are the text around the number.",
                callout: {
                    type: "tip",
                    body: "The value arc takes <code>ValueBrush</code>, the track takes <code>TrackBrush</code> — leave them empty and they come from the theme, so a Light/Dark switch recolours the gauge for free. Ticks use <code>TextTertiary</code>: the track colour makes them almost invisible."
                }
            },
            {
                title: "A reversed scale, e.g. depth",
                desc: "Set <code>Minimum</code> above <code>Maximum</code> for a reversed scale, or change <code>StartAngle</code>/<code>SweepAngle</code> to open the ring along the top for a short dashboard strip.",
                callout: {
                    type: "note",
                    body: "Each gauge is only a handful of primitives, but <b>do not</b> animate hundreds of gauges that update every second — every value change starts a <code>DoubleAnimation</code>."
                }
            }
        ]
    },

    particlefield: {
        title: "ParticleField — ambient particle backdrop",
        desc: "Slowly drifting particles for hero panels, splash screens and empty states. Particles wrap around the edges so the field never empties, <code>Seed</code> makes the layout reproducible, and the field rebuilds itself when its host is resized. Set <code>ConnectDistance</code> for the constellation look.",
        sections: [
            {
                title: "A particle backdrop for a hero card",
                desc: "Place it as the first child of a <code>Grid</code> so it sits behind the content. <code>IsHitTestVisible</code> is already off, so it never swallows clicks.",
                callout: {
                    type: "note",
                    body: "The field rebuilds its particles when the real size differs from the size it was built for — otherwise the particles only fill the top third of the box."
                }
            },
            {
                title: "Constellation: connecting nearby particles",
                desc: "Set <code>ConnectDistance</code> above 0 to draw lines between particles that are close. The distance is in pixels; larger values mean more lines and more CPU.",
                callout: {
                    type: "tip",
                    body: "Budget: about 60 unconnected particles are comfortable; with <code>ConnectDistance</code> the cost grows <b>quadratically</b> with the particle count — 48 is a safe ceiling for a full-window backdrop."
                }
            }
        ]
    },

    pagetransitionhost: {
        title: "PageTransitionHost — animated page swap",
        desc: "Change <code>Content</code> and the new page animates in: fade, slide from any side, or scale. The animation lives on the host, so it works with any content and needs no storyboard per page.",
        variants: [
            { desc: "Minimal template: a <code>ContentPresenter</code> wrapped in a <code>TransformGroup</code> (scale + translate)." }
        ],
        sections: [
            {
                title: "Sliding horizontally when the screen changes",
                desc: "<code>Transition</code> accepts <code>Fade</code>, <code>SlideLeft</code>, <code>SlideRight</code>, <code>SlideUp</code>, <code>SlideDown</code> or <code>Scale</code>. <code>Offset</code> is the travel distance, <code>Duration</code> the run time.",
                callout: {
                    type: "tip",
                    body: "The animation is <b>skipped entirely</b> when the control is not loaded, when <code>IsTransitionEnabled=\"False\"</code>, or when Windows animations are off — the content still swaps instantly and never gets stuck transparent."
                }
            },
            {
                title: "Picking a direction that matches the navigation",
                desc: "Going forward usually slides left, going back slides right. Bind <code>Transition</code> to the navigation direction and the motion reads correctly.",
                callout: {
                    type: "note",
                    body: "The host animates only the <b>incoming</b> content, never the outgoing one — one animation per swap, and no need to keep two views alive at once."
                }
            }
        ]
    },

    treedatagrid: {
        title: "TreeDataGrid — a grid that shows a hierarchy",
        desc: "A <code>DataGrid</code> for trees: bind <code>RootItems</code> and the grid flattens the tree into rows. Each row is <b>the item itself</b>, not a wrapper, so every column keeps binding straight to the item's properties.",
        sections: [
            {
                title: "A basic data tree",
                desc: "The first column is generated: it draws the indent and the expander. <code>HierarchyPath</code> names the property to show — the same idea as <code>DisplayMemberPath</code>. For anything richer, use <code>HierarchyContentTemplate</code>.",
                callout: {
                    type: "note",
                    body: "Set both and the template wins. Expansion state is keyed by <b>object reference</b>, not by id or index — two nodes with equal values are still two different branches, which is why the state survives every rebuild of the flat list."
                }
            },
            {
                title: "The generated column and the visible count",
                desc: "Set <code>ShowHierarchyColumn=\"False\"</code> to use it as a flat table; <code>VisibleItemCount</code> reports how many rows are on screen (expanded branches only), which is what a status strip wants.",
                callout: {
                    type: "tip",
                    body: "<code>CONCOST.TreeDataGrid.Base</code> is BasedOn <code>CONCOST.DataGrid.Base</code>, so the template, row style, grid lines and virtualization are shared — there is no second template copy to drift later."
                }
            }
        ]
    },

    multiselectdatagrid: {
        title: "MultiSelectDataGrid — checkbox multi-selection",
        desc: "A DataGrid with a checkbox column and a tri-state header checkbox. Clicking a checkbox cell <b>toggles that row</b> instead of replacing the selection, so picking twenty rows does not need the Ctrl key.",
        sections: [
            {
                title: "Select many, read the list",
                desc: "<code>SelectedItemsList</code> raises change notifications (unlike the DataGrid's own <code>SelectedItems</code>), so a command can bind to it and a button can enable itself.",
                callout: {
                    type: "note",
                    body: "<code>IsAllSelected</code> is tri-state: <code>true</code> when everything is selected, <code>false</code> when nothing is, <code>null</code> when only some are — the header draws the indeterminate bar you expect."
                }
            },
            {
                title: "A plain click stays a plain click",
                desc: "Only the checkbox cell toggles. Clicking anywhere else in the row behaves like a normal DataGrid: a plain click selects, Ctrl and Shift extend.",
                callout: {
                    type: "tip",
                    body: "The cell checkbox has <code>IsHitTestVisible=\"False\"</code> — the click lands on the row, which is exactly what makes a plain click toggle instead of replace."
                }
            }
        ]
    },

    treemap: {
        title: "TreeMap — area-proportional map",
        desc: "Each rectangle's area is proportional to its value, packed <b>squarified</b> so cells stay close to square instead of becoming long strips. Good for quantity breakdowns, cost allocation and consumption by category.",
        sections: [
            {
                title: "Quantity breakdown by element",
                desc: "<code>ValuePath</code> is the weight, <code>LabelPath</code> the caption. Zero and negative values are dropped — a zero-area rectangle is only an invisible sliver.",
                callout: {
                    type: "note",
                    body: "<code>Entries</code> returns the rectangles that were actually drawn (with <code>Bounds</code> and <code>Share</code>), so hit-testing, tooltips and your own labels always match the picture — no second layout pass."
                }
            },
            {
                title: "One colour, or no labels",
                desc: "Leave <code>Palette</code> empty and every cell uses <code>Fill</code> (the theme accent by default). Turn labels off when the cells get too small to read.",
                callout: {
                    type: "tip",
                    body: "The layout recomputes on resize. For dense data set <code>Gap=\"0\"</code> so no area is lost to the gutters."
                }
            }
        ]
    },

    dockingmanager: {
        title: "DockingManager — Visual Studio-style layout",
        desc: "A panel that arranges <code>ToolWindow</code> panes on four edges plus a centre. Panes on the same edge become <b>tabs of one group</b>, sizes stay proportional when the window resizes, headers drag with five drop zones, and the layout can be saved and restored.",
        sections: [
            {
                title: "Pane groups and tabs",
                desc: "Every pane declared <code>Dock=\"Left\"</code> belongs to <b>one group</b> on the left edge. From two panes up, the group shows as tabs along the bottom — each pane does not get its own slice of the edge. The selected tab is <code>IsActive</code>, and a pane's own header hides because the tab already names it.",
                callout: {
                    type: "tip",
                    body: "Sizes are synchronised inside a group: dragging a splitter or setting one pane's <code>DockSize</code> gives the other tabs the same size, so switching tab never makes the edge jump."
                }
            },
            {
                title: "Proportional sizing",
                desc: "By default a pane keeps its <b>share</b> of the window when the window resizes, like a <code>*</code> column in a <code>Grid</code>. Set <code>IsProportional=\"False\"</code> to pin it to pixels instead.",
                callout: {
                    type: "note",
                    body: "A splitter drag updates <code>DockSize</code> for the whole group; the next window resize takes the new ratio as its baseline."
                }
            },
            {
                title: "Drag to redock — five drop zones",
                desc: "Hold and drag a header: the region the pointer is aiming at lights up as a dashed outline. The zone is computed <b>per pane</b> (25% on each side, the rest is the centre), so dragging into the left quarter of the document area docks left rather than following the window's own edge bands.",
                callout: {
                    type: "tip",
                    body: "The outline shows the region the pane will actually occupy after the drop, not half of the hovered pane — the hint never lies."
                }
            },
            {
                title: "Nested groups — the layout is a tree",
                desc: "<code>DockPaneGroup</code> divides its area between its children by <code>Proportion</code>, and a child may be another group. That is what makes “the left edge split into two rows, the top row tabbed” expressible — something four fixed slots could never do.",
                callout: {
                    type: "tip",
                    body: "<code>Mode=\"Tabs\"</code> turns a group into tabs (only the child that is <code>IsActive</code> shows); <code>Mode=\"Split\"</code> divides the space. Each group draws and drags its own splitters, so nesting can go as deep as you like."
                }
            },
            {
                title: "Floating windows",
                desc: "Drag a header **outside** the docking area and the panel tears out into its own window. Call <code>Float(pane)</code> to do it from code; closing that window docks the panel back to the same edge at the same size.",
                callout: {
                    type: "note",
                    body: "A floating panel has <code>CanAutoHide</code> turned off — there is no edge to pin it to — and it is restored when the panel docks back."
                }
            },
            {
                title: "Dragging tabs",
                desc: "Hold and drag a tab: drop it back on the strip and it **reorders**, drop it on another edge and the panel joins that group, drop it outside the window and it tears out into a floating window. All three are callable from code.",
                callout: {
                    type: "tip",
                    body: "<code>DockAt</code> takes a point in the manager's coordinates: outside the bounds it floats the panel, inside it docks to the drop zone. That is what lets a custom tab strip reuse the whole gesture."
                }
            },
            {
                title: "Saving and restoring the layout",
                desc: "Each pane saves its edge, size, auto-hide state and whether it is the selected tab. Restoring matches panes by <code>Header</code>, so an unknown pane in an old file is skipped instead of breaking the layout.",
                callout: {
                    type: "note",
                    body: "The format is plain text with a version header (<code>CONCOST-DOCK-1</code>), so it can be read by eye and evolved without a JSON dependency."
                }
            }
        ]
    },

    searchabletreepicker: {
        title: "SearchableTreePicker — a tree with a search box",
        badge: "New",
        desc: "A search box in front of a tree: type to narrow it down to the matching nodes <b>and the branches that lead to them</b>, then pick one or several.",
        sections: [
            {
                title: "Filter, but keep the path",
                desc: "The filter keeps the ancestors of every match. Dropping every non-matching node would leave a flat list of leaves with no idea where they sit — keeping the path is what makes the result readable.",
                callout: {
                    type: "tip",
                    body: "Matching goes through <code>CommandPaletteText.Normalize</code>: <code>dam d1</code> finds “Dầm D1”, <code>tang 1</code> finds “Tầng 1”. In a Vietnamese-first library that is the difference between usable and not."
                }
            },
            {
                title: "One or many",
                desc: "<code>SelectionMode=\"Multiple\"</code> shows a checkbox per row. <code>SelectAllVisible()</code> takes only what is on screen — after a search, “select all” really means all the results.",
                callout: {
                    type: "note",
                    body: "Switching from Multiple to Single trims the selection to the last pick instead of leaving an invalid state behind."
                }
            }
        ]
    },

    meshgradient: {
        title: "Mesh gradient & spotlight",
        badge: "New",
        desc: "Two backdrop effects: <code>AnimatedMeshGradient</code> makes four colour blobs breathe around fixed anchors, and <code>CursorSpotlight</code> dims everything except a circle around the pointer.",
        sections: [
            {
                title: "A mesh, not an aurora",
                desc: "Unlike <code>AuroraBackground</code>: aurora drifts its blobs across the surface and reads as weather; a mesh keeps them near fixed anchors and only lets them oscillate, so it looks like a still image that happens to move. The radius is based on the <b>longer</b> side — using the shorter one leaves gaps between the anchors on a wide panel.",
                callout: {
                    type: "note",
                    body: "Drawn with radial gradients that fade to transparent, no <code>BlurEffect</code> — far cheaper at full window size."
                }
            },
            {
                title: "A spotlight that never steals a click",
                desc: "<code>CursorSpotlight</code> is not hit-testable, yet it still follows the pointer because <code>Mouse.GetPosition</code> reads the current position rather than requiring events. Set <code>FollowCursor=\"False\"</code> and drive <code>SpotlightCenter</code> yourself to follow a tour script instead.",
                callout: {
                    type: "tip",
                    body: "Paint <b>one</b> rectangle over the whole control with an absolute gradient and <code>SpreadMethod=Pad</code>. Drawing only an ellipse gives a dark ring in the middle — the exact opposite of what a spotlight means."
                }
            }
        ]
    },

    glasscard: {
        title: "Frosted glass & gradient ring",
        badge: "New",
        desc: "Two surfaces: <code>FrostedGlass</code> blurs exactly the region behind it, and <code>AnimatedGradientBorder</code> gives a card a gradient ring that rotates.",
        sections: [
            {
                title: "Frosted glass without a backdrop filter",
                desc: "WPF has no backdrop filter, so the blur is built: point <code>Source</code> at the element behind and the control paints a <code>VisualBrush</code> of it through a <code>BlurEffect</code>. The brush's viewbox is set to the panel's rectangle <b>in the source's coordinates</b>, so the blur lines up instead of stretching the whole backdrop into the panel.",
                callout: {
                    type: "warning",
                    body: "That copy is re-rendered whenever the source changes, so this is a control for <b>a few panels over a hero</b> — not a backdrop for a long scrolling list."
                }
            },
            {
                title: "The ring rotates with an animation, not OnRender",
                desc: "<code>AnimatedGradientBorder</code> rotates a <code>RotateTransform</code> attached to the brush with a <code>DoubleAnimation</code>. WPF runs it on the render thread, so a dozen of these cost the UI thread nothing and stay smooth while the window is busy.",
                callout: {
                    type: "note",
                    body: "When <code>MotionSettings.AnimationsEnabled</code> is off the animation is cleared and the ring keeps its first frame — a reduced-motion user sees the same card, not an empty box."
                }
            }
        ]
    },

    motion: {
        title: "Stagger & shared element",
        badge: "New",
        desc: "Two kinds of motion: a list arriving item by item, and an element growing out of where it used to be.",
        sections: [
            {
                title: "A list that arrives in a wave",
                desc: "<code>StaggerAnimation</code> is an attached behaviour: the delay is derived from the item's position, so it works with any panel and any item template. Containers are tracked by <b>reference</b> — a virtualising panel reuses them, and without that an item would replay its entrance every time it scrolls back into view.",
                callout: {
                    type: "tip",
                    body: "<code>MaxItems</code> caps the delay: a 200-row list does not take ten seconds to finish arriving."
                }
            },
            {
                title: "Shared element",
                desc: "<code>SharedElementTransition</code> never animates layout — the element is already in its final place, and is scaled and shifted back to the old rectangle for the first frame, then eased to identity. No layout thrash, and the element stays interactive the whole time.",
                callout: {
                    type: "note",
                    body: "Both rectangles must share a coordinate space — normally the element's parent. The transform is handed back at the end so hit-testing and clipping stay correct."
                }
            }
        ]
    },

    dragdrop: {
        title: "Drop zone, highlight & drag ghost",
        badge: "New",
        desc: "The three pieces of a drag gesture: <code>FileDropZone</code> takes files, <code>DropTargetHighlight</code> draws the hover outline around any element, and <code>DragGhost</code> carries a caption under the pointer.",
        sections: [
            {
                title: "The file drop area",
                desc: "<code>Filter</code> is checked <b>while the drag is still in the air</b>, so the zone only lights up for files it will take. Accepting every drag and then refusing the drop is what makes a drop area feel broken.",
                callout: {
                    type: "tip",
                    body: "The library never opens a file dialog itself: which picker to show, and whether the host wants one at all, is not a control's decision."
                }
            },
            {
                title: "A hover outline for any element",
                desc: "<code>DropTargetHighlight</code> is an attached property that draws a dashed outline with an <b>adorner</b>, so it sits above the content and never changes the layout — no extra border, no padding shift. It only reports the hover; accepting the drop stays the element's job through <code>DragDropEffects</code>.",
                callout: {
                    type: "note",
                    body: "Keeping those apart is what lets a host highlight a whole card while only one row inside it accepts the data."
                }
            },
            {
                title: "The ghost under the pointer",
                desc: "<code>DragGhost</code> is a transparent, topmost, hit-test-invisible window offset 14px from the pointer. There is one ghost per process — a user cannot drag two things at once.",
                callout: {
                    type: "tip",
                    body: "Pass <code>Move</code> the visual the pointer belongs to; the DPI conversion happens inside, so the ghost lands where the pointer is."
                }
            }
        ]
    },

    documenttabcontrol: {
        title: "DocumentTabControl — document tabs",
        desc: "A tab control for documents: each tab has an icon, an unsaved dot and a close button. Closing is a <b>request</b> — the host can veto it to ask about saving first.",
        sections: [
            {
                title: "Tabs with a close button and a dirty dot",
                desc: "<code>IsDirty</code> only shows the dot — the control never clears it, because saving belongs to the host. <code>CanClose=\"False\"</code> hides the close button on that one tab.",
                callout: {
                    type: "note",
                    body: "<code>Close()</code> raises the bubbling <code>CloseRequested</code> routed event and <code>DocumentTabControl</code> catches it with a class handler, so the button, a context menu and a shortcut all take the same path."
                }
            },
            {
                title: "Turning the close buttons off",
                desc: "When the window shows a single document, hide the buttons entirely — or hide them per tab.",
                callout: {
                    type: "tip",
                    body: "<code>CONCOST.DocumentTabControl.Base</code> inherits <code>CONCOST.TabControl.Base</code> but swaps <code>ItemContainerStyle</code> for the document style — without that swap the tabs fall back to the plain TabItem template and lose the close button."
                }
            }
        ]
    },

    statusbar: {
        title: "StatusBar — the status strip",
        desc: "The information strip along the bottom of a window. Each cell is a <code>StatusBarItem</code>; a cell marked <code>Alignment=\"Right\"</code> hugs the right edge and the gap in the middle absorbs the slack.",
        sections: [
            {
                title: "Two sides, one gap in the middle",
                desc: "<code>StatusBarPanel</code> lays left cells out from the left edge and right cells from the right edge, so you do not need a <code>Grid</code> with a <code>*</code> column to push the two groups apart.",
                callout: {
                    type: "tip",
                    body: "Declaration order sets the order within each group, not the position on screen — a <code>Right</code> cell declared between two <code>Left</code> ones still sits on the right."
                }
            },
            {
                title: "Clickable cells",
                desc: "Give a cell a <code>Command</code> to turn it into a button — opening a log panel or switching units, for instance. A cell with no command keeps the default cursor instead of pretending to be clickable.",
                callout: {
                    type: "note",
                    body: "<code>Content</code> wins over <code>Text</code>: to put a control in a cell (a progress bar, a unit combo) drop <code>Text</code> rather than setting both."
                }
            }
        ]
    },

    commandbar: {
        title: "CommandBar — a command strip with a “more” menu",
        desc: "A single-row toolbar of icon-and-label buttons that pushes whatever does not fit into a drop-down instead of clipping it.",
        sections: [
            {
                title: "Declaring commands as data",
                desc: "Each entry is a <code>CommandBarItem</code>. The control does not decide what fits — <code>OverflowPanel</code> measures during layout and reports back, so <code>OverflowItems</code> always matches what is actually hidden.",
                callout: {
                    type: "tip",
                    body: "<code>ShowLabel=\"False\"</code> keeps the icon and drops the caption — use it on secondary commands to keep the strip short enough that it never needs to overflow."
                }
            },
            {
                title: "The overflow menu and its state",
                desc: "<code>HasOverflow</code> and <code>OverflowItems</code> are read-only and refresh after every layout pass. <code>IsOverflowOpen</code> is two-way, so you can bind it and close the menu when the user switches tabs.",
                callout: {
                    type: "note",
                    body: "When nothing is hidden any more the control sets <code>IsOverflowOpen = false</code> itself — the menu can never open empty."
                }
            }
        ]
    },

    quickaccesstoolbar: {
        title: "QuickAccessToolbar — the icon strip on top",
        desc: "The standalone version of the ribbon's quick-access toolbar: icons only, 28px tall, usable in any window without a ribbon.",
        sections: [
            {
                title: "Icon, tooltip, done",
                desc: "Each entry is a <code>QuickAccessItem</code> — icon, tooltip, command. No captions, no groups: this is the place for the commands used every day.",
                callout: {
                    type: "tip",
                    body: "<code>IsCustomizable=\"False\"</code> hides the chevron when nothing has overflowed — a good fit for a child window with three or four fixed commands."
                }
            },
            {
                title: "Overflowing into a menu",
                desc: "Same mechanism as <code>CommandBar</code>: when the space runs out the extra items drop into a popup rather than being clipped.",
                callout: {
                    type: "note",
                    body: "This is a standalone control, not a child of <code>Ribbon</code>. The ribbon keeps its own internal <code>RibbonQuickAccessToolBar</code> — same idea, different scope."
                }
            }
        ]
    }

});
