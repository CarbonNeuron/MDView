using Spectre.Console.Cli;

namespace MDView.Tests;

public class ViewCommandTests
{
    [Fact]
    public void Execute_WithValidFile_ReturnsZero()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tempFile, "# Hello\n\nA paragraph.");
            var app = new CommandApp<ViewCommand>();
            var result = app.Run(new[] { tempFile });
            Assert.Equal(0, result);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void Execute_WithEmptyFile_ReturnsZero()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tempFile, "");
            var app = new CommandApp<ViewCommand>();
            var result = app.Run(new[] { tempFile });
            Assert.Equal(0, result);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void Execute_WithMarkdownContent_ReturnsZero()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tempFile, "- item1\n- item2\n\n```csharp\nvar x = 1;\n```");
            var app = new CommandApp<ViewCommand>();
            var result = app.Run(new[] { tempFile });
            Assert.Equal(0, result);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void Execute_WithNonExistentFile_ReturnsNonZero()
    {
        var app = new CommandApp<ViewCommand>();
        var result = app.Run(new[] { "/nonexistent/path/file.md" });
        Assert.NotEqual(0, result);
    }

    [Fact]
    public void Execute_NoArgsWithRedirectedStdin_ReturnsZero()
    {
        // In test/CI environments, stdin is typically redirected (empty).
        // The command reads empty stdin and renders it successfully.
        if (!Console.IsInputRedirected)
            return; // Skip when running interactively — would block on stdin

        var app = new CommandApp<ViewCommand>();
        var result = app.Run(Array.Empty<string>());
        Assert.Equal(0, result);
    }
}
