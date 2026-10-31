using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace CONCOST.UI.Controls;

/// <summary>
/// Darkens everything except a soft circle around the pointer — the "look here" overlay for onboarding,
/// empty states and product tours.
/// </summary>
/// <remarks>
/// <para>
/// The overlay is not hit-testable, so it never steals a click from the page it covers. Following the
/// cursor without hit testing works because <see cref="Mouse.GetPosition(Visual)"/> reads the input
/// manager's current position rather than hit-testing — the overlay can track a pointer it cannot
/// receive events from.
/// </para>
/// <para>
/// <see cref="SpotlightCenter"/> is a dependency property, so a host can drive the circle from its own
/// state (a tour step, a selected control) instead of the cursor.
/// </para>
/// </remarks>
public class CursorSpotlight : AnimatedEffectBase
{
    private bool _hasCentre;

    public static readonly DependencyProperty SpotDimProperty =
        DependencyProperty.Register(nameof(SpotDim), typeof(double), typeof(CursorSpotlight),
            new FrameworkPropertyMetadata(0.55, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>How dark everything outside the circle gets, 0–1. Default 0.55.</summary>
    public double SpotDim
    {
        get => (double)GetValue(SpotDimProperty);
        set => SetValue(SpotDimProperty, value);
    }

    public static readonly DependencyProperty RadiusProperty =
        DependencyProperty.Register(nameof(Radius), typeof(double), typeof(CursorSpotlight),
            new FrameworkPropertyMetadata(160.0, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Radius of the clear circle, in pixels. Default 160.</summary>
    public double Radius
    {
        get => (double)GetValue(RadiusProperty);
        set => SetValue(RadiusProperty, value);
    }

    public static readonly DependencyProperty FeatherProperty =
        DependencyProperty.Register(nameof(Feather), typeof(double), typeof(CursorSpotlight),
            new FrameworkPropertyMetadata(0.45, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>How much of the radius is the soft edge, 0–1. Default 0.45.</summary>
    public double Feather
    {
        get => (double)GetValue(FeatherProperty);
        set => SetValue(FeatherProperty, value);
    }

    public static readonly DependencyProperty DimColorProperty =
        DependencyProperty.Register(nameof(DimColor), typeof(Color), typeof(CursorSpotlight),
            new FrameworkPropertyMetadata(Colors.Black, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Colour of the dimmed area. Default black.</summary>
    public Color DimColor
    {
        get => (Color)GetValue(DimColorProperty);
        set => SetValue(DimColorProperty, value);
    }

    public static readonly DependencyProperty SpotColorProperty =
        DependencyProperty.Register(nameof(SpotColor), typeof(Color), typeof(CursorSpotlight),
            new FrameworkPropertyMetadata(Colors.Transparent, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Tint inside the circle. Transparent leaves the content untouched. Default transparent.</summary>
    public Color SpotColor
    {
        get => (Color)GetValue(SpotColorProperty);
        set => SetValue(SpotColorProperty, value);
    }

    public static readonly DependencyProperty FollowCursorProperty =
        DependencyProperty.Register(nameof(FollowCursor), typeof(bool), typeof(CursorSpotlight),
            new PropertyMetadata(true));

    /// <summary>Track the pointer. Set to <c>false</c> to drive <see cref="SpotlightCenter"/> yourself.</summary>
    public bool FollowCursor
    {
        get => (bool)GetValue(FollowCursorProperty);
        set => SetValue(FollowCursorProperty, value);
    }

    public static readonly DependencyProperty SpotlightCenterProperty =
        DependencyProperty.Register(nameof(SpotlightCenter), typeof(Point), typeof(CursorSpotlight),
            new FrameworkPropertyMetadata(new Point(double.NaN, double.NaN), FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Centre of the circle, in this control's coordinates. NaN means "follow the cursor".</summary>
    public Point SpotlightCenter
    {
        get => (Point)GetValue(SpotlightCenterProperty);
        set => SetValue(SpotlightCenterProperty, value);
    }

    protected override void OnTick(double seconds)
    {
        if (!FollowCursor) return;

        var centre = SpotlightCenter;

        // An explicit centre wins; only track the pointer when the host has not set one.
        if (!double.IsNaN(centre.X) && !double.IsNaN(centre.Y)) return;

        var mouse = Mouse.GetPosition(this);

        if (mouse.X < 0 || mouse.Y < 0 || mouse.X > ActualWidth || mouse.Y > ActualHeight)
        {
            if (_hasCentre) return;
            mouse = new Point(ActualWidth / 2, ActualHeight / 2);
        }

        _hasCentre = true;
        _centre = mouse;
        InvalidateVisual();
    }

    private Point _centre;

    protected override void OnRender(DrawingContext dc)
    {
        double w = ActualWidth;
        double h = ActualHeight;
        if (w <= 0 || h <= 0) return;

        var centre = SpotlightCenter;
        if (double.IsNaN(centre.X) || double.IsNaN(centre.Y)) centre = _centre;

        if (centre.X == 0 && centre.Y == 0 && !_hasCentre) centre = new Point(w / 2, h / 2);

        double radius = Math.Max(1, Radius);
        double feather = Math.Max(0.01, Math.Min(0.99, Feather));

        var brush = new RadialGradientBrush
        {
            MappingMode = BrushMappingMode.Absolute,
            Center = centre,
            GradientOrigin = centre,
            RadiusX = radius,
            RadiusY = radius,
            SpreadMethod = GradientSpreadMethod.Pad
        };

        brush.GradientStops.Add(new GradientStop(WithAlpha(SpotColor, SpotColor.A / 255.0), 0));
        brush.GradientStops.Add(new GradientStop(WithAlpha(SpotColor, SpotColor.A / 255.0), Math.Max(0.01, 1 - feather)));
        brush.GradientStops.Add(new GradientStop(WithAlpha(DimColor, SpotDim), 1));

        // Fill the whole control with the brush, not just a circle: with Pad spreading, everything
        // outside the radius keeps the last stop, which is the dim. Drawing only an ellipse would paint
        // a dark ring in the middle and leave the edges clear — the exact opposite of a spotlight.
        dc.DrawRectangle(brush, null, new Rect(0, 0, w, h));
    }

    private static Color WithAlpha(Color color, double alpha) =>
        Color.FromArgb((byte)Math.Max(0, Math.Min(255, alpha * 255)), color.R, color.G, color.B);
}
