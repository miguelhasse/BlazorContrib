# BlazorContrib

`BlazorContrib` is a small collection of reusable Blazor components and helpers focused on common UI building blocks.

The solution currently contains three libraries:

- `Hasseware.AspNetCore.Components.Forms` for reflection-based form generation from annotated models.
- `Hasseware.FluentUI.AspNetCore.Components.Forms` for Fluent UI field rendering and input components that plug into the dynamic forms package.
- `Hasseware.AspNetCore.Components.Markdown` for rendering Markdown content as Blazor render tree output by using Markdig.

It also includes a sample app under `samples` that demonstrates the Markdown component in a running Blazor Server application.

## Projects

| Project | Path | Purpose |
| --- | --- | --- |
| Forms | `src\Forms` | Generates editors and validation messages from an `EditForm` model and its `DataAnnotations`. |
| Fluent UI Forms | `src\FluentUI.Forms` | Provides Fluent UI-based field selection and specialized components for the forms library. |
| Markdown | `src\Markdown` | Converts Markdown child content into Blazor-rendered output with configurable Markdig extensions. |
| BlazorMarkdown | `samples` | Runs the Markdown component against example pages. |

Each project directory now contains its own `README.md` with package-specific details and examples.

## Getting started

Restore and build the solution from the repository root:

```powershell
dotnet restore
dotnet build BlazorContrib.sln
```

To run the included sample application:

```powershell
dotnet run --project .\samples\BlazorMarkdown.csproj
```

## Highlights

### Dynamic forms

The forms library inspects the current `EditForm` model, creates a field list from public properties, and picks editors based on property type plus attributes such as `DisplayAttribute`, `DataTypeAttribute`, `EditableAttribute`, `EditorAttribute`, `RangeAttribute`, and `UIHintAttribute`.

By default, `DynamicFormFields` only includes properties marked with `DisplayAttribute`, which helps keep generated forms explicit and model-driven.

### Fluent UI integration

The Fluent UI package builds on the base forms abstractions and swaps the default editor selection with Fluent components such as text fields, number fields, date pickers, checkboxes, and validation messages.

### Markdown rendering

The Markdown component accepts Markdown content as `ChildContent`, parses it with Markdig, and renders the result directly as Blazor UI instead of emitting raw HTML strings. The `Extensions` parameter is passed through to Markdig's pipeline configuration so features can be enabled as needed.

## Notes

- The libraries currently target `.NET 10.0`.
- The sample app demonstrates the Markdown package today; the forms packages are library-focused building blocks.
