using Markdig.Syntax;

namespace Hasseware.Markdig.Renderers
{
    internal class HtmlBlockRenderer : BlazorObjectRenderer<HtmlBlock>
    {
        protected override void Write(BlazorRenderer renderer, HtmlBlock html)
        {
            renderer.WriteLeafRawLines(html);
        }
    }
}
