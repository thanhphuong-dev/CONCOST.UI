using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CONCOST.UI.Controls.Ribbon;

/// <summary>
/// The rich screen tip used by ribbon commands: a bold title, a wrapped description, the shortcut
/// on the right and an optional preview bitmap — the layout Office and AutoCAD both use.
/// </summary>
/// <remarks>
/// <see cref="RibbonItem"/> builds one of these automatically from <see cref="RibbonItem.Header"/>,
/// <see cref="RibbonItem.ScreenTip"/> and <see cref="RibbonItem.GestureText"/>, so a plain
/// <c>&lt;ribbon:RibbonButton Header="Save" ScreenTip="…" GestureText="Ctrl+S" /&gt;</c> already gets
/// a full tip without any extra markup.
/// </remarks>
public class RibbonToolTip : ToolTip
{
    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(nameof(Title), typeof(string), typeof(RibbonToolTip),
            new FrameworkPropertyMetadata(string.Empty));

    public static readonly DependencyProperty DescriptionProperty =
        DependencyProperty.Register(nameof(Description), typeof(string), typeof(RibbonToolTip),
            new FrameworkPropertyMetadata(string.Empty));

    public static readonly DependencyProperty GestureTextProperty =
        DependencyProperty.Register(nameof(GestureText), typeof(string), typeof(RibbonToolTip),
            new FrameworkPropertyMetadata(string.Empty));

    public static readonly DependencyProperty PreviewProperty =
        DependencyProperty.Register(nameof(Preview), typeof(ImageSource), typeof(RibbonToolTip),
            new FrameworkPropertyMetadata(null));

    public static readonly DependencyProperty FooterProperty =
        DependencyProperty.Register(nameof(Footer), typeof(string), typeof(RibbonToolTip),
            new FrameworkPropertyMetadata(string.Empty));

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

    /// <summary>What the command does. Wraps at 320 px.</summary>
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

    /// <summary>Optional picture shown above the description.</summary>
    public ImageSource? Preview
    {
        get => (ImageSource?)GetValue(PreviewProperty);
        set => SetValue(PreviewProperty, value);
    }

    /// <summary>Small grey line at the bottom — usually “Press F1 for more help”.</summary>
    public string Footer
    {
        get => (string)GetValue(FooterProperty);
        set => SetValue(FooterProperty, value);
    }
}
