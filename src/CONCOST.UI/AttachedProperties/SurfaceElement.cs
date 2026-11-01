using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CONCOST.UI.AttachedProperties;

/// <summary>Which material a surface is cut from, in the words of the design system rather than a hex.</summary>
public enum SurfaceRole
{
    /// <summary>Leave the element's own background alone.</summary>
    None,

    /// <summary>The card paper: the default panel in the shell.</summary>
    Base,

    /// <summary>A recessed well inside a card - a caption strip, a table footer.</summary>
    Secondary,

    /// <summary>A panel that floats above the shell - a popover, an inspector, a hovered tile.</summary>
    Elevated,

    /// <summary>Below the shell - an input well, a drawing frame.</summary>
    Sunken,

    /// <summary>The accent, filled. Text on it must be <c>CONCOST.Brush.TextInverse</c>.</summary>
    Accent,

    /// <summary>The accent at 12%: a selected row, a keyed badge, a hovered tile.</summary>
    AccentSoft,

    /// <summary>The danger colour: a destructive action or an error band.</summary>
    Danger
}

/// <summary>Which border colour a surface is outlined with.</summary>
public enum SurfaceBorder
{
    /// <summary>Leave the element's own border alone.</summary>
    None,

    /// <summary>The resting hairline: the border every card wears.</summary>
    Soft,

    /// <summary>The hairline of a focused or hovered surface, one step darker.</summary>
    Strong,

    /// <summary>An accent hairline for the selected card in a set.</summary>
    Accent
}

/// <summary>
/// A quick skin for any surface element: the theme's paper, border and corner radius, addressed by
/// role instead of by brush key. The properties resolve through <c>DynamicResource</c>, so a surface
/// painted with them follows a theme or accent change without a line of code.
/// </summary>
/// <example>
/// <code>
/// &lt;Border attached:SurfaceElement.Role="Elevated"
///         attached:SurfaceElement.Border="Soft"
///         attached:SurfaceElement.Radius="12"
///         BorderThickness="1" Padding="16"&gt;
///     &lt;TextBlock Text="Tổng khối lượng bê tông" /&gt;
/// &lt;/Border&gt;
/// </code>
/// </example>
/// <remarks>
/// Roles apply to the four element shapes that actually paint a background - <see cref="Border"/>,
/// <see cref="Panel"/>, <see cref="Control"/> and <see cref="TextBlock"/> - because WPF declares a
/// separate <c>Background</c> dependency property on each of them. Anything else is left alone
/// rather than silently mis-painted.
/// </remarks>
public static class SurfaceElement
{
    /// <summary>The material of the surface. Follows the theme.</summary>
    public static readonly DependencyProperty RoleProperty =
        DependencyProperty.RegisterAttached("Role", typeof(SurfaceRole), typeof(SurfaceElement),
            new FrameworkPropertyMetadata(SurfaceRole.None, OnSkinChanged));

    public static SurfaceRole GetRole(DependencyObject obj) => (SurfaceRole)obj.GetValue(RoleProperty);
    public static void SetRole(DependencyObject obj, SurfaceRole value) => obj.SetValue(RoleProperty, value);

    /// <summary>The outline colour of the surface. Pair it with a non-zero <c>BorderThickness</c>.</summary>
    public static readonly DependencyProperty BorderProperty =
        DependencyProperty.RegisterAttached("Border", typeof(SurfaceBorder), typeof(SurfaceElement),
            new FrameworkPropertyMetadata(SurfaceBorder.None, OnSkinChanged));

    public static SurfaceBorder GetBorder(DependencyObject obj) => (SurfaceBorder)obj.GetValue(BorderProperty);
    public static void SetBorder(DependencyObject obj, SurfaceBorder value) => obj.SetValue(BorderProperty, value);

    /// <summary>
    /// The corner radius: written straight to a <see cref="Border"/>, and through
    /// <see cref="BorderElement.CornerRadius"/> for a control whose template draws the radius itself.
    /// </summary>
    public static readonly DependencyProperty RadiusProperty =
        DependencyProperty.RegisterAttached("Radius", typeof(CornerRadius?), typeof(SurfaceElement),
            new FrameworkPropertyMetadata(null, OnSkinChanged));

    public static CornerRadius? GetRadius(DependencyObject obj) => (CornerRadius?)obj.GetValue(RadiusProperty);
    public static void SetRadius(DependencyObject obj, CornerRadius? value) => obj.SetValue(RadiusProperty, value);

    // ------------------------------------------------------------------ state

