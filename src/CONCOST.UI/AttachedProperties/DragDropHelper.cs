using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace CONCOST.UI.AttachedProperties;

/// <summary>
/// High-performance Adorner that renders a visual insertion line between items during drag and drop.
/// Uses pre-frozen hardware brushes and smart state caching to eliminate frame jitter and visual tearing.
/// </summary>
public class InsertionAdorner : Adorner
{
    private bool _isBottom;
    private static readonly Brush OrangeBrush;
    private static readonly Pen OrangePen;

    static InsertionAdorner()
    {
        var brush = new SolidColorBrush(Color.FromRgb(255, 121, 0));
        brush.Freeze();
        OrangeBrush = brush;

        var pen = new Pen(OrangeBrush, 2.5);
        pen.Freeze();
        OrangePen = pen;
    }

    public bool IsBottom
    {
        get => _isBottom;
        set
        {
            if (_isBottom != value)
            {
                _isBottom = value;
                InvalidateVisual();
            }
        }
    }

    public InsertionAdorner(UIElement adornedElement, bool isBottom) : base(adornedElement)
    {
        IsHitTestVisible = false;
        _isBottom = isBottom;
    }

    protected override void OnRender(DrawingContext dc)
    {
        var rect = new Rect(AdornedElement.RenderSize);
        double y = _isBottom ? rect.Bottom : rect.Top;

        // Draw horizontal line with start & end circular anchors
        dc.DrawLine(OrangePen, new Point(rect.Left, y), new Point(rect.Right, y));
        dc.DrawEllipse(OrangeBrush, null, new Point(rect.Left, y), 3.5, 3.5);
        dc.DrawEllipse(OrangeBrush, null, new Point(rect.Right, y), 3.5, 3.5);
    }
}

/// <summary>
/// Floating semi-transparent visual preview adorner that follows the mouse cursor while dragging,
/// giving the user tactile feedback of holding and moving the item.
/// </summary>
public class DragPreviewAdorner : Adorner
{
    private readonly VisualBrush _brush;
    private readonly Size _size;
    private Point _currentPosition;
    private readonly Point _offset;
    private static readonly Pen BorderPen;

    static DragPreviewAdorner()
    {
        var pen = new Pen(new SolidColorBrush(Color.FromArgb(180, 255, 121, 0)), 1.5);
        pen.Freeze();
        BorderPen = pen;
    }

    public DragPreviewAdorner(UIElement rootVisual, FrameworkElement draggedContainer, Point offset)
        : base(rootVisual)
    {
        IsHitTestVisible = false;
        _size = new Size(draggedContainer.ActualWidth, draggedContainer.ActualHeight);
        _offset = offset;

        _brush = new VisualBrush(draggedContainer)
        {
            Opacity = 0.85,
            Stretch = Stretch.None
        };
    }

    public void UpdatePosition(Point point)
    {
        _currentPosition = point;
        InvalidateVisual();
    }

    protected override void OnRender(DrawingContext dc)
    {
        var origin = new Point(_currentPosition.X - _offset.X, _currentPosition.Y - _offset.Y);
        var rect = new Rect(origin, _size);

        // Draw card preview with rounded corners and accent border
        dc.DrawRoundedRectangle(_brush, BorderPen, rect, 6, 6);
    }
}

/// <summary>
/// Powerful Attached Properties Drag & Drop helper inspired by gong-wpf-dragdrop.
/// Enables seamless reordering, transfer between ListBox, ListView, TreeView, and DataGrid purely via XAML.
/// Upgraded with smooth DragPreviewAdorner, zero-jitter InsertionAdorner caching, and full gap tolerance.
/// </summary>
public static class DragDropHelper
{
    private static Point _dragStartPoint;
    private static Point _dragOffsetInContainer;
    private static object? _draggedData;
    private static FrameworkElement? _draggedContainer;
    private static ItemsControl? _sourceItemsControl;
    private static InsertionAdorner? _currentAdorner;
    private static DragPreviewAdorner? _dragPreviewAdorner;
    private static Window? _adornedWindow;

