using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using CONCOST.UI.AttachedProperties;

namespace CONCOST.UI.Controls;

/// <summary>Carries the object picked in a <see cref="PropertyReferencePicker"/>.</summary>
public class ReferencePickedEventArgs : EventArgs
{
    public ReferencePickedEventArgs(object? item) => Item = item;

    /// <summary>The object that was picked, or <c>null</c> when the selection was cleared.</summary>
    public object? Item { get; }
}

/// <summary>
/// A value cell for a property that points at another model object — a host wall, a level, a material
/// — with a drop-down list instead of free text.
/// </summary>
/// <remarks>
/// Picking from a list rather than typing a name is what keeps a reference from dangling: the cell can
/// only ever hold an object the caller offered, and the display text comes from the same object. The
/// list is searchable because a project can carry thousands of levels and materials.
/// </remarks>
public class PropertyReferencePicker : Grid
{
    /// <summary>Identifies the <see cref="SelectedItem"/> property.</summary>
    public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(
        nameof(SelectedItem), typeof(object), typeof(PropertyReferencePicker),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedItemChanged));

    /// <summary>Identifies the <see cref="Placeholder"/> property.</summary>
    public static readonly DependencyProperty PlaceholderProperty = DependencyProperty.Register(
        nameof(Placeholder), typeof(string), typeof(PropertyReferencePicker),
        new FrameworkPropertyMetadata("<none>"));

    /// <summary>Identifies the <see cref="IsReadOnly"/> property.</summary>
    public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register(
        nameof(IsReadOnly), typeof(bool), typeof(PropertyReferencePicker),
        new FrameworkPropertyMetadata(false, OnReadOnlyChanged));

    private readonly TextBlock _display;
    private readonly Button _dropDown;
    private readonly Popup _popup;
    private readonly ListBox _list;
    private readonly SearchBox _search;

    private List<object> _candidates = new();
    private bool _syncing;

    public PropertyReferencePicker()
    {
        ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        _display = new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(8, 0, 4, 0),
            TextTrimming = TextTrimming.CharacterEllipsis,
            FontSize = 12
        };

        _display.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextPrimary");

        _dropDown = new Button { Width = 24, Height = 24, Margin = new Thickness(0, 0, 2, 0), Focusable = false };
        _dropDown.SetResourceReference(StyleProperty, "CONCOST.Button.Icon");
        IconElement.SetKind(_dropDown, Icons.IconKind.ArrowDown);
        IconElement.SetIconSize(_dropDown, 12);
        _dropDown.Click += (_, _) => TogglePopup();

        var inner = new Grid();
        inner.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        inner.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        System.Windows.Controls.Grid.SetColumn(_display, 0);
        System.Windows.Controls.Grid.SetColumn(_dropDown, 1);

        inner.Children.Add(_display);
        inner.Children.Add(_dropDown);

        var surface = new Border
        {
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(4),
            Child = inner
        };

        surface.SetResourceReference(Border.BackgroundProperty, "CONCOST.Brush.InputBackground");
        surface.SetResourceReference(Border.BorderBrushProperty, "CONCOST.Brush.InputBorder");

        Children.Add(surface);

        _search = new SearchBox { Height = 26, Margin = new Thickness(4), FontSize = 12, Placeholder = "Search" };
        _search.TextChanged += (_, _) => ApplyFilter();

        _list = new ListBox { Height = 220, BorderThickness = new Thickness(0), FontSize = 12 };
        _list.SelectionChanged += OnListSelectionChanged;

        var panel = new DockPanel();
        DockPanel.SetDock(_search, Dock.Top);
        panel.Children.Add(_search);
        panel.Children.Add(_list);

        var frame = new Border
        {
            Width = 260,
            Padding = new Thickness(2),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Child = panel
        };

        frame.SetResourceReference(Border.BackgroundProperty, "CONCOST.Brush.Surface");
        frame.SetResourceReference(Border.BorderBrushProperty, "CONCOST.Brush.Border");

        _popup = new Popup
        {
            PlacementTarget = surface,
            Placement = PlacementMode.Bottom,
            StaysOpen = false,
            Child = frame,
            AllowsTransparency = true
        };

        _popup.Closed += (_, _) => ApplyFilter(clearSearch: true);

        UpdateDisplay();
    }

    /// <summary>The objects that can be picked.</summary>
    public IEnumerable? ItemsSource { get; set; }

    /// <summary>How an object is shown; defaults to <c>ToString()</c>.</summary>
    public Func<object, string>? DisplaySelector { get; set; }

    /// <summary>The object currently referenced.</summary>
    public object? SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    /// <summary>Text shown when nothing is referenced.</summary>
    public string Placeholder
    {
        get => (string)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    /// <summary>Blocks the picker without disabling it, so the value stays selectable for copying.</summary>
    public bool IsReadOnly
    {
        get => (bool)GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    /// <summary>Raised when a different object is picked or the reference is cleared.</summary>
    public event EventHandler<ReferencePickedEventArgs>? ReferencePicked;

    /// <summary>Opens or closes the drop-down list.</summary>
    public void TogglePopup()
    {
        if (IsReadOnly) return;

        if (_popup.IsOpen)
        {
            _popup.IsOpen = false;
            return;
        }

        _candidates = (ItemsSource?.Cast<object>() ?? Enumerable.Empty<object>())
            .Where(item => item is not null)
            .ToList();

        ApplyFilter();
        _popup.IsOpen = true;
    }

    private void ApplyFilter(bool clearSearch = false)
    {
        if (clearSearch) _search.Text = string.Empty;

        string filter = _search.Text ?? string.Empty;

        var visible = filter.Length == 0
            ? _candidates
            : _candidates.Where(item => Label(item).IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0).ToList();

        _syncing = true;

        try
        {
            _list.ItemsSource = visible;
            _list.SelectedItem = visible.FirstOrDefault(item => ReferenceEquals(item, SelectedItem));
        }
        finally
        {
            _syncing = false;
        }
    }

    private void OnListSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_syncing) return;

        var picked = _list.SelectedItem;

        _popup.IsOpen = false;
        SelectedItem = picked;
    }

    private static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not PropertyReferencePicker picker) return;

        picker.UpdateDisplay();

        if (picker._syncing) return;

        picker.ReferencePicked?.Invoke(picker, new ReferencePickedEventArgs(e.NewValue));
    }

    private static void OnReadOnlyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is PropertyReferencePicker picker) picker._dropDown.IsEnabled = !(bool)e.NewValue;
    }

    private void UpdateDisplay()
    {
        bool empty = SelectedItem is null;

        _display.Text = empty ? Placeholder : Label(SelectedItem!);
        _display.Opacity = empty ? 0.6 : 1;
    }

    private string Label(object item) => DisplaySelector?.Invoke(item) ?? item.ToString() ?? string.Empty;
}
