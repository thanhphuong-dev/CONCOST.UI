using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace CONCOST.UI.Controls;

/// <summary>
/// The colours a <see cref="MarkdownParser"/> paints its decorations with: rules, quote bars, code
/// frames and inline code. A null brush means "use the theme default", which is what keeps a
/// document pane on the right theme when the shell flips between light and dark.
/// </summary>
public sealed class MarkdownRenderTheme
{
    /// <summary>The horizontal rule.</summary>
    public Brush? RuleBrush { get; set; }

    /// <summary>The bar down the left of a block quote.</summary>
    public Brush? QuoteBrush { get; set; }

    /// <summary>The frame around a fenced code block.</summary>
    public Brush? CodeBorderBrush { get; set; }

    /// <summary>The wash behind a fenced code block; null draws no wash.</summary>
    public Brush? CodeBackgroundBrush { get; set; }

    /// <summary>The wash behind inline code.</summary>
    public Brush? InlineCodeBackgroundBrush { get; set; }

    /// <summary>The colour of hyperlinks.</summary>
    public Brush? HyperlinkBrush { get; set; }

    /// <summary>Resolves every brush from the <c>CONCOST.Brush.*</c> tokens in force.</summary>
    public static MarkdownRenderTheme FromResources()
    {
        Brush Resolve(string key, Brush fallback) =>
            Application.Current?.TryFindResource(key) as Brush ?? fallback;

        return new MarkdownRenderTheme
        {
            RuleBrush = Resolve("CONCOST.Brush.Border", Brushes.Gray),
            QuoteBrush = Resolve("CONCOST.Brush.Border", Brushes.Gray),
            CodeBorderBrush = Resolve("CONCOST.Brush.Border", Brushes.Gray),
            CodeBackgroundBrush = Resolve("CONCOST.Brush.SurfaceSecondary", Brushes.Gainsboro),
            InlineCodeBackgroundBrush = Resolve("CONCOST.Brush.SurfaceSecondary", Brushes.Gainsboro),
            HyperlinkBrush = Resolve("CONCOST.Brush.Primary", Brushes.DodgerBlue)
        };
    }
}

/// <summary>
/// Renders a useful subset of Markdown into a <see cref="FlowDocument"/>: headings, paragraphs,
/// emphasis, inline code, fenced code, lists, quotes, rules and links.
/// </summary>
/// <remarks>
/// <para>
/// It is a block-by-block parser, not a CommonMark implementation. Every construct it does support
/// survives a round trip through the <see cref="MarkdownViewer"/>; everything else degrades to plain
/// text instead of throwing, which is what a documentation pane needs from a viewer that receives
/// text from a file the user just edited.
/// </para>
/// <para>
/// Emphasis is resolved with a single left-to-right scan, so <c>**bold**</c> inside a line is handled
/// without a regular expression and without backtracking — a line of asterisks stays linear.
/// </para>
/// </remarks>
public static class MarkdownParser
{
    /// <summary>Parses <paramref name="markdown"/> into a document ready for a viewer.</summary>
    public static FlowDocument Parse(string? markdown) => Parse(markdown, null);

