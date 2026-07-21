using Hasseware.Markdig.Renderers.Extensions;
using Hasseware.Markdig.Renderers.Inlines;
using Markdig.Helpers;
using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Syntax;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System.Diagnostics.CodeAnalysis;

namespace Hasseware.Markdig.Renderers
{
    internal class BlazorRenderer : RendererBase
    {
        // All the renderer instances below are stateless (they only take the BlazorRenderer and the
        // MarkdownObject being written as parameters), so a single shared set is reused across every
        // BlazorRenderer instance instead of allocating ~25 new objects on every component render.
        private static readonly IMarkdownObjectRenderer[] _sharedRenderers =
        [
            new CodeBlockRenderer(),
            new ListRenderer(),
            new HeadingRenderer(),
            new HtmlBlockRenderer(),
            new ParagraphRenderer(),
            new QuoteBlockRenderer(),
            new ThematicBreakRenderer(),

            // Default inline renderers
            new AutolinkInlineRenderer(),
            new CodeInlineRenderer(),
            new DelimiterInlineRenderer(),
            new EmphasisInlineRenderer(),
            new LineBreakInlineRenderer(),
            new HtmlInlineRenderer(),
            new HtmlEntityInlineRenderer(),
            new LinkInlineRenderer(),
            new LiteralInlineRenderer(),

            //Extension renderers
            new AbbreviationRenderer(),
            new DefinitionListRenderer(),
            new FigureCaptionRenderer(),
            new FigureRenderer(),
            new FooterBlockRenderer(),
            new FootnoteGroupRenderer(),
            new FootnoteLinkRenderer(),
            new JiraLinksRenderer(),
            new MathBlockRenderer(),
            new MathInlineRenderer(),
            new TableRenderer(),
            new TaskListRenderer(),
            new YamlFrontMatterRenderer(),
        ];

        private readonly RenderTreeBuilder _builder;
        private readonly NavigationManager _navigation;
        private int _sequence;

        public BlazorRenderer(RenderTreeBuilder builder, NavigationManager navigation, int sequence)
        {
            this._builder = builder;
            this._navigation = navigation;
            this._sequence = sequence;

            foreach (var renderer in _sharedRenderers)
                ObjectRenderers.Add(renderer);
        }

        public override object Render(MarkdownObject markdownObject)
        {
            OpenElement("div");
            AddAttribute("class", "markdown-body");
            Write(markdownObject);
            CloseElement();

            return this._builder;
        }

        public void OpenElement(string elementName) => this._builder.OpenElement(this._sequence++, elementName);

        public void CloseElement() => this._builder.CloseElement();

        public void AddAttribute(string name, string value) => this._builder.AddAttribute(this._sequence++, name, value);

        public void AddAttribute(string name, bool value) => this._builder.AddAttribute(this._sequence++, name, value);

        public void AddAttribute(string name, object value) => this._builder.AddAttribute(this._sequence++, name, value);

        public void AddContent(string textContent) => this._builder.AddContent(this._sequence++, textContent);

        public void AddContent(object textContent) => this._builder.AddContent(this._sequence++, textContent);

        public void AddMarkupContent(string markupContent) => this._builder.AddMarkupContent(this._sequence++, markupContent);

        [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Reviewed")]
        public void AddUriAttribute(string name, string value)
        {
            if (_navigation != null
                && Uri.TryCreate(_navigation.Uri, UriKind.Absolute, out Uri currentUri)
                && Uri.TryCreate(currentUri, value, out Uri finalUri))
            {
                try { value = _navigation.ToBaseRelativePath(finalUri.AbsoluteUri); }
                catch { }
            }

            this._builder.AddAttribute(this._sequence++, name, value);
        }

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
