using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CONCOST.UI.Controls.Ribbon;
using Xunit;

namespace CONCOST.UI.Tests;

/// <summary>
/// The customisation switches on <see cref="Ribbon"/> are template triggers. A DP that is
/// registered but never wired into the template looks correct in the property grid and changes
/// nothing on screen, which is exactly the failure these tests pin down.
/// </summary>
public class RibbonCustomizationTests
{
    private const double W = 900;
    private const double H = 160;

    private static Ribbon MakeRibbon()
    {
        var group = new RibbonGroup { Header = "Edit" };
        group.Items.Add(new RibbonButton { Header = "Undo", Size = RibbonItemSize.Small });

        var home = new RibbonTabItem { Header = "Home", Id = "home" };
        home.Content = new List<RibbonGroup> { group };

        var insert = new RibbonTabItem { Header = "Insert", Id = "insert" };
        insert.Content = new List<RibbonGroup> { new RibbonGroup { Header = "Media" } };

        return new Ribbon
        {
            Width = W,
            Height = H,
            ApplicationMenu = new RibbonApplicationMenu { Header = "File" },
            ItemsSource = new object[] { home, insert }
        };
    }

    private static void Host(Ribbon ribbon)
    {
        var host = new Border
        {
            Child = ribbon,
            Width = W,
            Height = H,
            Background = Brushes.White
        };

        host.Resources.MergedDictionaries.Add(new ResourceDictionary
        {
            Source = new Uri("pack://application:,,,/CONCOST.UI;component/Themes/ThemeResources.xaml", UriKind.Absolute)
        });

        host.Measure(new Size(W, H));
        host.Arrange(new Rect(0, 0, W, H));
        host.UpdateLayout();

        ribbon.ApplyTemplate();
        ribbon.UpdateLayout();
    }

    private static FrameworkElement Part(Ribbon ribbon, string name) =>
        (FrameworkElement)ribbon.Template.FindName(name, ribbon);

    [Fact]
    public void ShowTabStrip_collapses_the_strip()
    {
        WpfApplicationHost.Run(() =>
        {
            var ribbon = MakeRibbon();
            Host(ribbon);

            var strip = Part(ribbon, "PART_TabScroll");
            Assert.Equal(Visibility.Visible, strip.Visibility);

            ribbon.ShowTabStrip = false;
            ribbon.UpdateLayout();
            Assert.Equal(Visibility.Collapsed, strip.Visibility);

            ribbon.ShowTabStrip = true;
            ribbon.UpdateLayout();
            Assert.Equal(Visibility.Visible, strip.Visibility);
        });
    }

    [Fact]
    public void ShowApplicationMenu_collapses_the_file_block()
    {
        WpfApplicationHost.Run(() =>
        {
            var ribbon = MakeRibbon();
            Host(ribbon);

            var block = Part(ribbon, "AppMenuHost");
            Assert.Equal(Visibility.Visible, block.Visibility);

            ribbon.ShowApplicationMenu = false;
            ribbon.UpdateLayout();
            Assert.Equal(Visibility.Collapsed, block.Visibility);

            ribbon.ShowApplicationMenu = true;
            ribbon.UpdateLayout();
            Assert.Equal(Visibility.Visible, block.Visibility);
        });
    }
}
