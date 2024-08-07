using Markdig.Syntax;

namespace Hasseware.Markdig.Renderers
{
    internal class ListRenderer : BlazorObjectRenderer<ListBlock>
    {
        protected override void Write(BlazorRenderer renderer, ListBlock list)
        {
            renderer.OpenElement(list.IsOrdered ? "ol" : "ul");

            if (list.BulletType != '1')
            {
                renderer.AddAttribute("type", list.BulletType);
            }

            if (list.OrderedStart != null && (list.OrderedStart != "1"))
            {
                renderer.AddAttribute("start", list.OrderedStart);
            }

            foreach (var item in list)
            {
                renderer.OpenElement("li");
                var listItem = (ListItemBlock)item;
                renderer.WriteChildren(listItem);
                renderer.CloseElement();
            }

            renderer.CloseElement();
        }
    }
}
