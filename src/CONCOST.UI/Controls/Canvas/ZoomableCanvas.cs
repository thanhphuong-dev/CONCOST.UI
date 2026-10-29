using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System;

namespace CONCOST.UI.Controls.Canvas;

public class ZoomableCanvas : System.Windows.Controls.Canvas
{
    private Point _lastPanPoint;
    private bool _isPanning;
    private readonly ScaleTransform _scaleTransform = new(1.0, 1.0);
    private readonly TranslateTransform _translateTransform = new(0.0, 0.0);
    private readonly TransformGroup _transformGroup = new();

    static ZoomableCanvas()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(ZoomableCanvas), new FrameworkPropertyMetadata(typeof(ZoomableCanvas)));
    }

    public static readonly DependencyProperty ZoomProperty =
        DependencyProperty.Register(nameof(Zoom), typeof(double), typeof(ZoomableCanvas),
            new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnZoomChanged, CoerceZoom));

    public static readonly DependencyProperty MinZoomProperty =
        DependencyProperty.Register(nameof(MinZoom), typeof(double), typeof(ZoomableCanvas),
            new PropertyMetadata(0.1, OnZoomBoundsChanged));

    public static readonly DependencyProperty MaxZoomProperty =
        DependencyProperty.Register(nameof(MaxZoom), typeof(double), typeof(ZoomableCanvas),
            new PropertyMetadata(15.0, OnZoomBoundsChanged));

    public static readonly DependencyProperty ShowGridProperty =
        DependencyProperty.Register(nameof(ShowGrid), typeof(bool), typeof(ZoomableCanvas),
            new PropertyMetadata(true, OnGridChanged));

    public static readonly DependencyProperty GridSizeProperty =
        DependencyProperty.Register(nameof(GridSize), typeof(double), typeof(ZoomableCanvas),
            new PropertyMetadata(20.0, OnGridChanged, CoerceGridSize));

    public static readonly DependencyProperty PanOffsetXProperty =
        DependencyProperty.Register(nameof(PanOffsetX), typeof(double), typeof(ZoomableCanvas),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnPanChanged));

    public static readonly DependencyProperty PanOffsetYProperty =
        DependencyProperty.Register(nameof(PanOffsetY), typeof(double), typeof(ZoomableCanvas),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnPanChanged));

    public event EventHandler? ViewportChanged;

    public double Zoom
    {
        get => (double)GetValue(ZoomProperty);
        set => SetValue(ZoomProperty, value);
    }

    public double PanOffsetX
    {
        get => (double)GetValue(PanOffsetXProperty);
        set => SetValue(PanOffsetXProperty, value);
    }

    public double PanOffsetY
    {
        get => (double)GetValue(PanOffsetYProperty);
        set => SetValue(PanOffsetYProperty, value);
    }

    public double MinZoom
    {
        get => (double)GetValue(MinZoomProperty);
        set => SetValue(MinZoomProperty, value);
    }

    public double MaxZoom
    {
        get => (double)GetValue(MaxZoomProperty);
        set => SetValue(MaxZoomProperty, value);
    }

    public bool ShowGrid
    {
        get => (bool)GetValue(ShowGridProperty);
        set => SetValue(ShowGridProperty, value);
    }

    public double GridSize
    {
        get => (double)GetValue(GridSizeProperty);
        set => SetValue(GridSizeProperty, value);
    }

    public ZoomableCanvas()
    {
        _transformGroup.Children.Add(_scaleTransform);
        _transformGroup.Children.Add(_translateTransform);
        RenderTransform = _transformGroup;
        ClipToBounds = true;
        Focusable = true;
    }

    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
        if (e.Delta == 0) return;
        base.OnMouseWheel(e);

        double zoomFactor = e.Delta > 0 ? 1.15 : 0.85;
        double newZoom = NumericCompatibility.Clamp(Zoom * zoomFactor, MinZoom, MaxZoom);

        if (Math.Abs(newZoom - Zoom) > 0.001)
        {
            Point mousePos = e.GetPosition(this);
            double absoluteX = (mousePos.X - _translateTransform.X) / _scaleTransform.ScaleX;
            double absoluteY = (mousePos.Y - _translateTransform.Y) / _scaleTransform.ScaleY;

            Zoom = newZoom;
            _scaleTransform.ScaleX = Zoom;
            _scaleTransform.ScaleY = Zoom;

            _translateTransform.X = mousePos.X - absoluteX * Zoom;
            _translateTransform.Y = mousePos.Y - absoluteY * Zoom;

            PanOffsetX = _translateTransform.X;
            PanOffsetY = _translateTransform.Y;
            ViewportChanged?.Invoke(this, EventArgs.Empty);
        }

        e.Handled = true;
    }

    protected override void OnMouseDown(MouseButtonEventArgs e)
    {
        base.OnMouseDown(e);

        if (e.MiddleButton == MouseButtonState.Pressed ||
            (e.LeftButton == MouseButtonState.Pressed && Keyboard.IsKeyDown(Key.Space)))
        {
            _isPanning = true;
            _lastPanPoint = e.GetPosition(this);
            CaptureMouse();
            Cursor = Cursors.SizeAll;
            e.Handled = true;
        }
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);

        if (_isPanning)
        {
            Point currentPoint = e.GetPosition(this);
            Vector delta = currentPoint - _lastPanPoint;

            _translateTransform.X += delta.X;
            _translateTransform.Y += delta.Y;

            PanOffsetX = _translateTransform.X;
            PanOffsetY = _translateTransform.Y;
            ViewportChanged?.Invoke(this, EventArgs.Empty);

            _lastPanPoint = currentPoint;
            e.Handled = true;
        }
    }

    protected override void OnMouseUp(MouseButtonEventArgs e)
    {
        base.OnMouseUp(e);

        if (_isPanning)
        {
            _isPanning = false;
            ReleaseMouseCapture();
            Cursor = Cursors.Arrow;
            e.Handled = true;
        }
    }

    public void ZoomToFit(Rect bounds, Size viewSize)
    {
        if (bounds.Width <= 0 || bounds.Height <= 0 || viewSize.Width <= 0 || viewSize.Height <= 0)
            return;

        double scaleX = viewSize.Width / (bounds.Width * 1.2);
        double scaleY = viewSize.Height / (bounds.Height * 1.2);
        double fitZoom = NumericCompatibility.Clamp(Math.Min(scaleX, scaleY), MinZoom, MaxZoom);

        Zoom = fitZoom;
        _scaleTransform.ScaleX = Zoom;
        _scaleTransform.ScaleY = Zoom;

        _translateTransform.X = (viewSize.Width - bounds.Width * Zoom) / 2.0 - bounds.X * Zoom;
        _translateTransform.Y = (viewSize.Height - bounds.Height * Zoom) / 2.0 - bounds.Y * Zoom;

        PanOffsetX = _translateTransform.X;
        PanOffsetY = _translateTransform.Y;
        ViewportChanged?.Invoke(this, EventArgs.Empty);
    }

    public void ResetView()
    {
        Zoom = 1.0;
        _scaleTransform.ScaleX = 1.0;
        _scaleTransform.ScaleY = 1.0;
        _translateTransform.X = 0;
        _translateTransform.Y = 0;
        PanOffsetX = 0;
        PanOffsetY = 0;
        ViewportChanged?.Invoke(this, EventArgs.Empty);
    }

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);

        if (Background != null)
        {
            dc.DrawRectangle(Background, null, new Rect(0, 0, ActualWidth, ActualHeight));
        }

        if (ShowGrid && GridSize > 2)
        {
            var pen = new Pen(new SolidColorBrush(Color.FromArgb(40, 150, 150, 150)), 1);
            pen.Freeze();

            double step = GridSize;
            for (double x = 0; x < ActualWidth; x += step)
            {
                dc.DrawLine(pen, new Point(x, 0), new Point(x, ActualHeight));
            }
            for (double y = 0; y < ActualHeight; y += step)
            {
                dc.DrawLine(pen, new Point(0, y), new Point(ActualWidth, y));
            }
        }
    }

    private static void OnZoomChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ZoomableCanvas canvas)
        {
            double z = (double)e.NewValue;
            canvas._scaleTransform.ScaleX = z;
            canvas._scaleTransform.ScaleY = z;
            canvas.ViewportChanged?.Invoke(canvas, EventArgs.Empty);
        }
    }

    private static object CoerceZoom(DependencyObject d, object value)
    {
        var canvas = (ZoomableCanvas)d;
        var zoom = (double)value;
        var minimum = NumericCompatibility.IsFinite(canvas.MinZoom) && canvas.MinZoom > 0 ? canvas.MinZoom : 0.1d;
        var maximum = NumericCompatibility.IsFinite(canvas.MaxZoom) ? Math.Max(minimum, canvas.MaxZoom) : Math.Max(15d, minimum);
        return NumericCompatibility.IsFinite(zoom) ? NumericCompatibility.Clamp(zoom, minimum, maximum) : NumericCompatibility.Clamp(1d, minimum, maximum);
    }

    private static void OnZoomBoundsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ZoomableCanvas canvas)
        {
            CoerceZoomBounds(canvas);
            if (e.Property == MinZoomProperty && canvas.MaxZoom < canvas.MinZoom)
                canvas.SetCurrentValue(MaxZoomProperty, canvas.MinZoom);
            canvas.CoerceValue(ZoomProperty);
        }
    }

    private static void CoerceZoomBounds(ZoomableCanvas canvas)
    {
        if (!NumericCompatibility.IsFinite(canvas.MinZoom) || canvas.MinZoom <= 0) canvas.SetCurrentValue(MinZoomProperty, 0.1d);
        if (!NumericCompatibility.IsFinite(canvas.MaxZoom)) canvas.SetCurrentValue(MaxZoomProperty, Math.Max(15d, canvas.MinZoom));
        else if (canvas.MaxZoom < canvas.MinZoom) canvas.SetCurrentValue(MaxZoomProperty, canvas.MinZoom);
    }

    private static object CoerceGridSize(DependencyObject d, object value)
    {
        var size = (double)value;
        return NumericCompatibility.IsFinite(size) ? Math.Max(0, size) : 20d;
    }

    private static void OnPanChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ZoomableCanvas canvas)
        {
            canvas._translateTransform.X = canvas.PanOffsetX;
            canvas._translateTransform.Y = canvas.PanOffsetY;
            canvas.ViewportChanged?.Invoke(canvas, EventArgs.Empty);
        }
    }

    private static void OnGridChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ZoomableCanvas canvas)
        {
            canvas.InvalidateVisual();
        }
    }
}
