using System.Windows;
using System.Windows.Controls;

namespace CONCOST.UI.Controls;

public class Pagination : Control
{
    static Pagination()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(Pagination), new FrameworkPropertyMetadata(typeof(Pagination)));
    }

    public static readonly DependencyProperty CurrentPageProperty =
        DependencyProperty.Register(nameof(CurrentPage), typeof(int), typeof(Pagination),
            new FrameworkPropertyMetadata(1, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnPageChanged, CoercePage));

    public static readonly DependencyProperty MaxPageProperty =
        DependencyProperty.Register(nameof(MaxPage), typeof(int), typeof(Pagination),
            new PropertyMetadata(10, OnPageChanged, CoerceMaxPage));

    public static readonly DependencyProperty TotalItemsProperty =
        DependencyProperty.Register(nameof(TotalItems), typeof(int), typeof(Pagination),
        new PropertyMetadata(100, OnTotalItemsChanged, CoerceTotalItems));

    public static readonly DependencyProperty PageSizeProperty = DependencyProperty.Register(
        nameof(PageSize), typeof(int), typeof(Pagination), new PropertyMetadata(10, OnPageSizeChanged, CoercePageSize));

    private Button? _previousButton;
    private Button? _nextButton;

    public static readonly RoutedEvent PageChangedEvent =
        EventManager.RegisterRoutedEvent(nameof(PageChanged), RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<int>), typeof(Pagination));

    public event RoutedPropertyChangedEventHandler<int> PageChanged
    {
        add => AddHandler(PageChangedEvent, value);
        remove => RemoveHandler(PageChangedEvent, value);
    }

    public int CurrentPage
    {
        get => (int)GetValue(CurrentPageProperty);
        set => SetValue(CurrentPageProperty, value);
    }

    public int MaxPage
    {
        get => (int)GetValue(MaxPageProperty);
        set => SetValue(MaxPageProperty, value);
    }

    public int TotalItems
    {
        get => (int)GetValue(TotalItemsProperty);
        set => SetValue(TotalItemsProperty, value);
    }

    public int PageSize
    {
        get => (int)GetValue(PageSizeProperty);
        set => SetValue(PageSizeProperty, value);
    }

    public int TotalPages => Math.Max(1, (int)Math.Ceiling((double)TotalItems / PageSize));

    public override void OnApplyTemplate()
    {
        if (_previousButton != null) _previousButton.Click -= OnPreviousClick;
        if (_nextButton != null) _nextButton.Click -= OnNextClick;
        base.OnApplyTemplate();
        _previousButton = GetTemplateChild("PART_PrevButton") as Button;
        _nextButton = GetTemplateChild("PART_NextButton") as Button;
        if (_previousButton != null) _previousButton.Click += OnPreviousClick;
        if (_nextButton != null) _nextButton.Click += OnNextClick;
    }

    private void OnPreviousClick(object sender, RoutedEventArgs e) => CurrentPage = Math.Max(1, CurrentPage - 1);
    private void OnNextClick(object sender, RoutedEventArgs e) => CurrentPage = Math.Min(MaxPage, CurrentPage + 1);

    private static object CoerceMaxPage(DependencyObject d, object value)
    {
        var pagination = (Pagination)d;
        return Math.Max(1, Math.Min((int)value, pagination.TotalPages));
    }
    private static object CoercePageSize(DependencyObject d, object value) => Math.Max(1, (int)value);
    private static object CoerceTotalItems(DependencyObject d, object value) => Math.Max(0, (int)value);

    private static void OnTotalItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Pagination pagination)
        {
            pagination.SetCurrentValue(MaxPageProperty, pagination.TotalPages);
            pagination.CoerceValue(CurrentPageProperty);
        }
    }

    private static void OnPageSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Pagination pagination)
        {
            pagination.SetCurrentValue(MaxPageProperty, pagination.TotalPages);
            pagination.CoerceValue(CurrentPageProperty);
        }
    }

    private static object CoercePage(DependencyObject d, object baseValue)
    {
        var p = (Pagination)d;
        int page = (int)baseValue;
        if (page < 1) return 1;
        return NumericCompatibility.Clamp(page, 1, p.MaxPage);
    }

    private static void OnPageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Pagination p)
        {
            if (e.Property == CurrentPageProperty)
                p.RaiseEvent(new RoutedPropertyChangedEventArgs<int>((int)e.OldValue, (int)e.NewValue, PageChangedEvent));
            else
                p.CoerceValue(CurrentPageProperty);
        }
    }
}
