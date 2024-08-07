namespace Hasseware.AspNetCore.Components.Forms;

public interface IDynamicFormFieldProvider
{
    (Type ComponentType, IDictionary<string, object>? AdditonalAttributes) GetEditorType(DynamicFormField field);

    (Type ComponentType, IDictionary<string, object>? AdditonalAttributes) GetValidationType(DynamicFormField field);
}
