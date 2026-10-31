using System;
using System.Windows;
using System.Windows.Media;

namespace CONCOST.UI.Controls;

/// <summary>
/// An animated mesh gradient: a handful of large colour blobs drifting on their own orbits and blending
/// into each other, for hero panels and empty states.
/// </summary>
/// <remarks>
/// <para>
/// This is deliberately not <see cref="AuroraBackground"/>. Aurora drifts a few soft blobs across a
/// surface and reads as a backdrop; a mesh gradient keeps its blobs near fixed anchors and lets them
/// <em>breathe</em>, so the result looks like a still image that happens to move rather than weather.
/// </para>
/// <para>
/// Each blob is a radial gradient with a transparent rim, drawn with additive-ish blending by stacking
/// them — no blur effect, so it stays cheap at full window size.
/// </para>
/// </remarks>
public class AnimatedMeshGradient : AnimatedEffectBase
{
    private readonly Random _random = new(20261031);
    private Point[] _anchors = Array.Empty<Point>();
    private double[] _phase = Array.Empty<double>();

    public static readonly DependencyProperty ColorAProperty =
        DependencyProperty.Register(nameof(ColorA), typeof(Color), typeof(AnimatedMeshGradient),
            new FrameworkPropertyMetadata(Color.FromRgb(0x7F, 0x77, 0xDD), FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>First colour of the mesh. Default a violet.</summary>
    public Color ColorA
    {
        get => (Color)GetValue(ColorAProperty);
        set => SetValue(ColorAProperty, value);
    }

    public static readonly DependencyProperty ColorBProperty =
        DependencyProperty.Register(nameof(ColorB), typeof(Color), typeof(AnimatedMeshGradient),
            new FrameworkPropertyMetadata(Color.FromRgb(0x1D, 0x9E, 0x75), FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Second colour. Default a teal.</summary>
    public Color ColorB
    {
        get => (Color)GetValue(ColorBProperty);
        set => SetValue(ColorBProperty, value);
    }

    public static readonly DependencyProperty ColorCProperty =
        DependencyProperty.Register(nameof(ColorC), typeof(Color), typeof(AnimatedMeshGradient),
            new FrameworkPropertyMetadata(Color.FromRgb(0xEF, 0x9F, 0x27), FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Third colour. Default an amber.</summary>
    public Color ColorC
    {
        get => (Color)GetValue(ColorCProperty);
        set => SetValue(ColorCProperty, value);
    }

    public static readonly DependencyProperty ColorDProperty =
        DependencyProperty.Register(nameof(ColorD), typeof(Color), typeof(AnimatedMeshGradient),
            new FrameworkPropertyMetadata(Color.FromRgb(0xD4, 0x53, 0x7E), FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Fourth colour. Default a rose.</summary>
    public Color ColorD
    {
        get => (Color)GetValue(ColorDProperty);
        set => SetValue(ColorDProperty, value);
    }

    public static readonly DependencyProperty SpeedProperty =
        DependencyProperty.Register(nameof(Speed), typeof(double), typeof(AnimatedMeshGradient),
            new PropertyMetadata(1.0));

    /// <summary>Multiplier on the drift speed. Default 1.</summary>
    public double Speed
    {
        get => (double)GetValue(SpeedProperty);
        set => SetValue(SpeedProperty, value);
    }

    public static readonly DependencyProperty BlobOpacityProperty =
        DependencyProperty.Register(nameof(BlobOpacity), typeof(double), typeof(AnimatedMeshGradient),
            new FrameworkPropertyMetadata(0.55, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Opacity of each blob where it is strongest. Default 0.55.</summary>
    public double BlobOpacity
    {
        get => (double)GetValue(BlobOpacityProperty);
        set => SetValue(BlobOpacityProperty, value);
    }

    public static readonly DependencyProperty SpreadProperty =
        DependencyProperty.Register(nameof(Spread), typeof(double), typeof(AnimatedMeshGradient),
            new FrameworkPropertyMetadata(0.75, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Blob radius as a fraction of the smaller side. Default 0.75.</summary>
    public double Spread
    {
        get => (double)GetValue(SpreadProperty);
        set => SetValue(SpreadProperty, value);
    }

    public static readonly DependencyProperty BackgroundProperty =
        DependencyProperty.Register(nameof(Background), typeof(Brush), typeof(AnimatedMeshGradient),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Base fill under the blobs. Empty paints nothing, so the panel behind shows through.</summary>
    public Brush? Background
    {
        get => (Brush?)GetValue(BackgroundProperty);
        set => SetValue(BackgroundProperty, value);
    }

    protected override void OnRestarted()
    {
        BuildAnchors();
    }

    private void BuildAnchors()
    {
        if (_anchors.Length > 0) return;

        // Four anchors at the corners of an inset rectangle: the blobs overlap in the middle, which is
        // what turns four circles into a mesh instead of four spots.
        _anchors = new[]
        {
            new Point(0.26, 0.28),
            new Point(0.74, 0.22),
            new Point(0.30, 0.76),
            new Point(0.78, 0.72)
        };

        _phase = new double[_anchors.Length];
        for (int i = 0; i < _phase.Length; i++) _phase[i] = _random.NextDouble() * Math.PI * 2;
    }

    protected override void OnTick(double seconds)
    {
        BuildAnchors();
    }

    protected override void OnRender(DrawingContext dc)
    {
        BuildAnchors();

        double w = ActualWidth;
        double h = ActualHeight;
        if (w <= 0 || h <= 0) return;

        if (Background is { } background) dc.DrawRectangle(background, null, new Rect(0, 0, w, h));

        double t = Elapsed * Math.Max(0, Speed);
        // Radius is based on the longer side, not the shorter one: on a wide hero panel a radius of
        // half the height leaves white gaps between the anchors instead of a mesh.
        double radius = Math.Max(1, Math.Max(w, h) * 0.55 * Math.Max(0.1, Spread));
        var colors = new[] { ColorA, ColorB, ColorC, ColorD };

        for (int i = 0; i < _anchors.Length; i++)
        {
            double phase = _phase[i];

            double x = (_anchors[i].X + 0.07 * Math.Sin(t * 0.31 + phase)) * w;
            double y = (_anchors[i].Y + 0.07 * Math.Cos(t * 0.27 + phase * 1.7)) * h;

            var color = colors[i % colors.Length];
            var brush = new RadialGradientBrush
            {
                Center = new Point(0.5, 0.5),
                GradientOrigin = new Point(0.5, 0.5),
                RadiusX = 0.5,
                RadiusY = 0.5
            };

            brush.GradientStops.Add(new GradientStop(WithAlpha(color, BlobOpacity), 0));
            brush.GradientStops.Add(new GradientStop(WithAlpha(color, BlobOpacity * 0.45), 0.45));
            brush.GradientStops.Add(new GradientStop(WithAlpha(color, 0), 1));

            dc.DrawEllipse(brush, null, new Point(x, y), radius, radius);
        }
    }

    private static Color WithAlpha(Color color, double alpha) =>
        Color.FromArgb((byte)Math.Max(0, Math.Min(255, alpha * 255)), color.R, color.G, color.B);
}
