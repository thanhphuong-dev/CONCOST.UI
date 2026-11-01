using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using CONCOST.UI.Icons;

namespace CONCOST.UI.Controls;

/// <summary>
/// A search input field with an optional leading icon, clear button, debounced instant-search,
/// minimum-length guard, and a <see cref="SearchCommand"/> that fires on Enter or after the debounce delay.
/// </summary>
public class SearchBox : Control
{
    private TextBox? _textBox;
    private Button? _clearButton;
    private DispatcherTimer? _debounceTimer;

    static SearchBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(SearchBox), new FrameworkPropertyMetadata(typeof(SearchBox)));
    }

    // ------------------------------------------------------------------ text

    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(nameof(Text), typeof(string), typeof(SearchBox),
            new FrameworkPropertyMetadata(string.Empty,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnTextChanged));

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    // ------------------------------------------------------------------ placeholder

    public static readonly DependencyProperty PlaceholderProperty =
        DependencyProperty.Register(nameof(Placeholder), typeof(string), typeof(SearchBox),
            new PropertyMetadata("Search..."));

    public string Placeholder
    {
        get => (string)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    // ------------------------------------------------------------------ instant search

    public static readonly DependencyProperty IsInstantSearchProperty =
        DependencyProperty.Register(nameof(IsInstantSearch), typeof(bool), typeof(SearchBox),
            new PropertyMetadata(true));

    public bool IsInstantSearch
    {
        get => (bool)GetValue(IsInstantSearchProperty);
        set => SetValue(IsInstantSearchProperty, value);
    }

    // ------------------------------------------------------------------ debounce

    /// <summary>
    /// Milliseconds to wait after the last keystroke before firing an instant search.
    /// Default 300. Set to 0 to fire synchronously on every keystroke.
    /// </summary>
    public static readonly DependencyProperty SearchDelayProperty =
        DependencyProperty.Register(nameof(SearchDelay), typeof(int), typeof(SearchBox),
            new PropertyMetadata(300));

    public int SearchDelay
    {
        get => (int)GetValue(SearchDelayProperty);
        set => SetValue(SearchDelayProperty, value);
    }

    // ------------------------------------------------------------------ minimum search length

    /// <summary>Minimum number of characters before an instant search fires. Default 0.</summary>
    public static readonly DependencyProperty MinSearchLengthProperty =
        DependencyProperty.Register(nameof(MinSearchLength), typeof(int), typeof(SearchBox),
            new PropertyMetadata(0));

    public int MinSearchLength
    {
        get => (int)GetValue(MinSearchLengthProperty);
        set => SetValue(MinSearchLengthProperty, value);
    }

    // ------------------------------------------------------------------ shape

    /// <summary>Corner radius of the text box. Default <c>CornerRadius(6)</c>.</summary>
    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(SearchBox),
            new FrameworkPropertyMetadata(new CornerRadius(6)));

    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    // ------------------------------------------------------------------ icon

    /// <summary>Leading search icon. Default <see cref="IconKind.Search"/>.</summary>
    public static readonly DependencyProperty SearchIconProperty =
        DependencyProperty.Register(nameof(SearchIcon), typeof(IconKind), typeof(SearchBox),
            new PropertyMetadata(IconKind.Search));

    public IconKind SearchIcon
    {
        get => (IconKind)GetValue(SearchIconProperty);
        set => SetValue(SearchIconProperty, value);
    }

    /// <summary>Whether the leading search icon is visible. Default <c>true</c>.</summary>
    public static readonly DependencyProperty ShowSearchIconProperty =
        DependencyProperty.Register(nameof(ShowSearchIcon), typeof(bool), typeof(SearchBox),
            new PropertyMetadata(true));

    public bool ShowSearchIcon
    {
        get => (bool)GetValue(ShowSearchIconProperty);
        set => SetValue(ShowSearchIconProperty, value);
    }

    // ------------------------------------------------------------------ clear button

    /// <summary>Whether the clear (×) button is visible. Default <c>true</c>.</summary>
    public static readonly DependencyProperty ShowClearButtonProperty =
        DependencyProperty.Register(nameof(ShowClearButton), typeof(bool), typeof(SearchBox),
            new PropertyMetadata(true));

    public bool ShowClearButton
    {
        get => (bool)GetValue(ShowClearButtonProperty);
        set => SetValue(ShowClearButtonProperty, value);
    }

    // ------------------------------------------------------------------ HasText (read-only)

    private static readonly DependencyPropertyKey HasTextPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(HasText), typeof(bool), typeof(SearchBox),
            new PropertyMetadata(false));

    /// <summary><c>true</c> when <see cref="Text"/> is not null or empty. Useful for clear-button visibility triggers.</summary>
    public static readonly DependencyProperty HasTextProperty = HasTextPropertyKey.DependencyProperty;

    public bool HasText => (bool)GetValue(HasTextProperty);

    // ------------------------------------------------------------------ command

    public static readonly DependencyProperty SearchCommandProperty =
        DependencyProperty.Register(nameof(SearchCommand), typeof(ICommand), typeof(SearchBox),
            new PropertyMetadata(null));

    public ICommand? SearchCommand
    {
        get => (ICommand?)GetValue(SearchCommandProperty);
        set => SetValue(SearchCommandProperty, value);
    }

    /// <summary>Extra parameter forwarded to <see cref="SearchCommand"/>. Defaults to the search text when null.</summary>
    public static readonly DependencyProperty CommandParameterProperty =
        DependencyProperty.Register(nameof(CommandParameter), typeof(object), typeof(SearchBox),
            new PropertyMetadata(null));

    public object? CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    // ------------------------------------------------------------------ routed events

    public static readonly RoutedEvent SearchStartedEvent =
        EventManager.RegisterRoutedEvent(nameof(SearchStarted), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(SearchBox));

    public static readonly RoutedEvent ClearEvent =
        EventManager.RegisterRoutedEvent(nameof(Clear), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(SearchBox));

    public static readonly RoutedEvent TextChangedEvent =
        EventManager.RegisterRoutedEvent(nameof(TextChanged), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(SearchBox));

    public event RoutedEventHandler SearchStarted
    {
        add => AddHandler(SearchStartedEvent, value);
        remove => RemoveHandler(SearchStartedEvent, value);
    }

    public event RoutedEventHandler Clear
    {
        add => AddHandler(ClearEvent, value);
        remove => RemoveHandler(ClearEvent, value);
    }

    public event RoutedEventHandler TextChanged
    {
        add => AddHandler(TextChangedEvent, value);
        remove => RemoveHandler(TextChangedEvent, value);
    }

    // ------------------------------------------------------------------ template

    public override void OnApplyTemplate()
    {
        if (_textBox != null)
        {
            _textBox.TextChanged -= OnInnerTextChanged;
            _textBox.KeyDown -= OnInnerKeyDown;
        }
        if (_clearButton != null) _clearButton.Click -= OnClearClick;

        base.OnApplyTemplate();

        _textBox = GetTemplateChild("PART_TextBox") as TextBox;
        if (_textBox != null)
        {
            _textBox.TextChanged += OnInnerTextChanged;
            _textBox.KeyDown += OnInnerKeyDown;
        }

        _clearButton = GetTemplateChild("PART_ClearButton") as Button;
        if (_clearButton != null) _clearButton.Click += OnClearClick;
    }

    // ------------------------------------------------------------------ inner handlers

    private void OnInnerTextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        if (_textBox is null) return;
        Text = _textBox.Text;

        if (IsInstantSearch)
        {
            int delay = SearchDelay;
            if (delay <= 0)
            {
                TryExecuteSearch();
            }
            else
            {
                // Restart debounce timer
                if (_debounceTimer is null)
                {
                    _debounceTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(delay) };
                    _debounceTimer.Tick += OnDebounceTimerTick;
                }
                else
                {
                    _debounceTimer.Stop();
                    _debounceTimer.Interval = TimeSpan.FromMilliseconds(delay);
                }

                _debounceTimer.Start();
            }
        }
    }

    private void OnDebounceTimerTick(object? sender, EventArgs e)
    {
        _debounceTimer?.Stop();
        TryExecuteSearch();
    }

    private void OnInnerKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            _debounceTimer?.Stop();
            ExecuteSearch();
            e.Handled = true;
        }
        else if (e.Key == Key.Escape)
        {
            OnClearClick(this, new RoutedEventArgs());
            e.Handled = true;
        }
    }

    private void TryExecuteSearch()
    {
        if (Text.Length >= MinSearchLength)
            ExecuteSearch();
    }

    private void ExecuteSearch()
    {
        RaiseEvent(new RoutedEventArgs(SearchStartedEvent, this));
        object? param = CommandParameter ?? Text;
        if (SearchCommand?.CanExecute(param) == true) SearchCommand.Execute(param);
    }

    private void OnClearClick(object sender, RoutedEventArgs e)
    {
        _debounceTimer?.Stop();
        Text = string.Empty;
        if (_textBox != null) _textBox.Text = string.Empty;
        RaiseEvent(new RoutedEventArgs(ClearEvent, this));
        object? param = CommandParameter ?? string.Empty;
        if (SearchCommand?.CanExecute(param) == true) SearchCommand.Execute(param);
    }

    private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not SearchBox sb) return;

        string newText = (string?)e.NewValue ?? string.Empty;
        sb.SetValue(HasTextPropertyKey, newText.Length > 0);

        if (sb._textBox != null && sb._textBox.Text != newText)
            sb._textBox.Text = newText;

        sb.RaiseEvent(new RoutedEventArgs(TextChangedEvent, sb));
    }
}
