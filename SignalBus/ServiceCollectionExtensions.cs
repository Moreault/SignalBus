using Microsoft.Extensions.DependencyInjection;

namespace ToolBX.SignalBus;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSignalBus(this IServiceCollection services)
    {
        services.AddScoped<ISignalBus, SignalBus>();
        return services;
    }
}