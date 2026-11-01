using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace CONCOST.UI.Controls;

/// <summary>
/// A zoomable, pannable, rotatable picture — the sheet you want to look at when a plan is on screen.
/// </summary>
/// <remarks>
/// <para>
/// The image is drawn with a transform rather than put inside a <c>ScrollViewer</c>, so zooming and
/// panning cost one render and no layout pass, and a 6000 px drawing stays smooth.
/// </para>
/// <para>
/// Wheel zoom keeps the point under the pointer still: the offset is rescaled about the cursor before
/// <see cref="Zoom"/> changes. With a rotation applied the point is only approximately preserved —
/// the image is rotated about its centre, so the cursor is a good anchor rather than an exact one.
/// </para>
/// </remarks>
public class ImageViewer : FrameworkElement
{
    /// <summary>Identifies the <see cref="Source"/> property.</summary>
    public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(
        nameof(Source), typeof(ImageSource), typeof(ImageViewer),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Identifies the <see cref="Zoom"/> property.</summary>
    public static readonly DependencyProperty ZoomProperty = DependencyProperty.Register(
        nameof(Zoom), typeof(double), typeof(ImageViewer),
        new FrameworkPropertyMetadata(1d, FrameworkPropertyMetadataOptions.AffectsRender, null, CoerceZoom));

    /// <summary>Identifies the <see cref="Rotation"/> property.</summary>
    public static readonly DependencyProperty RotationProperty = DependencyProperty.Register(
        nameof(Rotation), typeof(double), typeof(ImageViewer),
        new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Identifies the <see cref="Offset"/> property.</summary>
    public static readonly DependencyProperty OffsetProperty = DependencyProperty.Register(
        nameof(Offset), typeof(Point), typeof(ImageViewer),
        new FrameworkPropertyMetadata(new Point(0, 0), FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Identifies the <see cref="MinZoom"/> property.</summary>
    public static readonly DependencyProperty MinZoomProperty = DependencyProperty.Register(
        nameof(MinZoom), typeof(double), typeof(ImageViewer), new PropertyMetadata(0.05d));

    /// <summary>Identifies the <see cref="MaxZoom"/> property.</summary>
    public static readonly DependencyProperty MaxZoomProperty = DependencyProperty.Register(
        nameof(MaxZoom), typeof(double), typeof(ImageViewer), new PropertyMetadata(16d));

    /// <summary>Identifies the <see cref="ShowCheckerboard"/> property.</summary>
    public static readonly DependencyProperty ShowCheckerboardProperty = DependencyProperty.Register(
        nameof(ShowCheckerboard), typeof(bool), typeof(ImageViewer),
        new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender));

    private Point _panOrigin;
    private Point _panStart;
    private bool _panning;

    private static readonly Brush CheckerBrush = CreateCheckerBrush();

    public ImageViewer()
    {
        ClipToBounds = true;
        Focusable = true;
        Cursor = Cursors.Hand;
        RenderOptions.SetBitmapScalingMode(this, BitmapScalingMode.HighQuality);
    }

    private static Brush CreateCheckerBrush()
    {
        var brush = new SolidColorBrush(Color.FromRgb(0xEE, 0xEE, 0xEE));
        brush.Freeze();
        return brush;
    }

    /// <summary>The picture to show.</summary>
    public ImageSource? Source
    {
        get => (ImageSource?)GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    /// <summary>Scale factor; clamped to <see cref="MinZoom"/>..<see cref="MaxZoom"/>.</summary>
    public double Zoom
    {
        get => (double)GetValue(ZoomProperty);
        set => SetValue(ZoomProperty, value);
    }

    /// <summary>Clockwise rotation in degrees.</summary>
    public double Rotation
    {
        get => (double)GetValue(RotationProperty);
        set => SetValue(RotationProperty, value);
    }

    /// <summary>Top-left corner of the picture, in control coordinates.</summary>
    public Point Offset
    {
        get => (Point)GetValue(OffsetProperty);
        set => SetValue(OffsetProperty, value);
    }

    /// <summary>Smallest allowed zoom.</summary>
    public double MinZoom
    {
        get => (double)GetValue(MinZoomProperty);
        set => SetValue(MinZoomProperty, value);
    }

    /// <summary>Largest allowed zoom.</summary>
    public double MaxZoom
    {
        get => (double)GetValue(MaxZoomProperty);
        set => SetValue(MaxZoomProperty, value);
    }

    /// <summary>Draws a grey checker behind the picture, so transparent drawings read as transparent.</summary>
    public bool ShowCheckerboard
    {
        get => (bool)GetValue(ShowCheckerboardProperty);
        set => SetValue(ShowCheckerboardProperty, value);
    }

    /// <summary>True when a picture is loaded and it has a usable size.</summary>
    public bool HasImage => Source is { Width: > 0, Height: > 0 };

    private static object CoerceZoom(DependencyObject d, object baseValue)
    {
        if (d is not ImageViewer viewer) return baseValue;

        double value = (double)baseValue;
        if (double.IsNaN(value) || double.IsInfinity(value)) return viewer.Zoom;

        return Math.Max(viewer.MinZoom, Math.Min(viewer.MaxZoom, value));
    }

    /// <summary>The rectangle the picture covers, including rotation and pan.</summary>
    public Rect GetImageBounds()
    {
        if (Source is not { Width: > 0, Height: > 0 } source) return Rect.Empty;

        double width = source.Width * Zoom;
        double height = source.Height * Zoom;

        var rect = new Rect(Offset.X, Offset.Y, width, height);
        if (Math.Abs(Rotation % 360) < 0.01) return rect;

        var rotated = new RotateTransform(Rotation, Offset.X + width / 2, Offset.Y + height / 2);
        return rotated.TransformBounds(rect);
    }

    /// <summary>Zooms so the whole picture fits, and centres it.</summary>
    public void Fit()
    {
        if (Source is not { Width: > 0, Height: > 0 } source) return;

        double availableWidth = ActualWidth > 0 ? ActualWidth : source.Width;
        double availableHeight = ActualHeight > 0 ? ActualHeight : source.Height;

        double zoom = Math.Min(availableWidth / source.Width, availableHeight / source.Height);

        SetCurrentValue(ZoomProperty, Math.Max(MinZoom, Math.Min(MaxZoom, zoom)));
        Centre();
    }

    /// <summary>Puts the picture back at 100 %, unrotated and centred.</summary>
    public void Reset()
    {
        SetCurrentValue(RotationProperty, 0d);
        SetCurrentValue(ZoomProperty, 1d);
        Centre();
    }

    /// <summary>Rotates a quarter turn clockwise; normalised to 0..359.</summary>
    public void RotateClockwise()
    {
        double next = (Rotation + 90) % 360;
        SetCurrentValue(RotationProperty, next < 0 ? next + 360 : next);
        Centre();
    }

    /// <summary>
    /// Zooms by <paramref name="factor"/> while keeping <paramref name="anchor"/> (control
    /// coordinates) over the same part of the picture.
    /// </summary>
    public void ZoomAt(Point anchor, double factor)
    {
        if (factor <= 0 || double.IsNaN(factor) || double.IsInfinity(factor)) return;

        double current = Zoom;
        double next = Math.Max(MinZoom, Math.Min(MaxZoom, current * factor));
        if (Math.Abs(next - current) < 0.0001) return;

        double applied = next / current;

        Offset = new Point(
            anchor.X - (anchor.X - Offset.X) * applied,
            anchor.Y - (anchor.Y - Offset.Y) * applied);

        SetCurrentValue(ZoomProperty, next);
    }

    /// <summary>Centres the picture in the control.</summary>
    public void Centre()
    {
        if (Source is not { Width: > 0, Height: > 0 } source) return;

        double width = source.Width * Zoom;
        double height = source.Height * Zoom;

        Offset = new Point((ActualWidth - width) / 2, (ActualHeight - height) / 2);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        double width = Source?.Width ?? 0;
        double height = Source?.Height ?? 0;

        if (width <= 0 || height <= 0) return new Size(240, 180);

        return new Size(
            double.IsInfinity(availableSize.Width) ? width : Math.Min(width, availableSize.Width),
            double.IsInfinity(availableSize.Height) ? height : Math.Min(height, availableSize.Height));
    }

    protected override void OnRender(DrawingContext dc)
    {
        var background = TryFindResource("CONCOST.Brush.SurfaceSecondary") as Brush ?? Brushes.WhiteSmoke;
        dc.DrawRectangle(background, null, new Rect(RenderSize));

        if (Source is not { Width: > 0, Height: > 0 } source) return;

        if (ShowCheckerboard) DrawCheckerboard(dc);

        double width = source.Width * Zoom;
        double height = source.Height * Zoom;
        var rect = new Rect(Offset.X, Offset.Y, width, height);

        dc.PushTransform(new RotateTransform(Rotation, Offset.X + width / 2, Offset.Y + height / 2));
        dc.DrawImage(source, rect);
        dc.Pop();

        var border = TryFindResource("CONCOST.Brush.Border") as Brush ?? Brushes.Gainsboro;
        dc.DrawRectangle(null, new Pen(border, 1), rect);
    }

    private void DrawCheckerboard(DrawingContext dc)
    {
        const double cell = 12;

        dc.PushClip(new RectangleGeometry(new Rect(RenderSize)));

        for (double y = 0; y < ActualHeight; y += cell)
        {
            for (double x = 0; x < ActualWidth; x += cell)
            {
                bool odd = ((int)(x / cell) + (int)(y / cell)) % 2 == 1;
                dc.DrawRectangle(odd ? CheckerBrush : Brushes.White, null, new Rect(x, y, cell, cell));
            }
        }

        dc.Pop();
    }

    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
        base.OnMouseWheel(e);

        double factor = e.Delta > 0 ? 1.15 : 1 / 1.15;

        if ((Keyboard.Modifiers & ModifierKeys.Shift) != 0)
        {
            // Shift + wheel rotates in 15° steps, which is how a drawing is usually straightened.
            double next = Rotation + Math.Sign(e.Delta) * 15;
            SetCurrentValue(RotationProperty, next % 360);
        }
        else
        {
            ZoomAt(e.GetPosition(this), factor);
        }

        e.Handled = true;
    }

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);

        if (e.ClickCount > 1)
        {
            Fit();
            e.Handled = true;
            return;
        }

        Focus();
        _panning = true;
        _panOrigin = e.GetPosition(this);
        _panStart = Offset;
        CaptureMouse();
        Cursor = Cursors.SizeAll;
        e.Handled = true;
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);

        if (!_panning || e.LeftButton != MouseButtonState.Pressed) return;

        var position = e.GetPosition(this);
        Offset = new Point(
            _panStart.X + position.X - _panOrigin.X,
            _panStart.Y + position.Y - _panOrigin.Y);
    }

    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonUp(e);

        if (!_panning) return;

        _panning = false;
        ReleaseMouseCapture();
        Cursor = Cursors.Hand;
    }
}
