using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CONCOST.UI.Controls;

/// <summary>Where a <see cref="CoachMark"/> prefers to sit relative to its target.</summary>
public enum CoachMarkPlacement
{
    /// <summary>Picks the first side with room, preferring below.</summary>
    Auto,
    Above,
    Below,
    Left,
    Right
}

/// <summary>
/// A first-run callout that dims the window and points at one control.
/// </summary>
/// <remarks>
/// <para>
/// The control fills an overlay panel and positions its own card, so a host puts it in the same
/// <see cref="Grid"/> as the content it describes:
/// <c>&lt;ui:CoachMark Target="{Binding ElementName=SaveButton}" /&gt;</c>. No popup, no adorner, and
/// nothing to keep in sync when the window moves — the card is re-laid out whenever the target or the
/// overlay changes size.
/// </para>
/// <para>
/// <see cref="Placement"/> is a preference, not a promise: a side that would push the card outside the
/// overlay is flipped, and <see cref="CardRect"/> reports where it actually landed so a host can draw
/// its own spotlight or test the geometry.
/// </para>
/// </remarks>
[TemplatePart(Name = CardPart, Type = typeof(FrameworkElement))]
[TemplatePart(Name = NextPart, Type = typeof(Button))]
[TemplatePart(Name = ClosePart, Type = typeof(Button))]
public class CoachMark : Control
{
    private const string CardPart = "PART_Card";
    private const string NextPart = "PART_Next";
    private const string ClosePart = "PART_Close";

    /// <summary>Identifies the <see cref="Target"/> property.</summary>
    public static readonly DependencyProperty TargetProperty = DependencyProperty.Register(
        nameof(Target), typeof(FrameworkElement), typeof(CoachMark),
        new PropertyMetadata(null, OnLayoutInputChanged));

    /// <summary>Identifies the <see cref="Placement"/> property.</summary>
    public static readonly DependencyProperty PlacementProperty = DependencyProperty.Register(
        nameof(Placement), typeof(CoachMarkPlacement), typeof(CoachMark),
        new PropertyMetadata(CoachMarkPlacement.Auto, OnLayoutInputChanged));

