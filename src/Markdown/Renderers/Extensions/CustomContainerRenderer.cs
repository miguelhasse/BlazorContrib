using Markdig.Extensions.CustomContainers;

namespace Hasseware.Markdig.Renderers.Extensions
{
    internal class CustomContainerRenderer : BlazorObjectRenderer<CustomContainer>
    {
        protected override void Write(BlazorRenderer renderer, CustomContainer container)
        {
            renderer.OpenElement("div");
            renderer.WriteAttributes(container);
            renderer.WriteChildren(container);
            renderer.CloseElement();
        }
    }
}
