using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Media3D;
using CONCOST.UI.Controls;
using Xunit;
using Xunit.Abstractions;

namespace CONCOST.UI.Tests;

/// <summary>
/// The 1.2.0 ViewCube upgrade: keyboard navigation (arrows step, Ctrl+arrows roll, Home returns),
/// the orientation readout, saved views and the orbit switch. The keyboard paths run as real key
/// events through a shown window, the way <see cref="KeyboardInputTests"/> does.
/// </summary>
public class ViewCubeUpgradeTests
{
    private readonly ITestOutputHelper _output;

    public ViewCubeUpgradeTests(ITestOutputHelper output) => _output = output;

    private static void RunOnSTA(Action action) => WpfApplicationHost.Run(action);

    private static ViewCube Show(double size = 120)
    {
        var cube = new ViewCube { Width = size, Height = size, AnimatedTransitions = false };
        var window = new Window
        {
            Content = cube,
            Width = size + 40,
            Height = size + 40,
            ShowInTaskbar = false,
            WindowStartupLocation = WindowStartupLocation.Manual,
            Left = -4000,
            Top = -4000
        };

        window.Show();
        window.UpdateLayout();
        WpfApplicationHost.Pump();

        return cube;
    }

    private static void PressKey(ViewCube cube, Key key)
    {
        var source = PresentationSource.FromVisual(cube)
            ?? throw new InvalidOperationException("The cube is not connected to a presentation source.");

        var args = new KeyEventArgs(Keyboard.PrimaryDevice, source, 0, key)
        {
            RoutedEvent = Keyboard.KeyDownEvent
        };

        cube.RaiseEvent(args);
    }

