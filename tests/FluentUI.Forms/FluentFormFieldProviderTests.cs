using Hasseware.AspNetCore.Components.Forms;
using Microsoft.FluentUI.AspNetCore.Components;
using Xunit;

namespace Hasseware.FluentUI.AspNetCore.Components.Forms.Tests;

public class FluentFormFieldProviderTests : Bunit.BunitContext
{
    private static readonly FluentFormFieldProvider Provider = new();

    private List<DynamicFormField> CaptureFields(object model, bool disabled = false)
    {
        var captured = new List<DynamicFormField>();

        Render(EditFormTestHost.Render(model, EditFormTestHost.DynamicFormFields(
            requireDisplayAnnotation: false,
            disabled: disabled,
            fieldTemplate: field => builder => captured.Add(field))));

        return captured;
    }

    private DynamicFormField GetField(object model, string propertyName, bool disabled = false) =>
        CaptureFields(model, disabled).Single(f => f.Property.Name == propertyName);

    [Fact]
    public void Bool_MapsToFluentCheckbox()
    {
        var (componentType, _) = Provider.GetEditorType(GetField(new InferenceModel(), nameof(InferenceModel.IsActive)));
        Assert.Equal(typeof(FluentCheckbox), componentType);
    }

    [Fact]
    public void PlainString_MapsToFluentTextField()
    {
        var (componentType, _) = Provider.GetEditorType(GetField(new InferenceModel(), nameof(InferenceModel.PlainText)));
        Assert.Equal(typeof(FluentTextField), componentType);
    }

    [Theory]
    [InlineData(nameof(InferenceModel.Email), TextFieldType.Email)]
    [InlineData(nameof(InferenceModel.Password), TextFieldType.Password)]
    [InlineData(nameof(InferenceModel.Phone), TextFieldType.Tel)]
    [InlineData(nameof(InferenceModel.Website), TextFieldType.Url)]
    public void StringWithDataType_MapsToFluentTextFieldWithCorrectType(string propertyName, TextFieldType expectedType)
    {
        var (componentType, attrs) = Provider.GetEditorType(GetField(new InferenceModel(), propertyName));

        Assert.Equal(typeof(FluentTextField), componentType);
        Assert.Equal(expectedType, attrs![nameof(TextFieldType)]);
    }

    [Fact]
    public void MultilineText_MapsToFluentTextArea()
    {
        var (componentType, _) = Provider.GetEditorType(GetField(new InferenceModel(), nameof(InferenceModel.Notes)));
        Assert.Equal(typeof(FluentTextArea), componentType);
    }

    [Fact]
    public void DateOnlyString_MapsToFluentDatePicker()
    {
        var (componentType, _) = Provider.GetEditorType(GetField(new InferenceModel(), nameof(InferenceModel.DateText)));
        Assert.Equal(typeof(FluentDatePicker), componentType);
    }

    [Fact]
    public void TimeString_MapsToFluentTimePicker()
    {
        var (componentType, _) = Provider.GetEditorType(GetField(new InferenceModel(), nameof(InferenceModel.TimeText)));
        Assert.Equal(typeof(FluentTimePicker), componentType);
    }

    [Fact]
    public void DateTimeString_MapsToFluentDateTime()
    {
        var (componentType, _) = Provider.GetEditorType(GetField(new InferenceModel(), nameof(InferenceModel.DateTimeText)));
        Assert.Equal(typeof(FluentDateTime<>).MakeGenericType(typeof(string)), componentType);
    }

    [Fact]
    public void NumericType_MapsToFluentNumberField()
    {
        var (componentType, _) = Provider.GetEditorType(GetField(new InferenceModel(), nameof(InferenceModel.Count)));

        Assert.True(componentType.IsGenericType);
        Assert.Equal(typeof(FluentNumberField<>), componentType.GetGenericTypeDefinition());
        Assert.Equal(typeof(int), componentType.GetGenericArguments()[0]);
    }

