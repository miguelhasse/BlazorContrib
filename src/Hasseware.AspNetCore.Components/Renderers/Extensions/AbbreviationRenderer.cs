using Markdig.Extensions.Abbreviations;

namespace Hasseware.Markdig.Renderers.Extensions
{
    internal class AbbreviationRenderer : BlazorObjectRenderer<AbbreviationInline>
    {
        protected override void Write(BlazorRenderer renderer, AbbreviationInline abbreviation)
        {
            renderer.OpenElement("abbr");
            renderer.WriteAttribute("title", abbreviation.Abbreviation.Text);
            renderer.WriteAttributes(abbreviation);
            renderer.AddContent(abbreviation.Abbreviation.Label);
            renderer.CloseElement();
        }
    }
}
