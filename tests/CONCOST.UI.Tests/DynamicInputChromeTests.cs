using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using CONCOST.UI.AttachedProperties;
using CONCOST.UI.Controls;
using Xunit;

namespace CONCOST.UI.Tests;

/// <summary>
/// The dress of the dynamic input: how many outlines it draws, what each variant keeps, and whether
/// the size, unit and placement properties reach the parts they name.
/// </summary>
/// <remarks>
/// The page used to show a white plate with a rounded border and a stock <c>TextBox</c> inside it —
/// two outlines around one value, an empty field and nothing tying it to the point it belonged to.
/// The load-bearing test here is therefore the first one: the field draws an underline and the plate
/// draws the outline, never both at once. The rest guard the property plumbing around it.
/// </remarks>
public class DynamicInputChromeTests
{
    private const int Width = 320;
    private const int Height = 220;
    private static readonly Point AnchorPoint = new(60, 40);

    private static void RunOnSTA(Action action) => WpfApplicationHost.Run(action);

    /// <summary>
    /// The tokens and styles the box reads. Host-local: the same dictionaries merged into
    /// <see cref="Application.Current"/> would change what every other test on this thread finds.
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

    /// <summary>An open box laid out inside a white host.</summary>
    private static DynamicInput Hosted(Action<DynamicInput>? configure = null, Point? anchor = null)
    {
        var input = new DynamicInput { Kind = DynamicInputKind.Distance, Prompt = "Chiều dài" };

        configure?.Invoke(input);

        var host = new Border { Child = input, Background = Brushes.White, Width = Width, Height = Height };

        host.Resources.MergedDictionaries.Add(Theme());

        host.Measure(new Size(Width, Height));
        host.Arrange(new Rect(0, 0, Width, Height));

        input.Open(anchor ?? AnchorPoint);
        host.UpdateLayout();

        return input;
    }

    /// <summary>The leader canvas is child 0 and the plate is child 1, in the order they are added.</summary>
    private static Border Frame(DynamicInput input) => input.Children.OfType<Border>().First();

    private static Grid Layout(DynamicInput input) => (Grid)Frame(input).Child;

    private static TextBox Field(DynamicInput input) => Layout(input).Children.OfType<TextBox>().First();

    private static TextBlock Label(DynamicInput input) => Layout(input).Children.OfType<TextBlock>().First();

    private static TextBlock Unit(DynamicInput input) => Layout(input).Children.OfType<TextBlock>().ElementAt(1);

    private static TextBlock Hint(DynamicInput input) => Layout(input).Children.OfType<TextBlock>().ElementAt(2);

    private static Line Leader(DynamicInput input) =>
        (Line)((System.Windows.Controls.Canvas)input.Children[0]).Children[0];

    private static Color Token(DynamicInput input, string key) =>
        ((SolidColorBrush)input.FindResource(key)).Color;

    [Fact]
    public void The_field_draws_an_underline_and_the_plate_draws_the_outline()
    {
        RunOnSTA(() =>
        {
            var input = Hosted();

            // One outline only: the plate. A full rectangle inside it was the form-in-a-form look.
            Assert.Equal(new Thickness(1), Frame(input).BorderThickness);
            Assert.Equal(new Thickness(0, 0, 0, 1.5), Field(input).BorderThickness);
        });
    }

    [Fact]
    public void Compact_puts_the_question_and_the_field_on_one_row_and_drops_the_plate()
    {
        RunOnSTA(() =>
        {
            var input = Hosted(box => box.Variant = DynamicInputVariant.Compact);

            Assert.Equal(new Thickness(0), Frame(input).BorderThickness);
            Assert.Equal(0, VisualElement.GetElevation(Frame(input)));

            // Same two elements, one row: nothing is rebuilt when the variant changes.
            Assert.Equal(Grid.GetRow(Label(input)), Grid.GetRow(Field(input)));
            Assert.Equal(1, Grid.GetColumn(Field(input)));
        });
    }

    [Fact]
    public void Glass_keeps_the_shadow_and_turns_the_plate_translucent()
    {
        RunOnSTA(() =>
        {
            var input = Hosted(box => box.Variant = DynamicInputVariant.Glass);

            Assert.Equal(new Thickness(1), Frame(input).BorderThickness);
            Assert.Equal(0.94, Frame(input).Opacity, 3);
            Assert.True(VisualElement.GetElevation(Frame(input)) >= 2);
        });
    }

    [Fact]
    public void Switching_the_variant_back_restores_the_plate()
    {
        RunOnSTA(() =>
        {
            var input = Hosted(box => box.Variant = DynamicInputVariant.Compact);

            input.Variant = DynamicInputVariant.Floating;

            Assert.Equal(new Thickness(1), Frame(input).BorderThickness);
            Assert.Equal(1, Frame(input).Opacity);
            Assert.Equal(1, VisualElement.GetElevation(Frame(input)));
        });
    }

    [Fact]
    public void Corner_radius_and_elevation_reach_the_plate()
    {
        RunOnSTA(() =>
        {
            var input = Hosted(box =>
            {
                box.CornerRadius = new CornerRadius(16);
                box.Elevation = 3;
            });

            Assert.Equal(new CornerRadius(16), Frame(input).CornerRadius);
            Assert.Equal(3, VisualElement.GetElevation(Frame(input)));
        });
    }

