using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Media3D;
using CONCOST.UI.Controls;

namespace CONCOST.UI.Services;

/// <summary>
/// The drafting state one window shares: which snaps are on, which plane new geometry lands on, where
/// the cursor is, and how dirty the document is.
/// </summary>
/// <remarks>
/// <para>
/// One state, many surfaces. The status bar toggles, a ribbon button, a keyboard shortcut and a saved
/// layout all want to read and write the same five bits; if the toggles owned them, the ribbon would
/// have to reach into the status bar to stay in step. Here nobody owns them and
/// <see cref="Attach"/> keeps every surface mirrored.
/// </para>
/// <para>
/// The indexer is the point of the class: a caller that has a <see cref="CadToggle"/> — from a menu
/// tag, for instance — does not need a switch.
/// </para>
/// </remarks>
public class ApplicationStateService
{
    private static readonly Lazy<ApplicationStateService> Shared = new(() => new ApplicationStateService());

    private readonly List<CadStatusBar> _bars = new();
    private readonly bool[] _toggles = { false, true, false, false, true };

    private UcsMode _workPlane = UcsMode.World;
    private Point3D _cursor;
    private string? _statusMessage;
    private bool _isModified;

    /// <summary>The shared instance the application uses.</summary>
    public static ApplicationStateService Current => Shared.Value;

    /// <summary>Reads or writes one drafting toggle.</summary>
    public bool this[CadToggle toggle]
    {
        get => _toggles[(int)toggle];
        set
        {
            if (_toggles[(int)toggle] == value) return;

            _toggles[(int)toggle] = value;

            foreach (var bar in _bars) bar.SetToggle(toggle, value);

            ToggleChanged?.Invoke(this, new CadToggleEventArgs(toggle, value));
        }
    }

    /// <summary>Whether the cursor pulls onto snap points.</summary>
    public bool SnapEnabled
    {
        get => this[CadToggle.Snap];
        set => this[CadToggle.Snap] = value;
    }

    /// <summary>Whether the grid is drawn.</summary>
    public bool GridEnabled
    {
        get => this[CadToggle.Grid];
        set => this[CadToggle.Grid] = value;
    }

    /// <summary>Whether drawing is constrained to horizontal and vertical.</summary>
    public bool OrthoEnabled
    {
        get => this[CadToggle.Ortho];
        set => this[CadToggle.Ortho] = value;
    }

    /// <summary>Whether drawing is constrained to fixed angle increments.</summary>
    public bool PolarEnabled
    {
        get => this[CadToggle.Polar];
        set => this[CadToggle.Polar] = value;
    }

    /// <summary>Whether snaps to existing geometry are on.</summary>
    public bool ObjectSnapEnabled
    {
        get => this[CadToggle.ObjectSnap];
        set => this[CadToggle.ObjectSnap] = value;
    }

    /// <summary>The plane new geometry is drawn on.</summary>
    public UcsMode WorkPlane
    {
        get => _workPlane;
        set
        {
            if (_workPlane == value) return;

            _workPlane = value;

            foreach (var bar in _bars) bar.WorkPlane = value;

            WorkPlaneChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>The cursor position in model space, as the status bar reports it.</summary>
    public Point3D CursorPosition
    {
        get => _cursor;
        set
        {
            if (_cursor == value) return;

            _cursor = value;

            foreach (var bar in _bars) bar.UpdateCoordinates(value);

            CursorMoved?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>The line the status bar shows, e.g. "Specify next point".</summary>
    public string? StatusMessage
    {
        get => _statusMessage;
        set
        {
            if (string.Equals(_statusMessage, value, StringComparison.Ordinal)) return;

            _statusMessage = value;

            StatusChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>Whether the active document has unsaved changes.</summary>
    public bool IsModified
    {
        get => _isModified;
        set
        {
            if (_isModified == value) return;

            _isModified = value;

            ModifiedChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>Raised when a drafting toggle changes, whoever changed it.</summary>
    public event EventHandler<CadToggleEventArgs>? ToggleChanged;

    /// <summary>Raised when the work plane changes.</summary>
    public event EventHandler? WorkPlaneChanged;

    /// <summary>Raised when the reported cursor position changes.</summary>
    public event EventHandler? CursorMoved;

    /// <summary>Raised when the status line changes.</summary>
    public event EventHandler? StatusChanged;

    /// <summary>Raised when the modified flag changes.</summary>
    public event EventHandler? ModifiedChanged;

    /// <summary>Flips one toggle and returns its new state.</summary>
    public bool Toggle(CadToggle toggle)
    {
        this[toggle] = !this[toggle];

        return this[toggle];
    }

    /// <summary>
    /// Mirrors this state onto a status bar and follows it afterwards, so a click on the bar lands
    /// here and a change made elsewhere shows on the bar.
    /// </summary>
    public void Attach(CadStatusBar bar)
    {
        if (bar is null || _bars.Contains(bar)) return;

        _bars.Add(bar);

        foreach (CadToggle toggle in Enum.GetValues(typeof(CadToggle))) bar.SetToggle(toggle, this[toggle]);

        bar.WorkPlane = WorkPlane;
        bar.UpdateCoordinates(CursorPosition);

        bar.ToggleChanged += OnBarToggleChanged;
        bar.WorkPlaneChanged += OnBarWorkPlaneChanged;
    }

    /// <summary>Stops mirroring a status bar, e.g. when a window closes.</summary>
    public void Detach(CadStatusBar bar)
    {
        if (bar is null || !_bars.Remove(bar)) return;

        bar.ToggleChanged -= OnBarToggleChanged;
        bar.WorkPlaneChanged -= OnBarWorkPlaneChanged;
    }

    /// <summary>Copies the state into a plain object, for layout profiles.</summary>
    public IReadOnlyDictionary<CadToggle, bool> CaptureToggles() =>
        Enum.GetValues(typeof(CadToggle))
            .Cast<CadToggle>()
            .ToDictionary(toggle => toggle, toggle => this[toggle]);

    /// <summary>Writes a captured toggle set back.</summary>
    public void ApplyToggles(IReadOnlyDictionary<CadToggle, bool> toggles)
    {
        if (toggles is null) return;

        foreach (var pair in toggles) this[pair.Key] = pair.Value;
    }

    private void OnBarToggleChanged(object? sender, CadToggleEventArgs e)
    {
        // The bar is already showing the new state, so write straight to the field and skip the
        // echo back to the bar that started this.
        if (_toggles[(int)e.Toggle] == e.IsEnabled) return;

        _toggles[(int)e.Toggle] = e.IsEnabled;

        foreach (var bar in _bars.Where(bar => !ReferenceEquals(bar, sender))) bar.SetToggle(e.Toggle, e.IsEnabled);

        ToggleChanged?.Invoke(this, e);
    }

    private void OnBarWorkPlaneChanged(object? sender, EventArgs e)
    {
        if (sender is not CadStatusBar bar || bar.WorkPlane == _workPlane) return;

        _workPlane = bar.WorkPlane;

        foreach (var other in _bars.Where(other => !ReferenceEquals(other, sender))) other.WorkPlane = _workPlane;

        WorkPlaneChanged?.Invoke(this, EventArgs.Empty);
    }
}
