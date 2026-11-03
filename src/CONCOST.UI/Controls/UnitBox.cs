using System.Windows;
using System.Windows.Controls;

namespace CONCOST.UI.Controls;

/// <summary>
/// A numeric field with the engineering unit pinned to its right edge —
/// <c>3200</c> next to <c>mm</c>, <c>25</c> next to <c>kN/m³</c>. The unit is a
/// label the host owns, not a parser: <see cref="Value"/> stays in the unit
/// <see cref="Unit"/> names, and converting between units is the host's model
/// concern. The optional <see cref="Header"/> turns the box into a labelled
/// form row; arrow keys and the inner spin buttons step by <see cref="Increment"/>.
/// </summary>
public class UnitBox : Control
{
    static UnitBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(UnitBox), new FrameworkPropertyMetadata(typeof(UnitBox)));
    }

    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(nameof(Value), typeof(double), typeof(UnitBox),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public static readonly DependencyProperty UnitProperty =
        DependencyProperty.Register(nameof(Unit), typeof(string), typeof(UnitBox),
            new PropertyMetadata("mm"));

    public static readonly DependencyProperty DecimalPlacesProperty =
        DependencyProperty.Register(nameof(DecimalPlaces), typeof(int), typeof(UnitBox),
            new PropertyMetadata(1));

    public static readonly DependencyProperty HeaderProperty =
        DependencyProperty.Register(nameof(Header), typeof(string), typeof(UnitBox),
            new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty IncrementProperty =
        DependencyProperty.Register(nameof(Increment), typeof(double), typeof(UnitBox),
            new PropertyMetadata(1.0));

    public static readonly DependencyProperty MinimumProperty =
        DependencyProperty.Register(nameof(Minimum), typeof(double), typeof(UnitBox),
            new PropertyMetadata(double.NegativeInfinity));

    public static readonly DependencyProperty MaximumProperty =
        DependencyProperty.Register(nameof(Maximum), typeof(double), typeof(UnitBox),
            new PropertyMetadata(double.PositiveInfinity));

    public static readonly DependencyProperty IsReadOnlyProperty =
        DependencyProperty.Register(nameof(IsReadOnly), typeof(bool), typeof(UnitBox),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public static readonly RoutedEvent ValueChangedEvent = EventManager.RegisterRoutedEvent(
        nameof(ValueChanged), RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<double>), typeof(UnitBox));

    public UnitBox()
    {
        // The editor inside the template raises NumericUpDown's own routed event — a
        // different instance from ours, so bubbling alone never matches a host's
        // handler. Listen for it and raise the UnitBox one in step.
        AddHandler(NumericUpDown.ValueChangedEvent, new RoutedPropertyChangedEventHandler<double>(OnEditorValueChanged));
    }

    private void OnEditorValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        RaiseEvent(new RoutedPropertyChangedEventArgs<double>(e.OldValue, e.NewValue, ValueChangedEvent));
    }

    public double Value
    {
        get => (double)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public string Unit
    {
        get => (string)GetValue(UnitProperty);
        set => SetValue(UnitProperty, value);
    }

    public int DecimalPlaces
    {
        get => (int)GetValue(DecimalPlacesProperty);
        set => SetValue(DecimalPlacesProperty, value);
    }

    /// <summary>Optional label above the field; the box is a plain field when it is empty.</summary>
    public string Header
    {
        get => (string)GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    /// <summary>How far one arrow-key press or spin-button click moves the value.</summary>
    public double Increment
    {
        get => (double)GetValue(IncrementProperty);
        set => SetValue(IncrementProperty, value);
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

    public bool IsReadOnly
    {
        get => (bool)GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    /// <summary>Raised when the inner numeric editor commits a different value.</summary>
    public event RoutedPropertyChangedEventHandler<double> ValueChanged
    {
        add => AddHandler(ValueChangedEvent, value);
        remove => RemoveHandler(ValueChangedEvent, value);
    }
}
