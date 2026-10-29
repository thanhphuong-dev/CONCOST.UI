using System;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using WpfCanvas = System.Windows.Controls.Canvas;

namespace CONCOST.UI.Controls.Dashboard;

public class CardCarouselItem : ContentControl
{
    static CardCarouselItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(CardCarouselItem), new FrameworkPropertyMetadata(typeof(CardCarouselItem)));
    }

    public static readonly DependencyProperty ScaleProperty =
        DependencyProperty.Register(nameof(Scale), typeof(double), typeof(CardCarouselItem), new PropertyMetadata(1.0));

    public double Scale
    {
        get => (double)GetValue(ScaleProperty);
        set => SetValue(ScaleProperty, value);
    }

    public static readonly DependencyProperty OffsetXProperty =
        DependencyProperty.Register(nameof(OffsetX), typeof(double), typeof(CardCarouselItem), new PropertyMetadata(0.0));

    public double OffsetX
    {
        get => (double)GetValue(OffsetXProperty);
        set => SetValue(OffsetXProperty, value);
    }

    public static readonly DependencyProperty IsActiveProperty =
        DependencyProperty.Register(nameof(IsActive), typeof(bool), typeof(CardCarouselItem), new PropertyMetadata(false));

    public bool IsActive
    {
        get => (bool)GetValue(IsActiveProperty);
        set => SetValue(IsActiveProperty, value);
    }
}

[TemplatePart(Name = ElementCanvas, Type = typeof(WpfCanvas))]
[TemplatePart(Name = ElementPrevButton, Type = typeof(Button))]
[TemplatePart(Name = ElementNextButton, Type = typeof(Button))]
public class CardCarousel : ItemsControl
{
    private const string ElementCanvas = "PART_Canvas";
    private const string ElementPrevButton = "PART_PrevButton";
    private const string ElementNextButton = "PART_NextButton";

    private WpfCanvas? _canvas;
    private Button? _prevButton;
    private Button? _nextButton;
    private DispatcherTimer? _autoTimer;

