using Markdig.Syntax;

namespace Hasseware.Markdig.Renderers
{
    internal class ThematicBreakRenderer : BlazorObjectRenderer<ThematicBreakBlock>
    {
        protected override void Write(BlazorRenderer renderer, ThematicBreakBlock thematicBreak)
        {
            renderer.OpenElement("hr");
            renderer.CloseElement();
        }
    }
}
