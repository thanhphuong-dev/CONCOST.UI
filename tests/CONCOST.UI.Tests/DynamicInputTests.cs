using System;
using System.Windows;
using CONCOST.UI.Controls;
using Xunit;

namespace CONCOST.UI.Tests;

/// <summary>
/// The dynamic input box: the parser behind it and the open/commit/cancel cycle a drawing command
/// drives.
/// </summary>
public class DynamicInputTests
{
    private static void RunOnSTA(Action action) => WpfApplicationHost.Run(action);

    [Theory]
    [InlineData("3200", 3200)]
    [InlineData("3.2m", 3200)]
    [InlineData("320cm", 3200)]
    [InlineData("3'-6\"", 1066.8)]
    public void TryParse_Distance_ReadsDrawingUnits(string text, double millimetres)
    {
        Assert.True(DynamicInput.TryParse(DynamicInputKind.Distance, text, out double number, out _));
        Assert.Equal(millimetres, number, 2);
    }

    [Fact]
    public void TryParse_Angle_DropsTheDegreeSign()
    {
        Assert.True(DynamicInput.TryParse(DynamicInputKind.Angle, "45°", out double degrees, out _));
        Assert.Equal(45, degrees, 3);
    }

    [Fact]
    public void TryParse_Coordinate_ReturnsThePoint()
    {
        Assert.True(DynamicInput.TryParse(DynamicInputKind.Coordinate, "1000,500", out _, out var point));
        Assert.Equal(1000, point.X, 3);
        Assert.Equal(500, point.Y, 3);
    }

    [Fact]
    public void TryParse_Text_RejectsAnEmptyAnswerButTakesWords()
    {
        Assert.False(DynamicInput.TryParse(DynamicInputKind.Text, "   ", out _, out _));
        Assert.True(DynamicInput.TryParse(DynamicInputKind.Text, "Dam D1", out _, out _));
    }

    [Fact]
    public void TryParse_Garbage_IsRefused() =>
        Assert.False(DynamicInput.TryParse(DynamicInputKind.Distance, "abc", out _, out _));

    [Fact]
    public void Open_PlacesTheBoxNextToTheAnchor() => RunOnSTA(() =>
    {
        var input = new DynamicInput { Offset = 20 };

        input.Open(new Point(140, 90));

        Assert.True(input.IsOpen);
        Assert.Equal(Visibility.Visible, input.Visibility);
        Assert.Equal(new Thickness(160, 110, 0, 0), input.Margin);
    });

    [Fact]
    public void MoveTo_RepositionsWithoutClearingTheText() => RunOnSTA(() =>
    {
        var input = new DynamicInput { Offset = 10 };

        input.Open(new Point(10, 10));
        input.Text = "1200";
        input.MoveTo(new Point(60, 40));

        Assert.Equal("1200", input.Text);
        Assert.Equal(new Thickness(70, 50, 0, 0), input.Margin);
    });

    [Fact]
    public void Commit_ReportsTheParsedValueAndCloses() => RunOnSTA(() =>
    {
        var input = new DynamicInput { Kind = DynamicInputKind.Distance };
        DynamicInputValueEventArgs? seen = null;

        input.ValueCommitted += (_, e) => seen = e;
        input.Open(new Point(0, 0));
        input.Text = "3.2m";

        Assert.True(input.Commit());

        Assert.NotNull(seen);
        Assert.Equal(DynamicInputKind.Distance, seen!.Kind);
        Assert.Equal(3200, seen.Number, 2);
        Assert.Equal("3.2m", seen.Text);
        Assert.False(input.IsOpen);
    });

    [Fact]
    public void Commit_Coordinate_CarriesThePoint() => RunOnSTA(() =>
    {
        var input = new DynamicInput { Kind = DynamicInputKind.Coordinate };
        DynamicInputValueEventArgs? seen = null;

        input.ValueCommitted += (_, e) => seen = e;
        input.Open(new Point(0, 0));
        input.Text = "1000,500,250";

        Assert.True(input.Commit());
        Assert.NotNull(seen!.Point);
        Assert.Equal(250, seen.Point!.Value.Z, 3);
    });

    [Fact]
    public void Commit_UnreadableText_KeepsTheBoxOpenAndExplainsWhy() => RunOnSTA(() =>
    {
        var input = new DynamicInput { Kind = DynamicInputKind.Distance };
        bool raised = false;

        input.ValueCommitted += (_, _) => raised = true;
        input.Open(new Point(0, 0));
        input.Text = "abc";

        Assert.False(input.Commit());

        Assert.False(raised);
        Assert.True(input.IsOpen);
        Assert.False(input.IsValid);
        Assert.Contains("abc", input.ValidationMessage);
    });

    [Fact]
    public void TypingAfterAnError_ClearsTheMessage() => RunOnSTA(() =>
    {
        var input = new DynamicInput();

        input.Open(new Point(0, 0));
        input.Text = "abc";
        input.Commit();

        Assert.False(input.IsValid);

        input.Text = "1200";

        Assert.True(input.IsValid);
        Assert.Equal(string.Empty, input.ValidationMessage);
    });

    [Fact]
    public void Cancel_ClearsAndRaisesOnce() => RunOnSTA(() =>
    {
        var input = new DynamicInput();
        int cancelled = 0;

        input.Cancelled += (_, _) => cancelled++;
        input.Open(new Point(0, 0));
        input.Text = "1200";
        input.Cancel();

        Assert.Equal(1, cancelled);
        Assert.Equal(string.Empty, input.Text);
        Assert.False(input.IsOpen);

        // Cancelling an already closed box is not an event.
        input.Cancel();
        Assert.Equal(1, cancelled);
    });

    [Fact]
    public void Prompt_ShowsTheUnitForTheKind() => RunOnSTA(() =>
    {
        var input = new DynamicInput { Prompt = "Chieu dai", Kind = DynamicInputKind.Distance };

        Assert.True(RenderedText.Contains(input, "Chieu dai"));
        Assert.True(RenderedText.Contains(input, "(mm)"));

        input.Kind = DynamicInputKind.Angle;

        Assert.True(RenderedText.Contains(input, "(°)"));
    });

    /// <summary>Reads every <see cref="System.Windows.Controls.TextBlock"/> inside a control's tree.</summary>
    private static class RenderedText
    {
        public static bool Contains(DependencyObject root, string fragment)
        {
            if (root is System.Windows.Controls.TextBlock block && block.Text.Contains(fragment, StringComparison.Ordinal))
                return true;

            for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(root); i++)
            {
                if (Contains(System.Windows.Media.VisualTreeHelper.GetChild(root, i), fragment)) return true;
            }

            return false;
        }
    }
}