    [Fact]
    public void DateTime_MapsToFluentDateTime()
    {
        var (componentType, _) = Provider.GetEditorType(GetField(new InferenceModel(), nameof(InferenceModel.When)));
        Assert.Equal(typeof(FluentDateTime<>).MakeGenericType(typeof(DateTime)), componentType);
    }

    [Fact]
    public void Guid_MapsToFluentUrlOrGuid()
    {
        var (componentType, _) = Provider.GetEditorType(GetField(new InferenceModel(), nameof(InferenceModel.Identifier)));
        Assert.Equal(typeof(FluentUrlOrGuid<>).MakeGenericType(typeof(Guid)), componentType);
    }

    [Fact]
    public void Uri_MapsToFluentUrlOrGuid()
    {
        var (componentType, _) = Provider.GetEditorType(GetField(new InferenceModel(), nameof(InferenceModel.Homepage)));
        Assert.Equal(typeof(FluentUrlOrGuid<Uri>), componentType);
    }

    [Fact]
    public void Enum_MapsToFluentEnumSelect()
    {
        var (componentType, _) = Provider.GetEditorType(GetField(new InferenceModel(), nameof(InferenceModel.Favorite)));
        Assert.Equal(typeof(FluentEnumSelect<>).MakeGenericType(typeof(Color)), componentType);
    }

    [Fact]
    public void FlagsEnum_FallsBackToFluentTextField()
    {
        var (componentType, _) = Provider.GetEditorType(GetField(new InferenceModel(), nameof(InferenceModel.Access)));
        Assert.Equal(typeof(FluentTextField), componentType);
    }

    [Fact]
    public void Required_IsInferred_FromRequiredAttribute()
    {
        var (_, attrs) = Provider.GetEditorType(GetField(new InferenceModel(), nameof(InferenceModel.RequiredText)));
        Assert.True((bool)attrs!["Required"]);
    }

    [Fact]
    public void Required_IsInferred_FromKeyAttribute()
    {
        var (_, attrs) = Provider.GetEditorType(GetField(new InferenceModel(), nameof(InferenceModel.KeyText)));
        Assert.True((bool)attrs!["Required"]);
    }

    [Fact]
    public void Required_IsNotInferred_WhenPlainString()
    {
        var (_, attrs) = Provider.GetEditorType(GetField(new InferenceModel(), nameof(InferenceModel.PlainText)));
        Assert.False(attrs!.ContainsKey("Required"));
    }

    [Fact]
    public void Required_IsNotInferred_WhenFieldIsDisabled()
    {
        var (_, attrs) = Provider.GetEditorType(GetField(new InferenceModel(), nameof(InferenceModel.DisabledRequiredText), disabled: true));

        Assert.True((bool)attrs!["Disabled"]);
        Assert.False(attrs.ContainsKey("Required"));
    }

    [Fact]
    public void StringLength_MapsToMinAndMaxLengthParameters()
    {
        var (_, attrs) = Provider.GetEditorType(GetField(new InferenceModel(), nameof(InferenceModel.Constrained)));

        Assert.Equal(5, attrs!["Minlength"]);
        Assert.Equal(50, attrs["Maxlength"]);
    }

    [Fact]
    public void Range_MapsToMinAndMaxParameters()
    {
        var (_, attrs) = Provider.GetEditorType(GetField(new InferenceModel(), nameof(InferenceModel.RangedCount)));

        Assert.Equal("1", attrs!["Min"]);
        Assert.Equal("10", attrs["Max"]);
    }

    [Fact]
    public void GetValidationType_ReturnsFluentValidationMessage()
    {
        var (componentType, _) = Provider.GetValidationType(GetField(new InferenceModel(), nameof(InferenceModel.PlainText)));
        Assert.Equal(typeof(FluentValidationMessage<>).MakeGenericType(typeof(string)), componentType);
    }
}
