using System;
using System.Threading;
using System.Windows;
using CONCOST.UI.Controls;
using Xunit;

namespace CONCOST.UI.Tests;

public class RangeSliderTests
{
    private static void RunOnSTA(Action action) => WpfApplicationHost.Run(action);

    [Fact]
    public void RangeSlider_Defaults_AreValid()
    {
        RunOnSTA(() =>
        {
            var slider = new RangeSlider();
            Assert.Equal(0.0, slider.Minimum);
            Assert.Equal(100.0, slider.Maximum);
            Assert.Equal(20.0, slider.StartValue);
            Assert.Equal(80.0, slider.EndValue);
            Assert.Equal(1.0, slider.SmallChange);
        });
    }

    [Fact]
    public void RangeSlider_Clamping_PreventsCrossing()
    {
        RunOnSTA(() =>
        {
            var slider = new RangeSlider
            {
                Minimum = 100,
                Maximum = 1000,
                StartValue = 200,
                EndValue = 800
            };

            // Moving StartValue beyond EndValue clamps StartValue to EndValue (800)
            slider.StartValue = 900;
            Assert.Equal(800.0, slider.StartValue);
            Assert.Equal(800.0, slider.EndValue);

            // Moving EndValue below StartValue clamps EndValue to StartValue (800)
            slider.EndValue = 700;
            Assert.Equal(800.0, slider.EndValue);
            Assert.Equal(800.0, slider.StartValue);
        });
    }

    [Fact]
    public void RangeSlider_ValueChanged_EventFires()
    {
        RunOnSTA(() =>
        {
            var slider = new RangeSlider
            {
                Minimum = 0,
                Maximum = 500,
                StartValue = 50,
                EndValue = 250
            };

            int eventCount = 0;
            slider.ValueChanged += (s, e) =>
            {
                eventCount++;
            };

            slider.StartValue = 80;
            slider.EndValue = 300;

            Assert.True(eventCount >= 2);
        });
    }
}
