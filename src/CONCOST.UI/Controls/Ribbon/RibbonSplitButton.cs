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

    static RibbonSplitButton()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(RibbonSplitButton),
            new FrameworkPropertyMetadata(typeof(RibbonSplitButton)));
    }

    public RibbonSplitButton()
    {
        Cursor = Cursors.Hand;
        AddHandler(RibbonMenuItem.ClickEvent, new RoutedEventHandler((_, _) => IsOpen = false));
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

    /// <summary>Runs the primary zone; use <see cref="IsOpen"/> to reach the menu.</summary>
    public override void PerformClick()
    {
        if (!IsEnabled) return;

        Focus();
        if (Command != null && Command.CanExecute(CommandParameter)) Command.Execute(CommandParameter);
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
            if (Command != null && Command.CanExecute(CommandParameter)) Command.Execute(CommandParameter);
            Click?.Invoke(this, new RoutedEventArgs());
        }

        e.Handled = true;
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
            if (Command != null && Command.CanExecute(CommandParameter)) Command.Execute(CommandParameter);
            Click?.Invoke(this, new RoutedEventArgs());
            e.Handled = true;
        }
    }
}
