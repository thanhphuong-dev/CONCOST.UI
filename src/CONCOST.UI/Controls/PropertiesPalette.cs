using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using CONCOST.UI.AttachedProperties;
using CONCOST.UI.Icons;
using CONCOST.UI.Services;

namespace CONCOST.UI.Controls;

/// <summary>
/// The properties palette: selection header, type selector, search, the category grid, a validation
/// row and a reset button.
/// </summary>
/// <remarks>
/// <para>
/// It is a thin shell over <see cref="PropertyGrid"/>: the rows are built from
/// <see cref="PropertyManager"/> descriptors rather than by reflection, which is what lets a row carry
/// a unit, a fixed choice list or a validator. Rebuilding on every selection change keeps a stale row
/// from writing to the previous object, the classic properties-palette bug.
/// </para>
/// <para>
/// Edits go out through the manager, so they are validated and transacted in one place. The palette
/// never mutates the model itself.
/// </para>
/// </remarks>
public class PropertiesPalette : Grid
{
    private readonly PropertyManager _properties;
    private readonly SelectionManager _selection;

    private readonly TextBlock _title;
    private readonly TextBlock _subtitle;
    private readonly Grid _header;
    private readonly ComboBox _typeSelector;
    private readonly Button _editType;
    private readonly Button _reset;
    private readonly SearchBox _search;
    private readonly Border _searchBorder;
    private readonly TextBlock _validation;
    private readonly Grid _typeRow;
    private readonly Border _typeBorder;
    private readonly Grid _body;
    private readonly StackPanel _emptyState;
    private readonly TextBlock _emptyHint;
    private readonly List<PropertyItem> _items = new();

    private object? _target;
    private PropertyItem? _lastTouched;
    private bool _syncing;

    /// <summary>Identifies the <see cref="EmptyTitle"/> property.</summary>
    public static readonly DependencyProperty EmptyTitleProperty =
        DependencyProperty.Register(nameof(EmptyTitle), typeof(string), typeof(PropertiesPalette),
            new PropertyMetadata("Nothing selected", OnChromeChanged));

    /// <summary>Identifies the <see cref="EmptySubtitle"/> property.</summary>
    public static readonly DependencyProperty EmptySubtitleProperty =
        DependencyProperty.Register(nameof(EmptySubtitle), typeof(string), typeof(PropertiesPalette),
            new PropertyMetadata("Select an element to edit its parameters.", OnChromeChanged));

    /// <summary>Identifies the <see cref="EmptyHint"/> property.</summary>
    public static readonly DependencyProperty EmptyHintProperty =
        DependencyProperty.Register(nameof(EmptyHint), typeof(string), typeof(PropertiesPalette),
            new PropertyMetadata("Pick a wall in the drawing to see its type, dimensions and constraints.", OnChromeChanged));

    /// <summary>Identifies the <see cref="SearchPlaceholder"/> property.</summary>
    public static readonly DependencyProperty SearchPlaceholderProperty =
        DependencyProperty.Register(nameof(SearchPlaceholder), typeof(string), typeof(PropertiesPalette),
            new PropertyMetadata("Search parameters", OnChromeChanged));

    /// <summary>Identifies the <see cref="EditTypeText"/> property.</summary>
    public static readonly DependencyProperty EditTypeTextProperty =
        DependencyProperty.Register(nameof(EditTypeText), typeof(string), typeof(PropertiesPalette),
            new PropertyMetadata("Edit Type", OnChromeChanged));

    /// <summary>Identifies the <see cref="TypeToolTip"/> property.</summary>
    public static readonly DependencyProperty TypeToolTipProperty =
        DependencyProperty.Register(nameof(TypeToolTip), typeof(string), typeof(PropertiesPalette),
            new PropertyMetadata("Instance type", OnChromeChanged));

    /// <summary>Identifies the <see cref="ResetToolTip"/> property.</summary>
    public static readonly DependencyProperty ResetToolTipProperty =
        DependencyProperty.Register(nameof(ResetToolTip), typeof(string), typeof(PropertiesPalette),
            new PropertyMetadata("Reset the last edited parameter to its default", OnChromeChanged));

