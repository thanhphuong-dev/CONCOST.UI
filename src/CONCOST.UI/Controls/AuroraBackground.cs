using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using CONCOST.UI.Core;

namespace CONCOST.UI.Controls;

/// <summary>
/// Animated aurora backdrop: a few large, soft radial blobs drifting on a dark or
/// light surface. Rendered directly (no blur effect), so it stays cheap enough for
/// splash screens, hero panels and login windows.
/// </summary>
public class AuroraBackground : FrameworkElement
{
    private readonly List<Blob> _blobs = new();
    private TimeSpan _last;
    private double _time;
    private bool _hooked;
    private Random _random = new Random(20261029);

    public static readonly DependencyProperty BlobCountProperty =
        DependencyProperty.Register(nameof(BlobCount), typeof(int), typeof(AuroraBackground),
            new FrameworkPropertyMetadata(4, FrameworkPropertyMetadataOptions.AffectsRender, OnLayoutChanged));

    public static readonly DependencyProperty SpeedProperty =
        DependencyProperty.Register(nameof(Speed), typeof(double), typeof(AuroraBackground),
            new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty BlobOpacityProperty =
        DependencyProperty.Register(nameof(BlobOpacity), typeof(double), typeof(AuroraBackground),
            new FrameworkPropertyMetadata(0.40, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty IsAnimatedProperty =
        DependencyProperty.Register(nameof(IsAnimated), typeof(bool), typeof(AuroraBackground),
            new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty AccentBrushProperty =
        DependencyProperty.Register(nameof(AccentBrush), typeof(Brush), typeof(AuroraBackground),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty SecondaryBrushProperty =
        DependencyProperty.Register(nameof(SecondaryBrush), typeof(Brush), typeof(AuroraBackground),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty TertiaryBrushProperty =
        DependencyProperty.Register(nameof(TertiaryBrush), typeof(Brush), typeof(AuroraBackground),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    public int BlobCount
    {
        get => (int)GetValue(BlobCountProperty);
        set => SetValue(BlobCountProperty, value);
    }

    public double Speed
    {
        get => (double)GetValue(SpeedProperty);
        set => SetValue(SpeedProperty, value);
    }

    public double BlobOpacity
    {
        get => (double)GetValue(BlobOpacityProperty);
        set => SetValue(BlobOpacityProperty, value);
    }

    public bool IsAnimated
    {
        get => (bool)GetValue(IsAnimatedProperty);
        set => SetValue(IsAnimatedProperty, value);
    }

    public Brush? AccentBrush
    {
        get => (Brush?)GetValue(AccentBrushProperty);
        set => SetValue(AccentBrushProperty, value);
    }

    public Brush? SecondaryBrush
    {
        get => (Brush?)GetValue(SecondaryBrushProperty);
        set => SetValue(SecondaryBrushProperty, value);
    }

    public Brush? TertiaryBrush
    {
        get => (Brush?)GetValue(TertiaryBrushProperty);
        set => SetValue(TertiaryBrushProperty, value);
    }

    public AuroraBackground()
    {
        ClipToBounds = true;
        IsHitTestVisible = false;
    }

    private sealed class Blob
    {
        public double PhaseX;
        public double PhaseY;
        public double SpeedX;
        public double SpeedY;
        public double RadiusFactor;
        public int ColorIndex;
    }

    private static void OnLayoutChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is AuroraBackground aurora) aurora.RebuildBlobs();
    }

    private void RebuildBlobs()
    {
        _blobs.Clear();
        _random = new Random(20261029);
        int count = Math.Max(1, Math.Min(8, BlobCount));

        for (int i = 0; i < count; i++)
        {
            _blobs.Add(new Blob
            {
                PhaseX = _random.NextDouble() * Math.PI * 2,
                PhaseY = _random.NextDouble() * Math.PI * 2,
                SpeedX = 0.10 + _random.NextDouble() * 0.16,
                SpeedY = 0.08 + _random.NextDouble() * 0.14,
                RadiusFactor = 0.45 + _random.NextDouble() * 0.35,
                ColorIndex = i % 3
            });
        }

        InvalidateVisual();
    }

    protected override void OnRender(DrawingContext dc)
    {
        double width = ActualWidth;
        double height = ActualHeight;
        if (width <= 1 || height <= 1) return;

        if (_blobs.Count == 0) RebuildBlobs();

        Brush[] palette =
        {
            AccentBrush ?? TryFindResource("CONCOST.Brush.Primary") as Brush ?? Brushes.OrangeRed,
            SecondaryBrush ?? new SolidColorBrush(Color.FromRgb(0x02, 0x84, 0xC7)),
            TertiaryBrush ?? new SolidColorBrush(Color.FromRgb(0x8B, 0x5C, 0xF6))
        };

        double maxRadius = Math.Max(width, height) * 0.55;

        foreach (Blob blob in _blobs)
        {
            double cx = width * (0.5 + 0.42 * Math.Sin(_time * blob.SpeedX * Speed + blob.PhaseX));
            double cy = height * (0.5 + 0.42 * Math.Cos(_time * blob.SpeedY * Speed + blob.PhaseY));
            double radius = maxRadius * blob.RadiusFactor;

            Color color = palette[blob.ColorIndex] is SolidColorBrush solid ? solid.Color : Colors.Orange;
            var brush = new RadialGradientBrush
            {
                GradientOrigin = new Point(0.5, 0.5),
                Center = new Point(0.5, 0.5),
                RadiusX = 0.5,
                RadiusY = 0.5
            };
            byte core = (byte)Math.Round(255 * Math.Max(0, Math.Min(1, BlobOpacity)));
            brush.GradientStops.Add(new GradientStop(Color.FromArgb(core, color.R, color.G, color.B), 0.0));
            brush.GradientStops.Add(new GradientStop(Color.FromArgb((byte)(core / 2), color.R, color.G, color.B), 0.55));
            brush.GradientStops.Add(new GradientStop(Color.FromArgb(0, color.R, color.G, color.B), 1.0));
            brush.Freeze();

            dc.DrawEllipse(brush, null, new Point(cx, cy), radius, radius);
        }
    }

    private void OnFrame(object? sender, EventArgs e)
    {
        if (!MotionSettings.AnimationsEnabled || !IsAnimated || ActualWidth <= 0) { _last = TimeSpan.Zero; return; }

        TimeSpan now = TimeSpan.FromTicks(DateTime.UtcNow.Ticks);
        if (_last == TimeSpan.Zero) { _last = now; return; }

        double delta = (now - _last).TotalSeconds;
        _last = now;
        if (delta <= 0 || delta > 0.5) delta = 0.016;

        _time += delta;
        InvalidateVisual();
    }

    private void Hook()
    {
        if (_hooked) return;
        CompositionTarget.Rendering += OnFrame;
        _hooked = true;
    }

    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
        base.OnRenderSizeChanged(sizeInfo);
        Hook();
    }
}
