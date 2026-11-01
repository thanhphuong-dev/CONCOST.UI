using System;
using System.Collections.Generic;
using System.Linq;

namespace CONCOST.UI.Services;

/// <summary>How a selection changed.</summary>
public enum SelectionChangeKind
{
    Replaced,
    Added,
    Removed,
    Cleared
}

/// <summary>Carries the selection after a change.</summary>
public class SelectionChangeEventArgs : EventArgs
{
    public SelectionChangeEventArgs(SelectionChangeKind kind, IReadOnlyList<object> added,
        IReadOnlyList<object> removed, object? primary)
    {
        Kind = kind;
        Added = added;
        Removed = removed;
        Primary = primary;
    }

    /// <summary>What kind of change this was.</summary>
    public SelectionChangeKind Kind { get; }

    /// <summary>Objects that entered the selection.</summary>
    public IReadOnlyList<object> Added { get; }

    /// <summary>Objects that left it.</summary>
    public IReadOnlyList<object> Removed { get; }

    /// <summary>The object the properties palette shows, or <c>null</c> when nothing is selected.</summary>
    public object? Primary { get; }

    /// <summary><c>true</c> when the selection is now empty.</summary>
    public bool IsEmpty => Primary is null;
}

/// <summary>
/// What is selected, shared by the viewport, the properties palette, the status bar counter and the
/// contextual ribbon.
/// </summary>
/// <remarks>
/// Selection is compared by reference, not by <c>Equals</c>: two walls with the same parameters are
/// still two walls, and a model object that overrides equality for value semantics must not collapse
/// the selection because of it. The service keeps a list rather than a set for that reason, and
/// because the last item selected is the one the properties palette shows.
/// </remarks>
public class SelectionManager
{
    private static readonly Lazy<SelectionManager> Shared = new(() => new SelectionManager());

    private readonly List<object> _items = new();

    /// <summary>The instance a host wires its panels to, unless it creates its own for a test.</summary>
    public static SelectionManager Current => Shared.Value;

    /// <summary>Raised after any change, with what was added, what was removed and the new primary.</summary>
    public event EventHandler<SelectionChangeEventArgs>? SelectionChanged;

    /// <summary>What is selected, in selection order.</summary>
    public IReadOnlyList<object> Items => _items;

    /// <summary>How many objects are selected — the number the status bar shows.</summary>
    public int Count => _items.Count;

    /// <summary>The most recently selected object, or <c>null</c>.</summary>
    public object? Primary => _items.Count == 0 ? null : _items[_items.Count - 1];

    /// <summary>Whether the selection is empty.</summary>
    public bool IsEmpty => _items.Count == 0;

    /// <summary>Replaces the selection with a single object, or clears it when <c>null</c>.</summary>
    public void Select(object? item)
    {
        if (item is null)
        {
            Clear();
            return;
        }

        if (_items.Count == 1 && ReferenceEquals(_items[0], item)) return;

        var removed = _items.ToArray();

        _items.Clear();
        _items.Add(item);

        Raise(SelectionChangeKind.Replaced, new[] { item }, removed);
    }

    /// <summary>Replaces the selection with a set of objects.</summary>
    public void SelectMany(IEnumerable<object> items)
    {
        var next = Distinct(items);

        if (next.Count == 0)
        {
            Clear();
            return;
        }

        var removed = _items.Where(existing => !next.Any(candidate => ReferenceEquals(candidate, existing))).ToArray();
        var added = next.Where(candidate => !_items.Any(existing => ReferenceEquals(candidate, existing))).ToArray();

        if (removed.Length == 0 && added.Length == 0) return;

        _items.Clear();
        _items.AddRange(next);

        Raise(SelectionChangeKind.Replaced, added, removed);
    }

    /// <summary>Adds an object unless it is already selected.</summary>
    public void Add(object item)
    {
        if (item is null) return;
        if (_items.Any(existing => ReferenceEquals(existing, item))) return;

        _items.Add(item);
        Raise(SelectionChangeKind.Added, new[] { item }, Array.Empty<object>());
    }

    /// <summary>Removes an object from the selection.</summary>
    public void Remove(object item)
    {
        int index = IndexOf(item);
        if (index < 0) return;

        _items.RemoveAt(index);
        Raise(SelectionChangeKind.Removed, Array.Empty<object>(), new[] { item });
    }

    /// <summary>Adds the object when it is not selected and removes it when it is — Ctrl+click.</summary>
    public void Toggle(object item)
    {
        if (Contains(item)) Remove(item);
        else Add(item);
    }

    /// <summary>Empties the selection.</summary>
    public void Clear()
    {
        if (_items.Count == 0) return;

        var removed = _items.ToArray();

        _items.Clear();
        Raise(SelectionChangeKind.Cleared, Array.Empty<object>(), removed);
    }

    /// <summary>Whether the object is selected, by reference.</summary>
    public bool Contains(object item) => IndexOf(item) >= 0;

    private int IndexOf(object item)
    {
        for (int i = 0; i < _items.Count; i++)
        {
            if (ReferenceEquals(_items[i], item)) return i;
        }

        return -1;
    }

    private static List<object> Distinct(IEnumerable<object> items)
    {
        var result = new List<object>();

        foreach (var item in items)
        {
            if (item is null) continue;
            if (result.Any(existing => ReferenceEquals(existing, item))) continue;

            result.Add(item);
        }

        return result;
    }

    private void Raise(SelectionChangeKind kind, IReadOnlyList<object> added, IReadOnlyList<object> removed) =>
        SelectionChanged?.Invoke(this, new SelectionChangeEventArgs(kind, added, removed, Primary));
}
