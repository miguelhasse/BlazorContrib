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
                renderer.AddAttribute("href", $"#fnref:{link.Index}");
                renderer.AddContent("&#8617;");
            }
            else
            {
                var order = link.Footnote.Order;
                renderer.AddAttribute("id", $"#fnref:{link.Index}");
                renderer.AddAttribute("class", "footnote-ref");
                renderer.AddAttribute("href", $"#fn:{order}");
                renderer.AddMarkupContent($"<sup>{order}</sup>");
            }

            renderer.CloseElement();
        }
    }
}
