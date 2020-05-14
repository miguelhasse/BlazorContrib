using Markdig.Syntax.Inlines;

namespace Hasseware.Markdig.Renderers.Inlines
{
    internal class LineBreakInlineRenderer : BlazorObjectRenderer<LineBreakInline>
    {
        protected override void Write(BlazorRenderer renderer, LineBreakInline lineBreakInline)
        {
            if (lineBreakInline.IsHard)
            {
                renderer.OpenElement("br");
                renderer.CloseElement();
            }
        }
    }
}
