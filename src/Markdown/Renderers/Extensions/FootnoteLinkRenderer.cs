using Markdig.Extensions.Footnotes;

namespace Hasseware.Markdig.Renderers.Extensions
{
    internal class FootnoteLinkRenderer : BlazorObjectRenderer<FootnoteLink>
    {
        protected override void Write(BlazorRenderer renderer, FootnoteLink link)
        {
            renderer.OpenElement("a");

            if (link.IsBackLink)
            {
                renderer.AddAttribute("class", "footnote-back-ref");
                renderer.AddUriAttribute("href", $"#fnref:{link.Index}");
                renderer.AddMarkupContent("&#8617;");
            }
            else
            {
                var order = link.Footnote.Order;
                renderer.AddAttribute("id", $"fnref:{link.Index}");
                renderer.AddAttribute("class", "footnote-ref");
                renderer.AddUriAttribute("href", $"#fn:{order}");
                renderer.AddMarkupContent($"<sup>{order}</sup>");
            }

            renderer.CloseElement();
        }
    }
}
