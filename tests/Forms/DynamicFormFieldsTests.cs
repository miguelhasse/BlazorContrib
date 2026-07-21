using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Bunit;
using Xunit;

namespace Hasseware.AspNetCore.Components.Forms.Tests;

public class DynamicFormFieldsTests : Bunit.BunitContext
{
    [Fact]
    public void Throws_WhenNoCascadingEditContext()
    {
        Assert.Throws<InvalidOperationException>(() => Render<DynamicFormFields>());
    }

    [Fact]
    public void RendersOneFieldPerQualifyingProperty_InOrder()
    {
        var model = new DisplayModel();

        var cut = Render(EditFormTestHost.Render(model, EditFormTestHost.DynamicFormFields(
            requireDisplayAnnotation: false,
            skipReadOnly: false)));

        var labels = cut.FindAll("label").Select(l => l.TextContent).ToArray();

        Assert.Contains("Full Name", labels);
        Assert.Contains("Legacy Label", labels);
        Assert.Contains(nameof(DisplayModel.Raw), labels);
    }

    [Fact]
    public void OnModelChanged_FiresWhenAFieldValueChanges()
    {
        var model = new DisplayModel { Raw = "initial" };
        var changedModels = new List<object>();

        var cut = Render(EditFormTestHost.Render(model, EditFormTestHost.DynamicFormFields(
            requireDisplayAnnotation: false,
            skipReadOnly: false,
            onModelChanged: EventCallback.Factory.Create<object>(this, m => changedModels.Add(m)))));

        var input = cut.FindAll("input").First(i => i.GetAttribute("id")!.Contains("Raw"));
        input.Change("updated");

        Assert.Equal("updated", model.Raw);
        Assert.Single(changedModels);
        Assert.Same(model, changedModels[0]);
    }

    [Fact]
    public void ReadOnlyParameter_MarksAllFields_ReadOnly()
    {
        var model = new DisplayModel { Raw = "value" };
        var captured = new List<DynamicFormField>();

        Render(EditFormTestHost.Render(model, EditFormTestHost.DynamicFormFields(
            requireDisplayAnnotation: false,
            skipReadOnly: false,
            readOnly: true,
            fieldTemplate: field => builder => captured.Add(field))));

        Assert.NotEmpty(captured);
        Assert.All(captured, f => Assert.True(f.ReadOnly));
    }
}
