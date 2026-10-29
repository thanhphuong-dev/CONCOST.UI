using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace CONCOST.UI.Controls;

public enum StepStatus
{
    Waiting,
    UnderWay,
    Complete,
    Error
}

public class StepBarItem : ContentControl
{
    static StepBarItem()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(StepBarItem), new FrameworkPropertyMetadata(typeof(StepBarItem)));
    }

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
}

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

    public static readonly DependencyProperty StepIndexProperty =
        DependencyProperty.Register(nameof(StepIndex), typeof(int), typeof(StepBar),
            new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnStepIndexChanged, CoerceStepIndex));

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
        if (d is StepBar sb)
        {
            sb.UpdateSteps((int)e.NewValue);
        }
    }

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
            if (container == null && Items[i] is StepBarItem sbi)
            {
                container = sbi;
            }
            if (container != null)
            {
                container.Index = i + 1;
            }
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

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        _progressBarBack = GetTemplateChild("PART_ProgressBarBack") as ProgressBar;
        UpdateSteps(StepIndex);
    }

    protected override bool IsItemItsOwnContainerOverride(object item) => item is StepBarItem;
    protected override DependencyObject GetContainerForItemOverride() => new StepBarItem();

    public void Next() => StepIndex++;
    public void Prev() => StepIndex--;

    protected override void OnRender(DrawingContext drawingContext)
    {
        base.OnRender(drawingContext);

        int colCount = Items.Count;
        if (_progressBarBack == null || colCount <= 1) return;

        if (Dock == Dock.Top || Dock == Dock.Bottom)
        {
            _progressBarBack.Width = (colCount - 1) * (ActualWidth / colCount);
        }
        else
        {
            _progressBarBack.Height = (colCount - 1) * (ActualHeight / colCount);
        }
    }

    public void UpdateSteps(int stepIndex)
    {
        int count = Items.Count;
        if (count == 0) return;

        if (_progressBarBack != null)
        {
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
            if (container == null && Items[i] is StepBarItem sbi)
            {
                container = sbi;
            }

            if (container != null)
            {
                container.Index = i + 1;
                if (i < stepIndex)
                {
                    container.Status = StepStatus.Complete;
                }
                else if (i == stepIndex)
                {
                    container.Status = StepStatus.UnderWay;
                }
                else
                {
                    container.Status = StepStatus.Waiting;
                }
            }
        }
    }
}