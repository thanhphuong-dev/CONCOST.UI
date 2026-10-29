using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using CONCOST.UI.Core;

namespace CONCOST.UI.Controls;

/// <summary>
/// Short confetti burst for success moments (a saved model, an approved submittal).
/// Set <see cref="IsBursting"/> to true to fire; the control animates the particles
/// itself and resets the flag when the burst is over.
/// </summary>
public class ConfettiBurst : FrameworkElement
{
    private sealed class Particle
    {
        public double X;
        public double Y;
        public double Vx;
        public double Vy;
        public double Angle;
        public double Spin;
        public double Life;
        public double MaxLife;
        public double Size;
        public Color Color;
    }

    private readonly List<Particle> _particles = new();
    private TimeSpan _last;
    private bool _hooked;
    private Random _random = new Random();

    public static readonly DependencyProperty IsBurstingProperty =
        DependencyProperty.Register(nameof(IsBursting), typeof(bool), typeof(ConfettiBurst),
            new FrameworkPropertyMetadata(false, OnIsBurstingChanged));

    public static readonly DependencyProperty ParticleCountProperty =
        DependencyProperty.Register(nameof(ParticleCount), typeof(int), typeof(ConfettiBurst),
            new PropertyMetadata(90));

    public static readonly DependencyProperty DurationProperty =
        DependencyProperty.Register(nameof(Duration), typeof(Duration), typeof(ConfettiBurst),
            new PropertyMetadata(new Duration(TimeSpan.FromSeconds(1.8))));

    public static readonly DependencyProperty GravityProperty =
        DependencyProperty.Register(nameof(Gravity), typeof(double), typeof(ConfettiBurst),
            new PropertyMetadata(1100.0));

    public static readonly DependencyProperty SpreadProperty =
        DependencyProperty.Register(nameof(Spread), typeof(double), typeof(ConfettiBurst),
            new PropertyMetadata(620.0));

    public static readonly DependencyProperty OriginProperty =
        DependencyProperty.Register(nameof(Origin), typeof(Point), typeof(ConfettiBurst),
            new PropertyMetadata(new Point(double.NaN, double.NaN)));

    public static readonly DependencyProperty PaletteProperty =
        DependencyProperty.Register(nameof(Palette), typeof(IEnumerable<Color>), typeof(ConfettiBurst),
            new PropertyMetadata(null));

    public bool IsBursting
    {
        get => (bool)GetValue(IsBurstingProperty);
        set => SetValue(IsBurstingProperty, value);
    }

    public int ParticleCount
    {
        get => (int)GetValue(ParticleCountProperty);
        set => SetValue(ParticleCountProperty, value);
    }

    public Duration Duration
    {
        get => (Duration)GetValue(DurationProperty);
        set => SetValue(DurationProperty, value);
    }

    public double Gravity
    {
        get => (double)GetValue(GravityProperty);
        set => SetValue(GravityProperty, value);
    }

    public double Spread
    {
        get => (double)GetValue(SpreadProperty);
        set => SetValue(SpreadProperty, value);
    }

    public Point Origin
    {
        get => (Point)GetValue(OriginProperty);
        set => SetValue(OriginProperty, value);
    }

    public IEnumerable<Color>? Palette
    {
        get => (IEnumerable<Color>?)GetValue(PaletteProperty);
        set => SetValue(PaletteProperty, value);
    }

