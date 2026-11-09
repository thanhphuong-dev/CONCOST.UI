using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using CONCOST.UI.Controls.Ribbon;
using Xunit;

namespace CONCOST.UI.Tests;

public class RibbonContextualTabTests
{
    private static void RunOnSta(Action action) => WpfApplicationHost.Run(action);

    private static RibbonTabItem Tab(string header, string id) =>
        new() { Header = header, Id = id, Content = new List<RibbonGroup>() };

    [Fact]
    public void ContextualGroup_Header_Change_Updates_Tabs()
    {
        RunOnSta(() =>
        {
            var sketchGroup = new RibbonContextualTabGroup
            {
                Header = "Old Header",
                Accent = Brushes.Crimson
            };
            var sketchTab = Tab("Sketch", "sketch");
            sketchGroup.Items.Add(sketchTab);

            Assert.Equal("Old Header", sketchTab.ContextualGroup);

            // Change header dynamically
            sketchGroup.Header = "New Header";

            Assert.Equal("New Header", sketchTab.ContextualGroup);
        });
    }

    [Fact]
    public void ContextualGroup_AutoActivate_Selects_ContextualTab_And_Restores_Previous()
    {
        RunOnSta(() =>
        {
            var homeTab = Tab("Home", "home");
            var viewTab = Tab("View", "view");

            var sketchGroup = new RibbonContextualTabGroup
            {
                Header = "Drawing Tools",
                Accent = Brushes.Crimson,
                Visibility = Visibility.Collapsed
            };
            var sketchTab = Tab("Sketch", "sketch");
            sketchGroup.Items.Add(sketchTab);

            var ribbon = new Ribbon
            {
                ItemsSource = new object[] { homeTab, sketchGroup, viewTab }
            };

            // Initially on Home tab
            Assert.Same(homeTab, ribbon.SelectedItem);

            // 1. Contextual group becomes visible -> auto activates sketchTab
            sketchGroup.Visibility = Visibility.Visible;
            Assert.Equal(3, ribbon.VisibleTabs.Count);
            Assert.Same(sketchTab, ribbon.SelectedItem);

            // 2. Contextual group collapses -> restores homeTab
            sketchGroup.Visibility = Visibility.Collapsed;
            Assert.Equal(2, ribbon.VisibleTabs.Count);
            Assert.Same(homeTab, ribbon.SelectedItem);
        });
    }

    [Fact]
    public void ContextualGroup_Preserves_ViewTab_When_AutoActivate_False()
    {
        RunOnSta(() =>
        {
            var homeTab = Tab("Home", "home");
            var viewTab = Tab("View", "view");

            var sketchGroup = new RibbonContextualTabGroup
            {
                Header = "Drawing Tools",
                Accent = Brushes.Crimson,
                AutoActivate = false,
                Visibility = Visibility.Collapsed
            };
            var sketchTab = Tab("Sketch", "sketch");
            sketchGroup.Items.Add(sketchTab);

            var ribbon = new Ribbon
            {
                ItemsSource = new object[] { homeTab, sketchGroup, viewTab }
            };

            ribbon.SelectedTabId = "view";
            Assert.Same(viewTab, ribbon.SelectedItem);

            // Toggle sketch visible with AutoActivate=false: View tab must remain selected,
            // and the contextual tab sits at the far right end of VisibleTabs (index 2).
            sketchGroup.Visibility = Visibility.Visible;
            Assert.Equal(3, ribbon.VisibleTabs.Count);
            Assert.Same(viewTab, ribbon.SelectedItem);
            Assert.Equal(1, ribbon.SelectedIndex);
            Assert.Same(sketchTab, ribbon.VisibleTabs[2]);
            Assert.Equal("Drawing Tools | Sketch", sketchTab.DisplayHeader);
        });
    }

    [Fact]
    public void Ribbon_Items_Collection_Expands_Correctly_Without_ItemsSource()
    {
        RunOnSta(() =>
        {
            var ribbon = new Ribbon();
            var homeTab = Tab("Home", "home");
            var sketchTab = Tab("Sketch", "sketch");

            var group = new RibbonContextualTabGroup
            {
                Header = "Sketch Tools",
                Accent = Brushes.Crimson,
                Visibility = Visibility.Visible
            };
            group.Items.Add(sketchTab);

            ribbon.Items.Add(homeTab);
            ribbon.Items.Add(group);

            Assert.Equal(2, ribbon.Tabs.Count);
            Assert.Equal(2, ribbon.VisibleTabs.Count);
            Assert.Same(homeTab, ribbon.VisibleTabs[0]);
            Assert.Same(sketchTab, ribbon.VisibleTabs[1]);
        });
    }

    [Fact]
    public void ContextualTab_Receives_ContextualColor_From_Group()
    {
        RunOnSta(() =>
        {
            var crimson = Brushes.Crimson;
            var group = new RibbonContextualTabGroup
            {
                Header = "Sketch Tools",
                Accent = crimson
            };
            var tab = Tab("Sketch", "sketch");
            group.Items.Add(tab);

            Assert.True(tab.IsContextual);
            Assert.Same(crimson, tab.ContextualColor);
            Assert.Equal("Sketch Tools", tab.ContextualGroup);
            Assert.Equal("Sketch Tools | Sketch", tab.DisplayHeader);
        });
    }

    [Fact]
    public void ContextualTab_SingleRow_Arranges_Cleanly()
    {
        RunOnSta(() =>
        {
            var ribbon = new Ribbon { Width = 800 };
            var homeTab = Tab("Home", "home");
            var sketchTab = Tab("Sketch", "sketch");
            var sketchGroup = new RibbonContextualTabGroup
            {
                Header = "Modify",
                AutoActivate = true,
                Visibility = Visibility.Visible
            };
            sketchGroup.Items.Add(sketchTab);

            ribbon.ItemsSource = new object[] { homeTab, sketchGroup };
            ribbon.Measure(new Size(800, 200));
            ribbon.Arrange(new Rect(0, 0, 800, 200));

            Assert.Equal(2, ribbon.VisibleTabs.Count);
            Assert.Equal("Modify | Sketch", sketchTab.DisplayHeader);
        });
    }

    [Fact]
    public void QuickAccessPlacement_AboveRibbon_Places_QAT_Above()
    {
        RunOnSta(() =>
        {
            var btn = new RibbonButton { Header = "Save" };
            var group = new RibbonGroup { Header = "History" };
            group.Items.Add(btn);
            var tab = new RibbonTabItem { Header = "Home", Content = new List<RibbonGroup> { group } };

            var ribbon = new Ribbon
            {
                QuickAccessPlacement = RibbonQuickAccessPlacement.AboveRibbon,
                ShowQuickAccessToolBar = true,
                ItemsSource = new[] { tab }
            };

            ribbon.PinToQuickAccess(btn);

            var window = new Window { Content = ribbon, Width = 800, Height = 400 };
            window.Show();
            ribbon.ApplyTemplate();
            ribbon.UpdateLayout();

            var qatAbove = ribbon.Template.FindName("PART_QatAbove", ribbon) as ContentControl;
            Assert.NotNull(qatAbove);
            Assert.Equal(Visibility.Visible, qatAbove!.Visibility);
            var bar = qatAbove.Content as RibbonQuickAccessToolBar;
            Assert.NotNull(bar);
            Assert.True(bar!.Items.Count > 0, $"bar.Items.Count is {bar.Items.Count}");

            window.Close();
        });
    }
}
