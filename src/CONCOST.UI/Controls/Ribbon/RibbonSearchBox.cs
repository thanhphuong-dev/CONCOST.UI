using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CONCOST.UI.Controls.Ribbon;

/// <summary>
/// A ribbon-sized search field, e.g. the “Tell me what you want to do” box in the Office header.
/// <para>
/// A thin shell around <see cref="Controls.SearchBox"/>: the placeholder, the instant-search
/// behaviour and the clear button all come from the base control. Give it
/// <see cref="RibbonItem.ColumnSpan"/> = 2 or 3 so the query has room.
/// </para>
/// </summary>
public class RibbonSearchBox : RibbonItem
{
    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(nameof(Text), typeof(string), typeof(RibbonSearchBox),
            new FrameworkPropertyMetadata(string.Empty,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public static readonly DependencyProperty PlaceholderProperty =
        DependencyProperty.Register(nameof(Placeholder), typeof(string), typeof(RibbonSearchBox),
            new FrameworkPropertyMetadata("Search"));

    public static readonly DependencyProperty IsInstantSearchProperty =
        DependencyProperty.Register(nameof(IsInstantSearch), typeof(bool), typeof(RibbonSearchBox),
            new FrameworkPropertyMetadata(true));

    public static readonly DependencyProperty SearchCommandProperty =
        DependencyProperty.Register(nameof(SearchCommand), typeof(ICommand), typeof(RibbonSearchBox),
            new FrameworkPropertyMetadata(null));

    static RibbonSearchBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(RibbonSearchBox),
            new FrameworkPropertyMetadata(typeof(RibbonSearchBox)));
    }

    public RibbonSearchBox()
    {
        // The inner SearchBox owns the caret and the tab stop.
        Focusable = false;
        IsTabStop = false;
    }

    /// <summary>Current query.</summary>
    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    /// <summary>Hint shown while the query is empty.</summary>
    public string Placeholder
    {
        get => (string)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    /// <summary>Runs <see cref="SearchCommand"/> on every keystroke instead of only on Enter.</summary>
    public bool IsInstantSearch
    {
        get => (bool)GetValue(IsInstantSearchProperty);
        set => SetValue(IsInstantSearchProperty, value);
    }

    /// <summary>Command invoked with the query text.</summary>
    public ICommand? SearchCommand
    {
        get => (ICommand?)GetValue(SearchCommandProperty);
        set => SetValue(SearchCommandProperty, value);
    }

    /// <summary>Moves the caret into the field and selects the query.</summary>
    public override void PerformClick()
    {
        if (!IsEnabled) return;

        if (GetTemplateChild("Editor") is Controls.SearchBox editor)
        {
            editor.Focus();
        }
    }
}
