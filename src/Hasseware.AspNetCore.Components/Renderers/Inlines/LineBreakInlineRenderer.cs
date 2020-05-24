using Markdig.Syntax.Inlines;

namespace Hasseware.Markdig.Renderers.Inlines
{
    internal class LineBreakInlineRenderer : BlazorObjectRenderer<LineBreakInline>
    {
        public bool RenderAsHardlineBreak { get; set; }

        protected override void Write(BlazorRenderer renderer, LineBreakInline lineBreakInline)
        {
            if (lineBreakInline.IsHard || RenderAsHardlineBreak)
            {
                renderer.OpenElement("br");
                renderer.CloseElement();
            }
            else
            {
                renderer.AddContent(" ");
            }
        }
    }
}
