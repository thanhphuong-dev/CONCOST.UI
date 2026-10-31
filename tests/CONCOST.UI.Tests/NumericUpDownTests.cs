using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using CONCOST.UI.Controls;
using Xunit;

namespace CONCOST.UI.Tests;

public class NumericUpDownTests
{
    private static void RunOnSta(Action action)
    {
        Exception? exception = null;
        WpfApplicationHost.Run(() =>
        {
            try { action(); }
            catch (Exception ex) { exception = ex; }
        });
        if (exception is not null) throw new AggregateException("STA test failed", exception);
    }

    private static ResourceDictionary EnsureResources()
    {
        var app = Application.Current;
        if (app == null)
        {
            try { app = new Application(); }
            catch { app = Application.Current; }
        }

        var uri = new System.Uri("pack://application:,,,/CONCOST.UI;component/Themes/Generic.xaml", System.UriKind.Absolute);
        foreach (ResourceDictionary dictionary in app!.Resources.MergedDictionaries)
        {
            if (dictionary.Source == uri) return app.Resources;
        }

        app.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = uri });
        return app.Resources;
    }

    [Fact]
    public void Value_IsBoundedAndRejectsNonFiniteInputs()
    {
        RunOnSta(() =>
        {
            var control = new NumericUpDown { Minimum = 0, Maximum = 10 };
            control.Value = 12;
            Assert.Equal(10, control.Value);
            control.Value = double.NaN;
            Assert.Equal(0, control.Value);
            control.Value = double.PositiveInfinity;
            Assert.Equal(10, control.Value);
        });
    }

    [Fact]
    public void RangeChangesRemainValidAndClampCurrentValue()
    {
        RunOnSta(() =>
        {
            var control = new NumericUpDown { Minimum = 0, Maximum = 20, Value = 15 };
            control.Maximum = 8;
            Assert.Equal(8, control.Value);
            control.Minimum = 12;
            Assert.Equal(12, control.Minimum);
            Assert.Equal(12, control.Maximum);
            Assert.Equal(12, control.Value);
        });
    }

    [Fact]
    public void IncrementAndPrecision_AreCoercedToSafeValues()
    {
        RunOnSta(() =>
        {
            var control = new NumericUpDown();
            control.Increment = double.NaN;
            control.DecimalPlaces = 99;
            Assert.Equal(1, control.Increment);
            Assert.Equal(15, control.DecimalPlaces);
        });
    }

    [Fact]
    public void StepButtons_UseStableDecimalArithmeticAndExposeBoundaryState()
    {
        RunOnSta(() =>
        {
            var resources = EnsureResources();
            var control = new NumericUpDown
            {
                Style = (Style)resources["CONCOST.NumericUpDown.Base"],
                Minimum = 0,
                Maximum = 0.3,
                Increment = 0.1
            };
            control.ApplyTemplate();

            var increase = (ButtonBase)control.Template.FindName("PART_IncreaseButton", control);
            var decrease = (ButtonBase)control.Template.FindName("PART_DecreaseButton", control);
            var textBox = (TextBox)control.Template.FindName("PART_TextBox", control);
            Assert.False(decrease.IsEnabled);
            Assert.True(increase.IsEnabled);

            int valueChangeCount = 0;
            control.ValueChanged += (_, _) => valueChangeCount++;

            increase.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            increase.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            increase.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));

            Assert.Equal(0.3, control.Value, 10);
            Assert.Equal("0.3", textBox.Text);
            Assert.Equal(3, valueChangeCount);
            Assert.False(increase.IsEnabled);
            Assert.True(decrease.IsEnabled);
        });
    }

    [Fact]
    public void FormatString_ControlsDisplayedNumberAndInvalidRangeValuesAreCoerced()
    {
        RunOnSta(() =>
        {
            var resources = EnsureResources();
            var control = new NumericUpDown
            {
                Style = (Style)resources["CONCOST.NumericUpDown.Base"],
                Value = 1234.5,
                FormatString = "N2"
            };
            control.ApplyTemplate();

            var textBox = (TextBox)control.Template.FindName("PART_TextBox", control);
            Assert.Equal(control.Value.ToString("N2", System.Globalization.CultureInfo.CurrentCulture), textBox.Text);

            control.Minimum = -10;
            control.Minimum = double.NaN;
            control.Maximum = 10;
            control.Maximum = double.PositiveInfinity;
            Assert.Equal(double.MinValue, control.Minimum);
            Assert.Equal(double.MaxValue, control.Maximum);
        });
    }

    [Fact]
    public void PercentFormat_ParsesDisplayedPercentBackToNumericValue()
    {
        RunOnSta(() =>
        {
            var resources = EnsureResources();
            var control = new NumericUpDown
            {
                Style = (Style)resources["CONCOST.NumericUpDown.Base"],
                Value = 0.125,
                FormatString = "P1"
            };
            control.ApplyTemplate();
            var textBox = (TextBox)control.Template.FindName("PART_TextBox", control);
            textBox.Text = 0.25.ToString("P1", System.Globalization.CultureInfo.CurrentCulture);
            textBox.RaiseEvent(new RoutedEventArgs(TextBox.LostFocusEvent));

            Assert.Equal(0.25, control.Value, 10);
            Assert.Equal(control.Value.ToString("P1", System.Globalization.CultureInfo.CurrentCulture), textBox.Text);
        });
    }
}
