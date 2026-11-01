using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace CONCOST.UI.Controls;

public enum PropertyEditorType
{
    Text,
    Number,
    Boolean,
    Enum,
    HorizontalAlignment,
    VerticalAlignment,
    ImageSource
}

public class PropertyItem : INotifyPropertyChanged
{
    private object? _value;
    private string? _validationError;

    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Category { get; set; } = "Miscellaneous";
    public string Description { get; set; } = string.Empty;
    public Type PropertyType { get; set; } = typeof(string);
    public PropertyEditorType EditorType { get; set; } = PropertyEditorType.Text;
    public Array? EnumOptions { get; set; }
    public bool IsReadOnly { get; set; }
    public string? ValidationError
    {
        get => _validationError;
        set
        {
            if (_validationError == value) return;
            _validationError = value;
            OnPropertyChanged(nameof(ValidationError));
        }
    }

    public object? Value
    {
        get => _value;
        set
        {
            if (!Equals(_value, value))
            {
                _value = value;
                OnPropertyChanged(nameof(Value));
                OnPropertyChanged(nameof(IsBoolTrue));
                OnPropertyChanged(nameof(IsAlignLeft));
                OnPropertyChanged(nameof(IsAlignCenter));
                OnPropertyChanged(nameof(IsAlignRight));
                OnPropertyChanged(nameof(IsAlignStretch));
                OnPropertyChanged(nameof(IsValignTop));
                OnPropertyChanged(nameof(IsValignCenter));
                OnPropertyChanged(nameof(IsValignBottom));
                OnPropertyChanged(nameof(IsValignStretch));
            }
        }
    }

    public bool IsBoolTrue
    {
        get => Value is bool b && b;
        set => Value = value;
    }

    public bool IsAlignLeft
    {
        get => Value is HorizontalAlignment h && h == HorizontalAlignment.Left;
        set { if (value) Value = HorizontalAlignment.Left; }
    }
    public bool IsAlignCenter
    {
        get => Value is HorizontalAlignment h && h == HorizontalAlignment.Center;
        set { if (value) Value = HorizontalAlignment.Center; }
    }
    public bool IsAlignRight
    {
        get => Value is HorizontalAlignment h && h == HorizontalAlignment.Right;
        set { if (value) Value = HorizontalAlignment.Right; }
    }
    public bool IsAlignStretch
    {
        get => Value is HorizontalAlignment h && h == HorizontalAlignment.Stretch;
        set { if (value) Value = HorizontalAlignment.Stretch; }
    }

    public bool IsValignTop
    {
        get => Value is VerticalAlignment v && v == VerticalAlignment.Top;
        set { if (value) Value = VerticalAlignment.Top; }
    }
    public bool IsValignCenter
    {
        get => Value is VerticalAlignment v && v == VerticalAlignment.Center;
        set { if (value) Value = VerticalAlignment.Center; }
    }
    public bool IsValignBottom
    {
        get => Value is VerticalAlignment v && v == VerticalAlignment.Bottom;
        set { if (value) Value = VerticalAlignment.Bottom; }
    }
    public bool IsValignStretch
    {
        get => Value is VerticalAlignment v && v == VerticalAlignment.Stretch;
        set { if (value) Value = VerticalAlignment.Stretch; }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected virtual void OnPropertyChanged(string propName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
}

public class PropertyCategoryGroup : INotifyPropertyChanged
{
    private bool _isExpanded = true;
    public string CategoryName { get; set; } = string.Empty;
    public bool IsExpanded
    {
        get => _isExpanded;
        set { _isExpanded = value; OnPropertyChanged(nameof(IsExpanded)); }
    }
    public ObservableCollection<PropertyItem> Properties { get; } = new();

    public event PropertyChangedEventHandler? PropertyChanged;
    protected virtual void OnPropertyChanged(string propName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
}

public class PropertyGrid : Control
{
    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> PropertyCache = new();
    private TextBox? _searchBox;
    private RadioButton? _sortCategoryButton;
    private RadioButton? _sortNameButton;
    private bool _sortByCategory = true;
    private string _filterQuery = string.Empty;
    private INotifyPropertyChanged? _observedObject;
    private readonly HashSet<PropertyItem> _updatingFromObject = new();

    static PropertyGrid()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(PropertyGrid), new FrameworkPropertyMetadata(typeof(PropertyGrid)));
    }

    public static readonly DependencyProperty SelectedObjectProperty =
        DependencyProperty.Register(nameof(SelectedObject), typeof(object), typeof(PropertyGrid),
            new PropertyMetadata(null, OnSelectedObjectChanged));

    public static readonly DependencyProperty CategoriesProperty =
        DependencyProperty.Register(nameof(Categories), typeof(ObservableCollection<PropertyCategoryGroup>), typeof(PropertyGrid),
            new PropertyMetadata(null));

    public static readonly DependencyProperty ShowToolbarProperty =
        DependencyProperty.Register(nameof(ShowToolbar), typeof(bool), typeof(PropertyGrid),
            new PropertyMetadata(true));

    public object? SelectedObject
    {
        get => GetValue(SelectedObjectProperty);
        set => SetValue(SelectedObjectProperty, value);
    }

    public ObservableCollection<PropertyCategoryGroup> Categories
    {
        get => (ObservableCollection<PropertyCategoryGroup>)GetValue(CategoriesProperty);
        set => SetValue(CategoriesProperty, value);
    }

    /// <summary>
    /// Shows the view switch and the filter box above the rows. A host that already offers its own
    /// filter turns it off rather than stacking a second search box over the first.
    /// </summary>
    public bool ShowToolbar
    {
        get => (bool)GetValue(ShowToolbarProperty);
        set => SetValue(ShowToolbarProperty, value);
    }

    public PropertyGrid()
    {
        Categories = new ObservableCollection<PropertyCategoryGroup>();
    }

    public override void OnApplyTemplate()
    {
        if (_searchBox != null) _searchBox.TextChanged -= OnSearchTextChanged;
        if (_sortCategoryButton != null) _sortCategoryButton.Checked -= OnSortByCategoryChecked;
        if (_sortNameButton != null) _sortNameButton.Checked -= OnSortByNameChecked;
        base.OnApplyTemplate();
        _searchBox = GetTemplateChild("PART_SearchBox") as TextBox;
        _sortCategoryButton = GetTemplateChild("PART_SortCategoryBtn") as RadioButton;
        _sortNameButton = GetTemplateChild("PART_SortNameBtn") as RadioButton;
        if (_searchBox != null) _searchBox.TextChanged += OnSearchTextChanged;
        if (_sortCategoryButton != null) _sortCategoryButton.Checked += OnSortByCategoryChecked;
        if (_sortNameButton != null) _sortNameButton.Checked += OnSortByNameChecked;

        // Only when there is an object to reflect over. A host that fills Categories itself owns
        // them, and clearing them on the first layout pass wipes the rows it just added — which is
        // exactly what happened when a palette only became visible at the moment of a selection.
        if (SelectedObject is not null) BuildProperties();
    }

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e) { if (SelectedObject is null) return; _filterQuery = _searchBox?.Text.Trim() ?? string.Empty; BuildProperties(); }
    private void OnSortByCategoryChecked(object sender, RoutedEventArgs e) { _sortByCategory = true; BuildProperties(); }
    private void OnSortByNameChecked(object sender, RoutedEventArgs e) { _sortByCategory = false; BuildProperties(); }

