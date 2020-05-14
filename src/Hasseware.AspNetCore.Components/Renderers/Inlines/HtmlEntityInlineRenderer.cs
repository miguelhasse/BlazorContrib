using Markdig.Syntax.Inlines;

namespace Hasseware.Markdig.Renderers.Inlines
{
    internal class HtmlEntityInlineRenderer : BlazorObjectRenderer<HtmlEntityInline>
    {
        protected override void Write(BlazorRenderer renderer, HtmlEntityInline htmlEntityInline)
        {
            renderer.Write(htmlEntityInline.Transcoded);
        }
    }
}
