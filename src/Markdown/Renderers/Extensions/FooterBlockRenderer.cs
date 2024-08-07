using Markdig.Extensions.Footers;

namespace Hasseware.Markdig.Renderers.Extensions
{
    internal class FooterBlockRenderer : BlazorObjectRenderer<FooterBlock>
    {
        protected override void Write(BlazorRenderer renderer, FooterBlock footer)
        {
            renderer.OpenElement("footer");
            renderer.WriteAttributes(footer);
            renderer.WriteChildren(footer);
            renderer.CloseElement();
        }
    }
}
