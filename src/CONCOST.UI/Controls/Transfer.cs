using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace CONCOST.UI.Controls;

[TemplatePart(Name = "PART_SourceListBox", Type = typeof(ListBox))]
[TemplatePart(Name = "PART_TargetListBox", Type = typeof(ListBox))]
[TemplatePart(Name = "PART_ToTargetBtn", Type = typeof(Button))]
[TemplatePart(Name = "PART_ToSourceBtn", Type = typeof(Button))]
[TemplatePart(Name = "PART_AllToTargetBtn", Type = typeof(Button))]
[TemplatePart(Name = "PART_AllToSourceBtn", Type = typeof(Button))]
[TemplatePart(Name = "PART_SourceSearchBox", Type = typeof(TextBox))]
[TemplatePart(Name = "PART_TargetSearchBox", Type = typeof(TextBox))]
public class Transfer : Control
{
    private ListBox? _sourceListBox;
    private ListBox? _targetListBox;
    private Button? _toTargetBtn;
    private Button? _toSourceBtn;
    private Button? _allToTargetBtn;
    private Button? _allToSourceBtn;
    private TextBox? _sourceSearchBox;
    private TextBox? _targetSearchBox;

    private bool _isInternalTransfer = false;

    public ObservableCollection<object> SourceItems { get; } = new();
    public ObservableCollection<object> TargetItems { get; } = new();

    private ICollectionView? _sourceView;
    private ICollectionView? _targetView;

