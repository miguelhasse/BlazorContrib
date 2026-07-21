using Markdig.Parsers;
using Markdig.Syntax;

namespace Hasseware.Markdig.Renderers
{
    internal class CodeBlockRenderer : BlazorObjectRenderer<CodeBlock>
    {
        protected override void Write(BlazorRenderer renderer, CodeBlock code)
        {
            renderer.OpenElement("pre");
            renderer.OpenElement("code");

            if (code is IFencedBlock fencedCodeBlock && fencedCodeBlock.Info != null)
            {
                var infoPrefix = (code.Parser as FencedCodeBlockParser)?.InfoPrefix ?? FencedCodeBlockParser.DefaultInfoPrefix;
                renderer.AddAttribute("class", string.Concat(infoPrefix, fencedCodeBlock.Info));
            }

            WriteLines(renderer, code);

            renderer.CloseElement();
            renderer.CloseElement();
        }

        // Writes the code block's raw lines joined by literal newlines (instead of <br> elements),
        // so the content stays a single text run inside <code> for copy/paste and client-side
        // syntax highlighters (e.g. Prism, highlight.js) that operate on the element's text content.
        private static void WriteLines(BlazorRenderer renderer, CodeBlock code)
        {
            if (code.Lines.Lines != null)
            {
                for (int n = 0; n < code.Lines.Count; n++)
                {
                    if (n > 0)
                    {
                        renderer.AddContent("\n");
                    }

                    renderer.Write(code.Lines.Lines[n].Slice);
                }
            }
        }
    }
}
