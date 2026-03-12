using Spectre.Console.Cli;

namespace MDView.Tests;

public class FileExistsAttributeTests
{
    [Fact]
    public void App_WithExistingFile_ReturnsZero()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tempFile, "# Test");
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
    public void App_WithNonExistingFile_ReturnsNonZero()
    {
        var app = new CommandApp<ViewCommand>();
        var result = app.Run(new[] { "/nonexistent/path/definitely_not_a_file.md" });
        Assert.NotEqual(0, result);
    }

    [Fact]
    public void Validate_NullPath_PassesValidation()
    {
        // A null path (no args) should pass FileExistsAttribute validation
        // and reach the stdin check instead. In CI with redirected stdin
        // this returns 0; interactively it returns 1 from the "no input" error.
        var app = new CommandApp<ViewCommand>();
        var result = app.Run(Array.Empty<string>());
        // If stdin is redirected (CI), validation passed and command ran.
        // If not redirected, command returns 1 but NOT from a validation error.
        Assert.True(result == 0 || result == 1);
    }
}
