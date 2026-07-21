using Markdig.Extensions.JiraLinks;

namespace Hasseware.Markdig.Renderers.Extensions
{
    internal class JiraLinksRenderer : BlazorObjectRenderer<JiraLink>
    {
        protected override void Write(BlazorRenderer renderer, JiraLink link)
        {
            renderer.OpenElement("a");
            renderer.AddUriAttribute("href", link.Url);

            if (!string.IsNullOrEmpty(link.Title))
            {
                renderer.AddAttribute("title", link.Title);
            }

            renderer.WriteAttributes(link);
            renderer.AddContent(string.Join('-', link.ProjectKey, link.Issue));
            renderer.CloseElement();
        }
    }
}

