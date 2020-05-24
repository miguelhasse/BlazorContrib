using Markdig.Extensions.Mathematics;

namespace Hasseware.Markdig.Renderers.Extensions
{
    internal class MathInlineRenderer : BlazorObjectRenderer<MathInline>
    {
        protected override void Write(BlazorRenderer renderer, MathInline math)
        {
            renderer.OpenElement("span");
            renderer.WriteAttributes(math);
            renderer.AddContent("\\(");
            renderer.Write(math.Content);
            renderer.AddContent("\\)");
            renderer.CloseElement();
        }
    }
}
