using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CONCOST.UI.Core.Helpers;

namespace CONCOST.UI.AttachedProperties;

/// <summary>
/// Provides attached properties and routed commands for TabControl and TabItem,
/// including closable tabs, tab close commands, and header action hosts.
/// </summary>
public static class TabControlHelper
{
    #region IsClosable

    public static readonly DependencyProperty IsClosableProperty =
        DependencyProperty.RegisterAttached(
            "IsClosable",
            typeof(bool),
            typeof(TabControlHelper),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.Inherits));

    public static bool GetIsClosable(DependencyObject obj) => (bool)obj.GetValue(IsClosableProperty);
    public static void SetIsClosable(DependencyObject obj, bool value) => obj.SetValue(IsClosableProperty, value);

    #endregion

    #region HeaderExtraContent

    public static readonly DependencyProperty HeaderExtraContentProperty =
        DependencyProperty.RegisterAttached(
            "HeaderExtraContent",
            typeof(object),
            typeof(TabControlHelper),
            new FrameworkPropertyMetadata(null));

    public static object? GetHeaderExtraContent(DependencyObject obj) => obj.GetValue(HeaderExtraContentProperty);
    public static void SetHeaderExtraContent(DependencyObject obj, object? value) => obj.SetValue(HeaderExtraContentProperty, value);

    #endregion

    #region Routed Commands

    private static RoutedUICommand? _closeCommand;
    public static RoutedUICommand CloseCommand =>
        _closeCommand ??= new RoutedUICommand("Close Tab", "CloseTab", typeof(TabControlHelper));

    private static RoutedUICommand? _addCommand;
    public static RoutedUICommand AddCommand =>
        _addCommand ??= new RoutedUICommand("Add Tab", "AddTab", typeof(TabControlHelper));

    #endregion

    #region Routed Events

    public static readonly RoutedEvent TabClosingEvent =
        EventManager.RegisterRoutedEvent(
            "TabClosing",
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(TabControlHelper));

    public static readonly RoutedEvent TabClosedEvent =
        EventManager.RegisterRoutedEvent(
            "TabClosed",
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(TabControlHelper));

    public static void AddTabClosingHandler(DependencyObject d, RoutedEventHandler handler)
    {
        if (d is UIElement uie) uie.AddHandler(TabClosingEvent, handler);
    }
    public static void RemoveTabClosingHandler(DependencyObject d, RoutedEventHandler handler)
    {
        if (d is UIElement uie) uie.RemoveHandler(TabClosingEvent, handler);
    }

    public static void AddTabClosedHandler(DependencyObject d, RoutedEventHandler handler)
    {
        if (d is UIElement uie) uie.AddHandler(TabClosedEvent, handler);
    }
    public static void RemoveTabClosedHandler(DependencyObject d, RoutedEventHandler handler)
    {
        if (d is UIElement uie) uie.RemoveHandler(TabClosedEvent, handler);
    }

    #endregion

    static TabControlHelper()
    {
        // Register class command binding on TabItem so any Button inside TabItem template
        // with Command="attached:TabControlHelper.CloseCommand" can automatically close its TabItem.
        CommandManager.RegisterClassCommandBinding(
            typeof(TabItem),
            new CommandBinding(CloseCommand, OnCloseTabExecuted, OnCloseTabCanExecute));
    }

    private static void OnCloseTabCanExecute(object sender, CanExecuteRoutedEventArgs e)
    {
        if (sender is TabItem tabItem && GetIsClosable(tabItem))
        {
            var tabControl = VisualTreeHelperEx.FindParent<TabControl>(tabItem);
            if (tabControl?.ItemsSource is IList source)
                e.CanExecute = !source.IsReadOnly && !source.IsFixedSize;
            else
                e.CanExecute = tabControl?.Items.Contains(tabItem) == true;
        }
        e.Handled = true;
    }

    private static void OnCloseTabExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        if (sender is not TabItem tabItem || !GetIsClosable(tabItem)) return;

        var tabControl = VisualTreeHelperEx.FindParent<TabControl>(tabItem);
        if (tabControl == null) return;

        var closingArgs = new RoutedEventArgs(TabClosingEvent, tabItem);
        tabControl.RaiseEvent(closingArgs);
        if (closingArgs.Handled) return;

        // If data-bound via ItemsSource
        if (tabControl.ItemsSource is IList list && !list.IsReadOnly && !list.IsFixedSize)
        {
            var itemToRemove = tabItem.DataContext ?? tabItem;
            if (list.Contains(itemToRemove))
            {
                list.Remove(itemToRemove);
                tabControl.RaiseEvent(new RoutedEventArgs(TabClosedEvent, tabItem));
                e.Handled = true;
                return;
            }
        }

        // Direct Items collection
        if (tabControl.Items.Contains(tabItem))
        {
            tabControl.Items.Remove(tabItem);
            tabControl.RaiseEvent(new RoutedEventArgs(TabClosedEvent, tabItem));
            e.Handled = true;
        }
    }
}
