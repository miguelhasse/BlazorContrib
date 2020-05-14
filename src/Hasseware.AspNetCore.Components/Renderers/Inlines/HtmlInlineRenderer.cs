using Markdig.Syntax.Inlines;

namespace Hasseware.Markdig.Renderers.Inlines
{
    internal class HtmlInlineRenderer : BlazorObjectRenderer<HtmlInline>
    {
        protected override void Write(BlazorRenderer renderer, HtmlInline inline)
        {
            renderer.AddMarkupContent(inline.Tag);
        }
    }
}
