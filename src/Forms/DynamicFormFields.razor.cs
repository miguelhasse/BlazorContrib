using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components;

namespace Hasseware.AspNetCore.Components.Forms;

public partial class DynamicFormFields : OwningComponentBase
{
    internal string BaseEditorId { get; } = "form-field-" + Guid.NewGuid().ToString("n");

    internal object Model => CurrentEditContext.Model;

    protected IEnumerable<DynamicFormField>? Fields { get; private set; }

    protected internal IDynamicFormFieldProvider FieldProvider { get; private set; } = default!;

    [CascadingParameter]
    private EditContext CurrentEditContext { get; set; } = default!;

    [Parameter]
    public string? PresentationLayer { get; set; }

    [Parameter]
    public string? EditorClass { get; set; }

    [Parameter]
    public bool ReadOnly { get; set; } = false;

    [Parameter]
    public bool Disabled { get; set; } = false;

    [Parameter]
    public bool SkipReadOnly { get; set; } = true; //TODO: Rebuild field list on change

    [Parameter]
    public bool RequireDisplayAnnotation { get; set; } = true; //TODO: Rebuild field list on change

    [Parameter]
    public EventCallback<object> OnModelChanged { get; set; }

    [Parameter]
    public RenderFragment<DynamicFormField>? FieldTemplate { get; set; }   

    protected override void OnParametersSet()
    {
        if (Fields != null)
        {
            foreach (var field in Fields)
            {
                field.ValueChanged -= OnValueChanged;
            }
        }

        if (CurrentEditContext == null)
        {
            throw new InvalidOperationException($"{this.GetType()} requires a cascading parameter " +
                $"of type {nameof(EditContext)}. For example, you can use {this.GetType()} inside " +
                $"an {nameof(EditForm)}.");
        }

        if (CurrentEditContext.Model != null)
        {
            Fields = DynamicFormField.Create(this);

            foreach (var field in Fields)
            {
                field.ValueChanged += OnValueChanged;
            }
        }
        else
        {
            Fields = null;
        }

        FieldProvider = (IDynamicFormFieldProvider?)ScopedServices.GetService(typeof(IDynamicFormFieldProvider)) ?? new DefaultFormFieldProvider();
    }

    private void OnValueChanged(object? sender, EventArgs e)
    {
        InvokeAsync(() => OnModelChanged.InvokeAsync(CurrentEditContext.Model));
    }
}