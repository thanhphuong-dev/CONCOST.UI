using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using System.Windows.Threading;
using CONCOST.UI.AttachedProperties;

namespace CONCOST.UI.Controls;

/// <summary>What a <see cref="DynamicInput"/> box is asking for.</summary>
public enum DynamicInputKind
{
    /// <summary>A length in millimetres; accepts <c>mm</c>, <c>cm</c>, <c>m</c>, <c>in</c>, <c>ft</c> and <c>3'-6"</c>.</summary>
    Distance,

    /// <summary>An angle in degrees; accepts a trailing <c>°</c>.</summary>
    Angle,

    /// <summary>An <c>x,y</c> (optionally <c>x,y,z</c>) point in model units.</summary>
    Coordinate,

    /// <summary>Free text — nothing is parsed and <see cref="DynamicInputValueEventArgs.Number"/> stays zero.</summary>
    Text
}

/// <summary>How a <see cref="DynamicInput"/> box is dressed.</summary>
public enum DynamicInputVariant
{
    /// <summary>A plate with one outline, a shadow and a leader back to the point it hangs off.</summary>
    Floating,

    /// <summary>One row — the question, the field and the unit share a line — for a status strip.</summary>
    Compact,

    /// <summary>A 94 % plate that keeps its shadow, so a busy drawing still reads through it.</summary>
    Glass
}

/// <summary>Where the box sits relative to the point it hangs off.</summary>
public enum DynamicInputPlacement
{
    /// <summary>Right and below, flipping each axis on its own when the box would leave its parent.</summary>
    Auto,

    /// <summary>Right and below, even at the edge of the viewport.</summary>
    Right,

    /// <summary>Left and below, for a point near the right edge.</summary>
    Left,

    /// <summary>Right and above, for a point near the bottom.</summary>
    Above
}

/// <summary>The value a <see cref="DynamicInput"/> accepted.</summary>
public sealed class DynamicInputValueEventArgs : EventArgs
{
    internal DynamicInputValueEventArgs(DynamicInputKind kind, string text, double number, Point3D? point)
    {
        Kind = kind;
        Text = text ?? string.Empty;
        Number = number;
        Point = point;
    }

    /// <summary>What the box was asking for.</summary>
    public DynamicInputKind Kind { get; }

    /// <summary>The text exactly as typed, so a host can echo it into the command history.</summary>
    public string Text { get; }

    /// <summary>Millimetres for <see cref="DynamicInputKind.Distance"/>, degrees for <see cref="DynamicInputKind.Angle"/>.</summary>
    public double Number { get; }

    /// <summary>The point for <see cref="DynamicInputKind.Coordinate"/>; <c>null</c> for the other kinds.</summary>
    public Point3D? Point { get; }
}

/// <summary>
/// The input box that follows the cursor while a drawing command is running, so a length or a
/// coordinate can be typed where the user is looking instead of at the bottom of the window.
/// </summary>
/// <remarks>
/// <para>
/// Drop it into the overlay of a <see cref="Navigation.ViewportControl"/> and call
/// <see cref="Open"/> with the current cursor position: the box places itself next to that point and
/// moves with it. <c>Enter</c> commits, <c>Esc</c> cancels, and an unreadable value keeps the box
/// open with the reason underneath rather than closing on a typo.
/// </para>
/// <para>
/// Parsing goes through <see cref="CadInputParser"/>, the same helper the command line uses, so
/// <c>3.2m</c> means 3200 mm in both places. <see cref="TryParse"/> is public and pure, which is what
/// makes the accepted syntax testable without a window.
/// </para>
/// </remarks>
public class DynamicInput : Grid
{
    private static readonly FontFamily MonoFont = new("Consolas");

    private readonly Border _frame;
    private readonly Grid _layout;
    private readonly TextBlock _label;
    private readonly TextBlock _unit;
    private readonly TextBox _box;
    private readonly TextBlock _hint;
    private readonly Line _leader;

    private bool _toLeft;
    private bool _above;

