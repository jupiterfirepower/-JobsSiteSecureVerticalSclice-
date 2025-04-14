using Microsoft.Extensions.DependencyInjection;

namespace Jobs.Common.Extentions;

public static class ServiceProviderExtension
{
    public static T ResolveWith<T>(this IServiceProvider provider, params object[] parameters) where T : class => 
        ActivatorUtilities.CreateInstance<T>(provider, parameters);
}