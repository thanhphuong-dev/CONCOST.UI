using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using CONCOST.UI.Controls;
using Xunit;

namespace CONCOST.UI.Tests;

/// <summary>
/// The §107 controls: the waterfall panel, the signature pad, the tag box and the wizard.
/// </summary>
public class AdvancedControlsTests
{
    private static void RunOnSTA(Action action) => WpfApplicationHost.Run(action);

    private static Border Box(double height) => new() { Height = height, Child = new TextBlock { Text = "ô" } };

    // ---------------------------------------------------------------- MasonryPanel

    [Fact]
    public void MasonryPanel_SecondCardGoesToTheOtherColumn()
    {
        RunOnSTA(() =>
        {
            var panel = new MasonryPanel { Columns = 2, ColumnSpacing = 10, RowSpacing = 10 };
            var first = Box(100);
            var second = Box(100);
            panel.Children.Add(first);
            panel.Children.Add(second);

            panel.Measure(new Size(410, double.PositiveInfinity));
            panel.Arrange(new Rect(0, 0, 410, panel.DesiredSize.Height));

            // Two columns of 200 inside 410: the second card is beside the first, not below it.
            Assert.Equal(0, VisualTreeHelper.GetOffset(first).X, 1);
            Assert.Equal(210, VisualTreeHelper.GetOffset(second).X, 1);
            Assert.Equal(0, VisualTreeHelper.GetOffset(second).Y, 1);
        });
    }

    [Fact]
    public void MasonryPanel_ThirdCardGoesUnderTheShorterColumn()
    {
        RunOnSTA(() =>
        {
            var panel = new MasonryPanel { Columns = 2, ColumnSpacing = 0, RowSpacing = 10 };
            var tall = Box(200);
            var shortOne = Box(50);
            var third = Box(50);
            panel.Children.Add(tall);
            panel.Children.Add(shortOne);
            panel.Children.Add(third);

            panel.Measure(new Size(200, double.PositiveInfinity));
            panel.Arrange(new Rect(0, 0, 200, panel.DesiredSize.Height));

            // Column 1 is at y=50 after the short card, so the third card stacks under it.
            Assert.Equal(100, VisualTreeHelper.GetOffset(shortOne).X, 1);
            Assert.Equal(100, VisualTreeHelper.GetOffset(third).X, 1);
            Assert.Equal(60, VisualTreeHelper.GetOffset(third).Y, 1);
        });
    }

    [Fact]
    public void MasonryPanel_InReadingOrderFillsEachColumnFirst()
    {
        RunOnSTA(() =>
        {
            var panel = new MasonryPanel { Columns = 2, IsBalanced = false, RowSpacing = 0, ColumnSpacing = 0 };
            var cards = new[] { Box(40), Box(40), Box(40), Box(40) };
            foreach (Border card in cards) panel.Children.Add(card);

            panel.Measure(new Size(200, double.PositiveInfinity));
            panel.Arrange(new Rect(0, 0, 200, panel.DesiredSize.Height));

            // Reading order: the first two cards fill column 0, then the next two start column 1.
            Assert.Equal(0, VisualTreeHelper.GetOffset(cards[1]).X, 1);
            Assert.Equal(40, VisualTreeHelper.GetOffset(cards[1]).Y, 1);
            Assert.Equal(100, VisualTreeHelper.GetOffset(cards[2]).X, 1);
            Assert.Equal(0, VisualTreeHelper.GetOffset(cards[2]).Y, 1);
        });
    }

    [Theory]
    [InlineData(500, 200, 10, 2)]
    [InlineData(200, 200, 10, 1)]
    [InlineData(1000, 200, 0, 5)]
    public void MasonryPanel_CountsColumnsFromTheAvailableWidth(double width, double minColumnWidth,
        double spacing, int expected)
    {
        RunOnSTA(() =>
        {
            var panel = new MasonryPanel { MinColumnWidth = minColumnWidth, ColumnSpacing = spacing };
            Assert.Equal(expected, panel.GetColumnCount(width));
        });
    }

