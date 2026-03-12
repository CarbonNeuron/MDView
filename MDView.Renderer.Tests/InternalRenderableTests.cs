using Spectre.Console;
using Spectre.Console.Rendering;
using static MDView.MarkdownRenderer;

namespace MDView.Tests;

public class NonEmptyRenderableTests
{
    [Fact]
    public void NonEmptyRenderable_WithContent_PassesThroughSegments()
    {
        var inner = new Text("hello world");
        var wrapped = new NonEmptyRenderable(inner);
        var text = RenderHelper.GetPlainText(wrapped);
        Assert.Contains("hello world", text);
    }

    [Fact]
    public void NonEmptyRenderable_WithEmptyContent_ProducesAtLeastOneSegment()
    {
        var inner = new Rows(new List<IRenderable>());
        var wrapped = new NonEmptyRenderable(inner);
        var options = RenderHelper.CreateOptions();
        var segments = wrapped.Render(options, 80).ToList();
        Assert.NotEmpty(segments);
    }

    [Fact]
    public void NonEmptyRenderable_Measure_DelegatesToInner()
    {
        var inner = new Text("measure me");
        var wrapped = new NonEmptyRenderable(inner);
        var options = RenderHelper.CreateOptions();
        var innerMeasure = ((IRenderable)inner).Measure(options, 80);
        var wrappedMeasure = ((IRenderable)wrapped).Measure(options, 80);
        Assert.Equal(innerMeasure.Min, wrappedMeasure.Min);
        Assert.Equal(innerMeasure.Max, wrappedMeasure.Max);
    }
}

public class PrefixedRenderableTests
{
    [Fact]
    public void PrefixedRenderable_AddsPrefix()
    {
        var inner = new Text("item text");
        var prefixed = new PrefixedRenderable("[grey]>[/] ", 2, inner);
        var text = RenderHelper.GetPlainText(prefixed);
        Assert.Contains(">", text);
        Assert.Contains("item text", text);
    }

    [Fact]
    public void PrefixedRenderable_WithRepeatPrefix_RepeatsOnEachLine()
    {
        var inner = new Rows(new Text("line1"), new Text("line2"));
        var prefixed = new PrefixedRenderable("[grey]| [/]", 2, inner, repeatPrefix: true);
        var text = RenderHelper.GetPlainText(prefixed);
        Assert.Contains("line1", text);
        Assert.Contains("line2", text);
        var lines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        var prefixedLines = lines.Where(l => l.Contains("|")).ToList();
        Assert.True(prefixedLines.Count >= 2, "Repeated prefix should appear on multiple lines");
    }

    [Fact]
    public void PrefixedRenderable_WithoutRepeatPrefix_PadsSubsequentLines()
    {
        var inner = new Rows(new Text("line1"), new Text("line2"));
        var prefixed = new PrefixedRenderable("[grey]* [/]", 2, inner, repeatPrefix: false);
        var text = RenderHelper.GetPlainText(prefixed);
        Assert.Contains("line1", text);
        Assert.Contains("line2", text);
    }

    [Fact]
    public void PrefixedRenderable_Measure_IncludesPrefixWidth()
    {
        var inner = new Text("content");
        var prefixed = new PrefixedRenderable(">> ", 3, inner);
        var options = RenderHelper.CreateOptions();
        var innerMeasure = ((IRenderable)inner).Measure(options, 77); // 80 - 3
        var prefixedMeasure = ((IRenderable)prefixed).Measure(options, 80);
        Assert.Equal(innerMeasure.Min + 3, prefixedMeasure.Min);
        Assert.Equal(innerMeasure.Max + 3, prefixedMeasure.Max);
    }
}
