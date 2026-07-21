using Hasseware.Markdig.Renderers;
using Markdig.Syntax;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace Hasseware.MarkdownTests;

/// <summary>
/// Minimal test-only host component that renders a hand-built <see cref="MarkdownObject"/> directly
/// through the internal <see cref="BlazorRenderer"/>, bypassing the Markdig parser and pipeline entirely.
/// This lets tests exercise renderers for AST node types that cannot be reached through the public
/// <c>Markdown</c> component's <c>Extensions</c> string (e.g. JiraLinks, which Markdig's
/// <c>MarkdownExtensions.Configure(string)</c> does not support activating).
/// </summary>
internal sealed class TestMarkdownHost : ComponentBase
{
    [Parameter]
    public MarkdownObject? Node { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var renderer = new BlazorRenderer(builder, null!, 0);
        renderer.Write(Node!);
    }
}