    [Fact]
    public void MasonryPanel_FixedColumnsWinOverTheWidth()
    {
        RunOnSTA(() =>
        {
            var panel = new MasonryPanel { Columns = 3, MinColumnWidth = 200 };
            Assert.Equal(3, panel.GetColumnCount(200));
            Assert.Equal((200 - 24) / 3.0, panel.GetColumnWidth(200), 1);
        });
    }

    // ---------------------------------------------------------------- SignaturePad

    private static Stroke MakeStroke()
    {
        var points = new StylusPointCollection(new List<Point> { new(10, 60), new(40, 20), new(80, 70) });
        return new Stroke(points);
    }

    [Fact]
    public void SignaturePad_AddingAStrokeTurnsOnHasSignatureAndTheImage()
    {
        RunOnSTA(() =>
        {
            var pad = new SignaturePad();
            int changes = 0;
            pad.SignatureChanged += (_, _) => changes++;

            Assert.Null(pad.SignatureImage);

            pad.Strokes.Add(MakeStroke());

            Assert.True(pad.HasSignature);
            Assert.NotNull(pad.SignatureImage);
            Assert.Equal(1, changes);
        });
    }

    [Fact]
    public void SignaturePad_ClearEmptiesTheStrokesAndTheImage()
    {
        RunOnSTA(() =>
        {
            var pad = new SignaturePad();
            pad.Strokes.Add(MakeStroke());
            pad.Strokes.Add(MakeStroke());

            pad.Clear();

            Assert.False(pad.HasSignature);
            Assert.Null(pad.SignatureImage);
            Assert.Empty(pad.Strokes);
        });
    }

    [Fact]
    public void SignaturePad_UndoRemovesTheLastStrokeOnly()
    {
        RunOnSTA(() =>
        {
            var pad = new SignaturePad();
            Stroke first = MakeStroke();
            pad.Strokes.Add(first);
            pad.Strokes.Add(MakeStroke());

            Assert.True(pad.Undo());
            Assert.Single(pad.Strokes);
            Assert.Same(first, pad.Strokes[0]);

            Assert.True(pad.Undo());
            Assert.False(pad.Undo());
        });
    }

    [Fact]
    public void SignaturePad_ExportUsesTheRequestedSize()
    {
        RunOnSTA(() =>
        {
            var pad = new SignaturePad();
            pad.Strokes.Add(MakeStroke());

            var image = pad.Export(1200, 400, Brushes.White);

            Assert.Equal(1200, image.PixelWidth);
            Assert.Equal(400, image.PixelHeight);
            Assert.True(image.IsFrozen);
        });
    }

    [Fact]
    public void SignaturePad_RecoloursStrokesAlreadyDrawn()
    {
        RunOnSTA(() =>
        {
            var pad = new SignaturePad();
            pad.Strokes.Add(MakeStroke());

            pad.StrokeBrush = Brushes.Red;

            Assert.Equal(Colors.Red, pad.Strokes[0].DrawingAttributes.Color);
        });
    }

    // ---------------------------------------------------------------- TagInput

    [Fact]
    public void TagInput_CommitTextSplitsOnTheDelimiter()
    {
        RunOnSTA(() =>
        {
            var input = new TagInput { Text = " bê tông , thép ,, cốp pha " };

            int added = input.CommitText();

            Assert.Equal(3, added);
            Assert.Equal(new[] { "bê tông", "thép", "cốp pha" }, input.Tags.Select(t => t.Text).ToArray());
            Assert.Equal(string.Empty, input.Text);
        });
    }

    [Fact]
    public void TagInput_RefusesDuplicatesIgnoringAccentsAndCase()
    {
        RunOnSTA(() =>
        {
            var input = new TagInput();

            Assert.True(input.AddTag("Kết cấu"));
            Assert.False(input.AddTag("ket cau"));
            Assert.False(input.AddTag("KẾT CẤU"));
            Assert.Single(input.Tags);
        });
    }

