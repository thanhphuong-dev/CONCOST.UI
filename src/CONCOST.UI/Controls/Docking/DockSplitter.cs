using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CONCOST.UI.Controls;

/// <summary>
/// The draggable divider between two docked areas.
/// </summary>
/// <remarks>
/// A <see cref="Orientation.Vertical"/> splitter is a thin vertical bar that resizes horizontally;
/// <see cref="Orientation.Horizontal"/> is the opposite. Dragging updates <see cref="Value"/>, which
/// the host reads to resize the panel on one side of it.
/// </remarks>
public class DockSplitter : FrameworkElement
{
    public DockSplitter()
    {
        Focusable = false;
        Cursor = Cursors.SizeWE;
    }

    public static readonly DependencyProperty OrientationProperty =
        DependencyProperty.Register(nameof(Orientation), typeof(Orientation), typeof(DockSplitter),
            new FrameworkPropertyMetadata(Orientation.Vertical, FrameworkPropertyMetadataOptions.AffectsRender, OnOrientationChanged));

    /// <summary>Direction of the bar itself. Default <see cref="Orientation.Vertical"/>.</summary>
    public Orientation Orientation
    {
        get => (Orientation)GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    public static readonly DependencyProperty ThicknessProperty =
        DependencyProperty.Register(nameof(Thickness), typeof(double), typeof(DockSplitter),
            new FrameworkPropertyMetadata(6.0, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Hit area of the bar, in pixels. Default 6.</summary>
    public double Thickness
    {
        get => (double)GetValue(ThicknessProperty);
        set => SetValue(ThicknessProperty, value);
    }

    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(nameof(Value), typeof(double), typeof(DockSplitter),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValueChanged));

    /// <summary>Current size of the docked area this splitter resizes.</summary>
    public double Value
    {
        get => (double)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public static readonly DependencyProperty MinimumProperty =
        DependencyProperty.Register(nameof(Minimum), typeof(double), typeof(DockSplitter),
            new PropertyMetadata(0.0));

    /// <summary>Lower clamp while dragging.</summary>
    public double Minimum
    {
        get => (double)GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    public static readonly DependencyProperty MaximumProperty =
        DependencyProperty.Register(nameof(Maximum), typeof(double), typeof(DockSplitter),
            new PropertyMetadata(double.PositiveInfinity));

    /// <summary>Upper clamp while dragging.</summary>
    public double Maximum
    {
        get => (double)GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    public static readonly DependencyProperty LineBrushProperty =
        DependencyProperty.Register(nameof(LineBrush), typeof(Brush), typeof(DockSplitter),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Colour of the line. Empty takes the theme border brush.</summary>
    public Brush? LineBrush
    {
        get => (Brush?)GetValue(LineBrushProperty);
        set => SetValue(LineBrushProperty, value);
    }

    /// <summary>Raised after the user finished dragging.</summary>
    public event EventHandler? DragCompleted;

    private bool _isDragging;
    private double _start;
    private Point _origin;

    private static void OnOrientationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DockSplitter splitter)
        {
            splitter.Cursor = splitter.Orientation == Orientation.Vertical ? Cursors.SizeWE : Cursors.SizeNS;
        }
    }

    private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DockSplitter splitter) splitter.InvalidateVisual();
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        double thickness = Math.Max(1, Thickness);

        return Orientation == Orientation.Vertical
            ? new Size(thickness, double.IsInfinity(availableSize.Height) ? 0 : availableSize.Height)
            : new Size(double.IsInfinity(availableSize.Width) ? 0 : availableSize.Width, thickness);
    }

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);

        var brush = LineBrush ?? TryFindResource("CONCOST.Brush.Border") as Brush ?? Brushes.Gray;
        var bounds = new Rect(0, 0, ActualWidth, ActualHeight);
        if (bounds.Width <= 0 || bounds.Height <= 0) return;

        if (_isDragging)
        {
            var accent = TryFindResource("CONCOST.Brush.Primary") as Brush ?? Brushes.DodgerBlue;
            dc.DrawRectangle(accent, null, bounds);
            return;
        }

        var pen = new Pen(brush, 1);
        if (Orientation == Orientation.Vertical)
        {
            double x = Math.Floor(bounds.Width / 2) + 0.5;
            dc.DrawLine(pen, new Point(x, 0), new Point(x, bounds.Height));
        }
        else
        {
            double y = Math.Floor(bounds.Height / 2) + 0.5;
            dc.DrawLine(pen, new Point(0, y), new Point(bounds.Width, y));
        }
    }

    protected override void OnMouseEnter(MouseEventArgs e)
    {
        base.OnMouseEnter(e);
        if (!_isDragging) InvalidateVisual();
    }

    protected override void OnMouseLeave(MouseEventArgs e)
    {
        base.OnMouseLeave(e);
        if (!_isDragging) InvalidateVisual();
    }

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);

        _isDragging = true;
        _start = Value;
        _origin = e.GetPosition(this);
        CaptureMouse();
        InvalidateVisual();
        e.Handled = true;
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        if (!_isDragging || e.LeftButton != MouseButtonState.Pressed) return;

        var position = e.GetPosition(this);
        double delta = Orientation == Orientation.Vertical
            ? position.X - _origin.X
            : position.Y - _origin.Y;

        double next = _start + delta;
        if (next < Minimum) next = Minimum;
        if (next > Maximum) next = Maximum;

        Value = next;
    }

    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonUp(e);
        if (!_isDragging) return;

        _isDragging = false;
        ReleaseMouseCapture();
        InvalidateVisual();
        DragCompleted?.Invoke(this, EventArgs.Empty);
    }
}
