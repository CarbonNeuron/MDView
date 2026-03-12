using System.Text;
using Markdig;
using Markdig.Extensions.Tables;
using Markdig.Extensions.TaskLists;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace MDView;

/// <summary>
/// Converts a markdown string into Spectre.Console <see cref="IRenderable"/> objects.
/// Can be called repeatedly on growing text (streaming) — it re-parses each time.
/// </summary>
public static class MarkdownRenderer
{
    private static readonly MarkdownPipeline Pipeline =
        new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .Build();

    public static IRenderable Render(string markdown)
    {
        if (string.IsNullOrWhiteSpace(markdown))
            return new Text(" ");

        var doc = Markdown.Parse(markdown, Pipeline);
        var parts = new List<IRenderable>();

        foreach (var block in doc)
        {
            var renderable = RenderBlock(block);
            if (renderable is not null)
            {
                if (parts.Count > 0)
                    parts.Add(new Text(""));
                parts.Add(renderable);
            }
        }

        // Fallback: if Markdig produced no blocks, show raw text
        if (parts.Count == 0 && markdown.Length > 0)
            parts.Add(new Markup(Markup.Escape(markdown)));

        // Wrap to guarantee at least one segment — empty renderables crash
        // Spectre's LiveDisplay (SegmentShape.Calculate calls Max on empty list).
        var result = parts.Count > 0 ? (IRenderable)new Rows(parts) : new Text(" ");
        return new NonEmptyRenderable(result);
    }

    // ── Block-level ──────────────────────────────────────────────

    private static IRenderable? RenderBlock(Block block) => block switch
    {
        HeadingBlock heading              => RenderHeading(heading),
        ParagraphBlock para               => SafeMarkup(RenderInlines(para.Inline)),
        FencedCodeBlock fenced            => RenderFencedCode(fenced),
        CodeBlock code                    => RenderCodeBlock(code),
        ListBlock list                    => RenderList(list),
        QuoteBlock quote                  => RenderQuote(quote),
        HtmlBlock html                    => RenderHtmlBlock(html),
        ThematicBreakBlock                => new Rule { Style = new Style(Color.Grey) },
        Markdig.Extensions.Tables.Table t => RenderTable(t),
        _ => block is LeafBlock leaf && leaf.Inline != null
            ? SafeMarkup(RenderInlines(leaf.Inline))
            : null,
    };

    /// <summary>Returns a <see cref="Markup"/> or null when the text is empty
    /// (empty Markup produces zero segments, which crashes LiveDisplay).</summary>
    private static IRenderable? SafeMarkup(string text)
        => string.IsNullOrEmpty(text) ? null : new Markup(text);

    private static IRenderable RenderHeading(HeadingBlock heading)
    {
        var text = RenderInlines(heading.Inline);
        return heading.Level switch
        {
            1 => new Rule($"[bold blue]{text}[/]") { Style = new Style(Color.Grey), Justification = Justify.Left },
            2 => new Rule($"[bold white]{text}[/]") { Style = new Style(Color.Grey), Justification = Justify.Left },
            3 => new Markup($"[bold italic]{text}[/]"),
            _ => new Markup($"[bold dim]{text}[/]"),
        };
    }

    private static IRenderable RenderFencedCode(FencedCodeBlock code)
    {
        var content = GetLeafText(code);
        var language = string.IsNullOrWhiteSpace(code.Info) ? null : code.Info;
        return new CodeBlockRenderable(content, language);
    }

    private static IRenderable RenderCodeBlock(CodeBlock code)
        => new CodeBlockRenderable(GetLeafText(code));

    private static IRenderable RenderHtmlBlock(HtmlBlock html)
    {
        var content = GetLeafText(html);
        return string.IsNullOrWhiteSpace(content)
            ? new Text(" ")
            : new CodeBlockRenderable(content, "html");
    }

