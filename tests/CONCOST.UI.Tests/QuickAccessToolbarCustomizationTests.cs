using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Shapes;
using CONCOST.UI.AttachedProperties;
using CONCOST.UI.Controls;
using CONCOST.UI.Icons;
using Xunit;

namespace CONCOST.UI.Tests;

/// <summary>
/// The standalone quick-access strip's own metrics. The ribbon's copy of this strip is sized by the
/// ribbon; this one is dropped into a title bar or floated over a drawing, so every number it draws
/// with has to be settable without retemplating it.
/// </summary>
public class QuickAccessToolbarCustomizationTests
{
    private static void RunOnSta(Action action) => WpfApplicationHost.Run(action);

    private static IEnumerable<DependencyObject> Walk(DependencyObject root)
    {
        yield return root;

        int count = VisualTreeHelper.GetChildrenCount(root);
        for (int i = 0; i < count; i++)
        {
            foreach (var child in Walk(VisualTreeHelper.GetChild(root, i))) yield return child;
        }
    }

    private static readonly QuickAccessItem[] Items =
    {
        new(IconKind.Save, "Save", null),
        new(IconKind.Undo, "Undo", null),
        new() { IsSeparator = true },
        new(IconKind.Print, "Print", null)
    };

    /// <summary>Hosts a strip in a real window so its template, panel and layout all run.</summary>
    private sealed class Host : IDisposable
    {
        private readonly Window _window;

        internal Host(QuickAccessToolbar strip)
        {
            Strip = strip;
            _window = new Window
            {
                Width = 420,
                Height = 120,
                Content = strip,
                ShowInTaskbar = false,
                WindowStyle = WindowStyle.None
            };

            _window.Show();
            _window.UpdateLayout();
        }

        internal QuickAccessToolbar Strip { get; }

        internal IEnumerable<T> Find<T>(string? name = null) where T : FrameworkElement =>
            Walk(Strip).OfType<T>().Where(e => name is null || e.Name == name);

        public void Dispose()
        {
            _window.Content = null;
            _window.Close();
        }
    }

    private static QuickAccessToolbar Strip() => new() { ItemsSource = Items };

    // ---------------------------------------------------------------- metrics

    [Fact]
    public void ButtonSize_and_IconSize_reach_every_rendered_button()
    {
        RunOnSta(() =>
        {
            var strip = Strip();
            using var host = new Host(strip);

            strip.ButtonSize = 32;
            strip.IconSize = 18;
            host.Strip.UpdateLayout();

            var buttons = host.Find<Button>().ToList();

            // One button per item — including the separator's, which is collapsed and so has no size.
            Assert.Equal(Items.Length, buttons.Count);
            var drawn = buttons.Where(b => b.Visibility == Visibility.Visible).ToList();
            Assert.Equal(3, drawn.Count);

            Assert.All(drawn, button => Assert.Equal(32, button.Width));
            Assert.All(drawn, button => Assert.Equal(18, IconElement.GetIconSize(button)));

            // The ghost styles carry MinWidth/MinHeight of 34, so Width alone proves nothing: a
            // 32 px request rendered at 34 px until the minimums were bound too.
            Assert.All(drawn, button => Assert.Equal(32, button.ActualWidth));
            Assert.All(drawn, button => Assert.Equal(32, button.ActualHeight));
        });
    }

    [Fact]
    public void Overflow_items_are_drawn_at_the_same_size_as_the_strip()
    {
        RunOnSta(() =>
        {
            var strip = Strip();
            strip.ButtonSize = 28;
            strip.IconSize = 15;

            using var host = new Host(strip);
            host.Strip.UpdateLayout();

            // A popup cannot be opened from a headless host, and measuring its child takes the test
            // process down. The list's template is instantiated on its own instead, with the strip
            // handed to it the same way the popup plate does — that is the whole binding contract.
            var popup = host.Find<Popup>().Single();
            var source = Walk((DependencyObject)popup.Child).OfType<ItemsControl>().First();

            var list = new ItemsControl
            {
                ItemTemplate = source.ItemTemplate,
                ItemsSource = Items,
                DataContext = strip
            };

            var listHost = new Border { Width = 300, Height = 300, Child = list };
            listHost.Measure(new Size(300, 300));
            listHost.Arrange(new Rect(0, 0, 300, 300));
            listHost.UpdateLayout();

            var buttons = Walk(list).OfType<Button>().ToList();
            Assert.Equal(Items.Length, buttons.Count);
            Assert.All(buttons, button => Assert.Equal(28, button.ActualWidth));
            Assert.All(buttons, button => Assert.Equal(15, IconElement.GetIconSize(button)));
        });
    }

