using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CONCOST.UI.Icons;

namespace CONCOST.UI.Controls;

public class SearchBox : Control
{
    private TextBox? _textBox;
    private Button? _clearButton;

    static SearchBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(SearchBox), new FrameworkPropertyMetadata(typeof(SearchBox)));
    }

    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(nameof(Text), typeof(string), typeof(SearchBox),
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnTextChanged));

    public static readonly DependencyProperty PlaceholderProperty =
        DependencyProperty.Register(nameof(Placeholder), typeof(string), typeof(SearchBox),
            new PropertyMetadata("Search..."));

    public static readonly DependencyProperty IsInstantSearchProperty =
        DependencyProperty.Register(nameof(IsInstantSearch), typeof(bool), typeof(SearchBox),
            new PropertyMetadata(true));

    public static readonly DependencyProperty SearchCommandProperty =
        DependencyProperty.Register(nameof(SearchCommand), typeof(ICommand), typeof(SearchBox),
            new PropertyMetadata(null));

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

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public string Placeholder
    {
        get => (string)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    public bool IsInstantSearch
    {
        get => (bool)GetValue(IsInstantSearchProperty);
        set => SetValue(IsInstantSearchProperty, value);
    }

    public ICommand? SearchCommand
    {
        get => (ICommand?)GetValue(SearchCommandProperty);
        set => SetValue(SearchCommandProperty, value);
    }

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

    private void OnInnerTextChanged(object sender, TextChangedEventArgs e)
    {
        if (_textBox == null) return;
        Text = _textBox.Text;
        if (IsInstantSearch) ExecuteSearch();
    }

    private void OnInnerKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter) { ExecuteSearch(); e.Handled = true; }
    }

    private void ExecuteSearch()
    {
        RaiseEvent(new RoutedEventArgs(SearchStartedEvent, this));
        if (SearchCommand?.CanExecute(Text) == true) SearchCommand.Execute(Text);
    }

    private void OnClearClick(object sender, RoutedEventArgs e)
    {
        Text = string.Empty;
        if (_textBox != null) _textBox.Text = string.Empty;
        RaiseEvent(new RoutedEventArgs(ClearEvent, this));
        if (SearchCommand?.CanExecute(string.Empty) == true) SearchCommand.Execute(string.Empty);
    }

    private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SearchBox sb)
        {
            if (sb._textBox != null && sb._textBox.Text != (string)e.NewValue)
            {
                sb._textBox.Text = (string)e.NewValue ?? string.Empty;
            }
            sb.RaiseEvent(new RoutedEventArgs(TextChangedEvent, sb));
        }
    }
}
