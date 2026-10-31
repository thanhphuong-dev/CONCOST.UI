using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace CONCOST.UI.Controls;

/// <summary>
/// Ambient field of drifting particles for hero panels, splash screens and empty states.
/// Particles wrap around the edges, so the field never empties however long it runs.
/// </summary>
/// <remarks>
/// Built on <see cref="AnimatedEffectBase"/>, so it stops with the rest of the library when the
/// Windows "show animations" setting is off, and it never hit-tests: it is a backdrop, not a surface.
/// <see cref="Seed"/> fixes the layout, which keeps screenshots reproducible.
/// </remarks>
public class ParticleField : AnimatedEffectBase
{
    private sealed class Particle
    {
        public double X;
        public double Y;
        public double Vx;
        public double Vy;
        public double Radius;
        public double Alpha;
    }

    private readonly List<Particle> _particles = new();
    private Random _random = new(20260101);
    private Size _builtFor = Size.Empty;

    public static readonly DependencyProperty ParticleCountProperty =
        DependencyProperty.Register(nameof(ParticleCount), typeof(int), typeof(ParticleField),
            new FrameworkPropertyMetadata(60, FrameworkPropertyMetadataOptions.AffectsRender, OnLayoutPropertyChanged));

    public static readonly DependencyProperty MinRadiusProperty =
        DependencyProperty.Register(nameof(MinRadius), typeof(double), typeof(ParticleField),
            new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.AffectsRender, OnLayoutPropertyChanged));

    public static readonly DependencyProperty MaxRadiusProperty =
        DependencyProperty.Register(nameof(MaxRadius), typeof(double), typeof(ParticleField),
            new FrameworkPropertyMetadata(2.6, FrameworkPropertyMetadataOptions.AffectsRender, OnLayoutPropertyChanged));

    public static readonly DependencyProperty SpeedProperty =
        DependencyProperty.Register(nameof(Speed), typeof(double), typeof(ParticleField),
            new FrameworkPropertyMetadata(18.0, OnSpeedChanged));

    public static readonly DependencyProperty DirectionProperty =
        DependencyProperty.Register(nameof(Direction), typeof(double), typeof(ParticleField),
            new FrameworkPropertyMetadata(270.0, OnSpeedChanged));

    public static readonly DependencyProperty SpreadProperty =
        DependencyProperty.Register(nameof(Spread), typeof(double), typeof(ParticleField),
            new FrameworkPropertyMetadata(0.28, OnSpeedChanged));

    public static readonly DependencyProperty ParticleBrushProperty =
        DependencyProperty.Register(nameof(ParticleBrush), typeof(Brush), typeof(ParticleField),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty ParticleOpacityProperty =
        DependencyProperty.Register(nameof(ParticleOpacity), typeof(double), typeof(ParticleField),
            new FrameworkPropertyMetadata(0.75, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty ConnectDistanceProperty =
        DependencyProperty.Register(nameof(ConnectDistance), typeof(double), typeof(ParticleField),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty SeedProperty =
        DependencyProperty.Register(nameof(Seed), typeof(int), typeof(ParticleField),
            new FrameworkPropertyMetadata(20260101, FrameworkPropertyMetadataOptions.AffectsRender, OnLayoutPropertyChanged));

    /// <summary>How many particles to draw.</summary>
    public int ParticleCount
    {
        get => (int)GetValue(ParticleCountProperty);
        set => SetValue(ParticleCountProperty, value);
    }

    /// <summary>Smallest particle radius.</summary>
    public double MinRadius
    {
        get => (double)GetValue(MinRadiusProperty);
        set => SetValue(MinRadiusProperty, value);
    }

    /// <summary>Largest particle radius.</summary>
    public double MaxRadius
    {
        get => (double)GetValue(MaxRadiusProperty);
        set => SetValue(MaxRadiusProperty, value);
    }

    /// <summary>Base drift speed in device-independent pixels per second.</summary>
    public double Speed
    {
        get => (double)GetValue(SpeedProperty);
        set => SetValue(SpeedProperty, value);
    }

    /// <summary>Drift direction in degrees; 0 is right, 90 is down. Defaults to 270 (upward).</summary>
    public double Direction
    {
        get => (double)GetValue(DirectionProperty);
        set => SetValue(DirectionProperty, value);
    }

    /// <summary>Random jitter added to each particle's direction, as a fraction of the unit circle.</summary>
    public double Spread
    {
        get => (double)GetValue(SpreadProperty);
        set => SetValue(SpreadProperty, value);
    }

    /// <summary>Particle colour. Defaults to <c>CONCOST.Brush.Primary</c>.</summary>
    public Brush? ParticleBrush
    {
        get => (Brush?)GetValue(ParticleBrushProperty);
        set => SetValue(ParticleBrushProperty, value);
    }

    /// <summary>Overall opacity of the field, 0–1.</summary>
    public double ParticleOpacity
    {
        get => (double)GetValue(ParticleOpacityProperty);
        set => SetValue(ParticleOpacityProperty, value);
    }

    /// <summary>
    /// Draws a line between particles closer than this many pixels. 0 turns the constellation off.
    /// </summary>
    public double ConnectDistance
    {
        get => (double)GetValue(ConnectDistanceProperty);
        set => SetValue(ConnectDistanceProperty, value);
    }

    /// <summary>Seed for the particle layout, so the same field can be reproduced.</summary>
    public int Seed
    {
        get => (int)GetValue(SeedProperty);
        set => SetValue(SeedProperty, value);
    }

    /// <summary>Number of particles currently alive. Exposed for tests.</summary>
    public int ActiveParticleCount => _particles.Count;

    public ParticleField()
    {
        Loaded += (_, _) => Build();
        SizeChanged += (_, e) =>
        {
            if (e.NewSize.Width <= 0 || e.NewSize.Height <= 0) return;

            // The first pass measures with a placeholder size; rebuild once the real box is known,
            // and again whenever the host is resized, so the field always fills its own bounds.
            if (Math.Abs(e.NewSize.Width - _builtFor.Width) > 1 ||
                Math.Abs(e.NewSize.Height - _builtFor.Height) > 1)
            {
                Build();
            }
        };
    }

    private static void OnLayoutPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ParticleField field) field.Build();
    }

    private static void OnSpeedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ParticleField field) field.ApplyVelocities();
    }

    protected override void OnRestarted()
    {
        Build();
    }

    private void Build()
    {
        _particles.Clear();

        double width = ActualWidth > 0 ? ActualWidth : 600;
        double height = ActualHeight > 0 ? ActualHeight : 400;
        _builtFor = new Size(width, height);

        _random = new Random(Seed);

        int count = Math.Max(0, ParticleCount);
        for (int i = 0; i < count; i++)
        {
            var particle = new Particle
            {
                X = _random.NextDouble() * width,
                Y = _random.NextDouble() * height,
                Radius = MinRadius + _random.NextDouble() * Math.Max(0, MaxRadius - MinRadius),
                Alpha = 0.35 + _random.NextDouble() * 0.65
            };
            _particles.Add(particle);
        }

        ApplyVelocities();
        InvalidateVisual();
    }

    private void ApplyVelocities()
    {
        if (_particles.Count == 0) return;

        // Re-seed so direction/speed changes keep the same layout but redistribute the drift.
        _random = new Random(Seed + 17);

        double baseRadians = Direction * Math.PI / 180.0;

        foreach (Particle particle in _particles)
        {
            double jitter = (_random.NextDouble() - 0.5) * 2.0 * Spread * Math.PI;
            double radians = baseRadians + jitter;
            double speed = Speed * (0.55 + _random.NextDouble() * 0.9);
            particle.Vx = Math.Cos(radians) * speed;
            particle.Vy = Math.Sin(radians) * speed;
        }
    }

    protected override void OnTick(double seconds)
    {
        if (_particles.Count == 0) return;

        double width = ActualWidth;
        double height = ActualHeight;
        if (width <= 0 || height <= 0) return;

        foreach (Particle particle in _particles)
        {
            particle.X += particle.Vx * seconds;
            particle.Y += particle.Vy * seconds;

            double margin = particle.Radius * 2;

            if (particle.X < -margin) particle.X = width + margin;
            else if (particle.X > width + margin) particle.X = -margin;

            if (particle.Y < -margin) particle.Y = height + margin;
            else if (particle.Y > height + margin) particle.Y = -margin;
        }
    }

    protected override void OnRender(DrawingContext dc)
    {
        if (_particles.Count == 0) return;

        Brush brush = ParticleBrush ?? TryFindResource("CONCOST.Brush.Primary") as Brush ?? Brushes.DodgerBlue;
        double opacity = ParticleOpacity < 0 ? 0 : ParticleOpacity > 1 ? 1 : ParticleOpacity;

        double connect = ConnectDistance;

        if (connect > 0)
        {
            var pen = new Pen(brush, 1.0);
            pen.Freeze();

            for (int i = 0; i < _particles.Count; i++)
            {
                for (int j = i + 1; j < _particles.Count; j++)
                {
                    double dx = _particles[i].X - _particles[j].X;
                    double dy = _particles[i].Y - _particles[j].Y;
                    double distance = Math.Sqrt(dx * dx + dy * dy);
                    if (distance > connect) continue;

                    double strength = (1.0 - distance / connect) * 0.45 * opacity;
                    dc.PushOpacity(strength);
                    dc.DrawLine(pen, new Point(_particles[i].X, _particles[i].Y), new Point(_particles[j].X, _particles[j].Y));
                    dc.Pop();
                }
            }
        }

        foreach (Particle particle in _particles)
        {
            dc.PushOpacity(particle.Alpha * opacity);
            dc.DrawEllipse(brush, null, new Point(particle.X, particle.Y), particle.Radius, particle.Radius);
            dc.Pop();
        }
    }
}
