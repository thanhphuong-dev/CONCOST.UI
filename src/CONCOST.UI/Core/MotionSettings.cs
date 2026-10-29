using System;
using System.Windows;

namespace CONCOST.UI.Core;

/// <summary>
/// Central switch for motion in the library. Effects check
/// <see cref="AnimationsEnabled"/> before running their frame loop, so the whole
/// application follows the Windows "Show animations" setting (WCAG 2.2.2) and can be
/// muted from code or XAML.
/// </summary>
public static class MotionSettings
{
    private static bool _forceDisabled;

    static MotionSettings()
    {
        SystemParameters.StaticPropertyChanged += OnSystemParametersChanged;
    }

    /// <summary>Raised when motion availability changes, so effects can repaint.</summary>
    public static event EventHandler? Changed;

    /// <summary>Windows "Show animations in Windows" setting (true by default).</summary>
    public static bool SystemAnimationsEnabled => SystemParameters.ClientAreaAnimation;

    /// <summary>
    /// Application-level override. Set to true to mute every effect regardless of the
    /// OS setting (for example a "reduce motion" toggle in the app settings).
    /// </summary>
    public static bool ForceDisabled
    {
        get => _forceDisabled;
        set
        {
            if (_forceDisabled == value) return;
            _forceDisabled = value;
            Changed?.Invoke(null, EventArgs.Empty);
        }
    }

    /// <summary>True when effects should animate. Defaults to the OS preference.</summary>
    public static bool AnimationsEnabled => !_forceDisabled && SystemAnimationsEnabled;

    private static void OnSystemParametersChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SystemParameters.ClientAreaAnimation))
            Changed?.Invoke(null, EventArgs.Empty);
    }
}
