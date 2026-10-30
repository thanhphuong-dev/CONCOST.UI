using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CONCOST.UI.Controls.Ribbon;

/// <summary>
/// A ribbon-sized colour button: a swatch plus a label, and a pop-up holding the full
/// <see cref="ColorPicker"/>.
/// <para>
/// The item only shows the current colour — the hue/saturation pad, the alpha slider and the
/// presets all come from the picker inside the pop-up, so a ribbon colour and a standalone colour
/// picker behave identically.
/// </para>
/// </summary>
public class RibbonColorPicker : RibbonItem
{
    public static readonly DependencyProperty SelectedColorProperty =
        DependencyProperty.Register(nameof(SelectedColor), typeof(Color), typeof(RibbonColorPicker),
            new FrameworkPropertyMetadata(Colors.DodgerBlue,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedColorChanged));

    public static readonly DependencyProperty IsDropDownOpenProperty =
        DependencyProperty.Register(nameof(IsDropDownOpen), typeof(bool), typeof(RibbonColorPicker),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public static readonly DependencyProperty ShowAlphaProperty =
        DependencyProperty.Register(nameof(ShowAlpha), typeof(bool), typeof(RibbonColorPicker),
            new FrameworkPropertyMetadata(false));

    private static readonly DependencyPropertyKey SwatchBrushPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(SwatchBrush), typeof(Brush), typeof(RibbonColorPicker),
            new FrameworkPropertyMetadata(Brushes.DodgerBlue));

    /// <summary>Bindable form of <see cref="SelectedColor"/> for the swatch in the template.</summary>
    public static readonly DependencyProperty SwatchBrushProperty = SwatchBrushPropertyKey.DependencyProperty;

    static RibbonColorPicker()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(RibbonColorPicker),
            new FrameworkPropertyMetadata(typeof(RibbonColorPicker)));
    }

    public RibbonColorPicker()
    {
        Cursor = Cursors.Hand;
    }

    /// <summary>Raised whenever the user picks a different colour.</summary>
    public event EventHandler? SelectedColorChanged;

    /// <summary>Colour shown in the swatch and edited in the pop-up.</summary>
    public Color SelectedColor
    {
        get => (Color)GetValue(SelectedColorProperty);
        set => SetValue(SelectedColorProperty, value);
    }

    /// <summary>True while the picker pop-up is showing.</summary>
    public bool IsDropDownOpen
    {
        get => (bool)GetValue(IsDropDownOpenProperty);
        set => SetValue(IsDropDownOpenProperty, value);
    }

    /// <summary>Shows the opacity slider in the pop-up.</summary>
    public bool ShowAlpha
    {
        get => (bool)GetValue(ShowAlphaProperty);
        set => SetValue(ShowAlphaProperty, value);
    }

    /// <summary>Frozen brush matching <see cref="SelectedColor"/>.</summary>
    public Brush SwatchBrush => (Brush)GetValue(SwatchBrushProperty);

    /// <summary>Opens the picker.</summary>
    public override void PerformClick()
    {
        if (!IsEnabled) return;

        IsDropDownOpen = true;
    }

    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonUp(e);
        if (!IsEnabled) return;

        IsDropDownOpen = !IsDropDownOpen;
        e.Handled = true;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (!IsEnabled) return;

        if (e.Key is Key.Down or Key.Enter or Key.Space)
        {
            IsDropDownOpen = true;
            e.Handled = true;
        }
        else if (e.Key == Key.Escape && IsDropDownOpen)
        {
            IsDropDownOpen = false;
            e.Handled = true;
        }
    }

    private static void OnSelectedColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not RibbonColorPicker picker) return;

        var brush = new SolidColorBrush((Color)e.NewValue);
        brush.Freeze();
        picker.SetValue(SwatchBrushPropertyKey, brush);

        picker.SelectedColorChanged?.Invoke(picker, EventArgs.Empty);
    }
}
