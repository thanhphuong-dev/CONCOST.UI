using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CONCOST.UI.Controls;

/// <summary>
/// Thin reading-progress bar for a <see cref="ScrollViewer"/>. When no <see cref="Target"/> is
/// set it walks up the visual tree to the nearest scroll viewer, so dropping it above a page
/// is usually enough.
/// </summary>
public class ScrollProgressBar : FrameworkElement
{
    private ScrollViewer? _hooked;
    private double _fraction;

    public static readonly DependencyProperty TargetProperty =
        DependencyProperty.Register(nameof(Target), typeof(ScrollViewer), typeof(ScrollProgressBar),
            new PropertyMetadata(null, OnTargetChanged));

    public static readonly DependencyProperty TrackBrushProperty =
        DependencyProperty.Register(nameof(TrackBrush), typeof(Brush), typeof(ScrollProgressBar),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty BarBrushProperty =
        DependencyProperty.Register(nameof(BarBrush), typeof(Brush), typeof(ScrollProgressBar),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty ThicknessProperty =
        DependencyProperty.Register(nameof(Thickness), typeof(double), typeof(ScrollProgressBar),
            new FrameworkPropertyMetadata(3.0, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.Register(nameof(CornerRadius), typeof(double), typeof(ScrollProgressBar),
            new FrameworkPropertyMetadata(2.0, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty ShowTrackProperty =
        DependencyProperty.Register(nameof(ShowTrack), typeof(bool), typeof(ScrollProgressBar),
            new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Scroll viewer to track. Leave empty to use the nearest ancestor.</summary>
    public ScrollViewer? Target
    {
        get => (ScrollViewer?)GetValue(TargetProperty);
        set => SetValue(TargetProperty, value);
    }

    public Brush? TrackBrush
    {
        get => (Brush?)GetValue(TrackBrushProperty);
        set => SetValue(TrackBrushProperty, value);
    }

    public Brush? BarBrush
    {
        get => (Brush?)GetValue(BarBrushProperty);
        set => SetValue(BarBrushProperty, value);
    }

    public double Thickness
    {
        get => (double)GetValue(ThicknessProperty);
        set => SetValue(ThicknessProperty, value);
    }

    public double CornerRadius
    {
        get => (double)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    public bool ShowTrack
    {
        get => (bool)GetValue(ShowTrackProperty);
        set => SetValue(ShowTrackProperty, value);
    }

    /// <summary>0–1 reading progress of the tracked scroll viewer.</summary>
    public double Fraction => _fraction;

    public ScrollProgressBar()
    {
        IsHitTestVisible = false;
        Loaded += OnLoaded;
        Unloaded += (_, _) => Detach();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (Target != null) { Attach(Target); return; }

        DependencyObject? current = VisualTreeHelper.GetParent(this);
        while (current != null)
        {
            if (current is ScrollViewer viewer) { Attach(viewer); return; }
            current = VisualTreeHelper.GetParent(current);
        }
    }

    private static void OnTargetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not ScrollProgressBar bar) return;
        if (e.OldValue is ScrollViewer old) old.ScrollChanged -= bar.OnScrollChanged;
        bar._hooked = null;
        if (e.NewValue is ScrollViewer viewer) bar.Attach(viewer);
    }

    private void Attach(ScrollViewer viewer)
    {
        if (ReferenceEquals(_hooked, viewer)) return;
        Detach();
        _hooked = viewer;
        viewer.ScrollChanged += OnScrollChanged;
        UpdateFraction();
    }

    private void Detach()
    {
        if (_hooked is null) return;
        _hooked.ScrollChanged -= OnScrollChanged;
        _hooked = null;
    }

    private void OnScrollChanged(object sender, ScrollChangedEventArgs e) => UpdateFraction();

    private void UpdateFraction()
    {
        if (_hooked is null) return;

        double scrollable = _hooked.ScrollableHeight;
        double fraction = scrollable <= 0.5 ? 1 : _hooked.VerticalOffset / scrollable;
        fraction = Math.Max(0, Math.Min(1, fraction));

        if (Math.Abs(fraction - _fraction) < 0.0005) return;
        _fraction = fraction;
        InvalidateVisual();
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        double width = double.IsInfinity(availableSize.Width) ? 240 : availableSize.Width;
        return new Size(width, Math.Max(1, Thickness));
    }

    protected override void OnRender(DrawingContext dc)
    {
        double width = ActualWidth;
        double height = ActualHeight;
        if (width <= 1 || height <= 0.5) return;

        double radius = Math.Max(0, Math.Min(CornerRadius, height / 2));

        if (ShowTrack)
        {
            Brush track = TrackBrush ?? TryFindResource("CONCOST.Brush.Border") as Brush ?? Brushes.Gainsboro;
            dc.DrawRoundedRectangle(track, null, new Rect(0, 0, width, height), radius, radius);
        }

        double barWidth = width * _fraction;
        if (barWidth <= 0.5) return;

        Brush bar = BarBrush ?? TryFindResource("CONCOST.Brush.Primary") as Brush ?? Brushes.Orange;
        dc.DrawRoundedRectangle(bar, null, new Rect(0, 0, barWidth, height), radius, radius);
    }
}
