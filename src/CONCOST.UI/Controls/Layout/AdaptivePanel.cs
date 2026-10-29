using System.Windows;
using System.Windows.Controls;

namespace CONCOST.UI.Controls.Layout;

public enum LayoutMode
{
    Compact,
    Normal,
    Wide
}

public class Breakpoint
{
    public double MinWidth { get; set; }
    public LayoutMode Layout { get; set; }

    public Breakpoint(double minWidth, LayoutMode layout)
    {
        MinWidth = minWidth;
        Layout = layout;
    }
}

public class AdaptivePanel : ContentControl
{
    static AdaptivePanel()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(AdaptivePanel), new FrameworkPropertyMetadata(typeof(AdaptivePanel)));
    }

    public static readonly DependencyProperty CurrentLayoutProperty =
        DependencyProperty.Register(nameof(CurrentLayout), typeof(LayoutMode), typeof(AdaptivePanel),
            new FrameworkPropertyMetadata(LayoutMode.Normal, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public LayoutMode CurrentLayout
    {
        get => (LayoutMode)GetValue(CurrentLayoutProperty);
        set => SetValue(CurrentLayoutProperty, value);
    }

    public List<Breakpoint> Breakpoints { get; } = new()
    {
        new(0, LayoutMode.Compact),
        new(900, LayoutMode.Normal),
        new(1400, LayoutMode.Wide)
    };

    public AdaptivePanel()
    {
        SizeChanged += OnAdaptiveSizeChanged;
    }

    private void OnAdaptiveSizeChanged(object sender, SizeChangedEventArgs e)
    {
        double width = e.NewSize.Width;
        var matched = Breakpoints
            .Where(b => width >= b.MinWidth)
            .OrderByDescending(b => b.MinWidth)
            .FirstOrDefault();

        if (matched != null && matched.Layout != CurrentLayout)
        {
            CurrentLayout = matched.Layout;
        }
    }
}
