using Hasseware.Markdig.Renderers;
using Markdig;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.RenderTree;
using System;
using System.Diagnostics;
using System.Text;
using MarkdownParser = Markdig.Markdown;

namespace Hasseware.AspNetCore.Components
{
    public class Markdown : ComponentBase
    {
        [Parameter]
        public RenderFragment ChildContent { get; set; }

        [Parameter]
        public string Extensions { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "BL0006:Do not use RenderTree types", Justification = "Reviewed")]
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

            Debug.WriteLine(sb.ToString());

            var pipeline  = new MarkdownPipelineBuilder()
                .UseAdvancedExtensions()
                .Configure(Extensions)
                .Build();
            
            MarkdownParser.Convert(sb.ToString(), new BlazorRenderer(builder, 0), pipeline);
        }
    }
}
