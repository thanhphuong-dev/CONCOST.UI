using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;

namespace CONCOST.UI.Controls;

public enum AggregateType
{
    None,
    Sum,
    Average,
    Count,
    Min,
    Max,
    Custom
}

// ─────────────────────────────────────────────────────────────────────────────
// Tree-based Column Header model (Extensible, Interactive, Full Feature Support)
// ─────────────────────────────────────────────────────────────────────────────

public abstract class ColumnNode : DependencyObject
{
    internal event EventHandler? NodeChanged;

    protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        NodeChanged?.Invoke(this, EventArgs.Empty);
    }

    public static readonly DependencyProperty HeaderTextProperty =
        DependencyProperty.Register(nameof(HeaderText), typeof(string), typeof(ColumnNode), new PropertyMetadata(string.Empty));
    public string HeaderText { get => (string)GetValue(HeaderTextProperty); set => SetValue(HeaderTextProperty, value); }

    public static readonly DependencyProperty HeaderTemplateProperty =
        DependencyProperty.Register(nameof(HeaderTemplate), typeof(DataTemplate), typeof(ColumnNode), new PropertyMetadata(null));
    public DataTemplate? HeaderTemplate { get => (DataTemplate?)GetValue(HeaderTemplateProperty); set => SetValue(HeaderTemplateProperty, value); }

    public static readonly DependencyProperty ForegroundProperty =
        DependencyProperty.Register(nameof(Foreground), typeof(Brush), typeof(ColumnNode), new PropertyMetadata(null));
    public Brush? Foreground { get => (Brush?)GetValue(ForegroundProperty); set => SetValue(ForegroundProperty, value); }

    public static readonly DependencyProperty BackgroundProperty =
        DependencyProperty.Register(nameof(Background), typeof(Brush), typeof(ColumnNode), new PropertyMetadata(null));
    public Brush? Background { get => (Brush?)GetValue(BackgroundProperty); set => SetValue(BackgroundProperty, value); }

    public static readonly DependencyProperty ToolTipProperty =
        DependencyProperty.Register(nameof(ToolTip), typeof(object), typeof(ColumnNode), new PropertyMetadata(null));
    public object? ToolTip { get => GetValue(ToolTipProperty); set => SetValue(ToolTipProperty, value); }

    public static readonly DependencyProperty HorizontalAlignmentProperty =
        DependencyProperty.Register(nameof(HorizontalAlignment), typeof(HorizontalAlignment), typeof(ColumnNode), new PropertyMetadata(HorizontalAlignment.Center));
    public HorizontalAlignment HorizontalAlignment { get => (HorizontalAlignment)GetValue(HorizontalAlignmentProperty); set => SetValue(HorizontalAlignmentProperty, value); }

    public abstract IEnumerable<ColumnLeaf> GetLeaves();
    public abstract int GetDepth();
}

[ContentProperty("Children")]
public class ColumnGroup : ColumnNode
{
    public ObservableCollection<ColumnNode> Children { get; } = new();
    public override IEnumerable<ColumnLeaf> GetLeaves() => Children.SelectMany(c => c.GetLeaves());
    public override int GetDepth() => Children.Count == 0 ? 1 : 1 + Children.Max(c => c.GetDepth());
}

public class ColumnLeaf : ColumnNode
{
    public static readonly DependencyProperty BindingPathProperty =
        DependencyProperty.Register(nameof(BindingPath), typeof(string), typeof(ColumnLeaf), new PropertyMetadata(string.Empty));
    public string BindingPath { get => (string)GetValue(BindingPathProperty); set => SetValue(BindingPathProperty, value); }

    public static readonly DependencyProperty SortMemberPathProperty =
        DependencyProperty.Register(nameof(SortMemberPath), typeof(string), typeof(ColumnLeaf), new PropertyMetadata(string.Empty));
    public string SortMemberPath { get => (string)GetValue(SortMemberPathProperty); set => SetValue(SortMemberPathProperty, value); }

    public static readonly DependencyProperty CanUserSortProperty =
        DependencyProperty.Register(nameof(CanUserSort), typeof(bool), typeof(ColumnLeaf), new PropertyMetadata(true));
    public bool CanUserSort { get => (bool)GetValue(CanUserSortProperty); set => SetValue(CanUserSortProperty, value); }

    public static readonly DependencyProperty CanUserResizeProperty =
        DependencyProperty.Register(nameof(CanUserResize), typeof(bool), typeof(ColumnLeaf), new PropertyMetadata(true));
    public bool CanUserResize { get => (bool)GetValue(CanUserResizeProperty); set => SetValue(CanUserResizeProperty, value); }

    public static readonly DependencyProperty WidthProperty =
        DependencyProperty.Register(nameof(Width), typeof(DataGridLength), typeof(ColumnLeaf),
            new PropertyMetadata(new DataGridLength(1, DataGridLengthUnitType.Star)));
    public DataGridLength Width { get => (DataGridLength)GetValue(WidthProperty); set => SetValue(WidthProperty, value); }

    public static readonly DependencyProperty MinWidthProperty =
        DependencyProperty.Register(nameof(MinWidth), typeof(double), typeof(ColumnLeaf), new PropertyMetadata(40.0));
    public double MinWidth { get => (double)GetValue(MinWidthProperty); set => SetValue(MinWidthProperty, value); }

    public static readonly DependencyProperty MaxWidthProperty =
        DependencyProperty.Register(nameof(MaxWidth), typeof(double), typeof(ColumnLeaf), new PropertyMetadata(double.PositiveInfinity));
    public double MaxWidth { get => (double)GetValue(MaxWidthProperty); set => SetValue(MaxWidthProperty, value); }

    // Specialized Cell Type
    public static readonly DependencyProperty CellTypeProperty =
        DependencyProperty.Register(nameof(CellType), typeof(CellType), typeof(ColumnLeaf), new PropertyMetadata(CellType.Text));
    public CellType CellType { get => (CellType)GetValue(CellTypeProperty); set => SetValue(CellTypeProperty, value); }

    public static readonly DependencyProperty StringFormatProperty =
        DependencyProperty.Register(nameof(StringFormat), typeof(string), typeof(ColumnLeaf), new PropertyMetadata(string.Empty));
    public string StringFormat { get => (string)GetValue(StringFormatProperty); set => SetValue(StringFormatProperty, value); }

    // Aggregate Footer Properties
    public static readonly DependencyProperty AggregateTypeProperty =
        DependencyProperty.Register(nameof(AggregateType), typeof(AggregateType), typeof(ColumnLeaf), new PropertyMetadata(AggregateType.None));
    public AggregateType AggregateType { get => (AggregateType)GetValue(AggregateTypeProperty); set => SetValue(AggregateTypeProperty, value); }