    [StructLayout(LayoutKind.Sequential)]
    private struct Win32Point
    {
        public int X;
        public int Y;
    }

    [DllImport("user32.dll")]
    private static extern bool GetCursorPos(out Win32Point lpPoint);

    public static readonly DependencyProperty IsDragSourceProperty =
        DependencyProperty.RegisterAttached("IsDragSource", typeof(bool), typeof(DragDropHelper),
            new PropertyMetadata(false, OnIsDragSourceChanged));

    public static readonly DependencyProperty IsDropTargetProperty =
        DependencyProperty.RegisterAttached("IsDropTarget", typeof(bool), typeof(DragDropHelper),
            new PropertyMetadata(false, OnIsDropTargetChanged));

    public static readonly DependencyProperty DragDropGroupProperty =
        DependencyProperty.RegisterAttached("DragDropGroup", typeof(string), typeof(DragDropHelper),
            new PropertyMetadata(string.Empty));

    public static bool GetIsDragSource(DependencyObject obj) => (bool)obj.GetValue(IsDragSourceProperty);
    public static void SetIsDragSource(DependencyObject obj, bool value) => obj.SetValue(IsDragSourceProperty, value);

    public static bool GetIsDropTarget(DependencyObject obj) => (bool)obj.GetValue(IsDropTargetProperty);
    public static void SetIsDropTarget(DependencyObject obj, bool value) => obj.SetValue(IsDropTargetProperty, value);

    public static string GetDragDropGroup(DependencyObject obj) => (string)obj.GetValue(DragDropGroupProperty);
    public static void SetDragDropGroup(DependencyObject obj, string value) => obj.SetValue(DragDropGroupProperty, value);

