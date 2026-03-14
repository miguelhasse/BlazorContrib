# Hasseware.AspNetCore.Components.Forms

This project provides dynamic form generation helpers for Blazor `EditForm` components.

Its main goal is to reduce repetitive field markup by inspecting a model's public properties and creating editors and validation messages from metadata already expressed with `DataAnnotations`.

## Main pieces

- `DynamicFormFields` renders a field list for the current `EditContext`.
- `DynamicFormField` represents an individual generated field and exposes editor and validation fragments.
- `DefaultFormFieldProvider` chooses an editor component based on property type and annotations.
- `RecursiveAnnotationsValidator` adds recursive validation support for nested object graphs.
- `InputDateTime<TValue>`, `InputEnumSelect<TEnum>`, and `InputUrlOrGuid<TValue>` cover input cases not handled by the built-in Blazor components.

## Supported field inference

The default provider maps common CLR types and attributes to components:

- `bool` -> `InputCheckbox`
- `string` -> `InputText` or `InputTextArea`, with `DataTypeAttribute` used for `date`, `datetime-local`, `email`, `password`, `tel`, `time`, and `url`
- Numeric types -> `InputNumber<TValue>`
- `DateTime` and `DateTimeOffset` -> `InputDate<TValue>` or `InputDateTime<TValue>`
- `Guid` and `Uri` -> `InputUrlOrGuid<TValue>`
- Enums -> `InputEnumSelect<TEnum>`

You can override the chosen editor with `EditorAttribute` or supply a custom `IDynamicFormFieldProvider`.

## Usage

`DynamicFormFields` must be placed inside an `EditForm` so it can access the cascading `EditContext`.

```razor
@using System.ComponentModel.DataAnnotations
@using Hasseware.AspNetCore.Components.Forms

<EditForm Model="@model">
    <DataAnnotationsValidator />
    <DynamicFormFields />
</EditForm>

@code {
    private Person model = new();

    private sealed class Person
    {
        [Display(Name = "Full name", Prompt = "Ada Lovelace", Order = 1)]
        public string? Name { get; set; }

        [Display(Name = "Birthday", Order = 2)]
        [DataType(DataType.Date)]
        public DateTime? BirthDate { get; set; }

        [Display(Name = "Favorite color", Order = 3)]
        public ConsoleColor FavoriteColor { get; set; }
    }
}
```

## Behavior notes

- `DisplayAttribute` is required by default. Set `RequireDisplayAnnotation="false"` to include all supported public properties.
- Read-only properties are skipped by default. Set `SkipReadOnly="false"` to include them.
- `FieldTemplate` lets you control the surrounding layout while still reusing the generated editor and validation fragments.
- `OnModelChanged` fires when a generated field updates the model.

## Custom provider registration

If you want to replace the default editor selection, register your own `IDynamicFormFieldProvider` in DI:

```csharp
using Hasseware.AspNetCore.Components.Forms;

builder.Services.AddSingleton<IDynamicFormFieldProvider, MyFormFieldProvider>();
```

When no provider is registered, `DynamicFormFields` falls back to `DefaultFormFieldProvider`.
