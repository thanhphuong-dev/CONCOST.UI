using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CONCOST.UI.Icons;

namespace CONCOST.UI.Controls;

public enum DividerLineStyle
{
    Solid,
    Dashed,
    Dotted
}

/// <summary>
/// A horizontal or vertical rule with an optional centred label and icon.
/// Supports solid, dashed and dotted lines, custom brush/thickness, and text styling.
/// </summary>
public class Divider : Control
{
    static Divider()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(Divider), new FrameworkPropertyMetadata(typeof(Divider)));
    }

    // ------------------------------------------------------------------ original properties

    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(nameof(Text), typeof(string), typeof(Divider),
            new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty OrientationProperty =
        DependencyProperty.Register(nameof(Orientation), typeof(Orientation), typeof(Divider),
            new PropertyMetadata(Orientation.Horizontal));

    public static readonly DependencyProperty IconProperty =
        DependencyProperty.Register(nameof(Icon), typeof(IconKind), typeof(Divider),
            new PropertyMetadata(IconKind.None));

    public static readonly DependencyProperty LineStyleProperty =
        DependencyProperty.Register(nameof(LineStyle), typeof(DividerLineStyle), typeof(Divider),
            new PropertyMetadata(DividerLineStyle.Solid));

    public static readonly DependencyProperty TextAlignmentProperty =
        DependencyProperty.Register(nameof(TextAlignment), typeof(HorizontalAlignment), typeof(Divider),
            new PropertyMetadata(HorizontalAlignment.Center));

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public Orientation Orientation
    {
        get => (Orientation)GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    public IconKind Icon
    {
        get => (IconKind)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public DividerLineStyle LineStyle
    {
        get => (DividerLineStyle)GetValue(LineStyleProperty);
        set => SetValue(LineStyleProperty, value);
    }

    public HorizontalAlignment TextAlignment
    {
        get => (HorizontalAlignment)GetValue(TextAlignmentProperty);
        set => SetValue(TextAlignmentProperty, value);
    }

    // ------------------------------------------------------------------ line styling

    /// <summary>Thickness of the rule line. Default 1.</summary>
    public static readonly DependencyProperty LineThicknessProperty =
        DependencyProperty.Register(nameof(LineThickness), typeof(double), typeof(Divider),
            new FrameworkPropertyMetadata(1.0));

    public double LineThickness
    {
        get => (double)GetValue(LineThicknessProperty);
        set => SetValue(LineThicknessProperty, value);
    }

    /// <summary>Brush of the rule line. Null falls back to the theme border brush.</summary>
    public static readonly DependencyProperty LineBrushProperty =
        DependencyProperty.Register(nameof(LineBrush), typeof(Brush), typeof(Divider),
            new FrameworkPropertyMetadata(null));

    public Brush? LineBrush
    {
        get => (Brush?)GetValue(LineBrushProperty);
        set => SetValue(LineBrushProperty, value);
    }

    // ------------------------------------------------------------------ text styling

    /// <summary>Font size of the centred label text. Default 12.</summary>
    public static readonly DependencyProperty TextFontSizeProperty =
        DependencyProperty.Register(nameof(TextFontSize), typeof(double), typeof(Divider),
            new FrameworkPropertyMetadata(12.0));

    public double TextFontSize
    {
        get => (double)GetValue(TextFontSizeProperty);
        set => SetValue(TextFontSizeProperty, value);
    }

    /// <summary>Foreground brush of the centred label. Null uses the theme text brush.</summary>
    public static readonly DependencyProperty TextForegroundProperty =
        DependencyProperty.Register(nameof(TextForeground), typeof(Brush), typeof(Divider),
            new FrameworkPropertyMetadata(null));

    public Brush? TextForeground
    {
        get => (Brush?)GetValue(TextForegroundProperty);
        set => SetValue(TextForegroundProperty, value);
    }

    /// <summary>Padding on each side of the label, between the text and the line. Default <c>8,0,8,0</c>.</summary>
    public static readonly DependencyProperty TextPaddingProperty =
        DependencyProperty.Register(nameof(TextPadding), typeof(Thickness), typeof(Divider),
            new FrameworkPropertyMetadata(new Thickness(8, 0, 8, 0)));

    public Thickness TextPadding
    {
        get => (Thickness)GetValue(TextPaddingProperty);
        set => SetValue(TextPaddingProperty, value);
    }

    // ------------------------------------------------------------------ outer spacing

    /// <summary>
    /// Additional outer margin: top and bottom for a horizontal divider, left and right for a vertical one.
    /// Default 0 leaves the outer margin from <see cref="FrameworkElement.Margin"/> unchanged.
    /// </summary>
    public static readonly DependencyProperty SpacingProperty =
        DependencyProperty.Register(nameof(Spacing), typeof(double), typeof(Divider),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsMeasure));

    public double Spacing
    {
        get => (double)GetValue(SpacingProperty);
        set => SetValue(SpacingProperty, value);
    }
}
