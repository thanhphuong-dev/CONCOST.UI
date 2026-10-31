using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows;
using CONCOST.UI.Controls;
using CONCOST.UI.Icons;
using Xunit;

namespace CONCOST.UI.Tests;

public class DrawerMenuTests
{
    private static void RunOnSTA(Action action) => WpfApplicationHost.Run(action);

    private static ResourceDictionary EnsureResources()
    {
        var app = Application.Current;
        if (app == null)
        {
            try { app = new Application(); } catch { app = Application.Current; }
        }

        var uri = new Uri("pack://application:,,,/CONCOST.UI;component/Themes/Generic.xaml", UriKind.Absolute);
        foreach (ResourceDictionary md in app.Resources.MergedDictionaries)
        {
            if (md.Source == uri) return app.Resources;
        }

        var dict = new ResourceDictionary { Source = uri };
        app.Resources.MergedDictionaries.Add(dict);
        return app.Resources;
    }

    [Fact]
    public void DrawerMenu_Properties_DefaultAndChangeCorrectly()
    {
        RunOnSTA(() =>
        {
            EnsureResources();
            var menu = new DrawerMenu();
            Assert.True(menu.IsOpen);
            Assert.Equal(64.0, menu.CompactWidth);
            Assert.Equal(240.0, menu.ExpandedWidth);

            menu.IsOpen = false;
            Assert.False(menu.IsOpen);
        });
    }

    [Fact]
    public void DrawerMenu_SelectionChanged_FiresWhenSelectedItemChanges()
    {
        RunOnSTA(() =>
        {
            EnsureResources();
            var menu = new DrawerMenu();
            var items = new ObservableCollection<DrawerMenuItem>
            {
                new DrawerMenuItem("Item 1", IconKind.Layers),
                new DrawerMenuItem("Item 2", IconKind.Folder),
                new DrawerMenuItem("Item 3", IconKind.File)
            };
            menu.ItemsSource = items;

            DrawerMenuItem? reportedOld = null;
            DrawerMenuItem? reportedNew = null;
            int changeCount = 0;

            menu.SelectionChanged += (s, e) =>
            {
                changeCount++;
                reportedOld = e.OldValue;
                reportedNew = e.NewValue;
            };

            // Select item 2
            menu.SelectedItem = items[1];

            Assert.Equal(1, changeCount);
            Assert.Null(reportedOld);
            Assert.Same(items[1], reportedNew);
            Assert.True(items[1].IsSelected);

            // Switch to item 3
            menu.SelectedItem = items[2];
            Assert.Equal(2, changeCount);
            Assert.Same(items[1], reportedOld);
            Assert.Same(items[2], reportedNew);
            Assert.False(items[1].IsSelected);
            Assert.True(items[2].IsSelected);
        });
    }

    [Fact]
    public void DrawerMenu_InitialSelection_SynchronizesFromItemsSource()
    {
        RunOnSTA(() =>
        {
            EnsureResources();
            var menu = new DrawerMenu();
            var items = new ObservableCollection<DrawerMenuItem>
            {
                new DrawerMenuItem("Item 1", IconKind.Layers),
                new DrawerMenuItem("Item 2", IconKind.Folder) { IsSelected = true }
            };
            menu.ItemsSource = items;

            Assert.Same(items[1], menu.SelectedItem);
            Assert.True(items[1].IsSelected);
        });
    }
}
