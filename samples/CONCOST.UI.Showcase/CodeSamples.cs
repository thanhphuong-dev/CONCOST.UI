namespace CONCOST.UI.Showcase;

public static class CodeSamples
{
    public static string ButtonXaml =>
        "<Button Style=\"{StaticResource CONCOST.Button.Primary}\" Content=\"Click Me\" />\n" +
        "<Button Style=\"{StaticResource CONCOST.Button.Secondary}\" Content=\"Cancel\" />\n" +
        "<Button Style=\"{StaticResource CONCOST.Button.Success}\" Content=\"Save\" />\n" +
        "<Button Style=\"{StaticResource CONCOST.Button.Outline}\" Content=\"Learn More\" />\n" +
        "<Button Style=\"{StaticResource CONCOST.Button.Danger}\" Content=\"Delete\" />";

    public static string ButtonCSharp =>
        "var btn = new Button();\n" +
        "btn.Style = (Style)FindResource(\"CONCOST.Button.Primary\");\n" +
        "btn.Content = \"Click Me\";\n" +
        "btn.Click += (s, e) => MessageBox.Show(\"Clicked!\");";

    public static string TextBoxXaml =>
        "<TextBox Style=\"{StaticResource CONCOST.TextBox.Base}\"\n" +
        "         attached:InfoElement.Placeholder=\"Enter text...\" />";

    public static string TextBoxCSharp =>
        "var tb = new TextBox();\n" +
        "tb.Style = (Style)FindResource(\"CONCOST.TextBox.Base\");\n" +
        "InfoElement.SetPlaceholder(tb, \"Enter text...\");";
}
