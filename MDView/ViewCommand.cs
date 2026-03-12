using Spectre.Console;
using Spectre.Console.Cli;

namespace MDView;

public class ViewCommand : Command<ViewSettings>
{
    public override int Execute(CommandContext context, ViewSettings settings, CancellationToken cancellationToken)
    {
        string text;
        if (!string.IsNullOrEmpty(settings.Path))
        {
            text = File.ReadAllText(settings.Path);
        }
        else
        {
            if (Console.IsInputRedirected)
            {
                text = Console.In.ReadToEnd();
            }
            else
            {
                AnsiConsole.MarkupLine("[red]Error:[/] No file path provided and no input redirected from STDIN.");
                return 1;
            }
        }
        
        var rendered = MarkdownRenderer.Render(text);
        var width = Console.WindowWidth;
        if (width > 0)
            AnsiConsole.Console.Profile.Width = width;
        AnsiConsole.Write(rendered);
        return 0;
    }
}