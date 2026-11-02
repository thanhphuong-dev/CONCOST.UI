using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using CONCOST.UI.Showcase;
using Xunit;
using Xunit.Abstractions;

namespace CONCOST.UI.Tests;

/// <summary>
/// Builds every Showcase page in-process and lays it out. This is the only test that can catch a
/// crash inside a template: a stack overflow — which is what a template that recurses through an
/// attached-property callback produces — takes the whole process down and cannot be caught by a
/// normal assertion, so the pages are built one by one and the run simply dies on the culprit.
/// It caught exactly that in <c>ScrollViewerHelper.SetupBackToTop</c>, which recursed for ever on
/// any viewer whose template has no <c>PART_BackToTop</c>.
/// </summary>
public class ShowcasePagesTests
{
    private readonly ITestOutputHelper _output;

    public ShowcasePagesTests(ITestOutputHelper output) => _output = output;

    [Fact]
    public void Every_page_builds_and_lays_out()
    {
        WpfApplicationHost.Run(() =>
        {
            // The theme is merged for the duration of this test only: the dictionaries live on
            // Application.Current, which every other test in the process shares, and leaving the
            // library's Generic.xaml there changes how unrelated tests resolve implicit styles.
            var restore = MergeTheme();

            try
            {
                var failures = BuildEveryPage();

                foreach (var failure in failures) _output.WriteLine(failure);

                Assert.True(failures.Count == 0,
                    $"{failures.Count} page(s) failed to build:{Environment.NewLine}" +
                    string.Join(Environment.NewLine, failures));
            }
            finally
            {
                restore();
            }
        });
    }

    private static List<string> BuildEveryPage()
    {
        var viewModel = new ShowcaseViewModel();
        var factories = viewModel.GetType()
            .GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
            .Where(m => m.Name.StartsWith("Create", StringComparison.Ordinal) && m.GetParameters().Length == 0)
            .OrderBy(m => m.Name, StringComparer.Ordinal)
            .ToList();

        Assert.True(factories.Count > 100, $"only {factories.Count} page factories found");

        var failures = new List<string>();

        foreach (var factory in factories)
        {
            try
            {
                var page = factory.Invoke(viewModel, null);
                if (page is not FrameworkElement element) continue;

                // A page that never measures never applies its templates, which is where the
                // recursion and the bad token live, so the layout pass is the point of the test.
                element.Measure(new Size(1000, 2000));
                element.Arrange(new Rect(0, 0, 1000, 2000));
                element.UpdateLayout();
            }
            catch (TargetInvocationException ex)
            {
                var inner = ex.InnerException ?? ex;
                failures.Add($"{factory.Name}: {inner.GetType().Name}: {inner.Message}");
            }
            catch (Exception ex)
            {
                failures.Add($"{factory.Name}: {ex.GetType().Name}: {ex.Message}");
            }
        }

        return failures;
    }

    /// <summary>
    /// Several pages read tokens and styles straight out of <see cref="Application.Current"/>'s
    /// resources with an indexer rather than a binding, so the two dictionaries the app merges have
    /// to be there before a page is built. Returns an action that takes them back out.
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
            var alreadyThere = resources.MergedDictionaries.Any(d =>
                d.Source is not null && d.Source.OriginalString.EndsWith(fileName, StringComparison.OrdinalIgnoreCase));

            if (alreadyThere) return;

            var dictionary = new ResourceDictionary
            {
                Source = new Uri($"pack://application:,,,/CONCOST.UI;component/Themes/{fileName}", UriKind.Absolute)
            };

            resources.MergedDictionaries.Add(dictionary);
            added.Add(dictionary);
        }
    }
}
