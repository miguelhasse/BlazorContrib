using Markdig.Extensions.Figures;

namespace Hasseware.Markdig.Renderers.Extensions
{
    internal class FigureRenderer : BlazorObjectRenderer<Figure>
    {
        protected override void Write(BlazorRenderer renderer, Figure figure)
        {
            renderer.OpenElement("figure");
            renderer.WriteAttributes(figure);
            renderer.WriteChildren(figure);
            renderer.CloseElement();
        }
    }
}
