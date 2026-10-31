using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace CONCOST.UI.Controls;

/// <summary>
/// A tab control for documents: every tab can be closed, and closing goes through the tab's
/// <see cref="DocumentTabItem.CloseCommand"/> when it has one.
/// </summary>
/// <remarks>
/// <para>
/// Closing is a request, not a removal. A tab with a <c>CloseCommand</c> lets the host prompt for
/// unsaved changes and cancel; a tab without one is simply removed from <see cref="Items"/>.
/// </para>
/// <para>
/// The three helpers — <see cref="CloseTab"/>, <see cref="CloseOtherTabs"/>, <see cref="CloseAllTabs"/>
/// — go through the same path, so a context menu behaves exactly like the close button.
/// </para>
/// </remarks>
public class DocumentTabControl : TabControl
{
    static DocumentTabControl()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(DocumentTabControl),
            new FrameworkPropertyMetadata(typeof(DocumentTabControl)));

        // One handler for every tab: the close request bubbles here.
        EventManager.RegisterClassHandler(typeof(DocumentTabControl),
            DocumentTabItem.CloseRequestedEvent, new RoutedEventHandler(OnTabCloseRequested));
    }

    public static readonly DependencyProperty ShowCloseButtonsProperty =
        DependencyProperty.Register(nameof(ShowCloseButtons), typeof(bool), typeof(DocumentTabControl),
            new PropertyMetadata(true));

    /// <summary>Master switch for the close buttons. Default <c>true</c>.</summary>
    public bool ShowCloseButtons
    {
        get => (bool)GetValue(ShowCloseButtonsProperty);
        set => SetValue(ShowCloseButtonsProperty, value);
    }

    /// <summary>The tabs, in order.</summary>
    public IEnumerable<DocumentTabItem> Documents => Items.OfType<DocumentTabItem>();

    /// <summary>Closes one tab, honouring its <see cref="DocumentTabItem.CloseCommand"/>.</summary>
    public void CloseTab(object? item)
    {
        if (item is null) return;

        if (item is DocumentTabItem tab)
        {
            if (!tab.CanClose) return;

            if (tab.CloseCommand is { } command)
            {
                if (command.CanExecute(tab)) command.Execute(tab);
                return;
            }
        }

        Items.Remove(item);
    }

    /// <summary>Closes every tab except <paramref name="keep"/>.</summary>
    public void CloseOtherTabs(object? keep)
    {
        foreach (var tab in Documents.Where(t => !ReferenceEquals(t, keep)).ToList())
        {
            CloseTab(tab);
        }
    }

    /// <summary>Closes every tab.</summary>
    public void CloseAllTabs()
    {
        foreach (var tab in Documents.ToList())
        {
            CloseTab(tab);
        }
    }

    private static void OnTabCloseRequested(object sender, RoutedEventArgs e)
    {
        if (sender is not DocumentTabControl control) return;
        if (e.OriginalSource is not DocumentTabItem tab) return;

        control.CloseTab(tab);
        e.Handled = true;
    }
}
