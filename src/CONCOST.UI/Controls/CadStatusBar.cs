using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using CONCOST.UI.AttachedProperties;
using CONCOST.UI.Icons;

namespace CONCOST.UI.Controls;

/// <summary>The drafting settings a status bar can toggle.</summary>
public enum CadToggle
{
    /// <summary>Pull the cursor onto snap points.</summary>
    Snap,

    /// <summary>Show the drawing grid.</summary>
    Grid,

    /// <summary>Constrain drawing to horizontal and vertical.</summary>
    Ortho,

    /// <summary>Constrain drawing to fixed angle increments.</summary>
    Polar,

    /// <summary>Snap to parts of existing geometry — endpoints, midpoints, centres.</summary>
    ObjectSnap
}

/// <summary>Carries the toggle that changed and its new state.</summary>
public class CadToggleEventArgs : EventArgs
{
    public CadToggleEventArgs(CadToggle toggle, bool isEnabled)
    {
        Toggle = toggle;
        IsEnabled = isEnabled;
    }

    /// <summary>Which setting changed.</summary>
    public CadToggle Toggle { get; }

    /// <summary>The new state.</summary>
    public bool IsEnabled { get; }
}

/// <summary>One row of the background task queue.</summary>
public class CadTask
{
    public CadTask(string title, double percent = 0)
    {
        Title = title ?? string.Empty;
        Percent = percent;
    }

    /// <summary>What is running.</summary>
    public string Title { get; set; }

    /// <summary>How far along it is, 0 to 100; ignored when <see cref="IsIndeterminate"/> is set.</summary>
    public double Percent { get; set; }

    /// <summary>Work with no measurable length, e.g. waiting on a service.</summary>
    public bool IsIndeterminate { get; set; }

    /// <summary>Anything the host wants to hang off the row.</summary>
    public object? Tag { get; set; }

    /// <inheritdoc />
    public override string ToString() => Title;
}

/// <summary>How loud a status bar notification is.</summary>
public enum CadNotificationSeverity
{
    Info,
    Success,
    Warning,
    Error
}

/// <summary>One entry of the status bar notification list.</summary>
public class CadNotification
{
    public CadNotification(string title, string message = "", CadNotificationSeverity severity = CadNotificationSeverity.Info)
    {
        Title = title ?? string.Empty;
        Message = message ?? string.Empty;
        Severity = severity;
        Timestamp = DateTime.Now;
    }

    /// <summary>One line summary.</summary>
    public string Title { get; set; }

    /// <summary>The detail, shown under the title.</summary>
    public string Message { get; set; }

    /// <summary>How loud the entry is; drives its icon and colour.</summary>
    public CadNotificationSeverity Severity { get; set; }

    /// <summary>When the entry was raised.</summary>
    public DateTime Timestamp { get; }

    /// <summary>True once the list has been opened and the entry seen.</summary>
    public bool IsRead { get; set; }
}

/// <summary>How a <see cref="CadStatusBar"/> is dressed.</summary>
public enum CadStatusBarVariant
{
    /// <summary>A surface plate with one outline: the bar that sits at the foot of a window.</summary>
    Floating,

    /// <summary>No plate at all, only the flat controls: the bar a host lays into its own strip.</summary>
    Seamless,

    /// <summary>A translucent plate for a view drawn behind it, where an opaque one would hide the model.</summary>
    Glass
}

/// <summary>
/// The drafting status bar: selection summary and work plane on the left, live coordinates in the
/// middle, and the snap toggles, the background task queue and the notification count on the right.
/// </summary>
/// <remarks>
/// <para>
/// The toggles are one bit each and the host owns what they mean; this bar only keeps them in sync
/// with its dependency properties so a ribbon button and a status bar toggle can drive the same
/// setting without either of them owning it.
/// </para>
/// <para>
/// Exactly one outline belongs to the bar: the plate. Five switches live on it permanently, so a
/// solid accent fill on the ones that are on would read as five alarms, and five outlined chips
/// inside an outlined plate would read as a table of boxes. The toggles therefore stay flat until
/// they are hovered and say <em>on</em> with the soft accent tint, and they carry the captions
/// SNAP, GRID, ORTHO, POLAR and OSNAP that make a status bar readable without pointing at it.
/// <see cref="Variant"/> decides whether the plate exists at all; see
/// <see cref="CadStatusBarVariant"/>.
/// </para>
/// </remarks>
public class CadStatusBar : Grid
{
    /// <summary>Identifies the <see cref="SelectionCount"/> property.</summary>
    public static readonly DependencyProperty SelectionCountProperty = DependencyProperty.Register(
        nameof(SelectionCount), typeof(int), typeof(CadStatusBar),
        new FrameworkPropertyMetadata(0, OnSelectionCountChanged));

    /// <summary>Identifies the <see cref="WorkPlane"/> property.</summary>
    public static readonly DependencyProperty WorkPlaneProperty = DependencyProperty.Register(
        nameof(WorkPlane), typeof(UcsMode), typeof(CadStatusBar),
        new FrameworkPropertyMetadata(UcsMode.World, OnWorkPlaneChanged));