    private static IRenderable RenderList(ListBlock list, int depth = 0)
    {
        var rows = new List<IRenderable>();
        var index = list.OrderedStart is not null && int.TryParse(list.OrderedStart, out var start)
            ? start : 1;

        foreach (var item in list)
        {
            if (item is not ListItemBlock listItem) continue;

            var taskInline = listItem.Count > 0 && listItem[0] is ParagraphBlock taskPara
                ? taskPara.Inline?.FirstChild as TaskList
                : null;

            string prefixMarkup;
            int prefixWidth;

            if (taskInline != null)
            {
                prefixMarkup = taskInline.Checked ? ":check_mark_button: " : ":white_square_button: ";
                prefixWidth = 2;
            }
            else if (list.IsOrdered)
            {
                var num = $"{index++}. ";
                prefixMarkup = Markup.Escape(num);
                prefixWidth = num.Length;
            }
            else
            {
                var bullets = new[] { "•", "◦", "‣", "⁃" };
                var bullet = bullets[Math.Min(depth, bullets.Length - 1)];
                prefixMarkup = $"[grey]{bullet}[/] ";
                prefixWidth = 2;
            }

            var innerParts = new List<IRenderable>();
            foreach (var sub in listItem)
            {
                if (sub is ParagraphBlock para)
                {
                    var m = SafeMarkup(RenderInlines(para.Inline));
                    if (m is not null) innerParts.Add(m);
                }
                else if (sub is ListBlock nested)
                    innerParts.Add(RenderList(nested, depth + 1));
                else
                {
                    var r = RenderBlock(sub);
                    if (r is not null) innerParts.Add(r);
                }
            }

            var innerContent = innerParts.Count == 1 ? innerParts[0] : new Rows(innerParts);
            rows.Add(new PrefixedRenderable(prefixMarkup, prefixWidth, innerContent));
        }

        return new Rows(rows);
    }

    private static IRenderable RenderQuote(QuoteBlock quote)
    {
        var inner = new List<IRenderable>();

        foreach (var child in quote)
        {
            if (child is ParagraphBlock para)
            {
                var text = RenderInlines(para.Inline);
                if (!string.IsNullOrEmpty(text))
                    inner.Add(new Markup($"[italic]{text}[/]"));
            }
            else
            {
                var r = RenderBlock(child);
                if (r is not null) inner.Add(r);
            }
        }

        var content = inner.Count == 1 ? inner[0] : new Rows(inner);
        return new PrefixedRenderable("[dim]│ [/]", 2, content, repeatPrefix: true);
    }

    private static IRenderable RenderTable(Markdig.Extensions.Tables.Table mdTable)
    {
        var spectreTable = new Spectre.Console.Table
        {
            Border = TableBorder.Rounded,
            BorderStyle = new Style(Color.Grey)
        };

        if (mdTable.Count > 0 && mdTable[0] is Markdig.Extensions.Tables.TableRow headerRow)
        {
            foreach (var cell in headerRow.OfType<TableCell>())
                spectreTable.AddColumn(new TableColumn(new Markup(RenderCellMarkup(cell))));

            for (var i = 1; i < mdTable.Count; i++)
            {
                if (mdTable[i] is Markdig.Extensions.Tables.TableRow dataRow)
                {
                    var cells = dataRow.OfType<TableCell>()
                        .Select(c => (IRenderable)new Markup(RenderCellMarkup(c)))
                        .ToList();
                    if (cells.Count > 0)
                        spectreTable.AddRow(cells);
                }
            }
        }

        return spectreTable;
    }

    // ── Inline-level ─────────────────────────────────────────────

    private static string RenderInlines(ContainerInline? container)
    {
        if (container is null) return "";
        var sb = new StringBuilder();
        foreach (var inline in container)
            sb.Append(RenderInline(inline));
        return sb.ToString();
    }

    private static string RenderInline(Inline inline) => inline switch
    {
        TaskList => "",

        LiteralInline lit => Markup.Escape(lit.Content.ToString()),

        EmphasisInline { DelimiterChar: '~' } em
            => $"[strikethrough]{RenderInlineChildren(em)}[/]",

        EmphasisInline { DelimiterChar: '+' } em
            => $"[underline]{RenderInlineChildren(em)}[/]",

        EmphasisInline { DelimiterCount: >= 2 } em
            => $"[bold]{RenderInlineChildren(em)}[/]",

        EmphasisInline em
            => $"[italic]{RenderInlineChildren(em)}[/]",

        CodeInline code
            => $"[grey85 on grey23]{Markup.Escape(code.Content)}[/]",

        LinkInline link => RenderLink(link),

        HtmlInline html => $"[grey85 on grey23]{Markup.Escape(html.Tag)}[/]",

        LineBreakInline lb => lb.IsHard ? "\n" : " ",

        ContainerInline container => RenderInlineChildren(container),

        _ => Markup.Escape(inline.ToString() ?? ""),
    };

