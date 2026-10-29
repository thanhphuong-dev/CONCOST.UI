using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using CONCOST.UI.Icons;

namespace CONCOST.UI.Controls;

public class DrawerMenuItem : DependencyObject
{
    public static readonly DependencyProperty IconProperty =
        DependencyProperty.Register(nameof(Icon), typeof(IconKind), typeof(DrawerMenuItem), new PropertyMetadata(IconKind.Folder));

    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(nameof(Text), typeof(string), typeof(DrawerMenuItem), new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty BadgeProperty =
        DependencyProperty.Register(nameof(Badge), typeof(string), typeof(DrawerMenuItem),
            new PropertyMetadata(string.Empty, OnBadgeChanged));

    public static readonly DependencyProperty HasBadgeProperty =
        DependencyProperty.Register(nameof(HasBadge), typeof(bool), typeof(DrawerMenuItem), new PropertyMetadata(false));

    public static readonly DependencyProperty IsSelectedProperty =
        DependencyProperty.Register(nameof(IsSelected), typeof(bool), typeof(DrawerMenuItem), new PropertyMetadata(false));

    public static readonly DependencyProperty TagProperty =
        DependencyProperty.Register(nameof(Tag), typeof(object), typeof(DrawerMenuItem), new PropertyMetadata(null));

    public static readonly DependencyProperty CommandProperty =
        DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(DrawerMenuItem), new PropertyMetadata(null));

    public static readonly DependencyProperty ChildrenProperty =
        DependencyProperty.Register(nameof(Children), typeof(ObservableCollection<DrawerMenuItem>), typeof(DrawerMenuItem), new PropertyMetadata(null));

    public IconKind Icon
    {
        get => (IconKind)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
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

    public object? Tag
    {
        get => GetValue(TagProperty);
        set => SetValue(TagProperty, value);
    }

    public ICommand? Command
    {
        get => (ICommand?)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public ObservableCollection<DrawerMenuItem> Children
    {
        get => (ObservableCollection<DrawerMenuItem>)GetValue(ChildrenProperty);
        set => SetValue(ChildrenProperty, value);
    }

    public DrawerMenuItem()
    {
        Children = new ObservableCollection<DrawerMenuItem>();
    }

    public DrawerMenuItem(string text, IconKind icon, string badge = "", object? tag = null) : this()
    {
        Text = text;
        Icon = icon;
        Badge = badge;
        HasBadge = !string.IsNullOrWhiteSpace(badge);
        Tag = tag;
    }

    private static void OnBadgeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DrawerMenuItem item)
        {
            item.HasBadge = !string.IsNullOrWhiteSpace(item.Badge);
        }
    }
}

