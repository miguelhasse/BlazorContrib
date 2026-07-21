using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FluentUI.AspNetCore.Components;
using Bunit;
using Xunit;

namespace Hasseware.FluentUI.AspNetCore.Components.Forms.Tests;

public class FluentDateTimeTests : Bunit.BunitContext
{
    private class Model
    {
        public DateTime When { get; set; } = new DateTime(2024, 1, 15);
    }

    public FluentDateTimeTests()
    {
        Services.AddFluentUIComponents();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    private static RenderFragment<EditContext> FluentDateTimeFor(DateTime value, Action<DateTime> setValue, System.Linq.Expressions.Expression<Func<DateTime>> valueExpression) => _ => builder =>
    {
        builder.OpenComponent<FluentDateTime<DateTime>>(0);
        builder.AddAttribute(1, "Value", value);
        builder.AddAttribute(2, "ValueChanged", EventCallback.Factory.Create(new object(), setValue));
        builder.AddAttribute(3, "ValueExpression", valueExpression);
        builder.CloseComponent();
    };

    [Fact]
    public void Renders_WithoutThrowing()
    {
        var model = new Model();

        var cut = Render(EditFormTestHost.Render(model,
            FluentDateTimeFor(model.When, v => model.When = v, () => model.When)));

        Assert.Contains("fluent-text-field", cut.Markup);
    }

    [Fact]
    public void UnsupportedGenericType_ThrowsOnConstruction()
    {
        Assert.Throws<InvalidOperationException>(() => new FluentDateTime<int>());
    }

    [Fact]
    public void ValueRoundTrips_ThroughFormattedString()
    {
        var model = new Model();

        var cut = Render(EditFormTestHost.Render(model,
            FluentDateTimeFor(model.When, v => model.When = v, () => model.When)));

        var expected = model.When.ToString(System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern);
        Assert.Contains(expected, cut.Markup);
    }
}
