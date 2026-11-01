using System;
using System.Linq;
using System.Windows;
using System.Windows.Media.Media3D;
using CONCOST.UI.Controls;
using CONCOST.UI.Services;
using Xunit;

namespace CONCOST.UI.Tests;

/// <summary>
/// The camera, the view cube and the two navigation bars: the parts a user drives the view with.
/// </summary>
public class CadViewTests
{
    private static readonly Size Viewport = new(800, 600);

    private static void RunOnSTA(Action action) => WpfApplicationHost.Run(action);

    /// <summary>One of the 26 zones by the name the cube puts on it.</summary>
    private static ViewCubeRegion Region(string name) => ViewCubeMath.Regions.Single(r => r.Name == name);

    [Fact]
    public void Camera_ProjectingTheTarget_LandsAtTheViewportCentre()
    {
        var camera = new CameraController { Target = new Point3D(10, 20, 30) };

        Assert.True(camera.TryProject(camera.Target, Viewport, out Point screen));
        Assert.Equal(400, screen.X, 3);
        Assert.Equal(300, screen.Y, 3);
    }

    [Fact]
    public void Camera_ProjectThenUnproject_RoundTripsOnTheTargetPlane()
    {
        var camera = new CameraController
        {
            Target = new Point3D(1000, 500, 0),
            Projection = CameraProjection.Orthographic,
            OrthographicHeight = 4000
        };

        var point = camera.Target + camera.Right * 500 + camera.Up * -200;

        Assert.True(camera.TryProject(point, Viewport, out Point screen));

        var back = camera.Unproject(screen, Viewport);

        Assert.Equal(point.X, back.X, 3);
        Assert.Equal(point.Y, back.Y, 3);
        Assert.Equal(point.Z, back.Z, 3);
    }

    [Fact]
    public void Camera_UnprojectingTheCentre_GivesTheTarget()
    {
        var camera = new CameraController { Target = new Point3D(4, 5, 6), Projection = CameraProjection.Orthographic };

        var world = camera.Unproject(new Point(400, 300), Viewport);

        Assert.Equal(4, world.X, 3);
        Assert.Equal(5, world.Y, 3);
        Assert.Equal(6, world.Z, 3);
    }

    [Fact]
    public void Camera_ZoomAt_KeepsTheWorldPointUnderTheCursor()
    {
        var camera = new CameraController
        {
            Target = new Point3D(0, 0, 0),
            Projection = CameraProjection.Orthographic,
            OrthographicHeight = 10000
        };

        var anchor = new Point(620, 180);
        var before = camera.Unproject(anchor, Viewport);

        camera.ZoomAt(anchor, Viewport, 0.5);

        var after = camera.Unproject(anchor, Viewport);

        Assert.Equal(before.X, after.X, 2);
        Assert.Equal(before.Y, after.Y, 2);
        Assert.True(camera.OrthographicHeight < 10000);
    }

    [Fact]
    public void Camera_FitTo_ReCentresAndPullsBack()
    {
        var camera = new CameraController();

        camera.FitTo(new Rect3D(0, 0, 0, 1000, 2000, 500));

        Assert.Equal(500, camera.Target.X, 3);
        Assert.Equal(1000, camera.Target.Y, 3);
        Assert.Equal(250, camera.Target.Z, 3);
        Assert.True(camera.Distance > CameraController.MinDistance);
    }

    [Fact]
    public void Camera_SwitchingProjection_KeepsTheApparentSize()
    {
        var camera = new CameraController { Projection = CameraProjection.Orthographic, OrthographicHeight = 2000 };

        // Only the target plane is promised to survive the flip: a point nearer than the target is
        // foreshortened by perspective, which is the whole difference between the two projections.
        var probe = camera.Target + camera.Right * 500 + camera.Up * 300;

        Assert.True(camera.TryProject(probe, Viewport, out Point orthographic));

        camera.Projection = CameraProjection.Perspective;

        Assert.True(camera.TryProject(probe, Viewport, out Point perspective));

        Assert.Equal(orthographic.X, perspective.X, 1);
        Assert.Equal(orthographic.Y, perspective.Y, 1);
    }

    // ------------------------------------------------------------------ view cube maths

