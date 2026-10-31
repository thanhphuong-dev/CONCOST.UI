using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CONCOST.UI.Icons;

namespace CONCOST.UI.Controls;

/// <summary>
/// One document tab: icon, title, a dirty dot and a close button.
/// </summary>
/// <remarks>
/// The tab does not remove itself. <see cref="Close"/> raises the bubbling
/// <see cref="CloseRequestedEvent"/> and <see cref="DocumentTabControl"/> decides what that means —
/// which is what lets a host ask "save before closing?" without subclassing the tab.
/// </remarks>
public class DocumentTabItem : TabItem
{
    static DocumentTabItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(DocumentTabItem),
            new FrameworkPropertyMetadata(typeof(DocumentTabItem)));
    }

    public DocumentTabItem()
    {
        Focusable = true;
    }

    // ---------------------------------------------------------------- state

    public static readonly DependencyProperty IconProperty =
        DependencyProperty.Register(nameof(Icon), typeof(IconKind), typeof(DocumentTabItem),
            new PropertyMetadata(IconKind.None));

    /// <summary>Glyph shown before the title.</summary>
    public IconKind Icon
    {
        get => (IconKind)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public static readonly DependencyProperty IsDirtyProperty =
        DependencyProperty.Register(nameof(IsDirty), typeof(bool), typeof(DocumentTabItem),
            new PropertyMetadata(false));

    /// <summary>Shows the unsaved dot. The control never clears it — saving is the host's business.</summary>
    public bool IsDirty
    {
        get => (bool)GetValue(IsDirtyProperty);
        set => SetValue(IsDirtyProperty, value);
    }

    public static readonly DependencyProperty CanCloseProperty =
        DependencyProperty.Register(nameof(CanClose), typeof(bool), typeof(DocumentTabItem),
            new PropertyMetadata(true));

    /// <summary>Show the close button. Default <c>true</c>.</summary>
    public bool CanClose
    {
        get => (bool)GetValue(CanCloseProperty);
        set => SetValue(CanCloseProperty, value);
    }

    public static readonly DependencyProperty CloseCommandProperty =
        DependencyProperty.Register(nameof(CloseCommand), typeof(ICommand), typeof(DocumentTabItem),
            new PropertyMetadata(null));

    /// <summary>
    /// Optional close command. When set, <see cref="DocumentTabControl"/> runs it instead of removing
    /// the tab, so a host can cancel or prompt first.
    /// </summary>
    public ICommand? CloseCommand
    {
        get => (ICommand?)GetValue(CloseCommandProperty);
        set => SetValue(CloseCommandProperty, value);
    }

    // ---------------------------------------------------------------- close plumbing

    /// <summary>Raised when the close button is pressed. Bubbles to the owning tab control.</summary>
    public static readonly RoutedEvent CloseRequestedEvent =
        EventManager.RegisterRoutedEvent(nameof(CloseRequested), RoutingStrategy.Bubble,
            typeof(RoutedEventHandler), typeof(DocumentTabItem));

    /// <summary>Raised when the close button is pressed.</summary>
    public event RoutedEventHandler CloseRequested
    {
        add => AddHandler(CloseRequestedEvent, value);
        remove => RemoveHandler(CloseRequestedEvent, value);
    }

    /// <summary>Asks to close this tab.</summary>
    public void Close() => RaiseEvent(new RoutedEventArgs(CloseRequestedEvent, this));

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        if (GetTemplateChild("CloseButton") is Button button)
        {
            button.Click -= OnCloseButtonClick;
            button.Click += OnCloseButtonClick;
        }
    }

    private void OnCloseButtonClick(object sender, RoutedEventArgs e)
    {
        e.Handled = true;
        Close();
    }
}
