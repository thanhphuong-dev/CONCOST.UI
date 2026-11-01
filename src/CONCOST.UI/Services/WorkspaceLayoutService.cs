using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CONCOST.UI.Controls;
using CONCOST.UI.Core.Settings;

namespace CONCOST.UI.Services;

/// <summary>
/// A saved arrangement of the workspace: how the views are tiled, which view is in front, and which
/// drafting settings were on.
/// </summary>
/// <remarks>
/// A profile holds settings, not element references. Saving the tiled layout as objects would tie the
/// file to one session; saving the five toggles and a tile mode restores the same arrangement in a new
/// window, which is what a layout profile is for.
/// </remarks>
public class WorkspaceLayoutProfile
{
    /// <summary>The name the user picks in the layout list.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>How the views were tiled.</summary>
    public DocumentTileMode TileMode { get; set; } = DocumentTileMode.Single;

    /// <summary>The view that was in front, by name.</summary>
    public string? ActiveViewName { get; set; }

    /// <summary>The work plane that was active.</summary>
    public UcsMode WorkPlane { get; set; } = UcsMode.World;

    /// <summary>Whether snap was on.</summary>
    public bool SnapEnabled { get; set; }

    /// <summary>Whether the grid was on.</summary>
    public bool GridEnabled { get; set; } = true;

    /// <summary>Whether ortho was on.</summary>
    public bool OrthoEnabled { get; set; }

    /// <summary>Whether polar tracking was on.</summary>
    public bool PolarEnabled { get; set; }

    /// <summary>Whether object snap was on.</summary>
    public bool ObjectSnapEnabled { get; set; } = true;

    /// <summary>When the profile was saved.</summary>
    public DateTime SavedUtc { get; set; } = DateTime.UtcNow;

    /// <summary>Round-trips the profile to a dictionary, for hosts that store their own settings.</summary>
    public IReadOnlyDictionary<CadToggle, bool> ToToggles() => new Dictionary<CadToggle, bool>
    {
        [CadToggle.Snap] = SnapEnabled,
        [CadToggle.Grid] = GridEnabled,
        [CadToggle.Ortho] = OrthoEnabled,
        [CadToggle.Polar] = PolarEnabled,
        [CadToggle.ObjectSnap] = ObjectSnapEnabled
    };

    /// <summary>Reads the toggles back out of a capture.</summary>
    public void FromToggles(IReadOnlyDictionary<CadToggle, bool> toggles)
    {
        if (toggles is null) return;

        SnapEnabled = toggles.TryGetValue(CadToggle.Snap, out bool snap) && snap;
        GridEnabled = toggles.TryGetValue(CadToggle.Grid, out bool grid) && grid;
        OrthoEnabled = toggles.TryGetValue(CadToggle.Ortho, out bool ortho) && ortho;
        PolarEnabled = toggles.TryGetValue(CadToggle.Polar, out bool polar) && polar;
        ObjectSnapEnabled = toggles.TryGetValue(CadToggle.ObjectSnap, out bool objectSnap) && objectSnap;
    }

    /// <inheritdoc />
    public override string ToString() => Name;
}

/// <summary>
/// Captures, stores and restores workspace layout profiles.
/// </summary>
/// <remarks>
/// The service holds the parts it reads rather than searching the visual tree: a profile is saved from
/// the shell that owns the workspace and the status bar, and looking them up by type would break the
/// moment a dialog hosts a second workspace.
/// </remarks>
public class WorkspaceLayoutService
{
    private const string SettingsKey = "Workspace.LayoutProfiles";

    private static readonly Lazy<WorkspaceLayoutService> Shared = new(() => new WorkspaceLayoutService());

    private readonly ISettingsService _settings;

    public WorkspaceLayoutService(ISettingsService? settings = null)
    {
        _settings = settings ?? JsonSettingsService.Current;

        Load();
    }

    /// <summary>The shared instance the application uses.</summary>
    public static WorkspaceLayoutService Current => Shared.Value;

    /// <summary>The saved profiles, in save order.</summary>
    public ObservableCollection<WorkspaceLayoutProfile> Profiles { get; } = new();

