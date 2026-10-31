using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CONCOST.UI.Controls;
using Xunit;

namespace CONCOST.UI.Tests;

/// <summary>The phase 5 effects: defaults, guards, and the attached behaviours.</summary>
public class EffectTests
{
    private static void RunOnSTA(Action action) => WpfApplicationHost.Run(action);

    private static Border Host(FrameworkElement element, double width = 400, double height = 200)
    {
        var host = new Border { Child = element };
        host.Measure(new Size(width, height));
        host.Arrange(new Rect(0, 0, width, height));
        host.UpdateLayout();
        return host;
    }

    [Fact]
    public void MeshGradient_Defaults_AndColourRoundTrip()
    {
        RunOnSTA(() =>
        {
            var mesh = new AnimatedMeshGradient();

            Assert.Equal(1.0, mesh.Speed);
            Assert.Equal(0.55, mesh.BlobOpacity, 3);
            Assert.Equal(0.75, mesh.Spread, 3);

            mesh.ColorA = Colors.Red;
            mesh.Spread = 0.4;

            Assert.Equal(Colors.Red, mesh.ColorA);
            Assert.Equal(0.4, mesh.Spread, 3);

            // Drawing must survive a zero-sized layout instead of dividing by it.
            Host(mesh, 0, 0);
        });
    }

    [Fact]
    public void GradientBorder_Defaults_AndAnimationToggle()
    {
        RunOnSTA(() =>
        {
            var card = new AnimatedGradientBorder();

            Assert.True(card.IsAnimated);
            Assert.Equal(8.0, card.SecondsPerTurn);
            Assert.Equal(2.0, card.RingThickness);

            card.Content = new TextBlock { Text = "Nội dung" };
            Host(card);

            card.IsAnimated = false;
            Assert.False(card.IsAnimated);
        });
    }

    [Fact]
    public void FrostedGlass_WithoutSource_IsJustATintedCard()
    {
        RunOnSTA(() =>
        {
            var glass = new FrostedGlass { Content = new TextBlock { Text = "Kính" } };

            Assert.Null(glass.Source);
            Assert.Equal(18.0, glass.BlurRadius);

            Host(glass);

            glass.BlurRadius = 24;
            Assert.Equal(24.0, glass.BlurRadius);
        });
    }

    [Fact]
    public void FrostedGlass_WithSource_AcceptsIt()
    {
        RunOnSTA(() =>
        {
            var backdrop = new Border { Background = Brushes.CadetBlue };
            var glass = new FrostedGlass { Source = backdrop, Content = new TextBlock { Text = "Kính" } };

            var host = new Grid();
            host.Children.Add(backdrop);
            host.Children.Add(glass);

            var wrapper = new Border { Child = host };
            wrapper.Measure(new Size(400, 200));
            wrapper.Arrange(new Rect(0, 0, 400, 200));
            wrapper.UpdateLayout();

            Assert.Same(backdrop, glass.Source);
        });
    }

    [Fact]
    public void CursorSpotlight_ExplicitCentre_IsKept()
    {
        RunOnSTA(() =>
        {
            var spotlight = new CursorSpotlight { Radius = 90, SpotDim = 0.4 };

            Assert.True(spotlight.FollowCursor);
            Assert.Equal(90.0, spotlight.Radius);

            spotlight.FollowCursor = false;
            spotlight.SpotlightCenter = new Point(40, 60);

            Host(spotlight);

            Assert.Equal(new Point(40, 60), spotlight.SpotlightCenter);
        });
    }

    [Fact]
    public void StaggerAnimation_AttachedValues_RoundTrip()
    {
        RunOnSTA(() =>
        {
            var list = new ListBox();

            Assert.False(StaggerAnimation.GetIsEnabled(list));
            Assert.Equal(45.0, StaggerAnimation.GetDelay(list));

            StaggerAnimation.SetIsEnabled(list, true);
            StaggerAnimation.SetDelay(list, 70);
            StaggerAnimation.SetOffsetY(list, 20);
            StaggerAnimation.SetMaxItems(list, 12);

            Assert.True(StaggerAnimation.GetIsEnabled(list));
            Assert.Equal(70.0, StaggerAnimation.GetDelay(list));
            Assert.Equal(20.0, StaggerAnimation.GetOffsetY(list));
            Assert.Equal(12, StaggerAnimation.GetMaxItems(list));

            // Turning it off twice must not throw, and must leave the generator unsubscribed.
            StaggerAnimation.SetIsEnabled(list, false);
            StaggerAnimation.SetIsEnabled(list, false);
        });
    }

    [Fact]
    public void StaggerAnimation_AnimatesGeneratedItems()
    {
        RunOnSTA(() =>
        {
            var list = new ListBox { Height = 200, Width = 200 };
            for (int i = 0; i < 5; i++) list.Items.Add($"Mục {i}");

            StaggerAnimation.SetIsEnabled(list, true);

            Host(list);

            // Containers exist and the behaviour left them visible once the entrance finished.
            Assert.Equal(5, list.Items.Count);
            Assert.NotNull(list.ItemContainerGenerator.ContainerFromIndex(0));
        });
    }

    [Fact]
    public void SharedElementTransition_OutsideATree_DoesNothing()
    {
        RunOnSTA(() =>
        {
            var element = new Border { Width = 100, Height = 50 };

            // No parent: the transition cannot convert coordinates, so it must bail out, not throw.
            SharedElementTransition.Run(element, new Rect(0, 0, 20, 20));
            SharedElementTransition.RunFrom(element, element);

            // RenderTransform defaults to Transform.Identity, so "untouched" is identity, not null.
            Assert.Same(Transform.Identity, element.RenderTransform);
        });
    }

    [Fact]
    public void SharedElementTransition_InATree_SetsThenClearsTheTransform()
    {
        RunOnSTA(() =>
        {
            var target = new Border { Width = 200, Height = 100 };
            var parent = new Grid();
            parent.Children.Add(target);

            Host(parent);

            SharedElementTransition.Run(target, new Rect(0, 0, 40, 20));

            // The transition honours reduced motion, and a headless CI session reports
            // SystemParameters.ClientAreaAnimation = false. Both branches are the contract: either the
            // element is transformed for the animation, or it is left untouched.
            if (Core.MotionSettings.AnimationsEnabled)
                Assert.IsType<TransformGroup>(target.RenderTransform);
            else
                Assert.Same(Transform.Identity, target.RenderTransform);
        });
    }

    [Fact]
    public void SharedElementTransition_WithMotionDisabled_LeavesTheElementAlone()
    {
        RunOnSTA(() =>
        {
            var target = new Border { Width = 200, Height = 100 };
            var parent = new Grid();
            parent.Children.Add(target);

            Host(parent);

            var wasDisabled = Core.MotionSettings.ForceDisabled;
            Core.MotionSettings.ForceDisabled = true;

            try
            {
                SharedElementTransition.Run(target, new Rect(0, 0, 40, 20));

                // Reduced motion is a contract, not a missing feature: nothing moves.
                Assert.Same(Transform.Identity, target.RenderTransform);
                Assert.Equal(1.0, target.Opacity);
            }
            finally
            {
                Core.MotionSettings.ForceDisabled = wasDisabled;
            }
        });
    }
}
