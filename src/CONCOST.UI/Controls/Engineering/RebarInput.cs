using System.Windows;
using System.Windows.Controls;

namespace CONCOST.UI.Controls.Engineering;

public class RebarInput : Control
{
    private TextBox? _textBox;

    static RebarInput()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(RebarInput), new FrameworkPropertyMetadata(typeof(RebarInput)));
    }

    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(nameof(Text), typeof(string), typeof(RebarInput),
            new FrameworkPropertyMetadata("4D20", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnTextChanged));

    public static readonly DependencyProperty IsValidProperty =
        DependencyProperty.Register(nameof(IsValid), typeof(bool), typeof(RebarInput),
            new PropertyMetadata(true));

    public static readonly DependencyProperty AreaMm2Property =
        DependencyProperty.Register(nameof(AreaMm2), typeof(double), typeof(RebarInput),
            new PropertyMetadata(1256.6));

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public bool IsValid
    {
        get => (bool)GetValue(IsValidProperty);
        private set => SetValue(IsValidProperty, value);
    }

    public double AreaMm2
    {
        get => (double)GetValue(AreaMm2Property);
        private set => SetValue(AreaMm2Property, value);
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        _textBox = GetTemplateChild("PART_TextBox") as TextBox;
        UpdateParsedState(Text);
    }

    private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is RebarInput input)
        {
            input.UpdateParsedState(e.NewValue as string);
        }
    }

    private void UpdateParsedState(string? text)
    {
        var info = RebarParser.Parse(text);
        IsValid = info.IsValid;
        AreaMm2 = Math.Round(info.TotalAreaMm2, 1);
    }
}
