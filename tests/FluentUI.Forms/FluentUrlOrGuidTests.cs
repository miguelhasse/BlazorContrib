using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FluentUI.AspNetCore.Components;
using Bunit;
using Xunit;

namespace Hasseware.FluentUI.AspNetCore.Components.Forms.Tests;

public class FluentUrlOrGuidTests : Bunit.BunitContext
{
    private class Model
    {
        public Guid Identifier { get; set; } = Guid.NewGuid();

        public Uri? Homepage { get; set; } = new Uri("https://example.com");
    }

    public FluentUrlOrGuidTests()
    {
        Services.AddFluentUIComponents();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    private static RenderFragment<EditContext> FluentUrlOrGuidFor<TValue>(TValue value, Action<TValue> setValue, System.Linq.Expressions.Expression<Func<TValue>> valueExpression) => _ => builder =>
    {
        builder.OpenComponent<FluentUrlOrGuid<TValue>>(0);
        builder.AddAttribute(1, "Value", value);
        builder.AddAttribute(2, "ValueChanged", EventCallback.Factory.Create(new object(), setValue));
        builder.AddAttribute(3, "ValueExpression", valueExpression);
        builder.CloseComponent();
    };

    [Fact]
    public void Guid_RendersTextFieldWithFormattedValue()
    {
        var model = new Model();

        var cut = Render(EditFormTestHost.Render(model,
            FluentUrlOrGuidFor(model.Identifier, v => model.Identifier = v, () => model.Identifier)));

        Assert.Contains(model.Identifier.ToString(), cut.Markup);
    }

    [Fact]
    public void Uri_RendersTextFieldWithFormattedValue()
    {
        var model = new Model();

        var cut = Render(EditFormTestHost.Render(model,
            FluentUrlOrGuidFor(model.Homepage!, v => model.Homepage = v, () => model.Homepage!)));

        Assert.Contains("https://example.com", cut.Markup);
    }

    [Fact]
    public void ChangingValue_UpdatesBoundGuid()
    {
        var model = new Model();
        var newGuid = Guid.NewGuid();

        var cut = Render(EditFormTestHost.Render(model,
            FluentUrlOrGuidFor(model.Identifier, v => model.Identifier = v, () => model.Identifier)));

        cut.Find("fluent-text-field").Change(newGuid.ToString());

        Assert.Equal(newGuid, model.Identifier);
    }

    [Fact]
    public void UnsupportedGenericType_ThrowsOnConstruction()
    {
        Assert.Throws<InvalidOperationException>(() => new FluentUrlOrGuid<int>());
    }
}
