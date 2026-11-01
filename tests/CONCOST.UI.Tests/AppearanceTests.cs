using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using CONCOST.UI.AttachedProperties;
using CONCOST.UI.Controls;
using Xunit;

namespace CONCOST.UI.Tests;

/// <summary>
/// The appearance pack (<see cref="VisualElement"/>), the surface roles (<see cref="SurfaceElement"/>)
/// and the card's extended properties.
/// </summary>
/// <remarks>
/// Every behaviour here is attached rather than templated, so the tests drive it the way a user does:
/// raise the pointer events, flip the property, and check what the element ended up painting.
/// </remarks>
public class AppearanceTests
{
    private static void RunOnSTA(Action action) => WpfApplicationHost.Run(action);

    /// <summary>Hosts an element with the theme merged in, the way a real window would.</summary>
    private static Border Host(FrameworkElement element, double width = 360, double height = 240)
    {
        var host = new Border { Child = element };
        host.Resources.MergedDictionaries.Add(new ResourceDictionary
        {
            Source = new Uri("pack://application:,,,/CONCOST.UI;component/Themes/ThemeResources.xaml", UriKind.Absolute)
        });

        host.Measure(new Size(width, height));
        host.Arrange(new Rect(0, 0, width, height));
        host.UpdateLayout();

        return host;
    }

    private static void Hover(UIElement element)
        => element.RaiseEvent(new MouseEventArgs(Mouse.PrimaryDevice, 0) { RoutedEvent = UIElement.MouseEnterEvent });

    private static void Unhover(UIElement element)
        => element.RaiseEvent(new MouseEventArgs(Mouse.PrimaryDevice, 0) { RoutedEvent = UIElement.MouseLeaveEvent });

