using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.FluentUI.AspNetCore.Components;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;

namespace Hasseware.FluentUI.AspNetCore.Components.Forms;

[CascadingTypeParameter(nameof(TEnum))]
public partial class FluentEnumSelect<TEnum> : FluentInputBase<TEnum> where TEnum : Enum
{
    // The value -> display-name mapping is fixed for a given closed TEnum type, so it is computed
    // once (via this per-closed-generic-type static field) instead of reflecting on every render.
    private static readonly Lazy<IReadOnlyDictionary<object, string?>> _displayNames = new(BuildDisplayNames);

    protected override bool TryParseValueFromString(string? value, [MaybeNullWhen(false)] out TEnum result, [NotNullWhen(false)] out string? validationErrorMessage)
    {
        // Let's Blazor convert the value for us 😊
        if (BindConverter.TryConvertTo(value, CultureInfo.CurrentCulture, out TEnum? parsedValue))
        {
            result = parsedValue!;
            validationErrorMessage = string.Empty;
            return true;
        }

        // Map null/empty value to null if the bound object is nullable
        if (string.IsNullOrEmpty(value))
        {
            var nullableType = Nullable.GetUnderlyingType(typeof(TEnum));
            if (nullableType != null)
            {
                result = default!;
                validationErrorMessage = string.Empty;
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

        return _displayNames.Value.TryGetValue(value, out var displayName) ? displayName : value.ToString();
    }

    // Build the value -> display-name map once per closed TEnum type
    private static IReadOnlyDictionary<object, string?> BuildDisplayNames()
    {
        var enumType = GetEnumType();
        var map = new Dictionary<object, string?>();

        foreach (var value in Enum.GetValues(enumType))
        {
            var valueAsString = value.ToString();
            string? displayName = valueAsString;

            if (valueAsString is not null && enumType.GetMember(valueAsString) is [var member, ..])
            {
                displayName = member.GetCustomAttribute<DisplayAttribute>()?.GetName() ?? displayName;
            }

            map[value] = displayName;
        }

        return map;
    }

    // Get the actual enum type. It unwrap Nullable<T> if needed
    // MyEnum  => MyEnum
    // MyEnum? => MyEnum
    private static Type GetEnumType() => Nullable.GetUnderlyingType(typeof(TEnum)) ?? typeof(TEnum);
}