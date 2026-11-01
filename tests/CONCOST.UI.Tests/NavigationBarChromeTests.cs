using System;
using System.Collections.Generic;
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
/// The dress of the navigation bar: how many outlines it draws, which variant removes the plate,
/// and whether the new size and group properties actually reach the parts they name.
/// </summary>
/// <remarks>
/// The bar's whole complaint was visual — a plate outlined inside an outlined demo frame, with an
/// outlined chip per button. So the load-bearing test here paints the bar and counts the ink along
/// the top edge; the rest guard the property plumbing that keeps that single outline intact.
/// </remarks>
public class NavigationBarChromeTests
{
    private const int Width = 400;
    private const int Height = 56;

    /// <summary>The bar's own margin, which puts the frame's outer edge this far in.</summary>
    private const int Inset = 8;

    private static void RunOnSTA(Action action) => WpfApplicationHost.Run(action);

    /// <summary>
    /// The tokens and the styles the bar reads. Without them every <c>SetResourceReference</c> in
    /// the control comes back empty and the commands fall back to the stock WPF button — which is
    /// itself an outlined box, so the test would pass for the wrong reason.
    /// </summary>
    /// <remarks>
    /// The dictionaries hang off the host, not off <see cref="Application.Current"/>: that one is
    /// shared by every test on the single STA thread, and merging the whole theme into it changed
    /// what the ribbon and the overlay tests found when they went looking for a resource.
    /// </remarks>
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
    /// A bar laid out inside a white host. <paramref name="configure"/> runs before the first layout
    /// pass, because that is when the resource references resolve and the frame is measured.
    /// </summary>
    private static NavigationBar Hosted(Action<NavigationBar>? configure = null)
    {
        var bar = new NavigationBar();

        configure?.Invoke(bar);

        var host = new Border { Child = bar, Background = Brushes.White, Width = Width, Height = Height };

        host.Resources.MergedDictionaries.Add(Theme());

        host.Measure(new Size(Width, Height));
        host.Arrange(new Rect(0, 0, Width, Height));
        host.UpdateLayout();

        return bar;
    }

    /// <summary>The one plate the bar draws.</summary>
    private static Border Frame(NavigationBar bar) => (Border)bar.Children[0];

    private static StackPanel Strip(NavigationBar bar) => (StackPanel)Frame(bar).Child;

    /// <summary>The rules between the groups, in the order they appear on the bar.</summary>
    private static Border[] Dividers(NavigationBar bar) => Strip(bar).Children.OfType<Border>().ToArray();

    /// <summary>The coordinate-system label, which is the strip's first text button.</summary>
    private static Button Label(NavigationBar bar) =>
        Strip(bar).Children.OfType<Button>().First(b => b.Content is string);

    private static StackPanel ZoomCluster(NavigationBar bar) =>
        Strip(bar).Children.OfType<StackPanel>().First();

    private static StackPanel ToolCluster(NavigationBar bar) =>
        Strip(bar).Children.OfType<StackPanel>().ElementAt(1);

    /// <summary>
    /// The icon-sized buttons: the three zoom commands, the three tools and the perspective button.
    /// The coordinate-system label is a text button and sizes itself, so it is not one of these.
    /// </summary>
    private static IEnumerable<ButtonBase> Commands(NavigationBar bar) =>
        Descendants<ButtonBase>(Frame(bar)).Where(command => !ReferenceEquals(command, Label(bar)));

    private static IEnumerable<T> Descendants<T>(DependencyObject root) where T : DependencyObject
    {
        int count = System.Windows.Media.VisualTreeHelper.GetChildrenCount(root);

        for (int i = 0; i < count; i++)
        {
            var child = System.Windows.Media.VisualTreeHelper.GetChild(root, i);

            if (child is T match) yield return match;

            foreach (var nested in Descendants<T>(child)) yield return nested;
        }
    }