    /// <summary>Creates a closed input box.</summary>
    public DynamicInput()
    {
        HorizontalAlignment = HorizontalAlignment.Left;
        VerticalAlignment = VerticalAlignment.Top;
        Visibility = Visibility.Collapsed;

        _label = new TextBlock
        {
            FontSize = 11,
            VerticalAlignment = VerticalAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis
        };
        _label.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");
        _label.SetResourceReference(TextBlock.FontFamilyProperty, "CONCOST.FontFamily.Default");

        // The unit belongs to the value, not to the question: it sits at the end of the first row in
        // tabular type, so "3200" and "(mm)" line up down a column of boxes.
        _unit = new TextBlock
        {
            FontFamily = MonoFont,
            FontSize = 10.5,
            Opacity = 0.75,
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(10, 0, 0, 0)
        };
        _unit.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");

        // The underline style is the point: a boxed TextBox inside a bordered plate is two outlines
        // around one value, and that is exactly what made the old box read as a form on a drawing.
        _box = new TextBox
        {
            FontFamily = MonoFont,
            FontSize = 13,
            MinHeight = 0,
            Height = 24,
            Padding = new Thickness(0, 0, 0, 1),
            VerticalContentAlignment = VerticalAlignment.Center,
            FontWeight = FontWeights.SemiBold
        };
        _box.SetResourceReference(StyleProperty, "CONCOST.TextBox.Underline");

        _box.SetBinding(TextBox.TextProperty, new Binding(nameof(Text))
        {
            Source = this,
            Mode = BindingMode.TwoWay,
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
        });

        _box.TextChanged += (_, _) =>
        {
            // A new keystroke retires the last complaint: the box must not keep shouting after a fix.
            ClearError();
        };

        _box.PreviewKeyDown += OnBoxPreviewKeyDown;

        _hint = new TextBlock
        {
            FontSize = 10.5,
            Margin = new Thickness(0, 4, 0, 0),
            TextWrapping = TextWrapping.Wrap,
            Visibility = Visibility.Collapsed
        };
        _hint.SetResourceReference(TextBlock.FontFamilyProperty, "CONCOST.FontFamily.Default");

        _layout = new Grid();
        _layout.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        _layout.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        _layout.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        _layout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        _layout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        _layout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        _layout.Children.Add(_label);
        _layout.Children.Add(_unit);
        _layout.Children.Add(_box);
        _layout.Children.Add(_hint);

        _frame = new Border { Child = _layout };
        _frame.SetResourceReference(Border.BackgroundProperty, "CONCOST.Brush.Surface");
        _frame.SetResourceReference(Border.BorderBrushProperty, "CONCOST.Brush.Border");

        // The leader is what ties the plate to the point it belongs to; without it the gap between
        // a box and a cursor reads as a box floating nowhere.
        _leader = new Line
        {
            StrokeThickness = 1,
            StrokeDashArray = new DoubleCollection { 2, 2 },
            Opacity = 0.6,
            IsHitTestVisible = false,
            Visibility = Visibility.Collapsed
        };
        _leader.SetResourceReference(Shape.StrokeProperty, "CONCOST.Brush.Primary");

        // Qualified: CONCOST.UI.Controls.Canvas is the canvas control family, not this one.
        var leaderHost = new System.Windows.Controls.Canvas { IsHitTestVisible = false };
        leaderHost.Children.Add(_leader);

        Children.Add(leaderHost);
        Children.Add(_frame);

        // The frame is measured after the first layout pass, which is when a box near an edge
        // learns whether it has to flip. A margin change does not resize it, so this cannot loop.
        _frame.SizeChanged += (_, _) => ApplyPlacement();

        SyncLabel();
        RefreshChrome();
        ApplyFieldWidth();
        ApplyPlacement();
        SyncVisibility();
    }

    /// <summary>Raised when the typed value parsed. The box has already closed.</summary>
    public event EventHandler<DynamicInputValueEventArgs>? ValueCommitted;

