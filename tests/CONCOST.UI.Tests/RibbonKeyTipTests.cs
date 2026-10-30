using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using CONCOST.UI.Controls.Ribbon;
using Xunit;

namespace CONCOST.UI.Tests;

/// <summary>
/// R6: key tips, state persistence, the launcher row and the automation peers.
/// <para>
/// A key-tip session only badges what is actually on screen — it asks every element
/// <see cref="UIElement.IsVisible"/>, which is false without a presentation source — so these
/// tests host the ribbon in a real window and close it again in the same STA turn.
/// </para>
/// </summary>
public class RibbonKeyTipTests
{
    private static void RunOnSta(Action action)
    {
        Exception? exception = null;
        var thread = new Thread(() =>
        {
            try { action(); }
            catch (Exception ex) { exception = ex; }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
        if (exception is not null) throw new AggregateException("STA test failed", exception);
    }

    /// <summary>A ribbon inside a shown window; disposing closes the window on the same thread.</summary>
    private sealed class Host : IDisposable
    {
        private readonly Window _window;

        internal Host(params RibbonTabItem[] tabs)
        {
            Ribbon = new Ribbon { ItemsSource = tabs };
            _window = new Window
            {
                Width = 1000,
                Height = 320,
                Content = Ribbon,
                ShowInTaskbar = false,
                WindowStyle = WindowStyle.None
            };

            _window.Show();
            _window.UpdateLayout();
        }

        internal Ribbon Ribbon { get; }

        public void Dispose()
        {
            _window.Content = null;
            _window.Close();
        }
    }

    /// <summary>Runs everything the ribbon queued, e.g. the badge pass after a tab switch.</summary>
    private static void Pump()
    {
        var frame = new DispatcherFrame();
        Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.ContextIdle,
            new Action(() => frame.Continue = false));
        Dispatcher.PushFrame(frame);
    }

    private static RibbonTabItem Tab(string header, params RibbonGroup[] groups) =>
        new() { Header = header, Content = groups.ToList() };

    private static RibbonGroup Group(string header, params RibbonItem[] items)
    {
        var group = new RibbonGroup { Header = header };

        foreach (RibbonItem item in items) group.Items.Add(item);

        return group;
    }

    private static List<RibbonKeyTip> Badges(DependencyObject root)
    {
        var found = new List<RibbonKeyTip>();

        void Walk(DependencyObject node)
        {
            int count = VisualTreeHelper.GetChildrenCount(node);

            for (int i = 0; i < count; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(node, i);
                if (child is RibbonKeyTip badge) found.Add(badge);

                Walk(child);
            }
        }

        Walk(root);
        return found;
    }

    private static string[] Tips(Ribbon ribbon) =>
        Badges(ribbon).Select(b => b.Content as string ?? string.Empty).ToArray();

    [Fact]
    public void AltBadgesOneEntryPerTab()
    {
        RunOnSta(() =>
        {
            using var host = new Host(
                Tab("Home", Group("Font", new RibbonButton { Header = "Bold" })),
                Tab("Insert", Group("Pages", new RibbonButton { Header = "Cover" })));

            host.Ribbon.ShowKeyTips();

            Assert.True(host.Ribbon.IsShowingKeyTips);

            // Tabs first, then the commands of the tab that is already open.
            Assert.Equal(new[] { "H", "I", "B" }, Tips(host.Ribbon));
        });
    }

    [Fact]
    public void TypingATabLetterSwitchesTabAndShowsItsCommands()
    {
        RunOnSta(() =>
        {
            using var host = new Host(
                Tab("Home", Group("Font", new RibbonButton { Header = "Bold" })),
                Tab("Insert", Group("Pages", new RibbonButton { Header = "Cover" })));

            host.Ribbon.ShowKeyTips();
            Assert.True(host.Ribbon.ProcessKeyTipKey(Key.I));
            Pump();

            Assert.Equal(1, host.Ribbon.SelectedIndex);
            Assert.True(host.Ribbon.IsShowingKeyTips);

            // The tab badges stay live; the new tab brings its own commands.
            Assert.Equal(new[] { "H", "I", "C" }, Tips(host.Ribbon));
        });
    }

    [Fact]
    public void TypingACommandLetterRunsItAndClosesTheSession()
    {
        RunOnSta(() =>
        {
            int runs = 0;
            var bold = new RibbonButton { Header = "Bold" };
            bold.Click += (_, _) => runs++;

            using var host = new Host(Tab("Home", Group("Font", bold)));

            host.Ribbon.ShowKeyTips();
            Assert.True(host.Ribbon.ProcessKeyTipKey(Key.B));

            Assert.Equal(1, runs);
            Assert.False(host.Ribbon.IsShowingKeyTips);
            Assert.Empty(Badges(host.Ribbon));
        });
    }

    [Fact]
    public void EscapeClosesTheSessionWithoutRunningAnything()
    {
        RunOnSta(() =>
        {
            int runs = 0;
            var bold = new RibbonButton { Header = "Bold" };
            bold.Click += (_, _) => runs++;

            using var host = new Host(Tab("Home", Group("Font", bold)));

            host.Ribbon.ShowKeyTips();
            Assert.True(host.Ribbon.ProcessKeyTipKey(Key.Escape));

            Assert.False(host.Ribbon.IsShowingKeyTips);
            Assert.Equal(0, runs);
        });
    }

    [Fact]
    public void ExplicitKeyTipsWinAndDuplicatesArePushedAside()
    {
        RunOnSta(() =>
        {
            var bold = new RibbonButton { Header = "Bold", KeyTip = "1" };
            var box = new RibbonCheckBox { Header = "Bullets" };

            using var host = new Host(Tab("Home", Group("Font", bold, box)));

            host.Ribbon.ShowKeyTips();
            string[] tips = Tips(host.Ribbon);

            Assert.Contains("1", tips);
            Assert.Contains("B", tips);   // Bullets gets its own letter, not the taken one
            Assert.Equal(tips.Length, tips.Distinct().Count());
        });
    }

    [Fact]
    public void CollapsedGroupOffersItsLauncherAsAMenuRow()
    {
        RunOnSta(() =>
        {
            bool launched = false;
            var command = new RelayCommand(() => launched = true);

            var group = Group("Font", new RibbonButton { Header = "Bold" });
            group.DialogLauncherCommand = command;
            group.IsUserCollapsed = true;

            var row = group.CollapsedMenu.Items.OfType<RibbonMenuItem>().Last();
            Assert.Equal("Group settings", row.Header);

            row.Command!.Execute(null);
            Assert.True(launched);
        });
    }

    [Fact]
    public void AGroupWithoutALauncherAddsNoMenuRow()
    {
        RunOnSta(() =>
        {
            var group = Group("Font", new RibbonButton { Header = "Bold" });

            Assert.DoesNotContain(group.CollapsedMenu.Items.OfType<RibbonMenuItem>(),
                row => Equals(row.Header, "Group settings"));
        });
    }

    [Fact]
    public void StateSurvivesAJsonRoundTrip()
    {
        RunOnSta(() =>
        {
            var pinned = new RibbonButton { Header = "Save", Id = "save" };
            var folded = Group("Clipboard", new RibbonButton { Header = "Paste" });

            string json;

            using (var first = new Host(
                Tab("Home", Group("File", pinned), folded),
                new RibbonTabItem { Header = "Insert", Id = "insert", Content = new List<RibbonGroup>() }))
            {
                first.Ribbon.SelectedIndex = 1;
                first.Ribbon.IsMinimized = true;
                first.Ribbon.PinToQuickAccess(pinned);
                folded.IsUserCollapsed = true;

                json = RibbonStateStore.ToJson(first.Ribbon.CaptureState());
            }

            using var second = new Host(
                Tab("Home", Group("File", new RibbonButton { Header = "Save", Id = "save" }),
                    Group("Clipboard", new RibbonButton { Header = "Paste" })),
                new RibbonTabItem { Header = "Insert", Id = "insert", Content = new List<RibbonGroup>() });

            second.Ribbon.ApplyState(RibbonStateStore.FromJson(json));

            Assert.Equal(1, second.Ribbon.SelectedIndex);
            Assert.True(second.Ribbon.IsMinimized);

            RibbonItem save = second.Ribbon.AllItems.First(i => i.Id == "save");
            Assert.True(second.Ribbon.IsPinnedToQuickAccess(save));

            var groups = Assert.IsAssignableFrom<IEnumerable<RibbonGroup>>(second.Ribbon.Tabs[0].Content);
            Assert.True(groups.Last().IsUserCollapsed);
        });
    }

    [Fact]
    public void ApplyingAStateFromAnOlderRibbonIgnoresWhatDisappeared()
    {
        RunOnSta(() =>
        {
            var state = new RibbonState { SelectedTabId = "gone", IsMinimized = false };
            state.QuickAccessItemIds.Add("removed");

            using var host = new Host(
                Tab("Home", Group("Font", new RibbonButton { Header = "Bold", Id = "bold" })));

            host.Ribbon.ApplyState(state);

            Assert.Equal(0, host.Ribbon.SelectedIndex);
            Assert.DoesNotContain(host.Ribbon.AllItems, host.Ribbon.IsPinnedToQuickAccess);
        });
    }

    [Fact]
    public void TheButtonPeerInvokesTheCommand()
    {
        RunOnSta(() =>
        {
            int runs = 0;
            var bold = new RibbonButton { Header = "Bold" };
            bold.Click += (_, _) => runs++;

            using var host = new Host(Tab("Home", Group("Font", bold)));

            var peer = UIElementAutomationPeer.CreatePeerForElement(bold);
            Assert.NotNull(peer);
            Assert.Equal(AutomationControlType.Button, peer!.GetAutomationControlType());
            Assert.Equal("Bold", peer.GetName());

            var invoke = Assert.IsAssignableFrom<IInvokeProvider>(peer.GetPattern(PatternInterface.Invoke));
            invoke.Invoke();

            Assert.Equal(1, runs);
        });
    }

    [Fact]
    public void TheTogglePeerReportsAndChangesItsState()
    {
        RunOnSta(() =>
        {
            var box = new RibbonCheckBox { Header = "Bold" };

            using var host = new Host(Tab("Home", Group("Font", box)));

            var peer = (CONCOST.UI.Controls.Ribbon.RibbonToggleButtonAutomationPeer)
                UIElementAutomationPeer.CreatePeerForElement(box)!;

            Assert.Equal(AutomationControlType.CheckBox, peer.GetAutomationControlType());
            Assert.Equal(ToggleState.Off, peer.ToggleState);

            var toggle = Assert.IsAssignableFrom<IToggleProvider>(peer.GetPattern(PatternInterface.Toggle));
            toggle.Toggle();

            Assert.True(box.IsChecked);
            Assert.Equal(ToggleState.On, peer.ToggleState);
        });
    }

    [Fact]
    public void TheGalleryPeerExpandsAndCollapses()
    {
        RunOnSta(() =>
        {
            var gallery = new RibbonGallery();

            using var host = new Host(Tab("Home", Group("Styles", gallery)));

            var peer = (CONCOST.UI.Controls.Ribbon.RibbonGalleryAutomationPeer)
                UIElementAutomationPeer.CreatePeerForElement(gallery)!;

            Assert.Equal(ExpandCollapseState.Collapsed, peer.ExpandCollapseState);

            peer.Expand();
            Assert.True(gallery.IsDropDownOpen);
            Assert.Equal(ExpandCollapseState.Expanded, peer.ExpandCollapseState);

            peer.Collapse();
            Assert.False(gallery.IsDropDownOpen);
        });
    }

    /// <summary>Minimal command for the launcher test.</summary>
    private sealed class RelayCommand : ICommand
    {
        private readonly Action _run;

        internal RelayCommand(Action run) => _run = run;

        public event EventHandler? CanExecuteChanged { add { } remove { } }

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter) => _run();
    }
}
