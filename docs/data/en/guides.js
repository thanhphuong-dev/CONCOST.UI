// English overlay — getting started, plugin hosting, theming, attached properties, FAQ, migration.
Object.assign(DOCS_EN.pages, {

    intro: {
        title: "A WPF library for engineering, CAD &amp; BIM",
        desc: "CONCOST.UI is a WPF component library tuned for structural, BIM, CAD and technical desktop applications. It ships design tokens that adapt to light and dark, six theme presets and more than 60 controls.",
        heroStats: [
            { label: "Controls" },
            { label: "Target frameworks" },
            { label: "Theme presets" },
            { label: "API properties" }
        ],
        quick: [
            { title: "Use inside a plugin DLL", desc: "Revit, AutoCAD, Tekla, Navisworks — no App.xaml required" },
            { title: "Theme & Design Tokens", desc: "Accent colour, presets, dark mode and system tokens" },
            { title: "Attached Properties", desc: "Icon, badge, placeholder, validation, drag-and-drop" },
            { title: "Spreadsheet", desc: "Formula-driven engineering worksheets with multiple sheets" },
            { title: "RebarInput", desc: "Parse rebar notation and compute As (mm²)" },
            { title: "FAQ & Troubleshooting", desc: "The integration problems people actually hit" }
        ],
        sections: [
            {
                title: "Install and integrate through XAML (standalone WPF app)",
                desc: "Register the CONCOST.UI dictionaries in <code>App.xaml</code>. <b>ThemeResources.xaml</b> already merges the token dictionaries (Colors, Typography, Spacing, CornerRadius) plus the default Light theme — one line is enough for the foundation.",
                callout: {
                    title: "Do not merge Themes/Light.xaml on its own",
                    body: "That file only contains theme brushes. <code>CONCOST.FontSize.*</code>, <code>CONCOST.Spacing.*</code> and <code>CONCOST.CornerRadius.*</code> live in <code>Themes/Tokens/*</code> and are only loaded through <code>ThemeResources.xaml</code>. Merging without them leaves styles unresolved and controls sized incorrectly."
                }
            },
            {
                title: "2. Declare the namespaces in your view",
                desc: "Declare the XML namespaces for controls, icons and attached properties:"
            }
        ]
    },

    "class-library": {
        title: "Inside a Class Library &amp; CAD/BIM plugin",
        desc: "How to host CONCOST.UI inside a plugin DLL for Revit, AutoCAD, Tekla or Navisworks: scoped resources, runtime theme switching and integration with dockable panels — without polluting the host application.",
        sections: [
            {
                title: "1. Architecture: standalone exe versus class library DLL",
                desc: "A standalone WPF app owns <code>Application.Current</code>. A plugin DLL does not — the host process does. Writing to <code>Application.Current.Resources</code> from a plugin can break the host UI.",
                table: [
                    { token: "Problem: Application.Current", desc: "Owned by the host application. Overwriting <code>Application.Current.Resources</code> can damage the Revit/AutoCAD UI or crash the process." },
                    { token: "Problem: assembly loading", desc: "Resource dictionaries (<code>Generic.xaml</code>, <code>Light.xaml</code>, <code>Dark.xaml</code>) must be loaded through an explicit pack URI that names the assembly." },
                    { token: "Solution: local resource scoping", desc: "<code>ThemeManager.Current.ApplyTheme(rootElement, theme)</code> loads styles, brushes and tokens into the plugin's root element (<code>Window</code> or <code>UserControl</code>), fully isolated from the host." }
                ]
            },
            {
                title: "2. Method 1: load automatically in the C# constructor (recommended)",
                desc: "Merge the dictionaries into the view's own <code>Resources</code> before the first render, then apply the theme to that root element."
            },
            {
                title: "3. Method 2: declare the dictionaries statically in XAML",
                desc: "Equivalent to method 1, but keeps the resource wiring declarative inside <code>UserControl.Resources</code>."
            },
            {
                title: "4. Runtime theme and accent switching",
                desc: "Switch between light and dark while the plugin is open; the change stays scoped to the plugin view."
            },
            {
                title: "5. Use inside a dockable panel (Revit IDockablePaneProvider / AutoCAD PaletteSet)",
                desc: "Dockable panes are long-lived controls: apply the theme once in the constructor and reuse the instance instead of recreating it on every activation."
            },
            {
                title: "6. Complete integration sample: Revit IExternalCommand with Win32 owner handle",
                desc: "Shows how to parent the WPF window to the Revit main window so the dialog stays on top and minimises with the host."
            }
        ]
    },

    theming: {
        title: "Theme system &amp; colour tokens",
        desc: "Every colour and surface in CONCOST.UI is resolved through <code>DynamicResource</code>, which lets the application switch between dark and light at runtime without rebuilding controls.",
        sections: [
            {
                title: "1. Standard colour and brush tokens",
                desc: "Reference these keys instead of hard-coded colours so the UI follows the active theme and accent.",
                table: {
                    "CONCOST.Brush.Primary": "Accent brush — primary actions, active states, focus rings.",
                    "CONCOST.Brush.Background": "Window/page background behind all content.",
                    "CONCOST.Brush.Surface": "Card, panel and dialog background.",
                    "CONCOST.Brush.SurfaceSecondary": "Secondary surface for toolbars, translucent inputs and badges.",
                    "CONCOST.Brush.Border": "Default 1px border and grid line colour.",
                    "CONCOST.Brush.TextPrimary": "Primary text colour, adapts to the theme.",
                    "CONCOST.Brush.TextSecondary": "Secondary/label text colour."
                }
            },
            {
                title: "2. Auto mode — follow the host window (Revit / AutoCAD plugins)",
                desc: "<code>AppTheme.Auto</code> reads the background colour of the window the control lives in and picks Light or Dark from its brightness. That is the mode for an add-in: a plugin has <b>no <code>Application.Current</code></b> of its own, only the host's window — and that window already decides whether the surrounding UI is light or dark. Detection walks up from the plugin root to the first element that actually paints a background (a plugin panel is usually transparent, the host's panel is not) and falls back to the Windows setting when it finds none.",
                xaml: `<!-- The add-in root: keep it transparent so detection reaches the host panel -->
<UserControl x:Name="PluginRoot" Background="Transparent">
    <!-- plugin content -->
</UserControl>`,
                csharp: `// Apply once, when the plugin view loads
ThemeManager.Current.ApplyTheme(PluginRoot, AppTheme.Auto);

// The host just switched its own theme (Revit/AutoCAD light ↔ dark)
ThemeManager.Current.RefreshAutoTheme(PluginRoot);

// Ask first if you need to know which way the host is
var hostTheme = ThemeManager.DetectTheme(PluginRoot);`,
                callout: {
                    type: "tip",
                    body: "Auto applies the theme to a <b>root element</b>, so it never touches the rest of the application: two panels in two different windows can be light and dark at the same time. <code>AppTheme.System</code> is the opposite — it follows the Windows light/dark setting."
                }
            }
        ]
    },

    "attached-props": {
        title: "Attached properties",
        desc: "CONCOST.UI extends native WPF controls (Button, TextBox, Border, DataGrid…) with attached properties, so you keep the standard control and still get icons, badges, validation metadata and drag-and-drop.",
        sections: [
            {
                title: "1. BorderElement (corner radius, circular buttons, hover brushes)",
                desc: "Applies to any <code>Border</code>-templated control.",
                table: {
                    "attached:BorderElement.CornerRadius": "Corner radius applied to the control frame.",
                    "attached:BorderElement.Circular": "Makes the control perfectly round (icon buttons).",
                    "attached:BorderElement.HoverBackground": "Background brush used while hovering.",
                    "attached:BorderElement.HoverBorderBrush": "Border brush used while hovering."
                }
            },
            {
                title: "2. InfoElement &amp; InputBase (input metadata, title, clear button)",
                desc: "Adds a label, placeholder, required marker, prefix/suffix and a clear button to any input control.",
                table: {
                    "attached:InfoElement.Placeholder": "Hint text shown while the input is empty.",
                    "attached:InfoElement.Title": "Label rendered above the input.",
                    "attached:InfoElement.TitlePlacement": "Label position: <code>Dock.Top</code> or <code>Dock.Left</code>.",
                    "attached:InfoElement.Required": "Marks the field as required (red asterisk).",
                    "attached:InfoElement.Prefix": "Content inserted before the input (icon, unit, currency).",
                    "attached:InfoElement.Suffix": "Content inserted after the input (unit, button).",
                    "attached:InfoElement.ShowClearButton": "Shows the quick-clear [X] button."
                }
            },
            {
                title: "3. IconElement (prefix/suffix vector icons and size)",
                desc: "Places a vector icon inside any content control.",
                table: {
                    "attached:IconElement.Kind": "Icon glyph from <code>IconKind</code>.",
                    "attached:IconElement.IconSize": "Icon size in pixels.",
                    "attached:IconElement.Placement": "Icon position: <code>Dock.Left</code> or <code>Dock.Right</code>."
                }
            },
            {
                title: "4. ButtonElement (loading spinner and notification badge)",
                desc: "Turns any <code>ButtonBase</code> into a loading button or attaches a count badge.",
                table: {
                    "attached:ButtonElement.IsLoading": "Shows a spinner and blocks repeated clicks.",
                    "attached:ButtonElement.LoadingText": "Text replacing <code>Content</code> while loading.",
                    "attached:ButtonElement.Badge": "Count/notification badge in the top-right corner."
                }
            },
            {
                title: "5. VisualElement (elevation, micro-interactions and state)",
                desc: "A shared appearance pack that works on any <code>UIElement</code> (Card, Button, Border, Image, Panel) with no custom template. Shadows are rebuilt from theme tokens, so they follow Light ↔ Dark and accent changes automatically.",
                table: {
                    "attached:VisualElement.Elevation": "Shadow level from 0 to 4, resolved from the theme <code>Shadow</code> / <code>ShadowStrong</code> tokens.",
                    "attached:VisualElement.HoverElevation": "Shadow level while hovering — usually <code>Elevation + 1</code> so the surface lifts.",
                    "attached:VisualElement.HoverScale": "Scale factor applied on hover (1.0 = no effect).",
                    "attached:VisualElement.PressScale": "Scale factor applied while pressed (e.g. <code>0.97</code>) for tactile feedback.",
                    "attached:VisualElement.ScaleOrigin": "Origin of the scale transform, 0–1 (0.5 = centre, 0 = top-left).",
                    "attached:VisualElement.MotionDuration": "Animation duration in milliseconds; set 0 to disable motion.",
                    "attached:VisualElement.ShadowColor": "Overrides the shadow colour (theme token by default).",
                    "attached:VisualElement.Glow": "Accent-coloured glow while hovering.",
                    "attached:VisualElement.FocusRing": "Draws a 2px accent focus ring — restores the previous visual when disabled.",
                    "attached:VisualElement.DisabledOpacity": "Opacity applied while <code>IsEnabled</code> is false, restored when re-enabled."
                }
            },
            {
                title: "6. SurfaceElement (themed surface skin in one line)",
                desc: "Instead of hand-picking background, border brush and corner radius per control, pick a surface role — colours and borders follow theme tokens and switch with Light/Dark.",
                table: {
                    "attached:SurfaceElement.Role": "Surface role: <code>Base</code>, <code>Secondary</code>, <code>Elevated</code>, <code>Sunken</code>, <code>Accent</code>, <code>AccentSoft</code>, <code>Danger</code>.",
                    "attached:SurfaceElement.Border": "Border style: <code>Soft</code>, <code>Strong</code>, <code>Accent</code> (<code>None</code> clears it).",
                    "attached:SurfaceElement.Radius": "Corner radius override (e.g. <code>Radius=\"10\"</code> or <code>\"12,12,0,0\"</code>)."
                }
            },
            {
                title: "7. ScrollViewerHelper (inertia scrolling and auto-hiding scrollbars)",
                desc: "Adds touch-like momentum and a back-to-top affordance to any scroll viewer.",
                table: {
                    "attached:ScrollViewerHelper.IsInertiaEnabled": "Enables momentum scrolling."
                }
            },
            {
                title: "8. PasswordBoxHelper (two-way MVVM binding for passwords)",
                desc: "<code>PasswordBox.Password</code> is not bindable; this helper makes it bindable without breaking security expectations.",
                table: {
                    "attached:PasswordBoxHelper.Attach": "Enables the helper on the <code>PasswordBox</code>.",
                    "attached:PasswordBoxHelper.Password": "Bindable password value.",
                    "attached:PasswordBoxHelper.PasswordLength": "Length of the current password (read-only)."
                }
            },
            {
                title: "9. ProgressBarElement &amp; DataGridElement",
                desc: "Display refinements for progress bars and data grids.",
                table: {
                    "attached:ProgressBarElement.ShowText": "Shows the percentage on the bar.",
                    "attached:ProgressBarElement.TextPattern": "Format string for the percentage label.",
                    "attached:DataGridElement.RowSpacing": "Spacing between rows.",
                    "attached:DataGridElement.ShowRowDivider": "Draws a divider between rows."
                }
            },
            {
                title: "10. DragDropHelper (reordering and list transfer, gong-wpf-dragdrop style)",
                desc: "Enables drag-and-drop between lists or inside a single list, grouped so unrelated lists do not accept each other's items.",
                table: {
                    "attached:DragDropHelper.IsDragSource": "Allows dragging this element.",
                    "attached:DragDropHelper.IsDropTarget": "Allows dropping onto this element.",
                    "attached:DragDropHelper.DragDropGroup": "Group name — only elements in the same group accept drops."
                }
            }
        ]
    },

    faq: {
        title: "FAQ &amp; Troubleshooting",
        desc: "The integration problems people actually hit when adding CONCOST.UI to a WPF application or a CAD/BIM plugin, with the root cause and the fix.",
        sections: [
            {
                title: "Styles are not applied / controls render blank",
                desc: "Almost always a missing dictionary or the wrong resource reference.",
                table: [
                    { token: "Missing Themes/ThemeResources.xaml", desc: "Without <code>ThemeResources.xaml</code> the keys <code>CONCOST.Spacing.*</code>, <code>CONCOST.FontSize.*</code> and <code>CONCOST.CornerRadius.*</code> do not exist, so styles fail to resolve." },
                    { token: "Missing Themes/Generic.xaml", desc: "Controls still render, but with the default WPF look: no colour, no rounded corners, no animation." },
                    { token: "StaticResource for a theme-aware style", desc: "When the theme changes at runtime the brush is already frozen into the control. Use <code>DynamicResource</code> for every brush and style that must follow the theme." },
                    { token: "Merging after the window has loaded", desc: "Merging in <code>Loaded</code> means the template was already created. Merge in <code>App.xaml</code> or before <code>InitializeComponent()</code>." }
                ],
                callout: {
                    body: "Quick check: watch the Visual Studio Output window for <code>System.Windows.ResourceReferenceKeyNotFoundException</code>. The missing key is printed in the message."
                }
            },
            {
                title: "Revit / AutoCAD plugin: the UI inherits the host theme",
                desc: "A plugin DLL has no <code>Application.Current</code> of its own. Writing to <code>Application.Current.Resources</code> overrides the styles of Revit/AutoCAD itself.",
                callout: {
                    title: "Do not use App.xaml in a class library",
                    body: "A plugin has no entry point. If you need a single initialisation point, create <code>static class ThemeBootstrap.Init(FrameworkElement root)</code> and call it from every view constructor."
                }
            },
            {
                title: "Build and packaging errors per target framework",
                desc: "The three targets have different dependencies — the usual cause of runtime errors that only appear on a customer machine.",
                table: [
                    { token: "net48 + System.Text.Json", desc: "The <code>net48</code> build uses <code>System.Text.Json</code>; when copying DLLs by hand you must also ship its dependencies from the build output." },
                    { token: "Revit 2021–2024", desc: "Runs on .NET Framework 4.8 → reference <code>net48</code>. Revit 2025+ runs on .NET 8 → use <code>net8.0-windows</code>." },
                    { token: "AutoCAD 2025+", desc: "Runs on .NET 8. Always confirm the runtime of the exact host release before choosing the assembly." },
                    { token: "dotnet build: missing targeting pack", desc: "Install the .NET Framework 4.8 Developer Pack, or build only <code>-f net8.0-windows</code> when <code>net48</code> is not needed." }
                ],
                callout: {
                    body: "Use <code>dotnet build CONCOST.UI.sln -c Release</code> to build all three targets; a <code>ProjectReference</code> then picks the one that matches the host."
                }
            },
            {
                title: "Performance with large data sets",
                desc: "The data controls virtualise by default, but a few XAML patterns silently disable it.",
                table: [
                    { token: "ScrollViewer wrapping a DataGrid", desc: "A ScrollViewer gives the grid infinite height, disabling virtualisation — 10,000 rows will create 10,000 containers. Remove the ScrollViewer and let the grid scroll." },
                    { token: "StackPanel instead of a virtualising panel", desc: "Keep the default <code>VirtualizingStackPanel</code> for <code>ListBox</code>/<code>ListView</code>; only switch to WrapPanel for short lists." },
                    { token: "Too many active spreadsheet sheets", desc: "Keep a single <code>ActiveSheet</code>; each sheet holds its own grid in memory." },
                    { token: "Heavy effects inside item templates", desc: "<code>Ripple</code>, <code>Skeleton</code> and shadows (<code>VisualElement.Elevation</code>) multiply per item — disable ripple inside long item templates." }
                ]
            }
        ]
    },

    migration: {
        title: "Migrating from WinForms / plain WPF",
        desc: "Mapping table from WinForms and stock WPF to CONCOST.UI, a four-step migration path and the deprecation policy.",
        sections: [
            {
                title: "Control mapping",
                desc: "The right column is a real style key or control from the library, not a suggestion.",
                table: [
                    { token: "<code>Button</code> → <code>CONCOST.Button.Primary</code>", desc: "23 style variants: Primary, Secondary, Success, Warning, Danger, Info, Outline.*, Icon.*, *.Light." },
                    { token: "<code>TextBox</code> (numeric) → <code>ui:NumericUpDown</code>", desc: "Supports Minimum/Maximum/Increment/DecimalPlaces/Suffix, arrow keys and mouse wheel." },
                    { token: "<code>TextBox</code> (unit) → <code>ui:UnitBox</code>", desc: "Renders the unit and a consistent number of decimals across the application." },
                    { token: "<code>TextBox</code> (password) → <code>ui:PasswordInput</code>", desc: "Show/hide toggle, lock icon, placeholder." },
                    { token: "<code>DataGridView</code> → <code>DataGrid</code> + <code>CONCOST.DataGrid.Base</code>", desc: "Zebra rows, theme-aware, plus the <code>DataGridElement.RowSpacing</code> attached property." },
                    { token: "Hand-drawn stacked header → <code>ui:StackedHeaderDataGrid</code>", desc: "Nest <code>StackedColumn</code> elements; the footer aggregate is computed automatically." },
                    { token: "CAD drawing panel → <code>canvas:ZoomableCanvas</code> + <code>canvas:RulerControl</code>", desc: "Infinite zoom/pan, millimetre grid, synchronised rulers." },
                    { token: "MessageBox → <code>ui:Dialog</code> / <code>Growl</code>", desc: "Dialog with mask and close-by-token; Growl for non-blocking toasts." },
                    { token: "Error tooltip → <code>attached:InfoElement.*</code>", desc: "Title, Placeholder, Required, Prefix/Suffix, ShowClearButton." }
                ]
            },
            {
                title: "Four-step migration path",
                desc: "This order keeps the application buildable throughout the migration.",
                callout: {
                    body: "<b>1.</b> Add the reference and merge the dictionaries. <b>2.</b> Replace controls following the mapping table. <b>3.</b> Enable ThemeManager. <b>4.</b> Replace MessageBox with Dialog or Growl."
                }
            },
            {
                title: "Breaking-change policy",
                desc: "Semantic versioning applies to the whole assembly.",
                table: [
                    { token: "Deprecation", desc: "Replaced APIs are marked <code>[Obsolete]</code> for at least two minor versions before removal." },
                    { token: "Major release", desc: "Breaking changes only ship in a major version and come with migration examples in <code>CHANGELOG.md</code>." },
                    { token: "Theme keys", desc: "Resource keys (<code>CONCOST.*</code>) are public API — renaming a key is a breaking change." }
                ]
            }
        ]
    }
});