    /// <summary>Identifies the <see cref="ShowSearch"/> property.</summary>
    public static readonly DependencyProperty ShowSearchProperty =
        DependencyProperty.Register(nameof(ShowSearch), typeof(bool), typeof(PropertiesPalette),
            new PropertyMetadata(true, OnChromeChanged));

    /// <summary>Uses the flat parameter rows and type selector spacing of Revit's Properties palette.</summary>
    public static readonly DependencyProperty RevitLayoutProperty =
        DependencyProperty.Register(nameof(RevitLayout), typeof(bool), typeof(PropertiesPalette),
            new PropertyMetadata(false, OnChromeChanged));

    /// <summary>Creates a palette over the shared services, or over the ones a host passes in.</summary>
    public PropertiesPalette(PropertyManager? properties = null, SelectionManager? selection = null)
    {
        _properties = properties ?? PropertyManager.Current;
        _selection = selection ?? SelectionManager.Current;

        RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        _title = new TextBlock { FontSize = 13, FontWeight = FontWeights.SemiBold, TextTrimming = TextTrimming.CharacterEllipsis };
        _title.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextPrimary");
        _title.Text = EmptyTitle;

        // One pixel of air: at eleven pixels the subtitle otherwise sits on the descenders of the
        // title and the pair reads as a single smudged line.
        _subtitle = new TextBlock { FontSize = 11, Margin = new Thickness(0, 1, 0, 0), TextTrimming = TextTrimming.CharacterEllipsis };
        _subtitle.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");
        _subtitle.Text = EmptySubtitle;

        _typeSelector = new ComboBox { Height = 26, FontSize = 12, ToolTip = TypeToolTip };
        _typeSelector.SelectionChanged += OnTypeSelected;

        _editType = new Button { Content = EditTypeText, Height = 26, Padding = new Thickness(8, 0, 8, 0), FontSize = 11, Margin = new Thickness(4, 0, 0, 0) };
        _editType.SetResourceReference(StyleProperty, "CONCOST.Button.Secondary");
        _editType.Click += (_, _) => _editTypeRequested?.Invoke(this, EventArgs.Empty);

        _typeRow = new Grid();
        _typeRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        _typeRow.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        _typeRow.Children.Add(_typeSelector);
        Grid.SetColumn(_editType, 1);
        _typeRow.Children.Add(_editType);

        _typeBorder = new Border
        {
            BorderThickness = new Thickness(0, 0, 0, 1),
            Padding = new Thickness(8, 6, 8, 6),
            Visibility = Visibility.Collapsed,
            SnapsToDevicePixels = true
        };
        _typeBorder.SetResourceReference(Border.BorderBrushProperty, "CONCOST.Brush.Border");
        _typeBorder.Child = _typeRow;

        _reset = new Button
        {
            Width = 24,
            Height = 24,
            ToolTip = ResetToolTip,
            Margin = new Thickness(4, 0, 0, 0)
        };

        _reset.SetResourceReference(StyleProperty, "CONCOST.Button.Icon");
        IconElement.SetKind(_reset, IconKind.Refresh);
        IconElement.SetIconSize(_reset, 13);
        _reset.Click += (_, _) => ResetLastTouched();

        _header = new Grid { Margin = new Thickness(10, 8, 10, 6) };
        _header.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        _header.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        var captions = new StackPanel();
        captions.Children.Add(_title);
        captions.Children.Add(_subtitle);
        _header.Children.Add(captions);

        Grid.SetColumn(_reset, 1);
        _header.Children.Add(_reset);

        _search = new SearchBox
        {
            Placeholder = SearchPlaceholder,
            Height = 26,
            Margin = new Thickness(0),
            FontSize = 11.5,
            CornerRadius = new CornerRadius(0)
        };

        _search.TextChanged += (_, _) => Build(_target);

        _searchBorder = new Border
        {
            BorderThickness = new Thickness(0, 0, 0, 1),
            Height = 28,
            Padding = new Thickness(6, 0, 6, 0),
            SnapsToDevicePixels = true
        };
        _searchBorder.SetResourceReference(Border.BorderBrushProperty, "CONCOST.Brush.Border");
        _searchBorder.Child = _search;

        // No border and no toolbar: the palette already owns the search box, and the grid's own
        // filter passes would do nothing here — the palette fills Categories itself rather than
        // handing the grid a SelectedObject to reflect over.
        GridControl = new PropertyGrid
        {
            Margin = new Thickness(0),
            BorderThickness = new Thickness(0),
            ShowToolbar = false
        };

        _validation = new TextBlock
        {
            FontSize = 11,
            Margin = new Thickness(12, 4, 12, 8),
            TextWrapping = TextWrapping.Wrap,
            Visibility = Visibility.Collapsed
        };

        _validation.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.Danger");

        SetRow(_header, 0);
        Children.Add(_header);
        SetRow(_typeBorder, 1);
        Children.Add(_typeBorder);

        // Search and grid travel together in one row so the empty state leaves no gap behind.
        // A Grid, not a StackPanel: the grid's own template is a star row, and a star row measured
        // inside a stack panel collapses to nothing — the rows would vanish the moment a wall
        // was selected.
        _body = new Grid();
        _body.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        _body.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        _body.Children.Add(_searchBorder);

        SetRow(GridControl, 1);
        _body.Children.Add(GridControl);

        SetRow(_body, 2);
        Children.Add(_body);

        // An empty palette is a tall white column that reads as a control that failed to load. The
        // hint sits where the rows will appear, so the panel looks deliberate with nothing selected.
        _emptyState = new StackPanel
        {
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(20, 0, 20, 40)
        };

        var emptyIcon = new Icon { Kind = IconKind.SettingsSliders, Width = 24, Height = 24, Opacity = 0.55 };
        emptyIcon.SetResourceReference(Control.ForegroundProperty, "CONCOST.Brush.TextSecondary");
        _emptyState.Children.Add(emptyIcon);

        _emptyHint = new TextBlock
        {
            Text = EmptyHint,
            FontSize = 11,
            TextWrapping = TextWrapping.Wrap,
            TextAlignment = TextAlignment.Center,
            MaxWidth = 210,
            Margin = new Thickness(0, 10, 0, 0)
        };

        _emptyHint.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");
        _emptyState.Children.Add(_emptyHint);

        SetRow(_emptyState, 2);
        Children.Add(_emptyState);

        SetRow(_validation, 3);
        Children.Add(_validation);

        _selection.SelectionChanged += (_, e) => Build(e.Primary);
        _properties.ValueChanged += (_, e) => OnValueChanged(e);

        Build(_selection.Primary);
    }

