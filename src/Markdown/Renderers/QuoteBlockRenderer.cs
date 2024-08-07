using Markdig.Syntax;

namespace Hasseware.Markdig.Renderers
{
    internal class QuoteBlockRenderer : BlazorObjectRenderer<QuoteBlock>
    {
        protected override void Write(BlazorRenderer renderer, QuoteBlock quote)
        {
            renderer.OpenElement("blockquote");
            renderer.WriteChildren(quote);
            renderer.CloseElement();
        }
    }
}
