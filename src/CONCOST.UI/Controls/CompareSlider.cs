using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace CONCOST.UI.Controls;

/// <summary>
/// CompareSlider splits two visual contents (e.g. Before / After CAD drawings, 3D renderings, revisions)
/// with a draggable split handle and smooth clipping mask.
/// </summary>
public class CompareSlider : Control
{
    private FrameworkElement? _handle;
    private FrameworkElement? _targetContentPresenter;
    private bool _isDragging;

    static CompareSlider()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(CompareSlider), new FrameworkPropertyMetadata(typeof(CompareSlider)));
    }

    public static readonly DependencyProperty SourceContentProperty =
        DependencyProperty.Register(nameof(SourceContent), typeof(object), typeof(CompareSlider),
            new PropertyMetadata(null));

    public static readonly DependencyProperty TargetContentProperty =
        DependencyProperty.Register(nameof(TargetContent), typeof(object), typeof(CompareSlider),
            new PropertyMetadata(null));

    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(nameof(Value), typeof(double), typeof(CompareSlider),
            new FrameworkPropertyMetadata(0.5, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValueChanged, CoerceValue));

    public static readonly DependencyProperty OrientationProperty =
        DependencyProperty.Register(nameof(Orientation), typeof(Orientation), typeof(CompareSlider),
            new PropertyMetadata(Orientation.Horizontal));

    public static readonly DependencyProperty HandleSizeProperty =
        DependencyProperty.Register(nameof(HandleSize), typeof(double), typeof(CompareSlider),
            new PropertyMetadata(36.0));

    public object? SourceContent
    {
        get => GetValue(SourceContentProperty);
        set => SetValue(SourceContentProperty, value);
    }

    public object? TargetContent
    {
        get => GetValue(TargetContentProperty);
        set => SetValue(TargetContentProperty, value);
    }

    public double Value
    {
        get => (double)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public Orientation Orientation
    {
        get => (Orientation)GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    public double HandleSize
    {
        get => (double)GetValue(HandleSizeProperty);
        set => SetValue(HandleSizeProperty, value);
    }

    public override void OnApplyTemplate()
    {
        if (_handle != null)
        {
            _handle.MouseLeftButtonDown -= OnHandleMouseDown;
            _handle.MouseLeftButtonUp -= OnHandleMouseUp;
            _handle.MouseMove -= OnHandleMouseMove;
        }
        MouseLeftButtonDown -= OnControlMouseDown;
        MouseMove -= OnControlMouseMove;
        MouseLeftButtonUp -= OnControlMouseUp;
        SizeChanged -= OnSliderSizeChanged;
        base.OnApplyTemplate();

        _handle = GetTemplateChild("PART_Handle") as FrameworkElement;
        _targetContentPresenter = GetTemplateChild("PART_TargetContent") as FrameworkElement;

        if (_handle != null)
        {
            _handle.MouseLeftButtonDown += OnHandleMouseDown;
            _handle.MouseLeftButtonUp += OnHandleMouseUp;
            _handle.MouseMove += OnHandleMouseMove;
        }

        MouseLeftButtonDown += OnControlMouseDown;
        MouseMove += OnControlMouseMove;
        MouseLeftButtonUp += OnControlMouseUp;
        SizeChanged += OnSliderSizeChanged;

        UpdateClip();
    }

    private static object CoerceValue(DependencyObject d, object value)
    {
        var number = (double)value;
        return NumericCompatibility.IsFinite(number) ? NumericCompatibility.Clamp(number, 0d, 1d) : 0.5d;
    }

    private void OnSliderSizeChanged(object sender, SizeChangedEventArgs e) => UpdateClip();

    private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is CompareSlider slider)
        {
            slider.UpdateClip();
        }
    }

    private void OnHandleMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton != MouseButton.Left) return;
        _isDragging = true;
        _handle?.CaptureMouse();
        e.Handled = true;
    }

    private void OnHandleMouseUp(object sender, MouseButtonEventArgs e)
    {
        if (_isDragging)
        {
            _isDragging = false;
            _handle?.ReleaseMouseCapture();
            e.Handled = true;
        }
    }

    private void OnHandleMouseMove(object sender, MouseEventArgs e)
    {
        if (!_isDragging) return;
        UpdateValueFromPoint(e.GetPosition(this));
    }

    private void OnControlMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton != MouseButton.Left || e.OriginalSource is DependencyObject source && FindVisualParent<ButtonBase>(source) != null) return;
        _isDragging = true;
        CaptureMouse();
        UpdateValueFromPoint(e.GetPosition(this));
        e.Handled = true;
    }

    private static T? FindVisualParent<T>(DependencyObject child) where T : DependencyObject
    {
        for (var parent = VisualTreeHelper.GetParent(child); parent != null; parent = VisualTreeHelper.GetParent(parent))
            if (parent is T match) return match;
        return null;
    }

    private void OnControlMouseMove(object sender, MouseEventArgs e)
    {
        if (!_isDragging) return;
        UpdateValueFromPoint(e.GetPosition(this));
    }

    private void OnControlMouseUp(object sender, MouseButtonEventArgs e)
    {
        if (_isDragging)
        {
            _isDragging = false;
            ReleaseMouseCapture();
            e.Handled = true;
        }
    }

    private void UpdateValueFromPoint(Point point)
    {
        if (ActualWidth <= 0 || ActualHeight <= 0) return;

        double progress = Orientation == Orientation.Horizontal
            ? point.X / ActualWidth
            : point.Y / ActualHeight;

        Value = Math.Max(0.0, Math.Min(1.0, progress));
    }

    private void UpdateClip()
    {
        if (ActualWidth <= 0 || ActualHeight <= 0) return;

        double w = ActualWidth;
        double h = ActualHeight;
        double splitPos = (Orientation == Orientation.Horizontal ? w : h) * Value;

        if (_targetContentPresenter != null)
        {
            Rect clipRect = Orientation == Orientation.Horizontal
                ? new Rect(0, 0, splitPos, h)
                : new Rect(0, 0, w, splitPos);

            _targetContentPresenter.Clip = new RectangleGeometry(clipRect);
        }

        var handleGrid = GetTemplateChild("PART_HandleGrid") as FrameworkElement;
        if (handleGrid != null)
        {
            handleGrid.Margin = Orientation == Orientation.Horizontal
                ? new Thickness(splitPos - handleGrid.ActualWidth / 2.0, 0, 0, 0)
                : new Thickness(0, splitPos - handleGrid.ActualHeight / 2.0, 0, 0);
        }
    }
}