    [Fact]
    public void ViewCubeMath_BuildsTheSixFacesTheTwelveEdgesAndTheEightCorners()
    {
        var regions = ViewCubeMath.Regions;

        Assert.Equal(26, regions.Count);
        Assert.Equal(6, regions.Count(r => r.Kind == ViewCubeRegionKind.Face));
        Assert.Equal(12, regions.Count(r => r.Kind == ViewCubeRegionKind.Edge));
        Assert.Equal(8, regions.Count(r => r.Kind == ViewCubeRegionKind.Corner));
        Assert.Equal(26, regions.Select(r => r.Name).Distinct().Count());

        // Only the six squares carry a caption; an edge or a corner is aimed at, not read.
        Assert.All(regions.Where(r => r.IsFace), r => Assert.False(string.IsNullOrEmpty(r.Label)));
        Assert.All(regions.Where(r => !r.IsFace), r => Assert.Empty(r.Label));

        foreach (ViewCubeRegion region in regions)
        {
            Assert.False(region.IsEmpty);
            Assert.Equal(1d, region.Normal.Length, 9);
            Assert.Equal(1d, region.Up.Length, 9);
            Assert.Equal(0d, Vector3D.DotProduct(region.Normal, region.Up), 9);
        }
    }

    [Theory]
    [InlineData("FRONT", 0, -1, 0, 0, 0, 1)]
    [InlineData("BACK", 0, 1, 0, 0, 0, 1)]
    [InlineData("RIGHT", 1, 0, 0, 0, 0, 1)]
    [InlineData("LEFT", -1, 0, 0, 0, 0, 1)]
    [InlineData("TOP", 0, 0, 1, 0, 1, 0)]
    [InlineData("BOTTOM", 0, 0, -1, 0, -1, 0)]
    public void ViewCubeMath_Faces_CarryTheNormalsAndUprightsOfTheSpecification(
        string name, double nx, double ny, double nz, double ux, double uy, double uz)
    {
        ViewCubeRegion region = Region(name);

        Assert.Equal(nx, region.Normal.X, 9);
        Assert.Equal(ny, region.Normal.Y, 9);
        Assert.Equal(nz, region.Normal.Z, 9);

        Assert.Equal(-nx, region.Forward.X, 9);
        Assert.Equal(-ny, region.Forward.Y, 9);
        Assert.Equal(-nz, region.Forward.Z, 9);

        Assert.Equal(ux, region.Up.X, 9);
        Assert.Equal(uy, region.Up.Y, 9);
        Assert.Equal(uz, region.Up.Z, 9);
    }

    [Fact]
    public void ViewCubeMath_EdgeAndCornerNormals_AreTheSumOfTheirFaces()
    {
        // An edge leans equally towards the two faces it joins, a corner towards its three.
        ViewCubeRegion edge = Region("TOP-FRONT");
        Vector3D edgeNormal = ViewCubeMath.Normalize(new Vector3D(0, -1, 1));

        Assert.Equal(edgeNormal.X, edge.Normal.X, 9);
        Assert.Equal(edgeNormal.Y, edge.Normal.Y, 9);
        Assert.Equal(edgeNormal.Z, edge.Normal.Z, 9);

        ViewCubeRegion corner = Region("TOP-FRONT-RIGHT");
        Vector3D cornerNormal = ViewCubeMath.Normalize(new Vector3D(1, -1, 1));

        Assert.Equal(cornerNormal.X, corner.Normal.X, 9);
        Assert.Equal(cornerNormal.Y, corner.Normal.Y, 9);
        Assert.Equal(cornerNormal.Z, corner.Normal.Z, 9);

        // Neither of them ever looks straight up or down, so u = (z - (z.f) f) / |...| is always defined.
        foreach (ViewCubeRegion region in ViewCubeMath.Regions.Where(r => !r.IsFace))
        {
            Vector3D up = ViewCubeMath.WorldUp
                - region.Forward * Vector3D.DotProduct(ViewCubeMath.WorldUp, region.Forward);

            Assert.Equal(0d, ViewCubeMath.Angle(ViewCubeMath.Normalize(up), region.Up), 4);
            Assert.True(region.Up.Z > 0);
            Assert.Equal(0d, Vector3D.DotProduct(region.Up, region.Forward), 9);
        }
    }

    [Fact]
    public void ViewCubeMath_RotationFor_LooksStraightAtTheRegion()
    {
        foreach (ViewCubeRegion region in ViewCubeMath.Regions)
        {
            Quaternion rotation = ViewCubeMath.RotationFor(region);

            ViewCubeMath.ToBasis(rotation, out _, out Vector3D up, out Vector3D forward);

            Assert.Equal(0d, ViewCubeMath.Angle(-forward, region.Normal), 4);
            Assert.Equal(0d, ViewCubeMath.Angle(up, region.Up), 4);
            Assert.Equal(region, ViewCubeMath.Closest(rotation));

            Assert.True(ViewCubeMath.TryClosest(-forward, 0.5, out ViewCubeRegion found));
            Assert.Equal(region, found);
        }
    }

