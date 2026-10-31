using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CONCOST.UI.Controls;

/// <summary>
/// A drop area for files: drag files onto it and it reports them, with a visual state that says
/// whether the drag will be accepted.
/// </summary>
/// <remarks>
/// <para>
/// The filter is checked while the drag is still in the air, so the zone only lights up for files it
/// will actually take. Accepting every drag and then refusing the drop is the behaviour that makes a
/// drop area feel broken.
/// </para>
/// <para>
/// Clicking raises <see cref="BrowseRequested"/> instead of opening a dialog: which picker to show, and
/// whether the host even wants one, is not a library decision.
/// </para>
/// </remarks>
public class FileDropZone : Control
{
    private static readonly char[] Separators = { ';', ',' };

    static FileDropZone()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(FileDropZone),
            new FrameworkPropertyMetadata(typeof(FileDropZone)));
    }

    public FileDropZone()
    {
        AllowDrop = true;
        Cursor = Cursors.Hand;

        DragEnter += OnDragEnter;
        DragOver += OnDragOver;
        DragLeave += OnDragLeave;
        Drop += OnDrop;
    }

    /// <summary>Raised when files are dropped or picked. <c>e.Files</c> holds the full paths.</summary>
    public event EventHandler<FileDropEventArgs>? FilesDropped;

    /// <summary>Raised when the user clicks the zone. Open a picker and call <see cref="AddFiles"/>.</summary>
    public event EventHandler? BrowseRequested;

    // ---------------------------------------------------------------- properties

    public static readonly DependencyProperty FilterProperty =
        DependencyProperty.Register(nameof(Filter), typeof(string), typeof(FileDropZone),
            new PropertyMetadata("*"));

    /// <summary>
    /// Semicolon-separated patterns, e.g. <c>*.dwg;*.dxf</c>. <c>*</c> accepts anything.
    /// </summary>
    public string Filter
    {
        get => (string)GetValue(FilterProperty);
        set => SetValue(FilterProperty, value);
    }

    public static readonly DependencyProperty AllowMultipleProperty =
        DependencyProperty.Register(nameof(AllowMultiple), typeof(bool), typeof(FileDropZone),
            new PropertyMetadata(true));

    /// <summary>Accept more than one file per drop. Default <c>true</c>.</summary>
    public bool AllowMultiple
    {
        get => (bool)GetValue(AllowMultipleProperty);
        set => SetValue(AllowMultipleProperty, value);
    }

    public static readonly DependencyProperty CaptionProperty =
        DependencyProperty.Register(nameof(Caption), typeof(string), typeof(FileDropZone),
            new PropertyMetadata("Kéo tệp vào đây"));

    /// <summary>Main line of text.</summary>
    public string Caption
    {
        get => (string)GetValue(CaptionProperty);
        set => SetValue(CaptionProperty, value);
    }

    public static readonly DependencyProperty HintProperty =
        DependencyProperty.Register(nameof(Hint), typeof(string), typeof(FileDropZone),
            new PropertyMetadata("hoặc bấm để chọn tệp"));

    /// <summary>Second line of text.</summary>
    public string Hint
    {
        get => (string)GetValue(HintProperty);
        set => SetValue(HintProperty, value);
    }

    private static readonly DependencyPropertyKey IsDragOverKey =
        DependencyProperty.RegisterReadOnly(nameof(IsDragOver), typeof(bool), typeof(FileDropZone),
            new PropertyMetadata(false));

    public static readonly DependencyProperty IsDragOverProperty = IsDragOverKey.DependencyProperty;

    /// <summary>True while a drag that this zone accepts is over it — the template highlights on it.</summary>
    public bool IsDragOver => (bool)GetValue(IsDragOverProperty);

    private static readonly DependencyPropertyKey IsDragRejectedKey =
        DependencyProperty.RegisterReadOnly(nameof(IsDragRejected), typeof(bool), typeof(FileDropZone),
            new PropertyMetadata(false));

    public static readonly DependencyProperty IsDragRejectedProperty = IsDragRejectedKey.DependencyProperty;

    /// <summary>True while a drag is over the zone but the files do not pass <see cref="Filter"/>.</summary>
    public bool IsDragRejected => (bool)GetValue(IsDragRejectedProperty);

    /// <summary>The files the zone has taken, most recent last.</summary>
    public ObservableCollection<string> Files { get; } = new();

    // ---------------------------------------------------------------- public API

    /// <summary>Adds files that the host picked from its own dialog, applying the same filter.</summary>
    public void AddFiles(IEnumerable<string> paths)
    {
        var accepted = paths.Where(Matches).ToList();
        if (!AllowMultiple && accepted.Count > 1) accepted = accepted.Take(1).ToList();
        if (accepted.Count == 0) return;

        foreach (var path in accepted) Files.Add(path);

        FilesDropped?.Invoke(this, new FileDropEventArgs(accepted));
    }

    /// <summary>Empties <see cref="Files"/>.</summary>
    public void Clear() => Files.Clear();

    /// <summary>True when the path passes <see cref="Filter"/>.</summary>
    public bool Matches(string path)
    {
        var filter = Filter;

        if (string.IsNullOrWhiteSpace(filter) || filter.Trim() == "*") return true;

        var patterns = filter.Split(Separators, StringSplitOptions.RemoveEmptyEntries)
            .Select(p => p.Trim())
            .Where(p => p.Length > 0)
            .ToList();

        if (patterns.Count == 0) return true;

        var name = System.IO.Path.GetFileName(path);

        foreach (var pattern in patterns)
        {
            if (pattern.StartsWith("*.", StringComparison.Ordinal))
            {
                if (name.EndsWith(pattern.Substring(1), StringComparison.OrdinalIgnoreCase)) return true;
            }
            else if (string.Equals(pattern, name, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    // ---------------------------------------------------------------- drag

    private static void OnDragOverOrEnter(DragEventArgs e, bool entering)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop)) { }
        else
        {
            e.Effects = DragDropEffects.None;
            e.Handled = true;
            return;
        }

        var zone = (FileDropZone)e.Source;
        var paths = (string[])e.Data.GetData(DataFormats.FileDrop) ?? Array.Empty<string>();
        bool ok = paths.Length > 0 && (zone.AllowMultiple || paths.Length == 1) && paths.Any(zone.Matches);

        e.Effects = ok ? DragDropEffects.Copy : DragDropEffects.None;
        e.Handled = true;

        zone.SetValue(IsDragOverKey, ok);
        zone.SetValue(IsDragRejectedKey, !ok);

        _ = entering;
    }

    private void OnDragEnter(object sender, DragEventArgs e) => OnDragOverOrEnter(e, true);

    private void OnDragOver(object sender, DragEventArgs e) => OnDragOverOrEnter(e, false);

    private void OnDragLeave(object sender, DragEventArgs e)
    {
        SetValue(IsDragOverKey, false);
        SetValue(IsDragRejectedKey, false);
    }

    private void OnDrop(object sender, DragEventArgs e)
    {
        SetValue(IsDragOverKey, false);
        SetValue(IsDragRejectedKey, false);

        if (e.Data.GetData(DataFormats.FileDrop) is not string[] paths) return;

        e.Handled = true;
        AddFiles(paths);
    }

    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonUp(e);

        if (!IsEnabled) return;

        BrowseRequested?.Invoke(this, EventArgs.Empty);
        e.Handled = true;
    }
}

/// <summary>Files a <see cref="FileDropZone"/> accepted.</summary>
public sealed class FileDropEventArgs : EventArgs
{
    public FileDropEventArgs(IReadOnlyList<string> files) => Files = files;

    /// <summary>The accepted paths, in drop order.</summary>
    public IReadOnlyList<string> Files { get; }
}