    private static void OnIsBurstingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ConfettiBurst confetti && (bool)e.NewValue) confetti.Burst();
    }

    private static readonly Color[] DefaultPalette =
    {
        Color.FromRgb(0xFF, 0x79, 0x00),
        Color.FromRgb(0x02, 0x84, 0xC7),
        Color.FromRgb(0x16, 0xA3, 0x4A),
        Color.FromRgb(0x8B, 0x5C, 0xF6),
        Color.FromRgb(0xDC, 0x26, 0x26),
        Color.FromRgb(0xFB, 0xBF, 0x24)
    };

    private void Burst()
    {
        _particles.Clear();
        _random = new Random();

        double width = ActualWidth > 1 ? ActualWidth : 300;
        double height = ActualHeight > 1 ? ActualHeight : 200;

        Point origin = Origin;
        if (double.IsNaN(origin.X) || double.IsNaN(origin.Y))
            origin = new Point(width / 2, height * 0.62);

        List<Color> palette = Palette is null ? new List<Color>(DefaultPalette) : new List<Color>(Palette);
        if (palette.Count == 0) palette.AddRange(DefaultPalette);

        double life = Math.Max(0.2, Duration.TimeSpan.TotalSeconds);
        int count = Math.Max(1, Math.Min(400, ParticleCount));

        for (int i = 0; i < count; i++)
        {
            double angle = -Math.PI / 2 + (_random.NextDouble() - 0.5) * Math.PI * 0.95;
            double speed = Spread * (0.45 + _random.NextDouble() * 0.75);

            _particles.Add(new Particle
            {
                X = origin.X,
                Y = origin.Y,
                Vx = Math.Cos(angle) * speed,
                Vy = Math.Sin(angle) * speed,
                Angle = _random.NextDouble() * Math.PI * 2,
                Spin = (_random.NextDouble() - 0.5) * 12,
                Life = 0,
                MaxLife = life * (0.7 + _random.NextDouble() * 0.5),
                Size = 4 + _random.NextDouble() * 5,
                Color = palette[_random.Next(palette.Count)]
            });
        }

        Hook();
        InvalidateVisual();
    }

    private void OnFrame(object? sender, EventArgs e)
    {
        if (!MotionSettings.AnimationsEnabled || _particles.Count == 0) { _last = TimeSpan.Zero; return; }

        TimeSpan now = TimeSpan.FromTicks(DateTime.UtcNow.Ticks);
        if (_last == TimeSpan.Zero) { _last = now; return; }

        double delta = (now - _last).TotalSeconds;
        _last = now;
        if (delta <= 0 || delta > 0.5) delta = 0.016;

        for (int i = _particles.Count - 1; i >= 0; i--)
        {
            Particle particle = _particles[i];
            particle.Life += delta;
            if (particle.Life >= particle.MaxLife) { _particles.RemoveAt(i); continue; }

            particle.Vy += Gravity * delta;
            particle.Vx *= 1 - 0.9 * delta;
            particle.X += particle.Vx * delta;
            particle.Y += particle.Vy * delta;
            particle.Angle += particle.Spin * delta;
        }

        if (_particles.Count == 0)
        {
            _last = TimeSpan.Zero;
            SetCurrentValue(IsBurstingProperty, false);
        }

        InvalidateVisual();
    }

    protected override void OnRender(DrawingContext dc)
    {
        if (_particles.Count == 0) return;

        dc.PushClip(new RectangleGeometry(new Rect(0, 0, ActualWidth, ActualHeight)));

        foreach (Particle particle in _particles)
        {
            double progress = particle.Life / particle.MaxLife;
            byte alpha = (byte)Math.Round(255 * Math.Max(0, 1 - Math.Pow(progress, 2.2)));
            var brush = new SolidColorBrush(Color.FromArgb(alpha, particle.Color.R, particle.Color.G, particle.Color.B));
            brush.Freeze();

            dc.PushTransform(new RotateTransform(particle.Angle * 180 / Math.PI, particle.X, particle.Y));
            dc.DrawRectangle(brush, null, new Rect(
                particle.X - particle.Size / 2,
                particle.Y - particle.Size / 4,
                particle.Size,
                particle.Size / 2));
            dc.Pop();
        }

        dc.Pop();
    }

    private void Hook()
    {
        if (_hooked) return;
        CompositionTarget.Rendering += OnFrame;
        _hooked = true;
    }
}
