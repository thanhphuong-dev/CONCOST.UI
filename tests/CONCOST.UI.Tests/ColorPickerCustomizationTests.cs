using System.Windows;
using CONCOST.UI.Controls;
using Xunit;

namespace CONCOST.UI.Tests;

/// <summary>
/// The picker used to be a fixed 280 x 140 plate with a mandatory hex field and palette. These
/// overrides let a host fit it to a panel without retemplating it.
/// </summary>
public class ColorPickerCustomizationTests
{
    [Fact]
    public void Layout_overrides_have_sane_defaults()
    {
        WpfApplicationHost.Run(() =>
        {
            var picker = new ColorPicker();

            Assert.Equal(new CornerRadius(10), picker.CornerRadius);
            Assert.True(picker.ShowAlpha);
            Assert.True(picker.ShowHexInput);
            Assert.True(picker.ShowPresets);
            Assert.Equal(5, picker.PresetColumns);
            Assert.Equal(22, picker.SwatchSize);
            Assert.Equal(140, picker.FieldHeight);
        });
    }

    [Fact]
    public void Preset_columns_are_clamped()
    {
        WpfApplicationHost.Run(() =>
        {
            var picker = new ColorPicker { PresetColumns = 99 };
            Assert.Equal(12, picker.PresetColumns);

            picker.PresetColumns = 0;
            Assert.Equal(1, picker.PresetColumns);
        });
    }
}
