using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using CONCOST.UI.Services;

namespace CONCOST.UI.Controls;

/// <summary>Which part of a <see cref="ViewCube"/> a point landed on.</summary>
public enum ViewCubePart
{
    None,
    Face,
    Edge,
    Corner,
    Compass,
    Home,
    Arrow,
    Roll,
    Menu
}

/// <summary>What was under the pointer on a <see cref="ViewCube"/>.</summary>
public sealed class ViewCubeHit
{
    /// <summary>The hit that means "nothing".</summary>
    public static ViewCubeHit Empty { get; } = new();

    private ViewCubeHit()
    {
        Label = string.Empty;
    }

    private ViewCubeHit(
        ViewCubePart part,
        string label,
        ViewCubeRegion region = default,
        ArrowDirection arrow = ArrowDirection.Up,
        int roll = 0,
        Vector3D compass = default)
    {
        Part = part;
        Label = label;
        Region = region;
        Arrow = arrow;
        Roll = roll;
        Compass = compass;
    }

    /// <summary>Which widget was hit.</summary>
    public ViewCubePart Part { get; }

    /// <summary>Caption of the hit: <c>TOP</c>, <c>TOP-FRONT-RIGHT</c>, <c>N</c>, <c>HOME</c>.</summary>
    public string Label { get; }

    /// <summary>The zone that was hit; empty for the compass, the home button and the menu.</summary>
    public ViewCubeRegion Region { get; }

    /// <summary>For an arrow hit: which of the four was pressed.</summary>
    public ArrowDirection Arrow { get; }

    /// <summary>For a roll hit: +1 clockwise, -1 counter-clockwise.</summary>
    public int Roll { get; }

    /// <summary>For a compass hit: the horizontal world direction the letter points at.</summary>
    public Vector3D Compass { get; }

    /// <summary>Direction from the model towards the eye that this hit asks for.</summary>
    public Vector3D Direction => Region.Normal;

    /// <summary><c>true</c> when the point missed everything the control draws.</summary>
    public bool IsEmpty => Part == ViewCubePart.None;

    /// <summary><c>true</c> when the hit picked one of the 26 zones.</summary>
    public bool IsRegion => Part is ViewCubePart.Face or ViewCubePart.Edge or ViewCubePart.Corner;

    /// <inheritdoc />
    public override string ToString() => IsEmpty ? "none" : $"{Part}:{Label}";

    internal static ViewCubeHit ForRegion(ViewCubeRegion region) => new(
        region.Kind switch
        {
            ViewCubeRegionKind.Face => ViewCubePart.Face,
            ViewCubeRegionKind.Edge => ViewCubePart.Edge,
            _ => ViewCubePart.Corner
        },
        region.Name,
        region);

    internal static ViewCubeHit ForHome() => new(ViewCubePart.Home, "HOME");

    internal static ViewCubeHit ForArrow(ArrowDirection arrow) => new(ViewCubePart.Arrow, arrow.ToString(), arrow: arrow);

    internal static ViewCubeHit ForRoll(int direction) =>
        new(ViewCubePart.Roll, direction > 0 ? "ROLL+" : "ROLL-", roll: direction);

    internal static ViewCubeHit ForCompass(string label, Vector3D direction) =>
        new(ViewCubePart.Compass, label, compass: direction);

    internal static ViewCubeHit ForMenu() => new(ViewCubePart.Menu, "MENU");
}

/// <summary>Raised before a view change runs; a host may cancel it or redirect it.</summary>
public class ViewRequestedEventArgs : EventArgs
{
    public ViewRequestedEventArgs(ViewCubeHit hit, Quaternion target)
    {
        Hit = hit;
        Target = target;
    }

    /// <summary>What the user picked.</summary>
    public ViewCubeHit Hit { get; }

    /// <summary>The orientation the control is about to turn to; a host may replace it.</summary>
    public Quaternion Target { get; set; }

    /// <summary>Set to <c>true</c> to keep the view where it is, e.g. in a locked 2D view.</summary>
    public bool Cancel { get; set; }
}

/// <summary>Raised when the pointer moves on or off one of the 26 zones.</summary>
public class RegionHoverEventArgs : EventArgs
{
    public RegionHoverEventArgs(ViewCubeHit hit) => Hit = hit;

    /// <summary>The zone under the pointer, or an empty hit when there is none.</summary>
    public ViewCubeHit Hit { get; }

    /// <summary>The zone under the pointer, or an empty region.</summary>
    public ViewCubeRegion Region => Hit.Region;
}

/// <summary>Raised when the projection is chosen from the context menu.</summary>
public class ProjectionModeEventArgs : EventArgs
{
    public ProjectionModeEventArgs(ProjectionMode mode) => Mode = mode;

    /// <summary>The projection the user picked.</summary>
    public ProjectionMode Mode { get; }
}

/// <summary>
/// The colours of a <see cref="ViewCube"/>. Every brush is optional: a null one falls back to the
/// <c>CONCOST.Brush.ViewCube*</c> resource of the theme in force, which is what keeps a light shell
/// light when the theme is swapped at run time.
/// </summary>
public sealed class ViewCubeTheme
{
    /// <summary>Fill of the face, edge and corner zones.</summary>
    public Brush? Face { get; set; }

    /// <summary>The bevel lines between the zones.</summary>
    public Brush? Edge { get; set; }

    /// <summary>The outline of the silhouette.</summary>
    public Brush? Border { get; set; }

    /// <summary>Captions and the compass letters.</summary>
    public Brush? Text { get; set; }

    /// <summary>Fill and outline of whatever the pointer is over.</summary>
    public Brush? Highlight { get; set; }
}

