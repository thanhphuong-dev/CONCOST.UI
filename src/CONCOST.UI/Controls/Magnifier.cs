using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CONCOST.UI.Controls;

/// <summary>
/// Magnifier control that provides a floating zoom lens over any visual element.
/// Supports configurable zoom factor, lens radius, border brush, and border thickness.
/// </summary>
public class Magnifier : Control
{
    private FrameworkElement? _targetElement;
    private VisualBrush? _visualBrush;
    private Point _lastPositionInHost;
    private bool _hasPointerPosition;

    static Magnifier()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(Magnifier), new FrameworkPropertyMetadata(typeof(Magnifier)));
    }

    public Magnifier()
    {
        IsHitTestVisible = false;
        HorizontalAlignment = HorizontalAlignment.Left;
        VerticalAlignment = VerticalAlignment.Top;
        Visibility = Visibility.Collapsed;
        Width = Radius * 2;
        Height = Radius * 2;
        Effect = CreateShadow();
    }

    public static readonly DependencyProperty TargetProperty =
        DependencyProperty.Register(nameof(Target), typeof(FrameworkElement), typeof(Magnifier),
            new PropertyMetadata(null, OnTargetChanged));

    public static readonly DependencyProperty ScaleProperty =
        DependencyProperty.Register(nameof(Scale), typeof(double), typeof(Magnifier),
            new FrameworkPropertyMetadata(2.5, FrameworkPropertyMetadataOptions.None, OnVisualParamChanged, CoerceScale));

    public static readonly DependencyProperty RadiusProperty =
        DependencyProperty.Register(nameof(Radius), typeof(double), typeof(Magnifier),
            new FrameworkPropertyMetadata(60.0, FrameworkPropertyMetadataOptions.None, OnVisualParamChanged, CoerceRadius));

    public static readonly DependencyProperty ShowCrosshairProperty =
        DependencyProperty.Register(nameof(ShowCrosshair), typeof(bool), typeof(Magnifier),
            new PropertyMetadata(true, OnVisualParamChanged));

    public static readonly DependencyProperty CrosshairBrushProperty =
        DependencyProperty.Register(nameof(CrosshairBrush), typeof(Brush), typeof(Magnifier),
            new PropertyMetadata(null, OnVisualParamChanged));

    public static readonly DependencyProperty ShowRimProperty =
        DependencyProperty.Register(nameof(ShowRim), typeof(bool), typeof(Magnifier),
            new PropertyMetadata(true, OnVisualParamChanged));

    public static readonly DependencyProperty ShowShadowProperty =
        DependencyProperty.Register(nameof(ShowShadow), typeof(bool), typeof(Magnifier),
            new PropertyMetadata(true, OnShadowChanged));

    public static readonly DependencyProperty FollowPointerProperty =
        DependencyProperty.Register(nameof(FollowPointer), typeof(bool), typeof(Magnifier),
            new PropertyMetadata(true));

    public static readonly DependencyProperty CenterProperty =
        DependencyProperty.Register(nameof(Center), typeof(Point), typeof(Magnifier),
            new PropertyMetadata(new Point(0, 0)));

    public static readonly DependencyProperty IsActiveProperty =
        DependencyProperty.Register(nameof(IsActive), typeof(bool), typeof(Magnifier),
            new PropertyMetadata(false));

    public FrameworkElement? Target
    {
        get => (FrameworkElement?)GetValue(TargetProperty);
        set => SetValue(TargetProperty, value);
    }

    public double Scale
    {
        get => (double)GetValue(ScaleProperty);
        set => SetValue(ScaleProperty, value);
    }

    public double Radius
    {
        get => (double)GetValue(RadiusProperty);
        set => SetValue(RadiusProperty, value);
    }

    public bool ShowCrosshair
    {
        get => (bool)GetValue(ShowCrosshairProperty);
        set => SetValue(ShowCrosshairProperty, value);
    }

    /// <summary>Colour of the crosshair and centre mark; the ring brush when unset.</summary>
    public Brush? CrosshairBrush
    {
        get => (Brush?)GetValue(CrosshairBrushProperty);
        set => SetValue(CrosshairBrushProperty, value);
    }

    /// <summary>
    /// Draws a light inner rim between the ring and the image. On a drawing the zoomed content is
    /// often the same colour as the ring, and the rim is what keeps the lens readable as a lens.
    /// </summary>
    public bool ShowRim
    {
        get => (bool)GetValue(ShowRimProperty);
        set => SetValue(ShowRimProperty, value);
    }

    /// <summary>Draws a soft drop under the lens, so it reads as floating above the drawing.</summary>
    public bool ShowShadow
    {
        get => (bool)GetValue(ShowShadowProperty);
        set => SetValue(ShowShadowProperty, value);
    }

    /// <summary>
    /// Moves the lens with the pointer. Turn it off when the host drives <see cref="Center"/> from
    /// code — a digitizer, a camera feed, a scripted zoom — so the pointer stops fighting the host.
    /// </summary>
    public bool FollowPointer
    {
        get => (bool)GetValue(FollowPointerProperty);
        set => SetValue(FollowPointerProperty, value);
    }

    public Point Center
    {
        get => (Point)GetValue(CenterProperty);
        set => SetValue(CenterProperty, value);
    }

    public bool IsActive
    {
        get => (bool)GetValue(IsActiveProperty);
        set => SetValue(IsActiveProperty, value);
    }

    private static void OnVisualParamChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Magnifier m)
        {
            m.UpdateLensSizeAndPosition();
            m.InvalidateVisual();
        }
    }

    private static void OnShadowChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        // DrawingContext has no Effect overload (only the legacy BitmapEffect), so the shadow is a
        // control-level Effect — one frozen instance, swapped in and out with the switch.
        if (d is not Magnifier m) return;

        if ((bool)e.NewValue)
        {
            m.Effect ??= CreateShadow();
        }
        else
        {
            m.Effect = null;
        }
    }

    private static System.Windows.Media.Effects.DropShadowEffect CreateShadow() =>
        new()
        {
            BlurRadius = 12,
            ShadowDepth = 3,
            Direction = 270,
            Opacity = 0.35
        };

    private static object CoerceScale(DependencyObject d, object baseValue)
    {
        var value = (double)baseValue;
        return NumericCompatibility.IsFinite(value) && value > 0 ? NumericCompatibility.Clamp(value, 1.1, 12.0) : 2.5;
    }

    private static object CoerceRadius(DependencyObject d, object baseValue)
    {
        var value = (double)baseValue;
        return NumericCompatibility.IsFinite(value) ? NumericCompatibility.Clamp(value, 12.0, 500.0) : 60.0;
    }

    private static void OnTargetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Magnifier magnifier)
        {
            if (e.OldValue is FrameworkElement oldElem)
            {
                oldElem.PreviewMouseMove -= magnifier.OnTargetMouseMove;
                oldElem.MouseEnter -= magnifier.OnTargetMouseEnter;
                oldElem.MouseLeave -= magnifier.OnTargetMouseLeave;
            }

            if (e.NewValue is FrameworkElement newElem)
            {
                magnifier._targetElement = newElem;
                magnifier._visualBrush = new VisualBrush(newElem)
                {
                    ViewboxUnits = BrushMappingMode.Absolute,
                    Stretch = Stretch.None
                };

                newElem.PreviewMouseMove += magnifier.OnTargetMouseMove;
                newElem.MouseEnter += magnifier.OnTargetMouseEnter;
                newElem.MouseLeave += magnifier.OnTargetMouseLeave;
            }
            else
            {
                magnifier._targetElement = null;
                magnifier._visualBrush = null;
                magnifier._hasPointerPosition = false;
                magnifier.IsActive = false;
                magnifier.Visibility = Visibility.Collapsed;
            }
        }
    }

    private void OnTargetMouseEnter(object sender, MouseEventArgs e)
    {
        IsActive = true;
        Visibility = Visibility.Visible;
    }

    private void OnTargetMouseLeave(object sender, MouseEventArgs e)
    {
        IsActive = false;
        Visibility = Visibility.Collapsed;
    }

    private void OnTargetMouseMove(object sender, MouseEventArgs e)
    {
        if (_targetElement == null) return;

        Point pos = e.GetPosition(_targetElement);

        // A host driving the lens from code owns the Centre; the pointer only reports where it is.
        if (FollowPointer) Center = pos;

        IsActive = true;
        Visibility = Visibility.Visible;

        var host = VisualTreeHelper.GetParent(this) as Visual ?? _targetElement;

        if (FollowPointer)
        {
            try
            {
                _lastPositionInHost = _targetElement.TransformToVisual(host).Transform(pos);
            }
            catch (InvalidOperationException)
            {
                // The target may not share a presentation source yet; use target-local coordinates until it is connected.
                _lastPositionInHost = pos;
            }

            _hasPointerPosition = true;
            UpdateLensSizeAndPosition();
        }

        InvalidateVisual();
    }

    private void UpdateLensSizeAndPosition()
    {
        Width = Radius * 2;
        Height = Radius * 2;
        if (_hasPointerPosition)
            Margin = new Thickness(_lastPositionInHost.X - Radius, _lastPositionInHost.Y - Radius, 0, 0);
    }

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);

        if (!IsActive || _targetElement == null || _visualBrush == null || !NumericCompatibility.IsFinite(Scale) || Scale <= 0) return;

        double size = Radius * 2;
        double viewboxSize = size / Scale;

        _visualBrush.Viewbox = new Rect(
            Center.X - viewboxSize / 2,
            Center.Y - viewboxSize / 2,
            viewboxSize,
            viewboxSize
        );

        // Draw zoomed circle. The ring falls through the local brush, the theme's accent and a
        // neutral fallback, in that order — never a hard-coded brand colour. The shadow is the
        // control-level Effect, not a PushEffect: DrawingContext only takes the legacy kind.
        var borderBrush = BorderBrush ?? TryFindResource("CONCOST.Brush.Primary") as Brush ?? Brushes.Orange;
        double ringThickness = BorderThickness.Left > 0 ? BorderThickness.Left : 2.5;

        dc.DrawEllipse(_visualBrush, new Pen(borderBrush, ringThickness), new Point(Radius, Radius), Radius, Radius);

        // The rim: a light ring just inside the border, which is what keeps the lens readable when
        // the zoomed drawing happens to be the same colour as the ring.
        if (ShowRim)
        {
            var rim = TryFindResource("CONCOST.Brush.Surface") as Brush ?? Brushes.White;
            var rimPen = new Pen(rim, Math.Max(1.5, ringThickness * 0.6));

            dc.DrawEllipse(null, rimPen, new Point(Radius, Radius), Radius - ringThickness * 0.8, Radius - ringThickness * 0.8);
        }

        // Crosshair center indicator if enabled — its own brush, because the ring colour can vanish
        // against a busy drawing and the crosshair is the part the eye actually steers by.
        if (ShowCrosshair)
        {
            var crosshair = CrosshairBrush ?? borderBrush;
            var crosshairPen = new Pen(crosshair, 1.0);

            dc.DrawLine(crosshairPen, new Point(Radius - 8, Radius), new Point(Radius + 8, Radius));
            dc.DrawLine(crosshairPen, new Point(Radius, Radius - 8), new Point(Radius, Radius + 8));
            dc.DrawEllipse(null, crosshairPen, new Point(Radius, Radius), 3.5, 3.5);
        }
    }
}
