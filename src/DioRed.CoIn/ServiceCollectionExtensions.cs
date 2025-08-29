using System.Reflection;

using Microsoft.Extensions.DependencyInjection;

namespace DioRed.CoIn;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCoIn(
        this IServiceCollection services,
        params Assembly[] assemblies
    )
    {
        services.AddSingleton<IInvoker, Invoker>();

        if (assemblies is null or { Length: 0 })
        {
            assemblies = [Assembly.GetEntryAssembly()!];
        }

        foreach (var type in assemblies.SelectMany(a => a.GetTypes()))
        {
            foreach (var iface in type.GetInterfaces())
            {
                if (!iface.IsGenericType) continue;

                var def = iface.GetGenericTypeDefinition();
                if (def == typeof(ICommandHandler<>) ||
                    def == typeof(ICommandHandler<,>) ||
                    def == typeof(IAsyncCommandHandler<>) ||
                    def == typeof(IAsyncCommandHandler<,>))
                {
                    services.AddTransient(iface, type);
                }
            }
        }

        return services;
    }
}