/// <summary>
/// The navigation cube: a bevelled box with 26 clickable zones, a compass, a home button, four
/// quarter-turn arrows, two roll buttons and a context menu, kept in step with the camera of the
/// viewport it sits on.
/// </summary>
/// <remarks>
/// <para>
/// The control is a display layer. It reads <see cref="ICameraHost.Rotation"/> and writes it only
/// while a transition animates or the user drags the cube; everything else - panning, zooming,
/// framing, drawing the model - belongs to the host. The geometry and the camera maths live in
/// <see cref="ViewCubeMath"/> and never touch a drawing context, so a WinForms, OpenGL or web viewer
/// can reuse them with its own renderer.
/// </para>
/// <para>
/// Zones are resolved the way a user aims at them: the pointer's ray picks whichever zone faces the
/// eye at that point, so a corner is hit on the corner rather than on the face behind it. Only what
/// is actually drawn takes the pointer, which is what lets the transparent part of the control pass
/// a click down to the viewport underneath.
/// </para>
/// <para>
/// The Options table of the specification is exposed here as dependency properties - <see cref="Size"/>,
/// <see cref="Position"/>, <see cref="DockMargin"/>, <see cref="InactiveOpacity"/> and the rest - so a
/// host can bind or theme them instead of rebuilding the settings object.
/// </para>
/// </remarks>
public partial class ViewCube : FrameworkElement
{
    /// <summary>How far the arrow buttons stand off the silhouette, in pixels.</summary>
    private const double ArrowGap = 4;

    private const double ButtonSize = 16;
    private const double RollButtonSize = 16;

    private ProjectionMode _projection = ProjectionMode.Perspective;
    private ViewCubeHit _hover = ViewCubeHit.Empty;
    private bool _pointerOver;

    private DragMode _drag;
    private Point _dragStart;
    private Quaternion _dragRotation;
    private double _dragAngle;
    private bool _dragged;
    private ViewCubeHit _pressed = ViewCubeHit.Empty;

    private DispatcherTimer? _timer;
    private Quaternion _animationFrom;
    private Quaternion _animationTo;
    private long _animationStart;
    private double _animationDuration;
    private bool _animationUpright;