    public static readonly DependencyProperty AggregateFormatProperty =
        DependencyProperty.Register(nameof(AggregateFormat), typeof(string), typeof(ColumnLeaf), new PropertyMetadata(string.Empty));
    public string AggregateFormat { get => (string)GetValue(AggregateFormatProperty); set => SetValue(AggregateFormatProperty, value); }

    public static readonly DependencyProperty CustomAggregateTextProperty =
        DependencyProperty.Register(nameof(CustomAggregateText), typeof(string), typeof(ColumnLeaf), new PropertyMetadata(string.Empty));
    public string CustomAggregateText { get => (string)GetValue(CustomAggregateTextProperty); set => SetValue(CustomAggregateTextProperty, value); }

    public override IEnumerable<ColumnLeaf> GetLeaves() { yield return this; }
    public override int GetDepth() => 1;
}

// Legacy classes for backward compatibility
public class StackedColumn : DependencyObject
{
    public static readonly DependencyProperty HeaderTextProperty = DependencyProperty.Register(nameof(HeaderText), typeof(string), typeof(StackedColumn), new PropertyMetadata(string.Empty));
    public string HeaderText { get => (string)GetValue(HeaderTextProperty); set => SetValue(HeaderTextProperty, value); }
    public static readonly DependencyProperty ChildColumnsProperty = DependencyProperty.Register(nameof(ChildColumns), typeof(string), typeof(StackedColumn), new PropertyMetadata(string.Empty));
    public string ChildColumns { get => (string)GetValue(ChildColumnsProperty); set => SetValue(ChildColumnsProperty, value); }
    public static readonly DependencyProperty ForegroundProperty = DependencyProperty.Register(nameof(Foreground), typeof(Brush), typeof(StackedColumn), new PropertyMetadata(null));
    public Brush? Foreground { get => (Brush?)GetValue(ForegroundProperty); set => SetValue(ForegroundProperty, value); }
    public static readonly DependencyProperty BackgroundProperty = DependencyProperty.Register(nameof(Background), typeof(Brush), typeof(StackedColumn), new PropertyMetadata(null));
    public Brush? Background { get => (Brush?)GetValue(BackgroundProperty); set => SetValue(BackgroundProperty, value); }
}

public class StackedHeaderRow : DependencyObject
{
    public ObservableCollection<StackedColumn> StackedColumns { get; } = new();
    public static readonly DependencyProperty RowHeightProperty = DependencyProperty.Register(nameof(RowHeight), typeof(double), typeof(StackedHeaderRow), new PropertyMetadata(32.0));
    public double RowHeight { get => (double)GetValue(RowHeightProperty); set => SetValue(RowHeightProperty, value); }
}

// ─────────────────────────────────────────────────────────────────────────────
// StackedHeaderDataGrid: Enterprise DataGrid with Multi-Level Headers, Rich Cells & Clipboard
// ─────────────────────────────────────────────────────────────────────────────

public class StackedHeaderDataGrid : DataGrid
{
    public static readonly DependencyProperty ColumnHeaderTreeProperty =
        DependencyProperty.Register(nameof(ColumnHeaderTree),
            typeof(ObservableCollection<ColumnNode>), typeof(StackedHeaderDataGrid),
            new PropertyMetadata(null, OnColumnHeaderTreeChanged));

    public ObservableCollection<ColumnNode> ColumnHeaderTree
    {
        get => (ObservableCollection<ColumnNode>)GetValue(ColumnHeaderTreeProperty);
        set => SetValue(ColumnHeaderTreeProperty, value);
    }

