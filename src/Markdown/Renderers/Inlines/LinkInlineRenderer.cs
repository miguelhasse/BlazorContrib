using Markdig.Syntax.Inlines;
using System.Text;

namespace Hasseware.Markdig.Renderers.Inlines
{
    internal class LinkInlineRenderer : BlazorObjectRenderer<LinkInline>
    {
        protected override void Write(BlazorRenderer renderer, LinkInline link)
        {
            var url = link.GetDynamicUrl != null ? link.GetDynamicUrl() ?? link.Url : link.Url;

            if (link.IsImage)
            {
                renderer.OpenElement("img");
                renderer.AddAttribute("src", url);
                renderer.AddAttribute("alt", GetPlainText(link));
            }
            else
            {
                renderer.OpenElement("a");
                renderer.AddUriAttribute("href", url);
            }

            if (!string.IsNullOrEmpty(link.Title))
            {
                renderer.AddAttribute("title", link.Title);
            }

            renderer.WriteAttributes(link);

            if (!link.IsImage)
            {
                renderer.WriteChildren(link);
            }

            renderer.CloseElement();
        }

        // Images are void elements: their child inlines don't render as markup, they only supply
        // the plain-text "alt" attribute value (mirroring Markdig's own HtmlRenderer, which
        // temporarily disables inline HTML rendering while writing an image's children).
        private static string GetPlainText(ContainerInline container)
        {
            var builder = new StringBuilder();
            AppendPlainText(builder, container);
            return builder.ToString();
        }

        private static void AppendPlainText(StringBuilder builder, ContainerInline container)
        {
            foreach (var inline in container)
            {
                switch (inline)
                {
                    case LiteralInline literal:
                        builder.Append(literal.Content.AsSpan());
                        break;
                    case CodeInline code:
                        builder.Append(code.Content);
                        break;
                    case HtmlEntityInline entity:
                        builder.Append(entity.Transcoded.AsSpan());
                        break;
                    case LineBreakInline:
                        builder.Append(' ');
                        break;
                    case ContainerInline childContainer:
                        AppendPlainText(builder, childContainer);
                        break;
                }
            }
        }
    }
}
