using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CONCOST.UI.Icons;

public class Icon : Control
{
    static Icon()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(Icon), new FrameworkPropertyMetadata(typeof(Icon)));
    }

    public static readonly DependencyProperty KindProperty =
        DependencyProperty.Register(nameof(Kind), typeof(IconKind), typeof(Icon),
            new FrameworkPropertyMetadata(IconKind.None, FrameworkPropertyMetadataOptions.AffectsRender, OnKindChanged));

    public IconKind Kind
    {
        get => (IconKind)GetValue(KindProperty);
        set => SetValue(KindProperty, value);
    }

    public static readonly DependencyProperty DataProperty =
        DependencyProperty.Register(nameof(Data), typeof(Geometry), typeof(Icon),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    public Geometry? Data
    {
        get => (Geometry?)GetValue(DataProperty);
        set => SetValue(DataProperty, value);
    }

    private static void OnKindChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Icon icon)
        {
            icon.Data = IconProvider.GetGeometry((IconKind)e.NewValue);
        }
    }
}
