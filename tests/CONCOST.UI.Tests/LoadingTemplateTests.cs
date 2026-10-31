using System.Windows;
using System.Windows.Media.Animation;
using CONCOST.UI.Controls;
using Xunit;

namespace CONCOST.UI.Tests;

/// <summary>
/// The roll and cycle spinners keep their storyboard in <c>ControlTemplate.Resources</c>. It used to
/// sit in the template's inner <c>Grid.Resources</c>, which is a different scope: the trigger's
/// <c>{StaticResource RollStoryboard}</c> could not see it, so instantiating the template threw
/// <c>XamlParseException</c> — and materialising the whole theme dictionary (which the showcase
/// inspector does when it lists style keys) took the application down with it.
/// </summary>
public class LoadingTemplateTests
{
    private static void RunOnSta(Action action) => WpfApplicationHost.Run(action);

    private static void Host(UIElement content, Action<Window> body)
    {
        var window = new Window
        {
            Width = 220,
            Height = 220,
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

    [Fact]
    public void RollLoading_template_owns_its_storyboard()
    {
        RunOnSta(() =>
        {
            var roll = new RollLoading();

            Host(roll, _ =>
            {
                Assert.NotNull(roll.Template);

                // The resource must live in the template, not in an element inside it.
                Assert.IsType<Storyboard>(roll.Template!.Resources["RollStoryboard"]);

                // …and the template still finds its parts.
                Assert.NotNull(roll.Template.FindName("PART_Orb1", roll));
            });
        });
    }

    [Fact]
    public void CycleLoading_starts_its_storyboard_when_loading()
    {
        RunOnSta(() =>
        {
            var cycle = new CycleLoading();

            Host(cycle, _ =>
            {
                Assert.NotNull(cycle.Template);
                Assert.IsType<Storyboard>(cycle.Template!.Resources["CycleStoryboard"]);
                Assert.NotNull(cycle.Template.FindName("PART_RotateRing", cycle));

                // EnterActions resolve the same resource — this is where the old scope blew up.
                cycle.IsLoading = true;
                cycle.UpdateLayout();
                WpfApplicationHost.Pump();

                Assert.True(cycle.IsLoading);
            });
        });
    }
}
