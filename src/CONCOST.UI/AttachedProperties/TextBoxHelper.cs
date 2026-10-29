using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CONCOST.UI.AttachedProperties;

/// <summary>
/// Supported input restriction types for TextBox.
/// </summary>
public enum TextBoxInputType
{
    None,
    DigitsOnly,
    PositiveInteger,
    Integer,
    PositiveDecimal,
    Decimal,
    Alphanumeric,
    Formula
}

/// <summary>
/// Advanced attached properties and behaviors for TextBox:
/// Input filtering/masks, SelectAllOnFocus, ClearOnEscape, CommitOnEnter, Character count tracking.
/// </summary>
public static class TextBoxHelper
{
    #region CharacterCountText (Read-only Attached)

    private static readonly DependencyPropertyKey CharacterCountTextPropertyKey =
        DependencyProperty.RegisterAttachedReadOnly(
            "CharacterCountText",
            typeof(string),
            typeof(TextBoxHelper),
            new FrameworkPropertyMetadata(string.Empty));

    public static readonly DependencyProperty CharacterCountTextProperty =
        CharacterCountTextPropertyKey.DependencyProperty;

    public static string GetCharacterCountText(DependencyObject obj) =>
        (string)obj.GetValue(CharacterCountTextProperty);

    internal static void SetCharacterCountText(DependencyObject obj, string value) =>
        obj.SetValue(CharacterCountTextPropertyKey, value);

    public static void UpdateCharacterCount(TextBox tb)
    {
        if (tb == null) return;
        int len = tb.Text?.Length ?? 0;
        int max = tb.MaxLength;
        string text = max > 0 ? $"{len}/{max}" : $"{len}";
        SetCharacterCountText(tb, text);
    }

    #endregion

    #region SelectAllOnFocus

    public static readonly DependencyProperty SelectAllOnFocusProperty =
        DependencyProperty.RegisterAttached(
            "SelectAllOnFocus",
            typeof(bool),
            typeof(TextBoxHelper),
            new FrameworkPropertyMetadata(false, OnSelectAllOnFocusChanged));

    public static bool GetSelectAllOnFocus(DependencyObject obj) =>
        (bool)obj.GetValue(SelectAllOnFocusProperty);

    public static void SetSelectAllOnFocus(DependencyObject obj, bool value) =>
        obj.SetValue(SelectAllOnFocusProperty, value);

