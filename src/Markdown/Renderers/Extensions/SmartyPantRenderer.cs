using Markdig.Extensions.SmartyPants;

namespace Hasseware.Markdig.Renderers.Extensions
{
    internal class SmartyPantRenderer : BlazorObjectRenderer<SmartyPant>
    {
        private static readonly SmartyPantOptions _options = new SmartyPantOptions();

        protected override void Write(BlazorRenderer renderer, SmartyPant smartyPant)
        {
            if (_options.Mapping.TryGetValue(smartyPant.Type, out string text))
            {
                renderer.AddMarkupContent(text);
            }
        }
    }
}
