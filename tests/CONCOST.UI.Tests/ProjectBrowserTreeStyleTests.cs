using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Media;
using CONCOST.UI.AttachedProperties;
using CONCOST.UI.Controls;
using Xunit;

namespace CONCOST.UI.Tests;

/// <summary>
/// The classic row treatment: a boxed +/−, one dense row, and a selection painted as a band across
/// the full width instead of a rounded pill hugging the caption.
/// </summary>
public class ProjectBrowserTreeStyleTests
{
    private static void RunOnSta(Action action) => WpfApplicationHost.Run(action);

    private static IEnumerable<DependencyObject> Walk(DependencyObject root)
    {
        yield return root;

        int count = VisualTreeHelper.GetChildrenCount(root);
        for (int i = 0; i < count; i++)
        {
            foreach (var child in Walk(VisualTreeHelper.GetChild(root, i))) yield return child;
        }
    }

    private sealed class Host : IDisposable
    {
        private readonly Window _window;

        internal Host(ProjectBrowser browser)
        {
            Browser = browser;
            _window = new Window
            {
                Width = 320,
                Height = 420,
                Content = browser,
                ShowInTaskbar = false,
                WindowStyle = WindowStyle.None
            };

            // The row styles are picked up by key at runtime, so the dictionary has to be in the
            // element's resource chain — the real app merges it at Application level.
            _window.Resources.MergedDictionaries.Add(new ResourceDictionary
            {
                Source = new Uri("pack://application:,,,/CONCOST.UI;component/Themes/ThemeResources.xaml", UriKind.Absolute)
            });

            // Keyed styles live in Generic.xaml, which a real app merges alongside the tokens.
            _window.Resources.MergedDictionaries.Add(new ResourceDictionary
            {
                Source = new Uri("pack://application:,,,/CONCOST.UI;component/Themes/Generic.xaml", UriKind.Absolute)
            });

            _window.Show();
            _window.UpdateLayout();
        }

        internal ProjectBrowser Browser { get; }

        internal TreeView Tree() => Walk(Browser).OfType<TreeView>().First();

        internal IEnumerable<T> Find<T>() where T : FrameworkElement => Walk(Browser).OfType<T>();

        public void Dispose()
        {
            _window.Content = null;
            _window.Close();
        }
    }

    private static ProjectBrowser Build(ProjectBrowserTreeStyle style)
    {
        var browser = new ProjectBrowser { TreeStyle = style, Height = 400 };

        var project = browser.AddRoot("Tower", ProjectNodeKind.Project);
        var views = project.Add("Views", ProjectNodeKind.Folder);
        views.Add("Level 1", ProjectNodeKind.View);
        views.Add("Level 2", ProjectNodeKind.View);

        browser.Refresh();
        browser.ExpandAll();

        return browser;
    }

    [Fact]
    public void Classic_hides_the_chrome_a_revit_palette_does_not_have()
    {
        RunOnSta(() =>
        {
            var browser = Build(ProjectBrowserTreeStyle.Classic);
            using var host = new Host(browser);
            host.Browser.UpdateLayout();

            // Search stays: a project browser is a search box over a list.
            var search = host.Find<SearchBox>().First();
            Assert.Equal(Visibility.Visible, search.Visibility);

            // Everything else is this library's own palette chrome.
            var combos = host.Find<ComboBox>().ToList();
            Assert.NotEmpty(combos);
            Assert.All(combos, combo => Assert.Equal(Visibility.Collapsed, combo.Visibility));
        });
    }

    [Fact]
    public void Modern_keeps_the_chrome()
    {
        RunOnSta(() =>
        {
            var browser = Build(ProjectBrowserTreeStyle.Modern);
            using var host = new Host(browser);
            host.Browser.UpdateLayout();

            var combos = host.Find<ComboBox>().ToList();
            Assert.NotEmpty(combos);
            Assert.Contains(combos, combo => combo.Visibility == Visibility.Visible);
        });
    }

