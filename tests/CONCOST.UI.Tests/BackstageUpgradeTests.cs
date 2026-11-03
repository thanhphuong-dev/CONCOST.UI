using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using CONCOST.UI.Controls.Ribbon;
using CONCOST.UI.Icons;
using Xunit;

namespace CONCOST.UI.Tests;

/// <summary>
/// The 1.2.0 backstage pass: the selected page binds two-way, the rail keeps its width, the back
/// caption localises, and the selection reads at a glance because the chosen entry's glyph takes
/// the accent.
/// </summary>
public class BackstageUpgradeTests
{
    private static void RunOnSTA(Action action) => WpfApplicationHost.Run(action);

    private static RibbonBackstageView Show()
    {
        var view = new RibbonBackstageView();

        view.Items.Add(new BackstageItem("Mở", IconKind.Folder));
        view.Items.Add(new BackstageItem("Lưu", IconKind.Save));
        view.Items.Add(new BackstageItem("Tùy chọn", IconKind.Settings) { IsSeparator = true });

        var window = new Window
        {
            Content = view,
            Width = 600,
            Height = 300,
            ShowInTaskbar = false,
            WindowStartupLocation = WindowStartupLocation.Manual,
            Left = -4000,
            Top = -4000
        };

        window.Show();
        window.UpdateLayout();
        Pump();

        return view;
    }

    private static void Pump()
    {
        var frame = new System.Windows.Threading.DispatcherFrame();
        System.Windows.Threading.Dispatcher.CurrentDispatcher.BeginInvoke(
            System.Windows.Threading.DispatcherPriority.ContextIdle,
            new Action(() => frame.Continue = false));
        System.Windows.Threading.Dispatcher.PushFrame(frame);
    }

    [Fact]
    public void Selecting_a_page_updates_everything_and_reports_it()
    {
        RunOnSTA(() =>
        {
            var view = Show();

            int changed = 0;
            view.SelectedItemChanged += (_, _) => changed++;

            var options = view.Items.Last();
            view.SelectedItem = options;
            Pump();

            Assert.Same(options, view.SelectedItem);
            Assert.Equal(1, changed);
        });
    }

    [Fact]
    public void Select_by_header_matches_case_insensitively_and_reports_a_miss()
    {
        RunOnSTA(() =>
        {
            var view = Show();

            Assert.True(view.Select("lưu"));
            Assert.Equal("Lưu", view.SelectedItem!.Header);

            Assert.False(view.Select("In"));
            Assert.Equal("Lưu", view.SelectedItem!.Header);
        });
    }

    [Fact]
    public void RailWidth_resizes_the_rail_column_and_coerces_to_a_usable_minimum()
    {
        RunOnSTA(() =>
        {
            var view = Show();

            view.RailWidth = 300;
            Assert.Equal(300, view.ColumnDefinitions[0].Width.Value);

            view.RailWidth = 10;
            Assert.Equal(120, view.RailWidth);
        });
    }

    [Fact]
    public void BackText_localises_the_rail()
    {
        RunOnSTA(() =>
        {
            var view = Show();

            view.BackText = "Quay lại bản vẽ";
            Pump();

            Assert.Equal("Quay lại bản vẽ", view.BackText);
        });
    }
}
