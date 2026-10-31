using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CONCOST.UI.Controls;
using CONCOST.UI.Core;
using CONCOST.UI.Core.MVVM;
using Xunit;

namespace CONCOST.UI.Tests;

/// <summary>
/// The phase-3 controls: <see cref="RadialGauge"/>, <see cref="ParticleField"/>,
/// <see cref="PageTransitionHost"/> and <see cref="CommandPalette"/>.
/// </summary>
public class Phase3ControlsTests
{
    private static void RunOnSTA(Action action) => WpfApplicationHost.Run(action);

    private static T Arrange<T>(T element, double width = 400, double height = 300) where T : FrameworkElement
    {
        var host = new Border { Child = element };
        host.Measure(new Size(width, height));
        host.Arrange(new Rect(0, 0, width, height));
        host.UpdateLayout();
        return element;
    }

    // ------------------------------------------------------------------ RadialGauge

    [Fact]
    public void RadialGauge_Defaults_MatchSpecifications()
    {
        RunOnSTA(() =>
        {
            var gauge = new RadialGauge();

            Assert.Equal(0, gauge.Minimum);
            Assert.Equal(100, gauge.Maximum);
            Assert.Equal(0, gauge.Value);
            Assert.Equal(135, gauge.StartAngle);
            Assert.Equal(270, gauge.SweepAngle);
            Assert.Equal(12, gauge.TrackThickness);
            Assert.True(gauge.ShowValue);
            Assert.False(gauge.ShowTicks);
            Assert.Equal(10, gauge.TickCount);
            Assert.Equal("0", gauge.ValueFormat);
            Assert.Equal(string.Empty, gauge.Suffix);
            Assert.True(gauge.IsAnimated);
        });
    }

    [Theory]
    [InlineData(0, 0.0)]
    [InlineData(25, 0.25)]
    [InlineData(50, 0.5)]
    [InlineData(100, 1.0)]
    public void RadialGauge_NormalizedValue_MapsRangeToUnitInterval(double value, double expected)
    {
        RunOnSTA(() =>
        {
            var gauge = new RadialGauge { Value = value };
            Assert.Equal(expected, gauge.NormalizedValue, 6);
        });
    }

    [Fact]
    public void RadialGauge_ValueOutsideRange_IsClamped()
    {
        RunOnSTA(() =>
        {
            var gauge = new RadialGauge { Value = 150 };
            Assert.Equal(1.0, gauge.NormalizedValue, 6);

            gauge.Value = -40;
            Assert.Equal(0.0, gauge.NormalizedValue, 6);
        });
    }

    [Fact]
    public void RadialGauge_CustomRange_IsHonoured()
    {
        RunOnSTA(() =>
        {
            var gauge = new RadialGauge { Minimum = 200, Maximum = 400, Value = 300 };
            Assert.Equal(0.5, gauge.NormalizedValue, 6);
        });
    }

    [Fact]
    public void RadialGauge_ReversedRange_DoesNotThrow()
    {
        RunOnSTA(() =>
        {
            var gauge = new RadialGauge { Minimum = 100, Maximum = 0, Value = 50 };
            double normalized = gauge.NormalizedValue;

            Assert.False(double.IsNaN(normalized));
            Assert.InRange(normalized, 0, 1);
        });
    }

    [Fact]
    public void RadialGauge_ZeroWidthRange_ReportsZero()
    {
        RunOnSTA(() =>
        {
            var gauge = new RadialGauge { Minimum = 50, Maximum = 50, Value = 50 };
            Assert.Equal(0, gauge.NormalizedValue);
        });
    }

    // ------------------------------------------------------------------ ParticleField

    [Fact]
    public void ParticleField_Defaults_MatchSpecifications()
    {
        RunOnSTA(() =>
        {
            var field = new ParticleField();

            Assert.Equal(60, field.ParticleCount);
            Assert.Equal(1.0, field.MinRadius);
            Assert.Equal(2.6, field.MaxRadius);
            Assert.Equal(18.0, field.Speed);
            Assert.Equal(270.0, field.Direction);
            Assert.Equal(0.0, field.ConnectDistance);
            Assert.True(field.IsAnimated);
            Assert.False(field.IsHitTestVisible);
        });
    }