    /// <summary>Raised when the input was abandoned through <see cref="Cancel"/> or <c>Esc</c>.</summary>
    public event EventHandler? Cancelled;

    /// <summary>The question shown above the box, e.g. <c>"Length"</c>.</summary>
    public static readonly DependencyProperty PromptProperty = DependencyProperty.Register(
        nameof(Prompt), typeof(string), typeof(DynamicInput),
        new FrameworkPropertyMetadata(string.Empty, OnPromptChanged));

    /// <summary>The typed text (two-way, so a host can pre-fill a value).</summary>
    public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
        nameof(Text), typeof(string), typeof(DynamicInput),
        new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    /// <summary>The point, in the coordinates of the host panel, the box hangs off.</summary>
    public static readonly DependencyProperty AnchorProperty = DependencyProperty.Register(
        nameof(Anchor), typeof(Point), typeof(DynamicInput),
        new FrameworkPropertyMetadata(new Point(0, 0), OnPlacementChanged));

    /// <summary>Gap in pixels between <see cref="Anchor"/> and the box. 18 by default.</summary>
    public static readonly DependencyProperty OffsetProperty = DependencyProperty.Register(
        nameof(Offset), typeof(double), typeof(DynamicInput),
        new FrameworkPropertyMetadata(18.0, OnPlacementChanged));

    /// <summary>What the box is asking for.</summary>
    public static readonly DependencyProperty KindProperty = DependencyProperty.Register(
        nameof(Kind), typeof(DynamicInputKind), typeof(DynamicInput),
        new FrameworkPropertyMetadata(DynamicInputKind.Distance, OnPromptChanged));

    private static readonly DependencyPropertyKey IsValidPropertyKey = DependencyProperty.RegisterReadOnly(
        nameof(IsValid), typeof(bool), typeof(DynamicInput), new PropertyMetadata(true));

    /// <summary>Whether the current text is readable; true while the box is empty.</summary>
    public static readonly DependencyProperty IsValidProperty = IsValidPropertyKey.DependencyProperty;

    private static readonly DependencyPropertyKey ValidationMessagePropertyKey = DependencyProperty.RegisterReadOnly(
        nameof(ValidationMessage), typeof(string), typeof(DynamicInput), new PropertyMetadata(string.Empty));

    /// <summary>The reason the last commit was refused, or an empty string.</summary>
    public static readonly DependencyProperty ValidationMessageProperty = ValidationMessagePropertyKey.DependencyProperty;

    /// <summary>Whether the box is on screen and taking typing.</summary>
    public static readonly DependencyProperty IsOpenProperty = DependencyProperty.Register(
        nameof(IsOpen), typeof(bool), typeof(DynamicInput),
        new FrameworkPropertyMetadata(false, OnOpenChanged));

    /// <summary>How the box is dressed.</summary>
    public static readonly DependencyProperty VariantProperty = DependencyProperty.Register(
        nameof(Variant), typeof(DynamicInputVariant), typeof(DynamicInput),
        new FrameworkPropertyMetadata(DynamicInputVariant.Floating, OnChromeChanged));

    /// <summary>Where the box sits relative to its anchor.</summary>
    public static readonly DependencyProperty PlacementProperty = DependencyProperty.Register(
        nameof(Placement), typeof(DynamicInputPlacement), typeof(DynamicInput),
        new FrameworkPropertyMetadata(DynamicInputPlacement.Auto, OnPlacementChanged));

    /// <summary>Corner radius of the plate. 6 by default; ignored by <c>Compact</c>, which is a strip.</summary>
    public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register(
        nameof(CornerRadius), typeof(CornerRadius), typeof(DynamicInput),
        new FrameworkPropertyMetadata(new CornerRadius(6), OnChromeChanged));

    /// <summary>Depth of the plate's shadow, 0-4. <c>Compact</c> is flat and ignores it.</summary>
    public static readonly DependencyProperty ElevationProperty = DependencyProperty.Register(
        nameof(Elevation), typeof(int), typeof(DynamicInput),
        new FrameworkPropertyMetadata(1, OnChromeChanged));

