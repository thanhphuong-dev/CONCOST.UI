using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using CONCOST.UI.Controls.Ribbon;
using Xunit;

namespace CONCOST.UI.Tests;

/// <summary>
/// The four display states, the contextual-tab API, the new item types and the two-level tooltip —
/// the parts of the ribbon that only exist once the shell starts folding it.
/// </summary>
public class RibbonDisplayModeTests
{
    private static void RunOnSta(Action action) => WpfApplicationHost.Run(action);

    private static void Pump()
    {
        var frame = new DispatcherFrame();
        Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.ContextIdle,
            new Action(() => frame.Continue = false));
        Dispatcher.PushFrame(frame);
    }

    private static RibbonTabItem Tab(string header, string id, params RibbonGroup[] groups) =>
        new() { Header = header, Id = id, Content = groups.ToList() };

    private static RibbonGroup Group(string header)
    {
        var group = new RibbonGroup { Header = header };
        group.Items.Add(new RibbonButton { Header = header + " go", Size = RibbonItemSize.Large });
        return group;
    }

    private sealed class Host : IDisposable
    {
        private readonly Window _window;

        internal Host(params RibbonTabItem[] tabs)
        {
            Ribbon = new Ribbon { ItemsSource = tabs };
            _window = new Window
            {
                Width = 900,
                Height = 320,
                Content = Ribbon,
                ShowInTaskbar = false,
                WindowStyle = WindowStyle.None
            };

            _window.Show();
            _window.UpdateLayout();
            Pump();
        }

        internal Ribbon Ribbon { get; }

        public void Dispose()
        {
            _window.Content = null;
            _window.Close();
        }
    }

    // ---------------------------------------------------------------- display mode

    [Fact]
    public void CycleState_walks_the_four_states_and_comes_back()
    {
        RunOnSta(() =>
        {
            using var host = new Host(Tab("Home", "home", Group("File")));

            Assert.Equal(RibbonDisplayMode.Full, host.Ribbon.DisplayMode);

            var seen = new System.Collections.Generic.List<RibbonDisplayMode> { host.Ribbon.DisplayMode };

            for (int i = 0; i < 4; i++)
            {
                host.Ribbon.CycleState();
                seen.Add(host.Ribbon.DisplayMode);
            }

            Assert.Equal(
                new[]
                {
                    RibbonDisplayMode.Full,
                    RibbonDisplayMode.MinimizeToTabs,
                    RibbonDisplayMode.MinimizeToPanelTitles,
                    RibbonDisplayMode.MinimizeToPanelButtons,
                    RibbonDisplayMode.Full
                },
                seen);
        });
    }

    [Fact]
    public void IsMinimized_stays_in_step_with_the_display_mode()
    {
        RunOnSta(() =>
        {
            using var host = new Host(Tab("Home", "home", Group("File")));

            host.Ribbon.DisplayMode = RibbonDisplayMode.MinimizeToTabs;
            Assert.True(host.Ribbon.IsMinimized);

            // The old boolean is still the property a host knows about, so it has to drive the new
            // state as well, not only follow it.
            host.Ribbon.IsMinimized = false;
            Assert.Equal(RibbonDisplayMode.Full, host.Ribbon.DisplayMode);
        });
    }

    [Fact]
    public void Panel_titles_state_keeps_the_caption_and_drops_the_body()
    {
        RunOnSta(() =>
        {
            var group = Group("File");
            using var host = new Host(Tab("Home", "home", group));

            host.Ribbon.DisplayMode = RibbonDisplayMode.MinimizeToPanelTitles;
            host.Ribbon.UpdateLayout();
            Pump();

            Assert.True(group.IsTitleOnly);
            Assert.False(group.IsButtonOnly);
        });
    }

    [Fact]
    public void Panel_buttons_state_turns_each_group_into_one_button()
    {
        RunOnSta(() =>
        {
            var group = Group("File");
            using var host = new Host(Tab("Home", "home", group));

            host.Ribbon.DisplayMode = RibbonDisplayMode.MinimizeToPanelButtons;
            host.Ribbon.UpdateLayout();
            Pump();

            Assert.True(group.IsButtonOnly);
            Assert.False(group.IsTitleOnly);
        });
    }

    [Fact]
    public void Folding_to_tabs_opens_the_overlay_only_once_a_tab_is_clicked()
    {
        RunOnSta(() =>
        {
            using var host = new Host(Tab("Home", "home", Group("File")), Tab("View", "view", Group("Panels")));

            host.Ribbon.DisplayMode = RibbonDisplayMode.MinimizeToTabs;
            host.Ribbon.UpdateLayout();
            Pump();

            Assert.False(host.Ribbon.IsOverlayOpen);

            // Going back to the full ribbon has to shut the overlay: a popup would otherwise float
            // over a body the user has just asked for.
            host.Ribbon.IsOverlayOpen = true;
            host.Ribbon.DisplayMode = RibbonDisplayMode.Full;

            Assert.False(host.Ribbon.IsOverlayOpen);
        });
    }

    // ---------------------------------------------------------------- contextual tabs

    [Fact]
    public void ShowContextualTab_selects_it_and_HideContextualTab_goes_back()
    {
        RunOnSta(() =>
        {
            using var host = new Host(Tab("Home", "home", Group("File")), Tab("View", "view", Group("Panels")));

            host.Ribbon.SelectedTabId = "view";

            var modify = new RibbonTabItem
            {
                Header = "Modify | Walls",
                Id = "modify-walls",
                ContextualGroup = "Modify",
                Content = new[] { Group("Wall") }.ToList()
            };

            host.Ribbon.ShowContextualTab(modify);
            host.Ribbon.UpdateLayout();
            Pump();

            Assert.Equal(3, host.Ribbon.Tabs.Count);
            Assert.Same(modify, host.Ribbon.SelectedItem);

            host.Ribbon.HideContextualTab("modify-walls");
            host.Ribbon.UpdateLayout();
            Pump();

            Assert.Equal(2, host.Ribbon.Tabs.Count);
            Assert.Equal("view", host.Ribbon.SelectedTabId);
        });
    }

    [Fact]
    public void ShowContextualTab_without_activate_leaves_the_current_tab_alone()
    {
        RunOnSta(() =>
        {
            using var host = new Host(Tab("Home", "home", Group("File")), Tab("View", "view", Group("Panels")));

            host.Ribbon.SelectedTabId = "view";

            var modify = new RibbonTabItem
            {
                Header = "Modify | Walls",
                Id = "modify-walls",
                ContextualGroup = "Modify",
                Content = new[] { Group("Wall") }.ToList()
            };

            host.Ribbon.ShowContextualTab(modify, activate: false);

            Assert.Equal(3, host.Ribbon.Tabs.Count);
            Assert.Equal("view", host.Ribbon.SelectedTabId);
        });
    }

    // ---------------------------------------------------------------- new item types

    [Fact]
    public void RibbonRadioGroup_keeps_exactly_one_member_checked()
    {
        RunOnSta(() =>
        {
            var first = new RibbonRadioButton { Header = "Small" };
            var second = new RibbonRadioButton { Header = "Large" };

            var group = new RibbonRadioGroup();
            group.Items.Add(first);
            group.Items.Add(second);

            Assert.Equal(-1, group.SelectedIndex);

            group.SelectedIndex = 0;
            Assert.True(first.IsChecked);
            Assert.False(second.IsChecked);

            group.SelectedIndex = 1;
            Assert.False(first.IsChecked);
            Assert.True(second.IsChecked);
            Assert.Same(second, group.SelectedItem);

            // A member can be ticked by the user too; the container follows rather than fights it.
            first.IsChecked = true;
            Assert.Equal(0, group.SelectedIndex);

            // …and it cannot leave the set with nothing ticked.
            first.IsChecked = false;
            Assert.Equal(0, group.SelectedIndex);
            Assert.True(first.IsChecked);
        });
    }

    [Fact]
    public void RibbonLabel_is_inert_and_owns_a_single_row()
    {
        RunOnSta(() =>
        {
            var label = new RibbonLabel { Header = "Text height" };

            Assert.False(label.Focusable);
            Assert.False(label.IsTabStop);
            Assert.False(label.IsHitTestVisible);
            Assert.Equal(RibbonItemSize.Small, label.Size);
        });
    }

    [Fact]
    public void RibbonGroup_reports_a_slideout_only_when_it_has_one()
    {
        RunOnSta(() =>
        {
            var group = Group("File");
            Assert.False(group.HasSlideOut);

            group.SlideOut = "More options";
            Assert.True(group.HasSlideOut);

            group.SlideOut = null;
            Assert.False(group.HasSlideOut);
            Assert.False(group.IsSlideOutOpen);
        });
    }

    // ---------------------------------------------------------------- split button

    [Fact]
    public void Split_button_body_replays_the_entry_it_last_ran()
    {
        RunOnSta(() =>
        {
            var ran = new System.Collections.Generic.List<string>();

            var menuItem = new RibbonMenuItem
            {
                Header = "Save As",
                Command = new RecordingCommand(() => ran.Add("save-as"))
            };

            var button = new RibbonSplitButton { Header = "Save", RememberLast = true };
            button.Items.Add(menuItem);

            // Nothing picked yet: the body stands for its own command.
            button.PerformClick();
            Assert.Null(button.LastUsedItem);
            Assert.Equal("Save", button.BodyHeader);

            menuItem.PerformClick();

            // In the shell the row lives inside the button's own popup, so the click reaches it by
            // bubbling. Without a template there is no route, so the event is raised through the
            // button the same way the popup would deliver it.
            button.RaiseEvent(new RoutedEventArgs(RibbonMenuItem.ClickEvent, menuItem));

            button.PerformClick();

            Assert.Same(menuItem, button.LastUsedItem);
            Assert.Equal("Save As", button.BodyHeader);
            Assert.Equal(new[] { "save-as", "save-as" }, ran);
        });
    }

    [Fact]
    public void Split_button_without_RememberLast_keeps_its_own_caption()
    {
        RunOnSta(() =>
        {
            var menuItem = new RibbonMenuItem { Header = "Save As" };

            var button = new RibbonSplitButton { Header = "Save", RememberLast = false };
            button.Items.Add(menuItem);

            menuItem.PerformClick();
            button.RaiseEvent(new RoutedEventArgs(RibbonMenuItem.ClickEvent, menuItem));

            Assert.Null(button.LastUsedItem);
            Assert.Equal("Save", button.BodyHeader);
        });
    }

    // ---------------------------------------------------------------- item ladder + tooltip

    [Fact]
    public void ShowItemLabels_false_hides_every_caption_below_it()
    {
        RunOnSta(() =>
        {
            var item = new RibbonButton { Header = "Wall" };
            var group = new RibbonGroup { Header = "Build" };
            group.Items.Add(item);

            using var host = new Host(Tab("Home", "home", group));

            Assert.True(item.IsLabelVisible);

            host.Ribbon.SetValue(Ribbon.ShowItemLabelsProperty, false);
            Pump();

            Assert.False(item.IsLabelVisible);

            host.Ribbon.ClearValue(Ribbon.ShowItemLabelsProperty);
            Pump();

            Assert.True(item.IsLabelVisible);
        });
    }

    [Fact]
    public void An_item_that_never_demotes_keeps_its_large_tile()
    {
        RunOnSta(() =>
        {
            var pinned = new RibbonButton { Header = "Wall", Size = RibbonItemSize.Large, CollapsePriority = 0 };
            var group = new RibbonGroup { Header = "Build" };
            group.Items.Add(pinned);

            using var host = new Host(Tab("Home", "home", group));

            group.SizeMode = RibbonGroupSizeMode.Small;
            host.Ribbon.UpdateLayout();
            Pump();

            Assert.Equal(RibbonItemSize.Large, pinned.EffectiveSize);
        });
    }

    [Fact]
    public void Tooltip_grows_only_when_there_is_a_long_form()
    {
        RunOnSta(() =>
        {
            var plain = new RibbonToolTip { Title = "Save", Description = "Saves the file." };
            Assert.False(plain.CanExpand);

            var rich = new RibbonToolTip { Title = "Save", Description = "Saves the file." };
            rich.ExpandedDescription = "Saves the active drawing under a new name.";
            Assert.True(rich.CanExpand);

            // A footer alone is enough to make the long form worth showing.
            var footer = new RibbonToolTip { Title = "Save" };
            footer.Footer = "Press F1 for more help";
            Assert.True(footer.CanExpand);
        });
    }

    [Fact]
    public void An_item_promotes_its_extended_screen_tip_into_the_tooltip()
    {
        RunOnSta(() =>
        {
            var item = new RibbonButton
            {
                Header = "Wall",
                ScreenTip = "Draws a wall.",
                ExtendedScreenTip = "Draws a wall between two points, using the current wall type.",
                ToolTipFooter = "Press F1 for more help"
            };

            Assert.True(item.ToolTip is RibbonToolTip);

            var tip = (RibbonToolTip)item.ToolTip;
            Assert.Equal("Draws a wall.", tip.Description);
            Assert.Equal("Draws a wall between two points, using the current wall type.", tip.ExpandedDescription);
            Assert.Equal("Press F1 for more help", tip.Footer);
            Assert.True(tip.CanExpand);
        });
    }

    /// <summary>Counts executions and reports itself as always runnable.</summary>
    private sealed class RecordingCommand : System.Windows.Input.ICommand
    {
        private readonly Action _run;

        internal RecordingCommand(Action run) => _run = run;

        public event EventHandler? CanExecuteChanged { add { } remove { } }

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter) => _run();
    }
}
