using Markdig.Syntax.Inlines;

namespace Hasseware.Markdig.Renderers.Inlines
{
    internal class AutolinkInlineRenderer : BlazorObjectRenderer<AutolinkInline>
    {
        protected override void Write(BlazorRenderer renderer, AutolinkInline autolink)
        {
            renderer.OpenElement("a");
            renderer.AddAttribute("href", autolink.IsEmail ? $"mailto:{autolink.Url}" : autolink.Url);
            renderer.WriteAttributes(autolink);
            renderer.CloseElement();
        }
    }
}
