using System.Windows.Media;
using System.Threading;
using System.Windows.Controls;
using CONCOST.UI.Controls.Engineering;
using CONCOST.UI.Controls.Layout;
using CONCOST.UI.Core.Converters;
using CONCOST.UI.Core.Helpers;
using CONCOST.UI.Core.MVVM;
using CONCOST.UI.Core.UndoRedo;
using CONCOST.UI.Icons;
using CONCOST.UI.Services;
using CONCOST.UI.Themes.Tools;
using Xunit;

namespace CONCOST.UI.Tests;

public class CoreAndIconTests
{
    [Fact]
    public void ColorHelper_FromHex_ParsesCorrectly()
    {
        var color = ColorHelper.FromHex("#FF7900");
        Assert.Equal(255, color.R);
        Assert.Equal(121, color.G);
        Assert.Equal(0, color.B);
        Assert.Equal(255, color.A);
    }

    [Fact]
    public void ColorHelper_LightenAndDarken_Work()
    {
        var baseColor = Color.FromRgb(100, 100, 100);
        var lighter = ColorHelper.Lighten(baseColor, 0.5);
        var darker = ColorHelper.Darken(baseColor, 0.5);

        Assert.True(lighter.R > baseColor.R);
        Assert.True(darker.R < baseColor.R);
    }

    [Fact]
    public void ContrastChecker_CalculatesRatioAccurately()
    {
        var result = ContrastChecker.Check(Colors.Black, Colors.White);
        Assert.True(result.Ratio > 20.0);
        Assert.True(result.PassesAA);
        Assert.True(result.PassesAAA);
    }

    [Fact]
    public void AccentPaletteGenerator_GeneratesFullPalette()
    {
        var palette = AccentPaletteGenerator.Generate(Color.FromRgb(255, 121, 0));
        Assert.NotNull(palette);
        Assert.Equal(255, palette.Primary.R);
        Assert.True(palette.PrimaryLight.R >= 255);
    }

    [Fact]
    public void ThemeBuilder_SerializesAndDeserializesJson()
    {
        var theme = ThemeBuilder.Create()
            .WithName("OceanTheme")
            .WithPrimary(Colors.DodgerBlue)
            .WithCornerRadius(8.0)
            .Build();

        string json = ThemeBuilder.Create().WithPrimary(Colors.DodgerBlue).ToJson();
        var deserialized = ThemeBuilder.FromJson(json);

        Assert.NotNull(deserialized);
    }

    [Fact]
    public void ThemeManager_AppliesConfiguredPreset()
    {
        var manager = UI.Themes.ThemeManager.Current;
        manager.ApplyPreset(UI.Themes.ThemePreset.Ocean);

        Assert.Equal(UI.Themes.ThemePreset.Ocean, manager.Preset);
        Assert.Equal(System.Windows.Media.Color.FromRgb(2, 132, 199), manager.AccentColor);
    }

    [Fact]
    public void ThemeManager_AppliesThemeAndTokensToPluginRootWithoutApplication()
    {
        Exception? exception = null;
        WpfApplicationHost.Run(() =>
        {
            try
            {
                var root = new Grid();
                var manager = UI.Themes.ThemeManager.Current;

                manager.ApplyTheme(root, UI.Themes.AppTheme.Light);
                var lightBrush = Assert.IsType<SolidColorBrush>(root.TryFindResource("CONCOST.Brush.TextPrimary"));
                Assert.Equal(Color.FromRgb(15, 23, 42), lightBrush.Color);

                manager.ApplyTheme(root, UI.Themes.AppTheme.Dark);
                var darkBrush = Assert.IsType<SolidColorBrush>(root.TryFindResource("CONCOST.Brush.TextPrimary"));
                Assert.Equal(Color.FromRgb(248, 250, 252), darkBrush.Color);
                Assert.Single(root.Resources.MergedDictionaries);

                var accent = Color.FromRgb(2, 132, 199);
                manager.ApplyAccentColor(root, accent);
                Assert.Equal(accent, Assert.IsType<SolidColorBrush>(root.TryFindResource("CONCOST.Brush.Primary")).Color);

                manager.ApplySurfaceStyle(root, UI.Themes.SurfaceStyle.Elevated);
                Assert.Equal(4.0, root.TryFindResource("CONCOST.Shadow.Depth"));
            }
            catch (Exception ex) { exception = ex; }
        });

        if (exception != null) throw new AggregateException("STA test failed", exception);
    }

    [Fact]
    public void AdaptivePanel_HasDefaultBreakpoints()
    {
        WpfApplicationHost.Run(() =>
        {
            var panel = new AdaptivePanel();
            Assert.True(panel.Breakpoints.Count >= 3);
            Assert.Equal(LayoutMode.Normal, panel.CurrentLayout);
        });
    }

    [Fact]
    public void RelayCommand_ExecutesProperly()
    {
        bool executed = false;
        var cmd = new RelayCommand(() => executed = true);

        Assert.True(cmd.CanExecute(null));
        cmd.Execute(null);
        Assert.True(executed);
    }

    [Fact]
    public void UndoRedoManager_ExecutesAndUndoesCommand()
    {
        var manager = new UndoRedoManager();
        int val = 10;

        var cmd = new ChangeValueCommand<int>("SetVal", v => val = v, val, 20);
        manager.Execute(cmd);

        Assert.Equal(20, val);
        Assert.True(manager.CanUndo);

        manager.Undo();
        Assert.Equal(10, val);
        Assert.True(manager.CanRedo);

        manager.Redo();
        Assert.Equal(20, val);
    }

    [Fact]
    public void IconProvider_ReturnsGeometryForRegisteredIcons()
    {
        var saveGeo = IconProvider.GetGeometry(IconKind.Save);
        var addGeo = IconProvider.GetGeometry(IconKind.Add);
        var rulerGeo = IconProvider.GetGeometry(IconKind.Ruler);

        Assert.NotNull(saveGeo);
        Assert.NotNull(addGeo);
        Assert.NotNull(rulerGeo);
    }

    [Theory]
    [InlineData("4D20", 4, 20, false, true)]
    [InlineData("2Ø16", 2, 16, false, true)]
    [InlineData("4Φ20", 4, 20, false, true)]
    [InlineData("2φ16", 2, 16, false, true)]
    [InlineData("4 phi 20", 4, 20, false, true)]
    [InlineData("2x3D18", 6, 18, false, true)]
    [InlineData("D10@200", 0, 10, true, true)]
    [InlineData("InvalidSyntax", 0, 0, false, false)]
    public void RebarParser_ParsesVariousSyntax(string input, int expectedCount, int expectedDia, bool isStirrup, bool isValid)
    {
        var info = RebarParser.Parse(input);
        Assert.Equal(isValid, info.IsValid);
        if (isValid)
        {
            Assert.Equal(expectedCount, info.Quantity);
            Assert.Equal(expectedDia, info.Diameter);
            Assert.Equal(isStirrup, info.IsStirrup);
            Assert.True(info.TotalAreaMm2 > 0);
        }
    }

    [Fact]
    public void LocalizationManager_ReturnsCorrectTranslations()
    {
        var loc = LocalizationManager.Current;
        loc.Culture = new System.Globalization.CultureInfo("vi-VN");
        Assert.Equal("Lưu", loc["Common.Save"]);

        loc.Culture = new System.Globalization.CultureInfo("en-US");
        Assert.Equal("Save", loc["Common.Save"]);
    }
}
