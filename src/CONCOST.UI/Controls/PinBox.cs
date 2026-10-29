using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CONCOST.UI.Controls;

public class PinBox : Control
{
    private StackPanel? _panel;
    private readonly List<TextBox> _textBoxes = new();
    private readonly List<Border> _borders = new();
    private readonly List<string> _actualCharacters = new();
    private bool _isUpdatingText = false;

    static PinBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(PinBox), new FrameworkPropertyMetadata(typeof(PinBox)));
    }

    public static readonly DependencyProperty LengthProperty =
        DependencyProperty.Register(nameof(Length), typeof(int), typeof(PinBox),
            new FrameworkPropertyMetadata(4, OnStructureChanged));

    public static readonly DependencyProperty PasswordProperty =
        DependencyProperty.Register(nameof(Password), typeof(string), typeof(PinBox),
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnPasswordChanged));

    public static readonly DependencyProperty PasswordCharProperty =
        DependencyProperty.Register(nameof(PasswordChar), typeof(char), typeof(PinBox),
            new PropertyMetadata('●', OnStructureChanged));

    public static readonly DependencyProperty IsPasswordProperty =
        DependencyProperty.Register(nameof(IsPassword), typeof(bool), typeof(PinBox),
            new PropertyMetadata(false, OnStructureChanged));

    public static readonly DependencyProperty ItemWidthProperty =
        DependencyProperty.Register(nameof(ItemWidth), typeof(double), typeof(PinBox),
            new PropertyMetadata(48.0, OnStructureChanged));

    public static readonly DependencyProperty ItemHeightProperty =
        DependencyProperty.Register(nameof(ItemHeight), typeof(double), typeof(PinBox),
            new PropertyMetadata(52.0, OnStructureChanged));

    public static readonly DependencyProperty ItemMarginProperty =
        DependencyProperty.Register(nameof(ItemMargin), typeof(Thickness), typeof(PinBox),
            new PropertyMetadata(new Thickness(0, 0, 10, 0), OnStructureChanged));

    public static readonly DependencyProperty ItemCornerRadiusProperty =
        DependencyProperty.Register(nameof(ItemCornerRadius), typeof(CornerRadius), typeof(PinBox),
            new PropertyMetadata(new CornerRadius(8), OnStructureChanged));

    public int Length
    {
        get => (int)GetValue(LengthProperty);
        set => SetValue(LengthProperty, value);
    }

    public string Password
    {
        get => (string)GetValue(PasswordProperty);
        set => SetValue(PasswordProperty, value);
    }

    public char PasswordChar
    {
        get => (char)GetValue(PasswordCharProperty);
        set => SetValue(PasswordCharProperty, value);
    }

    public bool IsPassword
    {
        get => (bool)GetValue(IsPasswordProperty);
        set => SetValue(IsPasswordProperty, value);
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

    public Thickness ItemMargin
    {
        get => (Thickness)GetValue(ItemMarginProperty);
        set => SetValue(ItemMarginProperty, value);
    }

    public CornerRadius ItemCornerRadius
    {
        get => (CornerRadius)GetValue(ItemCornerRadiusProperty);
        set => SetValue(ItemCornerRadiusProperty, value);
    }

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        _panel = GetTemplateChild("PART_Panel") as StackPanel;
        RebuildCells();
    }

    private static void OnStructureChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is PinBox pb)
        {
            pb.RebuildCells();
        }
    }

    private static void OnPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is PinBox pb && !pb._isUpdatingText)
        {
            pb.SyncTextToBoxes();
        }
    }

    private void RebuildCells()
    {
        if (_panel == null) return;
        _panel.Children.Clear();
        _textBoxes.Clear();
        _borders.Clear();
        _actualCharacters.Clear();

        int len = Math.Max(1, Length);
        for (int i = 0; i < len; i++)
        {
            _actualCharacters.Add(string.Empty);
            int index = i;
            var border = new Border
            {
                Width = ItemWidth,
                Height = ItemHeight,
                Margin = (i == len - 1) ? new Thickness(0) : ItemMargin,
                CornerRadius = ItemCornerRadius,
                BorderThickness = new Thickness(1.5),
                Background = (Brush)FindResource("CONCOST.Brush.SurfaceSecondary"),
                BorderBrush = (Brush)FindResource("CONCOST.Brush.Border"),
                ClipToBounds = true
            };

            var tb = new TextBox
            {
                FontSize = 20,
                FontWeight = FontWeights.Normal,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center,
                BorderThickness = new Thickness(0),
                Padding = new Thickness(0),
                Background = Brushes.Transparent,
                Foreground = (Brush)FindResource("CONCOST.Brush.TextPrimary"),
                MaxLength = 1,
                CaretBrush = (Brush)FindResource("CONCOST.Brush.Primary")
            };

            var template = new ControlTemplate(typeof(TextBox));
            var factory = new FrameworkElementFactory(typeof(ScrollViewer), "PART_ContentHost");
            factory.SetValue(ScrollViewer.HorizontalScrollBarVisibilityProperty, ScrollBarVisibility.Hidden);
            factory.SetValue(ScrollViewer.VerticalScrollBarVisibilityProperty, ScrollBarVisibility.Hidden);
            factory.SetValue(FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            factory.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);
            template.VisualTree = factory;
            tb.Template = template;

            tb.PreviewKeyDown += (s, e) =>
            {
                if (e.Key == Key.Back)
                {
                    if (string.IsNullOrEmpty(_actualCharacters[index]) && index > 0)
                    {
                        _actualCharacters[index - 1] = string.Empty;
                        _textBoxes[index - 1].Text = string.Empty;
                        _textBoxes[index - 1].Focus();
                        SyncActualToPassword();
                        e.Handled = true;
                    }
                    else
                    {
                        _actualCharacters[index] = string.Empty;
                        _textBoxes[index].Text = string.Empty;
                        SyncActualToPassword();
                        e.Handled = true;
                    }
                }
                else if (e.Key == Key.Left && index > 0)
                {
                    _textBoxes[index - 1].Focus();
                    _textBoxes[index - 1].SelectAll();
                    e.Handled = true;
                }
                else if (e.Key == Key.Right && index < _textBoxes.Count - 1)
                {
                    _textBoxes[index + 1].Focus();
                    _textBoxes[index + 1].SelectAll();
                    e.Handled = true;
                }
                else if (e.Key == Key.V && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
                {
                    if (Clipboard.ContainsText())
                    {
                        var clip = Clipboard.GetText().Trim();
                        if (!string.IsNullOrEmpty(clip))
                        {
                            Password = clip.Length > Length ? clip.Substring(0, Length) : clip;
                            SyncTextToBoxes();
                            e.Handled = true;
                        }
                    }
                }
            };

            tb.PreviewTextInput += (s, e) =>
            {
                if (!string.IsNullOrEmpty(e.Text))
                {
                    char c = e.Text[0];
                    _actualCharacters[index] = c.ToString();
                    _isUpdatingText = true;
                    tb.Text = IsPassword ? PasswordChar.ToString() : c.ToString();
                    _isUpdatingText = false;

                    SyncActualToPassword();

                    if (index < _textBoxes.Count - 1)
                    {
                        _textBoxes[index + 1].Focus();
                        _textBoxes[index + 1].SelectAll();
                    }
                    e.Handled = true;
                }
            };

            tb.GotFocus += (s, e) =>
            {
                border.BorderBrush = (Brush)FindResource("CONCOST.Brush.Primary");
                tb.SelectAll();
            };

            tb.LostFocus += (s, e) =>
            {
                border.BorderBrush = (Brush)FindResource("CONCOST.Brush.Border");
            };

            border.Child = tb;
            _panel.Children.Add(border);
            _textBoxes.Add(tb);
            _borders.Add(border);
        }

        SyncTextToBoxes();
    }

    private void SyncActualToPassword()
    {
        _isUpdatingText = true;
        var sb = new StringBuilder();
        foreach (var ch in _actualCharacters)
        {
            sb.Append(ch);
        }
        Password = sb.ToString();
        _isUpdatingText = false;
    }

    private void SyncTextToBoxes()
    {
        _isUpdatingText = true;
        string pw = Password ?? string.Empty;
        for (int i = 0; i < _textBoxes.Count; i++)
        {
            if (i < pw.Length)
            {
                _actualCharacters[i] = pw[i].ToString();
                _textBoxes[i].Text = IsPassword ? PasswordChar.ToString() : pw[i].ToString();
            }
            else
            {
                _actualCharacters[i] = string.Empty;
                _textBoxes[i].Text = string.Empty;
            }
        }
        _isUpdatingText = false;
    }
}