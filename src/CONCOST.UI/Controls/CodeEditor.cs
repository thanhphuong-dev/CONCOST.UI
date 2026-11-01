using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace CONCOST.UI.Controls;

/// <summary>Language the <see cref="CodeHighlighter"/> knows enough about to colour.</summary>
public enum CodeLanguage
{
    /// <summary>No colouring.</summary>
    Plain,

    /// <summary>C# including comments, strings, numbers and keywords.</summary>
    CSharp,

    /// <summary>XAML: element names, attribute names and quoted values.</summary>
    Xaml,

    /// <summary>SQL with case-insensitive keywords and <c>--</c> comments.</summary>
    Sql,

    /// <summary>JSON strings, numbers and literals.</summary>
    Json
}

/// <summary>What a <see cref="CodeToken"/> is, so a host can theme it.</summary>
public enum CodeTokenKind
{
    Keyword,
    String,
    Comment,
    Number,
    Tag,
    Attribute
}

/// <summary>A run of characters with one meaning. Characters between tokens are plain text.</summary>
public readonly struct CodeToken
{
    public CodeToken(int start, int length, CodeTokenKind kind)
    {
        Start = start;
        Length = length;
        Kind = kind;
    }

    /// <summary>Index of the first character.</summary>
    public int Start { get; }

    /// <summary>Number of characters.</summary>
    public int Length { get; }

    /// <summary>What the run is.</summary>
    public CodeTokenKind Kind { get; }
}

/// <summary>
/// A small tokenizer for the languages a desktop tool actually shows in a help pane or a script
/// editor. It is deliberately a scanner rather than a parser: it never throws, always covers the
/// budget in linear time, and returns the runs a renderer needs.
/// </summary>
public static class CodeHighlighter
{
    private static readonly HashSet<string> CSharpKeywords = new(StringComparer.Ordinal)
    {
        "abstract", "as", "async", "await", "base", "bool", "break", "byte", "case", "catch", "char",
        "checked", "class", "const", "continue", "decimal", "default", "delegate", "do", "double",
        "else", "enum", "event", "explicit", "extern", "false", "finally", "fixed", "float", "for",
        "foreach", "get", "goto", "if", "implicit", "in", "int", "interface", "internal", "is", "lock",
        "long", "namespace", "new", "null", "object", "operator", "out", "override", "params",
        "private", "protected", "public", "readonly", "record", "ref", "return", "sealed", "set",
        "short", "sizeof", "stackalloc", "static", "string", "struct", "switch", "this", "throw",
        "true", "try", "typeof", "uint", "ulong", "unchecked", "unsafe", "ushort", "using", "var",
        "virtual", "void", "volatile", "when", "where", "while", "yield"
    };

    private static readonly HashSet<string> SqlKeywords = new(StringComparer.OrdinalIgnoreCase)
    {
        "add", "all", "alter", "and", "as", "asc", "begin", "between", "by", "case", "cast", "column",
        "commit", "constraint", "create", "default", "delete", "desc", "distinct", "drop", "else",
        "end", "exec", "exists", "from", "full", "group", "having", "if", "in", "index", "inner",
        "insert", "into", "is", "join", "left", "like", "limit", "not", "null", "on", "or", "order",
        "outer", "primary", "procedure", "right", "rollback", "select", "set", "table", "then", "top",
        "transaction", "union", "unique", "update", "values", "view", "when", "where", "with"
    };

    private static readonly HashSet<string> JsonKeywords = new(StringComparer.Ordinal)
    {
        "true", "false", "null"
    };

    /// <summary>Returns the coloured runs of <paramref name="text"/>; gaps are plain text.</summary>
    public static IReadOnlyList<CodeToken> Tokenize(string? text, CodeLanguage language)
    {
        var tokens = new List<CodeToken>();
        if (text is null || text.Length == 0 || language == CodeLanguage.Plain) return tokens;

        int i = 0;

        while (i < text.Length)
        {
            char c = text[i];

            switch (language)
            {
                case CodeLanguage.Xaml:
                    i = ScanXaml(text, i, tokens);
                    continue;
                case CodeLanguage.Sql:
                    i = ScanSql(text, i, tokens);
                    continue;
                case CodeLanguage.Json:
                    i = ScanJson(text, i, tokens);
                    continue;
                default:
                    i = ScanCSharp(text, i, tokens);
                    continue;
            }
        }

        return tokens;
    }

