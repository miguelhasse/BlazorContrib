using Markdig.Syntax;

namespace Hasseware.Markdig.Renderers
{
    internal class HeadingRenderer : BlazorObjectRenderer<HeadingBlock>
    {
        protected override void Write(BlazorRenderer renderer, HeadingBlock heading)
        {
            string elementName = $"h{heading.Level - 1}";

            renderer.OpenElement(elementName);
            renderer.WriteAttributes(heading);
            renderer.WriteLeafRawLines(heading);
            renderer.CloseElement();
        }
    }
}
