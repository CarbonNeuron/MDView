namespace MDView.Tests;

public class ViewSettingsTests
{
    [Fact]
    public void Path_DefaultsToNull()
    {
        var settings = new ViewSettings();
        Assert.Null(settings.Path);
    }

    [Fact]
    public void Path_CanBeSet()
    {
        var settings = new ViewSettings { Path = "/some/file.md" };
        Assert.Equal("/some/file.md", settings.Path);
    }
}