    /// <summary>The workspace a profile is captured from and applied to.</summary>
    public DocumentWorkspace? Workspace { get; set; }

    /// <summary>The status bar a profile is captured from and applied to.</summary>
    public CadStatusBar? StatusBar { get; set; }

    /// <summary>The name of the view in front, which the shell sets when the active tab changes.</summary>
    public string? ActiveViewName { get; set; }

    /// <summary>Raised after a profile is applied.</summary>
    public event EventHandler<WorkspaceLayoutProfile>? ProfileApplied;

    /// <summary>Raised after the profile list changes.</summary>
    public event EventHandler? ProfilesChanged;

    /// <summary>Points the service at the parts a profile is built from.</summary>
    public void Attach(DocumentWorkspace workspace, CadStatusBar statusBar)
    {
        Workspace = workspace;
        StatusBar = statusBar;
    }

    /// <summary>Reads the current arrangement into a profile; it is not stored until saved.</summary>
    public WorkspaceLayoutProfile Capture(string name)
    {
        var profile = new WorkspaceLayoutProfile
        {
            Name = name ?? string.Empty,
            TileMode = Workspace?.TileMode ?? DocumentTileMode.Single,
            ActiveViewName = ActiveViewName,
            WorkPlane = StatusBar?.WorkPlane ?? ApplicationStateService.Current.WorkPlane,
            SavedUtc = DateTime.UtcNow
        };

        profile.FromToggles(StatusBar is null
            ? ApplicationStateService.Current.CaptureToggles()
            : new Dictionary<CadToggle, bool>
            {
                [CadToggle.Snap] = StatusBar.SnapEnabled,
                [CadToggle.Grid] = StatusBar.GridEnabled,
                [CadToggle.Ortho] = StatusBar.OrthoEnabled,
                [CadToggle.Polar] = StatusBar.PolarEnabled,
                [CadToggle.ObjectSnap] = StatusBar.ObjectSnapEnabled
            });

        return profile;
    }

    /// <summary>Saves the current arrangement under a name, replacing a profile of the same name.</summary>
    public WorkspaceLayoutProfile Save(string name)
    {
        var profile = Capture(name);

        Remove(name);

        Profiles.Add(profile);

        Persist();
        ProfilesChanged?.Invoke(this, EventArgs.Empty);

        return profile;
    }

    /// <summary>Applies a profile to the attached workspace and status bar.</summary>
    public void Apply(WorkspaceLayoutProfile profile)
    {
        if (profile is null) return;

        if (Workspace is not null) Workspace.TileMode = profile.TileMode;

        if (StatusBar is not null)
        {
            StatusBar.WorkPlane = profile.WorkPlane;

            foreach (var pair in profile.ToToggles()) StatusBar.SetToggle(pair.Key, pair.Value);
        }

        ActiveViewName = profile.ActiveViewName;

        ProfileApplied?.Invoke(this, profile);
    }

    /// <summary>Applies a profile by name.</summary>
    public bool Apply(string name)
    {
        var profile = Find(name);

        if (profile is null) return false;

        Apply(profile);

        return true;
    }

    /// <summary>Finds a profile by name, ignoring case.</summary>
    public WorkspaceLayoutProfile? Find(string name) =>
        Profiles.FirstOrDefault(profile => string.Equals(profile.Name, name, StringComparison.OrdinalIgnoreCase));

    /// <summary>Deletes a profile and returns whether one was there.</summary>
    public bool Remove(string name)
    {
        var profile = Find(name);

        if (profile is null) return false;

        Profiles.Remove(profile);

        Persist();
        ProfilesChanged?.Invoke(this, EventArgs.Empty);

        return true;
    }

    /// <summary>Writes the profile list to settings.</summary>
    public void Persist() => _settings.Set(SettingsKey, Profiles.ToList());

    private void Load()
    {
        var stored = _settings.Get<List<WorkspaceLayoutProfile>>(SettingsKey);

        if (stored is null) return;

        foreach (var profile in stored.Where(profile => profile is not null)) Profiles.Add(profile);
    }
}
