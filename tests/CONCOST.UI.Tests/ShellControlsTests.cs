using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CONCOST.UI.Controls;
using CONCOST.UI.Core.MVVM;
using CONCOST.UI.Icons;
using Xunit;

namespace CONCOST.UI.Tests;

/// <summary>
/// The shell controls: <see cref="StatusBar"/>, <see cref="CommandBar"/>,
/// <see cref="QuickAccessToolbar"/> and the <see cref="OverflowPanel"/> behind them.
/// </summary>
public class ShellControlsTests
{
    private static void RunOnSTA(Action action) => WpfApplicationHost.Run(action);

    private static void HostInWindow(FrameworkElement content, double width, double height, Action<Window> body)
    {
        var window = new Window
        {
            Width = width,
            Height = height,
            Content = content,
            ShowInTaskbar = false,
            WindowStyle = WindowStyle.None
        };

        window.Show();
        window.UpdateLayout();
        WpfApplicationHost.Pump();

        try
        {
            body(window);
        }
        finally
        {
            window.Content = null;
            window.Close();
        }
    }

    private static CommandBarItem Item(string label, ICommand? command = null) =>
        new(label, command, IconKind.File);

    // ------------------------------------------------------------------ StatusBar

    [Fact]
    public void StatusBarItem_Defaults_MatchSpecifications()
    {
        RunOnSTA(() =>
        {
            var item = new StatusBarItem();

            Assert.Equal(IconKind.None, item.Icon);
            Assert.Equal(string.Empty, item.Text);
            Assert.False(item.IsSeparator);
            Assert.Equal(StatusBarAlignment.Left, item.Alignment);
            Assert.Null(item.Command);
        });
    }

    [Fact]
    public void StatusBarPanel_ArrangesLeftAndRightCellsFromTheirEdges()
    {
        RunOnSTA(() =>
        {
            var panel = new StatusBarPanel { Spacing = 10 };
            var left = new StatusBarItem { Text = "Sẵn sàng" };
            var right = new StatusBarItem { Text = "100%", Alignment = StatusBarAlignment.Right };

            panel.Children.Add(left);
            panel.Children.Add(right);

            var host = new Border { Child = panel };
            host.Measure(new Size(400, 28));
            host.Arrange(new Rect(0, 0, 400, 28));
            host.UpdateLayout();

            Assert.True(left.ActualWidth > 0);
            Assert.True(right.ActualWidth > 0);

            var leftOrigin = left.TranslatePoint(new Point(0, 0), panel);
            var rightOrigin = right.TranslatePoint(new Point(0, 0), panel);

            Assert.Equal(0, leftOrigin.X, 1);
            Assert.Equal(400, rightOrigin.X + right.ActualWidth, 1);
        });
    }

    [Fact]
    public void StatusBar_LaysOutBothSidesInsideItsPadding()
    {
        RunOnSTA(() =>
        {
            var bar = new StatusBar { Width = 400, Height = 28 };
            var left = new StatusBarItem { Text = "Sẵn sàng" };
            var right = new StatusBarItem { Text = "100%", Alignment = StatusBarAlignment.Right };

            bar.Items.Add(left);
            bar.Items.Add(right);

            var host = new Border { Child = bar };
            host.Measure(new Size(400, 28));
            host.Arrange(new Rect(0, 0, 400, 28));
            host.UpdateLayout();

            Assert.True(left.ActualWidth > 0);
            Assert.True(right.ActualWidth > 0);

            var leftOrigin = left.TranslatePoint(new Point(0, 0), bar);
            var rightOrigin = right.TranslatePoint(new Point(0, 0), bar);

            // The template insets the strip by 12px, so the left cell starts there and the right
            // one finishes 12px short of the far edge.
            Assert.InRange(leftOrigin.X, 0, 20);
            Assert.InRange(rightOrigin.X + right.ActualWidth, 380, 400);
        });
    }

