using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace CONCOST.UI.Controls;

/// <summary>One block drawn on a <see cref="MiniMap"/>: a wall, a sheet border, a zone.</summary>
public class MiniMapItem
{
    /// <summary>Position and size in the map's world coordinates.</summary>
    public Rect Bounds { get; set; }

    /// <summary>Colour; falls back to <see cref="MiniMap.ItemBrush"/>.</summary>
    public Brush? Brush { get; set; }
}

/// <summary>Reports a viewport the user dragged on the <see cref="MiniMap"/>.</summary>
public class MiniMapViewportChangedEventArgs : EventArgs
{
    public MiniMapViewportChangedEventArgs(Rect viewport, bool isFinal)
    {
        Viewport = viewport;
        IsFinal = isFinal;
    }

    /// <summary>The new viewport, in world coordinates.</summary>
    public Rect Viewport { get; }

    /// <summary><c>true</c> on the last event of a drag, so a host can defer the heavy redraw.</summary>
    public bool IsFinal { get; }
}

/// <summary>
/// The whole drawing at a glance, with a rectangle you can drag to move the view — the corner map
/// every CAD viewport has.
/// </summary>
/// <remarks>
/// <para>
/// The map is driven by world-space rectangles rather than by a live visual, so it costs nothing to
/// keep updated: the host pushes the sheet extents, the item list and the current viewport, and the
/// control only does arithmetic. That also makes it usable over a DirectX viewport, where there is no
/// WPF visual tree to shrink.
/// </para>
/// <para>
/// Clicking outside the viewport rectangle recentres it on the pointer and starts a drag, which is the
/// behaviour people expect from a mini map rather than a jump that needs two clicks.
/// </para>
/// </remarks>
public class MiniMap : FrameworkElement
{
    /// <summary>Identifies the <see cref="ContentBounds"/> property.</summary>
    public static readonly DependencyProperty ContentBoundsProperty = DependencyProperty.Register(
        nameof(ContentBounds), typeof(Rect), typeof(MiniMap),
        new FrameworkPropertyMetadata(new Rect(0, 0, 1000, 700), FrameworkPropertyMetadataOptions.AffectsRender, OnMappingChanged));

    /// <summary>Identifies the <see cref="Viewport"/> property.</summary>
    public static readonly DependencyProperty ViewportProperty = DependencyProperty.Register(
        nameof(Viewport), typeof(Rect), typeof(MiniMap),
        new FrameworkPropertyMetadata(new Rect(0, 0, 400, 280), FrameworkPropertyMetadataOptions.AffectsRender, OnMappingChanged));