    [Fact]
    public void ViewCubeMath_FromBasisUndoesToBasis_ForEveryRegionAndARoll()
    {
        foreach (ViewCubeRegion region in ViewCubeMath.Regions)
        {
            foreach (double roll in new[] { 0d, 25d, -90d, 137d })
            {
                Quaternion rotation = ViewCubeMath.Roll(ViewCubeMath.RotationFor(region), roll);

                ViewCubeMath.ToBasis(rotation, out Vector3D right, out Vector3D up, out Vector3D forward);

                Assert.Equal(1d, right.Length, 9);
                Assert.Equal(1d, up.Length, 9);
                Assert.Equal(1d, forward.Length, 9);
                Assert.Equal(0d, ViewCubeMath.AngleBetween(rotation, ViewCubeMath.FromBasis(right, up, forward)), 4);
            }
        }
    }

    [Fact]
    public void ViewCubeMath_Slerp_TakesTheShortArc()
    {
        Quaternion from = ViewCubeMath.RotationFor(Region("TOP-FRONT-RIGHT"));
        Quaternion to = ViewCubeMath.RotationFor(Region("BOTTOM-LEFT"));

        double total = ViewCubeMath.AngleBetween(from, to);

        Assert.Equal(total / 2, ViewCubeMath.AngleBetween(from, ViewCubeMath.Slerp(from, to, 0.5)), 4);
        Assert.Equal(0d, ViewCubeMath.AngleBetween(from, ViewCubeMath.Slerp(from, to, 0)), 4);
        Assert.Equal(0d, ViewCubeMath.AngleBetween(to, ViewCubeMath.Slerp(from, to, 1)), 4);

        // q and -q are the same pose. A slerp that followed the sign blindly would spin the cube the
        // long way round to a pose it is already heading for.
        var flipped = new Quaternion(-to.X, -to.Y, -to.Z, -to.W);

        Assert.Equal(
            0d,
            ViewCubeMath.AngleBetween(ViewCubeMath.Slerp(from, to, 0.35), ViewCubeMath.Slerp(from, flipped, 0.35)),
            6);
    }

    [Fact]
    public void ViewCubeMath_Roll_TurnsTheSceneClockwiseAndUprightUndoesIt()
    {
        Quaternion level = ViewCubeMath.RotationFor(Region("FRONT"));

        ViewCubeMath.ToBasis(level, out Vector3D right, out Vector3D up, out _);
        ViewCubeMath.ToBasis(ViewCubeMath.Roll(level, 90), out Vector3D rolledRight, out Vector3D rolledUp, out _);

        // A quarter turn clockwise takes screen up onto screen right, so a point that sat at the top
        // of the view ends up on the right of it.
        Assert.Equal(0d, ViewCubeMath.Angle(rolledUp, -right), 4);
        Assert.Equal(0d, ViewCubeMath.Angle(rolledRight, up), 4);
        Assert.Equal(0d, ViewCubeMath.AngleBetween(ViewCubeMath.Roll(level, 90), ViewCubeMath.Roll(level, -270)), 4);
        Assert.Equal(0d, ViewCubeMath.AngleBetween(ViewCubeMath.Upright(ViewCubeMath.Roll(level, 90)), level), 4);
    }

    [Fact]
    public void ViewCubeMath_TurningTheFront_TurnsTheCaptionAboutTheVertical()
    {
        ViewCubeRegion front = Region("FRONT").Rotated(90);

        // The caption stays with the front while the side it looks out of turns onto +X.
        Assert.Equal("FRONT", front.Name);
        Assert.Equal(1d, front.Normal.X, 9);
        Assert.Equal(0d, front.Normal.Y, 9);
        Assert.Equal(0d, front.Normal.Z, 9);

        // TOP has no heading, so turning the front leaves it where it was.
        ViewCubeRegion top = Region("TOP").Rotated(90);

        Assert.Equal(0d, top.Normal.X, 9);
        Assert.Equal(0d, top.Normal.Y, 9);
        Assert.Equal(1d, top.Normal.Z, 9);

        Assert.Equal(
            0d,
            ViewCubeMath.AngleBetween(
                ViewCubeMath.RotationFor(Region("FRONT"), 90),
                ViewCubeMath.RotationFor(Region("RIGHT"))),
            6);
    }

    // ------------------------------------------------------------------ view cube control

