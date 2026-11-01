using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Media.Media3D;
using CONCOST.UI.Controls;
using CONCOST.UI.Core.Settings;
using CONCOST.UI.Core.UndoRedo;
using CONCOST.UI.Services;
using Xunit;

namespace CONCOST.UI.Tests;

/// <summary>
/// The services behind the drafting shell: selection, transactions, properties, state and the document
/// and view managers.
/// </summary>
public class CadServicesTests
{
    private static void RunOnSTA(Action action) => WpfApplicationHost.Run(action);

    private sealed class Wall
    {
        public double Length { get; set; } = 4000;

        public string Mark { get; set; } = "W1";

        public bool IsLocked { get; set; }

        public override string ToString() => Mark;
    }

    private sealed class MemorySettings : ISettingsService
    {
        private readonly Dictionary<string, object?> _values = new();

        public T? Get<T>(string key, T? defaultValue = default) =>
            _values.TryGetValue(key, out var value) && value is T typed ? typed : defaultValue;

        public void Set<T>(string key, T value) => _values[key] = value;

        public void Save()
        {
        }

        public void Load()
        {
        }
    }

    private static PropertyManager CreatePropertyManager(out UndoRedoManager undo)
    {
        undo = new UndoRedoManager();

        var manager = new PropertyManager(new TransactionManager(undo));

        manager.Register<Wall>(
            new PropertyDescriptor("Length", target => ((Wall)target).Length, (target, value) => ((Wall)target).Length = Convert.ToDouble(value, CultureInfo.InvariantCulture))
            {
                DisplayName = "Length",
                Category = "Dimensions",
                Editor = PropertyEditorKind.Numeric,
                Unit = "mm",
                DefaultValue = 4000.0,
                Validator = (_, value) => value is double length && length > 0 ? null : "Length must be positive."
            },
            new PropertyDescriptor("Mark", target => ((Wall)target).Mark, (target, value) => ((Wall)target).Mark = value as string ?? string.Empty)
            {
                DisplayName = "Mark",
                Category = "Identity",
                DefaultValue = "W1"
            },
            new PropertyDescriptor("IsLocked", target => ((Wall)target).IsLocked)
            {
                DisplayName = "Locked",
                Category = "Identity",
                Editor = PropertyEditorKind.Boolean
            });

        return manager;
    }

    [Fact]
    public void SelectionManager_ReplacesAddsAndClears()
    {
        var selection = new SelectionManager();
        var a = new object();
        var b = new object();

        selection.Select(a);

        Assert.Equal(1, selection.Count);
        Assert.Same(a, selection.Primary);

        selection.Add(b);

        Assert.Equal(2, selection.Count);
        Assert.Same(b, selection.Primary);

        selection.Toggle(b);

        Assert.Equal(1, selection.Count);
        Assert.Same(a, selection.Primary);

        selection.Clear();

        Assert.True(selection.IsEmpty);
        Assert.Null(selection.Primary);
    }

    [Fact]
    public void SelectionManager_ReportsWhatChanged()
    {
        var selection = new SelectionManager();
        SelectionChangeKind? kind = null;

        selection.SelectionChanged += (_, e) => kind = e.Kind;

        selection.Select(new object());

        Assert.Equal(SelectionChangeKind.Replaced, kind);

        selection.Clear();

        Assert.Equal(SelectionChangeKind.Cleared, kind);
    }

    [Fact]
    public void Transaction_Commit_PushesOneUndoStep()
    {
        var undo = new UndoRedoManager();
        var manager = new TransactionManager(undo);
        int value = 0;

        manager.Run("Move", transaction =>
        {
            transaction.Record("x", () => value = 10, () => value = 0);
            transaction.Record("y", () => value += 5, () => value -= 5);
        });

        Assert.Equal(15, value);
        Assert.True(undo.CanUndo);

        undo.Undo();

        Assert.Equal(0, value);
    }

    [Fact]
    public void Transaction_Rollback_RestoresWithoutAnUndoEntry()
    {
        var undo = new UndoRedoManager();
        var manager = new TransactionManager(undo);
        int value = 0;

        var transaction = manager.Begin("Move");

        transaction.Record("x", () => value = 7, () => value = 0);

        Assert.Equal(7, value);

        transaction.Rollback();

        Assert.Equal(0, value);
        Assert.False(undo.CanUndo);
        Assert.False(manager.IsInTransaction);
    }

    [Fact]
    public void TransactionManager_RefusesToOpenTwoAtOnce()
    {
        var manager = new TransactionManager(new UndoRedoManager());

        using var first = manager.Begin("First");

        Assert.Throws<InvalidOperationException>(() => manager.Begin("Second"));
    }

    [Fact]
    public void PropertyManager_ReadsDescriptorsAcrossCategories()
    {
        var manager = CreatePropertyManager(out _);

        var descriptors = manager.GetDescriptors(new Wall());

        Assert.Equal(3, descriptors.Count);
        Assert.Contains(descriptors, descriptor => descriptor.Category == "Dimensions");
        Assert.Contains(descriptors, descriptor => descriptor.Editor == PropertyEditorKind.Boolean);
    }

