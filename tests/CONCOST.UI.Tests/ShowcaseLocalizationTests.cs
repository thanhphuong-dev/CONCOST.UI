using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CONCOST.UI.Services;
using CONCOST.UI.Showcase;
using Xunit;

namespace CONCOST.UI.Tests;

/// <summary>
/// The language selector only translated the shell until now: a demo page is built imperatively
/// and cached on its <see cref="ShowcaseItem"/>, so switching culture left the old page on screen.
/// These tests pin the rebuild path and the <c>Loc.T</c> pair used by the CAD cards.
/// </summary>
public class ShowcaseLocalizationTests
{
    private static IEnumerable<DependencyObject> Walk(DependencyObject root)
    {
        yield return root;

        int count = VisualTreeHelper.GetChildrenCount(root);
        for (int i = 0; i < count; i++)
        {
            foreach (var child in Walk(VisualTreeHelper.GetChild(root, i))) yield return child;
        }
    }

    private static List<string> TextOf(object content)
    {
        var page = (FrameworkElement)content;
        page.Measure(new Size(1300, 900));
        page.Arrange(new Rect(0, 0, 1300, 900));
        page.UpdateLayout();

        return Walk(page).OfType<TextBlock>()
            .Select(t => t.Text ?? string.Empty)
            .Where(t => t.Length > 0)
            .ToList();
    }

    [Fact]
    public void Switching_language_rebuilds_the_demo_and_translates_it()
    {
        WpfApplicationHost.Run(() =>
        {
            var previous = LocalizationManager.Current.Culture;

            try
            {
                LocalizationManager.Current.Culture = CultureInfo.GetCultureInfo("vi-VN");

                var vm = new ShowcaseViewModel();
                var item = vm.AllItems.First(i => i.Name == "ViewCube");

                var vietnamese = TextOf(item.Content);
                Assert.Contains("Khối điều hướng Revit ViewCube 3D", vietnamese);

                object before = item.Content;

                LocalizationManager.Current.Culture = CultureInfo.GetCultureInfo("en-US");

                Assert.NotSame(before, item.Content);

                var english = TextOf(item.Content);
                Assert.Contains("Revit ViewCube 3D", english);
                Assert.DoesNotContain("Khối điều hướng Revit ViewCube 3D", english);
            }
            finally
            {
                LocalizationManager.Current.Culture = previous;
            }
        });
    }

    [Fact]
    public void Search_filters_as_text_is_entered_and_updates_the_result_count()
    {
        WpfApplicationHost.Run(() =>
        {
            var previous = LocalizationManager.Current.Culture;

            try
            {
                LocalizationManager.Current.Culture = CultureInfo.GetCultureInfo("en-US");
                var vm = new ShowcaseViewModel();
                int total = vm.AllItems.Count;

                Assert.Equal($"{total} items", vm.CatalogCountText);

                vm.SearchText = "Spreadsheet";
                Assert.True(vm.FilteredItems.Count > 0);
                Assert.Equal($"{vm.FilteredItems.Count} of {total} items", vm.CatalogCountText);

                vm.SearchText = "no-such-component-92841";
                Assert.Empty(vm.FilteredItems);
                Assert.Equal($"0 of {total} items", vm.CatalogCountText);
            }
            finally
            {
                LocalizationManager.Current.Culture = previous;
            }
        });
    }

    [Fact]
    public void Documentation_page_lists_current_targets_themes_and_catalog_count()
    {
        WpfApplicationHost.Run(() =>
        {
            var previous = LocalizationManager.Current.Culture;

            try
            {
                LocalizationManager.Current.Culture = CultureInfo.GetCultureInfo("en-US");
                var vm = new ShowcaseViewModel();
                var documentation = vm.AllItems.First(item => item.Name == "Documentation");
                var text = TextOf(documentation.Content);

                string pageText = string.Join("\n", text);
                Assert.Contains("net48", pageText);
                Assert.Contains("net10.0-windows", pageText);
                Assert.Contains("Light, Soft Light, Dark, Revit", pageText);
                Assert.Contains($"{vm.AllItems.Count} catalog items across 8 categories", pageText);
            }
            finally
            {
                LocalizationManager.Current.Culture = previous;
            }
        });
    }
}
