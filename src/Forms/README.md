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

## Real-world examples and edge cases

### Validating nested objects and collections

`RecursiveAnnotationsValidator` is a drop-in replacement for `DataAnnotationsValidator` that
walks the object graph: it revalidates every non-string, non-value-type public property, and
if that property is enumerable, every item inside it. This is useful for order/line-item style
models where child objects carry their own `DataAnnotations`.

```razor
<EditForm Model="@order">
    <RecursiveAnnotationsValidator />
    <DynamicFormFields />
</EditForm>

@code {
    private Order order = new();

    private sealed class Order
    {
        [Display(Name = "Customer", Order = 1)]
        [Required]
        public string? CustomerName { get; set; }

        [Display(Name = "Shipping address", Order = 2)]
        public Address ShippingAddress { get; set; } = new();

        public List<LineItem> Items { get; set; } = new();
    }

    private sealed class Address
    {
        [Required, Display(Name = "Street")]
        public string? Street { get; set; }
    }

    private sealed class LineItem
    {
        [Range(1, 100, ErrorMessage = "Quantity must be between 1 and 100.")]
        public int Quantity { get; set; }
    }
}
```

Validation errors reported for nested members surface as dotted field names (for example
`ShippingAddress.Street`), and per-field edits are revalidated individually as the user types
thanks to `EditContext.OnFieldChanged`.

### `[Flags]` enums fall back to plain text

`DefaultFormFieldProvider` only maps enums to `InputEnumSelect<TEnum>` when they are **not**
decorated with `[Flags]`. A `[Flags]` enum property renders as a plain `InputText` instead,
since there is no built-in multi-select editor for bit-combined values. If you need a proper
editor for flag enums, provide a custom `IDynamicFormFieldProvider` (or an `EditorAttribute`
override, see below) that recognizes `[Flags]` types and returns your own component.

### Overriding the editor for a single property

Use `EditorAttribute` with `EditorBaseTypeName` set to `typeof(InputBase<>).AssemblyQualifiedName`
to force a specific property to use a custom input component instead of the inferred one:

```csharp
[Display(Name = "Favorite color", Order = 3)]
[Editor(typeof(ColorPickerInput), typeof(InputBase<>))]
public string? FavoriteColorHex { get; set; }
```

`DefaultFormFieldProvider` checks for this attribute before falling back to its built-in type
and annotation inference, so it takes precedence for that property only.

### Nullable value types

Nullable numeric and date properties (`int?`, `decimal?`, `DateTime?`, `DateTimeOffset?`) are
supported directly: the provider resolves the underlying type with `Nullable.GetUnderlyingType`
to pick the right editor (`InputNumber<TValue>`, `InputDate<TValue>`, or
`InputDateTime<TValue>`), while still constructing the generic component over the original
nullable property type so binding round-trips `null` correctly.

### `ReadOnly`, `Disabled`, and `SkipReadOnly` are independent controls

These three settings solve different problems and can be combined:

- `SkipReadOnly="true"` (the default) removes properties without a public setter from the
  generated field list entirely — useful for computed/read-only display properties.
- `ReadOnly="true"` keeps a property in the field list but renders its editor as read-only
  (the user can see but not change the value through that editor).
- `Disabled="true"` renders the editor as disabled, which also affects `Required` inference in
  the Fluent UI provider (see the Fluent UI Forms README).

Set `SkipReadOnly="false"` if you want read-only CLR properties to still appear (for example,
to show a calculated total next to editable fields) while leaving `ReadOnly`/`Disabled` unset.
