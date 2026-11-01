using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace CONCOST.UI.Controls;

/// <summary>
/// A font chooser: family, size, bold and italic, with a live sample underneath.
/// </summary>
/// <remarks>
/// <para>
/// Every family the machine has is offered, sorted by name, and the combos are editable so a size the
/// list does not carry can still be typed. The four values are separate dependency properties rather
/// than one <see cref="TextElement"/> style, because a host usually binds them to four fields of its own
/// model and cannot unwrap a composite.
/// </para>
/// <para>
/// The sample is the point: a family name says nothing about legibility at 8 points, and picking a
/// font for a drawing note is a judgement the sample has to support.
/// </para>
/// </remarks>
public class FontPicker : Grid
{
    /// <summary>Identifies the <see cref="SelectedFontFamily"/> property.</summary>
    public static readonly DependencyProperty SelectedFontFamilyProperty = DependencyProperty.Register(
        nameof(SelectedFontFamily), typeof(FontFamily), typeof(FontPicker),
        new FrameworkPropertyMetadata(new FontFamily("Segoe UI"), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnAppearanceChanged));

    /// <summary>Identifies the <see cref="SelectedFontSize"/> property.</summary>
    public static readonly DependencyProperty SelectedFontSizeProperty = DependencyProperty.Register(
        nameof(SelectedFontSize), typeof(double), typeof(FontPicker),
        new FrameworkPropertyMetadata(12.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnAppearanceChanged));