    private static void OnIsDragSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not ItemsControl itemsControl) return;

        if ((bool)e.NewValue)
        {
            itemsControl.PreviewMouseLeftButtonDown += OnPreviewMouseLeftButtonDown;
            itemsControl.PreviewMouseMove += OnPreviewMouseMove;
        }
        else
        {
            itemsControl.PreviewMouseLeftButtonDown -= OnPreviewMouseLeftButtonDown;
            itemsControl.PreviewMouseMove -= OnPreviewMouseMove;
        }
    }

    private static void OnIsDropTargetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not ItemsControl itemsControl) return;

        if ((bool)e.NewValue)
        {
            itemsControl.AllowDrop = true;
            itemsControl.PreviewDragOver += OnPreviewDragOver;
            itemsControl.PreviewDragLeave += OnPreviewDragLeave;
            itemsControl.PreviewDrop += OnPreviewDrop;
        }
        else
        {
            itemsControl.AllowDrop = false;
            itemsControl.PreviewDragOver -= OnPreviewDragOver;
            itemsControl.PreviewDragLeave -= OnPreviewDragLeave;
            itemsControl.PreviewDrop -= OnPreviewDrop;
        }
    }

    private static void OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is ItemsControl itemsControl)
        {
            _dragStartPoint = e.GetPosition(itemsControl);
            var itemContainer = GetItemContainer(itemsControl, e.OriginalSource as DependencyObject);
            if (itemContainer != null)
            {
                _draggedData = itemsControl.ItemContainerGenerator.ItemFromContainer(itemContainer);
                _draggedContainer = itemContainer;
                _dragOffsetInContainer = e.GetPosition(itemContainer);
                _sourceItemsControl = itemsControl;
            }
        }
    }

    private static void OnPreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (e.LeftButton != MouseButtonState.Pressed || _draggedData == null || sender is not ItemsControl itemsControl)
            return;

        Point currentPoint = e.GetPosition(itemsControl);
        Vector diff = currentPoint - _dragStartPoint;

        if (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
            Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
        {
            var dataObj = new DataObject("CONCOST_DRAG_DATA", _draggedData);

            var window = Window.GetWindow(itemsControl);
            AdornerLayer? rootLayer = null;
            if (window != null && _draggedContainer != null)
            {
                rootLayer = AdornerLayer.GetAdornerLayer(window.Content as Visual ?? window);
                if (rootLayer != null)
                {
                    _dragPreviewAdorner = new DragPreviewAdorner(window.Content as UIElement ?? window, _draggedContainer, _dragOffsetInContainer);
                    rootLayer.Add(_dragPreviewAdorner);
                    _adornedWindow = window;
                }
            }

            itemsControl.GiveFeedback += OnGiveFeedback;

            try
            {
                System.Windows.DragDrop.DoDragDrop(itemsControl, dataObj, DragDropEffects.Move | DragDropEffects.Copy);
            }
            finally
            {
                itemsControl.GiveFeedback -= OnGiveFeedback;
                if (_dragPreviewAdorner != null && rootLayer != null)
                {
                    rootLayer.Remove(_dragPreviewAdorner);
                    _dragPreviewAdorner = null;
                }
                _draggedData = null;
                _sourceItemsControl = null;
                _draggedContainer = null;
                _adornedWindow = null;
                RemoveAdorner();
            }
        }
    }

    private static void OnGiveFeedback(object sender, GiveFeedbackEventArgs e)
    {
        if (_dragPreviewAdorner != null && _adornedWindow != null)
        {
            if (GetCursorPos(out var win32Pt))
            {
                try
                {
                    Point screenPt = new Point(win32Pt.X, win32Pt.Y);
                    Point wpfPt = _adornedWindow.PointFromScreen(screenPt);
                    _dragPreviewAdorner.UpdatePosition(wpfPt);
                }
                catch { }
            }
        }
    }

    private static void OnPreviewDragOver(object sender, DragEventArgs e)
    {
        if (!e.Data.GetDataPresent("CONCOST_DRAG_DATA") || sender is not ItemsControl targetItemsControl)
        {
            e.Effects = DragDropEffects.None;
            e.Handled = true;
            return;
        }

        // Check DragDropGroup compatibility
        string sourceGroup = _sourceItemsControl != null ? GetDragDropGroup(_sourceItemsControl) : string.Empty;
        string targetGroup = GetDragDropGroup(targetItemsControl);

        if (!string.IsNullOrEmpty(sourceGroup) && !string.IsNullOrEmpty(targetGroup) && sourceGroup != targetGroup)
        {
            e.Effects = DragDropEffects.None;
            e.Handled = true;
            RemoveAdorner();
            return;
        }

        e.Effects = DragDropEffects.Move;

        // Show Insertion line adorner with gap-bridging and zero-flicker caching
        var (itemContainer, isBottom) = FindTargetItemAndPosition(targetItemsControl, e);
        if (itemContainer != null)
        {
            ShowAdorner(itemContainer, isBottom);
        }
        else
        {
            RemoveAdorner();
        }

        e.Handled = true;
    }

    private static void OnPreviewDragLeave(object sender, DragEventArgs e)
    {
        RemoveAdorner();
    }

    private static void OnPreviewDrop(object sender, DragEventArgs e)
    {
        RemoveAdorner();

        if (!e.Data.GetDataPresent("CONCOST_DRAG_DATA") || sender is not ItemsControl targetItemsControl)
            return;

        object droppedData = e.Data.GetData("CONCOST_DRAG_DATA");
        if (droppedData == null) return;

        var sourceList = GetUnderlyingList(_sourceItemsControl);
        var targetList = GetUnderlyingList(targetItemsControl);

        if (targetList == null) return;

        var (targetContainer, isBottom) = FindTargetItemAndPosition(targetItemsControl, e);
        int targetIndex = targetList.Count;

        if (targetContainer != null)
        {
            object targetItem = targetItemsControl.ItemContainerGenerator.ItemFromContainer(targetContainer);
            int idx = targetList.IndexOf(targetItem);
            if (idx >= 0)
            {
                targetIndex = isBottom ? idx + 1 : idx;
            }
        }

        // Reordering within the same list
        if (sourceList == targetList && sourceList != null)
        {
            int oldIndex = sourceList.IndexOf(droppedData);
            if (oldIndex >= 0)
            {
                if (targetIndex > oldIndex) targetIndex--;
                sourceList.RemoveAt(oldIndex);
                targetIndex = Math.Max(0, Math.Min(sourceList.Count, targetIndex));
                sourceList.Insert(targetIndex, droppedData);
            }
        }
        // Moving across different lists
        else if (sourceList != null)
        {
            sourceList.Remove(droppedData);
            targetIndex = Math.Max(0, Math.Min(targetList.Count, targetIndex));
            targetList.Insert(targetIndex, droppedData);
        }

        e.Handled = true;
    }

    private static (FrameworkElement? container, bool isBottom) FindTargetItemAndPosition(ItemsControl itemsControl, DragEventArgs e)
    {
        int count = itemsControl.Items.Count;
        if (count == 0)
        {
            return (null, false);
        }

        // 1. Direct hit-test from e.OriginalSource
        var directContainer = GetItemContainer(itemsControl, e.OriginalSource as DependencyObject);
        if (directContainer != null)
        {
            Point pos = e.GetPosition(directContainer);
            bool isBottom = pos.Y > directContainer.RenderSize.Height / 2.0;
            return (directContainer, isBottom);
        }

        // 2. If cursor is in the inter-item margin gap or below items, resolve closest container
        Point mousePos = e.GetPosition(itemsControl);
        FrameworkElement? closestContainer = null;
        bool closestIsBottom = false;
        double minDistance = double.MaxValue;

        for (int i = 0; i < count; i++)
        {
            var container = itemsControl.ItemContainerGenerator.ContainerFromIndex(i) as FrameworkElement;
            if (container == null || !container.IsVisible) continue;

            Point containerPos = container.TransformToAncestor(itemsControl).Transform(new Point(0, 0));
            double top = containerPos.Y;
            double bottom = top + container.ActualHeight;
            double mid = (top + bottom) / 2.0;

            if (mousePos.Y >= top && mousePos.Y <= bottom)
            {
                return (container, mousePos.Y > mid);
            }

            double dist = Math.Abs(mousePos.Y - mid);
            if (dist < minDistance)
            {
                minDistance = dist;
                closestContainer = container;
                closestIsBottom = mousePos.Y > mid;
            }
        }

        if (closestContainer != null)
        {
            return (closestContainer, closestIsBottom);
        }

        var lastContainer = itemsControl.ItemContainerGenerator.ContainerFromIndex(count - 1) as FrameworkElement;
        if (lastContainer != null)
        {
            return (lastContainer, true);
        }

        return (null, false);
    }

    private static IList? GetUnderlyingList(ItemsControl? itemsControl)
    {
        if (itemsControl == null) return null;
        if (itemsControl.ItemsSource is IList list) return list;
        return itemsControl.Items;
    }

    private static FrameworkElement? GetItemContainer(ItemsControl itemsControl, DependencyObject? visualChild)
    {
        while (visualChild != null && visualChild != itemsControl)
        {
            if (itemsControl.ItemContainerGenerator.ItemFromContainer(visualChild) != DependencyProperty.UnsetValue)
            {
                return visualChild as FrameworkElement;
            }
            visualChild = VisualTreeHelper.GetParent(visualChild);
        }
        return null;
    }

    private static void ShowAdorner(UIElement targetElement, bool isBottom)
    {
        if (_currentAdorner != null)
        {
            if (_currentAdorner.AdornedElement == targetElement)
            {
                // Seamless state update: avoid removing/re-adding to AdornerLayer to eliminate frame flickering
                _currentAdorner.IsBottom = isBottom;
                return;
            }

            RemoveAdorner();
        }

        var layer = AdornerLayer.GetAdornerLayer(targetElement);
        if (layer == null) return;

        _currentAdorner = new InsertionAdorner(targetElement, isBottom);
        layer.Add(_currentAdorner);
    }

    private static void RemoveAdorner()
    {
        if (_currentAdorner != null)
        {
            var layer = AdornerLayer.GetAdornerLayer(_currentAdorner.AdornedElement);
            layer?.Remove(_currentAdorner);
            _currentAdorner = null;
        }
    }
}
