using Markdig.Extensions.Mathematics;

namespace Hasseware.Markdig.Renderers.Extensions
{
    internal class MathBlockRenderer : BlazorObjectRenderer<MathBlock>
    {
        protected override void Write(BlazorRenderer renderer, MathBlock math)
        {
            renderer.OpenElement("div");
            renderer.WriteAttributes(math);
            renderer.AddContent("\\[");
            renderer.WriteLeafRawLines(math);
            renderer.AddContent("\\]");
            renderer.CloseElement();
        }
    }
}
