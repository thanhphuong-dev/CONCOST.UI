using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace CONCOST.UI.Services;

/// <summary>
/// Implemented by a model object whose type can be swapped for another one — the wall that becomes a
/// curtain wall, the family instance that changes family type.
/// </summary>
/// <remarks>
/// The palette only raises the request: what "change the type" means — replace the instance, keep its
/// location and parameters, cascade to the host's own caches — belongs to the document model.
/// </remarks>
public interface ITypeSelectable
{
    /// <summary>Type the object currently is.</summary>
    string TypeName { get; }

    /// <summary>Types the object could become.</summary>
    IReadOnlyList<string> AvailableTypes { get; }
}

/// <summary>The editor the properties palette shows for a value.</summary>
public enum PropertyEditorKind
{
    Text,
    Numeric,
    Boolean,
    Choice,
    Color,
    Reference
}

/// <summary>
/// One editable property of a model object: how to read it, how to write it, and how the palette
/// should present it.
/// </summary>
public class PropertyDescriptor
{
    private readonly Func<object, object?> _getter;
    private readonly Action<object, object?>? _setter;

    public PropertyDescriptor(string name, Func<object, object?> getter, Action<object, object?>? setter = null)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        _getter = getter ?? throw new ArgumentNullException(nameof(getter));
        _setter = setter;

        DisplayName = name;
    }

    /// <summary>Property key, unique within a category.</summary>
    public string Name { get; }

    /// <summary>Caption in the palette; defaults to <see cref="Name"/>.</summary>
    public string DisplayName { get; set; }

    /// <summary>Group the row is sorted into.</summary>
    public string Category { get; set; } = "General";

    /// <summary>Tooltip text.</summary>
    public string? Description { get; set; }

    /// <summary>Which editor to use.</summary>
    public PropertyEditorKind Editor { get; set; } = PropertyEditorKind.Text;

    /// <summary>Value the reset button goes back to.</summary>
    public object? DefaultValue { get; set; }

    /// <summary>Options for <see cref="PropertyEditorKind.Choice"/>.</summary>
    public IReadOnlyList<object>? Choices { get; set; }

    /// <summary>Unit suffix such as <c>mm</c>, shown after the value.</summary>
    public string? Unit { get; set; }

    /// <summary>Format string for numeric values.</summary>
    public string? Format { get; set; }

    /// <summary>Returns an error message for an invalid value, or <c>null</c> when it is acceptable.</summary>
    public Func<object, object?, string?>? Validator { get; set; }

    /// <summary><c>true</c> when the property has no setter.</summary>
    public bool IsReadOnly => _setter is null;

    /// <summary>Reads the value from <paramref name="target"/>.</summary>
    public object? GetValue(object target) => _getter(target);

    /// <summary>Writes the value; does nothing when the property is read-only.</summary>
    public void SetValue(object target, object? value)
    {
        if (_setter is null) return;

        _setter(target, value);
    }
}

/// <summary>Carries one committed property edit.</summary>
public class PropertyValueChangedEventArgs : EventArgs
{
    public PropertyValueChangedEventArgs(object target, PropertyDescriptor property, object? oldValue, object? newValue)
    {
        Target = target;
        Property = property;
        OldValue = oldValue;
        NewValue = newValue;
    }

    /// <summary>The edited object.</summary>
    public object Target { get; }

    /// <summary>The descriptor that was written.</summary>
    public PropertyDescriptor Property { get; }

    /// <summary>Value before the edit.</summary>
    public object? OldValue { get; }

    /// <summary>Value after the edit.</summary>
    public object? NewValue { get; }
}

/// <summary>
/// The property system behind <see cref="CONCOST.UI.Controls.PropertiesPalette"/>: which properties an object has,
/// how to read and write them, and validation.
/// </summary>
/// <remarks>
/// <para>
/// Descriptors are registered per type rather than discovered by reflection. Reflection would need a
/// naming convention, would surface every CLR property of a WPF object the moment one was selected, and
/// could not express a unit, a choice list or a validator — all of which the palette needs.
/// </para>
/// <para>
/// Every successful write goes through a transaction, so a parameter edit is one undo step and the
/// viewport refresh happens on <see cref="TransactionManager.Changed"/> rather than per keystroke.
/// </para>
/// </remarks>
public class PropertyManager
{
    private static readonly Lazy<PropertyManager> Shared = new(() => new PropertyManager());

    private readonly Dictionary<Type, List<PropertyDescriptor>> _descriptors = new();
    private readonly TransactionManager _transactions;

    /// <summary>Creates a manager over its own transaction manager, or the shared one.</summary>
    public PropertyManager(TransactionManager? transactions = null) =>
        _transactions = transactions ?? TransactionManager.Current;

    /// <summary>The instance a host shares between its palette and its commands.</summary>
    public static PropertyManager Current => Shared.Value;

    /// <summary>Raised after a value was written.</summary>
    public event EventHandler<PropertyValueChangedEventArgs>? ValueChanged;

