using Markdig.Extensions.TaskLists;

namespace Hasseware.Markdig.Renderers.Extensions
{
    internal class TaskListRenderer : BlazorObjectRenderer<TaskList>
    {
        protected override void Write(BlazorRenderer renderer, TaskList list)
        {
            renderer.OpenElement("span");
            renderer.AddMarkupContent(list.Checked ? "&#128505;" : "&#9744;");
            renderer.CloseElement();
        }
    }
}