    [Fact]
    public void PropertyManager_TypedTextAppliesAndUndoes()
    {
        var manager = CreatePropertyManager(out var undo);
        var wall = new Wall();

        Assert.True(manager.TrySetValueFromText(wall, "Length", "5000", out string? error), error);
        Assert.Equal(5000, wall.Length);
        Assert.True(undo.CanUndo);

        undo.Undo();

        Assert.Equal(4000, wall.Length);
    }

    [Fact]
    public void PropertyManager_RejectsBadNumbersAndValidatorFailures()
    {
        var manager = CreatePropertyManager(out _);
        var wall = new Wall();

        Assert.False(manager.TrySetValueFromText(wall, "Length", "long", out string? notANumber));
        Assert.False(string.IsNullOrEmpty(notANumber));

        Assert.False(manager.TrySetValue(wall, "Length", -5.0, out string? rejected));
        Assert.Equal("Length must be positive.", rejected);

        Assert.Equal(4000, wall.Length);
    }

    [Fact]
    public void PropertyManager_ReadOnlyPropertyIsRefused()
    {
        var manager = CreatePropertyManager(out _);

        Assert.False(manager.TrySetValue(new Wall(), "IsLocked", true, out string? error));
        Assert.Contains("read-only", error!);
    }

    [Fact]
    public void PropertiesPalette_BuildsTheRowsAndResetsAValue()
    {
        RunOnSTA(() =>
        {
            var manager = CreatePropertyManager(out _);
            var palette = new PropertiesPalette(manager, new SelectionManager());
            var wall = new Wall { Length = 6000 };

            palette.Build(wall);

            Assert.Same(wall, palette.Target);
            Assert.NotEmpty(palette.GridControl.Categories);

            Assert.True(palette.ResetProperty("Length", out string? error), error);
            Assert.Equal(4000, wall.Length);
        });
    }

    [Fact]
    public void PropertiesPalette_ResettingAPropertyWithoutADefaultReportsWhy()
    {
        RunOnSTA(() =>
        {
            var manager = new PropertyManager(new TransactionManager(new UndoRedoManager()));

            manager.Register<Wall>(new PropertyDescriptor("Mark", target => ((Wall)target).Mark));

            var palette = new PropertiesPalette(manager, new SelectionManager());

            palette.Build(new Wall());

            Assert.False(palette.ResetProperty("Mark", out string? error));
            Assert.False(string.IsNullOrEmpty(error));
        });
    }

    [Fact]
    public void ApplicationStateService_MirrorsOntoEveryAttachedBar()
    {
        RunOnSTA(() =>
        {
            var state = new ApplicationStateService();
            var first = new CadStatusBar();
            var second = new CadStatusBar();

            state.Attach(first);
            state.Attach(second);

            try
            {
                state.OrthoEnabled = true;

                Assert.True(first.OrthoEnabled);
                Assert.True(second.OrthoEnabled);

                first.SetToggle(CadToggle.Polar, true);

                Assert.True(state.PolarEnabled);
                Assert.True(second.PolarEnabled);

                state.WorkPlane = UcsMode.Front;

                Assert.Equal(UcsMode.Front, second.WorkPlane);
            }
            finally
            {
                state.Detach(first);
                state.Detach(second);
            }
        });
    }

    [Fact]
    public void ApplicationStateService_ToggleFlipsAndReports()
    {
        var state = new ApplicationStateService();
        CadToggle? changed = null;

        state.ToggleChanged += (_, e) => changed = e.Toggle;

        bool snap = state.Toggle(CadToggle.Snap);

        Assert.True(snap);
        Assert.Equal(CadToggle.Snap, changed);
    }

    [Fact]
    public void DocumentManager_OpensMarksAndCloses()
    {
        var manager = new DocumentManager();

        var document = manager.Open("Level 1", "C:\\level1.rvt");

        Assert.Single(manager.Documents);
        Assert.Same(document, manager.ActiveDocument);
        Assert.Equal("Level 1", manager.BuildTitle("CONCOST").Split('\u2014')[0].Trim());

        manager.SetDirty(document, true);

        Assert.Equal("Level 1*", document.DisplayTitle);

        Assert.Same(document, manager.Open("Level 1 again", "C:\\level1.rvt"));
        Assert.Single(manager.Documents);

        Assert.True(manager.Close(document));
        Assert.Empty(manager.Documents);
        Assert.Null(manager.ActiveDocument);
    }

    [Fact]
    public void DocumentManager_CloseCanBeVetoed()
    {
        var manager = new DocumentManager();
        var document = manager.Open("Sheet A101");

        manager.DocumentClosing += (_, e) => e.Cancel = document.IsDirty;

        manager.SetDirty(document, true);

        Assert.False(manager.Close(document));
        Assert.Single(manager.Documents);

        manager.SetDirty(document, false);

        Assert.True(manager.Close(document));
    }

