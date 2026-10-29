using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using CONCOST.UI.Icons;

namespace CONCOST.UI.Controls;

public class Rating : Control
{
    private StackPanel? _panel;
    private TextBlock? _scoreText;

    static Rating()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(Rating), new FrameworkPropertyMetadata(typeof(Rating)));
    }

    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(nameof(Value), typeof(double), typeof(Rating),
            new FrameworkPropertyMetadata(5.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValueChanged, CoerceValue));

    public static readonly DependencyProperty MaxProperty =
        DependencyProperty.Register(nameof(Max), typeof(int), typeof(Rating),
            new PropertyMetadata(5, OnStructureChanged, CoerceMax));

    public static readonly DependencyProperty ItemSizeProperty =
        DependencyProperty.Register(nameof(ItemSize), typeof(double), typeof(Rating),
            new PropertyMetadata(20.0, OnStructureChanged));

    public static readonly DependencyProperty ItemMarginProperty =
        DependencyProperty.Register(nameof(ItemMargin), typeof(Thickness), typeof(Rating),
            new PropertyMetadata(new Thickness(0, 0, 6, 0), OnStructureChanged));

    public static readonly DependencyProperty IsReadOnlyProperty =
        DependencyProperty.Register(nameof(IsReadOnly), typeof(bool), typeof(Rating),
            new PropertyMetadata(false));

    public static readonly DependencyProperty ShowScoreProperty =
        DependencyProperty.Register(nameof(ShowScore), typeof(bool), typeof(Rating),
            new PropertyMetadata(false, OnStructureChanged));

    public static readonly DependencyProperty ActiveBrushProperty =
        DependencyProperty.Register(nameof(ActiveBrush), typeof(Brush), typeof(Rating),
            new PropertyMetadata(null, OnStructureChanged));

    public static readonly DependencyProperty InactiveBrushProperty =
        DependencyProperty.Register(nameof(InactiveBrush), typeof(Brush), typeof(Rating),
            new PropertyMetadata(null, OnStructureChanged));

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

    public bool IsReadOnly
    {
        get => (bool)GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    public bool ShowScore
    {
        get => (bool)GetValue(ShowScoreProperty);
        set => SetValue(ShowScoreProperty, value);
    }

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

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        _panel = GetTemplateChild("PART_Panel") as StackPanel;
        _scoreText = GetTemplateChild("PART_Score") as TextBlock;
        RebuildStars();
    }

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

    private void RebuildStars()
    {
        if (_panel == null) return;
        _panel.Children.Clear();

        var starGeometry = Geometry.Parse("M12,17.27L18.18,21L16.54,13.97L22,9.24L14.81,8.62L12,2L9.19,8.62L2,9.24L7.45,13.97L5.82,21L12,17.27Z");
        starGeometry.Freeze();

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

            // Background Star (Inactive)
            var bgPath = new Path
            {
                Data = starGeometry,
                Stretch = Stretch.Uniform,
                Fill = InactiveBrush ?? (Brush)FindResource("CONCOST.Brush.Border")
            };
            container.Children.Add(bgPath);

            // Foreground Star (Active filled with Clip)
            var fgPath = new Path
            {
                Data = starGeometry,
                Stretch = Stretch.Uniform,
                Fill = ActiveBrush ?? (Brush)FindResource("CONCOST.Brush.Warning")
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
                container.MouseEnter += (s, e) =>
                {
                    container.RenderTransform = new ScaleTransform(1.15, 1.15, ItemSize / 2, ItemSize / 2);
                };
                container.MouseLeave += (s, e) =>
                {
                    container.RenderTransform = Transform.Identity;
                };
                container.MouseLeftButtonDown += (s, e) =>
                {
                    Value = starIndex;
                };
            }

            _panel.Children.Add(container);
        }

        UpdateStarsFill();
    }

    private void UpdateStarsFill()
    {
        if (_panel == null) return;

        double val = Math.Max(0, Math.Min(Max, Value));
        for (int i = 0; i < _panel.Children.Count; i++)
        {
            if (_panel.Children[i] is Grid container && container.Children.Count >= 2 && container.Children[1] is Border clipBorder)
            {
                double fillRatio = Math.Max(0.0, Math.Min(1.0, val - i));
                clipBorder.Width = ItemSize * fillRatio;
            }
        }

        if (_scoreText != null)
        {
            _scoreText.Text = $"{Value:0.0} / {Max}";
            _scoreText.Visibility = ShowScore ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
