using Markdig.Extensions.Figures;

namespace Hasseware.Markdig.Renderers.Extensions
{
    internal class FigureCaptionRenderer : BlazorObjectRenderer<FigureCaption>
    {
        protected override void Write(BlazorRenderer renderer, FigureCaption caption)
        {
            renderer.OpenElement("figcaption");
            renderer.WriteAttributes(caption);
            renderer.Write(caption.Inline);
            renderer.CloseElement();
        }
    }
}
