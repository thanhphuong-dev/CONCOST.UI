using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace CONCOST.UI.Controls.Ribbon;

/// <summary>
/// A ribbon-sized drop-down list, e.g. <c>Font [ Calibri ▾ ]</c>.
/// <para>
/// A thin shell around a normal <see cref="ComboBox"/>: items, templates, keyboard navigation and
/// the drop-down popup all come from the base control. The label comes from
/// <see cref="HeaderedContentControl.Header"/>; leave it empty for a bare box.
/// </para>
/// </summary>
public class RibbonComboBox : RibbonItem
{
    public static readonly DependencyProperty ItemsSourceProperty =
        DependencyProperty.Register(nameof(ItemsSource), typeof(IEnumerable), typeof(RibbonComboBox),
            new FrameworkPropertyMetadata(null));

    public static readonly DependencyProperty SelectedItemProperty =
        DependencyProperty.Register(nameof(SelectedItem), typeof(object), typeof(RibbonComboBox),
            new FrameworkPropertyMetadata(null,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedItemChanged));

    public static readonly DependencyProperty SelectedIndexProperty =
        DependencyProperty.Register(nameof(SelectedIndex), typeof(int), typeof(RibbonComboBox),
            new FrameworkPropertyMetadata(-1, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnSelectedIndexChanged));

    public static readonly DependencyProperty DisplayMemberPathProperty =
        DependencyProperty.Register(nameof(DisplayMemberPath), typeof(string), typeof(RibbonComboBox),
            new FrameworkPropertyMetadata(string.Empty));

    public static readonly DependencyProperty ItemTemplateProperty =
        DependencyProperty.Register(nameof(ItemTemplate), typeof(DataTemplate), typeof(RibbonComboBox),
            new FrameworkPropertyMetadata(null));

    public static readonly DependencyProperty IsEditableProperty =
        DependencyProperty.Register(nameof(IsEditable), typeof(bool), typeof(RibbonComboBox),
            new FrameworkPropertyMetadata(false));

    public static readonly DependencyProperty MaxDropDownHeightProperty =
        DependencyProperty.Register(nameof(MaxDropDownHeight), typeof(double), typeof(RibbonComboBox),
            new FrameworkPropertyMetadata(320.0));

    static RibbonComboBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(RibbonComboBox),
            new FrameworkPropertyMetadata(typeof(RibbonComboBox)));
    }

    public RibbonComboBox()
    {
        // The inner ComboBox is the tab stop.
        Focusable = false;
        IsTabStop = false;
    }

    /// <summary>Choices offered by the drop-down.</summary>
    public IEnumerable? ItemsSource
    {
        get => (IEnumerable?)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    /// <summary>Currently selected entry, or the typed text when <see cref="IsEditable"/> is set.</summary>
    public object? SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    /// <summary>Index of the selected entry, -1 when nothing is selected.</summary>
    public int SelectedIndex
    {
        get => (int)GetValue(SelectedIndexProperty);
        set => SetValue(SelectedIndexProperty, value);
    }

    /// <summary>Name of the property shown for each entry, when the entries are objects.</summary>
    public string DisplayMemberPath
    {
        get => (string)GetValue(DisplayMemberPathProperty);
        set => SetValue(DisplayMemberPathProperty, value);
    }

    /// <summary>Template used to render each entry in the list and in the closed box.</summary>
    public DataTemplate? ItemTemplate
    {
        get => (DataTemplate?)GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }

    /// <summary>Lets the user type a value that is not in the list.</summary>
    public bool IsEditable
    {
        get => (bool)GetValue(IsEditableProperty);
        set => SetValue(IsEditableProperty, value);
    }

    /// <summary>Height cap of the drop-down, before it starts scrolling.</summary>
    public double MaxDropDownHeight
    {
        get => (double)GetValue(MaxDropDownHeightProperty);
        set => SetValue(MaxDropDownHeightProperty, value);
    }

    /// <summary>The <see cref="ComboBox"/> inside the template, or null before it is applied.</summary>
    internal ComboBox? Editor => GetTemplateChild("Editor") as ComboBox;

    /// <summary>Focuses the box and drops the list open.</summary>
    public override void PerformClick()
    {
        if (!IsEnabled) return;

        if (Editor is { } editor)
        {
            editor.Focus();
            editor.IsDropDownOpen = true;
        }
    }

    protected override System.Windows.Automation.Peers.AutomationPeer OnCreateAutomationPeer() =>
        new RibbonComboBoxAutomationPeer(this);

    private bool _syncingSelection;

    private static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not RibbonComboBox combo || combo._syncingSelection) return;
        if (combo.ItemsSource == null) return;

        int index = 0;
        foreach (object? entry in combo.ItemsSource)
        {
            if (Equals(entry, e.NewValue))
            {
                combo.Sync(() =>
                {
                    if (combo.SelectedIndex != index) combo.SetCurrentValue(SelectedIndexProperty, index);
                });
                return;
            }

            index++;
        }

        combo.Sync(() =>
        {
            if (combo.SelectedIndex != -1) combo.SetCurrentValue(SelectedIndexProperty, -1);
        });
    }

    private static void OnSelectedIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not RibbonComboBox combo || combo._syncingSelection) return;
        if (combo.ItemsSource == null) return;

        int index = (int)e.NewValue;
        object? value = null;

        int current = 0;
        foreach (object? entry in combo.ItemsSource)
        {
            if (current++ != index) continue;
            value = entry;
            break;
        }

        combo.Sync(() =>
        {
            if (!Equals(combo.SelectedItem, value)) combo.SetCurrentValue(SelectedItemProperty, value);
        });
    }

    /// <summary>Runs a selection update without letting the two properties bounce off each other.</summary>
    private void Sync(Action update)
    {
        _syncingSelection = true;

        try
        {
            update();
        }
        finally
        {
            _syncingSelection = false;
        }
    }
}
