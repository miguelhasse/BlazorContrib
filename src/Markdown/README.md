# Hasseware.AspNetCore.Components.Markdown

This project provides a Blazor `Markdown` component that renders Markdown content directly into a Blazor render tree.

Internally it uses `Markdig` to parse the Markdown source and a custom renderer to translate the parsed document into Blazor output.

## Usage

Wrap Markdown text inside the component and optionally enable Markdig extension sets through the `Extensions` parameter:

```razor
@using Hasseware.AspNetCore.Components

<Markdown Extensions="advanced+emojis">
    # Hello from Markdown

    This component supports **formatted text**, tables, lists, links, and
    other Markdig features enabled by the selected extension set.
</Markdown>
```

## How it works

- Markdown is supplied through `ChildContent`.
- The component reads the child render fragments, reconstructs the Markdown source, and trims indentation at the start of each new line.
- A `MarkdownPipelineBuilder` is configured with `UseAdvancedExtensions()` plus the optional `Extensions` string.
- The parsed output is rendered through `BlazorRenderer`, which produces Blazor content instead of raw HTML strings.

## Useful scenarios

- Rendering embedded Markdown in Razor components
- Showing documentation or help text stored inline with a component
- Reusing Markdig extensions while keeping output in Blazor's rendering model

## Real-world examples and edge cases

### Indentation from Razor markup is trimmed automatically

Because the Markdown source is written inline inside a Razor component, it usually carries the
surrounding code's indentation. `Markdown` reconstructs the child render fragment's text and
strips leading spaces at the start of every line, so this renders identically whether the
content is indented four spaces or not at all:

```razor
<div class="card">
    <Markdown>
        ## Card title

        This paragraph is indented in the source file, but the leading
        whitespace on each line is removed before parsing, so Markdig sees
        clean, left-aligned Markdown.
    </Markdown>
</div>
```

This makes it safe to nest `<Markdown>` blocks inside other components without worrying about
accidental code-block formatting from indentation.

### Enabling and disabling individual Markdig extensions

`UseAdvancedExtensions()` is always applied first, then the `Extensions` parameter is passed to
`MarkdownPipelineBuilder.Configure(...)`, which accepts a `+`/`-` delimited extension string.
Use `+` to add an extension on top of the advanced set, and `-` to turn one off:

```razor
<Markdown Extensions="emojis+pipetables-hardlinebreak">
    :rocket: Emoji shortcodes and pipe tables are enabled, and single
    line breaks are no longer converted to `<br>` tags.
</Markdown>
```

### Combining footnotes, math, and YAML front matter

The custom renderers under `Renderers/Extensions` (`FootnoteGroupRenderer`,
`FootnoteLinkRenderer`, `MathBlockRenderer`, `MathInlineRenderer`, `YamlFrontMatterRenderer`,
etc.) let you mix advanced content types in a single block:

```razor
<Markdown Extensions="advanced+mathematics+yaml">
    ---
    title: Physics notes
    ---

    Einstein's mass-energy equivalence is $$E = mc^2$$.[^1]

    [^1]: Originally published in 1905.
</Markdown>
```

### Relative links resolve against the current page

`BlazorRenderer` receives the app's `NavigationManager` (injected into `Markdown`), so relative
links and images in the Markdown source (for example `[docs](../docs/readme.md)`) resolve
against the current Blazor page URI instead of the raw Markdown file location — useful when the
same Markdown content is rendered from different routes.

## Sample app

A runnable sample is included in `samples\BlazorMarkdown.csproj`.

Start it from the repository root with:

```powershell
dotnet run --project .\samples\BlazorMarkdown.csproj
```

The sample pages demonstrate several extension combinations, including emoji, advanced Markdown features, tables, footnotes, and mathematics.
