using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;

namespace MDView;

public class ViewSettings : CommandSettings
{
    [FileExists]
    [CommandArgument(0, "[path]")]
    [Description("The file to view")]
    public string? Path { get; init; }
    
}