using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using CONCOST.UI.Themes;

namespace CONCOST.UI.AttachedProperties;

/// <summary>
/// The appearance half of <see cref="VisualElement"/>: a shadow that lifts an element off the page,
/// pointer and press feedback, an accent glow, a focus ring, and a dimming rule for disabled
/// elements. Everything here is attached behaviour, so it applies to any <see cref="UIElement"/> -
/// including the WPF controls the library does not own - without a template of its own.
/// </summary>
/// <remarks>
/// <para>
/// The properties are read once per change into a small state object kept in a
/// <see cref="ConditionalWeakTable{TKey,TValue}"/>, and one set of handlers is attached to the
/// element: a style or a template may be re-applied as often as it likes without stacking a second
/// copy of the animation on top of the first.
/// </para>
/// <para>
/// The shadow colour comes from the theme (<c>CONCOST.Color.Shadow</c>), and every watched element
/// is re-evaluated when the theme, the preset or the accent colour changes, so a light card keeps a
/// soft light shadow when the shell is switched to dark.
/// </para>
/// </remarks>
public static partial class VisualElement
{
    private const string ShadowColorKey = "CONCOST.Color.Shadow";
    private const string ShadowStrongColorKey = "CONCOST.Color.ShadowStrong";
    private const string AccentColorKey = "CONCOST.Color.Primary";

    /// <summary>
    /// Extra shadow levels while the pointer is over the element: the "this is clickable" lift of a
    /// card or a toolbar tile.
    /// </summary>
    public static readonly DependencyProperty HoverElevationProperty =
        DependencyProperty.RegisterAttached("HoverElevation", typeof(int), typeof(VisualElement),
            new FrameworkPropertyMetadata(0, OnLookChanged));

    public static int GetHoverElevation(DependencyObject obj) => (int)obj.GetValue(HoverElevationProperty);
    public static void SetHoverElevation(DependencyObject obj, int value) => obj.SetValue(HoverElevationProperty, value);

    /// <summary>A coloured glow in the accent colour while the pointer is over the element.</summary>
    public static readonly DependencyProperty GlowProperty =
        DependencyProperty.RegisterAttached("Glow", typeof(bool), typeof(VisualElement),
            new FrameworkPropertyMetadata(false, OnLookChanged));

    public static bool GetGlow(DependencyObject obj) => (bool)obj.GetValue(GlowProperty);
    public static void SetGlow(DependencyObject obj, bool value) => obj.SetValue(GlowProperty, value);

    /// <summary>
    /// Scale while the left button is held down, below 1 to press the element into the page. The
    /// default of 1 leaves the element alone.
    /// </summary>
    public static readonly DependencyProperty PressScaleProperty =
        DependencyProperty.RegisterAttached("PressScale", typeof(double), typeof(VisualElement),
            new FrameworkPropertyMetadata(1.0, OnLookChanged));

    public static double GetPressScale(DependencyObject obj) => (double)obj.GetValue(PressScaleProperty);
    public static void SetPressScale(DependencyObject obj, double value) => obj.SetValue(PressScaleProperty, value);

    /// <summary>
    /// Opacity applied while the element is disabled, so a host can keep it readable instead of the
    /// washed-out grey the control templates use. 1 leaves the element alone.
    /// </summary>
    public static readonly DependencyProperty DisabledOpacityProperty =
        DependencyProperty.RegisterAttached("DisabledOpacity", typeof(double), typeof(VisualElement),
            new FrameworkPropertyMetadata(1.0, OnLookChanged));

    public static double GetDisabledOpacity(DependencyObject obj) => (double)obj.GetValue(DisabledOpacityProperty);
    public static void SetDisabledOpacity(DependencyObject obj, double value) => obj.SetValue(DisabledOpacityProperty, value);

    /// <summary>
    /// Draws a themed focus ring around the element when it has keyboard focus. A plain
    /// <see cref="FrameworkElement"/> has no focus visual at all, so this is the one way to give a
    /// custom drawn control a focus state the keyboard user can see.
    /// </summary>
    public static readonly DependencyProperty FocusRingProperty =
        DependencyProperty.RegisterAttached("FocusRing", typeof(bool), typeof(VisualElement),
            new FrameworkPropertyMetadata(false, OnLookChanged));

    public static bool GetFocusRing(DependencyObject obj) => (bool)obj.GetValue(FocusRingProperty);
    public static void SetFocusRing(DependencyObject obj, bool value) => obj.SetValue(FocusRingProperty, value);

