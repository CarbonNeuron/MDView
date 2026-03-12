using Spectre.Console;
using Spectre.Console.Rendering;

namespace MDView.Tests;

public class MarkdownRendererTests
{
    // -- Empty / whitespace input --

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Render_EmptyOrWhitespace_ReturnsNonEmptyRenderable(string input)
    {
        var result = MarkdownRenderer.Render(input);
        var text = RenderHelper.GetPlainText(result);
        Assert.NotNull(result);
        Assert.True(text.Length > 0);
    }

    // -- Headings --

    [Fact]
    public void Render_H1Heading_ContainsHeadingText()
    {
        var result = MarkdownRenderer.Render("# Hello World");
        var text = RenderHelper.GetPlainText(result);
        Assert.Contains("Hello World", text);
    }

    [Fact]
    public void Render_H2Heading_ContainsHeadingText()
    {
        var result = MarkdownRenderer.Render("## Section Two");
        var text = RenderHelper.GetPlainText(result);
        Assert.Contains("Section Two", text);
    }

    [Fact]
    public void Render_H3Heading_ContainsHeadingText()
    {
        var result = MarkdownRenderer.Render("### Subsection");
        var text = RenderHelper.GetPlainText(result);
        Assert.Contains("Subsection", text);
    }

    [Fact]
    public void Render_H4Heading_ContainsHeadingText()
    {
        var result = MarkdownRenderer.Render("#### Deep Heading");
        var text = RenderHelper.GetPlainText(result);
        Assert.Contains("Deep Heading", text);
    }

    // -- Paragraphs --

    [Fact]
    public void Render_SimpleParagraph_ContainsText()
    {
        var result = MarkdownRenderer.Render("This is a simple paragraph.");
        var text = RenderHelper.GetPlainText(result);
        Assert.Contains("This is a simple paragraph.", text);
    }

    [Fact]
    public void Render_MultipleParagraphs_ContainsBothTexts()
    {
        var result = MarkdownRenderer.Render("First paragraph.\n\nSecond paragraph.");
        var text = RenderHelper.GetPlainText(result);
        Assert.Contains("First paragraph.", text);
        Assert.Contains("Second paragraph.", text);
    }

    // -- Inline formatting --

    [Fact]
    public void Render_BoldText_ContainsText()
    {
        var result = MarkdownRenderer.Render("This is **bold** text.");
        var text = RenderHelper.GetPlainText(result);
        Assert.Contains("bold", text);
        Assert.Contains("This is", text);
    }

    [Fact]
    public void Render_ItalicText_ContainsText()
    {
        var result = MarkdownRenderer.Render("This is *italic* text.");
        var text = RenderHelper.GetPlainText(result);
        Assert.Contains("italic", text);
    }

    [Fact]
    public void Render_StrikethroughText_ContainsText()
    {
        var result = MarkdownRenderer.Render("This is ~~struck~~ text.");
        var text = RenderHelper.GetPlainText(result);
        Assert.Contains("struck", text);
    }

    [Fact]
    public void Render_InlineCode_ContainsCodeText()
    {
        var result = MarkdownRenderer.Render("Use `Console.WriteLine` here.");
        var text = RenderHelper.GetPlainText(result);
        Assert.Contains("Console.WriteLine", text);
    }

    // -- Links --

    [Fact]
    public void Render_Link_ContainsLinkText()
    {
        var result = MarkdownRenderer.Render("[Click here](https://example.com)");
        var text = RenderHelper.GetPlainText(result);
        Assert.Contains("Click here", text);
    }

    // -- Fenced code blocks --

    [Fact]
    public void Render_FencedCodeBlock_ContainsCode()
    {
        var md = "```csharp\nConsole.WriteLine(\"Hello\");\n```";
        var result = MarkdownRenderer.Render(md);
        var text = RenderHelper.GetPlainText(result);
        Assert.Contains("Console", text);
        Assert.Contains("Hello", text);
    }

    [Fact]
    public void Render_FencedCodeBlockWithoutLanguage_ContainsCode()
    {
        var md = "```\nsome plain code\n```";
        var result = MarkdownRenderer.Render(md);
        var text = RenderHelper.GetPlainText(result);
        Assert.Contains("some plain code", text);
    }

    // -- Indented code blocks --

    [Fact]
    public void Render_IndentedCodeBlock_ContainsCode()
    {
        var md = "Paragraph before.\n\n    indented code line\n\nParagraph after.";
        var result = MarkdownRenderer.Render(md);
        var text = RenderHelper.GetPlainText(result);
        Assert.Contains("indented code line", text);
    }

    // -- Lists --

    [Fact]
    public void Render_UnorderedList_ContainsAllItems()
    {
        var md = "- Apple\n- Banana\n- Cherry";
        var result = MarkdownRenderer.Render(md);
        var text = RenderHelper.GetPlainText(result);
        Assert.Contains("Apple", text);
        Assert.Contains("Banana", text);
        Assert.Contains("Cherry", text);
    }

    [Fact]
    public void Render_OrderedList_ContainsAllItems()
    {
        var md = "1. First\n2. Second\n3. Third";
        var result = MarkdownRenderer.Render(md);
        var text = RenderHelper.GetPlainText(result);
        Assert.Contains("First", text);
        Assert.Contains("Second", text);
        Assert.Contains("Third", text);
    }

