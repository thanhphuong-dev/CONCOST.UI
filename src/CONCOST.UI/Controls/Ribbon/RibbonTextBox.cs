using System.Windows;
using System.Windows.Controls;

namespace CONCOST.UI.Controls.Ribbon;

/// <summary>
/// A compact single-line input that sits on the ribbon grid, optionally preceded by a label
/// (<see cref="HeaderedContentControl.Header"/>), e.g. <c>Width [ 120 ]</c>.
/// <para>
/// The control itself is a thin shell: the typing, selection and clipboard behaviour all come from
/// a normal <see cref="TextBox"/> in the template, so IME, undo and accessibility keep working.
/// Give it <see cref="RibbonItem.ColumnSpan"/> = 2 when the value needs room.
/// </para>
/// </summary>
public class RibbonTextBox : RibbonItem
{
    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(nameof(Text), typeof(string), typeof(RibbonTextBox),
            new FrameworkPropertyMetadata(string.Empty,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public static readonly DependencyProperty PlaceholderTextProperty =
        DependencyProperty.Register(nameof(PlaceholderText), typeof(string), typeof(RibbonTextBox),
            new FrameworkPropertyMetadata(string.Empty));

    public static readonly DependencyProperty IsReadOnlyProperty =
        DependencyProperty.Register(nameof(IsReadOnly), typeof(bool), typeof(RibbonTextBox),
            new FrameworkPropertyMetadata(false));

    public static readonly DependencyProperty MaxLengthProperty =
        DependencyProperty.Register(nameof(MaxLength), typeof(int), typeof(RibbonTextBox),
            new FrameworkPropertyMetadata(0));

    public static readonly DependencyProperty TextAlignmentProperty =
        DependencyProperty.Register(nameof(TextAlignment), typeof(TextAlignment), typeof(RibbonTextBox),
            new FrameworkPropertyMetadata(TextAlignment.Left));

    static RibbonTextBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(RibbonTextBox),
            new FrameworkPropertyMetadata(typeof(RibbonTextBox)));
    }

    public RibbonTextBox()
    {
        // The inner TextBox is the tab stop; a focusable shell would stop the caret twice.
        Focusable = false;
        IsTabStop = false;
    }

    /// <summary>Moves the caret into the field and selects the current value.</summary>
    public override void PerformClick()
    {
        if (!IsEnabled) return;

        if (GetTemplateChild("Editor") is TextBox editor)
        {
            editor.Focus();
            editor.SelectAll();
        }
    }

    /// <summary>Current value.</summary>
    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    /// <summary>Hint shown while <see cref="Text"/> is empty.</summary>
    public string PlaceholderText
    {
        get => (string)GetValue(PlaceholderTextProperty);
        set => SetValue(PlaceholderTextProperty, value);
    }

    /// <summary>Prevents typing while still allowing selection and copy.</summary>
    public bool IsReadOnly
    {
        get => (bool)GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    /// <summary>Maximum number of characters, 0 for no limit.</summary>
    public int MaxLength
    {
        get => (int)GetValue(MaxLengthProperty);
        set => SetValue(MaxLengthProperty, value);
    }

    /// <summary>Horizontal alignment of the typed text.</summary>
    public TextAlignment TextAlignment
    {
        get => (TextAlignment)GetValue(TextAlignmentProperty);
        set => SetValue(TextAlignmentProperty, value);
    }
}
