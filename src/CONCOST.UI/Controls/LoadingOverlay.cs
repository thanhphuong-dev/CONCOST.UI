using System.Windows;
using System.Windows.Controls;
using System;

namespace CONCOST.UI.Controls;

public enum LoadingOverlayVariant
{
    Standard,
    NeonGlow,
    GlassMorphism,
    MinimalDot,
    PulsingRing,
    LiquidBlob
}

public class LoadingOverlay : ContentControl
{
    static LoadingOverlay()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(LoadingOverlay), new FrameworkPropertyMetadata(typeof(LoadingOverlay)));
    }

    public static readonly DependencyProperty IsLoadingProperty =
        DependencyProperty.Register(nameof(IsLoading), typeof(bool), typeof(LoadingOverlay),
            new PropertyMetadata(false));

    public static readonly DependencyProperty MessageProperty =
        DependencyProperty.Register(nameof(Message), typeof(string), typeof(LoadingOverlay),
            new PropertyMetadata("Loading calculations..."));

    public static readonly DependencyProperty SubMessageProperty =
        DependencyProperty.Register(nameof(SubMessage), typeof(string), typeof(LoadingOverlay),
            new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty ProgressProperty =
        DependencyProperty.Register(nameof(Progress), typeof(double), typeof(LoadingOverlay),
            new PropertyMetadata(0.0, null, CoerceProgress));

    public static readonly DependencyProperty IsIndeterminateProperty =
        DependencyProperty.Register(nameof(IsIndeterminate), typeof(bool), typeof(LoadingOverlay),
            new PropertyMetadata(true));

    public static readonly DependencyProperty IndicatorTypeProperty =
        DependencyProperty.Register(nameof(IndicatorType), typeof(LoadingIndicatorType), typeof(LoadingOverlay),
            new PropertyMetadata(LoadingIndicatorType.ThreeDotsWave));

    public static readonly DependencyProperty VariantProperty =
        DependencyProperty.Register(nameof(Variant), typeof(LoadingOverlayVariant), typeof(LoadingOverlay),
            new PropertyMetadata(LoadingOverlayVariant.Standard));

    public static readonly DependencyProperty GlowColorProperty =
        DependencyProperty.Register(nameof(GlowColor), typeof(System.Windows.Media.Brush), typeof(LoadingOverlay),
            new PropertyMetadata(null));

    public static readonly DependencyProperty ShowProgressRingProperty =
        DependencyProperty.Register(nameof(ShowProgressRing), typeof(bool), typeof(LoadingOverlay),
            new PropertyMetadata(false));

    public static readonly DependencyProperty BlurRadiusProperty =
        DependencyProperty.Register(nameof(BlurRadius), typeof(double), typeof(LoadingOverlay),
            new PropertyMetadata(10.0));

    public bool IsLoading
    {
        get => (bool)GetValue(IsLoadingProperty);
        set => SetValue(IsLoadingProperty, value);
    }

    public string Message
    {
        get => (string)GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    public string SubMessage
    {
        get => (string)GetValue(SubMessageProperty);
        set => SetValue(SubMessageProperty, value);
    }

    public double Progress
    {
        get => (double)GetValue(ProgressProperty);
        set => SetValue(ProgressProperty, value);
    }

    public bool IsIndeterminate
    {
        get => (bool)GetValue(IsIndeterminateProperty);
        set => SetValue(IsIndeterminateProperty, value);
    }

    public LoadingIndicatorType IndicatorType
    {
        get => (LoadingIndicatorType)GetValue(IndicatorTypeProperty);
        set => SetValue(IndicatorTypeProperty, value);
    }

    public LoadingOverlayVariant Variant
    {
        get => (LoadingOverlayVariant)GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }

    public System.Windows.Media.Brush? GlowColor
    {
        get => (System.Windows.Media.Brush?)GetValue(GlowColorProperty);
        set => SetValue(GlowColorProperty, value);
    }

    public bool ShowProgressRing
    {
        get => (bool)GetValue(ShowProgressRingProperty);
        set => SetValue(ShowProgressRingProperty, value);
    }

    public double BlurRadius
    {
        get => (double)GetValue(BlurRadiusProperty);
        set => SetValue(BlurRadiusProperty, value);
    }

    private static object CoerceProgress(DependencyObject d, object value)
    {
        var progress = (double)value;
        return NumericCompatibility.IsFinite(progress) ? NumericCompatibility.Clamp(progress, 0d, 100d) : 0d;
    }
}
