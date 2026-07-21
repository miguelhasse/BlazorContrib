using Bunit;
using Markdig.Extensions.JiraLinks;
using Markdig.Helpers;
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
        var html = RenderMarkdown("- one\n- two");

        Assert.Contains("<ul", html);
        Assert.Contains("<li>", html);
        Assert.Contains("one", html);
        Assert.Contains("two", html);
    }

    [Fact]
    public void Table_RendersTableElement()
    {
        var html = RenderMarkdown("| A | B |\n|---|---|\n| 1 | 2 |");

        Assert.Contains("<table>", html);
    }

    [Fact]
    public void TaskList_RendersCheckedAndUncheckedIndicators()
    {
        var html = RenderMarkdown("- [x] done\n- [ ] todo");

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
        var html = RenderMarkdown(markdown);

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
        var html = RenderMarkdown($"> [!{kind}]\n> Some message");

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
        var html = RenderMarkdown("> just a quote, not an alert");

        Assert.Contains("<blockquote", html);
        Assert.DoesNotContain("markdown-alert", html);
    }

    // --- Fix 3: code block structure ---

    [Fact]
    public void FencedCodeBlock_RendersPreCodeWithLanguageClassAndRealNewlines()
    {
        var html = RenderMarkdown("```csharp\nvar x = 1;\nvar y = 2;\n```");

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
}