    /// <summary>Identifies the <see cref="Title"/> property.</summary>
    public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
        nameof(Title), typeof(string), typeof(CoachMark), new PropertyMetadata(string.Empty));

    /// <summary>Identifies the <see cref="StepIndex"/> property.</summary>
    public static readonly DependencyProperty StepIndexProperty = DependencyProperty.Register(
        nameof(StepIndex), typeof(int), typeof(CoachMark), new PropertyMetadata(1));

    /// <summary>Identifies the <see cref="StepCount"/> property.</summary>
    public static readonly DependencyProperty StepCountProperty = DependencyProperty.Register(
        nameof(StepCount), typeof(int), typeof(CoachMark), new PropertyMetadata(1));

    /// <summary>Identifies the <see cref="CardWidth"/> property.</summary>
    public static readonly DependencyProperty CardWidthProperty = DependencyProperty.Register(
        nameof(CardWidth), typeof(double), typeof(CoachMark), new PropertyMetadata(300d));

    /// <summary>Identifies the <see cref="Gap"/> property.</summary>
    public static readonly DependencyProperty GapProperty = DependencyProperty.Register(
        nameof(Gap), typeof(double), typeof(CoachMark), new PropertyMetadata(14d));

    /// <summary>Identifies the <see cref="OverlayBrush"/> property.</summary>
    public static readonly DependencyProperty OverlayBrushProperty = DependencyProperty.Register(
        nameof(OverlayBrush), typeof(Brush), typeof(CoachMark), new PropertyMetadata(null));

    private static readonly DependencyPropertyKey CardRectPropertyKey = DependencyProperty.RegisterReadOnly(
        nameof(CardRect), typeof(Rect), typeof(CoachMark), new PropertyMetadata(Rect.Empty));

    /// <summary>Identifies the <see cref="CardRect"/> property.</summary>
    public static readonly DependencyProperty CardRectProperty = CardRectPropertyKey.DependencyProperty;

    private FrameworkElement? _card;
    private Button? _next;
    private Button? _close;

    static CoachMark()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(CoachMark), new FrameworkPropertyMetadata(typeof(CoachMark)));
    }

    /// <summary>Raised by the card's Next button; the host advances the tour.</summary>
    public event EventHandler? Next;

    /// <summary>Raised when the card is dismissed.</summary>
    public event EventHandler? Dismissed;

    /// <summary>The element the card points at.</summary>
    public FrameworkElement? Target
    {
        get => (FrameworkElement?)GetValue(TargetProperty);
        set => SetValue(TargetProperty, value);
    }

    /// <summary>Preferred side of the target.</summary>
    public CoachMarkPlacement Placement
    {
        get => (CoachMarkPlacement)GetValue(PlacementProperty);
        set => SetValue(PlacementProperty, value);
    }

    /// <summary>Bold line at the top of the card.</summary>
    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>One-based step shown as "2 / 4" when <see cref="StepCount"/> is greater than one.</summary>
    public int StepIndex
    {
        get => (int)GetValue(StepIndexProperty);
        set => SetValue(StepIndexProperty, value);
    }

    /// <summary>Total number of steps in the tour.</summary>
    public int StepCount
    {
        get => (int)GetValue(StepCountProperty);
        set => SetValue(StepCountProperty, value);
    }

    /// <summary>Fixed card width.</summary>
    public double CardWidth
    {
        get => (double)GetValue(CardWidthProperty);
        set => SetValue(CardWidthProperty, value);
    }

    /// <summary>Distance between the target and the card.</summary>
    public double Gap
    {
        get => (double)GetValue(GapProperty);
        set => SetValue(GapProperty, value);
    }

    /// <summary>Colour of the dimming layer; a dark translucent brush by default.</summary>
    public Brush? OverlayBrush
    {
        get => (Brush?)GetValue(OverlayBrushProperty);
        set => SetValue(OverlayBrushProperty, value);
    }

    /// <summary>Where the card was placed by the last arrange pass.</summary>
    public Rect CardRect => (Rect)GetValue(CardRectProperty);

    /// <summary>True when the tour is on its last step.</summary>
    public bool IsLastStep => StepCount <= 1 || StepIndex >= StepCount;

    /// <summary>Raises <see cref="Next"/>; the host decides whether there is another step.</summary>
    public void Advance() => Next?.Invoke(this, EventArgs.Empty);

    /// <summary>Hides the callout and raises <see cref="Dismissed"/>, once.</summary>
    public void Dismiss()
    {
        // Showing the next step sets Visibility back to Visible, so the flag lives in the state the
        // host can already see rather than in a field that would have to be reset by the host.
        if (Visibility == Visibility.Collapsed) return;

        Visibility = Visibility.Collapsed;
        Dismissed?.Invoke(this, EventArgs.Empty);
    }

    private static void OnLayoutInputChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not CoachMark mark) return;

        mark.InvalidateMeasure();
        mark.InvalidateArrange();
    }

    public override void OnApplyTemplate()
    {
        if (_next is not null) _next.Click -= OnNextClick;
        if (_close is not null) _close.Click -= OnCloseClick;

        base.OnApplyTemplate();

        _card = GetTemplateChild(CardPart) as FrameworkElement;

        _next = GetTemplateChild(NextPart) as Button;
        if (_next is not null) _next.Click += OnNextClick;

        _close = GetTemplateChild(ClosePart) as Button;
        if (_close is not null) _close.Click += OnCloseClick;

        InvalidateMeasure();
    }

    private void OnNextClick(object sender, RoutedEventArgs e) => Advance();

    private void OnCloseClick(object sender, RoutedEventArgs e) => Dismiss();

    /// <summary>Where the card would land for a target, in overlay coordinates.</summary>
    public Rect ComputeCardRect(Size host)
    {
        if (Target is null || host.Width <= 0 || host.Height <= 0) return Rect.Empty;

        Point corner;

        try
        {
            if (!IsAncestorOf(Target) && Target.FindCommonVisualAncestor(this) is null) return Rect.Empty;
            corner = Target.TranslatePoint(new Point(0, 0), this);
        }
        catch (InvalidOperationException)
        {
            // The target lives in another visual tree (a popup, another window), so there is no
            // meaningful point to point at.
            return Rect.Empty;
        }

        var bounds = new Rect(corner, Target.RenderSize);
        double width = Math.Min(CardWidth, Math.Max(120, host.Width - 2 * Gap));
        double height = _card?.DesiredSize.Height ?? 140;

        var placement = Placement == CoachMarkPlacement.Auto ? PickSide(bounds, host, width, height) : Placement;

        double x;
        double y;

        switch (placement)
        {
            case CoachMarkPlacement.Above:
                x = bounds.X + bounds.Width / 2 - width / 2;
                y = bounds.Y - height - Gap;
                break;

            case CoachMarkPlacement.Left:
                x = bounds.X - width - Gap;
                y = bounds.Y + bounds.Height / 2 - height / 2;
                break;

            case CoachMarkPlacement.Right:
                x = bounds.Right + Gap;
                y = bounds.Y + bounds.Height / 2 - height / 2;
                break;

            default:
                x = bounds.X + bounds.Width / 2 - width / 2;
                y = bounds.Bottom + Gap;
                break;
        }

        x = Math.Max(Gap / 2, Math.Min(host.Width - width - Gap / 2, x));
        y = Math.Max(Gap / 2, Math.Min(host.Height - height - Gap / 2, y));

        return new Rect(x, y, width, height);
    }

    private CoachMarkPlacement PickSide(Rect target, Size host, double width, double height)
    {
        if (target.Bottom + Gap + height <= host.Height) return CoachMarkPlacement.Below;
        if (target.Y - Gap - height >= 0) return CoachMarkPlacement.Above;
        if (target.Right + Gap + width <= host.Width) return CoachMarkPlacement.Right;
        if (target.X - Gap - width >= 0) return CoachMarkPlacement.Left;

        return CoachMarkPlacement.Below;
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        double width = double.IsInfinity(availableSize.Width) ? 0 : availableSize.Width;
        double height = double.IsInfinity(availableSize.Height) ? 0 : availableSize.Height;

        if (_card is not null) _card.Measure(new Size(Math.Min(CardWidth, Math.Max(120, width)), double.PositiveInfinity));

        return new Size(width, height);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var rect = ComputeCardRect(finalSize);

        if (_card is not null)
        {
            if (rect.IsEmpty)
            {
                _card.Arrange(new Rect(finalSize));
            }
            else
            {
                // Canvas.Left/Top is a layout-time hint, so setting it here cannot start another pass.
                System.Windows.Controls.Canvas.SetLeft(_card, rect.X);
                System.Windows.Controls.Canvas.SetTop(_card, rect.Y);
                _card.Width = rect.Width;
                _card.Arrange(new Rect(rect.X, rect.Y, rect.Width, rect.Height));
            }
        }

        SetValue(CardRectPropertyKey, rect);
        return finalSize;
    }
}
