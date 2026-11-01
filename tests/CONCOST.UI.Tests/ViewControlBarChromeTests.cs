using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
/// The dress of the view control bar: how many outlines it draws, which variant removes the plate,
/// and whether the size, group and list properties actually reach the parts they name.
/// </summary>
/// <remarks>
/// Three outlines used to sit on top of each other here — the bar's plate, a bordered chip per
/// toggle and a bordered hide button — and the demo page in the Showcase added a fourth. So the
/// load-bearing test paints the bar and counts the ink along the top edge of the plate; the rest
/// guard the property plumbing that keeps that one outline intact.
/// </remarks>
public class ViewControlBarChromeTests
{
    private const int Width = 560;
    private const int Height = 60;

    private static void RunOnSTA(Action action) => WpfApplicationHost.Run(action);

    /// <summary>
    /// The tokens and the styles the bar reads. Without them every <c>SetResourceReference</c> in
    /// the control comes back empty and the toggles fall back to the stock WPF button — which is
    /// itself an outlined box with a 1 px border, so the test would pass for the wrong reason.
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
    private static ViewControlBar Hosted(Action<ViewControlBar>? configure = null)
    {
        var bar = new ViewControlBar();

        configure?.Invoke(bar);

        var host = new Border { Child = bar, Background = Brushes.White, Width = Width, Height = Height };

        host.Resources.MergedDictionaries.Add(Theme());

        host.Measure(new Size(Width, Height));
        host.Arrange(new Rect(0, 0, Width, Height));
        host.UpdateLayout();

        return bar;
    }

    /// <summary>The one plate the bar draws.</summary>
    private static Border Frame(ViewControlBar bar) => (Border)bar.Children[0];

    private static StackPanel Strip(ViewControlBar bar) => (StackPanel)Frame(bar).Child;

    /// <summary>The scale picker, which is the strip's first child.</summary>
    private static ComboBox Scale(ViewControlBar bar) => (ComboBox)Strip(bar).Children[0];

    private static ComboBox Detail(ViewControlBar bar) => (ComboBox)Strip(bar).Children[1];

    private static ComboBox StyleBox(ViewControlBar bar) => (ComboBox)Strip(bar).Children[2];

    /// <summary>The rules between the groups, in the order they appear on the bar.</summary>
    private static Border[] Dividers(ViewControlBar bar) => Strip(bar).Children.OfType<Border>().ToArray();

    private static StackPanel ToggleCluster(ViewControlBar bar) =>
        Strip(bar).Children.OfType<StackPanel>().First();

    private static ToggleButton[] Toggles(ViewControlBar bar) =>
        ToggleCluster(bar).Children.OfType<ToggleButton>().ToArray();

    private static SplitButton Hide(ViewControlBar bar) => Strip(bar).Children.OfType<SplitButton>().First();

    /// <summary>Every clickable part of the bar: the three toggles and the hide/isolate button.</summary>
    private static IEnumerable<Control> Commands(ViewControlBar bar) =>
        Toggles(bar).Cast<Control>().Concat(new Control[] { Hide(bar) });

