using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CONCOST.UI.AttachedProperties;
using CONCOST.UI.Controls;
using Xunit;

namespace CONCOST.UI.Tests;

/// <summary>
/// The dress of the project browser: how many outlines it draws, whether the pickers stand at the
/// height the browser asked for, and whether every caption and group switch reaches its part.
/// </summary>
/// <remarks>
/// The browser used to draw four 26 px chips and two 34 px dropdowns inside whatever frame it was
/// dropped in, and the Showcase put a second search field and a second sort picker around it. So the
/// load-bearing test paints the browser and counts the ink along its top edge — one outline for a
/// card, none for a seamless palette — and the rest guard the height, caption and group plumbing.
/// </remarks>
public class ProjectBrowserChromeTests
{
    private const int Width = 300;
    private const int Height = 380;

    private static void RunOnSTA(Action action) => WpfApplicationHost.Run(action);

    /// <summary>
    /// The tokens and the styles the browser reads. Without them every <c>SetResourceReference</c>
    /// comes back empty and the tools fall back to the stock WPF button — itself an outlined box, so
    /// the outline test would pass for the wrong reason.
    /// </summary>
    private static ResourceDictionary Theme()
    {
        var theme = new ResourceDictionary();

        theme.MergedDictionaries.Add(Dictionary("pack://application:,,,/CONCOST.UI;component/Themes/ThemeResources.xaml"));
        theme.MergedDictionaries.Add(Dictionary("pack://application:,,,/CONCOST.UI;component/Themes/Generic.xaml"));

        return theme;

        static ResourceDictionary Dictionary(string path) =>
            new() { Source = new Uri(path, UriKind.Absolute) };
    }

    /// <summary>
    /// A browser laid out inside a white host. <paramref name="configure"/> runs before the first
    /// layout pass, because that is when the resource references resolve and the plate is measured.
    /// </summary>
    private static ProjectBrowser Hosted(Action<ProjectBrowser>? configure = null, bool withTree = true)
    {
        var browser = new ProjectBrowser();

        configure?.Invoke(browser);

        if (withTree)
        {
            var project = browser.AddRoot("Building B", ProjectNodeKind.Project);
            var views = project.Add("Views", ProjectNodeKind.Folder);

            views.Add("Level 1", ProjectNodeKind.View);
            views.Add("Level 2", ProjectNodeKind.View);

            project.Add("Sheets", ProjectNodeKind.Folder).Add("A101", ProjectNodeKind.Sheet);

            browser.Refresh();
            browser.ExpandAll();
        }

        var host = new Border { Child = browser, Background = Brushes.White, Width = Width, Height = Height };

        host.Resources.MergedDictionaries.Add(Theme());

        host.Measure(new Size(Width, Height));
        host.Arrange(new Rect(0, 0, Width, Height));
        host.UpdateLayout();

        return browser;
    }

    /// <summary>The one surface the browser owns.</summary>
    private static Border Frame(ProjectBrowser browser) => (Border)browser.Children[0];

    private static Grid Shell(ProjectBrowser browser) => (Grid)Frame(browser).Child;

    private static T Child<T>(ProjectBrowser browser, int row) where T : UIElement =>
        Shell(browser).Children.OfType<T>().First(element => Grid.GetRow(element) == row);

    private static StackPanel Header(ProjectBrowser browser) => Child<StackPanel>(browser, 0);

    private static Border HeaderRule(ProjectBrowser browser) => Child<Border>(browser, 1);

    private static Border Footer(ProjectBrowser browser) => Child<Border>(browser, 3);

    private static TextBlock Counter(ProjectBrowser browser) => (TextBlock)Footer(browser).Child;

    private static Grid FilterRow(ProjectBrowser browser) => Header(browser).Children.OfType<Grid>().First();

    private static SearchBox Search(ProjectBrowser browser) => FilterRow(browser).Children.OfType<SearchBox>().First();

    private static ComboBox KindFilter(ProjectBrowser browser) => FilterRow(browser).Children.OfType<ComboBox>().First();

    private static Grid ToolRow(ProjectBrowser browser) => Header(browser).Children.OfType<Grid>().Skip(1).First();