    private static bool IsWordStart(char c) => char.IsLetter(c) || c == '_';

    private static bool IsWordPart(char c) => char.IsLetterOrDigit(c) || c == '_';

    private static int ScanNumber(string text, int start, List<CodeToken> tokens)
    {
        int i = start;

        while (i < text.Length && (char.IsDigit(text[i]) || text[i] == '.' || text[i] == '_' ||
                                   (text[i] is 'x' or 'X' && i == start + 1) ||
                                   (i > start && (text[i] is 'e' or 'E') && i + 1 < text.Length && (char.IsDigit(text[i + 1]) || text[i + 1] is '+' or '-'))))
        {
            i++;
        }

        tokens.Add(new CodeToken(start, i - start, CodeTokenKind.Number));
        return i;
    }

    private static int ScanWord(string text, int start, HashSet<string> keywords, List<CodeToken> tokens)
    {
        int i = start + 1;
        while (i < text.Length && IsWordPart(text[i])) i++;

        if (keywords.Contains(text.Substring(start, i - start)))
            tokens.Add(new CodeToken(start, i - start, CodeTokenKind.Keyword));

        return i;
    }

    private static int ScanQuoted(string text, int start, char quote, List<CodeToken> tokens)
    {
        int i = start + 1;

        while (i < text.Length)
        {
            if (text[i] == '\\' && i + 1 < text.Length)
            {
                i += 2;
                continue;
            }

            if (text[i] == quote)
            {
                i++;
                break;
            }

            // An unterminated literal stops at the line end, so one stray quote cannot paint the
            // rest of the file as a string.
            if (text[i] is '\r' or '\n') break;

            i++;
        }

        tokens.Add(new CodeToken(start, i - start, CodeTokenKind.String));
        return i;
    }

    private static int ScanLineComment(string text, int start, List<CodeToken> tokens)
    {
        int i = start;
        while (i < text.Length && text[i] is not '\r' and not '\n') i++;

        tokens.Add(new CodeToken(start, i - start, CodeTokenKind.Comment));
        return i;
    }

    private static int ScanCSharp(string text, int start, List<CodeToken> tokens)
    {
        char c = text[start];

        if (c == '/' && start + 1 < text.Length && text[start + 1] == '/') return ScanLineComment(text, start, tokens);

        if (c == '/' && start + 1 < text.Length && text[start + 1] == '*')
        {
            int end = text.IndexOf("*/", start + 2, StringComparison.Ordinal);
            end = end < 0 ? text.Length : end + 2;

            tokens.Add(new CodeToken(start, end - start, CodeTokenKind.Comment));
            return end;
        }

        if (c == '"')
        {
            return ScanQuoted(text, start, '"', tokens);
        }

        if (c == '\'') return ScanQuoted(text, start, '\'', tokens);

        if (char.IsDigit(c)) return ScanNumber(text, start, tokens);
        if (IsWordStart(c)) return ScanWord(text, start, CSharpKeywords, tokens);

        return start + 1;
    }

    private static int ScanSql(string text, int start, List<CodeToken> tokens)
    {
        char c = text[start];

        if (c == '-' && start + 1 < text.Length && text[start + 1] == '-') return ScanLineComment(text, start, tokens);
        if (c == '\'') return ScanQuoted(text, start, '\'', tokens);
        if (char.IsDigit(c)) return ScanNumber(text, start, tokens);
        if (IsWordStart(c)) return ScanWord(text, start, SqlKeywords, tokens);

        return start + 1;
    }

    private static int ScanJson(string text, int start, List<CodeToken> tokens)
    {
        char c = text[start];

        if (c == '"') return ScanQuoted(text, start, '"', tokens);
        if (char.IsDigit(c) || (c == '-' && start + 1 < text.Length && char.IsDigit(text[start + 1]))) return ScanNumber(text, start, tokens);
        if (IsWordStart(c)) return ScanWord(text, start, JsonKeywords, tokens);

        return start + 1;
    }

