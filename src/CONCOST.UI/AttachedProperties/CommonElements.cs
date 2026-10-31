using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Animation;
using CONCOST.UI.Icons;

namespace CONCOST.UI.AttachedProperties;

/// <summary>
/// Controls border styling, corner radius, circular pill, and hover borders.
/// </summary>
public static class BorderElement
{
    public static readonly DependencyProperty CornerRadiusProperty =
        DependencyProperty.RegisterAttached("CornerRadius", typeof(CornerRadius), typeof(BorderElement),
            new FrameworkPropertyMetadata(new CornerRadius(4), FrameworkPropertyMetadataOptions.Inherits));

    public static CornerRadius GetCornerRadius(DependencyObject obj) => (CornerRadius)obj.GetValue(CornerRadiusProperty);
    public static void SetCornerRadius(DependencyObject obj, CornerRadius value) => obj.SetValue(CornerRadiusProperty, value);

    public static readonly DependencyProperty CircularProperty =
        DependencyProperty.RegisterAttached("Circular", typeof(bool), typeof(BorderElement),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsMeasure));

    public static bool GetCircular(DependencyObject obj) => (bool)obj.GetValue(CircularProperty);
    public static void SetCircular(DependencyObject obj, bool value) => obj.SetValue(CircularProperty, value);

    public static readonly DependencyProperty HoverBorderBrushProperty =
        DependencyProperty.RegisterAttached("HoverBorderBrush", typeof(Brush), typeof(BorderElement),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));

    public static Brush? GetHoverBorderBrush(DependencyObject obj) => (Brush?)obj.GetValue(HoverBorderBrushProperty);
    public static void SetHoverBorderBrush(DependencyObject obj, Brush? value) => obj.SetValue(HoverBorderBrushProperty, value);

    public static readonly DependencyProperty HoverBackgroundProperty =
        DependencyProperty.RegisterAttached("HoverBackground", typeof(Brush), typeof(BorderElement),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));

    public static Brush? GetHoverBackground(DependencyObject obj) => (Brush?)obj.GetValue(HoverBackgroundProperty);
    public static void SetHoverBackground(DependencyObject obj, Brush? value) => obj.SetValue(HoverBackgroundProperty, value);
}

