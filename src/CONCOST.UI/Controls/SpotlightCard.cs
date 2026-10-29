using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace CONCOST.UI.Controls;

/// <summary>
/// Card surface with a spotlight that follows the cursor plus an optional gradient
/// border revealed on hover — the web "spotlight card" pattern, ported to WPF.
/// </summary>
public class SpotlightCard : ContentControl
{
    private FrameworkElement? _spotlight;
    private Border? _glow;
    private Ellipse? _spotlightEllipse;
    private bool _tracking;

    static SpotlightCard()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(SpotlightCard), new FrameworkPropertyMetadata(typeof(SpotlightCard)));
    }

    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(SpotlightCard),
            new PropertyMetadata(new CornerRadius(14)));

    public static readonly DependencyProperty SpotlightBrushProperty =
        DependencyProperty.Register(nameof(SpotlightBrush), typeof(Brush), typeof(SpotlightCard),
            new PropertyMetadata(null, OnSpotlightBrushChanged));

    public static readonly DependencyProperty SpotlightSizeProperty =
        DependencyProperty.Register(nameof(SpotlightSize), typeof(double), typeof(SpotlightCard),
            new PropertyMetadata(260.0));

    public static readonly DependencyProperty SpotlightOpacityProperty =
        DependencyProperty.Register(nameof(SpotlightOpacity), typeof(double), typeof(SpotlightCard),
            new PropertyMetadata(0.20));

    public static readonly DependencyProperty IsSpotlightEnabledProperty =
        DependencyProperty.Register(nameof(IsSpotlightEnabled), typeof(bool), typeof(SpotlightCard),
            new PropertyMetadata(true));

    public static readonly DependencyProperty BorderGradientProperty =
        DependencyProperty.Register(nameof(BorderGradient), typeof(Brush), typeof(SpotlightCard),
            new PropertyMetadata(null));

    public static readonly DependencyProperty ShowHoverBorderProperty =
        DependencyProperty.Register(nameof(ShowHoverBorder), typeof(bool), typeof(SpotlightCard),
            new PropertyMetadata(true));

    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    public Brush? SpotlightBrush
    {
        get => (Brush?)GetValue(SpotlightBrushProperty);
        set => SetValue(SpotlightBrushProperty, value);
    }

    public double SpotlightSize
    {
        get => (double)GetValue(SpotlightSizeProperty);
        set => SetValue(SpotlightSizeProperty, value);
    }

    public double SpotlightOpacity
    {
        get => (double)GetValue(SpotlightOpacityProperty);
        set => SetValue(SpotlightOpacityProperty, value);
    }

    public bool IsSpotlightEnabled
    {
        get => (bool)GetValue(IsSpotlightEnabledProperty);
        set => SetValue(IsSpotlightEnabledProperty, value);
    }

    public Brush? BorderGradient
    {
        get => (Brush?)GetValue(BorderGradientProperty);
        set => SetValue(BorderGradientProperty, value);
    }

    public bool ShowHoverBorder
    {
        get => (bool)GetValue(ShowHoverBorderProperty);
        set => SetValue(ShowHoverBorderProperty, value);
    }

    private static void OnSpotlightBrushChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SpotlightCard card) card.UpdateSpotlightFill();
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _spotlight = GetTemplateChild("PART_SpotlightLayer") as FrameworkElement;
        _glow = GetTemplateChild("PART_Glow") as Border;
        _spotlightEllipse = GetTemplateChild("PART_Spotlight") as Ellipse;

        UpdateSpotlightFill();

        if (!_tracking)
        {
            MouseMove += OnMouseMoveHandler;
            MouseLeave += OnMouseLeaveHandler;
            _tracking = true;
        }
    }

    private void OnMouseMoveHandler(object sender, System.Windows.Input.MouseEventArgs e)
    {
        if (!IsSpotlightEnabled || _spotlightEllipse is null) return;

        Point position = e.GetPosition(this);
        double half = SpotlightSize / 2.0;

        _spotlightEllipse.Width = SpotlightSize;
        _spotlightEllipse.Height = SpotlightSize;
        System.Windows.Controls.Canvas.SetLeft(_spotlightEllipse, position.X - half);
        System.Windows.Controls.Canvas.SetTop(_spotlightEllipse, position.Y - half);
        _spotlightEllipse.Opacity = SpotlightOpacity;
    }

    private void OnMouseLeaveHandler(object sender, System.Windows.Input.MouseEventArgs e)
    {
        if (_spotlightEllipse is not null) _spotlightEllipse.Opacity = 0;
    }

    private void UpdateSpotlightFill()
    {
        if (_spotlightEllipse is null) return;

        Brush source = SpotlightBrush ?? (TryFindResource("CONCOST.Brush.Primary") as Brush) ?? Brushes.Orange;
        Color color = source is SolidColorBrush solid ? solid.Color : Colors.Orange;

        var brush = new RadialGradientBrush
        {
            GradientOrigin = new Point(0.5, 0.5),
            Center = new Point(0.5, 0.5),
            RadiusX = 0.5,
            RadiusY = 0.5
        };
        brush.GradientStops.Add(new GradientStop(color, 0.0));
        brush.GradientStops.Add(new GradientStop(Color.FromArgb(0, color.R, color.G, color.B), 0.75));

        _spotlightEllipse.Fill = brush;
    }

    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
        base.OnRenderSizeChanged(sizeInfo);
        if (_glow is not null && _glow.CornerRadius != CornerRadius) _glow.CornerRadius = CornerRadius;
    }
}
