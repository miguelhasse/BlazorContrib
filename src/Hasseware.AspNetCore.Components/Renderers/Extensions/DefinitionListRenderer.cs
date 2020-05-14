using Markdig.Extensions.DefinitionLists;

namespace Hasseware.Markdig.Renderers.Extensions
{
    internal class DefinitionListRenderer : BlazorObjectRenderer<DefinitionList>
    {
        protected override void Write(BlazorRenderer renderer, DefinitionList list)
        {
            renderer.OpenElement("dl");
            renderer.WriteAttributes(list);

            foreach (var item in list)
            {
                var definitionItem = (DefinitionItem)item;

                for (int i = 0; i < definitionItem.Count; i++)
                {
                    var definitionTermOrContent = definitionItem[i];

                    if (definitionTermOrContent is DefinitionTerm definitionTerm)
                    {
                        renderer.OpenElement("dt");
                        renderer.WriteAttributes(definitionTerm);
                        renderer.Write(definitionTerm.Inline);
                        renderer.CloseElement();
                    }
                    else
                    {
                        renderer.OpenElement("dd");
                        renderer.WriteAttributes(definitionTermOrContent);
                        renderer.Write(definitionTermOrContent);
                        renderer.CloseElement();
                    }
                }
            }

            renderer.CloseElement();
        }
    }
}
