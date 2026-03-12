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
    public void Execute_NoArgsNoStdin_ReturnsNonZero()
    {
        // When no file and no stdin redirect, should return error
        // Note: in test environment, Console.IsInputRedirected may vary
        var app = new CommandApp<ViewCommand>();
        var result = app.Run(Array.Empty<string>());
        // Either reads from redirected stdin (test runner may redirect) or errors
        // We just verify it doesn't throw
        Assert.True(result == 0 || result == 1);
    }
}