    [Fact]
    public void StatusBarItem_RunsItsCommandOnClick()
    {
        RunOnSTA(() =>
        {
            int runs = 0;
            var item = new StatusBarItem
            {
                Text = "Bấm",
                Command = new RelayCommand(() => runs++)
            };

            item.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, MouseButton.Left)
            {
                RoutedEvent = Mouse.MouseUpEvent,
                Source = item
            });

            Assert.Equal(1, runs);
        });
    }

    // ------------------------------------------------------------------ OverflowPanel

    [Fact]
    public void OverflowPanel_StopsWhenTheWidthRunsOut()
    {
        RunOnSTA(() =>
        {
            var panel = new OverflowPanel { Spacing = 0 };

            for (int i = 0; i < 5; i++)
            {
                panel.Children.Add(new Border { Width = 40, Height = 20 });
            }

            panel.Measure(new Size(100, 20));

            Assert.Equal(2, panel.VisibleCount);
            Assert.Equal(3, panel.OverflowCount);
        });
    }

    [Fact]
    public void OverflowPanel_WithRoomForEverything_OverflowsNothing()
    {
        RunOnSTA(() =>
        {
            var panel = new OverflowPanel { Spacing = 0 };
            panel.Children.Add(new Border { Width = 30, Height = 20 });
            panel.Children.Add(new Border { Width = 30, Height = 20 });

            panel.Measure(new Size(200, 20));

            Assert.Equal(2, panel.VisibleCount);
            Assert.Equal(0, panel.OverflowCount);
        });
    }

    // ------------------------------------------------------------------ CommandBar

    [Fact]
    public void CommandBar_PushesWhatDoesNotFitIntoOverflow()
    {
        RunOnSTA(() =>
        {
            var bar = new CommandBar
            {
                ItemsSource = new List<CommandBarItem>
                {
                    Item("Xuất PDF"), Item("Xuất Excel"), Item("In"), Item("Cấu hình"), Item("Trợ giúp")
                }
            };

            HostInWindow(bar, 220, 60, _ =>
            {
                Assert.True(bar.HasOverflow);
                Assert.NotEmpty(bar.OverflowItems);
            });
        });
    }

    [Fact]
    public void CommandBar_WithEnoughRoom_ShowsEverything()
    {
        RunOnSTA(() =>
        {
            var bar = new CommandBar
            {
                ItemsSource = new List<CommandBarItem> { Item("Một"), Item("Hai") }
            };

            HostInWindow(bar, 600, 60, _ =>
            {
                Assert.False(bar.HasOverflow);
                Assert.Empty(bar.OverflowItems);
                Assert.False(bar.IsOverflowOpen);
            });
        });
    }

    [Fact]
    public void CommandBar_Invoke_RunsTheItemCommand()
    {
        RunOnSTA(() =>
        {
            int runs = 0;
            var item = Item("Chạy", new RelayCommand(() => runs++));
            var bar = new CommandBar { ItemsSource = new List<CommandBarItem> { item } };

            bar.Invoke(item);

            Assert.Equal(1, runs);
        });
    }

    [Fact]
    public void CommandBar_Invoke_SkipsDisabledItems()
    {
        RunOnSTA(() =>
        {
            int runs = 0;
            var item = Item("Tắt", new RelayCommand(() => runs++));
            item.IsEnabled = false;

            var bar = new CommandBar { ItemsSource = new List<CommandBarItem> { item } };
            bar.Invoke(item);

            Assert.Equal(0, runs);
        });
    }

    // ------------------------------------------------------------------ QuickAccessToolbar

    [Fact]
    public void QuickAccessToolbar_Defaults_AndInvoke()
    {
        RunOnSTA(() =>
        {
            int runs = 0;
            var item = new QuickAccessItem(IconKind.Save, "Lưu", new RelayCommand(() => runs++));
            var toolbar = new QuickAccessToolbar { ItemsSource = new List<QuickAccessItem> { item } };

            Assert.True(toolbar.IsCustomizable);
            Assert.False(toolbar.HasOverflow);

            toolbar.Invoke(item);
            Assert.Equal(1, runs);
        });
    }

    [Fact]
    public void QuickAccessToolbar_OverflowsIntoTheMenu()
    {
        RunOnSTA(() =>
        {
            var items = new List<QuickAccessItem>();
            for (int i = 0; i < 12; i++) items.Add(new QuickAccessItem(IconKind.File, "Mục " + i, null));

            var toolbar = new QuickAccessToolbar { ItemsSource = items };

            HostInWindow(toolbar, 120, 40, _ =>
            {
                Assert.True(toolbar.HasOverflow);
                Assert.NotEmpty(toolbar.OverflowItems);
            });
        });
    }
}
