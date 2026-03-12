using Spectre.Console;
using Spectre.Console.Cli;

namespace MDView;

public class FileExistsAttribute : ParameterValidationAttribute
{
    public FileExistsAttribute() : base("File not found")
    {
    }

    public override ValidationResult Validate(CommandParameterContext context)
    {
        if (context.Value is null)
            return ValidationResult.Success();

        if (!File.Exists(context.Value as string))
            return ValidationResult.Error("File not found");
        return ValidationResult.Success();
    }
}