    private static void Press(UIElement element)
        => element.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left)
        {
            RoutedEvent = UIElement.PreviewMouseLeftButtonDownEvent
        });

    private static void Release(UIElement element)
        => element.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left)
        {
            RoutedEvent = UIElement.PreviewMouseLeftButtonUpEvent
        });

    // ------------------------------------------------------------------ elevation

    [Fact]
    public void Elevation_AddsThemedShadow_AndZeroTakesItAway()
    {
        RunOnSTA(() =>
        {
            var button = new Button { Content = "Áp dụng" };
            Host(button);

            VisualElement.SetElevation(button, 2);

            var shadow = Assert.IsType<DropShadowEffect>(button.Effect);
            Assert.True(shadow.BlurRadius > 0);
            Assert.True(shadow.ShadowDepth > 0);

            VisualElement.SetElevation(button, 0);

            Assert.Null(button.Effect);
        });
    }

    [Fact]
    public void Elevation_ClampsLevels_SoAHighValueDoesNotBlowUpTheEffect()
    {
        RunOnSTA(() =>
        {
            var panel = new Border();
            Host(panel);

            VisualElement.SetElevation(panel, 99);

            var shadow = Assert.IsType<DropShadowEffect>(panel.Effect);
            Assert.InRange(shadow.BlurRadius, 1, 40);
        });
    }

    [Fact]
    public void ShadowColor_OverridesTheThemeToken()
    {
        RunOnSTA(() =>
        {
            var button = new Button();
            Host(button);

            VisualElement.SetShadowColor(button, Colors.Red);
            VisualElement.SetElevation(button, 1);

            var shadow = Assert.IsType<DropShadowEffect>(button.Effect);
            Assert.Equal(255, shadow.Color.R);
            Assert.Equal(0, shadow.Color.G);
            Assert.Equal(0, shadow.Color.B);
        });
    }

    [Fact]
    public void Elevation_FollowsAThemeSwitch_WithoutTouchingTheShadowsOwner()
    {
        RunOnSTA(() =>
        {
            var panel = new Border();
            Host(panel);

            VisualElement.SetElevation(panel, 1);

            var first = Assert.IsType<DropShadowEffect>(panel.Effect);

            VisualElement.SetElevation(panel, 3);

            var second = Assert.IsType<DropShadowEffect>(panel.Effect);

            // A new effect object, not the same frozen one re-used: the level really was re-built.
            Assert.NotSame(first, second);
            Assert.True(second.BlurRadius > first.BlurRadius);
        });
    }

    // ------------------------------------------------------------------ motion

    [Fact]
    public void HoverScale_GrowsOnEnter_AndReturnsOnLeave()
    {
        RunOnSTA(() =>
        {
            var button = new Button();
            Host(button);

            VisualElement.SetMotionDuration(button, 0);
            VisualElement.SetHoverScale(button, 1.08);

            Hover(button);

            var scale = Assert.IsType<ScaleTransform>(button.RenderTransform);
            Assert.Equal(1.08, scale.ScaleX, 3);
            Assert.Equal(1.08, scale.ScaleY, 3);

            Unhover(button);

            Assert.Equal(1.0, scale.ScaleX, 3);
        });
    }

    [Fact]
    public void PressScale_ShrinksWhileTheButtonIsHeld()
    {
        RunOnSTA(() =>
        {
            var button = new Button();
            Host(button);

            VisualElement.SetMotionDuration(button, 0);
            VisualElement.SetPressScale(button, 0.92);

            Hover(button);
            Press(button);

            var scale = Assert.IsType<ScaleTransform>(button.RenderTransform);
            Assert.Equal(0.92, scale.ScaleX, 3);

            Release(button);

            Assert.Equal(1.0, scale.ScaleX, 3);
        });
    }

    [Fact]
    public void HoverScale_AnimatesWhenADurationIsSet()
    {
        RunOnSTA(() =>
        {
            var button = new Button();
            Host(button);

            VisualElement.SetMotionDuration(button, 200);
            VisualElement.SetHoverScale(button, 1.05);

            Hover(button);

            var scale = Assert.IsType<ScaleTransform>(button.RenderTransform);
            Assert.True(scale.HasAnimatedProperties);
        });
    }

    [Fact]
    public void ScaleOrigin_MovesThePointTheElementGrowsFrom()
    {
        RunOnSTA(() =>
        {
            var tile = new Border();
            Host(tile);

            VisualElement.SetMotionDuration(tile, 0);
            VisualElement.SetScaleOrigin(tile, 0);
            VisualElement.SetHoverScale(tile, 1.05);

            Hover(tile);

            Assert.Equal(new Point(0, 0), tile.RenderTransformOrigin);
        });
    }

    [Fact]
    public void RepeatedPropertyChanges_DoNotStackASecondAnimation()
    {
        RunOnSTA(() =>
        {
            var button = new Button();
            Host(button);

            VisualElement.SetMotionDuration(button, 0);
            VisualElement.SetHoverScale(button, 1.05);
            VisualElement.SetHoverScale(button, 1.05);
            VisualElement.SetHoverScale(button, 1.07);

            Hover(button);

            var scale = Assert.IsType<ScaleTransform>(button.RenderTransform);
            Assert.Equal(1.07, scale.ScaleX, 3);
        });
    }

    // ------------------------------------------------------------------ disabled

    [Fact]
    public void DisabledOpacity_DimsADisabledElement_AndRestoresWhatWasThere()
    {
        RunOnSTA(() =>
        {
            var button = new Button { Opacity = 0.9 };
            Host(button);

            VisualElement.SetDisabledOpacity(button, 0.4);

            button.IsEnabled = false;
            Assert.Equal(0.4, button.Opacity, 3);

            button.IsEnabled = true;

            // The host's own 0.9 comes back, not a hard-coded 1.
            Assert.Equal(0.9, button.Opacity, 3);
        });
    }

    // ------------------------------------------------------------------ focus ring

    [Fact]
    public void FocusRing_InstallsAThemedVisual_RestoresTheOldOneAndNull()
    {
        RunOnSTA(() =>
        {
            var box = new TextBox();
            Host(box);

            Style? original = box.FocusVisualStyle;

            VisualElement.SetFocusRing(box, true);

            Assert.NotNull(box.FocusVisualStyle);
            Assert.NotSame(original, box.FocusVisualStyle);

            VisualElement.SetFocusRing(box, false);

            Assert.Same(original, box.FocusVisualStyle);
        });
    }

    [Fact]
    public void FocusRing_RemovesItself_WhenThereWasNoVisualBefore()
    {
        RunOnSTA(() =>
        {
            // Pinning null locally is the only way to be sure the theme is not supplying a default
            // focus visual for this element type.
            var panel = new Border { FocusVisualStyle = null };
            Host(panel);

            VisualElement.SetFocusRing(panel, true);
            Assert.NotNull(panel.FocusVisualStyle);

            VisualElement.SetFocusRing(panel, false);

            // A null original has to be distinguishable from "not captured yet", which is what
            // left the accent ring stuck on the element when the property was switched off.
            Assert.Null(panel.FocusVisualStyle);
        });
    }

    // ------------------------------------------------------------------ surface roles

    [Fact]
    public void SurfaceRole_PaintsTheThemedBrush_AndGivesTheOldOneBack()
    {
        RunOnSTA(() =>
        {
            var panel = new Border();
            var host = Host(panel);

            panel.Background = Brushes.Pink;

            SurfaceElement.SetRole(panel, SurfaceRole.Sunken);

            var expected = host.Resources["CONCOST.Brush.SurfaceSunken"];
            Assert.Same(expected, panel.Background);

            SurfaceElement.SetRole(panel, SurfaceRole.None);

            Assert.Same(Brushes.Pink, panel.Background);
        });
    }

    [Fact]
    public void SurfaceRole_HandlesPanelsControlsAndTextBlocks()
    {
        RunOnSTA(() =>
        {
            var grid = new Grid();
            var button = new Button();
            var text = new TextBlock { Text = "Ghi chú" };
            var host = Host(new StackPanel { Children = { grid, button, text } });

            var expected = host.Resources["CONCOST.Brush.SurfaceElevated"];

            SurfaceElement.SetRole(grid, SurfaceRole.Elevated);
            SurfaceElement.SetRole(button, SurfaceRole.Elevated);
            SurfaceElement.SetRole(text, SurfaceRole.Elevated);

            Assert.Same(expected, grid.Background);
            Assert.Same(expected, button.Background);
            Assert.Same(expected, text.Background);
        });
    }

    [Fact]
    public void SurfaceBorder_PaintsTheOutline()
    {
        RunOnSTA(() =>
        {
            var panel = new Border { BorderThickness = new Thickness(1) };
            var host = Host(panel);

            SurfaceElement.SetBorder(panel, SurfaceBorder.Strong);

            Assert.Same(host.Resources["CONCOST.Brush.BorderHover"], panel.BorderBrush);
        });
    }

    [Fact]
    public void SurfaceRadius_ReachesABorderDirectly_AndAControlThroughItsTemplate()
    {
        RunOnSTA(() =>
        {
            var panel = new Border();
            var button = new Button();
            Host(new StackPanel { Children = { panel, button } });

            SurfaceElement.SetRadius(panel, new CornerRadius(14));
            SurfaceElement.SetRadius(button, new CornerRadius(9));

            Assert.Equal(new CornerRadius(14), panel.CornerRadius);
            Assert.Equal(new CornerRadius(9), BorderElement.GetCornerRadius(button));

            SurfaceElement.SetRadius(panel, null);

            Assert.Equal(new CornerRadius(0), panel.CornerRadius);
        });
    }

    // ------------------------------------------------------------------ card

    [Fact]
    public void Card_Defaults_KeepTheLookTheyAlwaysHad()
    {
        RunOnSTA(() =>
        {
            var card = new Card { Content = new TextBlock { Text = "Thẻ" } };

            Assert.Equal(1, card.Elevation);
            Assert.False(card.IsInteractive);
            Assert.Equal(CardAccent.None, card.AccentBar);
            Assert.Equal(3.0, card.AccentThickness);
        });
    }

    [Fact]
    public void Card_Elevation_ReachesTheElementItself_NotTheBorderInsideTheTemplate()
    {
        RunOnSTA(() =>
        {
            var card = new Card { Content = new TextBlock { Text = "Thẻ" }, Width = 220 };
            Host(card);

            card.Elevation = 3;

            Assert.IsType<DropShadowEffect>(card.Effect);

            // The template's border must stay flat: the same effect in both places drew the shadow
            // twice and made every card in the application heavier than its elevation.
            var root = Assert.IsType<Border>(VisualTreeHelper.GetChild(card, 0));
            Assert.Null(root.Effect);

            card.Elevation = 0;

            Assert.Null(card.Effect);
        });
    }

    [Fact]
    public void Card_AccentCorners_OnlyRoundTheEdgeTheyTouch()
    {
        RunOnSTA(() =>
        {
            var card = new Card { CornerRadius = new CornerRadius(12) };

            card.AccentBar = CardAccent.Left;
            Assert.Equal(new CornerRadius(12, 0, 0, 12), card.AccentCornerRadius);

            card.AccentBar = CardAccent.Right;
            Assert.Equal(new CornerRadius(0, 12, 12, 0), card.AccentCornerRadius);

            card.AccentBar = CardAccent.Top;
            Assert.Equal(new CornerRadius(12, 12, 0, 0), card.AccentCornerRadius);

            card.AccentBar = CardAccent.Bottom;
            Assert.Equal(new CornerRadius(0, 0, 12, 12), card.AccentCornerRadius);

            card.AccentBar = CardAccent.None;
            Assert.Equal(new CornerRadius(0), card.AccentCornerRadius);
        });
    }

    [Fact]
    public void Card_HeaderAndFooterCorners_FollowTheRadius()
    {
        RunOnSTA(() =>
        {
            var card = new Card { CornerRadius = new CornerRadius(6, 18, 4, 10) };

            Assert.Equal(new CornerRadius(6, 18, 0, 0), card.HeaderCornerRadius);
            Assert.Equal(new CornerRadius(0, 0, 4, 10), card.FooterCornerRadius);

            card.CornerRadius = new CornerRadius(0);

            Assert.Equal(new CornerRadius(0), card.HeaderCornerRadius);
            Assert.Equal(new CornerRadius(0), card.FooterCornerRadius);
        });
    }

    [Fact]
    public void Card_AccentBar_TurnsTheTemplateRuleOn()
    {
        RunOnSTA(() =>
        {
            var card = new Card
            {
                Header = "Dầm phụ D2",
                AccentBar = CardAccent.Top,
                Content = new TextBlock { Text = "Nội dung" },
                Width = 240
            };

            var host = Host(card);
            card.ApplyTemplate();

            var bar = Assert.IsType<Border>(card.Template.FindName("AccentBarH", card));

            Assert.Equal(Visibility.Visible, bar.Visibility);
            Assert.Same(host.Resources["CONCOST.Brush.Primary"], bar.Background);
        });
    }

    [Fact]
    public void Card_AccentBar_MovesToTheEdgeItNames()
    {
        RunOnSTA(() =>
        {
            var card = new Card
            {
                AccentBar = CardAccent.Right,
                Content = new TextBlock { Text = "Nội dung" },
                Width = 200
            };

            Host(card);
            card.ApplyTemplate();

            var bar = Assert.IsType<Border>(card.Template.FindName("AccentBarV", card));

            Assert.Equal(Visibility.Visible, bar.Visibility);
            Assert.Equal(2, Grid.GetColumn(bar));
        });
    }

    [Fact]
    public void Card_Footer_AppearsOnlyWhenThereIsOne()
    {
        RunOnSTA(() =>
        {
            var card = new Card { Content = new TextBlock { Text = "Nội dung" }, Width = 220 };
            var host = Host(card);
            card.ApplyTemplate();

            var footer = Assert.IsType<Border>(card.Template.FindName("FooterBorder", card));
            Assert.Equal(Visibility.Collapsed, footer.Visibility);

            card.Footer = "Bê tông M300";
            host.UpdateLayout();

            Assert.Equal(Visibility.Visible, footer.Visibility);
        });
    }

    [Fact]
    public void Card_AccentBrush_OverridesTheAccentColour()
    {
        RunOnSTA(() =>
        {
            var card = new Card
            {
                AccentBar = CardAccent.Left,
                AccentBrush = Brushes.SeaGreen,
                Content = new TextBlock { Text = "Nội dung" },
                Width = 200
            };

            Host(card);
            card.ApplyTemplate();

            var bar = Assert.IsType<Border>(card.Template.FindName("AccentBarV", card));

            Assert.Same(Brushes.SeaGreen, bar.Background);
        });
    }

    [Fact]
    public void Card_IsInteractive_GrowsOneStepOnHover()
    {
        RunOnSTA(() =>
        {
            var card = new Card { Elevation = 2 };
            Host(card);

            card.IsInteractive = true;

            Assert.Equal(3, VisualElement.GetHoverElevation(card));
            Assert.Equal(1.01, VisualElement.GetHoverScale(card), 3);
            Assert.Equal(Cursors.Hand, card.Cursor);

            card.IsInteractive = false;

            Assert.Equal(0, VisualElement.GetHoverElevation(card));
            Assert.Equal(1.0, VisualElement.GetHoverScale(card), 3);
        });
    }

    // ------------------------------------------------------------------ theme tokens

    [Fact]
    public void Theme_DefinesTheTokensTheNewPropertiesAskFor()
    {
        RunOnSTA(() =>
        {
            var host = Host(new Border());

            foreach (var key in new[]
            {
                "CONCOST.Brush.SurfaceElevated",
                "CONCOST.Brush.SurfaceSunken",
                "CONCOST.Brush.Overlay",
                "CONCOST.Brush.PrimarySoft",
                "CONCOST.Color.Shadow",
                "CONCOST.Color.ShadowStrong"
            })
            {
                Assert.NotNull(host.Resources[key]);
            }
        });
    }
}