    [Fact]
    public void Spacing_reaches_the_overflow_panel()
    {
        RunOnSta(() =>
        {
            var strip = Strip();
            using var host = new Host(strip);

            strip.Spacing = 9;
            host.Strip.UpdateLayout();

            var panel = host.Find<OverflowPanel>().Single();
            Assert.Equal(9, panel.Spacing);
        });
    }

    // ---------------------------------------------------------------- separators

    [Fact]
    public void Separators_follow_the_items_and_the_switch()
    {
        RunOnSta(() =>
        {
            var strip = Strip();
            using var host = new Host(strip);
            host.Strip.UpdateLayout();

            var rule = host.Find<Rectangle>("QuickSeparator")
                .Single(r => r.DataContext is QuickAccessItem { IsSeparator: true });
            Assert.Equal(Visibility.Visible, rule.Visibility);

            // The rule stretches instead of carrying a fixed height, so it grows with ButtonSize.
            strip.ButtonSize = 36;
            host.Strip.UpdateLayout();
            Assert.True(rule.ActualHeight > 20, $"expected the rule to grow, got {rule.ActualHeight}");

            strip.ShowSeparators = false;
            host.Strip.UpdateLayout();
            Assert.Equal(Visibility.Collapsed, rule.Visibility);
        });
    }

    // ---------------------------------------------------------------- variant

    [Fact]
    public void Variant_decides_how_much_plate_is_painted()
    {
        RunOnSta(() =>
        {
            var strip = Strip();
            using var host = new Host(strip);
            host.Strip.UpdateLayout();

            var plate = host.Find<Border>("Plate").Single();

            Assert.Equal(0, plate.BorderThickness.Left);
            Assert.Equal(0, plate.Padding.Left);

            strip.Variant = QuickAccessToolbarVariant.Outlined;
            host.Strip.UpdateLayout();
            Assert.Equal(1, plate.BorderThickness.Left);
            Assert.Equal(3, plate.Padding.Left);
            Assert.Equal(new CornerRadius(6), plate.CornerRadius);

            strip.Variant = QuickAccessToolbarVariant.Elevated;
            host.Strip.UpdateLayout();
            Assert.Equal(1, plate.BorderThickness.Left);
            Assert.Equal(4, plate.Padding.Left);
            Assert.Equal(new CornerRadius(8), plate.CornerRadius);
            Assert.NotNull(plate.Effect);
        });
    }

    // ---------------------------------------------------------------- chevron

    [Fact]
    public void Chevron_shows_for_a_customisable_strip_or_a_real_overflow()
    {
        RunOnSta(() =>
        {
            // Room for everything, and the host asked for the customise chevron.
            var strip = Strip();
            using var host = new Host(strip);
            host.Strip.UpdateLayout();

            var chevron = host.Find<ToggleButton>("MoreButton").Single();
            Assert.Equal(Visibility.Visible, chevron.Visibility);

            // The ghost toggle style carries MinWidth/MinHeight of 34, which used to win over the
            // strip's 24 and left a chevron half again the size of the icons beside it.
            Assert.Equal(strip.ButtonSize, chevron.ActualWidth);
            Assert.Equal(strip.ButtonSize, chevron.ActualHeight);

            strip.ButtonSize = 30;
            host.Strip.UpdateLayout();
            Assert.Equal(30, chevron.ActualWidth);

            // An open menu is a disclosure, not a toggled command: it must not fill with the accent
            // the way a checked RibbonToggleButton does.
            var plate = Walk(chevron).OfType<Border>().First(b => b.Name == "Bd");
            chevron.IsChecked = true;
            host.Strip.UpdateLayout();

            Assert.NotNull(plate.Background);
            if (chevron.TryFindResource("CONCOST.Brush.Primary") is SolidColorBrush accent &&
                plate.Background is SolidColorBrush actual)
            {
                Assert.NotEqual(accent.Color, actual.Color);
            }

            // Same strip, chevron off, nothing hidden: the chevron has nothing to say.
            chevron.IsChecked = false;
            strip.IsCustomizable = false;
            host.Strip.UpdateLayout();
            Assert.Equal(Visibility.Collapsed, chevron.Visibility);

            // Squeezed until the last command no longer fits: it comes back on its own.
            strip.Width = 40;
            host.Strip.UpdateLayout();
            Assert.True(strip.HasOverflow);
            Assert.Equal(Visibility.Visible, chevron.Visibility);
        });
    }
}
