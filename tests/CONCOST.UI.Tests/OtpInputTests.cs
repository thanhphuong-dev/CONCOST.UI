using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CONCOST.UI.Controls;
using Xunit;

namespace CONCOST.UI.Tests;

/// <summary>
/// The rebuilt <see cref="OtpInput"/>: real cells, auto-advance, backspace retreat, paste
/// distribution and the <see cref="OtpInput.Completed"/> signal. The keyboard paths run as real
/// routed events through a shown window, the same layer the <see cref="KeyboardInputTests"/> guard.
/// </summary>
public class OtpInputTests
{
    private static void RunOnSTA(Action action) => WpfApplicationHost.Run(action);

    private static Window Show(OtpInput otp)
    {
        var window = new Window
        {
            Width = 420,
            Height = 160,
            Content = otp,
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

    private static void TypeCharacter(TextBox cell, string text)
    {
        var composition = new TextComposition(InputManager.Current, cell, text);
        var args = new TextCompositionEventArgs(Keyboard.PrimaryDevice, composition)
        {
            RoutedEvent = UIElement.PreviewTextInputEvent
        };

        cell.RaiseEvent(args);
    }

    private static void PressKey(TextBox cell, Key key)
    {
        var source = PresentationSource.FromVisual(cell)
            ?? throw new InvalidOperationException("The cell is not connected to a presentation source.");

        var args = new KeyEventArgs(Keyboard.PrimaryDevice, source, 0, key)
        {
            RoutedEvent = Keyboard.PreviewKeyDownEvent
        };

        cell.RaiseEvent(args);
    }

    private static TextBox? FocusedCell(OtpInput otp) => Keyboard.FocusedElement as TextBox;

    [Fact]
    public void Template_applies_and_builds_one_cell_per_length()
    {
        RunOnSTA(() =>
        {
            var otp = new OtpInput { Length = 6 };
            Show(otp);

            var panel = (StackPanel)otp.Template!.FindName("PART_Panel", otp)!;
            Assert.Equal(6, panel.Children.Count);
            Assert.Equal(string.Empty, otp.Value);
            Assert.False(otp.IsCompleted);
        });
    }

    [Fact]
    public void Length_coerces_into_the_one_to_ten_band()
    {
        RunOnSTA(() =>
        {
            Assert.Equal(1, new OtpInput { Length = 0 }.Length);
            Assert.Equal(10, new OtpInput { Length = 24 }.Length);
        });
    }

    [Fact]
    public void Typing_fills_the_cell_advances_focus_and_updates_value()
    {
        RunOnSTA(() =>
        {
            var otp = new OtpInput { Length = 4 };
            Show(otp);

            var panel = (StackPanel)otp.Template!.FindName("PART_Panel", otp)!;
            var first = (TextBox)((Border)panel.Children[0]).Child;

            first.Focus();
            WpfApplicationHost.Pump();

            TypeCharacter(first, "4");
            WpfApplicationHost.Pump();

            Assert.Equal("4", otp.Value);
            Assert.Equal(Keyboard.FocusedElement, (TextBox)((Border)panel.Children[1]).Child);
        });
    }

    [Fact]
    public void Backspace_on_an_empty_cell_clears_the_previous_one_and_retreats()
    {
        RunOnSTA(() =>
        {
            var otp = new OtpInput { Length = 4 };
            Show(otp);

            var panel = (StackPanel)otp.Template!.FindName("PART_Panel", otp)!;
            var first = (TextBox)((Border)panel.Children[0]).Child;
            var second = (TextBox)((Border)panel.Children[1]).Child;

            first.Focus();
            TypeCharacter(first, "7");
            WpfApplicationHost.Pump();

            // The caret advanced into the second cell; backspace there clears the first.
            PressKey(second, Key.Back);
            WpfApplicationHost.Pump();

            Assert.Equal(string.Empty, otp.Value);
            Assert.Equal(Keyboard.FocusedElement, first);
        });
    }

    [Fact]
    public void Numeric_only_rejects_letters_by_default()
    {
        RunOnSTA(() =>
        {
            var otp = new OtpInput { Length = 4 };
            Show(otp);

            var panel = (StackPanel)otp.Template!.FindName("PART_Panel", otp)!;
            var first = (TextBox)((Border)panel.Children[0]).Child;

            first.Focus();
            TypeCharacter(first, "a");
            WpfApplicationHost.Pump();

            Assert.Equal(string.Empty, otp.Value);

            otp.NumericOnly = false;
            TypeCharacter(first, "a");
            WpfApplicationHost.Pump();

            Assert.Equal("a", otp.Value);
        });
    }

    [Fact]
    public void Completed_fires_only_when_the_last_cell_is_filled()
    {
        RunOnSTA(() =>
        {
            int raised = 0;
            var otp = new OtpInput { Length = 3 };
            otp.Completed += (_, _) => raised++;
            Show(otp);

            var panel = (StackPanel)otp.Template!.FindName("PART_Panel", otp)!;

            otp.Value = "12";
            WpfApplicationHost.Pump();
            Assert.Equal(0, raised);
            Assert.False(otp.IsCompleted);

            var third = (TextBox)((Border)panel.Children[2]).Child;
            third.Focus();
            TypeCharacter(third, "9");
            WpfApplicationHost.Pump();

            Assert.Equal(1, raised);
            Assert.True(otp.IsCompleted);
            Assert.Equal("129", otp.Value);
        });
    }

    [Fact]
    public void Setting_value_from_the_host_fills_the_cells()
    {
        RunOnSTA(() =>
        {
            var otp = new OtpInput { Length = 4 };
            Show(otp);

            otp.Value = "8624";
            WpfApplicationHost.Pump();

            Assert.True(otp.IsCompleted);

            otp.Value = string.Empty;
            WpfApplicationHost.Pump();

            Assert.False(otp.IsCompleted);
            Assert.Equal(string.Empty, otp.Value);
        });
    }

    [Fact]
    public void Mask_character_replaces_every_digit_on_screen_but_not_in_value()
    {
        RunOnSTA(() =>
        {
            var otp = new OtpInput { Length = 4, MaskCharacter = '•' };
            Show(otp);

            otp.Value = "1357";
            WpfApplicationHost.Pump();

            var panel = (StackPanel)otp.Template!.FindName("PART_Panel", otp)!;

            foreach (Border border in panel.Children)
            {
                var cell = (TextBox)border.Child;
                Assert.Equal("•", cell.Text);
            }

            Assert.Equal("1357", otp.Value);
        });
    }

    [Fact]
    public void Changing_length_rebuilds_the_cells_and_trims_the_value()
    {
        RunOnSTA(() =>
        {
            var otp = new OtpInput { Length = 4, Value = "1234" };
            Show(otp);

            otp.Length = 2;
            WpfApplicationHost.Pump();

            var panel = (StackPanel)otp.Template!.FindName("PART_Panel", otp)!;
            Assert.Equal(2, panel.Children.Count);
            Assert.Equal("12", otp.Value);
        });
    }
}
