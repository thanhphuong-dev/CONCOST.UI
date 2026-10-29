using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CONCOST.UI.Icons;

namespace CONCOST.UI.Controls;

public class EmptyState : Control
{
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
            new PropertyMetadata(string.Empty));

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

    static EmptyState()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(EmptyState), new FrameworkPropertyMetadata(typeof(EmptyState)));
    }
}
