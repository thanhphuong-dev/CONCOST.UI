using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using CONCOST.UI.Controls;
using Xunit;

namespace CONCOST.UI.Tests;

/// <summary>
/// The interaction utilities of phase 4: <see cref="FileDropZone"/> and
/// <see cref="DropTargetHighlight"/>.
/// </summary>
public class InputUtilityTests
{
    private static void RunOnSTA(Action action) => WpfApplicationHost.Run(action);

    [Fact]
    public void FileDropZone_Matches_HonoursTheFilter()
    {
        RunOnSTA(() =>
        {
            var zone = new FileDropZone { Filter = "*.dwg; *.dxf" };

            Assert.True(zone.Matches(@"C:\ban ve\cot.dwg"));
            Assert.True(zone.Matches(@"C:\ban ve\COT.DXF"));
            Assert.False(zone.Matches(@"C:\ban ve\bang.xlsx"));
        });
    }

    [Fact]
    public void FileDropZone_StarAcceptsEverything()
    {
        RunOnSTA(() =>
        {
            var zone = new FileDropZone();

            Assert.Equal("*", zone.Filter);
            Assert.True(zone.Matches("anything.bin"));
        });
    }

    [Fact]
    public void FileDropZone_AddFiles_FiltersAndRaisesTheEvent()
    {
        RunOnSTA(() =>
        {
            var zone = new FileDropZone { Filter = "*.dwg" };

            var accepted = new List<string>();
            zone.FilesDropped += (_, e) => accepted.AddRange(e.Files);

            zone.AddFiles(new[] { "a.dwg", "b.xlsx", "c.dwg" });

            Assert.Equal(new[] { "a.dwg", "c.dwg" }, accepted);
            Assert.Equal(2, zone.Files.Count);
        });
    }

    [Fact]
    public void FileDropZone_SingleFileMode_TakesOnlyTheFirst()
    {
        RunOnSTA(() =>
        {
            var zone = new FileDropZone { Filter = "*.dwg", AllowMultiple = false };

            zone.AddFiles(new[] { "a.dwg", "b.dwg" });

            Assert.Single(zone.Files);
        });
    }

    [Fact]
    public void FileDropZone_NothingAccepted_DoesNotRaise()
    {
        RunOnSTA(() =>
        {
            var zone = new FileDropZone { Filter = "*.dwg" };

            int raised = 0;
            zone.FilesDropped += (_, _) => raised++;

            zone.AddFiles(new[] { "a.xlsx" });

            Assert.Equal(0, raised);
            Assert.Empty(zone.Files);
        });
    }

    [Fact]
    public void FileDropZone_ClickRaisesBrowseRequested()
    {
        RunOnSTA(() =>
        {
            var zone = new FileDropZone();

            int raised = 0;
            zone.BrowseRequested += (_, _) => raised++;

            zone.RaiseEvent(new System.Windows.Input.MouseButtonEventArgs(
                System.Windows.Input.Mouse.PrimaryDevice, Environment.TickCount,
                System.Windows.Input.MouseButton.Left)
            {
                RoutedEvent = UIElement.MouseLeftButtonUpEvent
            });

            Assert.Equal(1, raised);
        });
    }

    [Fact]
    public void DropTargetHighlight_AttachedValues_RoundTrip()
    {
        RunOnSTA(() =>
        {
            var border = new Border();

            Assert.False(DropTargetHighlight.GetIsEnabled(border));

            DropTargetHighlight.SetIsEnabled(border, true);
            DropTargetHighlight.SetCornerRadius(border, new CornerRadius(10));

            Assert.True(DropTargetHighlight.GetIsEnabled(border));
            Assert.Equal(new CornerRadius(10), DropTargetHighlight.GetCornerRadius(border));

            // Turning it off detaches the handlers; doing it twice must not throw.
            DropTargetHighlight.SetIsEnabled(border, false);
            DropTargetHighlight.SetIsEnabled(border, false);

            Assert.False(DropTargetHighlight.GetIsEnabled(border));
        });
    }

    [Fact]
    public void DragGhost_ShowAndHide_DoesNotThrow()
    {
        RunOnSTA(() =>
        {
            try
            {
                Assert.False(DragGhost.IsVisible);

                DragGhost.Show("Cột C1", Icons.IconKind.Layers);
                DragGhost.Move(null, new Point(100, 100));
                DragGhost.Show("Cột C2");

                DragGhost.Hide();
                DragGhost.Hide();

                Assert.False(DragGhost.IsVisible);
            }
            finally
            {
                DragGhost.Hide();
            }
        });
    }
}