    private static void OnColumnHeaderTreeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var g = (StackedHeaderDataGrid)d;
        if (e.OldValue is ObservableCollection<ColumnNode> old) old.CollectionChanged -= g.OnTreeChanged;
        if (e.NewValue is ObservableCollection<ColumnNode> nw) nw.CollectionChanged += g.OnTreeChanged;
        g.RefreshTreeSubscriptions();
        g.RebuildFromTree();
    }

    private readonly HashSet<ColumnNode> _observedNodes = new();
    private readonly HashSet<DataGridColumn> _observedColumns = new();
    private System.Windows.Threading.DispatcherOperation? _pendingFooterRebuild;
    private long _footerGeneration;

    private void OnTreeChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        RefreshTreeSubscriptions();
        RebuildFromTree();
    }

    private void RefreshTreeSubscriptions()
    {
        foreach (var node in _observedNodes)
        {
            node.NodeChanged -= OnTreeNodeChanged;
            if (node is ColumnGroup group) group.Children.CollectionChanged -= OnTreeChanged;
        }
        _observedNodes.Clear();

        if (ColumnHeaderTree == null) return;
        var pending = new Stack<ColumnNode>(ColumnHeaderTree.Reverse());
        while (pending.Count > 0)
        {
            var node = pending.Pop();
            if (!_observedNodes.Add(node)) continue;
            node.NodeChanged += OnTreeNodeChanged;
            if (node is ColumnGroup group)
            {
                group.Children.CollectionChanged += OnTreeChanged;
                for (int i = group.Children.Count - 1; i >= 0; i--) pending.Push(group.Children[i]);
            }
        }
    }

    private void OnTreeNodeChanged(object? sender, EventArgs e) => RebuildFromTree();

    public static readonly DependencyProperty StackedHeaderRowsProperty =
        DependencyProperty.Register(nameof(StackedHeaderRows),
            typeof(ObservableCollection<StackedHeaderRow>), typeof(StackedHeaderDataGrid), new PropertyMetadata(null));
    public ObservableCollection<StackedHeaderRow> StackedHeaderRows
    {
        get => (ObservableCollection<StackedHeaderRow>)GetValue(StackedHeaderRowsProperty);
        set => SetValue(StackedHeaderRowsProperty, value);
    }

    public static readonly DependencyProperty StackedRowHeightProperty =
        DependencyProperty.Register(nameof(StackedRowHeight), typeof(double), typeof(StackedHeaderDataGrid), new PropertyMetadata(32.0));
    public double StackedRowHeight { get => (double)GetValue(StackedRowHeightProperty); set => SetValue(StackedRowHeightProperty, value); }

    public static readonly DependencyProperty ShowFooterSummaryProperty =
        DependencyProperty.Register(nameof(ShowFooterSummary), typeof(bool), typeof(StackedHeaderDataGrid), new PropertyMetadata(true, OnFooterSummarySettingChanged));
    public bool ShowFooterSummary { get => (bool)GetValue(ShowFooterSummaryProperty); set => SetValue(ShowFooterSummaryProperty, value); }

    public static readonly DependencyProperty FooterRowHeightProperty =
        DependencyProperty.Register(nameof(FooterRowHeight), typeof(double), typeof(StackedHeaderDataGrid), new PropertyMetadata(30.0, OnFooterSummarySettingChanged));
    public double FooterRowHeight { get => (double)GetValue(FooterRowHeightProperty); set => SetValue(FooterRowHeightProperty, value); }

    public static readonly DependencyProperty EnableContextMenuProperty =
        DependencyProperty.Register(nameof(EnableContextMenu), typeof(bool), typeof(StackedHeaderDataGrid), new PropertyMetadata(true));
    public bool EnableContextMenu { get => (bool)GetValue(EnableContextMenuProperty); set => SetValue(EnableContextMenuProperty, value); }

    private static void OnFooterSummarySettingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is StackedHeaderDataGrid g) g.RebuildFooterSummary();
    }

    private Grid? _stackedHeaderGrid;
    private Grid? _footerSummaryGrid;
    private DataGridColumnHeadersPresenter? _columnHeadersPresenter;
    private ScrollViewer? _scrollViewer;
    private readonly Dictionary<ColumnLeaf, Path> _sortIndicators = new();
    private bool _isSyncingWidths = false;

    static StackedHeaderDataGrid()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StackedHeaderDataGrid), new FrameworkPropertyMetadata(typeof(StackedHeaderDataGrid)));
    }

    public StackedHeaderDataGrid()
    {
        ColumnHeaderTree = new ObservableCollection<ColumnNode>();
        StackedHeaderRows = new ObservableCollection<StackedHeaderRow>();
        StackedHeaderRows.CollectionChanged += (s, e) => RebuildStackedHeaders();
        Loaded += OnLoaded;
    }

    protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
    {
        base.OnItemsSourceChanged(oldValue, newValue);
        if (oldValue is INotifyCollectionChanged oldCol) oldCol.CollectionChanged -= OnItemsSourceCollectionChanged;
        if (newValue is INotifyCollectionChanged newCol) newCol.CollectionChanged += OnItemsSourceCollectionChanged;
        ConfigureTextColumnBindings();
        RebuildFooterSummary();
    }

    private void OnItemsSourceCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (_pendingFooterRebuild is { Status: System.Windows.Threading.DispatcherOperationStatus.Pending }) return;
        _pendingFooterRebuild = Dispatcher.BeginInvoke(new Action(() =>
        {
            _pendingFooterRebuild = null;
            RebuildFooterSummary();
        }), System.Windows.Threading.DispatcherPriority.Background);
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        EnsureTemplateElements();
        if (EnableContextMenu && ContextMenu == null)
            ContextMenu = DataGridContextMenuHelper.CreateDefaultContextMenu(this);

        if (GroupStyle.Count == 0 && TryFindResource("CONCOST.DataGridGroupItem.Base") is Style groupItemStyle)
        {
            var gs = new GroupStyle { ContainerStyle = groupItemStyle };
            GroupStyle.Add(gs);
        }

        RebuildStackedHeaders();
        RebuildFooterSummary();
        HookColumnActualWidthChanges();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.Key == Key.C && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
        {
            DataGridClipboardHelper.CopyToClipboard(this, false);
            e.Handled = true;
        }
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        EnsureTemplateElements();
        RebuildStackedHeaders();
        RebuildFooterSummary();
        HookColumnActualWidthChanges();
    }

    private void EnsureTemplateElements()
    {
        if (_stackedHeaderGrid == null)
            _stackedHeaderGrid = FindVisualChildByName<Grid>(this, "PART_StackedHeaderGrid");
        if (_footerSummaryGrid == null)
            _footerSummaryGrid = FindVisualChildByName<Grid>(this, "PART_FooterSummaryGrid");
        if (_columnHeadersPresenter == null)
            _columnHeadersPresenter = FindVisualChild<DataGridColumnHeadersPresenter>(this);
        if (_scrollViewer == null)
        {
            _scrollViewer = FindVisualChild<ScrollViewer>(this);
            if (_scrollViewer != null)
            {
                _scrollViewer.ScrollChanged -= OnScrollViewerScrollChanged;
                _scrollViewer.ScrollChanged += OnScrollViewerScrollChanged;
            }
        }
    }

    private void OnScrollViewerScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        // CRITICAL FIX: Ignore ScrollChanged events bubbling up from inner TextBoxes, ComboBoxes or cell controls
        if (e.OriginalSource != _scrollViewer) return;

        var transform = new TranslateTransform(-_scrollViewer.HorizontalOffset, 0);
        if (_stackedHeaderGrid != null) _stackedHeaderGrid.RenderTransform = transform;
        if (_footerSummaryGrid != null) _footerSummaryGrid.RenderTransform = transform;
    }

    private void HookColumnActualWidthChanges()
    {
        var descriptor = DependencyPropertyDescriptor.FromProperty(DataGridColumn.ActualWidthProperty, typeof(DataGridColumn));
        if (descriptor != null)
        {
            foreach (var oldColumn in _observedColumns.ToArray())
            {
                if (Columns.Contains(oldColumn)) continue;
                descriptor.RemoveValueChanged(oldColumn, OnColumnActualWidthChanged);
                _observedColumns.Remove(oldColumn);
            }
            foreach (var col in Columns)
            {
                if (_observedColumns.Add(col)) descriptor.AddValueChanged(col, OnColumnActualWidthChanged);
            }
        }
    }

    private void OnColumnActualWidthChanged(object? sender, EventArgs e)
    {
        SyncColumnWidths();
    }

    private void OnGridCellEditEnding(object? sender, DataGridCellEditEndingEventArgs e)
    {
        Dispatcher.BeginInvoke(new Action(() =>
        {
            SyncColumnWidths();
            RebuildFooterSummary();
        }), System.Windows.Threading.DispatcherPriority.Background);
    }

    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
        base.OnRenderSizeChanged(sizeInfo);
        SyncColumnWidths();
    }

    private void RebuildFromTree()
    {
        if (ColumnHeaderTree == null || ColumnHeaderTree.Count == 0)
        {
            if (Columns.Count > 0)
            {
                Columns.Clear();
                HookColumnActualWidthChanges();
            }
            RebuildStackedHeaders();
            RebuildFooterSummary();
            return;
        }

        // Auto-generate DataGrid columns with Rich Cell Types
        Columns.Clear();
        var editStyle = TryFindResource("CONCOST.DataGrid.EditingTextBox") as Style;

        foreach (var leaf in ColumnHeaderTree.SelectMany(n => n.GetLeaves()))
        {
            DataGridColumn col;

            switch (leaf.CellType)
            {
                case CellType.Progress:
                    col = new DataGridTemplateColumn
                    {
                        Header = leaf.HeaderText,
                        CellTemplate = DataGridCellTemplateFactory.CreateProgressTemplate(leaf.BindingPath),
                        Width = leaf.Width,
                        MinWidth = leaf.MinWidth,
                        MaxWidth = leaf.MaxWidth,
                        CanUserSort = leaf.CanUserSort,
                        CanUserResize = leaf.CanUserResize,
                        SortMemberPath = string.IsNullOrEmpty(leaf.SortMemberPath) ? leaf.BindingPath : leaf.SortMemberPath
                    };
                    break;

                case CellType.Badge:
                    col = new DataGridTemplateColumn
                    {
                        Header = leaf.HeaderText,
                        CellTemplate = DataGridCellTemplateFactory.CreateBadgeTemplate(leaf.BindingPath),
                        Width = leaf.Width,
                        MinWidth = leaf.MinWidth,
                        MaxWidth = leaf.MaxWidth,
                        CanUserSort = leaf.CanUserSort,
                        CanUserResize = leaf.CanUserResize,
                        SortMemberPath = string.IsNullOrEmpty(leaf.SortMemberPath) ? leaf.BindingPath : leaf.SortMemberPath
                    };
                    break;

                case CellType.Rating:
                    col = new DataGridTemplateColumn
                    {
                        Header = leaf.HeaderText,
                        CellTemplate = DataGridCellTemplateFactory.CreateRatingTemplate(leaf.BindingPath),
                        Width = leaf.Width,
                        MinWidth = leaf.MinWidth,
                        MaxWidth = leaf.MaxWidth,
                        CanUserSort = leaf.CanUserSort,
                        CanUserResize = leaf.CanUserResize,
                        SortMemberPath = string.IsNullOrEmpty(leaf.SortMemberPath) ? leaf.BindingPath : leaf.SortMemberPath
                    };
                    break;

                case CellType.Date:
                    col = new DataGridTemplateColumn
                    {
                        Header = leaf.HeaderText,
                        CellTemplate = DataGridCellTemplateFactory.CreateDateTemplate(leaf.BindingPath, string.IsNullOrEmpty(leaf.StringFormat) ? "dd/MM/yyyy" : leaf.StringFormat),
                        Width = leaf.Width,
                        MinWidth = leaf.MinWidth,
                        MaxWidth = leaf.MaxWidth,
                        CanUserSort = leaf.CanUserSort,
                        CanUserResize = leaf.CanUserResize,
                        SortMemberPath = string.IsNullOrEmpty(leaf.SortMemberPath) ? leaf.BindingPath : leaf.SortMemberPath
                    };
                    break;

                case CellType.Numeric:
                    col = new DataGridTemplateColumn
                    {
                        Header = leaf.HeaderText,
                        CellTemplate = DataGridCellTemplateFactory.CreateNumericTemplate(leaf.BindingPath, string.IsNullOrEmpty(leaf.StringFormat) ? "N0" : leaf.StringFormat, leaf.HorizontalAlignment),
                        Width = leaf.Width,
                        MinWidth = leaf.MinWidth,
                        MaxWidth = leaf.MaxWidth,
                        CanUserSort = leaf.CanUserSort,
                        CanUserResize = leaf.CanUserResize,
                        SortMemberPath = string.IsNullOrEmpty(leaf.SortMemberPath) ? leaf.BindingPath : leaf.SortMemberPath
                    };
                    break;

                case CellType.Hyperlink:
                    col = new DataGridTemplateColumn
                    {
                        Header = leaf.HeaderText,
                        CellTemplate = DataGridCellTemplateFactory.CreateHyperlinkTemplate(leaf.BindingPath),
                        Width = leaf.Width,
                        MinWidth = leaf.MinWidth,
                        MaxWidth = leaf.MaxWidth,
                        CanUserSort = leaf.CanUserSort,
                        CanUserResize = leaf.CanUserResize,
                        SortMemberPath = string.IsNullOrEmpty(leaf.SortMemberPath) ? leaf.BindingPath : leaf.SortMemberPath
                    };
                    break;

                case CellType.RowExpander:
                    col = new DataGridTemplateColumn
                    {
                        Header = leaf.HeaderText,
                        CellTemplate = DataGridCellTemplateFactory.CreateRowExpanderTemplate(),
                        Width = leaf.Width.IsStar ? new DataGridLength(36, DataGridLengthUnitType.Pixel) : leaf.Width,
                        MinWidth = 30,
                        MaxWidth = 50,
                        CanUserSort = false,
                        CanUserResize = false
                    };
                    break;

                case CellType.CheckBox:
                    col = new DataGridTemplateColumn
                    {
                        Header = leaf.HeaderText,
                        CellTemplate = DataGridCellTemplateFactory.CreateCheckBoxTemplate(leaf.BindingPath),
                        Width = leaf.Width,
                        MinWidth = leaf.MinWidth,
                        MaxWidth = leaf.MaxWidth,
                        CanUserSort = leaf.CanUserSort,
                        CanUserResize = leaf.CanUserResize,
                        SortMemberPath = string.IsNullOrEmpty(leaf.SortMemberPath) ? leaf.BindingPath : leaf.SortMemberPath
                    };
                    break;

                default:
                    var textCol = new DataGridTextColumn
                    {
                        Header = leaf.HeaderText,
                        Width = leaf.Width,
                        MinWidth = leaf.MinWidth,
                        MaxWidth = leaf.MaxWidth,
                        CanUserSort = leaf.CanUserSort,
                        CanUserResize = leaf.CanUserResize,
                        SortMemberPath = string.IsNullOrEmpty(leaf.SortMemberPath) ? leaf.BindingPath : leaf.SortMemberPath
                    };

                    var textBlockStyle = new Style(typeof(TextBlock));
                    textBlockStyle.Setters.Add(new Setter(TextBlock.TextTrimmingProperty, TextTrimming.CharacterEllipsis));
                    textBlockStyle.Setters.Add(new Setter(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center));
                    if (leaf.HorizontalAlignment == HorizontalAlignment.Left)
                        textBlockStyle.Setters.Add(new Setter(TextBlock.TextAlignmentProperty, TextAlignment.Left));
                    else if (leaf.HorizontalAlignment == HorizontalAlignment.Right)
                        textBlockStyle.Setters.Add(new Setter(TextBlock.TextAlignmentProperty, TextAlignment.Right));
                    else
                        textBlockStyle.Setters.Add(new Setter(TextBlock.TextAlignmentProperty, TextAlignment.Center));
                    textCol.ElementStyle = textBlockStyle;

                    if (editStyle != null) textCol.EditingElementStyle = editStyle;
                    col = textCol;
                    break;
            }

            Columns.Add(col);
        }

        CellEditEnding -= OnGridCellEditEnding;
        CellEditEnding += OnGridCellEditEnding;

        ConfigureTextColumnBindings();
        HookColumnActualWidthChanges();
        RebuildStackedHeaders();
        RebuildFooterSummary();
    }

    /// <summary>
    /// Uses a TwoWay binding only when the bound item property has a public setter.
    /// Read-only models (including anonymous types) must not enter DataGrid edit mode,
    /// because DataGridTextColumn otherwise creates a TwoWay binding by default.
    /// </summary>
    private void ConfigureTextColumnBindings()
    {
        var leaves = ColumnHeaderTree?.SelectMany(n => n.GetLeaves()).ToList();
        if (leaves == null || Columns.Count == 0) return;

        var itemType = GetItemsSourceItemType(ItemsSource);
        for (int i = 0; i < Columns.Count && i < leaves.Count; i++)
        {
            if (Columns[i] is not DataGridTextColumn textColumn) continue;

            var path = leaves[i].BindingPath;
            if (string.IsNullOrWhiteSpace(path))
            {
                textColumn.Binding = null;
                textColumn.IsReadOnly = true;
                continue;
            }

            bool canWrite = HasPublicSetter(itemType, path);
            textColumn.Binding = new Binding(path)
            {
                Mode = canWrite ? BindingMode.TwoWay : BindingMode.OneWay,
                UpdateSourceTrigger = canWrite ? UpdateSourceTrigger.LostFocus : UpdateSourceTrigger.Default
            };
            textColumn.IsReadOnly = !canWrite;
        }
    }

    private static Type? GetItemsSourceItemType(IEnumerable? source)
    {
        if (source == null) return null;

        foreach (var item in source)
        {
            if (item != null) return item.GetType();
        }

        var sourceType = source.GetType();
        var enumerableType = sourceType.GetInterfaces()
            .Append(sourceType)
            .FirstOrDefault(type => type.IsGenericType &&
                type.GetGenericTypeDefinition() == typeof(IEnumerable<>));
        return enumerableType?.GetGenericArguments()[0];
    }

    private static bool HasPublicSetter(Type? itemType, string bindingPath)
    {
        if (itemType == null || string.IsNullOrWhiteSpace(bindingPath)) return false;

        var currentType = itemType;
        PropertyInfo? property = null;
        foreach (var segment in bindingPath.Split('.'))
        {
            property = currentType.GetProperty(segment, BindingFlags.Public | BindingFlags.Instance);
            if (property == null) return false;
            currentType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
        }

        return property?.SetMethod?.IsPublic == true;
    }

    public void RebuildStackedHeaders()
    {
        EnsureTemplateElements();
        if (_stackedHeaderGrid == null) return;

        if (ColumnHeaderTree != null && ColumnHeaderTree.Count > 0)
        {
            if (_columnHeadersPresenter != null)
                _columnHeadersPresenter.Visibility = Visibility.Collapsed;

            RebuildStackedHeadersFromTree();
        }
        else if (StackedHeaderRows != null && StackedHeaderRows.Count > 0)
        {
            if (_columnHeadersPresenter != null)
                _columnHeadersPresenter.Visibility = Visibility.Visible;

            RebuildLegacyStackedHeaders();
        }
        else
        {
            _stackedHeaderGrid.Visibility = Visibility.Collapsed;
            if (_columnHeadersPresenter != null)
                _columnHeadersPresenter.Visibility = Visibility.Visible;
        }
    }

    private void RebuildStackedHeadersFromTree()
    {
        if (_stackedHeaderGrid == null) return;
        _stackedHeaderGrid.Children.Clear();
        _stackedHeaderGrid.RowDefinitions.Clear();
        _stackedHeaderGrid.ColumnDefinitions.Clear();
        _sortIndicators.Clear();

        if (ColumnHeaderTree == null || ColumnHeaderTree.Count == 0)
        {
            _stackedHeaderGrid.Visibility = Visibility.Collapsed;
            return;
        }

        int totalDepth = ColumnHeaderTree.Max(n => n.GetDepth());
        if (totalDepth == 0) { _stackedHeaderGrid.Visibility = Visibility.Collapsed; return; }

        _stackedHeaderGrid.Visibility = Visibility.Visible;

        for (int r = 0; r < totalDepth; r++)
            _stackedHeaderGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(StackedRowHeight, GridUnitType.Pixel) });

        var leaves = ColumnHeaderTree.SelectMany(n => n.GetLeaves()).ToList();
        for (int i = 0; i < leaves.Count; i++)
        {
            var leaf = leaves[i];
            var colDef = new ColumnDefinition();
            if (leaf.Width.IsStar) colDef.Width = new GridLength(leaf.Width.Value, GridUnitType.Star);
            else if (leaf.Width.IsAbsolute) colDef.Width = new GridLength(leaf.Width.Value, GridUnitType.Pixel);
            else colDef.Width = new GridLength(1, GridUnitType.Star);
            _stackedHeaderGrid.ColumnDefinitions.Add(colDef);
        }

        int colOffset = 0;
        foreach (var node in ColumnHeaderTree)
        {
            PlaceNode(node, 0, colOffset, totalDepth);
            colOffset += node.GetLeaves().Count();
        }

        if (_scrollViewer != null && _scrollViewer.HorizontalOffset > 0)
            _stackedHeaderGrid.RenderTransform = new TranslateTransform(-_scrollViewer.HorizontalOffset, 0);

        SyncColumnWidths();
        Dispatcher.BeginInvoke(new Action(SyncColumnWidths), System.Windows.Threading.DispatcherPriority.Loaded);
    }

    private void PlaceNode(ColumnNode node, int row, int colOffset, int totalDepth)
    {
        int leafCount = node.GetLeaves().Count();
        int nodeDepth = node.GetDepth();
        int rowSpan = (nodeDepth == 1) ? (totalDepth - row) : 1;
        bool hasBorderBottom = (nodeDepth > 1);
        bool isLeaf = (node is ColumnLeaf);

        var cell = CreateInteractiveHeaderCell(node, isLeaf, hasBorderBottom, colOffset);
        Grid.SetRow(cell, row);
        Grid.SetColumn(cell, colOffset);
        Grid.SetColumnSpan(cell, leafCount);
        Grid.SetRowSpan(cell, rowSpan);
        _stackedHeaderGrid!.Children.Add(cell);

        if (node is ColumnGroup grp && grp.Children.Count > 0)
        {
            int off = colOffset;
            foreach (var c in grp.Children)
            {
                PlaceNode(c, row + 1, off, totalDepth);
                off += c.GetLeaves().Count();
            }
        }
    }

    private FrameworkElement CreateInteractiveHeaderCell(ColumnNode node, bool isLeaf, bool hasBorderBottom, int leafColumnIndex)
    {
        var grid = new Grid
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch
        };

        var border = new Border
        {
            BorderThickness = new Thickness(0, 0, 1, hasBorderBottom ? 1 : 0),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Padding = new Thickness(6, 4, 6, 4),
            Cursor = Cursors.Arrow
        };

        if (node.Background != null) border.Background = node.Background;
        else border.SetResourceReference(Border.BackgroundProperty, "CONCOST.Brush.SurfaceSecondary");
        border.SetResourceReference(Border.BorderBrushProperty, "CONCOST.Brush.Border");

        if (node.ToolTip != null) border.ToolTip = node.ToolTip;

        var contentPanel = new DockPanel
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            LastChildFill = true
        };

        Path? sortArrow = null;
        if (isLeaf && node is ColumnLeaf leaf && leaf.CanUserSort && CanUserSortColumns)
        {
            sortArrow = new Path
            {
                Width = 8,
                Height = 6,
                Stretch = Stretch.Fill,
                Margin = new Thickness(4, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center,
                Visibility = Visibility.Collapsed
            };
            sortArrow.SetResourceReference(Shape.FillProperty, "CONCOST.Brush.Primary");
            DockPanel.SetDock(sortArrow, Dock.Right);
            contentPanel.Children.Add(sortArrow);
            _sortIndicators[leaf] = sortArrow;
        }

        if (node.HeaderTemplate != null)
        {
            var presenter = new ContentPresenter
            {
                Content = node.HeaderText,
                ContentTemplate = node.HeaderTemplate,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = node.HorizontalAlignment
            };
            contentPanel.Children.Add(presenter);
        }
        else
        {
            var tb = new TextBlock
            {
                Text = node.HeaderText,
                FontWeight = (!isLeaf) ? FontWeights.Normal : FontWeights.Normal,
                FontSize = 11.5,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                TextAlignment = (node.HorizontalAlignment == HorizontalAlignment.Left) ? TextAlignment.Left :
                                (node.HorizontalAlignment == HorizontalAlignment.Right) ? TextAlignment.Right : TextAlignment.Center,
                TextWrapping = TextWrapping.Wrap,
                TextTrimming = TextTrimming.CharacterEllipsis
            };
            if (node.Foreground != null) tb.Foreground = node.Foreground;
            else if (!isLeaf) tb.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.Primary");
            else tb.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextPrimary");
            contentPanel.Children.Add(tb);
        }

        border.Child = contentPanel;
        grid.Children.Add(border);

        if (isLeaf && node is ColumnLeaf leafNode)
        {
            Point dragStart = default;
            bool isDragging = false;

            border.PreviewMouseLeftButtonDown += (s, e) =>
            {
                dragStart = e.GetPosition(null);
                isDragging = false;
            };

            border.PreviewMouseMove += (s, e) =>
            {
                if (e.LeftButton == MouseButtonState.Pressed && !isDragging)
                {
                    Point cur = e.GetPosition(null);
                    if (Math.Abs(cur.X - dragStart.X) > SystemParameters.MinimumHorizontalDragDistance ||
                        Math.Abs(cur.Y - dragStart.Y) > SystemParameters.MinimumVerticalDragDistance)
                    {
                        isDragging = true;
                        string prop = !string.IsNullOrEmpty(leafNode.BindingPath) ? leafNode.BindingPath : leafNode.HeaderText;
                        var data = new DataObject(DataFormats.StringFormat, prop);
                        data.SetData(typeof(string), prop);
                        DragDrop.DoDragDrop(border, data, DragDropEffects.Move);
                    }
                }
            };

            border.MouseLeftButtonUp += (s, e) =>
            {
                if (!isDragging && leafNode.CanUserSort && CanUserSortColumns)
                {
                    PerformSort(leafNode, leafColumnIndex);
                }
                isDragging = false;
            };

            if (leafNode.CanUserResize && CanUserResizeColumns)
            {
                var gripper = new Thumb
                {
                    Width = 5,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Cursor = Cursors.SizeWE,
                    Opacity = 0,
                    Margin = new Thickness(0, 0, -2, 0)
                };
                gripper.DragDelta += (s, e) =>
                {
                    if (leafColumnIndex >= 0 && leafColumnIndex < Columns.Count)
                    {
                        var targetCol = Columns[leafColumnIndex];
                        double currentWidth = targetCol.ActualWidth > 0 ? targetCol.ActualWidth : 100;
                        double newWidth = Math.Max(leafNode.MinWidth, Math.Min(leafNode.MaxWidth, currentWidth + e.HorizontalChange));
                        targetCol.Width = new DataGridLength(newWidth, DataGridLengthUnitType.Pixel);
                        SyncColumnWidths();
                    }
                };
                grid.Children.Add(gripper);
            }
        }

        return grid;
    }

    private void PerformSort(ColumnLeaf leaf, int columnIndex)
    {
        if (columnIndex < 0 || columnIndex >= Columns.Count) return;
        var col = Columns[columnIndex];
        string sortPath = !string.IsNullOrEmpty(leaf.SortMemberPath) ? leaf.SortMemberPath :
                          !string.IsNullOrEmpty(leaf.BindingPath) ? leaf.BindingPath : col.SortMemberPath;

        if (string.IsNullOrEmpty(sortPath)) return;

        var view = CollectionViewSource.GetDefaultView(ItemsSource);
        if (view == null) return;

        ListSortDirection newDirection = ListSortDirection.Ascending;
        if (col.SortDirection == ListSortDirection.Ascending)
            newDirection = ListSortDirection.Descending;
        else if (col.SortDirection == ListSortDirection.Descending)
            newDirection = ListSortDirection.Ascending;

        foreach (var c in Columns) c.SortDirection = null;
        foreach (var kvp in _sortIndicators) kvp.Value.Visibility = Visibility.Collapsed;

        col.SortDirection = newDirection;

        if (_sortIndicators.TryGetValue(leaf, out var arrow))
        {
            arrow.Visibility = Visibility.Visible;
            if (newDirection == ListSortDirection.Ascending)
                arrow.Data = Geometry.Parse("M 0 4 L 4 0 L 8 4 Z");
            else
                arrow.Data = Geometry.Parse("M 0 0 L 4 4 L 8 0 Z");
        }

        using (view.DeferRefresh())
        {
            view.SortDescriptions.Clear();
            view.SortDescriptions.Add(new SortDescription(sortPath, newDirection));
        }

        RebuildFooterSummary();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Bottom Aggregate Footer Panel Logic (Sum, Avg, Count, Min, Max)
    // ─────────────────────────────────────────────────────────────────────────

    private static readonly Dictionary<(Type, string), PropertyInfo?> _propertyCache = new();

    public void RebuildFooterSummary()
    {
        long generation = ++_footerGeneration;
        EnsureTemplateElements();
        if (_footerSummaryGrid == null) return;

        _footerSummaryGrid.Children.Clear();
        _footerSummaryGrid.RowDefinitions.Clear();
        _footerSummaryGrid.ColumnDefinitions.Clear();

        var leaves = ColumnHeaderTree?.SelectMany(n => n.GetLeaves()).ToList();
        bool hasAnyAggregate = ShowFooterSummary && leaves != null &&
            leaves.Any(l => l.AggregateType != AggregateType.None || !string.IsNullOrEmpty(l.CustomAggregateText));

        if (!hasAnyAggregate || leaves == null || leaves.Count == 0)
        {
            _footerSummaryGrid.Visibility = Visibility.Collapsed;
            return;
        }

        _footerSummaryGrid.Visibility = Visibility.Visible;
        _footerSummaryGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(FooterRowHeight, GridUnitType.Pixel) });

        var textBlocks = new List<(ColumnLeaf leaf, TextBlock tb)>();

        for (int i = 0; i < leaves.Count; i++)
        {
            var leaf = leaves[i];
            var colDef = new ColumnDefinition();
            if (leaf.Width.IsStar) colDef.Width = new GridLength(leaf.Width.Value, GridUnitType.Star);
            else if (leaf.Width.IsAbsolute) colDef.Width = new GridLength(leaf.Width.Value, GridUnitType.Pixel);
            else colDef.Width = new GridLength(1, GridUnitType.Star);
            _footerSummaryGrid.ColumnDefinitions.Add(colDef);

            var cellBorder = new Border
            {
                BorderThickness = new Thickness(0, 0, 1, 0),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Padding = new Thickness(8, 4, 8, 4)
            };
            cellBorder.SetResourceReference(Border.BackgroundProperty, "CONCOST.Brush.SurfaceSecondary");
            cellBorder.SetResourceReference(Border.BorderBrushProperty, "CONCOST.Brush.Border");

            var tb = new TextBlock
            {
                Text = !string.IsNullOrEmpty(leaf.CustomAggregateText) ? leaf.CustomAggregateText : "...",
                FontWeight = FontWeights.Normal,
                FontSize = 11.5,
                VerticalAlignment = VerticalAlignment.Center,
                TextAlignment = (leaf.HorizontalAlignment == HorizontalAlignment.Left) ? TextAlignment.Left :
                                (leaf.HorizontalAlignment == HorizontalAlignment.Right) ? TextAlignment.Right : TextAlignment.Center,
                TextWrapping = TextWrapping.NoWrap
            };
            tb.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextPrimary");

            cellBorder.Child = tb;
            Grid.SetRow(cellBorder, 0);
            Grid.SetColumn(cellBorder, i);
            _footerSummaryGrid.Children.Add(cellBorder);

            if (string.IsNullOrEmpty(leaf.CustomAggregateText) && leaf.AggregateType != AggregateType.None)
            {
                textBlocks.Add((leaf, tb));
            }
        }

        if (_scrollViewer != null && _scrollViewer.HorizontalOffset > 0)
            _footerSummaryGrid.RenderTransform = new TranslateTransform(-_scrollViewer.HorizontalOffset, 0);

        SyncColumnWidths();

        // High-Performance Fast Aggregation
        if (textBlocks.Count > 0 && ItemsSource != null)
        {
            var rawItems = new List<object>();
            foreach (var itm in ItemsSource)
            {
                if (itm != null) rawItems.Add(itm);
            }

            if (rawItems.Count == 0)
            {
                foreach (var item in textBlocks) item.tb.Text = string.Empty;
                return;
            }

            // Compute in Task if > 20000 items, otherwise compute directly for instant responsiveness
            if (rawItems.Count > 20000)
            {
                System.Threading.Tasks.Task.Run(() =>
                {
                    var results = new Dictionary<ColumnLeaf, string>();
                    foreach (var (leaf, _) in textBlocks)
                    {
                        results[leaf] = ComputeAggregateFast(leaf, rawItems);
                    }

                    Dispatcher.InvokeAsync(new Action(() =>
                    {
                        if (generation != _footerGeneration) return;
                        foreach (var (leaf, tb) in textBlocks)
                        {
                            if (results.TryGetValue(leaf, out var text))
                                tb.Text = text;
                        }
                    }), System.Windows.Threading.DispatcherPriority.Normal);
                });
            }
            else
            {
                foreach (var (leaf, tb) in textBlocks)
                {
                    tb.Text = ComputeAggregateFast(leaf, rawItems);
                }
            }
        }
    }

    private static string ComputeAggregateFast(ColumnLeaf leaf, List<object> itemsList)
    {
        if (itemsList.Count == 0 || leaf.AggregateType == AggregateType.None) return string.Empty;

        if (leaf.AggregateType == AggregateType.Count)
            return string.Format(CultureInfo.InvariantCulture, "Count: {0}", itemsList.Count);

        string bindingPath = leaf.BindingPath;
        if (string.IsNullOrEmpty(bindingPath)) return string.Empty;

        Type? firstItemType = itemsList[0]?.GetType();
        if (firstItemType == null) return string.Empty;

        var prop = GetCachedProperty(firstItemType, bindingPath);
        if (prop == null) return string.Empty;

        var numbers = new List<double>(itemsList.Count);
        for (int i = 0; i < itemsList.Count; i++)
        {
            var val = prop.GetValue(itemsList[i]);
            if (val is double d) numbers.Add(d);
            else if (val is int intVal) numbers.Add(intVal);
            else if (val is float f) numbers.Add(f);
            else if (val is decimal dec) numbers.Add((double)dec);
            else if (val != null && double.TryParse(val.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out double parsed))
            {
                numbers.Add(parsed);
            }
        }

        if (numbers.Count == 0) return string.Empty;

        double result = 0;
        string prefix = "";

        switch (leaf.AggregateType)
        {
            case AggregateType.Sum:
                result = numbers.Sum();
                prefix = "Sum: ";
                break;
            case AggregateType.Average:
                result = numbers.Average();
                prefix = "Avg: ";
                break;
            case AggregateType.Min:
                result = numbers.Min();
                prefix = "Min: ";
                break;
            case AggregateType.Max:
                result = numbers.Max();
                prefix = "Max: ";
                break;
            default:
                return string.Empty;
        }

        if (!string.IsNullOrEmpty(leaf.AggregateFormat))
        {
            return string.Format(CultureInfo.CurrentCulture, leaf.AggregateFormat, result);
        }

        return prefix + result.ToString("N0", CultureInfo.CurrentCulture);
    }

    private static PropertyInfo? GetCachedProperty(Type type, string propertyName)
    {
        var key = (type, propertyName);
        lock (_propertyCache)
        {
            if (_propertyCache.TryGetValue(key, out var prop))
                return prop;

            prop = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            _propertyCache[key] = prop;
            return prop;
        }
    }

    private void RebuildLegacyStackedHeaders()
    {
        if (_stackedHeaderGrid == null) return;
        _stackedHeaderGrid.Children.Clear();
        _stackedHeaderGrid.RowDefinitions.Clear();
        _stackedHeaderGrid.ColumnDefinitions.Clear();

        if (StackedHeaderRows == null || StackedHeaderRows.Count == 0 || Columns.Count == 0)
        {
            _stackedHeaderGrid.Visibility = Visibility.Collapsed;
            return;
        }

        _stackedHeaderGrid.Visibility = Visibility.Visible;

        foreach (var col in Columns)
        {
            var cd = new ColumnDefinition();
            if (col.Width.IsStar) cd.Width = new GridLength(col.Width.Value, GridUnitType.Star);
            else if (col.Width.IsAbsolute) cd.Width = new GridLength(col.Width.Value, GridUnitType.Pixel);
            else cd.Width = new GridLength(1, GridUnitType.Star);
            _stackedHeaderGrid.ColumnDefinitions.Add(cd);
        }

        int totalRows = StackedHeaderRows.Count;
        foreach (var hr in StackedHeaderRows)
            _stackedHeaderGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(hr.RowHeight, GridUnitType.Pixel) });

        for (int ri = 0; ri < totalRows; ri++)
        {
            var hr = StackedHeaderRows[ri];
            var occupied = new HashSet<int>();
            foreach (var sc in hr.StackedColumns)
            {
                var names = (sc.ChildColumns ?? "").Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToList();
                if (names.Count == 0) continue;
                int s = -1, e = -1;
                for (int i = 0; i < Columns.Count; i++)
                {
                    var h = Columns[i].Header?.ToString() ?? "";
                    if (names.Any(n => n.Equals(h, StringComparison.OrdinalIgnoreCase) || (int.TryParse(n, out int idx) && idx == i)))
                    {
                        if (s == -1) s = i;
                        e = i;
                        occupied.Add(i);
                    }
                }
                if (s >= 0 && e >= s)
                {
                    int span = e - s + 1;
                    var b = new Border
                    {
                        BorderThickness = new Thickness(0, 0, 1, 1),
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        VerticalAlignment = VerticalAlignment.Stretch,
                        Padding = new Thickness(8, 4, 8, 4)
                    };
                    if (sc.Background != null) b.Background = sc.Background;
                    else b.SetResourceReference(Border.BackgroundProperty, "CONCOST.Brush.SurfaceSecondary");
                    b.SetResourceReference(Border.BorderBrushProperty, "CONCOST.Brush.Border");

                    var tb = new TextBlock
                    {
                        Text = sc.HeaderText,
                        FontWeight = FontWeights.Normal,
                        FontSize = 11.5,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        TextAlignment = TextAlignment.Center
                    };
                    if (sc.Foreground != null) tb.Foreground = sc.Foreground;
                    else tb.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.Primary");
                    b.Child = tb;

                    Grid.SetRow(b, ri);
                    Grid.SetColumn(b, s);
                    Grid.SetColumnSpan(b, span);
                    _stackedHeaderGrid.Children.Add(b);
                }
            }
        }
    }

    private void SyncColumnWidths()
    {
        if (_isSyncingWidths) return;
        if (_stackedHeaderGrid == null || Columns.Count == 0) return;

        try
        {
            _isSyncingWidths = true;
            var leaves = ColumnHeaderTree?.SelectMany(n => n.GetLeaves()).ToList();

            for (int i = 0; i < Columns.Count; i++)
            {
                var col = Columns[i];
                var leaf = (leaves != null && i < leaves.Count) ? leaves[i] : null;

                double targetWidth = col.ActualWidth;
                if (targetWidth <= 0)
                {
                    if (col.Width.IsAbsolute) targetWidth = col.Width.Value;
                    else if (leaf != null && leaf.Width.IsAbsolute) targetWidth = leaf.Width.Value;
                    else targetWidth = (leaf != null) ? Math.Max(leaf.MinWidth, 80) : 80;
                }

                if (i < _stackedHeaderGrid.ColumnDefinitions.Count)
                    _stackedHeaderGrid.ColumnDefinitions[i].Width = new GridLength(targetWidth, GridUnitType.Pixel);

                if (_footerSummaryGrid != null && i < _footerSummaryGrid.ColumnDefinitions.Count)
                    _footerSummaryGrid.ColumnDefinitions[i].Width = new GridLength(targetWidth, GridUnitType.Pixel);
            }

            // Compute exact total from the ColumnDefinitions we just set — never from ActualWidth
            double totalWidth = _stackedHeaderGrid.ColumnDefinitions
                .Sum(cd => cd.Width.IsAbsolute ? cd.Width.Value : 0);

            if (totalWidth > 0)
            {
                _stackedHeaderGrid.Width = totalWidth;
                if (_footerSummaryGrid != null) _footerSummaryGrid.Width = totalWidth;
            }
        }
        finally
        {
            _isSyncingWidths = false;
        }
    }

    private static T? FindVisualChildByName<T>(DependencyObject parent, string name) where T : FrameworkElement
    {
        if (parent == null) return null;
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is T fe && fe.Name == name) return fe;
            var desc = FindVisualChildByName<T>(child, name);
            if (desc != null) return desc;
        }
        return null;
    }

    private static T? FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
    {
        if (parent == null) return null;
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is T t) return t;
            var desc = FindVisualChild<T>(child);
            if (desc != null) return desc;
        }
        return null;
    }
}