    private static void OnSelectAllOnFocusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not TextBox tb) return;

        if ((bool)e.NewValue)
        {
            tb.GotFocus += OnTextBoxGotFocus;
            tb.PreviewMouseLeftButtonDown += OnTextBoxPreviewMouseLeftButtonDown;
        }
        else
        {
            tb.GotFocus -= OnTextBoxGotFocus;
            tb.PreviewMouseLeftButtonDown -= OnTextBoxPreviewMouseLeftButtonDown;
        }
    }

    private static void OnTextBoxGotFocus(object sender, RoutedEventArgs e)
    {
        if (sender is TextBox tb)
        {
            tb.SelectAll();
        }
    }

    private static void OnTextBoxPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is TextBox tb && !tb.IsKeyboardFocusWithin)
        {
            tb.Focus();
            e.Handled = true;
        }
    }

    #endregion

    #region ClearOnEscape

    public static readonly DependencyProperty ClearOnEscapeProperty =
        DependencyProperty.RegisterAttached(
            "ClearOnEscape",
            typeof(bool),
            typeof(TextBoxHelper),
            new FrameworkPropertyMetadata(false, OnClearOnEscapeChanged));

    public static bool GetClearOnEscape(DependencyObject obj) =>
        (bool)obj.GetValue(ClearOnEscapeProperty);

    public static void SetClearOnEscape(DependencyObject obj, bool value) =>
        obj.SetValue(ClearOnEscapeProperty, value);

    private static void OnClearOnEscapeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not TextBox tb) return;

        if ((bool)e.NewValue)
            tb.PreviewKeyDown += OnClearOnEscapeKeyDown;
        else
            tb.PreviewKeyDown -= OnClearOnEscapeKeyDown;
    }

    private static void OnClearOnEscapeKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape && sender is TextBox tb)
        {
            tb.Clear();
            e.Handled = true;
        }
    }

    #endregion

    #region CommitOnEnter

    public static readonly DependencyProperty CommitOnEnterProperty =
        DependencyProperty.RegisterAttached(
            "CommitOnEnter",
            typeof(bool),
            typeof(TextBoxHelper),
            new FrameworkPropertyMetadata(false, OnCommitOnEnterChanged));

    public static bool GetCommitOnEnter(DependencyObject obj) =>
        (bool)obj.GetValue(CommitOnEnterProperty);

    public static void SetCommitOnEnter(DependencyObject obj, bool value) =>
        obj.SetValue(CommitOnEnterProperty, value);

    private static void OnCommitOnEnterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not TextBox tb) return;

        if ((bool)e.NewValue)
            tb.PreviewKeyDown += OnCommitOnEnterKeyDown;
        else
            tb.PreviewKeyDown -= OnCommitOnEnterKeyDown;
    }

    private static void OnCommitOnEnterKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && sender is TextBox tb)
        {
            tb.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
            Keyboard.ClearFocus();
            e.Handled = true;
        }
    }

    #endregion

    #region InputType Filtering

    public static readonly DependencyProperty InputTypeProperty =
        DependencyProperty.RegisterAttached(
            "InputType",
            typeof(TextBoxInputType),
            typeof(TextBoxHelper),
            new FrameworkPropertyMetadata(TextBoxInputType.None, OnInputTypeChanged));

    public static TextBoxInputType GetInputType(DependencyObject obj) =>
        (TextBoxInputType)obj.GetValue(InputTypeProperty);

    public static void SetInputType(DependencyObject obj, TextBoxInputType value) =>
        obj.SetValue(InputTypeProperty, value);

    private static void OnInputTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not TextBox tb) return;

        tb.PreviewTextInput -= OnTextBoxPreviewTextInput;
        DataObject.RemovePastingHandler(tb, OnTextBoxPasting);

        if ((TextBoxInputType)e.NewValue != TextBoxInputType.None)
        {
            tb.PreviewTextInput += OnTextBoxPreviewTextInput;
            DataObject.AddPastingHandler(tb, OnTextBoxPasting);
        }
    }

    private static void OnTextBoxPreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        if (sender is not TextBox tb) return;

        var inputType = GetInputType(tb);
        if (!IsTextAllowed(tb, e.Text, inputType))
        {
            e.Handled = true;
        }
    }

    private static void OnTextBoxPasting(object sender, DataObjectPastingEventArgs e)
    {
        if (sender is not TextBox tb) return;

        var inputType = GetInputType(tb);
        if (inputType == TextBoxInputType.None) return;

        if (e.DataObject.GetDataPresent(typeof(string)))
        {
            string pasteText = (string)e.DataObject.GetData(typeof(string));
            if (!IsTextAllowed(tb, pasteText, inputType))
            {
                e.CancelCommand();
            }
        }
        else
        {
            e.CancelCommand();
        }
    }

    public static bool IsTextAllowed(TextBox tb, string newText, TextBoxInputType inputType)
    {
        if (string.IsNullOrEmpty(newText)) return true;

        string currentText = tb.Text ?? string.Empty;
        int selStart = tb.SelectionStart;
        int selLength = tb.SelectionLength;

        // Ensure safe range for replacement
        if (selStart < 0) selStart = 0;
        if (selStart > currentText.Length) selStart = currentText.Length;
        if (selStart + selLength > currentText.Length) selLength = currentText.Length - selStart;

        string proposedText = currentText.Remove(selStart, selLength).Insert(selStart, newText);

        return inputType switch
        {
            TextBoxInputType.DigitsOnly => Regex.IsMatch(newText, @"^\d+$"),
            TextBoxInputType.PositiveInteger => Regex.IsMatch(proposedText, @"^\d+$"),
            TextBoxInputType.Integer => Regex.IsMatch(proposedText, @"^[+-]?\d*$"),
            TextBoxInputType.PositiveDecimal => Regex.IsMatch(proposedText, @"^\d*([.,]\d*)?$"),
            TextBoxInputType.Decimal => Regex.IsMatch(proposedText, @"^[+-]?\d*([.,]\d*)?$"),
            TextBoxInputType.Alphanumeric => Regex.IsMatch(newText, @"^[a-zA-Z0-9]+$"),
            TextBoxInputType.Formula => Regex.IsMatch(newText, @"^[a-zA-Z0-9\+\-\*\/\^\(\)\.\,\s_]+$"),
            _ => true
        };
    }

    #endregion
}
