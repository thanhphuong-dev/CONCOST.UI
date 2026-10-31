using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using CONCOST.UI.Controls;
using Xunit;

namespace CONCOST.UI.Tests;

public class CheckComboBoxTests
{
    private static void RunOnSTA(Action action) => WpfApplicationHost.Run(action);

    private static ResourceDictionary EnsureResources()
    {
        var app = Application.Current;
        if (app == null)
        {
            try { app = new Application(); } catch { app = Application.Current; }
        }

        var uri = new Uri("pack://application:,,,/CONCOST.UI;component/Themes/Generic.xaml", UriKind.Absolute);
        foreach (ResourceDictionary md in app.Resources.MergedDictionaries)
        {
            if (md.Source == uri) return app.Resources;
        }

        var dict = new ResourceDictionary { Source = uri };
        app.Resources.MergedDictionaries.Add(dict);
        return app.Resources;
    }

    [Fact]
    public void CheckComboBox_Template_LoadsAndUpdatesDisplayText()
    {
        RunOnSTA(() =>
        {
            var res = EnsureResources();
            var ccb = new CheckComboBox
            {
                ItemsSource = new[] { "Foundation B2", "Basement B1", "Ground L1" }
            };

            var style = res["CONCOST.CheckComboBox.Base"] as Style;
            Assert.NotNull(style);
            ccb.Style = style;
            ccb.ApplyTemplate();

            Assert.Equal(3, ccb.InternalItems.Count);

            // Initially placeholder
            Assert.Equal("Select items...", ccb.DisplayText);

            // Select 1 item
            ccb.InternalItems[0].IsSelected = true;
            Assert.Equal("Foundation B2", ccb.SelectedValue);
            Assert.Equal("Foundation B2", ccb.DisplayText);

            // Select 2nd item
            ccb.InternalItems[1].IsSelected = true;
            Assert.Equal("Foundation B2, Basement B1", ccb.SelectedValue);
            Assert.Equal("Foundation B2, Basement B1", ccb.DisplayText);

            // Select 3rd item (all items selected)
            ccb.InternalItems[2].IsSelected = true;
            Assert.Equal("All Selected (3)", ccb.DisplayText);

            // Unselect all
            ccb.UnselectAll();
            Assert.Equal(string.Empty, ccb.SelectedValue);
            Assert.Equal("Select items...", ccb.DisplayText);
        });
    }

    [Fact]
    public void CheckComboBox_SelectAll_And_UnselectAll_WorkCorrectly()
    {
        RunOnSTA(() =>
        {
            var res = EnsureResources();
            var ccb = new CheckComboBox
            {
                ItemsSource = new[] { "Column C1", "Column C2", "Beam B1" }
            };

            var style = res["CONCOST.CheckComboBox.Base"] as Style;
            ccb.Style = style;
            ccb.ApplyTemplate();

            Assert.Empty(ccb.SelectedItems);

            ccb.SelectAll();
            Assert.Equal(3, ccb.SelectedItems.Count);
            Assert.Equal("All Selected (3)", ccb.DisplayText);

            ccb.UnselectAll();
            Assert.Empty(ccb.SelectedItems);
            Assert.Equal("Select items...", ccb.DisplayText);
        });
    }

    [Fact]
    public void CheckComboBox_Styles_ExistAndCanApply()
    {
        RunOnSTA(() =>
        {
            var res = EnsureResources();

            var styles = new[]
            {
                "CONCOST.CheckComboBox.Base",
                "CONCOST.CheckComboBox.Pill",
                "CONCOST.CheckComboBox.Compact"
            };

            foreach (var styleKey in styles)
            {
                var style = res[styleKey] as Style;
                Assert.NotNull(style);

                var ccb = new CheckComboBox
                {
                    Style = style,
                    ItemsSource = new[] { "Test 1", "Test 2" }
                };
                ccb.ApplyTemplate();
                Assert.Equal(2, ccb.InternalItems.Count);
            }
        });
    }

    [Fact]
    public void CheckComboBox_DropDownOpen_TogglesState()
    {
        RunOnSTA(() =>
        {
            var res = EnsureResources();
            var ccb = new CheckComboBox
            {
                ItemsSource = new[] { "Item A", "Item B" }
            };

            var style = res["CONCOST.CheckComboBox.Base"] as Style;
            ccb.Style = style;
            ccb.ApplyTemplate();

            Assert.False(ccb.IsDropDownOpen);

            ccb.IsDropDownOpen = true;
            Assert.True(ccb.IsDropDownOpen);

            ccb.IsDropDownOpen = false;
            Assert.False(ccb.IsDropDownOpen);
        });
    }

    [Fact]
    public void CheckComboBox_SelectedItemsPreviewMaxWidth_ConstrainsTemplateTextAndRejectsInvalidValues()
    {
        RunOnSTA(() =>
        {
            var resources = EnsureResources();
            var ccb = new CheckComboBox
            {
                Style = (Style)resources["CONCOST.CheckComboBox.Base"],
                ItemsSource = new[] { "Foundation Basement B2", "Level 2 Floor Framing" },
                SelectedItemsPreviewMaxWidth = 180
            };
            ccb.ApplyTemplate();

            var displayText = (TextBlock)ccb.Template.FindName("PART_DisplayText", ccb);
            Assert.Equal(180, ccb.SelectedItemsPreviewMaxWidth);
            Assert.Equal(180, displayText.MaxWidth);

            ccb.SelectedItemsPreviewMaxWidth = -1;
            Assert.Equal(double.PositiveInfinity, ccb.SelectedItemsPreviewMaxWidth);

            ccb.SelectedItemsPreviewMaxWidth = double.NaN;
            Assert.Equal(double.PositiveInfinity, ccb.SelectedItemsPreviewMaxWidth);
        });
    }
}
