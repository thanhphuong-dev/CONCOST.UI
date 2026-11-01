using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using CONCOST.UI.Core.MVVM;

namespace CONCOST.UI.Controls;

/// <summary>One chip in a <see cref="TagInput"/>: the item it came from, plus the text it shows.</summary>
public class TagEntry
{
    /// <summary>Creates an entry. <paramref name="text"/> is what the chip displays.</summary>
    public TagEntry(object item, string text)
    {
        Item = item;
        Text = text;
    }

    /// <summary>The value behind the chip — a string for free text, the original object for a suggestion.</summary>
    public object Item { get; }

    /// <summary>Chip caption, resolved from <see cref="TagInput.DisplayPath"/> (or the item's own <c>ToString</c>).</summary>
    public string Text { get; }

    public override string ToString() => Text;
}

/// <summary>Describes a tag that a <see cref="TagInput"/> just added or removed.</summary>
public class TagChangedEventArgs : EventArgs
{
    public TagChangedEventArgs(object item, string text, bool isAdded)
    {
        Item = item;
        Text = text;
        IsAdded = isAdded;
    }

    /// <summary>The value behind the tag.</summary>
    public object Item { get; }

    /// <summary>The chip caption.</summary>
    public string Text { get; }

    /// <summary><c>true</c> when the tag was added, <c>false</c> when it was removed.</summary>
    public bool IsAdded { get; }
}

/// <summary>
/// A text box that collects tags: type a word, press <c>Enter</c>, and it becomes a chip in the box.
/// With a <see cref="SuggestionSource"/> it behaves like a mention box — typing narrows a list and
/// <c>Enter</c> takes the highlighted entry.
/// </summary>
/// <remarks>
/// <para>
/// The chips wrap above the input line, so the box grows downwards as tags are added instead of
/// scrolling sideways away from the caret.
/// </para>
/// <para>
/// Bind <see cref="ItemsSource"/> to an <see cref="IList"/> (an <see cref="ObservableCollection{T}"/>
/// is the usual case) and the control edits that collection directly, so the view model owns the
/// tags and the view is just a projection. Bind a sequence the control cannot write to and the tags
/// are still shown, while <see cref="TagsChanged"/> tells the host what the user asked for.
/// </para>
/// <para>
/// Duplicate detection and the suggestion filter run through
/// <see cref="CommandPaletteText.Normalize"/>, so <c>ket cau</c> matches "Kết cấu" and a tag cannot be
/// added twice with a different accent.
/// </para>
/// </remarks>
public class TagInput : Control
{
    private TextBox? _editor;
    private ItemsControl? _chips;
    private ListBox? _suggestionList;
    private Popup? _popup;
    private TextBlock? _placeholder;
    private int _highlight = -1;
    private bool _syncing;

    private readonly ObservableCollection<TagEntry> _visibleSuggestions = new();
    private readonly List<TagEntry> _suggestions = new();
    private const int MaxSuggestionRows = 6;

