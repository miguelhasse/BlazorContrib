using Hasseware.Markdig.Renderers;
using Markdig;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.RenderTree;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using MarkdownParser = Markdig.Markdown;

namespace Hasseware.AspNetCore.Components
{
    public class Markdown : ComponentBase
    {
        // MarkdownPipeline is immutable once built and expensive to construct (it initializes every
        // registered extension), so pipelines are cached and reused per distinct Extensions value.
        private static readonly ConcurrentDictionary<string, MarkdownPipeline> _pipelineCache = new();

        [Parameter]
        public RenderFragment ChildContent { get; set; }

        [Parameter]
        public string Extensions { get; set; }

        [Inject]
        private NavigationManager NavigationManager { get; set; }

        [SuppressMessage("Usage", "BL0006:Do not use RenderTree types", Justification = "Reviewed")]
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            var childBuilder = new RenderTreeBuilder();
            ChildContent(childBuilder);

            var frames = childBuilder.GetFrames().Array;
            var sb = new StringBuilder();

            for (int n = 0; n < frames.Length; n++)
            {
                if (frames[n] is RenderTreeFrame renderFrame)
                {
                    switch (renderFrame.FrameType)
                    {
                        case RenderTreeFrameType.Text:
                            sb.Append(renderFrame.TextContent);
                            break;
                        case RenderTreeFrameType.Markup:
                            sb.Append(renderFrame.MarkupContent);
                            break;
                    }
                }
            }

            // Remove whitespaces at the beginning of each new line
            for (int n = 0, i = 0, step = Environment.NewLine.Length; n + step < sb.Length; n++, i++)
            {
                if (sb[n] != Environment.NewLine[i])
                {
                    i = 0;
                    continue;
                }

                if (i + 1 == step)
                {
                    var pos = n;
                    while (sb[pos + 1] == ' ') pos++;

                    i = 0;
                    sb.Remove(n + 1, pos - n);
                }
            }

            var pipeline = _pipelineCache.GetOrAdd(Extensions ?? string.Empty, static extensions =>
                new MarkdownPipelineBuilder().UseAdvancedExtensions().Configure(extensions).Build());

            MarkdownParser.Convert(sb.ToString(), new BlazorRenderer(builder, NavigationManager, 0), pipeline);
        }
    }
}
