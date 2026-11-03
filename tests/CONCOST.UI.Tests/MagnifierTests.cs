using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using CONCOST.UI.Controls;
using Xunit;

namespace CONCOST.UI.Tests;

public class MagnifierTests
{
    private static void RunOnSTA(Action action) => WpfApplicationHost.Run(action);

    [Fact]
    public void Magnifier_Defaults_AreValid()
    {
        RunOnSTA(() =>
        {
            var m = new Magnifier();
            Assert.Equal(2.5, m.Scale);
            Assert.Equal(60.0, m.Radius);
            Assert.True(m.ShowCrosshair);
            Assert.False(m.IsHitTestVisible);
            Assert.False(m.IsActive);
            Assert.Equal(120.0, m.Width);
            Assert.Equal(120.0, m.Height);
        });
    }

    [Fact]
    public void Magnifier_SettingRadius_UpdatesDimensions()
    {
        RunOnSTA(() =>
        {
            var m = new Magnifier();
            m.Radius = 85.0;

            Assert.Equal(85.0, m.Radius);
            Assert.Equal(170.0, m.Width);
            Assert.Equal(170.0, m.Height);
        });
    }

    [Fact]
    public void Magnifier_InvalidZoomParameters_AreCoercedToSafeValues()
    {
        RunOnSTA(() =>
        {
            var m = new Magnifier { Radius = double.NaN, Scale = 0 };

            Assert.Equal(60.0, m.Radius);
            Assert.Equal(2.5, m.Scale);
            Assert.Equal(120.0, m.Width);
            Assert.Equal(120.0, m.Height);

            m.Scale = 30;
            m.Radius = 700;
            Assert.Equal(12.0, m.Scale);
            Assert.Equal(500.0, m.Radius);
        });
    }

    [Fact]
    public void Magnifier_TargetAttachment_AttachesEventsSafely()
    {
        RunOnSTA(() =>
        {
            var m = new Magnifier();
            var target = new Border { Width = 200, Height = 200 };

            m.Target = target;
            Assert.Same(target, m.Target);

            // Detach target
            m.Target = null;
            Assert.Null(m.Target);
            Assert.False(m.IsActive);
            Assert.Equal(Visibility.Collapsed, m.Visibility);
        });
    }

    // ------------------------------------------------------------------ 1.2.0 upgrade

    [Fact]
    public void Magnifier_NewSwitches_DefaultOnAndReachTheRender()
    {
        RunOnSTA(() =>
        {
            var m = new Magnifier();

            Assert.True(m.ShowRim);
            Assert.True(m.ShowShadow);
            Assert.True(m.FollowPointer);
            Assert.True(m.CrosshairBrush is null);

            // The shadow is the control-level effect the switch swaps in and out.
            Assert.NotNull(m.Effect);
            m.ShowShadow = false;
            Assert.Null(m.Effect);
            m.ShowShadow = true;
            Assert.NotNull(m.Effect);
        });
    }

    [Fact]
    public void Magnifier_FollowPointerOff_KeepsTheHostInChargeOfTheCentre()
    {
        RunOnSTA(() =>
        {
            var magnifier = new Magnifier();
            var target = new Border { Width = 300, Height = 300 };
            magnifier.Target = target;

            var host = new Grid();
            host.Children.Add(target);
            host.Children.Add(magnifier);

            var window = new Window
            {
                Content = host,
                Width = 360,
                Height = 360,
                ShowInTaskbar = false,
                WindowStartupLocation = WindowStartupLocation.Manual,
                Left = -4000,
                Top = -4000
            };
            window.Show();
            window.UpdateLayout();

            try
            {
                magnifier.FollowPointer = false;
                magnifier.Center = new Point(120, 80);
                magnifier.IsActive = true;
                magnifier.Visibility = Visibility.Visible;

                // A pointer move reports through, but neither the centre nor the lens position moves.
                var move = new System.Windows.Input.MouseEventArgs(System.Windows.Input.Mouse.PrimaryDevice, 0)
                {
                    RoutedEvent = UIElement.MouseMoveEvent
                };
                target.RaiseEvent(move);

                Assert.Equal(new Point(120, 80), magnifier.Center);
            }
            finally
            {
                window.Content = null;
                window.Close();
            }
        });
    }
}