    [Fact]
    public void Render_OrderedList_ContainsNumbers()
    {
        var md = "1. First\n2. Second";
        var result = MarkdownRenderer.Render(md);
        var text = RenderHelper.GetPlainText(result);
        Assert.Contains("1.", text);
        Assert.Contains("2.", text);
    }

    [Fact]
    public void Render_NestedList_ContainsAllItems()
    {
        var md = "- Parent\n  - Child\n  - Child2\n- Parent2";
        var result = MarkdownRenderer.Render(md);
        var text = RenderHelper.GetPlainText(result);
        Assert.Contains("Parent", text);
        Assert.Contains("Child", text);
        Assert.Contains("Parent2", text);
    }

    [Fact]
    public void Render_TaskList_ContainsItemText()
    {
        var md = "- [x] Done task\n- [ ] Pending task";
        var result = MarkdownRenderer.Render(md);
        var text = RenderHelper.GetPlainText(result);
        Assert.Contains("Done task", text);
        Assert.Contains("Pending task", text);
    }

    // -- Blockquotes --

    [Fact]
    public void Render_Blockquote_ContainsQuotedText()
    {
        var md = "> This is a quote.";
        var result = MarkdownRenderer.Render(md);
        var text = RenderHelper.GetPlainText(result);
        Assert.Contains("This is a quote.", text);
    }

    [Fact]
    public void Render_Blockquote_HasBorderPrefix()
    {
        var md = "> Quoted text here";
        var result = MarkdownRenderer.Render(md);
        var text = RenderHelper.GetPlainText(result);
        Assert.Contains("\u2502", text); // │ character
    }

    [Fact]
    public void Render_NestedBlockquote_ContainsText()
    {
        var md = "> Outer\n> > Inner";
        var result = MarkdownRenderer.Render(md);
        var text = RenderHelper.GetPlainText(result);
        Assert.Contains("Outer", text);
        Assert.Contains("Inner", text);
    }

    // -- Tables --

    [Fact]
    public void Render_Table_ContainsHeadersAndData()
    {
        var md = "| Name | Age |\n|------|-----|\n| Alice | 30 |\n| Bob | 25 |";
        var result = MarkdownRenderer.Render(md);
        var text = RenderHelper.GetPlainText(result);
        Assert.Contains("Name", text);
        Assert.Contains("Age", text);
        Assert.Contains("Alice", text);
        Assert.Contains("Bob", text);
        Assert.Contains("30", text);
        Assert.Contains("25", text);
    }

    // -- Thematic breaks --

    [Fact]
    public void Render_ThematicBreak_ProducesRenderable()
    {
        var md = "Before\n\n---\n\nAfter";
        var result = MarkdownRenderer.Render(md);
        var text = RenderHelper.GetPlainText(result);
        Assert.Contains("Before", text);
        Assert.Contains("After", text);
    }

    // -- HTML blocks --

    [Fact]
    public void Render_HtmlBlock_ContainsContent()
    {
        var md = "<div>Hello HTML</div>";
        var result = MarkdownRenderer.Render(md);
        var text = RenderHelper.GetPlainText(result);
        Assert.Contains("Hello HTML", text);
    }

    // -- Mixed content --

    [Fact]
    public void Render_ComplexDocument_ContainsAllParts()
    {
        var md = """
            # Title

            A paragraph with **bold** and *italic*.

            - Item 1
            - Item 2

            ```python
            print("hello")
            ```

            > A blockquote

            | Col1 | Col2 |
            |------|------|
            | A    | B    |
            """;
        var result = MarkdownRenderer.Render(md);
        var text = RenderHelper.GetPlainText(result);
        Assert.Contains("Title", text);
        Assert.Contains("bold", text);
        Assert.Contains("italic", text);
        Assert.Contains("Item 1", text);
        Assert.Contains("print", text);
        Assert.Contains("hello", text);
        Assert.Contains("A blockquote", text);
        Assert.Contains("Col1", text);
    }

    // -- NonEmptyRenderable guarantee --

    [Fact]
    public void Render_AlwaysProducesAtLeastOneSegment()
    {
        var result = MarkdownRenderer.Render("");
        var options = RenderHelper.CreateOptions();
        var segments = result.Render(options, 80).ToList();
        Assert.NotEmpty(segments);
    }

    // -- Repeated calls (streaming use case) --

    [Fact]
    public void Render_CalledRepeatedly_ProducesFreshOutput()
    {
        var result1 = MarkdownRenderer.Render("# First");
        var result2 = MarkdownRenderer.Render("# First\n\nMore content");
        var text1 = RenderHelper.GetPlainText(result1);
        var text2 = RenderHelper.GetPlainText(result2);
        Assert.Contains("First", text1);
        Assert.Contains("First", text2);
        Assert.Contains("More content", text2);
        Assert.DoesNotContain("More content", text1);
    }

    // -- Special characters --

    [Fact]
    public void Render_SpecialMarkupCharacters_DoesNotThrow()
    {
        var result = MarkdownRenderer.Render("Text with [brackets] and more.");
        var text = RenderHelper.GetPlainText(result);
        Assert.Contains("brackets", text);
    }
}
