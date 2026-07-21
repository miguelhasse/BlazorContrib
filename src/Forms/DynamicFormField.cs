using Microsoft.AspNetCore.Components;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;

namespace Hasseware.AspNetCore.Components.Forms;

public sealed class DynamicFormField
{
    private static readonly MethodInfo EventCallbackFactoryCreate = GetEventCallbackFactoryCreate();

    // Per-property reflection results (display metadata, UIHint attributes, etc.) never change for a
    // given PropertyInfo, so they are computed once and reused across every form/model instance.
    private static readonly ConcurrentDictionary<PropertyInfo, FieldMetadata> _metadataCache = new();

    // Qualifying property list only depends on the model type plus the two form flags below,
    // so it is cached instead of being re-computed via reflection on every field-list rebuild.
    private static readonly ConcurrentDictionary<(Type ModelType, bool RequireDisplayAnnotation, bool SkipReadOnly), PropertyInfo[]> _propertyCache = new();

    private RenderFragment? _editorTemplate;
    private RenderFragment? _fieldValidationTemplate;
    private LambdaExpression? _valueExpression;
    private object? _valueChangedHandler;

    private readonly UIHintAttribute? _uiHintAttr;

    public event EventHandler? ValueChanged;

    private DynamicFormField(DynamicFormFields form, PropertyInfo propertyInfo)
    {
        Owner = form;
        Property = propertyInfo;
        EditorId = Owner.BaseEditorId + '_' + Property.Name;

        var metadata = _metadataCache.GetOrAdd(propertyInfo, static prop => FieldMetadata.Create(prop));

        ReadOnly = form.ReadOnly || metadata.ReadOnly;
        DisplayName = metadata.DisplayName;
        Description = metadata.Description;
        Placeholder = metadata.Placeholder;
        GroupName = metadata.GroupName;
        Order = metadata.Order;

        _uiHintAttr = metadata.UIHints.Length == 0 ? null : metadata.UIHints
            .Where(attr => attr.PresentationLayer == null || attr.PresentationLayer == form.PresentationLayer)
            .OrderByDescending(attr => attr.PresentationLayer)
            .FirstOrDefault();
    }

    private DynamicFormFields Owner { get; }

    /// <summary>
    /// Gets the <see cref="PropertyInfo"/> for the model property represented by this field.
    /// </summary>
    public PropertyInfo Property { get; }

    /// <summary>
    /// Gets the type of the model property represented by this field.
    /// </summary>
    public Type PropertyType => Property.PropertyType;

    /// <summary>
    /// Gets the presentation layer for which this field was created. This is used to select the appropriate <see cref="UIHintAttribute"/> when multiple are defined for a property.
    /// </summary>
    public string? PresentationLayer => Owner.PresentationLayer;

    /// <summary>
    /// Gets a value indicating whether this field is read-only. A field is considered read-only if the form is read-only or if the underlying property
    /// does not have a setter or has an <see cref="EditableAttribute"/> with <see cref="EditableAttribute.AllowEdit"/> set to false.
    /// </summary>
    public bool ReadOnly { get; }

    /// <summary>
    /// Gets a value indicating whether this field is disabled. A field is considered disabled if the form is disabled.
    /// </summary>
    public bool Disabled => Owner.Disabled;

    /// <summary>
    /// Gets the <see cref="UIHintAttribute.UIHint"/> value for this field, if any. This is used to select the appropriate editor component for the field.
    /// </summary>
    public string? UIHint => _uiHintAttr?.UIHint;

    /// <summary>
    /// Gets the control parameters for this field, if any. These parameters are used to configure the editor component.
    /// </summary>
    public IDictionary<string, object?> ControlParameters => _uiHintAttr?.ControlParameters ?? new Dictionary<string, object?>(0);

    /// <summary>
    /// Gets or sets the value of the model property represented by this field. Setting the value will update the underlying model property
    /// and raise the <see cref="ValueChanged"/> event if the value has changed.
    /// </summary>
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

    /// <summary>
    /// Gets the unique ID for the editor component associated with this field. This ID is used to link the editor with its corresponding label and validation message.
    /// </summary>
    public string EditorId { get; }

    /// <summary>
    /// Gets the display name for this field. This is used as the label text for the editor component.
    /// </summary>
    public string DisplayName { get; }

    /// <summary>
    /// Gets the order for this field. This is used to determine the order in which fields are rendered in the form.
    /// </summary>
    public int Order { get; }

