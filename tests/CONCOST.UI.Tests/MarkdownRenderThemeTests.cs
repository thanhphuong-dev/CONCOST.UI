using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using CONCOST.UI.Controls;
using Xunit;

namespace CONCOST.UI.Tests;

/// <summary>
/// The 1.2.0 <see cref="MarkdownViewer"/> pass: every decoration — rules, quote bars, code frames,
/// inline code washes, links — reads the <c>CONCOST.Brush.*</c> tokens instead of the four fixed
/// greys it drew before, which on a dark shell meant grey lines over dark surfaces. A host can
/// override the whole palette through <see cref="MarkdownViewer.RenderTheme"/>.
/// </summary>
public class MarkdownRenderThemeTests
{
    [Fact]
    public void Parse_without_a_theme_resolves_the_tokens_in_force()
    {
        var document = MarkdownParser.Parse("# Tiêu đề\n\n> Trích dẫn\n\n```\ncode\n```\n\n---\n\n`ghép`");

        // No throw, and every block landed — the palette path is the default path now.
        Assert.True(document.Blocks.Count >= 5);
    }

    [Fact]
    public void Decorations_take_their_brushes_from_the_theme()
    {
        var accent = new SolidColorBrush(Colors.Crimson);
        var wash = new SolidColorBrush(Colors.LemonChiffon);

        var theme = new MarkdownRenderTheme
        {
            QuoteBrush = accent,
            CodeBorderBrush = accent,
            CodeBackgroundBrush = wash,
            InlineCodeBackgroundBrush = wash,
            HyperlinkBrush = accent
        };

        var document = MarkdownParser.Parse("> lời trích\n\n```\ncode\n```\n\n`ghép`", theme);

        var quote = document.Blocks.OfType<Paragraph>().First(p => p.BorderThickness.Left == 3);
        Assert.Same(accent, quote.BorderBrush);

        // Picked by its text: two decorations share the accent, so thickness is not a reliable key.
        var code = document.Blocks.OfType<Paragraph>()
            .First(p => p.Inlines.OfType<Run>().Any(r => r.Text.TrimStart().StartsWith("code")));
        Assert.Same(accent, code.BorderBrush);
        Assert.Same(wash, code.Background);

        // The code block's own run is Consolas too, so the inline run is picked by its text —
        // the wash lives on the run only for inline code.
        var inline = document.Blocks
            .OfType<Paragraph>()
            .SelectMany(p => p.Inlines.OfType<Run>())
            .First(r => r.Text == "ghép");
        Assert.Same(wash, inline.Background);
    }

    [Fact]
    public void Links_wear_the_theme_accent_instead_of_the_system_blue()
    {
        var accent = new SolidColorBrush(Colors.MediumSeaGreen);
        var theme = new MarkdownRenderTheme { HyperlinkBrush = accent };

        var document = MarkdownParser.Parse("Xem [hướng dẫn](https://example.com) đầy đủ.", theme);

        var link = document.Blocks
            .OfType<Paragraph>()
            .SelectMany(p => p.Inlines.OfType<Hyperlink>())
            .Single();

        Assert.Same(accent, link.Foreground);

        // Without an explicit theme the parser resolves the tokens in force — in a bare test host
        // that is the neutral fallback, never the accent the test handed in.
        var plain = MarkdownParser.Parse("[trần](https://example.com)")
            .Blocks.OfType<Paragraph>()
            .SelectMany(p => p.Inlines.OfType<Hyperlink>())
            .Single();

        Assert.NotSame(accent, plain.Foreground);
    }

    [Fact]
    public void Viewer_rebuilds_when_the_render_theme_changes()
    {
        WpfApplicationHost.Run(() =>
        {
            var viewer = new MarkdownViewer { Markdown = "---" };
            var before = viewer.Document;

            viewer.RenderTheme = new MarkdownRenderTheme { RuleBrush = new SolidColorBrush(Colors.Crimson) };

            Assert.NotSame(before, viewer.Document);

            var rule = viewer.Document!.Blocks.OfType<Paragraph>().Single();
            Assert.Equal(Colors.Crimson, ((SolidColorBrush)rule.BorderBrush!).Color);
        });
    }
}