    [Fact]
    public void Classic_selection_is_a_full_width_accent_band()
    {
        RunOnSta(() =>
        {
            var browser = Build(ProjectBrowserTreeStyle.Classic);
            using var host = new Host(browser);
            host.Browser.UpdateLayout();

            var item = host.Find<TreeViewItem>().First(container => container.Tag is ProjectTreeNode { Name: "Views" });
            browser.Select((ProjectTreeNode)item.Tag);
            host.Browser.UpdateLayout();

            var row = Walk(item).OfType<Border>().First(border => border.Name == "Bd");
            Assert.True(item.IsSelected);

            // PrimarySoft/PrimaryLight are the tokens ThemeManager regenerates when the accent
            // changes; PrimarySoftLight/PrimarySoftBorder are static orange and would not follow it.
            if (browser.TryFindResource("CONCOST.Brush.PrimarySoft") is SolidColorBrush band &&
                row.Background is SolidColorBrush actual)
            {
                Assert.Equal(band.Color, actual.Color);
            }

            // The band spans the tree, not a rounded box around the caption: a short row has to be
            // as wide as the palette, which is what the disabled horizontal scroll buys.
            // The shortfall is the vertical scrollbar, not the caption: 288 of 302 in a 320 px host.
            var tree = host.Tree();
            Assert.True(row.ActualWidth > tree.ActualWidth - 20,
                $"band was {row.ActualWidth} of a {tree.ActualWidth} tree");
            Assert.Equal(new CornerRadius(0), row.CornerRadius);
        });
    }

    [Fact]
    public void Icon_only_tools_and_node_captions_carry_their_text()
    {
        RunOnSta(() =>
        {
            var browser = Build(ProjectBrowserTreeStyle.Modern);
            using var host = new Host(browser);
            host.Browser.UpdateLayout();

            // An icon-only button has no text of its own: without an accessible name a screen reader
            // announces "button" three times in a row.
            // The browser's own tools are the icon-only ones; a combo box's internal button is not.
            var tools = host.Find<Button>()
                .Where(b => b.ToolTip is string && IconElement.GetKind(b) != CONCOST.UI.Icons.IconKind.None)
                .ToList();
            Assert.NotEmpty(tools);
            Assert.All(tools, button => Assert.False(string.IsNullOrWhiteSpace(AutomationProperties.GetName(button))));

            // A palette is narrow and view names are long: the caption has to ellipsize and keep the
            // full text reachable, instead of being cut mid-word with no way to read it.
            var caption = host.Find<TextBlock>().First(t => t.Text == "Level 1");
            Assert.Equal(TextTrimming.CharacterEllipsis, caption.TextTrimming);
            Assert.Equal("Level 1", caption.ToolTip);
        });
    }

    [Fact]
    public void A_palette_turns_the_fading_scrollbar_off()
    {
        RunOnSta(() =>
        {
            var browser = new ProjectBrowser { TreeStyle = ProjectBrowserTreeStyle.Classic, Height = 400 };

            var project = browser.AddRoot("Tower", ProjectNodeKind.Project);
            var views = project.Add("Views", ProjectNodeKind.Folder);
            for (int i = 0; i < 40; i++) views.Add("View " + i, ProjectNodeKind.View);

            browser.Refresh();
            browser.ExpandAll();

            using var host = new Host(browser);
            host.Browser.UpdateLayout();

            // The library fades scrollbars out until the pointer arrives. A palette must not: with
            // the bar invisible, a list that continues past the bottom edge reads as a cut-off list.
            // The ScrollViewer style pins the property to True, so the control has to clear it on the
            // scroller itself — a style setter beats inheritance from the tree.
            var scroller = host.Find<ScrollViewer>().First(sv => sv.Name == "PART_ScrollViewer" || sv.TemplatedParent is TreeView);
            Assert.False(ScrollViewerHelper.GetIsAutoHideEnabled(scroller));
        });
    }

    [Fact]
    public void Switching_style_back_restores_the_modern_rows()
    {
        RunOnSta(() =>
        {
            var browser = Build(ProjectBrowserTreeStyle.Classic);
            using var host = new Host(browser);
            host.Browser.UpdateLayout();

            var classicPadding = host.Find<TreeViewItem>().First().Padding;
            Assert.Equal(new Thickness(5, 3, 5, 3), classicPadding);

            browser.TreeStyle = ProjectBrowserTreeStyle.Modern;
            host.Browser.UpdateLayout();

            var modernPadding = host.Find<TreeViewItem>().First().Padding;
            Assert.Equal(new Thickness(4, 4, 4, 4), modernPadding);
        });
    }
}
