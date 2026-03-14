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

## Sample app

A runnable sample is included in `samples\BlazorMarkdown.csproj`.

Start it from the repository root with:

```powershell
dotnet run --project .\samples\BlazorMarkdown.csproj
```

The sample pages demonstrate several extension combinations, including emoji, advanced Markdown features, tables, footnotes, and mathematics.
