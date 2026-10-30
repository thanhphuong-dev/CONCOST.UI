using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CONCOST.UI.Controls.Ribbon;

/// <summary>
/// A set of tabs that only belong to one mode — “Drawing Tools” while a sketch is open,
/// “Table Tools” while the caret is inside a table.
/// </summary>
/// <remarks>
/// <para>
/// Drop one of these straight into <see cref="Ribbon.ItemsSource"/>; the ribbon expands it into its
/// child tabs and keeps them in the order you wrote them. Nothing else has to change.
/// </para>
/// <para>
/// The group stamps <see cref="RibbonTabItem.ContextualGroup"/> and
/// <see cref="RibbonTabItem.ContextualColor"/> onto its tabs, so the strip can merge consecutive
/// tabs of the same group under a single coloured band. <see cref="IsVisible"/> hides the whole
/// group without touching the item source.
/// </para>
/// </remarks>
public class RibbonContextualTabGroup : HeaderedItemsControl
{
    public static readonly DependencyProperty AccentProperty =
        DependencyProperty.Register(nameof(Accent), typeof(Brush), typeof(RibbonContextualTabGroup),
            new FrameworkPropertyMetadata(null, OnAccentChanged));

    static RibbonContextualTabGroup()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(RibbonContextualTabGroup),
            new FrameworkPropertyMetadata(typeof(RibbonContextualTabGroup)));

        // Visibility is the natural switch for “this mode is not active”: no extra boolean needed.
        VisibilityProperty.OverrideMetadata(typeof(RibbonContextualTabGroup),
            new FrameworkPropertyMetadata(Visibility.Visible, OnVisibilityChanged));
    }

    /// <summary>Raised when the set of visible tabs may have changed.</summary>
    public event EventHandler? TabsChanged;

    /// <summary>Band colour. Defaults to the primary accent when left null.</summary>
    public Brush? Accent
    {
        get => (Brush?)GetValue(AccentProperty);
        set => SetValue(AccentProperty, value);
    }

    /// <summary>
    /// Set to <see cref="Visibility.Collapsed"/> to hide every tab in the group — the way a
    /// contextual group disappears when its mode ends.
    /// </summary>
    public bool IsGroupVisible
    {
        get => Visibility == Visibility.Visible;
        set => Visibility = value ? Visibility.Visible : Visibility.Collapsed;
    }

    /// <summary>Tabs of this group, in declaration order.</summary>
    public IReadOnlyList<RibbonTabItem> Tabs => Items.OfType<RibbonTabItem>().ToList();

    protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
    {
        base.OnItemsChanged(e);
        Apply();
        TabsChanged?.Invoke(this, EventArgs.Empty);
    }

    protected override void OnInitialized(EventArgs e)
    {
        base.OnInitialized(e);
        Apply();
    }

    /// <summary>Pushes the group's caption, colour and visibility down to its tabs.</summary>
    private void Apply()
    {
        string caption = Header as string ?? string.Empty;
        bool visible = IsGroupVisible;

        foreach (RibbonTabItem tab in Items.OfType<RibbonTabItem>())
        {
            tab.ContextualGroup = caption;
            tab.ContextualColor = Accent;
            tab.IsContextual = true;
            tab.IsAvailable = visible;
        }
    }

    private static void OnAccentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is RibbonContextualTabGroup group) group.Apply();
    }

    private static void OnVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not RibbonContextualTabGroup group) return;

        group.Apply();
        group.TabsChanged?.Invoke(group, EventArgs.Empty);
    }
}
