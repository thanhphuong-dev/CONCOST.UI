using System;
using System.Windows;
using System.Windows.Media;
using CONCOST.UI.Controls;
using Xunit;

namespace CONCOST.UI.Tests;

/// <summary>
/// The editor used to fall back to hard-coded VS Code colours because the
/// <c>CONCOST.Code.*</c> tokens it looked up were never defined. These tests pin the tokens in both
/// themes and the per-instance overrides.
/// </summary>
public class CodeEditorCustomizationTests
{
    private static readonly string[] Tokens =
    [
        "CONCOST.Code.Keyword",
        "CONCOST.Code.String",
        "CONCOST.Code.Comment",
        "CONCOST.Code.Number",
        "CONCOST.Code.Tag",
        "CONCOST.Code.Attribute",
    ];

    private static ResourceDictionary Load(string fileName) => new()
    {
        Source = new Uri($"pack://application:,,,/CONCOST.UI;component/Themes/{fileName}", UriKind.Absolute)
    };

    [Theory]
    [InlineData("Light.xaml")]
    [InlineData("Dark.xaml")]
    public void Theme_exposes_every_syntax_token(string fileName)
    {
        WpfApplicationHost.Run(() =>
        {
            var dictionary = Load(fileName);

            foreach (string key in Tokens)
            {
                Assert.True(dictionary.Contains(key), $"{fileName} is missing {key}.");
                Assert.IsAssignableFrom<Brush>(dictionary[key]);
            }
        });
    }

    [Fact]
    public void Syntax_brushes_can_be_overridden_per_instance()
    {
        WpfApplicationHost.Run(() =>
        {
            var editor = new CodeEditor();

            Assert.Null(editor.KeywordBrush);

            editor.KeywordBrush = Brushes.Red;

            Assert.Same(Brushes.Red, editor.KeywordBrush);
            Assert.Null(editor.StringBrush);
        });
    }
}