/// <summary>
/// Provides input metadata, placeholders, prefix/suffix labels, clear button, and character limit.
/// </summary>
public static class InfoElement
{
    public static readonly DependencyProperty PlaceholderProperty =
        DependencyProperty.RegisterAttached("Placeholder", typeof(string), typeof(InfoElement),
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.Inherits));

    public static string GetPlaceholder(DependencyObject obj) => (string)obj.GetValue(PlaceholderProperty);
    public static void SetPlaceholder(DependencyObject obj, string value) => obj.SetValue(PlaceholderProperty, value);

    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.RegisterAttached("Title", typeof(string), typeof(InfoElement),
            new FrameworkPropertyMetadata(null));

    public static string? GetTitle(DependencyObject obj) => (string?)obj.GetValue(TitleProperty);
    public static void SetTitle(DependencyObject obj, string? value) => obj.SetValue(TitleProperty, value);

    public static readonly DependencyProperty TitlePlacementProperty =
        DependencyProperty.RegisterAttached("TitlePlacement", typeof(Dock), typeof(InfoElement),
            new FrameworkPropertyMetadata(Dock.Top));

    public static Dock GetTitlePlacement(DependencyObject obj) => (Dock)obj.GetValue(TitlePlacementProperty);
    public static void SetTitlePlacement(DependencyObject obj, Dock value) => obj.SetValue(TitlePlacementProperty, value);

    public static readonly DependencyProperty RequiredProperty =
        DependencyProperty.RegisterAttached("Required", typeof(bool), typeof(InfoElement),
            new FrameworkPropertyMetadata(false));

    public static bool GetRequired(DependencyObject obj) => (bool)obj.GetValue(RequiredProperty);
    public static void SetRequired(DependencyObject obj, bool value) => obj.SetValue(RequiredProperty, value);

    public static readonly DependencyProperty PrefixProperty =
        DependencyProperty.RegisterAttached("Prefix", typeof(object), typeof(InfoElement),
            new FrameworkPropertyMetadata(null));

    public static object? GetPrefix(DependencyObject obj) => obj.GetValue(PrefixProperty);
    public static void SetPrefix(DependencyObject obj, object? value) => obj.SetValue(PrefixProperty, value);

    public static readonly DependencyProperty SuffixProperty =
        DependencyProperty.RegisterAttached("Suffix", typeof(object), typeof(InfoElement),
            new FrameworkPropertyMetadata(null));

    public static object? GetSuffix(DependencyObject obj) => obj.GetValue(SuffixProperty);
    public static void SetSuffix(DependencyObject obj, object? value) => obj.SetValue(SuffixProperty, value);

    public static readonly DependencyProperty ShowClearButtonProperty =
        DependencyProperty.RegisterAttached("ShowClearButton", typeof(bool), typeof(InfoElement),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.Inherits));

    public static bool GetShowClearButton(DependencyObject obj) => (bool)obj.GetValue(ShowClearButtonProperty);
    public static void SetShowClearButton(DependencyObject obj, bool value) => obj.SetValue(ShowClearButtonProperty, value);

    public static readonly DependencyProperty IsClearButtonAlwaysVisibleProperty =
        DependencyProperty.RegisterAttached("IsClearButtonAlwaysVisible", typeof(bool), typeof(InfoElement),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.Inherits));

    public static bool GetIsClearButtonAlwaysVisible(DependencyObject obj) => (bool)obj.GetValue(IsClearButtonAlwaysVisibleProperty);
    public static void SetIsClearButtonAlwaysVisible(DependencyObject obj, bool value) => obj.SetValue(IsClearButtonAlwaysVisibleProperty, value);

    public static readonly DependencyProperty ShowCharacterCountProperty =
        DependencyProperty.RegisterAttached("ShowCharacterCount", typeof(bool), typeof(InfoElement),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.Inherits, OnShowCharacterCountChanged));

    public static bool GetShowCharacterCount(DependencyObject obj) => (bool)obj.GetValue(ShowCharacterCountProperty);
    public static void SetShowCharacterCount(DependencyObject obj, bool value) => obj.SetValue(ShowCharacterCountProperty, value);

    private static void OnShowCharacterCountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TextBox tb)
        {
            TextBoxHelper.UpdateCharacterCount(tb);
            if ((bool)e.NewValue)
                tb.TextChanged += OnTextBoxTextChangedForCount;
            else
                tb.TextChanged -= OnTextBoxTextChangedForCount;
        }
    }

    private static void OnTextBoxTextChangedForCount(object sender, TextChangedEventArgs e)
    {
        if (sender is TextBox tb)
        {
            TextBoxHelper.UpdateCharacterCount(tb);
        }
    }

    private static RoutedUICommand? _clearCommand;
    public static RoutedUICommand ClearCommand =>
        _clearCommand ??= new RoutedUICommand("Clear Text", "Clear", typeof(InfoElement));

    static InfoElement()
    {
        CommandManager.RegisterClassCommandBinding(
            typeof(TextBox),
            new CommandBinding(ClearCommand, (sender, e) =>
            {
                if (sender is TextBox tb)
                {
                    tb.Clear();
                    tb.Focus();
                }
            }, (sender, e) =>
            {
                if (sender is TextBox tb)
                    e.CanExecute = !string.IsNullOrEmpty(tb.Text);
            }));
    }
}

/// <summary>
/// Attaches vector icons, placement, size, and secondary state icons.
/// </summary>
public static class IconElement
{
    public static readonly DependencyProperty KindProperty =
        DependencyProperty.RegisterAttached("Kind", typeof(IconKind), typeof(IconElement),
            new FrameworkPropertyMetadata(IconKind.None, FrameworkPropertyMetadataOptions.Inherits));

    public static IconKind GetKind(DependencyObject obj) => (IconKind)obj.GetValue(KindProperty);
    public static void SetKind(DependencyObject obj, IconKind value) => obj.SetValue(KindProperty, value);

    public static readonly DependencyProperty IconSizeProperty =
        DependencyProperty.RegisterAttached("IconSize", typeof(double), typeof(IconElement),
            new FrameworkPropertyMetadata(16.0, FrameworkPropertyMetadataOptions.Inherits));

    public static double GetIconSize(DependencyObject obj) => (double)obj.GetValue(IconSizeProperty);
    public static void SetIconSize(DependencyObject obj, double value) => obj.SetValue(IconSizeProperty, value);

    public static readonly DependencyProperty PlacementProperty =
        DependencyProperty.RegisterAttached("Placement", typeof(Dock), typeof(IconElement),
            new FrameworkPropertyMetadata(Dock.Left));

    public static Dock GetPlacement(DependencyObject obj) => (Dock)obj.GetValue(PlacementProperty);
    public static void SetPlacement(DependencyObject obj, Dock value) => obj.SetValue(PlacementProperty, value);
}

/// <summary>
/// Advanced button features: Loading spinner, Ripple wave effect, Badge counters.
/// </summary>
public static class ButtonElement
{
    public static readonly DependencyProperty IsLoadingProperty =
        DependencyProperty.RegisterAttached("IsLoading", typeof(bool), typeof(ButtonElement),
            new FrameworkPropertyMetadata(false));

