# MDView

A terminal-based markdown viewer that renders markdown files with syntax-highlighted code blocks using rich console output.

## Features

- Full markdown rendering in the terminal (headings, lists, blockquotes, tables, links, emphasis, etc.)
- Syntax-highlighted fenced code blocks powered by TextMate grammars (30+ languages)
- Supports reading from a file path or piped stdin
- Styled output via Spectre.Console

## Requirements

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download)

## Usage

```bash
# View a markdown file
dotnet run --project MDView -- README.md

# Pipe from stdin
cat README.md | dotnet run --project MDView
```

## Building

```bash
dotnet build
```

## Dependencies

- [Markdig](https://github.com/xoofx/markdig) - Markdown parsing
- [Spectre.Console](https://spectreconsole.net/) - Rich terminal rendering and CLI framework
- [TextMateSharp](https://github.com/nicknash/TextMateSharp) - VS Code-style syntax highlighting
