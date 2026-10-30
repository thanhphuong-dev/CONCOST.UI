using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CONCOST.UI.Controls.Canvas;

/// <summary>
/// A single snowflake particle with position, velocity, oscillation phase, and size.
/// </summary>
public class Snowflake
{
    public double X { get; set; }
    public double Y { get; set; }
    public double Radius { get; set; }
    public double SpeedY { get; set; }
    public double SwingSpeed { get; set; }
    public double SwingAngle { get; set; }
    public double SwingRadius { get; set; }
    public double Opacity { get; set; }
}

/// <summary>
/// SnowCanvas creates a high-performance, GPU-accelerated falling snow particle effect.
/// Supports customizable particle count, colors, speed, wind drift, and pause/resume animation.
/// </summary>
public class SnowCanvas : System.Windows.Controls.Canvas
{
    private readonly List<Snowflake> _flakes = new();
    private readonly Random _rand = new();
    private bool _isRendering;

    public static readonly DependencyProperty SnowflakeCountProperty =
        DependencyProperty.Register(nameof(SnowflakeCount), typeof(int), typeof(SnowCanvas),
            new FrameworkPropertyMetadata(60, OnConfigChanged));

    public static readonly DependencyProperty SnowflakeBrushProperty =
        DependencyProperty.Register(nameof(SnowflakeBrush), typeof(Brush), typeof(SnowCanvas),
            new FrameworkPropertyMetadata(Brushes.White, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty MinSnowflakeSizeProperty =
        DependencyProperty.Register(nameof(MinSnowflakeSize), typeof(double), typeof(SnowCanvas),
            new FrameworkPropertyMetadata(1.5, OnConfigChanged));

    public static readonly DependencyProperty MaxSnowflakeSizeProperty =
        DependencyProperty.Register(nameof(MaxSnowflakeSize), typeof(double), typeof(SnowCanvas),
            new FrameworkPropertyMetadata(4.5, OnConfigChanged));

    public static readonly DependencyProperty MinSpeedProperty =
        DependencyProperty.Register(nameof(MinSpeed), typeof(double), typeof(SnowCanvas),
            new FrameworkPropertyMetadata(0.8, OnConfigChanged));

    public static readonly DependencyProperty MaxSpeedProperty =
        DependencyProperty.Register(nameof(MaxSpeed), typeof(double), typeof(SnowCanvas),
            new FrameworkPropertyMetadata(2.5, OnConfigChanged));

    public static readonly DependencyProperty WindProperty =
        DependencyProperty.Register(nameof(Wind), typeof(double), typeof(SnowCanvas),
            new FrameworkPropertyMetadata(0.2));

    public static readonly DependencyProperty IsActiveProperty =
        DependencyProperty.Register(nameof(IsActive), typeof(bool), typeof(SnowCanvas),
            new FrameworkPropertyMetadata(true, OnIsActiveChanged));

    public int SnowflakeCount
    {
        get => (int)GetValue(SnowflakeCountProperty);
        set => SetValue(SnowflakeCountProperty, value);
    }

    public Brush SnowflakeBrush
    {
        get => (Brush)GetValue(SnowflakeBrushProperty);
        set => SetValue(SnowflakeBrushProperty, value);
    }

    public double MinSnowflakeSize
    {
        get => (double)GetValue(MinSnowflakeSizeProperty);
        set => SetValue(MinSnowflakeSizeProperty, value);
    }

    public double MaxSnowflakeSize
    {
        get => (double)GetValue(MaxSnowflakeSizeProperty);
        set => SetValue(MaxSnowflakeSizeProperty, value);
    }

    public double MinSpeed
    {
        get => (double)GetValue(MinSpeedProperty);
        set => SetValue(MinSpeedProperty, value);
    }

    public double MaxSpeed
    {
        get => (double)GetValue(MaxSpeedProperty);
        set => SetValue(MaxSpeedProperty, value);
    }

    public double Wind
    {
        get => (double)GetValue(WindProperty);
        set => SetValue(WindProperty, value);
    }

    public bool IsActive
    {
        get => (bool)GetValue(IsActiveProperty);
        set => SetValue(IsActiveProperty, value);
    }

    public SnowCanvas()
    {
        ClipToBounds = true;
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
        SizeChanged += OnSizeChanged;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        InitFlakes();
        StartRendering();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        StopRendering();
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (ActualWidth > 0 && ActualHeight > 0 && _flakes.Count == 0)
        {
            InitFlakes();
        }
    }

    private static void OnConfigChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SnowCanvas canvas)
        {
            canvas.InitFlakes();
        }
    }

    private static void OnIsActiveChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SnowCanvas canvas)
        {
            if ((bool)e.NewValue) canvas.StartRendering();
            else canvas.StopRendering();
        }
    }

    private void InitFlakes()
    {
        _flakes.Clear();
        double w = ActualWidth > 0 ? ActualWidth : 800;
        double h = ActualHeight > 0 ? ActualHeight : 600;

        for (int i = 0; i < SnowflakeCount; i++)
        {
            _flakes.Add(CreateSnowflake(w, h, true));
        }
    }

    private Snowflake CreateSnowflake(double width, double height, bool randomY = false)
    {
        double radius = MinSnowflakeSize + _rand.NextDouble() * (MaxSnowflakeSize - MinSnowflakeSize);
        double speed = MinSpeed + _rand.NextDouble() * (MaxSpeed - MinSpeed);
        double y = randomY ? _rand.NextDouble() * height : -radius * 2;

        return new Snowflake
        {
            X = _rand.NextDouble() * width,
            Y = y,
            Radius = radius,
            SpeedY = speed,
            SwingAngle = _rand.NextDouble() * Math.PI * 2,
            SwingSpeed = 0.02 + _rand.NextDouble() * 0.03,
            SwingRadius = 0.5 + _rand.NextDouble() * 1.5,
            Opacity = 0.4 + _rand.NextDouble() * 0.6
        };
    }

    private void StartRendering()
    {
        if (_isRendering || !IsActive) return;
        CompositionTarget.Rendering += OnRendering;
        _isRendering = true;
    }

    private void StopRendering()
    {
        if (!_isRendering) return;
        CompositionTarget.Rendering -= OnRendering;
        _isRendering = false;
    }

    private void OnRendering(object? sender, EventArgs e)
    {
        if (ActualWidth <= 0 || ActualHeight <= 0) return;

        double w = ActualWidth;
        double h = ActualHeight;

        foreach (var flake in _flakes)
        {
            flake.SwingAngle += flake.SwingSpeed;
            flake.X += Math.Sin(flake.SwingAngle) * flake.SwingRadius + Wind;
            flake.Y += flake.SpeedY;

            // Reset when reaching bottom
            if (flake.Y > h + flake.Radius * 2)
            {
                flake.Y = -flake.Radius * 2;
                flake.X = _rand.NextDouble() * w;
            }

            // Wrap around horizontal edges
            if (flake.X < -flake.Radius * 2) flake.X = w + flake.Radius;
            else if (flake.X > w + flake.Radius * 2) flake.X = -flake.Radius;
        }

        InvalidateVisual();
    }

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);

        if (_flakes.Count == 0) return;

        var brush = SnowflakeBrush;

        foreach (var flake in _flakes)
        {
            dc.PushOpacity(flake.Opacity);
            dc.DrawEllipse(brush, null, new Point(flake.X, flake.Y), flake.Radius, flake.Radius);
            dc.Pop();
        }
    }
}