    /// <summary>Title shown when nothing is selected.</summary>
    public string EmptyTitle
    {
        get => (string)GetValue(EmptyTitleProperty);
        set => SetValue(EmptyTitleProperty, value);
    }

    /// <summary>Subtitle shown when nothing is selected.</summary>
    public string EmptySubtitle
    {
        get => (string)GetValue(EmptySubtitleProperty);
        set => SetValue(EmptySubtitleProperty, value);
    }

    /// <summary>Hint shown in the empty state.</summary>
    public string EmptyHint
    {
        get => (string)GetValue(EmptyHintProperty);
        set => SetValue(EmptyHintProperty, value);
    }

    /// <summary>Watermark of the parameter search box.</summary>
    public string SearchPlaceholder
    {
        get => (string)GetValue(SearchPlaceholderProperty);
        set => SetValue(SearchPlaceholderProperty, value);
    }

    /// <summary>Caption of the type-edit button.</summary>
    public string EditTypeText
    {
        get => (string)GetValue(EditTypeTextProperty);
        set => SetValue(EditTypeTextProperty, value);
    }

    /// <summary>Tooltip of the instance-type selector.</summary>
    public string TypeToolTip
    {
        get => (string)GetValue(TypeToolTipProperty);
        set => SetValue(TypeToolTipProperty, value);
    }

    /// <summary>Tooltip of the reset button.</summary>
    public string ResetToolTip
    {
        get => (string)GetValue(ResetToolTipProperty);
        set => SetValue(ResetToolTipProperty, value);
    }

    /// <summary>Shows the parameter search box.</summary>
    public bool ShowSearch
    {
        get => (bool)GetValue(ShowSearchProperty);
        set => SetValue(ShowSearchProperty, value);
    }