    /// <summary>
    /// Parses with an explicit render theme; <c>null</c> reads the <c>CONCOST.Brush.*</c> tokens in
    /// force, so the document matches the theme that is showing without the caller knowing about it.
    /// </summary>
    public static FlowDocument Parse(string? markdown, MarkdownRenderTheme? theme)
    {
        MarkdownRenderTheme palette = theme ?? MarkdownRenderTheme.FromResources();

        var document = new FlowDocument { PagePadding = new Thickness(0) };
        var lines = (markdown ?? string.Empty).Replace("\r\n", "\n").Split('\n');

        int i = 0;

        while (i < lines.Length)
        {
            string line = lines[i];

            if (string.IsNullOrWhiteSpace(line))
            {
                i++;
                continue;
            }

            if (line.TrimStart().StartsWith("```", StringComparison.Ordinal))
            {
                var code = new StringBuilder();
                i++;

                while (i < lines.Length && !lines[i].TrimStart().StartsWith("```", StringComparison.Ordinal))
                {
                    code.AppendLine(lines[i]);
                    i++;
                }

                i++;
                document.Blocks.Add(CodeBlock(code.ToString(), palette));
                continue;
            }

            if (IsRule(line))
            {
                document.Blocks.Add(new Paragraph
                {
                    Margin = new Thickness(0, 6, 0, 6),
                    BorderBrush = palette.RuleBrush,
                    BorderThickness = new Thickness(0, 0, 0, 1)
                });

                i++;
                continue;
            }

            if (HeadingLevel(line) is int level)
            {
                document.Blocks.Add(Heading(line.TrimStart('#').Trim(), level, palette));
                i++;
                continue;
            }

            if (line.TrimStart().StartsWith(">", StringComparison.Ordinal))
            {
                var quote = new StringBuilder();

                while (i < lines.Length && lines[i].TrimStart().StartsWith(">", StringComparison.Ordinal))
                {
                    quote.AppendLine(lines[i].TrimStart().TrimStart('>').Trim());
                    i++;
                }

                var quoteParagraph = Para(quote.ToString().TrimEnd(), palette);
                quoteParagraph.Margin = new Thickness(12, 4, 0, 4);
                quoteParagraph.Padding = new Thickness(10, 6, 10, 6);
                quoteParagraph.BorderBrush = palette.QuoteBrush;
                quoteParagraph.BorderThickness = new Thickness(3, 0, 0, 0);
                quoteParagraph.FontStyle = FontStyles.Italic;

                document.Blocks.Add(quoteParagraph);

                continue;
            }

            if (IsOrderedItem(line, out _))
            {
                var list = new System.Windows.Documents.List { MarkerStyle = TextMarkerStyle.Decimal, Margin = new Thickness(16, 2, 0, 2) };

                while (i < lines.Length && IsOrderedItem(lines[i], out _))
                {
                    int dot = lines[i].IndexOf('.');
                    list.ListItems.Add(new ListItem(Para(lines[i].Substring(dot + 1).Trim(), palette)));
                    i++;
                }

                document.Blocks.Add(list);
                continue;
            }

            if (IsBulletItem(line))
            {
                var list = new System.Windows.Documents.List { MarkerStyle = TextMarkerStyle.Disc, Margin = new Thickness(16, 2, 0, 2) };

                while (i < lines.Length && IsBulletItem(lines[i]))
                {
                    list.ListItems.Add(new ListItem(Para(lines[i].TrimStart('-', '*', '+', ' '), palette)));
                    i++;
                }

                document.Blocks.Add(list);
                continue;
            }

            // Any other run of lines is one paragraph, so hard-wrapped prose reads as a paragraph.
            var paragraphText = new StringBuilder();

            while (i < lines.Length && !string.IsNullOrWhiteSpace(lines[i]) && !StartsBlock(lines[i]))
            {
                if (paragraphText.Length > 0) paragraphText.Append(' ');
                paragraphText.Append(lines[i].Trim());
                i++;
            }

            var body = Para(paragraphText.ToString(), palette);
            body.Margin = new Thickness(0, 2, 0, 6);
            document.Blocks.Add(body);
        }

        if (document.Blocks.Count == 0) document.Blocks.Add(new Paragraph(new Run(string.Empty)));

        return document;
    }

    private static bool StartsBlock(string line)
    {
        string trimmed = line.TrimStart();

        return trimmed.StartsWith("#", StringComparison.Ordinal) ||
               trimmed.StartsWith("```", StringComparison.Ordinal) ||
               trimmed.StartsWith(">", StringComparison.Ordinal) ||
               IsBulletItem(line) ||
               IsOrderedItem(line, out _) ||
               IsRule(line);
    }

    private static bool IsRule(string line)
    {
        string trimmed = line.Trim();
        if (trimmed.Length < 3) return false;

        char first = trimmed[0];
        if (first is not ('-' or '*' or '_')) return false;

        foreach (char c in trimmed)
        {
            if (c != first) return false;
        }

        return true;
    }

    private static int? HeadingLevel(string line)
    {
        int hashes = 0;
        while (hashes < line.Length && line[hashes] == '#') hashes++;

        if (hashes is < 1 or > 6) return null;
        if (hashes < line.Length && line[hashes] != ' ') return null;

        return hashes;
    }

    private static bool IsBulletItem(string line)
    {
        string trimmed = line.TrimStart();
        return trimmed.Length > 1 && (trimmed[0] is '-' or '*' or '+') && trimmed[1] == ' ';
    }

    private static bool IsOrderedItem(string line, out int number)
    {
        number = 0;
        string trimmed = line.TrimStart();

        int digits = 0;
        while (digits < trimmed.Length && char.IsDigit(trimmed[digits])) digits++;

        if (digits == 0 || digits + 1 >= trimmed.Length) return false;
        if (trimmed[digits] != '.' || trimmed[digits + 1] != ' ') return false;

        return int.TryParse(trimmed.Substring(0, digits), out number);
    }

