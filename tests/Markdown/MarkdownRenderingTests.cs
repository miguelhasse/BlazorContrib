using Bunit;
using Markdig.Extensions.JiraLinks;
using Markdig.Helpers;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using MarkdownComponent = Hasseware.AspNetCore.Components.Markdown;

namespace Hasseware.MarkdownTests;

public class MarkdownRenderingTests : Bunit.BunitContext
{
    private string RenderMarkdown(string markdown, string? extensions = null)
    {
        var cut = Render<MarkdownComponent>(parameters => parameters
            .Add(p => p.Extensions, extensions!)
            .AddChildContent(markdown));

        return cut.Markup;
    }

    // --- Baseline CommonMark / advanced-extension smoke coverage ---

    [Fact]
    public void Heading_RendersCorrectTag()
    {
        Assert.Contains("<h2", RenderMarkdown("## Section title"));
    }

    [Fact]
    public void Paragraph_BoldAndItalic_RenderExpectedTags()
    {
        var html = RenderMarkdown("**bold** and *italic*");

        Assert.Contains("<strong>bold</strong>", html);
        Assert.Contains("<em>italic</em>", html);
    }

    [Fact]
    public void List_RendersUnorderedList()
    {
        var html = RenderMarkdown("- one\n- two", "advanced");

        Assert.Contains("<ul", html);
        Assert.Contains("<li>", html);
        Assert.Contains("one", html);
        Assert.Contains("two", html);
    }

    [Fact]
    public void Table_RendersTableElement()
    {
        var html = RenderMarkdown("| A | B |\n|---|---|\n| 1 | 2 |", "pipetables");

        Assert.Contains("<table>", html);
    }

    [Fact]
    public void TaskList_RendersCheckedAndUncheckedIndicators()
    {
        var html = RenderMarkdown("- [x] done\n- [ ] todo", "advanced");

        Assert.Contains("&#128505;", html);
        Assert.Contains("&#9744;", html);
    }

    [Fact]
    public void Link_RendersAnchorWithHref()
    {
        var html = RenderMarkdown("[text](https://example.com)");

        Assert.Contains("<a", html);
        Assert.Contains("href=\"https://example.com", html);
    }

    // --- Fix 1: EmphasisExtras / Citations tag mapping ---

    [Theory]
    [InlineData("~~strike~~", "del")]
    [InlineData("~sub~", "sub")]
    [InlineData("^sup^", "sup")]
    [InlineData("++insert++", "ins")]
    [InlineData("==mark==", "mark")]
    [InlineData("\"\"citation\"\"", "cite")]
    public void EmphasisExtras_And_Citations_RenderExpectedTag(string markdown, string expectedTag)
    {
        var html = RenderMarkdown(markdown, "advanced");

        Assert.Contains($"<{expectedTag}>", html);
        Assert.Contains($"</{expectedTag}>", html);
    }

    // --- Fix 2: GitHub-style alert blocks ---

    [Theory]
    [InlineData("NOTE")]
    [InlineData("TIP")]
    [InlineData("IMPORTANT")]
    [InlineData("WARNING")]
    [InlineData("CAUTION")]
    public void AlertBlock_RendersDivWithKindSpecificClassAndTitle(string kind)
    {
        var html = RenderMarkdown($"> [!{kind}]\n> Some message", "advanced");

        var expectedClass = $"markdown-alert markdown-alert-{kind.ToLowerInvariant()}";

        Assert.Contains($"class=\"{expectedClass}\"", html);
        Assert.Contains("markdown-alert-title", html);
        Assert.Contains(kind, html);
        Assert.Contains("Some message", html);
        Assert.DoesNotContain("<blockquote", html);
    }

    [Fact]
    public void PlainBlockquote_StillRendersAsBlockquote()
    {
        var html = RenderMarkdown("> just a quote, not an alert", "advanced");

        Assert.Contains("<blockquote", html);
        Assert.DoesNotContain("markdown-alert", html);
    }

    // --- Fix 3: code block structure ---

    [Fact]
    public void FencedCodeBlock_RendersPreCodeWithLanguageClassAndRealNewlines()
    {
        var html = RenderMarkdown("```csharp\nvar x = 1;\nvar y = 2;\n```", "advanced");

        Assert.Contains("<pre>", html);
        Assert.Contains("<code class=\"language-csharp\">", html);
        Assert.Contains("var x = 1;\nvar y = 2;", html);
        Assert.DoesNotContain("<br", html);
    }