    /// <summary>Uses Revit-style parameter rows without changing the default CONCOST palette.</summary>
    public bool RevitLayout
    {
        get => (bool)GetValue(RevitLayoutProperty);
        set => SetValue(RevitLayoutProperty, value);
    }

    /// <summary>The grid that renders the rows.</summary>
    public PropertyGrid GridControl { get; }

    /// <summary>The property system the palette reads and writes through.</summary>
    public PropertyManager Properties => _properties;

    /// <summary>The selection the palette follows.</summary>
    public SelectionManager Selection => _selection;

    /// <summary>The object whose parameters are shown, or <c>null</c>.</summary>
    public object? Target => _target;

    /// <summary>Last validation message, or <c>null</c> when the last write succeeded.</summary>
    public string? ValidationMessage => _validation.Visibility == Visibility.Visible ? _validation.Text : null;

    private EventHandler? _editTypeRequested;

    /// <summary>Raised when "Edit type" is pressed.</summary>
    public event EventHandler? EditTypeRequested
    {
        add
        {
            _editTypeRequested += value;
            _editType.IsEnabled = true;
            _editType.ToolTip = EditTypeText;
        }
        remove
        {
            _editTypeRequested -= value;
            _editType.IsEnabled = _editTypeRequested != null;
            _editType.ToolTip = _editTypeRequested == null
                ? "Type editing is not available in this build."
                : EditTypeText;
        }
    }

    /// <summary>Raised when another type is picked in the type selector.</summary>
    public event EventHandler<string>? TypeChangeRequested;

    /// <summary>Raised after a parameter was written.</summary>
    public event EventHandler<PropertyValueChangedEventArgs>? PropertyCommitted;