    private static Paragraph Heading(string text, int level, MarkdownRenderTheme palette)
    {
        var paragraph = Para(text, palette);
        paragraph.Margin = new Thickness(0, level == 1 ? 10 : 8, 0, 4);
        paragraph.FontWeight = FontWeights.SemiBold;

        if (paragraph.Inlines.FirstInline is Run run) run.FontSize = level switch { 1 => 20, 2 => 17, 3 => 15, _ => 13 };

        return paragraph;
    }

    private static Paragraph CodeBlock(string code, MarkdownRenderTheme palette)
    {
        var paragraph = new Paragraph(new Run(code.TrimEnd()))
        {
            FontFamily = new FontFamily("Consolas, Courier New"),
            FontSize = 12,
            Margin = new Thickness(0, 4, 0, 8),
            Padding = new Thickness(10, 8, 10, 8),
            BorderThickness = new Thickness(1),
            BorderBrush = palette.CodeBorderBrush,
            Background = palette.CodeBackgroundBrush
        };

        return paragraph;
    }

    private static Paragraph Para(string text) => Para(text, null);

    private static Paragraph Para(string text, MarkdownRenderTheme? palette)
    {
        var paragraph = new Paragraph();

        foreach (var inline in ParseInlines(text, palette)) paragraph.Inlines.Add(inline);

        return paragraph;
    }

    /// <summary>Turns one line into inlines, honouring code, bold, italic and links. Reads the tokens in force.</summary>
    public static IEnumerable<Inline> ParseInlines(string? text) => ParseInlines(text, MarkdownRenderTheme.FromResources());

    /// <summary>
    /// Parses inlines with the decorations a <see cref="MarkdownRenderTheme"/> carries: links dressed
    /// in its accent instead of the system link blue, inline code on its wash. Null keeps the plain
    /// system look.
    /// </summary>
    public static IEnumerable<Inline> ParseInlines(string? text, MarkdownRenderTheme? palette)
    {
        var inlines = new List<Inline>();
        if (text is null || text.Length == 0) return inlines;

        var plain = new StringBuilder();
        int i = 0;

        void Flush()
        {
            if (plain.Length == 0) return;
            inlines.Add(new Run(plain.ToString()));
            plain.Clear();
        }

        while (i < text.Length)
        {
            char c = text[i];

            if (c == '`')
            {
                int end = text.IndexOf('`', i + 1);
                if (end > i + 1)
                {
                    Flush();
                    inlines.Add(new Run(text.Substring(i + 1, end - i - 1))
                    {
                        FontFamily = new FontFamily("Consolas, Courier New"),
                        Background = palette?.InlineCodeBackgroundBrush
                    });

                    i = end + 1;
                    continue;
                }
            }

            if (c == '*' && i + 1 < text.Length && text[i + 1] == '*')
            {
                int end = text.IndexOf("**", i + 2, StringComparison.Ordinal);
                if (end > i + 1)
                {
                    Flush();
                    inlines.Add(new Bold(new Run(text.Substring(i + 2, end - i - 2))));
                    i = end + 2;
                    continue;
                }
            }

            if (c == '*')
            {
                int end = text.IndexOf('*', i + 1);
                if (end > i + 1)
                {
                    Flush();
                    inlines.Add(new Italic(new Run(text.Substring(i + 1, end - i - 1))));
                    i = end + 1;
                    continue;
                }
            }

            if (c == '[')
            {
                int close = text.IndexOf(']', i + 1);
                int open = close > 0 && close + 1 < text.Length && text[close + 1] == '('
                    ? text.IndexOf(')', close + 2)
                    : -1;

                if (open > close && close > i)
                {
                    Flush();
                    string caption = text.Substring(i + 1, close - i - 1);
                    string url = text.Substring(close + 2, open - close - 2);

                    var link = new Hyperlink(new Run(caption));

                    // The system link blue is invisible on some themes; the accent is the one colour
                    // every theme promises to be readable against its own surfaces.
                    if (palette?.HyperlinkBrush is { } linkAccent) link.Foreground = linkAccent;

                    if (Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out var uri)) link.NavigateUri = uri;

                    inlines.Add(link);
                    i = open + 1;
                    continue;
                }
            }

            plain.Append(c);
            i++;
        }

        Flush();
        return inlines;
    }
}

