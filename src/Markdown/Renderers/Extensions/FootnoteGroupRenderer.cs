using Markdig.Extensions.Footnotes;

namespace Hasseware.Markdig.Renderers.Extensions
{
    internal class FootnoteGroupRenderer : BlazorObjectRenderer<FootnoteGroup>
    {
        protected override void Write(BlazorRenderer renderer, FootnoteGroup footnotes)
        {
            renderer.OpenElement("div");
            renderer.AddAttribute("class", "footnotes");
            renderer.AddMarkupContent("<hr />");
            renderer.OpenElement("ol");

            for (int i = 0; i < footnotes.Count; i++)
            {
                var footnote = (Footnote)footnotes[i];
                renderer.OpenElement("li");
                renderer.AddAttribute("id", $"fn:{footnote.Order}");
                renderer.WriteChildren(footnote);
                renderer.CloseElement();
            }

            renderer.CloseElement();
            renderer.CloseElement();
        }
    }
}