    /// <summary>Length of the hover and press animation in milliseconds. 0 snaps instead of easing.</summary>
    public static readonly DependencyProperty MotionDurationProperty =
        DependencyProperty.RegisterAttached("MotionDuration", typeof(double), typeof(VisualElement),
            new FrameworkPropertyMetadata(150.0, OnLookChanged));

    public static double GetMotionDuration(DependencyObject obj) => (double)obj.GetValue(MotionDurationProperty);
    public static void SetMotionDuration(DependencyObject obj, double value) => obj.SetValue(MotionDurationProperty, value);

    /// <summary>
    /// Overrides the shadow colour for this element - a blue cast under a blue button reads as light
    /// rather than as dirt. Leave it unset to follow the theme.
    /// </summary>
    public static readonly DependencyProperty ShadowColorProperty =
        DependencyProperty.RegisterAttached("ShadowColor", typeof(Color?), typeof(VisualElement),
            new FrameworkPropertyMetadata(null, OnLookChanged));

    public static Color? GetShadowColor(DependencyObject obj) => (Color?)obj.GetValue(ShadowColorProperty);
    public static void SetShadowColor(DependencyObject obj, Color? value) => obj.SetValue(ShadowColorProperty, value);

    /// <summary>
    /// The point the hover and press scale grows from, 0 to 1 on both axes. 0.5 is the centre; 1 grows
    /// up and left out of the bottom-right corner, which is what a tile anchored in a corner wants.
    /// </summary>
    public static readonly DependencyProperty ScaleOriginProperty =
        DependencyProperty.RegisterAttached("ScaleOrigin", typeof(double), typeof(VisualElement),
            new FrameworkPropertyMetadata(0.5, OnLookChanged));

    public static double GetScaleOrigin(DependencyObject obj) => (double)obj.GetValue(ScaleOriginProperty);
    public static void SetScaleOrigin(DependencyObject obj, double value) => obj.SetValue(ScaleOriginProperty, value);

    // ------------------------------------------------------------------ state

    /// <summary>What the element was told to look like, and what it looked like before we touched it.</summary>
    private sealed class Look
    {
        public int Elevation;
        public int HoverElevation;
        public bool Glow;
        public double HoverScale = 1;
        public double PressScale = 1;
        public double DisabledOpacity = 1;
        public double Duration = 150;
        public double Origin = 0.5;
        public Color? ShadowColor;
        public bool Hovered;
        public bool Pressed;

        public Effect? PreviousEffect;
        public bool OwnsEffect;

        public Transform? PreviousTransform;
        public ScaleTransform? Scale;
        public bool OwnsTransform;

        public Style? PreviousFocusVisual;
        public bool FocusVisualCaptured;

        public double PreviousOpacity = 1;
        public bool OpacityCaptured;
    }

    private static readonly ConditionalWeakTable<UIElement, Look> Looks = new();
    private static readonly List<WeakReference<UIElement>> Watched = new();

    static VisualElement()
    {
        // A theme swap changes the shadow colour and the accent, and neither is a DynamicResource on
        // an Effect, so the watched elements are re-evaluated by hand.
        ThemeManager.Current.PropertyChanged += OnThemeChanged;
    }

