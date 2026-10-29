using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using CONCOST.UI.Core;

namespace CONCOST.UI.Controls;

/// <summary>
/// Card that leans toward the pointer and lifts slightly — a perspective illusion built
/// from skew + scale, so it stays a plain 2D visual (text stays crisp, no Viewport3D cost).
/// Eases back to flat when the pointer leaves. Compose with <see cref="SpotlightCard"/> if
/// you also want a glow.
/// </summary>
public class TiltCard : ContentControl
{
    private readonly ScaleTransform _scale = new(1, 1);
    private readonly SkewTransform _skew = new(0, 0);
    private Point _target;
    private double _tiltX;
    private double _tiltY;
    private bool _pointerInside;
    private TimeSpan _last;
    private bool _hooked;

    public static readonly DependencyProperty MaxTiltProperty =
        DependencyProperty.Register(nameof(MaxTilt), typeof(double), typeof(TiltCard),
            new PropertyMetadata(6.0));

    public static readonly DependencyProperty LiftProperty =
        DependencyProperty.Register(nameof(Lift), typeof(double), typeof(TiltCard),
            new PropertyMetadata(1.02));

    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(TiltCard),
            new PropertyMetadata(new CornerRadius(14)));

    /// <summary>Maximum tilt in degrees at the edges of the card.</summary>
    public double MaxTilt
    {
        get => (double)GetValue(MaxTiltProperty);
        set => SetValue(MaxTiltProperty, value);
    }

    /// <summary>Scale applied while hovered (1 = no lift).</summary>
    public double Lift
    {
        get => (double)GetValue(LiftProperty);
        set => SetValue(LiftProperty, value);
    }

    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    static TiltCard()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(TiltCard), new FrameworkPropertyMetadata(typeof(TiltCard)));
    }

    public TiltCard()
    {
        var group = new TransformGroup();
        group.Children.Add(_scale);
        group.Children.Add(_skew);
        RenderTransform = group;
        RenderTransformOrigin = new Point(0.5, 0.5);

        Loaded += (_, _) => Hook();
        Unloaded += (_, _) => Unhook();
        MouseEnter += (_, _) => { _pointerInside = true; Hook(); };
        MouseLeave += (_, _) => _pointerInside = false;
        MouseMove += (_, e) => { _target = e.GetPosition(this); _pointerInside = true; Hook(); };
    }

    private void Hook()
    {
        if (_hooked) return;
        CompositionTarget.Rendering += OnFrame;
        _hooked = true;
    }

    private void Unhook()
    {
        if (!_hooked) return;
        CompositionTarget.Rendering -= OnFrame;
        _hooked = false;
        _last = TimeSpan.Zero;
    }

    private void OnFrame(object? sender, EventArgs e)
    {
        if (!MotionSettings.AnimationsEnabled)
        {
            _skew.AngleX = 0;
            _skew.AngleY = 0;
            _scale.ScaleX = 1;
            _scale.ScaleY = 1;
            return;
        }

        TimeSpan now = TimeSpan.FromTicks(DateTime.UtcNow.Ticks);
        if (_last == TimeSpan.Zero) { _last = now; return; }

        double delta = (now - _last).TotalSeconds;
        _last = now;
        if (delta <= 0 || delta > 0.25) delta = 0.016;

        double width = Math.Max(1, ActualWidth);
        double height = Math.Max(1, ActualHeight);

        double goalX = 0;
        double goalY = 0;

        if (_pointerInside)
        {
            double nx = Math.Max(-1, Math.Min(1, (_target.X / width - 0.5) * 2));
            double ny = Math.Max(-1, Math.Min(1, (_target.Y / height - 0.5) * 2));
            goalX = nx * MaxTilt;
            goalY = -ny * MaxTilt;
        }

        double ease = Math.Min(1, delta * 10);
        _tiltX += (goalX - _tiltX) * ease;
        _tiltY += (goalY - _tiltY) * ease;

        // Skew is a cheap stand-in for the perspective shift of a small rotation.
        _skew.AngleX = -_tiltY * 0.9;
        _skew.AngleY = _tiltX * 0.9;

        double lift = _pointerInside ? Math.Max(1, Lift) : 1;
        _scale.ScaleX += (lift - _scale.ScaleX) * ease;
        _scale.ScaleY += (lift - _scale.ScaleY) * ease;
    }
}
