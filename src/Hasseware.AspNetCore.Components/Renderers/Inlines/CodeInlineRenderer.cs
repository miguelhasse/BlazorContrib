﻿using Markdig.Syntax.Inlines;

namespace Hasseware.Markdig.Renderers.Inlines
{
    internal class CodeInlineRenderer : BlazorObjectRenderer<CodeInline>
    {
        protected override void Write(BlazorRenderer renderer, CodeInline code)
        {
            renderer.OpenElement("pre");
            renderer.AddContent(code.Content);
            renderer.CloseElement();
        }
    }
}