    private static int ScanXaml(string text, int start, List<CodeToken> tokens)
    {
        char c = text[start];

        if (c == '<' && start + 3 < text.Length && text.Substring(start, 4) == "<!--")
        {
            int end = text.IndexOf("-->", start + 4, StringComparison.Ordinal);
            end = end < 0 ? text.Length : end + 3;

            tokens.Add(new CodeToken(start, end - start, CodeTokenKind.Comment));
            return end;
        }

        if (c == '"' || c == '\'') return ScanQuoted(text, start, c, tokens);

        if (c == '<')
        {
            int i = start + 1;
            if (i < text.Length && text[i] == '/') i++;

            int nameStart = i;
            while (i < text.Length && (char.IsLetterOrDigit(text[i]) || text[i] is '_' or ':' or '.' or '-')) i++;

            if (i > nameStart) tokens.Add(new CodeToken(nameStart, i - nameStart, CodeTokenKind.Tag));

            return Math.Max(i, start + 1);
        }

        if (char.IsLetter(c) || c == '_')
        {
            int i = start + 1;
            while (i < text.Length && (char.IsLetterOrDigit(text[i]) || text[i] is '_' or ':' or '.' or '-')) i++;

            int probe = i;
            while (probe < text.Length && char.IsWhiteSpace(text[probe])) probe++;

            if (probe < text.Length && text[probe] == '=')
                tokens.Add(new CodeToken(start, i - start, CodeTokenKind.Attribute));

            return i;
        }

        return start + 1;
    }
}

/// <summary>
/// A code box with line numbers, tab handling and optional syntax colouring.
/// </summary>
/// <remarks>
/// <para>
/// The editor is a <see cref="RichTextBox"/>, because colouring a plain text box is not possible.
/// Rebuilding the document is what costs something: it clears the undo stack. Set
/// <see cref="HighlightSyntax"/> to <c>false</c> for a long-lived editing session where undo matters
/// more than colour, and leave it on for script panes and snippets.
/// </para>
/// <para>
/// Colouring is scheduled, not immediate: a keystroke updates <see cref="Text"/> straight away and
/// asks for a repaint 180 ms later, so typing a fast identifier re-tokenizes once instead of eight
/// times. The caret is re-applied by text offset after the rebuild.
/// </para>
/// </remarks>
[TemplatePart(Name = EditorPart, Type = typeof(RichTextBox))]
[TemplatePart(Name = LinesPart, Type = typeof(ItemsControl))]
public class CodeEditor : Control
{
    private const string EditorPart = "PART_Editor";
    private const string LinesPart = "PART_Lines";

