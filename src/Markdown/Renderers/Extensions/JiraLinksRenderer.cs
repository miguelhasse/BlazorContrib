using Markdig.Extensions.JiraLinks;

namespace Hasseware.Markdig.Renderers.Extensions
{
    internal class JiraLinksRenderer : BlazorObjectRenderer<JiraLink>
    {
        protected override void Write(BlazorRenderer renderer, JiraLink link)
        {
            renderer.AddContent(string.Join('-', link.ProjectKey, link.Issue));
        }
    }
}

