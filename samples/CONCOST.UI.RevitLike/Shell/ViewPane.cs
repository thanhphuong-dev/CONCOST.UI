using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Media3D;
using CONCOST.UI.Controls;
using CONCOST.UI.RevitLike.Model;
using CONCOST.UI.Services;

namespace CONCOST.UI.RevitLike.Shell;

/// <summary>
/// One view tab: the viewport, the model drawn into it, and the three navigation widgets that sit on
/// top of it exactly as they do in Revit.
/// </summary>
/// <remarks>
/// Each pane owns its own camera, so switching the visible tab never disturbs the zoom of the one
/// behind it. The pane also owns the local navigation commands (zoom, extents, projection) because
/// they only ever touch its own camera; anything that needs the model or the selection is left to
/// the window through <see cref="Viewport"/>.
/// </remarks>
public sealed class ViewPane : Grid
{
    private readonly BuildingModel _model;
    private bool _fitting;
    private bool _navigated;

    public ViewPane(BuildingModel model, LevelModel? level, bool isThreeD)
    {
        _model = model;
        Level = level;
        IsThreeD = isThreeD;

        Viewport = new ViewportControl
        {
            GridStep = 500,
            ShowGrid = true,
            ShowCrosshair = true,
            SnapToGrid = true,
            SnapDistance = 10
        };

        // Straight down for a storey plan, three-quarter from the south-east for the model. The
        // projection has to be set before the first zoom-to-fit: switching it recomputes the
        // orthographic height from the current distance, which would throw the fit away.
        Viewport.Camera.Projection = isThreeD ? CameraProjection.Perspective : CameraProjection.Orthographic;
        Viewport.Camera.SetOrientation(isThreeD ? new Vector3D(0.8, -1.0, 0.65) : new Vector3D(0, 0, 1), isFinal: false);

        Children.Add(Viewport);

        Renderer = new PlanRenderer(Viewport, model, level, isThreeD);

        Cube = new ViewCube
        {
            Size = ViewCubeSize.Normal,
            Position = ViewCubePosition.TopRight,
            DockMargin = 16,
            ShowCompass = true,
            Host = Viewport.Camera,

            // The three-quarter corner the model opens in, the same one Revit's default {3D} uses.
            HomeRotation = ViewCubeMath.RotationFromEye(new Vector3D(0.8, -1.0, 0.65)),
            ToolTip = "View cube — click a face, an edge or a corner to look from there"
        };

        Children.Add(Cube);

        // Bottom-right, not top-left. The plan's dimension strings run above the drawing and the
        // zoom buttons used to sit right on top of them; the corner the drawing never reaches is the
        // one under the palette. The view control bar keeps the bottom-left, so the two strips read
        // as one row of chrome along the bottom of the view, as they do in Revit.
        Navigation = new NavigationBar
        {
            ShowWalk = isThreeD,
            Ucs = isThreeD ? UcsMode.World : UcsMode.Top,
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Bottom,
            Margin = new Thickness(8, 0, 8, 8)
        };

        Children.Add(Navigation);

        Controls = new ViewControlBar
        {
            ShowShadows = isThreeD,
            VisualStyle = isThreeD ? VisualStyleMode.Shaded : VisualStyleMode.HiddenLine
        };

        Children.Add(Controls);

        Cube.ViewRequested += OnCubeViewRequested;
        Navigation.NavigationRequested += OnNavigationRequested;
        Viewport.Camera.Changed += OnCameraChanged;
        Viewport.Camera.FitToViewRequested += (_, _) => ZoomExtents();
        Viewport.SizeChanged += (_, _) => OnViewportSizeChanged();
    }

    /// <summary>The drafting surface itself.</summary>
    public ViewportControl Viewport { get; }

    /// <summary>Draws the model into the viewport overlay.</summary>
    public PlanRenderer Renderer { get; }

    /// <summary>The storey shown, or <c>null</c> when the pane shows the whole building.</summary>
    public LevelModel? Level { get; }

    /// <summary>True for the three-dimensional view.</summary>
    public bool IsThreeD { get; }

    /// <summary>The orientation cube in the top-right corner.</summary>
    public ViewCube Cube { get; }

    /// <summary>Zoom, pan and orbit strip in the top-left corner.</summary>
    public NavigationBar Navigation { get; }

    /// <summary>Scale, detail level and visual style strip along the bottom.</summary>
    public ViewControlBar Controls { get; }

    /// <summary>Repaints after the model or the selection changed.</summary>
    public void Redraw() => Renderer.Redraw();

    /// <summary>Fits the pane's own extents, which differ between a storey and the whole model.</summary>
    public void ZoomExtents()
    {
        // An explicit fit also re-arms the automatic one, so the next resize frames the drawing again.
        _navigated = false;

        Fit();
    }

    /// <summary>Looks at the model from the given target-to-eye direction.</summary>
    public void Orient(Vector3D direction)
    {
        Viewport.Camera.SetOrientation(direction);

        if (!IsThreeD) Viewport.Camera.Projection = CameraProjection.Orthographic;
    }

    private void Fit()
    {
        _fitting = true;

        try
        {
            Viewport.ZoomToFit(Level is null ? _model.ModelBounds : _model.PlanBounds(Level));
        }
        finally
        {
            _fitting = false;
        }
    }

    private void OnCameraChanged(object? sender, EventArgs e)
    {
        // A camera that moves while the pane is not fitting is the user's doing; from then on the pane
        // keeps the view they set rather than re-framing itself on the next resize.
        if (!_fitting) _navigated = true;

        // The cube reads this camera directly, so a camera on the move is all it needs to turn.
        Redraw();
    }

    private void OnViewportSizeChanged()
    {
        // The early layout passes report sizes far smaller than the final one, and a fit taken then
        // leaves the drawing a postage stamp in the middle of the pane. Keep re-framing until the
        // user takes over.
        if (!_navigated && Viewport.ActualWidth > 8 && Viewport.ActualHeight > 8) Fit();

        Redraw();
    }

    private void OnNavigationRequested(object? sender, NavigationRequestedEventArgs e)
    {
        switch (e.Command)
        {
            case NavigationCommand.ZoomIn:
                Viewport.Camera.Zoom(1 / 1.25);
                break;

            case NavigationCommand.ZoomOut:
                Viewport.Camera.Zoom(1.25);
                break;

            case NavigationCommand.ZoomExtents:
                ZoomExtents();
                break;

            case NavigationCommand.ZoomSelection:
                ZoomExtents();
                break;

            case NavigationCommand.Perspective:
                Viewport.Camera.Projection = Viewport.Camera.Projection == CameraProjection.Perspective
                    ? CameraProjection.Orthographic
                    : CameraProjection.Perspective;
                break;
        }
    }

    private void OnCubeViewRequested(object? sender, ViewRequestedEventArgs e)
    {
        // The cube turns the camera itself; the pane only adds what a view type demands. A storey plan
        // stays orthographic however it is looked at, exactly as a Revit plan does, so its dimension
        // strings keep their true length.
        if (!IsThreeD) Viewport.Camera.Projection = CameraProjection.Orthographic;
    }
}
