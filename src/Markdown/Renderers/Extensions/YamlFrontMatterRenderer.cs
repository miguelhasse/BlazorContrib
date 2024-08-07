using Markdig.Extensions.Yaml;

namespace Hasseware.Markdig.Renderers.Extensions
{
    internal class YamlFrontMatterRenderer : BlazorObjectRenderer<YamlFrontMatterBlock>
    {
        protected override void Write(BlazorRenderer renderer, YamlFrontMatterBlock yaml)
        {
            renderer.OpenElement("div");
            renderer.AddAttribute("class", "yaml");
            renderer.WriteLeafRawLines(yaml);
            renderer.CloseElement();
        }
    }
}