    [Fact]
    public void ParticleField_BuildsOneParticlePerRequestedSlot()
    {
        RunOnSTA(() =>
        {
            var field = Arrange(new ParticleField { ParticleCount = 24 }, 320, 200);

            Assert.Equal(24, field.ActiveParticleCount);
        });
    }

    [Fact]
    public void ParticleField_ChangingCount_RebuildsTheField()
    {
        RunOnSTA(() =>
        {
            var field = Arrange(new ParticleField { ParticleCount = 10 }, 320, 200);
            Assert.Equal(10, field.ActiveParticleCount);

            field.ParticleCount = 35;
            Assert.Equal(35, field.ActiveParticleCount);
        });
    }

    [Fact]
    public void ParticleField_ZeroCount_DrawsNothingAndDoesNotThrow()
    {
        RunOnSTA(() =>
        {
            var field = Arrange(new ParticleField { ParticleCount = 0 }, 320, 200);
            Assert.Equal(0, field.ActiveParticleCount);
        });
    }

    // ------------------------------------------------------------------ PageTransitionHost

    [Fact]
    public void PageTransitionHost_Defaults_MatchSpecifications()
    {
        RunOnSTA(() =>
        {
            var host = new PageTransitionHost();

            Assert.Equal(PageTransition.Fade, host.Transition);
            Assert.Equal(TimeSpan.FromMilliseconds(260), host.Duration);
            Assert.True(host.IsTransitionEnabled);
            Assert.Equal(22.0, host.Offset);
            Assert.True(host.ClipToBounds);
        });
    }

    [Fact]
    public void PageTransitionHost_SwappingContent_LeavesTheNewContentVisible()
    {
        RunOnSTA(() =>
        {
            var host = Arrange(new PageTransitionHost { Transition = PageTransition.None }, 300, 200);

            host.Content = new TextBlock { Text = "first" };
            host.Content = new TextBlock { Text = "second" };

            Assert.IsType<TextBlock>(host.Content);
            Assert.Equal("second", ((TextBlock)host.Content).Text);
        });
    }

    [Fact]
    public void PageTransitionHost_WithMotionOff_StaysAtFullOpacity()
    {
        RunOnSTA(() =>
        {
            bool previous = MotionSettings.ForceDisabled;
            MotionSettings.ForceDisabled = true;

            try
            {
                var host = Arrange(new PageTransitionHost { Transition = PageTransition.SlideLeft }, 300, 200);
                host.Content = new TextBlock { Text = "page" };

                Assert.Equal(1.0, host.Opacity);
                Assert.False(host.IsTransitioning);
            }
            finally
            {
                MotionSettings.ForceDisabled = previous;
            }
        });
    }

    [Fact]
    public void PageTransitionHost_DisabledTransitions_DoNotAnimate()
    {
        RunOnSTA(() =>
        {
            var host = Arrange(new PageTransitionHost { IsTransitionEnabled = false }, 300, 200);
            host.Content = new TextBlock { Text = "page" };

            Assert.Equal(1.0, host.Opacity);
            Assert.False(host.IsTransitioning);
        });
    }

    [Fact]
    public void PageTransitionHost_PlayWithNoTransition_ResetsInsteadOfAnimating()
    {
        RunOnSTA(() =>
        {
            var host = Arrange(new PageTransitionHost { Transition = PageTransition.None }, 300, 200);
            host.Content = new TextBlock { Text = "page" };

            host.Play();

            Assert.Equal(1.0, host.Opacity);
            Assert.False(host.IsTransitioning);
        });
    }

    // ------------------------------------------------------------------ search helpers

    [Theory]
    [InlineData("Tổng quan", "tong quan")]
    [InlineData("Đường kính", "duong kinh")]
    [InlineData("Cấu hình thép", "cau hinh thep")]
    [InlineData("XUẤT PDF", "xuat pdf")]
    public void Normalize_StripsVietnameseDiacritics(string input, string expected)
    {
        Assert.Equal(expected, CommandPaletteText.Normalize(input));
    }

