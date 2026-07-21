using Markdig.Extensions.Alerts;
using Markdig.Syntax;

namespace Hasseware.Markdig.Renderers
{
    internal class QuoteBlockRenderer : BlazorObjectRenderer<QuoteBlock>
    {
        protected override void Write(BlazorRenderer renderer, QuoteBlock quote)
        {
            if (quote is AlertBlock alert)
            {
                var kind = alert.Kind.ToString().ToUpperInvariant();

                renderer.OpenElement("div");
                renderer.AddAttribute("class", $"markdown-alert markdown-alert-{kind.ToLowerInvariant()}");

                renderer.OpenElement("p");
                renderer.AddAttribute("class", "markdown-alert-title");
                renderer.AddContent(kind);
                renderer.CloseElement();

                renderer.WriteChildren(alert);
                renderer.CloseElement();
            }
            else
            {
                renderer.OpenElement("blockquote");
                renderer.WriteChildren(quote);
                renderer.CloseElement();
            }
        }
    }
}
