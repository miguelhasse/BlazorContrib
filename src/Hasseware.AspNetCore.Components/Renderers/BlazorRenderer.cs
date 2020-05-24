using Hasseware.Markdig.Renderers.Extensions;
using Hasseware.Markdig.Renderers.Inlines;
using Markdig.Helpers;
using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Syntax;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Linq;

namespace Hasseware.Markdig.Renderers
{
    public class BlazorRenderer : RendererBase
    {
        private readonly RenderTreeBuilder _builder;
        private int _sequence;

        public BlazorRenderer(RenderTreeBuilder builder, int sequence)
        {
            this._builder = builder;
            this._sequence = sequence;

            ObjectRenderers.Add(new CodeBlockRenderer());
            ObjectRenderers.Add(new ListRenderer());
            ObjectRenderers.Add(new HeadingRenderer());
            ObjectRenderers.Add(new HtmlBlockRenderer());
            ObjectRenderers.Add(new ParagraphRenderer());
            ObjectRenderers.Add(new QuoteBlockRenderer());
            ObjectRenderers.Add(new ThematicBreakRenderer());

            // Default inline renderers
            ObjectRenderers.Add(new AutolinkInlineRenderer());
            ObjectRenderers.Add(new CodeInlineRenderer());
            ObjectRenderers.Add(new DelimiterInlineRenderer());
            ObjectRenderers.Add(new EmphasisInlineRenderer());
            ObjectRenderers.Add(new LineBreakInlineRenderer());
            ObjectRenderers.Add(new HtmlInlineRenderer());
            ObjectRenderers.Add(new HtmlEntityInlineRenderer());
            ObjectRenderers.Add(new LinkInlineRenderer());
            ObjectRenderers.Add(new LiteralInlineRenderer());

            //Extension renderers
            ObjectRenderers.Add(new AbbreviationRenderer());
            ObjectRenderers.Add(new DefinitionListRenderer());
            ObjectRenderers.Add(new FigureCaptionRenderer());
            ObjectRenderers.Add(new FigureRenderer());
            ObjectRenderers.Add(new FooterBlockRenderer());
            ObjectRenderers.Add(new FootnoteGroupRenderer());
            ObjectRenderers.Add(new FootnoteLinkRenderer());
            ObjectRenderers.Add(new JiraLinksRenderer());
            ObjectRenderers.Add(new MathBlockRenderer());
            ObjectRenderers.Add(new MathInlineRenderer());
            ObjectRenderers.Add(new TableRenderer());
            ObjectRenderers.Add(new TaskListRenderer());
            ObjectRenderers.Add(new YamlFrontMatterRenderer());
        }

        public override object Render(MarkdownObject markdownObject)
        {
            OpenElement("div");
            AddAttribute("class", "markdown-body");
            Write(markdownObject);
            CloseElement();

            return this._builder;
        }

        internal void AddAttribute(string name, string value) => this._builder.AddAttribute(this._sequence++, name, value);

        internal void AddAttribute(string name, bool value) => this._builder.AddAttribute(this._sequence++, name, value);

        internal void AddAttribute(string name, object value) => this._builder.AddAttribute(this._sequence++, name, value);

        internal void AddContent(string textContent) => this._builder.AddContent(this._sequence++, textContent);

        internal void AddContent(object textContent) => this._builder.AddContent(this._sequence++, textContent);

        internal void AddMarkupContent(string markupContent) => this._builder.AddMarkupContent(this._sequence++, markupContent);

        internal void CloseElement() => this._builder.CloseElement();

        internal void OpenElement(string elementName) => this._builder.OpenElement(this._sequence++, elementName);

        public BlazorRenderer Write(StringSlice slice)
        {
            if (slice.Start <= slice.End)
            {
                var value = slice.Text.AsSpan(slice.Start, slice.End - slice.Start + 1);
                AddMarkupContent(new string(value));
            }

            return this;
        }

        public BlazorRenderer WriteAttribute(string name, StringSlice slice)
        {
            if (slice.Start <= slice.End)
            {
                var value = slice.Text.AsSpan(slice.Start, slice.End - slice.Start + 1);
                AddAttribute(name, new string(value));
            }

            return this;
        }

        public BlazorRenderer WriteAttributes(MarkdownObject markdownObject, Func<string, string> classFilter = null)
        {
            var attributes = markdownObject.TryGetAttributes();

            if (attributes != null)
            {
                if (attributes.Id != null)
                {
                    AddAttribute("id", attributes.Id);
                }

                if (attributes.Classes != null && attributes.Classes.Count > 0)
                {
                    var @class = string.Join(' ', attributes.Classes.Select(s => classFilter != null ? classFilter(s) : s));
                    AddAttribute("class", @class);
                }

                if (attributes.Properties != null && attributes.Properties.Count > 0)
                {
                    foreach (var property in attributes.Properties)
                    {
                        AddAttribute(property.Key, property.Value);
                    }
                }
            }

            return this;
        }

        public void WriteLeafRawLines(LeafBlock leafBlock)
        {
            if (leafBlock.Lines.Lines != null)
            {
                for (int n = 0; n < leafBlock.Lines.Count; n++)
                {
                    if (n > 0)
                    {
                        OpenElement("br");
                        CloseElement();
                    }

                    this.Write(leafBlock.Lines.Lines[n].Slice);
                }
            }
        }
    }
}