    private static void OnThemeChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is not (nameof(ThemeManager.Theme) or nameof(ThemeManager.Preset) or nameof(ThemeManager.AccentColor)))
            return;

        lock (Watched)
        {
            for (int i = Watched.Count - 1; i >= 0; i--)
            {
                if (!Watched[i].TryGetTarget(out UIElement? element))
                {
                    Watched.RemoveAt(i);
                    continue;
                }

                if (Looks.TryGetValue(element, out Look? look))
                {
                    ApplyFocusRing(element, look);
                    Apply(element, look);
                }
            }
        }
    }

    private static void OnLookChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not UIElement element) return;

        Look look = Watch(element);

        look.Elevation = GetElevation(element);
        look.HoverElevation = GetHoverElevation(element);
        look.Glow = GetGlow(element);
        look.HoverScale = GetHoverScale(element);
        look.PressScale = GetPressScale(element);
        look.DisabledOpacity = GetDisabledOpacity(element);
        look.Duration = GetMotionDuration(element);
        look.Origin = Math.Min(1, Math.Max(0, GetScaleOrigin(element)));
        look.ShadowColor = GetShadowColor(element);

        ApplyFocusRing(element, look);
        Apply(element, look);
    }

    /// <summary>
    /// Attaches the pointer, press, focus and enabled handlers to an element exactly once and
    /// remembers what the element looked like before the first property was set.
    /// </summary>
    private static Look Watch(UIElement element)
    {
        if (Looks.TryGetValue(element, out Look? existing)) return existing;

        var look = new Look();

        Looks.Add(element, look);

        lock (Watched)
        {
            Watched.Add(new WeakReference<UIElement>(element));
        }

        element.MouseEnter += (_, _) =>
        {
            look.Hovered = true;
            Apply(element, look);
        };

        element.MouseLeave += (_, _) =>
        {
            look.Hovered = false;
            look.Pressed = false;
            Apply(element, look);
        };

        element.PreviewMouseLeftButtonDown += (_, _) =>
        {
            if (look.PressScale >= 1) return;

            look.Pressed = true;
            Apply(element, look);
        };

        element.PreviewMouseLeftButtonUp += (_, _) =>
        {
            if (!look.Pressed) return;

            look.Pressed = false;
            Apply(element, look);
        };

        element.IsEnabledChanged += (_, _) => Apply(element, look);

        return look;
    }

    private static void Apply(UIElement element, Look look)
    {
        ApplyEffect(element, look);
        ApplyScale(element, look);
        ApplyEnabled(element, look);
    }

    // ------------------------------------------------------------------ shadow

    private static void ApplyEffect(UIElement element, Look look)
    {
        bool glow = look.Glow && look.Hovered;
        int level = Math.Max(0, look.Elevation) + (look.Hovered ? Math.Max(0, look.HoverElevation) : 0);

        if (!glow && level <= 0)
        {
            if (!look.OwnsEffect) return;

            element.Effect = look.PreviousEffect;
            look.OwnsEffect = false;

            return;
        }

        if (!look.OwnsEffect)
        {
            look.PreviousEffect = element.Effect;
            look.OwnsEffect = true;
        }

        element.Effect = glow ? Glow(element) : Shadow(element, look, level);
    }

    /// <summary>
    /// A drop shadow for one of the four levels, in the theme's shadow colour: a light theme wants a
    /// whisper of grey under a card, a dark theme wants a deep black pool. Levels 3 and 4 switch to
    /// the stronger shadow token rather than a magic multiplier, so a designer can tune the two
    /// tokens and leave every elevation in the application correct.
    /// </summary>
    private static DropShadowEffect Shadow(UIElement element, Look look, int level)
    {
        int clamped = Math.Min(4, Math.Max(1, level));
        bool strong = clamped >= 3;

        Color theme = look.ShadowColor ?? ShadowColor(element, strong);

        // The alpha is nudged, not the effect opacity: a themed shadow token can then carry the whole
        // light/dark difference and the levels only add weight on top of it.
        double factor = clamped == 1 || clamped == 3 ? 0.85 : 1.0;
        byte alpha = (byte)Math.Min(255, Math.Round(theme.A * factor));

        var shadow = new DropShadowEffect
        {
            Color = Color.FromArgb(alpha, theme.R, theme.G, theme.B),
            BlurRadius = 6 + 4 * clamped,
            ShadowDepth = 2 * clamped,
            Direction = 270,
            Opacity = 1,
            RenderingBias = RenderingBias.Performance
        };

        shadow.Freeze();

        return shadow;
    }

    /// <summary>A soft halo in the accent colour: the "selected command" look, for a hovered tile.</summary>
    private static DropShadowEffect Glow(UIElement element)
    {
        var glow = new DropShadowEffect
        {
            Color = AccentColor(element),
            BlurRadius = 20,
            ShadowDepth = 0,
            Direction = 270,
            Opacity = 0.55,
            RenderingBias = RenderingBias.Performance
        };

        glow.Freeze();

        return glow;
    }

    private static Color ShadowColor(UIElement element, bool strong) => strong
        ? ColorOf(element, ShadowStrongColorKey, Color.FromArgb(0x33, 0x0F, 0x17, 0x2A))
        : ColorOf(element, ShadowColorKey, Color.FromArgb(0x1A, 0x0F, 0x17, 0x2A));

    private static Color AccentColor(UIElement element) => ColorOf(element, AccentColorKey, Color.FromRgb(0xFF, 0x79, 0x00));

    private static Color ColorOf(UIElement element, string key, Color fallback)
    {
        object? found = Lookup(element, key);

        return found switch
        {
            Color color => color,
            SolidColorBrush brush => brush.Color,
            _ => fallback
        };
    }

    /// <summary>
    /// Resolves a theme token the way the element itself would, falling back to the application
    /// resources and then to nothing - a control measured off-tree still gets a shadow.
    /// </summary>
    private static object? Lookup(UIElement element, string key)
    {
        try
        {
            if (element is FrameworkElement framework && framework.TryFindResource(key) is object local)
                return local;

            return Application.Current?.TryFindResource(key);
        }
        catch (InvalidOperationException)
        {
            // A resource lookup on a torn-down tree can throw; a missing shadow is never worth the
            // whole render.
            return null;
        }
    }

    // ------------------------------------------------------------------ scale

    private static void ApplyScale(UIElement element, Look look)
    {
        double target = look.Pressed && look.PressScale < 1
            ? look.PressScale
            : look.Hovered
                ? look.HoverScale
                : 1.0;

        if (Math.Abs(target - 1) < 0.0005 && !look.OwnsTransform) return;

        if (!look.OwnsTransform)
        {
            look.PreviousTransform = element.RenderTransform;
            look.Scale = new ScaleTransform(1, 1);
            look.OwnsTransform = true;

            element.RenderTransformOrigin = new Point(look.Origin, look.Origin);
            element.RenderTransform = look.Scale;
        }

        ScaleTransform scale = look.Scale!;

        // The origin can be changed while an element is already scaled, and a stale origin makes the
        // hover jump sideways.
        element.RenderTransformOrigin = new Point(look.Origin, look.Origin);

        if (look.Duration <= 0)
        {
            scale.ScaleX = target;
            scale.ScaleY = target;
            return;
        }

        var animation = new DoubleAnimation(target, TimeSpan.FromMilliseconds(look.Duration))
        {
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };

        scale.BeginAnimation(ScaleTransform.ScaleXProperty, animation);
        scale.BeginAnimation(ScaleTransform.ScaleYProperty, animation);
    }

    // ------------------------------------------------------------------ disabled

    private static void ApplyEnabled(UIElement element, Look look)
    {
        if (!element.IsEnabled && look.DisabledOpacity < 1)
        {
            if (!look.OpacityCaptured)
            {
                look.PreviousOpacity = element.Opacity;
                look.OpacityCaptured = true;
            }

            element.Opacity = look.DisabledOpacity;
            return;
        }

        if (!look.OpacityCaptured) return;

        // Restore whatever the host had rather than assuming a dimmed element was opaque.
        element.Opacity = look.PreviousOpacity;
        look.OpacityCaptured = false;
    }

    // ------------------------------------------------------------------ focus ring

    private static void ApplyFocusRing(UIElement element, Look look)
    {
        if (element is not FrameworkElement framework) return;

        if (!GetFocusRing(element))
        {
            if (look.FocusVisualCaptured)
            {
                framework.FocusVisualStyle = look.PreviousFocusVisual;
                look.PreviousFocusVisual = null;
                look.FocusVisualCaptured = false;
            }

            return;
        }

        if (!look.FocusVisualCaptured)
        {
            look.PreviousFocusVisual = framework.FocusVisualStyle;
            look.FocusVisualCaptured = true;
        }

        framework.FocusVisualStyle = FocusRingStyle();
    }

    private static Style? _focusRing;

    /// <summary>
    /// The focus visual: a two pixel accent ring drawn outside the element. Built once and shared,
    /// from markup rather than a <c>FrameworkElementFactory</c>, because the ring has to carry a
    /// <c>DynamicResource</c> stroke to follow the palette when the accent changes.
    /// </summary>
    private static Style FocusRingStyle()
    {
        if (_focusRing is not null) return _focusRing;

        const string markup =
            "<ControlTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' " +
            "TargetType='Control'><Rectangle Margin='-2' RadiusX='4' RadiusY='4' StrokeThickness='2' " +
            "Stroke='{DynamicResource CONCOST.Brush.Primary}' /></ControlTemplate>";

        var style = new Style(typeof(Control));

        style.Setters.Add(new Setter(Control.TemplateProperty, System.Windows.Markup.XamlReader.Parse(markup)));
        style.Seal();

        return _focusRing = style;
    }
}
