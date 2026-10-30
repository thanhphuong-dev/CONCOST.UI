using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CONCOST.UI.Controls.Ribbon;
using Xunit;

namespace CONCOST.UI.Tests;

/// <summary>
/// The ribbon layout engine is a pure function of (items, size mode), so it can be tested without
/// a window: build a group, measure it, and read back where the panel put every child.
/// </summary>
public class RibbonLayoutTests
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

    private static RibbonGroupPanel Layout(RibbonGroup group, double width = 1000)
    {
        var host = new Border { Child = group };
        host.Measure(new Size(width, 400));
        host.Arrange(new Rect(0, 0, width, 400));
        host.UpdateLayout();

        return FindPanel(group) ?? throw new InvalidOperationException("RibbonGroupPanel was not created.");
    }

    private static RibbonGroupPanel? FindPanel(DependencyObject root)
    {
        int count = VisualTreeHelper.GetChildrenCount(root);

        for (int i = 0; i < count; i++)
        {
            DependencyObject child = VisualTreeHelper.GetChild(root, i);
            if (child is RibbonGroupPanel panel) return panel;

            RibbonGroupPanel? nested = FindPanel(child);
            if (nested != null) return nested;
        }

        return null;
    }

    private static List<Rect> Bounds(RibbonGroupPanel panel) =>
        panel.Children.Cast<UIElement>()
            .Select(child => new Rect(child.TranslatePoint(new Point(0, 0), panel), child.RenderSize))
            .ToList();

    [Fact]
    public void WideItemsStackInTheSameColumnBand()
    {
        RunOnSta(() =>
        {
            var group = new RibbonGroup();

            var first = new RibbonTextBox { ColumnSpan = 2 };
            var second = new RibbonTextBox { ColumnSpan = 2 };
            group.Items.Add(first);
            group.Items.Add(second);

            List<Rect> bounds = Bounds(Layout(group));

            Assert.Equal(bounds[0].X, bounds[1].X, 3);
            Assert.True(bounds[1].Y > bounds[0].Y, "the second wide item should sit under the first");
            Assert.Equal(2 * 72, bounds[0].Width, 3);
        });
    }

    [Fact]
    public void ASpanningItemNeverOverlapsAnEarlierItem()
    {
        RunOnSta(() =>
        {
            var group = new RibbonGroup();

            // Three one-row items fill the first column, so the wide item has to start column two.
            for (int i = 0; i < 3; i++)
                group.Items.Add(new RibbonButton { Size = RibbonItemSize.Small });

            group.Items.Add(new RibbonTextBox { ColumnSpan = 2 });

            List<Rect> bounds = Bounds(Layout(group));

            Assert.Equal(0, bounds[2].X, 3);
            Assert.Equal(72, bounds[3].X, 3);
            Assert.Equal(0, bounds[3].Y, 3);
            Assert.Equal(2 * 72, bounds[3].Width, 3);
        });
    }

    [Fact]
    public void SeparatorStartsItsOwnColumn()
    {
        RunOnSta(() =>
        {
            var group = new RibbonGroup();
            group.Items.Add(new RibbonButton { Size = RibbonItemSize.Small });
            group.Items.Add(new RibbonSeparator());
            group.Items.Add(new RibbonButton { Size = RibbonItemSize.Small });

            List<Rect> bounds = Bounds(Layout(group));

            Assert.Equal(72, bounds[1].X, 3);
            Assert.Equal(9, bounds[1].Width, 3);
            Assert.Equal(81, bounds[2].X, 3);
        });
    }

    [Fact]
    public void GroupDensityCapsItemSize()
    {
        RunOnSta(() =>
        {
            var button = new RibbonButton { Size = RibbonItemSize.Large };
            var group = new RibbonGroup { SizeMode = RibbonGroupSizeMode.Medium };
            group.Items.Add(button);

            Layout(group);

            Assert.Equal(RibbonItemSize.Medium, button.EffectiveSize);
        });
    }

    [Fact]
    public void FixedSizeItemKeepsItsHeightWhenTheGroupCompacts()
    {
        RunOnSta(() =>
        {
            var gallery = new RibbonGallery { Size = RibbonItemSize.Large, IsFixedSize = true };
            var group = new RibbonGroup { SizeMode = RibbonGroupSizeMode.Small };
            group.Items.Add(gallery);

            List<Rect> bounds = Bounds(Layout(group));

            Assert.Equal(RibbonItemSize.Large, gallery.EffectiveSize);
            Assert.True(bounds[0].Height > 3 * 24, "a fixed-size gallery keeps all three rows");
        });
    }

    [Fact]
    public void ComboSelectionKeepsItemAndIndexInStep()
    {
        RunOnSta(() =>
        {
            var combo = new RibbonComboBox
            {
                ItemsSource = new List<string> { "mm", "cm", "m" }
            };

            combo.SelectedItem = "cm";
            Assert.Equal(1, combo.SelectedIndex);

            combo.SelectedIndex = 2;
            Assert.Equal("m", combo.SelectedItem);

            combo.SelectedItem = "not in the list";
            Assert.Equal(-1, combo.SelectedIndex);
        });
    }

    [Fact]
    public void GalleryRaisesSelectionChangedAndClosesThePanel()
    {
        RunOnSta(() =>
        {
            var items = new List<RibbonGalleryItem>
            {
                new("Steel", null),
                new("Sand", null)
            };

            var gallery = new RibbonGallery { ItemsSource = items, IsDropDownOpen = true };

            int raised = 0;
            gallery.SelectionChanged += (_, _) => raised++;

            gallery.SelectedItem = items[1];

            Assert.Equal(1, raised);
            Assert.False(gallery.IsDropDownOpen);
        });
    }

    [Fact]
    public void CollapsedGroupTurnsASelectionListIntoASubmenu()
    {
        RunOnSta(() =>
        {
            var gallery = new RibbonGallery
            {
                Header = "Styles",
                ItemsSource = new List<RibbonGalleryItem> { new("Steel", null), new("Sand", null) }
            };

            var group = new RibbonGroup();
            group.Items.Add(gallery);

            // The menu is built on first access, exactly when the group collapses.
            group.IsCollapsed = true;
            RibbonMenu menu = group.CollapsedMenu;

            RibbonMenuItem entry = Assert.IsType<RibbonMenuItem>(menu.Items[0]);
            Assert.Equal("Styles", entry.Header);
            Assert.Equal(2, entry.Items.Count);
        });
    }
}