    [Fact]
    public void Tokenize_SplitsOnWhitespaceAndDropsEmptyParts()
    {
        IReadOnlyList<string> tokens = CommandPaletteText.Tokenize("  xuat   PDF  ");

        Assert.Equal(2, tokens.Count);
        Assert.Equal("xuat", tokens[0]);
        Assert.Equal("pdf", tokens[1]);
    }

    [Fact]
    public void Matches_RequiresEveryToken()
    {
        var item = new CommandPaletteItem("Xuất PDF", "Xuất bản vẽ ra PDF", category: "Xuất bản");

        Assert.True(CommandPaletteText.Matches(item, CommandPaletteText.Tokenize("xuat pdf")));
        Assert.True(CommandPaletteText.Matches(item, CommandPaletteText.Tokenize("xuat ban")));
        Assert.False(CommandPaletteText.Matches(item, CommandPaletteText.Tokenize("xuat excel")));
        Assert.True(CommandPaletteText.Matches(item, CommandPaletteText.Tokenize("")));
    }

    // ------------------------------------------------------------------ CommandPalette

    private static List<CommandPaletteItem> SampleCommands(int[]? hits = null)
    {
        return new List<CommandPaletteItem>
        {
            new("Tổng quan dự án", "Bảng điều khiển KPI", new RelayCommand(() => Count(hits, 0)), shortcut: "Ctrl+1", category: "Điều hướng"),
            new("Xuất PDF", "Xuất bản vẽ ra PDF", new RelayCommand(() => Count(hits, 1)), shortcut: "Ctrl+P", category: "Xuất bản"),
            new("Xuất Excel", "Xuất bảng khối lượng", new RelayCommand(() => Count(hits, 2)), shortcut: "Ctrl+E", category: "Xuất bản")
        };

        static void Count(int[]? store, int index)
        {
            if (store is not null) store[index]++;
        }
    }

    [Fact]
    public void CommandPalette_Defaults_MatchSpecifications()
    {
        RunOnSTA(() =>
        {
            var palette = new CommandPalette();

            Assert.False(palette.IsOpen);
            Assert.Equal(string.Empty, palette.FilterText);
            Assert.Equal(-1, palette.SelectedIndex);
            Assert.Null(palette.SelectedItem);
            Assert.True(palette.CloseOnInvoke);
            Assert.Equal(276.0, palette.MaxListHeight);
        });
    }

    [Fact]
    public void CommandPalette_ListsEveryCommandWhenTheFilterIsEmpty()
    {
        RunOnSTA(() =>
        {
            var palette = new CommandPalette { ItemsSource = SampleCommands() };

            Assert.Equal(3, palette.FilteredItems.Count);
            Assert.True(palette.HasResults);
            Assert.Equal(0, palette.SelectedIndex);
            Assert.Equal("Tổng quan dự án", palette.SelectedItem?.Title);
        });
    }

    [Fact]
    public void CommandPalette_FilterIgnoresDiacriticsAndCase()
    {
        RunOnSTA(() =>
        {
            var palette = new CommandPalette { ItemsSource = SampleCommands() };

            palette.FilterText = "tong quan";
            Assert.Single(palette.FilteredItems);
            Assert.Equal("Tổng quan dự án", palette.SelectedItem?.Title);

            palette.FilterText = "XUAT";
            Assert.Equal(2, palette.FilteredItems.Count);
        });
    }

    [Fact]
    public void CommandPalette_FilterThatMatchesNothing_ReportsNoResults()
    {
        RunOnSTA(() =>
        {
            var palette = new CommandPalette { ItemsSource = SampleCommands() };

            palette.FilterText = "khong co lenh nay";

            Assert.Empty(palette.FilteredItems);
            Assert.False(palette.HasResults);
            Assert.Equal(-1, palette.SelectedIndex);
            Assert.Null(palette.SelectedItem);
        });
    }

    [Fact]
    public void CommandPalette_MoveSelection_ClampsAtBothEnds()
    {
        RunOnSTA(() =>
        {
            var palette = new CommandPalette { ItemsSource = SampleCommands() };

            palette.MoveSelection(-5);
            Assert.Equal(0, palette.SelectedIndex);

            palette.MoveSelection(1);
            Assert.Equal(1, palette.SelectedIndex);

            palette.MoveSelection(99);
            Assert.Equal(2, palette.SelectedIndex);
            Assert.Equal("Xuất Excel", palette.SelectedItem?.Title);
        });
    }

