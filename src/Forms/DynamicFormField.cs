using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.AspNetCore.Components;

namespace Hasseware.AspNetCore.Components.Forms;

public sealed class DynamicFormField
{
    private static readonly MethodInfo EventCallbackFactoryCreate = GetEventCallbackFactoryCreate();

    private RenderFragment? _editorTemplate;
    private RenderFragment? _fieldValidationTemplate;

    private readonly UIHintAttribute? _uiHintAttr;

    public event EventHandler? ValueChanged;

    private DynamicFormField(DynamicFormFields form, PropertyInfo propertyInfo)
    {
        Owner = form;
        Property = propertyInfo;
        EditorId = Owner.BaseEditorId + '_' + Property.Name;
        ReadOnly = form.ReadOnly || (propertyInfo.SetMethod == null)
            || (propertyInfo.GetCustomAttribute<EditableAttribute>() is { } editor && !editor.AllowEdit);

        if (propertyInfo.GetCustomAttribute<DisplayAttribute>() is { } displayAttribute)
        {
            DisplayName = displayAttribute.GetName()!;
            Description = displayAttribute.GetDescription()!;
            Placeholder = displayAttribute.GetPrompt()!;
            GroupName = displayAttribute.GetGroupName();
            Order = displayAttribute.GetOrder() ?? 10_000;
        }

        DisplayName ??= propertyInfo.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName ?? Property.Name;
        Description ??= propertyInfo.GetCustomAttribute<DescriptionAttribute>()?.Description!;

        _uiHintAttr = propertyInfo.GetCustomAttributes<UIHintAttribute>()
            .Where(attr => attr.PresentationLayer == null || attr.PresentationLayer == form.PresentationLayer)
            .OrderByDescending(attr => attr.PresentationLayer)
            .FirstOrDefault();
    }

    private DynamicFormFields Owner { get; }

    public PropertyInfo Property { get; }

    public Type PropertyType => Property.PropertyType;

    public string? PresentationLayer => Owner.PresentationLayer;

    public bool ReadOnly { get; }

    public bool Disabled => Owner.Disabled;

    public string? UIHint => _uiHintAttr?.UIHint;

    public IDictionary<string, object?> ControlParameters => _uiHintAttr?.ControlParameters ?? new Dictionary<string, object?>(0);

    public object? Value
    {
        get => Property.GetValue(Owner.Model);
        set
        {
            if (Property.SetMethod != null && !Equals(Value, value))
            {
                Property.SetValue(Owner.Model, value);
                ValueChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    public string EditorId { get; }

    public string DisplayName { get; }

    public int Order { get; }

    public string? Description { get; }

    public string? Placeholder { get; }

    public string? GroupName { get; }

    public RenderFragment EditorTemplate => _editorTemplate ??= builder =>
    {
        var (ComponentType, AdditonalAttributes) = Owner.FieldProvider.GetEditorType(this);

        builder.OpenComponent(0, ComponentType);
        builder.AddAttribute(1, "Value", Value);
        builder.AddAttribute(2, "ValueChanged", CreateValueChangedHandler());
        builder.AddAttribute(3, "ValueExpression", CreateValueExpression());
        builder.AddAttribute(4, "id", EditorId);
        builder.AddAttribute(5, "class", Owner.EditorClass);
        builder.AddAttribute(6, "placeholder", Placeholder);
        builder.AddMultipleAttributes(7, AdditonalAttributes);
        builder.CloseComponent();
    };

    public RenderFragment? FieldValidationTemplate => _fieldValidationTemplate ??= builder =>
    {
        var (componentType, additonalAttributes) = Owner.FieldProvider.GetValidationType(this);

        builder.OpenComponent(0, componentType);
        builder.AddAttribute(1, "For", CreateValueExpression());
        builder.AddMultipleAttributes(2, additonalAttributes);
        builder.CloseComponent();
    };

    private LambdaExpression CreateValueExpression()
    {
        // Expression<Func<T>>: () => Owner.Model.Property
        var access = Expression.Property(Expression.Constant(Owner.Model, Owner.Model.GetType()), Property);
        return Expression.Lambda(typeof(Func<>).MakeGenericType(PropertyType), access);
    }

    private object? CreateValueChangedHandler()
    {
        // Expression<Action<T>>: value => this.Value = (object)value;
        var changeHandlerParameter = Expression.Parameter(PropertyType);
        var body = Expression.Assign(Expression.Property(Expression.Constant(this), nameof(Value)), Expression.Convert(changeHandlerParameter, typeof(object)));
        var changeHandlerLambda = Expression.Lambda(typeof(Action<>).MakeGenericType(PropertyType), body, changeHandlerParameter);

        // Create the handler from the expression using EventCallbackFactory.Create<T>(object receiver, Action<T> callback)
        var method = EventCallbackFactoryCreate.MakeGenericMethod(PropertyType);
        return method.Invoke(EventCallback.Factory, [this, changeHandlerLambda.Compile()]);
    }

    internal static IEnumerable<DynamicFormField> Create(DynamicFormFields form)
    {
        var properties = form.Model.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);

        foreach (var property in properties)
        {
            var type = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
            if (!type.IsValueType && type != typeof(string) && type != typeof(Uri))
                continue;

            // Skip properties without display annotation
            if (form.RequireDisplayAnnotation && property.GetCustomAttribute<DisplayAttribute>() is null)
                continue;

            // Skip readonly properties
            if (form.SkipReadOnly && (property.SetMethod == null))
                continue;

            yield return new DynamicFormField(form, property);
        }
    }

    private static MethodInfo GetEventCallbackFactoryCreate() => typeof(EventCallbackFactory).GetMethods().Single(m =>
    {
        if (m.Name != "Create" || !m.IsPublic || m.IsStatic || !m.IsGenericMethod)
            return false;

        if (m.GetGenericArguments().Length != 1)
            return false;

        var args = m.GetParameters();
        return args.Length == 2
            && args[0].ParameterType == typeof(object)
            && args[1].ParameterType.IsGenericType
            && args[1].ParameterType.GetGenericTypeDefinition() == typeof(Action<>);
    });
}