    [Fact]
    public void TagInput_RefusesFreeTextWhenItIsTurnedOff()
    {
        RunOnSTA(() =>
        {
            var input = new TagInput { AllowFreeText = false, SuggestionSource = new[] { "Kết cấu" } };

            Assert.False(input.AddTag("tự do"));
            Assert.True(input.AddTag("Kết cấu"));
        });
    }

    [Fact]
    public void TagInput_StopsAtMaxTags()
    {
        RunOnSTA(() =>
        {
            var input = new TagInput { MaxTags = 2 };

            Assert.True(input.AddTag("một"));
            Assert.True(input.AddTag("hai"));
            Assert.False(input.AddTag("ba"));
            Assert.Equal(2, input.Tags.Count);
        });
    }

    [Fact]
    public void TagInput_EditsTheBoundCollectionBothWays()
    {
        RunOnSTA(() =>
        {
            var source = new ObservableCollection<string> { "có sẵn" };
            var input = new TagInput { ItemsSource = source };

            Assert.Single(input.Tags);

            input.AddTag("thêm");
            Assert.Contains("thêm", source);
            Assert.Equal(2, input.Tags.Count);

            input.RemoveTagCommand.Execute(input.Tags[0]);
            Assert.DoesNotContain("có sẵn", source);
            Assert.Single(input.Tags);
        });
    }

    [Fact]
    public void TagInput_SuggestionsFilterWithoutAccents()
    {
        RunOnSTA(() =>
        {
            var input = new TagInput
            {
                SuggestionSource = new[] { "Kết cấu", "Kiến trúc", "Cấp phối" }
            };

            input.Text = "ket";

            Assert.True(input.IsSuggestionOpen);
            Assert.Single(input.Suggestions);
            Assert.Equal("Kết cấu", input.Suggestions[0].Text);

            input.Text = string.Empty;
            Assert.False(input.IsSuggestionOpen);
        });
    }

    [Fact]
    public void TagInput_ReadsCaptionsFromDisplayPath()
    {
        RunOnSTA(() =>
        {
            var input = new TagInput
            {
                ItemsSource = new[] { new Material { Name = "Bê tông M300" } },
                DisplayPath = nameof(Material.Name)
            };

            Assert.Equal("Bê tông M300", input.Tags[0].Text);
        });
    }

    [Fact]
    public void TagInput_ClearTagsRemovesEveryChip()
    {
        RunOnSTA(() =>
        {
            var input = new TagInput();
            input.AddTag("một");
            input.AddTag("hai");

            var removed = new List<string>();
            input.TagsChanged += (_, e) => removed.Add(e.Text);

            input.ClearTags();

            Assert.Empty(input.Tags);
            Assert.Equal(new[] { "một", "hai" }, removed);
        });
    }

    private class Material
    {
        public string Name { get; set; } = string.Empty;
    }

    // ---------------------------------------------------------------- Wizard

    private static Wizard MakeWizard(int steps = 3)
    {
        var wizard = new Wizard();

        for (int i = 0; i < steps; i++)
        {
            wizard.Steps.Add(new WizardStep
            {
                Header = $"Bước {i + 1}",
                Description = i == 0 ? "Bắt đầu" : string.Empty,
                Content = new TextBlock { Text = $"nội dung {i + 1}" }
            });
        }

        return wizard;
    }

    [Fact]
    public void Wizard_StartsOnTheFirstStepAndNumbersIt()
    {
        RunOnSTA(() =>
        {
            Wizard wizard = MakeWizard();

            Assert.Equal(0, wizard.CurrentIndex);
            Assert.True(wizard.Steps[0].IsCurrent);
            Assert.Equal(1, wizard.Steps[0].Number);
            Assert.Equal(3, wizard.Steps[2].Number);
            Assert.False(wizard.CanGoBack);
            Assert.True(wizard.CanGoNext);
            Assert.False(wizard.IsLastStep);
        });
    }

