using System.Windows;
using System.Windows.Controls;

namespace CONCOST.UI.Controls.Ribbon;

/// <summary>
/// The little letter badge that Alt draws over a tab, a group or a command.
/// <para>
/// Badges are created and positioned by the ribbon itself — you never place one by hand. The class
/// is public so the look can be retargeted in a theme, and so the text can be read back in tests.
/// </para>
/// </summary>
public class RibbonKeyTip : ContentControl
{
    public static readonly DependencyProperty TargetProperty =
        DependencyProperty.Register(nameof(Target), typeof(FrameworkElement), typeof(RibbonKeyTip),
            new FrameworkPropertyMetadata(null));

    static RibbonKeyTip()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(RibbonKeyTip),
            new FrameworkPropertyMetadata(typeof(RibbonKeyTip)));
    }

    public RibbonKeyTip()
    {
        // A badge is decoration: it must never steal a click or a tab stop from the control below.
        Focusable = false;
        IsTabStop = false;
        IsHitTestVisible = false;
    }

    /// <summary>The element this badge points at, or null once the badge is detached.</summary>
    public FrameworkElement? Target
    {
        get => (FrameworkElement?)GetValue(TargetProperty);
        set => SetValue(TargetProperty, value);
    }

    /// <summary>True while the badge still matches what the user has typed.</summary>
    internal bool IsFiltered
    {
        get => _isFiltered;
        set
        {
            if (_isFiltered == value) return;
            _isFiltered = value;
            Visibility = value ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    private bool _isFiltered = true;
}
