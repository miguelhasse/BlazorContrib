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
                var kind = alert.Kind.AsSpan();
                Span<char> buffer = stackalloc char[kind.Length];

                kind.ToLowerInvariant(buffer);
                var cssClass = $"markdown-alert markdown-alert-{buffer}";

                kind.ToUpperInvariant(buffer);
                var title = buffer.ToString();

                renderer.OpenElement("div");
                renderer.AddAttribute("class", cssClass);

                renderer.OpenElement("p");
                renderer.AddAttribute("class", "markdown-alert-title");
                renderer.AddContent(title);
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