    /// <summary>Identifies the <see cref="Coordinates"/> property.</summary>
    public static readonly DependencyProperty CoordinatesProperty = DependencyProperty.Register(
        nameof(Coordinates), typeof(Point3D), typeof(CadStatusBar),
        new FrameworkPropertyMetadata(new Point3D(0, 0, 0), OnCoordinatesChanged));

    /// <summary>Identifies the <see cref="UnitSuffix"/> property.</summary>
    public static readonly DependencyProperty UnitSuffixProperty = DependencyProperty.Register(
        nameof(UnitSuffix), typeof(string), typeof(CadStatusBar),
        new FrameworkPropertyMetadata("mm", OnCoordinatesChanged));

    /// <summary>Identifies the <see cref="ShowCoordinates"/> property.</summary>
    public static readonly DependencyProperty ShowCoordinatesProperty = DependencyProperty.Register(
        nameof(ShowCoordinates), typeof(bool), typeof(CadStatusBar),
        new FrameworkPropertyMetadata(true, OnCoordinatesChanged));

    /// <summary>Identifies the <see cref="SnapEnabled"/> property.</summary>
    public static readonly DependencyProperty SnapEnabledProperty = DependencyProperty.Register(
        nameof(SnapEnabled), typeof(bool), typeof(CadStatusBar),
        new FrameworkPropertyMetadata(false, OnTogglePropertyChanged));

    /// <summary>Identifies the <see cref="GridEnabled"/> property.</summary>
    public static readonly DependencyProperty GridEnabledProperty = DependencyProperty.Register(
        nameof(GridEnabled), typeof(bool), typeof(CadStatusBar),
        new FrameworkPropertyMetadata(true, OnTogglePropertyChanged));

    /// <summary>Identifies the <see cref="OrthoEnabled"/> property.</summary>
    public static readonly DependencyProperty OrthoEnabledProperty = DependencyProperty.Register(
        nameof(OrthoEnabled), typeof(bool), typeof(CadStatusBar),
        new FrameworkPropertyMetadata(false, OnTogglePropertyChanged));

    /// <summary>Identifies the <see cref="PolarEnabled"/> property.</summary>
    public static readonly DependencyProperty PolarEnabledProperty = DependencyProperty.Register(
        nameof(PolarEnabled), typeof(bool), typeof(CadStatusBar),
        new FrameworkPropertyMetadata(false, OnTogglePropertyChanged));

    /// <summary>Identifies the <see cref="ObjectSnapEnabled"/> property.</summary>
    public static readonly DependencyProperty ObjectSnapEnabledProperty = DependencyProperty.Register(
        nameof(ObjectSnapEnabled), typeof(bool), typeof(CadStatusBar),
        new FrameworkPropertyMetadata(true, OnTogglePropertyChanged));

    /// <summary>Identifies the <see cref="Variant"/> property.</summary>
    public static readonly DependencyProperty VariantProperty = DependencyProperty.Register(
        nameof(Variant), typeof(CadStatusBarVariant), typeof(CadStatusBar),
        new FrameworkPropertyMetadata(CadStatusBarVariant.Floating, OnChromeChanged));

    /// <summary>Identifies the <see cref="CornerRadius"/> property.</summary>
    public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register(
        nameof(CornerRadius), typeof(CornerRadius), typeof(CadStatusBar),
        new FrameworkPropertyMetadata(new CornerRadius(6), OnChromeChanged));

    /// <summary>Identifies the <see cref="Elevation"/> property.</summary>
    public static readonly DependencyProperty ElevationProperty = DependencyProperty.Register(
        nameof(Elevation), typeof(int), typeof(CadStatusBar),
        new FrameworkPropertyMetadata(1, OnChromeChanged));

    /// <summary>Identifies the <see cref="ControlHeight"/> property.</summary>
    public static readonly DependencyProperty ControlHeightProperty = DependencyProperty.Register(
        nameof(ControlHeight), typeof(double), typeof(CadStatusBar),
        new FrameworkPropertyMetadata(24.0, FrameworkPropertyMetadataOptions.AffectsMeasure, OnControlHeightChanged));

    /// <summary>Identifies the <see cref="IconSize"/> property.</summary>
    public static readonly DependencyProperty IconSizeProperty = DependencyProperty.Register(
        nameof(IconSize), typeof(double), typeof(CadStatusBar),
        new FrameworkPropertyMetadata(13.0, FrameworkPropertyMetadataOptions.AffectsMeasure, OnControlHeightChanged));

    /// <summary>Identifies the <see cref="ShowDividers"/> property.</summary>
    public static readonly DependencyProperty ShowDividersProperty = DependencyProperty.Register(
        nameof(ShowDividers), typeof(bool), typeof(CadStatusBar),
        new FrameworkPropertyMetadata(true, OnGroupVisibilityChanged));

