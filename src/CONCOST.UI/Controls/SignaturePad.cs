using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CONCOST.UI.Controls;

/// <summary>
/// A place to sign: the user draws with the mouse or a pen, the strokes are kept as a
/// <see cref="StrokeCollection"/>, and the pad can hand the result back as a bitmap.
/// </summary>
/// <remarks>
/// <para>
/// Built for approving engineering records — a site instruction, a quantity sheet, an as-built drawing
/// — so the two things that matter are that the ink is real vector data (it exports at any resolution
/// and can be saved as Ink Serialized Format) and that the pad tells the host when it is no longer
/// empty, which is what enables the "Approve" button.
/// </para>
/// <para>
/// The strokes are drawn in <see cref="OnRender"/> rather than with an <see cref="InkCanvas"/>: the
/// pad owns its look (guideline, placeholder, rounded border) and does not inherit the default ink
/// canvas chrome, which cannot be themed without replacing its template anyway.
/// </para>
/// </remarks>
public class SignaturePad : FrameworkElement
{
    private readonly List<Point> _pending = new();
    private StylusPointCollection? _pendingPoints;
    private bool _isDrawing;

    public SignaturePad()
    {
        Focusable = true;
        Cursor = Cursors.Pen;
        Strokes = new StrokeCollection();
        Strokes.StrokesChanged += OnStrokesChanged;
    }

    /// <summary>The ink drawn so far. Add or remove a stroke to drive the pad from code.</summary>
    public StrokeCollection Strokes { get; }

