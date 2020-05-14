using Markdig.Syntax;

namespace Hasseware.Markdig.Renderers
{
    internal class ParagraphRenderer : BlazorObjectRenderer<ParagraphBlock>
    {
        protected override void Write(BlazorRenderer renderer, ParagraphBlock paragraph)
        {
            if (!paragraph.IsOpen)
            {
                renderer.OpenElement("p");
            }

            if (paragraph.ProcessInlines)
            {
                renderer.Write(paragraph.Inline);
            }
            else
            {
                renderer.WriteLeafRawLines(paragraph);
            }

            if (!paragraph.IsOpen)
            {
                renderer.CloseElement();
            }
        }
    }
}
