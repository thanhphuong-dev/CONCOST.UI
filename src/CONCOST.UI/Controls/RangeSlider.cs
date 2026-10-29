using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace CONCOST.UI.Controls;

/// <summary>
/// RangeSlider allows selecting a numeric sub-range [StartValue, EndValue] with dual interactive thumbs (inspired by HandyControl).
/// Essential for engineering filters, structural dimensions, cost intervals, and load limits.
/// </summary>
public class RangeSlider : Control
{
    private Thumb? _startThumb;
    private Thumb? _endThumb;
    private FrameworkElement? _track;
    private FrameworkElement? _activeTrack;

    static RangeSlider()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(RangeSlider), new FrameworkPropertyMetadata(typeof(RangeSlider)));
    }

    public static readonly DependencyProperty MinimumProperty =
        DependencyProperty.Register(nameof(Minimum), typeof(double), typeof(RangeSlider),
            new FrameworkPropertyMetadata(0.0, OnRangeChanged));

    public static readonly DependencyProperty MaximumProperty =
        DependencyProperty.Register(nameof(Maximum), typeof(double), typeof(RangeSlider),
            new FrameworkPropertyMetadata(100.0, OnRangeChanged));

    public static readonly DependencyProperty StartValueProperty =
        DependencyProperty.Register(nameof(StartValue), typeof(double), typeof(RangeSlider),
            new FrameworkPropertyMetadata(20.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValueChanged, CoerceStartValue));

    public static readonly DependencyProperty EndValueProperty =
        DependencyProperty.Register(nameof(EndValue), typeof(double), typeof(RangeSlider),
            new FrameworkPropertyMetadata(80.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValueChanged, CoerceEndValue));

    public static readonly DependencyProperty SmallChangeProperty =
        DependencyProperty.Register(nameof(SmallChange), typeof(double), typeof(RangeSlider),
            new PropertyMetadata(1.0, null, CoerceSmallChange));

    public static readonly DependencyProperty UnitProperty =
        DependencyProperty.Register(nameof(Unit), typeof(string), typeof(RangeSlider),
            new PropertyMetadata(string.Empty));

    public static readonly RoutedEvent ValueChangedEvent =
        EventManager.RegisterRoutedEvent(nameof(ValueChanged), RoutingStrategy.Bubble,
            typeof(RoutedPropertyChangedEventHandler<double>), typeof(RangeSlider));

    public event RoutedPropertyChangedEventHandler<double> ValueChanged
    {
        add => AddHandler(ValueChangedEvent, value);
        remove => RemoveHandler(ValueChangedEvent, value);
    }

    public double Minimum
    {
        get => (double)GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    public double Maximum
    {
        get => (double)GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    public double StartValue
    {
        get => (double)GetValue(StartValueProperty);
        set => SetValue(StartValueProperty, value);
    }

    public double EndValue
    {
        get => (double)GetValue(EndValueProperty);
        set => SetValue(EndValueProperty, value);
    }

    public double SmallChange
    {
        get => (double)GetValue(SmallChangeProperty);
        set => SetValue(SmallChangeProperty, value);
    }

    public string Unit
    {
        get => (string)GetValue(UnitProperty);
        set => SetValue(UnitProperty, value);
    }

    public override void OnApplyTemplate()
    {
        if (_startThumb != null) _startThumb.DragDelta -= OnStartThumbDragDelta;
        if (_endThumb != null) _endThumb.DragDelta -= OnEndThumbDragDelta;
        base.OnApplyTemplate();

        _startThumb = GetTemplateChild("PART_StartThumb") as Thumb;
        _endThumb = GetTemplateChild("PART_EndThumb") as Thumb;
        _track = GetTemplateChild("PART_Track") as FrameworkElement;
        _activeTrack = GetTemplateChild("PART_ActiveTrack") as FrameworkElement;

        if (_startThumb != null)
        {
            _startThumb.DragDelta += OnStartThumbDragDelta;
        }

        if (_endThumb != null)
        {
            _endThumb.DragDelta += OnEndThumbDragDelta;
        }

        SizeChanged -= OnSliderSizeChanged;
        SizeChanged += OnSliderSizeChanged;
        UpdateThumbPositions();
    }

    private void OnSliderSizeChanged(object sender, SizeChangedEventArgs e) => UpdateThumbPositions();

    private static object CoerceSmallChange(DependencyObject d, object value)
    {
        var number = (double)value;
        return NumericCompatibility.IsFinite(number) && number > 0 ? number : 1d;
    }

    private static object CoerceStartValue(DependencyObject d, object value)
    {
        var slider = (RangeSlider)d;
        var number = (double)value;
        if (!NumericCompatibility.IsFinite(number)) number = slider.Minimum;
        double maxAllowed = Math.Max(slider.Minimum, slider.EndValue);
        return NumericCompatibility.Clamp(number, slider.Minimum, maxAllowed);
    }

    private static object CoerceEndValue(DependencyObject d, object value)
    {
        var slider = (RangeSlider)d;
        var number = (double)value;
        if (!NumericCompatibility.IsFinite(number)) number = slider.Maximum;
        double minAllowed = Math.Min(slider.Maximum, slider.StartValue);
        return NumericCompatibility.Clamp(number, minAllowed, slider.Maximum);
    }

    private static void OnRangeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is RangeSlider slider)
        {
            if (e.Property == StartValueProperty && slider.StartValue > slider.EndValue)
                slider.SetCurrentValue(EndValueProperty, NumericCompatibility.Clamp(slider.StartValue, slider.Minimum, slider.Maximum));
            else if (e.Property == EndValueProperty && slider.EndValue < slider.StartValue)
                slider.SetCurrentValue(StartValueProperty, NumericCompatibility.Clamp(slider.EndValue, slider.Minimum, slider.Maximum));
            if (e.Property == MinimumProperty && slider.Maximum < slider.Minimum)
                slider.SetCurrentValue(MaximumProperty, slider.Minimum);
            else if (e.Property == MaximumProperty && slider.Minimum > slider.Maximum)
                slider.SetCurrentValue(MinimumProperty, slider.Maximum);
            if (e.Property == MinimumProperty || e.Property == MaximumProperty)
            {
                slider.SetCurrentValue(StartValueProperty, NumericCompatibility.Clamp(slider.StartValue, slider.Minimum, slider.Maximum));
                slider.SetCurrentValue(EndValueProperty, NumericCompatibility.Clamp(slider.EndValue, slider.Minimum, slider.Maximum));
                if (slider.StartValue > slider.EndValue)
                    slider.SetCurrentValue(StartValueProperty, slider.EndValue);
            }
            if (e.Property == MinimumProperty || e.Property == MaximumProperty)
            {
                slider.SetCurrentValue(StartValueProperty, NumericCompatibility.Clamp(slider.StartValue, slider.Minimum, slider.Maximum));
                slider.SetCurrentValue(EndValueProperty, NumericCompatibility.Clamp(slider.EndValue, slider.Minimum, slider.Maximum));
                if (slider.StartValue > slider.EndValue)
                    slider.SetCurrentValue(StartValueProperty, slider.EndValue);
            }
            slider.CoerceValue(StartValueProperty);
            slider.CoerceValue(EndValueProperty);
            slider.UpdateThumbPositions();
        }
    }

    private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is RangeSlider slider)
        {
            if (e.Property == StartValueProperty)
            {
                slider.CoerceValue(EndValueProperty);
            }
            else if (e.Property == EndValueProperty)
            {
                slider.CoerceValue(StartValueProperty);
            }
            slider.UpdateThumbPositions();
            var args = new RoutedPropertyChangedEventArgs<double>((double)e.OldValue, (double)e.NewValue)
            {
                RoutedEvent = ValueChangedEvent
            };
            slider.RaiseEvent(args);
        }
    }

    protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
    {
        base.OnPreviewMouseDown(e);
        if (_track != null && _track.ActualWidth > 0 && e.ChangedButton == MouseButton.Left)
        {
            Point pt = e.GetPosition(_track);
            double range = Maximum - Minimum;
            if (range > 0)
            {
                double clickValue = Minimum + (pt.X / _track.ActualWidth) * range;
                if (SmallChange > 0)
                {
                    clickValue = Math.Round(clickValue / SmallChange) * SmallChange;
                }
                double distToStart = Math.Abs(clickValue - StartValue);
                double distToEnd = Math.Abs(clickValue - EndValue);

                if (distToStart <= distToEnd)
                {
                    StartValue = Math.Min(EndValue, Math.Max(Minimum, clickValue));
                }
                else
                {
                    EndValue = Math.Max(StartValue, Math.Min(Maximum, clickValue));
                }
            }
        }
    }

    private void OnStartThumbDragDelta(object sender, DragDeltaEventArgs e)
    {
        if (_track == null || _track.ActualWidth <= 0) return;

        double deltaValue = (e.HorizontalChange / _track.ActualWidth) * (Maximum - Minimum);
        double newVal = StartValue + deltaValue;
        if (SmallChange > 0)
        {
            newVal = Math.Round(newVal / SmallChange) * SmallChange;
        }
        StartValue = Math.Max(Minimum, Math.Min(EndValue, newVal));
    }

    private void OnEndThumbDragDelta(object sender, DragDeltaEventArgs e)
    {
        if (_track == null || _track.ActualWidth <= 0) return;

        double deltaValue = (e.HorizontalChange / _track.ActualWidth) * (Maximum - Minimum);
        double newVal = EndValue + deltaValue;
        if (SmallChange > 0)
        {
            newVal = Math.Round(newVal / SmallChange) * SmallChange;
        }
        EndValue = Math.Max(StartValue, Math.Min(Maximum, newVal));
    }

    private void UpdateThumbPositions()
    {
        if (_track == null || _track.ActualWidth <= 0 || _startThumb == null || _endThumb == null) return;

        double range = Maximum - Minimum;
        if (range <= 0 || !NumericCompatibility.IsFinite(range)) return;

        double width = _track.ActualWidth;
        double startPos = NumericCompatibility.Clamp(((StartValue - Minimum) / range) * width, 0, width);
        double endPos = NumericCompatibility.Clamp(((EndValue - Minimum) / range) * width, startPos, width);

        _startThumb.Margin = new Thickness(Math.Max(0, startPos - 8), 0, 0, 0);
        _endThumb.Margin = new Thickness(Math.Max(0, endPos - 8), 0, 0, 0);

        if (_activeTrack != null)
        {
            _activeTrack.Margin = new Thickness(startPos, 0, 0, 0);
            _activeTrack.Width = Math.Max(0, endPos - startPos);
        }
    }
}