    static CardCarousel()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(CardCarousel), new FrameworkPropertyMetadata(typeof(CardCarousel)));
    }

    public CardCarousel()
    {
        Loaded += (s, e) =>
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() =>
            {
                UpdateLayoutPositions(false);
                SetupAutoRun();
            }));
        };
        SizeChanged += (s, e) => UpdateLayoutPositions(false);
    }

    #region Dependency Properties

    public static readonly DependencyProperty SelectedIndexProperty =
        DependencyProperty.Register(nameof(SelectedIndex), typeof(int), typeof(CardCarousel),
            new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedIndexChanged));

    public int SelectedIndex
    {
        get => (int)GetValue(SelectedIndexProperty);
        set => SetValue(SelectedIndexProperty, value);
    }

    private static void OnSelectedIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is CardCarousel carousel)
        {
            carousel.UpdateLayoutPositions(true);
            carousel.SelectionChanged?.Invoke(carousel, new RoutedPropertyChangedEventArgs<int>((int)e.OldValue, (int)e.NewValue));
        }
    }

    public static readonly DependencyProperty ItemWidthProperty =
        DependencyProperty.Register(nameof(ItemWidth), typeof(double), typeof(CardCarousel), new PropertyMetadata(280.0, OnVisualParamChanged));

    public double ItemWidth
    {
        get => (double)GetValue(ItemWidthProperty);
        set => SetValue(ItemWidthProperty, value);
    }

    public static readonly DependencyProperty ItemHeightProperty =
        DependencyProperty.Register(nameof(ItemHeight), typeof(double), typeof(CardCarousel), new PropertyMetadata(180.0, OnVisualParamChanged));

    public double ItemHeight
    {
        get => (double)GetValue(ItemHeightProperty);
        set => SetValue(ItemHeightProperty, value);
    }

    public static readonly DependencyProperty SpacingProperty =
        DependencyProperty.Register(nameof(Spacing), typeof(double), typeof(CardCarousel), new PropertyMetadata(160.0, OnVisualParamChanged));

    public double Spacing
    {
        get => (double)GetValue(SpacingProperty);
        set => SetValue(SpacingProperty, value);
    }

    public static readonly DependencyProperty MinScaleProperty =
        DependencyProperty.Register(nameof(MinScale), typeof(double), typeof(CardCarousel), new PropertyMetadata(0.8, OnVisualParamChanged));

    public double MinScale
    {
        get => (double)GetValue(MinScaleProperty);
        set => SetValue(MinScaleProperty, value);
    }

    public static readonly DependencyProperty AutoRunProperty =
        DependencyProperty.Register(nameof(AutoRun), typeof(bool), typeof(CardCarousel), new PropertyMetadata(false, OnAutoRunChanged));

    public bool AutoRun
    {
        get => (bool)GetValue(AutoRunProperty);
        set => SetValue(AutoRunProperty, value);
    }

    public static readonly DependencyProperty IntervalProperty =
        DependencyProperty.Register(nameof(Interval), typeof(int), typeof(CardCarousel), new PropertyMetadata(3000, OnAutoRunChanged));

    public int Interval
    {
        get => (int)GetValue(IntervalProperty);
        set => SetValue(IntervalProperty, value);
    }

    private static void OnVisualParamChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is CardCarousel c) c.UpdateLayoutPositions(false);
    }

    private static void OnAutoRunChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is CardCarousel c) c.SetupAutoRun();
    }

    #endregion

    public event RoutedPropertyChangedEventHandler<int>? SelectionChanged;

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        if (_prevButton != null) _prevButton.Click -= OnPrevClick;
        if (_nextButton != null) _nextButton.Click -= OnNextClick;

        _canvas = GetTemplateChild(ElementCanvas) as WpfCanvas;
        _prevButton = GetTemplateChild(ElementPrevButton) as Button;
        _nextButton = GetTemplateChild(ElementNextButton) as Button;

        if (_prevButton != null) _prevButton.Click += OnPrevClick;
        if (_nextButton != null) _nextButton.Click += OnNextClick;

        UpdateLayoutPositions(false);
    }

    private void OnPrevClick(object sender, RoutedEventArgs e) => MovePrev();

    private void OnNextClick(object sender, RoutedEventArgs e) => MoveNext();

    public void MovePrev()
    {
        if (Items.Count == 0) return;
        SelectedIndex = (SelectedIndex - 1 + Items.Count) % Items.Count;
    }

    public void MoveNext()
    {
        if (Items.Count == 0) return;
        SelectedIndex = (SelectedIndex + 1) % Items.Count;
    }

    protected override bool IsItemItsOwnContainerOverride(object item) => item is CardCarouselItem;

    protected override DependencyObject GetContainerForItemOverride() => new CardCarouselItem();

    protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
    {
        base.PrepareContainerForItemOverride(element, item);
        if (element is CardCarouselItem cItem)
        {
            cItem.Width = ItemWidth;
            cItem.Height = ItemHeight;
            cItem.MouseLeftButtonDown += (s, e) =>
            {
                int idx = ItemContainerGenerator.IndexFromContainer(cItem);
                if (idx >= 0 && idx != SelectedIndex)
                {
                    SelectedIndex = idx;
                }
            };
        }
        UpdateLayoutPositions(false);
    }

    protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
    {
        base.OnItemsChanged(e);
        if (SelectedIndex >= Items.Count && Items.Count > 0)
        {
            SelectedIndex = Items.Count - 1;
        }
        UpdateLayoutPositions(false);
    }

    private void SetupAutoRun()
    {
        _autoTimer?.Stop();
        if (AutoRun && Interval > 0)
        {
            _autoTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(Interval) };
            _autoTimer.Tick += (s, e) =>
            {
                if (IsMouseOver) return; // Pause on hover
                MoveNext();
            };
            _autoTimer.Start();
        }
    }

    public void UpdateLayoutPositions(bool animate)
    {
        if (Items.Count == 0) return;

        if (_canvas == null)
        {
            _canvas = FindVisualChild<WpfCanvas>(this);
        }

        double canvasWidth = _canvas != null && _canvas.ActualWidth > 0 ? _canvas.ActualWidth : ActualWidth;
        double canvasHeight = _canvas != null && _canvas.ActualHeight > 0 ? _canvas.ActualHeight : ActualHeight;
        if (canvasWidth <= 0 || canvasHeight <= 0) return;

        double centerX = canvasWidth / 2.0;
        double centerY = canvasHeight / 2.0;
        int count = Items.Count;
        int current = SelectedIndex;

        for (int i = 0; i < count; i++)
        {
            if (ItemContainerGenerator.ContainerFromIndex(i) is not CardCarouselItem container) continue;

            container.Width = ItemWidth;
            container.Height = ItemHeight;

            // Calculate distance offset in circular ring
            int offset = i - current;
            // Normalize offset for shortest path in wrap
            if (offset > count / 2) offset -= count;
            if (offset < -count / 2) offset += count;

            int absOffset = Math.Abs(offset);
            bool isActive = (offset == 0);
            container.IsActive = isActive;

            // ZIndex: Center item highest, others decay
            int zIndex = 100 - absOffset * 10;
            WpfCanvas.SetZIndex(container, zIndex);

            // Calculate Target Coordinates & Scale with curved 3D depth perspective
            double scale = isActive ? 1.0 : Math.Max(MinScale, 1.0 - absOffset * 0.12);
            double opacity = isActive ? 1.0 : Math.Max(0.55, 1.0 - absOffset * 0.22);
            double targetX = centerX + Math.Sign(offset) * (Math.Abs(offset) * Spacing) - (ItemWidth / 2.0);
            double targetY = centerY - (ItemHeight / 2.0) + (absOffset * 8.0); // Slightly drop distant cards down for depth

            if (animate)
            {
                AnimateProperty(container, WpfCanvas.LeftProperty, targetX);
                AnimateProperty(container, WpfCanvas.TopProperty, targetY);
                AnimateProperty(container, UIElement.OpacityProperty, opacity);

                var scaleTransform = container.RenderTransform as ScaleTransform;
                if (scaleTransform == null)
                {
                    scaleTransform = new ScaleTransform(1, 1, ItemWidth / 2.0, ItemHeight / 2.0);
                    container.RenderTransformOrigin = new Point(0.5, 0.5);
                    container.RenderTransform = scaleTransform;
                }
                AnimateTransform(scaleTransform, ScaleTransform.ScaleXProperty, scale);
                AnimateTransform(scaleTransform, ScaleTransform.ScaleYProperty, scale);
            }
            else
            {
                WpfCanvas.SetLeft(container, targetX);
                WpfCanvas.SetTop(container, targetY);
                container.Opacity = opacity;

                var scaleTransform = container.RenderTransform as ScaleTransform;
                if (scaleTransform == null)
                {
                    scaleTransform = new ScaleTransform(scale, scale, ItemWidth / 2.0, ItemHeight / 2.0);
                    container.RenderTransformOrigin = new Point(0.5, 0.5);
                    container.RenderTransform = scaleTransform;
                }
                else
                {
                    scaleTransform.ScaleX = scale;
                    scaleTransform.ScaleY = scale;
                }
            }
        }
    }

    private void AnimateProperty(UIElement target, DependencyProperty property, double toValue)
    {
        var anim = new DoubleAnimation
        {
            To = toValue,
            Duration = TimeSpan.FromMilliseconds(350),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };
        target.BeginAnimation(property, anim);
    }

    private void AnimateTransform(ScaleTransform target, DependencyProperty property, double toValue)
    {
        var anim = new DoubleAnimation
        {
            To = toValue,
            Duration = TimeSpan.FromMilliseconds(350),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };
        target.BeginAnimation(property, anim);
    }

    private static T? FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
    {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is T typedChild) return typedChild;
            var descendant = FindVisualChild<T>(child);
            if (descendant != null) return descendant;
        }
        return null;
    }
}
