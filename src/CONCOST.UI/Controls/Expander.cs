using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace CONCOST.UI.Controls;

public class Expander : HeaderedContentControl
{
    private Button? _headerButton;
    static Expander()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(Expander), new FrameworkPropertyMetadata(typeof(Expander)));
    }

    public static readonly DependencyProperty IsExpandedProperty =
        DependencyProperty.Register(nameof(IsExpanded), typeof(bool), typeof(Expander),
            new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public static readonly DependencyProperty ExpandDirectionProperty =
        DependencyProperty.Register(nameof(ExpandDirection), typeof(ExpandDirection), typeof(Expander),
            new PropertyMetadata(ExpandDirection.Down));

    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(Expander),
            new PropertyMetadata(new CornerRadius(8)));

    public bool IsExpanded
    {
        get => (bool)GetValue(IsExpandedProperty);
        set => SetValue(IsExpandedProperty, value);
    }

    public ExpandDirection ExpandDirection
    {
        get => (ExpandDirection)GetValue(ExpandDirectionProperty);
        set => SetValue(ExpandDirectionProperty, value);
    }

    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    public override void OnApplyTemplate()
    {
        if (_headerButton != null) _headerButton.Click -= OnHeaderButtonClick;
        base.OnApplyTemplate();
        _headerButton = GetTemplateChild("PART_HeaderButton") as Button;
        if (_headerButton != null) _headerButton.Click += OnHeaderButtonClick;
    }

    private void OnHeaderButtonClick(object sender, RoutedEventArgs e) => IsExpanded = !IsExpanded;
}
