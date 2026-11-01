using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using CONCOST.UI.Icons;

namespace CONCOST.UI.Controls;

/// <summary>One open drawing, schedule or sheet in a <see cref="DocumentWorkspace"/>.</summary>
public class DocumentPane
{
    public DocumentPane(string title, object? content = null, IconKind icon = IconKind.FileText)
    {
        Title = title ?? string.Empty;
        Content = content;
        Icon = icon;
    }

    /// <summary>Caption on the tab.</summary>
    public string Title { get; set; }

    /// <summary>The view itself — a <see cref="ViewportControl"/>, a schedule, anything.</summary>
    public object? Content { get; set; }

    /// <summary>Tab icon.</summary>
    public IconKind Icon { get; set; }

    /// <summary>Shows the unsaved dot.</summary>
    public bool IsDirty { get; set; }

    /// <summary>The model view behind the pane, so a host can map tab back to data.</summary>
    public object? Tag { get; set; }

    /// <summary>The window the pane is floating in, or <c>null</c> when it is docked.</summary>
    public Window? FloatingWindow { get; internal set; }
}

/// <summary>Asks whether a document may close.</summary>
public class DocumentClosingEventArgs : EventArgs
{
    public DocumentClosingEventArgs(DocumentPane pane) => Pane = pane;

    /// <summary>The document that is closing.</summary>
    public DocumentPane Pane { get; }

    /// <summary>Set to <c>true</c> to keep the document open, e.g. to show a save prompt.</summary>
    public bool Cancel { get; set; }
}

/// <summary>How the open documents share the workspace.</summary>
/// <remarks>
/// Named with the <c>Document</c> prefix because <c>System.Windows.Media.TileMode</c> already owns the
/// short name, and a picture brush is not what this means nine times out of ten.
/// </remarks>
public enum DocumentTileMode
{
    /// <summary>One at a time, chosen by the tab strip.</summary>
    Single,

    /// <summary>Side by side, with a splitter between them.</summary>
    Vertical,

    /// <summary>Stacked, with a splitter between them.</summary>
    Horizontal,

    /// <summary>A uniform grid, for more than two views.</summary>
    Grid
}

/// <summary>
/// The document area: a tab strip, a layout host that can show one view or tile several, a placeholder
/// for the empty state, and floating windows.
/// </summary>
/// <remarks>
/// <para>
/// The tab strip and the tile area are separate on purpose. A <see cref="TabControl"/> owns its content
/// presenter, so a view that must also appear in a tiled layout cannot be its tab's
/// <see cref="ContentControl.Content"/>; the workspace keeps the content and only borrows the strip for
/// selection. That also makes floating a view out a matter of re-parenting one element.
/// </para>
/// <para>
/// Closing is a request, not an action: <see cref="DocumentClosing"/> can cancel it, which is where a
/// "save changes?" prompt goes.
/// </para>
/// </remarks>
public class DocumentWorkspace : Grid
{
    private readonly DocumentTabControl _tabs;
    private readonly Grid _body;
    private readonly Border _placeholder;

    private DocumentTileMode _tileMode = DocumentTileMode.Single;
    private DocumentPane? _active;
    private bool _syncing;

    public DocumentWorkspace()
    {
        RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

        _tabs = new DocumentTabControl { Visibility = Visibility.Collapsed };
        _tabs.SelectionChanged += OnTabSelectionChanged;

        SetRow(_tabs, 0);
        Children.Add(_tabs);

        _body = new Grid { Margin = new Thickness(0) };

        SetRow(_body, 1);
        Children.Add(_body);

        _placeholder = CreatePlaceholder();
        _body.Children.Add(_placeholder);
    }

    /// <summary>The open documents, in tab order.</summary>
    public ObservableCollection<DocumentPane> Documents { get; } = new();

