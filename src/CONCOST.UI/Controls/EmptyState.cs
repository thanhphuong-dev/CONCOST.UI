using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CONCOST.UI.Icons;

namespace CONCOST.UI.Controls;

/// <summary>
/// A configurable empty-state placeholder with icon (or image), title, description and up to two
/// action buttons. Supports horizontal and vertical layout, custom icon/text sizes, and secondary actions.
/// </summary>
public class EmptyState : Control
{
    static EmptyState()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(EmptyState), new FrameworkPropertyMetadata(typeof(EmptyState)));
    }

    // ------------------------------------------------------------------ original properties

    public static readonly DependencyProperty IconProperty =
        DependencyProperty.Register(nameof(Icon), typeof(IconKind), typeof(EmptyState),
            new PropertyMetadata(IconKind.Folder));

    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(nameof(Title), typeof(string), typeof(EmptyState),
            new PropertyMetadata("No Data Available"));

    public static readonly DependencyProperty DescriptionProperty =
        DependencyProperty.Register(nameof(Description), typeof(string), typeof(EmptyState),
            new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty ActionButtonTextProperty =
        DependencyProperty.Register(nameof(ActionButtonText), typeof(string), typeof(EmptyState),
            new PropertyMetadata(string.Empty, OnActionsChanged));

    public static readonly DependencyProperty ActionButtonIconProperty =
        DependencyProperty.Register(nameof(ActionButtonIcon), typeof(IconKind), typeof(EmptyState),
            new PropertyMetadata(IconKind.None));

    public static readonly DependencyProperty ActionCommandProperty =
        DependencyProperty.Register(nameof(ActionCommand), typeof(ICommand), typeof(EmptyState),
            new PropertyMetadata(null));

    public static readonly DependencyProperty SecondaryActionTextProperty =
        DependencyProperty.Register(nameof(SecondaryActionText), typeof(string), typeof(EmptyState),
            new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty ExtraContentProperty =
        DependencyProperty.Register(nameof(ExtraContent), typeof(object), typeof(EmptyState),
            new PropertyMetadata(null));

    public IconKind Icon
    {
        get => (IconKind)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string Description
    {
        get => (string)GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    public string ActionButtonText
    {
        get => (string)GetValue(ActionButtonTextProperty);
        set => SetValue(ActionButtonTextProperty, value);
    }

    public IconKind ActionButtonIcon
    {
        get => (IconKind)GetValue(ActionButtonIconProperty);
        set => SetValue(ActionButtonIconProperty, value);
    }

    public ICommand? ActionCommand
    {
        get => (ICommand?)GetValue(ActionCommandProperty);
        set => SetValue(ActionCommandProperty, value);
    }

    public string SecondaryActionText
    {
        get => (string)GetValue(SecondaryActionTextProperty);
        set => SetValue(SecondaryActionTextProperty, value);
    }

    public object? ExtraContent
    {
        get => GetValue(ExtraContentProperty);
        set => SetValue(ExtraContentProperty, value);
    }

    // ------------------------------------------------------------------ secondary action (new)

    /// <summary>Command executed when the secondary action button is clicked.</summary>
    public static readonly DependencyProperty SecondaryActionCommandProperty =
        DependencyProperty.Register(nameof(SecondaryActionCommand), typeof(ICommand), typeof(EmptyState),
            new PropertyMetadata(null));

    public ICommand? SecondaryActionCommand
    {
        get => (ICommand?)GetValue(SecondaryActionCommandProperty);
        set => SetValue(SecondaryActionCommandProperty, value);
    }

    /// <summary>Optional icon for the secondary action button.</summary>
    public static readonly DependencyProperty SecondaryActionIconProperty =
        DependencyProperty.Register(nameof(SecondaryActionIcon), typeof(IconKind), typeof(EmptyState),
            new PropertyMetadata(IconKind.None));

    public IconKind SecondaryActionIcon
    {
        get => (IconKind)GetValue(SecondaryActionIconProperty);
        set => SetValue(SecondaryActionIconProperty, value);
    }

    // ------------------------------------------------------------------ icon visuals

    /// <summary>Size of the icon glyph (or image) in device-independent pixels. Default 64.</summary>
    public static readonly DependencyProperty IconSizeProperty =
        DependencyProperty.Register(nameof(IconSize), typeof(double), typeof(EmptyState),
            new FrameworkPropertyMetadata(64.0));

    public double IconSize
    {
        get => (double)GetValue(IconSizeProperty);
        set => SetValue(IconSizeProperty, value);
    }

    /// <summary>Foreground brush of the icon glyph. Null uses the theme secondary text brush.</summary>
    public static readonly DependencyProperty IconBrushProperty =
        DependencyProperty.Register(nameof(IconBrush), typeof(Brush), typeof(EmptyState),
            new FrameworkPropertyMetadata(null));

    public Brush? IconBrush
    {
        get => (Brush?)GetValue(IconBrushProperty);
        set => SetValue(IconBrushProperty, value);
    }

    // ------------------------------------------------------------------ text sizes

    /// <summary>Font size of the title. Default 18.</summary>
    public static readonly DependencyProperty TitleFontSizeProperty =
        DependencyProperty.Register(nameof(TitleFontSize), typeof(double), typeof(EmptyState),
            new FrameworkPropertyMetadata(18.0));

    public double TitleFontSize
    {
        get => (double)GetValue(TitleFontSizeProperty);
        set => SetValue(TitleFontSizeProperty, value);
    }

    /// <summary>Font size of the description text. Default 14.</summary>
    public static readonly DependencyProperty DescriptionFontSizeProperty =
        DependencyProperty.Register(nameof(DescriptionFontSize), typeof(double), typeof(EmptyState),
            new FrameworkPropertyMetadata(14.0));

    public double DescriptionFontSize
    {
        get => (double)GetValue(DescriptionFontSizeProperty);
        set => SetValue(DescriptionFontSizeProperty, value);
    }

    // ------------------------------------------------------------------ layout

    /// <summary>
    /// Whether the icon/image and text stack vertically (default) or arrange side-by-side horizontally.
    /// </summary>
    public static readonly DependencyProperty OrientationProperty =
        DependencyProperty.Register(nameof(Orientation), typeof(Orientation), typeof(EmptyState),
            new PropertyMetadata(Orientation.Vertical));

    public Orientation Orientation
    {
        get => (Orientation)GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    // ------------------------------------------------------------------ image

    /// <summary>Optional illustration image displayed instead of (or in addition to) the icon glyph.</summary>
    public static readonly DependencyProperty ImageSourceProperty =
        DependencyProperty.Register(nameof(ImageSource), typeof(ImageSource), typeof(EmptyState),
            new PropertyMetadata(null));

    public ImageSource? ImageSource
    {
        get => (ImageSource?)GetValue(ImageSourceProperty);
        set => SetValue(ImageSourceProperty, value);
    }

    // ------------------------------------------------------------------ HasActions (read-only)

    private static readonly DependencyPropertyKey HasActionsPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(HasActions), typeof(bool), typeof(EmptyState),
            new PropertyMetadata(false));

    /// <summary><c>true</c> when <see cref="ActionButtonText"/> is not empty. Useful for template triggers.</summary>
    public static readonly DependencyProperty HasActionsProperty = HasActionsPropertyKey.DependencyProperty;

    public bool HasActions => (bool)GetValue(HasActionsProperty);

    // ------------------------------------------------------------------ callback

    private static void OnActionsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is EmptyState es)
            es.SetValue(HasActionsPropertyKey, !string.IsNullOrEmpty(e.NewValue as string));
    }
}
