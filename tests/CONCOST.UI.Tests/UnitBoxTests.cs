using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CONCOST.UI.Controls;
using Xunit;

namespace CONCOST.UI.Tests;

/// <summary>
/// <see cref="UnitBox"/> carried a <see cref="UnitBox.Header"/> property the template never showed,
/// and took no step. The rebuild renders the header as a label row, passes the step and the range
/// through to the inner numeric editor, and bubbles its <see cref="UnitBox.ValueChanged"/> event.
/// </summary>
public class UnitBoxTests
{
    private static void RunOnSTA(Action action) => WpfApplicationHost.Run(action);

    private static Window Show(UnitBox box)
    {
        var window = new Window
        {
            Width = 260,
            Height = 120,
            Content = box,
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

    [Fact]
    public void Template_applies_and_names_the_inner_editor()
    {
        RunOnSTA(() =>
        {
            var box = new UnitBox();
            Show(box);

            var editor = box.Template!.FindName("Editor", box);
            Assert.IsType<NumericUpDown>(editor);
        });
    }

    [Fact]
    public void Empty_header_collapses_the_label_row()
    {
        RunOnSTA(() =>
        {
            var plain = new UnitBox();
            Show(plain);

            var hidden = (TextBlock)plain.Template!.FindName("HeaderHost", plain)!;
            Assert.Equal(Visibility.Collapsed, hidden.Visibility);

            var labelled = new UnitBox { Header = "Bề rộng dầm" };
            Show(labelled);

            var visible = (TextBlock)labelled.Template!.FindName("HeaderHost", labelled)!;
            Assert.Equal(Visibility.Visible, visible.Visibility);
            Assert.Equal("Bề rộng dầm", visible.Text);
        });
    }

    [Fact]
    public void Step_and_range_pass_through_to_the_inner_editor()
    {
        RunOnSTA(() =>
        {
            var box = new UnitBox { Increment = 25, Minimum = 0, Maximum = 500 };
            Show(box);

            var editor = (NumericUpDown)box.Template!.FindName("Editor", box)!;
            Assert.Equal(25, editor.Increment);
            Assert.Equal(0, editor.Minimum);
            Assert.Equal(500, editor.Maximum);
        });
    }

    [Fact]
    public void Value_round_trips_through_the_inner_editor()
    {
        RunOnSTA(() =>
        {
            var box = new UnitBox { Value = 3200, Unit = "mm" };
            Show(box);

            var editor = (NumericUpDown)box.Template!.FindName("Editor", box)!;
            Assert.Equal(3200, editor.Value);

            editor.Value = 4800;
            WpfApplicationHost.Pump();

            Assert.Equal(4800, box.Value);
        });
    }

    [Fact]
    public void ValueChanged_bubbles_from_the_inner_editor()
    {
        RunOnSTA(() =>
        {
            int changes = 0;
            double observed = double.NaN;

            var box = new UnitBox();
            box.ValueChanged += (_, e) => { changes++; observed = e.NewValue; };
            Show(box);

            var editor = (NumericUpDown)box.Template!.FindName("Editor", box)!;
            editor.Value = 42;
            WpfApplicationHost.Pump();

            Assert.Equal(1, changes);
            Assert.Equal(42, observed);
        });
    }
}