    [Fact]
    public void ViewCube_HitTest_ResolvesEachOfTheTwentySixZones()
    {
        RunOnSTA(() =>
        {
            var cube = new ViewCube { Size = ViewCubeSize.Normal, DockMargin = 0 };

            cube.Measure(new Size(200, 200));
            cube.Arrange(new Rect(0, 0, 100, 100));

            foreach (ViewCubeRegion region in ViewCubeMath.Regions)
            {
                // Square on, the zone a region names is the one over the middle of the cube, which is
                // where a user aims when they mean that region.
                cube.Rotation = ViewCubeMath.RotationFor(region);

                ViewCubeHit hit = cube.HitTest(new Point(50, 50));

                Assert.True(hit.IsRegion, $"{region.Name} was not hit: {hit}");
                Assert.Equal(region, hit.Region);
            }
        });
    }

    [Fact]
    public void ViewCube_HitTest_OutsideTheCube_ReturnsEmpty()
    {
        RunOnSTA(() =>
        {
            var cube = new ViewCube { Size = ViewCubeSize.Normal, DockMargin = 0 };

            cube.Measure(new Size(200, 200));
            cube.Arrange(new Rect(0, 0, 100, 100));

            // A corner of the control box: outside the silhouette and outside the compass ring.
            Assert.True(cube.HitTest(new Point(3, 3)).IsEmpty);
            Assert.True(cube.HitTest(new Point(-40, -40)).IsEmpty);
        });
    }

    [Fact]
    public void ViewCube_MeasuresASquareThatHoldsTheCompass()
    {
        RunOnSTA(() =>
        {
            var cube = new ViewCube { Size = ViewCubeSize.Normal, DockMargin = 0 };

            cube.Measure(new Size(400, 400));

            Assert.Equal(100d, cube.DesiredSize.Width, 4);
            Assert.Equal(cube.DesiredSize.Width, cube.DesiredSize.Height, 4);

            // Automatic is a fifth of the viewport's short side, kept between the two limits of the table.
            var automatic = new ViewCube { Size = ViewCubeSize.Automatic, DockMargin = 0 };

            automatic.Measure(new Size(400, 300));
            Assert.Equal(64d, automatic.DesiredSize.Width, 4);

            automatic.Measure(new Size(2000, 1200));
            Assert.Equal(160d, automatic.DesiredSize.Width, 4);

            automatic.Measure(new Size(1000, 1000));
            Assert.Equal(160d, automatic.DesiredSize.Width, 4);
        });
    }

    [Fact]
    public void ViewCube_GoHome_ReturnsToTheStoredHome()
    {
        RunOnSTA(() =>
        {
            var cube = new ViewCube { AnimatedTransitions = false, DockMargin = 0 };

            Quaternion home = ViewCubeMath.RotationFor(Region("FRONT"));

            cube.HomeRotation = home;
            cube.Rotation = ViewCubeMath.RotationFor(Region("BOTTOM-LEFT"));
            cube.GoHome();

            Assert.Equal(0d, ViewCubeMath.AngleBetween(cube.CurrentRotation, home), 4);

            // "Set Current View as Home" replaces what the home button returns to.
            cube.Rotation = ViewCubeMath.RotationFor(Region("LEFT"));
            cube.SetCurrentViewAsHome();

            Assert.Equal(0d, ViewCubeMath.AngleBetween(cube.HomeRotation, cube.CurrentRotation), 4);
        });
    }

    [Fact]
    public void ViewCube_DeclaringTheCurrentViewFront_TurnsTheFrontAngle()
    {
        RunOnSTA(() =>
        {
            var cube = new ViewCube { DockMargin = 0 };

            // Looking from the east: declaring that the front turns the caption onto the right face.
            cube.Rotation = ViewCubeMath.RotationFor(Region("RIGHT"));
            cube.SetCurrentViewAsFront();

            Assert.Equal(90d, cube.FrontAngle, 4);

            cube.ResetFront();

            Assert.Equal(0d, cube.FrontAngle, 4);
        });
    }

    [Fact]
    public void ViewCube_SteppingFromAFace_ReachesTheFaceAbove()
    {
        RunOnSTA(() =>
        {
            var cube = new ViewCube { AnimatedTransitions = false, DockMargin = 0 };

            cube.Rotation = ViewCubeMath.RotationFor(Region("FRONT"));
            cube.StepToNeighbor(ArrowDirection.Up);

            Assert.Equal(
                0d,
                ViewCubeMath.AngleBetween(cube.CurrentRotation, ViewCubeMath.RotationFor(Region("TOP"))),
                6);
        });
    }

