using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CONCOST.UI.Controls.Ribbon;

/// <summary>
/// One tab of a <see cref="Ribbon"/>. Its <see cref="ContentControl.Content"/> is the collection of
/// <see cref="RibbonGroup"/> instances the ribbon shows when this tab is selected.
/// </summary>
public class RibbonTabItem : HeaderedContentControl
{
    public static readonly DependencyProperty IconProperty =
        DependencyProperty.Register(nameof(Icon), typeof(ImageSource), typeof(RibbonTabItem),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure));

    public static readonly DependencyProperty IsSelectedProperty =
        DependencyProperty.Register(nameof(IsSelected), typeof(bool), typeof(RibbonTabItem),
            new FrameworkPropertyMetadata(false));

    public static readonly DependencyProperty IsContextualProperty =
        DependencyProperty.Register(nameof(IsContextual), typeof(bool), typeof(RibbonTabItem),
            new FrameworkPropertyMetadata(false));

    public static readonly DependencyProperty ContextualColorProperty =
        DependencyProperty.Register(nameof(ContextualColor), typeof(Brush), typeof(RibbonTabItem),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsParentMeasure));

    public static readonly DependencyProperty ContextualGroupProperty =
        DependencyProperty.Register(nameof(ContextualGroup), typeof(string), typeof(RibbonTabItem),
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.AffectsParentMeasure));

    public static readonly DependencyProperty IsAvailableProperty =
        DependencyProperty.Register(nameof(IsAvailable), typeof(bool), typeof(RibbonTabItem),
            new FrameworkPropertyMetadata(true));

    /// <summary>Same property as <see cref="KeyTipService.KeyTipProperty"/>, so both spellings work.</summary>
    public static readonly DependencyProperty KeyTipProperty = KeyTipService.KeyTipProperty;

    public static readonly DependencyProperty IdProperty =
        DependencyProperty.Register(nameof(Id), typeof(string), typeof(RibbonTabItem),
            new FrameworkPropertyMetadata(string.Empty));

    static RibbonTabItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(RibbonTabItem),
            new FrameworkPropertyMetadata(typeof(RibbonTabItem)));
    }

    /// <summary>Optional icon shown before the header text.</summary>
    public ImageSource? Icon
    {
        get => (ImageSource?)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>True when this tab is the active one.</summary>
    public bool IsSelected
    {
        get => (bool)GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

    /// <summary>Marks a tab that only appears for a mode (Modify, Edit, Sketch…).</summary>
    public bool IsContextual
    {
        get => (bool)GetValue(IsContextualProperty);
        set => SetValue(IsContextualProperty, value);
    }

    /// <summary>Accent colour of a contextual tab header.</summary>
    public Brush? ContextualColor
    {
        get => (Brush?)GetValue(ContextualColorProperty);
        set => SetValue(ContextualColorProperty, value);
    }

    /// <summary>
    /// Caption of the coloured band drawn above the tabs — “Drawing Tools”, “Table Tools”…
    /// Consecutive tabs that share the same value are drawn under one merged band.
    /// Leave empty for an ordinary tab.
    /// </summary>
    public string ContextualGroup
    {
        get => (string)GetValue(ContextualGroupProperty);
        set => SetValue(ContextualGroupProperty, value);
    }

    /// <summary>
    /// False hides the tab from the strip without removing it from the item source. This is how a
    /// contextual group appears and disappears with the current mode.
    /// </summary>
    public bool IsAvailable
    {
        get => (bool)GetValue(IsAvailableProperty);
        set => SetValue(IsAvailableProperty, value);
    }

    /// <summary>Raised when <see cref="IsAvailable"/> changes, so the owning ribbon can re-filter.</summary>
    public event EventHandler? IsAvailableChanged;

    protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.Property == IsAvailableProperty) IsAvailableChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Access key shown by the KeyTip layer.</summary>
    public string KeyTip
    {
        get => (string)GetValue(KeyTipProperty);
        set => SetValue(KeyTipProperty, value);
    }

    /// <summary>
    /// Stable identifier used when the ribbon state is saved and restored. Leave it empty to fall
    /// back on <see cref="HeaderedContentControl.Header"/>, which is fine as long as headers are
    /// unique and never translated at runtime.
    /// </summary>
    public string Id
    {
        get => (string)GetValue(IdProperty);
        set => SetValue(IdProperty, value);
    }
}