    [Fact]
    public void ViewManager_OpensAndRemembersTheCamera()
    {
        var manager = new ViewManager();
        var view = manager.Add("Level 1 Plan");

        ViewDefinition? opened = null;

        manager.OpenRequested += (_, e) => opened = e.View;

        manager.Open(view);

        Assert.True(view.IsOpen);
        Assert.Same(view, manager.ActiveView);
        Assert.Same(view, opened);

        var camera = new CameraController { Yaw = 12, Pitch = 34, Distance = 900 };

        manager.RememberCamera(view, camera);

        var restored = new CameraController();

        manager.RestoreCamera(view, restored);

        Assert.Equal(12, restored.Yaw, 6);
        Assert.Equal(34, restored.Pitch, 6);
        Assert.Equal(900, restored.Distance, 6);

        Assert.True(manager.Close(view));
        Assert.False(view.IsOpen);
    }

    [Fact]
    public void WorkspaceLayoutService_SavesCapturesAndApplies()
    {
        RunOnSTA(() =>
        {
            var settings = new MemorySettings();
            var service = new WorkspaceLayoutService(settings);
            var workspace = new DocumentWorkspace { TileMode = DocumentTileMode.Vertical };
            var bar = new CadStatusBar { WorkPlane = UcsMode.Top, OrthoEnabled = true };

            service.Attach(workspace, bar);
            service.ActiveViewName = "Level 2";

            var profile = service.Save("Two views");

            Assert.Equal(DocumentTileMode.Vertical, profile.TileMode);
            Assert.True(profile.OrthoEnabled);
            Assert.Equal(UcsMode.Top, profile.WorkPlane);
            Assert.Equal("Level 2", profile.ActiveViewName);

            workspace.TileMode = DocumentTileMode.Single;
            bar.SetToggle(CadToggle.Ortho, false);

            Assert.True(service.Apply("Two views"));
            Assert.Equal(DocumentTileMode.Vertical, workspace.TileMode);
            Assert.True(bar.OrthoEnabled);

            Assert.True(service.Remove("Two views"));
            Assert.Null(service.Find("Two views"));
        });
    }

    [Fact]
    public void WorkspaceLayoutService_LoadsProfilesFromSettings()
    {
        var settings = new MemorySettings();
        var first = new WorkspaceLayoutService(settings);

        first.Save("Drafting");

        var second = new WorkspaceLayoutService(settings);

        Assert.NotNull(second.Find("Drafting"));
    }

    [Fact]
    public void RendererBridge_ProjectsAndReportsTheCursorWithoutAnAdapter()
    {
        var bridge = new RendererBridge();

        bridge.Camera.Target = new Point3D(100, 200, 0);
        bridge.Camera.Projection = CameraProjection.Orthographic;

        Assert.False(bridge.IsAccelerated);
        Assert.True(bridge.TryProject(new Point3D(100, 200, 0), new Size(400, 400), out Point screen));
        Assert.Equal(200, screen.X, 3);

        bridge.UpdateCursor(new Point(200, 200), new Size(400, 400));

        Assert.Equal(100, bridge.CursorWorld.X, 3);
        Assert.Equal(200, bridge.CursorWorld.Y, 3);
    }

    [Fact]
    public void RendererBridge_PrefersTheAdapterWhenThereIsOne()
    {
        var bridge = new RendererBridge { Adapter = new OffsetAdapter() };

        Assert.True(bridge.IsAccelerated);
        Assert.True(bridge.TryProject(new Point3D(0, 0, 0), new Size(100, 100), out Point screen));
        Assert.Equal(42, screen.X, 6);
    }

    private sealed class OffsetAdapter : IRendererAdapter
    {
        public bool TryProject(Point3D world, Size viewport, out Point screen)
        {
            screen = new Point(42, 42);

            return true;
        }

        public bool TryUnproject(Point screen, Size viewport, out Point3D world)
        {
            world = new Point3D(42, 42, 0);

            return true;
        }

        public bool TryPick(Point screen, Size viewport, out object? entity)
        {
            entity = "picked";

            return true;
        }
    }

    [Fact]
    public void CameraSnapshot_CapturesAndRestoresFraming()
    {
        var camera = new CameraController
        {
            Target = new Point3D(1, 2, 3),
            Yaw = -20,
            Pitch = 15,
            Distance = 2500,
            OrthographicHeight = 3000,
            Projection = CameraProjection.Orthographic
        };

        var snapshot = CameraSnapshot.Capture(camera);
        var restored = new CameraController();

        snapshot.ApplyTo(restored);

        Assert.Equal(camera.Target, restored.Target);
        Assert.Equal(camera.Yaw, restored.Yaw, 6);
        Assert.Equal(camera.Pitch, restored.Pitch, 6);
        Assert.Equal(camera.Distance, restored.Distance, 6);
        Assert.Equal(CameraProjection.Orthographic, restored.Projection);
    }
}
