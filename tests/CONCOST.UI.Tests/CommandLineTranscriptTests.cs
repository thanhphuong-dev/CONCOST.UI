using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CONCOST.UI.AttachedProperties;
using CONCOST.UI.Controls;
using Xunit;

namespace CONCOST.UI.Tests;

/// <summary>
/// The parts of the command line that make it read like a CAD transcript rather than a one-box form:
/// the prompt echoed with its answer on the same line, prompt keywords in brackets, the three
/// placements, and the history window.
/// </summary>
/// <remarks>
/// The complaint that started this was a pair of instructional lines sitting where the dialogue
/// belongs. So the first test here asserts the shape of the transcript — <c>Specify first point:
/// 0,0</c> — and the rest pin the behaviour around it.
/// </remarks>
public class CommandLineTranscriptTests
{
    private const int Width = 620;
    private const int Height = 220;

    private static void RunOnSTA(Action action) => WpfApplicationHost.Run(action);

    /// <summary>The tokens and styles the line reads. Host-local, so no other test sees them.</summary>
    private static ResourceDictionary Theme()
    {
        var theme = new ResourceDictionary();

        theme.MergedDictionaries.Add(Dictionary("pack://application:,,,/CONCOST.UI;component/Themes/ThemeResources.xaml"));
        theme.MergedDictionaries.Add(Dictionary("pack://application:,,,/CONCOST.UI;component/Themes/Generic.xaml"));

        return theme;

        static ResourceDictionary Dictionary(string path) =>
            new() { Source = new Uri(path, UriKind.Absolute) };
    }

    private static CommandLine Hosted(Action<CommandLine>? configure = null)
    {
        var line = new CommandLine();

        line.AddCommand(new CommandDefinition("LINE", "Draw a polyline") { Prompt = "Specify first point" });
        line.AddCommand(new CommandDefinition("ZOOM", "Zoom the view").WithAliases("Z"));

        configure?.Invoke(line);

        var host = new Border { Child = line, Background = Brushes.White, Width = Width, Height = Height };

        host.Resources.MergedDictionaries.Add(Theme());

        host.Measure(new Size(Width, Height));
        host.Arrange(new Rect(0, 0, Width, Height));
        host.UpdateLayout();

        return line;
    }

    private static Border Frame(CommandLine line) => (Border)line.Children[0];

    private static ScrollViewer Feed(CommandLine line) =>
        (ScrollViewer)((Grid)Frame(line).Child).Children[0];

    [Fact]
    public void The_prompt_is_echoed_and_its_answer_lands_on_the_same_line()
    {
        RunOnSTA(() =>
        {
            var line = Hosted();

            line.Text = "LINE";
            line.Submit();

            // The question is part of the transcript, not a label hidden beside the input.
            Assert.Contains(line.Messages, message => message.Text == "Specify first point:");

            line.Text = "0,0";
            line.Submit();

            Assert.Contains(line.Messages, message => message.Text == "Specify first point: 0,0");

            // One line for the exchange, not a question and an orphaned answer under it.
            Assert.DoesNotContain(line.Messages, message => message.Text == "0,0");
        });
    }

    [Fact]
    public void An_empty_answer_ends_the_command_without_tacking_a_space_onto_the_question()
    {
        RunOnSTA(() =>
        {
            var line = Hosted();

            line.BeginPrompt("Specify next point", new[] { "Undo", "Close" });

            // Enter on a prompt is how a CAD user ends a polyline: the question stays as printed.
            line.Submit();

            Assert.Equal(new[] { "Specify next point or [Undo/Close]:" }, line.Messages.Select(message => message.Text));
        });
    }

    [Fact]
    public void A_command_with_two_steps_writes_two_questions()
    {
        RunOnSTA(() =>
        {
            var line = Hosted();

            line.Text = "LINE";
            line.Submit();

            line.Text = "0,0";
            line.Submit();

            line.BeginPrompt("Specify next point", new[] { "Undo", "Close" });
            line.Text = "4000,0";
            line.Submit();

            Assert.Equal(
                new[] { "> LINE", "Specify first point: 0,0", "Specify next point or [Undo/Close]: 4000,0" },
                line.Messages.Select(message => message.Text));
        });
    }

    [Fact]
    public void The_prompt_label_carries_the_keywords_but_the_property_stays_raw()
    {
        RunOnSTA(() =>
        {
            var line = Hosted();

            line.BeginPrompt("Specify next point", new[] { "Undo", "Close" });

            Assert.Equal("Specify next point", line.Prompt);
            Assert.Equal("Specify next point or [Undo/Close]:", line.PromptText);
        });
    }

    [Theory]
    [InlineData("U", "Undo")]
    [InlineData("undo", "Undo")]
    [InlineData("C", "Close")]
    [InlineData("Cl", "Close")]
    [InlineData("4000,0", null)]
    [InlineData("", null)]
    public void An_option_is_picked_by_its_capital_or_its_name(string typed, string? expected)
    {
        RunOnSTA(() =>
        {
            var line = Hosted();
            CommandPromptEventArgs? answered = null;

            line.PromptCompleted += (_, e) => answered = e;

            line.BeginPrompt("Specify next point", new[] { "Undo", "Close" });
            line.Text = typed;
            line.Submit();

            Assert.NotNull(answered);
            Assert.Equal(expected, answered!.Option);
        });
    }

    [Fact]
    public void A_beginning_that_fits_two_keywords_is_not_an_answer()
    {
        RunOnSTA(() =>
        {
            // Both keywords share the capital U and both begin with "Un", so neither spelling can
            // say which one was meant.
            Assert.Null(CommandLine.MatchOption(new[] { "Undo", "Union" }, "U"));
            Assert.Null(CommandLine.MatchOption(new[] { "Undo", "Union" }, "Un"));
            Assert.Equal("Union", CommandLine.MatchOption(new[] { "Undo", "Union" }, "Uni"));
            Assert.Equal("Undo", CommandLine.MatchOption(new[] { "Undo", "Union" }, "Und"));
        });
    }

