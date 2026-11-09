using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace CONCOST.UI.Controls;

/// <summary>
/// The drop hint shown while a panel is being dragged: a translucent accent rectangle with a dashed
/// border over the area the panel would occupy.
/// </summary>
public class DockPreview : FrameworkElement
{
    public DockPreview()
    {
        IsHitTestVisible = false;
        Visibility = Visibility.Collapsed;
    }

    public static readonly DependencyProperty IsOpenProperty =
        DependencyProperty.Register(nameof(IsOpen), typeof(bool), typeof(DockPreview),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender, OnIsOpenChanged));

    /// <summary>Show the hint.</summary>
    public bool IsOpen
    {
        get => (bool)GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    public static readonly DependencyProperty SideProperty =
        DependencyProperty.Register(nameof(Side), typeof(DockSide), typeof(DockPreview),
            new FrameworkPropertyMetadata(DockSide.Center, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Edge the panel would dock to. Changes only the drawn hint, not the geometry.</summary>
    public DockSide Side
    {
        get => (DockSide)GetValue(SideProperty);
        set => SetValue(SideProperty, value);
    }

    public static readonly DependencyProperty PreviewBrushProperty =
        DependencyProperty.Register(nameof(PreviewBrush), typeof(Brush), typeof(DockPreview),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Fill of the hint. Empty uses the theme accent at low opacity.</summary>
    public Brush? PreviewBrush
    {
        get => (Brush?)GetValue(PreviewBrushProperty);
        set => SetValue(PreviewBrushProperty, value);
    }

    public static readonly DependencyProperty BorderBrushProperty =
        DependencyProperty.Register(nameof(BorderBrush), typeof(Brush), typeof(DockPreview),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Dashed outline colour. Empty uses the theme accent.</summary>
    public Brush? BorderBrush
    {
        get => (Brush?)GetValue(BorderBrushProperty);
        set => SetValue(BorderBrushProperty, value);
    }

    public static readonly DependencyProperty HighlightedSideProperty =
        DependencyProperty.Register(nameof(HighlightedSide), typeof(DockSide), typeof(DockPreview),
            new FrameworkPropertyMetadata(DockSide.Center, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Target highlighted by the docking compass.</summary>
    public DockSide HighlightedSide
    {
        get => (DockSide)GetValue(HighlightedSideProperty);
        set => SetValue(HighlightedSideProperty, value);
    }

    public static readonly DependencyProperty CompassCenterProperty =
        DependencyProperty.Register(nameof(CompassCenter), typeof(Point), typeof(DockPreview),
            new FrameworkPropertyMetadata(default(Point), FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Center of the dock compass in manager coordinates.</summary>
    public Point CompassCenter
    {
        get => (Point)GetValue(CompassCenterProperty);
        set => SetValue(CompassCenterProperty, value);
    }

    public static readonly DependencyProperty TargetRectProperty =
        DependencyProperty.Register(nameof(TargetRect), typeof(Rect), typeof(DockPreview),
            new FrameworkPropertyMetadata(Rect.Empty, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Manager-space rectangle that the current drop would occupy.</summary>
    public Rect TargetRect
    {
        get => (Rect)GetValue(TargetRectProperty);
        set => SetValue(TargetRectProperty, value);
    }

    public static readonly DependencyProperty IsOuterTargetProperty =
        DependencyProperty.Register(nameof(IsOuterTarget), typeof(bool), typeof(DockPreview),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Highlights a manager-edge target instead of a target-pane compass button.</summary>
    public bool IsOuterTarget
    {
        get => (bool)GetValue(IsOuterTargetProperty);
        set => SetValue(IsOuterTargetProperty, value);
    }

    private static void OnIsOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not DockPreview preview) return;

        preview.BeginAnimation(OpacityProperty, null);
        if ((bool)e.NewValue)
        {
            preview.Visibility = Visibility.Visible;
            preview.Opacity = 0;
            preview.BeginAnimation(OpacityProperty, new DoubleAnimation(0, 1,
                new Duration(TimeSpan.FromMilliseconds(110)))
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            });
            return;
        }

        if (preview.Visibility != Visibility.Visible) return;
        var fadeOut = new DoubleAnimation(preview.Opacity, 0,
            new Duration(TimeSpan.FromMilliseconds(85)))
        {
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseIn }
        };
        fadeOut.Completed += (_, _) =>
        {
            if (!preview.IsOpen) preview.Visibility = Visibility.Collapsed;
        };
        preview.BeginAnimation(OpacityProperty, fadeOut);
    }

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);

        if (Visibility != Visibility.Visible || ActualWidth <= 0 || ActualHeight <= 0) return;

        var accent = TryFindResource("CONCOST.Brush.Primary") as Brush ?? Brushes.DodgerBlue;
        var border = BorderBrush ?? accent;

        // The wash is derived from the same brush as the dashed outline. Hardcoding an orange fill
        // meant a host with a different accent got an orange rectangle with a blue border.
        var fill = PreviewBrush ?? Tinted(accent);
        var surface = TryFindResource("CONCOST.Brush.Surface") as Brush ?? Brushes.White;

        var pen = new Pen(border, 1.5) { DashStyle = new DashStyle(new double[] { 4, 3 }, 0) };
        var target = TargetRect.IsEmpty ? new Rect(0, 0, ActualWidth, ActualHeight) : TargetRect;
        dc.DrawRectangle(fill, pen, target);

        // Revit and Visual Studio expose a compass while dragging. Keep it in the preview layer so
        // the user can aim at the centre or any of the four split targets without hunting edges.
        // The preview spans the manager, so the compass remains visible even while the dragged
        // pane is still under the pointer. Docking targets are deliberately compact and centered.
        var center = new Point(
            Math.Max(50, Math.Min(CompassCenter.X, Math.Max(50, ActualWidth - 50))),
            Math.Max(50, Math.Min(CompassCenter.Y, Math.Max(50, ActualHeight - 50))));
        double button = 36;
        double gap = 42;
        DrawTarget(dc, new Rect(center.X - button / 2, center.Y - gap - button / 2, button, button), DockSide.Top, border, fill, surface);
        DrawTarget(dc, new Rect(center.X + gap - button / 2, center.Y - button / 2, button, button), DockSide.Right, border, fill, surface);
        DrawTarget(dc, new Rect(center.X - button / 2, center.Y + gap - button / 2, button, button), DockSide.Bottom, border, fill, surface);
        DrawTarget(dc, new Rect(center.X - gap - button / 2, center.Y - button / 2, button, button), DockSide.Left, border, fill, surface);
        DrawTarget(dc, new Rect(center.X - button / 2, center.Y - button / 2, button, button), DockSide.Center, border, fill, surface);

        // Four manager-wide zones stay available around the outer frame, as in IDE dock guides.
        const double edgeButton = 32;
        DrawTarget(dc, new Rect(ActualWidth / 2 - edgeButton / 2, 8, edgeButton, edgeButton), DockSide.Top, border, fill, surface, true);
        DrawTarget(dc, new Rect(ActualWidth - edgeButton - 8, ActualHeight / 2 - edgeButton / 2, edgeButton, edgeButton), DockSide.Right, border, fill, surface, true);
        DrawTarget(dc, new Rect(ActualWidth / 2 - edgeButton / 2, ActualHeight - edgeButton - 8, edgeButton, edgeButton), DockSide.Bottom, border, fill, surface, true);
        DrawTarget(dc, new Rect(8, ActualHeight / 2 - edgeButton / 2, edgeButton, edgeButton), DockSide.Left, border, fill, surface, true);
    }

    private void DrawTarget(DrawingContext dc, Rect rect, DockSide side, Brush border, Brush fill, Brush surface, bool outer = false)
    {
        bool active = side == HighlightedSide && (outer == IsOuterTarget);
        var targetFill = active ? fill : surface;
        var outline = new Pen(active ? border : border, active ? 2 : 1);
        dc.DrawRoundedRectangle(targetFill, outline, rect, 5, 5);

        double cell = Math.Max(3, (rect.Width - 16) / 3);
        double gridX = rect.X + (rect.Width - cell * 3) / 2;
        double gridY = rect.Y + (rect.Height - cell * 3) / 2;
        var selected = Tinted(border);
        var glyphPen = new Pen(border, .8);
        for (int row = 0; row < 3; row++)
        for (int column = 0; column < 3; column++)
        {
            bool destination = side switch
            {
                DockSide.Left => column == 0,
                DockSide.Right => column == 2,
                DockSide.Top => row == 0,
                DockSide.Bottom => row == 2,
                _ => row == 1 && column == 1
            };
            var cellRect = new Rect(gridX + column * cell, gridY + row * cell, cell, cell);
            dc.DrawRectangle(destination ? (active ? border : selected) : surface, glyphPen, cellRect);
        }
    }

    /// <summary>
    /// A translucent version of the accent colour, so the hint tints the surface it covers without
    /// hiding what is under it.
    /// </summary>
    private static Brush Tinted(Brush accent)
    {
        if (accent is SolidColorBrush solid && solid.Color is Color colour)
        {
            var tinted = new SolidColorBrush(Color.FromArgb(34, colour.R, colour.G, colour.B));
            tinted.Freeze();
            return tinted;
        }

        return new SolidColorBrush(Color.FromArgb(34, 255, 121, 0));
    }
}
