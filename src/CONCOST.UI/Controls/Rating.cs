using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using CONCOST.UI.Icons;

namespace CONCOST.UI.Controls;

/// <summary>
/// A star-rating control with support for half-star precision, custom geometry icons,
/// hover preview, clear-on-click, and a formatted score label.
/// </summary>
public class Rating : Control
{
    private StackPanel? _panel;
    private TextBlock? _scoreText;

    // Default star path (Material Design star)
    private static readonly Geometry DefaultStarGeometry;

    static Rating()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(Rating), new FrameworkPropertyMetadata(typeof(Rating)));

        var g = Geometry.Parse("M12,17.27L18.18,21L16.54,13.97L22,9.24L14.81,8.62L12,2L9.19,8.62L2,9.24L7.45,13.97L5.82,21L12,17.27Z");
        g.Freeze();
        DefaultStarGeometry = g;
    }

    // ------------------------------------------------------------------ value / max

    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(nameof(Value), typeof(double), typeof(Rating),
            new FrameworkPropertyMetadata(5.0,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnValueChanged, CoerceValue));

    public static readonly DependencyProperty MaxProperty =
        DependencyProperty.Register(nameof(Max), typeof(int), typeof(Rating),
            new PropertyMetadata(5, OnStructureChanged, CoerceMax));

    public double Value
    {
        get => (double)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public int Max
    {
        get => (int)GetValue(MaxProperty);
        set => SetValue(MaxProperty, value);
    }

    // ------------------------------------------------------------------ item sizing / spacing

    public static readonly DependencyProperty ItemSizeProperty =
        DependencyProperty.Register(nameof(ItemSize), typeof(double), typeof(Rating),
            new PropertyMetadata(20.0, OnStructureChanged));

    public static readonly DependencyProperty ItemMarginProperty =
        DependencyProperty.Register(nameof(ItemMargin), typeof(Thickness), typeof(Rating),
            new PropertyMetadata(new Thickness(0, 0, 6, 0), OnStructureChanged));

    /// <summary>Uniform gap between items in device-independent pixels. Writing this sets <see cref="ItemMargin"/>.</summary>
    public static readonly DependencyProperty ItemGapProperty =
        DependencyProperty.Register(nameof(ItemGap), typeof(double), typeof(Rating),
            new PropertyMetadata(6.0, OnItemGapChanged));

    public double ItemSize
    {
        get => (double)GetValue(ItemSizeProperty);
        set => SetValue(ItemSizeProperty, value);
    }

    public Thickness ItemMargin
    {
        get => (Thickness)GetValue(ItemMarginProperty);
        set => SetValue(ItemMarginProperty, value);
    }

    public double ItemGap
    {
        get => (double)GetValue(ItemGapProperty);
        set => SetValue(ItemGapProperty, value);
    }

    private static void OnItemGapChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Rating r)
            r.ItemMargin = new Thickness(0, 0, (double)e.NewValue, 0);
    }

    // ------------------------------------------------------------------ interaction

    public static readonly DependencyProperty IsReadOnlyProperty =
        DependencyProperty.Register(nameof(IsReadOnly), typeof(bool), typeof(Rating),
            new PropertyMetadata(false, OnStructureChanged));

    /// <summary>Allow selecting half-star values (0.5 steps). Default <c>false</c>.</summary>
    public static readonly DependencyProperty AllowHalfProperty =
        DependencyProperty.Register(nameof(AllowHalf), typeof(bool), typeof(Rating),
            new PropertyMetadata(false, OnStructureChanged));

    /// <summary>
    /// When <c>true</c>, clicking a star that is already the current full value resets the rating to 0.
    /// Default <c>true</c>.
    /// </summary>
    public static readonly DependencyProperty AllowClearProperty =
        DependencyProperty.Register(nameof(AllowClear), typeof(bool), typeof(Rating),
            new PropertyMetadata(true));

    public bool IsReadOnly
    {
        get => (bool)GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    public bool AllowHalf
    {
        get => (bool)GetValue(AllowHalfProperty);
        set => SetValue(AllowHalfProperty, value);
    }

    public bool AllowClear
    {
        get => (bool)GetValue(AllowClearProperty);
        set => SetValue(AllowClearProperty, value);
    }

    // ------------------------------------------------------------------ score display

    public static readonly DependencyProperty ShowScoreProperty =
        DependencyProperty.Register(nameof(ShowScore), typeof(bool), typeof(Rating),
            new PropertyMetadata(false, OnStructureChanged));

    /// <summary>
    /// Composite format string for the score label. Argument {0} = value, {1} = max.
    /// Default <c>"{0:0.0} / {1}"</c>.
    /// </summary>
    public static readonly DependencyProperty ScoreFormatProperty =
        DependencyProperty.Register(nameof(ScoreFormat), typeof(string), typeof(Rating),
            new PropertyMetadata("{0:0.0} / {1}", OnStructureChanged));

    public bool ShowScore
    {
        get => (bool)GetValue(ShowScoreProperty);
        set => SetValue(ShowScoreProperty, value);
    }

    public string ScoreFormat
    {
        get => (string)GetValue(ScoreFormatProperty);
        set => SetValue(ScoreFormatProperty, value);
    }

    // ------------------------------------------------------------------ brushes

    public static readonly DependencyProperty ActiveBrushProperty =
        DependencyProperty.Register(nameof(ActiveBrush), typeof(Brush), typeof(Rating),
            new PropertyMetadata(null, OnStructureChanged));

    public static readonly DependencyProperty InactiveBrushProperty =
        DependencyProperty.Register(nameof(InactiveBrush), typeof(Brush), typeof(Rating),
            new PropertyMetadata(null, OnStructureChanged));

    public Brush? ActiveBrush
    {
        get => (Brush?)GetValue(ActiveBrushProperty);
        set => SetValue(ActiveBrushProperty, value);
    }

    public Brush? InactiveBrush
    {
        get => (Brush?)GetValue(InactiveBrushProperty);
        set => SetValue(InactiveBrushProperty, value);
    }

    // ------------------------------------------------------------------ custom icon shape

    /// <summary>
    /// Custom path geometry used instead of the default star. Set to <c>null</c> to restore the star.
    /// </summary>
    public static readonly DependencyProperty IconGeometryProperty =
        DependencyProperty.Register(nameof(IconGeometry), typeof(Geometry), typeof(Rating),
            new PropertyMetadata(null, OnStructureChanged));

    public Geometry? IconGeometry
    {
        get => (Geometry?)GetValue(IconGeometryProperty);
        set => SetValue(IconGeometryProperty, value);
    }

    // ------------------------------------------------------------------ HoverValue (read-only)

    private static readonly DependencyPropertyKey HoverValuePropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(HoverValue), typeof(double), typeof(Rating),
            new PropertyMetadata(0.0));

    /// <summary>The rating value the pointer is currently hovering over, for live preview. Read-only.</summary>
    public static readonly DependencyProperty HoverValueProperty = HoverValuePropertyKey.DependencyProperty;

    public double HoverValue => (double)GetValue(HoverValueProperty);

    // ------------------------------------------------------------------ ValueChanged routed event

    public static readonly RoutedEvent ValueChangedEvent =
        EventManager.RegisterRoutedEvent(nameof(ValueChanged), RoutingStrategy.Bubble,
            typeof(RoutedPropertyChangedEventHandler<double>), typeof(Rating));

    public event RoutedPropertyChangedEventHandler<double> ValueChanged
    {
        add => AddHandler(ValueChangedEvent, value);
        remove => RemoveHandler(ValueChangedEvent, value);
    }

    // ------------------------------------------------------------------ template

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        _panel = GetTemplateChild("PART_Panel") as StackPanel;
        _scoreText = GetTemplateChild("PART_Score") as TextBlock;
        RebuildStars();
    }

    // ------------------------------------------------------------------ coerce / callbacks

    private static object CoerceMax(DependencyObject d, object value) => Math.Max(1, (int)value);

    private static object CoerceValue(DependencyObject d, object value)
    {
        var number = (double)value;
        if (!NumericCompatibility.IsFinite(number)) number = 0;
        return NumericCompatibility.Clamp(number, 0, ((Rating)d).Max);
    }

    private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Rating rating)
        {
            rating.UpdateStarsFill();
            rating.RaiseEvent(new RoutedPropertyChangedEventArgs<double>(
                (double)e.OldValue, (double)e.NewValue, ValueChangedEvent));
        }
    }

    private static void OnStructureChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Rating rating)
        {
            rating.CoerceValue(ValueProperty);
            rating.RebuildStars();
        }
    }

    // ------------------------------------------------------------------ build stars

    private void RebuildStars()
    {
        if (_panel == null) return;
        _panel.Children.Clear();

        Geometry iconGeom = IconGeometry ?? DefaultStarGeometry;

        int max = Math.Max(1, Max);
        for (int i = 1; i <= max; i++)
        {
            int starIndex = i;

            var container = new Grid
            {
                Width = ItemSize,
                Height = ItemSize,
                Margin = (i == max) ? new Thickness(0) : ItemMargin,
                Cursor = IsReadOnly ? Cursors.Arrow : Cursors.Hand,
                Background = Brushes.Transparent
            };

            // Background (inactive) path
            var bgPath = new Path
            {
                Data = iconGeom,
                Stretch = Stretch.Uniform,
                Fill = InactiveBrush ?? TryFindBrush("CONCOST.Brush.Border")
            };
            container.Children.Add(bgPath);

            // Foreground (active) path clipped to show partial fill
            var fgPath = new Path
            {
                Data = iconGeom,
                Stretch = Stretch.Uniform,
                Fill = ActiveBrush ?? TryFindBrush("CONCOST.Brush.Warning")
            };
            var clipBorder = new Border
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Stretch,
                ClipToBounds = true,
                Child = fgPath
            };
            container.Children.Add(clipBorder);

            if (!IsReadOnly)
            {
                container.MouseMove += (s, e) =>
                {
                    double hv = starIndex - (AllowHalf && e.GetPosition(container).X < ItemSize / 2 ? 0.5 : 0);
                    SetValue(HoverValuePropertyKey, hv);
                    PreviewStarsFill(hv);
                };

                container.MouseLeave += (s, e) =>
                {
                    SetValue(HoverValuePropertyKey, 0.0);
                    UpdateStarsFill();
                };

                container.MouseLeftButtonDown += (s, e) =>
                {
                    double clickVal = starIndex - (AllowHalf && e.GetPosition(container).X < ItemSize / 2 ? 0.5 : 0);

                    // Clear on same value click
                    if (AllowClear && Math.Abs(Value - clickVal) < 0.001)
                        Value = 0;
                    else
                        Value = clickVal;
                };

                // Scale on hover
                container.MouseEnter += (s, e) =>
                {
                    container.RenderTransform = new ScaleTransform(1.15, 1.15, ItemSize / 2, ItemSize / 2);
                };
                container.MouseLeave += (s, e) =>
                {
                    container.RenderTransform = Transform.Identity;
                };
            }

            _panel.Children.Add(container);
        }

        UpdateStarsFill();
    }

    private Brush TryFindBrush(string key)
    {
        try { return (Brush)FindResource(key); }
        catch { return Brushes.Gray; }
    }

    private void PreviewStarsFill(double hoverVal)
    {
        if (_panel == null) return;
        for (int i = 0; i < _panel.Children.Count; i++)
        {
            if (_panel.Children[i] is Grid container && container.Children.Count >= 2
                && container.Children[1] is Border clipBorder)
            {
                double fillRatio = Math.Max(0.0, Math.Min(1.0, hoverVal - i));
                clipBorder.Width = ItemSize * fillRatio;
            }
        }
    }

    private void UpdateStarsFill()
    {
        if (_panel == null) return;

        double val = Math.Max(0, Math.Min(Max, Value));
        for (int i = 0; i < _panel.Children.Count; i++)
        {
            if (_panel.Children[i] is Grid container && container.Children.Count >= 2
                && container.Children[1] is Border clipBorder)
            {
                double fillRatio = Math.Max(0.0, Math.Min(1.0, val - i));
                clipBorder.Width = ItemSize * fillRatio;
            }
        }

        if (_scoreText != null)
        {
            try { _scoreText.Text = string.Format(ScoreFormat, Value, Max); }
            catch { _scoreText.Text = $"{Value:0.0} / {Max}"; }

            _scoreText.Visibility = ShowScore ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