    /// <summary>Rebuilds the rows for <paramref name="target"/>; pass <c>null</c> for the empty state.</summary>
    private static void OnChromeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is PropertiesPalette palette) palette.ApplyChrome();
    }

    private void ApplyChrome()
    {
        _emptyHint.Text = EmptyHint;
        _search.Placeholder = SearchPlaceholder;
        _editType.Content = EditTypeText;
        _typeSelector.ToolTip = TypeToolTip;
        _reset.ToolTip = ResetToolTip;
        GridControl.SetResourceReference(StyleProperty,
            RevitLayout ? "CONCOST.PropertyGrid.Revit" : "CONCOST.PropertyGrid.Compact");
        _reset.Visibility = RevitLayout ? Visibility.Collapsed : Visibility.Visible;

        // Keep the chrome in sync when a host switches variants after the palette has
        // already been built. Revit uses the type selector as its heading; Compact keeps
        // the object title above it.
        _header.Visibility = _target is null || !RevitLayout || _target is not ITypeSelectable
            ? Visibility.Visible
            : Visibility.Collapsed;
        _typeBorder.Visibility = _target is ITypeSelectable ? Visibility.Visible : Visibility.Collapsed;

        if (RevitLayout)
        {
            _typeBorder.BorderThickness = new Thickness(0, 0, 0, 1);
            _typeBorder.Padding = new Thickness(8, 6, 8, 6);
            _searchBorder.BorderThickness = new Thickness(0, 0, 0, 1);
            _searchBorder.Padding = new Thickness(6, 0, 6, 0);
            _searchBorder.Height = 28;
            _search.BorderThickness = new Thickness(0);
            _search.Background = Brushes.Transparent;
            _search.Height = 26;
            _search.CornerRadius = new CornerRadius(0);
            _search.Margin = new Thickness(0);
            GridControl.Margin = new Thickness(0);
        }
        else
        {
            _typeBorder.BorderThickness = new Thickness(0);
            _typeBorder.Padding = new Thickness(10, 6, 10, 0);
            _searchBorder.BorderThickness = new Thickness(0);
            _searchBorder.Padding = new Thickness(0);
            _searchBorder.Height = double.NaN;
            _search.BorderThickness = new Thickness(1);
            _search.ClearValue(Control.BackgroundProperty);
            _search.Height = 28;
            _search.CornerRadius = new CornerRadius(6);
            _search.Margin = new Thickness(10, 0, 10, 6);
            GridControl.Margin = new Thickness(6, 0, 6, 0);
        }

        if (_target is null)
        {
            _title.Text = EmptyTitle;
            _subtitle.Text = EmptySubtitle;
            _search.Visibility = Visibility.Collapsed;
            _searchBorder.Visibility = Visibility.Collapsed;
            return;
        }

        _search.Visibility = ShowSearch ? Visibility.Visible : Visibility.Collapsed;
        _searchBorder.Visibility = ShowSearch ? Visibility.Visible : Visibility.Collapsed;
    }

    public void Build(object? target)
    {
        _syncing = true;

        try
        {
            _target = target;
            _lastTouched = null;

            GridControl.Categories.Clear();
            _items.Clear();
            ShowValidation(null);

            _body.Visibility = target is null ? Visibility.Collapsed : Visibility.Visible;
            _emptyState.Visibility = target is null ? Visibility.Visible : Visibility.Collapsed;

            if (target is null)
            {
                _header.Visibility = Visibility.Visible;
                _title.Text = EmptyTitle;
                _subtitle.Text = EmptySubtitle;
                _subtitle.Visibility = Visibility.Visible;
                _typeBorder.Visibility = Visibility.Collapsed;
                _search.Visibility = Visibility.Collapsed;
                _searchBorder.Visibility = Visibility.Collapsed;
                return;
            }

            // Revit puts the type selector at the top of the palette. A second "Wall" title above
            // it consumes a row and duplicates the category already conveyed by the selector.
            _header.Visibility = target is ITypeSelectable ? Visibility.Collapsed : Visibility.Visible;

            _search.Visibility = ShowSearch ? Visibility.Visible : Visibility.Collapsed;
            _searchBorder.Visibility = ShowSearch ? Visibility.Visible : Visibility.Collapsed;

            string filter = _search.Text ?? string.Empty;

            int total = 0;
            var descriptors = _properties.GetDescriptors(target);

            foreach (var group in descriptors.GroupBy(descriptor => descriptor.Category)
                         .OrderBy(group => CategoryOrder(group.Key))
                         .ThenBy(group => group.Key, StringComparer.Ordinal))
            {
                var category = new PropertyCategoryGroup { CategoryName = group.Key, IsExpanded = true };

                foreach (var descriptor in group)
                {
                    total++;

                    if (filter.Length > 0 && descriptor.DisplayName.IndexOf(filter, StringComparison.OrdinalIgnoreCase) < 0) continue;

                    var item = CreateItem(target, descriptor);
                    category.Properties.Add(item);
                    _items.Add(item);
                }

                if (category.Properties.Count > 0) GridControl.Categories.Add(category);
            }

            if (target is IPropertyPaletteDisplay display)
            {
                _title.Text = display.PropertyTitle;
                _subtitle.Text = display.PropertySubtitle;
            }
            else
            {
                _title.Text = target.GetType().Name;
                _subtitle.Text = _selection.Count > 1
                    ? $"{_selection.Count} selected — showing the last one. {total} parameters."
                    : $"{total} parameters.";
            }

            SyncTypeSelector(target);
        }
        finally
        {
            _syncing = false;
        }
    }

    /// <summary>Rebuilds the rows for the current target, e.g. after the model changed underneath.</summary>
    public void Refresh() => Build(_target);

    /// <summary>Puts one parameter back to its registered default, as a transaction.</summary>
    public bool ResetProperty(string name, out string? error)
    {
        error = null;

        if (_target is null)
        {
            error = "Nothing is selected.";
            return false;
        }

        var descriptor = _properties.Find(_target, name);
        if (descriptor is null)
        {
            error = $"'{name}' is not a property of {_target.GetType().Name}.";
            return false;
        }

        if (descriptor.IsReadOnly)
        {
            error = $"'{descriptor.DisplayName}' is read-only.";
            return false;
        }

        if (!_properties.TrySetValue(_target, name, descriptor.DefaultValue, out error))
        {
            ShowValidation(error);
            return false;
        }

        var item = _items.FirstOrDefault(candidate => string.Equals(candidate.Name, name, StringComparison.Ordinal));

        if (item is not null)
        {
            _syncing = true;
            item.Value = descriptor.DefaultValue;
            item.ValidationError = null;
            _syncing = false;
        }

        ShowValidation(null);
        return true;
    }

    private void ResetLastTouched()
    {
        if (_lastTouched is null)
        {
            ShowValidation("Edit a parameter first, then reset it.");
            return;
        }

        ResetProperty(_lastTouched.Name, out _);
    }

    private PropertyItem CreateItem(object target, PropertyDescriptor descriptor)
    {
        object? value = descriptor.GetValue(target);

        var item = new PropertyItem
        {
            Name = descriptor.Name,
            DisplayName = descriptor.DisplayName,
            Category = descriptor.Category,
            Description = descriptor.Description ?? string.Empty,
            PropertyType = value?.GetType() ?? typeof(string),
            EditorType = MapEditor(descriptor),
            EnumOptions = descriptor.Choices?.ToArray(),
            IsReadOnly = descriptor.IsReadOnly,
            Value = value
        };

        // The setter lives here rather than in a binding: a rejected value has to reach the row's
        // ValidationError and the palette's message line, and the model has to stay untouched.
        item.PropertyChanged += (_, e) =>
        {
            if (_syncing || e.PropertyName != nameof(PropertyItem.Value)) return;

            _lastTouched = item;

            if (!_properties.TrySetValueFromText(target, item.Name, item.Value?.ToString(), out var error))
            {
                item.ValidationError = error;
                ShowValidation(error);
                return;
            }

            item.ValidationError = null;
            ShowValidation(null);

            // A write can cascade (a type change relabels rows), so the display values are refreshed
            // from the model rather than assumed to match what was typed.
            RefreshValues();
        };

        return item;
    }

    private void RefreshValues()
    {
        if (_target is null) return;

        _syncing = true;

        try
        {
            foreach (var item in _items)
            {
                var descriptor = _properties.Find(_target, item.Name);
                if (descriptor is null) continue;

                item.Value = descriptor.GetValue(_target);
            }
        }
        finally
        {
            _syncing = false;
        }
    }

    private void OnValueChanged(PropertyValueChangedEventArgs e)
    {
        PropertyCommitted?.Invoke(this, e);

        ShowValidation(null);
    }

    private void SyncTypeSelector(object target)
    {
        if (target is not ITypeSelectable selectable)
        {
            _typeBorder.Visibility = Visibility.Collapsed;
            _subtitle.Visibility = Visibility.Visible;
            return;
        }

        var availableTypes = selectable.AvailableTypes;
        _syncing = true;

        try
        {
            _typeBorder.Visibility = Visibility.Visible;
            var choices = availableTypes.Count == 0 ? new[] { selectable.TypeName } : availableTypes;
            _typeSelector.ItemsSource = choices;
            _typeSelector.SelectedItem = selectable.TypeName;
            _typeSelector.IsEnabled = availableTypes.Count > 1;
            _editType.IsEnabled = _editTypeRequested is not null;
            _editType.ToolTip = _editTypeRequested is null
                ? "Type editing is not available in this build."
                : EditTypeText;
            _subtitle.Visibility = Visibility.Collapsed;
        }
        finally
        {
            _syncing = false;
        }
    }

    private void OnTypeSelected(object sender, SelectionChangedEventArgs e)
    {
        if (_syncing || _typeSelector.SelectedItem is not string type) return;

        TypeChangeRequested?.Invoke(this, type);
    }

    private void ShowValidation(string? message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            _validation.Visibility = Visibility.Collapsed;
            _validation.Text = string.Empty;
            return;
        }

        _validation.Text = message;
        _validation.Visibility = Visibility.Visible;
    }

    private static PropertyEditorType MapEditor(PropertyDescriptor descriptor) => descriptor.Editor switch
    {
        PropertyEditorKind.Numeric => PropertyEditorType.Number,
        PropertyEditorKind.Boolean => PropertyEditorType.Boolean,
        PropertyEditorKind.Choice => PropertyEditorType.Enum,
        _ => PropertyEditorType.Text
    };

    private static int CategoryOrder(string category) => category switch
    {
        "Graphics" => 0,
        "Underlay" => 1,
        "Extents" => 2,
        "Constraints" => 3,
        "Geometric Position" => 4,
        "Materials and Finishes" => 5,
        "Structural" => 6,
        "Dimensions" => 7,
        "Identity Data" => 8,
        "Phasing" => 9,
        "IFC Parameters" => 10,
        _ => 11,
    };
}
