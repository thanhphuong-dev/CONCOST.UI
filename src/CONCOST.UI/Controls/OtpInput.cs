using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CONCOST.UI.Controls;

/// <summary>
/// A fixed-length verification code entry — the one-time password field of an
/// MFA dialog. Each digit lives in its own cell: typing advances, Backspace
/// retreats, the arrow keys walk the cells and a pasted code fills the cells
/// from wherever the caret stands. <see cref="Completed"/> fires when the last
/// cell receives its character.
/// </summary>
/// <remarks>
/// For secret entry (a licence PIN nobody should read over the shoulder) use
/// <see cref="PinBox"/>, which masks as it types; this control keeps the code
/// readable because a verification code is meant to be checked against a
/// message on the same screen.
/// </remarks>
public class OtpInput : Control
{
    static OtpInput()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(OtpInput), new FrameworkPropertyMetadata(typeof(OtpInput)));
    }

    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(nameof(Value), typeof(string), typeof(OtpInput),
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValueChanged));

    public static readonly DependencyProperty LengthProperty =
        DependencyProperty.Register(nameof(Length), typeof(int), typeof(OtpInput),
            new FrameworkPropertyMetadata(4, OnStructureChanged, CoerceLength));

    public static readonly DependencyProperty NumericOnlyProperty =
        DependencyProperty.Register(nameof(NumericOnly), typeof(bool), typeof(OtpInput),
            new FrameworkPropertyMetadata(true, OnStructureChanged));

    public static readonly DependencyProperty MaskCharacterProperty =
        DependencyProperty.Register(nameof(MaskCharacter), typeof(char), typeof(OtpInput),
            new FrameworkPropertyMetadata('\0', OnValueChanged));

    public static readonly DependencyProperty ItemWidthProperty =
        DependencyProperty.Register(nameof(ItemWidth), typeof(double), typeof(OtpInput),
            new FrameworkPropertyMetadata(36.0, OnStructureChanged));

    public static readonly DependencyProperty ItemHeightProperty =
        DependencyProperty.Register(nameof(ItemHeight), typeof(double), typeof(OtpInput),
            new FrameworkPropertyMetadata(42.0, OnStructureChanged));

    public static readonly DependencyProperty ItemSpacingProperty =
        DependencyProperty.Register(nameof(ItemSpacing), typeof(double), typeof(OtpInput),
            new FrameworkPropertyMetadata(6.0, OnStructureChanged));

    public static readonly DependencyProperty ItemCornerRadiusProperty =
        DependencyProperty.Register(nameof(ItemCornerRadius), typeof(CornerRadius), typeof(OtpInput),
            new FrameworkPropertyMetadata(new CornerRadius(6), OnStructureChanged));

    /// <summary>Identifies the <see cref="Completed"/> routed event.</summary>
    public static readonly RoutedEvent CompletedEvent = EventManager.RegisterRoutedEvent(
        nameof(Completed), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(OtpInput));

    private static readonly DependencyPropertyKey IsCompletedPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(IsCompleted), typeof(bool), typeof(OtpInput),
            new PropertyMetadata(false));

    /// <summary>Identifies the <see cref="IsCompleted"/> read-only property.</summary>
    public static readonly DependencyProperty IsCompletedProperty = IsCompletedPropertyKey.DependencyProperty;

    private StackPanel? _panel;
    private readonly List<TextBox> _cells = new();
    private readonly List<Border> _borders = new();
    private readonly List<string> _characters = new();
    private bool _syncing;

    public OtpInput()
    {
        Loaded += (_, _) => RestyleBorders();
    }

    /// <summary>The code entered so far, as one string.</summary>
    public string Value
    {
        get => (string)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    /// <summary>Number of cells; the control carries that many characters exactly.</summary>
    public int Length
    {
        get => (int)GetValue(LengthProperty);
        set => SetValue(LengthProperty, value);
    }

    /// <summary>Accepts digits only when true (the default for a verification code); any character when false.</summary>
    public bool NumericOnly
    {
        get => (bool)GetValue(NumericOnlyProperty);
        set => SetValue(NumericOnlyProperty, value);
    }

    /// <summary>Displays this character instead of the typed one; <c>\\0</c> shows the code as typed.</summary>
    public char MaskCharacter
    {
        get => (char)GetValue(MaskCharacterProperty);
        set => SetValue(MaskCharacterProperty, value);
    }

    public double ItemWidth
    {
        get => (double)GetValue(ItemWidthProperty);
        set => SetValue(ItemWidthProperty, value);
    }

    public double ItemHeight
    {
        get => (double)GetValue(ItemHeightProperty);
        set => SetValue(ItemHeightProperty, value);
    }

    public double ItemSpacing
    {
        get => (double)GetValue(ItemSpacingProperty);
        set => SetValue(ItemSpacingProperty, value);
    }

    public CornerRadius ItemCornerRadius
    {
        get => (CornerRadius)GetValue(ItemCornerRadiusProperty);
        set => SetValue(ItemCornerRadiusProperty, value);
    }

    /// <summary>True when every cell carries a character.</summary>
    public bool IsCompleted => (bool)GetValue(IsCompletedProperty);

    /// <summary>Raised when the last cell receives its character.</summary>
    public event RoutedEventHandler Completed
    {
        add => AddHandler(CompletedEvent, value);
        remove => RemoveHandler(CompletedEvent, value);
    }

    /// <summary>Clears every cell and returns focus to the first one.</summary>
    public void Clear()
    {
        Value = string.Empty;
        if (_cells.Count > 0) _cells[0].Focus();
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        _panel = GetTemplateChild("PART_Panel") as StackPanel;
        RebuildCells();
    }

    private static object CoerceLength(DependencyObject d, object value)
    {
        var length = (int)value;
        if (length < 1) return 1;
        if (length > 10) return 10;
        return length;
    }

    private static void OnStructureChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is OtpInput otp) otp.RebuildCells();
    }

    private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is OtpInput otp && !otp._syncing) otp.SyncCellsFromValue();
    }

    private void RebuildCells()
    {
        if (_panel is null) return;

        _panel.Children.Clear();
        _cells.Clear();
        _borders.Clear();
        _characters.Clear();

        for (int i = 0; i < Length; i++)
        {
            _characters.Add(string.Empty);
            BuildCell(i);
        }

        SyncCellsFromValue();
        // A host that shrank Length while a code was showing leaves the extra
        // characters with nowhere to live — trim them out of the model too.
        WriteValue();
    }

    private void BuildCell(int index)
    {
        var border = new Border
        {
            Width = ItemWidth,
            Height = ItemHeight,
            CornerRadius = ItemCornerRadius,
            BorderThickness = new Thickness(1.5),
            Margin = index == Length - 1 ? default : new Thickness(0, 0, ItemSpacing, 0),
            SnapsToDevicePixels = true
        };

        var box = new TextBox
        {
            FontSize = Math.Max(14, ItemHeight * 0.45),
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Center,
            BorderThickness = new Thickness(0),
            Padding = new Thickness(0),
            Background = Brushes.Transparent,
            MaxLength = 1,
            CaretBrush = Brushes.Transparent
        };

        box.Template = BuildBareTextBoxTemplate();

        int captured = index;
        box.PreviewTextInput += (_, e) => OnCellTextInput(captured, e);
        box.PreviewKeyDown += (_, e) => OnCellKeyDown(captured, e);
        box.TextChanged += (_, _) => OnCellTextChanged(captured);
        box.GotKeyboardFocus += (_, _) => HighlightCell(captured, focused: true);
        box.LostKeyboardFocus += (_, _) => HighlightCell(captured, focused: false);

        _borders.Add(border);
        _cells.Add(box);
        border.Child = box;
        _panel!.Children.Add(border);
    }

    private void OnCellTextInput(int index, TextCompositionEventArgs e)
    {
        e.Handled = true;

        string text = e.Text ?? string.Empty;
        if (text.Length == 0) return;

        char character = text[0];
        if (!Accepts(character)) return;

        SetCharacter(index, character);

        if (index < _cells.Count - 1)
        {
            _cells[index + 1].Focus();
            _cells[index + 1].SelectAll();
        }
        else
        {
            _cells[index].Focus();
            _cells[index].SelectAll();
        }

        RaiseCompletedIfFull();
    }

    private void OnCellKeyDown(int index, KeyEventArgs e)
    {
        bool paste = e.Key == Key.V && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control;
        if (paste)
        {
            e.Handled = PasteIntoCells(index);
            return;
        }

        switch (e.Key)
        {
            case Key.Back:
                e.Handled = BackspaceAt(index);
                break;
            case Key.Delete:
                e.Handled = DeleteAt(index);
                break;
            case Key.Left when index > 0:
                _cells[index - 1].Focus();
                _cells[index - 1].SelectAll();
                e.Handled = true;
                break;
            case Key.Right when index < _cells.Count - 1:
                _cells[index + 1].Focus();
                _cells[index + 1].SelectAll();
                e.Handled = true;
                break;
            case Key.Home:
                _cells[0].Focus();
                _cells[0].SelectAll();
                e.Handled = true;
                break;
            case Key.End:
                _cells[_cells.Count - 1].Focus();
                _cells[_cells.Count - 1].SelectAll();
                e.Handled = true;
                break;
        }
    }

    private void OnCellTextChanged(int index)
    {
        // A TextChanged that was not ours (IME, accessibility tools) still lands
        // in the model so Value never disagrees with what is on screen.
        if (_syncing) return;

        var box = _cells[index];
        string text = box.Text ?? string.Empty;

        if (text.Length > 0 && Accepts(text[0]))
        {
            SetCharacter(index, text[0]);
            RaiseCompletedIfFull();
        }
        else
        {
            SetCharacter(index, null);
        }
    }

    private bool PasteIntoCells(int startIndex)
    {
        if (!Clipboard.ContainsText()) return false;

        string clip = (Clipboard.GetText() ?? string.Empty).Trim();
        if (clip.Length == 0) return false;

        int written = 0;
        for (int i = startIndex; i < _cells.Count && written < clip.Length; i++, written++)
        {
            char character = clip[written];
            if (!Accepts(character)) break;
            SetCharacter(i, character);
        }

        int next = Math.Min(startIndex + written, _cells.Count - 1);
        _cells[next].Focus();
        _cells[next].SelectAll();
        RaiseCompletedIfFull();

        return true;
    }

    private bool BackspaceAt(int index)
    {
        if (string.IsNullOrEmpty(_characters[index]))
        {
            if (index == 0) return true;

            int previous = index - 1;
            SetCharacter(previous, null);
            _cells[previous].Focus();
            _cells[previous].SelectAll();
            return true;
        }

        SetCharacter(index, null);
        return true;
    }

    private bool DeleteAt(int index)
    {
        for (int i = index; i < _cells.Count - 1; i++)
        {
            SetCharacter(i, string.IsNullOrEmpty(_characters[i + 1]) ? null : _characters[i + 1][0]);
        }

        SetCharacter(_cells.Count - 1, null);
        return true;
    }

    private void SetCharacter(int index, char? character)
    {
        _syncing = true;
        try
        {
            _characters[index] = character?.ToString() ?? string.Empty;

            var box = _cells[index];
            bool focused = box.IsKeyboardFocusWithin;
            box.Text = character is null ? string.Empty : Display(character.Value).ToString();
            if (focused && box.Text.Length > 0) box.SelectAll();

            WriteValue();
            UpdateCompletedFlag();
        }
        finally
        {
            _syncing = false;
        }
    }

    private void SyncCellsFromValue()
    {
        if (_panel is null || _cells.Count == 0) return;

        _syncing = true;
        try
        {
            string value = Value ?? string.Empty;

            for (int i = 0; i < _cells.Count; i++)
            {
                _characters[i] = i < value.Length ? value[i].ToString() : string.Empty;
                _cells[i].Text = i < value.Length ? Display(value[i]).ToString() : string.Empty;
            }

            UpdateCompletedFlag();
        }
        finally
        {
            _syncing = false;
        }
    }

    private void WriteValue()
    {
        var builder = new System.Text.StringBuilder();
        foreach (string character in _characters) builder.Append(character);

        string value = builder.ToString();
        if (Value != value) Value = value;
    }

    private void UpdateCompletedFlag()
    {
        bool complete = _characters.Count > 0;
        foreach (string character in _characters)
        {
            if (character.Length == 0)
            {
                complete = false;
                break;
            }
        }

        SetValue(IsCompletedPropertyKey, complete);
    }

    private void RaiseCompletedIfFull()
    {
        if (!IsCompleted) return;

        RaiseEvent(new RoutedEventArgs(CompletedEvent));
    }

    private char Display(char character) => MaskCharacter == '\0' ? character : MaskCharacter;

    private bool Accepts(char character)
    {
        if (char.IsControl(character)) return false;
        if (!NumericOnly) return true;
        return char.IsDigit(character);
    }

    private void HighlightCell(int index, bool focused)
    {
        if (index < 0 || index >= _borders.Count) return;

        RestyleBorders();
        _borders[index].BorderBrush = focused ? FindBrush("CONCOST.Brush.Primary", Brushes.DodgerBlue) : FindBrush("CONCOST.Brush.Border", Brushes.Gray);
    }

    private void RestyleBorders()
    {
        var borderBrush = FindBrush("CONCOST.Brush.Border", Brushes.Gray);
        var surface = FindBrush("CONCOST.Brush.SurfaceSecondary", new SolidColorBrush(Color.FromRgb(0xF3, 0xF4, 0xF6)));

        foreach (var border in _borders)
        {
            border.Background = surface;
            border.BorderBrush = borderBrush;
        }
    }

    private static Brush FindBrush(string key, Brush fallback) =>
        Application.Current?.TryFindResource(key) as Brush ?? fallback;

    private static ControlTemplate BuildBareTextBoxTemplate()
    {
        var template = new ControlTemplate(typeof(TextBox));
        var host = new FrameworkElementFactory(typeof(ScrollViewer), "PART_ContentHost");
        host.SetValue(ScrollViewer.HorizontalScrollBarVisibilityProperty, ScrollBarVisibility.Hidden);
        host.SetValue(ScrollViewer.VerticalScrollBarVisibilityProperty, ScrollBarVisibility.Hidden);
        host.SetValue(FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Center);
        host.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);
        template.VisualTree = host;
        return template;
    }
}