    private static byte[] Render(NavigationBar bar)
    {
        var host = (FrameworkElement)VisualTreeHelper.GetParent(bar);
        var bitmap = new RenderTargetBitmap(Width, Height, 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(host);

        var pixels = new byte[Width * Height * 4];
        bitmap.CopyPixels(pixels, Width * 4, 0);

        return pixels;
    }

    /// <summary>How many columns of the frame's top edge carry a painted border.</summary>
    private static int InkedAlongTheTopEdge(byte[] pixels)
    {
        int inked = 0;

        // The corners are rounded, so they are left out: on a floating bar the middle of the edge is
        // a rule from one end of the plate to the other.
        for (int x = 60; x < Width - 60; x++)
        {
            if (Inked(pixels, x, Inset) || Inked(pixels, x, Inset + 1)) inked++;
        }

        return inked;
    }

    private static bool Inked(byte[] pixels, int x, int y)
    {
        int index = (y * Width + x) * 4;

        return pixels[index] != 0xFF || pixels[index + 1] != 0xFF || pixels[index + 2] != 0xFF;
    }

    [Fact]
    public void The_default_bar_draws_one_outline_around_flat_commands()
    {
        RunOnSTA(() =>
        {
            var bar = Hosted();
            var frame = Frame(bar);

            Assert.Equal(new Thickness(1), frame.BorderThickness);
            Assert.NotNull(frame.BorderBrush);

            // Not one command may add an outline of its own: a row of outlined chips inside an
            // outlined plate is the "two borders" the bar used to show.
            Assert.Equal(new Thickness(0), Label(bar).BorderThickness);

            foreach (var command in Commands(bar))
            {
                Assert.Equal(new Thickness(0), command.BorderThickness);
            }

            Assert.Equal(7, Commands(bar).Count());
        });
    }

    [Fact]
    public void The_plate_is_the_only_ink_on_the_top_edge()
    {
        RunOnSTA(() =>
        {
            // Two pictures need two bars: a laid-out element has already painted its one frame.
            var floating = InkedAlongTheTopEdge(Render(Hosted()));
            var seamless = InkedAlongTheTopEdge(Render(Hosted(bar => bar.Variant = NavigationBarVariant.Seamless)));

            Assert.True(floating > Width * 0.5,
                $"The floating frame painted {floating} of {Width - 120} columns along its top edge.");

            Assert.Equal(0, seamless);
        });
    }

    [Fact]
    public void Seamless_drops_the_plate_entirely()
    {
        RunOnSTA(() =>
        {
            var frame = Frame(Hosted(bar => bar.Variant = NavigationBarVariant.Seamless));

            Assert.Equal(new Thickness(0), frame.BorderThickness);
            Assert.Equal(0, VisualElement.GetElevation(frame));

            // A transparent background, not a white one: the toolbar underneath must show through.
            Assert.Equal(0, ((SolidColorBrush)frame.Background).Color.A);
        });
    }

    [Fact]
    public void Glass_keeps_the_shadow_but_drops_the_outline()
    {
        RunOnSTA(() =>
        {
            var frame = Frame(Hosted(bar => bar.Variant = NavigationBarVariant.Glass));

            Assert.Equal(new Thickness(0), frame.BorderThickness);
            Assert.Equal(0.9, frame.Opacity, 3);

            // Glass floats over a scene, so it keeps at least the shadow a floating bar has.
            Assert.True(VisualElement.GetElevation(frame) >= 2);
        });
    }

    [Fact]
    public void Switching_the_variant_back_restores_the_frame()
    {
        RunOnSTA(() =>
        {
            var bar = Hosted(bar => bar.Variant = NavigationBarVariant.Seamless);
            var frame = Frame(bar);

            bar.Variant = NavigationBarVariant.Floating;

            Assert.Equal(new Thickness(1), frame.BorderThickness);
            Assert.Equal(1, VisualElement.GetElevation(frame));

            // The plate is opaque again, which the seam-free variant had switched off.
            Assert.Equal(0xFF, ((SolidColorBrush)frame.Background).Color.A);
        });
    }

    [Fact]
    public void Elevation_and_corner_radius_reach_the_frame()
    {
        RunOnSTA(() =>
        {
            var frame = Frame(Hosted(bar =>
            {
                bar.Elevation = 3;
                bar.CornerRadius = new CornerRadius(18);
            }));

            Assert.Equal(3, VisualElement.GetElevation(frame));
            Assert.Equal(new CornerRadius(18), frame.CornerRadius);
        });
    }

    [Fact]
    public void A_button_size_is_not_coerced_by_the_touch_target()
    {
        RunOnSTA(() =>
        {
            const double Size = 30;

            var bar = Hosted(bar => bar.ButtonSize = Size);

            foreach (var command in Commands(bar))
            {
                // The ghost styles ask for a 34 px minimum; the bar has to overrule it or the size
                // property silently does nothing.
                Assert.Equal(0, command.MinWidth);
                Assert.Equal(Size, command.Width);
                Assert.Equal(Size, command.ActualWidth);
            }
        });
    }

    [Fact]
    public void The_default_button_size_is_the_cad_26()
    {
        RunOnSTA(() =>
        {
            var bar = Hosted();

            Assert.Equal(26, bar.ButtonSize);
            Assert.Equal(26, Commands(bar).First().ActualWidth);
            Assert.Equal(14, bar.IconSize);
        });
    }

    [Fact]
    public void The_strip_turns_with_its_orientation()
    {
        RunOnSTA(() =>
        {
            var bar = Hosted(bar => bar.Orientation = Orientation.Vertical);

            Assert.Equal(Orientation.Vertical, Strip(bar).Orientation);
            Assert.Equal(Orientation.Vertical, ZoomCluster(bar).Orientation);
            Assert.Equal(Orientation.Vertical, ToolCluster(bar).Orientation);

            // The rule turns too: one pixel across the column instead of one pixel along the row.
            var divider = Dividers(bar)[0];

            Assert.True(double.IsNaN(divider.Width));
            Assert.Equal(1, divider.Height);
        });
    }

    [Fact]
    public void Hiding_a_group_takes_its_rules_with_it()
    {
        RunOnSTA(() =>
        {
            var bar = Hosted(bar =>
            {
                bar.ShowUcsLabel = false;
                bar.ShowZoom = false;
            });

            Assert.Equal(Visibility.Collapsed, Label(bar).Visibility);
            Assert.Equal(Visibility.Collapsed, ZoomCluster(bar).Visibility);

            // Both rules that used to stand beside those groups go with them; the one between the
            // tools and the perspective button has nothing to do with the change and stays.
            Assert.Equal(Visibility.Collapsed, Dividers(bar)[0].Visibility);
            Assert.Equal(Visibility.Collapsed, Dividers(bar)[1].Visibility);
            Assert.Equal(Visibility.Visible, Dividers(bar)[2].Visibility);
        });
    }

    [Fact]
    public void Show_walk_removes_only_the_walk_toggle()
    {
        RunOnSTA(() =>
        {
            var bar = Hosted(bar => bar.ShowWalk = false);
            var tools = ToolCluster(bar).Children.OfType<ToggleButton>().ToArray();

            Assert.Equal(3, tools.Length);
            Assert.Equal(Visibility.Visible, tools[0].Visibility);
            Assert.Equal(Visibility.Visible, tools[1].Visibility);

            // Walk is the third one, and it leaves the pan and orbit tools alone.
            Assert.Equal(Visibility.Collapsed, tools[2].Visibility);
        });
    }

    [Fact]
    public void The_dividers_can_be_turned_off_as_a_set()
    {
        RunOnSTA(() =>
        {
            var bar = Hosted(bar => bar.ShowDividers = false);

            Assert.All(Dividers(bar), rule => Assert.Equal(Visibility.Collapsed, rule.Visibility));

            // The groups themselves stay: only the rules were asked to leave.
            Assert.Equal(Visibility.Visible, Label(bar).Visibility);
            Assert.Equal(Visibility.Visible, ZoomCluster(bar).Visibility);
        });
    }
}
