# Hasseware.FluentUI.AspNetCore.Components.Forms

This project adapts the base dynamic forms package to `Microsoft.FluentUI.AspNetCore.Components`.

Use it when you want `DynamicFormFields` from the forms library to emit Fluent UI controls instead of the default Blazor input components.

## What it includes

- `FluentFormFieldProvider` chooses Fluent UI editors and validation components for generated fields.
- `FluentDateTime<TValue>` adds date and date-time input support for Fluent forms.
- `FluentEnumSelect<TEnum>` renders enum values with a Fluent UI selector.
- `FluentUrlOrGuid<TValue>` supports `string`, `Uri`, and `Guid`-style values through a Fluent input.

The project references `Hasseware.AspNetCore.Components.Forms`, so it is designed to be used alongside the base forms abstractions.

## Usage

Register `FluentFormFieldProvider` as the active `IDynamicFormFieldProvider`, then use `DynamicFormFields` as usual:

```csharp
using Hasseware.AspNetCore.Components.Forms;
using Hasseware.FluentUI.AspNetCore.Components.Forms;

builder.Services.AddSingleton<IDynamicFormFieldProvider, FluentFormFieldProvider>();
```

```razor
@using Hasseware.AspNetCore.Components.Forms

<EditForm Model="@model">
    <DynamicFormFields PresentationLayer="FluentUI" />
</EditForm>
```

## Fluent-specific mapping

`FluentFormFieldProvider` applies validation and metadata from common annotations:

- `DisplayAttribute` contributes labels, prompts, descriptions, and order.
- `RequiredAttribute` and `KeyAttribute` mark fields as required when editable.
- `StringLengthAttribute`, `MinLengthAttribute`, and `MaxLengthAttribute` are passed through as input constraints.
- `RangeAttribute` is translated into `Min` and `Max` values where supported.
- `UIHintAttribute` can be used to select a specific Fluent editor for a presentation layer.

## When to use this package

Choose this project if:

- your application already uses `Microsoft.FluentUI.AspNetCore.Components`
- you want dynamic forms without hand-authoring each Fluent field
- you need the specialized `FluentDateTime`, `FluentEnumSelect`, or `FluentUrlOrGuid` components directly

If you want generated fields with standard Blazor components instead, use the base package in `src\Forms`.
