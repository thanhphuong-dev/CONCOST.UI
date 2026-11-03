using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CONCOST.UI.Controls;
using Xunit;
using Xunit.Abstractions;

namespace CONCOST.UI.Tests;

/// <summary>
/// The compass existed for three releases with an <c>Angle</c> property the template never read —
/// the letter N was hard-coded at the top and nothing rotated. These tests pin the rebuild: the
/// coerce maths, the dial actually turning, and the cardinal letters folding away.
/// </summary>
public class CompassIndicatorTests
{
    private readonly ITestOutputHelper _output;

    public CompassIndicatorTests(ITestOutputHelper output) => _output = output;

    private const int Size = 96;

    [Theory]
    [InlineData(90, 90)]
    [InlineData(450, 90)]
    [InlineData(-90, 270)]
    [InlineData(720, 0)]
    [InlineData(double.NaN, 0)]
    [InlineData(double.PositiveInfinity, 0)]
    public void Angle_coerces_into_zero_to_360(double input, double expected)
    {
        WpfApplicationHost.Run(() =>
        {
            Assert.Equal(expected, new CompassIndicator { Angle = input }.Angle);
        });
    }

    [Fact]
    public void Template_applies_and_owns_the_dial_parts()
    {
        WpfApplicationHost.Run(() =>
        {
            var compass = new CompassIndicator();
            var window = new Window { Content = compass, Width = 120, Height = 120, ShowInTaskbar = false };
            window.Show();
            window.UpdateLayout();
            WpfApplicationHost.Pump();

            Assert.NotNull(compass.Template);
            Assert.NotNull(compass.Template!.FindName("Cardinals", compass));
        });
    }

    [Fact]
    public void Angle_turns_the_dial()
    {
        var baseline = Render(c => c.Angle = 0);
        var turned = Render(c => c.Angle = 137);

        int diff = CountDifferences(baseline, turned);
        _output.WriteLine($"Angle 0 vs 137: {diff} pixel(s) differ");

        Assert.True(diff > 25, $"Changing Angle changed {diff} pixels — the dial is not rotating.");
    }

    [Fact]
    public void ShowCardinals_false_removes_the_letters_but_keeps_the_dial_turning()
    {
        var lettersOn = Render(c => { c.Angle = 0; c.ShowCardinals = true; });
        var lettersOff = Render(c => { c.Angle = 0; c.ShowCardinals = false; });

        int removed = CountDifferences(lettersOn, lettersOff);
        _output.WriteLine($"Cardinals on vs off: {removed} pixel(s) differ");

        Assert.True(removed > 10, $"Hiding the cardinals changed {removed} pixels — the letters are not bound to the switch.");

        // With the letters gone the needle still tells the angle.
        var offZero = Render(c => { c.Angle = 0; c.ShowCardinals = false; });
        var offTurned = Render(c => { c.Angle = 90; c.ShowCardinals = false; });

        Assert.True(CountDifferences(offZero, offTurned) > 10, "Without letters the dial no longer reacts to Angle.");
    }

    private static byte[] Render(Action<CompassIndicator> configure)
    {
        byte[]? pixels = null;

        WpfApplicationHost.Run(() =>
        {
            var restore = MergeTheme();
            try
            {
                var compass = new CompassIndicator { Width = Size, Height = Size };
                configure(compass);

                var host = new Border
                {
                    Width = Size,
                    Height = Size,
                    Background = Brushes.White,
                    Child = compass
                };

                var window = new Window
                {
                    Content = host,
                    Width = Size + 20,
                    Height = Size + 20,
                    ShowInTaskbar = false,
                    WindowStyle = WindowStyle.None,
                    ShowActivated = false,
                    WindowStartupLocation = WindowStartupLocation.Manual,
                    Left = -4000,
                    Top = -4000
                };
                window.Show();
                window.UpdateLayout();
                WpfApplicationHost.Pump();

                var bitmap = new RenderTargetBitmap(Size, Size, 96, 96, PixelFormats.Pbgra32);
                bitmap.Render(host);

                int stride = Size * 4;
                pixels = new byte[stride * Size];
                bitmap.CopyPixels(pixels, stride, 0);

                window.Content = null;
                window.Close();
            }
            finally
            {
                restore();
            }
        });

        return pixels!;
    }

    private static int CountDifferences(byte[] a, byte[] b)
    {
        int count = 0;
        for (int i = 0; i < a.Length; i += 4)
        {
            if (a[i] != b[i] || a[i + 1] != b[i + 1] || a[i + 2] != b[i + 2] || a[i + 3] != b[i + 3])
                count++;
        }
        return count;
    }

    /// <summary>
    /// The template's brushes and styles live in two dictionaries — ThemeResources.xaml carries the
    /// CONCOST.Brush.* tokens, Generic.xaml the control styles — so both have to be merged before a
    /// template can paint. Takes them back out afterwards: Application.Current is shared with every
    /// other test in the process.
    /// </summary>
    private static Action MergeTheme()
    {
        if (Application.Current is null) return () => { };

        var added = new List<ResourceDictionary>();

        Add("ThemeResources.xaml");
        Add("Generic.xaml");

        return () =>
        {
            foreach (var dictionary in added) Application.Current.Resources.MergedDictionaries.Remove(dictionary);
        };

        void Add(string fileName)
        {
            var resources = Application.Current.Resources;
            foreach (var dictionary in resources.MergedDictionaries)
            {
                if (dictionary.Source?.OriginalString.EndsWith(fileName, StringComparison.OrdinalIgnoreCase) == true)
                    return;
            }

            var merged = new ResourceDictionary
            {
                Source = new Uri($"pack://application:,,,/CONCOST.UI;component/Themes/{fileName}", UriKind.Absolute)
            };
            resources.MergedDictionaries.Add(merged);
            added.Add(merged);
        }
    }
}
