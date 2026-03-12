using Spectre.Console;
using Spectre.Console.Rendering;
using TextMateSharp.Grammars;

namespace MDView.Tests;

public class SyntaxHighlighterTests
{
    // -- Known languages --

    [Theory]
    [InlineData("csharp")]
    [InlineData("cs")]
    [InlineData("c#")]
    [InlineData("javascript")]
    [InlineData("js")]
    [InlineData("python")]
    [InlineData("py")]
    [InlineData("rust")]
    [InlineData("rs")]
    [InlineData("go")]
    [InlineData("html")]
    [InlineData("css")]
    [InlineData("json")]
    [InlineData("yaml")]
    [InlineData("sql")]
    [InlineData("bash")]
    [InlineData("sh")]
    [InlineData("typescript")]
    [InlineData("ts")]
    public void Highlight_KnownLanguage_ReturnsRenderable(string language)
    {
        var result = SyntaxHighlighter.Highlight("var x = 1;", language);
        Assert.NotNull(result);
        var text = RenderHelper.GetPlainText(result);
        Assert.Contains("x", text);
    }

    // -- Unknown language --

    [Fact]
    public void Highlight_UnknownLanguage_ReturnsPlainText()
    {
        var result = SyntaxHighlighter.Highlight("some code", "nonexistentlang");
        var text = RenderHelper.GetPlainText(result);
        Assert.Contains("some code", text);
    }

    // -- Empty / null input --

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Highlight_EmptyOrWhitespaceCode_ReturnsRenderable(string code)
    {
        var result = SyntaxHighlighter.Highlight(code, "csharp");
        Assert.NotNull(result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Highlight_NullOrEmptyLanguage_ReturnsPlainText(string? language)
    {
        var result = SyntaxHighlighter.Highlight("var x = 1;", language);
        var text = RenderHelper.GetPlainText(result);
        Assert.Contains("var x = 1;", text);
    }

    // -- Multi-line code --

    [Fact]
    public void Highlight_MultiLineCode_PreservesAllLines()
    {
        var code = "var a = 1;\nvar b = 2;\nvar c = 3;";
        var result = SyntaxHighlighter.Highlight(code, "csharp");
        var text = RenderHelper.GetPlainText(result);
        Assert.Contains("a", text);
        Assert.Contains("b", text);
        Assert.Contains("c", text);
    }

    // -- Code with special characters --

    [Fact]
    public void Highlight_CodeWithSpecialChars_DoesNotThrow()
    {
        var code = "if (x < 10 && y > 5) { return \"[value]\"; }";
        var result = SyntaxHighlighter.Highlight(code, "csharp");
        var text = RenderHelper.GetPlainText(result);
        Assert.Contains("return", text);
    }

    // -- Theme switching --

    [Fact]
    public void SetTheme_ChangesCurrentTheme()
    {
        var original = SyntaxHighlighter.CurrentTheme;
        try
        {
            SyntaxHighlighter.SetTheme(ThemeName.Light);
            Assert.Equal(ThemeName.Light, SyntaxHighlighter.CurrentTheme);

            SyntaxHighlighter.SetTheme(ThemeName.Monokai);
            Assert.Equal(ThemeName.Monokai, SyntaxHighlighter.CurrentTheme);
        }
        finally
        {
            SyntaxHighlighter.SetTheme(original);
        }
    }

    [Fact]
    public void SetTheme_SameTheme_DoesNothing()
    {
        var current = SyntaxHighlighter.CurrentTheme;
        SyntaxHighlighter.SetTheme(current);
        Assert.Equal(current, SyntaxHighlighter.CurrentTheme);
    }

    [Fact]
    public void AvailableThemes_ContainsMultipleThemes()
    {
        Assert.True(SyntaxHighlighter.AvailableThemes.Count > 1);
    }

    // -- Highlight produces styled output for known language --

    [Fact]
    public void Highlight_CSharpCode_ProducesStyledSegments()
    {
        var code = "public class Foo { }";
        var result = SyntaxHighlighter.Highlight(code, "csharp");
        var options = RenderHelper.CreateOptions();
        var segments = result.Render(options, 120).Where(s => !s.IsLineBreak).ToList();
        Assert.NotEmpty(segments);
        Assert.True(segments.Count > 1, "Syntax highlighting should produce multiple styled segments");
    }

    // -- Language alias coverage --

    [Theory]
    [InlineData("c++", "cpp")]
    [InlineData("golang", "go")]
    [InlineData("ruby", "rb")]
    [InlineData("shell", "sh")]
    [InlineData("powershell", "ps1")]
    [InlineData("kotlin", "kt")]
    [InlineData("fsharp", "fs")]
    [InlineData("f#", "fs")]
    [InlineData("vbnet", "vb")]
    [InlineData("markdown", "md")]
    [InlineData("yml", "yaml")]
    [InlineData("htm", "html")]
    public void Highlight_LanguageAliases_BothProduceOutput(string alias1, string alias2)
    {
        var code = "x = 1";
        var result1 = SyntaxHighlighter.Highlight(code, alias1);
        var result2 = SyntaxHighlighter.Highlight(code, alias2);
        var text1 = RenderHelper.GetPlainText(result1);
        var text2 = RenderHelper.GetPlainText(result2);
        Assert.True(text1.Length > 0);
        Assert.True(text2.Length > 0);
    }
}
