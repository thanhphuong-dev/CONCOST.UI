using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CONCOST.UI.Controls;

/// <summary>
/// Sprite is a floating assistant widget inspired by HandyControl (or Assistive Touch / Floating Action Button).
/// It floats above application content, is freely draggable across the window, automatically snaps to screen edges,
/// and expands an interactive quick-action menu when clicked or hovered.
/// </summary>
public class Sprite : ContentControl
{
    private Point _dragStartPoint;
    private Point _startMarginPoint;
    private bool _isDragging;
    private Window? _parentWindow;

    static Sprite()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(Sprite), new FrameworkPropertyMetadata(typeof(Sprite)));
    }

    public static readonly DependencyProperty IsOpenProperty =
        DependencyProperty.Register(nameof(IsOpen), typeof(bool), typeof(Sprite),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public static readonly DependencyProperty MenuContentProperty =
        DependencyProperty.Register(nameof(MenuContent), typeof(object), typeof(Sprite),
            new PropertyMetadata(null));

    public static readonly DependencyProperty AutoSnapProperty =
        DependencyProperty.Register(nameof(AutoSnap), typeof(bool), typeof(Sprite),
            new PropertyMetadata(true));

    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(Sprite),
            new PropertyMetadata(new CornerRadius(24)));

    public static readonly DependencyProperty ShowBorderProperty =
        DependencyProperty.Register(nameof(ShowBorder), typeof(bool), typeof(Sprite),
            new PropertyMetadata(true));

    public bool IsOpen
    {
        get => (bool)GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    public object? MenuContent
    {
        get => GetValue(MenuContentProperty);
        set => SetValue(MenuContentProperty, value);
    }

    public bool AutoSnap
    {
        get => (bool)GetValue(AutoSnapProperty);
        set => SetValue(AutoSnapProperty, value);
    }

    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    public bool ShowBorder
    {
        get => (bool)GetValue(ShowBorderProperty);
        set => SetValue(ShowBorderProperty, value);
    }

    public Sprite()
    {
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        _parentWindow = Window.GetWindow(this);
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        _parentWindow = null;
    }

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);

        if (_parentWindow == null) _parentWindow = Window.GetWindow(this);
        if (_parentWindow == null) return;

        _isDragging = false;
        _dragStartPoint = e.GetPosition(_parentWindow);
        _startMarginPoint = new Point(Margin.Left, Margin.Top);

        CaptureMouse();
        e.Handled = true;
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);

        if (!IsMouseCaptured || _parentWindow == null) return;

        Point currentPoint = e.GetPosition(_parentWindow);
        Vector diff = currentPoint - _dragStartPoint;

        if (Math.Abs(diff.X) > 4 || Math.Abs(diff.Y) > 4)
        {
            _isDragging = true;
            IsOpen = false; // close menu while dragging

            double newLeft = _startMarginPoint.X + diff.X;
            double newTop = _startMarginPoint.Y + diff.Y;

            // Restrict within window bounds
            double maxLeft = Math.Max(0, _parentWindow.ActualWidth - ActualWidth - 10);
            double maxTop = Math.Max(0, _parentWindow.ActualHeight - ActualHeight - 40);

            newLeft = Math.Max(10, Math.Min(maxLeft, newLeft));
            newTop = Math.Max(10, Math.Min(maxTop, newTop));

            HorizontalAlignment = HorizontalAlignment.Left;
            VerticalAlignment = VerticalAlignment.Top;
            Margin = new Thickness(newLeft, newTop, 0, 0);
        }
    }

    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonUp(e);

        if (IsMouseCaptured)
        {
            ReleaseMouseCapture();

            if (!_isDragging)
            {
                // Clicked without drag -> toggle popup menu
                IsOpen = !IsOpen;
            }
            else if (AutoSnap && _parentWindow != null)
            {
                SnapToNearestEdge();
            }

            _isDragging = false;
            e.Handled = true;
        }
    }

    private void SnapToNearestEdge()
    {
        if (_parentWindow == null) return;

        double left = Margin.Left;
        double top = Margin.Top;
        double winWidth = _parentWindow.ActualWidth;

        double targetLeft = (left + ActualWidth / 2.0 < winWidth / 2.0) ? 16 : Math.Max(16, winWidth - ActualWidth - 24);

        Margin = new Thickness(targetLeft, top, 0, 0);
    }
}
