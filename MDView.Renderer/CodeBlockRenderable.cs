using Spectre.Console;
using Spectre.Console.Rendering;

namespace MDView;

/// <summary>
/// A reusable code block component that renders syntax-highlighted code
/// inside a styled panel with a filled background, similar to VS Code.
/// </summary>
internal sealed class CodeBlockRenderable : IRenderable
{
    private static readonly Color Background = Color.Grey23;
    private static readonly Style BorderStyle = new(Color.Grey, Color.Grey23);

    private readonly IRenderable _panel;

    public CodeBlockRenderable(string code, string? language = null)
    {
        var highlighted = language is not null
            ? SyntaxHighlighter.Highlight(code, language)
            : (IRenderable)new Text(code);

        _panel = new Panel(new FilledBackground(highlighted, Background, horizontalPadding: 1))
        {
            Border = BoxBorder.Rounded,
            BorderStyle = BorderStyle,
            Padding = new Padding(0),
            Header = language is not null
                ? new PanelHeader($" [grey85]{Markup.Escape(language)}[/] ")
                : null
        };
    }

    public Measurement Measure(RenderOptions options, int maxWidth)
        => _panel.Measure(options, maxWidth);

    public IEnumerable<Segment> Render(RenderOptions options, int maxWidth)
        => _panel.Render(options, maxWidth);

    /// <summary>
    /// Applies a background color to every segment and pads each line to fill
    /// the available width, producing a solid colored block behind the content.
    /// </summary>
    internal sealed class FilledBackground(IRenderable inner, Color background, int horizontalPadding = 0) : IRenderable
    {
        public Measurement Measure(RenderOptions options, int maxWidth)
        {
            var pad = horizontalPadding * 2;
            var m = inner.Measure(options, Math.Max(1, maxWidth - pad));
            return new Measurement(m.Min + pad, m.Max + pad);
        }

        public IEnumerable<Segment> Render(RenderOptions options, int maxWidth)
        {
            var bgStyle = new Style(background: background);
            var pad = horizontalPadding > 0 ? new Segment(new string(' ', horizontalPadding), bgStyle) : null;
            var innerWidth = Math.Max(1, maxWidth - horizontalPadding * 2);
            int lineWidth = 0;
            bool atLineStart = true;

            foreach (var segment in inner.Render(options, innerWidth))
            {
                if (segment.IsLineBreak)
                {
                    var remaining = maxWidth - lineWidth;
                    if (remaining > 0)
                        yield return new Segment(new string(' ', remaining), bgStyle);
                    yield return segment;
                    lineWidth = 0;
                    atLineStart = true;
                }
                else
                {
                    if (atLineStart && pad is not null)
                    {
                        yield return pad;
                        lineWidth += horizontalPadding;
                        atLineStart = false;
                    }

                    var style = new Style(
                        segment.Style.Foreground,
                        background,
                        segment.Style.Decoration);
                    yield return new Segment(segment.Text, style);
                    lineWidth += segment.Text.GetCellWidth();
                }
            }

            if (lineWidth > 0 && lineWidth < maxWidth)
                yield return new Segment(new string(' ', maxWidth - lineWidth), bgStyle);
        }
    }
}
