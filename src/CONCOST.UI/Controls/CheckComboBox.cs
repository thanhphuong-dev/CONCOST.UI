using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace CONCOST.UI.Controls;

public class CheckComboBoxItem : INotifyPropertyChanged
{
    private bool _isSelected;
    private object? _content;

    public object? Content
    {
        get => _content;
        set
        {
            _content = value;
            OnPropertyChanged(nameof(Content));
        }
    }

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected != value)
            {
                _isSelected = value;
                OnPropertyChanged(nameof(IsSelected));
                Parent?.OnItemSelectionToggled();
            }
        }
    }

    public CheckComboBox? Parent { get; set; }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected virtual void OnPropertyChanged(string propName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));

    public override string ToString() => Content?.ToString() ?? string.Empty;
}

public class CheckComboBox : Control
{
    private ToggleButton? _toggleButton;
    private ButtonBase? _button;
    private TextBlock? _displayText;
    private Popup? _popup;
    private CheckBox? _selectAllCheckBox;
    private bool _isUpdatingAll = false;
    private bool _isInternalValueUpdate = false;

    static CheckComboBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(CheckComboBox), new FrameworkPropertyMetadata(typeof(CheckComboBox)));
    }

    #region Dependency Properties

    public static readonly DependencyProperty ItemsSourceProperty =
        DependencyProperty.Register(nameof(ItemsSource), typeof(IEnumerable), typeof(CheckComboBox),
            new PropertyMetadata(null, OnItemsSourceChanged));

    public static readonly DependencyProperty SelectedValueProperty =
        DependencyProperty.Register(nameof(SelectedValue), typeof(string), typeof(CheckComboBox),
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedValueChanged));

    public static readonly DependencyProperty DisplayTextProperty =
        DependencyProperty.Register(nameof(DisplayText), typeof(string), typeof(CheckComboBox),
            new PropertyMetadata(string.Empty));

    /// <summary>Gets or sets the maximum width of the selected-items preview text.</summary>
    public static readonly DependencyProperty SelectedItemsPreviewMaxWidthProperty =
        DependencyProperty.Register(nameof(SelectedItemsPreviewMaxWidth), typeof(double), typeof(CheckComboBox),
            new FrameworkPropertyMetadata(double.PositiveInfinity, null, CoerceSelectedItemsPreviewMaxWidth));

    public static readonly DependencyProperty PlaceholderProperty =
        DependencyProperty.Register(nameof(Placeholder), typeof(string), typeof(CheckComboBox),
            new PropertyMetadata("Select items...", OnPlaceholderChanged));

    public static readonly DependencyProperty IsDropDownOpenProperty =
        DependencyProperty.Register(nameof(IsDropDownOpen), typeof(bool), typeof(CheckComboBox),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnIsDropDownOpenChanged));

    public static readonly DependencyProperty ShowSelectAllProperty =
        DependencyProperty.Register(nameof(ShowSelectAll), typeof(bool), typeof(CheckComboBox),
            new PropertyMetadata(true));

    public static readonly DependencyProperty SelectAllTextProperty =
        DependencyProperty.Register(nameof(SelectAllText), typeof(string), typeof(CheckComboBox),
            new PropertyMetadata("Select All Items"));

    public static readonly DependencyProperty MaxDropDownHeightProperty =
        DependencyProperty.Register(nameof(MaxDropDownHeight), typeof(double), typeof(CheckComboBox),
            new PropertyMetadata(260.0));

    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(CheckComboBox),
            new PropertyMetadata(new CornerRadius(6)));

    #endregion

    #region Routed Events

    public static readonly RoutedEvent SelectionChangedEvent =
        EventManager.RegisterRoutedEvent(nameof(SelectionChanged), RoutingStrategy.Bubble,
            typeof(RoutedEventHandler), typeof(CheckComboBox));

    public event RoutedEventHandler SelectionChanged
    {
        add => AddHandler(SelectionChangedEvent, value);
        remove => RemoveHandler(SelectionChangedEvent, value);
    }

    #endregion

    #region Properties

    public IEnumerable ItemsSource
    {
        get => (IEnumerable)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public string SelectedValue
    {
        get => (string)GetValue(SelectedValueProperty);
        set => SetValue(SelectedValueProperty, value);
    }

    public string DisplayText
    {
        get => (string)GetValue(DisplayTextProperty);
        set => SetValue(DisplayTextProperty, value);
    }

    public double SelectedItemsPreviewMaxWidth
    {
        get => (double)GetValue(SelectedItemsPreviewMaxWidthProperty);
        set => SetValue(SelectedItemsPreviewMaxWidthProperty, value);
    }

    public string Placeholder
    {
        get => (string)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    public bool IsDropDownOpen
    {
        get => (bool)GetValue(IsDropDownOpenProperty);
        set => SetValue(IsDropDownOpenProperty, value);
    }

    public bool ShowSelectAll
    {
        get => (bool)GetValue(ShowSelectAllProperty);
        set => SetValue(ShowSelectAllProperty, value);
    }

    public string SelectAllText
    {
        get => (string)GetValue(SelectAllTextProperty);
        set => SetValue(SelectAllTextProperty, value);
    }

    public double MaxDropDownHeight
    {
        get => (double)GetValue(MaxDropDownHeightProperty);
        set => SetValue(MaxDropDownHeightProperty, value);
    }

    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    public ObservableCollection<CheckComboBoxItem> InternalItems { get; } = new();

    public IList<object> SelectedItems => InternalItems.Where(x => x.IsSelected).Select(x => x.Content!).ToList();

    #endregion

    public override void OnApplyTemplate()
    {
        if (_selectAllCheckBox != null) _selectAllCheckBox.Click -= OnSelectAllClick;
        if (_button != null) _button.Click -= OnButtonClick;

        base.OnApplyTemplate();

        _toggleButton = (GetTemplateChild("PART_DropDownToggle") ?? GetTemplateChild("PART_Button")) as ToggleButton;
        _button = GetTemplateChild("PART_Button") as ButtonBase;
        _displayText = GetTemplateChild("PART_DisplayText") as TextBlock;
        _popup = GetTemplateChild("PART_Popup") as Popup;
        _selectAllCheckBox = GetTemplateChild("PART_SelectAll") as CheckBox;

        // If template uses a regular Button instead of ToggleButton, bind click to toggle
        if (_button != null && _button is not ToggleButton)
        {
            _button.Click += OnButtonClick;
        }

        if (_selectAllCheckBox != null)
        {
            _selectAllCheckBox.Click += OnSelectAllClick;
        }

        RefreshItems(forceRecreate: false);
    }

    private void OnButtonClick(object sender, RoutedEventArgs e)
    {
        IsDropDownOpen = !IsDropDownOpen;
    }

    private static void OnIsDropDownOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is CheckComboBox ccb && ccb._popup != null)
        {
            bool isOpen = (bool)e.NewValue;
            if (ccb._popup.IsOpen != isOpen)
            {
                ccb._popup.IsOpen = isOpen;
            }
        }
    }

    private static void OnPlaceholderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is CheckComboBox ccb)
        {
            ccb.RefreshDisplay();
        }
    }

    private static object CoerceSelectedItemsPreviewMaxWidth(DependencyObject d, object baseValue)
    {
        double width = (double)baseValue;
        return double.IsNaN(width) || width < 0 ? double.PositiveInfinity : width;
    }

    private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is CheckComboBox ccb)
        {
            ccb.RefreshItems(forceRecreate: true);
        }
    }

    private static void OnSelectedValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is CheckComboBox ccb && !ccb._isInternalValueUpdate)
        {
            var val = e.NewValue as string;
            if (string.IsNullOrEmpty(val))
            {
                ccb.UnselectAll();
            }
            else
            {
                var parts = val.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(part => part.Trim())
                    .Where(part => part.Length > 0)
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);
                ccb._isUpdatingAll = true;
                try
                {
                    foreach (var item in ccb.InternalItems)
                    {
                        item.IsSelected = parts.Contains(item.Content?.ToString() ?? string.Empty);
                    }
                }
                finally { ccb._isUpdatingAll = false; }
                ccb.RefreshDisplay();
            }
        }
    }

    private void RefreshItems(bool forceRecreate)
    {
        if (forceRecreate || InternalItems.Count == 0)
        {
            var oldSelected = InternalItems.Where(x => x.IsSelected).Select(x => x.Content?.ToString()).Where(x => x != null).ToHashSet();
            InternalItems.Clear();
            if (ItemsSource != null)
            {
                foreach (var obj in ItemsSource)
                {
                    var item = new CheckComboBoxItem
                    {
                        Content = obj,
                        IsSelected = oldSelected.Contains(obj?.ToString()),
                        Parent = this
                    };
                    InternalItems.Add(item);
                }
            }
        }
        RefreshDisplay();
    }

    public void OnItemSelectionToggled()
    {
        if (!_isUpdatingAll)
        {
            RefreshDisplay();
        }
    }

    public void SelectAll()
    {
        _isUpdatingAll = true;
        try
        {
            foreach (var item in InternalItems) item.IsSelected = true;
        }
        finally { _isUpdatingAll = false; }
        RefreshDisplay();
    }

    public void UnselectAll()
    {
        _isUpdatingAll = true;
        try
        {
            foreach (var item in InternalItems) item.IsSelected = false;
        }
        finally { _isUpdatingAll = false; }
        RefreshDisplay();
    }

    private void OnSelectAllClick(object sender, RoutedEventArgs e)
    {
        if (_selectAllCheckBox == null) return;
        _isUpdatingAll = true;
        try
        {
            bool isChecked = _selectAllCheckBox.IsChecked == true;
            foreach (var item in InternalItems) item.IsSelected = isChecked;
        }
        finally { _isUpdatingAll = false; }
        RefreshDisplay();
    }

    private void RefreshDisplay()
    {
        var selected = InternalItems.Where(x => x.IsSelected).ToList();
        string text;
        Brush foregroundBrush;

        if (selected.Count == 0)
        {
            text = Placeholder;
            foregroundBrush = TryFindResource("CONCOST.Brush.TextTertiary") as Brush ?? Brushes.Gray;
        }
        else if (selected.Count == 1)
        {
            text = selected[0].ToString();
            foregroundBrush = TryFindResource("CONCOST.Brush.TextPrimary") as Brush ?? Brushes.Black;
        }
        else if (selected.Count == InternalItems.Count && InternalItems.Count > 1)
        {
            text = $"All Selected ({selected.Count})";
            foregroundBrush = TryFindResource("CONCOST.Brush.Primary") as Brush ?? Brushes.DodgerBlue;
        }
        else if (selected.Count == 2)
        {
            text = $"{selected[0]}, {selected[1]}";
            foregroundBrush = TryFindResource("CONCOST.Brush.TextPrimary") as Brush ?? Brushes.Black;
        }
        else
        {
            text = $"{selected.Count} Items Selected ({selected[0]}, {selected[1]}...)";
            foregroundBrush = TryFindResource("CONCOST.Brush.TextPrimary") as Brush ?? Brushes.Black;
        }

        DisplayText = text;
        if (_displayText != null)
        {
            _displayText.Text = text;
            _displayText.Foreground = foregroundBrush;
        }

        if (_selectAllCheckBox != null && !_isUpdatingAll)
        {
            if (selected.Count == 0)
            {
                _selectAllCheckBox.IsChecked = false;
            }
            else if (selected.Count == InternalItems.Count)
            {
                _selectAllCheckBox.IsChecked = true;
            }
            else
            {
                _selectAllCheckBox.IsChecked = null;
            }
        }

        _isInternalValueUpdate = true;
        try
        {
            SelectedValue = string.Join(", ", selected.Select(x => x.ToString()));
        }
        finally { _isInternalValueUpdate = false; }

        RaiseEvent(new RoutedEventArgs(SelectionChangedEvent, this));
    }
}
