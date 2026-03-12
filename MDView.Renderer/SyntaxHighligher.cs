using System.Globalization;
using System.Text;
using Spectre.Console;
using Spectre.Console.Rendering;
using TextMateSharp.Grammars;
using TextMateSharp.Themes;

namespace MDView;

/// <summary>
/// TextMate grammar-based syntax highlighter that converts source code into styled
/// Spectre.Console <see cref="IRenderable"/> objects. Supports theme switching.
/// </summary>
public static class SyntaxHighlighter
{
    private static readonly object SyncRoot = new();
    private static ThemeName _themeName = ThemeName.DarkPlus;
    private static RegistryOptions _options = new(ThemeName.DarkPlus);
    private static TextMateSharp.Registry.Registry _registry = new(_options);
    private static Theme _theme = _registry.GetTheme();

    private static readonly Dictionary<string, string> LanguageAliases =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["cs"] = ".cs", ["csharp"] = ".cs", ["c#"] = ".cs",
            ["js"] = ".js", ["javascript"] = ".js", ["jsx"] = ".jsx",
            ["ts"] = ".ts", ["typescript"] = ".ts", ["tsx"] = ".tsx",
            ["py"] = ".py", ["python"] = ".py",
            ["rb"] = ".rb", ["ruby"] = ".rb",
            ["rs"] = ".rs", ["rust"] = ".rs",
            ["go"] = ".go", ["golang"] = ".go",
            ["java"] = ".java",
            ["c"] = ".c", ["cpp"] = ".cpp", ["c++"] = ".cpp",
            ["h"] = ".h", ["hpp"] = ".hpp",
            ["html"] = ".html", ["htm"] = ".html",
            ["xml"] = ".xml", ["xaml"] = ".xml", ["svg"] = ".svg",
            ["css"] = ".css", ["scss"] = ".scss", ["less"] = ".less",
            ["json"] = ".json",
            ["yaml"] = ".yaml", ["yml"] = ".yaml",
            ["sql"] = ".sql",
            ["sh"] = ".sh", ["bash"] = ".sh", ["shell"] = ".sh", ["zsh"] = ".sh",
            ["ps1"] = ".ps1", ["powershell"] = ".ps1",
            ["md"] = ".md", ["markdown"] = ".md",
            ["php"] = ".php",
            ["swift"] = ".swift",
            ["kotlin"] = ".kt", ["kt"] = ".kt",
            ["dart"] = ".dart",
            ["lua"] = ".lua",
            ["r"] = ".r",
            ["fsharp"] = ".fs", ["fs"] = ".fs", ["f#"] = ".fs",
            ["vb"] = ".vb", ["vbnet"] = ".vb",
            ["toml"] = ".toml",
            ["dockerfile"] = ".dockerfile",
            ["tex"] = ".tex", ["latex"] = ".tex",
        };

    /// <summary>The currently active theme.</summary>
    public static ThemeName CurrentTheme
    {
        get { lock (SyncRoot) return _themeName; }
    }

    /// <summary>All available built-in themes.</summary>
    public static IReadOnlyList<ThemeName> AvailableThemes { get; } =
        (ThemeName[])Enum.GetValues(typeof(ThemeName));

    /// <summary>Switch to a different theme. Takes effect on subsequent <see cref="Highlight"/> calls.</summary>
    public static void SetTheme(ThemeName theme)
    {
        lock (SyncRoot)
        {
            if (_themeName == theme) return;
            _themeName = theme;
            _options = new RegistryOptions(theme);
            _registry = new TextMateSharp.Registry.Registry(_options);
            _theme = _registry.GetTheme();
        }
    }

    /// <summary>
    /// Highlights <paramref name="code"/> using TextMate grammars for the given <paramref name="language"/>.
    /// Returns plain <see cref="Text"/> when the language is unknown or highlighting fails.
    /// </summary>
    public static IRenderable Highlight(string code, string? language)
    {
        if (string.IsNullOrWhiteSpace(code))
            return new Text("");

        if (string.IsNullOrWhiteSpace(language))
            return new Text(code);

        lock (SyncRoot)
        {
            return HighlightCore(code, language);
        }
    }

    private static IRenderable HighlightCore(string code, string language)
    {
        var extension = ResolveExtension(language);
        if (extension is null)
            return new Text(code);

        string? scopeName;
        try { scopeName = _options.GetScopeByExtension(extension); }
        catch { return new Text(code); }

        if (string.IsNullOrEmpty(scopeName))
            return new Text(code);

        IGrammar? grammar;
        try { grammar = _registry.LoadGrammar(scopeName); }
        catch { return new Text(code); }

        if (grammar is null)
            return new Text(code);

        return TokenizeToMarkup(code, grammar);
    }

    private static IRenderable TokenizeToMarkup(string code, IGrammar grammar)
    {
        var sb = new StringBuilder();
        var lines = code.Split('\n');
        IStateStack? ruleStack = null;

        for (int i = 0; i < lines.Length; i++)
        {
            if (i > 0) sb.Append('\n');

            var line = lines[i];
            if (line.Length == 0)
                continue;

            ITokenizeLineResult result;
            try
            {
                result = grammar.TokenizeLine(line.AsMemory(), ruleStack, TimeSpan.MaxValue);
            }
            catch
            {
                sb.Append(Markup.Escape(line));
                continue;
            }

            ruleStack = result.RuleStack;

            foreach (var token in result.Tokens)
            {
                var start = Math.Min(token.StartIndex, line.Length);
                var end = Math.Min(token.EndIndex, line.Length);
                if (start >= end) continue;

                var tokenText = line[start..end];
                var escaped = Markup.Escape(tokenText);

                var styleTag = BuildStyleTag(token.Scopes);
                if (styleTag is not null)
                    sb.Append($"[{styleTag}]{escaped}[/]");
                else
                    sb.Append(escaped);
            }
        }

        try
        {
            return new Markup(sb.ToString());
        }
        catch
        {
            return new Text(code);
        }
    }

    private static string? BuildStyleTag(IReadOnlyList<string> scopes)
    {
        var foreground = -1;
        var fontStyle = FontStyle.NotSet;

        var scopeList = scopes as IList<string> ?? scopes.ToList();
        foreach (var rule in _theme.Match(scopeList))
        {
            if (foreground == -1 && rule.foreground > 0)
                foreground = rule.foreground;
            if (fontStyle == FontStyle.NotSet && rule.fontStyle > 0)
                fontStyle = rule.fontStyle;
        }

        if (foreground == -1 && fontStyle == FontStyle.NotSet)
            return null;

        var parts = new List<string>(3);

        if (foreground > 0)
        {
            var hexColor = _theme.GetColor(foreground);
            if (!string.IsNullOrEmpty(hexColor))
                parts.Add(hexColor);
        }

        if (fontStyle != FontStyle.NotSet)
        {
            if ((fontStyle & FontStyle.Bold) != 0) parts.Add("bold");
            if ((fontStyle & FontStyle.Italic) != 0) parts.Add("italic");
            if ((fontStyle & FontStyle.Underline) != 0) parts.Add("underline");
        }

        return parts.Count > 0 ? string.Join(" ", parts) : null;
    }

    private static string? ResolveExtension(string language) =>
        LanguageAliases.TryGetValue(language.Trim().ToLowerInvariant(), out var ext) ? ext : null;

    private static Color HexToColor(string hex)
    {
        if (hex.Length > 0 && hex[0] == '#')
            hex = hex.Substring(1);

        if (hex.Length < 6)
            return Color.Default;

        var r = byte.Parse(hex.Substring(0, 2), NumberStyles.HexNumber);
        var g = byte.Parse(hex.Substring(2, 2), NumberStyles.HexNumber);
        var b = byte.Parse(hex.Substring(4, 2), NumberStyles.HexNumber);
        return new Color(r, g, b);
    }
}