    [Fact]
    public void The_echo_can_be_switched_off_for_a_read_only_log()
    {
        RunOnSTA(() =>
        {
            var line = Hosted(bar => bar.EchoPrompt = false);

            line.Text = "LINE";
            line.Submit();

            Assert.DoesNotContain(line.Messages, message => message.Text.StartsWith("Specify"));
        });
    }

    [Fact]
    public void The_answer_is_written_back_into_the_message()
    {
        RunOnSTA(() =>
        {
            var line = Hosted();

            line.Text = "LINE";
            line.Submit();

            var prompt = line.Messages.Last();

            line.Text = "12,34";
            line.Submit();

            // Hosts read Messages, not the visual tree, so the model has to grow with the line.
            Assert.Equal("Specify first point: 12,34", prompt.Text);
        });
    }

    [Fact]
    public void Docked_draws_one_rule_along_its_top_edge()
    {
        RunOnSTA(() =>
        {
            var frame = Frame(Hosted(line => line.Variant = CommandLineVariant.Docked));

            Assert.Equal(new Thickness(0, 1, 0, 0), frame.BorderThickness);
            Assert.NotNull(frame.BorderBrush);

            // The sides and the bottom belong to the window in a CAD shell.
            Assert.Equal(0, VisualElement.GetElevation(frame));
            Assert.Equal(0xFF, ((SolidColorBrush)frame.Background).Color.A);
        });
    }

    [Fact]
    public void Floating_draws_a_rounded_plate_with_a_shadow()
    {
        RunOnSTA(() =>
        {
            var frame = Frame(Hosted(line =>
            {
                line.Variant = CommandLineVariant.Floating;
                line.CornerRadius = new CornerRadius(10);
                line.Elevation = 3;
            }));

            Assert.Equal(new Thickness(1), frame.BorderThickness);
            Assert.Equal(new CornerRadius(10), frame.CornerRadius);
            Assert.Equal(3, VisualElement.GetElevation(frame));
        });
    }

    [Fact]
    public void Inline_paints_nothing_of_its_own()
    {
        RunOnSTA(() =>
        {
            var frame = Frame(Hosted());

            Assert.Equal(new Thickness(0), frame.BorderThickness);
            Assert.Equal(0, VisualElement.GetElevation(frame));
            Assert.Equal(0, ((SolidColorBrush)frame.Background).Color.A);
        });
    }

    [Fact]
    public void F2_grows_the_history_and_folds_it_again()
    {
        RunOnSTA(() =>
        {
            var line = Hosted(bar =>
            {
                bar.HistoryHeight = 96;
                bar.ExpandedHistoryHeight = 240;
            });

            Assert.Equal(96, Feed(line).MaxHeight);

            line.ToggleHistory();

            Assert.True(line.IsHistoryExpanded);
            Assert.Equal(240, Feed(line).MaxHeight);

            line.ToggleHistory();

            Assert.False(line.IsHistoryExpanded);
            Assert.Equal(96, Feed(line).MaxHeight);
        });
    }

    [Fact]
    public void Space_is_enter_by_default()
    {
        RunOnSTA(() =>
        {
            // A CAD command line takes space for Enter; a host whose prompt answers contain a space
            // can switch it off, which is why it is a property and not a constant.
            Assert.True(new CommandLine().SpaceSubmits);
        });
    }

    [Fact]
    public void The_suggestion_list_names_the_alias_and_the_description()
    {
        RunOnSTA(() =>
        {
            var line = Hosted(bar => bar.AddCommand(
                new CommandDefinition("LINEAR", "Linear dimension").WithAliases("LIA")));

            line.Text = "ZO";

            Assert.Single(line.Suggestions);
            Assert.Equal("ZOOM", line.Suggestions[0].Name);
            Assert.Equal("ZOOM (Z)   Zoom the view", line.Suggestions[0].Display);
        });
    }

    [Fact]
    public void The_first_suggestion_is_what_enter_would_run()
    {
        RunOnSTA(() =>
        {
            var line = Hosted(bar => bar.AddCommand(
                new CommandDefinition("AREA", "Measure an area").WithAliases("QUERY")));

            line.Text = "Q";

            // Enter resolves QUERY exactly, so the list may not offer something else first.
            Assert.Same(line.Resolve("Q"), line.Suggestions[0]);
        });
    }

    [Fact]
    public void The_history_keeps_its_limit_in_the_model_and_the_view()
    {
        RunOnSTA(() =>
        {
            var line = Hosted(bar => bar.MaxHistory = 3);

            for (int i = 0; i < 6; i++) line.WriteLine($"line {i}");

            Assert.Equal(3, line.Messages.Count);
            Assert.Equal(new[] { "line 3", "line 4", "line 5" }, line.Messages.Select(message => message.Text));

            // The rows in the feed follow the messages; the oldest three left both.
            Assert.Equal(3, ((StackPanel)Feed(line).Content).Children.Count);
        });
    }

    [Fact]
    public void A_prompt_no_definition_asked_for_still_reads_as_one()
    {
        RunOnSTA(() =>
        {
            var line = Hosted();

            line.BeginPrompt("Select objects", new[] { "Remove", "Undo", "All" });

            Assert.True(line.IsPromptActive);
            Assert.Equal("Select objects or [Remove/Undo/All]:", line.PromptText);

            line.CancelPrompt();

            Assert.False(line.IsPromptActive);
            Assert.Equal("Command:", line.PromptText);
        });
    }
}
