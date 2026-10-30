using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace CONCOST.UI.Controls;

public enum ClockDisplayMode
{
    Hour,
    Minute,
    Second
}

[TemplatePart(Name = "PART_Canvas", Type = typeof(System.Windows.Controls.Canvas))]
public class Clock : Control
{
    private System.Windows.Controls.Canvas? _canvas;
    private bool _isDragging;

    static Clock()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(Clock), new FrameworkPropertyMetadata(typeof(Clock)));
    }

    public static readonly DependencyProperty SelectedTimeProperty =
        DependencyProperty.Register(nameof(SelectedTime), typeof(DateTime), typeof(Clock),
            new FrameworkPropertyMetadata(DateTime.Now, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedTimeChanged));

    public static readonly DependencyProperty DisplayModeProperty =
        DependencyProperty.Register(nameof(DisplayMode), typeof(ClockDisplayMode), typeof(Clock),
            new FrameworkPropertyMetadata(ClockDisplayMode.Hour, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnDisplayModeChanged));

    public static readonly DependencyProperty ShowSecondsProperty =
        DependencyProperty.Register(nameof(ShowSeconds), typeof(bool), typeof(Clock),
            new PropertyMetadata(false, OnShowSecondsChanged));

    public static readonly RoutedEvent SelectedTimeChangedEvent = EventManager.RegisterRoutedEvent(
        nameof(SelectedTimeChanged), RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<DateTime>), typeof(Clock));

    public DateTime SelectedTime
    {
        get => (DateTime)GetValue(SelectedTimeProperty);
        set => SetValue(SelectedTimeProperty, value);
    }

    public ClockDisplayMode DisplayMode
    {
        get => (ClockDisplayMode)GetValue(DisplayModeProperty);
        set => SetValue(DisplayModeProperty, value);
    }

    public bool ShowSeconds
    {
        get => (bool)GetValue(ShowSecondsProperty);
        set => SetValue(ShowSecondsProperty, value);
    }

    public string FormattedTime => SelectedTime.ToString(ShowSeconds ? "HH:mm:ss" : "HH:mm");

    public event RoutedPropertyChangedEventHandler<DateTime> SelectedTimeChanged
    {
        add => AddHandler(SelectedTimeChangedEvent, value);
        remove => RemoveHandler(SelectedTimeChangedEvent, value);
    }

    private static void OnSelectedTimeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Clock clock)
        {
            clock.RaiseEvent(new RoutedPropertyChangedEventArgs<DateTime>((DateTime)e.OldValue, (DateTime)e.NewValue, SelectedTimeChangedEvent));
            clock.UpdateClockVisuals();
        }
    }

    private static void OnDisplayModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Clock clock)
        {
            clock.UpdateClockVisuals();
        }
    }

    private static void OnShowSecondsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Clock clock)
        {
            clock.UpdateClockVisuals();
        }
    }

    public override void OnApplyTemplate()
    {
        if (_canvas != null)
        {
            _canvas.MouseLeftButtonDown -= OnCanvasMouseDown;
            _canvas.MouseMove -= OnCanvasMouseMove;
            _canvas.MouseLeftButtonUp -= OnCanvasMouseUp;
            _canvas.Loaded -= OnCanvasLoaded;
            _canvas.SizeChanged -= OnCanvasSizeChanged;
        }
        base.OnApplyTemplate();
        _canvas = GetTemplateChild("PART_Canvas") as System.Windows.Controls.Canvas;
        if (_canvas != null)
        {
            _canvas.MouseLeftButtonDown += OnCanvasMouseDown;
            _canvas.MouseMove += OnCanvasMouseMove;
            _canvas.MouseLeftButtonUp += OnCanvasMouseUp;
            _canvas.Loaded += OnCanvasLoaded;
            _canvas.SizeChanged += OnCanvasSizeChanged;
        }
        UpdateClockVisuals();
    }

    private void OnCanvasLoaded(object sender, RoutedEventArgs e) => UpdateClockVisuals();
    private void OnCanvasSizeChanged(object sender, SizeChangedEventArgs e) => UpdateClockVisuals();

    private void OnCanvasMouseDown(object sender, MouseButtonEventArgs e)
    {
        _isDragging = true;
        _canvas?.CaptureMouse();
        UpdatePointerFromPoint(e.GetPosition(_canvas));
    }

    private void OnCanvasMouseMove(object sender, MouseEventArgs e)
    {
        if (_isDragging && _canvas != null)
        {
            UpdatePointerFromPoint(e.GetPosition(_canvas));
        }
    }

    private void OnCanvasMouseUp(object sender, MouseButtonEventArgs e)
    {
        if (_isDragging)
        {
            _isDragging = false;
            _canvas?.ReleaseMouseCapture();

            // Auto-advance mode: Hour -> Minute
            if (DisplayMode == ClockDisplayMode.Hour)
            {
                DisplayMode = ClockDisplayMode.Minute;
            }
            else if (DisplayMode == ClockDisplayMode.Minute && ShowSeconds)
            {
                DisplayMode = ClockDisplayMode.Second;
            }
        }
    }

    private void UpdatePointerFromPoint(Point p)
    {
        if (_canvas == null) return;
        double w = _canvas.ActualWidth > 0 ? _canvas.ActualWidth : 220;
        double h = _canvas.ActualHeight > 0 ? _canvas.ActualHeight : 220;
        double cx = w / 2.0;
        double cy = h / 2.0;

        double dx = p.X - cx;
        double dy = p.Y - cy;
        double angle = Math.Atan2(dy, dx) * (180.0 / Math.PI) + 90.0;
        if (angle < 0) angle += 360.0;

        var cur = SelectedTime;
        if (DisplayMode == ClockDisplayMode.Hour)
        {
            int hour = (int)Math.Round(angle / 30.0) % 12;
            if (hour == 0) hour = 12;
            SelectedTime = new DateTime(cur.Year, cur.Month, cur.Day, hour, cur.Minute, cur.Second);
        }
        else if (DisplayMode == ClockDisplayMode.Minute)
        {
            int minute = (int)Math.Round(angle / 6.0) % 60;
            SelectedTime = new DateTime(cur.Year, cur.Month, cur.Day, cur.Hour, minute, cur.Second);
        }
        else if (DisplayMode == ClockDisplayMode.Second)
        {
            int sec = (int)Math.Round(angle / 6.0) % 60;
            SelectedTime = new DateTime(cur.Year, cur.Month, cur.Day, cur.Hour, cur.Minute, sec);
        }
    }

    public void UpdateClockVisuals()
    {
        if (_canvas == null) return;
        _canvas.Children.Clear();

        double w = _canvas.ActualWidth > 0 ? _canvas.ActualWidth : 220;
        double h = _canvas.ActualHeight > 0 ? _canvas.ActualHeight : 220;
        double size = Math.Min(w, h);
        double cx = w / 2.0;
        double cy = h / 2.0;
        double radius = size * 0.40;

        // Draw numbers (12 positions: 1..12 for Hour, 5,10,15..00 for Min/Sec)
        for (int i = 1; i <= 12; i++)
        {
            int val;
            string text;
            bool isSelected;

            if (DisplayMode == ClockDisplayMode.Hour)
            {
                val = i;
                text = i.ToString();
                int currentHour = SelectedTime.Hour % 12;
                if (currentHour == 0) currentHour = 12;
                isSelected = (val == currentHour);
            }
            else
            {
                val = (i * 5) % 60;
                text = val == 0 ? "00" : val.ToString();
                int currentUnit = DisplayMode == ClockDisplayMode.Minute ? SelectedTime.Minute : SelectedTime.Second;
                isSelected = (Math.Abs(currentUnit - val) < 3) || (val == 0 && currentUnit >= 58);
            }

            double ang = (i * 30.0 - 90.0) * (Math.PI / 180.0);
            double nx = cx + radius * Math.Cos(ang);
            double ny = cy + radius * Math.Sin(ang);

            var tb = new TextBlock
            {
                Text = text,
                FontSize = isSelected ? 12 : 11.5,
                FontWeight = isSelected ? FontWeights.Bold : FontWeights.Normal,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            if (isSelected)
            {
                tb.Foreground = Brushes.White;
            }
            else
            {
                tb.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextPrimary");
            }

            var numBorder = new Border
            {
                Width = 26,
                Height = 26,
                CornerRadius = new CornerRadius(13),
                Child = tb
            };

            if (isSelected)
            {
                numBorder.SetResourceReference(Border.BackgroundProperty, "CONCOST.Brush.Primary");
            }
            else
            {
                numBorder.Background = Brushes.Transparent;
            }

            System.Windows.Controls.Canvas.SetLeft(numBorder, nx - 13);
            System.Windows.Controls.Canvas.SetTop(numBorder, ny - 13);
            _canvas.Children.Add(numBorder);
        }

        // Draw pointer hand
        double handAngle = DisplayMode switch
        {
            ClockDisplayMode.Hour => (SelectedTime.Hour % 12 * 30.0 + SelectedTime.Minute * 0.5),
            ClockDisplayMode.Minute => SelectedTime.Minute * 6.0,
            _ => SelectedTime.Second * 6.0
        };

        // Pointer container
        var handCanvas = new System.Windows.Controls.Canvas
        {
            Width = w,
            Height = h
        };
        System.Windows.Controls.Canvas.SetLeft(handCanvas, 0);
        System.Windows.Controls.Canvas.SetTop(handCanvas, 0);

        // Hand Line
        double radAngle = (handAngle - 90.0) * (Math.PI / 180.0);
        double endX = cx + (radius - 12) * Math.Cos(radAngle);
        double endY = cy + (radius - 12) * Math.Sin(radAngle);

        var line = new System.Windows.Shapes.Line
        {
            X1 = cx,
            Y1 = cy,
            X2 = endX,
            Y2 = endY,
            StrokeThickness = 2,
            StrokeEndLineCap = PenLineCap.Round
        };
        line.SetResourceReference(System.Windows.Shapes.Line.StrokeProperty, "CONCOST.Brush.Primary");
        handCanvas.Children.Add(line);

        // Selected tip dot
        var tipDot = new Border
        {
            Width = 8,
            Height = 8,
            CornerRadius = new CornerRadius(4)
        };
        tipDot.SetResourceReference(Border.BackgroundProperty, "CONCOST.Brush.Primary");
        System.Windows.Controls.Canvas.SetLeft(tipDot, endX - 4);
        System.Windows.Controls.Canvas.SetTop(tipDot, endY - 4);
        handCanvas.Children.Add(tipDot);

        // Center Pin
        var centerPin = new Border
        {
            Width = 8,
            Height = 8,
            CornerRadius = new CornerRadius(4)
        };
        centerPin.SetResourceReference(Border.BackgroundProperty, "CONCOST.Brush.Primary");
        System.Windows.Controls.Canvas.SetLeft(centerPin, cx - 4);
        System.Windows.Controls.Canvas.SetTop(centerPin, cy - 4);
        handCanvas.Children.Add(centerPin);

        _canvas.Children.Add(handCanvas);
    }
}
