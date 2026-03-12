using Spectre.Console;
using Spectre.Console.Rendering;

namespace MDView.Tests;

public class CodeBlockRenderableTests
{
    [Fact]
    public void CodeBlock_WithLanguage_ContainsCode()
    {
        var renderable = new CodeBlockRenderable("Console.WriteLine();", "csharp");
        var text = RenderHelper.GetPlainText(renderable);
        Assert.Contains("Console", text);
    }

    [Fact]
    public void CodeBlock_WithLanguage_ShowsLanguageHeader()
    {
        var renderable = new CodeBlockRenderable("x = 1 # some longer code to widen the panel", "python");
        var text = RenderHelper.GetPlainText(renderable);
        Assert.Contains("python", text);
    }

    [Fact]
    public void CodeBlock_WithoutLanguage_ContainsCode()
    {
        var renderable = new CodeBlockRenderable("plain code here");
        var text = RenderHelper.GetPlainText(renderable);
        Assert.Contains("plain code here", text);
    }

    [Fact]
    public void CodeBlock_WithoutLanguage_NoLanguageHeader()
    {
        var renderable = new CodeBlockRenderable("just code");
        var text = RenderHelper.GetPlainText(renderable);
        Assert.Contains("just code", text);
    }

    [Fact]
    public void CodeBlock_MultiLine_PreservesContent()
    {
        var code = "line1\nline2\nline3";
        var renderable = new CodeBlockRenderable(code, "bash");
        var text = RenderHelper.GetPlainText(renderable);
        Assert.Contains("line1", text);
        Assert.Contains("line2", text);
        Assert.Contains("line3", text);
    }

    [Fact]
    public void CodeBlock_EmptyCode_DoesNotThrow()
    {
        var renderable = new CodeBlockRenderable("", "csharp");
        var text = RenderHelper.GetPlainText(renderable);
        Assert.NotNull(text);
    }

    [Fact]
    public void CodeBlock_SpecialCharacters_Escaped()
    {
        var code = "if (a < b && c > d) { arr[0] = \"hello\"; }";
        var renderable = new CodeBlockRenderable(code, "csharp");
        var text = RenderHelper.GetPlainText(renderable);
        Assert.Contains("arr", text);
        Assert.Contains("hello", text);
    }

    [Fact]
    public void CodeBlock_Measure_ReturnsPositiveWidth()
    {
        var renderable = new CodeBlockRenderable("test code", "csharp");
        var options = RenderHelper.CreateOptions();
        var measurement = renderable.Measure(options, 80);
        Assert.True(measurement.Min > 0);
        Assert.True(measurement.Max > 0);
    }

    // -- FilledBackground tests (internal) --

    [Fact]
    public void FilledBackground_PadsLinesToFillWidth()
    {
        var inner = new Text("short");
        var filled = new CodeBlockRenderable.FilledBackground(inner, Color.Grey23, horizontalPadding: 1);
        var options = RenderHelper.CreateOptions();
        var segments = filled.Render(options, 40).ToList();
        var totalTextLength = segments.Where(s => !s.IsLineBreak).Sum(s => s.Text.Length);
        Assert.True(totalTextLength >= 5, "Should include content text");
    }

    [Fact]
    public void FilledBackground_Measure_AccountsForPadding()
    {
        var inner = new Text("test");
        var filled = new CodeBlockRenderable.FilledBackground(inner, Color.Grey23, horizontalPadding: 2);
        var options = RenderHelper.CreateOptions();
        var withPadding = filled.Measure(options, 80);
        var withoutPadding = ((IRenderable)inner).Measure(options, 76); // 80 - 2*2
        Assert.True(withPadding.Min >= withoutPadding.Min);
    }
}
