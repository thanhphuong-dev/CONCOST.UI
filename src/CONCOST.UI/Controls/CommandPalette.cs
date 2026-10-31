using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace CONCOST.UI.Controls;

/// <summary>Raised when a <see cref="CommandPalette"/> runs an entry.</summary>
public class CommandPaletteInvokedEventArgs : EventArgs
{
    public CommandPaletteInvokedEventArgs(CommandPaletteItem item) => Item = item;

    /// <summary>The entry that was run.</summary>
    public CommandPaletteItem Item { get; }
}

/// <summary>
/// The Ctrl+K / command-bar overlay: a search box over a filtered list of commands, driven
/// entirely by the keyboard.
/// </summary>
/// <remarks>
/// <para>
/// Drop it into a <see cref="Grid"/> that covers the area it should dim, bind <see cref="ItemsSource"/>
/// and toggle <see cref="IsOpen"/>. The control draws its own scrim, so the host does not need a popup.
/// </para>
/// <para>
/// Search matches every token independently against the title, subtitle, category and shortcut, and
/// ignores diacritics — a Vietnamese-first library where typing <c>tong quan</c> has to find
/// "Tổng quan". Selection follows the filtered list, so <c>↑</c> / <c>↓</c> never land on a row that
/// was filtered away.
/// </para>
/// </remarks>
public class CommandPalette : Control
{
    static CommandPalette()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(CommandPalette),
            new FrameworkPropertyMetadata(typeof(CommandPalette)));
    }

    public static readonly DependencyProperty IsOpenProperty =
        DependencyProperty.Register(nameof(IsOpen), typeof(bool), typeof(CommandPalette),
            new FrameworkPropertyMetadata(false,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnIsOpenChanged));

    public static readonly DependencyProperty ItemsSourceProperty =
        DependencyProperty.Register(nameof(ItemsSource), typeof(IEnumerable), typeof(CommandPalette),
            new PropertyMetadata(null, OnItemsSourceChanged));

    public static readonly DependencyProperty FilterTextProperty =
        DependencyProperty.Register(nameof(FilterText), typeof(string), typeof(CommandPalette),
            new FrameworkPropertyMetadata(string.Empty, OnFilterTextChanged));

    public static readonly DependencyProperty SelectedIndexProperty =
        DependencyProperty.Register(nameof(SelectedIndex), typeof(int), typeof(CommandPalette),
            new FrameworkPropertyMetadata(-1, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedIndexChanged));

    public static readonly DependencyProperty SelectedItemProperty =
        DependencyProperty.Register(nameof(SelectedItem), typeof(CommandPaletteItem), typeof(CommandPalette),
            new FrameworkPropertyMetadata(null, OnSelectedItemChanged));

    public static readonly DependencyProperty HasResultsProperty =
        DependencyProperty.Register(nameof(HasResults), typeof(bool), typeof(CommandPalette),
            new PropertyMetadata(true));

    public static readonly DependencyProperty PlaceholderProperty =
        DependencyProperty.Register(nameof(Placeholder), typeof(string), typeof(CommandPalette),
            new PropertyMetadata("Type a command…"));

    public static readonly DependencyProperty EmptyTextProperty =
        DependencyProperty.Register(nameof(EmptyText), typeof(string), typeof(CommandPalette),
            new PropertyMetadata("No command matches."));

    public static readonly DependencyProperty MaxListHeightProperty =
        DependencyProperty.Register(nameof(MaxListHeight), typeof(double), typeof(CommandPalette),
            new PropertyMetadata(276.0, OnMaxListHeightChanged));

    /// <summary>Height of one row in the template. The list snaps to whole rows so it never shows half of one.</summary>
    private const double RowHeight = 44;

    /// <summary>Vertical padding around the rows in the template.</summary>
    private const double ListPadding = 12;

    public static readonly DependencyProperty CloseOnInvokeProperty =
        DependencyProperty.Register(nameof(CloseOnInvoke), typeof(bool), typeof(CommandPalette),
            new PropertyMetadata(true));

    private readonly ObservableCollection<CommandPaletteItem> _filtered = new();
    private TextBox? _searchBox;
    private ListBox? _list;
    private bool _syncingSelection;

    /// <summary>Raised after an entry runs.</summary>
    public event EventHandler<CommandPaletteInvokedEventArgs>? ItemInvoked;

    /// <summary>Raised when the palette becomes visible.</summary>
    public event EventHandler? Opened;

    /// <summary>Raised when the palette hides.</summary>
    public event EventHandler? Closed;

    /// <summary>Whether the overlay is showing.</summary>
    public bool IsOpen
    {
        get => (bool)GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    /// <summary>Commands to list. Any <see cref="IEnumerable"/> of <see cref="CommandPaletteItem"/>.</summary>
    public IEnumerable? ItemsSource
    {
        get => (IEnumerable?)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    /// <summary>Current search term, two-way bound to the search box.</summary>
    public string FilterText
    {
        get => (string)GetValue(FilterTextProperty);
        set => SetValue(FilterTextProperty, value);
    }

    /// <summary>Index into <see cref="FilteredItems"/>.</summary>
    public int SelectedIndex
    {
        get => (int)GetValue(SelectedIndexProperty);
        set => SetValue(SelectedIndexProperty, value);
    }

    /// <summary>The highlighted entry.</summary>
    public CommandPaletteItem? SelectedItem
    {
        get => (CommandPaletteItem?)GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    /// <summary>False when the current search term matches nothing.</summary>
    public bool HasResults
    {
        get => (bool)GetValue(HasResultsProperty);
        private set => SetValue(HasResultsProperty, value);
    }

    /// <summary>Hint shown in the empty search box.</summary>
    public string Placeholder
    {
        get => (string)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    /// <summary>Message shown when nothing matches.</summary>
    public string EmptyText
    {
        get => (string)GetValue(EmptyTextProperty);
        set => SetValue(EmptyTextProperty, value);
    }

    /// <summary>
    /// Height cap for the result list before it scrolls. Rounded down to whole rows, so the
    /// default (276) shows exactly six.
    /// </summary>
    public double MaxListHeight
    {
        get => (double)GetValue(MaxListHeightProperty);
        set => SetValue(MaxListHeightProperty, value);
    }

    /// <summary>Closes the palette after running an entry. Defaults to true.</summary>
    public bool CloseOnInvoke
    {
        get => (bool)GetValue(CloseOnInvokeProperty);
        set => SetValue(CloseOnInvokeProperty, value);
    }

    /// <summary>The entries currently visible, after filtering.</summary>
    public IReadOnlyList<CommandPaletteItem> FilteredItems => _filtered;

    /// <summary>Shows the overlay and moves focus to the search box.</summary>
    public void Open()
    {
        if (IsOpen) return;
        SetCurrentValue(IsOpenProperty, true);
    }

    /// <summary>Hides the overlay.</summary>
    public void Close()
    {
        if (!IsOpen) return;
        SetCurrentValue(IsOpenProperty, false);
    }

    /// <summary>Toggles the overlay.</summary>
    public void Toggle()
    {
        if (IsOpen) Close();
        else Open();
    }

    /// <summary>Rebuilds the filtered list from <see cref="ItemsSource"/> and <see cref="FilterText"/>.</summary>
    public void Refresh()
    {
        IReadOnlyList<string> tokens = CommandPaletteText.Tokenize(FilterText);
        CommandPaletteItem? previous = SelectedItem;

        _filtered.Clear();

        if (ItemsSource is not null)
        {
            foreach (object? entry in ItemsSource)
            {
                if (entry is not CommandPaletteItem item) continue;
                if (!CommandPaletteText.Matches(item, tokens)) continue;
                _filtered.Add(item);
            }
        }

        HasResults = _filtered.Count > 0;

        // Keep the highlight on the same command when it survives the new filter.
        int index = previous is null ? -1 : _filtered.IndexOf(previous);
        if (index < 0 && _filtered.Count > 0) index = 0;

        _syncingSelection = true;
        try
        {
            SelectedIndex = index;
            SelectedItem = index >= 0 ? _filtered[index] : null;
        }
        finally
        {
            _syncingSelection = false;
        }
    }

    /// <summary>Moves the highlight by <paramref name="delta"/> rows, clamped to the list.</summary>
    public void MoveSelection(int delta)
    {
        if (_filtered.Count == 0) return;

        int index = SelectedIndex + delta;
        if (index < 0) index = 0;
        if (index >= _filtered.Count) index = _filtered.Count - 1;
        SelectedIndex = index;
    }

    /// <summary>Runs the highlighted entry. Returns false when there is nothing to run.</summary>
    public bool ExecuteSelected()
    {
        CommandPaletteItem? item = SelectedItem ?? (SelectedIndex >= 0 && SelectedIndex < _filtered.Count
            ? _filtered[SelectedIndex]
            : null);

        if (item is null) return false;
        if (!item.Invoke()) return false;

        ItemInvoked?.Invoke(this, new CommandPaletteInvokedEventArgs(item));
        if (CloseOnInvoke) Close();
        return true;
    }

    /// <summary>Runs a specific entry, as a mouse click does.</summary>
    public bool InvokeItem(CommandPaletteItem item)
    {
        if (item is null) return false;

        _syncingSelection = true;
        try
        {
            int index = _filtered.IndexOf(item);
            if (index >= 0) SelectedIndex = index;
            SelectedItem = item;
        }
        finally
        {
            _syncingSelection = false;
        }

        return ExecuteSelected();
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        if (_searchBox is not null) _searchBox.TextChanged -= OnSearchTextChanged;
        if (_list is not null) _list.PreviewMouseLeftButtonUp -= OnListMouseUp;

        _searchBox = GetTemplateChild("PART_SearchBox") as TextBox;
        _list = GetTemplateChild("PART_List") as ListBox;

        if (_searchBox is not null) _searchBox.TextChanged += OnSearchTextChanged;
        if (_list is not null) _list.PreviewMouseLeftButtonUp += OnListMouseUp;

        ApplyListHeight();

        if (IsOpen) FocusSearchBox();
    }

    private void ApplyListHeight()
    {
        if (_list is null) return;

        int rows = (int)Math.Floor((MaxListHeight - ListPadding) / RowHeight);
        if (rows < 1) rows = 1;

        _list.Height = ListPadding + rows * RowHeight;
    }

    private static void OnMaxListHeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is CommandPalette palette) palette.ApplyListHeight();
    }

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        if (_searchBox is null) return;
        if (!string.Equals(FilterText, _searchBox.Text, StringComparison.Ordinal))
            SetCurrentValue(FilterTextProperty, _searchBox.Text);
    }

    private void OnListMouseUp(object sender, MouseButtonEventArgs e)
    {
        if (_list?.SelectedItem is not CommandPaletteItem item) return;

        e.Handled = true;
        InvokeItem(item);
    }

    private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is CommandPalette palette) palette.Refresh();
    }

    private static void OnFilterTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is CommandPalette palette) palette.Refresh();
    }

    private static void OnIsOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not CommandPalette palette) return;

        if ((bool)e.NewValue)
        {
            palette.SetCurrentValue(FilterTextProperty, string.Empty);
            palette.Refresh();
            palette.FocusSearchBox();
            palette.Opened?.Invoke(palette, EventArgs.Empty);
        }
        else
        {
            palette.Closed?.Invoke(palette, EventArgs.Empty);
        }
    }

    private static void OnSelectedIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not CommandPalette palette || palette._syncingSelection) return;

        int index = (int)e.NewValue;
        CommandPaletteItem? item = index >= 0 && index < palette._filtered.Count
            ? palette._filtered[index]
            : null;

        palette._syncingSelection = true;
        try
        {
            palette.SetCurrentValue(SelectedItemProperty, item);
        }
        finally
        {
            palette._syncingSelection = false;
        }
    }

    private static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not CommandPalette palette || palette._syncingSelection) return;

        int index = e.NewValue is CommandPaletteItem item ? palette._filtered.IndexOf(item) : -1;

        palette._syncingSelection = true;
        try
        {
            palette.SetCurrentValue(SelectedIndexProperty, index);
        }
        finally
        {
            palette._syncingSelection = false;
        }
    }

    private void FocusSearchBox()
    {
        // The template may not be applied yet when IsOpen is set from XAML.
        Dispatcher.BeginInvoke(DispatcherPriority.Input, new Action(() =>
        {
            if (_searchBox is null) return;
            _searchBox.Focus();
            Keyboard.Focus(_searchBox);
            _searchBox.SelectAll();
        }));
    }

    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
        base.OnPreviewKeyDown(e);

        if (!IsOpen) return;

        switch (e.Key)
        {
            case Key.Down:
                MoveSelection(1);
                e.Handled = true;
                break;
            case Key.Up:
                MoveSelection(-1);
                e.Handled = true;
                break;
            case Key.PageDown:
                MoveSelection(5);
                e.Handled = true;
                break;
            case Key.PageUp:
                MoveSelection(-5);
                e.Handled = true;
                break;
            case Key.Enter:
                ExecuteSelected();
                e.Handled = true;
                break;
            case Key.Escape:
                Close();
                e.Handled = true;
                break;
        }
    }
}
