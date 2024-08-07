using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;

namespace Hasseware.FluentUI.AspNetCore.Components.Forms;

[CascadingTypeParameter(nameof(TEnum))]
public partial class FluentEnumSelect<TEnum> : FluentInputBase<TEnum>
{
	protected override bool TryParseValueFromString(string? value, [MaybeNullWhen(false)] out TEnum result, [NotNullWhen(false)] out string? validationErrorMessage)
    {
        // Let's Blazor convert the value for us 😊
        if (BindConverter.TryConvertTo(value, CultureInfo.CurrentCulture, out TEnum? parsedValue))
        {
            result = parsedValue!;
            validationErrorMessage = "";
            return true;
        }

        // Map null/empty value to null if the bound object is nullable
        if (string.IsNullOrEmpty(value))
        {
            var nullableType = Nullable.GetUnderlyingType(typeof(TEnum));
            if (nullableType != null)
            {
                result = default!;
                validationErrorMessage = "";
                return true;
            }
        }

        // The value is invalid => set the error message
        result = default;
        validationErrorMessage = $"The {FieldIdentifier.FieldName} field is not valid.";
        return false;
    }

    // Get the display text for an enum value:
    // - Use the DisplayAttribute if set on the enum member, so this support localization
    // - Fallback on Humanizer to decamelize the enum member name
    private static string? GetDisplayName(object? value)
    {
        if (value is null)
            return null;

        // Read the Display attribute name
        var valueAsString = value.ToString();

        if (valueAsString is not null)
        {
            var member = value.GetType().GetMember(valueAsString)[0];
            return member.GetCustomAttribute<DisplayAttribute>()?.GetName();
        }

		return valueAsString;
    }

    // Get the actual enum type. It unwrap Nullable<T> if needed
    // MyEnum  => MyEnum
    // MyEnum? => MyEnum
    private static Type GetEnumType() => Nullable.GetUnderlyingType(typeof(TEnum)) ?? typeof(TEnum);
}