    private static string RenderInlineChildren(ContainerInline container)
    {
        var sb = new StringBuilder();
        foreach (var child in container)
            sb.Append(RenderInline(child));
        return sb.ToString();
    }

    private static string RenderLink(LinkInline link)
    {
        var text = RenderInlineChildren(link);
        if (link.Url is null)
            return text;

        // Encode characters that break Spectre's markup tag parser:
        // spaces (parsed as style separators), [ and ] (markup delimiters)
        var safeUrl = link.Url
            .Replace("%", "%25")
            .Replace(" ", "%20")
            .Replace("[", "%5B")
            .Replace("]", "%5D");
        return $"[blue underline link={Markup.Escape(safeUrl)}]{text}[/]";
    }

    private static string RenderCellMarkup(TableCell cell)
    {
        var sb = new StringBuilder();
        foreach (var block in cell)
        {
            if (block is ParagraphBlock para && para.Inline != null)
                sb.Append(RenderInlines(para.Inline));
        }
        return sb.ToString();
    }

    // ── Helpers ──────────────────────────────────────────────────

    private static string GetLeafText(LeafBlock leaf)
    {
        var sb = new StringBuilder();
        var lines = leaf.Lines;

        for (int i = 0; i < lines.Count; i++)
        {
            if (i > 0) sb.Append('\n');
            sb.Append(lines.Lines[i].Slice.ToString());
        }

        return sb.ToString();
    }

    // ── Segment-safety wrapper ────────────────────────────────────

    /// <summary>
    /// Wraps an inner renderable and guarantees at least one <see cref="Segment"/>
    /// is produced. Spectre's <c>LiveRenderable</c> crashes when a renderable
    /// yields zero segments (SegmentShape.Calculate calls Max on an empty list).
    /// </summary>
    internal sealed class NonEmptyRenderable(IRenderable inner) : IRenderable
    {
        public Measurement Measure(RenderOptions options, int maxWidth)
            => inner.Measure(options, maxWidth);

        public IEnumerable<Segment> Render(RenderOptions options, int maxWidth)
        {
            bool any = false;
            foreach (var segment in inner.Render(options, maxWidth))
            {
                any = true;
                yield return segment;
            }
            if (!any)
                yield return new Segment(" ");
        }
    }

    // ── Wrap-aware prefix renderable ─────────────────────────────

    /// <summary>
    /// Prepends a styled prefix to every rendered line. Inner content is measured
    /// and rendered at <c>maxWidth - prefixWidth</c> so the prefix never causes overflow.
    /// </summary>
    internal sealed class PrefixedRenderable(
        string prefixMarkup, int prefixWidth, IRenderable inner,
        bool repeatPrefix = false) : IRenderable
    {
        public Measurement Measure(RenderOptions options, int maxWidth)
        {
            var m = inner.Measure(options, Math.Max(1, maxWidth - prefixWidth));
            return new Measurement(m.Min + prefixWidth, m.Max + prefixWidth);
        }

        public IEnumerable<Segment> Render(RenderOptions options, int maxWidth)
        {
            var innerWidth = Math.Max(1, maxWidth - prefixWidth);

            var prefixSegments = ((IRenderable)new Markup(prefixMarkup))
                .Render(options, prefixWidth)
                .Where(s => !s.IsLineBreak)
                .ToList();

            var padSegment = new Segment(new string(' ', prefixWidth));

            bool atLineStart = true;
            bool firstLine = true;

            foreach (var segment in inner.Render(options, innerWidth))
            {
                if (atLineStart && !segment.IsLineBreak)
                {
                    if (firstLine)
                    {
                        foreach (var ps in prefixSegments)
                            yield return ps;
                        firstLine = false;
                    }
                    else
                    {
                        if (repeatPrefix)
                            foreach (var ps in prefixSegments)
                                yield return ps;
                        else
                            yield return padSegment;
                    }

                    atLineStart = false;
                }

                yield return segment;

                if (segment.IsLineBreak)
                    atLineStart = true;
            }
        }
    }
}