using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using CONCOST.UI.AttachedProperties;
using CONCOST.UI.Icons;

namespace CONCOST.UI.Controls;

/// <summary>Which edge of a <see cref="Card"/> carries the accent rule.</summary>
public enum CardAccent
{
    /// <summary>No accent rule.</summary>
    None,

    /// <summary>A rule along the top edge - a section header.</summary>
    Top,

    /// <summary>A rule down the left edge - a record that belongs to a group.</summary>
    Left,

    /// <summary>A rule down the right edge.</summary>
    Right,

    /// <summary>A rule along the bottom edge - a footer callout.</summary>
    Bottom
}

public class Card : HeaderedContentControl
{
    static Card()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(Card), new FrameworkPropertyMetadata(typeof(Card)));
    }

    // ------------------------------------------------------------------ elevation and interaction

    /// <summary>
    /// Shadow levels 0-4, resolved from the theme's shadow token, so the same card sits on paper in
    /// the light theme and on ink in the dark one. 0 removes the shadow completely.
    /// </summary>
    public static readonly DependencyProperty ElevationProperty =
        DependencyProperty.Register(nameof(Elevation), typeof(int), typeof(Card),
            new FrameworkPropertyMetadata(1, OnElevationChanged));

    public int Elevation
    {
        get => (int)GetValue(ElevationProperty);
        set => SetValue(ElevationProperty, value);
    }

    /// <summary>
    /// Makes the card behave like a clickable tile: it lifts one elevation step, scales a hair and
    /// shows the hand cursor on hover.
    /// </summary>
    public static readonly DependencyProperty IsInteractiveProperty =
        DependencyProperty.Register(nameof(IsInteractive), typeof(bool), typeof(Card),
            new FrameworkPropertyMetadata(false, OnInteractiveChanged));

    public bool IsInteractive
    {
        get => (bool)GetValue(IsInteractiveProperty);
        set => SetValue(IsInteractiveProperty, value);
    }

    // ------------------------------------------------------------------ accent rule

    /// <summary>Which edge carries a rule in the accent colour.</summary>
    public static readonly DependencyProperty AccentBarProperty =
        DependencyProperty.Register(nameof(AccentBar), typeof(CardAccent), typeof(Card),
            new FrameworkPropertyMetadata(CardAccent.None, OnCornersAffected));

    public CardAccent AccentBar
    {
        get => (CardAccent)GetValue(AccentBarProperty);
        set => SetValue(AccentBarProperty, value);
    }

    /// <summary>Brush of the rule. Left unset, it follows the theme's accent colour.</summary>
    public static readonly DependencyProperty AccentBrushProperty =
        DependencyProperty.Register(nameof(AccentBrush), typeof(Brush), typeof(Card),
            new FrameworkPropertyMetadata(null));

    public Brush? AccentBrush
    {
        get => (Brush?)GetValue(AccentBrushProperty);
        set => SetValue(AccentBrushProperty, value);
    }

    /// <summary>Thickness of the accent rule in device independent pixels.</summary>
    public static readonly DependencyProperty AccentThicknessProperty =
        DependencyProperty.Register(nameof(AccentThickness), typeof(double), typeof(Card),
            new FrameworkPropertyMetadata(3.0, FrameworkPropertyMetadataOptions.AffectsMeasure));

    public double AccentThickness
    {
        get => (double)GetValue(AccentThicknessProperty);
        set => SetValue(AccentThicknessProperty, value);
    }

    // ------------------------------------------------------------------ surfaces

    /// <summary>
    /// Fill of the header strip. Unset, the header is transparent on the card paper - the filled
    /// variant turns it into a second surface, as a titled panel in a dashboard does.
    /// </summary>
    public static readonly DependencyProperty HeaderBackgroundProperty =
        DependencyProperty.Register(nameof(HeaderBackground), typeof(Brush), typeof(Card),
            new FrameworkPropertyMetadata(null));

    public Brush? HeaderBackground
    {
        get => (Brush?)GetValue(HeaderBackgroundProperty);
        set => SetValue(HeaderBackgroundProperty, value);
    }

    /// <summary>Fill of the footer strip; unset falls back to the secondary surface.</summary>
    public static readonly DependencyProperty FooterBackgroundProperty =
        DependencyProperty.Register(nameof(FooterBackground), typeof(Brush), typeof(Card),
            new FrameworkPropertyMetadata(null));

    public Brush? FooterBackground
    {
        get => (Brush?)GetValue(FooterBackgroundProperty);
        set => SetValue(FooterBackgroundProperty, value);
    }

    // ------------------------------------------------------------------ geometry

    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.Register(nameof(CornerRadius), typeof(CornerRadius), typeof(Card),
            new FrameworkPropertyMetadata(new CornerRadius(8), OnCornersAffected));

    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    private static readonly DependencyPropertyKey AccentCornerRadiusKey =
        DependencyProperty.RegisterReadOnly(nameof(AccentCornerRadius), typeof(CornerRadius), typeof(Card),
            new PropertyMetadata(new CornerRadius(0)));

    /// <summary>The accent rule's corners: only the two that meet the card edge are rounded.</summary>
    public static readonly DependencyProperty AccentCornerRadiusProperty = AccentCornerRadiusKey.DependencyProperty;

    public CornerRadius AccentCornerRadius => (CornerRadius)GetValue(AccentCornerRadiusProperty);

    private static readonly DependencyPropertyKey HeaderCornerRadiusKey =
        DependencyProperty.RegisterReadOnly(nameof(HeaderCornerRadius), typeof(CornerRadius), typeof(Card),
            new PropertyMetadata(new CornerRadius(0)));

    /// <summary>The header strip's corners, following <see cref="CornerRadius"/>.</summary>
    public static readonly DependencyProperty HeaderCornerRadiusProperty = HeaderCornerRadiusKey.DependencyProperty;

    public CornerRadius HeaderCornerRadius => (CornerRadius)GetValue(HeaderCornerRadiusProperty);

    private static readonly DependencyPropertyKey FooterCornerRadiusKey =
        DependencyProperty.RegisterReadOnly(nameof(FooterCornerRadius), typeof(CornerRadius), typeof(Card),
            new PropertyMetadata(new CornerRadius(0)));

    /// <summary>The footer strip's corners, following <see cref="CornerRadius"/>.</summary>
    public static readonly DependencyProperty FooterCornerRadiusProperty = FooterCornerRadiusKey.DependencyProperty;

    public CornerRadius FooterCornerRadius => (CornerRadius)GetValue(FooterCornerRadiusProperty);

    // ------------------------------------------------------------------ header and footer

    public static readonly DependencyProperty IconProperty =
        DependencyProperty.Register(nameof(Icon), typeof(IconKind), typeof(Card),
            new PropertyMetadata(IconKind.None));

    public static readonly DependencyProperty FooterProperty =
        DependencyProperty.Register(nameof(Footer), typeof(object), typeof(Card),
            new PropertyMetadata(null));

    public static readonly DependencyProperty FooterTemplateProperty =
        DependencyProperty.Register(nameof(FooterTemplate), typeof(DataTemplate), typeof(Card),
            new PropertyMetadata(null));

    public IconKind Icon
    {
        get => (IconKind)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public object? Footer
    {
        get => GetValue(FooterProperty);
        set => SetValue(FooterProperty, value);
    }

    public DataTemplate? FooterTemplate
    {
        get => (DataTemplate?)GetValue(FooterTemplateProperty);
        set => SetValue(FooterTemplateProperty, value);
    }

    // ------------------------------------------------------------------ plumbing

    protected override void OnInitialized(EventArgs e)
    {
        base.OnInitialized(e);

        // A card dressed by the theme still needs its shadow and radii pushed once; the property
        // callbacks alone would leave a card created entirely in code flat.
        ApplyElevation();
        UpdateCorners();
    }

    private static void OnElevationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not Card card) return;

        card.ApplyElevation();
        card.ApplyInteractive();
    }

    private static void OnInteractiveChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not Card card) return;

        card.ApplyInteractive();
    }

    private static void OnCornersAffected(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not Card card) return;

        card.UpdateCorners();
    }

    private void ApplyElevation() => VisualElement.SetElevation(this, Elevation);

    /// <summary>
    /// An interactive card borrows the appearance pack rather than reimplementing a hover animation:
    /// one more elevation step and a 1% scale, both themed and both respecting reduced motion.
    /// </summary>
    private void ApplyInteractive()
    {
        VisualElement.SetHoverElevation(this, IsInteractive ? Elevation + 1 : 0);
        VisualElement.SetHoverScale(this, IsInteractive ? 1.01 : 1.0);

        if (IsInteractive)
            Cursor = Cursors.Hand;
        else if (Cursor == Cursors.Hand)
            Cursor = null;
    }

    private void UpdateCorners()
    {
        CornerRadius radius = CornerRadius;

        SetValue(HeaderCornerRadiusKey, new CornerRadius(radius.TopLeft, radius.TopRight, 0, 0));
        SetValue(FooterCornerRadiusKey, new CornerRadius(0, 0, radius.BottomRight, radius.BottomLeft));

        SetValue(AccentCornerRadiusKey, AccentBar switch
        {
            CardAccent.Top => new CornerRadius(radius.TopLeft, radius.TopRight, 0, 0),
            CardAccent.Left => new CornerRadius(radius.TopLeft, 0, 0, radius.BottomLeft),
            CardAccent.Right => new CornerRadius(0, radius.TopRight, radius.BottomRight, 0),
            CardAccent.Bottom => new CornerRadius(0, 0, radius.BottomRight, radius.BottomLeft),
            _ => new CornerRadius(0)
        });
    }
}
