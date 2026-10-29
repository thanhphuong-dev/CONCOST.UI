using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace CONCOST.UI.Controls;

public class CalendarWithClock : Control
{
    private Clock? _clock;
    private System.Windows.Controls.Calendar? _calendar;
    private Button? _nowButton;
    private Button? _confirmButton;

    static CalendarWithClock()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(CalendarWithClock), new FrameworkPropertyMetadata(typeof(CalendarWithClock)));
    }

    public static readonly DependencyProperty SelectedDateTimeProperty =
        DependencyProperty.Register(nameof(SelectedDateTime), typeof(DateTime), typeof(CalendarWithClock),
            new FrameworkPropertyMetadata(DateTime.Now, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedDateTimeChanged));

    public static readonly DependencyProperty ShowSecondsProperty =
        DependencyProperty.Register(nameof(ShowSeconds), typeof(bool), typeof(CalendarWithClock),
            new PropertyMetadata(false));

    public DateTime SelectedDateTime
    {
        get => (DateTime)GetValue(SelectedDateTimeProperty);
        set => SetValue(SelectedDateTimeProperty, value);
    }

    public bool ShowSeconds
    {
        get => (bool)GetValue(ShowSecondsProperty);
        set => SetValue(ShowSecondsProperty, value);
    }

    private static void OnSelectedDateTimeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is CalendarWithClock cwc && e.NewValue is DateTime dt)
        {
            if (cwc._calendar != null && cwc._calendar.SelectedDate != dt.Date)
            {
                cwc._calendar.SelectedDate = dt.Date;
            }
            if (cwc._clock != null && cwc._clock.SelectedTime != dt)
            {
                cwc._clock.SelectedTime = dt;
            }
        }
    }

    public override void OnApplyTemplate()
    {
        if (_calendar != null) _calendar.SelectedDatesChanged -= OnCalendarSelectionChanged;
        if (_nowButton != null) _nowButton.Click -= OnNowClick;
        if (_confirmButton != null) _confirmButton.Click -= OnConfirmClick;
        base.OnApplyTemplate();
        _calendar = GetTemplateChild("PART_Calendar") as System.Windows.Controls.Calendar;
        _clock = GetTemplateChild("PART_Clock") as Clock;

        if (_calendar != null)
        {
            _calendar.SelectedDate = SelectedDateTime.Date;
            _calendar.SelectedDatesChanged += OnCalendarSelectionChanged;
        }

        if (_clock != null)
        {
            _clock.SelectedTime = SelectedDateTime;
            _clock.ShowSeconds = ShowSeconds;
            _clock.SelectedTimeChanged -= OnClockTimeChanged;
            _clock.SelectedTimeChanged += OnClockTimeChanged;
        }

        _nowButton = GetTemplateChild("PART_NowButton") as Button;
        _confirmButton = GetTemplateChild("PART_ConfirmButton") as Button;
        if (_nowButton != null) _nowButton.Click += OnNowClick;
        if (_confirmButton != null) _confirmButton.Click += OnConfirmClick;
    }

    private void OnCalendarSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_calendar?.SelectedDate is DateTime date)
            SelectedDateTime = new DateTime(date.Year, date.Month, date.Day, SelectedDateTime.Hour, SelectedDateTime.Minute, SelectedDateTime.Second);
    }

    private void OnClockTimeChanged(object? sender, RoutedPropertyChangedEventArgs<DateTime> e)
    {
        var time = e.NewValue;
        SelectedDateTime = new DateTime(SelectedDateTime.Year, SelectedDateTime.Month, SelectedDateTime.Day, time.Hour, time.Minute, time.Second);
    }

    private void OnNowClick(object sender, RoutedEventArgs e) => SelectedDateTime = DateTime.Now;
    private void OnConfirmClick(object sender, RoutedEventArgs e) => RaiseEvent(new RoutedEventArgs(ConfirmedEvent));

    public static readonly RoutedEvent ConfirmedEvent = EventManager.RegisterRoutedEvent(
        nameof(Confirmed), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(CalendarWithClock));

    public event RoutedEventHandler Confirmed
    {
        add => AddHandler(ConfirmedEvent, value);
        remove => RemoveHandler(ConfirmedEvent, value);
    }
}
