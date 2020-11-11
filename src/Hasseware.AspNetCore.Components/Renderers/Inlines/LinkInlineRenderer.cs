using Markdig.Syntax.Inlines;

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
            renderer.WriteChildren(link);
            renderer.CloseElement();
        }
    }
}