    /// <summary>Identifies the <see cref="Text"/> property.</summary>
    public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
        nameof(Text), typeof(string), typeof(CodeEditor),
        new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnTextChanged));

    /// <summary>Identifies the <see cref="Language"/> property.</summary>
    public static readonly DependencyProperty SyntaxProperty = DependencyProperty.Register(
        nameof(Syntax), typeof(CodeLanguage), typeof(CodeEditor),
        new PropertyMetadata(CodeLanguage.CSharp, OnColorAffectingChanged));

    /// <summary>Identifies the <see cref="IsReadOnly"/> property.</summary>
    public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register(
        nameof(IsReadOnly), typeof(bool), typeof(CodeEditor), new PropertyMetadata(false));

    /// <summary>Identifies the <see cref="HighlightSyntax"/> property.</summary>
    public static readonly DependencyProperty HighlightSyntaxProperty = DependencyProperty.Register(
        nameof(HighlightSyntax), typeof(bool), typeof(CodeEditor),
        new PropertyMetadata(true, OnColorAffectingChanged));

    /// <summary>Identifies the <see cref="ShowLineNumbers"/> property.</summary>
    public static readonly DependencyProperty ShowLineNumbersProperty = DependencyProperty.Register(
        nameof(ShowLineNumbers), typeof(bool), typeof(CodeEditor), new PropertyMetadata(true));

    /// <summary>Identifies the <see cref="TabSize"/> property.</summary>
    public static readonly DependencyProperty TabSizeProperty = DependencyProperty.Register(
        nameof(TabSize), typeof(int), typeof(CodeEditor), new PropertyMetadata(4, null, CoerceTabSize));

    private static readonly DependencyPropertyKey LineCountPropertyKey = DependencyProperty.RegisterReadOnly(
        nameof(LineCount), typeof(int), typeof(CodeEditor), new PropertyMetadata(1));

    /// <summary>Identifies the <see cref="LineCount"/> property.</summary>
    public static readonly DependencyProperty LineCountProperty = LineCountPropertyKey.DependencyProperty;

    /// <summary>Identifies the <see cref="CaretLine"/> property.</summary>
    public static readonly DependencyProperty CaretLineProperty = DependencyProperty.Register(
        nameof(CaretLine), typeof(int), typeof(CodeEditor), new PropertyMetadata(1));

    private RichTextBox? _editor;
    private ItemsControl? _lines;
    private DispatcherTimer? _highlightTimer;
    private bool _syncing;

    static CodeEditor()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(CodeEditor), new FrameworkPropertyMetadata(typeof(CodeEditor)));
    }

    /// <summary>The code, kept in sync with the editor both ways.</summary>
    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    /// <summary>Which tokenizer to run.</summary>
    public CodeLanguage Syntax
    {
        get => (CodeLanguage)GetValue(SyntaxProperty);
        set => SetValue(SyntaxProperty, value);
    }

    /// <summary>Blocks editing but keeps selection and copy.</summary>
    public bool IsReadOnly
    {
        get => (bool)GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    /// <summary>Colour keywords, strings and comments. Off means plain text and intact undo.</summary>
    public bool HighlightSyntax
    {
        get => (bool)GetValue(HighlightSyntaxProperty);
        set => SetValue(HighlightSyntaxProperty, value);
    }

    /// <summary>Shows the line-number gutter.</summary>
    public bool ShowLineNumbers
    {
        get => (bool)GetValue(ShowLineNumbersProperty);
        set => SetValue(ShowLineNumbersProperty, value);
    }

    /// <summary>Spaces a tab inserts.</summary>
    public int TabSize
    {
        get => (int)GetValue(TabSizeProperty);
        set => SetValue(TabSizeProperty, value);
    }

    /// <summary>Number of logical lines, kept current as the text changes.</summary>
    public int LineCount => (int)GetValue(LineCountProperty);

    /// <summary>One-based line the caret is on.</summary>
    public int CaretLine
    {
        get => (int)GetValue(CaretLineProperty);
        private set => SetValue(CaretLineProperty, value);
    }

    private static object CoerceTabSize(DependencyObject d, object baseValue)
    {
        int value = (int)baseValue;
        return value < 1 ? 1 : value > 12 ? 12 : value;
    }

    private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not CodeEditor editor || editor._syncing) return;

        string text = e.NewValue as string ?? string.Empty;

        // The line count is part of the contract even before a template is applied, so it is updated
        // here rather than only inside LoadText.
        editor.UpdateLineCount(text);
        editor.LoadText(text, true);
    }

    private static void OnColorAffectingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is CodeEditor editor) editor.ScheduleHighlight();
    }

    public override void OnApplyTemplate()
    {
        if (_editor is not null)
        {
            _editor.TextChanged -= OnEditorTextChanged;
            _editor.PreviewKeyDown -= OnEditorPreviewKeyDown;
            _editor.SelectionChanged -= OnEditorSelectionChanged;
        }

        base.OnApplyTemplate();

        _editor = GetTemplateChild(EditorPart) as RichTextBox;
        _lines = GetTemplateChild(LinesPart) as ItemsControl;

        if (_editor is null) return;

        _editor.AcceptsTab = true;
        _editor.IsReadOnly = IsReadOnly;
        _editor.TextChanged += OnEditorTextChanged;
        _editor.PreviewKeyDown += OnEditorPreviewKeyDown;
        _editor.SelectionChanged += OnEditorSelectionChanged;

        LoadText(Text, false);
    }

    protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.Property == IsReadOnlyProperty && _editor is not null) _editor.IsReadOnly = IsReadOnly;
    }

    /// <summary>Puts the caret at the start of a one-based line and scrolls it into view.</summary>
    public bool GoToLine(int line)
    {
        if (_editor is null) return false;

        line = Math.Max(1, Math.Min(LineCount, line));

        // Walk the text rather than the document: offsets in the document count paragraph symbols
        // too, so line * length is only reliable when taken from the same string.
        int offset = 0;
        int current = 1;

        foreach (string piece in SplitLines(Text))
        {
            if (current == line) break;
            offset += piece.Length + Environment.NewLine.Length;
            current++;
        }

        if (_editor.Document.ContentStart.GetPositionAtOffset(offset) is not TextPointer position) return false;

        _editor.CaretPosition = position;
        _editor.Focus();
        return true;
    }

    /// <summary>
    /// Selects the next occurrence of <paramref name="term"/> and returns <c>true</c> when one was
    /// found. The search wraps once, from the caret to the end and then from the start.
    /// </summary>
    public bool FindNext(string? term, bool matchCase = false)
    {
        // test the term directly rather than through string.IsNullOrEmpty: the .NET Framework
        // reference assemblies carry no nullable annotations, so the call does not narrow it.
        if (_editor is null || term is null || term.Length == 0) return false;

        int caret = new TextRange(_editor.Document.ContentStart, _editor.Selection.End).Text.Length;

        StringComparison comparison = matchCase ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
        int at = Text.IndexOf(term, Math.Min(caret, Text.Length), comparison);
        if (at < 0) at = Text.IndexOf(term, 0, comparison);
        if (at < 0) return false;

        if (_editor.Document.ContentStart.GetPositionAtOffset(at) is not TextPointer start) return false;
        if (start.GetPositionAtOffset(term.Length) is not TextPointer end) return false;

        _editor.Selection.Select(start, end);
        _editor.Focus();
        return true;
    }

    /// <summary>Applies the current text to the editor and refreshes the gutter.</summary>
    public void Refresh()
    {
        LoadText(Text, false);
    }

    private static IEnumerable<string> SplitLines(string? text)
    {
        if (text is null || text.Length == 0) return new[] { string.Empty };
        return text.Replace("\r\n", "\n").Split('\n');
    }

    private void LoadText(string text, bool fromHost)
    {
        if (_editor is null) return;

        if (fromHost)
        {
            string current = ReadText();
            if (current == text) return;
        }

        _syncing = true;

        try
        {
            _editor.Document = BuildDocument(text);
        }
        finally
        {
            _syncing = false;
        }

        UpdateLineCount(text);
    }

    private void OnEditorTextChanged(object sender, TextChangedEventArgs e)
    {
        if (_syncing) return;

        string text = ReadText();

        _syncing = true;
        SetCurrentValue(TextProperty, text);
        _syncing = false;

        UpdateLineCount(text);
        ScheduleHighlight();
    }

    private void OnEditorSelectionChanged(object sender, RoutedEventArgs e)
    {
        if (_editor is null) return;

        int caret = new TextRange(_editor.Document.ContentStart, _editor.CaretPosition).Text.Length;
        int line = Text.Take(Math.Max(0, Math.Min(caret, Text.Length))).Count(c => c == '\n') + 1;

        if (line < 1) line = 1;
        if (CaretLine != line) CaretLine = line;
    }

    private void OnEditorPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (_editor is null || IsReadOnly) return;

        if (e.Key == Key.Tab)
        {
            _editor.Selection.Text = new string(' ', TabSize);
            _editor.CaretPosition = _editor.Selection.End;
            e.Handled = true;
            return;
        }

        if (e.Key == Key.Enter)
        {
            // Keep the indentation of the line being split, which is what every code box does.
            string before = new TextRange(_editor.Document.ContentStart, _editor.CaretPosition).Text;
            int lineStart = before.LastIndexOf('\n') + 1;

            var indent = new StringBuilder();
            for (int i = lineStart; i < before.Length && (before[i] == ' ' || before[i] == '\t'); i++)
                indent.Append(before[i]);

            _editor.Selection.Text = Environment.NewLine + indent;
            _editor.CaretPosition = _editor.Selection.End;
            e.Handled = true;
        }
    }

    private void ScheduleHighlight()
    {
        if (_editor is null || !HighlightSyntax) return;

        _highlightTimer ??= new DispatcherTimer(DispatcherPriority.Background)
        {
            Interval = TimeSpan.FromMilliseconds(180)
        };

        _highlightTimer.Tick -= OnHighlightTick;
        _highlightTimer.Tick += OnHighlightTick;
        _highlightTimer.Stop();
        _highlightTimer.Start();
    }

    private void OnHighlightTick(object? sender, EventArgs e)
    {
        _highlightTimer?.Stop();
        if (_editor is null) return;

        int caret = new TextRange(_editor.Document.ContentStart, _editor.CaretPosition).Text.Length;

        _syncing = true;
        _editor.Document = BuildDocument(Text);
        _syncing = false;

        if (_editor.Document.ContentStart.GetPositionAtOffset(Math.Min(caret, Text.Length)) is TextPointer position)
        {
            _editor.CaretPosition = position;
        }
    }

    private string ReadText()
    {
        if (_editor is null) return Text;

        var range = new TextRange(_editor.Document.ContentStart, _editor.Document.ContentEnd);
        return range.Text.TrimEnd('\r', '\n');
    }

    private void UpdateLineCount(string text)
    {
        int lines = Math.Max(1, SplitLines(text).Count());
        SetValue(LineCountPropertyKey, lines);

        if (_lines is null || !ShowLineNumbers) return;

        _lines.ItemsSource = Enumerable.Range(1, lines).ToArray();
    }

    private FlowDocument BuildDocument(string text)
    {
        var document = new FlowDocument
        {
            PagePadding = new Thickness(0),
            FontFamily = FontFamily,
            FontSize = FontSize,
            Background = Brushes.Transparent
        };

        var paragraph = new Paragraph { Margin = new Thickness(0), LineHeight = FontSize * 1.5 };

        var tokens = HighlightSyntax ? CodeHighlighter.Tokenize(text, Syntax) : Array.Empty<CodeToken>();
        int cursor = 0;

        foreach (var token in tokens.OrderBy(t => t.Start))
        {
            if (token.Start < cursor || token.Start + token.Length > text.Length) continue;

            if (token.Start > cursor) paragraph.Inlines.Add(new Run(text.Substring(cursor, token.Start - cursor)));

            var run = new Run(text.Substring(token.Start, token.Length)) { Foreground = BrushFor(token.Kind) };
            paragraph.Inlines.Add(run);

            cursor = token.Start + token.Length;
        }

        if (cursor < text.Length) paragraph.Inlines.Add(new Run(text.Substring(cursor)));

        document.Blocks.Add(paragraph);
        return document;
    }

    private Brush BrushFor(CodeTokenKind kind)
    {
        string key = kind switch
        {
            CodeTokenKind.Keyword => "CONCOST.Code.Keyword",
            CodeTokenKind.String => "CONCOST.Code.String",
            CodeTokenKind.Comment => "CONCOST.Code.Comment",
            CodeTokenKind.Number => "CONCOST.Code.Number",
            CodeTokenKind.Tag => "CONCOST.Code.Tag",
            _ => "CONCOST.Code.Attribute"
        };

        if (TryFindResource(key) is Brush brush) return brush;

        return kind switch
        {
            CodeTokenKind.Keyword => new SolidColorBrush(Color.FromRgb(0x56, 0x9C, 0xD6)),
            CodeTokenKind.String => new SolidColorBrush(Color.FromRgb(0xCE, 0x91, 0x78)),
            CodeTokenKind.Comment => new SolidColorBrush(Color.FromRgb(0x6A, 0x99, 0x55)),
            CodeTokenKind.Number => new SolidColorBrush(Color.FromRgb(0xB5, 0xCE, 0xA8)),
            CodeTokenKind.Tag => new SolidColorBrush(Color.FromRgb(0x56, 0x9C, 0xD6)),
            _ => new SolidColorBrush(Color.FromRgb(0x9C, 0xDC, 0xFE))
        };
    }
}
