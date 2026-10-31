using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using CONCOST.UI.AttachedProperties;
using Xunit;

namespace CONCOST.UI.Tests;

/// <summary>
/// The searchable combo box: typing in the drop-down narrows the list, the bound source is never
/// modified, and the selection box stays read-only so the user can only pick a value.
/// </summary>
public class ComboBoxSearchTests
{
    private static void RunOnSTA(Action action) => WpfApplicationHost.Run(action);

    /// <summary>The pop-up closes on a dispatcher pass, so DropDownClosed needs one.</summary>
    private static void Pump()
    {
        var frame = new DispatcherFrame();
        Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.ContextIdle,
            new Action(() => frame.Continue = false));
        Dispatcher.PushFrame(frame);
    }

    public class Grade
    {
        public string Name { get; set; } = string.Empty;

        public override string ToString() => Name;
    }

    private static ComboBox Combo(IEnumerable<object> items, string? displayMember = null)
    {
        var combo = new ComboBox { ItemsSource = items.ToList(), DisplayMemberPath = displayMember };
        ComboBoxHelper.SetIsSearchEnabled(combo, true);
        return combo;
    }

    [Fact]
    public void Search_narrows_the_list_without_touching_the_source()
    {
        RunOnSTA(() =>
        {
            var source = new List<object> { "Concrete C30", "Concrete C35", "Steel CB400" };
            var combo = Combo(source);

            ComboBoxHelper.SetSearchText(combo, "concrete");

            Assert.Equal(2, combo.Items.Count);
            Assert.Equal(3, source.Count);
        });
    }

    [Fact]
    public void Search_is_case_insensitive_and_matches_anywhere()
    {
        RunOnSTA(() =>
        {
            var combo = Combo(new List<object> { "Concrete C30", "Concrete C35", "Steel CB400" });

            ComboBoxHelper.SetSearchText(combo, "cb400");

            Assert.Single(combo.Items);
            Assert.Equal("Steel CB400", combo.Items[0]);
        });
    }

    [Fact]
    public void Search_reads_the_display_member_path()
    {
        RunOnSTA(() =>
        {
            var grades = new List<object>
            {
                new Grade { Name = "C30" },
                new Grade { Name = "C35" },
                new Grade { Name = "Steel" }
            };

            var combo = Combo(grades, nameof(Grade.Name));

            ComboBoxHelper.SetSearchText(combo, "c3");

            Assert.Equal(2, combo.Items.Count);
        });
    }

    [Fact]
    public void Clearing_the_search_brings_every_item_back()
    {
        RunOnSTA(() =>
        {
            var combo = Combo(new List<object> { "Alpha", "Beta", "Gamma" });

            ComboBoxHelper.SetSearchText(combo, "be");
            Assert.Single(combo.Items);

            ComboBoxHelper.SetSearchText(combo, string.Empty);

            Assert.Equal(3, combo.Items.Count);
            Assert.Null(combo.Items.Filter);
        });
    }

    [Fact]
    public void The_selection_box_stays_read_only()
    {
        RunOnSTA(() =>
        {
            var combo = Combo(new List<object> { "Alpha", "Beta" });

            ComboBoxHelper.SetSearchText(combo, "al");

            // The user can narrow the list, but never type a value that is not in it.
            Assert.False(combo.IsEditable);
            Assert.Single(combo.Items);
        });
    }

    [Fact]
    public void Resetting_the_search_brings_every_item_back()
    {
        RunOnSTA(() =>
        {
            var combo = Combo(new List<object> { "Alpha", "Beta", "Gamma" });

            ComboBoxHelper.SetSearchText(combo, "gam");
            Assert.Single(combo.Items);

            // Closing the drop-down does exactly this, so the list is never stale when reopened.
            ComboBoxHelper.ResetSearch(combo);

            Assert.Equal(string.Empty, ComboBoxHelper.GetSearchText(combo));
            Assert.Equal(3, combo.Items.Count);
            Assert.Null(combo.Items.Filter);
        });
    }

    [Fact]
    public void Turning_the_feature_off_restores_the_full_list()
    {
        RunOnSTA(() =>
        {
            var combo = Combo(new List<object> { "Alpha", "Beta" });

            ComboBoxHelper.SetSearchText(combo, "al");
            Assert.Single(combo.Items);

            ComboBoxHelper.SetIsSearchEnabled(combo, false);

            Assert.Equal(2, combo.Items.Count);
            Assert.Equal(string.Empty, ComboBoxHelper.GetSearchText(combo));
        });
    }
}
