using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Bunit;
using Xunit;

namespace Hasseware.AspNetCore.Components.Forms.Tests;

public class InputEnumSelectTests : Bunit.BunitContext
{
    private class Model
    {
        public Color Favorite { get; set; } = Color.Red;

        public Color? MaybeFavorite { get; set; }
    }

    private static RenderFragment<EditContext> InputEnumSelectFor<TEnum>(TEnum value, Action<TEnum> setValue, System.Linq.Expressions.Expression<Func<TEnum>> valueExpression) => _ => builder =>
    {
        builder.OpenComponent<InputEnumSelect<TEnum>>(0);
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

        Assert.Equal(3, cut.FindAll("option").Count);
    }

    [Fact]
    public void OptionText_UsesDisplayAttribute_WhenPresent()
    {
        var model = new Model();

        var cut = Render(EditFormTestHost.Render(model,
            InputEnumSelectFor(model.Favorite, v => model.Favorite = v, () => model.Favorite)));

        var options = cut.FindAll("option");

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

        cut.Find("select").Change("Green");

        Assert.Equal(Color.Green, model.Favorite);
    }

    [Fact]
    public void NullableEnum_EmptySelection_MapsToNull()
    {
        var model = new Model { MaybeFavorite = Color.Green };

        var cut = Render(EditFormTestHost.Render(model,
            InputEnumSelectFor(model.MaybeFavorite, v => model.MaybeFavorite = v, () => model.MaybeFavorite)));

        cut.Find("select").Change(string.Empty);

        Assert.Null(model.MaybeFavorite);
    }
}