    static Transfer()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(Transfer), new FrameworkPropertyMetadata(typeof(Transfer)));
    }

    public Transfer()
    {
        _sourceView = CollectionViewSource.GetDefaultView(SourceItems);
        _targetView = CollectionViewSource.GetDefaultView(TargetItems);
    }

    #region Dependency Properties

    public static readonly DependencyProperty ItemsSourceProperty =
        DependencyProperty.Register(nameof(ItemsSource), typeof(IEnumerable), typeof(Transfer),
            new PropertyMetadata(null, OnItemsSourceChanged));

    public static readonly DependencyProperty TransferredItemsProperty =
        DependencyProperty.Register(nameof(TransferredItems), typeof(IList), typeof(Transfer),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnTransferredItemsChanged));

    public static readonly DependencyProperty SourceTitleProperty =
        DependencyProperty.Register(nameof(SourceTitle), typeof(string), typeof(Transfer),
            new PropertyMetadata("Available Items"));

    public static readonly DependencyProperty TargetTitleProperty =
        DependencyProperty.Register(nameof(TargetTitle), typeof(string), typeof(Transfer),
            new PropertyMetadata("Selected Items"));

    public static readonly DependencyProperty ItemTemplateProperty =
        DependencyProperty.Register(nameof(ItemTemplate), typeof(DataTemplate), typeof(Transfer),
            new PropertyMetadata(null));

    public static readonly DependencyProperty ListWidthProperty =
        DependencyProperty.Register(nameof(ListWidth), typeof(double), typeof(Transfer),
            new PropertyMetadata(260.0));

    public static readonly DependencyProperty ListHeightProperty =
        DependencyProperty.Register(nameof(ListHeight), typeof(double), typeof(Transfer),
            new PropertyMetadata(320.0));

    public static readonly DependencyProperty ShowSearchProperty =
        DependencyProperty.Register(nameof(ShowSearch), typeof(bool), typeof(Transfer),
            new PropertyMetadata(true));

    public static readonly DependencyProperty ShowSelectAllProperty =
        DependencyProperty.Register(nameof(ShowSelectAll), typeof(bool), typeof(Transfer),
            new PropertyMetadata(true));

    public IEnumerable? ItemsSource
    {
        get => (IEnumerable?)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public IList? TransferredItems
    {
        get => (IList?)GetValue(TransferredItemsProperty);
        set => SetValue(TransferredItemsProperty, value);
    }

    public string SourceTitle
    {
        get => (string)GetValue(SourceTitleProperty);
        set => SetValue(SourceTitleProperty, value);
    }

    public string TargetTitle
    {
        get => (string)GetValue(TargetTitleProperty);
        set => SetValue(TargetTitleProperty, value);
    }

    public DataTemplate? ItemTemplate
    {
        get => (DataTemplate?)GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }

    public double ListWidth
    {
        get => (double)GetValue(ListWidthProperty);
        set => SetValue(ListWidthProperty, value);
    }

    public double ListHeight
    {
        get => (double)GetValue(ListHeightProperty);
        set => SetValue(ListHeightProperty, value);
    }

    public bool ShowSearch
    {
        get => (bool)GetValue(ShowSearchProperty);
        set => SetValue(ShowSearchProperty, value);
    }

    public bool ShowSelectAll
    {
        get => (bool)GetValue(ShowSelectAllProperty);
        set => SetValue(ShowSelectAllProperty, value);
    }

    #endregion

    #region Routed Events

    public static readonly RoutedEvent TransferredEvent =
        EventManager.RegisterRoutedEvent(nameof(Transferred), RoutingStrategy.Bubble,
            typeof(RoutedEventHandler), typeof(Transfer));

    public event RoutedEventHandler Transferred
    {
        add => AddHandler(TransferredEvent, value);
        remove => RemoveHandler(TransferredEvent, value);
    }

    #endregion

    public override void OnApplyTemplate()
    {
        if (_toTargetBtn != null) _toTargetBtn.Click -= OnTransferToTargetClick;
        if (_toSourceBtn != null) _toSourceBtn.Click -= OnTransferToSourceClick;
        if (_allToTargetBtn != null) _allToTargetBtn.Click -= OnTransferAllToTargetClick;
        if (_allToSourceBtn != null) _allToSourceBtn.Click -= OnTransferAllToSourceClick;
        if (_sourceSearchBox != null) _sourceSearchBox.TextChanged -= OnSourceSearchTextChanged;
        if (_targetSearchBox != null) _targetSearchBox.TextChanged -= OnTargetSearchTextChanged;
        if (_sourceListBox != null)
        {
            _sourceListBox.SelectionChanged -= OnSourceListBoxSelectionChanged;
            _sourceListBox.MouseDoubleClick -= OnSourceListBoxMouseDoubleClick;
        }
        if (_targetListBox != null)
        {
            _targetListBox.SelectionChanged -= OnTargetListBoxSelectionChanged;
            _targetListBox.MouseDoubleClick -= OnTargetListBoxMouseDoubleClick;
        }

        base.OnApplyTemplate();

        _sourceListBox = GetTemplateChild("PART_SourceListBox") as ListBox;
        _targetListBox = GetTemplateChild("PART_TargetListBox") as ListBox;
        _toTargetBtn = GetTemplateChild("PART_ToTargetBtn") as Button;
        _toSourceBtn = GetTemplateChild("PART_ToSourceBtn") as Button;
        _allToTargetBtn = GetTemplateChild("PART_AllToTargetBtn") as Button;
        _allToSourceBtn = GetTemplateChild("PART_AllToSourceBtn") as Button;
        _sourceSearchBox = GetTemplateChild("PART_SourceSearchBox") as TextBox;
        _targetSearchBox = GetTemplateChild("PART_TargetSearchBox") as TextBox;

        if (_sourceListBox != null)
        {
            _sourceListBox.ItemsSource = SourceItems;
            _sourceListBox.SelectionChanged += OnSourceListBoxSelectionChanged;
            _sourceListBox.MouseDoubleClick += OnSourceListBoxMouseDoubleClick;
        }

        if (_targetListBox != null)
        {
            _targetListBox.ItemsSource = TargetItems;
            _targetListBox.SelectionChanged += OnTargetListBoxSelectionChanged;
            _targetListBox.MouseDoubleClick += OnTargetListBoxMouseDoubleClick;
        }

        if (_toTargetBtn != null) _toTargetBtn.Click += OnTransferToTargetClick;
        if (_toSourceBtn != null) _toSourceBtn.Click += OnTransferToSourceClick;
        if (_allToTargetBtn != null) _allToTargetBtn.Click += OnTransferAllToTargetClick;
        if (_allToSourceBtn != null) _allToSourceBtn.Click += OnTransferAllToSourceClick;

        if (_sourceSearchBox != null)
        {
            _sourceSearchBox.TextChanged += OnSourceSearchTextChanged;
        }

        if (_targetSearchBox != null)
        {
            _targetSearchBox.TextChanged += OnTargetSearchTextChanged;
        }

        PopulateItems();
        UpdateButtonStates();
    }

    private void OnSourceListBoxSelectionChanged(object sender, SelectionChangedEventArgs e) => UpdateButtonStates();
    private void OnTargetListBoxSelectionChanged(object sender, SelectionChangedEventArgs e) => UpdateButtonStates();

    private void OnSourceListBoxMouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
        {
            TransferToTarget();
        }
    }

    private void OnTargetListBoxMouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
        {
            TransferToSource();
        }
    }

    public void UpdateButtonStates()
    {
        if (_toTargetBtn != null)
        {
            _toTargetBtn.IsEnabled = _sourceListBox?.SelectedItems.Count > 0;
        }

        if (_toSourceBtn != null)
        {
            _toSourceBtn.IsEnabled = _targetListBox?.SelectedItems.Count > 0;
        }

        if (_allToTargetBtn != null)
        {
            _allToTargetBtn.IsEnabled = SourceItems.Count > 0;
        }

        if (_allToSourceBtn != null)
        {
            _allToSourceBtn.IsEnabled = TargetItems.Count > 0;
        }
    }

    private void OnTransferToTargetClick(object sender, RoutedEventArgs e) => TransferToTarget();
    private void OnTransferToSourceClick(object sender, RoutedEventArgs e) => TransferToSource();
    private void OnTransferAllToTargetClick(object sender, RoutedEventArgs e) => TransferAllToTarget();
    private void OnTransferAllToSourceClick(object sender, RoutedEventArgs e) => TransferAllToSource();

    private void OnSourceSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        string filter = _sourceSearchBox?.Text?.Trim() ?? string.Empty;
        if (_sourceView != null)
        {
            _sourceView.Filter = item => string.IsNullOrEmpty(filter) || (item?.ToString()?.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0);
        }
        UpdateButtonStates();
    }

    private void OnTargetSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        string filter = _targetSearchBox?.Text?.Trim() ?? string.Empty;
        if (_targetView != null)
        {
            _targetView.Filter = item => string.IsNullOrEmpty(filter) || (item?.ToString()?.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0);
        }
        UpdateButtonStates();
    }

    private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Transfer t)
        {
            t.PopulateItems();
        }
    }

    private static void OnTransferredItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Transfer t && !t._isInternalTransfer)
        {
            t.PopulateItems();
        }
    }

    private void PopulateItems()
    {
        SourceItems.Clear();
        TargetItems.Clear();

        if (ItemsSource == null) return;

        var transferredSet = new HashSet<object>();
        if (TransferredItems != null)
        {
            foreach (var ti in TransferredItems)
            {
                if (ti != null) transferredSet.Add(ti);
            }
        }

        foreach (var item in ItemsSource)
        {
            if (item == null) continue;
            if (transferredSet.Contains(item))
            {
                TargetItems.Add(item);
            }
            else
            {
                SourceItems.Add(item);
            }
        }

        UpdateButtonStates();
    }

    public void TransferToTarget()
    {
        if (_sourceListBox == null) return;
        var selected = _sourceListBox.SelectedItems.Cast<object>().ToList();
        if (selected.Count == 0) return;

        foreach (var item in selected)
        {
            SourceItems.Remove(item);
            TargetItems.Add(item);
        }

        _sourceView?.Refresh();
        _targetView?.Refresh();

        UpdateTransferredItemsBinding();
        UpdateButtonStates();
        RaiseEvent(new RoutedEventArgs(TransferredEvent, this));
    }

    public void TransferToSource()
    {
        if (_targetListBox == null) return;
        var selected = _targetListBox.SelectedItems.Cast<object>().ToList();
        if (selected.Count == 0) return;

        foreach (var item in selected)
        {
            TargetItems.Remove(item);
            SourceItems.Add(item);
        }

        _sourceView?.Refresh();
        _targetView?.Refresh();

        UpdateTransferredItemsBinding();
        UpdateButtonStates();
        RaiseEvent(new RoutedEventArgs(TransferredEvent, this));
    }

    public void TransferAllToTarget()
    {
        var all = SourceItems.ToList();
        if (all.Count == 0) return;

        foreach (var item in all)
        {
            SourceItems.Remove(item);
            TargetItems.Add(item);
        }

        _sourceView?.Refresh();
        _targetView?.Refresh();

        UpdateTransferredItemsBinding();
        UpdateButtonStates();
        RaiseEvent(new RoutedEventArgs(TransferredEvent, this));
    }

    public void TransferAllToSource()
    {
        var all = TargetItems.ToList();
        if (all.Count == 0) return;

        foreach (var item in all)
        {
            TargetItems.Remove(item);
            SourceItems.Add(item);
        }

        _sourceView?.Refresh();
        _targetView?.Refresh();

        UpdateTransferredItemsBinding();
        UpdateButtonStates();
        RaiseEvent(new RoutedEventArgs(TransferredEvent, this));
    }

    private void UpdateTransferredItemsBinding()
    {
        _isInternalTransfer = true;
        try
        {
            TransferredItems = TargetItems.ToList();
        }
        finally
        {
            _isInternalTransfer = false;
        }
    }
}
