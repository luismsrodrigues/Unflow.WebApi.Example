using System.Reflection;
using Unflow.WebApi.Example.Business;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBusiness(
        this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        var assembly = MethodBase.GetCurrentMethod()?.DeclaringType?.Assembly;
        var exportedTypes = assembly.ExportedTypes;

        foreach (var t in exportedTypes.Where(t => t.IsClass && !t.IsAbstract))
        {
            var implementedInterfaces = t.GetInterfaces();

            foreach (var i in implementedInterfaces.Where(e => e.IsInterface))
            {
                if (i == typeof(IService))
                {
                    services.Add(new ServiceDescriptor(t, t, ServiceLifetime.Transient));
                }
            }
        }
        
        return services;
    }
}