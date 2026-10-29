using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace CONCOST.UI.Controls;

public enum DrawerPlacement
{
    Left,
    Right,
    Top,
    Bottom
}

[TemplatePart(Name = "PART_Mask", Type = typeof(Border))]
[TemplatePart(Name = "PART_DrawerPanel", Type = typeof(Border))]
public class Drawer : ContentControl
{
    private static readonly QuadraticEase EaseIn = CreateEasing(EasingMode.EaseIn);
    private static readonly QuadraticEase EaseOut = CreateEasing(EasingMode.EaseOut);
    private Border? _mask;
    private Border? _drawerPanel;
    private TranslateTransform? _drawerTransform;
    private BitmapCache? _animationCache;
    private int _animationVersion;

    static Drawer()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(Drawer), new FrameworkPropertyMetadata(typeof(Drawer)));
    }

    public static readonly DependencyProperty IsOpenProperty =
        DependencyProperty.Register(nameof(IsOpen), typeof(bool), typeof(Drawer),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnIsOpenChanged));

    public static readonly DependencyProperty PlacementProperty =
        DependencyProperty.Register(nameof(Placement), typeof(DrawerPlacement), typeof(Drawer),
            new PropertyMetadata(DrawerPlacement.Right, OnPlacementChanged));

    public static readonly DependencyProperty DrawerWidthProperty =
        DependencyProperty.Register(nameof(DrawerWidth), typeof(double), typeof(Drawer),
            new FrameworkPropertyMetadata(340.0, OnSizeChanged, CoerceDrawerSize));

    public static readonly DependencyProperty DrawerHeightProperty =
        DependencyProperty.Register(nameof(DrawerHeight), typeof(double), typeof(Drawer),
            new FrameworkPropertyMetadata(280.0, OnSizeChanged, CoerceDrawerSize));

    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(nameof(Title), typeof(string), typeof(Drawer),
            new PropertyMetadata("Drawer Title"));

    public bool IsOpen
    {
        get => (bool)GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    public DrawerPlacement Placement
    {
        get => (DrawerPlacement)GetValue(PlacementProperty);
        set => SetValue(PlacementProperty, value);
    }

    public double DrawerWidth
    {
        get => (double)GetValue(DrawerWidthProperty);
        set => SetValue(DrawerWidthProperty, value);
    }

    public double DrawerHeight
    {
        get => (double)GetValue(DrawerHeightProperty);
        set => SetValue(DrawerHeightProperty, value);
    }

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public override void OnApplyTemplate()
    {
        if (_mask != null) _mask.MouseLeftButtonDown -= OnMaskMouseLeftButtonDown;
        base.OnApplyTemplate();
        _mask = GetTemplateChild("PART_Mask") as Border;
        _drawerPanel = GetTemplateChild("PART_DrawerPanel") as Border;
        _drawerTransform = new TranslateTransform();
        if (_drawerPanel != null)
        {
            _drawerPanel.RenderTransform = _drawerTransform;
            _drawerPanel.RenderTransformOrigin = new Point(0.5, 0.5);
        }

        if (_mask != null)
        {
            _mask.MouseLeftButtonDown += OnMaskMouseLeftButtonDown;
        }

        UpdateAlignment();
        UpdateState(IsOpen);
    }

    private void OnMaskMouseLeftButtonDown(object sender, MouseButtonEventArgs e) => IsOpen = false;

    private static void OnIsOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Drawer dr)
        {
            dr.UpdateState((bool)e.NewValue);
        }
    }

    private static void OnPlacementChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Drawer dr)
        {
            dr.UpdateAlignment();
            dr.ResetTransformForCurrentPlacement();
        }
    }

    private static void OnSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not Drawer drawer || drawer._drawerPanel == null) return;
        drawer.UpdateAlignment();
        drawer.ResetTransformForCurrentPlacement();
    }

    private static object CoerceDrawerSize(DependencyObject d, object baseValue)
    {
        double value = (double)baseValue;
        return NumericCompatibility.IsFinite(value) ? Math.Max(0, value) : 0;
    }

    private static QuadraticEase CreateEasing(EasingMode mode)
    {
        var easing = new QuadraticEase { EasingMode = mode };
        easing.Freeze();
        return easing;
    }

    private void UpdateAlignment()
    {
        if (_drawerPanel == null) return;

        switch (Placement)
        {
            case DrawerPlacement.Left:
                _drawerPanel.HorizontalAlignment = HorizontalAlignment.Left;
                _drawerPanel.VerticalAlignment = VerticalAlignment.Stretch;
                _drawerPanel.Width = DrawerWidth;
                _drawerPanel.Height = double.NaN;
                _drawerPanel.BorderThickness = new Thickness(0, 0, 1, 0);
                break;
            case DrawerPlacement.Right:
                _drawerPanel.HorizontalAlignment = HorizontalAlignment.Right;
                _drawerPanel.VerticalAlignment = VerticalAlignment.Stretch;
                _drawerPanel.Width = DrawerWidth;
                _drawerPanel.Height = double.NaN;
                _drawerPanel.BorderThickness = new Thickness(1, 0, 0, 0);
                break;
            case DrawerPlacement.Top:
                _drawerPanel.HorizontalAlignment = HorizontalAlignment.Stretch;
                _drawerPanel.VerticalAlignment = VerticalAlignment.Top;
                _drawerPanel.Width = double.NaN;
                _drawerPanel.Height = DrawerHeight;
                _drawerPanel.BorderThickness = new Thickness(0, 0, 0, 1);
                break;
            case DrawerPlacement.Bottom:
                _drawerPanel.HorizontalAlignment = HorizontalAlignment.Stretch;
                _drawerPanel.VerticalAlignment = VerticalAlignment.Bottom;
                _drawerPanel.Width = double.NaN;
                _drawerPanel.Height = DrawerHeight;
                _drawerPanel.BorderThickness = new Thickness(0, 1, 0, 0);
                break;
        }
    }

    private void UpdateState(bool open)
    {
        if (_mask == null || _drawerPanel == null)
        {
            Visibility = open ? Visibility.Visible : Visibility.Collapsed;
            return;
        }

        UpdateAlignment();
        if (_drawerTransform == null)
        {
            CloseCompleted();
            return;
        }

        bool wasVisible = Visibility == Visibility.Visible && _drawerPanel.Visibility == Visibility.Visible;
        double currentX = _drawerTransform.X;
        double currentY = _drawerTransform.Y;
        CancelAnimations(currentX, currentY);
        int version = ++_animationVersion;

        if (open)
        {
            Visibility = Visibility.Visible;
            _mask.Visibility = Visibility.Visible;
            _drawerPanel.Visibility = Visibility.Visible;
            _mask.Opacity = 1.0;

            if (!IsLoaded)
            {
                SetTransform(0, 0);
                return;
            }

            var offset = GetPlacementOffset();
            double from = wasVisible ? (offset.IsHorizontal ? currentX : currentY) : offset.Distance;
            StartSlideAnimation(offset.IsHorizontal, from, 0, TimeSpan.FromMilliseconds(250), EaseOut, version, closeOnComplete: false);
        }
        else
        {
            if (!wasVisible || !IsLoaded)
            {
                SetTransform(GetPlacementOffset());
                CloseCompleted();
                return;
            }

            var offset = GetPlacementOffset();
            double from = offset.IsHorizontal ? currentX : currentY;
            StartSlideAnimation(offset.IsHorizontal, from, offset.Distance, TimeSpan.FromMilliseconds(200), EaseIn, version, closeOnComplete: true);
        }
    }

    private void StartSlideAnimation(bool horizontal, double from, double to, TimeSpan duration,
        IEasingFunction easing, int version, bool closeOnComplete)
    {
        if (_drawerTransform == null || _drawerPanel == null) return;

        _animationCache ??= new BitmapCache();
        _drawerPanel.CacheMode = _animationCache;

        var animation = new DoubleAnimation(from, to, duration)
        {
            EasingFunction = easing,
            FillBehavior = FillBehavior.Stop
        };
        animation.Completed += (_, _) =>
        {
            if (version != _animationVersion) return;

            var property = horizontal ? TranslateTransform.XProperty : TranslateTransform.YProperty;
            _drawerTransform.BeginAnimation(property, null);
            if (horizontal) _drawerTransform.X = to;
            else _drawerTransform.Y = to;
            _drawerPanel.CacheMode = null;

            if (closeOnComplete && !IsOpen)
                CloseCompleted();
        };

        _drawerTransform.BeginAnimation(
            horizontal ? TranslateTransform.XProperty : TranslateTransform.YProperty,
            animation,
            HandoffBehavior.SnapshotAndReplace);
    }

    private (bool IsHorizontal, double Distance) GetPlacementOffset() => Placement switch
    {
        DrawerPlacement.Left => (true, -DrawerWidth),
        DrawerPlacement.Right => (true, DrawerWidth),
        DrawerPlacement.Top => (false, -DrawerHeight),
        DrawerPlacement.Bottom => (false, DrawerHeight),
        _ => (true, DrawerWidth)
    };

    private void CancelAnimations(double currentX, double currentY)
    {
        if (_drawerTransform == null) return;

        _drawerTransform.BeginAnimation(TranslateTransform.XProperty, null);
        _drawerTransform.BeginAnimation(TranslateTransform.YProperty, null);
        _drawerTransform.X = currentX;
        _drawerTransform.Y = currentY;
        if (_drawerPanel != null) _drawerPanel.CacheMode = null;
    }

    private void SetTransform(double x, double y)
    {
        CancelAnimations(x, y);
    }

    private void SetTransform((bool IsHorizontal, double Distance) offset)
    {
        SetTransform(offset.IsHorizontal ? offset.Distance : 0, offset.IsHorizontal ? 0 : offset.Distance);
    }

    private void ResetTransformForCurrentPlacement()
    {
        if (_drawerTransform == null) return;
        CancelAnimations(0, 0);
        if (!IsOpen)
            SetTransform(GetPlacementOffset());
    }

    private void CloseCompleted()
    {
        _animationVersion++;
        if (_drawerTransform != null)
            CancelAnimations(_drawerTransform.X, _drawerTransform.Y);
        if (_mask != null) _mask.Visibility = Visibility.Collapsed;
        if (_drawerPanel != null) _drawerPanel.Visibility = Visibility.Collapsed;
        Visibility = Visibility.Collapsed;
    }
}