    [Fact]
    public void The_unit_follows_the_kind_until_it_is_overridden()
    {
        RunOnSTA(() =>
        {
            var input = Hosted();

            Assert.Equal("(mm)", Unit(input).Text);
            Assert.Equal(Visibility.Visible, Unit(input).Visibility);

            input.Kind = DynamicInputKind.Angle;
            Assert.Equal("(°)", Unit(input).Text);

            // A point carries no unit, and an inch drawing says so instead of guessing.
            input.Kind = DynamicInputKind.Coordinate;
            Assert.Equal(Visibility.Collapsed, Unit(input).Visibility);

            input.Kind = DynamicInputKind.Distance;
            input.UnitSuffix = "(in)";

            Assert.Equal("(in)", Unit(input).Text);

            input.ShowUnit = false;
            Assert.Equal(Visibility.Collapsed, Unit(input).Visibility);
        });
    }

    [Fact]
    public void The_question_keeps_its_own_text_and_the_unit_is_a_second_block()
    {
        RunOnSTA(() =>
        {
            var input = Hosted();

            // The old box glued "  (mm)" onto the prompt, so a translated prompt read as one run-on.
            Assert.Equal("Chiều dài", Label(input).Text);
            Assert.Equal("(mm)", Unit(input).Text);
        });
    }

    [Fact]
    public void Field_width_reaches_the_value_field()
    {
        RunOnSTA(() =>
        {
            var input = Hosted(box => box.FieldWidth = 200);

            Assert.Equal(200, Field(input).MinWidth);
        });
    }

    [Fact]
    public void Auto_placement_flips_the_box_inward_at_the_right_edge()
    {
        RunOnSTA(() =>
        {
            var input = Hosted(anchor: new Point(Width - 12, 10));

            // The plate would run off the host, so it goes to the left of the point instead.
            Assert.True(input.Margin.Left < Width - 12, $"left margin was {input.Margin.Left}");
            Assert.True(input.Margin.Left >= 0);
        });
    }

    [Fact]
    public void A_forced_placement_does_not_flip()
    {
        RunOnSTA(() =>
        {
            var input = Hosted(box => box.Placement = DynamicInputPlacement.Right,
                anchor: new Point(Width - 12, 10));

            // Same point, same edge, but the host asked for the right side explicitly.
            Assert.Equal(Width - 12 + 18, input.Margin.Left, 1);
        });
    }

    [Fact]
    public void The_leader_runs_from_the_plate_corner_to_the_anchor()
    {
        RunOnSTA(() =>
        {
            var input = Hosted(box =>
            {
                box.Offset = 18;
                box.Placement = DynamicInputPlacement.Right;
            });

            var leader = Leader(input);

            Assert.Equal(Visibility.Visible, leader.Visibility);
            Assert.Equal(0, leader.X1);
            Assert.Equal(0, leader.Y1);
            Assert.Equal(-18, leader.X2);
            Assert.Equal(-18, leader.Y2);
        });
    }

    [Fact]
    public void The_leader_has_a_switch_and_no_room_in_a_strip()
    {
        RunOnSTA(() =>
        {
            var off = Hosted(box => box.ShowLeader = false);
            Assert.Equal(Visibility.Collapsed, Leader(off).Visibility);

            var strip = Hosted(box => box.Variant = DynamicInputVariant.Compact);
            Assert.Equal(Visibility.Collapsed, Leader(strip).Visibility);

            // A closed box has nothing to point at.
            var closed = Hosted();
            closed.IsOpen = false;

            Assert.Equal(Visibility.Collapsed, Leader(closed).Visibility);
        });
    }

    [Fact]
    public void An_unreadable_value_paints_the_one_outline_in_the_danger_colour()
    {
        RunOnSTA(() =>
        {
            var input = Hosted();

            Assert.Equal(Token(input, "CONCOST.Brush.Border"), ((SolidColorBrush)Frame(input).BorderBrush).Color);

            input.Text = "abc";
            Assert.False(input.Commit());

            Assert.True(input.IsOpen);
            Assert.Equal(Token(input, "CONCOST.Brush.Danger"), ((SolidColorBrush)Frame(input).BorderBrush).Color);
            Assert.Equal(Visibility.Visible, Hint(input).Visibility);

            input.Text = "1200";

            Assert.Equal(Token(input, "CONCOST.Brush.Border"), ((SolidColorBrush)Frame(input).BorderBrush).Color);
            Assert.Equal(Visibility.Collapsed, Hint(input).Visibility);
        });
    }

    [Fact]
    public void Opening_with_a_prefill_starts_from_the_current_measurement()
    {
        RunOnSTA(() =>
        {
            var input = new DynamicInput();

            input.Open(AnchorPoint, "Chiều dài", "3200");

            Assert.Equal("3200", input.Text);

            // Opening again without one clears it, so yesterday's answer cannot be committed twice.
            input.Open(AnchorPoint);

            Assert.Equal(string.Empty, input.Text);
        });
    }

    [Fact]
    public void The_error_line_moves_with_the_variant()
    {
        RunOnSTA(() =>
        {
            var floating = Hosted();
            var compact = Hosted(box => box.Variant = DynamicInputVariant.Compact);

            // Two rows in a plate (question, field, hint) and one row plus the complaint in a strip.
            Assert.Equal(2, Grid.GetRow(Hint(floating)));
            Assert.Equal(1, Grid.GetRow(Hint(compact)));
            Assert.Equal(1, Grid.GetRow(Field(floating)));
        });
    }
}
