using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CONCOST.UI.Icons;

namespace CONCOST.UI.Controls;

public class SideMenuItem : DependencyObject
{
    public static readonly DependencyProperty IconProperty =
        DependencyProperty.Register(nameof(Icon), typeof(IconKind), typeof(SideMenuItem), new PropertyMetadata(IconKind.Folder));

    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(nameof(Title), typeof(string), typeof(SideMenuItem), new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty BadgeProperty =
        DependencyProperty.Register(nameof(Badge), typeof(string), typeof(SideMenuItem),
            new PropertyMetadata(string.Empty, OnBadgeChanged));

    public static readonly DependencyProperty HasBadgeProperty =
        DependencyProperty.Register(nameof(HasBadge), typeof(bool), typeof(SideMenuItem), new PropertyMetadata(false));

    public static readonly DependencyProperty IsSelectedProperty =
        DependencyProperty.Register(nameof(IsSelected), typeof(bool), typeof(SideMenuItem), new PropertyMetadata(false));

    public static readonly DependencyProperty CommandProperty =
        DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(SideMenuItem), new PropertyMetadata(null));

    public static readonly DependencyProperty ChildrenProperty =
        DependencyProperty.Register(nameof(Children), typeof(ObservableCollection<SideMenuItem>), typeof(SideMenuItem), new PropertyMetadata(null));

    public IconKind Icon
    {
        get => (IconKind)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string Badge
    {
        get => (string)GetValue(BadgeProperty);
        set => SetValue(BadgeProperty, value);
    }

    public bool HasBadge
    {
        get => (bool)GetValue(HasBadgeProperty);
        private set => SetValue(HasBadgeProperty, value);
    }

    public bool IsSelected
    {
        get => (bool)GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

    private static void OnBadgeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SideMenuItem item)
        {
            item.HasBadge = !string.IsNullOrWhiteSpace(item.Badge);
        }
    }

    public ICommand? Command
    {
        get => (ICommand?)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public ObservableCollection<SideMenuItem> Children
    {
        get => (ObservableCollection<SideMenuItem>)GetValue(ChildrenProperty);
        set => SetValue(ChildrenProperty, value);
    }

    public SideMenuItem()
    {
        Children = new ObservableCollection<SideMenuItem>();
    }

    public SideMenuItem(string title, IconKind icon, string badge = "") : this()
    {
        Title = title;
        Icon = icon;
        Badge = badge;
        HasBadge = !string.IsNullOrWhiteSpace(badge);
    }
}

public class SideMenu : Control
{
    static SideMenu()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(SideMenu), new FrameworkPropertyMetadata(typeof(SideMenu)));
    }

    public static readonly DependencyProperty ItemsProperty =
        DependencyProperty.Register(nameof(Items), typeof(ObservableCollection<SideMenuItem>), typeof(SideMenu),
            new PropertyMetadata(null));

    public static readonly DependencyProperty SelectedItemProperty =
        DependencyProperty.Register(nameof(SelectedItem), typeof(SideMenuItem), typeof(SideMenu),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedItemChanged));

    public static readonly DependencyProperty IsCollapsedProperty =
        DependencyProperty.Register(nameof(IsCollapsed), typeof(bool), typeof(SideMenu),
            new PropertyMetadata(false));

    public static readonly DependencyProperty HeaderProperty =
        DependencyProperty.Register(nameof(Header), typeof(object), typeof(SideMenu),
            new PropertyMetadata(null));

    public static readonly DependencyProperty FooterProperty =
        DependencyProperty.Register(nameof(Footer), typeof(object), typeof(SideMenu),
            new PropertyMetadata(null));

    public static readonly RoutedEvent SelectionChangedEvent =
        EventManager.RegisterRoutedEvent(nameof(SelectionChanged), RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<SideMenuItem?>), typeof(SideMenu));

    public event RoutedPropertyChangedEventHandler<SideMenuItem?> SelectionChanged
    {
        add => AddHandler(SelectionChangedEvent, value);
        remove => RemoveHandler(SelectionChangedEvent, value);
    }

    public ObservableCollection<SideMenuItem> Items
    {
        get => (ObservableCollection<SideMenuItem>)GetValue(ItemsProperty);
        set => SetValue(ItemsProperty, value);
    }

    public SideMenuItem? SelectedItem
    {
        get => (SideMenuItem?)GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    public bool IsCollapsed
    {
        get => (bool)GetValue(IsCollapsedProperty);
        set => SetValue(IsCollapsedProperty, value);
    }

    public object? Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    public object? Footer
    {
        get => GetValue(FooterProperty);
        set => SetValue(FooterProperty, value);
    }

    public SideMenu()
    {
        Items = new ObservableCollection<SideMenuItem>();
    }

    private static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SideMenu menu)
        {
            if (e.OldValue is SideMenuItem oldItem) oldItem.IsSelected = false;
            if (e.NewValue is SideMenuItem newItem) newItem.IsSelected = true;
            menu.RaiseEvent(new RoutedPropertyChangedEventArgs<SideMenuItem?>((SideMenuItem?)e.OldValue, (SideMenuItem?)e.NewValue, SelectionChangedEvent));
        }
    }
}
