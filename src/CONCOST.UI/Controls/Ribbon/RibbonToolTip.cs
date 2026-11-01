using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace CONCOST.UI.Controls.Ribbon;

/// <summary>
/// The rich screen tip used by ribbon commands: a bold title, a wrapped description, the shortcut
/// on the right and an optional preview bitmap — the layout Office and AutoCAD both use.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="RibbonItem"/> builds one of these automatically from <see cref="RibbonItem.Header"/>,
/// <see cref="RibbonItem.ScreenTip"/> and <see cref="RibbonItem.GestureText"/>, so a plain
/// <c>&lt;ribbon:RibbonButton Header="Save" ScreenTip="…" GestureText="Ctrl+S" /&gt;</c> already gets
/// a full tip without any extra markup.
/// </para>
/// <para>
/// The tip is two-level. It opens with the short form — name, one sentence, shortcut — and after
/// <see cref="ExpandDelay"/> milliseconds without the pointer moving it grows into the long form
/// with the preview image, the full explanation and the help hint. One tooltip window, two
/// densities, so nothing flickers as the second level arrives.
/// </para>
/// </remarks>
public class RibbonToolTip : ToolTip
{
    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(nameof(Title), typeof(string), typeof(RibbonToolTip),
            new FrameworkPropertyMetadata(string.Empty));

    public static readonly DependencyProperty DescriptionProperty =
        DependencyProperty.Register(nameof(Description), typeof(string), typeof(RibbonToolTip),
            new FrameworkPropertyMetadata(string.Empty, OnExpandContentChanged));

    public static readonly DependencyProperty GestureTextProperty =
        DependencyProperty.Register(nameof(GestureText), typeof(string), typeof(RibbonToolTip),
            new FrameworkPropertyMetadata(string.Empty));

    public static readonly DependencyProperty PreviewProperty =
        DependencyProperty.Register(nameof(Preview), typeof(ImageSource), typeof(RibbonToolTip),
            new FrameworkPropertyMetadata(null, OnExpandContentChanged));

    public static readonly DependencyProperty FooterProperty =
        DependencyProperty.Register(nameof(Footer), typeof(string), typeof(RibbonToolTip),
            new FrameworkPropertyMetadata(string.Empty, OnExpandContentChanged));

    public static readonly DependencyProperty ExpandedDescriptionProperty =
        DependencyProperty.Register(nameof(ExpandedDescription), typeof(string), typeof(RibbonToolTip),
            new FrameworkPropertyMetadata(string.Empty, OnExpandContentChanged));

    public static readonly DependencyProperty ExpandDelayProperty =
        DependencyProperty.Register(nameof(ExpandDelay), typeof(int), typeof(RibbonToolTip),
            new FrameworkPropertyMetadata(1500));

    private static readonly DependencyPropertyKey IsExpandedPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(IsExpanded), typeof(bool), typeof(RibbonToolTip),
            new FrameworkPropertyMetadata(false));

    public static readonly DependencyProperty IsExpandedProperty = IsExpandedPropertyKey.DependencyProperty;

    private static readonly DependencyPropertyKey CanExpandPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(CanExpand), typeof(bool), typeof(RibbonToolTip),
            new FrameworkPropertyMetadata(false));

    /// <summary>True when there is something the tip can grow into; the template keys off it.</summary>
    public static readonly DependencyProperty CanExpandProperty = CanExpandPropertyKey.DependencyProperty;

    private DispatcherTimer? _timer;

    static RibbonToolTip()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(RibbonToolTip),
            new FrameworkPropertyMetadata(typeof(RibbonToolTip)));
    }

    public RibbonToolTip()
    {
        // A ribbon must explain *why* a command is greyed out, so the tip has to survive IsEnabled.
        ToolTipService.SetShowOnDisabled(this, true);
    }

    /// <summary>Command name, shown in bold at the top.</summary>
    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>What the command does, in the short form of the tip.</summary>
    public string Description
    {
        get => (string)GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    /// <summary>Shortcut shown right-aligned, e.g. <c>Ctrl+S</c>. Display only.</summary>
    public string GestureText
    {
        get => (string)GetValue(GestureTextProperty);
        set => SetValue(GestureTextProperty, value);
    }

    /// <summary>Optional picture shown by the expanded tip.</summary>
    public ImageSource? Preview
    {
        get => (ImageSource?)GetValue(PreviewProperty);
        set => SetValue(PreviewProperty, value);
    }

    /// <summary>Small grey line at the bottom of the expanded tip — “Press F1 for more help”.</summary>
    public string Footer
    {
        get => (string)GetValue(FooterProperty);
        set => SetValue(FooterProperty, value);
    }

    /// <summary>
    /// The long form, shown under <see cref="Description"/> once the tip expands. Left empty for a
    /// simple command, whose tip then never grows.
    /// </summary>
    public string ExpandedDescription
    {
        get => (string)GetValue(ExpandedDescriptionProperty);
        set => SetValue(ExpandedDescriptionProperty, value);
    }

    /// <summary>Milliseconds the pointer has to rest before the tip grows, 1500 by default.</summary>
    public int ExpandDelay
    {
        get => (int)GetValue(ExpandDelayProperty);
        set => SetValue(ExpandDelayProperty, value);
    }

    /// <summary>True once the long form is showing.</summary>
    public bool IsExpanded => (bool)GetValue(IsExpandedProperty);

    /// <summary>True when there is something the tip can grow into.</summary>
    public bool CanExpand => (bool)GetValue(CanExpandProperty);

    protected override void OnOpened(RoutedEventArgs e)
    {
        base.OnOpened(e);

        SetValue(IsExpandedPropertyKey, false);
        StartExpandTimer();
    }

    protected override void OnClosed(RoutedEventArgs e)
    {
        base.OnClosed(e);

        _timer?.Stop();
        _timer = null;
        SetValue(IsExpandedPropertyKey, false);
    }

    private void StartExpandTimer()
    {
        if (!CanExpand || ExpandDelay <= 0) return;

        _timer?.Stop();

        _timer = new DispatcherTimer(DispatcherPriority.Background, Dispatcher)
        {
            Interval = TimeSpan.FromMilliseconds(ExpandDelay)
        };

        _timer.Tick += (_, _) =>
        {
            _timer?.Stop();
            _timer = null;
            SetValue(IsExpandedPropertyKey, true);
        };

        _timer.Start();
    }

    private static void OnExpandContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not RibbonToolTip tip) return;

        bool canExpand = tip.Preview != null ||
                         tip.ExpandedDescription.Length > 0 ||
                         tip.Footer.Length > 0;

        tip.SetValue(CanExpandPropertyKey, canExpand);

        // The pointer is already resting on the item, so the new content has to arm the timer.
        if (tip.IsOpen) tip.StartExpandTimer();
    }
}