/// <summary>
/// DrawerMenu navigation control inspired by WPFDevelopers.
/// Features smooth animated collapse/expand, mask overlay, header/footer branding, search filter, and menu item selection.
/// </summary>
public class DrawerMenu : HeaderedItemsControl
{
    static DrawerMenu()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(DrawerMenu), new FrameworkPropertyMetadata(typeof(DrawerMenu)));
    }

    public static readonly DependencyProperty IsOpenProperty =
        DependencyProperty.Register(nameof(IsOpen), typeof(bool), typeof(DrawerMenu),
            new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnIsOpenChanged));

    public static readonly DependencyProperty CompactWidthProperty =
        DependencyProperty.Register(nameof(CompactWidth), typeof(double), typeof(DrawerMenu),
            new PropertyMetadata(64.0));

    public static readonly DependencyProperty ExpandedWidthProperty =
        DependencyProperty.Register(nameof(ExpandedWidth), typeof(double), typeof(DrawerMenu),
            new PropertyMetadata(240.0));

    public static readonly DependencyProperty SelectedItemProperty =
        DependencyProperty.Register(nameof(SelectedItem), typeof(DrawerMenuItem), typeof(DrawerMenu),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedItemChanged));

    public static readonly DependencyProperty FooterProperty =
        DependencyProperty.Register(nameof(Footer), typeof(object), typeof(DrawerMenu),
            new PropertyMetadata(null));

    public static readonly DependencyProperty ShowToggleButtonProperty =
        DependencyProperty.Register(nameof(ShowToggleButton), typeof(bool), typeof(DrawerMenu),
            new PropertyMetadata(true));

    public static readonly RoutedEvent SelectionChangedEvent =
        EventManager.RegisterRoutedEvent(nameof(SelectionChanged), RoutingStrategy.Bubble,
            typeof(RoutedPropertyChangedEventHandler<DrawerMenuItem?>), typeof(DrawerMenu));

    public event RoutedPropertyChangedEventHandler<DrawerMenuItem?> SelectionChanged
    {
        add => AddHandler(SelectionChangedEvent, value);
        remove => RemoveHandler(SelectionChangedEvent, value);
    }

    public static readonly RoutedEvent IsOpenChangedEvent =
        EventManager.RegisterRoutedEvent(nameof(IsOpenChanged), RoutingStrategy.Bubble,
            typeof(RoutedPropertyChangedEventHandler<bool>), typeof(DrawerMenu));

    public event RoutedPropertyChangedEventHandler<bool> IsOpenChanged
    {
        add => AddHandler(IsOpenChangedEvent, value);
        remove => RemoveHandler(IsOpenChangedEvent, value);
    }

    public bool IsOpen
    {
        get => (bool)GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    public double CompactWidth
    {
        get => (double)GetValue(CompactWidthProperty);
        set => SetValue(CompactWidthProperty, value);
    }

    public double ExpandedWidth
    {
        get => (double)GetValue(ExpandedWidthProperty);
        set => SetValue(ExpandedWidthProperty, value);
    }

    public DrawerMenuItem? SelectedItem
    {
        get => (DrawerMenuItem?)GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    public object? Footer
    {
        get => GetValue(FooterProperty);
        set => SetValue(FooterProperty, value);
    }

    public bool ShowToggleButton
    {
        get => (bool)GetValue(ShowToggleButtonProperty);
        set => SetValue(ShowToggleButtonProperty, value);
    }

    public DrawerMenu()
    {
        Loaded += OnLoaded;
        AddHandler(RadioButton.CheckedEvent, new RoutedEventHandler(OnRadioButtonChecked));
    }

    private void OnRadioButtonChecked(object sender, RoutedEventArgs e)
    {
        if (e.OriginalSource is FrameworkElement fe && fe.DataContext is DrawerMenuItem item)
        {
            if (!ReferenceEquals(SelectedItem, item))
            {
                SelectedItem = item;
            }
        }
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        Width = IsOpen ? ExpandedWidth : CompactWidth;
        SynchronizeInitialSelection();
    }

    protected override void OnItemsSourceChanged(System.Collections.IEnumerable oldValue, System.Collections.IEnumerable newValue)
    {
        base.OnItemsSourceChanged(oldValue, newValue);
        SynchronizeInitialSelection();
    }

    private void SynchronizeInitialSelection()
    {
        if (SelectedItem == null && ItemsSource != null)
        {
            foreach (var obj in ItemsSource)
            {
                if (obj is DrawerMenuItem item && item.IsSelected)
                {
                    SelectedItem = item;
                    break;
                }
            }
        }
        else if (SelectedItem != null)
        {
            SelectedItem.IsSelected = true;
        }
    }

    private static void OnIsOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DrawerMenu menu)
        {
            bool isOpen = (bool)e.NewValue;
            double targetWidth = isOpen ? menu.ExpandedWidth : menu.CompactWidth;

            if (menu.IsLoaded)
            {
                var anim = new DoubleAnimation(targetWidth, TimeSpan.FromMilliseconds(250))
                {
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                };
                menu.BeginAnimation(FrameworkElement.WidthProperty, anim);
            }
            else
            {
                menu.Width = targetWidth;
            }

            var args = new RoutedPropertyChangedEventArgs<bool>((bool)e.OldValue, isOpen)
            {
                RoutedEvent = IsOpenChangedEvent
            };
            menu.RaiseEvent(args);
        }
    }

    private static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DrawerMenu menu)
        {
            if (e.OldValue is DrawerMenuItem oldItem) oldItem.IsSelected = false;
            if (e.NewValue is DrawerMenuItem newItem)
            {
                newItem.IsSelected = true;
                newItem.Command?.Execute(newItem.Tag);
            }
            var args = new RoutedPropertyChangedEventArgs<DrawerMenuItem?>((DrawerMenuItem?)e.OldValue, (DrawerMenuItem?)e.NewValue)
            {
                RoutedEvent = SelectionChangedEvent
            };
            menu.RaiseEvent(args);
        }
    }
}
