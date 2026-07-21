using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Rendering;
using Hasseware.AspNetCore.Components.Forms;

namespace Hasseware.FluentUI.AspNetCore.Components.Forms.Tests;

/// <summary>
/// bUnit's <c>AddChildContent&lt;TChildComponent&gt;</c> helper does not support components whose
/// <c>ChildContent</c> parameter is itself generic (like <see cref="EditForm.ChildContent"/>, a
/// <see cref="RenderFragment{EditContext}"/>), so this helper builds the render tree manually.
/// </summary>
internal static class EditFormTestHost
{
    public static RenderFragment Render(object model, RenderFragment<EditContext> childContent) => builder =>
    {
        builder.OpenComponent<EditForm>(0);
        builder.AddAttribute(1, "Model", model);
        builder.AddAttribute(2, "ChildContent", childContent);
        builder.CloseComponent();
    };

    public static RenderFragment<EditContext> DynamicFormFields(
        bool requireDisplayAnnotation = true,
        bool skipReadOnly = true,
        bool readOnly = false,
        bool disabled = false,
        RenderFragment<DynamicFormField>? fieldTemplate = null) => _ => builder =>
    {
        builder.OpenComponent<Hasseware.AspNetCore.Components.Forms.DynamicFormFields>(0);
        builder.AddAttribute(1, "RequireDisplayAnnotation", requireDisplayAnnotation);
        builder.AddAttribute(2, "SkipReadOnly", skipReadOnly);
        builder.AddAttribute(3, "ReadOnly", readOnly);
        builder.AddAttribute(4, "Disabled", disabled);

        if (fieldTemplate != null)
            builder.AddAttribute(5, "FieldTemplate", fieldTemplate);

        builder.CloseComponent();
    };
}