    static TagInput()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(TagInput),
            new FrameworkPropertyMetadata(typeof(TagInput)));
    }

    public TagInput()
    {
        Tags = new ObservableCollection<TagEntry>();
        Tags.CollectionChanged += (_, _) => UpdatePlaceholderVisibility();

        // The chip's close button lives inside an item template, so it reaches the control through a
        // command rather than an event handler.
        RemoveTagCommand = new RelayCommand(parameter =>
        {
            if (parameter is TagEntry entry) RemoveTag(entry);
        });
    }

    /// <summary>The chips currently on screen, in order.</summary>
    public ObservableCollection<TagEntry> Tags { get; }

    /// <summary>The entries the suggestion list is showing right now, after filtering.</summary>
    public ObservableCollection<TagEntry> Suggestions => _visibleSuggestions;

    /// <summary>
    /// Removes the tag a chip's close button carries. The default template binds the button to this
    /// command, which is how the button reaches the control from inside an item template.
    /// </summary>
    public ICommand RemoveTagCommand { get; }

    public static readonly DependencyProperty ItemsSourceProperty =
        DependencyProperty.Register(nameof(ItemsSource), typeof(IEnumerable), typeof(TagInput),
            new PropertyMetadata(null, OnItemsSourceChanged));

    /// <summary>The collection the tags come from (and, when it is an <see cref="IList"/>, are written back to).</summary>
    public IEnumerable? ItemsSource
    {
        get => (IEnumerable?)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public static readonly DependencyProperty DisplayPathProperty =
        DependencyProperty.Register(nameof(DisplayPath), typeof(string), typeof(TagInput),
            new PropertyMetadata(string.Empty, OnDisplayPathChanged));

    /// <summary>
    /// Property (or dotted path) used as a chip caption, for both the tags and the suggestions. Empty
    /// uses the item's own <c>ToString()</c>, which is all a <c>string</c> collection needs.
    /// </summary>
    public string DisplayPath
    {
        get => (string)GetValue(DisplayPathProperty);
        set => SetValue(DisplayPathProperty, value);
    }

    public static readonly DependencyProperty SuggestionSourceProperty =
        DependencyProperty.Register(nameof(SuggestionSource), typeof(IEnumerable), typeof(TagInput),
            new PropertyMetadata(null, OnSuggestionSourceChanged));

    /// <summary>Values offered while the user types. Empty turns the control into a plain tag box.</summary>
    public IEnumerable? SuggestionSource
    {
        get => (IEnumerable?)GetValue(SuggestionSourceProperty);
        set => SetValue(SuggestionSourceProperty, value);
    }

    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(nameof(Text), typeof(string), typeof(TagInput),
            new FrameworkPropertyMetadata(string.Empty,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnTextChanged));

    /// <summary>What the user has typed and not committed yet. Two-way by default.</summary>
    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public static readonly DependencyProperty PlaceholderProperty =
        DependencyProperty.Register(nameof(Placeholder), typeof(string), typeof(TagInput),
            new PropertyMetadata(string.Empty, OnPlaceholderChanged));

    /// <summary>Hint shown while there is no tag and nothing typed.</summary>
    public string Placeholder
    {
        get => (string)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    public static readonly DependencyProperty AllowFreeTextProperty =
        DependencyProperty.Register(nameof(AllowFreeText), typeof(bool), typeof(TagInput),
            new PropertyMetadata(true));

    /// <summary>
    /// When <c>true</c> (the default) anything typed can become a tag. When <c>false</c> the user can
    /// only pick from <see cref="SuggestionSource"/>, which is what a "choose the materials" box wants.
    /// </summary>
    public bool AllowFreeText
    {
        get => (bool)GetValue(AllowFreeTextProperty);
        set => SetValue(AllowFreeTextProperty, value);
    }

    public static readonly DependencyProperty AllowDuplicatesProperty =
        DependencyProperty.Register(nameof(AllowDuplicates), typeof(bool), typeof(TagInput),
            new PropertyMetadata(false));

    /// <summary>Allow the same tag twice. Default <c>false</c>; comparison ignores case and accents.</summary>
    public bool AllowDuplicates
    {
        get => (bool)GetValue(AllowDuplicatesProperty);
        set => SetValue(AllowDuplicatesProperty, value);
    }

    public static readonly DependencyProperty MaxTagsProperty =
        DependencyProperty.Register(nameof(MaxTags), typeof(int), typeof(TagInput),
            new PropertyMetadata(0));

    /// <summary>Upper bound on the number of tags. <c>0</c> (the default) means no limit.</summary>
    public int MaxTags
    {
        get => (int)GetValue(MaxTagsProperty);
        set => SetValue(MaxTagsProperty, value);
    }

    public static readonly DependencyProperty DelimiterProperty =
        DependencyProperty.Register(nameof(Delimiter), typeof(string), typeof(TagInput),
            new PropertyMetadata(","));

    /// <summary>
    /// Characters that commit the text as a tag, in addition to <c>Enter</c>. Default <c>,</c> —
    /// which is how a user pastes "bê tông, thép, cốp pha" in one go. Empty disables it.
    /// </summary>
    public string Delimiter
    {
        get => (string)GetValue(DelimiterProperty);
        set => SetValue(DelimiterProperty, value);
    }

    public static readonly DependencyProperty IsSuggestionOpenProperty =
        DependencyProperty.Register(nameof(IsSuggestionOpen), typeof(bool), typeof(TagInput),
            new FrameworkPropertyMetadata(false,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSuggestionOpenChanged));

    /// <summary>Whether the suggestion list is showing. Two-way, so a host can close it on demand.</summary>
    public bool IsSuggestionOpen
    {
        get => (bool)GetValue(IsSuggestionOpenProperty);
        set => SetValue(IsSuggestionOpenProperty, value);
    }

    /// <summary>Raised when a tag is added or removed, including when the user only asked for it.</summary>
    public event EventHandler<TagChangedEventArgs>? TagsChanged;

    /// <summary>
    /// Turns the current <see cref="Text"/> into tags, splitting on <see cref="Delimiter"/>. Returns
    /// the number of tags that were actually added.
    /// </summary>
    public int CommitText()
    {
        string raw = Text ?? string.Empty;
        if (raw.Length == 0) return 0;

        char[] separators = string.IsNullOrEmpty(Delimiter) ? Array.Empty<char>() : Delimiter.ToCharArray();
        string[] parts = separators.Length == 0 ? new[] { raw } : raw.Split(separators);

        int added = 0;
        foreach (string part in parts)
        {
            string text = part.Trim();
            if (text.Length == 0) continue;
            if (AddTag(text)) added++;
        }

        SetCurrentValue(TextProperty, string.Empty);
        CloseSuggestions();

        return added;
    }

    /// <summary>
    /// Adds a tag. A <c>string</c> is taken as free text, anything else as an item resolved through
    /// <see cref="DisplayPath"/>. Returns <c>false</c> when the tag was rejected — empty, a duplicate,
    /// over <see cref="MaxTags"/>, or free text while <see cref="AllowFreeText"/> is off.
    /// </summary>
    public bool AddTag(object? value)
    {
        if (value is null) return false;

        bool isText = value is string;
        string text = isText ? ((string)value).Trim() : TextOf(value);
        if (string.IsNullOrWhiteSpace(text)) return false;

        if (isText && !AllowFreeText && !IsKnownSuggestion(text)) return false;
        if (MaxTags > 0 && Tags.Count >= MaxTags) return false;
        if (!AllowDuplicates && Tags.Any(t => Keys(t.Text) == Keys(text))) return false;

        var entry = new TagEntry(value, text);
        Tags.Add(entry);

        // A collection the control can write to is the source of truth; anything else is read-only and
        // the host is expected to apply the change when TagsChanged arrives.
        if (ItemsSource is IList list && !list.Contains(value))
        {
            list.Add(value);

            // A plain List<T> raises nothing, so rebuild from it to stay in step; an observable one
            // raises CollectionChanged and the handler would rebuild anyway.
            if (ItemsSource is not INotifyCollectionChanged) ReloadTags();
        }

        TagsChanged?.Invoke(this, new TagChangedEventArgs(value, text, true));
        return true;
    }

    /// <summary>Removes a chip and, when it can, the item behind it in <see cref="ItemsSource"/>.</summary>
    public bool RemoveTag(TagEntry? entry)
    {
        if (entry is null) return false;

        int index = Tags.IndexOf(entry);
        if (index < 0) return false;

        Tags.RemoveAt(index);

        if (ItemsSource is IList list && list.Contains(entry.Item))
        {
            list.Remove(entry.Item);
            if (ItemsSource is not INotifyCollectionChanged) ReloadTags();
        }

        TagsChanged?.Invoke(this, new TagChangedEventArgs(entry.Item, entry.Text, false));
        return true;
    }

    /// <summary>Removes every tag. Raises <see cref="TagsChanged"/> once per removed tag.</summary>
    public void ClearTags()
    {
        if (Tags.Count == 0) return;

        foreach (TagEntry entry in Tags.ToList()) RemoveTag(entry);
    }

    public override void OnApplyTemplate()
    {
        if (_editor is not null)
        {
            _editor.PreviewKeyDown -= OnEditorKeyDown;
            _editor.LostKeyboardFocus -= OnEditorLostFocus;
        }

        if (_suggestionList is not null)
        {
            _suggestionList.PreviewMouseLeftButtonUp -= OnSuggestionClicked;
            _suggestionList.SelectionChanged -= OnSuggestionSelectionChanged;
        }

        base.OnApplyTemplate();

        _editor = GetTemplateChild("PART_Editor") as TextBox;
        _chips = GetTemplateChild("PART_Chips") as ItemsControl;
        _suggestionList = GetTemplateChild("PART_SuggestionList") as ListBox;
        _popup = GetTemplateChild("PART_Popup") as Popup;
        _placeholder = GetTemplateChild("PART_Placeholder") as TextBlock;

        if (_chips is not null) _chips.ItemsSource = Tags;

        if (_suggestionList is not null)
        {
            _suggestionList.ItemsSource = _visibleSuggestions;
            _suggestionList.PreviewMouseLeftButtonUp += OnSuggestionClicked;
            _suggestionList.SelectionChanged += OnSuggestionSelectionChanged;
        }

        if (_editor is not null)
        {
            _editor.PreviewKeyDown += OnEditorKeyDown;
            _editor.LostKeyboardFocus += OnEditorLostFocus;
        }

        UpdatePlaceholderVisibility();
        ApplyPopupState();
    }

    private void OnEditorKeyDown(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Enter:
                // Enter takes the highlighted suggestion when the list is open, otherwise it commits
                // what was typed — the two things a user means by Enter in this box.
                if (IsSuggestionOpen && _highlight >= 0 && _highlight < _visibleSuggestions.Count)
                {
                    AddTag(_visibleSuggestions[_highlight].Item);
                    SetCurrentValue(TextProperty, string.Empty);
                    CloseSuggestions();
                }
                else
                {
                    CommitText();
                }

                e.Handled = true;
                break;

            case Key.Down:
                MoveHighlight(1);
                e.Handled = true;
                break;

            case Key.Up:
                MoveHighlight(-1);
                e.Handled = true;
                break;

            case Key.Escape:
                if (IsSuggestionOpen)
                {
                    CloseSuggestions();
                    e.Handled = true;
                }

                break;

            case Key.Back:
                // Backspace in an empty box deletes the last chip: the shortcut everyone expects from
                // a tag field, and the only way to remove one without the mouse.
                if (string.IsNullOrEmpty(Text) && Tags.Count > 0)
                {
                    RemoveTag(Tags[Tags.Count - 1]);
                    e.Handled = true;
                }

                break;
        }
    }

    private void OnEditorLostFocus(object sender, KeyboardFocusChangedEventArgs e)
    {
        // Committing on blur keeps a half-typed tag from being silently thrown away when the user
        // clicks "Save".
        CommitText();
    }

    private void OnSuggestionClicked(object sender, MouseButtonEventArgs e)
    {
        if (_suggestionList is null) return;

        var source = e.OriginalSource as DependencyObject;

        while (source is not null and not ListBoxItem)
        {
            source = System.Windows.Media.VisualTreeHelper.GetParent(source);
        }

        if (source is ListBoxItem { DataContext: TagEntry entry })
        {
            AddTag(entry.Item);
            SetCurrentValue(TextProperty, string.Empty);
            CloseSuggestions();
            e.Handled = true;
        }
    }

    private void OnSuggestionSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_suggestionList is not null) _highlight = _suggestionList.SelectedIndex;
    }

    private void MoveHighlight(int delta)
    {
        if (!IsSuggestionOpen || _visibleSuggestions.Count == 0) return;

        int next = _highlight + delta;
        if (next < 0) next = _visibleSuggestions.Count - 1;
        if (next >= _visibleSuggestions.Count) next = 0;

        _highlight = next;
        if (_suggestionList is not null) _suggestionList.SelectedIndex = next;
    }

    private void FilterSuggestions()
    {
        string query = CommandPaletteText.Normalize(Text);

        _visibleSuggestions.Clear();

        if (query.Length == 0)
        {
            CloseSuggestions();
            return;
        }

        foreach (TagEntry entry in _suggestions)
        {
            if (CommandPaletteText.Normalize(entry.Text).Contains(query, StringComparison.Ordinal))
            {
                _visibleSuggestions.Add(entry);

                // A list that grows past six rows would cover the form it is filtering; the host can
                // still reach the rest by typing one more character.
                if (_visibleSuggestions.Count >= MaxSuggestionRows) break;
            }
        }

        _highlight = _visibleSuggestions.Count > 0 ? 0 : -1;

        if (_suggestionList is not null) _suggestionList.SelectedIndex = _highlight;

        SetCurrentValue(IsSuggestionOpenProperty, _visibleSuggestions.Count > 0);
    }

    private void CloseSuggestions() => SetCurrentValue(IsSuggestionOpenProperty, false);

    private void ApplyPopupState()
    {
        if (_popup is null) return;
        _popup.IsOpen = IsSuggestionOpen;
    }

    private void UpdatePlaceholderVisibility()
    {
        if (_placeholder is null) return;

        _placeholder.Visibility = Tags.Count == 0 && string.IsNullOrEmpty(Text)
            ? Visibility.Visible
            : Visibility.Collapsed;
    }

    private void ReloadTags()
    {
        if (_syncing) return;

        _syncing = true;
        try
        {
            Tags.Clear();
            if (ItemsSource is null) return;

            foreach (object? item in ItemsSource)
            {
                if (item is null) continue;
                Tags.Add(new TagEntry(item, TextOf(item)));
            }
        }
        finally
        {
            _syncing = false;
            UpdatePlaceholderVisibility();
        }
    }

    private void LoadSuggestions()
    {
        _suggestions.Clear();

        if (SuggestionSource is not null)
        {
            foreach (object? item in SuggestionSource)
            {
                if (item is null) continue;
                _suggestions.Add(new TagEntry(item, TextOf(item)));
            }
        }

        if (IsSuggestionOpen) FilterSuggestions();
    }

    private string TextOf(object item) =>
        string.IsNullOrEmpty(DisplayPath) ? item.ToString() ?? string.Empty : ReadPath(item, DisplayPath) ?? string.Empty;

    private static string? ReadPath(object item, string path)
    {
        object? current = item;

        foreach (string part in path.Split('.'))
        {
            if (current is null) return null;

            PropertyInfo? property = current.GetType().GetProperty(part,
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

            if (property is null) return null;
            current = property.GetValue(current);
        }

        return current?.ToString();
    }

    /// <summary>
    /// Whether <paramref name="text"/> names one of the suggestions. A suggestion whose item is a
    /// plain string arrives here as free text, so the list is what decides whether it may be added.
    /// </summary>
    private bool IsKnownSuggestion(string text)
    {
        string key = Keys(text);
        return _suggestions.Any(s => Keys(s.Text) == key);
    }

    /// <summary>Duplicate key: the caption with case and diacritics removed.</summary>
    private static string Keys(string text) => CommandPaletteText.Normalize(text);

    private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not TagInput input) return;

        if (e.OldValue is INotifyCollectionChanged oldObservable)
            oldObservable.CollectionChanged -= input.OnSourceCollectionChanged;

        if (e.NewValue is INotifyCollectionChanged newObservable)
            newObservable.CollectionChanged += input.OnSourceCollectionChanged;

        input.ReloadTags();
    }

    private void OnSourceCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) => ReloadTags();

    private static void OnSuggestionSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TagInput input) input.LoadSuggestions();
    }

    private static void OnDisplayPathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not TagInput input) return;

        // Captions are derived, so both lists have to be rebuilt when the path changes.
        input.ReloadTags();
        input.LoadSuggestions();
    }

    private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not TagInput input) return;

        input.UpdatePlaceholderVisibility();
        input.FilterSuggestions();
    }

    private static void OnPlaceholderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TagInput input) input.UpdatePlaceholderVisibility();
    }

    private static void OnSuggestionOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TagInput input) input.ApplyPopupState();
    }
}