    /// <summary>
    /// Gets the description for this field. This is used as the help text for the editor component.
    /// </summary>
    public string? Description { get; }

    /// <summary>
    /// Gets the placeholder text for this field. This is used as the placeholder attribute for the editor component.
    /// </summary>
    public string? Placeholder { get; }

    /// <summary>
    /// Gets the group name for this field. This is used to group fields together in the form.
    /// </summary>
    public string? GroupName { get; }

    /// <summary>
    /// Gets the <see cref="RenderFragment"/> for the editor component associated with this field.
    /// This fragment is used to render the editor component in the form.
    /// </summary>
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

    /// <summary>
    /// Gets the <see cref="RenderFragment"/> for the validation message component associated with this field.
    /// This fragment is used to render the validation message component in the form.
    /// </summary>
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
        if (_valueExpression != null)
            return _valueExpression;

        // Expression<Func<T>>: () => Owner.Model.Property
        var access = Expression.Property(Expression.Constant(Owner.Model, Owner.Model.GetType()), Property);
        return _valueExpression = Expression.Lambda(typeof(Func<>).MakeGenericType(PropertyType), access);
    }

    private object? CreateValueChangedHandler()
    {
        if (_valueChangedHandler != null)
            return _valueChangedHandler;

        // Expression<Action<T>>: value => this.Value = (object)value;
        var changeHandlerParameter = Expression.Parameter(PropertyType);
        var body = Expression.Assign(Expression.Property(Expression.Constant(this), nameof(Value)), Expression.Convert(changeHandlerParameter, typeof(object)));
        var changeHandlerLambda = Expression.Lambda(typeof(Action<>).MakeGenericType(PropertyType), body, changeHandlerParameter);

        // Create the handler from the expression using EventCallbackFactory.Create<T>(object receiver, Action<T> callback)
        var method = EventCallbackFactoryCreate.MakeGenericMethod(PropertyType);
        return _valueChangedHandler = method.Invoke(EventCallback.Factory, [this, changeHandlerLambda.Compile()]);
    }

    internal static IEnumerable<DynamicFormField> Create(DynamicFormFields form)
    {
        var cacheKey = (ModelType: form.Model.GetType(), form.RequireDisplayAnnotation, form.SkipReadOnly);

        var properties = _propertyCache.GetOrAdd(cacheKey, static key =>
            key.ModelType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy)
                .Where(property =>
                {
                    var type = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
                    if (!type.IsValueType && type != typeof(string) && type != typeof(Uri))
                        return false;

                    // Skip properties without display annotation
                    if (key.RequireDisplayAnnotation && property.GetCustomAttribute<DisplayAttribute>() is null)
                        return false;

                    // Skip readonly properties
                    if (key.SkipReadOnly && property.SetMethod == null)
                        return false;

                    return true;
                })
                .ToArray());

        foreach (var property in properties)
            yield return new DynamicFormField(form, property);
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

    private sealed class FieldMetadata
    {
        public required bool ReadOnly { get; init; }

        public required string DisplayName { get; init; }

        public required string? Description { get; init; }

        public required string? Placeholder { get; init; }

        public required string? GroupName { get; init; }

        public required int Order { get; init; }

        public required UIHintAttribute[] UIHints { get; init; }

        public static FieldMetadata Create(PropertyInfo propertyInfo)
        {
            string? displayName = null, description = null, placeholder = null, groupName = null;
            int order = 10_000;

            if (propertyInfo.GetCustomAttribute<DisplayAttribute>() is { } displayAttribute)
            {
                displayName = displayAttribute.GetName();
                description = displayAttribute.GetDescription();
                placeholder = displayAttribute.GetPrompt();
                groupName = displayAttribute.GetGroupName();
                order = displayAttribute.GetOrder() ?? 10_000;
            }

            displayName ??= propertyInfo.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName ?? propertyInfo.Name;
            description ??= propertyInfo.GetCustomAttribute<DescriptionAttribute>()?.Description;

            bool readOnly = propertyInfo.SetMethod == null
                || (propertyInfo.GetCustomAttribute<EditableAttribute>() is { } editor && !editor.AllowEdit);

            var uiHints = propertyInfo.GetCustomAttributes<UIHintAttribute>().ToArray();

            return new FieldMetadata
            {
                ReadOnly = readOnly,
                DisplayName = displayName,
                Description = description,
                Placeholder = placeholder,
                GroupName = groupName,
                Order = order,
                UIHints = uiHints,
            };
        }
    }
}