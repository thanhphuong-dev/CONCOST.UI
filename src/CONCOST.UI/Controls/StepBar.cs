using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using CONCOST.UI.Icons;

namespace CONCOST.UI.Controls;

public enum StepStatus
{
    Waiting,
    UnderWay,
    Complete,
    Error
}

/// <summary>
/// A single step in a <see cref="StepBar"/>: shows an index, icon, header, sub-title and status.
/// </summary>
public class StepBarItem : ContentControl
{
    static StepBarItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StepBarItem), new FrameworkPropertyMetadata(typeof(StepBarItem)));
    }

    // ------------------------------------------------------------------ original properties

    public static readonly DependencyProperty IndexProperty =
        DependencyProperty.Register(nameof(Index), typeof(int), typeof(StepBarItem), new PropertyMetadata(1));

    public static readonly DependencyProperty StatusProperty =
        DependencyProperty.Register(nameof(Status), typeof(StepStatus), typeof(StepBarItem), new PropertyMetadata(StepStatus.Waiting));

    public static readonly DependencyProperty DescriptionProperty =
        DependencyProperty.Register(nameof(Description), typeof(string), typeof(StepBarItem), new PropertyMetadata(string.Empty));

    public int Index
    {
        get => (int)GetValue(IndexProperty);
        set => SetValue(IndexProperty, value);
    }

    public StepStatus Status
    {
        get => (StepStatus)GetValue(StatusProperty);
        set => SetValue(StatusProperty, value);
    }

    public string Description
    {
        get => (string)GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    // ------------------------------------------------------------------ new properties

    /// <summary>Optional icon displayed instead of the step number. <see cref="IconKind.None"/> shows the number.</summary>
    public static readonly DependencyProperty IconProperty =
        DependencyProperty.Register(nameof(Icon), typeof(IconKind), typeof(StepBarItem),
            new PropertyMetadata(IconKind.None));

    public IconKind Icon
    {
        get => (IconKind)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>Whether this item can be navigated to when <see cref="StepBar.IsNavigable"/> is <c>true</c>.</summary>
    public static readonly DependencyProperty CanNavigateProperty =
        DependencyProperty.Register(nameof(CanNavigate), typeof(bool), typeof(StepBarItem),
            new PropertyMetadata(true));

    public bool CanNavigate
    {
        get => (bool)GetValue(CanNavigateProperty);
        set => SetValue(CanNavigateProperty, value);
    }

    /// <summary>Secondary label shown below the main header text.</summary>
    public static readonly DependencyProperty SubTitleProperty =
        DependencyProperty.Register(nameof(SubTitle), typeof(string), typeof(StepBarItem),
            new PropertyMetadata(null));

    public string? SubTitle
    {
        get => (string?)GetValue(SubTitleProperty);
        set => SetValue(SubTitleProperty, value);
    }
}

/// <summary>
/// A horizontal or vertical step progress bar. Supports navigation, per-status brushes,
/// animated progress and routed <see cref="StepChanged"/>/<see cref="StepCompleted"/> events.
/// </summary>
[TemplatePart(Name = "PART_ProgressBarBack", Type = typeof(ProgressBar))]
public class StepBar : ItemsControl
{
    private ProgressBar? _progressBarBack;
    private int _oriStepIndex = -1;

    static StepBar()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StepBar), new FrameworkPropertyMetadata(typeof(StepBar)));
    }

    public StepBar()
    {
        ItemContainerGenerator.StatusChanged += ItemContainerGenerator_StatusChanged;
        Loaded += (s, e) => UpdateSteps(StepIndex);
    }

    // ------------------------------------------------------------------ original properties

    public static readonly DependencyProperty StepIndexProperty =
        DependencyProperty.Register(nameof(StepIndex), typeof(int), typeof(StepBar),
            new FrameworkPropertyMetadata(0,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnStepIndexChanged, CoerceStepIndex));

    public static readonly DependencyProperty DockProperty =
        DependencyProperty.Register(nameof(Dock), typeof(Dock), typeof(StepBar), new PropertyMetadata(Dock.Top));

    public int StepIndex
    {
        get => (int)GetValue(StepIndexProperty);
        set => SetValue(StepIndexProperty, value);
    }

    public Dock Dock
    {
        get => (Dock)GetValue(DockProperty);
        set => SetValue(DockProperty, value);
    }

    // ------------------------------------------------------------------ navigation

    /// <summary>
    /// When <c>true</c>, clicking a completed step navigates back to it (subject to <see cref="StepBarItem.CanNavigate"/>).
    /// Default <c>false</c>.
    /// </summary>
    public static readonly DependencyProperty IsNavigableProperty =
        DependencyProperty.Register(nameof(IsNavigable), typeof(bool), typeof(StepBar),
            new PropertyMetadata(false));

    public bool IsNavigable
    {
        get => (bool)GetValue(IsNavigableProperty);
        set => SetValue(IsNavigableProperty, value);
    }

    // ------------------------------------------------------------------ progress bar

    /// <summary>Whether the connecting progress bar between step indicators is visible. Default <c>true</c>.</summary>
    public static readonly DependencyProperty ShowProgressBarProperty =
        DependencyProperty.Register(nameof(ShowProgressBar), typeof(bool), typeof(StepBar),
            new PropertyMetadata(true));

    public bool ShowProgressBar
    {
        get => (bool)GetValue(ShowProgressBarProperty);
        set => SetValue(ShowProgressBarProperty, value);
    }

    /// <summary>Thickness of the step connector line. Default 2.</summary>
    public static readonly DependencyProperty StepConnectorThicknessProperty =
        DependencyProperty.Register(nameof(StepConnectorThickness), typeof(double), typeof(StepBar),
            new PropertyMetadata(2.0));

    public double StepConnectorThickness
    {
        get => (double)GetValue(StepConnectorThicknessProperty);
        set => SetValue(StepConnectorThicknessProperty, value);
    }

    // ------------------------------------------------------------------ status brushes

    /// <summary>Background/fill brush of completed step circles. Null uses the theme primary brush.</summary>
    public static readonly DependencyProperty CompletedBrushProperty =
        DependencyProperty.Register(nameof(CompletedBrush), typeof(Brush), typeof(StepBar),
            new FrameworkPropertyMetadata(null));

    public Brush? CompletedBrush
    {
        get => (Brush?)GetValue(CompletedBrushProperty);
        set => SetValue(CompletedBrushProperty, value);
    }

    /// <summary>Brush of the active (current) step indicator. Null uses the theme primary brush.</summary>
    public static readonly DependencyProperty ActiveBrushProperty =
        DependencyProperty.Register(nameof(ActiveBrush), typeof(Brush), typeof(StepBar),
            new FrameworkPropertyMetadata(null));

    public Brush? ActiveBrush
    {
        get => (Brush?)GetValue(ActiveBrushProperty);
        set => SetValue(ActiveBrushProperty, value);
    }

    /// <summary>Brush of waiting (future) step indicators. Null uses the theme border brush.</summary>
    public static readonly DependencyProperty WaitingBrushProperty =
        DependencyProperty.Register(nameof(WaitingBrush), typeof(Brush), typeof(StepBar),
            new FrameworkPropertyMetadata(null));

    public Brush? WaitingBrush
    {
        get => (Brush?)GetValue(WaitingBrushProperty);
        set => SetValue(WaitingBrushProperty, value);
    }

    /// <summary>Brush of error step indicators. Null uses the theme danger brush.</summary>
    public static readonly DependencyProperty ErrorBrushProperty =
        DependencyProperty.Register(nameof(ErrorBrush), typeof(Brush), typeof(StepBar),
            new FrameworkPropertyMetadata(null));

    public Brush? ErrorBrush
    {
        get => (Brush?)GetValue(ErrorBrushProperty);
        set => SetValue(ErrorBrushProperty, value);
    }

    // ------------------------------------------------------------------ routed events

    /// <summary>Raised when <see cref="StepIndex"/> changes.</summary>
    public static readonly RoutedEvent StepChangedEvent =
        EventManager.RegisterRoutedEvent(nameof(StepChanged), RoutingStrategy.Bubble,
            typeof(RoutedPropertyChangedEventHandler<int>), typeof(StepBar));

    /// <summary>Raised when <see cref="StepIndex"/> advances past the last item (all steps completed).</summary>
    public static readonly RoutedEvent StepCompletedEvent =
        EventManager.RegisterRoutedEvent(nameof(StepCompleted), RoutingStrategy.Bubble,
            typeof(RoutedEventHandler), typeof(StepBar));

    public event RoutedPropertyChangedEventHandler<int> StepChanged
    {
        add => AddHandler(StepChangedEvent, value);
        remove => RemoveHandler(StepChangedEvent, value);
    }

    public event RoutedEventHandler StepCompleted
    {
        add => AddHandler(StepCompletedEvent, value);
        remove => RemoveHandler(StepCompletedEvent, value);
    }

    // ------------------------------------------------------------------ public API

    /// <summary>Navigates directly to the specified step index, respecting <see cref="IsNavigable"/>.</summary>
    public void GoToStep(int index)
    {
        int count = Items.Count;
        if (count == 0) return;

        int clamped = Math.Max(0, Math.Min(count - 1, index));

        // Only allow going backwards if navigable; always allow going forwards
        if (clamped < StepIndex && !IsNavigable) return;

        // Check per-item CanNavigate when going back
        if (clamped < StepIndex && IsNavigable)
        {
            var container = ItemContainerGenerator.ContainerFromIndex(clamped) as StepBarItem
                            ?? (Items[clamped] as StepBarItem);
            if (container?.CanNavigate == false) return;
        }

        StepIndex = clamped;
    }

    // ------------------------------------------------------------------ coerce / callbacks

    private static object CoerceStepIndex(DependencyObject d, object baseValue)
    {
        var ctl = (StepBar)d;
        int stepIndex = (int)baseValue;
        if (ctl.Items.Count == 0 && stepIndex > 0)
        {
            ctl._oriStepIndex = stepIndex;
            return 0;
        }

        return stepIndex < 0 ? 0 : (stepIndex >= ctl.Items.Count ? (ctl.Items.Count == 0 ? 0 : ctl.Items.Count - 1) : stepIndex);
    }

    private static void OnStepIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not StepBar sb) return;

        int oldIdx = (int)e.OldValue;
        int newIdx = (int)e.NewValue;

        sb.UpdateSteps(newIdx);

        sb.RaiseEvent(new RoutedPropertyChangedEventArgs<int>(oldIdx, newIdx, StepChangedEvent));

        // Raise StepCompleted when last step becomes active
        if (newIdx == sb.Items.Count - 1 && newIdx > oldIdx)
            sb.RaiseEvent(new RoutedEventArgs(StepCompletedEvent, sb));
    }

    // ------------------------------------------------------------------ container generation

    protected override bool IsItemItsOwnContainerOverride(object item) => item is StepBarItem;
    protected override DependencyObject GetContainerForItemOverride() => new StepBarItem();

    // ------------------------------------------------------------------ navigation steps

    public void Next() => StepIndex++;
    public void Prev() => StepIndex--;

    // ------------------------------------------------------------------ template

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        _progressBarBack = GetTemplateChild("PART_ProgressBarBack") as ProgressBar;
        UpdateSteps(StepIndex);
    }

    // ------------------------------------------------------------------ item container status

    private void ItemContainerGenerator_StatusChanged(object? sender, EventArgs e)
    {
        if (ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated) return;

        int count = Items.Count;
        InvalidateVisual();

        if (_progressBarBack != null)
        {
            _progressBarBack.Maximum = Math.Max(1, count - 1);
            _progressBarBack.Value = StepIndex;
        }

        if (count <= 0) return;

        for (int i = 0; i < count; i++)
        {
            var container = ItemContainerGenerator.ContainerFromIndex(i) as StepBarItem;
            if (container == null && Items[i] is StepBarItem sbi) container = sbi;
            if (container != null) container.Index = i + 1;
        }

        if (_oriStepIndex >= 0)
        {
            StepIndex = _oriStepIndex;
            _oriStepIndex = -1;
        }
        else
        {
            UpdateSteps(StepIndex);
        }
    }

    // ------------------------------------------------------------------ render layout

    protected override void OnRender(DrawingContext drawingContext)
    {
        base.OnRender(drawingContext);

        int colCount = Items.Count;
        if (_progressBarBack == null || colCount <= 1) return;

        if (Dock == Dock.Top || Dock == Dock.Bottom)
            _progressBarBack.Width = (colCount - 1) * (ActualWidth / colCount);
        else
            _progressBarBack.Height = (colCount - 1) * (ActualHeight / colCount);
    }

    // ------------------------------------------------------------------ update step states

    public void UpdateSteps(int stepIndex)
    {
        int count = Items.Count;
        if (count == 0) return;

        if (_progressBarBack != null)
        {
            _progressBarBack.Visibility = ShowProgressBar ? Visibility.Visible : Visibility.Collapsed;
            _progressBarBack.Maximum = Math.Max(1, count - 1);
            var anim = new DoubleAnimation(stepIndex, TimeSpan.FromMilliseconds(250))
            {
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
            };
            _progressBarBack.BeginAnimation(RangeBase.ValueProperty, anim);
        }

        for (int i = 0; i < count; i++)
        {
            var container = ItemContainerGenerator.ContainerFromIndex(i) as StepBarItem;
            if (container == null && Items[i] is StepBarItem sbi) container = sbi;

            if (container != null)
            {
                container.Index = i + 1;
                container.Status = i < stepIndex ? StepStatus.Complete
                    : i == stepIndex ? StepStatus.UnderWay
                    : StepStatus.Waiting;
            }
        }
    }
}
