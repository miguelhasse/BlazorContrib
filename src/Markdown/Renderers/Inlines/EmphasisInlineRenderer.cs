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
            switch (obj.DelimiterChar)
            {
                case '*':
                case '_':
                    return obj.DelimiterCount == 2 ? "strong" : "em";

                // EmphasisExtras: ~~strike~~ / ~sub~
                case '~':
                    return obj.DelimiterCount == 2 ? "del" : "sub";

                // EmphasisExtras: ^sup^
                case '^':
                    return "sup";

                // EmphasisExtras: ++insert++
                case '+':
                    return "ins";

                // EmphasisExtras: ==mark==
                case '=':
                    return "mark";

                // Citations: ""citation""
                case '"':
                    return "cite";

                default:
                    return null;
            }
        }
    }
}
