using Markdig.Syntax.Inlines;

namespace Hasseware.Markdig.Renderers.Inlines
{
    internal class DelimiterInlineRenderer : BlazorObjectRenderer<DelimiterInline>
    {
        protected override void Write(BlazorRenderer renderer, DelimiterInline delimiter)
        {
            if (delimiter.Type != DelimiterType.Close)
            {
                renderer.AddMarkupContent(delimiter.ToLiteral());
            }

            renderer.WriteChildren(delimiter);

            if (delimiter.Type == DelimiterType.Close)
            {
                renderer.AddMarkupContent(delimiter.ToLiteral());
            }
        }
    }
}