    [Fact]
    public void ViewCube_RollingAQuarter_KeepsTheSameSideInFront()
    {
        RunOnSTA(() =>
        {
            var cube = new ViewCube { AnimatedTransitions = false, DockMargin = 0 };
            Quaternion front = ViewCubeMath.RotationFor(Region("FRONT"));

            cube.Rotation = front;
            cube.RollBy(1);

            Assert.Equal(90d, ViewCubeMath.AngleBetween(cube.CurrentRotation, front), 4);

            ViewCubeMath.ToBasis(cube.CurrentRotation, out _, out _, out Vector3D forward);

            Assert.Equal(0d, ViewCubeMath.Angle(-forward, Region("FRONT").Normal), 4);
        });
    }

    [Fact]
    public void ViewCube_WithAHost_TurnsTheCameraRatherThanAPoseOfItsOwn()
    {
        RunOnSTA(() =>
        {
            var camera = new CameraController();
            var cube = new ViewCube { Host = camera, AnimatedTransitions = false, DockMargin = 0 };

            cube.GoTo(Region("TOP-FRONT"));

            ViewCubeMath.ToBasis(camera.Rotation, out _, out _, out Vector3D forward);

            Assert.Equal(0d, ViewCubeMath.Angle(-forward, Region("TOP-FRONT").Normal), 4);
            Assert.Equal(0d, ViewCubeMath.AngleBetween(cube.CurrentRotation, camera.Rotation), 4);
        });
    }

    [Fact]
    public void Camera_Roll_RoundTripsThroughTheQuaternion()
    {
        var camera = new CameraController();

        camera.SetOrientation(new Vector3D(0.8, -1.0, 0.65), isFinal: false);

        Quaternion level = camera.Rotation;

        camera.Rotation = ViewCubeMath.Roll(level, 25);

        Assert.Equal(25d, camera.Roll, 4);

        ViewCubeMath.ToBasis(camera.Rotation, out Vector3D right, out Vector3D up, out Vector3D forward);

        Assert.Equal(0d, ViewCubeMath.Angle(forward, camera.Forward), 4);
        Assert.Equal(0d, ViewCubeMath.Angle(up, camera.Up), 4);
        Assert.Equal(0d, ViewCubeMath.Angle(right, camera.Right), 4);

        camera.Roll = 0;

        Assert.Equal(0d, ViewCubeMath.AngleBetween(camera.Rotation, level), 4);
    }

    [Fact]
    public void NavigationBar_PressingTheActiveTool_TurnsItOff()
    {
        RunOnSTA(() =>
        {
            var bar = new NavigationBar();

            bar.SetActiveTool(NavigationTool.Pan);

            Assert.Equal(NavigationTool.Pan, bar.ActiveTool);

            bar.SetActiveTool(NavigationTool.Orbit);

            Assert.Equal(NavigationTool.Orbit, bar.ActiveTool);

            bar.SetActiveTool(NavigationTool.Orbit);

            Assert.Equal(NavigationTool.None, bar.ActiveTool);
        });
    }

    [Fact]
    public void NavigationBar_UcsChange_RaisesTheEvent()
    {
        RunOnSTA(() =>
        {
            var bar = new NavigationBar();

            bool raised = false;

            bar.UcsChanged += (_, _) => raised = true;

            bar.Ucs = UcsMode.Front;

            Assert.True(raised);
        });
    }

    [Fact]
    public void ViewControlBar_ChangingASetting_RaisesItsName()
    {
        RunOnSTA(() =>
        {
            var bar = new ViewControlBar();

            string? changed = null;

            bar.SettingsChanged += (_, name) => changed = name;

            bar.ShowShadows = true;

            Assert.Equal(nameof(ViewControlBar.ShowShadows), changed);

            bar.DetailLevel = ViewDetailLevel.Fine;

            Assert.Equal(nameof(ViewControlBar.DetailLevel), changed);
        });
    }

    [Fact]
    public void ViewportControl_StartsAsAnOrthographicPlanView()
    {
        RunOnSTA(() =>
        {
            var viewport = new ViewportControl();

            Assert.True(viewport.ShowGrid);
            Assert.True(viewport.SnapToGrid);
            Assert.Equal(SnapKind.None, viewport.SnapKind);
            Assert.Equal(0, viewport.ToWorld(new Point(0, 0)).Z, 3);
        });
    }

    [Fact]
    public void ViewportControl_ZoomToFit_KeepsTheCameraUsable()
    {
        RunOnSTA(() =>
        {
            var viewport = new ViewportControl();

            viewport.ZoomToFit(new Rect3D(0, 0, 0, 10000, 8000, 0));

            Assert.True(double.IsFinite(viewport.UnitsToPixels(1000)));
        });
    }
}
