using Markdig.Syntax;

namespace Hasseware.Markdig.Renderers
{
    internal class HeadingRenderer : BlazorObjectRenderer<HeadingBlock>
    {
        protected override void Write(BlazorRenderer renderer, HeadingBlock heading)
        {
            string elementName = $"h{heading.Level}";

            renderer.OpenElement(elementName);
            renderer.WriteAttributes(heading);

            if (heading.ProcessInlines)
            {
                renderer.Write(heading.Inline);
            }
            else
            {
                renderer.WriteLeafRawLines(heading);
            }

            renderer.CloseElement();
        }
    }
}
