using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace CONCOST.UI.Controls;

/// <summary>
/// The start-up overlay: product name, version, what it is doing right now, and a progress bar.
/// </summary>
/// <remarks>
/// <para>
/// It is a control, not a window, so it can sit over the shell while the document loads and slide away
/// without a second HWND — the window appears once, at its final size, and the splash fades inside it.
/// </para>
/// <para>
/// <see cref="Dismiss(bool)"/> animates opacity and then collapses; pass <c>false</c> in tests or when
/// the system has animations switched off. <see cref="Dismissed"/> fires in both cases, once.
/// </para>
/// </remarks>
public class SplashScreen : Control
{
    /// <summary>Identifies the <see cref="AppName"/> property.</summary>
    public static readonly DependencyProperty AppNameProperty = DependencyProperty.Register(
        nameof(AppName), typeof(string), typeof(SplashScreen), new PropertyMetadata("CONCOST"));

    /// <summary>Identifies the <see cref="Version"/> property.</summary>
    public static readonly DependencyProperty VersionProperty = DependencyProperty.Register(
        nameof(Version), typeof(string), typeof(SplashScreen), new PropertyMetadata(string.Empty));

    /// <summary>Identifies the <see cref="Status"/> property.</summary>
    public static readonly DependencyProperty StatusProperty = DependencyProperty.Register(
        nameof(Status), typeof(string), typeof(SplashScreen), new PropertyMetadata(string.Empty));

    /// <summary>Identifies the <see cref="Progress"/> property.</summary>
    public static readonly DependencyProperty ProgressProperty = DependencyProperty.Register(
        nameof(Progress), typeof(double), typeof(SplashScreen),
        new PropertyMetadata(0d, null, (_, value) => Math.Max(0, Math.Min(100, (double)value))));

    /// <summary>Identifies the <see cref="ShowProgress"/> property.</summary>
    public static readonly DependencyProperty ShowProgressProperty = DependencyProperty.Register(
        nameof(ShowProgress), typeof(bool), typeof(SplashScreen), new PropertyMetadata(true));

    /// <summary>Identifies the <see cref="IsIndeterminate"/> property.</summary>
    public static readonly DependencyProperty IsIndeterminateProperty = DependencyProperty.Register(
        nameof(IsIndeterminate), typeof(bool), typeof(SplashScreen), new PropertyMetadata(false));

    /// <summary>Identifies the <see cref="Logo"/> property.</summary>
    public static readonly DependencyProperty LogoProperty = DependencyProperty.Register(
        nameof(Logo), typeof(ImageSource), typeof(SplashScreen), new PropertyMetadata(null));

    /// <summary>Identifies the <see cref="Tagline"/> property.</summary>
    public static readonly DependencyProperty TaglineProperty = DependencyProperty.Register(
        nameof(Tagline), typeof(string), typeof(SplashScreen), new PropertyMetadata(string.Empty));

    private bool _dismissed;

    static SplashScreen()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(SplashScreen), new FrameworkPropertyMetadata(typeof(SplashScreen)));
    }

    /// <summary>Raised once, after the splash has faded out.</summary>
    public event EventHandler? Dismissed;

    /// <summary>Product name shown in the largest text.</summary>
    public string AppName
    {
        get => (string)GetValue(AppNameProperty);
        set => SetValue(AppNameProperty, value);
    }

    /// <summary>Version string under the name.</summary>
    public string Version
    {
        get => (string)GetValue(VersionProperty);
        set => SetValue(VersionProperty, value);
    }

    /// <summary>Current step — "Loading families…", "Restoring workspace…".</summary>
    public string Status
    {
        get => (string)GetValue(StatusProperty);
        set => SetValue(StatusProperty, value);
    }

    /// <summary>Progress from 0 to 100.</summary>
    public double Progress
    {
        get => (double)GetValue(ProgressProperty);
        set => SetValue(ProgressProperty, value);
    }

    /// <summary>Shows or hides the bar; keep the rest of the layout for a short, silent start-up.</summary>
    public bool ShowProgress
    {
        get => (bool)GetValue(ShowProgressProperty);
        set => SetValue(ShowProgressProperty, value);
    }

    /// <summary>Runs the bar as a marquee when there is no real progress to report.</summary>
    public bool IsIndeterminate
    {
        get => (bool)GetValue(IsIndeterminateProperty);
        set => SetValue(IsIndeterminateProperty, value);
    }

    /// <summary>Optional bitmap logo above the name.</summary>
    public ImageSource? Logo
    {
        get => (ImageSource?)GetValue(LogoProperty);
        set => SetValue(LogoProperty, value);
    }

    /// <summary>Second line of description, shown above the status.</summary>
    public string Tagline
    {
        get => (string)GetValue(TaglineProperty);
        set => SetValue(TaglineProperty, value);
    }

    /// <summary>Brings the splash back, for a reload of the document.</summary>
    public void Show()
    {
        _dismissed = false;
        BeginAnimation(OpacityProperty, null);
        Opacity = 1;
        Visibility = Visibility.Visible;
    }

    /// <summary>
    /// Fades the splash out and collapses it. <paramref name="animate"/> is there for tests and for
    /// hosts that honour the system animation setting.
    /// </summary>
    public void Dismiss(bool animate = true)
    {
        if (_dismissed) return;

        _dismissed = true;

        if (!animate || !SystemParameters.ClientAreaAnimation)
        {
            Visibility = Visibility.Collapsed;
            Dismissed?.Invoke(this, EventArgs.Empty);
            return;
        }

        var fade = new DoubleAnimation(0, TimeSpan.FromMilliseconds(260))
        {
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
        };

        fade.Completed += (_, _) =>
        {
            Visibility = Visibility.Collapsed;
            Opacity = 1;
            Dismissed?.Invoke(this, EventArgs.Empty);
        };

        BeginAnimation(OpacityProperty, fade);
    }

    /// <summary>Sets <see cref="Progress"/> and <see cref="Status"/> in one call.</summary>
    public void Report(double progress, string? status = null)
    {
        Progress = progress;
        if (status is not null) Status = status;
    }
}
