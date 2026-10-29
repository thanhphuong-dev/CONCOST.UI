using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace CONCOST.UI.Controls;

public class ColorPicker : Control
{
    private Slider? _hueSlider;
    private Slider? _alphaSlider;
    private System.Windows.Controls.Canvas? _svCanvas;
    private Border? _svThumb;
    private Rectangle? _hueBackgroundRect;
    private bool _isDraggingSV;
    private bool _isSynchronizingColor;

    static ColorPicker()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(ColorPicker), new FrameworkPropertyMetadata(typeof(ColorPicker)));
    }

    public static readonly DependencyProperty SelectedColorProperty =
        DependencyProperty.Register(nameof(SelectedColor), typeof(Color), typeof(ColorPicker),
            new FrameworkPropertyMetadata(Colors.DodgerBlue, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedColorChanged));

    public static readonly DependencyProperty HexCodeProperty =
        DependencyProperty.Register(nameof(HexCode), typeof(string), typeof(ColorPicker),
            new FrameworkPropertyMetadata("#FF1E90FF", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnHexCodeChanged));

    public static readonly DependencyProperty ShowAlphaProperty =
        DependencyProperty.Register(nameof(ShowAlpha), typeof(bool), typeof(ColorPicker),
            new PropertyMetadata(true));

    public Color SelectedColor
    {
        get => (Color)GetValue(SelectedColorProperty);
        set => SetValue(SelectedColorProperty, value);
    }

    public string HexCode
    {
        get => (string)GetValue(HexCodeProperty);
        set => SetValue(HexCodeProperty, value);
    }

    public bool ShowAlpha
    {
        get => (bool)GetValue(ShowAlphaProperty);
        set => SetValue(ShowAlphaProperty, value);
    }

    public ObservableCollection<Color> PresetColors { get; } = new()
    {
        (Color)ColorConverter.ConvertFromString("#FF7900"),
        (Color)ColorConverter.ConvertFromString("#0284C7"),
        (Color)ColorConverter.ConvertFromString("#16A34A"),
        (Color)ColorConverter.ConvertFromString("#DC2626"),
        (Color)ColorConverter.ConvertFromString("#8B5CF6"),
        (Color)ColorConverter.ConvertFromString("#0D9488"),
        (Color)ColorConverter.ConvertFromString("#D97706"),
        (Color)ColorConverter.ConvertFromString("#DB2777"),
        (Color)ColorConverter.ConvertFromString("#475569"),
        (Color)ColorConverter.ConvertFromString("#0F172A")
    };

    private static void OnSelectedColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ColorPicker cp && e.NewValue is Color c && !cp._isSynchronizingColor)
        {
            var hex = $"#{c.A:X2}{c.R:X2}{c.G:X2}{c.B:X2}";
            cp._isSynchronizingColor = true;
            try { if (cp.HexCode != hex) cp.HexCode = hex; }
            finally { cp._isSynchronizingColor = false; }
        }
    }

    private static void OnHexCodeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ColorPicker cp && e.NewValue is string hex && !cp._isSynchronizingColor)
        {
            try
            {
                if (ColorConverter.ConvertFromString(hex) is Color c)
                {
                    cp._isSynchronizingColor = true;
                    cp.SelectedColor = c;
                }
            }
            catch (FormatException) { }
            catch (NotSupportedException) { }
            finally { cp._isSynchronizingColor = false; }
        }
    }

    public override void OnApplyTemplate()
    {
        if (_hueSlider != null) _hueSlider.ValueChanged -= OnHueSliderValueChanged;
        if (_alphaSlider != null) _alphaSlider.ValueChanged -= OnAlphaSliderValueChanged;
        if (_svCanvas != null)
        {
            _svCanvas.MouseLeftButtonDown -= OnSvMouseDown;
            _svCanvas.MouseMove -= OnSvMouseMove;
            _svCanvas.MouseLeftButtonUp -= OnSvMouseUp;
        }
        base.OnApplyTemplate();
        _hueSlider = GetTemplateChild("PART_HueSlider") as Slider;
        _alphaSlider = GetTemplateChild("PART_AlphaSlider") as Slider;
        _svCanvas = GetTemplateChild("PART_SVCanvas") as System.Windows.Controls.Canvas;
        _svThumb = GetTemplateChild("PART_SVThumb") as Border;
        _hueBackgroundRect = GetTemplateChild("PART_HueBackgroundRect") as Rectangle;

        if (_hueSlider != null)
        {
            _hueSlider.ValueChanged += OnHueSliderValueChanged;
        }

        if (_alphaSlider != null)
        {
            _alphaSlider.ValueChanged += OnAlphaSliderValueChanged;
        }

        if (_svCanvas != null)
        {
            _svCanvas.MouseLeftButtonDown += OnSvMouseDown;
            _svCanvas.MouseMove += OnSvMouseMove;
            _svCanvas.MouseLeftButtonUp += OnSvMouseUp;
        }

        UpdateHueBackground();
    }

    private void OnHueSliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        UpdateHueBackground();
        UpdateColorFromHSV();
    }

    private void OnAlphaSliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        var color = SelectedColor;
        SelectedColor = Color.FromArgb((byte)NumericCompatibility.Clamp(_alphaSlider?.Value ?? color.A, 0, 255), color.R, color.G, color.B);
    }

    private void OnSvMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton != MouseButton.Left || _svCanvas == null) return;
        _isDraggingSV = true;
        _svCanvas.CaptureMouse();
        UpdateSVFromPoint(e.GetPosition(_svCanvas));
        e.Handled = true;
    }

    private void OnSvMouseMove(object sender, MouseEventArgs e)
    {
        if (_isDraggingSV && _svCanvas != null) UpdateSVFromPoint(e.GetPosition(_svCanvas));
    }

    private void OnSvMouseUp(object sender, MouseButtonEventArgs e)
    {
        if (!_isDraggingSV) return;
        _isDraggingSV = false;
        _svCanvas?.ReleaseMouseCapture();
        e.Handled = true;
    }

    private void UpdateHueBackground()
    {
        if (_hueBackgroundRect == null) return;
        double hue = _hueSlider?.Value ?? 0.0;
        var pureHueColor = ColorFromHSV(hue, 1.0, 1.0, 255);
        _hueBackgroundRect.Fill = new SolidColorBrush(pureHueColor);
    }

    private void UpdateSVFromPoint(Point p)
    {
        if (_svCanvas == null) return;
        double w = _svCanvas.ActualWidth > 0 ? _svCanvas.ActualWidth : 200;
        double h = _svCanvas.ActualHeight > 0 ? _svCanvas.ActualHeight : 140;

        double s = NumericCompatibility.Clamp(p.X / w, 0.0, 1.0);
        double v = NumericCompatibility.Clamp(1.0 - (p.Y / h), 0.0, 1.0);

        if (_svThumb != null)
        {
            System.Windows.Controls.Canvas.SetLeft(_svThumb, NumericCompatibility.Clamp(p.X - 7, 0, w - 14));
            System.Windows.Controls.Canvas.SetTop(_svThumb, NumericCompatibility.Clamp(p.Y - 7, 0, h - 14));
        }

        double hue = _hueSlider?.Value ?? 0.0;
        byte a = (byte)(_alphaSlider?.Value ?? 255.0);
        SelectedColor = ColorFromHSV(hue, s, v, a);
    }

    private void UpdateColorFromHSV()
    {
        if (_svCanvas == null) return;
        double w = _svCanvas.ActualWidth > 0 ? _svCanvas.ActualWidth : 200;
        double h = _svCanvas.ActualHeight > 0 ? _svCanvas.ActualHeight : 140;

        double thumbX = _svThumb != null ? System.Windows.Controls.Canvas.GetLeft(_svThumb) + 7 : w / 2;
        double thumbY = _svThumb != null ? System.Windows.Controls.Canvas.GetTop(_svThumb) + 7 : h / 2;

        double s = NumericCompatibility.Clamp(thumbX / w, 0.0, 1.0);
        double v = NumericCompatibility.Clamp(1.0 - (thumbY / h), 0.0, 1.0);

        double hue = _hueSlider?.Value ?? 0.0;
        byte a = (byte)(_alphaSlider?.Value ?? 255.0);
        SelectedColor = ColorFromHSV(hue, s, v, a);
    }

    private static Color ColorFromHSV(double hue, double saturation, double value, byte alpha = 255)
    {
        int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
        double f = hue / 60 - Math.Floor(hue / 60);

        value = value * 255;
        byte v = Convert.ToByte(value);
        byte p = Convert.ToByte(value * (1 - saturation));
        byte q = Convert.ToByte(value * (1 - f * saturation));
        byte t = Convert.ToByte(value * (1 - (1 - f) * saturation));

        return hi switch
        {
            0 => Color.FromArgb(alpha, v, t, p),
            1 => Color.FromArgb(alpha, q, v, p),
            2 => Color.FromArgb(alpha, p, v, t),
            3 => Color.FromArgb(alpha, p, q, v),
            4 => Color.FromArgb(alpha, t, p, v),
            _ => Color.FromArgb(alpha, v, p, q)
        };
    }
}