    private static StackPanel Tools(ProjectBrowser browser) => ToolRow(browser).Children.OfType<StackPanel>().First();

    private static ComboBox Sort(ProjectBrowser browser) => ToolRow(browser).Children.OfType<ComboBox>().First();

    private static Button[] ToolButtons(ProjectBrowser browser) => Tools(browser).Children.OfType<Button>().ToArray();

    private static ToggleButton GroupSwitch(ProjectBrowser browser) => Tools(browser).Children.OfType<ToggleButton>().First();

    private static Grid TreeHost(ProjectBrowser browser) => Child<Grid>(browser, 2);

    private static TreeView Tree(ProjectBrowser browser) => TreeHost(browser).Children.OfType<TreeView>().First();

    private static TextBlock EmptyState(ProjectBrowser browser) => TreeHost(browser).Children.OfType<TextBlock>().First();

    private static FrameworkElement FirstNodeIcon(ProjectBrowser browser)
    {
        var item = (TreeViewItem)Tree(browser).Items[0];
        var header = (StackPanel)item.Header;

        return (FrameworkElement)header.Children[0];
    }

    private static byte[] Render(ProjectBrowser browser)
    {
        var host = (FrameworkElement)VisualTreeHelper.GetParent(browser);
        var bitmap = new RenderTargetBitmap(Width, Height, 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(host);

        var pixels = new byte[Width * Height * 4];
        bitmap.CopyPixels(pixels, Width * 4, 0);

        return pixels;
    }

    /// <summary>Pixels along the first row of the host that are not the white backdrop.</summary>
    private static int InkAlongTopEdge(ProjectBrowser browser)
    {
        byte[] pixels = Render(browser);
        int ink = 0;

        for (int x = 0; x < Width; x++)
        {
            int offset = x * 4;

            if (pixels[offset] < 244 || pixels[offset + 1] < 244 || pixels[offset + 2] < 244) ink++;
        }

        return ink;
    }

    [Fact]
    public void Seamless_draws_no_plate_at_all()
    {
        RunOnSTA(() =>
        {
            var browser = Hosted();
            Border frame = Frame(browser);

            Assert.Equal(ProjectBrowserVariant.Seamless, browser.Variant);
            Assert.Equal(new Thickness(0), frame.BorderThickness);
            Assert.Equal(Brushes.Transparent, frame.Background);
            Assert.Equal(0, InkAlongTopEdge(browser));
        });
    }

    [Fact]
    public void Card_draws_one_outline_and_it_is_the_only_ink_on_the_top_edge()
    {
        RunOnSTA(() =>
        {
            var browser = Hosted(b => b.Variant = ProjectBrowserVariant.Card);
            Border frame = Frame(browser);

            Assert.Equal(new Thickness(1), frame.BorderThickness);
            Assert.NotNull(frame.Background);
            Assert.True(InkAlongTopEdge(browser) > Width / 2, "the plate should ink the length of its top edge");
        });
    }

    [Fact]
    public void Floating_keeps_its_shadow_and_a_seamless_browser_ignores_the_elevation()
    {
        RunOnSTA(() =>
        {
            var floating = Hosted(b =>
            {
                b.Variant = ProjectBrowserVariant.Floating;
                b.Elevation = 3;
            });

            Assert.Equal(3, VisualElement.GetElevation(Frame(floating)));

            var seamless = Hosted(b =>
            {
                b.Variant = ProjectBrowserVariant.Seamless;
                b.Elevation = 3;
            });

            Assert.Equal(0, VisualElement.GetElevation(Frame(seamless)));
        });
    }

    [Fact]
    public void Switching_the_variant_back_restores_the_plate()
    {
        RunOnSTA(() =>
        {
            var browser = Hosted(b => b.Variant = ProjectBrowserVariant.Card);

            browser.Variant = ProjectBrowserVariant.Seamless;
            Assert.Equal(new Thickness(0), Frame(browser).BorderThickness);

            browser.Variant = ProjectBrowserVariant.Card;
            Assert.Equal(new Thickness(1), Frame(browser).BorderThickness);
        });
    }

    [Fact]
    public void Elevation_and_corner_radius_reach_the_frame()
    {
        RunOnSTA(() =>
        {
            var browser = Hosted(b =>
            {
                b.Variant = ProjectBrowserVariant.Card;
                b.CornerRadius = new CornerRadius(12);
                b.Elevation = 2;
            });

            Assert.Equal(new CornerRadius(12), Frame(browser).CornerRadius);
            Assert.Equal(2, VisualElement.GetElevation(Frame(browser)));
        });
    }

    [Fact]
    public void The_default_control_height_is_24_and_nothing_falls_back_to_34()
    {
        RunOnSTA(() =>
        {
            var browser = Hosted();

            Assert.Equal(24, browser.ControlHeight);
            Assert.Equal(24, Search(browser).ActualHeight);
            Assert.Equal(24, KindFilter(browser).ActualHeight);
            Assert.Equal(24, Sort(browser).ActualHeight);

            // The combo style's own floor is 34; a minimum beats an explicit Height unless it goes.
            Assert.Equal(0, KindFilter(browser).MinHeight);
            Assert.Equal(0, Sort(browser).MinHeight);
        });
    }

    [Fact]
    public void A_control_height_reaches_every_part_of_the_browser()
    {
        RunOnSTA(() =>
        {
            var browser = Hosted(b => b.ControlHeight = 30);

            Assert.Equal(30, KindFilter(browser).ActualHeight);
            Assert.Equal(30, Sort(browser).ActualHeight);

            foreach (var button in ToolButtons(browser).Cast<Control>().Append(GroupSwitch(browser)))
            {
                Assert.Equal(30, button.ActualWidth);
                Assert.Equal(30, button.ActualHeight);
            }
        });
    }

    [Fact]
    public void The_tools_are_ghost_buttons_with_no_minimum_of_their_own()
    {
        RunOnSTA(() =>
        {
            var browser = Hosted();

            Assert.Equal(3, ToolButtons(browser).Length);

            foreach (var button in ToolButtons(browser))
            {
                Assert.Equal(0, button.MinWidth);
                Assert.Equal(0, button.MinHeight);

                // Ghost: a transparent fill and no border, so the plate is the only edge in the palette.
                Assert.Equal(new Thickness(0), button.BorderThickness);
            }
        });
    }

    [Fact]
    public void The_icon_size_reaches_the_tools_and_the_node_headers()
    {
        RunOnSTA(() =>
        {
            var browser = Hosted(b => b.IconSize = 16);

            Assert.Equal(16, FirstNodeIcon(browser).Width);

            foreach (var button in ToolButtons(browser)) Assert.Equal(16, IconElement.GetIconSize(button));
        });
    }

    [Fact]
    public void The_captions_reach_the_search_box_and_both_pickers()
    {
        RunOnSTA(() =>
        {
            var browser = Hosted(b =>
            {
                b.SearchPlaceholder = "Tìm view, sheet, family…";
                b.AllKindsCaption = "Mọi loại";
                b.SortCaptions = new[] { "Tên A→Z", "Tên Z→A", "Theo loại" };
            });

            Assert.Equal("Tìm view, sheet, family…", Search(browser).Placeholder);
            Assert.Equal("Mọi loại", KindFilter(browser).Items[0]);
            Assert.Equal(new[] { "Tên A→Z", "Tên Z→A", "Theo loại" }, Sort(browser).Items.Cast<string>());
        });
    }

    [Fact]
    public void A_partial_caption_list_keeps_the_defaults_for_the_rest()
    {
        RunOnSTA(() =>
        {
            var browser = Hosted(b => b.SortCaptions = new[] { "Name A→Z" });

            Assert.Equal(new[] { "Name A→Z", "Name Z–A", "By kind" }, Sort(browser).Items.Cast<string>());
        });
    }

    [Fact]
    public void The_footer_counts_what_the_filter_left()
    {
        RunOnSTA(() =>
        {
            var browser = Hosted(b => b.CountCaption = "{0} mục");

            // Project, Views, Level 1, Level 2, Sheets and A101.
            Assert.Equal("6 mục", Counter(browser).Text);

            browser.SetSearchText("Level");

            Assert.Equal("4 mục", Counter(browser).Text);
        });
    }

    [Fact]
    public void The_empty_state_says_so_and_goes_away_again()
    {
        RunOnSTA(() =>
        {
            var browser = Hosted(b => b.EmptyText = "Không có mục nào khớp bộ lọc.");

            Assert.Equal(Visibility.Collapsed, EmptyState(browser).Visibility);

            browser.SetSearchText("nothing matches this");

            Assert.Equal(Visibility.Visible, EmptyState(browser).Visibility);
            Assert.Equal("Không có mục nào khớp bộ lọc.", EmptyState(browser).Text);
            Assert.Empty(Tree(browser).Items);

            browser.SetSearchText(string.Empty);

            Assert.Equal(Visibility.Collapsed, EmptyState(browser).Visibility);
        });
    }

    [Fact]
    public void A_hidden_group_leaves_and_the_header_follows_the_last_one()
    {
        RunOnSTA(() =>
        {
            var browser = Hosted(b =>
            {
                b.ShowToolbar = false;
                b.ShowSearch = false;
                b.ShowKindFilter = false;
                b.ShowSort = false;
                b.ShowGroup = false;
            });

            // Nothing left in the header: the tree starts at the top edge instead of under a band.
            Assert.Equal(Visibility.Collapsed, Header(browser).Visibility);
            Assert.Equal(Visibility.Collapsed, HeaderRule(browser).Visibility);

            Assert.Equal(Visibility.Collapsed, Tools(browser).Visibility);
            Assert.Equal(Visibility.Collapsed, Search(browser).Visibility);

            browser.ShowSearch = true;

            Assert.Equal(Visibility.Visible, Header(browser).Visibility);
            Assert.Equal(Visibility.Visible, Search(browser).Visibility);
            Assert.Equal(Visibility.Collapsed, KindFilter(browser).Visibility);
        });
    }

    [Fact]
    public void The_footer_rule_leaves_with_the_count()
    {
        RunOnSTA(() =>
        {
            var browser = Hosted(b => b.ShowCount = false);

            Assert.Equal(Visibility.Collapsed, Footer(browser).Visibility);

            browser.ShowCount = true;

            Assert.Equal(Visibility.Visible, Footer(browser).Visibility);
        });
    }

    [Fact]
    public void The_group_switch_follows_the_property_both_ways()
    {
        RunOnSTA(() =>
        {
            var browser = Hosted();

            Assert.False(GroupSwitch(browser).IsChecked);

            browser.GroupMode = ProjectGroupMode.ByKind;

            Assert.True(GroupSwitch(browser).IsChecked);
            Assert.NotEmpty(Tree(browser).Items);

            browser.GroupMode = ProjectGroupMode.None;

            Assert.False(GroupSwitch(browser).IsChecked);
        });
    }

    [Fact]
    public void The_sort_picker_follows_the_property_both_ways()
    {
        RunOnSTA(() =>
        {
            var browser = Hosted();

            Assert.Equal(0, Sort(browser).SelectedIndex);

            browser.SortMode = ProjectSortMode.NameDescending;

            Assert.Equal(1, Sort(browser).SelectedIndex);

            Sort(browser).SelectedIndex = 2;

            Assert.Equal(ProjectSortMode.Kind, browser.SortMode);
        });
    }

    [Fact]
    public void A_narrow_browser_still_lays_out_the_search_row_on_one_line()
    {
        RunOnSTA(() =>
        {
            var browser = Hosted(b => b.Width = 240);
            double top = Search(browser).TranslatePoint(new Point(0, 0), browser).Y;
            double kindTop = KindFilter(browser).TranslatePoint(new Point(0, 0), browser).Y;

            // The kind filter sits beside the search, not under it: a palette cannot afford two
            // stacked full-width bars before the tree begins.
            Assert.Equal(top, kindTop, 1.0);
            Assert.True(KindFilter(browser).TranslatePoint(new Point(0, 0), browser).X > 100);
        });
    }
}
