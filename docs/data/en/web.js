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
                desc: "The ribbon measures the active tab and steps down: demote every Large to Medium, then everything to Small, then collapse groups into a single button one at a time. A collapsed group keeps all its commands in its flyout.",
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
                desc: "<code>RibbonMenuBar</code> is a horizontal strip placed above a ribbon (or used alone in a simpler app). Each <code>RibbonMenuBarItem</code> opens a dropdown of <code>RibbonMenuItem</code> and <code>RibbonMenuSeparator</code> instances, reusing the existing menu system outright. Click to open, hover to switch while a menu is open, Left/Right arrows to navigate, Escape to close.",
                csharp: `var file = new RibbonMenuBarItem { Header = "File" };
file.Items.Add(new RibbonMenuItem { Header = "New" });

var bar = new RibbonMenuBar();
bar.Items.Add(file);
bar.Items.Add(new RibbonMenuBarItem { Header = "Edit" });`
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

});
