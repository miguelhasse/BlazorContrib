using System.Collections;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace Hasseware.AspNetCore.Components.Forms;

/// <summary>
/// Adds custom Data Annotations validation support to an <see cref="EditContext"/>.
/// </summary>
public class RecursiveAnnotationsValidator : ComponentBase
{
    private static readonly ConcurrentDictionary<(Type ModelType, string FieldName), PropertyInfo> _propertyInfoCache = new();

    [CascadingParameter]
    public EditContext CurrentEditContext { get; set; } = default!;

    [Inject]
    private IServiceProvider ServiceProvider { get; set; } = default!;

    /// <inheritdoc/>
    protected override void OnInitialized()
    {
        if (this.CurrentEditContext == null)
        {
            throw new InvalidOperationException($"{nameof(RecursiveAnnotationsValidator)} requires a cascading " +
                $"parameter of type {nameof(EditContext)}. For example, you can use {nameof(RecursiveAnnotationsValidator)} " +
                $"inside an EditForm.");
        }

        var editContext = this.CurrentEditContext;
        var messages = new ValidationMessageStore(editContext);

        // Perform object-level validation on request
        editContext.OnValidationRequested += (sender, eventArgs) => ValidateModel((EditContext)sender!, messages, this.ServiceProvider);

        // Perform per-field validation on each field edit
        editContext.OnFieldChanged += (sender, eventArgs) => ValidateField(editContext, messages, this.ServiceProvider, eventArgs.FieldIdentifier);
    }

    private static void ValidateModel(EditContext editContext, ValidationMessageStore messages, IServiceProvider serviceProvider)
    {
        var validationResults = new List<ValidationResult>();
        messages.Clear();

        if (TryValidateObjectRecursive(editContext.Model, serviceProvider, validationResults) == false)
        {
            // Transfer results to the ValidationMessageStore
            foreach (var validationResult in validationResults)
            {
                if (!validationResult.MemberNames.Any())
                {
                    messages.Add(new FieldIdentifier(editContext.Model, string.Empty), validationResult.ErrorMessage!);
                    continue;
                }

                foreach (var memberName in validationResult.MemberNames)
                {
                    messages.Add(editContext.Field(memberName), validationResult.ErrorMessage!);
                }
            }
        }

        editContext.NotifyValidationStateChanged();
    }

    private static void ValidateField(EditContext editContext, ValidationMessageStore messages, IServiceProvider serviceProvider, in FieldIdentifier fieldIdentifier)
    {
        if (TryGetValidatableProperty(fieldIdentifier, out var propertyInfo))
        {
            var propertyValue = propertyInfo.GetValue(fieldIdentifier.Model);

            var validationContext = new ValidationContext(fieldIdentifier.Model, serviceProvider, null)
            {
                MemberName = propertyInfo.Name,
            };

            var results = new List<ValidationResult>();
            messages.Clear(fieldIdentifier);

            if (Validator.TryValidateProperty(propertyValue, validationContext, results) == false)
            {
                messages.Add(fieldIdentifier, results.Select(result => result.ErrorMessage!));
            }

            // We have to notify even if there were no messages before and are still no messages now,
            // because the "state" that changed might be the completion of some async validation task
            editContext.NotifyValidationStateChanged();
        }
    }

    private static bool TryValidateObjectRecursive(object instance, IServiceProvider serviceProvider, List<ValidationResult> results)
    {
        var validationContext = new ValidationContext(instance, serviceProvider, null);
        bool result = Validator.TryValidateObject(instance, validationContext, results, true);

        var properties = instance.GetType().GetProperties()
            .Where(prop => prop.CanRead && prop.GetIndexParameters().Length == 0)
            .ToList();

        foreach (var property in properties)
        {
            object? value;

            if (property.PropertyType == typeof(string)
                || property.PropertyType.IsValueType
                || (value = property.GetValue(instance, null)) == null)
            {
                continue;
            }

            var asEnumerable = value is IEnumerable enumerable ? enumerable : new[] { value };

            foreach (var enumObj in asEnumerable)
            {
                var nestedResults = new List<ValidationResult>();

                if (TryValidateObjectRecursive(enumObj, serviceProvider, nestedResults) == false)
                {
                    result = false;
                    foreach (var validationResult in nestedResults)
                    {
                        var memberNames = validationResult.MemberNames.Select(name => string.Join('.', property.Name, name));
                        results.Add(new ValidationResult(validationResult.ErrorMessage, memberNames));
                    }
                }
            }
        }

        return result;
    }

    private static bool TryGetValidatableProperty(in FieldIdentifier fieldIdentifier, out PropertyInfo propertyInfo)
    {
        var cacheKey = (ModelType: fieldIdentifier.Model.GetType(), fieldIdentifier.FieldName);
        if (!_propertyInfoCache.TryGetValue(cacheKey, out propertyInfo!))
        {
            // DataAnnotations only validates public properties, so that's all we'll look for
            // If we can't find it, cache 'null' so we don't have to try again next time
            propertyInfo = cacheKey.ModelType.GetProperty(cacheKey.FieldName)!;

            // No need to lock, because it doesn't matter if we write the same value twice
            _propertyInfoCache[cacheKey] = propertyInfo;
        }

        return propertyInfo != null;
    }
}
