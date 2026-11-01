// English overlay — the Revit-inspired CAD shell: viewport, navigation, command line, status bar,
// browsers and window chrome.
Object.assign(DOCS_EN.pages, {

    viewcube: {
        title: "ViewCube — orientation cube",
        desc: "A bevelled cube in the corner of the drawing area that turns with the camera: <b>26 hit regions</b> — six captioned faces, twelve edge strips and eight corner triangles — plus a compass on the XY plane, a home button, four 90° arrows, two roll buttons and a context menu. The control is a display layer only: it owns no camera, reads and writes a single <code>Quaternion</code> through <code>ICameraHost</code>, and keeps every piece of geometry in <code>ViewCubeMath</code>, so the same maths drives WinForms, OpenGL or a web viewer.",
        sections: [
            {
                title: "Dock it on the host's camera",
                desc: "<code>Host</code> takes any <code>ICameraHost</code>, and the interface has four members: <code>Rotation</code>, <code>PivotPoint</code>, <code>Projection</code> and <code>FitToView</code>. <code>CameraController</code> already implements it, so the viewport works as it stands. A camera that also implements <code>ICameraHostEvents</code> keeps the cube in step when the user orbits; one that cannot raises nothing, and the host calls <code>Invalidate()</code> from its own render loop.",
                xaml: `<Grid>
    <ui:ViewportControl x:Name="Viewport" />
    <ui:ViewCube x:Name="Cube"
                 Host="{Binding ElementName=Viewport, Path=Camera}"
                 Position="TopRight"
                 Size="Normal" />
</Grid>`,
                csharp: `// A camera from another engine: four members are enough
public sealed class MyCamera : ICameraHost, ICameraHostEvents
{
    public Quaternion Rotation { get; set; } = Quaternion.Identity;
    public Point3D PivotPoint => _pivot;
    public ProjectionMode Projection { get; set; }
    public void FitToView() => _scene.FrameEverything();

    public event EventHandler? CameraChanged;      // ICameraHostEvents
}`,
                callout: {
                    type: "note",
                    body: "Without a <code>Host</code> the cube keeps a pose of its own in <code>Rotation</code>, which is what a demo and a test want. With one, <code>Rotation</code> is a mirror of the camera, so the viewport and the cube cannot drift apart."
                }
            },
            {
                title: "26 hit regions, one source of geometry",
                desc: "Every face is a square of <code>FaceRatio</code> in its middle, every edge a bevel between two faces and every corner a small triangle — exactly 6 + 12 + 8 = 26 regions. An edge or corner normal is the normalised sum of its adjoining faces; forward and up follow the specification's table, and an edge or corner takes <code>u = (ẑ − (ẑ·f)f)</code>. Captions are upper case, centred and turned with the face. <code>ViewCubeMath.Regions</code> hands back all 26 with <code>RotationFor</code>, so posing the cube on any one of them is a single line.",
                csharp: `// Walk the 26 regions without a mouse
foreach (ViewCubeRegion region in ViewCubeMath.Regions)
    Console.WriteLine($"{region.Kind} {region.Name} f={region.Forward} u={region.Up}");

// Pose the cube on one of them
Cube.GoTo(ViewCubeMath.Regions.First(r => r.Name == "Face:FRONT"));

// What is under a point, for a test
ViewCubeHit hit = Cube.HitTest(new Point(70, 70));`,
                callout: {
                    type: "tip",
                    body: "By the Revit convention <code>FRONT</code> looks in from −Y: X to the right, Z up, so the Z on the cube stands upright as it does on the drawing. A host camera has to be Z-up too for the two to agree."
                }
            },
            {
                title: "A compass on the XY plane",
                desc: "The ring lies on the ground with N, E, S and W outside it: dragging a letter turns the model about Z (yaw), clicking the ring jumps to that heading. Seen from too low the ring flattens into a line and all four letters land on the same spot, so below 10° the dial hides itself; <code>ShowCompass=False</code> hides it always. <code>FrontAngle</code> is the angle between the model's front and the Y axis, and all four letters turn with it.",
                xaml: `<ui:ViewCube ShowCompass="True"
             FrontAngle="0" />`,
                csharp: `// A drawing turned 30° off north
Cube.SetCurrentViewAsFront();      // FrontAngle := the heading being looked from
Cube.SetCurrentViewAsHome();       // HomeRotation := the pose on screen

Cube.ResetFront();                 // put the front back on 0

Cube.FrontChanged += (_, _) => settings.Save("front", Cube.FrontAngle);`
            },
            {
                title: "Context menu, transitions and the options table",
                desc: "The menu button in the bottom-right corner opens Go Home, Orthographic, Perspective, Perspective with Ortho Faces, Set Current View as Home, Set Current View as Front, Reset Front, Show Compass and Options… — each of them runs the API above, so a ribbon can offer the same entry. Before a transition the cube raises <code>ViewRequested</code>, which the host may cancel or answer with a different <code>Target</code>; the move itself is a short-arc <code>slerp</code> eased by <code>t²(3−2t)</code> over 200-600 ms according to the angle, and an interrupted one continues from the pose on screen instead of jumping.",
                csharp: `Cube.ViewRequested += (_, e) =>
{
    if (!CanOrbit)
    {
        e.Cancel = true;               // a 2D view that cannot turn
        return;
    }

    e.Target = LockTarget(e.Target);   // or replace the pose
};

Cube.ProjectionModeChanged += (_, e) => Camera.Projection = e.Mode;
Cube.OptionsRequested += (_, _) => Host.ShowOptionsDialog();`,
                callout: {
                    type: "note",
                    body: "The specification's options table is all dependency properties: <code>Visible</code>, <code>Position</code>, <code>DockMargin</code> (10), <code>Size</code> (Automatic is a fifth of the short side clamped to 64-160 px, or Tiny 64, Small 80, Normal 100, Large 128), <code>InactiveOpacity</code> 50, <code>ShowCompass</code>, <code>SnapToClosestView</code>, <code>SnapAngle</code> 10, <code>FitToViewOnChange</code>, <code>AnimatedTransitions</code>, <code>AnimationDuration</code> 500, <code>KeepSceneUpright</code> and <code>ClickThreshold</code> 4 — a press shorter than the threshold clicks, a longer one orbits."
                }
            }
        ]
    },

    navigationbar: {
        title: "NavigationBar — the navigation strip",
        desc: "A strip of buttons for zoom in/out/extents, pan, orbit, walk plus a working-coordinate-system label. Pan, orbit and walk are <b>latched tools</b>: click again to leave, exactly like a CAD navigation bar.",
        sections: [
            {
                title: "Commands and tools are two channels",
                desc: "<code>NavigationRequested</code> fires once per click (zoom, extents…), while <code>ActiveTool</code> holds state until it changes — so a mouse drag only pans while the tool is on.",
                callout: {
                    type: "tip",
                    body: "Drive <code>ActiveTool</code> from a keyboard shortcut: <code>SetActiveTool</code> clicks the button for you, and calling it again with the same tool turns it off."
                }
            },
            {
                title: "Toggling a tool from code",
                desc: "<code>SetActiveTool</code> clicks the matching button once. Useful for a temporary pan on the Space key, or for restoring the tool a saved session was using."
            },
            {
                title: "One outline, flat commands inside",
                desc: "The old bar drew <b>three</b> layers of border: its own frame, a 1 px outline on every icon button (<code>CONCOST.Button.Icon</code> asks for 34 px and an outline), and the wrapper the demo page put around it. Now only the frame carries one — every command is flat until hovered, and the UCS label is a text button with a caret. The rules take their colour from the theme token, so they no longer turn into a bright line when the theme goes dark.",
                callout: {
                    type: "note",
                    body: "Do not wrap <code>NavigationBar</code> in another <code>Border</code> — it already has a frame. When the bar belongs inside a toolbar, set <code>Variant=\"Seamless\"</code> and it drops its own."
                },
                xaml: `<ui:NavigationBar Variant="Floating"
                  CornerRadius="6"
                  Elevation="1"
                  ButtonSize="26"
                  IconSize="14" />`,
                csharp: `// One line changes the whole dress: Floating, Seamless, Glass
void Dress(NavigationBar bar, string placement) =>
    bar.Variant = placement switch
    {
        "toolbar" => NavigationBarVariant.Seamless,
        "scene"   => NavigationBarVariant.Glass,
        _         => NavigationBarVariant.Floating
    };`
            },
            {
                title: "Three variants, three placements, one command set",
                desc: "<code>Floating</code> sits over a viewport: Surface fill, 1 px outline, a shadow sized by <code>Elevation</code>. <code>Seamless</code> drops the plate so the bar fits a toolbar or a status bar without a second outline. <code>Glass</code> is a 90% plate with a shadow, so a busy 3D scene still reads through it. <code>Orientation</code> turns the row into a column and the rules turn with it; hiding a group takes the rule beside it along.",
                xaml: `<ui:NavigationBar Variant="Seamless"
                  Orientation="Vertical"
                  ShowUcsLabel="False"
                  ShowZoom="False"
                  ShowDividers="True"
                  ShowWalk="False" />`,
                csharp: `// A touch target: the ghost style asks for 34 px, ButtonSize overrules it
bar.ButtonSize = 34;
bar.IconSize   = 16;

// Hiding a group takes its rule with it, so no line is left floating
bar.ShowUcsLabel = false;   // drops the label and the first rule
bar.ShowZoom     = false;   // drops the zoom cluster and the second rule
bar.ShowDividers = false;   // drops all three`
            }
        ]
    },

    viewcontrolbar: {
        title: "ViewControlBar — view properties strip",
        desc: "Scale, detail level, visual style, shadows, crop, reveal hidden and a hide/isolate group. Every change — <b>by pointer or by code</b> — raises <code>SettingsChanged</code> with the property name. Three variants (<code>Floating</code> / <code>Seamless</code> / <code>Glass</code>), one height for the whole bar, and a scale list the host owns.",
        sections: [
            {
                title: "One owner for the view settings",
                desc: "The host does not keep a copy: the control owns the value and reports it, so the ribbon, the properties palette and the viewport all read from one source.",
                callout: {
                    type: "note",
                    body: "Restoring a saved view is just a sequence of assignments; <code>SettingsChanged</code> fires for each one, which is what keeps AC-013 true."
                }
            },
            {
                title: "Restoring a saved view",
                desc: "Assigning a property from code raises the event too, so reopening a saved view needs no extra code path — set the values and let the viewport follow.",
                callout: {
                    type: "tip",
                    body: "The event argument is the property name (<code>nameof(ViewControlBar.VisualStyle)</code>), not the UI label, so it survives translation."
                }
            },
            {
                title: "One outline, flat controls inside",
                desc: "The old bar drew <b>four</b> layers of border: its own frame, a 1 px outline on every toggle (<code>CONCOST.ToggleButton.Icon</code> asks for 34 px and an outline), the outline of the hide/isolate button, and one more from the demo wrapper. Now only the frame carries one — the toggles and the hide button are ghost styles, flat until hovered or ticked. And the property that made the bar look coarse was an invisible number: the combo style sets <code>MinHeight</code> to 34 px, so a bar declared at 26 px still rendered 34 px tall. <code>ControlHeight</code> now zeroes the minimum before setting the height, and the rules take their colour from a theme token instead of a fixed grey.",
                callout: {
                    type: "note",
                    body: "Do not wrap <code>ViewControlBar</code> in a <code>Border</code> - it already has a frame. When the bar belongs in a status bar, set <code>Variant=\"Seamless\"</code> and it drops its own."
                },
                xaml: `<ui:ViewControlBar Variant="Floating"
                   ControlHeight="26" IconSize="14"
                   CornerRadius="6" Elevation="1" />`,
                csharp: `// One height for the whole bar: pickers, toggles and the hide button
bar.ControlHeight = 26;   // the combo style asks for 34 px; the bar overrules it
bar.IconSize      = 14;
bar.CornerRadius  = new CornerRadius(6);
bar.Elevation     = 1;    // 0 is flat, 4 is the deepest shadow`
            },
            {
                title: "Three variants, three placements, one command set",
                desc: "<code>Floating</code> sits over a viewport: Surface fill, a 1 px outline, a shadow sized by <code>Elevation</code>. <code>Seamless</code> drops the plate so the bar fits a status bar without a second outline. <code>Glass</code> is a 90% plate with a shadow, so the drawing behind it still reads. Each group can leave on its own — <code>ShowScale</code>, <code>ShowDetailLevel</code>, <code>ShowVisualStyle</code>, <code>ShowToggles</code>, <code>ShowHideIsolate</code> — and the rule beside it goes along, so a collapsed cluster never leaves a bare line at the end of the bar.",
                xaml: `<!-- Inside a status bar: no frame, no shadow -->
<ui:ViewControlBar Variant="Seamless"
                   ShowToggles="False"
                   VerticalAlignment="Center" />`,
                csharp: `// The scale list belongs to the host, and it can grow
var scales = new ObservableCollection<string> { "1:50", "1:100", "1:250" };
bar.Scales = scales;
scales.Add("1:500");     // the dropdown refills itself

// A view saved on a scale the list never offered still shows correctly
bar.ViewScale = "1:750";

// The caption is translatable
bar.HideIsolateCaption = "Hide";`
            }
        ]
    },

    viewportcontrol: {
        title: "ViewportControl — the drawing surface",
        desc: "The drawing surface: grid, crosshair, snap marker, grips, temporary dimensions, selection rectangle and an overlay layer. The camera is a real <code>CameraController</code>, so one camera drives the mini map, the viewport and a print sheet.",
        sections: [
            {
                title: "Two-way coordinates",
                desc: "<code>ToWorld</code> maps a screen point onto the plane through <code>Camera.Target</code>; <code>ToScreen</code> goes back. <code>CursorMoved</code> reports both the coordinates and whether the point snapped.",
                callout: {
                    type: "tip",
                    body: "<code>UnitsToPixels</code> converts a model length to a pixel length at the current zoom — use it to size grips and markers in model terms."
                }
            },
            {
                title: "Grips, dimensions and the overlay",
                desc: "Three properties take already-projected data: <code>Grips</code>, <code>TemporaryDimension</code> and <code>SelectionRectangle</code>. Anything else goes into <code>Overlay</code> — a Canvas on top sharing the same screen coordinate system."
            },
            {
                title: "The line through the middle is the world datum",
                desc: "One horizontal and one vertical line crossing at the centre of the surface are <b>the model's X and Y axes</b>, drawn through the origin. They are a reference, not a warning, so the default is a dash in <code>CONCOST.Brush.CanvasAxis</code>: the accent colour is kept for whatever is selected. <code>ShowAxes</code> removes both lines, <code>ShowOriginMarker</code> keeps a small ring at the origin even when the lines are gone.",
                xaml: `<ui:ViewportControl ShowAxes="True"
                    ShowOriginMarker="True"
                    AxisThickness="1"
                    AxisDashStyle="{x:Static DashStyles.Dash}" />`,
                csharp: `// null keeps the theme token; setting a brush hands the colour to the host
Viewport.AxisThickness = 1;
Viewport.AxisDashStyle = DashStyles.Solid;   // a survey grid is usually solid
Viewport.AxisBrush = Brushes.SlateGray;

// Or follow the theme even though the host set the brush
Viewport.SetResourceReference(ViewportControl.AxisBrushProperty,
                              "CONCOST.Brush.TextSecondary");`,
                callout: {
                    type: "note",
                    body: "The two axes are projected from the camera rather than assumed to be horizontal and vertical: after an orbit they tilt with it, and an axis the camera looks straight down is skipped instead of being drawn as a zero-length line."
                }
            },
            {
                title: "One brush and one switch per layer",
                desc: "The grid, the cursor, the snap marker, the pick box and the dimension each take their own brush; <code>null</code> means the theme token, so switching theme re-colours the whole surface. <code>GridMajorEvery</code> decides which line of every n is heavier, and <code>MinGridPixelSpacing</code> doubles the step until it reads — which is what keeps a dense grid from turning into a grey wash.",
                csharp: `Viewport.GridBrush = new SolidColorBrush(Color.FromRgb(0xEF, 0xF3, 0xF8));
Viewport.GridMajorBrush = new SolidColorBrush(Color.FromRgb(0xDC, 0xE3, 0xEC));
Viewport.GridMajorEvery = 5;        // every fifth line is heavier
Viewport.MinGridPixelSpacing = 8;   // under 8 px the step doubles

Viewport.CrosshairBrush = Brushes.Gray;
Viewport.ShowAperture = false;      // drop the pick box, keep the cross

Viewport.SnapMarkerBrush = Brushes.DeepSkyBlue;
Viewport.SnapMarkerSize = 6;        // px; each snap kind has its own glyph
Viewport.ShowSnapLabel = false;     // drop the GRID / ENDPOINT caption

Viewport.SelectionBrush = Brushes.DodgerBlue;
Viewport.SelectionFillOpacity = 0;  // 0 = outline only, 0.12 = default, 1 = solid
Viewport.DimensionBrush = Brushes.Navy;
Viewport.FitMargin = 1.15;          // ZoomToFit leaves 15% of air around the bounds`,
                callout: {
                    type: "tip",
                    body: "The caption beside a snap marker is the snap kind in capitals — <code>GRID</code> for a grid snap, <code>ENDPOINT</code> for an endpoint — so switch it off with <code>ShowSnapLabel</code> rather than by changing the snap kind."
                }
            }
        ]
    },

    commandline: {
        title: "CommandLine — the drafting command line",
        desc: "Command history, an input box with <b>completion</b>, arrow-key recall, Tab to finish a command name, and a <b>prompt mode</b> for a value the command still needs. The prompt is written into the history as a transcript, keywords go in brackets, three placements, and space is Enter.",
        sections: [
            {
                title: "The top line is the transcript, not a caption",
                desc: "A CAD command line is not an input box with a log beside it: it is the <b>transcript of the dialogue</b>. So <code>BeginPrompt</code> writes the question into the history and the answer joins it on the same line — <code>Specify first point: 0,0</code> — instead of standing alone. Prompt keywords sit in brackets the way CAD shows them: <code>WithOptions(\"Undo\", \"Close\")</code> renders <code>Specify next point or [Undo/Close]:</code>, typing <code>U</code> or <code>C</code> is enough, and <code>e.Option</code> reports the full keyword — <code>null</code> when the answer was a value.",
                callout: {
                    type: "note",
                    body: "Do not seed the history with <code>WriteLine</code> instructions: that space belongs to the commands, not to the help text. Keep the instructions outside the control."
                },
                xaml: `<ui:CommandLine Variant="Docked"
                 HistoryHeight="96"
                 ExpandedHistoryHeight="240" />`,
                csharp: `new CommandDefinition("LINE", "Draw a chain of segments")
    { Prompt = "Specify first point" };

commandLine.BeginPrompt("Specify next point", new[] { "Undo", "Close" });
// Specify next point or [Undo/Close]:

commandLine.PromptCompleted += (_, e) =>
{
    if (e.Option == "Undo") StepBack();          // typing "U" lands here too
    else if (e.Option == "Close") ClosePolyline();
    else if (CadInputParser.TryParseCoordinate(e.Text, out var point)) Draw(point);
};`
            },
            {
                title: "Three placements, and space is Enter",
                desc: "<code>Variant</code> decides the surface: <code>Docked</code> draws exactly one rule along the top — the other three sides are the window frame; <code>Floating</code> is a rounded plate with a shadow; <code>Inline</code> draws nothing so the control sits inside a card. <code>SpaceSubmits</code> is on by default, so the space bar runs the command like Enter — turn it off when an answer may contain spaces. <code>F2</code> expands the history to <code>ExpandedHistoryHeight</code>, and <code>ShowHistory = false</code> collapses the bar to one line, the way AutoCAD's command line folds away.",
                xaml: `<ui:CommandLine Variant="Floating" CornerRadius="6" Elevation="3" />`,
                csharp: `bar.Variant = CommandLineVariant.Docked;   // strip along the bottom of the window
bar.Variant = CommandLineVariant.Floating; // a floating plate
bar.Variant = CommandLineVariant.Inline;   // no surface at all

bar.ShowHistory       = false;  // one line only
bar.IsHistoryExpanded = true;   // as if F2 were pressed
bar.SpaceSubmits      = true;   // space acts as Enter`
            },
            {
                title: "Register commands and answer prompts",
                desc: "<code>CommandDefinition.Prompt</code> turns the next entry into an answer instead of a new command — the way CAD asks for one more parameter. Aliases give shortcuts (<code>L</code> for <code>LINE</code>).",
                callout: {
                    type: "note",
                    body: "An unknown name raises <code>UnknownCommand</code> with the raw text; the history keeps it, so the user can see what they typed."
                }
            },
            {
                title: "Reading input like a drawing",
                desc: "<code>CadInputParser</code> is a pure helper: lengths accept <code>mm</code>, <code>cm</code>, <code>m</code>, <code>in</code>, <code>ft</code> and feet-inch <code>3'-6\"</code>; a bare number means millimetres. One input box then serves both metric and imperial drawings."
            }
        ]
    },

    dynamicinput: {
        title: "DynamicInput — type where you are looking",
        desc: "The small field that follows the cursor while a command runs: type a length, an angle or a coordinate next to the geometry instead of looking at the bottom of the window. <b>One plate, one underline</b> — no input box nested inside a second frame — the question heads it, the unit is its own block, and the digits are tabular so a column of boxes lines up. A value that does not parse keeps the box <b>open</b> with the reason underneath, so the typo is never thrown away.",
        sections: [
            {
                title: "Dock it in the viewport overlay",
                desc: "Put it in the <code>Overlay</code> of a <code>ViewportControl</code>; the control places itself <code>Offset</code> away from <code>Anchor</code>, so the host only has to call <code>Open</code> and then <code>MoveTo</code> as the pointer moves.",
                callout: {
                    type: "tip",
                    body: "<code>Enter</code> commits, <code>Esc</code> cancels. A cancelled input raises <code>Cancelled</code> once — cancelling an already closed box is not an event."
                }
            },
            {
                title: "Four kinds, one pure parser",
                desc: "<code>TryParse</code> is static and needs no window, so the accepted syntax is testable. Lengths and angles go through the same <code>CadInputParser</code> as the command line, so <code>3.2m</code> means 3200 mm in both places.",
                callout: {
                    type: "note",
                    body: "<code>DynamicInputKind.Text</code> takes any non-empty answer and reports a zero number; use it for a name a command asks for mid-drawing."
                }
            },
            {
                title: "One outline, one underline, one unit block",
                desc: "The earlier box drew a rounded frame and nested a stock <code>TextBox</code> inside it — two outlines around one number, which reads as a form inside a form and sat oddly beside the rest of the library. This one keeps <b>one</b> outline, on the plate; the field uses <code>CONCOST.TextBox.Underline</code>, a 1.5 px rule that takes the focus colour by itself, so the number lies <b>on</b> the plate instead of inside a second box. The unit became its own block at the end of the question row rather than being glued onto the prompt string, so a translated question no longer turns into one long run. A value that does not parse paints that same outline in <code>CONCOST.Brush.Danger</code> and shows the reason underneath.",
                callout: {
                    type: "note",
                    body: "Every colour and font comes through <code>SetResourceReference</code>, so switching the theme repaints the box — there is not one hard-coded colour in the control."
                },
                xaml: `<ui:DynamicInput Kind="Distance" Prompt="Length" FieldWidth="140" />`,
                csharp: `input.Prompt     = "Length";      // the heading line
input.UnitSuffix = "(mm)";        // its own block on the right, not glued to the prompt
input.FieldWidth = 140;           // a coordinate box needs more room than a length
input.ShowUnit   = false;         // the drawing already carries its unit in the status bar

input.Text = "abc";
input.Commit();                   // false: the outline turns to the warning colour, the box stays open`
            },
            {
                title: "Three looks, three contexts",
                desc: "One box, three ways to sit. <code>Floating</code> keeps a 1 px outline and a shadow, for when it has to float over the drawing. <code>Compact</code> drops the outline, flattens the shadow and puts the question on the same row as the field, so it fits a 40 px tool strip. <code>Glass</code> makes the plate 6% translucent and always keeps at least a level-2 shadow so it stays readable over a dark render. Nothing is rebuilt when the variant changes — only <code>Grid.Row</code>, <code>Grid.Column</code> and the frame move — so the typed text and the focus survive the switch.",
                xaml: `<ui:DynamicInput Variant="Glass" Elevation="3" Kind="Angle" Text="45" />`,
                csharp: `input.Variant = DynamicInputVariant.Floating; // a plate over the drawing
input.Variant = DynamicInputVariant.Compact;   // one row inside a tool strip
input.Variant = DynamicInputVariant.Glass;     // a frosted plate over a render`
            },
            {
                title: "It flips at the edge, and the leader points home",
                desc: "<code>Placement = Auto</code> compares the box with its parent on each axis: near the right edge it moves left, near the bottom it moves up, and it measures again whenever the box changes size — so widening <code>FieldWidth</code> mid-command cannot push it outside. A dashed leader joins the plate's nearest corner to the exact <code>Anchor</code> under the cursor, so the eye never has to guess which point the box belongs to. A <code>Compact</code> strip has no room and drops the leader for you.",
                xaml: `<ui:DynamicInput Placement="Auto" Offset="18" ShowLeader="True" />`,
                csharp: `input.Placement  = DynamicInputPlacement.Auto;  // flips before it runs off
input.Placement  = DynamicInputPlacement.Left;  // or force Right / Left / Above
input.Offset     = 18;                          // the gap to the anchor
input.ShowLeader = false;                       // no leader at all`
            }
        ]
    },

    cadstatusbar: {
        title: "CadStatusBar — drafting status bar",
        desc: "One plate, and no border inside a border: selection and work-plane summary on the left, cursor coordinates in the middle, five snap toggles captioned SNAP, GRID, ORTHO, POLAR and OSNAP with the background task queue and the notification bell on the right. The toggles are flat and say <em>on</em> with the soft accent tint rather than a solid block of accent, so a bar with three settings enabled does not read as three alarms. The bar only keeps those bits in sync — their meaning belongs to the host.",
        sections: [
            {
                title: "Five toggles, one source",
                desc: "Declare the state once in <code>ApplicationStateService</code>; the ribbon, the shortcuts and the status bar then read and write the same value, so no two places hold the same flag.",
                xaml: `<ui:CadStatusBar Variant="Floating" SelectionCount="{Binding Selection.Count}"
               Coordinates="{Binding Cursor}"
               WorkPlane="Top"
               ToggleChanged="OnToggleChanged" />`,
                callout: {
                    type: "tip",
                    body: "<code>SetToggle</code> writes the bit without a click, and <code>Attach</code> mirrors a whole service onto the bar."
                }
            },
            {
                title: "One plate, one outline",
                desc: "The bar draws exactly one border and that is the plate. The five switches print straight onto it, flat until they are hovered; when they are on the fill is <code>PrimarySoft</code> at 12%, not the solid accent, because five permanent blocks of accent read as five alarms. The task counter and the bell wear the same dress, so no grey capsule competes with the bar. Do not wrap the bar in a <code>Border</code> of your own — two outlines around one strip is the mistake every status bar gets from its host.",
                xaml: `<ui:CadStatusBar ControlHeight="24" IconSize="13"
               CornerRadius="6" Elevation="1"
               ShowToggleLabels="True" />`,
                csharp: `bar.ShowToggleLabels = false;   // a narrow bar drops SNAP/OSNAP
bar.ControlHeight    = 30;      // a finger-sized switch
bar.CornerRadius     = new CornerRadius(15);   // a pill

// Each group goes on its own, and the rules step aside with them
bar.ShowSelection = false;
bar.ShowToggles   = false;`
            },
            {
                title: "Three dresses, three contexts",
                desc: "The same bar, and what changes is what it is standing on. <code>Floating</code> is the plate with a 1 px outline and a shadow — the bar anchored at the foot of a window. <code>Seamless</code> draws no plate at all: no outline, no shadow, so it fills a strip the host already drew without becoming a second layer. <code>Glass</code> keeps the plate at 90% opacity and always carries at least a level-2 shadow, so text stays readable over a 3D view.",
                xaml: `<ui:CadStatusBar Variant="Glass" ShowSelection="False" />`,
                csharp: `bar.Variant = CadStatusBarVariant.Seamless;  // inside the host's strip
bar.Variant = CadStatusBarVariant.Glass;     // over a 3D view
bar.Variant = CadStatusBarVariant.Floating;  // a plate with an outline and a shadow`
            },
            {
                title: "Drop a group and its rule leaves too",
                desc: "Every group has its own switch: the summary, the work plane, the toggle cluster, the task queue, the notifications and the coordinates. The rules are not fixed decoration — each one knows whether the group beside it is still there, so a bar that dropped a cluster never leaves a line floating at the end. The task counter also hides itself while the queue is empty, and <code>ShowTaskQueue = false</code> means it never appears.",
                xaml: `<ui:CadStatusBar ShowTaskQueue="False"
               ShowDividers="False" />`,
                csharp: `bar.ShowWorkPlane = false;   // the ribbon owns the plane
bar.ShowDividers  = false;   // both rules at once`
            },
            {
                title: "Coordinates, tasks and notifications",
                desc: "<code>UpdateCoordinates</code> is called per cursor move, not per painted pixel. Background tasks are a list with a percentage; notifications are a list with a severity — unread ones show a count.",
                callout: {
                    type: "note",
                    body: "<code>FormatCoordinates</code> is public, so a tooltip or a title bar can show exactly the same text as the bar."
                }
            }
        ]
    },

    projectbrowser: {
        title: "ProjectBrowser — the project tree",
        desc: "A tree of views, sheets, families, schedules and levels. Search keeps the path down to each match, filtering and grouping go by kind, sorting goes by name or kind, and the context menu differs per node kind. The search box, both pickers and the group switch already live inside the control — adding a second one outside is the fastest way to duplicate it.",
        sections: [
            {
                title: "Build the tree from the model",
                desc: "<code>ProjectTreeNode</code> is just a name, a kind and children — not a control. Add nodes, then call <code>Refresh</code> to rebuild the displayed tree.",
                callout: {
                    type: "note",
                    body: "Double-click or Enter opens a node and raises <code>OpenRequested</code>; delete is a separate request so the host can confirm it."
                }
            },
            {
                title: "One plate, no outlined chips",
                desc: "<code>Variant</code> decides who draws the edge. Take <code>Seamless</code> when the hosting palette already draws one — two nested outlines is the most common mistake when dropping this into a dock. The tools are ghost glyph buttons, so the plate is the only edge in the palette.",
                callout: {
                    type: "note",
                    body: "<code>ControlHeight</code> zeroes the <code>MinHeight</code> the combo style ships with; a minimum beats an explicit height, so 34 px would win without it."
                }
            },
            {
                title: "Search, group and sort",
                desc: "The search box, kind filter, sort picker and group switch are all inside the control. Search is case-insensitive and keeps the ancestors of each match, so the result still reads like a branch. <code>VisibleNodes</code> returns exactly what is on screen — handy for tests and for the item count."
            },
            {
                title: "The host owns the words",
                desc: "No caption is hard-coded. Placeholder, kind filter, the three sort labels, the empty-state line and the count format are all properties, so a Vietnamese shell never has to live with “All kinds”."
            }
        ]
    },

    documentworkspace: {
        title: "DocumentWorkspace — the view area",
        desc: "Hosts views as tabs, splits them vertically, horizontally or into a grid, floats one view into its own window, and lets the host <b>refuse</b> a close while there are unsaved changes.",
        sections: [
            {
                title: "Add, tile, float",
                desc: "<code>TileMode</code> divides the visible area evenly; <code>FloatDocument</code> moves a pane into its own <code>Window</code> and closes that window when the pane closes.",
                callout: {
                    type: "tip",
                    body: "<code>CloseOthers</code> and <code>CloseAll</code> go through the same closing event, so a dirty document is never dropped silently."
                }
            },
            {
                title: "Closing can be vetoed",
                desc: "<code>DocumentClosing</code> fires before the pane leaves the tree; set <code>Cancel</code> to keep it — that is where the “save changes?” question belongs.",
                callout: {
                    type: "note",
                    body: "<code>PlaceholderTitle</code> covers the no-documents case, so an empty workspace never shows a blank rectangle."
                }
            }
        ]
    },

    propertiespalette: {
        title: "PropertiesPalette — the properties palette",
        desc: "The properties of the selected object: a title, a search box, a grid grouped by category, an error line and a reset-to-default button. Every write goes through <code>TransactionManager</code>, so it is <b>one undo step</b>.",
        sections: [
            {
                title: "Declare descriptors, not reflection",
                desc: "Reflection exposes every WPF property and cannot express units, choice lists or validators. A hand-written descriptor states exactly what the grid needs and does not depend on property names.",
                callout: {
                    type: "note",
                    body: "<code>PropertyDescriptor.Validator</code> returns an error string or <code>null</code>; a rejected value never reaches the target."
                }
            },
            {
                title: "Reset to default",
                desc: "<code>ResetProperty</code> returns the descriptor's <code>DefaultValue</code> when one is declared; otherwise it returns <code>false</code> and the reason in <code>error</code> instead of failing silently.",
                callout: {
                    type: "tip",
                    body: "<code>TrySetValueFromText</code> runs the same validation path as the grid, so a command line and a palette agree."
                }
            }
        ]
    },

    applicationtitlebar: {
        title: "ApplicationTitleBar — the window title bar",
        desc: "A document-style window title: the drawing name with a <code>*</code> when there are unsaved changes, a quick-access strip that overflows into a menu, drag to move, double-click to maximise, and three window buttons that raise events.",
        sections: [
            {
                title: "A title that shows unsaved changes",
                desc: "<code>ComposeTitle</code> is a pure function, so the same string serves the caption, a tooltip and a test comparison.",
                callout: {
                    type: "note",
                    body: "<code>DocumentTitle</code> empty means an untitled drawing; the bar still shows the application name."
                }
            },
            {
                title: "Quick access overflows into a menu",
                desc: "The strip takes <code>TitleBarAction</code> items (icon, tooltip, command). When the window is narrow, the ones that no longer fit collapse into a menu button instead of being clipped."
            }
        ]
    },

    applicationinfobar: {
        title: "ApplicationInfoBar — the message bar",
        desc: "A single line under the ribbon: four severities (Info, Success, Warning, Error) decide the icon and the tinted background, plus one action and a close button. It sits in the layout and collapses itself, so the host never adds or removes a row.",
        sections: [
            {
                title: "Four severities, one action",
                desc: "One action is deliberate: a bar with three choices is a dialog pretending to be a bar. When you need real choices, use <code>Dialog</code>.",
                callout: {
                    type: "note",
                    body: "<code>Hide()</code> collapses the bar without raising <code>Closed</code>, which is what a reversible warning needs."
                }
            },
            {
                title: "Tinted from the accent colour",
                desc: "The background is the severity accent at alpha <code>0x16</code> and the border is that same accent, so a theme change carries through. The control resolves the brush on its first layout pass, so merge order does not matter."
            }
        ]
    },

    backstageview: {
        title: "BackstageView — the full-window File page",
        desc: "A full-window page behind the File button: a rail of items on the left (with a Back button) and the selected item's content on the right. It is a panel of the window rather than a popup, so Print or Options can use the whole window.",
        sections: [
            {
                title: "The content belongs to the host",
                desc: "A backstage item is only a header, an icon and any <code>Content</code>, so a file list, a print preview or a settings form all fit without the control knowing what they are.",
                callout: {
                    type: "tip",
                    body: "<code>IsSeparator</code> draws a rule above an item, which is how the File page groups its recent files and its settings."
                }
            },
            {
                title: "Closed by Esc or the Back button",
                desc: "<code>IsOpen</code> only changes <code>Visibility</code>, so the drawing area behind keeps its state. <code>IsOpenChanged</code> is where the host stops an animation or returns focus."
            }
        ]
    },

    fontpicker: {
        title: "FontPicker — font chooser",
        desc: "A font family box, a size box, bold and italic toggles, and a live preview line. Enough for a text-style dialog without wiring four controls together.",
        sections: [
            {
                title: "Preview updates as you change it",
                desc: "The preview line uses the selected family, size, weight and style — <code>EffectiveWeight</code> and <code>EffectiveStyle</code> return the combined value for the host to apply to the real object.",
                callout: {
                    type: "note",
                    body: "The family list comes from <code>Fonts.SystemFontFamilies</code>, sorted by name, so it matches what the machine can actually draw."
                }
            },
            {
                title: "No preview needed",
                desc: "<code>ShowPreview=False</code> for a toolbar; the selection still reports through <code>SelectionChanged</code>."
            }
        ]
    }

});