/// <summary>
/// Shows Markdown from a help file, a release note or a README without pulling in a markdown engine.
/// </summary>
/// <remarks>
/// The document is rebuilt when <see cref="Markdown"/> changes and cached in <see cref="Document"/>,
/// so a template can bind the document and a host can print or export it without asking the viewer
/// to re-parse.
/// </remarks>
public class MarkdownViewer : Control
{
    /// <summary>Identifies the <see cref="Markdown"/> property.</summary>
    public static readonly DependencyProperty MarkdownProperty = DependencyProperty.Register(
        nameof(Markdown), typeof(string), typeof(MarkdownViewer),
        new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.AffectsRender, OnMarkdownChanged));

    /// <summary>Identifies the <see cref="RenderTheme"/> property.</summary>
    public static readonly DependencyProperty RenderThemeProperty = DependencyProperty.Register(
        nameof(RenderTheme), typeof(MarkdownRenderTheme), typeof(MarkdownViewer),
        new FrameworkPropertyMetadata(null, OnRenderThemeChanged));

    private static void OnRenderThemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is MarkdownViewer viewer) viewer.Rebuild();
    }

    private static readonly DependencyPropertyKey DocumentPropertyKey = DependencyProperty.RegisterReadOnly(
        nameof(Document), typeof(FlowDocument), typeof(MarkdownViewer), new PropertyMetadata(null));

    /// <summary>Identifies the <see cref="Document"/> property.</summary>
    public static readonly DependencyProperty DocumentProperty = DocumentPropertyKey.DependencyProperty;

    private static readonly DependencyPropertyKey LinkCountPropertyKey = DependencyProperty.RegisterReadOnly(
        nameof(LinkCount), typeof(int), typeof(MarkdownViewer), new PropertyMetadata(0));

    /// <summary>Identifies the <see cref="LinkCount"/> property.</summary>
    public static readonly DependencyProperty LinkCountProperty = LinkCountPropertyKey.DependencyProperty;

    static MarkdownViewer()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(MarkdownViewer), new FrameworkPropertyMetadata(typeof(MarkdownViewer)));
    }

    /// <summary>Source text.</summary>
    public string Markdown
    {
        get => (string)GetValue(MarkdownProperty);
        set => SetValue(MarkdownProperty, value);
    }

    /// <summary>
    /// The colours the decorations — rules, quotes, code frames, inline code, links — are drawn
    /// with. Null reads the <c>CONCOST.Brush.*</c> tokens in force, which is what follows the theme
    /// swap; a host that tints one panel differently sets its own.
    /// </summary>
    public MarkdownRenderTheme? RenderTheme
    {
        get => (MarkdownRenderTheme?)GetValue(RenderThemeProperty);
        set => SetValue(RenderThemeProperty, value);
    }

    /// <summary>The parsed document, ready to be shown or printed.</summary>
    public FlowDocument? Document => (FlowDocument?)GetValue(DocumentProperty);

    /// <summary>Number of hyperlinks in the current document.</summary>
    public int LinkCount => (int)GetValue(LinkCountProperty);

    private static void OnMarkdownChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is MarkdownViewer viewer) viewer.Rebuild();
    }

    /// <summary>Re-parses <see cref="Markdown"/>; call it after mutating the text in place.</summary>
    public void Rebuild()
    {
        var document = MarkdownParser.Parse(Markdown, RenderTheme);
        SetValue(DocumentPropertyKey, document);

        int links = 0;
        foreach (var block in document.Blocks) links += CountLinks(block);

        SetValue(LinkCountPropertyKey, links);
    }

    private static int CountLinks(Block block)
    {
        switch (block)
        {
            case Paragraph paragraph:
            {
                int count = 0;
                foreach (var inline in paragraph.Inlines) count += CountLinks(inline);
                return count;
            }

            case System.Windows.Documents.List list:
            {
                int count = 0;

                foreach (var item in list.ListItems)
                {
                    foreach (var inner in item.Blocks) count += CountLinks(inner);
                }

                return count;
            }

            case Section section:
            {
                int count = 0;
                foreach (var inner in section.Blocks) count += CountLinks(inner);
                return count;
            }

            default:
                return 0;
        }
    }

    private static int CountLinks(Inline inline)
    {
        int count = inline is Hyperlink ? 1 : 0;

        if (inline is Span span)
        {
            foreach (var child in span.Inlines) count += CountLinks(child);
        }

        return count;
    }
}
