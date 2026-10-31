using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

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

    private static void OnIsOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DockPreview preview)
        {
            preview.Visibility = (bool)e.NewValue ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);

        if (!IsOpen || ActualWidth <= 0 || ActualHeight <= 0) return;

        var accent = TryFindResource("CONCOST.Brush.Primary") as Brush ?? Brushes.DodgerBlue;
        var fill = PreviewBrush ?? new SolidColorBrush(Color.FromArgb(38, 255, 121, 0));
        var border = BorderBrush ?? accent;

        var bounds = new Rect(0, 0, ActualWidth, ActualHeight);
        var pen = new Pen(border, 1.5) { DashStyle = new DashStyle(new double[] { 4, 3 }, 0) };

        dc.DrawRectangle(fill, pen, bounds);
    }
}