    public static readonly DependencyProperty StrokeBrushProperty =
        DependencyProperty.Register(nameof(StrokeBrush), typeof(Brush), typeof(SignaturePad),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, OnInkChanged));

    /// <summary>Ink colour. Empty takes <c>CONCOST.Brush.TextPrimary</c>, so a signature is legible in both themes.</summary>
    public Brush? StrokeBrush
    {
        get => (Brush?)GetValue(StrokeBrushProperty);
        set => SetValue(StrokeBrushProperty, value);
    }

    public static readonly DependencyProperty StrokeThicknessProperty =
        DependencyProperty.Register(nameof(StrokeThickness), typeof(double), typeof(SignaturePad),
            new FrameworkPropertyMetadata(2.2, FrameworkPropertyMetadataOptions.AffectsRender, OnInkChanged));

    /// <summary>Ink width in pixels. Default 2.2 — thin enough to look like a pen, thick enough at 125% DPI.</summary>
    public double StrokeThickness
    {
        get => (double)GetValue(StrokeThicknessProperty);
        set => SetValue(StrokeThicknessProperty, value);
    }

    public static readonly DependencyProperty FillProperty =
        DependencyProperty.Register(nameof(Fill), typeof(Brush), typeof(SignaturePad),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Background of the pad. Empty takes <c>CONCOST.Brush.Surface</c>.</summary>
    public Brush? Fill
    {
        get => (Brush?)GetValue(FillProperty);
        set => SetValue(FillProperty, value);
    }

    public static readonly DependencyProperty EdgeBrushProperty =
        DependencyProperty.Register(nameof(EdgeBrush), typeof(Brush), typeof(SignaturePad),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Border colour. Empty takes <c>CONCOST.Brush.Border</c>.</summary>
    public Brush? EdgeBrush
    {
        get => (Brush?)GetValue(EdgeBrushProperty);
        set => SetValue(EdgeBrushProperty, value);
    }

    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(SignaturePad),
            new FrameworkPropertyMetadata(new CornerRadius(10), FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Corner radius of the pad. Default 10.</summary>
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    public static readonly DependencyProperty ShowGuidelineProperty =
        DependencyProperty.Register(nameof(ShowGuideline), typeof(bool), typeof(SignaturePad),
            new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Draws the "sign on the line" rule and its two crosses. Default <c>true</c>.</summary>
    public bool ShowGuideline
    {
        get => (bool)GetValue(ShowGuidelineProperty);
        set => SetValue(ShowGuidelineProperty, value);
    }

    public static readonly DependencyProperty PlaceholderProperty =
        DependencyProperty.Register(nameof(Placeholder), typeof(string), typeof(SignaturePad),
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Hint shown while the pad is empty, for example "Ký và ghi rõ họ tên".</summary>
    public string Placeholder
    {
        get => (string)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    public static readonly DependencyProperty PlaceholderBrushProperty =
        DependencyProperty.Register(nameof(PlaceholderBrush), typeof(Brush), typeof(SignaturePad),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Colour of <see cref="Placeholder"/>. Empty takes <c>CONCOST.Brush.TextTertiary</c>.</summary>
    public Brush? PlaceholderBrush
    {
        get => (Brush?)GetValue(PlaceholderBrushProperty);
        set => SetValue(PlaceholderBrushProperty, value);
    }

    private static readonly DependencyPropertyKey HasSignatureKey =
        DependencyProperty.RegisterReadOnly(nameof(HasSignature), typeof(bool), typeof(SignaturePad),
            new FrameworkPropertyMetadata(false));

    public static readonly DependencyProperty HasSignatureProperty = HasSignatureKey.DependencyProperty;

    /// <summary>
    /// <c>true</c> once there is at least one stroke. Read-only, and the property that an "Approve"
    /// button's <c>IsEnabled</c> wants to bind to.
    /// </summary>
    public bool HasSignature => (bool)GetValue(HasSignatureProperty);

    public static readonly DependencyProperty SignatureImageProperty =
        DependencyProperty.Register(nameof(SignatureImage), typeof(BitmapSource), typeof(SignaturePad),
            new FrameworkPropertyMetadata(null));

    /// <summary>
    /// The current signature as a bitmap, refreshed after every completed stroke. Bind it
    /// <c>Mode=OneWayToSource</c> (or read <see cref="Export"/> for a specific pixel size): the pad
    /// writes it, the host never sets it.
    /// </summary>
    public BitmapSource? SignatureImage
    {
        get => (BitmapSource?)GetValue(SignatureImageProperty);
        set => SetValue(SignatureImageProperty, value);
    }

    /// <summary>Raised whenever the ink changes — a stroke added, undone or cleared.</summary>
    public event EventHandler? SignatureChanged;

    /// <summary>Removes the last stroke. Returns <c>false</c> when there was nothing to remove.</summary>
    public bool Undo()
    {
        if (Strokes.Count == 0) return false;

        Strokes.RemoveAt(Strokes.Count - 1);
        return true;
    }

    /// <summary>Removes every stroke and clears <see cref="SignatureImage"/>.</summary>
    public void Clear()
    {
        if (Strokes.Count > 0) Strokes.Clear();
    }

    /// <summary>
    /// Renders the signature to a bitmap. The strokes are vector data, so the export can be larger
    /// than the pad on screen without turning into pixels.
    /// </summary>
    /// <param name="pixelWidth">Target width; 0 uses the pad's own width, or 600 when it has not been laid out.</param>
    /// <param name="pixelHeight">Target height; 0 uses the pad's own height, or 220 when it has not been laid out.</param>
    /// <param name="background">
    /// Paper colour. Pass <c>null</c> for transparent — which is what a signature stamped onto a PDF wants.
    /// </param>
    public BitmapSource Export(double pixelWidth = 0, double pixelHeight = 0, Brush? background = null)
    {
        double targetWidth = pixelWidth > 0 ? pixelWidth : ActualWidth > 0 ? ActualWidth : 600;
        double targetHeight = pixelHeight > 0 ? pixelHeight : ActualHeight > 0 ? ActualHeight : 220;

        int width = Math.Max(1, (int)Math.Round(targetWidth));
        int height = Math.Max(1, (int)Math.Round(targetHeight));

        var visual = new DrawingVisual();
        using (DrawingContext dc = visual.RenderOpen())
        {
            if (background is not null) dc.DrawRectangle(background, null, new Rect(0, 0, width, height));

            // A pad that has not been laid out has nothing to scale from; draw the ink as-is.
            double scaleX = ActualWidth > 0 ? width / ActualWidth : 1;
            double scaleY = ActualHeight > 0 ? height / ActualHeight : 1;

            if (Math.Abs(scaleX - 1) > 0.0001 || Math.Abs(scaleY - 1) > 0.0001)
            {
                dc.PushTransform(new ScaleTransform(scaleX, scaleY));
            }

            foreach (Stroke stroke in Strokes) stroke.Draw(dc);

            if (Math.Abs(scaleX - 1) > 0.0001 || Math.Abs(scaleY - 1) > 0.0001) dc.Pop();
        }

        var bitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(visual);
        bitmap.Freeze();
        return bitmap;
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        // A pad with no size of its own still needs a usable box, otherwise it collapses to nothing in
        // a StackPanel and the user has nowhere to draw.
        double width = double.IsInfinity(availableSize.Width) ? 320 : availableSize.Width;
        double height = double.IsInfinity(availableSize.Height) ? 160 : availableSize.Height;
        return new Size(width, height);
    }

    protected override void OnRender(DrawingContext dc)
    {
        double width = ActualWidth;
        double height = ActualHeight;
        if (width <= 0 || height <= 0) return;

        Brush fill = Fill ?? TryFindResource("CONCOST.Brush.Surface") as Brush ?? Brushes.White;
        Pen edge = new(EdgeBrush ?? TryFindResource("CONCOST.Brush.Border") as Brush ?? Brushes.LightGray, 1);

        var frame = new Rect(0.5, 0.5, Math.Max(0, width - 1), Math.Max(0, height - 1));
        dc.DrawRoundedRectangle(fill, edge, frame, CornerRadius.TopLeft, CornerRadius.TopLeft);

        if (ShowGuideline) DrawGuideline(dc, width, height);

        if (Strokes.Count == 0)
        {
            DrawPlaceholder(dc, width, VisualTreeHelper.GetDpi(this).PixelsPerDip);
            return;
        }

        // Clip while drawing so a stroke that runs past the edge does not paint over the frame.
        dc.PushClip(new RectangleGeometry(frame, CornerRadius.TopLeft, CornerRadius.TopLeft));

        foreach (Stroke stroke in Strokes) stroke.Draw(dc);
        DrawPendingStroke(dc);

        dc.Pop();
    }

    private void DrawGuideline(DrawingContext dc, double width, double height)
    {
        Brush rule = PlaceholderBrush ?? TryFindResource("CONCOST.Brush.TextTertiary") as Brush ?? Brushes.Silver;
        double y = Math.Round(height * 0.68) + 0.5;

        var pen = new Pen(rule, 1) { DashStyle = new DashStyle(new double[] { 4, 4 }, 0) };
        dc.DrawLine(pen, new Point(18, y), new Point(Math.Max(19, width - 18), y));

        // The classic "x" at each end of the line, so it reads as a place to sign even with no text.
        var cross = new Pen(rule, 1.2);
        foreach (double x in new[] { 14.0, width - 14 })
        {
            dc.DrawLine(cross, new Point(x - 3, y - 7), new Point(x + 3, y - 1));
            dc.DrawLine(cross, new Point(x + 3, y - 7), new Point(x - 3, y - 1));
        }
    }

    private void DrawPlaceholder(DrawingContext dc, double width, double dpi)
    {
        if (string.IsNullOrEmpty(Placeholder)) return;

        Brush brush = PlaceholderBrush ?? TryFindResource("CONCOST.Brush.TextTertiary") as Brush ?? Brushes.Gray;

        // 13 px: the size a hint should be — visible, but never mistaken for something that has been signed.
        var text = new FormattedText(Placeholder, System.Globalization.CultureInfo.CurrentUICulture,
            FlowDirection.LeftToRight, new Typeface("Segoe UI"), 13, brush, dpi)
        {
            MaxTextWidth = Math.Max(10, width - 36),
            TextAlignment = TextAlignment.Left
        };

        dc.DrawText(text, new Point(18, Math.Max(12, ActualHeight * 0.68 - text.Height - 10)));
    }

    private void DrawPendingStroke(DrawingContext dc)
    {
        if (_pending.Count < 2) return;

        var geometry = new StreamGeometry();
        using (StreamGeometryContext ctx = geometry.Open())
        {
            ctx.BeginFigure(_pending[0], false, false);
            ctx.PolyLineTo(_pending.GetRange(1, _pending.Count - 1), true, true);
        }

        geometry.Freeze();

        var brush = StrokeBrush ?? TryFindResource("CONCOST.Brush.TextPrimary") as Brush ?? Brushes.Black;
        var pen = new Pen(brush, Math.Max(0.5, StrokeThickness))
        {
            StartLineCap = PenLineCap.Round,
            EndLineCap = PenLineCap.Round,
            LineJoin = PenLineJoin.Round
        };

        dc.DrawGeometry(null, pen, geometry);
    }

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);

        Focus();
        CaptureMouse();
        _isDrawing = true;
        _pending.Clear();
        _pendingPoints = new StylusPointCollection();

        AddPoint(e.GetPosition(this));
        e.Handled = true;
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);

        if (!_isDrawing || e.LeftButton != MouseButtonState.Pressed) return;

        AddPoint(e.GetPosition(this));
    }

    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonUp(e);

        if (!_isDrawing) return;

        _isDrawing = false;
        AddPoint(e.GetPosition(this));
        ReleaseMouseCapture();

        if (_pendingPoints is { Count: > 1 })
        {
            var attributes = new DrawingAttributes
            {
                Color = (StrokeBrush ?? TryFindResource("CONCOST.Brush.TextPrimary") as Brush ?? Brushes.Black) is SolidColorBrush solid
                    ? solid.Color
                    : Colors.Black,
                Width = Math.Max(0.5, StrokeThickness),
                Height = Math.Max(0.5, StrokeThickness),
                FitToCurve = true,
                StylusTip = StylusTip.Ellipse
            };

            Strokes.Add(new Stroke(_pendingPoints, attributes));
        }

        _pending.Clear();
        _pendingPoints = null;
        InvalidateVisual();

        e.Handled = true;
    }

    protected override void OnLostMouseCapture(MouseEventArgs e)
    {
        base.OnLostMouseCapture(e);

        // A capture lost mid-stroke (Alt+Tab, a drag onto another window) must not leave the pad in a
        // state where the next mouse move keeps appending to a stroke nobody released.
        if (!_isDrawing) return;

        _isDrawing = false;
        _pending.Clear();
        _pendingPoints = null;
        InvalidateVisual();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (e.Key == Key.Z && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
        {
            if (Undo()) e.Handled = true;
        }
        else if (e.Key == Key.Delete && Strokes.Count > 0)
        {
            Clear();
            e.Handled = true;
        }
    }

    private void AddPoint(Point point)
    {
        if (_pending.Count > 0)
        {
            Point last = _pending[_pending.Count - 1];

            // Sub-pixel moves are noise: they bloat the stroke without changing what is drawn.
            if (Math.Abs(last.X - point.X) < 0.4 && Math.Abs(last.Y - point.Y) < 0.4) return;
        }

        _pending.Add(point);
        _pendingPoints?.Add(new StylusPoint(point.X, point.Y));
        InvalidateVisual();
    }

    private void OnStrokesChanged(object? sender, StrokeCollectionChangedEventArgs e)
    {
        if (e.Added.Count > 0 || e.Removed.Count > 0)
        {
            SetValue(HasSignatureKey, Strokes.Count > 0);
            SignatureChanged?.Invoke(this, EventArgs.Empty);
            InvalidateVisual();

            // Only a completed stroke updates the bitmap: doing it per mouse move would render the
            // whole pad on every pixel of movement.
            SetCurrentValue(SignatureImageProperty, Strokes.Count > 0 ? Export() : null);
        }
    }

    private static void OnInkChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not SignaturePad pad) return;

        // Keep the strokes already drawn in the new colour; otherwise a theme switch leaves a black
        // signature on a dark pad.
        if (e.Property == StrokeBrushProperty && pad.Strokes.Count > 0)
        {
            Color color = (pad.StrokeBrush as SolidColorBrush)?.Color ?? Colors.Black;

            foreach (Stroke stroke in pad.Strokes)
            {
                DrawingAttributes attributes = stroke.DrawingAttributes.Clone();
                attributes.Color = color;
                stroke.DrawingAttributes = attributes;
            }

            pad.SetCurrentValue(SignatureImageProperty, pad.Export());
        }
        else if (e.Property == StrokeThicknessProperty && pad.Strokes.Count > 0)
        {
            double size = Math.Max(0.5, pad.StrokeThickness);

            foreach (Stroke stroke in pad.Strokes)
            {
                DrawingAttributes attributes = stroke.DrawingAttributes.Clone();
                attributes.Width = size;
                attributes.Height = size;
                stroke.DrawingAttributes = attributes;
            }

            pad.SetCurrentValue(SignatureImageProperty, pad.Export());
        }

        pad.InvalidateVisual();
    }
}
