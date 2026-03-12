// See https://aka.ms/new-console-template for more information

using MDView;
using Spectre.Console.Cli;

Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.InputEncoding = System.Text.Encoding.UTF8;

var app = new CommandApp<ViewCommand>();
return app.Run(args);