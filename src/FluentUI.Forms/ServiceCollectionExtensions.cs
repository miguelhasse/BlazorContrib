using Hasseware.AspNetCore.Components.Forms;
using Hasseware.FluentUI.AspNetCore.Components.Forms;

namespace Microsoft.Extensions.DependencyInjection;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFluentFormFieldProvider(this IServiceCollection services)
    {
        services.AddSingleton<IDynamicFormFieldProvider, FluentFormFieldProvider>();
        return services;
    }
}
