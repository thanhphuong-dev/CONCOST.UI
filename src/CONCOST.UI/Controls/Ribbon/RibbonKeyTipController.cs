using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

// CONCOST.UI.Controls.Canvas is a namespace in this library, so the panel needs a distinct alias.
using KeyTipCanvas = System.Windows.Controls.Canvas;

namespace CONCOST.UI.Controls.Ribbon;

/// <summary>
/// The key-tip session: builds one <see cref="RibbonKeyTip"/> per reachable element, matches the
/// letters the user types and runs the command.
/// <para>
/// The first badge pass covers the tabs, the File block and the commands of the tab that is already
/// open, so a command you can see is one letter away. Choosing another tab re-badges: the new tab's
/// commands appear and the tab badges stay live, so a wrong guess costs one letter, not a restart.
/// </para>
/// </summary>
internal sealed class RibbonKeyTipController
{
    private readonly Ribbon _ribbon;
    private readonly KeyTipCanvas _layer;
    private readonly List<Entry> _entries = new();
    private readonly List<RibbonKeyTip> _badges = new();

    private string _typed = string.Empty;

    internal RibbonKeyTipController(Ribbon ribbon, KeyTipCanvas layer)
    {
        _ribbon = ribbon;
        _layer = layer;
    }

    /// <summary>True while badges are on screen.</summary>
    internal bool IsShowing { get; private set; }

    /// <summary>Letters typed so far, upper-cased. Exposed for tests.</summary>
    internal string Typed => _typed;

    /// <summary>Badges currently on screen, in creation order. Exposed for tests.</summary>
    internal IReadOnlyList<RibbonKeyTip> Badges => _badges;

    /// <summary>Opens the session at level 1.</summary>
    internal void Show()
    {
        if (_layer.Parent == null && _layer.TemplatedParent == null) return;

        _typed = string.Empty;
        IsShowing = true;
        KeyTipService.SetIsShowing(_ribbon, true);

        Build();
    }

    /// <summary>Closes the session and removes every badge.</summary>
    internal void Hide()
    {
        if (!IsShowing) return;

        IsShowing = false;
        _typed = string.Empty;
        KeyTipService.SetIsShowing(_ribbon, false);

        _layer.Children.Clear();
        _badges.Clear();
        _entries.Clear();
    }