    private static void OnSelectedObjectChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is PropertyGrid pg)
        {
            if (pg._observedObject != null)
                pg._observedObject.PropertyChanged -= pg.OnSelectedObjectPropertyChanged;
            pg._observedObject = e.NewValue as INotifyPropertyChanged;
            if (pg._observedObject != null)
                pg._observedObject.PropertyChanged += pg.OnSelectedObjectPropertyChanged;
            pg.BuildProperties();
        }
    }

    private void OnSelectedObjectPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (!Dispatcher.CheckAccess())
        {
            Dispatcher.BeginInvoke(new Action(() => OnSelectedObjectPropertyChanged(sender, e)));
            return;
        }
        if (SelectedObject == null) return;
        foreach (var item in Categories.SelectMany(category => category.Properties))
        {
            if (string.IsNullOrEmpty(e.PropertyName) ||
                string.Equals(item.Name, e.PropertyName, StringComparison.Ordinal))
            {
                var property = PropertyCache.GetOrAdd(SelectedObject.GetType(), GetPublicProperties)
                    .FirstOrDefault(candidate => candidate.Name == item.Name);
                if (property == null) continue;
                try
                {
                    _updatingFromObject.Add(item);
                    item.Value = property.GetValue(SelectedObject);
                    item.ValidationError = null;
                }
                catch (TargetInvocationException) { }
                finally { _updatingFromObject.Remove(item); }
            }
        }
    }

    private static PropertyInfo[] GetPublicProperties(Type type) =>
        type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

    public void BuildProperties()
    {
        Categories.Clear();
        if (SelectedObject == null) return;

        var type = SelectedObject.GetType();
        var props = PropertyCache.GetOrAdd(type, GetPublicProperties);
        var selectedObject = SelectedObject;
        var allItems = new List<PropertyItem>();

        foreach (var prop in props)
        {
            if (!prop.CanRead || prop.GetIndexParameters().Length != 0 ||
                prop.GetCustomAttribute<BrowsableAttribute>()?.Browsable == false)
                continue;

            string cat = "Miscellaneous";
            var catAttr = prop.GetCustomAttribute<CategoryAttribute>();
            if (catAttr != null && !string.IsNullOrEmpty(catAttr.Category))
                cat = catAttr.Category;

            string dispName = prop.Name;
            var dispAttr = prop.GetCustomAttribute<DisplayNameAttribute>();
            if (dispAttr != null && !string.IsNullOrEmpty(dispAttr.DisplayName))
                dispName = dispAttr.DisplayName;

            if (!string.IsNullOrEmpty(_filterQuery) &&
                !dispName.Contains(_filterQuery, StringComparison.OrdinalIgnoreCase) &&
                !prop.Name.Contains(_filterQuery, StringComparison.OrdinalIgnoreCase) &&
                !cat.Contains(_filterQuery, StringComparison.OrdinalIgnoreCase) &&
                !(prop.GetCustomAttribute<DescriptionAttribute>()?.Description ?? string.Empty)
                    .Contains(_filterQuery, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            string desc = string.Empty;
            var descAttr = prop.GetCustomAttribute<DescriptionAttribute>();
            if (descAttr != null && !string.IsNullOrEmpty(descAttr.Description))
                desc = descAttr.Description;

            bool readOnly = !prop.CanWrite || prop.GetCustomAttribute<ReadOnlyAttribute>()?.IsReadOnly == true ||
                prop.GetSetMethod(nonPublic: false) == null;
            object? val;
            try { val = prop.GetValue(selectedObject); }
            catch (TargetInvocationException) { continue; }

            var editorType = PropertyEditorType.Text;
            Array? enumOpts = null;

            var effectiveType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
            if (effectiveType == typeof(bool))
                editorType = PropertyEditorType.Boolean;
            else if (IsNumericType(effectiveType))
                editorType = PropertyEditorType.Number;
            else if (effectiveType == typeof(HorizontalAlignment))
                editorType = PropertyEditorType.HorizontalAlignment;
            else if (effectiveType == typeof(VerticalAlignment))
                editorType = PropertyEditorType.VerticalAlignment;
            else if (effectiveType.IsEnum)
            {
                editorType = PropertyEditorType.Enum;
                enumOpts = Enum.GetValues(effectiveType);
            }
            else if (typeof(ImageSource).IsAssignableFrom(prop.PropertyType) || prop.Name.Contains("Image", StringComparison.OrdinalIgnoreCase))
            {
                editorType = PropertyEditorType.ImageSource;
            }

            var item = new PropertyItem
            {
                Name = prop.Name,
                DisplayName = dispName,
                Category = cat,
                Description = desc,
                PropertyType = prop.PropertyType,
                EditorType = editorType,
                EnumOptions = enumOpts,
                IsReadOnly = readOnly,
                Value = val
            };

            item.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(PropertyItem.Value) && !item.IsReadOnly &&
                    !_updatingFromObject.Contains(item) && ReferenceEquals(SelectedObject, selectedObject))
                {
                    try
                    {
                        prop.SetValue(selectedObject, ConvertPropertyValue(item.Value, prop.PropertyType));
                        item.ValidationError = null;
                    }
                    catch (Exception ex) when (ex is ArgumentException or InvalidCastException or FormatException or
                        OverflowException or NotSupportedException or TargetInvocationException)
                    {
                        item.ValidationError = ex is TargetInvocationException { InnerException: not null } invocation
                            ? invocation.InnerException?.Message ?? invocation.Message
                            : ex.Message;
                    }
                }
            };

            allItems.Add(item);
        }

        if (_sortByCategory)
        {
            var groups = allItems.GroupBy(x => x.Category).OrderBy(x => x.Key);
            foreach (var grp in groups)
            {
                var catGroup = new PropertyCategoryGroup { CategoryName = grp.Key };
                foreach (var itm in grp.OrderBy(x => x.DisplayName))
                    catGroup.Properties.Add(itm);
                Categories.Add(catGroup);
            }
        }
        else
        {
            var catGroup = new PropertyCategoryGroup { CategoryName = "All Properties (A-Z)" };
            foreach (var itm in allItems.OrderBy(x => x.DisplayName))
                catGroup.Properties.Add(itm);
            Categories.Add(catGroup);
        }
    }

    private static bool IsNumericType(Type type) => Type.GetTypeCode(type) is
        TypeCode.Byte or TypeCode.SByte or TypeCode.UInt16 or TypeCode.UInt32 or TypeCode.UInt64 or
        TypeCode.Int16 or TypeCode.Int32 or TypeCode.Int64 or TypeCode.Decimal or TypeCode.Double or TypeCode.Single;

    private static object? ConvertPropertyValue(object? value, Type propertyType)
    {
        if (value == null || value is string text && string.IsNullOrWhiteSpace(text))
        {
            if (!propertyType.IsValueType || Nullable.GetUnderlyingType(propertyType) != null)
                return null;
            if (value is string) return Activator.CreateInstance(propertyType);
        }

        var targetType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;
        if (value == null) return null;
        if (targetType.IsInstanceOfType(value)) return value;
        if (targetType.IsEnum)
            return value is string enumText ? Enum.Parse(targetType, enumText, ignoreCase: true) : Enum.ToObject(targetType, value);

        var converter = TypeDescriptor.GetConverter(targetType);
        if (value is string sourceText && converter.CanConvertFrom(typeof(string)))
            return converter.ConvertFrom(null, CultureInfo.CurrentCulture, sourceText);
        if (converter.CanConvertFrom(value.GetType()))
            return converter.ConvertFrom(null, CultureInfo.CurrentCulture, value);
        return Convert.ChangeType(value, targetType, CultureInfo.CurrentCulture);
    }
}