    /// <summary>The document the workspace is showing; <c>null</c> when there are none.</summary>
    public DocumentPane? ActiveDocument
    {
        get => _active;
        set
        {
            if (ReferenceEquals(value, _active)) return;

            _active = value;
            SyncTabs();
            RebuildLayout();

            ActiveDocumentChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>How the documents share the area.</summary>
    public DocumentTileMode TileMode
    {
        get => _tileMode;
        set
        {
            if (_tileMode == value) return;

            _tileMode = value;
            RebuildLayout();
        }
    }

    /// <summary>Caption shown when nothing is open.</summary>
    public string PlaceholderTitle { get; set; } = "No views open";

    /// <summary>Second line of the placeholder.</summary>
    public string PlaceholderText { get; set; } = "Open a view from the project browser.";

    /// <summary>How many panes the layout is currently showing; a floating document is not one.</summary>
    public int PaneCount => _body.Children.Count;

    /// <summary>Raised before a document closes; set <see cref="DocumentClosingEventArgs.Cancel"/> to keep it.</summary>
    public event EventHandler<DocumentClosingEventArgs>? DocumentClosing;

    /// <summary>Raised after a document was added.</summary>
    public event EventHandler<DocumentPane>? DocumentAdded;

    /// <summary>Raised after a document was closed.</summary>
    public event EventHandler<DocumentPane>? DocumentClosed;

    /// <summary>Raised when the active document changes.</summary>
    public event EventHandler? ActiveDocumentChanged;

    /// <summary>Raised after a document was floated into its own window.</summary>
    public event EventHandler<DocumentPane>? DocumentFloated;

    /// <summary>Adds a document and activates it.</summary>
    public DocumentPane AddDocument(string title, object? content = null, IconKind icon = IconKind.FileText)
    {
        var pane = new DocumentPane(title, content, icon);

        Documents.Add(pane);

        // Through the property, not the field: opening a view is the most common way for the active
        // document to change, and a host that never hears about it keeps the previous view's title
        // and status bar.
        ActiveDocument = pane;

        DocumentAdded?.Invoke(this, pane);

        return pane;
    }

    /// <summary>Adds an existing pane, e.g. restored from a layout profile.</summary>
    public void AddDocument(DocumentPane pane)
    {
        if (pane is null) return;

        Documents.Add(pane);

        ActiveDocument = pane;

        DocumentAdded?.Invoke(this, pane);
    }

    /// <summary>Closes a document unless a handler cancels it.</summary>
    public bool CloseDocument(DocumentPane pane)
    {
        if (pane is null || !Documents.Contains(pane)) return false;

        var args = new DocumentClosingEventArgs(pane);
        DocumentClosing?.Invoke(this, args);

        if (args.Cancel) return false;

        pane.FloatingWindow?.Close();

        Documents.Remove(pane);

        if (ReferenceEquals(_active, pane)) _active = Documents.LastOrDefault();

        SyncTabs();
        RebuildLayout();

        DocumentClosed?.Invoke(this, pane);

        return true;
    }

    /// <summary>Closes everything except one document.</summary>
    public void CloseOthers(DocumentPane keep)
    {
        foreach (var pane in Documents.Where(pane => !ReferenceEquals(pane, keep)).ToArray())
        {
            CloseDocument(pane);
        }
    }

    /// <summary>Closes every document, honouring the same veto.</summary>
    public void CloseAll()
    {
        foreach (var pane in Documents.ToArray()) CloseDocument(pane);
    }

    /// <summary>Moves a document into its own window, the way a torn-off drawing tab behaves.</summary>
    public Window? FloatDocument(DocumentPane pane)
    {
        if (pane is null || pane.Content is not UIElement content) return null;
        if (pane.FloatingWindow is not null)
        {
            pane.FloatingWindow.Activate();
            return pane.FloatingWindow;
        }

        RebuildLayout();

        var window = new Window
        {
            Title = pane.Title,
            Content = content,
            Width = 720,
            Height = 520,
            WindowStartupLocation = WindowStartupLocation.CenterScreen,
            Owner = Window.GetWindow(this)
        };

        pane.FloatingWindow = window;

        window.Closed += (_, _) =>
        {
            pane.FloatingWindow = null;

            // The content is parentless while it floats; taking it out of the window lets the tiled
            // layout adopt it again.
            window.Content = null;

            RebuildLayout();
        };

        window.Show();

        RebuildLayout();
        DocumentFloated?.Invoke(this, pane);

        return window;
    }

    private void SyncTabs()
    {
        _syncing = true;

        try
        {
            _tabs.Items.Clear();

            foreach (var pane in Documents)
            {
                var item = new DocumentTabItem
                {
                    Header = pane.Title,
                    Icon = pane.Icon,
                    IsDirty = pane.IsDirty,
                    Tag = pane
                };

                item.CloseRequested += (sender, _) =>
                {
                    if ((sender as DocumentTabItem)?.Tag is DocumentPane closing) CloseDocument(closing);
                };

                _tabs.Items.Add(item);
            }

            _tabs.Visibility = Documents.Count == 0 ? Visibility.Collapsed : Visibility.Visible;

            var active = Documents.FirstOrDefault(pane => ReferenceEquals(pane, _active));
            if (active is not null) _tabs.SelectedItem = _tabs.Items.OfType<DocumentTabItem>().FirstOrDefault(item => ReferenceEquals(item.Tag, active));
        }
        finally
        {
            _syncing = false;
        }
    }

    private void RebuildLayout()
    {
        _body.Children.Clear();
        _body.ColumnDefinitions.Clear();
        _body.RowDefinitions.Clear();

        var visible = Documents.Where(pane => pane.FloatingWindow is null).ToList();

        if (visible.Count == 0)
        {
            _body.Children.Add(_placeholder);

            if (_active is null || Documents.Count == 0) return;

            // Everything is floating: the workspace is empty but the placeholder text should not claim
            // that nothing is open.
            return;
        }

        var layout = TileMode == DocumentTileMode.Single
            ? (ReferenceEquals(_active, null) ? new List<DocumentPane> { visible[0] } : new List<DocumentPane> { visible.Contains(_active) ? _active : visible[0] })
            : visible;

        switch (TileMode)
        {
            case DocumentTileMode.Vertical:
                BuildVertical(layout);
                break;

            case DocumentTileMode.Horizontal:
                BuildHorizontal(layout);
                break;

            case DocumentTileMode.Grid:
                BuildGrid(layout);
                break;

            default:
                foreach (var pane in layout) _body.Children.Add(PaneContent(pane));
                break;
        }
    }

    private void BuildVertical(List<DocumentPane> panes)
    {
        for (int i = 0; i < panes.Count; i++)
        {
            if (i > 0)
            {
                _body.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                var splitter = new GridSplitter { Width = 4, HorizontalAlignment = HorizontalAlignment.Stretch, Background = Brushes.Transparent };

                System.Windows.Controls.Grid.SetColumn(splitter, _body.ColumnDefinitions.Count - 1);
                _body.Children.Add(splitter);
            }

            _body.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var content = PaneContent(panes[i]);

            System.Windows.Controls.Grid.SetColumn(content, _body.ColumnDefinitions.Count - 1);
            _body.Children.Add(content);
        }
    }

    private void BuildHorizontal(List<DocumentPane> panes)
    {
        for (int i = 0; i < panes.Count; i++)
        {
            if (i > 0)
            {
                _body.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                var splitter = new GridSplitter { Height = 4, HorizontalAlignment = HorizontalAlignment.Stretch, Background = Brushes.Transparent };

                System.Windows.Controls.Grid.SetRow(splitter, _body.RowDefinitions.Count - 1);
                _body.Children.Add(splitter);
            }

            _body.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            var content = PaneContent(panes[i]);

            System.Windows.Controls.Grid.SetRow(content, _body.RowDefinitions.Count - 1);
            _body.Children.Add(content);
        }
    }

    private void BuildGrid(List<DocumentPane> panes)
    {
        var uniform = new UniformGrid { Columns = 0, Rows = 0 };

        foreach (var pane in panes) uniform.Children.Add(PaneContent(pane));

        _body.Children.Add(uniform);
    }

    /// <summary>
    /// The pane's content element, detached from whatever hosted it before. A second parent throws, and
    /// a tiled layout re-parents the same element on every change.
    /// </summary>
    private UIElement PaneContent(DocumentPane pane)
    {
        if (pane.Content is UIElement element)
        {
            Detach(element);
            return element;
        }

        var text = new TextBlock
        {
            Text = $"{pane.Title} has no content.",
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            FontSize = 12
        };

        text.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");

        return text;
    }

    private static void Detach(UIElement element)
    {
        switch (element)
        {
            case ContentControl content when ReferenceEquals(content.Content, element):
                break;

            case System.Windows.Media.Visual:
            {
                var parent = VisualTreeHelper.GetParent(element);

                if (parent is Panel panel) panel.Children.Remove(element);
                else if (parent is ContentControl control) control.Content = null;
                else if (parent is Decorator decorator) decorator.Child = null;
                else if (parent is ContentPresenter presenter) presenter.Content = null;

                break;
            }
        }
    }

    private Border CreatePlaceholder()
    {
        var icon = new Icon { Kind = IconKind.Viewport3D, Width = 34, Height = 34, HorizontalAlignment = HorizontalAlignment.Center };
        icon.SetResourceReference(Control.ForegroundProperty, "CONCOST.Brush.TextDisabled");

        var title = new TextBlock
        {
            Text = PlaceholderTitle,
            FontSize = 14,
            FontWeight = FontWeights.SemiBold,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 10, 0, 2)
        };

        title.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");

        var text = new TextBlock
        {
            Text = PlaceholderText,
            FontSize = 12,
            HorizontalAlignment = HorizontalAlignment.Center
        };

        text.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextTertiary");

        var panel = new StackPanel { HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
        panel.Children.Add(icon);
        panel.Children.Add(title);
        panel.Children.Add(text);

        return new Border { Background = Brushes.Transparent, Child = panel };
    }

    private void OnTabSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_syncing) return;

        if ((_tabs.SelectedItem as DocumentTabItem)?.Tag is not DocumentPane pane) return;

        ActiveDocument = pane;
    }
}
