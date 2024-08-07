using Markdig.Renderers;
using Markdig.Syntax;

namespace Hasseware.Markdig.Renderers
{
    internal abstract class BlazorObjectRenderer<TObject> : MarkdownObjectRenderer<BlazorRenderer, TObject> where TObject : MarkdownObject
    {
    }
}
