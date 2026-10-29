using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using CONCOST.UI.Controls;
using Xunit;

namespace CONCOST.UI.Tests;

public class MagnifierTests
{
    private static void RunOnSTA(Action action)
    {
        Exception? exception = null;
        var thread = new Thread(() =>
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                exception = ex;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        if (exception != null)
        {
            throw new AggregateException("STA test failed", exception);
        }
    }

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
}
