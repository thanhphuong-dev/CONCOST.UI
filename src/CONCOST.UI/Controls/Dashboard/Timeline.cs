using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CONCOST.UI.Icons;

namespace CONCOST.UI.Controls.Dashboard;

public enum TimelineItemType
{
    Default,
    Primary,
    Success,
    Warning,
    Danger,
    Info
}

public class TimelineItem : HeaderedContentControl
{
    static TimelineItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(TimelineItem), new FrameworkPropertyMetadata(typeof(TimelineItem)));
    }

    public static readonly DependencyProperty TimeProperty =
        DependencyProperty.Register(nameof(Time), typeof(string), typeof(TimelineItem), new PropertyMetadata(string.Empty));

    public string Time
    {
        get => (string)GetValue(TimeProperty);
        set => SetValue(TimeProperty, value);
    }

    public static readonly DependencyProperty DescriptionProperty =
        DependencyProperty.Register(nameof(Description), typeof(string), typeof(TimelineItem), new PropertyMetadata(string.Empty));

    public string Description
    {
        get => (string)GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    public static readonly DependencyProperty IconProperty =
        DependencyProperty.Register(nameof(Icon), typeof(IconKind), typeof(TimelineItem), new PropertyMetadata(IconKind.None));

    public IconKind Icon
    {
        get => (IconKind)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public static readonly DependencyProperty TypeProperty =
        DependencyProperty.Register(nameof(Type), typeof(TimelineItemType), typeof(TimelineItem), new PropertyMetadata(TimelineItemType.Default));

    public TimelineItemType Type
    {
        get => (TimelineItemType)GetValue(TypeProperty);
        set => SetValue(TypeProperty, value);
    }

    public static readonly DependencyProperty DotBrushProperty =
        DependencyProperty.Register(nameof(DotBrush), typeof(Brush), typeof(TimelineItem), new PropertyMetadata(null));

    public Brush? DotBrush
    {
        get => (Brush?)GetValue(DotBrushProperty);
        set => SetValue(DotBrushProperty, value);
    }

    public static readonly DependencyProperty IsLastProperty =
        DependencyProperty.Register(nameof(IsLast), typeof(bool), typeof(TimelineItem), new PropertyMetadata(false));

    public bool IsLast
    {
        get => (bool)GetValue(IsLastProperty);
        set => SetValue(IsLastProperty, value);
    }
}

public class Timeline : ItemsControl
{
    static Timeline()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(Timeline), new FrameworkPropertyMetadata(typeof(Timeline)));
    }

    protected override bool IsItemItsOwnContainerOverride(object item) => item is TimelineItem;

    protected override DependencyObject GetContainerForItemOverride() => new TimelineItem();

    protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
    {
        base.PrepareContainerForItemOverride(element, item);
        UpdateLastItemFlag();
    }

    protected override void OnItemsChanged(System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        base.OnItemsChanged(e);
        UpdateLastItemFlag();
    }

    private void UpdateLastItemFlag()
    {
        for (int i = 0; i < Items.Count; i++)
        {
            if (ItemContainerGenerator.ContainerFromIndex(i) is TimelineItem tItem)
            {
                tItem.IsLast = (i == Items.Count - 1);
            }
        }
    }
}