    /// <summary>Identifies the <see cref="IsBold"/> property.</summary>
    public static readonly DependencyProperty IsBoldProperty = DependencyProperty.Register(
        nameof(IsBold), typeof(bool), typeof(FontPicker),
        new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnAppearanceChanged));

    /// <summary>Identifies the <see cref="IsItalic"/> property.</summary>
    public static readonly DependencyProperty IsItalicProperty = DependencyProperty.Register(
        nameof(IsItalic), typeof(bool), typeof(FontPicker),
        new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnAppearanceChanged));

    /// <summary>Identifies the <see cref="ShowPreview"/> property.</summary>
    public static readonly DependencyProperty ShowPreviewProperty = DependencyProperty.Register(
        nameof(ShowPreview), typeof(bool), typeof(FontPicker), new FrameworkPropertyMetadata(true, OnPreviewVisibilityChanged));

    /// <summary>Identifies the <see cref="PreviewText"/> property.</summary>
    public static readonly DependencyProperty PreviewTextProperty = DependencyProperty.Register(
        nameof(PreviewText), typeof(string), typeof(FontPicker),
        new FrameworkPropertyMetadata("The quick brown fox jumps over the lazy dog", OnPreviewTextChanged));

    private static readonly double[] Sizes = { 8, 9, 10, 11, 12, 14, 16, 18, 20, 24, 28, 32, 36, 48, 72 };

    private readonly ComboBox _family;
    private readonly ComboBox _size;
    private readonly ToggleButton _bold;
    private readonly ToggleButton _italic;
    private readonly TextBlock _preview;

    private bool _syncing;

    public FontPicker()
    {
        RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(72) });
        ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        _family = new ComboBox
        {
            Height = 26,
            FontSize = 12,
            IsEditable = true,
            IsTextSearchEnabled = true,
            ItemsSource = Fonts.SystemFontFamilies.OrderBy(family => family.Source, StringComparer.CurrentCultureIgnoreCase).ToList(),
            DisplayMemberPath = "Source",
            ToolTip = "Font family"
        };

        _family.SelectionChanged += (_, _) =>
        {
            if (_syncing) return;

            if (_family.SelectedItem is FontFamily selected) SelectedFontFamily = selected;
        };

        _family.LostKeyboardFocus += (_, _) => CommitTypedFamily();

        SetColumn(_family, 0);
        Children.Add(_family);

        _size = new ComboBox
        {
            Height = 26,
            Margin = new Thickness(6, 0, 0, 0),
            FontSize = 12,
            IsEditable = true,
            ItemsSource = Sizes.ToList(),
            Text = "12",
            ToolTip = "Font size"
        };

        _size.SelectionChanged += (_, _) =>
        {
            if (_syncing) return;

            if (_size.SelectedItem is double chosen) SelectedFontSize = chosen;
        };

        _size.LostKeyboardFocus += (_, _) => CommitTypedSize();

        SetColumn(_size, 1);
        Children.Add(_size);

        _bold = CreateStyleToggle("B", "Bold", isBold: true);
        _italic = CreateStyleToggle("I", "Italic", isBold: false);

        SetColumn(_bold, 2);
        Children.Add(_bold);

        SetColumn(_italic, 3);
        Children.Add(_italic);

        _preview = new TextBlock
        {
            Margin = new Thickness(0, 8, 0, 0),
            TextWrapping = TextWrapping.Wrap,
            TextTrimming = TextTrimming.CharacterEllipsis,
            MaxHeight = 48
        };

        _preview.SetResourceReference(TextBlock.ForegroundProperty, "CONCOST.Brush.TextPrimary");

        SetRow(_preview, 1);
        SetColumnSpan(_preview, 4);

        Children.Add(_preview);

        ApplyAppearance();
    }

    /// <summary>The chosen family.</summary>
    public FontFamily SelectedFontFamily
    {
        get => (FontFamily)GetValue(SelectedFontFamilyProperty);
        set => SetValue(SelectedFontFamilyProperty, value);
    }

    /// <summary>The chosen size in device-independent points.</summary>
    public double SelectedFontSize
    {
        get => (double)GetValue(SelectedFontSizeProperty);
        set => SetValue(SelectedFontSizeProperty, value);
    }

    /// <summary>Whether the text is bold.</summary>
    public bool IsBold
    {
        get => (bool)GetValue(IsBoldProperty);
        set => SetValue(IsBoldProperty, value);
    }

    /// <summary>Whether the text is italic.</summary>
    public bool IsItalic
    {
        get => (bool)GetValue(IsItalicProperty);
        set => SetValue(IsItalicProperty, value);
    }

    /// <summary>Shows the sample line under the controls.</summary>
    public bool ShowPreview
    {
        get => (bool)GetValue(ShowPreviewProperty);
        set => SetValue(ShowPreviewProperty, value);
    }

    /// <summary>The text the sample shows.</summary>
    public string PreviewText
    {
        get => (string)GetValue(PreviewTextProperty);
        set => SetValue(PreviewTextProperty, value);
    }

    /// <summary>The weight the chosen values amount to.</summary>
    public FontWeight EffectiveWeight => IsBold ? FontWeights.Bold : FontWeights.Normal;

    /// <summary>The style the chosen values amount to.</summary>
    public FontStyle EffectiveStyle => IsItalic ? FontStyles.Italic : FontStyles.Normal;

    /// <summary>Raised when any of the four values changes.</summary>
    public event EventHandler? SelectionChanged;

    private ToggleButton CreateStyleToggle(string glyph, string tooltip, bool isBold)
    {
        var label = new TextBlock
        {
            Text = glyph,
            FontSize = 13,
            FontWeight = isBold ? FontWeights.Bold : FontWeights.Normal,
            FontStyle = isBold ? FontStyles.Normal : FontStyles.Italic
        };

        var button = new ToggleButton
        {
            Width = 26,
            Height = 26,
            Margin = new Thickness(4, 0, 0, 0),
            ToolTip = tooltip,
            Content = label
        };

        button.SetResourceReference(StyleProperty, "CONCOST.ToggleButton.Base");

        button.Click += (_, _) =>
        {
            if (isBold) IsBold = button.IsChecked == true;
            else IsItalic = button.IsChecked == true;
        };

        return button;
    }

    private void CommitTypedFamily()
    {
        string typed = (_family.Text ?? string.Empty).Trim();

        if (typed.Length == 0) return;

        var match = Fonts.SystemFontFamilies.FirstOrDefault(family =>
            string.Equals(family.Source, typed, StringComparison.CurrentCultureIgnoreCase));

        if (match is not null) SelectedFontFamily = match;
    }

    private void CommitTypedSize()
    {
        string typed = (_size.Text ?? string.Empty).Trim();

        if (!double.TryParse(typed, out double size) || size <= 0) return;

        SelectedFontSize = size;
    }

    private void ApplyAppearance()
    {
        _preview.Text = PreviewText;
        _preview.FontFamily = SelectedFontFamily;
        _preview.FontSize = Math.Max(1, SelectedFontSize);
        _preview.FontWeight = EffectiveWeight;
        _preview.FontStyle = EffectiveStyle;

        _preview.Visibility = ShowPreview ? Visibility.Visible : Visibility.Collapsed;

        _syncing = true;

        try
        {
            _family.SelectedItem = SelectedFontFamily;
            _size.Text = SelectedFontSize.ToString("0.##", System.Globalization.CultureInfo.CurrentCulture);
            _bold.IsChecked = IsBold;
            _italic.IsChecked = IsItalic;
        }
        finally
        {
            _syncing = false;
        }
    }

    private static void OnAppearanceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not FontPicker picker) return;

        picker.ApplyAppearance();

        if (!picker._syncing) picker.SelectionChanged?.Invoke(picker, EventArgs.Empty);
    }

    private static void OnPreviewVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FontPicker picker) picker.ApplyAppearance();
    }

    private static void OnPreviewTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FontPicker picker) picker._preview.Text = (string)e.NewValue ?? string.Empty;
    }
}
