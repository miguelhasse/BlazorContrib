using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Bunit;
using Xunit;

namespace Hasseware.AspNetCore.Components.Forms.Tests;

public class DynamicFormFieldTests : Bunit.BunitContext
{
    private List<DynamicFormField> CaptureFields(object model, bool requireDisplayAnnotation = true, bool skipReadOnly = true)
    {
        var captured = new List<DynamicFormField>();

        Render(EditFormTestHost.Render(model, EditFormTestHost.DynamicFormFields(
            requireDisplayAnnotation: requireDisplayAnnotation,
            skipReadOnly: skipReadOnly,
            fieldTemplate: field => builder => captured.Add(field))));

        return captured;
    }

    [Fact]
    public void Create_RequiresDisplayAnnotation_ByDefault()
    {
        var fields = CaptureFields(new DisplayModel());

        // Only "Name" and "OnlyWithDisplay" carry a [Display] attribute.
        Assert.Equal(new[] { nameof(DisplayModel.Name), nameof(DisplayModel.OnlyWithDisplay) },
            fields.Select(f => f.Property.Name).OrderBy(n => n));
    }

    [Fact]
    public void Create_WithoutRequiringDisplayAnnotation_IncludesAllQualifyingProperties()
    {
        var fields = CaptureFields(new DisplayModel(), requireDisplayAnnotation: false, skipReadOnly: false);

        var names = fields.Select(f => f.Property.Name).ToHashSet();

        Assert.Contains(nameof(DisplayModel.Name), names);
        Assert.Contains(nameof(DisplayModel.Legacy), names);
        Assert.Contains(nameof(DisplayModel.Raw), names);
        Assert.Contains(nameof(DisplayModel.NotEditable), names);
        Assert.Contains(nameof(DisplayModel.ReadOnlyComputed), names);
    }

    [Fact]
    public void Create_SkipReadOnly_ExcludesPropertiesWithoutSetter()
    {
        var fields = CaptureFields(new DisplayModel(), requireDisplayAnnotation: false, skipReadOnly: true);

        Assert.DoesNotContain(fields, f => f.Property.Name == nameof(DisplayModel.ReadOnlyComputed));
    }

    [Fact]
    public void DisplayName_FallsBackFromDisplayAttributeToDisplayNameToPropertyName()
    {
        var fields = CaptureFields(new DisplayModel(), requireDisplayAnnotation: false, skipReadOnly: false)
            .ToDictionary(f => f.Property.Name);

        Assert.Equal("Full Name", fields[nameof(DisplayModel.Name)].DisplayName);
        Assert.Equal("Legacy Label", fields[nameof(DisplayModel.Legacy)].DisplayName);
        Assert.Equal(nameof(DisplayModel.Raw), fields[nameof(DisplayModel.Raw)].DisplayName);
    }

    [Fact]
    public void DisplayMetadata_DescriptionPlaceholderGroupAndOrder_AreResolvedFromDisplayAttribute()
    {
        var field = CaptureFields(new DisplayModel(), requireDisplayAnnotation: false, skipReadOnly: false)
            .Single(f => f.Property.Name == nameof(DisplayModel.Name));

        Assert.Equal("Enter your name", field.Description);
        Assert.Equal("e.g. Jane Doe", field.Placeholder);
        Assert.Equal("Basics", field.GroupName);
        Assert.Equal(5, field.Order);
    }

    [Fact]
    public void ReadOnly_IsTrue_WhenEditableAttributeDisallowsEdit()
    {
        var field = CaptureFields(new DisplayModel(), requireDisplayAnnotation: false, skipReadOnly: false)
            .Single(f => f.Property.Name == nameof(DisplayModel.NotEditable));

        Assert.True(field.ReadOnly);
    }

    [Fact]
    public void ReadOnly_IsFalse_ForNormalWritableProperty()
    {
        var field = CaptureFields(new DisplayModel(), requireDisplayAnnotation: false, skipReadOnly: false)
            .Single(f => f.Property.Name == nameof(DisplayModel.Raw));

        Assert.False(field.ReadOnly);
    }

    [Fact]
    public void Value_GetSet_RoundTripsAndRaisesValueChangedOnlyOnActualChange()
    {
        var model = new DisplayModel { Raw = "initial" };
        var field = CaptureFields(model, requireDisplayAnnotation: false, skipReadOnly: false)
            .Single(f => f.Property.Name == nameof(DisplayModel.Raw));

        var changeCount = 0;
        field.ValueChanged += (_, _) => changeCount++;

        Assert.Equal("initial", field.Value);

        field.Value = "initial"; // no actual change
        Assert.Equal(0, changeCount);

        field.Value = "updated";
        Assert.Equal(1, changeCount);
        Assert.Equal("updated", model.Raw);
    }

    [Fact]
    public void EditorTemplate_And_FieldValidationTemplate_RenderWithoutThrowing()
    {
        var cut = Render(EditFormTestHost.Render(new DisplayModel { Raw = "hello" }, EditFormTestHost.DynamicFormFields(
            requireDisplayAnnotation: false,
            skipReadOnly: false)));

        Assert.Contains("Raw", cut.Markup);
    }
}
