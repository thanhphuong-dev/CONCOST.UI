using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using CONCOST.UI.Services;
using CONCOST.UI.Themes;
using Xunit;

namespace CONCOST.UI.Tests;

/// <summary>
/// Temporary: audits every showcase page for the defects that are measurable rather than a matter of
/// taste — horizontal overflow, empty cards, low-contrast body text, unnamed icon-only controls.
/// </summary>
public class ShowcasePageAuditTests
{
    private const int PageWidth = 1080;

    private sealed class Row
    {
        public string Name = "";
        public string Group = "";
        public double Width;
        public double Height;
        public int Cards;
        public int Overflowing;
        public string Widest = "";
        public int TertiaryText;
        public int UnnamedIconButtons;
        public int Instances;
        public string? Error;
    }

    private static IEnumerable<DependencyObject> Walk(DependencyObject root)
    {
        yield return root;
        int count = VisualTreeHelper.GetChildrenCount(root);
        for (int i = 0; i < count; i++)
            foreach (var child in Walk(VisualTreeHelper.GetChild(root, i))) yield return child;
    }

    private static bool InsideHorizontalScroller(DependencyObject element)
    {
        for (var current = VisualTreeHelper.GetParent(element); current is not null; current = VisualTreeHelper.GetParent(current))
        {
            if (current is ScrollViewer scroller &&
                scroller.HorizontalScrollBarVisibility != ScrollBarVisibility.Disabled)
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsTertiary(TextBlock text, Color tertiary) =>
        text.Foreground is SolidColorBrush brush && brush.Color == tertiary && text.FontSize >= 10.5;

    private static bool IsUnnamedIconOnly(FrameworkElement element)
    {
        if (element is not ButtonBase) return false;
        if (string.IsNullOrWhiteSpace(element.ToolTip as string)) return false;
        if (!string.IsNullOrWhiteSpace(AutomationProperties.GetName(element))) return false;

        // A button whose content carries any text already has a name: UI Automation derives it from
        // the content tree. Only a control that is nothing but a glyph is actually unnamed.
        if (element is ContentControl { Content: string caption }) return string.IsNullOrWhiteSpace(caption);

        if (element is ContentControl { Content: DependencyObject content } &&
            Walk(content).OfType<TextBlock>().Any(t => !string.IsNullOrWhiteSpace(t.Text)))
        {
            return false;
        }

        return true;
    }

    [Fact]
    public void Audit_every_page()
    {
        WpfApplicationHost.Run(() =>
        {
            var rows = new List<Row>();

            // App.xaml merges both dictionaries at startup; without them a page that calls
            // SetResourceReference in its constructor throws before it is ever hosted.
            var app = Application.Current;
            var merged = new List<ResourceDictionary>();

            if (app is not null)
            {
                foreach (var uri in new[]
                {
                    "pack://application:,,,/CONCOST.UI;component/Themes/ThemeResources.xaml",
                    "pack://application:,,,/CONCOST.UI;component/Themes/Generic.xaml"
                })
                {
                    var dictionary = new ResourceDictionary { Source = new Uri(uri, UriKind.Absolute) };
                    merged.Add(dictionary);
                    app.Resources.MergedDictionaries.Add(dictionary);
                }
            }

            var vm = new Showcase.ShowcaseViewModel();
            var tertiary = ((SolidColorBrush)Application.Current.TryFindResource("CONCOST.Brush.TextTertiary")
                            ?? new SolidColorBrush(Colors.Gray)).Color;

            foreach (var item in vm.AllItems)
            {
                var row = new Row { Name = item.Name, Group = item.Category };

                try
                {
                    var page = (FrameworkElement)item.Content;
                    var host = new Border { Child = page, Width = PageWidth };
                    ThemeManager.Current.ApplyTheme(host, AppTheme.Light);

                    host.Measure(new Size(PageWidth, double.PositiveInfinity));
                    row.Width = page.DesiredSize.Width;
                    row.Height = page.DesiredSize.Height;

                    host.Arrange(new Rect(0, 0, PageWidth, Math.Max(1, row.Height)));
                    host.UpdateLayout();

                    row.Cards = Walk(page).OfType<Border>().Count(b => b.Name.Length == 0 && b.CornerRadius.TopLeft >= 10);
                    row.TertiaryText = Walk(page).OfType<TextBlock>().Count(t => IsTertiary(t, tertiary));
                    row.UnnamedIconButtons = Walk(page).OfType<FrameworkElement>().Count(IsUnnamedIconOnly);

                    // How many copies of the page's own control are on it: a page is meant to be one
                    // example plus the switches that change it, not a gallery.
                    row.Instances = Walk(page).Count(e => e.GetType().Name == row.Name);

                    // A child wider than the page is clipped by the card it lives in — unless it
                    // sits in something that is allowed to scroll sideways, which is by design.
                    // 16 px of slack: a grid pulled out by a negative margin so its children's own
                    // gutters land flush is a deliberate trick, not an overflow.
                    var wide = Walk(page).OfType<FrameworkElement>()
                        .Where(e => e.ActualWidth > PageWidth + 16 && !InsideHorizontalScroller(e))
                        .ToList();
                    row.Overflowing = wide.Count;
                    if (wide.Count > 0)
                        row.Widest = string.Join(", ", wide.Take(3).Select(e => $"{e.GetType().Name}:{e.ActualWidth:0}"));
                }
                catch (Exception ex)
                {
                    row.Error = ex.GetType().Name + ": " + ex.Message;
                }

                rows.Add(row);
                host_cleanup(item);
            }

            var sb = new StringBuilder();
            sb.AppendLine("# Showcase audit");
            sb.AppendLine();
            sb.AppendLine($"Pages: {rows.Count} | failed: {rows.Count(r => r.Error is not null)} | " +
                          $"overflowing: {rows.Count(r => r.Overflowing > 0)} | " +
                          $"with tertiary body text: {rows.Count(r => r.TertiaryText > 0)} | " +
                          $"with unnamed icon buttons: {rows.Count(r => r.UnnamedIconButtons > 0)}");
            sb.AppendLine();

            sb.AppendLine("## Failed to build");
            foreach (var r in rows.Where(r => r.Error is not null)) sb.AppendLine($"- **{r.Name}** — {r.Error}");
            sb.AppendLine();

            sb.AppendLine("## Horizontal overflow");
            sb.AppendLine("| page | desired | widest child |");
            sb.AppendLine("|---|---|---|");
            foreach (var r in rows.Where(r => r.Overflowing > 0).OrderByDescending(r => r.Width))
                sb.AppendLine($"| {r.Name} | {r.Width:0} | {r.Widest} |");
            sb.AppendLine();

            sb.AppendLine("## Tertiary body text (contrast)");
            sb.AppendLine("| page | count |");
            sb.AppendLine("|---|---|");
            foreach (var r in rows.Where(r => r.TertiaryText > 0).OrderByDescending(r => r.TertiaryText))
                sb.AppendLine($"| {r.Name} | {r.TertiaryText} |");
            sb.AppendLine();

            sb.AppendLine("## Unnamed icon-only controls");
            sb.AppendLine("| page | count |");
            sb.AppendLine("|---|---|");
            foreach (var r in rows.Where(r => r.UnnamedIconButtons > 0).OrderByDescending(r => r.UnnamedIconButtons))
                sb.AppendLine($"| {r.Name} | {r.UnnamedIconButtons} |");
            sb.AppendLine();

            sb.AppendLine("## Pages showing the control more than once");
            sb.AppendLine("| page | copies |");
            sb.AppendLine("|---|---|");
            foreach (var r in rows.Where(r => r.Instances > 1).OrderByDescending(r => r.Instances))
                sb.AppendLine($"| {r.Name} | {r.Instances} |");
            sb.AppendLine();

            sb.AppendLine("## Tallest pages");
            foreach (var r in rows.OrderByDescending(r => r.Height).Take(15))
                sb.AppendLine($"- {r.Name}: {r.Height:0} px");

            var failed = rows.Count(r => r.Error is not null);
            var overflowing = rows.Count(r => r.Overflowing > 0);

            if (failed > 0 || overflowing > 0)
                File.WriteAllText(Path.Combine(AppContext.BaseDirectory, "showcase-audit.md"), sb.ToString());

            // Two hard guarantees: every page builds, and nothing is wider than the page it lives in.
            // The rest of the report (contrast, accessible names) is advisory and printed for a human.
            Assert.Equal(0, failed);
            Assert.Equal(0, overflowing);

            // Leave the application exactly as it was found: this test builds 145 pages and merges
            // two dictionaries to do it, and a leftover dictionary changes how every later test
            // resolves a resource.
            foreach (var dictionary in merged) app?.Resources.MergedDictionaries.Remove(dictionary);
        });
    }

    private static void host_cleanup(Showcase.ShowcaseItem item)
    {
        // Nothing to dispose: the page is cached by the view model and reused by the app.
    }
}
