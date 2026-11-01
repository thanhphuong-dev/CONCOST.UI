using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Media3D;
using CONCOST.UI.Controls;
using CONCOST.UI.Services;
using Xunit;

namespace CONCOST.UI.Tests;

/// <summary>
/// Opening a view is the moment the shell has to hear about it, and framing a plan is the moment the
/// camera has to fit the drawing rather than the sphere around it.
/// </summary>
public class WorkspaceActivationTests
{
    private static void RunOnSTA(Action action) => WpfApplicationHost.Run(action);

    [Fact]
    public void AddDocument_ActivatesThePaneAndRaisesTheEvent()
    {
        RunOnSTA(() =>
        {
            var workspace = new DocumentWorkspace();
            int raised = 0;

            workspace.ActiveDocumentChanged += (_, _) => raised++;

            DocumentPane pane = workspace.AddDocument("Level 1", new Border());

            Assert.Same(pane, workspace.ActiveDocument);
            Assert.Equal(1, raised);
        });
    }

    [Fact]
    public void ZoomToFit_Orthographic_FramesTheDrawingNotItsDiagonal()
    {
        RunOnSTA(() =>
        {
            var viewport = new ViewportControl();

            // Straight down, which is the plan orientation the shell uses.
            viewport.Camera.Projection = CameraProjection.Orthographic;
            viewport.Camera.SetOrientation(new Vector3D(0, 0, 1), isFinal: false);

            var host = new Border { Child = viewport };

            host.Measure(new Size(800, 600));
            host.Arrange(new Rect(0, 0, 800, 600));
            host.UpdateLayout();

            viewport.ZoomToFit(new Rect3D(0, 0, 0, 24000, 16000, 0));

            // The 24 m width against a 4:3 pane needs 18 m of height, so the width decides; the
            // margin takes it to 19.8 m. Fitting the sphere around the bounds instead would have
            // returned the 29 m diagonal, leaving the drawing at two thirds of the size.
            Assert.InRange(viewport.Camera.OrthographicHeight, 19000, 20500);
        });
    }
}
