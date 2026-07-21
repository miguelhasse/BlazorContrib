using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FluentUI.AspNetCore.Components;
using Bunit;
using Xunit;

namespace Hasseware.FluentUI.AspNetCore.Components.Forms.Tests;

public class FluentEnumSelectTests : Bunit.BunitContext
{
    private class Model
    {
        public Color Favorite { get; set; } = Color.Red;
    }

    public FluentEnumSelectTests()
    {
        Services.AddFluentUIComponents();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    private static RenderFragment<EditContext> InputEnumSelectFor(Color value, Action<Color> setValue, System.Linq.Expressions.Expression<Func<Color>> valueExpression) => _ => builder =>
    {
        builder.OpenComponent<FluentEnumSelect<Color>>(0);
        builder.AddAttribute(1, "Value", value);
        builder.AddAttribute(2, "ValueChanged", EventCallback.Factory.Create(new object(), setValue));
        builder.AddAttribute(3, "ValueExpression", valueExpression);
        builder.CloseComponent();
    };

    [Fact]
    public void RendersOneOptionPerEnumValue()
    {
        var model = new Model();

        var cut = Render(EditFormTestHost.Render(model,
            InputEnumSelectFor(model.Favorite, v => model.Favorite = v, () => model.Favorite)));

        Assert.Equal(3, cut.FindAll("fluent-option").Count);
    }

    [Fact]
    public void OptionText_UsesDisplayAttribute_WhenPresent()
    {
        var model = new Model();

        var cut = Render(EditFormTestHost.Render(model,
            InputEnumSelectFor(model.Favorite, v => model.Favorite = v, () => model.Favorite)));

        var options = cut.FindAll("fluent-option");

        Assert.Contains(options, o => o.TextContent == "Deep Blue");
        Assert.Contains(options, o => o.TextContent == "Red");
        Assert.DoesNotContain(options, o => o.TextContent == "Blue");
    }

    [Fact]
    public void ChangingSelect_UpdatesBoundValue()
    {
        var model = new Model();

        var cut = Render(EditFormTestHost.Render(model,
            InputEnumSelectFor(model.Favorite, v => model.Favorite = v, () => model.Favorite)));

        cut.Find("fluent-select").Change("Green");

        Assert.Equal(Color.Green, model.Favorite);
    }
}
