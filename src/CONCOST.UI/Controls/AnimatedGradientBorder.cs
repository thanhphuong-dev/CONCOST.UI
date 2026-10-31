using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using CONCOST.UI.Core;

namespace CONCOST.UI.Controls;

/// <summary>
/// A card whose outline is a gradient that slowly rotates around it — the "this one is live" ring used
/// on pricing cards, active plan tiles and feature highlights.
/// </summary>
/// <remarks>
/// <para>
/// The rotation is a <see cref="DoubleAnimation"/> on a <see cref="RotateTransform"/> attached to the
/// brush, not a per-frame <c>OnRender</c>. WPF runs it on the render thread, so a dozen of these cost
/// nothing on the UI thread and the motion stays smooth while the window is busy.
/// </para>
/// <para>
/// When <see cref="MotionSettings.AnimationsEnabled"/> is false the animation is cleared and the ring
/// keeps its first frame, which is what a reduced-motion user should see: the same card, not a blank one.
/// </para>
/// </remarks>
public class AnimatedGradientBorder : ContentControl
{
    private Border? _frame;
    private RotateTransform? _rotate;

    static AnimatedGradientBorder()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(AnimatedGradientBorder),
            new FrameworkPropertyMetadata(typeof(AnimatedGradientBorder)));
    }

    public AnimatedGradientBorder()
    {
        MotionSettings.Changed += (_, _) => ApplyAnimation();
        Unloaded += (_, _) => StopAnimation();
    }

    public static readonly DependencyProperty ColorAProperty =
        DependencyProperty.Register(nameof(ColorA), typeof(Color), typeof(AnimatedGradientBorder),
            new FrameworkPropertyMetadata(Color.FromRgb(0x7F, 0x77, 0xDD), FrameworkPropertyMetadataOptions.AffectsRender, OnColorsChanged));

    /// <summary>First colour of the ring. Default a violet.</summary>
    public Color ColorA
    {
        get => (Color)GetValue(ColorAProperty);
        set => SetValue(ColorAProperty, value);
    }

    public static readonly DependencyProperty ColorBProperty =
        DependencyProperty.Register(nameof(ColorB), typeof(Color), typeof(AnimatedGradientBorder),
            new FrameworkPropertyMetadata(Color.FromRgb(0x1D, 0x9E, 0x75), FrameworkPropertyMetadataOptions.AffectsRender, OnColorsChanged));

    /// <summary>Second colour of the ring. Default a teal.</summary>
    public Color ColorB
    {
        get => (Color)GetValue(ColorBProperty);
        set => SetValue(ColorBProperty, value);
    }

    public static readonly DependencyProperty ColorCProperty =
        DependencyProperty.Register(nameof(ColorC), typeof(Color), typeof(AnimatedGradientBorder),
            new FrameworkPropertyMetadata(Color.FromRgb(0xEF, 0x9F, 0x27), FrameworkPropertyMetadataOptions.AffectsRender, OnColorsChanged));

    /// <summary>Third colour of the ring. Default an amber.</summary>
    public Color ColorC
    {
        get => (Color)GetValue(ColorCProperty);
        set => SetValue(ColorCProperty, value);
    }

    public static readonly DependencyProperty RingThicknessProperty =
        DependencyProperty.Register(nameof(RingThickness), typeof(double), typeof(AnimatedGradientBorder),
            new FrameworkPropertyMetadata(2.0, FrameworkPropertyMetadataOptions.AffectsRender, OnColorsChanged));

    /// <summary>Thickness of the ring, in pixels. Default 2.</summary>
    public double RingThickness
    {
        get => (double)GetValue(RingThicknessProperty);
        set => SetValue(RingThicknessProperty, value);
    }

    public static readonly DependencyProperty CornerProperty =
        DependencyProperty.Register(nameof(Corner), typeof(CornerRadius), typeof(AnimatedGradientBorder),
            new FrameworkPropertyMetadata(new CornerRadius(12), FrameworkPropertyMetadataOptions.AffectsRender, OnColorsChanged));

    /// <summary>Corner radius of the card. Default 12.</summary>
    public CornerRadius Corner
    {
        get => (CornerRadius)GetValue(CornerProperty);
        set => SetValue(CornerProperty, value);
    }

    public static readonly DependencyProperty SecondsPerTurnProperty =
        DependencyProperty.Register(nameof(SecondsPerTurn), typeof(double), typeof(AnimatedGradientBorder),
            new PropertyMetadata(8.0, OnColorsChanged));

    /// <summary>How long one full rotation takes. Default 8 seconds.</summary>
    public double SecondsPerTurn
    {
        get => (double)GetValue(SecondsPerTurnProperty);
        set => SetValue(SecondsPerTurnProperty, value);
    }

    public static readonly DependencyProperty IsAnimatedProperty =
        DependencyProperty.Register(nameof(IsAnimated), typeof(bool), typeof(AnimatedGradientBorder),
            new PropertyMetadata(true, OnColorsChanged));

    /// <summary>Turn the rotation on or off. Default <c>true</c>.</summary>
    public bool IsAnimated
    {
        get => (bool)GetValue(IsAnimatedProperty);
        set => SetValue(IsAnimatedProperty, value);
    }

    public static readonly DependencyProperty FillProperty =
        DependencyProperty.Register(nameof(Fill), typeof(Brush), typeof(AnimatedGradientBorder),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, OnColorsChanged));

    /// <summary>Background inside the ring. Empty takes <c>CONCOST.Brush.Surface</c>.</summary>
    public Brush? Fill
    {
        get => (Brush?)GetValue(FillProperty);
        set => SetValue(FillProperty, value);
    }

    private static void OnColorsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not AnimatedGradientBorder border) return;

        border.BuildBrush();
        border.ApplyAnimation();
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _frame = GetTemplateChild("PART_Frame") as Border;
        BuildBrush();
        ApplyAnimation();
    }

    private void BuildBrush()
    {
        if (_frame is null) return;

        var brush = new LinearGradientBrush
        {
            StartPoint = new Point(0, 0),
            EndPoint = new Point(1, 1)
        };

        brush.GradientStops.Add(new GradientStop(ColorA, 0));
        brush.GradientStops.Add(new GradientStop(ColorB, 0.5));
        brush.GradientStops.Add(new GradientStop(ColorC, 1));

        _rotate = new RotateTransform(0, 0.5, 0.5);
        brush.RelativeTransform = _rotate;

        _frame.BorderBrush = brush;
        _frame.BorderThickness = new Thickness(RingThickness);
        _frame.CornerRadius = Corner;

        if (Fill is not null) _frame.Background = Fill;
    }

    private void ApplyAnimation()
    {
        if (_rotate is null) return;

        if (IsAnimated && MotionSettings.AnimationsEnabled && IsLoaded)
        {
            double seconds = Math.Max(1, SecondsPerTurn);

            _rotate.BeginAnimation(RotateTransform.AngleProperty, new DoubleAnimation
            {
                From = 0,
                To = 360,
                Duration = TimeSpan.FromSeconds(seconds),
                RepeatBehavior = RepeatBehavior.Forever
            });
        }
        else
        {
            StopAnimation();
        }
    }

    private void StopAnimation()
    {
        _rotate?.BeginAnimation(RotateTransform.AngleProperty, null);
    }
}
