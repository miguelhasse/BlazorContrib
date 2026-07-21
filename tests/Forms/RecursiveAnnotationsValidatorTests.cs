using Microsoft.AspNetCore.Components.Forms;
using Xunit;

namespace Hasseware.AspNetCore.Components.Forms.Tests;

public class RecursiveAnnotationsValidatorTests : Bunit.BunitContext
{
    [Fact]
    public void Throws_WhenNoCascadingEditContext()
    {
        var ex = Assert.Throws<InvalidOperationException>(() => Render<RecursiveAnnotationsValidator>());
        Assert.Contains(nameof(RecursiveAnnotationsValidator), ex.Message);
    }

    [Fact]
    public void Validate_SurfacesRequiredErrorsOnRootModel()
    {
        var model = new ParentModel();
        var editContext = new EditContext(model);

        Render<RecursiveAnnotationsValidator>(parameters => parameters.AddCascadingValue(editContext));

        var isValid = editContext.Validate();

        Assert.False(isValid);
        Assert.Contains(editContext.GetValidationMessages(), m => m.Contains("Title"));
    }

    [Fact]
    public void Validate_RecursesIntoNestedObject_AndSurfacesDottedMemberName()
    {
        var model = new ParentModel { Title = "ok", Child = new ChildModel() };
        var editContext = new EditContext(model);

        Render<RecursiveAnnotationsValidator>(parameters => parameters.AddCascadingValue(editContext));

        var isValid = editContext.Validate();

        Assert.False(isValid);
        Assert.Contains(editContext.GetValidationMessages(editContext.Field("Child.Name")), _ => true);
    }

    [Fact]
    public void Validate_RecursesIntoCollectionItems()
    {
        var model = new ParentModel { Title = "ok", Items = { new ChildModel() } };
        var editContext = new EditContext(model);

        Render<RecursiveAnnotationsValidator>(parameters => parameters.AddCascadingValue(editContext));

        var isValid = editContext.Validate();

        Assert.False(isValid);
        Assert.Contains(editContext.GetValidationMessages(editContext.Field("Items.Name")), _ => true);
    }

    [Fact]
    public void Validate_PassesWhenModelIsValid()
    {
        var model = new ParentModel { Title = "ok", Child = new ChildModel { Name = "child" }, Items = { new ChildModel { Name = "item" } } };
        var editContext = new EditContext(model);

        Render<RecursiveAnnotationsValidator>(parameters => parameters.AddCascadingValue(editContext));

        Assert.True(editContext.Validate());
    }

    [Fact]
    public void FieldChanged_ValidatesOnlyThatField()
    {
        var model = new ParentModel { Title = null };
        var editContext = new EditContext(model);

        Render<RecursiveAnnotationsValidator>(parameters => parameters.AddCascadingValue(editContext));

        var field = editContext.Field(nameof(ParentModel.Title));
        editContext.NotifyFieldChanged(field);

        Assert.NotEmpty(editContext.GetValidationMessages(field));
    }

    [Fact]
    public void Dispose_UnsubscribesHandlers_SoNoFurtherMessagesAreAdded()
    {
        var model = new ParentModel { Title = null };
        var editContext = new EditContext(model);

        var cut = Render<RecursiveAnnotationsValidator>(parameters => parameters.AddCascadingValue(editContext));

        cut.Instance.Dispose();

        var isValid = editContext.Validate();

        // With the handler detached, the validator no longer participates, so no validation
        // messages are added to the EditContext (isValid remains "true" from its perspective).
        Assert.True(isValid);
        Assert.Empty(editContext.GetValidationMessages());
    }

    [Fact]
    public void Dispose_CanBeCalledTwice_WithoutThrowing()
    {
        var editContext = new EditContext(new ParentModel());

        var cut = Render<RecursiveAnnotationsValidator>(parameters => parameters.AddCascadingValue(editContext));

        cut.Instance.Dispose();
        cut.Instance.Dispose();
    }
}
