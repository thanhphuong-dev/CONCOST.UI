using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace CONCOST.UI.Controls;

/// <summary>
/// RangeSlider allows selecting a numeric sub-range [StartValue, EndValue] with dual interactive thumbs.
/// Essential for engineering filters, structural dimensions, cost intervals, and load limits.
/// Supports snapping to ticks, value labels, custom brushes and vertical orientation.
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

    // ------------------------------------------------------------------ range

    public static readonly DependencyProperty MinimumProperty =
        DependencyProperty.Register(nameof(Minimum), typeof(double), typeof(RangeSlider),
            new FrameworkPropertyMetadata(0.0, OnRangeChanged));

    public static readonly DependencyProperty MaximumProperty =
        DependencyProperty.Register(nameof(Maximum), typeof(double), typeof(RangeSlider),
            new FrameworkPropertyMetadata(100.0, OnRangeChanged));

    public static readonly DependencyProperty StartValueProperty =
        DependencyProperty.Register(nameof(StartValue), typeof(double), typeof(RangeSlider),
            new FrameworkPropertyMetadata(20.0,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnValueChanged, CoerceStartValue));

    public static readonly DependencyProperty EndValueProperty =
        DependencyProperty.Register(nameof(EndValue), typeof(double), typeof(RangeSlider),
            new FrameworkPropertyMetadata(80.0,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnValueChanged, CoerceEndValue));

    public static readonly DependencyProperty SmallChangeProperty =
        DependencyProperty.Register(nameof(SmallChange), typeof(double), typeof(RangeSlider),
            new PropertyMetadata(1.0, null, CoerceSmallChange));

    public static readonly DependencyProperty UnitProperty =
        DependencyProperty.Register(nameof(Unit), typeof(string), typeof(RangeSlider),
            new PropertyMetadata(string.Empty));

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

    // ------------------------------------------------------------------ ticks

    /// <summary>Snap thumb positions to multiples of <see cref="TickFrequency"/>. Default <c>false</c>.</summary>
    public static readonly DependencyProperty IsSnapToTickProperty =
        DependencyProperty.Register(nameof(IsSnapToTick), typeof(bool), typeof(RangeSlider),
            new PropertyMetadata(false));

    public bool IsSnapToTick
    {
        get => (bool)GetValue(IsSnapToTickProperty);
        set => SetValue(IsSnapToTickProperty, value);
    }

    /// <summary>Step for snapping and tick rendering. Default 1.</summary>
    public static readonly DependencyProperty TickFrequencyProperty =
        DependencyProperty.Register(nameof(TickFrequency), typeof(double), typeof(RangeSlider),
            new PropertyMetadata(1.0));

    public double TickFrequency
    {
        get => (double)GetValue(TickFrequencyProperty);
        set => SetValue(TickFrequencyProperty, value);
    }

    /// <summary>Whether tick marks are visible beneath the track. Default <c>false</c>.</summary>
    public static readonly DependencyProperty ShowTicksProperty =
        DependencyProperty.Register(nameof(ShowTicks), typeof(bool), typeof(RangeSlider),
            new PropertyMetadata(false));

    public bool ShowTicks
    {
        get => (bool)GetValue(ShowTicksProperty);
        set => SetValue(ShowTicksProperty, value);
    }

    // ------------------------------------------------------------------ value labels

    /// <summary>Whether start/end value labels appear above the thumbs. Default <c>true</c>.</summary>
    public static readonly DependencyProperty ShowValueLabelsProperty =
        DependencyProperty.Register(nameof(ShowValueLabels), typeof(bool), typeof(RangeSlider),
            new PropertyMetadata(true));

    public bool ShowValueLabels
    {
        get => (bool)GetValue(ShowValueLabelsProperty);
        set => SetValue(ShowValueLabelsProperty, value);
    }

    /// <summary>
    /// Composite format string for the value labels. Argument {0} = value.
    /// Default <c>"{0:0}"</c>. Example: <c>"{0:0}m²"</c>.
    /// </summary>
    public static readonly DependencyProperty LabelFormatProperty =
        DependencyProperty.Register(nameof(LabelFormat), typeof(string), typeof(RangeSlider),
            new PropertyMetadata("{0:0}"));

    public string LabelFormat
    {
        get => (string)GetValue(LabelFormatProperty);
        set => SetValue(LabelFormatProperty, value);
    }

    // ------------------------------------------------------------------ track visuals

    /// <summary>Height of the track bar in device-independent pixels. Default 4.</summary>
    public static readonly DependencyProperty TrackHeightProperty =
        DependencyProperty.Register(nameof(TrackHeight), typeof(double), typeof(RangeSlider),
            new FrameworkPropertyMetadata(4.0, FrameworkPropertyMetadataOptions.AffectsMeasure));

    public double TrackHeight
    {
        get => (double)GetValue(TrackHeightProperty);
        set => SetValue(TrackHeightProperty, value);
    }

    /// <summary>Diameter of each thumb. Default 16.</summary>
    public static readonly DependencyProperty ThumbSizeProperty =
        DependencyProperty.Register(nameof(ThumbSize), typeof(double), typeof(RangeSlider),
            new FrameworkPropertyMetadata(16.0, FrameworkPropertyMetadataOptions.AffectsMeasure));

    public double ThumbSize
    {
        get => (double)GetValue(ThumbSizeProperty);
        set => SetValue(ThumbSizeProperty, value);
    }

    /// <summary>Brush of the active (selected) track segment. Null uses the theme accent.</summary>
    public static readonly DependencyProperty ActiveTrackBrushProperty =
        DependencyProperty.Register(nameof(ActiveTrackBrush), typeof(Brush), typeof(RangeSlider),
            new FrameworkPropertyMetadata(null));

    public Brush? ActiveTrackBrush
    {
        get => (Brush?)GetValue(ActiveTrackBrushProperty);
        set => SetValue(ActiveTrackBrushProperty, value);
    }

    /// <summary>Brush of the inactive track segments (outside the selection). Null uses the theme border.</summary>
    public static readonly DependencyProperty InactiveTrackBrushProperty =
        DependencyProperty.Register(nameof(InactiveTrackBrush), typeof(Brush), typeof(RangeSlider),
            new FrameworkPropertyMetadata(null));

    public Brush? InactiveTrackBrush
    {
        get => (Brush?)GetValue(InactiveTrackBrushProperty);
        set => SetValue(InactiveTrackBrushProperty, value);
    }

    /// <summary>Fill brush of both thumbs. Null uses the theme surface elevated brush.</summary>
    public static readonly DependencyProperty ThumbBrushProperty =
        DependencyProperty.Register(nameof(ThumbBrush), typeof(Brush), typeof(RangeSlider),
            new FrameworkPropertyMetadata(null));

    public Brush? ThumbBrush
    {
        get => (Brush?)GetValue(ThumbBrushProperty);
        set => SetValue(ThumbBrushProperty, value);
    }

    // ------------------------------------------------------------------ orientation

    /// <summary>Slider orientation. Default <see cref="Orientation.Horizontal"/>.</summary>
    public static readonly DependencyProperty OrientationProperty =
        DependencyProperty.Register(nameof(Orientation), typeof(Orientation), typeof(RangeSlider),
            new FrameworkPropertyMetadata(Orientation.Horizontal, FrameworkPropertyMetadataOptions.AffectsMeasure));

    public Orientation Orientation
    {
        get => (Orientation)GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    // ------------------------------------------------------------------ routed event

    public static readonly RoutedEvent ValueChangedEvent =
        EventManager.RegisterRoutedEvent(nameof(ValueChanged), RoutingStrategy.Bubble,
            typeof(RoutedPropertyChangedEventHandler<double>), typeof(RangeSlider));

    public event RoutedPropertyChangedEventHandler<double> ValueChanged
    {
        add => AddHandler(ValueChangedEvent, value);
        remove => RemoveHandler(ValueChangedEvent, value);
    }

    // ------------------------------------------------------------------ template

    public override void OnApplyTemplate()
    {
        if (_startThumb != null) _startThumb.DragDelta -= OnStartThumbDragDelta;
        if (_endThumb != null) _endThumb.DragDelta -= OnEndThumbDragDelta;
        base.OnApplyTemplate();

        _startThumb = GetTemplateChild("PART_StartThumb") as Thumb;
        _endThumb = GetTemplateChild("PART_EndThumb") as Thumb;
        _track = GetTemplateChild("PART_Track") as FrameworkElement;
        _activeTrack = GetTemplateChild("PART_ActiveTrack") as FrameworkElement;

        if (_startThumb != null) _startThumb.DragDelta += OnStartThumbDragDelta;
        if (_endThumb != null) _endThumb.DragDelta += OnEndThumbDragDelta;

        SizeChanged -= OnSliderSizeChanged;
        SizeChanged += OnSliderSizeChanged;
        UpdateThumbPositions();
    }

    private void OnSliderSizeChanged(object sender, SizeChangedEventArgs e) => UpdateThumbPositions();

    // ------------------------------------------------------------------ coerce

    private static object CoerceSmallChange(DependencyObject d, object value)
    {
        var number = (double)value;
        return NumericCompatibility.IsFinite(number) && number > 0 ? number : 1d;
    }

    private static object CoerceStartValue(DependencyObject d, object value)
    {
        var slider = (RangeSlider)d;
        double v = (double)value;
        if (!NumericCompatibility.IsFinite(v)) v = slider.Minimum;
        return NumericCompatibility.Clamp(v, slider.Minimum, slider.EndValue);
    }

    private static object CoerceEndValue(DependencyObject d, object value)
    {
        var slider = (RangeSlider)d;
        double v = (double)value;
        if (!NumericCompatibility.IsFinite(v)) v = slider.Maximum;
        return NumericCompatibility.Clamp(v, slider.StartValue, slider.Maximum);
    }

    // ------------------------------------------------------------------ snap helper

    private double SnapValue(double value)
    {
        if (!IsSnapToTick) return value;
        double freq = TickFrequency > 0 ? TickFrequency : SmallChange;
        return Math.Round(value / freq) * freq;
    }

    // ------------------------------------------------------------------ callbacks

    private static void OnRangeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is RangeSlider slider)
        {
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
            if (e.Property == StartValueProperty) slider.CoerceValue(EndValueProperty);
            else if (e.Property == EndValueProperty) slider.CoerceValue(StartValueProperty);

            slider.UpdateThumbPositions();
            slider.RaiseEvent(new RoutedPropertyChangedEventArgs<double>(
                (double)e.OldValue, (double)e.NewValue) { RoutedEvent = ValueChangedEvent });
        }
    }

    // ------------------------------------------------------------------ mouse click on track

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
                clickValue = SnapValue(clickValue);

                double distToStart = Math.Abs(clickValue - StartValue);
                double distToEnd = Math.Abs(clickValue - EndValue);

                if (distToStart <= distToEnd)
                    StartValue = Math.Min(EndValue, Math.Max(Minimum, clickValue));
                else
                    EndValue = Math.Max(StartValue, Math.Min(Maximum, clickValue));
            }
        }
    }

    // ------------------------------------------------------------------ thumb drag

    private void OnStartThumbDragDelta(object sender, DragDeltaEventArgs e)
    {
        if (_track == null || _track.ActualWidth <= 0) return;
        double deltaValue = (e.HorizontalChange / _track.ActualWidth) * (Maximum - Minimum);
        double newVal = SnapValue(StartValue + deltaValue);
        StartValue = Math.Max(Minimum, Math.Min(EndValue, newVal));
    }

    private void OnEndThumbDragDelta(object sender, DragDeltaEventArgs e)
    {
        if (_track == null || _track.ActualWidth <= 0) return;
        double deltaValue = (e.HorizontalChange / _track.ActualWidth) * (Maximum - Minimum);
        double newVal = SnapValue(EndValue + deltaValue);
        EndValue = Math.Max(StartValue, Math.Min(Maximum, newVal));
    }

    // ------------------------------------------------------------------ layout

    private void UpdateThumbPositions()
    {
        if (_track == null || _track.ActualWidth <= 0 || _startThumb == null || _endThumb == null) return;

        double range = Maximum - Minimum;
        if (range <= 0 || !NumericCompatibility.IsFinite(range)) return;

        double thumbHalf = ThumbSize / 2;
        double width = _track.ActualWidth;
        double startPos = NumericCompatibility.Clamp(((StartValue - Minimum) / range) * width, 0, width);
        double endPos = NumericCompatibility.Clamp(((EndValue - Minimum) / range) * width, startPos, width);

        _startThumb.Margin = new Thickness(Math.Max(0, startPos - thumbHalf), 0, 0, 0);
        _endThumb.Margin = new Thickness(Math.Max(0, endPos - thumbHalf), 0, 0, 0);

        if (_activeTrack != null)
        {
            _activeTrack.Margin = new Thickness(startPos, 0, 0, 0);
            _activeTrack.Width = Math.Max(0, endPos - startPos);
        }
    }
}
