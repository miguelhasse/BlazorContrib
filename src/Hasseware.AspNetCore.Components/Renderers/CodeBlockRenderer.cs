using Markdig.Parsers;
using Markdig.Syntax;

namespace Hasseware.Markdig.Renderers
{
    internal class CodeBlockRenderer : BlazorObjectRenderer<CodeBlock>
    {
        protected override void Write(BlazorRenderer renderer, CodeBlock code)
        {
            renderer.OpenElement("pre");

            if (code is IFencedBlock fencedCodeBlock && fencedCodeBlock.Info != null)
            {
                var infoPrefix = (code.Parser as FencedCodeBlockParser)?.InfoPrefix ?? FencedCodeBlockParser.DefaultInfoPrefix;
                renderer.AddAttribute("class", string.Concat(infoPrefix, fencedCodeBlock.Info));
            }

            renderer.WriteLeafRawLines(code);
            renderer.CloseElement();
        }
    }
}