    /// <summary>
    /// Handles a key while the session is open. Returns true when the ribbon consumed it, so the
    /// host window never sees the letters.
    /// </summary>
    internal bool HandleKey(Key key)
    {
        if (!IsShowing) return false;

        if (key == Key.Escape)
        {
            Hide();
            return true;
        }

        if (key == Key.Back)
        {
            if (_typed.Length > 0)
            {
                _typed = _typed[..^1];
                Filter();
            }

            return true;
        }

        char? typed = ToChar(key);
        if (typed == null) return false;

        string next = _typed + typed.Value;

        var matches = _entries
            .Where(e => e.Tip.StartsWith(next, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (matches.Count == 0)
        {
            // Nothing starts with those letters: keep the session, drop the buffer.
            _typed = string.Empty;
            Filter();
            return true;
        }

        Entry? exact = matches.FirstOrDefault(
            e => string.Equals(e.Tip, next, StringComparison.OrdinalIgnoreCase));

        if (exact != null)
        {
            Activate(exact);
            return true;
        }

        // A longer badge could still match — hold the letters and narrow the badges.
        _typed = next;
        Filter();
        return true;
    }

    private void Activate(Entry entry)
    {
        if (entry.IsTab)
        {
            // Switch tab and keep going. The new tab's content is built during the next layout
            // pass, so the first rebuild badges the strip and a deferred one adds the commands.
            entry.Activate?.Invoke();
            _typed = string.Empty;
            Build();

            _ribbon.Dispatcher.BeginInvoke(new Action(Build), DispatcherPriority.Loaded);
            return;
        }

        Hide();
        entry.Activate?.Invoke();
    }

    private void Filter()
    {
        foreach (Entry entry in _entries)
        {
            if (entry.Badge == null) continue;
            entry.Badge.IsFiltered = entry.Tip.StartsWith(_typed, StringComparison.OrdinalIgnoreCase);
        }
    }

    /// <summary>Rebuilds the badges for the current level.</summary>
    private void Build()
    {
        _layer.Children.Clear();
        _badges.Clear();
        _entries.Clear();

        var specs = new List<Spec>();

        if (_ribbon.ApplicationMenu is RibbonApplicationMenu file)
        {
            specs.Add(new Spec(file, KeyTipService.GetKeyTip(file), "File", isTab: false)
            {
                Activate = () => file.IsOpen = true
            });
        }

        ListBox? strip = _ribbon.TabStrip;
        IReadOnlyList<RibbonTabItem> tabs = _ribbon.VisibleTabs;

        for (int i = 0; i < tabs.Count; i++)
        {
            if (strip?.ItemContainerGenerator.ContainerFromIndex(i) is not FrameworkElement container) continue;
            if (!container.IsVisible) continue;

            RibbonTabItem tab = tabs[i];
            int index = i;

            specs.Add(new Spec(container, KeyTipService.GetKeyTip(tab), tab.Header as string, isTab: true)
            {
                Activate = () => _ribbon.SelectedIndex = index
            });
        }

        if (_ribbon.SelectedItem is RibbonTabItem selected && !_ribbon.IsMinimized)
        {
            foreach (RibbonGroup group in Ribbon.GroupsOf(selected))
            {
                if (!group.IsVisible) continue;

                if (group.DialogLauncherCommand != null)
                {
                    ICommand launcher = group.DialogLauncherCommand;

                    specs.Add(new Spec(group, KeyTipService.GetKeyTip(group), group.Header as string, isTab: false)
                    {
                        AtCorner = true,
                        Activate = () =>
                        {
                            if (launcher.CanExecute(null)) launcher.Execute(null);
                        }
                    });
                }

                foreach (RibbonItem item in group.Items.OfType<RibbonItem>())
                {
                    if (!item.IsVisible || !item.IsEnabled) continue;

                    RibbonItem captured = item;
                    specs.Add(new Spec(item, KeyTipService.GetKeyTip(item), item.Header as string, isTab: false)
                    {
                        Activate = captured.PerformClick
                    });
                }
            }
        }

        AssignTips(specs);

        foreach (Spec spec in specs)
        {
            var entry = new Entry(spec.Target, spec.Tip, spec.IsTab, spec.AtCorner, spec.Activate);
            _entries.Add(entry);
            entry.Badge = CreateBadge(entry);
        }
    }

    private RibbonKeyTip? CreateBadge(Entry entry)
    {
        var badge = new RibbonKeyTip
        {
            Content = entry.Tip,
            Target = entry.Target
        };

        _layer.Children.Add(badge);
        _badges.Add(badge);

        // A Canvas measures its children with infinite room, so one explicit pass gives us the
        // badge size before we decide where to put it.
        badge.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        Size size = badge.DesiredSize;

        Point origin;

        try
        {
            // TransformToVisual, not TransformToAncestor: the badge layer is a sibling of the
            // tab strip and the groups, never an ancestor of them.
            origin = entry.Target.TransformToVisual(_layer).Transform(new Point(0, 0));
        }
        catch (InvalidOperationException)
        {
            // Detached or not yet laid out: leave it at 0,0 rather than taking the ribbon down.
            origin = new Point(0, 0);
        }

        // Sit on a corner of the control, half over it, and never leave the ribbon.
        double left;
        double top;

        if (entry.AtCorner)
        {
            left = origin.X + entry.Target.ActualWidth - size.Width;
            top = origin.Y + entry.Target.ActualHeight - size.Height - 1;
        }
        else
        {
            left = origin.X;
            top = origin.Y - (size.Height / 2);

            if (top < 0) top = origin.Y + 2;
        }

        if (left + size.Width > _layer.ActualWidth && _layer.ActualWidth > 0) left = _layer.ActualWidth - size.Width;
        if (left < 0) left = 0;

        KeyTipCanvas.SetLeft(badge, left);
        KeyTipCanvas.SetTop(badge, top);

        return badge;
    }

    /// <summary>
    /// Gives every spec a badge that is unique inside its level: the explicit text when it is free,
    /// otherwise the first unused letter of the header.
    /// </summary>
    private static void AssignTips(IReadOnlyList<Spec> specs)
    {
        var used = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (Spec spec in specs)
        {
            string explicitTip = (spec.ExplicitTip ?? string.Empty).Trim().ToUpperInvariant();

            if (explicitTip.Length > 0 && used.Add(explicitTip))
            {
                spec.Tip = explicitTip;
                continue;
            }

            spec.Tip = DeriveUnique(spec.Label, used);
        }
    }

    private static string DeriveUnique(string? label, HashSet<string> used)
    {
        foreach (char c in label ?? string.Empty)
        {
            if (!char.IsLetterOrDigit(c)) continue;

            string candidate = char.ToUpperInvariant(c).ToString();
            if (used.Add(candidate)) return candidate;
        }

        for (char c = 'A'; c <= 'Z'; c++)
        {
            string candidate = c.ToString();
            if (used.Add(candidate)) return candidate;
        }

        for (int i = 1; ; i++)
        {
            string candidate = "K" + i;
            if (used.Add(candidate)) return candidate;
        }
    }

    private static char? ToChar(Key key)
    {
        if (key is >= Key.A and <= Key.Z) return (char)('A' + (key - Key.A));
        if (key is >= Key.D0 and <= Key.D9) return (char)('0' + (key - Key.D0));
        if (key is >= Key.NumPad0 and <= Key.NumPad9) return (char)('0' + (key - Key.NumPad0));

        return null;
    }

    private sealed class Spec
    {
        internal Spec(FrameworkElement target, string? explicitTip, string? label, bool isTab)
        {
            Target = target;
            ExplicitTip = explicitTip;
            Label = label;
            IsTab = isTab;
        }

        /// <summary>Badge sits on the bottom-right corner instead of the top-left.</summary>
        internal bool AtCorner { get; set; }

        internal FrameworkElement Target { get; }
        internal string? ExplicitTip { get; }
        internal string? Label { get; }
        internal bool IsTab { get; }
        internal string Tip { get; set; } = string.Empty;
        internal Action? Activate { get; set; }
    }

    private sealed class Entry
    {
        internal Entry(FrameworkElement target, string tip, bool isTab, bool atCorner, Action? activate)
        {
            Target = target;
            Tip = tip;
            IsTab = isTab;
            AtCorner = atCorner;
            Activate = activate;
        }

        internal bool AtCorner { get; }

        internal FrameworkElement Target { get; }
        internal string Tip { get; }
        internal bool IsTab { get; }
        internal Action? Activate { get; }
        internal RibbonKeyTip? Badge { get; set; }
    }
}