    /// <summary>Registers the properties of <typeparamref name="T"/>.</summary>
    public void Register<T>(params PropertyDescriptor[] descriptors) => Register(typeof(T), descriptors);

    /// <summary>Registers the properties of a type, replacing whatever was registered before.</summary>
    public void Register(Type type, params PropertyDescriptor[] descriptors)
    {
        if (type is null) return;

        _descriptors[type] = descriptors?.ToList() ?? new List<PropertyDescriptor>();
    }

    /// <summary>The descriptors that apply to <paramref name="target"/>, base types included.</summary>
    public IReadOnlyList<PropertyDescriptor> GetDescriptors(object target)
    {
        if (target is null) return Array.Empty<PropertyDescriptor>();

        var result = new List<PropertyDescriptor>();
        var seen = new HashSet<string>(StringComparer.Ordinal);

        // Derived type first so an override of a base property wins.
        for (var type = target.GetType(); type is not null && type != typeof(object); type = type.BaseType)
        {
            if (!_descriptors.TryGetValue(type, out var descriptors)) continue;

            foreach (var descriptor in descriptors)
            {
                if (seen.Add(descriptor.Name)) result.Add(descriptor);
            }
        }

        return result;
    }

    /// <summary>Finds a descriptor by name.</summary>
    public PropertyDescriptor? Find(object target, string name) =>
        GetDescriptors(target).FirstOrDefault(descriptor => string.Equals(descriptor.Name, name, StringComparison.Ordinal));

    /// <summary>Reads a property value by name.</summary>
    public object? GetValue(object target, string name) => Find(target, name)?.GetValue(target);

    /// <summary>Validates a candidate value; returns an error message or <c>null</c>.</summary>
    public string? Validate(object target, PropertyDescriptor property, object? value)
    {
        if (property is null) return null;

        try
        {
            return property.Validator?.Invoke(target, value);
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }

    /// <summary>
    /// Validates and writes a value inside a transaction, then raises <see cref="ValueChanged"/>.
    /// Returns <c>false</c> and an error message when the value is rejected.
    /// </summary>
    public bool TrySetValue(object target, string name, object? value, out string? error)
    {
        error = null;

        var property = Find(target, name);
        if (property is null)
        {
            error = $"'{name}' is not a property of {target?.GetType().Name ?? "null"}.";
            return false;
        }

        if (property.IsReadOnly)
        {
            error = $"'{property.DisplayName}' is read-only.";
            return false;
        }

        error = Validate(target, property, value);
        if (error is not null) return false;

        object? previous = property.GetValue(target);

        void Apply() => property.SetValue(target, value);
        void Revert() => property.SetValue(target, previous);

        // Join an open transaction when there is one, so a command that edits five properties is one
        // undo step; otherwise the edit is its own step.
        var active = _transactions.Active;

        if (active is not null)
        {
            active.Record(property.DisplayName, Apply, Revert);
        }
        else
        {
            using var transaction = _transactions.Begin($"Change {property.DisplayName}");
            transaction.Record(property.DisplayName, Apply, Revert);
        }

        ValueChanged?.Invoke(this, new PropertyValueChangedEventArgs(target, property, previous, value));

        return true;
    }

    /// <summary>Writes a value with a typed conversion, e.g. the text a cell editor produced.</summary>
    public bool TrySetValueFromText(object target, string name, string? text, out string? error)
    {
        error = null;

        var property = Find(target, name);
        if (property is null)
        {
            error = $"'{name}' is not a property of {target?.GetType().Name ?? "null"}.";
            return false;
        }

        if (!TryConvert(property, text, out var value, out error)) return false;

        return TrySetValue(target, name, value, out error);
    }

    /// <summary>Turns edited text into the property's own type.</summary>
    public static bool TryConvert(PropertyDescriptor property, string? text, out object? value, out string? error)
    {
        error = null;
        value = text;

        switch (property.Editor)
        {
            case PropertyEditorKind.Numeric:
            {
                if (string.IsNullOrWhiteSpace(text))
                {
                    error = "A number is required.";
                    return false;
                }

                if (!double.TryParse(text, NumberStyles.Float, CultureInfo.CurrentCulture, out double number) &&
                    !double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out number))
                {
                    error = $"'{text}' is not a number.";
                    return false;
                }

                value = number;
                return true;
            }

            case PropertyEditorKind.Boolean:
                value = string.Equals(text, "True", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(text, "1", StringComparison.Ordinal);
                return true;

            case PropertyEditorKind.Choice:
            {
                var match = property.Choices?.FirstOrDefault(candidate =>
                    string.Equals(candidate?.ToString(), text, StringComparison.OrdinalIgnoreCase));

                if (match is null && !string.IsNullOrWhiteSpace(text))
                {
                    error = $"'{text}' is not one of the allowed values.";
                    return false;
                }

                value = match;
                return true;
            }

            default:
                return true;
        }
    }
}