    /// <summary>Identifies the <see cref="ItemsSource"/> property.</summary>
    public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
        nameof(ItemsSource), typeof(IEnumerable), typeof(MiniMap),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Identifies the <see cref="ItemBrush"/> property.</summary>
    public static readonly DependencyProperty ItemBrushProperty = DependencyProperty.Register(
        nameof(ItemBrush), typeof(Brush), typeof(MiniMap),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Identifies the <see cref="ViewportBrush"/> property.</summary>
    public static readonly DependencyProperty ViewportBrushProperty = DependencyProperty.Register(
        nameof(ViewportBrush), typeof(Brush), typeof(MiniMap),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    private Point _dragOrigin;
    private Rect _dragStart;
    private bool _dragging;

    /// <summary>Raised while the viewport rectangle is dragged.</summary>
    public event EventHandler<MiniMapViewportChangedEventArgs>? ViewportChanged;

    /// <summary>Extents of the whole drawing, in the same units as <see cref="Viewport"/>.</summary>
    public Rect ContentBounds
    {
        get => (Rect)GetValue(ContentBoundsProperty);
        set => SetValue(ContentBoundsProperty, value);
    }

    /// <summary>The visible part of the drawing, in world coordinates.</summary>
    public Rect Viewport
    {
        get => (Rect)GetValue(ViewportProperty);
        set => SetValue(ViewportProperty, value);
    }

    /// <summary>The blocks to draw, usually one per wall or sheet region.</summary>
    public IEnumerable? ItemsSource
    {
        get => (IEnumerable?)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    /// <summary>Default block colour.</summary>
    public Brush? ItemBrush
    {
        get => (Brush?)GetValue(ItemBrushProperty);
        set => SetValue(ItemBrushProperty, value);
    }

    /// <summary>Fill of the viewport rectangle.</summary>
    public Brush? ViewportBrush
    {
        get => (Brush?)GetValue(ViewportBrushProperty);
        set => SetValue(ViewportBrushProperty, value);
    }

    private static void OnMappingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is MiniMap map) map.InvalidateMeasure();
    }

    /// <summary>Scale that maps a world unit to a pixel, honouring letterboxing.</summary>
    public double Scale
    {
        get
        {
            var content = ContentBounds;
            if (content.Width <= 0 || content.Height <= 0) return 1;

            double width = ActualWidth > 0 ? ActualWidth : 160;
            double height = ActualHeight > 0 ? ActualHeight : 120;

            return Math.Min(width / content.Width, height / content.Height);
        }
    }

    /// <summary>Converts a world point to a point inside the control.</summary>
    public Point WorldToMap(Point world)
    {
        double scale = Scale;
        var content = ContentBounds;

        double offsetX = (ActualWidth - content.Width * scale) / 2;
        double offsetY = (ActualHeight - content.Height * scale) / 2;

        return new Point(
            offsetX + (world.X - content.X) * scale,
            offsetY + (world.Y - content.Y) * scale);
    }

    /// <summary>Converts a point inside the control back to world coordinates.</summary>
    public Point MapToWorld(Point point)
    {
        double scale = Scale <= 0 ? 1 : Scale;
        var content = ContentBounds;

        double offsetX = (ActualWidth - content.Width * scale) / 2;
        double offsetY = (ActualHeight - content.Height * scale) / 2;

        return new Point(
            content.X + (point.X - offsetX) / scale,
            content.Y + (point.Y - offsetY) / scale);
    }

    /// <summary>Moves the viewport so its centre sits on <paramref name="world"/>, clamped to the sheet.</summary>
    public void CentreOn(Point world)
    {
        var viewport = Viewport;
        var next = new Rect(
            world.X - viewport.Width / 2,
            world.Y - viewport.Height / 2,
            viewport.Width,
            viewport.Height);

        Viewport = Clamp(next);
    }

    /// <summary>Fits the viewport to the whole drawing.</summary>
    public void Fit() => Viewport = ContentBounds;

    private Rect Clamp(Rect viewport)
    {
        var content = ContentBounds;

        double width = Math.Min(Math.Max(1, viewport.Width), Math.Max(1, content.Width));
        double height = Math.Min(Math.Max(1, viewport.Height), Math.Max(1, content.Height));

        double x = Math.Max(content.X, Math.Min(content.X + content.Width - width, viewport.X));
        double y = Math.Max(content.Y, Math.Min(content.Y + content.Height - height, viewport.Y));

        return new Rect(x, y, width, height);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        double width = double.IsInfinity(availableSize.Width) ? 180 : availableSize.Width;
        double height = double.IsInfinity(availableSize.Height) || availableSize.Height <= 0
            ? 130
            : Math.Min(availableSize.Height, 130);

        return new Size(width, height);
    }

    protected override void OnRender(DrawingContext dc)
    {
        var surface = TryFindResource("CONCOST.Brush.SurfaceSecondary") as Brush ?? Brushes.WhiteSmoke;
        var border = TryFindResource("CONCOST.Brush.Border") as Brush ?? Brushes.Gainsboro;
        var accent = ViewportBrush ?? TryFindResource("CONCOST.Brush.Primary") as Brush ?? Brushes.SteelBlue;
        var itemBrush = ItemBrush ?? TryFindResource("CONCOST.Brush.TextSecondary") as Brush ?? Brushes.Gray;

        var area = new Rect(RenderSize);
        dc.DrawRectangle(surface, new Pen(border, 1), area);

        double scale = Scale;

        foreach (var item in Items)
        {
            var bounds = item.Bounds;
            if (bounds.Width <= 0 || bounds.Height <= 0) continue;

            var topLeft = WorldToMap(bounds.TopLeft);
            var rect = new Rect(topLeft.X, topLeft.Y, Math.Max(1, bounds.Width * scale), Math.Max(1, bounds.Height * scale));

            dc.DrawRectangle(item.Brush ?? itemBrush, null, rect);
        }

        var viewport = Viewport;
        if (viewport.Width > 0 && viewport.Height > 0)
        {
            var topLeft = WorldToMap(viewport.TopLeft);

            dc.DrawRectangle(accent, new Pen(accent, 1.5),
                new Rect(topLeft.X, topLeft.Y, viewport.Width * scale, viewport.Height * scale));
        }
    }

    private IEnumerable<MiniMapItem> Items =>
        (ItemsSource as IEnumerable)?.OfType<MiniMapItem>() ?? Enumerable.Empty<MiniMapItem>();

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);

        var point = e.GetPosition(this);
        var world = MapToWorld(point);

        if (!Viewport.Contains(world)) CentreOn(world);

        _dragging = true;
        _dragOrigin = point;
        _dragStart = Viewport;
        CaptureMouse();
        e.Handled = true;
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);

        if (!_dragging || e.LeftButton != MouseButtonState.Pressed) return;

        double scale = Scale <= 0 ? 1 : Scale;
        var point = e.GetPosition(this);

        var moved = new Rect(
            _dragStart.X - (point.X - _dragOrigin.X) / scale,
            _dragStart.Y - (point.Y - _dragOrigin.Y) / scale,
            _dragStart.Width,
            _dragStart.Height);

        Viewport = Clamp(moved);
        ViewportChanged?.Invoke(this, new MiniMapViewportChangedEventArgs(Viewport, false));
    }

    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonUp(e);

        if (!_dragging) return;

        _dragging = false;
        ReleaseMouseCapture();
        ViewportChanged?.Invoke(this, new MiniMapViewportChangedEventArgs(Viewport, true));
    }
}
