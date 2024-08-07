
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics;
using System.Globalization;
using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;

namespace Hasseware.FluentUI.AspNetCore.Components.Forms;

[CascadingTypeParameter(nameof(TValue))]
public partial class FluentUrlOrGuid<TValue> : FluentInputBase<TValue>
{
    public FluentUrlOrGuid()
    {
        var type = Nullable.GetUnderlyingType(typeof(TValue)) ?? typeof(TValue);

        if (type != typeof(string) && type != typeof(Uri) && type != typeof(Guid))
        {
            throw new InvalidOperationException($"Unsupported {GetType()} type param '{type}'.");
        }
    }

    [Parameter] public string ParsingErrorMessage { get; set; } = string.Empty;

    /// <inheritdoc />
    protected override string FormatValueAsString(TValue? value)
    {
        return value switch
        {
			string str => str,
			Uri uri => uri.ToString(),
			Guid guid => guid.ToString(),
            _ => string.Empty,// Handles null for Nullable<DateTime>, etc.
        };
    }

    /// <inheritdoc />
    protected override bool TryParseValueFromString(string? value, [MaybeNullWhen(false)] out TValue result, [NotNullWhen(false)] out string? validationErrorMessage)
    {
        var targetType = typeof(TValue);

        bool success;
		if (targetType == typeof(string))
		{
			success = TryParseString(value, out result);
		}
		else if (targetType == typeof(Uri))
		{
			success = TryParseUri(value, out result);
		}
		else if(targetType == typeof(Guid) || targetType == typeof(Guid?))
        {
            success = Guid.TryParse(value, out var guid);
            result = success ? (TValue)(object)guid : default;
        }
        else
        {
            throw new InvalidOperationException($"The type '{targetType}' is not a supported date type.");
        }

        if (success)
        {
            Debug.Assert(result is not null);
            validationErrorMessage = null;
            return true;
        }
        else
        {
            validationErrorMessage = string.Format(CultureInfo.CurrentCulture, ParsingErrorMessage, FieldIdentifier.FieldName);
            return false;
        }
    }

	private static bool TryParseString(string? value, out TValue? result)
	{
		if (value is not null)
		{
			result = (TValue)(object)value;
			return true;
		}

		result = default;
		return false;
	}

	private static bool TryParseUri(string? value, out TValue? result)
	{
		if (Uri.TryCreate(value, UriKind.RelativeOrAbsolute, out var uri))
		{
			result = (TValue)(object)uri;
			return true;
		}

		result = default;
		return false;
	}
}
