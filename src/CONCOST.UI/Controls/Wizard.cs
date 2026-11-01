using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace CONCOST.UI.Controls;

/// <summary>
/// One page of a <see cref="Wizard"/>: a header, a description and the content the user fills in.
/// </summary>
public class WizardStep : HeaderedContentControl
{
    public static readonly DependencyProperty DescriptionProperty =
        DependencyProperty.Register(nameof(Description), typeof(string), typeof(WizardStep),
            new PropertyMetadata(string.Empty));

    /// <summary>Second line under the header — what the step is for.</summary>
    public string Description
    {
        get => (string)GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    public static readonly DependencyProperty IsValidProperty =
        DependencyProperty.Register(nameof(IsValid), typeof(bool), typeof(WizardStep),
            new PropertyMetadata(true));

    /// <summary>
    /// Whether the step's input is acceptable. <c>false</c> disables the Next button, which is what
    /// makes "Next" mean "this page is done" instead of "I clicked a button".
    /// </summary>
    public bool IsValid
    {
        get => (bool)GetValue(IsValidProperty);
        set => SetValue(IsValidProperty, value);
    }

    private static readonly DependencyPropertyKey IndexKey =
        DependencyProperty.RegisterReadOnly(nameof(Index), typeof(int), typeof(WizardStep),
            new PropertyMetadata(-1));

    public static readonly DependencyProperty IndexProperty = IndexKey.DependencyProperty;

    /// <summary>Zero-based position in the wizard. Set by the wizard, not by hand.</summary>
    public int Index => (int)GetValue(IndexProperty);

    private static readonly DependencyPropertyKey NumberKey =
        DependencyProperty.RegisterReadOnly(nameof(Number), typeof(int), typeof(WizardStep),
            new PropertyMetadata(0));

    public static readonly DependencyProperty NumberProperty = NumberKey.DependencyProperty;

    /// <summary><see cref="Index"/> as the user counts it, starting at 1 — what the header's dot shows.</summary>
    public int Number => (int)GetValue(NumberProperty);

    private static readonly DependencyPropertyKey IsCurrentKey =
        DependencyProperty.RegisterReadOnly(nameof(IsCurrent), typeof(bool), typeof(WizardStep),
            new PropertyMetadata(false));

    public static readonly DependencyProperty IsCurrentProperty = IsCurrentKey.DependencyProperty;

    /// <summary><c>true</c> for the step on screen.</summary>
    public bool IsCurrent => (bool)GetValue(IsCurrentProperty);

    private static readonly DependencyPropertyKey IsCompletedKey =
        DependencyProperty.RegisterReadOnly(nameof(IsCompleted), typeof(bool), typeof(WizardStep),
            new PropertyMetadata(false));

    public static readonly DependencyProperty IsCompletedProperty = IsCompletedKey.DependencyProperty;

    /// <summary><c>true</c> for a step the user has already left — the header's tick.</summary>
    public bool IsCompleted => (bool)GetValue(IsCompletedProperty);

    internal void SetPosition(int index, bool isCurrent, bool isCompleted)
    {
        SetValue(IndexKey, index);
        SetValue(NumberKey, index + 1);
        SetValue(IsCurrentKey, isCurrent);
        SetValue(IsCompletedKey, isCompleted);
    }
}

/// <summary>Describes a step change that is about to happen; <see cref="Cancel"/> stops it.</summary>
public class WizardNavigatingEventArgs : EventArgs
{
    public WizardNavigatingEventArgs(int fromIndex, int toIndex)
    {
        FromIndex = fromIndex;
        ToIndex = toIndex;
    }

    /// <summary>The step the user is leaving, or -1 before the first one.</summary>
    public int FromIndex { get; }

    /// <summary>The step the user asked for.</summary>
    public int ToIndex { get; }

    /// <summary>Set to <c>true</c> to stay where you are — the hook for "fix the errors first".</summary>
    public bool Cancel { get; set; }
}

/// <summary>
/// A multi-step form: a header that shows where you are, one step's content at a time, and back /
/// next / finish buttons that only advance when the current step says it is valid.
/// </summary>
/// <remarks>
/// <para>
/// Steps are children of the wizard, so a form reads as a form in XAML:
/// </para>
/// <code>
/// &lt;ui:Wizard Finished="OnFinished"&gt;
///     &lt;ui:WizardStep Header="Thông tin dự án"&gt;...&lt;/ui:WizardStep&gt;
///     &lt;ui:WizardStep Header="Nhập tiết diện"&gt;...&lt;/ui:WizardStep&gt;
/// &lt;/ui:Wizard&gt;
/// </code>
/// <para>
/// Validation is the step's own <see cref="WizardStep.IsValid"/>; the wizard only reads it. That keeps
/// the check next to the fields it belongs to, and lets a validator flip one boolean rather than
/// reach into the wizard.
/// </para>
/// </remarks>
[ContentProperty(nameof(Steps))]
public class Wizard : Control
{
    /// <summary>A step is not in the visual tree, so its validity is observed rather than walked to.</summary>
    private static readonly System.ComponentModel.DependencyPropertyDescriptor IsValidDescriptor =
        System.ComponentModel.DependencyPropertyDescriptor.FromProperty(WizardStep.IsValidProperty, typeof(WizardStep));