    // --- Fix 4: JiraLinks hyperlink rendering ---
    // The JiraLinks extension cannot be activated through the public Extensions string (Markdig's
    // Configure(string) does not support it), so the AST node is constructed directly and rendered
    // through the internal BlazorRenderer via TestMarkdownHost.
    [Fact]
    public void JiraLink_RendersAsHyperlink()
    {
        var link = new JiraLink
        {
            ProjectKey = new StringSlice("PROJ"),
            Issue = new StringSlice("123"),
            Url = "https://mycompany.atlassian.net/browse/PROJ-123",
        };

        var cut = Render<TestMarkdownHost>(parameters => parameters
            .Add(p => p.Node, link));

        Assert.Contains("<a", cut.Markup);
        Assert.Contains("href=\"https://mycompany.atlassian.net/browse/PROJ-123\"", cut.Markup);
        Assert.Contains("PROJ-123", cut.Markup);
    }

    // --- Extensions parameter / pipeline-cache functional correctness ---

    [Fact]
    public void RenderingTwiceWithSameExtensions_ProducesCorrectOutputBothTimes()
    {
        const string markdown = "# Title\n\n**bold**";

        var first = RenderMarkdown(markdown, "pipetables");
        var second = RenderMarkdown(markdown, "pipetables");

        Assert.Equal(first, second);
        Assert.Contains("<h1", first);
        Assert.Contains("<strong>bold</strong>", first);
    }

    // --- SmartyPants ---

    [Fact]
    public void SmartyPants_ReplacesQuotesAndDashesWithTypographicEntities()
    {
        var html = RenderMarkdown("\"quoted\" -- text", "smartypants");

        Assert.Contains("&ldquo;", html);
        Assert.Contains("&rdquo;", html);
        Assert.Contains("&ndash;", html);
    }

    // --- CustomContainers ---

    [Fact]
    public void CustomContainerBlock_RendersDivWithClass()
    {
        var html = RenderMarkdown(":::spoiler\ncontent\n:::", "customcontainers");

        Assert.Contains("<div class=\"spoiler\"", html);
        Assert.Contains("content", html);
    }

    [Fact]
    public void CustomContainerInline_RendersSpanWithClass()
    {
        var html = RenderMarkdown("::text::{.highlight}", "customcontainers+attributes");

        Assert.Contains("<span class=\"highlight\"", html);
        Assert.Contains("text", html);
    }

    // --- HtmlBlockRenderer / HtmlInlineRenderer / HtmlEntityInlineRenderer ---

    [Fact]
    public void HtmlBlock_RendersRawHtmlPassthrough()
    {
        var html = RenderMarkdown("<div>raw</div>");

        Assert.Contains("<div>raw</div>", html);
    }

    [Fact]
    public void HtmlInline_RendersRawInlineHtml()
    {
        var html = RenderMarkdown("before <span>raw</span> after");

        Assert.Contains("<span>raw</span>", html);
    }

    [Fact]
    public void HtmlEntityInline_PreservesNamedEntity()
    {
        var html = RenderMarkdown("&amp;");

        // HtmlEntityInlineRenderer decodes the entity to its literal character and writes it as
        // markup content (unlike Markdig's own HtmlRenderer, which writes an HTML-escaped string).
        Assert.Contains("&", html);
    }

    // --- ThematicBreakRenderer ---

    [Fact]
    public void ThematicBreak_RendersHrElement()
    {
        var html = RenderMarkdown("para\n\n---\n\npara2");

        Assert.Contains("<hr", html);
    }

    // --- ListRenderer (ordered) ---

    [Fact]
    public void List_RendersOrderedList()
    {
        var html = RenderMarkdown("1. one\n2. two");

        Assert.Contains("<ol", html);
        Assert.Contains("<li>", html);
    }

    // --- AutolinkInlineRenderer ---

    [Fact]
    public void Autolink_RendersAnchorForBareUrl()
    {
        var html = RenderMarkdown("<https://example.com>");

        Assert.Contains("<a", html);
        Assert.Contains("href=\"https://example.com", html);
    }

    // --- CodeInlineRenderer ---

    [Fact]
    public void CodeInline_RendersCodeElement()
    {
        var html = RenderMarkdown("`inline code`");

        Assert.Contains("<code>inline code</code>", html);
    }

    // --- LinkInlineRenderer (image variant) ---

