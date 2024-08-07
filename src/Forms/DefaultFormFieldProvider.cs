using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Microsoft.AspNetCore.Components.Forms;

namespace Hasseware.AspNetCore.Components.Forms;

public class DefaultFormFieldProvider : IDynamicFormFieldProvider
{
    public (Type ComponentType, IDictionary<string, object>? AdditonalAttributes) GetEditorType(DynamicFormField field)
    {
        foreach (var editorAttribute in field.Property.GetCustomAttributes<EditorAttribute>())
        {
            if (editorAttribute.EditorBaseTypeName == typeof(InputBase<>).AssemblyQualifiedName)
                return (Type.GetType(editorAttribute.EditorTypeName, throwOnError: true)!, null);
        }

        // Infer the editor based on the property type and other annotations
        if (field.PropertyType == typeof(bool))
            return (typeof(InputCheckbox), null);

        if (field.PropertyType == typeof(string))
        {
            var dataType = field.Property.GetCustomAttribute<DataTypeAttribute>();

            if (dataType is not null)
            {
                if (dataType.DataType == DataType.Date)
                    return (typeof(InputText), new Dictionary<string, object> { { "type", "date" } });

                if (dataType.DataType == DataType.DateTime)
                    return (typeof(InputText), new Dictionary<string, object> { { "type", "datetime-local" } });

                if (dataType.DataType == DataType.EmailAddress)
                    return (typeof(InputText), new Dictionary<string, object> { { "type", "email" } });

                if (dataType.DataType == DataType.MultilineText)
                    return (typeof(InputTextArea), null);

                if (dataType.DataType == DataType.Password)
                    return (typeof(InputText), new Dictionary<string, object> { { "type", "password" } });

                if (dataType.DataType == DataType.PhoneNumber)
                    return (typeof(InputText), new Dictionary<string, object> { { "type", "tel" } });

                if (dataType.DataType == DataType.Time)
                    return (typeof(InputText), new Dictionary<string, object> { { "type", "time" } });

                if (dataType.DataType == DataType.Url)
                    return (typeof(InputText), new Dictionary<string, object> { { "type", "url" } });
            }

            return (typeof(InputText), null);
        }

        var underlyingType = Nullable.GetUnderlyingType(field.PropertyType) ?? field.PropertyType;

        if (underlyingType == typeof(short))
            return (typeof(InputNumber<>).MakeGenericType(field.PropertyType), null);

        if (underlyingType == typeof(int))
            return (typeof(InputNumber<>).MakeGenericType(field.PropertyType), null);

        if (underlyingType == typeof(long))
            return (typeof(InputNumber<>).MakeGenericType(field.PropertyType), null);

        if (underlyingType == typeof(float))
            return (typeof(InputNumber<>).MakeGenericType(field.PropertyType), null);

        if (underlyingType == typeof(double))
            return (typeof(InputNumber<>).MakeGenericType(field.PropertyType), null);

        if (underlyingType == typeof(decimal))
            return (typeof(InputNumber<>).MakeGenericType(field.PropertyType), null);

        if (underlyingType == typeof(DateTime) || underlyingType == typeof(DateTimeOffset))
        {
            var dataType = field.Property.GetCustomAttribute<DataTypeAttribute>();
            if (dataType is not null && dataType.DataType == DataType.Date)
                return (typeof(InputDate<>).MakeGenericType(field.PropertyType), null);

            return (typeof(InputDateTime<>).MakeGenericType(field.PropertyType), null);
        }

        if (underlyingType == typeof(Guid))
            return (typeof(InputUrlOrGuid<>).MakeGenericType(field.PropertyType), null);

        if (field.PropertyType == typeof(Uri))
            return (typeof(InputUrlOrGuid<Uri>), null);

        if (field.PropertyType.IsEnum)
        {
            if (!field.PropertyType.IsDefined(typeof(FlagsAttribute), inherit: true))
                return (typeof(InputEnumSelect<>).MakeGenericType(field.PropertyType), null);
        }

        return (typeof(InputText), null);
    }

    public (Type ComponentType, IDictionary<string, object>? AdditonalAttributes) GetValidationType(DynamicFormField field)
    {
        return (typeof(ValidationMessage<>).MakeGenericType(field.PropertyType), null);
    }
}
