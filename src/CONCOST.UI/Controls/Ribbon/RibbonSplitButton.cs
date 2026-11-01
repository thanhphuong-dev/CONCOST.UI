using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CONCOST.UI.Controls.Ribbon;

/// <summary>
/// A button with two independent hit-test zones: the body runs <see cref="RibbonItem.Command"/>,
/// the arrow on the right opens the menu. This is the classic “Save / Save As” control.
/// </summary>
public class RibbonSplitButton : RibbonItem
{
    private readonly ObservableCollection<object> _items = new();

    public static readonly DependencyProperty IsOpenProperty =
        DependencyProperty.Register(nameof(IsOpen), typeof(bool), typeof(RibbonSplitButton),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public static readonly DependencyProperty ArrowWidthProperty =
        DependencyProperty.Register(nameof(ArrowWidth), typeof(double), typeof(RibbonSplitButton),
            new FrameworkPropertyMetadata(20.0, FrameworkPropertyMetadataOptions.AffectsMeasure));

    public static readonly DependencyProperty RememberLastProperty =
        DependencyProperty.Register(nameof(RememberLast), typeof(bool), typeof(RibbonSplitButton),
            new FrameworkPropertyMetadata(false));

    private static readonly DependencyPropertyKey LastUsedItemPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(LastUsedItem), typeof(RibbonMenuItem), typeof(RibbonSplitButton),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, OnLastUsedItemChanged));

    /// <summary>
    /// The menu entry the body of the button will run instead of <see cref="RibbonItem.Command"/>.
    /// Null while <see cref="RememberLast"/> is off or nothing has been picked yet.
    /// </summary>
    public static readonly DependencyProperty LastUsedItemProperty = LastUsedItemPropertyKey.DependencyProperty;

    private static readonly DependencyPropertyKey BodyHeaderPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(BodyHeader), typeof(object), typeof(RibbonSplitButton),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Caption the body of the button shows — the remembered entry's once one is picked.</summary>
    public static readonly DependencyProperty BodyHeaderProperty = BodyHeaderPropertyKey.DependencyProperty;

    private static readonly DependencyPropertyKey BodyIconPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(BodyIcon), typeof(System.Windows.Media.ImageSource), typeof(RibbonSplitButton),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Icon the body of the button shows — the remembered entry's once one is picked.</summary>
    public static readonly DependencyProperty BodyIconProperty = BodyIconPropertyKey.DependencyProperty;

    static RibbonSplitButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(RibbonSplitButton),
            new FrameworkPropertyMetadata(typeof(RibbonSplitButton)));
    }

    public RibbonSplitButton()
    {
        Cursor = Cursors.Hand;

        // Picking an entry from the menu has to be recorded before the menu closes, so the body can
        // run the same entry the next time it is pressed.
        AddHandler(RibbonMenuItem.ClickEvent, new RoutedEventHandler(OnMenuEntryClicked));
    }

    /// <summary>Raised when the main zone runs its command.</summary>
    public event RoutedEventHandler? Click;
    /// <summary>Menu content shown by the arrow zone.</summary>
    public ObservableCollection<object> Items => _items;

    /// <summary>Whether the dropdown is showing.</summary>
    public bool IsOpen
    {
        get => (bool)GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    /// <summary>Height of the arrow band under a large button's label, in pixels.</summary>
    private const double ArrowBand = 14;

    /// <summary>Width of the arrow zone, in pixels.</summary>
    public double ArrowWidth
    {
        get => (double)GetValue(ArrowWidthProperty);
        set => SetValue(ArrowWidthProperty, value);
    }

    /// <summary>
    /// When true, picking an entry from the menu turns the body into that entry — the “Save or
    /// Save As, then Save means Save As” behaviour of the ribbon.
    /// </summary>
    public bool RememberLast
    {
        get => (bool)GetValue(RememberLastProperty);
        set => SetValue(RememberLastProperty, value);
    }

    /// <summary>The entry the body currently stands for, or null while it means <see cref="RibbonItem.Command"/>.</summary>
    public RibbonMenuItem? LastUsedItem => (RibbonMenuItem?)GetValue(LastUsedItemProperty);

    /// <summary>Caption the body shows: the remembered entry's, or the button's own.</summary>
    public object? BodyHeader => GetValue(BodyHeaderProperty);

    /// <summary>Icon the body shows: the remembered entry's, or the button's own.</summary>
    public System.Windows.Media.ImageSource? BodyIcon => (System.Windows.Media.ImageSource?)GetValue(BodyIconProperty);

    /// <summary>Runs the primary zone; use <see cref="IsOpen"/> to reach the menu.</summary>
    public override void PerformClick()
    {
        if (!IsEnabled) return;

        Focus();
        RunBody();
        Click?.Invoke(this, new RoutedEventArgs());
    }

    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonUp(e);
        if (!IsEnabled) return;

        Point position = e.GetPosition(this);

        // Autodesk-style: a large split button keeps its arrow in the bottom-right corner, so the
        // label gets the full width; smaller ones use a strip along the right edge.
        // AutoCAD puts the triangle under the label on large buttons, so the arrow band is the
        // bottom strip there; small buttons keep the triangle on the right edge.
        bool onArrow = EffectiveSize == RibbonItemSize.Large
            ? position.Y >= ActualHeight - ArrowBand
            : position.X >= ActualWidth - ArrowWidth;

        Focus();

        if (onArrow)
        {
            IsOpen = !IsOpen;
        }
        else
        {
            RunBody();
            Click?.Invoke(this, new RoutedEventArgs());
        }

        e.Handled = true;
    }

    /// <summary>
    /// Runs whatever the body currently stands for. With <see cref="RememberLast"/> on and an entry
    /// already picked, that entry wins over the default command.
    /// </summary>
    private void RunBody()
    {
        RibbonMenuItem? remembered = LastUsedItem;

        if (remembered != null)
        {
            remembered.PerformClick();
            return;
        }

        InvokeCommand();
    }

    private void OnMenuEntryClicked(object sender, RoutedEventArgs e)
    {
        if (RememberLast && e.OriginalSource is RibbonMenuItem row && !row.HasItems)
            SetValue(LastUsedItemPropertyKey, row);

        IsOpen = false;
    }

    private static void OnLastUsedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not RibbonSplitButton button) return;

        var remembered = (RibbonMenuItem?)e.NewValue;

        button.SetValue(BodyHeaderPropertyKey, remembered?.Header ?? button.Header);
        button.SetValue(BodyIconPropertyKey, remembered?.Icon ?? button.EffectiveIcon);
    }

    protected override void OnInitialized(System.EventArgs e)
    {
        base.OnInitialized(e);

        // Header and Icon are usually set by the host after the constructor ran, and WPF does not
        // raise OnPropertyChanged for the default. Seed the body here so it always starts in sync.
        SetValue(BodyHeaderPropertyKey, Header);
        SetValue(BodyIconPropertyKey, EffectiveIcon);
    }

    protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        // Without a remembered entry the body follows the button itself, so its caption has to
        // travel on every change of Header or Icon.
        if (LastUsedItem == null &&
            (e.Property == HeaderProperty || e.Property == IconProperty ||
             e.Property == LargeIconProperty || e.Property == SmallIconProperty))
        {
            SetValue(BodyHeaderPropertyKey, Header);
            SetValue(BodyIconPropertyKey, EffectiveIcon);
        }
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (!IsEnabled) return;

        if (e.Key is Key.Down or Key.F4)
        {
            IsOpen = true;
            e.Handled = true;
        }
        else if (e.Key == Key.Escape && IsOpen)
        {
            IsOpen = false;
            e.Handled = true;
        }
        else if (e.Key is Key.Enter or Key.Space)
        {
            RunBody();
            Click?.Invoke(this, new RoutedEventArgs());
            e.Handled = true;
        }
    }
}
