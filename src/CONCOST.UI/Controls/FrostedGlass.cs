using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Shapes;

namespace CONCOST.UI.Controls;

/// <summary>
/// A frosted glass panel: it paints a blurred copy of the element behind it, tints it, and draws a
/// hairline edge — the translucent card used over photos and gradient backdrops.
/// </summary>
/// <remarks>
/// <para>
/// WPF has no backdrop filter, so the blur has to be built: point <see cref="Source"/> at the element
/// behind the panel and the control paints a <see cref="VisualBrush"/> of it through a
/// <see cref="BlurEffect"/>. The brush's viewbox is set to the panel's own rectangle <em>in the
/// source's coordinates</em>, so the blur lines up with what is actually behind it instead of stretching
/// the whole source into the panel.
/// </para>
/// <para>
/// That copy is re-rendered every frame the source changes, so this is a "a few panels over a hero
/// image" control, not a general-purpose backdrop for a scrolling list.
/// </para>
/// </remarks>
public class FrostedGlass : ContentControl
{
    private Rectangle? _glass;
    private Border? _tint;

    static FrostedGlass()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(FrostedGlass),
            new FrameworkPropertyMetadata(typeof(FrostedGlass)));
    }

    public static readonly DependencyProperty SourceProperty =
        DependencyProperty.Register(nameof(Source), typeof(UIElement), typeof(FrostedGlass),
            new PropertyMetadata(null, OnVisualChanged));

    /// <summary>The element behind the panel. Without it the panel is just a tinted card.</summary>
    public UIElement? Source
    {
        get => (UIElement?)GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    public static readonly DependencyProperty BlurRadiusProperty =
        DependencyProperty.Register(nameof(BlurRadius), typeof(double), typeof(FrostedGlass),
            new PropertyMetadata(18.0, OnVisualChanged));

    /// <summary>Blur strength in pixels. Default 18.</summary>
    public double BlurRadius
    {
        get => (double)GetValue(BlurRadiusProperty);
        set => SetValue(BlurRadiusProperty, value);
    }

    public static readonly DependencyProperty TintProperty =
        DependencyProperty.Register(nameof(Tint), typeof(Brush), typeof(FrostedGlass),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, OnVisualChanged));

    /// <summary>Colour laid over the blur — keep it semi-transparent. Empty takes a light veil.</summary>
    public Brush? Tint
    {
        get => (Brush?)GetValue(TintProperty);
        set => SetValue(TintProperty, value);
    }

    public static readonly DependencyProperty CornerProperty =
        DependencyProperty.Register(nameof(Corner), typeof(CornerRadius), typeof(FrostedGlass),
            new PropertyMetadata(new CornerRadius(14), OnVisualChanged));

    /// <summary>Corner radius of the panel. Default 14.</summary>
    public CornerRadius Corner
    {
        get => (CornerRadius)GetValue(CornerProperty);
        set => SetValue(CornerProperty, value);
    }

    public static readonly DependencyProperty EdgeBrushProperty =
        DependencyProperty.Register(nameof(EdgeBrush), typeof(Brush), typeof(FrostedGlass),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Hairline around the panel. Empty takes <c>CONCOST.Brush.Border</c>.</summary>
    public Brush? EdgeBrush
    {
        get => (Brush?)GetValue(EdgeBrushProperty);
        set => SetValue(EdgeBrushProperty, value);
    }

    private static void OnVisualChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FrostedGlass glass) glass.Refresh();
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _glass = GetTemplateChild("PART_Glass") as Rectangle;
        _tint = GetTemplateChild("PART_Tint") as Border;

        Refresh();
    }

    protected override Size ArrangeOverride(Size arrangeBounds)
    {
        var size = base.ArrangeOverride(arrangeBounds);

        // The panel moved: the slice of the source it shows moved with it.
        Refresh();

        return size;
    }

    private void Refresh()
    {
        if (_glass is null) return;

        _glass.RadiusX = Corner.TopLeft;
        _glass.RadiusY = Corner.TopLeft;

        if (_tint is not null)
        {
            _tint.CornerRadius = Corner;
            _tint.BorderBrush = EdgeBrush;
            _tint.Background = Tint ?? DefaultTint;
        }

        if (Source is null)
        {
            _glass.Fill = null;
            _glass.Effect = null;
            return;
        }

        var brush = new VisualBrush(Source)
        {
            Stretch = Stretch.None,
            AlignmentX = AlignmentX.Left,
            AlignmentY = AlignmentY.Top,
            ViewboxUnits = BrushMappingMode.Absolute
        };

        // Show exactly the region behind the panel: the panel's rectangle expressed in the source's
        // own coordinates. Without this the whole source would be squeezed into the panel.
        var offset = TranslatePoint(new Point(0, 0), Source);

        brush.Viewbox = new Rect(offset.X, offset.Y,
            Math.Max(1, ActualWidth), Math.Max(1, ActualHeight));

        _glass.Fill = brush;
        _glass.Effect = new BlurEffect
        {
            Radius = Math.Max(0, BlurRadius),
            KernelType = KernelType.Gaussian,
            RenderingBias = RenderingBias.Performance
        };
    }

    private static Brush DefaultTint
    {
        get
        {
            var brush = new SolidColorBrush(Color.FromArgb(0x88, 0xFF, 0xFF, 0xFF));
            brush.Freeze();
            return brush;
        }
    }
}