    /// <summary>
    /// What the value is measured in. Empty means the unit of <see cref="Kind"/> — <c>(mm)</c> for a
    /// length, <c>(°)</c> for an angle, nothing for a point — so a drawing in inches can say <c>(in)</c>.
    /// </summary>
    public static readonly DependencyProperty UnitSuffixProperty = DependencyProperty.Register(
        nameof(UnitSuffix), typeof(string), typeof(DynamicInput),
        new FrameworkPropertyMetadata(string.Empty, OnPromptChanged));

    /// <summary>Whether the unit is printed at the end of the question row.</summary>
    public static readonly DependencyProperty ShowUnitProperty = DependencyProperty.Register(
        nameof(ShowUnit), typeof(bool), typeof(DynamicInput),
        new FrameworkPropertyMetadata(true, OnPromptChanged));

    /// <summary>Whether a dashed leader runs from the plate back to <see cref="Anchor"/>.</summary>
    public static readonly DependencyProperty ShowLeaderProperty = DependencyProperty.Register(
        nameof(ShowLeader), typeof(bool), typeof(DynamicInput),
        new FrameworkPropertyMetadata(true, OnChromeChanged));

    /// <summary>Width of the value field in pixels; the coordinate box needs more room than a length.</summary>
    public static readonly DependencyProperty FieldWidthProperty = DependencyProperty.Register(
        nameof(FieldWidth), typeof(double), typeof(DynamicInput),
        new FrameworkPropertyMetadata(120.0, OnFieldWidthChanged));

    /// <summary>
    /// Whether opening the box selects what is already in it. On by default: a CAD prompt starts
    /// with the current measurement, and typing over it must not append to it.
    /// </summary>
    public static readonly DependencyProperty SelectAllOnOpenProperty = DependencyProperty.Register(
        nameof(SelectAllOnOpen), typeof(bool), typeof(DynamicInput),
        new FrameworkPropertyMetadata(true));

    /// <summary>The question shown above the box.</summary>
    public string Prompt
    {
        get => (string)GetValue(PromptProperty);
        set => SetValue(PromptProperty, value);
    }

    /// <summary>The typed text.</summary>
    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    /// <summary>The point the box hangs off.</summary>
    public Point Anchor
    {
        get => (Point)GetValue(AnchorProperty);
        set => SetValue(AnchorProperty, value);
    }

    /// <summary>Gap in pixels between <see cref="Anchor"/> and the box.</summary>
    public double Offset
    {
        get => (double)GetValue(OffsetProperty);
        set => SetValue(OffsetProperty, value);
    }

    /// <summary>What the box is asking for.</summary>
    public DynamicInputKind Kind
    {
        get => (DynamicInputKind)GetValue(KindProperty);
        set => SetValue(KindProperty, value);
    }

    /// <summary>Whether the current text is readable.</summary>
    public bool IsValid => (bool)GetValue(IsValidProperty);

    /// <summary>The reason the last commit was refused.</summary>
    public string ValidationMessage => (string)GetValue(ValidationMessageProperty);