    [Fact]
    public void CommandPalette_ExecuteSelected_RunsTheCommandAndCloses()
    {
        RunOnSTA(() =>
        {
            var hits = new int[3];
            var palette = new CommandPalette { ItemsSource = SampleCommands(hits) };

            palette.Open();
            palette.FilterText = "excel";

            bool invoked = palette.ExecuteSelected();

            Assert.True(invoked);
            Assert.Equal(1, hits[2]);
            Assert.Equal(0, hits[0]);
            Assert.False(palette.IsOpen);
        });
    }

    [Fact]
    public void CommandPalette_ExecuteSelected_WithNoMatchDoesNothing()
    {
        RunOnSTA(() =>
        {
            var hits = new int[3];
            var palette = new CommandPalette { ItemsSource = SampleCommands(hits) };

            palette.Open();
            palette.FilterText = "zzzz";

            Assert.False(palette.ExecuteSelected());
            Assert.Equal(0, hits[0] + hits[1] + hits[2]);
        });
    }

    [Fact]
    public void CommandPalette_DisabledItem_IsListedButCannotRun()
    {
        RunOnSTA(() =>
        {
            var hits = new int[1];
            var item = new CommandPaletteItem("Khoá", null, new RelayCommand(() => hits[0]++)) { IsEnabled = false };
            var palette = new CommandPalette { ItemsSource = new List<CommandPaletteItem> { item } };

            Assert.Single(palette.FilteredItems);
            Assert.False(item.CanInvoke);
            Assert.False(palette.ExecuteSelected());
            Assert.Equal(0, hits[0]);
        });
    }

    [Fact]
    public void CommandPalette_ItemInvoked_RaisesTheEvent()
    {
        RunOnSTA(() =>
        {
            var palette = new CommandPalette { ItemsSource = SampleCommands() };
            CommandPaletteItem? captured = null;
            palette.ItemInvoked += (_, e) => captured = e.Item;

            palette.Open();
            palette.ExecuteSelected();

            Assert.NotNull(captured);
            Assert.Equal("Tổng quan dự án", captured!.Title);
        });
    }

    [Fact]
    public void CommandPalette_KeepOpenAfterInvoke_LeavesTheOverlayOpen()
    {
        RunOnSTA(() =>
        {
            var palette = new CommandPalette { ItemsSource = SampleCommands(), CloseOnInvoke = false };

            palette.Open();
            palette.ExecuteSelected();

            Assert.True(palette.IsOpen);
        });
    }

    [Fact]
    public void CommandPalette_OpenAndClose_RaiseTheEvents()
    {
        RunOnSTA(() =>
        {
            var palette = new CommandPalette { ItemsSource = SampleCommands() };
            int opened = 0, closed = 0;
            palette.Opened += (_, _) => opened++;
            palette.Closed += (_, _) => closed++;

            palette.Open();
            palette.Close();

            Assert.Equal(1, opened);
            Assert.Equal(1, closed);
            Assert.False(palette.IsOpen);
        });
    }

    [Fact]
    public void CommandPalette_SelectionStaysInsideTheFilteredList()
    {
        RunOnSTA(() =>
        {
            var palette = new CommandPalette { ItemsSource = SampleCommands() };

            palette.MoveSelection(2);
            Assert.Equal("Xuất Excel", palette.SelectedItem?.Title);

            // The highlighted row is filtered away; the palette must not keep pointing at it.
            palette.FilterText = "tong";

            Assert.Single(palette.FilteredItems);
            Assert.Equal(0, palette.SelectedIndex);
            Assert.Equal("Tổng quan dự án", palette.SelectedItem?.Title);
        });
    }

    [Fact]
    public void CommandPalette_InvokeItem_RunsTheClickedRow()
    {
        RunOnSTA(() =>
        {
            var hits = new int[3];
            var items = SampleCommands(hits);
            var palette = new CommandPalette { ItemsSource = items };

            palette.Open();
            bool invoked = palette.InvokeItem(items[1]);

            Assert.True(invoked);
            Assert.Equal(1, hits[1]);
            Assert.False(palette.IsOpen);
        });
    }
}
