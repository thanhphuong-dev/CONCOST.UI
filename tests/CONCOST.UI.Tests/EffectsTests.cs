using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CONCOST.UI.Controls;
using CONCOST.UI.AttachedProperties;
using Xunit;

namespace CONCOST.UI.Tests;

public class EffectsTests
{
    private static void RunOnSTA(Action action) => WpfApplicationHost.Run(action);

    [Fact]
    public void Ripple_DefaultProperties_MatchSpecifications()
    {
        RunOnSTA(() =>
        {
            var ripple = new Ripple();

            Assert.Equal(RippleMode.Target, ripple.Mode);
            Assert.True(ripple.IsRippleEnabled);
            Assert.Equal(0.35, ripple.MaxOpacity);
            Assert.Equal(TimeSpan.FromMilliseconds(450), ripple.Duration);
            Assert.Equal(new CornerRadius(0), ripple.CornerRadius);
            Assert.Null(ripple.RippleBrush);
        });
    }

    [Fact]
    public void Ripple_CustomProperties_SetAndRetrieveCorrectly()
    {
        RunOnSTA(() =>
        {
            var brush = new SolidColorBrush(Colors.Orange);
            var ripple = new Ripple
            {
                Mode = RippleMode.Center,
                IsRippleEnabled = false,
                MaxOpacity = 0.5,
                Duration = TimeSpan.FromMilliseconds(300),
                CornerRadius = new CornerRadius(8),
                RippleBrush = brush
            };

            Assert.Equal(RippleMode.Center, ripple.Mode);
            Assert.False(ripple.IsRippleEnabled);
            Assert.Equal(0.5, ripple.MaxOpacity);
            Assert.Equal(TimeSpan.FromMilliseconds(300), ripple.Duration);
            Assert.Equal(new CornerRadius(8), ripple.CornerRadius);
            Assert.Same(brush, ripple.RippleBrush);
        });
    }

    [Fact]
    public void Ripple_RejectsInvalidAnimationSettings()
    {
        RunOnSTA(() =>
        {
            var ripple = new Ripple();

            Assert.Throws<ArgumentException>(() => ripple.MaxOpacity = 1.01);
            Assert.Throws<ArgumentException>(() => ripple.Duration = TimeSpan.Zero);
            Assert.Throws<ArgumentException>(() => ripple.MaxConcurrentRipples = 0);
        });
    }

    [Fact]
    public void RippleElement_AttachedProperties_WorkCorrectly()
    {
        RunOnSTA(() =>
        {
            var button = new Button();

            Assert.False(RippleElement.GetIsEnabled(button));
            Assert.Equal(new CornerRadius(0), RippleElement.GetCornerRadius(button));
            Assert.Null(RippleElement.GetBrush(button));

            RippleElement.SetIsEnabled(button, true);
            RippleElement.SetCornerRadius(button, new CornerRadius(6));
            RippleElement.SetMode(button, RippleMode.Center);
            RippleElement.SetDuration(button, TimeSpan.FromMilliseconds(500));
            RippleElement.SetMaxOpacity(button, 0.25);
            RippleElement.SetMaxConcurrentRipples(button, 2);
            var brush = new SolidColorBrush(Colors.Red);
            RippleElement.SetBrush(button, brush);

            Assert.True(RippleElement.GetIsEnabled(button));
            Assert.Equal(new CornerRadius(6), RippleElement.GetCornerRadius(button));
            Assert.Equal(RippleMode.Center, RippleElement.GetMode(button));
            Assert.Equal(TimeSpan.FromMilliseconds(500), RippleElement.GetDuration(button));
            Assert.Equal(0.25, RippleElement.GetMaxOpacity(button));
            Assert.Equal(2, RippleElement.GetMaxConcurrentRipples(button));
            Assert.Same(brush, RippleElement.GetBrush(button));
        });
    }

    [Fact]
    public void Skeleton_DefaultProperties_MatchSpecifications()
    {
        RunOnSTA(() =>
        {
            var skeleton = new Skeleton();

            Assert.True(skeleton.IsLoading);
            Assert.Equal(SkeletonShape.Rectangle, skeleton.SkeletonShape);
            Assert.Equal(new CornerRadius(6), skeleton.CornerRadius);
            Assert.Equal(TimeSpan.FromSeconds(1.5), skeleton.Duration);
            Assert.True(skeleton.IsWaveEnabled);
            Assert.Null(skeleton.BaseBrush);
            Assert.Null(skeleton.HighlightBrush);
        });
    }

    [Fact]
    public void Skeleton_CustomProperties_SetAndRetrieveCorrectly()
    {
        RunOnSTA(() =>
        {
            var baseBrush = new SolidColorBrush(Colors.DarkGray);
            var highlightBrush = new SolidColorBrush(Colors.White);

            var skeleton = new Skeleton
            {
                IsLoading = false,
                SkeletonShape = SkeletonShape.Circle,
                CornerRadius = new CornerRadius(20),
                Duration = TimeSpan.FromSeconds(2.0),
                IsWaveEnabled = false,
                BaseBrush = baseBrush,
                HighlightBrush = highlightBrush
            };

            Assert.False(skeleton.IsLoading);
            Assert.Equal(SkeletonShape.Circle, skeleton.SkeletonShape);
            Assert.Equal(new CornerRadius(20), skeleton.CornerRadius);
            Assert.Equal(TimeSpan.FromSeconds(2.0), skeleton.Duration);
            Assert.False(skeleton.IsWaveEnabled);
            Assert.Same(baseBrush, skeleton.BaseBrush);
            Assert.Same(highlightBrush, skeleton.HighlightBrush);
        });
    }

    [Fact]
    public void Skeleton_ShapePresets_EnumValuesAreValid()
    {
        RunOnSTA(() =>
        {
            var skeleton = new Skeleton();

            skeleton.SkeletonShape = SkeletonShape.TextLine;
            Assert.Equal(SkeletonShape.TextLine, skeleton.SkeletonShape);

            skeleton.SkeletonShape = SkeletonShape.Card;
            Assert.Equal(SkeletonShape.Card, skeleton.SkeletonShape);

            skeleton.SkeletonShape = SkeletonShape.Circle;
            Assert.Equal(SkeletonShape.Circle, skeleton.SkeletonShape);
        });
    }
}
