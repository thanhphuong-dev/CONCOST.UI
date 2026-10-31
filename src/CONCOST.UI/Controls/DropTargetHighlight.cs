using System;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace CONCOST.UI.Controls;

/// <summary>
/// Draws a highlight over any element while a drag is hovering it — the "you can drop here" outline,
/// without the element having to know about drag and drop.
/// </summary>
/// <remarks>
/// <para>
/// The outline is an <see cref="Adorner"/>, so it sits above the element's own content and never
/// disturbs its layout: no extra border, no padding shift, and it works on a <c>TextBlock</c> as well
/// as on a panel.
/// </para>
/// <para>
/// It only reports the hover; accepting the drop is still the element's job through
/// <see cref="DragDropEffects"/>. Keeping those apart is what lets a host highlight a whole card while
/// only one row inside it accepts the data.
/// </para>
/// </remarks>
public static class DropTargetHighlight
{
    public static readonly DependencyProperty IsEnabledProperty =
        DependencyProperty.RegisterAttached("IsEnabled", typeof(bool), typeof(DropTargetHighlight),
            new PropertyMetadata(false, OnIsEnabledChanged));

    /// <summary>Turns the hover outline on for this element.</summary>
    public static bool GetIsEnabled(DependencyObject element) => (bool)element.GetValue(IsEnabledProperty);

    public static void SetIsEnabled(DependencyObject element, bool value) =>
        element.SetValue(IsEnabledProperty, value);

    public static readonly DependencyProperty BrushProperty =
        DependencyProperty.RegisterAttached("Brush", typeof(Brush), typeof(DropTargetHighlight),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));

    /// <summary>Stroke of the outline. Empty takes <c>CONCOST.Brush.Primary</c>.</summary>
    public static Brush? GetBrush(DependencyObject element) => (Brush?)element.GetValue(BrushProperty);

    public static void SetBrush(DependencyObject element, Brush? value) =>
        element.SetValue(BrushProperty, value);

    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.RegisterAttached("CornerRadius", typeof(CornerRadius), typeof(DropTargetHighlight),
            new PropertyMetadata(new CornerRadius(6)));

    /// <summary>Corner radius of the outline. Default 6.</summary>
    public static CornerRadius GetCornerRadius(DependencyObject element) =>
        (CornerRadius)element.GetValue(CornerRadiusProperty);

    public static void SetCornerRadius(DependencyObject element, CornerRadius value) =>
        element.SetValue(CornerRadiusProperty, value);

    private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not UIElement element) return;

        if ((bool)e.NewValue)
        {
            element.DragEnter += OnDragEnter;
            element.DragOver += OnDragOver;
            element.DragLeave += OnDragLeave;
            element.Drop += OnDrop;
        }
        else
        {
            element.DragEnter -= OnDragEnter;
            element.DragOver -= OnDragOver;
            element.DragLeave -= OnDragLeave;
            element.Drop -= OnDrop;
            Hide(element);
        }
    }

    private static void OnDragEnter(object sender, DragEventArgs e) => Show((UIElement)sender);

    private static void OnDragOver(object sender, DragEventArgs e) => Show((UIElement)sender);

    private static void OnDragLeave(object sender, DragEventArgs e) => Hide((UIElement)sender);

    private static void OnDrop(object sender, DragEventArgs e) => Hide((UIElement)sender);

    private static void Show(UIElement element)
    {
        var layer = AdornerLayer.GetAdornerLayer(element);
        if (layer is null) return;

        if (Find(layer, element) is not null) return;

        layer.Add(new HighlightAdorner(element));
    }

    private static void Hide(UIElement element)
    {
        var layer = AdornerLayer.GetAdornerLayer(element);
        if (layer is null) return;

        if (Find(layer, element) is { } adorner) layer.Remove(adorner);
    }

    private static HighlightAdorner? Find(AdornerLayer layer, UIElement element)
    {
        foreach (var adorner in layer.GetAdorners(element) ?? Array.Empty<Adorner>())
        {
            if (adorner is HighlightAdorner match) return match;
        }

        return null;
    }

    private sealed class HighlightAdorner : Adorner
    {
        public HighlightAdorner(UIElement adorned) : base(adorned)
        {
            IsHitTestVisible = false;
        }

        protected override void OnRender(DrawingContext dc)
        {
            var brush = GetBrush(AdornedElement)
                        ?? LookupBrush("CONCOST.Brush.Primary") as Brush
                        ?? Brushes.DodgerBlue;

            var radius = GetCornerRadius(AdornedElement);
            var rect = new Rect(AdornedElement.RenderSize);
            rect.Inflate(-1, -1);

            var pen = new Pen(brush, 1.5)
            {
                DashStyle = new DashStyle(new double[] { 4, 3 }, 0)
            };

            dc.DrawRoundedRectangle(null, pen, rect, radius.TopLeft, radius.TopLeft);
        }

        private object? LookupBrush(object key) =>
            AdornedElement is FrameworkElement element ? element.TryFindResource(key) : null;
    }
}
