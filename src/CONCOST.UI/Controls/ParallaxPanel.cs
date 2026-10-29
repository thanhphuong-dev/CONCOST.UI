using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CONCOST.UI.Controls;

/// <summary>
/// Layer that drifts slower (or faster) than the page as it scrolls — the parallax depth
/// trick. Put it behind the content and give it a speed below 1; it translates itself, so
/// nothing inside has to know about the scroll position.
/// </summary>
public class ParallaxPanel : ContentControl
{
    private ScrollViewer? _hooked;
    private readonly TranslateTransform _translate = new();

    public static readonly DependencyProperty TargetProperty =
        DependencyProperty.Register(nameof(Target), typeof(ScrollViewer), typeof(ParallaxPanel),
            new PropertyMetadata(null, OnTargetChanged));

    public static readonly DependencyProperty SpeedProperty =
        DependencyProperty.Register(nameof(Speed), typeof(double), typeof(ParallaxPanel),
            new PropertyMetadata(0.18));

    public static readonly DependencyProperty MaxOffsetProperty =
        DependencyProperty.Register(nameof(MaxOffset), typeof(double), typeof(ParallaxPanel),
            new PropertyMetadata(140.0));

    public static readonly DependencyProperty AxisProperty =
        DependencyProperty.Register(nameof(Axis), typeof(Orientation), typeof(ParallaxPanel),
            new PropertyMetadata(Orientation.Vertical));

    static ParallaxPanel()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(ParallaxPanel), new FrameworkPropertyMetadata(typeof(ParallaxPanel)));
    }

    public ParallaxPanel()
    {
        RenderTransform = _translate;
        IsHitTestVisible = false;
        Loaded += OnLoaded;
        Unloaded += (_, _) => Detach();
    }

    /// <summary>Scroll viewer to follow. Leave empty to use the nearest ancestor.</summary>
    public ScrollViewer? Target
    {
        get => (ScrollViewer?)GetValue(TargetProperty);
        set => SetValue(TargetProperty, value);
    }

    /// <summary>Fraction of the scroll distance the layer moves (0.1–0.4 = subtle depth).</summary>
    public double Speed
    {
        get => (double)GetValue(SpeedProperty);
        set => SetValue(SpeedProperty, value);
    }

    /// <summary>Clamp so the layer never drifts more than this many pixels.</summary>
    public double MaxOffset
    {
        get => (double)GetValue(MaxOffsetProperty);
        set => SetValue(MaxOffsetProperty, value);
    }

    public Orientation Axis
    {
        get => (Orientation)GetValue(AxisProperty);
        set => SetValue(AxisProperty, value);
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (Target is not null) { Attach(Target); return; }

        DependencyObject? current = VisualTreeHelper.GetParent(this);
        while (current is not null)
        {
            if (current is ScrollViewer viewer) { Attach(viewer); return; }
            current = VisualTreeHelper.GetParent(current);
        }
    }

    private static void OnTargetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not ParallaxPanel panel) return;
        if (e.OldValue is ScrollViewer old) old.ScrollChanged -= panel.OnScrollChanged;
        panel._hooked = null;
        if (e.NewValue is ScrollViewer viewer) panel.Attach(viewer);
    }

    private void Attach(ScrollViewer viewer)
    {
        if (ReferenceEquals(_hooked, viewer)) return;
        Detach();
        _hooked = viewer;
        viewer.ScrollChanged += OnScrollChanged;
        Update();
    }

    private void Detach()
    {
        if (_hooked is null) return;
        _hooked.ScrollChanged -= OnScrollChanged;
        _hooked = null;
    }

    private void OnScrollChanged(object sender, ScrollChangedEventArgs e) => Update();

    private void Update()
    {
        if (_hooked is null) return;

        double speed = Math.Max(-2, Math.Min(2, Speed));
        double limit = Math.Max(0, MaxOffset);

        double offset = Axis == Orientation.Horizontal
            ? _hooked.HorizontalOffset
            : _hooked.VerticalOffset;

        double shift = Math.Max(-limit, Math.Min(limit, -offset * speed));

        if (Axis == Orientation.Horizontal)
        {
            _translate.X = shift;
            _translate.Y = 0;
        }
        else
        {
            _translate.X = 0;
            _translate.Y = shift;
        }
    }
}