    private readonly HashSet<WizardStep> _tracked = new();
    private Button? _backButton;
    private Button? _nextButton;

    static Wizard()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(Wizard),
            new FrameworkPropertyMetadata(typeof(Wizard)));
    }

    public Wizard()
    {
        Steps = new ObservableCollection<WizardStep>();
        Steps.CollectionChanged += OnStepsChanged;
    }

    /// <summary>The steps, in order. Adding or removing one renumbers the rest.</summary>
    public ObservableCollection<WizardStep> Steps { get; }

    public static readonly DependencyProperty CurrentIndexProperty =
        DependencyProperty.Register(nameof(CurrentIndex), typeof(int), typeof(Wizard),
            new FrameworkPropertyMetadata(-1, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnCurrentIndexChanged));

    /// <summary>Zero-based index of the step on screen. Two-way, so a view model can drive the wizard.</summary>
    public int CurrentIndex
    {
        get => (int)GetValue(CurrentIndexProperty);
        set => SetValue(CurrentIndexProperty, value);
    }

    private static readonly DependencyPropertyKey CurrentStepKey =
        DependencyProperty.RegisterReadOnly(nameof(CurrentStep), typeof(WizardStep), typeof(Wizard),
            new PropertyMetadata(null));

    public static readonly DependencyProperty CurrentStepProperty = CurrentStepKey.DependencyProperty;

    /// <summary>The step on screen, or <c>null</c> when the wizard has no steps.</summary>
    public WizardStep? CurrentStep => (WizardStep?)GetValue(CurrentStepProperty);

    private static readonly DependencyPropertyKey CanGoBackKey =
        DependencyProperty.RegisterReadOnly(nameof(CanGoBack), typeof(bool), typeof(Wizard),
            new PropertyMetadata(false));

    public static readonly DependencyProperty CanGoBackProperty = CanGoBackKey.DependencyProperty;

    /// <summary><c>true</c> when there is a step before the current one.</summary>
    public bool CanGoBack => (bool)GetValue(CanGoBackProperty);

    private static readonly DependencyPropertyKey CanGoNextKey =
        DependencyProperty.RegisterReadOnly(nameof(CanGoNext), typeof(bool), typeof(Wizard),
            new PropertyMetadata(false));

    public static readonly DependencyProperty CanGoNextProperty = CanGoNextKey.DependencyProperty;

    /// <summary><c>true</c> when there is a step after the current one and the current one is valid.</summary>
    public bool CanGoNext => (bool)GetValue(CanGoNextProperty);

    private static readonly DependencyPropertyKey IsLastStepKey =
        DependencyProperty.RegisterReadOnly(nameof(IsLastStep), typeof(bool), typeof(Wizard),
            new PropertyMetadata(false));

    public static readonly DependencyProperty IsLastStepProperty = IsLastStepKey.DependencyProperty;

    /// <summary><c>true</c> on the final step, where the button says Finish.</summary>
    public bool IsLastStep => (bool)GetValue(IsLastStepProperty);

    public static readonly DependencyProperty ShowHeaderProperty =
        DependencyProperty.Register(nameof(ShowHeader), typeof(bool), typeof(Wizard),
            new PropertyMetadata(true, OnChromeChanged));

    /// <summary>Draw the step header. Turn it off when the wizard sits inside a dialog that already titles the work.</summary>
    public bool ShowHeader
    {
        get => (bool)GetValue(ShowHeaderProperty);
        set => SetValue(ShowHeaderProperty, value);
    }

    public static readonly DependencyProperty ShowFooterProperty =
        DependencyProperty.Register(nameof(ShowFooter), typeof(bool), typeof(Wizard),
            new PropertyMetadata(true, OnChromeChanged));

    /// <summary>Draw the back / next row. Turn it off to drive the wizard from your own buttons.</summary>
    public bool ShowFooter
    {
        get => (bool)GetValue(ShowFooterProperty);
        set => SetValue(ShowFooterProperty, value);
    }

    public static readonly DependencyProperty BackTextProperty =
        DependencyProperty.Register(nameof(BackText), typeof(string), typeof(Wizard),
            new PropertyMetadata("Back", OnButtonTextChanged));

    /// <summary>Caption of the back button. Default "Back".</summary>
    public string BackText
    {
        get => (string)GetValue(BackTextProperty);
        set => SetValue(BackTextProperty, value);
    }

    public static readonly DependencyProperty NextTextProperty =
        DependencyProperty.Register(nameof(NextText), typeof(string), typeof(Wizard),
            new PropertyMetadata("Next", OnButtonTextChanged));

    /// <summary>Caption of the next button. Default "Next".</summary>
    public string NextText
    {
        get => (string)GetValue(NextTextProperty);
        set => SetValue(NextTextProperty, value);
    }

    public static readonly DependencyProperty FinishTextProperty =
        DependencyProperty.Register(nameof(FinishText), typeof(string), typeof(Wizard),
            new PropertyMetadata("Finish", OnButtonTextChanged));

    /// <summary>Caption of the next button on the last step. Default "Finish".</summary>
    public string FinishText
    {
        get => (string)GetValue(FinishTextProperty);
        set => SetValue(FinishTextProperty, value);
    }

    /// <summary>Raised before a step change; set <see cref="WizardNavigatingEventArgs.Cancel"/> to stop it.</summary>
    public event EventHandler<WizardNavigatingEventArgs>? Navigating;

    /// <summary>Raised after the step changed.</summary>
    public event EventHandler? StepChanged;

    /// <summary>Raised when the user clicks Finish on the last step.</summary>
    public event EventHandler? Finished;

    /// <summary>Moves forward one step. Returns <c>false</c> when that was not allowed.</summary>
    public bool Next() => GoTo(CurrentIndex + 1);

    /// <summary>Moves back one step. Returns <c>false</c> when already on the first step.</summary>
    public bool Back() => GoTo(CurrentIndex - 1);

    /// <summary>
    /// Moves to <paramref name="index"/>. Forward moves are refused while the current step is invalid,
    /// and any move can be cancelled from <see cref="Navigating"/>.
    /// </summary>
    public bool GoTo(int index)
    {
        if (index < 0 || index >= Steps.Count) return false;
        if (index == CurrentIndex) return false;
        if (index > CurrentIndex && CurrentStep is { IsValid: false }) return false;

        var args = new WizardNavigatingEventArgs(CurrentIndex, index);
        Navigating?.Invoke(this, args);
        if (args.Cancel) return false;

        SetCurrentValue(CurrentIndexProperty, index);
        StepChanged?.Invoke(this, EventArgs.Empty);
        return true;
    }

    /// <summary>Raises <see cref="Finished"/> — what the Finish button calls.</summary>
    public void Finish()
    {
        if (!IsLastStep || CurrentStep is { IsValid: false }) return;
        Finished?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Recomputes the button states; call it after flipping a step's validity by hand.</summary>
    public void RefreshState() => ApplyState();

    public override void OnApplyTemplate()
    {
        if (_backButton is not null) _backButton.Click -= OnBackClick;
        if (_nextButton is not null) _nextButton.Click -= OnNextClick;

        base.OnApplyTemplate();

        _backButton = GetTemplateChild("PART_Back") as Button;
        _nextButton = GetTemplateChild("PART_Next") as Button;

        if (_backButton is not null) _backButton.Click += OnBackClick;
        if (_nextButton is not null) _nextButton.Click += OnNextClick;

        ApplyState();
    }

    private void OnBackClick(object sender, RoutedEventArgs e) => Back();

    private void OnNextClick(object sender, RoutedEventArgs e)
    {
        if (IsLastStep) Finish();
        else Next();
    }

    private void OnStepsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        TrackSteps();

        int index = CurrentIndex;

        // A wizard that lost its steps has nothing to show; one that gained its first step should show it.
        if (Steps.Count == 0) index = -1;
        else if (index < 0) index = 0;
        else if (index >= Steps.Count) index = Steps.Count - 1;

        SetCurrentValue(CurrentIndexProperty, index);
        ApplyState();
    }

    private void TrackSteps()
    {
        foreach (WizardStep step in _tracked.ToList())
        {
            if (Steps.Contains(step)) continue;
            IsValidDescriptor.RemoveValueChanged(step, OnStepValidityChanged);
            _tracked.Remove(step);
        }

        foreach (WizardStep step in Steps)
        {
            if (_tracked.Add(step)) IsValidDescriptor.AddValueChanged(step, OnStepValidityChanged);
        }
    }

    private void OnStepValidityChanged(object? sender, EventArgs e) => ApplyState();

    private void ApplyState()
    {
        int index = CurrentIndex;
        WizardStep? current = index >= 0 && index < Steps.Count ? Steps[index] : null;

        for (int i = 0; i < Steps.Count; i++)
        {
            Steps[i].SetPosition(i, i == index, i < index);
        }

        SetValue(CurrentStepKey, current);
        SetValue(CanGoBackKey, index > 0);
        SetValue(CanGoNextKey, current is not null && index < Steps.Count - 1 && current.IsValid);
        SetValue(IsLastStepKey, current is not null && index == Steps.Count - 1);

        if (_backButton is not null)
        {
            _backButton.Content = BackText;
            _backButton.IsEnabled = CanGoBack;
        }

        if (_nextButton is not null)
        {
            _nextButton.Content = IsLastStep ? FinishText : NextText;
            _nextButton.IsEnabled = IsLastStep ? current is { IsValid: true } : CanGoNext;
        }
    }

    private static void OnCurrentIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Wizard wizard) wizard.ApplyState();
    }

    private static void OnButtonTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Wizard wizard) wizard.ApplyState();
    }

    private static void OnChromeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Wizard wizard) wizard.ApplyState();
    }
}
