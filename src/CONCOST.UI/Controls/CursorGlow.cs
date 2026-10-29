using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using CONCOST.UI.Core;

namespace CONCOST.UI.Controls;

/// <summary>
/// Soft light that trails the pointer across the surface it covers — the “cursor glow”
/// used on dark hero sections. Place it as the top layer of a Grid; it never blocks input.
/// </summary>
public class CursorGlow : AnimatedEffectBase
{
    private Point _current;
    private Point _target;
    private bool _hasPointer;
    private double _intensity;

    public static readonly DependencyProperty GlowSizeProperty =
        DependencyProperty.Register(nameof(GlowSize), typeof(double), typeof(CursorGlow),
            new FrameworkPropertyMetadata(220.0, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty GlowBrushProperty =
        DependencyProperty.Register(nameof(GlowBrush), typeof(Brush), typeof(CursorGlow),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty GlowOpacityProperty =
        DependencyProperty.Register(nameof(GlowOpacity), typeof(double), typeof(CursorGlow),
            new FrameworkPropertyMetadata(0.30, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty FollowSpeedProperty =
        DependencyProperty.Register(nameof(FollowSpeed), typeof(double), typeof(CursorGlow),
            new FrameworkPropertyMetadata(0.18, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Diameter of the glow in pixels.</summary>
    public double GlowSize
    {
        get => (double)GetValue(GlowSizeProperty);
        set => SetValue(GlowSizeProperty, value);
    }

    public Brush? GlowBrush
    {
        get => (Brush?)GetValue(GlowBrushProperty);
        set => SetValue(GlowBrushProperty, value);
    }

    public double GlowOpacity
    {
        get => (double)GetValue(GlowOpacityProperty);
        set => SetValue(GlowOpacityProperty, value);
    }

    /// <summary>0 = glow snaps to the pointer, 1 = never arrives. 0.15–0.25 feels right.</summary>
    public double FollowSpeed
    {
        get => (double)GetValue(FollowSpeedProperty);
        set => SetValue(FollowSpeedProperty, value);
    }

    protected override void OnTick(double seconds)
    {
        Point pointer = Mouse.GetPosition(this);
        bool inside = pointer.X >= 0 && pointer.Y >= 0 && pointer.X <= ActualWidth && pointer.Y <= ActualHeight;

        if (inside)
        {
            _hasPointer = true;
            _target = pointer;
            if (_intensity < 0.001) _current = pointer;
        }
        else
        {
            _hasPointer = false;
        }

        double follow = Math.Max(0.02, Math.Min(1, FollowSpeed));
        _current = new Point(
            _current.X + (_target.X - _current.X) * follow,
            _current.Y + (_target.Y - _current.Y) * follow);

        double goal = _hasPointer ? 1 : 0;
        _intensity += (goal - _intensity) * Math.Min(1, seconds * 6);
        if (_intensity < 0.002) _intensity = 0;
    }

    protected override void OnMotionStopped()
    {
        // Reduced motion: show a soft static light in the middle instead of a frozen trail.
        _current = new Point(ActualWidth / 2, ActualHeight / 2);
        _intensity = _hasPointer ? 0.6 : 0;
    }

    protected override void OnRender(DrawingContext dc)
    {
        double width = ActualWidth;
        double height = ActualHeight;
        if (width <= 1 || height <= 1 || _intensity <= 0.002) return;

        Color color = GlowBrush is SolidColorBrush solid
            ? solid.Color
            : (TryFindResource("CONCOST.Brush.Primary") as SolidColorBrush)?.Color ?? Colors.DodgerBlue;

        double radius = Math.Max(8, GlowSize) / 2;
        double peak = Math.Max(0, Math.Min(1, GlowOpacity)) * _intensity;

        var brush = new RadialGradientBrush
        {
            GradientOrigin = new Point(0.5, 0.5),
            Center = new Point(0.5, 0.5),
            RadiusX = 0.5,
            RadiusY = 0.5
        };
        brush.GradientStops.Add(new GradientStop(Color.FromArgb((byte)(255 * peak), color.R, color.G, color.B), 0));
        brush.GradientStops.Add(new GradientStop(Color.FromArgb((byte)(255 * peak * 0.45), color.R, color.G, color.B), 0.55));
        brush.GradientStops.Add(new GradientStop(Color.FromArgb(0, color.R, color.G, color.B), 1));
        brush.Freeze();

        dc.DrawEllipse(brush, null, _current, radius, radius);
    }
}
