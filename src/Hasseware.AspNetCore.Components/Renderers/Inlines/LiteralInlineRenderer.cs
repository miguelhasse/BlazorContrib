using Markdig.Syntax.Inlines;

namespace Hasseware.Markdig.Renderers.Inlines
{
    internal class LiteralInlineRenderer : BlazorObjectRenderer<LiteralInline>
    {
        protected override void Write(BlazorRenderer renderer, LiteralInline literalInline)
        {
            renderer.Write(literalInline.Content);
        }
    }
}
