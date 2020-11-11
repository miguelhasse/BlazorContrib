using Markdig.Extensions.TaskLists;

namespace Hasseware.Markdig.Renderers.Extensions
{
    internal class TaskListRenderer : BlazorObjectRenderer<TaskList>
    {
        protected override void Write(BlazorRenderer renderer, TaskList list)
        {
            renderer.AddContent(list.Checked ? "[x]" : "[ ]");
        }
    }
}
