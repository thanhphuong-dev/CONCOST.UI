using System.Threading;
using System.Windows;
using System.Windows.Controls;
using CONCOST.UI.AttachedProperties;
using Xunit;

namespace CONCOST.UI.Tests;

public class TabControlTests
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

    private static Application EnsureApplication()
    {
        var app = Application.Current;
        if (app == null)
        {
            try { app = new Application(); } catch { app = Application.Current; }
        }

        var uri = new Uri("pack://application:,,,/CONCOST.UI;component/Themes/Generic.xaml", UriKind.Absolute);
        foreach (ResourceDictionary md in app.Resources.MergedDictionaries)
        {
            if (md.Source == uri) return app;
        }

        app.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = uri });
        return app;
    }

    [Fact]
    public void TabControlVariants_ExposeVisibleKeyboardFocus()
    {
        RunOnSta(() =>
        {
            var app = EnsureApplication();

            foreach (var key in new[]
                     {
                         "CONCOST.TabItem.Base", "CONCOST.TabItem.Card", "CONCOST.TabItem.Pills",
                         "CONCOST.TabItem.Capsule", "CONCOST.TabItem.Sliding", "CONCOST.TabItem.Vertical",
                         "CONCOST.TabItem.VerticalRight", "CONCOST.TabItem.Bottom", "CONCOST.TabItem.Chrome"
                     })
            {
                var style = (Style)app.FindResource(key);
                var focusSetter = Assert.Single(style.Setters.OfType<Setter>(), setter => setter.Property == Control.FocusVisualStyleProperty);
                var focusStyle = Assert.IsType<Style>(focusSetter.Value);
                Assert.Equal(typeof(Control), focusStyle.TargetType);
                var templateSetter = Assert.Single(focusStyle.Setters.OfType<Setter>(), setter => setter.Property == Control.TemplateProperty);
                Assert.IsType<ControlTemplate>(templateSetter.Value);
            }
        });
    }

    [Fact]
    public void ChromeCloseCommand_IsAvailableOnlyForClosableTabs()
    {
        RunOnSta(() =>
        {
            var host = new TabControl();
            var closable = new TabItem { Header = "Model.xaml" };
            var pinned = new TabItem { Header = "Overview" };
            TabControlHelper.SetIsClosable(closable, true);
            host.Items.Add(closable);
            host.Items.Add(pinned);
            var window = new Window { Content = host, Width = 320, Height = 180, ShowInTaskbar = false };
            window.Show();
            host.UpdateLayout();

            Assert.True(TabControlHelper.CloseCommand.CanExecute(null, closable));
            Assert.False(TabControlHelper.CloseCommand.CanExecute(null, pinned));

            bool closedFired = false;
            TabControlHelper.AddTabClosedHandler(host, (s, e) => { closedFired = true; });

            // Execute close
            TabControlHelper.CloseCommand.Execute(null, closable);
            host.UpdateLayout();

            Assert.False(host.Items.Contains(closable));
            Assert.True(host.Items.Contains(pinned));
            Assert.True(closedFired);

            window.Close();
        });
    }

    [Fact]
    public void TabClosingEvent_WhenHandled_CancelsTabRemoval()
    {
        RunOnSta(() =>
        {
            var host = new TabControl();
            var closable = new TabItem { Header = "UnsavedWork.calc" };
            TabControlHelper.SetIsClosable(closable, true);
            host.Items.Add(closable);

            // Handler cancels the closing event
            TabControlHelper.AddTabClosingHandler(host, (s, e) =>
            {
                e.Handled = true; // Cancel close
            });

            TabControlHelper.CloseCommand.Execute(null, closable);

            // Verify tab remains in collection because closing was cancelled
            Assert.True(host.Items.Contains(closable));
        });
    }

    [Fact]
    public void TabControlStyles_ExistAndCanApply()
    {
        RunOnSta(() =>
        {
            var app = EnsureApplication();

            var styleKeys = new[]
            {
                "CONCOST.TabControl.Base",
                "CONCOST.TabControl.InLine",
                "CONCOST.TabControl.Card",
                "CONCOST.TabControl.Pills",
                "CONCOST.TabControl.Capsule",
                "CONCOST.TabControl.Segmented",
                "CONCOST.TabControl.Sliding",
                "CONCOST.TabControl.Vertical",
                "CONCOST.TabControl.VerticalRight",
                "CONCOST.TabControl.Bottom",
                "CONCOST.TabControl.Chrome"
            };

            foreach (var key in styleKeys)
            {
                var style = (Style)app.FindResource(key);
                Assert.NotNull(style);

                var tc = new TabControl { Style = style };
                tc.Items.Add(new TabItem { Header = "Tab 1" });
                tc.Items.Add(new TabItem { Header = "Tab 2" });
                Assert.Equal(2, tc.Items.Count);
            }
        });
    }
}
