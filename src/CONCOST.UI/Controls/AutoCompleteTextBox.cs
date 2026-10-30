using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;

namespace CONCOST.UI.Controls;

/// <summary>
/// AutoCompleteTextBox provides an instant filterable dropdown suggestion list as the user types.
/// Supports custom ItemTemplate, DisplayMemberPath, SelectedItem binding, search icon, clear button, and placeholder text.
/// Optimized for zero lag with cached property reflection and high-performance UI filtering.
/// </summary>
public class AutoCompleteTextBox : TextBox
{
    private Popup? _popup;
    private ListBox? _listBox;
    private Button? _clearBtn;
    private bool _isInternalChange;
    private bool _isCommittingSelection;
    private readonly DispatcherTimer _debounceTimer;
    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> _propertyCache = new();

    static AutoCompleteTextBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(AutoCompleteTextBox), new FrameworkPropertyMetadata(typeof(AutoCompleteTextBox)));
    }

    public AutoCompleteTextBox()
    {
        _debounceTimer = new DispatcherTimer(DispatcherPriority.Normal)
        {
            Interval = TimeSpan.FromMilliseconds(10)
        };
        _debounceTimer.Tick += (s, e) =>
        {
            _debounceTimer.Stop();
            FilterSuggestionsInternal(Text);
        };
    }

    public static readonly DependencyProperty ItemsSourceProperty =
        DependencyProperty.Register(nameof(ItemsSource), typeof(IEnumerable), typeof(AutoCompleteTextBox),
            new PropertyMetadata(null, OnItemsSourceChanged));

    public static readonly DependencyProperty SelectedItemProperty =
        DependencyProperty.Register(nameof(SelectedItem), typeof(object), typeof(AutoCompleteTextBox),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedItemChanged));

    public static readonly DependencyProperty ItemTemplateProperty =
        DependencyProperty.Register(nameof(ItemTemplate), typeof(DataTemplate), typeof(AutoCompleteTextBox),
            new PropertyMetadata(null));

    public static readonly DependencyProperty DisplayMemberPathProperty =
        DependencyProperty.Register(nameof(DisplayMemberPath), typeof(string), typeof(AutoCompleteTextBox),
            new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty MaxDropDownHeightProperty =
        DependencyProperty.Register(nameof(MaxDropDownHeight), typeof(double), typeof(AutoCompleteTextBox),
            new PropertyMetadata(240.0, null, CoerceMaxDropDownHeight));

    public static readonly DependencyProperty IsDropDownOpenProperty =
        DependencyProperty.Register(nameof(IsDropDownOpen), typeof(bool), typeof(AutoCompleteTextBox),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public static readonly RoutedEvent ItemSelectedEvent =
        EventManager.RegisterRoutedEvent(nameof(ItemSelected), RoutingStrategy.Bubble,
            typeof(RoutedPropertyChangedEventHandler<object?>), typeof(AutoCompleteTextBox));

    public event RoutedPropertyChangedEventHandler<object?> ItemSelected
    {
        add => AddHandler(ItemSelectedEvent, value);
        remove => RemoveHandler(ItemSelectedEvent, value);
    }

    public IEnumerable? ItemsSource
    {
        get => (IEnumerable?)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public object? SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    public DataTemplate? ItemTemplate
    {
        get => (DataTemplate?)GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }

    public string DisplayMemberPath
    {
        get => (string)GetValue(DisplayMemberPathProperty);
        set => SetValue(DisplayMemberPathProperty, value);
    }

    public double MaxDropDownHeight
    {
        get => (double)GetValue(MaxDropDownHeightProperty);
        set => SetValue(MaxDropDownHeightProperty, value);
    }

    public bool IsDropDownOpen
    {
        get => (bool)GetValue(IsDropDownOpenProperty);
        set => SetValue(IsDropDownOpenProperty, value);
    }

    public override void OnApplyTemplate()
    {
        if (_listBox != null)
        {
            _listBox.SelectionChanged -= OnListBoxSelectionChanged;
            _listBox.PreviewKeyDown -= OnListBoxPreviewKeyDown;
        }
        if (_clearBtn != null)
        {
            _clearBtn.Click -= OnClearButtonClicked;
        }

        base.OnApplyTemplate();

        _popup = GetTemplateChild("PART_Popup") as Popup;
        _listBox = GetTemplateChild("PART_ListBox") as ListBox;
        _clearBtn = GetTemplateChild("PART_ClearButton") as Button;

        if (_listBox != null)
        {
            _listBox.SelectionChanged += OnListBoxSelectionChanged;
            _listBox.PreviewKeyDown += OnListBoxPreviewKeyDown;
        }
        if (_clearBtn != null)
        {
            _clearBtn.Click += OnClearButtonClicked;
        }

        LostKeyboardFocus -= OnLostKeyboardFocus;
        LostKeyboardFocus += OnLostKeyboardFocus;

        FilterSuggestionsInternal(Text);
    }

    private void OnClearButtonClicked(object sender, RoutedEventArgs e)
    {
        ClearValue();
        Focus();
    }

    public void ClearValue()
    {
        _debounceTimer.Stop();
        _isInternalChange = true;
        Text = string.Empty;
        _isInternalChange = false;
        SelectedItem = null;
        IsDropDownOpen = false;
    }

    private void OnLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
    {
        if (_popup != null && !_popup.IsKeyboardFocusWithin)
        {
            IsDropDownOpen = false;
        }
    }

    private static object CoerceMaxDropDownHeight(DependencyObject d, object value)
    {
        var height = (double)value;
        return NumericCompatibility.IsFinite(height) ? Math.Max(0, height) : 240d;
    }

    protected override void OnTextChanged(TextChangedEventArgs e)
    {
        base.OnTextChanged(e);

        if (_isInternalChange) return;

        // Immediate filtering for short lists, ultra-responsive 10ms debounce for others
        _debounceTimer.Stop();
        _debounceTimer.Start();
    }

    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
        base.OnPreviewKeyDown(e);

        if (e.Key == Key.Down)
        {
            _debounceTimer.Stop();
            if (!IsDropDownOpen)
            {
                FilterSuggestionsInternal(Text, openIfEmpty: true);
                e.Handled = true;
            }
            else if (_listBox != null && _listBox.Items.Count > 0)
            {
                _listBox.Focus();
                if (_listBox.SelectedIndex < 0) _listBox.SelectedIndex = 0;
                e.Handled = true;
            }
        }
        else if (e.Key == Key.Escape)
        {
            _debounceTimer.Stop();
            IsDropDownOpen = false;
            e.Handled = true;
        }
    }

    private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is AutoCompleteTextBox actb)
        {
            actb.FilterSuggestionsInternal(actb.Text);
        }
    }

    private static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is AutoCompleteTextBox actb && !actb._isCommittingSelection)
        {
            actb._isInternalChange = true;
            actb.Text = actb.GetItemText(e.NewValue);
            actb.CaretIndex = actb.Text.Length;
            actb._isInternalChange = false;
        }
    }

    public string GetItemText(object? item)
    {
        if (item == null) return string.Empty;
        if (item is string str) return str;

        if (!string.IsNullOrEmpty(DisplayMemberPath))
        {
            var prop = GetCachedProperties(item.GetType()).FirstOrDefault(p => p.Name == DisplayMemberPath);
            if (prop != null)
            {
                var val = prop.GetValue(item);
                if (val != null) return val.ToString() ?? string.Empty;
            }
        }
        return item.ToString() ?? string.Empty;
    }

    public void FilterSuggestions(string query, bool openIfEmpty = false)
    {
        _debounceTimer.Stop();
        FilterSuggestionsInternal(query, openIfEmpty);
    }

    private void FilterSuggestionsInternal(string query, bool openIfEmpty = false)
    {
        if (_listBox == null) return;
        if (ItemsSource == null)
        {
            _listBox.ItemsSource = Array.Empty<object>();
            IsDropDownOpen = false;
            return;
        }

        if (string.IsNullOrWhiteSpace(query))
        {
            if (openIfEmpty)
            {
                var all = ItemsSource.Cast<object>().ToList();
                _listBox.ItemsSource = all;
                _listBox.SelectedIndex = -1;
                IsDropDownOpen = all.Count > 0;
            }
            else
            {
                IsDropDownOpen = false;
            }
            return;
        }

        var list = new List<object>();
        foreach (var item in ItemsSource)
        {
            if (item == null) continue;

            // Fast path for string items
            if (item is string s)
            {
                if (s.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    list.Add(item);
                }
                continue;
            }

            // Object search: DisplayMemberPath or cached properties
            string displayStr = GetItemText(item);
            if (displayStr.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                list.Add(item);
                continue;
            }

            // Inspect cached string properties (Code, Name, Standard, Title, etc.)
            var props = GetCachedProperties(item.GetType());
            bool matches = false;
            for (int i = 0; i < props.Length; i++)
            {
                var p = props[i];
                if (p.PropertyType == typeof(string) && p.CanRead)
                {
                    var val = p.GetValue(item) as string;
                    if (!string.IsNullOrEmpty(val) && val.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        matches = true;
                        break;
                    }
                }
            }

            if (matches)
            {
                list.Add(item);
            }
        }

        _listBox.ItemsSource = list;
        _listBox.SelectedIndex = -1;
        IsDropDownOpen = list.Count > 0;
    }

    private static PropertyInfo[] GetCachedProperties(Type type)
    {
        return _propertyCache.GetOrAdd(type, t => t.GetProperties(BindingFlags.Public | BindingFlags.Instance));
    }

    private void OnListBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!_isCommittingSelection && _listBox?.SelectedItem != null)
        {
            CommitSelection();
        }
    }

    private void OnListBoxPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            if (_listBox?.SelectedItem != null)
            {
                CommitSelection();
                e.Handled = true;
            }
        }
    }

    private void CommitSelection()
    {
        if (_listBox?.SelectedItem is null) return;
        _isCommittingSelection = true;
        try
        {
            var selected = _listBox.SelectedItem;
            var oldSelected = SelectedItem;
            _isInternalChange = true;
            Text = GetItemText(selected);
            CaretIndex = Text.Length;
            SelectedItem = selected;
            _isInternalChange = false;
            IsDropDownOpen = false;
            Focus();

            RaiseEvent(new RoutedPropertyChangedEventArgs<object?>(oldSelected, selected, ItemSelectedEvent));
        }
        finally
        {
            _isInternalChange = false;
            _isCommittingSelection = false;
        }
    }
}