    /// <summary>Identifies the <see cref="Host"/> property.</summary>
    public static readonly DependencyProperty HostProperty = DependencyProperty.Register(
        nameof(Host), typeof(ICameraHost), typeof(ViewCube),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, OnHostChanged));

    /// <summary>Identifies the <see cref="Rotation"/> property.</summary>
    public static readonly DependencyProperty RotationProperty = DependencyProperty.Register(
        nameof(Rotation), typeof(Quaternion), typeof(ViewCube),
        new FrameworkPropertyMetadata(DefaultHome(), FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Identifies the <see cref="HomeRotation"/> property.</summary>
    public static readonly DependencyProperty HomeRotationProperty = DependencyProperty.Register(
        nameof(HomeRotation), typeof(Quaternion), typeof(ViewCube),
        new FrameworkPropertyMetadata(DefaultHome(), FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Identifies the <see cref="FrontAngle"/> property.</summary>
    public static readonly DependencyProperty FrontAngleProperty = DependencyProperty.Register(
        nameof(FrontAngle), typeof(double), typeof(ViewCube),
        new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Identifies the <see cref="Theme"/> property.</summary>
    public static readonly DependencyProperty ThemeProperty = DependencyProperty.Register(
        nameof(Theme), typeof(ViewCubeTheme), typeof(ViewCube),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Identifies the <see cref="Visible"/> property.</summary>
    public static readonly DependencyProperty VisibleProperty = DependencyProperty.Register(
        nameof(Visible), typeof(bool), typeof(ViewCube), new FrameworkPropertyMetadata(true));

    /// <summary>Identifies the <see cref="Position"/> property.</summary>
    public static readonly DependencyProperty PositionProperty = DependencyProperty.Register(
        nameof(Position), typeof(ViewCubePosition), typeof(ViewCube),
        new FrameworkPropertyMetadata(ViewCubePosition.TopRight));

    /// <summary>Identifies the <see cref="DockMargin"/> property.</summary>
    public static readonly DependencyProperty DockMarginProperty = DependencyProperty.Register(
        nameof(DockMargin), typeof(double), typeof(ViewCube), new FrameworkPropertyMetadata(10.0));

    /// <summary>Identifies the <see cref="Size"/> property.</summary>
    public static readonly DependencyProperty SizeProperty = DependencyProperty.Register(
        nameof(Size), typeof(ViewCubeSize), typeof(ViewCube),
        new FrameworkPropertyMetadata(ViewCubeSize.Automatic, FrameworkPropertyMetadataOptions.AffectsMeasure));

    /// <summary>Identifies the <see cref="InactiveOpacity"/> property.</summary>
    public static readonly DependencyProperty InactiveOpacityProperty = DependencyProperty.Register(
        nameof(InactiveOpacity), typeof(double), typeof(ViewCube),
        new FrameworkPropertyMetadata(50.0, OnInactiveOpacityChanged));

    /// <summary>Identifies the <see cref="ShowCompass"/> property.</summary>
    public static readonly DependencyProperty ShowCompassProperty = DependencyProperty.Register(
        nameof(ShowCompass), typeof(bool), typeof(ViewCube),
        new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Identifies the <see cref="SnapToClosestView"/> property.</summary>
    public static readonly DependencyProperty SnapToClosestViewProperty = DependencyProperty.Register(
        nameof(SnapToClosestView), typeof(bool), typeof(ViewCube), new FrameworkPropertyMetadata(true));

    /// <summary>Identifies the <see cref="SnapAngle"/> property.</summary>
    public static readonly DependencyProperty SnapAngleProperty = DependencyProperty.Register(
        nameof(SnapAngle), typeof(double), typeof(ViewCube), new FrameworkPropertyMetadata(10.0));

    /// <summary>Identifies the <see cref="FitToViewOnChange"/> property.</summary>
    public static readonly DependencyProperty FitToViewOnChangeProperty = DependencyProperty.Register(
        nameof(FitToViewOnChange), typeof(bool), typeof(ViewCube), new FrameworkPropertyMetadata(true));

    /// <summary>Identifies the <see cref="AnimatedTransitions"/> property.</summary>
    public static readonly DependencyProperty AnimatedTransitionsProperty = DependencyProperty.Register(
        nameof(AnimatedTransitions), typeof(bool), typeof(ViewCube), new FrameworkPropertyMetadata(true));

    /// <summary>Identifies the <see cref="AnimationDuration"/> property.</summary>
    public static readonly DependencyProperty AnimationDurationProperty = DependencyProperty.Register(
        nameof(AnimationDuration), typeof(double), typeof(ViewCube), new FrameworkPropertyMetadata(500.0));

    /// <summary>Identifies the <see cref="KeepSceneUpright"/> property.</summary>
    public static readonly DependencyProperty KeepSceneUprightProperty = DependencyProperty.Register(
        nameof(KeepSceneUpright), typeof(bool), typeof(ViewCube), new FrameworkPropertyMetadata(true));

    /// <summary>Identifies the <see cref="ClickThreshold"/> property.</summary>
    public static readonly DependencyProperty ClickThresholdProperty = DependencyProperty.Register(
        nameof(ClickThreshold), typeof(double), typeof(ViewCube), new FrameworkPropertyMetadata(4.0));

    /// <summary>Identifies the <see cref="FaceBrush"/> property.</summary>
    public static readonly DependencyProperty FaceBrushProperty = DependencyProperty.Register(
        nameof(FaceBrush), typeof(Brush), typeof(ViewCube),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Identifies the <see cref="EdgeBrush"/> property.</summary>
    public static readonly DependencyProperty EdgeBrushProperty = DependencyProperty.Register(
        nameof(EdgeBrush), typeof(Brush), typeof(ViewCube),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Identifies the <see cref="BorderBrush"/> property.</summary>
    public static readonly DependencyProperty BorderBrushProperty = DependencyProperty.Register(
        nameof(BorderBrush), typeof(Brush), typeof(ViewCube),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Identifies the <see cref="TextBrush"/> property.</summary>
    public static readonly DependencyProperty TextBrushProperty = DependencyProperty.Register(
        nameof(TextBrush), typeof(Brush), typeof(ViewCube),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Identifies the <see cref="HighlightBrush"/> property.</summary>
    public static readonly DependencyProperty HighlightBrushProperty = DependencyProperty.Register(
        nameof(HighlightBrush), typeof(Brush), typeof(ViewCube),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Identifies the <see cref="ArrowBrush"/> property.</summary>
    public static readonly DependencyProperty ArrowBrushProperty = DependencyProperty.Register(
        nameof(ArrowBrush), typeof(Brush), typeof(ViewCube),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Identifies the <see cref="CompassBrush"/> property.</summary>
    public static readonly DependencyProperty CompassBrushProperty = DependencyProperty.Register(
        nameof(CompassBrush), typeof(Brush), typeof(ViewCube),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Identifies the <see cref="CompassTextBrush"/> property.</summary>
    public static readonly DependencyProperty CompassTextBrushProperty = DependencyProperty.Register(
        nameof(CompassTextBrush), typeof(Brush), typeof(ViewCube),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Identifies the <see cref="TopLabel"/> property.</summary>
    public static readonly DependencyProperty TopLabelProperty = DependencyProperty.Register(
        nameof(TopLabel), typeof(string), typeof(ViewCube),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Identifies the <see cref="BottomLabel"/> property.</summary>
    public static readonly DependencyProperty BottomLabelProperty = DependencyProperty.Register(
        nameof(BottomLabel), typeof(string), typeof(ViewCube),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Identifies the <see cref="FrontLabel"/> property.</summary>
    public static readonly DependencyProperty FrontLabelProperty = DependencyProperty.Register(
        nameof(FrontLabel), typeof(string), typeof(ViewCube),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Identifies the <see cref="BackLabel"/> property.</summary>
    public static readonly DependencyProperty BackLabelProperty = DependencyProperty.Register(
        nameof(BackLabel), typeof(string), typeof(ViewCube),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Identifies the <see cref="LeftLabel"/> property.</summary>
    public static readonly DependencyProperty LeftLabelProperty = DependencyProperty.Register(
        nameof(LeftLabel), typeof(string), typeof(ViewCube),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Identifies the <see cref="RightLabel"/> property.</summary>
    public static readonly DependencyProperty RightLabelProperty = DependencyProperty.Register(
        nameof(RightLabel), typeof(string), typeof(ViewCube),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Identifies the <see cref="AlwaysShowTools"/> property.</summary>
    public static readonly DependencyProperty AlwaysShowToolsProperty = DependencyProperty.Register(
        nameof(AlwaysShowTools), typeof(bool), typeof(ViewCube),
        new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>Identifies the <see cref="DragSensitivity"/> property.</summary>
    public static readonly DependencyProperty DragSensitivityProperty = DependencyProperty.Register(
        nameof(DragSensitivity), typeof(double), typeof(ViewCube),
        new FrameworkPropertyMetadata(0.65, null, CoerceSensitivity));

    private static object CoerceSensitivity(DependencyObject d, object baseValue) =>
        baseValue is double val ? Math.Max(0.1, Math.Min(3.0, val)) : 0.65;

    /// <summary>Raised before a transition runs; cancellable, and the target may be replaced.</summary>
    public event EventHandler<ViewRequestedEventArgs>? ViewRequested;

    /// <summary>Raised when the pointer moves onto or off a zone.</summary>
    public event EventHandler<RegionHoverEventArgs>? RegionHovered;

    /// <summary>Raised when a transition finished, or when a change was applied without animating.</summary>
    public event EventHandler? ViewTransitionCompleted;

    /// <summary>Raised after "Set Current View as Home" stored a new home.</summary>
    public event EventHandler? HomeChanged;

    /// <summary>Raised after the front angle changed, by "Set Current View as Front" or "Reset Front".</summary>
    public event EventHandler? FrontChanged;

    /// <summary>Raised when the projection is chosen from the context menu.</summary>
    public event EventHandler<ProjectionModeEventArgs>? ProjectionModeChanged;

    /// <summary>Raised by the "Options…" entry, for a host that owns a settings dialog.</summary>
    public event EventHandler? OptionsRequested;

    /// <summary>Creates a cube that shows the isometric corner until a host or a rotation is assigned.</summary>
    public ViewCube()
    {
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
        ApplyOpacity();
        ApplyDock();
    }

    /// <summary>
    /// The camera the cube mirrors. Assign the viewport's camera and the cube follows it; leave it
    /// null and the control shows <see cref="Rotation"/> on its own, which is what a designer surface
    /// or a test needs.
    /// </summary>
    public ICameraHost? Host
    {
        get => (ICameraHost?)GetValue(HostProperty);
        set => SetValue(HostProperty, value);
    }

    /// <summary>
    /// The orientation drawn when there is no <see cref="Host"/>. With a host the value tracks the
    /// camera, so switching the host off keeps the pose instead of jumping back to the default.
    /// </summary>
    public Quaternion Rotation
    {
        get => (Quaternion)GetValue(RotationProperty);
        set => SetValue(RotationProperty, value);
    }

    /// <summary>The view the home button returns to; the Top-Front-Right corner by default.</summary>
    public Quaternion HomeRotation
    {
        get => (Quaternion)GetValue(HomeRotationProperty);
        set => SetValue(HomeRotationProperty, value);
    }

    /// <summary>
    /// Which way the model's front faces, in degrees about the vertical axis. "Set Current View as
    /// Front" turns it, and the FRONT caption and the compass turn with it.
    /// </summary>
    public double FrontAngle
    {
        get => (double)GetValue(FrontAngleProperty);
        set => SetValue(FrontAngleProperty, value);
    }

    /// <summary>Colours; null falls back to the theme's <c>CONCOST.Brush.ViewCube*</c> resources.</summary>
    public ViewCubeTheme? Theme
    {
        get => (ViewCubeTheme?)GetValue(ThemeProperty);
        set => SetValue(ThemeProperty, value);
    }

    /// <summary>Whether the cube is on screen at all.</summary>
    public bool Visible
    {
        get => (bool)GetValue(VisibleProperty);
        set => SetValue(VisibleProperty, value);
    }

    /// <summary>Which corner of the viewport the cube docks to.</summary>
    public ViewCubePosition Position
    {
        get => (ViewCubePosition)GetValue(PositionProperty);
        set => SetValue(PositionProperty, value);
    }

    /// <summary>Distance from the viewport's edges, in pixels. The <c>Margin</c> of the specification.</summary>
    public double DockMargin
    {
        get => (double)GetValue(DockMarginProperty);
        set => SetValue(DockMarginProperty, value);
    }

    /// <summary>Overall size of the widget: the cube is fitted inside it, compass and buttons included.</summary>
    public ViewCubeSize Size
    {
        get => (ViewCubeSize)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    /// <summary>Opacity, in percent, while the pointer is elsewhere; 100 while it is over the cube.</summary>
    public double InactiveOpacity
    {
        get => (double)GetValue(InactiveOpacityProperty);
        set => SetValue(InactiveOpacityProperty, value);
    }

    /// <summary>Draws the compass ring with N, E, S and W under the cube.</summary>
    public bool ShowCompass
    {
        get => (bool)GetValue(ShowCompassProperty);
        set => SetValue(ShowCompassProperty, value);
    }

    /// <summary>Snaps to the nearest of the 26 views when a drag ends close to one.</summary>
    public bool SnapToClosestView
    {
        get => (bool)GetValue(SnapToClosestViewProperty);
        set => SetValue(SnapToClosestViewProperty, value);
    }

    /// <summary>How close, in degrees, a drag has to end for <see cref="SnapToClosestView"/> to pull it in.</summary>
    public double SnapAngle
    {
        get => (double)GetValue(SnapAngleProperty);
        set => SetValue(SnapAngleProperty, value);
    }

    /// <summary>Asks the host to frame the model after a view change.</summary>
    public bool FitToViewOnChange
    {
        get => (bool)GetValue(FitToViewOnChangeProperty);
        set => SetValue(FitToViewOnChangeProperty, value);
    }

    /// <summary>Runs a transition instead of jumping straight to the target.</summary>
    public bool AnimatedTransitions
    {
        get => (bool)GetValue(AnimatedTransitionsProperty);
        set => SetValue(AnimatedTransitionsProperty, value);
    }

    /// <summary>Base duration of a transition, in milliseconds; a quarter turn takes this long.</summary>
    public double AnimationDuration
    {
        get => (double)GetValue(AnimationDurationProperty);
        set => SetValue(AnimationDurationProperty, value);
    }

    /// <summary>Forbids roll while dragging: screen up stays the projection of world up.</summary>
    public bool KeepSceneUpright
    {
        get => (bool)GetValue(KeepSceneUprightProperty);
        set => SetValue(KeepSceneUprightProperty, value);
    }

    /// <summary>How far, in pixels, the pointer may travel before a press becomes a drag.</summary>
    public double ClickThreshold
    {
        get => (double)GetValue(ClickThresholdProperty);
        set => SetValue(ClickThresholdProperty, value);
    }

    /// <summary>Custom brush for the cube faces.</summary>
    public Brush? FaceBrush
    {
        get => (Brush?)GetValue(FaceBrushProperty);
        set => SetValue(FaceBrushProperty, value);
    }

    /// <summary>Custom brush for the bevel edge lines.</summary>
    public Brush? EdgeBrush
    {
        get => (Brush?)GetValue(EdgeBrushProperty);
        set => SetValue(EdgeBrushProperty, value);
    }

    /// <summary>Custom brush for outer border and compass ring.</summary>
    public Brush? BorderBrush
    {
        get => (Brush?)GetValue(BorderBrushProperty);
        set => SetValue(BorderBrushProperty, value);
    }

    /// <summary>Custom brush for text labels.</summary>
    public Brush? TextBrush
    {
        get => (Brush?)GetValue(TextBrushProperty);
        set => SetValue(TextBrushProperty, value);
    }

    /// <summary>Custom brush for hovered part highlight.</summary>
    public Brush? HighlightBrush
    {
        get => (Brush?)GetValue(HighlightBrushProperty);
        set => SetValue(HighlightBrushProperty, value);
    }

    /// <summary>Custom brush for step arrows and roll buttons.</summary>
    public Brush? ArrowBrush
    {
        get => (Brush?)GetValue(ArrowBrushProperty);
        set => SetValue(ArrowBrushProperty, value);
    }

    /// <summary>Custom brush for compass ring.</summary>
    public Brush? CompassBrush
    {
        get => (Brush?)GetValue(CompassBrushProperty);
        set => SetValue(CompassBrushProperty, value);
    }

    /// <summary>Custom brush for compass direction letters.</summary>
    public Brush? CompassTextBrush
    {
        get => (Brush?)GetValue(CompassTextBrushProperty);
        set => SetValue(CompassTextBrushProperty, value);
    }

    /// <summary>Custom label for the Top face (default: TOP).</summary>
    public string? TopLabel
    {
        get => (string?)GetValue(TopLabelProperty);
        set => SetValue(TopLabelProperty, value);
    }

    /// <summary>Custom label for the Bottom face (default: BOTTOM).</summary>
    public string? BottomLabel
    {
        get => (string?)GetValue(BottomLabelProperty);
        set => SetValue(BottomLabelProperty, value);
    }

    /// <summary>Custom label for the Front face (default: FRONT).</summary>
    public string? FrontLabel
    {
        get => (string?)GetValue(FrontLabelProperty);
        set => SetValue(FrontLabelProperty, value);
    }

    /// <summary>Custom label for the Back face (default: BACK).</summary>
    public string? BackLabel
    {
        get => (string?)GetValue(BackLabelProperty);
        set => SetValue(BackLabelProperty, value);
    }

    /// <summary>Custom label for the Left face (default: LEFT).</summary>
    public string? LeftLabel
    {
        get => (string?)GetValue(LeftLabelProperty);
        set => SetValue(LeftLabelProperty, value);
    }

    /// <summary>Custom label for the Right face (default: RIGHT).</summary>
    public string? RightLabel
    {
        get => (string?)GetValue(RightLabelProperty);
        set => SetValue(RightLabelProperty, value);
    }

    /// <summary>Whether helper tools (Home, Roll, 90-degree step arrows, Context menu) remain visible even without hovering.</summary>
    public bool AlwaysShowTools
    {
        get => (bool)GetValue(AlwaysShowToolsProperty);
        set => SetValue(AlwaysShowToolsProperty, value);
    }

    /// <summary>Multiplier for mouse drag orbit rotation sensitivity (default: 0.65 for smooth, controlled motion).</summary>
    public double DragSensitivity
    {
        get => (double)GetValue(DragSensitivityProperty);
        set => SetValue(DragSensitivityProperty, value);
    }

    /// <summary>Animates the view back to the designated Home orientation.</summary>
    public void ResetToHome() => Request(ViewCubeHit.ForHome(), HomeRotation);

    /// <summary>Animates the view to a standard orthographic face view (TOP, BOTTOM, FRONT, BACK, LEFT, RIGHT).</summary>
    public void GoToFace(string faceName)
    {
        foreach (var region in ViewCubeMath.Regions)
        {
            if (region.IsFace && string.Equals(region.Name, faceName, StringComparison.OrdinalIgnoreCase))
            {
                Request(ViewCubeHit.ForRegion(region), ViewCubeMath.RotationFor(region, FrontAngle));
                break;
            }
        }
    }

    /// <summary>The projection the user picked from the menu; the host applies it.</summary>
    public ProjectionMode Projection
    {
        get => _projection;
        set => SetProjection(value);
    }

    /// <summary>What the pointer is over, or <see cref="ViewCubeHit.Empty"/>.</summary>
    public ViewCubeHit HoveredRegion => _hover;

    /// <summary>The zone the cube is drawn from, whichever of the host and the local rotation has it.</summary>
    public Quaternion CurrentRotation
    {
        get
        {
            ICameraHost? host = Host;
            return ViewCubeMath.NormalizeRotation(host is null ? Rotation : host.Rotation);
        }
    }

    /// <summary>Redraws from the host's current rotation; call it from a host that cannot raise changes.</summary>
    public void Invalidate()
    {
        DropProjection();
        InvalidateVisual();
    }

    /// <summary>Turns to <paramref name="region"/>, animating unless transitions are off.</summary>
    public void GoTo(ViewCubeRegion region)
    {
        if (region.IsEmpty) return;

        Request(ViewCubeHit.ForRegion(region), ViewCubeMath.RotationFor(region, FrontAngle));
    }

    /// <summary>Returns to <see cref="HomeRotation"/>.</summary>
    public void GoHome() => Request(ViewCubeHit.ForHome(), ViewCubeMath.NormalizeRotation(HomeRotation));

    /// <summary>Stores the current orientation as the one the home button returns to.</summary>
    public void SetCurrentViewAsHome()
    {
        HomeRotation = CurrentRotation;
        HomeChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Declares the current heading to be the model's front.</summary>
    public void SetCurrentViewAsFront()
    {
        ViewCubeMath.ToBasis(CurrentRotation, out _, out Vector3D up, out Vector3D forward);

        var eye = -forward;
        double x = eye.X, y = eye.Y;

        // Straight down there is no heading on screen, so the top of the screen is taken as north.
        if (Math.Sqrt(x * x + y * y) < 1e-6)
        {
            x = up.X;
            y = up.Y;
        }

        double heading = Math.Atan2(y, x) * 180 / Math.PI;

        // FRONT looks out of the -Y side, i.e. along -90 degrees; the front angle is what turns that
        // onto the heading the user is looking from.
        FrontAngle = NormaliseAngle(heading + 90);
        FrontChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Puts the model's front back where the convention says it is.</summary>
    public void ResetFront()
    {
        if (Math.Abs(FrontAngle) < 1e-9) return;

        FrontAngle = 0;
        FrontChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Turns the camera about its own axis by a quarter turn, positive clockwise.</summary>
    public void RollBy(int quarterTurns)
    {
        if (quarterTurns == 0) return;

        int direction = Math.Sign(quarterTurns);
        Request(ViewCubeHit.ForRoll(direction), ViewCubeMath.Roll(CurrentRotation, 90 * direction));
    }

    /// <summary>Turns to the face next to the one being looked at square on.</summary>
    public void StepToNeighbor(ArrowDirection direction)
    {
        Quaternion target;

        if (ViewCubeMath.TryFace(CurrentRotation, 0.5, out _))
        {
            // The arrows point at the faces that are up, right, down and left on screen. With the front
            // turned, that is the world axis turned back by the same angle.
            var axis = direction switch
            {
                ArrowDirection.Up => new Vector3D(0, 0, 1),
                ArrowDirection.Down => new Vector3D(0, 0, -1),
                ArrowDirection.Left => new Vector3D(-1, 0, 0),
                _ => new Vector3D(1, 0, 0)
            };

            target = ViewCubeMath.RotationFor(ViewCubeMath.Closest(ViewCubeMath.Rotate(axis, ViewCubeMath.WorldUp, -FrontAngle)), FrontAngle);
        }
        else
        {
            ViewCubeMath.ToBasis(CurrentRotation, out Vector3D right, out Vector3D up, out _);

            var eye = direction switch
            {
                ArrowDirection.Up => up,
                ArrowDirection.Down => -up,
                ArrowDirection.Left => -right,
                _ => right
            };

            target = ViewCubeMath.RotationFromEye(eye);
        }

        Request(ViewCubeHit.ForArrow(direction), target);
    }

    /// <summary>Picks a projection from code, as the context menu does.</summary>
    public void SetProjection(ProjectionMode mode)
    {
        if (_projection == mode) return;

        _projection = mode;

        if (Host is not null) Host.Projection = mode;

        ProjectionModeChanged?.Invoke(this, new ProjectionModeEventArgs(mode));
    }

    // ------------------------------------------------------------------ host plumbing

    private static void OnHostChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var cube = (ViewCube)d;

        if (e.OldValue is ICameraHostEvents previous) previous.CameraChanged -= cube.OnHostCameraChanged;
        if (e.NewValue is ICameraHostEvents next) next.CameraChanged += cube.OnHostCameraChanged;

        if (e.NewValue is ICameraHost host)
        {
            cube.SetCurrentValue(RotationProperty, ViewCubeMath.NormalizeRotation(host.Rotation));
            cube._projection = host.Projection;
        }

        cube.Invalidate();
    }

    private void OnHostCameraChanged(object? sender, EventArgs e) => Invalidate();

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (_pointerOver) Opacity = 1;
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        StopAnimation();
    }

    private static void OnInactiveOpacityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
        ((ViewCube)d).ApplyOpacity();

    private void ApplyOpacity()
    {
        Opacity = _pointerOver ? 1 : Math.Max(0, Math.Min(1, InactiveOpacity / 100));
    }

    private void ApplyDock()
    {
        switch (Position)
        {
            case ViewCubePosition.TopLeft:
                HorizontalAlignment = HorizontalAlignment.Left;
                VerticalAlignment = VerticalAlignment.Top;
                break;

            case ViewCubePosition.BottomLeft:
                HorizontalAlignment = HorizontalAlignment.Left;
                VerticalAlignment = VerticalAlignment.Bottom;
                break;

            case ViewCubePosition.BottomRight:
                HorizontalAlignment = HorizontalAlignment.Right;
                VerticalAlignment = VerticalAlignment.Bottom;
                break;

            default:
                HorizontalAlignment = HorizontalAlignment.Right;
                VerticalAlignment = VerticalAlignment.Top;
                break;
        }

        Margin = new Thickness(Math.Max(0, DockMargin));
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.Property == PositionProperty || e.Property == DockMarginProperty) ApplyDock();

        if (e.Property == VisibleProperty)
        {
            Visibility = Visible ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    // ------------------------------------------------------------------ transitions

    private void Request(ViewCubeHit hit, Quaternion target)
    {
        var args = new ViewRequestedEventArgs(hit, target);
        ViewRequested?.Invoke(this, args);

        if (args.Cancel) return;

        AnimateTo(args.Target);
    }

    private void AnimateTo(Quaternion target)
    {
        StopAnimation();

        target = ViewCubeMath.NormalizeRotation(target);
        Quaternion from = CurrentRotation;

        if (!AnimatedTransitions)
        {
            ApplyRotation(target);
            Complete();
            return;
        }

        double angle = ViewCubeMath.AngleBetween(from, target);
        if (angle < 0.05)
        {
            ApplyRotation(target);
            Complete();
            return;
        }

        ToBasisForward(target, out double elevation);

        _animationFrom = from;
        _animationTo = target;
        _animationUpright = KeepSceneUpright && elevation < 89.9;
        _animationDuration = Math.Max(200, Math.Min(600, AnimationDuration * angle / 90));
        _animationStart = Stopwatch.GetTimestamp();

        _timer = new DispatcherTimer(DispatcherPriority.Render) { Interval = TimeSpan.FromMilliseconds(16) };
        _timer.Tick += OnAnimationTick;
        _timer.Start();
    }

    private void OnAnimationTick(object? sender, EventArgs e)
    {
        double elapsed = (Stopwatch.GetTimestamp() - _animationStart) * 1000.0 / Stopwatch.Frequency;
        double t = Math.Min(1, elapsed / Math.Max(1, _animationDuration));

        Quaternion step = ViewCubeMath.Slerp(_animationFrom, _animationTo, ViewCubeMath.SmoothStep(t));

        // Upright all the way through, not just at the ends: a slerp between two upright poses can dip
        // through a rolled one, and the model visibly tilts for a few frames when it does.
        ApplyRotation(_animationUpright ? ViewCubeMath.Upright(step) : step);

        if (t < 1) return;

        ApplyRotation(_animationTo);
        StopAnimation();
        Complete();
    }

    private void StopAnimation()
    {
        DispatcherTimer? timer = _timer;

        if (timer is null) return;

        _timer = null;
        timer.Stop();
        timer.Tick -= OnAnimationTick;
    }

    private void Complete()
    {
        ViewTransitionCompleted?.Invoke(this, EventArgs.Empty);

        if (!FitToViewOnChange) return;

        Host?.FitToView();
    }

    private void ApplyRotation(Quaternion rotation)
    {
        SetCurrentValue(RotationProperty, rotation);

        if (Host is { } host)
        {
            host.Rotation = rotation;

            // The automatic projection is the cube's promise, so it is the cube that asks for the
            // orthographic matrix whenever a face ends up square on.
            if (_projection == ProjectionMode.PerspectiveWithOrthoFaces)
            {
                ProjectionMode wanted = ViewCubeMath.TryFace(rotation, 0.5, out _)
                    ? ProjectionMode.Orthographic
                    : ProjectionMode.PerspectiveWithOrthoFaces;

                if (host.Projection != wanted) host.Projection = wanted;
            }
        }

        DropProjection();
        InvalidateVisual();
    }

    private static void ToBasisForward(Quaternion rotation, out double elevation)
    {
        ViewCubeMath.ToBasis(rotation, out _, out _, out Vector3D forward);
        elevation = Math.Abs(Math.Asin(Math.Min(1, Math.Max(-1, forward.Z)))) * 180 / Math.PI;
    }

    private static Quaternion DefaultHome() =>
        ViewCubeMath.RotationFor(ViewCubeMath.Closest(ViewCubeMath.DefaultEye));

    private static double NormaliseAngle(double degrees)
    {
        double next = ((degrees % 360) + 360) % 360;
        return next >= 180 ? next - 360 : next;
    }

    // ------------------------------------------------------------------ context menu

    /// <summary>Opens the menu under the widget, or wherever the right button asked for it.</summary>
    public void OpenContextMenu()
    {
        ContextMenu menu = BuildMenu();
        menu.PlacementTarget = this;
        menu.Placement = PlacementMode.Bottom;
        menu.IsOpen = true;
    }

    private ContextMenu BuildMenu()
    {
        var menu = new ContextMenu();

        menu.Items.Add(Item("Go Home", () => GoHome()));
        menu.Items.Add(new Separator());

        foreach (ProjectionMode mode in new[]
                 {
                     ProjectionMode.Orthographic,
                     ProjectionMode.Perspective,
                     ProjectionMode.PerspectiveWithOrthoFaces
                 })
        {
            ProjectionMode captured = mode;
            var item = Item(Describe(mode), () => SetProjection(captured));
            item.IsCheckable = true;
            item.IsChecked = _projection == mode;
            menu.Items.Add(item);
        }

        menu.Items.Add(new Separator());
        menu.Items.Add(Item("Set Current View as Home", () => SetCurrentViewAsHome()));
        menu.Items.Add(Item("Set Current View as Front", () => SetCurrentViewAsFront()));
        menu.Items.Add(Item("Reset Front", () => ResetFront()));
        menu.Items.Add(new Separator());

        var compass = Item("Show Compass", () => ShowCompass = !ShowCompass);
        compass.IsCheckable = true;
        compass.IsChecked = ShowCompass;
        menu.Items.Add(compass);

        menu.Items.Add(new Separator());
        menu.Items.Add(Item("Options…", () => OptionsRequested?.Invoke(this, EventArgs.Empty)));

        return menu;
    }

    private static string Describe(ProjectionMode mode) => mode switch
    {
        ProjectionMode.Orthographic => "Orthographic",
        ProjectionMode.Perspective => "Perspective",
        _ => "Perspective with Ortho Faces"
    };

    private static MenuItem Item(string header, Action action)
    {
        var item = new MenuItem { Header = header };

        item.Click += (_, _) => action();
        return item;
    }

    // ------------------------------------------------------------------ pointer

    private enum DragMode
    {
        None,
        Cube,
        Compass
    }

    /// <inheritdoc />
    protected override void OnMouseEnter(MouseEventArgs e)
    {
        base.OnMouseEnter(e);

        if (_pointerOver) return;

        _pointerOver = true;
        ApplyOpacity();
        InvalidateVisual();
    }

    /// <inheritdoc />
    protected override void OnMouseLeave(MouseEventArgs e)
    {
        base.OnMouseLeave(e);

        if (!_pointerOver) return;

        _pointerOver = false;

        // A drag keeps the buttons up even if the pointer has left the square, because the capture keeps
        // sending moves here and the arrows are what the user is aiming for.
        if (_drag == DragMode.None) ApplyOpacity();

        UpdateHover(ViewCubeHit.Empty);
        InvalidateVisual();
    }

    /// <inheritdoc />
    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);

        Point point = e.GetPosition(this);

        if (_drag != DragMode.None && e.LeftButton == MouseButtonState.Pressed)
        {
            if (!_dragged)
            {
                var travel = point - _dragStart;
                _dragged = Math.Sqrt(travel.X * travel.X + travel.Y * travel.Y) > Math.Max(1, ClickThreshold);
            }

            if (_dragged)
            {
                if (Cursor != Cursors.SizeAll) Cursor = Cursors.SizeAll;
                if (_drag == DragMode.Cube) Orbit(point);
                else TurnCompass(point);

                e.Handled = true;
                return;
            }
        }

        UpdateHover(HitTest(point));
    }

    /// <inheritdoc />
    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);

        Point point = e.GetPosition(this);
        ViewCubeHit hit = HitTest(point);

        if (hit.IsEmpty)
        {
            double side = ControlSide;
            Point centre = Centre(side);
            double dx = point.X - centre.X;
            double dy = point.Y - centre.Y;
            if (dx * dx + dy * dy > (side * 0.58) * (side * 0.58)) return;

            _pressed = ViewCubeHit.Empty;
            _drag = DragMode.Cube;
        }
        else
        {
            // A press on a button is a click and nothing else; a press on the cube is a drag until it ends.
            _pressed = hit;
            _drag = hit.IsRegion ? DragMode.Cube : hit.Part == ViewCubePart.Compass ? DragMode.Compass : DragMode.None;
        }
        _dragStart = point;
        _dragRotation = CurrentRotation;
        _dragAngle = AngleAroundCentre(point);
        _dragged = false;

        // Taking the animation's place here is what makes an interrupted transition continue from where
        // it was instead of snapping to the target first.
        StopAnimation();

        CaptureMouse();
        e.Handled = true;
    }

    /// <inheritdoc />
    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonUp(e);

        if (_drag != DragMode.None || !_pressed.IsEmpty) ReleaseMouseCapture();

        ViewCubeHit pressed = _pressed;
        bool dragged = _dragged;
        DragMode mode = _drag;

        _pressed = ViewCubeHit.Empty;
        _drag = DragMode.None;
        _dragged = false;
        Cursor = null;

        if (!_pointerOver) ApplyOpacity();

        if (dragged)
        {
            if (mode == DragMode.Cube) SnapToNearestView();
            e.Handled = true;
            return;
        }

        if (pressed.IsEmpty) return;

        Activate(pressed);
        e.Handled = true;
    }

    /// <inheritdoc />
    protected override void OnMouseRightButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseRightButtonDown(e);

        if (HitTest(e.GetPosition(this)).IsEmpty) return;

        OpenContextMenu();
        e.Handled = true;
    }

    private void Activate(ViewCubeHit hit)
    {
        switch (hit.Part)
        {
            case ViewCubePart.Face:
            case ViewCubePart.Edge:
            case ViewCubePart.Corner:
                GoTo(hit.Region);
                break;

            case ViewCubePart.Home:
                GoHome();
                break;

            case ViewCubePart.Arrow:
                StepToNeighbor(hit.Arrow);
                break;

            case ViewCubePart.Roll:
                RollBy(hit.Roll);
                break;

            case ViewCubePart.Compass:
                LookFromCompass(hit);
                break;

            case ViewCubePart.Menu:
                OpenContextMenu();
                break;
        }
    }

    private void LookFromCompass(ViewCubeHit hit)
    {
        Vector3D direction = hit.Compass;
        direction = ViewCubeMath.Normalize(new Vector3D(direction.X, direction.Y, 0));

        // Clicking a letter keeps the elevation: the compass only says which way to stand, not how high.
        ViewCubeMath.ToBasis(CurrentRotation, out _, out _, out Vector3D forward);
        double elevation = Math.Asin(Math.Min(1, Math.Max(-1, -forward.Z)));

        var eye = new Vector3D(
            direction.X * Math.Cos(elevation),
            direction.Y * Math.Cos(elevation),
            Math.Sin(elevation));

        Request(hit, ViewCubeMath.RotationFromEye(eye));
    }

    private void Orbit(Point point)
    {
        double unit = (180 / Math.Max(1, ControlSide)) * DragSensitivity;
        double yaw = -(point.X - _dragStart.X) * unit;
        double pitch = -(point.Y - _dragStart.Y) * unit;

        ViewCubeMath.ToBasis(_dragRotation, out _, out Vector3D up, out Vector3D forward);

        var turned = ViewCubeMath.Rotate(forward, ViewCubeMath.WorldUp, yaw);
        var turnedUp = ViewCubeMath.Rotate(up, ViewCubeMath.WorldUp, yaw);
        var right = ViewCubeMath.RightFor(turned, turnedUp);

        if (KeepSceneUpright)
        {
            // Stop just short of the pole: exactly above, "which way is up on screen" has no answer and
            // the view would flip as the pointer crossed it.
            double elevation = ViewCubeMath.Elevation(-turned);
            double next = Math.Max(-89.5, Math.Min(89.5, elevation - pitch));
            pitch += next - elevation;
        }

        var nextForward = ViewCubeMath.Rotate(turned, right, -pitch);
        var nextUp = ViewCubeMath.Rotate(turnedUp, right, -pitch);

        if (KeepSceneUpright) nextUp = ViewCubeMath.UprightUp(nextForward, nextUp);

        ApplyRotation(ViewCubeMath.FromBasis(ViewCubeMath.RightFor(nextForward, nextUp), nextUp, nextForward));
    }

    private void TurnCompass(Point point)
    {
        double swept = AngleAroundCentre(point) - _dragAngle;

        // The compass is a dial on the ground: only the heading changes, the elevation stays put.
        ViewCubeMath.ToBasis(_dragRotation, out Vector3D right, out Vector3D up, out Vector3D forward);

        ApplyRotation(ViewCubeMath.FromBasis(
            ViewCubeMath.Rotate(right, ViewCubeMath.WorldUp, -swept),
            ViewCubeMath.Rotate(up, ViewCubeMath.WorldUp, -swept),
            ViewCubeMath.Rotate(forward, ViewCubeMath.WorldUp, -swept)));
    }

    private void SnapToNearestView()
    {
        Quaternion current = CurrentRotation;

        if (!SnapToClosestView) return;

        ViewCubeRegion region = ViewCubeMath.Closest(current);

        if (region.IsEmpty) return;

        Quaternion target = ViewCubeMath.RotationFor(region, FrontAngle);

        if (ViewCubeMath.AngleBetween(current, target) > Math.Max(0.5, SnapAngle)) return;

        Request(ViewCubeHit.ForRegion(region), target);
    }

    /// <summary>Angle of a point round the control's centre, in degrees, clockwise from screen right.</summary>
    private double AngleAroundCentre(Point point)
    {
        double side = ControlSide;
        double centreX = ActualWidth > 0 ? ActualWidth / 2 : side / 2;
        double centreY = ActualHeight > 0 ? ActualHeight / 2 : side / 2;

        return Math.Atan2(point.Y - centreY, point.X - centreX) * 180 / Math.PI;
    }

    private void UpdateHover(ViewCubeHit hit)
    {
        if (hit.Part == _hover.Part && string.Equals(hit.Label, _hover.Label, StringComparison.Ordinal)) return;

        _hover = hit;
        Cursor = hit.IsEmpty ? null : Cursors.Hand;

        RegionHovered?.Invoke(this, new RegionHoverEventArgs(hit));
        InvalidateVisual();
    }
}
