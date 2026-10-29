using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace CONCOST.UI.Controls;

/// <summary>A culture-aware numeric editor with bounded keyboard and button stepping.</summary>
public class NumericUpDown : Control
{
    private ButtonBase? _increaseButton;
    private ButtonBase? _decreaseButton;
    private TextBox? _textBox;

    private static readonly DependencyPropertyKey CanIncreasePropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(CanIncrease), typeof(bool), typeof(NumericUpDown), new PropertyMetadata(true));
    private static readonly DependencyPropertyKey CanDecreasePropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(CanDecrease), typeof(bool), typeof(NumericUpDown), new PropertyMetadata(true));

    static NumericUpDown()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(NumericUpDown), new FrameworkPropertyMetadata(typeof(NumericUpDown)));
    }

    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
        nameof(Value), typeof(double), typeof(NumericUpDown),
        new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValueChanged, CoerceValue));
    public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register(
        nameof(Minimum), typeof(double), typeof(NumericUpDown), new FrameworkPropertyMetadata(double.MinValue, OnRangeChanged, CoerceMinimum));
    public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register(
        nameof(Maximum), typeof(double), typeof(NumericUpDown), new FrameworkPropertyMetadata(double.MaxValue, OnRangeChanged, CoerceMaximum));
    public static readonly DependencyProperty IncrementProperty = DependencyProperty.Register(
        nameof(Increment), typeof(double), typeof(NumericUpDown), new PropertyMetadata(1d, null, CoerceIncrement));
    public static readonly DependencyProperty DecimalPlacesProperty = DependencyProperty.Register(
        nameof(DecimalPlaces), typeof(int), typeof(NumericUpDown), new PropertyMetadata(2, OnDecimalPlacesChanged, CoerceDecimalPlaces));
    public static readonly DependencyProperty SuffixProperty = DependencyProperty.Register(nameof(Suffix), typeof(string), typeof(NumericUpDown), new PropertyMetadata(string.Empty));
    public static readonly DependencyProperty PrefixProperty = DependencyProperty.Register(nameof(Prefix), typeof(string), typeof(NumericUpDown), new PropertyMetadata(string.Empty));
    public static readonly DependencyProperty FormatStringProperty = DependencyProperty.Register(
        nameof(FormatString), typeof(string), typeof(NumericUpDown), new PropertyMetadata(string.Empty, OnFormatChanged));
    public static readonly DependencyProperty CanIncreaseProperty = CanIncreasePropertyKey.DependencyProperty;
    public static readonly DependencyProperty CanDecreaseProperty = CanDecreasePropertyKey.DependencyProperty;

    public static readonly RoutedEvent ValueChangedEvent = EventManager.RegisterRoutedEvent(
        nameof(ValueChanged), RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<double>), typeof(NumericUpDown));

    public event RoutedPropertyChangedEventHandler<double> ValueChanged
    {
        add => AddHandler(ValueChangedEvent, value);
        remove => RemoveHandler(ValueChangedEvent, value);
    }

    public double Value { get => (double)GetValue(ValueProperty); set => SetValue(ValueProperty, value); }
    public double Minimum { get => (double)GetValue(MinimumProperty); set => SetValue(MinimumProperty, value); }
    public double Maximum { get => (double)GetValue(MaximumProperty); set => SetValue(MaximumProperty, value); }
    public double Increment { get => (double)GetValue(IncrementProperty); set => SetValue(IncrementProperty, value); }
    public int DecimalPlaces { get => (int)GetValue(DecimalPlacesProperty); set => SetValue(DecimalPlacesProperty, value); }
    public string Suffix { get => (string)GetValue(SuffixProperty); set => SetValue(SuffixProperty, value); }
    public string Prefix { get => (string)GetValue(PrefixProperty); set => SetValue(PrefixProperty, value); }
    /// <summary>Gets or sets a .NET numeric format string. When empty, DecimalPlaces controls the display precision.</summary>
    public string FormatString { get => (string)GetValue(FormatStringProperty); set => SetValue(FormatStringProperty, value); }
    public bool CanIncrease => (bool)GetValue(CanIncreaseProperty);
    public bool CanDecrease => (bool)GetValue(CanDecreaseProperty);

    public override void OnApplyTemplate()
    {
        DetachTemplateHandlers();
        base.OnApplyTemplate();

        _increaseButton = GetTemplateChild("PART_IncreaseButton") as ButtonBase;
        _decreaseButton = GetTemplateChild("PART_DecreaseButton") as ButtonBase;
        _textBox = GetTemplateChild("PART_TextBox") as TextBox;

        if (_increaseButton is not null) _increaseButton.Click += OnIncreaseClick;
        if (_decreaseButton is not null) _decreaseButton.Click += OnDecreaseClick;
        if (_textBox is not null)
        {
            _textBox.Text = FormatValue(Value);
            _textBox.LostFocus += OnTextBoxLostFocus;
            _textBox.KeyDown += OnTextBoxKeyDown;
        }
        UpdateButtonStates();
    }

    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
        if (IsKeyboardFocusWithin && e.Delta != 0)
        {
            Step(e.Delta > 0 ? 1 : -1);
            e.Handled = true;
        }
        base.OnMouseWheel(e);
    }

    private void DetachTemplateHandlers()
    {
        if (_increaseButton is not null) _increaseButton.Click -= OnIncreaseClick;
        if (_decreaseButton is not null) _decreaseButton.Click -= OnDecreaseClick;
        if (_textBox is not null)
        {
            _textBox.LostFocus -= OnTextBoxLostFocus;
            _textBox.KeyDown -= OnTextBoxKeyDown;
        }
        _increaseButton = null;
        _decreaseButton = null;
        _textBox = null;
    }

    private void OnIncreaseClick(object sender, RoutedEventArgs e) => Step(1);
    private void OnDecreaseClick(object sender, RoutedEventArgs e) => Step(-1);

    private void OnTextBoxLostFocus(object sender, RoutedEventArgs e) => ParseAndApplyText();

    private void OnTextBoxKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            ParseAndApplyText();
            e.Handled = true;
        }
        else if (e.Key == Key.Up || e.Key == Key.Down)
        {
            Step(e.Key == Key.Up ? 1 : -1);
            e.Handled = true;
        }
        else if (e.Key == Key.Escape && _textBox is not null)
        {
            _textBox.Text = FormatValue(Value);
            e.Handled = true;
        }
    }

    private void Step(int direction)
    {
        double next;
        try
        {
            next = (double)((decimal)Value + direction * (decimal)Increment);
        }
        catch (OverflowException)
        {
            next = Value + direction * Increment;
        }

        if (!NumericCompatibility.IsFinite(next)) return;
        SetCurrentValue(ValueProperty, next);
        UpdateTextBox();
    }

    private void ParseAndApplyText()
    {
        if (_textBox is null) return;
        var text = _textBox.Text?.Trim();
        var styles = NumberStyles.Float | NumberStyles.AllowThousands;
        bool isPercent = false;
        if (!string.IsNullOrWhiteSpace(FormatString))
        {
            char specifier = FormatString.TrimStart()[0];
            isPercent = specifier is 'P' or 'p';
            if (specifier is 'C' or 'c') styles |= NumberStyles.AllowParentheses;
        }

        string currentText = text ?? string.Empty;
        string invariantText = currentText;
        if (isPercent)
        {
            currentText = currentText.Replace(CultureInfo.CurrentCulture.NumberFormat.PercentSymbol, string.Empty, StringComparison.Ordinal).Trim();
            invariantText = invariantText.Replace(CultureInfo.InvariantCulture.NumberFormat.PercentSymbol, string.Empty, StringComparison.Ordinal).Trim();
        }
        else if (!string.IsNullOrWhiteSpace(FormatString) && FormatString.TrimStart()[0] is 'C' or 'c')
        {
            currentText = currentText.Replace(CultureInfo.CurrentCulture.NumberFormat.CurrencySymbol, string.Empty, StringComparison.Ordinal).Trim();
            invariantText = invariantText.Replace(CultureInfo.InvariantCulture.NumberFormat.CurrencySymbol, string.Empty, StringComparison.Ordinal).Trim();
        }

        if ((double.TryParse(currentText, styles, CultureInfo.CurrentCulture, out var value) ||
             double.TryParse(invariantText, styles, CultureInfo.InvariantCulture, out value)) && NumericCompatibility.IsFinite(value))
            Value = isPercent ? value / 100.0 : value;

        _textBox.Text = FormatValue(Value);
    }

    private string FormatValue(double value)
    {
        if (!string.IsNullOrWhiteSpace(FormatString))
        {
            try { return value.ToString(FormatString, CultureInfo.CurrentCulture); }
            catch (FormatException) { }
        }

        return value.ToString(DecimalPlaces == 0 ? "0" : "0." + new string('#', DecimalPlaces), CultureInfo.CurrentCulture);
    }

    private static object CoerceValue(DependencyObject d, object baseValue)
    {
        var control = (NumericUpDown)d;
        var value = (double)baseValue;
        if (double.IsNaN(value)) value = control.Minimum <= 0 && control.Maximum >= 0 ? 0 : control.Minimum;
        if (value < control.Minimum) return control.Minimum;
        if (value > control.Maximum) return control.Maximum;
        return value;
    }

    private static object CoerceMinimum(DependencyObject d, object baseValue)
    {
        double value = (double)baseValue;
        return NumericCompatibility.IsFinite(value) ? value : double.MinValue;
    }

    private static object CoerceMaximum(DependencyObject d, object baseValue)
    {
        double value = (double)baseValue;
        return NumericCompatibility.IsFinite(value) ? value : double.MaxValue;
    }

    private static object CoerceIncrement(DependencyObject d, object baseValue)
    {
        var increment = (double)baseValue;
        return NumericCompatibility.IsFinite(increment) && increment > 0 ? increment : 1d;
    }

    private static object CoerceDecimalPlaces(DependencyObject d, object baseValue) => NumericCompatibility.Clamp((int)baseValue, 0, 15);

    private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (NumericUpDown)d;
        if (control._textBox is not null && !control._textBox.IsKeyboardFocused)
            control._textBox.Text = control.FormatValue((double)e.NewValue);
        control.UpdateButtonStates();
        control.RaiseEvent(new RoutedPropertyChangedEventArgs<double>(
            (double)e.OldValue, (double)e.NewValue, ValueChangedEvent));
    }

    private static void OnRangeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (NumericUpDown)d;
        if (e.Property == MinimumProperty && control.Maximum < control.Minimum)
            control.SetCurrentValue(MaximumProperty, control.Minimum);
        else if (e.Property == MaximumProperty && control.Minimum > control.Maximum)
            control.SetCurrentValue(MinimumProperty, control.Maximum);
        control.CoerceValue(ValueProperty);
        control.UpdateButtonStates();
    }

    private static void OnDecimalPlacesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (NumericUpDown)d;
        if (control._textBox is not null && !control._textBox.IsKeyboardFocused)
            control._textBox.Text = control.FormatValue(control.Value);
    }

    private static void OnFormatChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (NumericUpDown)d;
        if (control._textBox is not null && !control._textBox.IsKeyboardFocused)
            control._textBox.Text = control.FormatValue(control.Value);
    }

    private void UpdateTextBox()
    {
        if (_textBox is not null)
            _textBox.Text = FormatValue(Value);
    }

    private void UpdateButtonStates()
    {
        SetValue(CanIncreasePropertyKey, Value < Maximum);
        SetValue(CanDecreasePropertyKey, Value > Minimum);
    }
}
