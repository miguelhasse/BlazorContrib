using Microsoft.AspNetCore.Components.Forms;
using Xunit;

namespace Hasseware.AspNetCore.Components.Forms.Tests;

public class DefaultFormFieldProviderTests : Bunit.BunitContext
{
    private static readonly DefaultFormFieldProvider Provider = new();

    private List<DynamicFormField> CaptureFields(object model)
    {
        var captured = new List<DynamicFormField>();

        Render(EditFormTestHost.Render(model, EditFormTestHost.DynamicFormFields(
            requireDisplayAnnotation: false,
            fieldTemplate: field => builder => captured.Add(field))));

        return captured;
    }

    private DynamicFormField GetField(object model, string propertyName)
    {
        var field = CaptureFields(model).Single(f => f.Property.Name == propertyName);
        return field;
    }

    [Fact]
    public void Bool_MapsToInputCheckbox()
    {
        var (componentType, _) = Provider.GetEditorType(GetField(new InferenceModel(), nameof(InferenceModel.IsActive)));
        Assert.Equal(typeof(InputCheckbox), componentType);
    }

    [Fact]
    public void PlainString_MapsToInputText()
    {
        var (componentType, attrs) = Provider.GetEditorType(GetField(new InferenceModel(), nameof(InferenceModel.PlainText)));
        Assert.Equal(typeof(InputText), componentType);
        Assert.Null(attrs);
    }

    [Theory]
    [InlineData(nameof(InferenceModel.DateText), "date")]
    [InlineData(nameof(InferenceModel.DateTimeText), "datetime-local")]
    [InlineData(nameof(InferenceModel.Email), "email")]
    [InlineData(nameof(InferenceModel.Password), "password")]
    [InlineData(nameof(InferenceModel.Phone), "tel")]
    [InlineData(nameof(InferenceModel.TimeText), "time")]
    [InlineData(nameof(InferenceModel.Website), "url")]
    public void StringWithDataType_MapsToInputTextWithTypeAttribute(string propertyName, string expectedType)
    {
        var (componentType, attrs) = Provider.GetEditorType(GetField(new InferenceModel(), propertyName));

        Assert.Equal(typeof(InputText), componentType);
        Assert.NotNull(attrs);
        Assert.Equal(expectedType, attrs!["type"]);
    }

    [Fact]
    public void MultilineText_MapsToInputTextArea()
    {
        var (componentType, _) = Provider.GetEditorType(GetField(new InferenceModel(), nameof(InferenceModel.Notes)));
        Assert.Equal(typeof(InputTextArea), componentType);
    }

    [Theory]
    [InlineData(nameof(InferenceModel.Count), typeof(int))]
    [InlineData(nameof(InferenceModel.NullableCount), typeof(int?))]
    [InlineData(nameof(InferenceModel.Ratio), typeof(double))]
    [InlineData(nameof(InferenceModel.Amount), typeof(decimal))]
    public void NumericTypes_MapToGenericInputNumber(string propertyName, Type expectedValueType)
    {
        var (componentType, _) = Provider.GetEditorType(GetField(new InferenceModel(), propertyName));

        Assert.True(componentType.IsGenericType);
        Assert.Equal(typeof(InputNumber<>), componentType.GetGenericTypeDefinition());
        Assert.Equal(expectedValueType, componentType.GetGenericArguments()[0]);
    }

    [Fact]
    public void DateTime_WithoutDateOnlyAnnotation_MapsToInputDateTime()
    {
        var (componentType, _) = Provider.GetEditorType(GetField(new InferenceModel(), nameof(InferenceModel.When)));

        Assert.Equal(typeof(InputDateTime<>).MakeGenericType(typeof(DateTime)), componentType);
    }

    [Fact]
    public void DateTime_WithDateOnlyAnnotation_MapsToInputDate()
    {
        var (componentType, _) = Provider.GetEditorType(GetField(new InferenceModel(), nameof(InferenceModel.WhenDateOnly)));

        Assert.Equal(typeof(InputDate<>).MakeGenericType(typeof(DateTime)), componentType);
    }

    [Fact]
    public void DateTimeOffset_MapsToInputDateTime()
    {
        var (componentType, _) = Provider.GetEditorType(GetField(new InferenceModel(), nameof(InferenceModel.WhenOffset)));

        Assert.Equal(typeof(InputDateTime<>).MakeGenericType(typeof(DateTimeOffset)), componentType);
    }

    [Fact]
    public void Guid_MapsToInputUrlOrGuid()
    {
        var (componentType, _) = Provider.GetEditorType(GetField(new InferenceModel(), nameof(InferenceModel.Identifier)));

        Assert.Equal(typeof(InputUrlOrGuid<>).MakeGenericType(typeof(Guid)), componentType);
    }

    [Fact]
    public void Uri_MapsToInputUrlOrGuid()
    {
        var (componentType, _) = Provider.GetEditorType(GetField(new InferenceModel(), nameof(InferenceModel.Homepage)));

        Assert.Equal(typeof(InputUrlOrGuid<Uri>), componentType);
    }

    [Fact]
    public void Enum_MapsToInputEnumSelect()
    {
        var (componentType, _) = Provider.GetEditorType(GetField(new InferenceModel(), nameof(InferenceModel.Favorite)));

        Assert.Equal(typeof(InputEnumSelect<>).MakeGenericType(typeof(Color)), componentType);
    }

    [Fact]
    public void FlagsEnum_FallsBackToInputText()
    {
        var (componentType, _) = Provider.GetEditorType(GetField(new InferenceModel(), nameof(InferenceModel.Access)));

        Assert.Equal(typeof(InputText), componentType);
    }

    [Fact]
    public void ExplicitEditorAttribute_IsRespected()
    {
        var (componentType, attrs) = Provider.GetEditorType(GetField(new InferenceModel(), nameof(InferenceModel.CustomEdited)));

        Assert.Equal(typeof(CustomEditorStub), componentType);
        Assert.Null(attrs);
    }

    [Fact]
    public void GetValidationType_ReturnsGenericValidationMessage()
    {
        var (componentType, attrs) = Provider.GetValidationType(GetField(new InferenceModel(), nameof(InferenceModel.PlainText)));

        Assert.Equal(typeof(ValidationMessage<>).MakeGenericType(typeof(string)), componentType);
        Assert.Null(attrs);
    }
}
