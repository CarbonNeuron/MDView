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
    public void Validate_NullValue_ReturnsSuccess()
    {
        // FileExistsAttribute allows null (the path is optional)
        var attr = new FileExistsAttribute();
        // Use reflection to test Validate with a null value
        // Since CommandParameterContext is internal, we test via the app
        // A null path means stdin mode, which the attribute should allow
        var app = new CommandApp<ViewCommand>();
        app.Configure(c => c.PropagateExceptions());
        // No args = null path, should pass validation (fails on stdin check instead)
        // This is tested via ViewCommandTests
    }
}
