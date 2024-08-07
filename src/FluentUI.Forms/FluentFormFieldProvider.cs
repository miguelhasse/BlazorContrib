using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.FluentUI.AspNetCore.Components;
using Hasseware.AspNetCore.Components.Forms;

namespace Hasseware.FluentUI.AspNetCore.Components.Forms;

public class FluentFormFieldProvider : IDynamicFormFieldProvider
{
    public (Type ComponentType, IDictionary<string, object>? AdditonalAttributes) GetEditorType(DynamicFormField field)
    {
        var parameters = new Dictionary<string, object>
        {
            { "Label", field.DisplayName },
            { "AriaLabel", field.DisplayName },
            { "Placeholder", field.Placeholder! },
            { "ReadOnly", field.ReadOnly },
            { "Disabled", field.Disabled },
        };

        foreach (var param in field.ControlParameters.Where(f => f.Value != null))
        {
            parameters[param.Key] = param.Value!;
        }

        if (parameters["Disabled"] is false 
            && (field.Property.GetCustomAttribute<RequiredAttribute>() is not null
            || field.Property.GetCustomAttribute<KeyAttribute>() is not null))
        {
            parameters["Required"] = true;
        }

        var editorTypeName = field.Property.GetCustomAttributes<EditorAttribute>()
            .Where(attr => attr.EditorBaseTypeName == typeof(FluentInputBase<>).AssemblyQualifiedName
                && (field.UIHint == null || attr.EditorTypeName.StartsWith(field.UIHint)))
            .FirstOrDefault()?.EditorTypeName;

        // Infer the editor based on the property type and other annotations
        if (editorTypeName is null && field.PropertyType == typeof(bool))
        {
            return (typeof(FluentCheckbox), parameters);
        }

        var stringLength = field.Property.GetCustomAttribute<StringLengthAttribute>();
        var minlength = stringLength?.MinimumLength ?? field.Property.GetCustomAttribute<MinLengthAttribute>()?.Length;
        var maxlength = stringLength?.MaximumLength ?? field.Property.GetCustomAttribute<MaxLengthAttribute>()?.Length;

        if (minlength is not null)
        {
            parameters["Minlength"] = minlength;
        }
        if (maxlength is not null)
        {
            parameters["Maxlength"] = maxlength;
        }

        if (field.Property.GetCustomAttribute<RangeAttribute>() is {} range)
        {
            if (range.Minimum is not null)
            {
                parameters["Min"] = range.Minimum.ToString()!;
            }
            if (range.Maximum is not null)
            {
                parameters["Max"] = range.Maximum.ToString()!;
            }
        }

        if (editorTypeName is not null && Type.GetType(editorTypeName, false, true) is var editorType && editorType is not null)
        {
            return (editorType, null);
        }

        if (field.PropertyType == typeof(string))
        {
            var dataType = field.Property.GetCustomAttribute<DataTypeAttribute>();

            if (dataType is not null)
            {
                if (dataType.DataType == DataType.Date)
                {
                    return (typeof(FluentDatePicker), parameters);
                }
                if (dataType.DataType == DataType.DateTime)
                {
                    return (typeof(FluentDateTime<>).MakeGenericType(field.PropertyType), parameters);
                }
                if (dataType.DataType == DataType.EmailAddress)
                {
                    parameters[nameof(TextFieldType)] = TextFieldType.Email;
                    return (typeof(FluentTextField), parameters);
                }
                if (dataType.DataType == DataType.MultilineText)
                {
                    return (typeof(FluentTextArea), parameters);
                }
                if (dataType.DataType == DataType.Password)
                {
                    parameters[nameof(TextFieldType)] = TextFieldType.Password;
                    return (typeof(FluentTextField), parameters);
                }
                if (dataType.DataType == DataType.PhoneNumber)
                {

                    parameters[nameof(TextFieldType)] = TextFieldType.Tel;
                    return (typeof(FluentTextField), parameters);
                }
                if (dataType.DataType == DataType.Time)
                {
                    return (typeof(FluentTimePicker), parameters);
                }
                if (dataType.DataType == DataType.Url)
                {
                    parameters[nameof(TextFieldType)] = TextFieldType.Url;
                    return (typeof(FluentTextField), parameters);
                }
            }

            return (typeof(FluentTextField), parameters);
        }

        var underlyingType = Nullable.GetUnderlyingType(field.PropertyType) ?? field.PropertyType;

        if (underlyingType == typeof(short))
        {
            return (typeof(FluentNumberField<>).MakeGenericType(field.PropertyType), parameters);
        }
        if (underlyingType == typeof(int))
        {
            return (typeof(FluentNumberField<>).MakeGenericType(field.PropertyType), parameters);
        }
        if (underlyingType == typeof(long))
        {
            return (typeof(FluentNumberField<>).MakeGenericType(field.PropertyType), parameters);
        }
        if (underlyingType == typeof(float))
        {
            return (typeof(FluentNumberField<>).MakeGenericType(field.PropertyType), parameters);
        }
        if (underlyingType == typeof(double))
        {
            return (typeof(FluentNumberField<>).MakeGenericType(field.PropertyType), parameters);
        }
        if (underlyingType == typeof(decimal))
        {
            return (typeof(FluentNumberField<>).MakeGenericType(field.PropertyType), parameters);
        }
        if (underlyingType == typeof(DateTime) || underlyingType == typeof(DateTimeOffset))
        {
            //TODO: Replace with fluent controls
            var dataType = field.Property.GetCustomAttribute<DataTypeAttribute>();

            if (dataType is not null)
            {
                parameters["Type"] = dataType.DataType switch
                {
                    DataType.Date => InputDateType.Date,
                    DataType.Time => InputDateType.Time,
                    _ => InputDateType.DateTimeLocal,
                };
            }

            return (typeof(FluentDateTime<>).MakeGenericType(field.PropertyType), parameters);
        }
        if (underlyingType == typeof(Guid))
        {
            return (typeof(FluentUrlOrGuid<>).MakeGenericType(field.PropertyType), parameters);
        }
        if (field.PropertyType == typeof(Uri))
        {
            return (typeof(FluentUrlOrGuid<Uri>), parameters);
        }
        if (field.PropertyType.IsEnum)
        {
            if (!field.PropertyType.IsDefined(typeof(FlagsAttribute), inherit: true))
                return (typeof(FluentEnumSelect<>).MakeGenericType(field.PropertyType), parameters);
        }

        return (typeof(FluentTextField), parameters);
    }

    public (Type ComponentType, IDictionary<string, object>? AdditonalAttributes) GetValidationType(DynamicFormField field)
    {
        return (typeof(FluentValidationMessage<>).MakeGenericType(field.PropertyType), null);
    }
}
