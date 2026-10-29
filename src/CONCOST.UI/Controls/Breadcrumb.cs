using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CONCOST.UI.Icons;

namespace CONCOST.UI.Controls;

public class BreadcrumbItem : DependencyObject
{
    public static readonly DependencyProperty HeaderProperty =
        DependencyProperty.Register(nameof(Header), typeof(string), typeof(BreadcrumbItem), new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty IconProperty =
        DependencyProperty.Register(nameof(Icon), typeof(IconKind), typeof(BreadcrumbItem), new PropertyMetadata(IconKind.None));

    public static readonly DependencyProperty IsSelectedProperty =
        DependencyProperty.Register(nameof(IsSelected), typeof(bool), typeof(BreadcrumbItem), new PropertyMetadata(false));

    public static readonly DependencyProperty CommandProperty =
        DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(BreadcrumbItem), new PropertyMetadata(null));

    public static readonly DependencyProperty TagProperty =
        DependencyProperty.Register(nameof(Tag), typeof(object), typeof(BreadcrumbItem), new PropertyMetadata(null));

    public string Header
    {
        get => (string)GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

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

    public ICommand? Command
    {
        get => (ICommand?)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public object? Tag
    {
        get => GetValue(TagProperty);
        set => SetValue(TagProperty, value);
    }
}

public class Breadcrumb : Control
{
    public static readonly DependencyProperty ItemsProperty =
        DependencyProperty.Register(nameof(Items), typeof(ObservableCollection<BreadcrumbItem>), typeof(Breadcrumb),
            new PropertyMetadata(null));

    public static readonly DependencyProperty SeparatorIconProperty =
        DependencyProperty.Register(nameof(SeparatorIcon), typeof(IconKind), typeof(Breadcrumb),
            new PropertyMetadata(IconKind.ChevronRight));

    public ObservableCollection<BreadcrumbItem> Items
    {
        get => (ObservableCollection<BreadcrumbItem>)GetValue(ItemsProperty);
        set => SetValue(ItemsProperty, value);
    }

    public IconKind SeparatorIcon
    {
        get => (IconKind)GetValue(SeparatorIconProperty);
        set => SetValue(SeparatorIconProperty, value);
    }

    static Breadcrumb()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(Breadcrumb), new FrameworkPropertyMetadata(typeof(Breadcrumb)));
    }

    public Breadcrumb()
    {
        Items = new ObservableCollection<BreadcrumbItem>();
    }
}
