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
        Center = pos;
        IsActive = true;
        Visibility = Visibility.Visible;

        var host = VisualTreeHelper.GetParent(this) as Visual ?? _targetElement;
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

        // Draw zoomed circle
        var borderBrush = BorderBrush;
        if (borderBrush == null)
        {
            if (Application.Current != null && Application.Current.TryFindResource("CONCOST.Brush.Primary") is Brush appBrush)
            {
                borderBrush = appBrush;
            }
            else
            {
                borderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF7900"));
            }
        }

        dc.DrawEllipse(_visualBrush, new Pen(borderBrush, BorderThickness.Left > 0 ? BorderThickness.Left : 2.5),
            new Point(Radius, Radius), Radius, Radius);

        // Draw crosshair center indicator if enabled
        if (ShowCrosshair)
        {
            var crosshairPen = new Pen(borderBrush, 1.0);
            dc.DrawLine(crosshairPen, new Point(Radius - 8, Radius), new Point(Radius + 8, Radius));
            dc.DrawLine(crosshairPen, new Point(Radius, Radius - 8), new Point(Radius, Radius + 8));
            dc.DrawEllipse(null, crosshairPen, new Point(Radius, Radius), 3.5, 3.5);
        }
    }
}