    private static void InvokeStepOrRoll(ViewCube cube, ArrowDirection direction, ModifierKeys modifiers)
    {
        typeof(ViewCube)
            .GetMethod("StepOrRoll", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
            .Invoke(cube, new object[] { direction, modifiers });
    }

    // ------------------------------------------------------------------ orientation names

    [Theory]
    [InlineData("TOP", "TOP")]
    [InlineData("FRONT", "FRONT")]
    [InlineData("TOP-FRONT-RIGHT", "SE Isometric")]
    [InlineData("BOTTOM-BACK-LEFT", "Bottom NW Isometric")]
    [InlineData("TOP-FRONT", "TOP-FRONT")]
    public void OrientationName_follows_the_compass_the_widget_draws(string regionName, string expected)
    {
        var region = ViewCubeMath.Regions.Single(r => r.Name == regionName);

        Assert.Equal(expected, ViewCubeMath.OrientationName(region));
    }

    [Fact]
    public void OrientationName_reports_the_closest_region_to_the_current_rotation()
    {
        RunOnSTA(() =>
        {
            var cube = Show();

            cube.Rotation = ViewCubeMath.RotationFor(ViewCubeMath.Regions.Single(r => r.Name == "TOP"));
            Assert.Equal("TOP", cube.OrientationName);

            cube.Rotation = ViewCubeMath.RotationFor(ViewCubeMath.Regions.Single(r => r.Name == "TOP-FRONT-RIGHT"));
            Assert.Equal("SE Isometric", cube.OrientationName);

            cube.OrientationNameOverride = "Góc đang lưu";
            Assert.Equal("Góc đang lưu", cube.OrientationName);
        });
    }

    // ------------------------------------------------------------------ keyboard

    [Fact]
    public void Arrow_keys_step_to_the_neighbouring_face()
    {
        RunOnSTA(() =>
        {
            var cube = Show();
            cube.Rotation = ViewCubeMath.RotationFor(ViewCubeMath.Regions.Single(r => r.Name == "FRONT"));

            cube.Focus();
            WpfApplicationHost.Pump();
            Assert.True(cube.IsKeyboardFocused);

            int requested = 0;
            cube.ViewRequested += (_, e) =>
            {
                requested++;
                Assert.Equal(ViewCubePart.Arrow, e.Hit.Part);
            };

            PressKey(cube, Key.Right);
            PressKey(cube, Key.Up);

            Assert.Equal(2, requested);
        });
    }

    [Fact]
    public void Ctrl_arrows_roll_instead_of_stepping()
    {
        RunOnSTA(() =>
        {
            var cube = Show();

            int rolled = 0;
            cube.ViewRequested += (_, e) =>
            {
                if (e.Hit.Part == ViewCubePart.Roll) rolled += e.Hit.Roll;
            };

            InvokeStepOrRoll(cube, ArrowDirection.Right, ModifierKeys.Control);
            InvokeStepOrRoll(cube, ArrowDirection.Left, ModifierKeys.Control);

            Assert.Equal(0, rolled); // +1 then -1 cancel out
        });
    }

    [Fact]
    public void Home_returns_to_the_home_rotation()
    {
        RunOnSTA(() =>
        {
            var cube = Show();
            cube.Rotation = ViewCubeMath.RotationFor(ViewCubeMath.Regions.Single(r => r.Name == "TOP"));
            cube.HomeRotation = ViewCubeMath.RotationFor(ViewCubeMath.Regions.Single(r => r.Name == "BACK"));

            cube.Focus();
            WpfApplicationHost.Pump();

            PressKey(cube, Key.Home);
            WpfApplicationHost.Pump();

            Assert.Equal(ViewCubeMath.NormalizeRotation(cube.HomeRotation), ViewCubeMath.NormalizeRotation(cube.CurrentRotation));
        });
    }

    // ------------------------------------------------------------------ saved views

    [Fact]
    public void Saved_views_round_trip_through_save_go_and_remove()
    {
        RunOnSTA(() =>
        {
            var cube = Show();
            var top = ViewCubeMath.RotationFor(ViewCubeMath.Regions.Single(r => r.Name == "TOP"));
            var back = ViewCubeMath.RotationFor(ViewCubeMath.Regions.Single(r => r.Name == "BACK"));

            cube.Rotation = top;
            var saved = cube.SaveCurrentView("Đỉnh");
            Assert.Single(cube.SavedViews);
            Assert.Equal(top, saved.Rotation);

            cube.Rotation = back;
            Assert.True(cube.GoToSavedView("đỉnh")); // name matching ignores case
            WpfApplicationHost.Pump();

            Assert.Equal(ViewCubeMath.NormalizeRotation(top), ViewCubeMath.NormalizeRotation(cube.CurrentRotation));

            ViewCubeSavedView? activated = null;
            cube.SavedViewActivated += (_, v) => activated = v;

            cube.Rotation = back;
            cube.GoToSavedView("Đỉnh");
            Assert.NotNull(activated);
            Assert.Equal("Đỉnh", activated!.Name);

            Assert.True(cube.RemoveSavedView("Đỉnh"));
            Assert.Empty(cube.SavedViews);
            Assert.False(cube.GoToSavedView("Đỉnh"));
        });
    }

    [Fact]
    public void Saving_under_the_same_name_overwrites()
    {
        RunOnSTA(() =>
        {
            var cube = Show();
            cube.Rotation = ViewCubeMath.RotationFor(ViewCubeMath.Regions.Single(r => r.Name == "TOP"));
            cube.SaveCurrentView("A");

            var back = ViewCubeMath.RotationFor(ViewCubeMath.Regions.Single(r => r.Name == "BACK"));
            cube.Rotation = back;
            cube.SaveCurrentView("A");

            Assert.Single(cube.SavedViews);
            Assert.Equal(ViewCubeMath.NormalizeRotation(back), ViewCubeMath.NormalizeRotation(cube.SavedViews[0].Rotation));
        });
    }

    [Fact]
    public void SaveCurrentView_rejects_a_blank_name()
    {
        RunOnSTA(() =>
        {
            Assert.Throws<ArgumentException>(() => Show().SaveCurrentView("   "));
        });
    }

    // ------------------------------------------------------------------ orbit switch

    [Fact]
    public void Orientation_label_and_selector_report_their_hit_zone()
    {
        RunOnSTA(() =>
        {
            var cube = Show();
            cube.ShowOrientationLabel = true;
            WpfApplicationHost.Pump();

            double width = cube.ActualWidth;
            double height = cube.ActualHeight;

            var hit = cube.HitTest(new Point(width / 2, height - 4));
            Assert.Equal(ViewCubePart.OrientationSelector, hit.Part);

            // Off the band everything reads as before.
            var centre = cube.HitTest(new Point(width / 2, height / 2));
            Assert.NotEqual(ViewCubePart.OrientationSelector, centre.Part);

            cube.ShowOrientationLabel = false;
            Assert.Equal(ViewCubePart.None, cube.HitTest(new Point(width / 2, height - 4)).Part);
        });
    }

    [Fact]
    public void ShowTooltips_names_the_zone_under_the_pointer()
    {
        RunOnSTA(() =>
        {
            var cube = Show();
            cube.Rotation = ViewCubeMath.RotationFor(ViewCubeMath.Regions.Single(r => r.Name == "FRONT"));
            cube.ShowTooltips = true;
            WpfApplicationHost.Pump();

            // UpdateHover is what a real pointer move lands in; drive it directly so the tooltip
            // assertion does not depend on where the off-screen mouse happens to be.
            typeof(ViewCube)
                .GetMethod("UpdateHover", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
                .Invoke(cube, new object[] { cube.HitTest(new Point(cube.ActualWidth / 2, cube.ActualHeight / 2 + 12)) });

            Assert.Equal("FRONT", ToolTipService.GetToolTip(cube));

            cube.ShowTooltips = false;
            typeof(ViewCube)
                .GetMethod("UpdateHover", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
                .Invoke(cube, new object[] { ViewCubeHit.Empty });

            Assert.Null(ToolTipService.GetToolTip(cube));
        });
    }
}
