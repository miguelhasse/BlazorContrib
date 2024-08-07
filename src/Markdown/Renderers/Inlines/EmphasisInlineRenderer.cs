using Markdig.Syntax.Inlines;

namespace Hasseware.Markdig.Renderers.Inlines
{
    internal class EmphasisInlineRenderer : BlazorObjectRenderer<EmphasisInline>
    {
        protected override void Write(BlazorRenderer renderer, EmphasisInline emphasis)
        {
            var elementName = GetDefaultTag(emphasis);

            if (elementName != null)
            {
                renderer.OpenElement(elementName);
                renderer.WriteChildren(emphasis);
                renderer.CloseElement();
            }
            else
            {
                renderer.WriteChildren(emphasis);
            }
        }

        private string GetDefaultTag(EmphasisInline obj)
        {
            if (obj.DelimiterChar == '*' || obj.DelimiterChar == '_')
            {
                return obj.DelimiterCount == 2 ? "strong" : "em";
            }
            return null;
        }
    }
}