    [Fact]
    public void Image_RendersImgWithAltAndSrc()
    {
        var html = RenderMarkdown("![alt text](https://example.com/img.png)");

        Assert.Contains("<img", html);
        Assert.Contains("alt=\"alt text\"", html);
        Assert.Contains("src=\"https://example.com/img.png\"", html);
    }

    // --- LineBreakInlineRenderer ---

    [Fact]
    public void LineBreak_RendersBrForHardBreak()
    {
        var html = RenderMarkdown("line one  \nline two");

        Assert.Contains("<br", html);
    }

    // --- DelimiterInlineRenderer ---

    [Fact]
    public void UnmatchedEmphasisDelimiter_RendersAsLiteralText()
    {
        var html = RenderMarkdown("1 * 2");

        Assert.Contains("*", html);
        Assert.DoesNotContain("<em>", html);
    }

    // --- AbbreviationRenderer ---

    [Fact]
    public void Abbreviation_RendersAbbrWithTitle()
    {
        var html = RenderMarkdown("*[HTML]: Hypertext Markup Language\n\nUsing HTML here", "abbreviations");

        Assert.Contains("<abbr title=\"Hypertext Markup Language\">HTML</abbr>", html);
    }

    // --- DefinitionListRenderer ---

    [Fact]
    public void DefinitionList_RendersDlDtDd()
    {
        var html = RenderMarkdown("Term 1\n:   Definition text", "definitionlists");

        Assert.Contains("<dl", html);
        Assert.Contains("<dt>Term 1</dt>", html);
        Assert.Contains("<dd", html);
        Assert.Contains("Definition text", html);
    }

    // --- FigureRenderer / FigureCaptionRenderer ---

    [Fact]
    public void Figure_RendersFigureAndFigcaption()
    {
        var html = RenderMarkdown("^^^\nThis is a figure\n^^^ This is a *caption*", "figures");

        Assert.Contains("<figure", html);
        Assert.Contains("This is a figure", html);
        Assert.Contains("<figcaption", html);
        Assert.Contains("This is a <em>caption</em>", html);
    }

    // --- FooterBlockRenderer ---

    [Fact]
    public void Footer_RendersFooterElement()
    {
        var html = RenderMarkdown("^^ This is a footer\n^^ multi-line", "footers");

        Assert.Contains("<footer", html);
        Assert.Contains("This is a footer", html);
    }

    // --- FootnoteGroupRenderer / FootnoteLinkRenderer ---

    [Fact]
    public void Footnote_RendersReferenceAndGroupWithBackLink()
    {
        var html = RenderMarkdown("Text[^1]\n\n[^1]: Note text", "footnotes");

        Assert.Contains("class=\"footnote-ref\"", html);
        Assert.Contains("<sup>1</sup>", html);
        Assert.Contains("class=\"footnotes\"", html);
        Assert.Contains("class=\"footnote-back-ref\"", html);
        Assert.Contains("Note text", html);
    }

    // --- MathBlockRenderer / MathInlineRenderer ---

    [Fact]
    public void MathBlock_RendersDisplayMathDelimiters()
    {
        var html = RenderMarkdown("$$\nx^2\n$$", "mathematics");

        Assert.Contains("\\[", html);
        Assert.Contains("\\]", html);
        Assert.Contains("x^2", html);
    }

    [Fact]
    public void MathInline_RendersInlineMathDelimiters()
    {
        var html = RenderMarkdown("$x^2$", "mathematics");

        Assert.Contains("\\(", html);
        Assert.Contains("\\)", html);
        Assert.Contains("x^2", html);
    }

    // --- YamlFrontMatterRenderer ---

    [Fact]
    public void YamlFrontMatter_RendersHiddenYamlDiv()
    {
        var html = RenderMarkdown("---\ntitle: Test\n---\n\nBody text", "yaml");

        Assert.Contains("class=\"yaml\"", html);
        Assert.Contains("title: Test", html);
        Assert.Contains("Body text", html);
    }

    // --- BlazorRenderer.AddUriAttribute base-relative rewriting ---

    [Fact]
    public void Link_WithAbsoluteUriMatchingBaseUri_RendersBaseRelativeHref()
    {
        var navigationManager = Services.GetRequiredService<NavigationManager>();
        navigationManager.NavigateTo("articles/page1");

        var html = RenderMarkdown("[docs](http://localhost/articles/docs/readme.md)");

        Assert.Contains("<a", html);
        Assert.Contains("href=\"articles/docs/readme.md\"", html);
    }
}