    public static bool GetIsLoading(DependencyObject obj) => (bool)obj.GetValue(IsLoadingProperty);
    public static void SetIsLoading(DependencyObject obj, bool value) => obj.SetValue(IsLoadingProperty, value);

    public static readonly DependencyProperty LoadingTextProperty =
        DependencyProperty.RegisterAttached("LoadingText", typeof(string), typeof(ButtonElement),
            new FrameworkPropertyMetadata("Loading..."));

    public static string GetLoadingText(DependencyObject obj) => (string)obj.GetValue(LoadingTextProperty);
    public static void SetLoadingText(DependencyObject obj, string value) => obj.SetValue(LoadingTextProperty, value);

    public static readonly DependencyProperty BadgeProperty =
        DependencyProperty.RegisterAttached("Badge", typeof(object), typeof(ButtonElement),
            new FrameworkPropertyMetadata(null));

    public static object? GetBadge(DependencyObject obj) => obj.GetValue(BadgeProperty);
    public static void SetBadge(DependencyObject obj, object? value) => obj.SetValue(BadgeProperty, value);
}

/// <summary>
/// Floating micro-interaction and scale effects on hover or focus.
/// </summary>
public static class VisualElement
{
    public static readonly DependencyProperty ElevationProperty =
        DependencyProperty.RegisterAttached("Elevation", typeof(int), typeof(VisualElement),
            new FrameworkPropertyMetadata(0));

    public static int GetElevation(DependencyObject obj) => (int)obj.GetValue(ElevationProperty);
    public static void SetElevation(DependencyObject obj, int value) => obj.SetValue(ElevationProperty, value);

    public static readonly DependencyProperty HoverScaleProperty =
        DependencyProperty.RegisterAttached("HoverScale", typeof(double), typeof(VisualElement),
            new FrameworkPropertyMetadata(1.0, OnHoverScaleChanged));

    public static double GetHoverScale(DependencyObject obj) => (double)obj.GetValue(HoverScaleProperty);
    public static void SetHoverScale(DependencyObject obj, double value) => obj.SetValue(HoverScaleProperty, value);

    private static void OnHoverScaleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not UIElement element) return;
        double scale = (double)e.NewValue;

        if (scale > 1.0)
        {
            var transform = new ScaleTransform(1.0, 1.0);
            element.RenderTransformOrigin = new Point(0.5, 0.5);
            element.RenderTransform = transform;

            element.MouseEnter += (s, ev) =>
            {
                var anim = new DoubleAnimation(scale, TimeSpan.FromMilliseconds(150)) { EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut } };
                transform.BeginAnimation(ScaleTransform.ScaleXProperty, anim);
                transform.BeginAnimation(ScaleTransform.ScaleYProperty, anim);
            };
        }
    }
}

/// <summary>
/// Attached properties for ProgressBar: Show progress text / percentage label, format string pattern.
/// </summary>
public static class ProgressBarElement
{
    public static readonly DependencyProperty ShowTextProperty =
        DependencyProperty.RegisterAttached("ShowText", typeof(bool), typeof(ProgressBarElement),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.Inherits));

    public static bool GetShowText(DependencyObject obj) => (bool)obj.GetValue(ShowTextProperty);
    public static void SetShowText(DependencyObject obj, bool value) => obj.SetValue(ShowTextProperty, value);

    public static readonly DependencyProperty TextPatternProperty =
        DependencyProperty.RegisterAttached("TextPattern", typeof(string), typeof(ProgressBarElement),
            new FrameworkPropertyMetadata("{0:0}%", FrameworkPropertyMetadataOptions.Inherits));

    public static string GetTextPattern(DependencyObject obj) => (string)obj.GetValue(TextPatternProperty);
    public static void SetTextPattern(DependencyObject obj, string value) => obj.SetValue(TextPatternProperty, value);
}

/// <summary>
/// Attached properties for DataGrid: Row spacing, row margin, divider lines, and Header/Cell alignment.
/// </summary>
public static class DataGridElement
{
    public static readonly DependencyProperty RowSpacingProperty =
        DependencyProperty.RegisterAttached("RowSpacing", typeof(double), typeof(DataGridElement),
            new FrameworkPropertyMetadata(4.0, FrameworkPropertyMetadataOptions.Inherits));

    public static double GetRowSpacing(DependencyObject obj) => (double)obj.GetValue(RowSpacingProperty);
    public static void SetRowSpacing(DependencyObject obj, double value) => obj.SetValue(RowSpacingProperty, value);