    /// <summary>What the host had before we painted, so clearing a role gives the element back.</summary>
    private sealed class Skin
    {
        public readonly Dictionary<DependencyProperty, object> Saved = new();
        public bool Applying;
    }

    private static readonly ConditionalWeakTable<DependencyObject, Skin> Skins = new();

    private static void OnSkinChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not FrameworkElement element) return;

        Skin skin = Skins.GetValue(d, _ => new Skin());

        if (skin.Applying) return;

        skin.Applying = true;

        try
        {
            ApplyBackground(element, skin);
            ApplyBorder(element, skin);
            ApplyRadius(element, skin);
        }
        finally
        {
            skin.Applying = false;
        }
    }

    // ------------------------------------------------------------------ background

    private static void ApplyBackground(FrameworkElement element, Skin skin)
    {
        DependencyProperty? target = BackgroundOf(element);

        if (target is null) return;

        string? key = ResourceKey(GetRole(element));

        if (key is null)
        {
            Restore(element, target, skin);
            return;
        }

        Save(element, target, skin);
        element.SetResourceReference(target, key);
    }

    private static string? ResourceKey(SurfaceRole role) => role switch
    {
        SurfaceRole.Base => "CONCOST.Brush.Surface",
        SurfaceRole.Secondary => "CONCOST.Brush.SurfaceSecondary",
        SurfaceRole.Elevated => "CONCOST.Brush.SurfaceElevated",
        SurfaceRole.Sunken => "CONCOST.Brush.SurfaceSunken",
        SurfaceRole.Accent => "CONCOST.Brush.Primary",
        SurfaceRole.AccentSoft => "CONCOST.Brush.PrimarySoft",
        SurfaceRole.Danger => "CONCOST.Brush.Danger",
        _ => null
    };

    /// <summary>
    /// The <c>Background</c> a given element paints, or null when it has none. WPF registers a
    /// separate property on each of these types, so setting the wrong one would paint nothing.
    /// </summary>
    private static DependencyProperty? BackgroundOf(FrameworkElement element) => element switch
    {
        Border => Border.BackgroundProperty,
        Panel => Panel.BackgroundProperty,
        Control => Control.BackgroundProperty,
        TextBlock => TextBlock.BackgroundProperty,
        _ => null
    };

    // ------------------------------------------------------------------ border

    private static void ApplyBorder(FrameworkElement element, Skin skin)
    {
        DependencyProperty? target = element switch
        {
            Border => Border.BorderBrushProperty,
            Control => Control.BorderBrushProperty,
            _ => null
        };

        if (target is null) return;

        string? key = GetBorder(element) switch
        {
            SurfaceBorder.Soft => "CONCOST.Brush.Border",
            SurfaceBorder.Strong => "CONCOST.Brush.BorderHover",
            SurfaceBorder.Accent => "CONCOST.Brush.Primary",
            _ => null
        };

        if (key is null)
        {
            Restore(element, target, skin);
            return;
        }

        Save(element, target, skin);
        element.SetResourceReference(target, key);
    }

    // ------------------------------------------------------------------ radius

    private static void ApplyRadius(FrameworkElement element, Skin skin)
    {
        CornerRadius? radius = GetRadius(element);

        switch (element)
        {
            case Border border:
                if (radius is null)
                {
                    Restore(border, Border.CornerRadiusProperty, skin);
                }
                else
                {
                    Save(border, Border.CornerRadiusProperty, skin);
                    border.CornerRadius = radius.Value;
                }

                break;

            case Control control when radius is not null:
                // A control draws its own radius in its template, so hand it the value the theme reads.
                BorderElement.SetCornerRadius(control, radius.Value);
                break;
        }
    }

    // ------------------------------------------------------------------ restore

    private static void Save(DependencyObject d, DependencyProperty dp, Skin skin)
    {
        if (skin.Saved.ContainsKey(dp)) return;

        object? local = d.ReadLocalValue(dp);

        skin.Saved[dp] = local is null ? DependencyProperty.UnsetValue : local;
    }

    /// <summary>Puts back whatever the host had, so turning a role off leaves no trace behind.</summary>
    private static void Restore(DependencyObject d, DependencyProperty dp, Skin skin)
    {
        if (!skin.Saved.TryGetValue(dp, out object? saved)) return;

        skin.Saved.Remove(dp);

        if (ReferenceEquals(saved, DependencyProperty.UnsetValue) || saved is null)
            d.ClearValue(dp);
        else
            d.SetValue(dp, saved);
    }
}