    [Fact]
    public void Wizard_MarksStepsCompletedAsItMovesOn()
    {
        RunOnSTA(() =>
        {
            Wizard wizard = MakeWizard();

            Assert.True(wizard.Next());

            Assert.True(wizard.Steps[0].IsCompleted);
            Assert.False(wizard.Steps[0].IsCurrent);
            Assert.True(wizard.Steps[1].IsCurrent);
            Assert.True(wizard.CanGoBack);
        });
    }

    [Fact]
    public void Wizard_RefusesToAdvancePastAnInvalidStep()
    {
        RunOnSTA(() =>
        {
            Wizard wizard = MakeWizard();
            bool navigated = false;
            wizard.Navigating += (_, _) => navigated = true;

            wizard.Steps[0].IsValid = false;

            Assert.False(wizard.CanGoNext);
            Assert.False(wizard.Next());
            Assert.False(navigated);
            Assert.Equal(0, wizard.CurrentIndex);
        });
    }

    [Fact]
    public void Wizard_NavigatingCanCancelTheMove()
    {
        RunOnSTA(() =>
        {
            Wizard wizard = MakeWizard();

            wizard.Navigating += (_, e) =>
            {
                if (e.ToIndex == 1) e.Cancel = true;
            };

            Assert.False(wizard.Next());
            Assert.Equal(0, wizard.CurrentIndex);

            // Bypassing step 1 by jumping straight to the end is still free to succeed.
            Assert.True(wizard.GoTo(2));
        });
    }

    [Fact]
    public void Wizard_FinishOnlyFiresOnTheLastStep()
    {
        RunOnSTA(() =>
        {
            Wizard wizard = MakeWizard(2);
            int finished = 0;
            wizard.Finished += (_, _) => finished++;

            wizard.Finish();
            Assert.Equal(0, finished);

            Assert.True(wizard.GoTo(1));
            Assert.True(wizard.IsLastStep);
            wizard.Finish();
            Assert.Equal(1, finished);
        });
    }

    [Fact]
    public void Wizard_StepChangedFollowsTheNavigation()
    {
        RunOnSTA(() =>
        {
            Wizard wizard = MakeWizard();
            var visited = new List<int>();
            wizard.StepChanged += (_, _) => visited.Add(wizard.CurrentIndex);

            wizard.Next();
            wizard.Next();
            wizard.Back();

            Assert.Equal(new[] { 1, 2, 1 }, visited);
        });
    }

    [Fact]
    public void Wizard_TemplateDisablesNextWhileTheStepIsInvalid()
    {
        RunOnSTA(() =>
        {
            Wizard wizard = MakeWizard(2);
            var host = new Border { Child = wizard, Width = 420, Height = 320 };
            host.Measure(new Size(420, 320));
            host.Arrange(new Rect(0, 0, 420, 320));
            host.UpdateLayout();

            var next = wizard.Template.FindName("PART_Next", wizard) as Button;
            var back = wizard.Template.FindName("PART_Back", wizard) as Button;

            Assert.NotNull(next);
            Assert.NotNull(back);
            Assert.True(next!.IsEnabled);
            Assert.False(back!.IsEnabled);

            wizard.Steps[0].IsValid = false;
            Assert.False(next.IsEnabled);

            wizard.Steps[0].IsValid = true;
            Assert.True(next.IsEnabled);
        });
    }

    [Fact]
    public void Wizard_RemovingTheLastStepMovesBackToAKnownOne()
    {
        RunOnSTA(() =>
        {
            Wizard wizard = MakeWizard(3);
            wizard.GoTo(2);

            wizard.Steps.RemoveAt(2);

            Assert.Equal(1, wizard.CurrentIndex);
            Assert.True(wizard.IsLastStep);
        });
    }
}
