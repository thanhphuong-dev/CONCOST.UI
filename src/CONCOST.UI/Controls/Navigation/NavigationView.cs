using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using CONCOST.UI.Icons;

namespace CONCOST.UI.Controls.Navigation;

public class NavigationItem : HeaderedContentControl
{
    static NavigationItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(NavigationItem), new FrameworkPropertyMetadata(typeof(NavigationItem)));
    }

    public static readonly DependencyProperty IconProperty =
        DependencyProperty.Register(nameof(Icon), typeof(IconKind), typeof(NavigationItem),
            new PropertyMetadata(IconKind.None));

    public static readonly DependencyProperty IsSelectedProperty =
        DependencyProperty.Register(nameof(IsSelected), typeof(bool), typeof(NavigationItem),
            new PropertyMetadata(false));

    public static readonly DependencyProperty TagNameProperty =
        DependencyProperty.Register(nameof(TagName), typeof(string), typeof(NavigationItem),
            new PropertyMetadata(string.Empty));

    public IconKind Icon
    {
        get => (IconKind)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public bool IsSelected
    {
        get => (bool)GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

    public string TagName
    {
        get => (string)GetValue(TagNameProperty);
        set => SetValue(TagNameProperty, value);
    }
}

public class NavigationView : HeaderedItemsControl
{
    static NavigationView()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(NavigationView), new FrameworkPropertyMetadata(typeof(NavigationView)));
    }

    public static readonly DependencyProperty SelectedItemProperty =
        DependencyProperty.Register(nameof(SelectedItem), typeof(object), typeof(NavigationView),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public object? SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }
}
