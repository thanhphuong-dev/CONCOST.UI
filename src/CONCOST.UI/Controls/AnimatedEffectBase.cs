using System;
using System.Windows;
using System.Windows.Media;
using CONCOST.UI.Core;

namespace CONCOST.UI.Controls;

/// <summary>
/// Base class for effects that paint themselves every frame. It owns the
/// <see cref="CompositionTarget.Rendering"/> plumbing, the delta-time normalisation,
/// the visibility handling and the reduced-motion contract, so derived controls only
/// implement <see cref="OnTick"/> and <see cref="OnRender"/>.
/// </summary>
public abstract class AnimatedEffectBase : FrameworkElement
{
    private TimeSpan _lastFrame;
    private bool _hooked;

    public static readonly DependencyProperty IsAnimatedProperty =
        DependencyProperty.Register(nameof(IsAnimated), typeof(bool), typeof(AnimatedEffectBase),
            new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender, OnMotionSettingsChanged));

    /// <summary>Turns this effect on or off (does not unload it).</summary>
    public bool IsAnimated
    {
        get => (bool)GetValue(IsAnimatedProperty);
        set => SetValue(IsAnimatedProperty, value);
    }

    /// <summary>True when the effect may animate right now.</summary>
    protected bool MotionEnabled => IsAnimated && MotionSettings.AnimationsEnabled && IsVisible && IsLoaded;

    /// <summary>Time accumulated while the effect was running, in seconds.</summary>
    protected double Elapsed { get; private set; }

    /// <summary>
    /// Restarts the effect from its first frame. Use this for a “replay” button:
    /// toggling <see cref="IsAnimated"/> is not enough, because <see cref="Elapsed"/>
    /// keeps its value and a finished effect simply stays finished.
    /// </summary>
    public void Restart()
    {
        Elapsed = 0;
        _lastFrame = TimeSpan.Zero;
        OnRestarted();
        InvalidateVisual();
        SyncMotionState();
    }

    /// <summary>Called by <see cref="Restart"/> so derived controls can clear their own phase.</summary>
    protected virtual void OnRestarted() { }

    protected AnimatedEffectBase()
    {
        IsHitTestVisible = false;
        ClipToBounds = true;
        Loaded += (_, _) => { Hook(); SyncMotionState(); };
        Unloaded += (_, _) => { Unhook(); };
        IsVisibleChanged += (_, _) => SyncMotionState();
        MotionSettings.Changed += OnGlobalMotionChanged;
    }

    /// <summary>Called once per frame with the elapsed seconds (clamped).</summary>
    protected virtual void OnTick(double seconds) { }

    /// <summary>
    /// Called when animation stops (reduced motion, hidden, or IsAnimated = false) so the
    /// derived control can settle on a static frame instead of freezing mid-animation.
    /// </summary>
    protected virtual void OnMotionStopped() { }

    private static void OnMotionSettingsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is AnimatedEffectBase effect) effect.SyncMotionState();
    }

    private void OnGlobalMotionChanged(object? sender, EventArgs e) => SyncMotionState();

    private void SyncMotionState()
    {
        if (MotionEnabled)
        {
            Hook();
        }
        else
        {
            _lastFrame = TimeSpan.Zero;
            OnMotionStopped();
            InvalidateVisual();
        }
    }

    private void Hook()
    {
        if (_hooked) return;
        CompositionTarget.Rendering += OnFrame;
        _hooked = true;
    }

    private void Unhook()
    {
        if (!_hooked) return;
        CompositionTarget.Rendering -= OnFrame;
        _hooked = false;
        _lastFrame = TimeSpan.Zero;
    }

    private void OnFrame(object? sender, EventArgs e)
    {
        if (!MotionEnabled) return;

        TimeSpan now = TimeSpan.FromTicks(DateTime.UtcNow.Ticks);
        if (_lastFrame == TimeSpan.Zero)
        {
            _lastFrame = now;
            return;
        }

        double delta = (now - _lastFrame).TotalSeconds;
        _lastFrame = now;

        // Guard against debugger pauses / machine sleep producing a huge jump.
        if (delta <= 0 || delta > 0.25) delta = 0.016;

        Elapsed += delta;
        OnTick(delta);
        InvalidateVisual();
    }
}
