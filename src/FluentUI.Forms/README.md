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

## Real-world examples and edge cases

### `Required` is inferred, not just copied from annotations

`FluentFormFieldProvider` only sets `Required="true"` when the field is **not** `Disabled` and
is annotated with `RequiredAttribute` or `KeyAttribute`. A disabled required field will not show
as required in the UI, which matters for wizard-style forms where some fields are conditionally
disabled:

```csharp
[Display(Name = "Order number", Order = 1)]
[Required, Key]
public string? OrderNumber { get; set; }
```

```razor
<EditForm Model="@model">
    <DynamicFormFields Disabled="@isReadOnlyMode" PresentationLayer="FluentUI" />
</EditForm>
```

When `isReadOnlyMode` is `true`, the `OrderNumber` field renders disabled and without the
required indicator, even though the property itself is still `[Required]`.

### Combining length constraints on the same property

`StringLengthAttribute` takes precedence over `MinLengthAttribute`/`MaxLengthAttribute` when
both are present; the provider prefers `StringLengthAttribute.MinimumLength`/`MaximumLength` and
only falls back to the separate attributes if `StringLengthAttribute` is absent:

```csharp
[Display(Name = "Nickname")]
[StringLength(20, MinimumLength = 2)]
public string? Nickname { get; set; }

[Display(Name = "Bio")]
[MinLength(0), MaxLength(500)]
public string? Bio { get; set; }
```

Both examples map to a `FluentTextField`/`FluentTextArea` with `Minlength`/`Maxlength`
parameters populated from whichever attribute supplied the value.

### Selecting a specific Fluent editor with `UIHintAttribute`

Combine `UIHintAttribute` with an `EditorAttribute` targeting `FluentInputBase<>` to pick a
particular Fluent component for a presentation layer without writing a custom provider:

```csharp
[Display(Name = "Country")]
[UIHint("FluentUI")]
[Editor(typeof(FluentCountryPicker), typeof(FluentInputBase<>))]
public string? CountryCode { get; set; }
```

The provider only honors the `EditorAttribute` whose `EditorTypeName` starts with the given
`UIHint`, so the same property could resolve to a different custom editor for a different
presentation layer.

### `[Flags]` enums fall back to a plain text field

As with the base Forms package, `FluentFormFieldProvider` skips `FluentEnumSelect<TEnum>` for
enums marked `[Flags]` and falls back to `FluentTextField`. If you need a multi-select editor
for bit-combined flag values, register a custom `IDynamicFormFieldProvider` (or a `UIHint` +
`EditorAttribute` override as shown above) that recognizes `[Flags]` enum types.
