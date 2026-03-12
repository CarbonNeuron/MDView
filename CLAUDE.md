# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

MDView is a terminal-based markdown viewer that renders markdown files with syntax-highlighted code blocks using rich console output. It reads a markdown file (or stdin) and displays styled output in the terminal.

## Build and Run

```bash
dotnet build                          # Build the project
dotnet run --project MDView -- file.md   # View a markdown file
cat file.md | dotnet run --project MDView   # Pipe from stdin
```

## Testing

```bash
dotnet test                           # Run all tests
dotnet test --project MDView.Renderer.Tests   # Renderer tests only
dotnet test --project MDView.Tests            # CLI tests only
```

Tests use xUnit. There are two test projects:

- **MDView.Renderer.Tests** — Tests for `MarkdownRenderer`, `SyntaxHighlighter`, `CodeBlockRenderable`, and internal renderables (`NonEmptyRenderable`, `PrefixedRenderable`). Uses a `RenderHelper` utility to extract plain text from Spectre `IRenderable` objects for assertions.
- **MDView.Tests** — Tests for `ViewCommand`, `ViewSettings`, and `FileExistsAttribute`.

CI runs `dotnet test` on every push and pull request via GitHub Actions (`.github/workflows/ci.yml`).

## Architecture

The solution contains four projects, all using the `MDView` namespace:

| Project | Type | Description |
|---------|------|-------------|
| **MDView** | Console app | CLI tool — reads files/stdin and displays rendered output |
| **MDView.Renderer** | Class library | Core renderer — parses markdown and produces Spectre.Console renderables |
| **MDView.Renderer.Tests** | xUnit tests | Tests for the renderer library |
| **MDView.Tests** | xUnit tests | Tests for the CLI |

**Pipeline:** CLI parsing → file/stdin reading → Markdig AST parsing → recursive rendering to Spectre.Console renderables → terminal output.

### Key Components

- **Program.cs** — Entry point. Sets UTF-8 encoding, creates Spectre `CommandApp<ViewCommand>`, runs CLI.
- **ViewCommand.cs / ViewSettings.cs** — Spectre.Console.Cli command. Reads file path arg or stdin, calls `MarkdownRenderer.Render()`, writes to `AnsiConsole`.
- **MarkdownRenderer.cs** — Static class, core of the app. Parses markdown with Markdig (using `UseAdvancedExtensions()`), then recursively converts the AST into Spectre `IRenderable` objects. Contains two internal custom renderables:
  - `NonEmptyRenderable` — wraps output to guarantee at least one Segment (prevents Spectre crashes on empty renderable lists).
  - `PrefixedRenderable` — prepends styled prefixes to rendered lines with wrap awareness; used for list bullets and blockquote borders.
- **CodeBlockRenderable.cs** — Renders fenced/indented code blocks inside a `Panel` with rounded borders. Contains nested `FilledBackground` class that applies a solid background color across every line of highlighted code.
- **SyntaxHighligher.cs** (note: filename typo is intentional/existing) — Thread-safe TextMate-based syntax highlighter. Maps 30+ language aliases to file extensions, loads grammars via `TextMateSharp.Registry`, tokenizes code, and converts TextMate scopes/styles to Spectre markup tags. Falls back to plain text on any error.

### Key Libraries

- **Markdig** — Markdown parsing into AST
- **Spectre.Console / Spectre.Console.Cli** — Rich terminal rendering and CLI framework
- **TextMateSharp** — VS Code-style syntax highlighting via TextMate grammars

### Design Notes

- `SyntaxHighlighter` uses a global lock (`Lock SyncRoot`) around all grammar/registry operations since TextMateSharp is not thread-safe.
- The `MarkdownRenderer.Render()` method is designed to be callable repeatedly on growing text (streaming use case) — it re-parses each time.
- Inline rendering returns Spectre markup strings; block rendering returns `IRenderable` objects. The two levels compose at `ParagraphBlock` boundaries via `SafeMarkup()`.
