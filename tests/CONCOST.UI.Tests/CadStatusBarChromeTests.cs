using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using CONCOST.UI.AttachedProperties;
using CONCOST.UI.Controls;
using Xunit;

namespace CONCOST.UI.Tests;

/// <summary>
/// The dress of the CAD status bar: how many outlines it draws, which variant removes the plate,
/// and whether the size, caption and group properties actually reach the parts they name.
/// </summary>
/// <remarks>
/// The bar used to draw a plate inside the page's card, five chips of 34 px that each drew their
/// own box, and three solid accent fills on the switches that happened to be on. So the
/// load-bearing test paints the bar and counts the ink along the top edge of the plate; the rest
/// guard the plumbing that keeps that one outline and those five captions intact.
/// </remarks>
public class CadStatusBarChromeTests
{
    private const int Width = 760;
    private const int Height = 44;

    private static void RunOnSTA(Action action) => WpfApplicationHost.Run(action);

    /// <summary>
    /// The tokens and the styles the bar reads. Without them every <c>SetResourceReference</c> in
    /// the control comes back empty and the toggles fall back to the stock WPF button — itself an
    /// outlined box, so the outline test would pass for the wrong reason.
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
    /// A bar laid out inside a white host. <paramref name="configure"/> runs before the first layout
    /// pass, because that is when the resource references resolve and the plate is measured.
    /// </summary>
    private static CadStatusBar Hosted(Action<CadStatusBar>? configure = null)
    {
        var bar = new CadStatusBar();

        configure?.Invoke(bar);

        var host = new Border { Child = bar, Background = Brushes.White, Width = Width, Height = Height };

        host.Resources.MergedDictionaries.Add(Theme());

        host.Measure(new Size(Width, Height));
        host.Arrange(new Rect(0, 0, Width, Height));
        host.UpdateLayout();

        return bar;
    }

    /// <summary>The one plate the bar draws.</summary>
    private static Border Frame(CadStatusBar bar) => (Border)bar.Children[0];

    private static Grid Layout(CadStatusBar bar) => (Grid)Frame(bar).Child;

    private static StackPanel Left(CadStatusBar bar) => (StackPanel)Layout(bar).Children[0];

    private static TextBlock Selection(CadStatusBar bar) => (TextBlock)Left(bar).Children[0];

    private static SplitButton WorkPlane(CadStatusBar bar) => Left(bar).Children.OfType<SplitButton>().First();

    private static TextBlock Coordinates(CadStatusBar bar) => Layout(bar).Children.OfType<TextBlock>().First();

    private static StackPanel Right(CadStatusBar bar) => (StackPanel)Layout(bar).Children[2];

    private static StackPanel ToggleStrip(CadStatusBar bar) => Right(bar).Children.OfType<StackPanel>().First();

    private static ToggleButton[] Toggles(CadStatusBar bar) => ToggleStrip(bar).Children.OfType<ToggleButton>().ToArray();

    /// <summary>The background task counter, then the notification bell.</summary>
    private static Button[] Counters(CadStatusBar bar) => Right(bar).Children.OfType<Button>().ToArray();

    private static Border LeftDivider(CadStatusBar bar) => Left(bar).Children.OfType<Border>().First();

    private static Border RightDivider(CadStatusBar bar) => Right(bar).Children.OfType<Border>().First();

    /// <summary>Every clickable part of the bar: five switches, two counters and the work plane.</summary>
    private static Control[] Commands(CadStatusBar bar) =>
        Toggles(bar).Cast<Control>().Concat(Counters(bar)).Append(WorkPlane(bar)).ToArray();

