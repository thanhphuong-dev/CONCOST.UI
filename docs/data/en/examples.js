// English overlay — cookbook page.
Object.assign(DOCS_EN.pages, {

    examples: {
        title: "Usage examples",
        desc: "Four real-world scenarios that combine several controls: a beam parameter form, a quantity take-off screen, a settings panel inside a Revit plugin, and a KPI dashboard with a loading state. Every example ships runnable XAML and C#.",
        sections: [
            {
                title: "1. Beam parameter form",
                desc: "Combines <code>UnitBox</code>, <code>NumericUpDown</code>, <code>RebarInput</code>, <code>CheckBox</code> and <code>SplitButton</code> into one compact form with units and built-in validation.",
                callout: {
                    body: "<code>UnitBox</code> and <code>RebarInput</code> have no <code>Header</code> property — use a <code>TextBlock</code> with the <code>CONCOST.TextBlock.Caption</code> style as the label, or <code>attached:InfoElement.Title</code> if you write your own template."
                }
            },
            {
                title: "2. Quantity take-off screen with search and paging",
                desc: "The standard take-off layout: toolbar on top, pager at the bottom, multi-level header grid in the middle with an automatic aggregate footer."
            },
            {
                title: "3. Settings panel inside a Revit / AutoCAD plugin",
                desc: "A UserControl has no <code>App.xaml</code>: load the resources into itself, apply the theme locally, and use <code>Expander</code> + <code>PropertyGrid</code> to stay compact in a narrow panel.",
                callout: {
                    title: "Never write to Application.Current.Resources",
                    body: "That is Revit/AutoCAD's own resource dictionary. Assign to <code>UserControl.Resources</code> (or call <code>ThemeManager.Current.ApplyTheme(this, …)</code>) so you do not break the host UI."
                }
            },
            {
                title: "4. KPI dashboard with a loading state",
                desc: "Combines <code>StatisticCard</code>, <code>ChartLine</code> and <code>Skeleton</code>: placeholders while loading, real data once it arrives."
            }
        ]
    }
});