    public static readonly DependencyProperty ShowRowDividerProperty =
        DependencyProperty.RegisterAttached("ShowRowDivider", typeof(bool), typeof(DataGridElement),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.Inherits));

    public static bool GetShowRowDivider(DependencyObject obj) => (bool)obj.GetValue(ShowRowDividerProperty);
    public static void SetShowRowDivider(DependencyObject obj, bool value) => obj.SetValue(ShowRowDividerProperty, value);

    public static readonly DependencyProperty HeaderHorizontalContentAlignmentProperty =
        DependencyProperty.RegisterAttached("HeaderHorizontalContentAlignment", typeof(HorizontalAlignment), typeof(DataGridElement),
            new FrameworkPropertyMetadata(HorizontalAlignment.Left, FrameworkPropertyMetadataOptions.Inherits));

    public static HorizontalAlignment GetHeaderHorizontalContentAlignment(DependencyObject obj) => (HorizontalAlignment)obj.GetValue(HeaderHorizontalContentAlignmentProperty);
    public static void SetHeaderHorizontalContentAlignment(DependencyObject obj, HorizontalAlignment value) => obj.SetValue(HeaderHorizontalContentAlignmentProperty, value);

    public static readonly DependencyProperty CellHorizontalContentAlignmentProperty =
        DependencyProperty.RegisterAttached("CellHorizontalContentAlignment", typeof(HorizontalAlignment), typeof(DataGridElement),
            new FrameworkPropertyMetadata(HorizontalAlignment.Left, FrameworkPropertyMetadataOptions.Inherits));

    public static HorizontalAlignment GetCellHorizontalContentAlignment(DependencyObject obj) => (HorizontalAlignment)obj.GetValue(CellHorizontalContentAlignmentProperty);
    public static void SetCellHorizontalContentAlignment(DependencyObject obj, HorizontalAlignment value) => obj.SetValue(CellHorizontalContentAlignmentProperty, value);

    // ── Quick per-column helpers ──

    public static readonly DependencyProperty TextHeaderProperty =
        DependencyProperty.RegisterAttached("TextHeader", typeof(string), typeof(DataGridElement),
            new PropertyMetadata(null, OnTextHeaderChanged));

    public static string GetTextHeader(DependencyObject obj) => (string)obj.GetValue(TextHeaderProperty);
    public static void SetTextHeader(DependencyObject obj, string value) => obj.SetValue(TextHeaderProperty, value);

    private static void OnTextHeaderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DataGridColumn col && e.NewValue is string s)
            col.Header = s;
    }

    public static readonly DependencyProperty HeaderAlignmentProperty =
        DependencyProperty.RegisterAttached("HeaderAlignment", typeof(HorizontalAlignment?), typeof(DataGridElement),
            new PropertyMetadata(null, OnHeaderAlignmentChanged));

    public static HorizontalAlignment? GetHeaderAlignment(DependencyObject obj) => (HorizontalAlignment?)obj.GetValue(HeaderAlignmentProperty);
    public static void SetHeaderAlignment(DependencyObject obj, HorizontalAlignment? value) => obj.SetValue(HeaderAlignmentProperty, value);

    private static void OnHeaderAlignmentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not DataGridColumn col || e.NewValue is not HorizontalAlignment ha) return;

        var existing = col.HeaderStyle;
        var style = existing != null
            ? new Style(typeof(DataGridColumnHeader), existing)
            : new Style(typeof(DataGridColumnHeader));
        style.Setters.Add(new Setter(Control.HorizontalContentAlignmentProperty, ha));
        col.HeaderStyle = style;
    }

    public static readonly DependencyProperty CellAlignmentProperty =
        DependencyProperty.RegisterAttached("CellAlignment", typeof(HorizontalAlignment?), typeof(DataGridElement),
            new PropertyMetadata(null, OnCellAlignmentChanged));

    public static HorizontalAlignment? GetCellAlignment(DependencyObject obj) => (HorizontalAlignment?)obj.GetValue(CellAlignmentProperty);
    public static void SetCellAlignment(DependencyObject obj, HorizontalAlignment? value) => obj.SetValue(CellAlignmentProperty, value);

    private static void OnCellAlignmentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not DataGridBoundColumn col || e.NewValue is not HorizontalAlignment ha) return;

        var existing = col.ElementStyle;
        var style = existing != null
            ? new Style(typeof(TextBlock), existing)
            : new Style(typeof(TextBlock));
        style.Setters.Add(new Setter(FrameworkElement.HorizontalAlignmentProperty, ha));
        col.ElementStyle = style;

        if (col is DataGridTextColumn textCol)
        {
            var editExisting = textCol.EditingElementStyle;
            var editStyle = editExisting != null
                ? new Style(typeof(TextBox), editExisting)
                : new Style(typeof(TextBox));
            editStyle.Setters.Add(new Setter(Control.HorizontalContentAlignmentProperty, ha));
            textCol.EditingElementStyle = editStyle;
        }
    }
}