    private static byte[] Render(CadStatusBar bar)
    {
        var host = (FrameworkElement)VisualTreeHelper.GetParent(bar);
        var bitmap = new RenderTargetBitmap(Width, Height, 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(host);

        var pixels = new byte[Width * Height * 4];
        bitmap.CopyPixels(pixels, Width * 4, 0);

        return pixels;
    }

    /// <summary>The row the plate's top edge lands on, read from the visual tree instead of guessed.</summary>
    private static int TopEdge(CadStatusBar bar)
    {
        var host = (FrameworkElement)VisualTreeHelper.GetParent(bar);
        var origin = Frame(bar).TransformToAncestor(host).Transform(new Point(0, 0));

        return (int)Math.Round(origin.Y);
    }

    /// <summary>How many columns of the plate's top edge carry a painted border.</summary>
    private static int InkedAlongTheTopEdge(CadStatusBar bar)
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

            // Not one control may add an outline of its own: five outlined chips inside an
            // outlined plate is the "table of boxes" the bar used to show.
            foreach (var command in Commands(bar))
            {
                Assert.Equal(new Thickness(0), command.BorderThickness);
            }

            Assert.Equal(5, Toggles(bar).Length);
        });
    }

    [Fact]
    public void The_plate_is_the_only_ink_on_the_top_edge()
    {
        RunOnSTA(() =>
        {
            // Two pictures need two bars: a laid-out element has already painted its one frame.
            int floating = InkedAlongTheTopEdge(Hosted());
            int seamless = InkedAlongTheTopEdge(Hosted(bar => bar.Variant = CadStatusBarVariant.Seamless));

            Assert.True(floating > (Width - 80) * 0.5,
                $"The floating plate painted {floating} of {Width - 80} columns along its top edge.");

            Assert.Equal(0, seamless);
        });
    }

    [Fact]
    public void Seamless_drops_the_plate_entirely()
    {
        RunOnSTA(() =>
        {
            var frame = Frame(Hosted(bar => bar.Variant = CadStatusBarVariant.Seamless));

            Assert.Equal(new Thickness(0), frame.BorderThickness);
            Assert.Equal(0, VisualElement.GetElevation(frame));

            // A transparent background, not a white one: the strip underneath must show through.
            Assert.Equal(0, ((SolidColorBrush)frame.Background).Color.A);
        });
    }

    [Fact]
    public void Glass_keeps_a_shadow_but_drops_the_outline()
    {
        RunOnSTA(() =>
        {
            var frame = Frame(Hosted(bar => bar.Variant = CadStatusBarVariant.Glass));

            Assert.Equal(new Thickness(0), frame.BorderThickness);
            Assert.Equal(0.9, frame.Opacity, 3);

            // Glass sits over a drawing, so it keeps at least the shadow a floating plate has.
            Assert.True(VisualElement.GetElevation(frame) >= 2);
        });
    }

    [Fact]
    public void Switching_the_variant_back_restores_the_frame()
    {
        RunOnSTA(() =>
        {
            var bar = Hosted(bar => bar.Variant = CadStatusBarVariant.Seamless);
            var frame = Frame(bar);

            bar.Variant = CadStatusBarVariant.Floating;

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
                bar.CornerRadius = new CornerRadius(16);
            }));

            Assert.Equal(3, VisualElement.GetElevation(frame));
            Assert.Equal(new CornerRadius(16), frame.CornerRadius);
        });
    }

    [Fact]
    public void The_switches_carry_their_captions()
    {
        RunOnSTA(() =>
        {
            var captions = Toggles(Hosted()).Select(toggle => toggle.Content).ToArray();

            Assert.Equal(new object?[] { "SNAP", "GRID", "ORTHO", "POLAR", "OSNAP" }, captions);
        });
    }

    [Fact]
    public void The_captions_go_away_with_show_toggle_labels()
    {
        RunOnSTA(() =>
        {
            var bar = Hosted();

            foreach (var toggle in Toggles(bar))
            {
                Assert.NotNull(toggle.Content);
            }

            bar.ShowToggleLabels = false;

            // Null, not an empty string: the template collapses the caption on a null content, so
            // the glyph sits on the middle of its own hit area again.
            foreach (var toggle in Toggles(bar))
            {
                Assert.Null(toggle.Content);
            }
        });
    }

    [Fact]
    public void The_default_height_is_the_cad_24_and_nothing_is_coerced_to_34()
    {
        RunOnSTA(() =>
        {
            var bar = Hosted();

            Assert.Equal(24, bar.ControlHeight);

            // The stock icon toggle style asks for a 34 px box; the bar has to overrule it or five
            // switches become the tallest thing on the bar.
            foreach (var toggle in Toggles(bar))
            {
                Assert.Equal(0, toggle.MinWidth);
                Assert.Equal(0, toggle.MinHeight);
                Assert.Equal(24, toggle.ActualHeight);
            }

            Assert.Equal(24, WorkPlane(bar).ActualHeight);

            foreach (var counter in Counters(bar))
            {
                // The task counter is out of sight while nothing is running, so what it carries is
                // the property rather than a laid-out height.
                Assert.Equal(24, counter.Height);
            }

            Assert.Equal(24, Counters(bar)[1].ActualHeight);
        });
    }

    [Fact]
    public void A_control_height_reaches_every_part_of_the_bar()
    {
        RunOnSTA(() =>
        {
            const double Row = 30;

            var bar = Hosted(bar =>
            {
                bar.ControlHeight = Row;
                bar.IconSize = 16;
            });

            foreach (var toggle in Toggles(bar))
            {
                Assert.Equal(Row, toggle.ActualHeight);
            }

            Assert.Equal(Row, WorkPlane(bar).ActualHeight);

            foreach (var counter in Counters(bar))
            {
                Assert.Equal(Row, counter.Height);
            }

            foreach (var toggle in Toggles(bar))
            {
                Assert.Equal(16, IconElement.GetIconSize(toggle));
            }

            Assert.Equal(16, IconElement.GetIconSize(Counters(bar)[1]));
            Assert.Equal(16, WorkPlane(bar).IconSize);
        });
    }

    [Fact]
    public void A_hidden_group_takes_its_rule_with_it()
    {
        RunOnSTA(() =>
        {
            var bar = Hosted();

            Assert.Equal(Visibility.Visible, LeftDivider(bar).Visibility);
            Assert.Equal(Visibility.Visible, RightDivider(bar).Visibility);

            bar.ShowToggles = false;
            bar.ShowSelection = false;

            Assert.Equal(Visibility.Collapsed, ToggleStrip(bar).Visibility);
            Assert.Equal(Visibility.Collapsed, Selection(bar).Visibility);

            // A rule with no group on one side of it is a floating line at the end of the bar.
            Assert.Equal(Visibility.Collapsed, RightDivider(bar).Visibility);
            Assert.Equal(Visibility.Collapsed, LeftDivider(bar).Visibility);
        });
    }

    [Fact]
    public void Show_dividers_hides_both_rules()
    {
        RunOnSTA(() =>
        {
            var bar = Hosted(bar => bar.ShowDividers = false);

            Assert.Equal(Visibility.Collapsed, LeftDivider(bar).Visibility);
            Assert.Equal(Visibility.Collapsed, RightDivider(bar).Visibility);

            // The rest of the bar is untouched by the property.
            Assert.Equal(Visibility.Visible, ToggleStrip(bar).Visibility);
            Assert.Equal(Visibility.Visible, WorkPlane(bar).Visibility);
        });
    }

    [Fact]
    public void The_counters_stay_out_of_sight_until_they_have_something_to_say()
    {
        RunOnSTA(() =>
        {
            var bar = Hosted();

            Assert.Equal(Visibility.Collapsed, Counters(bar)[0].Visibility);
            Assert.Equal(Visibility.Visible, Counters(bar)[1].Visibility);

            bar.AddTask(new CadTask("Regenerating", 25));

            Assert.Equal(Visibility.Visible, Counters(bar)[0].Visibility);
            Assert.Equal("1", Counters(bar)[0].Content);

            bar.Notify(new CadNotification("Saved", "Project1.rvt", CadNotificationSeverity.Success));

            Assert.Equal("1", Counters(bar)[1].Content);
        });
    }

    [Fact]
    public void A_host_can_turn_the_counters_off_altogether()
    {
        RunOnSTA(() =>
        {
            var bar = Hosted(bar =>
            {
                bar.ShowTaskQueue = false;
                bar.ShowNotifications = false;
            });

            bar.AddTask(new CadTask("Regenerating", 25));
            bar.Notify(new CadNotification("Saved"));

            Assert.Equal(Visibility.Collapsed, Counters(bar)[0].Visibility);
            Assert.Equal(Visibility.Collapsed, Counters(bar)[1].Visibility);

            // With nothing left on the right, the rule that separated it goes too.
            Assert.Equal(Visibility.Collapsed, RightDivider(bar).Visibility);
        });
    }

    [Fact]
    public void The_coordinates_take_the_middle_column()
    {
        RunOnSTA(() =>
        {
            var bar = Hosted(bar =>
            {
                bar.Coordinates = new Point3D(1250, 800, 0);
                bar.SelectionCount = 3;
            });

            Assert.Equal(1, Grid.GetColumn(Coordinates(bar)));
            Assert.Contains("X", Coordinates(bar).Text);
            Assert.Contains("mm", Coordinates(bar).Text);

            Assert.Equal("3 objects selected", Selection(bar).Text);

            bar.ShowCoordinates = false;

            Assert.Equal(Visibility.Collapsed, Coordinates(bar).Visibility);
        });
    }

    [Fact]
    public void A_tick_reaches_the_property_and_the_event()
    {
        RunOnSTA(() =>
        {
            var bar = Hosted();
            bool fired = false;

            bar.ToggleChanged += (_, e) => fired = e.Toggle == CadToggle.Ortho && e.IsEnabled;

            Toggles(bar)[2].IsChecked = true;
            Toggles(bar)[2].RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));

            Assert.True(bar.OrthoEnabled);
            Assert.True(fired);
        });
    }
}