    /// <summary>Identifies the <see cref="ShowSelection"/> property.</summary>
    public static readonly DependencyProperty ShowSelectionProperty = DependencyProperty.Register(
        nameof(ShowSelection), typeof(bool), typeof(CadStatusBar),
        new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsMeasure, OnGroupVisibilityChanged));

    /// <summary>Identifies the <see cref="ShowWorkPlane"/> property.</summary>
    public static readonly DependencyProperty ShowWorkPlaneProperty = DependencyProperty.Register(
        nameof(ShowWorkPlane), typeof(bool), typeof(CadStatusBar),
        new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsMeasure, OnGroupVisibilityChanged));

    /// <summary>Identifies the <see cref="ShowToggles"/> property.</summary>
    public static readonly DependencyProperty ShowTogglesProperty = DependencyProperty.Register(
        nameof(ShowToggles), typeof(bool), typeof(CadStatusBar),
        new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsMeasure, OnGroupVisibilityChanged));

    /// <summary>Identifies the <see cref="ShowTaskQueue"/> property.</summary>
    public static readonly DependencyProperty ShowTaskQueueProperty = DependencyProperty.Register(
        nameof(ShowTaskQueue), typeof(bool), typeof(CadStatusBar),
        new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsMeasure, OnGroupVisibilityChanged));

    /// <summary>Identifies the <see cref="ShowNotifications"/> property.</summary>
    public static readonly DependencyProperty ShowNotificationsProperty = DependencyProperty.Register(
        nameof(ShowNotifications), typeof(bool), typeof(CadStatusBar),
        new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsMeasure, OnGroupVisibilityChanged));

    /// <summary>Identifies the <see cref="ShowToggleLabels"/> property.</summary>
    public static readonly DependencyProperty ShowToggleLabelsProperty = DependencyProperty.Register(
        nameof(ShowToggleLabels), typeof(bool), typeof(CadStatusBar),
        new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsMeasure, OnGroupVisibilityChanged));

    private static readonly FontFamily MonoFont = new("Consolas");

    private static readonly UcsMode[] Planes = { UcsMode.World, UcsMode.Top, UcsMode.Front, UcsMode.Right };

    /// <summary>The captions a drafting status bar prints beside its switches.</summary>
    private static readonly Dictionary<CadToggle, string> ToggleCaptions = new()
    {
        [CadToggle.Snap] = "SNAP",
        [CadToggle.Grid] = "GRID",
        [CadToggle.Ortho] = "ORTHO",
        [CadToggle.Polar] = "POLAR",
        [CadToggle.ObjectSnap] = "OSNAP"
    };

    private readonly Border _frame;
    private readonly Grid _layout;
    private readonly StackPanel _left;
    private readonly StackPanel _right;
    private readonly StackPanel _toggleStrip;
    private readonly TextBlock _selection;
    private readonly TextBlock _coords;
    private readonly SplitButton _workPlane;
    private readonly TextBlock _workPlaneText;
    private readonly Dictionary<CadToggle, ToggleButton> _toggles = new();
    private readonly Button _taskButton;
    private readonly Popup _taskPopup;
    private readonly StackPanel _taskPanel;
    private readonly Button _notificationButton;
    private readonly Popup _notificationPopup;
    private readonly StackPanel _notificationPanel;
    private readonly Border[] _dividers;

    private bool _syncing;

    public CadStatusBar()
    {
        // ---------------------------------------------------------------- the plate
        // One surface carries the whole bar, and it is the only thing on the bar that draws an
        // outline: a chip inside a plate with an edge of its own is the double border this used to
        // show.
        _frame = new Border { Padding = new Thickness(5, 3, 5, 3) };

        _layout = new Grid();

        _layout.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        _layout.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        _layout.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        // ---------------------------------------------------------------- left
        _left = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center
        };

        _selection = new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center,
            FontSize = 11,
            Margin = new Thickness(4, 0, 0, 0),
            Text = SelectionSummary(0)
        };

        _selection.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");

        _workPlaneText = new TextBlock { VerticalAlignment = VerticalAlignment.Center, FontSize = 11, Text = "World" };
        _workPlaneText.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextPrimary");

        _workPlane = new SplitButton
        {
            Height = 22,
            MinHeight = 0,
            Padding = new Thickness(6, 0, 4, 0),
            Icon = IconKind.AxisSystem,
            ToolTip = "Work plane",
            Content = _workPlaneText,
            DropDownContent = BuildPlaneMenu()
        };

        // Ghost, not the accent split button: which plane a drawing is on is a mode, not a call to
        // action, and a filled pill in the corner of every window outshouts the drawing it frames.
        _workPlane.SetResourceReference(StyleProperty, "CONCOST.SplitButton.Ghost");

        var leftDivider = Divider();

        _left.Children.Add(_selection);
        _left.Children.Add(leftDivider);
        _left.Children.Add(_workPlane);

        SetColumn(_left, 0);
        _layout.Children.Add(_left);

        // ---------------------------------------------------------------- middle
        _coords = new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Left,
            Margin = new Thickness(12, 0, 12, 0),
            FontFamily = MonoFont,
            FontSize = 11,
            Text = FormatCoordinates(default, "mm")
        };

        _coords.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");

        SetColumn(_coords, 1);
        _layout.Children.Add(_coords);

        // ---------------------------------------------------------------- right
        _right = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center
        };

        _toggleStrip = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };

        _toggleStrip.Children.Add(CreateToggle(CadToggle.Snap, IconKind.SnapPoint, "Snap to the grid"));
        _toggleStrip.Children.Add(CreateToggle(CadToggle.Grid, IconKind.SnapGrid, "Show grid"));
        _toggleStrip.Children.Add(CreateToggle(CadToggle.Ortho, IconKind.OrthographicView, "Ortho mode"));
        _toggleStrip.Children.Add(CreateToggle(CadToggle.Polar, IconKind.GridPolar, "Polar tracking"));
        _toggleStrip.Children.Add(CreateToggle(CadToggle.ObjectSnap, IconKind.SnapEndpoint, "Object snap"));

        var rightDivider = Divider();

        _taskButton = new Button
        {
            Height = 22,
            MinHeight = 0,
            Padding = new Thickness(6, 0, 6, 0),
            Margin = new Thickness(2, 0, 2, 0),
            ToolTip = "Background tasks",
            Visibility = Visibility.Collapsed
        };

        // Status, not Secondary: a grey capsule inside the plate would be a second frame around a
        // counter that only needs a glyph and a number.
        _taskButton.SetResourceReference(StyleProperty, "CONCOST.Button.Status");
        IconElement.SetKind(_taskButton, IconKind.Clock);

        _taskPanel = new StackPanel { Margin = new Thickness(6), MinWidth = 220 };

        _taskPopup = CreatePopup(_taskButton, _taskPanel);

        _taskButton.Click += (_, _) =>
        {
            RefreshTasks();

            _taskPopup.IsOpen = !_taskPopup.IsOpen;
        };

        _notificationButton = new Button
        {
            Height = 22,
            MinHeight = 0,
            Padding = new Thickness(6, 0, 6, 0),
            Margin = new Thickness(1, 0, 0, 0),
            ToolTip = "Notifications"
        };

        _notificationButton.SetResourceReference(StyleProperty, "CONCOST.Button.Status");
        IconElement.SetKind(_notificationButton, IconKind.Bell);

        _notificationPanel = new StackPanel { Margin = new Thickness(6), MinWidth = 240 };

        _notificationPopup = CreatePopup(_notificationButton, _notificationPanel);

        _notificationButton.Click += (_, _) =>
        {
            RefreshNotifications();
            MarkNotificationsRead();

            _notificationPopup.IsOpen = !_notificationPopup.IsOpen;
        };

        _right.Children.Add(_toggleStrip);
        _right.Children.Add(rightDivider);
        _right.Children.Add(_taskButton);
        _right.Children.Add(_notificationButton);

        SetColumn(_right, 2);
        _layout.Children.Add(_right);

        _dividers = new[] { leftDivider, rightDivider };

        _frame.Child = _layout;

        Children.Add(_frame);

        Tasks.CollectionChanged += (_, _) => RefreshTasks();
        Notifications.CollectionChanged += (_, _) => RefreshNotifications();

        ApplyChrome();
        ApplyControlHeight();
        ApplyGroupVisibility();

        RefreshTasks();
        RefreshNotifications();
    }

    /// <summary>Work that is running in the background.</summary>
    public ObservableCollection<CadTask> Tasks { get; } = new();

    /// <summary>What the status bar has been told, newest last.</summary>
    public ObservableCollection<CadNotification> Notifications { get; } = new();

    /// <summary>How many objects are selected.</summary>
    public int SelectionCount
    {
        get => (int)GetValue(SelectionCountProperty);
        set => SetValue(SelectionCountProperty, value);
    }

    /// <summary>The plane new geometry is drawn on.</summary>
    public UcsMode WorkPlane
    {
        get => (UcsMode)GetValue(WorkPlaneProperty);
        set => SetValue(WorkPlaneProperty, value);
    }

    /// <summary>The cursor position to report.</summary>
    public Point3D Coordinates
    {
        get => (Point3D)GetValue(CoordinatesProperty);
        set => SetValue(CoordinatesProperty, value);
    }

    /// <summary>Appended to the coordinate readout, e.g. <c>mm</c>.</summary>
    public string UnitSuffix
    {
        get => (string)GetValue(UnitSuffixProperty);
        set => SetValue(UnitSuffixProperty, value);
    }

    /// <summary>Hides the coordinate readout when there is no viewport under it.</summary>
    public bool ShowCoordinates
    {
        get => (bool)GetValue(ShowCoordinatesProperty);
        set => SetValue(ShowCoordinatesProperty, value);
    }

    /// <summary>Whether the cursor pulls onto snap points.</summary>
    public bool SnapEnabled
    {
        get => (bool)GetValue(SnapEnabledProperty);
        set => SetValue(SnapEnabledProperty, value);
    }

    /// <summary>Whether the grid is drawn.</summary>
    public bool GridEnabled
    {
        get => (bool)GetValue(GridEnabledProperty);
        set => SetValue(GridEnabledProperty, value);
    }

    /// <summary>Whether drawing is constrained to horizontal and vertical.</summary>
    public bool OrthoEnabled
    {
        get => (bool)GetValue(OrthoEnabledProperty);
        set => SetValue(OrthoEnabledProperty, value);
    }

    /// <summary>Whether drawing is constrained to fixed angle increments.</summary>
    public bool PolarEnabled
    {
        get => (bool)GetValue(PolarEnabledProperty);
        set => SetValue(PolarEnabledProperty, value);
    }

    /// <summary>Whether snaps to existing geometry are on.</summary>
    public bool ObjectSnapEnabled
    {
        get => (bool)GetValue(ObjectSnapEnabledProperty);
        set => SetValue(ObjectSnapEnabledProperty, value);
    }

    /// <summary>
    /// How the bar is dressed: <see cref="CadStatusBarVariant.Floating"/> at the foot of a window,
    /// <see cref="CadStatusBarVariant.Seamless"/> inside a strip the host already drew,
    /// <see cref="CadStatusBarVariant.Glass"/> over a drawing that must stay readable.
    /// </summary>
    public CadStatusBarVariant Variant
    {
        get => (CadStatusBarVariant)GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }

    /// <summary>Corner radius of the plate. Half the height makes a pill.</summary>
    public CornerRadius CornerRadius
    {
        get => (CornerRadius)GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    /// <summary>Shadow depth of the plate, 0 to 4. Ignored by the seamless variant.</summary>
    public int Elevation
    {
        get => (int)GetValue(ElevationProperty);
        set => SetValue(ElevationProperty, value);
    }

    /// <summary>Height of every toggle and button on the bar, in pixels.</summary>
    public double ControlHeight
    {
        get => (double)GetValue(ControlHeightProperty);
        set => SetValue(ControlHeightProperty, value);
    }

    /// <summary>Size of the glyph inside a toggle or a counter, in pixels.</summary>
    public double IconSize
    {
        get => (double)GetValue(IconSizeProperty);
        set => SetValue(IconSizeProperty, value);
    }

    /// <summary>Shows the rules between the groups. A rule also steps aside with the group beside it.</summary>
    public bool ShowDividers
    {
        get => (bool)GetValue(ShowDividersProperty);
        set => SetValue(ShowDividersProperty, value);
    }

    /// <summary>Shows the "nothing selected" summary.</summary>
    public bool ShowSelection
    {
        get => (bool)GetValue(ShowSelectionProperty);
        set => SetValue(ShowSelectionProperty, value);
    }

    /// <summary>Shows the work plane split button.</summary>
    public bool ShowWorkPlane
    {
        get => (bool)GetValue(ShowWorkPlaneProperty);
        set => SetValue(ShowWorkPlaneProperty, value);
    }

    /// <summary>Shows the snap, grid, ortho, polar and object-snap switches as one group.</summary>
    public bool ShowToggles
    {
        get => (bool)GetValue(ShowTogglesProperty);
        set => SetValue(ShowTogglesProperty, value);
    }

    /// <summary>Shows the background task counter. It also stays out of sight while nothing is running.</summary>
    public bool ShowTaskQueue
    {
        get => (bool)GetValue(ShowTaskQueueProperty);
        set => SetValue(ShowTaskQueueProperty, value);
    }

    /// <summary>Shows the notification bell.</summary>
    public bool ShowNotifications
    {
        get => (bool)GetValue(ShowNotificationsProperty);
        set => SetValue(ShowNotificationsProperty, value);
    }

    /// <summary>
    /// Prints SNAP, GRID, ORTHO, POLAR and OSNAP beside their glyphs. Turn it off on a narrow bar,
    /// where five captions cost more room than the switches are worth.
    /// </summary>
    public bool ShowToggleLabels
    {
        get => (bool)GetValue(ShowToggleLabelsProperty);
        set => SetValue(ShowToggleLabelsProperty, value);
    }

    /// <summary>Raised when one of the drafting toggles changes, including by binding.</summary>
    public event EventHandler<CadToggleEventArgs>? ToggleChanged;

    /// <summary>Raised when the work plane changes.</summary>
    public event EventHandler? WorkPlaneChanged;

    /// <summary>Raised when a row of the task list is picked.</summary>
    public event EventHandler<CadTask>? TaskSelected;

    /// <summary>Raised when a row of the notification list is picked.</summary>
    public event EventHandler<CadNotification>? NotificationSelected;

    /// <summary>The text a selection count reads as.</summary>
    public static string SelectionSummary(int count) => count switch
    {
        0 => "Nothing selected",
        1 => "1 object selected",
        _ => string.Format(CultureInfo.CurrentCulture, "{0:N0} objects selected", count)
    };

    /// <summary>The coordinate readout, e.g. <c>X 1,000   Y 2,000   Z 0 mm</c>.</summary>
    public static string FormatCoordinates(Point3D point, string? unitSuffix)
    {
        string suffix = string.IsNullOrEmpty(unitSuffix) ? string.Empty : " " + unitSuffix;

        return string.Format(CultureInfo.CurrentCulture, "X {0:N0}   Y {1:N0}   Z {2:N0}{3}", point.X, point.Y, point.Z, suffix);
    }

    /// <summary>The state of one toggle, whichever its kind.</summary>
    public bool IsToggleEnabled(CadToggle toggle) => toggle switch
    {
        CadToggle.Snap => SnapEnabled,
        CadToggle.Grid => GridEnabled,
        CadToggle.Ortho => OrthoEnabled,
        CadToggle.Polar => PolarEnabled,
        CadToggle.ObjectSnap => ObjectSnapEnabled,
        _ => false
    };

    /// <summary>Sets one toggle, so a ribbon button and the status bar stay in step.</summary>
    public void SetToggle(CadToggle toggle, bool isEnabled)
    {
        switch (toggle)
        {
            case CadToggle.Snap:
                SnapEnabled = isEnabled;
                break;

            case CadToggle.Grid:
                GridEnabled = isEnabled;
                break;

            case CadToggle.Ortho:
                OrthoEnabled = isEnabled;
                break;

            case CadToggle.Polar:
                PolarEnabled = isEnabled;
                break;

            case CadToggle.ObjectSnap:
                ObjectSnapEnabled = isEnabled;
                break;
        }
    }

    /// <summary>Adds a background task and shows the indicator.</summary>
    public void AddTask(CadTask task)
    {
        if (task is null) return;

        Tasks.Add(task);
    }

    /// <summary>Drops a background task, e.g. when it finishes.</summary>
    public void RemoveTask(CadTask task) => Tasks.Remove(task);

    /// <summary>Adds a notification and raises the unread count.</summary>
    public void Notify(CadNotification notification)
    {
        if (notification is null) return;

        Notifications.Add(notification);
    }

    /// <summary>Moves the coordinate readout, e.g. from a viewport mouse move.</summary>
    public void UpdateCoordinates(Point3D point) => Coordinates = point;

    /// <summary>Rebuilds the task list rows after a task's percentage changed.</summary>
    public void RefreshTasks()
    {
        _taskPanel.Children.Clear();

        foreach (var task in Tasks)
        {
            var row = new Button
            {
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                Padding = new Thickness(6, 4, 6, 4),
                Margin = new Thickness(0, 0, 0, 2),
                FontSize = 11,
                Tag = task,
                Content = CreateTaskContent(task)
            };

            row.SetResourceReference(StyleProperty, "CONCOST.Button.Secondary");

            row.Click += (sender, _) =>
            {
                _taskPopup.IsOpen = false;

                if ((sender as FrameworkElement)?.Tag is CadTask picked) TaskSelected?.Invoke(this, picked);
            };

            _taskPanel.Children.Add(row);
        }

        if (_taskPanel.Children.Count == 0)
        {
            var empty = new TextBlock { Text = "No background tasks", Margin = new Thickness(6), FontSize = 11 };
            empty.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextTertiary");

            _taskPanel.Children.Add(empty);
        }

        _taskButton.Content = Tasks.Count == 0
            ? null
            : Tasks.Count.ToString(CultureInfo.CurrentCulture);

        RefreshCounters();
    }

    /// <summary>Rebuilds the notification rows.</summary>
    public void RefreshNotifications()
    {
        _notificationPanel.Children.Clear();

        foreach (var notification in Notifications)
        {
            var row = new Button
            {
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                Padding = new Thickness(6, 4, 6, 4),
                Margin = new Thickness(0, 0, 0, 2),
                FontSize = 11,
                Tag = notification,
                Content = CreateNotificationContent(notification)
            };

            row.SetResourceReference(StyleProperty, "CONCOST.Button.Secondary");

            row.Click += (sender, _) =>
            {
                _notificationPopup.IsOpen = false;

                if ((sender as FrameworkElement)?.Tag is CadNotification picked)
                {
                    picked.IsRead = true;

                    RefreshNotifications();

                    NotificationSelected?.Invoke(this, picked);
                }
            };

            _notificationPanel.Children.Add(row);
        }

        if (_notificationPanel.Children.Count == 0)
        {
            var empty = new TextBlock { Text = "Nothing new", Margin = new Thickness(6), FontSize = 11 };
            empty.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextTertiary");

            _notificationPanel.Children.Add(empty);
        }

        int unread = Notifications.Count(notification => !notification.IsRead);

        _notificationButton.Content = unread == 0 ? null : unread.ToString(CultureInfo.CurrentCulture);
        _notificationButton.ToolTip = unread == 0
            ? "Notifications"
            : string.Format(CultureInfo.CurrentCulture, "{0} unread notifications", unread);

        RefreshCounters();
    }

    private void MarkNotificationsRead()
    {
        bool changed = false;

        foreach (var notification in Notifications.Where(notification => !notification.IsRead))
        {
            notification.IsRead = true;
            changed = true;
        }

        if (changed) RefreshNotifications();
    }

    private static StackPanel CreateTaskContent(CadTask task)
    {
        var panel = new StackPanel { Orientation = Orientation.Horizontal };

        var icon = SmallIcon(IconKind.Clock);

        icon.Margin = new Thickness(0, 0, 6, 0);

        panel.Children.Add(icon);

        var title = new TextBlock { Text = task.Title, FontSize = 11, VerticalAlignment = VerticalAlignment.Center };
        title.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextPrimary");

        panel.Children.Add(title);

        var percent = new TextBlock
        {
            Text = task.IsIndeterminate ? "…" : string.Format(CultureInfo.CurrentCulture, "{0:0}%", task.Percent),
            FontSize = 11,
            Margin = new Thickness(8, 0, 0, 0),
            VerticalAlignment = VerticalAlignment.Center
        };

        percent.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");

        panel.Children.Add(percent);

        return panel;
    }

    private static StackPanel CreateNotificationContent(CadNotification notification)
    {
        var panel = new StackPanel();

        var header = new StackPanel { Orientation = Orientation.Horizontal };

        var icon = SmallIcon(IconFor(notification.Severity));

        icon.Margin = new Thickness(0, 0, 6, 0);

        header.Children.Add(icon);

        var title = new TextBlock
        {
            Text = notification.Title,
            FontSize = 11,
            FontWeight = notification.IsRead ? FontWeights.Normal : FontWeights.SemiBold,
            VerticalAlignment = VerticalAlignment.Center
        };

        title.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextPrimary");

        header.Children.Add(title);

        panel.Children.Add(header);

        if (notification.Message.Length > 0)
        {
            var message = new TextBlock
            {
                Text = notification.Message,
                FontSize = 10,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(20, 1, 0, 0)
            };

            message.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextSecondary");

            panel.Children.Add(message);
        }

        return panel;
    }

    private static IconKind IconFor(CadNotificationSeverity severity) => severity switch
    {
        CadNotificationSeverity.Success => IconKind.CheckCircle,
        CadNotificationSeverity.Warning => IconKind.Warning,
        CadNotificationSeverity.Error => IconKind.Error,
        _ => IconKind.Info
    };

    private StackPanel BuildPlaneMenu()
    {
        var menu = new StackPanel { MinWidth = 120 };

        foreach (var plane in Planes)
        {
            var item = new Button
            {
                Content = plane.ToString(),
                HorizontalContentAlignment = HorizontalAlignment.Left,
                Padding = new Thickness(8, 4, 8, 4),
                Margin = new Thickness(0, 0, 0, 2),
                FontSize = 11,
                Tag = plane
            };

            item.SetResourceReference(StyleProperty, "CONCOST.Button.Secondary");

            item.Click += (sender, _) =>
            {
                if ((sender as FrameworkElement)?.Tag is not UcsMode picked) return;

                WorkPlane = picked;

                _workPlane.IsDropDownOpen = false;
            };

            menu.Children.Add(item);
        }

        return menu;
    }

    private ToggleButton CreateToggle(CadToggle toggle, IconKind kind, string tooltip)
    {
        var button = new ToggleButton
        {
            Height = 22,
            MinWidth = 0,
            MinHeight = 0,
            Margin = new Thickness(1, 0, 1, 0),
            ToolTip = tooltip,
            IsChecked = IsToggleEnabled(toggle),
            Tag = toggle
        };

        // Status, not Icon: the stock icon toggle asks for a 34 px box and carries no caption, so
        // five of them read as five anonymous squares on a bar that has to be read at a glance.
        button.SetResourceReference(StyleProperty, "CONCOST.ToggleButton.Status");
        IconElement.SetKind(button, kind);
        IconElement.SetIconSize(button, IconSize);

        button.Click += (sender, _) =>
        {
            if (_syncing) return;

            bool state = (sender as ToggleButton)?.IsChecked == true;

            SetToggle((CadToggle)(sender as FrameworkElement)!.Tag, state);

            // The property callback raises the event, so a binding and a click report identically.
        };

        _toggles[toggle] = button;

        return button;
    }

    /// <summary>Dresses the plate. Only this border draws an outline; the controls inside stay flat.</summary>
    private void ApplyChrome()
    {
        _frame.CornerRadius = CornerRadius;
        _frame.Opacity = 1;

        switch (Variant)
        {
            case CadStatusBarVariant.Seamless:
                // No plate at all: the bar borrows the strip the host docked it into.
                _frame.SetValue(Border.BackgroundProperty, Brushes.Transparent);
                _frame.BorderThickness = new Thickness(0);
                VisualElement.SetElevation(_frame, 0);
                break;

            case CadStatusBarVariant.Glass:
                // A translucent plate, so a view drawn underneath still reads through the bar.
                _frame.SetResourceReference(Border.BackgroundProperty, "CONCOST.Brush.Surface");
                _frame.BorderThickness = new Thickness(0);
                _frame.Opacity = 0.9;
                VisualElement.SetElevation(_frame, Math.Max(2, Elevation));
                break;

            default:
                _frame.SetResourceReference(Border.BackgroundProperty, "CONCOST.Brush.Surface");
                _frame.SetResourceReference(Border.BorderBrushProperty, "CONCOST.Brush.Border");
                _frame.BorderThickness = new Thickness(1);
                VisualElement.SetElevation(_frame, Elevation);
                break;
        }
    }

    private void ApplyControlHeight()
    {
        double height = double.IsNaN(ControlHeight) || ControlHeight < 18 ? 18 : ControlHeight;
        double icon = double.IsNaN(IconSize) || IconSize < 8 ? 8 : IconSize;
        double font = Math.Max(9.5, Math.Round(height * 0.42));

        _selection.FontSize = font;
        _coords.FontSize = font;
        _workPlaneText.FontSize = font;

        _workPlane.MinHeight = 0;
        _workPlane.Height = height;
        _workPlane.FontSize = font;
        _workPlane.IconSize = icon;
        _workPlane.CornerRadius = new CornerRadius(4);

        foreach (var toggle in _toggles.Values)
        {
            // A minimum beats a height, so the minimum goes first — otherwise the style's own
            // floor decides how tall the bar is instead of ControlHeight.
            toggle.MinWidth = 0;
            toggle.MinHeight = 0;
            toggle.Height = height;
            toggle.FontSize = font;
            IconElement.SetIconSize(toggle, icon);
        }

        foreach (var button in new[] { _taskButton, _notificationButton })
        {
            button.MinWidth = 0;
            button.MinHeight = 0;
            button.Height = height;
            button.FontSize = font;
            IconElement.SetIconSize(button, icon);
        }

        double air = Math.Max(4, Math.Round(height * 0.18));

        foreach (var divider in _dividers)
        {
            divider.Margin = new Thickness(air, Math.Max(2, air - 1), air, Math.Max(2, air - 1));
        }
    }

    private void ApplyGroupVisibility()
    {
        _selection.Visibility = ShowSelection ? Visibility.Visible : Visibility.Collapsed;
        _workPlane.Visibility = ShowWorkPlane ? Visibility.Visible : Visibility.Collapsed;
        _toggleStrip.Visibility = ShowToggles ? Visibility.Visible : Visibility.Collapsed;
        _coords.Visibility = ShowCoordinates ? Visibility.Visible : Visibility.Collapsed;

        ApplyToggleLabels();
        RefreshCounters();
    }

    private void ApplyToggleLabels()
    {
        foreach (var pair in _toggles)
        {
            pair.Value.Content = ShowToggleLabels && ToggleCaptions.TryGetValue(pair.Key, out string? caption)
                ? caption
                : null;
        }
    }

    /// <summary>The two counters only keep a place on the bar while they have something to say.</summary>
    private void RefreshCounters()
    {
        _taskButton.Visibility = ShowTaskQueue && Tasks.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        _notificationButton.Visibility = ShowNotifications ? Visibility.Visible : Visibility.Collapsed;

        RefreshDividers();
    }

    private void RefreshDividers()
    {
        bool on = ShowDividers;

        // A rule also leaves with the group beside it, so a collapsed cluster never leaves a
        // floating line at the end of the bar.
        _dividers[0].Visibility = on && Shows(_selection) && Shows(_workPlane) ? Visibility.Visible : Visibility.Collapsed;
        _dividers[1].Visibility = on && Shows(_toggleStrip) && ShowsAny() ? Visibility.Visible : Visibility.Collapsed;
    }

    private bool ShowsAny() =>
        _taskButton.Visibility == Visibility.Visible || _notificationButton.Visibility == Visibility.Visible;

    private static bool Shows(UIElement element) => element.Visibility == Visibility.Visible;

    private static Popup CreatePopup(UIElement target, UIElement child)
    {
        var frame = new Border
        {
            Padding = new Thickness(2),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(6),
            Child = child
        };

        frame.SetResourceReference(Border.BackgroundProperty, "CONCOST.Brush.Surface");
        frame.SetResourceReference(Border.BorderBrushProperty, "CONCOST.Brush.Border");

        return new Popup
        {
            PlacementTarget = target,
            Placement = PlacementMode.Top,
            StaysOpen = false,
            AllowsTransparency = true,
            Child = frame
        };
    }

    private static Icon SmallIcon(IconKind kind) => new() { Kind = kind, Width = 12, Height = 12, VerticalAlignment = VerticalAlignment.Center };

    private static Border Divider()
    {
        var divider = new Border
        {
            Width = 1,
            Margin = new Thickness(6, 4, 6, 4),
            VerticalAlignment = VerticalAlignment.Stretch,
            SnapsToDevicePixels = true
        };

        // A token, not a fixed grey: a fixed grey turns into a bright line the moment the theme
        // goes dark.
        divider.SetResourceReference(Border.BackgroundProperty, "CONCOST.Brush.Border");

        return divider;
    }

    private static void OnChromeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is CadStatusBar bar) bar.ApplyChrome();
    }

    private static void OnControlHeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is CadStatusBar bar) bar.ApplyControlHeight();
    }

    private static void OnGroupVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is CadStatusBar bar) bar.ApplyGroupVisibility();
    }

    private static void OnSelectionCountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is CadStatusBar bar) bar._selection.Text = SelectionSummary((int)e.NewValue);
    }

    private static void OnCoordinatesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not CadStatusBar bar) return;

        bar._coords.Visibility = bar.ShowCoordinates ? Visibility.Visible : Visibility.Collapsed;
        bar._coords.Text = FormatCoordinates(bar.Coordinates, bar.UnitSuffix);
    }

    private static void OnWorkPlaneChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not CadStatusBar bar) return;

        bar._workPlaneText.Text = ((UcsMode)e.NewValue).ToString();
        bar.WorkPlaneChanged?.Invoke(bar, EventArgs.Empty);
    }

    private static void OnTogglePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not CadStatusBar bar) return;

        var toggle = e.Property switch
        {
            var property when property == SnapEnabledProperty => CadToggle.Snap,
            var property when property == GridEnabledProperty => CadToggle.Grid,
            var property when property == OrthoEnabledProperty => CadToggle.Ortho,
            var property when property == PolarEnabledProperty => CadToggle.Polar,
            var property when property == ObjectSnapEnabledProperty => CadToggle.ObjectSnap,
            _ => (CadToggle?)null
        };

        if (toggle is null) return;

        if (bar._toggles.TryGetValue(toggle.Value, out var button))
        {
            bar._syncing = true;

            try
            {
                button.IsChecked = (bool)e.NewValue;
            }
            finally
            {
                bar._syncing = false;
            }
        }

        bar.ToggleChanged?.Invoke(bar, new CadToggleEventArgs(toggle.Value, (bool)e.NewValue));
    }
}
