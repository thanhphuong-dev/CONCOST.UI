using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using CONCOST.UI.Controls;
using CONCOST.UI.Core.MVVM;
using Xunit;

namespace CONCOST.UI.Tests;

/// <summary>
/// Automated UI tests (§97): real key events routed through a shown window, instead of calling the
/// handlers directly.
/// </summary>
/// <remarks>
/// The difference matters: <see cref="Phase3ControlsTests"/> proves the palette's model is right,
/// these prove the keystrokes actually reach it once the template is applied and the window has a
/// presentation source — the layer where focus and routing usually break.
/// </remarks>
public class KeyboardInputTests
{
    private static void RunOnSTA(Action action) => WpfApplicationHost.Run(action);

    private static Window Show(CommandPalette palette)
    {
        var window = new Window
        {
            Width = 800,
            Height = 600,
            Content = new Grid { Children = { palette } },
            ShowInTaskbar = false,
            WindowStartupLocation = WindowStartupLocation.Manual,
            Left = -4000,
            Top = -4000
        };

        window.Show();
        window.UpdateLayout();
        WpfApplicationHost.Pump();

        return window;
    }

    private static void PressKey(UIElement target, Key key)
    {
        PresentationSource source = PresentationSource.FromVisual(target)
            ?? throw new InvalidOperationException("The element is not connected to a presentation source.");

        var args = new KeyEventArgs(Keyboard.PrimaryDevice, source, 0, key)
        {
            RoutedEvent = Keyboard.PreviewKeyDownEvent
        };

        target.RaiseEvent(args);
    }

    [Fact]
    public void CommandPalette_ArrowDownMovesTheHighlight()
    {
        RunOnSTA(() =>
        {
            var palette = new CommandPalette
            {
                ItemsSource = new List<CommandPaletteItem>
                {
                    new("Tổng quan dự án"),
                    new("Xuất PDF"),
                    new("Xuất Excel")
                }
            };

            Window window = Show(palette);

            palette.Open();
            WpfApplicationHost.Pump();

            Assert.Equal(0, palette.SelectedIndex);

            PressKey(palette, Key.Down);
            Assert.Equal(1, palette.SelectedIndex);

            PressKey(palette, Key.Down);
            PressKey(palette, Key.Down);
            Assert.Equal(2, palette.SelectedIndex);

            PressKey(palette, Key.Up);
            Assert.Equal(1, palette.SelectedIndex);

            window.Close();
        });
    }

    [Fact]
    public void CommandPalette_EnterRunsTheHighlightedCommand()
    {
        RunOnSTA(() =>
        {
            var hits = new int[3];
            var palette = new CommandPalette
            {
                ItemsSource = new List<CommandPaletteItem>
                {
                    new("Tổng quan dự án", command: new RelayCommand(() => hits[0]++)),
                    new("Xuất PDF", command: new RelayCommand(() => hits[1]++)),
                    new("Xuất Excel", command: new RelayCommand(() => hits[2]++))
                }
            };

            Window window = Show(palette);

            palette.Open();
            WpfApplicationHost.Pump();

            PressKey(palette, Key.Down);
            PressKey(palette, Key.Enter);

            Assert.Equal(1, hits[1]);
            Assert.Equal(0, hits[0]);
            Assert.False(palette.IsOpen);

            window.Close();
        });
    }

    [Fact]
    public void CommandPalette_EscapeClosesWithoutRunningAnything()
    {
        RunOnSTA(() =>
        {
            var hits = new int[3];
            var palette = new CommandPalette
            {
                ItemsSource = new List<CommandPaletteItem>
                {
                    new("Tổng quan dự án", command: new RelayCommand(() => hits[0]++)),
                    new("Xuất PDF", command: new RelayCommand(() => hits[1]++)),
                    new("Xuất Excel", command: new RelayCommand(() => hits[2]++))
                }
            };

            Window window = Show(palette);

            palette.Open();
            WpfApplicationHost.Pump();

            PressKey(palette, Key.Escape);

            Assert.False(palette.IsOpen);
            Assert.Equal(0, hits[0] + hits[1] + hits[2]);

            window.Close();
        });
    }

    [Fact]
    public void CommandPalette_OpenMovesFocusToTheSearchBox()
    {
        RunOnSTA(() =>
        {
            var palette = new CommandPalette();
            Window window = Show(palette);

            palette.Open();
            WpfApplicationHost.Pump();
            WpfApplicationHost.Pump();

            var searchBox = FindTextBox(palette);
            Assert.NotNull(searchBox);
            Assert.True(searchBox!.IsKeyboardFocused || searchBox.IsFocused,
                "The search box should own the keyboard once the palette opens.");

            window.Close();
        });
    }

    private static TextBox? FindTextBox(DependencyObject root)
    {
        int count = VisualTreeHelper.GetChildrenCount(root);
        for (int i = 0; i < count; i++)
        {
            DependencyObject child = VisualTreeHelper.GetChild(root, i);
            if (child is TextBox box) return box;

            TextBox? nested = FindTextBox(child);
            if (nested is not null) return nested;
        }

        return null;
    }
}