    /// <summary>Whether the box is on screen.</summary>
    public bool IsOpen
    {
        get => (bool)GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    /// <summary>How the box is dressed.</summary>
    public DynamicInputVariant Variant
    {
        get => (DynamicInputVariant)GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }

    /// <summary>Where the box sits relative to its anchor.</summary>
    public DynamicInputPlacement Placement
    {
        get => (DynamicInputPlacement)GetValue(PlacementProperty);
        set => SetValue(PlacementProperty, value);
    }

    /// <summary>Corner radius of the plate.</summary>
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    /// <summary>Depth of the plate's shadow, 0-4.</summary>
    public int Elevation
    {
        get => (int)GetValue(ElevationProperty);
        set => SetValue(ElevationProperty, value);
    }

    /// <summary>What the value is measured in; empty means the unit of <see cref="Kind"/>.</summary>
    public string UnitSuffix
    {
        get => (string)GetValue(UnitSuffixProperty);
        set => SetValue(UnitSuffixProperty, value);
    }

    /// <summary>Whether the unit is printed beside the question.</summary>
    public bool ShowUnit
    {
        get => (bool)GetValue(ShowUnitProperty);
        set => SetValue(ShowUnitProperty, value);
    }

    /// <summary>Whether a dashed leader runs from the plate back to the anchor.</summary>
    public bool ShowLeader
    {
        get => (bool)GetValue(ShowLeaderProperty);
        set => SetValue(ShowLeaderProperty, value);
    }

    /// <summary>Width of the value field in pixels.</summary>
    public double FieldWidth
    {
        get => (double)GetValue(FieldWidthProperty);
        set => SetValue(FieldWidthProperty, value);
    }

    /// <summary>Whether opening the box selects what is already in it.</summary>
    public bool SelectAllOnOpen
    {
        get => (bool)GetValue(SelectAllOnOpenProperty);
        set => SetValue(SelectAllOnOpenProperty, value);
    }

    /// <summary>
    /// Opens the box next to <paramref name="anchor"/>. Pass a prompt to change the question, a
    /// prefill to start from the current measurement, or nothing to keep what the box had.
    /// </summary>
    public void Open(Point anchor, string? prompt = null, string? prefill = null)
    {
        Anchor = anchor;

        if (prompt is not null) Prompt = prompt;

        SetCurrentValue(TextProperty, prefill ?? string.Empty);
        ClearError();
        IsOpen = true;

        // Focus has to wait for the box to be visible, or WPF drops it.
        Dispatcher.BeginInvoke(DispatcherPriority.Input, new Action(() =>
        {
            if (!IsOpen) return;

            _box.Focus();

            if (SelectAllOnOpen) _box.SelectAll();
            else _box.CaretIndex = _box.Text.Length;
        }));
    }

    /// <summary>Moves the open box without touching what has been typed.</summary>
    public void MoveTo(Point anchor) => Anchor = anchor;

    /// <summary>
    /// Parses the text and closes the box. Returns false and writes the reason into
    /// <see cref="ValidationMessage"/> when the text does not read as the requested kind.
    /// </summary>
    public bool Commit()
    {
        if (!TryParse(Kind, Text, out double number, out Point3D point))
        {
            Fail(Kind == DynamicInputKind.Text ? "A value is required." : $"Cannot read “{Text}”.");
            return false;
        }

        var args = new DynamicInputValueEventArgs(Kind, Text, number,
            Kind == DynamicInputKind.Coordinate ? point : null);

        IsOpen = false;
        ValueCommitted?.Invoke(this, args);

        return true;
    }

    /// <summary>Abandons the input: clears the text, closes the box and raises <see cref="Cancelled"/>.</summary>
    public void Cancel()
    {
        bool wasOpen = IsOpen;

        SetCurrentValue(TextProperty, string.Empty);
        ClearError();
        IsOpen = false;

        if (wasOpen) Cancelled?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Reads <paramref name="text"/> as <paramref name="kind"/>. Pure, so the accepted syntax can be
    /// tested without a window.
    /// </summary>
    public static bool TryParse(DynamicInputKind kind, string? text, out double number, out Point3D point)
    {
        number = 0;
        point = default;

        switch (kind)
        {
            case DynamicInputKind.Text:
                // Any non-empty text is a value; an empty box is still a missing answer.
                return !string.IsNullOrWhiteSpace(text);

            case DynamicInputKind.Angle:
                return CadInputParser.TryParseAngle(text, out number);

            case DynamicInputKind.Coordinate:
                if (!CadInputParser.TryParseCoordinate(text, out point)) return false;

                number = point.X;
                return true;

            default:
                return CadInputParser.TryParseLength(text, out number);
        }
    }

    private void Fail(string message)
    {
        SetValue(ValidationMessagePropertyKey, message);
        SetValue(IsValidPropertyKey, false);
        SyncHint();
        ApplyErrorChrome();
    }

    private void ClearError()
    {
        if (ValidationMessage.Length == 0 && IsValid) return;

        SetValue(ValidationMessagePropertyKey, string.Empty);
        SetValue(IsValidPropertyKey, true);
        SyncHint();
        ApplyErrorChrome();
    }

    private void OnBoxPreviewKeyDown(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Enter:
                e.Handled = true;
                Commit();
                break;

            case Key.Escape:
                e.Handled = true;
                Cancel();
                break;
        }
    }

    private void SyncLabel()
    {
        if (_label is null) return;

        _label.Text = string.IsNullOrWhiteSpace(Prompt) ? "Value" : Prompt;

        SyncUnit();
    }

    private void SyncUnit()
    {
        if (_unit is null) return;

        string suffix = UnitSuffix;

        if (string.IsNullOrWhiteSpace(suffix))
        {
            suffix = Kind switch
            {
                DynamicInputKind.Distance => "(mm)",
                DynamicInputKind.Angle => "(°)",
                _ => string.Empty
            };
        }

        _unit.Text = suffix;
        _unit.Visibility = ShowUnit && suffix.Length > 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    private void SyncHint()
    {
        if (_hint is null) return;

        string message = ValidationMessage;

        _hint.Text = message;
        _hint.Visibility = message.Length == 0 ? Visibility.Collapsed : Visibility.Visible;

        if (message.Length == 0)
        {
            _hint.ClearValue(TextBlock.ForegroundProperty);
            return;
        }

        _hint.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.Danger");
    }

    /// <summary>Puts the plate, the question and the field back in step with the current properties.</summary>
    private void RefreshChrome()
    {
        ApplyLayout();
        ApplyChrome();
    }

    /// <summary>
    /// The same four elements are rearranged rather than rebuilt: two rows in a plate, one row in a
    /// strip, so switching variant never loses the text or the focus.
    /// </summary>
    private void ApplyLayout()
    {
        if (_layout is null) return;

        bool compact = Variant == DynamicInputVariant.Compact;

        _label.SetValue(Grid.RowProperty, 0);
        _label.SetValue(Grid.ColumnProperty, 0);
        _label.SetValue(Grid.ColumnSpanProperty, compact ? 1 : 2);
        _label.Margin = compact ? new Thickness(0, 0, 10, 0) : new Thickness(0, 0, 0, 3);

        _unit.SetValue(Grid.RowProperty, 0);
        _unit.SetValue(Grid.ColumnProperty, 2);
        _unit.Margin = compact ? new Thickness(10, 0, 0, 0) : new Thickness(6, 0, 0, 3);

        _box.SetValue(Grid.RowProperty, compact ? 0 : 1);
        _box.SetValue(Grid.ColumnProperty, compact ? 1 : 0);
        _box.SetValue(Grid.ColumnSpanProperty, compact ? 1 : 3);

        _hint.SetValue(Grid.RowProperty, compact ? 1 : 2);
        _hint.SetValue(Grid.ColumnProperty, 0);
        _hint.SetValue(Grid.ColumnSpanProperty, 3);
    }

    /// <summary>
    /// One plate, one outline. <c>Floating</c> carries the outline and the shadow, <c>Compact</c>
    /// drops both and collapses to a single row, and <c>Glass</c> keeps the shadow while turning the
    /// plate translucent so the drawing behind it still reads.
    /// </summary>
    private void ApplyChrome()
    {
        if (_frame is null) return;

        switch (Variant)
        {
            case DynamicInputVariant.Compact:
                _frame.BorderThickness = new Thickness(0);
                _frame.CornerRadius = new CornerRadius(Math.Max(0, CornerRadius.TopLeft - 2));
                _frame.Padding = new Thickness(8, 2, 8, 2);
                _frame.Opacity = 1;
                VisualElement.SetElevation(_frame, 0);
                break;

            case DynamicInputVariant.Glass:
                _frame.BorderThickness = new Thickness(1);
                _frame.CornerRadius = CornerRadius;
                _frame.Padding = new Thickness(9, 6, 9, 6);
                _frame.Opacity = 0.94;
                VisualElement.SetElevation(_frame, Math.Max(Elevation, 2));
                break;

            default:
                _frame.BorderThickness = new Thickness(1);
                _frame.CornerRadius = CornerRadius;
                _frame.Padding = new Thickness(9, 6, 9, 6);
                _frame.Opacity = 1;
                VisualElement.SetElevation(_frame, Elevation);
                break;
        }

        ApplyErrorChrome();
        UpdateLeader(FrameSize());
    }

    /// <summary>An unreadable value paints the plate's one outline in the danger colour.</summary>
    private void ApplyErrorChrome()
    {
        if (_frame is null) return;

        if (IsValid) _frame.SetResourceReference(Border.BorderBrushProperty, "CONCOST.Brush.Border");
        else _frame.SetResourceReference(Border.BorderBrushProperty, "CONCOST.Brush.Danger");
    }

    private void ApplyFieldWidth()
    {
        if (_box is null) return;

        _box.MinWidth = Math.Max(0, FieldWidth);
    }

    private Size FrameSize() =>
        _frame.ActualWidth > 0 && _frame.ActualHeight > 0
            ? new Size(_frame.ActualWidth, _frame.ActualHeight)
            : _frame.DesiredSize;

    /// <summary>
    /// Places the plate beside the anchor and flips it when it would leave its parent, the way a
    /// tooltip does — an overlay box that runs off the viewport is the one thing a dynamic input
    /// must never do.
    /// </summary>
    private void ApplyPlacement()
    {
        if (_frame is null) return;

        double offset = Math.Max(0, Offset);
        Size size = FrameSize();

        bool toLeft = Placement == DynamicInputPlacement.Left;
        bool above = Placement == DynamicInputPlacement.Above;

        if (Placement == DynamicInputPlacement.Auto && Parent is FrameworkElement parent && parent.ActualWidth > 0)
        {
            toLeft = Anchor.X + offset + size.Width > parent.ActualWidth;
            above = Anchor.Y + offset + size.Height > parent.ActualHeight;
        }

        _toLeft = toLeft;
        _above = above;

        double x = toLeft ? Anchor.X - offset - size.Width : Anchor.X + offset;
        double y = above ? Anchor.Y - offset - size.Height : Anchor.Y + offset;

        Margin = new Thickness(x, y, 0, 0);

        UpdateLeader(size);
    }

    private void UpdateLeader(Size size)
    {
        if (_leader is null) return;

        if (!ShowLeader || !IsOpen || Variant == DynamicInputVariant.Compact)
        {
            _leader.Visibility = Visibility.Collapsed;
            return;
        }

        double offset = Math.Max(0, Offset);

        _leader.X1 = _toLeft ? size.Width : 0;
        _leader.Y1 = _above ? size.Height : 0;
        _leader.X2 = _toLeft ? size.Width + offset : -offset;
        _leader.Y2 = _above ? size.Height + offset : -offset;
        _leader.Visibility = Visibility.Visible;
    }

    private void SyncVisibility()
    {
        Visibility = IsOpen ? Visibility.Visible : Visibility.Collapsed;

        UpdateLeader(FrameSize());

        if (IsOpen) ApplyPlacement();
    }

    private static void OnPromptChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        => ((DynamicInput)d).SyncLabel();

    private static void OnPlacementChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        => ((DynamicInput)d).ApplyPlacement();

    private static void OnChromeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var input = (DynamicInput)d;

        input.RefreshChrome();
        input.ApplyPlacement();
    }

    private static void OnFieldWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        => ((DynamicInput)d).ApplyFieldWidth();

    private static void OnOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        => ((DynamicInput)d).SyncVisibility();
}
