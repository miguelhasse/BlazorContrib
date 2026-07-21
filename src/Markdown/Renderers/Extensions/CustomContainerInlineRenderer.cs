using Markdig.Extensions.CustomContainers;

namespace Hasseware.Markdig.Renderers.Extensions
{
    internal class CustomContainerInlineRenderer : BlazorObjectRenderer<CustomContainerInline>
    {
        protected override void Write(BlazorRenderer renderer, CustomContainerInline container)
        {
            renderer.OpenElement("span");
            renderer.WriteAttributes(container);
            renderer.WriteChildren(container);
            renderer.CloseElement();
        }
    }
}
