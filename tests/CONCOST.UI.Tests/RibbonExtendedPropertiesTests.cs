using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;
using CONCOST.UI.Controls.Ribbon;
using Xunit;

namespace CONCOST.UI.Tests;

/// <summary>
/// The properties that let a host bend the ribbon to its own shell: selection by id, the group
/// collapse switch, the locked-down quick-access strip, the shell outline, group captions and the
/// empty state.
/// </summary>
public class RibbonExtendedPropertiesTests
{
    private static void RunOnSta(Action action) => WpfApplicationHost.Run(action);

    /// <summary>Runs whatever the ribbon queued — the collapse ladder works one step per pass.</summary>
    private static void Pump()
    {
        var frame = new DispatcherFrame();
        Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.ContextIdle,
            new Action(() => frame.Continue = false));
        Dispatcher.PushFrame(frame);
    }

    private static RibbonTabItem Tab(string header, string id, params RibbonGroup[] groups) =>
        new() { Header = header, Id = id, Content = groups.ToList() };

    private static RibbonGroup Group(string header, int largeButtons)
    {
        var group = new RibbonGroup { Header = header };

        for (int i = 0; i < largeButtons; i++)
            group.Items.Add(new RibbonButton { Header = header + i, Size = RibbonItemSize.Large });

        return group;
    }

    private sealed class Host : IDisposable
    {
        private readonly Window _window;

        internal Host(double width, params RibbonTabItem[] tabs)
        {
            Ribbon = new Ribbon { ItemsSource = tabs };
            _window = new Window
            {
                Width = width,
                Height = 320,
                Content = Ribbon,
                ShowInTaskbar = false,
                WindowStyle = WindowStyle.None
            };

            _window.Show();
            _window.UpdateLayout();

            // The template is applied on a dispatcher pass; without this the parts are sometimes
            // still missing on a loaded machine and every FindName comes back null.
            Pump();
        }

        internal Ribbon Ribbon { get; }

        internal void Resize(double width)
        {
            _window.Width = width;
            _window.UpdateLayout();
        }

        internal T? Part<T>(string name) where T : class
        {
            if (Ribbon.Template.FindName(name, Ribbon) is T found) return found;

            Pump();
            Ribbon.UpdateLayout();

            return Ribbon.Template.FindName(name, Ribbon) as T;
        }

        public void Dispose()
        {
            _window.Content = null;
            _window.Close();
        }
    }

    // ---------------------------------------------------------------- selection by id

    [Fact]
    public void SelectedTabId_selects_the_matching_tab()
    {
        RunOnSta(() =>
        {
            var ribbon = new Ribbon
            {
                ItemsSource = new[]
                {
                    Tab("Home", "home", Group("File", 1)),
                    Tab("Insert", "insert", Group("Table", 1)),
                    Tab("View", "view", Group("Show", 1))
                }
            };

            ribbon.SelectedTabId = "view";

            Assert.Equal(2, ribbon.SelectedIndex);
            Assert.Equal("view", ribbon.SelectedItem?.Id);
        });
    }

    [Fact]
    public void SelectedTabId_follows_the_selection()
    {
        RunOnSta(() =>
        {
            var ribbon = new Ribbon
            {
                ItemsSource = new[]
                {
                    Tab("Home", "home", Group("File", 1)),
                    Tab("Insert", "insert", Group("Table", 1))
                }
            };

            ribbon.SelectedIndex = 1;

            Assert.Equal("insert", ribbon.SelectedTabId);
        });
    }

    [Fact]
    public void SelectedTabId_ignores_an_unknown_id()
    {
        RunOnSta(() =>
        {
            var ribbon = new Ribbon
            {
                ItemsSource = new[] { Tab("Home", "home", Group("File", 1)) }
            };

            ribbon.SelectedTabId = "nope";

            Assert.Equal(0, ribbon.SelectedIndex);
        });
    }

    // ---------------------------------------------------------------- empty state

    [Fact]
    public void IsEmpty_tracks_the_visible_tab_count()
    {
        RunOnSta(() =>
        {
            var tab = Tab("Home", "home", Group("File", 1));
            var ribbon = new Ribbon { ItemsSource = new[] { tab } };

            Assert.False(ribbon.IsEmpty);

            // A contextual group going away leaves nothing to show.
            tab.IsAvailable = false;
            ribbon.RefreshTabs();

            Assert.True(ribbon.IsEmpty);
        });
    }

    // ---------------------------------------------------------------- collapse switch

    [Fact]
    public void IsGroupCollapseEnabled_false_never_turns_a_group_into_a_button()
    {
        RunOnSta(() =>
        {
            // Three groups of three large buttons each: far wider than the window, so the ladder
            // would normally reach its last rung and collapse groups one by one.
            var groups = new[] { Group("One", 3), Group("Two", 3), Group("Three", 3) };

            using var host = new Host(420, Tab("Home", "home", groups));
            host.Ribbon.IsGroupCollapseEnabled = false;

            for (int i = 0; i < 8; i++)
            {
                host.Ribbon.InvalidateMeasure();
                host.Ribbon.UpdateLayout();
                Pump();
            }

            Assert.All(groups, g => Assert.False(g.IsCollapsed));
        });
    }

    [Fact]
    public void IsGroupCollapseEnabled_true_still_collapses_when_space_runs_out()
    {
        RunOnSta(() =>
        {
            // Three groups of three large buttons each. The window has to be narrower than the
            // icon-only rung as well, otherwise the ladder stops one step short of folding a group.
            var groups = new[] { Group("One", 3), Group("Two", 3), Group("Three", 3) };

            using var host = new Host(240, Tab("Home", "home", groups));

            for (int i = 0; i < 8; i++)
            {
                host.Ribbon.InvalidateMeasure();
                host.Ribbon.UpdateLayout();
                Pump();
            }

            Assert.Contains(groups, g => g.IsCollapsed);
        });
    }

    // ---------------------------------------------------------------- shell chrome

    [Fact]
    public void ShowBorder_false_removes_the_outline()
    {
        RunOnSta(() =>
        {
            using var host = new Host(900, Tab("Home", "home", Group("File", 2)));
            var shell = host.Part<Border>("ShellBorder");

            Assert.NotNull(shell);
            Assert.NotEqual(new Thickness(0), shell!.BorderThickness);

            host.Ribbon.ShowBorder = false;
            host.Ribbon.UpdateLayout();
            Pump();

            Assert.Equal(new Thickness(0), shell.BorderThickness);
        });
    }

    [Fact]
    public void IsQuickAccessCustomizable_false_hides_the_chevron()
    {
        RunOnSta(() =>
        {
            using var host = new Host(900, Tab("Home", "home", Group("File", 2)));

            var quickAccess = host.Ribbon.QuickAccess;
            Assert.NotNull(quickAccess);

            // The chevron only exists once something is pinned, so pin one first.
            quickAccess!.Pin(host.Ribbon.AllItems[0]);
            host.Ribbon.UpdateLayout();
            Pump();

            var chevron = quickAccess.Template.FindName("Chevron", quickAccess) as ToggleButton;
            Assert.NotNull(chevron);
            Assert.Equal(Visibility.Visible, chevron!.Visibility);

            host.Ribbon.IsQuickAccessCustomizable = false;
            host.Ribbon.UpdateLayout();
            Pump();

            Assert.Equal(Visibility.Collapsed, chevron.Visibility);
        });
    }

    [Fact]
    public void IsQuickAccessCustomizable_false_closes_an_open_panel()
    {
        RunOnSta(() =>
        {
            using var host = new Host(900, Tab("Home", "home", Group("File", 2)));

            host.Ribbon.QuickAccess!.IsCustomizeOpen = true;
            Assert.True(host.Ribbon.QuickAccess.IsCustomizeOpen);

            host.Ribbon.IsQuickAccessCustomizable = false;

            Assert.False(host.Ribbon.QuickAccess.IsCustomizeOpen);
        });
    }

    // ---------------------------------------------------------------- group captions

    [Fact]
    public void ShowGroupLabels_false_hides_the_caption_row()
    {
        RunOnSta(() =>
        {
            var group = Group("File", 2);
            using var host = new Host(900, Tab("Home", "home", group));

            Pump();
            var header = group.Template.FindName("HeaderHost", group) as UIElement;
            Assert.NotNull(header);
            Assert.Equal(Visibility.Visible, header!.Visibility);

            var before = group.ActualHeight;

            Ribbon.SetShowGroupLabels(host.Ribbon, false);
            host.Ribbon.UpdateLayout();
            Pump();

            Assert.Equal(Visibility.Collapsed, header.Visibility);
            Assert.True(group.ActualHeight < before, "the caption row should give its height back");
        });
    }

    // ---------------------------------------------------------------- narrow windows

    [Fact]
    public void Narrow_window_overflows_the_tab_strip_and_offers_the_tab_list()
    {
        RunOnSta(() =>
        {
            var tabs = new[]
            {
                Tab("Home", "home", Group("File", 2)),
                Tab("Insert", "insert", Group("Table", 2)),
                Tab("Annotate", "annotate", Group("Text", 2)),
                Tab("View", "view", Group("Show", 2))
            };

            using var host = new Host(1200, tabs);
            Assert.False(host.Ribbon.IsTabStripOverflowing);

            host.Resize(340);
            for (int i = 0; i < 4; i++) { host.Ribbon.UpdateLayout(); Pump(); }

            Assert.True(host.Ribbon.IsTabStripOverflowing);

            var button = host.Part<Button>("TabOverflowButton");
            Assert.NotNull(button);
            Assert.Equal(Visibility.Visible, button!.Visibility);
        });
    }

    [Fact]
    public void Groups_still_collapse_before_the_tabs_overflow()
    {
        RunOnSta(() =>
        {
            // Three wide groups in a 520 px window: the ladder must fold them instead of leaving
            // the row to clip silently.
            var groups = new[] { Group("One", 3), Group("Two", 3), Group("Three", 3) };

            using var host = new Host(520, Tab("Home", "home", groups));

            for (int i = 0; i < 8; i++) { host.Ribbon.InvalidateMeasure(); host.Ribbon.UpdateLayout(); Pump(); }

            Assert.Contains(groups, g => g.IsCollapsed || g.SizeMode != RibbonGroupSizeMode.Full);
        });
    }

    // ---------------------------------------------------------------- menu bar

    [Fact]
    public void MenuBar_spans_the_ribbon_and_can_drop_its_divider()
    {
        RunOnSta(() =>
        {
            using var host = new Host(900, Tab("Home", "home", Group("File", 2)));

            var bar = new RibbonMenuBar();
            bar.Items.Add(new RibbonMenuBarItem { Header = "File" });

            host.Ribbon.MenuBar = bar;
            host.Ribbon.MenuBarPlacement = RibbonMenuBarPlacement.AboveRibbon;
            host.Ribbon.UpdateLayout();
            Pump();
            host.Ribbon.UpdateLayout();

            Assert.True(bar.ActualWidth > 700, $"the bar should stretch with the ribbon, was {bar.ActualWidth}");
            Assert.NotEqual(new Thickness(0), bar.BorderThickness);

            bar.ShowDivider = false;
            host.Ribbon.UpdateLayout();

            Assert.Equal(new Thickness(0), bar.BorderThickness);
        });
    }

    // ---------------------------------------------------------------- empty content

    [Fact]
    public void EmptyContent_is_shown_when_there_is_no_tab()
    {
        RunOnSta(() =>
        {
            using var host = new Host(900);
            host.Ribbon.EmptyContent = "No workspace open.";

            // ItemsSource is empty, so the ribbon is already in its empty state.
            host.Ribbon.UpdateLayout();
            Pump();

            Assert.True(host.Ribbon.IsEmpty);

            var emptyHost = host.Part<ContentPresenter>("EmptyHost");
            Assert.NotNull(emptyHost);
            Assert.Equal(Visibility.Visible, emptyHost!.Visibility);
            Assert.Equal("No workspace open.", emptyHost.Content);
        });
    }
}
