# BlazorMarkdown sample

This project is a small Blazor Server app that demonstrates the Markdown component from `Hasseware.AspNetCore.Components.Markdown`.

It references the library project directly and exposes a few pages with progressively richer Markdown examples.

## Pages

- `/` shows a compact example with fenced code blocks, tables, task lists, and emoji support.
- `/sample1` exercises headings, blockquotes, lists, inline code, and links.
- `/sample2` demonstrates more advanced content including tables, definition lists, footnotes, images, and mathematics.

## Real-world scenarios per page

- `/` is the best starting point for the common "docs page" scenario: task lists and code
  blocks are typical for changelogs or contributor guides, and emoji shortcodes show how the
  `Extensions` string can be extended beyond `UseAdvancedExtensions()`.
- `/sample1` covers link- and quote-heavy content such as blog posts or release notes, and is a
  good reference for the indentation-trimming behavior described in the Markdown package README
  when content is nested inside other Razor markup.
- `/sample2` is the edge-case showcase: definition lists, footnotes, and math together
  demonstrate the custom renderers under `Renderers/Extensions` and how multiple Markdig
  extensions can be combined in a single `Extensions` string (see the Markdown package README's
  "Combining footnotes, math, and YAML front matter" section).

## Run the sample

From the solution root:

```powershell
dotnet run --project .\samples\BlazorMarkdown.csproj
```

Then browse to the routes above to compare the rendered output with the inline Markdown source in `Pages\Index.razor`, `Pages\Sample1.razor`, and `Pages\Sample2.razor`.

## Project structure

- `Pages` contains the Markdown examples.
- `wwwroot` contains static assets used by the sample.
- `Program.cs` and `Startup.cs` configure the Blazor Server host.