    private static byte[] Render(ViewControlBar bar)
    {
        var host = (FrameworkElement)VisualTreeHelper.GetParent(bar);
        var bitmap = new RenderTargetBitmap(Width, Height, 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(host);

        var pixels = new byte[Width * Height * 4];
        bitmap.CopyPixels(pixels, Width * 4, 0);

        return pixels;
    }

    /// <summary>
    /// The row the plate's top edge lands on. The bar is bottom-aligned with an 8 px margin, so the
    /// row depends on the height of the plate and is read from the visual tree instead of guessed.
    /// </summary>
    private static int TopEdge(ViewControlBar bar)
    {
        var host = (FrameworkElement)VisualTreeHelper.GetParent(bar);
        var origin = Frame(bar).TransformToAncestor(host).Transform(new Point(0, 0));

        return (int)Math.Round(origin.Y);
    }

    /// <summary>How many columns of the frame's top edge carry a painted border.</summary>
    private static int InkedAlongTheTopEdge(ViewControlBar bar)
    {
        byte[] pixels = Render(bar);
        int top = TopEdge(bar);
        int inked = 0;

        // The corners are rounded, so they are left out: on a floating bar the middle of the edge
        // is a rule from one end of the plate to the other.
        for (int x = 40; x < Width - 40; x++)
        {
            if (Inked(pixels, x, top) || Inked(pixels, x, top + 1)) inked++;
        }

        return inked;
    }

    private static bool Inked(byte[] pixels, int x, int y)
    {
        int index = (y * Width + x) * 4;

        return pixels[index] != 0xFF || pixels[index + 1] != 0xFF || pixels[index + 2] != 0xFF;
    }

    [Fact]
    public void The_default_bar_draws_one_outline_around_flat_controls()
    {
        RunOnSTA(() =>
        {
            var bar = Hosted();
            var frame = Frame(bar);

            Assert.Equal(new Thickness(1), frame.BorderThickness);
            Assert.NotNull(frame.BorderBrush);

            // Not one control may add an outline of its own: a row of outlined chips inside an
            // outlined plate is the "two borders" the bar used to show.
            foreach (var command in Commands(bar))
            {
                Assert.Equal(new Thickness(0), command.BorderThickness);
            }

            Assert.Equal(3, Toggles(bar).Length);
        });
    }

    [Fact]
    public void The_plate_is_the_only_ink_on_the_top_edge()
    {
        RunOnSTA(() =>
        {
            // Two pictures need two bars: a laid-out element has already painted its one frame.
            int floating = InkedAlongTheTopEdge(Hosted());
            int seamless = InkedAlongTheTopEdge(Hosted(bar => bar.Variant = ViewControlBarVariant.Seamless));

            Assert.True(floating > (Width - 80) * 0.5,
                $"The floating frame painted {floating} of {Width - 80} columns along its top edge.");

            Assert.Equal(0, seamless);
        });
    }

    [Fact]
    public void Seamless_drops_the_plate_entirely()
    {
        RunOnSTA(() =>
        {
            var frame = Frame(Hosted(bar => bar.Variant = ViewControlBarVariant.Seamless));

            Assert.Equal(new Thickness(0), frame.BorderThickness);
            Assert.Equal(0, VisualElement.GetElevation(frame));

            // A transparent background, not a white one: the status bar underneath must show through.
            Assert.Equal(0, ((SolidColorBrush)frame.Background).Color.A);
        });
    }

    [Fact]
    public void Glass_keeps_a_shadow_but_drops_the_outline()
    {
        RunOnSTA(() =>
        {
            var frame = Frame(Hosted(bar => bar.Variant = ViewControlBarVariant.Glass));

            Assert.Equal(new Thickness(0), frame.BorderThickness);
            Assert.Equal(0.9, frame.Opacity, 3);

            // Glass floats over a drawing, so it keeps at least the shadow a floating bar has.
            Assert.True(VisualElement.GetElevation(frame) >= 2);
        });
    }

    [Fact]
    public void Switching_the_variant_back_restores_the_frame()
    {
        RunOnSTA(() =>
        {
            var bar = Hosted(bar => bar.Variant = ViewControlBarVariant.Seamless);
            var frame = Frame(bar);

            bar.Variant = ViewControlBarVariant.Floating;

            Assert.Equal(new Thickness(1), frame.BorderThickness);
            Assert.Equal(1, VisualElement.GetElevation(frame));

            // The plate is opaque again, which the seamless variant had switched off.
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
    public void The_default_height_is_the_cad_26()
    {
        RunOnSTA(() =>
        {
            var bar = Hosted();

            Assert.Equal(26, bar.ControlHeight);
            Assert.Equal(26, Scale(bar).ActualHeight);

            // The toggle style asks for a 34 px touch target; the bar has to overrule it.
            foreach (var toggle in Toggles(bar))
            {
                Assert.Equal(0, toggle.MinWidth);
                Assert.Equal(0, toggle.MinHeight);
                Assert.Equal(26, toggle.ActualHeight);
            }
        });
    }

    [Fact]
    public void A_control_height_is_not_coerced_by_the_combo_minimum()
    {
        RunOnSTA(() =>
        {
            const double Row = 30;

            var bar = Hosted(bar => bar.ControlHeight = Row);

            foreach (var combo in new[] { Scale(bar), Detail(bar), StyleBox(bar) })
            {
                // The combo style asks for a 34 px row; a local height loses to it unless the
                // minimum is zeroed first, which is why the property used to do nothing.
                Assert.Equal(0, combo.MinHeight);
                Assert.Equal(Row, combo.ActualHeight);
            }

            foreach (var toggle in Toggles(bar))
            {
                Assert.Equal(Row, toggle.ActualWidth);
                Assert.Equal(Row, toggle.ActualHeight);
            }

            Assert.Equal(Row, Hide(bar).ActualHeight);
        });
    }

    [Fact]
    public void Icon_size_reaches_every_toggle_and_the_hide_button()
    {
        RunOnSTA(() =>
        {
            var bar = Hosted(bar => bar.IconSize = 18);

            Assert.All(Toggles(bar), toggle => Assert.Equal(18, IconElement.GetIconSize(toggle)));
            Assert.Equal(18, Hide(bar).IconSize);
        });
    }

    [Fact]
    public void Hiding_the_toggle_cluster_takes_both_rules_with_it()
    {
        RunOnSTA(() =>
        {
            var bar = Hosted(bar => bar.ShowToggles = false);

            Assert.Equal(Visibility.Collapsed, ToggleCluster(bar).Visibility);

            // A rule also leaves with the group beside it, so no floating line is left behind.
            Assert.Equal(Visibility.Collapsed, Dividers(bar)[0].Visibility);
            Assert.Equal(Visibility.Collapsed, Dividers(bar)[1].Visibility);

            // The pickers and the hide button were not asked to leave.
            Assert.Equal(Visibility.Visible, Scale(bar).Visibility);
            Assert.Equal(Visibility.Visible, Hide(bar).Visibility);
        });
    }

    [Fact]
    public void Each_picker_can_leave_on_its_own()
    {
        RunOnSTA(() =>
        {
            var bar = Hosted(bar =>
            {
                bar.ShowScale = false;
                bar.ShowDetailLevel = false;
                bar.ShowVisualStyle = false;
            });

            Assert.Equal(Visibility.Collapsed, Scale(bar).Visibility);
            Assert.Equal(Visibility.Collapsed, Detail(bar).Visibility);
            Assert.Equal(Visibility.Collapsed, StyleBox(bar).Visibility);

            // The rule between the pickers and the toggles has lost a side, so it goes; the one
            // between the toggles and the hide button still stands between two live groups.
            Assert.Equal(Visibility.Collapsed, Dividers(bar)[0].Visibility);
            Assert.Equal(Visibility.Visible, Dividers(bar)[1].Visibility);
        });
    }

    [Fact]
    public void Show_hide_isolate_removes_the_button_and_its_rule()
    {
        RunOnSTA(() =>
        {
            var bar = Hosted(bar => bar.ShowHideIsolate = false);

            Assert.Equal(Visibility.Collapsed, Hide(bar).Visibility);
            Assert.Equal(Visibility.Collapsed, Dividers(bar)[1].Visibility);

            // The rule between the pickers and the toggles still has both its neighbours.
            Assert.Equal(Visibility.Visible, Dividers(bar)[0].Visibility);
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
            Assert.Equal(Visibility.Visible, Scale(bar).Visibility);
            Assert.Equal(Visibility.Visible, ToggleCluster(bar).Visibility);
        });
    }

    [Fact]
    public void The_hide_caption_is_translatable()
    {
        RunOnSTA(() =>
        {
            var bar = Hosted(bar => bar.HideIsolateCaption = "Ẩn");

            Assert.Equal("Ẩn", Hide(bar).Content);
        });
    }

    [Fact]
    public void The_scale_list_refills_the_dropdown_as_it_grows()
    {
        RunOnSTA(() =>
        {
            var scales = new ObservableCollection<string> { "1:50", "1:250" };
            var bar = Hosted(b => b.Scales = scales);

            // The two the host offered, in its order. The scale the bar is currently on is added
            // behind them, so a view restored on a scale nobody listed is never a blank box.
            Assert.Equal(new[] { "1:50", "1:250" }, Scale(bar).Items.Cast<string>().Take(2));
            Assert.Contains(bar.ViewScale, Scale(bar).Items.Cast<string>());

            scales.Add("1:500");

            // An observable list is a live list, not a snapshot taken at assignment.
            Assert.Contains("1:500", Scale(bar).Items.Cast<string>());
        });
    }

    [Fact]
    public void A_saved_scale_outside_the_list_is_still_shown()
    {
        RunOnSTA(() =>
        {
            var bar = Hosted(b => b.ViewScale = "1:250");

            // The dropdown grows instead of going blank, because a restored view may be read at a
            // scale the bar was never told about.
            Assert.Contains("1:250", Scale(bar).Items.Cast<string>());
            Assert.Equal("1:250", Scale(bar).SelectedItem);
        });
    }

    [Fact]
    public void The_default_scales_are_the_five_a_plan_is_read_at()
    {
        RunOnSTA(() =>
        {
            var bar = Hosted();

            Assert.Equal(new[] { "1:50", "1:100", "1:200", "1:500", "1:1000" }, Scale(bar).Items.Cast<string>());
            Assert.Equal("1:100", Scale(bar).SelectedItem);
        });
    }
}
