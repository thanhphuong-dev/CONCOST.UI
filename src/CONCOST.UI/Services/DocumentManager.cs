using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace CONCOST.UI.Services;

/// <summary>One open project document: the file, its title, its modified flag.</summary>
public class DocumentModel
{
    public DocumentModel(string title, string? filePath = null)
    {
        Title = title ?? string.Empty;
        FilePath = filePath;
        CreatedUtc = DateTime.UtcNow;
    }

    /// <summary>The name shown on the tab and the title bar, without the modified marker.</summary>
    public string Title { get; set; }

    /// <summary>Where the document came from, or <c>null</c> for one that was never saved.</summary>
    public string? FilePath { get; set; }

    /// <summary>True when there are unsaved changes.</summary>
    public bool IsDirty { get; set; }

    /// <summary>When the document was opened.</summary>
    public DateTime CreatedUtc { get; }

    /// <summary>Anything the host hangs off the document.</summary>
    public object? Tag { get; set; }

    /// <summary>The title with the modified marker a tab shows.</summary>
    public string DisplayTitle => IsDirty ? Title + "*" : Title;

    /// <inheritdoc />
    public override string ToString() => DisplayTitle;
}

/// <summary>Asks whether a document may close.</summary>
public class DocumentClosingEventArgs : EventArgs
{
    public DocumentClosingEventArgs(DocumentModel document) => Document = document;

    /// <summary>The document that is closing.</summary>
    public DocumentModel Document { get; }

    /// <summary>Set to <c>true</c> to keep the document open, e.g. to show a save prompt.</summary>
    public bool Cancel { get; set; }
}

/// <summary>
/// The set of open documents and which one is active.
/// </summary>
/// <remarks>
/// This is the file-level model; <see cref="CONCOST.UI.Controls.DocumentWorkspace"/> is what draws
/// tabs for it. Keeping them apart means a save prompt can be answered without a window, which is what
/// makes <see cref="Close"/> testable.
/// </remarks>
public class DocumentManager
{
    private static readonly Lazy<DocumentManager> Shared = new(() => new DocumentManager());

    private DocumentModel? _active;

    /// <summary>The shared instance the application uses.</summary>
    public static DocumentManager Current => Shared.Value;

    /// <summary>The open documents, in open order.</summary>
    public ObservableCollection<DocumentModel> Documents { get; } = new();

    /// <summary>The document commands act on.</summary>
    public DocumentModel? ActiveDocument
    {
        get => _active;
        set
        {
            if (ReferenceEquals(_active, value)) return;

            _active = value;

            ActiveDocumentChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>Raised before a document closes; cancel it to veto.</summary>
    public event EventHandler<DocumentClosingEventArgs>? DocumentClosing;

    /// <summary>Raised after a document was opened.</summary>
    public event EventHandler<DocumentModel>? DocumentOpened;

    /// <summary>Raised after a document closed.</summary>
    public event EventHandler<DocumentModel>? DocumentClosed;

    /// <summary>Raised when the active document changes.</summary>
    public event EventHandler? ActiveDocumentChanged;

    /// <summary>Raised when a document's modified flag changes.</summary>
    public event EventHandler<DocumentModel>? DirtyChanged;

    /// <summary>Adds a document and makes it active.</summary>
    public DocumentModel Open(string title, string? filePath = null)
    {
        var existing = filePath is null
            ? null
            : Documents.FirstOrDefault(document => string.Equals(document.FilePath, filePath, StringComparison.OrdinalIgnoreCase));

        if (existing is not null)
        {
            ActiveDocument = existing;

            return existing;
        }

        var document = new DocumentModel(title, filePath);

        Documents.Add(document);
        _active = document;

        DocumentOpened?.Invoke(this, document);
        ActiveDocumentChanged?.Invoke(this, EventArgs.Empty);

        return document;
    }

    /// <summary>Closes a document unless a handler cancels it.</summary>
    public bool Close(DocumentModel document)
    {
        if (document is null || !Documents.Contains(document)) return false;

        var args = new DocumentClosingEventArgs(document);
        DocumentClosing?.Invoke(this, args);

        if (args.Cancel) return false;

        Documents.Remove(document);

        if (ReferenceEquals(_active, document)) _active = Documents.LastOrDefault();

        DocumentClosed?.Invoke(this, document);
        ActiveDocumentChanged?.Invoke(this, EventArgs.Empty);

        return true;
    }

    /// <summary>Closes every document, honouring the same veto on each.</summary>
    public bool CloseAll()
    {
        bool closed = false;

        foreach (var document in Documents.ToArray()) closed |= Close(document);

        return closed;
    }

    /// <summary>Sets the modified flag and raises <see cref="DirtyChanged"/> when it changed.</summary>
    public void SetDirty(DocumentModel document, bool isDirty)
    {
        if (document is null || document.IsDirty == isDirty) return;

        document.IsDirty = isDirty;

        DirtyChanged?.Invoke(this, document);
    }

    /// <summary>The text a window title reads as, e.g. <c>Level 1* — Project1</c>.</summary>
    public string BuildTitle(string applicationName)
    {
        string document = _active?.DisplayTitle ?? "No document";

        return string.IsNullOrEmpty(applicationName) ? document : $"{document} \u2014 {applicationName}";
    }
}
