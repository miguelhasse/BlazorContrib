using Hasseware.AspNetCore.Components.Forms;

namespace Microsoft.Extensions.DependencyInjection;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFormFieldProvider(this IServiceCollection services)
    {
        services.AddSingleton<IDynamicFormFieldProvider, DefaultFormFieldProvider>();
        return services;
    }
}
